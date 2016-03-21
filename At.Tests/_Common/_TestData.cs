using System;
using System.Linq;

using System.IO;



namespace At.Tests
{
public partial class TestData : IDisposable
{    
    const string testUsername = "test";

    Test test;

    static bool initialized;

    //cctor
    static TestData()
    {
        Init();
    }

    public string Identifier(int? i = null) 
    {
        const int ascii_A = 65;

        return 
            (i != null) ? 
                ((char) Math.Abs(i.Value) + ascii_A).ToString() :
                "X";
                
        
    }

    static internal void Init()
    {
        if (initialized) return;



        initialized = true;
    }
  
    //ctor
    public TestData(Test t) 
    {
        this.test = t;

    }


    //SubmitChanges
    public void SubmitChanges() 
    { 
    }


    //IDisposable.Dispose()
    void IDisposable.Dispose()
    {  
    }

}
}
