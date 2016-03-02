namespace At.Syntax
{
public class IdentifierSyntax : ExpressionSyntax
{
    internal IdentifierSyntax(string text) : base(text) {}

    public string Identifier
    {
        get;
        internal set;
    }
}
}