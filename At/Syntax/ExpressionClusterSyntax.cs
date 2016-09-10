using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
public class ExpressionClusterSyntax : ExpressionSyntax
{
    internal ExpressionClusterSyntax(IEnumerable<AtSyntaxNode> nodes, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics) : base(nodes,expDef,diagnostics)
    {
    }

    public override IEnumerable<string> PatternStrings()
    {
        yield return $"ExprCluster({(string.Join(",",nodes.Select(_=>_.PatternName())))})";
        yield return PatternName();

        foreach(var x in base.PatternStrings())
            yield return x;
    }

    public override string PatternName()
    {
        return "ExprCluster";
    }
}
}