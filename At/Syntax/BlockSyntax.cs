using System;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At 
{
//"{ ... }"
public class BlockSyntax : ExpressionSyntax 
{

    internal BlockSyntax(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics) : 
        base(new AtSyntaxNode[] {startDelimiter}.Concat(contents).Concat(new[] {endDelimiter}), expDef,diagnostics) 
    {
        StartDelimiter = startDelimiter;
        EndDelimiter = endDelimiter;
    }

    public AtToken StartDelimiter {get;}
    public AtToken EndDelimiter {get;}
}
}