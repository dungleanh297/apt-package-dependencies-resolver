using System.Runtime.CompilerServices;

namespace APTPackageDependenciesResolver;

public class DebianPackageVersionValidator
{
    public static void ThrowOnInvalid(in ReadOnlySpan<char> upstreamVersion, in ReadOnlySpan<char> debianRevision)
    {
        foreach (char c in debianRevision)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '+' && c != '~')
            {
                throw new FormatException($"Invalid character '{c}' in Debian revision. The Debian revision can only contain letters, digits, dots and plus signs: {debianRevision}");
            }
        }

        if (upstreamVersion.Length == 0)
        {
            throw new FormatException("Upstream version cannot be empty.");
        }

        if (!char.IsDigit(upstreamVersion[0]))
        {
            throw new FormatException($"Invalid upstream version. The upstream version must start with a digit: {upstreamVersion}");    
        }

        if (debianRevision.Length == 0)
        {
            goto CheckWhenShouldNotHaveHyphenInUpstream;
        }
        else
        {
            goto CheckWhenCanHaveHyphenInUpstream;
        }

    
    CheckWhenShouldNotHaveHyphenInUpstream:
        foreach (char c in upstreamVersion)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '+' && c != '~')
            {
                throw new FormatException($"Invalid character '{c}' in upstream version. The upstream version can only contain letters, digits, dots, plus signs and tildes and cannot have hyphens when there is no Debian revision: {upstreamVersion}");
            }
        }

        return;

    CheckWhenCanHaveHyphenInUpstream:
        foreach (char c in upstreamVersion)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '+' && c != '~' && c != '-')
            {
                throw new FormatException($"Invalid character '{c}' in upstream version. The upstream version can only contain letters, digits, dots, plus signs, tildes and hyphens: {upstreamVersion}");
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidPrefixCharacter(char c)
    {
        return char.IsAsciiLetterOrDigit(c) || c == '.' || c == '+' || c == '-' || c == '~';
    }
}
