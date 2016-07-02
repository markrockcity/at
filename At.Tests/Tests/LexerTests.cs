using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace At.Tests
{
    [TestClass]
public class LexerTests : Test
{
    //# Lexer Test
    [TestMethod] 
    public void LexerTest1()
    {
        assert_equals(KnownTokenKind.TokenCluster,()=>(KnownTokenKind)lexerTest("<>",1).Single().RawKind);
        lexerTest("@x = 5\r\n5",1);

        var lexer  = new AtLexer();

        lexer.TriviaDefinitions.Add(KnownTokenKind.StartOfFile);
        var tokens = lexer.Lex("x").ToList();
        assert_equals(1,()=>tokens.Count);
        Write(()=>tokens[0].leadingTrivia);
        var sof = tokens[0].leadingTrivia.Single();
        assert_equals((int)KnownTokenKind.StartOfFile,sof.RawKind);    
        
        lexer.TokenDefinitions.Add(KnownTokenKind.StartOfFile);   
        tokens =  lexer.Lex("x").ToList();
        Write(()=>tokens);
        assert_equals(2,()=>tokens.Count);// <StartOfFile> & 'x'

        lexer.TriviaDefinitions.Add(KnownTokenKind.EndOfFile);
        tokens =  lexer.Lex("x").ToList();
        Write(()=>tokens[1].trailingTrivia);
        var eof = tokens[1].trailingTrivia.Single();
        assert_equals((int)KnownTokenKind.EndOfFile,eof.RawKind);  


        lexer.TokenDefinitions.Add(KnownTokenKind.EndOfFile);   
        tokens =  lexer.Lex("x").ToList();
        Write(()=>tokens);
        assert_equals(3,()=>tokens.Count);// <StartOfFile> & 'x' & <EOF>
    }

    //# Numeric Literal Test
    [TestMethod]
    public void NumericLiteralTest()
    {
        foreach(var n in numericLiterals())
        {
            var lexer = new AtLexer();
            var token = lexer.Lex(n).First(); //first token = <StartOfFile>
            assert_not_null(()=>token);
            assert_equals(KnownTokenKind.NumericLiteral,()=>(KnownTokenKind)token.RawKind);
            assert_equals(n,()=>token.Text);
        }
    }

    IList<AtToken> lexerTest(string input, int expectedTokenCount)
    {
        var lexer  = new AtLexer();
        var tokens = lexer.Lex(input).ToList();
        var count  = expectedTokenCount;
        assert_equals(count,()=>tokens.Count);
        return tokens.ToList();
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
