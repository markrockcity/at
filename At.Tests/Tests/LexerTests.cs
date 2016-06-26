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
        var lexer  = new AtLexer(new AtSourceText("<>"));
        var tokens = lexer.Lex().ToList();
        var count  = 4; //<StartOfFile> + "<" + ">" + <EOF>
        assert_equals(count,()=>tokens.Count);
    }

    //# Numeric Literal Test
    [TestMethod]
    public void NumericLiteralTest()
    {
        foreach(var n in numericLiterals())
        {
            var lexer = new AtLexer(n);
            var token = lexer.Lex().Skip(1).First(); //first token = <StartOfFile>
            assert_not_null(()=>token);
            assert_equals(TokenKind.NumericLiteral,()=>token.Kind);
            assert_equals(n,()=>token.Text);
        }
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
