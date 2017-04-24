using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using At.Binding;
using At.Contexts;
using At.Symbols;

namespace At
{
//Declaration
/// <summary>Language context, e.g., compiler, file, namespace, type, method, parameter-list</summary>
public abstract class Context : IBindingNode
{
    readonly Lazy<ConcurrentDictionary<string,ISymbol>>     _lazy_nameToSymbolMap;
    readonly Lazy<ConcurrentDictionary<ISymbol,Operation>>  _lazy_undefinedSymbols;
    readonly Lazy<ConcurrentDictionary<Operation,List<ISymbol>>> _lazy_expressionToUndefinedSymbols;

    protected @Context(Context parentCtx, DiagnosticsBag diagnostics, ContextSymbol symbol = null, AtSyntaxNode syntaxNode = null)
    {
        Diagnostics   = diagnostics;
        ParentContext = parentCtx;
        Syntax = syntaxNode;
        _lazy_nameToSymbolMap  = new Lazy<ConcurrentDictionary<string, ISymbol>>();
        _lazy_undefinedSymbols = new Lazy<ConcurrentDictionary<ISymbol, Operation>>();
        _lazy_expressionToUndefinedSymbols = new Lazy<ConcurrentDictionary<Operation, List<ISymbol>>>();
        
        if (!(this is IDefinition) || parentCtx is DeclarationContext) 
            parentCtx?.AddNode(this);
    }

    public DiagnosticsBag Diagnostics {get;}

    public abstract bool HasContents {get;}

    public Context ParentContext {get;}
    public AtSyntaxNode Syntax {get;}

    public virtual Context TopContext
    {
        get
        {
            Context topCtx(Context ctx) => ctx.ParentContext == null ? ctx : topCtx(ctx.ParentContext);
            return topCtx(this);
        }
    }

    protected internal virtual ContextSymbol ContextSymbol {get;}

    public virtual T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitContext(this);
    public abstract IEnumerable<IBindingNode> Contents();

    public ISymbol LookupSymbol(string name, bool checkParentContext = true)
    {
        nameToSymbolMap.TryGetValue(name,out ISymbol s);
        if ((s == null || s is UndefinedSymbol) && checkParentContext)   
        { 
            Debug.WriteLine($"{GetType().Name}.Lookup(\"{name}\")=={(object)s ?? "<null>"}");
            return ParentContext?.LookupSymbol(name);
        }

        return s;
    }

    //looks up context member ctx {.foo} -> ctx.foo
    public virtual Symbol LookupContextMember(string text)
    {
        throw new NotImplementedException($"{GetType()}.LookupContextMember({text})");
    }

    public override string ToString() => GetType().Name;

    protected internal virtual void AddNode(IBindingNode node)
    {
        throw new NotImplementedException(GetType()+".AddNode("+node.ToString()+")");
    }

    ConcurrentDictionary<string,ISymbol> nameToSymbolMap => _lazy_nameToSymbolMap.Value;
    ConcurrentDictionary<ISymbol,Operation> undefinedSymbols_boundOperation => _lazy_undefinedSymbols.Value;
    ConcurrentDictionary<Operation,List<ISymbol>> boundOperation_UndefinedSymbols => _lazy_expressionToUndefinedSymbols.Value;

 
    /// <remarks>Does not call OnSymbolDefined()</remarks>
    protected internal void Define(ISymbol s)
    {
        Debug.WriteLine($"{this}.Define({s.Name} = {s})");
        nameToSymbolMap[s.Name] = s;
    }

    /// <remarks>Calls OnSymbolDefined(), which is virtual and 
    /// therefore not suitable to be called from a constructor.</remarks>
    protected internal void Define(string key, ISymbol value)
    {
        Debug.WriteLine($"{this}.Define({key} = {value})");
        OnSymbolDefined(this,key,value);
    }

    protected virtual void OnSymbolDefined(Context ctx, string name, ISymbol newSymbol)
    {
        if (this==ctx)
        {
            if (nameToSymbolMap.TryGetValue(name, out ISymbol symbol))
            {
                if (symbol.IsUndefined)
                {
                    if (undefinedSymbols_boundOperation.TryGetValue(symbol, out Operation oldOp))
                    {
                        var newOp = oldOp.ReplaceSymbol(symbol,newSymbol);
                        boundOperation_UndefinedSymbols.TryRemove(oldOp,out List<ISymbol> oldList);
                    
                        foreach(var x in oldList)
                            undefinedSymbols_boundOperation[x] = newOp;

                        boundOperation_UndefinedSymbols.TryAdd(newOp,oldList);
                        undefinedSymbols_boundOperation.TryRemove(symbol,out oldOp);
                    }

                    nameToSymbolMap.TryRemove(name,out ISymbol removed);
                }
                else
                {
                    Debug.WriteLine($"\"{name}\" already defined in {ctx}");
                    Diagnostics.Add(AtDiagnostic.Create(DiagnosticIds.SymbolAlreadyDefined,newSymbol.Syntax,DiagnosticSeverity.Error,string.Format(SR.SymbolAlreadyDefinedF,name)));
                }            
            }        
        
            nameToSymbolMap.TryAdd(name,newSymbol);
        }
   
        foreach(var childCtx in Contents().OfType<Context>())
            childCtx.OnSymbolDefined(ctx,name,newSymbol);
    }


    /// <summary>Defined symbols</summary>
    protected virtual IEnumerable<ISymbol> Symbols() => nameToSymbolMap.Values.Where(_=>!(_ is UndefinedSymbol)).Distinct();

    internal IEnumerable<(ISymbol symbol,Operation op)> getUndefinedSymbols()
    {
        if (_lazy_undefinedSymbols.IsValueCreated)
            foreach(var kv in undefinedSymbols_boundOperation.Select(_=>(_.Key,_.Value)).ToArray())
                yield return kv;

        foreach(var kv in Contents().OfType<Context>().SelectMany(_=> _.getUndefinedSymbols()))
            yield return kv;
    }

    internal void registerUndefinedSymbol(UndefinedSymbol undef, Operation boundOperation)
    {
        nameToSymbolMap.AddOrUpdate(undef.Name,undef,(s,e)=>e);
        undefinedSymbols_boundOperation.AddOrUpdate(undef,boundOperation,(s,e)=>e);

        if (boundOperation_UndefinedSymbols.ContainsKey(boundOperation))
            boundOperation_UndefinedSymbols[boundOperation].Add(undef);        
        else
            boundOperation_UndefinedSymbols.TryAdd(boundOperation,new List<ISymbol>{ undef });
            
        ParentContext?.registerUndefinedSymbol(undef,boundOperation);
    } 

}
}
