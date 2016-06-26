using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using atSyntax = At.Syntax;
using cs = Microsoft.CodeAnalysis.CSharp;

namespace At.Tests
{
public class AtTest : Test
{

    //verify output (assembly)
    protected void verifyOutput(Assembly assembly, params string[] ids) 
    {
        assert_not_null(()=>assembly);

        var types = assembly.GetTypes();

        foreach(var className in ids)
            assert_true(()=>types.Any(_=>_.Name==className&&_.IsClass), ()=>types);
    }

    //verify output (syntax tree - declaration)
    protected T verifyOutput<T>(string input, AtSyntaxTree tree, string id) where T : atSyntax.DeclarationSyntax
    {
        return verifyOutput<T>(input,tree,id,decl=>decl.Identifier.Text);
    }

    //verify output (syntax tree)
    protected T verifyOutput<T>(string input, AtSyntaxTree tree, string expectedId, Func<T,string> actualId)  where T : AtSyntaxNode
    {
        assert_not_null(()=>tree);

        assert_equals(()=>0,()=>tree.GetDiagnostics().Count(),"Syntax tree contains diagnostics: {0}",tree.GetDiagnostics().FirstOrDefault()?.Message);

        var root = tree.GetRoot();
        assert_equals(()=>input,()=>root.FullText);

        var node = root.DescendantNodes().OfType<T>().First();
        assert_equals(()=>expectedId, ()=>actualId(node));

        return node;
    }

    //verify output (C# tree)
    protected void verifyOutput<T>(cs.CSharpSyntaxTree csharpTree,
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

        Write(root);
        
        if (id2!=null && getId2!=null)
        {
            assert_equals(id2,getId2(any));
        }                    
    }

    protected string identifier(int? i = null) => TestData.Identifier();
    protected AtSyntaxTree parseTree(string input) => AtSyntaxTree.ParseText(input);
}
}
