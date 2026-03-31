using System;

public class MyQueue<T>
{
    private class Node
    {
        public T Data;
        public Node Next;

        public Node(T data)
        {
            Data = data;
            Next = null;
        }
    }

    private Node front;
    private Node rear;
    private int count;

    public int Count => count;

    public void Enqueue(T item)
    {
        Node newNode = new Node(item);

        if (rear == null)
        {
            front = newNode;
            rear = newNode;
        }
        else
        {
            rear.Next = newNode;
            rear = newNode;
        }

        count++;
    }

    public T Dequeue()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        T item = front.Data;
        front = front.Next;
        count--;

        if (front == null)
        {
            rear = null;
        }

        return item;
    }

    public T Peek()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        return front.Data;
    }

    public bool IsEmpty()
    {
        return count == 0;
    }

    public void Clear()
    {
        front = null;
        rear = null;
        count = 0;
    }
}
