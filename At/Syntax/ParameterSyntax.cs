using System;
using System.Collections.Generic;

namespace At.Syntax
{
// Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax
public class ParameterSyntax : VariableDeclarationSyntax
{
    AtSyntaxNode identifier;

    internal ParameterSyntax(AtToken identifier, NameSyntax parameterTypeName, IExpressionSource exprSrc,  IEnumerable<AtDiagnostic> diagnostics) : base(null,identifier,parameterTypeName,new[] {identifier},exprSrc,diagnostics)
    {
        this.identifier = identifier;
    }


    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        throw new NotImplementedException();
    }
}
}