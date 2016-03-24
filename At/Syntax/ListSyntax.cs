using System;
using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
//non-expression list (e.g., parameters, arguments, etc.)
public class ListSyntax<T> : AtSyntaxNode where T : AtSyntaxNode
{
    private AtToken startDelimiter;
    private AtToken endDelimiter;

    internal ListSyntax(AtToken startDelimiter, SeparatedSyntaxList<T> list, AtToken endDelimiter) : 
    base(new AtSyntaxNode[] {startDelimiter}.Concat(list).Concat(new[] {endDelimiter}))
    {        
        if (list==null)
            throw new ArgumentNullException(nameof(list));
    
        this.startDelimiter = startDelimiter;
        this.endDelimiter = endDelimiter;
        List = list;
    }

    public SeparatedSyntaxList<T> List {get;}

    public override string FullText
    {
        get
        {
            return startDelimiter?.FullText+List+endDelimiter?.FullText;
        }
    }

}
}