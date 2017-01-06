using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace At.Symbols
{
public interface IContextSymbol : ISymbol
{
    bool IsTopContext {get;}

    /// <param name="name">name of child symbol or <c>null</c> for all</param>
    IEnumerable<ISymbol> ChildSymbols(string name = null);

}

//NamespaceOrTypeSymbol, NamespaceSymbol
internal abstract class ContextSymbol : Symbol, IContextSymbol
{
    private IReadOnlyList<Symbol> allChildSymbols;
    private ConcurrentDictionary<string, Symbol> cachedLookup;
    private Symbol parentSymbol;

    protected ContextSymbol(ContextSymbol parentContext)
    {
        this.parentSymbol = parentContext;
        this.cachedLookup = new ConcurrentDictionary<string, Symbol>();
    }

    public virtual bool IsTopContext => ParentContext == null;
    public override Symbol ParentSymbol => parentSymbol;

    public virtual IReadOnlyList<Symbol> ChildSymbols(string name = null)
    {
	    if (this.allChildSymbols==null)
	    {
		    var list = new List<Symbol>();
		    list.AddRange(this.cachedLookup.Values);
		    this.allChildSymbols = list;
	    }

	    return (name != null) 
                    ? this.allChildSymbols.Where(_=>_.Name==name).ToList()
                    : this.allChildSymbols;
    }
    IEnumerable<ISymbol> IContextSymbol.ChildSymbols(string name) => ChildSymbols(name).Cast<ISymbol>();

    public override void Accept(SymbolVisitor visitor)
    {
       visitor.VisitContext(this);
    }

    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
    {
        return default(TResult);
    }

    public override TResult Accept<TResult, TArgument>(SymbolVisitor<TResult,TArgument> visitor,TArgument argument)
    {
        return visitor.VisitContext(this,argument);
    }

}

//MergedNamespaceSymbol
internal class TopContextSymbol : ContextSymbol
{
    private AtCompilation atCompilation;
  
    public @TopContextSymbol(AtCompilation atCompilation) : base(parentContext:null)
    {
        this.atCompilation = atCompilation;
    }
}
}