using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using At.Binding;
using At.Symbols;

namespace At.Contexts
{
public class MethodDefinition : Definition
{
    List<Operation> _operations = new List<Operation>();
    List<Operation> _exits = new List<Operation>();
    Operation lastOperation; //on main branch
    bool ended = false;


    public MethodDefinition(MethodSymbol methodSymbol, IEnumerable<ParameterSymbol> parameters, Context parentCtx,DiagnosticsBag diagnostics,AtSyntaxNode syntaxNode = null) : base(parentCtx,diagnostics,syntaxNode)
    {
        Symbol = methodSymbol;

        if  (methodSymbol != null)
            methodSymbol._definition = this;

        Constraints = ImmutableArray<IBindingConstraint>.Empty;
        TypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;

        var _parameters = new List<ParameterSymbol>();
        foreach(var p in parameters)
        {
            Define(p);

            if (p.Type == TypeSymbol.Unknown)
            {
                var typeParameter = new TypeParameterSymbol($"T_{toPascalCase(p.Name)}",parentContext: methodSymbol);
                p.Type = typeParameter;
                TypeParameters = TypeParameters.Add(typeParameter);
            }

            _parameters.Add(p);
        }
        Parameters = _parameters.ToImmutableArray();

        Define(methodSymbol);
    }

    public ImmutableArray<IBindingConstraint> Constraints {get; private set;} 
    public ImmutableArray<Operation> Exits => _exits.ToImmutableArray(); 
    public MethodSymbol Symbol {get;}
    public ImmutableArray<TypeParameterSymbol> TypeParameters {get; private set;}
    public ImmutableArray<ParameterSymbol> Parameters {get;}
    public TypeSymbol ReturnType {get; internal  set;}

    protected internal override ContextSymbol ContextSymbol => Symbol;

    public override bool HasContents => _operations.Any();

    public override IEnumerable<IBindingNode> Contents()
    {
        return _operations.ToList();
    }

    /// <summary>Called after all nodes have been added</summary>
    protected internal virtual void End()
    {
        if (ended)
            return;

        lock(endLock)
        {
            if (   lastOperation is BinaryOperation o 
                && (   o.Left.Type  is TypeParameterSymbol 
                    || o.Right.Type is TypeParameterSymbol))
            {
                var t = new TypeParameterSymbol("R_"+Symbol.Name,parentContext:Symbol);
                TypeParameters = TypeParameters.Add(t);
                if (ReturnType == null || ReturnType == TypeSymbol.Unknown)
                    ReturnType = t;                
            }          
            
            if (ReturnType == null || ReturnType == TypeSymbol.Unknown)
                    ReturnType = lastOperation?.Type;

            if (lastOperation != null)
                _exits.Add(lastOperation);
            ended = true;
        }

    } object endLock = new object();

    protected internal override void AddNode(IBindingNode node)
    {
        if (ended)
            throw new InvalidOperationException(GetType()+".End() has already been called.");

        if (node is Operation o)
        {
            _operations.Add(o);
            lastOperation = o;

            if (o is BinaryOperation bo)
            {
                addConstraint(new OperatorConstraint((OperatorSymbol) bo.Operator,new[]{bo.Left.Type,bo.Right.Type},bo.Type));
            }
        }
        else
            base.AddNode(node); //???
    }

    private void addConstraint(IBindingConstraint p)
    {
        Constraints = Constraints.Add(p);
    }

    // http://stackoverflow.com/questions/23345348/topascalcase-c-sharp-for-all-caps-abbreviations
    private string toPascalCase(string s)
    {
        var m = Regex.Match(s, "^(?<word>^[a-z]+|[A-Z]+|[A-Z][a-z]+)+$");
        var g = m.Groups["word"];
        var t = Thread.CurrentThread.CurrentCulture.TextInfo;
        var sb = new StringBuilder();
        foreach (var c in g.Captures.Cast<Capture>())
            sb.Append(t.ToTitleCase(c.Value.ToLower()));
        return sb.ToString();
    }}
}
