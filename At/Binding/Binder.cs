using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Contexts;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
//ModuleBuilder()?
/// <summary>
/// A Binder converts names in to symbols and syntax nodes into binding trees. It is context
/// dependent, relative to a location in source code.
/// </summary>
class Binder : AtSyntaxVisitor<IBindingNode>
{
    private Context ctx;
    private Operation lastOperation;

    public Binder(Context ctx)
    {
        this.ctx = ctx;
    }

    public static MethodDefinition ApplyTypeArguments(MethodDefinition method, IEnumerable<TypeArgument> typeArgs)
    {
        //parameters
        var parameters = new List<ParameterSymbol>(method.Parameters.Length);        
        foreach(var p in method.Parameters)
        {
            if (p.ParameterType is TypeParameterSymbol tp)
                parameters.Add(new ParameterSymbol(p.Name,typeArgs.Single(_=>_.Parameter==tp).Argument));
            else
                parameters.Add(p);
        }
        
        //definition
        var d = new MethodDefinition(method.Symbol,parameters,method,method.Diagnostics);
        var b = new TypeArgumentBinder(d,typeArgs);
        foreach(var c in method.Contents())
        {
            var x = b.Visit(c);
            d.AddNode(x);
        }

        //return type
        if (d.ReturnType is TypeParameterSymbol tp2)
            d.ReturnType = typeArgs.Single(_=>_.Parameter==tp2).Argument;

        d.End();
        return d;
    }

    protected internal override IBindingNode VisitCompilationUnit(CompilationUnitSyntax compilationUnitSyntax)
    {
        var cuCtx    = new CompilationUnit((CompilationContext) ctx, compilationUnitSyntax, ctx.Diagnostics);
        var cuBinder = new Binder(cuCtx);

        foreach(var expression in compilationUnitSyntax.Expressions)
        {
            var node = expression.Accept(cuBinder);

            if (!(node is Context)) //contexts should add themselves to parent context
                cuCtx.AddNode(node);
        }

        return cuCtx;
    }

    protected internal override IBindingNode VisitArgument(AtSyntaxNode argumentSyntax)
    {
        var callSite = (CallSite) ctx;
        var _arg = Visit(argumentSyntax is ArgumentSyntax a ? a.Expression : argumentSyntax);

        var op =   _arg is Operation o ? o 
                 : _arg is CallSite cs ? cs.Invocation
                 : throw new NotSupportedException($"{_arg} ({_arg.GetType()})");


        return new Argument(op,null,argumentSyntax);
    }

    protected internal override IBindingNode VisitApply(ApplicationSyntax applicationSyntax)
    {
        var subject = (SymbolReference) applicationSyntax.Subject.Accept(this);
        //var args    = applicationSyntax.Arguments.Select(_=>_.Accept(this)).ToList();

        //Operation op;

        switch(subject.Symbol)
        {
            case KeywordSymbol keyword:
                return invocation(applicationSyntax,applicationSyntax.Subject,applicationSyntax.Arguments);

            default:
                throw new NotImplementedException($"{GetType()}.{nameof(VisitApply)}({subject.Symbol})");        
        }

        //var l =           new Invocation(subject,args,ctx,applicationSyntax,lastOperation);
        //lastOperation = op;
        //return op;

    }

    protected internal override IBindingNode VisitContext(ContextSyntax syntaxNode)
    {
        return syntaxNode?.Accept(this) ?? ctx;
    }

    protected internal override IBindingNode DefaultVisit(AtSyntaxNode node)
    {
        throw new NotImplementedException($"Visit{node.GetType().Name.Replace("Syntax","")}()");
    }

    protected internal override IBindingNode VisitBinary(BinaryExpressionSyntax binaryExpressionSyntax)
    {
        var left  = (Operation) Visit(binaryExpressionSyntax.Left);
        var right = (Operation) Visit(binaryExpressionSyntax.Right);
        var op    = ctx.LookupSymbol(binaryExpressionSyntax.Operator.Text) as OperatorSymbol ?? OperatorSymbol.Undefined(ctx, binaryExpressionSyntax.Operator,binaryExpressionSyntax,null);
        var l = new BinaryOperation(ctx, binaryExpressionSyntax,op,left,right,lastOperation);
        lastOperation = l;
        return l;
    }

    protected internal override IBindingNode VisitTokenCluster(TokenClusterSyntax tokenClusterSyntax)
    {
        var symbolRef = new SymbolReference(
                                ctx.LookupSymbol(tokenClusterSyntax.TokenCluster.Text) ?? new UndefinedSymbol(ctx, tokenClusterSyntax.TokenCluster),
                                ctx,
                                tokenClusterSyntax,
                                lastOperation);

        if (symbolRef.Symbol is UndefinedSymbol undef)
            ctx.registerUndefinedSymbol(undef,symbolRef);

        lastOperation = symbolRef;
        return symbolRef;
    }


    protected internal override IBindingNode VisitLiteral(LiteralExpressionSyntax literalExpressionSyntax)
    {
        var l =  new Literal(ctx, TypeSymbol.For(literalExpressionSyntax.Literal.Value.GetType()), literalExpressionSyntax, literalExpressionSyntax.Literal.Value, lastOperation);
        lastOperation = l;
        return l;
    }

    protected internal override IBindingNode VisitTypeDeclaration(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        var symbol = new TypeSymbol(typeDeclarationSyntax.Identifier.Text,null,null,typeDeclarationSyntax);

        Context _ctx = ctx;
        TypeDefinition def = null;

        if (typeDeclarationSyntax.Members.Any())
        {
            _ctx = new DeclarationContext(ctx, syntaxNode: typeDeclarationSyntax);
            def  = new TypeDefinition(symbol, _ctx, null, typeDeclarationSyntax);

            var defb = new Binder(def);

            foreach(var m in typeDeclarationSyntax.Members)
            {
                var node = defb.Visit(m);

                if (!(node is Context))
                    def.AddNode(node);
            }
        }

        var decl = new TypeDeclaration(symbol,_ctx,null,null,def,typeDeclarationSyntax,lastOperation);
        
        if ((_ctx is DeclarationContext))
            _ctx.AddNode(decl);
        
        lastOperation = decl;
        return (_ctx is DeclarationContext) ? (IBindingNode) _ctx : decl;
    }

    protected internal override IBindingNode VisitDirective(DirectiveSyntax directiveSyntax)
    {
        var l =  new Directive(ctx,directiveSyntax,lastOperation);
        lastOperation = l;
        return l;
    }

    protected internal override IBindingNode VisitVariableDeclaration(VariableDeclarationSyntax variableDeclarationSyntax)
    {
        var type = variableDeclarationSyntax.Type != null ? ctx.LookupSymbol(variableDeclarationSyntax.Type.Text) as TypeSymbol : null;
        //TO-DO: make sure name isn't already defined
        var l = new VariableDeclaration(ctx,null,type,variableDeclarationSyntax,lastOperation); 
        lastOperation = l;
        return l;

    }

    protected internal override IBindingNode VisitMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        var symbol = new MethodSymbol(methodDeclarationSyntax.Identifier.Text,methodDeclarationSyntax,null);
        var parameters = methodDeclarationSyntax.Parameters.List.Select(syntax=>new ParameterSymbol(syntax.Identifier.Text,null,syntax));  
        
        Context _ctx = ctx;
        MethodDefinition def = null;

        if (methodDeclarationSyntax.Body != null)
        {
            _ctx = new DeclarationContext(ctx,syntaxNode:methodDeclarationSyntax);
            def  = new MethodDefinition(symbol,parameters,_ctx,null,methodDeclarationSyntax);
            
            var binder = new Binder(def);

            if (methodDeclarationSyntax.Body is BlockSyntax block)
            {
                foreach (var e in block.Content)
                {
                    var n = binder.Visit(e);

                    if (!(n is Context)  || n is DeclarationContext)
                        def.AddNode(n);
                }
            }
            else if (methodDeclarationSyntax.Body != null)
            {
                var n = binder.Visit(methodDeclarationSyntax.Body);
                if (!(n is Context)  || n is DeclarationContext)
                    def.AddNode(n);
            }    

            def.End();
        }
        
        var decl = new MethodDeclaration(_ctx,methodDeclarationSyntax,symbol,null,parameters,/*returnType*/null,def,lastOperation);
    
        if (_ctx is DeclarationContext)
            _ctx.AddNode(decl);
            
        lastOperation = decl;
        return (_ctx is DeclarationContext) ? (IBindingNode) _ctx : decl;
    }

    protected internal override IBindingNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax namespaceDeclarationSyntax)
    {
        var symbol = new NamespaceSymbol(namespaceDeclarationSyntax.Identifier.Text,namespaceDeclarationSyntax,null);

        Context _ctx = ctx;
        NamespaceDefinition def = null;

        if (namespaceDeclarationSyntax.Members.Any())
        {
            _ctx = new DeclarationContext(ctx, namespaceDeclarationSyntax);
            def  = new NamespaceDefinition(symbol,_ctx, null, namespaceDeclarationSyntax);

            var defb = new Binder(def);

            foreach(var m in namespaceDeclarationSyntax.Members)
            {
                var node = defb.Visit(m);

                if (!(node is Context))
                    def.AddNode(node);
            }
        }

         var decl = new NamespaceDeclaration(symbol,_ctx,def,namespaceDeclarationSyntax,lastOperation);
        
        if (_ctx is DeclarationContext)
            _ctx.AddNode(decl);
        
        lastOperation = decl;
        return (_ctx is DeclarationContext) ? (IBindingNode) _ctx : decl;        
   }

    protected internal override IBindingNode VisitRoundBlock(RoundBlockSyntax roundBlockSyntax)
    {
        return   roundBlockSyntax.Content.Count == 0 ? ctx.LookupSymbol("()")
               : roundBlockSyntax.Content.Count == 1 ? Visit(roundBlockSyntax.Content[0])
               : throw new NotImplementedException(roundBlockSyntax.FullText);
    }

    protected internal override IBindingNode VisitInvoke(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        return invocation(invocationExpressionSyntax,invocationExpressionSyntax.Expression,invocationExpressionSyntax.Arguments.List);
    }

    private IBindingNode invocation(ExpressionSyntax syntaxNode, ExpressionSyntax target, IEnumerable<AtSyntaxNode> args)
    {
        var callSite = new CallSite(ctx,null);
        var callSiteBinder   = new Binder(callSite);

        var e = callSiteBinder.Visit(target);
        var symbolRef = e as SymbolReference;
        callSite.AddNode(e);

        var _args = args.Select(callSiteBinder.VisitArgument).Cast<Argument>().ToList();
        var typeArgs = new List<TypeArgument>();

        //MethodSymbol : match arguments with parameters and check constraints
        if (symbolRef?.Symbol is MethodSymbol method)
        {
            var methodDef = method?.Definition;
            var paramTypes = new List<TypeSymbol>(_args.Count);

            //arguments
            for(int i = 0; i < _args.Count; ++i)
            {
                var _arg = _args[i];
                var param = method.Declaration?.Parameters[i];
                _arg.Parameter = param;
                
                if (   param.Type is TypeParameterSymbol typeParam 
                    && (methodDef?.TypeParameters.Contains(typeParam) ?? false) 
                    && !@typeArgs.Any(_=>_.Parameter==typeParam))
                {
                    var targ = new TypeArgument(_arg.Type ?? TypeSymbol.Unknown,typeParam);
                    typeArgs.Add(targ);
                    callSite.AddNode(targ);
                }

                callSite.AddNode(_arg);
            }

            //constraints
            foreach(var constraint in methodDef.Constraints)
            {
                if (!constraint.IsSatisfiedBy(callSite))
                    ctx.Diagnostics?.Add(AtDiagnostic.Create(DiagnosticIds.BindingConstraintViolated,callSite.Syntax,DiagnosticSeverity.Error,$"Binding constraint violated : {constraint}"));                   

                if (constraint is OperatorConstraint oc && oc.ParameterTypes.OfType<TypeParameterSymbol>().Any(ot=>callSite.TypeArguments.Any(ta=>ot==ta.Parameter)))
                {
                    var ot1 = oc.ParameterTypes[0] is TypeParameterSymbol tp1 
                                ? typeArgs.Single(_=>_.Parameter==tp1).Argument
                                : oc.ParameterTypes[0];

                    var ot2 = oc.ParameterTypes.Length > 1 
                                ? (oc.ParameterTypes[1] is TypeParameterSymbol tp2 
                                        ? typeArgs.Single(_=>_.Parameter==tp2).Argument
                                        : oc.ParameterTypes[1])
                                : null;

                    var rt = oc.ReturnType is TypeParameterSymbol tp3
                                ? typeArgs.Single(_=>_.Parameter==tp3).Argument ?? TypeSymbol.Unknown
                                : oc.ReturnType;
                    
                    oc.Symbol.EnsureImplementation(ot1,ot2,rt);
                }
            }
            
            method.EnsureImplementation(typeArgs);
        }
            
        var invocation = new Invocation(e,typeArgs,_args,callSite,syntaxNode,lastOperation);       
        lastOperation = invocation;

        callSite.AddNode(invocation);

        if (symbolRef?.Symbol is UndefinedSymbol undef)
            callSite.registerUndefinedSymbol(undef,invocation);

        return callSite;
    }
}
}
