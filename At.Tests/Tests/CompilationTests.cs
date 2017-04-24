using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using At.Binding;
using At.Contexts;
using At.Targets.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace At.Tests
{
    [TestClass]
public  class CompilationTests : AtTest
{
    //Compile-String-To-Assembly Test
    [TestMethod] 
    public void CompileStringToAssemblyTest()
    {
        var className1 = TestData.Identifier(0);
        var baseClass1 = TestData.Identifier(1);
        var className2 = TestData.Identifier(2);
        var variableName1 = TestData.Identifier(3);
        var variableName2 = TestData.Identifier(4);
        var className3 = TestData.Identifier(5);
        var functionName1 = TestData.Identifier(6);
        var ns = TestData.Identifier(7);

        var input = "#import System;\r\n"+
                    $"@{className1}< T , U > : {baseClass1}<{className2}, T>{{ \r\n @P<>; @G() }}\r\n"+ 
                    $"@{baseClass1}<T, U>;\r\n"+
                    $"@{className2}<>;"+
                    $"@{variableName1} : {className1}<{className2},{className2}>;"+
                    $"@{functionName1}()"+
                    $"@{ns} : namespace {{@{functionName1}(); @{@className1}<> }}"+
                    "@ns1 : namespace  {@f(); @variable : y; @y<>; @class<>  : y {@P<>;@G()}}"
                    ;

        var output = compileToAssembly(input);
        verifyOutput(output, className1+"`2", className2, baseClass1+"`2", "P");

        var class1 = output.GetType(className1+"`2");

        assert_not_null(()=>class1.GetNestedType("P"),ifFail:()=>class1.GetNestedTypes());
        assert_not_null(()=>class1.GetMethod("G"),ifFail:()=>class1.GetMethods());    

        var _ =  output.GetType(CSharpSyntaxTreeConverter.defaultClassName);
        var variable = _.GetField(variableName1);

        assert_not_null(()=>variable,ifFail:()=>_.GetFields());
        assert_not_null(()=>_.GetMethod(functionName1),ifFail:()=>_.GetMethods());
        assert_equals(()=>$"{className1}`2",()=>variable.FieldType.Name);
    }

    [TestMethod] 
    public void CompileStringTest1()
    {
        var input = "@A<B,C> : D<int,B> { @E<B,C> : D<int,B> {} }; @D<E,F>";
        var output = compileToAssembly(input);
        assert_not_null(()=>output);
    }

    [TestMethod] 
    public void CompileExpressionTest()
    {
        var input = "1 + 2 * (2 + 1)";
        var output = compileToAssembly(input);
        assert_not_null(()=>output);
        assert_equals("7\r\n",()=>getConsoleOutput(output));
    }


    [TestMethod] 
    public void CompileHelloWorldTest()
    {
        var input = "output 'Hello World!'";
        var output = compileToAssembly(input);
        assert_not_null(()=>output);
        assert_equals("Hello World!\r\n",()=>getConsoleOutput(output));
    }

    [TestMethod] 
    public void CompileFunctionTest()
    {
        var input = "@add(a,b) { a + b } output add(5,6)";

        var tree = AtSyntaxTree.ParseText(input);
        var compilation = AtCompilation.Create(tree);
        var r = AtCompiler.Bind(compilation,new DiagnosticsBag(),new System.Threading.CancellationToken());
        var ctx1 = r.Context.Contents().OfType<CompilationUnit>().Single();
        var dc = ctx1.Contents().OfType<DeclarationContext>().Single();

        assert_equals(2, ()=>((MethodDeclaration)dc.Declaration).Parameters.Length, "Method parameters don't match");

        Write(()=>dc.Definition.Contents());

        var output = compileToAssembly(input);
        assert_not_null(()=>output);
        assert_equals("11\r\n",()=>getConsoleOutput(output));
    }


    private Assembly compileToAssembly(string input)
    {
        Assembly output = null;
        
        try
        {
            output = AtProgram.compileStringToAssembly(input);
        }
        catch(CompilationException ex)
        {
            Assert.Fail($"\r\n{input}\r\n\r\n{ex}");
        }   
        
        return output; 
    }

    private string getConsoleOutput(Assembly a)
    {
        var _ = Console.Out;
        var sb = new StringBuilder();
        var tw = new StringWriter(sb);
        Console.SetOut(tw);
        a.EntryPoint.Invoke(null,null);
        Console.SetOut(_);
        return sb.ToString();
    }
}
}
