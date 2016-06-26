using System;
using System.Linq;

using System.IO;



namespace At.Tests
{
public partial class TestData : IDisposable
{    
    static bool initialized;

    readonly string testUsername = "test";
    readonly Test test;

    //cctor
    static @TestData()
    {
        Init();
    }

    //ctor
    public @TestData(Test t) 
    {
        this.test = t;

    }

    internal static void @Init()
    {
        if (initialized) return;



        initialized = true;
    }

    public string @Identifier(int? i = null) 
    {
        const int ascii_A = 65;

        return 
            (i != null) ? 
                ((char) (Math.Abs(i.Value) + ascii_A)).ToString() :
                "X";
                
        
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
