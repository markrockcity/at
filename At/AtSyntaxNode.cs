using System;

namespace At
{
public abstract class AtSyntaxNode
{
    public virtual string Text
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}
}