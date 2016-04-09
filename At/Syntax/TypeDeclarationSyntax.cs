using System;
using System.Collections.Generic;

namespace At.Syntax
{
public class TypeDeclarationSyntax: DeclarationSyntax
{

    internal TypeDeclarationSyntax(
                                AtToken atSymbol, 
                                AtToken identifier, 
                                ListSyntax<ParameterSyntax> typeParameterList, 
                                ListSyntax<NameSyntax> baseList,
                                IEnumerable<DeclarationSyntax> members,
                                IEnumerable<AtSyntaxNode> nodes,
                                IEnumerable<AtDiagnostic> diagnostics) : 
        base(atSymbol, identifier, nodes, diagnostics)
    {
        TypeParameters = typeParameterList;
        BaseTypes = baseList;
        Members = new AtSyntaxList<DeclarationSyntax>(this,members);
    }


    public ListSyntax<NameSyntax> BaseTypes {get;}
    public ListSyntax<ParameterSyntax> TypeParameters {get;}
    public AtSyntaxList<DeclarationSyntax> Members {get;}
}
}