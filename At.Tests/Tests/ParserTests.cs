
using Microsoft.VisualStudio.TestTools.UnitTesting;
using At.Syntax;
using System.Linq;

namespace At.Tests
{
[TestClass]
public class ParserTests : AtTest
{
    AtParser parser;

    protected override void Setup()
    {
        base.Setup();
        parser = new AtParser(AtLexer.CreateDefaultLexer());
    }

    //Parser Test 1
    [TestMethod] 
    public void ParserTest1()
    {
        var compilationUnitSyntax = parser.ParseCompilationUnit("@X<>");
        Write(()=>compilationUnitSyntax);
        assert_equals(1,()=>compilationUnitSyntax.Expressions.Count);
        assert_type<ExpressionClusterSyntax>(()=>compilationUnitSyntax.Expressions[0]);

        //StartDeclaration (@...)
        parser.Operators.Add(0,OperatorDefinition.StartDeclaration.AddRule(_=>_.VariableDeclaration));
        parser.ExpressionRules.Add(ExpressionRule.TokenClusterSyntax);
        var expr1 = parser.ParseExpression("@X");
        assert_type<VariableDeclarationSyntax>(()=>expr1);  
        
        OperatorDefinition.StartDeclaration.AddRule(_=>_.MethodDeclaration);
        parser.Operators.Add(0,OperatorDefinition.PostRoundBlock);
        var expr2 = parser.ParseExpression("@X()");
        assert_type<MethodDeclarationSyntax>(()=>expr2);    

        OperatorDefinition.StartDeclaration.AddRule(_=>_.TypeDeclaration);
        parser.Operators.Add(0,OperatorDefinition.PostPointyBlock);
        var expr3 = parser.ParseExpression("@X<>");
        assert_type<TypeDeclarationSyntax>(()=>expr3);    
    }


    //Parser Test 2
    [TestMethod] 
    public void ParserTest2()
    {
        parser = AtParser.CreateDefaultParser();

        var s = "A<B,C> : D<E,F> {}";
        var e = parser.ParseExpression(s);

        Write(()=>e);
        Write(()=>e.PatternStrings().First());
    }

    //Parser Test 3 
    [TestMethod] 
    public void ParserTest3()
    {

        parser = AtParser.CreateDefaultParser();
        var s = "(5)";
        var e = parser.ParseExpression(s);
        assert_not_null(()=>e);
        
        Write(()=>e);
    }

    //ApplicationExpressionTest
    [TestMethod]
    public void ApplicationExpressionTest()
    {
        parser = AtParser.CreateDefaultParser();

        var s = "A B C";
        var e = parser.ParseExpression(s);

        Write(()=>e);
        Write(()=>e.PatternStrings().First());

        var ae = e as ApplicationSyntax;
        assert_not_null(ae);
        assert_equals(2,()=>ae.Arguments.Count); //"A(B,C)"
    }

    //CommaTest
    [TestMethod]
    public void CommaTest()
    {
         parser.Operators.Add(1,OperatorDefinition.Comma);
         var e1 = parser.ParseExpression("x,y");
         Write(e1.PatternStrings().First());
         assert_type<BinaryExpressionSyntax>(()=>e1);
    }

    //Circumfix test
    [TestMethod]
    public void CircumfixTest()
    {
        parser.Operators.Add(0,OperatorDefinition.RoundBlock);
        var e1 = parser.ParseExpression("()");
        assert_type<RoundBlockSyntax>(()=>e1);

        var e2 = parser.ParseExpression(" ( @x ) ");
        assert_type<RoundBlockSyntax>(()=>e2);

        var e3 = parser.ParseExpression("(()()())");
        assert_type<RoundBlockSyntax>(()=>e3);
    }

    //ImportTest
    [TestMethod]
    public void ParseHelloWorldTest()
    {
        parser = AtParser.CreateDefaultParser();
        var e1 = parser.ParseExpression("output 'Hello World!'");
        assert_type<ApplicationSyntax>(()=>e1);
    }

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

    //PostCircumfix test
    [TestMethod]
    public void PostCircumfixTest()
    {
        parser.Operators.Add(0,OperatorDefinition.PostRoundBlock);
        var e1 = parser.ParseExpression("x()");
        assert_type<PostBlockSyntax>(()=>e1);

        parser.Operators.Add(0,OperatorDefinition.PostPointyBlock);
        var e2 = parser.ParseExpression("x< y >");
        assert_type<PostBlockSyntax>(()=>e2);
    }

    //Type declaration test 1
    [TestMethod]
    public void TypeDeclarationTest1()
    {
        parser = AtParser.CreateDefaultParser();

        var s1 = "@X<A,B>";
        var t1 = parseTree(s1);
        verifyOutput<TypeDeclarationSyntax>(s1,t1,"X");

        var e1 = (TypeDeclarationSyntax) parser.ParseExpression(s1);
        assert_equals(()=>2,()=>e1.TypeParameters.List.Count);
        assert_equals(()=>"A",()=>e1.TypeParameters.List[0].Text);
        assert_equals(()=>"B",()=>e1.TypeParameters.List[1].Text);
    }

    //Type declaration test 2
    [TestMethod]
    public void TypeDeclarationTest2()
    {
        parser = AtParser.CreateDefaultParser();

        var s1 = "@A<B,C> : D<E,F> {}";

        //A<B,C>
        var e1 = (TypeDeclarationSyntax) parser.ParseExpression(s1);
        Write(()=>e1);
        assert_equals(()=>2,()=>e1.TypeParameters.List.Count);
        assert_equals(()=>"B",()=>e1.TypeParameters.List[0].Text);
        assert_equals(()=>"C",()=>e1.TypeParameters.List[1].Text);

        Write(e1.PatternStrings());

        // : D<E,F>
        var e2 = e1.BaseTypes.List[0];
        assert_equals(2,e2.TypeArguments?.List.Count);
        assert_equals(()=>"E",()=>e2.TypeArguments.List[0].Text);
        assert_equals(()=>"F",()=>e2.TypeArguments.List[1].Text);
    }
}
}
