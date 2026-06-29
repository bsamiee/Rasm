using System.Collections.Frozen;
using System.Xml.Linq;

namespace Rasm.TestKit;

// --- [MODELS] --------------------------------------------------------------------------------
public sealed class ProjectAdmission {
    internal ProjectAdmission(string relativePath, XDocument document) {
        RelativePath = relativePath;
        Document = document;
    }

    public string RelativePath { get; }
    internal XDocument Document { get; }
    public FrozenSet<string> Packages => Names("PackageReference").ToFrozenSet(StringComparer.Ordinal);
    public FrozenSet<string> ProjectReferences => Attrs("ProjectReference", "Include")
        .Select(static path => path.Replace('\\', '/'))
        .ToFrozenSet(StringComparer.Ordinal);
    public FrozenSet<string> VersionedPackages => Elements("PackageReference")
        .Where(static row => row.Attribute("Version") is not null)
        .Select(static row => row.Attribute("Include")?.Value ?? string.Empty)
        .Where(static name => name.Length > 0)
        .ToFrozenSet(StringComparer.Ordinal);

    public void IncludesPackages(params string[] names) {
        AssertNoVersionedPackageReferences();
        PackageAdmission.CentralPackages(names);
        Assert.Empty(collection: names.Except(second: Packages, comparer: StringComparer.Ordinal));
    }

    public void IncludesOnlyPackages(params string[] names) {
        IncludesPackages(names: names);
        Assert.Equal(expected: Sorted(rows: names), actual: Sorted(rows: Packages));
    }

    public void ExcludesPackages(params string[] names) {
        AssertNoVersionedPackageReferences();
        Assert.Empty(collection: names.Intersect(Packages, StringComparer.Ordinal));
    }

    public void IncludesProjects(params string[] paths) {
        AssertNoVersionedPackageReferences();
        Assert.Empty(collection: paths.Except(second: ProjectReferences, comparer: StringComparer.Ordinal));
    }

    public void IncludesOnlyProjects(params string[] paths) {
        IncludesProjects(paths: paths);
        Assert.Equal(expected: Sorted(rows: paths), actual: Sorted(rows: ProjectReferences));
    }

    public void ExcludesProjects(params string[] paths) {
        AssertNoVersionedPackageReferences();
        Assert.Empty(collection: paths.Intersect(ProjectReferences, StringComparer.Ordinal));
    }

    public void IncludesNoPackages() {
        AssertNoVersionedPackageReferences();
        Assert.Empty(collection: Packages);
    }

    public void IncludesNoProjects() {
        AssertNoVersionedPackageReferences();
        Assert.Empty(collection: ProjectReferences);
    }

    public void PackageReferenceHasAttribute(string packageName, string attributeName, string expectedValue) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: packageName);
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: attributeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: expectedValue);
        XElement reference = Elements(localName: "PackageReference").Single(row =>
            string.Equals(a: row.Attribute("Include")?.Value, b: packageName, comparisonType: StringComparison.Ordinal));
        Assert.Equal(expected: expectedValue, actual: reference.Attribute(attributeName)?.Value);
    }

    private void AssertNoVersionedPackageReferences() =>
        Assert.Empty(collection: VersionedPackages);

    private IEnumerable<XElement> Elements(string localName) =>
        Document.Descendants().Where(row => string.Equals(a: row.Name.LocalName, b: localName, comparisonType: StringComparison.Ordinal));

    private IEnumerable<string> Attrs(string element, string attr) =>
        Elements(localName: element).Select(row => row.Attribute(attr)?.Value ?? string.Empty).Where(static value => value.Length > 0);

    internal IEnumerable<string> Names(string localName) =>
        Attrs(element: localName, attr: "Include");

    private static string[] Sorted(IEnumerable<string> rows) =>
        [.. rows.Order(comparer: StringComparer.Ordinal)];
}

// --- [SERVICES] ------------------------------------------------------------------------------
public static class PackageAdmission {
    public static ProjectAdmission Project(string relativePath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relativePath);
        return new ProjectAdmission(relativePath: relativePath, document: XDocument.Load(uri: PathOf(relativePath)));
    }

    public static void CentralPackages(params string[] names) {
        AssertNoPackageVersionOverrides();
        Assert.Empty(collection: names.Except(
            second: Project(relativePath: "Directory.Packages.props").Names(localName: "PackageVersion").ToFrozenSet(StringComparer.Ordinal),
            comparer: StringComparer.Ordinal));
    }

    public static void ApiCatalogues(string relativeDirectory, params string[] names) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relativeDirectory);
        FrozenSet<string> catalogues = Directory.Exists(path: PathOf(relativeDirectory))
            ? Directory.EnumerateFiles(path: PathOf(relativeDirectory), searchPattern: "*.md").Select(selector: Path.GetFileName).OfType<string>().ToFrozenSet(StringComparer.Ordinal)
            : Enumerable.Empty<string>().ToFrozenSet(StringComparer.Ordinal);
        Assert.Empty(collection: names.Except(second: catalogues, comparer: StringComparer.Ordinal));
    }

    public static FrozenSet<string> SolutionProjects() =>
        XDocument.Load(uri: PathOf("Workspace.slnx"))
            .Descendants()
            .Where(static row => string.Equals(a: row.Name.LocalName, b: "Project", comparisonType: StringComparison.Ordinal))
            .Select(static row => row.Attribute("Path")?.Value ?? string.Empty)
            .Where(static path => path.EndsWith(value: ".csproj", comparisonType: StringComparison.Ordinal))
            .ToFrozenSet(StringComparer.Ordinal);

    public static FrozenSet<string> DiskProjects(params string[] roots) {
        ArgumentNullException.ThrowIfNull(argument: roots);
        return roots.SelectMany(root => Directory.Exists(path: PathOf(root))
                ? Directory.EnumerateFiles(path: PathOf(root), searchPattern: "*.csproj", searchOption: SearchOption.AllDirectories)
                : [])
            .Select(path => Path.GetRelativePath(relativeTo: Root.FullName, path: path).Replace('\\', '/'))
            .ToFrozenSet(StringComparer.Ordinal);
    }

    public static FrozenSet<string> Files(string relativeRoot, string pattern) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relativeRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: pattern);
        string root = PathOf(relativePath: relativeRoot);
        return Directory.Exists(path: root)
            ? Directory.EnumerateFiles(path: root, searchPattern: pattern, searchOption: SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(relativeTo: Root.FullName, path: path).Replace('\\', '/'))
                .Order(comparer: StringComparer.Ordinal)
                .ToFrozenSet(StringComparer.Ordinal)
            : Enumerable.Empty<string>().ToFrozenSet(StringComparer.Ordinal);
    }

    public static string ReadText(string relativePath) =>
        File.ReadAllText(path: PathOf(relativePath));

    public static string PathOf(string relativePath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relativePath);
        return Path.Combine(path1: Root.FullName, path2: relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static void AssertNoPackageVersionOverrides() =>
        Assert.Equal(expected: "false", actual: Project(relativePath: "Directory.Packages.props").Document.Root?
            .Descendants().FirstOrDefault(predicate: static row => string.Equals(a: row.Name.LocalName, b: "CentralPackageVersionOverrideEnabled", comparisonType: StringComparison.Ordinal))?.Value);

    private static DirectoryInfo Root {
        get {
            DirectoryInfo? directory = new(path: AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(path: Path.Combine(path1: directory.FullName, path2: "Workspace.slnx"))) {
                directory = directory.Parent;
            }
            return directory ?? throw new InvalidOperationException(message: "Workspace.slnx not found above test output.");
        }
    }
}
