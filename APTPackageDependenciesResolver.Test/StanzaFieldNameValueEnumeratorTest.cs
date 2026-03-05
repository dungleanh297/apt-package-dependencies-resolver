namespace APTPackageDependenciesResolver;

[TestClass]
public sealed class StanzaFieldNameValueEnumeratorTest
{
    private static void AssertEnumerator(string stanza, StanzaFieldNameValueEnumerator enumerator, params string[][] expectedPairs)
    {
        int i = 0;
        
        while (enumerator.MoveNext() && i < expectedPairs.Length)
        {
            StanzaFieldNameValuePair current = enumerator.Current;
            (int nameStart, int nameLength) = current.Name.GetOffsetAndLength(stanza.Length);
            (int valueStart, int valueLength) = current.Value.GetOffsetAndLength(stanza.Length);
            if (!stanza.AsSpan().Slice(nameStart, nameLength).SequenceEqual(expectedPairs[i][0].AsSpan()))
            {
                Assert.Fail($"Mismatch stanza field name. Expected {expectedPairs[i][0]}. Actual {stanza.Substring(nameStart, nameLength)}.");   
            }

            if (!stanza.AsSpan().Slice(valueStart, valueLength).SequenceEqual(expectedPairs[i][1].AsSpan()))
            {
                Assert.Fail($"Mismatch stanza field value. Expected {expectedPairs[i][1]}. Actual {stanza.Substring(valueStart, valueLength)}.");
            }

            ++i;
        }

        while (enumerator.MoveNext())
        {
            i++;
        }
        
        Assert.AreEqual(expectedPairs.Length, i, "Incorrect number of enumerated element.");
    }

    [TestMethod]
    [DataRow("Package:7z", "Package", "7z")]
    [DataRow("Package:7z   ", "Package", "7z")]
    [DataRow("Package:   7z", "Package", "7z")]
    [DataRow("Package:   7z   ", "Package", "7z")]
    [DataRow("  Package:7z", "Package", "7z")]
    [DataRow("Package   :7z", "Package", "7z")]
    [DataRow("   Package   :7z", "Package", "7z")]
    [DataRow("   Package   :   7z   ", "Package", "7z")]
    public void SingleSimpleField(string stanza, string expectedName, string expectedValue)
    {
        var nameValuePairs = new StanzaFieldNameValueEnumerator(stanza);
        AssertEnumerator(stanza, nameValuePairs, [ expectedName, expectedValue ]);
    }

    [TestMethod]
    [DataRow("a: b\nc: d\ne: f", new[] { "a", "b" }, new[] { "c", "d" }, new[] { "e", "f" })]
    public void SimpleFieldsOnly(string stanza, params string[][] expectedResults)
    {
        var nameValuePairs = new StanzaFieldNameValueEnumerator(stanza);
        AssertEnumerator(stanza, nameValuePairs, expectedResults);
    }

    [TestMethod]
    [DataRow("Package: ca-certificates-java\nDescription: Common CA certificates\n This package uses hooks",
     new[] { "Package", "ca-certificates-java" },
     new[] { "Description", "Common CA certificates\n This package uses hooks" })]
    public void MultilineFieldAtTheEndOfStanza(string stanza, params string[][] expectedPairs)
    {
        var enumerator = new StanzaFieldNameValueEnumerator(stanza);
        AssertEnumerator(stanza, enumerator, expectedPairs);
    }

    [TestMethod]
    [DataRow(
    """
    Package: test-package
    Description: First line of description
     This is a continuation line
     Another continuation line
    Version: 1.0.0
    """,
     new[] { "Package", "test-package" },
     new[] { "Description", "First line of description\n This is a continuation line\n Another continuation line" },
     new[] { "Version", "1.0.0" })]
    public void MultilineFieldInTheMiddle(string stanza, params string[][] expectedPairs)
    {
        var enumerator = new StanzaFieldNameValueEnumerator(stanza);
        AssertEnumerator(stanza, enumerator, expectedPairs);
    }

    [TestMethod]
    [DataRow("Package: test-package\nDescription: Main description\n\tContinuation with tab\n\tAnother tab continuation",
     new[] { "Package", "test-package" },
     new[] { "Description", "Main description\n\tContinuation with tab\n\tAnother tab continuation" })]
    public void TabCharacterForLineContinuation(string stanza, params string[][] expectedPairs)
    {
        var enumerator = new StanzaFieldNameValueEnumerator(stanza);
        AssertEnumerator(stanza, enumerator, expectedPairs);
    }

    [TestMethod]
    [DataRow(
    """
    Name: pkg
    Description: First multiline
     Continuation of first
    Depends: lib1, lib2,
    """ + 
    "\n\tlib3, lib4\n" +
    """
    Recommends: opt1,
     opt2,
     opt3
    """,
     new[] { "Name", "pkg" },
     new[] { "Description", "First multiline\n Continuation of first" },
     new[] { "Depends", "lib1, lib2,\n\tlib3, lib4" },
     new[] { "Recommends", "opt1,\n opt2,\n opt3" })]
    public void ConsecutiveMultilineFields(string stanza, params string[][] expectedPairs)
    {
        var enumerator = new StanzaFieldNameValueEnumerator(stanza);
        AssertEnumerator(stanza, enumerator, expectedPairs);
    }

    [TestMethod]
    [DataRow(
    """
    Package: golang-github-mitchellh-mapstructure-dev
    Source: golang-github-mitchellh-mapstructure
    Description: Go library for decoding generic map values
     mapstructure is a Go library for decoding generic map values
    """,
     new[] { "Package", "golang-github-mitchellh-mapstructure-dev" },
     new[] { "Source", "golang-github-mitchellh-mapstructure" },
     new[] { "Description", "Go library for decoding generic map values\n mapstructure is a Go library for decoding generic map values" })]
    public void MixedComplexScenario(string stanza, params string[][] expectedPairs)
    {
        var enumerator = new StanzaFieldNameValueEnumerator(stanza);
        AssertEnumerator(stanza, enumerator, expectedPairs);
    }

    [TestMethod]
    [DataRow(": no field name\n")]
    public void ThrowsOnMissingFieldName(string stanza)
    {
        Assert.Throws<FormatException>(() => new StanzaFieldNameValueEnumerator(stanza).MoveNext(),
            "Exception should be thrown when field name is missing.");
    }

    [TestMethod]
    [DataRow("PackageWithoutColon value\n")]
    public void ThrowsOnMissingColon(string stanza)
    {
        Assert.Throws<FormatException>(() => new StanzaFieldNameValueEnumerator(stanza).MoveNext(),
            "Exception should be thrown when encountering missing colon.");
    }

    private static void AssertFieldValuePair(string[] expectedNameValuePair, string actualName, string actualValue)
    {
        Assert.AreEqual(expectedNameValuePair[0], actualName, "Incorrect field name");
        Assert.AreEqual(expectedNameValuePair[1], actualValue, "Incorrect field value");
    }
}
