using At.Syntax;

namespace At 
{
internal class CurlyBlockSyntax : ExpressionSyntax 
{
    private AtToken atToken;
    private string v;

    public CurlyBlockSyntax(string v, AtToken atToken, string text=null) : base(text) 
    {
        this.v = v; //"curly" ???
        this.atToken = atToken;
    }
}
}