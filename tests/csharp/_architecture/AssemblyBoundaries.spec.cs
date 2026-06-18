using System.Collections.Frozen;
using System.Reflection;
using Rasm.TestKit;

namespace Rasm.Architecture.Tests;

// --- [MODELS] --------------------------------------------------------------------------
internal static class ArchitectureModel {
    public static readonly Assembly Core = typeof(Op).Assembly;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class AssemblyBoundaryLaws {
    private static readonly string[] ProjectRoots = ["apps", "libs", "tests", "tools"];

    private static readonly FrozenDictionary<string, FrozenSet<string>> ExpectedProjectReferences = new Dictionary<string, string[]>(StringComparer.Ordinal) {
        ["libs/csharp/Rasm/Rasm.csproj"] = [],
        ["libs/csharp/Rasm.Materials/Rasm.Materials.csproj"] = ["../Rasm/Rasm.csproj"],
        ["libs/csharp/Rasm.Bim/Rasm.Bim.csproj"] = ["../Rasm/Rasm.csproj"],
        ["libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj"] = ["../Rasm/Rasm.csproj"],
        ["libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj"] = [],
        ["libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj"] = ["../Rasm.AppHost/Rasm.AppHost.csproj"],
        ["libs/csharp/Rasm.Compute/Rasm.Compute.csproj"] = ["../Rasm/Rasm.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj"],
        ["libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj"] = ["../Rasm/Rasm.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Compute/Rasm.Compute.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj"],
        ["libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj"] = ["../Rasm/Rasm.csproj"],
        ["libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj"] = ["../Rasm/Rasm.csproj"],
    }.ToFrozenDictionary(
        keySelector: static row => row.Key,
        elementSelector: static row => row.Value.ToFrozenSet(StringComparer.Ordinal),
        comparer: StringComparer.Ordinal);

    [Fact]
    public void WorkspaceSolutionContainsEveryCsharpProject() {
        FrozenSet<string> solution = PackageAdmission.SolutionProjects();
        FrozenSet<string> disk = PackageAdmission.DiskProjects(roots: ProjectRoots);
        Assert.Equal(expected: Sorted(disk), actual: Sorted(solution));
    }

    [Fact]
    public void CsharpPackageGraphFollowsPlanningStrata() {
        string plan = PackageAdmission.ReadText(relativePath: "libs/csharp/.planning/ARCHITECTURE.md");
        foreach ((string project, FrozenSet<string> expected) in ExpectedProjectReferences) {
            Assert.Contains(expectedSubstring: Path.GetFileNameWithoutExtension(path: project), actualString: plan, comparisonType: StringComparison.Ordinal);
            Assert.Equal(expected: Sorted(expected), actual: Sorted(PackageAdmission.Project(relativePath: project).ProjectReferences));
        }
    }

    [Fact]
    public void CoreDoesNotDependOnBoundaryLibraries() {
        FrozenSet<string> references = AssemblyReferences(assembly: ArchitectureModel.Core);
        Assert.DoesNotContain(expected: "Rasm.Grasshopper", collection: references);
        Assert.DoesNotContain(expected: "Rasm.Rhino", collection: references);
    }

    [Fact]
    public void BoundaryAssembliesDoNotReferenceEachOtherUpstream() {
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj").ExcludesProjects("../Rasm.Grasshopper/Rasm.Grasshopper.csproj");
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj").ExcludesProjects("../Rasm.Rhino/Rasm.Rhino.csproj");
    }

    [Fact]
    public void DomainDoesNotDependOnVectorOrAnalysisNamespaces() =>
        Assert.Empty(collection: DependencyNamespaces(sourceNamespace: "Rasm.Domain", blocked: ["Rasm.Vectors", "Rasm.Analysis"]));

    [Fact]
    public void VectorsDoesNotDependOnAnalysisNamespace() =>
        Assert.Empty(collection: DependencyNamespaces(sourceNamespace: "Rasm.Vectors", blocked: ["Rasm.Analysis"]));

    private static string[] Sorted(IEnumerable<string> rows) =>
        [.. rows.Order(comparer: StringComparer.Ordinal)];

    private static FrozenSet<string> AssemblyReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(static name => name.Name)
            .OfType<string>()
            .ToFrozenSet(StringComparer.Ordinal);

    private static FrozenSet<string> DependencyNamespaces(string sourceNamespace, params string[] blocked) {
        FrozenSet<string> blockedSet = blocked.ToFrozenSet(StringComparer.Ordinal);
        return SourceNamespaceReferences(sourceNamespace: sourceNamespace, blocked: blockedSet)
            .ToFrozenSet(StringComparer.Ordinal);
    }

    private static IEnumerable<string> SourceNamespaceReferences(string sourceNamespace, FrozenSet<string> blocked) {
        string folder = sourceNamespace[(sourceNamespace.LastIndexOf(value: '.') + 1)..];
        foreach (string file in Directory.EnumerateFiles(path: PackageAdmission.PathOf(relativePath: $"libs/csharp/Rasm/{folder}"), searchPattern: "*.cs", searchOption: SearchOption.TopDirectoryOnly)) {
            string text = File.ReadAllText(path: file);
            foreach (string blockedNamespace in blocked.Where(blockedNamespace => text.Contains(value: $"using {blockedNamespace}", comparisonType: StringComparison.Ordinal) || text.Contains(value: $"{blockedNamespace}.", comparisonType: StringComparison.Ordinal))) {
                yield return blockedNamespace;
            }
        }
    }
}
