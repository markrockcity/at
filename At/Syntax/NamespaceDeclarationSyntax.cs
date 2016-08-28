using System;
using System.Collections.Generic;

namespace At.Syntax
{
public class NamespaceDeclarationSyntax : DeclarationSyntax, IHasIdentifier
{
    internal NamespaceDeclarationSyntax
    (
        AtToken atSymbol, 
        AtToken identifier, 
        IEnumerable<DeclarationSyntax> members,
        IEnumerable<AtSyntaxNode> nodes,
        IExpressionSource expDef,
        IEnumerable<AtDiagnostic> diagnostics)

        :base(atSymbol,new NameSyntax(identifier), nodes, expDef,diagnostics) {

        Identifier = identifier;
        Members = new AtSyntaxList<DeclarationSyntax>(this,members);
    }

    public AtToken Identifier
    {
        get;
    }

    public AtSyntaxList<DeclarationSyntax> Members {get;}
}
}