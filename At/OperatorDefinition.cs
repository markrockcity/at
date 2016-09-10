using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using At.Syntax;
using static At.SyntaxFactory;

namespace At
{

public enum OperatorPosition
{    
    End, // [" ... "] ;
    Start, //StartPrefix: @[" ... "]
    //StartInfix: x : ["   ... "] //
    Prefix, // @x
    Postfix,// x++
    Infix, // x : y
    Circumfix,
    PostCircumfix
}

public enum OperatorAssociativity
{
    Unspecified = 0,
    Left,
    Right,
    Chain,
    List
}

/*
public interface IExpressionDefinition : IExpressionSource
{
       
}
*/

public interface IOperatorDefinition : IExpressionSource
{
    OperatorAssociativity Associativity {get;}
    OperatorPosition OperatorPosition {get;}
    TokenKind TokenKind {get;}
}

///<summary>implemented by [post]circumfix operator with a different end delimiter than start delimiter</summary>
///<remarks>IOperatorDefinition.TokenKind is the start delimiter</remarks>
public interface ICircumfixOperator : IOperatorDefinition
{
    TokenKind EndDelimiterKind {get;}
}

public class BlockSyntaxDefinition : OperatorDefinition, ICircumfixOperator
{
    public BlockSyntaxDefinition
    (
        TokenKind startDelimiterKind, 
        TokenKind endDelimiterKind, 
        Func<OperatorDefinition, AtSyntaxNode[],ExpressionSyntax> createExpression,
        bool isPostCircumfix = false)

        : base(startDelimiterKind,isPostCircumfix ? OperatorPosition.PostCircumfix : OperatorPosition.Circumfix,OperatorAssociativity.Unspecified,createExpression) {
        
        EndDelimiterKind = endDelimiterKind;   
    }
    
    public TokenKind EndDelimiterKind {get;}
}



public class OperatorDefinition : IOperatorDefinition
{
    
    public readonly static DeclaratorDefinition  StartDeclaration  = new DeclaratorDefinition(TokenKind.AtSymbol,OperatorPosition.Start);
    public readonly static OperatorDefinition    ColonPair = new OperatorDefinition(TokenKind.Colon,OperatorPosition.Infix,OperatorAssociativity.List,Binary);
    public readonly static DeclaratorDefinition  PrefixDeclaration = new DeclaratorDefinition(TokenKind.AtSymbol,OperatorPosition.Prefix);
    public readonly static BlockSyntaxDefinition RoundBlock = new BlockSyntaxDefinition(TokenKind.OpenParenthesis,TokenKind.CloseParenthesis,SyntaxFactory.RoundBlock);
    public readonly static BlockSyntaxDefinition PostRoundBlock = new BlockSyntaxDefinition(TokenKind.OpenParenthesis,TokenKind.CloseParenthesis,(src,nodes)=>PostBlock(src,nodes[0],RoundBlock(src,nodes.Skip(1).ToArray())),isPostCircumfix:true);
    public readonly static BlockSyntaxDefinition PointyBlock = new BlockSyntaxDefinition(TokenKind.LessThan,TokenKind.GreaterThan,SyntaxFactory.PointyBlock);
    public readonly static BlockSyntaxDefinition PostPointyBlock = new BlockSyntaxDefinition(TokenKind.LessThan,TokenKind.GreaterThan,(src,nodes)=>PostBlock(src,nodes[0],PointyBlock(src,nodes.Skip(1).ToArray())),isPostCircumfix:true);
    public readonly static OperatorDefinition    SemiColon = new OperatorDefinition(TokenKind.SemiColon,OperatorPosition.End,(src,nodes)=>nodes.Length>1?((ExpressionSyntax)nodes[0]).WithEndToken((AtToken)nodes[1]):Empty(src,nodes[0]));
    public readonly static OperatorDefinition    Comma = new OperatorDefinition(TokenKind.Comma,OperatorPosition.Infix,OperatorAssociativity.List,Binary);
   
     readonly Func<OperatorDefinition,AtSyntaxNode[],ExpressionSyntax> createExpression;
        
    public OperatorDefinition(TokenKind tokenKind, OperatorPosition opPosition, Func<OperatorDefinition, AtSyntaxNode[],ExpressionSyntax> createExpression = null)
        : this(tokenKind,opPosition,OperatorAssociativity.Unspecified,createExpression) {}
    public OperatorDefinition(TokenKind tokenKind, OperatorPosition opPosition, OperatorAssociativity associativity, Func<OperatorDefinition, AtSyntaxNode[], ExpressionSyntax> createExpression = null)
    {
        TokenKind = tokenKind;
        OperatorPosition  = opPosition;
        Associativity = associativity;
        this.createExpression = createExpression;
    }

    public TokenKind TokenKind {get;}
    public OperatorAssociativity Associativity {get;}
    public OperatorPosition OperatorPosition {get;}

    public ExpressionSyntax CreateExpression(IEnumerable<AtSyntaxNode> nodes) => CreateExpression(nodes.ToArray());
    public ExpressionSyntax CreateExpression(AtSyntaxNode[] nodes) => createExpression?.Invoke(this,nodes);
}

public class OperatorDefinitionList : ICollection<IList<IOperatorDefinition>>
{
    SortedDictionary<int,IList<IOperatorDefinition>> dict = new SortedDictionary<int,IList<IOperatorDefinition>>();

    public IList<IOperatorDefinition> this[int prescedence] => dict[prescedence];
    public int  Count => dict.Count;

    public void Add(int prescedence, IOperatorDefinition operatorDefinition)
    {
        if (operatorDefinition==null)
            throw new ArgumentNullException(nameof(operatorDefinition));       
    
        if(!dict.ContainsKey(prescedence))
            dict.Add(prescedence,new List<IOperatorDefinition>());

        dict[prescedence].Add(operatorDefinition);
    }

    public void Clear()=>dict.Clear();
    public int  Prescedence(IOperatorDefinition op)=>dict.First(_=>_.Value.Contains(op)).Key;
    public IOperatorDefinition FirstOrDefault(Func<IOperatorDefinition,bool> p = null)
    {
      return  p == null
                ? dict.Values.SelectMany(_=>_).FirstOrDefault() 
                : dict.Values.SelectMany(_=>_).FirstOrDefault(p);
    }
    public IEnumerable<IOperatorDefinition> Where(Func<IOperatorDefinition,bool> p) => dict.Values.SelectMany(_=>_).Where(p);

    bool ICollection<IList<IOperatorDefinition>>.IsReadOnly => false;
    void ICollection<IList<IOperatorDefinition>>.Add(IList<IOperatorDefinition> item)=>dict.Add(0,item);
    bool ICollection<IList<IOperatorDefinition>>.Contains(IList<IOperatorDefinition> item) => dict.Values.Contains(item);
    
    void ICollection<IList<IOperatorDefinition>>.CopyTo(IList<IOperatorDefinition>[] array,int arrayIndex)
    {
        var i = arrayIndex;
        foreach(var v in dict.Values)
            array[i] = v;
    }

    bool ICollection<IList<IOperatorDefinition>>.Remove(IList<IOperatorDefinition> item)
    {
        var keyToRemove = (int?) null;
        foreach(var kv in dict)
            if (kv.Value.Equals(item))
            {
                keyToRemove = kv.Key;
                break;
            }
        if (keyToRemove != null)
            dict.Remove((int) keyToRemove);
        return (keyToRemove != null);
    }

    IEnumerator<IList<IOperatorDefinition>> IEnumerable<IList<IOperatorDefinition>>.GetEnumerator() => dict.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => dict.Values.GetEnumerator();
}
}
