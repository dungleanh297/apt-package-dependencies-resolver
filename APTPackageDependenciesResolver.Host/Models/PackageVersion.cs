using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace APTPackageDependenciesResolver;

public class DebianPackageVersion : ISpanParsable<DebianPackageVersion>, IComparable<DebianPackageVersion>, IEquatable<DebianPackageVersion>
{
    public uint Epoch { get; private init; }

    public string UpstreamVersion { get; private init; }

    public string? DebianRevision { get; private init; }

    public DebianPackageVersion(uint epoch, string upstreamVersion, string? debianRevision)
    {
        ArgumentNullException.ThrowIfNull(upstreamVersion, nameof(upstreamVersion));
        DebianPackageVersionValidator.ThrowOnInvalid(upstreamVersion, debianRevision);
        Epoch = epoch;
        UpstreamVersion = upstreamVersion;
        DebianRevision = debianRevision;
    }

    public static DebianPackageVersion Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (s.IsEmpty)
        {
            throw new FormatException("Version string cannot be empty.");
        }

        uint epoch = default;
        ReadOnlySpan<char> upstreamVersion;
        ReadOnlySpan<char> debianRevision = default;

        int epochIndexSeperator = s.IndexOf(':');

        if (epochIndexSeperator != -1)
        {
            if (!uint.TryParse(s[0..epochIndexSeperator], out epoch))
            {
                throw new FormatException($"Invalid epoch in version string. The epoch value must be a valid unsigned integer: {s}");
            }
        }

        int debianRevisionIndexSeperator = s.LastIndexOf('-');

        if (debianRevisionIndexSeperator != -1)
        {
            upstreamVersion = s[(epochIndexSeperator + 1)..debianRevisionIndexSeperator];
            debianRevision = s[(debianRevisionIndexSeperator + 1)..];
        }
        else
        {
            upstreamVersion = s[(epochIndexSeperator + 1)..];
        }

        DebianPackageVersionValidator.ThrowOnInvalid(upstreamVersion, debianRevision);

        return new DebianPackageVersion(
            epoch,
            new string(upstreamVersion),
            debianRevision.Length > 0 ? new string(debianRevision) : null
        );
    }


    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out DebianPackageVersion result)
    {
        try
        {
            result = Parse(s, provider);
            return true;
        }
        catch (FormatException)
        {
            result = null;
            return false;
        }
    }

    public static DebianPackageVersion Parse(string s, IFormatProvider? provider)
    {
        return Parse(s.AsSpan(), provider);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out DebianPackageVersion result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    public bool Equals(DebianPackageVersion? other)
    {
        if (other is null)
        {
            return false;
        }

        return Epoch == other.Epoch &&
               UpstreamVersion == other.UpstreamVersion &&
               DebianRevision == other.DebianRevision;
    }

    // This comparation is simple and completely wrong, I haven't read the documentation yet.
    public int CompareTo(DebianPackageVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        int comparationResult = Epoch.CompareTo(other.Epoch);

        if (comparationResult != 0)
        {
            return comparationResult;
        }

        comparationResult = UpstreamVersion.CompareTo(other.UpstreamVersion);

        if (comparationResult != 0)
        {
            return comparationResult;
        }

        if (DebianRevision is not null)
        {
            return DebianRevision.CompareTo(other.DebianRevision);
        }
        else if (other.DebianRevision is null)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public override string ToString()
    {
        if (Epoch == 0 && DebianRevision is null)
        {
            return UpstreamVersion;
        }
        StringBuilder stringBuilder = new StringBuilder();
        if (Epoch != 0)
        {
            stringBuilder.Append(Epoch);
            stringBuilder.Append(':');
        }

        stringBuilder.Append(UpstreamVersion);

        if (DebianRevision is not null)
        {
            stringBuilder.Append('-');
            stringBuilder.Append(DebianRevision);
        }

        return stringBuilder.ToString();
    }
}
