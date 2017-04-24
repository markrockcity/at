using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public class Literal : Operation
{
    public Literal(Context ctx, TypeSymbol type, LiteralExpressionSyntax node, object value, Operation prev) : base(ctx,node,prev)
    {
        Value = value;
        Type = type;
    }
    
    public object Value {get; }

    public override TypeSymbol Type  {get; }

    public override T Accept<T>(BindingTreeVisitor<T> visitor) => 
        visitor.VisitLiteral(this);
    

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol)
    {
        Debug.WriteLine($"LiteralExpression.ReplaceSymbol({undefinedSymbol},{newSymbol})");
        return this;
    }

    public override string ToString()
    {
        return $"Literal({Value ?? "null"})";
    }
}
}
