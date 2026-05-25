using System.Data;
using System.Runtime.CompilerServices;

namespace APTPackageDependenciesResolver;

[TestClass]
public class DebianPackageParserTest
{
    [TestMethod]
    [DataRow("Package: 7z\nPre-Depends: a", ExpectationProperties.PreDepends)]
    [DataRow("Package: 7z\nDepends: a", ExpectationProperties.Depends)]
    [DataRow("Package: 7z\nRecommends: a", ExpectationProperties.Recommends)]
    [DataRow("Package: 7z\nSuggests: a", ExpectationProperties.Suggests)]
    [DataRow("Package: 7z\nProvides: a", ExpectationProperties.Provides)]
    public void ParseSimplePackage(string stanza, ExpectationProperties expectationProperties)
    {
        const string PackageName = "7z";

        var stanzaRanges = new Dictionary<string, Range>
        {
            { PackageName, new Range(Index.Start, Index.End) }
        };

        var context = new DebianPackageParsingContext(stanza, stanzaRanges, [], []);
        var package = DebianPackageParser.Parse(PackageName, new Range(Index.Start, Index.End), context);
        Assert.AreEqual(PackageName, package.Name, "Package name should not be null");
        AssertPropertyShouldBeNullOrNot(package.PreDepends, (expectationProperties & ExpectationProperties.PreDepends) == 0);
        AssertPropertyShouldBeNullOrNot(package.Depends, (expectationProperties & ExpectationProperties.Depends) == 0);
        AssertPropertyShouldBeNullOrNot(package.Recommends, (expectationProperties & ExpectationProperties.Recommends) == 0);
        AssertPropertyShouldBeNullOrNot(package.Suggests, (expectationProperties & ExpectationProperties.Suggests) == 0);
    }

    private static void AssertPropertyShouldBeNullOrNot(object? value, bool shouldBeNull,
        [CallerArgumentExpression(nameof(value))] string callerExpression = "")
    {
        if (shouldBeNull)
        {
            Assert.IsNull(value, "This property should be null", callerExpression);
        }
        else
        {
            Assert.IsNotNull(value, "This property should be not null", callerExpression);
        }
    }

    [Flags]
    public enum ExpectationProperties
    {
        PreDepends = 1,
        Depends = 2,
        Recommends = 4,
        Suggests = 8,
        Provides = 16,
    }
}
