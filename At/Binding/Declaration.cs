using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public abstract class Declaration : Expression
{
    public Declaration(Context ctx, DeclarationSyntax syntaxNode) : base(ctx,syntaxNode)
    {
    }

    public abstract ContextSymbol Symbol {get;}

    public DeclarationSyntax Syntax => (DeclarationSyntax) ExpressionSyntax;

}
}
