using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace APTPackageDependenciesResolver.Test;

[TestClass]
public class RelationshipParserTest
{
	private static DebianPackageParsingContext CreateContext(params string[] packageNames)
	{
		var packages = new Dictionary<string, DebianPackage>(StringComparer.Ordinal);

		foreach (var name in packageNames)
		{
			var pkg = new DebianPackage { Name = name };
			packages[name] = pkg;
		}

		return new DebianPackageParsingContext("control", new Dictionary<string, Range>(), packages, new Dictionary<string, DebianVirtualPackage>());
	}

	[TestMethod]
	public void Parse_SinglePackage_NoVersion()
	{
		var context = CreateContext("pkg");
		var result = RelationshipParser.Parse("pkg".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(PackageRelationship));

		var pr = (PackageRelationship)result;
		Assert.AreEqual("pkg", pr.Package.Name);
		Assert.IsNull(pr.Version);
	}

	[TestMethod]
	public void Parse_Package_WithExactVersion_WithOperatorAndSpaces()
	{
		var context = CreateContext("pkg");
		var result = RelationshipParser.Parse("pkg ( = 1.2.3 )".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(PackageRelationship));
		var pr = (PackageRelationship)result;
		Assert.AreEqual(VersionRelationType.ExactlyEqual, pr.RelationType);
		Assert.IsNotNull(pr.Version);
		Assert.AreEqual("1.2.3", pr.Version!.UpstreamVersion);
	}

	[TestMethod]
	public void Parse_Package_WithExactVersion_NoOperator()
	{
		var context = CreateContext("pkg");
		var result = RelationshipParser.Parse("pkg ( 1.2.3 )".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(PackageRelationship));
		var pr = (PackageRelationship)result;
		Assert.AreEqual(VersionRelationType.ExactlyEqual, pr.RelationType);
		Assert.AreEqual("1.2.3", pr.Version!.UpstreamVersion);
	}

	[TestMethod]
	public void Parse_Package_WithOperatorNoSpace()
	{
		var context = CreateContext("pkg");
		var result = RelationshipParser.Parse("pkg (>=1.2.3)".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(PackageRelationship));
		var pr = (PackageRelationship)result;
		Assert.AreEqual(VersionRelationType.LaterOrEqual, pr.RelationType);
		Assert.AreEqual("1.2.3", pr.Version!.UpstreamVersion);
	}

	[TestMethod]
	public void Parse_CommaSeparated_ReturnsMultipleRelationships()
	{
		var context = CreateContext("a", "b");
		var result = RelationshipParser.Parse("a, b".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(MultipleRelationships));
	}

	[TestMethod]
	public void Parse_PipeSeparated_ReturnsAnyRelationship()
	{
		var context = CreateContext("a", "b");
		var result = RelationshipParser.Parse("a | b".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(AnyRelationship));
	}

	[TestMethod]
	public void Parse_MixedAnyThenMultiple_ReturnsMultipleRelationships()
	{
		var context = CreateContext("a", "b", "c");
		var result = RelationshipParser.Parse("a | b, c".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(MultipleRelationships));
	}

	[TestMethod]
	public void Parse_MissingClosingParenthesis_Throws()
	{
		var context = CreateContext("pkg");

		Assert.Throws<Exception>(() => RelationshipParser.Parse("pkg (>= 1.2".AsSpan(), context));
	}

	[TestMethod]
	public void Parse_UnknownRelationOperator_Throws()
	{
		var context = CreateContext("pkg");

		Assert.Throws<Exception>(() => RelationshipParser.Parse("pkg (?? 1.2)".AsSpan(), context));
	}

	[TestMethod]
	public void Parse_Trimming_Works()
	{
		var context = CreateContext("pkg");
		var result = RelationshipParser.Parse("  pkg (= 1.0 )  ".AsSpan(), context);

		Assert.IsInstanceOfType(result, typeof(PackageRelationship));
		var pr = (PackageRelationship)result;
		Assert.AreEqual("pkg", pr.Package.Name);
		Assert.AreEqual("1.0", pr.Version!.UpstreamVersion);
	}
}
