using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace At 
{
public class AtEmitResult 
{
    bool success = false;

    public AtEmitResult(bool success,ImmutableArray<Diagnostic> diagnostics)
    {
        this.success     = success;
        this.Diagnostics = diagnostics;
    }

    public bool Success
    {
        get { return this.success; }
    }

    public ImmutableArray<Diagnostic> Diagnostics {get;}

}
}