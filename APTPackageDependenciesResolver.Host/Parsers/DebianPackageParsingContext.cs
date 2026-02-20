namespace APTPackageDependenciesResolver;

public record class DebianPackageParsingContext(
    string ControlData,
    Dictionary<string, Range> StanzaRanges,
    Dictionary<string, DebianPackage> Packages,
    Dictionary<string, DebianVirtualPackage> VirtualPackages
);
