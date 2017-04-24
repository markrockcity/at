using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Binding;
using At.Contexts;

namespace At.Symbols
{
public class MethodSymbol : ContextSymbol
{
    readonly Dictionary<IEnumerable<TypeArgument>,MethodDefinition> _implementations
        = new Dictionary<IEnumerable<TypeArgument>, MethodDefinition>();

    protected internal MethodSymbol(string name, AtSyntaxNode syntaxNode, ContextSymbol parentContext) : base(name,syntaxNode,parentContext)
    {
    }

    public MethodDefinition Definition => (MethodDefinition) _definition;
    public MethodDeclaration Declaration => (MethodDeclaration) _declaration;
    public override TypeSymbol Type {get; protected internal set;}

    /// <summary>Gets a version of this method with type arguments applied.</summary>
    /// <returns>A method definition without type parameters.</returns>
    public MethodDefinition GetImplementation(IEnumerable<TypeArgument> typeArgs)
    {
        if (Definition.TypeParameters.Any() && (typeArgs?.Any() ?? false))
        {
            var _impl = _implementations.SingleOrDefault(impl=>typeArgs.All(_=>impl.Key.Contains(_))).Value ;                   
            return _impl;
        }
        else
        {
            return Definition;
        }
                    
    }

    internal MethodDefinition EnsureImplementation(IEnumerable<TypeArgument> typeArgs)
    {
        var impl =  GetImplementation(typeArgs);

        if (impl == null)
            impl = Binder.ApplyTypeArguments(Definition,typeArgs);        

        if (!_implementations.ContainsValue(impl))
            _implementations.Add(typeArgs, impl);
            
        return impl;  
    }
}
}
