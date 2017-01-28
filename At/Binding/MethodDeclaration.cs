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
public class MethodDeclaration : Declaration
{
    public MethodDeclaration(Context ctx, MethodDeclarationSyntax syntaxNode) : base(ctx,syntaxNode)
    {
        var d = ctx as Definition;
        Symbol = new TypeSymbol(syntaxNode.Identifier.Text,syntaxNode,d?.Symbol);
    }

    public override ContextSymbol Symbol { get; }


    public override void Accept(BindingTreeVisitor visitor)
    {
        visitor.VisitMethodDeclaration(this);
    }

    public override Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol, ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

}
}
