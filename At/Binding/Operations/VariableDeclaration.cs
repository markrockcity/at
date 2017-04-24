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
public class VariableDeclaration : Declaration
{
    public VariableDeclaration(Context ctx, string name, TypeSymbol variableType, VariableDeclarationSyntax syntaxNode,Operation prev) : base(ctx,syntaxNode,prev)
    {
        var d = ctx as IDefinition;
        Symbol = new VariableSymbol(name ?? syntaxNode.Identifier.Text,syntaxNode,variableType,d?.Symbol);
        VariableType = variableType ?? TypeSymbol.Unknown;
    }

    public Symbol Symbol {get;}
    public TypeSymbol VariableType {get;}

    public VariableDeclarationSyntax Syntax => (VariableDeclarationSyntax) ExpressionSyntax;

    protected internal override Symbol DeclaredSymbol => Symbol;
    protected internal override TypeSymbol DeclaredType => (TypeSymbol) Context.LookupSymbol("At.Variable");

    public override T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitVariableDeclaration(this);

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

}
}
