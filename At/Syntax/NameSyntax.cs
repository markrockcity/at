using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace At.Syntax
{
//TypeSyntax, etc.
public class NameSyntax : ExpressionSyntax
{
    
    internal NameSyntax(AtToken identifier, ListSyntax<NameSyntax> args = null,IExpressionSource expDef = null, IEnumerable<AtDiagnostic> diagnostics = null) :
         base(_nodes(identifier,args),expDef,diagnostics)
    {
        Debug.Assert(identifier!= null);
    
        Identifier = identifier;
        Arguments = args ?? new ListSyntax<NameSyntax>(null,new SeparatedSyntaxList<NameSyntax>(this,null),null,null);
    }

    public AtToken Identifier {get;}

    /// <summary>E.g., type arguments (Foo&lt;Bar>)</summary>
    public ListSyntax<NameSyntax> Arguments {get;}
    
    static IEnumerable<AtSyntaxNode> _nodes(AtSyntaxNode a,AtSyntaxNode b) => (b != null) ?  new [] {a,b} : new [] {a};
}
}