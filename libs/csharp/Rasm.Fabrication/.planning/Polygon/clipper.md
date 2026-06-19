# [RASM_FABRICATION_CLIPPER]

The one 2D polygon-algebra substrate over Clipper2: `PolygonAlgebra` the single owner of offset/inflate, Boolean clip, Minkowski sum, and the open-path clip the projection kernel consumes. Clipper2 (Boost-1.0, dependency-free, integer-robust) owns all four primitives at decimal-precision-scaled integer arithmetic; the kernels that hand-rolled offsetting, the NFP Minkowski merge, and the HLR screen clip collapse onto this owner, per the `algorithms.md` rule that a hand-rolled kernel is admitted only after a benchmark defeats the library. The orientation verdict stays the kernel `Rasm.Geometry/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` exact sign — Clipper2 owns the polygon Boolean and the geometric construction, `Predicate.Orient2D` owns the winding/side verdict every kernel reads, and the two never overlap. The owner composes the `Process/owner#FABRICATION_OWNER` `Loop`/`Edge3` shared atoms, boundary-mapping each `Loop` to a Clipper2 `PathD`/`PathsD` at the one edge and back; it computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. `PolygonAlgebra` is a pure-managed in-process owner — Clipper2 is managed AnyCPU IL with no native asset and no RID burden. Its outputs are `Loop` sets the sibling fabrication kernels (`Toolpath/motion`, `Nesting/nfp`, `Posting/projection`, `Posting/program`) read; no result crosses a browser or peer wire.

## [01]-[INDEX]

- [01]-[POLYGON_ALGEBRA]: owns `PolygonAlgebra` over Clipper2 — the `ClipOp`/`OffsetEnds` axes and the `Offset`/`Clip`/`MinkowskiSum`/`ClipOpenPath` operations covering offset/inflate, Boolean clip, Minkowski sum (NFP), and the open-path screen clip; the one Clipper2 owner.

## [02]-[POLYGON_ALGEBRA]

- Owner: `ClipOp` `[SmartEnum<string>]` the Boolean operation axis (`union`/`intersect`/`difference`/`xor`) mapping to the Clipper2 `ClipType` row; `OffsetEnds` `[SmartEnum<string>]` the offset end/join axis (`polygon`/`open-round`/`open-butt`/`open-square`) mapping to the Clipper2 `EndType`/`JoinType` row; `PolygonAlgebra` the static surface owning `Offset`, `Clip`, `MinkowskiSum`, `ClipOpenPath`, and `Simplify`, each boundary-mapping a `Loop`/`Edge3` to a Clipper2 path and folding the result back to `Loop` sets.
- Cases: `ClipOp` rows `union` · `intersect` · `difference` · `xor` (4); `OffsetEnds` rows `polygon` (closed contour offset — kerf/contour rings) · `open-round` · `open-butt` · `open-square` (open-path offset — leads) (4); the four operations `Offset`/`Clip`/`MinkowskiSum`/`ClipOpenPath` are one owner each, never a per-caller bespoke routine.
- Entry: the four static operations return `Fin<Seq<Loop>>` (or `(Seq<Edge3> Inside, Seq<Edge3> Outside)` for `ClipOpenPath`) where a `GeometryFault.DegenerateInput(...).ToError()` routes on an empty or non-finite path set; the decimal `Precision.Digits` count (6) is the one Clipper2 robustness knob the facade and the `ClipperD` engine read to scale doubles into integer arithmetic and back.
- Auto: `Offset` maps each `Loop` to a Clipper2 `PathsD`, runs `Clipper.InflatePaths` with the `OffsetEnds` join/end row, the signed delta (negative shrinks inward for contour kerf, positive inflates for lead clearance), and the `precision` digit count, and folds the result paths back to `Loop` sets re-oriented through `AsCcw`; `Clip` runs `Clipper.BooleanOp` of the subject and clip path sets under the `ClipOp` row at the same precision; `MinkowskiSum` runs `Minkowski.Sum` of the fixed path and the (reflected) orbiting path at the same `Precision.Digits` decimal scale the offset and Boolean read — the `Clipper.MinkowskiSum` facade fixes `decimalPlaces` at the package default and is the deleted form — producing the NFP boundary the nesting kernel reads; `ClipOpenPath` drives a `ClipperD` engine — `AddOpenSubject` for the `Edge3`-as-open-path, `AddClip` for the occluder polygons, then `Execute(ClipType, FillRule.NonZero, PolyTreeD, openPaths)` collecting the surviving open sub-paths, run once as intersection (the inside/hidden runs) and once as difference (the outside/visible runs) the projection kernel splits. Every fold-back re-imposes the `Predicate.Orient2D` winding verdict; Clipper2's own orientation inference is never trusted as the domain sign.
- Receipt: each operation returns the typed `Loop`/`Edge3` set directly — the polygon set IS the evidence the consuming kernel reads; no generic polygon ledger and no Clipper2 `PathsD` escaping the owner.
- Packages: `Rasm`/Vectors (`Point3d`/`Vector3d`/`BoundingBox` — composed), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — settled, the winding/side verdict), Clipper2 (`Clipper2Lib` namespace — the `Clipper` facade `BooleanOp`/`InflatePaths`/`SimplifyPaths`, the `Minkowski` facade `Sum` carrying the `decimalPlaces` precision the `Clipper.MinkowskiSum` shorthand drops, the `ClipperD` engine `AddOpenSubject`/`AddClip`/`Execute` open-path overload, `ClipType`/`FillRule`/`JoinType`/`EndType`/`PathD`/`PathsD`/`PointD`/`PolyTreeD`; the one polygon-algebra substrate, this folder is its first admitter), LanguageExt.Core, BCL inbox.
- Growth: a new Boolean operation is one `ClipOp` row; a new offset end style is one `OffsetEnds` row; a higher-precision scale is one `Precision.Digits` constant change; the irregular/non-convex NFP (convex decomposition + per-piece Minkowski union) is one arm on `MinkowskiSum` over the same owner; a weighted/variable-kerf offset is one `Offset` arm driving the `ClipperOffset` engine's `DeltaCallback64` per-vertex variable delta (the weighted-straight-skeleton and torch-taper consumer); zero new surface.
- Boundary: `PolygonAlgebra` is the ONE Clipper2 owner and a second `Clipper`/`ClipperD` call site in any sibling kernel is the named duplication defect — the CAM contour offset, the NFP Minkowski merge, and the HLR screen clip all route here; the orientation verdict is the kernel `Predicate.Orient2D` exact sign and Clipper2's inferred orientation is never the domain sign; the `ToPath`/`FromPaths` boundary map is the one place `double` coordinates cross into the Clipper2 `PathD`/`PathsD` domain and back, and a `PathsD`/`Path64`/`PolyTreeD` type in a sibling kernel signature is the seam violation; Clipper2 exposes a public `Triangulate`/`TriangulateResult` surface in `2.0.0`/`2.0.1` that the author flags as buggy (open infinite-loop defects in the internal `Delaunay` kernel), so this owner exposes no triangulation arm and a 2D-meshing need routes to the kernel triangulation owner or a separate admission against a real triangulation library; a hand-rolled per-vertex-normal offset (`OffsetRing`), a hand-rolled angle-sorted edge merge (the old NFP construction), or a hand-rolled parameter-interval screen subtraction (`SpanInside`) is the deleted form this owner subsumes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Clipper2Lib;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Process;
using Rasm.Geometry;
using Rasm.Geometry.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ClipOp {
    public static readonly ClipOp Union = new("union", ClipType.Union);
    public static readonly ClipOp Intersect = new("intersect", ClipType.Intersection);
    public static readonly ClipOp Difference = new("difference", ClipType.Difference);
    public static readonly ClipOp Xor = new("xor", ClipType.Xor);

    public ClipType Type { get; }
}

[SmartEnum<string>]
public sealed partial class OffsetEnds {
    public static readonly OffsetEnds Polygon = new("polygon", JoinType.Miter, EndType.Polygon);
    public static readonly OffsetEnds OpenRound = new("open-round", JoinType.Round, EndType.Round);
    public static readonly OffsetEnds OpenButt = new("open-butt", JoinType.Square, EndType.Butt);
    public static readonly OffsetEnds OpenSquare = new("open-square", JoinType.Square, EndType.Square);

    public JoinType Join { get; }
    public EndType End { get; }
}

// --- [CONSTANTS] --------------------------------------------------------------------------
file static class Precision {
    public const int Digits = 6;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class PolygonAlgebra {
    public static Fin<Seq<Loop>> Offset(Seq<Loop> loops, double delta, OffsetEnds ends) =>
        loops.IsEmpty
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("offset:empty").ToError())
            : Fin.Succ(FromPaths(Clipper.InflatePaths(ToPaths(loops), delta, ends.Join, ends.End, precision: Precision.Digits)));

    public static Fin<Seq<Loop>> Clip(Seq<Loop> subject, Seq<Loop> clip, ClipOp op) =>
        subject.IsEmpty
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("clip:empty-subject").ToError())
            : Fin.Succ(FromPaths(Clipper.BooleanOp(op.Type, ToPaths(subject), ToPaths(clip), FillRule.NonZero, precision: Precision.Digits)));

    public static Fin<Seq<Loop>> MinkowskiSum(Loop fixedPart, Loop orbiting) =>
        Fin.Succ(FromPaths(Minkowski.Sum(ToPath(fixedPart), ToPath(orbiting), isClosed: true, decimalPlaces: Precision.Digits)));

    public static (Seq<Edge3> Inside, Seq<Edge3> Outside) ClipOpenPath(Edge3 edge, Seq<Loop> occluders) =>
        occluders.IsEmpty
            ? (Seq<Edge3>(), Seq(edge))
            : (Split(ClipType.Intersection, edge, occluders), Split(ClipType.Difference, edge, occluders));

    public static Fin<Seq<Loop>> Simplify(Seq<Loop> loops, double epsilon) =>
        loops.IsEmpty
            ? Fin.Fail<Seq<Loop>>(GeometryFault.DegenerateInput("simplify:empty").ToError())
            : Fin.Succ(FromPaths(Clipper.SimplifyPaths(ToPaths(loops), epsilon, isClosedPath: true)));

    // --- [BOUNDARIES] ---------------------------------------------------------------------
    static Seq<Edge3> Split(ClipType op, Edge3 edge, Seq<Loop> occluders) {
        var engine = new ClipperD(Precision.Digits);
        engine.AddOpenSubject(new PathsD { new PathD { ToPoint(edge.A), ToPoint(edge.B) } });
        engine.AddClip(ToPaths(occluders));
        var open = new PathsD();
        engine.Execute(op, FillRule.NonZero, new PolyTreeD(), open);
        return Runs(open);
    }

    static PathsD ToPaths(Seq<Loop> loops) => new(loops.Map(ToPath));
    static PathD ToPath(Loop loop) => new(loop.AsCcw().Vertices.Map(ToPoint));
    static PointD ToPoint(Point3d p) => new(p.X, p.Y);

    static Seq<Loop> FromPaths(PathsD paths) =>
        toSeq(paths).Map(path => new Loop(toSeq(path).Map(pt => new Point3d(pt.x, pt.y, 0.0)).ToArr(), Closed: true).AsCcw());

    static Seq<Edge3> Runs(PathsD paths) =>
        toSeq(paths).Bind(path => toSeq(Enumerable.Range(0, path.Count - 1))
            .Map(i => new Edge3(new Point3d(path[i].x, path[i].y, 0.0), new Point3d(path[i + 1].x, path[i + 1].y, 0.0))));
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Loop["Loop / Edge3"] -->|boundary map| PA["PolygonAlgebra"]
    PA -->|InflatePaths| Offset["Offset / inflate"]
    PA -->|BooleanOp| Clip["Boolean clip"]
    PA -->|MinkowskiSum| NFP["Minkowski / NFP"]
    PA -->|ClipperD AddOpenSubject| Screen["Open-path screen clip"]
    Offset -->|fold-back + Orient2D winding| LoopOut["Loop sets"]
    Clip --> LoopOut
    NFP --> LoopOut
    Screen --> EdgeOut["Edge3 runs"]
```
