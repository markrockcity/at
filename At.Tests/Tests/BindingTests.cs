using System;
using System.Collections.Generic;
using System.Linq;
using At.Binding;
using At.Contexts;
using At.Symbols;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace At.Tests
{
    [TestClass]
public  class BindingTests : AtTest
{


    //UndefinedSymbol reference
    [TestMethod] 
    public void UndefinedSymbolReferenceTest()
    {   
        undefinedTest("a");
        undefinedTest("b()");
        undefinedTest("output c");
        undefinedTest("output d()");
    }

    //Symbol reference
    [TestMethod] 
    public void SimpleReferenceTest()
    {   
        //simpleTest("@x; @f(x)   ; output f(x)");
        simpleTest("@y; @g(y) {}; output g(y)");
    }

    //Generic operator
    [TestMethod] 
    public void GenericOperatorTest()
    {
        var tree = parseTree("@f(x,y){ x + y }; f(1,2)");
        var btree = bind(tree);

        var dc = btree.Contents().OfType<DeclarationContext>().First();
        var def = (MethodDefinition) dc.Definition;        
        Write(dc);

        var cs = btree.Contents().OfType<CallSite>().First();
        var impl = ((MethodSymbol)dc.DeclaredSymbol).GetImplementation(cs.TypeArguments);
        assert_not_null(()=>impl);
        assert_equals(()=>impl.Parameters.Length,()=>cs.Arguments.Length,"impl.Parameters");
        Write(cs);
    }

    
    //Nested type
    [TestMethod] 
    public void NestedTypeTest()
    {
        var tree = parseTree("@A<B,C> : D<int,B> { @E<B,C> : D<int,B> { } }");
        var btree = bind(tree);

        var dc = btree.Contents().OfType<DeclarationContext>().First();
        Write(dc);

        var def = (TypeDefinition) dc.Definition;        
        Write(def);

        var td = def.Contents().OfType<TypeDeclaration>().First();
        Write(td);

        var def2 = td.Definition;
        Write(def2);

    }

    //Namespace
    [TestMethod] public void NamespaceTest()
    {
        var input = @"
            @ns1 : namespace  { @class<> { @x } }
        ";

        var tree = parseTree(input);
        Write(tree);

        var btree = bind(tree);
        Write(btree);

        void f(Context ct1, IEnumerable<IBindingNode> xs)
        {
            foreach(var x in xs)
            {
                 Write($"{ct1}.{x}");
                 if (x is Context ct) 
                    f(ct, ct.Contents());
            }
        }

        f(btree, btree.Contents());  
        
        var ns1 = btree.Contents().OfType<DeclarationContext>().First(_=>_.DeclaredSymbol.Name=="ns1").Definition;
        var css = ns1.Contents().OfType<DeclarationContext>().Where(_=>_.DeclaredSymbol?.Name=="class");
        assert_equals(1,()=>css.Count());
    }



    void simpleTest(string input)
    {
        var tree = parseTree(input);
        var btree = bind(tree);
        
        var undefinedSymbols = btree.getUndefinedSymbols();

        assert_equals(0,()=>undefinedSymbols.Count(),string.Join(",",undefinedSymbols.Select(_=>_.Item1)));
    }

    void undefinedTest(string input)
    {
        var tree = parseTree(input);
        var c = AtCompilation.Create(tree);

        var r = c.Bind();
        assert_false(()=>r.Success, $"{r.Context.Contents().First().Syntax?.Text ?? "<?>"}");
        assert_equals(DiagnosticIds.UndefinedSymbol,r.Diagnostics.First().Id);
        Write(r.Diagnostics.First().Message);

        var s =  r.Context.Contents().OfType<Context>().First().getUndefinedSymbols().FirstOrDefault().Item1;
        assert_not_null(()=>s);
        assert_type<UndefinedSymbol>(()=>s);  
        Write("----");
    }

    

}
}
