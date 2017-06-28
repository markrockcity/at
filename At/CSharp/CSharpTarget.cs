using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using At.Contexts;
using At.Symbols;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace At.Targets.CSharp
{
    static class CSharpTarget
{
    public static AtEmitResult Emit(BindResult compileResult,Stream peStream,CancellationToken cancellationToken)
    {
        var csTrees = ConvertToCSharpSyntaxTrees(compileResult, defaultMap);

        try
        {
            var csCompilation = CSharpCompilation.Create(  
                                    compileResult.Compilation.assemblyName,
                                    csTrees,
                                    references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)},
                                    options: null);
            var csResult = csCompilation.Emit(peStream, cancellationToken: cancellationToken);
            return createAtEmitResult(csResult, csTrees,cancellationToken);
        }
        catch(Exception)
        {
            if (csTrees != null && csTrees.Any())
            {
                Debug.WriteLine(csTrees.First().GetRoot().NormalizeWhitespace().ToFullString());
            }
        
            throw;
        }
    }
    
    public static AtEmitResult Emit(BindResult compileResult, CancellationToken cancellationToken)
    {
        var cSharpTrees = ConvertToCSharpSyntaxTrees(compileResult, defaultMap);
 
        var cSharpCompilation = CSharpCompilation.Create(  compileResult.Compilation.assemblyName
                                                          ,cSharpTrees
                                                          ,references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)}
                                                          ,options: null);

        var result = cSharpCompilation.Emit(compileResult.Compilation.assemblyName+".dll", cancellationToken: cancellationToken);
        return createAtEmitResult(result, cSharpTrees,cancellationToken);
    }


    public static IEnumerable<CSharpSyntaxTree> ConvertToCSharpSyntaxTrees(BindResult compileResult, Func<Symbol,CSharpSyntaxNode> map)
    {
        foreach(CompilationUnit ctx in compileResult.Context.Contents())
        { 
           var converter = new CSharpSyntaxTreeConverter(ctx,map);        
           yield return converter.ConvertToCSharpTree();
        }
    }

    static AtEmitResult createAtEmitResult(EmitResult result, IEnumerable<SyntaxTree> syntaxTrees,CancellationToken cancellationToken)
    {
        return new AtEmitResult(  result.Success
                                 ,ImmutableArray<AtDiagnostic>.Empty.AddRange(result.Diagnostics.Select(_=>new MsDiagnostic(_)))
                                 ,syntaxTrees.Select(_=>_.GetRoot().NormalizeWhitespace().ToFullString()));
    }
    
    static CSharpSyntaxNode defaultMap(Symbol s)
    {
        return   s is KeywordSymbol k && k.Name == "output" 
                    ? (CSharpSyntaxNode) MemberAccessExpression
                                            (SyntaxKind.SimpleMemberAccessExpression,
                                             MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("System"),
                                                IdentifierName("Console")),
                                             IdentifierName("WriteLine"))

               : s==TypeSymbol.Number ? ParseTypeName("decimal")

               : null;
    }
}
}
