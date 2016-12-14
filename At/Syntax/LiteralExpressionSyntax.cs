using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
public class LiteralExpressionSyntax : ExpressionSyntax
{

    internal LiteralExpressionSyntax
    (
        AtToken                     literalToken,
        IEnumerable<AtSyntaxNode>   nodes,
        IExpressionSource           expSrc,
        IEnumerable<AtDiagnostic>   diagnostics
    )   : base(nodes, expSrc, diagnostics) 
    {
        this.Literal = literalToken;
    }

    public AtToken Literal
    {
        get;
        private set;
    }

    public override string PatternName => "Literal";


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
        yield return $"{PatternName}({Literal.Text})";
        yield return PatternName;
        foreach(var x in base.PatternStrings())
            yield return x;
    }

}
}