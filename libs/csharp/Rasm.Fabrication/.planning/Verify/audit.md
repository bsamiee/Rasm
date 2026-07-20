# [RASM_FABRICATION_AUDIT]

`Audit.Preflight` admits one additive `SliceStack`, rasterizes it in one correlated build frame, derives per-layer components and cross-layer lineage, resolves three-dimensional void escape, and emits geometry and process-risk evidence before scan or production commitment. `AuditReceipt` is the sole preflight receipt consumed by scan-path and production gates.

`AuditModality` keys risk policy to the canonical `AdditiveProcess` family, and every `AuditDefect` case names the `AuditRisk` it belongs to, so one filter applies the whole modality policy and the receipt census is total over the risk vocabulary. `AuditEnvelope` binds section frame and build intervals into one relational owner. `RasterWorkspace` owns pooled solid, void, and overhang-reach planes, `ParallelHelper.For2D` owns independent occupancy cells, and QuikGraph owns component, void, lineage, and thin-feature topology.

## [01]-[INDEX]

- [01]-[AUDIT]: owns `AuditRisk`, `AuditModality`, `AuditEnvelope`, `AuditPolicy`, `AuditDefect`, layer/component/risk receipts, pooled raster execution, and `Audit.Preflight`.

## [02]-[AUDIT]

- Owner: `AuditPolicy` composes modality, correlated build envelope, raster demand, one computational-radius ceiling, geometric thresholds, process evidence, support evidence, recoater truth, and evaluation instant. `AuditReceipt` carries component lineage, layer metrics, void escape, bound evidence, process risks, and the exhaustive per-risk census.
- Cases: `AuditModality` carries one row for every canonical `AdditiveProcess`, including electron-beam melting. `AuditRisk` carries one row per risk family, and `AuditDefect.Risk` maps every defect case onto exactly one of them. `LayerProcessEvidence` separates thermal and recoater payload timing. `VoidReceipt` separates enclosed volume from an escaping volume with an equivalent exposed-opening diameter. `AuditDefect` covers open contour, island, split/merge lineage, enclosed medium, restricted escape, unsupported area, thin wall, independent axis bound, heat accumulation, recoater strike, area jump, and unsupported-mass trend.
- Entry: `public static Fin<AuditReceipt> Preflight(SliceStack stack, AuditPolicy policy)` admits the stack channels first, because every later gate indexes them, then accumulates policy, process-evidence, and support admission before opening the pooled kernel. Allocation and graph construction cross one `Try` boundary; every owner disposes before egress.
- Auto: `RasterWorkspace.Index` bounds contours once per layer and supplies row-local candidates to `ParallelHelper.For2D`, which fills disjoint occupancy and overhang-reach cells. `UndirectedGraph<Cell, SEdge<Cell>>.ConnectedComponents` labels each solid plane, the three-dimensional void lattice, and each medial thin-feature witness set. `BidirectionalGraph<ComponentId, SEdge<ComponentId>>.WeaklyConnectedComponents` derives overhang-aware cross-layer genealogies from one prior-layer label index. Support coverage subtracts `SupportPlan.PlanarRows` regions and `TreeNodes` branch capsules; one state fold consumes thermal energy, exposure, optional flow direction, recoat timing and direction, and recoater clearance.
- Receipt: `AuditReceipt` carries `EvaluatedAt`, modality, per-layer metrics, component rows with parents and children, void rows with escape disposition, typed defects, and `Census` keyed by `AuditRisk`. A new defect case reports through the census without a receipt edit; no scalar census can discard an axis or witness.
- Packages: `Rasm.Meshing` (`SliceStack`, `SliceFrame` contract); `Additive/support` (`SupportPlan.PlanarRows`, `SupportPlan.TreeNodes`); `Additive/production` (`AdditiveProcess`, `RecoaterEnvelope`); `Process/owner` (`Loop.Covers`); `Process/faults` (`MachineAxis`); `CommunityToolkit.HighPerformance` (`MemoryOwner<T>`, `Memory2D<T>`, `AsMemory2D`, `ParallelHelper.For2D`, `IAction2D`); QuikGraph (`UndirectedGraph`, `BidirectionalGraph`, `ConnectedComponents`, `WeaklyConnectedComponents`); `NodaTime` (`Instant`, `Duration`); Thinktecture.Runtime.Extensions; LanguageExt.Core.
- Growth: an additive process is one `AuditModality` row referencing `AdditiveProcess`; a risk family is one `AuditRisk` row and its modality memberships; a process signal is one `LayerProcessEvidence` case; a defect is one `AuditDefect` case and its `Risk` arm; a new raster pass consumes the existing workspace.
- Boundary: slicing owns contour topology and elevations; support owns generated support; scan-path owns vector planning; production owns `AdditiveProcess`, `RecoaterEnvelope`, and machine commitment. Audit reads those facts and never regenerates them. Build bounds and raster coordinates share one admitted local frame. `Components`, `Voids`, `Metrics`, `ThinWalls`, `Unsupported`, `BranchCovers`, and the `RasterWorkspace` fill actions are the named numerical or platform kernels. Every violated `MachineAxis`, disconnected thin feature, and void disposition remains an independent row. Clean means every admitted risk family produced no defect.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
using RhinoInterval = Rhino.Geometry.Interval;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class AuditRisk {
    public static readonly AuditRisk Contour = new("contour");
    public static readonly AuditRisk Lineage = new("lineage");
    public static readonly AuditRisk Support = new("support");
    public static readonly AuditRisk Trap = new("trap");
    public static readonly AuditRisk Drainage = new("drainage");
    public static readonly AuditRisk Wall = new("wall");
    public static readonly AuditRisk Bound = new("bound");
    public static readonly AuditRisk Thermal = new("thermal");
    public static readonly AuditRisk Recoater = new("recoater");
}

[SmartEnum<string>]
public sealed partial class TrapMedium {
    public static readonly TrapMedium Resin = new("resin");
    public static readonly TrapMedium Powder = new("powder");
    public static readonly TrapMedium Binder = new("binder");
    public static readonly TrapMedium ProcessGas = new("process-gas");
}

// Contour closure and build-envelope clearance bind every process, so each row carries them; the remaining families
// are the physics the process actually exhibits.
[SmartEnum]
public sealed partial class AuditModality {
    private static readonly Set<AuditRisk> Universal = Set(AuditRisk.Contour, AuditRisk.Bound, AuditRisk.Wall, AuditRisk.Lineage);

    public static readonly AuditModality FusedFilament = new(AdditiveProcess.FusedFilament,
        Universal + Set(AuditRisk.Support), None);
    public static readonly AuditModality PowderBed = new(AdditiveProcess.PowderBed,
        Universal + Set(AuditRisk.Support, AuditRisk.Trap, AuditRisk.Drainage, AuditRisk.Thermal, AuditRisk.Recoater),
        Some(TrapMedium.Powder));
    public static readonly AuditModality ElectronBeam = new(AdditiveProcess.ElectronBeam,
        Universal + Set(AuditRisk.Trap, AuditRisk.Drainage, AuditRisk.Thermal), Some(TrapMedium.Powder));
    public static readonly AuditModality Vat = new(AdditiveProcess.Vat,
        Universal + Set(AuditRisk.Support, AuditRisk.Trap, AuditRisk.Drainage), Some(TrapMedium.Resin));
    public static readonly AuditModality DirectedEnergy = new(AdditiveProcess.DirectedEnergy,
        Universal + Set(AuditRisk.Support, AuditRisk.Trap, AuditRisk.Thermal), Some(TrapMedium.ProcessGas));
    public static readonly AuditModality BinderJet = new(AdditiveProcess.BinderJet,
        Universal + Set(AuditRisk.Trap, AuditRisk.Drainage, AuditRisk.Recoater), Some(TrapMedium.Binder));
    public static readonly AuditModality MaterialJet = new(AdditiveProcess.MaterialJet,
        Universal + Set(AuditRisk.Support, AuditRisk.Trap, AuditRisk.Drainage), Some(TrapMedium.Resin));
    public static readonly AuditModality SheetLamination = new(AdditiveProcess.SheetLamination, Universal, None);

    public AdditiveProcess Process { get; }
    public Set<AuditRisk> Risks { get; }
    public Option<TrapMedium> Medium { get; }
}

[ComplexValueObject]
public sealed partial class AuditEnvelope {
    public Plane Frame { get; }
    public RhinoInterval U { get; }
    public RhinoInterval V { get; }
    public RhinoInterval W { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Plane frame,
        ref RhinoInterval u, ref RhinoInterval v, ref RhinoInterval w) {
        if (!frame.IsValid || !u.IsValid || !v.IsValid || !w.IsValid || u.Length <= 0.0 || v.Length <= 0.0 || w.Length <= 0.0)
            validationError = new ValidationError(message: "audit envelope requires one valid frame and positive local build intervals");
    }

    public Point3d Local(Point3d world) {
        Frame.RemapToPlaneSpace(world, out Point3d local);
        return local;
    }

    public Point3d World(Point3d local) => Frame.PointAt(local.X, local.Y, local.Z);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerProcessEvidence {
    private LayerProcessEvidence() { }

    public sealed record Thermal(int LayerIndex, double EnergyJ, Duration Exposure, Option<Vector2d> FlowDirection) : LayerProcessEvidence;
    public sealed record Recoat(int LayerIndex, Duration Duration, Vector2d Direction) : LayerProcessEvidence;

    public int Layer => Switch(
        thermal: static value => value.LayerIndex,
        recoat: static value => value.LayerIndex);

    public AuditRisk Risk => Switch(
        thermal: static _ => AuditRisk.Thermal,
        recoat: static _ => AuditRisk.Recoater);

    public bool Valid => Switch(
        thermal: static value => value.LayerIndex >= 0 && double.IsFinite(value.EnergyJ) && value.EnergyJ > 0.0
            && value.Exposure > Duration.Zero && value.FlowDirection.ForAll(ValidDirection),
        recoat: static value => value.LayerIndex >= 0 && value.Duration > Duration.Zero && ValidDirection(value.Direction));

    private static bool ValidDirection(Vector2d direction) => direction.IsValid
        && Math.Abs(direction.X) + Math.Abs(direction.Y) > double.Epsilon;
}

[ComplexValueObject]
public sealed partial class AuditThresholds {
    public double CellMm { get; }
    public double MinIslandAreaMm2 { get; }
    public double MinUnsupportedAreaMm2 { get; }
    public double OverhangAngleDeg { get; }
    public double MinWallMm { get; }
    public double MinEscapeDiameterMm { get; }
    public double BoundMarginMm { get; }
    public double MaxHeatIndex { get; }
    public double MaxAreaJumpRatio { get; }
    public double MaxUnsupportedMassTrendKg { get; }
    public double MaxRecoaterLikelihood { get; }
    public double MaterialDensityKgM3 { get; }
    public Duration CoolingTime { get; }
    public long CellCap { get; }
    public int MaximumRadiusCells { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double cellMm,
        ref double minIslandAreaMm2, ref double minUnsupportedAreaMm2, ref double overhangAngleDeg,
        ref double minWallMm, ref double minEscapeDiameterMm, ref double boundMarginMm, ref double maxHeatIndex,
        ref double maxAreaJumpRatio, ref double maxUnsupportedMassTrendKg, ref double maxRecoaterLikelihood,
        ref double materialDensityKgM3, ref Duration coolingTime, ref long cellCap, ref int maximumRadiusCells) {
        double[] finite = [cellMm, minIslandAreaMm2, minUnsupportedAreaMm2, overhangAngleDeg, minWallMm,
            minEscapeDiameterMm, boundMarginMm, maxHeatIndex, maxAreaJumpRatio, maxUnsupportedMassTrendKg,
            maxRecoaterLikelihood, materialDensityKgM3];
        long diameter = (2L * maximumRadiusCells) + 1L;
        bool positive = cellMm > 0.0 && minIslandAreaMm2 >= 0.0 && minUnsupportedAreaMm2 >= 0.0
            && overhangAngleDeg is > 0.0 and < 90.0 && minWallMm >= cellMm && minEscapeDiameterMm >= cellMm
            && boundMarginMm >= 0.0 && maxHeatIndex > 0.0 && maxAreaJumpRatio >= 0.0
            && maxUnsupportedMassTrendKg >= 0.0 && maxRecoaterLikelihood is >= 0.0 and <= 1.0
            && materialDensityKgM3 > 0.0 && coolingTime > Duration.Zero && cellCap is > 0L and <= int.MaxValue
            && maximumRadiusCells is > 0 and <= ((int.MaxValue - 1) / 2) && diameter * diameter <= cellCap;
        if (!finite.ForAll(double.IsFinite) || !positive)
            validationError = new ValidationError(message: "audit thresholds require finite physical limits, positive cooling, and bounded raster demand");
    }
}

[ComplexValueObject]
public sealed partial class AuditPolicy {
    public AuditModality Modality { get; }
    public AuditEnvelope Envelope { get; }
    public AuditThresholds Thresholds { get; }
    public Option<SupportPlan> Supports { get; }
    public Option<RecoaterEnvelope> Recoater { get; }
    public Seq<LayerProcessEvidence> Process { get; }
    public Instant EvaluatedAt { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref AuditModality modality,
        ref AuditEnvelope envelope, ref AuditThresholds thresholds, ref Option<SupportPlan> supports,
        ref Option<RecoaterEnvelope> recoater, ref Seq<LayerProcessEvidence> process, ref Instant evaluatedAt) {
        bool rows = modality is not null && process.ForAll(row => row is not null && row.Valid
            && modality.Risks.Contains(row.Risk)
            && process.Count(candidate => candidate is not null && candidate.Layer == row.Layer
                && candidate.Risk == row.Risk) == 1);
        bool required = modality is not null && (!modality.Risks.Contains(AuditRisk.Recoater) || recoater.IsSome);
        if (modality is null || envelope is null || thresholds is null || supports.Exists(static value => value is null)
            || recoater.Exists(static value => value is null) || !rows || !required)
            validationError = new ValidationError(message: "audit policy requires coherent modality, envelope, thresholds, supports, recoater, and process evidence");
    }
}

public readonly record struct Cell(int Layer, int Row, int Column);
public readonly record struct ComponentId(int Layer, int Label);

public sealed record ComponentReceipt(ComponentId Id, int Cells, double AreaMm2, Point3d Witness,
    Seq<ComponentId> Parents, Seq<ComponentId> Children, int Genealogy);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VoidReceipt {
    private VoidReceipt() { }

    public sealed record Enclosed(int Id, int Cells, double VolumeMm3, Point3d Witness) : VoidReceipt;
    public sealed record Escaping(int Id, int Cells, double VolumeMm3, double EquivalentOpeningDiameterMm, Point3d Witness) : VoidReceipt;
}

public sealed record LayerMetric(int Layer, double AreaMm2, double PerimeterMm, double UnsupportedAreaMm2,
    Option<Point3d> UnsupportedAt, double UnsupportedMassKg, double UnsupportedMassTrendKg,
    double GasExposureMm, double RecoatExposureMm, Option<Point3d> RecoaterAt,
    double HeatIndex, double AreaJumpRatio, double RecoaterLikelihood);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AuditDefect {
    private AuditDefect() { }

    public sealed record OpenContour(int Layer, int Count) : AuditDefect;
    public sealed record Island(ComponentId Component, double AreaMm2, Point3d At) : AuditDefect;
    public sealed record LineageSplit(ComponentId Component, Seq<ComponentId> Children) : AuditDefect;
    public sealed record LineageMerge(ComponentId Component, Seq<ComponentId> Parents) : AuditDefect;
    public sealed record EnclosedMedium(int Void, TrapMedium Medium, double VolumeMm3, Point3d At) : AuditDefect;
    public sealed record EscapeRestriction(int Void, double MouthMm, double RequiredMm, Point3d At) : AuditDefect;
    public sealed record UnsupportedArea(int Layer, double AreaMm2, Point3d At) : AuditDefect;
    public sealed record ThinWall(int Layer, double ThicknessUpperBoundMm, Point3d At) : AuditDefect;
    public sealed record TouchingBound(int Layer, MachineAxis Axis, double ClearanceMm, Point3d At) : AuditDefect;
    public sealed record HeatAccumulation(int Layer, double Index, double Limit) : AuditDefect;
    public sealed record RecoaterStrike(int Layer, double Likelihood, double ClearanceMm, Point3d At) : AuditDefect;
    public sealed record AreaJump(int Layer, double Ratio, double Limit) : AuditDefect;
    public sealed record UnsupportedMassTrend(int Layer, double Kilograms, double Limit) : AuditDefect;

    // One arm per case is the whole modality policy: production is unconditional and this projection decides which
    // defects the process in hand can actually exhibit.
    public AuditRisk Risk => Switch(
        openContour: static _ => AuditRisk.Contour,
        island: static _ => AuditRisk.Lineage,
        lineageSplit: static _ => AuditRisk.Lineage,
        lineageMerge: static _ => AuditRisk.Lineage,
        enclosedMedium: static _ => AuditRisk.Trap,
        escapeRestriction: static _ => AuditRisk.Drainage,
        unsupportedArea: static _ => AuditRisk.Support,
        thinWall: static _ => AuditRisk.Wall,
        touchingBound: static _ => AuditRisk.Bound,
        heatAccumulation: static _ => AuditRisk.Thermal,
        recoaterStrike: static _ => AuditRisk.Recoater,
        areaJump: static _ => AuditRisk.Recoater,
        unsupportedMassTrend: static _ => AuditRisk.Recoater);
}

public sealed record AuditReceipt(
    Instant EvaluatedAt,
    AuditModality Modality,
    int Layers,
    Seq<ComponentReceipt> Components,
    Seq<VoidReceipt> Voids,
    Seq<LayerMetric> Metrics,
    Seq<AuditDefect> Defects) {
    public bool Clean => Defects.IsEmpty;

    public Map<AuditRisk, int> Census => Defects.Fold(
        toSeq(AuditRisk.Items).Fold(Map<AuditRisk, int>(), static (counts, risk) => counts.AddOrUpdate(risk, 0)),
        static (counts, defect) => counts.AddOrUpdate(defect.Risk, counts.Find(defect.Risk).IfNone(0) + 1));

    public int Count(AuditRisk risk) => Census.Find(risk).IfNone(0);
}

internal sealed record AdmittedAudit(SliceStack Stack, AuditPolicy Policy, Seq<Seq<Loop>> Layers, RasterGrid Grid);

[ComplexValueObject]
internal sealed partial class RasterGrid {
    public double MinU { get; }
    public double MinV { get; }
    public double CellMm { get; }
    public int Rows { get; }
    public int Columns { get; }

    public int CellCount => Rows * Columns;
    public double CellAreaMm2 => CellMm * CellMm;
    public Point3d Local(int row, int column, double w) => new(MinU + (column + 0.5) * CellMm, MinV + (row + 0.5) * CellMm, w);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double minU, ref double minV,
        ref double cellMm, ref int rows, ref int columns) {
        if (!double.IsFinite(minU) || !double.IsFinite(minV) || !double.IsFinite(cellMm) || cellMm <= 0.0 || rows <= 0 || columns <= 0)
            validationError = new ValidationError(message: "raster grid requires finite origin, positive pitch, rows, and columns");
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Audit {
    public static Fin<AuditReceipt> Preflight(SliceStack stack, AuditPolicy policy) =>
        from admitted in Admit(stack, policy)
        from receipt in Try.lift(() => Run(admitted)).Run()
            .MapFail(error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"audit:kernel:{error.Message}").ToError())
        from result in receipt
        select result;

    // Channel admission binds first because every accumulating gate below indexes the channels it proves; the three
    // independent gates then report together.
    private static Fin<AdmittedAudit> Admit(SliceStack stack, AuditPolicy policy) =>
        from admittedStack in Optional(stack).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "audit:stack").ToError())
        from admittedPolicy in Optional(policy).ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "audit:policy").ToError())
        from _channels in StackGate(admittedStack)
        from _gates in (DemandGate(admittedStack, admittedPolicy), ProcessGate(admittedStack, admittedPolicy),
            SupportGate(admittedStack, admittedPolicy))
            .Apply(static (_, _, _) => unit).As().ToFin()
        from context in Context.Millimeters().ToFin()
        from loops in Range(0, admittedStack.LayerCount)
            .Map(layer => Loops(admittedStack, layer, context, admittedPolicy.Envelope)).TraverseM(identity).As()
        from grid in Grid(admittedPolicy)
        select new AdmittedAudit(admittedStack, admittedPolicy, loops, grid);

    private static Fin<Unit> StackGate(SliceStack stack) {
        bool channels = stack.LayerCount > 1 && stack.ContourCount > 0
            && stack.Elevations.Length == stack.LayerCount
            && stack.LayerPtr.Length == stack.LayerCount + 1
            && stack.ContourPtr.Length == stack.ContourCount + 1
            && stack.X.Length == stack.Y.Length && stack.X.Length == stack.Z.Length
            && stack.ContourPtr.Length > 0 && stack.LayerPtr[0] == 0 && stack.LayerPtr[^1] == stack.ContourCount
            && stack.ContourPtr[0] == 0 && stack.ContourPtr[^1] == stack.X.Length
            && stack.Parent.Length == stack.ContourCount && stack.ChildPtr.Length == stack.ContourCount + 1
            && stack.ChildPtr[0] == 0 && stack.ChildPtr[^1] == stack.Children.Length
            && Range(1, stack.LayerPtr.Length - 1).ForAll(index => stack.LayerPtr[index] >= stack.LayerPtr[index - 1])
            && Range(1, stack.ContourPtr.Length - 1).ForAll(index => stack.ContourPtr[index] > stack.ContourPtr[index - 1])
            && Range(1, stack.ChildPtr.Length - 1).ForAll(index => stack.ChildPtr[index] >= stack.ChildPtr[index - 1])
            && stack.Parent.ForAll(parent => parent >= -1 && parent < stack.ContourCount)
            && stack.Children.ForAll(child => child >= 0 && child < stack.ContourCount)
            && stack.Open.ForAll(contour => contour >= 0 && contour < stack.ContourCount)
            && Range(1, Math.Max(0, stack.Open.Length - 1)).ForAll(index => stack.Open[index] > stack.Open[index - 1])
            && Range(0, stack.ContourCount).ForAll(contour => stack.ContourPtr[contour + 1] - stack.ContourPtr[contour]
                >= (stack.IsOpen(contour) ? 2 : 3))
            && stack.Elevations.ForAll(double.IsFinite) && stack.X.ForAll(double.IsFinite)
            && stack.Y.ForAll(double.IsFinite) && stack.Z.ForAll(double.IsFinite);
        bool elevations = Range(1, Math.Max(0, stack.LayerCount - 1))
            .ForAll(index => stack.Elevations[index] > stack.Elevations[index - 1]);
        return channels && elevations
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "audit:stack-channels").ToError());
    }

    private static K<Validation<Error>, Unit> DemandGate(SliceStack stack, AuditPolicy policy) {
        bool extent = policy.Envelope.W.Contains(stack.Elevations[0]) && policy.Envelope.W.Contains(stack.Elevations[^1]);
        bool correlated = Range(0, stack.LayerCount).ForAll(layer => stack.LayerAt(layer)
            .ForAll(chain => chain.Points.ForAll(point => Math.Abs(
                policy.Envelope.Local(point).Z - stack.Elevations[layer]) <= Rhino.RhinoMath.ZeroTolerance)));
        double rows = Math.Ceiling(policy.Envelope.V.Length / policy.Thresholds.CellMm);
        double columns = Math.Ceiling(policy.Envelope.U.Length / policy.Thresholds.CellMm);
        double wallRadius = Math.Ceiling(policy.Thresholds.MinWallMm / policy.Thresholds.CellMm);
        bool radii = BoundedRadius(wallRadius, policy.Thresholds.MaximumRadiusCells)
            && Range(1, Math.Max(0, stack.LayerCount - 1)).ForAll(layer =>
                BoundedRadius(OverhangRadiusDemand(stack, policy, layer), policy.Thresholds.MaximumRadiusCells));
        // Solid, void, and overhang-reach planes share the admitted cell budget, so the census counts all three.
        bool demand = rows is >= 1.0 and <= int.MaxValue && columns is >= 1.0 and <= int.MaxValue
            && rows * columns * stack.LayerCount * RasterWorkspace.Planes <= policy.Thresholds.CellCap;
        return (extent && correlated && demand && radii
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "audit:policy-demand").ToError())).ToValidation();
    }

    private static K<Validation<Error>, Unit> ProcessGate(SliceStack stack, AuditPolicy policy) {
        bool covered = Seq(AuditRisk.Thermal, AuditRisk.Recoater).ForAll(risk =>
            !policy.Modality.Risks.Contains(risk) || Range(0, stack.LayerCount).ForAll(layer =>
                policy.Process.Exists(value => value.Risk == risk && value.Layer == layer)));
        bool bounded = policy.Process.ForAll(row => row.Layer >= 0 && row.Layer < stack.LayerCount);
        return (covered && bounded
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "audit:process-evidence").ToError())).ToValidation();
    }

    private static K<Validation<Error>, Unit> SupportGate(SliceStack stack, AuditPolicy policy) {
        bool rows = policy.Supports.ForAll(plan => plan.PlanarRows.ForAll(layer => layer.Layer >= 0 && layer.Layer < stack.LayerCount));
        return (rows ? Fin.Succ(unit) : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "audit:support-evidence").ToError())).ToValidation();
    }

    private static Fin<Seq<Loop>> Loops(SliceStack stack, int layer, Context context, AuditEnvelope envelope) =>
        stack.LayerAt(layer).Filter(static chain => chain.Closed).Map(chain => {
            Seq<Point3d> points = toSeq(chain.Points);
            Seq<Point3d> open = points.Count > 1 && points[0].EpsilonEquals(points[^1], Rhino.RhinoMath.ZeroTolerance)
                ? points.Init : points;
            return Loop.Admit(open.Map(envelope.Local).ToArr(), true, Arr<double>(), context);
        }).TraverseM(identity).As();

    private static Fin<RasterGrid> Grid(AuditPolicy policy) {
        int rows = (int)Math.Ceiling(policy.Envelope.V.Length / policy.Thresholds.CellMm);
        int columns = (int)Math.Ceiling(policy.Envelope.U.Length / policy.Thresholds.CellMm);
        ValidationError? error = RasterGrid.Validate(policy.Envelope.U.Min, policy.Envelope.V.Min,
            policy.Thresholds.CellMm, rows, columns, out RasterGrid? grid);
        return error is null && grid is not null
            ? Fin.Succ(grid)
            : Fin.Fail<RasterGrid>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "audit:grid").ToError());
    }

    private static Fin<AuditReceipt> Run(AdmittedAudit admitted) {
        using RasterWorkspace workspace = RasterWorkspace.Allocate(admitted.Stack.LayerCount, admitted.Grid);
        workspace.Fill(admitted);
        Seq<ComponentReceipt> components = Components(workspace, admitted);
        Seq<VoidReceipt> voids = admitted.Policy.Modality.Risks.Contains(AuditRisk.Trap)
            || admitted.Policy.Modality.Risks.Contains(AuditRisk.Drainage)
            ? Voids(workspace, admitted) : Seq<VoidReceipt>();
        Seq<LayerMetric> metrics = Metrics(workspace, admitted);
        Seq<AuditDefect> defects = Defects(workspace, admitted, components, voids, metrics)
            .Filter(defect => admitted.Policy.Modality.Risks.Contains(defect.Risk));
        return Fin.Succ(new AuditReceipt(admitted.Policy.EvaluatedAt, admitted.Policy.Modality,
            admitted.Stack.LayerCount, components, voids, metrics, defects));
    }

    private static Seq<ComponentReceipt> Components(RasterWorkspace workspace, AdmittedAudit admitted) {
        Seq<(ComponentId Id, Seq<Cell> Cells, Point3d Witness)> seeds =
            Range(0, admitted.Stack.LayerCount).Bind(layer => ComponentsAt(workspace, admitted, layer));
        BidirectionalGraph<ComponentId, SEdge<ComponentId>> lineage = new(allowParallelEdges: false);
        lineage.AddVertexRange(seeds.Map(static seed => seed.Id));
        // One label index per prior layer plus one disc walk per child cell replaces a per-component-pair rescan.
        Range(1, admitted.Stack.LayerCount - 1).Iter(layer => {
            Seq<(int Row, int Column)> disc = Disc(OverhangRadius(admitted, layer));
            Map<(int Row, int Column), ComponentId> prior = seeds.Filter(seed => seed.Id.Layer == layer - 1)
                .Bind(static seed => seed.Cells.Map(cell => ((cell.Row, cell.Column), seed.Id)))
                .Fold(Map<(int Row, int Column), ComponentId>(), static (index, row) => index.AddOrUpdate(row.Item1, row.Item2));
            seeds.Filter(seed => seed.Id.Layer == layer)
                .Bind(child => child.Cells
                    .Bind(cell => disc.Choose(step => prior.Find((cell.Row + step.Row, cell.Column + step.Column))))
                    .Distinct()
                    .Map(parent => new SEdge<ComponentId>(parent, child.Id)))
                .Iter(edge => lineage.AddEdge(edge));
        });
        Dictionary<ComponentId, int> genealogies = [];
        _ = lineage.WeaklyConnectedComponents(genealogies);
        return seeds.Map(seed => new ComponentReceipt(seed.Id, seed.Cells.Count,
            seed.Cells.Count * admitted.Grid.CellAreaMm2, seed.Witness,
            lineage.InEdges(seed.Id).Map(static edge => edge.Source).ToSeq(),
            lineage.OutEdges(seed.Id).Map(static edge => edge.Target).ToSeq(), genealogies[seed.Id]));
    }

    private static Seq<(ComponentId Id, Seq<Cell> Cells, Point3d Witness)> ComponentsAt(
        RasterWorkspace workspace, AdmittedAudit admitted, int layer) {
        UndirectedGraph<Cell, SEdge<Cell>> graph = GridGraph(workspace.Solid(layer), layer, admitted.Grid);
        Dictionary<Cell, int> labels = [];
        _ = graph.ConnectedComponents(labels);
        return toSeq(labels.GroupBy(static row => row.Value)).Map(group => {
            Seq<Cell> cells = Ordered(toSeq(group.Select(static row => row.Key)));
            Cell witness = cells.Head.IfNone(default(Cell));
            return (new ComponentId(layer, group.Key), cells,
                admitted.Policy.Envelope.World(admitted.Grid.Local(
                    witness.Row, witness.Column, admitted.Stack.Elevations[layer])));
        });
    }

    private static Seq<VoidReceipt> Voids(RasterWorkspace workspace, AdmittedAudit admitted) {
        UndirectedGraph<Cell, SEdge<Cell>> graph = VolumeGraph(workspace, admitted.Grid, admitted.Stack.LayerCount);
        Dictionary<Cell, int> labels = [];
        _ = graph.ConnectedComponents(labels);
        return toSeq(labels.GroupBy(static row => row.Value)).Map(group => {
            Seq<Cell> cells = Ordered(toSeq(group.Select(static row => row.Key)));
            Seq<Cell> boundary = cells.Filter(cell => OnBoundary(cell, admitted));
            double volume = cells.Map(cell => admitted.Grid.CellAreaMm2 * LayerHeight(admitted.Stack, cell.Layer)).Sum();
            Cell witness = boundary.Head.IfNone(cells.Head.IfNone(default(Cell)));
            Point3d at = admitted.Policy.Envelope.World(admitted.Grid.Local(
                witness.Row, witness.Column, admitted.Stack.Elevations[witness.Layer]));
            return boundary.IsEmpty
                ? (VoidReceipt)new VoidReceipt.Enclosed(group.Key, cells.Count, volume, at)
                : new VoidReceipt.Escaping(group.Key, cells.Count, volume,
                    EquivalentOpeningDiameter(boundary, admitted.Stack, admitted.Grid), at);
        });
    }

    private static bool OnBoundary(Cell cell, AdmittedAudit admitted) =>
        cell.Layer == 0 || cell.Layer == admitted.Stack.LayerCount - 1 || cell.Row == 0 || cell.Column == 0
        || cell.Row == admitted.Grid.Rows - 1 || cell.Column == admitted.Grid.Columns - 1;

    private static Seq<Cell> Ordered(Seq<Cell> cells) => cells
        .OrderBy(static cell => cell.Layer).ThenBy(static cell => cell.Row).ThenBy(static cell => cell.Column).ToSeq();

    private static Seq<LayerMetric> Metrics(RasterWorkspace workspace, AdmittedAudit admitted) =>
        Range(0, admitted.Stack.LayerCount).Fold(
            (Heat: 0.0, PreviousArea: 0.0, PreviousMass: 0.0, Rows: Seq<LayerMetric>()),
            (state, layer) => {
            Memory2D<byte> solid = workspace.Solid(layer);
            int cells = Count(solid, admitted.Grid);
            double area = cells * admitted.Grid.CellAreaMm2;
            double perimeter = Perimeter(solid, admitted.Grid);
            (double unsupported, Option<Point3d> unsupportedAt) = layer == 0
                || !admitted.Policy.Modality.Risks.Contains(AuditRisk.Support)
                ? (0.0, Option<Point3d>.None) : Unsupported(workspace, admitted, layer);
            double height = LayerHeight(admitted.Stack, layer);
            double mass = unsupported * height * admitted.Policy.Thresholds.MaterialDensityKgM3 / 1e9;
            Option<LayerProcessEvidence.Thermal> thermal = admitted.Policy.Process
                .Filter(value => value is LayerProcessEvidence.Thermal row && row.LayerIndex == layer)
                .Map(static value => (LayerProcessEvidence.Thermal)value).Head;
            Option<LayerProcessEvidence.Recoat> recoat = admitted.Policy.Process
                .Filter(value => value is LayerProcessEvidence.Recoat row && row.LayerIndex == layer)
                .Map(static value => (LayerProcessEvidence.Recoat)value).Head;
            double recoatSeconds = recoat.Map(static value => value.Duration.TotalSeconds).IfNone(0.0);
            double energy = thermal.Map(static value => value.EnergyJ).IfNone(0.0);
            double exposureSeconds = thermal.Map(static value => value.Exposure.TotalSeconds).IfNone(0.0);
            double gasExposure = thermal.Bind(static value => value.FlowDirection)
                .Map(value => DirectionalExposure(solid, value, admitted.Grid).Millimeters).IfNone(0.0);
            (double recoatExposure, Option<(int Row, int Column)> recoatWitness) = recoat
                .Map(value => DirectionalExposure(solid, value.Direction, admitted.Grid))
                .IfNone((Millimeters: 0.0, Witness: Option<(int Row, int Column)>.None));
            Option<Point3d> recoaterAt = recoatWitness.Map(cell => admitted.Policy.Envelope.World(
                admitted.Grid.Local(cell.Row, cell.Column, admitted.Stack.Elevations[layer])));
            double decay = Math.Exp(-recoatSeconds / admitted.Policy.Thresholds.CoolingTime.TotalSeconds);
            double ventilation = 1.0 + gasExposure / Math.Max(perimeter, admitted.Grid.CellMm);
            double powerDensity = energy / Math.Max(exposureSeconds, double.Epsilon)
                / Math.Max(area, admitted.Grid.CellAreaMm2);
            double heat = state.Heat * decay + powerDensity / ventilation;
            double jump = state.PreviousArea <= 0.0 ? 0.0 : Math.Max(0.0, area - state.PreviousArea) / state.PreviousArea;
            double trend = Math.Max(0.0, mass - state.PreviousMass);
            double trendRatio = trend / Math.Max(admitted.Policy.Thresholds.MaxUnsupportedMassTrendKg, double.Epsilon);
            double exposureRatio = recoatExposure / Math.Max(perimeter, admitted.Grid.CellMm);
            double clearanceFactor = admitted.Policy.Recoater
                .Map(value => admitted.Grid.CellMm / Math.Max(value.Clearance.Millimeters, admitted.Grid.CellMm)).IfNone(0.0);
            double recoater = Math.Clamp((jump + trendRatio + exposureRatio) * clearanceFactor, 0.0, 1.0);
            return (Heat: heat, PreviousArea: area, PreviousMass: mass,
                Rows: state.Rows.Add(new LayerMetric(layer, area, perimeter, unsupported, unsupportedAt, mass, trend,
                    gasExposure, recoatExposure, recoaterAt, heat, jump, recoater)));
        }).Rows;

    // Defect production is unconditional; `Run` applies the modality policy through one `AuditDefect.Risk` filter, so
    // no family carries its own guard and no new case can silently escape the policy.
    private static Seq<AuditDefect> Defects(RasterWorkspace workspace, AdmittedAudit admitted,
        Seq<ComponentReceipt> components, Seq<VoidReceipt> voids, Seq<LayerMetric> metrics) =>
        Range(0, admitted.Stack.LayerCount)
            .Map(layer => (Layer: layer, Count: admitted.Stack.LayerAt(layer).Count(static chain => !chain.Closed)))
            .Filter(static row => row.Count > 0)
            .Map(static row => (AuditDefect)new AuditDefect.OpenContour(row.Layer, row.Count))
        .Concat(components.Bind(component =>
            (component.Id.Layer > 0 && component.Parents.IsEmpty
                && component.AreaMm2 >= admitted.Policy.Thresholds.MinIslandAreaMm2
                ? Seq<AuditDefect>(new AuditDefect.Island(component.Id, component.AreaMm2, component.Witness)) : Seq<AuditDefect>())
            .Concat(component.Parents.Count > 1
                ? Seq<AuditDefect>(new AuditDefect.LineageMerge(component.Id, component.Parents)) : Seq<AuditDefect>())
            .Concat(component.Children.Count > 1
                ? Seq<AuditDefect>(new AuditDefect.LineageSplit(component.Id, component.Children)) : Seq<AuditDefect>())))
        .Concat(voids.Bind(value => admitted.Policy.Modality.Medium.Map(medium => value.Switch(
            state: medium,
            enclosed: static (trapped, enclosed) => Seq<AuditDefect>(new AuditDefect.EnclosedMedium(
                enclosed.Id, trapped, enclosed.VolumeMm3, enclosed.Witness)),
            escaping: (_, escaping) => escaping.EquivalentOpeningDiameterMm < admitted.Policy.Thresholds.MinEscapeDiameterMm
                ? Seq<AuditDefect>(new AuditDefect.EscapeRestriction(escaping.Id, escaping.EquivalentOpeningDiameterMm,
                    admitted.Policy.Thresholds.MinEscapeDiameterMm, escaping.Witness))
                : Seq<AuditDefect>())).IfNone(Seq<AuditDefect>())))
        .Concat(metrics.Bind(metric => Risks(admitted, metric)))
        .Concat(admitted.Policy.Modality.Risks.Contains(AuditRisk.Wall) ? ThinWalls(workspace, admitted) : Seq<AuditDefect>())
        .Concat(Bounds(admitted));

    private static Seq<AuditDefect> Risks(AdmittedAudit admitted, LayerMetric metric) =>
        (metric.UnsupportedAreaMm2 >= admitted.Policy.Thresholds.MinUnsupportedAreaMm2
            ? metric.UnsupportedAt.Map(at => Seq<AuditDefect>(new AuditDefect.UnsupportedArea(
                metric.Layer, metric.UnsupportedAreaMm2, at))).IfNone(Seq<AuditDefect>())
            : Seq<AuditDefect>())
        .Concat(metric.HeatIndex > admitted.Policy.Thresholds.MaxHeatIndex
            ? Seq<AuditDefect>(new AuditDefect.HeatAccumulation(metric.Layer, metric.HeatIndex,
                admitted.Policy.Thresholds.MaxHeatIndex)) : Seq<AuditDefect>())
        .Concat(metric.AreaJumpRatio > admitted.Policy.Thresholds.MaxAreaJumpRatio
            ? Seq<AuditDefect>(new AuditDefect.AreaJump(metric.Layer, metric.AreaJumpRatio,
                admitted.Policy.Thresholds.MaxAreaJumpRatio)) : Seq<AuditDefect>())
        .Concat(metric.UnsupportedMassTrendKg > admitted.Policy.Thresholds.MaxUnsupportedMassTrendKg
            ? Seq<AuditDefect>(new AuditDefect.UnsupportedMassTrend(metric.Layer, metric.UnsupportedMassTrendKg,
                admitted.Policy.Thresholds.MaxUnsupportedMassTrendKg)) : Seq<AuditDefect>())
        .Concat(metric.RecoaterLikelihood > admitted.Policy.Thresholds.MaxRecoaterLikelihood
            ? metric.RecoaterAt.Map(at => Seq<AuditDefect>(new AuditDefect.RecoaterStrike(
                metric.Layer, metric.RecoaterLikelihood,
                admitted.Policy.Recoater.Map(static value => value.Clearance.Millimeters).IfNone(0.0), at)))
                .IfNone(Seq<AuditDefect>())
            : Seq<AuditDefect>());

    private static Seq<AuditDefect> Bounds(AdmittedAudit admitted) {
        Seq<(int Layer, Point3d Local)> points = Range(0, admitted.Stack.LayerCount).Bind(layer =>
            admitted.Stack.LayerAt(layer).Bind(static chain => toSeq(chain.Points))
                .Map(point => (Layer: layer, Local: admitted.Policy.Envelope.Local(point))));
        Seq<AuditDefect> planar = toSeq(points.GroupBy(static row => row.Layer)).Bind(group => Seq(
                (Axis: MachineAxis.X, Interval: admitted.Policy.Envelope.U),
                (Axis: MachineAxis.Y, Interval: admitted.Policy.Envelope.V))
            .Choose(row => toSeq(group)
                .OrderBy(point => Clearance(point.Local, row.Axis, row.Interval)).ToSeq().Head
                .Filter(point => Clearance(point.Local, row.Axis, row.Interval) < admitted.Policy.Thresholds.BoundMarginMm)
                .Map(point => (AuditDefect)new AuditDefect.TouchingBound(group.Key, row.Axis,
                    Clearance(point.Local, row.Axis, row.Interval), admitted.Policy.Envelope.World(point.Local)))));
        Seq<AuditDefect> growth = Seq(
                (Extreme: points.OrderBy(static row => row.Local.Z).ToSeq().Head, Sign: 1.0, Bound: admitted.Policy.Envelope.W.Min),
                (Extreme: points.OrderByDescending(static row => row.Local.Z).ToSeq().Head, Sign: -1.0, Bound: admitted.Policy.Envelope.W.Max))
            .Choose(row => row.Extreme
                .Filter(value => row.Sign * (value.Local.Z - row.Bound) < admitted.Policy.Thresholds.BoundMarginMm)
                .Map(value => (AuditDefect)new AuditDefect.TouchingBound(value.Layer, MachineAxis.Z,
                    row.Sign * (value.Local.Z - row.Bound), admitted.Policy.Envelope.World(value.Local))));
        return planar.Concat(growth);
    }

    private static double Clearance(Point3d local, MachineAxis axis, RhinoInterval interval) {
        double value = axis == MachineAxis.X ? local.X : local.Y;
        return Math.Min(value - interval.Min, interval.Max - value);
    }

    private static Seq<AuditDefect> ThinWalls(RasterWorkspace workspace, AdmittedAudit admitted) {
        int radius = (int)Math.Ceiling(admitted.Policy.Thresholds.MinWallMm / admitted.Grid.CellMm);
        Seq<(int Row, int Column)> directions = Range(-radius, radius * 2 + 1)
            .Bind(row => Range(-radius, radius * 2 + 1).Map(column => (Row: row, Column: column)))
            .Filter(static value => (value.Column > 0 || value.Column == 0 && value.Row > 0)
                && Coprime(Math.Abs(value.Row), Math.Abs(value.Column)));
        return Range(0, admitted.Stack.LayerCount).Bind(layer => {
            Memory2D<byte> solid = workspace.Solid(layer);
            Dictionary<Cell, double> candidates = [];
            for (int row = 0; row < admitted.Grid.Rows; row++)
                for (int column = 0; column < admitted.Grid.Columns; column++)
                    if (solid.Span[row, column] == 1
                        && DirectionalThickness(solid, row, column, directions, admitted.Grid) is double thickness
                        && thickness < admitted.Policy.Thresholds.MinWallMm
                        && ThicknessRidge(solid, row, column, thickness, directions, admitted.Grid))
                        candidates[new Cell(layer, row, column)] = thickness;
            UndirectedGraph<Cell, SEdge<Cell>> graph = new(allowParallelEdges: false);
            graph.AddVertexRange(candidates.Keys);
            foreach (Cell cell in candidates.Keys)
                Seq(new Cell(layer, cell.Row - 1, cell.Column - 1), new Cell(layer, cell.Row - 1, cell.Column),
                        new Cell(layer, cell.Row - 1, cell.Column + 1), new Cell(layer, cell.Row, cell.Column - 1))
                    .Filter(candidates.ContainsKey).Iter(candidate => graph.AddEdge(new SEdge<Cell>(cell, candidate)));
            Dictionary<Cell, int> labels = [];
            _ = graph.ConnectedComponents(labels);
            return toSeq(labels.GroupBy(static row => row.Value)).Map(group => {
                Seq<Cell> cells = toSeq(group.Select(static row => row.Key));
                Cell witness = cells.OrderBy(cell => candidates[cell]).ThenBy(static cell => cell.Row)
                    .ThenBy(static cell => cell.Column).ToSeq().Head.IfNone(default(Cell));
                Point3d local = admitted.Grid.Local(witness.Row, witness.Column, admitted.Stack.Elevations[layer]);
                return (AuditDefect)new AuditDefect.ThinWall(layer, candidates[witness], admitted.Policy.Envelope.World(local));
            });
        });
    }

    private static double DirectionalThickness(Memory2D<byte> solid, int row, int column,
        Seq<(int Row, int Column)> directions, RasterGrid grid) => directions.Map(direction => {
            int backward = 0, forward = 0;
            while (!Empty(solid,
                row - (((long)backward + 1L) * direction.Row),
                column - (((long)backward + 1L) * direction.Column), grid)) backward++;
            while (!Empty(solid,
                row + (((long)forward + 1L) * direction.Row),
                column + (((long)forward + 1L) * direction.Column), grid)) forward++;
            double squared = ((double)direction.Row * direction.Row) + ((double)direction.Column * direction.Column);
            double step = Math.Sqrt(squared) * grid.CellMm;
            double cellChord = (Math.Abs(direction.Row) + Math.Abs(direction.Column))
                / Math.Sqrt(squared) * grid.CellMm;
            return ((double)backward + forward) * step + cellChord;
        }).Min();

    private static bool ThicknessRidge(Memory2D<byte> solid, int row, int column, double thickness,
        Seq<(int Row, int Column)> directions, RasterGrid grid) =>
        Range(-1, 3).Bind(dr => Range(-1, 3).Map(dc => (Row: row + dr, Column: column + dc)))
            .Filter(value => value.Row != row || value.Column != column)
            .Filter(value => !Empty(solid, value.Row, value.Column, grid))
            .ForAll(value => DirectionalThickness(solid, value.Row, value.Column, directions, grid)
                <= thickness + grid.CellMm * 1e-9);

    private static bool Coprime(int left, int right) {
        while (right != 0) (left, right) = (right, left % right);
        return left == 1;
    }

    private static bool Empty(Memory2D<byte> solid, long row, long column, RasterGrid grid) =>
        row < 0L || column < 0L || row >= grid.Rows || column >= grid.Columns || solid.Span[(int)row, (int)column] == 0;

    // Prior-layer overhang reach is rasterized, so support coverage is one plane probe per solid cell.
    private static (double AreaMm2, Option<Point3d> Witness) Unsupported(
        RasterWorkspace workspace, AdmittedAudit admitted, int layer) {
        Memory2D<byte> current = workspace.Solid(layer), reach = workspace.Reach(layer - 1);
        Option<SupportLayer> support = admitted.Policy.Supports.Bind(plan => plan.PlanarRows.Find(row => row.Layer == layer));
        int count = 0;
        Option<Point3d> witness = None;
        for (int row = 0; row < admitted.Grid.Rows; row++)
            for (int column = 0; column < admitted.Grid.Columns; column++)
                if (current.Span[row, column] == 1 && reach.Span[row, column] == 0) {
                    Point3d world = admitted.Policy.Envelope.World(admitted.Grid.Local(row, column, admitted.Stack.Elevations[layer]));
                    bool planar = support.Exists(value => value.Sparse.Covers(world)
                        || value.Interface.Covers(world) || value.Contact.Covers(world));
                    bool tree = admitted.Policy.Supports.Exists(plan => plan.TreeNodes.Exists(node =>
                        node.At.DistanceTo(world) <= node.Radius + admitted.Grid.CellMm * 0.5
                        || node.Parents.Exists(parentId => plan.TreeNodes.Find(parent => parent.Id == parentId)
                            .Exists(parent => BranchCovers(parent, node, world, admitted.Grid.CellMm * 0.5)))));
                    if (!planar && !tree) {
                        count++;
                        if (witness.IsNone) witness = Some(world);
                    }
                }
        return (count * admitted.Grid.CellAreaMm2, witness);
    }

    private static bool BranchCovers(TreeNode from, TreeNode to, Point3d point, double margin) {
        Vector3d axis = to.At - from.At;
        double lengthSquared = axis.SquareLength;
        double t = lengthSquared <= double.Epsilon ? 0.0
            : Math.Clamp(((point - from.At) * axis) / lengthSquared, 0.0, 1.0);
        Point3d centre = from.At + axis * t;
        double radius = from.Radius + (to.Radius - from.Radius) * t + margin;
        return centre.DistanceTo(point) <= radius;
    }

    // One disc and one overhang radius serve lineage linking, reach rasterization, and support coverage alike.
    private static Seq<(int Row, int Column)> Disc(int radius) =>
        Range(-radius, radius * 2 + 1).Bind(row => Range(-radius, radius * 2 + 1).Map(column => (Row: row, Column: column)))
            .Filter(offset => ((long)offset.Row * offset.Row) + ((long)offset.Column * offset.Column) <= (long)radius * radius);

    private static int OverhangRadius(AdmittedAudit admitted, int layer) =>
        (int)OverhangRadiusDemand(admitted.Stack, admitted.Policy, layer);

    private static double OverhangRadiusDemand(SliceStack stack, AuditPolicy policy, int layer) => Math.Ceiling(
        LayerHeight(stack, layer) / Math.Tan(policy.Thresholds.OverhangAngleDeg * Math.PI / 180.0)
        / policy.Thresholds.CellMm);

    private static bool BoundedRadius(double radius, int maximum) =>
        double.IsFinite(radius) && radius is >= 0.0 && radius <= maximum;

    private static UndirectedGraph<Cell, SEdge<Cell>> GridGraph(Memory2D<byte> plane, int layer, RasterGrid grid) {
        UndirectedGraph<Cell, SEdge<Cell>> graph = new(allowParallelEdges: false);
        for (int row = 0; row < grid.Rows; row++)
            for (int column = 0; column < grid.Columns; column++)
                if (plane.Span[row, column] == 1) {
                    Cell cell = new(layer, row, column);
                    graph.AddVertex(cell);
                    if (row > 0 && plane.Span[row - 1, column] == 1) graph.AddEdge(new SEdge<Cell>(cell, new Cell(layer, row - 1, column)));
                    if (column > 0 && plane.Span[row, column - 1] == 1) graph.AddEdge(new SEdge<Cell>(cell, new Cell(layer, row, column - 1)));
                }
        return graph;
    }

    private static UndirectedGraph<Cell, SEdge<Cell>> VolumeGraph(RasterWorkspace workspace, RasterGrid grid, int layers) {
        UndirectedGraph<Cell, SEdge<Cell>> graph = new(allowParallelEdges: false);
        for (int layer = 0; layer < layers; layer++) {
            Memory2D<byte> plane = workspace.Void(layer);
            Memory2D<byte> below = layer > 0 ? workspace.Void(layer - 1) : plane;
            for (int row = 0; row < grid.Rows; row++)
                for (int column = 0; column < grid.Columns; column++)
                    if (plane.Span[row, column] == 1) {
                        Cell cell = new(layer, row, column);
                        graph.AddVertex(cell);
                        if (row > 0 && plane.Span[row - 1, column] == 1) graph.AddEdge(new SEdge<Cell>(cell, new Cell(layer, row - 1, column)));
                        if (column > 0 && plane.Span[row, column - 1] == 1) graph.AddEdge(new SEdge<Cell>(cell, new Cell(layer, row, column - 1)));
                        if (layer > 0 && below.Span[row, column] == 1) graph.AddEdge(new SEdge<Cell>(cell, new Cell(layer - 1, row, column)));
                    }
        }
        return graph;
    }

    private static int Count(Memory2D<byte> plane, RasterGrid grid) {
        int count = 0;
        for (int row = 0; row < grid.Rows; row++)
            for (int column = 0; column < grid.Columns; column++) count += plane.Span[row, column];
        return count;
    }

    private static double Perimeter(Memory2D<byte> plane, RasterGrid grid) {
        int edges = 0;
        for (int row = 0; row < grid.Rows; row++)
            for (int column = 0; column < grid.Columns; column++)
                if (plane.Span[row, column] == 1)
                    edges += (Empty(plane, row - 1, column, grid) ? 1 : 0) + (Empty(plane, row + 1, column, grid) ? 1 : 0)
                        + (Empty(plane, row, column - 1, grid) ? 1 : 0) + (Empty(plane, row, column + 1, grid) ? 1 : 0);
        return edges * grid.CellMm;
    }

    private static (double Millimeters, Option<(int Row, int Column)> Witness) DirectionalExposure(
        Memory2D<byte> plane, Vector2d direction, RasterGrid grid) {
        double length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        double u = direction.X / length, v = direction.Y / length, exposure = 0.0, maximum = 0.0;
        Option<(int Row, int Column)> witness = None;
        for (int row = 0; row < grid.Rows; row++)
            for (int column = 0; column < grid.Columns; column++)
                if (plane.Span[row, column] == 1) {
                    double local = (Empty(plane, row, column - 1, grid) ? Math.Max(0.0, -u) : 0.0)
                        + (Empty(plane, row, column + 1, grid) ? Math.Max(0.0, u) : 0.0)
                        + (Empty(plane, row - 1, column, grid) ? Math.Max(0.0, -v) : 0.0)
                        + (Empty(plane, row + 1, column, grid) ? Math.Max(0.0, v) : 0.0);
                    exposure += local;
                    if (local > maximum) (maximum, witness) = (local, Some((row, column)));
                }
        return (exposure * grid.CellMm, witness);
    }

    private static double EquivalentOpeningDiameter(Seq<Cell> boundary, SliceStack stack, RasterGrid grid) {
        double bottom = boundary.Count(static cell => cell.Layer == 0) * grid.CellAreaMm2;
        double top = boundary.Count(cell => cell.Layer == stack.LayerCount - 1) * grid.CellAreaMm2;
        double left = boundary.Filter(static cell => cell.Column == 0)
            .Map(cell => grid.CellMm * LayerHeight(stack, cell.Layer)).Sum();
        double right = boundary.Filter(cell => cell.Column == grid.Columns - 1)
            .Map(cell => grid.CellMm * LayerHeight(stack, cell.Layer)).Sum();
        double front = boundary.Filter(static cell => cell.Row == 0)
            .Map(cell => grid.CellMm * LayerHeight(stack, cell.Layer)).Sum();
        double back = boundary.Filter(cell => cell.Row == grid.Rows - 1)
            .Map(cell => grid.CellMm * LayerHeight(stack, cell.Layer)).Sum();
        return 2.0 * Math.Sqrt(Seq(bottom, top, left, right, front, back).Max() / Math.PI);
    }

    private static double LayerHeight(SliceStack stack, int layer) => layer == 0
        ? stack.Elevations[1] - stack.Elevations[0]
        : stack.Elevations[layer] - stack.Elevations[layer - 1];

    private sealed class RasterWorkspace : IDisposable {
        public const int Planes = 3;

        private readonly MemoryOwner<byte> solid;
        private readonly MemoryOwner<byte> voids;
        private readonly MemoryOwner<byte> reach;
        private readonly int layers;
        private readonly RasterGrid grid;

        private RasterWorkspace(MemoryOwner<byte> solid, MemoryOwner<byte> voids, MemoryOwner<byte> reach, int layers, RasterGrid grid) =>
            (this.solid, this.voids, this.reach, this.layers, this.grid) = (solid, voids, reach, layers, grid);

        public static RasterWorkspace Allocate(int layers, RasterGrid grid) {
            MemoryOwner<byte>? solid = null;
            MemoryOwner<byte>? voids = null;
            MemoryOwner<byte>? reach = null;
            try {
                solid = MemoryOwner<byte>.Allocate(layers * grid.CellCount, AllocationMode.Clear);
                voids = MemoryOwner<byte>.Allocate(layers * grid.CellCount, AllocationMode.Clear);
                reach = MemoryOwner<byte>.Allocate(layers * grid.CellCount, AllocationMode.Clear);
                return new RasterWorkspace(solid, voids, reach, layers, grid);
            }
            catch {
                reach?.Dispose();
                voids?.Dispose();
                solid?.Dispose();
                throw;
            }
        }

        public Memory2D<byte> Solid(int layer) => Plane(solid, layer);
        public Memory2D<byte> Void(int layer) => Plane(voids, layer);
        public Memory2D<byte> Reach(int layer) => Plane(reach, layer);

        private Memory2D<byte> Plane(MemoryOwner<byte> owner, int layer) =>
            owner.Memory.Slice(layer * grid.CellCount, grid.CellCount).AsMemory2D(grid.Rows, grid.Columns);

        // Occupancy fills first because the reach pass dilates it; `Reach(layer)` is the overhang footprint that layer
        // projects onto the layer above, so lineage and support coverage read one plane instead of rescanning a disc.
        public void Fill(AdmittedAudit admitted) {
            for (int layer = 0; layer < layers; layer++) {
                double elevation = admitted.Stack.Elevations[layer];
                FillAction occupancy = new(Solid(layer), Void(layer), Index(admitted.Layers[layer], admitted.Grid, elevation),
                    admitted.Grid, elevation);
                ParallelHelper.For2D(0, grid.Rows, 0, grid.Columns, in occupancy, minimumActionsPerThread: 1);
            }
            for (int layer = 0; layer < layers - 1; layer++) {
                ReachAction dilation = new(Solid(layer), Reach(layer), Disc(OverhangRadius(admitted, layer + 1)), admitted.Grid);
                ParallelHelper.For2D(0, grid.Rows, 0, grid.Columns, in dilation, minimumActionsPerThread: 1);
            }
        }

        private static Arr<Seq<(Loop Loop, double MinU, double MaxU)>> Index(
            Seq<Loop> loops,
            RasterGrid grid,
            double elevation) {
            Seq<(Loop Loop, BoundingBox Bounds)> bounded = loops.Map(loop => (loop, loop.Bound()));
            return Range(0, grid.Rows).Map(row => {
                double v = grid.Local(row, 0, elevation).Y;
                return bounded
                    .Filter(item => item.Bounds.Min.Y <= v && v <= item.Bounds.Max.Y)
                    .Map(static item => (item.Loop, item.Bounds.Min.X, item.Bounds.Max.X));
            }).ToArr();
        }

        public void Dispose() {
            solid.Dispose();
            voids.Dispose();
            reach.Dispose();
        }
    }

    private readonly struct FillAction(
        Memory2D<byte> solid,
        Memory2D<byte> voids,
        Arr<Seq<(Loop Loop, double MinU, double MaxU)>> index,
        RasterGrid grid,
        double elevation) : IAction2D {
        public void Invoke(int row, int column) {
            Point3d local = grid.Local(row, column, elevation);
            int covering = index[row].Count(item => item.MinU <= local.X && local.X <= item.MaxU && item.Loop.Covers(local));
            if (covering % 2 == 1) solid.Span[row, column] = 1;
            else voids.Span[row, column] = 1;
        }
    }

    private readonly struct ReachAction(
        Memory2D<byte> solid,
        Memory2D<byte> reach,
        Seq<(int Row, int Column)> disc,
        RasterGrid grid) : IAction2D {
        public void Invoke(int row, int column) {
            if (disc.Exists(offset => !Empty(solid, row + offset.Row, column + offset.Column, grid)))
                reach.Span[row, column] = 1;
        }
    }
}
```
