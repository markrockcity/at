using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Linq;
using System;
using At.Syntax;

namespace At.Tests
{
[TestClass] public class MiscTests : Test
{

    [TestMethod]
    public void ParseTextTest()
    {
        using (var testData = new TestData(this))
        {
            var className = testData.Identifier();  
            var input = $"@{className}<>"; // @X<>   
            var tree = AtSyntaxTree.ParseText(input);
            verifyOutput(tree,className);
        }
    }

        
   [TestMethod]
    public void CompileStringToAssemblyTest()
    {
        using (var testData = new TestData(this))
        {
            var className = testData.Identifier();
            var input = $"@{className}<>"; // @X<>
            var output = AtProgram.compileStringToAssembly(input);
            verifyOutput(output, className);
        }
    }

    void verifyOutput(Assembly assembly, string className) 
    {
        assert_not_null(()=>assembly);
        assert_true(()=>assembly.GetTypes().Any(_=>_.Name==className&&_.IsClass));
    }

    void verifyOutput(AtSyntaxTree tree,string className)
    {
        assert_not_null(()=>tree);

        var root = tree.GetRoot();
        var classDecl = (ClassDeclarationSyntax) root.Nodes[0];
        assert_equals(()=>className, ()=>classDecl.Name);
        
    }
}
}
