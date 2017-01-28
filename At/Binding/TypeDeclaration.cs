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
    public TypeDeclaration(Context ctx,TypeDeclarationSyntax syntaxNode) : base(ctx,syntaxNode)
    {
        var d = ctx as Definition;
        Symbol = new TypeSymbol(syntaxNode.Identifier.Text,syntaxNode,d?.Symbol);
    }

    public override ContextSymbol Symbol { get; }


    public override void Accept(BindingTreeVisitor visitor)
    {
        visitor.VisitTypeDeclaration(this);
    }

    public override Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol, ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

}
}
