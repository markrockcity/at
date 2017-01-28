using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace At 
{
public class AtEmitResult 
{
    bool success = false;

    public AtEmitResult(bool success,ImmutableArray<AtDiagnostic> diagnostics,IEnumerable<string> convertedSources)
    {
        this.success     = success;
        Diagnostics = diagnostics;
        ConvertedSources = convertedSources.ToList().AsReadOnly();
    }

    public bool Success
    {
        get { return this.success; }
    }

    public ImmutableArray<AtDiagnostic> Diagnostics {get;}

    public ICollection<string>  ConvertedSources {get;}

}
}