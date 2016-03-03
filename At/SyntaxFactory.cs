using System;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At
{
internal class SyntaxFactory
{
    public static CompilationUnitSyntax CompilationUnit(IEnumerable<ExpressionSyntax> exprs)
    {
        return new CompilationUnitSyntax(exprs);
    }

    public static ErrorNode ErrorNode(IList<AtDiagnostic> diagnostics,string msg, AtSyntaxNode node)
    {
         return new ErrorNode(diagnostics, msg,node);
    }


    public static ExpressionSyntax ClassDeclaration(
                                                AtToken                   atSymbol, 
                                                AtToken                   identifier, 
                                                TypeParameterListSytnax   typeParameterList,
                                                IEnumerable<AtSyntaxNode> nodes)
    {
        return new ClassDeclarationSyntax(atSymbol,identifier,typeParameterList,nodes);
    }

    public static TypeParameterListSytnax TypeParameterList(AtToken lessThan,AtToken greaterThan)
    {
        return new TypeParameterListSytnax(lessThan,new AtSyntaxNode[0],greaterThan);
    }

    public static CurlyBlockSyntax CurlyBlock(AtToken leftBrace, IEnumerable<ExpressionSyntax> contents, AtToken rightBrace)
    {
        if (leftBrace == null)
            throw new ArgumentNullException(nameof(leftBrace));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (rightBrace == null)
            throw new ArgumentNullException(nameof(rightBrace));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new CurlyBlockSyntax(leftBrace,contents,rightBrace);
    }
}
}