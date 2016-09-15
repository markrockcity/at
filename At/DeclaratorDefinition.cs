using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using At.Syntax;
using static At.SyntaxFactory;

namespace At
{

public interface IDeclarationRule : IExpressionSource
{
    bool Matches(AtSyntaxNode[] node);
}

public class DeclarationDefinition : IDeclarationRule, ICollection
{
    //pattern -> (nodes => expr)
    readonly Dictionary<string,Func<AtSyntaxNode[],DeclarationSyntax>> dict;

    public DeclarationDefinition()
    {
        dict = new Dictionary<string, Func<AtSyntaxNode[], DeclarationSyntax>>();
    }

    public void Add(string pattern,Func<AtSyntaxNode[],DeclarationSyntax> createExpr)=>dict.Add(pattern,createExpr);

    int    ICollection.Count => dict.Count;
    object ICollection.SyncRoot => ((ICollection)this.dict).SyncRoot;
    bool   ICollection.IsSynchronized => ((ICollection)this.dict).IsSynchronized;
    
    public ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes)
    {
        var pattern = AtSyntaxNode.GetPatternStrings(nodes).First(dict.ContainsKey);
        var e = dict[pattern](nodes);
        return e;
    }

    public bool Matches(AtSyntaxNode[] nodes) => AtSyntaxNode.GetPatternStrings(nodes).Intersect(dict.Keys).Any();

    void ICollection.CopyTo(Array array,int index) => ((ICollection)this.dict).CopyTo(array,index);
    IEnumerator IEnumerable.GetEnumerator() => ((ICollection)this.dict).GetEnumerator();
}

public class DeclarationRule : IDeclarationRule
{
    readonly TokenKind declaratorKind;
    readonly Func<AtSyntaxNode[],ExpressionSyntax> create;
    readonly Func<TokenKind,AtSyntaxNode[],bool> matches;

    public DeclarationRule(TokenKind declaratorKind,Func<TokenKind,AtSyntaxNode[],bool> matches, Func<AtSyntaxNode[],ExpressionSyntax> create)
    {
        this.declaratorKind = declaratorKind;
        this.matches = matches;
        this.create = create;
    }

    public ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes)=>create(nodes);
    public bool Matches(params AtSyntaxNode[] nodes)=>matches(declaratorKind,nodes);
}

public class DeclaratorDefinition : OperatorDefinition
{
    public readonly DeclarationDefinition VariableDeclaration;
    public readonly DeclarationRule       MethodDeclaration;
    public readonly DeclarationDefinition TypeDeclaration;

    public DeclaratorDefinition(TokenKind declaratorKind, OperatorPosition opPosition) : base(declaratorKind,opPosition,(a,b)=>declaration((DeclaratorDefinition)a,b))
    {
        DeclarationRules = new DeclarationRuleList(this);

        VariableDeclaration = new DeclarationDefinition
        {
            //@X
            {$"Token({declaratorKind.Name}),TokenCluster",nodes => VariableDeclaration(nodes[0].AsToken(),((TokenClusterSyntax)nodes[1]).TokenCluster,null,null,nodes,this)},
        

            //@X : T
            {
                $"Token({declaratorKind.Name}),Binary[Colon](TokenCluster,Expr)",nodes =>
                { 
                    var declOp     = nodes[0].AsToken();
                    var colonPair  = (BinaryExpressionSyntax) nodes[1];
                    var identifier = ((TokenClusterSyntax)colonPair.Left).TokenCluster;
                    var type       = NameSyntax(colonPair.Right);

                    return VariableDeclaration(declOp,identifier,type,null,nodes,this);
                }
            },
        };


        MethodDeclaration = new DeclarationRule
        (
            declaratorKind,
        
            matches: (tk,nodes) =>  
            {
                if(nodes.Length != 2 || nodes[0].AsToken()?.Kind!=declaratorKind) 
                    return false;

                var postBlock = nodes[1] as PostBlockSyntax; 
                return (postBlock?.Block is RoundBlockSyntax && postBlock.Operand is TokenClusterSyntax);
            },

            create:  nodes  => 
            {
                var postBlock = nodes[1] as PostBlockSyntax; 

                //TODO: method parameters
                if (postBlock.Block.Contents.Count > 0)
                    throw new NotImplementedException("method paramters");

                //TODO: return type and body

                return MethodDeclaration(nodes[0].AsToken(),((TokenClusterSyntax)postBlock.Operand).TokenCluster,null,null,nodes,this);
            }
        );

        TypeDeclaration = new DeclarationDefinition
        {
            //@X<...>
            {
                $"Token({declaratorKind.Name}),PostBlock(TokenCluster,Pointy)",nodes =>
                {
                    var declOp  = nodes[0].AsToken();
                    var pb = (PostBlockSyntax) nodes[1];
                    var identifier = ((TokenClusterSyntax)pb.Operand).TokenCluster;
                    var typeArgs = new List<ParameterSyntax>();

                    return TypeDeclaration
                    (
                        declOp,
                        identifier,
                        List(pb.Block.StartDelimiter,SeparatedList<ParameterSyntax>(typeArgs),pb.Block.EndDelimiter),
                        null,null,nodes,this
                    );
                }            
            },

            //X<...> : ...
            {
                $"Token({declaratorKind.Name}),Binary[Colon](PostBlock(TokenCluster,Pointy),Expr)",nodes =>
                {
                    var declOp     = nodes[0].AsToken();
                    var colonPair  = (BinaryExpressionSyntax) nodes[1];
                    var pb = (PostBlockSyntax) colonPair.Left;
                    var identifier = ((TokenClusterSyntax)pb.Operand).TokenCluster;
                    var typeArgs = TypeParameterList(pb.Block);
                    var baseTypes = TypeList(colonPair.Right);

                    return TypeDeclaration
                    (
                        declOp,
                        identifier,
                        typeArgs,
                        baseTypes,
                        null,nodes,this
                    );
                }
            },
        
        };
    }

    public class DeclarationRuleList : ExpressionSourceList<IDeclarationRule>
    {
        readonly DeclaratorDefinition def;    
        internal DeclarationRuleList(DeclaratorDefinition def)
        {   
            this.def = def;
        }

        public DeclarationRule Add(Func<TokenKind,AtSyntaxNode[],bool> matches, Func<AtSyntaxNode[],ExpressionSyntax> create)
        {
            var def = new DeclarationRule(this.def.TokenKind,matches,create);
            InnerList.Add(def);
            return def;
        }
    
        public IList<IDeclarationRule> Matches(AtSyntaxNode[] nodes)
        {
            return InnerList.Where(_=>_.Matches(nodes)).ToList();
        }
    }

    public DeclarationRuleList DeclarationRules {get;}

    //e.g., AddRule(def => def.VariableDeclaration)
    public DeclaratorDefinition AddRule(Func<DeclaratorDefinition,IDeclarationRule> f)
    {
        DeclarationRules.Add(f(this));
        return this;            
    }
    public DeclaratorDefinition AddRules(params Func<DeclaratorDefinition,IDeclarationRule>[] fs)
    {
        foreach(var f in fs)
            DeclarationRules.Add(f(this));
        return this;            
    }
    
    static DeclarationSyntax declaration(DeclaratorDefinition def, AtSyntaxNode[] nodes)
    {
        switch(def.OperatorPosition)
        {
            case OperatorPosition.Start:
            case OperatorPosition.Prefix:
            {
                Debug.Assert(nodes.Length==2);

                if (nodes[0].AsToken()?.Kind != def.TokenKind)
                    throw new AtException(string.Format("Expected {0} {1}", def.TokenKind, nodes[1]));
            
            } break; 

            default:
                throw new NotImplementedException(def.OperatorPosition.ToString());            
        }

        var e = def.DeclarationRules.Matches(nodes).FirstOrDefault()?.CreateExpression(nodes);
        if (e == null)
            throw new NotImplementedException(AtSyntaxNode.GetPatternStrings(nodes).First());
        else 
            return (DeclarationSyntax) e;
    }
}
}
