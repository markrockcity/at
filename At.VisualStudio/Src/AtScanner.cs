using System.Diagnostics;
using EnvDTE;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace At
{
class AtScanner : IScanner
{
    //AtLexer lexer;
    IVsTextLines buffer;
    string source;

    public AtScanner(IVsTextLines buffer)
    {
        //MessageBox.Show($"new AtScanner(...)");
        Trace.WriteLine("new AtScanner()");    

        this.buffer = buffer;
    }

    bool b = true;
    public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo,ref int state)
    {
        Trace.WriteLine($"AtScanner.ScanTokenAndProvideInfoAboutIt(\"{tokenInfo}\",{state})");
        //System.Diagnostics.Debugger.Break();
       
        //tokenInfo.Type = TokenType.String;
        tokenInfo.Color = TokenColor.Comment;
        tokenInfo.StartIndex = 0;
        tokenInfo.EndIndex = source.Length-1;
        //tokenInfo.Token = 1;
        tokenInfo.Type = TokenType.Comment;
        tokenInfo.Trigger = TokenTriggers.MatchBraces;
        
        var r = b;
        b = false;
        return r;
    }

    public void SetSource(string source,int offset)
    {
        Trace.WriteLine($"AtScanner.SetSource({source},{offset})");   
         
        this.source = source.Substring(offset);

        var dte = (DTE) ServiceProvider.GlobalProvider.GetService(typeof(DTE));
        dte.StatusBar.Text = source;
    }
}
}
