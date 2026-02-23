using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace APTPackageDependenciesResolver;

public static class DebianPackageParser
{
    public static DebianPackage Parse(string packageName, Range stanzaRange, DebianPackageParsingContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageName);

        int setProperties = 0;
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
                        ThrowOnDuplicateField(context, nameValuePair.Name);
                    }
                    setProperties |= 0x1;
                    package.Depends = RelationshipParser.Parse(fieldValue, context);
                    break;

                case "Pre-Depends":
                    if ((setProperties | 0x2) != 0)
                    {
                        ThrowOnDuplicateField(context, nameValuePair.Name);
                    }
                    setProperties |= 0x2;
                    package.PreDepends = RelationshipParser.Parse(fieldValue, context);
                    break;

                case "Provides":
                    if ((setProperties | 0x4) != 0)
                    {
                        ThrowOnDuplicateField(context, nameValuePair.Name);
                    }
                    setProperties |= 0x4;
                    package.UpdateProvidesRelationship(RelationshipParser.Parse(fieldValue, context));
                    break;

                case "Recommends":
                    if ((setProperties | 0x8) != 0)
                    {
                        ThrowOnDuplicateField(context, nameValuePair.Name);
                    }
                    setProperties |= 0x8;
                    package.Recommends = RelationshipParser.Parse(fieldValue, context);
                    break;

                case "Suggests":
                    if ((setProperties | 0x10) != 0)
                    {
                        ThrowOnDuplicateField(context, nameValuePair.Name);
                    }
                    setProperties |= 0x10;
                    package.Suggests = RelationshipParser.Parse(fieldValue, context);
                    break;
            }

            if (setProperties >= 0x1F)
            {
                break;
            }
        }

        return package;
    }

    [StackTraceHidden]
    [DebuggerStepThrough]
    [DoesNotReturn]
    private static void ThrowOnDuplicateField(DebianPackageParsingContext context, Range stanzaRange)
    {
        ThrowHelper.ThrowInvalidFormat(context.ControlData, context.ControlData.AsSpan(stanzaRange), "This field has been appeared more than once");
    }
}
