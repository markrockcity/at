using At.Binding;
using At.Symbols;

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

    public override TypeSymbol Type {get; protected internal set;}

    public override TResult Accept<TResult>(BindingTreeVisitor<TResult> visitor)
    {
        return visitor.VisitKeyword(this);
    }

    public override string ToString()
    {
        return $"Keyword({Name})";
    }
}
}