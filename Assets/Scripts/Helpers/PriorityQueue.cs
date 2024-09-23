using System;
using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T>
{
    private readonly SortedDictionary<int, Queue<T>> _queue = new();

    public void Enqueue(T item, int priority)
    {
        if (!_queue.ContainsKey(priority))
        {
            _queue[priority] = new Queue<T>();
        }
        _queue[priority].Enqueue(item);
    }

    public T Dequeue()
    {
        if (_queue.Count == 0) throw new InvalidOperationException("Queue is empty");

        var firstKey = _queue.Keys.Min();
        var item = _queue[firstKey].Dequeue();
        if (_queue[firstKey].Count == 0)
        {
            _queue.Remove(firstKey);
        }
        return item;
    }

    public bool IsEmpty => _queue.Count == 0;
}