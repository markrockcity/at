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
    readonly Dictionary<SyntaxPattern,Func<SyntaxNodeMatchCollection,DeclarationSyntax>> dict;

    public DeclarationDefinition()
    {
        dict = new Dictionary<SyntaxPattern, Func<SyntaxNodeMatchCollection, DeclarationSyntax>>();
    }

    public void Add(string patternString,Func<SyntaxNodeMatchCollection,DeclarationSyntax> createExpr)
    {
        var pattern = ParseSyntaxPattern(patternString);
        dict.Add(pattern,createExpr);
    }

    int    ICollection.Count => dict.Count;
    object ICollection.SyncRoot => ((ICollection)this.dict).SyncRoot;
    bool   ICollection.IsSynchronized => ((ICollection)this.dict).IsSynchronized;
    
    public ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes)
    {
        var d = new Dictionary<string,AtSyntaxNode>();
        var pattern = dict.Keys.First(k=>AtSyntaxNode.MatchesPattern(k,nodes,d));
        var s = new SyntaxNodeMatchCollection(nodes,d);
        var e = dict[pattern](s);
        return e;
    }

    public bool Matches(AtSyntaxNode[] nodes)
    {
       return dict.Keys.Any(k=>AtSyntaxNode.MatchesPattern(k,nodes));
    }

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
    public readonly DeclarationDefinition NamespaceDeclaration;


    public DeclaratorDefinition(TokenKind declaratorKind, OperatorPosition opPosition) : base(declaratorKind,opPosition,(a,b)=>declaration((DeclaratorDefinition)a,b))
    {
        DeclarationRules = new DeclarationRuleList(this);

        VariableDeclaration = new DeclarationDefinition
        {
            //@X
            {$"Token({declaratorKind.Name}),TokenCluster",nodes => VariableDeclaration(nodes[0].AsToken(),((TokenClusterSyntax)nodes[1]).TokenCluster,null,null,nodes,this)},
        

            //@X : T
            {
                $"Token({declaratorKind.Name}),Binary[Colon](TokenCluster,TokenCluster)",nodes =>
                { 
                    var declOp     = nodes[0].AsToken();
                    var colonPair  = (BinaryExpressionSyntax) nodes[1];
                    var identifier = ((TokenClusterSyntax)colonPair.Left).TokenCluster;
                    var type       = NameSyntax(colonPair.Right);

                    return VariableDeclaration(declOp,identifier,type,null,nodes,this);
                }
            },

            //@X : T< ... >
            {
                $"Token({declaratorKind.Name}),Binary[Colon](TokenCluster,PostBlock(TokenCluster,Pointy))",nodes =>
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
                if (postBlock.Block.Content.Count > 0)
                    throw new NotImplementedException("method paramters");

                //TODO: return type and body

                return MethodDeclaration(nodes[0].AsToken(),((TokenClusterSyntax)postBlock.Operand).TokenCluster,null,null,nodes,this);
            }
        );

        TypeDeclaration = new DeclarationDefinition
        {
            //@X<...>
            {
                $"Token({declaratorKind.Name}),PostBlock(TokenCluster,p:Pointy)",nodes =>
                {
                    var declOp  = nodes[0].AsToken();
                    var pb = (PostBlockSyntax) nodes[1];
                    var identifier = ((TokenClusterSyntax)pb.Operand).TokenCluster;
                    var typeArgs = TypeParameterList(nodes.GetNode<BlockSyntax>("p"));
                    var decl = TypeDeclaration
                    (
                        declOp,
                        identifier,
                        typeArgs,
                        null,null,nodes,this
                    );
                    return decl;
                }            
            },

            //@X<...>{...}
            {
                $"a:Token({declaratorKind.Name}),PostBlock(pb:PostBlock(i:TokenCluster,p:Pointy),c:Curly)", _ =>
                {
                    var declOp = _.GetNode<AtToken>("a");
                    var pb = _.GetNode<PostBlockSyntax>("pb");
                    var identifier = ((TokenClusterSyntax)pb.Operand).TokenCluster;
                    var typeArgs = TypeParameterList(pb.Block);
                    var c = _.GetNode<CurlyBlockSyntax>("c");

                    return TypeDeclaration
                    (
                        declOp,
                        identifier,
                        typeArgs,
                        null,
                        c.Content.OfType<DeclarationSyntax>(),_,this
                    );

                    //throw new NotImplementedException("!"+AtSyntaxNode.PatternStrings(nodes).First());
                }
            },

            //@X<...> : Y<> {...} (ParseText2)
            {
                //$"a:Token({declaratorKind.Name}),PostBlock(Binary[Colon](pb1:PostBlock(i1:TokenCluster,p1:Pointy),i2:TokenCluster),c:Curly)", _ =>
                $"a:Token({declaratorKind.Name}),Binary[Colon](pb1:PostBlock(i1:TokenCluster,p1:Pointy),PostBlock(pb2:PostBlock(i2:TokenCluster,p2:Pointy),c:Curly))", _ =>
                {
                    var declOp = _.GetNode<AtToken>("a");
                    var pb1 = _.GetNode<PostBlockSyntax>("pb1");
                    var identifier = ((TokenClusterSyntax)pb1.Operand).TokenCluster;
                    var typeArgs = TypeParameterList(pb1.Block);
                    var c = _.GetNode<CurlyBlockSyntax>("c");
                    var baseTypes = TypeList(_.GetNode<PostBlockSyntax>("pb2"));

                    return TypeDeclaration
                    (
                        declOp,
                        identifier,
                        typeArgs,
                        baseTypes,
                        c.Content.OfType<DeclarationSyntax>(),_,this
                    );

                    //throw new NotImplementedException("!"+AtSyntaxNode.PatternStrings(nodes).First());
                }
            },
 /*
            
            //@X<...> : Y<...> {...} 
            {
                $"a:Token({declaratorKind.Name}),Binary[Colon](pb1:PostBlock(i1:TokenCluster,p1:Pointy),PostBlock(pb2:PostBlock(i2:TokenCluster,p2:Pointy),c:Curly))", _ =>
                {
                    var declOp = _.GetNode<AtToken>("a");
                    var pb1 = _.GetNode<PostBlockSyntax>("pb1");
                    var identifier = ((TokenClusterSyntax)pb1.Operand).TokenCluster;
                    var typeArgs = TypeParameterList(pb1.Block);
                    var c = _.GetNode<CurlyBlockSyntax>("c");
                    var baseTypes = TypeList(_.GetNode<PostBlockSyntax>("pb2"));

                    return TypeDeclaration
                    (
                        declOp,
                        identifier,
                        typeArgs,
                        baseTypes,
                        c.Content.OfType<DeclarationSyntax>(),_,this
                    );

                    //throw new NotImplementedException("!"+AtSyntaxNode.PatternStrings(nodes).First());
                }
            },
            */

            //@X<...> : Y {...} 
            {
                $"a:Token({declaratorKind.Name}),Binary[Colon](pb1:PostBlock(i1:TokenCluster,p1:Pointy),pb2:PostBlock(i2:TokenCluster,c:Curly))", _ =>
                {
                    var declOp = _.GetNode<AtToken>("a");
                    var pb1 = _.GetNode<PostBlockSyntax>("pb1");
                    var identifier = ((TokenClusterSyntax)pb1.Operand).TokenCluster;
                    var typeArgs = TypeParameterList(pb1.Block);
                    var c = _.GetNode<CurlyBlockSyntax>("c");
                    var baseTypes = TypeList(_.GetNode<TokenClusterSyntax>("i2"));

                    return TypeDeclaration
                    (
                        declOp,
                        identifier,
                        typeArgs,
                        baseTypes,
                        c.Content.OfType<DeclarationSyntax>(),_,this
                    );

                    //throw new NotImplementedException("!"+AtSyntaxNode.PatternStrings(nodes).First());
                }
            },

            //@X<...> : Y<...>
            {
                $"a:Token({declaratorKind.Name}),Binary[Colon](pb1:PostBlock(i1:TokenCluster,p1:Pointy),pb2:PostBlock(i2:TokenCluster,p2:Pointy))", _ =>
                {
                    var declOp = _.GetNode<AtToken>("a");
                    var pb1 = _.GetNode<PostBlockSyntax>("pb1");
                    var identifier = ((TokenClusterSyntax)pb1.Operand).TokenCluster;
                    var typeArgs = TypeParameterList(pb1.Block);
                    var baseTypes = TypeList(_.GetNode<PostBlockSyntax>("pb2"));

                    return TypeDeclaration
                    (
                        declOp,
                        identifier,
                        typeArgs,
                        baseTypes,
                        null,
                        _,
                        this
                    );

                    //throw new NotImplementedException("!"+AtSyntaxNode.PatternStrings(nodes).First());
                }
            },

   

            //@X<...> : Y
            {
                $"Token({declaratorKind.Name}),Binary[Colon](PostBlock(TokenCluster,Pointy),TokenCluster)",nodes =>
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

        NamespaceDeclaration = new DeclarationDefinition
        {
            //@X : namespace 
            {
                $"Token({declaratorKind.Name}),Binary[Colon](TokenCluster,TokenCluster(namespace))",nodes =>
                { 
                    var declOp     = nodes[0].AsToken();
                    var colonPair  = (BinaryExpressionSyntax) nodes[1];
                    var identifier = ((TokenClusterSyntax)colonPair.Left).TokenCluster;
                    return NamespaceDeclaration(declOp,identifier,null,nodes,this);
                }
            },

            //@X : namespace { ... }
            {
                $"Token({declaratorKind.Name}),b:Binary[Colon](TokenCluster,PostBlock(TokenCluster(namespace),c:Curly))",nodes =>
                { 
                    var declOp     = nodes[0].AsToken();
                    var colonPair  = nodes.GetNode<BinaryExpressionSyntax>("b");
                    var identifier = ((TokenClusterSyntax)colonPair.Left).TokenCluster;
                    var c          = nodes.GetNode<CurlyBlockSyntax>("c");
                    var members    = c.Content.OfType<DeclarationSyntax>();

                    return NamespaceDeclaration(declOp,identifier,members,nodes,this);
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

        var rule = def.DeclarationRules.Matches(nodes).FirstOrDefault();
        var e   = rule?.CreateExpression(nodes);

        if (e == null)
            throw new NotImplementedException(string.Join(",",(object[])nodes)+"\r\n\r\n"+AtSyntaxNode.PatternStrings(nodes).First());
        else 
            return (DeclarationSyntax) e;
    }
}
}
