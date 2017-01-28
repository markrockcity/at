using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public class AtSyntaxTrivia : AtToken
{
    internal AtSyntaxTrivia
    (
        TokenKind kind, 
        int       position,
        string    text=null, 
        ITokenSource tokenSrc = null,
        IEnumerable<AtDiagnostic> diagnostics = null) 

        : base(kind,position,text,tokenSrc: tokenSrc,diagnostics: diagnostics) {
    }

    public override bool IsTrivia => true;
}
}
