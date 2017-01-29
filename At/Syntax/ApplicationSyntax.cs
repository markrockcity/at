using System;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
/// <summary>Represents syntax for an application (e.g., function application)</summary>
public class ApplicationSyntax : ExpressionSyntax
{

    internal ApplicationSyntax
    (
        ExpressionSyntax subject,
        IList<ExpressionSyntax> args,
        IExpressionSource exprSrc = null,
        IEnumerable<AtDiagnostic>    diagnostics = null)
        
        : base(new AtSyntaxNode[]{subject}.Concat(args),exprSrc,diagnostics){

        this.Subject  = subject;
        this.Arguments = args.ToList().AsReadOnly();
    }

    public ExpressionSyntax Subject
    {
        get;
        private set;
    }

    public IReadOnlyList<ExpressionSyntax> Arguments
    {
        get;
        private set;
    }

    public override string PatternName => "Apply";

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
        
        foreach(var l in Subject.PatternStrings())
        foreach(var r in PatternStrings(Arguments))
        {
            yield return $"{name}({l},{r})";
            //yield return $"{name}({l},{r})";
        }
        
        //yield return $"{name}";
        yield return name;

        foreach(var b in base.PatternStrings())
            yield return b;
    }

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitApply(this);
    }
}
}