using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace APTPackageDependenciesResolver;

public sealed class DebianPackage : IPackage
{
    private List<PackageRelationship> _providesRelationship = [];

    public required string Name { get; set; }

    public IRelationship? PreDepends { get; set; }

    public IRelationship? Depends { get; set; }

    public IRelationship? Suggests { get; set; }

    public IRelationship? Recommends { get; set; }

    public ReadOnlySpan<PackageRelationship> Provides => CollectionsMarshal.AsSpan(_providesRelationship);

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

            DetachAllProvidesRelationship();

            foreach (var subRelationship in newRelationships)
            {
                AttachProvidesRelationship(Unsafe.As<PackageRelationship>(subRelationship));
            }

        }
        else if (relationship is PackageRelationship packageRelationship)
        {
            if (!IsValidPackageRelationship(packageRelationship))
            {
                throw new ArgumentException("Invalid relationship for Provides field. Only relationships with DebianVirtualPackage and optionally with an exact version are allowed.", nameof(relationship));
            }

            DetachAllProvidesRelationship();
            AttachProvidesRelationship(packageRelationship);
        }
        else
        {
            throw new ArgumentException("Invalid relationship type for Provides field. Only PackageRelationship or MultipleRelationships are allowed.", nameof(relationship));
        }

    }

    private void AttachProvidesRelationship(PackageRelationship packageRelationship)
    {
        /*

        Sometimes there's package out there with the declaration like this:
          Package: antigravity
          Replaces: antigravity
          Provides: antigravity
          Conflicts: antigravity

          In that case, just completely ignore them.
        */
        if (packageRelationship.Package is not DebianVirtualPackage virtualPackage)
        {
            return;
        }

        virtualPackage.AddProviderPackage(this);
    }

    private void DetachAllProvidesRelationship()
    {
        if (_providesRelationship is null)
        {
            return;
        }

        foreach (var packageRelationship in _providesRelationship)
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
        return !relationship.RelationType.HasValue || relationship.RelationType.Value == VersionRelationType.ExactlyEqual;
    }
}