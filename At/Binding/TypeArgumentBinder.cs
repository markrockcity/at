using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Contexts;
using At.Symbols;

namespace At.Binding
{
public class TypeArgumentBinder : BindingTreeVisitor<IBindingNode>
{
    private Context ctx;
    private Dictionary<TypeParameterSymbol,TypeSymbol> dict;

    public TypeArgumentBinder(Context ctx, IEnumerable<TypeArgument> typeArgs)
    {
        this.ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        dict = typeArgs.ToDictionary(_=>_.Parameter,_=>_.Argument)   
                ?? throw new ArgumentNullException(nameof(typeArgs)); 
    }

  
    protected internal override IBindingNode VisitSymbol(Symbol symbol)
    {
        Debug.WriteLine($"{this}.VisitSymbol({symbol} : {symbol.GetType()})");
        return symbol.Accept(this) ?? symbol;
    }

    protected internal override IBindingNode VisitBinary(BinaryOperation b)
    {
        var left = (Operation) b.Left.Accept(this) ?? b.Left;
        var right = (Operation) b.Right.Accept(this) ?? b.Right;

        var b2 =  new BinaryOperation(ctx,null,b.Operator,left,right,b.Previous);
//        b2.Type = impl.ReturnType
        return b2;
    }

    protected internal override IBindingNode VisitSymbolReference(SymbolReference symbolReference)
    {
        var s = symbolReference.Symbol.Accept(this) ?? symbolReference;
        return (s is SymbolReference) ? s : new SymbolReference((ISymbol) s,ctx,null,null);
    }

    protected internal override IBindingNode VisitParameter(ParameterSymbol p)
    {
        return p.ParameterType is TypeParameterSymbol tp
                    ? new ParameterSymbol(p.Name,dict[tp])
                    : p;
    }

}
}
