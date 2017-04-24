using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At.Syntax
{
    public class TypeParameterSyntax : NameSyntax
    {
        public TypeParameterSyntax(AtToken identifier,IExpressionSource expDef = null,IEnumerable<AtDiagnostic> diagnostics = null) : base(identifier,null,expDef,diagnostics)
        {
        }
    }
}
