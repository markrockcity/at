namespace At.Syntax
{
public class IdentifierSyntax : ExpressionSyntax
{
    internal IdentifierSyntax(AtToken token) : base(token)
    {
        Identifier = token;
    }

    public AtToken Identifier {get;}
}
}