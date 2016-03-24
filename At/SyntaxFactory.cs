using System;
using System.Collections;
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


    public static ExpressionSyntax TypeDeclaration(
                                            AtToken atSymbol, 
                                            AtToken identifier, 
                                            ListSyntax<ParameterSyntax>  typeParameterList,
                                            ListSyntax<NameSyntax> baseList,
                                            IEnumerable<AtSyntaxNode> nodes)
    {
        return new TypeDeclarationSyntax(atSymbol,identifier,typeParameterList,baseList, nodes);
    }

    public static ListSyntax<T> List<T>(AtToken startDelimiter,AtToken endDelimiter) where T : AtSyntaxNode
    {
        return new ListSyntax<T>(startDelimiter,new SeparatedSyntaxList<T>(null,new AtSyntaxNode[0]),endDelimiter);
    }

    public static ListSyntax<T> List<T>(AtToken startDelimiter, SeparatedSyntaxList<T> list,AtToken endDelimiter) where T : AtSyntaxNode
    {
        checkNull(startDelimiter,nameof(startDelimiter));        
        //checkNull(endDelimiter,nameof(endDelimiter));

        if (list == null)
            return List<T>(startDelimiter,endDelimiter);

        checkNull(list,nameof(list));
        return new ListSyntax<T>(startDelimiter,list,endDelimiter);
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

    public static NameSyntax NameSyntax(AtToken identifier, ListSyntax<NameSyntax> typeArgs = null)
    {
        checkNull(identifier,nameof(identifier));  
        return new NameSyntax(identifier, typeArgs);    
    }

    public static ParameterSyntax Parameter(AtToken identifier)
    {
        checkNull(identifier,nameof(identifier));
        return new ParameterSyntax(identifier);
    }

    static void checkNull(object obj, string name)
    {
        if (obj == null)
            throw new ArgumentNullException(name);

        if (obj is IEnumerable && ((IEnumerable) obj).Cast<object>().Any(_=>_== null))
            throw new ArgumentNullException(name,name+" contains a null reference");        
    }

}
}