using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using At;
using At.Syntax;
using static At.TokenKind;
//using static At.KnownTokenKind;

namespace At
{
public class AtParser : IDisposable
{

    public AtParser() : this(AtLexer.Default()) {}
    public AtParser(AtLexer lexer)
    {
        this.Lexer = lexer;
    }

    public AtLexer Lexer {get;}

    //ParseCompilationUnit()
    public CompilationUnitSyntax ParseCompilationUnit(IEnumerable<char> input)
    {
        var tokens = new Scanner<AtToken>(Lexer.Lex(input));
        var diagnostics = new List<AtDiagnostic>();
        var expressions = this.expressions(tokens,diagnostics);
        var compilationUnitSyntax = SyntaxFactory.CompilationUnit(expressions.ToList(),diagnostics);

        diagnostics.AddRange (compilationUnitSyntax.DescendantNodes()
                                                    .OfType<ExpressionClusterSyntax>()
                                                    .Select(_=> AtDiagnostic.Create(DiagnosticIds.ExpressionCluster,"Compiler","Expression cluster: "+_,DiagnosticSeverity.Error,0,true)));   
        return compilationUnitSyntax;
    }

    //Compilation Unit
    IEnumerable<ExpressionSyntax> expressions(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics)
    {
        if (tokens.Position < 0) 
            tokens.MoveNext();

        while (!tokens.End)
        {
            var token = tokens.Current;
            
            if (token.IsTrivia)
            {
                tokens.MoveNext();
                continue;
            }

            if (   token.Kind == AtSymbol 
                || token.Kind == StringLiteral 
                || token.Kind == NumericLiteral 
                || token.Kind == TokenCluster)
            {   
                yield return expression(tokens,diagnostics); 
            }
            else
            {
                tokens.MoveNext();
                yield return error(diagnostics, DiagnosticIds.UnexpectedToken,token,$"char {token.Position}: Unexpected token: '{token.Text}' ({token.Kind})".Replace("{","{{").Replace("}","}}")); 
            }
        }        
    }

    ErrorNode error(List<AtDiagnostic> diagnostics,string diagnosticId,AtToken token,string f, params object[] args) 
    {
        diagnostics.Add(new AtDiagnostic(diagnosticId,token,string.Format(f,args)));

        return SyntaxFactory.ErrorNode(
                                diagnostics,
                                string.Format(f,args),
                                token);
    }   

    //expression (stringLiteral | id)
    ExpressionSyntax expression(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics)
    {
        var current = tokens.Current;

        if (current.Kind == (AtSymbol)) 
            return declarationExpression(tokens,diagnostics);        
            
        if (current.Kind == (TokenCluster) && current.Text[0]=='#')
            return directiveExpression(tokens,diagnostics);

        if (current.Kind==NumericLiteral || current.Kind==StringLiteral)
            return literalExpression(tokens,diagnostics);

        throw new NotImplementedException($"{current.Kind}: {current.Text}");

        /*return x.Kind==TokenKind.TokenCluster ? new ExpressionSyntax("id",x)
                                                : new ExpressionSyntax("string literal",x);*/
    }

    LiteralExpressionSyntax literalExpression(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics)
    {
        assertCurrentAny(tokens,NumericLiteral,StringLiteral);
        return SyntaxFactory.LiteralExpression(tokens.Consume());
    }

    //#import namespace
    DirectiveSyntax directiveExpression(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics)
    {
        var nodes = new List<AtSyntaxNode>();
        var directive = tokens.Consume()/*(TokenCluster)*/; nodes.Add(directive);
        var name = this.name(tokens,diagnostics); nodes.Add(name);

        AtToken semiColon = null;
        if (tokens.Current?.Kind == (SemiColon))
        {
            semiColon = tokens.Consume(); //(SemiColon); 
            nodes.Add(semiColon);
        }

        return SyntaxFactory.Directive(directive,name,semiColon,nodes);
    }

    //declarationExpression "@TokenCluster[<...>][(...)][; | { ... }]"
    //declarationExpression "@TokenCluster ColonPair [; | { ... }]"
    DeclarationSyntax declarationExpression(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics)
    {
        var nodes = new List<AtSyntaxNode>();
        var atSymbol = tokens.Consume(); //(AtSymbol);
        var tc = tokens.Consume(); //(TokenCluster);
        var isNamespace = false;
        var isClass = false;
        var isMethod = false;       

        nodes.Add(atSymbol);
        nodes.Add(tc);        
  
        //<[...]>
        AtToken lessThan = null;
        AtToken greaterThan = null;
        ListSyntax<ParameterSyntax> typeParams = null;
        if (tokens.Current?.Kind==(LessThan))
        {                
            lessThan = tokens.Consume();//(LessThan);

            SeparatedSyntaxList<ParameterSyntax> typeParamList = null;
            if (tokens.Current.Kind!=(GreaterThan))
                typeParamList = list(tokens,diagnostics,Comma,typeParameter,GreaterThan);
                                   
            greaterThan =  tokens.Consume();//(GreaterThan);

            typeParams = SyntaxFactory.List(lessThan,typeParamList,greaterThan,null);
            nodes.Add(typeParams);
            isClass = true; 
        }

        //(...)
        ListSyntax<ParameterSyntax> methodParams = null;
        if (tokens.Current?.Kind==(OpenParenthesis))
        {
            var leftParen = tokens.Consume();//(OpenParenthesis);

            SeparatedSyntaxList<ParameterSyntax> methodParamList = null;
            if(tokens.Current.Kind!=(CloseParenthesis))
                methodParamList = list(tokens,diagnostics,TokenKind.Comma,methodParameter,CloseParenthesis);

            var rightParen = tokens.Consume();//(CloseParenthesis);
            methodParams = SyntaxFactory.List<ParameterSyntax>(leftParen,methodParamList,rightParen);
            nodes.Add(methodParams);
            isMethod = true;
            isClass  = false;
        }


        //: baseType<>[, ...]
        AtToken colon = null;
        ListSyntax<NameSyntax> baseList = null; 
        NameSyntax type = null;
        if (tokens.Current?.Kind==(Colon))
        {
            colon = tokens.Consume();//(Colon);
                
            if (isClass)
            {
                var baseTypeList = list(tokens,diagnostics,Comma,name,SemiColon,OpenBrace,EndOfFile);

                //TODO: remove colon from list? (PairSyntax<Colon>)
                baseList = SyntaxFactory.List<NameSyntax>(colon,baseTypeList,null,null);
                nodes.Add(baseList);
            }
            else
            {
                type = name(tokens,diagnostics);
                if (type.Text == "namespace")
                    isNamespace = true;
                nodes.Add(colon);
                nodes.Add(type);
            }
        }

            
        //";" | "{...}"
        //members:  
        var members = new List<DeclarationSyntax>();
        if (tokens.Current?.Kind==(SemiColon))
        {                
            nodes.Add(tokens.Consume());//(SemiColon));
        }
        else if (tokens.Current?.Kind==(OpenBrace))
        {
            nodes.Add(tokens.Consume());//(TokenKind.OpenBrace));

            while(tokens.Current.Kind!=(TokenKind.CloseBrace))
            {               
                if (tokens.Current.Kind!=(TokenKind.AtSymbol))
                {
                    nodes.Add(error(diagnostics,DiagnosticIds.UnexpectedToken,tokens.Consume(),"Expected an '@'."));
                    continue;
                }
            
                //TODO: support for ".ctor { }" expression 
                var member = declarationExpression(tokens,diagnostics);
                members.Add(member);
                nodes.Add(member);
            }

            nodes.Add(tokens.Consume());//(TokenKind.CloseBrace)); 
        }


        if (isClass)
            return SyntaxFactory.TypeDeclaration(atSymbol,tc,typeParams,baseList,members,nodes);

        if (isNamespace)
            return SyntaxFactory.NamespaceDeclaration(atSymbol,tc,members,nodes);

        //TODO: method decl, property decl, variable/field decl (= vs. <-)


        //TODO: @<assignmentExpression> (decl (assign newid value))
        //TODO: @<assignmentExpression> (decl (assign (colon-pair newid type) value))
        //TODO: @x : T { P = v, ...}
        //TODO: [(+ | -)]@x;
        if (tokens.Current?.Kind==(TokenKind.SemiColon))
            nodes.Add(tokens.Current);        

        if (isMethod)
            return SyntaxFactory.MethodDeclaration(atSymbol,tc, methodParams, returnType: null, nodes: nodes);

        return SyntaxFactory.VariableDeclaration(atSymbol, tc,type, value: null,nodes:nodes);

        //throw new NotImplementedException("non-class declaration expresssion");

        //return new ExpressionSyntax(isClass?"@class":"@obj",tc,afterColon ?? new Token());
        //return new ExpressionSyntax();

    }

    /*
    //{Curly Block}
    private BlockSyntax curlyBlock(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics)
    {
        var leftBrace = consumeToken(OpenBrace);
        var p = position();

        var contents = new List<ExpressionSyntax>();
        while(!isCurrent(CloseBrace))
        {
            contents.Add(expression());
        }

        return SyntaxFactory.Block(leftBrace,contents,rightBrace:consumeToken(CloseBrace));
    }*/

    //used by declarationExpression()
    ParameterSyntax methodParameter(Scanner<AtToken> tokens,List<AtDiagnostic> diagnostics)
    {
        throw new NotImplementedException();
    }

    NameSyntax name(Scanner<AtToken> tokens,List<AtDiagnostic> diagnostics)
    {
        Debug.Assert(tokens.Current.Kind==TokenCluster);
        var identifier = tokens.Consume(); //(TokenCluster);

        //type args <T, U, V>
        SeparatedSyntaxList<NameSyntax> typeArgs = null;
        AtToken lessThan = null;
        AtToken greaterThan = null;

        if (tokens.Current?.Kind==(LessThan))
        {
           lessThan = tokens.Consume(); //)
           typeArgs = list(tokens,diagnostics,Comma,name,GreaterThan);
           greaterThan = tokens.Consume(); //(GreaterThan);
        }

        return (lessThan != null) ?
                    SyntaxFactory.NameSyntax(identifier,SyntaxFactory.List<NameSyntax>(lessThan,typeArgs,greaterThan,null)):
                    SyntaxFactory.NameSyntax(identifier);
    }

    ParameterSyntax typeParameter(Scanner<AtToken> tokens,List<AtDiagnostic> diagnostics)
    {
        Debug.Assert(tokens.Current.Kind==TokenCluster);
        return SyntaxFactory.Parameter(tokens.Consume());
    }

    SeparatedSyntaxList<T> list<T>(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics, TokenKind separator, Func<Scanner<AtToken>,List<AtDiagnostic>,T> parseExpr, params TokenKind[] endDelimiters)
        where T : AtSyntaxNode
    {
        var list = new List<AtSyntaxNode>();
    
        if (!endDelimiters.Any(_=>tokens.Current.Kind==_))
        {
            if (tokens.Current.Kind==(separator))
            {
                error(diagnostics,DiagnosticIds.UnexpectedToken,tokens.Consume(),$"Unexpected token: {separator}");
            }

            while(true)
            {
                if (tokens.End || endDelimiters.Any(_=>tokens.Current.Kind==_))
                    break;  
        
                list.Add(parseExpr(tokens,diagnostics));

                if (tokens.Current?.Kind==(separator))
                    list.Add(tokens.Consume()); 
            }            
        }

        if (!tokens.End)
            assertCurrentAny(tokens,endDelimiters);
        return new SeparatedSyntaxList<T>(null,list);
    }

    
    void assertCurrent(Scanner<AtToken> tokens,  TokenKind tokenKind) => Debug.Assert(tokens.Current.Kind == tokenKind);
    void assertCurrentAny(Scanner<AtToken> tokens, TokenKind tokenKind, params TokenKind[] tokenKinds) => Debug.Assert(tokens.Current.Kind == tokenKind || tokenKinds.Contains(tokens.Current.Kind));
    void assertCurrentAny(Scanner<AtToken> tokens, IEnumerable<TokenKind> tokenKinds) => Debug.Assert(tokenKinds.Contains(tokens.Current.Kind));
    

    bool skip(Scanner<AtToken> tokens, params TokenKind[] tokenKinds)
    {
        for(int i=1;i < tokenKinds.Length+1;++i) 
        {
            if (tokens.LookAhead(i).Kind!=tokenKinds[i-1]) 
                return false;
        }

        for(int i=0; i < tokenKinds.Length; ++i) 
            tokens.MoveNext();

        return true;
    }


    void IDisposable.Dispose()
    {
            
    }
}
}