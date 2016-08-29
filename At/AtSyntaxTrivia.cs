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
        ITokenRule tokenDefinition = null,
        IEnumerable<AtDiagnostic> diagnostics = null) 

        : base(kind,position,text,tokenSrc: tokenDefinition,diagnostics: diagnostics) {
    }

    public override bool IsTrivia => true;
}
}
