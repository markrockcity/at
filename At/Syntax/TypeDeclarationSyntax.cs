using System.Collections.Generic;

namespace At.Syntax
{
public class TypeDeclarationSyntax: DeclarationSyntax
{

    internal TypeDeclarationSyntax(
                                AtToken atSymbol, 
                                AtToken identifier, 
                                ListSyntax<ParameterSyntax> typeParameterList, 
                                ListSyntax<NameSyntax> baseList,
                                IEnumerable<AtSyntaxNode> nodes) : 
        base(atSymbol, identifier, nodes)
    {
        TypeParameters = typeParameterList;
        BaseTypes = baseList;
    }


    public ListSyntax<NameSyntax> BaseTypes {get;}
    public ListSyntax<ParameterSyntax> TypeParameters {get;}
}
}