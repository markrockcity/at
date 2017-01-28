using System;
using System.Collections.Generic;

namespace At.Syntax
{
public class DirectiveSyntax : ExpressionSyntax
{
    internal const string importDirective = "#import";


    public DirectiveSyntax
    (
        AtToken   directive, 
        NameSyntax ns,
        IEnumerable<AtSyntaxNode> nodes=null,
        IExpressionSource expDef = null,
        IEnumerable<AtDiagnostic> diagnostics=null) 
        
        : base(nodes??new AtSyntaxNode[] {directive,ns},expDef,diagnostics){    

        this.Directive = directive;
        this.Name = ns;

    }


    public AtToken Directive
    {
        get;
        private set;
    }

    public NameSyntax Name
    {
        get;
        private set;
    }


    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
       return visitor.VisitDirective(this);
    }
}
}