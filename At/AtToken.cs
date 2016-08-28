using System;
using System.Collections.Generic;
using System.Linq;

namespace At
{
//SyntaxToken 
public class AtToken : AtSyntaxNode
{
    TokenKind _kind;
    
    internal AtSyntaxList<AtSyntaxTrivia> leadingTrivia  = AtSyntaxList<AtSyntaxTrivia>.Empty;
    internal AtSyntaxList<AtSyntaxTrivia> trailingTrivia = AtSyntaxList<AtSyntaxTrivia>.Empty;

    internal AtToken
    (
        TokenKind kind, 
        int       position,
        string    text=null, 
        ITokenRule tokenDefinition = null,
        IEnumerable<AtDiagnostic> diagnostics = null) 

        : base(new AtSyntaxNode[0],diagnostics){

        Text            = text;
        TokenDefinition = tokenDefinition;
        Position        = position;
        _kind           = kind;
    }

    public override bool   IsToken => true;
    public override int    Position {get;}
    public override string Text     {get;}

    public TokenKind Kind    => _kind;
    public int       RawKind => _kind.value;

    /// <summary>The token definition used by the lexer to extract this token.</summary>
    public ITokenRule TokenDefinition {get;}

    public override string FullText =>
        string.Concat(string.Concat(leadingTrivia.Select(_=>_.FullText)),
                        Text,
                        string.Concat(trailingTrivia.Select(_=>_.FullText)));

    public override string ToString() =>
        Kind==TokenKind.EndOfFile ? "<EOF>" :
        Kind==TokenKind.StartOfFile ? "<StartOfFile>" :
        $"{Kind.Name}({Text})";
}
}