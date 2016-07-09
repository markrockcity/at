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
        IEnumerable<AtDiagnostic> diagnostics = null) 

        : base(new AtSyntaxNode[0],diagnostics){

        this.Text     = text;
        this.Position = position;
        this._kind    = kind;
    }

    public override bool IsToken
    {
        get
        {
            return true;
        }
    }

    public TokenKind Kind => _kind;
    public int RawKind => _kind.value;

    public override int Position
    {
        get;
    }

    public override string Text
    {
        get;
    }

    public override string FullText
    {
        get
        {
            return 
                string.Concat(
                    string.Concat(leadingTrivia.Select(_=>_.FullText)),
                    Text,
                    string.Concat(trailingTrivia.Select(_=>_.FullText)));
        }
    }

    public override string ToString()
    {
        return 
            Kind==TokenKind.EndOfFile ? "<EOF>" :
            Kind==TokenKind.StartOfFile ? "<StartOfFile>" :
            $"{Kind.Name}({Text})";
    }
}
}