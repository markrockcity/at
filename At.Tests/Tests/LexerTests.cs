using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace At.Tests
{
    [TestClass]
public class LexerTests : Test
{
    //Lexer Test
    [TestMethod] 
    public void LexerTest1()
    {
        var lexer  = new AtLexer(new AtSourceText("<>"));
        var tokens = lexer.Lex().ToList();
        var count  = 4; //<StartOfFile> + "<" + ">" + <EOF>
        assert_equals(count,()=>tokens.Count);
    }
}
}
