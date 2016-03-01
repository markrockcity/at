using System;
using System.Collections.Generic;
using At.Syntax;

namespace At
{
internal class SyntaxFactory
{
    public static CompilationUnitSyntax CompilationUnit(IEnumerable<ExpressionSyntax> exprs)
    {
           return new CompilationUnitSyntax(exprs);
    }
}
}