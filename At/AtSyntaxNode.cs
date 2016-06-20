using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace At
{
//SyntaxNode + CSharpSyntaxNode + GreenNode
public abstract class AtSyntaxNode
{
    readonly AtSyntaxList<AtSyntaxNode> nodes;
    readonly ImmutableArray<AtDiagnostic> diagnostics;

    protected AtSyntaxNode(IEnumerable<AtSyntaxNode> nodes, IEnumerable<AtDiagnostic> diagnostics) 
    { 
        this.nodes = new AtSyntaxList<AtSyntaxNode>(this,nodes);
        this.diagnostics = ImmutableArray<AtDiagnostic>.Empty;
        
        if (diagnostics != null) 
            this.diagnostics = this.diagnostics.AddRange(diagnostics.Where(_=>_!=null));
    }

        public AtSyntaxNode(AtToken[] atToken)
        {
            this.atTokens = atToken;
        }

        public AtSyntaxNode Parent {get; internal set;}

    public virtual bool IsToken
    {
        get
        {
            return false;
        }
    }

    public virtual int Position
    {
        get
        {
            return nodes[0].Position;
        }
    }

    /// <summary>Includes trivia</summary>
    public virtual string FullText
    {
        get
        {
            if (_text == null)
                _text = string.Concat(nodes.Select(_=>_?.FullText));

            return _text;
        }
    } string _text;

    AtToken[] atTokens;

    public virtual string Text
    {
        get
        {
            return FullText.Trim();
        }
    } 

    public IEnumerable<AtDiagnostic> GetDiagnostics()
    {
        return diagnostics;
    }

    public IEnumerable<AtSyntaxNode> ChildNodes(bool includeTokens = false)
    {
        return nodes.Where(_=>includeTokens || !_.IsToken); 
    }

    public IEnumerable<AtSyntaxNode> DescendantNodes(Func<AtSyntaxNode,bool> filter = null,bool includeTokens = false)
    {
        return nodesRecursive(this,includeTokens,filter);
    }

    public override string ToString()
    {
        return FullText;
    }

    private IEnumerable<AtSyntaxNode> nodesRecursive(AtSyntaxNode parent, bool includeTokens,Func<AtSyntaxNode,bool> filter)
    {
        foreach(var node in parent?.nodes.Where
        (_=>   
               (_!=null) 
            && (includeTokens || !_.IsToken) 
            && (filter==null  || filter(_)))) {

            yield return node;

            foreach(var descendant in nodesRecursive(node,includeTokens, filter))
                yield return descendant;
        }
    }
}
}