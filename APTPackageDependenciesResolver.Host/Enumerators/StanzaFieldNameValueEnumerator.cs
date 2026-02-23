namespace APTPackageDependenciesResolver;

public ref struct StanzaFieldNameValueEnumerator
{
    private readonly ReadOnlySpan<char> _stanza;
    private StanzaFieldNameValuePair _current = default;
    private int _startIndex = 0;

    public StanzaFieldNameValueEnumerator(ReadOnlySpan<char> stanza)
    {
        _stanza = stanza;
    }

    public StanzaFieldNameValuePair Current => _current;

    public bool MoveNext()
    {
        if (_startIndex >= _stanza.Length)
        {
            return false;
        }

        int nameStartIndex = _startIndex, nameEndIndex;
        int seperatorIndex;
        int valueStartIndex, valueEndIndex;

        // Colon not only appearing as a field name and value seperator, but also be used in Description, Homepage (as a URL address), Version, etc...
        // We should find the new line first, then we can confidently know that the first colon is the seperator between name and value.
        // Multiple line and folded value will be discovered later.
        valueEndIndex = nameStartIndex + 1 + _stanza[(nameStartIndex + 1)..].IndexOf('\n');

        // Couldn't found the new line, so the value is the rest of the stanza
        if (valueEndIndex < nameStartIndex + 1)
        {
            valueEndIndex = _stanza.Length;
        }

        seperatorIndex = nameStartIndex + _stanza[nameStartIndex..valueEndIndex].IndexOf(':');

        if (seperatorIndex < nameStartIndex)
        {
            throw new FormatException($"Unable to find field seperator of this stanza:\n{new string(_stanza)}");
        }
        else if (seperatorIndex == nameStartIndex)
        {
            throw new FormatException($"This stanza contains a line that doesn't have a valid field name:\n{new string(_stanza)}");
        }

        // Follow the Debian's documentation, value can have multiple line, each new line must begin with either space or tab character.
        // Advance the index of the new line until we find the right one
        while (valueEndIndex <= _stanza.Length - 2 && (_stanza[valueEndIndex + 1] == ' ' || _stanza[valueEndIndex + 1] == '\t'))
        {
            int newFieldValueEndIndex = _stanza[(valueEndIndex + 2)..^0].IndexOf('\n');

            // Can't found the new line anymore, that means the multiline value is the rest of the stanza, starting from the valueStartIndex, break out of the loop.
            // And we also reached to the end of this stanza.
            if (newFieldValueEndIndex < 0)
            {
                valueEndIndex = _stanza.Length;
                break;
            }

            valueEndIndex += 2 + newFieldValueEndIndex;
        }

        nameEndIndex = nameStartIndex + _stanza[nameStartIndex..seperatorIndex].TrimEnd().Length;

        FieldNameValidator.ThrowOnInvalid(_stanza[nameStartIndex..nameEndIndex]);

        // The shorthand version of:
        // valueStartIndex = seperatorIndex + 1 + (valueEndIndex - (seperatorIndex + 1) - _stanza[(seperatorIndex + 1)..valueEndIndex].TrimStart().Length);
        // Which:
        //   seperatorIndex + 1: The index of character after the seperator
        //   valueEndIndex - (seperatorIndex + 1): The length of field name before trimming
        //   _stanza[(seperatorIndex + 1)..valueEndIndex].TrimStart().Length: The final length after trimming

        valueStartIndex = valueEndIndex - _stanza[(seperatorIndex + 1)..valueEndIndex].TrimStart().Length;

        // Update the new state of the enumerator
        // Skip the new line character
        _startIndex = valueEndIndex + 1;

        _current = new StanzaFieldNameValuePair
        {
            Name = new Range(new Index(nameStartIndex), new Index(nameEndIndex)),
            Value = new Range(new Index(valueStartIndex), new Index(valueEndIndex))
        };

        return true;
    }

    public StanzaFieldNameValueEnumerator GetEnumerator()
    {
        return this;
    }
}

public readonly record struct StanzaFieldNameValuePair(Range Name, Range Value);
