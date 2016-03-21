namespace At.Syntax
{
public class NameSyntax : ExpressionSyntax
{
    
    internal NameSyntax(AtToken identifier) : base(identifier)
    {
        Identifier = identifier;
    }

    public AtToken Identifier {get;}
}
}