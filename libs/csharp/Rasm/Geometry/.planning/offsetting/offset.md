# [RASM_OFFSETTING_OFFSET]

The predicate-exact offsetting owner that closes straight-skeleton, medial-axis, and Minkowski-sum/offset over ONE `OffsetOp` `[Union]` (`Skeleton`/`Medial`/`Minkowski`/`Offset`) driven by a wavefront-propagation event queue grounded on the `numerics/predicates#ROBUST_PREDICATES` exact `Orient2D`/`InCircle` predicate so a reflex-vertex split event never mis-fires on a near-collinear edge. No external geometry library is admitted (CGAL straight-skeleton is GPL and rejected, Clipper offsetting is float-epsilon and rejected for the robust core), so the shrinking-polygon edge/split-event queue (Aichholzer-Aurenhammer), the medial-axis read-off, and the Minkowski edge-normal convolution are authored from first principles over a flat `WavefrontStore` value layout. The page owns the `OffsetKind` discriminant (binding the sibling-owned `GeometryKeyPolicy` string-key comparer), the `WavefrontStore` struct-of-arrays propagation memory, the `OffsetOp` `[Union]` with its `Apply` rail, the `SkeletonGraph`/`MedialAxis`/`OffsetCurves` typed result carriers, and the `ToMesh`/`ToPolylines` projections that re-emit the result through the `Vectors` `MeshSpace`/`Polyline` seam.

The owner composes `Vectors` `Point3d`/`Vector3d`/`Polyline`/`MeshSpace` coordinates as settled vocabulary — read, compose, never re-mint — rides the `Predicate` exact-turn floor so the wavefront event ordering is deterministic, composes the `tessellation/delaunay#TESSELLATION` constrained-Delaunay arrangement as the medial-axis Voronoi-dual substrate, and operates on raw `double` only at the `Predicate` seam and the propagation-time inner loop (the sanctioned interior-double scope alongside `Expansion`/`ErrorBound` and the healing weld). Every failure routes the band-2400 `GeometryFault` union; the kernel computes no hash and mints no second identity. The `WavefrontStore`/result carriers are the hash-friendly immutable records the `topology/reconciliation#NAMING_HASH` `Encode` content-addresses through the `MeshSpace` projection; this owner content-addresses nothing itself.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                                                                                                                                                                                                                                               |
| :-----: | :--------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | OFFSETTING | `OffsetOp` `[Union]` (`Skeleton`/`Medial`/`Minkowski`/`Offset`) over one `WavefrontStore`; the Aichholzer-Aurenhammer wavefront edge/split-event queue driven by the exact `Orient2D` turn sign; the medial-axis read-off; the Minkowski edge-normal convolution; `ToMesh`/`ToPolylines` projections |

## [2]-[OFFSETTING]

- Owner: `OffsetKind` `[SmartEnum<string>]` the operation discriminant (`skeleton`/`medial`/`minkowski`/`offset`) binding the sibling-owned `GeometryKeyPolicy` (`faults/faults#FAULT_BAND`) as its string-key comparer carrying the per-kind `EmitsGraph`/`EmitsCurves` columns; `WavefrontVertex` the moving wavefront vertex (current position, the two incident edge directions, the bisector velocity, the spawn time); `WavefrontStore` the struct-of-arrays flat propagation memory the event queue mutates — `Position`/`Velocity` coordinate slot arrays, `Prev`/`Next` the doubly-linked active-wavefront ring, `Dead` plus the free list reuse a collapsed vertex slot, `SpawnTime`/`Origin` the skeleton-arc provenance; `OffsetEvent` the closed `[Union]` event algebra (`Edge` collapse, `Split` reflex-vertex hit) the time-ordered priority queue folds; `OffsetOp` `[Union]` `Skeleton`/`Medial`/`Minkowski`/`Offset` carrying the input polygon ring and the per-op parameter; `SkeletonGraph` the straight-skeleton node/arc record (every wavefront vertex's swept trace), `MedialAxis` the trimmed skeleton-plus-Voronoi dual, `OffsetCurves` the offset-distance polyline family; `Offsetting` the static surface whose `Apply` fold runs the wavefront propagation and projects the requested result.
- Cases: `OffsetKind` rows `skeleton` · `medial` · `minkowski` · `offset` (4); `OffsetOp` cases `Skeleton` · `Medial` · `Minkowski` · `Offset` (4); `OffsetEvent` cases `Edge` · `Split` (2).
- Entry: `public static Fin<OffsetResult> Apply(OffsetOp op)` — the ONE offsetting entrypoint discriminating by `OffsetOp` case, `Fin<T>` routing a band-2400 `GeometryFault.DegenerateOffset` when the input ring is empty, self-intersecting, non-finite, or zero-area (no wavefront propagates), and `GeometryFault.SkeletonStalled` when the event queue stalls with pending events past the time budget (a wavefront that cannot resolve an event is a defect, never a silent truncation); the fold seeds the active wavefront from the input ring's edge bisectors, drains the time-ordered `OffsetEvent` queue (each event collapses an edge or splits a reflex chain, re-emitting the affected bisector velocities), and projects the swept traces into the requested `OffsetResult` case. No `BuildSkeleton`/`BuildMedial`/`BuildMinkowski` sibling entrypoints — one polymorphic `Apply` discriminates by kind.
- Auto: `Apply` seeds the `WavefrontStore` from the input polygon ring — each vertex's bisector velocity is the inward angle bisector of its two incident edges scaled to unit perpendicular edge speed, and the active wavefront is the doubly-linked ring of moving vertices — then folds the time-ordered `OffsetEvent` priority queue: an `Edge` event fires when a wavefront edge collapses to zero length (its two endpoints meet — the exact `Predicate.Orient2D` sign of the shrinking triangle confirms the collapse is a true meet, not a near-miss), emitting a skeleton node and re-linking the ring; a `Split` event fires when a reflex wavefront vertex's bisector reaches an opposing edge (the exact `Orient2D` turn sign decides the reflex classification so a near-collinear vertex never spuriously splits, and the exact in-edge containment confirms the hit lies within the opposing segment), splitting the wavefront ring into two and spawning two new vertices with re-computed bisectors. The event time of each candidate is the analytic ray-intersection time computed once and re-validated by the exact turn sign at fire; the queue is drained in non-decreasing time so the propagation is deterministic. `Skeleton` projects the full swept-trace `SkeletonGraph` (every wavefront vertex's origin→collapse arc); `Medial` trims the skeleton arcs incident only to reflex chains and reconciles against the `tessellation/delaunay#TESSELLATION` constrained-Delaunay Voronoi dual so the medial axis is the exact bisector locus, not the skeleton's reflex-biased subset; `Offset` reads the wavefront ring positions frozen at the propagation distance `t` (the offset polygon at distance `t` is the active ring snapshot, valid until the next event past `t`, so a variable-distance offset family is the ring snapshots across the event sequence); `Minkowski` is the edge-normal convolution of the input ring with the structuring element — each edge contributes its outward-normal-offset segment and each convex vertex its arc fan, the segments ordered by the exact `Orient2D` turn sign so the convolution boundary is simple. The four kinds share ONE wavefront propagation — `Skeleton`/`Medial`/`Offset` read the same swept trace at different projections, `Minkowski` is the dual convolution over the same exact-turn ordering — never four propagation kernels.
- Receipt: none on a dedicated rail — the `OffsetResult` `[Union]` (`Graph`/`Axis`/`Curves`) IS the typed result the projection re-emits; the `Apply` rail returns the result itself, and the `SkeletonGraph`/`MedialAxis`/`OffsetCurves` records ARE the hash-friendly immutable records the reconciliation `Encode` content-addresses through the `MeshSpace`/`Polyline` projection.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d`/`Polyline`/`MeshSpace` — composed for ring geometry and the result projection), `Rasm.Geometry.Numerics` (`Predicate` `Orient2D`/`InCircle` and `Sign` — the exact-turn floor, composed never re-minted), `Rasm.Geometry.Tessellation` (`Tessellation` constrained-Delaunay Voronoi dual — the medial-axis substrate, composed never re-minted), `Rasm.Geometry` (`GeometryKeyPolicy` string-key comparer — composed, never re-minted), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`PriorityQueue<TElement,TPriority>`, `Stack<T>`, `List<T>`).
- Growth: a new offsetting modality (a weighted/mitered straight-skeleton, a curved-input offset) is one `OffsetKind` row plus one `OffsetOp` case reading the SAME wavefront propagation — never a parallel skeletonizer class with a duplicated event queue (a fifth kind is admitted only by a charter amendment, never widened silently from this leaf page); a new event shape is one `OffsetEvent` case plus one drain arm; a new propagation knob is one column on `OffsetPolicy`; zero new surface.
- Boundary: the offsetting owner is the ONE polymorphic `OffsetOp` `[Union]` and a `StraightSkeleton`/`MedialAxis`/`MinkowskiSum`/`PolygonOffset` sibling-class family each carrying its own `Build`/`Run` surface is the named density defect collapsed here onto one union folded by one `Apply` entrypoint — the four kinds differ ONLY in their result projection (and `Minkowski` in its dual convolution form), never in the wavefront propagation, so `Apply`/`ToMesh`/`ToPolylines` live on the union base and read the shared `WavefrontStore` kind-agnostically; the wavefront event classification composes the `Predicate.Orient2D` exact-turn sign and a hand-rolled epsilon-tolerant reflex test (instead of `Predicate.Orient2D`) is the named correctness defect — a reflex vertex mis-classified by a loosened float turn produces a spurious split event or a missed collapse, exactly the non-robustness the predicate floor exists to eliminate, and the offset of a polygon self-intersects on reflex vertices precisely where the event ordering must be exact; the medial axis composes the `Tessellation` constrained-Delaunay Voronoi dual and a domain-local Voronoi re-implementation beside the tessellation owner is the deleted double-owner form; the `OffsetEvent` queue is the ONE event algebra the skeleton/medial/offset projections share and a separate `CollapseEvent`/`SplitEvent` pair across parallel queues is the deleted form; `Apply` is total over the `Fin` rail and a thrown exception on a degenerate ring or a stalled wavefront is forbidden — the defect routes `GeometryFault.DegenerateOffset`/`SkeletonStalled(...).ToError()` over the band-2400 union; the result re-emits the canonical hash-friendly `MeshSpace`/`Polyline` the `topology/reconciliation#NAMING_HASH` `Encode` content-addresses and this owner mints NO second hash; the bisector velocities, the event times, and the propagation-distance reads operate on raw `double` only at the `Predicate` seam and the propagation inner loop because a coordinate and a propagation time are the domain's native scalars (a coordinate is not a unit-bearing quantity), and a `double` crossing a public offsetting signature outside a `Point3d` coordinate or a distance is the seam violation; the offsetting preserves capability — a `Split` event divides a wavefront ring rather than discarding a reflex chain, so no propagation drops a polygon feature to satisfy a budget.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rasm.Geometry.Tessellation;
using Rasm.Vectors;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Geometry.Offsetting;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<GeometryKeyPolicy, string>]
[KeyMemberComparer<GeometryKeyPolicy, string>]
public sealed partial class OffsetKind {
    public static readonly OffsetKind Skeleton   = new("skeleton", emitsGraph: true, emitsCurves: false);
    public static readonly OffsetKind Medial     = new("medial", emitsGraph: true, emitsCurves: false);
    public static readonly OffsetKind Minkowski  = new("minkowski", emitsGraph: false, emitsCurves: true);
    public static readonly OffsetKind Offset     = new("offset", emitsGraph: false, emitsCurves: true);

    public bool EmitsGraph { get; }
    public bool EmitsCurves { get; }
}

// --- [CONSTANTS] --------------------------------------------------------------------------
public sealed record OffsetPolicy(double TimeBudget, int MaxEvents, double CollapseEpsilon) {
    public static readonly OffsetPolicy Canonical = new(TimeBudget: 1e9, MaxEvents: 1 << 20, CollapseEpsilon: 1e-12);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record WavefrontStore(
    int Count,
    double[] Position,
    double[] Velocity,
    int[] Prev,
    int[] Next,
    bool[] Dead,
    double[] SpawnTime,
    int[] Origin,
    Stack<int> FreeList) {
    public Point3d At(int vertex, double time) =>
        new(Position[2 * vertex] + (time - SpawnTime[vertex]) * Velocity[2 * vertex],
            Position[2 * vertex + 1] + (time - SpawnTime[vertex]) * Velocity[2 * vertex + 1], 0.0);

    internal int Spawn(Point3d position, Vector3d velocity, double spawnTime, int origin) {
        int vertex = FreeList.Count > 0 ? FreeList.Pop() : Count;
        (Position[2 * vertex], Position[2 * vertex + 1]) = (position.X, position.Y);
        (Velocity[2 * vertex], Velocity[2 * vertex + 1]) = (velocity.X, velocity.Y);
        (SpawnTime[vertex], Origin[vertex], Dead[vertex]) = (spawnTime, origin, false);
        return vertex;
    }

    internal void Kill(int vertex) { Dead[vertex] = true; FreeList.Push(vertex); }
}

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
    public sealed record Axis(MedialAxis Medial) : OffsetResult;
    public sealed record Curves(OffsetCurves Offset) : OffsetResult;
}

public sealed record SkeletonNode(Point3d Position, double Time);
public sealed record SkeletonArc(int From, int To, int OriginEdge);
public sealed record SkeletonGraph(Seq<SkeletonNode> Nodes, Seq<SkeletonArc> Arcs);
public sealed record MedialAxis(Seq<SkeletonNode> Nodes, Seq<SkeletonArc> Arcs, Seq<(int A, int B)> Bisectors);
public sealed record OffsetCurves(Seq<Polyline> Loops, double Distance);

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetOp {
    private OffsetOp() { }

    public sealed record Skeleton(Polyline Ring, OffsetPolicy Policy) : OffsetOp;
    public sealed record Medial(Polyline Ring, OffsetPolicy Policy) : OffsetOp;
    public sealed record Minkowski(Polyline Ring, Polyline Element, OffsetPolicy Policy) : OffsetOp;
    public sealed record Offset(Polyline Ring, double Distance, OffsetPolicy Policy) : OffsetOp;

    public OffsetKind Kind =>
        Switch(
            skeleton:  static _ => OffsetKind.Skeleton,
            medial:    static _ => OffsetKind.Medial,
            minkowski: static _ => OffsetKind.Minkowski,
            offset:    static _ => OffsetKind.Offset);

    Polyline Ring =>
        Switch(
            skeleton:  static s => s.Ring,
            medial:    static m => m.Ring,
            minkowski: static k => k.Ring,
            offset:    static o => o.Ring);

    OffsetPolicy Policy =>
        Switch(
            skeleton:  static s => s.Policy,
            medial:    static m => m.Policy,
            minkowski: static k => k.Policy,
            offset:    static o => o.Policy);
}

public static class Offsetting {
    public static Fin<OffsetResult> Apply(OffsetOp op) =>
        Validate(op).Bind(ring => op switch {
            OffsetOp.Minkowski k => MinkowskiConvolution(ring, k.Element).Map(static loops => (OffsetResult)new OffsetResult.Curves(loops)),
            OffsetOp.Offset o    => Propagate(ring, op.Kind, op.Policy).Map(trace => (OffsetResult)new OffsetResult.Curves(OffsetSnapshot(trace, ring, o.Distance))),
            OffsetOp.Medial _    => Propagate(ring, op.Kind, op.Policy).Bind(trace => MedialFrom(trace, ring)).Map(static axis => (OffsetResult)new OffsetResult.Axis(axis)),
            _                    => Propagate(ring, op.Kind, op.Policy).Map(static trace => (OffsetResult)new OffsetResult.Graph(trace.Graph)),
        });

    static Fin<Polyline> Validate(OffsetOp op) {
        Polyline ring = op.Ring;
        return ring.Count < 4 || !ring.IsClosed
            ? Fin.Fail<Polyline>(GeometryFault.DegenerateOffset($"ring:open-or-too-few:{ring.Count}").ToError())
            : SignedArea(ring) == 0.0
                ? Fin.Fail<Polyline>(GeometryFault.DegenerateOffset("ring:zero-area").ToError())
                : SimplePolygon(ring)
                    ? Fin.Succ(Orient(ring))
                    : Fin.Fail<Polyline>(GeometryFault.DegenerateOffset("ring:self-intersecting").ToError());
    }

    // --- [WAVEFRONT]
    static Fin<Trace> Propagate(Polyline ring, OffsetKind kind, OffsetPolicy policy) {
        WavefrontStore store = Seed(ring);
        var queue = new PriorityQueue<OffsetEvent, double>();
        var nodes = new List<SkeletonNode>();
        var arcs = new List<SkeletonArc>();
        EnqueueAll(store, queue, 0.0, policy);
        int fired = 0;
        while (queue.Count > 0) {
            if (fired++ > policy.MaxEvents) return Fin.Fail<Trace>(GeometryFault.SkeletonStalled(queue.Count, queue.Peek().Time).ToError());
            OffsetEvent ev = queue.Dequeue();
            if (ev.Time > policy.TimeBudget) return Fin.Fail<Trace>(GeometryFault.SkeletonStalled(queue.Count, ev.Time).ToError());
            store = ev switch {
                OffsetEvent.Edge e  => Collapse(store, e, nodes, arcs, queue, policy),
                OffsetEvent.Split s => Divide(store, s, nodes, arcs, queue, policy),
                _                   => store,
            };
        }
        return Fin.Succ(new Trace(store, new SkeletonGraph(toSeq(nodes), toSeq(arcs))));
    }

    static WavefrontStore Seed(Polyline ring);
    static void EnqueueAll(WavefrontStore store, PriorityQueue<OffsetEvent, double> queue, double now, OffsetPolicy policy);
    static WavefrontStore Collapse(WavefrontStore store, OffsetEvent.Edge ev, List<SkeletonNode> nodes, List<SkeletonArc> arcs, PriorityQueue<OffsetEvent, double> queue, OffsetPolicy policy);
    static WavefrontStore Divide(WavefrontStore store, OffsetEvent.Split ev, List<SkeletonNode> nodes, List<SkeletonArc> arcs, PriorityQueue<OffsetEvent, double> queue, OffsetPolicy policy);

    static bool Reflex(Polyline ring, int i) {
        int n = ring.Count - 1;
        Point3d prev = ring[(i - 1 + n) % n], cur = ring[i % n], next = ring[(i + 1) % n];
        return Predicate.Orient2D(prev, cur, next) == Sign.Negative;
    }

    // --- [PROJECTIONS]
    static Fin<MedialAxis> MedialFrom(Trace trace, Polyline ring);
    static OffsetCurves OffsetSnapshot(Trace trace, Polyline ring, double distance);

    static Fin<OffsetCurves> MinkowskiConvolution(Polyline ring, Polyline element) {
        var segments = new List<(Point3d A, Point3d B)>();
        int rn = ring.Count - 1, en = element.Count - 1;
        for (int i = 0; i < rn; i++) {
            Vector3d edge = ring[i + 1] - ring[i];
            Vector3d normal = new(edge.Y, -edge.X, 0.0);
            for (int j = 0; j < en; j++)
                if (Predicate.Orient2D(Point3d.Origin, ToPoint(normal), element[j]) != Sign.Negative)
                    segments.Add((ring[i] + (element[j] - Point3d.Origin), ring[i + 1] + (element[j] - Point3d.Origin)));
        }
        return Assemble(segments).Map(loops => new OffsetCurves(loops, 0.0));
    }

    static Fin<Seq<Polyline>> Assemble(List<(Point3d A, Point3d B)> segments);

    // --- [PRIMITIVES]
    static Point3d ToPoint(Vector3d v) => new(v.X, v.Y, v.Z);

    static double SignedArea(Polyline ring) {
        double sum = 0.0;
        for (int i = 0; i < ring.Count - 1; i++) sum += (ring[i].X * ring[i + 1].Y) - (ring[i + 1].X * ring[i].Y);
        return 0.5 * sum;
    }

    static Polyline Orient(Polyline ring) {
        if (SignedArea(ring) >= 0.0) return ring;
        var reversed = new Polyline(ring);
        reversed.Reverse();
        return reversed;
    }

    static bool SimplePolygon(Polyline ring) {
        int n = ring.Count - 1;
        for (int i = 0; i < n; i++)
            for (int j = i + 2; j < n; j++) {
                if (i == 0 && j == n - 1) continue;
                if (SegmentsCross(ring[i], ring[i + 1], ring[j], ring[j + 1])) return false;
            }
        return true;
    }

    static bool SegmentsCross(Point3d a, Point3d b, Point3d c, Point3d d) {
        Sign d1 = Predicate.Orient2D(a, b, c), d2 = Predicate.Orient2D(a, b, d);
        Sign d3 = Predicate.Orient2D(c, d, a), d4 = Predicate.Orient2D(c, d, b);
        return d1 != d2 && d3 != d4 && d1 != Sign.Zero && d2 != Sign.Zero && d3 != Sign.Zero && d4 != Sign.Zero;
    }
}

public readonly record struct Trace(WavefrontStore Store, SkeletonGraph Graph);
```

```mermaid
flowchart LR
    Polyline -->|Seed bisectors| WavefrontStore
    WavefrontStore -->|Orient2D turn sign| Predicate
    WavefrontStore -->|time-ordered events| OffsetEvent
    OffsetEvent -->|Edge collapse / Split| SkeletonGraph
    SkeletonGraph -->|Medial trim + Voronoi dual| Tessellation
    SkeletonGraph -->|ring snapshot at t| OffsetCurves
    OffsetCurves -->|ToMesh / ToPolylines| MeshSpace
    OffsetOp -.->|degenerate / stalled| GeometryFault
```

## [3]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes — `Fin`/`GeometryFault` where the wavefront can fail its post-condition, pure carriers for the projection.

| [INDEX] | [AXIS/CONCERN]  | [OWNER]       | [KIND]                                                                                   | [RAIL]                                 | [CASES] |
| :-----: | :-------------- | :------------ | :--------------------------------------------------------------------------------------- | :------------------------------------- | :-----: |
|   [1]   | Offsetting      | `OffsetOp`    | `[Union]` (`Skeleton`/`Medial`/`Minkowski`/`Offset`) over one `WavefrontStore` + `Apply` | `Offsetting.Apply → Fin<OffsetResult>` |    4    |
|  [1a]   | Operation kind  | `OffsetKind`  | `[SmartEnum<string>]` skeleton/medial/minkowski/offset + emits-graph/curves columns      | discriminant (pure)                    |    4    |
|  [1b]   | Wavefront event | `OffsetEvent` | `[Union]` (`Edge`/`Split`) folded by one time-ordered queue                              | carrier (drained in `Propagate`)       |    2    |

The `Apply` fold and the exact-turn-guarded `Validate`/`Reflex`/`SegmentsCross`/`SimplePolygon`/`MinkowskiConvolution` primitives are transcription-complete pure-managed fences composing the `numerics/predicates` exact-sign floor. The `[WAVEFRONT]` cluster (`Seed` bisector construction, `EnqueueAll` candidate-event computation, `Collapse` edge-event re-link, `Divide` split-event ring-split) and the `[PROJECTIONS]` cluster (`MedialFrom` skeleton-trim plus `Tessellation` Voronoi-dual reconciliation, `OffsetSnapshot` ring-snapshot at distance `t`, `Assemble` Minkowski segment-loop assembly) are signature-fixed with their bodies the algorithm the `[STRAIGHT_SKELETON]` and `[MEDIAL_AXIS]` contracts specify — pure-managed transcription targets over the shared `WavefrontStore`. None depends on a live-host member spelling beyond the stable native `Polyline`/`Mesh` surface the topology sibling already pins.

## [4]-[RESEARCH]

- [STRAIGHT_SKELETON] — the `Propagate` body is the Aichholzer-Aurenhammer shrinking-polygon wavefront: `Seed` builds the active wavefront ring with each vertex's inward angle-bisector velocity, `EnqueueAll` computes every edge-collapse and reflex-split candidate event time by analytic ray intersection, and the time-ordered queue drains `Collapse`/`Divide` arms re-emitting affected bisector velocities until the wavefront vanishes. The reflex classification and the split-hit containment are decided by the exact `Predicate.Orient2D` turn sign so a near-collinear vertex never spuriously splits and a near-miss never collapses — the entire robustness claim, since a float-epsilon turn test mis-orders events on a near-degenerate polygon and produces a self-intersecting skeleton. The tier-2 law-matrix (`SkeletonLaws`, a CsCheck property suite under `testing-cs`) asserts the swept-trace is a valid planar straight skeleton (every input edge contributes exactly one monotone-shrinking face, the skeleton arcs partition the polygon interior, no arc crosses another), the propagation terminates within the event budget on a simple polygon, rigid-transform invariance, and that an offset polygon read at distance `t` (`OffsetSnapshot`) is simple and non-self-intersecting (the property a naive float offset violates on reflex vertices). The kernel is total over the `Fin` rail and needs NO live-host probe — `Polyline`, `Predicate`, and the analytic event times are stable.
- [MEDIAL_AXIS] — the `MedialFrom` body trims the straight-skeleton arcs incident only to reflex chains and reconciles the remaining bisector locus against the `tessellation/delaunay#TESSELLATION` constrained-Delaunay Voronoi dual so the medial axis is the EXACT bisector locus (the straight skeleton and the medial axis coincide on convex polygons but diverge at reflex vertices, where the medial axis carries a parabolic arc the linear straight-skeleton approximates — the Voronoi dual supplies the exact bisector). The owner composes the `Tessellation` Voronoi dual and re-implementing Voronoi beside the tessellation owner is the deleted double-owner form. The law-matrix asserts the medial axis is a deformation retract of the polygon interior and every interior point's nearest-boundary distance is realized by a medial-axis branch; no host probe — the `Tessellation` substrate and the `Predicate` floor are stable.
- [MINKOWSKI_CONVOLUTION] — `MinkowskiConvolution` is the edge-normal convolution of the input ring with the structuring element: each input edge contributes its element-translated segment where the element vertex is on the outward side (the exact `Predicate.Orient2D` sign of the edge normal against the element vertex orders the contributing segments), `Assemble` chains the contributing segments into simple boundary loops. The convolution boundary simplicity depends on the exact-turn ordering — a float test admits a wrong-side segment that breaks the loop. The law-matrix asserts the Minkowski boundary is simple and the result equals the brute-force per-vertex-sum convex-hull union on convex inputs; no host probe.
- [OFFSETTING_CONSUMERS] — the offsetting substrate ALIGNS to its consumers through their own owners, never by coupling into the wavefront interior: the AEC-domain fabrication/nesting and facade-panelization packages consume `Offsetting.Apply` and the `OffsetCurves`/`SkeletonGraph` projections through the `Vectors` `MeshSpace`/`Polyline` seam; the `healing/repair#HEALING` rail composes the medial axis for a future skeleton-driven sliver-collapse verdict. Each consumer reaches the owner through `Apply`, never by reading the interior `WavefrontStore` — the alignment is a future wire on the consuming task, never a coupling edit into this page.
