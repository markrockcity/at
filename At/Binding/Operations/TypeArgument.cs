using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public class TypeArgument : IBindingNode, IEquatable<TypeArgument>
{

    public TypeArgument(TypeSymbol argument, TypeParameterSymbol param, NameSyntax syntax = null)
    {
        Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        Parameter = param ?? throw new ArgumentNullException(nameof(param));
        Syntax = syntax;
    }

    public TypeSymbol Argument {get;}

    public TypeParameterSymbol Parameter {get;}

    public NameSyntax Syntax {get;}

    AtSyntaxNode IBindingNode.Syntax => Syntax;

    public  T Accept<T>(BindingTreeVisitor<T> visitor) => throw new NotImplementedException();
    

    public override string ToString() =>$"TypeArg({Parameter.Name}={Argument.Name})";

    bool IEquatable<TypeArgument>.Equals(TypeArgument other)
      => other.Parameter==Parameter && other.Argument==Argument;
}
}
