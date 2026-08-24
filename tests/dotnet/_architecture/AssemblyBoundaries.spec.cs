using System.Collections.Frozen;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.Loader;
using Rasm.Csp;
using Rasm.TestKit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Rasm.Architecture.Tests;

// --- [MODELS] --------------------------------------------------------------------------
// The host-free closure: every Rasm assembly loadable without a Rhino installation. Rules run over this architecture only; host-closed assemblies are manifest facts, never loaded types.
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class AssemblyBoundaryLaws {
    // Exact reference topology per project — "only" is implied by exactness, so per-project sibling facts collapse into this one folded table. `Rasm.Contracts` is the generated bindings
    // distribution, not a stratum: it references no sibling and every wire consumer references it.
    private static readonly (string Project, string[] References)[] Strata = [
        ("libs/dotnet/Rasm/Rasm.csproj", []),
        ("libs/dotnet/Rasm.Contracts/Rasm.Contracts.csproj", []),
        ("libs/dotnet/Rasm.Element/Rasm.Element.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj"]),
        ("libs/dotnet/Rasm.Materials/Rasm.Materials.csproj", ["../Rasm/Rasm.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj", "../Rasm.Element/Rasm.Element.csproj"]),
        ("libs/dotnet/Rasm.Bim/Rasm.Bim.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj", "../Rasm.Element/Rasm.Element.csproj"]),
        ("libs/dotnet/Rasm.Fabrication/Rasm.Fabrication.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj", "../Rasm.Element/Rasm.Element.csproj"]),
        ("libs/dotnet/Rasm.AppHost/Rasm.AppHost.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj"]),
        ("libs/dotnet/Rasm.Persistence/Rasm.Persistence.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj", "../Rasm.Element/Rasm.Element.csproj"]),
        ("libs/dotnet/Rasm.Compute/Rasm.Compute.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj"]),
        ("libs/dotnet/Rasm.AppUi/Rasm.AppUi.csproj", ["../Rasm/Rasm.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj", "../Rasm.Compute/Rasm.Compute.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.Fabrication/Rasm.Fabrication.csproj", "../Rasm.Materials/Rasm.Materials.csproj", "../Rasm.Persistence/Rasm.Persistence.csproj"]),
        ("libs/dotnet/Rasm.Rhino/Rasm.Rhino.csproj", ["../Rasm/Rasm.csproj", "../Rasm.Contracts/Rasm.Contracts.csproj"]),
        ("libs/dotnet/Rasm.Grasshopper/Rasm.Grasshopper.csproj", ["../Rasm/Rasm.csproj"]),
    ];

    [Fact]
    public void DotnetProjectGraphMatchesTheStrataTable() {
        Assert.Equal(
            expected: Sorted(rows: Manifests.DiskProjects(roots: "libs/dotnet")),
            actual: Sorted(rows: Strata.Select(selector: static row => row.Project)));
        Manifests.ProjectGraph(rows: Strata);
    }

    // The disk side is the WHOLE workspace walk, never a root roster: a csproj landing at a new top-level root fails this law loudly instead of silently skipping slnx and CPM parity.
    // The solution is generation-shaped — its project set equals disk exactly, with no carve.
    [Fact]
    public void WorkspaceSolutionMatchesDiskAndCarriesTheScenarioHome() {
        FrozenSet<string> solution = Manifests.SolutionProjects();
        FrozenSet<string> disk = Manifests.DiskProjects();
        Assert.Equal(expected: Sorted(rows: disk), actual: Sorted(rows: solution));
        Assert.Contains(expected: "tests/dotnet/scenarios/Rasm.Scenarios.csproj", collection: solution, comparer: StringComparer.Ordinal);
    }

    private static readonly string[] EstateFiles = ["Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props"];
    private static readonly string[] EstateRoots = ["libs", "apps", "tools", "tests"];

    // A nested MSBuild estate file that omits the upward chaining import silently erases the whole root estate and still builds green, so the import line is mandatory the moment one lands.
    [Fact]
    public void NestedMsBuildEstateFilesChainUpward() {
        string[] unchained = [.. EstateRoots
            .SelectMany(root => EstateFiles.SelectMany(name => Manifests.PrunedFiles(relativeRoot: root, pattern: name)))
            .Where(path => !File.ReadAllText(path: Manifests.PathOf(relativePath: path)).Contains(value: "GetPathOfFileAbove", comparisonType: StringComparison.Ordinal))];
        Spec.Holds(condition: unchained.Length == 0, label: $"nested MSBuild estate files missing the GetPathOfFileAbove chaining import: {string.Join(separator: "; ", values: unchained)}");
    }

    [Fact]
    public void CentralVersioningHasNoProjectLocalDrift() {
        Spec.Holds(condition: Manifests.CentralOverridesDisabled(), label: "Directory.Packages.props must pin CentralPackageVersionOverrideEnabled to false");
        Seq<(string Project, string Package)> rows = Manifests.VersionedPackageRows();
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

    // The TestKit is host-free and wire-blind end to end; the ScenarioKit assembly owns the bridge wire seam, so the whole TestKit assembly carries the wire-blind obligation.
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
