using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace At
{
public interface IScanner<T>
{
    T LookAhead(int k);
    T Current  {get;}
    T Next {get;}
    bool End {get;}
    int  Position {get;}
}

public class Scanner<T> : IScanner<T>, IEnumerator<T>
{
    readonly List<T> buffer = new List<T>();
    readonly IEnumerator<T> enumerator;

    T current;
    int position = -1;

    public Scanner(IEnumerable<T> input)
    {
        enumerator = input.GetEnumerator();
    }

    public T    Current  {get {return current;}}
    public T    Next     {get {return LookAhead(1);}}
    public bool End      {get; private set; }   
    public int  Position {get {return position;}}

    public T Consume()
    {
        var c = current;
        MoveNext();
        return c;
    }

    public bool MoveNext()
    {
       if (buffer.Count>0) 
       {
          current = buffer[0];
          buffer.RemoveAt(0);
          position++;
          return true;
       }       

       if (enumerator.MoveNext()) 
       {
          current = enumerator.Current;
          position++;
          return true;
       }

       current  = default(T);
       this.End = true;
       return false;
    }


    public T LookAhead(int k)
    {
       if (k==0) return current;

       while (k > buffer.Count) 
       {
          if (!enumerator.MoveNext()) 
            return default(T);

          buffer.Add(enumerator.Current);
       }

       return buffer[k-1];
    }

    void IDisposable.Dispose(){}
    object System.Collections.IEnumerator.Current{ get { return this.Current; }}
    void System.Collections.IEnumerator.Reset(){throw new NotSupportedException();}
}
}
