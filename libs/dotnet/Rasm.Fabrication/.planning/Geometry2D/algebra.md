# [RASM_FABRICATION_ALGEBRA]

`PolygonAlgebra` owns line-space fabrication geometry over `Clipper2`: one operation family admits line-only planar material, returns topology for region results and grouping for open runs, executes offset, Boolean, hygiene, morphology, inspection, and field projection, then emits one evidence-bearing result family. `Loop`, `Edge3`, and `Context` remain the boundary atoms, and `ArcAlgebra.Densify` remains the only bulge-to-line bridge.

`PolygonAlgebra.Apply` mirrors `Parametric.Apply`: one request, one `Op?` resolved through `OrDefault()`, and one `Fin<PolygonTrace>` result. Each arm names its case for provenance. Malformed policy routes `key.InvalidInput()`, degenerate geometry routes `GeometryFault.DegenerateInput` under the true `Kind` with the element ordinal where one exists, and provider throws route `key.InvalidResult(detail)`. Requests carry policy values, foreign carriers terminate inside the owner, and results re-enter at the admitted context and elevation.

## [01]-[INDEX]

- [02]-[OPERATION_ALGEBRA]: `PolygonOp`, its policy families, one `Apply` dispatch, the `PolygonScan` reusable-subject handle, the `FillProbe` classification probe, `EdgeSeparation`, and the typed `PolygonTrace` egress with its total projection family.
- [03]-[FIELD_PLANE]: `FieldMetric` and `FieldPlane` project occupancy, signed clearance, cutter engagement, cutter reachability, and local inscribed diameter over the kernel `CellLattice` into one finite-gated plane result.

## [02]-[OPERATION_ALGEBRA]

- Owner: `PolygonOp` is the complete request family and `PolygonAlgebra.Apply(PolygonOp?, Op?)` its only public execution surface; `PolygonScan` owns the reusable-subject placement handle and its lifetime; `FillProbe` owns the ONE fill classification every point relation reads, accumulating the signed winding and classifying it through the kernel `PolygonFill` row's own `Inside` delegate rather than re-spelling the four rules; `EdgeSeparation` extends the `Edge3` atom with the pure pair grammar — `Crosses(Edge3, double)` the tolerance-signed crossing verdict and `Gap(Edge3, double)` the exact segment-pair separation and `Gap(Point3d)` its point modality — zero on a crossing, else the least of the four clamped endpoint projections.
- Cases: `PolygonOp` and its input-shape policies jointly discriminate uniform and per-vertex offset, closed and open clipping, four hygiene algorithms, morphology, measurement, containment, topology, point-site cells, raster projection, and the minimum-area oriented rectangle. Boolean, fill, join, and end vocabularies are the kernel `Rasm.Meshing` rows — a fabrication request is already a kernel request.
- Law: admission owns its vocabulary — `HygieneRule.Admit` and `FieldMetric.Admit` gate their own scalars on the case that declares them, so no arm dispatches the same discriminant twice, and `Op.Need`/`Finite`/`Positive` carry presence and scalar gates rather than page-local re-derivations. `EdgeSeparation` is deliberately NON-monadic so hot per-pair censuses fold it directly; a `Fin` result per probe is the rejected shape, and the kernel `IntersectOp.SegmentSegment` stays the exact-predicate crossing-POINT owner this surface never re-derives.
- Law: `PolygonTrace` publishes its own total projections — `Regioned`, `Loops`, `Runs`, `Measure`, `Relations`, `Diagram`, `Envelope` — and every consumer takes its read from one of them, because a caller's `is PolygonTrace.Regions` test answers false on a widened family and lets the miss read as a legitimate verdict, where the generated total dispatch breaks at compile time. `Loops` is the one two-arm projection, collapsing a region forest to its boundary rings for a caller that accepts either shape and dropping `Depth`/`Parent`/`IsHole` when it does; the `Field` case publishes none, because a sampled plane has no consumer outside the raster arm that mints it. `LoopDemand` is the matching law for INPUT: closure and emptiness are one declared row, so no admission takes a boolean parameter at the position that decides what its own fault says.
- Exemption: `Hull`, `Envelope`, `Advance`, and `FillProbe.Relation` are named statement kernels — a monotone hull chain, an advancing-pointer caliper sweep, and an allocation-free per-point fill classification are measured byte-and-branch bodies whose expression forms allocate per candidate.
- Entry: `OffsetField` survives on scalar-versus-matrix arity and `HygieneRule` on algorithm payload timing; each case carries only the evidence its arm consumes. `PolygonScan.Scan` is the bracketed entry that owns the handle's whole lifetime; `PolygonScan.Of` hands the raw handle only to a caller whose subject outlives one fold, and that caller owns the bracket.
- Auto: offset, boolean, morphology, and cells lower onto the kernel owners — `Offsetting.Apply` wavefront offsets (per-edge reaches riding `OffsetReach.PerEdge`), `Arrangement.Apply` `PlanarOverlay` exact ring booleans, `OffsetOp.Minkowski` morphology with `MorphologyKind.ReflectPattern` as the lowering law, and `Tessellation.Build` + `VoronoiDual(boundary)` bounded point-site cells beside the unbounded `VoronoiDual(op)` the adjacency read folds; `ClipperD` owns the two shapes no owner below publishes — the region-nesting forest and the open-run partition — while area, extent, and orientation read the `Loop` atom's own polyline and the collinear and duplicate hygiene rules read that same built view through `RemoveRedundant`/`RemoveRepeatPos`; only error-bounded decimation and the three-valued point classification still cross to the `Clipper` statics, each carrying its own measured KERNEL-EXEMPTION, all behind the one `FillOf` interface; a placement scan hoists its recurring subject set into one `ReuseableDataContainer64` and folds it per position through `AddReuseableData`, `Rect64.Intersects` rejecting a disjoint candidate before the overlay runs; one `Try` boundary lowers package exceptions onto `Fin<T>`.
- Result: `PolygonTrace` distinguishes flat paths, region forests, split runs, measures, point relations, cell fields, sampled planes, and minimum-area oriented rectangles by evidence timing; `RegionNode.Parent` carries the pre-order ordinal the tree walk assigns, never a re-scanned reference match. `CellDiagram.Seeds` is the emitted diagram's own nearest-seed index and the owner of `Locate`, so a scan field pays one build rather than one ring walk per probe, and `CellDiagram.Adjacency` is what a merge rule reads to find a cell's true neighbours.
- Packages: `Rasm` (project) supplies the boolean/fill/join/end/reach vocabularies, the `Offsetting`/`Arrangement`/`Tessellation` owners, and the `CellLattice` plane; `Clipper2` supplies the region-nesting forest, the open-run partition, error-bounded decimation, the point classification, and the reusable scan container; `CavalierContours` supplies collinear and duplicate reduction through the polyline every `Loop` already holds; `Thinktecture` supplies generated owners and exhaustive dispatch; `LanguageExt` supplies admission, traversal, immutable carriers, and the exception channel; `Rasm.Domain` supplies the `Op` key carrier and the `Kind` fault taxonomy.
- Growth: a new operation is one `PolygonOp` case, one `PolygonTrace` case when its evidence differs, and one generated dispatch arm naming its own `Op`.
- Boundary: `ClipperD` and the point relation are the statement-bearing native kernels, and region MEASUREMENT is not among them — area, length, extent, and orientation are the `Loop` atom's own reads, so a second engine can never disagree with the loops the result publishes; kernel-lowered arms terminate their `Chain` results back into `Loop` at the admitted context and elevation. Cells are Voronoi by definition, so a foreign bounded Fortune tessellator — with the third forked draw stream it carried — is the deleted form; relaxation and merge are folds over the kernel dual, never provider modes. Inputs share one `Context` and elevation before XY projection; bulges, mixed contexts, mixed elevations, invalid open edges, and closure-policy conflicts fail before execution, each naming the index of the first offending path.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CavalierContours.Polyline;
using Clipper2Lib;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [TYPES] ---------------------------------------------------------------------------

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

[SmartEnum<string>]
internal sealed partial class LoopDemand {
    public static readonly LoopDemand Any = new("any", closes: false, admitsEmpty: false);
    public static readonly LoopDemand Closed = new("closed", closes: true, admitsEmpty: false);
    public static readonly LoopDemand Operand = new("operand", closes: true, admitsEmpty: true);

    internal bool Closes { get; }
    internal bool AdmitsEmpty { get; }
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

[ComplexValueObject]
public sealed partial class SiteMerge {
    public double MinimumArea { get; }
    public double MinimumSeparation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double minimumArea, ref double minimumSeparation) {
        if (!ValidityClaim.All(
            ValidityClaim.Nonnegative(minimumArea),
            ValidityClaim.Nonnegative(minimumSeparation)))
            validationError = new ValidationError("site-merge");
    }
}

[ComplexValueObject]
public sealed partial class SitePolicy {
    public int Relaxations { get; }
    public double RelaxationStrength { get; }
    public Option<SiteMerge> Merge { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref int relaxations, ref double relaxationStrength, ref Option<SiteMerge> merge) {
        if (!ValidityClaim.All(
            ValidityClaim.CountAtLeast(relaxations, 0),
            ValidityClaim.UnitInterval(relaxationStrength)))
            validationError = new ValidationError("site-policy");
    }

    public static SitePolicy Canonical { get; } = Create(relaxations: 0, relaxationStrength: 0.0, merge: None);
}

[Union]
public abstract partial record PolygonOp {
    public sealed record Offset(Seq<Loop> Paths, OffsetField Field, JoinType Join, EndType End, OffsetPolicy Policy) : PolygonOp;
    public sealed record Boolean(Seq<Loop> Subject, Seq<Loop> Clip, BooleanOp Kind, PolygonFill Fill) : PolygonOp;
    public sealed record ClipOpen(Seq<Seq<Edge3>> Subject, Seq<Loop> Clip, PolygonFill Fill) : PolygonOp;
    public sealed record Hygiene(Seq<Loop> Paths, HygieneRule Rule) : PolygonOp;
    public sealed record Morphology(Loop Pattern, Loop Path, MorphologyKind Kind) : PolygonOp;
    public sealed record Measure(Seq<Loop> Paths, PolygonFill Fill) : PolygonOp;
    public sealed record Contains(Seq<Loop> Paths, Arr<Point3d> Points, PolygonFill Fill) : PolygonOp;
    public sealed record Topology(Seq<Loop> Paths, PolygonFill Fill) : PolygonOp;
    public sealed record Cells(Arr<Point3d> Sites, Loop Boundary, SitePolicy Policy) : PolygonOp;
    public sealed record Raster(Seq<Loop> Paths, PolygonFill Fill, CellLattice Grid, FieldMetric Metric) : PolygonOp;
    public sealed record Calipers(Seq<Loop> Paths) : PolygonOp;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RegionNode(
    int Index,
    Option<int> Parent,
    int Depth,
    bool IsHole,
    Loop Boundary,
    double SignedArea,
    BoundingBox Bounds);

public sealed record RegionTopology(Seq<RegionNode> Nodes, PolygonFill Fill, Context Tolerance, double Plane);

public sealed record PolygonMeasure(
    double SignedArea,
    double FilledArea,
    double BoundaryLength,
    Point3d Centroid,
    BoundingBox Bounds,
    int Outers,
    int Holes);

public sealed record SiteCell(int Site, Point3d Seed, Loop Ring, Point3d Centroid, double Area);

public sealed record SiteEdge(int A, int B, Point3d Start, Point3d End) {
    public double Length => Start.DistanceTo(End);
    public Point3d Mid => new((Start.X + End.X) * 0.5, (Start.Y + End.Y) * 0.5, Start.Z);
}

public sealed record OrientedEnvelope(Point3d Anchor, Vector3d Along, double Length, double Width) {
    public double Area => Length * Width;
    public double Aspect => Length <= 0.0 || Width <= 0.0 ? 0.0 : Math.Min(Length, Width) / Math.Max(Length, Width);
}

public sealed record CellDiagram(
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

[Union]
public abstract partial record PolygonTrace {
    public sealed record Paths(Seq<Loop> Result) : PolygonTrace;
    public sealed record Regions(RegionTopology Result) : PolygonTrace;
    public sealed record SplitRuns(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside) : PolygonTrace;
    public sealed record Measured(PolygonMeasure Result) : PolygonTrace;
    public sealed record Related(Arr<PointRelation> Result) : PolygonTrace;
    public sealed record Celled(CellDiagram Result) : PolygonTrace;
    public sealed record Field(SampledField Result) : PolygonTrace;
    public sealed record Enveloped(OrientedEnvelope Result) : PolygonTrace;

    public Fin<RegionTopology> Regioned(Error refusal) => Switch(
        state: refusal,
        paths: static (fault, _) => Fin.Fail<RegionTopology>(fault),
        regions: static (_, value) => Fin.Succ(value.Result),
        splitRuns: static (fault, _) => Fin.Fail<RegionTopology>(fault),
        measured: static (fault, _) => Fin.Fail<RegionTopology>(fault),
        related: static (fault, _) => Fin.Fail<RegionTopology>(fault),
        celled: static (fault, _) => Fin.Fail<RegionTopology>(fault),
        field: static (fault, _) => Fin.Fail<RegionTopology>(fault),
        enveloped: static (fault, _) => Fin.Fail<RegionTopology>(fault));

    public Fin<Seq<Loop>> Loops(Error refusal) => Switch(
        state: refusal,
        paths: static (_, value) => Fin.Succ(value.Result),
        regions: static (_, value) => Fin.Succ(value.Result.Nodes.Map(static node => node.Boundary)),
        splitRuns: static (fault, _) => Fin.Fail<Seq<Loop>>(fault),
        measured: static (fault, _) => Fin.Fail<Seq<Loop>>(fault),
        related: static (fault, _) => Fin.Fail<Seq<Loop>>(fault),
        celled: static (fault, _) => Fin.Fail<Seq<Loop>>(fault),
        field: static (fault, _) => Fin.Fail<Seq<Loop>>(fault),
        enveloped: static (fault, _) => Fin.Fail<Seq<Loop>>(fault));

    public Fin<(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside)> Runs(Error refusal) => Switch(
        state: refusal,
        paths: static (fault, _) => Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(fault),
        regions: static (fault, _) => Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(fault),
        splitRuns: static (_, value) => Fin.Succ((value.Inside, value.Outside)),
        measured: static (fault, _) => Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(fault),
        related: static (fault, _) => Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(fault),
        celled: static (fault, _) => Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(fault),
        field: static (fault, _) => Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(fault),
        enveloped: static (fault, _) => Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(fault));

    public Fin<PolygonMeasure> Measure(Error refusal) => Switch(
        state: refusal,
        paths: static (fault, _) => Fin.Fail<PolygonMeasure>(fault),
        regions: static (fault, _) => Fin.Fail<PolygonMeasure>(fault),
        splitRuns: static (fault, _) => Fin.Fail<PolygonMeasure>(fault),
        measured: static (_, value) => Fin.Succ(value.Result),
        related: static (fault, _) => Fin.Fail<PolygonMeasure>(fault),
        celled: static (fault, _) => Fin.Fail<PolygonMeasure>(fault),
        field: static (fault, _) => Fin.Fail<PolygonMeasure>(fault),
        enveloped: static (fault, _) => Fin.Fail<PolygonMeasure>(fault));

    public Fin<Arr<PointRelation>> Relations(Error refusal) => Switch(
        state: refusal,
        paths: static (fault, _) => Fin.Fail<Arr<PointRelation>>(fault),
        regions: static (fault, _) => Fin.Fail<Arr<PointRelation>>(fault),
        splitRuns: static (fault, _) => Fin.Fail<Arr<PointRelation>>(fault),
        measured: static (fault, _) => Fin.Fail<Arr<PointRelation>>(fault),
        related: static (_, value) => Fin.Succ(value.Result),
        celled: static (fault, _) => Fin.Fail<Arr<PointRelation>>(fault),
        field: static (fault, _) => Fin.Fail<Arr<PointRelation>>(fault),
        enveloped: static (fault, _) => Fin.Fail<Arr<PointRelation>>(fault));

    public Fin<CellDiagram> Diagram(Error refusal) => Switch(
        state: refusal,
        paths: static (fault, _) => Fin.Fail<CellDiagram>(fault),
        regions: static (fault, _) => Fin.Fail<CellDiagram>(fault),
        splitRuns: static (fault, _) => Fin.Fail<CellDiagram>(fault),
        measured: static (fault, _) => Fin.Fail<CellDiagram>(fault),
        related: static (fault, _) => Fin.Fail<CellDiagram>(fault),
        celled: static (_, value) => Fin.Succ(value.Result),
        field: static (fault, _) => Fin.Fail<CellDiagram>(fault),
        enveloped: static (fault, _) => Fin.Fail<CellDiagram>(fault));

    public Fin<OrientedEnvelope> Envelope(Error refusal) => Switch(
        state: refusal,
        paths: static (fault, _) => Fin.Fail<OrientedEnvelope>(fault),
        regions: static (fault, _) => Fin.Fail<OrientedEnvelope>(fault),
        splitRuns: static (fault, _) => Fin.Fail<OrientedEnvelope>(fault),
        measured: static (fault, _) => Fin.Fail<OrientedEnvelope>(fault),
        related: static (fault, _) => Fin.Fail<OrientedEnvelope>(fault),
        celled: static (fault, _) => Fin.Fail<OrientedEnvelope>(fault),
        field: static (fault, _) => Fin.Fail<OrientedEnvelope>(fault),
        enveloped: static (_, value) => Fin.Succ(value.Result));
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
internal readonly struct FillProbe {
    private readonly PathD[] paths;
    private readonly int[] windings;
    private readonly PolygonFill fill;
    private readonly int precision;

    public FillProbe(Seq<Loop> paths, PolygonFill fill, int precision) {
        this.paths = [.. PolygonAlgebra.ToPaths(paths)];
        this.windings = [.. paths.Map(static path => path.Winding() == Sign.Positive ? 1 : -1)];
        this.fill = fill;
        this.precision = precision;
    }

    public PointRelation Relation(PointD point) {
        int winding = 0;
        for (int index = 0; index < paths.Length; index++) {
            switch (Clipper.PointInPolygon(point, paths[index], precision)) {
                case PointInPolygonResult.IsOn:
                    return PointRelation.Boundary;
                case PointInPolygonResult.IsInside:
                    winding += windings[index];
                    break;
            }
        }
        return fill.Inside(winding) ? PointRelation.Inside : PointRelation.Outside;
    }
}

public sealed class PolygonScan : IDisposable {
    private readonly ReuseableDataContainer64 subject;
    private readonly Rect64 bounds;
    private readonly double scale;
    private readonly FillRule fill;

    private PolygonScan(ReuseableDataContainer64 subject, Rect64 bounds, double scale, FillRule fill) {
        this.subject = subject; this.bounds = bounds; this.scale = scale; this.fill = fill;
    }

    public static Fin<T> Scan<T>(Seq<Loop> paths, PolygonFill fill, Func<PolygonScan, Fin<T>> fold, Op? key = null) =>
        Of(paths, fill, key).Bind(scan => {
            using (scan) { return fold(scan); }
        });

    public static Fin<PolygonScan> Of(Seq<Loop> paths, PolygonFill fill, Op? key = null) =>
        from admitted in PolygonAlgebra.Regions(paths)
        from resolved in key.OrDefault().Need(fill)
        from scan in key.OrDefault().Catch(() => {
            double scale = PolygonAlgebra.Scale(admitted[0].Tolerance);
            Paths64 native = PolygonAlgebra.ToPaths64(admitted, scale);
            ReuseableDataContainer64 container = new();
            container.AddPaths(native, PathType.Subject, isOpen: false);
            return Fin.Succ(new PolygonScan(container, Clipper.GetBounds(native), scale, PolygonAlgebra.FillOf(resolved)));
        })
        select scan;

    public Fin<bool> Intersects(Seq<Loop> candidate, Op? key = null) =>
        from admitted in PolygonAlgebra.Regions(candidate)
        from verdict in key.OrDefault().Catch(() => {
            Paths64 native = PolygonAlgebra.ToPaths64(admitted, scale);
            if (!bounds.Intersects(Clipper.GetBounds(native))) { return Fin.Succ(false); }
            Clipper64 engine = new();
            engine.AddReuseableData(subject);
            engine.AddClip(native);
            Paths64 result = [];
            return Fin.Succ(engine.Execute(ClipType.Intersection, fill, result) && result.Count > 0);
        })
        select verdict;

    public void Dispose() => subject.Clear();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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
        from result in resolved.Catch(() => admitted.Switch(
                state: resolved,
                offset: static (op, request) => OffsetOf(request, op),
                boolean: static (op, request) => BooleanOf(request, op),
                clipOpen: static (op, request) => OpenClipOf(request, op),
                hygiene: static (op, request) => HygieneOf(request, op),
                morphology: static (op, request) => MorphologyOf(request, op),
                measure: static (op, request) => MeasureOf(request, op),
                contains: static (op, request) => ContainsOf(request, op),
                topology: static (op, request) => TopologyOf(request, op),
                cells: static (op, request) => CellsOf(request, op),
                raster: static (op, request) => RasterOf(request, op),
                calipers: static (op, request) => CalipersOf(request, op)))
        select result;

    private static Op Resolve(PolygonOp operation, Op? key) => key ?? operation.Switch(
        offset: static _ => Op.Of(name: nameof(PolygonOp.Offset)),
        boolean: static _ => Op.Of(name: nameof(PolygonOp.Boolean)),
        clipOpen: static _ => Op.Of(name: nameof(PolygonOp.ClipOpen)),
        hygiene: static _ => Op.Of(name: nameof(PolygonOp.Hygiene)),
        morphology: static _ => Op.Of(name: nameof(PolygonOp.Morphology)),
        measure: static _ => Op.Of(name: nameof(PolygonOp.Measure)),
        contains: static _ => Op.Of(name: nameof(PolygonOp.Contains)),
        topology: static _ => Op.Of(name: nameof(PolygonOp.Topology)),
        cells: static _ => Op.Of(name: nameof(PolygonOp.Cells)),
        raster: static _ => Op.Of(name: nameof(PolygonOp.Raster)),
        calipers: static _ => Op.Of(name: nameof(PolygonOp.Calipers)));

    private static Fin<PolygonTrace> OffsetOf(PolygonOp.Offset request, Op op) =>
        from paths in Admitted(request.Paths, LoopDemand.Any)
        from field in op.Need(request.Field)
        from policy in op.Need(request.Policy)
        from _ in OffsetClosure(paths, request.End)
        from results in field.Switch(
            state: (Paths: paths, Join: request.Join, End: request.End, Policy: policy, Op: op),
            uniform: static (state, admitted) =>
                from delta in state.Op.Finite(admitted.Distance)
                from runs in state.Paths.TraverseM(path => Offsetting.Apply(
                    new OffsetOp.Offset(ToPolyline(path), new OffsetReach.Uniform(delta), state.Join, state.End, state.Policy), state.Op)).As()
                select runs,
            variable: static (state, admitted) =>
                from sized in guard(
                        admitted.Distances.Count == state.Paths.Count
                        && admitted.Distances.ToSeq().Zip(state.Paths, static (row, path) => row.Count == path.Spans).ForAll(identity)
                        && TensorPrimitives.IsFiniteAll(admitted.Distances.Bind(static row => row).ToArray()),
                        state.Op.InvalidInput())
                    .ToFin()
                from runs in state.Paths.Zip(admitted.Distances.ToSeq()).TraverseM(item => Offsetting.Apply(
                    new OffsetOp.Offset(ToPolyline(item.First), new OffsetReach.PerEdge(item.Second),
                        state.Join, state.End, state.Policy), state.Op)).As()
                select runs)
        from chains in results.TraverseM(result => ChainsOf(result, op)).As()
        from loops in FromChains(chains.Bind(static c => c), paths[0].Tolerance, paths[0].Plane)
        from tree in TreeOf(ToPaths(loops), PolygonFill.NonZero, paths[0].Tolerance, op)
        from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, PolygonFill.NonZero, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> BooleanOf(PolygonOp.Boolean request, Op op) =>
        from subject in Regions(request.Subject)
        from clip in Admitted(request.Clip, LoopDemand.Operand)
        from kind in op.Need(request.Kind)
        from fill in op.Need(request.Fill)
        from operands in Admitted(subject.Concat(clip), LoopDemand.Any)
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

    private static Fin<PolygonTrace> HygieneOf(PolygonOp.Hygiene request, Op op) =>
        from paths in Admitted(request.Paths, LoopDemand.Any)
        from rule in op.Need(request.Rule)
        from _ in rule.Admit(op)
        from result in paths.TraverseM(path => rule.Switch(
            state: path,
            simplify: static (loop, admitted) => FromPath(
                Clipper.SimplifyPath(ToPath(loop), admitted.Epsilon, loop.Closed), loop.Closed, loop.Tolerance, loop.Plane),
            ramerDouglasPeucker: static (loop, admitted) => FromPath(
                Clipper.RamerDouglasPeucker(ToPath(loop), admitted.Epsilon), loop.Closed, loop.Tolerance, loop.Plane),
            collinear: static (loop, _) => ArcAlgebra.FromPline(
                loop.View.Pline.RemoveRedundant(loop.Tolerance.Absolute.Value) ?? loop.View.Pline,
                loop.Tolerance, loop.Plane),
            duplicates: static (loop, _) => ArcAlgebra.FromPline(
                loop.View.Pline.RemoveRepeatPos(loop.Tolerance.Absolute.Value) ?? loop.View.Pline,
                loop.Tolerance, loop.Plane))).As()
        select (PolygonTrace)new PolygonTrace.Paths(result);

    private static Fin<PolygonTrace> MorphologyOf(PolygonOp.Morphology request, Op op) =>
        from kind in op.Need(request.Kind)
        from operands in Admitted(Seq(request.Pattern, request.Path), LoopDemand.Any)
        from _ in guard(operands[0].Closed, new GeometryFault.DegenerateInput(Kind.Polyline, 0, "morphology:open-pattern")).ToFin()
        from pattern in kind.ReflectPattern ? Reflected(operands[0]) : Fin.Succ(operands[0])
        from convolved in Offsetting.Apply(
            new OffsetOp.Minkowski(ToPolyline(operands[1]), ToPolyline(pattern), OffsetPolicy.Of(Context.Canonical)), op)
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
        from _ in guard(!topology.Nodes.IsEmpty, new GeometryFault.DegenerateInput(Kind.Polyline, None, "measure:empty-fill")).ToFin()
        select (PolygonTrace)new PolygonTrace.Measured(MeasureOf(topology));

    private static Fin<PolygonTrace> CalipersOf(PolygonOp.Calipers request, Op op) =>
        from paths in Admitted(request.Paths, LoopDemand.Any)
        let hull = Hull(paths.Bind(static path => toSeq(path.Vertices)))
        from _ in guard(hull.Count >= 3, new GeometryFault.DegenerateInput(Kind.Polyline, None, "calipers:collinear")).ToFin()
        select (PolygonTrace)new PolygonTrace.Enveloped(Envelope(hull));

    private static Arr<Point3d> Hull(Seq<Point3d> points) {
        Arr<Point3d> sorted = toArray(points.Distinct().OrderBy(static point => point.X).ThenBy(static point => point.Y));
        if (sorted.Count < 3) { return sorted; }
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
        return toArray(chain.AsSpan(0, k - 1).ToArray());
    }

    private static double Turn(Point3d a, Point3d b, Point3d c) =>
        ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));

    private static OrientedEnvelope Envelope(Arr<Point3d> hull) {
        int count = hull.Count;
        int right = 1;
        int top = 1;
        int left = 1;
        Option<OrientedEnvelope> best = None;
        for (int edge = 0; edge < count; edge++) {
            Point3d origin = hull[edge];
            Point3d head = hull[(edge + 1) % count];
            double span = origin.DistanceTo(head);
            if (span <= 0.0) { continue; }
            double ux = (head.X - origin.X) / span;
            double uy = (head.Y - origin.Y) / span;
            right = Advance(hull, right, ux, uy, 1.0, Along);
            top = Advance(hull, top, ux, uy, 1.0, Across);
            left = Advance(hull, left, ux, uy, -1.0, Along);
            double a0 = Along(hull[left], ux, uy);
            double a1 = Along(hull[right], ux, uy);
            double c0 = Across(origin, ux, uy);
            double c1 = Across(hull[top], ux, uy);
            OrientedEnvelope candidate = new(
                Anchor: new Point3d((a0 * ux) - (c0 * uy), (a0 * uy) + (c0 * ux), origin.Z),
                Along: new Vector3d(ux, uy, 0.0),
                Length: a1 - a0,
                Width: c1 - c0);
            best = best.Filter(current => current.Area <= candidate.Area).IsSome ? best : Some(candidate);
        }
        return best.IfNone(() => new OrientedEnvelope(hull[0], Vector3d.XAxis, 0.0, 0.0));
    }

    private static int Advance(Arr<Point3d> hull, int at, double ux, double uy, double scale, Func<Point3d, double, double, double> objective) {
        int count = hull.Count;
        int next = at;
        for (int step = 0; step < count; step++) {
            int ahead = (next + 1) % count;
            if (scale * objective(hull[ahead], ux, uy) < scale * objective(hull[next], ux, uy)) { break; }
            next = ahead;
        }
        return next;
    }

    private static double Along(Point3d point, double ux, double uy) => (point.X * ux) + (point.Y * uy);

    private static double Across(Point3d point, double ux, double uy) => (point.Y * ux) - (point.X * uy);

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

    private static Fin<PolygonTrace> CellsOf(PolygonOp.Cells request, Op op) =>
        from bounds in Regions(Seq(request.Boundary))
        from policy in op.Need(request.Policy)
        from _ in Defect(
                request.Sites.ToSeq(),
                point => !ValidityClaim.Finite(point) || Math.Abs(point.Z - bounds[0].Plane) > bounds[0].Tolerance.Absolute.Value,
                Kind.Point,
                "cells:off-plane")
            .As()
            .ToFin()
        from __ in guard(
                request.Sites.Count >= 3,
                new GeometryFault.DegenerateInput(Kind.Point, None, "cells:site-floor"))
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
        select (PolygonTrace)new PolygonTrace.Celled(new CellDiagram(
            merged.Cells, merged.Adjacency, index, bounds[0], bounds[0].Tolerance, bounds[0].Plane));

    private static Fin<PolygonTrace> RasterOf(PolygonOp.Raster request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from grid in op.Need(request.Grid)
        from metric in op.Need(request.Metric)
        from result in FieldPlane.Project(paths, fill, grid, metric, op)
        select (PolygonTrace)new PolygonTrace.Field(result);

    // --- [BOUNDARIES] ------------------------------------------------------------------
    internal static K<Validation<Error>, Unit> Check(bool condition, Kind kind, Option<int> index, string witness) =>
        AdmissionSlots.Gate(condition, new GeometryFault.DegenerateInput(kind, index, witness));

    private static K<Validation<Error>, Unit> Defect<T>(Seq<T> values, Func<T, bool> offends, Kind kind, string witness) {
        Option<int> at = values
            .Map(static (value, index) => (Value: value, Index: index))
            .Filter(row => offends(row.Value))
            .Map(static row => row.Index)
            .Head;
        return Check(at.IsNone, kind, at, witness);
    }

    private static Fin<Seq<Loop>> Admitted(Seq<Loop> paths, LoopDemand demand) =>
        (Check(demand.AdmitsEmpty || !paths.IsEmpty, Kind.Polyline, None, "empty"),
         Defect(paths, static path => path is null, Kind.Polyline, "null"),
         Defect(paths, static path => path is not null && path.Bulges.Exists(static bulge => bulge != 0.0), Kind.Polyline, "bulged"),
         Defect(paths, path => path is not null && paths[0] is not null && path.Tolerance != paths[0].Tolerance, Kind.Polyline, "mixed-context"),
         Defect(paths, path => path is not null && paths[0] is not null
             && Math.Abs(path.Plane - paths[0].Plane) > path.Tolerance.Absolute.Value, Kind.Polyline, "mixed-elevation"),
         Defect(paths, path => path is not null && demand.Closes && !path.Closed, Kind.Polyline, "open"))
            .Apply(static (_, _, _, _, _, _) => paths)
            .As()
            .ToFin();

    internal static Fin<Seq<Loop>> Regions(Seq<Loop> paths) => Admitted(paths, LoopDemand.Closed);

    private static Fin<Seq<Seq<Edge3>>> Edges(Seq<Seq<Edge3>> paths, Context tolerance, double plane) =>
        (Check(double.IsFinite(plane), Kind.Plane, None, "non-finite-plane"),
         Check(!paths.IsEmpty, Kind.Line, None, "empty"),
         Defect(paths, static path => path.IsEmpty, Kind.Line, "empty-run"),
         Defect(paths, static path => path.Exists(static edge => !ValidityClaim.All(
             ValidityClaim.Finite(edge.A), ValidityClaim.Finite(edge.B))), Kind.Line, "non-finite"),
         Defect(paths, path => path.Exists(edge => edge.A.DistanceTo(edge.B) <= tolerance.Absolute.Value), Kind.Line, "zero-length"),
         Defect(paths, path => path.Exists(edge => Math.Abs(edge.A.Z - plane) > tolerance.Absolute.Value
             || Math.Abs(edge.B.Z - plane) > tolerance.Absolute.Value), Kind.Line, "off-plane"),
         Defect(paths, path => Range(0, Math.Max(0, path.Count - 1))
             .Exists(index => path[index].B.DistanceTo(path[index + 1].A) > tolerance.Absolute.Value), Kind.Line, "discontinuous"))
            .Apply(static (_, _, _, _, _, _, _) => paths)
            .As()
            .ToFin();

    private static Fin<Unit> OffsetClosure(Seq<Loop> paths, EndType end) =>
        Defect(paths, path => end == EndType.Closed ? !path.Closed : path.Closed, Kind.Polyline, "offset:closure-conflict")
            .As()
            .ToFin();

    private static Fin<Seq<Chain>> ChainsOf(OffsetResult result, Op op) => result.Switch(
        state: op,
        graph: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "offset:non-curve-result")),
        curves: static (_, loops) => Fin.Succ(loops),
        probe: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "offset:non-curve-result")));

    private static Fin<Seq<Chain>> OverlayChainsOf(ArrangementResult result, Op op) => result.Switch(
        state: op,
        boolean: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "boolean:non-overlay-result")),
        overlay: static (_, admitted) => Fin.Succ(admitted.Loops),
        complex: static (key, _) => Fin.Fail<Seq<Chain>>(key.InvalidResult(detail: "boolean:non-overlay-result")));

    internal static Polyline ToPolyline(Loop path) =>
        new(path.Closed ? path.Vertices.Add(path.Vertices[0]) : path.Vertices);

    private static Fin<Seq<Loop>> FromChains(Seq<Chain> chains, Context tolerance, double plane) =>
        chains.TraverseM(chain => {
            Arr<Point3d> points = toSeq(chain.Points).ToArr();
            Arr<Point3d> vertices = chain.Points.IsClosed && points.Count > 1
                && points[0].DistanceTo(points[points.Count - 1]) <= tolerance.Absolute.Value
                    ? points.RemoveAt(points.Count - 1)
                    : points;
            return Loop.Admit(vertices.Map(point => new Point3d(point.X, point.Y, plane)), chain.Points.IsClosed, [], tolerance);
        }).As();

    private static Fin<Loop> Reflected(Loop pattern) =>
        Loop.Admit(
            pattern.Vertices.Map(point => new Point3d(-point.X, -point.Y, point.Z)),
            pattern.Closed,
            [],
            pattern.Tolerance);

    // --- [SITE_CELLS]
    private static Fin<(Arr<SiteCell> Cells, Arr<SiteEdge> Adjacency)> Dual(
        Arr<Point3d> seeds,
        Polyline ring,
        Loop boundary,
        Op op) =>
        from tessellation in Tessellation.Build(
            new TessellationOp.Points(
                TessellationKind.Triangulation,
                [.. seeds.Map(static seed => (Implicit)seed)],
                Seq<Conform>(),
                TessellationPolicy.Canonical,
                Axis.Z))
        from bounded in tessellation.VoronoiDual(ring, op)
        from graph in tessellation.VoronoiDual()
        from cells in toSeq(bounded).TraverseM(cell => Loop.Admit(
                toSeq(cell.Ring).Map(point => new Point3d(point.X, point.Y, boundary.Plane)).ToArr(),
                closed: true,
                Arr<double>(),
                boundary.Tolerance)
            .Map(loop => new SiteCell(
                cell.Site, seeds[cell.Site], loop, CentroidOf(Seq(loop)), Math.Abs(loop.Area())))).As()
        let surviving = cells.Map(static cell => cell.Site).ToHashSet()
        select (cells.ToArr(), toArray(Range(0, graph.Across.Length).ToSeq()
            .Filter(i => surviving.Contains(graph.Across[i].U) && surviving.Contains(graph.Across[i].V))
            .Map(i => new SiteEdge(
                Math.Min(graph.Across[i].U, graph.Across[i].V),
                Math.Max(graph.Across[i].U, graph.Across[i].V),
                Elevate(graph.Circumcenters[graph.Edges[i].A], boundary.Plane),
                Elevate(graph.Circumcenters[graph.Edges[i].B], boundary.Plane)))
            .DistinctBy(static edge => (edge.A, edge.B))));

    private static Point3d Elevate(Point3d point, double plane) => new(point.X, point.Y, plane);

    private static Arr<Point3d> Moved(Arr<Point3d> seeds, Arr<SiteCell> cells, double strength) =>
        cells.Fold(seeds, (moved, cell) => moved.SetItem(
            cell.Site,
            moved[cell.Site] + ((cell.Centroid - moved[cell.Site]) * strength)));

    private static Fin<(Arr<SiteCell> Cells, Arr<SiteEdge> Adjacency)> Merged(
        (Arr<SiteCell> Cells, Arr<SiteEdge> Adjacency) dual,
        SiteMerge rule,
        Polyline ring,
        Loop boundary,
        Op op) {
        Map<int, SiteCell> bySite = toMap(dual.Cells.Map(static cell => (cell.Site, cell)));
        Map<int, Seq<int>> neighbours = dual.Adjacency.Fold(
            Map<int, Seq<int>>(),
            static (index, edge) => index
                .AddOrUpdate(edge.A, existing => existing.Add(edge.B), () => Seq(edge.B))
                .AddOrUpdate(edge.B, existing => existing.Add(edge.A), () => Seq(edge.A)));
        Set<int> kept = dual.Cells.Fold(Set<int>(), (survivors, cell) =>
            cell.Area < rule.MinimumArea
            || neighbours.Find(cell.Site).IfNone(Seq<int>()).Exists(site =>
                survivors.Contains(site)
                && bySite.Find(site).Map(prior => prior.Centroid.DistanceTo(cell.Centroid) < rule.MinimumSeparation).IfNone(false))
                ? survivors
                : survivors.Add(cell.Site));
        return kept.Count == dual.Cells.Count
            ? Fin.Succ(dual)
            : kept.Count >= 3
                ? Dual(dual.Cells.Filter(cell => kept.Contains(cell.Site)).Map(static cell => cell.Seed), ring, boundary, op)
                : Fin.Fail<(Arr<SiteCell>, Arr<SiteEdge>)>(
                    new GeometryFault.DegenerateInput(Kind.Point, None, "cells:merge-exhausted"));
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

    private static Fin<RegionTopology> TopologyOf(PolyTreeD tree, Context tolerance, double plane, PolygonFill fill, Op op) =>
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
                        loop.Area(),
                        loop.Bound()))))
            .As()
            .Map(nodes => new RegionTopology(nodes, fill, tolerance, plane));

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

    private static PolygonMeasure MeasureOf(RegionTopology topology) {
        Seq<Loop> paths = topology.Nodes.Map(static node => node.Boundary);
        return new PolygonMeasure(
            topology.Nodes.Fold(0.0, static (area, node) => area + node.SignedArea),
            topology.Nodes.Fold(
                0.0,
                static (area, node) => area + (node.IsHole ? -Math.Abs(node.SignedArea) : Math.Abs(node.SignedArea))),
            paths.Fold(0.0, static (length, path) => length + path.Length()),
            CentroidOf(paths),
            paths.Fold(BoundingBox.Empty, static (bounds, path) => BoundingBox.Union(bounds, path.Bound())),
            topology.Nodes.Count(static node => !node.IsHole),
            topology.Nodes.Count(static node => node.IsHole));
    }

    private static Point3d CentroidOf(Seq<Loop> paths) {
        (double X, double Y, double Cross) moment = paths
            .Bind(path => Range(0, path.Spans).ToSeq().Map(index => (A: path.At(index), B: path.At(index + 1))).ToSeq())
            .Fold(
                (X: 0.0, Y: 0.0, Cross: 0.0),
                static (state, edge) => {
                    double cross = (edge.A.X * edge.B.Y) - (edge.B.X * edge.A.Y);
                    return (state.X + ((edge.A.X + edge.B.X) * cross), state.Y + ((edge.A.Y + edge.B.Y) * cross), state.Cross + cross);
                });
        double floor = paths[0].Tolerance.Absolute.Value * paths[0].Tolerance.Absolute.Value;
        return Math.Abs(moment.Cross) > floor
            ? new Point3d(moment.X / (3.0 * moment.Cross), moment.Y / (3.0 * moment.Cross), paths[0].Plane)
            : paths[0].Bound().Center;
    }

    private static Fin<PolygonTrace> PointsOf(Seq<Loop> paths, Arr<Point3d> points, PolygonFill fill) =>
        Defect(
                points.ToSeq(),
                point => !ValidityClaim.Finite(point) || Math.Abs(point.Z - paths[0].Plane) > paths[0].Tolerance.Absolute.Value,
                Kind.Point,
                "contains:off-plane")
            .As()
            .ToFin()
            .Map(_ => {
                FillProbe probe = new(paths, fill, Precision(paths[0].Tolerance));
                return (PolygonTrace)new PolygonTrace.Related(points.Map(point => probe.Relation(ToPoint(point))));
            });

    internal static int Precision(Context tolerance) =>
        int.Clamp((int)Math.Ceiling(-Math.Log10(tolerance.Absolute.Value)), -8, 8);

    internal static double Scale(Context tolerance) => Math.Pow(10.0, Precision(tolerance));

    internal static Paths64 ToPaths64(Seq<Loop> paths, double scale) => Clipper.ScalePaths64(ToPaths(paths), scale);

    internal static PathsD ToPaths(Seq<Loop> paths) => new(paths.Map(ToPath));

    private static PathsD ToOpenPaths(Seq<Seq<Edge3>> paths) => new(paths.Map(path =>
        new PathD(path.Map(edge => ToPoint(edge.A)).Concat(Seq(ToPoint(path[^1].B))))));

    private static PathD ToPath(Loop path) => new(path.Vertices.Map(ToPoint));

    internal static PointD ToPoint(Point3d point) => new(point.X, point.Y);

    internal static Fin<Loop> FromPath(PathD path, bool closed, Context tolerance, double plane) =>
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

- Owner: `FieldMetric` owns each cell's scalar interpretation and its own admission; `FieldPlane` owns the projection — the clearance sweep, the fill signing, the parallel partition, and the result statistics; the kernel `CellLattice` carries the sampled window whole, so extent, census, cell-center addressing, and the budget ceiling are `CellLattice.Of`'s, each caller passing its own ceiling value.
- Cases: occupancy derives fill membership; signed clearance derives boundary distance and sign; engagement derives cutter-radius overlap; reachability derives cutter-center admissibility at a tool radius; inscribed diameter doubles the nearest-boundary radius at interior cells — every row a projection of the one signed plane.
- Law: distance is the kernel's and the interior is this owner's — `ScalarField.DistanceCase` over `SupportSpace` measures each admitted ring and the union of those planes IS their POINTWISE MINIMUM, so the fold is one vectorized pass per ring rather than an N-deep CSG walk re-entered at every cell; the request's `PolygonFill` classification at the same cell centre supplies the sign a ring set cannot carry alone.
- Exemption: `RasterKernel.Invoke` is a named statement kernel — the package-required `IAction2D` shape is a per-cell body, and every structure it reads is hoisted to construction so the body allocates nothing.
- Entry: `PolygonOp.Raster` consumes one admitted region set, fill rule, grid, and metric through `PolygonAlgebra.Apply`, which composes `FieldPlane.Project` and owns no field logic of its own.
- Auto: `ScalarField.SampleLattice` sweeps each ring's unsigned clearance plane in one kernel pass and `TensorPrimitives.Min` folds them to the exact minimum; `ParallelHelper.For2D` partitions the signing and metric projection over the `IAction2D` kernel; `Memory2D<double>` materializes the plane, `ReadOnlyMemory2D<double>` publishes it, and `TensorPrimitives.IsFiniteAll`, `Min`, `Max`, `Average`, `StdDev`, `IndexOfMin`, and `IndexOfMax` derive the result statistics.
- Result: `SampledField` keeps the plane, grid, metric, finite extrema, dispersion, and the model-space cells holding those extrema together, so engagement, additive masks, and layer audits consume one substrate value; `MinimumAt` over a signed-clearance plane is the deepest interior cell, the largest inscribed disc a cutter can occupy.
- Packages: `Rasm` (project) supplies `CellLattice`, `ScalarField`/`SupportSpace`/`CsgKind`/`BlendKind`, and `BoundarySense`; `Clipper2` supplies the fill classification through `FillProbe`; `CommunityToolkit.HighPerformance` supplies the cell partition and the 2D memory carrier; `System.Numerics.Tensors` supplies the plane fold and the result statistics; `LanguageExt` and `Thinktecture` supply the result carrier and the generated metric family.
- Growth: a new field interpretation is one `FieldMetric` case over the same signed plane with its admission row; a row needing a second sampling strategy belongs to the kernel field algebra, not here.
- Boundary: field storage remains owned by the result, provider paths remain private, and a non-finite cell fails the whole projection. Page-local distance loops are the deleted form; the clearance plane is the kernel's by law.

```csharp
// --- [FIELD_PLANE]
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

    internal double Sample(double clearance) => Switch(
        state: clearance,
        occupancy: static (value, _) => value <= 0.0 ? 1.0 : 0.0,
        signedClearance: static (value, _) => value,
        engagement: static (value, field) => double.Clamp((field.CutterRadius - Math.Abs(value)) / field.CutterRadius, 0.0, 1.0),
        reachable: static (value, field) => value <= -field.ToolRadius ? 1.0 : 0.0,
        inscribedDiameter: static (value, _) => value < 0.0 ? -2.0 * value : 0.0);
}

public sealed record SampledField(
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

public static class FieldPlane {
    public static Fin<SampledField> Project(
        Seq<Loop> paths,
        PolygonFill fill,
        CellLattice grid,
        FieldMetric metric,
        Op op) =>
        from _ in PolygonAlgebra.Check(
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
        let kernel = new RasterKernel(
            values, clearance, new FillProbe(paths, fill, PolygonAlgebra.Precision(paths[0].Tolerance)), grid, metric)
        from result in Sampled(kernel, values, grid, metric, paths[0].Tolerance, paths[0].Plane)
        select result;

    private static Fin<double[]> Clearance(Seq<Loop> paths, CellLattice grid, Op op) =>
        from planes in paths.TraverseM(path => SupportSpace.Of(PolygonAlgebra.ToPolyline(path).ToPolylineCurve(), op)
            .Bind(source => new ScalarField.DistanceCase(source, BoundarySense.Toward)
                .SampleLattice(grid, paths[0].Tolerance, op))).As()
        from head in planes.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "raster:no-source"))
        select planes.Tail.Fold(head.ToArray(), static (least, plane) => Least(least, plane));

    private static double[] Least(double[] least, Arr<double> plane) {
        TensorPrimitives.Min<double>(least, plane.ToArray(), least);
        return least;
    }

    private static Fin<SampledField> Sampled(
        RasterKernel kernel,
        double[,] values,
        CellLattice grid,
        FieldMetric metric,
        Context tolerance,
        double plane) {
        ParallelHelper.For2D(0, values.GetLength(0), 0, values.GetLength(1), in kernel);
        ReadOnlySpan<double> samples = MemoryMarshal.CreateReadOnlySpan(ref values[0, 0], values.Length);
        return TensorPrimitives.IsFiniteAll(samples)
            ? Fin.Succ(new SampledField(
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
            : Fin.Fail<SampledField>(new GeometryFault.DegenerateInput(Kind.Plane, None, "raster:non-finite-cell"));
    }

    private static Point3d CellOf(CellLattice grid, int index) =>
        grid.Center(column: index % grid.Columns.Value, row: index / grid.Columns.Value);

    private readonly struct RasterKernel(
        double[,] values,
        double[] clearance,
        FillProbe probe,
        CellLattice grid,
        FieldMetric metric) : IAction2D {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int row, int column) {
            PointRelation relation = probe.Relation(PolygonAlgebra.ToPoint(grid.Center(column: column, row: row)));
            double magnitude = clearance[(int)grid.Linear(column: column, row: row, layer: 0)];
            values[row, column] = metric.Sample(
                relation == PointRelation.Boundary ? 0.0 : relation == PointRelation.Inside ? -magnitude : magnitude);
        }
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
