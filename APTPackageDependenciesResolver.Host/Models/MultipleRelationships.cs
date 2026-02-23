using APTPackageDependenciesResolver.Host.Models;

namespace APTPackageDependenciesResolver;

public sealed class MultipleRelationships : GrouppingRelationships, IRelationship
{
    public void Add(AnyRelationship anyRelationship) => base.Add(anyRelationship);

    public void Remove(AnyRelationship anyRelationship) => base.Remove(anyRelationship);

    public void Add(PackageRelationship packageRelationship) => base.Add(packageRelationship);

    public void Remove(PackageRelationship packageRelationship) => base.Remove(packageRelationship);

    public override bool Equals(IRelationship? other)
    {
        if (other is not MultipleRelationships otherRelationship)
        {
            return false;
        }

        return Relationships.SequenceEqual(otherRelationship.Relationships);
    }
}
