using System.Runtime.InteropServices;

namespace APTPackageDependenciesResolver.Host.Models;

public abstract class GrouppingRelationships : IRelationship, IComparable<IRelationship>
{
    private List<IRelationship>? _relationships;

    public ReadOnlySpan<IRelationship> Relationships => CollectionsMarshal.AsSpan(_relationships);

    public abstract bool Equals(IRelationship? relationship);

    protected void Add(IRelationship relationship)
    {
        _relationships ??= [];
        _relationships.BinaryInsert(relationship);
    }

    protected void Remove(IRelationship relationship)
    {
        _relationships ??= [];
        _relationships.BinaryRemove(relationship);
    }

    public int CompareTo(IRelationship? other)
    {
        ReadOnlySpan<IRelationship> relationships = Relationships;

        if (other is null)
        {
            return 1;
        }

        int compareResult;

        if (other is GrouppingRelationships grouppingRelationships)
        {
            compareResult = relationships.SequenceCompareTo(grouppingRelationships.Relationships);
            return compareResult != 0 ? compareResult : GetType().Name.CompareTo(other.GetType().Name);
        }

        if (relationships.Length == 0)
        {
            return -1;
        }

        compareResult = relationships[0].CompareTo(other);

        return compareResult != 0 ? compareResult : relationships[0].GetType().Name.CompareTo(other.GetType().Name);
    }
}
