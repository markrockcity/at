using Microsoft.VisualStudio.TestTools.UnitTesting;
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

}
}
