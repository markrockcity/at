using System;
using System.Diagnostics;
using At.Contexts;
using At.Symbols;

namespace At.Binding
{
//BoundTreeVisitor
public abstract class BindingTreeVisitor<TResult>
{ 
    public virtual TResult  Visit(IBindingNode node)
    {
        Debug.WriteLine(GetType()+$".Visit({node} : {node.GetType()})");

        if (node != null)
            return node.Accept(this);     

        return DefaultVisit(node);
    }

    protected internal virtual TResult DefaultVisit(IBindingNode node)
    {
         throw new NotImplementedException(node.ToString());
    }

    private TResult defaultVisit(IBindingNode node, string method)
    {
        Debug.WriteLine(GetType()+$".{method}({node})");
        return DefaultVisit(node);
    }

    protected internal virtual TResult VisitUndefined(UndefinedSymbol undefinedSymbol)
        => defaultVisit(undefinedSymbol,nameof(VisitUndefined));

    protected internal virtual TResult VisitOperator(OperatorSymbol operatorSymbol)
        => defaultVisit(operatorSymbol,nameof(VisitOperator));

    protected internal virtual TResult VisitKeyword(KeywordSymbol keywordSymbol)
        => defaultVisit(keywordSymbol,nameof(VisitKeyword));

    protected internal virtual TResult VisitBinary(BinaryOperation binaryOperation)
    {
        Debug.WriteLine(GetType()+$".VisitBinary({binaryOperation})");
        return DefaultVisit(binaryOperation);
    }

    protected internal virtual TResult VisitVariable(VariableSymbol variableSymbol)
     => defaultVisit(variableSymbol,nameof(VisitVariable));


    protected internal virtual  TResult VisitContext(ContextSymbol contextSymbol)
     => defaultVisit(contextSymbol,nameof(VisitContext));

    protected internal virtual TResult  VisitParameter(ParameterSymbol parameterSymbol)
    {
        Debug.WriteLine(GetType()+$".{nameof(VisitParameter)}({parameterSymbol})");
        return DefaultVisit(parameterSymbol);
    }

    protected internal virtual TResult VisitSymbolReference(SymbolReference symbolReference)
    {
        Debug.WriteLine(GetType()+$".{nameof(VisitSymbolReference)}({symbolReference})");
        return DefaultVisit(symbolReference);
    }

    protected internal virtual TResult VisitDirective(Directive directive)
    {
        Debug.WriteLine(GetType()+$".VisitDirective({directive})");
        return DefaultVisit(directive);
    }

    protected internal virtual TResult VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        Debug.WriteLine(GetType()+$".VisitTypeDeclaration({typeDeclaration})");
        return DefaultVisit(typeDeclaration);
    }

    protected internal virtual TResult VisitInvocation(Invocation invocation)
    {
        Debug.WriteLine(GetType()+$".VisitInvocation({invocation})");
        return DefaultVisit(invocation);
    }

    protected internal virtual TResult VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
    {
        Debug.WriteLine(GetType()+$".VisitNamespaceDeclaration({namespaceDeclaration})");
        return DefaultVisit(namespaceDeclaration);
    }

    protected internal virtual TResult VisitMethodDeclaration(MethodDeclaration methodDeclaration)
    {
        Debug.WriteLine(GetType()+$".VisitMethodDeclaration({methodDeclaration})");
        return DefaultVisit(methodDeclaration);
    }

    protected internal virtual TResult VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
        Debug.WriteLine(GetType()+$".VariableDeclaration({variableDeclaration})");
        return DefaultVisit(variableDeclaration);
    }

    protected internal virtual TResult VisitContext(Context context)
    {
        Debug.WriteLine(GetType()+$".VisitContext({context})");
        return DefaultVisit(context);
    }

    protected internal virtual TResult VisitLiteral(Literal literal)
    {
        Debug.WriteLine(GetType()+$".VisitLiteral({literal})");
        return DefaultVisit(literal);
    }

    protected internal virtual TResult VisitSymbol(Symbol symbol)
    {
        Debug.WriteLine(GetType()+$".VisitSymbol({symbol})");
        return DefaultVisit(symbol);
    }
}
}