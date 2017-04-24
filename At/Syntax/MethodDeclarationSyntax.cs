using System;
using System.Collections.Generic;

namespace At.Syntax
{
public class MethodDeclarationSyntax : DeclarationSyntax, IHasIdentifier
{
    public MethodDeclarationSyntax(AtToken atSymbol,AtToken identifier,ListSyntax<ParameterSyntax> methodParams,NameSyntax returnType,ExpressionSyntax body,IEnumerable<AtSyntaxNode> nodes,IExpressionSource expDef,IEnumerable<AtDiagnostic> diagnostics)
        : base(atSymbol,new NameSyntax(identifier),nodes,expDef,diagnostics)
    {
        Parameters = methodParams;
        Body = body;
        ReturnType = returnType;
        Identifier = identifier;
    }

    public AtToken Identifier  {get;}
    public ExpressionSyntax Body {get;}
    public NameSyntax ReturnType {get;}
    public ListSyntax<ParameterSyntax>  Parameters  {get;}

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitMethodDeclaration(this);
    }
}
}