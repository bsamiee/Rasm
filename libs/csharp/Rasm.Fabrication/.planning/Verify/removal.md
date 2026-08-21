# [RASM_FABRICATION_REMOVAL]

`Removal.Verify` owns post-program stock truth: one admitted `VerifyPolicy` materializes stock and target through the shared voxel runtime, folds setup-framed cutter sweeps and non-cutting body membership over actual stock, and projects residual stock, snapshots, signed surface deviation, and tolerance evidence onto `FabricationResult.VerificationResult` before firing the settled receipt onto the fabrication tap.

`FabricationPolicy.Verify`, `VoxelWire`, `ToolMagazine.HolderEnvelope`, `StockSnapshot`, `ContentKey.Of`, and the `FabricationFact.Removal` projection remain frozen seams. Arc geometry reads the S0 atom's own admitted `SweepRadians` and `Radius`, loop identity reads `Loop.CanonicalBytes`, byte framing reads `FabricationCanon`, and native handles terminate inside one exception-capture and disposal capsule; only process atoms leave the Verify plane.

## [01]-[INDEX]

- [02]-[POLICY]: generated admission for removal resolution, setup framing, arc-length stationing, tolerance, and native budget.
- [03]-[STOCK_FOLD]: setup-ordered cutter sweeps and shank-plus-holder membership tests over one mutable stock lease.
- [04]-[SURFACE_TRUTH]: signed nominal-to-actual deviation, residual topology, and payload-complete snapshot identity.
- [05]-[VERDICT]: finding adjudication, result projection, and the settled-receipt fact fire.

## [02]-[POLICY]

- Owner: `VerifyPolicy` admits the complete removal request once and publishes its resolved setup partition; `SetupWindow` admits each partition, and `RemovalBands` rides as ONE named column block carrying the admitting `Context` and every verdict and evidence-coverage threshold that vocabulary does not already name.
- Cases: `SweepSampling` rows carry only the bound-to-arc-length conversion their name states — chord length, arc length, sagitta height — so every row feeds one arc-length generator over the move family and stationing stays row-invariant.
- Law: a band the kernel tolerance vocabulary NAMES is derived, never stored. `RemovalBands.Gouge` and `.Surface` read `Context.For(ToleranceLane.Gouge)` and `Context.For(ToleranceLane.Deviation)` off the block's own admitted context, so a shop tightening either gate moves one `Context.Override` and no policy column can drift from the lane it copies. The residue volumes and the two coverage ceilings name no lane, so they stay OWNED columns — typed as `Volume` and `UnitInterval`, whose own bands prove finiteness, non-negativity, and the closed unit range the record used to re-test by hand.
- Law: the verification runs on the CALLER's admitted context. The grid every residual loop, canonical preimage, and section walk reads is `Bands.Model`, so a project that tightened its absolute band verifies against that band rather than against a millimetre context the operation minted for itself and no caller could reach.
- Law: a circular move's sweep and radius are the S0 atom's OWN admitted columns. `Move.Circular.SweepRadians` is signed with magnitude in `(0, Tau]` and `Move.Circular.Radius` is the centre-to-target distance, both proved at `Move.Circular.Of`; this page reads them and mints no endpoint re-derivation, no angular epsilon, and no clockwise sign convention. A full turn and its zero-sweep twin are distinct admitted moves, so the generator separates them where an `atan2` difference cannot.
- Law: the setup partition resolves ONCE off the admitted members and is held on the policy, so the coverage gate, the depth gate, the fold, and the snapshot preimage read one sequence rather than four re-sorts of the same rows.
- Entry: `Removal.Verify(FabricationPolicy.Verify, FabricationInput, FabricationTap?)` is the sole public operation; the policy case carries the admitted `VerifyPolicy`, the prior residual, and the prior snapshots, and the tap defaults silent so a headless verification emits nothing and branches nowhere.
- Auto: generated factories reject primitive defects on the fabrication band, one `AdmissionSlots.Gate` fan proves stock lineage, setup partition, cutting-motion presence, tool-frame coverage, silhouette generability, and voxel demand together, and `Capture` encloses native source construction, voxelization, callback execution, and lease disposal.
- Exemption: `RequiredCells` folds a `BigInteger` axis product to prove the grid fits the cell cap before any allocation.
- Growth: a sampling law is one `SweepSampling` row, and an acceptance regime is one `RemovalBands` value — a lane-named gate moves at the context override with no column added here.
- Boundary: `VoxelWire` remains the only stock ingress and egress codec; native `Library`, `Voxels`, `Lattice`, and `Mesh` leases never cross the operation. A verification whose motion carries no cutting move is refused rather than answered with a fabricated air-cut ratio, so the ratio's denominator is positive by admission at every later read.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using PicoGK;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SweepSampling {
    public static readonly SweepSampling Chord = new("chord", static (radius, bound) =>
        ArcOf(radius, Math.Min(bound, 2.0 * radius)));
    public static readonly SweepSampling Arc = new("arc", static (_, bound) => bound);
    // Sagitta height h over a circle of radius r subtends chord 2*sqrt(2rh - h^2); the row converts that chord to
    // its own arc length so a deviation bound and a chord bound reach the one generator identically.
    public static readonly SweepSampling Sagitta = new("sagitta", static (radius, bound) =>
        ArcOf(radius, 2.0 * Math.Sqrt(Math.Max(0.0, (2.0 * radius * bound) - (bound * bound)))));

    // Every row converts its own bound into the one arc-length step the circular generator consumes.
    [UseDelegateFromConstructor]
    private partial double ArcStep(double radiusMm, double boundMm);

    // Stationing is TOTAL over an admitted move: the linear arm needs only its endpoints and the circular arm
    // reads the atom's own proved sweep and radius, so no arm can fail and no rail is widened to carry a failure.
    public Seq<Point3d> Project(Point3d from, Move move, double boundMm) => move.Switch(
        state: (From: from, Bound: boundMm, Row: this),
        rapid: static (state, row) => Linear(state.From, row.Target, state.Bound),
        linear: static (state, row) => Linear(state.From, row.Target, state.Bound),
        circular: static (state, row) => Circular(state.From, row, state.Row.ArcStep(row.Radius, state.Bound)));

    private static double ArcOf(double radiusMm, double chordMm) =>
        2.0 * radiusMm * Math.Asin(Math.Clamp(chordMm / (2.0 * radiusMm), 0.0, 1.0));

    private static Seq<Point3d> Linear(Point3d from, Point3d to, double stepMm) {
        int count = Math.Max(1, (int)Math.Ceiling(from.DistanceTo(to) / stepMm));
        return toSeq(Enumerable.Range(1, count)).Map(index =>
            index == count ? to : Lerp(from, to, (double)index / count));
    }

    // The admitted sweep IS the parameterization: the start radius vector rotates by `SweepRadians * t` about the
    // arc centre, so direction, magnitude, and the full-turn case all come from the atom and none from the endpoints.
    private static Seq<Point3d> Circular(Point3d from, Move.Circular arc, double stepMm) {
        Vector3d radial = from - arc.Arc.Center;
        int count = Math.Max(1, (int)Math.Ceiling(Math.Abs(arc.SweepRadians) * arc.Radius / stepMm));
        return toSeq(Enumerable.Range(1, count)).Map(index => {
            double t = (double)index / count;
            return index == count
                ? arc.Target
                : Revolved(arc.Arc.Center, radial, arc.SweepRadians * t, from.Z + ((arc.Target.Z - from.Z) * t));
        });
    }

    private static Point3d Revolved(Point3d center, Vector3d radial, double radians, double z) {
        (double Cos, double Sin) turn = (Math.Cos(radians), Math.Sin(radians));
        return new Point3d(
            center.X + (radial.X * turn.Cos) - (radial.Y * turn.Sin),
            center.Y + (radial.X * turn.Sin) + (radial.Y * turn.Cos),
            z);
    }

    private static Point3d Lerp(Point3d from, Point3d to, double t) =>
        new(from.X + ((to.X - from.X) * t), from.Y + ((to.Y - from.Y) * t), from.Z + ((to.Z - from.Z) * t));
}

// The acceptance regime as one named column block. Every member arrives ALREADY ADMITTED in a carrier that owns its
// own range — `Volume` carries dimension and finiteness, `UnitInterval` the closed unit band both ratios were
// hand-tested against — and the two model-space gates are DERIVED off the block's own context rather than stored,
// so the six-clause range body this record used to run has nothing left to prove and no separate admission exists.
public readonly record struct RemovalBands(
    Context Model,
    Volume Uncut,
    Volume Overcut,
    UnitInterval AirCut,
    UnitInterval Coverage) {
    // Depth past nominal rides the vocabulary's own sub-tolerance gouge lane; signed surface error rides the
    // deviation lane. Both READ the context, so an override moves the gate and a stale copy cannot exist.
    public Tolerance Gouge => Model.For(ToleranceLane.Gouge);
    public Tolerance Surface => Model.For(ToleranceLane.Deviation);
}

[ComplexValueObject]
public sealed partial class SetupWindow {
    public int Setup { get; }
    public int FirstMove { get; }
    public int Count { get; }
    public Plane Frame { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int setup,
        ref int firstMove,
        ref int count,
        ref Plane frame) {
        if (!(ValidityClaim.All(ValidityClaim.Nonnegative(setup), ValidityClaim.Nonnegative(firstMove), count > 0, frame.IsValid)))
            validationError = new ValidationError("removal:setup-window");
    }

    public static Fin<SetupWindow> Admit(int setup, int firstMove, int count, Plane frame) =>
        Validate(setup, firstMove, count, frame, out SetupWindow window).Admitted(window);
}

[ComplexValueObject]
public sealed partial class VerifyPolicy {
    public FabricationResult.Motion Motion { get; }
    public Point3d Origin { get; }
    public CutterForm Cutter { get; }
    public Option<ToolAssembly> Holder { get; }
    public VoxelWire Stock { get; }
    public VoxelWire Target { get; }
    public BoundingBox Bounds { get; }
    public Length VoxelSize { get; }

    // Cell counts run past `int` on a fine grid over a large envelope, and the branch count carrier is int-keyed, so
    // the cap stays a `long` with its own positivity clause rather than taking a type that cannot hold the value.
    public long VoxelCap { get; }

    public Length Station { get; }
    public Dimension SurfaceSamples { get; }
    public SweepSampling Sampling { get; }
    public RemovalBands Bands { get; }
    public CalibrationPolicy Calibration { get; }
    public Seq<SetupWindow> Setups { get; }
    public Map<int, Plane> ToolFrames { get; }

    // The resolved partition is DERIVED from admitted members, so it is out of construction, equality, and every
    // codec, and it is forced on first read. Four consumers previously re-sorted the same rows on every call.
    [IgnoreMember]
    private Seq<SetupWindow>? windows;

    public Seq<SetupWindow> Windows => windows ??= Resolved(Setups, Motion, Origin);

    // The grid every generator, preimage, and section walk reads is the CALLER's, carried on the band block, so one
    // admitted context serves the whole verification and no lane forks against a locally minted one.
    public Context Model => Bands.Model;

    // Station and voxel resolution both bound the sweep, so the finer of the two is the ONE step every generator,
    // silhouette profile, and membership walk consumes.
    public double StepMm => Math.Min(Station.Millimeters, VoxelSize.Millimeters);

    // Half the voxel edge is the finest silhouette feature the field can resolve, so it is the profile floor and
    // the beam-radius floor at once — a section thinner than this rasterizes to nothing.
    public double SilhouetteMm => VoxelSize.Millimeters * 0.5;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FabricationResult.Motion motion,
        ref Point3d origin,
        ref CutterForm cutter,
        ref Option<ToolAssembly> holder,
        ref VoxelWire stock,
        ref VoxelWire target,
        ref BoundingBox bounds,
        ref Length voxelSize,
        ref long voxelCap,
        ref Length station,
        ref Dimension surfaceSamples,
        ref SweepSampling sampling,
        ref RemovalBands bands,
        ref CalibrationPolicy calibration,
        ref Seq<SetupWindow> setups,
        ref Map<int, Plane> toolFrames) {
        // The quantity and count carriers already proved finiteness and their own ranges, so this body states only
        // the claims no carrier can: geometry validity, a positive cell cap, a positive resolution pair, and a tool
        // frame roster whose ordinals address real moves.
        if (!ValidityClaim.All(
            ValidityClaim.Finite(origin), bounds.IsValid,
            ValidityClaim.Positive(voxelSize.Millimeters), ValidityClaim.Positive(station.Millimeters),
            voxelCap > 0L, ValidityClaim.CountAtLeast(surfaceSamples.Value, floor: 1),
            toolFrames.ForAll(static row => ValidityClaim.All(ValidityClaim.Nonnegative(row.Key), row.Value.IsValid))))
            validationError = new ValidationError("removal:policy");
    }

    public static Fin<VerifyPolicy> Admit(
        FabricationResult.Motion motion,
        Point3d origin,
        CutterForm cutter,
        Option<ToolAssembly> holder,
        VoxelWire stock,
        VoxelWire target,
        BoundingBox bounds,
        Length voxelSize,
        long voxelCap,
        Length station,
        Dimension surfaceSamples,
        SweepSampling sampling,
        RemovalBands bands,
        CalibrationPolicy calibration,
        Seq<SetupWindow> setups,
        Map<int, Plane> toolFrames) =>
        Validate(motion, origin, cutter, holder, stock, target, bounds, voxelSize, voxelCap, station,
            surfaceSamples, sampling, bands, calibration, setups, toolFrames, out VerifyPolicy policy)
            .Admitted(policy);

    // An empty setup roster is ONE window over the whole program on the origin frame; a declared roster orders by
    // first move. Either way the sequence is total over the program and the partition gate proves it.
    private static Seq<SetupWindow> Resolved(Seq<SetupWindow> setups, FabricationResult.Motion motion, Point3d origin) =>
        setups.IsEmpty
            ? SetupWindow.Admit(0, 0, motion.Moves.Count, new Plane(origin, Vector3d.XAxis, Vector3d.YAxis))
                .Match(Succ: Seq1, Fail: static _ => Seq<SetupWindow>())
            : toSeq(setups.OrderBy(static row => row.FirstMove));
}
```

## [03]-[STOCK_FOLD]

- Owner: `Removal` folds every setup from its admitted frame origin and commits each setup as one `BoolSubtractAll` batch.
- Cases: the swept envelope derives from `CutterFamily`'s own `CornerRadius` seat and `TaperFrom` body law, so every admitted family generates its silhouette and a new row needs no arm here.
- Law: an oriented move carries its tool frame at BOTH ends, so the sweep interpolates the axis per station instead of holding the setup normal across a tilting cut. An axis-free move keeps the setup normal exactly, which is what makes its planar swept envelope exact; neither case approximates the other silently.
- Law: a non-cutting body strike is a pure MEMBERSHIP question — the shank and holder silhouettes are program invariants sampled once, and a strike is any silhouette or axis point inside remaining stock. No prism field is rasterized, no voxel is allocated, and no polygon containment is computed, so the per-station cost is a point query rather than a Boolean over two fields.
- Entry: setup and move arity collapse into immutable sequences consumed by `FoldM`, while resource custody stays inside the native boundary capsule.
- Auto: `HolderEnvelope` arrives as an admitted `Loop` and the shank silhouette is a three-vertex bulge circle, so both obstruction rings sample arc-exactly through `Loop.Apply` and no polygonal circle generator exists; every silhouette derivation is outward-bounding, so a narrowing family verifies against a superset of its own body.
- Exemption: `RemoveWindow` holds the native shadow lease and its cut list, `AddTool` and `Seal` push lattice beams across the provider ABI, and `Difference`/`Intersects` probe two voxel fields — the native statement kernels. Every one is a per-element `Lattice`/`Voxels` call the provider publishes no batched entry for, so no fold, span, or tensor operator can stand where they do; the branch's own sequence combinators own every walk that is not such a call.
- Receipt: `RemovalFinding` retains gouge, strike, uncut, overcut, air-cut, signed-deviation, and unresolved-coverage evidence, and each case carries its own invalidating verdict through one total dispatch.
- Growth: a cutter geometry is one `CutterFamily` row on the existing rule columns; a new non-cutting body is one `Obstruction` row.
- Boundary: a body that crashes never reads as material removed, because obstruction membership never subtracts.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemovalFinding {
    private RemovalFinding() { }

    public sealed record Gouge(int Setup, int Move, Point3d Point, CutterForm Cutter, double DepthMm) : RemovalFinding;
    public sealed record Strike(int Setup, int Move, Point3d Point, CollisionContact Contact, double ReachMm) : RemovalFinding;
    public sealed record Uncut(double VolumeMm3) : RemovalFinding;
    public sealed record Overcut(double VolumeMm3) : RemovalFinding;
    public sealed record AirCut(double Ratio) : RemovalFinding;
    public sealed record Deviation(DeviationField Field) : RemovalFinding;
    public sealed record Unresolved(int Setup, int Count, double Ratio) : RemovalFinding;

    // Volume and air-cut findings are quality evidence the verification atom projects and its `Clean` property
    // adjudicates; only a physical strike, a gouge past band, or evidence too sparse to support any claim
    // invalidates the run itself.
    public Option<Error> Fault(RemovalBands bands, CollisionZone zone) => Switch(
        state: (Bands: bands, Zone: zone),
        gouge: static (state, row) => row.DepthMm > state.Bands.Gouge.Value
            ? Some<Error>(new FabricationFault.Gouge(row.Point, row.Cutter))
            : None,
        strike: static (state, row) => Some<Error>(new FabricationFault.Collision(state.Zone, row.Contact)),
        uncut: static (_, _) => Option<Error>.None,
        overcut: static (_, _) => Option<Error>.None,
        airCut: static (_, _) => Option<Error>.None,
        deviation: static (state, row) => row.Field.Samples
            .Find(sample => sample.SignedMm < -state.Bands.Surface.Value)
            .Map<Error>(sample => new FabricationFault.Gouge(sample.Nominal, row.Field.Cutter)),
        unresolved: static (state, row) => row.Ratio > state.Bands.Coverage.Value
            ? Some<Error>(new KernelFault.InvalidValue("removal", "removal:surface-coverage"))
            : None);

    public Option<GougeWitness> Witness => Switch(
        gouge: static row => Some(new GougeWitness(row.Setup, row.Move, row.Point, row.DepthMm)),
        strike: static _ => Option<GougeWitness>.None,
        uncut: static _ => Option<GougeWitness>.None,
        overcut: static _ => Option<GougeWitness>.None,
        airCut: static _ => Option<GougeWitness>.None,
        deviation: static _ => Option<GougeWitness>.None,
        unresolved: static _ => Option<GougeWitness>.None);
}

public readonly record struct DeviationSample(Point3d Nominal, Vector3d Normal, double SignedMm);

// A deviation extent exists only where a sample does. The pair is absent TOGETHER, so one absence arm carries
// both bounds and no reader can meet a floor without its ceiling.
public readonly record struct DeviationSpan(double MinimumMm, double MaximumMm);

public sealed record DeviationField(
    int Setup,
    ContentKey Field,
    ContentKey Key,
    CutterForm Cutter,
    Seq<DeviationSample> Samples,
    int Unresolved,
    Option<DeviationSpan> Span);

public readonly record struct RemovalMetrics(double UncutVolume, double OvercutVolume, double AirCutRatio);

file readonly record struct CutterSection(double OffsetMm, double RadiusMm, bool Round);

file readonly record struct Obstruction(
    CollisionContact Contact,
    Seq<(double X, double Y)> Silhouette,
    double StartMm,
    double LengthMm,
    double ReachMm);

file sealed record RemovalState(
    Point3d Cursor,
    Seq<StockSnapshot> Snapshots,
    Seq<RemovalFinding> Findings,
    Option<ContentKey> Field,
    int AirMoves,
    int FeedMoves);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Removal {
    // Two independent draws per sampled face share the coordinate key and differ only by stream, so the barycentric
    // pair is reproducible and uncorrelated without a second salt vocabulary.
    private const int SweepStream = 1;

    // A verification leases the voxel runtime and renders no layer stack, so the grayscale row is the lease-only
    // encoding this page never reads back; the stack itself is never materialized.
    private static readonly CliMode Lease =
        new CliMode.Grayscale(ESliceMode.SignedDistance, MaskSampling.Interpolated, ESliceAxis.Z);

    // The one operation key both preimage closes and every native lift raise under.
    internal static readonly Op RemovalOp = Op.Of(name: "fabrication:removal");

    public static Fin<FabricationResult.VerificationResult> Verify(
        FabricationPolicy.Verify request,
        FabricationInput input,
        FabricationTap? tap = null) =>
        Solve(request, input).Map(result => Fired(result, tap ?? FabricationTap.Silent));

    // The settled receipt IS the fact: the projection reads the result's own public columns, so no measurement is
    // taken twice and a headless run fires into the silent tap with no branch at the call site.
    private static FabricationResult.VerificationResult Fired(
        FabricationResult.VerificationResult result, FabricationTap tap) {
        _ = tap.Fire(FabricationFact.Removal.Of(result));
        return result;
    }

    private static Fin<FabricationResult.VerificationResult> Solve(
        FabricationPolicy.Verify request, FabricationInput input) {
        VerifyPolicy policy = request.Policy;
        // One admitted grid serves the whole verification: residual-loop admission, every canonical preimage, and
        // the section walk read the CALLER's `Context` off the band block, so no per-window re-admission and no
        // locally minted millimetre default forks the tolerance the project actually declared.
        Context context = policy.Model;
        return from requiredCells in RequiredCells(policy)
               from _ in Admit(policy, request, input, requiredCells)
               from budget in VoxelBudget.Admit(policy.Bounds, policy.VoxelSize.Millimeters, policy.VoxelCap, requiredCells)
               from runtime in ImplicitPolicy.Validate(
                       budget,
                       policy.VoxelSize,
                       Lease,
                       policy.Calibration,
                       policy.Stock.FromVoxels,
                       out ImplicitPolicy composed)
                   .Admitted(composed)
               from obstructions in Obstructions(policy, context)
               from result in Capture(() => Implicit.Voxelize(
                   Seq<ImplicitOp>(
                       new ImplicitOp.Source(policy.Stock, Seq<VoxelMorphologyStep>(), runtime),
                       new ImplicitOp.Source(policy.Target, Seq<VoxelMorphologyStep>(), runtime)),
                   scopes => Execute(policy, request.Snapshots, scopes[0].Native, scopes[1].Native, obstructions, context)))
               select result;
    }

    private static Fin<Unit> Admit(
        VerifyPolicy policy, FabricationPolicy.Verify request, FabricationInput input, long requiredCells) =>
        (AdmissionSlots.Gate(policy.Motion.Moves.Count > 0, FabConcern.Verify, "removal:motion", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(policy.Motion.Moves.Exists(static move => move is not Move.Rapid),
             FabConcern.Verify, "removal:cutting-motion", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(Descends(policy, request, input), FabConcern.Verify, "removal:stock-lineage", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(Partitioned(policy), FabConcern.Verify, "removal:setup-partition", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(policy.ToolFrames.ForAll(row => row.Key < policy.Motion.Moves.Count),
             FabConcern.Verify, "removal:tool-frame", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(DepthWithin(policy), FabConcern.Verify, "removal:tool-depth", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(Generable(policy.Cutter), FabConcern.Verify, "removal:cutter-point-angle", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(requiredCells <= policy.VoxelCap, FabConcern.Verify, "removal:voxel-cap", FabricationFault.Inadmissible))
        .Apply(static (_, _, _, _, _, _, _, _) => unit)
        .As()
        .ToFin();


    // The verified stock descends from the run's declared ancestry: it is either a source the run names, a parent
    // run's artifact, or the residual the policy is re-verifying. Verifying a field the run never consumed produces
    // a snapshot whose lineage no traveler can close.
    private static bool Descends(VerifyPolicy policy, FabricationPolicy.Verify request, FabricationInput input) =>
        request.Residual.Map(residual => residual.Key == policy.Stock.Key).IfNone(false)
        || input.Sources.Exists(key => key == policy.Stock.Key)
        || input.ParentRuns.Exists(key => key == policy.Stock.Key);

    private static bool Partitioned(VerifyPolicy policy) {
        Seq<SetupWindow> windows = policy.Windows;
        return windows.Head.Map(static row => row.FirstMove == 0).IfNone(false)
            && windows.Last.Map(row => row.FirstMove + row.Count == policy.Motion.Moves.Count).IfNone(false)
            && windows.Map(static row => row.Setup).Distinct().Count == windows.Count
            && !toSeq(Enumerable.Range(1, Math.Max(0, windows.Count - 1)))
                .Exists(index => windows[index - 1].FirstMove + windows[index - 1].Count != windows[index].FirstMove);
    }

    private static bool DepthWithin(VerifyPolicy policy) {
        Seq<double> admitted = Seq(
                policy.Cutter.MaxDepthMm,
                policy.Cutter.UsableLengthMm,
                policy.Cutter.FunctionalLengthMm,
                Some(policy.Cutter.FluteLength))
            .Bind(static value => value.ToSeq());
        return admitted.Head.Map(seed => admitted.Tail.Fold(seed, double.Min)).Match(
            Some: floor => policy.Windows.ForAll(window =>
                Moves(policy, window).ForAll(row =>
                    row.Move is Move.Rapid
                    || Math.Abs((row.Move.Target - window.Frame.Origin) * window.Frame.ZAxis) <= floor)),
            None: static () => false);
    }

    // A half-point-angle family generates its body from `radius / tan(halfAngle)`, so a point angle at or past a
    // straight line describes no point and no silhouette. Admission proves it once; the generator is then total.
    private static bool Generable(CutterForm cutter) =>
        cutter.Family.TaperFrom != TaperSource.HalfPointAngle
        || cutter.PointAngleDeg.IfNone(cutter.TaperAngle * 2.0) < 180.0;

    private static Fin<long> RequiredCells(VerifyPolicy policy) {
        double edge = policy.VoxelSize.Millimeters;
        Seq<double> axes = Seq(
            Math.Ceiling((policy.Bounds.Max.X - policy.Bounds.Min.X) / edge),
            Math.Ceiling((policy.Bounds.Max.Y - policy.Bounds.Min.Y) / edge),
            Math.Ceiling((policy.Bounds.Max.Z - policy.Bounds.Min.Z) / edge));
        if (!axes.ForAll(static count => double.IsFinite(count) && count >= 1.0 && count <= long.MaxValue))
            return Fin.Fail<long>(new KernelFault.InvalidValue("removal", "removal:voxel-grid"));
        BigInteger required = axes.Map(static count => new BigInteger(count))
            .Fold(BigInteger.One, static (product, count) => product * count);
        return required <= long.MaxValue
            ? Fin.Succ((long)required)
            : Fin.Fail<long>(new KernelFault.InvalidValue("removal", "removal:voxel-grid"));
    }

    // --- [OBSTRUCTIONS]
    // Holder envelope and shank silhouette are program invariants sampled ONCE, so the per-station strike test is a
    // membership query over already-resolved rings.
    private static Fin<Seq<Obstruction>> Obstructions(VerifyPolicy policy, Context context) =>
        policy.Holder.Traverse(assembly =>
            from envelope in ToolMagazine.HolderEnvelope(assembly)
            from holder in Ring(envelope, policy.StepMm)
            from shank in ShankRing(policy, assembly, context)
            select shank.ToSeq() + Seq(new Obstruction(
                CollisionContact.Holder,
                holder,
                assembly.Stickout,
                assembly.GaugeLength,
                Reach(holder)))).As()
        .Map(static rows => rows.IfNone(Seq<Obstruction>()));

    private static Fin<Option<Obstruction>> ShankRing(VerifyPolicy policy, ToolAssembly assembly, Context context) =>
        (policy.Cutter.BodyDiameterMm | policy.Cutter.ShankDiameterMm)
            .Map(static diameter => diameter * 0.5)
            .Filter(_ => assembly.Stickout > policy.Cutter.FluteLength)
            .Traverse(radius =>
                from circle in Circle(radius, context)
                from ring in Ring(circle, policy.StepMm)
                select new Obstruction(
                    CollisionContact.Shank,
                    ring,
                    policy.Cutter.FluteLength,
                    assembly.Stickout - policy.Cutter.FluteLength,
                    radius))
            .As();

    // A circle is three vertices carrying the bulge of their own 120-degree span — `tan(sweep / 4)` is the atom's
    // own bulge law — so the shank ring is arc-exact and the polygonal circle generator has no reason to exist.
    private static Fin<Loop> Circle(double radiusMm, Context context) {
        double bulge = Math.Tan(Math.Tau / 12.0);
        Arr<Point3d> vertices = toSeq(Enumerable.Range(0, 3))
            .Map(index => Math.Tau * index / 3.0)
            .Map(angle => new Point3d(radiusMm * Math.Cos(angle), radiusMm * Math.Sin(angle), 0.0))
            .ToArr();
        return Loop.Admit(vertices, closed: true, bulges: Arr(bulge, bulge, bulge), tolerance: context);
    }

    // Arc-length stationing over the loop's own parameterization, so a bulged span samples on its arc rather than
    // on the chord its vertices span.
    private static Fin<Seq<(double X, double Y)>> Ring(Loop envelope, double resolutionMm) =>
        from measured in envelope.Apply(new ProfileOp.Measure())
        from path in measured is ProfileResult.Measure row
            ? Fin.Succ(row.Path.Millimeters)
            : Fin.Fail<double>(new KernelFault.InvalidValue("removal", "removal:holder-measure"))
        let count = Math.Max(envelope.Count, (int)Math.Ceiling(path / resolutionMm))
        from ring in toSeq(Enumerable.Range(0, count)).TraverseM(index =>
            envelope.Apply(new ProfileOp.Sample(Length.FromMillimeters(path * index / count))).Bind(sample =>
                sample is ProfileResult.Sampled point
                    ? Fin.Succ((point.Point.X, point.Point.Y))
                    : Fin.Fail<(double X, double Y)>(
                        new KernelFault.InvalidValue("removal", "removal:holder-sample")))).As()
        select ring;

    private static double Reach(Seq<(double X, double Y)> ring) =>
        ring.Map(static point => Math.Sqrt((point.X * point.X) + (point.Y * point.Y))).Fold(0.0, double.Max);

    // --- [SETUP_FOLD]
    private static Fin<FabricationResult.VerificationResult> Execute(
        VerifyPolicy policy,
        Seq<StockSnapshot> prior,
        Voxels actual,
        Voxels target,
        Seq<Obstruction> obstructions,
        Context tolerance) =>
        policy.Windows.FoldM<Fin, RemovalState>(
                new RemovalState(policy.Origin, prior, Seq<RemovalFinding>(), Field: None, AirMoves: 0, FeedMoves: 0),
                (state, window) => RemoveWindow(policy, actual, target, obstructions, state, window, tolerance))
            .As()
            .Bind(run => Project(policy, actual, target, run, tolerance));

    private static Fin<RemovalState> RemoveWindow(
        VerifyPolicy policy,
        Voxels actual,
        Voxels target,
        Seq<Obstruction> obstructions,
        RemovalState state,
        SetupWindow window,
        Context tolerance) {
        using Voxels shadow = actual.voxDuplicate();
        List<Voxels> cuts = [];
        try {
            return Moves(policy, window).FoldM<Fin, RemovalState>(state with { Cursor = window.Frame.Origin },
                    (current, row) => Advance(policy, shadow, obstructions, cuts, window, current, row.Move, row.Index))
                .As()
                .Bind(removed => CommitWindow(policy, actual, target, cuts, window, removed, tolerance));
        }
        finally { cuts.ForEach(static cut => cut.Dispose()); }
    }

    private static Seq<(Move Move, int Index)> Moves(VerifyPolicy policy, SetupWindow window) =>
        toSeq(policy.Motion.Moves.Skip(window.FirstMove).Take(window.Count))
            .Map((move, offset) => (move, window.FirstMove + offset));

    private static Fin<RemovalState> Advance(
        VerifyPolicy policy,
        Voxels shadow,
        Seq<Obstruction> obstructions,
        List<Voxels> cuts,
        SetupWindow window,
        RemovalState state,
        Move move,
        int index) {
        Plane frame = policy.ToolFrames.Find(index).IfNone(window.Frame);
        Seq<Point3d> stations = policy.Sampling.Project(state.Cursor, move, policy.StepMm);
        Seq<RemovalFinding> strikes = Strikes(
            policy, shadow, obstructions, state.Cursor, stations, move, frame, window.Setup, index);
        if (move is Move.Rapid)
            return Fin.Succ(state with { Cursor = move.Target, Findings = state.Findings + strikes });
        return Fin.Succ(CommitMove(
            shadow, cuts, SweepTool(policy, state.Cursor, stations, move, frame), state, move, strikes));
    }

    // The commit ASKS the field whether the sweep removed anything, because the answer is a property of the two
    // volumes it already holds. A caller-computed verdict handed in beside the sweep is one fact answered twice and
    // lets a site pass a probe of a field the commit never touches.
    private static RemovalState CommitMove(
        Voxels shadow,
        List<Voxels> cuts,
        Voxels swept,
        RemovalState state,
        Move move,
        Seq<RemovalFinding> strikes) {
        bool removes = Intersects(shadow, swept);
        if (removes) {
            cuts.Add(swept);
            shadow.BoolSubtract(swept);
        } else swept.Dispose();
        return state with {
            Cursor = move.Target,
            Findings = state.Findings + strikes,
            FeedMoves = state.FeedMoves + 1,
            AirMoves = state.AirMoves + (removes ? 0 : 1),
        };
    }

    private static Fin<RemovalState> CommitWindow(
        VerifyPolicy policy,
        Voxels actual,
        Voxels target,
        List<Voxels> cuts,
        SetupWindow window,
        RemovalState state,
        Context tolerance) {
        actual.BoolSubtractAll(cuts);
        RemovalMetrics metrics = Metrics(actual, target, state);
        return from fieldKey in policy.Stock.FromVoxels(actual)
               from loops in ResidualLoops(actual, window.Frame, tolerance)
               from field in Surface(policy, actual, target, window, fieldKey, loops, metrics, tolerance)
               from snapshot in StockSnapshot.Admit(window.Setup, field.Key, loops)
               select state with {
                   Snapshots = state.Snapshots.Add(snapshot),
                   Findings = state.Findings + DeviationFindings(policy, window, field),
                   Field = Some(fieldKey),
               };
    }

    // --- [SWEPT_ENVELOPE]
    // The tool axis interpolates ACROSS the move where the atom carries a continuous frame; an axis-free move holds
    // the setup normal, which is exactly the case whose planar envelope the atom proves exact.
    private static Voxels SweepTool(VerifyPolicy policy, Point3d from, Seq<Point3d> stations, Move move, Plane frame) {
        using Lattice lattice = new();
        Seq<CutterSection> sections = Sections(policy.Cutter, policy.SilhouetteMm);
        // The leading face seals ONCE, before the walk, because only the first station has an unswept body behind
        // it. Sealing it here deletes the per-station flag the fold used to carry and the branch every later
        // station paid to read it.
        Seal(lattice, from, Axis(move, frame, 0.0), sections);
        _ = Parameterized(stations).Fold((Point: from, At: 0.0), (held, row) => {
            AddTool(lattice, held.Point, row.Point, Axis(move, frame, held.At), Axis(move, frame, row.At), sections);
            return (row.Point, row.At);
        });
        return new Voxels(lattice);
    }

    // Stations carry their own move parameter, so the tool axis, the strike walk, and the beam pair all read one
    // ordinate rather than three conventions for where along the move a sample sits.
    private static Seq<(Point3d Point, double At)> Parameterized(Seq<Point3d> stations) =>
        stations.Map((point, index) => (point, (double)(index + 1) / stations.Count));

    private static Vector3d Axis(Move move, Plane frame, double t) => move.Orientation.Match(
        Some: oriented => Interpolated(oriented.AxisAtStart, oriented.AxisAtEnd, t),
        None: () => frame.ZAxis);

    private static Vector3d Interpolated(Vector3d start, Vector3d finish, double t) {
        Vector3d blended = (start * (1.0 - t)) + (finish * t);
        return blended.Unitize() ? blended : start;
    }

    // One span of the swept body: an axial beam per section between the two stations, plus the trailing face the
    // consecutive sections bridge at the arriving station.
    private static void AddTool(
        Lattice lattice,
        Point3d from,
        Point3d to,
        Vector3d axisFrom,
        Vector3d axisTo,
        Seq<CutterSection> sections) {
        sections.Iter(section => lattice.AddBeam(
            ToVector(from + (axisFrom * section.OffsetMm)), (float)section.RadiusMm,
            ToVector(to + (axisTo * section.OffsetMm)), (float)section.RadiusMm,
            bRoundCap: section.Round));
        Seal(lattice, to, axisTo, sections);
    }

    // The section-to-section bridge at ONE station: the cutter's own body closed across its profile. The leading
    // and trailing faces are the same construction at two stations, so one member serves both and neither reads a
    // flag to decide which it is.
    private static void Seal(Lattice lattice, Point3d at, Vector3d axis, Seq<CutterSection> sections) =>
        _ = sections.Fold(Option<CutterSection>.None, (previous, section) => {
            _ = previous.Iter(prior => lattice.AddBeam(
                ToVector(at + (axis * prior.OffsetMm)), (float)prior.RadiusMm,
                ToVector(at + (axis * section.OffsetMm)), (float)section.RadiusMm,
                bRoundCap: prior.Round || section.Round));
            return Some(section);
        });

    // Swept-envelope geometry derives from the family's own admission columns, never from a per-family arm:
    // `CornerRadius` seats the nose arc (zero flat, half-diameter ball, between toroidal) and `TaperFrom` selects
    // the body law, so a seventeenth `CutterFamily` row generates its silhouette with no edit here. Every
    // derivation is outward-bounding, so a narrowing family verifies against a superset of its own body.
    private static Seq<CutterSection> Sections(CutterForm cutter, double resolutionMm) {
        double radius = cutter.Diameter * 0.5;
        double nose = Math.Min(cutter.CornerRadius, radius);
        return cutter.Family.TaperFrom.Switch(
            state: (Radius: radius, Nose: nose, Length: cutter.FluteLength, Resolution: resolutionMm, Form: cutter),
            flat: static state => Extend(
                Nose(state.Radius, state.Nose, state.Length, state.Resolution), state.Length, state.Radius),
            edgeAngle: static state => Nose(state.Radius, state.Nose, state.Length, state.Resolution)
                .Add(new CutterSection(
                    state.Length,
                    state.Radius + ((state.Length - state.Nose) * Tilt(state.Form.TaperAngle)),
                    false)),
            halfPointAngle: static state => Extend(
                Seq(
                    new CutterSection(0.0, state.Resolution, false),
                    new CutterSection(
                        Math.Min(state.Length, state.Radius / Tilt(HalfPoint(state.Form))),
                        state.Radius,
                        false)),
                state.Length,
                state.Radius));
    }

    // The taper angle is admitted in `[0, 90)` at `CutterForm` and every taper-source family carries `TaperRule`
    // `Tapered` or `Any`, so the tangent is finite and positive without a clamp.
    private static double Tilt(double degrees) => Math.Tan(degrees * Math.PI / 180.0);

    private static double HalfPoint(CutterForm cutter) => cutter.PointAngleDeg.IfNone(cutter.TaperAngle * 2.0) * 0.5;

    private static Seq<CutterSection> Nose(double radiusMm, double noseMm, double lengthMm, double resolutionMm) =>
        noseMm <= resolutionMm
            ? Seq(new CutterSection(0.0, radiusMm, false))
            : Profile(
                Math.Min(noseMm, lengthMm),
                resolutionMm,
                offset => radiusMm - noseMm + Math.Sqrt(Math.Max(0.0, (noseMm * noseMm) - Math.Pow(noseMm - offset, 2.0))));

    // A beam thinner than the finest resolvable section rasterizes to nothing, so the silhouette floor is the
    // outward-bounding radius the field can actually hold.
    private static Seq<CutterSection> Profile(double extentMm, double resolutionMm, Func<double, double> radius) {
        int count = Math.Max(1, (int)Math.Ceiling(extentMm / resolutionMm));
        return toSeq(Enumerable.Range(0, count + 1))
            .Map(index => extentMm * index / count)
            .Map(offset => new CutterSection(offset, Math.Max(radius(offset), resolutionMm), true));
    }

    private static Seq<CutterSection> Extend(Seq<CutterSection> profile, double lengthMm, double radiusMm) =>
        profile.Last.Filter(last => last.OffsetMm < lengthMm)
            .Map(_ => profile.Add(new CutterSection(lengthMm, radiusMm, false)))
            .IfNone(profile);

    // --- [OBSTRUCTION_MEMBERSHIP]
    // A strike is remaining stock inside a non-cutting body. The body's silhouette and its axis line are already
    // resolved, so the test walks those points across the body's slab and asks the field for membership — no
    // prism is rasterized, no field is duplicated, and no containment predicate is evaluated.
    private static Seq<RemovalFinding> Strikes(
        VerifyPolicy policy,
        Voxels actual,
        Seq<Obstruction> obstructions,
        Point3d from,
        Seq<Point3d> stations,
        Move move,
        Plane frame,
        int setup,
        int index) => obstructions.IsEmpty
        ? Seq<RemovalFinding>()
        : obstructions.Bind(row => (Seq((from, 0.0)) + Parameterized(stations))
            .Find(station => Touches(actual, row, Axis(move, frame, station.At), frame, station.Point, policy.StepMm))
            .Map(station => (RemovalFinding)new RemovalFinding.Strike(
                setup, index, station.Point, row.Contact, row.ReachMm))
            .ToSeq());

    private static bool Touches(
        Voxels actual, Obstruction row, Vector3d axis, Plane frame, Point3d station, double stepMm) {
        int slabs = Math.Max(1, (int)Math.Ceiling(row.LengthMm / stepMm));
        return toSeq(Enumerable.Range(0, slabs + 1))
            .Map(step => station + (axis * (row.StartMm + (row.LengthMm * step / slabs))))
            .Exists(seat => actual.bIsInside(ToVector(seat))
                || row.Silhouette.Exists(point =>
                    actual.bIsInside(ToVector(seat + (frame.XAxis * point.X) + (frame.YAxis * point.Y)))));
    }

    private static double Difference(Voxels left, Voxels right) {
        if (left.bIsEqual(in right)) return 0.0;
        using Voxels delta = left.voxDuplicate();
        delta.BoolSubtract(right);
        delta.CalculateProperties(out float volume, out BBox3 _);
        return volume;
    }

    private static bool Intersects(Voxels left, Voxels right) {
        using Voxels overlap = right.voxDuplicate();
        overlap.BoolIntersect(left);
        return !overlap.bIsEmpty();
    }

    private static Vector3 ToVector(Point3d point) => new((float)point.X, (float)point.Y, (float)point.Z);
    private static Point3d ToPoint(Vector3 point) => new(point.X, point.Y, point.Z);
    private static Vector3d ToDirection(Vector3 value) => new(value.X, value.Y, value.Z);
}
```

## [04]-[SURFACE_TRUTH]

- Owner: `DeviationField` selects positive-area target triangles against a cumulative-area prefix so coverage is uniform over surface rather than tessellation, then projects each sampled nominal point onto the ACTUAL field through `Voxels.bClosestPointOnSurface` and orients it by the gradient `vecSurfaceNormal` at the hit.
- Law: the deviation ORACLE is `actual.bIsInside(nominal)` — a point sampled on the NOMINAL surface, tested for membership in the as-machined field. Inside the actual means stock still stands proud of nominal and the deviation is POSITIVE uncut; outside means material was taken past nominal and the deviation is NEGATIVE gouge. Testing a nominal point against the field that generated it asks whether a surface point lies inside its own zero level set, which decides nothing and returns noise.
- Law: absence is `Option`, never a bound. A field with no resolved sample carries no `DeviationSpan`, so a reader meets the empty case rather than a floor and ceiling of zero that reads as a perfect surface; the air-cut ratio needs no such arm because admission already refused a motion with no cutting move.
- Law: identity reads the S0 owners whole. `Loop.CanonicalBytes` is THE loop preimage — rotation-canonical and tolerance-quantized — and `FabricationCanon` is THE framing family and both of its closes, so this page declares no rotation rule, no coordinate writer, no presence tag, no hex render inside a preimage, and no writer mint of its own. Residual loops order by their own canonical origin and area under a stable sort, so a re-rooted section cannot fork one snapshot key.
- Auto: barycentric draws come from `Deterministic.UnitInterval` on the face centroid over two streams, so the field reproduces bit-identically and the pair stays uncorrelated; Boolean volume deltas remain the independent conservation check and neither scalar path substitutes for the other.
- Exemption: `Surface` folds the cumulative-area prefix and `ResidualLoops` welds extracted native vertices — the two native statement kernels here. Both walk `PicoGK.Mesh.GetTriangle`, an out-parameter call per triangle with no batched or spanned form, so the prefix scan and the vertex weld are statement bodies for the ABI's sake and not for the arithmetic's; the sample draw over that prefix is an ordinary sequence fold.
- Receipt: every setup snapshot key closes through `FabricationCanon.Keyed` — the ONE retaining mint over the `Rasm.Element` codec, answering on the `Fin` rail — framing stock lineage, motion, setup and tool frames, tool assembly identity, cutter policy, the acceptance bands, machined loops, metrics, and signed field samples, so a `-0.0`, a NaN payload, or a string boundary cannot fork one snapshot into two. The circular arm writes `SweepRadians`, so a full-turn arc and its zero-sweep twin keep distinct keys.
- Boundary: `ResidualLoops` reuses one Rhino vertex index per extracted native vertex before plane intersection; provider geometry terminates here.

```csharp signature
public static partial class Removal {
    private static Fin<DeviationField> Surface(
        VerifyPolicy policy,
        Voxels actual,
        Voxels target,
        SetupWindow window,
        ContentKey fieldKey,
        Arr<Loop> loops,
        RemovalMetrics metrics,
        Context tolerance) {
        using PicoGK.Mesh mesh = target.mshAsMesh();
        double floor = tolerance.Absolute.Value * tolerance.Absolute.Value;
        // Index-uniform triangle selection samples a finely tessellated region far denser than a coarse one, so the
        // deviation field would under-cover exactly the large flat faces a gouge escapes on; cumulative-area
        // prefixing makes selection area-uniform over the target surface instead.
        Seq<(int Triangle, double Area)> surface = toSeq(Enumerable.Range(0, mesh.nTriangleCount())).Choose(index => {
            mesh.GetTriangle(index, out Vector3 a, out Vector3 b, out Vector3 c);
            double area = 0.5 * Vector3.Cross(b - a, c - a).Length();
            return double.IsFinite(area) && area > floor ? Some((index, area)) : None;
        });
        if (surface.IsEmpty)
            return Fin.Fail<DeviationField>(
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "removal:target-surface"));

        int budget = policy.SurfaceSamples.Value;
        double[] cumulative = new double[surface.Count];
        _ = toSeq(Enumerable.Range(0, surface.Count)).Fold(0.0, (running, index) => {
            cumulative[index] = running + surface[index].Area;
            return cumulative[index];
        });
        double total = cumulative[surface.Count - 1];
        Seq<Option<DeviationSample>> rows = toSeq(Enumerable.Range(0, budget)).Map(index => {
            int face = Math.Clamp(
                Array.BinarySearch(cumulative, total * (index + 0.5) / budget) is var found && found >= 0
                    ? found
                    : ~found,
                0,
                surface.Count - 1);
            mesh.GetTriangle(surface[face].Triangle, out Vector3 a, out Vector3 b, out Vector3 c);
            Point3d centroid = ToPoint((a + b + c) / 3.0f);
            double root = Math.Sqrt(Deterministic.UnitInterval(centroid, salt: index));
            double sweep = Deterministic.UnitInterval(centroid, salt: index, seed: SweepStream);
            return Projected(
                actual,
                ((float)(1.0 - root) * a) + ((float)(root * (1.0 - sweep)) * b) + ((float)(root * sweep) * c));
        });
        Seq<DeviationSample> samples = rows.Bind(static row => row.ToSeq());
        int unresolved = rows.Count - samples.Count;
        return SnapshotKey(policy, window, fieldKey, loops, metrics, samples, unresolved, tolerance).Map(key =>
            new DeviationField(window.Setup, fieldKey, key, policy.Cutter, samples, unresolved, Span(samples)));
    }

    // The nominal point is sampled on the TARGET surface and its membership is asked of the ACTUAL field: inside
    // means stock still stands proud (positive, uncut), outside means material went past nominal (negative, gouge).
    // Nearest-surface projection is total over a non-empty field, so the unresolved census counts the empty one
    // alone, and the gradient normal frees the sampler from the tessellation density this owner fights.
    private static Option<DeviationSample> Projected(Voxels actual, Vector3 nominal) =>
        actual.bClosestPointOnSurface(nominal, out Vector3 hit)
            ? Some(new DeviationSample(
                ToPoint(nominal),
                ToDirection(actual.vecSurfaceNormal(hit)),
                (actual.bIsInside(nominal) ? 1.0 : -1.0) * Vector3.Distance(nominal, hit)))
            : None;

    // Floor and ceiling are absent together, so one arm carries both and an empty field reports no extent rather
    // than a fabricated pair of zeros that reads as a perfect surface.
    private static Option<DeviationSpan> Span(Seq<DeviationSample> samples) =>
        samples.Head.Map(seed => samples.Tail.Fold(
            new DeviationSpan(seed.SignedMm, seed.SignedMm),
            static (span, row) => new DeviationSpan(
                Math.Min(span.MinimumMm, row.SignedMm),
                Math.Max(span.MaximumMm, row.SignedMm))));

    // The snapshot preimage closes at the S0 facade: `FabricationCanon.Keyed` opens the RETAINING writer, frames
    // through the family, and answers on the `Fin` rail, because the codec publishes no public constructor and a
    // lane opening one keyed its artifact off bytes it never held.
    private static Fin<ContentKey> SnapshotKey(
        VerifyPolicy policy,
        SetupWindow window,
        ContentKey fieldKey,
        Arr<Loop> loops,
        RemovalMetrics metrics,
        Seq<DeviationSample> samples,
        int unresolved,
        Context tolerance) =>
        FabricationCanon.Keyed(EgressKind.StockSnapshot, tolerance, writer =>
            FrameSnapshot(writer, policy, window, fieldKey, loops, metrics, samples, unresolved), RemovalOp);

    // The band block writes its OWN millimetre-basis magnitudes — `Tolerance.Value`, `Volume.CubicMillimeters`, and
    // `UnitInterval.Value` are the same scalars the bare-double columns carried — so the typed head left the
    // preimage byte-identical and no landed snapshot key moved.
    private static CanonicalWriter FrameSnapshot(
        CanonicalWriter writer,
        VerifyPolicy policy,
        SetupWindow window,
        ContentKey fieldKey,
        Arr<Loop> loops,
        RemovalMetrics metrics,
        Seq<DeviationSample> samples,
        int unresolved) {
        writer.U128(fieldKey.Digest).U128(policy.Stock.Key.Digest).U128(policy.Target.Key.Digest)
            .Coords(policy.Origin)
            .Ordinal(window.Setup).Ordinal(window.FirstMove).Ordinal(window.Count);
        Frame(writer, window.Frame).Coords(policy.Bounds.Min).Coords(policy.Bounds.Max)
            .Discriminant(policy.Cutter.Family)
            .Double(policy.Cutter.Diameter).Double(policy.Cutter.CornerRadius)
            .Double(policy.Cutter.TaperAngle).Double(policy.Cutter.FluteLength);
        Seq(policy.Cutter.UsableLengthMm, policy.Cutter.FunctionalLengthMm, policy.Cutter.OverallLengthMm,
                policy.Cutter.BodyDiameterMm, policy.Cutter.ShankDiameterMm, policy.Cutter.MaxDepthMm,
                policy.Cutter.LeadAngleDeg, policy.Cutter.PointAngleDeg, policy.Cutter.OrientationDeg)
            .Fold(writer, static (sink, value) => sink.Maybe(value, static (row, amount) => row.Double(amount)));
        writer
            .Maybe(policy.Cutter.Evidence.Map(static evidence => evidence.StructuralDigest),
                static (row, digest) => row.String(digest))
            .Maybe(policy.Holder.Map(static assembly => assembly.Identity),
                static (row, identity) => row.U128(identity))
            .Double(policy.VoxelSize.Millimeters).I64(policy.VoxelCap).Double(policy.Station.Millimeters)
            .Ordinal(policy.SurfaceSamples.Value).Discriminant(policy.Sampling)
            .Ordinal(policy.Calibration.MinimumSamples).Ordinal(policy.Calibration.MaximumSamples)
            .Double(policy.Calibration.QuantileError.DecimalFractions)
            .Double(policy.Calibration.DensityFloor.DecimalFractions)
            .Double(policy.Calibration.GradientFloorPerMillimeter)
            .Double(policy.Bands.Gouge.Value).Double(policy.Bands.Uncut.CubicMillimeters)
            .Double(policy.Bands.Overcut.CubicMillimeters)
            .Double(policy.Bands.AirCut.Value).Double(policy.Bands.Surface.Value)
            .Double(policy.Bands.Coverage.Value)
            .Double(metrics.UncutVolume).Double(metrics.OvercutVolume).Double(metrics.AirCutRatio)
            .Ordinal(unresolved);
        // The arc carries `SweepRadians` as identity: target, centre, and sense leave a full-turn arc and its
        // zero-sweep twin sharing one preimage, so the snapshot key would not separate two distinct programs.
        writer.Rows(policy.Motion.Moves, static (row, move) => move.Switch(
            state: row,
            rapid: static (held, value) => held.Ordinal(0).Coords(value.Target),
            linear: static (held, value) => held.Ordinal(1).Coords(value.Target).Double(value.Feed),
            circular: static (held, value) => held.Ordinal(2).Coords(value.Target).Double(value.Feed)
                .Coords(value.Arc.Center).Discriminant(value.Arc.Sense).Double(value.SweepRadians)));
        writer.Rows(
            toSeq(policy.ToolFrames.AsIterable().OrderBy(static row => row.Key)),
            static (row, seat) => Frame(row.Ordinal(seat.Key), seat.Value));
        writer.Rows(
            policy.Windows,
            static (row, setup) => Frame(
                row.Ordinal(setup.Setup).Ordinal(setup.FirstMove).Ordinal(setup.Count), setup.Frame));
        // Canonicalizing first makes the rank read a rotation-invariant origin, and `Loop.CanonicalOrder` is the S0
        // owner's own comparison over that normal form — the same one `Loop.CanonicalBytes` frames — so the sort key
        // and the preimage can never separate two loops the codec mints one key for.
        writer.Rows(
            toSeq(loops.Map(static loop => loop.Canonical()).OrderBy(static loop => loop, Loop.CanonicalOrder)),
            static (row, loop) => loop.CanonicalBytes(row));
        return writer.Rows(samples, static (row, sample) =>
            row.Coords(sample.Nominal).Coords(sample.Normal).Double(sample.SignedMm));
    }

    private static Fin<ContentKey> ResidualKey(ContentKey field, Context tolerance) =>
        FabricationCanon.Keyed(EgressKind.Remnant, tolerance, writer => writer.U128(field.Digest), RemovalOp);

    // A plane is four `Coords` writes over the S0 family, so this composition adds no framing convention of its own.
    private static CanonicalWriter Frame(CanonicalWriter writer, Plane value) =>
        writer.Coords(value.Origin).Coords(value.XAxis).Coords(value.YAxis).Coords(value.ZAxis);

    private static Fin<Arr<Loop>> ResidualLoops(Voxels actual, Plane frame, Context context) {
        using PicoGK.Mesh extracted = actual.mshAsMesh();
        using Rhino.Geometry.Mesh native = new();
        Dictionary<Vector3, int> vertices = [];
        int Vertex(Vector3 point) {
            if (vertices.TryGetValue(point, out int index)) return index;
            int added = native.Vertices.Add(point.X, point.Y, point.Z);
            vertices.Add(point, added);
            return added;
        }
        _ = toSeq(Enumerable.Range(0, extracted.nTriangleCount())).Iter(index => {
            extracted.GetTriangle(index, out Vector3 a, out Vector3 b, out Vector3 c);
            native.Faces.AddFace(Vertex(a), Vertex(b), Vertex(c));
        });
        return from space in MeshSpace.Of(native, context)
               from result in Intersection.Apply(new IntersectOp.PlaneMesh(frame, space, IntersectPolicy.Canonical))
               from loops in result is IntersectResult.Chains chains
                   ? chains.Walked.Filter(static chain => chain.Closed)
                       .TraverseM(chain => Loop
                           .Admit(toSeq(chain.Points).ToArr(), closed: true, bulges: Arr<double>(), tolerance: context)
                           .Map(static loop => loop.AsCcw()))
                       .As()
                   : Fin.Fail<Seq<Loop>>(
                       new KernelFault.InvalidValue("removal", "removal:residual-section"))
               select loops.ToArr();
    }
}
```

## [05]-[VERDICT]

- Owner: `Project` adjudicates the finding set against the admitted tolerance, mints the residual stock, and closes on `FabricationResult.VerificationResult`; `Fired` is the one emission seam.
- Law: a verified program that missed its band is a RECEIPT with `Clean` false, not a failed rail — the atom carries the volumes, the ratio, and the gouge witnesses precisely so the consumer reads the verdict. Only a physical strike, an out-of-band gouge, or surface evidence too sparse to support any claim invalidates the run, and the volume tolerance floors at the one voxel the field can resolve.
- Entry: `Removal.Verify` returns the concrete `FabricationResult.VerificationResult`, so the run spine's synchronous lift binds it directly and the fact projection reads its typed columns without a downcast.
- Auto: `FabricationFact.Removal.Of` flattens the settled receipt to gouge count, uncut and overcut volume, and air-cut ratio, reaching `rasm.fabrication.removal.verifications`, `.defects`, `.residual`, and `.aircut` through `Process/telemetry#FACT_PROJECTION` under kind `removal`; the tap defaults to `FabricationTap.Silent`, whose send is a total no-op, so a headless verification pays no branch.
- Receipt: `FabricationResult.VerificationResult` carries residual stock, per-setup snapshots, gouge witnesses, both residue volumes, the air-cut ratio, and the voxel-derived volume tolerance its own `Clean` verdict reads.
- Boundary: the page opens no solver span, because the removal fold counts no solver-internal step and `FabricationEngine` rosters no removal lane; a traced removal lane is one `FabricationEngine` row at `Process/telemetry` before a bracket exists here.

```csharp signature
public static partial class Removal {
    private static Fin<FabricationResult.VerificationResult> Project(
        VerifyPolicy policy,
        Voxels actual,
        Voxels target,
        RemovalState run,
        Context tolerance) {
        RemovalMetrics metrics = Metrics(actual, target, run);
        Seq<RemovalFinding> findings = run.Findings + Findings(policy, metrics);
        double quantum = Math.Max(
            policy.Bands.Overcut.CubicMillimeters, Math.Pow(policy.VoxelSize.Millimeters, 3.0));
        return from final in run.Snapshots.Last
                   .ToFin(new KernelFault.InvalidValue("removal", "removal:no-snapshot"))
               from field in run.Field
                   .ToFin(new KernelFault.InvalidValue("removal", "removal:no-field"))
               from residualKey in ResidualKey(field, tolerance)
               from residual in ResidualStock.Admit(residualKey, final.Machined)
               from zone in CollisionZone.Admit(policy.Stock.Key, policy.Bounds)
               from _ in Invalidating(findings, policy.Bands, zone).Match(
                   Some: Fin.Fail<Unit>,
                   None: static () => Fin.Succ(unit))
               select new FabricationResult.VerificationResult(
                   residual,
                   run.Snapshots,
                   findings.Choose(static finding => finding.Witness),
                   metrics.UncutVolume,
                   metrics.OvercutVolume,
                   metrics.AirCutRatio,
                   quantum);
    }

    // Admission proved at least one cutting move, so the feed census is positive and the air-cut ratio is a
    // measured fraction rather than a zero standing in for an absent measurement.
    private static RemovalMetrics Metrics(Voxels actual, Voxels target, RemovalState run) =>
        new(Difference(actual, target), Difference(target, actual), (double)run.AirMoves / run.FeedMoves);

    private static Seq<RemovalFinding> Findings(VerifyPolicy policy, RemovalMetrics metrics) =>
        Seq(
            metrics.UncutVolume > policy.Bands.Uncut.CubicMillimeters
                ? Some<RemovalFinding>(new RemovalFinding.Uncut(metrics.UncutVolume)) : None,
            metrics.OvercutVolume > policy.Bands.Overcut.CubicMillimeters
                ? Some<RemovalFinding>(new RemovalFinding.Overcut(metrics.OvercutVolume)) : None,
            metrics.AirCutRatio > policy.Bands.AirCut.Value
                ? Some<RemovalFinding>(new RemovalFinding.AirCut(metrics.AirCutRatio)) : None)
        .Bind(static row => row.ToSeq());

    private static Seq<RemovalFinding> DeviationFindings(VerifyPolicy policy, SetupWindow window, DeviationField field) =>
        Seq<RemovalFinding>(new RemovalFinding.Deviation(field))
        + (field.Unresolved > 0
            ? Seq<RemovalFinding>(new RemovalFinding.Unresolved(
                field.Setup,
                field.Unresolved,
                (double)field.Unresolved / (field.Samples.Count + field.Unresolved)))
            : Seq<RemovalFinding>())
        + field.Samples
            .Filter(sample => sample.SignedMm < -policy.Bands.Gouge.Value)
            .Map(sample => (RemovalFinding)new RemovalFinding.Gouge(
                field.Setup,
                ClosestMove(policy, window, sample.Nominal),
                sample.Nominal,
                policy.Cutter,
                -sample.SignedMm));

    private static Option<Error> Invalidating(
        Seq<RemovalFinding> findings, RemovalBands bands, CollisionZone zone) {
        Seq<Error> errors = findings.Choose(finding => finding.Fault(bands, zone));
        return errors.Head.Map(first => errors.Tail.Fold(first, static (combined, error) => combined + error));
    }

    // The window carries at least one move by admission, so the search seeds on its FIRST segment rather than on a
    // sentinel distance no measurement can produce.
    private static int ClosestMove(VerifyPolicy policy, SetupWindow window, Point3d point) {
        Seq<(Move Move, int Index)> moves = Moves(policy, window);
        return moves.Head.Match(
            Some: first => moves.Tail.Fold(
                (Cursor: first.Move.Target,
                 Index: first.Index,
                 Distance: SegmentDistance(window.Frame.Origin, first.Move.Target, point)),
                (state, row) => {
                    double distance = SegmentDistance(state.Cursor, row.Move.Target, point);
                    return (row.Move.Target,
                        distance < state.Distance ? row.Index : state.Index,
                        Math.Min(distance, state.Distance));
                }).Index,
            None: () => window.FirstMove);
    }

    private static double SegmentDistance(Point3d from, Point3d to, Point3d point) {
        Vector3d direction = to - from;
        if (direction.SquareLength == 0.0) return point.DistanceTo(from);
        double t = Math.Clamp(((point - from) * direction) / direction.SquareLength, 0.0, 1.0);
        return point.DistanceTo(from + (direction * t));
    }

    // PicoGK allocation and library-mismatch exits are thrown, so the whole native walk funnels through Op.Catch.
    private static Fin<T> Capture<T>(Func<Fin<T>> native) =>
        Op.Of(name: "removal:native").Catch(native);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
