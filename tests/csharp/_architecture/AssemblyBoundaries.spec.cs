using System.Collections.Frozen;
using System.Reflection;
using ArchUnitNET.Loader;
using Rasm.TestKit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using TypesShouldConjunction = ArchUnitNET.Fluent.Syntax.Elements.Types.TypesShouldConjunction;

namespace Rasm.Architecture.Tests;

// --- [MODELS] --------------------------------------------------------------------------
internal static class ArchitectureModel {
    public static readonly Assembly Core = typeof(Op).Assembly;
    public static readonly ArchUnitNET.Domain.Architecture Kernel = new ArchLoader().LoadAssemblies(Core).Build();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class AssemblyBoundaryLaws {
    private static readonly string[] ProjectRoots = ["apps", "libs", "tests", "tools"];

    private static readonly FrozenDictionary<string, FrozenSet<string>> ExpectedProjectReferences = new Dictionary<string, string[]>(StringComparer.Ordinal) {
        ["libs/csharp/Rasm/Rasm.csproj"] = [],
        ["libs/csharp/Rasm.Element/Rasm.Element.csproj"] = ["../Rasm/Rasm.csproj"],
        ["libs/csharp/Rasm.Materials/Rasm.Materials.csproj"] = ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj"],
        ["libs/csharp/Rasm.Bim/Rasm.Bim.csproj"] = ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj"],
        ["libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj"] = ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj"],
        ["libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj"] = [],
        ["libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj"] = ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj"],
        ["libs/csharp/Rasm.Compute/Rasm.Compute.csproj"] = ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj"],
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
    public void KernelProjectHasNoProjectReferences() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm/Rasm.csproj").IncludesNoProjects();

    [Fact]
    public void ElementReferencesOnlyKernel() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Element/Rasm.Element.csproj").IncludesOnlyProjects("../Rasm/Rasm.csproj");

    [Fact]
    public void BimReferencesOnlyKernelAndElement() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Bim/Rasm.Bim.csproj").IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj");

    [Fact]
    public void FabricationReferencesOnlyKernelAndElement() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj").IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj");

    [Fact]
    public void RhinoBoundaryReferencesOnlyKernel() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj").IncludesOnlyProjects("../Rasm/Rasm.csproj");

    [Fact]
    public void GrasshopperBoundaryReferencesOnlyKernel() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj").IncludesOnlyProjects("../Rasm/Rasm.csproj");

    [Fact]
    public void AppUiReferencesOnlyApplicationRuntimeOwners() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj").IncludesOnlyProjects(
            "../Rasm/Rasm.csproj",
            "../Rasm.AppHost/Rasm.AppHost.csproj",
            "../Rasm.Compute/Rasm.Compute.csproj",
            "../Rasm.Persistence/Rasm.Persistence.csproj");

    [Fact]
    public void ComputeReferencesOnlyKernelElementAppHostAndPersistence() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Compute/Rasm.Compute.csproj").IncludesOnlyProjects(
            "../Rasm/Rasm.csproj",
            "../Rasm.Element/Rasm.Element.csproj",
            "../Rasm.AppHost/Rasm.AppHost.csproj",
            "../Rasm.Persistence/Rasm.Persistence.csproj");

    [Fact]
    public void MaterialsReferencesOnlyKernelAndElement() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Materials/Rasm.Materials.csproj").IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj");

    [Fact]
    public void PersistenceReferencesOnlyKernelElementAndAppHost() =>
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj").IncludesOnlyProjects(
            "../Rasm/Rasm.csproj",
            "../Rasm.Element/Rasm.Element.csproj",
            "../Rasm.AppHost/Rasm.AppHost.csproj");

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
    public void HostBoundaryProjectsDoNotDeclarePackageReferences() {
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj").IncludesNoPackages();
        PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj").IncludesNoPackages();
    }

    [Fact]
    public void KernelDomainTypesDoNotDependOnVectorOrAnalysisNamespaces() {
        TypesShouldConjunction domainTypesDoNotDependOnVectors =
            Types().That().ResideInNamespace("Rasm.Domain").Should().NotDependOnAny(Types().That().ResideInNamespace("Rasm.Vectors"));
        TypesShouldConjunction domainTypesDoNotDependOnAnalysis =
            Types().That().ResideInNamespace("Rasm.Domain").Should().NotDependOnAny(Types().That().ResideInNamespace("Rasm.Analysis"));

        Assert.True(condition: domainTypesDoNotDependOnVectors.HasNoViolations(ArchitectureModel.Kernel));
        Assert.True(condition: domainTypesDoNotDependOnAnalysis.HasNoViolations(ArchitectureModel.Kernel));
    }

    [Fact]
    public void KernelVectorTypesDoNotDependOnAnalysisNamespaces() {
        TypesShouldConjunction vectorTypesDoNotDependOnAnalysis =
            Types().That().ResideInNamespace("Rasm.Vectors").Should().NotDependOnAny(Types().That().ResideInNamespace("Rasm.Analysis"));

        Assert.True(condition: vectorTypesDoNotDependOnAnalysis.HasNoViolations(ArchitectureModel.Kernel));
    }

    private static string[] Sorted(IEnumerable<string> rows) =>
        [.. rows.Order(comparer: StringComparer.Ordinal)];

    private static FrozenSet<string> AssemblyReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(static name => name.Name)
            .OfType<string>()
            .ToFrozenSet(StringComparer.Ordinal);
}
