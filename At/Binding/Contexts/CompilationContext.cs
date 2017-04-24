using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;

namespace At.Contexts
{
//ModuleBuilder?
///<remarks>context where top-level built-in symbols are introduced.</remarks>
public class CompilationContext :  Context
{
    readonly AtCompilation _compilation;
    readonly List<IBindingNode> _nodes = new List<IBindingNode>();

    public @CompilationContext(Context parent, AtCompilation compilation, DiagnosticsBag diagnostics, IEnumerable<ValueTuple<string,Symbol>> builtins = null) : base(parent,diagnostics)
    {
        if (parent==null)
            throw new ArgumentNullException(nameof(parent));

        _compilation = compilation;

        initializeBuiltins(builtins ?? GetDefaultBuiltins(this));
    }

    public AtCompilation Compilation => _compilation;

    public override bool HasContents => _nodes.Any();

    public static IEnumerable<(string,Symbol)> GetDefaultBuiltins(CompilationContext ctx)
    {
        yield return ("output",new KeywordSymbol(ctx,"output"));
        yield return ("input",new KeywordSymbol(ctx,"input"));

        // +
        var op_plus = new OperatorSymbol(ctx,"+",null,null);
        op_plus.EnsureImplementation(TypeSymbol.Number,TypeSymbol.Number,TypeSymbol.Number);
        op_plus.EnsureImplementation(TypeSymbol.String,TypeSymbol.Top,TypeSymbol.String);
        op_plus.EnsureImplementation(TypeSymbol.Top,TypeSymbol.String,TypeSymbol.String);
        yield return ("+",op_plus);
        
        
        // *
        var op_asterisk = new OperatorSymbol(ctx,"*",null,null);
        op_asterisk.EnsureImplementation(TypeSymbol.Number,TypeSymbol.Number,TypeSymbol.Number);
        yield return ("*",op_asterisk);
    }
 
    //public ImmutableArray<AtSyntaxTree> SyntaxTrees => _compilation.SyntaxTrees;

    public override IEnumerable<IBindingNode> Contents()
    {
        return _nodes.ToList();
    }

    protected internal override void AddNode(IBindingNode node)
    {
        lock(_nodes)
        {
            _nodes.Add(node);
        }
    }


    private void initializeBuiltins(IEnumerable<ValueTuple<string,Symbol>> builtins )
    {
        foreach(var builtin in builtins)
            Define(builtin.Item1,builtin.Item2);
    }
}
}
