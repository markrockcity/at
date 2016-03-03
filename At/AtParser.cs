using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using At.Syntax;

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

        public  CompilationUnitSyntax ParseCompilationUnit()
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

        IEnumerable<ExpressionSyntax> compilationUnit()
        {
            while (this.tokens.MoveNext())
            {
                var token = this.tokens.Current;

                switch(token.Kind)
                {
                    case TokenKind.StartOfFile:
                    case TokenKind.EndOfFile  :
                    case TokenKind.Space:
                    case TokenKind.NewLine:  continue;  

                    case TokenKind.AtSymbol: 
                    case TokenKind.StringLiteral:
                    case TokenKind.TokenCluster: yield return expression(); break;
                            
                    default: yield return error(diagnostics, DiagnosticIds.UnexpectedToken,token,"char {1}: Unexpected token: '{0}'",token.Text,token.Position); break;
                }       
            }        
        }

        ErrorNode error(List<AtDiagnostic> diagnostics,object diagnosticId,AtToken token,string f, object arg1, object arg2) 
        {
           return SyntaxFactory.ErrorNode(
                                    diagnostics,
                                    string.Format(f,arg1,arg2),
                                    token);
        }   

        //expression (stringLiteral | id)
        ExpressionSyntax expression()
        {
            skipWhiteSpace();

            if (isCurrent(TokenKind.AtSymbol)) return declarationExpression();

            var x = tokens.Current;

            throw new NotImplementedException();

            /*return x.Kind==TokenKind.TokenCluster ? new ExpressionSyntax("id",x)
                                                    : new ExpressionSyntax("string literal",x);*/
        }

    ExpressionClusterSyntax expressionCluster()
    {
        throw new NotImplementedException();
    }

    //declarationExpression "@TokenCluster[<>][;]"
    ExpressionSyntax declarationExpression()
    {
        //TODO: include trivia (whitespace, etc.) in nodes
        var nodes = new List<AtSyntaxNode>();
        Debug.Assert(tokens.Current.Kind==TokenKind.AtSymbol);
        var atSymbol = tokens.Current;
        var isClass = false;
        AtToken afterColon = null;
        ExpressionSyntax body = null;

        nodes.Add(atSymbol);
        
        if (tokens.LookAhead(1).Kind==TokenKind.TokenCluster)
        {
            tokens.MoveNext();
            var tc = tokens.Current;
            nodes.Add(tc);

            //<>
            AtToken lessThan = null;
            AtToken greaterThan = null;
            TypeParameterListSyntax typeParams = null;
            skipWhiteSpace();
            if (tokens.LookAhead(1).Kind==TokenKind.LessThan)
            {
                tokens.MoveNext();
                lessThan = tokens.Current;

                tokens.MoveNext();
                greaterThan = tokens.Current;

                Debug.Assert(greaterThan.Kind == TokenKind.GreaterThan);
                typeParams = SyntaxFactory.TypeParameterList(lessThan,greaterThan);
                nodes.Add(typeParams);
                isClass = true; 
            }

            //TODO: implement base class
            //: className<>
            skipWhiteSpace();
            if (isNext(TokenKind.Colon))
            {
                skip(TokenKind.Colon);
                skipWhiteSpace();
                if (isNext(TokenKind.TokenCluster))
                {
                    tokens.MoveNext();
                    afterColon = tokens.Current;
                   
                    //<>
                    skipWhiteSpace();
                    if (tokens.LookAhead(1).Kind==TokenKind.LessThan)
                    {
                        skip(TokenKind.LessThan,TokenKind.GreaterThan);
                    }             

                    skipWhiteSpace();
                    if (isNext(TokenKind.LeftBrace))
                    {
                        body = curlyBlock();
                        nodes.Add(body);
                    }
                }
                else
                {
                    return error();
                    //return error(diagnostics,DiagnosticIds.UnexpectedToken,"{0}:unexpected: {1}",tokens.Current.Position+1,tokens.LookAhead(1));
                }
            }

            
            //";" | "{...}"
            skipWhiteSpace();
            if (isNext(TokenKind.SemiColon))
            {                
                moveNext();
                nodes.Add(current);
            }
            else if (isNext(TokenKind.LeftBrace))
            {
                body = curlyBlock();
                nodes.Add(body);
            }


            if (isClass)
                return SyntaxFactory.ClassDeclaration(atSymbol,tc,typeParams,nodes);

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
        AtToken current {get{return tokens.Current;}}

        private void moveNext()
        {
            tokens.MoveNext();
        }

        private ExpressionSyntax error() 
        {
            throw new NotImplementedException();
        }

        private CurlyBlockSyntax curlyBlock()
        {
            AtToken leftBrace  = null;
            tokens.MoveNext();
            assertCurrent(TokenKind.LeftBrace);
            leftBrace = tokens.Current;
            var p = tokens.Position;

            var contents = new List<ExpressionSyntax>();
            while(!isNext(TokenKind.RightBrace))
            {
               contents.Add(expression());
            }
            tokens.MoveNext();

            return SyntaxFactory.CurlyBlock(leftBrace,contents,rightBrace:tokens.Current);
        }

        void assertCurrent(TokenKind tokenKind)
        {
            Debug.Assert(tokens.Current.Kind==tokenKind);
        }

        bool skip(params TokenKind[] tokenKinds)
        {
           for(int i=1;i < tokenKinds.Length+1;++i) 
           {
              if (tokens.LookAhead(i).Kind!=tokenKinds[i-1]) return false;
              else skipWhiteSpace();
           }

           for(int i=0;i < tokenKinds.Length;++i) tokens.MoveNext();
           return true;
        }

        void skipWhiteSpace()
        {
            while (isWhiteSpace(tokens.LookAhead(1))) tokens.MoveNext();
        }

        static bool isWhiteSpace(AtToken token)
        {
            return token.Kind==TokenKind.Space  || token.Kind==TokenKind.NewLine;
        }

        bool isNext(TokenKind kind, int i = 1)
        {
           return tokens.LookAhead(i).Kind==kind;
        }

        bool isCurrent(TokenKind tokenKind)
        {
           return tokens.Current.Kind == tokenKind;
        }

        void IDisposable.Dispose()
        {
            
        }
    }
}