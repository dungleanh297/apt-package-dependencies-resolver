using System.Dynamic;

namespace APTPackageDependenciesResolver;

public static class DebianPackageInformationParser
{
    public static DebianPackage Parse(string packageName, Range stanzaRange, DebianPackageParsingContext context)
    {
        const int MaximumPropertiesCount = 5;

        int setPropertiesCount = 0;
        uint setProperties = 0;
        ReadOnlySpan<char> stanza = context.ControlData.AsSpan().Slice(stanzaRange);

        DebianPackage package = new DebianPackage()
        {
            Name = packageName,
        };

        // Add the package to the context before parsing relationships to handle circular dependencies
        context.Packages.Add(packageName, package);

        foreach (var nameValuePair in new StanzaFieldNameValueEnumerator(stanza))
        {
            var fieldName = stanza.Slice(nameValuePair.Name);
            var fieldValue = stanza.Slice(nameValuePair.Value);

            switch (fieldName)
            {
                case "Depends":
                    if ((setProperties | 0x1) != 0)
                    {
                        goto ThrowOnDuplicateField;
                    }
                    package.Depends = RelationshipParser.Parse(fieldValue, context);
                    goto IncreaseAndCheckTheCount;

                case "Pre-Depends":
                    if ((setProperties | 0x2) != 0)
                    {
                        goto ThrowOnDuplicateField;
                    }
                    package.PreDepends = RelationshipParser.Parse(fieldValue, context);
                    goto IncreaseAndCheckTheCount;

                case "Provides":
                    if ((setProperties | 0x4) != 0)
                    {
                        goto ThrowOnDuplicateField;
                    }
                    package.Provides = RelationshipParser.Parse(fieldValue, context);
                    goto IncreaseAndCheckTheCount;

                case "Recommends":
                    if ((setProperties | 0x8) != 0)
                    {
                        goto ThrowOnDuplicateField;
                    }
                    package.Recommends = RelationshipParser.Parse(fieldValue, context);
                    goto IncreaseAndCheckTheCount;

                case "Suggests":
                    if ((setProperties | 0x10) != 0)
                    {
                        goto ThrowOnDuplicateField;
                    }
                    package.Suggests = RelationshipParser.Parse(fieldValue, context);
                    goto IncreaseAndCheckTheCount;

                default:
                    continue;
            }

        IncreaseAndCheckTheCount:
            ++setPropertiesCount;

            if (setPropertiesCount >= MaximumPropertiesCount)
            {
                break;
            }
        }    

        return package;

    ThrowOnDuplicateField:
        ThrowHelper.ThrowInvalidFormat(context.ControlData, context.ControlData.AsSpan(stanzaRange), "This field has been appeared more than once");
        return null!;
    }
}
