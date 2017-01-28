using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Symbols
{
public class TypeSymbol : ContextSymbol
{
    protected internal TypeSymbol(string name, AtSyntaxNode syntaxNode,ContextSymbol parentContext) : base(name,syntaxNode,parentContext)
    {
    }
}
}
