using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;
using At.Syntax;

namespace At.Contexts
{
//ModuleBuilder()?
/// <summary>
/// A Binder converts names in to symbols and syntax nodes into bound trees. It is context
/// dependent, relative to a location in source code.
/// </summary>
class Binder : AtSyntaxVisitor<IBindingNode>
{
    private Context ctx;

    public Binder(Context ctx)
    {
        this.ctx = ctx;
    }

    protected internal override IBindingNode VisitCompilationUnit(CompilationUnitSyntax compilationUnitSyntax)
    {
        return  new CompilationUnitContext((CompilationContext) ctx, compilationUnitSyntax, ctx.Diagnostics);
    }

    protected internal override IBindingNode VisitApply(ApplicationSyntax applicationSyntax)
    {
        var subject = (Symbol) applicationSyntax.Subject.Accept(this);
        var args    = applicationSyntax.Arguments.Select(_=>_.Accept(this));

        return new ApplicationExpression(ctx,applicationSyntax,subject,args);
    }

    protected internal override IBindingNode VisitContext(ContextSyntax syntaxNode)
    {
        return syntaxNode.Accept(this);
    }

    protected internal override IBindingNode DefaultVisit(AtSyntaxNode node)
    {
        throw new NotImplementedException($"Visit{node.GetType().Name.Replace("Syntax","")}()");
    }

    protected internal override IBindingNode VisitBinary(BinaryExpressionSyntax binaryExpressionSyntax)
    {
        var left  = (Expression) Visit(binaryExpressionSyntax.Left);
        var right = (Expression) Visit(binaryExpressionSyntax.Right);
        var op    = ctx.LookupSymbol(binaryExpressionSyntax.Operator.Text) ?? new UndefinedSymbol(ctx, binaryExpressionSyntax.Operator);
        return new BinaryOperation(ctx, binaryExpressionSyntax,op,left,right);
    }

    protected internal override IBindingNode VisitTokenCluster(TokenClusterSyntax tokenClusterSyntax)
        => ctx.LookupSymbol(tokenClusterSyntax.TokenCluster.Text) ?? new UndefinedSymbol(ctx, tokenClusterSyntax.TokenCluster);

    protected internal override IBindingNode VisitLiteral(LiteralExpressionSyntax literalExpressionSyntax)
    {
        return new LiteralExpression(ctx, literalExpressionSyntax, literalExpressionSyntax.Literal.Value);
    }

    protected internal override IBindingNode VisitTypeDeclaration(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        return new TypeDeclaration(ctx,typeDeclarationSyntax);
    }

    protected internal override IBindingNode VisitDirective(DirectiveSyntax directiveSyntax)
    {
        return new Directive(ctx,directiveSyntax);
    }

    protected internal override IBindingNode VisitVariableDeclaration(VariableDeclarationSyntax variableDeclarationSyntax)
    {
        return new VariableDeclaration(ctx,variableDeclarationSyntax); 
    }

    protected internal override IBindingNode VisitMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        return new MethodDeclaration(ctx,methodDeclarationSyntax);
    }

    protected internal override IBindingNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax namespaceDeclarationSyntax)
    {
        return new NamespaceDeclaration(ctx,namespaceDeclarationSyntax);
    }

    protected internal override IBindingNode VisitRoundBlock(RoundBlockSyntax roundBlockSyntax)
    {
        return   roundBlockSyntax.Content.Count == 0 ? ctx.LookupSymbol("()")
               : roundBlockSyntax.Content.Count == 1 ? Visit(roundBlockSyntax.Content[0])
               : throw new NotImplementedException(roundBlockSyntax.FullText);
    }
}
}
