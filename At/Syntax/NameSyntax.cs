namespace At.Syntax
{
//TypeSyntax, etc.
public class NameSyntax : ExpressionSyntax
{
    
    internal NameSyntax(AtToken identifier, ListSyntax<NameSyntax> typeArgs = null) : base(identifier,typeArgs)
    {
        Identifier = identifier;
        TypeArguments = typeArgs;
    }

    public AtToken Identifier {get;}
    public ListSyntax<NameSyntax> TypeArguments {get;}
}
}