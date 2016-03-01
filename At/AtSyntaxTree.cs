using System;
using At.Syntax;

namespace At
{
public class AtSyntaxTree
{
    CompilationUnitSyntax compilationUnit;
    AtSourceText text;

    public AtSyntaxTree(AtSourceText text, CompilationUnitSyntax compilationUnit)
    {
        this.text = text;
        this.compilationUnit = compilationUnit;
    }

    //ParseText(string)
    public static AtSyntaxTree ParseText(string text)
    {
         return ParseText(AtSourceText.From(text));
    }

    //ParseText(AtSourceText)
    private static AtSyntaxTree ParseText(AtSourceText text)
    {
        using (var lexer = new AtLexer(text))
        {
            using (var parser = new AtParser(lexer))
            {
                var compilationUnit = parser.ParseCompilationUnit();
                var tree = new AtSyntaxTree(text, compilationUnit);
                //tree.VerifySource();
                return tree;
            }
        }
    }

    internal CompilationUnitSyntax GetRoot()
    {
        throw new NotImplementedException();
    }

}
}