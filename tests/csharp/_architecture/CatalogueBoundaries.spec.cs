using System.Collections.Frozen;
using Rasm.TestKit;

namespace Rasm.Architecture.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class CatalogueBoundaryLaws {
    [Fact]
    public void CsharpApiCataloguesDoNotCarryReadmePages() =>
        Assert.All(
            collection: Manifests.Files(relativeRoot: "libs/csharp", pattern: "README.md"),
            action: static path => Assert.DoesNotContain(expectedSubstring: "/.api/", actualString: path, comparisonType: StringComparison.Ordinal));

    [Fact]
    public void CentralCsharpApiCataloguesAreFlatApiCards() {
        FrozenSet<string> cards = Manifests.Files(relativeRoot: "libs/csharp/.api", pattern: "*.md");

        Assert.NotEmpty(collection: cards);
        Assert.All(collection: cards, action: static path =>
            Assert.StartsWith(expectedStartString: "libs/csharp/.api/api-", actualString: path, comparisonType: StringComparison.Ordinal));
    }

    [Fact]
    public void PackageLocalApiCataloguesStayPackageLocalCards() {
        string[] cards = [.. Manifests.Files(relativeRoot: "libs/csharp", pattern: "*.md")
            .Where(static path =>
                path.Contains(value: "/.api/", comparisonType: StringComparison.Ordinal)
                && !path.StartsWith(value: "libs/csharp/.api/", comparisonType: StringComparison.Ordinal))];

        Assert.All(collection: cards, action: static path =>
            Assert.DoesNotContain(expectedSubstring: "/README.md", actualString: path, comparisonType: StringComparison.Ordinal));
    }
}
