# [RASM_ANALYSIS_QUERY]

Measured-query runtime owns the kernel's public analysis entry. `AnalysisQuery` `[Union]` is THE one request algebra: twenty-five cases in four bands — geometry, family, relation, spatial — with the call arity recovered from the case through the `Single`/`Pair`/`Service` virtual dispatch, never a name suffix, a verb sibling, or a mode knob. Geometry band IS the absorbed geometry-request vocabulary: `Coerce`/`CurveForm`/`Vertices`/`SamplePoints`/`SurfaceUv`/`Closest`/`SignedDistance` are first-class cases and `Kind`/`Bounds`/`SurfaceForm`/`BrepForm` are factory spellings onto their dispatch-equivalent cases — a second request ADT beside this union, re-dispatched through a mapping switch into the same operations, is the killed form; one vocabulary, one dispatch. Family band forwards to the owning family unions (`Bounds`/`Measure`/`Location`/`Curves`/`Faces`/`Topologies`/`Meshes`/`Points`) through the ONE seam law: every family union exposes `internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>()`, and the `AnalysisQuery` case forwards to it — `Analysis/measure`, `Analysis/inspect`, `Analysis/select`, `Analysis/relations`, and `Parametric/locate` plug in as operation builders, never as parallel entry surfaces. Relation band routes the `Analysis/relations` pairwise lattice (`Intersections`/`Classification`/`CurveDeviation`/`SelfIntersection`/`Ray`/`Conformance`); the spatial band carries the `Spatial/neighbors` substrate (`NeighborIndex`/`NeighborQuery` box/sphere search, index overlaps, KNN/radius point-pairs) as `Service` operations over `Unit`. Union name, factory spellings (`Measure(…)`/`Bounds(…)`/`Selection(…)`/`MeshPointSpatial(…)`/`Location(…)`/`Intersections`/`Classification`/`CurveDeviation`/`SelfIntersection`/`Ray(…)`/`Conformance(…)`/`Search(…)`/`Overlaps(…)`/`PointPairs(…)`), and `Analyze`/`Env`/`Operation` runtime shapes are the frozen host contract — the Grasshopper output binding constructs `Analyze.Query<object, TOut>(query)` and `new Env(Context: …, Progress: …, Cancellation: …)`, the Rhino command context exposes `Analyze.In(context: …)` as a public `Analyze.Scope`, and the overlay runs `Analyze.Run<object, BoundingBox>(query: AnalysisQuery.Bounds(Bounds.AxisAligned), …)`; the host boundary re-enters against these exact spellings.

`Operation<TGeometry, TOut>` is the effect-carrying operation algebra: a private `Body` `[Union]` (`Rejected`/`PerItem`/`Aggregate`/`Service`) behind `Build`/`Reject`/`Service` constructors, a `Prepare` gate that folds cancellation, null-geometry admission, and the `Domain/validation` `Requirement` readiness matrix over every item before evaluation — geometry always earns at least the validity-oracle admission even under an empty requirement — and one `Apply(Seq<TGeometry>) → Eff<Env, Seq<TOut>>` fold that traverses per-item bodies, feeds aggregate bodies the whole prepared sequence, and runs service bodies input-free. `Env` is the reader record (`Context` + `IProgress<double>?` + `CancellationToken` + the optional `TelemetrySink` tap) with the `EnvAsks`/`Asks`/`Taps` runtime projections — the Op-threading law holds corpus-wide: `Op` travels as the explicit value key in operation state, `Eff<Env>` carries the ambient runtime, and no operation smuggles context through a second channel; `Apply` marks a `CostMark` before its body fold and charges the `OpCost` capsule on both exits through the sink, so kernel-grain cost and fault evidence flow with zero emit calls in domain code. `Analyze` is the one facade (a `static partial class` — each family page contributes its operation builders to this single owner): `Scope` binds context/progress/cancellation with `With` combinators and its own `Run`; `From(RhinoDoc)` is the ONE doc-coupled boundary adapter beside `Context.Of(RhinoDoc)`; `In(UnitSystem)`/`In(double, double, double, UnitSystem)`/`In(Context)` are the host-neutral scope builders; `Query` closes three arities and static `Run` closes four overloads over `Validation<Error, Seq<TOut>>`. `AnalysisOutput<TOut>` projects raw evaluator values onto the typed output rail with acceptance DELEGATING to the ONE `Domain/validation` oracle — `Op.AcceptValue` — because every Analysis receipt implements the `Domain/rails` `IValidityEvidence` contract and the oracle's evidence arm admits it with zero Analysis-side switch; a second acceptance oracle that re-declares per-receipt validity arms beside the Domain owner is the killed parallel rail.

Catalog status governs this target shape: `Eff.runtime<Env>()` is decompile-confirmed (`public static Eff<RT, RT> runtime<RT>()` on `LanguageExt.Eff`) yet absent from both applicable `.api` tiers, so both C# fences remain research. No `Env` projection, `Operation.Apply` cost charge, or facade body below is settled until `[EFF_RUNTIME_CATALOG]` arms.

## [01]-[INDEX]

- [02]-[REQUEST_ALGEBRA]: `AnalysisQuery` `[Union]` — four bands, twenty-five cases, the `Single`/`Pair`/`Service` arity dispatch, the absorbed geometry band with its `Analyze` operation builders, the family-union seam law.
- [03]-[OPERATION_RUNTIME_RESEARCH]: `Env` reader target; `Operation<TGeometry, TOut>` `Body` algebra with the `Prepare` gate and `Apply` effect fold; the `Analyze` facade and `AnalysisOutput<TOut>` projection remain conceptual until `[EFF_RUNTIME_CATALOG]` arms.

## [02]-[REQUEST_ALGEBRA]

- Owner: `AnalysisQuery` `[Union]` — the single public request vocabulary (request cases are data, not operations, so the union carries no `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in). GEOMETRY band: `CoerceCase(Type)` gated on `Capability.Coercible` and `Output == typeof(TOut)`; `CurveFormCase` producing the `Rasm.Domain` `CurveForm` classification; `VerticesCase`/`SamplePointsCase(int)` producing `Point3d` streams; `SurfaceUvCase(Point2d)` producing the inverse-evaluated `Point2d` under `Requirement.SurfaceEvaluation`; `ClosestCase(Point3d)` producing the `Domain/evaluation` `ClosestHit` receipt; `SignedDistanceCase(Point3d, ClosestHit)` producing the normal-signed scalar. FAMILY band: `BoundsCase(Bounds)`/`MeasureCase(Measure)`/`LocationCase(Location)`/`CurvesCase(Curves)`/`FacesCase(Faces)`/`TopologyCase(Topologies)`/`MeshesCase(Meshes)`/`PointsCase(Points)` — each forwards to the family union's internal `Operation<TGeometry, TOut>()` builder. RELATION band: `IntersectionsCase`/`ClassificationCase`/`CurveDeviationCase` (pair arity), `SelfIntersectionCase`/`RayCase(RayQuery)` (single arity), `ConformanceCase(ConformanceMetric, int, Seq<double>)` (pair arity). SPATIAL band: `SearchBoxCase(NeighborIndex, BoundingBox)`/`SearchSphereCase(NeighborIndex, Sphere)`/`OverlapCase(NeighborIndex, NeighborIndex, double)`/`PointPairsCase(Seq<Point3d>, Seq<Point3d>, NeighborQuery)` — service arity over the `Spatial/neighbors` substrate.
- Cases: geometry `Coerce` · `CurveForm` · `Vertices` · `SamplePoints` · `SurfaceUv` · `Closest` · `SignedDistance` (7 declared; `Kind`/`Bounds()`/`SurfaceForm`/`BrepForm` factory-preserved); family `Bounds` · `Measure` · `Location` · `Curves` · `Faces` · `Topologies` · `Meshes` · `Points` (8); relation `Intersections` · `Classification` · `CurveDeviation` · `SelfIntersection` · `Ray` · `Conformance` (6); spatial `SearchBox` · `SearchSphere` · `Overlap` · `PointPairs` (4).
- Entry: the three `internal virtual` dispatchers — `Single<TGeometry, TOut>(Op)`, `Pair<TA, TB, TOut>(Op)`, `Service<TOut>(Op)` — each defaulting to `key.Unsupported<…>()` so a case consumed at the wrong arity rejects on the rail, never throws; a case overrides exactly the arities it owns. Consumers reach the dispatch only through `Analyze.Query`/`Analyze.Run` — the union's dispatch surface stays internal.
- Auto: `SurfaceForm`/`BrepForm` collapse onto `Coerce(typeof(Surface))`/`Coerce(typeof(Brep))` because their operations are the same coercion op gated by the same output-type test — three factory spellings, one case, one operation; `Kind` routes to `Selection(Topologies.Kind)` and parameterless `Bounds()` defaults to `Bounds(Bounds.AxisAligned)` because those requests were always the identical operations reached through a second vocabulary. `Conformance` factory computes its percentile payload eagerly — percentiles survive only under `ConformanceMetric.Distribution`, every other metric carries the empty `Seq<double>`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`, generated `Switch`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Eff`), `Rasm.Domain` (`Op`/`Fault`/`Requirement`/`Context`/`Kind` capability web/`CurveForm`/`ClosestHit`/coercion-evaluation extension lattice), `Rasm.Spatial` (`NeighborIndex`/`NeighborQuery`/`NeighborSource`/`NeighborAnswer` — the `Spatial/neighbors` substrate), RhinoCommon (`Point3d`/`Point2d`/`BoundingBox`/`Sphere` payload values).
- Growth: a new query modality is one case with one factory on the owning band — a family page gaining a capability adds a case to ITS union and this algebra is untouched; a new relation is one case forwarding to a `Analysis/relations` builder; a new spatial probe shape is one `NeighborQuery` case on the `Spatial/neighbors` owner. A new band is admitted only by charter amendment.
- Boundary: the request algebra is ONE union — a `GeometryRequest`-style second ADT wrapped by a `Geometry(…)` case and re-dispatched through a `request switch` mapping into the same operations is the collapsed dead form, and the twin coercion builders it forced (`GeometryCoerce` beside `Coerce`) collapse to one; factory spellings preserve every absorbed request (`Kind`, `Bounds`, `CurveForm`, `SurfaceForm`, `BrepForm`, `Vertices`, `SamplePoints`, `SurfaceUv`, `Closest`, `SignedDistance`) so no consumer capability is dropped by the unification; the output-type gates (`Output == typeof(TOut)`, `typeof(TOut) == typeof(CurveForm)`) reject at operation-build time onto the `Fault.Unsupported` rail — code 9104, the host binding's probe discriminant — never at evaluation, while spatial VALUE defects (an invalid box or sphere, a negative or non-finite tolerance) reject `InvalidInput` at build so 9104 stays a pure modality discriminant; the geometry-band operations compose the `Domain/normalization` coercion lattice and the `Domain/evaluation` closest/sampling surface as settled owner vocabulary (`CoerceTo<TOut>`/`CurveForm`/`SurfaceForm`/`CurveFormOf`/`VerticesOf`/`SamplePoints`/`SurfaceUv`/`ClosestOf`/`SignedDistanceOf`), never re-implementing a coercion or an evaluation locally; the spatial band rides ONE service spine — every builder resolves its index, forwards to the `Spatial/neighbors` owner's `NeighborIndex.Query` dispatch (`Box`/`Ball`/`Overlaps`/`Pairs` cases) with the runtime token threaded into the substrate's cancellation capsule, and projects its `NeighborAnswer` union (`Hits(Seq<NeighborHit>)` and `PairsFound(Seq<NeighborPair>)` are the two arms this band lifts, every other answer shape rejects as `InvalidResult`); pair-probe admission is the substrate's own law — volume/overlap/nested probes refuse THERE, so a new pair-admissible probe lands as one upstream case with zero edits here — and a query-side probe whitelist, RTree wrapper, or second answer vocabulary beside that substrate is the deleted parallel rail.

```csharp conceptual
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Parametric;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record AnalysisQuery {
    private AnalysisQuery() { }

    // --- [GEOMETRY_BAND]
    public sealed record CoerceCase(Type Output) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Output == typeof(TOut) ? Analyze.GeometryCoerce<TGeometry, TOut>(key: key) : key.Unsupported<TGeometry, TOut>(); }
    public sealed record CurveFormCase : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => typeof(TOut) == typeof(CurveForm) ? Analyze.GeometryCurveForm<TGeometry, TOut>(key: key) : key.Unsupported<TGeometry, TOut>(); }
    public sealed record VerticesCase : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => typeof(TOut) == typeof(Point3d) ? Analyze.GeometryVertices<TGeometry, TOut>(key: key) : key.Unsupported<TGeometry, TOut>(); }
    public sealed record SamplePointsCase(int Count) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => typeof(TOut) == typeof(Point3d) ? Analyze.GeometrySamples<TGeometry, TOut>(count: Count, key: key) : key.Unsupported<TGeometry, TOut>(); }
    public sealed record SurfaceUvCase(Point2d Uv) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => typeof(TOut) == typeof(Point2d) ? Analyze.GeometrySurfaceUv<TGeometry, TOut>(uv: Uv, key: key) : key.Unsupported<TGeometry, TOut>(); }
    public sealed record ClosestCase(Point3d Target) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => typeof(TOut) == typeof(ClosestHit) ? Analyze.GeometryClosest<TGeometry, TOut>(target: Target, key: key) : key.Unsupported<TGeometry, TOut>(); }
    public sealed record SignedDistanceCase(Point3d Sample, ClosestHit Hit) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => typeof(TOut) == typeof(double) ? Analyze.GeometrySignedDistance<TGeometry, TOut>(sample: Sample, hit: Hit, key: key) : key.Unsupported<TGeometry, TOut>(); }

    // --- [FAMILY_BAND]
    public sealed record BoundsCase(Bounds Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record MeasureCase(Measure Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record LocationCase(Location Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record CurvesCase(Curves Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record FacesCase(Faces Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record TopologyCase(Topologies Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record MeshesCase(Meshes Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record PointsCase(Points Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }

    // --- [RELATION_BAND]
    public sealed record IntersectionsCase : AnalysisQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationIntersection<TA, TB, TOut>(key: key); }
    public sealed record ClassificationCase : AnalysisQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationClassification<TA, TB, TOut>(key: key); }
    public sealed record CurveDeviationCase : AnalysisQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationDeviation<TA, TB, TOut>(key: key); }
    public sealed record SelfIntersectionCase : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Analyze.RelationSelfIntersection<TGeometry, TOut>(key: key); }
    public sealed record RayCase(RayQuery Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Analyze.RelationRay<TGeometry, TOut>(query: Query, key: key); }
    public sealed record ConformanceCase(ConformanceMetric Metric, int Count, Seq<double> Percentiles) : AnalysisQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationConformance<TA, TB, TOut>(metric: Metric, count: Count, percentiles: Percentiles, key: key); }

    // --- [SPATIAL_BAND]
    public sealed record SearchBoxCase(NeighborIndex Index, BoundingBox Box) : AnalysisQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialSearch<TOut>(index: Index, box: Box, key: key); }
    public sealed record SearchSphereCase(NeighborIndex Index, Sphere Sphere) : AnalysisQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialSearch<TOut>(index: Index, sphere: Sphere, key: key); }
    public sealed record OverlapCase(NeighborIndex Left, NeighborIndex Right, double Tolerance) : AnalysisQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialOverlaps<TOut>(left: Left, right: Right, tolerance: Tolerance, key: key); }
    public sealed record PointPairsCase(Seq<Point3d> Points, Seq<Point3d> Needles, NeighborQuery Probe) : AnalysisQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialPointPairs<TOut>(points: Points, needles: Needles, probe: Probe, key: key); }

    // --- [FACTORIES]
    public static AnalysisQuery Kind => new TopologyCase(Query: Topologies.Kind);
    public static AnalysisQuery Coerce(Type output) => new CoerceCase(Output: output);
    public static AnalysisQuery CurveForm => new CurveFormCase();
    public static AnalysisQuery SurfaceForm => new CoerceCase(Output: typeof(Surface));
    public static AnalysisQuery BrepForm => new CoerceCase(Output: typeof(Brep));
    public static AnalysisQuery Vertices => new VerticesCase();
    public static AnalysisQuery SamplePoints(int count) => new SamplePointsCase(Count: count);
    public static AnalysisQuery SurfaceUv(Point2d uv) => new SurfaceUvCase(Uv: uv);
    public static AnalysisQuery Closest(Point3d target) => new ClosestCase(Target: target);
    public static AnalysisQuery SignedDistance(Point3d sample, ClosestHit hit) => new SignedDistanceCase(Sample: sample, Hit: hit);
    public static AnalysisQuery Bounds(Bounds? query = null) => new BoundsCase(Query: query ?? Analysis.Bounds.AxisAligned);
    public static AnalysisQuery Measure(Measure query) => new MeasureCase(Query: query);
    public static AnalysisQuery Location(Location query) => new LocationCase(Query: query);
    public static AnalysisQuery Selection(Curves query) => new CurvesCase(Query: query);
    public static AnalysisQuery Selection(Faces query) => new FacesCase(Query: query);
    public static AnalysisQuery Selection(Topologies query) => new TopologyCase(Query: query);
    public static AnalysisQuery MeshPointSpatial(Meshes query) => new MeshesCase(Query: query);
    public static AnalysisQuery MeshPointSpatial(Points query) => new PointsCase(Query: query);
    public static AnalysisQuery Intersections => new IntersectionsCase();
    public static AnalysisQuery Classification => new ClassificationCase();
    public static AnalysisQuery CurveDeviation => new CurveDeviationCase();
    public static AnalysisQuery SelfIntersection => new SelfIntersectionCase();
    public static AnalysisQuery Ray(RayQuery query) => new RayCase(Query: query);
    public static AnalysisQuery Conformance(ConformanceMetric metric, int count, params double[] percentiles) =>
        new ConformanceCase(Metric: metric, Count: count, Percentiles: Optional(metric).Bind(m => m.Equals(ConformanceMetric.Distribution) ? Some(toSeq(percentiles)) : Option<Seq<double>>.None).IfNone(Seq<double>()));
    public static AnalysisQuery Search(NeighborIndex index, BoundingBox box) => new SearchBoxCase(Index: index, Box: box);
    public static AnalysisQuery Search(NeighborIndex index, Sphere sphere) => new SearchSphereCase(Index: index, Sphere: sphere);
    public static AnalysisQuery Overlaps(NeighborIndex left, NeighborIndex right, double tolerance = 0.0) => new OverlapCase(Left: left, Right: right, Tolerance: tolerance);
    public static AnalysisQuery PointPairs(ReadOnlySpan<Point3d> points, ReadOnlySpan<Point3d> needles, NeighborQuery probe) => new PointPairsCase(Points: Seq(points), Needles: Seq(needles), Probe: probe);

    // --- [ARITY_DISPATCH]
    internal virtual Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull => key.Unsupported<TGeometry, TOut>();
    internal virtual Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) where TA : notnull where TB : notnull where TOut : notnull => key.Unsupported<(TA A, TB B), TOut>();
    internal virtual Operation<Unit, TOut> Service<TOut>(Op key) where TOut : notnull => key.Unsupported<Unit, TOut>();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<TGeometry, TOut> GeometryCoerce<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
        Capability.Coercible(source: typeof(TGeometry), target: typeof(TOut))
            ? Operation<TGeometry, TOut>.Build(key: key, requirement: Requirement.Basic, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from value in geometry.CoerceTo<TOut>(context: context, key: op).ToEff()
                    from output in new AnalysisOutput<TOut>(Key: op).One(value: value).ToEff()
                    select output)
            : key.Unsupported<TGeometry, TOut>();

    internal static Operation<TGeometry, TOut> GeometryCurveForm<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, CurveForm>.Build(key: key, requirement: Requirement.Basic, requiresContext: true, state: key,
            evaluator: static (op, geometry) =>
                from context in Env.Asks
                from form in Normalization.CurveForm(source: geometry, key: op).Bind(lease => lease.Use(curve => Normalization.CurveFormOf(curve: curve, context: context))).ToEff()
                from output in new AnalysisOutput<CurveForm>(Key: op).One(value: form).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    internal static Operation<TGeometry, TOut> GeometryVertices<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, Point3d>.Build(key: key, state: key,
            evaluator: static (op, geometry) =>
                from points in geometry.VerticesOf(key: op).ToEff()
                from output in new AnalysisOutput<Point3d>(Key: op).Many(values: points).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    internal static Operation<TGeometry, TOut> GeometrySamples<TGeometry, TOut>(int count, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, Point3d>.Build(key: key, requiresContext: true, state: (Key: key, Count: count),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from points in geometry.SamplePoints(count: state.Count, context: context, key: state.Key).ToEff()
                from output in new AnalysisOutput<Point3d>(Key: state.Key).Many(values: points).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    internal static Operation<TGeometry, TOut> GeometrySurfaceUv<TGeometry, TOut>(Point2d uv, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, Point2d>.Build(key: key, requirement: Requirement.SurfaceEvaluation, requiresContext: true, state: (Key: key, Uv: uv),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in Normalization.SurfaceForm(source: geometry, key: state.Key).Bind(lease => lease.Use(surface => Evaluation.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key))).ToEff()
                from output in new AnalysisOutput<Point2d>(Key: state.Key).One(value: result).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    internal static Operation<TGeometry, TOut> GeometryClosest<TGeometry, TOut>(Point3d target, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, ClosestHit>.Build(key: key, state: (Key: key, Target: target),
            evaluator: static (state, geometry) =>
                from hit in geometry.ClosestOf(target: state.Target, key: state.Key).ToEff()
                from output in new AnalysisOutput<ClosestHit>(Key: state.Key).One(value: hit).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    internal static Operation<TGeometry, TOut> GeometrySignedDistance<TGeometry, TOut>(Point3d sample, ClosestHit hit, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, double>.Build(key: key, state: (Key: key, Sample: sample, Hit: hit),
            evaluator: static (state, geometry) =>
                from distance in geometry.SignedDistanceOf(hit: state.Hit, sample: state.Sample, key: state.Key).ToEff()
                from output in new AnalysisOutput<double>(Key: state.Key).One(value: distance).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    // --- [SPATIAL_BAND_BUILDERS]
    internal static Operation<Unit, TOut> SpatialSearch<TOut>(NeighborIndex index, BoundingBox box, Op key) where TOut : notnull =>
        (typeof(TOut) == typeof(NeighborHit), box.IsValid) switch {
            (false, _) => key.Unsupported<Unit, TOut>(),
            (_, false) => Operation<Unit, TOut>.Reject(key: key, fault: key.InvalidInput()),
            _ => SpatialService<TOut>(key: key, resolve: _ => Fin.Succ(index), query: new NeighborQuery.BoxCase(Bounds: box), anchor: box.Center),
        };
    internal static Operation<Unit, TOut> SpatialSearch<TOut>(NeighborIndex index, Sphere sphere, Op key) where TOut : notnull =>
        (typeof(TOut) == typeof(NeighborHit), sphere.IsValid) switch {
            (false, _) => key.Unsupported<Unit, TOut>(),
            (_, false) => Operation<Unit, TOut>.Reject(key: key, fault: key.InvalidInput()),
            _ => SpatialService<TOut>(key: key, resolve: _ => Fin.Succ(index), query: new NeighborQuery.BallCase(Ball: sphere), anchor: sphere.Center),
        };
    internal static Operation<Unit, TOut> SpatialOverlaps<TOut>(NeighborIndex left, NeighborIndex right, double tolerance, Op key) where TOut : notnull =>
        (typeof(TOut) == typeof(NeighborPair), double.IsFinite(tolerance) && tolerance >= 0.0) switch {
            (false, _) => key.Unsupported<Unit, TOut>(),
            (_, false) => Operation<Unit, TOut>.Reject(key: key, fault: key.InvalidInput()),
            _ => SpatialService<TOut>(key: key, resolve: _ => Fin.Succ(left), query: new NeighborQuery.OverlapsCase(Other: right, Tolerance: tolerance), anchor: Point3d.Origin),
        };
    internal static Operation<Unit, TOut> SpatialPointPairs<TOut>(Seq<Point3d> points, Seq<Point3d> needles, NeighborQuery probe, Op key) where TOut : notnull =>
        typeof(TOut) == typeof(NeighborPair)
            ? SpatialService<TOut>(key: key, resolve: op => NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: points), key: op), query: new NeighborQuery.PairsCase(Needles: needles, Probe: probe), anchor: Point3d.Origin)
            : key.Unsupported<Unit, TOut>();
    private static Operation<Unit, TOut> SpatialService<TOut>(Op key, Func<Op, Fin<NeighborIndex>> resolve, NeighborQuery query, Point3d anchor) where TOut : notnull =>
        Operation<Unit, TOut>.Service(key: key, state: (Key: key, Resolve: resolve, Query: query, Anchor: anchor), evaluate: static state =>
            from runtime in Env.EnvAsks
            from index in state.Resolve(state.Key).ToEff()
            from answer in index.Query(query: state.Query, anchor: state.Anchor, key: state.Key, cancel: runtime.Cancellation).ToEff()
            from projected in ProjectAnswer<TOut>(answer: answer, key: state.Key).ToEff()
            select projected);
    private static Fin<Seq<TOut>> ProjectAnswer<TOut>(NeighborAnswer answer, Op key) => answer switch {
        NeighborAnswer.Hits found => new AnalysisOutput<TOut>(Key: key).Many(values: found.Values),
        NeighborAnswer.PairsFound found => new AnalysisOutput<TOut>(Key: key).Many(values: found.Values),
        _ => Fin.Fail<Seq<TOut>>(key.InvalidResult()),
    };
}
```

## [03]-[OPERATION_RUNTIME_RESEARCH]

This section preserves the decision-complete target body without settling its catalog-absent reader constructor.

- Owner: `Env` `[BoundaryAdapter]` — the `Eff` reader runtime (`Context` + `IProgress<double>?` + `CancellationToken` + `TelemetrySink?`) with the three static projections `EnvAsks` (the whole runtime), `Asks` (the `Context` alone), and `Taps` (the sink as `Option<TelemetrySink>`); the record shape is host-frozen — the Grasshopper binding constructs it positionally, and the sink is the defaulted trailing parameter so every frozen construction spelling survives unchanged. `Operation<TGeometry, TOut>` — the operation algebra: private `Body` `[Union]` (`Rejected(Error)` / `PerItem(Func<TGeometry, Eff<Env, Seq<TOut>>>)` / `Aggregate(Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>)` / `Service(Func<Eff<Env, Seq<TOut>>>)`); public `Key`, internal `Requirement`/`RequiresContext`/`IsSupported`/`IsAggregate`/`NeedsContext`; one constructor per `Body` case — `Build` wires the `Prepare` gate ahead of every per-item evaluator, `Aggregate` folds the same gate across the whole input before its one sequence projection, `Reject` carries a build-time fault, `Service` runs input-free over its own explicit state record — no constructor argument another constructor silently discards; `Apply(Seq<TGeometry>)` is the ONE execution fold over the `Body` `Switch`, opening a `CostMark` before the fold and charging the `OpCost` capsule through the `Env` tap on both exits — the fail exit also publishes the fault fact, and an absent sink costs one null test. `Analyze` — the facade: `Scope` (a `Fin<Context>`-carrying record with `With(IProgress<double>)`/`With(CancellationToken)`/`With(TelemetrySink)` and its own `Run`), `From(RhinoDoc?)`, `In(UnitSystem)`/`In(double, double, double, UnitSystem)`/`In(Context)`, three `Query` arities resolving an `AnalysisQuery?` onto typed operations, and four static `Run` overloads: single-query input, pair-query input, service-query input, and an already-built operation over single input. `AnalysisOutput<TOut>` `[BoundaryAdapter]` — the typed projection gate (`One`/`Many`/`Objects`) admitting every value through `Op.AcceptValue`, the one oracle; enumerable host ingress is `Op.Accept(values:)`'s job, never a second arm here.
- Entry: `Analyze.Run<TGeometry, TOut>(AnalysisQuery, params ReadOnlySpan<TGeometry>)` / `Run<TA, TB, TOut>(AnalysisQuery, params ReadOnlySpan<(TA A, TB B)>)` / `Run<TOut>(AnalysisQuery)` / `Run<TGeometry, TOut>(Operation<TGeometry, TOut>?, params ReadOnlySpan<TGeometry>)` → `Validation<Error, Seq<TOut>>`; scoped execution through `Analyze.In(…).With(progress).With(cancel).Run(operation, input)`; operation construction through `Analyze.Query<…>(query, key)`. One entry family, four overloads discriminated by query/input shape or an already-built operation — no `RunMany`/`RunPair`/`RunService` verb siblings.
- Auto: `Prepare` folds — cancellation first (`Fault.Cancelled`), null-admission second (`Fault.MissingGeometry`), then the `Requirement` matrix; an EMPTY requirement still routes `GeometryBase` values through `Requirement.Apply`'s validity-oracle admission, so no geometry reaches an evaluator unvetted while non-geometry service payloads pass untouched. Scope-less `Run` resolves context by need: an operation with `NeedsContext` and no scope fails `Fault.MissingContext`; a context-free operation defaults to `Context.Of(units: UnitSystem.Millimeters)`. `Apply` flattens per-item chunks (`TraverseM` + `Bind`), feeds aggregates the whole prepared `Seq`, and lifts a `Rejected` body's fault onto the effect rail — rejection is data until execution.
- Receipt: none on a dedicated rail — `Validation<Error, Seq<TOut>>` IS the public result carrier; faults accumulate the `Domain/rails` `Fault` union, and `Fault.Unsupported` (code 9104) is the probe discriminant the host binding branches on.
- Packages: LanguageExt.Core (`Validation`/`Fin` rails and `TraverseM`; `Eff.runtime<Env>()` remains catalog-blocked), Thinktecture.Runtime.Extensions (`Body` `[Union]` + generated `Switch`), `Rasm.Domain` (`Context.Of` builders, `Requirement.Apply`, the `Op`/`Fault` rail), RhinoCommon (`RhinoDoc` at the ONE `From` adapter, `UnitSystem`).
- Growth: a new execution modality is one `Body` case with one constructor on the SAME owner (a streaming body, a batched-chunk body) — never a second operation class; a new scope source is one `In`/`From` overload minting a `Context`; a new runtime capability is one field on `Env` threaded by the reader with zero operation edits — the telemetry sink target uses one defaulted trailing field and one `Taps` projection.
- Boundary: `Analyze.From(RhinoDoc)` is the ONE document-coupled adapter in the folder — every other scope builder is unit- or context-driven, and a second `RhinoDoc` reach anywhere in the analysis surface is the seam violation; acceptance delegates to `Op.AcceptValue` — the `Domain/validation` oracle extended by `IValidityEvidence` registration — and a folder-local `ValidityOf` switch re-declaring receipt arms is the killed parallel oracle; `Operation.Build` and `Operation.Service` evaluators receive state BY VALUE (`static` lambdas over an explicit state record, no closure capture) so operations stay allocation-lean and referentially transparent; the `As` object-lift is the sanctioned type-erasure bridge for object-typed host pipelines — it rejects onto `Fault.Unsupported`, never casts unsafely; `ValidationLifts.ToEff` is the one `Validation → Eff` rail bridge; exceptions never cross — host machinery that throws is wrapped at the owning boundary through `Op.Catch`.

```csharp conceptual
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Rasm.Csp;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record Env(Context Context, IProgress<double>? Progress, CancellationToken Cancellation, TelemetrySink? Telemetry = null) {
    public static readonly Eff<Env, Env> EnvAsks = Eff.runtime<Env>().As();
    public static readonly Eff<Env, Context> Asks = Eff.runtime<Env>().Map(static env => env.Context).As();
    public static readonly Eff<Env, Option<TelemetrySink>> Taps = Eff.runtime<Env>().Map(static env => Optional(env.Telemetry)).As();
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
internal readonly record struct AnalysisOutput<TOut>(Op Key) {
    public Fin<Seq<TOut>> One<TValue>(TValue value) => Many(values: Seq(value));
    public Fin<Seq<TOut>> Many<TValue>(Seq<TValue> values) => Project(key: Key, values: values);
    public Fin<Seq<TOut>> Objects(Seq<object> values, Type sourceType) {
        Op key = Key;
        return typeof(TOut) == sourceType
            ? values.TraverseM(value => key.AcceptValue(value: (TOut)value)).As()
            : Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: sourceType, outputType: typeof(TOut)));
    }
    private static Fin<Seq<TOut>> Project<TValue>(Op key, Seq<TValue> values) =>
        typeof(TOut) == typeof(TValue)
            ? values.TraverseM(value => key.AcceptValue(value: value)).As().Map(static admitted => admitted.Map(static value => (TOut)(object)value!))
            : Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut)));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed partial record Operation<TGeometry, TOut> where TGeometry : notnull {
    [Union]
    private abstract partial record Body {
        private Body() { }
        internal sealed record Rejected(Error Fault) : Body;
        internal sealed record PerItem(Func<TGeometry, Eff<Env, Seq<TOut>>> Evaluate) : Body;
        internal sealed record Aggregate(Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> Evaluate) : Body;
        internal sealed record Service(Func<Eff<Env, Seq<TOut>>> Evaluate) : Body;
    }
    private Operation(Op key, Requirement requirement, bool requiresContext, Body body) {
        Key = key;
        Requirement = requirement;
        RequiresContext = requiresContext;
        Execution = body;
    }
    public Op Key { get; }
    internal Requirement Requirement { get; init; }
    internal bool RequiresContext { get; init; }
    private Body Execution { get; init; }
    internal bool IsSupported => Execution is not Body.Rejected;
    internal bool IsAggregate => Execution is Body.Aggregate;
    internal bool NeedsContext => RequiresContext || !Requirement.IsEmpty;
    private static Error Charge(Env runtime, Op key, CostMark mark, int items, Error error) =>
        (Charge(runtime: runtime, key: key, mark: mark, items: items, succeeded: false), runtime.Telemetry is { } sink ? ignore(sink.Tap(fact: SignalFact.Fault(domain: KernelDomain.Analysis, key: key, fault: error))) : unit, error).Item3;
    private static Unit Charge(Env runtime, Op key, CostMark mark, int items, bool succeeded) =>
        runtime.Telemetry is { } sink
            ? ignore(sink.Tap(fact: SignalFact.Cost(cost: mark.Stop(key: key, domain: KernelDomain.Analysis, items: items, succeeded: succeeded))))
            : unit;
    internal static Operation<TGeometry, TOut> Build<TState>(Op key, TState state, Func<TState, TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false) {
        Requirement active = requirement ?? Requirement.None;
        return new Operation<TGeometry, TOut>(key: key, requirement: active, requiresContext: requiresContext,
            body: new Body.PerItem(Evaluate: geometry =>
                from prepared in Prepare(geometry: geometry, requirement: active)
                from value in evaluator(arg1: state, arg2: prepared)
                select value));
    }
    internal static Operation<TGeometry, TOut> Aggregate(Op key, Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> project, Requirement? requirement = null, bool requiresContext = false) {
        Requirement active = requirement ?? Requirement.None;
        return new Operation<TGeometry, TOut>(key: key, requirement: active, requiresContext: requiresContext,
            body: new Body.Aggregate(Evaluate: geometry =>
                from resolved in geometry.TraverseM(item => Prepare(geometry: item, requirement: active)).As()
                from result in project(arg: resolved)
                select result));
    }
    internal static Operation<TGeometry, TOut> Reject(Op key, Error fault) =>
        new(key: key, requirement: Requirement.None, requiresContext: false, body: new Body.Rejected(Fault: fault));
    internal static Operation<TGeometry, TOut> Service<TState>(Op key, TState state, Func<TState, Eff<Env, Seq<TOut>>> evaluate, bool requiresContext = false) =>
        new(key: key, requirement: Requirement.None, requiresContext: requiresContext, body: new Body.Service(Evaluate: () => evaluate(arg: state)));
    // Cost capsule: the mark opens before the body fold (Prepare runs inside the window), both exits
    // charge through the Env tap, and the fail exit also publishes the fault fact — sink absent, zero cost.
    public Eff<Env, Seq<TOut>> Apply(Seq<TGeometry> geometry) =>
        from runtime in Env.EnvAsks
        from mark in Fin.Succ(CostMark.Start()).ToEff()
        from result in Folded(geometry: geometry)
            .MapFail(error => Charge(runtime: runtime, key: Key, mark: mark, items: geometry.Count, error: error))
        from _ in Fin.Succ(Charge(runtime: runtime, key: Key, mark: mark, items: geometry.Count, succeeded: true)).ToEff()
        select result;
    private Eff<Env, Seq<TOut>> Folded(Seq<TGeometry> geometry) =>
        Execution.Switch(
            state: geometry,
            rejected: static (_, r) => Fin.Fail<Seq<TOut>>(r.Fault).ToEff(),
            perItem: static (items, i) => items.TraverseM(i.Evaluate).As().Map(static chunks => chunks.Bind(static chunk => chunk)),
            aggregate: static (items, a) => a.Evaluate(arg: items),
            service: static (_, s) => s.Evaluate());
    internal Fin<Operation<TGeometry, TOut>> Supported() =>
        Execution switch {
            Body.Rejected rejected => Fin.Fail<Operation<TGeometry, TOut>>(rejected.Fault),
            _ => Fin.Succ(this),
        };
    private static Eff<Env, TGeometry> Prepare(TGeometry geometry, Requirement requirement) =>
        from runtime in Env.EnvAsks
        from ready in (runtime.Cancellation.IsCancellationRequested switch {
            true => Fin.Fail<TGeometry>(new Fault.Cancelled()),
            false => Optional(geometry).ToFin(new Fault.MissingGeometry()),
        }).ToEff()
        from validated in (requirement.IsEmpty, ready) switch {
            (true, GeometryBase native) => from _ in requirement.Apply(context: runtime.Context, value: native, cancel: runtime.Cancellation).ToEff()
                                           select ready,
            (true, _) => Fin.Succ(ready).ToEff(),
            _ => from _ in requirement.Apply(context: runtime.Context, value: ready, cancel: runtime.Cancellation).ToEff()
                 select ready,
        }
        select validated;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Analyze {
    public sealed record Scope {
        public Fin<Context> Context { get; }
        public IProgress<double>? Progress { get; init; }
        public CancellationToken Cancellation { get; init; }
        public TelemetrySink? Telemetry { get; init; }
        internal Scope(Fin<Context> context) => Context = context;
        public Scope With(IProgress<double> progress) => this with { Progress = progress };
        public Scope With(CancellationToken cancellation) => this with { Cancellation = cancellation };
        public Scope With(TelemetrySink telemetry) => this with { Telemetry = telemetry };
        public Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(Operation<TGeometry, TOut>? operation, params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
            Analyze.Run(operation: operation, scope: Some(this), input: input);
    }
    public static Scope From(RhinoDoc? doc) => new(context: Context.Of(doc: doc).ToFin());
    public static Scope In(UnitSystem units) => new(context: Context.Of(units: units).ToFin());
    public static Scope In(double absolute, double relative, double angle, UnitSystem units) =>
        new(context: Context.Of(absolute: absolute, relative: relative, angle: angle, units: units).ToFin());
    public static Scope In(Context context) => new(context: Optional(context).ToFin(Op.Of(name: nameof(Scope)).MissingContext()));

    public static Operation<TGeometry, TOut> Query<TGeometry, TOut>(AnalysisQuery? query, Op? key = null) where TGeometry : notnull where TOut : notnull {
        Op active = key.OrDefault();
        return Optional(query).Map(q => q.Single<TGeometry, TOut>(key: active)).IfNone(Operation<TGeometry, TOut>.Reject(key: active, fault: active.InvalidInput()));
    }
    public static Operation<(TA A, TB B), TOut> Query<TA, TB, TOut>(AnalysisQuery? query, Op? key = null) where TA : notnull where TB : notnull where TOut : notnull {
        Op active = key.OrDefault();
        return Optional(query).Map(q => q.Pair<TA, TB, TOut>(key: active)).IfNone(Operation<(TA A, TB B), TOut>.Reject(key: active, fault: active.InvalidInput()));
    }
    public static Operation<Unit, TOut> Query<TOut>(AnalysisQuery? query, Op? key = null) where TOut : notnull {
        Op active = key.OrDefault();
        return Optional(query).Map(q => q.Service<TOut>(key: active)).IfNone(Operation<Unit, TOut>.Reject(key: active, fault: active.InvalidInput()));
    }

    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(AnalysisQuery query, params ReadOnlySpan<TGeometry> input) where TGeometry : notnull where TOut : notnull =>
        Run(operation: Query<TGeometry, TOut>(query: query), input: input);
    public static Validation<Error, Seq<TOut>> Run<TA, TB, TOut>(AnalysisQuery query, params ReadOnlySpan<(TA A, TB B)> input) where TA : notnull where TB : notnull where TOut : notnull =>
        Run(operation: Query<TA, TB, TOut>(query: query), input: input);
    public static Validation<Error, Seq<TOut>> Run<TOut>(AnalysisQuery query) where TOut : notnull =>
        Run(operation: Query<TOut>(query: query), input: Unit.Default);
    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(Operation<TGeometry, TOut>? operation, params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Run(operation: operation, scope: Option<Scope>.None, input: input);

    internal static Operation<TGeometry, TOut> Unsupported<TGeometry, TOut>(this Op key) where TGeometry : notnull =>
        Operation<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut)));
    internal static Operation<TGeometry, TOut> As<TGeometry, TOut>(this object operation, Op key) where TGeometry : notnull => operation switch {
        Operation<TGeometry, TOut> typed => typed,
        _ => Operation<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
    };
    internal static Operation<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue, TState>(Op key, TState state, Func<TState, TNative, Eff<Env, Seq<TValue>>> project, Requirement? requirement = null, bool requiresContext = false) where TGeometry : notnull where TNative : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (Key: key, State: state, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg1: state.State, arg2: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TValue))).ToEff(),
            }).As<TGeometry, TOut>(key: key);

    private static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(Operation<TGeometry, TOut>? operation, Option<Scope> scope, ReadOnlySpan<TGeometry> input) where TGeometry : notnull {
        TGeometry[] inputValues = input.ToArray();
        (IProgress<double>? progress, CancellationToken cancellation, TelemetrySink? telemetry) = scope.Case switch {
            Scope active => (active.Progress, active.Cancellation, active.Telemetry),
            _ => (null, CancellationToken.None, null),
        };
        return (
            from active in Optional(operation).ToFin(new Fault.MissingOperation())
            from accepted in active.Supported()
            from context in scope.Case switch {
                Scope provided => provided.Context,
                _ => accepted.NeedsContext switch {
                    true => Fin.Fail<Context>(accepted.Key.MissingContext()),
                    false => Context.Of(units: UnitSystem.Millimeters).ToFin(),
                },
            }
            from result in accepted.Apply(geometry: inputValues.AsIterable().ToSeq()).Run(env: new Env(Context: context, Progress: progress, Cancellation: cancellation, Telemetry: telemetry))
            select result).ToValidation();
    }
}

internal static class ValidationLifts {
    internal static Eff<Env, T> ToEff<T>(this Validation<Error, T> validation) => validation.ToFin();
}
```

```mermaid
flowchart LR
    AnalysisQuery -->|Single / Pair / Service arity| Operation
    AnalysisQuery -->|family band forwards| FamilyUnions[Bounds · Measure · Location · Curves · Faces · Topologies · Meshes · Points]
    FamilyUnions -->|internal Operation builder| Operation
    Operation -->|Prepare: cancel → admit → Requirement| Requirement
    Operation -->|Apply| Eff[Eff Env Seq TOut]
    Operation -->|CostMark → OpCost on both exits| Sink[TelemetrySink tap]
    Env -->|Context · Progress · Cancellation · Telemetry| Eff
    Eff -->|AnalysisOutput → Op.AcceptValue| Oracle[one validity oracle]
    Analyze -->|Scope / From RhinoDoc / In / Run| Operation
    Eff -.->|Fault union · Unsupported 9104| Validation[Validation Error Seq TOut]
```

## [04]-[DENSITY_BAR]

One owner per axis; capability is a case, a factory row, or a body arm, never a sibling surface. Each owner returns on the `[RAIL]` its row names; the facade closes 3 `Query` and 4 `Run` arities and the output gate projects `One`/`Many`/`Objects`, with the `[02]`/`[03]` sections spelling each band, body case, and member in full.

| [INDEX] | [CONCERN]           | [OWNER]                      | [KIND]                                | [RAIL]                         | [CASES] |
| :-----: | :------------------ | :--------------------------- | :------------------------------------ | :----------------------------- | :-----: |
|  [01]   | Request vocabulary  | `AnalysisQuery`              | `[Union]` — 4 bands, arity-dispatched | dispatch; build-time `Reject`  |   25    |
|  [02]   | Operation algebra   | `Operation<TGeometry, TOut>` | research `Body` + `Prepare` gate       | catalog-blocked `Eff<Env, _>`  |    4    |
|  [03]   | Runtime environment | `Env`                        | research `EnvAsks`/`Asks`/`Taps`      | catalog-blocked `Eff<Env, _>`  |    —    |
|  [04]   | Execution facade    | `Analyze`                    | `static partial class`                | `Validation<Error, Seq<TOut>>` |    —    |
|  [05]   | Output projection   | `AnalysisOutput<TOut>`       | `readonly record struct`              | `Fin<Seq<TOut>>`, one oracle   |    3    |

Both research fences preserve transcription-complete target bodies, but neither is settled while `Eff.runtime<Env>()` lacks catalog truth. Family and relation builders remain owned by `Analysis/measure`, `Analysis/inspect`, `Analysis/select`, `Analysis/relations`, and `Parametric/locate`.

## [05]-[RESEARCH]

- [EFF_RUNTIME_CATALOG]-[BLOCKED]: `public static Eff<RT, RT> runtime<RT>()` is decompile-confirmed on `LanguageExt.Eff` through the `tools.assay api query` exact-match rail, so the reader constructor behind `EnvAsks`, `Asks`, and `Taps` is real and catalog absence is the one remaining block; the row arms when `libs/csharp/.api/api-languageext.md` lands the verified declaration — then restore both C# fences to `csharp signature` and re-arm `TASKLOG.md` `[ENV_TELEMETRY_FIELD]` and `[OP_COST_CAPSULE]`.
- [REQUEST_UNIFICATION]-[OPEN]: Does the unified request algebra preserve every absorbed factory and arity verdict; assert that each factory produces the same `Key`, `Requirement`, output gate, and operation as its band case, with wrong-arity `Single`/`Pair` calls routing `Fault.Unsupported`, then exercise the host-independent dispatch through the bridge scenario rail. `Coerce` subsumes surface and brep forms, `Kind` and `Bounds` route to their family cases, and the remaining seven absorbed spellings are first-class cases.
- [OPERATION_MONAD]-[OPEN]: Does `Operation` admit every input before evaluation and preserve per-item distribution; drive null items, pre-cancelled tokens, and invalid geometry through every body kind with zero evaluator calls, then assert `Apply(a ++ b) == Apply(a) ++ Apply(b)` for a pure `PerItem` evaluator. `Prepare` gates every item, an empty requirement still admits `GeometryBase`, and `Aggregate` names the deliberate distribution break through `IsAggregate`.
- [ONE_ORACLE]-[OPEN]: Does every Analysis receipt agree with the domain validity oracle; assert `Op.AcceptValue` succeeds exactly when each receipt's `IValidityEvidence.IsValid` holds and rejects through `Fault.InvalidResult` with the operation key. `AnalysisOutput` routes every value through this authority, so a new receipt adds one interface implementation and zero oracle edits.
- [HOST_CONTRACT_FREEZE]-[OPEN]: Do the frozen runtime spellings still serve every dormant host consumer; exercise `Analyze.Query<object, TOut>(query)` with `Operation.Apply(…).Run(env: new Env(…))`, `Analyze.In(context: …)` to `Analyze.Scope`, and `Analyze.Run<object, BoundingBox>(query: AnalysisQuery.Bounds(Bounds.AxisAligned), …)` through a live-document bridge scenario. Any rename breaks the Grasshopper binding, Rhino command context, or overlay probe.
