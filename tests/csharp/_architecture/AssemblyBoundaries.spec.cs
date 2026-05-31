using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Rasm.Grasshopper.UI;
using Rasm.Materials.Bricks;
using Rasm.Rhino.Camera;
using ArchitectureGraph = ArchUnitNET.Domain.Architecture;

namespace Rasm.Architecture.Tests;

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class ArchitectureModel {
    public const string CoreAssembly = "^Rasm,";
    public const string MaterialsAssembly = "^Rasm\\.Materials,";
    public const string GrasshopperAssembly = "^Rasm\\.Grasshopper,";
    public const string RhinoAssembly = "^Rasm\\.Rhino,";

    public static readonly ArchitectureGraph Graph = new ArchLoader()
        .LoadAssemblies(
            typeof(Op).Assembly,
            typeof(Subscription).Assembly,
            typeof(Brick).Assembly,
            typeof(RhinoCamera).Assembly)
        .Build();
}

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class AssemblyBoundaryLaws {
    [Fact]
    public void CoreDoesNotDependOnBoundaryLibraries() =>
        ArchRuleDefinition.Types()
            .That().ResideInAssemblyMatching(ArchitectureModel.CoreAssembly)
            .Should().NotDependOnAny(ArchRuleDefinition.Types().That().ResideInAssemblyMatching(ArchitectureModel.GrasshopperAssembly).Or().ResideInAssemblyMatching(ArchitectureModel.RhinoAssembly))
            .Because(reason: "Rasm is the canonical kernel consumed by GH2 and Rhino boundaries.")
            .Check(architecture: ArchitectureModel.Graph);

    [Fact]
    public void MaterialsDoesNotDependOnRhinoOrGrasshopperBoundaries() =>
        ArchRuleDefinition.Types()
            .That().ResideInAssemblyMatching(ArchitectureModel.MaterialsAssembly)
            .Should().NotDependOnAny(ArchRuleDefinition.Types().That().ResideInAssemblyMatching(ArchitectureModel.GrasshopperAssembly).Or().ResideInAssemblyMatching(ArchitectureModel.RhinoAssembly))
            .Because(reason: "material layout remains a host-free scalar rail.")
            .Check(architecture: ArchitectureModel.Graph);

    [Fact]
    public void DomainDoesNotDependOnVectorOrAnalysisNamespaces() =>
        ArchRuleDefinition.Types()
            .That().ResideInNamespace("Rasm.Domain")
            .Should().NotDependOnAny(ArchRuleDefinition.Types().That().ResideInNamespace("Rasm.Vectors").Or().ResideInNamespace("Rasm.Analysis"))
            .Because(reason: "Domain owns generic geometry primitives; vector and analysis concerns stay downstream.")
            .Check(architecture: ArchitectureModel.Graph);

    [Fact]
    public void VectorsDoesNotDependOnAnalysisNamespace() =>
        ArchRuleDefinition.Types()
            .That().ResideInNamespace("Rasm.Vectors")
            .Should().NotDependOnAny(ArchRuleDefinition.Types().That().ResideInNamespace("Rasm.Analysis"))
            .Because(reason: "Analysis consumes vector rails; vectors must not call back upward.")
            .Check(architecture: ArchitectureModel.Graph);
}
