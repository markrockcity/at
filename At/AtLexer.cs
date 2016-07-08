using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static At.TokenKind;

namespace At
{
public class AtLexer : IDisposable
{
    public TokenDefinitionList TokenDefinitions  {get;} = new TokenDefinitionList();
    public TokenDefinitionList TriviaDefinitions {get;} = new TokenDefinitionList();

    public IEnumerable<AtToken> Lex(IEnumerable<char> input)
    {   
        var chars = new Scanner<char>(input);
        var leadingTrivia = new List<AtSyntaxTrivia>();
        var trailingTrivia = new List<AtSyntaxTrivia>();

        //<StartOfFile>? 
        AtSyntaxTrivia sof = null;
        if (TokenDefinitions.Contains(TokenDefinition.StartOfFile))
            yield return (sof = new AtSyntaxTrivia(StartOfFile,0));

        AtToken _token = null;
        while (!chars.End || _token != null)
        {        
            var c = chars.Current;
            var trivia = (_token==null) ? leadingTrivia : trailingTrivia;

            if (c == '\0') //NUL is before beginning and after end
            {
                if (chars.Position<0 && TriviaDefinitions.Contains(TokenDefinition.StartOfFile))
                    leadingTrivia.Add(sof ?? new AtSyntaxTrivia(StartOfFile,0));                               

                if (chars.End)
                    goto end;
                else
                    chars.MoveNext();
            }

            //trivia (non-tokens)
            var triviaDef = getDefinition(TriviaDefinitions,chars);
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
                var tokenDef = getDefinition(TokenDefinitions,chars);    

                if (tokenDef != null)
                {
                    var p = chars.Position;
                    _token = tokenDef.Lex(chars);
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
                if (chars.End && TriviaDefinitions.Contains(TokenDefinition.EndOfFile))
                    trailingTrivia.Add(new AtSyntaxTrivia(TokenKind.EndOfFile,chars.Position+1));
            
                if (leadingTrivia.Count > 0)
                    _token.leadingTrivia = new AtSyntaxList<AtSyntaxTrivia>(_token,leadingTrivia);
                
                if (trailingTrivia.Count > 0)
                    _token.trailingTrivia = new AtSyntaxList<AtSyntaxTrivia>(_token,trailingTrivia);
                
                if (_token != null)
                    yield return _token;
            
                _token = null;
                leadingTrivia.Clear();
                trailingTrivia.Clear();            
            }
        }  

        if (TokenDefinitions.Contains(TokenDefinition.EndOfFile))
            yield return new AtSyntaxTrivia(TokenKind.EndOfFile,chars.Position+1);
    }


    //# token cluster
    AtToken tokenCluster(Scanner<char> chars)
    {
        return token(TokenKind.TokenCluster,chars,c=>isPartOfTokenCluster(c,chars));
    }

    bool isPartOfTokenCluster(char c,Scanner<char> chars)
    {
        return !isTrivia(c,chars) && isAllowedInTokenCluster(c,chars);
    }

    bool isAllowedInTokenCluster(char c,Scanner<char> chars)
    {
        return getDefinition(TokenDefinitions,chars)?.IsAllowedInTokenCluster ?? true;
    }

    //is trivia (non-tokens)
    bool isTrivia(char c, Scanner<char> chars)
    {
        return getDefinition(TriviaDefinitions,chars) != null;
    }

    ITokenDefinition getDefinition(TokenDefinitionList definitions, Scanner<char> chars)
    {
        int k = -1;
        IList<ITokenDefinition> lastMatches = null, matches;

        if (definitions.Count > 0)
        {
            k = -1;
            //TODO: instead of re-querying {definitions} all the time, just do {lastMatches}
            while((matches = definitions.Matches(chars,++k)).Count>0)
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

    internal static  AtToken token (
        TokenKind kind, 
        Scanner<char> buffer, 
        Func<char,bool> predicate=null) {
        
            return token<AtToken>(kind,buffer,predicate);
        }
    internal static  T token<T>
    (
        TokenKind kind, 
        Scanner<char> buffer, 
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
                ? (T) (object) new AtSyntaxTrivia(kind,pos,text)
                : (T) new AtToken(kind,pos,text);
    }

    void IDisposable.Dispose(){}

    /*
    char consume()                 => chars.Consume();
    bool isNext(char c, int k = 1) => chars.LookAhead(k)==c; 
    bool moveNext()                => chars.MoveNext();
    char lookAhead(int k)          => chars.LookAhead(k);
    char current()                 => chars.Current;
    bool END()                     => chars.End;
    int  position()                => chars.Position+1;*/
}
}