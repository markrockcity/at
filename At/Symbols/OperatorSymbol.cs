using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Contexts;
using At.Symbols;

namespace At
{
public sealed class OperatorSymbol : ContextSymbol
{
    readonly List<OperatorImplementation> _implementations = new List<OperatorImplementation>();
    private Context ctx;

    public OperatorSymbol(Context ctx, string @operator, AtSyntaxNode syntax, ContextSymbol parentContext) : base(@operator,syntax,parentContext)
    {
        this.ctx = ctx;
    }

    public override bool IsUndefined => _isUndefined; bool _isUndefined;

    public override Symbol ParentSymbol => null;

    public override TypeSymbol Type {get; protected internal set;}

    public override TResult Accept<TResult>(BindingTreeVisitor<TResult> visitor)
    {
        return visitor.VisitOperator(this);
    }

    public OperatorImplementation GetImplementation(TypeSymbol operandType1, TypeSymbol operandType2 = null)
    {
        return _implementations.SingleOrDefault(_=>_.OperandType1==operandType1 && _.OperandType2==operandType2);
    }

    public override string ToString()
    {
        return $"Operator({Name})";
    }

    internal OperatorImplementation EnsureImplementation(TypeSymbol operandType1, TypeSymbol returnType)
    {
        var impl = GetImplementation(operandType1) 
                        ?? new OperatorImplementation(this,operandType1,null,returnType,ctx,ctx.Diagnostics);

        if (returnType != TypeSymbol.Unknown && impl.ReturnType != returnType)
        {
            throw new InvalidOperationException( $"Implementation of {this} for operand type "
                                                +$"'{operandType1}' already exists with "
                                                +$"return type {impl.ReturnType}."
                                                +$" {returnType} was expected.");
        }

        if (!_implementations.Contains(impl))
            _implementations.Add(impl);

        return impl;
    }

    internal OperatorImplementation EnsureImplementation(TypeSymbol operandType1, TypeSymbol operandType2, TypeSymbol returnType)
    {
        if (operandType2 == null)
            return EnsureImplementation(operandType1,returnType);

        var impl = GetImplementation(operandType1,operandType2) 
                        ?? new OperatorImplementation(this,operandType1,operandType2,returnType,ctx,ctx.Diagnostics);
        
        if (returnType != TypeSymbol.Unknown && impl.ReturnType != returnType)
        {
            throw new InvalidOperationException( $"Implementation of {this} for operand types "
                                                +$"'{operandType1}' and '{operandType2}' already exists with "
                                                +$"return type {impl.ReturnType}."
                                                +$" {returnType} was expected.");
        }

        if (!_implementations.Contains(impl))
            _implementations.Add(impl);

        return impl;
    }    

    internal OperatorImplementation EnsureImplementation(IReadOnlyList<TypeSymbol> paramTypes, TypeSymbol returnType)
    {
        switch(paramTypes.Count)
        {
            case 1: return EnsureImplementation(paramTypes[0],returnType);
            case 2: return EnsureImplementation(paramTypes[0],paramTypes[1],returnType);
            default: throw new NotSupportedException($"Operators with {paramTypes.Count} parameters");
        }
    }

    internal static OperatorSymbol Undefined(Context ctx, AtToken @operator, AtSyntaxNode syntax, ContextSymbol parentCtx)
    {
        var os = new OperatorSymbol(ctx,@operator.Text,syntax,parentCtx){_isUndefined=true};
        return os;
    }
}
}