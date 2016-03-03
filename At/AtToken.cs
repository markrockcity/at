using System;

namespace At
{
//SyntaxToken 
public class AtToken : AtSyntaxNode
{
    private TokenKind _kind;


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
}
}