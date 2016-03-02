using System.Collections.Generic;

namespace At.Syntax
{
public class SeparatedSyntaxList<TNode> where TNode : AtSyntaxNode
{
    //TODO: implement
    public IEnumerable<TNode> Nodes() 
    {
        return new TNode[0];
    }
}
}