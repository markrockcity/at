using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using At.Binding;

namespace At
{
//Declaration
/// <summary>Language context, e.g., compiler, file, namespace, type, method, parameter-list</summary>
public abstract class Context : IBindingNode
{
    Lazy<ImmutableArray<IBindingNode>> _contents;

    readonly Lazy<ConcurrentDictionary<string,ISymbol>>              _lazy_nameToSymbolMap;
    readonly Lazy<ConcurrentDictionary<UndefinedSymbol,Expression>>  _lazy_undefinedSymbols;
    readonly Lazy<ConcurrentDictionary<Expression,List<UndefinedSymbol>>> _lazy_expressionToUndefinedSymbols;

    protected @Context(Context parentCtx, DiagnosticsBag diagnostics, AtSyntaxNode syntaxNode = null)
    {
        Diagnostics   = diagnostics;
        ParentContext = parentCtx;
        Syntax = syntaxNode;
        Reset();
        _lazy_nameToSymbolMap  = new Lazy<ConcurrentDictionary<string, ISymbol>>();
        _lazy_undefinedSymbols = new Lazy<ConcurrentDictionary<UndefinedSymbol, Expression>>();
        _lazy_expressionToUndefinedSymbols = new Lazy<ConcurrentDictionary<Expression, List<UndefinedSymbol>>>();
        parentCtx?.AddNode(this);
    }

    public DiagnosticsBag Diagnostics {get;}
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
    
    public virtual void Accept(BindingTreeVisitor visitor) => visitor.VisitContext(this);
    public virtual ImmutableArray<IBindingNode> Contents() => _contents.Value;

    public ISymbol LookupSymbol(string name)
    {
        nameToSymbolMap.TryGetValue(name,out ISymbol s);
        if (s == null || s is UndefinedSymbol)   
        { 
            Debug.WriteLine($"{GetType().Name}.Lookup(\"{name}\")=={(object)s ?? "<null>"}");
            return ParentContext?.LookupSymbol(name);
        }

        return s;
    }

    //looks up context member ctx {.foo} -> ctx.foo
    public virtual Symbol LookupContextMember(string text)
    {
        throw new NotImplementedException();
    }

    protected abstract ImmutableArray<IBindingNode> MakeContents();   

    protected internal virtual void AddNode(IBindingNode node)
    {
        throw new NotImplementedException(GetType()+".AddNode("+node.ToString()+")");
    }

    ConcurrentDictionary<string,ISymbol> nameToSymbolMap => _lazy_nameToSymbolMap.Value;
    ConcurrentDictionary<UndefinedSymbol,Expression> undefinedSymbols => _lazy_undefinedSymbols.Value;
    ConcurrentDictionary<Expression,List<UndefinedSymbol>> expressionUndefinedSymbols => _lazy_expressionToUndefinedSymbols.Value;

    protected internal virtual void Define(string key, ISymbol value)
    {
       OnSymbolDefined(this,key,value);
       Reset();
    }

    protected virtual void OnSymbolDefined(Context ctx, string name, ISymbol newSymbol)
    {
        if (this==ctx)
        {
            if (nameToSymbolMap.TryGetValue(name, out ISymbol symbol))
            {
                if (symbol is UndefinedSymbol undefinedSymbol)
                {
                    if (undefinedSymbols.TryGetValue(undefinedSymbol, out Expression oldExpr))
                    {
                        var newExpr = oldExpr.ReplaceSymbol(undefinedSymbol,newSymbol);
                        expressionUndefinedSymbols.TryRemove(oldExpr,out List<UndefinedSymbol> oldList);
                    
                        foreach(var x in oldList)
                            undefinedSymbols[x] = newExpr;

                        expressionUndefinedSymbols.TryAdd(newExpr,oldList);
                        undefinedSymbols.TryRemove(undefinedSymbol,out oldExpr);
                    }

                    nameToSymbolMap.TryRemove(name,out ISymbol removed);
                }
                else
                {
                    Diagnostics.Add(AtDiagnostic.Create(DiagnosticIds.SymbolAlreadyDefined,newSymbol.Syntax,DiagnosticSeverity.Error,string.Format(SR.SymbolAlreadyDefinedF,name)));
                }            
            }        
        
            nameToSymbolMap.TryAdd(name,newSymbol);
        }
   
        foreach(var _ctx in Contents().OfType<Context>())
            _ctx.OnSymbolDefined(ctx,name,newSymbol);
    }
    
    ///<summary>Resets internal contents array. Call this when context contexts change.</summary>
    protected void Reset() => _contents = new Lazy<ImmutableArray<IBindingNode>>(MakeContents);

    internal IEnumerable<KeyValuePair<UndefinedSymbol,Expression>> getUndefinedSymbols()
    {
        if (_lazy_undefinedSymbols.IsValueCreated)
            foreach(var kv in undefinedSymbols.ToArray())
                yield return kv;

        foreach(var kv in Contents().OfType<Context>().SelectMany(_=> _._lazy_undefinedSymbols.IsValueCreated ? (IEnumerable<KeyValuePair<UndefinedSymbol,Expression>>) _.undefinedSymbols : new KeyValuePair<UndefinedSymbol,Expression>[0]))
            yield return kv;
    }

    internal void undefined(UndefinedSymbol undef, Expression bExpr)
    {
        nameToSymbolMap.AddOrUpdate(undef.Name,undef,(s,e)=>e);
        undefinedSymbols.AddOrUpdate(undef,bExpr,(s,e)=>e);

        if (expressionUndefinedSymbols.ContainsKey(bExpr))
            expressionUndefinedSymbols[bExpr].Add(undef);        
        else
            expressionUndefinedSymbols.TryAdd(bExpr,new List<UndefinedSymbol>{ undef });        
    } 

}
}
