using System;
using System.Collections.Generic;
using System.Linq;

namespace At
{
//SyntaxNode + CSharpSyntaxNode + GreenNode
public abstract class AtSyntaxNode
{
    readonly AtSyntaxList<AtSyntaxNode> nodes;

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

    public virtual string FullText
    {
        get
        {
            if (_text == null)
                _text = string.Concat(nodes.Select(_=>_.FullText));

            return _text;
        }
    } string _text;

    public virtual string Text
    {
        get
        {
            return FullText.Trim();
        }
    } 


    public IEnumerable<AtSyntaxNode> Nodes(bool includeTokens = false)
    {
        return nodesRecursive(this,includeTokens);
    }

    public override string ToString()
    {
        return FullText;
    }

    IEnumerable<AtSyntaxNode> nodesRecursive(AtSyntaxNode parent, bool includeTokens)
    {
        foreach(var node in parent.nodes.Where(_=> includeTokens || !_.IsToken))
        {
            yield return node;

            foreach(var descendant in nodesRecursive(node,includeTokens))
                yield return descendant;
        }
    }
}
}