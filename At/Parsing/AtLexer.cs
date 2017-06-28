using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static At.TokenRule;
using ITokenRule = Limpl.ITokenRule<At.AtToken>;
using ITriviaRule = Limpl.ITriviaRule<At.AtSyntaxTrivia>;

namespace At
{
public class AtLexer : Limpl.Lexer<AtToken,AtSyntaxTrivia>, IDisposable
{
    public AtLexer(IEnumerable<ITokenRule> tokenRules = null,IEnumerable<ITriviaRule> triviaRules = null,Limpl.Scanner<char> scanner = null) : base(tokenRules,triviaRules,scanner)
    {
    }

    public static AtLexer DefaultLexer = new AtLexer(
                                                tokenRules: new[]
                                                {
                                                    SemiColon,
                                                    LessThan,
                                                    TokenRule.AtSymbol,
                                                    GreaterThan,        
                                                    Dots,
                                                    Colon,
                                                    OpenBrace,
                                                    OpenParenthesis,
                                                    OpenBracket,
                                                    CloseBrace,
                                                    CloseParenthesis,
                                                    CloseBracket,
                                                    Comma,
                                                    StringLiteral, 
                                                    NumericLiteral,
                                                    Plus,
                                                    Asterisk,
                                                },

                                                triviaRules: new[]
                                                {
                                                    Space,  
                                                    EndOfLine,
                                                    TokenRule.StartOfFile,
                                                    TokenRule.EndOfFile,
                                                });

    public AtLexer WithTokenRule(ITokenRule tokenRule)
      => new AtLexer(TokenRules.Add(tokenRule),TriviaRules);

    public AtLexer WithTokenRules(params ITokenRule[] tokenRules)
      => new AtLexer(TokenRules.Concat(tokenRules),TriviaRules);

    public AtLexer WithTriviaRule(ITriviaRule rule)
      => new AtLexer(TokenRules,TriviaRules.Add(rule));


    protected override void OnStartOfFile(out AtToken sof)
    {
        base.OnStartOfFile(out sof);

        if (TokenRules.Contains(TokenRule.StartOfFile))
            sof = new AtSyntaxTrivia(TokenKind.StartOfFile,0);
    }

    protected override void OnEndOfFileTrivia(out AtSyntaxTrivia eof)
    {
        base.OnEndOfFileTrivia(out eof);

         if (Scanner.End && TriviaRules.Contains(TokenRule.EndOfFile))
            eof = new AtSyntaxTrivia(TokenKind.EndOfFile,Scanner.Position+1);
    }

    protected override void OnEndOfFileToken(out AtToken eof)
    {
        base.OnEndOfFileToken(out eof);

        if (TokenRules.Contains(TokenRule.EndOfFile))
            eof = new AtSyntaxTrivia(TokenKind.EndOfFile,Scanner.Position+1);
    }

    protected override AtToken LexFallbackToken(Limpl.IScanner<char> chars)
    {
        return tokenCluster(chars);
    }

    protected override void SetParent(ref AtSyntaxTrivia trivia,Limpl.ISyntaxNode parent)
    {
        trivia.Parent = parent;
    }

    protected override void SetLeadingTrivia(ref AtToken token,Limpl.SyntaxList<AtSyntaxTrivia> leadingTrivia)
    {
        token.LeadingTrivia = leadingTrivia;
    }

    protected override void SetTrailingTrivia(ref AtToken token,Limpl.SyntaxList<AtSyntaxTrivia> trailingTrivia)
    {
        token.TrailingTrivia = trailingTrivia;
    }


    // **token cluster**
    AtToken tokenCluster(Limpl.IScanner<char> chars)
    {
        return token(TokenKind.TokenCluster,chars,null,c=>isPartOfTokenCluster(c,chars));
    }

    bool isPartOfTokenCluster(char c,Limpl.IScanner<char> chars)
    {
        return !isTrivia(c,chars) && isAllowedInTokenCluster(c,chars);
    }

    bool isAllowedInTokenCluster(char c,Limpl.IScanner<char> chars)
    {
        return GetTokenRule(chars)?.IsAllowedInOtherToken ?? true;
    }

    //is trivia (non-tokens)
    bool isTrivia(char c, Limpl.IScanner<char> chars)
    {
        return GetTriviaRule(chars) != null;
    }

    internal static  AtToken token 
    (
        TokenKind kind, 
        Limpl.IScanner<char> buffer, 
        Limpl.ITokenRule<AtToken> tokendef,
        Func<char,bool> predicate=null) {
        
        return token<AtToken>(kind,buffer,tokendef,predicate);
    }
    internal static  T token<T>
    (
        TokenKind kind, 
        Limpl.IScanner<char> buffer, 
        Limpl.ITokenRule<AtToken> tokendef,
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