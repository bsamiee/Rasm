# [RASM_FABRICATION_GUARD]

`Guard` owns fail-closed motion admission from one aggregate request through arc-true planar sweep, protected-surface gouge, fixture and stock collision, medial-clearance, voxel-field, and robot-cell probes. `GuardReceipt` retains every hazard, its overlap severity, the probe scope it executed, and every provider warning, while provider errors terminate on the typed failure rail; no probe hides a prior contact or degrades a geometric failure into an empty result.

`Guard.Check` consumes one admitted `GuardRequest`. `GuardScope` separates a probed verdict from one elided by the clearance plane, so `GuardReceipt.Proven` distinguishes tested-and-clear from untested where `Clear` alone cannot. `HolderState` makes mounted and certified holder evidence mutually exclusive, `HolderCertificate.Admit` binds omission evidence to the exact `ToolAssembly.Identity`, cutter, operation, scope, and `ConservativeEnvelope` holder footprint, and native `Voxels` custody terminates inside the probe capsule.

## [01]-[INDEX]

- [02]-[GUARD]: `GuardRequest` closes aggregate admission, `HolderState` closes holder posture, `GuardScope` closes probe disposition, `GuardProbe` adds sidecar voxel and robot-cell evidence, and `Guard.Check` returns one accumulated `GuardReceipt`.

## [02]-[GUARD]

- Owner: `GuardRequest` is the admitted move, part, stock, fixture, fixture-state, policy, and probe aggregate; `GuardReceipt` is its evidence-complete result. `Fixture.Zones` is the sole exclusion-zone owner and the spatial-index ordinal domain; stock carries blank, forbidden, and snapshot geometry only.
- Cases: `HolderState` admits a mounted `ToolAssembly` or an identity-bound `HolderCertificate`; `ProbeRoute` admits the scalar reference or receipt-backed measured path; `GuardScope` admits a probed or plane-elided disposition; `GuardProbe` admits voxel-field and robot-cell providers; `RobotCollisionAdmission` closes accepted and refused provider evidence; `Hazard` closes gouge, fixed-zone, static-keepout, stock, channel, voxel, and robot contact.
- Law: every owner on this plane accumulates its INDEPENDENT axes through the `AdmissionSlots.Gate` deferred-mint arity and refuses on `<owner>:<axis>`, so a caller learns each bad column and a refusal token names the one that failed; the gate threads `FabConcern.Toolpath` and the locus into `FabricationFault.Inadmissible` on the failing arm alone, so a passing gate allocates nothing and a plane-local `Of(admitted, locus)` wrapper closing over the one concern has no job left. Dimensioned columns ride the kernel lane that names their gate — `ToleranceLane.Gouge` for the swept-face allowance, `ToleranceLane.Chord` for arc densification — so `Band` owns each range and no validator re-tests finiteness. The gouge allowance is `Option` because zero shrink is a real posture that `Band.Length` opens above; the clearance plane, the channel margin, and the probe pitch stay bare measures, being a work-coordinate ordinate, an additive standoff, and a sampling pitch respectively, none of which any lane derives.
- Cases: `FabricationBenchClaims` rosters the five kernel `BenchClaim` rows — NFP placement, ICP probe fit, skeleton offset, bend search, parallel clearance. `AcceptedBenchmarkClaim` binds one accepted result to the host digest its pass was stamped over, and `ProbeRoute.Measured` admits only the `ClearanceParallel` case, so unrelated solver evidence cannot authorize parallel clearance.
- Law: every roster row pairs with the measuring case beside its lane owner — `NestBench` at `Nesting/nfp`, `ProbeBench` at `Verify/probing`, `SkeletonBench` at `Toolpath/skeleton`, `BrakeBench` at `Forming/brake`, `ClearanceBench` here — each a workload admission plus the measured fold in folder types; measurement columns and the receipt projection stay the bench edge's under the `Rasm.AppHost/Observability/benchmarks#CLAIM_FIELD_MAP` Fabrication row, and `ClearanceBench.Reference` is the same-run cost the field map's reference-evidence law carries for the one lane-pair claim.
- Law: `AcceptedBenchmarkClaim.Admit` takes judgment as a `Func<string, UInt128, bool>` seam over the claim key and the `BenchmarkReceipt.ClaimKey` digest, the exact pair `BenchmarkGate.Judge` at `Rasm.AppHost/Observability/benchmarks#CLAIM_FIELD_MAP` resolves when it stamps `BenchmarkVerdict.Pass` — the Fabrication row keys `{Suite}/{Case}` under that page's own field map. This package supplies the bench roster and the host digest, never a verdict, and never persists one.
- Entry: `Guard.Check(GuardRequest)` preserves the frozen `Check` operation name and accumulates independent contacts through `Traverse`, `As`, and `Bind`.
- Auto: planar straight and circular moves lower once to an arc-true trajectory, round-ended offset sweeps retain cutter and holder separation, one `Surfaces` row set traverses every planar obstacle class against the shared cutter-and-holder `Faces` table, feed moves drop the cutter face against stock while the holder face always tests, and channel pinch uses the larger swept radius with the admitted margin.
- Receipt: `HolderEvidence` carries mounted or certified payload without a boolean cross-product; `ContactWitness` carries the contact point and its overlap area so `Hazard.Severity` ranks contacts rather than leaving them unordered; `ClearanceEvidence` retains minimum medial clearance, the optional skeleton witness, the requested route, and whether the parallel substrate executed; `VoxelContact` retains obstacle, membership, overlap volume, ray witness, and native memory; `RobotContact` retains provider target, meshes, duration, target census, and warnings, and PRESENCE of one is the collision — `RobotCollisionEvidence.Hit` and `RobotCollisionAdmission.Accepted.Contact` are both `Option`, so no reader pairs a flag with columns meaningful only under it.
- Receipt: `GuardReceipt` keeps its name on evidence and band it genuinely carries — `Hazards` with `Severity` ordering, `Scope`, and `Holder` — and does NOT seat on `Receipt<TEvidence>`: that carrier requires a `ContentKey` and an `Instant`, and `Guard.Check` runs once per `Move` inside the commit walk. `EgressKind` names machine artifacts, so no row keys a motion-admission verdict, and a stamp on a pure per-move fold buys a clock read per move for a fact nothing settles. `Clear` and `Proven` stay a PAIR because `GuardScope` makes them different questions — no hazards found, versus no hazards found by a lane that ran.
- Packages: `ToolMagazine.HolderEnvelope` derives mounted and certified holder footprints; `ArcAlgebra.Densify` preserves circular motion; `PolygonAlgebra.Apply` owns offset and intersection and receives the calling `Op` key so a trace refusal names its operation; `RegionNode.SignedArea` supplies contact severity without a second measure pass; `Spatial.Apply` owns indexed pruning; `CurveSkeleton.Clearance` owns arbitrary-probe clearance; `MemoryOwner<T>`, `ParallelHelper.For2D` with the admitted partition floor, and `TensorPrimitives` own pooled measured clearance reduction; PicoGK owns copied SDF intersection, membership, and ray witnesses; `MotionEvidence` supplies the admitted joint-and-duration trajectory the cell probe tests; `IRobotCollisionProvider` owns the executable robot-cell collision boundary and every provider handle behind it; kernel `Rasm.Domain` supplies `Tolerance` and `ToleranceLane` for the gate columns and `Rasm.Element` `AdmissionSlots` the one slot primitive, while `UnitsNet` `Length` carries the policy head's dimensioned magnitudes.
- Growth: a new obstacle is one `Hazard` case and one `Surfaces` row, a new swept face is one `Faces` row every obstacle class inherits, a new provider is one `GuardProbe` case, and a new execution substrate is one evidence-carrying `ProbeRoute` case.
- Boundary: `Clearance` and `ArcSpan` are the named statement kernels — pooled measured reduction and arc-frame numerics respectively; Rhino-native planar geometry stays inside the package wire, the cell probe names NO provider type — `CellCollisionRequest` carries the frozen `MotionEvidence` trajectory and a kernel `MeshSpace` environment, so the `Robots` and Rhino3dm alias crossing stays solely at `Kinematics/cell` — and PicoGK resources remain bracketed inside `ProbeVoxel`; no provider geometry escapes `GuardReceipt`.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HolderState {
    private HolderState() { }

    public sealed record Mounted(ToolAssembly Assembly) : HolderState;
    public sealed record Certified(HolderCertificate Certificate) : HolderState;
}

public static class FabricationBenchClaims {
    public const string Suite = "rasm.fabrication";

    public static readonly BenchClaim NfpPlacement = new(Op.Of(name: $"{Suite}/nfp-placement"), "Nest.Solve", "Nest.Solve", 1.0);
    public static readonly BenchClaim IcpProbeFit = new(Op.Of(name: $"{Suite}/icp-probe-fit"), "Probe.Inspect", "Probe.Inspect", 1.0);
    public static readonly BenchClaim SkeletonOffset = new(Op.Of(name: $"{Suite}/skeleton-offset"), "Skeleton.Walk", "Skeleton.Walk", 1.0);
    public static readonly BenchClaim BendSearch = new(Op.Of(name: $"{Suite}/bend-search"), "BendSequence.Plan", "BendSequence.Plan", 1.0);
    public static readonly BenchClaim ClearanceParallel = new(Op.Of(name: $"{Suite}/clearance-parallel"), "ParallelHelper.For2D", "CurveSkeleton.Clearance", 1.0);
}

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
        from _aggregate in AdmissionSlots.Accumulate(Seq(
                AdmissionSlots.Gate(bench is not null, FabConcern.Toolpath, "benchmark-claim:bench", FabricationFault.Inadmissible),
                AdmissionSlots.Gate(host != UInt128.Zero, FabConcern.Toolpath, "benchmark-claim:host", FabricationFault.Inadmissible),
                AdmissionSlots.Gate(judged is not null, FabConcern.Toolpath, "benchmark-claim:judge-absent", FabricationFault.Inadmissible)))
            .As()
            .ToFin()
        from accepted in Op.Of().Catch(() => Fin.Succ(judged(bench.Claim, host)))
        from _judged in accepted
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("guard", "benchmark-claim:refused"))
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

[ComplexValueObject]
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
        ref ValidationError? validationError,
        ref MotionEvidence motion,
        ref MeshSpace environment,
        ref int environmentPlane,
        ref double linearStepMm,
        ref double angularStepRad,
        ref Arr<int> first,
        ref Arr<int> second) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(motion is not null, FabConcern.Toolpath, "cell-collision-request:motion", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(environment is not null && environmentPlane >= 0,
                FabConcern.Toolpath, "cell-collision-request:environment", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(linearStepMm > 0.0 && double.IsFinite(linearStepMm),
                FabConcern.Toolpath, "cell-collision-request:linear-step", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(angularStepRad > 0.0 && double.IsFinite(angularStepRad),
                FabConcern.Toolpath, "cell-collision-request:angular-step", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(
                first.ForAll(static index => index >= 0) && second.ForAll(static index => index >= 0),
                FabConcern.Toolpath, "cell-collision-request:mechanism-ordinal", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static _ => new ValidationError("cell-collision-request"),
            Succ: static _ => null);
    }

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

public sealed record RobotHit(string Target, int Meshes);

public sealed record RobotCollisionEvidence(Option<RobotHit> Hit, Seq<string> Warnings);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RobotCollisionAdmission {
    private RobotCollisionAdmission() { }

    public sealed record Accepted(Option<RobotContact> Contact, Seq<string> Warnings) : RobotCollisionAdmission;
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
public sealed partial class VoxelRay {
    public Point3d Search { get; }
    public Vector3d Direction { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Point3d search,
        ref Vector3d direction) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(search.IsValid, FabConcern.Toolpath, "voxel-ray:search", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(direction.IsValid && !direction.IsZero, FabConcern.Toolpath, "voxel-ray:direction", FabricationFault.Inadmissible))
            .Apply(static (_, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static _ => new ValidationError("voxel-ray"),
            Succ: static _ => null);
    }

    public static Fin<VoxelRay> Admit(Point3d search, Vector3d direction) =>
        Validate(search, direction, out VoxelRay ray).Admitted(ray);
}

public sealed class VoxelLease : IDisposable {
    internal static readonly Op ReleaseBoundary = Op.Of(name: "guard:voxel-release");

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
            : Fin.Fail<VoxelLease>(new KernelFault.InvalidValue("guard", "guard:voxel-lease"));

    public Fin<Unit> Release() => Custody.Dispose(Seq(Tool, Stock, Fixture, Protected), ReleaseBoundary);

    public void Dispose() => _ = Release();
}

[ComplexValueObject]
public sealed partial class HolderCertificate {
    public UInt128 AssemblyIdentity { get; }
    public CutterForm Cutter { get; }
    public int Operation { get; }
    public BoundingBox Scope { get; }
    public Loop ConservativeEnvelope { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UInt128 assemblyIdentity,
        ref CutterForm cutter,
        ref int operation,
        ref BoundingBox scope,
        ref Loop conservativeEnvelope) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(assemblyIdentity != UInt128.Zero, FabConcern.Toolpath, "holder-certificate:assembly", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(operation >= 0, FabConcern.Toolpath, "holder-certificate:operation", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(scope.IsValid && scope.Diagonal.Length > 0.0,
                FabConcern.Toolpath, "holder-certificate:scope", FabricationFault.Inadmissible))
            .Apply(static (_, _, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static _ => new ValidationError("holder-certificate"),
            Succ: static _ => null);
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
public sealed partial class GuardPolicy {
    public Length ClearancePlane { get; }
    public Context Tolerance { get; }
    public OffsetPolicy SweepOffset { get; }
    public OffsetPolicy RegionOffset { get; }

    public Option<Tolerance> Gouge { get; }

    public Tolerance ArcChord { get; }

    public Length ChannelMargin { get; }

    public Length ClearanceProbeStep { get; }
    public int MaximumSweepSegments { get; }
    public int MaximumClearanceProbes { get; }

    public ProbeRoute Route { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length clearancePlane,
        ref Context tolerance,
        ref OffsetPolicy sweepOffset,
        ref OffsetPolicy regionOffset,
        ref Option<Tolerance> gouge,
        ref Tolerance arcChord,
        ref Length channelMargin,
        ref Length clearanceProbeStep,
        ref int maximumSweepSegments,
        ref int maximumClearanceProbes,
        ref ProbeRoute route) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(double.IsFinite(clearancePlane.Millimeters),
                FabConcern.Toolpath, "guard-policy:clearance-plane", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(
                gouge.Map(static row => row.Lane == ToleranceLane.Gouge && row.IsValid).IfNone(true),
                FabConcern.Toolpath, "guard-policy:gouge", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(sweepOffset.IsValid && regionOffset.IsValid,
                FabConcern.Toolpath, "guard-policy:offset-policy", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(
                arcChord.Lane == ToleranceLane.Chord && arcChord.IsValid,
                FabConcern.Toolpath, "guard-policy:arc-chord", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(
                channelMargin.Millimeters >= 0.0 && double.IsFinite(channelMargin.Millimeters),
                FabConcern.Toolpath, "guard-policy:channel-margin", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(
                clearanceProbeStep.Millimeters > 0.0 && double.IsFinite(clearanceProbeStep.Millimeters),
                FabConcern.Toolpath, "guard-policy:probe-step", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(maximumSweepSegments >= 8, FabConcern.Toolpath, "guard-policy:sweep-capacity", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(maximumClearanceProbes >= 2, FabConcern.Toolpath, "guard-policy:probe-capacity", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(route.Switch(
                reference: static _ => true,
                measured: static row => row.MinimumActionsPerThread > 0
                    && row.Claim.Bench.Claim.Equals(FabricationBenchClaims.ClearanceParallel.Claim)),
                FabConcern.Toolpath, "guard-policy:probe-route", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _, _, _, _, _, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static _ => new ValidationError("guard-policy"),
            Succ: static _ => null);
    }

    public static Fin<GuardPolicy> Admit(
        Length clearancePlane,
        Context tolerance,
        OffsetPolicy sweepOffset,
        OffsetPolicy regionOffset,
        Option<Tolerance> gouge,
        Tolerance arcChord,
        Length channelMargin,
        Length clearanceProbeStep,
        int maximumSweepSegments,
        int maximumClearanceProbes,
        ProbeRoute route) =>
        Validate(clearancePlane, tolerance, sweepOffset, regionOffset, gouge, arcChord,
            channelMargin, clearanceProbeStep, maximumSweepSegments, maximumClearanceProbes, route,
            out GuardPolicy policy).Admitted(policy);

    public Fin<int> ClearanceSegments(double length) =>
        double.IsFinite(length) && length >= 0.0
        && Math.Ceiling(length / ClearanceProbeStep.Millimeters) <= MaximumClearanceProbes
            ? Fin.Succ(Math.Max(1, (int)Math.Ceiling(length / ClearanceProbeStep.Millimeters)))
            : Fin.Fail<int>(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:clearance-capacity"));
}

[ComplexValueObject]
public sealed partial class GuardPart {
    public Point3d Cursor { get; }
    public Seq<Loop> Protected { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Point3d cursor,
        ref Seq<Loop> @protected) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(cursor.IsValid, FabConcern.Toolpath, "guard-part:cursor", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(
                @protected.ForAll(static loop => loop.Closed && loop.Count >= 3),
                FabConcern.Toolpath, "guard-part:protected", FabricationFault.Inadmissible))
            .Apply(static (_, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static _ => new ValidationError("guard-part"),
            Succ: static _ => null);
    }

    public static Fin<GuardPart> Admit(Point3d cursor, Seq<Loop> @protected) =>
        Validate(cursor, @protected, out GuardPart part).Admitted(part);
}

[ComplexValueObject]
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
        ref ValidationError? validationError,
        ref Seq<Loop> rawBlank,
        ref Seq<Loop> forbidden,
        ref Seq<StockSnapshot> snapshots,
        ref CutterForm cutter,
        ref HolderState holder,
        ref Option<CurveSkeleton> channel,
        ref Option<SpatialIndex> index) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(!rawBlank.IsEmpty && rawBlank.ForAll(static loop => loop is not null && loop.Closed && loop.Count >= 3),
                FabConcern.Toolpath, "guard-stock:blank", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(forbidden.ForAll(static loop => loop is not null && loop.Closed && loop.Count >= 3),
                FabConcern.Toolpath, "guard-stock:forbidden", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(snapshots.ForAll(static snapshot => snapshot is not null && snapshot.Setup >= 0
                && snapshot.Machined.ForAll(static loop => loop is not null && loop.Closed)),
                FabConcern.Toolpath, "guard-stock:snapshots", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(cutter is not null && cutter.Diameter > 0.0 && double.IsFinite(cutter.Diameter),
                FabConcern.Toolpath, "guard-stock:cutter", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(holder.Switch(
                mounted: static row => row.Assembly.Identity != UInt128.Zero,
                certified: static row => row.Certificate.AssemblyIdentity != UInt128.Zero),
                FabConcern.Toolpath, "guard-stock:holder", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(channel.Map(ChannelValid).IfNone(true), FabConcern.Toolpath, "guard-stock:channel", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _, _, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static _ => new ValidationError("guard-stock"),
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
}

[ComplexValueObject]
public sealed partial class GuardRequest {
    public Move Move { get; }
    public GuardPart Part { get; }
    public GuardStock Stock { get; }
    public Fixture Fixture { get; }
    public FixtureState State { get; }
    public GuardPolicy Policy { get; }
    public Seq<GuardProbe> Probes { get; }
    public Point3d Target => Move.Target;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Move move,
        ref GuardPart part,
        ref GuardStock stock,
        ref Fixture fixture,
        ref FixtureState state,
        ref GuardPolicy policy,
        ref Seq<GuardProbe> probes) {
        Validation<Error, Unit> admitted = (
            AdmissionSlots.Gate(move.AxisFree, FabConcern.Toolpath, "swept-solid:oriented-move", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(
                HolderValid(stock.Holder, stock.Cutter, fixture.Operation, MotionScope(move, part.Cursor)),
                FabConcern.Toolpath, "guard-request:holder", FabricationFault.Inadmissible))
            .Apply(static (_, _) => unit)
            .As();
        validationError = admitted.Match<ValidationError?>(
            Fail: static _ => new ValidationError("guard-request"),
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
}

// --- [MODELS] --------------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
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

    private sealed record ObstacleRow(
        string Axis,
        Func<GuardRequest, Fin<Seq<Loop>>> Obstacles,
        Func<GuardRequest, CollisionContact, bool> Tests,
        bool Shrinks,
        Func<Loop, CollisionContact, ContactWitness, Hazard> Mint);

    private static readonly Arr<ObstacleRow> Surfaces = Arr(
        new ObstacleRow(
            "protected",
            static request => Fin.Succ(request.Part.Protected),
            static (_, _) => true,
            Shrinks: true,
            static (loop, contact, witness) => new Hazard.Gouge(loop, contact, witness)),
        new ObstacleRow(
            "keepout",
            static request => Fin.Succ(request.Stock.Forbidden),
            static (_, _) => true,
            Shrinks: false,
            static (loop, contact, witness) => new Hazard.Keepout(loop, contact, witness)),
        new ObstacleRow(
            "stock",
            static request => Fin.Succ(request.Stock.Current(request.Fixture.Operation)),
            static (request, contact) => request.Move is Move.Rapid || contact == CollisionContact.Holder,
            Shrinks: false,
            static (loop, contact, witness) => new Hazard.Stock(loop, contact, witness)));

    private static GuardScope PlanarScope(GuardRequest request) =>
        request.Move is Move.Rapid
        && Math.Min(request.Part.Cursor.Z, request.Target.Z) >= request.Policy.ClearancePlane.Millimeters
        && request.Stock.Channel.IsNone
        && request.Fixture.Zones.Filter(zone => zone.Active.Contains(request.State))
            .ForAll(zone => zone.Upper.As(LengthUnit.Millimeter) <= request.Policy.ClearancePlane.Millimeters)
        && request.Part.Protected.Concat(request.Stock.Forbidden).Concat(request.Stock.Current(request.Fixture.Operation))
            .ForAll(loop => loop.Bound().Max.Z <= request.Policy.ClearancePlane.Millimeters)
            ? new GuardScope.Elided(
                request.Policy.ClearancePlane.Millimeters,
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
                .Bind(span => Chords(span, state.Policy.ArcChord.Value))
                .Bind(result => result.Spans <= state.Policy.MaximumSweepSegments
                    ? Fin.Succ(result)
                    : Fin.Fail<Loop>(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:sweep-capacity"))));

    private static Fin<Loop> ArcSpan(Point3d from, Move.Circular move, GuardPolicy policy) {
        Vector3d a = from - move.Arc.Center;
        Vector3d b = move.Target - move.Arc.Center;
        double radius = a.Length;
        if (!double.IsFinite(radius)
            || radius <= 0.0
            || Math.Abs(from.Z - move.Target.Z) > policy.Tolerance.Absolute.Value
            || Math.Abs(from.Z - move.Arc.Center.Z) > policy.Tolerance.Absolute.Value
            || Math.Abs(radius - b.Length) > policy.ArcChord.Value)
            return Fin.Fail<Loop>(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:arc-motion"));
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
        select new SweptEnvelope(cutter, holder, evidence, Math.Max(stock.Radius, holderRadius) + policy.ChannelMargin.Millimeters);

    private static Fin<double> FootprintRadius(Loop footprint, GuardPolicy policy) =>
        (footprint.Bulges.ForAll(static bulge => bulge == 0.0)
            ? Fin.Succ(footprint)
            : Chords(footprint, policy.ArcChord.Value))
        .Bind(rim => rim.Vertices.Fold(0.0, static (bound, vertex) =>
                Math.Max(bound, Math.Sqrt((vertex.X * vertex.X) + (vertex.Y * vertex.Y)))) is var radius
            && radius > 0.0 && double.IsFinite(radius)
                ? Fin.Succ(radius + policy.ArcChord.Value)
                : Fin.Fail<double>(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:holder-footprint")));

    private static Fin<Seq<Hazard>> Contacts(SweptEnvelope swept, GuardRequest request, ObstacleRow row) =>
        from obstacles in row.Obstacles(request)
        from rows in Faces(swept).Filter(face => row.Tests(request, face.Contact)).Traverse(face =>
            from envelope in (row.Shrinks ? request.Policy.Gouge : Option<Tolerance>.None).Match(
                Some: allowance => face.Envelope.Traverse(loop => Offset(
                        loop, -allowance.Value, JoinType.Round, EndType.Closed, request.Policy.RegionOffset))
                    .As().Map(static offsets => offsets.Bind(identity)),
                None: () => Fin.Succ(face.Envelope))
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
            .Bind(static trace => trace
                .Lowering(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:arc-projection-shape"))
                .Map(static evidence => evidence.Result));

    private static Fin<Seq<ExclusionZone>> StaticCandidates(SweptEnvelope swept, GuardRequest request) =>
        swept.Combined.IsEmpty
            ? Fin.Succ(Seq<ExclusionZone>())
            : request.Stock.Index.Map(index => Spatial.Apply(new SpatialOp.Query(index, new SpatialQuery.Range(swept.Bound, None)))
                    .Bind(answer => answer is SpatialAnswer.Result { Value: QueryResult.Hits hits }
                        ? hits.Ids.Exists(id => id < 0 || id >= request.Fixture.Zones.Count)
                            ? Fin.Fail<Seq<ExclusionZone>>(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:index-ordinal"))
                            : Fin.Succ(hits.Ids.Map(id => request.Fixture.Zones[id]))
                        : Fin.Fail<Seq<ExclusionZone>>(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:index-answer"))))
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

    private static (long X, long Y, long Z) Quantized(Point3d point, double grid) => (
        (long)Math.Round(point.X / grid),
        (long)Math.Round(point.Y / grid),
        (long)Math.Round(point.Z / grid));

    private static Fin<ClearanceEvidence> Clearance(Arr<Point3d> points, CurveSkeleton channel, ProbeRoute route) {
        using MemoryOwner<double> values = MemoryOwner<double>.Allocate(points.Count, AllocationMode.Clear);
        Option<int> floor = route.Switch(
            reference: static _ => Option<int>.None,
            measured: row => points.Count >= row.MinimumActionsPerThread ? Some(row.MinimumActionsPerThread) : None);
        bool parallel = floor.IsSome;
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
            return Fin.Fail<ClearanceEvidence>(FabricationFault.Inadmissible(FabConcern.Toolpath, "guard:clearance-finite"));
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

    private static Fin<(Seq<Hazard> Hazards, Seq<string> Warnings)> ProbeVoxel(GuardProbe.Voxel probe, Point3d target) =>
        Op.Of().Catch(probe.Acquire)
            .Bind(lease => Op.Of().Catch(() => Fin.Succ(Seq(
                        (VoxelObstacle.Stock, lease.Stock),
                        (VoxelObstacle.Fixture, lease.Fixture),
                        (VoxelObstacle.Protected, lease.Protected))
                    .Bind(row => VoxelContacts(lease.Tool, row.Item2, row.Item1, probe.Ray, target))))
                .Map(hazards => (hazards, Seq<string>()))
                .Settled(lease.Release, VoxelLease.ReleaseBoundary));

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

    private static Fin<(Seq<Hazard> Hazards, Seq<string> Warnings)> ProbeRobot(GuardProbe.Robot probe) =>
        Op.Of().Catch(() => probe.Provider.Check(probe.Request))
            .Bind(collision => AdmitRobotEvidence(probe.Request, collision).Switch(
                accepted: static row => Fin.Succ((
                    row.Contact.ToSeq().Map(static contact => (Hazard)new Hazard.Robot(contact)),
                    row.Warnings)),
                refused: static row => Fin.Fail<(Seq<Hazard>, Seq<string>)>(
                    FabricationFault.Joint(
                        new JointDiagnostic.Configuration(row.Field, nameof(IRobotCollisionProvider)), None))));

    private static RobotCollisionAdmission AdmitRobotEvidence(
        CellCollisionRequest request,
        RobotCollisionEvidence collision) =>
        collision is null
            ? new RobotCollisionAdmission.Refused("guard:robot-evidence:null")
            : collision.Hit.Match<RobotCollisionAdmission>(
                None: () => new RobotCollisionAdmission.Accepted(
                    Option<RobotContact>.None, collision.Warnings),
                Some: hit => hit.Meshes < 0
                    ? new RobotCollisionAdmission.Refused("guard:robot-evidence:meshes")
                    : string.IsNullOrWhiteSpace(hit.Target)
                        ? new RobotCollisionAdmission.Refused("guard:robot-evidence:target")
                        : new RobotCollisionAdmission.Accepted(
                            Some(new RobotContact(
                                hit.Target,
                                hit.Meshes,
                                request.Joints.Count,
                                request.Motion.Cycle.TotalSeconds,
                                collision.Warnings)),
                            collision.Warnings));

    private static Fin<Seq<Loop>> Offset(Loop path, double distance, JoinType join, EndType end, OffsetPolicy policy) =>
        PolygonAlgebra.Apply(new PolygonOp.Offset(Seq(path), new OffsetField.Uniform(distance), join, end, policy), Op.Of())
            .Bind(static trace => trace
                .Regioned(new KernelFault.InvalidValue("guard", "guard:offset-trace"))
                .Map(static topology => topology.Nodes.Filter(static node => !node.IsHole).Map(static node => node.Boundary)));

    private static Fin<Seq<ContactWitness>> Intersections(Seq<Loop> subject, Seq<Loop> clip) =>
        subject.IsEmpty || clip.IsEmpty
            ? Fin.Succ(Seq<ContactWitness>())
            : PolygonAlgebra.Apply(new PolygonOp.Boolean(subject, clip, BooleanOp.Intersection, PolygonFill.NonZero), Op.Of())
                .Bind(static trace => trace
                    .Regioned(new KernelFault.InvalidValue("guard", "guard:intersection-trace"))
                    .Map(static topology => topology.Nodes.Filter(static node => !node.IsHole)
                        .Map(static node => new ContactWitness(node.Centroid, Math.Abs(node.SignedArea)))));
}

public static class ClearanceBench {
    public const int PartitionFloor = 64;
    private const int ArcSteps = 8;

    public static Fin<Arr<Point3d>> Probes(CurveSkeleton channel) =>
        channel is { NodeCount: > 0, ArcCount: > 0 }
            ? Fin.Succ(Range(0, channel.NodeCount).ToSeq()
                .Map(node => new Point3d(channel.NodeX[node], channel.NodeY[node], channel.NodeZ[node]))
                .Concat(Range(0, channel.ArcCount).ToSeq().Bind(arc => {
                    Point3d from = new(channel.NodeX[channel.ArcFrom[arc]], channel.NodeY[channel.ArcFrom[arc]], channel.NodeZ[channel.ArcFrom[arc]]);
                    Point3d to = new(channel.NodeX[channel.ArcTo[arc]], channel.NodeY[channel.ArcTo[arc]], channel.NodeZ[channel.ArcTo[arc]]);
                    return Range(1, ArcSteps).ToSeq().Map(step => from + ((double)step / (ArcSteps + 1) * (to - from)));
                })).ToArr())
            : Fin.Fail<Arr<Point3d>>(new KernelFault.InvalidValue("guard", "bench:clearance-parallel"));

    public static double Measured(Arr<Point3d> points, CurveSkeleton channel) {
        using MemoryOwner<double> values = MemoryOwner<double>.Allocate(points.Count, AllocationMode.Clear);
        ParallelHelper.For2D(0, points.Count, 0, 1, new ClearanceAction(points, channel, values.Memory), PartitionFloor);
        return values.Span[TensorPrimitives.IndexOfMin(values.Span)];
    }

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
