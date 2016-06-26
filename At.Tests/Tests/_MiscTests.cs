using Microsoft.VisualStudio.TestTools.UnitTesting;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using atSyntax = At.Syntax;

namespace At.Tests
{
[TestClass] public class MiscTests : AtTest
{

    //Parse Text Test #1
    [TestMethod] 
    public void ParseTextTest1()
    {
        var className = identifier(0);
        var baseClass = identifier(1);
              
        foreach(var input in TestData.classInputs(className,baseClass))
        {
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

    //Method Test
    [TestMethod] 
    public void MethodTest()
    {
        var id = identifier();
        var input = $"@{id}();";
        var tree = parseTree(input);
        verifyOutput<atSyntax.MethodDeclarationSyntax>(input,tree,id);

        var csharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
        verifyOutput<csSyntax.MethodDeclarationSyntax>(csharpTree,id,_=>_.Identifier.Text);
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
 
            var csharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
            verifyOutput<csSyntax.FieldDeclarationSyntax>(csharpTree,
                id,_=>_.Declaration.Variables[0].Identifier.Text,
                decl.Type?.Text ?? "object",_=>_.Declaration.Type.ToString());
        }
    }
}
}
