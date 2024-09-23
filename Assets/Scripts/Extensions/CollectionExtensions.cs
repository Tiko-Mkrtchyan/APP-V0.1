using System.Collections.Generic;

public static class CollectionExtensions
{
    public static void AddUnique<T>(this IList<T> self, T item)
    {
        if(!self.Contains(item))
            self.Add(item);
    }
    
    public static void AddUnique<T>(this IList<T> self, IEnumerable<T> items )
    {
        foreach(var item in items)
            if(!self.Contains(item))
                self.Add(item);
    }
}