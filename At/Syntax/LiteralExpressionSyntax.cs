using System.Collections.Generic;

namespace At.Syntax
{
public class LiteralExpressionSyntax : ExpressionSyntax
{

    internal LiteralExpressionSyntax
    (
        AtToken literalToken,
        IEnumerable<AtSyntaxNode> nodes,
        IEnumerable<AtDiagnostic> diagnostics) 

        : base(nodes, diagnostics) {

        this.Literal = literalToken;
    }

    public AtToken Literal
    {
        get;
        private set;
    }
}
}