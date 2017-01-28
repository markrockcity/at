using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At.Binding
{
public class LiteralExpression : Expression
{
    public LiteralExpression(Context ctx, LiteralExpressionSyntax node, object value) : base(ctx,node)
    {
        Value = value;
    }
    
    public object Value {get; }

    public override void Accept(BindingTreeVisitor visitor)
    {
        visitor.VisitLiteral(this);
    }

    public override Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol, ISymbol newSymbol)
    {
        Debug.WriteLine($"BindingLiteral.ReplaceSymbol({undefinedSymbol},{newSymbol})");
        return this;
    }

    public override string ToString()
    {
        return $"Literal({Value ?? "null"})";
    }
}
}
