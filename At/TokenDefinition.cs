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
    bool IsAllowedInTokenCluster {get;}
}


public class TokenDefinition : ITokenDefinition
{
    Func<IScanner<char>,int,int> matchesUpTo;
    Func<Scanner<char>,AtToken> lex;    

    public readonly static TokenDefinition AtSymbol = SingleCharacterToken(TokenKind.AtSymbol,'@');       
    public readonly static TokenDefinition SemiColon = SingleCharacterToken(TokenKind.SemiColon,';');       
    public readonly static TokenDefinition LessThan = SingleCharacterToken(TokenKind.LessThan,'<');       
    public readonly static TokenDefinition GreaterThan = SingleCharacterToken(TokenKind.GreaterThan,'>');       
    public readonly static TokenDefinition Colon = SingleCharacterToken(TokenKind.Colon,':');       
    public readonly static TokenDefinition CloseBrace = SingleCharacterToken(TokenKind.CloseBrace,'}');       
    public readonly static TokenDefinition OpenBrace = SingleCharacterToken(TokenKind.OpenBrace,'{');  
    public readonly static TokenDefinition OpenParenthesis = SingleCharacterToken(TokenKind.OpenParenthesis,'(');       
    public readonly static TokenDefinition CloseParenthesis = SingleCharacterToken(TokenKind.CloseParenthesis,')');     
    public readonly static TokenDefinition Comma  = SingleCharacterToken(TokenKind.Comma,',');     
    public readonly static TokenDefinition StartOfFile = new TokenDefinition(TokenKind.StartOfFile,(s,i)=>s.Position<0&&i<1?1:-1,s=>new AtSyntaxTrivia(TokenKind.StartOfFile,-1));
    public readonly static TokenDefinition EndOfFile   = new TokenDefinition(TokenKind.EndOfFile,(s,i)=>s.End?int.MaxValue:-1,s=>new AtSyntaxTrivia(TokenKind.EndOfFile,s.Position+1));
    
    public readonly static TokenDefinition Space = new TokenDefinition
    (
        TokenKind.EndOfFile,
        (s,i)=>((s.Current==' ' || s.Current=='\t')) ? 1 : -1,
        s=>AtLexer.token<AtSyntaxTrivia>(TokenKind.Space,s,_=>_==' ' || _=='\t')
    );

    public readonly static TokenDefinition EndOfLine = new TokenDefinition
    (
        TokenKind.EndOfLine,
        (s,i)=>(s.Current=='\r' || s.Current=='\n') ? 1 : -1,
        s=>
        {
            var p = s.Position+1;
            var c = s.Consume();
 
            if (c == '\r' && s.Current == '\n')
            {
                s.MoveNext();
                //TODO: return AtToken only, cast to AtSyntaxTrivia in TokenDefList.Add()
                return new AtSyntaxTrivia(TokenKind.EndOfLine, p, "\r\n", null);
            }
            else
            {
                return new AtSyntaxTrivia(TokenKind.EndOfLine, p, c.ToString(), null);
            }       
        }
    );

    public readonly static ITokenDefinition StringLiteral = new StringLiteralDefinition('\"');
    

    public TokenDefinition(TokenKind tokenKind, Func<IScanner<char>,int,int> matchesUpTo, Func<Scanner<char>,AtToken> lex, bool allowedInCluster = false) 
    {  
        this.matchesUpTo = matchesUpTo;
        this.lex = lex;
        this.TokenKind = tokenKind; 
        this.IsAllowedInTokenCluster = allowedInCluster;
    }

    private class StringLiteralDefinition : ITokenDefinition
    {
        readonly char delimiter;    

        bool escaping = false;
        bool closed = false;

        public StringLiteralDefinition(char delimiter) { this.delimiter = delimiter; }
        
        public bool IsAllowedInTokenCluster => false;

        public AtToken Lex(Scanner<char> chars)
        {
            Debug.Assert(chars.Current==delimiter);

            var p  = chars.Position+1;
            var sb = new StringBuilder().Append(chars.Consume());

            while (!chars.End && chars.Current != delimiter)
            {
                if (chars.Current=='\\' && (chars.LookAhead(1)==delimiter||chars.LookAhead(1)=='\\'))
                    sb.Append(chars.Consume()).Append(chars.Consume()); // \" or \\
                else
                    sb.Append(chars.Consume());
            }

            Debug.Assert(chars.Current==delimiter);
            var text = sb.Append(chars.Consume()).ToString();
            return new AtToken(TokenKind.StringLiteral,p,text); 
        }

        public int MatchesUpTo(IScanner<char> chars, int i)
        {
            if (i==0)
            {
                closed   = false;
                escaping = false;

                return (chars.Current==delimiter) ? 1 : -1;                 
            }

            if (!closed)
            {
                if (chars.LookAhead(i)==delimiter && !escaping)
                    closed = true;

                escaping = (chars.LookAhead(i)=='\\');
  
                return chars.Position+1;
            }

            return i-1;
        }
    }

    public bool IsAllowedInTokenCluster {get;}
    public TokenKind TokenKind  {get;}
    public int MatchesUpTo(IScanner<char> input,int positionFromStart)=>matchesUpTo(input,positionFromStart);
    public AtToken Lex(Scanner<char> input) => lex(input);
    public static TokenDefinition SingleCharacterToken(TokenKind kind,char c, bool allowedInCluster=false) 
        => new TokenDefinition(kind,(s,k)=>k==0&&s.Current==c?1:-1,s=>new AtToken(kind,s.Position,c.ToString()),allowedInCluster);
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

    public int  Count => list.Count;
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

    /*
    public TokenDefinition Add(KnownTokenKind kind)
    {
        var tokenDef =   kind == KnownTokenKind.StartOfFile ? TokenDefinition.StartOfFile
                       : kind == KnownTokenKind.EndOfFile   ? TokenDefinition.EndOfFile
                       : kind == KnownTokenKind.Space       ? TokenDefinition.Space
                       : kind == KnownTokenKind.EndOfLine     ? TokenDefinition.NewLine
                       : null;

        if (tokenDef==null)
            throw new NotImplementedException("token kind = "+kind);

        list.Add(tokenDef);
        return tokenDef;
    }*/

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