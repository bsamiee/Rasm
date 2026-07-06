# [RASM_FABRICATION_SLICING]

The FFF/DED slicing author-kernel: `Slicing` the one static surface that takes a watertight `MeshSpace` solid and the `SlicePolicy` print parameters and produces the ordered `SliceLayer` set — per-height planar-section contours, inward perimeter shells, and `InfillPattern`-keyed hatch infill — the typed layer evidence the `Toolpath/motion#CAM_MOTION` `slice-layer` generator walks and the `Posting/program#CUT_PROGRAM` additive dialect emits. The planar section is the one genuinely-new geometry op the folder owns: it is NOT a `Geometry2D/algebra#POLYGON_ALGEBRA` composition (Clipper2 is 2D-only and carries no mesh-section primitive) and no managed slicer exists on NuGet, so the section is an in-folder author-kernel exactly the way `Toolpath/skeleton#STRAIGHT_SKELETON` authors the wavefront from first principles — a plane-vs-triangle sweep over the `MeshSpace` triangle traversal where each vertex's side is the exact `Rasm/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient3D` plane-side sign and the projected contour winding is closed by the exact `Predicate.Orient2D` sign, never a `double` dot-product side test that flips on a vertex grazing the plane. Once the per-height contour is a closed `Process/owner#FABRICATION_OWNER` `Loop`, the perimeter shells are inward `Geometry2D/algebra#POLYGON_ALGEBRA` `Offset` rings and the infill is a parametric hatch-line set clipped against the innermost shell through the same owner's `ClipOpenPath` — the slice composes the ONE Geometry2D owner and never a second polygon engine, and the buggy Clipper2 `Triangulate` stays excluded. The `InfillPattern` axis selects the hatch geometry as a behavior column over that one clip primitive — one row per pattern, never four parallel hatch generators. The kernel reads the `Process/physics#CUT_PARAMETER` `RemovalBudget.Additive` (extrusion-width/layer-height/print-speed/melt-temp) selected by `ProcessKind.Additive`'s `ProcessModality`, composes the `Rasm.Meshing` `MeshSpace` and `Rhino.Geometry` `Point3d`/`Vector3d` native vocabulary, computes no hash, and operates on raw coordinate doubles at the interior because a coordinate is the domain's native scalar, never a unit-bearing quantity.

Wire posture: HOST-LOCAL. The `SliceLayer` set crosses only the in-process seam to the `Toolpath/motion#CAM_MOTION` slice-layer generator and onward to the `Posting/program#CUT_PROGRAM` additive emitter — never a browser or peer wire. `SliceLayer`/`SlicePolicy`/`InfillPattern` are host-local types that never sit between wire and rail.

## [01]-[INDEX]

- [01]-[SLICING]: owns `Slicing` static surface — the plane-vs-triangle planar-section author-kernel over `Predicate.Orient3D` + MeshSpace traversal, the inward perimeter shells via Geometry2D `Offset`, and the `InfillPattern`-keyed hatch-clip infill via Geometry2D `ClipOpenPath`; `InfillPattern` `[SmartEnum<string>]` (rectilinear/concentric/honeycomb/grid) the hatch-geometry behavior column; `SliceLayer`/`SlicePolicy` the layer-set evidence the additive toolpath reads.

## [02]-[SLICING]

- Owner: `InfillPattern` `[SmartEnum<string>]` the hatch-geometry axis (`rectilinear`/`concentric`/`honeycomb`/`grid`) carrying the per-pattern hatch-line generation as a behavior column the one `ClipOpenPath` infill primitive reads, never four sibling hatch generators; `SlicePolicy` the print-parameter row (layer height, shell count, infill density and angle, contour tolerance, the adaptive-infill `DensityField` and the thin-wall `ThinWallBeadFloor` policy values) read once per slice; `SliceLayer` the per-height typed layer carrying its elevation, the closed outer `Loop` contours, the inward perimeter shell rings, and the clipped infill `Edge3` runs; `Slicing` the static surface owning `Layers` (the full ordered layer set from a `MeshSpace`) and the in-folder `Section` planar-section author-kernel (plane-vs-triangle over `Predicate.Orient3D`), `Shells` (the inward `Offset` rings), and `Infill` (the `InfillPattern` hatch clipped through `ClipOpenPath`).
- Cases: `InfillPattern` rows `rectilinear` (parallel hatch lines at the policy angle, alternating ±90° per layer for the dominant FFF pattern) · `concentric` (inward `Offset` rings of the innermost shell at the line-spacing pitch) · `honeycomb` (a hexagonal lattice hatch tiled across the layer bound) · `grid` (the rectilinear hatch unioned with its perpendicular for a full cross-hatch each layer) (4); the four patterns differ ONLY in the hatch-line set each emits, every one clipped against the innermost shell by the same `ClipOpenPath` primitive, never a per-pattern Boolean engine.
- Entry: `public static Fin<Seq<SliceLayer>> Layers(MeshSpace solid, SlicePolicy policy)` — the ONE slicing entrypoint; `Fin<T>` routes the kernel `GeometryFault.DegenerateInput` on an empty mesh or a non-positive layer height and the `FabricationFault.OpenLoop` when a planar section yields no closed contour at a height a print demands solid (the slice contract rejects an open section the mesh traversal admitted), each lowered with `.ToError()` per `Process/faults#FAULT_BAND`; the result is the elevation-ordered `SliceLayer` set from the model floor to its ceiling at the policy layer height, each layer carrying its contours, shells, and infill.
- Auto: `Layers` reads the `MeshSpace` once through `DuplicateNative` into the native `Mesh` triangle set (the same triangle traversal the `Rasm/Processing/repair#HEALING` `MeshEdit.OfMesh` composes — `mesh.Vertices` plus `mesh.Faces.GetFace(f)` fan-triangulated), takes the model Z-extent from its bounding box, and folds each layer height `z` from floor to ceiling through `Section`, `Shells`, then `Infill`. `Section` is the plane-vs-triangle author-kernel: for each triangle it reads the exact `Predicate.Orient3D(a, b, c, p)` plane-side sign of each vertex against the horizontal cut plane at `z` (a degenerate-plane point lifted off `z` to the three plane-defining points so the predicate places the vertex above/on/below the plane), keeps only triangles whose three vertices straddle the plane (a sign mix of `Positive`/`Negative`), and emits the one segment crossing the triangle by `Lerp`-interpolating the two crossed edges at their exact zero-crossing parameter; `Chain` then assembles the unordered segment soup into closed `Loop` contours by walking endpoint adjacency under the policy contour tolerance, each closed ring re-wound through `Loop.AsCcw` whose winding is the exact `Predicate.Orient2D` accumulation so an outer contour is CCW and a hole is CW, the verdict the shell-offset direction reads. `Shells` folds the closed contour inward by `k · extrusionWidth` constant `Geometry2D/algebra#POLYGON_ALGEBRA` `Offset` rings for `policy.ShellCount` passes, the innermost ring the infill boundary. `Infill` reads the `InfillPattern` row through the generated total `Switch` to lay the per-pattern hatch-line set across the innermost-shell bound at the policy density and angle, clips each hatch line against the innermost shell `Loop` set through `PolygonAlgebra.ClipOpenPath` (the open-path Boolean keeping the inside runs), and collects the surviving `Edge3` runs as the layer infill; the `concentric` row instead reads successive `Offset` rings of the innermost shell at the line pitch so the one infill surface serves both the hatch and the ring families. A new infill pattern is one `InfillPattern` row plus one `Switch` arm the generated dispatch breaks the build until it lands. Two policy laws strengthen the existing rows: THIN-WALL BEADING — where a wall's local thickness `2R` (the kernel medial clearance RADIUS, `Meshing/offset.md` `OffsetOp.Medial` → `ClearanceNode.Radius`) is below `ShellCount·ExtrusionWidth`, the shells re-divide `2R` into `⌈2R/w⌉` beads of width clamped to `[ThinWallBeadFloor·w, w]` — `BeadWidths` is the landed width LAW and the per-region `Offsetting.Apply(Medial)` query wires through the kernel-consumer shell fold (the `Slicing.Apply` re-route this page's roster row carries), the Arachne form as a consumer of the ONE kernel medial, never a second Voronoi/skeletal engine; ADAPTIVE INFILL DENSITY — `SpacingAt(p)` reads the optional `DensityField` policy VALUE to modulate the hatch pitch per line at its midpoint, every `InfillPattern` row reading the same field, never a new pattern family.
- Receipt: the `Seq<SliceLayer>` IS the typed evidence — each `SliceLayer` carries its elevation, the closed contour `Loop` set, the perimeter shell rings, and the clipped infill `Edge3` runs the `Toolpath/motion#CAM_MOTION` `slice-layer` generator walks directly; no generic slice ledger and no mesh primitive escaping the kernel into a layer.
- Packages: `Rasm.Meshing` (`MeshSpace` and the native `Mesh`/`MeshFace` triangle surface — composed via `DuplicateNative`), `Rhino.Geometry` (`Point3d`/`Vector3d`/`BoundingBox` and the native `Mesh`/`MeshFace` triangle surface — composed via `DuplicateNative`, never re-minted), `Rasm.Numerics` (`Predicate.Orient3D` the exact plane-side sign of the plane-vs-triangle straddle AND `Predicate.Orient2D` the projected-contour winding verdict, `Sign` — both VERIFIED settled members, the kernel exact floor), Clipper2 (via `Geometry2D/algebra#POLYGON_ALGEBRA` — `Offset` for the perimeter shells and the concentric rings, `ClipOpenPath` for the hatch-clip infill; the slice never opens a second `Clipper`/`ClipperD` engine and the buggy `Triangulate` stays excluded), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for `InfillPattern`), LanguageExt.Core (`Fin`/`Seq`/`Arr`), BCL inbox.
- Growth: a new infill pattern (gyroid, triangular, cubic) is one `InfillPattern` row plus one `Infill` `Switch` arm reading the same `ClipOpenPath` primitive, the generated dispatch breaking the build until the arm lands; a variable layer height (adaptive slicing by local surface slope) is one height-schedule column on `SlicePolicy` the `Layers` fold reads, never a second entrypoint; a non-planar/conformal section (DED on a curved substrate) is one cut-surface arm on `Section` over the same `Orient3D` plane-side floor generalized to a cut-plane normal; the support-structure pass is REALIZED at `Additive/support#SUPPORT` — its `SupportLayer` regions enter this fold as a region class (sparse density + dense interface), never a second overhang engine here; an infill density gradient is the `DensityField` policy VALUE the pattern rows read, never a new pattern row; zero new surface.
- Boundary: `Slicing` is the ONE additive-slicing owner and a `LayerContour`/`PerimeterShell`/`InfillHatch` sibling-class triple is the deleted form — the planar section, the shell offset, and the infill clip are members on one surface producing one `SliceLayer`; the planar-section side verdict reads the exact `Predicate.Orient3D` sign and a `double` plane-distance dot at the call site is the named robustness defect — a vertex grazing the cut plane classifies exactly or it is a defect (the `STRAIGHT_SKELETON` `Predicate.Orient2D` discipline lifted to 3D); the projected contour winding reads the exact `Predicate.Orient2D` accumulation through `Loop.AsCcw` and a hand-rolled signed-area winding loop is the deleted form the Fabrication owner already subsumes; the perimeter offset and the infill clip route the one `Geometry2D/algebra#POLYGON_ALGEBRA` owner and a slice-local `Clipper`/`ClipperD` call site or a hand-rolled per-vertex-normal `OffsetRing` is the named duplication defect; the infill pattern is the `InfillPattern` behavior column the one `Infill` `Switch` reads and a parallel `Rectilinear`/`Concentric`/`Honeycomb`/`Grid` hatch-generator quartet is the deleted form — one axis carries one discriminant, the row the `Switch` reads, never a second flag the arm re-derives; the additive print scalars are the `Process/physics#CUT_PARAMETER` `RemovalBudget.Additive` raw doubles and a UnitsNet quantity or a `RemovalBudget` widening in this kernel is the seam violation; the slice reads the `MeshSpace` triangle set through `DuplicateNative` and a CAM-side or posting-side re-slice of the same solid is the deleted form — the `SliceLayer` set is computed ONCE here and the toolpath/posting owners read it; the interior coordinate doubles inside `Section`/`Chain`/`Shells`/`Infill` are the sanctioned native-scalar posture and a unit-bearing quantity in a kernel signature is the seam violation; thin-wall beading READS the kernel medial clearance radius (`ClearanceNode.Radius`) and an in-folder skeletal-trapezoidation/Voronoi wall engine is the named duplication defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class InfillPattern {
    public static readonly InfillPattern Rectilinear = new("rectilinear");
    public static readonly InfillPattern Concentric = new("concentric");
    public static readonly InfillPattern Honeycomb = new("honeycomb");
    public static readonly InfillPattern Grid = new("grid");
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SlicePolicy(
    double LayerHeight,
    double ExtrusionWidth,
    int ShellCount,
    double InfillDensity,
    double InfillAngleRadians,
    double ContourTolerance,
    InfillPattern Pattern,
    Func<Point3d, double>? DensityField = null,     // adaptive-infill density VALUE: modulates spacing per hatch line, no new pattern rows
    double ThinWallBeadFloor = 0.0) {               // Arachne bead-width floor as a fraction of ExtrusionWidth; 0 disables thin-wall re-division
    public static SlicePolicy Fff(double layerHeight, double extrusionWidth) =>
        new(layerHeight, extrusionWidth, ShellCount: 2, InfillDensity: 0.2, InfillAngleRadians: Math.PI / 4.0,
            ContourTolerance: 1e-4, Pattern: InfillPattern.Rectilinear);

    public double LineSpacing => Math.Max(ExtrusionWidth, ExtrusionWidth / Math.Max(InfillDensity, 1e-3));

    public double SpacingAt(Point3d p) =>
        DensityField is null ? LineSpacing : Math.Max(ExtrusionWidth, ExtrusionWidth / Math.Max(DensityField(p), 1e-3));
}

public sealed record SliceLayer(double Elevation, Seq<Loop> Contours, Seq<Loop> Shells, Seq<Edge3> Infill);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Slicing {
    public static Fin<Seq<SliceLayer>> Layers(MeshSpace solid, SlicePolicy policy) =>
        policy.LayerHeight <= 1e-9
            ? Fin.Fail<Seq<SliceLayer>>(GeometryFault.DegenerateInput("slicing:non-positive-layer-height").ToError())
            : Triangles(solid) is { IsEmpty: true }
                ? Fin.Fail<Seq<SliceLayer>>(GeometryFault.DegenerateInput("slicing:empty-mesh").ToError())
                : Build(Triangles(solid), policy);

    static Fin<Seq<SliceLayer>> Build(Seq<(Point3d A, Point3d B, Point3d C)> tris, SlicePolicy policy) {
        BoundingBox bound = new(tris.Bind(static t => Seq(t.A, t.B, t.C)));
        double floor = bound.Min.Z + 0.5 * policy.LayerHeight;
        int count = Math.Max(1, (int)Math.Floor((bound.Max.Z - bound.Min.Z) / policy.LayerHeight));
        return toSeq(Enumerable.Range(0, count))
            .Map(k => Layer(tris, floor + k * policy.LayerHeight, bound, policy))
            .Sequence();
    }

    static Fin<SliceLayer> Layer(Seq<(Point3d A, Point3d B, Point3d C)> tris, double z, BoundingBox bound, SlicePolicy policy) {
        Seq<Loop> contours = Chain(Section(tris, z), policy.ContourTolerance).Map(static l => l.AsCcw());
        if (contours.IsEmpty) return Fin.Succ(new SliceLayer(z, contours, Seq<Loop>(), Seq<Edge3>()));
        Seq<Loop> shells = Shells(contours, policy);
        Seq<Loop> inner = shells.IsEmpty ? contours : Seq(shells.Last);
        return Fin.Succ(new SliceLayer(z, contours, shells, Infill(inner, z, bound, policy)));
    }

    // --- [PLANAR_SECTION]
    static Seq<Edge3> Section(Seq<(Point3d A, Point3d B, Point3d C)> tris, double z) =>
        tris.Bind(t => Cut(t, z));

    static Seq<Edge3> Cut((Point3d A, Point3d B, Point3d C) t, double z) {
        Sign sa = Side(t.A, z), sb = Side(t.B, z), sc = Side(t.C, z);
        if (sa == sb && sb == sc) return Seq<Edge3>();
        var pts = toSeq(Seq((t.A, sa, t.B, sb), (t.B, sb, t.C, sc), (t.C, sc, t.A, sa)))
            .Filter(static e => e.Item2 != e.Item4)
            .Map(static e => Crossing(e.Item1, e.Item3, z))
            .Somes();
        return pts.Count == 2 ? Seq(new Edge3(pts[0], pts[1])) : Seq<Edge3>();
    }

    static Sign Side(Point3d v, double z) =>
        Predicate.Orient3D(new Point3d(0.0, 0.0, z), new Point3d(1.0, 0.0, z), new Point3d(0.0, 1.0, z), v);

    static Option<Point3d> Crossing(Point3d p, Point3d q, double z) {
        double dp = p.Z - z, dq = q.Z - z;
        double denom = dp - dq;
        return Math.Abs(denom) < 1e-12 ? None : Some(p + (dp / denom) * (q - p));
    }

    // --- [CONTOUR_CHAIN]
    static Seq<Loop> Chain(Seq<Edge3> segments, double tolerance) {
        var remaining = segments.ToList();
        Seq<Loop> loops = Seq<Loop>();
        while (remaining.Count > 0) {
            Edge3 seed = remaining[0];
            remaining.RemoveAt(0);
            var ring = new System.Collections.Generic.List<Point3d> { seed.A, seed.B };
            Point3d head = seed.B;
            bool grown = true;
            while (grown && remaining.Count > 0) {
                grown = false;
                for (int i = 0; i < remaining.Count; i++) {
                    Edge3 e = remaining[i];
                    if (head.DistanceTo(e.A) <= tolerance) { ring.Add(e.B); head = e.B; remaining.RemoveAt(i); grown = true; break; }
                    if (head.DistanceTo(e.B) <= tolerance) { ring.Add(e.A); head = e.A; remaining.RemoveAt(i); grown = true; break; }
                }
            }
            if (ring.Count >= 3 && head.DistanceTo(ring[0]) <= Math.Max(tolerance, 1e-3))
                loops = loops.Add(new Loop(toArr(ring), Closed: true));
        }
        return loops;
    }

    // --- [PERIMETER_SHELLS]
    static Seq<Loop> Shells(Seq<Loop> contours, SlicePolicy policy) =>
        toSeq(Enumerable.Range(1, Math.Max(0, policy.ShellCount)))
            .Bind(k => PolygonAlgebra.Offset(contours, -(k - 0.5) * policy.ExtrusionWidth, OffsetEnds.Polygon).IfFail(Seq<Loop>()));

    // Arachne thin-wall law: local thickness 2R (kernel medial ClearanceNode.Radius, Meshing/offset.md) re-divides
    // into ceil(2R/w) beads clamped to [floor·w, w] — a CONSUMER of the one kernel medial, never a second Voronoi.
    static Arr<double> BeadWidths(double clearanceRadius, SlicePolicy policy) {
        double w = policy.ExtrusionWidth, floor = Math.Max(policy.ThinWallBeadFloor, 0.0) * w;
        int n = Math.Max(1, (int)Math.Ceiling(2.0 * clearanceRadius / Math.Max(w, 1e-6)));
        double width = Math.Clamp(2.0 * clearanceRadius / n, Math.Max(floor, 1e-6), w);
        return toArr(Enumerable.Repeat(width, n));
    }

    // --- [HATCH_INFILL]
    static Seq<Edge3> Infill(Seq<Loop> inner, double z, BoundingBox bound, SlicePolicy policy) =>
        policy.Pattern.Switch(
            state:       (inner, z, bound, policy),
            rectilinear: static s => Clip(Hatch(s.bound, s.z, s.policy.InfillAngleRadians + AlternateBy(s.z, s.policy), s.policy), s.inner),
            grid:        static s => Clip(Hatch(s.bound, s.z, s.policy.InfillAngleRadians, s.policy)
                                              .Concat(Hatch(s.bound, s.z, s.policy.InfillAngleRadians + Math.PI / 2.0, s.policy)), s.inner),
            honeycomb:   static s => Clip(Honeycomb(s.bound, s.z, s.policy), s.inner),
            concentric:  static s => Rings(s.inner, s.policy.LineSpacing));

    static Seq<Edge3> Clip(Seq<Edge3> hatch, Seq<Loop> inner) =>
        hatch.Bind(line => PolygonAlgebra.ClipOpenPath(line, inner).Inside);

    static Seq<Edge3> Hatch(BoundingBox bound, double z, double angle, SlicePolicy policy) {
        double diag = bound.Min.DistanceTo(bound.Max);
        Point3d centre = (bound.Min + bound.Max) * 0.5;
        Vector3d dir = new(Math.Cos(angle), Math.Sin(angle), 0.0);
        Vector3d step = new(-Math.Sin(angle), Math.Cos(angle), 0.0);
        var lines = new System.Collections.Generic.List<Edge3>();
        for (double offset = -0.5 * diag; offset <= 0.5 * diag;) {
            Point3d mid = new(centre.X + offset * step.X, centre.Y + offset * step.Y, z);
            lines.Add(new Edge3(mid - 0.5 * diag * dir, mid + 0.5 * diag * dir));
            offset += Math.Max(policy.SpacingAt(mid), 1e-3);    // DensityField modulates the pitch per line at its midpoint
        }
        return toSeq(lines);
    }

    static Seq<Edge3> Honeycomb(BoundingBox bound, double z, SlicePolicy policy) =>
        Hatch(bound, z, 0.0, policy)
            .Concat(Hatch(bound, z, Math.PI / 3.0, policy))
            .Concat(Hatch(bound, z, 2.0 * Math.PI / 3.0, policy));

    static Seq<Edge3> Rings(Seq<Loop> inner, double spacing) =>
        toSeq(Enumerable.Range(1, 64))
            .Map(k => PolygonAlgebra.Offset(inner, -k * spacing, OffsetEnds.Polygon).IfFail(Seq<Loop>()))
            .TakeWhile(static r => !r.IsEmpty)
            .Bind(static r => r)
            .Bind(static loop => toSeq(Enumerable.Range(0, loop.Count)).Map(i => new Edge3(loop.At(i), loop.At(i + 1))));

    static double AlternateBy(double z, SlicePolicy policy) =>
        (int)Math.Round(z / Math.Max(policy.LayerHeight, 1e-6)) % 2 == 0 ? 0.0 : Math.PI / 2.0;

    // --- [BOUNDARIES] ---------------------------------------------------------------------
    static Seq<(Point3d A, Point3d B, Point3d C)> Triangles(MeshSpace solid) {
        Mesh mesh = solid.DuplicateNative();
        Arr<Point3d> verts = toArr(mesh.Vertices.Select(static v => new Point3d(v.X, v.Y, v.Z)));
        return toSeq(Enumerable.Range(0, mesh.Faces.Count)).Bind(f => Fan(mesh.Faces.GetFace(f), verts));
    }

    static Seq<(Point3d A, Point3d B, Point3d C)> Fan(MeshFace face, Arr<Point3d> verts) =>
        face.IsTriangle
            ? Seq((verts[face.A], verts[face.B], verts[face.C]))
            : Seq((verts[face.A], verts[face.B], verts[face.C]), (verts[face.A], verts[face.C], verts[face.D]));
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Solid["MeshSpace solid"] -->|DuplicateNative triangles| Section["Section (plane-vs-triangle)"]
    Section -->|Orient3D plane-side sign| Crossing["Edge crossings"]
    Crossing -->|Chain endpoint-walk| Contour["Closed Loop (Orient2D winding)"]
    Contour -->|Geometry2D Offset| Shells["Perimeter shells"]
    Shells -->|innermost ring| Infill["Infill"]
    Pattern["InfillPattern row"] -->|generated Switch| Infill
    Infill -->|hatch / honeycomb / grid| ClipOpen["Geometry2D ClipOpenPath"]
    Infill -->|concentric| Rings["Geometry2D Offset rings"]
    ClipOpen -->|inside runs| Layer["SliceLayer"]
    Rings --> Layer
    Contour --> Layer
    Shells --> Layer
    Layer -->|elevation-ordered set| Cam["Toolpath/motion slice-layer"]
    Layer -->|additive dialect| Post["Posting/program additive"]
```

## [03]-[RESEARCH]

- [PLANAR_SECTION] The plane-vs-triangle planar section is realized as the `Section`/`Cut`/`Crossing` author-kernel over the exact `Predicate.Orient3D` plane-side floor — the one genuinely-new geometry op the fabrication folder owns, justified exactly as `Toolpath/skeleton#STRAIGHT_SKELETON` justifies its wavefront: no managed slicer exists on NuGet and Clipper2 is 2D-only, so the section is in-folder, but it re-mints NO geometric primitive — the side verdict is the kernel `Predicate.Orient3D` exact sign (a horizontal cut plane spelled by three plane-points at `z`, the query vertex placed above/on/below), the triangle traversal is the settled `Rasm/Processing/repair#HEALING` `MeshEdit.OfMesh` `DuplicateNative` idiom, the contour winding is the exact `Predicate.Orient2D` accumulation through `Loop.AsCcw`, and the perimeter/infill are the `Geometry2D/algebra#POLYGON_ALGEBRA` `Offset`/`ClipOpenPath` compositions. The one settled numeric assumption is the zero-crossing edge interpolation `p + (dp/(dp-dq))·(q-p)` at the exact `Orient3D`-confirmed straddle, the parameter taken only where the two endpoints' signs differ so a triangle coplanar with the cut plane (all three signs equal) emits no segment and is handled by its neighbors. If a kernel mesh-section primitive later lands in `Rasm`, this `Section` re-routes to it as a forward consumes-seam — the perimeter/shell/infill composition stays unchanged because it is already the one Geometry2D owner.
- [INFILL_TOPOLOGY] The `InfillPattern` axis is realized as a hatch-geometry behavior column the one `Infill` generated total `Switch` reads, never a parallel generator quartet: `rectilinear` emits the parallel hatch at the policy angle alternating ±90° per layer (the dominant FFF bonding pattern), `grid` unions the hatch with its perpendicular for a per-layer cross-hatch, `honeycomb` tiles the three 60°-rotated hatch families into the hexagonal lattice, and `concentric` reads successive inward `Offset` rings of the innermost shell at the line pitch so the ring family and the hatch family share the one infill surface. Every non-concentric pattern clips its hatch-line set against the innermost shell `Loop` through the one `PolygonAlgebra.ClipOpenPath` open-path Boolean keeping the inside runs — the surviving `Edge3` set is the layer infill the `Toolpath/motion#CAM_MOTION` slice-layer generator walks. A new pattern (gyroid, triangular, cubic) is one row plus one `Switch` arm over the same clip primitive, the generated dispatch breaking the build until the arm lands.
