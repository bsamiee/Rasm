# `evaluation.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/evaluation.md`

Apply these moves in order. The final queue removes **37 fenced C# LOC** and **16 authored module/type/member symbols** from the target. Direct consumer ripples remove another **27 fenced C# LOC** and **3 module members**. Generated members retired with `TypeMatch` are additional and are not counted.

| Order | Move | Target LOC | Authored symbols |
| ---: | --- | ---: | ---: |
| 1 | Inline the one-use lifted-surface hop | -1 | -1 |
| 2 | Absorb `BrepFace` into the `Surface` row | -7 | -1 |
| 3 | Derive matching from `ClosestForm.Native` | -9 | -5 |
| 4 | Memoize the pure roster function directly | -2 | -2 |
| 5 | Project directly from generic `Evaluate<TOut>` | -13 | -5 |
| 6 | Inline the one-use signed-distance fold | -3 | -1 |
| 7 | Inline the one-use surface-point projection | -2 | -1 |

## Move 1 — Inline the one-use lifted-surface hop

**Location:** `evaluation.md:182`, anchor `public static readonly ClosestForm Sphere`; and `:283`, anchor `private static Fin<ClosestHit> Lifted`.

**From:**

```csharp
public static readonly ClosestForm Sphere = new(
    native: typeof(Sphere), match: TypeMatch.Exact,
    recover: static (value, target, key) => Normalization.SurfaceForm(source: (Sphere)value, key: key)
        .Bind(lease => lease.Use((Target: target, Key: key), static (state, surface) => Lifted(surface: surface, target: state.Target, key: state.Key))));
private static Fin<ClosestHit> Lifted(Surface surface, Point3d target, Op key) =>
    Of(type: surface.GetType()).ToFin(key.Unsupported(inputType: surface.GetType(), outputType: typeof(ClosestHit)))
        .Bind(row => row.Recover(value: surface, target: target, key: key));
```

**To:**

```csharp
public static readonly ClosestForm Sphere = new(
    native: typeof(Sphere), match: TypeMatch.Exact,
    recover: static (value, target, key) => Normalization.SurfaceForm(source: (Sphere)value, key: key)
        .Bind(lease => lease.Use((Target: target, Key: key), static (state, surface) =>
            Of(type: surface.GetType()).ToFin(state.Key.Unsupported(inputType: surface.GetType(), outputType: typeof(ClosestHit)))
                .Bind(row => row.Recover(value: surface, target: state.Target, key: state.Key)))));
```

**Effect:** `-1` target LOC; `-1` module member (`ClosestForm.Lifted`).

**API/consumer proof:** `Lifted` has one caller and only composes `Option.ToFin` with `Fin.Bind`; it owns no admission, lifetime, or policy. Keep the `Sphere` row itself: deleting it and falling through `Recovered` would re-enter `Closest` and add a second `OpAcceptance.ValidityOf` gate over the lowered surface, which is not the current path and is therefore not an equivalence-preserving reduction.

**Ripples:** none.

## Move 2 — Absorb `BrepFace` into the `Surface` row

**Location:** `evaluation.md:206-230`, anchors `ClosestForm Face` and `ClosestForm Surface`.

### 2A — Delete the separate face-row shell

**From:**

```csharp
public static readonly ClosestForm Face = new(
    native: typeof(BrepFace), match: TypeMatch.Assignable,
    recover: static (value, target, key) => ((BrepFace)value) switch {
        BrepFace face when face.ClosestPointOnFace(testPoint: target, u: out double u, v: out double v, maximumDistance: 0.0) =>
```

**To:** delete.

### 2B — Delete the face-row materialization tail

**From:**

```csharp
            Evaluation.NormalAt(surface: face, uv: new Point2d(x: u, y: v), key: key).Map(normal => ClosestHit.At(
                target: target,
                point: face.PointAt(u: u, v: v),
                uv: Some(new Point2d(x: u, y: v)),
                normal: Some(normal),
                component: face.FaceIndex >= 0 ? Some(new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)) : Option<ComponentIndex>.None,
                frame: Evaluation.FrameAt(surface: face, uv: new Point2d(x: u, y: v), key: key).ToOption())),
        _ => Fin.Fail<ClosestHit>(key.InvalidInput()),
    });
```

**To:** delete.

### 2C — Give `Surface` one shared materializer

**From:**

```csharp
public static readonly ClosestForm Surface = new(
    native: typeof(Surface), match: TypeMatch.Assignable,
    recover: static (value, target, key) => ((Surface)value) switch {
        Surface surface when surface.ClosestPoint(testPoint: target, u: out double u, v: out double v) =>
            Evaluation.NormalAt(surface: surface, uv: new Point2d(x: u, y: v), key: key).Map(normal => ClosestHit.At(
```

**To:**

```csharp
public static readonly ClosestForm Surface = new(
    native: typeof(Surface), match: TypeMatch.Assignable,
    recover: static (value, target, key) => {
        Fin<ClosestHit> Hit(Surface surface, Point2d uv) =>
            Evaluation.NormalAt(surface: surface, uv: uv, key: key).Map(normal => ClosestHit.At(
                target: target, point: surface.PointAt(u: uv.X, v: uv.Y), uv: Some(uv),
                normal: Some(normal),
                component: surface is BrepFace { FaceIndex: >= 0 } face
                    ? Some(new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)) : Option<ComponentIndex>.None,
```

### 2D — Dispatch both host probes into that materializer

**From:**

```csharp
                target: target,
                point: surface.PointAt(u: u, v: v),
                uv: Some(new Point2d(x: u, y: v)),
                normal: Some(normal),
                frame: Evaluation.FrameAt(surface: surface, uv: new Point2d(x: u, y: v), key: key).ToOption())),
        _ => Fin.Fail<ClosestHit>(key.InvalidInput()),
    });
```

**To:**

```csharp
                frame: Evaluation.FrameAt(surface: surface, uv: uv, key: key).ToOption()));
        return ((Surface)value) switch {
            BrepFace face when face.ClosestPointOnFace(target, out double u, out double v, 0.0) =>
                Hit(face, new Point2d(x: u, y: v)),
            Surface surface when surface is not BrepFace && surface.ClosestPoint(target, out double u, out double v) =>
                Hit(surface, new Point2d(x: u, y: v)),
            _ => Fin.Fail<ClosestHit>(key.InvalidInput()),
        };
    });
```

**Effect:** `-7` target LOC; `-1` module member (`ClosestForm.Face`). The two-call local function adds no module surface and avoids the extra `Fin.Succ`/`Bind` allocation an intermediate hit-product carrier would introduce.

**API/consumer proof:** RhinoCommon establishes `BrepFace : Surface`. Both rows share identity, result, normal/frame construction, and consumer; only the host probe differs. The `BrepFace` arm remains first, and `surface is not BrepFace` is required so a failed trimmed-face probe cannot fall through to untrimmed `Surface.ClosestPoint`. `ClosestForm.Face` has no external consumer.

**Ripples:** update `[03]-[ROSTER]` to state that `Surface` probes `BrepFace` first internally; delete the separate-row ordering claim.

## Move 3 — Derive matching from `ClosestForm.Native`

**Location:** `evaluation.md:116-123`, the `TypeMatch` declaration; every remaining `ClosestForm` row header; and `:274-278`, anchors `Native`, `Match`, and `Admits`.

### 3A — Delete the two-item policy owner

**From:**

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class TypeMatch {
    public static readonly TypeMatch Exact = new(key: "exact", holds: static (candidate, native) => candidate == native);
    public static readonly TypeMatch Assignable = new(key: "assignable", holds: static (candidate, native) => native.IsAssignableFrom(c: candidate));
    [UseDelegateFromConstructor]
    internal partial bool Holds(Type candidate, Type native);
}
```

**To:** delete.

### 3B — Remove the reconstructable row argument

**From:**

```csharp
native: typeof(Point3d), match: TypeMatch.Exact,
native: typeof(Line), match: TypeMatch.Exact,
native: typeof(Polyline), match: TypeMatch.Exact,
native: typeof(Plane), match: TypeMatch.Exact,
native: typeof(Sphere), match: TypeMatch.Exact,
native: typeof(Box), match: TypeMatch.Exact,
native: typeof(BoundingBox), match: TypeMatch.Exact,
```

**To:**

```csharp
native: typeof(Point3d),
native: typeof(Line),
native: typeof(Polyline),
native: typeof(Plane),
native: typeof(Sphere),
native: typeof(Box),
native: typeof(BoundingBox),
```

**From:**

```csharp
native: typeof(RhinoPoint), match: TypeMatch.Assignable,
native: typeof(PointCloud), match: TypeMatch.Assignable,
native: typeof(Curve), match: TypeMatch.Assignable,
native: typeof(Surface), match: TypeMatch.Assignable,
native: typeof(Brep), match: TypeMatch.Assignable,
native: typeof(Mesh), match: TypeMatch.Assignable,
```

**To:**

```csharp
native: typeof(RhinoPoint),
native: typeof(PointCloud),
native: typeof(Curve),
native: typeof(Surface),
native: typeof(Brep),
native: typeof(Mesh),
```

### 3C — Read the CLR relation directly

**From:**

```csharp
internal Type Native { get; }
internal TypeMatch Match { get; }
[UseDelegateFromConstructor]
internal partial Fin<ClosestHit> Recover(object value, Point3d target, Op key);
internal bool Admits(Type type) => Match.Holds(candidate: type, native: Native);
```

**To:**

```csharp
internal Type Native { get; }
[UseDelegateFromConstructor]
internal partial Fin<ClosestHit> Recover(object value, Point3d target, Op key);
internal bool Admits(Type type) => Native.IsAssignableFrom(c: type);
```

**Effect:** `-9` target LOC; `-5` authored symbols (`TypeMatch`, `Exact`, `Assignable`, `Holds`, `ClosestForm.Match`).

**API/consumer proof:** every former `Exact` native is a value type, so `IsAssignableFrom` admits only that boxed type. Every former `Assignable` native is a reference base whose derived runtime forms already belong to the row. `TypeMatch` has no identity consumer and adds a constructor delegate solely to recover a fact already encoded by `Native`.

**Ripples:** replace the `TypeMatch` prose with the one `Native.IsAssignableFrom(runtimeType)` correspondence.

## Move 4 — Memoize the pure roster function directly

**Location:** `evaluation.md:278-281`, anchors `Admits`, `Resolved`, and `Of`; plus `:355`, the sole named-argument invocation.

**From:**

```csharp
internal bool Admits(Type type) => Native.IsAssignableFrom(c: type);
private static readonly Atom<HashMap<Type, Option<ClosestForm>>> Resolved = Atom(HashMap<Type, Option<ClosestForm>>());
internal static Option<ClosestForm> Of(Type type) =>
    Cell.Claim(cell: Resolved, key: type, mint: () => toSeq(Items).Find(row => row.Admits(type: type))).Current[type];
```

**To:**

```csharp
internal static readonly Func<Type, Option<ClosestForm>> Of =
    memo((Type type) => toSeq(Items).Find(row => row.Native.IsAssignableFrom(c: type)));
```

**From:**

```csharp
ClosestForm.Of(type: source.GetType())
```

**To:**

```csharp
ClosestForm.Of(source.GetType())
```

**Effect:** `-2` target LOC; `-2` module members (`ClosestForm.Admits`, `ClosestForm.Resolved`). `Of` remains one symbol.

**API/consumer proof:** the local LanguageExt catalogue provides synchronized `memo(Func<A,B>)`. Resolution is a pure `Type -> Option<ClosestForm>` function over immutable Thinktecture `Items`; `memo` preserves negative results and the same default `Type` equality. The current `Cell.Claim` mints its candidate before checking the held key, so it still rescans `Items` on every call; the pure memo is both smaller and the actual cache.

**Ripples:** none outside this file.

## Move 5 — Project directly from generic `Evaluate<TOut>`

**Location:** `evaluation.md:325-350`, anchors `EvaluationResult` and `Evaluate`; direct consumers in `Analysis/query.md`, `Analysis/measure.md`, `Analysis/select.md`, `Spatial/support.md`, and `Processing/sample.md`.

### 5A — Delete the transient result case family

**From:**

```csharp
[Union]
public abstract partial record EvaluationResult {
    private EvaluationResult() { }
    public sealed record Hit(ClosestHit Value) : EvaluationResult;
    public sealed record Distance(double Value) : EvaluationResult;
    public sealed record Points(Seq<Point3d> Value) : EvaluationResult;
```

**To:** delete.

**From:**

```csharp
    internal Fin<TOut> Project<TOut>(Op key) =>
        Switch(
            state: key,
            hit: static (op, result) => result.Value.Project<TOut>(key: op),
            distance: static (op, result) => AtomProjection.Value<double, TOut>(value: result.Value, key: op, owner: typeof(EvaluationResult)),
            points: static (op, result) => AtomProjection.Values<Point3d, TOut>(values: result.Value, key: op, owner: typeof(EvaluationResult)));
}
```

**To:** delete.

### 5B — Make the existing request fold the typed egress

**From:**

```csharp
public Fin<EvaluationResult> Evaluate(EvaluationRequest request, Op key) =>
```

**To:**

```csharp
public Fin<TOut> Evaluate<TOut>(EvaluationRequest request, Op key) =>
```

**From:**

```csharp
closest: static (state, verb) => Closest(source: state.Source, target: verb.Target, key: state.Key).Map(static hit => (EvaluationResult)new EvaluationResult.Hit(Value: hit)),
signed: static (state, verb) => Signed(source: state.Source, sample: verb.Sample, key: state.Key).Map(static distance => (EvaluationResult)new EvaluationResult.Distance(Value: distance)),
sample: static (state, verb) => Sampled(source: state.Source, count: verb.Count, context: verb.Model, key: state.Key).Map(static points => (EvaluationResult)new EvaluationResult.Points(Value: points)),
vertices: static (state, _) => Vertices(source: state.Source, key: state.Key).Map(static points => (EvaluationResult)new EvaluationResult.Points(Value: points)))
```

**To:**

```csharp
    closest: static (state, verb) => Closest(source: state.Source, target: verb.Target, key: state.Key).Bind(hit => hit.Project<TOut>(key: state.Key)),
    signed: static (state, verb) => Signed(source: state.Source, sample: verb.Sample, key: state.Key).Bind(value => AtomProjection.Value<double, TOut>(value, state.Key, typeof(Evaluation))),
    sample: static (state, verb) => Sampled(source: state.Source, count: verb.Count, context: verb.Model, key: state.Key).Bind(values => AtomProjection.Values<Point3d, TOut>(values, state.Key, typeof(Evaluation))),
    vertices: static (state, _) => Vertices(source: state.Source, key: state.Key).Bind(values => AtomProjection.Values<Point3d, TOut>(values, state.Key, typeof(Evaluation))))
```

**Target effect:** `-13` fenced LOC; `-5` authored symbols (`EvaluationResult`, its three cases, and `Project`).

**API/consumer proof:** every result consumer immediately projects the fresh case to its requested output; none stores, constructs, type-tests, serializes, or passes `EvaluationResult`. The same `ClosestHit.Project`, `AtomProjection.Value`, and `AtomProjection.Values` folds remain, now one hop earlier. This narrows the public surface and removes the wrapper allocation entirely. A Thinktecture ad-hoc struct union is rejected here: its generated storage carries the large `ClosestHit` field in every result value and introduces a poison `default`, while preserving a result owner no consumer needs.

### 5C — Collapse producer-plus-projection at consumers

`Spatial/support.md:160-166`:

**From:**

```csharp
.Evaluate(request: new EvaluationRequest.Closest(Target: s.Sample), key: s.Key)
.Bind(result => result.Project<ClosestHit>(key: s.Key))
Payload.Evaluate(request: new EvaluationRequest.Signed(Sample: sample), key: key)
    .Bind(result => result.Project<double>(key: key));
```

**To:**

```csharp
.Evaluate<ClosestHit>(request: new EvaluationRequest.Closest(Target: s.Sample), key: s.Key)
Payload.Evaluate<double>(request: new EvaluationRequest.Signed(Sample: sample), key: key);
```

`Analysis/query.md:338-372`:

**From:**

```csharp
from answer in geometry.Evaluate(request: new EvaluationRequest.Vertices(), key: op).ToEff()
from points in answer.Sites(key: op).ToEff()
from answer in geometry.Evaluate(request: new EvaluationRequest.Sample(Count: state.Count, Model: context), key: state.Key).ToEff()
from points in answer.Sites(key: state.Key).ToEff()
from answer in geometry.Evaluate(request: new EvaluationRequest.Closest(Target: state.Target), key: state.Key).ToEff()
from hit in answer.Hit(key: state.Key).ToEff()
from answer in geometry.Evaluate(request: new EvaluationRequest.Signed(Sample: state.Sample), key: state.Key).ToEff()
from distance in answer.Span(key: state.Key).ToEff()
```

**To:**

```csharp
from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Vertices(), key: op).ToEff()
from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: state.Count, Model: context), key: state.Key).ToEff()
from hit in geometry.Evaluate<ClosestHit>(request: new EvaluationRequest.Closest(Target: state.Target), key: state.Key).ToEff()
from distance in geometry.Evaluate<double>(request: new EvaluationRequest.Signed(Sample: state.Sample), key: state.Key).ToEff()
```

`Analysis/select.md:587-606`:

**From:**

```csharp
from answer in geometry.Evaluate(request: new EvaluationRequest.Vertices(), key: op).ToEff()
from points in answer.Sites(key: op).ToEff()
from answer in geometry.Evaluate(request: new EvaluationRequest.Vertices(), key: state.Key).ToEff()
from points in answer.Sites(key: state.Key).ToEff()
```

**To:**

```csharp
from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Vertices(), key: op).ToEff()
from points in geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Vertices(), key: state.Key).ToEff()
```

`Analysis/measure.md:584-585,739,743`:

**From:**

```csharp
.Bind(box => geometry.Evaluate(request: new EvaluationRequest.Sample(Count: count.IfNone(() => Budget(box: box, context: context)), Model: context), key: key)
    .Bind(answer => answer.Sites(key: key))
sampler: static (c, n, ctx, op) => c.Evaluate(request: new EvaluationRequest.Sample(Count: n, Model: ctx), key: op).Bind(answer => answer.Sites(key: op)),
sampler: static (s, n, ctx, op) => s.Evaluate(request: new EvaluationRequest.Sample(Count: n, Model: ctx), key: op).Bind(answer => answer.Sites(key: op)),
```

**To:**

```csharp
.Bind(box => geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: count.IfNone(() => Budget(box: box, context: context)), Model: context), key: key)
sampler: static (c, n, ctx, op) => c.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: n, Model: ctx), key: op),
sampler: static (s, n, ctx, op) => s.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: n, Model: ctx), key: op),
```

`Processing/sample.md:551-552`:

**From:**

```csharp
from evaluated in space.Payload.Evaluate(new EvaluationRequest.Sample(Count: (int)Math.Ceiling(a: count * Math.Max(1.0, kind.Facts.CandidateScale)), Model: context), key)
from points in evaluated.Project<Seq<Point3d>>(key: key)
```

**To:**

```csharp
from points in space.Payload.Evaluate<Seq<Point3d>>(new EvaluationRequest.Sample(Count: (int)Math.Ceiling(a: count * Math.Max(1.0, kind.Facts.CandidateScale)), Model: context), key)
```

### 5D — Delete the duplicate consumer assertions

**From:**

```csharp
extension(EvaluationResult result) {
    internal Fin<ClosestHit> Hit(Op key) => result.Switch(
        state: key,
        hit: static (_, value) => Fin.Succ(value.Value),
        distance: static (op, _) => Fin.Fail<ClosestHit>(op.InvalidResult()),
        points: static (op, _) => Fin.Fail<ClosestHit>(op.InvalidResult()));
```

**To:** delete.

**From:**

```csharp
    internal Fin<double> Span(Op key) => result.Switch(
        state: key,
        hit: static (op, _) => Fin.Fail<double>(op.InvalidResult()),
        distance: static (_, value) => Fin.Succ(value.Value),
        points: static (op, _) => Fin.Fail<double>(op.InvalidResult()));
```

**To:** delete.

**From:**

```csharp
    internal Fin<Seq<Point3d>> Sites(Op key) => result.Switch(
        state: key,
        hit: static (op, _) => Fin.Fail<Seq<Point3d>>(op.InvalidResult()),
        distance: static (op, _) => Fin.Fail<Seq<Point3d>>(op.InvalidResult()),
        points: static (_, value) => Fin.Succ(value.Value));
}
```

**To:** delete.

**Consumer effect:** `-27` fenced LOC and `-3` module members. This includes the 17-line `OperationLift` result-extension deletion and ten paired producer/projection collapses; the two one-line `Analysis/measure` samplers are token-only edits.

**Ripples:** update the target index/owner/entry/output prose and diagram to name typed `Evaluate<TOut>` egress, and remove `EvaluationResult` claims from `Spatial/support.md` and `Analysis/query.md`.

## Move 6 — Inline the one-use signed-distance fold

**Location:** `evaluation.md:84-88`, anchor `SignedDistanceFrom`; and `:367-368`, the `Capability.ClosestNormal` arm.

**From:**

```csharp
internal Fin<double> SignedDistanceFrom(Point3d sample, Op key) {
    ClosestHit hit = this;
    return hit.Distance.ToFin(Fail: key.InvalidResult()).Bind(distance =>
        hit.Normal.ToFin(Fail: key.InvalidResult()).Map(normal => ((sample - hit.Point) * normal) >= 0.0 ? distance : -distance));
}
```

**From:**

```csharp
object value when Capability.ClosestNormal.Admits(type: value.GetType()) =>
    Closest(source: value, target: point, key: key).Bind(hit => hit.SignedDistanceFrom(sample: point, key: key)),
```

**To:**

```csharp
object value when Capability.ClosestNormal.Admits(type: value.GetType()) =>
    Closest(source: value, target: point, key: key).Bind(hit =>
        hit.Distance.ToFin(Fail: key.InvalidResult()).Bind(distance =>
            hit.Normal.ToFin(Fail: key.InvalidResult()).Map(normal => ((point - hit.Point) * normal) >= 0.0 ? distance : -distance))),
```

Delete the `SignedDistanceFrom` member after replacing its sole call.

**Effect:** `-3` target LOC; `-1` module member. Failure order remains distance, then normal; the sign expression is unchanged.

**API/consumer proof:** repository search finds one call, inside the `Signed` arm that already owns the sample, hit, and failure carrier. The member adds no independent admission or reuse surface. `Option.ToFin`, `Fin.Bind`, and `Fin.Map` remain unchanged.

**Ripples:** move the sign-convention prose from `ClosestHit` to `Evaluation.Signed`.

## Move 7 — Inline the one-use surface-point projection

**Location:** `evaluation.md:375`, anchor `Surface surface => SurfaceSamplePoints`; and `:441-443`, anchor `SurfaceSamplePoints`.

**From:**

```csharp
Surface surface => SurfaceSamplePoints(surface: surface, count: count, context: context, key: key),
internal static Fin<Seq<Point3d>> SurfaceSamplePoints(Surface surface, int count, Context context, Op key) =>
    SurfaceSampleUv(surface: surface, count: count, context: context, key: key)
        .Map(uvs => uvs.Map(uv => surface.PointAt(u: uv.X, v: uv.Y)));
```

**To:**

```csharp
Surface surface => SurfaceSampleUv(surface: surface, count: count, context: context, key: key)
    .Map(uvs => uvs.Map(uv => surface.PointAt(u: uv.X, v: uv.Y))),
```

**Effect:** `-2` target LOC; `-1` module member (`Evaluation.SurfaceSamplePoints`).

**API/consumer proof:** the helper has one caller and only forwards through `Fin.Map` and `Seq.Map`. `SurfaceSampleUv` remains the shared admission owner used by `Parametric/locate` and `Parametric/projections`.

**Ripples:** none.

## Protected non-moves

- Keep `ClosestHit.Distance` as `Option<double>`. `ClosestHit` is a public `readonly record struct`; the option bit is what makes its CLR `default` invalid. A bare `double` would turn the zero/default ghost into valid evidence unless the whole hit becomes a generated admitted owner, which is not surgical.
- Keep both `ClosestHit.Basis` overloads and `Sense`. The origin/normal overload has four callers and centralizes the same Rhino `Plane` construction; deleting it would duplicate construction at every call for one LOC of cosmetic reduction. `Sense` and `Basis(Plane)` own predicates reused by `IsValid`.
- Keep the dedicated `Sphere` row after Move 1. Falling through `Recovered` adds a second validity gate over the lowered surface; that is a semantic tightening, not a pure deletion.
- Keep `PullBack`. It is single-use but is a named trimmed-face correction algorithm; inlining its seven-line two-probe fallback inside `Choose` saves no LOC and deepens the carrier expression.
- Keep `SurfaceDomain`, `SurfaceUv`, `SurfaceSampleUv`, `CurveSampleParameters`, `NormalAt`, and `FrameAt`; each has real cross-file consumers and owns shared admission or orientation semantics.
- Keep `EvaluationRequest.Sample.Count` as `int`. Replacing it with `Dimension` moves generated admission to every caller and introduces throwing `Create` calls or wider `Validate` plumbing for no net reduction.
- Keep the capability-specific recovery arm ahead of `ReadVertices` in `Sampled`; value forms can satisfy both, and deleting the first arm changes count-sensitive sampling into endpoint/corner extraction.
