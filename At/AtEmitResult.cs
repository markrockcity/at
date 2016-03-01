using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace At 
{
public class AtEmitResult 
{
        private ImmutableArray<Diagnostic> diagnostics;
        bool success = false;

        public AtEmitResult(bool success,ImmutableArray<Diagnostic> diagnostics)
        {
            this.success = success;
            this.diagnostics = diagnostics;
        }

        public bool Success
    {
        get { return this.success; }
    }
}
}