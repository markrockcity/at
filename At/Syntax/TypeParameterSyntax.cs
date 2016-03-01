namespace At.Syntax
{
public class TypeParameterSyntax : AtSyntaxNode
{
    AtSyntaxNode identifier;

    // Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterSyntax
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