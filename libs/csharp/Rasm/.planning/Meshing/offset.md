# [RASM_OFFSETTING_OFFSET]

The exact wavefront offsetting owner of `Rasm.Meshing` — ONE `OffsetOp` `[Union]` (`Skeleton`/`Weighted`/`Offset`/`Medial`/`Minkowski`/`Clearance`) folded by ONE `Offsetting.Apply(OffsetOp, Op? key = null)` entry over one Aichholzer-Aurenhammer wavefront propagation. The exactness lives where a sign decides structure: reflex classification and split-hit admission read the `Numerics/predicates#ROBUST_PREDICATES` exact `Orient2D` turn signs over INPUT geometry, ring simplicity routes `Meshing/intersect` `Intersection.Apply` (the local `SegmentsCross` copy is DEAD — the E7 collapse), and self-overlapping result loops resolve through `Meshing/arrangement` `PlanarOverlay` under the nonzero winding rule (the `Mesh.CreateFromClosedPolyline` null-skip of the prior fence is DEAD — loops route the arrangement DIRECTLY as ring sets); event times are analytic `double` schedule data validated at fire by liveness, ring adjacency, and the collapse band — the prior fence's decorative exact-zero test on float trajectory positions was ILLUSORY exactness and is deleted, the honest contract stated instead.

This page MINTS the kernel's ONE clearance vocabulary — `ClearanceNode(At, Radius, NearestEdge)`, the per-point clearance RADIUS as a first-class result field on every skeleton and medial node plus the `Clearance(probe)` arbitrary-probe op case — the SAME result family `Meshing/skeleton` (W4, 3D MCF) composes, so 2D medial and 3D curve-skeleton speak one clearance language across the `Rasm.Fabrication` toolpath seam (`FAB:22` — `Toolpath/Skeleton.cs` dies for `Offsetting.Apply`; the `RASM-CS-FABRICATION [V5]` weighted/variable-speed rows ride the existing `Weighted` modality). The medial axis is REAL against the `Meshing/delaunay` `VoronoiDual` projection: the ring's constrained Delaunay dual supplies circumcenters WITH circumradii (the clearance payload) whose interior sub-graph carries the parabolic reflex arcs the linear straight skeleton only approximates — the prior fence built the tessellation and DISCARDED it; this one reads the dual as the medial locus and reconciles the skeleton's reflex arcs against it. Corner strategy is a generator: `JoinType` (Miter/Round/Bevel/Square) and `EndType` (Closed/Butt/Square/Round) rows carry their own emission delegates over the ONE offset assembly — the next join is a data row, never a sibling assembler (the CavalierContours kerf/arc lane stays `Rasm.Fabrication` stratum). `MinkowskiConvolution` is COMPLETE as one SUPPORT-VERTEX walk: each ring edge translates by the element vertex extreme under its outward normal, each ring vertex walks the element boundary between supports — CCW at convex turns, CW at reflex turns — so translated A-edges AND every element fan emit from the one walk, connected by construction (a reflex element routes the typed fault — its convex decomposition is the recorded growth row), the cycle resolved through the arrangement. The `WavefrontStore` is an honest single-writer arena under the `Meshing/edit#ARENA_LAW` contract with amortized-doubling capacity — the fixed `2n` under-allocation and the immutable-record prose of the prior fence are dead. Failures route the `offsetting` cluster (`DegenerateOffset` 2416, `SkeletonStalled` 2417, `CollapseStalled` 2418).

## [01]-[INDEX]

- [01]-[OFFSETTING]: ONE `Offsetting.Apply(OffsetOp, Op?)` entry; the wavefront event queue (edge collapse / reflex split) over one `WavefrontStore` arena; the MINTED `ClearanceNode` family + `Clearance(probe)` query; medial REAL against the delaunay `VoronoiDual`; `JoinType`×`EndType` corner/cap generator rows; complete two-directional Minkowski convolution; loop resolution through the arrangement.

## [02]-[OFFSETTING]

- Owner: `JoinType` `[SmartEnum<string>]` (`miter`/`round`/`bevel`/`square`) — each row carries its `[UseDelegateFromConstructor]` `Corner` emission delegate (the INTERIOR fan a dressed corner inserts between its two tangent points: the clamped miter apex, the arc fan at `ArcTolerance`, the bare chord via the empty fan, the squared pair) so the offset assembly reads the row and a join `switch` is unspellable; `EndType` `[SmartEnum<string>]` (`closed`/`butt`/`square`/`round`) — cap emission rows for open-path offsets, `closed` the ring row that emits nothing; `OffsetPolicy` the policy row (`TimeBudget` · `MaxEvents` · `CollapseTolerance` · `MiterLimit` · `ArcTolerance` · `EdgeSpeed` the per-ORIGINAL-edge speed table — `Count` equals the ring's edge count, gated at the `Weighted` admission; a non-empty table IS the weighted lane) registering `IValidityEvidence`; `ClearanceNode` THE minted clearance carrier (`At` · `Radius` — the distance-to-boundary payload every consumer reads · `NearestEdge` the witness); `SkeletonArc(From, To, OriginEdge)`; `SkeletonGraph(Seq<ClearanceNode>, Seq<SkeletonArc>)` — ONE graph shape for skeleton AND medial, the `OffsetResult` case carrying the semantics, the propagation graph pre-seeding the ring vertices as radius-zero nodes `0..n-1` so arcs reference node ids uniformly; `OffsetCurves(Seq<Chain> Loops, double Distance)` — composing intersect's oriented `Chain` rows, open and closed; `WavefrontStore` the single-writer propagation arena (position/velocity columns, `Prev`/`Next` active rings, dead bitset + free list, `SpawnTime` + emanating-`Node` + `EdgeOf` original-edge provenance, the ring `Plane` elevation every emission returns at; `Spawn` grows every column by amortized doubling); `Trace(Store, Graph)` the propagation carrier; `OffsetEvent` `[Union]` (`Edge`/`Split`) the time-ordered queue algebra; `OffsetOp`/`OffsetResult` the request/result unions; `Offsetting` the static surface.
- Cases: `JoinType` rows 4; `EndType` rows 4; `OffsetEvent` cases 2; `OffsetOp` cases `Skeleton` · `Weighted` · `Offset` · `Medial` · `Minkowski` · `Clearance` (6); `OffsetResult` cases `Graph` · `Axis` · `Curves` · `Probe` (4).
- Entry: `public static Fin<OffsetResult> Apply(OffsetOp op, Op? key = null)` — the ONE entry discriminating on the op case. `Fin<T>` routes `GeometryFault.DegenerateOffset(vertex, time)` 2416 on an inadmissible input (an open or non-finite ring, zero area, self-intersecting — simplicity checked through `Intersection.Apply`, never a local crossing kernel; a degenerate open path; a `Weighted` speed table whose `Count` mismatches the edge count; an invalid policy row — gated ONCE at `Apply` over the derived `OffsetOp.Policy` projection); `GeometryFault.SkeletonStalled(pendingEvents, time)` 2417 when the event budget exhausts with the queue non-empty; `GeometryFault.CollapseStalled(iteration, residual)` 2418 on a zero-progress event cycle (a same-time re-enqueue livelock — the residual is the stalled time delta). `Skeleton`/`Weighted` return `Graph`; `Medial` returns `Axis`; `Offset`/`Minkowski` return `Curves`; `Clearance` returns `Probe`. `Offset` owns every offset modality in one case: an inward closed-ring offset (positive `Distance` — the wavefront lane, where topology events live; a distance past the inradius vanishes to a legitimately EMPTY curve set, never a fault), an outward closed-ring offset (negative `Distance` — the direct ribbon lane), and an open-path offset (the two-sided ribbon with `EndType` caps) — the input shape plus the distance sign discriminate, never a sibling `OffsetOpen`/`OffsetOutward` entry. The owner evaluates on the XY projection and every emission returns at the ring's leading elevation — section contours offset in their own plane. No `BuildSkeleton`/`BuildMedial`/`OffsetPolyline` sibling statics — one polymorphic `Apply`.
- Auto: `Skeleton`/`Weighted`/`Offset` run the ONE `Propagate` wavefront — `Seed` builds the active ring with each vertex's INWARD wavefront velocity (the CCW interior sits left of each edge; the velocity solves BOTH incident edge lines advancing at their own `EdgeSpeed` rates — the weighted straight skeleton is the SAME queue at non-unit speeds, never a parallel skeletonizer, and the `EdgeOf` original-edge column keys every respawned vertex's speeds through every rewire), the graph pre-seeds the ring vertices as radius-zero nodes, `EnqueueAt` computes each vertex's analytic edge-collapse and reflex-split candidate times (`IsReflex` the exact `Orient2D` turn sign — a near-collinear vertex never spuriously splits; `SplitTime` closes the reflex against each opposing edge's own inward speed), and the time-ordered `PriorityQueue` drains: an `Edge` event re-validates liveness + ring adjacency + the collapse band then merges the pair into one respawned vertex (skeleton node minted WITH its clearance radius — unit-speed collapse time IS the boundary distance; under `Weighted` the radius recomputes as the true Euclidean edge distance so the clearance payload never lies), a `Split` event re-validates the reflex hit on its opposing edge then divides the ring into two live rings, and every affected vertex re-enqueues its candidates. `Offset` splits by lane: the inward closed-ring offset drains the wavefront FROZEN at `until = Distance` (events past the instant stay unfired, so the store IS the state at `Distance` — never the fully-collapsed end state), EVERY surviving ring walks out as vertex ids (the multi-ring snapshot; the prior fence's first-ring-only walk dropped every post-split component), and `Dressed` replaces each REFLEX corner with the exact arc data of radius `Distance − spawn` centred on the corner's emanating node — tangent points from the lane, the INTERIOR fan from the `JoinType` row (convex corners ARE the exact offset and pass through); a wavefront fully collapsed before `Distance` emits the legitimately EMPTY curve set; the outward and open-path offsets ride the direct ribbon (per-edge translates + corner fans + `EndType` caps — no topology events exist on the growing side); and any self-overlapping loop set from either lane routes `Arrangement.Apply(PlanarOverlay(loops, ∅, Union, plane, policy))` under the nonzero winding rule. `Medial` builds the ring's constrained Delaunay (`Tessellation.Build`, `Delaunay` mode, boundary `Constraint.Segment` rows), takes `VoronoiDual`, keeps the interior sub-graph (dual nodes of inside triangles — the even-odd parity of each triangle against the ring), and emits the medial `SkeletonGraph` nodes as `ClearanceNode(circumcenter, circumradius, nearestEdge)` — the parabolic reflex arcs ARE the dual's curved sampling, reconciled against the straight skeleton's linear reflex arcs (the skeleton underestimates clearance at reflex fans; the dual carries the true bisector locus). `Minkowski` gates the element closed + CCW-oriented + convex (exact turn signs; a reflex turn faults — convex decomposition is the recorded growth row) then runs the SUPPORT-VERTEX walk: each ring edge translates by the element vertex extreme under its outward normal, each ring vertex walks the element boundary from the previous support to the next — CCW at convex turns (the fan), CW at reflex turns (the reversed arc) — one connected cycle carrying both convolution directions, resolved through the arrangement's nonzero winding into the sum boundary (a global two-pointer normal merge is unsound for non-convex rings, whose normal sequence is not angle-sorted). `Clearance` answers the arbitrary probe: the exact scan over ring edges for the minimum point-segment distance, returning `ClearanceNode(probe, radius, argminEdge)`.
- Receipt: none on a dedicated rail — the `OffsetResult` union IS the typed result, every node row carrying its clearance radius as first-class evidence; the hash-eligible artifacts are the emitted `Polyline`/`Chain` values, never the live `WavefrontStore`.
- Packages: `Rasm.Numerics` (`Predicate.Orient2D`/`Sign`/`Axis` — the exact-turn floor), `Rasm.Meshing` (`Intersection.Apply` `SegmentSegment` — ring simplicity + convolution crossing checks; `Chain` — the loop rows), `Rasm.Meshing` delaunay owners (`Tessellation.Build` + `VoronoiDual`/`DualGraph` — the medial substrate) and arrangement owners (`Arrangement.Apply` `PlanarOverlay` — loop resolution), `Rasm.Numerics` (`GeometryFault`), `Rasm.Domain` (`Op`, `Kind`, `ValidityClaim`/`IValidityEvidence`), `Rhino.Geometry` (`Point3d`/`Vector3d`/`Polyline`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`PriorityQueue<TElement,TPriority>`).
- Growth: a new offsetting modality (curved-input offset, multi-ring nesting offset) is one `OffsetOp` case over the SAME propagation; a new corner strategy is ONE `JoinType` row carrying its emission delegate (the generator law — never a fourth assembly body); a new cap is one `EndType` row; a new event shape is one `OffsetEvent` case plus one drain arm; the 3D clearance consumer (`Meshing/skeleton`, W4) COMPOSES `ClearanceNode` — the family widens by zero types; the Fabrication variable-speed rows ride `EdgeSpeed`; zero new surface.
- Boundary: the offsetting owner is the ONE `OffsetOp` `[Union]` and a `StraightSkeleton`/`MedialAxis`/`MinkowskiSum`/`PolygonOffset` sibling family is the named density defect collapsed onto one union; the clearance vocabulary is THE `ClearanceNode` family minted here and a radius-less medial node, a sibling 3D clearance shape, or a per-consumer clearance record is the named capability defect (`Meshing/skeleton` composes THIS family); event ordering is analytic schedule data and CLAIMED exactness over float trajectory positions is the named illusory-robustness defect this rebuild deletes — the exact signs live at reflex classification, split admission, simplicity, and convolution compatibility, all over INPUT geometry; ring simplicity and convolution crossings route `Intersection.Apply` and a local four-sign straddle copy is the deleted form; loop resolution routes `Arrangement.Apply` `PlanarOverlay` ring-direct and a `Mesh.CreateFromClosedPolyline` round-trip (null on exactly the self-intersecting loops the resolve exists for) is the deleted circular form; the medial COMPOSES the delaunay `VoronoiDual` and a discarded substrate build or a local Voronoi re-derivation is the deleted form; the `WavefrontStore` is an honest single-writer arena under the `Meshing/edit#ARENA_LAW` contract (amortized doubling — a store sized `2n` where splits spawn unbounded vertices is the deleted under-allocation) and immutable prose over a mutating store is the deleted claim; `Apply` is total over the `Fin` rail and a thrown exception on a degenerate ring or a stalled queue is forbidden; a `Split` divides the ring rather than discarding a reflex chain — no propagation drops a polygon feature to satisfy a budget.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
// CS0104 guard: LanguageExt.HashSet collides with the BCL name under the dual usings.
using IndexSet = System.Collections.Generic.HashSet<int>;

namespace Rasm.Meshing;

// --- [TYPES] ------------------------------------------------------------------------------
// GENERATOR_LAW: each row carries its corner emission — the INTERIOR fan inserted between the two
// tangent points of a dressed corner (the lanes supply the tangent endpoints; Bevel's empty fan
// leaves the bare chord). A new join is one row; a join switch is unspellable.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JoinType {
    public static readonly JoinType Miter  = new("miter", MiterCorner);
    public static readonly JoinType Round  = new("round", RoundCorner);
    public static readonly JoinType Bevel  = new("bevel", static (_, _, _, _, _) => Seq<Point3d>());
    public static readonly JoinType Square = new("square", SquareCorner);

    [UseDelegateFromConstructor]
    public partial Seq<Point3d> Corner(Point3d apex, Vector3d nIn, Vector3d nOut, double distance, OffsetPolicy policy);

    // Miter apex = bisector hit, clamped at MiterLimit x distance; beyond the clamp the row
    // degrades to the bevel chord — the clamp is the row's own law, never a caller branch.
    static Seq<Point3d> MiterCorner(Point3d apex, Vector3d nIn, Vector3d nOut, double distance, OffsetPolicy policy) {
        Vector3d bisector = nIn + nOut;
        double len = bisector.Length;
        if (len <= double.Epsilon) { return Seq<Point3d>(); }
        double reach = distance / (0.5 * len);
        return reach <= policy.MiterLimit * Math.Abs(distance)
            ? Seq(apex + (reach / len) * bisector)
            : Seq<Point3d>();
    }

    // In-plane rotation from nIn toward nOut through the SHORT arc — total at sweep = π (the
    // RoundCap's half circle), where a sin(sweep) slerp denominator dies.
    static Seq<Point3d> RoundCorner(Point3d apex, Vector3d nIn, Vector3d nOut, double distance, OffsetPolicy policy) {
        double cross = (nIn.X * nOut.Y) - (nIn.Y * nOut.X);
        double sweep = Math.Atan2(Math.Abs(cross), (nIn.X * nOut.X) + (nIn.Y * nOut.Y));
        double turn = cross < 0.0 ? -1.0 : 1.0;
        Vector3d perp = new(-turn * nIn.Y, turn * nIn.X, 0.0);
        int steps = int.Max(1, (int)Math.Ceiling(sweep / (2.0 * Math.Acos(double.Clamp(1.0 - (policy.ArcTolerance / Math.Abs(distance)), -1.0, 1.0)))));
        return toSeq(Enumerable.Range(1, steps - 1).Select(i => {
            double t = sweep * i / steps;
            return apex + (distance * ((Math.Cos(t) * nIn) + (Math.Sin(t) * perp)));
        }));
    }

    // The genuine square join: both points sit on the chord perpendicular to the bisector at the
    // offset distance — each offset point slides along its own tangent line to that cut, the
    // signed advance absorbing either turn direction.
    static Seq<Point3d> SquareCorner(Point3d apex, Vector3d nIn, Vector3d nOut, double distance, OffsetPolicy policy) {
        Vector3d bisector = nIn + nOut;
        double len = bisector.Length;
        if (len <= double.Epsilon) { return Seq<Point3d>(); }
        (double bx, double by) = (bisector.X / len, bisector.Y / len);
        double cos = (nIn.X * bx) + (nIn.Y * by);
        double dotIn = (-nIn.Y * bx) + (nIn.X * by);
        double dotOut = (-nOut.Y * bx) + (nOut.X * by);
        if (Math.Abs(dotIn) <= double.Epsilon || Math.Abs(dotOut) <= double.Epsilon) { return Seq<Point3d>(); }
        return Seq(
            apex + (distance * nIn) + ((distance * (1.0 - cos) / dotIn) * new Vector3d(-nIn.Y, nIn.X, 0.0)),
            apex + (distance * nOut) + ((distance * (1.0 - cos) / dotOut) * new Vector3d(-nOut.Y, nOut.X, 0.0)));
    }
}

// Cap rows for open-path offsets; `closed` is the ring row emitting nothing.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EndType {
    public static readonly EndType Closed = new("closed", static (_, _, _, _) => Seq<Point3d>());
    public static readonly EndType Butt   = new("butt", static (_, _, _, _) => Seq<Point3d>());
    public static readonly EndType Square = new("square", SquareCap);
    public static readonly EndType Round  = new("round", RoundCap);

    [UseDelegateFromConstructor]
    public partial Seq<Point3d> Cap(Point3d end, Vector3d tangent, double distance, OffsetPolicy policy);

    static Seq<Point3d> SquareCap(Point3d end, Vector3d tangent, double distance, OffsetPolicy policy) =>
        Seq(end + distance * new Vector3d(tangent.Y, -tangent.X, 0.0) + distance * tangent,
            end + distance * new Vector3d(-tangent.Y, tangent.X, 0.0) + distance * tangent);

    static Seq<Point3d> RoundCap(Point3d end, Vector3d tangent, double distance, OffsetPolicy policy) =>
        JoinType.Round.Corner(end, new Vector3d(tangent.Y, -tangent.X, 0.0), new Vector3d(-tangent.Y, tangent.X, 0.0), distance, policy);
}

// --- [CONSTANTS] --------------------------------------------------------------------------
// EdgeSpeed is the Weighted lane's PER-EDGE speed table (index = original ring edge; the Weighted
// admission gates Count == edge count — a wrapped or padded table is a mis-addressed request).
public sealed record OffsetPolicy(
    double TimeBudget, int MaxEvents, double CollapseTolerance, double MiterLimit, double ArcTolerance, Arr<double> EdgeSpeed = default) : IValidityEvidence {
    public static readonly OffsetPolicy Canonical =
        new(TimeBudget: 1e9, MaxEvents: 1 << 20, CollapseTolerance: 1e-12, MiterLimit: 2.0, ArcTolerance: 1e-3);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: TimeBudget),
        ValidityClaim.Positive(value: MaxEvents),
        ValidityClaim.Positive(value: CollapseTolerance),
        ValidityClaim.Positive(value: MiterLimit),
        ValidityClaim.Positive(value: ArcTolerance),
        ValidityClaim.Of(EdgeSpeed.ForAll(static speed => speed > 0.0)));
}

// --- [MODELS] -----------------------------------------------------------------------------
// THE minted clearance family — one vocabulary across 2D medial/skeleton and the W4 3D MCF
// skeleton: position, distance-to-boundary radius, and the nearest-feature witness.
public sealed record ClearanceNode(Point3d At, double Radius, int NearestEdge);

// ONE graph shape for skeleton AND medial (the OffsetResult case carries the semantics — two
// field-identical records are the deleted sibling form). The propagation graph pre-seeds the ring
// vertices as nodes 0..n-1 at radius zero (the degree-1 endpoints), so arcs reference node ids
// uniformly — never a ring-index/node-index pun in one field.
public sealed record SkeletonArc(int From, int To, int OriginEdge);
public sealed record SkeletonGraph(Seq<ClearanceNode> Nodes, Seq<SkeletonArc> Arcs);
public sealed record OffsetCurves(Seq<Chain> Loops, double Distance);

// Single-writer arena under the Meshing/edit ARENA_LAW: every Spawn grows every column by
// amortized doubling — splits spawn unbounded vertices, so a fixed-extent store is structurally
// impossible here, not merely discouraged. The wavefront evaluates on the XY projection; Plane
// carries the ring elevation so every emission returns at the source plane. Node = the skeleton
// node the vertex emanates from (ring seeds ARE nodes 0..n-1); EdgeOf = the ORIGINAL ring edge
// the vertex's outgoing wavefront edge descends from — the weighted lane's speed key through
// every rewire.
public sealed class WavefrontStore {
    double[] px, py, vx, vy, spawnTime;
    int[] prev, next, node, edgeOf;
    bool[] dead;
    readonly Stack<int> free = new();
    readonly double plane;
    int count;

    public WavefrontStore(int seed, double plane) {
        (px, py, vx, vy, spawnTime) = (new double[seed], new double[seed], new double[seed], new double[seed], new double[seed]);
        (prev, next, node, edgeOf, dead) = (new int[seed], new int[seed], new int[seed], new int[seed], new bool[seed]);
        this.plane = plane;
    }

    public int Count => count;
    public bool Alive(int v) => v >= 0 && v < count && !dead[v];
    public int Prev(int v) => prev[v];
    public int Next(int v) => next[v];
    public int Node(int v) => node[v];
    public int EdgeOf(int v) => edgeOf[v];
    public double SpawnTime(int v) => spawnTime[v];
    public Point3d At(int v, double time) =>
        new(px[v] + ((time - spawnTime[v]) * vx[v]), py[v] + ((time - spawnTime[v]) * vy[v]), plane);
    public Vector3d Velocity(int v) => new(vx[v], vy[v], 0.0);

    public int Spawn(Point3d at, Vector3d velocity, double time, int fromNode, int outEdge) {
        int v = free.Count > 0 ? free.Pop() : count++;
        Grow(v + 1);
        (px[v], py[v], vx[v], vy[v]) = (at.X, at.Y, velocity.X, velocity.Y);
        (spawnTime[v], node[v], edgeOf[v], dead[v]) = (time, fromNode, outEdge, false);
        return v;
    }

    public void Kill(int v) { dead[v] = true; free.Push(v); }
    public void LinkRing(int a, int b) { next[a] = b; prev[b] = a; }

    void Grow(int needed) {
        if (needed <= px.Length) { return; }
        int extent = int.Max(needed, px.Length << 1);
        Array.Resize(ref px, extent); Array.Resize(ref py, extent);
        Array.Resize(ref vx, extent); Array.Resize(ref vy, extent);
        Array.Resize(ref spawnTime, extent);
        Array.Resize(ref prev, extent); Array.Resize(ref next, extent);
        Array.Resize(ref node, extent); Array.Resize(ref edgeOf, extent);
        Array.Resize(ref dead, extent);
    }
}

public readonly record struct Trace(WavefrontStore Store, SkeletonGraph Graph);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetEvent {
    private OffsetEvent() { }

    public sealed record Edge(double Time, int Vertex, int NextVertex) : OffsetEvent;
    public sealed record Split(double Time, int Reflex, int OpposingA, int OpposingB) : OffsetEvent;

    public double Time =>
        Switch(edge: static e => e.Time, split: static s => s.Time);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetResult {
    private OffsetResult() { }

    public sealed record Graph(SkeletonGraph Skeleton) : OffsetResult;
    public sealed record Axis(SkeletonGraph Medial) : OffsetResult;
    public sealed record Curves(OffsetCurves Offset) : OffsetResult;
    public sealed record Probe(ClearanceNode Node) : OffsetResult;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetOp {
    private OffsetOp() { }

    public sealed record Skeleton(Polyline Ring, OffsetPolicy Policy) : OffsetOp;
    public sealed record Weighted(Polyline Ring, OffsetPolicy Policy) : OffsetOp;
    public sealed record Offset(Polyline Path, double Distance, JoinType Join, EndType End, OffsetPolicy Policy) : OffsetOp;
    public sealed record Medial(Polyline Ring, OffsetPolicy Policy) : OffsetOp;
    public sealed record Minkowski(Polyline Ring, Polyline Element, OffsetPolicy Policy) : OffsetOp;
    public sealed record Clearance(Polyline Ring, Point3d Probe, OffsetPolicy Policy) : OffsetOp;

    public OffsetPolicy Policy =>
        Switch(
            skeleton:  static s => s.Policy,
            weighted:  static w => w.Policy,
            offset:    static o => o.Policy,
            medial:    static m => m.Policy,
            minkowski: static k => k.Policy,
            clearance: static c => c.Policy);
}

public static class Offsetting {
    public static Fin<OffsetResult> Apply(OffsetOp op, Op? key = null) =>
        !op.Policy.IsValid
        ? Fin.Fail<OffsetResult>(new GeometryFault.DegenerateOffset(0, 0.0).ToError())
        : op.Switch(
            state: key,
            skeleton:  static (key, s) => AdmitRing(s.Ring, key).Bind(ring => Propagate(ring, s.Policy, Arr<double>.Empty)).Map(static t => (OffsetResult)new OffsetResult.Graph(t.Graph)),
            weighted:  static (key, w) => AdmitRing(w.Ring, key)
                .Bind(ring => w.Policy.EdgeSpeed.Count == ring.Count - 1
                    ? Propagate(ring, w.Policy, SignedArea(w.Ring) < 0.0 ? ReversedSpeeds(w.Policy.EdgeSpeed) : w.Policy.EdgeSpeed)
                    : Fin.Fail<Trace>(new GeometryFault.DegenerateOffset(w.Policy.EdgeSpeed.Count, 0.0).ToError()))
                .Map(static t => (OffsetResult)new OffsetResult.Graph(t.Graph)),
            offset:    static (key, o) => Snapshot(o, key),
            medial:    static (key, m) => AdmitRing(m.Ring, key).Bind(ring => MedialOf(ring, key)).Map(static axis => (OffsetResult)new OffsetResult.Axis(axis)),
            minkowski: static (key, k) => AdmitRing(k.Ring, key).Bind(ring => Convolve(ring, k.Element, key)).Map(static loops => (OffsetResult)new OffsetResult.Curves(loops)),
            clearance: static (key, c) => AdmitRing(c.Ring, key).Map(ring => (OffsetResult)new OffsetResult.Probe(ClearanceAt(ring, c.Probe))));

    // Admission once: finite, closed, non-zero-area, CCW-oriented, SIMPLE — simplicity routes the
    // ONE crossing owner per non-adjacent pair, never a local straddle copy. The owner evaluates
    // on the XY projection; the ring's leading elevation rides through to every emission.
    static Fin<Polyline> AdmitRing(Polyline ring, Op? key) {
        if (ring.Count < 4 || !ring.IsClosed) { return Fail(0); }
        for (int i = 0; i < ring.Count; i++) {
            if (!ring[i].IsValid) { return Fail(i); }
        }
        if (SignedArea(ring) == 0.0) { return Fail(0); }
        int n = ring.Count - 1;
        for (int i = 0; i < n; i++) {
            for (int j = i + 2; j < n; j++) {
                if (i == 0 && j == n - 1) { continue; }
                Fin<IntersectResult> hit = Intersection.Apply(
                    new IntersectOp.SegmentSegment(new Line(ring[i], ring[i + 1]), new Line(ring[j], ring[j + 1]), Axis.Z, IntersectPolicy.Canonical), key);
                if (hit.Case is IntersectResult.Points { Hits.IsEmpty: false }) { return Fail(i); }
            }
        }
        return Fin.Succ(Oriented(ring));
    }

    // Open-path admission: finite vertices, two or more of them, no zero-length edge (a coincident
    // pair degenerates the ribbon normal).
    static Fin<Polyline> AdmitPath(Polyline path) {
        if (path.Count < 2) { return Fail(0); }
        for (int i = 0; i < path.Count; i++) {
            if (!path[i].IsValid) { return Fail(i); }
            if (i > 0 && path[i] == path[i - 1]) { return Fail(i); }
        }
        return Fin.Succ(path);
    }

    static Fin<Polyline> Fail(int vertex) => Fin.Fail<Polyline>(new GeometryFault.DegenerateOffset(vertex, 0.0).ToError());

    // --- [WAVEFRONT]
    // Event times are analytic schedule data; validity at fire is liveness + ring adjacency + the
    // collapse band. The exact signs live at reflex classification and split admission over input
    // geometry — never a fake zero-test of float trajectory positions. `until` freezes the drain:
    // events past it stay unfired, so the store IS the wavefront state at that time — the offset
    // snapshot reads a true instant, never the fully-collapsed end state. The graph pre-seeds the
    // ring vertices as nodes 0..n-1 (radius zero — the boundary endpoints), so arcs reference node
    // ids uniformly; a non-empty speed table IS the weighted lane.
    static Fin<Trace> Propagate(Polyline ring, OffsetPolicy policy, Arr<double> speeds, double until = double.PositiveInfinity) {
        WavefrontStore store = Seed(ring, speeds);
        PriorityQueue<OffsetEvent, double> queue = new();
        int n = ring.Count - 1;
        List<ClearanceNode> nodes = new(Enumerable.Range(0, n).Select(i => new ClearanceNode(ring[i], 0.0, i)));
        List<SkeletonArc> arcs = new();
        for (int v = 0; v < store.Count; v++) { EnqueueAt(store, queue, v, 0.0, policy, speeds); }
        (int fired, double lastTime, int sameTime) = (0, -1.0, 0);
        while (queue.Count > 0 && queue.Peek().Time <= until) {
            if (fired++ > policy.MaxEvents) { return Fin.Fail<Trace>(new GeometryFault.SkeletonStalled(queue.Count, queue.Peek().Time).ToError()); }
            OffsetEvent ev = queue.Dequeue();
            if (ev.Time > policy.TimeBudget) { return Fin.Fail<Trace>(new GeometryFault.SkeletonStalled(queue.Count, ev.Time).ToError()); }
            sameTime = ev.Time == lastTime ? sameTime + 1 : 0;
            if (sameTime > store.Count << 2) { return Fin.Fail<Trace>(new GeometryFault.CollapseStalled(fired, ev.Time - lastTime).ToError()); }
            lastTime = ev.Time;
            // Generated TOTAL Switch: a new event case breaks this drain at compile time. A guard
            // miss is a stale event superseded by a rewire — skipped by validation, never a tolerance guess.
            ev.Switch(
                edge: e => {
                    if (store.Alive(e.Vertex) && store.Alive(e.NextVertex) && store.Next(e.Vertex) == e.NextVertex
                        && store.At(e.Vertex, e.Time).DistanceTo(store.At(e.NextVertex, e.Time)) <= policy.CollapseTolerance) {
                        Collapse(store, e, ring, nodes, arcs, queue, policy, speeds);
                    }
                },
                split: s => {
                    if (store.Alive(s.Reflex) && store.Alive(s.OpposingA) && store.Alive(s.OpposingB) && store.Next(s.OpposingA) == s.OpposingB) {
                        Divide(store, s, ring, nodes, arcs, queue, policy, speeds);
                    }
                });
        }
        return Fin.Succ(new Trace(store, new SkeletonGraph(toSeq(nodes), toSeq(arcs))));
    }

    static WavefrontStore Seed(Polyline ring, Arr<double> speeds) {
        int n = ring.Count - 1;
        WavefrontStore store = new(int.Max(2 * n, 16), ring[0].Z);
        for (int i = 0; i < n; i++) {
            int inEdge = (i - 1 + n) % n;
            store.Spawn(ring[i], Bisector(ring[inEdge], ring[i], ring[(i + 1) % n], Speed(speeds, inEdge), Speed(speeds, i)), 0.0, fromNode: i, outEdge: i);
        }
        for (int i = 0; i < n; i++) { store.LinkRing(i, (i + 1) % n); }
        return store;
    }

    static double Speed(Arr<double> speeds, int edge) => speeds.Count > 0 ? speeds[edge] : 1.0;

    // A CW input re-orients CCW at admission; reversed-ring edge k is caller edge n-1-k, so the
    // per-ORIGINAL-edge table re-indexes with the ring — a silently mis-keyed weighted front is
    // the deleted wrongness.
    static Arr<double> ReversedSpeeds(Arr<double> speeds) {
        double[] flipped = new double[speeds.Count];
        for (int e = 0; e < flipped.Length; e++) { flipped[e] = speeds[speeds.Count - 1 - e]; }
        return Arr.create<double>(flipped);
    }

    // INWARD wavefront velocity: the CCW ring's interior sits LEFT of each edge, so the inward
    // normal of (a -> b) is (a.Y - b.Y, b.X - a.X) — the (dy, -dx) spelling is OUTWARD and grows
    // the front (no event ever fires). The velocity solves v·n̂In = speedIn ∧ v·n̂Out = speedOut —
    // each incident edge line advances at its OWN speed (the weighted lane's exact vertex motion;
    // a bisector scaled by one blended speed under-runs the faster edge). Equal speeds recover the
    // classic 2b/|b|² bisector; parallel edges translate at the out-edge speed.
    static Vector3d Bisector(Point3d prev, Point3d cur, Point3d next, double speedIn, double speedOut) {
        Vector3d nIn = Unit(new Vector3d(prev.Y - cur.Y, cur.X - prev.X, 0.0));
        Vector3d nOut = Unit(new Vector3d(cur.Y - next.Y, next.X - cur.X, 0.0));
        double det = (nIn.X * nOut.Y) - (nIn.Y * nOut.X);
        return Math.Abs(det) <= double.Epsilon
            ? speedOut * nOut
            : new Vector3d(((speedIn * nOut.Y) - (speedOut * nIn.Y)) / det, ((speedOut * nIn.X) - (speedIn * nOut.X)) / det, 0.0);
    }

    static void EnqueueAt(WavefrontStore store, PriorityQueue<OffsetEvent, double> queue, int v, double now, OffsetPolicy policy, Arr<double> speeds) {
        if (!store.Alive(v)) { return; }
        int nxt = store.Next(v);
        EdgeCollapseTime(store, v, nxt, now).IfSome(t => { if (t <= policy.TimeBudget) { queue.Enqueue(new OffsetEvent.Edge(t, v, nxt), t); } });
        if (IsReflex(store, v, now)) {
            SplitTime(store, v, now, speeds).IfSome(s => { if (s.Time <= policy.TimeBudget) { queue.Enqueue(new OffsetEvent.Split(s.Time, v, s.A, s.B), s.Time); } });
        }
    }

    static void Collapse(WavefrontStore store, OffsetEvent.Edge ev, Polyline ring, List<ClearanceNode> nodes, List<SkeletonArc> arcs, PriorityQueue<OffsetEvent, double> queue, OffsetPolicy policy, Arr<double> speeds) {
        Point3d meet = store.At(ev.Vertex, ev.Time);
        (double radius, int witness) = EdgeDistance(ring, meet);
        int node = nodes.Count;
        nodes.Add(new ClearanceNode(meet, speeds.Count > 0 ? radius : ev.Time, witness));  // unit speed: time IS the boundary distance
        arcs.Add(new SkeletonArc(store.Node(ev.Vertex), node, store.EdgeOf(ev.Vertex)));
        arcs.Add(new SkeletonArc(store.Node(ev.NextVertex), node, store.EdgeOf(ev.NextVertex)));
        (int before, int after) = (store.Prev(ev.Vertex), store.Next(ev.NextVertex));
        store.Kill(ev.Vertex);
        store.Kill(ev.NextVertex);
        if (before == ev.NextVertex || after == ev.Vertex) { return; }  // a 2-ring dies at its node
        int outEdge = store.EdgeOf(ev.NextVertex);
        int merged = store.Spawn(meet,
            Bisector(store.At(before, ev.Time), meet, store.At(after, ev.Time), Speed(speeds, store.EdgeOf(before)), Speed(speeds, outEdge)),
            ev.Time, node, outEdge);
        store.LinkRing(before, merged);
        store.LinkRing(merged, after);
        EnqueueAt(store, queue, before, ev.Time, policy, speeds);
        EnqueueAt(store, queue, merged, ev.Time, policy, speeds);
    }

    static void Divide(WavefrontStore store, OffsetEvent.Split ev, Polyline ring, List<ClearanceNode> nodes, List<SkeletonArc> arcs, PriorityQueue<OffsetEvent, double> queue, OffsetPolicy policy, Arr<double> speeds) {
        Point3d hit = store.At(ev.Reflex, ev.Time);
        (double radius, int witness) = EdgeDistance(ring, hit);
        int node = nodes.Count;
        nodes.Add(new ClearanceNode(hit, speeds.Count > 0 ? radius : ev.Time, witness));
        arcs.Add(new SkeletonArc(store.Node(ev.Reflex), node, store.EdgeOf(ev.Reflex)));
        (int before, int after) = (store.Prev(ev.Reflex), store.Next(ev.Reflex));
        int opposingEdge = store.EdgeOf(ev.OpposingA);  // both halves of the split edge keep its origin
        int left = store.Spawn(hit,
            Bisector(store.At(before, ev.Time), hit, store.At(ev.OpposingB, ev.Time), Speed(speeds, store.EdgeOf(before)), Speed(speeds, opposingEdge)),
            ev.Time, node, opposingEdge);
        int right = store.Spawn(hit,
            Bisector(store.At(ev.OpposingA, ev.Time), hit, store.At(after, ev.Time), Speed(speeds, opposingEdge), Speed(speeds, store.EdgeOf(ev.Reflex))),
            ev.Time, node, store.EdgeOf(ev.Reflex));
        store.Kill(ev.Reflex);
        store.LinkRing(before, left);
        store.LinkRing(left, ev.OpposingB);
        store.LinkRing(ev.OpposingA, right);
        store.LinkRing(right, after);
        foreach (int v in (ReadOnlySpan<int>)[before, left, ev.OpposingA, right]) { EnqueueAt(store, queue, v, ev.Time, policy, speeds); }
    }

    static Option<double> EdgeCollapseTime(WavefrontStore store, int u, int v, double now) {
        (Point3d pu, Point3d pv) = (store.At(u, now), store.At(v, now));
        (Vector3d du, Vector3d dv) = (store.Velocity(u), store.Velocity(v));
        Vector3d gap = pv - pu, rel = dv - du;
        double closing = (gap.X * rel.X) + (gap.Y * rel.Y);
        double speed2 = (rel.X * rel.X) + (rel.Y * rel.Y);
        return closing < 0.0 && speed2 > 0.0 ? Some(now + (-closing / speed2)) : None;
    }

    // The opposing edge's line moves INWARD at its own speed; the reflex hits when its signed
    // offset along the inward normal m closes — t = now + m·(a − p) / (d·m − speed).
    static Option<(double Time, int A, int B)> SplitTime(WavefrontStore store, int reflex, double now, Arr<double> speeds) {
        Point3d p = store.At(reflex, now);
        Vector3d d = store.Velocity(reflex);
        (Option<(double, int, int)> best, double bestTime) = (None, double.PositiveInfinity);
        for (int e = store.Next(reflex); store.Alive(e) && e != store.Prev(reflex); e = store.Next(e)) {
            int f = store.Next(e);
            if (e == reflex || f == reflex || !store.Alive(f)) { continue; }
            (Point3d a, Point3d b) = (store.At(e, now), store.At(f, now));
            Vector3d m = Unit(new Vector3d(a.Y - b.Y, b.X - a.X, 0.0));
            double approach = (d.X * m.X) + (d.Y * m.Y) - Speed(speeds, store.EdgeOf(e));
            if (approach >= 0.0) { continue; }
            double t = now + (((m.X * (a.X - p.X)) + (m.Y * (a.Y - p.Y))) / approach);
            if (t > now && t < bestTime) { (best, bestTime) = (Some((t, e, f)), t); }
        }
        return best.Map(static x => (x.Item1, x.Item2, x.Item3));
    }

    // The exact turn sign at the vertex's CURRENT ring neighbours — a near-collinear vertex never
    // spuriously splits; the ring is CCW, so a clockwise turn is reflex.
    static bool IsReflex(WavefrontStore store, int v, double now) =>
        store.Alive(v)
        && Predicate.Orient2D(store.At(store.Prev(v), now), store.At(v, now), store.At(store.Next(v), now)) == Sign.Negative;

    // --- [OFFSET_ASSEMBLY]
    // Two lanes, one modality: INWARD (positive distance, closed ring) rides the wavefront frozen
    // at `until = Distance` — the event queue owns the topology changes, EVERY surviving ring
    // walks out, and a wavefront that fully collapses before `Distance` is a legitimately EMPTY
    // curve set (the offset vanished past the inradius), never a fault; OUTWARD (negative
    // distance) and OPEN paths ride the direct ribbon (edge translates + JoinType fans + EndType
    // caps — no topology events exist there). Both resolve self-overlap through the arrangement
    // under the nonzero winding rule.
    static Fin<OffsetResult> Snapshot(OffsetOp.Offset op, Op? key) =>
        (op.Path.IsClosed && op.Distance > 0.0
            ? AdmitRing(op.Path, key)
                .Bind(ring => Propagate(ring, op.Policy, Arr<double>.Empty, until: op.Distance))
                .Map(trace => Rings(trace.Store).Map(loop => Dressed(trace, loop, op)))
            : op.Path.IsClosed
                ? AdmitRing(op.Path, key).Map(ring => Ribbon(op with { Path = ring }))
                : AdmitPath(op.Path).Map(path => Ribbon(op with { Path = path })))
        .Bind(loops => loops.IsEmpty ? Fin.Succ(Seq<Chain>()) : Resolve(loops, key))
        .Map(chains => (OffsetResult)new OffsetResult.Curves(new OffsetCurves(chains, op.Distance)));

    static Seq<int[]> Rings(WavefrontStore store) {
        IndexSet seen = new();
        List<int[]> loops = new();
        for (int v = 0; v < store.Count; v++) {
            if (!store.Alive(v) || seen.Contains(v)) { continue; }
            List<int> loop = new();
            int cur = v;
            do {
                seen.Add(cur);
                loop.Add(cur);
                cur = store.Next(cur);
            } while (store.Alive(cur) && cur != v && !seen.Contains(cur));
            if (loop.Count > 2) { loops.Add([.. loop]); }
        }
        return toSeq(loops);
    }

    // Corner dressing on the wavefront snapshot: a REFLEX corner's true inward offset is the ARC
    // of radius (Distance - spawn) centred on the corner's emanating NODE — the lane supplies the
    // two tangent points (centre + r·m̂ along each adjacent edge's inward normal; the moving edge
    // stays tangent to that circle), the JoinType row emits only the INTERIOR fan between them
    // (Miter's fan is the CLAMPED apex — the trajectory vertex under MiterLimit, the bevel chord
    // past it; a row-equality bypass here would exempt Miter from its own clamp). Convex corners
    // ARE the exact offset and pass through untouched.
    static Polyline Dressed(Trace trace, int[] loop, OffsetOp.Offset op) {
        WavefrontStore store = trace.Store;
        Polyline dressed = new();
        int n = loop.Length;
        for (int k = 0; k < n; k++) {
            int v = loop[k];
            Point3d at = store.At(v, op.Distance);
            (Point3d prev, Point3d next) = (store.At(loop[(k - 1 + n) % n], op.Distance), store.At(loop[(k + 1) % n], op.Distance));
            double r = op.Distance - store.SpawnTime(v);
            bool reflex = r > 0.0 && Predicate.Orient2D(prev, at, next) == Sign.Negative;
            if (!reflex) { dressed.Add(at); continue; }
            Point3d centre = trace.Graph.Nodes[store.Node(v)].At;
            Vector3d mIn = Unit(new Vector3d(prev.Y - at.Y, at.X - prev.X, 0.0));
            Vector3d mOut = Unit(new Vector3d(at.Y - next.Y, next.X - at.X, 0.0));
            dressed.Add(centre + (r * mIn));
            foreach (Point3d fan in op.Join.Corner(centre, mIn, mOut, r, op.Policy)) { dressed.Add(fan); }
            dressed.Add(centre + (r * mOut));
        }
        if (dressed.Count > 2) { dressed.Add(dressed[0]); }
        return dressed;
    }

    // The direct ribbon: per-edge offset translates with the row's fan at every turned-convex
    // vertex; open paths close through the EndType caps and the mirrored return side.
    static Seq<Polyline> Ribbon(OffsetOp.Offset op) {
        Polyline path = op.Path;
        bool closed = path.IsClosed;
        int n = path.Count - (closed ? 1 : 0);
        double d = Math.Abs(op.Distance);
        Polyline cycle = new();
        Emit(cycle, path, n, closed, d, op);
        if (!closed) {
            foreach (Point3d cap in op.End.Cap(path[n - 1], Unit(path[n - 1] - path[n - 2]), d, op.Policy)) { cycle.Add(cap); }
            Emit(cycle, Reversed(path), n, closed: false, d, op);
            foreach (Point3d cap in op.End.Cap(path[0], Unit(path[0] - path[1]), d, op.Policy)) { cycle.Add(cap); }
        }
        if (cycle.Count > 2) { cycle.Add(cycle[0]); return Seq(cycle); }
        return Seq<Polyline>();

        static void Emit(Polyline cycle, Polyline path, int n, bool closed, double d, OffsetOp.Offset op) {
            int edges = closed ? n : n - 1;
            for (int i = 0; i < edges; i++) {
                (Point3d a, Point3d b) = (path[i], path[(i + 1) % n]);
                Vector3d normal = d * Unit(Normal(a, b));
                cycle.Add(a + normal);
                cycle.Add(b + normal);
                if (closed || i + 1 < edges) {
                    foreach (Point3d fan in op.Join.Corner(b, Unit(Normal(a, b)), Unit(Normal(b, path[(i + 2) % n])), d, op.Policy)) { cycle.Add(fan); }
                }
            }
        }

        static Polyline Reversed(Polyline path) {
            Polyline back = new(path);
            back.Reverse();
            return back;
        }
    }

    // Self-overlap check routes the ONE crossing owner; resolution routes the arrangement ring-direct.
    static Fin<Seq<Chain>> Resolve(Seq<Polyline> loops, Op? key) {
        bool overlapping = loops.Exists(loop => {
            int n = loop.Count - 1;
            for (int i = 0; i < n; i++) {
                for (int j = i + 2; j < n; j++) {
                    if (i == 0 && j == n - 1) { continue; }
                    Fin<IntersectResult> hit = Intersection.Apply(new IntersectOp.SegmentSegment(new Line(loop[i], loop[i + 1]), new Line(loop[j], loop[j + 1]), Axis.Z, IntersectPolicy.Canonical), key);
                    if (hit.Case is IntersectResult.Points { Hits.IsEmpty: false }) { return true; }
                }
            }
            return false;
        });
        return overlapping
            ? Arrangement.Apply(new ArrangementOp.PlanarOverlay(loops, Seq<Polyline>(), BooleanOp.Union, Axis.Z, ArrangementPolicy.Canonical), key)
                .Bind(static result => result is ArrangementResult.Overlay overlay
                    ? Fin.Succ(overlay.Loops)
                    : Fin.Fail<Seq<Chain>>(new GeometryFault.DegenerateOffset(0, 0.0).ToError()))
            : Fin.Succ(loops.Map(static loop => new Chain(loop, Closed: true)));
    }

    // --- [MEDIAL]
    // REAL against the delaunay dual: interior circumcenters ARE medial samples carrying their
    // circumradius as the clearance payload; the dual's curved sampling carries the parabolic
    // reflex arcs the linear skeleton approximates.
    static Fin<SkeletonGraph> MedialOf(Polyline ring, Op? key) {
        int n = ring.Count - 1;
        Implicit[] rows = [.. Enumerable.Range(0, n).Select(i => new Implicit(ring[i]))];
        Seq<Constraint> edges = toSeq(Enumerable.Range(0, n).Select(i => (Constraint)new Constraint.Segment(i, (i + 1) % n)));
        return Tessellation.Build(new TessellationOp.Points(TessellationKind.Triangulation, rows, edges, TessellationPolicy.Canonical, Axis.Z), key)
            .Bind(t => t.VoronoiDual(key).Map(dual => (Tess: t, Dual: dual)))
            .Bind(pair => pair.Tess.Triangles(key).Map(tris => (pair.Dual, Tris: tris)))
            .Map(x => {
                bool[] interior = [.. x.Tris.Select(tri => Inside(Centroid(tri), ring))];
                Dictionary<int, int> keep = new();
                List<ClearanceNode> nodes = new();
                for (int i = 0; i < x.Dual.Circumcenters.Length; i++) {
                    if (i < interior.Length && interior[i]) {
                        keep[i] = nodes.Count;
                        (double radius, int edge) = EdgeDistance(ring, x.Dual.Circumcenters[i]);
                        nodes.Add(new ClearanceNode(x.Dual.Circumcenters[i], double.Min(radius, x.Dual.Radius[i]), edge));
                    }
                }
                List<SkeletonArc> arcs = new();
                for (int e = 0; e < x.Dual.Edges.Length; e++) {
                    (int a, int b) = x.Dual.Edges[e];
                    if (keep.TryGetValue(a, out int ka) && keep.TryGetValue(b, out int kb)) { arcs.Add(new SkeletonArc(ka, kb, x.Dual.Across[e].U)); }
                }
                return new SkeletonGraph(toSeq(nodes), toSeq(arcs));
            });
    }

    // --- [MINKOWSKI]
    // The COMPLETE convolution as a SUPPORT-VERTEX walk — a global two-pointer normal merge is
    // unsound for non-convex rings (their normal sequence is not angle-sorted): each ring edge
    // translates by the element vertex EXTREME under its outward normal, and at each ring vertex
    // the element boundary walks from the previous support to the next — CCW at convex turns (the
    // vertex fan), CW at reflex turns (the reversed arc) — so BOTH convolution directions emit
    // from one walk, connected by construction; the possibly self-overlapping cycle resolves
    // through the arrangement's nonzero winding. The element is gated CLOSED + CCW-oriented +
    // CONVEX by exact turn signs (a reflex element routes the typed fault — its convex
    // decomposition is the recorded growth row); support ties on an edge-parallel normal differ by
    // a degenerate sliver the resolve removes.
    static Fin<OffsetCurves> Convolve(Polyline ring, Polyline element, Op? key) {
        if (element.Count < 4 || !element.IsClosed) { return Fin.Fail<OffsetCurves>(new GeometryFault.DegenerateOffset(0, 0.0).ToError()); }
        Polyline b = Oriented(element);
        int rn = ring.Count - 1, en = b.Count - 1;
        for (int j = 0; j < en; j++) {
            if (Predicate.Orient2D(b[(j - 1 + en) % en], b[j], b[(j + 1) % en]) == Sign.Negative) {
                return Fin.Fail<OffsetCurves>(new GeometryFault.DegenerateOffset(j, 0.0).ToError());
            }
        }
        int Support(Vector3d outward) {
            (int best, double reach) = (0, double.NegativeInfinity);
            for (int j = 0; j < en; j++) {
                double dot = (b[j].X * outward.X) + (b[j].Y * outward.Y);
                if (dot > reach) { (best, reach) = (j, dot); }
            }
            return best;
        }
        Span<int> support = new int[rn];
        for (int i = 0; i < rn; i++) { support[i] = Support(Normal(ring[i], ring[(i + 1) % rn])); }
        Polyline cycle = new();
        for (int i = 0; i < rn; i++) {
            int from = support[(i - 1 + rn) % rn], to = support[i];
            bool convex = Predicate.Orient2D(ring[(i - 1 + rn) % rn], ring[i], ring[(i + 1) % rn]) != Sign.Negative;
            for (int k = from, step = 0; k != to && step <= en; k = (k + (convex ? 1 : en - 1)) % en, step++) {
                cycle.Add(ring[i] + (b[k] - Point3d.Origin));  // the fan (CCW) or reversed arc (CW) at the vertex
            }
            cycle.Add(ring[i] + (b[to] - Point3d.Origin));
            cycle.Add(ring[(i + 1) % rn] + (b[to] - Point3d.Origin));  // the translated edge under its support
        }
        cycle.Add(cycle[0]);
        return Arrangement.Apply(new ArrangementOp.PlanarOverlay(Seq(cycle), Seq<Polyline>(), BooleanOp.Union, Axis.Z, ArrangementPolicy.Canonical), key)
            .Bind(static result => result is ArrangementResult.Overlay overlay
                ? Fin.Succ(new OffsetCurves(overlay.Loops, 0.0))
                : Fin.Fail<OffsetCurves>(new GeometryFault.DegenerateOffset(0, 0.0).ToError()));
    }

    // --- [CLEARANCE]
    // The arbitrary-probe query of the minted family: minimum point-segment distance + witness.
    static ClearanceNode ClearanceAt(Polyline ring, Point3d probe) {
        (double radius, int edge) = EdgeDistance(ring, probe);
        return new ClearanceNode(probe, radius, edge);
    }

    static (double Radius, int Edge) EdgeDistance(Polyline ring, Point3d p) {
        (double best, int at) = (double.PositiveInfinity, 0);
        for (int e = 0; e < ring.Count - 1; e++) {
            double d = new Line(ring[e], ring[e + 1]).DistanceTo(p, limitToFiniteSegment: true);
            if (d < best) { (best, at) = (d, e); }
        }
        return (best, at);
    }

    // --- [PRIMITIVES]
    // The RIGHT/OUTWARD normal of (a -> b) on a CCW ring — the ribbon and support lanes grow along
    // it; the wavefront's INWARD normals are its negation, spelled at their own sites.
    static Vector3d Normal(Point3d a, Point3d b) => new(b.Y - a.Y, a.X - b.X, 0.0);
    static Vector3d Unit(Vector3d v) { double len = v.Length; return len == 0.0 ? v : (1.0 / len) * v; }

    static double SignedArea(Polyline ring) {
        double sum = 0.0;
        for (int i = 0; i < ring.Count - 1; i++) { sum += (ring[i].X * ring[i + 1].Y) - (ring[i + 1].X * ring[i].Y); }
        return 0.5 * sum;
    }

    static Polyline Oriented(Polyline ring) {
        if (SignedArea(ring) >= 0.0) { return ring; }
        Polyline reversed = new(ring);
        reversed.Reverse();
        return reversed;
    }

    static bool Inside(Point3d probe, Polyline ring) {
        bool inside = false;
        for (int e = 0; e < ring.Count - 1; e++) {
            (Point3d a, Point3d b) = (ring[e], ring[e + 1]);
            if ((a.Y > probe.Y) == (b.Y > probe.Y)) { continue; }
            Sign side = Predicate.Orient2D(a, b, probe);
            if (side.Times(Sign.Of(b.Y.CompareTo(probe.Y))) == Sign.Negative) { inside = !inside; }
        }
        return inside;
    }

    static Point3d Centroid((Point3d A, Point3d B, Point3d C) tri) =>
        new((tri.A.X + tri.B.X + tri.C.X) / 3.0, (tri.A.Y + tri.B.Y + tri.C.Y) / 3.0, (tri.A.Z + tri.B.Z + tri.C.Z) / 3.0);
}
```

```mermaid
flowchart LR
    OffsetOp -->|admit: simplicity via Intersection.Apply| Intersection
    OffsetOp -->|Seed bisectors, EdgeSpeed rows| WavefrontStore
    WavefrontStore -->|exact Orient2D reflex/turn signs| Predicate
    WavefrontStore -->|time-ordered Edge/Split events| OffsetEvent
    OffsetEvent -->|ClearanceNode + SkeletonArc| SkeletonGraph
    OffsetOp -->|CDT + VoronoiDual| Tessellation
    Tessellation -->|interior circumcenters + radii| Medial["SkeletonGraph (medial)"]
    WavefrontStore -->|multi-ring snapshot at t| Dressing["JoinType x EndType rows"]
    Dressing -->|self-overlap resolve| Arrangement
    Arrangement -->|oriented Chain loops| OffsetCurves
    OffsetOp -.->|2416 / 2417 / 2418| GeometryFault
```

## [03]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes.

| [INDEX] | [AXIS/CONCERN]    | [OWNER]          | [KIND]                                                                                          | [RAIL]                                     | [CASES] |
| :-----: | :---------------- | :--------------- | :------------------------------------------------------------------------------------------------ | :------------------------------------------- | :-----: |
|  [01]   | Offsetting        | `OffsetOp`       | `[Union]` (`Skeleton`/`Weighted`/`Offset`/`Medial`/`Minkowski`/`Clearance`) folded by ONE `Apply` | `Offsetting.Apply → Fin<OffsetResult>`     |    6    |
|  [1a]   | Corner generator  | `JoinType`       | `[SmartEnum<string>]` — each row carries its `Corner` emission delegate                          | policy rows (the next join is a row)       |    4    |
|  [1b]   | Cap generator     | `EndType`        | `[SmartEnum<string>]` — each row carries its `Cap` emission delegate                             | policy rows                                |    4    |
|  [1c]   | Clearance family  | `ClearanceNode`  | THE minted 2D/3D clearance carrier — position · radius · nearest-feature witness                 | result rows (skeleton.md composes, W4)     |    —    |
|  [1d]   | Skeleton graph    | `SkeletonGraph`  | ONE graph shape, skeleton AND medial — ring seeds are radius-zero nodes, arcs node-uniform       | result carrier (`Graph`/`Axis` cases)      |    —    |
|  [1e]   | Wavefront arena   | `WavefrontStore` | single-writer SoA arena, amortized-doubling `Spawn`, ring links, node/edge provenance, elevation | arena (trace projections)                  |    —    |
|  [1f]   | Event algebra     | `OffsetEvent`    | `[Union]` (`Edge`/`Split`) drained time-ordered                                                  | carrier (drained in `Propagate`)           |    2    |

The prior fence's `SegmentsCross` copy, `Mesh.CreateFromClosedPolyline` loop path, discarded medial tessellation, single-direction fan-less Minkowski, first-ring-only snapshot, `2n` store sizing, decorative exact-zero event guard, outward-normal "inward" bisectors, unit-speed respawns under the weighted lane, ring-index/node-index pun, unconsulted `OffsetKind` vocabulary, and field-identical `MedialAxis` sibling record are all deleted; every deletion lands as a composition of the sibling owner that already carries the concern or a correction inside this one.

## [04]-[RESEARCH]

- [WAVEFRONT_HONESTY] — the Aichholzer-Aurenhammer propagation is exact where exactness is structural and analytic where it is schedule: `IsReflex` and the split-hit admission read exact `Orient2D` turn signs (a mis-classified reflex vertex produces a spurious split or a missed one — the failure class the predicate floor closes), while candidate event TIMES are analytic ray intersections whose staleness is caught at fire by liveness + ring-adjacency + collapse-band validation — a superseded event is skipped by structure, never by an epsilon guess, and the prior fence's exact-sign test over float trajectory positions (which could essentially never fire and silently skipped collapse events) is the deleted illusory form. Zero-progress livelock (a same-time re-enqueue cycle) routes `CollapseStalled(iteration, residual)`; budget exhaustion routes `SkeletonStalled(pending, time)`. The `Weighted` row is the SAME queue at non-unit `EdgeSpeed` — the `EdgeOf` original-edge column keys every respawned vertex's bisector and every split's opposing-edge closure to its OWN speed through every rewire (a unit-speed respawn under a weighted front is the deleted hollow form), and its skeleton nodes recompute the TRUE Euclidean clearance radius so the minted family never carries a weighted time as a distance. The snapshot dressing is the arc law made data: a reflex corner's exact inward offset is the circle of radius `Distance − spawn` centred on its emanating node — the moving edges stay tangent to it — so the lane emits the two tangent points and the `JoinType` row emits only the interior fan. The law-matrix (`SkeletonLaws`, CsCheck under `testing-cs`) asserts arcs partition the interior, per-edge monotone faces, termination within budget, weighted determinism, and that every snapshot ring is simple.
- [CLEARANCE_FAMILY] — `ClearanceNode(At, Radius, NearestEdge)` is the kernel's ONE clearance vocabulary: skeleton nodes carry collapse-time radii (unit speed = boundary distance), medial nodes carry circumradii off the dual, the `Clearance(probe)` case answers arbitrary probes with the exact minimum edge distance and its witness — and `Meshing/skeleton` (W4) emits its 3D MCF nodes as the SAME family, so the `Rasm.Fabrication` toolpath seam (`FAB:22`) reads one payload across dimensionalities: per-point clearance radius plus arbitrary-probe query, the `[V5]` weighted/variable-speed demands riding `EdgeSpeed` and the existing `Weighted` case.
- [MEDIAL_VIA_DUAL] — the medial axis composes the delaunay `VoronoiDual`: the ring's CDT interior circumcenters sample the true bisector locus — including the PARABOLIC point-edge bisectors at reflex fans the linear straight skeleton only approximates — each dual node carrying its circumradius, tightened against the exact edge-distance scan, as the clearance payload; interior selection is the exact-sign ray parity of each triangle centroid. The straight skeleton and the medial coincide on convex input and diverge exactly at reflex vertices — the two projections serve different consumers (offset assembly wants the skeleton's monotone faces; toolpath clearance wants the medial's true radii) off one wavefront + one dual, never two skeleton engines. The exact circumcenter side-of trim is the recorded growth row on the dual.
- [CONVOLUTION_AND_RESOLUTION] — the Minkowski convolution is one SUPPORT-VERTEX walk over the CCW ring and the CCW-convex element: each ring edge translates by the element vertex EXTREME under the edge's outward normal, and each ring vertex connects consecutive supports along the element boundary — CCW at convex turns (the vertex fan) and CW at reflex turns (the reversed arc), so BOTH convolution directions and every fan emit from one walk, the cycle connected by construction; a global two-pointer normal merge is UNSOUND here because a non-convex ring's edge-normal sequence is not angle-sorted — the deleted prior form. The element is gated CLOSED + CCW-oriented + CONVEX by exact turn signs (a reflex element faults typed, its convex decomposition the recorded growth row); a support TIE on an edge-parallel normal resolves either way and differs by a degenerate sliver the resolve removes — support selection is assembly scheduling, never a sign decision. The self-overlapping convolution cycle resolves through `Arrangement.Apply` `PlanarOverlay` under the NONZERO winding rule — even-odd would punch false holes where the convolution winds twice, so the arrangement's ring classification counts signed crossings. The same resolution serves the offset snapshot: any self-overlapping offset loop set routes the arrangement ring-direct — the prior `CreateFromClosedPolyline` bridge failed on exactly the self-intersecting loops the resolve exists for and is deleted as a circular dependency, not merely as noise.
