using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public class AtSyntaxTrivia : AtToken, Limpl.ISyntaxTrivia
{
    internal AtSyntaxTrivia
    (
        TokenKind kind, 
        int       position,
        string    text=null, 
        Limpl.ITokenRule<AtToken> tokenSrc = null,
        IEnumerable<AtDiagnostic> diagnostics = null) 

        : base(kind,position,text,tokenSrc: tokenSrc,diagnostics: diagnostics) {
    }

    public override bool IsTrivia => true;
}
}
