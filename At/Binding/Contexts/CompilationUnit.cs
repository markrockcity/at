using System;
using System.Collections.Immutable;
using At.Syntax;
using At.Binding;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace At.Contexts
{
public sealed class CompilationUnit : Context
{
    readonly CompilationUnitSyntax _syntaxNode;
    readonly AtSyntaxVisitor<IBindingNode> _visitor;
    readonly List<IBindingNode> contents = new List<IBindingNode>();
    
    public CompilationContext Compilation { get; }
        
    //Compiler-Context is top context
    public override Context TopContext => Compilation.TopContext;

    public override bool HasContents => contents.Any();

    internal CompilationUnit(CompilationContext parentCtx, CompilationUnitSyntax syntaxNode, DiagnosticsBag diagnostics) : base(parentCtx,diagnostics,null,syntaxNode)
    {
        Compilation = parentCtx;
        _syntaxNode = syntaxNode;
        _visitor    = new Binder(this);
    }

    public override IEnumerable<IBindingNode> Contents() => contents.ToImmutableList();

    protected internal override void AddNode(IBindingNode node)
    {
        contents.Add(node);

        var dc = node as DeclarationContext;
        var d = dc?.Declaration ?? node as IDeclaration;
        if (d != null && !Symbols().Contains(d.Symbol))
            Define(d.Symbol.Name, d.Symbol);
    }


}
}

