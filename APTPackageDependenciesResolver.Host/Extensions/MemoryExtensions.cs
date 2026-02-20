using System;
using System.Runtime.CompilerServices;

namespace APTPackageDependenciesResolver;

public static class MemoryExtensions
{
    extension<T> (ReadOnlySpan<T> source)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Slice(Range range)
        {
            return source[range.Start..range.End];
        }
    }
}
