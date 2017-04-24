using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public class SymbolReference : Operation
{
    
    public SymbolReference(ISymbol symbol, Context ctx, ExpressionSyntax syntaxNode,Operation previousOperation) : base(ctx,syntaxNode,previousOperation)
    {
        Symbol = symbol;
   }

    public ISymbol Symbol {get;}

    public override TypeSymbol Type => Symbol.Type;

    public override T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitSymbolReference(this);
    

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol)
    {
        return (Symbol==undefinedSymbol) ? new SymbolReference(newSymbol,Context,ExpressionSyntax,Previous) : this;
    }

    public override string ToString()
    {
        return $"SymbolRef({Symbol})";
    }
}
}
