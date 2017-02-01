using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;

namespace At
{
public sealed class OperatorSymbol : Symbol
{
    private Context ctx;

    public OperatorSymbol(Context ctx, string @operator) : base(@operator)
    {
        this.ctx = ctx;
    }

    public override Symbol ParentSymbol => null;
        
    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
    {
        return visitor.VisitOperator(this);
    }

    public override string ToString()
    {
        return $"Operator({Name})";
    }
}
}