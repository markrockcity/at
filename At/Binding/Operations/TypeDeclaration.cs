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
public class TypeDeclaration : Declaration
{
    public TypeDeclaration(TypeSymbol s, Context ctx, TypeSymbol baseType=null, TypeSymbol metatype = null, TypeDefinition def = null, TypeDeclarationSyntax syntaxNode = null, Operation prev = null) : base(ctx,syntaxNode,prev)
    {
        Definition = def;
        Symbol = s ?? throw new ArgumentNullException(nameof(s));
    }

    public TypeDefinition Definition {get; internal set;}
    public TypeSymbol Symbol {get;}
    public TypeDeclarationSyntax Syntax => (TypeDeclarationSyntax) ExpressionSyntax;

 

    protected internal override Symbol     DeclaredSymbol => Symbol;
    protected internal override TypeSymbol DeclaredType   => throw new NotImplementedException();

    public override T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitTypeDeclaration(this);

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

}
}
