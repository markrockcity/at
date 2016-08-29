using System;
using System.Collections.Generic;
using System.Linq;

namespace At
{
//SyntaxToken 
public class AtToken : AtSyntaxNode
{
    TokenKind _kind;

    internal AtToken
    (
        TokenKind kind,
        int       position,
        string    text=null, 
        ITokenSource tokenSrc = null,
        AtSyntaxList<AtSyntaxTrivia> leadingTrivia = null,
        AtSyntaxList<AtSyntaxTrivia> trailingTrivia = null,
        IEnumerable<AtDiagnostic> diagnostics = null)

        : base(new AtSyntaxNode[0],diagnostics){

        LeadingTrivia   = leadingTrivia  ?? AtSyntaxList<AtSyntaxTrivia>.Empty;
        TrailingTrivia  = trailingTrivia ?? AtSyntaxList<AtSyntaxTrivia>.Empty;
        Text            = text;
        TokenSource     = tokenSrc;
        Position        = position;
        _kind           = kind;
    }

    public override bool   IsToken => true;
    public override int    Position {get;}
    public override string Text     {get;}

    public TokenKind Kind    => _kind;
    public int       RawKind => _kind.value;

    /// <summary>The token source used by the lexer to extract this token.</summary>
    public ITokenSource TokenSource {get;}

    public override string FullText =>
        string.Concat(string.Concat(LeadingTrivia.Select(_=>_.FullText)),
                        Text,
                        string.Concat(TrailingTrivia.Select(_=>_.FullText)));

    public AtSyntaxList<AtSyntaxTrivia> LeadingTrivia {get;internal set;}
    public AtSyntaxList<AtSyntaxTrivia> TrailingTrivia {get;internal set;}

    public override string ToString() =>
        Kind==TokenKind.EndOfFile ? "<EOF>" :
        Kind==TokenKind.StartOfFile ? "<StartOfFile>" :
        $"{Kind.Name}({Text})";
}
}