# [RASM_FABRICATION_PROFILE_IMPORT]

The one portable 2D profile-ingress boundary: `ProfileImport` the single owner admitting external DXF/DWG closed part outlines into the canonical `Process/owner#FABRICATION_OWNER` `Loop` vocabulary through the pure-managed `ACadSharp` reader. A foreign CAD entity crosses into the interior exactly ONCE here — `ProfileImport.Read` reads the model-space entity set off a `CadDocument`, folds each `LwPolyline`/`Polyline2D`/`Line`/`Arc`/`Circle`/`Spline`/`Insert` through one total `Admit` switch over the entity case, tessellates every bulge/arc/circle span through the ACadSharp-owned `Arc.CreateFromBulge`/`PolygonalVertexes`/`Circle.PolygonalVertexes` curve sampler and every NURBS span through the `Spline.PolygonalVertexes`/`TryPolygonalVertexes` native tessellator at one `ChordTolerance` knob, recursively flattens each nested-block `Insert` through its composed `InsertPoint`/scale/`Rotation` placement transform over `BlockRecord.Entities`, and re-imposes the kernel `Rasm.Geometry/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` winding through `Loop.AsCcw` on the way out. No `CadDocument`, no ACadSharp entity, no `CSMath.XY`/`XYZ` type ever travels a sibling-kernel signature; the boundary is the seam, and the interior reads only `Loop`. The imported `Arr<Loop>` set is the part library that `Nesting/nfp#NESTING` nests for true-shape feasibility and `Posting/program#CUT_PROGRAM` cuts as a profile program — the same `Loop` the `Polygon/clipper#POLYGON_ALGEBRA` substrate offsets and clips, never a parallel imported-geometry shape.

Wire posture: HOST-LOCAL, HOST-NEUTRAL. `ACadSharp` is managed AnyCPU IL with no native asset and no RID burden, so the boundary is ALC-safe and runs on every runtime the folder targets; it coexists with the Rhino-native file I/O the architecture keeps as the host-bound read path (`ARCHITECTURE.md` coexistence rule) and is never thinned to feed it. The `Read` facade THROWS on a malformed/unreadable file — that exception NEVER escapes: it lowers to `GeometryFault.DegenerateInput` once at `Admit`, so the boundary's only outward contract is the `Fin<Arr<Loop>>` rail, never a leaked reader exception.

## [01]-[INDEX]

- [01]-[PROFILE_IMPORT]: `ProfileImport` static boundary over `ACadSharp` — `ChordTolerance` knob, the total `Admit` switch over the entity case, the `Read` fold into `Fin<Arr<Loop>>`, the bulge/arc/circle/spline tessellation through the package-owned curve sampler, and the recursive `Insert` block flattening through the composed placement transform; the ONE DXF/DWG ingress owner.

## [02]-[PROFILE_IMPORT]

- Owner: `ProfileImport` the static surface owning `Read` (the DXF/DWG file → `Fin<Arr<Loop>>` boundary fold) plus the private `Admit` total switch and the entity-to-`Loop` tessellation; `ChordTolerance` `[ValueObject<int>]` the one chord-precision knob (the `precision` segment count every `PolygonalVertexes` sampler reads). One owner, one knob, one fold — never a per-entity-type sibling reader triple (`LwPolyline`/`Arc`/`Circle` readers collapse to one `Admit` switch).
- Cases: the `Admit` switch arms over the ACadSharp entity union the boundary reads — `LwPolyline` (closed lightweight polyline, each `Vertex` a `Location: CSMath.XY` plus a `Bulge: double`; a zero-bulge vertex is the raw point, a non-zero-bulge span mints an `Arc` through `Arc.CreateFromBulge` and samples it) · `Polyline2D` (the `Polyline<Vertex2D>` form admitted at its straight-segment vertices through the `Pt(XYZ)` overload) · `Line` (the two-point degenerate loop, never closed) · `Arc` (a single arc span sampled through `Arc.PolygonalVertexes`) · `Circle` (the full circle sampled through `Circle.PolygonalVertexes`) · `Spline` (the NURBS profile sampled through the ACadSharp-owned native tessellator `Spline.TryPolygonalVertexes(chord.Segments, out)`, a `false` probe lowering `GeometryFault.DegenerateInput`, a `FitPoints`-only spline rebuilt through `UpdateFromFitPoints` before sampling, never a hand-rolled de Boor) · `Insert` (the nested-block reference flattened by reading `Insert.Block.Entities` and recursing `Admit` through the composed `InsertPoint`/`XScale`/`YScale`/`Rotation` placement transform, the one place a non-identity part placement enters the boundary) (7); any other entity is dropped (the `Option<Seq<Loop>>.None` arm — a `Text`/`Dimension`/`Hatch` is not a profile and never faults the read). The `Spline` and `Insert` arms are REAL `Admit` arms — `.api/api-acadsharp.md` `[SPLINE_SAMPLER]` ratifies `Spline.PolygonalVertexes`/`TryPolygonalVertexes`/`UpdateFromFitPoints` and `[BLOCK_TRAVERSAL]` ratifies `Insert.Block.Entities`/`InsertPoint`/`XScale`/`YScale`/`Rotation`/`ApplyTransform`, so the boundary transcribes them as ratified members, no longer deferred.
- Entry: `Read(string path, ChordTolerance chord, bool demandClosed)` returns `Fin<Arr<Loop>>` — the one polymorphic entrypoint discriminating on the file extension to route `DxfReader.Read` versus `DwgReader.Read`, never a `ReadDxf`/`ReadDwg` sibling pair. A successful read folds the model-space entities through `Admit` (an `Insert` arm recursing through the composed placement transform), partitions the admitted `Loop` set, and (when `demandClosed`) routes `FabricationFault.OpenLoop` if any admitted loop is non-closed; an unreadable/empty/non-finite file OR a caught `DxfReader`/`DwgReader` exception lowers `GeometryFault.DegenerateInput` ONCE at the boundary.
- Auto: `Read` wraps the `DxfReader.Read(path, notification: null)` / `DwgReader.Read(path, notification: null)` facade in the exception-to-`Fin` lowering (`Try` → `Fin`), reads `doc.Entities` (= `doc.ModelSpace.Entities`, the top-level model-space set, NOT auto-flattened), and folds each entity through `Admit`; `Admit` is the total switch returning `Option<Seq<Loop>>` (the dropped-entity arm is `None`, never a fault) — an `Insert` arm yields the FLATTENED block loop set so one entity admits many loops, every other arm yields a singleton `Seq`; `Tessellate` is the per-entity vertex stream — a straight `LwPolyline`/`Polyline2D` segment is the raw `Location`, a non-zero-bulge `LwPolyline` span mints `Arc.CreateFromBulge(prev, next, bulge)` and reads its `PolygonalVertexes(chord.Segments)`, a standalone `Arc` reads `Arc.PolygonalVertexes(chord.Segments)`, a `Circle` reads `Circle.PolygonalVertexes(chord.Segments)`, a `Spline` reads `Spline.TryPolygonalVertexes(chord.Segments, out var pts)` (the native NURBS tessellator, a `false` lowering `DegenerateInput`) — every sampled `XYZ`/`XY` projected to `Point3d(x, y, 0)` and the assembled `Loop` re-oriented through `AsCcw`; the `Insert` arm reads `block.Block.Entities`, composes the `InsertPoint`/scale/`Rotation` transform per reference, recurses `Admit` over each child entity, and applies the composed transform to each returned `Loop` so a block-referenced part instance flattens into the canonical `Loop` vocabulary (the `[BLOCK_TRAVERSAL]` gate confirms `Entities` does NOT auto-flatten, so the recursion is explicit). The empty-result read (no admitted profile) lowers `GeometryFault.DegenerateInput("profile:empty")`; a non-finite vertex coordinate lowers `GeometryFault.DegenerateInput("profile:non-finite")`.
- Receipt: `Read` returns the typed `Arr<Loop>` set directly — the loop set IS the part library the consuming kernel reads; no generic import-report, no `CadDocument` and no ACadSharp entity escaping the boundary. The fault evidence is the `GeometryFault`/`FabricationFault` union value the `Fin<Arr<Loop>>` failure channel carries, lowered through `.ToError()`.
- Packages: `ACadSharp` (`DxfReader.Read`/`DwgReader.Read` → `CadDocument`; `CadDocument.Entities`/`ModelSpace`; `LwPolyline.Vertices: List<Vertex>` with `Vertex.Location: CSMath.XY`/`Vertex.Bulge: double`/`LwPolyline.IsClosed`; `Polyline2D.Vertices: SeqendCollection<Vertex2D>` (enumerated through `toSeq`, `Vertex2D.Location: XYZ`)/`IsClosed`; `Line.StartPoint`/`EndPoint: XYZ`; `Arc : Circle` `Center: XYZ`/`Radius: double`/`Arc.CreateFromBulge(XY,XY,double)`/`Arc.PolygonalVertexes(int): List<XYZ>`; `Circle.PolygonalVertexes(int): List<XYZ>`; `Spline.TryPolygonalVertexes(int, out List<XYZ>): bool`/`Spline.PolygonalVertexes(int): List<XYZ>`/`Spline.UpdateFromFitPoints(uint)`; `Insert.Block: BlockRecord`/`Block.Entities: CadObjectCollection<Entity>`/`Insert.InsertPoint: XYZ`/`XScale`/`YScale`/`ZScale: double`/`Rotation: double`/`Normal: XYZ`/`Insert.ApplyTransform` — all spellings ratified by `.api/api-acadsharp.md` `[4]-[RATIFIED]` `[SPLINE_SAMPLER]`/`[BLOCK_TRAVERSAL]`, the boundary transcribes no unratified member), `Rasm`/Vectors (`Point3d` — the tessellated vertices), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — composed through `Loop.AsCcw`, the winding verdict, never re-rolled), `Rasm.Geometry` (`GeometryFault.DegenerateInput` band-2400), Thinktecture.Runtime.Extensions (`[ValueObject<int>]`), LanguageExt.Core (`Try`/`Fin`/`Option`/`Arr`/`Seq`), BCL inbox (`System.IO.Path` for the extension route).
- Growth: a new profile entity type is one `Admit` switch arm composing the same `PolygonalVertexes`/`CreateFromBulge` sampler; the `Spline` arm is the realized `Admit` arm over the ACadSharp native `Spline.TryPolygonalVertexes` tessellator (`.api/api-acadsharp.md` `[SPLINE_SAMPLER]` ratifies it); the nested-block `Insert`-flattening is the realized `Admit` arm reading `Insert.Block.Entities` and composing the `Insert` placement transform (`[BLOCK_TRAVERSAL]` ratified); an arrayed `Insert` (`ColumnCount`/`RowCount`/`ColumnSpacing`/`RowSpacing`) is one per-cell loop on the same `Insert` arm; a finer chord precision is one `ChordTolerance` value; an adaptive chord-deviation sampler (deviation-bounded segment count over the fixed `precision`) is one `ChordTolerance` arm over the same owner; zero new boundary, zero new entrypoint.
- Boundary: `ProfileImport` is the ONE DXF/DWG ingress owner — a second `DxfReader`/`DwgReader` call site, a `CadDocument` traversal, or an ACadSharp entity-type field in any sibling kernel is the named seam-violation defect (the foreign CAD entity crosses into `Loop` HERE and never travels the interior); the `Read` throw lowers to `GeometryFault.DegenerateInput` once at `Admit` and a reader exception escaping the boundary unlowered is the reject; a hand-rolled bulge-to-arc trigonometry where the package owns `Arc.CreateFromBulge`/`PolygonalVertexes`, or a hand-rolled NURBS de Boor where the package owns `Spline.PolygonalVertexes`/`TryPolygonalVertexes`, is the deleted form; the `Insert` arm composes the placement transform EXPLICITLY (`[BLOCK_TRAVERSAL]` warns `Entities` does NOT auto-flatten) and a phase-1 read assuming a flattened model space, or a hand-built OCS-to-WCS matrix where the package owns `Insert.ApplyTransform`, is the reject — the `Insert` recursion through the composed transform is the one place a non-identity part placement enters the boundary; this boundary is read-only profile INGRESS — writing DXF/DWG from this folder is the reject (Rhino owns the host-bound native write, and managed DXF/DWG WRITE is an `Rasm.AppUi`/Render drafting concern, never a Fabrication rail); `ACadSharp` is the SOLE read-side CAD owner and `netDxf` (present in the central manifest as an `Rasm.AppUi` DXF-write dependency) is the rejected second DXF reader — DXF-only, no DWG, no AC1014-AC1032 spread, no managed `Spline`/bulge sampler parity, so no sibling kernel opens a `netDxf` reader beside `ProfileImport`; the winding verdict is the kernel `Predicate.Orient2D` exact sign through `AsCcw` and the ACadSharp `IsClosed`/inferred orientation is never the domain sign.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Process;
using Rasm.Geometry;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<int>]
public readonly partial struct ChordTolerance {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value < 2
            ? new ValidationError("chord-tolerance: segment count must be >= 2")
            : null;

    public int Segments => Value;

    public static readonly ChordTolerance Default = Create(24);
}

// The composed block-placement transform: the one host-neutral OCS-to-WCS affine the Insert
// recursion threads as a 2x3 row-major matrix (scale-rotate-translate), never a leaked
// Rhino/CSMath matrix. Compose folds outer ∘ inner so a nested Insert places under its parent.
public readonly record struct Placement(double M00, double M01, double M02, double M10, double M11, double M12) {
    public static readonly Placement Identity = new(1.0, 0.0, 0.0, 0.0, 1.0, 0.0);

    public static Placement Of(Insert i) {
        double c = Math.Cos(i.Rotation), s = Math.Sin(i.Rotation);
        return new(c * i.XScale, -s * i.YScale, i.InsertPoint.X, s * i.XScale, c * i.YScale, i.InsertPoint.Y);
    }

    public Placement Compose(Placement b) =>
        new(M00 * b.M00 + M01 * b.M10, M00 * b.M01 + M01 * b.M11, M00 * b.M02 + M01 * b.M12 + M02,
            M10 * b.M00 + M11 * b.M10, M10 * b.M01 + M11 * b.M11, M10 * b.M02 + M11 * b.M12 + M12);

    public Loop Apply(Loop loop) =>
        this == Identity
            ? loop
            : new Loop(loop.Vertices.Map(v =>
                new Point3d(M00 * v.X + M01 * v.Y + M02, M10 * v.X + M11 * v.Y + M12, 0.0)).ToArr(), loop.Closed).AsCcw();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ProfileImport {
    public static Fin<Arr<Loop>> Read(string path, ChordTolerance chord, bool demandClosed) =>
        Open(path)
            .Bind(doc => Fold(doc, chord))
            .Bind(loops => demandClosed ? RequireClosed(loops) : Fin.Succ(loops));

    static Fin<Arr<Loop>> Fold(CadDocument doc, ChordTolerance chord) {
        Arr<Loop> loops = toSeq(doc.Entities)
            .Map(e => Admit(e, chord, Placement.Identity))
            .Somes()
            .Bind(identity)
            .ToArr();
        return loops.IsEmpty
            ? Fin.Fail<Arr<Loop>>(GeometryFault.DegenerateInput("profile:empty").ToError())
            : loops.Exists(NonFinite)
                ? Fin.Fail<Arr<Loop>>(GeometryFault.DegenerateInput("profile:non-finite").ToError())
                : Fin.Succ(loops);
    }

    static Fin<Arr<Loop>> RequireClosed(Arr<Loop> loops) =>
        loops.Find(l => !l.Closed).Match(
            Some: open => Fin.Fail<Arr<Loop>>(
                FabricationFault.OpenLoop($"profile:open:{open.Count}").ToError()),
            None: () => Fin.Succ(loops));

    // --- [BOUNDARIES] ---------------------------------------------------------------------
    static Fin<CadDocument> Open(string path) =>
        Try(() => Path.GetExtension(path).ToLowerInvariant() is ".dwg"
                ? DwgReader.Read(path, notification: null)
                : DxfReader.Read(path, notification: null))
            .ToFin()
            .MapFail(_ => GeometryFault.DegenerateInput($"profile:unreadable:{Path.GetFileName(path)}").ToError());

    static Option<Seq<Loop>> Admit(Entity entity, ChordTolerance chord, Placement place) =>
        entity switch {
            LwPolyline poly => Some(Seq(place.Apply(LoopOf(LwVerts(poly, chord), poly.IsClosed)))),
            Polyline2D poly => Some(Seq(place.Apply(LoopOf(toSeq(poly.Vertices).Map(v => Pt(v.Location)), poly.IsClosed)))),
            Arc arc         => Some(Seq(place.Apply(LoopOf(Sampled(arc.PolygonalVertexes(chord.Segments)), Closed: false)))),
            Circle circle   => Some(Seq(place.Apply(LoopOf(Sampled(circle.PolygonalVertexes(chord.Segments)), Closed: true)))),
            Line line       => Some(Seq(place.Apply(LoopOf(Seq(Pt(line.StartPoint), Pt(line.EndPoint)), Closed: false)))),
            Spline spline   => spline.TryPolygonalVertexes(chord.Segments, out List<XYZ> pts)
                                   ? Some(Seq(place.Apply(LoopOf(Sampled(pts), Closed: spline.IsClosed))))
                                   : None,
            Insert insert   => Some(Flatten(insert, chord, place.Compose(Placement.Of(insert)))),
            _               => None,
        };

    // An Insert flattens its block's entities through the composed OCS-to-WCS placement transform;
    // [BLOCK_TRAVERSAL] confirms Block.Entities does NOT auto-flatten, so the recursion is explicit.
    static Seq<Loop> Flatten(Insert insert, ChordTolerance chord, Placement place) =>
        toSeq(insert.Block.Entities).Map(e => Admit(e, chord, place)).Somes().Bind(identity);

    static Seq<Point3d> LwVerts(LwPolyline poly, ChordTolerance chord) =>
        toSeq(Enumerable.Range(0, poly.Vertices.Count)).Bind(i => Span(poly, i, chord));

    static Seq<Point3d> Span(LwPolyline poly, int i, ChordTolerance chord) {
        LwPolyline.Vertex v = poly.Vertices[i];
        if (Math.Abs(v.Bulge) < 1e-12) return Seq1(Pt(v.Location));
        int next = (i + 1) % poly.Vertices.Count;
        if (next == 0 && !poly.IsClosed) return Seq1(Pt(v.Location));
        List<XYZ> sampled = Arc.CreateFromBulge(v.Location, poly.Vertices[next].Location, v.Bulge)
            .PolygonalVertexes(chord.Segments);
        return Sampled(sampled).Take(sampled.Count - 1);
    }

    static Seq<Point3d> Sampled(List<XYZ> sampled) => toSeq(sampled).Map(Pt);

    static Loop LoopOf(Seq<Point3d> verts, bool Closed) =>
        new Loop(verts.ToArr(), Closed).AsCcw();

    static Point3d Pt(XY xy) => new(xy.X, xy.Y, 0.0);
    static Point3d Pt(XYZ xyz) => new(xyz.X, xyz.Y, 0.0);

    static bool NonFinite(Loop loop) =>
        loop.Vertices.Exists(p => !double.IsFinite(p.X) || !double.IsFinite(p.Y));
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    File["DXF / DWG path"] -->|extension route| Open["Open · DxfReader/DwgReader.Read"]
    Open -->|Try → Fin · throw lowered| Doc["CadDocument.Entities"]
    Open -.->|reader throw / unreadable| Degen["GeometryFault.DegenerateInput"]
    Doc -->|Admit total switch| Entity{"entity case"}
    Entity -->|LwPolyline · CreateFromBulge| Tess["Tessellate · PolygonalVertexes(chord)"]
    Entity -->|Arc / Circle| Tess
    Entity -->|Line / Polyline2D| Tess
    Entity -->|Spline · TryPolygonalVertexes| Tess
    Entity -->|Insert · Block.Entities + transform| Recurse["Flatten · recurse Admit ∘ Placement"]
    Recurse -->|composed placement| Entity
    Entity -.->|Text / Dimension / Hatch| Drop["None · dropped"]
    Tess -->|Pt → AsCcw winding| Loops["Arr&lt;Loop&gt;"]
    Recurse -->|placed loops| Loops
    Loops -.->|empty / non-finite| Degen
    Loops -->|demandClosed| Closed{"all closed?"}
    Closed -.->|non-closed| Open2["FabricationFault.OpenLoop"]
    Closed -->|yes| Out["Fin&lt;Arr&lt;Loop&gt;&gt; · part library"]
```
