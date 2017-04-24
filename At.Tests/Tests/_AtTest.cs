using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using At.Binding;
using At.Contexts;
using At.Symbols;
using Microsoft.CodeAnalysis;
using atSyntax = At.Syntax;
using cs = Microsoft.CodeAnalysis.CSharp;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace At.Tests
{
    public class AtTest : Test
{
    class TestBindingContext : Context
    {
        private AtTest test;

        public TestBindingContext(AtTest test,Context parentCtx=null,DiagnosticsBag diagnostics=null,ContextSymbol symbol = null,AtSyntaxNode syntaxNode = null) : base(parentCtx,diagnostics,symbol,syntaxNode)
        {
            this.test = test;
        }

        public override bool HasContents => throw new NotImplementedException();

        public override IEnumerable<IBindingNode> Contents()
        {
            throw new NotImplementedException();
        }

        protected internal override void AddNode(IBindingNode node)
        {
            //base.AddNode(node);
            test.Write($"TestBindingContext.AddNode({node})");
        }
    }


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
        return verifyOutput<T>(input,tree,id,decl=>((IHasIdentifier)decl).Identifier.Text);
    }

    //verify output (syntax tree)
    protected T verifyOutput<T>(string input, AtSyntaxTree atTree, string expectedId, Func<T,string> actualId)  where T : AtSyntaxNode
    {
        assert_not_null(()=>atTree);

        assert_equals(()=>0,()=>atTree.GetDiagnostics().Count(),"Syntax tree contains diagnostics: {0}",atTree.GetDiagnostics().FirstOrDefault()?.Message);

        var root = atTree.GetRoot();
        assert_equals(()=>input,()=>root.FullText);

        var node = root.DescendantNodes().OfType<T>().FirstOrDefault();
        assert_not_null(()=>node);
        assert_equals(()=>expectedId, ()=>actualId(node));

        return node;
    }

    //verify output (C# tree)
    protected void verifyOutput<T>
    (
        cs.CSharpSyntaxTree csharpTree,
        string              id,
        Func<T,string>      getId,
        string              id2    = null,
        Func<T,string>      getId2 = null) 

        where T : csSyntax.MemberDeclarationSyntax {

        var csRoot = csharpTree.GetRoot();
        Write(csRoot.NormalizeWhitespace());

        var any = csRoot.DescendantNodes()
                        .OfType<T>()
                        .Single(_=>getId(_)==id);
        assert_not_null(any);    
        
        if (id2!=null && getId2!=null)
        {
            assert_equals(id2,getId2(any));
        }                    
    }

    protected string identifier(int? i = null) => TestData.Identifier();
    protected AtSyntaxTree parseTree(string input)
    {
        try
        {
            return AtSyntaxTree.ParseText(input);
        }
        catch
        {
            Write(()=>input);
            throw;
        }
    }

    protected CompilationUnit bind(AtSyntaxTree tree)
    {
        var root = tree.GetRoot();
        var compilation = AtCompilation.Create(tree);
        var testCtx = new TestBindingContext(this);
        var compilationCtx = new CompilationContext(testCtx,compilation,null);
        var binder = new Binding.Binder(compilationCtx);
        var node = (CompilationUnit) binder.VisitCompilationUnit(root);
        return node;
    }

    protected CompilationUnit parseAndBind(string input)
    {
        return bind(parseTree(input));
    }
}
}
