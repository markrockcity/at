using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At
{
public class SyntaxNodeMatchCollection : IReadOnlyList<AtSyntaxNode>
{
    readonly AtSyntaxNode[] nodes;
    readonly Dictionary<string,AtSyntaxNode> d;

    public SyntaxNodeMatchCollection(AtSyntaxNode[] nodes, Dictionary<string,AtSyntaxNode> d)
    {
        this.nodes = nodes;
        this.d = d;
    }

    public int Count
    {
        get
        {
            return ((IReadOnlyCollection<AtSyntaxNode>)this.nodes).Count;
        }
    }

    public AtSyntaxNode this[int index]
    {
        get
        {
            return nodes[index];
        }
    }

    public ExpressionSyntax GetExpression(string key)
    {
        return (ExpressionSyntax) d[key];
    }

    public AtSyntaxNode GetNode(string key)
    {
         return d[key];
    }

    public TNode GetNode<TNode>(string key) where TNode : AtSyntaxNode
    {
        return (TNode) d[key];
    }

   
    IEnumerator<AtSyntaxNode> IEnumerable<AtSyntaxNode>.GetEnumerator()
    {
        return ((IReadOnlyCollection<AtSyntaxNode>)this.nodes).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IReadOnlyCollection<AtSyntaxNode>)this.nodes).GetEnumerator();
    }
}
}
