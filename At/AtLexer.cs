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
    //readonly AtSourceText text;

    /*
    static Dictionary<string,int> tokenKinds = new  Dictionary<string,int>
    {
        {nameof(StartOfFile) ,(int) StartOfFile },
        {nameof(EndOfFile)   ,(int) EndOfFile   },
        {nameof(TokenCluster),(int) TokenCluster},
    };*/


    //Dictionary<string,TokenKind> tokenKinds = new Dictionary<string, TokenKind>();
   

    /*
    Dictionary<char,TokenKind> 
    singleCharTokens = new Dictionary<char,TokenKind> 
    { 
        {'@',AtSymbol},
        {';',SemiColon},
        {'<',LessThan},
        {'>',GreaterThan},
        {':',Colon},
        {'{',LeftBrace},
        {'(',LeftParenthesis},
        {'}',RightBrace},
        {')',RightParenthesis},
        {',',Comma}
    };    */

    public TokenDefinitionList TokenDefinitions  {get;} = new TokenDefinitionList();
    public TokenDefinitionList TriviaDefinitions {get;} = new TokenDefinitionList();

    public IEnumerable<AtToken> Lex(IEnumerable<char> input)
    {   
        var chars = new Scanner<char>(input);
        var leadingTrivia = new List<AtSyntaxTrivia>();
        var trailingTrivia = new List<AtSyntaxTrivia>();

         
        //TODO: IF tokens includes StartOfFile, yield <SOF>
        AtSyntaxTrivia sof = null;
        if (TokenDefinitions.Contains(TokenDefinition.StartOfFile))
            yield return (sof = new AtSyntaxTrivia(StartOfFile,0));

        //emits <StartOfFile>
        //yield return ;

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

            int k = 0;
            IList<ITokenDefinition> lastMatches = null, matches;

            //trivia (non-tokens)
            if (TriviaDefinitions.Count > 0)
            {
                while((matches = TriviaDefinitions.Matches(chars,++k)).Count>0)
                    lastMatches = matches;

                if (lastMatches?.Count > 0)
                {
                    var p = chars.Position;
                    var _triv = (AtSyntaxTrivia)lastMatches[0].Lex(chars);
                    trivia.Add(_triv);
                    if (p == chars.Position && _triv.Text?.Length > 0)
                        chars.MoveNext();
                    continue;
                }
            }
            
            //tokens
            if (_token == null && TokenDefinitions.Count > 0)
            {
                k = 0;
                while((matches = TokenDefinitions.Matches(chars,++k)).Count>0)
                    lastMatches = matches;

                if (lastMatches?.Count > 0)
                {
                    var p = chars.Position;
                    _token = lastMatches[0].Lex(chars);
                    if (p == chars.Position && _token.Text.Length > 0)
                        chars.MoveNext();
                    continue;
                }
            }

            //if (triviaDef*.matchesUpTo(scanner, pos) >= pos): triviaDef.lex(scanner) 

             /*if (isSpaceChar(c))
            {
                trivia.Add(token(Space,chars,isSpaceChar));
                continue;
            } 
            
            if (isNewLineChar(c))
            {
                trivia.Add(lineBreak(chars, c));
                continue;
            }

            if (_token == null)
            {
                _token =      singleCharTokens.ContainsKey(c) 
                                ? token(singleCharTokens[c],chars)

                            : c == '\"'                       
                                ? stringLiteral(c)

                            : c == '.'                        
                                ? dot()

                            : char.IsDigit(c) || (c=='+'||c=='-') && char.IsDigit(lookAhead(2))
                                ? numericLiteral()

                            : token( TokenKind.TokenCluster,chars
                                    ,b=>!isWhiteSpace(b) && !singleCharTokens.ContainsKey(b));
                continue;            
            }*/
            if (_token == null)
            {
                _token = tokenCluster(chars);
                continue;    
            }                            

            end:
            {
                //TODO: IF trivia includes EndOfFile, trailingTrivia.Add(<EOF>)
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
    static AtToken tokenCluster(Scanner<char> chars)
    {
        return token(TokenKind.TokenCluster,chars,b=>true);
    }

    /*
    //# numeric literal
    AtToken numericLiteral()
    {
        Debug.Assert(char.IsDigit(lookAhead(1)) || isNext('+') || isNext('-'));
        var p = position()+1;
        bool dot = false;
        var sb = new StringBuilder();

        if (isNext('+') || isNext('-'))
        {
            moveNext();
            sb.Append(current());
        }            

        while (!END() && (char.IsDigit(lookAhead(1)) || !dot && isNext('.')))
        {            
            if (isNext('.'))
                dot = true;         

            moveNext();
            sb.Append(current());
        }

        return new AtToken(TokenKind.NumericLiteral,p,sb.ToString());
    }

    // . | .. | ...
    AtToken dot()
    {
        moveNext();
        assertCurrent('.');
        var p = position();
       
        if (isNext('.'))
        {
           moveNext();
           if (isNext('.')) 
           {
              moveNext();
              return new AtToken(TokenKind.Ellipsis,p,"...");
           }

           return new AtToken(TokenKind.DotDot,p,"..");
        }
        else
        {
           return new AtToken(TokenKind.Dot,p,".");
        }
    }

    void assertCurrent(char c)
    {
       Debug.Assert(current()==c);
    }

    AtToken stringLiteral(char delimiter)
    {
        moveNext();       
        var p  = position();
        var sb = new StringBuilder().Append(delimiter);
                
        while (!END() && lookAhead(1) != delimiter) 
        {
           if (lookAhead(1)=='\\' && lookAhead(2)==delimiter)
           {
                moveNext();
                moveNext();
                sb.Append('\\').Append(delimiter);
           }
           else
           {
                moveNext();
                sb.Append(current());           
           }
        }

        moveNext();
        var text = sb.Append(current()).ToString();
        return new AtToken(TokenKind.StringLiteral,p,text);    
    }

    static AtToken lineBreak(Buffer<char> buffer, char c)
    {
        buffer.MoveNext();
        var p = buffer.Position+1;

        if (c == '\r' && buffer.LookAhead(1) == '\n')
        {
            buffer.MoveNext();
            return new AtToken(TokenKind.EndOfLine, p, "\r\n");
        }
        else
        {
            return new AtToken(TokenKind.EndOfLine, p, c.ToString());
        }
    }

    bool isWhiteSpace(char b)
    {
        return isSpaceChar(b) || isNewLineChar(b);
    }

    private bool isNewLineChar(char c)
    {
        return c=='\r'||c=='\n';
    }


    bool isSpaceChar(char c)
    {
        return c==' ' || c=='\t';
    }*/

    static AtToken token(TokenKind kind, Scanner<char> buffer, Func<char,bool> predicate=null)
    {
        //buffer.MoveNext();
        if (predicate==null) 
            predicate = c => false;     
       
        var sb  = new StringBuilder().Append(buffer.Consume());
        var pos = buffer.Position;
        
        while (!buffer.End && predicate(buffer.Current)) 
            sb.Append(buffer.Consume());  

        var text = sb.ToString();
        return new AtToken(kind,pos,text);
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