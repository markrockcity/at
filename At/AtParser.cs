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

           diagnostics.AddRange (compilationUnitSyntax.Nodes()
                                                      .OfType<ExpressionClusterSyntax>()
                                                      .Select(_=> AtDiagnostic.Create(DiagnosticIds.ExpressionCluster,"Compiler","Expression cluster: "+_,DiagnosticSeverity.Error,0,true)));   
          return compilationUnitSyntax;
        }

        //Compilation Unit
        IEnumerable<ExpressionSyntax> compilationUnit()
        {
            while (moveNext())
            {
                var token = current();

                switch(token.Kind)
                {
                    case StartOfFile:
                    case EndOfFile  :
                    case Space:
                    case EndOfLine:  continue;  

                    case AtSymbol: 
                    case StringLiteral:
                    case TokenCluster: yield return expression(); break;
                            
                    default: yield return error(diagnostics, DiagnosticIds.UnexpectedToken,token,"char {1}: Unexpected token: '{0}'",token.Text,token.Position); break;
                }       
            }        
        }

        ErrorNode error(List<AtDiagnostic> diagnostics,object diagnosticId,AtToken token,string f, params object[] args) 
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

            var x = current();

            throw new NotImplementedException($"{x.Kind}: {x.Text}");

            /*return x.Kind==TokenKind.TokenCluster ? new ExpressionSyntax("id",x)
                                                    : new ExpressionSyntax("string literal",x);*/
        }

    //Expression Cluster: "a { ... } b() { ... } ;"
    ExpressionClusterSyntax expressionCluster()
    {
        throw new NotImplementedException();
    }

    //declarationExpression "@TokenCluster[<>][; | { ... }]"
    ExpressionSyntax declarationExpression()
    {
        
        var nodes = new List<AtSyntaxNode>();
        Debug.Assert(current().Kind==AtSymbol);
        var atSymbol = current();
        var isClass = false;
        //AtToken afterColon = null;
        ExpressionSyntax body = null;

        nodes.Add(atSymbol);
        
        if (isNext(TokenCluster))
        {
            moveNext();
            var tc = current();
            nodes.Add(tc);
            skipWhiteSpace();

            //<>
            AtToken lessThan = null;
            AtToken greaterThan = null;
            ListSyntax<ParameterSyntax> typeParams = null;
            if (isCurrent(LessThan))
            {                
                lessThan = current();
                skipWhiteSpace();

                SeparatedSyntaxList<ParameterSyntax> typeParamList = null;
                if (!isCurrent(GreaterThan))
                    typeParamList = list(Comma,typeParameter,GreaterThan);
                   
                assertCurrent(GreaterThan);             
                greaterThan = current();
                skipWhiteSpace();

                typeParams = SyntaxFactory.List<ParameterSyntax>(lessThan,typeParamList,greaterThan);
                nodes.Add(typeParams);
                isClass = true; 
            }


            //: baseType<>[, ...]
            AtToken colon = null;
            ListSyntax<NameSyntax> baseList = null; 
            if (isCurrent(Colon))
            {
                colon = consumeToken(Colon);
                
                var baseTypeList = list(Comma,name,SemiColon,LeftBrace,EndOfFile);

                //TODO: remove colon from list?
                baseList = SyntaxFactory.List<NameSyntax>(colon,baseTypeList,null);
                nodes.Add(baseList);
            }

            
            //";" | "{...}"
            if (isCurrent(SemiColon))
            {                
                nodes.Add(current());
            }
            else if (isCurrent(LeftBrace))
            {
                body = curlyBlock();
                nodes.Add(body);
            }


            if (isClass)
                return SyntaxFactory.TypeDeclaration(atSymbol,tc,typeParams,baseList,nodes);

            throw new NotImplementedException("non-class declaration expresssion");

            //return new ExpressionSyntax(isClass?"@class":"@obj",tc,afterColon ?? new Token());
            //return new ExpressionSyntax();
        }
        else
        {
            string msg = string.Format("character {1}: expected TokenCluster after '{0}'", atSymbol.Text, atSymbol.Position);            
            return error();
            //return  error(diagnostics,DiagnosticIds.UnexpectedToken, msg);
        }
    }

       
    AtToken current()        => tokens.Current;
    int     position()       => tokens.Position;
    AtToken lookAhead(int k) => tokens.LookAhead(k);
    bool    moveNext()       => tokens.MoveNext();

    AtToken consumeToken(TokenKind? assumedToken = null)
    {
        if (assumedToken != null)
            assertCurrent(assumedToken.Value);

        var c = tokens.Current;
        tokens.MoveNext();
        return c;
    }


    private ExpressionSyntax error() 
    {
        throw new NotImplementedException();
    }

    //{Curly Block}
    private CurlyBlockSyntax curlyBlock()
    {
        assertCurrent(LeftBrace);
        var leftBrace = current();
        var p = position();
        skipWhiteSpace();

        var contents = new List<ExpressionSyntax>();
        while(!isCurrent(RightBrace))
        {
            contents.Add(expression());
            skipWhiteSpace();
        }

        return SyntaxFactory.CurlyBlock(leftBrace,contents,rightBrace:current());
    }


    NameSyntax name()
    {
        assertCurrent(TokenCluster);
        var identifier = consumeToken();

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
                    SyntaxFactory.NameSyntax(identifier,SyntaxFactory.List<NameSyntax>(lessThan,typeArgs,greaterThan)):
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
    
        if (!isCurrent(endDelimiters))
        {
            if (isCurrent(separator))
            {
                error(diagnostics,DiagnosticIds.UnexpectedToken,current(),$"Unexpected token: {separator}");
                moveNext();
            }

            while(true)
            {
                if (isCurrent(endDelimiters))
                    break;  
        
                list.Add(parseExpr());

                if (isCurrent(separator))
                {
                    list.Add(current());
                    moveNext();
                }
            }            
        }

        assertCurrent(endDelimiters);
        return new SeparatedSyntaxList<T>(null,list);
    }


    void assertCurrent(params TokenKind[] tokenKinds)
    {
        Debug.Assert(tokenKinds.Contains(tokens.Current.Kind));
    }

    bool skip(params TokenKind[] tokenKinds)
    {
        for(int i=1;i < tokenKinds.Length+1;++i) 
        {
            if (lookAhead(i).Kind!=tokenKinds[i-1]) 
                return false;
            else 
                skipWhiteSpace();
        }

        for(int i=0; i < tokenKinds.Length; ++i) 
            moveNext();

        return true;
    }

    //TODO: remove this? 
    void skipWhiteSpace()
    {
        if (!isWhiteSpace(current())) 
            moveNext();
        
        while(isWhiteSpace(current())) 
            moveNext();
    }

    static bool isWhiteSpace(AtToken token)
    {
        return      (token != null)
                &&  (token.Kind==Space  || token.Kind==EndOfLine);
    }

    bool isNext(TokenKind kind, int k = 1)
    {
        return lookAhead(k).Kind==kind;
    }

    bool isCurrent(params TokenKind[] tokenKinds)
    {
        return tokenKinds.Any(_=>tokens.Current?.Kind==_);
    }

    void IDisposable.Dispose()
    {
            
    }
}
}