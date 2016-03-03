using System.Collections.Generic;

namespace At.Syntax
{
public class ClassDeclarationSyntax: DeclarationSyntax
{

    internal ClassDeclarationSyntax(AtToken atSymbol, AtToken identifier, TypeParameterListSytnax typeParameterList, IEnumerable<AtSyntaxNode> nodes) : 
        base(atSymbol, identifier, nodes)
    {
        TypeParameterList = typeParameterList;
    }


    public string BaseClass {get;}
    public TypeParameterListSytnax TypeParameterList {get;}
}
}