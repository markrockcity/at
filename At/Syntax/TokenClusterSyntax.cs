using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    public override bool MatchesPattern(SyntaxPattern pattern)
    {
        var s =pattern.ToString();
        return PatternStrings().Any(_=>_==s);
    }

    public override IEnumerable<string> PatternStrings()
    {
        yield return $"TokenCluster('{TokenCluster.Text}')";
        yield return PatternName;
        foreach(var x in base.PatternStrings())
            yield return x;
    }

    public override string PatternName => "TokenCluster";
}
}