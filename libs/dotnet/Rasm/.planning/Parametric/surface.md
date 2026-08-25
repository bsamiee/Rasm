# [RASM_PARAMETRIC_SURFACE]

`Surfaces` owns the surface op algebra of `Rasm.Parametric` and mints its UV-provenance seam: `Tessellate` emits `UvTessellation`, a frozen mesh carrying an index-aligned per-vertex `(u, v)` column beside its live surface binding — the one surface input the tier's downstream consumers admit by type.

Every emitted `NurbsForm.Surface` carries `ToEncodeForm()` into the reconciliation `EncodeForm.Parametric` identity chain.

## [01]-[INDEX]

- [02]-[SURFACES]: `SurfaceOp` folded by one `Apply` over the rule, grade, and policy vocabularies into typed `SurfaceResult` carriers.

## [02]-[SURFACES]

- Owner: `Surfaces` mints the one static entry, `Apply` folding every `SurfaceOp` case through the generated total `Switch`.
- Cases: `SurfaceOp` is the request `[Union]`, one case per surface operation; `SurfaceResult` the result `[Union]`, one typed carrier per request family; rule, grade, and policy rows are the vocabularies the ops read.
- Entry: `Geodesics` takes the `UvTessellation` carrier, so the provenance proof is the parameter type.
- Auto: every op composes the vendored engine with the landed distance, refit, and arena machinery; no evaluation arithmetic is local.
- Law: the curvature bands are `Stat<Scalar>` off `Stat<Scalar>.Of(ReadOnlySpan<double>, key)` — the kernel's ONE moment owner and the leg that already carries the vectorized reduction. NAMED LOSS: the page's local `FieldExtrema` triple and its registered `CurvatureSummaryClaim`; the speed claim belongs to the reduction's owner, and the consumer gains variance, RMS, and the rejected count no triple carries. WITNESS: `FieldExtrema.Of(k1Plane)` rebuilt as `Stat<Scalar>.Of(k1Plane, key)`, whose `Minimum`/`Maximum`/`Mean` read the same three values.
- Law: the area integral rides `Quadrature.Integrate` over `IntegrationDomain.Rectangle`, never a raw `Integrate.OnRectangle` — the funnel's finite guard, skip budget, and `QuadratureEvidence` are the receipt a bare product rule cannot produce, and a pole in `|Su×Sv|` poisons an unguarded weighted sum silently.
- Law: the dense-pullback seed lookup is `NeighborIndex` — Rasm `RULINGS [02]` seats bare-point neighborhoods there, and the query subject is a bare point. NAMED LOSS: the page-local `Supercluster.KDTree.Net` admission, its per-probe boxing of three doubles into an `IReadOnlyList<double>`, and a `.First()` that threw on an empty answer; the gain is one batch query, one owner, and a `Fin` rail through the seed leg.
- Receipt: `GeodesicField.Grade` records the distance lane a consumer dispatches on; `UvTessellation` carries no receipt, the carrier its own provenance evidence.
- Packages: `nurbs.md` the vendored engine (`NurbsPolicy` knobs, `SplinePolicy` the G5 refit seed); `Rasm.Numerics` for `Quadrature.Integrate`/`IntegrationDomain.Rectangle`/`IntervalSpec` area cubature, `Dimension` atoms, and `GeometryFault.ParametricFault`/`ParametricStage`; `Rasm.Spatial` for the `NeighborIndex`/`NeighborSource`/`NeighborKernel` bare-point seed lookup; `Rasm.Meshing` for the `MeshEdit` arena, the `MeshSpace` freeze, the `Chain` ring carrier, and the `Conform` constrained-tessellation carriage; `Rasm.Processing` for the landed distance lanes; `Rasm.Domain` for `Op`, `Context`/`ToleranceLane`, `Stat<Scalar>`/`Scalar`, and validity; Rhino.Geometry, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new tessellation density is one `TessellateRule` case; a new isoline selection one `IsolineRule` case; a second distance lane one `GeodesicGrade` row; a new field quantity one `CurvatureField` column off the same `CurvatureAt` sweep; a lofted, swept, or revolved construction is a growth row on the engine admission.
- Boundary: basis, derivative, and projection arithmetic stay `nurbs.md`'s engine members; a trimmed region is `Trim` DATA on the one `Tessellate` case — the constrained cells ride the `Meshing/delaunay` `Tessellation.Build` substrate with `PlanarOverlay`'s exact winding classification, so THIS owner emits both the full-domain and the trimmed `UvTessellation` and no consumer mints a constrained substrate beside it.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Processing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Parametric;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TessellateRule {
    private TessellateRule() { }

    public sealed record Grid(Dimension Nu, Dimension Nv) : TessellateRule;
    public sealed record Adaptive(Dimension BudgetU, Dimension BudgetV) : TessellateRule;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IsolineRule {
    private IsolineRule() { }

    public sealed record Even(Dimension CountU, Dimension CountV) : IsolineRule;
    public sealed record AtKnots : IsolineRule;
    public sealed record AtParameters(Arr<double> U, Arr<double> V) : IsolineRule;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeodesicGrade {
    public static readonly GeodesicGrade Heat  = new("heat");
    public static readonly GeodesicGrade Exact = new("exact");
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record GeodesicPlan(Arr<Point2d> Sources, Arr<double> Levels, GeodesicGrade Grade) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Sources.Count, floor: 1),
        ValidityClaim.CountAtLeast(count: Levels.Count, floor: 1),
        Levels.All(static level => ValidityClaim.Positive(value: level)));
}

public sealed record PullbackPolicy(Dimension DenseFloor, Dimension SeedU, Dimension SeedV, NurbsPolicy Projection) : IValidityEvidence {
    public static readonly PullbackPolicy Canonical = new(
        DenseFloor: Dimension.Create(value: 32), SeedU: Dimension.Create(value: 24), SeedV: Dimension.Create(value: 24),
        Projection: NurbsPolicy.Canonical);

    public static PullbackPolicy Of(Context context) => Canonical with { Projection = NurbsPolicy.Of(context: context) };

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: SeedU.Value, floor: 2),
        ValidityClaim.CountAtLeast(count: SeedV.Value, floor: 2),
        Projection.IsValid);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceOp {
    private SurfaceOp() { }

    public sealed record Tessellate(NurbsForm.Surface Surface, TessellateRule Rule, Context Model, Option<Seq<Chain>> Trim = default) : SurfaceOp;
    public sealed record Isolines(NurbsForm.Surface Surface, IsolineRule Rule) : SurfaceOp;
    public sealed record Geodesics(SurfaceResult.UvTessellation Source, GeodesicPlan Plan) : SurfaceOp;
    public sealed record NormalOffset(NurbsForm.Surface Surface, double Distance, SplinePolicy Refit, RefinePolicy Refine) : SurfaceOp;
    public sealed record CurvatureSample(NurbsForm.Surface Surface, Dimension Nu, Dimension Nv, Option<NurbsPolicy> Policy = default) : SurfaceOp;
    public sealed record Pullback(NurbsForm.Surface Surface, Arr<Point3d> Probes, PullbackPolicy Policy) : SurfaceOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceResult {
    private SurfaceResult() { }

    public sealed record UvTessellation(NurbsForm.Surface Source, MeshSpace Mesh, Arr<Point2d> Uv) : SurfaceResult;

    public sealed record Isolines(Arr<double> UParameters, Arr<NurbsForm.Curve> UCurves, Arr<double> VParameters, Arr<NurbsForm.Curve> VCurves) : SurfaceResult;

    public sealed record GeodesicField(Arr<int> Offsets, Arr<Point2d> Uv, Arr<Point3d> World, Arr<double> LevelOf, GeodesicGrade Grade) : SurfaceResult;

    public sealed record Offsets(NurbsForm.Surface Surface, RefineReceipt Receipt) : SurfaceResult;

    public sealed record CurvatureField(
        Arr<Point2d> Uv, Arr<double> K1, Arr<double> K2, Arr<double> Gaussian, Arr<double> Mean,
        Arr<Vector3d> Dir1, Arr<Vector3d> Dir2, Arr<double> AreaElement,
        Stat<Scalar> K1Band, Stat<Scalar> K2Band, double Area, int DegenerateNodes) : SurfaceResult;

    public sealed record Pulled(Arr<Point2d> Uv, Arr<Point3d> Feet, Arr<double> Distances) : SurfaceResult;
}

public static class Surfaces {
    public static Fin<SurfaceResult> Apply(SurfaceOp op, Op? key = null) =>
        op.Switch(
            state: key.OrDefault(),
            tessellate:      static (k, t) => TessellateOf(t, k),
            isolines:        static (_, i) => IsolinesOf(i),
            geodesics:       static (k, g) => GeodesicsOf(g, k),
            normalOffset:    static (k, o) => NormalOffsetOf(o, k),
            curvatureSample: static (k, c) => CurvatureOf(c, k),
            pullback:        static (k, p) => PullbackOf(p, k));

    // --- [TESSELLATE]
    static Fin<SurfaceResult> TessellateOf(SurfaceOp.Tessellate op, Op key) =>
        Lattice(op.Surface, op.Rule).Bind(grid =>
            op.Trim.Match(
                Some: rings => TrimmedCells(grid, rings, op.Model, key)
                    .Bind(cells => Lift(op.Surface, cells.Uv, _ => cells.Triangles, op.Model, key)),
                None: () => Lift(
                    op.Surface,
                    new Arr<Point2d>([.. grid.U.SelectMany(u => grid.V.Select(v => new Point2d(u, v)))]),
                    points => CellTriangles(grid.U.Length, grid.V.Length, points),
                    op.Model, key)));

    static Fin<(double[] U, double[] V)> Lattice(NurbsForm.Surface surface, TessellateRule rule);
    static (int A, int B, int C)[] CellTriangles(int nu, int nv, ReadOnlySpan<Point3d> points);
    static Fin<(Arr<Point2d> Uv, (int A, int B, int C)[] Triangles)> TrimmedCells((double[] U, double[] V) grid, Seq<Chain> rings, Context model, Op key);

    static Fin<SurfaceResult> Lift(NurbsForm.Surface surface, Arr<Point2d> uv, Func<Point3d[], (int A, int B, int C)[]> cells, Context model, Op key) {
        Point3d[] points = new Point3d[uv.Count];
        for (int i = 0; i < uv.Count; i++) { points[i] = surface.PointAt(uv[i].X, uv[i].Y); }
        using MeshEdit arena = MeshEdit.Of(points, cells(points), model);
        return arena.ToSpace(key).Map(space => (SurfaceResult)new SurfaceResult.UvTessellation(surface, space, uv));
    }

    // --- [ISOLINES]
    static Fin<SurfaceResult> IsolinesOf(SurfaceOp.Isolines op) =>
        IsoRows(op.Surface, op.Rule).Bind(rows =>
            rows.U.TraverseM(u => op.Surface.IsoCurve(u, ParametricDirection.U)).As().Bind(uCurves =>
                rows.V.TraverseM(v => op.Surface.IsoCurve(v, ParametricDirection.V)).As().Map(vCurves =>
                    (SurfaceResult)new SurfaceResult.Isolines(rows.U, new Arr<NurbsForm.Curve>([.. uCurves]), rows.V, new Arr<NurbsForm.Curve>([.. vCurves])))));

    static Fin<(Arr<double> U, Arr<double> V)> IsoRows(NurbsForm.Surface surface, IsolineRule rule);

    // --- [GEODESICS]
    static Fin<SurfaceResult> GeodesicsOf(SurfaceOp.Geodesics op, Op key) =>
        !op.Plan.IsValid
            ? Fault<SurfaceResult>(ParametricStage.Evaluation, ParametricCarrier.Geodesic, "empty sources or non-positive level")
            : VertexDistances(op.Source, op.Plan, key).Map(distances =>
                (SurfaceResult)ChainContours(op.Source, distances, op.Plan));

    static Fin<Arr<double>> VertexDistances(SurfaceResult.UvTessellation source, GeodesicPlan plan, Op key);
    static SurfaceResult.GeodesicField ChainContours(SurfaceResult.UvTessellation source, Arr<double> distances, GeodesicPlan plan);

    // --- [NORMAL_OFFSET]
    static Fin<SurfaceResult> NormalOffsetOf(SurfaceOp.NormalOffset op, Op key) =>
        Refine.Fold(
            op.Refine, GrevilleGrid(op.Surface),
            fit: (grid, round) => OffsetFit(op, grid, round, key),
            densify: Densified,
            unconverged: deviation => new GeometryFault.ParametricFault(ParametricStage.Construction, ParametricCarrier.Surface, $"normal offset unconverged at deviation {deviation}"))
        .Map(final => (SurfaceResult)new SurfaceResult.Offsets(final.Fit, final.Receipt));

    static Arr<Point2d> GrevilleGrid(NurbsForm.Surface surface);
    static Fin<RefineRound<NurbsForm.Surface, Point2d>> OffsetFit(SurfaceOp.NormalOffset op, Arr<Point2d> grid, int round, Op key);
    static Arr<Point2d> Densified(Arr<Point2d> grid, Arr<Point2d> breaching);

    // --- [CURVATURE_SAMPLE]
    static Fin<SurfaceResult> CurvatureOf(SurfaceOp.CurvatureSample op, Op key) {
        NurbsPolicy policy = op.Policy.IfNone(noneValue: NurbsPolicy.Canonical);
        return Quadrature.Integrate(
                new IntegrationDomain.Rectangle(
                    F: (u, v) => {
                        Vector3d[][] skl = op.Surface.RationalDerivatives(u, v);
                        return Vector3d.CrossProduct(skl[1][0], skl[0][1]).Length;
                    },
                    X: new IntervalSpec(Lower: 0.0, Upper: 1.0),
                    Y: new IntervalSpec(Lower: 0.0, Upper: 1.0),
                    Order: policy.GaussOrder.Value),
                control: Some(QuadratureControl.Default with { RequireErrorWitness = false }),
                key: key)
            .Bind(area => SweepCurvature(op, area.Value, key));
    }

    static Fin<SurfaceResult> SweepCurvature(SurfaceOp.CurvatureSample op, double area, Op key);

    // --- [PULLBACK]
    static Fin<SurfaceResult> PullbackOf(SurfaceOp.Pullback op, Op key) =>
        Seeds(op, key)
            .Bind(seeds => op.Probes
                .Zip(seeds, static (probe, seed) => (Probe: probe, Seed: seed))
                .TraverseM(row => op.Surface.ClosestParameter(row.Probe, Some(op.Policy.Projection), row.Seed, key)).As())
            .Map(uv => Emit(op, new Arr<(double U, double V)>([.. uv])));

    static Fin<Arr<Option<(double U, double V)>>> Seeds(SurfaceOp.Pullback op, Op key) {
        if (op.Probes.Count < op.Policy.DenseFloor.Value) {
            return Fin.Succ(new Arr<Option<(double U, double V)>>([.. op.Probes.Map(static _ => Option<(double U, double V)>.None)]));
        }
        (Point3d[] seeds, Point2d[] seedUv) = SeedGrid(op.Surface, op.Policy);
        return NeighborIndex.Of(source: new NeighborSource.StaticCase(Values: toSeq(seeds)), key: key)
            .Bind(index => NeighborKernel.GraphOf(
                index: index, needles: [.. op.Probes], count: Some(1), radius: Option<double>.None, key: key))
            .Map(graph => new Arr<Option<(double U, double V)>>([.. graph.Ids.Select(hits =>
                hits.Length > 0 ? Some((seedUv[hits[0]].X, seedUv[hits[0]].Y)) : Option<(double U, double V)>.None)]));
    }

    static (Point3d[] Seeds, Point2d[] SeedUv) SeedGrid(NurbsForm.Surface surface, PullbackPolicy policy);
    static SurfaceResult Emit(SurfaceOp.Pullback op, Arr<(double U, double V)> uv);

    static Fin<T> Fault<T>(ParametricStage stage, ParametricCarrier carrier, string witness) =>
        Fin.Fail<T>(new GeometryFault.ParametricFault(stage, carrier, witness));
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
    accTitle: Surface op dispatch and result carriers
    accDescr: Surfaces.Apply folds the SurfaceOp cases over nurbs.md engine members into typed SurfaceResult carriers; UvTessellation is the tier seam feeding develop, panelize, and patternmap, and refusals route to GeometryFault.
    Op["SurfaceOp — 6 cases"] -->|"Surfaces.Apply — ONE Switch"| Engine["nurbs.md NurbsForm.Surface members"]
    Engine -->|"lattice PointAt → soup arena → freeze"| UvT["UvTessellation — MeshSpace + (u,v) column + binding"]
    UvT -->|"tier input law"| Consumers["develop.md · panelize.md · patternmap.md"]
    UvT -->|"heat EnsureGeodesicDistances / exact PropagateWindows"| Contours["GeodesicField — UV-domain polylines"]
    Engine -->|"Greville + NormalAt → SurfaceThrough G5 refit"| Offsets["Offsets — REAL NURBS + RefineReceipt"]
    Engine -->|"CurvatureAt sweep + Rectangle cubature"| Field["CurvatureField SoA + Stat&lt;Scalar&gt; bands"]
    Engine -->|"NeighborIndex batch seed → seeded ClosestParameter"| Pulled
    Engine -->|"ToEncodeForm — 2 Directions U/V"| Identity["reconciliation EncodeForm.Parametric"]
    Op -.->|"ParametricFault — Construction / Evaluation"| GeometryFault
```

## [03]-[DENSITY_BAR]

`Surfaces` is the one entry-bearing owner; every other axis is a payload, discriminant, or policy row.

| [INDEX] | [AXIS_CONCERN]     | [OWNER]                  | [RAIL]                            |
| :-----: | :----------------- | :----------------------- | :-------------------------------- |
|  [01]   | Surface op algebra | `SurfaceOp` + `Surfaces` | `Apply → Fin<SurfaceResult>`      |
|  [02]   | Result carrier     | `SurfaceResult`          | carrier (drained at the consumer) |
|  [03]   | Grid rules         | `TessellateRule`         | payload                           |
|  [04]   | Isoline rules      | `IsolineRule`            | payload                           |
|  [05]   | Distance grade     | `GeodesicGrade`          | discriminant                      |
|  [06]   | Geodesic policy    | `GeodesicPlan`           | values (`IValidityEvidence`)      |
|  [07]   | Pullback policy    | `PullbackPolicy`         | values (`IValidityEvidence`)      |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
