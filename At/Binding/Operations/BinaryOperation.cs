using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public class BinaryOperation : Operation
{
    public BinaryOperation(Context ctx, BinaryExpressionSyntax syntaxNode, OperatorSymbol op, Operation left, Operation right, Operation prev) : base(ctx,syntaxNode,prev)
    {
        Operator = op    ?? throw new ArgumentNullException(nameof(op));
        Left     = left  ?? throw new ArgumentNullException(nameof(left)); 
        Right    = right ?? throw new ArgumentNullException(nameof(right));        
    }

    public Operation Left {get;}
    public Operation Right {get;}
    public OperatorSymbol Operator {get;} 
    public BinaryExpressionSyntax Syntax => (BinaryExpressionSyntax) ExpressionSyntax;

    public override TypeSymbol Type 
    {
        get
        {
            var impl = Operator?.GetImplementation(Left.Type,Right.Type);
            return impl?.ReturnType ?? TypeSymbol.Unknown;
        }
    } 

    public override TResult Accept<TResult>(BindingTreeVisitor<TResult> visitor)
    {
        return visitor.VisitBinary(this);
    }

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol)
    {

        var left  = Left.ReplaceSymbol(undefinedSymbol,newSymbol);
        var right = Right.ReplaceSymbol(undefinedSymbol, newSymbol);
        var op    = Operator.IsUndefined && Operator.Name==undefinedSymbol.Name ? newSymbol : Operator;
        return new BinaryOperation(Context, Syntax, Operator, left, right, Previous);
    }

    public override string ToString()
    {
        return $"Binary[{Operator.Name}]({Left},{Right})";
    }
}
}
