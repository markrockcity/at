using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
/// <summary>Represents syntax for a binary operation.</summary>
public class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryExpressionSyntax
    (
        ExpressionSyntax          leftOperand,
        AtToken                   @operator,
        ExpressionSyntax          rightOperand,
        IExpressionSource         exprSrc = null,
        IEnumerable<AtDiagnostic> diagnostics = null)
        
        : base(new AtSyntaxNode[]{leftOperand,@operator,rightOperand},exprSrc,diagnostics){

        this.Left     = leftOperand;
        this.Operator = @operator;
        this.Right    = rightOperand;
    }

    public ExpressionSyntax Left
    {
        get;
        private set;
    }

    public AtToken Operator
    {
        get;
        private set;
    }

    public ExpressionSyntax Right
    {
        get;
        private set;
    }

    public override bool MatchesPattern(SyntaxPattern p, IDictionary<string,AtSyntaxNode> d = null)
    {
        var t =    (p.Text == PatternName)
               &&  (p.Token1 == null || p.Token1 == Operator.PatternName)
               &&  (p.Token2 == null)
               &&  (p.Content == null || MatchesPatterns(p.Content,new[]{Left,Right}))

               ||  base.MatchesPattern(p,d);

         if (t && d != null && p.Key != null)
            d[p.Key] = this;

        return t;
    }


    public override IEnumerable<string> PatternStrings()
    {
        var name = PatternName;
        var o = Operator.PatternName;

        foreach(var l in Left.PatternStrings())
        foreach(var r in Right.PatternStrings())
        {
            yield return $"{name}[{o}]({l},{r})";
            yield return $"{name}({l},{r})";
        }
        
        yield return $"{name}[{o}]";
        yield return name;

        foreach(var b in base.PatternStrings())
            yield return b;
    }

}
}