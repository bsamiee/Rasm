# [RASM_FABRICATION_ALGEBRA]

`PolygonAlgebra` owns line-space fabrication geometry over `Clipper2`: one operation family admits line-only planar material, returns topology for region results and grouping for open runs, executes offset, Boolean, window, hygiene, morphology, inspection, and field projection, then emits one evidence-bearing result family. `Loop`, `Edge3`, and `Context` remain the boundary atoms, and `ArcAlgebra.Densify` remains the only bulge-to-line bridge.

`PolygonAlgebra.Apply` mirrors `Parametric.Apply`: one request, one `Op?` resolved through `OrDefault()`, and one `Fin<PolygonTrace>` rail. Each arm names its case for provenance. Malformed policy routes `key.InvalidInput()`, degenerate geometry routes `GeometryFault.DegenerateInput` under the true `Kind` with the element ordinal where one exists, and provider throws route `key.InvalidResult(detail)`. Requests carry policy values, foreign carriers terminate inside the owner, and results re-enter at the admitted context and elevation.

## [01]-[INDEX]

- [02]-[OPERATION_ALGEBRA]: `PolygonOp`, its policy families, one `Apply` dispatch, the `PolygonScan` reusable-subject handle, and the typed `PolygonTrace` egress.
- [03]-[FIELD_PLANE]: `FieldMetric` projects occupancy, signed clearance, cutter engagement, cutter reachability, and local inscribed diameter over the kernel `CellLattice` into one finite-gated plane receipt.

## [02]-[OPERATION_ALGEBRA]

- Owner: `PolygonOp` is the complete request family, and `PolygonAlgebra.Apply(PolygonOp?, Op?)` is its only public execution surface.
- Cases: `PolygonOp` and its input-shape policies jointly discriminate uniform and per-vertex offset, closed and open clipping, closed and open windowing, four hygiene algorithms, morphology, measurement, containment, topology, point-site cells, raster projection, and the minimum-area oriented envelope. Boolean, fill, join, and end vocabularies are the kernel `Rasm.Meshing` rows — a fabrication request is already a kernel request.
- Owner: `EdgeSeparation` extends the `Edge3` atom with the pure pair grammar — `Crosses(Edge3, double)` the tolerance-signed crossing verdict, `Gap(Edge3, double)` the exact separation (zero on a crossing, else the least of the four clamped endpoint projections) — deliberately non-monadic so hot per-pair censuses fold it directly; a `Fin` rail per probe is the rejected shape, and the kernel `IntersectOp.SegmentSegment` stays the exact-predicate crossing-point owner this surface never re-derives.
- Law: admission owns its vocabulary — `HygieneRule.Admit` and `FieldMetric.Admit` gate their own scalars on the case that declares them, so no arm dispatches the same discriminant twice, and `Op.Need`/`Finite`/`Positive` carry presence and scalar gates rather than page-local re-derivations.
- Entry: `OffsetField` survives on scalar-versus-matrix arity, `PolygonSource` on region-versus-run admission, and `HygieneRule` on algorithm payload timing; every collection owns singular and plural arity, and each case carries only the evidence its arm consumes.
- Auto: offset, boolean, morphology, and cells lower onto the kernel owners — `Offsetting.Apply` wavefront offsets (per-vertex distances riding `OffsetPolicy.EdgeSpeed` through the `Weighted` case), `Arrangement.Apply` `PlanarOverlay` exact ring booleans, `OffsetOp.Minkowski` morphology with `MorphologyKind.ReflectPattern` as the lowering law, and `Tessellation.Build` + `VoronoiDual(boundary)` bounded point-site cells; `ClipperD` owns the surviving precision-bearing lanes — window, hygiene, open-clip, region topology, measurement, containment, raster forest projection — behind the one `FillOf` seam; a placement scan hoists its recurring subject set into one `ReuseableDataContainer64` and folds it per position through `AddReuseableData`, `Rect64.Intersects` rejecting a disjoint candidate before the overlay runs; one `Try` boundary lowers package exceptions onto `Fin<T>`.
- Receipt: `PolygonTrace` distinguishes flat paths, region forests, grouped runs, split runs, measures, point relations, cell fields, sampled planes, and oriented envelopes by evidence timing; `RegionNode.Parent` carries the pre-order ordinal the tree walk assigns, never a re-scanned reference match. `Calipers` rests the optimum on a hull edge — its monotone-chain hull is a named statement kernel, structurally cheaper for a planar loop than the mesh-tier `Tessellation.LowerHull` or the host `CloudHullKind` rail, and an all-polygon-edges sweep under-searches a concave loop because the bridging hull edge is not a polygon edge.
- Packages: `Rasm` (project) supplies the boolean/fill/join/end vocabularies, the `Offsetting`/`Arrangement`/`Tessellation` owners, and the `CellLattice` plane; `Clipper2` supplies the surviving line-space lanes and the reusable scan container; `Thinktecture` supplies generated owners and exhaustive dispatch; `LanguageExt` supplies admission, traversal, immutable carriers, and the exception rail; `Rasm.Domain` supplies the `Op` key rail and the `Kind` fault taxonomy.
- Growth: a new operation is one `PolygonOp` case, one `PolygonTrace` case when its evidence differs, and one generated dispatch arm naming its own `Op`.
- Boundary: `ClipperD`, region measurement, and point relation are the statement-bearing native kernels; kernel-lowered arms terminate their `Chain` results back into `Loop` at the admitted context and elevation. Cells are Voronoi by definition, so a foreign bounded Fortune tessellator — with the third forked draw stream it carried — is the deleted form; relaxation and merge are folds over the kernel dual, never provider modes. Inputs share one `Context` and elevation before XY projection; bulges, mixed contexts, mixed elevations, invalid open edges, and closure-policy conflicts fail before execution, each naming the index of the first offending path.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Clipper2Lib;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// Boolean, fill, join, cap, and morphology vocabularies are the KERNEL'S — Rasm.Meshing BooleanOp
// (Union/Difference/Intersection/Xor), PolygonFill (NonZero/EvenOdd/Positive/Negative), JoinType, EndType
// (the joined open-ribbon row included), and OffsetOp.Minkowski. The second rosters this page carried
// mapped Clipper2 natives one-to-one and deleted with the engine lanes; the FillRule seam below is the
// one boundary map the SURVIVING Clipper2 lanes (window, hygiene, measure, contain, topology, open-clip,
// raster forest projection) still cross.

// Dilate-versus-erode is FABRICATION's discriminant, not the kernel's: both rows lower onto the ONE
// kernel Minkowski walk — Sum convolves directly, Difference convolves the point-reflected pattern and
// complements within the path region — so the engine the rows once selected is gone and the row data
// is the lowering law.
[SmartEnum<string>]
public sealed partial class MorphologyKind {
    public static readonly MorphologyKind Sum = new("sum", reflectPattern: false);
    public static readonly MorphologyKind Difference = new("difference", reflectPattern: true);

    internal bool ReflectPattern { get; }
}

[SmartEnum<string>]
public sealed partial class PointRelation {
    public static readonly PointRelation Outside = new("outside");
    public static readonly PointRelation Boundary = new("boundary");
    public static readonly PointRelation Inside = new("inside");
}

[Union]
public abstract partial record OffsetField {
    public sealed record Uniform(double Distance) : OffsetField;
    public sealed record Variable(Arr<Arr<double>> Distances) : OffsetField;
}

[Union]
public abstract partial record HygieneRule {
    public sealed record Simplify(double Epsilon) : HygieneRule;
    public sealed record RamerDouglasPeucker(double Epsilon) : HygieneRule;
    public sealed record Collinear : HygieneRule;
    public sealed record Duplicates : HygieneRule;

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        simplify: static (op, rule) => op.Positive(rule.Epsilon).Map(static _ => unit),
        ramerDouglasPeucker: static (op, rule) => op.Positive(rule.Epsilon).Map(static _ => unit),
        collinear: static (_, _) => Fin.Succ(unit),
        duplicates: static (_, _) => Fin.Succ(unit));
}

[Union]
public abstract partial record FieldMetric {
    public sealed record Occupancy : FieldMetric;
    public sealed record SignedClearance : FieldMetric;
    public sealed record Engagement(double CutterRadius) : FieldMetric;
    public sealed record Reachable(double ToolRadius) : FieldMetric;
    public sealed record InscribedDiameter : FieldMetric;

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        occupancy: static (_, _) => Fin.Succ(unit),
        signedClearance: static (_, _) => Fin.Succ(unit),
        engagement: static (op, metric) => op.Positive(metric.CutterRadius).Map(static _ => unit),
        reachable: static (op, metric) => op.Positive(metric.ToolRadius).Map(static _ => unit),
        inscribedDiameter: static (_, _) => Fin.Succ(unit));

    // Every row is a projection of ONE signed clearance plane, so the metric never selects a sampling strategy —
    // occupancy reads the sign, the cutter rows read the magnitude against their radius, and the plane is the kernel's.
    internal double Sample(double clearance) => Switch(
        state: clearance,
        occupancy: static (value, _) => value <= 0.0 ? 1.0 : 0.0,
        signedClearance: static (value, _) => value,
        engagement: static (value, field) => double.Clamp((field.CutterRadius - Math.Abs(value)) / field.CutterRadius, 0.0, 1.0),
        reachable: static (value, field) => value <= -field.ToolRadius ? 1.0 : 0.0,
        inscribedDiameter: static (value, _) => value < 0.0 ? -2.0 * value : 0.0);
}

[Union]
public abstract partial record PolygonSource {
    public sealed record Regions(Seq<Loop> Paths, PolygonFill Fill) : PolygonSource;
    public sealed record Edges(Seq<Seq<Edge3>> Paths, Context Tolerance, double Plane) : PolygonSource;
}

// Merge is DATA, never a caller delegate: a cell falls into its strongest surviving neighbour when its own area is
// under the floor or its centroid sits inside the separation of one already kept, so an area-driven and a
// distance-driven merge are one row apart and both replay from the receipt alone.
[ComplexValueObject]
public sealed partial class SiteMerge {
    public double MinimumArea { get; }
    public double MinimumSeparation { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double minimumArea, ref double minimumSeparation) =>
        validationError = double.IsFinite(minimumArea) && minimumArea >= 0.0
            && double.IsFinite(minimumSeparation) && minimumSeparation >= 0.0
            ? null
            : new ValidationError("site-merge:negative-or-non-finite");
}

// Relaxation is Lloyd's fold over the SAME kernel dual: each pass moves a seed toward its own cell centroid by the
// strength fraction and re-tessellates, so iteration count and strength are the whole vocabulary and no provider
// mode stands behind them. Strength one snaps to the centroid; zero leaves the seed field untouched.
[ComplexValueObject]
public sealed partial class SitePolicy {
    public int Relaxations { get; }
    public double RelaxationStrength { get; }
    public Option<SiteMerge> Merge { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref int relaxations, ref double relaxationStrength, ref Option<SiteMerge> merge) =>
        validationError = relaxations >= 0 && double.IsFinite(relaxationStrength) && relaxationStrength is >= 0.0 and <= 1.0
            ? null
            : new ValidationError("site-policy:relaxation-out-of-range");

    public static SitePolicy Canonical { get; } = Create(relaxations: 0, relaxationStrength: 0.0, merge: None);
}

// Offset and Boolean carry the KERNEL vocabularies whole — JoinType/EndType/OffsetPolicy and BooleanOp
// with the kernel fill row — so a fabrication request is already a kernel request and no second policy
// admission stands between them. Raster addresses its plane through the ONE CellLattice; the bounds,
// cell, census, and budget-ceiling admission that the deleted FieldGrid re-derived are CellLattice.Of's,
// each caller passing its own ceiling VALUE as the Of argument.
[Union]
public abstract partial record PolygonOp {
    public sealed record Offset(Seq<Loop> Paths, OffsetField Field, JoinType Join, EndType End, OffsetPolicy Policy) : PolygonOp;
    public sealed record Boolean(Seq<Loop> Subject, Seq<Loop> Clip, BooleanOp Kind, PolygonFill Fill) : PolygonOp;
    public sealed record ClipOpen(Seq<Seq<Edge3>> Subject, Seq<Loop> Clip, PolygonFill Fill) : PolygonOp;
    public sealed record Window(PolygonSource Source, BoundingBox Bounds) : PolygonOp;
    public sealed record Hygiene(Seq<Loop> Paths, HygieneRule Rule) : PolygonOp;
    public sealed record Morphology(Loop Pattern, Loop Path, MorphologyKind Kind) : PolygonOp;
    public sealed record Measure(Seq<Loop> Paths, PolygonFill Fill) : PolygonOp;
    public sealed record Contains(Seq<Loop> Paths, Arr<Point3d> Points, PolygonFill Fill) : PolygonOp;
    public sealed record Topology(Seq<Loop> Paths, PolygonFill Fill) : PolygonOp;
    public sealed record Cells(Arr<Point3d> Sites, Loop Boundary, SitePolicy Policy) : PolygonOp;
    public sealed record Raster(Seq<Loop> Paths, PolygonFill Fill, CellLattice Grid, FieldMetric Metric) : PolygonOp;
    public sealed record Calipers(Seq<Loop> Paths) : PolygonOp;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record RegionNode(
    int Index,
    Option<int> Parent,
    int Depth,
    bool IsHole,
    Loop Boundary,
    double SignedArea,
    BoundingBox Bounds);

public sealed record TopologyReceipt(Seq<RegionNode> Nodes, PolygonFill Fill, Context Tolerance, double Plane);

public sealed record PolygonMeasure(
    double SignedArea,
    double FilledArea,
    double BoundaryLength,
    Point3d Centroid,
    BoundingBox Bounds,
    int Outers,
    int Holes);

// Each cell carries its seed, its clipped ring, and the two figures every consumer reads off that ring — the
// centroid a relaxation pass moves the seed toward, and the area a merge rule tests. `Site` indexes the seed set the emitting
// pass tessellated: relaxation moves seeds and keeps that index, a merge drops seeds and renumbers over the
// survivors, so the receipt is the index authority and a caller re-keys off `Cells` rather than its own request.
public sealed record SiteCell(int Site, Point3d Seed, Loop Ring, Point3d Centroid, double Area);

// Adjacency carries the SHARED EDGE, not just the pair: the dual edge between two circumcentres IS the segment its
// two cells share, so a traversal weight, a common-line cut, and a link midpoint read off the diagram rather than
// re-deriving a boundary intersection the tessellation already computed.
// The minimum-area enclosing rectangle: `Along` is the unit direction of the hull edge the optimum rests on,
// `Anchor` the min-along/min-across corner at the loop's own elevation, `Length` the extent along, `Width`
// across; `Area` and `Aspect` derive here, so a yield or remnant consumer never re-sweeps directions the
// calipers already ranked.
public sealed record OrientedEnvelope(Point3d Anchor, Vector3d Along, double Length, double Width) {
    public double Area => Length * Width;
    public double Aspect => Length <= 0.0 || Width <= 0.0 ? 0.0 : Math.Min(Length, Width) / Math.Max(Length, Width);
}

public sealed record SiteEdge(int A, int B, Point3d Start, Point3d End) {
    public double Length => Start.DistanceTo(End);
    public Point3d Mid => new((Start.X + End.X) * 0.5, (Start.Y + End.Y) * 0.5, Start.Z);
}

// Seeds is the kernel point index over the RELAXED seed field, built once at emission — a Voronoi cell IS the
// nearest-seed region, so site lookup is that one query and never a ring walk, and a caller probing a whole scan
// field pays one build rather than one fold per probe.
public sealed record CellReceipt(
    Arr<SiteCell> Cells,
    Arr<SiteEdge> Adjacency,
    SpatialIndex Seeds,
    Loop Boundary,
    Context Tolerance,
    double Plane) {
    public Fin<int> Locate(Point3d sample, Op? key = null) {
        Op op = key.OrDefault();
        return Spatial.Apply(new SpatialOp.Query(Seeds, new SpatialQuery.Nearest(sample, K: 1)), op).Bind(answer => answer.Switch(
            state: op,
            index: static (key, _) => Fin.Fail<int>(key.InvalidResult(detail: "cells:non-query-answer")),
            wire: static (key, _) => Fin.Fail<int>(key.InvalidResult(detail: "cells:non-query-answer")),
            result: static (key, admitted) => admitted.Value is QueryResult.Nearest { Ordered: [int site, ..] }
                ? Fin.Succ(site)
                : Fin.Fail<int>(key.InvalidResult(detail: "cells:empty-nearest"))));
    }
}

public sealed record FieldReceipt(
    ReadOnlyMemory2D<double> Samples,
    CellLattice Grid,
    FieldMetric Metric,
    Context Tolerance,
    double Plane,
    double Minimum,
    double Maximum,
    double Average,
    double Deviation,
    Point3d MinimumAt,
    Point3d MaximumAt);

[Union]
public abstract partial record PolygonTrace {
    public sealed record Paths(Seq<Loop> Result) : PolygonTrace;
    public sealed record Regions(TopologyReceipt Result) : PolygonTrace;
    public sealed record Runs(Seq<Seq<Edge3>> Result) : PolygonTrace;
    public sealed record SplitRuns(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside) : PolygonTrace;
    public sealed record Measured(PolygonMeasure Result) : PolygonTrace;
    public sealed record Related(Arr<PointRelation> Result) : PolygonTrace;
    public sealed record Celled(CellReceipt Result) : PolygonTrace;
    public sealed record Field(FieldReceipt Result) : PolygonTrace;
    public sealed record Enveloped(OrientedEnvelope Result) : PolygonTrace;
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------------------------------------------------------------
// Scan shape beside the one-shot `Apply`: a placement scan re-tests ONE recurring subject set against every
// candidate position, so its vertex structure precomputes once and each position folds over that structure rather
// than rebuilding it. Precompute is integer-space by the container's own signature, so this capsule owns that
// one scale crossing; `Rect64.Intersects` rejects a disjoint candidate before any overlay runs, which a per-position
// `Execute` cannot express. A one-shot overlap is `PolygonOp.Boolean` — this handle earns its lifetime only when the
// subject outlives the candidate.
[BoundaryAdapter]
public sealed class PolygonScan : IDisposable {
    private readonly ReuseableDataContainer64 subject;
    private readonly Rect64 bounds;
    private readonly double scale;
    private readonly FillRule fill;

    private PolygonScan(ReuseableDataContainer64 subject, Rect64 bounds, double scale, FillRule fill) {
        this.subject = subject; this.bounds = bounds; this.scale = scale; this.fill = fill;
    }

    public static Fin<PolygonScan> Of(Seq<Loop> paths, PolygonFill fill, Op? key = null) =>
        from admitted in PolygonAlgebra.Regions(paths)
        from resolved in key.OrDefault().Need(fill)
        from scan in Try.lift(() => {
            double scale = PolygonAlgebra.Scale(admitted[0].Tolerance);
            Paths64 native = PolygonAlgebra.ToPaths64(admitted, scale);
            ReuseableDataContainer64 container = new();
            container.AddPaths(native, PathType.Subject, isOpen: false);
            return new PolygonScan(container, Clipper.GetBounds(native), scale, PolygonAlgebra.FillOf(resolved));
        }).Run().MapFail(error => key.OrDefault().InvalidResult(detail: error.Message)).As().ToFin()
        select scan;

    public Fin<bool> Intersects(Seq<Loop> candidate, Op? key = null) =>
        from admitted in PolygonAlgebra.Regions(candidate)
        from verdict in Try.lift(() => {
            Paths64 native = PolygonAlgebra.ToPaths64(admitted, scale);
            if (!bounds.Intersects(Clipper.GetBounds(native))) { return false; }
            Clipper64 engine = new();
            engine.AddReuseableData(subject);
            engine.AddClip(native);
            Paths64 result = [];
            return engine.Execute(ClipType.Intersection, fill, result) && result.Count > 0;
        }).Run().MapFail(error => key.OrDefault().InvalidResult(detail: error.Message)).As().ToFin()
        select verdict;

    public void Dispose() => subject.Clear();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
// Pure pair-separation surface over the `Edge3` atom — deliberately NON-monadic: per-pair censuses (scan-vector
// election, nest proximity) fold it inside hot loops where a `Fin` rail per probe is the rejected shape. The
// kernel `IntersectOp.SegmentSegment` stays the exact-predicate crossing-POINT owner; this surface answers the
// crossing VERDICT and the separation DISTANCE only and never re-derives the intersection point.
public static class EdgeSeparation {
    extension(Edge3 edge) {
        public bool Crosses(Edge3 other, double toleranceMm) {
            double areaFloor = toleranceMm * Math.Max(1.0, Math.Max(edge.A.DistanceTo(edge.B), other.A.DistanceTo(other.B)));
            int otherA = Orient(edge, other.A, areaFloor);
            int otherB = Orient(edge, other.B, areaFloor);
            int edgeA = Orient(other, edge.A, areaFloor);
            int edgeB = Orient(other, edge.B, areaFloor);
            return (otherA == 0 && Within(edge, other.A, toleranceMm))
                || (otherB == 0 && Within(edge, other.B, toleranceMm))
                || (edgeA == 0 && Within(other, edge.A, toleranceMm))
                || (edgeB == 0 && Within(other, edge.B, toleranceMm))
                || (otherA != otherB && edgeA != edgeB);
        }

        public double Gap(Edge3 other, double toleranceMm) =>
            edge.Crosses(other, toleranceMm)
                ? 0.0
                : Math.Min(
                    Math.Min(Projected(other.A, edge), Projected(other.B, edge)),
                    Math.Min(Projected(edge.A, other), Projected(edge.B, other)));

        // Point modality of the same grammar: non-crossing segment pairs meet at an endpoint projection, so the
        // pair form folds this one and a consumer's vertex-against-edge census reads it directly.
        public double Gap(Point3d probe) => Projected(probe, edge);
    }

    private static int Orient(Edge3 edge, Point3d probe, double areaFloor) {
        double area = ((edge.B.X - edge.A.X) * (probe.Y - edge.A.Y)) - ((edge.B.Y - edge.A.Y) * (probe.X - edge.A.X));
        return area > areaFloor ? 1 : area < -areaFloor ? -1 : 0;
    }

    private static bool Within(Edge3 edge, Point3d probe, double toleranceMm) =>
        probe.X >= Math.Min(edge.A.X, edge.B.X) - toleranceMm && probe.X <= Math.Max(edge.A.X, edge.B.X) + toleranceMm
     && probe.Y >= Math.Min(edge.A.Y, edge.B.Y) - toleranceMm && probe.Y <= Math.Max(edge.A.Y, edge.B.Y) + toleranceMm;

    private static double Projected(Point3d probe, Edge3 edge) {
        Vector3d span = edge.B - edge.A;
        Vector3d offset = probe - edge.A;
        double length = span * span;
        return probe.DistanceTo(edge.A + (length <= 0.0 ? Vector3d.Zero : Math.Clamp((offset * span) / length, 0.0, 1.0) * span));
    }
}

public static class PolygonAlgebra {
    public static Fin<PolygonTrace> Apply(PolygonOp? operation, Op? key = null) =>
        from admitted in key.OrDefault().Need(operation)
        let resolved = Resolve(admitted, key)
        from result in Try.lift<Fin<PolygonTrace>>(f: () => admitted.Switch(
                state: resolved,
                offset: static (op, request) => OffsetOf(request, op),
                boolean: static (op, request) => BooleanOf(request, op),
                clipOpen: static (op, request) => OpenClipOf(request, op),
                window: static (op, request) => WindowOf(request, op),
                hygiene: static (op, request) => HygieneOf(request, op),
                morphology: static (op, request) => MorphologyOf(request, op),
                measure: static (op, request) => MeasureOf(request, op),
                contains: static (op, request) => ContainsOf(request, op),
                topology: static (op, request) => TopologyOf(request, op),
                cells: static (op, request) => CellsOf(request, op),
                raster: static (op, request) => RasterOf(request, op),
                calipers: static (op, request) => CalipersOf(request, op)))
            .Run()
            .MapFail(error => resolved.InvalidResult(detail: error.Message))
            .Bind(static value => value)
        select result;

    private static Op Resolve(PolygonOp operation, Op? key) => key ?? operation.Switch(
        offset: static _ => Op.Of(name: nameof(PolygonOp.Offset)),
        boolean: static _ => Op.Of(name: nameof(PolygonOp.Boolean)),
        clipOpen: static _ => Op.Of(name: nameof(PolygonOp.ClipOpen)),
        window: static _ => Op.Of(name: nameof(PolygonOp.Window)),
        hygiene: static _ => Op.Of(name: nameof(PolygonOp.Hygiene)),
        morphology: static _ => Op.Of(name: nameof(PolygonOp.Morphology)),
        measure: static _ => Op.Of(name: nameof(PolygonOp.Measure)),
        contains: static _ => Op.Of(name: nameof(PolygonOp.Contains)),
        topology: static _ => Op.Of(name: nameof(PolygonOp.Topology)),
        cells: static _ => Op.Of(name: nameof(PolygonOp.Cells)),
        raster: static _ => Op.Of(name: nameof(PolygonOp.Raster)),
        calipers: static _ => Op.Of(name: nameof(PolygonOp.Calipers)));

    private static Fin<PolygonTrace> OffsetOf(PolygonOp.Offset request, Op op) =>
        from paths in Lines(request.Paths, Kind.Polyline)
        from field in op.Need(request.Field)
        from policy in op.Need(request.Policy)
        from _ in OffsetClosure(paths, request.End)
        from results in field.Switch(
            state: (Paths: paths, Join: request.Join, End: request.End, Policy: policy, Op: op),
            // Uniform distance: one kernel wavefront request per admitted path; join, cap, and policy pass whole.
            uniform: static (state, admitted) =>
                from delta in state.Op.Finite(admitted.Distance)
                from runs in state.Paths.TraverseM(path => Offsetting.Apply(
                    new OffsetOp.Offset(ToPolyline(path), delta, state.Join, state.End, state.Policy), state.Op)).As()
                select runs,
            // Per-vertex distances ride the kernel Weighted lane — each path's row IS its OffsetPolicy.EdgeSpeed table.
            variable: static (state, admitted) =>
                from sized in guard(
                        admitted.Distances.Count == state.Paths.Count
                        && admitted.Distances.ToSeq().Zip(state.Paths, static (row, path) => row.Count == path.Spans).ForAll(identity)
                        && TensorPrimitives.IsFiniteAll(admitted.Distances.Bind(static row => row).ToArray()),
                        state.Op.InvalidInput())
                    .ToFin()
                from runs in state.Paths.Zip(admitted.Distances.ToSeq()).TraverseM(item => Offsetting.Apply(
                    new OffsetOp.Weighted(ToPolyline(item.First), state.Policy with { EdgeSpeed = item.Second }), state.Op)).As()
                select runs)
        from chains in results.TraverseM(result => ChainsOf(result, op)).As()
        from loops in FromChains(chains.Bind(static c => c), paths[0].Tolerance, paths[0].Plane)
        from tree in TreeOf(ToPaths(loops), PolygonFill.NonZero, paths[0].Tolerance, op)
        from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, PolygonFill.NonZero, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> BooleanOf(PolygonOp.Boolean request, Op op) =>
        from subject in Regions(request.Subject)
        from clip in Regions(request.Clip, admitEmpty: true)
        from kind in op.Need(request.Kind)
        from fill in op.Need(request.Fill)
        from operands in Lines(subject.Concat(clip), Kind.Polyline)
        // Kernel exact overlay classifies signed winding under the request's own fill row; kept-region chains
        // re-enter as loops at the admitted elevation and the surviving union walk derives the region forest.
        from overlay in Arrangement.Apply(new ArrangementOp.PlanarOverlay(
            subject.Map(ToPolyline), clip.Map(ToPolyline), kind, Axis.Z,
            ArrangementPolicy.Canonical with { Fill = fill }), op)
        from chains in OverlayChainsOf(overlay, op)
        from loops in FromChains(chains, operands[0].Tolerance, operands[0].Plane)
        from tree in TreeOf(ToPaths(loops), fill, operands[0].Tolerance, op)
        from topology in TopologyOf(tree, operands[0].Tolerance, operands[0].Plane, fill, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> OpenClipOf(PolygonOp.ClipOpen request, Op op) =>
        from clip in Regions(request.Clip)
        from fill in op.Need(request.Fill)
        from subject in Edges(request.Subject, clip[0].Tolerance, clip[0].Plane)
        from result in ClipRuns(subject, clip, fill, op)
        select (PolygonTrace)new PolygonTrace.SplitRuns(result.Inside, result.Outside);

    private static Fin<PolygonTrace> WindowOf(PolygonOp.Window request, Op op) =>
        op.Need(request.Source).Bind(source => source.Switch(
            state: (Bounds: request.Bounds, Op: op),
            regions: static (state, admitted) =>
                from paths in Regions(admitted.Paths)
                from fill in state.Op.Need(admitted.Fill)
                from _ in Window(state.Bounds, paths[0].Tolerance)
                from clipped in FromPaths(
                    Clipper.RectClip(RectOf(state.Bounds), ToPaths(paths), Precision(paths[0].Tolerance)),
                    closed: true,
                    paths[0].Tolerance,
                    paths[0].Plane)
                from tree in TreeOf(ToPaths(clipped), fill, paths[0].Tolerance, state.Op)
                from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, fill, state.Op)
                select (PolygonTrace)new PolygonTrace.Regions(topology),
            edges: static (state, admitted) =>
                from tolerance in state.Op.Need(admitted.Tolerance)
                from paths in Edges(admitted.Paths, tolerance, admitted.Plane)
                from _ in Window(state.Bounds, tolerance)
                let clipped = Clipper.RectClipLines(RectOf(state.Bounds), ToOpenPaths(paths), Precision(tolerance))
                select (PolygonTrace)new PolygonTrace.Runs(Runs(clipped, admitted.Plane))));

    private static Fin<PolygonTrace> HygieneOf(PolygonOp.Hygiene request, Op op) =>
        from paths in Lines(request.Paths, Kind.Polyline)
        from rule in op.Need(request.Rule)
        from _ in rule.Admit(op)
        from result in paths.TraverseM(path => rule.Switch(
            state: path,
            simplify: static (loop, admitted) => FromPath(
                Clipper.SimplifyPath(ToPath(loop), admitted.Epsilon, loop.Closed), loop.Closed, loop.Tolerance, loop.Plane),
            ramerDouglasPeucker: static (loop, admitted) => FromPath(
                Clipper.RamerDouglasPeucker(ToPath(loop), admitted.Epsilon), loop.Closed, loop.Tolerance, loop.Plane),
            collinear: static (loop, _) => FromPath(
                Clipper.TrimCollinear(ToPath(loop), Precision(loop.Tolerance), isOpen: !loop.Closed),
                loop.Closed,
                loop.Tolerance,
                loop.Plane),
            duplicates: static (loop, _) => StripDuplicates(loop))).As()
        select (PolygonTrace)new PolygonTrace.Paths(result);

    private static Fin<PolygonTrace> MorphologyOf(PolygonOp.Morphology request, Op op) =>
        from kind in op.Need(request.Kind)
        from operands in Lines(Seq(request.Pattern, request.Path), Kind.Polyline)
        from _ in guard(operands[0].Closed, new GeometryFault.DegenerateInput(Kind.Polyline, 0, "morphology:open-pattern").ToError()).ToFin()
        // ReflectPattern IS the lowering law onto the ONE kernel Minkowski walk: Sum convolves the pattern
        // directly; Difference convolves the point-reflected pattern, then complements within the path region
        // through the same exact overlay the boolean arm crosses.
        from pattern in kind.ReflectPattern ? Reflected(operands[0]) : Fin.Succ(operands[0])
        from convolved in Offsetting.Apply(
            new OffsetOp.Minkowski(ToPolyline(operands[1]), ToPolyline(pattern), OffsetPolicy.Canonical), op)
        from chains in ChainsOf(convolved, op)
        from hull in FromChains(chains, operands[0].Tolerance, operands[0].Plane)
        from region in kind.ReflectPattern
            ? Arrangement.Apply(new ArrangementOp.PlanarOverlay(
                    Seq(ToPolyline(operands[1])), hull.Map(ToPolyline), BooleanOp.Difference, Axis.Z,
                    ArrangementPolicy.Canonical), op)
                .Bind(result => OverlayChainsOf(result, op))
                .Bind(complement => FromChains(complement, operands[0].Tolerance, operands[0].Plane))
            : Fin.Succ(hull)
        from tree in TreeOf(ToPaths(region), PolygonFill.NonZero, operands[0].Tolerance, op)
        from topology in TopologyOf(tree, operands[0].Tolerance, operands[0].Plane, PolygonFill.NonZero, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> MeasureOf(PolygonOp.Measure request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from tree in TreeOf(ToPaths(paths), fill, paths[0].Tolerance, op)
        from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, fill, op)
        from _ in guard(!topology.Nodes.IsEmpty, new GeometryFault.DegenerateInput(Kind.Polyline, None, "measure:empty-fill").ToError()).ToFin()
        select (PolygonTrace)new PolygonTrace.Measured(MeasureOf(topology));

    private static Fin<PolygonTrace> CalipersOf(PolygonOp.Calipers request, Op op) =>
        from paths in Lines(request.Paths, Kind.Polyline)
        let hull = Hull(paths.Bind(static path => toSeq(path.Vertices)))
        from _ in guard(hull.Count >= 3, new GeometryFault.DegenerateInput(Kind.Polyline, None, "calipers:collinear").ToError()).ToFin()
        select (PolygonTrace)new PolygonTrace.Enveloped(Envelope(hull));

    // Named statement kernel: Andrew monotone chain over the loop's own vertices. The mesh-tier exact
    // `Tessellation.LowerHull` and the host `CloudHullKind` rail both answer point CLOUDS at mesh or native
    // grain; a planar loop's O(n log n) chain is the structurally cheaper primitive here. The minimum-area
    // theorem rests the optimum on a hull EDGE — a concave loop's own edge set does not contain the bridging
    // hull edge, which is why an all-polygon-edges sweep under-searches and is the deleted form.
    private static Seq<Point3d> Hull(Seq<Point3d> points) {
        Arr<Point3d> sorted = points.Distinct().OrderBy(static point => point.X).ThenBy(static point => point.Y).ToArr();
        if (sorted.Count < 3) { return toSeq(sorted); }
        var chain = new Point3d[sorted.Count * 2];
        int k = 0;
        for (int i = 0; i < sorted.Count; i++) {
            while (k >= 2 && Turn(chain[k - 2], chain[k - 1], sorted[i]) <= 0.0) { k--; }
            chain[k++] = sorted[i];
        }
        for (int i = sorted.Count - 2, floor = k + 1; i >= 0; i--) {
            while (k >= floor && Turn(chain[k - 2], chain[k - 1], sorted[i]) <= 0.0) { k--; }
            chain[k++] = sorted[i];
        }
        return toSeq(chain.AsSpan(0, k - 1).ToArray());
    }

    private static double Turn(Point3d a, Point3d b, Point3d c) =>
        ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));

    private static OrientedEnvelope Envelope(Seq<Point3d> hull) {
        Seq<OrientedEnvelope> candidates = toSeq(Enumerable.Range(0, hull.Count))
            .Map(index => (A: hull[index], B: hull[(index + 1) % hull.Count]))
            .Filter(static edge => edge.A.DistanceTo(edge.B) > 0.0)
            .Map(edge => {
                double span = edge.A.DistanceTo(edge.B);
                (double ux, double uy) = ((edge.B.X - edge.A.X) / span, (edge.B.Y - edge.A.Y) / span);
                Seq<double> along = hull.Map(point => (point.X * ux) + (point.Y * uy));
                Seq<double> across = hull.Map(point => (point.Y * ux) - (point.X * uy));
                (double a0, double a1) = (along.Min(), along.Max());
                (double c0, double c1) = (across.Min(), across.Max());
                return new OrientedEnvelope(
                    Anchor: new Point3d((a0 * ux) - (c0 * uy), (a0 * uy) + (c0 * ux), hull[0].Z),
                    Along: new Vector3d(ux, uy, 0.0),
                    Length: a1 - a0,
                    Width: c1 - c0);
            });
        return candidates.Tail.Fold(candidates[0], static (best, next) => next.Area < best.Area ? next : best);
    }

    private static Fin<PolygonTrace> ContainsOf(PolygonOp.Contains request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from trace in PointsOf(paths, request.Points, fill)
        select trace;

    private static Fin<PolygonTrace> TopologyOf(PolygonOp.Topology request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from tree in TreeOf(ToPaths(paths), fill, paths[0].Tolerance, op)
        from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, fill, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    // Lloyd relaxation is the SAME dual re-read: tessellate, clip to the admitted ring, move each seed toward its
    // own cell centroid by the strength fraction, repeat. A zero-relaxation request runs the dual exactly once, so
    // relaxed and unrelaxed lanes stay one fold with no second entry and no provider mode behind either.
    private static Fin<PolygonTrace> CellsOf(PolygonOp.Cells request, Op op) =>
        from bounds in Regions(Seq(request.Boundary))
        from policy in op.Need(request.Policy)
        from _ in Defect(
                request.Sites.ToSeq(),
                point => !point.IsValid || Math.Abs(point.Z - bounds[0].Plane) > bounds[0].Tolerance.Absolute.Value,
                Kind.Point,
                "cells:off-plane")
            .As()
            .ToFin()
        from __ in guard(
                request.Sites.Count >= 3,
                new GeometryFault.DegenerateInput(Kind.Point, None, "cells:site-floor").ToError())
            .ToFin()
        let ring = ToPolyline(bounds[0])
        from relaxed in Range(0, policy.Relaxations).ToSeq().FoldM<Fin, Arr<Point3d>>(
            request.Sites,
            (seeds, _) => Dual(seeds, ring, bounds[0], op).Map(dual => Moved(seeds, dual.Cells, policy.RelaxationStrength))).As()
        from dual in Dual(relaxed, ring, bounds[0], op)
        from merged in policy.Merge.Match(
            Some: rule => Merged(dual, rule, ring, bounds[0], op),
            None: () => Fin.Succ(dual))
        from index in SeedIndex(merged.Cells, op)
        select (PolygonTrace)new PolygonTrace.Celled(new CellReceipt(
            merged.Cells, merged.Adjacency, index, bounds[0], bounds[0].Tolerance, bounds[0].Plane));

    private static Fin<PolygonTrace> RasterOf(PolygonOp.Raster request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from grid in op.Need(request.Grid)
        from metric in op.Need(request.Metric)
        from _ in Check(
                Math.Abs(grid.Bounds.Min.Z - paths[0].Plane) <= paths[0].Tolerance.Absolute.Value
                && Math.Abs(grid.Bounds.Max.Z - paths[0].Plane) <= paths[0].Tolerance.Absolute.Value,
                Kind.Plane,
                None,
                "raster:grid-elevation")
            .As()
            .ToFin()
            .Bind(_ => metric.Admit(op))
        from clearance in Clearance(paths, grid, op)
        let values = new double[grid.Rows.Value, grid.Columns.Value]
        let kernel = new RasterKernel(values, clearance, ToPaths(paths), fill, grid, Precision(paths[0].Tolerance), metric)
        from receipt in Raster(kernel, values, grid, metric, paths[0].Tolerance, paths[0].Plane)
        select (PolygonTrace)new PolygonTrace.Field(receipt);

    // --- [BOUNDARIES] -------------------------------------------------------------------------------------------------------------------------------
    private static K<Validation<Error>, Unit> Check(bool condition, Kind kind, Option<int> index, string witness) =>
        AdmissionSlots.Gate(condition, new GeometryFault.DegenerateInput(kind, index, witness).ToError());

    private static K<Validation<Error>, Unit> Defect<T>(Seq<T> values, Func<T, bool> offends, Kind kind, string witness) {
        Option<int> at = values
            .Map(static (value, index) => (Value: value, Index: index))
            .Filter(row => offends(row.Value))
            .Map(static row => row.Index)
            .Head;
        return Check(at.IsNone, kind, at, witness);
    }

    private static Fin<Seq<Loop>> Lines(Seq<Loop> paths, Kind kind, bool admitEmpty = false) =>
        (Check(admitEmpty || !paths.IsEmpty, kind, None, "empty"),
         Defect(paths, static path => path is null, kind, "null"),
         Defect(paths, static path => path is not null && path.Bulges.Exists(static bulge => bulge != 0.0), kind, "bulged"),
         Defect(paths, path => path is not null && paths[0] is not null && path.Tolerance != paths[0].Tolerance, kind, "mixed-context"),
         Defect(paths, path => path is not null && paths[0] is not null
             && Math.Abs(path.Plane - paths[0].Plane) > path.Tolerance.Absolute.Value, kind, "mixed-elevation"))
            .Apply(static (_, _, _, _, _) => paths)
            .As()
            .ToFin();

    internal static Fin<Seq<Loop>> Regions(Seq<Loop> paths, bool admitEmpty = false) =>
        from admitted in Lines(paths, Kind.Polyline, admitEmpty)
        from _ in Defect(admitted, static path => !path.Closed, Kind.Polyline, "open").As().ToFin()
        select admitted;

    private static Fin<Seq<Seq<Edge3>>> Edges(Seq<Seq<Edge3>> paths, Context tolerance, double plane) =>
        (Check(double.IsFinite(plane), Kind.Plane, None, "non-finite-plane"),
         Check(!paths.IsEmpty, Kind.Line, None, "empty"),
         Defect(paths, static path => path.IsEmpty, Kind.Line, "empty-run"),
         Defect(paths, static path => path.Exists(static edge => !edge.A.IsValid || !edge.B.IsValid), Kind.Line, "non-finite"),
         Defect(paths, path => path.Exists(edge => edge.A.DistanceTo(edge.B) <= tolerance.Absolute.Value), Kind.Line, "zero-length"),
         Defect(paths, path => path.Exists(edge => Math.Abs(edge.A.Z - plane) > tolerance.Absolute.Value
             || Math.Abs(edge.B.Z - plane) > tolerance.Absolute.Value), Kind.Line, "off-plane"),
         Defect(paths, path => Range(0, Math.Max(0, path.Count - 1))
             .Exists(index => path[index].B.DistanceTo(path[index + 1].A) > tolerance.Absolute.Value), Kind.Line, "discontinuous"))
            .Apply(static (_, _, _, _, _, _, _) => paths)
            .As()
            .ToFin();

    // Closed is the ring lane and demands closed rings; every cap row is the open-path ribbon and demands open runs.
    private static Fin<Unit> OffsetClosure(Seq<Loop> paths, EndType end) =>
        Defect(paths, path => end == EndType.Closed ? !path.Closed : path.Closed, Kind.Polyline, "offset:closure-conflict")
            .As()
            .ToFin();

    private static Fin<Unit> Window(BoundingBox bounds, Context tolerance) =>
        guard(
                bounds.IsValid
                && bounds.Diagonal.X > tolerance.Absolute.Value
                && bounds.Diagonal.Y > tolerance.Absolute.Value,
                new GeometryFault.DegenerateInput(Kind.BoundingBox, None, "window:degenerate").ToError())
            .ToFin();

    // Kernel Offset and Weighted arms return Curves by construction; any other case is a contract breach.
    private static Fin<Seq<Chain>> ChainsOf(OffsetResult result, Op op) => result.Switch(
        state: op,
        graph: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "offset:non-curve-result")),
        axis: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "offset:non-curve-result")),
        curves: static (_, admitted) => Fin.Succ(admitted.Offset.Loops),
        probe: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "offset:non-curve-result")));

    // PlanarOverlay requests answer Overlay by construction; the boolean and complex cases are mesh-lane results.
    private static Fin<Seq<Chain>> OverlayChainsOf(ArrangementResult result, Op op) => result.Switch(
        state: op,
        boolean: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "boolean:non-overlay-result")),
        overlay: static (_, admitted) => Fin.Succ(admitted.Loops),
        complex: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "boolean:non-overlay-result")));

    // Kernel seam adapters — a Loop rides its own 3D vertices into the kernel Polyline request (a closed ring
    // re-closes explicitly), and a returned Chain re-enters as a Loop at the admitted context and elevation.
    private static Polyline ToPolyline(Loop path) =>
        new(path.Closed ? path.Vertices.Add(path.Vertices[0]) : path.Vertices);

    private static Fin<Seq<Loop>> FromChains(Seq<Chain> chains, Context tolerance, double plane) =>
        chains.TraverseM(chain => {
            Arr<Point3d> points = toSeq(chain.Points).ToArr();
            Arr<Point3d> vertices = chain.Closed && points.Count > 1
                && points[0].DistanceTo(points[points.Count - 1]) <= tolerance.Absolute.Value
                    ? points.RemoveAt(points.Count - 1)
                    : points;
            return Loop.Admit(vertices.Map(point => new Point3d(point.X, point.Y, plane)), chain.Closed, [], tolerance);
        }).As();

    // Point-reflection through the pattern's reference origin — the Minkowski-difference pattern the kernel walk convolves.
    private static Fin<Loop> Reflected(Loop pattern) =>
        Loop.Admit(
            pattern.Vertices.Map(point => new Point3d(-point.X, -point.Y, point.Z)),
            pattern.Closed,
            [],
            pattern.Tolerance);

    // --- [SITE_CELLS]
    // One kernel tessellation per pass: the bounded-cell overload clips each site's cell to the admitted ring and the
    // plain dual carries the crossed Delaunay edge per dual edge, which IS cell adjacency in the same site-index
    // space. Both projections read ONE store, so adjacency can never disagree with the rings it describes.
    private static Fin<(Arr<SiteCell> Cells, Arr<SiteEdge> Adjacency)> Dual(
        Arr<Point3d> seeds,
        Polyline ring,
        Loop boundary,
        Op op) =>
        from tessellation in Tessellation.Build(
            new TessellationOp.Points(
                TessellationKind.Triangulation,
                [.. seeds.Map(static seed => new Implicit(seed))],
                Seq<Constraint>(),
                TessellationPolicy.Canonical,
                Axis.Z),
            op)
        from bounded in tessellation.VoronoiDual(ring, op)
        from graph in tessellation.VoronoiDual(op)
        from cells in toSeq(bounded).TraverseM(cell => Loop.Admit(
                toSeq(cell.Ring).Map(point => new Point3d(point.X, point.Y, boundary.Plane)).ToArr(),
                closed: true,
                Arr<double>(),
                boundary.Tolerance)
            .Map(loop => {
                double signed = Clipper.Area(ToPath(loop));
                return new SiteCell(cell.Site, seeds[cell.Site], loop, CentroidOf(Seq(loop), signed), Math.Abs(signed));
            })).As()
        // Any hull site whose cell the ring emptied leaves the diagram, so its dual edges leave with it. Edges and
        // Across are index-parallel by the dual's own contract — entry i is one dual segment and the site pair whose
        // bisector it lies on — so the shared segment travels with the pair rather than being re-derived downstream.
        let surviving = cells.Map(static cell => cell.Site).ToHashSet()
        select (cells.ToArr(), Range(0, graph.Across.Length).ToSeq()
            .Filter(i => surviving.Contains(graph.Across[i].U) && surviving.Contains(graph.Across[i].V))
            .Map(i => new SiteEdge(
                Math.Min(graph.Across[i].U, graph.Across[i].V),
                Math.Max(graph.Across[i].U, graph.Across[i].V),
                Elevate(graph.Circumcenters[graph.Edges[i].A], boundary.Plane),
                Elevate(graph.Circumcenters[graph.Edges[i].B], boundary.Plane)))
            .DistinctBy(static edge => (edge.A, edge.B))
            .ToArr());

    private static Point3d Elevate(Point3d point, double plane) => new(point.X, point.Y, plane);

    private static Arr<Point3d> Moved(Arr<Point3d> seeds, Arr<SiteCell> cells, double strength) =>
        cells.Fold(seeds, (moved, cell) => moved.SetItem(
            cell.Site,
            moved[cell.Site] + ((cell.Centroid - moved[cell.Site]) * strength)));

    // Merging is a SEED reduction, never a ring union: dropping a seed hands its region to the neighbours that
    // already bound it, so the survivors still tessellate a true Voronoi diagram and every ring keeps its clearance
    // meaning. One re-tessellation over the survivors closes the pass; a rule that empties the field faults typed.
    private static Fin<(Arr<SiteCell> Cells, Arr<SiteEdge> Adjacency)> Merged(
        (Arr<SiteCell> Cells, Arr<SiteEdge> Adjacency) dual,
        SiteMerge rule,
        Polyline ring,
        Loop boundary,
        Op op) {
        Arr<SiteCell> kept = dual.Cells.Fold(Arr<SiteCell>(), (survivors, cell) =>
            cell.Area < rule.MinimumArea
            || survivors.Exists(prior => prior.Centroid.DistanceTo(cell.Centroid) < rule.MinimumSeparation)
                ? survivors
                : survivors.Add(cell));
        return kept.Count == dual.Cells.Count
            ? Fin.Succ(dual)
            : kept.Count >= 3
                ? Dual(kept.Map(static cell => cell.Seed), ring, boundary, op)
                : Fin.Fail<(Arr<SiteCell>, Arr<SiteEdge>)>(
                    new GeometryFault.DegenerateInput(Kind.Point, None, "cells:merge-exhausted").ToError());
    }

    private static Fin<SpatialIndex> SeedIndex(Arr<SiteCell> cells, Op op) =>
        Spatial.Apply(
                new SpatialOp.Build(
                    SpatialKind.Bvh,
                    [.. cells.Map(static cell => new BoundingBox(cell.Seed, cell.Seed))],
                    BuildPolicy.Canonical),
                op)
            .Bind(answer => answer.Switch(
                state: op,
                index: static (_, admitted) => Fin.Succ(admitted.Value),
                result: static (key, _) => Fin.Fail<SpatialIndex>(key.InvalidResult(detail: "cells:non-index-answer")),
                wire: static (key, _) => Fin.Fail<SpatialIndex>(key.InvalidResult(detail: "cells:non-index-answer"))));

    // FillOf is the ONE FillRule seam the surviving Clipper2 lanes cross — kernel rows carry no engine ordinal, so a new
    // PolygonFill row is a compile-visible missing arm at this map alone.
    internal static FillRule FillOf(PolygonFill fill) => fill.Switch(
        nonZero: static () => FillRule.NonZero,
        evenOdd: static () => FillRule.EvenOdd,
        positive: static () => FillRule.Positive,
        negative: static () => FillRule.Negative);

    private static Fin<PolyTreeD> TreeOf(PathsD paths, PolygonFill fill, Context tolerance, Op op) {
        ClipperD engine = new(Precision(tolerance));
        PolyTreeD result = new();
        engine.AddSubject(paths);
        return engine.Execute(ClipType.Union, FillOf(fill), result)
            ? Fin.Succ(result)
            : Fin.Fail<PolyTreeD>(op.InvalidResult(detail: "clipper:tree-execute"));
    }

    private static Fin<TopologyReceipt> TopologyOf(PolyTreeD tree, Context tolerance, double plane, PolygonFill fill, Op op) =>
        Descendants(tree, None, Seq<(PolyPathD Node, Option<int> Parent)>.Empty)
            .Map(static (row, index) => (row.Node, row.Parent, Index: index))
            .TraverseM(row => op.Need(row.Node.Polygon)
                .Bind(path => FromPath(path, closed: true, tolerance, plane)
                    .Map(loop => new RegionNode(
                        row.Index,
                        row.Parent,
                        row.Node.Level - 1,
                        row.Node.IsHole,
                        loop,
                        Clipper.Area(path),
                        loop.Bound()))))
            .As()
            .Map(nodes => new TopologyReceipt(nodes, fill, tolerance, plane));

    // Pre-order emission assigns each child its ordinal before descending, so a parent index needs no reference re-scan.
    private static Seq<(PolyPathD Node, Option<int> Parent)> Descendants(
        PolyPathD node,
        Option<int> parent,
        Seq<(PolyPathD Node, Option<int> Parent)> emitted) =>
        toSeq(node.Cast<PolyPathD>()).Fold(
            emitted,
            (rows, child) => Descendants(child, Some(rows.Count), rows.Add((Node: child, Parent: parent))));

    private static Fin<(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside)> ClipRuns(
        Seq<Seq<Edge3>> subject,
        Seq<Loop> clip,
        PolygonFill fill,
        Op op) {
        ClipperD engine = new(Precision(clip[0].Tolerance));
        PathsD insideClosed = [];
        PathsD outsideClosed = [];
        PathsD inside = [];
        PathsD outside = [];
        engine.AddOpenSubject(ToOpenPaths(subject));
        engine.AddClip(ToPaths(clip));
        return engine.Execute(ClipType.Intersection, FillOf(fill), insideClosed, inside)
            && engine.Execute(ClipType.Difference, FillOf(fill), outsideClosed, outside)
            && insideClosed.Count == 0
            && outsideClosed.Count == 0
                ? Fin.Succ((Runs(inside, clip[0].Plane), Runs(outside, clip[0].Plane)))
                : Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(op.InvalidResult(detail: "clipper:open-partition"));
    }

    private static PolygonMeasure MeasureOf(TopologyReceipt topology) {
        Seq<Loop> paths = topology.Nodes.Map(static node => node.Boundary);
        PathsD native = ToPaths(paths);
        RectD bounds = Clipper.GetBounds(native);
        double signedArea = topology.Nodes.Fold(0.0, static (area, node) => area + node.SignedArea);
        double filledArea = topology.Nodes.Fold(
            0.0,
            static (area, node) => area + (node.IsHole ? -Math.Abs(node.SignedArea) : Math.Abs(node.SignedArea)));
        Point3d centroid = CentroidOf(paths, signedArea);
        return new PolygonMeasure(
            signedArea,
            filledArea,
            paths.Fold(0.0, static (length, path) => length + path.Length()),
            centroid,
            new BoundingBox(
                new Point3d(bounds.left, bounds.top, paths[0].Plane),
                new Point3d(bounds.right, bounds.bottom, paths[0].Plane)),
            topology.Nodes.Count(static node => !node.IsHole),
            topology.Nodes.Count(static node => node.IsHole));
    }

    private static Point3d CentroidOf(Seq<Loop> paths, double signedArea) {
        (double X, double Y, double Cross) moment = paths
            .Bind(path => Range(0, path.Spans).ToSeq().Map(index => (A: path.At(index), B: path.At(index + 1))).ToSeq())
            .Fold(
                (X: 0.0, Y: 0.0, Cross: 0.0),
                static (state, edge) => {
                    double cross = edge.A.X * edge.B.Y - edge.B.X * edge.A.Y;
                    return (state.X + (edge.A.X + edge.B.X) * cross, state.Y + (edge.A.Y + edge.B.Y) * cross, state.Cross + cross);
                });
        return Math.Abs(signedArea) > paths[0].Tolerance.Absolute.Value * paths[0].Tolerance.Absolute.Value
            ? new Point3d(moment.X / (3.0 * moment.Cross), moment.Y / (3.0 * moment.Cross), paths[0].Plane)
            : paths[0].Bound().Center;
    }

    private static Fin<PolygonTrace> PointsOf(Seq<Loop> paths, Arr<Point3d> points, PolygonFill fill) =>
        Defect(
                points.ToSeq(),
                point => !point.IsValid || Math.Abs(point.Z - paths[0].Plane) > paths[0].Tolerance.Absolute.Value,
                Kind.Point,
                "contains:off-plane")
            .As()
            .ToFin()
            .Map(_ => (PolygonTrace)new PolygonTrace.Related(
                points.Map(point => RelationOf(new PointD(point.X, point.Y), ToPaths(paths), fill, Precision(paths[0].Tolerance)))));

    private static PointRelation RelationOf(PointD point, PathsD paths, PolygonFill fill, int precision) {
        Seq<PointInPolygonResult> relations = toSeq(paths).Map(path => Clipper.PointInPolygon(point, path, precision));
        if (relations.Exists(static relation => relation == PointInPolygonResult.IsOn)) return PointRelation.Boundary;
        (int Crossings, int Winding) coverage = relations.Zip(toSeq(paths), static (relation, path) =>
                relation == PointInPolygonResult.IsInside
                    ? (Crossings: 1, Winding: Clipper.IsPositive(path) ? 1 : -1)
                    : (Crossings: 0, Winding: 0))
            .Fold((Crossings: 0, Winding: 0), static (state, item) =>
                (state.Crossings + item.Crossings, state.Winding + item.Winding));
        bool inside = fill.Switch(
            state: coverage,
            evenOdd: static value => value.Crossings % 2 == 1,
            nonZero: static value => value.Winding != 0,
            positive: static value => value.Winding > 0,
            negative: static value => value.Winding < 0);
        return inside ? PointRelation.Inside : PointRelation.Outside;
    }

    private static Fin<FieldReceipt> Raster(
        RasterKernel kernel,
        double[,] values,
        CellLattice grid,
        FieldMetric metric,
        Context tolerance,
        double plane) {
        ParallelHelper.For2D(0, values.GetLength(0), 0, values.GetLength(1), in kernel);
        ReadOnlySpan<double> samples = MemoryMarshal.CreateReadOnlySpan(ref values[0, 0], values.Length);
        return TensorPrimitives.IsFiniteAll(samples)
            ? Fin.Succ(new FieldReceipt(
                values.AsMemory2D(),
                grid,
                metric,
                tolerance,
                plane,
                TensorPrimitives.Min(samples),
                TensorPrimitives.Max(samples),
                TensorPrimitives.Average(samples),
                TensorPrimitives.StdDev(samples),
                CellOf(grid, TensorPrimitives.IndexOfMin(samples)),
                CellOf(grid, TensorPrimitives.IndexOfMax(samples))))
            : Fin.Fail<FieldReceipt>(new GeometryFault.DegenerateInput(Kind.Plane, None, "raster:non-finite-cell").ToError());
    }

    private static Point3d CellOf(CellLattice grid, int index) => grid.Center(column: index % grid.Columns.Value, row: index / grid.Columns.Value);

    // Clearance is the KERNEL field, swept once by the kernel's own lattice entry: one DistanceCase per admitted ring
    // over the ONE proximity handle, unioned under the hard blend so the combination IS an exact minimum. The
    // per-cell nearest-segment fold this owner once carried — and the metric's clearance-need gate that existed only
    // to skip it — delete whole: every metric now projects the same plane, so there is nothing left to gate.
    private static Fin<Arr<double>> Clearance(Seq<Loop> paths, CellLattice grid, Op op) =>
        from sources in paths.TraverseM(path => SupportSpace.Of(ToPolyline(path).ToPolylineCurve(), op)).As()
        from head in sources.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "raster:no-source").ToError())
        let field = sources.Tail.Fold(
            (ScalarField)new ScalarField.DistanceCase(head, BoundarySense.Toward),
            static (blended, source) => (ScalarField)new ScalarField.CsgCase(
                blended, new ScalarField.DistanceCase(source, BoundarySense.Toward), CsgKind.Union, BlendKind.Hard))
        from plane in field.SampleLattice(grid, paths[0].Tolerance, op)
        select plane;

    // Kernel planes stay UNSIGNED — a ring set carries no interior until a fill rule names one — so the sign is this
    // owner's fill classification at the same cell centre, a boundary cell reading exactly zero.
    private readonly struct RasterKernel(
        double[,] values,
        Arr<double> clearance,
        PathsD paths,
        PolygonFill fill,
        CellLattice grid,
        int precision,
        FieldMetric metric) : IAction2D {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int row, int column) {
            PointRelation relation = RelationOf(ToPoint(grid.Center(column: column, row: row)), paths, fill, precision);
            double magnitude = clearance[(int)grid.Linear(column: column, row: row, layer: 0)];
            values[row, column] = metric.Sample(
                relation == PointRelation.Boundary ? 0.0 : relation == PointRelation.Inside ? -magnitude : magnitude);
        }
    }

    private static Fin<Loop> StripDuplicates(Loop path) {
        double scale = Scale(path.Tolerance);
        Path64 stripped = Clipper.StripDuplicates(Clipper.ScalePath64(ToPath(path), scale), path.Closed);
        return FromPath(Clipper.ScalePathD(stripped, 1.0 / scale), path.Closed, path.Tolerance, path.Plane);
    }

    private static int Precision(Context tolerance) =>
        int.Clamp((int)Math.Ceiling(-Math.Log10(tolerance.Absolute.Value)), -8, 8);

    internal static double Scale(Context tolerance) => Math.Pow(10.0, Precision(tolerance));

    private static RectD RectOf(BoundingBox bounds) => new(bounds.Min.X, bounds.Min.Y, bounds.Max.X, bounds.Max.Y);

    internal static Paths64 ToPaths64(Seq<Loop> paths, double scale) => Clipper.ScalePaths64(ToPaths(paths), scale);

    private static PathsD ToPaths(Seq<Loop> paths) => new(paths.Map(ToPath));

    private static PathsD ToOpenPaths(Seq<Seq<Edge3>> paths) => new(paths.Map(path =>
        new PathD(path.Map(edge => ToPoint(edge.A)).Concat(Seq(ToPoint(path.Last.B))))));

    private static PathD ToPath(Loop path) => new(path.Vertices.Map(ToPoint));

    private static PointD ToPoint(Point3d point) => new(point.X, point.Y);

    private static Fin<Seq<Loop>> FromPaths(PathsD paths, bool closed, Context tolerance, double plane) =>
        toSeq(paths).TraverseM(path => FromPath(path, closed, tolerance, plane)).As();

    private static Fin<Loop> FromPath(PathD path, bool closed, Context tolerance, double plane) =>
        Loop.Admit(
            toSeq(path).Map(point => new Point3d(point.x, point.y, plane)).ToArr(),
            closed,
            [],
            tolerance);

    private static Seq<Seq<Edge3>> Runs(PathsD paths, double plane) =>
        toSeq(paths).Filter(static path => path.Count >= 2).Map(path => Range(0, path.Count - 1).ToSeq()
            .Map(index => new Edge3(
                new Point3d(path[index].x, path[index].y, plane),
                new Point3d(path[index + 1].x, path[index + 1].y, plane)))
            .ToSeq());
}
```

## [03]-[FIELD_PLANE]

- Owner: the kernel `CellLattice` carries the sampled window whole — extent, census, cell-center addressing, and the budget ceiling are `CellLattice.Of`'s, each caller passing its own ceiling value; `FieldMetric` owns each cell's scalar interpretation.
- Cases: occupancy derives fill membership; signed clearance derives boundary distance and sign; engagement derives cutter-radius overlap; reachability derives cutter-center admissibility at a tool radius; inscribed diameter doubles the nearest-boundary radius at interior cells — every row a projection of the one signed plane.
- Law: distance is the kernel's and the interior is this owner's — `ScalarField.DistanceCase` over `SupportSpace` measures each admitted ring, `CsgKind.Union` under `BlendKind.Hard` makes the ring fold an exact minimum, and the request's `PolygonFill` classification at the same cell centre supplies the sign a ring set cannot carry alone.
- Entry: `PolygonOp.Raster` consumes one admitted region set, fill rule, grid, and metric through `PolygonAlgebra.Apply`.
- Auto: `ScalarField.SampleLattice` sweeps the unsigned clearance plane in one kernel pass; `ParallelHelper.For2D` partitions the signing and metric projection over a package-required `IAction2D` kernel; `Memory2D<double>` materializes the plane, `ReadOnlyMemory2D<double>` publishes it, and `TensorPrimitives.IsFiniteAll`, `Min`, `Max`, `Average`, `StdDev`, `IndexOfMin`, and `IndexOfMax` derive the receipt statistics.
- Receipt: `FieldReceipt` keeps the plane, grid, metric, finite extrema, dispersion, and the model-space cells holding those extrema together, so engagement, additive masks, and layer audits consume one substrate value; `MinimumAt` over a signed-clearance plane is the deepest interior cell, the largest inscribed disc a cutter can occupy.
- Packages: `Rasm` (project) supplies `CellLattice`, `ScalarField`/`SupportSpace`/`CsgKind`/`BlendKind`, and `BoundarySense`; `Clipper2` supplies the fill classification; `CommunityToolkit.HighPerformance` supplies the cell partition and the 2D memory carrier; `System.Numerics.Tensors` supplies the receipt statistics; `LanguageExt` and `Thinktecture` supply the rail and the generated metric family.
- Growth: a new field interpretation is one `FieldMetric` case over the same signed plane with its admission row; a row needing a second sampling strategy belongs to the kernel field algebra, not here.
- Boundary: field storage remains owned by the receipt, provider paths remain private, and a non-finite cell fails the whole projection. Page-local distance loops are the deleted form; the clearance plane is the kernel's by law.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
