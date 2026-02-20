using System.Runtime.InteropServices;

namespace APTPackageDependenciesResolver;

public class DebianPackageReader
{
    private const int InitialPackageListCapacity = 500;

    public const string DefaultStatusFilePath = "/var/lib/dpkg/status";
    private readonly string _statusFilePath;

    public DebianPackageReader(string statusFilePath = DefaultStatusFilePath)
    {
        _statusFilePath = statusFilePath;
    }

    public List<DebianPackage> GetAllInstalledPackages()
    {
        string data;

        using (FileStream statusFile = File.Open(_statusFilePath, FileMode.Open, FileAccess.Read))
        {
            using StreamReader reader = new StreamReader(statusFile);
            data = reader.ReadToEnd();
        }

        Dictionary<string, Range> stanzaRanges = SplitToStanzasFromControlData(data);

        DebianPackageParsingContext context = new DebianPackageParsingContext(
            ControlData: data,
            StanzaRanges: stanzaRanges,
            Packages: new Dictionary<string, DebianPackage>(stanzaRanges.Count),
            VirtualPackages: new Dictionary<string, DebianVirtualPackage>()
        );

        List<DebianPackage> result = new List<DebianPackage>(stanzaRanges.Count);

        foreach ((string packageName, Range stanzaRange) in context.StanzaRanges)
        {
            ref DebianPackage packageInfo = ref CollectionsMarshal.GetValueRefOrAddDefault(context.Packages, packageName, out var _)!;
            
            if (packageInfo == null)
            {
                packageInfo = DebianPackageInformationParser.Parse(packageName, stanzaRange, context);
            }
            
            result.Add(packageInfo);
        }

        return result;
    }

    private Dictionary<string, Range> SplitToStanzasFromControlData(string data)
    {
        var result = new Dictionary<string, Range>(InitialPackageListCapacity);

        foreach (var range in data.AsSpan().Split("\n\n"))
        {
            if (range.Start.Equals(range.End))
            {
                continue;
            }
            
            string packageName = GetThePackageName(data.AsSpan().Slice(range));
            result.Add(packageName, range);
        }

        return result;
    }

    private string GetThePackageName(ReadOnlySpan<char> stanza)
    {
        const string PackageFieldNameWithSeperator = "Package:";

        var fieldNameIndex = stanza.IndexOf(PackageFieldNameWithSeperator);

        if (fieldNameIndex < 0)
        {
            throw new InvalidOperationException($"Unable to find the package field on this stanza: {new string(stanza)}");    
        }

        var findNewLineStartRange = fieldNameIndex + PackageFieldNameWithSeperator.Length;

        var newLineIndex = findNewLineStartRange + stanza.Slice(findNewLineStartRange).IndexOf('\n');

        if (newLineIndex < PackageFieldNameWithSeperator.Length)
        {
            newLineIndex = stanza.Length;
        }

        ReadOnlySpan<char> packageName = stanza[findNewLineStartRange..newLineIndex].Trim();

        return new string(packageName);
    }
}
