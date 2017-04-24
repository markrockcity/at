using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using At.Syntax;
using At.Utilities;

namespace At.Symbols
{
//NamespaceOrTypeSymbol, NamespaceSymbol
public abstract class ContextSymbol : Symbol //, IContextSymbol
{

    protected ContextSymbol(string name, AtSyntaxNode syntaxNode, ContextSymbol parentContext) : base(name,syntaxNode)
    {
        ParentSymbol = parentContext;
    }


    public override Symbol ParentSymbol { get;}

    public Context Context
    {
        get;
        internal set;
    }

    public override TResult Accept<TResult>(Binding.BindingTreeVisitor<TResult> visitor)
    {
        return visitor.VisitContext(this);
    }
}


//MergedNamespaceSymbol
internal class TopContextSymbol : ContextSymbol
{
    private AtCompilation atCompilation;

    class GlobalNamespaceNode : Syntax.NamespaceDeclarationSyntax
    {
        public GlobalNamespaceNode(AtCompilation compilation) : base(null,SyntaxFactory.ParseToken("__top"),compilation.SyntaxTrees.SelectMany(_=>_.GetRoot().ChildNodes().OfType<DeclarationSyntax>()),compilation.SyntaxTrees.SelectMany(_=>_.GetRoot().ChildNodes()),null,null)
        {
        }
    }

    public @TopContextSymbol(AtCompilation declaringCompilation) : base("#top",new GlobalNamespaceNode(declaringCompilation), parentContext:null)
    {
        this.atCompilation = declaringCompilation;
    }

    internal AtCompilation declaringCompilation => atCompilation;

    public override TypeSymbol Type {get; protected internal set;}
}
}
