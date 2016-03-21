using System;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
public class TypeParameterListSyntax : AtSyntaxNode
{
        private AtToken lessThan;
        private AtToken greaterThen;

        internal TypeParameterListSyntax(AtToken lessThan, SeparatedSyntaxList<TypeParameterSyntax> list, AtToken greaterThan) : 
        base(new AtSyntaxNode[] {lessThan}.Concat(list).Concat(new[] {greaterThan}))
    {        
        if (list==null)
            throw new ArgumentNullException(nameof(list));
    
        this.lessThan = lessThan;
        this.greaterThen = greaterThan;
        Parameters = list;
    }

    public SeparatedSyntaxList<TypeParameterSyntax> Parameters {get;}

    public override string FullText
    {
        get
        {
            return lessThan.FullText+Parameters+greaterThen.FullText;
        }
    }
}
}