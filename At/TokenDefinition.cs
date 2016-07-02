using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using static At.TokenKind;
using At;

namespace At
{

public interface ITokenDefinition
{
    //TokenKind TokenKind  {get;}

    /// <summary>Returns the number of characters the token definition matches the input up to 
    /// a given number of characters.</summary>
    int MatchesUpTo(IScanner<char> input, int positionFromStart);
    AtToken Lex(Scanner<char> input);
}


public class TokenDefinition : ITokenDefinition
{
    public readonly static TokenDefinition StartOfFile = new TokenDefinition(TokenKind.StartOfFile,(s,i)=>s.Position<0&&i<1?1:0,s=>new AtSyntaxTrivia(TokenKind.StartOfFile,-1));
    public readonly static TokenDefinition EndOfFile   = new TokenDefinition(TokenKind.EndOfFile,(s,i)=>s.End?int.MaxValue:0,s=>new AtSyntaxTrivia(TokenKind.EndOfFile,s.Position+1));

    public @TokenDefinition(TokenKind tokenKind, Func<IScanner<char>,int,int> matchesUpTo, Func<Scanner<char>,AtToken> lex) 
    {  
        this.matchesUpTo = matchesUpTo;
        this.lex = lex;
        this.TokenKind = tokenKind; 
    }

    private Func<IScanner<char>,int,int> matchesUpTo;
    private Func<Scanner<char>,AtToken> lex;

    public TokenKind TokenKind  {get;}

    public int MatchesUpTo(IScanner<char> input,int positionFromStart)=>matchesUpTo(input,positionFromStart);
    public AtToken Lex(Scanner<char> input) => lex(input);
}

public class TokenDefinitionList : IList<ITokenDefinition>
{
    List<ITokenDefinition> list = new List<ITokenDefinition>();

    public ITokenDefinition this[int index]
    {
        get
        {
            return list[index];
        }

        set
        {
            list[index] = value;
        }
    }

    public int  Count      => list.Count;
    public bool IsReadOnly => false;

    public TokenDefinition Add(string tokenText, TokenKind? kind = null)
    { 
       throw new NotImplementedException();
    }
    public TokenDefinition AddPattern(string pattern, TokenKind? kind = null)
    { 
       throw new NotImplementedException();
    }

    //                        (iscanner, positionFromStart) => maxPositionChecked
    public TokenDefinition Add(Func<IScanner<char>,int,int> f, Func<Scanner<char>,AtToken> lex) 
    { 
       throw new NotImplementedException();
    }
    public TokenDefinition Add(KnownTokenKind kind)
    {
        var tokenDef =   kind == KnownTokenKind.StartOfFile ? TokenDefinition.StartOfFile
                       : kind == KnownTokenKind.EndOfFile   ? TokenDefinition.EndOfFile
                       : null;

        if (tokenDef==null)
            throw new NotImplementedException("token kind = "+kind);

        list.Add(tokenDef);
        return tokenDef;
    }

    public void Add(ITokenDefinition item)
    {
        list.Add(item);
    }

    public void Clear()
    {
       list.Clear();
    }

    public bool Contains(ITokenDefinition item)
    {
        return list.Contains(item);
    }

    public void CopyTo(ITokenDefinition[] array,int arrayIndex)
    {
        list.CopyTo(array,arrayIndex);
    }

    public IEnumerator<ITokenDefinition> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public int IndexOf(ITokenDefinition item)
    {
        return list.IndexOf(item);
    }

    public void Insert(int index,ITokenDefinition item)
    {
        list.Insert(index,item);
    }

    public IList<ITokenDefinition> Matches(IScanner<char> chars, int k)
    {
        return list.Where(_=>_.MatchesUpTo(chars,k)>=k).ToList();
    }

    public bool Remove(ITokenDefinition item)
    {
       return list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.GetEnumerator();
    }
}
}