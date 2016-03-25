using System;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At 
{
//"{ ... }"
public class CurlyBlockSyntax : ExpressionSyntax 
{

    internal CurlyBlockSyntax(AtToken leftBrace, IEnumerable<ExpressionSyntax> contents, AtToken rightBrace, IEnumerable<AtDiagnostic> diagnostics) : 
        base(new AtSyntaxNode[] {leftBrace}.Concat(contents).Concat(new[] {rightBrace}), diagnostics) 
    {
    }
}
}