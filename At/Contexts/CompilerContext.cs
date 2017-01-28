using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using At.Symbols;
using At.Binding;

namespace At.Contexts
{
public sealed class CompilerContext : Context
{
    readonly CancellationToken cancellationToken;        

    internal CompilerContext(AtCompiler compiler, AtCompilation compilation,DiagnosticsBag diagnostics,CancellationToken cancellationToken):base(null,diagnostics)
    {
        Compiler = compiler;
        Compilation = new CompilationContext(this,compilation,diagnostics);
        this.cancellationToken = cancellationToken;
    }
    
    public AtCompiler Compiler { get; }

    //"GlobalNamespace"
    public CompilationContext Compilation { get; }
    public override Context TopContext  => this;
    protected override ImmutableArray<IBindingNode> MakeContents() => ImmutableArray.Create<IBindingNode>(Compilation);

    protected internal override void AddNode(IBindingNode node)
    {
        Debug.Assert(node is CompilationContext && Compilation == null); //Context.ctor(parent) { parent.AddNode(this); }
    }
}
}
