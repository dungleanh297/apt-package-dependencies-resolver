namespace APTPackageDependenciesResolver;

public interface IPackage : IComparable<IPackage>, IEquatable<IPackage>
{
    string Name { get; }
}
