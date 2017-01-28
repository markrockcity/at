using System;
using At.Symbols;

namespace At
{
/*
public abstract class SymbolVisitor
{   
    public virtual void Visit(ISymbol symbol)
    {
        symbol?.Accept(this);
    }

    public virtual void DefaultVisit(ISymbol symbol)
    {
    }

    public virtual void VisitContext(IContextSymbol symbol)=>DefaultVisit(symbol);
}
*/

//CommonSymbolVisitor<TResult>
public class SymbolVisitor<TResult>
{
    public virtual TResult Visit(Symbol symbol)
	{
		if (symbol == null)
		{
			return default(TResult);
		}

		return symbol.Accept(this);
	}

        internal TResult VisitKeyword(KeywordSymbol keywordSymbol)
        {
            throw new NotImplementedException();
        }

        public virtual TResult VisitUndefined(UndefinedSymbol undefinedSymbol)
    {
         return DefaultVisit(undefinedSymbol);
    }

    public virtual TResult VisitContext(ContextSymbol symbol)
    {
        return DefaultVisit(symbol);
    }
    
    public virtual TResult DefaultVisit(Symbol symbol)
    {
         return default(TResult);
    }

}

    /*

internal abstract class SymbolVisitor<TResult, TArgument>     
{
    public virtual TResult Visit(Symbol symbol, TArgument argument)
	{
		if (symbol == null)
		{
			return default(TResult);
		}

		return symbol.Accept(this, argument);
	}



}*/
}