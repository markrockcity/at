using System;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
public class TypeParameterListSyntax : AtSyntaxNode
{
    internal TypeParameterListSyntax(AtToken lessThan, IEnumerable<AtSyntaxNode> list, AtToken greaterThan) : 
        base(new AtSyntaxNode[] {lessThan}.Concat(list).Concat(new[] {greaterThan}))
    {
        Parameters = new SeparatedSyntaxList<TypeParameterSyntax>(this,list);
    }

    public SeparatedSyntaxList<TypeParameterSyntax> Parameters {get;}
}
}