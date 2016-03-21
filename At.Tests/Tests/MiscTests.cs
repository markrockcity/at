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
    public void LexerTest()
    {
        var lexer  = new AtLexer(new AtSourceText("<>"));
        var tokens = lexer.Lex().ToList();
        var count  = 4; //<StartOfFile> + "<" + ">" + <EOF>
        assert_equals(count,()=>tokens.Count);
    }


    [TestMethod]
    public void ParseTextTest()
    {

        using (var testData = new TestData(this))
        {
            var className = testData.Identifier(0);
            var baseClass = testData.Identifier(1);

            var inputs = new[] 
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
              
            foreach(var input in inputs)
            {
                var tree = AtSyntaxTree.ParseText(input);

                //TODO: verify that no ErrorNodes exist
                verifyOutput(input, tree,className);
            }
        }
    }

        
   [TestMethod] //TODO: className<T, U>
    public void CompileStringToAssemblyTest()
    {
        using (var testData = new TestData(this))
        {
            var className = testData.Identifier(0);
            var baseClass = testData.Identifier(1);

            var input = $"@{className}< T , U > : {baseClass}<T>{{ \r\n }}"+ // @X<>
                        $"@{baseClass}<X>;"; 
            var output = AtProgram.compileStringToAssembly(input);
            verifyOutput(output, className+"`2");
        }
    }

    void verifyOutput(Assembly assembly, string className) 
    {
        assert_not_null(()=>assembly);
        assert_true(()=>assembly.GetTypes().Any(_=>_.Name==className&&_.IsClass));
    }

    void verifyOutput(string input, AtSyntaxTree tree,string className)
    {
        assert_not_null(()=>tree);

        assert_equals(()=>0,()=>tree.GetDiagnostics().Count());

        var root = tree.GetRoot();
        assert_equals(()=>input,()=>root.FullText);

        var classDecl = (ClassDeclarationSyntax) root.Nodes().First();
        assert_equals(()=>className, ()=>classDecl.Identifier.Text);
        
    }
}
}
