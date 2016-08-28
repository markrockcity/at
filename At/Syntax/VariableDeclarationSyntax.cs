using System;
using System.Collections.Generic;

namespace At.Syntax
{
public class VariableDeclarationSyntax : DeclarationSyntax, IHasIdentifier
{
    public VariableDeclarationSyntax
    (
        AtToken atSymbol,
        AtToken identifier,
        NameSyntax type,
        IEnumerable<AtSyntaxNode> nodes,
        IExpressionSource expDef,
        IEnumerable<AtDiagnostic> diagnostics) : 

        base(atSymbol,new NameSyntax(identifier),nodes,expDef,diagnostics) {

        Identifier = identifier;
        Type = type;
    }

    public AtToken Identifier
    {
        get;
    }

    public NameSyntax Type {get; }
}
}