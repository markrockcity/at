using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
//#directive
public class Directive : Operation
{
    public Directive(Context ctx, DirectiveSyntax syntaxNode,Operation prev) : base(ctx,syntaxNode,prev)
    {
    }

    public DirectiveSyntax Syntax => (DirectiveSyntax) ((IBindingNode)this).Syntax;

        public override TypeSymbol Type => throw new NotImplementedException();

    public override T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitDirective(this);

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol,ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => $"Directive({Syntax.ToString().Trim()})";
}
}
