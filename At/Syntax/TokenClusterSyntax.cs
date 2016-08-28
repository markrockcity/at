using System.Collections.Generic;
using System.Diagnostics;

namespace At.Syntax
{
internal class TokenClusterSyntax : ExpressionSyntax
{
    private AtToken t;

    internal TokenClusterSyntax(AtToken tokenCluster, IExpressionSource expSrc = null, IEnumerable<AtDiagnostic> diagnostics = null) :
         base(new AtSyntaxNode[]{tokenCluster},expSrc,diagnostics)
    {
        Debug.Assert(tokenCluster!= null && tokenCluster.Kind==TokenKind.TokenCluster);
        TokenCluster = tokenCluster;
    }

    public AtToken TokenCluster {get;}
}
}