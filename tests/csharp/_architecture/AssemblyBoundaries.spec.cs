using System.Collections.Frozen;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.Loader;
using Rasm.Csp;
using Rasm.TestKit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Rasm.Architecture.Tests;

// --- [MODELS] --------------------------------------------------------------------------
// The host-free closure: every Rasm assembly loadable without a Rhino installation. Rules run
// over this architecture only; host-closed assemblies are manifest facts, never loaded types.
internal static class HostFreeModel {
    public static readonly System.Reflection.Assembly TestKit = typeof(Spec).Assembly;
    public static readonly System.Reflection.Assembly Contract = typeof(Bridge.Contract.Handshake).Assembly;
    public static readonly System.Reflection.Assembly CspContracts = typeof(CspScope).Assembly;
    public static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(TestKit, Contract, CspContracts).Build();

    // Every ArchUnitNET rule is vacuously true over an empty type set; rules call this gate first.
    public static void NonVacuous(params System.Reflection.Assembly[] assemblies) =>
        Spec.Matrix(rows: [.. assemblies.Select(assembly => (
            Label: $"types loaded for {assembly.GetName().Name}",
            Probe: (Func<bool>)(() => Architecture.Types.Any(type => string.Equals(a: type.Assembly.Name, b: assembly.GetName().Name, comparisonType: StringComparison.Ordinal))),
            Expected: true))]);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class AssemblyBoundaryLaws {
    private static readonly string[] ProjectRoots = ["apps", "libs", "tests", "tools"];

    // HOST_BOUNDARY_REENTRY: host-boundary rows live on disk but stay out of Workspace.slnx until
    // kernel realization lands; the roster shrinks to empty when those slnx rows return.
    private static readonly string[] HostBoundaryRows = [
        "libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj",
        "libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj",
        "tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj",
        "tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj",
    ];

    // Exact reference topology per project — "only" is implied by exactness, so per-project
    // sibling facts collapse into this one folded table.
    private static readonly (string Project, string[] References)[] Strata = [
        ("libs/csharp/Rasm/Rasm.csproj", []),
        ("libs/csharp/Rasm.Element/Rasm.Element.csproj", ["../Rasm/Rasm.csproj"]),
        ("libs/csharp/Rasm.Materials/Rasm.Materials.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj"]),
        ("libs/csharp/Rasm.Bim/Rasm.Bim.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj"]),
        ("libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj"]),
        ("libs/csharp/Rasm.AppHost/Rasm.AppHost.csproj", []),
        ("libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj"]),
        ("libs/csharp/Rasm.Compute/Rasm.Compute.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj"]),
        ("libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj", ["../Rasm/Rasm.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Compute/Rasm.Compute.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj"]),
        ("libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj", ["../Rasm/Rasm.csproj"]),
        ("libs/csharp/Rasm.Grasshopper/Rasm.Grasshopper.csproj", ["../Rasm/Rasm.csproj"]),
    ];

    [Fact]
    public void CsharpProjectGraphMatchesTheStrataTable() =>
        Manifests.ProjectGraph(rows: Strata);

    [Fact]
    public void WorkspaceSolutionMatchesDiskAndCarriesTheScenarioHome() {
        FrozenSet<string> solution = Manifests.SolutionProjects();
        FrozenSet<string> disk = Manifests.DiskProjects(roots: ProjectRoots);
        Assert.Equal(
            expected: Sorted(rows: disk.Except(second: HostBoundaryRows, comparer: StringComparer.Ordinal)),
            actual: Sorted(rows: solution));
        Assert.All(collection: HostBoundaryRows, action: row => Assert.Contains(expected: row, collection: disk));
        Assert.Contains(expected: "tests/csharp/scenarios/Rasm.Scenarios.csproj", collection: solution);
    }

    [Fact]
    public void CentralVersioningHasNoProjectLocalDrift() {
        Spec.Holds(condition: Manifests.CentralOverridesDisabled(), label: "Directory.Packages.props must pin CentralPackageVersionOverrideEnabled to false");
        Seq<(string Project, string Package)> rows = Manifests.VersionedPackageRows(roots: ProjectRoots);
        Spec.Holds(condition: rows.IsEmpty, label: $"Version-attributed PackageReference rows breach CPM: {string.Join(separator: "; ", values: rows.Map(static row => $"{row.Project}:{row.Package}"))}");
    }

    [Fact]
    public void ContractNeverDependsOnTheTestKit() {
        HostFreeModel.NonVacuous(HostFreeModel.Contract, HostFreeModel.TestKit);
        Assert.True(condition: Types().That().ResideInAssembly(HostFreeModel.Contract)
            .Should().NotDependOnAny(Types().That().ResideInAssembly(HostFreeModel.TestKit))
            .HasNoViolations(architecture: HostFreeModel.Architecture));
    }

    [Fact]
    public void CspContractsDependOnNoRasmAssembly() {
        HostFreeModel.NonVacuous(HostFreeModel.CspContracts);
        Assert.True(condition: Types().That().ResideInAssembly(HostFreeModel.CspContracts)
            .Should().NotDependOnAny(Types().That().ResideInAssembly(HostFreeModel.TestKit, HostFreeModel.Contract))
            .HasNoViolations(architecture: HostFreeModel.Architecture));
    }

    // The TestKit is host-free and wire-blind end to end; the ScenarioKit assembly owns the bridge
    // wire seam, so the whole TestKit assembly carries the wire-blind obligation.
    [Fact]
    public void TestKitStaysWireBlind() {
        HostFreeModel.NonVacuous(HostFreeModel.TestKit);
        Assert.True(condition: Types().That().ResideInAssembly(HostFreeModel.TestKit)
            .Should().NotDependOnAny(Types().That().ResideInAssembly(HostFreeModel.Contract))
            .HasNoViolations(architecture: HostFreeModel.Architecture));
    }

    [Fact]
    public void HostFreeRasmSlicesAreFreeOfCycles() {
        HostFreeModel.NonVacuous(HostFreeModel.TestKit, HostFreeModel.Contract, HostFreeModel.CspContracts);
        Assert.True(condition: SliceRuleDefinition.Slices().Matching(pattern: "Rasm.(*)")
            .Should().BeFreeOfCycles()
            .HasNoViolations(architecture: HostFreeModel.Architecture));
    }

    private static string[] Sorted(IEnumerable<string> rows) =>
        [.. rows.Order(comparer: StringComparer.Ordinal)];
}
