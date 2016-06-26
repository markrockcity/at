using System.Collections.Generic;

namespace At.Tests
{
partial class TestData
{
    //inputs
    public IEnumerable<string> classInputs(string className,string baseClass) => new[] 
    {
        $"@{className}<>",
        $"@{className}<>;",
        $"@{className}<>{{}}",
        $"@{className}<  > {{ \r\n }}",
        $"\r\n  @{className}<  > {{ \r\n }}\r\n\r\n  ",
        $"@{className}<T>",
        $"@{className}< T >",
        $"@{className}< T, U>",
        $"@{className}< T, U>",
        $"@{className}<T,U> : {baseClass}",
        $"@{className}<T,U> : {baseClass};",
        $"@{className}<T,U> : {baseClass}<T>",
        $"@{className}<T,U> : {baseClass}<T>;",
        $"@ns : namespace {{@{className}<T,U> : {baseClass}<T>;}}",
        $"#import System; @{className}<>",
    };       

    public IEnumerable<string> variableInputs(string id, string className) => new[]
    {
        $"@{id}",
        $"@{id};",
        $"@{id} : {className}",
        $"@{id} : {className};",
        $"@{id} : {className}<{className}>",
        $"@{id} : {className}<{className},{className}>;",
        $"@{id} : {className}<{className},{className}<{className}>>",
    };
}
}
