using At.Binding;
using At.Syntax;

namespace At.Symbols
{
public sealed class ParameterSymbol : VariableSymbol
{
    internal Context ctx;

    public ParameterSymbol(string name, TypeSymbol parameterType, ParameterSyntax syntaxNode = null) : base(name,syntaxNode,parameterType ?? TypeSymbol.Unknown)
    {
        
    }

    public TypeSymbol ParameterType => VariableType;

    public override Symbol ParentSymbol => ctx.LookupContextMember(".");
        
    public override TResult Accept<TResult>(BindingTreeVisitor<TResult> visitor)
    {
        return visitor.VisitParameter(this);
    }

    public override string ToString() => $"Parameter({Name})";
}
}