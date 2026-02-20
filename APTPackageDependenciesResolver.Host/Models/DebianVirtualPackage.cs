using System.Runtime.InteropServices;

namespace APTPackageDependenciesResolver;

public record class DebianVirtualPackage(string Name) : IPackage, IEquatable<DebianVirtualPackage>
{
    private List<DebianPackage>? _providers;

    public ReadOnlySpan<DebianPackage> Providers
    {
        get
        {
            if (_providers is null)
            {
                return default;
            }

            return CollectionsMarshal.AsSpan(_providers);
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

        return Name.CompareTo(other.Name);
    }

    public bool Equals(IPackage? other)
    {
        if (other is not DebianVirtualPackage otherPackage)
        {
            return false;
        }

        return Equals(otherPackage);
    }

    internal void AddProviderPackage(DebianPackage package)
    {
        _providers ??= new List<DebianPackage>();

        int index = _providers.BinarySearch(package);
        
        if (index < 0)
        {
            _providers.Insert(~index, package);
        }
    }

    internal void RemoveProviderPackage(DebianPackage package)
    {
        if (_providers is null)
        {
            return;
        }

        int index = _providers.BinarySearch(package);

        if (index >= 0)
        {
            _providers.RemoveAt(index);
        }
    }
}
