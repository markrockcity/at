using System.Collections.Generic;

namespace At.Syntax
{
public class SeparatedSyntaxList<TNode> where TNode : AtSyntaxNode
{
    public IEnumerable<TNode> Nodes() {return null;}
}
}