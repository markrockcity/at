using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;
using System.Collections.Immutable;

namespace At.Contexts
{
public class OperatorImplementation : Definition
{
    public OperatorImplementation(OperatorSymbol symbol, TypeSymbol operandType1, TypeSymbol operandType2, TypeSymbol returnType, Context parentCtx,DiagnosticsBag diagnostics,AtSyntaxNode syntaxNode = null) : base(parentCtx,diagnostics,syntaxNode)
    {
        Operator = symbol;
        OperandType1 = operandType1;
        OperandType2 = operandType2;
        ReturnType = returnType;
    }

    protected internal override ContextSymbol ContextSymbol {get;}

    public OperatorSymbol Operator {get;}
    public virtual TypeSymbol ReturnType {get;}
    public TypeSymbol OperandType1 {get;}
    public TypeSymbol OperandType2 {get;}

    public override bool HasContents => false;

    public override IEnumerable<IBindingNode> Contents() => new IBindingNode[0];

}
}
