using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace At.Syntax
{
//TypeSyntax, etc.
public class NameSyntax : ExpressionSyntax
{
    
    internal NameSyntax(AtToken identifier, ListSyntax<NameSyntax> typeArgs = null,IExpressionSource expDef = null, IEnumerable<AtDiagnostic> diagnostics = null) :
         base(_nodes(identifier,typeArgs),expDef,diagnostics)
    {
        Debug.Assert(identifier!= null);
    
        Identifier = identifier;
        TypeArguments = typeArgs;
    }

    public AtToken Identifier {get;}
    public ListSyntax<NameSyntax> TypeArguments {get;}
    
    static IEnumerable<AtSyntaxNode> _nodes(AtSyntaxNode a,AtSyntaxNode b) => (b != null) ?  new [] {a,b} : new [] {a};
}
}