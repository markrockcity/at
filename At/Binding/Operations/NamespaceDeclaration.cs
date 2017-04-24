using System;
using At.Contexts;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public class NamespaceDeclaration : Declaration
{
    public NamespaceDeclaration(NamespaceSymbol s,Context ctx,NamespaceDefinition def,NamespaceDeclarationSyntax syntaxNode,Operation prev) : base(ctx,syntaxNode,prev)
    {
        Symbol = s ?? throw new ArgumentNullException(nameof(s));
        Definition = def;
    }

    public NamespaceSymbol Symbol {get;}
    public NamespaceDefinition Definition {get;}
    public NamespaceDeclarationSyntax Syntax => (NamespaceDeclarationSyntax) ExpressionSyntax;

    protected internal override Symbol DeclaredSymbol=>Symbol;

    protected internal override TypeSymbol DeclaredType => throw new NotImplementedException();

    public override T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitNamespaceDeclaration(this);
    

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

}
}
