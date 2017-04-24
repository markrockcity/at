using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Contexts;
using At.Syntax;

namespace At.Symbols
{
public class VariableSymbol : Symbol
{
    public VariableSymbol
    (
        string                    name, 
        VariableDeclarationSyntax syntaxNode    = null, 
        TypeSymbol                declaredType  = null, 
        ContextSymbol             context       = null
    ) 
    :   base(name,syntaxNode)
    {
        VariableType = declaredType;
        Context = context;
    }

    public ContextSymbol Context {get;}

    public VariableDeclarationSyntax Syntax => (VariableDeclarationSyntax) ((ISymbol)this).Syntax; 
        
    public override Symbol ParentSymbol => throw new NotImplementedException();

    protected TypeSymbol VariableType
    {
        get;
        private set;
    }

    public override TypeSymbol Type 
    {
        get => VariableType; 
        protected internal set { VariableType = value; }
    }

    public override TResult Accept<TResult>(Binding.BindingTreeVisitor<TResult> visitor)
    {
        return visitor.VisitVariable(this);
    }
}
}
