using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
/// <summary>Binding context for a method/function call site.</summary>
public class Invocation : Operation
{
    public Invocation(IBindingNode target, IEnumerable<TypeArgument> typeArguments, IEnumerable<Argument> arguments,  Context ctx, ExpressionSyntax syntaxNode,Operation previousOperation) : base(ctx,syntaxNode,previousOperation)
    {
        Target = target;
        Arguments = arguments?.ToImmutableArray() ?? ImmutableArray<Argument>.Empty;
        TypeArguments = typeArguments?.ToImmutableArray() ?? ImmutableArray<TypeArgument>.Empty;
    }

    /// <summary>e.g., MethodSymbol</summary>
    public IBindingNode Target {get; private set;}

    public ImmutableArray<Argument> Arguments
    {
        get;
        private set;
    }

    public ImmutableArray<TypeArgument> TypeArguments
    {
        get;
        private set;
    }

    public override TypeSymbol Type => throw new NotImplementedException();

    public override T Accept<T>(BindingTreeVisitor<T> visitor) =>     
        visitor.VisitInvocation(this);
    

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol,ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => $"Invoke({Target}({string.Join(",",Arguments)}))";
}
}
