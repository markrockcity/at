
using Microsoft.VisualStudio.TestTools.UnitTesting;
using At.Syntax;

namespace At.Tests
{
    [TestClass]
public class ParserTests : AtTest
{
    //ImportTest
    [TestMethod]
    public void ImportDirectiveTest()
    {
        var nsid  = identifier();
        var inputs = new[] 
        { 
            $"#import {nsid}",
            $"#import {nsid};",
        };

        foreach(var input in inputs)
        {
            var tree  = parseTree(input);
            verifyOutput<DirectiveSyntax>(input,tree,nsid,_=>_.Name.Identifier.Text);
        }
    } 

    //LiteralTest()
    [TestMethod]
    public void LiteralTest()
    {
        var x = "5.0";
        var t = parseTree(x);
        verifyOutput<LiteralExpressionSyntax>(x,t,x,n=>n.Text);
    }
}
}
