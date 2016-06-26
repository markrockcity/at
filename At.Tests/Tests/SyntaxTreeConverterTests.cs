
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using atSyntax = At.Syntax;

namespace At.Tests
{
    [TestClass]
public class SyntaxTreeConverterTests : AtTest
{
    //Syntax Tree Converter Test
    [TestMethod] 
    public void MiscSyntaxTreeConverterTest()
    {
        var className = identifier(0);
        var baseClass = identifier(1);

        foreach(var input in TestData.classInputs(className,baseClass))
        {
            var tree = AtSyntaxTree.ParseText(input);            
            var cSharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
            verifyOutput<csSyntax.ClassDeclarationSyntax>(cSharpTree,className,_=>_.Identifier.Text);
        }

    }


}
}
