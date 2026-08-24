using System.Collections.Frozen;
using Rasm.TestKit;

namespace Rasm.Architecture.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class CatalogueBoundaryLaws {
    [Fact]
    public void DotnetApiCataloguesDoNotCarryReadmePages() =>
        Assert.All(
            collection: Manifests.Files(relativeRoot: "libs/dotnet", pattern: "README.md"),
            action: static path => Assert.DoesNotContain(expectedSubstring: "/.api/", actualString: path, comparisonType: StringComparison.Ordinal));

    [Fact]
    public void CentralDotnetApiCataloguesAreFlatApiCards() {
        FrozenSet<string> cards = Manifests.Files(relativeRoot: "libs/dotnet/.api", pattern: "*.md");

        Assert.NotEmpty(collection: cards);
        Assert.All(collection: cards, action: static path =>
            Assert.StartsWith(expectedStartString: "libs/dotnet/.api/api-", actualString: path, comparisonType: StringComparison.Ordinal));
    }

    [Fact]
    public void PackageLocalApiCataloguesStayPackageLocalCards() {
        string[] cards = [.. Manifests.Files(relativeRoot: "libs/dotnet", pattern: "*.md")
            .Where(static path =>
                path.Contains(value: "/.api/", comparisonType: StringComparison.Ordinal)
                && !path.StartsWith(value: "libs/dotnet/.api/", comparisonType: StringComparison.Ordinal))];

        Assert.All(collection: cards, action: static path =>
            Assert.DoesNotContain(expectedSubstring: "/README.md", actualString: path, comparisonType: StringComparison.Ordinal));
    }
}
