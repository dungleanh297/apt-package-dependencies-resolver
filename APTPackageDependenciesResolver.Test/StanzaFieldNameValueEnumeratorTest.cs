namespace APTPackageDependenciesResolver;

[TestClass]
public sealed class StanzaFieldNameValueEnumeratorTest
{
    [TestMethod]
    public void SingleSimpleField()
    {
        const string SampleStanza = "Package: 7z";

        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(SampleStanza))
        {
            nameValuePairs.Add(
                key: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        Assert.HasCount(1, nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Package", "7z"), nameValuePairs);
    }

    [TestMethod]
    public void SimpleFieldsOnly()
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

        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(SampleStanza))
        {
            nameValuePairs.Add(
                key: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        Assert.HasCount(7, nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Package", "docker-desktop"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Maintainer", "Docker Inc."), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Architecture", "amd64"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Version", "4.59.0-217644"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Depends", "curl, qemu-system-x86 (>= 5.2.0), docker-ce-cli, libseccomp2, libcap-ng0, pass, desktop-file-utils, libgtk-3-0, libx11-xcb1, uidmap"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Pre-Depends", "init-system-helpers (>= 1.54~)"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Description", "Docker Desktop is an easy-to-install application that enables you to locally build and share containerized applications and microservices."), nameValuePairs);
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

        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(SampleStanza))
        {
            nameValuePairs.Add(
                key: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        Assert.HasCount(6, nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Package",
            value: "ca-certificates-java"
        ), nameValuePairs);

        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Priority",
            value: "optional"
        ), nameValuePairs);

        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Installed-Size",
            value: "42"
        ), nameValuePairs);

        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Breaks",
            value: "openjdk-11-jre-headless (<< 11.0.19+7~1~), openjdk-17-jre-headless (<< 17.0.8~6-3~), openjdk-18-jre-headless (<< 18.0.2+9-2ubuntu1~), openjdk-19-jre-headless (<< 19.0.2+7-0ubuntu4~), openjdk-20-jre-headless (<< 20.0.1+9~1~), openjdk-21-jre-headless (<< 21~9ea-1~), openjdk-8-jre-headless (<< 8u382~b04-2~)"
        ), nameValuePairs);

        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Homepage",
            value: "https://java.com"
        ), nameValuePairs);

        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Description",
            value: 
            """
            Common CA certificates (JKS keystore)
             This package uses the hooks of the ca-certificates package to update the
             cacerts JKS keystore used for many java runtimes.
            """
        ), nameValuePairs);
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

        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(SampleStanza))
        {
            nameValuePairs.Add(
                key: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        Assert.HasCount(4, nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Package", "test-package"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Description",
            value:
            """
            First line of description
             This is a continuation line
             Another continuation line
            """
        ), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Version", "1.0.0"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Maintainer", "John Doe"), nameValuePairs);
    }

    [TestMethod]
    public void TabCharacterForLineContinuation()
    {
        const string SampleStanza = "Package: test-package\nDescription: Main description\n\tContinuation with tab\n\tAnother tab continuation\n";

        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(SampleStanza))
        {
            nameValuePairs.Add(
                key: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        Assert.HasCount(2, nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Package", "test-package"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Description",
            value: "Main description\n\tContinuation with tab\n\tAnother tab continuation"
        ), nameValuePairs);
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

        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(SampleStanza))
        {
            nameValuePairs.Add(
                key: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        Assert.HasCount(4, nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Description", "First multiline\n Continuation of first"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Depends", "lib1, lib2,\n\tlib3, lib4"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Recommends", "opt1,\n opt2,\n opt3"), nameValuePairs);
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

        Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

        foreach (var nameValueRangePair in new StanzaFieldNameValueEnumerator(SampleStanza))
        {
            nameValuePairs.Add(
                key: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Name)),
                value: new string(SampleStanza.AsSpan().Slice(nameValueRangePair.Value))
            );
        }

        Assert.HasCount(9, nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Package", "golang-github-mitchellh-mapstructure-dev"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>("Homepage", "https://github.com/mitchellh/mapstructure"), nameValuePairs);
        Assert.Contains(new KeyValuePair<string, string>
        (
            key: "Description",
            value:
            """
            Go library for decoding generic map values
             mapstructure is a Go library for decoding generic map values
             to structures and vice versa, while providing
             helpful error handling.
            """
        ), nameValuePairs);
    }

    [TestMethod]
    public void ThrowsOnMissingFieldName()
    {
        const string SampleStanza = ": no field name\n";
        var enumerator = new StanzaFieldNameValueEnumerator(SampleStanza);
        bool exceptionHasBeenThrown = false;

        try
        {
            enumerator.MoveNext();
        }
        catch (FormatException)
        {
            exceptionHasBeenThrown = true;    
        }

        Assert.IsTrue(exceptionHasBeenThrown, "Exception should be thrown when encountering invalid format.");
    }

    [TestMethod]
    public void ThrowsOnMissingColon()
    {
        const string SampleStanza = "PackageWithoutColon value\n";
        var enumerator = new StanzaFieldNameValueEnumerator(SampleStanza);
        bool exceptionHasBeenThrown = false;

        try
        {
            enumerator.MoveNext();
        }
        catch (FormatException)
        {
            exceptionHasBeenThrown = true;
        }

        Assert.IsTrue(exceptionHasBeenThrown, "Exception should be thrown when encountering invalid format.");
    }
}
