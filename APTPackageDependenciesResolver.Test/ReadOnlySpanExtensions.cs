namespace APTPackageDependenciesResolver;

public static class ReadOnlySpanExtensions
{
    public static bool Contains<T>(this in ReadOnlySpan<T> values, Predicate<T> predicate)
    {
        foreach (var value in values)
        {
            if (predicate(value))
            {
                return true;
            }
        }

        return false;
    }
}
