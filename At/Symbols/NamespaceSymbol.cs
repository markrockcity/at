using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Symbols
{
public class NamespaceSymbol : ContextSymbol
{
    public NamespaceSymbol(string name,AtSyntaxNode syntaxNode,ContextSymbol parentContext) : base(name,syntaxNode,parentContext)
    {
    }

    public override TypeSymbol Type
    {
        get => throw new NotImplementedException();
        protected internal set => throw new NotImplementedException();
    }
}
}
