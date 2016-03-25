using System;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax 
{
public abstract class ExpressionSyntax : AtSyntaxNode
{

    protected ExpressionSyntax(IEnumerable<AtSyntaxNode> nodes, IEnumerable<AtDiagnostic> diagnostics) : base(nodes,diagnostics)
    {
    }

}
}