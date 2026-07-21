# [RASM_FABRICATION_REMOVAL]

`Removal.Verify` owns post-program stock truth: one admitted `VerifyPolicy` materializes stock and target through the shared voxel runtime, folds setup-framed cutter sweeps and non-cutting obstruction prisms over actual stock, and projects residual stock, snapshots, signed surface deviation, and tolerance evidence onto `FabricationResult.VerificationResult`.

`FabricationPolicy.Verify`, `VoxelWire`, `ToolMagazine.HolderEnvelope`, `StockSnapshot`, and `ContentKey.Of` remain frozen seams. Native handles terminate inside one exception-capture and disposal capsule; only process atoms leave the Verify plane.

## [01]-[INDEX]

- [02]-[POLICY]: generated admission for removal resolution, setup framing, sampling, tolerance, and native budget.
- [03]-[STOCK_FOLD]: setup-ordered cutter sweeps and shank-plus-holder obstruction tests over one mutable stock lease.
- [04]-[SURFACE_TRUTH]: signed target-to-actual deviation, residual topology, payload-complete snapshot identity, and result projection.

## [02]-[POLICY]

- Owner: `VerifyPolicy` admits the complete removal request once; `SetupWindow` admits each setup partition, and `RemovalTolerance` carries every verdict and evidence-coverage threshold as data.
- Cases: `SweepSampling` rows carry only the bound-to-arc-length conversion their name states, so chord, arc, and sagitta stationing share one generator over the move family.
- Entry: `Removal.Verify` is the sole public operation and consumes an already admitted policy from `FabricationPolicy.Verify`.
- Auto: generated factories reject primitive defects, one `Validation<Error, Unit>` fan-in proves stock lineage, setup partition, tool-frame coverage, and voxel demand, and `Capture` encloses native source construction, voxelization, callback execution, and lease disposal.
- Growth: a sampling law is one `SweepSampling` row, and tolerance regimes arrive as `RemovalTolerance` values without another named preset or entrypoint.
- Boundary: `VoxelWire` remains the only stock ingress and egress codec; native `Library`, `Voxels`, `Lattice`, and `Mesh` leases never cross the operation.

## [03]-[STOCK_FOLD]

- Owner: `Removal` folds every setup from its admitted frame origin and commits each setup as one `BoolSubtractAll` batch.
- Cases: the swept envelope derives from `CutterFamily`'s own `CornerRadius` seat and `TaperFrom` body law, so every admitted family generates its silhouette and a new row needs no arm here.
- Entry: setup and move arity collapse into immutable sequences consumed by `FoldM`, while resource custody stays inside the native boundary capsule.
- Auto: arc admission proves one radius before station generation; the shank and holder rings sample once per program as `Obstruction` rows and test as non-cutting prisms, so a body that crashes never reads as material removed.
- Receipt: `RemovalFinding` retains gouge, strike, uncut, overcut, air-cut, signed-deviation, and unresolved-coverage evidence, and each case carries its own invalidating verdict through one total dispatch. `Removal.Verify` mints `FabricationFact.Removal.Of` over the settled `FabricationResult.VerificationResult`, projecting gouge counts, uncut/overcut volume, and the air-cut ratio onto `rasm.fabrication.removal.defects`, `rasm.fabrication.removal.residual`, and `rasm.fabrication.removal.aircut` through `Process/telemetry#FACT_PROJECTION` as kind `removal`.
- Growth: a cutter geometry is one `CutterFamily` row on the existing rule columns; a new non-cutting body is one `Obstruction` row.

## [04]-[SURFACE_TRUTH]

- Owner: `DeviationField` selects positive-area target triangles against a cumulative-area prefix so coverage is uniform over surface rather than tessellation, then samples each face along both normal directions through `Voxels.bRayCastToSurface`, retaining the nearest signed distance and unresolved rays.
- Auto: barycentric draws come from `Deterministic.UnitInterval` on the face centroid, so the field reproduces bit-identically; Boolean volume deltas remain the independent conservation check, and neither scalar path substitutes for the other.
- Receipt: every setup snapshot key length-frames stock lineage, motion, setup and tool frames, tool assembly identity, cutter and tolerance policy, machined loops, metrics, and signed field samples.
- Boundary: `ResidualLoops` reuses one Rhino vertex index for each extracted native vertex before plane intersection; `FabricationResult.VerificationResult` carries the verdict, so a program that missed its volume band returns the receipt with `Clean` false.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using PicoGK;
using Rasm.Domain;
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Tooling;
using Rasm.Meshing;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SweepSampling {
    public static readonly SweepSampling Chord = new("chord", static (radius, bound) =>
        2.0 * radius * Math.Asin(Math.Clamp(bound / (2.0 * radius), 0.0, 1.0)));
    public static readonly SweepSampling Arc = new("arc", static (_, bound) => bound);
    public static readonly SweepSampling Adaptive = new("adaptive", static (radius, bound) => Math.Max(
        2.0 * radius * Math.Sin(Math.Acos(Math.Clamp(1.0 - (bound / radius), -1.0, 1.0))),
        bound));

    // Every row converts its own bound — chord length, arc length, sagitta — into the one arc-length
    // step the circular generator consumes, so linear stationing is row-invariant.
    [UseDelegateFromConstructor]
    private partial double ArcStep(double radiusMm, double boundMm);

    public Fin<Seq<Point3d>> Project(Point3d from, Move move, double boundMm) => move.Switch(
        state: (From: from, Bound: boundMm, Row: this),
        rapid: static (state, row) => Fin.Succ(Stations(state.From, row.Target, state.Bound)),
        linear: static (state, row) => Fin.Succ(Stations(state.From, row.Target, state.Bound)),
        circular: static (state, row) => Radius(state.From, row.Target, row.Arc).Map(radius =>
            Circular(state.From, row.Target, row.Arc, radius, state.Row.ArcStep(radius, state.Bound))));

    private static Seq<Point3d> Stations(Point3d from, Point3d to, double stepMm) {
        int count = Math.Max(1, (int)Math.Ceiling(from.DistanceTo(to) / stepMm));
        return toSeq(Enumerable.Range(1, count)).Map(index => PointAt(from, to, (double)index / count));
    }

    private static Seq<Point3d> Circular(Point3d from, Point3d to, ArcCenter arc, double radius, double stepMm) {
        double start = Math.Atan2(from.Y - arc.Center.Y, from.X - arc.Center.X);
        double finish = Math.Atan2(to.Y - arc.Center.Y, to.X - arc.Center.X);
        double sweep = Sweep(start, finish, arc.Sense == RotationSense.Clockwise);
        int count = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) * radius / Math.Max(stepMm, radius * Math.Sqrt(double.Epsilon))));
        return toSeq(Enumerable.Range(1, count)).Map(index => {
            double t = (double)index / count;
            double angle = start + (sweep * t);
            return index == count
                ? to
                : new Point3d(arc.Center.X + (radius * Math.Cos(angle)), arc.Center.Y + (radius * Math.Sin(angle)), from.Z + ((to.Z - from.Z) * t));
        });
    }

    private static Fin<double> Radius(Point3d from, Point3d to, ArcCenter arc) {
        double start = Math.Sqrt(Math.Pow(from.X - arc.Center.X, 2.0) + Math.Pow(from.Y - arc.Center.Y, 2.0));
        double finish = Math.Sqrt(Math.Pow(to.X - arc.Center.X, 2.0) + Math.Pow(to.Y - arc.Center.Y, 2.0));
        return start > 0.0
            && Math.Abs(start - finish) <= Math.Sqrt(double.Epsilon) * Math.Max(start, finish)
            ? Fin.Succ(start)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:arc-radius").ToError());
    }

    private static double Sweep(double start, double finish, bool clockwise) {
        if (Math.Abs(finish - start) <= Math.Sqrt(double.Epsilon)) return clockwise ? -Math.Tau : Math.Tau;
        double delta = Math.IEEERemainder(finish - start, Math.Tau);
        return clockwise && delta > 0.0 ? delta - Math.Tau : !clockwise && delta < 0.0 ? delta + Math.Tau : delta;
    }

    private static Point3d PointAt(Point3d from, Point3d to, double t) =>
        new(from.X + ((to.X - from.X) * t), from.Y + ((to.Y - from.Y) * t), from.Z + ((to.Z - from.Z) * t));
}

[ComplexValueObject]
public sealed partial class RemovalTolerance {
    public double GougeMm { get; }
    public double UncutMm3 { get; }
    public double OvercutMm3 { get; }
    public double AirCutRatio { get; }
    public double SurfaceMm { get; }
    public double UnresolvedRatio { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double gougeMm,
        ref double uncutMm3,
        ref double overcutMm3,
        ref double airCutRatio,
        ref double surfaceMm,
        ref double unresolvedRatio) =>
        validationError = Seq(gougeMm, uncutMm3, overcutMm3, airCutRatio, surfaceMm, unresolvedRatio)
            .ForAll(static value => double.IsFinite(value) && value >= 0.0)
            && airCutRatio <= 1.0 && unresolvedRatio <= 1.0
                ? null
                : new ValidationError("removal:tolerance");
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
        ref Plane frame) =>
        validationError = setup >= 0 && firstMove >= 0 && count > 0 && frame.IsValid
            ? null
            : new ValidationError("removal:setup-window");
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
    public double VoxelSizeMm { get; }
    public long VoxelCap { get; }
    public double StationMm { get; }
    public int SurfaceSamples { get; }
    public SweepSampling Sampling { get; }
    public RemovalTolerance Tolerance { get; }
    public CalibrationPolicy Calibration { get; }
    public Seq<SetupWindow> Setups { get; }
    public Map<int, Plane> ToolFrames { get; }

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
        ref double voxelSizeMm,
        ref long voxelCap,
        ref double stationMm,
        ref int surfaceSamples,
        ref SweepSampling sampling,
        ref RemovalTolerance tolerance,
        ref CalibrationPolicy calibration,
        ref Seq<SetupWindow> setups,
        ref Map<int, Plane> toolFrames) {
        bool finite = Seq(voxelSizeMm, stationMm).ForAll(double.IsFinite);
        bool frames = toolFrames.ForAll(static row => row.Key >= 0 && row.Value.IsValid);
        validationError = motion is not null && origin.IsValid && cutter is not null && stock is not null && target is not null && bounds.IsValid
            && finite && voxelSizeMm > 0.0 && stationMm > 0.0 && voxelCap > 0L && surfaceSamples > 0
            && sampling is not null && tolerance is not null && calibration is not null
            && holder.ForAll(static value => value is not null)
            && setups.ForAll(static value => value is not null) && frames
                ? null
                : new ValidationError("removal:policy");
    }
}

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

    // Volume and air-cut findings are quality evidence the verification atom projects and its `Clean`
    // property adjudicates; only a physical strike, a gouge past band, or evidence too sparse to
    // support any claim invalidates the run itself.
    public Option<Error> Fault(RemovalTolerance tolerance, CollisionZone zone) => Switch(
        state: (Tolerance: tolerance, Zone: zone),
        gouge: static (state, row) => row.DepthMm > state.Tolerance.GougeMm
            ? Some<Error>(FabricationFault.Gouge(row.Point, row.Cutter).ToError())
            : None,
        strike: static (state, row) => Some<Error>(FabricationFault.Collision(state.Zone, row.Contact).ToError()),
        uncut: static (_, _) => Option<Error>.None,
        overcut: static (_, _) => Option<Error>.None,
        airCut: static (_, _) => Option<Error>.None,
        deviation: static (state, row) => row.Field.Samples
            .Find(sample => sample.SignedMm < -state.Tolerance.SurfaceMm)
            .Map<Error>(sample => FabricationFault.Gouge(sample.Nominal, row.Field.Cutter).ToError()),
        unresolved: static (state, row) => row.Ratio > state.Tolerance.UnresolvedRatio
            ? Some<Error>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:surface-coverage").ToError())
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
public sealed record DeviationField(
    int Setup,
    ContentKey Field,
    ContentKey Key,
    CutterForm Cutter,
    Seq<DeviationSample> Samples,
    int Unresolved,
    double MinimumMm,
    double MaximumMm);
public readonly record struct RemovalMetrics(double UncutVolume, double OvercutVolume, double AirCutRatio);
file readonly record struct CutterSection(double OffsetMm, double RadiusMm, bool Round);
file readonly record struct Obstruction(
    CollisionContact Contact,
    Seq<(double X, double Y)> Ring,
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
    public static Fin<FabricationResult> Verify(VerifyPolicy policy, FabricationInput input) =>
        from requiredCells in RequiredCells(policy)
        from _ in Admit(policy, input, requiredCells)
        from budget in Admitted(
            VoxelBudget.Validate(policy.Bounds, policy.VoxelSizeMm, policy.VoxelCap, requiredCells, out VoxelBudget cells),
            cells,
            "removal:voxel-budget")
        from runtime in Admitted(
            ImplicitPolicy.Validate(
                budget,
                Length.FromMillimeters(policy.VoxelSizeMm),
                new CliMode.Grayscale(ESliceMode.SignedDistance, MaskSampling.Interpolated),
                policy.Calibration,
                policy.Stock.FromVoxels,
                out ImplicitPolicy composed),
            composed,
            "removal:implicit-policy")
        from obstructions in Obstructions(policy)
        from result in Capture(() => {
            ImplicitOp.Source stock = new(policy.Stock, Seq<VoxelMorphologyStep>(), runtime);
            ImplicitOp.Source target = new(policy.Target, Seq<VoxelMorphologyStep>(), runtime);
            return Implicit.Voxelize(
                Seq<ImplicitOp>(stock, target),
                scopes => Execute(policy, input.Snapshots, scopes[0].Native, scopes[1].Native, obstructions));
        })
        select result;

    private static Fin<T> Admitted<T>(ValidationError? error, T value, string locus) =>
        error is { } rejection
            ? Fin.Fail<T>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"{locus}:{rejection.Message}").ToError())
            : Fin.Succ(value);

    // Holder envelope and shank silhouette are program invariants; sampling them once keeps the
    // per-station strike test a pure membership query.
    private static Fin<Seq<Obstruction>> Obstructions(VerifyPolicy policy) => policy.Holder.Traverse(assembly =>
        from envelope in ToolMagazine.HolderEnvelope(assembly)
        from ring in Ring(envelope, Step(policy))
        let shank = (policy.Cutter.BodyDiameterMm | policy.Cutter.ShankDiameterMm)
            .Map(static diameter => diameter * 0.5)
            .Filter(_ => assembly.Stickout > policy.Cutter.FluteLength)
            .Map(radius => new Obstruction(
                CollisionContact.Shank,
                Circle(radius, Step(policy)),
                policy.Cutter.FluteLength,
                assembly.Stickout - policy.Cutter.FluteLength,
                radius))
        select shank.ToSeq() + Seq(new Obstruction(
            CollisionContact.Holder,
            ring,
            assembly.Stickout,
            assembly.GaugeLength,
            Reach(ring)))).As().Map(static rows => rows.IfNone(Seq<Obstruction>()));

    private static Seq<(double X, double Y)> Circle(double radiusMm, double resolutionMm) {
        int count = Math.Max(3, (int)Math.Ceiling(Math.Tau * radiusMm / resolutionMm));
        return toSeq(Enumerable.Range(0, count)).Map(index => {
            double angle = Math.Tau * index / count;
            return (radiusMm * Math.Cos(angle), radiusMm * Math.Sin(angle));
        });
    }

    private static double Reach(Seq<(double X, double Y)> ring) =>
        ring.Map(static point => Math.Sqrt((point.X * point.X) + (point.Y * point.Y))).Fold(0.0, double.Max);

    private static double Step(VerifyPolicy policy) => Math.Min(policy.StationMm, policy.VoxelSizeMm);

    private static Fin<Unit> Admit(VerifyPolicy policy, FabricationInput input, long requiredCells) =>
        (Gate(policy.Motion.Moves.Count > 0, "removal:motion"),
         Gate(input.Residual.ForAll(residual => residual.Key == policy.Stock.Key), "removal:stock-lineage"),
         Gate(Partitioned(policy), "removal:setup-partition"),
         Gate(policy.ToolFrames.ForAll(row => row.Key < policy.Motion.Moves.Count), "removal:tool-frame"),
         Gate(DepthWithin(policy), "removal:tool-depth"),
         Gate(requiredCells <= policy.VoxelCap, "removal:voxel-cap"))
        .Apply(static (_, _, _, _, _, _) => unit)
        .As()
        .ToFin();

    private static K<Validation<Error>, Unit> Gate(bool valid, string locus) =>
        guard(valid, new GeometryFault.DegenerateInput(Kind.Mesh, -1, locus).ToError()).ToFin().ToValidation();

    private static bool Partitioned(VerifyPolicy policy) {
        Seq<SetupWindow> windows = Windows(policy);
        return windows.Head.Map(static row => row.FirstMove == 0).IfNone(false)
            && windows.Last.Map(row => row.FirstMove + row.Count == policy.Motion.Moves.Count).IfNone(false)
            && windows.Map(static row => row.Setup).Distinct().Count == windows.Count
            && !toSeq(Enumerable.Range(1, Math.Max(0, windows.Count - 1)))
                .Exists(index => windows[index - 1].FirstMove + windows[index - 1].Count != windows[index].FirstMove);
    }

    private static bool DepthWithin(VerifyPolicy policy) {
        double admitted = Seq(
                policy.Cutter.MaxDepthMm,
                policy.Cutter.UsableLengthMm,
                policy.Cutter.FunctionalLengthMm,
                Some(policy.Cutter.FluteLength))
            .Bind(static value => value.ToSeq())
            .Fold(double.PositiveInfinity, double.Min);
        return Windows(policy).ForAll(window =>
            toSeq(policy.Motion.Moves.Skip(window.FirstMove).Take(window.Count)).ForAll(move =>
                move is Move.Rapid || Math.Abs((Target(move) - window.Frame.Origin) * window.Frame.ZAxis) <= admitted));
    }

    private static Fin<long> RequiredCells(VerifyPolicy policy) {
        Seq<double> axes = Seq(
            Math.Ceiling((policy.Bounds.Max.X - policy.Bounds.Min.X) / policy.VoxelSizeMm),
            Math.Ceiling((policy.Bounds.Max.Y - policy.Bounds.Min.Y) / policy.VoxelSizeMm),
            Math.Ceiling((policy.Bounds.Max.Z - policy.Bounds.Min.Z) / policy.VoxelSizeMm));
        if (!axes.ForAll(static count => double.IsFinite(count) && count >= 1.0 && count <= long.MaxValue))
            return Fin.Fail<long>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:voxel-grid").ToError());
        BigInteger required = axes.Map(static count => new BigInteger(count)).Fold(BigInteger.One, static (product, count) => product * count);
        return required <= long.MaxValue
            ? Fin.Succ((long)required)
            : Fin.Fail<long>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:voxel-grid").ToError());
    }

    private static Fin<FabricationResult> Execute(
        VerifyPolicy policy,
        Seq<StockSnapshot> prior,
        Voxels actual,
        Voxels target,
        Seq<Obstruction> obstructions) =>
        Windows(policy).FoldM<Fin, RemovalState>(
                new RemovalState(policy.Origin, prior, Seq<RemovalFinding>(), Field: None, AirMoves: 0, FeedMoves: 0),
                (state, window) => RemoveWindow(policy, actual, target, obstructions, state, window))
            .As()
            .Bind(run => Project(policy, actual, target, run));

    private static Fin<RemovalState> RemoveWindow(
        VerifyPolicy policy,
        Voxels actual,
        Voxels target,
        Seq<Obstruction> obstructions,
        RemovalState state,
        SetupWindow window) {
        using Voxels shadow = actual.voxDuplicate();
        List<Voxels> cuts = [];
        try {
            Seq<(Move Move, int Index)> moves = toSeq(policy.Motion.Moves.Skip(window.FirstMove).Take(window.Count))
                .Map((move, offset) => (move, window.FirstMove + offset));
            return moves.FoldM<Fin, RemovalState>(state with { Cursor = window.Frame.Origin },
                    (current, row) => Advance(policy, shadow, obstructions, cuts, window, current, row.Move, row.Index))
                .As()
                .Bind(removed => CommitWindow(policy, actual, target, cuts, window, removed));
        }
        finally { cuts.ForEach(static cut => cut.Dispose()); }
    }

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
        return Strikes(policy, shadow, obstructions, state.Cursor, move, frame, window.Setup, index).Bind(strikes =>
            move is Move.Rapid
                ? Fin.Succ(state with { Cursor = Target(move), Findings = state.Findings + strikes })
                : from swept in SweepTool(policy, state.Cursor, move, frame)
                  let removes = Intersects(shadow, swept)
                  select CommitMove(shadow, cuts, swept, removes, state, move, strikes));
    }

    private static RemovalState CommitMove(
        Voxels shadow,
        List<Voxels> cuts,
        Voxels swept,
        bool removes,
        RemovalState state,
        Move move,
        Seq<RemovalFinding> strikes) {
        if (removes) {
            cuts.Add(swept);
            shadow.BoolSubtract(swept);
        } else swept.Dispose();
        return state with {
            Cursor = Target(move),
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
        RemovalState state) {
        actual.BoolSubtractAll(cuts);
        RemovalMetrics metrics = Metrics(actual, target, state);
        return from fieldKey in policy.Stock.FromVoxels(actual)
               from loops in ResidualLoops(actual, window.Frame)
               from field in Surface(policy, actual, target, window, fieldKey, loops, metrics)
               from snapshot in StockSnapshot.Admit(window.Setup, field.Key, loops)
               select state with {
                   Snapshots = state.Snapshots.Add(snapshot),
                   Findings = state.Findings + DeviationFindings(policy, window, field),
                   Field = Some(fieldKey),
               };
    }

    private static Fin<Voxels> SweepTool(VerifyPolicy policy, Point3d from, Move move, Plane frame) =>
        policy.Sampling.Project(from, move, Math.Min(policy.StationMm, policy.VoxelSizeMm)).Map(stations => {
            using Lattice lattice = new();
            Seq<CutterSection> sections = Sections(policy.Cutter, policy.VoxelSizeMm * 0.5);
            (Point3d Point, bool First) seed = (from, true);
            _ = stations.Fold(seed, (held, point) => {
                AddTool(lattice, held.Point, point, frame.ZAxis, sections, held.First);
                return (point, false);
            });
            return new Voxels(lattice);
        });

    private static void AddTool(Lattice lattice, Point3d from, Point3d to, Vector3d axis, Seq<CutterSection> sections, bool first) {
        Vector3 direction = new((float)axis.X, (float)axis.Y, (float)axis.Z);
        _ = sections.Fold(Option<CutterSection>.None, (previous, section) => {
            Vector3 a = ToVector(from) + (direction * (float)section.OffsetMm);
            Vector3 b = ToVector(to) + (direction * (float)section.OffsetMm);
            lattice.AddBeam(a, (float)section.RadiusMm, b, (float)section.RadiusMm, bRoundCap: section.Round);
            _ = previous.Iter(prior => lattice.AddBeam(
                b - (direction * (float)(section.OffsetMm - prior.OffsetMm)),
                (float)prior.RadiusMm,
                b,
                (float)section.RadiusMm,
                bRoundCap: prior.Round || section.Round));
            if (first) _ = previous.Iter(prior => lattice.AddBeam(
                a - (direction * (float)(section.OffsetMm - prior.OffsetMm)),
                (float)prior.RadiusMm,
                a,
                (float)section.RadiusMm,
                bRoundCap: prior.Round || section.Round));
            return Some(section);
        });
    }

    // Swept-envelope geometry derives from the family's own admission columns, never from a per-family arm:
    // `CornerRadius` seats the nose arc (zero flat, half-diameter ball, between toroidal) and `TaperFrom`
    // selects the body law, so a seventeenth `CutterFamily` row generates its silhouette with no edit here.
    // Every derivation is outward-bounding, so a narrowing family (dovetail) verifies against a superset.
    private static Seq<CutterSection> Sections(CutterForm cutter, double resolutionMm) {
        double radius = cutter.Diameter * 0.5;
        double nose = Math.Clamp(cutter.CornerRadius, 0.0, radius);
        double length = cutter.FluteLength;
        double floor = Math.Max(resolutionMm, radius * Math.Sqrt(double.Epsilon));
        return cutter.Family.TaperFrom.Switch(
            state: (Radius: radius, Nose: nose, Length: length, Floor: floor, Resolution: resolutionMm, Form: cutter),
            flat: static state => Extend(Nose(state.Radius, state.Nose, state.Length, state.Floor, state.Resolution), state.Length, state.Radius),
            edgeAngle: static state => Nose(state.Radius, state.Nose, state.Length, state.Floor, state.Resolution)
                .Add(new CutterSection(
                    state.Length,
                    state.Radius + ((state.Length - state.Nose) * Tilt(state.Form.TaperAngle)),
                    false)),
            halfPointAngle: static state => Extend(
                Seq(
                    new CutterSection(0.0, state.Floor, false),
                    new CutterSection(
                        Math.Min(state.Length, state.Radius / Tilt(state.Form.PointAngleDeg.IfNone(state.Form.TaperAngle * 2.0) * 0.5)),
                        state.Radius,
                        false)),
                state.Length,
                state.Radius));
    }

    private static double Tilt(double degrees) =>
        Math.Tan(Math.Clamp(degrees, Math.Sqrt(double.Epsilon), Math.BitDecrement(90.0)) * Math.PI / 180.0);

    private static Seq<CutterSection> Nose(double radiusMm, double noseMm, double lengthMm, double floorMm, double resolutionMm) =>
        noseMm <= floorMm
            ? Seq(new CutterSection(0.0, radiusMm, false))
            : Profile(
                Math.Min(noseMm, lengthMm),
                resolutionMm,
                offset => radiusMm - noseMm + Math.Sqrt(Math.Max(0.0, (noseMm * noseMm) - Math.Pow(noseMm - offset, 2.0))));

    private static Seq<CutterSection> Profile(double extentMm, double resolutionMm, Func<double, double> radius) {
        int count = Math.Max(1, (int)Math.Ceiling(extentMm / resolutionMm));
        return toSeq(Enumerable.Range(0, count + 1))
            .Map(index => new CutterSection(
                extentMm * index / count,
                Math.Max(radius(extentMm * index / count), resolutionMm * Math.Sqrt(double.Epsilon)),
                true));
    }

    private static Seq<CutterSection> Extend(Seq<CutterSection> profile, double lengthMm, double radiusMm) =>
        profile.Last.Filter(last => last.OffsetMm < lengthMm)
            .Map(_ => profile.Add(new CutterSection(lengthMm, radiusMm, false)))
            .IfNone(profile);

    private static Fin<Seq<RemovalFinding>> Strikes(
        VerifyPolicy policy,
        Voxels actual,
        Seq<Obstruction> obstructions,
        Point3d from,
        Move move,
        Plane frame,
        int setup,
        int index) => obstructions.IsEmpty
        ? Fin.Succ(Seq<RemovalFinding>())
        : from sampled in policy.Sampling.Project(from, move, Step(policy))
          let stations = Seq(from) + sampled
          select obstructions.Bind(row => stations
              .Find(point => Touches(actual, row, frame, point))
              .Map(point => (RemovalFinding)new RemovalFinding.Strike(setup, index, point, row.Contact, row.ReachMm))
              .ToSeq());

    private static Fin<Seq<(double X, double Y)>> Ring(Loop envelope, double resolutionMm) =>
        from result in envelope.Apply(new ProfileOp.Measure())
        from path in result is ProfileResult.Measure measured
            ? Fin.Succ(measured.Path.Millimeters)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:holder-measure").ToError())
        let count = Math.Max(envelope.Vertices.Count, (int)Math.Ceiling(path / resolutionMm))
        from ring in toSeq(Enumerable.Range(0, count)).TraverseM(index =>
            envelope.Apply(new ProfileOp.Sample(Length.FromMillimeters(path * index / count))).Bind(sample =>
                sample is ProfileResult.Sampled point
                    ? Fin.Succ((point.Point.X, point.Point.Y))
                    : Fin.Fail<(double X, double Y)>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:holder-sample").ToError()))).As()
        select ring;

    private static bool Touches(Voxels actual, Obstruction row, Plane frame, Point3d station) {
        Vector3 axis = new((float)frame.ZAxis.X, (float)frame.ZAxis.Y, (float)frame.ZAxis.Z);
        Vector3 center = ToVector(station) + (axis * (float)(row.StartMm + (row.LengthMm * 0.5)));
        float half = (float)(row.ReachMm + (row.LengthMm * 0.5));
        using Voxels prism = new(
            new ProfilePrism(row.Ring, frame, station, row.StartMm, row.LengthMm),
            new BBox3(center - new Vector3(half), center + new Vector3(half)));
        return Intersects(actual, prism);
    }

    private static Fin<DeviationField> Surface(
        VerifyPolicy policy,
        Voxels actual,
        Voxels target,
        SetupWindow window,
        ContentKey fieldKey,
        Arr<Loop> loops,
        RemovalMetrics metrics) {
        using PicoGK.Mesh mesh = target.mshAsMesh();
        int triangles = mesh.nTriangleCount();
        if (triangles == 0)
            return Fin.Fail<DeviationField>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:target-surface").ToError());
        // Index-uniform triangle selection samples a finely tessellated region far denser than a coarse
        // one, so the deviation field would under-cover exactly the large flat faces a gouge escapes on;
        // Cumulative-area prefixing makes selection area-uniform over the target surface instead.
        Seq<(int Triangle, double Area)> surface = toSeq(Enumerable.Range(0, triangles)).Choose(index => {
            mesh.GetTriangle(index, out Vector3 a, out Vector3 b, out Vector3 c);
            double area = 0.5 * Vector3.Cross(b - a, c - a).Length();
            return double.IsFinite(area) && area > 0.0 ? Some((index, area)) : None;
        });
        if (surface.IsEmpty)
            return Fin.Fail<DeviationField>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:target-area").ToError());
        double[] cumulative = new double[surface.Count];
        _ = toSeq(Enumerable.Range(0, surface.Count)).Fold(0.0, (running, index) => {
            cumulative[index] = running + surface[index].Area;
            return cumulative[index];
        });
        double total = cumulative[surface.Count - 1];
        Seq<(Option<DeviationSample> Sample, int Unresolved)> rows = toSeq(Enumerable.Range(0, policy.SurfaceSamples)).Map(index => {
            int face = Math.Clamp(
                Array.BinarySearch(cumulative, total * (index + 0.5) / policy.SurfaceSamples) is var found && found >= 0 ? found : ~found,
                0,
                surface.Count - 1);
            int triangle = surface[face].Triangle;
            mesh.GetTriangle(triangle, out Vector3 a, out Vector3 b, out Vector3 c);
            Vector3 cross = Vector3.Cross(b - a, c - a);
            if (!float.IsFinite(cross.LengthSquared()) || cross.LengthSquared() <= float.Epsilon)
                return (Option<DeviationSample>.None, 1);
            Vector3 normal = Vector3.Normalize(cross);
            Point3d centroid = ToPoint((a + b + c) / 3.0f);
            double root = Math.Sqrt(Deterministic.UnitInterval(centroid, salt: index));
            double sweep = Deterministic.UnitInterval(centroid, salt: index, seed: 1);
            Vector3 nominal = (float)(1.0 - root) * a + (float)(root * (1.0 - sweep)) * b + (float)(root * sweep) * c;
            Option<DeviationSample> positive = Ray(actual, nominal, normal, 1.0);
            Option<DeviationSample> negative = Ray(actual, nominal, -normal, -1.0);
            Option<DeviationSample> nearest = (positive, negative).Apply((outside, inside) =>
                Math.Abs(outside.SignedMm) <= Math.Abs(inside.SignedMm) ? outside : inside)
                | positive
                | negative;
            return (nearest, nearest.IsNone ? 1 : 0);
        });
        Seq<DeviationSample> samples = rows.Bind(static row => row.Sample.ToSeq());
        int unresolved = rows.Map(static row => row.Unresolved).Sum();
        ContentKey key = SnapshotKey(policy, window, fieldKey, loops, metrics, samples, unresolved);
        return Fin.Succ(new DeviationField(
            window.Setup,
            fieldKey,
            key,
            policy.Cutter,
            samples,
            unresolved,
            samples.Map(static row => row.SignedMm).Fold(double.PositiveInfinity, double.Min) is var minimum && double.IsFinite(minimum) ? minimum : 0.0,
            samples.Map(static row => row.SignedMm).Fold(double.NegativeInfinity, double.Max) is var maximum && double.IsFinite(maximum) ? maximum : 0.0));
    }

    private static Option<DeviationSample> Ray(Voxels actual, Vector3 nominal, Vector3 direction, double sign) =>
        actual.bRayCastToSurface(nominal, direction, out Vector3 hit)
            ? Some(new DeviationSample(ToPoint(nominal), new Vector3d(direction.X, direction.Y, direction.Z), sign * Vector3.Distance(nominal, hit)))
            : None;

    private static ContentKey SnapshotKey(
        VerifyPolicy policy,
        SetupWindow window,
        ContentKey fieldKey,
        Arr<Loop> loops,
        RemovalMetrics metrics,
        Seq<DeviationSample> samples,
        int unresolved) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, fieldKey.Digest);
        Write(writer, policy.Stock.Key.Digest);
        Write(writer, policy.Target.Key.Digest);
        Write(writer, policy.Origin);
        Write(writer, window.Setup);
        Write(writer, window.FirstMove);
        Write(writer, window.Count);
        Write(writer, window.Frame);
        Write(writer, policy.Bounds);
        Write(writer, policy.Cutter.Family.Key);
        Write(writer, policy.Cutter.Diameter);
        Write(writer, policy.Cutter.CornerRadius);
        Write(writer, policy.Cutter.TaperAngle);
        Write(writer, policy.Cutter.FluteLength);
        Write(writer, policy.Cutter.UsableLengthMm);
        Write(writer, policy.Cutter.FunctionalLengthMm);
        Write(writer, policy.Cutter.OverallLengthMm);
        Write(writer, policy.Cutter.BodyDiameterMm);
        Write(writer, policy.Cutter.ShankDiameterMm);
        Write(writer, policy.Cutter.MaxDepthMm);
        Write(writer, policy.Cutter.LeadAngleDeg);
        Write(writer, policy.Cutter.PointAngleDeg);
        Write(writer, policy.Cutter.OrientationDeg);
        Write(writer, policy.Cutter.Evidence.Map(static evidence => evidence.StructuralDigest));
        Write(writer, policy.Holder.Map(static assembly => assembly.Identity));
        Write(writer, policy.VoxelSizeMm);
        Write(writer, policy.VoxelCap);
        Write(writer, policy.StationMm);
        Write(writer, policy.SurfaceSamples);
        Write(writer, policy.Sampling.Key);
        Write(writer, policy.Calibration.MinimumSamples);
        Write(writer, policy.Calibration.MaximumSamples);
        Write(writer, policy.Calibration.QuantileError.DecimalFractions);
        Write(writer, policy.Calibration.DensityFloor.DecimalFractions);
        Write(writer, policy.Calibration.GradientFloorPerMillimeter);
        Write(writer, policy.Tolerance.GougeMm);
        Write(writer, policy.Tolerance.UncutMm3);
        Write(writer, policy.Tolerance.OvercutMm3);
        Write(writer, policy.Tolerance.AirCutRatio);
        Write(writer, policy.Tolerance.SurfaceMm);
        Write(writer, policy.Tolerance.UnresolvedRatio);
        Write(writer, metrics.UncutVolume);
        Write(writer, metrics.OvercutVolume);
        Write(writer, metrics.AirCutRatio);
        Write(writer, unresolved);
        Write(writer, policy.Motion.Moves.Count);
        _ = policy.Motion.Moves.Iter(move => move.Switch(
            state: writer,
            rapid: static (held, value) => {
                Write(held, 0);
                Write(held, value.Target);
                return unit;
            },
            linear: static (held, value) => {
                Write(held, 1);
                Write(held, value.Target);
                Write(held, value.Feed);
                return unit;
            },
            circular: static (held, value) => {
                Write(held, 2);
                Write(held, value.Target);
                Write(held, value.Feed);
                Write(held, value.Arc.Center);
                Write(held, value.Arc.Sense.Key);
                return unit;
            }));
        Write(writer, policy.ToolFrames.Count);
        _ = toSeq(policy.ToolFrames).OrderBy(static row => row.Key).Iter(row => {
            Write(writer, row.Key);
            Write(writer, row.Value);
        });
        Seq<SetupWindow> windows = Windows(policy);
        Write(writer, windows.Count);
        _ = windows.Iter(setup => {
            Write(writer, setup.Setup);
            Write(writer, setup.FirstMove);
            Write(writer, setup.Count);
            Write(writer, setup.Frame);
        });
        Write(writer, loops.Count);
        _ = loops.Map(Canonical).OrderBy(static payload => Convert.ToHexString(payload)).Iter(payload => {
            Write(writer, payload.Length);
            Write(writer, payload);
        });
        Write(writer, samples.Count);
        _ = samples.Iter(sample => {
            Write(writer, sample.Nominal);
            Write(writer, sample.Normal);
            Write(writer, sample.SignedMm);
        });
        return ContentKey.Of(EgressKind.StockSnapshot, writer.WrittenSpan);
    }

    private static ContentKey ResidualKey(ContentKey field) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, field.Digest);
        return ContentKey.Of(EgressKind.Remnant, writer.WrittenSpan);
    }

    private static byte[] Canonical(Loop loop) {
        using ArrayPoolBufferWriter<byte> writer = new();
        int start = toSeq(Enumerable.Range(1, loop.Vertices.Count - 1)).Fold(0, (best, index) =>
            (loop.Vertices[index].X, loop.Vertices[index].Y, loop.Vertices[index].Z)
                .CompareTo((loop.Vertices[best].X, loop.Vertices[best].Y, loop.Vertices[best].Z)) < 0
                ? index
                : best);
        Write(writer, loop.Vertices.Count);
        _ = toSeq(Enumerable.Range(0, loop.Vertices.Count)).Iter(offset => {
            int index = (start + offset) % loop.Vertices.Count;
            Write(writer, loop.Vertices[index]);
            Write(writer, loop.Bulges[index]);
        });
        return writer.WrittenSpan.ToArray();
    }

    // A verified program that missed its band is a receipt with `Clean` false, not a failed rail: the atom
    // carries the volumes, the ratio, and the gouge witnesses precisely so the consumer reads the verdict.
    // Only a physical strike, an out-of-band gouge, or surface evidence too sparse to support any claim
    // invalidates the run, and the volume tolerance floors at the one voxel the field can resolve.
    private static Fin<FabricationResult> Project(VerifyPolicy policy, Voxels actual, Voxels target, RemovalState run) {
        RemovalMetrics metrics = Metrics(actual, target, run);
        Seq<RemovalFinding> findings = run.Findings + Findings(policy, metrics);
        double quantum = Math.Max(policy.Tolerance.OvercutMm3, Math.Pow(policy.VoxelSizeMm, 3.0));
        return from final in run.Snapshots.Last.ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:no-snapshot").ToError())
               from field in run.Field.ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:no-field").ToError())
               from residual in ResidualStock.Admit(ResidualKey(field), final.Machined)
               from zone in Admitted(
                   CollisionZone.Validate(policy.Stock.Key, policy.Bounds, out CollisionZone extent),
                   extent,
                   "removal:collision-zone")
               from _ in Invalidating(findings, policy.Tolerance, zone).Match(
                   Some: Fin.Fail<Unit>,
                   None: static () => Fin.Succ(unit))
               select (FabricationResult)new FabricationResult.VerificationResult(
                   residual,
                   run.Snapshots,
                   findings.Choose(static finding => finding.Witness),
                   metrics.UncutVolume,
                   metrics.OvercutVolume,
                   metrics.AirCutRatio,
                   quantum);
    }

    private static RemovalMetrics Metrics(Voxels actual, Voxels target, RemovalState run) =>
        new(Difference(actual, target), Difference(target, actual), run.FeedMoves == 0 ? 0.0 : (double)run.AirMoves / run.FeedMoves);

    private static Seq<RemovalFinding> Findings(VerifyPolicy policy, RemovalMetrics metrics) =>
        Seq(
            metrics.UncutVolume > policy.Tolerance.UncutMm3 ? Some<RemovalFinding>(new RemovalFinding.Uncut(metrics.UncutVolume)) : None,
            metrics.OvercutVolume > policy.Tolerance.OvercutMm3 ? Some<RemovalFinding>(new RemovalFinding.Overcut(metrics.OvercutVolume)) : None,
            metrics.AirCutRatio > policy.Tolerance.AirCutRatio ? Some<RemovalFinding>(new RemovalFinding.AirCut(metrics.AirCutRatio)) : None)
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
            .Filter(sample => sample.SignedMm < -policy.Tolerance.GougeMm)
            .Map(sample => (RemovalFinding)new RemovalFinding.Gouge(
                field.Setup,
                ClosestMove(policy, window, sample.Nominal),
                sample.Nominal,
                policy.Cutter,
                -sample.SignedMm));

    private static Option<Error> Invalidating(Seq<RemovalFinding> findings, RemovalTolerance tolerance, CollisionZone zone) {
        Seq<Error> errors = findings.Choose(finding => finding.Fault(tolerance, zone));
        return errors.Head.Map(first => errors.Tail.Fold(first, static (combined, error) => combined + error));
    }

    private static int ClosestMove(VerifyPolicy policy, SetupWindow window, Point3d point) =>
        toSeq(policy.Motion.Moves.Skip(window.FirstMove).Take(window.Count))
            .Map((move, offset) => (Move: move, Index: window.FirstMove + offset))
            .Fold(
                (Cursor: window.Frame.Origin, Index: window.FirstMove, Distance: double.PositiveInfinity),
                (state, row) => {
                    Point3d target = Target(row.Move);
                    double distance = SegmentDistance(state.Cursor, target, point);
                    return (target, distance < state.Distance ? row.Index : state.Index, Math.Min(distance, state.Distance));
                })
            .Index;

    private static double SegmentDistance(Point3d from, Point3d to, Point3d point) {
        Vector3d direction = to - from;
        if (direction.SquareLength == 0.0) return point.DistanceTo(from);
        double t = Math.Clamp(((point - from) * direction) / direction.SquareLength, 0.0, 1.0);
        return point.DistanceTo(from + (direction * t));
    }

    private static Fin<Arr<Loop>> ResidualLoops(Voxels actual, Plane frame) {
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
        return from context in Context.Millimeters().ToFin()
               from space in MeshSpace.Of(native, context)
               from result in Intersection.Apply(new IntersectOp.PlaneMesh(frame, space, IntersectPolicy.Canonical))
               from loops in result is IntersectResult.Chains chains
                   ? chains.Walked.Filter(static chain => chain.Closed)
                       .TraverseM(chain => Loop.Admit(toSeq(chain.Points).ToArr(), closed: true, bulges: Arr<double>(), tolerance: context).Map(static loop => loop.AsCcw()))
                       .As()
                   : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "removal:residual-section").ToError())
               select loops.ToArr();
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

    private static Seq<SetupWindow> Windows(VerifyPolicy policy) => policy.Setups.IsEmpty
        ? Seq(SetupWindow.Create(
            setup: 0,
            firstMove: 0,
            count: policy.Motion.Moves.Count,
            frame: new Plane(policy.Origin, Vector3d.XAxis, Vector3d.YAxis)))
        : policy.Setups.OrderBy(static row => row.FirstMove).ToSeq();

    private static Point3d Target(Move move) => move.Switch(
        rapid: static row => row.Target,
        linear: static row => row.Target,
        circular: static row => row.Target);

    // PicoGK allocation and library-mismatch exits are thrown, so the whole native walk funnels through one
    // lift; the self-flattening bind collapses the capture rail onto the walk's own typed outcome.
    private static Fin<T> Capture<T>(Func<Fin<T>> native) {
        ArgumentNullException.ThrowIfNull(native);
        return Try.lift<Fin<T>>(native)
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"removal:native:{error.Message}").ToError())
            .Bind(static result => result);
    }

    private static Vector3 ToVector(Point3d point) => new((float)point.X, (float)point.Y, (float)point.Z);
    private static Point3d ToPoint(Vector3 point) => new(point.X, point.Y, point.Z);

    private static void Write(ArrayPoolBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, long value) {
        BinaryPrimitives.WriteInt64LittleEndian(writer.GetSpan(sizeof(long)), value);
        writer.Advance(sizeof(long));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, UInt128 value) {
        BinaryPrimitives.WriteUInt64LittleEndian(writer.GetSpan(sizeof(ulong)), (ulong)value);
        writer.Advance(sizeof(ulong));
        BinaryPrimitives.WriteUInt64LittleEndian(writer.GetSpan(sizeof(ulong)), (ulong)(value >> 64));
        writer.Advance(sizeof(ulong));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, string value) {
        byte[] payload = Encoding.UTF8.GetBytes(value);
        Write(writer, payload.Length);
        Write(writer, payload);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Option<double> value) {
        Write(writer, value.IsSome ? 1 : 0);
        _ = value.Iter(amount => Write(writer, amount));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Option<string> value) {
        Write(writer, value.IsSome ? 1 : 0);
        _ = value.Iter(text => Write(writer, text));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Option<UInt128> value) {
        Write(writer, value.IsSome ? 1 : 0);
        _ = value.Iter(identity => Write(writer, identity));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Point3d value) {
        Write(writer, value.X);
        Write(writer, value.Y);
        Write(writer, value.Z);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Vector3d value) {
        Write(writer, value.X);
        Write(writer, value.Y);
        Write(writer, value.Z);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, BoundingBox value) {
        Write(writer, value.Min);
        Write(writer, value.Max);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Plane value) {
        Write(writer, value.Origin);
        Write(writer, value.XAxis);
        Write(writer, value.YAxis);
        Write(writer, value.ZAxis);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, byte[] value) {
        value.CopyTo(writer.GetSpan(value.Length));
        writer.Advance(value.Length);
    }

    private sealed class ProfilePrism(Seq<(double X, double Y)> ring, Plane frame, Point3d station, double start, double length) : IImplicit {
        public float fSignedDistance(in Vector3 at) {
            Vector3d local = new Point3d(at.X, at.Y, at.Z) - station;
            double x = local * frame.XAxis;
            double y = local * frame.YAxis;
            double z = local * frame.ZAxis;
            double planar = ring.Map((point, index) => SegmentDistance(point, ring[(index + 1) % ring.Count], x, y)).Fold(double.PositiveInfinity, double.Min);
            bool inside = ring.Map((point, index) => (point, next: ring[(index + 1) % ring.Count]))
                .Count(edge => edge.point.Y > y != edge.next.Y > y && x < edge.point.X + (((edge.next.X - edge.point.X) * (y - edge.point.Y)) / (edge.next.Y - edge.point.Y))) % 2 == 1;
            double slab = Math.Max(start - z, z - (start + length));
            return (float)Math.Max(inside ? -planar : planar, slab);
        }

        private static double SegmentDistance((double X, double Y) a, (double X, double Y) b, double x, double y) {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = (dx * dx) + (dy * dy);
            if (lengthSquared == 0.0) return Math.Sqrt(Math.Pow(a.X - x, 2.0) + Math.Pow(a.Y - y, 2.0));
            double t = Math.Clamp((((x - a.X) * dx) + ((y - a.Y) * dy)) / lengthSquared, 0.0, 1.0);
            return Math.Sqrt(Math.Pow(a.X + (t * dx) - x, 2.0) + Math.Pow(a.Y + (t * dy) - y, 2.0));
        }
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
