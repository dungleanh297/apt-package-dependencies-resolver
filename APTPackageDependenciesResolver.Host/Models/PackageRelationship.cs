using System.Diagnostics.CodeAnalysis;
using APTPackageDependenciesResolver.Host.Models;

namespace APTPackageDependenciesResolver;

public sealed class PackageRelationship : IRelationship
{
    private readonly VersionRelationType _versionRelationType;

    public IPackage Package { get; }

    [NotNullIfNotNull(nameof(Version))]
    public VersionRelationType? RelationType => Version is null ? null : _versionRelationType;

    public DebianPackageVersion? Version { get; }

    public PackageRelationship(IPackage package)
    {
        Package = package ?? throw new ArgumentNullException(nameof(package));
    }

    public PackageRelationship(IPackage package, VersionRelationType versionRelationType, DebianPackageVersion version)
    {
        Package = package ?? throw new ArgumentNullException(nameof(package));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        _versionRelationType = versionRelationType;
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

    public int CompareTo(IRelationship? other)
    {
        // Will be greater null everytime
        if (other is null)
        {
            return 1;
        }

        // The comparation will be based on package's name, then the type of the relationship (just use the type name for comparation)
        int compareResult;

        if (other is GrouppingRelationships grouppingRelationships)
        {
            var relationships = grouppingRelationships.Relationships;

            // Empty groupping relationship is equivalent to the null

            if (relationships.Length == 0)
            {
                return 1;
            }

            compareResult = CompareTo(grouppingRelationships.Relationships[0]);

            return compareResult != 0 ? compareResult : -1;
        }
        else if (other is PackageRelationship packageRelationship)
        {
            compareResult = Package.CompareTo(packageRelationship.Package);

            if (compareResult != 0)
            {
                return compareResult;
            }

            if (RelationType.HasValue && packageRelationship.RelationType.HasValue)
            {
                compareResult = RelationType.Value.CompareTo(packageRelationship.RelationType.Value);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                return Version is not null ? Version.CompareTo(packageRelationship.Version) : packageRelationship.Version is not null ? -1 : 0;
            }
            else if (RelationType.HasValue || packageRelationship.RelationType.HasValue)
            {
                return RelationType.HasValue ? 1 : -1;
            }
        }

        return 1;
    }

    public bool Equals(IRelationship? other)
    {
        if (other is not PackageRelationship otherRelationship) {
            return false;
        }

        return Package.Equals(otherRelationship.Package) &&
               RelationType == otherRelationship.RelationType &&
               Version?.Equals(otherRelationship.Version) == true;
    }
}
