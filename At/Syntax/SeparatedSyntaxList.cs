using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace At.Syntax
{
public class SeparatedSyntaxList<TNode> : IReadOnlyList<TNode> where TNode : AtSyntaxNode
{
    internal readonly AtSyntaxList<AtSyntaxNode> _list;

    internal SeparatedSyntaxList(AtSyntaxNode owner, IEnumerable<AtSyntaxNode> nodes)
    {
        this._list = new AtSyntaxList<AtSyntaxNode>(owner,nodes);
    }


    public TNode this[int index]
    {
        get
        {
            return (TNode)((object)this._list[index << 1]);
        }
    }

    public int Count
    {
        get
        {
            return this._list.Count + 1 >> 1;
        }
    }

    public IEnumerator<TNode> GetEnumerator()
    {
        for(int i=0; i < Count; ++i)
            yield return (TNode) this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((SeparatedSyntaxList<TNode>)this).GetEnumerator();
    }

    public override string ToString()
    {
        return string.Join("",_list.Select(_=>_.FullText));
    }
}
}