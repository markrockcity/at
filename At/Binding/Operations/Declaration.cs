using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Contexts;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public interface IDeclaration : IBindingNode
{
    /// <summary>Symbol for declared name.</summary>
    Symbol Symbol {get;}

    /// <summary>Type of thing being declared (Class, Interface, Method, etc.)</summary>
    TypeSymbol Type {get;}
     
}

public abstract class Declaration : Operation, IDeclaration
{
    public Declaration(Context ctx, DeclarationSyntax syntaxNode, Operation prev) : base(ctx,syntaxNode,prev)
    {
    }

    Symbol IDeclaration.Symbol => DeclaredSymbol;
    TypeSymbol IDeclaration.Type => DeclaredType;

    public override TypeSymbol Type => TypeSymbol.Unit;
    public override string ToString() => $"{GetType().Name}(@{DeclaredSymbol.Name})";


    protected internal DeclarationSyntax DeclarationSyntax => (DeclarationSyntax) ExpressionSyntax;


    /// <summary>Symbol for declared name.</summary>
    protected internal abstract Symbol DeclaredSymbol {get;}
    protected internal abstract TypeSymbol DeclaredType {get;}
}
}
