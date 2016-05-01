using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;
using atSyntax = At.Syntax;
using cs = Microsoft.CodeAnalysis.CSharp;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace At.Tests
{
[TestClass] public class MiscTests : Test
{
    //Compile-String-To-Assembly Test
    [TestMethod] public void CompileStringToAssemblyTest()
    {
        var className1 = TestData.Identifier(0);
        var baseClass1 = TestData.Identifier(1);
        var className2 = TestData.Identifier(2);
        var variableName1 = TestData.Identifier(3);
        var variableName2 = TestData.Identifier(4);
        var className3 = TestData.Identifier(5);
        var functionName1 = TestData.Identifier(6);

        var input = $"@{className1}< T , U > : {baseClass1}<{className2}, T>{{ \r\n @P<>; @G() }}\r\n"+ 
                    $"@{baseClass1}<T, U>;\r\n"+
                    $"@{className2}<>;"+
                    $"@{variableName1} : {className1}<{className2},{className2}>;"+
                    $"@{functionName1}()";
        var output = AtProgram.compileStringToAssembly(input);

        verifyOutput(output, className1+"`2", className2, baseClass1+"`2", "P");

        var class1 = output.GetType(className1+"`2");

        assert_not_null(()=>class1.GetNestedType("P"),ifFail:()=>class1.GetNestedTypes());
        assert_not_null(()=>class1.GetMethod("G"),ifFail:()=>class1.GetMethods());

        var _ =  output.GetType(SyntaxTreeConverter.defaultClassName);
        var variable = _.GetField(variableName1);

        assert_not_null(()=>variable,ifFail:()=>_.GetFields());
        assert_not_null(()=>_.GetMethod(functionName1),()=>_.GetMethods());
        assert_equals(()=>$"{className1}`2",()=>variable.FieldType.Name);
    }

    //Method Test
    [TestMethod] public void MethodTest()
    {
        var id = TestData.Identifier();
        var input = $"@{id}();";
        var tree = AtSyntaxTree.ParseText(input);
        verifyOutput<atSyntax.MethodDeclarationSyntax>(input,tree,id);

        var csharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
        verifyOutput<csSyntax.MethodDeclarationSyntax>(csharpTree,id,_=>_.Identifier.Text);
    }

    //Lexer Test
    [TestMethod] public void LexerTest()
    {
        var lexer  = new AtLexer(new AtSourceText("<>"));
        var tokens = lexer.Lex().ToList();
        var count  = 4; //<StartOfFile> + "<" + ">" + <EOF>
        assert_equals(count,()=>tokens.Count);
    }

    //Parse Text Test #1
    [TestMethod] public void ParseTextTest1()
    {
        var className = TestData.Identifier(0);
        var baseClass = TestData.Identifier(1);
              
        foreach(var input in classInputs(className,baseClass))
        {
            var tree = AtSyntaxTree.ParseText(input);

            //TODO: verify that no ErrorNodes exist
            verifyOutput<atSyntax.TypeDeclarationSyntax>(input, tree,className);
        }
    }

    
    //Parse Text Test #2 
    [TestMethod] public void ParseTextTest2()
    {
        var input = "@class<> : y<> {@P<>}";
        var tree = AtSyntaxTree.ParseText(input);
        var root = tree.GetRoot();
        assert_not_null(()=>root);
        verifyOutput<atSyntax.TypeDeclarationSyntax>(input,tree,"class");
    }

        
    //Syntax Tree Converter Test
    [TestMethod] public void SyntaxTreeConverterTest()
    {
       
        var className = TestData.Identifier(0);
        var baseClass = TestData.Identifier(1);

        foreach(var input in classInputs(className,baseClass))
        {
            var tree = AtSyntaxTree.ParseText(input);            
            var cSharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
            verifyOutput<csSyntax.ClassDeclarationSyntax>(cSharpTree,className,_=>_.Identifier.Text);
        }

    }


    //Variable Test
    [TestMethod] public void VariableTest()
    {
        var id = TestData.Identifier(0);
        var className = TestData.Identifier(1);

        foreach(var input in variableInputs(id,className))
        {
            var tree = AtSyntaxTree.ParseText(input); //@x
            var decl = verifyOutput<atSyntax.VariableDeclarationSyntax>(input,tree,id);
 
            var csharpTree = new SyntaxTreeConverter(tree).ConvertToCSharpTree();
            verifyOutput<csSyntax.FieldDeclarationSyntax>(csharpTree,
                id,_=>_.Declaration.Variables[0].Identifier.Text,
                decl.Type?.Text ?? "System.Object",_=>_.Declaration.Type.ToString());
        }
    }

    //inputs
    IEnumerable<string> classInputs(string className,string baseClass) => new[] 
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
        $"@{className}<T,U> : {baseClass};",
        $"@{className}<T,U> : {baseClass}<T>",
        $"@{className}<T,U> : {baseClass}<T>;",
    };       
    IEnumerable<string> variableInputs(string id, string className) => new[]
    {
        $"@{id}",
        $"@{id};",
        $"@{id} : {className}",
        $"@{id} : {className};",
        $"@{id} : {className}<{className}>",
        $"@{id} : {className}<{className},{className}>;",
        $"@{id} : {className}<{className},{className}<{className}>>",
    };


    //verify output (assembly)
    void verifyOutput(Assembly assembly, params string[] ids) 
    {
        assert_not_null(()=>assembly);

        var types = assembly.GetTypes();

        foreach(var className in ids)
            assert_true(()=>types.Any(_=>_.Name==className&&_.IsClass), ()=>types);
    }

    //verify output (syntax tree)
    T verifyOutput<T>(string input, AtSyntaxTree tree, string id) where T : atSyntax.DeclarationSyntax
    {
        assert_not_null(()=>tree);

        assert_equals(()=>0,()=>tree.GetDiagnostics().Count());

        var root = tree.GetRoot();
        assert_equals(()=>input,()=>root.FullText);

        var declaration = (T) root.DescendantNodes().First();
        assert_equals(()=>id, ()=>declaration.Identifier.Text);

        return declaration;
    }

    //verify output (C# tree)
    void verifyOutput<T>(cs.CSharpSyntaxTree csharpTree,
                         string id,
                         Func<T,string> getId,
                         string id2 = null,
                         Func<T,string> getId2 = null) 
        where T : cs.Syntax.MemberDeclarationSyntax
    {
        var root = csharpTree.GetRoot();
        var any = root.DescendantNodes()
                        .OfType<T>()
                        .Single(_=>getId(_)==id);
        assert_not_null(any);
        
        if (id2!=null && getId2!=null)
        {
            assert_equals(id2,getId2(any));
        }                    
    }
}
}
