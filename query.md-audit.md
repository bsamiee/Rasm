# 1. Delete the duplicate verb admission gate

Location: request-algebra fence, `VerbGate` declaration (lines 68-74).

From

```csharp
[Union]
public abstract partial record VerbGate {
    private VerbGate() { }
    public sealed record OwnedCase(Func<Type, Type, bool> Admits) : VerbGate;
    public sealed record DelegatedCase : VerbGate;
}
```

To

```csharp
// VerbGate DELETED
```

Location: request-algebra fence, `AnalysisVerb.Delegated` (line 78).

From

```csharp
private static readonly VerbGate Delegated = new VerbGate.DelegatedCase();
```

To

```csharp
// Delegated DELETED
```

Location: request-algebra fence, geometry-band verb rows (lines 81-87).

From

```csharp
public static readonly AnalysisVerb Coerce = new(key: "coerce", band: AnalysisBand.Geometry, gate: new VerbGate.OwnedCase(Admits: static (g, o) => Capability.Coercible(source: g, target: o)));
public static readonly AnalysisVerb CurveForm = new(key: "curve-form", band: AnalysisBand.Geometry, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(Rasm.Domain.CurveForm)));
public static readonly AnalysisVerb Vertices = new(key: "vertices", band: AnalysisBand.Geometry, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(Point3d)));
public static readonly AnalysisVerb Samples = new(key: "sample-points", band: AnalysisBand.Geometry, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(Point3d)));
public static readonly AnalysisVerb SurfaceUv = new(key: "surface-uv", band: AnalysisBand.Geometry, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(Point2d)));
public static readonly AnalysisVerb Closest = new(key: "closest", band: AnalysisBand.Geometry, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(ClosestHit)));
public static readonly AnalysisVerb Signed = new(key: "signed-distance", band: AnalysisBand.Geometry, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(double)));
```

To

```csharp
public static readonly AnalysisVerb Coerce = new(key: "coerce", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb CurveForm = new(key: "curve-form", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb Vertices = new(key: "vertices", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb Samples = new(key: "sample-points", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb SurfaceUv = new(key: "surface-uv", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb Closest = new(key: "closest", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb Signed = new(key: "signed-distance", band: AnalysisBand.Geometry);
```

Location: request-algebra fence, family-band verb rows (lines 90-97).

From

```csharp
public static readonly AnalysisVerb Bounds = new(key: "bounds", band: AnalysisBand.Family, gate: Delegated);
public static readonly AnalysisVerb Measure = new(key: "measure", band: AnalysisBand.Family, gate: Delegated);
public static readonly AnalysisVerb Location = new(key: "location", band: AnalysisBand.Family, gate: Delegated);
public static readonly AnalysisVerb Curves = new(key: "curves", band: AnalysisBand.Family, gate: Delegated);
public static readonly AnalysisVerb Faces = new(key: "faces", band: AnalysisBand.Family, gate: Delegated);
public static readonly AnalysisVerb Topologies = new(key: "topologies", band: AnalysisBand.Family, gate: Delegated);
public static readonly AnalysisVerb Meshes = new(key: "meshes", band: AnalysisBand.Family, gate: Delegated);
public static readonly AnalysisVerb Points = new(key: "points", band: AnalysisBand.Family, gate: Delegated);
```

To

```csharp
public static readonly AnalysisVerb Bounds = new(key: "bounds", band: AnalysisBand.Family);
public static readonly AnalysisVerb Measure = new(key: "measure", band: AnalysisBand.Family);
public static readonly AnalysisVerb Location = new(key: "location", band: AnalysisBand.Family);
public static readonly AnalysisVerb Curves = new(key: "curves", band: AnalysisBand.Family);
public static readonly AnalysisVerb Faces = new(key: "faces", band: AnalysisBand.Family);
public static readonly AnalysisVerb Topologies = new(key: "topologies", band: AnalysisBand.Family);
public static readonly AnalysisVerb Meshes = new(key: "meshes", band: AnalysisBand.Family);
public static readonly AnalysisVerb Points = new(key: "points", band: AnalysisBand.Family);
```

Location: request-algebra fence, relation-band verb rows (lines 100-105).

From

```csharp
public static readonly AnalysisVerb Intersections = new(key: "intersections", band: AnalysisBand.Relation, gate: Delegated);
public static readonly AnalysisVerb Classification = new(key: "classification", band: AnalysisBand.Relation, gate: Delegated);
public static readonly AnalysisVerb Deviation = new(key: "curve-deviation", band: AnalysisBand.Relation, gate: Delegated);
public static readonly AnalysisVerb SelfIntersection = new(key: "self-intersection", band: AnalysisBand.Relation, gate: Delegated);
public static readonly AnalysisVerb Ray = new(key: "ray", band: AnalysisBand.Relation, gate: Delegated);
public static readonly AnalysisVerb Conformance = new(key: "conformance", band: AnalysisBand.Relation, gate: Delegated);
```

To

```csharp
public static readonly AnalysisVerb Intersections = new(key: "intersections", band: AnalysisBand.Relation);
public static readonly AnalysisVerb Classification = new(key: "classification", band: AnalysisBand.Relation);
public static readonly AnalysisVerb Deviation = new(key: "curve-deviation", band: AnalysisBand.Relation);
public static readonly AnalysisVerb SelfIntersection = new(key: "self-intersection", band: AnalysisBand.Relation);
public static readonly AnalysisVerb Ray = new(key: "ray", band: AnalysisBand.Relation);
public static readonly AnalysisVerb Conformance = new(key: "conformance", band: AnalysisBand.Relation);
```

Location: request-algebra fence, spatial-band verb rows (lines 108-111).

From

```csharp
public static readonly AnalysisVerb SearchBox = new(key: "search-box", band: AnalysisBand.Spatial, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(NeighborHit)));
public static readonly AnalysisVerb SearchSphere = new(key: "search-sphere", band: AnalysisBand.Spatial, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(NeighborHit)));
public static readonly AnalysisVerb Overlap = new(key: "overlap", band: AnalysisBand.Spatial, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(NeighborPair)));
public static readonly AnalysisVerb PointPairs = new(key: "point-pairs", band: AnalysisBand.Spatial, gate: new VerbGate.OwnedCase(Admits: static (_, o) => o == typeof(NeighborPair)));
```

To

```csharp
public static readonly AnalysisVerb SearchBox = new(key: "search-box", band: AnalysisBand.Spatial);
public static readonly AnalysisVerb SearchSphere = new(key: "search-sphere", band: AnalysisBand.Spatial);
public static readonly AnalysisVerb Overlap = new(key: "overlap", band: AnalysisBand.Spatial);
public static readonly AnalysisVerb PointPairs = new(key: "point-pairs", band: AnalysisBand.Spatial);
```

Location: request-algebra fence, `AnalysisVerb.Gate` and `AnalysisVerb.Admits` (lines 114-118).

From

```csharp
public VerbGate Gate { get; }
public bool Admits(Type geometry, Type output) => Gate.Switch(
    state: (Geometry: geometry, Output: output),
    ownedCase: static (types, owned) => owned.Admits(arg1: types.Geometry, arg2: types.Output),
    delegatedCase: static (_, _) => true);
```

To

```csharp
// Gate DELETED
// Admits DELETED
```

Location: request-algebra fence, arity dispatchers (lines 304-315).

From

```csharp
this is ISingleQuery single && Verb.Admits(geometry: typeof(TGeometry), output: typeof(TOut))
this is IPairQuery pair && Verb.Admits(geometry: typeof(TA), output: typeof(TOut))
this is IServiceQuery served && Verb.Admits(geometry: typeof(Unit), output: typeof(TOut))
```

To

```csharp
this is ISingleQuery single
this is IPairQuery pair
this is IServiceQuery served
```

Why: Every owned predicate is repeated by the operation it precedes: coercion has `Capability.Coercible`/`CoerceTo`, fixed-output builders close through `As`, spatial answers close through typed output projection, and family builders own their admission. The delegated arm always returns true, so the union adds no verdict.

Change: Let the arity floor reach the owning builder directly and remove the duplicate gate column, gate union, alias, and pre-dispatch checks.

Delta: -12 LOC; -3 module-level/nested types; -3 explicit members plus the generated union surface; neutral verb rows and capability.

Ripples: Update the request-algebra index/card/laws, operation diagram, and density row in `query.md`; no direct consumer outside the target names `VerbGate`, `Gate`, or `Admits`.

# 2. Remove the coercion output knob and inline its builder

Location: request-algebra fence, `CoerceCase` (lines 152-155 after task 1).

From

```csharp
public sealed record CoerceCase(Type Output) : AnalysisQuery, ISingleQuery {
    public override AnalysisVerb Verb => AnalysisVerb.Coerce;
    Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) => Cast<TGeometry, TOut>(output: Output, key: key);
}
```

To

```csharp
public sealed record CoerceCase : AnalysisQuery, ISingleQuery {
    public override AnalysisVerb Verb => AnalysisVerb.Coerce;
    Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) =>
        Capability.Coercible(source: typeof(TGeometry), target: typeof(TOut))
            ? Operation<TGeometry, TOut>.Build(key: key, requirement: Some(Requirement.Basic), requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from value in geometry.CoerceTo<TOut>(context: context, key: op).ToEff()
                    from admitted in new AnalysisOutput<TOut>(Key: op).One(value: value).ToEff()
                    select admitted)
            : key.Unsupported<TGeometry, TOut>();
}
```

Location: request-algebra fence, coercion factory (line 271).

From

```csharp
public static AnalysisQuery Coerce(Type output) => new CoerceCase(Output: output);
```

To

```csharp
// AnalysisQuery.Coerce DELETED
```

Location: request-algebra fence, geometry builder `Cast` (lines 318-326).

From

```csharp
private static Operation<TGeometry, TOut> Cast<TGeometry, TOut>(Type output, Op key) where TGeometry : notnull where TOut : notnull =>
    output == typeof(TOut)
        ? Operation<TGeometry, TOut>.Build(key: key, requirement: Some(Requirement.Basic), requiresContext: true, state: key,
            evaluator: static (op, geometry) =>
                from context in Env.Asks
                from value in geometry.CoerceTo<TOut>(context: context, key: op).ToEff()
                from admitted in new AnalysisOutput<TOut>(Key: op).One(value: value).ToEff()
                select admitted)
        : key.Unsupported<TGeometry, TOut>();
```

To

```csharp
// Cast DELETED
```

Why: `TOut` already identifies the requested output. Storing a second runtime `Type` permits contradictory requests and makes the helper compare two spellings of one decision.

Change: Make coercion stateless, derive the target from `TOut`, keep capability admission at the case-owned build, and delete both the one-call helper and policy-free case factory.

Delta: -2 LOC; -3 members (`CoerceCase.Output`, `Cast`, and `AnalysisQuery.Coerce`); neutral type count and coercion capability.

Ripples: Update the request-algebra card from a payload-bearing coercion case to a stateless case; no direct consumer outside the target calls `AnalysisQuery.Coerce`.

# 3. Inline curve-form construction into its query case

Location: request-algebra fence, `CurveFormCase.Build` (line 158).

From

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) => Form<TGeometry, TOut>(key: key);
```

To

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) =>
    Operation<TGeometry, Rasm.Domain.CurveForm>.Build(key: key, requirement: Some(Requirement.Basic), requiresContext: true, state: key,
        evaluator: static (op, geometry) =>
            from context in Env.Asks
            from form in Normalization.CurveForm(source: geometry, key: op).Map(lease => lease.Use(curve => Normalization.CurveFormOf(curve: curve, context: context))).ToEff()
            from admitted in new AnalysisOutput<Rasm.Domain.CurveForm>(Key: op).One(value: form).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

Location: request-algebra fence, geometry builder `Form` (lines 327-334).

From

```csharp
private static Operation<TGeometry, TOut> Form<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
    Operation<TGeometry, Rasm.Domain.CurveForm>.Build(key: key, requirement: Some(Requirement.Basic), requiresContext: true, state: key,
        evaluator: static (op, geometry) =>
            from context in Env.Asks
            from form in Normalization.CurveForm(source: geometry, key: op).Map(lease => lease.Use(curve => Normalization.CurveFormOf(curve: curve, context: context))).ToEff()
            from admitted in new AnalysisOutput<Rasm.Domain.CurveForm>(Key: op).One(value: form).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

To

```csharp
// Form DELETED
```

Why: `Form` has one caller and owns no policy outside `CurveFormCase`.

Change: Put normalization and output projection directly on the case-owned build.

Delta: -1 LOC; -1 member; neutral type count and capability.

# 4. Inline vertex construction into its query case

Location: request-algebra fence, `VerticesCase.Build` (line 162).

From

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) => Nodes<TGeometry, TOut>(key: key);
```

To

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) =>
    Operation<TGeometry, Point3d>.Build(key: key, state: key,
        evaluator: static (op, geometry) =>
            from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Vertices(), key: op).ToEff()
            from admitted in new AnalysisOutput<Point3d>(Key: op).Many(values: points).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

Location: request-algebra fence, geometry builder `Nodes` (lines 335-341).

From

```csharp
private static Operation<TGeometry, TOut> Nodes<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
    Operation<TGeometry, Point3d>.Build(key: key, state: key,
        evaluator: static (op, geometry) =>
            from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Vertices(), key: op).ToEff()
            from admitted in new AnalysisOutput<Point3d>(Key: op).Many(values: points).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

To

```csharp
// Nodes DELETED
```

Why: `Nodes` is a one-call alias whose name also diverges from the public `Vertices` term.

Change: Build the vertex projection directly on `VerticesCase`.

Delta: -1 LOC; -1 member; neutral type count and capability.

# 5. Admit the sample count once and inline its builder

Location: request-algebra fence, imports around `Rasm.Domain` (lines 34-35).

From

```csharp
using Rasm.Domain;
using Rasm.Parametric;
```

To

```csharp
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
```

Location: request-algebra fence, `SamplePointsCase` (lines 164-167).

From

```csharp
public sealed record SamplePointsCase(int Count) : AnalysisQuery, ISingleQuery {
    public override AnalysisVerb Verb => AnalysisVerb.Samples;
    Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) => Sampled<TGeometry, TOut>(count: Count, key: key);
}
```

To

```csharp
public sealed record SamplePointsCase(Dimension Count) : AnalysisQuery, ISingleQuery {
    public override AnalysisVerb Verb => AnalysisVerb.Samples;
    Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) =>
        Operation<TGeometry, Point3d>.Build(key: key, requiresContext: true, state: (Key: key, Count),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: state.Count, Model: context), key: state.Key).ToEff()
                from admitted in new AnalysisOutput<Point3d>(Key: state.Key).Many(values: points).ToEff()
                select admitted)
            .As<TGeometry, TOut>(key: key);
}
```

Location: request-algebra fence, sample-points factory (line 274).

From

```csharp
public static AnalysisQuery SamplePoints(int count) => new SamplePointsCase(Count: count);
```

To

```csharp
// AnalysisQuery.SamplePoints DELETED
```

Location: request-algebra fence, geometry builder `Sampled` (lines 342-349).

From

```csharp
private static Operation<TGeometry, TOut> Sampled<TGeometry, TOut>(int count, Op key) where TGeometry : notnull where TOut : notnull =>
    Operation<TGeometry, Point3d>.Build(key: key, requiresContext: true, state: (Key: key, Count: count),
        evaluator: static (state, geometry) =>
            from context in Env.Asks
            from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: state.Count, Model: context), key: state.Key).ToEff()
            from admitted in new AnalysisOutput<Point3d>(Key: state.Key).Many(values: points).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

To

```csharp
// Sampled DELETED
```

Why: A sample count is the existing positive `Dimension` domain value. Keeping a raw `int` in the request permits an invalid state that the evaluation owner must rediscover, while `Sampled` only transports that state once.

Change: Require an admitted `Dimension`, build directly on `SamplePointsCase`, and delete both the forwarding helper and policy-free case factory.

Delta: -1 LOC; -2 members; neutral type count; replaces one raw scalar with an existing generated value owner.

Ripples: Change `EvaluationRequest.Sample.Count` to `Dimension` in `libs/dotnet/Rasm/.planning/Domain/evaluation.md`, delete its `count > 0` revalidation, and pass admitted dimensions from the five request constructors in `Analysis/query`, `Analysis/measure`, and `Processing/sample`; no direct consumer outside the target calls `AnalysisQuery.SamplePoints`.

# 6. Inline surface-UV construction into its query case

Location: request-algebra fence, `SurfaceUvCase.Build` (line 170).

From

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) => Uv<TGeometry, TOut>(uv: Uv, key: key);
```

To

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) =>
    Operation<TGeometry, Point2d>.Build(key: key, requirement: Some(Requirement.SurfaceEvaluation), requiresContext: true, state: (Key: key, Uv),
        evaluator: static (state, geometry) =>
            from context in Env.Asks
            from result in Normalization.SurfaceForm(source: geometry, key: state.Key).Bind(lease => lease.Use(surface => Evaluation.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key))).ToEff()
            from admitted in new AnalysisOutput<Point2d>(Key: state.Key).One(value: result).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

Location: request-algebra fence, geometry builder `Uv` (lines 350-357).

From

```csharp
private static Operation<TGeometry, TOut> Uv<TGeometry, TOut>(Point2d uv, Op key) where TGeometry : notnull where TOut : notnull =>
    Operation<TGeometry, Point2d>.Build(key: key, requirement: Some(Requirement.SurfaceEvaluation), requiresContext: true, state: (Key: key, Uv: uv),
        evaluator: static (state, geometry) =>
            from context in Env.Asks
            from result in Normalization.SurfaceForm(source: geometry, key: state.Key).Bind(lease => lease.Use(surface => Evaluation.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key))).ToEff()
            from admitted in new AnalysisOutput<Point2d>(Key: state.Key).One(value: result).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

To

```csharp
// Uv DELETED
```

Why: `Uv` is a single-use transport for the case-owned coordinate.

Change: Build the surface-evaluation operation directly from `SurfaceUvCase.Uv`.

Delta: -1 LOC; -1 member; neutral type count and capability.

# 7. Inline closest-point construction into its query case

Location: request-algebra fence, `ClosestCase.Build` (line 174).

From

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) => Nearest<TGeometry, TOut>(target: Target, key: key);
```

To

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) =>
    Operation<TGeometry, ClosestHit>.Build(key: key, state: (Key: key, Target),
        evaluator: static (state, geometry) =>
            from hit in geometry.Evaluate<ClosestHit>(request: new EvaluationRequest.Closest(Target: state.Target), key: state.Key).ToEff()
            from admitted in new AnalysisOutput<ClosestHit>(Key: state.Key).One(value: hit).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

Location: request-algebra fence, geometry builder `Nearest` (lines 358-364).

From

```csharp
private static Operation<TGeometry, TOut> Nearest<TGeometry, TOut>(Point3d target, Op key) where TGeometry : notnull where TOut : notnull =>
    Operation<TGeometry, ClosestHit>.Build(key: key, state: (Key: key, Target: target),
        evaluator: static (state, geometry) =>
            from hit in geometry.Evaluate<ClosestHit>(request: new EvaluationRequest.Closest(Target: state.Target), key: state.Key).ToEff()
            from admitted in new AnalysisOutput<ClosestHit>(Key: state.Key).One(value: hit).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

To

```csharp
// Nearest DELETED
```

Why: `Nearest` is a one-call alias for the already canonical `ClosestCase` operation.

Change: Thread `ClosestCase.Target` directly into the evaluation request.

Delta: -1 LOC; -1 member; neutral type count and capability.

# 8. Inline signed-distance construction into its query case

Location: request-algebra fence, `SignedDistanceCase.Build` (line 178).

From

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) => Signed<TGeometry, TOut>(sample: Sample, key: key);
```

To

```csharp
Operation<TGeometry, TOut> ISingleQuery.Build<TGeometry, TOut>(Op key) =>
    Operation<TGeometry, double>.Build(key: key, state: (Key: key, Sample),
        evaluator: static (state, geometry) =>
            from distance in geometry.Evaluate<double>(request: new EvaluationRequest.Signed(Sample: state.Sample), key: state.Key).ToEff()
            from admitted in new AnalysisOutput<double>(Key: state.Key).One(value: distance).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

Location: request-algebra fence, geometry builder `Signed` (lines 365-371).

From

```csharp
private static Operation<TGeometry, TOut> Signed<TGeometry, TOut>(Point3d sample, Op key) where TGeometry : notnull where TOut : notnull =>
    Operation<TGeometry, double>.Build(key: key, state: (Key: key, Sample: sample),
        evaluator: static (state, geometry) =>
            from distance in geometry.Evaluate<double>(request: new EvaluationRequest.Signed(Sample: state.Sample), key: state.Key).ToEff()
            from admitted in new AnalysisOutput<double>(Key: state.Key).One(value: distance).ToEff()
            select admitted)
        .As<TGeometry, TOut>(key: key);
```

To

```csharp
// Signed DELETED
```

Why: `Signed` is a single-use helper whose shortened name diverges from its request case and factory.

Change: Build the signed-distance operation directly on `SignedDistanceCase`.

Delta: -1 LOC; -1 member; neutral type count and capability.

# 9. Align request symbols with the domain vocabulary

Location: request-algebra fence, three imprecise `AnalysisVerb` members (lines 84, 87, and 102 after task 1).

From

```csharp
public static readonly AnalysisVerb Samples = new(key: "sample-points", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb Signed = new(key: "signed-distance", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb Deviation = new(key: "curve-deviation", band: AnalysisBand.Relation);
```

To

```csharp
public static readonly AnalysisVerb SamplePoints = new(key: "sample-points", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb SignedDistance = new(key: "signed-distance", band: AnalysisBand.Geometry);
public static readonly AnalysisVerb CurveDeviation = new(key: "curve-deviation", band: AnalysisBand.Relation);
```

Location: request-algebra fence, corresponding query-case projections (lines 165, 177, and 225).

From

```csharp
public override AnalysisVerb Verb => AnalysisVerb.Samples;
public override AnalysisVerb Verb => AnalysisVerb.Signed;
public override AnalysisVerb Verb => AnalysisVerb.Deviation;
```

To

```csharp
public override AnalysisVerb Verb => AnalysisVerb.SamplePoints;
public override AnalysisVerb Verb => AnalysisVerb.SignedDistance;
public override AnalysisVerb Verb => AnalysisVerb.CurveDeviation;
```

Location: request-algebra fence, topology request case (line 202).

From

```csharp
public sealed record TopologyCase(Topologies Query) : AnalysisQuery, ISingleQuery {
```

To

```csharp
public sealed record TopologiesCase(Topologies Query) : AnalysisQuery, ISingleQuery {
```

Why: The existing verb members abbreviate precise operation keys, and singular `TopologyCase` disagrees with the `Topologies` request family it carries.

Change: Use `SamplePoints`, `SignedDistance`, and `CurveDeviation` for the verb rows and plural `TopologiesCase` for the family case. Do not rename forwarding factories that later tasks delete.

Delta: 0 LOC; neutral module-level symbol/member/type count; four symbols renamed.

Ripples: Update the `Selection(Topologies)` factory's constructor reference to `TopologiesCase` until Task 17 deletes that factory. No external planning-corpus consumer names the changed case or verb members.

# 10. Give the spatial helpers operational names

Location: request-algebra fence, four spatial case builds (lines 249, 256, 262, and 267).

From

```csharp
Spine<TOut>(key: key, resolve: _ => Fin.Succ(Index), query: new NeighborQuery.BoxCase(Bounds: Box), anchor: Box.Center)
Spine<TOut>(key: key, resolve: _ => Fin.Succ(Index), query: new NeighborQuery.BallCase(Ball: Sphere), anchor: Sphere.Center)
Spine<TOut>(key: key, resolve: _ => Fin.Succ(Left), query: new NeighborQuery.OverlapsCase(Other: Right, Band: Band), anchor: Point3d.Origin)
Spine<TOut>(key: key, resolve: op => NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: Points), key: op), query: new NeighborQuery.PairsCase(Needles: Needles, Probe: Probe), anchor: Point3d.Origin)
```

To

```csharp
Search<TOut>(key: key, resolve: _ => Fin.Succ(Index), query: new NeighborQuery.BoxCase(Bounds: Box), anchor: Box.Center)
Search<TOut>(key: key, resolve: _ => Fin.Succ(Index), query: new NeighborQuery.BallCase(Ball: Sphere), anchor: Sphere.Center)
Search<TOut>(key: key, resolve: _ => Fin.Succ(Left), query: new NeighborQuery.OverlapsCase(Other: Right, Band: Band), anchor: Point3d.Origin)
Search<TOut>(key: key, resolve: op => NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: Points), key: op), query: new NeighborQuery.PairsCase(Needles: Needles, Probe: Probe), anchor: Point3d.Origin)
```

Location: request-algebra fence, spatial helpers `Spine` and `Answer` (lines 374-385).

From

```csharp
private static Operation<Unit, TOut> Spine<TOut>(Op key, Func<Op, Fin<NeighborIndex>> resolve, NeighborQuery query, Point3d anchor) where TOut : notnull =>
    Operation<Unit, TOut>.Service(key: key, state: (Key: key, Resolve: resolve, Query: query, Anchor: anchor), evaluate: static state =>
        from runtime in Env.EnvAsks
        from index in state.Resolve(state.Key).ToEff()
        from answer in index.Query(query: state.Query, anchor: state.Anchor, key: state.Key, cancel: runtime.Cancellation).ToEff()
        from projected in Answer<TOut>(answer: answer, key: state.Key).ToEff()
        select projected);
private static Fin<Seq<TOut>> Answer<TOut>(NeighborAnswer answer, Op key) => answer.Switch(
```

To

```csharp
private static Operation<Unit, TOut> Search<TOut>(Op key, Func<Op, Fin<NeighborIndex>> resolve, NeighborQuery query, Point3d anchor) where TOut : notnull =>
    Operation<Unit, TOut>.Service(key: key, state: (Key: key, Resolve: resolve, Query: query, Anchor: anchor), evaluate: static state =>
        from runtime in Env.EnvAsks
        from index in state.Resolve(state.Key).ToEff()
        from answer in index.Query(query: state.Query, anchor: state.Anchor, key: state.Key, cancel: runtime.Cancellation).ToEff()
        from projected in Project<TOut>(answer: answer, key: state.Key).ToEff()
        select projected);
private static Fin<Seq<TOut>> Project<TOut>(NeighborAnswer answer, Op key) => answer.Switch(
```

Why: `Spine` names implementation shape rather than the neighbor operation, and noun `Answer` does not state that the helper projects an existing `NeighborAnswer`.

Change: Use the existing public operation term `Search` and the software-domain action `Project`.

Delta: 0 LOC; neutral module-level symbol/member/type count; two members renamed.

# 11. Make output projection stateless

Location: operation-runtime fence, interop import used only by `AnalysisOutput` (line 405).

From

```csharp
using System.Runtime.InteropServices;
```

To

```csharp
// System.Runtime.InteropServices DELETED
```

Location: operation-runtime fence, `AnalysisOutput<TOut>` (lines 426-434).

From

```csharp
[StructLayout(LayoutKind.Auto)]
internal readonly record struct AnalysisOutput<TOut>(Op Key) {
    public Fin<Seq<TOut>> One<TValue>(TValue value) => Many(values: Seq(value));
    public Fin<Seq<TOut>> Many<TValue>(Seq<TValue> values) => Project(key: Key, values: values);
    private static Fin<Seq<TOut>> Project<TValue>(Op key, Seq<TValue> values) =>
        typeof(TOut) == typeof(TValue)
            ? values.TraverseM(value => key.AcceptValue(value: value)).As().Map(static admitted => admitted.Map(static value => (TOut)(object)value!))
            : Fin.Fail<Seq<TOut>>(key.Unsupported(inputType: typeof(TValue), outputType: typeof(TOut)));
}
```

To

```csharp
internal static class AnalysisOutput<TOut> {
    public static Fin<Seq<TOut>> Project<TValue>(Op key, Seq<TValue> values) =>
        typeof(TOut) == typeof(TValue)
            ? values.TraverseM(value => key.AcceptValue(value: value)).As().Map(static admitted => admitted.Map(static value => (TOut)(object)value!))
            : Fin.Fail<Seq<TOut>>(key.Unsupported(inputType: typeof(TValue), outputType: typeof(TOut)));
}
```

Location: request-algebra fence, scalar output admissions after tasks 2-8.

From

```csharp
new AnalysisOutput<TOut>(Key: op).One(value: value)
new AnalysisOutput<Rasm.Domain.CurveForm>(Key: op).One(value: form)
new AnalysisOutput<Point2d>(Key: state.Key).One(value: result)
new AnalysisOutput<ClosestHit>(Key: state.Key).One(value: hit)
new AnalysisOutput<double>(Key: state.Key).One(value: distance)
```

To

```csharp
AnalysisOutput<TOut>.Project(key: op, values: Seq(value))
AnalysisOutput<Rasm.Domain.CurveForm>.Project(key: op, values: Seq(form))
AnalysisOutput<Point2d>.Project(key: state.Key, values: Seq(result))
AnalysisOutput<ClosestHit>.Project(key: state.Key, values: Seq(hit))
AnalysisOutput<double>.Project(key: state.Key, values: Seq(distance))
```

Location: request-algebra fence, sequence output admissions after tasks 4-5 and 10.

From

```csharp
new AnalysisOutput<Point3d>(Key: op).Many(values: points)
new AnalysisOutput<Point3d>(Key: state.Key).Many(values: points)
new AnalysisOutput<TOut>(Key: op).Many(values: found.Values)
new AnalysisOutput<TOut>(Key: op).Many(values: found.Values)
```

To

```csharp
AnalysisOutput<Point3d>.Project(key: op, values: points)
AnalysisOutput<Point3d>.Project(key: state.Key, values: points)
AnalysisOutput<TOut>.Project(key: op, values: found.Values)
AnalysisOutput<TOut>.Project(key: op, values: found.Values)
```

Why: `One` forwards to `Many`, `Many` forwards to `Project`, and the record exists only to carry one key into the final call.

Change: Retain one static projection operation and pass its key and sequence explicitly.

Delta: -4 LOC; -3 explicit members plus the generated record constructor/equality/deconstruction surface; neutral type count and admission capability; removes every projection-object construction.

Ripples: Replace `new AnalysisOutput<TOut>(Key: op).Many(values: Seq(deviation))` with `AnalysisOutput<TOut>.Project(key: op, values: Seq(deviation))` in `libs/dotnet/Rasm/.planning/Analysis/relations.md`; update the operation-runtime owner prose and density row from `One`/`Many` to `Project`.

# 12. Type the conformance budget and carry percentile data directly

Location: request-algebra fence, `ConformanceCase` payload and pair build (lines 236 and 240-241 after task 5).

From

```csharp
public sealed record ConformanceCase(ConformanceMetric Metric, Option<int> Count, Seq<double> Percentiles) : AnalysisQuery, ISingleQuery, IPairQuery {
Operation<(TA A, TB B), TOut> IPairQuery.Build<TA, TB, TOut>(Op key) =>
    ConformanceMetric.Sampled<TA, TB, TOut>(metric: Metric, count: Count, percentiles: Percentiles, key: key);
```

To

```csharp
public sealed record ConformanceCase(ConformanceMetric Metric, Option<Dimension> Count, Seq<double> Percentiles) : AnalysisQuery, ISingleQuery, IPairQuery {
Operation<(TA A, TB B), TOut> IPairQuery.Build<TA, TB, TOut>(Op key) =>
    ConformanceMetric.Sampled<TA, TB, TOut>(metric: Metric, count: Count, percentiles: Percentiles, key: key);
```

Location: request-algebra fence, conformance factory (lines 291-296).

From

```csharp
public static Fin<AnalysisQuery> Conformance(ConformanceMetric metric, Option<int> count = default, params ReadOnlySpan<double> percentiles) {
    Seq<double> requested = Iterable<double>.FromSpan(percentiles).ToSeq();
    return requested.IsEmpty || metric.Equals(ConformanceMetric.Distribution)
        ? Fin.Succ<AnalysisQuery>(new ConformanceCase(Metric: metric, Count: count, Percentiles: requested))
        : Fin.Fail<AnalysisQuery>(AnalysisVerb.Conformance.Op.InvalidInput(axis: nameof(percentiles)));
}
```

To

```csharp
public static Fin<AnalysisQuery> Conformance(ConformanceMetric metric, Option<Dimension> count = default, Seq<double> percentiles = default) =>
    percentiles.IsEmpty || metric.Equals(ConformanceMetric.Distribution)
        ? Fin.Succ<AnalysisQuery>(new ConformanceCase(Metric: metric, Count: count, Percentiles: percentiles))
        : Fin.Fail<AnalysisQuery>(AnalysisVerb.Conformance.Op.InvalidInput(axis: nameof(percentiles)));
```

Why: The sample budget is the same positive-count domain as other kernel budgets. The optional count placed before a `params` span also blocks positional percentile calls, while the case already stores the LanguageExt `Seq` the downstream fold consumes.

Change: Carry `Option<Dimension>` and accept the percentile carrier directly, removing the raw-count state, the optional-before-spread trap, and the span-to-sequence copy.

Delta: -2 LOC; neutral module-level symbol/member/type count; replaces one raw scalar with an existing generated value owner.

Ripples: Change `ConformanceMetric.Sampled` and its build path in `libs/dotnet/Rasm/.planning/Analysis/measure.md` from `Option<int>` to `Option<Dimension>`, using `Dimension.Value` only at the sampling edge; the sole direct external consumer calls `Conformance(metric)` and needs no change.

# 13. Defer unused rejected-operation construction

Location: operation-runtime fence, three `Analyze.Query` fallbacks (lines 561-571).

From

```csharp
return Optional(query).Map(q => q.Single<TGeometry, TOut>(key: active)).IfNone(Operation<TGeometry, TOut>.Reject(key: active, fault: active.InvalidInput()));
return Optional(query).Map(q => q.Pair<TA, TB, TOut>(key: active)).IfNone(Operation<(TA A, TB B), TOut>.Reject(key: active, fault: active.InvalidInput()));
return Optional(query).Map(q => q.Service<TOut>(key: active)).IfNone(Operation<Unit, TOut>.Reject(key: active, fault: active.InvalidInput()));
```

To

```csharp
return Optional(query).Map(q => q.Single<TGeometry, TOut>(key: active)).IfNone(() => Operation<TGeometry, TOut>.Reject(key: active, fault: active.InvalidInput()));
return Optional(query).Map(q => q.Pair<TA, TB, TOut>(key: active)).IfNone(() => Operation<(TA A, TB B), TOut>.Reject(key: active, fault: active.InvalidInput()));
return Optional(query).Map(q => q.Service<TOut>(key: active)).IfNone(() => Operation<Unit, TOut>.Reject(key: active, fault: active.InvalidInput()));
```

Why: The value overload constructs a rejected `Operation` even when the query is present. LanguageExt's lazy `IfNone(Func<T>)` preserves the same absence verdict without allocating the unused union body.

Change: Pass each rejected operation as the carrier's deferred fallback.

Delta: 0 LOC; neutral module-level symbol/member/type count and behavior; removes one unused rejected-operation construction from every successful query build.

# 14. Remove the unused telemetry projection

Location: operation-runtime fence, `Env.Taps` (line 421).

From

```csharp
public static readonly Eff<Env, Option<TelemetrySink>> Taps = Eff.runtime<Env>().Map(static env => env.Telemetry).As();
```

To

```csharp
// Taps DELETED
```

Why: `Taps` is a policy-free projection of `Env.Telemetry` with no direct consumer; telemetry charging reads the runtime once through `EnvAsks` and then reads the field.

Change: Remove the duplicate reader accessor while retaining `Env.Telemetry` and the used whole-runtime effect.

Delta: -1 LOC; -1 member; neutral type count and telemetry capability.

# 15. Remove the mirrored verb operation surface

Location: request-algebra fence, `AnalysisVerb.Op` and `Keys` (lines 121-123).

From

```csharp
public Op Op => Keys.Value[this];
private static readonly Lazy<FrozenDictionary<AnalysisVerb, Op>> Keys =
    new(static () => Items.ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Key)));
```

To

```csharp
// AnalysisVerb.Op DELETED
// AnalysisVerb.Keys DELETED
```

Location: request-algebra fence, the sole code consumer of `AnalysisVerb.Op` (line 295).

From

```csharp
AnalysisVerb.Conformance.Op.InvalidInput(axis: nameof(percentiles))
```

To

```csharp
Rasm.Domain.Op.Of(name: AnalysisVerb.Conformance.Key).InvalidInput(axis: nameof(percentiles))
```

Why: The frozen dictionary stores `Op.Of(row.Key)` for every verb, while the forwarding accessor has one code consumer. `Op` identity is its string value, so that consumer can derive it from the generated Thinktecture key without either module-level member.

Change: Derive the conformance fault key at its use and delete both the operation accessor and mirrored cache.

Delta: -3 LOC; -2 module-level members; type count neutral.

Ripples: Replace prose references to `AnalysisVerb.<row>.Op`, including `libs/dotnet/Rasm/.planning/Parametric/locate.md:18`, with direct `Op.Of(row.Key)` derivation only where a caller-owned operation key is actually required.

# 16. Remove geometry-query construction forwarders

Location: request-algebra fence, geometry factories after Tasks 2-8 (lines 271-277).

From

```csharp
public static AnalysisQuery CurveForm => new CurveFormCase();
public static AnalysisQuery Vertices => new VerticesCase();
public static AnalysisQuery SurfaceUv(Point2d uv) => new SurfaceUvCase(Uv: uv);
public static AnalysisQuery Closest(Point3d target) => new ClosestCase(Target: target);
public static AnalysisQuery SignedDistance(Point3d sample) => new SignedDistanceCase(Sample: sample);
```

To

```csharp
// AnalysisQuery.CurveForm DELETED
// AnalysisQuery.Vertices DELETED
// AnalysisQuery.SurfaceUv DELETED
// AnalysisQuery.Closest DELETED
// AnalysisQuery.SignedDistance DELETED
```

Why: Each member is a policy-free alias for a public case constructor. `Dimension` already owns sample-count admission, so its factory adds no validation either.

Change: Construct the five remaining geometry cases directly and delete the duplicate construction surface.

Delta: -5 LOC; -5 module-level members; type count neutral.

Ripples: Update request-algebra owner and growth prose to name public cases as the construction surface. The current planning corpus has no external call site for these five factories.

# 17. Remove family-query construction forwarders

Location: request-algebra fence, family factories after Task 9 (lines 279-285).

From

```csharp
public static AnalysisQuery Measure(Measure query) => new MeasureCase(Query: query);
public static AnalysisQuery Location(Location query) => new LocationCase(Query: query);
public static AnalysisQuery Selection(Curves query) => new CurvesCase(Query: query);
public static AnalysisQuery Selection(Faces query) => new FacesCase(Query: query);
public static AnalysisQuery Selection(Topologies query) => new TopologiesCase(Query: query);
public static AnalysisQuery MeshPointSpatial(Meshes query) => new MeshesCase(Query: query);
public static AnalysisQuery MeshPointSpatial(Points query) => new PointsCase(Query: query);
```

To

```csharp
// AnalysisQuery.Measure DELETED
// AnalysisQuery.Location DELETED
// AnalysisQuery.Selection(Curves) DELETED
// AnalysisQuery.Selection(Faces) DELETED
// AnalysisQuery.Selection(Topologies) DELETED
// AnalysisQuery.MeshPointSpatial(Meshes) DELETED
// AnalysisQuery.MeshPointSpatial(Points) DELETED
```

Why: These members pass one family value unchanged into its public query case. The case name already states the family and the wrapper owns no admission or normalization.

Change: Construct family query cases directly and delete the forwarding façade.

Delta: -7 LOC; -7 module-level members; type count neutral.

Ripples: In `libs/dotnet/Rasm.Fabrication/.planning/Spec/manufacturability.md:1338`, use `new AnalysisQuery.MeshesCase(Query: new Meshes.SamplesCase(Group: MeshSampleGroup.Defect))`. Replace `AnalysisQuery.Location(...)` prose and callers in `libs/dotnet/Rasm/.planning/Parametric/locate.md` and `libs/dotnet/Rasm/ARCHITECTURE.md` with `AnalysisQuery.LocationCase` construction. Update request-algebra owner and growth prose accordingly.

# 18. Remove relation and spatial construction forwarders

Location: request-algebra fence, relation and spatial factories after Tasks 9-10 (lines 286-299).

From

```csharp
public static AnalysisQuery Intersections => new IntersectionsCase();
public static AnalysisQuery Classification => new ClassificationCase();
public static AnalysisQuery CurveDeviation => new CurveDeviationCase();
public static AnalysisQuery SelfIntersection => new SelfIntersectionCase();
public static AnalysisQuery Ray(RayQuery query) => new RayCase(Query: query);
public static AnalysisQuery Search(NeighborIndex index, BoundingBox box) => new SearchBoxCase(Index: index, Box: box);
public static AnalysisQuery Search(NeighborIndex index, Sphere sphere) => new SearchSphereCase(Index: index, Sphere: sphere);
public static AnalysisQuery Overlaps(NeighborIndex left, NeighborIndex right, Tolerance band) => new OverlapCase(Left: left, Right: right, Band: band);
```

To

```csharp
// AnalysisQuery.Intersections DELETED
// AnalysisQuery.Classification DELETED
// AnalysisQuery.CurveDeviation DELETED
// AnalysisQuery.SelfIntersection DELETED
// AnalysisQuery.Ray DELETED
// AnalysisQuery.SearchBox DELETED
// AnalysisQuery.SearchSphere DELETED
// AnalysisQuery.Overlaps DELETED
```

Why: Every member forwards its arguments unchanged to a public case. The two `Search` overloads additionally obscure which spatial request case is being created.

Change: Construct the relation or spatial case directly. Retain `Bounds`, `Conformance`, and `PointPairs` because they respectively apply a canonical default, validate coupled arguments, and convert span-boundary inputs into owned sequences.

Delta: -8 LOC; -8 module-level members; type count neutral.

Ripples: Update request-algebra owner and growth prose to reserve factories for normalization or admission boundaries. The current planning corpus has no external call site for these eight factories.
