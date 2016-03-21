using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace At
{
public class AtSyntaxList<TNode> : IReadOnlyList<TNode> where TNode : AtSyntaxNode
{
    readonly AtSyntaxNode owner;
    readonly ImmutableList<TNode> nodes = ImmutableList<TNode>.Empty;

    internal AtSyntaxList(AtSyntaxNode owner, IEnumerable<TNode> nodes)
    {
        this.owner = owner;

        if (nodes == null)
            return;
        
        foreach(var node in nodes)
        {
            // - this might give a false negative where the parent wasn't changed
            //   but the child nodes have changed (e.g., in another thread)... maybe?
            if (node.Parent == owner)
                continue;

            if (node.Parent != null)
                throw new Exception("Adding nodes that already have parents is not allowed");
            
            node.Parent = owner;
        }

    
        this.nodes = this.nodes.AddRange(nodes);
    }

    public TNode this[int index]
    {
        get
        {
            return nodes[index];
        }
    }

    public int Count
    {
        get
        {
            return nodes.Count;
        }
    }

    public static AtSyntaxList<TNode> Empty
    {
        get
        {
            if (_Empty == null)
                _Empty = new AtSyntaxList<TNode>(null, new TNode[0]);
            return _Empty;
        }
    } static AtSyntaxList<TNode> _Empty = null;

    public IEnumerator<TNode> GetEnumerator()
    {
        return nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return nodes.GetEnumerator();
    }
}
}