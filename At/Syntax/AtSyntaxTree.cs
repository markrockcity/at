using System;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At
{
//SyntaxTree + ParsedSyntaxTree
public class AtSyntaxTree
{
    readonly CompilationUnitSyntax compilationUnit;
    readonly string text;

    public AtSyntaxTree(string text, CompilationUnitSyntax compilationUnit)
    {
        this.text = text;
        this.compilationUnit = compilationUnit;
    }

    //GetDiagnostics()
    public IEnumerable<AtDiagnostic> GetDiagnostics()
    {
       return this.compilationUnit.GetDiagnostics().Concat(this.compilationUnit.DescendantNodes(null,true).OfType<ErrorNode>().SelectMany(_=>_.Diagnostics));
    }

    //GetRoot()
    public CompilationUnitSyntax GetRoot()
    {
        return this.compilationUnit;
    }

    //ParseText(string)
    public static AtSyntaxTree ParseText(string text)
    {
         return parseText(text);
    }

    public override string ToString()
    {
        return $"[{compilationUnit.ToString()}]";
    }

    //parseText(AtSourceText)
    static AtSyntaxTree parseText(string text)
    {
        using (var lexer  = AtLexer.DefaultLexer)
        using (var parser = AtParser.CreateDefaultParser(lexer))
        {
            var compilationUnit = parser.ParseCompilationUnit(text);
            var tree = new AtSyntaxTree(text, compilationUnit);
            return tree;
        }
    }
}
}