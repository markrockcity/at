using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
//(IOperation/BoundExpression)
/// <summary>A binding/bound operation (expression)</summary>
public abstract class Operation : IBindingNode
{
    protected Operation(Context ctx, ExpressionSyntax syntaxNode, Operation previousOperation)
    {
        if (previousOperation != null) 
            previousOperation.Next = this;

        Previous = previousOperation;
        Context = ctx;
        ExpressionSyntax = syntaxNode;
    }
    
    public Operation Previous {get;}
    public Operation Next {get; private set;}

    public Context Context {get;}
    protected ExpressionSyntax ExpressionSyntax { get;}

    /// <summary>Symbol for type of operation's result.</summary>
    public abstract TypeSymbol Type {get;}

    AtSyntaxNode IBindingNode.Syntax => ExpressionSyntax;

    public abstract Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol);
   
    public abstract TResult Accept<TResult>(BindingTreeVisitor<TResult> visitor);

    internal void registerUndefinedSymbols(IEnumerable<UndefinedSymbol> undefinedSymbols)
    {
        foreach (var undef in undefinedSymbols)
            Context.registerUndefinedSymbol(undef,this); 
    }
    internal void registerUndefinedSymbol(UndefinedSymbol undefinedSymbol)                                                                                                                                                                                                                                                                                                                                                                                                    
    {
       Context.registerUndefinedSymbol(undefinedSymbol,this);
    }
}
}
