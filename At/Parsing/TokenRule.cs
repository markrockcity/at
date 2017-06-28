using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Scanner = Limpl.Scanner<char>;
using IScanner = Limpl.IReadOnlyScanner<char>;
using ITokenRule = Limpl.ITokenRule<At.AtToken>;
using Limpl;

namespace At
{
public class TokenRule : Limpl.ITokenRule<AtToken>, Limpl.ITriviaRule<AtSyntaxTrivia>
{
    Func<Limpl.IReadOnlyScanner<char>,int,bool> matchesUpTo;
    Func<TokenRule,Limpl.IScanner<char>,AtToken> lex;    

    public readonly static TokenRule AtSymbol = SingleCharacterToken(TokenKind.AtSymbol,'@');       
    public readonly static TokenRule SemiColon = SingleCharacterToken(TokenKind.SemiColon,';');       
    public readonly static TokenRule LessThan = SingleCharacterToken(TokenKind.LessThan,'<');       
    public readonly static TokenRule GreaterThan = SingleCharacterToken(TokenKind.GreaterThan,'>');       
    public readonly static TokenRule Colon = SingleCharacterToken(TokenKind.Colon,':');       
    public readonly static TokenRule CloseBrace = SingleCharacterToken(TokenKind.CloseBrace,'}');       
    public readonly static TokenRule OpenBrace = SingleCharacterToken(TokenKind.OpenBrace,'{');  
    public readonly static TokenRule OpenBracket = SingleCharacterToken(TokenKind.OpenBracket,'[');     
    public readonly static TokenRule CloseBracket = SingleCharacterToken(TokenKind.CloseBracket,']');     
    public readonly static TokenRule OpenParenthesis = SingleCharacterToken(TokenKind.OpenParenthesis,'(');         
    public readonly static TokenRule CloseParenthesis = SingleCharacterToken(TokenKind.CloseParenthesis,')');     
    public readonly static TokenRule Comma  = SingleCharacterToken(TokenKind.Comma,',');  
    public readonly static TokenRule Plus = SingleCharacterToken(TokenKind.Plus,'+');     
    public readonly static TokenRule Asterisk = SingleCharacterToken(TokenKind.Asterisk,'*');     
    public readonly static TokenRule StartOfFile = new TokenRule(TokenKind.StartOfFile,(s,i)=>s.Position<0&&i<1,(td,s)=>new AtSyntaxTrivia(TokenKind.StartOfFile,-1,tokenSrc:td));
    public readonly static TokenRule EndOfFile   = new TokenRule(TokenKind.EndOfFile,(s,i)=>s.End,(td,s)=>new AtSyntaxTrivia(TokenKind.EndOfFile,s.Position+1,tokenSrc:td));

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

    public readonly static Limpl.ITokenRule<AtToken> StringLiteral = new StringLiteralRule('\"','\'');
    public readonly static Limpl.ITokenRule<AtToken> NumericLiteral = new NumericLiteralRule();

    //TODO: convert to 3 different token-defs
    public readonly static Limpl.ITokenRule<AtToken> Dots = new DotsDefinition();
    
    /// <summary>Initializes a TokenRule object</summary>
    /// <param name="tokenKind"></param>
    /// <param name="matchesUpTo">A delegate that accepts an IScanner&lt;char> and a character position and returns a boolean saying whether the token definition matches all characters up to the given look-ahead position.</param>
    /// <param name="lex">A delegate that accepts a TokenRule (this object) and a Scanner&lt;char>, returning an AtToken.</param>
    /// <param name="allowedInCluster">If true, defined token may be part of a TokenCluster before being lexed.</param>
    public TokenRule(TokenKind tokenKind, Func<Limpl.IReadOnlyScanner<char>,int,bool> matchesUpTo, Func<Limpl.ITokenRule<AtToken>,Limpl.IScanner<char>,AtToken> lex, bool allowedInCluster = false) 
    {  
        this.matchesUpTo = matchesUpTo;
        this.lex = lex;
        this.TokenKind = tokenKind; 
        this.IsAllowedInTokenCluster = allowedInCluster;
    }

    private class NumericLiteralRule : Limpl.NumericLiteralRule<AtToken>
    {
        public override AtToken CreateToken(string s,double value, int position)
        {
            return new AtToken(TokenKind.NumericLiteral,position,s,this,value:value);
        }
    }

    private class StringLiteralRule : Limpl.StringLiteralRule<AtToken>
    {
        public StringLiteralRule(params char[] delimiters) : base(delimiters)
        {
        
        }
    
        public override AtToken CreateToken(string s, int position)
        {
            return new AtToken(TokenKind.StringLiteral,position,s,this,value:s);
        }
    }

    private class DotsDefinition : Limpl.ITokenRule<AtToken>
    {
        public bool IsAllowedInTokenCluster => true;
        bool Limpl.ITokenRule<AtToken>.IsAllowedInOtherToken => true;

        public AtToken Lex(Limpl.IScanner<char> chars)
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

        public bool MatchesUpTo(Limpl.IReadOnlyScanner<char> chars,int k) => (k>=0 && k<=2 && chars.LookAhead(k)=='.');
        public AtToken CreateToken(IEnumerable<char> chars) => Lex(new Scanner(chars));

            
    }

    public bool IsAllowedInTokenCluster {get;}
    public TokenKind TokenKind  {get;}

    bool ITokenRule.IsAllowedInOtherToken => IsAllowedInTokenCluster;

    public bool MatchesUpTo(IScanner chars,int k)=>matchesUpTo(chars,k);
    public AtToken Lex(Limpl.IScanner<char> input) => lex(this,input);
    public static TokenRule SingleCharacterToken(TokenKind kind,char c, bool allowedInCluster=false) 
        => new TokenRule(kind,(s,k)=>k==0&&s.Current==c,(rule,s)=>new AtToken(kind,s.Position,c.ToString(),tokenSrc: rule),allowedInCluster);       
    public AtToken CreateToken(IEnumerable<char> chars) => Lex(new Scanner(chars));

    AtSyntaxTrivia ITriviaRule<AtSyntaxTrivia>.Lex(IScanner<char> chars)
    {
       return (AtSyntaxTrivia) Lex(chars);
    } 

    AtSyntaxTrivia ITriviaSource<AtSyntaxTrivia>.CreateTrivia(IEnumerable<char> chars)
    {
       return (AtSyntaxTrivia) Lex(new Limpl.Scanner<char>(chars));
    }
}
}