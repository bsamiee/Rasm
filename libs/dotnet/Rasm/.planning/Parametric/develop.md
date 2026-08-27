# [RASM_PARAMETRIC_DEVELOP]

`Rasm.Parametric` develops a UV-provenanced surface into guaranteed-isometric planar strips, the exact-isometry fabrication tier: `Development.Apply` folds near-developable bands between exact geodesic edges, torsal ruling lines per band, and a rigid length-preserving unroll. Each strip proves itself through a per-strip `ddouble` isometry witness — a Fabrication acceptance reads its edge-length defect as evidence — and a strip over budget faults on its unit, never emitting an unwitnessed atlas.

`surface.md`'s `SurfaceResult.UvTessellation` — mesh, per-vertex `(u, v)`, and a live `NurbsForm.Surface` binding — carries the input, and edges compose through `Surfaces.Apply(SurfaceOp.Geodesics)` under `Some(WindowPropagationPolicy.Default)`. Emission converges on `flatten.md`'s `ChartAtlas` carrier type, and strip layout composes through QuikGraph.

## [01]-[INDEX]

- [02]-[DEVELOPMENT]: `DevelopOp` the two-case request `[Union]` folded by ONE `Apply` into strip decomposition, torsal rulings, and the exact unroll, `Isometry` carrying the `ddouble` isometry evidence.

## [02]-[DEVELOPMENT]

- Owner: `DevelopPolicy` the policy row (`Width` the geodesic edge spacing admitted once as `PositiveMagnitude` · `Stations` the per-strip station count · `Torsal` the lane-resolved ruling residual gate · `Isometry` the lane-resolved per-strip witness ceiling the acceptance reads · `Seed` the UV seed polyline the distance field grows from, `None` = the `u = 0` boundary isoline, an empty supplied polyline canonicalized to `None` at `Of`); `DevelopOp` the request `[Union]` (`Decompose` the strip partition + rulings for inspection · `Unroll` the full pipeline through the witness and atlas); `StripField` the SoA wire (edge offset-columns in UV · ruling endpoint/residual columns — component labels and the layout forest are `Emit`-local, computed after unrolling and never stored as decomposition output); `Isometry` the evidence row (the per-strip `Witness` column with its one derived `Stat<Scalar>` band, `Witness.Count` the strip census · `Torsal` the one `Stat<Scalar>` derived off the field's `TorsalResidual` column, `Torsal.Count` the ruling census · `Components`); `DevelopmentResult` the result `[Union]`; `Development` the static entry.
- Cases: `DevelopOp` cases `Decompose` · `Unroll` (2 — inspection versus fabrication modality, `Unroll` composing `Decompose`'s own fold, never a re-derivation); `DevelopmentResult` cases `Strips` · `Unrolled` (2).
- Entry: `public static Fin<DevelopmentResult> Apply(DevelopOp op)` — the ONE entry discriminating on the op case; both cases take the `SurfaceResult.UvTessellation` carrier, so the UV-provenance input law is the parameter TYPE.
- Auto: `Decompose` composes the geodesic distance through `Surfaces.Apply(SurfaceOp.Geodesics)` under `Some(WindowPropagationPolicy.Default)` on the `k·Width` ladder, takes the iso-distance contours as strip EDGES (UV and world columns lerp-consistent by the tessellation's own provenance), assigns faces to bands by vertex distance, and roots the torsal coplanarity residual per station (arc-spaced on the lower edge) through `Brent.TryFindRoot` coupled by one `Try.lift`-funnelled `Broyden.FindRoot` pass; a station short of the `Torsal` band records into `TorsalResidual` rather than faulting, because a mildly non-torsal ruling that still unrolls within budget is fabrication-acceptable and the witness is the acceptance criterion. `Unroll` develops each strip by rigid placement on exact edge lengths — ruling quads split on the shorter diagonal, the triangle chain seated from the origin by two-circle intersection with no solve or relaxation — accumulates the squared edge defect and its edge count per strip in `ddouble`, answers the RMS defect as the WITNESS, narrows at readout onto the isometry band, and faults `Isometry` breaches as `Strip`; layout folds strip adjacency into an `UndirectedGraph` of length-tagged shared-rail edges, reads `ConnectedComponents` and the `Tag`-weighted `MinimumSpanningTreeKruskal`, threads that tagged forest through `PlacementOrder` so placement never rescans the field, and the atlas emits one `UvIsland` per strip with edge `FeatureEdge` cuts beside a cross-check `Distortion`.
- Law: the layout tree is KRUSKAL, not Prim. Prim returns one component's tree, so on a multi-component strip graph — the ordinary case for a trimmed surface — a Prim ordering silently drops every strip outside the seeded component and the atlas packs a subset while `Components` reports the truth. Kruskal returns the forest the component labels already promise.
- Law: the two gates are `Tolerance` values off `ToleranceLane.Torsal` and `ToleranceLane.Deviation`, minted through `DevelopPolicy.Of(context, width)`. NAMED LOSS: the `DevelopPolicy.Canonical` static and its 1e-8/1e-10 literals; a fabrication gate a document's own tolerance did not set is a number nobody can defend at acceptance. `Width` stays a caller value because an edge spacing is a design width in model units, not a tolerance; it crosses the `AcceptValidated` bridge once at `Of`, so the interior never re-gates the scalar.
- Law: the isometry witness is the RMS edge defect, a LENGTH, so it gates against a `ToleranceLane.Deviation` band of the same dimension and carries no edge count. NAMED LOSS: the raw sum of squares — a length² growing with tessellation density, under which a fine strip failed the band a coarse one at identical per-edge defect cleared. The comparison stays in `ddouble` (`policy.Isometry.Value` widens, the witness never narrows) so the 106-bit accumulation survives the gate.
- Output: `Isometry` — the per-strip isometry witness column with its one derived `Stat<Scalar>` band, the torsal-residual band derived off its own column, component count, the strip and ruling census read off `Witness.Count` and `Torsal.Count` rather than stored twice — the Fabrication unroll dry-run and Generation developable-gate evidence; the `ChartAtlas.Distortion` rides BESIDE it for carrier-type compatibility, never instead of it.
- Packages: `Rasm.Parametric` `surface.md` (`SurfaceResult.UvTessellation` the input carrier; `Surfaces.Apply(SurfaceOp.Geodesics)` the exact-edge composition) + `nurbs.md` (`NurbsForm.Surface.NormalAt`/`RationalDerivatives` — the ruling normals and strip evaluation), `Rasm.Processing` (`GeodesicKernel.PropagateWindows` machinery surfaced through the surface geodesic lane; `FeatureEdge`/`MeshFeatureKind` — the cut-edge rows `segment.md` mints; `ChartAtlas`/`UvIsland`/`Distortion`/`ChartId` — the flatten carrier types, composed never re-minted), `Rasm.Meshing` (`MeshSpace`), TYoshimura.DoubleDouble (`ddouble` — the 106-bit cancellation-safe witness accumulation, `INumber<ddouble>`-bound fold), MathNet.Numerics (`Brent.TryFindRoot` the per-station torsal root; `Broyden.FindRoot` the coupled station refinement, `Try.lift`-funnelled), QuikGraph (`UndirectedGraph<int, STaggedEdge<int, double>>` + `AddEdgeRange` + `ConnectedComponents` + `MinimumSpanningTreeKruskal` — the layout fold, the shared-rail length riding each edge's `Tag` as the Kruskal weight), `Rasm.Numerics` (`Predicate.Orient2D` the flip-free proof; `Dimension`/`PositiveMagnitude`; the `GeometryFault` union), `Rasm.Domain` (`Try.lift`/`FactoryBridge.Accept`, `Context`/`ToleranceLane`/`Tolerance`, `Stat<Scalar>`/`Scalar`), Rhino.Geometry (`Point3d`/`Vector3d`/`Point2d`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new decomposition driver (principal-curvature-aligned edges instead of distance edges) is one edge-derivation arm feeding the SAME strip fold; a new ruling condition (a cone-point-aware torsal variant) is one residual function the same Brent/Broyden solve roots; a further layout packing modality is one ordering projection beside `PlacementOrder` off the same tagged Kruskal forest; zero new entry surfaces.
- Boundary: this owner holds the EXACT-ISOMETRY tier — re-deriving a conformal or distortion-minimizing solve here, or claiming isometry without the `ddouble` witness, is the tier violation; the input is the `UvTessellation` TYPE and an unbound mesh cannot enter, so the provenance law is structural; edges ride exact MMP propagation — `GeodesicPlan.Windows` present — by law; a heat-lane edge (`Windows` absent) is the drift defect, edge error becoming strip skew becoming witness noise; ruling normals read the surface BINDING at provenance UV — a mesh-normal approximation is the substitution defect; the unroll is rigid placement on exact edge lengths — a spring relaxation, an ARAP pass, or any distortion-minimizing solve here is the tier regression; the witness accumulates in `ddouble` and narrows ONLY at readout — a `double` running sum re-introduces the cancellation the fold exists to kill; every geometric failure routes `NoDevelopableStrips` or `StripIsometryExceeded` with the strip unit and its isometry measure, width admission the `FactoryBridge.Accept` bridge at `DevelopPolicy.Of`, every impossible-result branch the resolved `KernelFault.InvalidResult` channel, no exception crossing the surface.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using DoubleDouble;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Parametric;

// --- [POLICIES] ------------------------------------------------------------------------
public sealed record DevelopPolicy(
    PositiveMagnitude Width, Dimension Stations, Tolerance Torsal, Tolerance Isometry,
    Option<Arr<Point2d>> Seed) {
    public static Fin<DevelopPolicy> Of(
        Context context, double width, Option<Arr<Point2d>> seed = default) =>
        FactoryBridge.Accept<PositiveMagnitude>(candidate: width)
            .Map(admitted => new DevelopPolicy(
                Width: admitted, Stations: Dimension.Create(value: 32),
                Torsal: context.For(lane: ToleranceLane.Torsal),
                Isometry: context.For(lane: ToleranceLane.Deviation),
                Seed: seed.Filter(static points => !points.IsEmpty)));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StripField(
    Arr<int> RailOffsets, Arr<Point2d> RailUv,
    Arr<int> RulingOffsets, Arr<Point2d> RulingA, Arr<Point2d> RulingB, Arr<double> TorsalResidual);

public sealed record Isometry(Arr<double> Witness, Stat<Scalar> Band, Stat<Scalar> Torsal, int Components);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DevelopOp {
    private DevelopOp() { }

    public sealed record Decompose(SurfaceResult.UvTessellation Source, DevelopPolicy Policy) : DevelopOp;
    public sealed record Unroll(SurfaceResult.UvTessellation Source, DevelopPolicy Policy) : DevelopOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record DevelopmentResult {
    private DevelopmentResult() { }

    public sealed record Strips(StripField Field) : DevelopmentResult;

    public sealed record Unrolled(ChartAtlas Atlas, Isometry Isometry) : DevelopmentResult;
}

public static class Development {
    public static Fin<DevelopmentResult> Apply(DevelopOp op) =>
        op.Switch(
            decompose: static d => DecomposeOf(d.Source, d.Policy).Map(static field => (DevelopmentResult)new DevelopmentResult.Strips(field)),
            unroll:    static u => DecomposeOf(u.Source, u.Policy).Bind(field => UnrollOf(u.Source, u.Policy, field)));

    // --- [STRIP_DECOMPOSITION]
    static Fin<StripField> DecomposeOf(SurfaceResult.UvTessellation source, DevelopPolicy policy) =>
        Surfaces.Apply(
                new SurfaceOp.Geodesics(source, new GeodesicPlan(
                    SeedOf(source, policy), LevelLadder(source, policy.Width), Some(WindowPropagationPolicy.Default))))
            .Bind(edges => edges.SwitchPartially(
                state: (Source: source, Policy: policy),
                @default: static (state, _) => Fin.Fail<StripField>(new KernelFault.InvalidResult()),
                geodesicField: static (state, field) =>
                    Rulings(state.Source, state.Policy, field, state.Key)));

    static Arr<Point2d> SeedOf(SurfaceResult.UvTessellation source, DevelopPolicy policy);
    static Arr<double> LevelLadder(SurfaceResult.UvTessellation source, PositiveMagnitude width);

    static Fin<StripField> Rulings(SurfaceResult.UvTessellation source, DevelopPolicy policy, SurfaceResult.GeodesicField edges);

    // --- [EXACT_UNROLL]
    static Fin<DevelopmentResult> UnrollOf(SurfaceResult.UvTessellation source, DevelopPolicy policy, StripField field) =>
        (field.RailOffsets.Count - 1) switch {
            0 => Fin.Fail<DevelopmentResult>(new GeometryFault.NoDevelopableStrips()),
            int strips => Range(0, strips).ToSeq()
                .Traverse(strip => Develop(source, field, strip).Bind(unrolled =>
                    unrolled.Witness <= (ddouble)policy.Isometry.Value
                        ? Fin.Succ(unrolled)
                        : Fin.Fail<UnrolledStrip>(new GeometryFault.StripIsometryExceeded(strip, (double)unrolled.Witness, policy.Isometry))).ToValidation())
                .As().ToFin()
                .Bind(unrolled => Emit(source, field, unrolled, key)),
        };

    internal readonly record struct UnrolledStrip(Arr<int> Vertices, Arr<(int A, int B, int C)> Faces, Arr<Point2d> Planar, ddouble Witness, double MaxJacobianRatio);

    static Fin<UnrolledStrip> Develop(SurfaceResult.UvTessellation source, StripField field, int strip);

    // --- [LAYOUT_AND_ATLAS]
    static Fin<DevelopmentResult> Emit(SurfaceResult.UvTessellation source, StripField field, Seq<UnrolledStrip> strips) {
        UndirectedGraph<int, STaggedEdge<int, double>> adjacency = new(allowParallelEdges: false);
        adjacency.AddVertexRange(Enumerable.Range(0, strips.Count));
        adjacency.AddEdgeRange(SharedRails(field));
        Dictionary<int, int> components = new();
        int componentCount = adjacency.ConnectedComponents(components);
        Arr<int> componentOf = new([.. Enumerable.Range(0, strips.Count).Select(strip => components[strip])]);
        return Atlas(source, field, strips, componentOf, toSeq(adjacency.MinimumSpanningTreeKruskal(
            static edge => 1.0 / (1.0 + edge.Tag))), componentCount, key);
    }

    static Seq<STaggedEdge<int, double>> SharedRails(StripField field);
    internal static Arr<int> PlacementOrder(Seq<UnrolledStrip> strips, Arr<int> componentOf, Seq<STaggedEdge<int, double>> forest);
    static Fin<DevelopmentResult> Atlas(
        SurfaceResult.UvTessellation source, StripField field, Seq<UnrolledStrip> strips,
        Arr<int> componentOf, Seq<STaggedEdge<int, double>> forest, int componentCount);
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Development strip pipeline
    accDescr: Development.Apply decomposes exact geodesic edges into strips, roots torsal rulings, unrolls rigidly under the ddouble witness, and emits the strip atlas through the flatten carrier.
    UvT["surface.md UvTessellation — mesh + (u,v) + binding"] -->|"Development.Apply — ONE Switch"| Edges["Surfaces.Apply(Geodesics) — Windows PINNED present"]
    Edges -->|"iso-distance edges at k·Width"| Strips["strip bands"]
    Strips -->|"torsal g(t) roots — Brent per station, Broyden coupling"| Rulings["ruling rows + TorsalResidual"]
    Rulings -->|"rigid two-circle placement on exact lengths"| Unroll["planar strips"]
    Unroll -->|"ddouble √(Σ(‖e‖₃D−‖e‖₂D)²/edges) — narrow at readout"| Witness["RMS isometry witness — Isometry band gate"]
    Strips -->|"UndirectedGraph → components + Kruskal forest"| Layout["strip layout"]
    Unroll -->|"UvIsland strips · FeatureEdge cuts · Distortion"| AtlasOut["flatten.md ChartAtlas + Isometry"]
    UvT -.->|"NoDevelopableStrips / StripIsometryExceeded"| GeometryFault
```

## [03]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. `[RESULT]` names the owner's one return type.

| [INDEX] | [AXIS_CONCERN]      | [OWNER]                     | [RESULT]                          | [CASES] |
| :-----: | :------------------ | :-------------------------- | :-------------------------------- | :-----: |
|  [01]   | Development algebra | `DevelopOp` + `Development` | `Apply → Fin<DevelopmentResult>`  |    2    |
|  [02]   | Result carrier      | `DevelopmentResult`         | carrier (drained at the consumer) |    2    |
|  [03]   | Strip wire          | `StripField`                | value                             |    —    |
|  [04]   | Policy row          | `DevelopPolicy`             | value (`Of → Fin<DevelopPolicy>`) |    —    |
|  [05]   | Evidence            | `Isometry`                  | value                             |    —    |

One transcription-complete source file carries the op algebra, carriers, and kernels; each signature-pinned kernel's contract rides its in-fence comment. Distance field, projection arithmetic, graph algorithms, moment bands, and atlas types are composed owners; the only local mathematics is the torsal residual and the rigid placement, the pair no admitted surface carries.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
