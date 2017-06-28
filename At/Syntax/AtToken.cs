using System;
using System.Collections.Generic;
using System.Linq;
using Limpl;

namespace At
{
//SyntaxToken 
public class AtToken : AtSyntaxNode, Limpl.IToken
{
    TokenKind _kind;

    internal AtToken
    (
        TokenKind kind,
        int       position,
        string    text=null, 
        Limpl.ITokenSource<AtToken> tokenSrc = null,
        Limpl.SyntaxList<AtSyntaxTrivia> leadingTrivia = null,
        Limpl.SyntaxList<AtSyntaxTrivia> trailingTrivia = null,
        IEnumerable<AtDiagnostic> diagnostics = null,
        object value = null)

        : base(new AtSyntaxNode[0],diagnostics){

        _kind           = kind;
        LeadingTrivia   = leadingTrivia  ?? new Limpl.SyntaxList<AtSyntaxTrivia>(this,new AtSyntaxTrivia[0],(ref AtSyntaxTrivia n, Limpl.ISyntaxNode p) => n.Parent = p);
        TrailingTrivia  = trailingTrivia ?? new Limpl.SyntaxList<AtSyntaxTrivia>(this,new AtSyntaxTrivia[0],(ref AtSyntaxTrivia n, Limpl.ISyntaxNode p) => n.Parent = p);
        Text            = text;
        TokenSource     = tokenSrc;
        Position        = position;
        Value           = value;
    }

    public override bool   IsToken     => true;
    public override string PatternName => Kind.Name;
    public override int    Position {get;}
    public override string Text     {get;}

    public TokenKind Kind    => _kind;
    public int       RawKind => _kind.value;

    /// <summary>The token source used by the lexer to extract this token.</summary>
    public Limpl.ITokenSource<AtToken> TokenSource {get;}

    public override string FullText =>
        string.Concat(string.Concat(LeadingTrivia.Select(_=>_.FullText)),
                        Text,
                        string.Concat(TrailingTrivia.Select(_=>_.FullText)));

    
    public Limpl.SyntaxList<AtSyntaxTrivia> LeadingTrivia {get;internal set;}
    public Limpl.SyntaxList<AtSyntaxTrivia> TrailingTrivia {get;internal set;}
    public object Value { get; }

    ITokenKind IToken.Kind => Kind;
    IReadOnlyList<ISyntaxTrivia> IToken.LeadingTrivia => LeadingTrivia;
    IReadOnlyList<ISyntaxTrivia> IToken.TrailingTrivia => TrailingTrivia;
    bool ISyntaxNode.IsTrivia => this is AtSyntaxTrivia;
    ISyntaxNode ISyntaxNode.Parent => base.Parent;

    public override bool MatchesPattern(SyntaxPattern pattern, IDictionary<string,AtSyntaxNode> d = null)
    {
        var s = pattern.ToString(withKeyPrefix:false);
        var t = PatternStrings().Any(_=>_==s);
        if (t && d != null && pattern.Key != null)
            d[pattern.Key] = this;
        return t;
    }

    public override string ToString() =>
            Kind==TokenKind.EndOfFile 
                ? "<EOF>" 
            
        :   Kind==TokenKind.StartOfFile 
                ? "<StartOfFile>" 

        :   $"{Kind.Name}({Text})";

    public override IEnumerable<string> PatternStrings()
    {
        yield return $"Token({PatternName})";
        yield return "Token";
        yield return "Node";
    }

    public override TResult Accept<TResult>(AtSyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitToken(this);
    }
}
}