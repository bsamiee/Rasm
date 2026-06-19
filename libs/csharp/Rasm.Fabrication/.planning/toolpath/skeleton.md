# [RASM_FABRICATION_SKELETON]

The straight-skeleton/medial-axis author-kernel: `StraightSkeleton` the static surface computing the medial axis of a closed polygon by the wavefront-propagation construction, the primitive driving the `Toolpath/motion#CAM_MOTION` trochoidal adaptive-clearing strategy and the exact constant-offset contour where a naive per-vertex-normal offset self-intersects. This is the one fabrication kernel where the author-kernel posture is correct and forward: no managed straight-skeleton library exists on NuGet (CGAL is C++/GPL, carrying license and per-RID burden), so the primitive is authored from first principles atop the `Polygon/clipper#POLYGON_ALGEBRA` Clipper2 offset substrate and the kernel `Rasm.Geometry/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` exact orientation. The skeleton subsumes the self-intersecting `OffsetRing` the old CAM page hand-rolled: a constant-offset wavefront that collapses an edge or splits a reflex vertex is exactly the skeleton event the wavefront propagation resolves. It composes the `Process/owner#FABRICATION_OWNER` `Loop`/`Edge3` shared vocabulary; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The medial-axis `Edge3` set crosses only the in-process seam to the `Toolpath/motion#CAM_MOTION` trochoidal generator — never a browser or peer wire.

## [1]-[INDEX]

- [1]-[STRAIGHT_SKELETON]: owns `StraightSkeleton` — the `SkeletonEvent`/`Wavefront` propagation computing the medial axis and the exact constant-offset of a closed polygon; the trochoidal-clearing primitive.

## [2]-[STRAIGHT_SKELETON]

- Owner: `SkeletonEvent` `[Union]` the wavefront event (`Edge` an edge collapse keyed by its leading vertex index, `Split` a reflex-vertex split carrying the opposing edge and the exact hit point) the propagation resolves in time order; `Wavefront` the shrinking offset polygon state the propagation steps, each vertex moving along its angular bisector scaled to the inward edge-offset speed, with `Reflex` the exact `Orient2D` convexity verdict per vertex; `StraightSkeleton` the static surface owning `MedialAxis` (the skeleton arc set) and `OffsetAt` (the exact constant-offset polygon at a given inset, the offset that subsumes `OffsetRing`).
- Cases: `SkeletonEvent` arms `Edge` (an edge shrinks to zero length, merging its two neighbors at the meeting point of their bisectors) · `Split` (a reflex vertex's bisector reaches an opposing edge, splitting the wavefront into two by inserting the hit point) (2); the medial axis is the locus the propagating vertices trace between events, the skeleton arcs.
- Entry: `public static Fin<Seq<Edge3>> MedialAxis(Loop polygon)` and `public static Fin<Seq<Loop>> OffsetAt(Loop polygon, double inset)` — `Fin<T>` routes `FabricationFault.OpenLoop` on a non-closed polygon and the kernel `GeometryFault.DegenerateInput` on a zero-area or self-intersecting input, each lowered with `.ToError()`; `MedialAxis` returns the skeleton arc set, `OffsetAt` the exact offset polygon at the inset.
- Auto: `MedialAxis` runs the wavefront propagation — `Wavefront.Of` emits each vertex's angular bisector scaled by `1/sin(θ/2)` so a unit-time advance offsets every supporting edge inward by exactly one unit, and `Propagate` processes the earliest `SkeletonEvent` under a `Count²+8` step budget: `NextEvent` scans every adjacent-bisector meeting time (`RayMeet`, the `Edge` collapse) and every reflex-vertex-versus-non-incident-edge split time (`SplitMeet`, the `Split`), takes the earliest strictly-future event, `Advance` moves all vertices to that event time tracing each non-degenerate segment as a skeleton arc, and `Apply` drops the collapsed vertex (`Edge`) or inserts the split hit point (`Split`) and `Rebuild` recomputes the bisectors of the changed wavefront. `Reflex` classifies each vertex by `Predicate.Orient2D` exact sign and `SplitMeet` gates the opposing-edge side and the along-edge containment exactly, so a near-collinear bisector or a grazing split classifies deterministically and never seeds a phantom event. `OffsetAt` reads the wavefront state at the inset time through the `Polygon/clipper#POLYGON_ALGEBRA` `Offset` — for a convex inset this matches the Clipper2 offset, and the skeleton resolves the reflex-vertex split a per-vertex-normal offset cannot.
- Receipt: `MedialAxis` returns the `Edge3` arc set directly and `OffsetAt` the `Loop` set — the geometry IS the evidence the trochoidal generator reads; no generic skeleton ledger.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — settled, the bisector/side verdict), Clipper2 (via `Polygon/clipper#POLYGON_ALGEBRA` — the convex-inset offset cross-check), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a weighted straight skeleton (per-edge speed for variable-kerf offset) is one speed column on `Wavefront`; a polygon-with-holes medial axis is one hole-edge arm on the propagation; zero new surface.
- Boundary: `StraightSkeleton` is the ONE medial-axis owner and the trochoidal generator reads it, never a second skeleton routine; the `SkeletonEvent` time read dispatches through the generated total `Switch(edge:, split:)` and a `ev switch` pattern cascade with a `_` catch-all is the deleted form — a new event case fails the build until its arm lands; the convex-inset offset cross-checks the `Polygon/clipper#POLYGON_ALGEBRA` `Offset` and never re-mints the Clipper2 polygon offset for the convex case — the skeleton owns only the reflex-split and medial-axis construction Clipper2 does not expose; every bisector turn and opposing-edge side reads `Predicate.Orient2D` exact sign and a `double` cross at the call site is the named robustness defect; a hand-rolled per-vertex-normal `OffsetRing` is the deleted form this kernel subsumes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Frontier;
using Rasm.Fabrication.Geometry2D;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SkeletonEvent {
    private SkeletonEvent() { }

    public sealed record Edge(int VertexIndex, double Time) : SkeletonEvent;
    public sealed record Split(int ReflexVertex, int OpposingEdge, Point3d Hit, double Time) : SkeletonEvent;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Wavefront(Arr<Point3d> Vertices, Arr<Vector3d> Bisectors, double Time) {
    public int Count => Vertices.Count;
    public Point3d At(int i) => Vertices[((i % Count) + Count) % Count];
    public Vector3d Bisector(int i) => Bisectors[((i % Count) + Count) % Count];

    public static Wavefront Of(Loop polygon) {
        Loop ccw = polygon.AsCcw();
        var bisectors = toSeq(Enumerable.Range(0, ccw.Count)).Map(i => Bisect(ccw.At(i - 1), ccw.At(i), ccw.At(i + 1))).ToArr();
        return new Wavefront(ccw.Vertices, bisectors, 0.0);
    }

    public bool Reflex(int i) => Predicate.Orient2D(At(i - 1), At(i), At(i + 1)) == Sign.Negative;

    static Vector3d Bisect(Point3d prev, Point3d here, Point3d next) {
        Vector3d e0 = here - prev; e0.Unitize();
        Vector3d e1 = next - here; e1.Unitize();
        Vector3d sum = new(-(e0.Y + e1.Y), e0.X + e1.X, 0.0);
        double s = sum.Length;
        if (s < 1e-12) { Vector3d n = new(-e1.Y, e1.X, 0.0); n.Unitize(); return n; }
        double half = Math.Max(1e-9, Math.Sin(0.5 * Vector3d.VectorAngle(e0, e1) + Math.PI / 2.0));
        sum *= 1.0 / s / half;
        return sum;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class StraightSkeleton {
    public static Fin<Seq<Edge3>> MedialAxis(Loop polygon) =>
        !polygon.Closed
            ? Fin.Fail<Seq<Edge3>>(FabricationFault.OpenLoop("skeleton:open").ToError())
            : polygon.Count < 3
                ? Fin.Fail<Seq<Edge3>>(GeometryFault.DegenerateInput("skeleton:degenerate").ToError())
                : Fin.Succ(Propagate(Wavefront.Of(polygon), Seq<Edge3>(), polygon.Count * polygon.Count + 8));

    public static Fin<Seq<Loop>> OffsetAt(Loop polygon, double inset) =>
        PolygonAlgebra.Offset(Seq(polygon), -Math.Abs(inset), OffsetEnds.Polygon);

    static Seq<Edge3> Propagate(Wavefront front, Seq<Edge3> arcs, int budget) =>
        budget <= 0 || front.Count < 3
            ? arcs
            : NextEvent(front).Match(
                None: () => arcs,
                Some: ev => {
                    var (advanced, traced) = Advance(front, EventTime(ev));
                    Wavefront resolved = Apply(advanced, ev);
                    return Propagate(resolved, arcs.Concat(traced), budget - 1);
                });

    static Option<SkeletonEvent> NextEvent(Wavefront front) =>
        toSeq(toSeq(Enumerable.Range(0, front.Count))
            .Map(i => EdgeEvent(front, i))
            .Concat(toSeq(Enumerable.Range(0, front.Count)).Filter(front.Reflex).Bind(r => SplitEvents(front, r)))
            .Filter(ev => EventTime(ev) > 1e-9)
            .OrderBy(EventTime))
            .HeadOrNone();

    static SkeletonEvent EdgeEvent(Wavefront front, int i) {
        double t = RayMeet(front.At(i), front.Bisector(i), front.At(i + 1), front.Bisector(i + 1));
        return new SkeletonEvent.Edge(i, t > 1e-9 ? front.Time + t : double.PositiveInfinity);
    }

    static Seq<SkeletonEvent> SplitEvents(Wavefront front, int r) =>
        toSeq(Enumerable.Range(0, front.Count))
            .Filter(e => e != r && e != ((r - 1 + front.Count) % front.Count))
            .Map(e => {
                var hit = SplitMeet(front.At(r), front.Bisector(r), front.At(e), front.At(e + 1), front.Time);
                return hit.Map(h => (SkeletonEvent)new SkeletonEvent.Split(r, e, h.Point, h.Time));
            })
            .Somes();

    static (Wavefront Advanced, Seq<Edge3> Arcs) Advance(Wavefront front, double time) {
        double dt = time - front.Time;
        var moved = toSeq(Enumerable.Range(0, front.Count)).Map(i => front.At(i) + dt * front.Bisector(i)).ToArr();
        Seq<Edge3> traced = toSeq(Enumerable.Range(0, front.Count))
            .Map(i => new Edge3(front.At(i), moved[i]))
            .Filter(a => a.A.DistanceTo(a.B) > 1e-9);
        return (new Wavefront(moved, front.Bisectors, time), traced);
    }

    static Wavefront Apply(Wavefront front, SkeletonEvent ev) =>
        ev.Switch(
            edge:  e => Rebuild(Drop(front, (e.VertexIndex + 1) % front.Count)),
            split: s => Rebuild(front with { Vertices = front.Vertices.Insert(s.OpposingEdge + 1, s.Hit) }));

    static Wavefront Drop(Wavefront front, int i) =>
        front with { Vertices = front.Vertices.RemoveAt(i) };

    static Wavefront Rebuild(Wavefront front) {
        var verts = front.Vertices;
        var bis = toSeq(Enumerable.Range(0, verts.Count))
            .Map(i => Wavefront.Of(new Loop(verts, Closed: true)).Bisector(i)).ToArr();
        return new Wavefront(verts, bis, front.Time);
    }

    static double RayMeet(Point3d pa, Vector3d da, Point3d pb, Vector3d db) {
        double denom = da.X * db.Y - da.Y * db.X;
        if (Math.Abs(denom) < 1e-12) return double.PositiveInfinity;
        double t = ((pb.X - pa.X) * db.Y - (pb.Y - pa.Y) * db.X) / denom;
        return t > 1e-9 ? t : double.PositiveInfinity;
    }

    static Option<(Point3d Point, double Time)> SplitMeet(Point3d v, Vector3d dir, Point3d e0, Point3d e1, double now) {
        if (Predicate.Orient2D(e0, e1, v) != Sign.Positive) return None;
        Vector3d edge = e1 - e0; edge.Unitize();
        Vector3d nrm = new(-edge.Y, edge.X, 0.0);
        double speed = 1.0 - dir * nrm;
        if (Math.Abs(speed) < 1e-9) return None;
        double dist = (v - e0) * nrm;
        double t = dist / speed;
        if (t <= 1e-9) return None;
        Point3d hit = v + t * dir;
        double along = (hit - e0) * edge;
        return along >= -1e-9 && along <= e0.DistanceTo(e1) + 1e-9 ? Some((hit, now + t)) : None;
    }

    static double EventTime(SkeletonEvent ev) =>
        ev.Switch(edge: static e => e.Time, split: static s => s.Time);
}
```

## [3]-[RESEARCH]

- [SKELETON_EVENTS] The wavefront-event resolution is realized as the `NextEvent`/`Advance`/`Apply` author-kernel grounded on the Aichholzer-Aurenhammer construction over the exact `Predicate.Orient2D` floor: `NextEvent` is the earliest-event scheduler over the `RayMeet` edge-collapse and `SplitMeet` reflex-split candidates, `Advance` is the unit-speed bisector march tracing the arcs, and `Apply` is the vertex-drop / hit-insert wavefront edit with `Rebuild` bisector recomputation. The two settled numeric assumptions are the bisector speed scale `1/sin(θ/2)` (so unit time equals unit inset) and the `Count²+8` propagation budget bounding the event count on a simple polygon; the convex case cross-checks the `Polygon/clipper#POLYGON_ALGEBRA` `Offset` as the correctness witness, and the reflex-split case is the genuinely-new author-kernel content no managed library carries.
