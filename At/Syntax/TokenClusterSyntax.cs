using System.Collections.Generic;
using System.Diagnostics;

namespace At.Syntax
{
public class TokenClusterSyntax : ExpressionSyntax
{
    internal TokenClusterSyntax(AtToken tokenCluster, IExpressionSource expSrc = null, IEnumerable<AtDiagnostic> diagnostics = null) :
         base(new AtSyntaxNode[]{tokenCluster},expSrc,diagnostics)
    {
        Debug.Assert(tokenCluster!= null && tokenCluster.Kind==TokenKind.TokenCluster);
        TokenCluster = tokenCluster;
    }

    public AtToken TokenCluster {get;}

    public override IEnumerable<string> PatternStrings()
    {
        yield return $"TokenCluster('{TokenCluster.Text}')";
        yield return PatternName();
        foreach(var x in base.PatternStrings())
            yield return x;
    }

    public override string PatternName() => "TokenCluster";
}
}