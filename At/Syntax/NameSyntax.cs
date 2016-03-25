using System.Collections.Generic;
using System.Linq;

namespace At.Syntax
{
//TypeSyntax, etc.
public class NameSyntax : ExpressionSyntax
{
    
    internal NameSyntax(AtToken identifier, ListSyntax<NameSyntax> typeArgs = null, IEnumerable<AtDiagnostic> diagnostics = null) :
         base(new AtSyntaxNode[] { identifier}.Concat(typeArgs.List),diagnostics)
    {
        Identifier = identifier;
        TypeArguments = typeArgs;
    }

    public AtToken Identifier {get;}
    public ListSyntax<NameSyntax> TypeArguments {get;}
}
}