using System.Collections.Frozen;
using System.Xml.Linq;

namespace Rasm.TestKit;

// --- [MODELS] -------------------------------------------------------------------------------
// Parsed csproj projection: manifest facts only — reference topology and central-version
// discipline. Package rosters are never asserted from here.
public sealed record ProjectFacts(string RelativePath, FrozenSet<string> ProjectReferences, FrozenSet<string> VersionedPackages);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Manifests {
    // The workspace root resolves once per process by walking up from test output to the slnx.
    private static readonly Lazy<DirectoryInfo> Workspace = new(valueFactory: static () => {
        DirectoryInfo? directory = new(path: AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(path: Path.Combine(path1: directory.FullName, path2: "Workspace.slnx"))) {
            directory = directory.Parent;
        }
        return directory ?? throw new InvalidOperationException(message: "Workspace.slnx not found above test output.");
    });

    public static string PathOf(string relativePath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relativePath);
        return Path.Combine(path1: Workspace.Value.FullName, path2: relativePath.Replace(oldChar: '/', newChar: Path.DirectorySeparatorChar));
    }

    public static ProjectFacts Project(string relativePath) {
        XDocument document = XDocument.Load(uri: PathOf(relativePath: relativePath));
        return new ProjectFacts(
            RelativePath: relativePath,
            ProjectReferences: Attrs(document: document, element: "ProjectReference", attr: "Include")
                .Select(selector: static path => path.Replace(oldChar: '\\', newChar: '/'))
                .ToFrozenSet(StringComparer.Ordinal),
            VersionedPackages: Elements(document: document, localName: "PackageReference")
                .Where(predicate: static row => row.Attribute("Version") is not null)
                .Select(selector: static row => row.Attribute("Include")?.Value ?? string.Empty)
                .Where(predicate: static name => name.Length > 0)
                .ToFrozenSet(StringComparer.Ordinal));
    }

    public static FrozenSet<string> SolutionProjects() =>
        Elements(document: XDocument.Load(uri: PathOf(relativePath: "Workspace.slnx")), localName: "Project")
            .Select(selector: static row => row.Attribute("Path")?.Value ?? string.Empty)
            .Where(predicate: static path => path.EndsWith(value: ".csproj", comparisonType: StringComparison.Ordinal))
            .ToFrozenSet(StringComparer.Ordinal);

    public static FrozenSet<string> DiskProjects(params string[] roots) {
        ArgumentNullException.ThrowIfNull(argument: roots);
        return roots.SelectMany(root => Directory.Exists(path: PathOf(relativePath: root))
                ? Directory.EnumerateFiles(path: PathOf(relativePath: root), searchPattern: "*.csproj", searchOption: SearchOption.AllDirectories)
                : [])
            .Select(path => Path.GetRelativePath(relativeTo: Workspace.Value.FullName, path: path).Replace(oldChar: '\\', newChar: '/'))
            .ToFrozenSet(StringComparer.Ordinal);
    }

    public static FrozenSet<string> Files(string relativeRoot, string pattern) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: relativeRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: pattern);
        string root = PathOf(relativePath: relativeRoot);
        return Directory.Exists(path: root)
            ? Directory.EnumerateFiles(path: root, searchPattern: pattern, searchOption: SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(relativeTo: Workspace.Value.FullName, path: path).Replace(oldChar: '\\', newChar: '/'))
                .ToFrozenSet(StringComparer.Ordinal)
            : Enumerable.Empty<string>().ToFrozenSet(StringComparer.Ordinal);
    }

    // Central-version facts: every disk csproj carrying a Version-attributed PackageReference is a
    // CPM breach row, and the central manifest must keep version overrides disabled.
    public static Seq<(string Project, string Package)> VersionedPackageRows(params string[] roots) =>
        toSeq(DiskProjects(roots: roots).Order(comparer: StringComparer.Ordinal))
            .Bind(project => toSeq(Project(relativePath: project).VersionedPackages.Order(comparer: StringComparer.Ordinal))
                .Map(package => (Project: project, Package: package)));

    public static bool CentralOverridesDisabled() =>
        Elements(document: XDocument.Load(uri: PathOf(relativePath: "Directory.Packages.props")), localName: "CentralPackageVersionOverrideEnabled")
            .Any(predicate: static row => string.Equals(a: row.Value, b: "false", comparisonType: StringComparison.Ordinal));

    // One expectation-row verifier: exact reference sets per project, one folded verdict naming
    // every drifting project instead of stopping at the first.
    public static void ProjectGraph(params (string Project, string[] References)[] rows) {
        ArgumentNullException.ThrowIfNull(argument: rows);
        Spec.Holds(condition: rows.Length > 0, label: "ProjectGraph: empty expectation table proves nothing");
        string[] drift = [.. rows.SelectMany(row => {
            string[] expected = [.. row.References.Order(comparer: StringComparer.Ordinal)];
            string[] actual = [.. Project(relativePath: row.Project).ProjectReferences.Order(comparer: StringComparer.Ordinal)];
            string[] mismatch = expected.SequenceEqual(second: actual, comparer: StringComparer.Ordinal)
                ? []
                : [$"{row.Project}: expected [{string.Join(separator: ", ", values: expected)}], got [{string.Join(separator: ", ", values: actual)}]"];
            return mismatch;
        })];
        Spec.Holds(condition: drift.Length == 0, label: $"project graph drift: {string.Join(separator: "; ", values: drift)}");
    }

    private static IEnumerable<XElement> Elements(XDocument document, string localName) =>
        document.Descendants().Where(row => string.Equals(a: row.Name.LocalName, b: localName, comparisonType: StringComparison.Ordinal));

    private static IEnumerable<string> Attrs(XDocument document, string element, string attr) =>
        Elements(document: document, localName: element).Select(row => row.Attribute(attr)?.Value ?? string.Empty).Where(predicate: static value => value.Length > 0);
}
