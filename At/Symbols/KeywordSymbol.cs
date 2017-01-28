using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;

namespace At
{
public sealed class KeywordSymbol : Symbol
{
    private Context ctx;

    public KeywordSymbol(Context ctx, string name) : base(name)
    {
        this.ctx = ctx;
    }

    public override Symbol ParentSymbol => null;
        
    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
    {
        return visitor.VisitKeyword(this);
    }

    public override string ToString()
    {
        return $"Keyword({Name})";
    }
}
}