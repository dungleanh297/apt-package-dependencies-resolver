using System.Runtime.CompilerServices;

namespace APTPackageDependenciesResolver;

public static class MemoryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> Slice<T>(this in ReadOnlySpan<T> source, Range range)
    {
        return source[range.Start..range.End];
    }
}
