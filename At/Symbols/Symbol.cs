using System;
using At.Binding;
using At.Contexts;
using At.Symbols;

namespace At
{

public interface ISymbol : IBindingNode
{
    /// <summary>Gets the symbol name. Returns the empty string if unnamed.</summary>
    string Name { get; }

    ISymbol ParentSymbol {get;}

    ///<summary>declaration of symbol</summary>
    Declaration Declaration {get;}

    /// <summary>definition of method, class, etc.</summary>
    Definition Definition {get;}

    bool IsUndefined {get;}

    /// <summary>bound type for value represented by symbol</summary>
    TypeSymbol Type {get;}
}



public abstract class Symbol : ISymbol
{
    internal Definition _definition;
    internal Declaration _declaration;
    readonly AtSyntaxNode _syntax;

    public Symbol(string name, AtSyntaxNode syntaxNode = null, Declaration declaration = null, Definition definition = null) 
    {
         Name = name ?? "";
         _syntax = syntaxNode;
         _definition = definition;
         _declaration = declaration;
    }

    public virtual string Name { get; }

    /// <summary>bound type for value represented by symbol</summary>
    public abstract TypeSymbol Type {get; protected internal set;}


    AtSyntaxNode IBindingNode.Syntax => _syntax;
    Definition ISymbol.Definition => _definition;
    Declaration ISymbol.Declaration => _declaration;

    ISymbol ISymbol.ParentSymbol => ParentSymbol;

    public abstract Symbol ParentSymbol {get;}

    public virtual bool IsUndefined => this is UndefinedSymbol;

    public virtual T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitSymbol(this);
    public override string ToString() => $"{GetType().Name.Replace("Symbol","")}({Name})";
}
}