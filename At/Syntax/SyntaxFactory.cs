using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using At.Syntax;

namespace At
{
public class SyntaxFactory
{
    public static ExpressionSyntax Apply(ExpressionSyntax subj, params ExpressionSyntax[] args)
    {
        return new ApplicationSyntax(subj,args);
    }

    internal static BinaryExpressionSyntax Binary(IExpressionSource expSrc, params AtSyntaxNode[] nodes)
    {
        return new BinaryExpressionSyntax((ExpressionSyntax)nodes[0],nodes[1].AsToken(),(ExpressionSyntax)nodes[2],expSrc);
    }

    public static BinaryExpressionSyntax Binary
    (
        ExpressionSyntax          leftOperand,
        AtToken                   @operator,
        ExpressionSyntax          rightOperand,
        IExpressionSource         exprSrc = null,
        IEnumerable<AtDiagnostic> diagnostics = null){

        return new BinaryExpressionSyntax(leftOperand,@operator,rightOperand,exprSrc,diagnostics);
    }

    /// <summary>Invocation expression syntax with no arguments, e.g., <c>foo()</c></summary>
    /// <param name="expr">Expression syntax for target of invocation</param>
    /// <param name="startDelimiter"></param>
    /// <param name="endDelimiter"></param>
    /// <param name="exprSrc"></param>
    /// <returns></returns>
    public static ExpressionSyntax Invocation(ExpressionSyntax expr,AtToken startDelimiter,AtToken endDelimiter,IExpressionSource exprSrc=null,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new InvocationExpressionSyntax(expr, List<ArgumentSyntax>(startDelimiter,endDelimiter), exprSrc, diagnostics);
    }

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax expr, AtToken startDelimiter, SeparatedSyntaxList<ArgumentSyntax> args, AtToken endDelimiter, IExpressionSource expSrc= null, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new InvocationExpressionSyntax(expr, List(startDelimiter,args,endDelimiter), expSrc, diagnostics);
    }

    public static ArgumentSyntax Argument(ExpressionSyntax e, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new ArgumentSyntax(e,diagnostics);
    }

    internal static CurlyBlockSyntax CurlyBlock(IExpressionSource expSrc,params AtSyntaxNode[] nodes)
    {
        if (nodes.Length < 2)
            throw new ArgumentException(nameof(nodes),"Should have at least 2 nodes.");

        var contents = (nodes.Length > 2)
                            ? nodes.Skip(1).Take(nodes.Length - 2).Cast<ExpressionSyntax>()
                            : null;

        return new CurlyBlockSyntax(nodes[0].AsToken(),contents,nodes.Last().AsToken(),expSrc,null);
    }


    public static CurlyBlockSyntax CurlyBlock(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken rightDelimiter,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        if (startDelimiter == null)
            throw new ArgumentNullException(nameof(startDelimiter));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (rightDelimiter == null)
            throw new ArgumentNullException(nameof(rightDelimiter));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new CurlyBlockSyntax(startDelimiter,contents,rightDelimiter,expSrc,diagnostics);
    }

    public static CompilationUnitSyntax CompilationUnit(IEnumerable<ExpressionSyntax> exprs,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new CompilationUnitSyntax(exprs,diagnostics);
    }

    // **directive n[;]**
    public static DirectiveSyntax Directive(AtToken directive,NameSyntax n,List<AtSyntaxNode> nodes,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(directive,nameof(directive));
        checkNull(n,nameof(n));
        return new DirectiveSyntax(directive,n,nodes,expSrc,diagnostics);
    }

    public static SyntaxPattern SyntaxPattern(ExpressionSyntax e)
    {
        //X,Y
        if (e is BinaryExpressionSyntax b)
            return new SyntaxPattern(null,content: new[] { SyntaxPattern(b.Left),SyntaxPattern(b.Right) });

        if (e is PostBlockSyntax pb)
        {
            string token1 = null, token2 = null;
            var contentSpecified = false;

            //X[Y]
            if (pb.Block is SquareBlockSyntax sb)
            {
                if (sb.Content.Count == 1)
                {
                    var c0 = sb.Content[0];

                    if (c0 is TokenClusterSyntax || c0 is LiteralExpressionSyntax)
                    {
                        syntaxPatternKey(pb.Operand.Text, out string txt,out string key);
                        return new SyntaxPattern(txt, token1: c0.Text, key: key);
                    }

                    throw new NotSupportedException(c0 + "\r\n\r\n" + sb.Text + "\r\n\r\n" + sb.PatternStrings().First());
                }

                throw new NotSupportedException(pb.Block.Text + "\r\n\r\n" + pb.Block.PatternStrings().First());
            }

            //X(...)
            else if (pb.Block is RoundBlockSyntax)
            {
                var text = pb.Operand.Text;
                var content = new List<SyntaxPattern>();

                //X[Y](...)         
                if (pb.Operand is PostBlockSyntax pb2)
                {
                    text = pb2.Operand.Text;

                    //X[Y,Z](...)
                    if (pb2.Block.Content[0] is BinaryExpressionSyntax b2) //X[Y,Z]
                    {
                        token1 = b2.Left.Text;
                        token2 = b2.Right.Text;
                    }
                    else //X[Y]
                    {
                        token1 = pb2.Block.Content[0].Text;
                    }
                }

                //X()
                if (pb.Block.Content.Count==0)
                {
                    contentSpecified = true;
                }
                //X(...)
                else if (pb.Block.Content.Count == 1)
                {
                    //X(Y,Z)
                    var c0 = pb.Block.Content[0];
                    if (c0 is BinaryExpressionSyntax b2)
                    {
                        contentSpecified = true;
                        content.Add(SyntaxPattern(b2.Left));
                        content.Add(SyntaxPattern(b2.Right));
                    }

                    //X(Y)
                    else if (c0 is TokenClusterSyntax || c0 is LiteralExpressionSyntax)
                    {
                        contentSpecified = true;
                        content.Add(SyntaxPattern(c0));
                    }

                    //X(Y[Z])
                    else if (c0 is PostBlockSyntax pb3)
                    {
                        contentSpecified = true;
                        content.Add(SyntaxPattern(c0));
                    }
                    else
                    {
                        throw new NotSupportedException(c0 + "\r\n\r\n" + pb.Block.Text + "\r\n\r\n" + pb.Block.PatternStrings().First());
                    }
                }
                else
                {
                    throw new NotSupportedException
                    (
                        pb.Block.Content.Count + "\r\n"
                        + string.Join(", ",pb.Block.Content.AsEnumerable()) + "\r\n"
                        + pb.Block.PatternStrings().First()
                    );
                }

                string txt, key;
                syntaxPatternKey(text,out txt,out key);
                return new SyntaxPattern(txt,token1,token2,key: key,content: contentSpecified ? content.ToArray() : null);
            }
        }

        //
        if (e is TokenClusterSyntax tc)
        {
            string text, key;
            syntaxPatternKey(tc.TokenCluster.Text,out text,out key);
            return new SyntaxPattern(text,key: key);
        }

        if (e is EmptyExpressionSyntax)
            return new SyntaxPattern("");

        throw new NotSupportedException(e.PatternStrings().First()); 
     }

    public static SyntaxPattern ParseSyntaxPattern(string patternString)
    {
        using (var p = AtParser.CreateSyntaxPatternParser())
        {    
            var e = p.ParseExpression(patternString);
            return SyntaxPattern(e);   
        }      
    }

    static void syntaxPatternKey(string s, out string text, out string key)
    {
        if (s.IndexOf(':') > -1) 
        {
            var x = s.Split(':');
            key  = x[0];
            text = x[1];
        }
        else
        {
            text = s;
            key = null;
        }
    }

    public static EmptyExpressionSyntax Empty(IExpressionSource expSrc, AtSyntaxNode endToken)
    {
        return new EmptyExpressionSyntax(expSrc,endToken);    
    }

    public static ErrorNode ErrorNode(IList<AtDiagnostic> diagnostics,string msg, AtSyntaxNode node)
    {
         return new ErrorNode(diagnostics, msg,node);
    }

    internal static ListSyntax<T> List<T>(string leftToken,string rightToken) where T : AtSyntaxNode
    {
        return new ListSyntax<T>(ParseToken(leftToken,markAsMissing:true),new SeparatedSyntaxList<T>(null,new AtSyntaxNode[0]),ParseToken(rightToken,markAsMissing:true),null);
    }

    public static ListSyntax<T> List<T>(AtToken startDelimiter,AtToken endDelimiter, IEnumerable<AtDiagnostic> diagnostics = null) where T : AtSyntaxNode
    {
        return new ListSyntax<T>(startDelimiter,new SeparatedSyntaxList<T>(null,new AtSyntaxNode[0]),endDelimiter,diagnostics);
    }


    public static ListSyntax<T> List<T>(AtToken startDelimiter, SeparatedSyntaxList<T> list,AtToken endDelimiter, IEnumerable<AtDiagnostic> diagnostics = null) where T : AtSyntaxNode
    {
        //checkNull(startDelimiter,nameof(startDelimiter));        
        //checkNull(endDelimiter,nameof(endDelimiter));

        if (list == null)
            return List<T>(startDelimiter,endDelimiter,diagnostics);

        checkNull(list,nameof(list));
        return new ListSyntax<T>(startDelimiter,list,endDelimiter,diagnostics);
    }

    public static LiteralExpressionSyntax LiteralExpression(AtToken atToken, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(atToken,nameof(atToken)); 
        return new LiteralExpressionSyntax(atToken,new[] {atToken},expDef,diagnostics);
    }

    public static MethodDeclarationSyntax MethodDeclaration(AtToken atSymbol,AtToken tc,ListSyntax<ParameterSyntax> methodParams,NameSyntax returnType,ExpressionSyntax body,IEnumerable<AtSyntaxNode> nodes,IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics = null)
    {
       return new MethodDeclarationSyntax(atSymbol,tc,methodParams,returnType,body,nodes,expDef,diagnostics);
    }

    public static NamespaceDeclarationSyntax NamespaceDeclaration(AtToken atSymbol,AtToken identifier, IEnumerable<DeclarationSyntax> members, IEnumerable<AtSyntaxNode> nodes, IExpressionSource expDef, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new NamespaceDeclarationSyntax(atSymbol,identifier,members,nodes,expDef,diagnostics);
    }

    public static NameSyntax NameSyntax(AtToken identifier, ListSyntax<NameSyntax> typeArgs = null)
    {
        checkNull(identifier,nameof(identifier));  
        return new NameSyntax(identifier, typeArgs);    
    }

    public static NameSyntax NameSyntax(ExpressionSyntax e)
    {
        var n = e as NameSyntax;
        if (n != null)
            return n;

        var tc = e as TokenClusterSyntax;
        if (tc != null)
            return NameSyntax(tc.TokenCluster);

        var pb = e as PostBlockSyntax;
        if (pb != null)
        {
            var identifier = (TokenClusterSyntax) pb.Operand;
            var typeArgs   = TypeList(pb.Block);
            return NameSyntax(identifier.TokenCluster,typeArgs);
        }

        

        throw new NotImplementedException(e.GetType()+"–"+e.PatternStrings().First());
    }
    
    public static ParameterSyntax Parameter(ExpressionSyntax e, IExpressionSource exprSrc)
    {
        if (e is TokenClusterSyntax tc)
            return new ParameterSyntax(tc.TokenCluster,null,exprSrc,null);

        /*
        var pb = e as PostBlockSyntax;
        if (pb != null)
        {
            var identifier = (TokenClusterSyntax) pb.Operand;
            var typeArgs   = TypeList(pb.Block);
            return Parameter(identifier.TokenCluster,typeArgs);
        }*/

        throw new NotImplementedException(e.PatternStrings().First());
    }


    public static ParameterSyntax Parameter(AtToken identifier, NameSyntax paramType, IExpressionSource exprSrc, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(identifier,nameof(identifier));
        return new ParameterSyntax(identifier,paramType,exprSrc, diagnostics);
    }

    public static ExpressionSyntax ParseExpression(string text)
    {
        using (var parser = AtParser.CreateDefaultParser())
        {
            var expr = parser.ParseExpression(text);
            return expr;
        }
    }

    public static AtToken ParseToken(string text, bool markAsMissing = false)
    {
        using (var lexer = AtLexer.DefaultLexer)
        {
            var token = lexer.Lex(text).FirstOrDefault();

            if (token==null)
                throw new ArgumentException($"text = {(text != null ? $"\"{text}\"" : "<null>")}",nameof(text));

            token.IsMissing = markAsMissing;
            return token;
        }
    }

    public static PointyBlockSyntax PointyBlock(IExpressionSource expSrc,params AtSyntaxNode[] nodes)
    {
        if (nodes.Length < 2)
            throw new ArgumentException(nameof(nodes),"Should have at least 2 nodes.");

        var contents = (nodes.Length > 2)
                            ? nodes.Skip(1).Take(nodes.Length - 2).Cast<ExpressionSyntax>()
                            : null;

        return new PointyBlockSyntax(nodes[0].AsToken(),contents,nodes.Last().AsToken(),expSrc,null);
    }

    public static PointyBlockSyntax PointyBlock(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken endDelimiter,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        if (startDelimiter == null)
            throw new ArgumentNullException(nameof(startDelimiter));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (endDelimiter == null)
            throw new ArgumentNullException(nameof(endDelimiter));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new PointyBlockSyntax(startDelimiter,contents,endDelimiter,expSrc,diagnostics);
    }

    internal static ExpressionSyntax PostBlock(IExpressionSource expSrc, params AtSyntaxNode[] nodes)
    {
        if (nodes.Length != 2)
            throw new ArgumentException(nameof(nodes),"Should have 2 nodes.");
        
         return new PostBlockSyntax((ExpressionSyntax) nodes[0],(BlockSyntax) nodes[1],expSrc ,null);                        
    }

    public static ExpressionSyntax PostBlock(ExpressionSyntax operand, BlockSyntax postBlock,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
         return new PostBlockSyntax(operand,postBlock,expSrc,diagnostics);
    }


    internal static RoundBlockSyntax RoundBlock(IExpressionSource expSrc,params AtSyntaxNode[] nodes)
    {
        if (nodes.Length < 2) 
            throw new ArgumentException(nameof(nodes),"Should have at least 2 nodes (delimiters).");

        var contents = (nodes.Length > 2)
                            ? nodes.Skip(1).Take(nodes.Length - 2).Cast<ExpressionSyntax>()
                            : null;

        return new RoundBlockSyntax(nodes[0].AsToken(),contents,nodes.Last().AsToken(),expSrc,null);
    }


    public static RoundBlockSyntax RoundBlock(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken rightDelimiter,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        if (startDelimiter == null)
            throw new ArgumentNullException(nameof(startDelimiter));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (rightDelimiter == null)
            throw new ArgumentNullException(nameof(rightDelimiter));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new RoundBlockSyntax(startDelimiter,contents,rightDelimiter,expSrc,diagnostics);
    }

    public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(params AtSyntaxNode[] nodes) where TNode : AtSyntaxNode
        => SeparatedList<TNode>(nodes.AsEnumerable());

    public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<AtSyntaxNode> nodes) where TNode : AtSyntaxNode
    {
        return new SeparatedSyntaxList<TNode>(null,nodes);
    }

    internal static SquareBlockSyntax SquareBlock(IExpressionSource expSrc, params AtSyntaxNode[] nodes)
    {
        if (nodes.Length < 2)
            throw new ArgumentException(nameof(nodes),"Should have at least 2 nodes  (delimiters).");

        var contents = (nodes.Length > 2)
                            ? nodes.Skip(1).Take(nodes.Length - 2).Cast<ExpressionSyntax>()
                            : null;

        return new SquareBlockSyntax(nodes[0].AsToken(),contents,nodes.Last().AsToken(),expSrc,null);
    }

    public static SquareBlockSyntax SquareBlock(AtToken startDelimiter, IEnumerable<ExpressionSyntax> contents, AtToken rightDelimiter,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        if (startDelimiter == null)
            throw new ArgumentNullException(nameof(startDelimiter));
        if (contents == null)
            throw new ArgumentNullException(nameof(contents));
        if (rightDelimiter == null)
            throw new ArgumentNullException(nameof(rightDelimiter));

        if (contents.Any(_=>_==null))
            throw new ArgumentException(nameof(contents),"contents contains a null reference");

        return new SquareBlockSyntax(startDelimiter,contents,rightDelimiter,expSrc,diagnostics);
    }

    
    public static ExpressionSyntax TokenClusterExpression(AtToken tokenCluster,ExpressionRule expSrc, IEnumerable<AtDiagnostic> diagnostics = null)
    {
        return new TokenClusterSyntax(tokenCluster,expSrc,diagnostics);

    }

    public static TypeDeclarationSyntax TypeDeclaration
    (
        AtToken atSymbol, 
        AtToken identifier, 
        ListSyntax<ParameterSyntax>  typeParameterList,
        ListSyntax<NameSyntax> baseList,
        IEnumerable<DeclarationSyntax> members,
        IEnumerable<AtSyntaxNode> nodes,
        IExpressionSource expSrc,
        IEnumerable<AtDiagnostic> diagnostics = null){

        checkNull(identifier,nameof(identifier));
        return new TypeDeclarationSyntax(atSymbol,identifier,typeParameterList,baseList,members,expSrc,nodes,diagnostics);
    }

    //[... : ] <T,U<V>>, W, X
    public static ListSyntax<NameSyntax> TypeList(ExpressionSyntax e)
    {
        var block = e as BlockSyntax;
        if (block != null)
            return TypeList(block);

        var tc = e as TokenClusterSyntax;
        if (tc != null)
            return List(null,SeparatedList<NameSyntax>(NameSyntax(tc)),null);

        var pb = e as PostBlockSyntax;
        if (pb != null)
        {
            var identifier = (TokenClusterSyntax) pb.Operand;
            var typeArgs   = TypeList(pb.Block);
            return List(null,SeparatedList<NameSyntax>(NameSyntax(identifier.TokenCluster,typeArgs)),null);
        }

        throw new NotImplementedException(e.PatternStrings().First());        
    }
    //<T,U<V>>
    public static ListSyntax<NameSyntax> TypeList(BlockSyntax block)
    {        
        var nodes = new List<AtSyntaxNode>();
        switch(block.Content.Count)
        {
            case 1:
                 var b = block.Content[0] as BinaryExpressionSyntax;
                 if (b != null)
                 {
                     nodes.Add(NameSyntax(b.Left));
                     nodes.Add(b.Operator);
                     nodes.Add(NameSyntax(b.Right));
                 }
                 else
                 {
                    nodes.Add(NameSyntax(block.Content[0]));
                 }  
                 break;

            default:
                nodes.AddRange(block.Content.Select(NameSyntax));
                break;
        }

        return List(block.StartDelimiter,SeparatedList<NameSyntax>(nodes.AsEnumerable()),block.EndDelimiter);    
    }

    //...<T,U,V,...>
    public static ListSyntax<ParameterSyntax> TypeParameterList(BlockSyntax block, IExpressionSource exprSrc)
    {
        var nodes = new List<AtSyntaxNode>();
        switch(block.Content.Count)
        {
            case 1:
                 var b = block.Content[0] as BinaryExpressionSyntax;
                 if (b != null)
                 {
                     nodes.Add(Parameter(b.Left,exprSrc));
                     nodes.Add(b.Operator);
                     nodes.Add(Parameter(b.Right,exprSrc));
                 }
                 else
                 {
                     nodes.Add(Parameter(block.Content[0],exprSrc));
                 }  
                 break;

            default:
                nodes.AddRange(block.Content.Select(_=>Parameter(_,exprSrc)));
                break;
        }

        return List(block.StartDelimiter,SeparatedList<ParameterSyntax>(nodes.AsEnumerable()),block.EndDelimiter);    
    }

 

    public static VariableDeclarationSyntax VariableDeclaration(AtToken atSymbol,AtToken identifier,NameSyntax type,object value,IEnumerable<AtSyntaxNode> nodes,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics = null)
    {
        checkNull(identifier,nameof(identifier));
        return new VariableDeclarationSyntax(atSymbol,identifier,type,nodes,expSrc,diagnostics);
    }

    private static void checkNull(object obj, string name)
    {
        if (obj == null)
            throw new ArgumentNullException(name);

        if (obj is IEnumerable && ((IEnumerable) obj).Cast<object>().Any(_=>_== null))
            throw new ArgumentNullException(name,name+" contains a null reference");        
    }
}
}