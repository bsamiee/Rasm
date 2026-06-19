# [RASM_FABRICATION_PROFILE_IMPORT]

The one portable 2D profile-ingress boundary: `ProfileImport` the single owner admitting external DXF/DWG closed part outlines into the canonical `Process/owner#FABRICATION_OWNER` `Loop` vocabulary through the pure-managed `ACadSharp` reader. A foreign CAD entity crosses into the interior exactly ONCE here — `ProfileImport.Read` reads the model-space entity set off a `CadDocument`, folds each `LwPolyline`/`Polyline2D`/`Line`/`Arc`/`Circle` through one total `Admit` switch over the entity case, tessellates every bulge/arc/circle span through the ACadSharp-owned `Arc.CreateFromBulge`/`PolygonalVertexes`/`Circle.PolygonalVertexes` curve sampler at one `ChordTolerance` knob, and re-imposes the kernel `Rasm.Geometry/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` winding through `Loop.AsCcw` on the way out. No `CadDocument`, no ACadSharp entity, no `CSMath.XY`/`XYZ` type ever travels a sibling-kernel signature; the boundary is the seam, and the interior reads only `Loop`. The imported `Arr<Loop>` set is the part library that `Nesting/nfp#NESTING` nests for true-shape feasibility and `Posting/program#CUT_PROGRAM` cuts as a profile program — the same `Loop` the `Polygon/clipper#POLYGON_ALGEBRA` substrate offsets and clips, never a parallel imported-geometry shape.

Wire posture: HOST-LOCAL, HOST-NEUTRAL. `ACadSharp` is managed AnyCPU IL with no native asset and no RID burden, so the boundary is ALC-safe and runs on every runtime the folder targets; it coexists with the Rhino-native file I/O the architecture keeps as the host-bound read path (`ARCHITECTURE.md` coexistence rule) and is never thinned to feed it. The `Read` facade THROWS on a malformed/unreadable file — that exception NEVER escapes: it lowers to `GeometryFault.DegenerateInput` once at `Admit`, so the boundary's only outward contract is the `Fin<Arr<Loop>>` rail, never a leaked reader exception.

## [1]-[INDEX]

- [1]-[PROFILE_IMPORT]: `ProfileImport` static boundary over `ACadSharp` — `ChordTolerance` knob, the total `Admit` switch over the entity case, the `Read` fold into `Fin<Arr<Loop>>`, and the bulge/arc/circle/spline tessellation through the package-owned curve sampler; the ONE DXF/DWG ingress owner.

## [2]-[PROFILE_IMPORT]

- Owner: `ProfileImport` the static surface owning `Read` (the DXF/DWG file → `Fin<Arr<Loop>>` boundary fold) plus the private `Admit` total switch and the entity-to-`Loop` tessellation; `ChordTolerance` `[ValueObject<int>]` the one chord-precision knob (the `precision` segment count every `PolygonalVertexes` sampler reads). One owner, one knob, one fold — never a per-entity-type sibling reader triple (`LwPolyline`/`Arc`/`Circle` readers collapse to one `Admit` switch).
- Cases: the `Admit` switch arms over the ACadSharp entity union the boundary reads — `LwPolyline` (closed lightweight polyline, each `Vertex` a `Location: CSMath.XY` plus a `Bulge: double`; a zero-bulge vertex is the raw point, a non-zero-bulge span mints an `Arc` through `Arc.CreateFromBulge` and samples it) · `Polyline2D` (the `Polyline<Vertex2D>` form admitted at its straight-segment vertices through the `Pt(XYZ)` overload; a `Vertex2D.Bulge` arc span reuses the same `LwPolyline` `Arc.CreateFromBulge` path and lands as one deferred `Admit` arm, the bulge dropped to its chord at phase-1) · `Line` (the two-point degenerate loop, never closed) · `Arc` (a single arc span sampled through `Arc.PolygonalVertexes`) · `Circle` (the full circle sampled through `Circle.PolygonalVertexes`) (5); any other entity is dropped (the `Option<Loop>.None` arm — a `Text`/`Dimension`/`Hatch`/`Insert`/`Spline` is not a phase-1 profile and never faults the read). The `Spline` sampler and the `Insert`-flattening arm (reading `BlockRecord.Entities` and composing the placement transform) are the forward growth arms — each lands as one `Admit` arm once `.api/api-acadsharp.md` ratifies the `Spline` curve-sampling spelling, not a phase-1 case (the catalogue ratifies the curve sampler on `Arc`/`Circle` only; `RAIL_LAW` forbids transcribing an unratified `Spline` member).
- Entry: `Read(string path, ChordTolerance chord, bool demandClosed)` returns `Fin<Arr<Loop>>` — the one polymorphic entrypoint discriminating on the file extension to route `DxfReader.Read` versus `DwgReader.Read`, never a `ReadDxf`/`ReadDwg` sibling pair. A successful read folds the model-space entities through `Admit`, partitions the admitted `Loop` set, and (when `demandClosed`) routes `FabricationFault.OpenLoop` if any admitted loop is non-closed; an unreadable/empty/non-finite file OR a caught `DxfReader`/`DwgReader` exception lowers `GeometryFault.DegenerateInput` ONCE at the boundary.
- Auto: `Read` wraps the `DxfReader.Read(path, notification: null)` / `DwgReader.Read(path, notification: null)` facade in the exception-to-`Fin` lowering (`Try` → `Fin`), reads `doc.Entities` (= `doc.ModelSpace.Entities`, the top-level model-space set, NOT auto-flattened), and folds each entity through `Admit`; `Admit` is the total switch returning `Option<Loop>` (the dropped-entity arm is `None`, never a fault); `Tessellate` is the per-entity vertex stream — a straight `LwPolyline`/`Polyline2D` segment is the raw `Location`, a non-zero-bulge `LwPolyline` span mints `Arc.CreateFromBulge(prev, next, bulge)` and reads its `PolygonalVertexes(chord.Segments)`, a standalone `Arc` reads `Arc.PolygonalVertexes(chord.Segments)`, a `Circle` reads `Circle.PolygonalVertexes(chord.Segments)`, a `Spline` samples the control polygon at `chord.Segments` — every sampled `XYZ`/`XY` projected to `Point3d(x, y, 0)` and the assembled `Loop` re-oriented through `AsCcw`. The empty-result read (no admitted profile) lowers `GeometryFault.DegenerateInput("profile:empty")`; a non-finite vertex coordinate lowers `GeometryFault.DegenerateInput("profile:non-finite")`.
- Receipt: `Read` returns the typed `Arr<Loop>` set directly — the loop set IS the part library the consuming kernel reads; no generic import-report, no `CadDocument` and no ACadSharp entity escaping the boundary. The fault evidence is the `GeometryFault`/`FabricationFault` union value the `Fin<Arr<Loop>>` failure channel carries, lowered through `.ToError()`.
- Packages: `ACadSharp` (`DxfReader.Read`/`DwgReader.Read` → `CadDocument`; `CadDocument.Entities`/`ModelSpace`; `LwPolyline.Vertices: List<Vertex>` with `Vertex.Location: CSMath.XY`/`Vertex.Bulge: double`/`LwPolyline.IsClosed`; `Polyline2D.Vertices: SeqendCollection<Vertex2D>` (enumerated through `toSeq`, `Vertex2D.Location: XYZ`)/`IsClosed`; `Line.StartPoint`/`EndPoint: XYZ`; `Arc : Circle` `Center: XYZ`/`Radius: double`/`Arc.CreateFromBulge(XY,XY,double)`/`Arc.PolygonalVertexes(int): List<XYZ>`; `Circle.PolygonalVertexes(int): List<XYZ>` — all spellings ratified by `.api/api-acadsharp.md` `[4]-[RATIFIED]`, the boundary transcribes no unratified member, and the `Spline` curve sampler stays a deferred growth arm until the catalogue ratifies it), `Rasm`/Vectors (`Point3d` — the tessellated vertices), `Rasm.Geometry.Numerics` (`Predicate.Orient2D` — composed through `Loop.AsCcw`, the winding verdict, never re-rolled), `Rasm.Geometry` (`GeometryFault.DegenerateInput` band-2400), Thinktecture.Runtime.Extensions (`[ValueObject<int>]`), LanguageExt.Core (`Try`/`Fin`/`Option`/`Arr`), BCL inbox (`System.IO.Path` for the extension route).
- Growth: a new profile entity type is one `Admit` switch arm composing the same `PolygonalVertexes`/`CreateFromBulge` sampler; the `Spline` arm lands as one `Admit` arm once `.api/api-acadsharp.md` ratifies the spline curve-sampling spelling (the catalogue ratifies the sampler on `Arc`/`Circle` today); a finer chord precision is one `ChordTolerance` value; the nested-block `Insert`-flattening admission is one `Admit` arm reading `BlockRecords[name].Entities` and composing the `Insert` placement transform (the forward arm the catalogue's `[BLOCK_TRAVERSAL]` gate names); an adaptive chord-deviation sampler (deviation-bounded segment count over the fixed `precision`) is one `ChordTolerance` arm over the same owner; zero new boundary, zero new entrypoint.
- Boundary: `ProfileImport` is the ONE DXF/DWG ingress owner — a second `DxfReader`/`DwgReader` call site, a `CadDocument` traversal, or an ACadSharp entity-type field in any sibling kernel is the named seam-violation defect (the foreign CAD entity crosses into `Loop` HERE and never travels the interior); the `Read` throw lowers to `GeometryFault.DegenerateInput` once at `Admit` and a reader exception escaping the boundary unlowered is the reject; a hand-rolled bulge-to-arc trigonometry where the package owns `Arc.CreateFromBulge`/`PolygonalVertexes` is the deleted form; this boundary is read-only profile INGRESS — writing DXF/DWG from this folder is the reject (Rhino owns the host-bound native write); the winding verdict is the kernel `Predicate.Orient2D` exact sign through `AsCcw` and the ACadSharp `IsClosed`/inferred orientation is never the domain sign; the boundary admits ONLY the top-level `ModelSpace.Entities` closed profiles this campaign and a phase-1 read that auto-flattens nested `Insert` blocks is the reject (the catalogue's `[BLOCK_TRAVERSAL]` confirms `Entities` does NOT flatten).

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using CSMath;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Frontier;
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

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ProfileImport {
    public static Fin<Arr<Loop>> Read(string path, ChordTolerance chord, bool demandClosed) =>
        Open(path)
            .Bind(doc => Fold(doc, chord))
            .Bind(loops => demandClosed ? RequireClosed(loops) : Fin.Succ(loops));

    static Fin<Arr<Loop>> Fold(CadDocument doc, ChordTolerance chord) {
        Arr<Loop> loops = toSeq(doc.Entities)
            .Map(e => Admit(e, chord))
            .Somes()
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

    static Option<Loop> Admit(Entity entity, ChordTolerance chord) =>
        entity switch {
            LwPolyline poly => Some(LoopOf(LwVerts(poly, chord), poly.IsClosed)),
            Polyline2D poly => Some(LoopOf(toSeq(poly.Vertices).Map(v => Pt(v.Location)), poly.IsClosed)),
            Arc arc         => Some(LoopOf(Sampled(arc.PolygonalVertexes(chord.Segments)), Closed: false)),
            Circle circle   => Some(LoopOf(Sampled(circle.PolygonalVertexes(chord.Segments)), Closed: true)),
            Line line       => Some(LoopOf(Seq(Pt(line.StartPoint), Pt(line.EndPoint)), Closed: false)),
            _               => None,
        };

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
    Entity -.->|Text / Insert / Hatch / Spline| Drop["None · dropped"]
    Tess -->|Pt → AsCcw winding| Loops["Arr&lt;Loop&gt;"]
    Loops -.->|empty / non-finite| Degen
    Loops -->|demandClosed| Closed{"all closed?"}
    Closed -.->|non-closed| Open2["FabricationFault.OpenLoop"]
    Closed -->|yes| Out["Fin&lt;Arr&lt;Loop&gt;&gt; · part library"]
```
