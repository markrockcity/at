using System;
using System.Collections.Generic;

namespace At.Syntax
{
public class ArgumentSyntax : AtSyntaxNode
{

    internal ArgumentSyntax(ExpressionSyntax expression, IEnumerable<AtDiagnostic> diagnostics) : base(new[] {expression},diagnostics)
    {
        Expression = expression;
    }

    /// <summary></summary>
    public ExpressionSyntax Expression {get;}


    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.Visit(Expression);
    }
}
}