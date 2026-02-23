using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace APTPackageDependenciesResolver;

public static class ThrowHelper
{
    private const int MaximumContextLength = 128;

    [DoesNotReturn]
    public static void ThrowInvalidFormat(in ReadOnlySpan<char> context, in ReadOnlySpan<char> invalidPart, string message)
    {
        if (!context.Overlaps(invalidPart, out int overlapIndex) || overlapIndex < 0)
        {
            throw new ArgumentException("Invalid part is not belong to the context", nameof(invalidPart));
        }

        int contextStartIndex, contextEndIndex;
        int contextHalfIndex = overlapIndex + invalidPart.Length / 2;

        int invalidLineStartIndex = context[0..overlapIndex].LastIndexOf('\n') + 1;
        int invalidLineEndIndex = overlapIndex + invalidPart.Length + context[(overlapIndex + invalidPart.Length)..].IndexOf('\n') - 1;

        // Invalid line is the first line in the context, starting from the beginning
        if (invalidLineStartIndex < 1)
        {
            invalidLineStartIndex = 0;
        }

        // Invalid line is the last line in the context, ending at the last of the context
        if (invalidLineEndIndex < overlapIndex + invalidPart.Length - 1)
        {
            invalidLineEndIndex = context.Length;
        }

        // If the half left is too much, reduce it so it won't exceed to 128 character from the beginning to the middle part of invalid part
        if (contextHalfIndex > MaximumContextLength)
        {
            // Trim to the nearest new line that we found from the middle part of invalid part to the left, so we won't break the line in the middle
            contextStartIndex = contextHalfIndex - MaximumContextLength + context[(contextHalfIndex - MaximumContextLength)..contextHalfIndex].IndexOf('\n');

            // The last resort is to just trim to the maximum context length if we can't find any new line in the left part of the context
            if (contextStartIndex < 0)
            {
                contextStartIndex = contextHalfIndex - MaximumContextLength;
            }
        }
        else
        {
            // Take the whole context from the beginning to the middle of invalid part
            contextStartIndex = 0;
        }

        // Do the same thing for the right part of the context, the rule will be the same as the left part.
        if (context.Length - contextHalfIndex > MaximumContextLength)
        {
            contextEndIndex = contextHalfIndex + context[contextHalfIndex..(contextHalfIndex + MaximumContextLength)].LastIndexOf('\n');

            if (contextEndIndex < 0)
            {
                contextEndIndex = contextHalfIndex + MaximumContextLength;
            }
        }
        else
        {
            contextEndIndex = context.Length;
        }

        // If the invalid line is too long, compare to the context, then the whole context will be taken instead of the invalid line.
        if (contextStartIndex > invalidLineStartIndex)
        {
            invalidLineStartIndex = contextStartIndex;
        }
        if (contextEndIndex < invalidLineEndIndex)
        {
            invalidLineEndIndex = contextEndIndex;
        }

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(message);
        stringBuilder.Append("\n\n");

        if (contextStartIndex < invalidLineStartIndex)
        {
            stringBuilder.Append(context[contextStartIndex..invalidLineStartIndex]);
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(context[invalidLineStartIndex..invalidLineEndIndex]);
        stringBuilder.AppendLine();
        stringBuilder.Append(new string(' ', overlapIndex - invalidLineStartIndex));
        stringBuilder.Append(new string('^', invalidPart.Length));

        if (contextEndIndex > invalidLineEndIndex)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append(context[invalidLineEndIndex..contextEndIndex]);
        }

        throw new FormatException(stringBuilder.ToString());
    }
}
