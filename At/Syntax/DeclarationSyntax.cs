namespace At.Syntax
{
public abstract class DeclarationSyntax : ExpressionSyntax
{
    protected DeclarationSyntax(string name, string exprText) : base(exprText)
    {
        this.Name = name;
    }
    public string Name {get; internal set;}
}
}