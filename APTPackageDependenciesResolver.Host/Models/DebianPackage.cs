using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace APTPackageDependenciesResolver;

public sealed class DebianPackage : IPackage
{
    private List<PackageRelationship>? _provides;

    public required string Name { get; set; }

    public IRelationship? PreDepends { get; set; }

    public IRelationship? Depends { get; set; }

    public IRelationship? Suggests { get; set; }

    public IRelationship? Recommends { get; set; }

    public ReadOnlySpan<PackageRelationship> Provides => CollectionsMarshal.AsSpan(_provides ??= new List<PackageRelationship>());

    public override bool Equals(object? obj)
    {
        if (obj is IPackage package)
        {
            return ((IEquatable<IPackage>)this).Equals(package);
        }

        return false;
    }

    public void UpdateProvidesRelationship(IRelationship? relationship)
    {
        List<PackageRelationship>? oldProvides = _provides;

        if (relationship is MultipleRelationships grouppingRelationships)
        {
            ReadOnlySpan<IRelationship> relationships = grouppingRelationships.Relationships;
            List<IRelationship> newRelationships = [.. relationships];

            foreach (var subRelationship in newRelationships)
            {
                if (subRelationship is not PackageRelationship packageRelationship || !IsValidPackageRelationship(packageRelationship))
                {
                    throw new ArgumentException("Invalid relationship for Provides field. Only relationships with DebianVirtualPackage and optionally with an exact version are allowed.", nameof(relationship));
                }
            }

            DetachOldProvides(oldProvides);
            _provides = oldProvides;
        }
        else if (relationship is PackageRelationship packageRelationship)
        {
            if (!IsValidPackageRelationship(packageRelationship))
            {
                throw new ArgumentException("Invalid relationship for Provides field. Only relationships with DebianVirtualPackage and optionally with an exact version are allowed.", nameof(relationship));
            }

            if (_provides is null)
            {
                _provides = [];
            }
            else
            {
                DetachOldProvides(_provides);
                _provides.Clear();
            }

            _provides.Add(packageRelationship);

            Unsafe.As<DebianVirtualPackage>(packageRelationship.Package).AddProviderPackage(this);
        }
        else
        {
            throw new ArgumentException("Invalid relationship type for Provides field. Only PackageRelationship or MultipleRelationships are allowed.", nameof(relationship));
        }

    }

    private void DetachOldProvides(List<PackageRelationship>? oldProvides)
    {
        if (oldProvides is null)
        {
            return;
        }

        foreach (var packageRelationship in oldProvides)
        {
            Unsafe.As<DebianVirtualPackage>(packageRelationship.Package).RemoveProviderPackage(this);
        }
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public int CompareTo(IPackage? other)
    {
        if (other is null)
        {
            return -1;
        }

        int comparationResult = Name.CompareTo(other.Name);
        return comparationResult != 0 ? comparationResult : GetType().Name.CompareTo(other.GetType().Name);
    }

    public bool Equals(IPackage? other)
    {
        if (other is not DebianPackage otherPackage)
        {
            return false;
        }

        return Name.Equals(otherPackage.Name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidPackageRelationship(PackageRelationship relationship)
    {
        return relationship.Package is DebianVirtualPackage && (!relationship.RelationType.HasValue || relationship.RelationType.Value == VersionRelationType.ExactlyEqual);
    }
}