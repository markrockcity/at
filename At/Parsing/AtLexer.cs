using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static At.TokenRule;

namespace At
{
public class AtLexer : IDisposable
{
    public TokenRuleList TokenRules  {get;} = new TokenRuleList();
    public TokenRuleList TriviaRules {get;} = new TokenRuleList();

    public static AtLexer CreateDefaultLexer()
    {
        var lexer = new AtLexer();

        lexer.TriviaRules.Add(Space);  
        lexer.TriviaRules.Add(EndOfLine);
        lexer.TriviaRules.Add(StartOfFile);
        lexer.TriviaRules.Add(EndOfFile);

        lexer.TokenRules.Add(SemiColon);
        lexer.TokenRules.Add(LessThan);
        lexer.TokenRules.Add(TokenRule.AtSymbol);
        lexer.TokenRules.Add(GreaterThan);        
        lexer.TokenRules.Add(Dots);
        lexer.TokenRules.Add(Colon);
        lexer.TokenRules.Add(OpenBrace);
        lexer.TokenRules.Add(OpenParenthesis);
        lexer.TokenRules.Add(OpenBracket);
        lexer.TokenRules.Add(CloseBrace);
        lexer.TokenRules.Add(CloseParenthesis);
        lexer.TokenRules.Add(CloseBracket);
        lexer.TokenRules.Add(Comma);
        lexer.TokenRules.Add(StringLiteral); 
        lexer.TokenRules.Add(NumericLiteral);
        lexer.TokenRules.Add(Plus);

        return lexer;
    }

    public IEnumerable<AtToken> Lex(IEnumerable<char> input)
    {   
        var chars = new Scanner<char>(input);
        var leadingTrivia = new List<AtSyntaxTrivia>();
        var trailingTrivia = new List<AtSyntaxTrivia>();

        //<StartOfFile>? 
        AtSyntaxTrivia sof = null;
        if (TokenRules.Contains(TokenRule.StartOfFile))
            yield return (sof = new AtSyntaxTrivia(TokenKind.StartOfFile,0));

        AtToken _token = null;
        while (!chars.End || _token != null)
        {        
            var c = chars.Current;
            var trivia = (_token==null) ? leadingTrivia : trailingTrivia;

            if (c == '\0') //NUL is before beginning and after end
            {
                if (chars.Position<0 && TriviaRules.Contains(TokenRule.StartOfFile))
                    leadingTrivia.Add(sof ?? new AtSyntaxTrivia(TokenKind.StartOfFile,0));                               

                if (chars.End)
                    goto end;
                else
                    chars.MoveNext();
            }

            //trivia (non-tokens)
            var triviaDef = getRule(TriviaRules,chars);
            if (triviaDef != null)
            {
                var p = chars.Position;
                var _triv = (AtSyntaxTrivia) triviaDef.Lex(chars);
                trivia.Add(_triv);
                if (p == chars.Position && _triv.Text?.Length > 0)
                    chars.MoveNext();
                continue;
            }
            
            //tokens
            if (_token == null )
            {
                var tokenRule = getRule(TokenRules,chars);    

                if (tokenRule != null)
                {
                    var p = chars.Position;
                    _token = tokenRule.Lex(chars);
                    if (p == chars.Position && _token.Text.Length > 0)
                        chars.MoveNext();
                    continue;
                }
            }

            if (_token == null)
            {
                _token = tokenCluster(chars);
                continue;    
            }                            

            end:
            {
                if (chars.End && TriviaRules.Contains(TokenRule.EndOfFile))
                    trailingTrivia.Add(new AtSyntaxTrivia(TokenKind.EndOfFile,chars.Position+1));
            
                if (leadingTrivia.Count > 0)
                    _token.LeadingTrivia = new AtSyntaxList<AtSyntaxTrivia>(_token,leadingTrivia);
                
                if (trailingTrivia.Count > 0)
                    _token.TrailingTrivia = new AtSyntaxList<AtSyntaxTrivia>(_token,trailingTrivia);
                
                if (_token != null)
                    yield return _token;
            
                _token = null;
                leadingTrivia.Clear();
                trailingTrivia.Clear();            
            }
        }  

        if (TokenRules.Contains(TokenRule.EndOfFile))
            yield return new AtSyntaxTrivia(TokenKind.EndOfFile,chars.Position+1);
    }


    // **token cluster**
    AtToken tokenCluster(Scanner<char> chars)
    {
        return token(TokenKind.TokenCluster,chars,null,c=>isPartOfTokenCluster(c,chars));
    }

    bool isPartOfTokenCluster(char c,Scanner<char> chars)
    {
        return !isTrivia(c,chars) && isAllowedInTokenCluster(c,chars);
    }

    bool isAllowedInTokenCluster(char c,Scanner<char> chars)
    {
        return getRule(TokenRules,chars)?.IsAllowedInTokenCluster ?? true;
    }

    //is trivia (non-tokens)
    bool isTrivia(char c, Scanner<char> chars)
    {
        return getRule(TriviaRules,chars) != null;
    }

    ITokenRule getRule(TokenRuleList rules, Scanner<char> chars)
    {
        int k = -1;
        IList<ITokenRule> lastMatches = null, matches;

        if (rules.Count > 0)
        {
            k = -1;
            //TODO: instead of re-querying {rules} all the time, just do {lastMatches}
            while((matches = rules.Matches(chars,++k)).Count>0)
            {
                lastMatches = matches;

                if (chars.End)
                    break;
            }

            if (lastMatches?.Count > 0)
                return lastMatches[0];
        }    

        return null;
    }

    internal static  AtToken token 
    (
        TokenKind kind, 
        Scanner<char> buffer, 
        ITokenRule tokendef,
        Func<char,bool> predicate=null) {
        
        return token<AtToken>(kind,buffer,tokendef,predicate);
    }
    internal static  T token<T>
    (
        TokenKind kind, 
        Scanner<char> buffer, 
        ITokenRule tokendef,
        Func<char,bool> predicate=null)
        
        where T : AtToken {
    
        //TODO: change to false only after called once before? 
        if (predicate==null) 
            predicate = c => false;     
       
        var sb  = new StringBuilder().Append(buffer.Consume());
        var pos = buffer.Position;
        
        while (!buffer.End && predicate(buffer.Current)) 
            sb.Append(buffer.Consume());  

        var text = sb.ToString();

        return typeof(T)==typeof(AtSyntaxTrivia)
                ? (T) (object) new AtSyntaxTrivia(kind,pos,text,tokendef)
                : (T) new AtToken(kind,pos,text,tokendef);
    }

    void IDisposable.Dispose(){}
}
}