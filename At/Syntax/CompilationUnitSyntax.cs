using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace At.Syntax
{
public class CompilationUnitSyntax : AtSyntaxNode
{
    internal CompilationUnitSyntax(IEnumerable<ExpressionSyntax> exprs) : base(exprs)
    {
        
    }   
}
}