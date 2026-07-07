# [RASM_FABRICATION_REMOVAL]

The removal verifier is the program-level verify-time material truth plane for `Run(Verify)`: it starts from the content-keyed stock `VoxelWire`, subtracts the accumulated swept-tool volume from the actual stock state, compares that state against the target residual stock, and returns only owner#atoms-safe evidence through `FabricationResult.VerificationResult`. Guard remains the per-move design-time floor before Cam commits a feed; removal runs after the program exists and owns the verified stock truth, per-setup `StockSnapshot` chain, gouge fault routing, uncut/overcut/air-cut receipts, and narrow-phase K29 clearance checks.

## [01]-[INDEX]

- [01]-[REMOVAL]: owns `VerifyPolicy`, `SweepSampling`, `GougeTolerance`, `SetupWindow`, `RemovalFinding`, `RemovalMetrics`, `RemovalRun`, `RemovalCanonical`, and the one `Removal.Verify(VerifyPolicy, FabricationInput)` entry that subtracts PicoGK swept-tool voxels from the actual stock state and returns `FabricationResult.VerificationResult`.

## [02]-[REMOVAL]

- Owner: `VerifyPolicy` carries the already-conditioned `FabricationResult.Motion`, initial cursor, `CutterForm`, stock `VoxelWire`, target-residual `VoxelWire`, PicoGK bounds, voxel resolution, voxel cap, station pitch, sweep sampling row, tolerance row, and per-setup windows; `SweepSampling` owns station generation for chord, arc, and adaptive sampling; `GougeTolerance` owns finish/roughing/proof thresholds; `RemovalFinding` owns typed gouge/uncut/overcut/air-cut receipts; `RemovalRun` carries per-setup `StockSnapshot`s and air-cut counts; `RemovalCanonical` owns content bytes for `ContentKey.Of(EgressKind.StockSnapshot, ...)`; `Removal` owns the verify fold.
- Cases: `SweepSampling` rows `Chord` · `Arc` · `Adaptive`; `GougeTolerance` rows `Finish` · `Roughing` · `Proof`; `RemovalFinding` cases `Gouge(Point3d, CutterForm, double)` · `Uncut(double)` · `Overcut(double)` · `AirCut(double)`.
- Entry: `public static Fin<FabricationResult> Removal.Verify(VerifyPolicy policy, FabricationInput input)` is the ruled verify arm body for `FabricationPolicy.Verify(VerifyPolicy)`.
- Auto: `Verify` resolves the stock and target through `VoxelWire.ToVoxels`, mutates only the plane-local actual `Voxels`, partitions the carried motion by `SetupWindow`, builds each feed move's swept cutter as a PicoGK `Lattice` beam capsule, subtracts it through `Voxels.BoolSubtract`, mints a content-keyed `StockSnapshot` after every setup window, compares actual residual stock against target residual stock through voxel Boolean difference volumes, folds K29 `SupportSpace.Of(...).SignedDistance` as the narrow-phase clearance probe, projects every `ResidualStock`/`StockSnapshot` loop set from the ACTUAL residual voxel field (`mshAsMesh` → kernel `MeshSpace` → the K4 `PlaneMesh` section at the stock reference elevation — residual islands and cut topology from the true state, never a source-profile vertex filter; a failed projection yields the empty loop set beside the truthful key), and routes a verify-time critical gouge through shared `FabricationFault.Gouge(point, cutter).ToError()` 2706.
- Receipt: `RemovalFinding` is the local typed receipt set; the public result is `FabricationResult.VerificationResult(ResidualStock, Snapshots, Gouges, UncutVolume, OvercutVolume, AirCutRatio)` and carries no `Voxels`, `Lattice`, PicoGK `Mesh`, support-space handle, or internal receipt type.
- Packages: `api-picogk.md` (`Voxels`, `Lattice`, `BoolSubtract`, `BoolIntersect`, `voxDuplicate`, `CalculateProperties`, `bIsInside`, `BBox3`), `Additive/implicit#IMPLICIT` (`VoxelWire` and the ALC-safe content-keyed mesh↔`Voxels` seam), `Process/owner#FABRICATION_OWNER` (`CutterForm`, `Move`, `ResidualStock`, `StockSnapshot`, `ContentKey.Of`, `EgressKind.StockSnapshot`, `FabricationResult.VerificationResult`), `Process/faults#FAULT_BAND` (`Gouge` 2706), kernel K29 (`SupportSpace.Of`, `SignedDistance`), kernel K4/K15 (`MeshSpace.Of` admission + `Intersection.Apply` `PlaneMesh` — the residual-section projection over the extracted `mshAsMesh` wire), LanguageExt.Core, Thinktecture.Runtime.Extensions, RhinoCommon, BCL inbox.
- Growth: a new sampling law is one `SweepSampling` row; a new tolerance tranche is one `GougeTolerance` row; a new receipt scalar is one `RemovalFinding` case plus one `FabricationResult.VerificationResult` projection only when owner#atoms admits the payload; a new stock source is one `VoxelWire` adapter on the implicit seam; zero public entrypoint growth.
- Boundary: removal owns verify-time program truth, not design-time move admission; guard owns feed-floor safety before Cam commits; implicit owns PicoGK native posture and mesh↔voxel wire; owner#atoms owns result payloads and content keys; Persistence owns the `stock-snapshot` artifact-index enrollment row. A raw `XxHash128`/`GenerateHash`, a second voxel posture, terminal-only stock state, guard-side program verifier, result-carried `Voxels`, local clearance field, or removal-side Persistence type reference is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using LanguageExt;
using LanguageExt.Common;
using PicoGK;
using Rasm.Domain;                        // Context — the kernel admission runtime the residual projection threads
using Rasm.Fabrication.Additive;
using Rasm.Fabrication.Process;
using Rasm.Meshing;                       // MeshSpace · Intersection.Apply PlaneMesh — the K4 residual-section seam
using Rasm.Spatial;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Verify;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SweepSampling {
    public static readonly SweepSampling Chord = new("chord", static (from, move, step) => RemovalStations.Linear(from, move.To, step));
    public static readonly SweepSampling Arc = new("arc", static (from, move, step) => RemovalStations.Arc(from, move, step));
    public static readonly SweepSampling Adaptive = new("adaptive", static (from, move, step) => RemovalStations.Arc(from, move, Math.Max(step * 0.5, 1e-6)));

    [UseDelegateFromConstructor]
    public partial Seq<Point3d> Stations(Point3d from, Move move, double stepMm);
}

[SmartEnum<string>]
public sealed partial class GougeTolerance {
    public static readonly GougeTolerance Finish = new("finish", gougeMm: 0.01, uncutMm3: 0.10, overcutMm3: 0.05, airCutRatio: 0.20);
    public static readonly GougeTolerance Roughing = new("roughing", gougeMm: 0.05, uncutMm3: 1.00, overcutMm3: 0.25, airCutRatio: 0.35);
    public static readonly GougeTolerance Proof = new("proof", gougeMm: 0.001, uncutMm3: 0.01, overcutMm3: 0.01, airCutRatio: 0.05);

    public double GougeMm { get; }
    public double UncutMm3 { get; }
    public double OvercutMm3 { get; }
    public double AirCutRatio { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public readonly record struct SetupWindow(int Setup, int FirstMove, int Count);

public sealed record VerifyPolicy(
    FabricationResult.Motion Motion,
    Point3d Origin,
    CutterForm Cutter,
    VoxelWire Stock,
    VoxelWire Target,
    BBox3 Bounds,
    double VoxelSizeMm,
    long VoxelCap,
    double StationMm,
    SweepSampling Sampling,
    GougeTolerance Tolerance,
    Seq<SetupWindow> Setups);

public readonly record struct RemovalMetrics(double UncutVolume, double OvercutVolume, double AirCutRatio);

public readonly record struct RemovalRun(Seq<StockSnapshot> Snapshots, int AirMoves, int FeedMoves);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemovalFinding {
    private RemovalFinding() { }

    public sealed record Gouge(Point3d Point, CutterForm Tool, double DepthMm) : RemovalFinding;
    public sealed record Uncut(double VolumeMm3) : RemovalFinding;
    public sealed record Overcut(double VolumeMm3) : RemovalFinding;
    public sealed record AirCut(double Ratio) : RemovalFinding;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Removal {
    public static Fin<FabricationResult> Verify(VerifyPolicy policy, FabricationInput input) =>
        policy.Stock.ToVoxels().Bind(stock =>
            policy.Target.ToVoxels().Bind(target => Execute(policy, input, stock, target)));

    static Fin<FabricationResult> Execute(VerifyPolicy policy, FabricationInput input, Voxels stock, Voxels nominal) {
        using Voxels actual = stock;
        using Voxels target = nominal;
        RemovalRun run = Remove(policy, input, actual);
        RemovalMetrics metrics = Measure(actual, target, run);
        Seq<RemovalFinding> findings = Findings(policy, input, actual, target, metrics);
        ContentKey residualKey = run.Snapshots.Rev().HeadOrNone()
            .Map(static snapshot => snapshot.Key)
            .IfNone(ContentKey.Of(EgressKind.StockSnapshot, RemovalCanonical.Stock(actual, policy, setup: 0, metrics)));
        ResidualStock residual = new(residualKey, ResidualLoops(actual, policy));
        Seq<Point3d> gouges = GougeRows(findings).Map(static gouge => gouge.Point);
        FabricationResult.VerificationResult result = new(
            residual, run.Snapshots, gouges, metrics.UncutVolume, metrics.OvercutVolume, metrics.AirCutRatio);
        return CriticalGouge(findings, policy.Tolerance).Match(
            Some: fault => Fin.Fail<FabricationResult>(FabricationFault.Gouge(fault.Point, fault.Tool).ToError()),
            None: () => Fin.Succ<FabricationResult>(result));
    }

    static RemovalRun Remove(VerifyPolicy policy, FabricationInput input, Voxels actual) {
        Point3d cursor = policy.Origin;
        int air = 0;
        int feeds = 0;
        Seq<StockSnapshot> snapshots = Seq<StockSnapshot>();
        foreach (SetupWindow window in Windows(policy)) {
            Seq<Move> moves = toSeq(policy.Motion.Moves.Skip(window.FirstMove).Take(window.Count));
            foreach (Move move in moves) {
                if (move.Rapid) {
                    cursor = move.To;
                    continue;
                }
                feeds++;
                using Voxels swept = Sweep(cursor, move, policy);
                if (Touches(actual, swept)) actual.BoolSubtract(swept);
                else air++;
                cursor = move.To;
            }
            RemovalMetrics open = new(UncutVolume: Volume(actual), OvercutVolume: 0.0, AirCutRatio: feeds == 0 ? 0.0 : (double)air / feeds);
            ContentKey key = ContentKey.Of(EgressKind.StockSnapshot, RemovalCanonical.Stock(actual, policy, window.Setup, open));
            snapshots = snapshots.Add(new StockSnapshot(window.Setup, key, ResidualLoops(actual, policy)));
        }
        return new RemovalRun(snapshots, air, feeds);
    }

    // Ball-nose removal is the capsule beam; Lattice.AddBeam mints the swept cutter volume.
    static Voxels Sweep(Point3d from, Move move, VerifyPolicy policy) {
        using Lattice lattice = new();
        Point3d cursor = from;
        foreach (Point3d station in policy.Sampling.Stations(from, move, policy.StationMm)) {
            lattice.AddBeam(ToVector(cursor), ToolRadius(policy.Cutter), ToVector(station), ToolRadius(policy.Cutter), bRoundCap: true);
            cursor = station;
        }
        return new Voxels(lattice);
    }

    static bool Touches(Voxels actual, Voxels swept) {
        using Voxels overlap = swept.voxDuplicate();
        overlap.BoolIntersect(actual);
        return Volume(overlap) > 0.0;
    }

    static RemovalMetrics Measure(Voxels actual, Voxels target, RemovalRun run) =>
        new(
            UncutVolume: DifferenceVolume(actual, target),
            OvercutVolume: DifferenceVolume(target, actual),
            AirCutRatio: run.FeedMoves == 0 ? 0.0 : (double)run.AirMoves / run.FeedMoves);

    static Seq<RemovalFinding> Findings(VerifyPolicy policy, FabricationInput input, Voxels actual, Voxels target, RemovalMetrics metrics) {
        SupportSpace stockSpace = SupportSpace.Of(input.Profiles);
        Seq<RemovalFinding> gouges = policy.Motion.Moves
            .Filter(static move => !move.Rapid)
            .Bind(move => {
                double signed = stockSpace.SignedDistance(move.To);
                bool missing = target.bIsInside(ToVector(move.To)) && !actual.bIsInside(ToVector(move.To));
                double depth = Math.Max(Math.Abs(Math.Min(signed, 0.0)), missing ? policy.Tolerance.GougeMm + 1e-6 : 0.0);
                return depth > 0.0
                    ? Seq<RemovalFinding>(new RemovalFinding.Gouge(move.To, policy.Cutter, depth))
                    : Seq<RemovalFinding>();
            });
        return gouges.Concat(Seq<RemovalFinding>(
            new RemovalFinding.Uncut(metrics.UncutVolume),
            new RemovalFinding.Overcut(metrics.OvercutVolume),
            new RemovalFinding.AirCut(metrics.AirCutRatio)));
    }

    static Option<RemovalFinding.Gouge> CriticalGouge(Seq<RemovalFinding> findings, GougeTolerance tolerance) =>
        GougeRows(findings).Find(gouge => gouge.DepthMm > tolerance.GougeMm);

    static Seq<RemovalFinding.Gouge> GougeRows(Seq<RemovalFinding> findings) =>
        toSeq(findings.OfType<RemovalFinding.Gouge>());

    static Seq<SetupWindow> Windows(VerifyPolicy policy) =>
        policy.Setups.IsEmpty
            ? Seq(new SetupWindow(Setup: 0, FirstMove: 0, Count: policy.Motion.Moves.Count))
            : policy.Setups;

    static double DifferenceVolume(Voxels left, Voxels right) {
        using Voxels delta = left.voxDuplicate();
        delta.BoolSubtract(right);
        return Volume(delta);
    }

    static double Volume(Voxels voxels) {
        voxels.CalculateProperties(out float volume, out BBox3 _);
        return volume;
    }

    // The residual TRUTH projection: the final voxel field extracts through the declared mesh wire
    // (Voxels.mshAsMesh — the catalog's one crossing) into the kernel MeshSpace vocabulary and sections through
    // the kernel K4 PlaneMesh fold at the stock reference elevation — islands, holes, and cut topology come from
    // the ACTUAL residual state; the source profiles never masquerade as residual geometry. A projection failure
    // yields the EMPTY loop set (the snapshot key still identifies the true voxel state), never a stale substitute.
    static Arr<Loop> ResidualLoops(Voxels actual, VerifyPolicy policy) {
        using PicoGK.Mesh extracted = actual.mshAsMesh();
        Rhino.Geometry.Mesh native = new();
        foreach (int t in Enumerable.Range(0, extracted.nTriangleCount())) {
            extracted.GetTriangle(t, out Vector3 a, out Vector3 b, out Vector3 c);
            int ia = native.Vertices.Add(a.X, a.Y, a.Z);
            int ib = native.Vertices.Add(b.X, b.Y, b.Z);
            int ic = native.Vertices.Add(c.X, c.Y, c.Z);
            native.Faces.AddFace(ia, ib, ic);
        }
        return MeshSpace.Of(native, Context.Of(units: UnitSystem.Millimeters))
            .Bind(space => Intersection.Apply(new IntersectOp.PlaneMesh(
                new Plane(new Point3d(0.0, 0.0, policy.Origin.Z), Vector3d.ZAxis), space, IntersectPolicy.Canonical)))
            .Map(static result => result is IntersectResult.Chains chains
                ? chains.Walked.Filter(static chain => chain.Closed)
                    .Map(static chain => new Loop(toSeq(chain.Points).ToArr(), Closed: true).AsCcw())
                    .ToArr()
                : Arr<Loop>())
            .IfFail(Arr<Loop>());
    }

    static Vector3 ToVector(Point3d p) => new((float)p.X, (float)p.Y, (float)p.Z);

    static float ToolRadius(CutterForm cutter) => (float)Math.Max(cutter.Diameter * 0.5, 1e-6);
}

public static class RemovalStations {
    public static Seq<Point3d> Linear(Point3d from, Point3d to, double stepMm) {
        double length = from.DistanceTo(to);
        int count = Math.Max(1, (int)Math.Ceiling(length / Math.Max(stepMm, 1e-6)));
        return toSeq(Enumerable.Range(1, count)).Map(i => Lerp(from, to, (double)i / count));
    }

    public static Seq<Point3d> Arc(Point3d from, Move move, double stepMm) =>
        move.Arc.Match(
            Some: arc => ArcStations(from, move.To, arc, stepMm),
            None: () => Linear(from, move.To, stepMm));

    static Seq<Point3d> ArcStations(Point3d from, Point3d to, ArcCenter arc, double stepMm) {
        double radius = from.DistanceTo(arc.Center);
        double a0 = Math.Atan2(from.Y - arc.Center.Y, from.X - arc.Center.X);
        double a1 = Math.Atan2(to.Y - arc.Center.Y, to.X - arc.Center.X);
        double sweep = Delta(a0, a1, arc.Clockwise);
        int count = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) * Math.Max(radius, 1e-6) / Math.Max(stepMm, 1e-6)));
        return toSeq(Enumerable.Range(1, count)).Map(i => {
            double t = (double)i / count;
            double a = a0 + sweep * t;
            return new Point3d(
                arc.Center.X + radius * Math.Cos(a),
                arc.Center.Y + radius * Math.Sin(a),
                from.Z + (to.Z - from.Z) * t);
        });
    }

    static double Delta(double a0, double a1, bool clockwise) {
        double delta = a1 - a0;
        while (delta <= -Math.PI) delta += Math.Tau;
        while (delta > Math.PI) delta -= Math.Tau;
        return clockwise && delta > 0.0 ? delta - Math.Tau : !clockwise && delta < 0.0 ? delta + Math.Tau : delta;
    }

    static Point3d Lerp(Point3d a, Point3d b, double t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
}

public static class RemovalCanonical {
    public static byte[] Stock(Voxels actual, VerifyPolicy policy, int setup, RemovalMetrics metrics) {
        actual.CalculateProperties(out float volume, out BBox3 bounds);
        ArrayBufferWriter<byte> writer = new();
        Write(writer, setup);
        Write(writer, (double)volume);
        Write(writer, policy.VoxelSizeMm);
        Write(writer, policy.VoxelCap);
        Write(writer, policy.Cutter.Diameter);
        Write(writer, metrics.UncutVolume);
        Write(writer, metrics.OvercutVolume);
        Write(writer, metrics.AirCutRatio);
        Write(writer, (double)policy.Bounds.vecMin.X);
        Write(writer, (double)policy.Bounds.vecMin.Y);
        Write(writer, (double)policy.Bounds.vecMin.Z);
        Write(writer, (double)policy.Bounds.vecMax.X);
        Write(writer, (double)policy.Bounds.vecMax.Y);
        Write(writer, (double)policy.Bounds.vecMax.Z);
        Write(writer, (double)bounds.vecMin.X);
        Write(writer, (double)bounds.vecMin.Y);
        Write(writer, (double)bounds.vecMin.Z);
        Write(writer, (double)bounds.vecMax.X);
        Write(writer, (double)bounds.vecMax.Y);
        Write(writer, (double)bounds.vecMax.Z);
        return writer.WrittenSpan.ToArray();
    }

    static void Write(ArrayBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(4), value);
        writer.Advance(4);
    }

    static void Write(ArrayBufferWriter<byte> writer, long value) {
        BinaryPrimitives.WriteInt64LittleEndian(writer.GetSpan(8), value);
        writer.Advance(8);
    }

    static void Write(ArrayBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(8), value);
        writer.Advance(8);
    }
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Run["owner#run Verify policy case"] --> Removal["Removal.Verify"]
    Stock["Additive/implicit VoxelWire stock"] --> Actual["actual stock Voxels"]
    Target["target residual VoxelWire"] --> Nominal["target residual Voxels"]
    Motion["conditioned Motion moves"] --> Sweep["Lattice.AddBeam capsule sweep"]
    Cutter["owner#atoms CutterForm"] --> Sweep
    Sweep -->|"BoolSubtract per feed move"| Actual
    Actual -->|"ContentKey.Of stock-snapshot after each setup"| Snapshot["Seq<StockSnapshot>"]
    Actual -->|"Bool difference vs target"| Receipt["RemovalFinding gouge · uncut · overcut · air-cut"]
    Nominal --> Receipt
    Receipt -->|"critical gouge"| Fault["FabricationFault.Gouge 2706"]
    Receipt -->|"owner-safe projection"| Result["FabricationResult.VerificationResult"]
    Snapshot --> Result
    Actual --> Residual["ResidualStock for run N+1"]
    Residual --> Result
    K29["kernel K29 SupportSpace.SignedDistance"] --> Receipt
```
