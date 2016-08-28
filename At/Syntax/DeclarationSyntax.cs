using System.Collections.Generic;

namespace At.Syntax
{
public abstract class DeclarationSyntax : ExpressionSyntax
{
    protected DeclarationSyntax
    (
        AtToken delcarationOperator, // "@"
        ExpressionSyntax expression, 
        IEnumerable<AtSyntaxNode> nodes,
        IExpressionSource expDef,
        IEnumerable<AtDiagnostic> diagnostics ) : 

        base(nodes, expDef, diagnostics) {

    }
}
}