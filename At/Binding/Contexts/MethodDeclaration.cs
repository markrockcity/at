using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using At.Contexts;
using At.Symbols;
using At.Syntax;

namespace At.Binding
{
public class MethodDeclaration : Declaration
{
    public MethodDeclaration(Context parentContext,  MethodDeclarationSyntax syntaxNode, MethodSymbol methodSymbol = null, IEnumerable<TypeParameterSymbol> typeParameters = null, IEnumerable<ParameterSymbol> parameters = null, TypeSymbol returnType = null, MethodDefinition definition = null, Operation prev = null) : base(parentContext,syntaxNode,prev)
    {
        Symbol = methodSymbol ??  new MethodSymbol(syntaxNode.Identifier.Text,syntaxNode,parentContext?.ContextSymbol);
    
        if  (methodSymbol != null)
            methodSymbol._declaration = this;
            
        m_Parameters = parameters?.ToImmutableArray();
        m_TypeParameters = typeParameters?.ToImmutableArray();
        m_ReturnType = returnType ?? TypeSymbol.Unknown;
        Definition = definition;
    }

    public MethodSymbol Symbol         {get;}
    public MethodDefinition Definition {get;}

    public string Name => Symbol.Name;
    public MethodDeclarationSyntax Syntax => (MethodDeclarationSyntax) ExpressionSyntax;
         
    public ImmutableArray<TypeParameterSymbol> TypeParameters => Definition?.TypeParameters ?? m_TypeParameters ?? ImmutableArray<TypeParameterSymbol>.Empty;
    ImmutableArray<TypeParameterSymbol>? m_TypeParameters;

    public ImmutableArray<ParameterSymbol> Parameters => Definition?.Parameters ?? m_Parameters ?? ImmutableArray<ParameterSymbol>.Empty;
    ImmutableArray<ParameterSymbol>?  m_Parameters;

    public TypeSymbol ReturnType => Definition?.ReturnType ?? m_ReturnType ??  TypeSymbol.Unknown;
    TypeSymbol m_ReturnType;

    protected internal override Symbol DeclaredSymbol => Symbol;
    protected internal override TypeSymbol DeclaredType => throw new NotImplementedException();

    public override T Accept<T>(BindingTreeVisitor<T> visitor) => visitor.VisitMethodDeclaration(this);

    public override Operation ReplaceSymbol(ISymbol undefinedSymbol, ISymbol newSymbol)
    {
        throw new NotImplementedException();
    }

}
}
