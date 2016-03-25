using System.Collections.Generic;
using At.Syntax;

namespace At.Syntax
{
//TODO: remove ErrorNode class
public class ErrorNode : ExpressionSyntax
{
    public ErrorNode(IList<AtDiagnostic> diagnostics, string message, AtSyntaxNode node) : 
        base(new[] {node},diagnostics)
    {
        Diagnostics = diagnostics;
        Message = message;
    }

    public IList<AtDiagnostic> Diagnostics {get;}
    public string Message {get;}
}
}