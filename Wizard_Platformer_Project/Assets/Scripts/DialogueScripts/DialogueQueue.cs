using System;
using System.Collections.Generic;


public class DialogueQueue<T>
{
    
    private LinkedList<T> _list = new LinkedList<T>();

   
    public int Count => _list.Count;

   
    public bool IsEmpty => _list.Count == 0;

  
    public void Enqueue(T item)
    {
        _list.AddLast(item);
    }

    
    public T Dequeue()
    {
        if (IsEmpty)
            throw new InvalidOperationException("DialogueQueue is empty. Cannot Dequeue.");

        T value = _list.First.Value;
        _list.RemoveFirst();
        return value;
    }

   
    public T Peek()
    {
        if (IsEmpty)
            throw new InvalidOperationException("DialogueQueue is empty. Cannot Peek.");

        return _list.First.Value;
    }

    public void Clear()
    {
        _list.Clear();
    }

    public void LoadFromList(List<T> items)
    {
        Clear();
        foreach (T item in items)
            Enqueue(item);
    }
}
