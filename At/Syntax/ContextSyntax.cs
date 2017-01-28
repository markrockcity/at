using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Syntax
{
public abstract class ContextSyntax : AtSyntaxNode
{
    public ContextSyntax(IEnumerable<AtSyntaxNode> nodes,IEnumerable<AtDiagnostic> diagnostics,bool isMissing = false) : base(nodes,diagnostics,isMissing)
    {
    }
}
}
