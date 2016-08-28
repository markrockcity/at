
using Microsoft.VisualStudio.TestTools.UnitTesting;
using At.Syntax;

namespace At.Tests
{
[TestClass]
public class ParserTests : AtTest
{
    AtParser parser;

    protected override void Setup()
    {
        base.Setup();
        parser = new AtParser(AtLexer.Default());
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
        
        //TODO: implement CurlyBlock postcircumfix op &  MethodDeclaration rule
        var expr2 = parser.ParseExpression("@X()");
        assert_type<MethodDeclarationSyntax>(()=>expr2);    

        //TODO: implement PointyBlock postcircumfix op & TypeDeclaration rule
        var expr3 = parser.ParseExpression("@X<>");
        assert_type<TypeDeclarationSyntax>(()=>expr3);    
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
}
}
