using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using At.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharp = Microsoft.CodeAnalysis.CSharp;

namespace At
{
class SyntaxTreeConverter
{
    readonly AtSyntaxTree atSyntaxTree;

    public SyntaxTreeConverter(AtSyntaxTree atSyntaxTree)
    {
       this.atSyntaxTree = atSyntaxTree;
    }

    
    public CSharpSyntaxTree ConvertToCSharpTree()
    {
        var atRoot = atSyntaxTree.GetRoot();
        var csRoot = CsharpCompilationUnitSyntax(atRoot);        
        var csharpTree = CSharpSyntaxTree.Create(csRoot);

        return (CSharpSyntaxTree) csharpTree;
    }

    CSharp.Syntax.CompilationUnitSyntax CsharpCompilationUnitSyntax(At.Syntax.CompilationUnitSyntax atRoot)
    {
       //160316: this is mainly for making tests fail
       var error = atRoot.DescendantNodes().OfType<ErrorNode>().FirstOrDefault();
       if (error != null)
       {
            throw new Exception(error.Message);
       }
    
       var csharpSyntax = CSharp.SyntaxFactory.CompilationUnit();
       
       var members    = new List<CSharp.Syntax.MemberDeclarationSyntax>();
       var statements = new List<CSharp.Syntax.StatementSyntax>();

       processNodes(atRoot.DescendantNodes(_=>_.Parent==atRoot), members, statements);

       //class _ { static int Main() { <statements>; return 0; } }
       var defaultClass = CSharp.SyntaxFactory.ClassDeclaration("_")
                                .AddMembers(CSharp.SyntaxFactory.MethodDeclaration(CSharp.SyntaxFactory.ParseTypeName("int"),"Main")
                                                  .AddModifiers(CSharp.SyntaxFactory.ParseToken("static"))
                                                  .AddBodyStatements(statements.ToArray())
                                                  //return 0
                                                  .AddBodyStatements(new StatementSyntax[]{CSharp.SyntaxFactory.ReturnStatement(CSharp.SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,CSharp.SyntaxFactory.ParseToken("0")))}));
                                
       csharpSyntax = csharpSyntax.AddMembers(defaultClass).AddMembers(members.ToArray());
       return csharpSyntax;
    }

    //processNodes()
    void processNodes(IEnumerable<AtSyntaxNode> nodes,List<MemberDeclarationSyntax> members,List<StatementSyntax> statements)
    {
       foreach(var node in nodes)
       {
          var d = node as DeclarationSyntax;
          if (d != null)
          {
            var cSharpDecl = MemberDeclarationSyntax(d);
            members.Add(cSharpDecl);
            continue;
          }

          //SHOULD ALWAYS BE LAST
          var expr = node as At.Syntax.ExpressionSyntax;
          if (expr != null)
          {
             var csExprStmt = ExpressionStatementSyntax(expr);
             statements.Add(csExprStmt);
             continue;
          }

          //throw new NotSupportedException(node.GetType().ToString());  
       }
    }

    CSharp.Syntax.ExpressionStatementSyntax ExpressionStatementSyntax(At.Syntax.ExpressionSyntax expr)
    {
        return CSharp.SyntaxFactory.ExpressionStatement(ExpressionSyntax(expr));
    }

    CSharp.Syntax.ExpressionSyntax ExpressionSyntax(At.Syntax.ExpressionSyntax expr)
    {
        var id = expr as Syntax.NameSyntax;
        if (id != null) return CSharp.SyntaxFactory.IdentifierName(id.Identifier.Text);

        throw new NotImplementedException(expr.GetType().ToString());
    }

    CSharp.Syntax.MemberDeclarationSyntax MemberDeclarationSyntax(DeclarationSyntax d)
    {
        var classDecl = d as At.Syntax.TypeDeclarationSyntax;
        if (classDecl != null)
        { 
           var csharpClass = ClassDeclarationSyntax(classDecl);
           return csharpClass;
        }

        throw new NotSupportedException(d.GetType().ToString());
    }

    CSharp.Syntax.ClassDeclarationSyntax ClassDeclarationSyntax(At.Syntax.TypeDeclarationSyntax classDecl)
    {
        var classId = classDecl.Identifier;
        var csId = CSharp.SyntaxFactory.Identifier(lTrivia(classId),classId.Text,tTrivia(classId));
        var csClass = CSharp.SyntaxFactory.ClassDeclaration(csId).AddModifiers(
                             CSharp.SyntaxFactory.ParseToken("public"));
        var csTypeParams = classDecl.TypeParameters.List.Select(_=>
                                CSharp.SyntaxFactory.TypeParameter(_.Text));

        if (csTypeParams != null) 
            csClass = csClass.AddTypeParameterListParameters(csTypeParams.ToArray());

        if (classDecl.BaseTypes != null) 
            csClass = csClass.AddBaseListTypes(classDecl.BaseTypes.List.Select(_=>
                            CSharp.SyntaxFactory.SimpleBaseType(
                                CSharp.SyntaxFactory.ParseTypeName(_.Text))).ToArray());

        if (classDecl.Members != null)
            csClass = csClass.AddMembers(classDecl.Members.Select(MemberDeclarationSyntax).ToArray());

        return csClass;
    }

    SyntaxTriviaList lTrivia(AtToken token)
    {
        return CSharp.SyntaxFactory.ParseLeadingTrivia(string.Join("",token.leadingTrivia.Select(_=>_.FullText)));
    }

    SyntaxTriviaList tTrivia(AtToken token)
    {
        return CSharp.SyntaxFactory.ParseTrailingTrivia(string.Join("",token.trailingTrivia.Select(_=>_.FullText)));
    }

}
}
