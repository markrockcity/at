using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace At
{
public class AtProgram
{
    static void Main(string[] args)
    {
            
    }

    internal static Assembly compileStringToAssembly(string input) 
    {
        Assembly assembly = null;

        var tree = AtSyntaxTree.ParseText(input);
        var compilation = AtCompilation.Create(trees: new[] {tree});

        using (var ms = new MemoryStream())
        {
            var result = compilation.Emit(ms);

            if (result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                assembly = Assembly.Load(ms.ToArray());
            }
            else
            {
                throw new CompilationException(result);
            }

            #if DEBUG
            Console.WriteLine(Environment.CurrentDirectory);
            ms.Seek(0, SeekOrigin.Begin);
            var f = new FileStream("x.dll",FileMode.Create);
            ms.CopyTo(f);
            f.Close();
            #endif
        }

        return assembly;
    }
}
}
