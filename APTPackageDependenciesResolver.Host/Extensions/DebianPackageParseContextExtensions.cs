namespace APTPackageDependenciesResolver.Host.Extensions;

public static class DebianPackageParseContextExtensions
{
    public static IPackage GetPackageByName(this DebianPackageParsingContext context, string packageName)
    {
        if (context.Packages.TryGetValue(packageName, out var package))
        {
            return package;
        }

        if (context.StanzaRanges.TryGetValue(packageName, out var range))
        {
            // Don't need to add the package to the context here since it will be added in the DebianPackageInformationParser.Parse method before parsing relationships to handle circular dependencies
            return DebianPackageParser.Parse(packageName, range, context);
        }
        else if (context.VirtualPackages.TryGetValue(packageName, out var virtualPackage))
        {
            return virtualPackage;
        }
        else
        {
            virtualPackage = new DebianVirtualPackage(packageName);
            context.VirtualPackages[packageName] = virtualPackage;
            return virtualPackage;
        }

    }
}
