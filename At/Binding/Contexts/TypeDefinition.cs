using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;
using At.Syntax;

namespace At.Contexts
{
//"MergedDeclaration"
public class TypeDefinition : Definition
{
    internal protected TypeDefinition(TypeSymbol symbol,Context parentCtx, DiagnosticsBag diagnostics, TypeDeclarationSyntax syntaxNode = null) : base(parentCtx,diagnostics,syntaxNode)
    {
       Symbol = symbol;

    }


    public TypeSymbol Symbol
    {
        get;
        private set;
    }

    protected internal override ContextSymbol ContextSymbol => Symbol;


    public override bool HasContents => _contents.Any();
    

    public override IEnumerable<IBindingNode> Contents() => _contents.ToImmutableList();


    List<IBindingNode> _contents = new List<IBindingNode>();

    protected internal override void AddNode(IBindingNode node)
    {
        _contents.Add(node);
    }
}
}
