using System;
using System.Diagnostics;

namespace At 
{
//"StringText"
public class AtSourceText 
{
    readonly string _source;

    internal AtSourceText(string source)
    {
        Debug.Assert(source != null);
    
        this._source = source;
    }       

    public int    Length => _source.Length;      
    public string Source => _source;

    //AtSourceText.From()
    public static AtSourceText From(string text) 
    {
        return new AtSourceText(text);
    }

    public static implicit operator AtSourceText (string source) => new AtSourceText(source);
}
}