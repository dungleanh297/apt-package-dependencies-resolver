namespace APTPackageDependenciesResolver;

public static class FieldNameValidator
{
    public static void ThrowOnInvalid(ReadOnlySpan<char> fieldName)
    {
        if (fieldName[0] == '-' || fieldName[0] == '#')
        {
            throw new FormatException("Field name cannot begin with a hyphen ('-') or a hash ('#')");
        }

        // The conversion is allow more than just only ASCII character, including some special characters
        // But we're rarely encounter those characters in the field name, and we want to make sure the field name is valid, so we only allow ASCII character, number, and hyphen
        foreach (var character in fieldName)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character != '-')
            {
                throw new FormatException("Field name must contain only number, alpha character, and hyphen");
            }
        }
    }
}