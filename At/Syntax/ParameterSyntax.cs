namespace At.Syntax
{
// Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax
public class ParameterSyntax : AtSyntaxNode
{
    AtSyntaxNode identifier;


    internal ParameterSyntax(AtToken identifier) : base(new[] {identifier})
    {
        this.identifier = identifier;
    }

    /// <summary>Gets the identifier.</summary>
    public AtSyntaxNode Identifier
    {
        get
        {
            return this.identifier;
        }
    }
}
}