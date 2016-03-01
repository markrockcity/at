namespace At
{
public class AtToken 
{
    private TokenKind _kind;


    public AtToken(TokenKind kind, int position, string text = null) 
    {
        this._kind = kind;
        this.Text = text;
        this.Position = position;

    }

    public TokenKind Kind => _kind;


    public int Position
    {
        get;
    }

    public object Text
    {
        get;
    }
}
}