using System;
using System.Collections.Generic;
using At.Binding;
using At.Contexts;
//using At.Symbols;

namespace At
{

public interface ISymbol : IBindingNode
{
    /// <summary>Gets the symbol name. Returns the empty string if unnamed.</summary>
    string Name { get; }
    ISymbol ParentSymbol {get;}
    Definition Definition {get;}


    //void Accept(SymbolVisitor visitor);
    TResult Accept<TResult>(SymbolVisitor<TResult> visitor);
}



public abstract class Symbol : ISymbol
{
    internal Definition definition;
    readonly AtSyntaxNode _syntax;

    public Symbol(string name, AtSyntaxNode syntaxNode = null, Definition definition = null) 
    {
         Name = name ?? "";
         _syntax = syntaxNode;
         this.definition = definition;
    }

    public virtual string Name { get; }
    AtSyntaxNode IBindingNode.Syntax => _syntax;
    Definition ISymbol.Definition => definition;

    /*
    public ContextSymbol Context
    {
        get
        {
            for (var parentSymbol = ParentSymbol; parentSymbol != null; parentSymbol = parentSymbol.ParentSymbol)
            {
                if (parentSymbol is ContextSymbol ctxSymbol)
                {
                    return ctxSymbol;
                }
            }

            return null;        
        }
    } //ContextSymbol ISymbol.ParentContext => ParentContext;

    public virtual ContextSymbol TopContext
    {
        get
        {
            ContextSymbol getTopCtx(ContextSymbol ctx) => ctx.IsTopContext ? ctx : getTopCtx(ctx.Context);
            return getTopCtx(this is ContextSymbol thisContext ? thisContext : this.Context);
        }
    }
    */

    ISymbol ISymbol.ParentSymbol => ParentSymbol;

    public abstract Symbol ParentSymbol {get;}
    //ISymbol ISymbol.ParentSymbol => ParentSymbol;

    //public abstract void Accept(SymbolVisitor visitor);
    public abstract TResult Accept<TResult>(SymbolVisitor<TResult> visitor);
    public void Accept(BindingTreeVisitor visitor) => visitor.VisitSymbol(this);

    //public abstract TResult Accept<TResult,TArgument>(SymbolVisitor<TResult,TArgument> visitor, TArgument a);

    //internal virtual AtCompilation declaringCompilation => TopContext.declaringCompilation;
}
}