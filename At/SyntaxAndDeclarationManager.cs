using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace At
{
internal class SyntaxAndDeclarationManager
{
    internal ImmutableArray<AtSyntaxTree> syntaxTrees;

    public SyntaxAndDeclarationManager(ImmutableArray<AtSyntaxTree> trees)
    {
        this.syntaxTrees = trees;
    }

    public SyntaxAndDeclarationManager AddSyntaxTrees(ImmutableArray<AtSyntaxTree> trees)
    {
        return new SyntaxAndDeclarationManager(trees);
    }
}
}