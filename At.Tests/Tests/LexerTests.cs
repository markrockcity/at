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


        lexer.TriviaDefinitions.Add(TokenDefinition.StartOfFile);
        lexerTest("x", 1,tokens =>
        {
            Write(()=>tokens[0].leadingTrivia);
            var sof = tokens[0].leadingTrivia.Single();
            assert_equals(TokenKind.StartOfFile,sof.Kind);
        });  
        
        lexer.TokenDefinitions.Add(TokenDefinition.StartOfFile);  
        lexerTest("x",expectedTokenCount: 2); // <StartOfFile> & 'x'

        lexer.TriviaDefinitions.Add(TokenDefinition.EndOfFile);
        lexerTest("x",a:tokens=>
        {            
            Write(()=>tokens[1].trailingTrivia);
            var eof = tokens[1].trailingTrivia.Single();
            assert_equals(TokenKind.EndOfFile,eof.Kind);  
        });

        lexer.TokenDefinitions.Add(TokenDefinition.EndOfFile);   
        lexerTest("x",3); // <StartOfFile> & 'x' & <EOF>

        lexer.TokenDefinitions.Clear();
        lexer.TriviaDefinitions.Add(TokenDefinition.Space);   
        lexerTest("a b",2,tokens=>Write(()=>tokens.Select(_=>new {_, _.leadingTrivia, _.trailingTrivia})));
        
        lexer.TriviaDefinitions.Add(TokenDefinition.EndOfLine);
        lexerTest("a\r\n b",2);

        lexer.TokenDefinitions.Add(TokenDefinition.AtSymbol);
        lexer.TokenDefinitions.Add(TokenDefinition.LessThan);
        lexer.TokenDefinitions.Add(TokenDefinition.GreaterThan);
        lexer.TokenDefinitions.Add(TokenDefinition.SemiColon);
        lexer.TokenDefinitions.Add(TokenDefinition.Colon);
        lexer.TokenDefinitions.Add(TokenDefinition.OpenBrace);
        lexer.TokenDefinitions.Add(TokenDefinition.OpenParenthesis);
        lexer.TokenDefinitions.Add(TokenDefinition.CloseBrace);
        lexer.TokenDefinitions.Add(TokenDefinition.CloseParenthesis);
        lexer.TokenDefinitions.Add(TokenDefinition.Comma);
        lexerTest("a;<>,\r\n@b:{}(c)",13);

        lexer.TokenDefinitions.Add(TokenDefinition.StringLiteral);
        lexerTest(@"a""b{};\""\ \\""c",3); // a, "b{};\"\ \", c

        lexer.TokenDefinitions.Add(TokenDefinition.Dots);
        lexerTest(".a ..b ...c",6); //., a, .., b, ..., c
    }


    //# Numeric Literal Test
    [TestMethod]
    public void NumericLiteralTest()
    {
        foreach(var n in numericLiterals())
        {
            var token = lexer.Lex(n).First(); //first token = <StartOfFile>
            assert_not_null(()=>token);
            assert_equals(TokenKind.NumericLiteral,()=>token.Kind);
            assert_equals(n,()=>token.Text);
        }
    }

    IList<AtToken> lexerTest(string input, int? expectedTokenCount = null, Action<IList<AtToken>> a = null)
    {
        var tokens = lexer.Lex(input).ToList();

        Write(()=>input);
        Write(()=>tokens);

        if (expectedTokenCount != null)
            assert_equals(expectedTokenCount,()=>tokens.Count);        

        if (a != null)
            a(tokens);            

        return tokens;
    }


    string[] numericLiterals() => new []
    {
        "0",
        "-1",
        "2.5",
        "10",
        "+3.6",
        "-0.0008"
    }; 
}
}
