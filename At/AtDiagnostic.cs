using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At
{
/// <summary>E.g., error or warning</summary>
public class AtDiagnostic 
{
    private object diagnosticId;
    private AtToken token;
    private string v;

    public AtDiagnostic(object diagnosticId,AtToken token,string v)
    {
        this.diagnosticId = diagnosticId;
        this.token = token;
        this.v = v;
    }

    internal static AtDiagnostic Create(object expressionCluster,string v1,string v2,object error,int v3,bool v4)
    {
        throw new NotImplementedException();
    }
}
}
