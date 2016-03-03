using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace At.Syntax
{
public class SeparatedSyntaxList<TNode> : IReadOnlyList<TNode> where TNode : AtSyntaxNode
{
    readonly AtSyntaxList<AtSyntaxNode> _list;

    public SeparatedSyntaxList(AtSyntaxNode owner, IEnumerable<AtSyntaxNode> nodes)
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
        return _list.OfType<TNode>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((SeparatedSyntaxList<TNode>)this).GetEnumerator();
    }
}
}