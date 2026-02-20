using System.Diagnostics.CodeAnalysis;

namespace APTPackageDependenciesResolver;

public class DebianPackageVersion : ISpanParsable<DebianPackageVersion>
{
    public uint Epoch { get; private init;}

    public string UpstreamVersion { get; private init; }

    public string? DebianRevision { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private DebianPackageVersion() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


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

        return new DebianPackageVersion()
        {
            Epoch = epoch,
            UpstreamVersion = new string(upstreamVersion),
            DebianRevision = debianRevision.Length > 0 ? new string(debianRevision) : null
        };
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
}
