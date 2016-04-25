using System.Collections.Generic;

namespace At.Syntax
{
public class MethodDeclarationSyntax:DeclarationSyntax
{
    ListSyntax<ParameterSyntax> methodParams;

    public MethodDeclarationSyntax(AtToken atSymbol,AtToken identifier,ListSyntax<ParameterSyntax> methodParams,NameSyntax returnType,List<AtSyntaxNode> nodes,IEnumerable<AtDiagnostic> diagnostics)
        : base(atSymbol,identifier,nodes,diagnostics)
    {
        this.methodParams = methodParams;
        this.ReturnType = returnType;
    }

    public NameSyntax ReturnType {get;}
}
}