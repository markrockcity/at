using Microsoft.VisualStudio.TestTools.UnitTesting;
using atSyntax = At.Syntax;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

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
            verifyOutput<atSyntax.DirectiveSyntax>(input,tree,nsid,_=>_.Name.Identifier.Text);
        }
    } 

    //Method Test
    [TestMethod] 
    public void MethodTest()
    {
        var id = identifier();
        var input = $"@{id}();";
        var tree = AtSyntaxTree.ParseText(input);
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
            var tree = AtSyntaxTree.ParseText(input); //@x
            var decl = verifyOutput<atSyntax.VariableDeclarationSyntax>(input,tree,id);
 
            var csharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
            verifyOutput<csSyntax.FieldDeclarationSyntax>(csharpTree,
                id,_=>_.Declaration.Variables[0].Identifier.Text,
                decl.Type?.Text ?? "object",_=>_.Declaration.Type.ToString());
        }
    }

    AtSyntaxTree parseTree(string input) => AtSyntaxTree.ParseText(input);
}
}
