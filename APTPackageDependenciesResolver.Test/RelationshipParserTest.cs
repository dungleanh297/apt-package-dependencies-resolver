using System.Runtime.CompilerServices;
using APTPackageDependenciesResolver.Host.Models;

namespace APTPackageDependenciesResolver;

[TestClass]
public partial class RelationshipParserTest
{
	[TestMethod]
	[DataRow("7zip", "7zip")]
	[DataRow("   chromium", "chromium")]
	[DataRow("mint-x-theme   ", "mint-x-theme")]
	[DataRow("  papirus-icons  ", "papirus-icons")]
	public void Parse_SinglePackageWithNoVersionSpecification(string relationship, string expectedPackageName)
	{
		var context = CreateContext(relationship, expectedPackageName);
		var packageRelationship = Assert.IsInstanceOfType<PackageRelationship>(RelationshipParser.Parse(relationship.AsSpan(), context), $"The returned value's type must be a {nameof(PackageRelationship)}");
		var constaintPackage = Assert.IsInstanceOfType<DebianPackage>(packageRelationship.Package, RelationshipMustBePackageRelationship);

		Assert.AreEqual(expectedPackageName, constaintPackage.Name, IncorrectPackageNameInPackageRelation);
		Assert.IsNull(packageRelationship.Version, "No version has been specificed, Version property should be null");
		Assert.IsNull(packageRelationship.RelationType, "The relation type should also be null when version is null");
	}

	[TestMethod]
	[DataRow("git (<< 1:2.43.0-1ubuntu7.3)", "git", "1:2.43.0-1ubuntu7.3", VersionRelationType.StrictlyEarlier)]
	[DataRow("7zip (<= 23.01+dfsg-11)", "7zip", "23.01+dfsg-11", VersionRelationType.EarlierOrEqual)]
	[DataRow("pkg (= 1.2.3)", "pkg", "1.2.3", VersionRelationType.ExactlyEqual)]
	[DataRow("chromium (>= 145.0.7632.45)", "chromium", "145.0.7632.45", VersionRelationType.LaterOrEqual)]
	[DataRow("thunar (>> 4.18.8)", "thunar", "4.18.8", VersionRelationType.StrictlyLater)]

	[DataRow("git(<<  1:2.43.0-1ubuntu7.3  )", "git", "1:2.43.0-1ubuntu7.3", VersionRelationType.StrictlyEarlier)]
	[DataRow("7zip(<=  23.01+dfsg-11)", "7zip", "23.01+dfsg-11", VersionRelationType.EarlierOrEqual)]
	[DataRow("pkg(=1.2.3   )", "pkg", "1.2.3", VersionRelationType.ExactlyEqual)]
	[DataRow("chromium(>=  145.0.7632.45  )", "chromium", "145.0.7632.45", VersionRelationType.LaterOrEqual)]
	[DataRow("thunar(>>4.18.8)", "thunar", "4.18.8", VersionRelationType.StrictlyLater)]
	public void Parse_PackageRelationWithOperators(string relationship, string expectedPackageName, string expectedVersion, VersionRelationType expectedRelationType)
	{
		var context = CreateContext(relationship, expectedPackageName);
		var packageRelationship = Assert.IsInstanceOfType<PackageRelationship>(RelationshipParser.Parse(relationship.AsSpan(), context), RelationshipMustBePackageRelationship);
		var package = packageRelationship.Package;

		Assert.IsNotNull(packageRelationship.RelationType, MissingVersionRelationTypeInPackageRelationship);
		Assert.IsNotNull(packageRelationship.Version, MissingVersionRelationTypeInPackageRelationship);
		Assert.AreEqual(expectedPackageName, package.Name, IncorrectPackageNameInPackageRelation);
		Assert.AreEqual(expectedRelationType, packageRelationship.RelationType, IncorrectVersionRelationTypeInPackageRelation);
		Assert.AreEqual(expectedVersion, packageRelationship.Version.ToString(), IncorrectVersionNumberInPackageRelation);
	}

	[TestMethod]
	[DataRow("pkg ( 1.2.3 )", "pkg", "1.2.3")]
	[DataRow("pkg (  1.2.3  )", "pkg", "1.2.3")]
	[DataRow("pkg(1.2.3)", "pkg", "1.2.3")]
	[DataRow("pkg(1.2.3 )", "pkg", "1.2.3")]
	[DataRow("pkg( 1.2.3)", "pkg", "1.2.3")]
	public void Parse_PackageWithExactVersionButNoOperator(string relationship, string expectedPackageName, string expectedVersion)
	{
		var context = CreateContext(relationship, "pkg");
		var packageRelationship = Assert.IsInstanceOfType<PackageRelationship>(RelationshipParser.Parse(relationship.AsSpan(), context), RelationshipMustBePackageRelationship);

		Assert.IsNotNull(packageRelationship.Version);
		Assert.AreEqual(VersionRelationType.ExactlyEqual, packageRelationship.RelationType, IncorrectVersionRelationTypeInPackageRelation);
		Assert.AreEqual(expectedPackageName, packageRelationship.Package.Name);
		Assert.AreEqual(expectedVersion, packageRelationship.Version.ToString(), IncorrectVersionNumberInPackageRelation);
	}

	[TestMethod]
	[DataRow("a, b", new string[] { "a", "b" })]
	[DataRow("a,b", new string[] { "a", "b" })]
	[DataRow("a, b, c", new string[] { "a", "b", "c" })]
	[DataRow("d, b, a,c", new string[] { "a", "b", "c", "d"})]
	public void Parse_CommaSeparated_ReturnsMultipleRelationships(string relationship, string[] expectedPackages)
	{
		var context = CreateContext(relationship, expectedPackages);
		var multipleRelationships = Assert.IsInstanceOfType<MultipleRelationships>(RelationshipParser.Parse(relationship.AsSpan(), context), RelationshipMustBeMultpleRelationship);
		Assert.IsTrue(FlattenMultipleRelationship(multipleRelationships).Select(e => e.Package.Name).SequenceEqual(expectedPackages), IncorrectRelationshipItemsInGrouppingRelationship);
	}

	[TestMethod]
	[DataRow("a | b", new string[] { "a", "b" })]
	[DataRow("a|b", new string[] { "a", "b" })]
	[DataRow("a | b | c", new string[] { "a", "b", "c" })]
	[DataRow("d| b | a|c ", new string[] { "a", "b", "c", "d"})]
	public void Parse_PipeSeparated_ReturnsAnyRelationship(string relationship, string[] expectedPackages)
	{
		var context = CreateContext(relationship, expectedPackages);
		var anyRelationship = Assert.IsInstanceOfType<AnyRelationship>(RelationshipParser.Parse(relationship.AsSpan(), context), RelationshipMustBeAnyRelationship);
		Assert.IsTrue(FlattenAnyRelationship(anyRelationship).Select(e => e.Package.Name).SequenceEqual(expectedPackages), IncorrectRelationshipItemsInGrouppingRelationship);
	}

	[TestMethod]
	[DataRow(
		"a, b, c | d | e, f",
		new Type[] { typeof(PackageRelationship), typeof(PackageRelationship), typeof(AnyRelationship), typeof(PackageRelationship) },
		new string[] { "a" },
		new string[] { "b" },
		new string[] { "c", "d", "e" },
		new string[] { "f" }
	)]
	public void Parse_MixedAnyThenMultiple_ReturnsMultipleRelationships(string relationship, Type[] expectedRelationshipTypes, params string[][] expectedPackages)
	{
		var context = CreateContext(relationship, expectedPackages.SelectMany(e => e).ToArray());
		var multipleRelationships = Assert.IsInstanceOfType<MultipleRelationships>(RelationshipParser.Parse(relationship.AsSpan(), context), RelationshipMustBeMultpleRelationship);
		var relationshipAsList = GetRelationshipsAsList(multipleRelationships);

		Assert.IsTrue(relationshipAsList.Select(static rel => rel.GetType()).SequenceEqual(expectedRelationshipTypes), IncorrectRelationshipItemsInGrouppingRelationship);
		Assert.IsTrue(relationshipAsList.Select(static rel => rel is GrouppingRelationships grouppingRelationships ? grouppingRelationships.Relationships.Length : 1).SequenceEqual(expectedPackages.Select(e => e.Length)), IncorrectRelationshipItemsInGrouppingRelationship);
		Assert.IsTrue(FlattenMultipleRelationship(multipleRelationships).Select(e => e.Package.Name).SequenceEqual(expectedPackages.SelectMany(e => e)), IncorrectRelationshipItemsInGrouppingRelationship);
	}

	[TestMethod]
	[DataRow("pkg (>= 1.2")]
	public void Parse_MissingEnclosedParenthesis_Throws(string relationship)
	{
		var context = CreateContext(relationship);
		Assert.Throws<FormatException>(() => RelationshipParser.Parse(relationship.AsSpan(), context), ParserDoesntThrowFormatExceptionWhenRelationshipSyntaxIsInvalid);
	}

	[TestMethod]
	[DataRow("pkg (?? 1.2)")]
	[DataRow("pkg ( <  1.2)")]
	[DataRow("pkg(??1.2)")]
	public void Parse_UnknownRelationOperator_Throws(string relationship)
	{
		var context = CreateContext(relationship);
		Assert.Throws<FormatException>(() => RelationshipParser.Parse(relationship.AsSpan(), context), ParserDoesntThrowFormatExceptionWhenRelationshipSyntaxIsInvalid);
	}
}

public partial class RelationshipParserTest
{
	private const string RelationshipMustBePackageRelationship = $"The type of Package property must be a DebianPackage because this package has been resolved in the context";

	private const string RelationshipMustBeMultpleRelationship = $"The relationship must be a MultipleRelationship";

	private const string RelationshipMustBeAnyRelationship = $"The relationship must be a AnyRelationship";

	private const string MissingVersionRelationTypeInPackageRelationship = "Missing version relation type in package relationship";

	private const string MissingVersionNumberInPackageRelationship = "Missing version number in package relationship";

	private const string IncorrectPackageNameInPackageRelation = "Incorrect package name";

	private const string IncorrectVersionRelationTypeInPackageRelation = "Incorrect version relation type";

	private const string IncorrectVersionNumberInPackageRelation = "Incorrect version number in version relation";

	private const string IncorrectRelationshipItemsInGrouppingRelationship = "Incorrect relationship items in groupping relation type";

	private const string InvalidTypesOfRelationshipItemsOfAnyRelationship = "Relationship items in AnyRelationship must contains only PackageRelationship";

	private const string InvalidTypesOfRelationshipItemsOfMultipleRelationship = "Relationship items in MultipleRelationship must contains only AnyRelationship or PackageRelationship";

	private const string ParserDoesntThrowFormatExceptionWhenRelationshipSyntaxIsInvalid = "Parser hasn't thrown FormatException when relationship syntax is invalid";


	private static DebianPackageParsingContext CreateContext(string controlData, params string[] packageNames)
	{
		var packages = new Dictionary<string, DebianPackage>(StringComparer.Ordinal);

		foreach (var name in packageNames)
		{
			var pkg = new DebianPackage { Name = name };
			packages[name] = pkg;
		}

		return new DebianPackageParsingContext
		(
			ControlData: controlData,
			StanzaRanges: [],
			Packages: packages,
			VirtualPackages: []
		);
	}

	private static PackageRelationship AssertContainsPackageRelationship(GrouppingRelationships grouppingRelationships, Predicate<IPackage> predicate, string message)
	{
		foreach (var relationship in grouppingRelationships.Relationships)
		{
			if (relationship is PackageRelationship packageRelationship && predicate(packageRelationship.Package))
			{
				return packageRelationship;
			}
		}

		Assert.Fail(message);
		return null;
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_relationships")]
	private static extern ref List<IRelationship> GetRelationshipsAsList(GrouppingRelationships grouppingRelationships);

	private static IEnumerable<PackageRelationship> FlattenAnyRelationship(AnyRelationship anyRelationship)
	{
		return GetRelationshipsAsList(anyRelationship).Select(e => Assert.IsInstanceOfType<PackageRelationship>(e, InvalidTypesOfRelationshipItemsOfAnyRelationship));	
	}

	private static IEnumerable<PackageRelationship> FlattenMultipleRelationship(MultipleRelationships relationships)
	{
		return GetRelationshipsAsList(relationships).SelectMany(
			static rel =>
			{
				return rel is AnyRelationship anyRelationship ? FlattenAnyRelationship(anyRelationship) :
					new PackageRelationship[] { Assert.IsInstanceOfType<PackageRelationship>(rel, InvalidTypesOfRelationshipItemsOfMultipleRelationship) };
			}
		);
	}
}