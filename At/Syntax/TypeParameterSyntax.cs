namespace At.Syntax
{
// Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax
public class TypeParameterSyntax : AtSyntaxNode
{
    AtSyntaxNode identifier;


    internal TypeParameterSyntax(AtToken identifier) : base(new[] {identifier})
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