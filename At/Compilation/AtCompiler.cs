using System.Diagnostics;
using System.Threading;
using At.Binding;
using At.Contexts;
using At.Syntax;

namespace At
{
//MethodBodyCompiler
public sealed class AtCompiler : BindingTreeVisitor
{

    private @AtCompiler(AtCompilation compilation,DiagnosticsBag diagnostics,CancellationToken cancellationToken)
    {
        Debug.Assert(compilation != null);
        Debug.Assert(diagnostics != null);
        Context = new CompilerContext(this,compilation,diagnostics,cancellationToken);
    }
    
    /*

    public override void VisitContext(IContextSymbol symbol)
    {
        cancellationToken.ThrowIfCancellationRequested();
        compileContext(symbol);
    } 
    */   

    public CompilerContext Context { get; }

    internal static AtCompileResult Compile(
                                        AtCompilation     compilation,
                                        DiagnosticsBag    diagnostics,
                                        CancellationToken cancellationToken)
    {
        var compiler = new AtCompiler(compilation,diagnostics,cancellationToken);

        foreach(var syntaxTree in compilation.SyntaxTrees)
            compiler.compileContext(compiler.Context.Compilation, syntaxTree.GetRoot());

        foreach(var kv in compiler.Context.Compilation.getUndefinedSymbols())
            compiler.Context.Diagnostics.Add(AtDiagnostic.Create(DiagnosticIds.UndefinedSymbol,((IBindingNode)kv.Value).Syntax,DiagnosticSeverity.Error,string.Format(SR.UndefinedSymbolF,kv.Key.Name)));        

        return new AtCompileResult(compiler.Context.Compilation);
    }

    protected internal override void VisitApply(ApplicationExpression e)
    {
        e.Context.AddNode(e);
    }

    protected internal override void VisitBinary(BinaryOperation binaryOperation)
    {
        binaryOperation.Context.AddNode(binaryOperation);
    }

    protected internal override void VisitDirective(Directive directive)
    {
        directive.Context.AddNode(directive);
    }

    protected internal override void VisitSymbol(Symbol symbol)
    {
       Context.Compilation.AddNode(symbol);
    }

    protected internal override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
       typeDeclaration.Context.AddNode(typeDeclaration);
    }

    protected internal override void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        variableDeclaration.Context.AddNode(variableDeclaration);
    }

    protected internal override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        methodDeclaration.Context.AddNode(methodDeclaration);
    }

    protected internal override void VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
    {
        namespaceDeclaration.Context.AddNode(namespaceDeclaration);
    }

    //CompileNamespace()
    private void compileContext(Context parentContext, ContextSyntax syntaxNode)
    {
        var binder = new Binder(parentContext);
        var ctx = (Context) binder.VisitContext(syntaxNode);
                
        foreach(var item in ctx.Contents())
        { 
            item.Accept(this);            
        }
    }
}
}
