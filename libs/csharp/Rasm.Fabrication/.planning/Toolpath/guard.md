# [RASM_FABRICATION_GUARD]

`Guard` owns fail-closed motion admission from one aggregate request through arc-true planar sweep, protected-surface gouge, fixture and stock collision, medial-clearance, voxel-field, and robot-cell probes. `GuardReceipt` retains every hazard, its overlap severity, the probe scope it executed, and every provider warning, while provider errors terminate on the typed failure rail; no probe hides a prior contact or degrades a geometric failure into an empty result.

`Guard.Check` consumes one admitted `GuardRequest`. `GuardScope` separates a probed verdict from one elided by the clearance plane, so `GuardReceipt.Proven` distinguishes tested-and-clear from untested where `Clear` alone cannot. `HolderState` makes mounted and certified holder evidence mutually exclusive, `HolderCertificate.Admit` binds omission evidence to the exact `ToolAssembly.Identity`, cutter, operation, scope, and `ConservativeEnvelope` holder footprint, and native `Voxels` custody terminates inside the probe capsule.

## [01]-[INDEX]

- [02]-[GUARD]: `GuardRequest` closes aggregate admission, `HolderState` closes holder posture, `GuardScope` closes probe disposition, `GuardProbe` adds sidecar voxel and robot-cell evidence, and `Guard.Check` returns one accumulated `GuardReceipt`.

## [02]-[GUARD]

- Owner: `GuardRequest` is the admitted move, part, stock, fixture, fixture-state, policy, and probe aggregate; `GuardReceipt` is its evidence-complete result. `Fixture.Zones` is the sole exclusion-zone owner and the spatial-index ordinal domain; stock carries blank, forbidden, and snapshot geometry only.
- Cases: `HolderState` admits a mounted `ToolAssembly` or an identity-bound `HolderCertificate`; `ProbeRoute` admits the scalar reference or receipt-backed measured path; `GuardScope` admits a probed or plane-elided disposition; `GuardProbe` admits voxel-field and robot-cell providers; `RobotCollisionAdmission` closes accepted and refused provider evidence; `Hazard` closes gouge, fixed-zone, static-keepout, stock, channel, voxel, and robot contact.
- Cases: `FabricationBenchClaims` rosters the five kernel `BenchClaim` rows — NFP placement, ICP probe fit, skeleton offset, bend search, parallel clearance. `AcceptedBenchmarkClaim` binds one accepted result to the host digest its pass was stamped over, and `ProbeRoute.Measured` admits only the `ClearanceParallel` case, so unrelated solver evidence cannot authorize parallel clearance.
- Law: every roster row pairs with the measuring case beside its lane owner — `NestBench` at `Nesting/nfp`, `ProbeBench` at `Verify/probing`, `SkeletonBench` at `Toolpath/skeleton`, `BrakeBench` at `Forming/brake`, `ClearanceBench` here — each a workload admission plus the measured fold in folder types; measurement columns and the receipt projection stay the bench edge's under the `Rasm.AppHost/Observability/benchmarks#CLAIM_FIELD_MAP` Fabrication row, and `ClearanceBench.Reference` is the same-run cost the field map's reference-evidence law carries for the one lane-pair claim.
- Law: `AcceptedBenchmarkClaim.Admit` takes judgment as a `Func<string, UInt128, bool>` seam over the claim key and the `HostEvidence` digest, the exact pair `BenchmarkGate.Judge` at `Rasm.AppHost/Observability/benchmarks#CLAIM_FIELD_MAP` resolves when it stamps `BenchmarkVerdict.Pass` — the Fabrication row keys `{Suite}/{Case}` under that page's own field map. This package supplies the bench roster and the host digest, never a verdict, and never persists one.
- Entry: `Guard.Check(GuardRequest)` preserves the frozen `Check` operation name and accumulates independent contacts through `Traverse`, `As`, and `Bind`.
- Auto: planar straight and circular moves lower once to an arc-true trajectory, round-ended offset sweeps retain cutter and holder separation, one `Surfaces` row set traverses every planar obstacle class against the shared cutter-and-holder `Faces` table, feed moves drop the cutter face against stock while the holder face always tests, and channel pinch uses the larger swept radius with the admitted margin.
- Receipt: `HolderEvidence` carries mounted or certified payload without a boolean cross-product; `ContactWitness` carries the contact point and its overlap area so `Hazard.Severity` ranks contacts rather than leaving them unordered; `ClearanceEvidence` retains minimum medial clearance, the optional skeleton witness, the requested route, and whether the parallel substrate executed; `VoxelContact` retains obstacle, membership, overlap volume, ray witness, and native memory; `RobotContact` retains provider target, meshes, duration, target census, and warnings.
- Packages: `ToolMagazine.HolderEnvelope` derives mounted and certified holder footprints; `ArcAlgebra.Densify` preserves circular motion; `PolygonAlgebra.Apply` owns offset and intersection and receives the calling `Op` key so a trace refusal names its operation; `RegionNode.SignedArea` supplies contact severity without a second measure pass; `Spatial.Apply` owns indexed pruning; `CurveSkeleton.Clearance` owns arbitrary-probe clearance; `MemoryOwner<T>`, `ParallelHelper.For2D` with the admitted partition floor, and `TensorPrimitives` own pooled measured clearance reduction; PicoGK owns copied SDF intersection, membership, and ray witnesses; `MotionEvidence` supplies the admitted joint-and-duration trajectory the cell probe tests; `IRobotCollisionProvider` owns the executable robot-cell collision boundary and every provider handle behind it.
- Growth: a new obstacle is one `Hazard` case and one `Surfaces` row, a new swept face is one `Faces` row every obstacle class inherits, a new provider is one `GuardProbe` case, and a new execution substrate is one evidence-carrying `ProbeRoute` case.
- Boundary: `Clearance` and `ArcSpan` are the named statement kernels — pooled measured reduction and arc-frame numerics respectively; Rhino-native planar geometry stays inside the package wire, the cell probe names NO provider type — `CellCollisionRequest` carries the frozen `MotionEvidence` trajectory and a kernel `MeshSpace` environment, so the `Robots` and Rhino3dm alias crossing stays solely at `Kinematics/cell` — and PicoGK resources remain bracketed inside `ProbeVoxel`; no provider geometry escapes `GuardReceipt`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using PicoGK;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Fixturing;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Meshing;
using Rasm.Spatial;
using Rhino.Geometry;
using System.Numerics.Tensors;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using TimeDuration = NodaTime.Duration;
using PVector = PicoGK.Vector3;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HolderState {
    private HolderState() { }

    public sealed record Mounted(ToolAssembly Assembly) : HolderState;
    public sealed record Certified(HolderCertificate Certificate) : HolderState;
}

// Bench-case roster: five kernel BenchClaim rows, one per durable claim key in the `rasm.fabrication` suite,
// per the kernel law that claim rows land as static readonly rows on their owning pages — a folder-local claim
// enum is the deleted form. Every claim is algorithmic (Corpus: None). ClearanceParallel carries the one
// genuine lane pair this page declares — the partitioned For2D reduction against the sequential clearance
// walk; the four solver rows are no-regression claims judged against their own prior stamped receipt, so
// both lane columns spell the measured entry and the floor is 1.0. Every row's measuring case is the bench
// producer beside its lane owner — workload admission plus the measured fold, folder types only — and
// runtime benchmark types terminate at the AppHost projection into `AcceptedBenchmarkClaim`.
public static class FabricationBenchClaims {
    public const string Suite = "rasm.fabrication";

    public static readonly BenchClaim NfpPlacement = new(Op.Of(name: $"{Suite}/nfp-placement"), "Nest.Solve", "Nest.Solve", 1.0);
    public static readonly BenchClaim IcpProbeFit = new(Op.Of(name: $"{Suite}/icp-probe-fit"), "Probe.Inspect", "Probe.Inspect", 1.0);
    public static readonly BenchClaim SkeletonOffset = new(Op.Of(name: $"{Suite}/skeleton-offset"), "Skeleton.Walk", "Skeleton.Walk", 1.0);
    public static readonly BenchClaim BendSearch = new(Op.Of(name: $"{Suite}/bend-search"), "BendSequence.Plan", "BendSequence.Plan", 1.0);
    public static readonly BenchClaim ClearanceParallel = new(Op.Of(name: $"{Suite}/clearance-parallel"), "ParallelHelper.For2D", "CurveSkeleton.Clearance", 1.0);
}

// The accepted half of a speed claim. `ContentKey` cannot name a benchmark run at all — its `EgressKind` vocabulary
// is this package's machine-artifact families and a bench receipt is none of them — so the identity the AppHost gate
// can actually resolve is the claim key BESIDE the `HostEvidence` digest it stamped `Pass` over. That digest crosses
// as a VALUE, so no AppHost type reaches this plane, and a claim accepted on one host never authorizes the parallel
// lane on another.
public sealed record AcceptedBenchmarkClaim {
    private AcceptedBenchmarkClaim(BenchClaim bench, UInt128 host) => (Bench, Host) = (bench, host);

    public BenchClaim Bench { get; }
    public UInt128 Host { get; }

    public string Key => FormattableString.Invariant($"{(string)Bench.Claim}/{Host:x32}");

    [BoundaryAdapter]
    public static Fin<AcceptedBenchmarkClaim> Admit(
        BenchClaim bench,
        UInt128 host,
        Func<string, UInt128, bool> judged) =>
        from _aggregate in bench is not null && host != UInt128.Zero && judged is not null
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "benchmark-claim:aggregate"))
        from accepted in Try.lift(() => judged(bench.Claim, host)).Run()
            .MapFail(static error => new FabricationFault.PolicyInadmissible(
                FabConcern.Toolpath, $"benchmark-claim:judge:{error.Message}"))
        from _judged in accepted
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "benchmark-claim:refused"))
        select new AcceptedBenchmarkClaim(bench, host);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProbeRoute {
    private ProbeRoute() { }

    public sealed record Reference : ProbeRoute;
    public sealed record Measured(AcceptedBenchmarkClaim Claim, int MinimumActionsPerThread) : ProbeRoute;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GuardProbe {
    private GuardProbe() { }

    public sealed record Voxel(Func<Fin<VoxelLease>> Acquire, VoxelRay Ray) : GuardProbe;
    public sealed record Robot(IRobotCollisionProvider Provider, CellCollisionRequest Request) : GuardProbe;
}

// The cell probe is provider-free on BOTH sides: the trajectory arrives as the frozen `MotionEvidence` atom
// `Kinematics/cell` already publishes — the one joint-and-duration carrier the motion wire admits — and the
// environment as a kernel `MeshSpace`, so a `Robots` type and the `Rhino3dm` alias stay behind `Kinematics/cell`,
// which the package's single-crossing law names as their only home. `First` and `Second` restrict the mechanism
// pairs the provider tests; both empty means every pair. The implementor owns whatever provider handle it needs.
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class CellCollisionRequest {
    public MotionEvidence Motion { get; }
    public MeshSpace Environment { get; }
    public int EnvironmentPlane { get; }
    public double LinearStepMm { get; }
    public double AngularStepRad { get; }
    public Arr<int> First { get; }
    public Arr<int> Second { get; }

    public Seq<Arr<double>> Joints => Motion.Joints;
    public Seq<TimeDuration> Segments => Motion.SegmentDurations;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref MotionEvidence motion,
        ref MeshSpace environment,
        ref int environmentPlane,
        ref double linearStepMm,
        ref double angularStepRad,
        ref Arr<int> first,
        ref Arr<int> second) =>
        validationError = motion is not null && environment is not null && environmentPlane >= 0
            && linearStepMm > 0.0 && double.IsFinite(linearStepMm)
            && angularStepRad > 0.0 && double.IsFinite(angularStepRad)
            && first.ForAll(static index => index >= 0) && second.ForAll(static index => index >= 0)
                ? null
                : new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "cell-collision-request");

    public static Fin<CellCollisionRequest> Admit(
        MotionEvidence motion,
        MeshSpace environment,
        int environmentPlane,
        double linearStepMm,
        double angularStepRad,
        Arr<int> first,
        Arr<int> second) =>
        Validate(motion, environment, environmentPlane, linearStepMm, angularStepRad, first, second,
            out CellCollisionRequest request).Admitted(request);
}

public sealed record RobotCollisionEvidence(
    bool HasCollision,
    string Target,
    int Meshes,
    Seq<string> Warnings);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RobotCollisionAdmission {
    private RobotCollisionAdmission() { }

    public sealed record Accepted(
        bool HasCollision,
        string Target,
        int Meshes,
        int Targets,
        double DurationSeconds,
        Seq<string> Warnings) : RobotCollisionAdmission;
    public sealed record Refused(string Field) : RobotCollisionAdmission;
}

public interface IRobotCollisionProvider {
    Fin<RobotCollisionEvidence> Check(CellCollisionRequest request);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GuardScope {
    private GuardScope() { }

    public sealed record Probed : GuardScope;
    public sealed record Elided(double ClearancePlaneMm, double LowestZMm) : GuardScope;
}

[SmartEnum<string>(IsValidatable = true)]
public sealed partial class VoxelObstacle {
    public static readonly VoxelObstacle Stock = new("stock");
    public static readonly VoxelObstacle Fixture = new("fixture");
    public static readonly VoxelObstacle Protected = new("protected");
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class VoxelRay {
    public Point3d Search { get; }
    public Vector3d Direction { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Point3d search,
        ref Vector3d direction) =>
        validationError = search.IsValid && direction.IsValid && !direction.IsZero
            ? null
            : new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "voxel-ray");

    public static Fin<VoxelRay> Admit(Point3d search, Vector3d direction) =>
        Validate(search, direction, out VoxelRay ray).Admitted(ray);
}

public sealed class VoxelLease : IDisposable {
    public Voxels Tool { get; }
    public Voxels Stock { get; }
    public Voxels Fixture { get; }
    public Voxels Protected { get; }

    private VoxelLease(Voxels tool, Voxels stock, Voxels fixture, Voxels @protected) =>
        (Tool, Stock, Fixture, Protected) = (tool, stock, fixture, @protected);

    public static Fin<VoxelLease> Admit(Voxels tool, Voxels stock, Voxels fixture, Voxels @protected) =>
        Seq(tool, stock, fixture, @protected).ForAll(static field => field is not null)
        && !ReferenceEquals(tool, stock)
        && !ReferenceEquals(tool, fixture)
        && !ReferenceEquals(tool, @protected)
        && !ReferenceEquals(stock, fixture)
        && !ReferenceEquals(stock, @protected)
        && !ReferenceEquals(fixture, @protected)
            ? Fin.Succ(new VoxelLease(tool, stock, fixture, @protected))
            : Fin.Fail<VoxelLease>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "guard:voxel-lease"));

    // Release is a ROLLBACK over four independent native fields: each release runs, every failure lands on the
    // typed rail as one `Error.Many`, and no field is skipped because an earlier one threw. `Dispose` keeps the
    // `using` contract by draining that rail — the rail is what the probe reads, so a release defect is evidence
    // rather than an exception crossing a `Fin`-declaring boundary.
    public Fin<Unit> Release() =>
        Seq(Tool, Stock, Fixture, Protected).Fold(
            Seq<Error>(),
            static (failures, field) => Try.lift(() => { field.Dispose(); return unit; }).Run()
                .Match(Succ: _ => failures, Fail: failures.Add))
        is var refusals && refusals.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Error.Many([.. refusals]));

    public void Dispose() => _ = Release();
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class HolderCertificate {
    public UInt128 AssemblyIdentity { get; }
    public CutterForm Cutter { get; }
    public int Operation { get; }
    public BoundingBox Scope { get; }
    public Loop ConservativeEnvelope { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref UInt128 assemblyIdentity,
        ref CutterForm cutter,
        ref int operation,
        ref BoundingBox scope,
        ref Loop conservativeEnvelope) {
        if (assemblyIdentity == UInt128.Zero || operation < 0 || !scope.IsValid || scope.Diagonal.Length <= 0.0)
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "holder-certificate");
    }

    public static Fin<HolderCertificate> Admit(
        ToolAssembly assembly,
        CutterForm cutter,
        int operation,
        BoundingBox scope) =>
        from envelope in ToolMagazine.HolderEnvelope(assembly)
        from certificate in Validate(assembly.Identity, cutter, operation, scope, envelope, out HolderCertificate row)
            .Admitted(row)
        select certificate;
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class GuardPolicy {
    public double ClearancePlaneMm { get; }
    public double GougeToleranceMm { get; }
    public Context Tolerance { get; }
    public OffsetPolicy SweepOffset { get; }
    public OffsetPolicy RegionOffset { get; }
    public double ArcChordErrorMm { get; }
    public double ChannelMarginMm { get; }
    public double ClearanceProbeStepMm { get; }
    public int MaximumSweepSegments { get; }
    public int MaximumClearanceProbes { get; }

    // The clearance substrate is a policy column beside the probe budget it governs, not a call-site literal:
    // `ProbeRoute.Measured` carries the accepted benchmark receipt that authorizes the parallel lane, so a caller
    // that could name the route inline could authorize it inline. Seating it here makes the reference lane the
    // declared default and the measured lane an admitted policy value the same `Admit` gate validates.
    public ProbeRoute Route { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref double clearancePlaneMm,
        ref double gougeToleranceMm,
        ref Context tolerance,
        ref OffsetPolicy sweepOffset,
        ref OffsetPolicy regionOffset,
        ref double arcChordErrorMm,
        ref double channelMarginMm,
        ref double clearanceProbeStepMm,
        ref int maximumSweepSegments,
        ref int maximumClearanceProbes,
        ref ProbeRoute route) {
        Validation<Error, Unit> admitted = (
            Gate(double.IsFinite(clearancePlaneMm), "clearance-plane"),
            Gate(gougeToleranceMm >= 0.0 && double.IsFinite(gougeToleranceMm), "gouge-tolerance"),
            Gate(sweepOffset.IsValid && regionOffset.IsValid, "offset-policy"),
            Gate(arcChordErrorMm > 0.0 && double.IsFinite(arcChordErrorMm), "arc-chord"),
            Gate(channelMarginMm >= 0.0 && double.IsFinite(channelMarginMm), "channel-margin"),
            Gate(clearanceProbeStepMm > 0.0 && double.IsFinite(clearanceProbeStepMm), "probe-step"),
            Gate(maximumSweepSegments >= 8, "sweep-capacity"),
            Gate(maximumClearanceProbes >= 2, "probe-capacity"),
            // The measured lane admits only the parallel-clearance claim and only above the partition floor the
            // benchmark itself measured, so an unrelated claim or a degenerate partition refuses at policy admission
            // rather than silently running the reference lane under a measured label. Equality reads the accepted
            // claim's own `BenchClaim.Claim` — `AcceptedBenchmarkClaim.Key` interpolates the host digest beside it,
            // so comparing that composite to a bare claim name can never hold and leaves the lane dead.
            Gate(route.Switch(
                reference: static _ => true,
                measured: static row => row.MinimumActionsPerThread > 0
                    && row.Claim.Bench.Claim.Equals(FabricationBenchClaims.ClearanceParallel.Claim)), "probe-route"))
            .Apply(static (_, _, _, _, _, _, _, _, _) => unit)
            .As();
        validationError = admitted.Match<FabricationFault?>(
            Fail: static error => new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, error.Message),
            Succ: static _ => null);
    }

    public static Fin<GuardPolicy> Admit(
        double clearancePlaneMm,
        double gougeToleranceMm,
        Context tolerance,
        OffsetPolicy sweepOffset,
        OffsetPolicy regionOffset,
        double arcChordErrorMm,
        double channelMarginMm,
        double clearanceProbeStepMm,
        int maximumSweepSegments,
        int maximumClearanceProbes,
        ProbeRoute route) =>
        Validate(clearancePlaneMm, gougeToleranceMm, tolerance, sweepOffset, regionOffset, arcChordErrorMm,
            channelMarginMm, clearanceProbeStepMm, maximumSweepSegments, maximumClearanceProbes, route,
            out GuardPolicy policy).Admitted(policy);

    public Fin<int> ClearanceSegments(double length) =>
        double.IsFinite(length) && length >= 0.0
        && Math.Ceiling(length / ClearanceProbeStepMm) <= MaximumClearanceProbes
            ? Fin.Succ(Math.Max(1, (int)Math.Ceiling(length / ClearanceProbeStepMm)))
            : Fin.Fail<int>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "guard:clearance-capacity"));

    private static K<Validation<Error>, Unit> Gate(bool admitted, string axis) =>
        AdmissionSlots.Gate(admitted, new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, $"guard-policy:{axis}"));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class GuardPart {
    public Point3d Cursor { get; }
    public Seq<Loop> Protected { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Point3d cursor,
        ref Seq<Loop> @protected) {
        if (!(cursor.IsValid && @protected.ForAll(static loop => loop.Closed && loop.Count >= 3)))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "guard-part");
    }

    public static Fin<GuardPart> Admit(Point3d cursor, Seq<Loop> @protected) =>
        Validate(cursor, @protected, out GuardPart part).Admitted(part);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class GuardStock {
    public Seq<Loop> RawBlank { get; }
    public Seq<Loop> Forbidden { get; }
    public Seq<StockSnapshot> Snapshots { get; }
    public CutterForm Cutter { get; }
    public HolderState Holder { get; }
    public Option<CurveSkeleton> Channel { get; }
    public Option<SpatialIndex> Index { get; }

    public double Radius => Cutter.Diameter * 0.5;

    public Seq<Loop> Current(int setup) =>
        toSeq(Snapshots.Filter(snapshot => snapshot.Setup <= setup)
                .OrderByDescending(static snapshot => snapshot.Setup))
            .Head
            .Map(static snapshot => snapshot.Machined.ToSeq())
            .IfNone(RawBlank);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Seq<Loop> rawBlank,
        ref Seq<Loop> forbidden,
        ref Seq<StockSnapshot> snapshots,
        ref CutterForm cutter,
        ref HolderState holder,
        ref Option<CurveSkeleton> channel,
        ref Option<SpatialIndex> index) {
        Validation<Error, Unit> admitted = (
            Gate(!rawBlank.IsEmpty && rawBlank.ForAll(static loop => loop is not null && loop.Closed && loop.Count >= 3), "blank"),
            Gate(forbidden.ForAll(static loop => loop is not null && loop.Closed && loop.Count >= 3), "forbidden"),
            Gate(snapshots.ForAll(static snapshot => snapshot is not null && snapshot.Setup >= 0
                && snapshot.Machined.ForAll(static loop => loop is not null && loop.Closed)), "snapshots"),
            Gate(cutter is not null && cutter.Diameter > 0.0 && double.IsFinite(cutter.Diameter), "cutter"),
            Gate(holder.Switch(
                mounted: static row => row.Assembly.Identity != UInt128.Zero,
                certified: static row => row.Certificate.AssemblyIdentity != UInt128.Zero), "holder"),
            Gate(channel.Map(ChannelValid).IfNone(true), "channel"))
            .Apply(static (_, _, _, _, _, _) => unit)
            .As();
        validationError = admitted.Match<FabricationFault?>(
            Fail: static error => new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, error.Message),
            Succ: static _ => null);
    }

    public static Fin<GuardStock> Admit(
        Seq<Loop> rawBlank,
        Seq<Loop> forbidden,
        Seq<StockSnapshot> snapshots,
        CutterForm cutter,
        HolderState holder,
        Option<CurveSkeleton> channel,
        Option<SpatialIndex> index) =>
        Validate(rawBlank, forbidden, snapshots, cutter, holder, channel, index, out GuardStock stock)
            .Admitted(stock);

    private static bool ChannelValid(CurveSkeleton channel) =>
        channel.NodeCount > 0
        && channel.NodeX.Length == channel.NodeCount
        && channel.NodeY.Length == channel.NodeCount
        && channel.NodeZ.Length == channel.NodeCount
        && channel.Radius.Length == channel.NodeCount
        && channel.Witness.Length == channel.NodeCount
        && channel.ArcFrom.Length == channel.ArcCount
        && channel.ArcTo.Length == channel.ArcCount
        && channel.ArcOrigin.Length == channel.ArcCount
        && channel.Component.Length == channel.ArcCount
        && channel.ArcFrom.All(index => index >= 0 && index < channel.NodeCount)
        && channel.ArcTo.All(index => index >= 0 && index < channel.NodeCount)
        && channel.NodeX.Concat(channel.NodeY).Concat(channel.NodeZ).Concat(channel.Radius).All(double.IsFinite)
        && channel.Radius.All(static radius => radius >= 0.0);

    private static K<Validation<Error>, Unit> Gate(bool admitted, string axis) =>
        AdmissionSlots.Gate(admitted, new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, $"guard-stock:{axis}"));
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class GuardRequest {
    public Move Move { get; }
    public GuardPart Part { get; }
    public GuardStock Stock { get; }
    public Fixture Fixture { get; }
    public FixtureState State { get; }
    public GuardPolicy Policy { get; }
    public Seq<GuardProbe> Probes { get; }
    public Point3d Target => Move.Target;

    // A move arrives admitted through its own sealed factory, so the request re-proves nothing about it except the
    // ONE thing a planar swept solid cannot answer: an oriented move has no exact planar sweep, so the guard refuses
    // it by name rather than approximating the tilt silently.
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref Move move,
        ref GuardPart part,
        ref GuardStock stock,
        ref Fixture fixture,
        ref FixtureState state,
        ref GuardPolicy policy,
        ref Seq<GuardProbe> probes) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(move.AxisFree, new FabricationFault.PolicyInadmissible(
                FabConcern.Toolpath, "swept-solid:oriented-move")),
            Gate(HolderValid(stock.Holder, stock.Cutter, fixture.Operation, MotionScope(move, part.Cursor)), "holder"))
            .Apply(static (_, _) => unit)
            .As();
        validationError = admitted.Match<FabricationFault?>(
            Fail: static error => new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, error.Message),
            Succ: static _ => null);
    }

    public static Fin<GuardRequest> Admit(
        Move move,
        GuardPart part,
        GuardStock stock,
        Fixture fixture,
        FixtureState state,
        GuardPolicy policy,
        Seq<GuardProbe> probes) =>
        Validate(move, part, stock, fixture, state, policy, probes, out GuardRequest request).Admitted(request);

    private static bool HolderValid(HolderState holder, CutterForm cutter, int operation, BoundingBox motionScope) =>
        holder.Switch(
            mounted: static row => row.Assembly.Identity != UInt128.Zero,
            certified: row => row.Certificate.AssemblyIdentity != UInt128.Zero
                && row.Certificate.Cutter == cutter
                && row.Certificate.Operation == operation
                && row.Certificate.Scope.Contains(motionScope));

    private static BoundingBox MotionScope(Move move, Point3d from) => move.Switch(
        state: from,
        rapid: static (start, row) => new BoundingBox(start, row.Target),
        linear: static (start, row) => new BoundingBox(start, row.Target),
        circular: static (start, row) => new BoundingBox(
            new Point3d(
                row.Arc.Center.X - start.DistanceTo(row.Arc.Center),
                row.Arc.Center.Y - start.DistanceTo(row.Arc.Center),
                Math.Min(start.Z, row.Target.Z)),
            new Point3d(
                row.Arc.Center.X + start.DistanceTo(row.Arc.Center),
                row.Arc.Center.Y + start.DistanceTo(row.Arc.Center),
                Math.Max(start.Z, row.Target.Z))));

    private static K<Validation<Error>, Unit> Gate(bool admitted, string axis) =>
        AdmissionSlots.Gate(admitted, new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, $"guard-request:{axis}"));
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record ContactWitness(Point3d Surface, double AreaMm2);

public sealed record ClearanceEvidence(
    double MinimumMm,
    Point3d Probe,
    Option<int> SkeletonWitness,
    ProbeRoute Route,
    bool Parallel,
    int Samples);

public sealed record VoxelContact(
    VoxelObstacle Obstacle,
    bool SearchInside,
    Point3d Witness,
    double VolumeMm3,
    long NativeBytes);

public sealed record RobotContact(
    string CollisionTarget,
    int CollisionMeshes,
    int ProgramTargets,
    double DurationSeconds,
    Seq<string> Warnings);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HolderEvidence {
    private HolderEvidence() { }

    public sealed record Mounted(UInt128 AssemblyIdentity, Loop Footprint) : HolderEvidence;
    public sealed record Certified(UInt128 AssemblyIdentity, BoundingBox Scope, Loop Footprint) : HolderEvidence;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hazard {
    private Hazard() { }

    public sealed record Gouge(Loop Obstacle, CollisionContact Contact, ContactWitness Witness) : Hazard;
    public sealed record Fixed(ExclusionZone Obstacle, CollisionContact Contact, ContactWitness Witness) : Hazard;
    public sealed record Keepout(Loop Obstacle, CollisionContact Contact, ContactWitness Witness) : Hazard;
    public sealed record Stock(Loop Obstacle, CollisionContact Contact, ContactWitness Witness) : Hazard;
    public sealed record Channel(ClearanceEvidence Evidence, double RequiredMm) : Hazard;
    public sealed record Voxel(VoxelContact Contact) : Hazard;
    public sealed record Robot(RobotContact Contact) : Hazard;

    // Ordering rank only: planar arms carry mm2 overlap, voxel carries mm3 overlap, channel carries mm shortfall, and robot outranks all.
    public double Severity => Switch(
        gouge: static row => row.Witness.AreaMm2,
        @fixed: static row => row.Witness.AreaMm2,
        keepout: static row => row.Witness.AreaMm2,
        stock: static row => row.Witness.AreaMm2,
        channel: static row => row.RequiredMm - row.Evidence.MinimumMm,
        voxel: static row => row.Contact.VolumeMm3,
        robot: static _ => double.PositiveInfinity);
}

public sealed record GuardReceipt(
    Move Move,
    GuardScope Scope,
    Seq<Hazard> Hazards,
    Option<ClearanceEvidence> Clearance,
    HolderEvidence Holder,
    Seq<string> Warnings) {
    public bool Clear => Hazards.IsEmpty;

    public bool Proven => Hazards.IsEmpty && Scope is GuardScope.Probed;

    public Seq<Hazard> Ranked => toSeq(Hazards.OrderByDescending(static hazard => hazard.Severity));
}

file sealed record SweptEnvelope(
    Seq<Loop> Cutter,
    Seq<Loop> Holder,
    HolderEvidence Evidence,
    double RequiredClearanceMm) {
    public Seq<Loop> Combined => Cutter.Concat(Holder);
    public BoundingBox Bound => Combined.Map(static loop => loop.Bound()).Fold(BoundingBox.Empty, BoundingBox.Union);
}

file readonly record struct ClearanceAction(Arr<Point3d> Points, CurveSkeleton Channel, Memory<double> Values) : IAction2D {
    public void Invoke(int i, int _) => Values.Span[i] = Channel.Clearance(Points[i]).Radius;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Guard {
    public static Fin<GuardReceipt> Check(GuardRequest request) =>
        from trajectory in Trajectory(request.Move, request.Part.Cursor, request.Policy)
        from swept in Sweep(trajectory, request.Stock, request.Policy)
        let scope = PlanarScope(request)
        from planar in scope is GuardScope.Probed
            ? from rows in Surfaces.Traverse(row => Contacts(swept, request, row)).As()
              from zones in FixedContacts(swept, request)
              select rows.Bind(identity) + zones
            : Fin.Succ(Seq<Hazard>())
        from clearance in scope is GuardScope.Probed
            ? Channel(trajectory, request.Stock, request.Policy)
            : Fin.Succ(Option<ClearanceEvidence>.None)
        // A probe runs only where the planar lane ran: an elided scope tested nothing, so running the sidecar
        // providers under it would report hazards a `Proven` verdict then denies having tested.
        from probeRows in scope is GuardScope.Probed
            ? request.Probes.Traverse(probe => Probe(probe, request.Target)).As()
            : Fin.Succ(Seq<(Seq<Hazard> Hazards, Seq<string> Warnings)>())
        let channelHazards = clearance.Filter(evidence => evidence.MinimumMm < swept.RequiredClearanceMm)
            .Map(evidence => Seq<Hazard>(new Hazard.Channel(evidence, swept.RequiredClearanceMm)))
            .IfNone(Seq<Hazard>())
        select new GuardReceipt(
            request.Move,
            scope,
            planar.Concat(channelHazards).Concat(probeRows.Bind(static row => row.Hazards)),
            clearance,
            swept.Evidence,
            probeRows.Bind(static row => row.Warnings));

    // One obstacle class is one ROW: the loops it draws, the faces it tests, and the hazard it mints. The four
    // near-identical traversal bodies the rows replace differed only in those three columns.
    private sealed record ObstacleRow(
        string Axis,
        Func<GuardRequest, Fin<Seq<Loop>>> Obstacles,
        Func<GuardRequest, CollisionContact, bool> Tests,
        Func<Loop, CollisionContact, ContactWitness, Hazard> Mint);

    private static readonly Arr<ObstacleRow> Surfaces = Arr(
        new ObstacleRow(
            "protected",
            static request => Fin.Succ(request.Part.Protected),
            static (_, _) => true,
            static (loop, contact, witness) => new Hazard.Gouge(loop, contact, witness)),
        new ObstacleRow(
            "keepout",
            static request => Fin.Succ(request.Stock.Forbidden),
            static (_, _) => true,
            static (loop, contact, witness) => new Hazard.Keepout(loop, contact, witness)),
        new ObstacleRow(
            "stock",
            static request => Fin.Succ(request.Stock.Current(request.Fixture.Operation)),
            // A feed move cuts stock by design, so only the holder face tests against it; a rapid tests both.
            static (request, contact) => request.Move is Move.Rapid || contact == CollisionContact.Holder,
            static (loop, contact, witness) => new Hazard.Stock(loop, contact, witness)));

    private static GuardScope PlanarScope(GuardRequest request) =>
        request.Move is Move.Rapid
        && Math.Min(request.Part.Cursor.Z, request.Target.Z) >= request.Policy.ClearancePlaneMm
        && request.Stock.Channel.IsNone
        && request.Fixture.Zones.Filter(zone => zone.Active.Contains(request.State))
            .ForAll(zone => zone.Upper.As(LengthUnit.Millimeter) <= request.Policy.ClearancePlaneMm)
        && request.Part.Protected.Concat(request.Stock.Forbidden).Concat(request.Stock.Current(request.Fixture.Operation))
            .ForAll(loop => loop.Bound().Max.Z <= request.Policy.ClearancePlaneMm)
            ? new GuardScope.Elided(
                request.Policy.ClearancePlaneMm,
                Math.Min(request.Part.Cursor.Z, request.Target.Z))
            : new GuardScope.Probed();

    private static Seq<(Seq<Loop> Envelope, CollisionContact Contact)> Faces(SweptEnvelope swept) =>
        Seq((swept.Cutter, CollisionContact.Cutter), (swept.Holder, CollisionContact.Holder));

    private static Fin<Loop> Trajectory(Move move, Point3d cursor, GuardPolicy policy) =>
        move.Switch(
            state: (Cursor: cursor, Policy: policy),
            rapid: static (state, row) => Loop.Admit(Arr(state.Cursor, row.Target), false, [], state.Policy.Tolerance),
            linear: static (state, row) => Loop.Admit(Arr(state.Cursor, row.Target), false, [], state.Policy.Tolerance),
            circular: static (state, row) => ArcSpan(state.Cursor, row, state.Policy)
                .Bind(span => Chords(span, state.Policy.ArcChordErrorMm))
                .Bind(result => result.Spans <= state.Policy.MaximumSweepSegments
                    ? Fin.Succ(result)
                    : Fin.Fail<Loop>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "guard:sweep-capacity"))));

    private static Fin<Loop> ArcSpan(Point3d from, Move.Circular move, GuardPolicy policy) {
        Vector3d a = from - move.Arc.Center;
        Vector3d b = move.Target - move.Arc.Center;
        double radius = a.Length;
        if (!double.IsFinite(radius)
            || radius <= 0.0
            || Math.Abs(from.Z - move.Target.Z) > policy.Tolerance.Absolute.Value
            || Math.Abs(from.Z - move.Arc.Center.Z) > policy.Tolerance.Absolute.Value
            || Math.Abs(radius - b.Length) > policy.ArcChordErrorMm)
            return Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, None, "guard:arc-motion").ToError());
        if (from.DistanceTo(move.Target) <= policy.Tolerance.Absolute.Value) {
            Point3d opposite = new(
                2.0 * move.Arc.Center.X - from.X,
                2.0 * move.Arc.Center.Y - from.Y,
                from.Z);
            double half = Math.Tan(move.SweepRadians / 8.0);
            return Loop.Admit(Arr(from, opposite, move.Target), false, Arr(half, half, 0.0), policy.Tolerance);
        }
        return Loop.Admit(
            Arr(from, move.Target),
            false,
            Arr(Math.Tan(move.SweepRadians / 4.0), 0.0),
            policy.Tolerance);
    }

    private static Fin<SweptEnvelope> Sweep(Loop trajectory, GuardStock stock, GuardPolicy policy) =>
        from cutter in Offset(trajectory, stock.Radius, JoinType.Round, EndType.Round, policy.SweepOffset)
        from evidence in stock.Holder.Switch(
            mounted: static row => ToolMagazine.HolderEnvelope(row.Assembly)
                .Map(footprint => (HolderEvidence)new HolderEvidence.Mounted(row.Assembly.Identity, footprint)),
            certified: static row => Fin.Succ((HolderEvidence)new HolderEvidence.Certified(
                row.Certificate.AssemblyIdentity,
                row.Certificate.Scope,
                row.Certificate.ConservativeEnvelope)))
        from holderRadius in FootprintRadius(evidence.Switch(
            mounted: static row => row.Footprint,
            certified: static row => row.Footprint), policy)
        from holder in Offset(trajectory, holderRadius, JoinType.Round, EndType.Round, policy.SweepOffset)
        select new SweptEnvelope(cutter, holder, evidence, Math.Max(stock.Radius, holderRadius) + policy.ChannelMarginMm);

    private static Fin<double> FootprintRadius(Loop footprint, GuardPolicy policy) =>
        (footprint.Bulges.ForAll(static bulge => bulge == 0.0)
            ? Fin.Succ(footprint)
            : Chords(footprint, policy.ArcChordErrorMm))
        .Bind(rim => rim.Vertices.Fold(0.0, static (bound, vertex) =>
                Math.Max(bound, Math.Sqrt((vertex.X * vertex.X) + (vertex.Y * vertex.Y)))) is var radius
            && radius > 0.0 && double.IsFinite(radius)
                ? Fin.Succ(radius + policy.ArcChordErrorMm)
                : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, None, "guard:holder-footprint").ToError()));

    // The gouge tolerance shrinks the swept face before the protected surfaces test against it, so a contact
    // within the admitted tolerance is not a hazard; every other obstacle class tests the raw face.
    private static Fin<Seq<Hazard>> Contacts(SweptEnvelope swept, GuardRequest request, ObstacleRow row) =>
        from obstacles in row.Obstacles(request)
        from rows in Faces(swept).Filter(face => row.Tests(request, face.Contact)).Traverse(face =>
            from envelope in row.Axis == "protected" && request.Policy.GougeToleranceMm > 0.0
                ? face.Envelope.Traverse(loop => Offset(
                        loop, -request.Policy.GougeToleranceMm, JoinType.Round, EndType.Closed, request.Policy.RegionOffset))
                    .As().Map(static offsets => offsets.Bind(identity))
                : Fin.Succ(face.Envelope)
            from hazards in obstacles.Traverse(loop => Intersections(envelope, Seq(loop))
                .Map(witnesses => witnesses.Map(witness => row.Mint(loop, face.Contact, witness)))).As()
            select hazards.Bind(identity)).As()
        select rows.Bind(identity);

    private static Fin<Seq<Hazard>> FixedContacts(SweptEnvelope swept, GuardRequest request) =>
        from candidates in StaticCandidates(swept, request)
        let zones = candidates.Filter(zone => zone.Active.Contains(request.State)
            && Math.Min(request.Part.Cursor.Z, request.Target.Z) <= zone.Upper.As(LengthUnit.Millimeter)
            && Math.Max(request.Part.Cursor.Z, request.Target.Z) >= zone.Lower.As(LengthUnit.Millimeter))
        from rows in zones.Traverse(zone => Faces(swept).Traverse(face =>
            Intersections(face.Envelope, zone.Keepouts.Concat(zone.Walls))
                .Map(witnesses => witnesses.Map(witness => (Hazard)new Hazard.Fixed(zone, face.Contact, witness)))).As()).As()
        select rows.Bind(static row => row.Bind(identity));

    private static Fin<Loop> Chords(Loop exact, double error) =>
        ArcAlgebra.Densify(new ArcProjection.Lower(exact, error))
            .Bind(static trace => trace is ArcTrace.Densified densified
                ? Fin.Succ(densified.Receipt.Result)
                : Fin.Fail<Loop>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "guard:arc-projection-shape")));

    private static Fin<Seq<ExclusionZone>> StaticCandidates(SweptEnvelope swept, GuardRequest request) =>
        swept.Combined.IsEmpty
            ? Fin.Succ(Seq<ExclusionZone>())
            : request.Stock.Index.Map(index => Spatial.Apply(new SpatialOp.Query(index, new SpatialQuery.Range(swept.Bound, None)))
                    .Bind(answer => answer is SpatialAnswer.Result { Value: QueryResult.Hits hits }
                        ? hits.Ids.Exists(id => id < 0 || id >= request.Fixture.Zones.Count)
                            ? Fin.Fail<Seq<ExclusionZone>>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "guard:index-ordinal"))
                            : Fin.Succ(hits.Ids.Map(id => request.Fixture.Zones[id]))
                        : Fin.Fail<Seq<ExclusionZone>>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "guard:index-answer"))))
                .IfNone(Fin.Succ(request.Fixture.Zones));

    private static Fin<Option<ClearanceEvidence>> Channel(Loop trajectory, GuardStock stock, GuardPolicy policy) =>
        stock.Channel.Map(channel => Samples(trajectory, policy)
                .Bind(samples => Clearance(samples, channel, policy.Route).Map(Some)))
            .IfNone(Fin.Succ(Option<ClearanceEvidence>.None));

    private static Fin<Arr<Point3d>> Samples(Loop trajectory, GuardPolicy policy) =>
        Range(0, trajectory.Spans).ToSeq().Traverse(span => {
            Point3d from = trajectory.At(span);
            Point3d to = trajectory.At(span + 1);
            return policy.ClearanceSegments(from.DistanceTo(to)).Map(segments =>
                Range(0, segments + 1).ToSeq().Map(index => from + ((double)index / segments * (to - from))));
        }).As().Map(spans => spans.Bind(identity)
            .DistinctBy(point => Quantized(point, policy.Tolerance.Absolute.Value)).ToArr());

    // Sample points arrive from independent span walks that share their endpoints, so the duplicate census is a
    // TOLERANCE question: exact-double equality keeps two copies of a shared endpoint that differ by one ulp.
    private static (long X, long Y, long Z) Quantized(Point3d point, double grid) => (
        (long)Math.Round(point.X / grid),
        (long)Math.Round(point.Y / grid),
        (long)Math.Round(point.Z / grid));

    private static Fin<ClearanceEvidence> Clearance(Arr<Point3d> points, CurveSkeleton channel, ProbeRoute route) {
        using MemoryOwner<double> values = MemoryOwner<double>.Allocate(points.Count, AllocationMode.Clear);
        // The measured lane's floor is the union's own column, read through the generated switch: a type test plus a
        // null-forgiving projection restates a decision the closed family already answers.
        Option<int> floor = route.Switch(
            reference: static _ => Option<int>.None,
            measured: row => points.Count >= row.MinimumActionsPerThread ? Some(row.MinimumActionsPerThread) : None);
        bool parallel = floor.IsSome;
        // The clearance walk is ONE dimension; the 2D helper's second extent exists only because the action contract
        // is 2D, so the row extent stays the point census and the column extent stays the single degenerate row.
        floor.Iter(minimum => ParallelHelper.For2D(
            0,
            points.Count,
            0,
            1,
            new ClearanceAction(points, channel, values.Memory),
            minimum));
        if (!parallel)
            toSeq(points).Map(static (point, index) => (Point: point, Index: index))
                .Iter(cell => values.Span[cell.Index] = channel.Clearance(cell.Point).Radius);
        if (!TensorPrimitives.IsFiniteAll(values.Span))
            return Fin.Fail<ClearanceEvidence>(new GeometryFault.DegenerateInput(Kind.Curve, None, "guard:clearance-finite").ToError());
        int minimum = TensorPrimitives.IndexOfMin(values.Span);
        ClearanceNode witness = channel.Clearance(points[minimum]);
        return Fin.Succ(new ClearanceEvidence(
            values.Span[minimum],
            points[minimum],
            witness.NearestEdge >= 0 ? Some(witness.NearestEdge) : None,
            route,
            parallel,
            points.Count));
    }

    private static Fin<(Seq<Hazard> Hazards, Seq<string> Warnings)> Probe(GuardProbe probe, Point3d target) => probe.Switch(
        state: target,
        voxel: static (point, row) => ProbeVoxel(row, point),
        robot: static (_, row) => ProbeRobot(row));

    // The native cause is EVIDENCE: a discarded provider message leaves an operator with a locus and no reason, so
    // both the acquire and the probe legs carry theirs onto the typed rail, and the lease's own release rail joins
    // the result rather than throwing out of a `Fin`-declaring boundary.
    private static Fin<(Seq<Hazard> Hazards, Seq<string> Warnings)> ProbeVoxel(GuardProbe.Voxel probe, Point3d target) =>
        Try.lift(probe.Acquire).Run()
            .MapFail(static error => new FabricationFault.PolicyInadmissible(
                FabConcern.Toolpath, $"guard:voxel-acquire:{error.Message}"))
            .Bind(static lease => lease)
            .Bind(lease => Try.lift(() => Seq(
                        (VoxelObstacle.Stock, lease.Stock),
                        (VoxelObstacle.Fixture, lease.Fixture),
                        (VoxelObstacle.Protected, lease.Protected))
                    .Bind(row => VoxelContacts(lease.Tool, row.Item2, row.Item1, probe.Ray, target)))
                .Run()
                .MapFail(static error => new GeometryFault.DegenerateInput(
                    Kind.Curve, None, $"guard:voxel-native:{error.Message}").ToError())
                .Bind(hazards => lease.Release().Map(_ => (hazards, Seq<string>()))));

    private static Seq<Hazard> VoxelContacts(Voxels tool, Voxels obstacle, VoxelObstacle kind, VoxelRay ray, Point3d target) {
        using Voxels contact = tool.voxBoolIntersect(obstacle);
        if (contact.bIsEmpty())
            return Seq<Hazard>();
        PVector search = new((float)ray.Search.X, (float)ray.Search.Y, (float)ray.Search.Z);
        PVector direction = new((float)ray.Direction.X, (float)ray.Direction.Y, (float)ray.Direction.Z);
        contact.CalculateProperties(out float volumeMm3, out _);
        bool rayHit = contact.bRayCastToSurface(search, direction, out PVector surface);
        Point3d witness = rayHit ? new Point3d(surface.X, surface.Y, surface.Z) : target;
        return Seq<Hazard>(new Hazard.Voxel(new VoxelContact(kind, contact.bIsInside(search), witness, volumeMm3, contact.nMemUsage())));
    }

    // The planner-error gate is now an upstream INVARIANT rather than a runtime test: `Kinematics/cell` mints
    // `MotionEvidence` only after its own program errors are empty, so a trajectory that reached this probe already
    // planned clean, and the atom's admission proves the joint rows non-empty, finite, and duration-aligned.
    private static Fin<(Seq<Hazard> Hazards, Seq<string> Warnings)> ProbeRobot(GuardProbe.Robot probe) =>
        Try.lift<Fin<RobotCollisionEvidence>>(() => probe.Provider.Check(probe.Request))
            .Run()
            .MapFail(error => FabricationFault.Joint(
                new JointDiagnostic.Configuration(error.Message, nameof(IRobotCollisionProvider)), 0).ToError())
            .Bind(static collision => collision)
            .Bind(collision => AdmitRobotEvidence(probe.Request, collision).Switch(
                accepted: static row => Fin.Succ((
                    row.HasCollision
                        ? Seq<Hazard>(new Hazard.Robot(new RobotContact(
                            row.Target,
                            row.Meshes,
                            row.Targets,
                            row.DurationSeconds,
                            row.Warnings)))
                        : Seq<Hazard>(),
                    row.Warnings)),
                refused: static row => Fin.Fail<(Seq<Hazard>, Seq<string>)>(
                    FabricationFault.Joint(
                        new JointDiagnostic.Configuration(row.Field, nameof(IRobotCollisionProvider)), 0).ToError())));

    private static RobotCollisionAdmission AdmitRobotEvidence(
        CellCollisionRequest request,
        RobotCollisionEvidence collision) {
        if (collision is null)
            return new RobotCollisionAdmission.Refused("guard:robot-evidence:null");
        if (collision.Meshes < 0)
            return new RobotCollisionAdmission.Refused("guard:robot-evidence:meshes");
        if (collision.HasCollision && string.IsNullOrWhiteSpace(collision.Target))
            return new RobotCollisionAdmission.Refused("guard:robot-evidence:target");
        // The trajectory's own `RunWarning` rows are `Kinematics`' and ride the `MotionEvidence` the caller already
        // holds; re-emitting them here would file a producing plane's warning under this one on
        // `rasm.fabrication.run.warnings`. Only the provider's collision text is this probe's to publish.
        return new RobotCollisionAdmission.Accepted(
            collision.HasCollision,
            collision.Target,
            collision.Meshes,
            request.Joints.Count,
            request.Motion.Cycle.TotalSeconds,
            collision.Warnings);
    }

    private static Fin<Seq<Loop>> Offset(Loop path, double distance, JoinType join, EndType end, OffsetPolicy policy) =>
        PolygonAlgebra.Apply(new PolygonOp.Offset(Seq(path), new OffsetField.Uniform(distance), join, end, policy), Op.Of())
            .Bind(static trace => trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result.Nodes.Filter(static node => !node.IsHole).Map(static node => node.Boundary))
                : Fin.Fail<Seq<Loop>>(Op.Of(name: nameof(Offset)).InvalidResult()));

    private static Fin<Seq<ContactWitness>> Intersections(Seq<Loop> subject, Seq<Loop> clip) =>
        subject.IsEmpty || clip.IsEmpty
            ? Fin.Succ(Seq<ContactWitness>())
            : PolygonAlgebra.Apply(new PolygonOp.Boolean(subject, clip, BooleanOp.Intersection, PolygonFill.NonZero), Op.Of())
                .Bind(static trace => trace is PolygonTrace.Regions regions
                    // The witness locates the CONTACT: an overlap region's first vertex is an arbitrary seam the
                    // Boolean happened to open on, so two runs of one contact could report two different points.
                    ? Fin.Succ(regions.Result.Nodes.Filter(static node => !node.IsHole)
                        .Map(static node => new ContactWitness(node.Centroid, Math.Abs(node.SignedArea))))
                    : Fin.Fail<Seq<ContactWitness>>(Op.Of(name: nameof(Intersections)).InvalidResult()));
}

// The clearance-parallel measuring case: one probe census derived from the channel's own geometry, and the
// two lanes the ClearanceParallel claim names as folds over IDENTICAL points — the partitioned For2D
// reduction against the sequential clearance walk — each returning the same minimum, so a lane divergence
// is a correctness failure before it is a speed claim. The harness times each lane; the bench edge projects
// the pair through the AppHost field map with the reference lane's cost riding as reference evidence.
public static class ClearanceBench {
    public const int PartitionFloor = 64;
    private const int ArcSteps = 8;

    // Probe census: every node position plus ArcSteps interpolated stations per arc — derived from the
    // channel, never freehand, so the census scales with the workload and two runs over one channel agree.
    public static Fin<Arr<Point3d>> Probes(CurveSkeleton channel) =>
        channel is { NodeCount: > 0, ArcCount: > 0 }
            ? Fin.Succ(Range(0, channel.NodeCount).ToSeq()
                .Map(node => new Point3d(channel.NodeX[node], channel.NodeY[node], channel.NodeZ[node]))
                .Concat(Range(0, channel.ArcCount).ToSeq().Bind(arc => {
                    Point3d from = new(channel.NodeX[channel.ArcFrom[arc]], channel.NodeY[channel.ArcFrom[arc]], channel.NodeZ[channel.ArcFrom[arc]]);
                    Point3d to = new(channel.NodeX[channel.ArcTo[arc]], channel.NodeY[channel.ArcTo[arc]], channel.NodeZ[channel.ArcTo[arc]]);
                    return Range(1, ArcSteps).ToSeq().Map(step => from + ((double)step / (ArcSteps + 1) * (to - from)));
                })).ToArr())
            : Fin.Fail<Arr<Point3d>>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "bench:clearance-parallel"));

    // Measured lane: the exact partitioned reduction the claim's vectorized column names.
    public static double Measured(Arr<Point3d> points, CurveSkeleton channel) {
        using MemoryOwner<double> values = MemoryOwner<double>.Allocate(points.Count, AllocationMode.Clear);
        ParallelHelper.For2D(0, points.Count, 0, 1, new ClearanceAction(points, channel, values.Memory), PartitionFloor);
        return values.Span[TensorPrimitives.IndexOfMin(values.Span)];
    }

    // Reference lane: the sequential clearance walk the claim's reference column names.
    public static double Reference(Arr<Point3d> points, CurveSkeleton channel) {
        using MemoryOwner<double> values = MemoryOwner<double>.Allocate(points.Count, AllocationMode.Clear);
        toSeq(points).Map(static (point, index) => (Point: point, Index: index))
            .Iter(cell => values.Span[cell.Index] = channel.Clearance(cell.Point).Radius);
        return values.Span[TensorPrimitives.IndexOfMin(values.Span)];
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
