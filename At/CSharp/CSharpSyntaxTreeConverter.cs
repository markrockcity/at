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
using cs = Microsoft.CodeAnalysis.CSharp;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using ms = Microsoft.CodeAnalysis;
using At.Contexts;
using At.Binding;
using At.Symbols;
using System.Collections.Immutable;
using System.Diagnostics;

namespace At.Targets.CSharp
{
internal class CSharpSyntaxTreeConverter
{
    internal const string defaultClassName = "_";

    readonly atSyntax.CompilationUnitSyntax atSyntaxRoot;
    private cs.Syntax.ClassDeclarationSyntax defaultClass;
    readonly At.Contexts.CompilationUnitContext ctx;
    readonly Func<Symbol,csSyntax.ExpressionSyntax> map;

    public CSharpSyntaxTreeConverter(AtSyntaxTree atSyntaxTree) : this(atSyntaxTree.GetRoot()) { }

    public CSharpSyntaxTreeConverter(atSyntax.CompilationUnitSyntax atSyntaxRoot)
    {
       this.atSyntaxRoot = atSyntaxRoot;
       this.defaultClass = ClassDeclaration(defaultClassName).WithModifiers(TokenList(Token(PartialKeyword)));
    }

    public CSharpSyntaxTreeConverter(At.Contexts.CompilationUnitContext ctx, Func<Symbol,csSyntax.ExpressionSyntax> map) : this((atSyntax.CompilationUnitSyntax) ctx.Syntax)
    {
        this.ctx = ctx;    
        this.map = map;
    }

    private class SourceContext : Context
    {
        public SourceContext(Context parentCtx,DiagnosticsBag diagnostics,AtSyntaxNode syntaxNode) : base(parentCtx,diagnostics,syntaxNode)
        {
        }

        protected override ImmutableArray<IBindingNode> MakeContents()
        {
            return Syntax.ChildNodes().Select(_=>new SourceContext(this,Diagnostics,_)).ToImmutableArray<IBindingNode>();
        }

        protected internal override void AddNode(IBindingNode node)
        {
            //ignore
        }
    }

    public CSharpSyntaxTree ConvertToCSharpTree()
    {
        var csRoot = CsharpCompilationUnitSyntax(atSyntaxRoot, (Context) ctx ?? new SourceContext(null,null,atSyntaxRoot));        
        var csharpTree = CSharpSyntaxTree.Create(csRoot);

        return (CSharpSyntaxTree) csharpTree;
    }

    private cs.Syntax.CompilationUnitSyntax CsharpCompilationUnitSyntax(
                                                   atSyntax.CompilationUnitSyntax atRoot,
                                                   Context ctx) 
    {
       //160316: this is mainly for making tests fail
       var error = atRoot.DescendantNodes().OfType<ErrorNode>().FirstOrDefault();
       if (error != null)
            throw new AtException(error.GetDiagnostics().FirstOrDefault()?.Message ?? error.Message);
       
       var eCluster = atRoot.DescendantNodes().OfType<ExpressionClusterSyntax>().FirstOrDefault();
       if (eCluster != null)
            throw new AtException("Can't convert ExpressionClusterSyntax to C# : ❛"+eCluster+"❜");
       
       /*
       var tCluster = atRoot.DescendantNodes().OfType<TokenClusterSyntax>().FirstOrDefault();
       if (tCluster != null && ctx == null)
            throw new AtException("Can't convert TokenClusterSyntax (without context) to C# : ❛"+tCluster+"❜");
       */
        
       var csharpSyntax = CompilationUnit();
       var usings       = new List<UsingDirectiveSyntax>();
       var members      = new List<MemberDeclarationSyntax>();
       var statements   = new List<StatementSyntax>();

       processNodes(ctx.Contents(), usings, members, statements);

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
    void processNodes(IEnumerable<IBindingNode> nodes,List<UsingDirectiveSyntax> usings,List<MemberDeclarationSyntax> members,List<StatementSyntax> statements)
    {
        foreach(var node in nodes)
        {

            //#directive
            if (node is Directive directive && directive.Syntax.Directive.Text==DirectiveSyntax.importDirective)
            {
                var usingDir = cs.SyntaxFactory.UsingDirective(NameSyntax(directive.Syntax.Name));
                usings.Add(usingDir);
                continue;
            }

            //@declaration
            if (node is Declaration d)
            { 
                var csMember = MemberDeclarationSyntax(d.Syntax, d.Symbol);
                members.Add(csMember);
                continue;
            }
           
            //statement
            if (node is Expression expr)
            {
                var csExprStmt = ExpressionStatementSyntax(expr);
                statements.Add(csExprStmt);
                continue;
            }

            if (node is SourceContext ctx)
            {
                processNode(ctx.Syntax,usings,members,statements);
                continue;
            }

            throw new NotSupportedException(node.GetType().ToString());  
        }
    }

    class SourceExpression : Expression
    {
        public SourceExpression(Context ctx, atSyntax.ExpressionSyntax syntaxNode) : base(ctx,syntaxNode)
        {
        }

        public atSyntax.ExpressionSyntax Syntax => ExpressionSyntax;

        public override void Accept(BindingTreeVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol,ISymbol newSymbol)
        {
            throw new NotImplementedException();
        }
    }

    void processNode(AtSyntaxNode node,List<UsingDirectiveSyntax> usings,List<MemberDeclarationSyntax> members,List<StatementSyntax> statements)
    {
        //#directive
        var directive = node as atSyntax.DirectiveSyntax;
        if (directive?.Directive.Text==DirectiveSyntax.importDirective)
        {
            var usingDir = cs.SyntaxFactory.UsingDirective(NameSyntax(directive.Name));
            usings.Add(usingDir);
        }
           
        //@declaration
        else  if (node is DeclarationSyntax d)
        {
            var cSharpDecl = MemberDeclarationSyntax(d,null);
            members.Add(cSharpDecl);
        }

        //! SHOULD ALWAYS BE LAST
        //statement
        else if (node is atSyntax.ExpressionSyntax expr)
        {
            var csExprStmt = ExpressionStatementSyntax(new SourceExpression(null,expr));
            statements.Add(csExprStmt);
        }  
    }

    csSyntax.ExpressionStatementSyntax ExpressionStatementSyntax(Expression expr)
    {
        var e = ExpressionSyntax(expr);

        if (e is ParenthesizedExpressionSyntax)
            e = InvocationExpression
                    (MemberAccessExpression
                        (SyntaxKind.SimpleMemberAccessExpression,
                         MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("System"),
                            IdentifierName("Console")),
                         IdentifierName("WriteLine")),

                     ArgumentList(SeparatedList(new[]{Argument(e)})));

        return ExpressionStatement(e);
    }

    csSyntax.ExpressionSyntax ExpressionSyntax(IBindingNode expr)
    {
        switch(expr)
        {
            case KeywordSymbol         ks : return map?.Invoke(ks) ?? throw new NotImplementedException(ks.ToString());
            case Symbol                id : return IdentifierName(id.Name);
            case LiteralExpression     le : return LiteralExpression(_kind(le.Value),_literal(le.Value));
            case ApplicationExpression app: return InvocationExpression(ExpressionSyntax(app.Subject),ArgumentListSyntax(app.Arguments));
            case SourceExpression      se : return ExpressionSyntax((atSyntax.ExpressionSyntax) se.Syntax);
            case BinaryOperation       bo : return ParenthesizedExpression(BinaryExpression(_kind(bo.Operator),ExpressionSyntax(bo.Left),ExpressionSyntax(bo.Right)));

            default: throw new NotImplementedException($"{expr.GetType()}: {expr}");
        }        

        SyntaxKind _kind(object o) => o is OperatorSymbol os 
                                            ?   (  os.Name=="*" ? SyntaxKind.MultiplyExpression 
                                                 : os.Name=="+" ? SyntaxKind.AddExpression 
                                                 : throw new NotImplementedException($"Operator({os})"))
                                    : o is String ? SyntaxKind.StringLiteralExpression 
                                    : o.GetType() == typeof(double) ? SyntaxKind.NumericLiteralExpression
                                    : throw new NotImplementedException(o.GetType()+": "+o+" SyntaxKind");

        SyntaxToken _literal(object o) => o is String s ? Literal(s,s) 
                                        : o is double d ? Literal(d)
                                        : throw new NotImplementedException(o.GetType()+" literal");   

    }

    
    csSyntax.ExpressionSyntax ExpressionSyntax(atSyntax.ExpressionSyntax expr)
    {
        var id = expr as atSyntax.NameSyntax;
        if (id != null) 
            return cs.SyntaxFactory.IdentifierName(id.Identifier.Text);

        var app = expr as atSyntax.ApplicationSyntax;
        if (app != null)
            return InvocationExpression(ExpressionSyntax(app.Subject),ArgumentListSyntax(app.Arguments.Select(_=>new SourceExpression(null,_))));

        throw new NotImplementedException($"{expr.GetType()}: {expr}");
    }

    csSyntax.ArgumentListSyntax ArgumentListSyntax(IEnumerable<IBindingNode> args)
    {
        return cs.SyntaxFactory.ArgumentList(
                                    new ms.SeparatedSyntaxList<ArgumentSyntax>()
                                            .AddRange(args.Select(ArgumentSyntax)));
    }

    csSyntax.ArgumentSyntax ArgumentSyntax(IBindingNode e)
    {
        return cs.SyntaxFactory.Argument(ExpressionSyntax(e));
    }

    csSyntax.MemberDeclarationSyntax MemberDeclarationSyntax(DeclarationSyntax d, ContextSymbol symbol)
    {
        var nsDecl = d as atSyntax.NamespaceDeclarationSyntax;
        if (nsDecl != null)
        {
            var csharpNs = NamespaceDeclarationSyntax(nsDecl,symbol);
            return csharpNs;
        }

        var classDecl = d as atSyntax.TypeDeclarationSyntax;
        if (classDecl != null)
        { 
            var csharpClass = ClassDeclarationSyntax(classDecl,symbol);
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

    csSyntax.NameSyntax NameSyntax(atSyntax.NameSyntax atName)
    {
        return IdentifierName(atName.Text);
    }

    csSyntax.ClassDeclarationSyntax ClassDeclarationSyntax(atSyntax.TypeDeclarationSyntax classDecl, ContextSymbol symbol)
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
            csClass = csClass.AddMembers(classDecl.Members.Select(_=>MemberDeclarationSyntax(_,symbol)).ToArray());

        return csClass; 
        
    }


    csSyntax.FieldDeclarationSyntax FieldDeclarationSyntax(atSyntax.VariableDeclarationSyntax varDecl)
    {
        
        var fieldId = varDecl.Identifier;
        var csId = csIdentifer(fieldId);        
        var csVarDeclr = cs.SyntaxFactory.VariableDeclarator(csId);
        var csVarDecl = cs.SyntaxFactory.VariableDeclaration(varDecl.Type?.Text != null ? cs.SyntaxFactory.ParseTypeName(varDecl.Type.Text) : PredefinedType(Token(SyntaxKind.ObjectKeyword))) 
                            .AddVariables(csVarDeclr);
        var csField = cs.SyntaxFactory.FieldDeclaration(csVarDecl)
                                                .AddModifiers(cs.SyntaxFactory.ParseToken("public"),cs.SyntaxFactory.ParseToken("static"));
        
        return csField;
    }

    csSyntax.MethodDeclarationSyntax MethodDeclarationSyntax(atSyntax.MethodDeclarationSyntax methodDecl)
    {
        
        var methodId = methodDecl.Identifier;
        var returnType = methodDecl.ReturnType != null
                            ? cs.SyntaxFactory.ParseTypeName(methodDecl.ReturnType.Text)
                            : PredefinedType(Token(SyntaxKind.ObjectKeyword));
        var csMethod = cs.SyntaxFactory.MethodDeclaration(returnType,csIdentifer(methodId))
                        .AddModifiers(ParseToken("public"))
                        .AddBodyStatements(ParseStatement("return null;"));
                            

        return csMethod;
    }

    csSyntax.NamespaceDeclarationSyntax NamespaceDeclarationSyntax(atSyntax.NamespaceDeclarationSyntax nsDecl, At.Symbols.ContextSymbol ns)
    {
        
        var nsId = nsDecl.Identifier;
        var csId = csIdentifer(nsId);
        var usings     = new List<UsingDirectiveSyntax>();         
        var members    = new List<MemberDeclarationSyntax>();
        var statements = new List<StatementSyntax>();

        //processNodes(nsDecl.Members, usings, members, statements);
        processNodes(ns?.Context?.Contents() ?? nsDecl?.Members?.Select(_=>new SourceContext(null,null,_)).ToImmutableArray<IBindingNode>(), usings, members, statements);

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

    SyntaxToken csIdentifer(AtToken classId) => Identifier(lTrivia(classId),classId.Text,tTrivia(classId));       
    SyntaxTriviaList lTrivia(AtToken token)  => ParseLeadingTrivia(string.Join("",token.LeadingTrivia.Select(_=>_.FullText)));
    SyntaxTriviaList tTrivia(AtToken token)  => ParseTrailingTrivia(string.Join("",token.TrailingTrivia.Select(_=>_.FullText)));
}
}
