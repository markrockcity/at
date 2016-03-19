using System;

namespace At
{
//SyntaxToken 
public class AtToken : AtSyntaxNode
{
    TokenKind _kind;
    
    internal AtSyntaxList<AtToken> leadingTrivia  = AtSyntaxList<AtToken>.Empty;
    internal AtSyntaxList<AtToken> trailingTrivia = AtSyntaxList<AtToken>.Empty;


    public AtToken(TokenKind kind, int position, string text=null) : base(new AtSyntaxNode[0])
    {
        this.Text = text;
        this.Position = position;
        this._kind = kind;
    }

    public override bool IsToken
    {
        get
        {
            return true;
        }
    }

    public TokenKind Kind => _kind;

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
                    string.Concat(leadingTrivia),
                    Text,
                    string.Concat(trailingTrivia));
        }
    }

    public override string ToString()
    {
        return 
            _kind==TokenKind.EndOfFile ? "<EOF>" :
            _kind==TokenKind.StartOfFile ? "<StartOfFile>" :
            Text;
    }
}
}