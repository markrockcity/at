using System;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At 
{
//"{ ... }"
public class BlockSyntax : ExpressionSyntax 
{

    internal BlockSyntax(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter, IEnumerable<AtDiagnostic> diagnostics) : 
        base(new AtSyntaxNode[] {startDelimiter}.Concat(contents).Concat(new[] {endDelimiter}), diagnostics) 
    {
    }
}
}