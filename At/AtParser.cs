using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using At;
using At.Syntax;
using static At.TokenKind;

namespace At
{
public class AtParser : IDisposable
{
    bool parsing;

    readonly AtLexer            lexer;
    readonly Buffer<AtToken>    tokens;
    readonly List<AtDiagnostic> diagnostics = new List<AtDiagnostic>();
    readonly object             @lock       = new object();

    public AtParser(AtLexer lexer)
    {
        this.lexer  = lexer;
        this.tokens = new Buffer<AtToken>(lexer.Lex());
    }

    //ParseCompilationUnit()
    public CompilationUnitSyntax ParseCompilationUnit()
    {
        diagnostics.Clear();
        CompilationUnitSyntax compilationUnitSyntax;

        lock(@lock)
        {
            if (parsing) throw new Exception("PARSING ALREADY");
            parsing = true;

            compilationUnitSyntax = SyntaxFactory.CompilationUnit(compilationUnit().ToList());

            parsing = false;   
        }

        diagnostics.AddRange (compilationUnitSyntax.DescendantNodes()
                                                    .OfType<ExpressionClusterSyntax>()
                                                    .Select(_=> AtDiagnostic.Create(DiagnosticIds.ExpressionCluster,"Compiler","Expression cluster: "+_,DiagnosticSeverity.Error,0,true)));   
        return compilationUnitSyntax;
    }

    //Compilation Unit
    IEnumerable<ExpressionSyntax> compilationUnit()
    {
        if (position() < 0) 
            moveNext();

        while (!END())
        {
            var token = current();

            switch(token.Kind)
            {
                case StartOfFile:
                case EndOfFile  :
                case Space:
                case EndOfLine:  
                    moveNext();
                    continue; //TODO: #hash-statements, etc.

                case AtSymbol: 
                case StringLiteral:
                case NumericLiteral:
                case TokenCluster: yield return expression(); break;
                            
                default: 
                    moveNext();
                    yield return error(diagnostics, DiagnosticIds.UnexpectedToken,token,$"char {token.Position}: Unexpected token: '{token.Text}' ({token.Kind})"); break;
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
    ExpressionSyntax expression()
    {
        if (isCurrent(AtSymbol)) 
            return declarationExpression();        
            
        if (isCurrent(TokenCluster) && current().Text[0]=='#')
            return directiveExpression();

        if (isCurrentAny(NumericLiteral,StringLiteral))
            return literalExpression();

        var x = current();

        throw new NotImplementedException($"{x.Kind}: {x.Text}");

        /*return x.Kind==TokenKind.TokenCluster ? new ExpressionSyntax("id",x)
                                                : new ExpressionSyntax("string literal",x);*/
    }

    LiteralExpressionSyntax literalExpression()
    {
        assertCurrentAny(NumericLiteral,StringLiteral);
        return SyntaxFactory.LiteralExpression(consumeToken());
    }

    //#import namespace
    DirectiveSyntax directiveExpression()
    {
        var nodes = new List<AtSyntaxNode>();
        var directive = consumeToken(TokenCluster); nodes.Add(directive);
        var name = this.name(); nodes.Add(name);

        AtToken semi = null;
        if (isCurrent(SemiColon))
        {
            semi = consumeToken(SemiColon); 
            nodes.Add(semi);
        }

        return SyntaxFactory.Directive(directive,name,semi,nodes);
    }

    //Expression Cluster: "a { ... } b() { ... } ;"
    ExpressionClusterSyntax expressionCluster()
    {
        throw new NotImplementedException();
    }

    //declarationExpression "@TokenCluster[<...>][(...)][; | { ... }]"
    //declarationExpression "@TokenCluster ColonPair [; | { ... }]"
    DeclarationSyntax declarationExpression()
    {
        var nodes = new List<AtSyntaxNode>();
        var atSymbol = consumeToken(AtSymbol);
        var tc = consumeToken(TokenCluster);
        var isNamespace = false;
        var isClass = false;
        var isMethod = false;       

        nodes.Add(atSymbol);
        nodes.Add(tc);        
  
        //<[...]>
        AtToken lessThan = null;
        AtToken greaterThan = null;
        ListSyntax<ParameterSyntax> typeParams = null;
        if (isCurrent(LessThan))
        {                
            lessThan = consumeToken(LessThan);

            SeparatedSyntaxList<ParameterSyntax> typeParamList = null;
            if (!isCurrent(GreaterThan))
                typeParamList = list(Comma,typeParameter,GreaterThan);
                                   
            greaterThan = consumeToken(GreaterThan);

            typeParams = SyntaxFactory.List(lessThan,typeParamList,greaterThan,null);
            nodes.Add(typeParams);
            isClass = true; 
        }

        //(...)
        ListSyntax<ParameterSyntax> methodParams = null;
        if (isCurrent(LeftParenthesis))
        {
            var leftParen = consumeToken(LeftParenthesis);

            SeparatedSyntaxList<ParameterSyntax> methodParamList = null;
            if(!isCurrent(RightParenthesis))
                methodParamList = list(TokenKind.Comma,methodParameter,RightParenthesis);

            var rightParen = consumeToken(RightParenthesis);
            methodParams = SyntaxFactory.List<ParameterSyntax>(leftParen,methodParamList,rightParen);
            nodes.Add(methodParams);
            isMethod = true;
            isClass  = false;
        }


        //: baseType<>[, ...]
        AtToken colon = null;
        ListSyntax<NameSyntax> baseList = null; 
        NameSyntax type = null;
        if (isCurrent(Colon))
        {
            colon = consumeToken(Colon);
                
            if (isClass)
            {
                var baseTypeList = list(Comma,name,SemiColon,LeftBrace,EndOfFile);

                //TODO: remove colon from list? (PairSyntax<Colon>)
                baseList = SyntaxFactory.List<NameSyntax>(colon,baseTypeList,null,null);
                nodes.Add(baseList);
            }
            else
            {
                type = name();
                if (type.Text == "namespace")
                    isNamespace = true;
                nodes.Add(colon);
                nodes.Add(type);
            }
        }

            
        //";" | "{...}"
        members:  var members = new List<DeclarationSyntax>();
        if (isCurrent(SemiColon))
        {                
            nodes.Add(consumeToken(SemiColon));
        }
        else if (isCurrent(LeftBrace))
        {
            nodes.Add(consumeToken(LeftBrace));

            while(!isCurrent(RightBrace))
            {               
                if (!isCurrent(AtSymbol))
                {
                    nodes.Add(error(diagnostics,DiagnosticIds.UnexpectedToken,consumeToken(),"Expected an '@'."));
                    continue;
                }
            
                //TODO: support for ".ctor { }" expression 
                var member = declarationExpression();
                members.Add(member);
                nodes.Add(member);
            }

            nodes.Add(consumeToken(RightBrace)); 
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
        if (isCurrent(SemiColon))
            nodes.Add(current(SemiColon));        

        if (isMethod)
            return SyntaxFactory.MethodDeclaration(atSymbol,tc, methodParams, returnType: null, nodes: nodes);

        return SyntaxFactory.VariableDeclaration(atSymbol, tc,type, value: null,nodes:nodes);

        //throw new NotImplementedException("non-class declaration expresssion");

        //return new ExpressionSyntax(isClass?"@class":"@obj",tc,afterColon ?? new Token());
        //return new ExpressionSyntax();

    }

       
    AtToken current()        => tokens.Current;
    int     position()       => tokens.Position;
    AtToken lookAhead(int k) => tokens.LookAhead(k);
    bool    moveNext()       => tokens.MoveNext();
    bool    END()            => tokens.End;

    AtToken current(TokenKind assertedToken) 
    {
        assertCurrent(assertedToken);
        return tokens.Current;
    }

    AtToken consumeToken(TokenKind? assumedToken = null)
    {
        if (assumedToken != null)
            assertCurrent(assumedToken.Value);

        var c = tokens.Current;
        tokens.MoveNext();
        return c;
    }


    private T error<T>() 
    {
        throw new NotImplementedException();
    }

    //{Curly Block}
    private BlockSyntax curlyBlock()
    {
        var leftBrace = consumeToken(LeftBrace);
        var p = position();

        var contents = new List<ExpressionSyntax>();
        while(!isCurrent(RightBrace))
        {
            contents.Add(expression());
        }

        return SyntaxFactory.Block(leftBrace,contents,rightBrace:consumeToken(RightBrace));
    }

    ParameterSyntax methodParameter()
    {
        throw new NotImplementedException();
    }

    NameSyntax name()
    {
        assertCurrent(TokenCluster);
        var identifier = consumeToken(TokenCluster);

        //type args <T, U, V>
        SeparatedSyntaxList<NameSyntax> typeArgs = null;
        AtToken lessThan = null;
        AtToken greaterThan = null;

        if (isCurrent(LessThan))
        {
           lessThan = consumeToken(); 
           typeArgs = list(Comma,name,GreaterThan);
           greaterThan = consumeToken(GreaterThan);
        }

        return (lessThan != null) ?
                    SyntaxFactory.NameSyntax(identifier,SyntaxFactory.List<NameSyntax>(lessThan,typeArgs,greaterThan,null)):
                    SyntaxFactory.NameSyntax(identifier);
    }

    ParameterSyntax typeParameter()
    {
        assertCurrent(TokenCluster);
        return SyntaxFactory.Parameter(consumeToken());
    }

    SeparatedSyntaxList<T> list<T>(TokenKind separator, Func<T> parseExpr, params TokenKind[] endDelimiters)
        where T : AtSyntaxNode
    {
        var list = new List<AtSyntaxNode>();
    
        if (!isCurrentAny(endDelimiters))
        {
            if (isCurrent(separator))
            {
                error(diagnostics,DiagnosticIds.UnexpectedToken,current(),$"Unexpected token: {separator}");
                moveNext();
            }

            while(true)
            {
                if (isCurrentAny(endDelimiters))
                    break;  
        
                list.Add(parseExpr());

                if (isCurrent(separator))
                    list.Add(consumeToken(separator));
            }            
        }

        assertCurrentAny(endDelimiters);
        return new SeparatedSyntaxList<T>(null,list);
    }


    void assertCurrent(TokenKind tokenKind) => Debug.Assert(tokens.Current.Kind == tokenKind);
    void assertCurrentAny(TokenKind tokenKind, params TokenKind[] tokenKinds) => Debug.Assert(tokens.Current.Kind == tokenKind || tokenKinds.Contains(tokens.Current.Kind));
    void assertCurrentAny(IEnumerable<TokenKind> tokenKinds) => Debug.Assert(tokenKinds.Contains(tokens.Current.Kind));

    bool skip(params TokenKind[] tokenKinds)
    {
        for(int i=1;i < tokenKinds.Length+1;++i) 
        {
            if (lookAhead(i).Kind!=tokenKinds[i-1]) 
                return false;
        }

        for(int i=0; i < tokenKinds.Length; ++i) 
            moveNext();

        return true;
    }


    bool isNext(TokenKind kind, int k = 1)
    {
        return lookAhead(k)?.Kind==kind;
    }

    bool isCurrent(TokenKind tokenKind)
    {
        return tokens.Current?.Kind==tokenKind;
    }

    bool isCurrentAny(TokenKind tokenKind, params TokenKind[] tokenKinds)
    {
        return tokens.Current?.Kind==tokenKind || tokenKinds.Any(_=>tokens.Current?.Kind==_);
    }

    bool isCurrentAny(IEnumerable<TokenKind> tokenKinds)
    {
        return tokenKinds.Any(_=>tokens.Current?.Kind==_);
    }

    void IDisposable.Dispose()
    {
            
    }
}
}