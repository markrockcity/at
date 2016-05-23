using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace At
{
    class Colorizer:Microsoft.VisualStudio.Package.Colorizer
    {
        LanguageService svc;

        public Colorizer(LanguageService svc,IVsTextLines buffer,IScanner scanner) : base(svc,buffer,scanner)
        {
            this.svc = svc;
        }

        public override int ColorizeLine(int line,int length,IntPtr ptr,int state,uint[] attrs)
        {
                    
            if (attrs != null) 
            {
                for(var i = 0; i < attrs.Length; ++i)
                {
                    attrs[i] = (uint) i;
                }
            }
            else
            {
                (svc as AtLanguageService)?.Write("attrs==null");
            }

            return state;
        }
    }
}
