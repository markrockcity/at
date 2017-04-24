using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;
using static At.SyntaxFactory;

namespace At
{                                                                       

public interface IExpressionTransformation : IExpressionSource
{
    bool Matches(ExpressionSyntax e);
    ExpressionSyntax TransformExpression(ExpressionSyntax e);
}

public class ExpressionTransformation : IExpressionTransformation, IEnumerable
{
    //InvocationExpression
    public readonly static ExpressionTransformation InvocationExpression = new ExpressionTransformation
    {
        //f()
        {
            "PostBlock(tc:TokenCluster,r:Round())", (e, nodes, @this) =>
            {
                var expr = nodes.GetNode<TokenClusterSyntax>("tc");
                var r = nodes.GetNode<RoundBlockSyntax>("r");
                return Invocation(expr,r.StartDelimiter,r.EndDelimiter,exprSrc:@this);
            }
        },

        //f(x,y)
        {
            "PostBlock(tc:TokenCluster,r:Round(b:Binary[Comma]))", (e, nodes, @this) =>
            {
                var expr = nodes.GetNode<TokenClusterSyntax>("tc");
                var r = nodes.GetNode<RoundBlockSyntax>("r");
                var b = nodes.GetNode<BinaryExpressionSyntax>("b");
                return Invocation(expr,r.StartDelimiter,SeparatedList<ArgumentSyntax>(Argument(b.Left),b.Operator,Argument(b.Right)),r.EndDelimiter,@this);
            }
        },

        //f(x)
        {
            "PostBlock(tc:TokenCluster,r:Round(e:Expr))", (e, nodes, @this) =>
            {
                var expr = nodes.GetNode<TokenClusterSyntax>("tc");
                var r = nodes.GetNode<RoundBlockSyntax>("r");
                var expr2 = nodes.GetExpression("e");
                return Invocation(expr,r.StartDelimiter,SeparatedList<ArgumentSyntax>(Argument(expr2)),r.EndDelimiter,@this);
            }
        },
    };

    readonly List<(SyntaxPattern pattern,Func<ExpressionSyntax,SyntaxNodeMatchCollection,IExpressionTransformation,ExpressionSyntax> tx)> tuples;

    public ExpressionTransformation(params (string syntaxPattern, Func<ExpressionSyntax,SyntaxNodeMatchCollection,IExpressionTransformation,ExpressionSyntax> tx)[] tuples) 
    {
        this.tuples = new List<(SyntaxPattern pattern,Func<ExpressionSyntax,SyntaxNodeMatchCollection,IExpressionTransformation,ExpressionSyntax> tx)>();
    }
    
    public ExpressionTransformation(string syntaxPattern, Func<ExpressionSyntax,SyntaxNodeMatchCollection,IExpressionTransformation,ExpressionSyntax> tx) 
    {
        tuples = new List<(SyntaxPattern pattern,Func<ExpressionSyntax,SyntaxNodeMatchCollection,IExpressionTransformation,ExpressionSyntax> tx)>();
        tuples.Add((ParseSyntaxPattern(syntaxPattern),tx));
    }

    public void Add(string syntaxPattern, Func<ExpressionSyntax,SyntaxNodeMatchCollection,IExpressionTransformation,ExpressionSyntax> tx)
    {
        tuples.Add((ParseSyntaxPattern(syntaxPattern),tx));
    }

    public bool Matches(ExpressionSyntax e)
    {
        foreach(var t in tuples)
            if (e.MatchesPattern(t.pattern))
                return true;

        return false;
    }

    public ExpressionSyntax TransformExpression(ExpressionSyntax e)
    {
         var dict = new Dictionary<string,AtSyntaxNode>();
         var t  = tuples.First(_=>e.MatchesPattern(_.pattern,dict));
         var nodes = new SyntaxNodeMatchCollection(e.nodes.ToArray(),dict);
         var e2 = t.tx(e,nodes,this); 
         return e2;
    }

    ExpressionSyntax IExpressionSource.CreateExpression(params AtSyntaxNode[] nodes)
        => (nodes.Length==1 && nodes[0] is ExpressionSyntax e) 
                    ? TransformExpression(e)
                    : throw new NotSupportedException(string.Join<AtSyntaxNode>(" ",nodes));

    IEnumerator IEnumerable.GetEnumerator() => tuples.GetEnumerator();
}

public class ExpressionTransformationList : ExpressionSourceList<IExpressionTransformation>
{
    internal ExpressionTransformationList() { }
    
    private ExpressionTransformationList(IEnumerable<IExpressionTransformation> matches)
    {
        foreach(var m in matches)
            Add(m);
    }

    public ExpressionTransformationList Matches(ExpressionSyntax e)
    {
        return new ExpressionTransformationList(InnerList.Where(_=>_.Matches(e)));
    }
}
}
