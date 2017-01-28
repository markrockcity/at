using System;
using System.Diagnostics;
using At.Contexts;

namespace At.Binding
{
//BoundTreeVisitor
public abstract class BindingTreeVisitor 
{ 
    protected internal virtual void  Visit(IBindingNode node)
    {
        Debug.WriteLine(GetType()+$".VisitSymbol({node})");
        DefaultVisit(node);
    }

    protected internal virtual void DefaultVisit(IBindingNode node)
    {
         throw new NotImplementedException(node.ToString());
    }

    protected internal virtual void VisitDirective(Directive directive)
    {
        Debug.WriteLine(GetType()+$".VisitDirective({directive})");
        DefaultVisit(directive);
    }

    protected internal virtual void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        Debug.WriteLine(GetType()+$".VisitTypeDeclaration({typeDeclaration})");
        DefaultVisit(typeDeclaration);
    }

    protected internal virtual void VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
    {
        Debug.WriteLine(GetType()+$".VisitNamespaceDeclaration({namespaceDeclaration})");
        DefaultVisit(namespaceDeclaration);
    }

    protected internal virtual void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        Debug.WriteLine(GetType()+$".VisitMethodDeclaration({methodDeclaration})");
        DefaultVisit(methodDeclaration);
    }

    protected internal virtual void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        Debug.WriteLine(GetType()+$".VariableDeclaration({variableDeclaration})");
        DefaultVisit(variableDeclaration);
    }

    protected internal virtual void VisitContext(Context context)
    {
        Debug.WriteLine(GetType()+$".VisitContext({context})");
        DefaultVisit(context);
    }

    protected internal virtual void VisitLiteral(LiteralExpression literal)
    {
        Debug.WriteLine(GetType()+$".VisitLiteral({literal})");
        DefaultVisit(literal);
    }

    protected internal virtual void VisitSymbol(Symbol symbol)
    {
        Debug.WriteLine(GetType()+$".VisitSymbol({symbol})");
        DefaultVisit(symbol);
    }

    protected internal virtual void VisitApply(ApplicationExpression applicationExpression)
    {
        Debug.WriteLine(GetType()+$".VisitApply({applicationExpression})");
        DefaultVisit(applicationExpression);
    }
}
}