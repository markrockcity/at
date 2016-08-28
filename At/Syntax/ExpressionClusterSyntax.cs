using System.Collections.Generic;

namespace At.Syntax
{
public class ExpressionClusterSyntax : ExpressionSyntax
{
    internal ExpressionClusterSyntax(IEnumerable<AtSyntaxNode> nodes, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics) : base(nodes,expDef,diagnostics)
    {
    }
}
}