using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using At.Contexts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using cs = Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace At.Targets.CSharp
{
    static class CSharpTarget
{
    public static AtEmitResult Emit(AtCompileResult compileResult,Stream peStream,CancellationToken cancellationToken)
    {
        var csTrees       = ConvertToCSharpSyntaxTrees(compileResult, defaultMap);
        var csCompilation = CSharpCompilation.Create(  
                                compileResult.Compilation.assemblyName,
                                csTrees,
                                references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)},
                                options: null);
        var csResult = csCompilation.Emit(peStream, cancellationToken: cancellationToken);
        return createAtEmitResult(csResult, csTrees,cancellationToken);
    }
    
    public static AtEmitResult Emit(AtCompileResult compileResult, CancellationToken cancellationToken)
    {
        var cSharpTrees = ConvertToCSharpSyntaxTrees(compileResult, defaultMap);
 
        var cSharpCompilation = CSharpCompilation.Create(  compileResult.Compilation.assemblyName
                                                          ,cSharpTrees
                                                          ,references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)}
                                                          ,options: null);

        var result = cSharpCompilation.Emit(compileResult.Compilation.assemblyName+".dll", cancellationToken: cancellationToken);
        return createAtEmitResult(result, cSharpTrees,cancellationToken);
    }


    public static IEnumerable<CSharpSyntaxTree> ConvertToCSharpSyntaxTrees(AtCompileResult compileResult, Func<Symbol,ExpressionSyntax> map)
    {
        foreach(CompilationUnitContext ctx in compileResult.Context.Contents())
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
    
    static ExpressionSyntax defaultMap(Symbol s)
    {
        return   s is KeywordSymbol k && k.Name == "output" ? MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("System"),
                                                                    IdentifierName("Console")),
                                                                IdentifierName("WriteLine"))
               : null;
    }
}
}
