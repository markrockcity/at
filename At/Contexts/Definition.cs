using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;

namespace At.Contexts
{
//e.g., from a declaration
public abstract class Definition : Context
{
    protected Definition(Context parentCtx,DiagnosticsBag diagnostics,AtSyntaxNode syntaxNode = null) : base(parentCtx,diagnostics,syntaxNode)
    {
    }

    public ContextSymbol Symbol { get;}
}
}
