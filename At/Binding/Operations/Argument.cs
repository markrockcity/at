using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
/// <summary>Bound argument in an invocation operation.</summary>
public class Argument : IBindingNode
{
    public Argument(Operation op, ParameterSymbol param = null, AtSyntaxNode syntax = null)
    {
        Operation = op ?? throw new ArgumentNullException(nameof(op));
        Parameter = param;
        Syntax = syntax;
    }

    public Operation Operation {get;}

    public TypeSymbol Type => Operation.Type;

    public ParameterSymbol Parameter {get; internal set;}

    public AtSyntaxNode Syntax {get;} 

    public T Accept<T>(BindingTreeVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public override string ToString() =>$"Arg({Operation})";
}
}
