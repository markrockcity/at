using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At
{
/// <summary>
/// Represents a <see cref="AtSyntaxVisitor"/> visitor that visits only the single AtSyntaxNode
/// passed into its Visit method and produces a value of the type specified by the <typeparamref name="TResult"/> parameter.
/// </summary>
/// <typeparam name="TResult">
/// The type of the return value this visitor's Visit method.
/// </typeparam>
public abstract class AtSyntaxVisitor<TResult>
{
    public virtual TResult Visit(AtSyntaxNode node)
    {
        if (node != null)
        {
            return node.Accept(this);
        }
    
        return DefaultVisit(node);
    }

    /// <remarks>Arguments can either be <c ref='At.Syntax.ArgumentSyntax'>ArgumentSyntax</cref> nodes
    /// (from InvocationSyntax) or  naked ExpressionSyntax nodes (from application expressions)</remarks>
    protected internal virtual TResult VisitArgument(AtSyntaxNode argumentSyntax)
    {
        return DefaultVisit(argumentSyntax);
    }

    protected internal virtual TResult VisitNamespaceDeclaration(NamespaceDeclarationSyntax namespaceDeclarationSyntax)
    {
        return DefaultVisit(namespaceDeclarationSyntax);
    }

    protected internal virtual  TResult VisitVariableDeclaration(VariableDeclarationSyntax variableDeclarationSyntax)
    {
        return DefaultVisit(variableDeclarationSyntax);
    }

    protected internal virtual  TResult VisitMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        return DefaultVisit(methodDeclarationSyntax);
    }

    protected internal virtual  TResult VisitDirective(DirectiveSyntax directiveSyntax)
    {
        return DefaultVisit(directiveSyntax);
    }

    protected internal virtual TResult  VisitCompilationUnit(CompilationUnitSyntax compilationUnitSyntax)
    {
        return DefaultVisit(compilationUnitSyntax);
    }

    protected internal virtual  TResult VisitTypeDeclaration(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        return DefaultVisit(typeDeclarationSyntax);
    }

    protected internal virtual TResult DefaultVisit(AtSyntaxNode node)
    {
        Debug.Write($"{GetType()}.DefaultVisit({node.GetType()})");
        return default(TResult);
    }

    protected internal virtual TResult VisitInvoke(InvocationExpressionSyntax invocationExpressionSyntax)
    {
       return DefaultVisit(invocationExpressionSyntax);
    }

    protected internal virtual TResult VisitBinary(BinaryExpressionSyntax binaryExpressionSyntax)
        =>  DefaultVisit(binaryExpressionSyntax);

    protected internal virtual  TResult VisitLiteral(LiteralExpressionSyntax literalExpressionSyntax)
    {
        return DefaultVisit(literalExpressionSyntax);
    }

    protected internal virtual TResult  VisitApply(ApplicationSyntax applicationSyntax)
    {
        return DefaultVisit(applicationSyntax);
    }

    protected  internal virtual TResult  VisitContext(ContextSyntax syntaxNode)
    {
        return DefaultVisit(syntaxNode);
    }

    protected  internal virtual TResult VisitToken(AtToken atToken)
    {
        return DefaultVisit(atToken);
    }

    protected internal virtual TResult VisitTokenCluster(TokenClusterSyntax tokenClusterSyntax)
    {
        return DefaultVisit(tokenClusterSyntax);
    }

    protected internal virtual TResult VisitRoundBlock(RoundBlockSyntax roundBlockSyntax)
    {
        return DefaultVisit(roundBlockSyntax);
    }
}
}
