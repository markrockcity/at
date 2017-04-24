using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using At.Binding;
using At.Contexts;
using At.Symbols;
using At.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using atSyntax = At.Syntax;
using atSymbols = At.Symbols;
using cs = Microsoft.CodeAnalysis.CSharp;
using csSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using ms = Microsoft.CodeAnalysis;

namespace At.Targets.CSharp
{
    internal class CSharpSyntaxTreeConverter
{
    internal const string defaultClassName = "_";

    private cs.Syntax.ClassDeclarationSyntax defaultClass;
    readonly ConverterContext ctx;
    readonly Func<Symbol,CSharpSyntaxNode> map;

    CSharpSyntaxTreeConverter()
    {
       this.defaultClass = ClassDeclaration(defaultClassName).WithModifiers(TokenList(Token(PartialKeyword)));
    }

    public CSharpSyntaxTreeConverter(At.Context ctx, Func<Symbol,CSharpSyntaxNode> map) : this()
    {
        var usings       = new List<UsingDirectiveSyntax>();
        var members      = new List<MemberDeclarationSyntax>();
        var statements   = new List<StatementSyntax>();

        var cCtx = new ConverterContext(ctx, usings, members, statements);

        this.ctx = cCtx;    
        this.map = map;
    }

    protected CSharpSyntaxTreeConverter(ConverterContext ctx, Func<Symbol,CSharpSyntaxNode> map) 
    {
        this.ctx = ctx;    
        this.map = map;
    }

    protected class ConverterContext
    {
        public ConverterContext(Context ctx, 
                                List<UsingDirectiveSyntax> usings,
                                List<MemberDeclarationSyntax> members,
                                List<StatementSyntax> statements)
        {
            BindingContext = ctx;    
            Usings = usings;
            Members = members;
            Statements = statements;
        }

        public Context BindingContext {get;}
        public List<UsingDirectiveSyntax> Usings {get;}
        public List<MemberDeclarationSyntax> Members {get;}
        public List<StatementSyntax> Statements {get;}
    }

    public CSharpSyntaxTree ConvertToCSharpTree()
    {
        var csRoot = CsharpCompilationUnitSyntax();        
        var csharpTree = CSharpSyntaxTree.Create(csRoot);

        return (CSharpSyntaxTree) csharpTree;
    }

    private cs.Syntax.CompilationUnitSyntax CsharpCompilationUnitSyntax() 
    {
        var atRoot = (CompilationUnit) ctx.BindingContext;

       
       //160316: this is mainly for making tests fail
       var error = atRoot.Syntax.DescendantNodes().OfType<ErrorNode>().FirstOrDefault();
       if (error != null)
            throw new AtException(error.GetDiagnostics().FirstOrDefault()?.Message ?? error.Message);
       
       var eCluster = atRoot.Syntax.DescendantNodes().OfType<ExpressionClusterSyntax>().FirstOrDefault();
       if (eCluster != null)
            throw new AtException("Can't convert ExpressionClusterSyntax to C# : ❛"+eCluster+"❜");
             

       var csharpSyntax = CompilationUnit();
       processNodes();

       //class _ { <fields> static int Main() { <statements>; return 0; } }
       defaultClass = defaultClass.AddMembers(ctx.Members.OfType<FieldDeclarationSyntax>().ToArray())
                                  .AddMembers(ctx.Members.OfType<csSyntax.MethodDeclarationSyntax>().ToArray())
                                  .AddMembers(MethodDeclaration(ParseTypeName("int"),"Main")
                                                .AddModifiers(ParseToken("static"))
                                                .AddBodyStatements(ctx.Statements.ToArray())
                                                
                                                .AddBodyStatements
                                                (
                                                    (
                                                        from ns  in ctx.Members.OfType<csSyntax.NamespaceDeclarationSyntax>()
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
                                                         
       csharpSyntax = csharpSyntax.AddUsings(ctx.Usings.ToArray())
                                  .AddMembers(defaultClass)
                                  .AddMembers(ctx.Members.Where(_=>!(_ is null || _ is FieldDeclarationSyntax || _ is csSyntax.MethodDeclarationSyntax)).ToArray());
       return csharpSyntax;
    }

    //# processNodes()
    void processNodes()
    {
        foreach(var node in ctx.BindingContext.Contents())
        {

            //#directive
            if (node is Directive directive && directive.Syntax.Directive.Text==DirectiveSyntax.importDirective)
            {
                var usingDir = cs.SyntaxFactory.UsingDirective(NameSyntax(directive.Syntax.Name));
                ctx.Usings.Add(usingDir);
                continue;
            }

            //@declaration
            var dc = node as DeclarationContext;
            var d  = dc?.Declaration ?? node as IDeclaration; 
            if (d != null)
            { 
                var csMember = MemberDeclarationSyntax(d);
                ctx.Members.Add(csMember);
                continue;
            }            

            //method call
            if (node is CallSite callSite)
            {            
                var csExprStmt = StatementSyntax(callSite.Invocation, false);
                ctx.Statements.Add(csExprStmt);
                continue;            
            }
           
            //statement
            if (node is Operation expr)
            {
                var csExprStmt = StatementSyntax(expr, false);
                ctx.Statements.Add(csExprStmt);
                continue;
            }

            throw new NotSupportedException(node.GetType().ToString());  
        }
    }

    csSyntax.StatementSyntax StatementSyntax(Operation expr, bool returnStmtOnLast = true)
    {
        var e = ExpressionSyntax(expr);

        if (returnStmtOnLast & expr.Next == null)
            return ReturnStatement(e);

        else if (e is ParenthesizedExpressionSyntax)
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
            case SymbolReference id : return (id.Symbol is KeywordSymbol ks) ? (map?.Invoke(ks) as csSyntax.ExpressionSyntax ?? throw new NotImplementedException($"Keyword({ks.Name})"))  :  IdentifierName(id.Symbol.Name);
            case Literal         le : return LiteralExpression(_kind(le.Value),_literal(le.Value));
            case BinaryOperation bo : return ParenthesizedExpression(BinaryExpression(_kind(bo.Operator),ExpressionSyntax(bo.Left),ExpressionSyntax(bo.Right)));
            case CallSite        cs : return invocation(cs.Invocation);
            case Invocation      inv: return invocation(inv);

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
                                        : o is double d ? Literal((decimal) d)
                                        : throw new NotImplementedException(o.GetType()+" literal");   

    }

    csSyntax.InvocationExpressionSyntax invocation(Invocation inv)
    {
        var target = inv.Target is SymbolReference sr ? sr.Symbol : inv.Target;

        //emits method declaration as needed
        if (target is MethodSymbol method && toEmitAtCallSite.Contains(method.Declaration))
        {
            var methodId = method.Declaration.Name;

            var m = method.GetImplementation(inv.TypeArguments);
            ctx.Members.Add(MethodDeclarationSyntax(method.Declaration, m));
        }
        
        return InvocationExpression(ExpressionSyntax(inv.Target),ArgumentListSyntax(inv.Arguments));
    }

    csSyntax.ArgumentListSyntax ArgumentListSyntax(IEnumerable<IBindingNode> args)
    {
        return cs.SyntaxFactory.ArgumentList(
                                    new ms.SeparatedSyntaxList<csSyntax.ArgumentSyntax>()
                                            .AddRange(args.Select(ArgumentSyntax)));
    }

    csSyntax.ArgumentSyntax ArgumentSyntax(IBindingNode arg)
    {
        return cs.SyntaxFactory.Argument(ExpressionSyntax(arg is Argument a ? a.Operation : arg));
    }

    csSyntax.MemberDeclarationSyntax MemberDeclarationSyntax(IDeclaration d)
    {
        Debug.Assert(d!=null);

        if (d is NamespaceDeclaration nsDecl)
        {
            var csharpNs = NamespaceDeclarationSyntax(nsDecl);
            return csharpNs;
        }

        if (d is TypeDeclaration classDecl)
        {
            var csharpClass = ClassDeclarationSyntax(classDecl,classDecl.Definition);
            return csharpClass;
        }

        if (d is VariableDeclaration varDecl)
        {
            var csharpField = FieldDeclarationSyntax(varDecl != null ? (atSyntax.VariableDeclarationSyntax)varDecl.Syntax : (atSyntax.VariableDeclarationSyntax)d.Syntax);
            return csharpField;
        }

        if (d is MethodDeclaration methodDecl)
        {
            var csharpMethod = MethodDeclarationSyntax(methodDecl ?? new MethodDeclaration(null,(atSyntax.MethodDeclarationSyntax)d.Syntax));
            return csharpMethod;
        }

        throw new NotSupportedException(d.GetType().ToString());
    }

    csSyntax.NameSyntax NameSyntax(atSyntax.NameSyntax atName)
    {
        return IdentifierName(atName.Text);
    }

    csSyntax.ClassDeclarationSyntax ClassDeclarationSyntax(TypeDeclaration classDecl, TypeDefinition classDef)
    {
        
        var classId = classDecl.Syntax.Identifier;
        var csId = csIdentifer(classId);
        var csClass = ClassDeclaration(csId).AddModifiers(
                             ParseToken("public"));
        var csTypeParams = classDecl.Syntax.TypeParameters.List.Select(_=>
                                cs.SyntaxFactory.TypeParameter(_.Text));

        if (csTypeParams != null) 
            csClass = csClass.AddTypeParameterListParameters(csTypeParams.ToArray());

        if (classDecl.Syntax.BaseTypes != null) 
            csClass = csClass.AddBaseListTypes(classDecl.Syntax.BaseTypes.List.Select(_=>
                            cs.SyntaxFactory.SimpleBaseType(
                                cs.SyntaxFactory.ParseTypeName(_.Text))).ToArray());

        if (classDef?.HasContents ?? false)
        {
            var declarations = classDef.Contents().OfType<IDeclaration>();
            var members = new List<csSyntax.MemberDeclarationSyntax>();

            foreach(var d in declarations)
            {
                var m = MemberDeclarationSyntax(d);
                members.Add(m);
            }

            csClass = csClass.AddMembers(members.ToArray());
        }

        return csClass; 
    }


    csSyntax.FieldDeclarationSyntax FieldDeclarationSyntax(atSyntax.VariableDeclarationSyntax varDecl)
    {
        
        var fieldId = varDecl.Identifier;
        var csId = csIdentifer(fieldId);        
        var csVarDeclr = VariableDeclarator(csId);
        var csVarDecl = VariableDeclaration(varDecl.Type?.Text != null ? ParseTypeName(varDecl.Type.Text) : PredefinedType(Token(ObjectKeyword))) 
                            .AddVariables(csVarDeclr);
        var csField = FieldDeclaration(csVarDecl)
                                                .AddModifiers(ParseToken("public"),ParseToken("static"));
        
        return csField;
    }


    List<MethodDeclaration> toEmitAtCallSite = new List<MethodDeclaration>();
    csSyntax.MethodDeclarationSyntax MethodDeclarationSyntax(MethodDeclaration methodDecl, MethodDefinition def = null)
    {
        TypeSyntax paramType(atSymbols.ParameterSymbol p) 
            => (TypeSyntax) map?.Invoke(p.ParameterType) ?? TypeName(p.ParameterType);

        var _def = def ?? methodDecl.Definition;
        var _retType  = def?.ReturnType  ?? methodDecl.ReturnType;
        var _params = def?.Parameters ?? methodDecl.Parameters;
        var _tparams = def?.TypeParameters ?? methodDecl.TypeParameters;

        var methodHasTypeParamaters = (_tparams.Length > 0);

        if (methodHasTypeParamaters && (_def?.Constraints.OfType<OperatorConstraint>().Any() ?? false))
        {
            toEmitAtCallSite.Add(methodDecl);
            return null;
        }
        
        var methodId = methodDecl.Name;
        var returnType = _retType != null
                            ? (TypeSyntax) map?.Invoke(_retType) ?? TypeName(_retType)
                            : PredefinedType(Token(ObjectKeyword));

        var stmts = _def?.Contents().Select(_=>StatementSyntax((Operation)_)).ToArray();
        var csMethod = MethodDeclaration(returnType,csIdentifer(((IHasIdentifier)methodDecl.Syntax).Identifier))
                        .AddModifiers(ParseToken("public"),ParseToken("static"))
                        .AddParameterListParameters(_params.Select(_=>Parameter(ParseToken(_.Name)).WithType(paramType(_))).ToArray())
                        .AddBodyStatements(stmts!=null && stmts.Length > 0 ?  stmts : new[]{ParseStatement("return null;")});

        if (methodHasTypeParamaters)
        {
            var typeParams = methodDecl.TypeParameters.Select(_=>TypeParameterSyntax(_));
            csMethod = csMethod.AddTypeParameterListParameters(typeParams.ToArray());
        }

        return csMethod;
    }

    csSyntax.TypeParameterSyntax TypeParameterSyntax(atSymbols.TypeParameterSymbol typeParameter)
    {
        return TypeParameter(typeParameter.Name);
    }

    csSyntax.TypeSyntax TypeName(atSymbols.TypeSymbol atType)
    {
        return atType == TypeSymbol.Unknown ? PredefinedType(Token(ObjectKeyword)) : ParseTypeName(atType.Name);
    }

     

    csSyntax.NamespaceDeclarationSyntax NamespaceDeclarationSyntax(NamespaceDeclaration nsDecl)
    {
        var nsId = nsDecl.Syntax.Identifier;
        var csId = csIdentifer(nsId);
        var usings     = new List<UsingDirectiveSyntax>();         
        var members    = new List<MemberDeclarationSyntax>();
        var statements = new List<StatementSyntax>();


        //processNodes(nsDecl.Members, usings, members, statements);
        var nsCtx = new ConverterContext(nsDecl.Definition,usings,members,statements);
        var nsConverter = new CSharpSyntaxTreeConverter(nsCtx,map);
        nsConverter.processNodes();

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
