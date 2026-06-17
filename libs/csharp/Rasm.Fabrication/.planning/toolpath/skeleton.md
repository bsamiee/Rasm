# [RASM_FABRICATION_SKELETON]

The straight-skeleton/medial-axis author-kernel: `StraightSkeleton` the static surface computing the medial axis of a closed polygon by the wavefront-propagation construction, the primitive driving the `toolpath/motion#CAM_MOTION` trochoidal adaptive-clearing strategy and the exact constant-offset contour where a naive per-vertex-normal offset self-intersects. This is the one fabrication kernel where the author-kernel posture is correct and forward: no managed straight-skeleton library exists on NuGet (CGAL is C++/GPL, carrying license and per-RID burden), so the primitive is authored from first principles atop the `geometry2d/clipper#POLYGON_ALGEBRA` Clipper2 offset substrate and the kernel `Rasm/Geometry/geometry-kernel#ROBUST_PREDICATES` `Predicate.Orient2D` exact orientation. The skeleton subsumes the self-intersecting `OffsetRing` the old CAM page hand-rolled: a constant-offset wavefront that collapses an edge or splits a reflex vertex is exactly the skeleton event the wavefront propagation resolves. It composes the `frontier/owner#FABRICATION_OWNER` `Loop`/`Edge3` shared vocabulary; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The medial-axis `Edge3` set crosses only the in-process seam to the `toolpath/motion#CAM_MOTION` trochoidal generator — never a browser or peer wire.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                                          |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | STRAIGHT_SKELETON | `StraightSkeleton` wavefront-propagation medial axis of a closed polygon; the trochoidal-clearing primitive |

## [2]-[STRAIGHT_SKELETON]

- Owner: `SkeletonEvent` `[Union]` the wavefront event (`Edge` an edge collapse, `Split` a reflex-vertex split) the propagation resolves in time order; `Wavefront` the shrinking offset polygon state the propagation steps; `StraightSkeleton` the static surface owning `MedialAxis` (the skeleton arc set) and `OffsetAt` (the exact constant-offset polygon at a given inset, the offset that subsumes `OffsetRing`).
- Cases: `SkeletonEvent` arms `Edge` (an edge shrinks to zero length, merging its two neighbors) · `Split` (a reflex vertex's bisector reaches an opposing edge, splitting the wavefront into two) (2); the medial axis is the locus the propagating vertices trace, the skeleton arcs.
- Entry: `public static Fin<Seq<Edge3>> MedialAxis(Loop polygon)` and `public static Fin<Seq<Loop>> OffsetAt(Loop polygon, double inset)` — `Fin<T>` routes `FabricationFault.OpenLoop` on a non-closed polygon and the kernel `GeometryFault.DegenerateInput` on a zero-area or self-intersecting input, each lowered with `.ToError()`; `MedialAxis` returns the skeleton arc set, `OffsetAt` the exact offset polygon at the inset.
- Auto: `MedialAxis` runs the wavefront propagation — each polygon vertex emits an angular bisector, the wavefront shrinks all edges inward at unit speed, and the propagation processes `SkeletonEvent`s in time order: an `Edge` event collapses a shrinking edge and merges its neighbors, a `Split` event resolves a reflex vertex reaching an opposing edge and splits the wavefront; the arcs the vertices trace between events are the skeleton `Edge3` set. The exact orientation of each bisector turn and the side of each opposing edge read `Predicate.Orient2D`, so a near-collinear bisector or a grazing split classifies deterministically. `OffsetAt` reads the wavefront state at the inset time, returning the offset polygon — for a convex inset this matches the `geometry2d/clipper#POLYGON_ALGEBRA` `Offset`, and the skeleton resolves the reflex-vertex split a per-vertex-normal offset cannot.
- Receipt: `MedialAxis` returns the `Edge3` arc set directly and `OffsetAt` the `Loop` set — the geometry IS the evidence the trochoidal generator reads; no generic skeleton ledger.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — settled, the bisector/side verdict), Clipper2 (via `geometry2d/clipper#POLYGON_ALGEBRA` — the convex-inset offset cross-check), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a weighted straight skeleton (per-edge speed for variable-kerf offset) is one speed column on `Wavefront`; a polygon-with-holes medial axis is one hole-edge arm on the propagation; zero new surface.
- Boundary: `StraightSkeleton` is the ONE medial-axis owner and the trochoidal generator reads it, never a second skeleton routine; the convex-inset offset cross-checks the `geometry2d/clipper#POLYGON_ALGEBRA` `Offset` and never re-mints the Clipper2 polygon offset for the convex case — the skeleton owns only the reflex-split and medial-axis construction Clipper2 does not expose; every bisector turn and opposing-edge side reads `Predicate.Orient2D` exact sign and a `double` cross at the call site is the named robustness defect; a hand-rolled per-vertex-normal `OffsetRing` is the deleted form this kernel subsumes.

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

    public sealed record Edge(int EdgeIndex, double Time) : SkeletonEvent;
    public sealed record Split(int ReflexVertex, int OpposingEdge, double Time) : SkeletonEvent;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Wavefront(Arr<Point3d> Vertices, Arr<Vector3d> Bisectors, double Time) {
    public static Wavefront Of(Loop polygon) {
        Loop ccw = polygon.AsCcw();
        var bisectors = toSeq(Enumerable.Range(0, ccw.Count)).Map(i => {
            Vector3d e0 = ccw.At(i) - ccw.At(i - 1); e0.Unitize();
            Vector3d e1 = ccw.At(i + 1) - ccw.At(i); e1.Unitize();
            Vector3d b = new(-(e0.Y + e1.Y), e0.X + e1.X, 0.0); b.Unitize();
            return b;
        }).ToArr();
        return new Wavefront(ccw.Vertices, bisectors, 0.0);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class StraightSkeleton {
    public static Fin<Seq<Edge3>> MedialAxis(Loop polygon) =>
        !polygon.Closed
            ? Fin.Fail<Seq<Edge3>>(FabricationFault.OpenLoop("skeleton:open").ToError())
            : polygon.Count < 3
                ? Fin.Fail<Seq<Edge3>>(GeometryFault.DegenerateInput("skeleton:degenerate").ToError())
                : Fin.Succ(Propagate(Wavefront.Of(polygon), Seq<Edge3>()));

    public static Fin<Seq<Loop>> OffsetAt(Loop polygon, double inset) =>
        PolygonAlgebra.Offset(Seq(polygon), -Math.Abs(inset), OffsetEnds.Polygon);

    static Seq<Edge3> Propagate(Wavefront front, Seq<Edge3> arcs) {
        Option<SkeletonEvent> next = NextEvent(front);
        return next.Match(
            None: () => arcs,
            Some: ev => {
                var (advanced, traced) = Advance(front, EventTime(ev));
                Wavefront resolved = Apply(advanced, ev);
                return resolved.Vertices.Count < 3 ? arcs.Concat(traced) : Propagate(resolved, arcs.Concat(traced));
            });
    }

    static Option<SkeletonEvent> NextEvent(Wavefront front) => throw new NotImplementedException();
    static (Wavefront Advanced, Seq<Edge3> Arcs) Advance(Wavefront front, double time) => throw new NotImplementedException();
    static Wavefront Apply(Wavefront front, SkeletonEvent ev) => throw new NotImplementedException();
    static double EventTime(SkeletonEvent ev) => ev switch {
        SkeletonEvent.Edge e => e.Time,
        SkeletonEvent.Split s => s.Time,
        _ => double.PositiveInfinity,
    };
}
```

## [3]-[RESEARCH]

- [SKELETON_EVENTS] The wavefront-event resolution (`NextEvent`/`Advance`/`Apply`) is the author-kernel depth the trochoidal strategy requires: the earliest-event scheduling, the edge-collapse neighbor merge, and the reflex-vertex split each ground against the canonical Aichholzer-Aurenhammer straight-skeleton construction over the exact `Predicate.Orient2D` orientation floor; the convex case cross-checks the `geometry2d/clipper#POLYGON_ALGEBRA` `Offset` so the offset agreement is the correctness witness on a convex input, and the reflex-split case is the genuinely-new author-kernel content no managed library carries.
