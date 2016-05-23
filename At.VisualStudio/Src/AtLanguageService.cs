using System;
using System.Diagnostics;
using EnvDTE;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace At
{

public class AtLanguageService : LanguageService
{
    LanguagePreferences prefs;
    AtScanner scanner;

    public override string Name => "At";
    public override string GetFormatFilterList() => "At files (*.at)";

    public override LanguagePreferences GetLanguagePreferences() 
    { 
       return prefs ?? (prefs = new LanguagePreferences(Site,GetType().GUID,Name));
    }

    private ColorableItem[] colors = new[] 
    {
        new ColorableItem("keyword" ,"at - keyword",
                            COLORINDEX.CI_BLUE,
                            COLORINDEX.CI_SYSPLAINTEXT_BK,
                            System.Drawing.Color.Brown,
                            System.Drawing.Color.Cyan,
                            FONTFLAGS.FF_DEFAULT),


        new ColorableItem("comment" ,"at - comment",
                            COLORINDEX.CI_DARKGRAY,
                            COLORINDEX.CI_LIGHTGRAY,
                            System.Drawing.Color.Silver,
                            System.Drawing.Color.Black,
                            FONTFLAGS.FF_BOLD),

        new ColorableItem("string" ,"___aaaa",
                            COLORINDEX.CI_PURPLE,
                            COLORINDEX.CI_MAGENTA,
                            System.Drawing.Color.AliceBlue,
                            System.Drawing.Color.Empty,
                            FONTFLAGS.FF_BOLD),    

        new ColorableItem("custom" ,"_ - __aaaa",
                            COLORINDEX.CI_RED,
                            COLORINDEX.CI_CYAN,
                            System.Drawing.Color.Red,
                            System.Drawing.Color.Empty,
                            FONTFLAGS.FF_STRIKETHROUGH),      
                            
    };

    public override int GetColorableItem(int i, out IVsColorableItem item)
    {
        item = colors[i%colors.Length];
        return S_OK;
    }

    public override int GetItemCount(out int count)
    {
        count = colors.Length;
        return 0;
    }

    internal void Write(string s)
    {
        Trace.WriteLine("AtLanguageService: "+s);

        var dte = (DTE) ServiceProvider.GlobalProvider.GetService(typeof(DTE));
        dte.StatusBar.Text = s;
    }

    public override Microsoft.VisualStudio.Package.Colorizer GetColorizer(IVsTextLines buffer)
    {
        return new Colorizer(this,buffer,GetScanner(buffer));
    }

    public override IScanner GetScanner(IVsTextLines buffer) 
    {
        return scanner ?? (scanner = new AtScanner(buffer));
    }

    public override AuthoringScope ParseSource(ParseRequest req)
    {
        Trace.WriteLine("AtLanguageService.ParseSource()");
        return null;
    }
}
}