using System.Collections.Generic;

namespace At.Syntax
{
public class NamespaceDeclarationSyntax : DeclarationSyntax
{
    internal NamespaceDeclarationSyntax
    (
        AtToken atSymbol, 
        AtToken identifier, 
        IEnumerable<DeclarationSyntax> members,
        IEnumerable<AtSyntaxNode> nodes,
        IEnumerable<AtDiagnostic> diagnostics)

        :base(atSymbol,identifier, nodes,diagnostics) {

        Members = new AtSyntaxList<DeclarationSyntax>(this,members);
    }

    public AtSyntaxList<DeclarationSyntax> Members {get;}
}
}