using System;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax 
{
public abstract class ExpressionSyntax : AtSyntaxNode
{
    protected ExpressionSyntax(IEnumerable<AtSyntaxNode> nodes, IExpressionSource expressionSource, IEnumerable<AtDiagnostic> diagnostics) : base(nodes,diagnostics)
    {
        ExpressionSource = expressionSource;
    }

    //The expression definition that created this expression
    public IExpressionSource ExpressionSource {get;}

    //e.g., semicolon
    public ExpressionSyntax WithEndToken(AtToken endToken)
    {
        nodes.append(endToken);
        _text = null;
        return this;
    }
}
}