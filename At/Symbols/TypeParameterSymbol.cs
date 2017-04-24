using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At.Symbols
{
    public class TypeParameterSymbol : TypeSymbol
    {
        public TypeParameterSymbol(string name, TypeSymbol baseType = null, TypeParameterSyntax syntaxNode = null,ContextSymbol parentContext = null) : base(name,baseType,TypeSymbol.TypeParameter,syntaxNode,parentContext)
        {
        }
    }
}
