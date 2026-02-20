namespace APTPackageDependenciesResolver;

public class AnyRelationship : IRelationship
{
    private readonly List<IRelationship> _packages = [];
    
    public bool Add(IRelationship package)
    {
        return _packages.BinaryInsert(package);
    }

    public bool Remove(IRelationship package)
    {
        return _packages.BinaryRemove(package);
    }
}