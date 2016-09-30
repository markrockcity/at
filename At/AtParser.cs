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

    public AtParser() : this(AtLexer.CreateDefaultLexer()) {}
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

    public static AtParser CreateDefaultParser(AtLexer lexer = null)
    {
        var parser = new AtParser(lexer ?? AtLexer.CreateDefaultLexer());

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

    //for SyntaxPattern parser
    class CommaOperatorDefinition : IOperatorDefinition
    {
        public OperatorAssociativity Associativity    => OperatorAssociativity.List;
        public OperatorPosition      OperatorPosition => Infix;
        public TokenKind             TokenKind        => TokenKind.Comma;
        public ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes) => Binary(this,nodes);
    }

    //for SyntaxPattern parser
    class PostCircumfixOperatorDefinition : ICircumfixOperatorDefinition
    {
        readonly Func<IOperatorDefinition,AtSyntaxNode[],ExpressionSyntax> createExpression;
        public PostCircumfixOperatorDefinition(TokenKind tk1, TokenKind tk2, Func<IOperatorDefinition,AtSyntaxNode[],ExpressionSyntax> e)
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

    public static AtParser CreateSyntaxPatternParser(AtLexer lexer = null)
    {
        var parser = new AtParser(lexer ?? AtLexer.CreateDefaultLexer());
        parser.Lexer.TokenRules.Remove(TokenRule.Colon);

        parser.ExpressionRules.Add(ExpressionRule.TokenClusterSyntax);

        //x,y
        parser.Operators.Add(1,new CommaOperatorDefinition());

        //x()
        parser.Operators.Add(2,new PostCircumfixOperatorDefinition(TokenKind.OpenParenthesis,TokenKind.CloseParenthesis,(src,nodes)=>PostBlock(src,nodes[0],RoundBlock(src,nodes.Skip(1).ToArray()))));

        //x[]
        parser.Operators.Add(2,new PostCircumfixOperatorDefinition(TokenKind.OpenBracket,TokenKind.CloseBracket,(src,nodes)=>PostBlock(src,nodes[0],SquareBlock(src,nodes.Skip(1).ToArray()))));

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
                var op = circumfixOp as ICircumfixOperatorDefinition;
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
            var op = postCircumfixOp as ICircumfixOperatorDefinition;
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

    void IDisposable.Dispose()
    {
            
    }
}
}