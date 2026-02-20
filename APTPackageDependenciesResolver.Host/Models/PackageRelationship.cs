using System.Diagnostics.CodeAnalysis;

namespace APTPackageDependenciesResolver;

public class PackageRelationship : IRelationship, 
    IComparable<PackageRelationship>, 
    IEquatable<PackageRelationship>
{
    public IPackage Package { get; }

    public VersionRelationType? RelationType { get; }

    [NotNullIfNotNull(nameof(RelationType))]
    public DebianPackageVersion? Version { get; }

    public PackageRelationship(IPackage package)
    {
        Package = package ?? throw new ArgumentNullException(nameof(package));
    }

    public PackageRelationship(IPackage package, VersionRelationType relationType, DebianPackageVersion version)
    {
        Package = package ?? throw new ArgumentNullException(nameof(package));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        RelationType = relationType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Package, RelationType, Version);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PackageRelationship otherRelationship)
        {
            return false;
        }

        return Equals(otherRelationship);
    }

    public int CompareTo(PackageRelationship? other)
    {
        if (other is null)
        {
            return -1;
        }

        return Package.CompareTo(other.Package);
    }

    public bool Equals(PackageRelationship? other)
    {
        if (other is null)
        {
            return false;
        }

        return Package.Equals(other.Package) && RelationType == other.RelationType && Version == other.Version;
    }
}
