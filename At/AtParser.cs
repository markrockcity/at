using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using At.Syntax;
using static At.OperatorAssociativity;
using static At.OperatorPosition;
using static At.SyntaxFactory;

namespace At
{
public class AtParser : IDisposable
{
    const int initialPrescedence = 0;

    public AtParser() : this(AtLexer.Default()) {}
    public AtParser(AtLexer lexer)
    {
        this.Lexer = lexer;
    }

    public AtLexer Lexer {get;}
    public OperatorDefinitionList Operators {get;}   = new OperatorDefinitionList();
    public ExpressionRuleList ExpressionRules {get;} = new ExpressionRuleList();

    //ParseCompilationUnit(input)
    public CompilationUnitSyntax ParseCompilationUnit(IEnumerable<char> input)
    {
        var tokens = new Scanner<AtToken>(Lexer.Lex(input));
        var diagnostics = new List<AtDiagnostic>();
        var expressions = this.expressions(tokens,diagnostics,initialPrescedence);
        var compilationUnitSyntax = SyntaxFactory.CompilationUnit(expressions.ToList(),diagnostics);

        diagnostics.AddRange (compilationUnitSyntax.DescendantNodes()
                                                    .OfType<ExpressionClusterSyntax>()
                                                    .Select(_=> AtDiagnostic.Create(DiagnosticIds.ExpressionCluster,(AtToken) _.ChildNodes().FirstOrDefault(x=>x.IsToken),"Expression cluster: "+_)));   
        return compilationUnitSyntax;
    }

    public static AtParser Default(AtLexer lexer = null)
    {
        var parser = new AtParser(lexer ?? AtLexer.Default());

        parser.ExpressionRules.Add(ExpressionRule.TokenClusterSyntax);
        parser.ExpressionRules.Add(ExpressionRule.NumericLiteral);
        parser.ExpressionRules.Add(ExpressionRule.Directive);
        
        parser.Operators.Add(0,OperatorDefinition.SemiColon);

        parser.Operators.Add
        (
            0,

            OperatorDefinition.StartDeclaration.AddRules
            (
                _=>_.NamespaceDeclaration,
                _=>_.VariableDeclaration,
                _=>_.MethodDeclaration,
                _=>_.TypeDeclaration
            )
        );

        parser.Operators.Add(1,OperatorDefinition.Comma);

        parser.Operators.Add(2,OperatorDefinition.PostRoundBlock);
        parser.Operators.Add(2,OperatorDefinition.PostCurlyBlock);
        parser.Operators.Add(2,OperatorDefinition.PrefixDeclaration);

        parser.Operators.Add(3,OperatorDefinition.ColonPair);

        parser.Operators.Add(4,OperatorDefinition.PostPointyBlock);

        parser.Operators.Add(10,OperatorDefinition.RoundBlock);
        parser.Operators.Add(10,OperatorDefinition.CurlyBlock);
       
        return parser;
    }

    class Comma : IOperatorDefinition
    {
        public OperatorAssociativity Associativity    => OperatorAssociativity.List;
        public OperatorPosition      OperatorPosition => Infix;
        public TokenKind             TokenKind        => TokenKind.Comma;
        public ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes) => Binary(this,nodes);
    }

    class PostCircumfix : ICircumfixOperator
    {
        readonly Func<IOperatorDefinition,AtSyntaxNode[],ExpressionSyntax> createExpression;
        public PostCircumfix(TokenKind tk1, TokenKind tk2, Func<IOperatorDefinition,AtSyntaxNode[],ExpressionSyntax> e)
        {
            TokenKind = tk1;
            EndDelimiterKind = tk2;
            createExpression = e;
        }
        public OperatorAssociativity Associativity    => OperatorAssociativity.List;
        public OperatorPosition      OperatorPosition => OperatorPosition.PostCircumfix;
        public TokenKind             TokenKind {get;}
        public ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes) => createExpression(this,nodes);
        public TokenKind EndDelimiterKind {get;}
    }

    public static AtParser SyntaxPattern(AtLexer lexer = null)
    {
        var parser = new AtParser(lexer ?? AtLexer.Default());
        parser.Lexer.TokenRules.Remove(TokenRule.Colon);

        parser.ExpressionRules.Add(ExpressionRule.TokenClusterSyntax);

        //x,y
        parser.Operators.Add(1,new Comma());

        //x()
        parser.Operators.Add(2,new PostCircumfix(TokenKind.OpenParenthesis,TokenKind.CloseParenthesis,(src,nodes)=>PostBlock(src,nodes[0],RoundBlock(src,nodes.Skip(1).ToArray()))));

        //x[]
        parser.Operators.Add(2,new PostCircumfix(TokenKind.OpenBracket,TokenKind.CloseBracket,(src,nodes)=>PostBlock(src,nodes[0],SquareBlock(src,nodes.Skip(1).ToArray()))));

        return parser;
    }

    //ParseExpression(input)
    public ExpressionSyntax ParseExpression(IEnumerable<char> input)
    {
        var tokens = new Scanner<AtToken>(Lexer.Lex(input));
        var diagnostics = new List<AtDiagnostic>();

        tokens.MoveNext(); //move to first token
        Debug.Assert(tokens.Position==0);
        var expr = expression(tokens,diagnostics,initialPrescedence);
        return expr;
    }

    //expressions()
    IEnumerable<ExpressionSyntax> expressions(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics,int prescendence)
    {
        if (tokens.Position < 0) 
            tokens.MoveNext();

        while (!tokens.End)
            yield return expression(tokens,diagnostics,prescendence);      
    }

    //error()
    internal static ErrorNode error(List<AtDiagnostic> diagnostics,string diagnosticId,AtToken token,string f, params object[] args) 
    {
        diagnostics.Add(new AtDiagnostic(diagnosticId,token,string.Format(f,args)));

        return ErrorNode(diagnostics, string.Format(f,args),token);
    }   

    //expression()
    ExpressionSyntax expression(Scanner<AtToken> tokens,  List<AtDiagnostic> diagnostics, int prescedence, int lastPosition = -1, TokenKind? endDelimiterKind = null)
    {           
        AtToken start = null;
        IOperatorDefinition startOp = null;
        ExpressionSyntax leftOperand = null;

        //predicate() - closes over {prescendence}
        Func<IOperatorDefinition,bool> predicate = (IOperatorDefinition _) => _.TokenKind==tokens.Current?.Kind && Operators.Prescedence(_) >= (startOp != null ? Operators.Prescedence(startOp) : prescedence);
        //bool predicate(IOperatorDefinition _) => _.TokenKind==tokens.Current?.Kind && Operators.Prescedence(_) >= (startOp != null ? Operators.Prescedence(startOp) : prescedence);

        //BEGIN PARSING:

        //No Operators??
        if (Operators.Count==0)
            return expressionCluster(new AtToken[0],tokens,null,diagnostics);
        
        //End of [post]circumfix expression? (1)
        if (tokens.Current.Kind == endDelimiterKind)
            return null;

        //End operator at beginning? (e.g., ";;")
        var endOps = Operators.Where(_=>_.OperatorPosition==End);   
        var endOp = endOps.FirstOrDefault(predicate);
        if (endOp != null)
            return endOp.CreateExpression(tokens.Consume());
        
        //Start operator? ("@[x;]", "if[(x){ ... }]", etc.)
        var startOps = Operators.Where(_=>_.OperatorPosition==Start);
        startOp = startOps.FirstOrDefault(predicate);
        if (startOp != null)
        {
            start = tokens.Consume();
            if (tokens.Current.Kind == endDelimiterKind)
                return startOp.CreateExpression(start);
        }
        
        //Prefix op ?
        var prefixOps = Operators.Where(_=>_.OperatorPosition==Prefix);
        var prefixOp = prefixOps.FirstOrDefault(predicate);
        if (prefixOp != null)
        {
            var prefixOpToken = tokens.Consume();
            var e = expression(tokens,diagnostics,Operators.Prescedence(prefixOp));
            leftOperand = prefixOp.CreateExpression(prefixOpToken, e);
        }        
        else 
        {
           //Circumfix op?
           var circumfixOps = Operators.Where(_=>_.OperatorPosition==OperatorPosition.Circumfix);
           var circumfixOp =  circumfixOps.FirstOrDefault(predicate);
           if (circumfixOp != null)
           {

                var startDelimiter = tokens.Consume();
                var op = circumfixOp as ICircumfixOperator;
                var _endDelimiterKind = op != null ? op.EndDelimiterKind : circumfixOp.TokenKind;
                var list = new List<AtSyntaxNode> {startDelimiter, };
                while(tokens.Current.Kind != _endDelimiterKind)
                {
                    var e = expression(tokens,diagnostics,0,tokens.Position,_endDelimiterKind);
                    if (e != null)
                        list.Add(e);
                } 
                list.Add(tokens.Consume()); //assuming end delimiter
                leftOperand = circumfixOp.CreateExpression(list.ToArray());
            }

            //...
            else
            {
                //checks passed-in position from recursive call to prevent stack overflow
                if  (lastPosition != tokens.Position) 
                {
                    leftOperand = expression(tokens,diagnostics,startOp != null ? Operators.Prescedence(startOp) : prescedence,  tokens.Position);
                }
            
                //same position as before? check expression rules
                else
                {
                    var exprRule = getRule(tokens);
                    if (exprRule != null)
                    {
                        var pos = tokens.Position;

                        leftOperand = exprRule.ParseExpression(tokens);

                        if (leftOperand != null && pos == tokens.Position && leftOperand.Text.Length > 0)
                            tokens.MoveNext();
                    }

                    if (leftOperand == null)
                    {
                        leftOperand = expressionCluster(new[]{tokens.Consume()},null,null,diagnostics); 
                    }
                }
            }
        }
    

        //End?
        endOp = endOps.FirstOrDefault(predicate);
        if (endOp != null)
            return endOp.CreateExpression(leftOperand != null ? new AtSyntaxNode[]{leftOperand, tokens.Consume()} : new AtSyntaxNode[] {tokens.Consume()} );

        //Postcircumfix?
        var postCircumfixOps = Operators.Where(_=>_.OperatorPosition==OperatorPosition.PostCircumfix);
        var postCircumfixOp = postCircumfixOps.FirstOrDefault(predicate);
        while (postCircumfixOp != null) //compund postcircumfix expressions
        {
            var startDelimiter = tokens.Consume();
            var op = postCircumfixOp as ICircumfixOperator;
            var _endDelimiterKind = op != null ? op.EndDelimiterKind : postCircumfixOp.TokenKind;
            var list = new List<AtSyntaxNode> {leftOperand, startDelimiter, };
            while(tokens.Current.Kind != _endDelimiterKind)
            {
                var e = expression(tokens,diagnostics,0,tokens.Position,_endDelimiterKind);
                if (e != null)
                    list.Add(e);
            } 
            list.Add(tokens.Consume()); //assuming end delimiter
            leftOperand = postCircumfixOp.CreateExpression(list.ToArray());

            postCircumfixOp = postCircumfixOps.FirstOrDefault(predicate);
        }

        //Postfix?
        var postfixOps = Operators.SelectMany(_=>_).Where(_=>_.OperatorPosition==Postfix);
        var postfixOp = postfixOps.FirstOrDefault(predicate);
        if (postfixOp != null)
        {
            var postfixOpToken = tokens.Consume();
            leftOperand = postfixOp.CreateExpression(leftOperand,postfixOpToken);
        }

        //Binary op?
        IOperatorDefinition binaryOp = null;
        foreach(var ops in Operators)
        {
            binaryOp = ops.FirstOrDefault(_=>predicate(_) && _.OperatorPosition==Infix);
            if (binaryOp != null)
            {
               //Same precedence, but not right-associative—deal with this "later"
               if (Operators.Prescedence(binaryOp)==prescedence && binaryOp.Associativity!=Right)
                 break;
               
               var opToken = tokens.Consume();
               var rightOperand = expression(tokens,diagnostics,Operators.Prescedence(binaryOp));
               leftOperand = binaryOp.CreateExpression(leftOperand, opToken, rightOperand);
            }
        }

        //End?
        endOp = endOps.FirstOrDefault(predicate);
        if (endOp != null)
            return endOp.CreateExpression(leftOperand != null ? new AtSyntaxNode[]{leftOperand, tokens.Consume()} : new AtSyntaxNode[] {tokens.Consume()} );

        Debug.Assert(leftOperand != null);

        return    endOp != null 
                    ? endOp.CreateExpression(leftOperand != null 
                                                ? new AtSyntaxNode[]{leftOperand, tokens.Consume()} 
                                                : new AtSyntaxNode[]{tokens.Consume()})
                : start != null 
                    ? startOp.CreateExpression(start,leftOperand) 

                : leftOperand;
    }

    IExpressionRule getRule(Scanner<AtToken> tokens)
    {
        int k = -1;
        IList<IExpressionRule> lastMatches = null, matches;

        if (ExpressionRules.Count > 0)
        {
            k = -1;
            //TODO: instead of re-querying {ExpressionRules} all the time, just do {lastMatches}
            while((matches = ExpressionRules.Matches(tokens,++k)).Count>0)
            {
                lastMatches = matches;

                if (tokens.End)
                    break;
            }

            if (lastMatches?.Count > 0)
                return lastMatches[0];
        }    

        return null;    
    }

    ExpressionClusterSyntax expressionCluster(IEnumerable<AtToken> tokens1, Scanner<AtToken> tokens2, IExpressionSource expSrc, List<AtDiagnostic> diagnostics)
    {
        var nodes = new List<AtSyntaxNode>(tokens1);
        while (!tokens2?.End ?? false)
            nodes.Add(tokens2.Consume());
        return new ExpressionClusterSyntax(nodes,expSrc,diagnostics);
    }

    /*
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
    }* /

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

    */

    void IDisposable.Dispose()
    {
            
    }
}
}