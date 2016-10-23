using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using At.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace At
{
public class AtCompilation
{
    readonly string assemblyName;
    readonly SyntaxAndDeclarationManager _syntaxAndDeclarations;

    AtCompilation(string assemblyName = null,
                  //...
                  SyntaxAndDeclarationManager syntaxAndDeclarations = null)
    {
        this.assemblyName = assemblyName;
        this._syntaxAndDeclarations = syntaxAndDeclarations ?? 
                                      new SyntaxAndDeclarationManager(ImmutableArray<AtSyntaxTree>.Empty);
    }

    public static AtCompilation Create(AtSyntaxTree[] trees)
    {
        var compilation = new AtCompilation();

        if (trees != null)
            compilation = compilation.AddSyntaxTrees(ImmutableArray<AtSyntaxTree>.Empty.AddRange(trees));

        return compilation;
    }

    public AtCompilation AddSyntaxTrees(ImmutableArray<AtSyntaxTree> trees) 
    {
        if (trees == null)
        {
            throw new ArgumentNullException(nameof(trees));
        }    

        if (!trees.Any())
            return this;

         var syntaxAndDeclarations = _syntaxAndDeclarations;
         syntaxAndDeclarations = syntaxAndDeclarations.AddSyntaxTrees(trees);
         return Update(syntaxAndDeclarations);
    }

    private AtCompilation Update(SyntaxAndDeclarationManager syntaxAndDeclarations)
    {
        return new AtCompilation(assemblyName,syntaxAndDeclarations);
    }

    public AtEmitResult Emit(CancellationToken cancellationToken = default(CancellationToken))
    {
        checkForExpressionClusters();
    
        var cSharpTrees = convertToCSharpSyntaxTrees(_syntaxAndDeclarations.syntaxTrees);
 
        var cSharpCompilation = CSharpCompilation.Create(  assemblyName
                                                          ,cSharpTrees
                                                          ,references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)}
                                                          ,options: null);

        var result = cSharpCompilation.Emit(assemblyName+".dll", cancellationToken: cancellationToken);
        return createAtEmitResult(result, cSharpTrees,cancellationToken);
    }

    public AtEmitResult Emit(Stream peStream, CancellationToken cancellationToken = default(CancellationToken)) 
    {
        checkForExpressionClusters();
    
        if (peStream == null)
        {
            throw new ArgumentNullException(nameof(peStream));
        }
 
        if (!peStream.CanWrite)
        {
            throw new ArgumentException("peStream must support write", nameof(peStream));
        }

        var csTrees       = convertToCSharpSyntaxTrees(_syntaxAndDeclarations.syntaxTrees);
        var csCompilation = CSharpCompilation.Create(  
                                assemblyName,
                                csTrees,
                                references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)},
                                options: null);
        var csResult = csCompilation.Emit(peStream, cancellationToken: cancellationToken);
        return createAtEmitResult(csResult, csTrees,cancellationToken);
    }

    void checkForExpressionClusters()
    {
        var cluster = _syntaxAndDeclarations.syntaxTrees.SelectMany(_=>_.GetRoot().DescendantNodes().OfType<ExpressionClusterSyntax>()).FirstOrDefault();
        if (cluster != null)
            throw new CompilationException(new AtEmitResult(false,ImmutableArray<AtDiagnostic>.Empty.Add(AtDiagnostic.Create(DiagnosticIds.ExpressionCluster,cluster,"Cannot compile syntax trees with expression clusters")),new string[0]));
    }

    IEnumerable<CSharpSyntaxTree> convertToCSharpSyntaxTrees(ImmutableArray<AtSyntaxTree> atSyntaxTrees)
    {
       
        foreach(var tree in atSyntaxTrees)
        { 
           var converter = new SyntaxTreeConverter(atSyntaxTree: tree);        
           yield return converter.ConvertToCSharpTree();
        }
    }

    AtEmitResult createAtEmitResult(EmitResult result, IEnumerable<SyntaxTree> syntaxTrees,CancellationToken cancellationToken)
    {
        return new AtEmitResult(  result.Success
                                 ,ImmutableArray<AtDiagnostic>.Empty.AddRange(result.Diagnostics.Select(_=>new MsDiagnostic(_)))
                                 ,syntaxTrees.Select(_=>_.GetRoot().NormalizeWhitespace().ToFullString()));
    }
}
}