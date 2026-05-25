using System.Runtime.CompilerServices;
using APTPackageDependenciesResolver.Host.Models;

namespace APTPackageDependenciesResolver;

public static class RelationshipExtensions
{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_relationships")]
	private static extern ref List<IRelationship> GetRelationshipsAsListInternal(GrouppingRelationships grouppingRelationships);

    public static List<IRelationship> GetRelationshipsAsList(this GrouppingRelationships grouppingRelationships)
    {
        return GetRelationshipsAsListInternal(grouppingRelationships);
    }

}
