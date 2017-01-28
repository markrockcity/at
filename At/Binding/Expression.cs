using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At.Binding
{

public abstract class Expression : IBindingNode
{
    protected Expression(Context ctx, ExpressionSyntax syntaxNode)
    {
        Context = ctx;
        ExpressionSyntax = syntaxNode;
    }

    public Context Context {get;}
    protected ExpressionSyntax ExpressionSyntax { get;}

    AtSyntaxNode IBindingNode.Syntax => ExpressionSyntax;

    public abstract Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol, ISymbol newSymbol);
   
    public abstract void Accept(BindingTreeVisitor visitor);

    internal void undefined(IEnumerable<UndefinedSymbol> undefinedSymbols)
    {
      foreach (var undef in undefinedSymbols)
            Context.undefined(undef,this); 
    }
    internal void undefined(UndefinedSymbol undefinedSymbol)
    {
       Context.undefined(undefinedSymbol,this);
    }
}
}
