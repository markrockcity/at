
using Microsoft.VisualStudio.TestTools.UnitTesting;
using At.Syntax;
using System.Linq;

namespace At.Tests
{
[TestClass]
public class SyntaxPatternTests : AtTest
{
    AtParser parser;

    protected override void Setup()
    {
        base.Setup();
        

    }

    //Pattern Strings test 1
    [TestMethod]
    public void PatternStringsTest()
    {
        var expr0 = SyntaxFactory.ParseExpression("x;");
        Write(()=>expr0);
        foreach(var e in expr0.PatternStrings())
            Write(e);

        Write("----------------------------------");


        var expr1 = SyntaxFactory.ParseExpression("(t)");
        Write(()=>expr1);
        foreach(var e in expr1.PatternStrings())
            Write(e);

        Write("----------------------------------");

        var expr2 = SyntaxFactory.ParseExpression("u<t>");
        Write(()=>expr2);
        foreach(var e in expr2.PatternStrings())
            Write(e);

            
        Write("----------------------------------");
    
        var expr3 = SyntaxFactory.ParseExpression("x : u<t>");
        Write(()=>expr3);
        foreach(var e in expr3.PatternStrings())
            Write(e);
    }


    [TestMethod]
    public void SyntaxPatternTest1()
    {
        var str1 = "TokenCluster(AtSymbol),Binary[Colon](TokenCluster,Expr)";
        var p1 = SyntaxFactory.ParseSyntaxPattern(str1);
        assert_equals(()=>str1,()=>p1.ToString());


        var str2 = "TokenCluster('X')";
        var p2 = SyntaxFactory.ParseSyntaxPattern(str2);
        assert_equals(()=>str2,()=>p2.ToString());

        var str3 = "Binary[Colon](Expr,Expr)";
        var p3 = SyntaxFactory.ParseSyntaxPattern(str3);
        assert_equals(()=>str3,()=>p3.ToString());
        assert_equals(()=>"Colon",()=>p3.Token1);
        //Write(()=>expr1);
        //var ps = expr1.PatternStrings().First();
        //Write(()=>ps);

        var str4 = "Binary[Colon](TokenCluster,TokenCluster('namespace'))";
        var p4 = SyntaxFactory.ParseSyntaxPattern(str4);
        var e4 = SyntaxFactory.ParseExpression("A:B");
        assert_equals(()=>str4,()=>p4.ToString());
        assert_false(()=>e4.MatchesPattern(p4));
    }

    [TestMethod]
    public void SyntaxPatternKeyTest()
    {
        var str1 = "x:A";
        var p1 = SyntaxFactory.ParseSyntaxPattern(str1);
        assert_equals(()=>"x",()=>p1.Key);
    }
}
}