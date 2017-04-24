using System.Collections;
using System.Collections.Generic;
using At.Syntax;

namespace At
{
public interface IExpressionSource 
{
    ExpressionSyntax CreateExpression(params AtSyntaxNode[] nodes);
}


public class ExpressionSourceList<T> : IList<T> where T : IExpressionSource
{
    protected List<T> InnerList {get;} = new List<T>();

    public T this[int index]
    {
        get
        {
            return InnerList[index];
        }
        set
        {
            InnerList[index] = value;
        }
    }

    public int  Count => InnerList.Count;
    public bool IsReadOnly => false;

    public void Add(T item) => InnerList.Add(item);
    public void Clear() =>  InnerList.Clear();
    public bool Contains(T item)=>InnerList.Contains(item);

    public void CopyTo(T[] array,int arrayIndex)
    {
        InnerList.CopyTo(array,arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return InnerList.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return InnerList.IndexOf(item);
    }

    public void Insert(int index,T item)
    {
        InnerList.Insert(index,item);
    }

    public bool Remove(T item)
    {
       return InnerList.Remove(item);
    }

    public void RemoveAt(int index)
    {
        InnerList.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return InnerList.GetEnumerator();
    }
}
}
