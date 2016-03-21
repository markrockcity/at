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


    public static ExpressionSyntax ClassDeclaration(
                                                AtToken                   atSymbol, 
                                                AtToken                   identifier, 
                                                TypeParameterListSyntax   typeParameterList,
                                                IEnumerable<AtSyntaxNode> nodes)
    {
        return new ClassDeclarationSyntax(atSymbol,identifier,typeParameterList,nodes);
    }

    public static TypeParameterListSyntax TypeParameterList(AtToken lessThan,AtToken greaterThan)
    {
        return new TypeParameterListSyntax(lessThan,new SeparatedSyntaxList<TypeParameterSyntax>(null,new AtSyntaxNode[0]),greaterThan);
    }

    public static TypeParameterListSyntax TypeParameterList(AtToken lessThan, SeparatedSyntaxList<TypeParameterSyntax> typeParamList,AtToken greaterThan)
    {
        checkNull(lessThan,nameof(lessThan));        
        checkNull(greaterThan,nameof(greaterThan));

        if (typeParamList == null)
            return TypeParameterList(lessThan,greaterThan);

        checkNull(typeParamList,nameof(typeParamList));
        return new TypeParameterListSyntax(lessThan,typeParamList,greaterThan);
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

    public static NameSyntax NameSyntax(AtToken identifier)
    {
        if (identifier == null)
            throw new ArgumentNullException(nameof(identifier));   
              
        return new NameSyntax(identifier);    
    }

    public static TypeParameterSyntax TypeParameter(AtToken identifier)
    {
        checkNull(identifier,nameof(identifier));
        return new TypeParameterSyntax(identifier);
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