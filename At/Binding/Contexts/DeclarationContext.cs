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
/// <summary>For when a declaration has an accompanying definition</summary>
public class DeclarationContext : Context
{
    public DeclarationContext(Context parentCtx,  DeclarationSyntax syntaxNode = null, Declaration declaration = null, Definition definition = null, DiagnosticsBag diagnostics=null) : base(parentCtx,diagnostics,null,syntaxNode)
    {
        Declaration = declaration;
        Definition  = definition;
    }

    public Symbol      DeclaredSymbol => Declaration?.DeclaredSymbol;
    public Declaration Declaration {get; private set;}
    public Definition  Definition  {get; private set;}
    public DeclarationSyntax DeclarationSyntax => (DeclarationSyntax) Syntax;

    protected internal override ContextSymbol ContextSymbol => DeclaredSymbol is ContextSymbol cs ? cs : null;

    public override bool HasContents => (Declaration != null) || (Definition != null);

    public override IEnumerable<IBindingNode> Contents() => _contents().ToImmutableList();
    IEnumerable<IBindingNode> _contents()
    {
        if (Declaration != null)
            yield return Declaration;

        if (Definition != null)
            yield return Definition;
    }

    protected internal override void AddNode(IBindingNode node)
    {
        //Declaration
        if (node is Declaration decl)
        {
            if (Definition != null && Definition.ContextSymbol != decl.DeclaredSymbol)
                throw new InvalidOperationException($"Expected declaration for {decl.DeclaredSymbol} but got {Definition.ContextSymbol}");
        
            if (Declaration == null)
            {
                Declaration = decl;
                ParentContext?.Define(decl.DeclaredSymbol.Name,decl.DeclaredSymbol);
            }
            else
                throw new InvalidOperationException("Declaration is already assigned.");
        }

        //Defintion
        else if (node is Definition def)
        {
            if (Declaration != null && def.ContextSymbol != DeclaredSymbol)
                throw new InvalidOperationException($"Expected definition for {DeclaredSymbol} but got {def.ContextSymbol}");

            if (Definition == null)
                Definition = def;
            else
                throw new InvalidOperationException("Definition is already assigned.");
        }
        else
            throw new InvalidOperationException($"Expected declaration or definition but instead got {node}.");
            
    }

    public override string ToString() 
    {
        var list = new List<object>(2);
        if (Declaration != null)
            list.Add(Declaration);
        if (Definition != null)
            list.Add(Definition);
        return $"{GetType().Name}[{string.Join(",",list)}]";                
    }
        
}
}
