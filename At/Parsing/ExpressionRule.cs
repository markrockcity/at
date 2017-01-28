using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using At.Syntax;
using static At.SyntaxFactory;
using static At.TokenKind;

namespace At
{

public interface IExpressionRule : IExpressionSource
{
    bool MatchesUpTo(IScanner<AtToken> tokens, int k);
    ExpressionSyntax ParseExpression(Scanner<AtToken> tokens);
}

public interface IExpressionSource 
{
    ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes);
}

public class ExpressionRule : IExpressionRule
{
    Func<IScanner<AtToken>,int,bool> matchesUpTo;
    Func<ExpressionRule,Scanner<AtToken>,ExpressionSyntax> parse;  

    public readonly static ExpressionRule TokenClusterSyntax = SingleTokenExpression(TokenKind.TokenCluster,(ExpressionRule r,AtToken t)=>TokenClusterExpression(tokenCluster:t,expSrc:r));
    public readonly static ExpressionRule NumericLiteral = SingleTokenExpression(TokenKind.NumericLiteral,(rule,token)=>LiteralExpression(token,rule));
    public readonly static ExpressionRule StringLiteral = SingleTokenExpression(TokenKind.StringLiteral,(rule,token)=>LiteralExpression(token,rule));
    public readonly static ExpressionRule Directive = new ExpressionRule((s,k)=>matchDirective(s,k), (rule,s)=>directiveExpression(rule,s));

    /// <summary>Initializes an ExpressionRule object</summary>
    /// <param name="matchesUpTo">A delegate that accepts an IScanner&lt;AtToken> and a character position and returns a boolean saying whether the expression rule matches all characters up to the given look-ahead position.</param>
    /// <param name="parse">A delegate that accepts a ExpressionRule (this object) and a Scanner&lt;AtToken>, returning an ExpressionSyntax object.</param>
    public ExpressionRule(Func<IScanner<AtToken>,int,bool> matchesUpTo, Func<ExpressionRule,Scanner<AtToken>,ExpressionSyntax> parse) 
    {  
        this.matchesUpTo = matchesUpTo;
        this.parse = parse;
    }

    public bool MatchesUpTo(IScanner<AtToken> tokens,int k)=>matchesUpTo(tokens,k);
    public ExpressionSyntax ParseExpression(Scanner<AtToken> input) => parse(this,input);

    static ExpressionRule SingleTokenExpression(TokenKind tk,Func<ExpressionRule,AtToken,ExpressionSyntax> c)
       => new ExpressionRule((s,k)=>k==0 && s.Current?.Kind==tk, (rule,s)=>c(rule,s.Consume()));

    ExpressionSyntax IExpressionSource.CreateExpression(AtSyntaxNode[] nodes)
    {
        var s = new Scanner<AtToken>(nodes.Cast<AtToken>());
        return ParseExpression(s);
    }

    static bool matchDirective(IScanner<AtToken> s,int k)
    {
       return    k==0 && s.Current?.Kind==TokenKind.TokenCluster && s.Current?.Text?[0]=='#'
              || k==1 && s.LookAhead(1)?.Kind==TokenKind.TokenCluster;
    }

    static ExpressionSyntax directiveExpression(ExpressionRule rule,Scanner<AtToken> tokens)
    {
        var nodes = new List<AtSyntaxNode>();
        var directive = tokens.Consume()/*(TokenCluster)*/; 
        nodes.Add(directive);
        var name = NameSyntax(tokens.Consume()); 
        nodes.Add(name);
        return Directive(directive,name,nodes,rule);   
    }
}

public class ExpressionRuleList : ExpressionSourceList<IExpressionRule>
{
    public IList<IExpressionRule> Matches(IScanner<AtToken> tokens, int k)
    {
        return InnerList.Where(_=>_.MatchesUpTo(tokens,k)).ToList();
    }
}


public class ExpressionSourceList<T> : IList<T> where T : IExpressionSource
{
    protected List<T> InnerList {get;} = new List<T>();

    public T this[int index]
    {
        get
        {
            return InnerList[index];
        }
        set
        {
            InnerList[index] = value;
        }
    }

    public int  Count => InnerList.Count;
    public bool IsReadOnly => false;

    public void Add(T item) => InnerList.Add(item);
    public void Clear() =>  InnerList.Clear();
    public bool Contains(T item)=>InnerList.Contains(item);

    public void CopyTo(T[] array,int arrayIndex)
    {
        InnerList.CopyTo(array,arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return InnerList.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return InnerList.IndexOf(item);
    }

    public void Insert(int index,T item)
    {
        InnerList.Insert(index,item);
    }

    public bool Remove(T item)
    {
       return InnerList.Remove(item);
    }

    public void RemoveAt(int index)
    {
        InnerList.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return InnerList.GetEnumerator();
    }

}
}
