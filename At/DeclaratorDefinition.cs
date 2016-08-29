using System;
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
    public readonly DeclarationRule VariableDeclaration;
    public readonly DeclarationRule MethodDeclaration;

    public DeclaratorDefinition(TokenKind declaratorKind, OperatorPosition opPosition) : base(declaratorKind,opPosition,(a,b)=>declaration((DeclaratorDefinition)a,b))
    {
        DeclarationRules = new DeclarationRuleList(this);

        VariableDeclaration = new DeclarationRule
        (
            declaratorKind,
        
            matches: (tk,nodes) =>     nodes.Length == 2 
                                    && nodes[0].AsToken()?.Kind==declaratorKind
                                    && nodes[1] is TokenClusterSyntax,

            create:  nodes  => VariableDeclaration(nodes[0].AsToken(),((TokenClusterSyntax)nodes[1]).TokenCluster,null,null,nodes,this)
        );

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

                return MethodDeclaration(nodes[0].AsToken(),((TokenClusterSyntax)postBlock.Operand).TokenCluster,null,null,nodes,this);
            }
        );

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
    public DeclaratorDefinition AddRule(Func<DeclaratorDefinition,DeclarationRule> f)
    {
        DeclarationRules.Add(f(this));
        return this;            
    }
    public DeclaratorDefinition AddRules(params Func<DeclaratorDefinition,DeclarationRule>[] fs)
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
            throw new NotImplementedException(string.Join(",",nodes.Select(_=>$"{_} : {_.GetType()}")));
        else 
            return (DeclarationSyntax) e;
    }
}
}
