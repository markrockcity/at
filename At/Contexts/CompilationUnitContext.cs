using System;
using System.Collections.Immutable;
using At.Syntax;
using At.Binding;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace At.Contexts
{
public sealed class CompilationUnitContext : Context
{
    readonly CompilationUnitSyntax _syntaxNode;
    readonly AtSyntaxVisitor<IBindingNode> _visitor;

    public CompilationContext Compilation { get; }
        
    //Compiler-Context is top context
    public override Context TopContext => Compilation.TopContext;

    internal CompilationUnitContext(CompilationContext parentCtx, CompilationUnitSyntax syntaxNode, DiagnosticsBag diagnostics) : base(parentCtx,diagnostics,syntaxNode)
    {
        Compilation = parentCtx;
        _syntaxNode = syntaxNode;
        _visitor    = new Binder(this);
    }

    protected override ImmutableArray<IBindingNode> MakeContents()
             => _syntaxNode.Expressions.Select(_=>_.Accept(_visitor)).ToImmutableArray();

    protected internal override void AddNode(IBindingNode node)
    {
        Debug.Assert(Contents().Contains(node),$"Compilation unit doesn't contain {node}");
    }
}
}

