using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Syntax;

namespace At.Binding
{
public class BinaryExpression : Expression
{
    public BinaryExpression(Context ctx, BinaryExpressionSyntax syntaxNode, ISymbol op, Expression left, Expression right) : base(ctx,syntaxNode)
    {
        Operation = op    ?? throw new ArgumentNullException(nameof(op));
        Left      = left  ?? throw new ArgumentNullException(nameof(left)); 
        Right     = right ?? throw new ArgumentNullException(nameof(right));        
    }

    public Expression Left {get;}
    public Expression Right {get;}
    public ISymbol Operation {get;}
    public BinaryExpressionSyntax Syntax => (BinaryExpressionSyntax) ExpressionSyntax;
    
    public override void Accept(BindingTreeVisitor visitor)
    {
        visitor.VisitBinary(this);
    }

    public override Expression ReplaceSymbol(UndefinedSymbol undefinedSymbol, ISymbol newSymbol)
    {

        var left  = Left.ReplaceSymbol(undefinedSymbol,newSymbol);
        var right = Right.ReplaceSymbol(undefinedSymbol, newSymbol);
        return new BinaryExpression(Context, Syntax, Operation, left, right);
    }

    public override string ToString()
    {
        return $"Binary[{Operation.Name}]({Left},{Right})";
    }
}
}
