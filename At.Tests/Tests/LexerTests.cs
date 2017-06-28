using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace At.Tests
{
[TestClass]
public class LexerTests : Test
{
    AtLexer lexer;

    protected override void Setup()
    {
        base.Setup();
        lexer = new AtLexer();
    }

    //# Lexer Test 1
    [TestMethod] 
    public void LexerTest1()
    {
        assert_equals(TokenKind.TokenCluster,()=>lexerTest("<>",1,null).Single().Kind);
        lexerTest("@x = 5\r\n5", expectedTokenCount:  1);


        lexer = lexer.WithTriviaRule(TokenRule.StartOfFile);
        lexerTest("x", 1,tokens =>
        {
            Write(()=>tokens[0].LeadingTrivia);
            var sof = tokens[0].LeadingTrivia.Single();
            assert_equals(TokenKind.StartOfFile,sof.Kind);
        });  
        
        lexer = lexer.WithTokenRule(TokenRule.StartOfFile);  
        lexerTest("x",expectedTokenCount: 2); // <StartOfFile> & 'x'

        lexer = lexer.WithTriviaRule(TokenRule.EndOfFile);
        lexerTest("x",a:tokens=>
        {            
            Write(()=>tokens[1].TrailingTrivia);
            var eof = tokens[1].TrailingTrivia.Single();
            assert_equals(TokenKind.EndOfFile,eof.Kind);  
        });

        lexer = lexer.WithTokenRule(TokenRule.EndOfFile);   
        lexerTest("x",3); // <StartOfFile> & 'x' & <EOF>

        lexer = new AtLexer(triviaRules:lexer.TriviaRules.Add(TokenRule.Space));   
        lexerTest("a b",2,tokens=>Write(()=>tokens.Select(_=>new {_, _.LeadingTrivia, _.TrailingTrivia})));
        
        lexer = lexer.WithTriviaRule(TokenRule.EndOfLine);
        lexerTest("a\r\n b",2);

        lexer = lexer.WithTokenRules(TokenRule.AtSymbol,
        TokenRule.LessThan,
        TokenRule.GreaterThan,
        TokenRule.SemiColon,
        TokenRule.Colon,
        TokenRule.OpenBrace,
        TokenRule.OpenParenthesis,
        TokenRule.CloseBrace,
        TokenRule.CloseParenthesis,
        TokenRule.Comma);
        lexerTest("a;<>,\r\n@b:{}(c)",13);

        lexer = lexer.WithTokenRule(TokenRule.StringLiteral);
        lexerTest(@"a""b{};\""\ \\""c",3); // a, "b{};\"\ \", c

        lexer = lexer.WithTokenRule(TokenRule.Dots);
        lexerTest(".a ..b ...c",6); //., a, .., b, ..., c

        lexer = lexer.WithTokenRule(TokenRule.NumericLiteral);
        lexerTest("a 9.0.b c",5); // a, 9.0, ., b, c
    }


    //# Default Lexer Test
    [TestMethod]
    public void DefaultLexerTest()
    {
        lexer = AtLexer.DefaultLexer;
        lexerTest("(5)",3);
        lexerTest("@X<>",4);
        lexerTest("'Hello world!'",1);        
    }

    //# Numeric Literal Test
    [TestMethod]
    public void NumericLiteralTest()
    {
        lexer = lexer.WithTokenRule(TokenRule.NumericLiteral);

        foreach(var n in numericLiterals())
        {
            var token = lexer.Lex(n).First();
            assert_not_null(()=>token);
            assert_equals(TokenKind.NumericLiteral,()=>token.Kind);
            assert_equals(n,()=>token.Text);
        }
    }

    IList<AtToken> lexerTest(string input, int? expectedTokenCount = null, Action<IList<AtToken>> a = null)
    {
        var tokens = lexer.Lex(input);
        var tokenList = tokens.ToList();

        Write(()=>input);
        Write(()=>tokenList);

        if (expectedTokenCount != null)
            assert_equals(expectedTokenCount,()=>tokenList.Count);        

        if (a != null)
            a(tokenList);            

        return tokenList;
    }

    string[] numericLiterals() => new []
    {
        "0",
        "1",
        "2.5",
        "10",
        "3.6",
        "0.0008"
    }; 
}
}
