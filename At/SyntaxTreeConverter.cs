using System;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using atSyntax = At.Syntax;
using cs       = Microsoft.CodeAnalysis.CSharp;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace At
{
class SyntaxTreeConverter
{
    internal const string defaultClassName = "_";

    readonly AtSyntaxTree atSyntaxTree;
    private cs.Syntax.ClassDeclarationSyntax defaultClass;

    public SyntaxTreeConverter(AtSyntaxTree atSyntaxTree)
    {
       this.atSyntaxTree = atSyntaxTree;
       this.defaultClass = ClassDeclaration(defaultClassName).WithModifiers(TokenList(Token(PartialKeyword)));
    }
    
    public CSharpSyntaxTree ConvertToCSharpTree()
    {
        var atRoot = atSyntaxTree.GetRoot();
        var csRoot = CsharpCompilationUnitSyntax(atRoot);        
        var csharpTree = CSharpSyntaxTree.Create(csRoot);

        return (CSharpSyntaxTree) csharpTree;
    }

    cs.Syntax.CompilationUnitSyntax CsharpCompilationUnitSyntax(atSyntax.CompilationUnitSyntax atRoot)
    {
       //160316: this is mainly for making tests fail
       var error = atRoot.DescendantNodes().OfType<ErrorNode>().FirstOrDefault();
       if (error != null)
            throw new Exception(error.GetDiagnostics().FirstOrDefault()?.Message ?? error.Message);
       
       var cluster = atRoot.DescendantNodes().OfType<ExpressionClusterSyntax>().FirstOrDefault();
       if (cluster != null)
            throw new Exception("Can't convert ExpressionClusterSyntax to C# :"+cluster);
    
       var csharpSyntax = CompilationUnit();
       var usings       = new List<UsingDirectiveSyntax>();
       var members      = new List<MemberDeclarationSyntax>();
       var statements   = new List<StatementSyntax>();

       processNodes(atRoot.ChildNodes(), usings, members, statements);

       //class _ { <fields> static int Main() { <statements>; return 0; } }
       defaultClass = defaultClass.AddMembers(members.OfType<FieldDeclarationSyntax>().ToArray())
                                  .AddMembers(members.OfType<csSyntax.MethodDeclarationSyntax>().ToArray())
                                  .AddMembers(MethodDeclaration(ParseTypeName("int"),"Main")
                                                .AddModifiers(ParseToken("static"))
                                                .AddBodyStatements(statements.ToArray())
                                                
                                                .AddBodyStatements
                                                (
                                                    (
                                                        from ns  in members.OfType<csSyntax.NamespaceDeclarationSyntax>()
                                                        from cls in ns.Members.OfType<csSyntax.ClassDeclarationSyntax>()
                                                        where cls.Identifier.Text == ns.Name.ToString()
                                                        select ExpressionStatement(
                                                                    InvocationExpression(
                                                                            MemberAccessExpression
                                                                            (
                                                                                SimpleMemberAccessExpression,
                                                                                MemberAccessExpression(
                                                                                    SimpleMemberAccessExpression,
                                                                                    IdentifierName(ns.Name.ToString()),
                                                                                    IdentifierName(cls.Identifier.ToString())),
                                                                                IdentifierName("Init")))
                                                                            )
                                                    ).ToArray()
                                                )

                                                //return 0
                                                .AddBodyStatements(new StatementSyntax[]{ReturnStatement(LiteralExpression(NumericLiteralExpression,ParseToken("0")))}));
                                                         
       csharpSyntax = csharpSyntax.AddUsings(usings.ToArray())
                                  .AddMembers(defaultClass)
                                  .AddMembers(members.Where(_=>!(_ is FieldDeclarationSyntax || _ is csSyntax.MethodDeclarationSyntax)).ToArray());
       return csharpSyntax;
    }

    //# processNodes()
    void processNodes(IEnumerable<AtSyntaxNode> nodes,List<UsingDirectiveSyntax> usings,List<MemberDeclarationSyntax> members,List<StatementSyntax> statements)
    {
       foreach(var node in nodes)
       {
          //#directive
          var directive = node as atSyntax.DirectiveSyntax;
          if (directive?.Directive.Text==DirectiveSyntax.importDirective)
          {
             var usingDir = UsingDirective(NameSyntax(directive.Name));
             usings.Add(usingDir);
             continue;
          }
       
          //@declaration
          var d = node as DeclarationSyntax;
          if (d != null)
          {
            var cSharpDecl = MemberDeclarationSyntax(d);
            members.Add(cSharpDecl);
            continue;
          }

          //! SHOULD ALWAYS BE LAST
          //statement
          var expr = node as atSyntax.ExpressionSyntax;
          if (expr != null)
          {
             var csExprStmt = ExpressionStatementSyntax(expr);
             statements.Add(csExprStmt);
             continue;
          }

          //throw new NotSupportedException(node.GetType().ToString());  
       }
    }

    cs.Syntax.ExpressionStatementSyntax ExpressionStatementSyntax(atSyntax.ExpressionSyntax expr)
    {
        return ExpressionStatement(ExpressionSyntax(expr));
    }

    cs.Syntax.ExpressionSyntax ExpressionSyntax(atSyntax.ExpressionSyntax expr)
    {
        var id = expr as atSyntax.NameSyntax;
        if (id != null) 
            return cs.SyntaxFactory.IdentifierName(id.Identifier.Text);

        throw new NotImplementedException($"{expr.GetType()}: {expr}");
    }

    cs.Syntax.MemberDeclarationSyntax MemberDeclarationSyntax(DeclarationSyntax d)
    {
        var nsDecl = d as atSyntax.NamespaceDeclarationSyntax;
        if (nsDecl != null)
        {
            var csharpNs = NamespaceDeclarationSyntax(nsDecl);
            return csharpNs;
        }

        var classDecl = d as atSyntax.TypeDeclarationSyntax;
        if (classDecl != null)
        { 
            var csharpClass = ClassDeclarationSyntax(classDecl);
            return csharpClass;
        }

        var varDecl = d as atSyntax.VariableDeclarationSyntax;
        if (varDecl != null)
        {
            var csharpField = FieldDeclarationSyntax(varDecl);
            return csharpField;
        }

        var methodDecl = d as atSyntax.MethodDeclarationSyntax;
        if (methodDecl != null)
        {
            var csharpMethod = MethodDeclarationSyntax(methodDecl);
            return csharpMethod;
        }

        throw new NotSupportedException(d.GetType().ToString());
    }

    cs.Syntax.NameSyntax NameSyntax(atSyntax.NameSyntax atName)
    {
        return IdentifierName(atName.Text);
    }

    cs.Syntax.ClassDeclarationSyntax ClassDeclarationSyntax(atSyntax.TypeDeclarationSyntax classDecl)
    {
        
        var classId = classDecl.Identifier;
        var csId = csIdentifer(classId);
        var csClass = ClassDeclaration(csId).AddModifiers(
                             ParseToken("public"));
        var csTypeParams = classDecl.TypeParameters.List.Select(_=>
                                cs.SyntaxFactory.TypeParameter(_.Text));

        if (csTypeParams != null) 
            csClass = csClass.AddTypeParameterListParameters(csTypeParams.ToArray());

        if (classDecl.BaseTypes != null) 
            csClass = csClass.AddBaseListTypes(classDecl.BaseTypes.List.Select(_=>
                            cs.SyntaxFactory.SimpleBaseType(
                                cs.SyntaxFactory.ParseTypeName(_.Text))).ToArray());

        if (classDecl.Members != null)
            csClass = csClass.AddMembers(classDecl.Members.Select(MemberDeclarationSyntax).ToArray());

        return csClass; 
        
    }


    cs.Syntax.FieldDeclarationSyntax FieldDeclarationSyntax(atSyntax.VariableDeclarationSyntax varDecl)
    {
        
        var fieldId = varDecl.Identifier;
        var csId = csIdentifer(fieldId);        
        var csVarDeclr = VariableDeclarator(csId);
        var csVarDecl = VariableDeclaration(varDecl.Type?.Text != null ? cs.SyntaxFactory.ParseTypeName(varDecl.Type.Text) : PredefinedType(Token(SyntaxKind.ObjectKeyword))) 
                            .AddVariables(csVarDeclr);
        var csField = FieldDeclaration(csVarDecl)
                                                .AddModifiers(cs.SyntaxFactory.ParseToken("public"),cs.SyntaxFactory.ParseToken("static"));
        
        return csField;
    }

    cs.Syntax.MethodDeclarationSyntax MethodDeclarationSyntax(atSyntax.MethodDeclarationSyntax methodDecl)
    {
        
        var methodId = methodDecl.Identifier;
        var returnType = methodDecl.ReturnType != null
                            ? ParseTypeName(methodDecl.ReturnType.Text)
                            : PredefinedType(Token(SyntaxKind.ObjectKeyword));
        var csMethod = MethodDeclaration(returnType,csIdentifer(methodId))
                        .AddModifiers(ParseToken("public"))
                        .AddBodyStatements(ParseStatement("return null;"));
                            

        return csMethod;
    }

    cs.Syntax.NamespaceDeclarationSyntax NamespaceDeclarationSyntax(atSyntax.NamespaceDeclarationSyntax nsDecl)
    {
        
        var nsId = nsDecl.Identifier;
        var csId = csIdentifer(nsId);
        var usings     = new List<UsingDirectiveSyntax>();         
        var members    = new List<MemberDeclarationSyntax>();
        var statements = new List<StatementSyntax>();

       processNodes(nsDecl.Members, usings, members, statements);

       //class _ { <fields> static int Main() { <statements>; return 0; } }
       var defaultClass = ClassDeclaration(nsId.Text).WithModifiers(TokenList(Token(PartialKeyword)))
                                  .AddMembers(members.OfType<FieldDeclarationSyntax>().ToArray())
                                  .AddMembers(members.OfType<csSyntax.MethodDeclarationSyntax>().ToArray())
                                  .AddMembers(MethodDeclaration(PredefinedType(Token(VoidKeyword)),"Init")
                                                .AddModifiers(Token(InternalKeyword),Token(StaticKeyword))
                                                .AddBodyStatements(statements.ToArray()));
                                                         
        var csNs = NamespaceDeclaration(IdentifierName(csId))
                    .AddUsings(usings.ToArray())
                    .AddMembers(defaultClass)
                    .AddMembers(members.Where(_=>!(_ is FieldDeclarationSyntax || _ is csSyntax.MethodDeclarationSyntax)).ToArray());
        
        return csNs;
    }

    SyntaxToken csIdentifer(AtToken classId) => cs.SyntaxFactory.Identifier(lTrivia(classId),classId.Text,tTrivia(classId));       
    SyntaxTriviaList lTrivia(AtToken token)  => cs.SyntaxFactory.ParseLeadingTrivia(string.Join("",token.LeadingTrivia.Select(_=>_.FullText)));
    SyntaxTriviaList tTrivia(AtToken token)  => cs.SyntaxFactory.ParseTrailingTrivia(string.Join("",token.TrailingTrivia.Select(_=>_.FullText)));
}
}
