using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public class SyntaxPattern
{
    public SyntaxPattern(string text, string token1 = null, string token2 = null, string key = null, SyntaxPattern[] content = null)
    {
        if (text==null && content==null)
            throw new ArgumentException("Either text or content must be non-null.",nameof(text));

        Text = text;
        Token1 = token1;
        Token2 = token2;
        Key     = key;
        Content = content;
    }

    public string Text   {get;} //should be null if SyntaxPattern represents multiple expressions
    public string Token1 {get;} 
    public string Token2 {get;} //Token2 = end delimiter
    public string Key    {get;} //x:Token
    public SyntaxPattern[] Content {get;} //null if no content specified, non-null but empty to match empty content
    
    public override string ToString()
    {
        return Text != null
                ?     Text 
                    + (Token1 != null ? "[" +Token1 : "")
                    + (Token2 != null ? ","+Token2 : "")
                    + (Token1 != null ? "]" : "")
                    + (Content != null ? $"({string.Join(",",(object[])Content)})" : "")

                : string.Join(",",(object[])Content);
    }
}
}
