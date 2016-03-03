using System.Collections.Generic;

namespace At.Syntax
{
public abstract class DeclarationSyntax : ExpressionSyntax
{
    protected DeclarationSyntax(AtToken atSymbol, AtToken identifier, IEnumerable<AtSyntaxNode> nodes) : 
        base(nodes)
    {
        this.Identifier = identifier;
    }

    public AtToken Identifier {get;}
}
}