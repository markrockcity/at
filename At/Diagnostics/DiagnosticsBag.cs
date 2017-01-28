using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public class DiagnosticsBag : IEnumerable<AtDiagnostic>
{
    Lazy<List<AtDiagnostic>> diagnostics = new Lazy<List<AtDiagnostic>>();

    public int Count => diagnostics.IsValueCreated ? diagnostics.Value.Count : 0;

    public void Add(AtDiagnostic diagnostic) 
    {
        lock(diagnostics) 
        {
            diagnostics.Value.Add(diagnostic);    
        }
    }


    ///<summary>Returns true if the bag has any diagnostics with Severity=Error. 
    /// Does not consider warnings or informationals.</summary>
    public bool HasAnyErrors()
    {
        if (Count ==0)
            return false;

        foreach (var diagnostic in diagnostics.Value)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                return true;
            }
        }
 
        return false;
    }

    public AtDiagnostic FirstOrDefault() => Count > 0 ? diagnostics.Value[0] : null;

    public IEnumerator<AtDiagnostic> GetEnumerator() => diagnostics.Value.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
}
