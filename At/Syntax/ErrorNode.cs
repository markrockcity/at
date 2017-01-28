using System;
using System.Collections.Generic;
using At.Syntax;

namespace At.Syntax
{
//TODO: remove ErrorNode class
public class ErrorNode : ExpressionSyntax
{
    public ErrorNode(IList<AtDiagnostic> diagnostics, string message, AtSyntaxNode node, IExpressionSource expDef=null) : 
        base(new[] {node},expDef,diagnostics)
    {
        Diagnostics = diagnostics;
        Message = message;
    }

    public IList<AtDiagnostic> Diagnostics {get;}
    public string Message {get;}

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        throw new NotImplementedException();
    }
}
}