namespace APTPackageDependenciesResolver;

[TestClass]
public sealed class StanzaFieldNameValueEnumeratorTest
{
    private static Dictionary<string, string> EnumerateStanzaToDictionary(string stanza)
    {
        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(stanza))
        {
            nameValuePairs.Add(
                key: new string(stanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(stanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        return nameValuePairs;
    }

    private static void AssertNameValuePair(Dictionary<string, string> actualNameValuePairs, params (string, string)[] expectedPairs)
    {
        Assert.HasCount(expectedPairs.Length, actualNameValuePairs);

        foreach (var expectedPair in expectedPairs.Select(p => new KeyValuePair<string, string>(p.Item1, p.Item2)))
        {
            Assert.Contains(expectedPair, actualNameValuePairs);
        }
    }

    [TestMethod]
    public void SingleSimpleField(string stanza, string expectedName, string expectedValue)
    {
        const string SampleStanza = "Package: 7z";

        Dictionary<string, string> nameValuePairs = EnumerateStanzaToDictionary(SampleStanza);
    }

    [TestMethod]
    public void SimpleFieldsOnly(string stanza, params string[][] expectedResults)
    {
        const string SampleStanza =
        """
        Package: docker-desktop
        Maintainer: Docker Inc.
        Architecture: amd64
        Version: 4.59.0-217644
        Depends: curl, qemu-system-x86 (>= 5.2.0), docker-ce-cli, libseccomp2, libcap-ng0, pass, desktop-file-utils, libgtk-3-0, libx11-xcb1, uidmap
        Pre-Depends: init-system-helpers (>= 1.54~)
        Description: Docker Desktop is an easy-to-install application that enables you to locally build and share containerized applications and microservices.
        """;

        Dictionary<string, string> nameValuePairs = EnumerateStanzaToDictionary(SampleStanza);

        AssertNameValuePair(nameValuePairs,
            ("Package", "docker-desktop"),
            ("Maintainer", "Docker Inc."),
            ("Architecture", "amd64"),
            ("Version", "4.59.0-217644"),
            ("Depends", "curl, qemu-system-x86 (>= 5.2.0), docker-ce-cli, libseccomp2, libcap-ng0, pass, desktop-file-utils, libgtk-3-0, libx11-xcb1, uidmap"),
            ("Pre-Depends", "init-system-helpers (>= 1.54~)"),
            ("Description", "Docker Desktop is an easy-to-install application that enables you to locally build and share containerized applications and microservices.")
        );
    }

    [TestMethod]
    public void MultilineFieldAtTheEndOfStanza()
    {
        const string SampleStanza =
        $"""
        Package: ca-certificates-java
        Priority: optional
        Installed-Size: 42
        Breaks: openjdk-11-jre-headless (<< 11.0.19+7~1~), openjdk-17-jre-headless (<< 17.0.8~6-3~), openjdk-18-jre-headless (<< 18.0.2+9-2ubuntu1~), openjdk-19-jre-headless (<< 19.0.2+7-0ubuntu4~), openjdk-20-jre-headless (<< 20.0.1+9~1~), openjdk-21-jre-headless (<< 21~9ea-1~), openjdk-8-jre-headless (<< 8u382~b04-2~)
        Homepage: https://java.com
        Description: Common CA certificates (JKS keystore)
         This package uses the hooks of the ca-certificates package to update the
         cacerts JKS keystore used for many java runtimes.
        """;

        Dictionary<string, string> nameValuePairs = EnumerateStanzaToDictionary(SampleStanza);

        AssertNameValuePair(nameValuePairs,
            ("Package", "ca-certificates-java"),
            ("Priority", "optional"),
            ("Installed-Size", "42"),
            ("Breaks", "openjdk-11-jre-headless (<< 11.0.19+7~1~), openjdk-17-jre-headless (<< 17.0.8~6-3~), openjdk-18-jre-headless (<< 18.0.2+9-2ubuntu1~), openjdk-19-jre-headless (<< 19.0.2+7-0ubuntu4~), openjdk-20-jre-headless (<< 20.0.1+9~1~), openjdk-21-jre-headless (<< 21~9ea-1~), openjdk-8-jre-headless (<< 8u382~b04-2~)"),
            ("Homepage", "https://java.com"),
            ("Description", """
                            Common CA certificates (JKS keystore)
                             This package uses the hooks of the ca-certificates package to update the
                             cacerts JKS keystore used for many java runtimes.
                            """)
        );
    }

    [TestMethod]
    public void MultilineFieldInTheMiddle()
    {
        const string SampleStanza =
        """
        Package: test-package
        Description: First line of description
         This is a continuation line
         Another continuation line
        Version: 1.0.0
        Maintainer: John Doe
        """;

        Dictionary<string, string> nameValuePairs = EnumerateStanzaToDictionary(SampleStanza);

        AssertNameValuePair(nameValuePairs,
            ("Package", "test-package"),
            ("Description", """
                            First line of description
                             This is a continuation line
                             Another continuation line
                            """),
            ("Version", "1.0.0"),
            ("Maintainer", "John Doe")
        );
    }

    [TestMethod]
    public void TabCharacterForLineContinuation()
    {
        string SampleStanza =
        $"""
        Package: test-package
        Description: Main description
        {'\t'}Continuation with tab
        {'\t'}Another tab continuation
        """;

        Dictionary<string, string> nameValuePairs = EnumerateStanzaToDictionary(SampleStanza);

        AssertNameValuePair(nameValuePairs,
            ("Package", "test-package"),
            ("Description", $"""
                            Main description
                            {'\t'}Continuation with tab
                            {'\t'}Another tab continuation
                            """)
        );
    }

    [TestMethod]
    public void ConsecutiveMultilineFields()
    {
        string SampleStanza =
        $"""
        Name: pkg
        Description: First multiline
         Continuation of first
        Depends: lib1, lib2,
        {'\t'}lib3, lib4
        Recommends: opt1,
         opt2,
         opt3
        """;

        Dictionary<string, string> nameValuePairs = EnumerateStanzaToDictionary(SampleStanza);

        AssertNameValuePair(nameValuePairs,
            ("Name", "pkg"),
            ("Description", "First multiline\n Continuation of first"),
            ("Depends", "lib1, lib2,\n\tlib3, lib4"),
            ("Recommends", "opt1,\n opt2,\n opt3")
        );
    }

    [TestMethod]
    public void MixedComplexScenario()
    {
        const string SampleStanza =
        """
        Package: golang-github-mitchellh-mapstructure-dev
        Source: golang-github-mitchellh-mapstructure
        Version: 1.5.0-1
        Architecture: all
        Maintainer: Debian Go Packaging Team <team+go@tracker.debian.org>
        Installed-Size: 42
        Depends: golang-github-mitchellh-reflectwalk-dev
        Homepage: https://github.com/mitchellh/mapstructure
        Description: Go library for decoding generic map values
         mapstructure is a Go library for decoding generic map values
         to structures and vice versa, while providing
         helpful error handling.
        """;

        Dictionary<string, string> nameValuePairs = EnumerateStanzaToDictionary(SampleStanza);

        AssertNameValuePair(nameValuePairs,
            ("Package", "golang-github-mitchellh-mapstructure-dev"),
            ("Source", "golang-github-mitchellh-mapstructure"),
            ("Version", "1.5.0-1"),
            ("Architecture", "all"),
            ("Maintainer", "Debian Go Packaging Team <team+go@tracker.debian.org>"),
            ("Installed-Size", "42"),
            ("Depends", "golang-github-mitchellh-reflectwalk-dev"),
            ("Homepage", "https://github.com/mitchellh/mapstructure"),
            ("Description", """
                            Go library for decoding generic map values
                             mapstructure is a Go library for decoding generic map values
                             to structures and vice versa, while providing
                             helpful error handling.
                            """)
        );
    }

    [TestMethod]
    public void ThrowsOnMissingFieldName()
    {
        const string SampleStanza = ": no field name\n";
        Assert.Throws<FormatException>(() => new StanzaFieldNameValueEnumerator(SampleStanza).MoveNext(), "Exception should be thrown when field name is missing.");
    }

    [TestMethod]
    public void ThrowsOnMissingColon()
    {
        const string SampleStanza = "PackageWithoutColon value\n";
        Assert.Throws<FormatException>(() => new StanzaFieldNameValueEnumerator(SampleStanza).MoveNext(), "Exception should be thrown when encountering missing colon.");
    }

    private static void AssertFieldValuePair(string[] expectedNameValuePair, string actualName, string actualValue)
    {
        Assert.AreEqual(expectedNameValuePair[0], actualName, "Incorrect field name");
        Assert.AreEqual(expectedNameValuePair[1], actualValue, "Incorrect field value");
    }
}
