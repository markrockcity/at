using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using At.Contexts;
using At.Symbols;
using At.Syntax;


namespace At
{
public sealed class AtCompilation
{
    internal readonly string assemblyName;

    @AtCompilation(string name, ImmutableArray<AtSyntaxTree> trees)
    {
        assemblyName = name;
        SyntaxTrees = trees;
    }

    public ImmutableArray<AtSyntaxTree> SyntaxTrees { get; }

    public static AtCompilation Create(params AtSyntaxTree[] trees)
    {
        var compilation = new AtCompilation(null,trees.ToImmutableArray());
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

         return update(trees);
    }

    private AtCompilation update(ImmutableArray<AtSyntaxTree> trees)
    {
        return new AtCompilation(assemblyName,SyntaxTrees.Concat(trees).ToImmutableArray());
    }
    
    /// <remarks>(DLL)</remarks>
    public AtEmitResult Emit(CancellationToken cancellationToken = default(CancellationToken))
    {
        checkForExpressionClusters();
        var compileResult = Compile(cancellationToken);
        return compileResult.Success
                    ? Targets.CSharp.CSharpTarget.Emit(compileResult,cancellationToken)
                    : new AtEmitResult(false,compileResult.Diagnostics,null);
    }

    public AtEmitResult Emit(Stream peStream, CancellationToken cancellationToken = default(CancellationToken)) 
    {
        if (peStream == null)
        {
            throw new ArgumentNullException(nameof(peStream));
        } 

        if (!peStream.CanWrite)
        {
            throw new ArgumentException("peStream must support write", nameof(peStream));
        }

        checkForExpressionClusters();
        var result = Compile(cancellationToken);
        return result.Success 
                    ? Targets.CSharp.CSharpTarget.Emit(result,peStream,cancellationToken)
                    : new AtEmitResult(false,result.Diagnostics,null);
    }

    public AtCompileResult Compile(CancellationToken cancellationToken)
    {
        var diagnostics = new DiagnosticsBag();
        return AtCompiler.Compile(this,diagnostics,cancellationToken);
    }

    private void checkForExpressionClusters()
    {
        var cluster = SyntaxTrees.SelectMany(_=>_.GetRoot().DescendantNodes().OfType<ExpressionClusterSyntax>()).FirstOrDefault();
        if (cluster != null)
            throw new CompilationException(new AtEmitResult(false,ImmutableArray<AtDiagnostic>.Empty.Add(AtDiagnostic.Create(DiagnosticIds.ExpressionCluster,cluster,DiagnosticSeverity.Error,"Cannot compile syntax trees with expression clusters")),new string[0]));
    }
}
}