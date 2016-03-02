using System;

namespace At
{
public abstract class AtSyntaxNode
{
    protected AtSyntaxNode(string text) { Text = text;}
    public virtual string Text {get;}
}
}