using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Linq;

namespace At.Tests
{
[TestClass] public class MiscTests : Test
{
    [TestMethod]
    public void Test1()
    {
        using (var testData = new TestData(this))
        {
            var className = testData.Identifier();
            var input = $"@{className}<>"; // @X<>
            var output = AtProgram.compileStringToAssembly(input);
            verifyOutput(output, className);
        }
    }

    void verifyOutput(Assembly assembly, string className) 
    {
        assert_not_null(()=>assembly);
        assert_true(()=>assembly.GetTypes().Any(_=>_.Name==className&&_.IsClass));
    }
}
}
