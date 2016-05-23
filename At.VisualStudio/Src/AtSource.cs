using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace At
{
class AtSource:Source
{
    public AtSource(LanguageService service,IVsTextLines textLines,Colorizer colorizer) 
    : base(service,textLines,colorizer)
    {
        Trace.WriteLine("new AtSource()");    
    }
}
}
