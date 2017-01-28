using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;

namespace At.Contexts
{
//"MergedDeclaration"
public class TypeDefinition : Definition
{
    internal protected TypeDefinition(Context parentCtx, DiagnosticsBag diagnostics, AtSyntaxNode syntaxNode = null) : base(parentCtx,diagnostics,syntaxNode)
    {
    }


    protected override ImmutableArray<IBindingNode> MakeContents()
    {
        throw new NotImplementedException();
    }
}
}
