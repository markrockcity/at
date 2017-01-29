using System.Linq;
using At.Targets.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using atSyntax = At.Syntax;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace At.Tests
{
[TestClass] 
public class MiscTests : AtTest
{

    //Hello World Test
    [TestMethod] 
    public void HelloWorldTest()
    {
        var p  = AtParser.CreateDefaultParser();
    
        var s1 = "output \"Hello, World!\"";
        var e  = p.ParseExpression(s1);

        Write(()=>e);

        verifyOutput<atSyntax.LiteralExpressionSyntax>(s1,e);
    }

    //Binary Operation Test
    [TestMethod]
    public void BinaryOperationTest()
    {
        var p  = AtParser.CreateDefaultParser();
    
        var s1 = "1 + 2 * 2";
        var e  = p.ParseExpression(s1);

        Write(()=>e.PatternStrings().First());

        verifyOutput<atSyntax.BinaryExpressionSyntax>(s1,e);

        var be = e as atSyntax.BinaryExpressionSyntax;
        assert_not_null(()=>be);

        verifyOutput<atSyntax.BinaryExpressionSyntax>("1 + 2",be.Left);
    }

    //Method Test
    [TestMethod] 
    public void MethodTest()
    {
        var id = identifier();
        var input = $"@{id}();";
        var atTree = parseTree(input);
        verifyOutput<atSyntax.MethodDeclarationSyntax>(input,atTree,id);

        var csharpTree = new CSharpSyntaxTreeConverter(atTree.GetRoot()).ConvertToCSharpTree();
        verifyOutput<csSyntax.MethodDeclarationSyntax>(csharpTree,id,_=>_.Identifier.Text);
    }

    //Parse Text Test #1
    [TestMethod] 
    public void ParseTextTest1()
    {
        var className = identifier(0);
        var baseClass = identifier(1);
              
        foreach(var input in TestData.classInputs(className,baseClass))
        {
            Write(()=>input);
            var tree = AtSyntaxTree.ParseText(input);
            verifyOutput<atSyntax.TypeDeclarationSyntax>(input, tree,className);
        }
    }

    
    //Parse Text Test #2 
    [TestMethod] 
    public void ParseTextTest2()
    {
        var input = "@ns : namespace {@f(); @class<> : y<> {@P<>} }";
        var tree  = AtSyntaxTree.ParseText(input);
        var root  = tree.GetRoot();
        assert_not_null(()=>root);
        verifyOutput<atSyntax.NamespaceDeclarationSyntax>(input,tree,"ns");
    }


    //Variable Test
    [TestMethod] public void VariableTest()
    {
        var id = TestData.Identifier(0);
        var className = TestData.Identifier(1);

        foreach(var input in TestData.variableInputs(id,className))
        {
            var tree = parseTree(input); //@x
            var decl = verifyOutput<atSyntax.VariableDeclarationSyntax>(input,tree,id);
 
            var csharpTree = new CSharpSyntaxTreeConverter(tree.GetRoot()).ConvertToCSharpTree();
            verifyOutput<csSyntax.FieldDeclarationSyntax>(csharpTree,
                id,_=>_.Declaration.Variables[0].Identifier.Text,
                decl.Type?.Text ?? "object",_=>_.Declaration.Type.ToString());
        }
    }

    //TypeDeclarationTest
    [TestMethod] public void TypeDeclarationTest()
    {
        var input = "@A<B,C> : D<E,F> {}";
        var tree = parseTree(input);

        var n = tree.GetRoot().DescendantNodes().OfType<atSyntax.TypeDeclarationSyntax>().First();
        Write(n);
        assert_equals(2,n.BaseTypes.List[0].TypeArguments.List.Count);
    }


    protected void verifyOutput<TNode>(string input, AtSyntaxNode node) where TNode : AtSyntaxNode
    {
        var tnodes = node.DescendantNodesAndSelf().OfType<TNode>();

        if (!tnodes.Any())
            Write($"No {typeof(TNode)} was found in node '{node}'");

        assert_equals(1, ()=>tnodes.Count());
        assert_equals(()=>input, ()=>node.FullText);
    }


}
}
