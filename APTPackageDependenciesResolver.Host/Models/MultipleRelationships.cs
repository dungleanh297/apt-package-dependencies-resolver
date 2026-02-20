namespace APTPackageDependenciesResolver;

public class MultipleRelationships : IRelationship
{
    private readonly List<IRelationship> _conditions = [];

    public bool Add(IRelationship condition)
    {
        return _conditions.BinaryInsert(condition);
    }

    public bool Remove(IRelationship condition)
    {
        return _conditions.BinaryRemove(condition);
    }
}
