using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At.Binding
{
//#directive
public class Directive : Expression
{
    public Directive(Context ctx, DirectiveSyntax syntaxNode) : base(ctx,syntaxNode)
    {
    }

    public DirectiveSyntax Syntax => (DirectiveSyntax) ((IBindingNode)this).Syntax;

    public override void Accept(BindingTreeVisitor visitor)
    {
        visitor.VisitDirective(this);
    }

    public override Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol,ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{{{Syntax.ToString().Trim()}}}";
}
}
