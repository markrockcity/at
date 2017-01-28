
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using atSyntax = At.Syntax;
using Microsoft.CodeAnalysis;
using System.Threading;
using System.Linq;
using At.Targets.CSharp;

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
            Write(()=>input);
            var tree = AtSyntaxTree.ParseText(input);            
            var cSharpTree = new CSharpSyntaxTreeConverter(tree.GetRoot()).ConvertToCSharpTree();
                verifyOutput(cSharpTree,className,(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax _)=> _.Identifier.Text);
        }
    }

    [TestMethod] 
    public  void SyntaxTreeConverterTest1()
    {
        var input = "@A<B,C> : D<int,B> {} \r\n @D<E,F>";
        var tree = AtSyntaxTree.ParseText(input);            
        var cSharpTree = new CSharpSyntaxTreeConverter(tree.GetRoot()).ConvertToCSharpTree();
        Write(cSharpTree.GetRoot(new CancellationToken()).NormalizeWhitespace("    ",false).ToString());
    
        //public class A ...
        var csClass = cSharpTree.GetRoot().DescendantNodes().OfType<csSyntax.ClassDeclarationSyntax>().FirstOrDefault(_=>_.Identifier.Text=="A");
        assert_not_null(()=>csClass);

        //... : D<int,B>
        var genType = csClass.BaseList.Types[0].Type as csSyntax.GenericNameSyntax;
        assert_not_null(()=>genType);
        assert_equals(2,()=>genType.Arity);
    }


}
}
