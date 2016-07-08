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

    /// <summary>Returns true if the token definition matches the input up to 
    /// the given k-lookahead.</summary>
    bool MatchesUpTo(IScanner<char> chars, int k);
    AtToken Lex(Scanner<char> chars);
    bool IsAllowedInTokenCluster {get;}
}


public class TokenDefinition : ITokenDefinition
{
    Func<IScanner<char>,int,bool> matchesUpTo;
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
    public readonly static TokenDefinition StartOfFile = new TokenDefinition(TokenKind.StartOfFile,(s,i)=>s.Position<0&&i<1,s=>new AtSyntaxTrivia(TokenKind.StartOfFile,-1));
    public readonly static TokenDefinition EndOfFile   = new TokenDefinition(TokenKind.EndOfFile,(s,i)=>s.End,s=>new AtSyntaxTrivia(TokenKind.EndOfFile,s.Position+1));
    
    public readonly static TokenDefinition Space = new TokenDefinition
    (
        TokenKind.Space,
        (s,i)=>((s.LookAhead(i)==' ' || s.LookAhead(i)=='\t')),
        s=>AtLexer.token<AtSyntaxTrivia>(TokenKind.Space,s,_=>_==' ' || _=='\t')
    );

    public readonly static TokenDefinition EndOfLine = new TokenDefinition
    (
        TokenKind.EndOfLine,
        (s,i)=>(i>=0 && s.LookAhead(i)=='\r' || i>=0 && i<=1 && s.LookAhead(i)=='\n'),
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
    public readonly static ITokenDefinition NumericLiteral = new NumericLiteralDefinition();

    //TODO: convert to 3 different token-defs
    public readonly static ITokenDefinition Dots = new DotsDefinition();
    

    public TokenDefinition(TokenKind tokenKind, Func<IScanner<char>,int,bool> matchesUpTo, Func<Scanner<char>,AtToken> lex, bool allowedInCluster = false) 
    {  
        this.matchesUpTo = matchesUpTo;
        this.lex = lex;
        this.TokenKind = tokenKind; 
        this.IsAllowedInTokenCluster = allowedInCluster;
    }

        private class NumericLiteralDefinition:ITokenDefinition
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
        }

        private class StringLiteralDefinition : ITokenDefinition
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
    }

    private class DotsDefinition:ITokenDefinition
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
    }

    public bool IsAllowedInTokenCluster {get;}
    public TokenKind TokenKind  {get;}
    public bool MatchesUpTo(IScanner<char> chars,int k)=>matchesUpTo(chars,k);
    public AtToken Lex(Scanner<char> input) => lex(input);
    public static TokenDefinition SingleCharacterToken(TokenKind kind,char c, bool allowedInCluster=false) 
        => new TokenDefinition(kind,(s,k)=>k==0&&s.Current==c,s=>new AtToken(kind,s.Position,c.ToString()),allowedInCluster);       
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
        return list.Where(_=>_.MatchesUpTo(chars,k)).ToList();
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