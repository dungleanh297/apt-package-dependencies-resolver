using System.Runtime.CompilerServices;
using System.Xml;
using APTPackageDependenciesResolver.Host.Extensions;

namespace APTPackageDependenciesResolver;

public static class RelationshipParser
{
    public static IRelationship Parse(in ReadOnlySpan<char> relationshipsAsString, DebianPackageParsingContext context)
    {
        MultipleRelationships? multipleRelationships = null;
        AnyRelationship? anyRelationship = null;
        PackageRelationship packageRelationship;

        foreach (Range packageRelationshipRange in relationshipsAsString.SplitAny([',', '|']))
        {
            int seperatorIndex = packageRelationshipRange.End.Value;

            packageRelationship = ParsePackageRelationship(UnfoldAndTrim(relationshipsAsString.Slice(packageRelationshipRange), seperatorIndex), context);

            // The last item in the chain of relationship
            if (seperatorIndex == relationshipsAsString.Length)
            {
                if (anyRelationship is not null)
                {
                    anyRelationship.Add(packageRelationship);
                    multipleRelationships?.Add(anyRelationship);
                }
                else if (multipleRelationships is not null)
                {
                    multipleRelationships.Add(packageRelationship);
                }

                return (IRelationship?)multipleRelationships ?? (IRelationship?)anyRelationship ?? packageRelationship;
            }

            char seperator = relationshipsAsString[packageRelationshipRange.End.Value];

            switch (seperator)
            {
                case ',':
                    multipleRelationships ??= new MultipleRelationships();
                    // The last item in any relationship, add it to multiple relationship immediately
                    if (anyRelationship is not null)
                    {
                        anyRelationship.Add(packageRelationship);
                        multipleRelationships.Add(anyRelationship);
                        anyRelationship = null;
                    }
                    else
                    {                    
                        multipleRelationships.Add(packageRelationship);
                    }
                    break;


                case '|':
                    anyRelationship ??= new AnyRelationship();
                    anyRelationship.Add(packageRelationship);
                    break;
            }
        }

        throw new Exception($"Failed to parse relationship: {new string(relationshipsAsString)}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> UnfoldAndTrim(ReadOnlySpan<char> relationship, int seperatorIndex)
    { 
        int newLineIndex = relationship.LastIndexOf('\n');
        
        if (newLineIndex < 0)
        {
            return relationship;
        }

        return relationship[Math.Min(newLineIndex, seperatorIndex)..].Trim();
    }

    private static PackageRelationship ParsePackageRelationship(in ReadOnlySpan<char> packageRelation, DebianPackageParsingContext context)
    {
        int versionSpecificStartIndex = packageRelation.IndexOf('(');
        VersionRelationType relationType = default;
        DebianPackageVersion? version = null;
        ReadOnlySpan<char> packageName;
        
        if (versionSpecificStartIndex >= 0)
        {
            int versionSpecificEndIndex = versionSpecificStartIndex + packageRelation[versionSpecificStartIndex..].IndexOf(')');
            
            if (versionSpecificEndIndex < 0)
            {
                throw new Exception($"Version relationship must be ended with enclosed parenthesis: {new string(packageRelation)}");
            }

            relationType = ParseVersionRelation(packageRelation[(versionSpecificStartIndex + 1)..versionSpecificEndIndex].Trim(), out var versionAsString, context);
            version = DebianPackageVersion.Parse(versionAsString);
            packageName = packageRelation[..versionSpecificStartIndex].Trim();
        }
        else
        {
            packageName = packageRelation.Trim();
        }

        IPackage package = context.GetPackageByName(new string(packageName));
        
        return version is null ? new(package) : new(package, relationType, version);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static VersionRelationType ParseVersionRelation(in ReadOnlySpan<char> versionRelation, out ReadOnlySpan<char> version, DebianPackageParsingContext context)
    {
        int seperatorIndex = versionRelation.IndexOf(' ');
        ReadOnlySpan<char> relationTypeAsString = default;

        version = default;

        if (seperatorIndex < 0)
        {
            for (int i = 0; i < versionRelation.Length; ++i)
            {
                char character = versionRelation[i];
                
                if (DebianPackageVersionValidator.IsValidPrefixCharacter(character))
                {
                    relationTypeAsString = versionRelation[0..i];
                    version = versionRelation[i..];
                }

                if (i == versionRelation.Length)
                {
                    ThrowHelper.ThrowInvalidFormat(context.ControlData, versionRelation, "Version relation not followed with a valid version");
                }
            }
        }
        else
        {
            version = versionRelation[(seperatorIndex + 1)..].Trim();
            relationTypeAsString = versionRelation[0..seperatorIndex];

            if (relationTypeAsString.Length == 0)
            {
                return VersionRelationType.ExactlyEqual;
            }
        }

        switch (relationTypeAsString)
        {
            case "=":
                return VersionRelationType.ExactlyEqual;

            case "<<":
                return VersionRelationType.StrictlyEarlier;

            case "<=":
                return VersionRelationType.EarlierOrEqual;

            case ">=":
                return VersionRelationType.LaterOrEqual;

            case ">>":
                return VersionRelationType.StrictlyLater;

            default:
                ThrowHelper.ThrowInvalidFormat(context.ControlData, relationTypeAsString, "Unknown version relation type.");
                return default;
        }
    }
}
