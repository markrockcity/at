using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;

namespace At
{
public sealed class UndefinedSymbol : Symbol
{
    private Context ctx;
    private AtToken token;


    public UndefinedSymbol(Context ctx, AtToken token) : base(token.Text, token)
    {
        this.ctx = ctx;
        this.token = token;
    }

    public override Symbol ParentSymbol => null;
        
    public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
    {
        return visitor.VisitUndefined(this);
    }

    public override string ToString()
    {
        return $"Undefined({token.Text})";
    }
}
}