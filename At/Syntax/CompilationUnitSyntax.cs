using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace At.Syntax
{
public class CompilationUnitSyntax : ContextSyntax
{
    internal CompilationUnitSyntax(IEnumerable<ExpressionSyntax> exprs, IEnumerable<AtDiagnostic> diagnostics) : base(exprs, diagnostics)
    {
        Expressions = new AtSyntaxList<ExpressionSyntax>(this,exprs);
    }   

    public AtSyntaxList<ExpressionSyntax> Expressions {get;}

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCompilationUnit(this);
    }
}
}