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


    public static LiteralExpressionSyntax LiteralExpression(AtToken atToken, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(atToken,nameof(atToken)); 
        return new LiteralExpressionSyntax(atToken,new[] {atToken},expDef,diagnostics);
    }

    public static MethodDeclarationSyntax MethodDeclaration(AtToken atSymbol,AtToken tc,ListSyntax<ParameterSyntax> methodParams,NameSyntax returnType,List<AtSyntaxNode> nodes,IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics = null)
    {
       return new MethodDeclarationSyntax(atSymbol,tc,methodParams,returnType,nodes,expDef,diagnostics);
    }

    public static NamespaceDeclarationSyntax NamespaceDeclaration(AtToken atSymbol,AtToken identifier, List<DeclarationSyntax> members,List<AtSyntaxNode> nodes,IExpressionSource expDef/* = null*/, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new NamespaceDeclarationSyntax(atSymbol,identifier,members,nodes,expDef,diagnostics);
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

    public static PointyBlockSyntax PointyBlock(IExpressionSource expSrc,params AtSyntaxNode[] nodes)
    {
        if (nodes.Length < 2)
            throw new ArgumentException(nameof(nodes),"Should have at least 2 nodes.");

        var contents = (nodes.Length > 2)
                            ? nodes.Skip(1).Take(nodes.Length - 2).Cast<ExpressionSyntax>()
                            : null;

        return new PointyBlockSyntax(nodes[0].AsToken(),contents,nodes.Last().AsToken(),expSrc,null);
    }

    public static PointyBlockSyntax PointyBlock(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        if (startDelimiter == null)
            throw new ArgumentNullException(nameof(startDelimiter));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (endDelimiter == null)
            throw new ArgumentNullException(nameof(endDelimiter));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new PointyBlockSyntax(startDelimiter,contents,endDelimiter,expSrc,diagnostics);
    }

    public static RoundBlockSyntax RoundBlock(IExpressionSource expSrc,params AtSyntaxNode[] nodes)
    {
        if (nodes.Length < 2)
            throw new ArgumentException(nameof(nodes),"Should have at least 2 nodes.");

        var contents = (nodes.Length > 2)
                            ? nodes.Skip(1).Take(nodes.Length - 2).Cast<ExpressionSyntax>()
                            : null;

        return new RoundBlockSyntax(nodes[0].AsToken(),contents,nodes.Last().AsToken(),expSrc,null);
    }


    public static RoundBlockSyntax RoundBlock(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken rightDelimiter,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        if (startDelimiter == null)
            throw new ArgumentNullException(nameof(startDelimiter));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (rightDelimiter == null)
            throw new ArgumentNullException(nameof(rightDelimiter));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new RoundBlockSyntax(startDelimiter,contents,rightDelimiter,expSrc,diagnostics);
    }
    
    public static TypeDeclarationSyntax TypeDeclaration
    (
        AtToken atSymbol, 
        AtToken identifier, 
        ListSyntax<ParameterSyntax>  typeParameterList,
        ListSyntax<NameSyntax> baseList,
        IEnumerable<DeclarationSyntax> members,
        IEnumerable<AtSyntaxNode> nodes,
        IExpressionSource expSrc,
        IEnumerable<AtDiagnostic> diagnostics = null){

        checkNull(identifier,nameof(identifier));
        return new TypeDeclarationSyntax(atSymbol,identifier,typeParameterList,baseList,members,expSrc,nodes,diagnostics);
    }

    // **directive n[;]**
    public static DirectiveSyntax Directive(AtToken directive,NameSyntax n,AtToken semi,List<AtSyntaxNode> nodes,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(directive,nameof(directive));
        checkNull(n,nameof(n));
        return new DirectiveSyntax(directive,n,nodes,expSrc,diagnostics);
    }

    public static VariableDeclarationSyntax VariableDeclaration(AtToken atSymbol,AtToken identifier,NameSyntax type,object value,IEnumerable<AtSyntaxNode> nodes,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(identifier,nameof(identifier));
        return new VariableDeclarationSyntax(atSymbol,identifier,type,nodes,expSrc,diagnostics);
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