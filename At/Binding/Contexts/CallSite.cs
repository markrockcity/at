using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;

namespace At.Contexts
{
/// <summary>Binding context for a method/function call site.</summary>
public class CallSite : Context
{
    protected internal CallSite(Context parentCtx, DiagnosticsBag diagnostics) : base(parentCtx,diagnostics,null)
    {

    }

    /// <summary>e.g., MethodSymbol</summary>
    public Invocation Invocation {get; private set;}

    public IBindingNode Target
    {
        get
        {
            return _target ?? Invocation?.Target;
        }
    } IBindingNode _target;

    public ImmutableArray<IBindingNode> Arguments
    {
        get
        {
            return args.Count > 0 ? args.ToImmutableArray() :  Invocation?.Arguments.Cast<IBindingNode>().ToImmutableArray() ?? ImmutableArray<IBindingNode>.Empty;
        }
    }

    
    public ImmutableArray<TypeArgument> TypeArguments
    {
        get
        {
            return targs.Count > 0 ? targs.ToImmutableArray() :  Invocation?.TypeArguments ?? ImmutableArray<TypeArgument>.Empty;
        }
    }

    public override bool HasContents => Invocation != null;

    public override IEnumerable<IBindingNode> Contents()
    {
        return Invocation != null ? new IBindingNode[]{Invocation} : throw new InvalidOperationException("No Invocation added to context");
    }

    public override string ToString() => $"{GetType().Name}({Invocation})";


    List<IBindingNode> args = new List<IBindingNode>();
    List<TypeArgument> targs = new List<TypeArgument>();

    protected internal override void AddNode(IBindingNode node)
    {
        if (Invocation==null)
        {
            switch(node)
            {
                case Argument arg:  args.Add(arg); return;
                case TypeArgument targ: targs.Add(targ); return;

                case Invocation inv:
                {                
                    Debug.Assert(inv.Context==this);
                    Debug.Assert(_target==null || inv.Target is SymbolReference sr && sr.Symbol==_target);
                    Debug.Assert(invocationContainsAddedArgs(inv));
                    Invocation = inv;
                } return;

                case SymbolReference sr : AddNode(sr.Symbol); return;
                case MethodSymbol method: _target = method; return;
                case KeywordSymbol kw: _target = kw; return;  
                case UndefinedSymbol undef: _target = undef; return;
                
                default: 
                {
                    Debug.Assert(_target != null, $"{node} ({node.GetType()}");
                    args.Add(node);
                } return;
            }        
        }

        throw new InvalidOperationException(node.GetType().ToString());
    }

    private bool invocationContainsAddedArgs(Invocation inv)
    {
        foreach(var addedArg in args)
        foreach(var invArg in inv.Arguments)
        {
            if (addedArg is CallSite cs && cs.Invocation != invArg.Operation)
              return false;
        }

        return true;
    }
}
}
