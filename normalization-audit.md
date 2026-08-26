# `normalization.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/normalization.md`

Authority: `CLAUDE.md`; monorepo and .NET planning law; `docs/stacks/csharp/`; both checked-in `.api` tiers; and the direct `Rasm` consumers. The queue preserves the current owner partition: `Topology` recovery strata, `Kind` nominal identity, `Capability` admission, `CurveForm` evidence, `AnalyticForm` correspondence, `Normalization` erased ingress, and `TopologyProjection` custody.

Apply the moves in order. Every code edit below is independently bounded to fewer than ten changed lines; multi-site moves are split into anchored subchanges.

## 1. Delete the refusal-only `Topology.Unknown` row

Location: `Topology.Unknown`; the remaining `Topology` keys; `Domain/validation.md` `Requirement.ForKind`.

### 1a. Delete the row

From:

```csharp
public static readonly Topology Unknown = new(key: 0, recover: static (source, key) => Normalization.UnsupportedGeometry(source: source, key: key));
```

To:

```csharp
// deleted
```

### 1b. Compact the unshipped keys

From:

```csharp
key: 1, // Topology.Point
key: 2, // Topology.Curve
key: 3, // Topology.Surface
key: 4, // Topology.Brep
key: 5, // Topology.Mesh
key: 6, // Topology.SubD
key: 7, // Topology.PointCloud
key: 8, // Topology.Hatch
key: 9, // Topology.Extrusion
```

To:

```csharp
key: 0, // Topology.Point
key: 1, // Topology.Curve
key: 2, // Topology.Surface
key: 3, // Topology.Brep
key: 4, // Topology.Mesh
key: 5, // Topology.SubD
key: 6, // Topology.PointCloud
key: 7, // Topology.Hatch
key: 8, // Topology.Extrusion
```

### 1c. Remove the generated-dispatch arm

From, in `Requirement.ForKind`:

```csharp
unknown: Basic,
```

To:

```csharp
// deleted
```

Effect: target fenced LOC `-1`; consumer fenced LOC `-1`; type-member symbols `-1`; public smart-enum rows `-1`; generated `Topology.Map` arms `-1`.

API/consumer proof: no `Kind` row carries `Topology.Unknown`, no factory returns it, and the only cross-file mention is the exhaustive `unknown:` arm in `Requirement.ForKind`. Unsupported geometry already exits through `Fin` before a `Topology` exists. Thinktecture regenerates the nine-arm `Map` surface from the remaining rows; no numeric key, `Get`, `TryGet`, storage, or wire consumer exists in the checked-in corpus.

Ripples: remove the single `unknown:` arm from `Domain/validation.md`. No other consumer changes.

## 2. Derive analytic coercion from `AnalyticForm`; delete the mirrored capability

Location: analytic `Kind` memberships; `Kind.CurvePrimitives`, `Kind.SurfacePrimitives`, `Kind.CanCoerceTo`, `Kind.TypesHolding`; `Capability.Analytic`.

### 2a. Remove the five curve marker memberships

From, on `Line`, `Polyline`, `Circle`, `Arc`, and `Ellipse`:

```csharp
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.ReadEdges, Capability.CurveForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.ReadEdges, Capability.CurveForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.CurveForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.CurveForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.CurveForm, Capability.Analytic)
```

To:

```csharp
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.ReadEdges, Capability.CurveForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.ReadEdges, Capability.CurveForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.CurveForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.CurveForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.CurveForm)
```

### 2b. Remove the six surface/brep marker memberships

From, on `Plane`, `Sphere`, `Cylinder`, `Cone`, `Torus`, and `Box`:

```csharp
CapabilitySet<Capability>.Of(Capability.ReadControlPoints, Capability.SurfaceForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm, Capability.Analytic)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.ReadEdges, Capability.BrepForm, Capability.Analytic)
```

To:

```csharp
CapabilitySet<Capability>.Of(Capability.ReadControlPoints, Capability.SurfaceForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadControlPoints, Capability.SurfaceForm, Capability.BrepForm)
CapabilitySet<Capability>.Of(Capability.Bound, Capability.OrientedBound, Capability.ReadVertices, Capability.ReadControlPoints, Capability.ReadEdges, Capability.BrepForm)
```

### 2c. Replace both cached mirrors with the exact correspondence rows

From:

```csharp
private static readonly Lazy<FrozenSet<Type>> CurvePrimitives = TypesHolding(CapabilitySet<Capability>.Of(Capability.CurveForm, Capability.Analytic));
private static readonly Lazy<FrozenSet<Type>> SurfacePrimitives = TypesHolding(CapabilitySet<Capability>.Of(Capability.SurfaceForm, Capability.Analytic));
```

To:

```csharp
// deleted
```

From, in `CanCoerceTo`:

```csharp
|| (CurvePrimitives.Value.Contains(target) && Type == typeof(Curve))
|| (SurfacePrimitives.Value.Contains(target) && (Type == typeof(Brep) || Type == typeof(Surface)))
```

To:

```csharp
|| (Type == typeof(Curve) && AnalyticForm.Lane(topology: Topology.Curve)
    .Exists(row => row.Kind.Type == target))
|| ((Type == typeof(Brep) || Type == typeof(Surface)) && AnalyticForm.Lane(topology: Topology.Surface)
    .Exists(row => row.Kind.Type == target))
```

### 2d. Delete the cache builder and mirrored row

From:

```csharp
private static Lazy<FrozenSet<Type>> TypesHolding(CapabilitySet<Capability> required) =>
    new(() => Items.Where(row => row.Capabilities.AdmitsAll(required: required)).Select(static row => row.Type).ToFrozenSet());
public static readonly Capability Analytic = new("analytic", FrozenSet<Type>.Empty, Universal);
```

To:

```csharp
// deleted
```

Effect: fenced LOC `-3`; type-member symbols `-4`; public smart-enum rows `-1`; lazy caches `-2`; mirrored memberships `-11`.

API/consumer proof: `AnalyticForm.Lane` already derives from Thinktecture `Items` and returns `Seq<AnalyticForm>`; LanguageExt `Exists` tests the exact `Kind.Type`. Exact type equality preserves the old frozen-set behavior for derived target types. The curve lane contains `Line`, `Polyline`, `Circle`, `Arc`, and `Ellipse`; the surface lane contains `Plane`, `Sphere`, `Cylinder`, `Cone`, and `Torus`; `Box` remains on its explicit Brep arm. `Capability.Analytic` has no cross-file consumer.

Ripples: remove `Analytic` from the taxonomy prose. No consumer fence changes.

## 3. Delete the unused capability-rank mirror

Location: `Capability.Rank` and `Capability.RankIndex`.

From:

```csharp
public int Rank => RankIndex.Value[Key];
private static readonly Lazy<FrozenDictionary<string, int>> RankIndex = new(static () =>
    Items.Select(static (row, index) => (row.Key, Index: index)).ToFrozenDictionary(static pair => pair.Key, static pair => pair.Index, StringComparer.Ordinal));
```

To:

```csharp
// deleted
```

Effect: fenced LOC `3 -> 0` (`-3`); type-member symbols `-2`; public members `-1`; lazy caches `-1`.

API/consumer proof: `ICapability<TSelf>` requires only static `Items` and instance `Key`. No target or consumer reads `Capability.Rank`; `CapabilitySet.Wire` orders by key with `StringComparer.Ordinal`, not declaration rank.

Ripples: none.

## 4. Inline the one-hop `KindAdmits` wrapper

Location: `Capability.DecomposeFaces`, `Capability.EvaluateTopology`, and `Capability.KindAdmits`.

From:

```csharp
public static readonly Capability DecomposeFaces = new("decompose-faces", Set(typeof(BrepFace)),
    static type => Universal(type: type) || KindAdmits(type: type, predicate: static kind => kind.CanCoerceTo(target: typeof(Brep))));
public static readonly Capability EvaluateTopology = new("evaluate-topology", Set(typeof(Mesh), typeof(Brep)),
    static type => Universal(type: type) || KindAdmits(type: type, predicate: static kind =>
        kind.Topology.Equals(Topology.Mesh) || kind.Topology.Equals(Topology.Brep) || kind.CanCoerceTo(target: typeof(Brep))));
private static bool KindAdmits(Type type, Func<Kind, bool> predicate) => Kind.Of(type: type).Exists(predicate);
```

To:

```csharp
public static readonly Capability DecomposeFaces = new("decompose-faces", Set(typeof(BrepFace)),
    static type => Universal(type: type) || Kind.Of(type: type).Exists(static kind => kind.CanCoerceTo(target: typeof(Brep))));
public static readonly Capability EvaluateTopology = new("evaluate-topology", Set(typeof(Mesh), typeof(Brep)),
    static type => Universal(type: type) || Kind.Of(type: type).Exists(static kind =>
        kind.Topology.Equals(Topology.Mesh) || kind.Topology.Equals(Topology.Brep) || kind.CanCoerceTo(target: typeof(Brep))));
```

Effect: fenced LOC `6 -> 5` (`-1`); type-member symbols `-1`.

API/consumer proof: the deleted helper forwards once to checked-in `Option.Exists` and owns no policy, evidence, or fault. Both rows retain unknown-kind-false behavior.

Ripples: none.

## 5. Admit bounds once through `Op.AcceptInput`

Location: `Kind.NativeBounds` and `Normalization.BoundsOf`.

From:

```csharp
private static Fin<BoundingBox> NativeBounds(object value, Op key) =>
    guard(((GeometryBase)value).IsValid, key.InvalidInput()).ToFin().Map(_ => ((GeometryBase)value).GetBoundingBox(accurate: true));
```

To:

```csharp
private static Fin<BoundingBox> NativeBounds(object value, Op _) =>
    Fin.Succ(((GeometryBase)value).GetBoundingBox(accurate: true));
```

From:

```csharp
public Fin<BoundingBox> BoundsOf(Op key) =>
    Optional(geometry).ToFin(key.InvalidInput()).Bind(source =>
        guard(OpAcceptance.ValidityOf(source: source).Exists(static valid => valid), key.InvalidInput()).ToFin().Bind(_ => Kind.Of(type: source.GetType()).Case switch {
            Kind kind => kind.Bounds(value: source, key: key),
            _ => source is GeometryBase native
                ? guard(native.IsValid, key.InvalidInput()).ToFin().Map(_ => native.GetBoundingBox(accurate: true))
                : Fin.Fail<BoundingBox>(key.Unsupported(inputType: source.GetType(), outputType: typeof(BoundingBox))),
        }));
```

To:

```csharp
public Fin<BoundingBox> BoundsOf(Op key) =>
    Optional(geometry).ToFin(key.InvalidInput()).Bind(source =>
        key.AcceptInput(value: source).Bind(admitted => Kind.Of(type: admitted.GetType()).Case switch {
            Kind kind => kind.Bounds(value: admitted, key: key),
            _ => admitted is GeometryBase native
                ? Fin.Succ(native.GetBoundingBox(accurate: true))
                : Fin.Fail<BoundingBox>(key.Unsupported(inputType: admitted.GetType(), outputType: typeof(BoundingBox))),
        }));
```

Effect: fenced LOC `0`; validity predicates on a successful native path `2 -> 1`; direct oracle reads outside its gate `1 -> 0`.

API/consumer proof: `AcceptInput` is the local canonical `InvalidInput` projection over `OpAcceptance.ValidityOf`. `Kind.Bounds` has no consumer other than `BoundsOf`, and the local RhinoCommon catalogue exposes `GeometryBase.GetBoundingBox(bool)`.

Ripples: none; all `BoundsOf` consumers retain `Fin<BoundingBox>`.

## 6. Return total curve classification without an invented `Fin`

Location: `Normalization.CurveFormOf`; consumers `Analysis/query.md` and `Analysis/select.md`.

From:

```csharp
internal static Fin<CurveForm> CurveFormOf(Curve curve, Context context) =>
    Fin.Succ(AnalyticForm.Lane(topology: Topology.Curve)
```

```csharp
                Dimension: curve.Dimension)));
```

To:

```csharp
internal static CurveForm CurveFormOf(Curve curve, Context context) =>
    AnalyticForm.Lane(topology: Topology.Curve)
```

```csharp
                Dimension: curve.Dimension));
```

From, the two callers:

```csharp
projection.As<Curve>(key: op).Bind(curve => Normalization.CurveFormOf(curve: curve, context: context));
Normalization.CurveForm(source: geometry, key: op).Bind(lease => lease.Use(curve => Normalization.CurveFormOf(curve: curve, context: context)))
```

To:

```csharp
projection.As<Curve>(key: op).Map(curve => Normalization.CurveFormOf(curve: curve, context: context));
Normalization.CurveForm(source: geometry, key: op).Map(lease => lease.Use(curve => Normalization.CurveFormOf(curve: curve, context: context)))
```

Effect: fenced LOC `0`; result constructions `-1`; dependent result binds `-2`.

API/consumer proof: every analytic miss deterministically constructs `CurveForm.NurbsCase`; the method has no failure arm. The two consumers already own the only fallible steps, so checked-in `Fin.Map` is the exact projection.

Ripples: change the two caller lines above in the same implementation pass.

## 7. Inline the sole curve-case classifier

Location: the `Classified` call inside `CurveFormOf` and `Normalization.Classified`; apply after move 6.

From:

```csharp
.Bind(primitive => Classified(primitive: primitive, closed: curve.IsClosed))
```

```csharp
private static Option<CurveForm> Classified(object primitive, bool closed) =>
    primitive switch {
        Line line => Some<CurveForm>(new CurveForm.LineCase(Value: line)),
        Circle circle => Some<CurveForm>(new CurveForm.CircleCase(Value: circle)),
        Arc arc => Some<CurveForm>(new CurveForm.ArcCase(Value: arc)),
        Ellipse ellipse => Some<CurveForm>(new CurveForm.EllipseCase(Value: ellipse)),
        Polyline polyline => Some<CurveForm>(new CurveForm.PolylineCase(Value: polyline, IsClosed: closed)),
        _ => Option<CurveForm>.None,
    };
```

To:

```csharp
.Bind(primitive => primitive switch {
    Line line => Some<CurveForm>(new CurveForm.LineCase(Value: line)),
    Circle circle => Some<CurveForm>(new CurveForm.CircleCase(Value: circle)),
    Arc arc => Some<CurveForm>(new CurveForm.ArcCase(Value: arc)),
    Ellipse ellipse => Some<CurveForm>(new CurveForm.EllipseCase(Value: ellipse)),
    Polyline polyline => Some<CurveForm>(new CurveForm.PolylineCase(Value: polyline, IsClosed: curve.IsClosed)),
    _ => Option<CurveForm>.None,
})
```

Effect: fenced LOC `10 -> 8` (`-2`); type-member symbols `-1`.

API/consumer proof: the switch is the helper's sole call site and keeps the original curve's `IsClosed` evidence exactly; no inference source changes and the fallback remains `None`.

Ripples: none outside the target.

## 8. Use the shared admission and rollback algebra for projection construction

Location: `TopologyProjection.Admitted`.

From:

```csharp
private static Fin<TopologyProjection> Admitted(Lease<GeometryBase> value, ComponentIndex source, bool reversed, bool detachedSingleFace) {
    TopologyProjection projection = new(value: value, source: source, reversed: reversed, detachedSingleFace: detachedSingleFace);
    if (projection.IsValid) { return Fin.Succ(projection); }
    projection.Dispose();
    return Fin.Fail<TopologyProjection>(Key.InvalidInput());
}
```

To:

```csharp
private static Fin<TopologyProjection> Admitted(Lease<GeometryBase> value, ComponentIndex source, bool reversed, bool detachedSingleFace) {
    TopologyProjection projection = new(value: value, source: source, reversed: reversed, detachedSingleFace: detachedSingleFace);
    return Key.AcceptInput(value: projection).Rollback(projection);
}
```

Effect: fenced LOC `6 -> 4` (`-2`); imperative result branches `2 -> 0`.

API/consumer proof: `TopologyProjection : IValidityEvidence`, so `AcceptInput` reaches the same `IsValid` oracle and preserves `InvalidInput`. The local `Custody.Rollback(params ReadOnlySpan<IDisposable?>)` releases only a failed fold and aggregates a cleanup fault with the primary failure. Every factory and `DetachFrom` already converges here.

Ripples: none.

## 9. Read the cached face lease through its proven `Option` case

Location: `TopologyProjection.FaceBrep`.

From:

```csharp
private Option<Brep> FaceBrep(BrepFace face) {
    Option<Brep> held = faceBrep.Map(static lease => lease.Resource);
    if (held.IsSome) { return held; }
    Option<Brep> minted = Optional(face.DuplicateFace(duplicateMeshes: false));
    faceBrep = minted.Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep));
    return minted;
}
```

To:

```csharp
private Option<Brep> FaceBrep(BrepFace face) {
    if (faceBrep is { IsSome: true, Case: Lease<Brep> held }) { return Some(held.Resource); }
    faceBrep = Optional(face.DuplicateFace(duplicateMeshes: false)).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep));
    return faceBrep.Map(static lease => lease.Resource);
}
```

Effect: fenced LOC `7 -> 5` (`-2`); method-local symbols `2 -> 1`.

API/consumer proof: the checked-in LanguageExt catalogue permits `Option.Case` only under the `IsSome` proof. A failed duplicate remains `None`; a successful duplicate stays in `Lease<Brep>.Owned` until carrier disposal.

Ripples: none.

## 10. Make transfer identity exact and internal

Location: `TopologyProjection.Transfers` and `TopologyProjection.SameAs`; apply before move 11.

From:

```csharp
public bool Transfers(object? output) =>
    Optional(output).Exists(present => present switch {
        TopologyProjection projection => SameAs(other: projection),
        GeometryBase geometry => ReferenceEquals(objA: Value, objB: geometry) || (Value, geometry) switch {
            (Brep brep, BrepFace face) => ReferenceEquals(objA: brep, objB: face.Brep),
            (BrepFace face, Brep brep) => ReferenceEquals(objA: face.Brep, objB: brep),
            (BrepFace source, BrepFace face) => ReferenceEquals(objA: source.Brep, objB: face.Brep),
            _ => false,
        },
        _ => false,
    });
private bool SameAs(TopologyProjection? other) =>
    other switch { TopologyProjection p => ReferenceEquals(objA: Value, objB: p.Value) && Source.Equals(p.Source), _ => false };
```

To:

```csharp
private bool Transfers(object? output) =>
    output switch {
        TopologyProjection projection => ReferenceEquals(objA: Value, objB: projection.Value) && Source.Equals(projection.Source),
        GeometryBase geometry => ReferenceEquals(objA: Value, objB: geometry)
            || faceBrep.Exists(held => ReferenceEquals(objA: held.Resource, objB: geometry))
            || (Value, geometry) switch {
                (Brep brep, BrepFace face) => ReferenceEquals(objA: brep, objB: face.Brep),
                (BrepFace face, Brep brep) => ReferenceEquals(objA: face.Brep, objB: brep),
                (BrepFace source, BrepFace face) => ReferenceEquals(objA: source.Brep, objB: face.Brep),
                _ => false,
            },
        _ => false,
    };
```

Effect: fenced LOC `0`; type-member symbols `-1`; public members `-1`.

API/consumer proof: a nullable switch already maps null to false, and `SameAs` has no remaining caller after inlining. `As<Brep>()` on a `BrepFace` can return the cached duplicate rather than `face.Brep`; the `faceBrep.Exists` arm is therefore required for exact transfer identity. No external consumer calls `Transfers`.

Ripples: none.

## 11. Dispose against actual returned values; delete the type-level proxy

Location: `TopologyProjection.Yields` and `TopologyProjection.Project`; apply after move 10.

### 11a. Delete the proxy

From:

```csharp
public bool Yields(Type outputType) {
    ArgumentNullException.ThrowIfNull(argument: outputType);
    return outputType.IsAssignableFrom(typeof(TopologyProjection))
        || (Value is Curve curve && outputType.IsInstanceOfType(curve))
        || (Value is Brep or BrepFace && outputType.IsAssignableFrom(typeof(Brep)));
}
```

To:

```csharp
// deleted
```

### 11b. Inspect the successful values

From:

```csharp
internal static Fin<Seq<TValue>> Project<TValue>(Seq<TopologyProjection> all, Seq<TopologyProjection> chosen, Func<Seq<TopologyProjection>, Fin<Seq<TValue>>> project) {
    Fin<Seq<TValue>> result = Key.Catch(body: () => project(arg: chosen));
    _ = all.Filter(v => !result.IsSucc || !chosen.Exists(c => c.SameAs(other: v) && c.Yields(outputType: typeof(TValue)))).Iter(static v => v.Dispose());
    return result;
}
```

To:

```csharp
internal static Fin<Seq<TValue>> Project<TValue>(Seq<TopologyProjection> all, Seq<TopologyProjection> chosen, Func<Seq<TopologyProjection>, Fin<Seq<TValue>>> project) {
    Fin<Seq<TValue>> result = Key.Catch(body: () => project(arg: chosen));
    _ = all.Filter(item => !result.Exists(values => values.Exists(output => item.Transfers(output)))).Iter(static item => item.Dispose());
    return result;
}
```

Effect: fenced LOC `11 -> 5` (`-6`); type-member symbols `-1`; public members `-1`.

API/consumer proof: checked-in `Fin.Exists` is false on failure, so every projection still releases on failure. On success, `Seq.Exists` preserves only a projection whose exact identity occurs in returned values. The two `Analysis/select.md` call sites return direct `Curve`, `Brep`, `TopologyProjection`, or non-transferring derived values; move 10 covers every direct transfer relation, including the cached face duplicate.

Ripples: none.

## Protected non-moves

- Keep `AnalyticForm` keyless. Re-keying it by `Kind` would add generated lookup storage and a broader generated smart-enum surface to save one small scan; it is not a net surface reduction.
- Keep `Kind.ByType`, `Kind.ByObjectType`, and `Kind.InheritsBase`. They preserve nearest registered base resolution and O(1) exact/native lookup; an `Items.Find(IsAssignableFrom)` rewrite makes classification order-sensitive and linear.
- Keep `PrimitiveOf`, `Kind.NativeBounds`, `Kind.SolidBounds`, `AnalyticForm.Face`, `Normalization.Owned`, `Normalization.Cast`, and the three `CurveForm`/`SurfaceForm`/`BrepForm` entries. Each owns heterogeneous conversion or lifetime semantics; generic collapse would move the same branches behind `typeof` tests without deleting logic.
- Keep `CurveForm.IsClosed`. It is the union's universal evidence column, not a duplicated capability or a consumer-count convenience.
- Keep `Transfers`; move 11 makes it the exact cleanup predicate. Deleting it would leave the type-level `Yields` approximation in custody policy.

## Net effect

Applying all eleven moves yields target fenced LOC `-20`, cross-file fenced LOC `-1`, type-member symbols `-11`, public members `-5`, smart-enum rows `-2`, lazy caches `-3`, eleven fewer mirrored memberships, one fewer invented result construction, and two fewer dependent result binds. Cross-file code ripples are limited to deleting `Requirement.ForKind`'s `unknown:` arm and changing the two `CurveFormOf` consumers from `Bind` to `Map`.
