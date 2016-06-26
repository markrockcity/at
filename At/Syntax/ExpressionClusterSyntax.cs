using System.Collections.Generic;

namespace At.Syntax
{
public class ExpressionClusterSyntax : AtSyntaxNode
{
    internal ExpressionClusterSyntax(IEnumerable<AtSyntaxNode> nodes, IEnumerable<AtDiagnostic> diagnostics) : base(nodes,diagnostics)
    {
    }
}
}