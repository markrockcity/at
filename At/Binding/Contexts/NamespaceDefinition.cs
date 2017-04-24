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
public class NamespaceDefinition : Definition
{
    internal protected NamespaceDefinition(NamespaceSymbol s, Context parentCtx, DiagnosticsBag diagnostics, NamespaceDeclarationSyntax syntaxNode = null) 
    : base(parentCtx,diagnostics,syntaxNode)
    {
        Symbol = s ?? throw new ArgumentNullException(nameof(s));
    }


    public NamespaceSymbol Symbol {get;}

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
