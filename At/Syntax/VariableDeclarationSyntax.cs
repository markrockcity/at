using System.Collections.Generic;

namespace At.Syntax
{
public class VariableDeclarationSyntax : DeclarationSyntax
{
    public VariableDeclarationSyntax(
                                AtToken atSymbol,
                                AtToken identifier,
                                IEnumerable<AtSyntaxNode> nodes,
                                IEnumerable<AtDiagnostic> diagnostics) : 
         base(atSymbol,identifier,nodes,diagnostics)
    {
        
    }
}
}