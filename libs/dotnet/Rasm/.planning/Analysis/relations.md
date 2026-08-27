# [RASM_ANALYSIS_RELATIONS]

`Relations` owns pairwise geometric relation across the RhinoCommon-native intersection surface — intersection, classification, deviation, self-intersection, and ray casting. One data-driven table binds each type-pair to an admission predicate, a result shape, and a host-`Intersection` compute delegate taking the `Analysis/query` `Env` whole, so a new geometry pair is one row, never an `IntersectXxYy` method family. It is the host-parametric altitude, capturing the host's tolerance-banded machinery. This page's own static class owns it — the `Analyze` facade lives once on `Analysis/query` and this page adds no fragment to it.

Every relation answer is oracle-admitted evidence: the hit `[Union]`, the `RayQuery` request, and `CurveDeviation` declare `IValidityEvidence` and register with the one `Domain/validation` oracle. Curve-like operands recover through the `Domain/normalization` `CurveForm` lease, tangency classification folds the `Numerics/atoms` `VectorRelation.Of` verdict onto the intersection answer, mesh work reads `ToleranceLane.MeshIntersection`, and every pair builder shares the one `Pair` admission spine — with `IntersectionHit` and `RayQuery` the frozen boundary spellings the Grasshopper surface re-enters by name.

## [01]-[INDEX]

- [02]-[RELATION_EVIDENCE]: kind and tangency vocabularies, `RayQuery`, `CurveDeviation`, and `IntersectionHit` under the `HitProjection` roster.
- [03]-[INTERSECTION_TABLE]: `IntersectionResult` shapes, the `PairOrder`/`RayTarget` rows, and the first-match `Relations` table.
- [04]-[RELATION_OPERATIONS]: relation builders, deviation, and self-intersection kernels on the same owner over the one `Pair` admission spine.
- [05]-[DENSITY_BAR]: one owner per axis; a new pair, shape, or relation is a row, a case, or a builder over the one spine.

## [02]-[RELATION_EVIDENCE]

- Owner: `IntersectionKind` and `IntersectionTangency` `[SmartEnum<int>]` — the result-geometry discriminant and the curve-pair contact classification every projection answers; `RayQuery` `readonly record struct` — a ray and a reflection floor admitted through the oracle, its CEILING riding the `RayTarget` family that can serve it; `CurveDeviation` `readonly record struct` — the exact min/max deviation with witness points, the admitted `Tolerance` it was measured against, and a DERIVED verdict; `IntersectionHit` `[Union]` — point, curve, and overlap contact behind facet accessors, with `HitProjection` the roster binding each admitted output to its projection and transfer verdict.
- Cases: `IntersectionHit` closes on point, curve, and overlap contact; `HitProjection` rosters the six admitted outputs, and `Project<TOut>` admits exactly the rows it carries.
- Entry: `IntersectionHit.Project<TOut>` is the one batch projection — it admits every hit through `AcceptValue`, so the oracle rail and never a direct `IsValid` read gates the batch, resolves the `HitProjection` row, and applies that row's transfer verdict: curve payloads survive under the hit and curve outputs alone, every other output and EVERY failure releasing them. Hits and deviations construct only through the table and the deviation kernel.
- Law: `IntersectionKind` carries THREE rows because three producers exist — the point, curve, and overlap answers. Census `Unknown` has no producer at all (`Kind` reads `Point`, the case's own `CurveKind`, or `Overlap`), so it is a reachable spelling nothing means; it DELETES. `IntersectionTangency.Unknown` STAYS: an unclassifiable contact is a real third answer the tangency probe returns when its projection refuses.
- Law: `CurveDeviation.WithinBand` is derived off the carried `Tolerance`, so the stored verdict and the coherence conjunct that re-proved it both die — an incoherent deviation is unrepresentable rather than checked.
- Auto: hit evidence is per-case — a point hit demands a finite point, a curve hit a live valid curve tagged `Curve` or `Overlap`, an overlap hit finite endpoints with valid intervals and any carried sub-curve valid; the batch projection short-circuits on the first invalid hit, so a poisoned table answer never half-projects. `RayQuery` demands a valid anchored direction above tolerance and at least one reflection, its upper bound answering at the `RayTarget` row.
- Law: all three carriers register with the one `Domain/validation` oracle through their `IValidityEvidence` arm.
- Packages: RhinoCommon (`Ray3d`, `Curve`, `Interval`, `Point3d`), `Rasm.Numerics` (`EpsilonPolicy.ZeroTolerance`), `Rasm.Domain` (`Op`/`Fault`, `IValidityEvidence`/`ValidityClaim`, `Tolerance`, the oracle), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new hit facet is one field, one facet accessor, and one claim conjunct; a new projectable output is ONE `HitProjection` row carrying its binding, transfer verdict, and element projection; a new tangency refinement is one `IntersectionTangency` row fed by the enrichment fold.
- Boundary: `IntersectionHit` and `RayQuery` are frozen boundary spellings the host re-enters against by docID. Curve payloads are host resources: a projection that drops a curve without disposing it is the named leak, one that disposes a transferred curve the named use-after-free. Validity is the `ValidityClaim` fold per the `Domain/results` law, never a hand-rolled `&&`-chain; a reflection budget is a `RayTarget` column, never a page-global literal no consumer honours.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Runtime.InteropServices;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class IntersectionKind {
    public static readonly IntersectionKind Point = new(key: 0);
    public static readonly IntersectionKind Overlap = new(key: 1);
    public static readonly IntersectionKind Curve = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class IntersectionTangency {
    public static readonly IntersectionTangency Unknown = new(key: 0);
    public static readonly IntersectionTangency Transversal = new(key: 1);
    public static readonly IntersectionTangency Tangent = new(key: 2);
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct RayQuery(Ray3d Ray, int MaxReflections = 1) : IValidityEvidence {
    public static Fin<RayQuery> Of(Ray3d ray, Option<int> maxReflections = default) =>
        key.OrDefault().AcceptValue(value: new RayQuery(Ray: ray, MaxReflections: maxReflections.IfNone(noneValue: 1)));
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Ray.Position),
        ValidityClaim.Finite(Ray.Direction),
        Ray.Direction.SquareLength > EpsilonPolicy.ZeroTolerance * EpsilonPolicy.ZeroTolerance,
        ValidityClaim.CountAtLeast(count: MaxReflections, floor: 1));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CurveDeviation(
    double MinimumDistance,
    Point3d MinimumA,
    Point3d MinimumB,
    double MaximumDistance,
    Point3d MaximumA,
    Point3d MaximumB,
    Tolerance Band) : IValidityEvidence {
    public ValidityClaim WithinBand => MaximumDistance <= Band.Value;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Nonnegative(MinimumDistance),
        ValidityClaim.Ordered(lower: MinimumDistance, upper: MaximumDistance),
        ValidityClaim.Finite(MinimumA),
        ValidityClaim.Finite(MinimumB),
        ValidityClaim.Finite(MaximumA),
        ValidityClaim.Finite(MaximumB),
        ValidityClaim.Evidence(Some(Band)));
}

[Union]
public abstract partial record IntersectionHit : IValidityEvidence {
    private IntersectionHit() { }
    public sealed record PointCase(Point3d Point, IntersectionTangency Tangency) : IntersectionHit;
    public sealed record CurveCase(Curve Curve, IntersectionKind CurveKind) : IntersectionHit;
    public sealed record OverlapCase(Point3d Start, Point3d End, Interval OverlapA, Interval OverlapB, Option<Curve> Curve) : IntersectionHit;
    public IntersectionKind Kind => Switch(pointCase: static _ => IntersectionKind.Point, curveCase: static c => c.CurveKind, overlapCase: static _ => IntersectionKind.Overlap);
    public IntersectionTangency Tangency => Switch(
        pointCase: static p => p.Tangency,
        curveCase: static _ => IntersectionTangency.Unknown,
        overlapCase: static _ => IntersectionTangency.Unknown);
    public Seq<Curve> Curves => Switch(pointCase: static _ => Seq<Curve>(), curveCase: static c => Seq(c.Curve), overlapCase: static o => o.Curve.ToSeq());
    public Seq<Point3d> Points => Switch(pointCase: static p => Seq(p.Point), curveCase: static _ => Seq<Point3d>(), overlapCase: static o => Seq(o.Start, o.End));
    public Seq<Interval> Intervals => Switch(pointCase: static _ => Seq<Interval>(), curveCase: static _ => Seq<Interval>(), overlapCase: static o => Seq(o.OverlapA, o.OverlapB));
    public static IntersectionHit At(Point3d point, Option<IntersectionTangency> tangency = default) => new PointCase(Point: point, Tangency: tangency.IfNone(IntersectionTangency.Unknown));
    public static IntersectionHit Overlap(Point3d start, Point3d end, Interval overlapA, Interval overlapB, Option<Curve> curve = default) =>
        new OverlapCase(Start: start, End: end, OverlapA: overlapA, OverlapB: overlapB, Curve: curve);
    public bool IsValid => Switch(
        pointCase: static p => ValidityClaim.Finite(p.Point),
        curveCase: static c => ValidityClaim.All(
            c.CurveKind.Equals(IntersectionKind.Curve) || c.CurveKind.Equals(IntersectionKind.Overlap),
            Optional(c.Curve).Exists(static curve => curve.IsValid)),
        overlapCase: static o => ValidityClaim.All(
            ValidityClaim.Finite(o.Start),
            ValidityClaim.Finite(o.End),
            o.OverlapA.IsValid,
            o.OverlapB.IsValid,
            o.Curve.Map(static curve => curve.IsValid).IfNone(noneValue: true)));
    internal Unit Dispose() => Curves.Iter(static curve => curve.Dispose());
    internal static Fin<Seq<TOut>> Project<TOut>(Seq<IntersectionHit> hits) =>
        hits.TraverseM(hit => Acceptance.Value(value: hit)).As().BiBind(
            Succ: admitted => HitProjection.For(output: typeof(TOut))
                .ToFin(new KernelFault.Unsupported(InputType: typeof(IntersectionHit), OutputType: typeof(TOut)))
                .BiBind(
                    Succ: row => Releasing(hits: admitted, transfers: row.Transfers, result: row.Binding.Admit<TOut>(values: row.Of(hits: admitted))),
                    Fail: cause => DropCurves(hits: admitted, result: Fin.Fail<Seq<TOut>>(cause))),
            Fail: cause => DropCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(cause)));
    private static Fin<Seq<TOut>> Releasing<TOut>(Seq<IntersectionHit> hits, bool transfers, Fin<Seq<TOut>> result) =>
        transfers && result.IsSucc ? result : DropCurves(hits: hits, result: result);
    private static Fin<Seq<TOut>> DropCurves<TOut>(Seq<IntersectionHit> hits, Fin<Seq<TOut>> result) {
        _ = hits.Iter(static hit => hit.Dispose());
        return result;
    }
}

[SmartEnum]
internal sealed partial class HitProjection {
    public static readonly HitProjection Hits = new(binding: OutputBinding.Of<IntersectionHit>(), transfers: true,
        of: static hits => hits.Map(static hit => (object)hit));
    public static readonly HitProjection Curves = new(binding: OutputBinding.Of<Curve>(), transfers: true,
        of: static hits => hits.Bind(static hit => hit.Curves).Map(static curve => (object)curve));
    public static readonly HitProjection Points = new(binding: OutputBinding.Of<Point3d>(), transfers: false,
        of: static hits => hits.Bind(static hit => hit.Points).Map(static point => (object)point));
    public static readonly HitProjection Intervals = new(binding: OutputBinding.Of<Interval>(), transfers: false,
        of: static hits => hits.Bind(static hit => hit.Intervals).Map(static interval => (object)interval));
    public static readonly HitProjection Kinds = new(binding: OutputBinding.Of<IntersectionKind>(), transfers: false,
        of: static hits => hits.Map(static hit => (object)hit.Kind));
    public static readonly HitProjection Tangencies = new(binding: OutputBinding.Of<IntersectionTangency>(), transfers: false,
        of: static hits => hits.Map(static hit => (object)hit.Tangency));
    public OutputBinding Binding { get; }
    public bool Transfers { get; }
    [UseDelegateFromConstructor] internal partial Seq<object> Of(Seq<IntersectionHit> hits);
    private static readonly Lazy<FrozenDictionary<Type, HitProjection>> ByOutput =
        new(static () => Items.ToFrozenDictionary(static row => row.Binding.Declared));
    internal static Option<HitProjection> For(Type output) => ByOutput.Value.TryGetValue(key: output, out HitProjection? row) ? Some(row) : None;
}
```

## [03]-[INTERSECTION_TABLE]

- Owner: `IntersectionResult` internal `[Union]` carries the result shape each table row declares, with `Supports` the static admission probe reading `Capability.Universal` for erased ingress, `Native` the per-shape `OutputBinding` both the output test and the unbox read, and `Tag` the whole-result kind the shapes carrying no per-element one publish. `PairOrder` keyless `[SmartEnum]` carries the operand permutation the scan tries, its one generated `Switch` permuting whatever pair it is handed. `Relations` internal static is the page's own owner — `IntersectionCase` its row model (an admission predicate, a declared shape, and a compute delegate), `Pair<TL, TR>` the typed factory whose one `Supports` column serves both the build-time probe and the runtime scan, `RayTarget` the ray-family roster carrying each target's bounce budget, and `IntersectionCases` the table.
- Cases: rows group in bands by derivation — the value-primitive band clamps to finite segments; the curve band derives hits from host events; the solid band derives from solved arrays with join-curves on; the mesh band runs under `ToleranceLane.MeshIntersection`, the plane row leasing a `MeshIntersectionCache` and the mesh/mesh row a `TextLog` with cancellation and progress threading; the ray band is ONE row over the `RayTarget` roster, whose rows carry mesh single-cast, surface multi-reflection, and trim-aware brep casting with the bounce budget each can serve; the lowering band recovers analytic curve-likes through `Normalization.CurveForm` and re-enters the ordered scan exactly once.
- Entry: `Relations.IntersectionOf` is the one compute entry — admit both operands, then fold the `PairOrder` row's attempt sequence, each attempt reading the table for the FIRST row whose `Supports` column accepts the runtime pair, that row alone computing; classification, holding two admitted `Curve` leases, enters that same first-match `Scan` once with no permutation to try. `Relations.ShapeOf` is the build-time shape probe the operation builders gate on, folding the same row's permutation over types.
- Law: `PairOrder.Pairs<T>` is ONE generic permutation — the build-time probe folds it over `Type`, the runtime scan over the admitted values — so the probe and the scan cannot disagree on which pairs a row is offered.
- Law: only a MISSING ROW admits the next attempt. Scans seed on `KernelFault.Unsupported` and continue solely while the settled cause is that fault, so a cancelled mesh/mesh pair or an invalid ray stops the fold rather than being masked by a second lookup that also has no row.
- Auto: finite-segment discipline is per-row — line/line demands both parameters in `[0, 1]`, line/circle and line/sphere filter to the finite segment at `ToleranceLane.Distance`, line/box clamps the interval against `[0, 1]` preserving direction. Event-derived rows lease the host event set and convert overlap events under the re-parameterization law: when a `Line` proxy clamps, the A-interval re-derives through the B-interval's normalized clamp so both intervals describe the same clamped overlap, trimming the sub-curve off the source; the clamping band travels WITH the line in one `Option` pair, so a band riding no line is unrepresentable. Solved-array rows demand the host flag, and the rows naming `Solved`'s `acceptPartial` fact — curve/brep and the trim-aware ray — also accept found geometry a false-returning host still reports; every row tags curves `Curve` or `Overlap` per its semantic. Trim-aware ray rows size a proxy `LineCurve` by the target's bounding diagonal and keep only forward hits (`(p − origin) · direction ≥ 0`). Host intersectors taking an OVERLAP tolerance beside their distance tolerance read two lanes — `ToleranceLane.Distance` and the coincidence lane `ToleranceLane.Weld` — so neither axis rides the other's value.
- Packages: RhinoCommon (`Intersection.LineLine`/`LinePlane`/`PlanePlane`/`LineCircle`/`LineSphere`/`LineBox`/`CurveLine`/`CurveCurve`/`CurvePlane`/`CurveBrepFace`/`CurveBrep`/`CurveSurface`/`SurfaceSurface`/`BrepPlane`/`BrepSurface`/`BrepBrep`/`MeshLineSorted`/`MeshPlane`/`MeshMesh`/`MeshRay`/`RayShoot`, `CurveIntersections`, `MeshIntersectionCache`, `LineCircleIntersection`/`LineSphereIntersection`, `TextLog`), `Rasm.Domain` (`ToleranceLane` rows, `Normalization.CurveForm`, `Capability` rows, `Lease`, `HostEdge.Slot`, `Op`/`Fault`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new geometry pair is ONE row — admission, shape, compute — and every relation operation, output gate, and consumer reads it with zero edits; a new result shape is one `IntersectionResult` case + its `Native`, `Tag`, and `Elements` arms, `CanProject` and `Project` reading them; a new host intersector (a SubD band when the host ships one) is rows, never a parallel dispatcher.
- Boundary: the table IS the dispatch — a `switch` over type pairs or an `IntersectAB` method family beside it is the deleted form. Every host-minted disposable is leased under `using` or `Lease`, and a host-minted ARRAY the row does not transfer into a hit carrier releases on every non-transferring exit; a bare host handle crossing an expression boundary is the named leak. Mesh rows thread `ToleranceLane.MeshIntersection` and return `Errors.Cancelled` on a direct token poll, never an empty result. This table captures the host's parametric machinery and never re-mints the predicate-exact robust computation.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
internal sealed partial class PairOrder {
    public static readonly PairOrder Ordered = new();
    public static readonly PairOrder Unordered = new();
    internal Seq<(T Left, T Right)> Pairs<T>(T left, T right) => Switch(
        state: (Left: left, Right: right),
        ordered: static pair => Seq(pair),
        unordered: static pair => Seq(pair, (Left: pair.Right, Right: pair.Left)));
}

[SmartEnum]
internal sealed partial class RayTarget {
    public static readonly RayTarget MeshCast = new(reflections: Dimension.Create(value: 1),
        admits: static type => typeof(Mesh).IsAssignableFrom(c: type),
        shoot: static (query, target, _, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(
            Intersection.MeshRay(mesh: (Mesh)target, ray: query.Ray) switch {
                double t when ValidityClaim.Nonnegative(value: t).Holds => Seq(IntersectionHit.At(point: query.Ray.PointAt(t: t))),
                _ => Seq<IntersectionHit>(),
            })));
    public static readonly RayTarget SurfaceCast = new(reflections: Dimension.Create(value: 1000),
        admits: static type => typeof(Surface).IsAssignableFrom(c: type),
        shoot: static (query, target, _, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(
            toSeq(Intersection.RayShoot(Seq((GeometryBase)target).AsIterable(), query.Ray, query.MaxReflections) ?? [])
                .Map(static hit => IntersectionHit.At(point: hit.Point)))));
    public static readonly RayTarget BrepCast = new(reflections: Dimension.Create(value: 1),
        admits: static type => typeof(Brep).IsAssignableFrom(c: type) || Capability.BrepForm.Admits(type: type),
        shoot: static (query, target, env, op) => target is Brep brep
            ? Relations.TrimAwareRay(query: query, brep: brep, env: env)
            : Normalization.BrepForm(source: target, key: op).Bind(lease => lease.Use(body: lowered => Relations.TrimAwareRay(query: query, brep: lowered, env: env), key: op)));
    public Dimension Reflections { get; }
    [UseDelegateFromConstructor] internal partial bool Admits(Type type);
    [UseDelegateFromConstructor] internal partial Fin<IntersectionResult> Shoot(RayQuery query, object target, Env env);
    internal static Option<RayTarget> For(Type target) => toSeq(Items).Find(predicate: row => row.Admits(type: target));
    internal Fin<RayQuery> Admit(RayQuery query) =>
        query.MaxReflections <= Reflections.Value ? Fin.Succ(query) : Fin.Fail<RayQuery>(new KernelFault.InvalidInput(Axis: Some(nameof(RayQuery.MaxReflections))));
}

[Union]
internal abstract partial record IntersectionResult {
    private IntersectionResult() { }
    public sealed record Lines(Seq<Line> Values) : IntersectionResult;
    public sealed record Points(Seq<Point3d> Values) : IntersectionResult;
    public sealed record Intervals(Seq<Interval> Values) : IntersectionResult;
    public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult;
    public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
    internal static readonly IntersectionResult LinesShape = new Lines(Values: Seq<Line>());
    internal static readonly IntersectionResult PointsShape = new Points(Values: Seq<Point3d>());
    internal static readonly IntersectionResult IntervalsShape = new Intervals(Values: Seq<Interval>());
    internal static readonly IntersectionResult PolylinesShape = new Polylines(Values: Seq<(Polyline Curve, IntersectionKind Kind)>());
    internal static readonly IntersectionResult HitsShape = new Hits(Values: Seq<IntersectionHit>());
    internal static bool Supports(Type left, Type right, Type output, PairOrder order) =>
        Capability.Universal(type: left) || Capability.Universal(type: right)
            ? Seq(LinesShape, PointsShape, IntervalsShape, PolylinesShape, HitsShape).Exists(shape => shape.CanProject(output: output))
            : Relations.ShapeOf(left: left, right: right, output: output, order: order).IsSome;
    internal OutputBinding Native => Switch(
        lines: static _ => OutputBinding.Of<Line>(),
        points: static _ => OutputBinding.Of<Point3d>(),
        intervals: static _ => OutputBinding.Of<Interval>(),
        polylines: static _ => OutputBinding.Of<Polyline>(),
        hits: static _ => OutputBinding.Of<IntersectionHit>());
    private Seq<object> Elements => Switch(
        lines: static l => l.Values.Map(static value => (object)value),
        points: static p => p.Values.Map(static value => (object)value),
        intervals: static i => i.Values.Map(static value => (object)value),
        polylines: static p => p.Values.Map(static row => (object)row.Curve),
        hits: static h => h.Values.Map(static value => (object)value));
    internal bool CanProject(Type output) => Switch(
        state: output,
        lines: Serves, points: Serves, intervals: Serves,
        polylines: static (o, p) => o == typeof(IntersectionKind) || Serves(output: o, shape: p),
        hits: static (o, _) => HitProjection.For(output: o).IsSome);
    internal Fin<Seq<TOut>> Project<TOut>() => Switch(
        state: key,
        lines: Uniform<TOut>, points: Uniform<TOut>, intervals: Uniform<TOut>,
        polylines: static (k, p) => typeof(TOut) == typeof(IntersectionKind)
            ? OutputBinding.Of<IntersectionKind>().Admit<TOut>(values: p.Values.Map(static row => (object)row.Kind), key: k)
            : Uniform<TOut>(key: k, shape: p),
        hits: static (k, h) => IntersectionHit.Project<TOut>(hits: h.Values, key: k));
    private static bool Serves(Type output, IntersectionResult shape) => shape.Native.Declared == output;
    private static Fin<Seq<TOut>> Uniform<TOut>(IntersectionResult shape) =>
        shape.Native.Admit<TOut>(values: shape.Elements);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Relations {
    private readonly record struct IntersectionCase(
        Func<Type, Type, bool> Supports,
        IntersectionResult Shape,
        Func<object, object, Env, Fin<IntersectionResult>> Compute) {
        internal static IntersectionCase Pair<TL, TR>(IntersectionResult shape, Func<TL, TR, Env, Fin<IntersectionResult>> compute) where TL : notnull where TR : notnull =>
            new(
                Supports: static (l, r) => typeof(TL).IsAssignableFrom(l) && typeof(TR).IsAssignableFrom(r),
                Shape: shape,
                Compute: (left, right, env, op) => compute(arg1: (TL)left, arg2: (TR)right, arg3: env, arg4: op));
    }
    private static readonly Seq<IntersectionCase> IntersectionCases = Seq(
        IntersectionCase.Pair<Line, Line>(IntersectionResult.PointsShape, static (a, b, env, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineLine(lineA: a, lineB: b, a: out double ta, b: out double _, tolerance: env.Context.For(lane: ToleranceLane.Distance).Value, finiteSegments: true) ? Seq(a.PointAt(t: ta)) : Seq<Point3d>()))),
        IntersectionCase.Pair<Line, Plane>(IntersectionResult.PointsShape, static (a, b, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LinePlane(a, b, out double t) && t is >= 0.0 and <= 1.0 ? Seq(a.PointAt(t: t)) : Seq<Point3d>()))),
        IntersectionCase.Pair<Plane, Plane>(IntersectionResult.LinesShape, static (a, b, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Lines(Intersection.PlanePlane(a, b, out Line line) ? Seq(line) : Seq<Line>()))),
        IntersectionCase.Pair<Line, Circle>(IntersectionResult.PointsShape, static (a, b, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineCircle(a, b, out double t1, out Point3d p1, out double t2, out Point3d p2) switch {
                LineCircleIntersection.Single when t1 is >= 0.0 and <= 1.0 => Seq(p1),
                LineCircleIntersection.Multiple => Seq((T: t1, Point: p1), (T: t2, Point: p2)).Filter(static hit => hit.T is >= 0.0 and <= 1.0).Map(static hit => hit.Point),
                _ => Seq<Point3d>(),
            }))),
        IntersectionCase.Pair<Line, Sphere>(IntersectionResult.PointsShape, static (a, b, env, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineSphere(a, b, out Point3d p1, out Point3d p2) switch {
                LineSphereIntersection.Single when OnFiniteLine(line: a, point: p1, band: env.Context.For(lane: ToleranceLane.Distance)) => Seq(p1),
                LineSphereIntersection.Multiple => Seq(p1, p2).Filter(point => OnFiniteLine(line: a, point: point, band: env.Context.For(lane: ToleranceLane.Distance))),
                _ => Seq<Point3d>(),
            }))),
        IntersectionCase.Pair<Line, BoundingBox>(IntersectionResult.IntervalsShape, static (a, b, env, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, env.Context.For(lane: ToleranceLane.Distance).Value, out Interval interval) ? SegmentInterval(interval: interval) : Seq<Interval>()))),
        IntersectionCase.Pair<Line, Box>(IntersectionResult.IntervalsShape, static (a, b, env, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, env.Context.For(lane: ToleranceLane.Distance).Value, out Interval interval) ? SegmentInterval(interval: interval) : Seq<Interval>()))),
        IntersectionCase.Pair<Line, Curve>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            CurveAgainst(a: b, b: a, env: env, intersect: static (curve, line, tolerance, overlap) => Intersection.CurveLine(curve, line, tolerance, overlap), clamp: Some((Line: a, Band: env.Context.For(lane: ToleranceLane.Distance))))),
        IntersectionCase.Pair<Curve, Curve>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            new Lease<CurveIntersections>.Owned(Value: Intersection.CurveCurve(a, b, env.Context.For(lane: ToleranceLane.Distance).Value, env.Context.For(lane: ToleranceLane.Weld).Value)).Use(hits =>
                env.Cancellation.IsCancellationRequested ? Fin.Fail<IntersectionResult>(Errors.Cancelled) : HitsFromEvents(hits: Optional(hits), source: Some(a)))),
        IntersectionCase.Pair<Curve, Plane>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            CurveAgainst(a: a, b: b, env: env, intersect: static (curve, plane, tolerance, _) => Intersection.CurvePlane(curve, plane, tolerance))),
        IntersectionCase.Pair<Curve, Line>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            CurveAgainst(a: a, b: b, env: env, intersect: static (curve, line, tolerance, overlap) => Intersection.CurveLine(curve, line, tolerance, overlap), clamp: Some((Line: b, Band: env.Context.For(lane: ToleranceLane.Distance))))),
        IntersectionCase.Pair<Curve, BrepFace>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            Solved(acceptPartial: false, solved: Intersection.CurveBrepFace(a, b, env.Context.For(lane: ToleranceLane.Distance).Value, out Curve[] curves, out Point3d[] points), curves: Optional(curves), points: Optional(points), kind: IntersectionKind.Overlap, cancel: env.Cancellation)),
        IntersectionCase.Pair<Curve, Brep>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            Solved(acceptPartial: true, solved: Intersection.CurveBrep(a, b, env.Context.For(lane: ToleranceLane.Distance).Value, out Curve[] curves, out Point3d[] points), curves: Optional(curves), points: Optional(points), kind: IntersectionKind.Overlap, cancel: env.Cancellation)),
        IntersectionCase.Pair<Curve, Surface>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            CurveAgainst(a: a, b: b, env: env, intersect: static (curve, surface, tolerance, overlap) => Intersection.CurveSurface(curve, surface, tolerance, overlap))),
        IntersectionCase.Pair<Surface, Surface>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            Solved(acceptPartial: false, solved: Intersection.SurfaceSurface(a, b, env.Context.For(lane: ToleranceLane.Distance).Value, out Curve[] curves, out Point3d[] points), curves: Optional(curves), points: Optional(points), kind: IntersectionKind.Curve, cancel: env.Cancellation)),
        IntersectionCase.Pair<Brep, Plane>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            Solved(acceptPartial: false, solved: Intersection.BrepPlane(a, b, env.Context.For(lane: ToleranceLane.Distance).Value, out Curve[] curves, out Point3d[] points), curves: Optional(curves), points: Optional(points), kind: IntersectionKind.Curve, cancel: env.Cancellation)),
        IntersectionCase.Pair<Brep, Surface>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            Solved(acceptPartial: false, solved: Intersection.BrepSurface(brep: a, surface: b, tolerance: env.Context.For(lane: ToleranceLane.Distance).Value, joinCurves: true, intersectionCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: Optional(curves), points: Optional(points), kind: IntersectionKind.Curve, cancel: env.Cancellation)),
        IntersectionCase.Pair<Brep, Brep>(IntersectionResult.HitsShape, static (a, b, env, op) =>
            Solved(acceptPartial: false, solved: Intersection.BrepBrep(brepA: a, brepB: b, tolerance: env.Context.For(lane: ToleranceLane.Distance).Value, joinCurves: true, intersectionCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: Optional(curves), points: Optional(points), kind: IntersectionKind.Curve, cancel: env.Cancellation)),
        IntersectionCase.Pair<Mesh, Line>(IntersectionResult.PointsShape, static (a, b, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(toSeq(Intersection.MeshLineSorted(a, b, out int[] _) ?? [])))),
        IntersectionCase.Pair<Mesh, Plane>(IntersectionResult.PolylinesShape, static (a, b, env, _) =>
            new Lease<MeshIntersectionCache>.Owned(Value: new MeshIntersectionCache()).Use(cache =>
                Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(
                    toSeq(Optional(Intersection.MeshPlane(mesh: a, cache: cache, plane: b, tolerance: env.Context.For(lane: ToleranceLane.MeshIntersection).Value, overlaps: true)).ToSeq().Bind(static found => found))
                        .Map(static polyline => (Curve: polyline, Kind: IntersectionKind.Curve)))))),
        IntersectionCase.Pair<Mesh, Mesh>(IntersectionResult.PolylinesShape, static (a, b, env, op) =>
            new Lease<TextLog>.Owned(Value: new TextLog()).Use(log =>
                Intersection.MeshMesh(meshes: [a, b], tolerance: env.Context.For(lane: ToleranceLane.MeshIntersection).Value, intersections: out Polyline[] crossings, overlapsPolylines: true, overlapsPolylinesResult: out Polyline[] overlaps, overlapsMesh: false, overlapsMeshResult: out Mesh _, textLog: log, cancel: env.Cancellation, progress: HostEdge.Slot(env.Progress)) switch {
                    true => Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(
                        toSeq(Optional(crossings).ToSeq().Bind(static found => found)).Map(static polyline => (Curve: polyline, Kind: IntersectionKind.Curve))
                        + toSeq(Optional(overlaps).ToSeq().Bind(static found => found)).Map(static polyline => (Curve: polyline, Kind: IntersectionKind.Overlap)))),
                    false when env.Cancellation.IsCancellationRequested => Fin.Fail<IntersectionResult>(Errors.Cancelled),
                    false => Fin.Fail<IntersectionResult>(new KernelFault.InvalidResult()),
                })),
        new IntersectionCase(
            Supports: static (l, r) => l == typeof(RayQuery) && RayTarget.For(target: r).IsSome,
            Shape: IntersectionResult.HitsShape,
            Compute: static (left, right, env, op) =>
                RayTarget.For(target: right.GetType()).ToFin(new KernelFault.Unsupported(InputType: right.GetType(), OutputType: typeof(IntersectionResult)))
                    .Bind(row => row.Admit(query: (RayQuery)left, op: op).Bind(query => row.Shoot(query: query, target: right, env: env)))),
        new IntersectionCase(
            Supports: static (l, r) => l != typeof(Curve) && !typeof(Curve).IsAssignableFrom(c: l) && Capability.CurveForm.Admits(type: l)
                && (Capability.CurveForm.Admits(type: r) || r == typeof(Plane) || r == typeof(Line) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r) || typeof(BrepFace).IsAssignableFrom(r)),
            Shape: IntersectionResult.HitsShape,
            Compute: static (left, right, env, op) =>
                Normalization.CurveForm(source: left, key: op).Bind(lease => lease.Use(body: curve => Scan(left: curve, right: right, env: env), key: op))),
        new IntersectionCase(
            Supports: static (l, r) => r != typeof(Curve) && !typeof(Curve).IsAssignableFrom(c: r) && Capability.CurveForm.Admits(type: r)
                && (Capability.CurveForm.Admits(type: l) || l == typeof(Plane) || l == typeof(Line) || typeof(Surface).IsAssignableFrom(l) || typeof(Brep).IsAssignableFrom(l) || typeof(BrepFace).IsAssignableFrom(l)),
            Shape: IntersectionResult.HitsShape,
            Compute: static (left, right, env, op) =>
                Normalization.CurveForm(source: right, key: op).Bind(lease => lease.Use(body: curve => Scan(left: left, right: curve, env: env), key: op))));

    internal static Option<IntersectionResult> ShapeOf(Type left, Type right, Type output, PairOrder order) =>
        order.Pairs(left: left, right: right).Fold(
            initialState: Option<IntersectionResult>.None,
            f: (found, row) => found.IsSome ? found : IntersectionCases.Find(predicate: entry => entry.Supports(row.Left, row.Right) && entry.Shape.CanProject(output)).Map(static entry => entry.Shape));
    internal static Fin<IntersectionResult> IntersectionOf<TL, TR>(TL left, TR right, Env env, PairOrder order) where TL : notnull where TR : notnull =>
        (Optional(left).ToFin(new KernelFault.InvalidInput()), Optional(right).ToFin(new KernelFault.InvalidInput())).Apply(static (l, r) => (L: (object)l, R: (object)r)).As()
            .Bind(pair => order.Pairs(left: pair.L, right: pair.R).Fold(
                initialState: Fin.Fail<IntersectionResult>(new KernelFault.Unsupported(pair.L.GetType(), pair.R.GetType())),
                f: (settled, attempt) => settled.BindFail(cause =>
                    cause is KernelFault.Unsupported
                        ? Scan(left: attempt.Left, right: attempt.Right, env: env)
                        : Fin.Fail<IntersectionResult>(cause))));
    internal static Fin<IntersectionResult> ClassifiedOf<TL, TR>(TL left, TR right, Env env) where TL : notnull where TR : notnull =>
        Normalization.CurveForm(source: left).Bind(leftLease => leftLease.Use(
            body: leftCurve => Normalization.CurveForm(source: right, key: op).Bind(rightLease => rightLease.Use(
                body: rightCurve => Scan(left: leftCurve, right: rightCurve, env: env)
                    .Bind(result => result.Switch(
                        state: (Env: env, Left: leftCurve, Right: rightCurve),
                        lines: Unenriched, points: Unenriched, intervals: Unenriched, polylines: Unenriched,
                        hits: static (s, h) => EnrichTangency(hits: h.Values, left: s.Left, right: s.Right, context: s.Env.Context)
                            .Map(static enriched => (IntersectionResult)new IntersectionResult.Hits(Values: enriched)))),
                key: op))));
    private static Fin<IntersectionResult> Unenriched<TState>(TState _, IntersectionResult shape) => Fin.Succ(shape);
    private static Fin<IntersectionResult> Scan(object left, object right, Env env) =>
        env.Cancellation.IsCancellationRequested
            ? Fin.Fail<IntersectionResult>(Errors.Cancelled)
            : IntersectionCases.Find(predicate: row => row.Supports(left.GetType(), right.GetType()))
                .ToFin(new KernelFault.Unsupported(left.GetType(), right.GetType()))
                .Bind(row => row.Compute(arg1: left, arg2: right, arg3: env, arg4: op));
    private static bool OnFiniteLine(Line line, Point3d point, Tolerance band) =>
        ValidityClaim.Finite(point).Holds && point.DistanceTo(other: line.ClosestPoint(testPoint: point, limitToFiniteSegment: true)) <= band.Value;
    private static Seq<Interval> SegmentInterval(Interval interval) =>
        (Math.Min(interval.T0, interval.T1), Math.Max(interval.T0, interval.T1)) switch {
            (double min, double max) when Math.Max(min, 0.0) <= Math.Min(max, 1.0) => Seq(new Interval(
                t0: interval.T0 <= interval.T1 ? Math.Max(min, 0.0) : Math.Min(max, 1.0),
                t1: interval.T0 <= interval.T1 ? Math.Min(max, 1.0) : Math.Max(min, 0.0))),
            _ => Seq<Interval>(),
        };
    private static Fin<IntersectionResult> HitsFromEvents(Option<CurveIntersections> hits, Option<Curve> source = default, Option<(Line Line, Tolerance Band)> clamp = default) =>
        hits.Match(
            Some: native => toSeq(native.AsIterable()).Partition(predicate: static hit => hit.IsPoint || hit.IsOverlap) switch {
                (_, Seq<IntersectionEvent> foreign) when !foreign.IsEmpty => Fin.Fail<IntersectionResult>(new KernelFault.InvalidResult(Detail: Some(nameof(IntersectionEvent)))),
                (Seq<IntersectionEvent> modelled, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(Values: modelled.Bind(hit => hit switch {
                    { IsPoint: true } => clamp.Map(c => OnFiniteLine(line: c.Line, point: hit.PointB, band: c.Band)).IfNone(noneValue: true)
                        ? Seq(IntersectionHit.At(point: hit.PointA))
                        : Seq<IntersectionHit>(),
                    _ => (clamp.IsSome
                        ? SegmentInterval(interval: hit.OverlapB).Head.Map(clamped => (A: new Interval(t0: hit.OverlapA.ParameterAt(hit.OverlapB.NormalizedParameterAt(clamped.T0)), t1: hit.OverlapA.ParameterAt(hit.OverlapB.NormalizedParameterAt(clamped.T1))), B: clamped))
                        : Some((A: hit.OverlapA, B: hit.OverlapB))).Map(overlap => source
                            .Map(curve => IntersectionHit.Overlap(start: curve.PointAt(t: overlap.A.T0), end: curve.PointAt(t: overlap.A.T1), overlapA: overlap.A, overlapB: overlap.B, curve: Optional(curve.Trim(domain: overlap.A))))
                            .IfNone(IntersectionHit.Overlap(start: hit.PointA, end: hit.PointA2, overlapA: overlap.A, overlapB: overlap.B))).ToSeq(),
                }))),
            },
            None: static () => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(Values: Seq<IntersectionHit>())));
    private static Fin<IntersectionResult> Solved(bool acceptPartial, bool solved, Option<Curve[]> curves, Option<Point3d[]> points, IntersectionKind kind, CancellationToken cancel) =>
        (Curves: toSeq(curves.IfNone([])), Points: toSeq(points.IfNone([]))) switch {
            (Seq<Curve> found, Seq<Point3d> hits) => (solved || (acceptPartial && (!found.IsEmpty || !hits.IsEmpty)), cancel.IsCancellationRequested) switch {
                (_, true) => Releasing(owned: found, result: Fin.Fail<IntersectionResult>(Errors.Cancelled)),
                (true, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(Values: found.Map<IntersectionHit>(curve => new IntersectionHit.CurveCase(Curve: curve, CurveKind: kind)) + hits.Map(static point => IntersectionHit.At(point: point)))),
                _ => Releasing(owned: found, result: Fin.Fail<IntersectionResult>(new KernelFault.InvalidResult())),
            },
        };
    private static Fin<IntersectionResult> Releasing(Seq<Curve> owned, Fin<IntersectionResult> result) {
        _ = owned.Iter(static curve => curve.Dispose());
        return result;
    }
    private static Fin<IntersectionResult> CurveAgainst<TRight>(Curve a, TRight b, Env env, Func<Curve, TRight, double, double, CurveIntersections?> intersect, Option<(Line Line, Tolerance Band)> clamp = default) {
        using CurveIntersections? hits = intersect(
            arg1: a, arg2: b,
            arg3: env.Context.For(lane: ToleranceLane.Distance).Value,
            arg4: env.Context.For(lane: ToleranceLane.Weld).Value);
        return HitsFromEvents(hits: Optional(hits), source: Some(a), clamp: clamp);
    }
    private static Fin<Seq<IntersectionHit>> EnrichTangency(Seq<IntersectionHit> hits, Curve left, Curve right, Context context) =>
        hits.TraverseM(hit => hit switch {
            IntersectionHit.PointCase point when point.Tangency.Equals(IntersectionTangency.Unknown) =>
                TangencyAt(left: left, right: right, point: point.Point, context: context)
                    .Map(tangency => IntersectionHit.At(point: point.Point, tangency: Some(tangency))),
            _ => Fin.Succ(hit),
        }).As();
    private static Fin<IntersectionTangency> TangencyAt(Curve left, Curve right, Point3d point, Context context) =>
        (left.ClosestPoint(testPoint: point, t: out double tl), right.ClosestPoint(testPoint: point, t: out double tr)) switch {
            (true, true) => Rasm.Numerics.VectorRelation.Of(a: left.TangentAt(t: tl), b: right.TangentAt(t: tr), context: context)
                .Map(static relation => relation.Equals(Rasm.Numerics.VectorRelation.Parallel) || relation.Equals(Rasm.Numerics.VectorRelation.AntiParallel)
                    ? IntersectionTangency.Tangent
                    : IntersectionTangency.Transversal)
                .BindFail(static cause => cause is KernelFault.Unsupported or KernelFault.InvalidResult
                    ? Fin.Succ(IntersectionTangency.Unknown)
                    : Fin.Fail<IntersectionTangency>(cause)),
            _ => Fin.Succ(IntersectionTangency.Unknown),
        };
    internal static Fin<IntersectionResult> TrimAwareRay(RayQuery query, Brep brep, Env env) {
        BoundingBox box = brep.GetBoundingBox(accurate: true);
        using LineCurve ray = new(line: new Line(
            start: query.Ray.Position,
            direction: query.Ray.Direction,
            length: query.Ray.Position.DistanceTo(other: box.Center) + box.Diagonal.Length));
        return box.IsValid
            ? Solved(
                acceptPartial: true,
                solved: Intersection.CurveBrep(ray, brep, env.Context.For(lane: ToleranceLane.Distance).Value, out Curve[] curves, out Point3d[] points),
                curves: Optional(curves),
                points: Some<Point3d[]>([.. points.Where(point => (point - query.Ray.Position) * query.Ray.Direction >= 0.0)]),
                kind: IntersectionKind.Overlap,
                cancel: env.Cancellation)
            : Fin.Fail<IntersectionResult>(new KernelFault.InvalidResult());
    }
}
```

## [04]-[RELATION_OPERATIONS]

- Owner: the same `Relations` owner's operations half — `Intersect` the unordered table pair, `Classify` the curve-pair table with tangency enrichment, `Deviate` the exact curve-deviation pair, `SelfIntersect` curve self-events and mesh perforation capture, `Cast` the admitted `RayQuery` against a single target — over one `Pair` admission spine shared by every pair builder and the deviation and self-intersection kernels.
- Entry: each builder is the target of an `Analysis/query` relation-band case, `Pair`- or `Single`-dispatched; build-time gates read `IntersectionResult.Supports`, `Capability.CurveForm` admission of both deviation operands, and the curve-or-mesh self-intersection admission seated on `SelfIntersect`, so an inadmissible combination rejects onto `KernelFault.Unsupported` before any geometry is touched.
- Auto: `Pair` resolves the pair through `RequirementContext.Pair` — kind-resolve both operands, apply `Requirement.Basic`, under cancellation — except when one operand is a `RayQuery`: the ray is a request value, not geometry, so it admits through `Op.AcceptInput` while the geometry operand alone runs the readiness gate on whichever side it rides. `Deviate` escalates both operands to `Requirement.CurveLength`, since a below-tolerance curve carries no meaningful deviation. `SelfIntersect` runs under `Requirement.Basic` and discriminates curve (leased `Intersection.CurveSelf` events) from mesh (`GetSelfIntersections` at `ToleranceLane.MeshIntersection`, perforations tagged `Curve`, overlaps `Overlap`), a direct cancellation poll returning `Errors.Cancelled`. Every builder projects through the shape's `Project<TOut>`, so the output gate, oracle, and curve-disposal law apply uniformly.
- Law: `CurveDeviation` constructs once and admits WHOLE through `AcceptValue` — its own claims refuse a non-finite, negative, or unordered host answer, so no per-field admission precedes it — and its band is the admitted `ToleranceLane.Deviation` `Tolerance` the verdict derives from; intersection answers carry no side carrier — the typed hits ARE the evidence, each oracle-admitted.
- Packages: RhinoCommon (`Curve.GetDistancesBetweenCurves`/`ClosestPoint`/`TangentAt`/`Trim`/`PointAt`, `Intersection.CurveSelf`, `Mesh.GetSelfIntersections`, `CurveIntersections`, `TextLog`), `Rasm.Domain` (`RequirementContext.Pair`, `Requirement` rows, `Normalization.CurveForm`, `Capability.CurveForm`, `Tolerance`/`ToleranceLane`, `Lease`, `HostEdge.Slot`, `Op`/`Fault`), `Rasm.Numerics` (`VectorRelation.Of` — the tangency verb), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new pairwise relation (a clearance query, a minimal-distance witness pair) is one builder over the same `Pair` spine with its kernel — admission, preparation, and projection are inherited; a new self-intersecting form is one `SelfIntersectionOf` arm with its disjunct in the `SelfIntersect` gate.
- Boundary: `Pair` is the one pair-admission spine — a builder re-deriving kind resolution, readiness, or ray asymmetry locally is the deleted repetition. Classification never re-intersects and never re-leases: it opens the curve pair ONCE and hands the live natives to both the scan and the enrichment, a second curve-pair intersector or a second form recovery for tangency being the killed form; the tangency probe degrades to `Unknown` on the projection's OWN refusal — `KernelFault.Unsupported` and `KernelFault.InvalidResult` alone — since an unclassifiable contact is still a contact, while a cancellation or bad input rides out as itself rather than reading as a verdict. Deviation is exact by contract — the host extremum computation, never a sampled estimate, and `Analysis/measure`'s sampled conformance pipeline short-circuits to `DeviationOf` when exactness is demanded. Self-intersection disposal is total: event sets lease, and mesh polylines lift into owned curves the hit carriers dispose under the projection law.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Relations {
    internal static Operation<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        Pair<TA, TB, TOut>(supported: IntersectionResult.Supports(left: typeof(TA), right: typeof(TB), output: typeof(TOut), order: PairOrder.Unordered),
            compute: static (a, b, env, op) => IntersectionOf(left: a, right: b, env: env, order: PairOrder.Unordered));
    internal static Operation<(TA A, TB B), TOut> Classify<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        Pair<TA, TB, TOut>(supported: Capability.CurveForm.Admits(type: typeof(TA)) && Capability.CurveForm.Admits(type: typeof(TB))
                       && IntersectionResult.Supports(left: typeof(TA), right: typeof(TB), output: typeof(TOut), order: PairOrder.Unordered)
                       && (typeof(TOut) == typeof(IntersectionHit) || typeof(TOut) == typeof(IntersectionTangency)),
            compute: static (a, b, env, op) => ClassifiedOf(left: a, right: b, env: env));
    internal static Operation<(TA A, TB B), TOut> Deviate<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        (Capability.CurveForm.Admits(type: typeof(TA)) && Capability.CurveForm.Admits(type: typeof(TB)) && typeof(TOut) == typeof(CurveDeviation))
            ? Operation<(TA A, TB B), TOut>.Build(requiresContext: true, state: key,
                evaluator: static (op, pair) =>
                    from runtime in Env.EnvAsks
                    from resolved in runtime.Context.Pair(a: pair.A, b: pair.B, requirements: static (_, _, _) => Fin.Succ((A: Requirement.CurveLength, B: Requirement.CurveLength)), cancel: runtime.Cancellation).ToFin().ToEff()
                    from deviation in DeviationOf(left: resolved.A, right: resolved.B, context: runtime.Context).ToEff()
                    from result in AnalysisOutput<TOut>.Project(key: op, values: Seq(deviation)).ToEff()
                    select result)
            : key.Unsupported<(TA A, TB B), TOut>();
    internal static Operation<TGeometry, TOut> SelfIntersect<TGeometry, TOut>() where TGeometry : notnull =>
        ((typeof(TGeometry) == typeof(object)
            || typeof(Curve).IsAssignableFrom(c: typeof(TGeometry))
            || typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)))
         && IntersectionResult.HitsShape.CanProject(output: typeof(TOut)))
            ? Operation<TGeometry, TOut>.Build(requirement: Some(Requirement.Basic), state: key,
                evaluator: static (op, geometry) =>
                    from runtime in Env.EnvAsks
                    from result in SelfIntersectionOf(geometry: geometry, env: runtime).ToEff()
                    from typed in result.Project<TOut>().ToEff()
                    select typed)
            : new KernelFault.Unsupported();
    internal static Operation<TGeometry, TOut> Cast<TGeometry, TOut>(RayQuery query) where TGeometry : notnull =>
        IntersectionResult.Supports(left: typeof(RayQuery), right: typeof(TGeometry), output: typeof(TOut), order: PairOrder.Ordered)
            ? Operation<TGeometry, TOut>.Build(requiresContext: true, state: (Key: key, Query: query),
                evaluator: static (state, geometry) =>
                    from runtime in Env.EnvAsks
                    from ray in Acceptance.Input(value: state.Query).ToEff()
                    from ready in Requirement.Basic.Apply(context: runtime.Context, value: geometry, cancel: runtime.Cancellation).ToFin().ToEff()
                    from result in IntersectionOf(left: ray, right: ready, env: runtime, order: PairOrder.Ordered).ToEff()
                    from typed in result.Project<TOut>().ToEff()
                    select typed)
            : new KernelFault.Unsupported();

    internal static Fin<CurveDeviation> DeviationOf<TL, TR>(TL left, TR right, Context context) where TL : notnull where TR : notnull =>
        Normalization.CurveForm(source: left)
            .Bind(leftLease => leftLease.Use(leftCurve => Normalization.CurveForm(source: right)
                .Bind(rightLease => rightLease.Use(rightCurve => DeviationOf(left: leftCurve, right: rightCurve, context: context)))));
    internal static Fin<CurveDeviation> DeviationOf(Curve left, Curve right, Context context) {
        Tolerance band = context.For(lane: ToleranceLane.Deviation);
        return Curve.GetDistancesBetweenCurves(curveA: left, curveB: right, tolerance: band.Value, maxDistance: out double maxDistance, maxDistanceParameterA: out double maxA, maxDistanceParameterB: out double maxB, minDistance: out double minDistance, minDistanceParameterA: out double minA, minDistanceParameterB: out double minB) switch {
            true => Acceptance.Value(value: new CurveDeviation(
                MinimumDistance: minDistance, MinimumA: left.PointAt(t: minA), MinimumB: right.PointAt(t: minB),
                MaximumDistance: maxDistance, MaximumA: left.PointAt(t: maxA), MaximumB: right.PointAt(t: maxB),
                Band: band)),
            false => Fin.Fail<CurveDeviation>(new KernelFault.InvalidResult()),
        };
    }
    internal static Fin<IntersectionResult> SelfIntersectionOf<TGeometry>(TGeometry geometry, Env env) where TGeometry : notnull =>
        Optional(geometry).ToFin(new KernelFault.InvalidInput()).Bind(g => (env.Cancellation.IsCancellationRequested, g) switch {
            (true, _) => Fin.Fail<IntersectionResult>(Errors.Cancelled),
            (_, Curve curve) => new Lease<CurveIntersections>.Owned(Value: Intersection.CurveSelf(curve: curve, tolerance: env.Context.For(lane: ToleranceLane.Distance).Value)).Use(hits => HitsFromEvents(hits: Optional(hits), source: Some(curve))),
            (_, Mesh mesh) => new Lease<TextLog>.Owned(Value: new TextLog()).Use(log =>
                mesh.GetSelfIntersections(tolerance: env.Context.For(lane: ToleranceLane.MeshIntersection).Value, perforations: out Polyline[] perforations, overlapsPolylines: true, overlapsPolylinesResult: out Polyline[] overlaps, overlapsMesh: false, overlapsMeshResult: out Mesh _, textLog: log, cancel: env.Cancellation, progress: HostEdge.Slot(env.Progress)) switch {
                    true => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(Values:
                        toSeq(Optional(perforations).ToSeq().Bind(static found => found)).Map<IntersectionHit>(static polyline => new IntersectionHit.CurveCase(Curve: polyline.ToNurbsCurve(), CurveKind: IntersectionKind.Curve))
                        + toSeq(Optional(overlaps).ToSeq().Bind(static found => found)).Map<IntersectionHit>(static polyline => new IntersectionHit.CurveCase(Curve: polyline.ToNurbsCurve(), CurveKind: IntersectionKind.Overlap)))),
                    false when env.Cancellation.IsCancellationRequested => Fin.Fail<IntersectionResult>(Errors.Cancelled),
                    false => Fin.Fail<IntersectionResult>(new KernelFault.InvalidResult()),
                }),
            _ => Fin.Fail<IntersectionResult>(new KernelFault.Unsupported(g.GetType(), typeof(IntersectionResult))),
        });
    private static Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(bool supported, Func<TA, TB, Env, Fin<IntersectionResult>> compute) where TA : notnull where TB : notnull =>
        supported switch {
            true => Operation<(TA A, TB B), TOut>.Build(requiresContext: true, state: (Key: key, Compute: compute),
                evaluator: static (state, pair) =>
                    from runtime in Env.EnvAsks
                    from resolved in (RayRole(a: pair.A, b: pair.B).Case switch {
                        (RayQuery query, GeometryBase target, bool rayLeads) =>
                            (Acceptance.Input(value: query), Requirement.Basic.Apply(context: runtime.Context, value: target, cancel: runtime.Cancellation).ToFin())
                                .Apply((admitted, ready) => rayLeads
                                    ? (A: (TA)(object)admitted, B: (TB)(object)ready)
                                    : (A: (TA)(object)ready, B: (TB)(object)admitted)).As(),
                        _ => runtime.Context.Pair(a: pair.A, b: pair.B, requirements: static (_, _, _) => Fin.Succ((A: Requirement.Basic, B: Requirement.Basic)), cancel: runtime.Cancellation)
                            .ToFin()
                            .Map(static resolved => (resolved.A, resolved.B)),
                    }).ToEff()
                    from result in state.Compute(resolved.A, resolved.B, runtime, state.Key).ToEff()
                    from typed in result.Project<TOut>().ToEff()
                    select typed),
            false => key.Unsupported<(TA A, TB B), TOut>(),
        };
    private static Option<(RayQuery Query, GeometryBase Target, bool RayLeads)> RayRole<TA, TB>(TA a, TB b) =>
        (a, b) switch {
            (RayQuery query, GeometryBase target) => Some((Query: query, Target: target, RayLeads: true)),
            (GeometryBase target, RayQuery query) => Some((Query: query, Target: target, RayLeads: false)),
            _ => Option<(RayQuery Query, GeometryBase Target, bool RayLeads)>.None,
        };
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
    accTitle: Geometric relation dispatch band
    accDescr: The query relation band dispatching pair and single builders through a twenty-four-row intersection case table whose PairOrder-driven first-match read yields the shape union under one validity oracle, with curve-form lowering, tangency enrichment, and exact deviation beside the predicate-exact meshing path that coexists uncalled.
    Query[Analysis/query relation band] -->|Pair / Single dispatch| Builders[Intersect · Classify · Deviate · SelfIntersect · Cast]
    Builders -->|Pair spine: kind-resolve × Requirement × ray asymmetry| Order[PairOrder attempts]
    Order -->|first-match row read, Unsupported-only continuation| Table[24-row IntersectionCases]
    Table -->|acceptPartial fact + RayTarget budget| Shapes[IntersectionResult: Lines · Points · Intervals · Polylines · Hits]
    Table -->|curve-form lowering rows| Norm[Normalization.CurveForm lease]
    Shapes -->|OutputBinding.Admit + transfer verdict| Oracle[one validity oracle]
    Builders -->|VectorRelation.Of tangency fold| Hits[IntersectionHit enrichment]
    Builders -->|GetDistancesBetweenCurves exact| Deviation[CurveDeviation + Tolerance band]
    Robust[settled Meshing/intersect — predicate-exact] -.->|coexists, never called| Table
```

## [05]-[DENSITY_BAR]

One owner per axis; a new pair, shape, or relation is a row, a case, or a builder over the one spine — never a sibling dispatcher. Every evidence carrier registers with the one validity oracle through `IValidityEvidence`.

| [INDEX] | [CONCERN]           | [OWNER]                | [KIND]                                | [RESULT]                          | [CASES] |
| :-----: | :------------------ | :--------------------- | :------------------------------------ | :-------------------------------- | :-----: |
|  [01]   | Contact kind        | `IntersectionKind`     | `[SmartEnum<int>]` result kind        | row (pure)                        |    3    |
|  [02]   | Contact tangency    | `IntersectionTangency` | `[SmartEnum<int>]` curve-pair contact | row (pure)                        |    3    |
|  [03]   | Operand permutation | `PairOrder`            | `[SmartEnum]` generic `Pairs<T>` row  | `Seq` of pairs (pure)             |    2    |
|  [04]   | Ray request         | `RayQuery`             | `record struct`, admitted `Of`        | `Fin<RayQuery>` → oracle          |    —    |
|  [05]   | Deviation           | `CurveDeviation`       | 7-field carrier, derived verdict      | evidence → oracle                 |    —    |
|  [06]   | Hit evidence        | `IntersectionHit`      | `[Union]` + facets + `HitProjection`  | `Fin<Seq<TOut>>` gate             |    3    |
|  [07]   | Result shape        | `IntersectionResult`   | internal `[Union]` + `OutputBinding`  | generated `Switch` → output gate  |    5    |
|  [08]   | Pair dispatch       | `Relations`            | 24-row table + `Find` + attempt fold  | `Fin<IntersectionResult>`         |   24    |
|  [09]   | Relation operations | `Relations`            | 5 `Pair` builders + kernels           | `Operation → Eff<Env, Seq<TOut>>` |    5    |

`RequirementContext.Pair`, the `Requirement` rows, and the oracle are `Domain/validation` law; the form recoveries are `Domain/normalization` law; the tangency relation is `Numerics/atoms` law — composed here, legislated there.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
