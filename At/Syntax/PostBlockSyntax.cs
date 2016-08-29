using System.Collections.Generic;
using At.Syntax;

namespace At
{
internal class PostBlockSyntax : ExpressionSyntax
{
    public PostBlockSyntax(ExpressionSyntax operand,BlockSyntax block,IExpressionSource expSrc,IEnumerable<AtDiagnostic> diagnostics)
    : base(new AtSyntaxNode[] {operand,block}, expSrc, diagnostics)
    {
        this.Operand = operand;
        this.Block = block;
    }

    public BlockSyntax Block {get;}
    public ExpressionSyntax Operand {get;}
}
}