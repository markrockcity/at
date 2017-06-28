using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using At.Syntax;

namespace At
{
public class AtSyntaxList<TNode> : Limpl.SyntaxList<TNode>, IReadOnlyList<TNode> where TNode : AtSyntaxNode
{
    internal AtSyntaxList(AtSyntaxNode owner, IEnumerable<TNode> nodes) : base(owner, nodes, (ref TNode n, Limpl.ISyntaxNode p) => n.Parent = p)
    {
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

    internal void append(Limpl.IToken token) => Append(token);
}
}