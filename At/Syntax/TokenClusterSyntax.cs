using System;
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

    public override string PatternName => "TokenCluster";


    public override bool MatchesPattern(SyntaxPattern pattern, IDictionary<string,AtSyntaxNode> d = null)
    {
        var s = pattern.ToString(withKeyPrefix:false);

        var t=PatternStrings().Any(_=>_==s);

        if (t && d != null && pattern.Key != null)
            d[pattern.Key] = this;

        return t;
    }

    public override IEnumerable<string> PatternStrings()
    {
        yield return $"TokenCluster({TokenCluster.Text})";
        yield return PatternName;
        foreach(var x in base.PatternStrings())
            yield return x;
    }

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitTokenCluster(this);
    }

    public override string ToString()
    {
        return TokenCluster.Text;
    }
}
}