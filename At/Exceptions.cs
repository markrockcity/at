using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;

namespace At
{
[Serializable]
public class AtException : Exception
{
    internal AtException()
    {
    }
    internal AtException(string message) : base(message)
    {
    }
    internal AtException(string message,Exception innerException) : base(message,innerException)
    {
    }
    protected AtException(SerializationInfo info,StreamingContext context) : base(info,context)
    {
    }
}

public class CompilationException : AtException
{
    readonly AtEmitResult result;

    internal CompilationException(AtEmitResult result) : base
    (
       result.ConvertedSources().First()+result.Diagnostics[0].GetMessage()){
       this.result = result;
    }

    public override string ToString()
    {
        return result.ConvertedSources().First()+"\r\n\r\n"+base.ToString();
    }
}
}