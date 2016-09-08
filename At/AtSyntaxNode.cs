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

    IEnumerable<AtSyntaxNode> nodesRecursive(AtSyntaxNode parent, bool includeTokens,Func<AtSyntaxNode,bool> predicate)
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