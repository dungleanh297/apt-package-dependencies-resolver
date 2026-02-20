using System;

namespace APTPackageDependenciesResolver;

public static class BinaryListExtensions
{
    public static bool BinaryInsert<T>(this List<T> list, T item)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        int index = list.BinarySearch(item);

        if (index < 0)
        {
            list.Insert(~index, item);
            return true;
        }

        return false;
    }

    public static bool BinaryInsert<T>(this List<T> list, T item, IComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ArgumentNullException.ThrowIfNull(comparer, nameof(comparer));

        int index = list.BinarySearch(item, comparer);

        if (index < 0)
        {
            list.Insert(~index, item);
            return true;
        }

        return false;
    }

    public static bool BinaryRemove<T>(this List<T> list, T item)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        int index = list.BinarySearch(item);

        if (index >= 0)
        {
            list.RemoveAt(index);
            return true;
        }

        return false;
    }

    public static bool BinaryRemove<T>(this List<T> list, T item, IComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ArgumentNullException.ThrowIfNull(comparer, nameof(comparer));

        int index = list.BinarySearch(item, comparer);

        if (index >= 0)
        {
            list.RemoveAt(index);
            return true;
        }

        return false;
    }   
}
