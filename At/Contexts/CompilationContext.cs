using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;

namespace At.Contexts
{
//ModuleBuilder?
public class CompilationContext :  Context
{
    readonly AtCompilation _compilation;
    readonly List<IBindingNode> _nodes = new List<IBindingNode>();

    public @CompilationContext(Context parent, AtCompilation compilation, DiagnosticsBag diagnostics, IEnumerable<KeyValuePair<string,Symbol>> builtins = null) : base(parent,diagnostics)
    {
        if (parent==null)
            throw new ArgumentNullException(nameof(parent));

        _compilation = compilation;

        initializeBuiltins(builtins ?? GetDefaultBuiltins(this));
    }

    public AtCompilation Compilation => _compilation;

    public static IEnumerable<KeyValuePair<string,Symbol>> GetDefaultBuiltins(CompilationContext ctx)
    {
        yield return new KeyValuePair<string,Symbol>("output",new KeywordSymbol(ctx,"output"));
        yield return new KeyValuePair<string,Symbol>("input",new KeywordSymbol(ctx,"input"));
    }
 
    //public ImmutableArray<AtSyntaxTree> SyntaxTrees => _compilation.SyntaxTrees;

    protected override ImmutableArray<IBindingNode> MakeContents()
    {
        return _nodes.ToImmutableArray();
    }

    protected internal override void AddNode(IBindingNode node)
    {
        lock(_nodes)
        {
            Reset();
            _nodes.Add(node);
        }
    }


    private void initializeBuiltins(IEnumerable<KeyValuePair<string,Symbol>> builtins )
    {
        foreach(var builtin in builtins)
            Define(builtin.Key,builtin.Value);
    }
}
}
