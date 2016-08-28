using System.Collections.Generic;

namespace At.Syntax
{
public class LiteralExpressionSyntax : ExpressionSyntax
{

    internal LiteralExpressionSyntax
    (
        AtToken literalToken,
        IEnumerable<AtSyntaxNode> nodes,
        IExpressionSource expSrc,
        IEnumerable<AtDiagnostic> diagnostics) 

        : base(nodes, expSrc, diagnostics) {

        this.Literal = literalToken;
    }

    public AtToken Literal
    {
        get;
        private set;
    }
}
}