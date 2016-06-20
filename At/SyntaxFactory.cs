using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At
{
public class SyntaxFactory
{
    public static CompilationUnitSyntax CompilationUnit(IEnumerable<ExpressionSyntax> exprs,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new CompilationUnitSyntax(exprs,diagnostics);
    }

    public static ErrorNode ErrorNode(IList<AtDiagnostic> diagnostics,string msg, AtSyntaxNode node)
    {
         return new ErrorNode(diagnostics, msg,node);
    }


    public static ListSyntax<T> List<T>(AtToken startDelimiter,AtToken endDelimiter, IEnumerable<AtDiagnostic> diagnostics = null) where T : AtSyntaxNode
    {
        return new ListSyntax<T>(startDelimiter,new SeparatedSyntaxList<T>(null,new AtSyntaxNode[0]),endDelimiter,diagnostics);
    }

    public static ListSyntax<T> List<T>(AtToken startDelimiter, SeparatedSyntaxList<T> list,AtToken endDelimiter, IEnumerable<AtDiagnostic> diagnostics = null) where T : AtSyntaxNode
    {
        checkNull(startDelimiter,nameof(startDelimiter));        
        //checkNull(endDelimiter,nameof(endDelimiter));

        if (list == null)
            return List<T>(startDelimiter,endDelimiter,diagnostics);

        checkNull(list,nameof(list));
        return new ListSyntax<T>(startDelimiter,list,endDelimiter,diagnostics);
    }

    public static BlockSyntax Block(AtToken leftBrace, IEnumerable<ExpressionSyntax> contents, AtToken rightBrace,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        if (leftBrace == null)
            throw new ArgumentNullException(nameof(leftBrace));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (rightBrace == null)
            throw new ArgumentNullException(nameof(rightBrace));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new BlockSyntax(leftBrace,contents,rightBrace,diagnostics);
    }

    public static MethodDeclarationSyntax MethodDeclaration(AtToken atSymbol,AtToken tc,ListSyntax<ParameterSyntax> methodParams,NameSyntax returnType,List<AtSyntaxNode> nodes, IEnumerable<AtDiagnostic> diagnostics = null)
    {
       return new MethodDeclarationSyntax(atSymbol,tc,methodParams,returnType,nodes,diagnostics);
    }

    public static NamespaceDeclarationSyntax NamespaceDeclaration(AtToken atSymbol,AtToken identifier, List<DeclarationSyntax> members,List<AtSyntaxNode> nodes, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new NamespaceDeclarationSyntax(atSymbol,identifier,members,nodes,diagnostics);
    }

    public static NameSyntax NameSyntax(AtToken identifier, ListSyntax<NameSyntax> typeArgs = null)
    {
        checkNull(identifier,nameof(identifier));  
        return new NameSyntax(identifier, typeArgs);    
    }
                                                   
    public static ParameterSyntax Parameter(AtToken identifier,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(identifier,nameof(identifier));
        return new ParameterSyntax(identifier, diagnostics);
    }

    
    public static TypeDeclarationSyntax TypeDeclaration(
                                                AtToken atSymbol, 
                                                AtToken identifier, 
                                                ListSyntax<ParameterSyntax>  typeParameterList,
                                                ListSyntax<NameSyntax> baseList,
                                                IEnumerable<DeclarationSyntax> members,
                                                IEnumerable<AtSyntaxNode> nodes,
                                                IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(identifier,nameof(identifier));
        return new TypeDeclarationSyntax(atSymbol,identifier,typeParameterList,baseList,members,nodes,diagnostics);
    }

    public static VariableDeclarationSyntax VariableDeclaration(AtToken atSymbol,AtToken identifier,NameSyntax type,object value,IEnumerable<AtSyntaxNode> nodes,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(identifier,nameof(identifier));
        return new VariableDeclarationSyntax(atSymbol,identifier,type,nodes,diagnostics);
    }

    private static void checkNull(object obj, string name)
    {
        if (obj == null)
            throw new ArgumentNullException(name);

        if (obj is IEnumerable && ((IEnumerable) obj).Cast<object>().Any(_=>_== null))
            throw new ArgumentNullException(name,name+" contains a null reference");        
    }
}
}