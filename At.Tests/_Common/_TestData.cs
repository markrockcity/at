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
