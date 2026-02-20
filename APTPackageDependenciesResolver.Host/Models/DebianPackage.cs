using System.Runtime.InteropServices;

namespace APTPackageDependenciesResolver;

public class DebianPackage : IPackage
{
    private List<DebianVirtualPackage>? _provides;

    public string Name { get; set; } = string.Empty;

    public IRelationship? PreDepends { get; set; }

    public IRelationship? Depends { get; set; }

    public IRelationship? Suggests { get; set; }

    public IRelationship? Recommends { get; set; }

    public IRelationship? Provides
    {
        get => field;
        
        set 
        {
            throw new NotImplementedException();
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is IPackage package)
        {
            return ((IEquatable<IPackage>)this).Equals(package);
        }

        return false;
    }

    internal void AddProvidesPackage(DebianVirtualPackage package)
    {
        _provides ??= new List<DebianVirtualPackage>();
        _provides.Add(package);

        package.AddProviderPackage(this);
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

        return Name.CompareTo(other.Name);
    }

    public bool Equals(IPackage? other)
    {
        if (other is not DebianPackage otherPackage)
        {
            return false;
        }

        return Name.Equals(otherPackage.Name);
    }
}