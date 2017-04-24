using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;

namespace At.Contexts
{

public interface IDefinition : IBindingNode
{
    /// <summary>The symbol being defined by this definition.</summary>
    ContextSymbol Symbol {get;}
}

//e.g., from a declaration
public abstract class Definition : Context, IDefinition
{
    protected Definition(Context parentCtx, DiagnosticsBag diagnostics, AtSyntaxNode syntaxNode = null) : base(parentCtx,diagnostics,null,syntaxNode)
    {
    }

    public override string ToString() => $"{GetType().Name}({ContextSymbol.Name})";

    protected internal override abstract ContextSymbol ContextSymbol { get;}

    ContextSymbol IDefinition.Symbol => ContextSymbol;
}
}
