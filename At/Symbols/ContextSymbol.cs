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
    /*
public interface IContextSymbol : ISymbol
{
    bool IsTopContext {get;}

    /// <param name="name">name of child symbol or <c>null</c> for all</param>
    IEnumerable<ISymbol> ChildSymbols(string name = null);

}*/

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

        /*

        public virtual ImmutableArray<Symbol> ChildSymbols(string name = null)
        {
            if (_childSymbols==null)
            {
                ImmutableInterlocked.InterlockedInitialize(ref _childSymbols,makeChildSymbols());            
            }

            return (name != null) 
                        ? _childSymbols.Where(_=>_.Name==name).ToImmutableArray()
                        : _childSymbols;
        }
        //IEnumerable<ISymbol> IContextSymbol.ChildSymbols(string name) => ChildSymbols(name).Cast<ISymbol>();

        private ImmutableArray<Symbol> makeChildSymbols()
        {
           throw new NotImplementedException();

            //var builder = new ContextBuilder(this,syntaxNode);            
            var list    = new List<Symbol>();

            //foreach(var node in syntaxNode.ChildNodes())
            //    list.Add(builder.Visit(node));        

            if (_nameToChildrenMap == null)
                    Interlocked.CompareExchange(ref _nameToChildrenMap, makeNameToChildrenMap(list), null);

            return list.ToImmutableArray();
        }

        private Dictionary<string,ImmutableArray<Symbol>> makeNameToChildrenMap(List<Symbol> children)
        {
            var builder = new NameToSymbolMapBuilder(children.Count);

            foreach (var child in children)
                builder.Add(child);

            return builder.CreateMap();
        }

            / *
        public override void Accept(SymbolVisitor visitor)
        {
           visitor.VisitContext(this);
        }*/

        public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
    {
        return visitor.VisitContext(this);
    }

        /*
    public override TResult Accept<TResult, TArgument>(SymbolVisitor<TResult,TArgument> visitor,TArgument argument)
    {
        return visitor.VisitContext(this,argument);
    }*/

    //@ContextSymbol.NameToSymbolMapBuilder<>
    /* private struct NameToSymbolMapBuilder
    {
        private readonly Dictionary<string, object> _dictionary;



        public NameToSymbolMapBuilder(int capacity)
        {
            _dictionary = new Dictionary<string, object>(capacity, StringOrdinalComparer.Instance);
        }

       
        public void Add(Symbol symbol)
        {
            string name = symbol.Name;
            if (_dictionary.TryGetValue(name, out object item))
            {
                var list = item as List<Symbol>;
                if (list == null)
                {
                    list = new List<Symbol>();
                    list.Add((Symbol)item);
                    _dictionary[name] = list;
                }
                list.Add(symbol);
            }
            else
            {
                _dictionary[name] = symbol;
            }
        }

        public Dictionary<String, ImmutableArray<Symbol>> CreateMap()
        {
            if (_dictionary==null)
                throw new InvalidOperationException("use constructor NameToSymbolMapBuilder(int capacity)");

            var result = new Dictionary<String, ImmutableArray<Symbol>>(_dictionary.Count, StringOrdinalComparer.Instance);

            foreach (var kv in _dictionary)
            {
                object value = kv.Value;
                ImmutableArray<Symbol> children;

                var list = value as List<Symbol>;
                if (list != null)
                {
                    Debug.Assert(list.Count > 1);
                    children = list.ToImmutableArray();
                }
                else
                {
                    Symbol symbol = (Symbol)value;
                    children = ImmutableArray.Create(symbol);
                }

                result.Add(kv.Key, children);
            }

            return result;
        }*/
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
}
}
