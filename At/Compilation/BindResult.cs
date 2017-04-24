using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Contexts;

namespace At
{
/// <summary>Represents the result of a binding</summary>
public class BindResult
{
    //readonly Lazy<ImmutableArray<AtSyntaxTree>> _lazy_syntaxTrees;

    public BindResult(CompilationContext context)
    {
        //_lazy_syntaxTrees = new Lazy<ImmutableArray<AtSyntaxTree>>(makeSyntaxTrees);
        Compilation = context.Compilation;
        Context     = context;
        Diagnostics = context.Diagnostics.ToImmutableArray();
        Success     = !context.Diagnostics.HasAnyErrors();
    }

    //public string AssemblyName => Compilation.assemblyName;
    public AtCompilation Compilation {get;}
    public CompilationContext Context {get;}
    public ImmutableArray<AtDiagnostic> Diagnostics {get;}        
    public bool Success {get;}

    /*
    public ImmutableArray<AtSyntaxTree> SyntaxTrees {get;}

    private ImmutableArray<AtSyntaxTree> makeSyntaxTrees()
    {
        var list = new List<AtSyntaxTree>();

        foreach(CompilationUnitContext ctx in Context.Contents())
            list.Add(ctx.Sy

        return list.ToImmutableArray();
    }*/
}
}
