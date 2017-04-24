using System.Diagnostics;
using System.Threading;
using At.Binding;
using At.Contexts;
using At.Syntax;

namespace At
{
//MethodBodyCompiler
public sealed class AtCompiler 
{

    private @AtCompiler(AtCompilation compilation,DiagnosticsBag diagnostics,CancellationToken cancellationToken)
    {
        Debug.Assert(compilation != null);
        Debug.Assert(diagnostics != null);
        Context = new CompilerContext(this,compilation,diagnostics,cancellationToken);
    }
    
    public CompilerContext Context { get; }

    public override string ToString() => nameof(AtCompiler);

    internal static BindResult Bind(
                                AtCompilation     compilation,
                                DiagnosticsBag    diagnostics,
                                CancellationToken cancellationToken)
    {
        var compiler = new AtCompiler(compilation,diagnostics,cancellationToken);

        foreach(var syntaxTree in compilation.SyntaxTrees)
            compiler.bindCompilationUnit(compiler.Context.Compilation, syntaxTree.GetRoot());

        foreach(var kv in compiler.Context.Compilation.getUndefinedSymbols())
            compiler.Context.Diagnostics.Add(AtDiagnostic.Create(DiagnosticIds.UndefinedSymbol,((IBindingNode)kv.op).Syntax,DiagnosticSeverity.Error,string.Format(SR.UndefinedSymbolF,kv.symbol.Name)));        

        return new BindResult(compiler.Context.Compilation);
    }

    //CompileNamespace()
    private  CompilationUnit bindCompilationUnit(Context parentContext, CompilationUnitSyntax syntaxNode)
    {
        var binder = new Binder(parentContext);
        var ctx = (CompilationUnit) binder.VisitContext(syntaxNode);
        return ctx;
        
        //whole context is assumed to be bound at this point and ready to be emmited to target
                
        /*
        foreach(var item in ctx.Contents())
        { 
            item.Accept(this);            
        }*/
    }
}
}
