using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Linq;
using System;
using At.Syntax;
using System.Collections.Generic;
using CSharp = Microsoft.CodeAnalysis.CSharp;

namespace At.Tests
{
[TestClass] public class MiscTests : Test
{

    //Compile String To Assembly
    [TestMethod] public void CompileStringToAssemblyTest()
    {
        using (var testData = new TestData(this))
        {
            var className1 = testData.Identifier(0);
            var baseClass1 = testData.Identifier(1);
            var className2 = testData.Identifier(2);

            var input = $"@{className1}< T , U > : {baseClass1}<{className2}, T>{{ \r\n }}"+ // @X<>
                        $"@{baseClass1}<T, U>;"+
                        $"@{className2}<>"; 
            var output = AtProgram.compileStringToAssembly(input);
            verifyOutput(output, className1+"`2", className2);
        }
    }

    //Lexer
    [TestMethod] public void LexerTest()
    {
        var lexer  = new AtLexer(new AtSourceText("<>"));
        var tokens = lexer.Lex().ToList();
        var count  = 4; //<StartOfFile> + "<" + ">" + <EOF>
        assert_equals(count,()=>tokens.Count);
    }

    //Parse Text #1
    [TestMethod] public void ParseTextTest1()
    {

        using (var testData = new TestData(this))
        {
            var className = testData.Identifier(0);
            var baseClass = testData.Identifier(1);
              
            foreach(var input in inputs(className,baseClass))
            {
                var tree = AtSyntaxTree.ParseText(input);

                //TODO: verify that no ErrorNodes exist
                verifyOutput(input, tree,className);
            }
        }
    }

    
    //Parse Text #2
    [TestMethod] public void ParseTextTest2()
    {
        var input = "@class<> : y<> {@P<>}";
        var tree = AtSyntaxTree.ParseText(input);
        var root = tree.GetRoot();
        assert_not_null(()=>root);
        verifyOutput(input,tree,"class");
    }

        
    //Syntax Tree Converter
    [TestMethod] public void SyntaxTreeConverterTest()
    {
        using (var testData = new TestData(this))
        {
            var className = testData.Identifier(0);
            var baseClass = testData.Identifier(1);

            foreach(var input in inputs(className,baseClass))
            {
                var tree = AtSyntaxTree.ParseText(input);            
                var cSharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
                verifyOutput(cSharpTree,className);
            }
        }
    }


    //inputs
    IEnumerable<string> inputs(string className,string baseClass) => new[] 
    {
        $"@{className}<>",
        $"@{className}<>;",
        $"@{className}<>{{}}",
        $"@{className}<  > {{ \r\n }}",
        $"\r\n  @{className}<  > {{ \r\n }}\r\n\r\n  ",
        $"@{className}<T>",
        $"@{className}< T >",
        $"@{className}< T, U>",
        $"@{className}< T, U>",
        $"@{className}<T,U> : {baseClass}",
        $"@{className}<T,U> : {baseClass}<T>",
    };       

    //verify output (assembly)
    void verifyOutput(Assembly assembly, params string[] classNames) 
    {
        assert_not_null(()=>assembly);

        var types = assembly.GetTypes();

        foreach(var className in classNames)
            assert_true(()=>types.Any(_=>_.Name==className&&_.IsClass));
    }

    //verify output (syntax tree)
    void verifyOutput(string input, AtSyntaxTree tree,string className)
    {
        assert_not_null(()=>tree);

        assert_equals(()=>0,()=>tree.GetDiagnostics().Count());

        var root = tree.GetRoot();
        assert_equals(()=>input,()=>root.FullText);

        var classDecl = (TypeDeclarationSyntax) root.Nodes().First();
        assert_equals(()=>className, ()=>classDecl.Identifier.Text);
        
    }

    //verify output (C# tree)
    void verifyOutput(CSharp.CSharpSyntaxTree tree,string className)
    {
        var root = tree.GetRoot();
        var any = root.DescendantNodes()
                        .OfType<CSharp.Syntax.ClassDeclarationSyntax>()
                        .Any(_=>_.Identifier.Text == className);
        assert_true(any);                    
    }
}
}
