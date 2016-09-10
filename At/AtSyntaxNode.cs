using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace At
{
//SyntaxNode + CSharpSyntaxNode + GreenNode
public abstract class AtSyntaxNode
{
    internal readonly AtSyntaxList<AtSyntaxNode> nodes;
    readonly ImmutableArray<AtDiagnostic> diagnostics;

    protected AtSyntaxNode(IEnumerable<AtSyntaxNode> nodes, IEnumerable<AtDiagnostic> diagnostics, bool isMissing = false) 
    { 
        this.nodes = new AtSyntaxList<AtSyntaxNode>(this,nodes);
        this.diagnostics = ImmutableArray<AtDiagnostic>.Empty;
        
        if (diagnostics != null) 
            this.diagnostics = this.diagnostics.AddRange(diagnostics.Where(_=>_!=null));
    
        IsMissing = isMissing;
    }

    public AtSyntaxNode   Parent {get; internal set;}
    public virtual bool   IsTrivia => false;
    public virtual bool   IsToken  => false;
    public virtual int    Position => nodes[0].Position;
    public virtual string Text => FullText.Trim();

    /// <summary>True if absent from source.</summary>
    public bool IsMissing {get; internal set;}

    /// <summary>Includes trivia</summary>
    public virtual string FullText
    {
        get
        {
            if (_text == null)
                _text = string.Concat(nodes.Select(_=>_?.FullText));

            return _text;
        }
    } internal string _text;

    public AtToken AsToken() => this as AtToken;
    public virtual AtSyntaxNode Clone() => (AtSyntaxNode) MemberwiseClone();
    public override string ToString() => FullText;
    public IEnumerable<AtDiagnostic> GetDiagnostics() => diagnostics;
    public IReadOnlyList<AtSyntaxNode> ChildNodes(bool includeTokens = false) => nodes.Where(_=>includeTokens || !_.IsToken).ToImmutableList(); 
    public IEnumerable<AtSyntaxNode> DescendantNodes(Func<AtSyntaxNode,bool> filter = null,bool includeTokens = false) => nodesRecursive(this,includeTokens,filter);

    public static IEnumerable<string> GetPatternStrings(IReadOnlyList<AtSyntaxNode> nodes)
        => getPatternStringsRecursive(0,nodes);

    /// <summary>(...from most specific)</summary>
    public virtual IEnumerable<string> PatternStrings()
    {
        yield return "Node";
    }

    public virtual string PatternName()
    {
        var t = GetType();
        return (t.Assembly==typeof(AtSyntaxNode).Assembly)
                ? t.Name
                : t.FullName;
    }

    static IEnumerable<string> getPatternStringsRecursive(int index, IReadOnlyList<AtSyntaxNode> nodes)
    {
        if (nodes.Count <= index)
            yield break;

        foreach(var x in nodes[index].PatternStrings())
        {
            var ys = getPatternStringsRecursive(index+1,nodes);

            if (ys.Any())
                foreach(var y in ys)
                    yield return $"{x},{y}";
            else
                yield return x;           
        }
    }

    static IEnumerable<AtSyntaxNode> nodesRecursive(AtSyntaxNode parent, bool includeTokens,Func<AtSyntaxNode,bool> predicate)
    {
        foreach(var node in parent?.nodes.Where
        (ifNode=>  
               (ifNode!=null)
            && (includeTokens   || !ifNode.IsToken) 
            && (predicate==null || predicate(ifNode)))) {

                yield return node;

                foreach(var descendant in nodesRecursive(node,includeTokens, predicate))
                    yield return descendant;
        }
    }
}
}