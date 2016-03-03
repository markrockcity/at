using System;
using System.Collections.Generic;
using System.Linq;

namespace At
{
//SyntaxNode + CSharpSyntaxNode + GreenNode
public abstract class AtSyntaxNode
{
    internal readonly AtSyntaxList<AtSyntaxNode> nodes;

    protected AtSyntaxNode(IEnumerable<AtSyntaxNode> nodes) 
    { 
        this.nodes = new AtSyntaxList<AtSyntaxNode>(this,nodes);
    }

    public AtSyntaxNode Parent {get; internal set;}

    public virtual bool IsToken
    {
        get
        {
            return false;
        }
    }

    public virtual int Position
    {
        get
        {
            return nodes[0].Position;
        }
    }

    public virtual string Text
    {
        get
        {
            if (_text == null)
                _text = string.Concat(nodes.Select(_=>_.Text));

            return _text;
        }
    } 
    string _text;
}
}