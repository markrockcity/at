using System;

namespace At.Syntax
{
public class TypeParameterListSytnax : AtSyntaxNode
{
    public SeparatedSyntaxList<TypeParameterSyntax> Parameters {get;set;}
}
}