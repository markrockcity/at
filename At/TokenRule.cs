using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using static At.TokenKind;
using At;
using System.Text.RegularExpressions;

namespace At
{

public interface ITokenSource
{
    AtToken CreateToken(IEnumerable<char> chars);
}

public interface ITokenRule : ITokenSource
{
    //TokenKind TokenKind  {get;}

    /// <summary>Returns true if the token definition matches the input up to 
    /// the given k-lookahead.</summary>
    bool MatchesUpTo(IScanner<char> chars, int k);
    AtToken Lex(Scanner<char> chars);
    bool IsAllowedInTokenCluster {get;}
}

/*

public interface ITokenDefinition : ITokenSource
{
    Regex Pattern {get;}
}
*/

public class TokenRule : ITokenRule
{
    Func<IScanner<char>,int,bool> matchesUpTo;
    Func<TokenRule,Scanner<char>,AtToken> lex;    

    public readonly static TokenRule AtSymbol = SingleCharacterToken(TokenKind.AtSymbol,'@');       
    public readonly static TokenRule SemiColon = SingleCharacterToken(TokenKind.SemiColon,';');       
    public readonly static TokenRule LessThan = SingleCharacterToken(TokenKind.LessThan,'<');       
    public readonly static TokenRule GreaterThan = SingleCharacterToken(TokenKind.GreaterThan,'>');       
    public readonly static TokenRule Colon = SingleCharacterToken(TokenKind.Colon,':');       
    public readonly static TokenRule CloseBrace = SingleCharacterToken(TokenKind.CloseBrace,'}');       
    public readonly static TokenRule OpenBrace = SingleCharacterToken(TokenKind.OpenBrace,'{');  
    public readonly static TokenRule OpenParenthesis = SingleCharacterToken(TokenKind.OpenParenthesis,'(');       
    public readonly static TokenRule CloseParenthesis = SingleCharacterToken(TokenKind.CloseParenthesis,')');     
    public readonly static TokenRule Comma  = SingleCharacterToken(TokenKind.Comma,',');     
    public readonly static TokenRule StartOfFile = new TokenRule(TokenKind.StartOfFile,(s,i)=>s.Position<0&&i<1,(td,s)=>new AtSyntaxTrivia(TokenKind.StartOfFile,-1,tokenDefinition:td));
    public readonly static TokenRule EndOfFile   = new TokenRule(TokenKind.EndOfFile,(s,i)=>s.End,(td,s)=>new AtSyntaxTrivia(TokenKind.EndOfFile,s.Position+1,tokenDefinition:td));
    
    public readonly static TokenRule Space = new TokenRule
    (
        TokenKind.Space,
        (s,i)=>((s.LookAhead(i)==' ' || s.LookAhead(i)=='\t')),
        (td,s)=>AtLexer.token<AtSyntaxTrivia>(TokenKind.Space,s,td,_=>_==' ' || _=='\t')
    );

    public readonly static TokenRule EndOfLine = new TokenRule
    (
        TokenKind.EndOfLine,
        (s,i)=>(i>=0 && s.LookAhead(i)=='\r' || i>=0 && i<=1 && s.LookAhead(i)=='\n'),
        (td,s)=>
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

    public readonly static ITokenRule StringLiteral = new StringLiteralDefinition('\"');
    public readonly static ITokenRule NumericLiteral = new NumericLiteralDefinition();

    //TODO: convert to 3 different token-defs
    public readonly static ITokenRule Dots = new DotsDefinition();
    
    /// <summary>Initializes a TokenRule object</summary>
    /// <param name="tokenKind"></param>
    /// <param name="matchesUpTo">A delegate that accepts an IScanner&lt;char> and a character position and returns a boolean saying whether the token definition matches all characters up to the given look-ahead position.</param>
    /// <param name="lex">A delegate that accepts a TokenRule (this object) and a Scanner&lt;char>, returning an AtToken.</param>
    /// <param name="allowedInCluster">If true, defined token may be part of a TokenCluster before being lexed.</param>
    public TokenRule(TokenKind tokenKind, Func<IScanner<char>,int,bool> matchesUpTo, Func<TokenRule,Scanner<char>,AtToken> lex, bool allowedInCluster = false) 
    {  
        this.matchesUpTo = matchesUpTo;
        this.lex = lex;
        this.TokenKind = tokenKind; 
        this.IsAllowedInTokenCluster = allowedInCluster;
    }

    private class NumericLiteralDefinition : ITokenRule
    {
        bool alreadyHasDecimalPoint = false;
        
        public bool IsAllowedInTokenCluster => true;

        public AtToken Lex(Scanner<char> chars)
        {
            Debug.Assert(char.IsDigit(chars.Current));
            alreadyHasDecimalPoint = false;
            var p = chars.Position+1;
            var sb = new StringBuilder();
                
            while (!chars.End && (char.IsDigit(chars.Current) || !alreadyHasDecimalPoint && chars.Current=='.' && char.IsDigit(chars.Next)))
            {            
                if (chars.Current=='.')
                    alreadyHasDecimalPoint = true;         

                sb.Append(chars.Consume());
            }

            return new AtToken(TokenKind.NumericLiteral,p,sb.ToString());            
        }

        public bool MatchesUpTo(IScanner<char> chars,int k)
        {
            if (k==0)
                alreadyHasDecimalPoint = false; //reset
            
            var c = chars.LookAhead(k);
            var isDigit = char.IsDigit(c);

            if (isDigit)
                return true;

            var isDecimalPoint = (c=='.');

            if (k==0 || !isDecimalPoint || alreadyHasDecimalPoint)
                return false;

            Debug.Assert(isDecimalPoint);
            alreadyHasDecimalPoint = true;
            return char.IsDigit(chars.LookAhead(k+1));
        }

        AtToken ITokenSource.CreateToken(IEnumerable<char> chars) => Lex(new Scanner<char>(chars));
    }

    private class StringLiteralDefinition : ITokenRule
    {
        readonly char delimiter;    

        bool escaping = false;
        bool closed = false;
        bool matchesSoFar = false;

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

        public bool MatchesUpTo(IScanner<char> chars, int i)
        {
            if (i==0)
            {
                closed   = false;
                escaping = false;
                matchesSoFar = (chars.Current==delimiter);

                return matchesSoFar;                 
            }

            var c = chars.LookAhead(i);

            if (matchesSoFar && !closed && c!='\0')
            {
                if (c==delimiter && !escaping)
                    closed = true;

                escaping = (!escaping && c=='\\');
  
                return true;
            }

            return false;
        }

        AtToken ITokenSource.CreateToken(IEnumerable<char> chars) => Lex(new Scanner<char>(chars));
    }

    private class DotsDefinition : ITokenRule
    {
        public bool IsAllowedInTokenCluster => true;

        public AtToken Lex(Scanner<char> chars)
        {
            Debug.Assert(chars.Current=='.');
            var p = chars.Position+1;
            chars.MoveNext();
       
            if (chars.Current=='.')
            {
                chars.MoveNext();
                if (chars.Current=='.') 
                {
                    chars.MoveNext();
                    return new AtToken(TokenKind.Ellipsis,p,"...");
                }

                return new AtToken(TokenKind.DotDot,p,"..");
            }
            else
            {
                return new AtToken(TokenKind.Dot,p,".");
            }       
        }

        public bool MatchesUpTo(IScanner<char> chars,int k) => (k>=0 && k<=2 && chars.LookAhead(k)=='.');
        AtToken ITokenSource.CreateToken(IEnumerable<char> chars) => Lex(new Scanner<char>(chars));
    }

    public bool IsAllowedInTokenCluster {get;}
    public TokenKind TokenKind  {get;}
    public bool MatchesUpTo(IScanner<char> chars,int k)=>matchesUpTo(chars,k);
    public AtToken Lex(Scanner<char> input) => lex(this,input);
    public static TokenRule SingleCharacterToken(TokenKind kind,char c, bool allowedInCluster=false) 
        => new TokenRule(kind,(s,k)=>k==0&&s.Current==c,(rule,s)=>new AtToken(kind,s.Position,c.ToString(),rule),allowedInCluster);       
    AtToken ITokenSource.CreateToken(IEnumerable<char> chars) => Lex(new Scanner<char>(chars));
}

public class TokenRuleList : TokenSourceList<ITokenRule>
{
    public IList<ITokenRule> Matches(IScanner<char> chars, int k)
    {
        return InnerList.Where(_=>_.MatchesUpTo(chars,k)).ToList();
    }

    public TokenRule Add(string tokenText, TokenKind? kind = null)
    { 
       throw new NotImplementedException();
    }

    public TokenRule AddPattern(string pattern, TokenKind? kind = null)
    { 
       throw new NotImplementedException();
    }

    //                        (iscanner, positionFromStart) => maxPositionChecked
    public TokenRule Add(Func<IScanner<char>,int,int> f, Func<Scanner<char>,AtToken> lex) 
    { 
       throw new NotImplementedException();
    }

}

public class TokenSourceList<T> : IList<T> where T : ITokenSource
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


    public void Add(T item)
    {
        InnerList.Add(item);
    }

    public void Clear()
    {
       InnerList.Clear();
    }

    public bool Contains(T item)
    {
        return InnerList.Contains(item);
    }

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