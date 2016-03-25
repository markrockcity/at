using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace At
{
public class AtCompilation
{
    readonly string assemblyName;
    readonly SyntaxAndDeclarationManager _syntaxAndDeclarations;

    AtCompilation(string assemblyName = null,
                  //...
                  SyntaxAndDeclarationManager syntaxAndDeclarations = null)
    {
        this.assemblyName = assemblyName;
        this._syntaxAndDeclarations = syntaxAndDeclarations ?? 
                                      new SyntaxAndDeclarationManager(ImmutableArray<AtSyntaxTree>.Empty);
    }

    public static AtCompilation Create(AtSyntaxTree[] trees)
    {
        var compilation = new AtCompilation();

        if (trees != null)
            compilation = compilation.AddSyntaxTrees(ImmutableArray<AtSyntaxTree>.Empty.AddRange(trees));

        return compilation;
    }

    public AtCompilation AddSyntaxTrees(ImmutableArray<AtSyntaxTree> trees) 
    {
        if (trees == null)
        {
            throw new ArgumentNullException(nameof(trees));
        }    

        if (!trees.Any())
            return this;

         var syntaxAndDeclarations = _syntaxAndDeclarations;
         syntaxAndDeclarations = syntaxAndDeclarations.AddSyntaxTrees(trees);
         return Update(syntaxAndDeclarations);
    }

    private AtCompilation Update(SyntaxAndDeclarationManager syntaxAndDeclarations)
    {
        return new AtCompilation(assemblyName,syntaxAndDeclarations);
    }

    public AtEmitResult Emit(Stream peStream, CancellationToken cancellationToken = default(CancellationToken)) 
    {
        if (peStream == null)
        {
            throw new ArgumentNullException(nameof(peStream));
        }
 
        if (!peStream.CanWrite)
        {
            throw new ArgumentException("peStream must support write", nameof(peStream));
        }

        var cSharpTrees = csharpSyntaxTrees(_syntaxAndDeclarations.syntaxTrees);
 
        var cSharpCompilation = CSharpCompilation.Create(  assemblyName
                                                          ,cSharpTrees
                                                          ,references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)}
                                                          ,options: null);

        var result = cSharpCompilation.Emit(peStream, cancellationToken: cancellationToken);
        return atEmitREsult(result, cSharpTrees,cancellationToken);
    }

    private IEnumerable<CSharpSyntaxTree> csharpSyntaxTrees(ImmutableArray<AtSyntaxTree> atSyntaxTrees)
    {
       
        foreach(var tree in atSyntaxTrees)
        { 
           var converter = new SyntaxTreeConverter(atSyntaxTree: tree);        
           yield return converter.ConvertToCSharpTree();
        }
    }

    AtEmitResult atEmitREsult(EmitResult result, IEnumerable<SyntaxTree> syntaxTrees,CancellationToken cancellationToken)
    {
        return new AtEmitResult(  result.Success
                                 ,result.Diagnostics
                                 ,syntaxTrees.Select(_=>_.GetText(cancellationToken).ToString()));
    }
}
}