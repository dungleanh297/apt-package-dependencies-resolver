using APTPackageDependenciesResolver.Host.Models;

namespace APTPackageDependenciesResolver;

public sealed class AnyRelationship : GrouppingRelationships
{
    public void Add(PackageRelationship package) => base.Add(package);

    public void Remove(PackageRelationship package) => base.Remove(package);

    public override bool Equals(IRelationship? other)
    {
        if (other is not AnyRelationship otherRelationship)
        {
            return false;
        }

        return Relationships.SequenceEqual(otherRelationship.Relationships);
    }

}