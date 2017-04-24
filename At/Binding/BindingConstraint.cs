using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Contexts;
using At.Symbols;

namespace At.Binding
{

public interface IBindingConstraint 
{
    bool IsSatisfiedBy(IBindingNode reference);
}


/// <summary>Constraint requiring existence of an operator</summary>
public class OperatorConstraint : IBindingConstraint
{

    public OperatorConstraint(OperatorSymbol symbol, IEnumerable<TypeSymbol> parameterTypes, TypeSymbol returnType)
    {
        Symbol = symbol;
        ParameterTypes = parameterTypes.ToImmutableArray();
        ReturnType = returnType;
    }

    public OperatorSymbol Symbol
    {
        get;
    }

    public ImmutableArray<TypeSymbol> ParameterTypes
    {
        get;
    }

    public TypeSymbol ReturnType
    {
        get;
    }

    public bool IsSatisfiedBy(IBindingNode reference)
    {
        if (reference is CallSite cs)
        {
            foreach(var pt in ParameterTypes)
            foreach(var arg in cs.Arguments)
            {
                if (   pt is TypeParameterSymbol tp  
                    && arg is Argument a
                    && a.Parameter.ParameterType == tp) return true;
            }                

            if (ReturnType == TypeSymbol.Unknown)
                return true;
        }

        
        throw new NotImplementedException($"{reference} {(reference.GetType())}");
    }
}
}

