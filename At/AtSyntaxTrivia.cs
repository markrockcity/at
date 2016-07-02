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
        IEnumerable<AtDiagnostic> diagnostics = null) 

        : base(kind,position,text,diagnostics){
    }

    public override bool IsTrivia
    {
        get
        {
            return true;
        }
    }
}
}
