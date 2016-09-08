namespace At.Syntax
{
/// <summary>Represents syntax for an empty expression, e.g., ";"</summary>
public class EmptyExpressionSyntax : ExpressionSyntax
{
    public EmptyExpressionSyntax(IExpressionSource expSrc,AtSyntaxNode endToken)
    : base(new AtSyntaxNode[] {endToken},expSrc,null)
    {
    }
}
}