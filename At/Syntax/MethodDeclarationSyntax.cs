using System;
using System.Collections.Generic;

namespace At.Syntax
{
public class MethodDeclarationSyntax : DeclarationSyntax, IHasIdentifier
{
    ListSyntax<ParameterSyntax> methodParams;

    public MethodDeclarationSyntax(AtToken atSymbol,AtToken identifier,ListSyntax<ParameterSyntax> methodParams,NameSyntax returnType,IEnumerable<AtSyntaxNode> nodes,IExpressionSource expDef,IEnumerable<AtDiagnostic> diagnostics)
        : base(atSymbol,new NameSyntax(identifier),nodes,expDef,diagnostics)
    {
        this.methodParams = methodParams;
        ReturnType = returnType;
        Identifier = identifier;
    }

    public AtToken Identifier
    {
        get;
    }

    public NameSyntax ReturnType {get;}

    /*
    public override void Accept(AtSyntaxVisitor visitor)
    {
        throw new NotImplementedException();
    }
    */

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitMethodDeclaration(this);
    }
}
}