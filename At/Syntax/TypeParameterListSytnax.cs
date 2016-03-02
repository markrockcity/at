using System;

namespace At.Syntax
{
public class TypeParameterListSytnax : AtSyntaxNode
{
    internal TypeParameterListSytnax() : base(null) {}

    public SeparatedSyntaxList<TypeParameterSyntax> Parameters {get;}
        = new SeparatedSyntaxList<TypeParameterSyntax>();
}
}