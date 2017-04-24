using System;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
/// <summary>Represents syntax for an application (e.g., function application)</summary>
public class InvocationExpressionSyntax : ExpressionSyntax
{

    internal InvocationExpressionSyntax
    (
        ExpressionSyntax exp,
        ListSyntax<ArgumentSyntax> args,
        IExpressionSource exprSrc = null,
        IEnumerable<AtDiagnostic>    diagnostics = null)
        
        : base(new AtSyntaxNode[]{exp}.Concat(args.nodes),exprSrc,diagnostics){

        this.Expression  = exp;
        this.Arguments = args;
    }

    public ExpressionSyntax Expression
    {
        get;
        private set;
    }

    public ListSyntax<ArgumentSyntax> Arguments
    {
        get;
        private set;
    }

    public override string PatternName => "Invoke";

    public override bool MatchesPattern(SyntaxPattern p, IDictionary<string,AtSyntaxNode> d = null)
    {
        var isMatch =    (p.Text == PatternName)
                      && (p.Content == null || MatchesPatterns(p.Content,nodes,d))

               ||  base.MatchesPattern(p,d);

         if (isMatch && d != null && p.Key != null)
            d[p.Key] = this;

        return isMatch;
    }


    public override IEnumerable<string> PatternStrings()
    {
        var name = PatternName;
        
        foreach(var l in Expression.PatternStrings())
        {
            if (!Arguments.List.Any())
                yield return $"{name}[{l}]()";
            else
                foreach(var r in PatternStrings(Arguments.List))
                {
                    yield return $"{name}[{l}]({r})";
                }
        }
                
        yield return name;

        foreach(var b in base.PatternStrings())
            yield return b;
    }

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitInvoke(this);
    }
}
}