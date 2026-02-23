using System.Runtime.CompilerServices;
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

            if (packageRelationshipRange.Start.Value == seperatorIndex)
            {
                ThrowHelper.ThrowInvalidFormat(context.ControlData, relationshipsAsString, "Relationship cannot contains empty relationship");
            }

            packageRelationship = ParsePackageRelationship(TrimMultilineValue(relationshipsAsString.Slice(packageRelationshipRange)), context);

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

    private static ReadOnlySpan<char> TrimMultilineValue(in ReadOnlySpan<char> relationship)
    {
        int startIndex = 0;
        int endIndex = relationship.Length;
        int limit = endIndex - 2;
        char firstCharacter, secondCharacter;

        while (startIndex <= limit)
        {
            secondCharacter = relationship[startIndex + 1];
            firstCharacter = relationship[startIndex];

            if (firstCharacter == '\n' && (secondCharacter == ' ' | secondCharacter == '\t'))
            {
                startIndex += 2;
            }
            else
            {
                break;
            }
        }

        limit = startIndex + 2;
        while (endIndex >= limit)
        {
            firstCharacter = relationship[endIndex - 2];
            secondCharacter = relationship[endIndex - 1];

            if (firstCharacter == '\n' && (secondCharacter == ' ' | secondCharacter == '\t'))
            {
                endIndex -= 2;
            }
            else
            {
                break;
            }
        }

        return relationship.Slice(startIndex, endIndex);
    }

    private static PackageRelationship ParsePackageRelationship(in ReadOnlySpan<char> packageRelation, DebianPackageParsingContext context)
    {
        int versionRelationStartIndex = packageRelation.IndexOf('(');
        VersionRelationType relationType = default;
        DebianPackageVersion? version = null;
        ReadOnlySpan<char> packageName;

        if (versionRelationStartIndex >= 0)
        {
            int versionRelationEndIndex = versionRelationStartIndex + packageRelation[versionRelationStartIndex..].IndexOf(')');

            if (versionRelationEndIndex < versionRelationStartIndex)
            {
                ThrowHelper.ThrowInvalidFormat(context.ControlData, packageRelation, $"Version relationship must be ended with enclosed parenthesis.");
            }

            relationType = ParseVersionRelation(packageRelation[(versionRelationStartIndex + 1)..versionRelationEndIndex], out var versionAsString, context);
            version = DebianPackageVersion.Parse(versionAsString);
            packageName = packageRelation[..versionRelationStartIndex].Trim();
        }
        else
        {
            packageName = packageRelation.Trim();
        }

        IPackage package = context.GetPackageByName(new string(packageName));

        return version is null ? new(package) : new(package, relationType, version);
    }

    private static VersionRelationType ParseVersionRelation(in ReadOnlySpan<char> versionRelation, out ReadOnlySpan<char> version, DebianPackageParsingContext context)
    {
        // Space character is used to seperate between relation type and version text, this is not required so we need to check for both cases.
        int seperatorIndex;
        ReadOnlySpan<char> relationTypeAsString = default;
        version = default;

        for (seperatorIndex = 0; seperatorIndex < versionRelation.Length; ++seperatorIndex)
        {
            char character = versionRelation[seperatorIndex];

            if (DebianPackageVersionValidator.IsValidPrefixCharacter(character))
            {
                relationTypeAsString = versionRelation[0..seperatorIndex].Trim();
                version = versionRelation[seperatorIndex..].Trim();
                break;
            }
        }

        if (seperatorIndex == versionRelation.Length)
        {
            ThrowHelper.ThrowInvalidFormat(context.ControlData, versionRelation, "Version relation not followed with a valid version");
        }

        switch (relationTypeAsString)
        {
            case "":
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
