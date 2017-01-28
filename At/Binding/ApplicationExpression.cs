using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using At.Syntax;

namespace At.Binding
{
public class ApplicationExpression : Expression
{
    private ApplicationSyntax node;

    public ApplicationExpression(Context context, ApplicationSyntax syntaxNode, ISymbol subject, IEnumerable<IBindingNode> args) : base(context,syntaxNode)
    {
        node = syntaxNode;
        Subject = subject;
        Arguments = args.ToImmutableArray();

        if (subject is UndefinedSymbol u)
            undefined(u);
        undefined(args.OfType<UndefinedSymbol>());                            
    }

    public ImmutableArray<IBindingNode> Arguments
    {
        get;
        private set;
    }

    public ISymbol Subject
    {
        get;
        private set;
    }

    public override void Accept(BindingTreeVisitor visitor)
    {
        visitor.VisitApply(this);
    }

    public override Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol, ISymbol newSymbol)
    {
        var newSubj = Subject==undefinedSymbol ? newSymbol : Subject;
        var args    = Arguments.Select(_=>_==undefinedSymbol ? newSymbol : _);
        return new ApplicationExpression(Context,node,newSubj,args);
    }


    public override string ToString()
    {
        return $"{{Apply({Subject}, {(string.Join(",",Arguments))})}}";
    }
}
}
