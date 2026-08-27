# 1. Keep topology classification typed

`inspect.md:28,130-138` — the culture import and string-output arm of `Topologies.Classify`.

From

```csharp
using System.Globalization;
Type t when t == typeof(string) => Lift<TGeometry, string, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.ToString(format: null, formatProvider: CultureInfo.InvariantCulture))), requiresContext: true).As<TGeometry, TOut>(key: key),
```

To

```csharp
// System.Globalization DELETED
// string classification arm DELETED
```

Why

`Kind` and `Topology` already carry the classification domain. Formatting either into an invariant string erases that type and creates a second output vocabulary.

Change

Delete the string projection and its now-unused import; callers choose `Kind` or `Topology` and format only at a host boundary.

Delta

-2 LOC; neutral module-level type/member count.

# 2. Remove topology scalar construction forwarders

`inspect.md:53-60` — the scalar shortcut members on `Topologies`.

From

```csharp
public static Topologies Manifold => new ScalarCase(Scalar: TopologyScalar.Manifold);
public static Topologies Euler => new ScalarCase(Scalar: TopologyScalar.Euler);
public static Topologies BoundaryLoops => new ScalarCase(Scalar: TopologyScalar.BoundaryLoops);
public static Topologies Genus => new ScalarCase(Scalar: TopologyScalar.Genus);
public static Topologies HoleCount => new ScalarCase(Scalar: TopologyScalar.HoleCount);
public static Topologies FaceCount => new ScalarCase(Scalar: TopologyScalar.FaceCount);
public static Topologies EdgeCount => new ScalarCase(Scalar: TopologyScalar.EdgeCount);
public static Topologies VertexCount => new ScalarCase(Scalar: TopologyScalar.VertexCount);
```

To

```csharp
// Topologies.Manifold DELETED
// Topologies.Euler DELETED
// Topologies.BoundaryLoops DELETED
// Topologies.Genus DELETED
// Topologies.HoleCount DELETED
// Topologies.FaceCount DELETED
// Topologies.EdgeCount DELETED
// Topologies.VertexCount DELETED
```

Why

The eight members only rename public `ScalarCase` construction. Replacing them with one parameterized factory would retain the same unnecessary construction layer under a new name.

Change

Construct `new Topologies.ScalarCase(Scalar: row)` directly at call sites and delete the complete shortcut roster.

Delta

-8 LOC and -8 module-level members.

# 3. Derive topology case operation keys

`inspect.md:39-47,61-63` — the topology operation cases and reflection-keyed operation cache.

From

```csharp
[Union]
public abstract partial record Topologies {
    public sealed record KindCase : Topologies;
    public sealed record DomainsCase : Topologies;
    public sealed record SolidOrientationCase : Topologies;
    public sealed record ComponentsCase : Topologies;
    public sealed record ContainsPointCase(Point3d Point) : Topologies;
    public sealed record ScalarCase(TopologyScalar Scalar) : Topologies;
    internal Op Key => Keys.Value[GetType()];
    private static readonly Lazy<FrozenDictionary<Type, Op>> Keys = new(static () =>
        typeof(Topologies).GetNestedTypes().ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Name)));
```

To

```csharp
[Union]
public abstract partial record Topologies {
    public sealed record KindCase : Topologies;
    public sealed record DomainsCase : Topologies;
    public sealed record SolidOrientationCase : Topologies;
    public sealed record ComponentsCase : Topologies;
    public sealed record ContainsPointCase(Point3d Point) : Topologies;
    public sealed record ScalarCase(TopologyScalar Scalar) : Topologies;
    internal Op Key => Switch(
        kindCase: static _ => Op.Of(name: nameof(KindCase)),
        domainsCase: static _ => Op.Of(name: nameof(DomainsCase)),
        solidOrientationCase: static _ => Op.Of(name: nameof(SolidOrientationCase)),
        componentsCase: static _ => Op.Of(name: nameof(ComponentsCase)),
        containsPointCase: static _ => Op.Of(name: nameof(ContainsPointCase)),
        scalarCase: static _ => Op.Of(name: nameof(ScalarCase)));
// Keys DELETED
```

Why

The generated exhaustive `Switch` already identifies the active case. Reflecting all nested runtime types builds a second case mechanism, includes non-case nested types, and makes trimming determine whether lookup succeeds; generating a separate `SelfOp` member per case would replace one cache with six new symbols.

Change

Dispatch the active case through the generated `Switch`, derive its operation name from that case symbol, and delete the reflection cache without adding per-case members.

Delta

+4 LOC and -1 module-level member; generated member count and type count neutral.

# 4. Inline the classification builder

`inspect.md:66,130-138` — the `KindCase` dispatch arm and its single-caller `Classify` helper.

From

```csharp
kindCase: static (key, _) => Classify<TGeometry, TOut>(key: key),
private static Operation<TGeometry, TOut> Classify<TGeometry, TOut>(Op key) where TGeometry : notnull =>
    (Capability.Universal(type: typeof(TGeometry)) || Rasm.Domain.Kind.Of(type: typeof(TGeometry)).IsSome)
        ? typeof(TOut) switch {
            Type t when t == typeof(Kind) => Lift<TGeometry, Kind, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k)), requiresContext: true).As<TGeometry, TOut>(key: key),
            Type t when t == typeof(Topology) => Lift<TGeometry, Topology, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.Topology)), requiresContext: true).As<TGeometry, TOut>(key: key),
            _ => key.Unsupported<TGeometry, TOut>(),
        }
        : key.Unsupported<TGeometry, TOut>();
```

To

```csharp
kindCase: static (key, _) =>
    (Capability.Universal(type: typeof(TGeometry)) || Rasm.Domain.Kind.Of(type: typeof(TGeometry)).IsSome)
        ? typeof(TOut) switch {
            Type t when t == typeof(Kind) => Lift<TGeometry, Kind, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k)), requiresContext: true).As<TGeometry, TOut>(key: key),
            Type t when t == typeof(Topology) => Lift<TGeometry, Topology, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.Topology)), requiresContext: true).As<TGeometry, TOut>(key: key),
            _ => key.Unsupported<TGeometry, TOut>(),
        }
        : key.Unsupported<TGeometry, TOut>(),
// Classify DELETED
```

Why

`Classify` has one caller and owns no independent invariant; it only pushes the dispatch arm one hop away from its body.

Change

Place the typed output selection directly on the `KindCase` arm and delete the helper.

Delta

-1 LOC and -1 module-level member.

# 5. Inline domain and orientation builders

`inspect.md:67-68,80-87,139-146` — the domain/orientation dispatch arms and their one-call builder chain.

From

```csharp
domainsCase: static (key, _) => Spans<TGeometry, TOut>(key: key),
solidOrientationCase: static (key, _) => Orientation<TGeometry, TOut>(key: key),
internal static Fin<BrepSolidOrientation> SolidOrientationOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
    OnGeometry(geometry: geometry, op: op,
        onMesh: mesh => Fin.Succ(mesh.SolidOrientation() switch {
            1 => BrepSolidOrientation.Outward,
            -1 => BrepSolidOrientation.Inward,
            _ => BrepSolidOrientation.None,
        }),
        onBrep: brep => Fin.Succ(brep.SolidOrientation));
private static Operation<TGeometry, TOut> Spans<TGeometry, TOut>(Op key) where TGeometry : notnull =>
    typeof(TOut) == typeof(Interval) && (Capability.CurveForm.Admits(type: typeof(TGeometry)) || Capability.SurfaceForm.Admits(type: typeof(TGeometry)))
        ? Lift<TGeometry, Interval, Op>(key: key, state: key, extract: static (op, g, _) => DomainsOf(geometry: g, op: op).Bind(domains => op.Accept(values: domains))).As<TGeometry, TOut>(key: key)
        : key.Unsupported<TGeometry, TOut>();
private static Operation<TGeometry, TOut> Orientation<TGeometry, TOut>(Op key) where TGeometry : notnull =>
    typeof(TOut) == typeof(BrepSolidOrientation) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
        ? Lift<TGeometry, BrepSolidOrientation, Op>(key: key, state: key, extract: static (op, g, _) => SolidOrientationOf(geometry: g, op: op).Bind(orientation => op.Accept(value: orientation))).As<TGeometry, TOut>(key: key)
        : key.Unsupported<TGeometry, TOut>();
```

To

```csharp
domainsCase: static (key, _) => typeof(TOut) == typeof(Interval) && (Capability.CurveForm.Admits(type: typeof(TGeometry)) || Capability.SurfaceForm.Admits(type: typeof(TGeometry)))
    ? Lift<TGeometry, Interval, Op>(key: key, state: key, extract: static (op, g, _) => DomainsOf(geometry: g, op: op).Bind(domains => op.Accept(values: domains))).As<TGeometry, TOut>(key: key)
    : key.Unsupported<TGeometry, TOut>(),
solidOrientationCase: static (key, _) => typeof(TOut) == typeof(BrepSolidOrientation) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
    ? Lift<TGeometry, BrepSolidOrientation, Op>(key: key, state: key, extract: static (op, g, _) => OnGeometry(geometry: g, op: op,
        onMesh: mesh => Fin.Succ(mesh.SolidOrientation() switch { 1 => BrepSolidOrientation.Outward, -1 => BrepSolidOrientation.Inward, _ => BrepSolidOrientation.None }),
        onBrep: brep => Fin.Succ(brep.SolidOrientation)).Bind(orientation => op.Accept(value: orientation))).As<TGeometry, TOut>(key: key)
    : key.Unsupported<TGeometry, TOut>(),
// SolidOrientationOf DELETED
// Spans DELETED
// Orientation DELETED
```

Why

Each builder is called by exactly one union arm, and `SolidOrientationOf` is then called only by its builder. None owns reusable domain meaning outside that arm.

Change

Keep `DomainsOf` because `Analysis/select` consumes it directly; inline the remaining build and orientation chain into generated dispatch.

Delta

-9 LOC and -3 module-level members.

# 6. Inline component output selection

`inspect.md:69,147-153` — the `ComponentsCase` arm and its `Pieces`/`Decomposes` indirection.

From

```csharp
componentsCase: static (key, _) => Pieces<TGeometry, TOut>(key: key),
private static Operation<TGeometry, TOut> Pieces<TGeometry, TOut>(Op key) where TGeometry : notnull =>
    Decomposes<TGeometry, TOut>()
        ? Lift<TGeometry, TOut, Op>(key: key, state: key, extract: static (op, g, _) => ComponentsOf(geometry: g, op: op).Bind(components => ProjectPieces<TOut>(components: components, op: op))).As<TGeometry, TOut>(key: key)
        : key.Unsupported<TGeometry, TOut>();
private static bool Decomposes<TGeometry, TOut>() =>
    (typeof(TOut) == typeof(Brep) || typeof(TOut) == typeof(Mesh))
    && (Capability.Universal(type: typeof(TGeometry)) || typeof(TOut).IsAssignableFrom(c: typeof(TGeometry)));
```

To

```csharp
componentsCase: static (key, _) =>
    (typeof(TOut) == typeof(Brep) || typeof(TOut) == typeof(Mesh))
    && (Capability.Universal(type: typeof(TGeometry)) || typeof(TOut).IsAssignableFrom(c: typeof(TGeometry)))
        ? Lift<TGeometry, TOut, Op>(key: key, state: key, extract: static (op, g, _) => ComponentsOf(geometry: g, op: op).Bind(components => ProjectPieces<TOut>(components: components, op: op))).As<TGeometry, TOut>(key: key)
        : key.Unsupported<TGeometry, TOut>(),
// Pieces DELETED
// Decomposes DELETED
```

Why

The predicate exists only to select this arm and the builder exists only to call it; separating them obscures the output condition without adding reuse.

Change

Place the output/capability predicate and operation construction directly on `ComponentsCase`.

Delta

-3 LOC and -2 module-level members.

# 7. Admit containment once

`inspect.md:70,88-93,154-158` — the containment arm, evaluator helper, and repeated point check.

From

```csharp
containsPointCase: static (key, cp) => Inside<TGeometry, TOut>(point: cp.Point, key: key),
internal static Fin<bool> ContainsPoint<TGeometry>(TGeometry geometry, Point3d target, Context context, Op op) where TGeometry : notnull =>
    from _ in guard(ValidityClaim.Finite(target).Holds, op.InvalidInput())
    from contained in OnGeometry(geometry: geometry, op: op,
        onMesh: mesh => Fin.Succ(mesh.IsPointInside(point: target, tolerance: context.For(lane: ToleranceLane.Distance).Value, strictlyIn: false)),
        onBrep: brep => Fin.Succ(brep.IsPointInside(point: target, tolerance: context.For(lane: ToleranceLane.Distance).Value, strictlyIn: false)))
    select contained;
private static Operation<TGeometry, TOut> Inside<TGeometry, TOut>(Point3d point, Op key) where TGeometry : notnull =>
    ValidityClaim.Finite(point).Holds && typeof(TOut) == typeof(bool) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
        ? Lift<TGeometry, bool, (Op Key, Point3d Target)>(key: key, state: (Key: key, Target: point), requirement: Some(Requirement.SolidTopology),
            extract: static (s, g, ctx) => ContainsPoint(geometry: g, target: s.Target, context: ctx, op: s.Key).Bind(contained => s.Key.Accept(value: contained))).As<TGeometry, TOut>(key: key)
        : key.Unsupported<TGeometry, TOut>();
```

To

```csharp
containsPointCase: static (key, cp) =>
    ValidityClaim.Finite(cp.Point).Holds && typeof(TOut) == typeof(bool) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
        ? Lift<TGeometry, bool, (Op Key, Point3d Target)>(key: key, state: (Key: key, Target: cp.Point), requirement: Some(Requirement.SolidTopology),
            extract: static (s, g, ctx) => OnGeometry(geometry: g, op: s.Key,
                onMesh: mesh => Fin.Succ(mesh.IsPointInside(point: s.Target, tolerance: ctx.For(lane: ToleranceLane.Distance).Value, strictlyIn: false)),
                onBrep: brep => Fin.Succ(brep.IsPointInside(point: s.Target, tolerance: ctx.For(lane: ToleranceLane.Distance).Value, strictlyIn: false)))
                .Bind(contained => s.Key.Accept(value: contained))).As<TGeometry, TOut>(key: key)
        : key.Unsupported<TGeometry, TOut>(),
// ContainsPoint DELETED
// Inside DELETED
```

Why

The point is immutable captured state and is already screened before the operation is built. Rechecking it during every evaluation is double admission, while both helpers have one caller.

Change

Validate the point once at construction, inline the mesh/brep evaluator into the arm, and delete both helpers.

Delta

-4 LOC and -2 module-level members.

# 8. Inline scalar operation construction

`inspect.md:71,159-164` — the `ScalarCase` arm and its single-call builder.

From

```csharp
scalarCase: static (_, scalar) => Scalar<TGeometry, TOut>(scalar: scalar.Scalar));
private static Operation<TGeometry, TOut> Scalar<TGeometry, TOut>(TopologyScalar scalar) where TGeometry : notnull =>
    scalar.Output.Serves<TOut>() && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
        ? Lift<TGeometry, TOut, TopologyScalar>(key: scalar.Op, state: scalar,
            extract: static (row, g, _) => OnGeometry(geometry: g, op: row.Op, onAny: native => row.Extract(geometry: native, op: row.Op))
                .Bind(value => row.Output.Admit<TOut>(values: Seq(value.Boxed), key: row.Op)))
        : scalar.Op.Unsupported<TGeometry, TOut>();
```

To

```csharp
scalarCase: static (_, scalar) => {
    Op key = Op.Of(name: scalar.Scalar.Key);
    return scalar.Scalar.Output.Serves<TOut>() && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
        ? Lift<TGeometry, TOut, (TopologyScalar Row, Op Key)>(key: key, state: (scalar.Scalar, key),
            extract: static (state, g, _) => OnGeometry(geometry: g, op: state.Key, onAny: native => state.Row.Extract(geometry: native, op: state.Key))
                .Bind(value => state.Row.Output.Admit<TOut>(values: Seq(value.Boxed), key: state.Key)))
        : key.Unsupported<TGeometry, TOut>();
});
// Scalar operation builder DELETED
```

Why

The builder is a one-call forwarder from the union arm. The row already owns a generated string key, so operation identity is derived at use rather than stored in a parallel column.

Change

Build directly from the carried row on `ScalarCase` and derive its `Op` from the generated key.

Delta

-1 implementation LOC and -1 module-level member.

# 9. Use topology row keys as operation identity

`inspect.md:222-237` — `TopologyScalar` declaration, row construction, and label-backed key cache.

From

```csharp
[SmartEnum<int>]
public static readonly TopologyScalar Manifold = new(key: 0, label: nameof(Manifold), output: OutputBinding.Of<bool>(), extract: static (g, op) => Topologies.ManifoldOf(geometry: g, op: op).Map(MeasuredValue.Flag));
public static readonly TopologyScalar Euler = new(key: 1, label: nameof(Euler), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.EulerOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar BoundaryLoops = new(key: 2, label: nameof(BoundaryLoops), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.BoundaryLoopsOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar Genus = new(key: 3, label: nameof(Genus), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.GenusOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar HoleCount = new(key: 4, label: nameof(HoleCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.HoleCountOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar FaceCount = new(key: 5, label: nameof(FaceCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.Faces.Count, brepCount: static b => b.Faces.Count).Map(MeasuredValue.Count));
public static readonly TopologyScalar EdgeCount = new(key: 6, label: nameof(EdgeCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.TopologyEdges.Count, brepCount: static b => b.Edges.Count).Map(MeasuredValue.Count));
public static readonly TopologyScalar VertexCount = new(key: 7, label: nameof(VertexCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.Vertices.Count, brepCount: static b => b.Vertices.Count).Map(MeasuredValue.Count));
public string Label { get; }
internal Op Op => Keys.Value[this];
private static readonly Lazy<FrozenDictionary<TopologyScalar, Op>> Keys = new(static () =>
    Items.ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Label)));
```

To

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public static readonly TopologyScalar Manifold = new(key: nameof(Manifold), output: OutputBinding.Of<bool>(), extract: static (g, op) => Topologies.ManifoldOf(geometry: g, op: op).Map(MeasuredValue.Flag));
public static readonly TopologyScalar Euler = new(key: nameof(Euler), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.EulerOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar BoundaryLoops = new(key: nameof(BoundaryLoops), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.BoundaryLoopsOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar Genus = new(key: nameof(Genus), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.GenusOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar HoleCount = new(key: nameof(HoleCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.HoleCountOf(geometry: g, op: op).Map(MeasuredValue.Count));
public static readonly TopologyScalar FaceCount = new(key: nameof(FaceCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.Faces.Count, brepCount: static b => b.Faces.Count).Map(MeasuredValue.Count));
public static readonly TopologyScalar EdgeCount = new(key: nameof(EdgeCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.TopologyEdges.Count, brepCount: static b => b.Edges.Count).Map(MeasuredValue.Count));
public static readonly TopologyScalar VertexCount = new(key: nameof(VertexCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.Vertices.Count, brepCount: static b => b.Vertices.Count).Map(MeasuredValue.Count));
// Label DELETED
// TopologyScalar.Op DELETED
// Keys DELETED
```

Why

The ordinal never drives behavior, while `Label`, `Op`, and the frozen dictionary are parallel representations of the same operation name. Thinktecture's generated string key is the one identity needed by lookup and direct `Op.Of(row.Key)` derivation.

Change

Use the real operation name as the Thinktecture key and delete the label, stored operation column, and lazy operation mirror.

Delta

-3 LOC and -3 declared module-level members; generated keyed lookup remains owned by Thinktecture.

Ripples

`inspect.md:15,593-594` describes `TopologyScalar` as a string-keyed behavior roster whose generated key supplies operation identity and reduces its row count after the `HoleCount` deletion below.

# 10. Remove mesh sample construction forwarders

`inspect.md:291-295` — fixed sample shortcuts and optional face-metric defaulting on `Meshes`.

From

```csharp
public static Meshes Validity => new SamplesCase(Group: MeshSampleGroup.Validity);
public static Meshes Counts => new SamplesCase(Group: MeshSampleGroup.Count);
public static Meshes Defects => new SamplesCase(Group: MeshSampleGroup.Defect);
public static Meshes Quality => new SamplesCase(Group: MeshSampleGroup.Quality);
public static Meshes FaceQuality(Option<MeshMetric> metric = default) => new FaceQualityCase(Metric: metric.IfNone(MeshMetric.EdgeAspect));
```

To

```csharp
// Meshes.Validity DELETED
// Meshes.Counts DELETED
// Meshes.Defects DELETED
// Meshes.Quality DELETED
// Meshes.FaceQuality DELETED
```

Why

The public cases already carry the sample band and metric. Consolidating the aliases into parameterized factories would preserve an unnecessary forwarding layer, while the absent metric default is undeclared policy.

Change

Construct `SamplesCase` and `FaceQualityCase` directly, requiring the selected metric explicitly.

Delta

-5 LOC and -5 module-level members.

Ripples

`libs/dotnet/Rasm.Fabrication/.planning/Spec/manufacturability.md:1338` changes `Meshes.Defects` to `new Meshes.SamplesCase(Group: MeshSampleGroup.Defect)`. `inspect.md:244-246` removes the factory and absent-metric-default claims; no direct `FaceQuality` construction exists in the current planning corpus.

# 11. Derive mesh case operation keys

`inspect.md:278-290` — the mesh operation cases and reflection-keyed operation cache.

From

```csharp
[Union]
public abstract partial record Meshes {
    public sealed record SamplesCase(MeshSampleGroup Group) : Meshes;
    public sealed record FaceQualityCase(MeshMetric Metric) : Meshes;
    public sealed record FaceShapeCase : Meshes;
    public sealed record AtVisiblePolygonCase(Option<int> Value) : Meshes;
    public sealed record VisiblePolygonCountCase : Meshes;
    public sealed record NakedEdgesCase : Meshes;
    public sealed record OutlineCase(Plane Plane) : Meshes;
    internal Op Key => Keys.Value[GetType()];
    private static readonly Lazy<FrozenDictionary<Type, Op>> Keys = new(static () =>
        typeof(Meshes).GetNestedTypes().ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Name)));
```

To

```csharp
[Union]
public abstract partial record Meshes {
    public sealed record SamplesCase(MeshSampleGroup Group) : Meshes;
    public sealed record FaceQualityCase(MeshMetric Metric) : Meshes;
    public sealed record FaceShapeCase : Meshes;
    public sealed record AtVisiblePolygonCase(Option<int> Value) : Meshes;
    public sealed record VisiblePolygonCountCase : Meshes;
    public sealed record NakedEdgesCase : Meshes;
    public sealed record OutlineCase(Plane Plane) : Meshes;
    internal Op Key => Switch(
        samplesCase: static _ => Op.Of(name: nameof(SamplesCase)),
        faceQualityCase: static _ => Op.Of(name: nameof(FaceQualityCase)),
        faceShapeCase: static _ => Op.Of(name: nameof(FaceShapeCase)),
        atVisiblePolygonCase: static _ => Op.Of(name: nameof(AtVisiblePolygonCase)),
        visiblePolygonCountCase: static _ => Op.Of(name: nameof(VisiblePolygonCountCase)),
        nakedEdgesCase: static _ => Op.Of(name: nameof(NakedEdgesCase)),
        outlineCase: static _ => Op.Of(name: nameof(OutlineCase)));
// Keys DELETED
```

Why

The generated exhaustive `Switch` already owns case discrimination. Runtime reflection duplicates that correspondence and admits nested non-cases into the cache; generating a `SelfOp` member per case would add seven symbols where direct derivation needs none.

Change

Dispatch through the generated `Switch`, derive the operation name from each typed case symbol, and delete the reflection cache without adding per-case members.

Delta

+6 LOC and -1 module-level member; generated member count and type count neutral.

# 12. Collapse mesh census rosters

`inspect.md:27` — the frozen-collection import after all mirrored dictionaries are deleted.

From

```csharp
using System.Collections.Frozen;
```

To

```csharp
// System.Collections.Frozen DELETED
```

`inspect.md:271-276` — the two-row capture vocabulary.

From

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CensusSource {
    public static readonly CensusSource Native = new(key: "native", capture: static _ => Fin.Succ(MeshCheckParameters.Defaults()));
    public static readonly CensusSource Checked = new(key: "checked", capture: static mesh => Requirement.MeshReport(mesh: mesh, check: Checked.Key));
    [UseDelegateFromConstructor] internal partial Fin<MeshCheckParameters> Capture(Mesh mesh);
}
```

To

```csharp
// CensusSource DELETED
```

`inspect.md:348-368` — the group rows, columns, and census capture call.

From

```csharp
[SmartEnum<int>]
public static readonly MeshSampleGroup Validity = new(key: 1, decade: 0, label: nameof(Validity), source: CensusSource.Native);
public static readonly MeshSampleGroup Count = new(key: 2, decade: 1, label: nameof(Count), source: CensusSource.Native);
public static readonly MeshSampleGroup Defect = new(key: 3, decade: 2, label: nameof(Defect), source: CensusSource.Checked);
public static readonly MeshSampleGroup Quality = new(key: 4, decade: 4, label: nameof(Quality), source: CensusSource.Native);
public string Label { get; }
internal int Decade { get; }
internal CensusSource Source { get; }
internal static MeshSampleGroup OfDecade(int decade) => Decades.Value[decade];
private static readonly Lazy<FrozenDictionary<int, MeshSampleGroup>> Decades = new(static () =>
    Items.ToFrozenDictionary(static row => row.Decade, static row => row));
internal Seq<MeshSampleKind> Kinds => Bands.Value[this];
private static readonly Lazy<FrozenDictionary<MeshSampleGroup, Seq<MeshSampleKind>>> Bands = new(static () =>
    Items.ToFrozenDictionary(static row => row, static row => toSeq(MeshSampleKind.Items).Filter(kind => kind.Group.Equals(row))));
Operation<Mesh, MeshSample>.Build(key: key, state: (Key: key, Kinds, Source),
    evaluator: static (state, mesh) =>
        from parameters in state.Source.Capture(mesh: mesh).ToEff()
```

To

```csharp
[SmartEnum]
public static readonly MeshSampleGroup Validity = new(capture: static _ => Fin.Succ(MeshCheckParameters.Defaults()));
public static readonly MeshSampleGroup Count = new(capture: static _ => Fin.Succ(MeshCheckParameters.Defaults()));
public static readonly MeshSampleGroup Defect = new(capture: static mesh => Requirement.MeshReport(mesh: mesh, check: nameof(Defect)));
public static readonly MeshSampleGroup Quality = new(capture: static _ => Fin.Succ(MeshCheckParameters.Defaults()));
[UseDelegateFromConstructor] internal partial Fin<MeshCheckParameters> Capture(Mesh mesh);
Operation<Mesh, MeshSample>.Build(key: key, state: (Key: key, Kinds: toSeq(MeshSampleKind.Items).Filter(kind => kind.Group.Equals(this)), Group: this),
    evaluator: static (state, mesh) =>
        from parameters in state.Group.Capture(mesh: mesh).ToEff()
// Label DELETED
// Decade DELETED
// Source DELETED
// OfDecade DELETED
// Decades DELETED
// Kinds DELETED
// Bands DELETED
```

`inspect.md:371-409` — sample-row keys, label-backed operation cache, and the decade-derived group projection.

From

```csharp
[SmartEnum<int>]
public static readonly MeshSampleKind Valid = new(key: 1, label: nameof(Valid), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsValid)));
public static readonly MeshSampleKind Closed = new(key: 2, label: nameof(Closed), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsClosed)));
public static readonly MeshSampleKind Oriented = new(key: 3, label: nameof(Oriented), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented)));
public static readonly MeshSampleKind Solid = new(key: 4, label: nameof(Solid), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsSolid)));
public static readonly MeshSampleKind Manifold = new(key: 5, label: nameof(Manifold), sample: static (m, _, key) => Topologies.ManifoldOf(geometry: m, op: key).Map(MeasuredValue.Flag));
public static readonly MeshSampleKind BoundaryFree = new(key: 6, label: nameof(BoundaryFree), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary)));
public static readonly MeshSampleKind Vertices = new(key: 10, label: nameof(Vertices), sample: static (m, _, key) => TopologyScalar.VertexCount.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Faces = new(key: 11, label: nameof(Faces), sample: static (m, _, key) => TopologyScalar.FaceCount.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Triangles = new(key: 12, label: nameof(Triangles), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.Faces.TriangleCount)));
public static readonly MeshSampleKind Quads = new(key: 13, label: nameof(Quads), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.Faces.QuadCount)));
public static readonly MeshSampleKind Edges = new(key: 14, label: nameof(Edges), sample: static (m, _, key) => TopologyScalar.EdgeCount.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Euler = new(key: 15, label: nameof(Euler), sample: static (m, _, key) => TopologyScalar.Euler.Extract(geometry: m, op: key));
public static readonly MeshSampleKind VisiblePolygons = new(key: 16, label: nameof(VisiblePolygons), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.GetNgonAndFacesCount())));
public static readonly MeshSampleKind DegenerateFaces = new(key: 20, label: nameof(DegenerateFaces), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DegenerateFaceCount)));
public static readonly MeshSampleKind DisjointMeshes = new(key: 21, label: nameof(DisjointMeshes), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DisjointMeshCount)));
public static readonly MeshSampleKind DuplicateFaces = new(key: 22, label: nameof(DuplicateFaces), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DuplicateFaceCount)));
public static readonly MeshSampleKind ExtremelyShortEdges = new(key: 23, label: nameof(ExtremelyShortEdges), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.ExtremelyShortEdgeCount)));
public static readonly MeshSampleKind InvalidNgons = new(key: 24, label: nameof(InvalidNgons), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.InvalidNgonCount)));
public static readonly MeshSampleKind NakedEdges = new(key: 25, label: nameof(NakedEdges), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NakedEdgeCount)));
public static readonly MeshSampleKind NonManifoldEdges = new(key: 26, label: nameof(NonManifoldEdges), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NonManifoldEdgeCount)));
public static readonly MeshSampleKind NonUnitVectorNormals = new(key: 27, label: nameof(NonUnitVectorNormals), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NonUnitVectorNormalCount)));
public static readonly MeshSampleKind RandomFaceNormals = new(key: 28, label: nameof(RandomFaceNormals), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.RandomFaceNormalCount)));
public static readonly MeshSampleKind SelfIntersectingPairs = new(key: 29, label: nameof(SelfIntersectingPairs), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.SelfIntersectingPairsCount)));
public static readonly MeshSampleKind UnusedVertices = new(key: 30, label: nameof(UnusedVertices), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.UnusedVertexCount)));
public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: 31, label: nameof(VertexFaceNormalsDiffer), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.VertexFaceNormalsDifferCount)));
public static readonly MeshSampleKind ZeroLengthNormals = new(key: 32, label: nameof(ZeroLengthNormals), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.ZeroLengthNormalCount)));
public static readonly MeshSampleKind MaximumValence = new(key: 40, label: nameof(MaximumValence), sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Count((int)stat.Maximum.Value)));
public static readonly MeshSampleKind MinimumValence = new(key: 41, label: nameof(MinimumValence), sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Count((int)stat.Minimum.Value)));
public static readonly MeshSampleKind BoundaryLoopCount = new(key: 42, label: nameof(BoundaryLoopCount), sample: static (m, _, key) => TopologyScalar.BoundaryLoops.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Genus = new(key: 43, label: nameof(Genus), sample: static (m, _, key) => TopologyScalar.Genus.Extract(geometry: m, op: key));
public static readonly MeshSampleKind AverageValence = new(key: 44, label: nameof(AverageValence), sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Statistic(stat.Mean)));
public string Label { get; }
internal MeshSampleGroup Group => MeshSampleGroup.OfDecade(decade: Key / 10);
internal Op Op => Keys.Value[this];
private static readonly Lazy<FrozenDictionary<MeshSampleKind, Op>> Keys = new(static () =>
    Items.ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Label)));
```

To

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public static readonly MeshSampleKind Valid = new(key: nameof(Valid), group: MeshSampleGroup.Validity, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsValid)));
public static readonly MeshSampleKind Closed = new(key: nameof(Closed), group: MeshSampleGroup.Validity, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsClosed)));
public static readonly MeshSampleKind Oriented = new(key: nameof(Oriented), group: MeshSampleGroup.Validity, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented)));
public static readonly MeshSampleKind Solid = new(key: nameof(Solid), group: MeshSampleGroup.Validity, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsSolid)));
public static readonly MeshSampleKind Manifold = new(key: nameof(Manifold), group: MeshSampleGroup.Validity, sample: static (m, _, key) => Topologies.ManifoldOf(geometry: m, op: key).Map(MeasuredValue.Flag));
public static readonly MeshSampleKind BoundaryFree = new(key: nameof(BoundaryFree), group: MeshSampleGroup.Validity, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary)));
public static readonly MeshSampleKind Vertices = new(key: nameof(Vertices), group: MeshSampleGroup.Count, sample: static (m, _, key) => TopologyScalar.VertexCount.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Faces = new(key: nameof(Faces), group: MeshSampleGroup.Count, sample: static (m, _, key) => TopologyScalar.FaceCount.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Triangles = new(key: nameof(Triangles), group: MeshSampleGroup.Count, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.Faces.TriangleCount)));
public static readonly MeshSampleKind Quads = new(key: nameof(Quads), group: MeshSampleGroup.Count, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.Faces.QuadCount)));
public static readonly MeshSampleKind Edges = new(key: nameof(Edges), group: MeshSampleGroup.Count, sample: static (m, _, key) => TopologyScalar.EdgeCount.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Euler = new(key: nameof(Euler), group: MeshSampleGroup.Count, sample: static (m, _, key) => TopologyScalar.Euler.Extract(geometry: m, op: key));
public static readonly MeshSampleKind VisiblePolygons = new(key: nameof(VisiblePolygons), group: MeshSampleGroup.Count, sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.GetNgonAndFacesCount())));
public static readonly MeshSampleKind DegenerateFaces = new(key: nameof(DegenerateFaces), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DegenerateFaceCount)));
public static readonly MeshSampleKind DisjointMeshes = new(key: nameof(DisjointMeshes), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DisjointMeshCount)));
public static readonly MeshSampleKind DuplicateFaces = new(key: nameof(DuplicateFaces), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DuplicateFaceCount)));
public static readonly MeshSampleKind ExtremelyShortEdges = new(key: nameof(ExtremelyShortEdges), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.ExtremelyShortEdgeCount)));
public static readonly MeshSampleKind InvalidNgons = new(key: nameof(InvalidNgons), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.InvalidNgonCount)));
public static readonly MeshSampleKind NakedEdges = new(key: nameof(NakedEdges), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NakedEdgeCount)));
public static readonly MeshSampleKind NonManifoldEdges = new(key: nameof(NonManifoldEdges), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NonManifoldEdgeCount)));
public static readonly MeshSampleKind NonUnitVectorNormals = new(key: nameof(NonUnitVectorNormals), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NonUnitVectorNormalCount)));
public static readonly MeshSampleKind RandomFaceNormals = new(key: nameof(RandomFaceNormals), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.RandomFaceNormalCount)));
public static readonly MeshSampleKind SelfIntersectingPairs = new(key: nameof(SelfIntersectingPairs), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.SelfIntersectingPairsCount)));
public static readonly MeshSampleKind UnusedVertices = new(key: nameof(UnusedVertices), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.UnusedVertexCount)));
public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: nameof(VertexFaceNormalsDiffer), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.VertexFaceNormalsDifferCount)));
public static readonly MeshSampleKind ZeroLengthNormals = new(key: nameof(ZeroLengthNormals), group: MeshSampleGroup.Defect, sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.ZeroLengthNormalCount)));
public static readonly MeshSampleKind MaximumValence = new(key: nameof(MaximumValence), group: MeshSampleGroup.Quality, sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Count((int)stat.Maximum.Value)));
public static readonly MeshSampleKind MinimumValence = new(key: nameof(MinimumValence), group: MeshSampleGroup.Quality, sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Count((int)stat.Minimum.Value)));
public static readonly MeshSampleKind BoundaryLoopCount = new(key: nameof(BoundaryLoopCount), group: MeshSampleGroup.Quality, sample: static (m, _, key) => TopologyScalar.BoundaryLoops.Extract(geometry: m, op: key));
public static readonly MeshSampleKind Genus = new(key: nameof(Genus), group: MeshSampleGroup.Quality, sample: static (m, _, key) => TopologyScalar.Genus.Extract(geometry: m, op: key));
public static readonly MeshSampleKind AverageValence = new(key: nameof(AverageValence), group: MeshSampleGroup.Quality, sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Statistic(stat.Mean)));
internal MeshSampleGroup Group { get; }
// Label DELETED
// MeshSampleKind.Op DELETED
// Keys DELETED
```

`inspect.md:367` — the sole sample-operation key read.

From

```csharp
state.Kinds.TraverseM(kind => kind.Sample(mesh: mesh, parameters: parameters, key: kind.Op)
```

To

```csharp
state.Kinds.TraverseM(kind => kind.Sample(mesh: mesh, parameters: parameters, key: Op.Of(name: kind.Key))
```

Why

`CensusSource` has no identity beyond selecting one of two delegates for the four groups, while the integer keys and labels on both rosters never cross a boundary. The decade classifier is also false: defect rows `30` through `32` derive decade `3`, for which no group exists. `Bands` freezes one filter result per group behind a one-read `Kinds` accessor even though `Census` can derive its selected rows directly once while constructing the operation.

Change

Make groups keyless, carry capture on the group, derive its sample sequence directly when constructing the census operation, make sample names their Thinktecture string keys, carry only the group and behavior on each sample row, and delete both mirrored indexes plus their accessors.

Delta

-19 LOC, -1 module-level type, and -12 declared module-level members; only the unused group keyed surface disappears.

Ripples

`inspect.md:10,243-251,575-599` removes `CensusSource` and decade-derived grouping from the mesh card, diagram, and density table; the replacement states that each string-keyed sample row carries its one group and derives `Op.Of(row.Key)` at the census call, while each keyless group carries capture directly.

# 13. Inline visible-polygon addressing

`inspect.md:308,326-334` — the `AtVisiblePolygonCase` arm and its one-call `Addressed` builder.

From

```csharp
atVisiblePolygonCase: static (key, at) => Lift<TGeometry, TOut, TopologyProjection>(key: key, source: Addressed(index: at.Value, key: key)),
private static Operation<Mesh, TopologyProjection> Addressed(Option<int> index, Op key) =>
    Analysis.Operation<Mesh, TopologyProjection>.Build(key: key, state: (Key: key, Selector: index),
        evaluator: static (state, geometry) => PolygonsOf(mesh: geometry, key: state.Key).Bind(polygons => (Source: polygons, Index: state.Selector.IfNone(0)) switch {
            (Seq<MeshNgon> source, _) when source.Count == 0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()),
            (Seq<MeshNgon> source, int selected) when selected < 0 || selected >= source.Count => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
            (Seq<MeshNgon> source, int selected) => SourceOf(mesh: geometry, polygon: source[selected], key: state.Key)
                .Bind(component => TopologyProjection.Of(mesh: geometry, source: component))
                .Bind(projection => state.Key.Accept(value: projection)),
        }).ToEff());
```

To

```csharp
atVisiblePolygonCase: static (key, at) => Lift<TGeometry, TOut, TopologyProjection>(key: key,
    source: Analysis.Operation<Mesh, TopologyProjection>.Build(key: key, state: (Key: key, Selector: at.Value),
        evaluator: static (state, geometry) => PolygonsOf(mesh: geometry, key: state.Key).Bind(polygons => (Source: polygons, Index: state.Selector.IfNone(0)) switch {
            (Seq<MeshNgon> source, _) when source.Count == 0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()),
            (Seq<MeshNgon> source, int selected) when selected < 0 || selected >= source.Count => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
            (Seq<MeshNgon> source, int selected) => SourceOf(mesh: geometry, polygon: source[selected], key: state.Key)
                .Bind(component => TopologyProjection.Of(mesh: geometry, source: component))
                .Bind(projection => state.Key.Accept(value: projection)),
        }).ToEff())),
// Addressed DELETED
```

Why

`Addressed` is called by one generated case and carries no vocabulary outside that case.

Change

Seat the selector state and evaluator directly on `AtVisiblePolygonCase`.

Delta

-1 LOC and -1 module-level member.

# 14. Inline simple mesh operation builders

`inspect.md:309-311,335-345` — the count, naked-edge, and outline arms with their single-call builders.

From

```csharp
visiblePolygonCountCase: static (key, _) => Lift<TGeometry, TOut, int>(key: key, source: Tally(key: key)),
nakedEdgesCase: static (key, _) => Lift<TGeometry, TOut, Polyline>(key: key, source: Boundaries(key: key)),
outlineCase: static (key, o) => Lift<TGeometry, TOut, Polyline>(key: key, source: Section(plane: o.Plane, key: key)));
private static Operation<Mesh, int> Tally(Op key) =>
    Analysis.Operation<Mesh, int>.Build(key: key, state: key,
        evaluator: static (op, mesh) => op.Accept(value: mesh.GetNgonAndFacesCount()).ToEff());
private static Operation<Mesh, Polyline> Boundaries(Op key) =>
    Analysis.Operation<Mesh, Polyline>.Build(key: key, state: key,
        evaluator: static (op, mesh) => Optional(mesh.GetNakedEdges()).Map(loops => op.Accept(values: loops)).IfNone(Fin.Succ(Seq<Polyline>())).ToEff());
private static Operation<Mesh, Polyline> Section(Plane plane, Op key) =>
    plane.IsValid
        ? Analysis.Operation<Mesh, Polyline>.Build(key: key, state: (Key: key, Plane: plane),
            evaluator: static (state, mesh) => state.Key.Accept(values: mesh.GetOutlines(plane: state.Plane)).ToEff())
        : Analysis.Operation<Mesh, Polyline>.Reject(key: key, fault: key.InvalidInput());
```

To

```csharp
visiblePolygonCountCase: static (key, _) => Lift<TGeometry, TOut, int>(key: key, source:
    Analysis.Operation<Mesh, int>.Build(key: key, state: key, evaluator: static (op, mesh) => op.Accept(value: mesh.GetNgonAndFacesCount()).ToEff())),
nakedEdgesCase: static (key, _) => Lift<TGeometry, TOut, Polyline>(key: key, source:
    Analysis.Operation<Mesh, Polyline>.Build(key: key, state: key, evaluator: static (op, mesh) => Optional(mesh.GetNakedEdges()).Map(loops => op.Accept(values: loops)).IfNone(Fin.Succ(Seq<Polyline>())).ToEff())),
outlineCase: static (key, o) => Lift<TGeometry, TOut, Polyline>(key: key, source: o.Plane.IsValid
    ? Analysis.Operation<Mesh, Polyline>.Build(key: key, state: (Key: key, Plane: o.Plane), evaluator: static (state, mesh) =>
        Optional(mesh.GetOutlines(plane: state.Plane)).Map(outlines => state.Key.Accept(values: outlines)).IfNone(Fin.Succ(Seq<Polyline>())).ToEff())
    : Analysis.Operation<Mesh, Polyline>.Reject(key: key, fault: key.InvalidInput())));
// Tally DELETED
// Boundaries DELETED
// Section DELETED
```

Why

Each helper is a one-call wrapper around `Operation.Build` or `Reject`; the generated case already supplies its complete discriminant and state. RhinoCommon uses a null outline array for an empty projection, which is an empty successful result rather than an invalid collection.

Change

Construct the three operations at their dispatch arms, normalize an absent outline array to an empty sequence, and delete the wrappers.

Delta

-6 LOC and -3 module-level members.

# 15. Inline face-shape projection

`inspect.md:453-470` — the shape census and its single-call polygon projection.

From

```csharp
internal static Operation<Mesh, MeshFaceShape> Shapes(Op key) =>
    Operation<Mesh, MeshFaceShape>.Build(key: key, state: key, requirement: Some(Requirement.MeshCheck), requiresContext: true,
        evaluator: static (op, mesh) =>
            from runtime in Env.EnvAsks
            from shapes in Meshes.PolygonsOf(mesh: mesh, key: op).Bind(polygons => (Moments: FaceMoments.Seeded(), Polygons: polygons) switch {
                var run => run.Polygons.TraverseM(polygon => runtime.Cancellation.IsCancellationRequested
                    ? Fin.Fail<MeshFaceShape>(Errors.Cancelled)
                    : Shape(mesh: mesh, polygon: polygon, moments: run.Moments, context: runtime.Context, key: op)).As(),
            }).ToEff()
            select shapes);
internal static Fin<MeshFaceShape> Shape(Mesh mesh, MeshNgon polygon, FaceMoments moments, Context context, Op key) =>
    Probe(mesh: mesh, polygon: polygon, moments: moments, key: key)
        .Bind(probe => Ring<VectorCloudShape>(metric: VectorCloudMetric.Shape, probe: probe, context: context, key: key)
            .Map(shape => new MeshFaceShape(Source: probe.Source, Shape: shape)));
```

To

```csharp
internal static Operation<Mesh, MeshFaceShape> Shapes(Op key) =>
    Operation<Mesh, MeshFaceShape>.Build(key: key, state: key, requirement: Some(Requirement.MeshCheck), requiresContext: true,
        evaluator: static (op, mesh) =>
            from runtime in Env.EnvAsks
            from shapes in Meshes.PolygonsOf(mesh: mesh, key: op).Bind(polygons => (Moments: FaceMoments.Seeded(), Polygons: polygons) switch {
                var run => run.Polygons.TraverseM(polygon => runtime.Cancellation.IsCancellationRequested
                    ? Fin.Fail<MeshFaceShape>(Errors.Cancelled)
                    : Probe(mesh: mesh, polygon: polygon, moments: run.Moments, key: op)
                        .Bind(probe => Ring<VectorCloudShape>(metric: VectorCloudMetric.Shape, probe: probe, context: runtime.Context, key: op)
                            .Map(shape => new MeshFaceShape(Source: probe.Source, Shape: shape)))).As(),
            }).ToEff()
            select shapes);
// Shape DELETED
```

Why

`Shape` is called only by `Shapes` and only forwards one polygon through the reusable `Probe` and `Ring` owners. `Shapes` must remain on `MeshMetric`: moving its body into `Meshes` would make the call depend on `MeshMetric`'s private implementation members.

Change

Inline the polygon projection into the shape census and delete only the one-call helper.

Delta

-1 LOC and -1 module-level member.

# 16. Inline the metric census into its operation

`inspect.md:446-452,475-480` — `MeshMetric.Folded` and its single-call `Census` helper.

From

```csharp
private Operation<Mesh, TValue> Folded<TValue>(Op key, Func<Seq<MeshMetricSample>, Op, Fin<Seq<TValue>>> terminal) where TValue : notnull =>
    Operation<Mesh, TValue>.Build(key: key, state: (Key: key, Metric: this, Terminal: terminal), requirement: Some(Requirement.MeshCheck), requiresContext: true,
        evaluator: static (state, mesh) =>
            from runtime in Env.EnvAsks
            from values in Census(mesh: mesh, metric: state.Metric, context: runtime.Context, key: state.Key, cancel: runtime.Cancellation)
                .Bind(samples => state.Terminal(arg1: samples, arg2: state.Key)).ToEff()
            select values);
private static Fin<Seq<MeshMetricSample>> Census(Mesh mesh, MeshMetric metric, Context context, Op key, CancellationToken cancel) =>
    Meshes.PolygonsOf(mesh: mesh, key: key).Bind(polygons => (Moments: FaceMoments.Seeded(), Polygons: polygons) switch {
        var run => run.Polygons.TraverseM(polygon => cancel.IsCancellationRequested
            ? Fin.Fail<MeshMetricSample>(Errors.Cancelled)
            : metric.Sample(mesh: mesh, polygon: polygon, moments: run.Moments, context: context, key: key)).As(),
    });
```

To

```csharp
private Operation<Mesh, TValue> Folded<TValue>(Op key, Func<Seq<MeshMetricSample>, Op, Fin<Seq<TValue>>> terminal) where TValue : notnull =>
    Operation<Mesh, TValue>.Build(key: key, state: (Key: key, Metric: this, Terminal: terminal), requirement: Some(Requirement.MeshCheck), requiresContext: true,
        evaluator: static (state, mesh) =>
            from runtime in Env.EnvAsks
            from values in Meshes.PolygonsOf(mesh: mesh, key: state.Key)
                .Bind(polygons => (Moments: FaceMoments.Seeded(), Polygons: polygons) switch {
                    var run => run.Polygons.TraverseM(polygon => runtime.Cancellation.IsCancellationRequested
                        ? Fin.Fail<MeshMetricSample>(Errors.Cancelled)
                        : state.Metric.Sample(mesh: mesh, polygon: polygon, moments: run.Moments, context: runtime.Context, key: state.Key)).As(),
                }).Bind(samples => state.Terminal(arg1: samples, arg2: state.Key)).ToEff()
            select values);
// Census DELETED
```

Why

`Census` has one caller and merely unwraps the same runtime context `Folded` already reads before invoking the metric row.

Change

Seat polygon traversal and cancellation directly in the metric operation evaluator and delete the forwarding helper.

Delta

-2 LOC and -1 module-level member.

# 17. Use the keyed LanguageExt cache directly

`inspect.md:417-427,457,476,502,506,511-516,527` — `FaceMoments`, `PolygonProbe`, cache seeding, cache reads, and the face-moment producer.

From

```csharp
internal readonly record struct FaceMoments(Atom<HashMap<int, (Vector3d Normal, double Area)>> Held) {
    internal static FaceMoments Seeded() => new(Held: Atom(HashMap<int, (Vector3d Normal, double Area)>()));
    internal Fin<(Vector3d Normal, double Area)> At(int face, Func<Fin<(Vector3d Normal, double Area)>> measure) =>
        Held.Value.Find(key: face).Match(
            Some: Fin.Succ,
            None: () => measure().Map(moment => Cell.Claim(cell: Held, key: face, mint: () => moment).Current[face]));
}
internal readonly record struct PolygonProbe(Mesh Mesh, ComponentIndex Source, Option<Seq<Point3d>> Vertices, FaceMoments Moments) {
(Moments: FaceMoments.Seeded(), Polygons: polygons)
probe.Moments.At(face: face, measure: () => FaceMomentOf(probe: probe, face: face, context: context, key: key))
private static Fin<(Vector3d Normal, double Area)> FaceMomentOf(PolygonProbe probe, int face, Context context, Op key) =>
    Normals(mesh: probe.Mesh)
        ? from normal in Rasm.Numerics.Direction.Of(value: new Vector3d(probe.Mesh.FaceNormals[face]), context: context, key: key).Map(static direction => direction.Value)
          from area in Ring<double>(metric: VectorCloudMetric.Area, probe: probe.AtFace(face: face), context: context, key: key)
          select (Normal: normal, Area: area)
        : Fin.Fail<(Vector3d Normal, double Area)>(key.InvalidResult());
```

To

```csharp
internal readonly record struct PolygonProbe(Mesh Mesh, ComponentIndex Source, Option<Seq<Point3d>> Vertices, AtomHashMap<int, (Vector3d Normal, double Area)> Moments) {
(Moments: AtomHashMap(HashMap<int, (Vector3d Normal, double Area)>()), Polygons: polygons)
FaceMomentOf(probe: probe, face: face, context: context, key: key)
private static Fin<(Vector3d Normal, double Area)> FaceMomentOf(PolygonProbe probe, int face, Context context, Op key) =>
    probe.Moments.Find(face).Map(static moment => Fin.Succ(moment)).IfNone(() =>
        (Normals(mesh: probe.Mesh)
            ? from normal in Rasm.Numerics.Direction.Of(value: new Vector3d(probe.Mesh.FaceNormals[face]), context: context, key: key).Map(static direction => direction.Value)
              from area in Ring<double>(metric: VectorCloudMetric.Area, probe: probe.AtFace(face: face), context: context, key: key)
              select (Normal: normal, Area: area)
            : Fin.Fail<(Vector3d Normal, double Area)>(key.InvalidResult()))
        .Map(moment => probe.Moments.FindOrMaybeAdd(face, () => Some(moment)).IfNone(moment)));
// FaceMoments DELETED
```

Why

`FaceMoments` only renames a cell and forwards one cache read, while `Atom<HashMap<...>>` rebuilds the whole persistent map for a keyed insertion. LanguageExt already supplies `AtomHashMap`, its key-grain cache with atomic find-or-add.

Change

Carry `AtomHashMap` directly on `PolygonProbe`, seed it at the two census sites, make `FaceMomentOf` perform the cache lookup and insertion around its existing producer, and delete the wrapper type and forwarding members.

Delta

-4 LOC, -1 module-level type, and -2 module-level members.

# 18. Keep inspection read-only

`inspect.md:500-517` — polygon-normal selection, face-moment fallback, and the normal-buffer probe. The `From` below is the state AFTER Task 17 has landed, which is what the file holds when this task runs.

From

```csharp
{ ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < probe.Mesh.Ngons.Count =>
    FaceIndicesOf(mesh: probe.Mesh, ngon: ngon, key: key)
        .Bind(faces => Normals(mesh: probe.Mesh)
            ? faces.TraverseM(face => FaceMomentOf(probe: probe, face: face, context: context, key: key)).As()
                .Bind(moments => Rasm.Numerics.Direction.Of(value: moments.Fold(initialState: Vector3d.Zero, f: static (sum, moment) => sum + (moment.Normal * moment.Area)), context: context, key: key).Map(static direction => direction.Value))
            : Ring<Vector3d>(metric: VectorCloudMetric.Normal, probe: probe, context: context, key: key)),
private static Fin<(Vector3d Normal, double Area)> FaceMomentOf(PolygonProbe probe, int face, Context context, Op key) =>
    probe.Moments.Find(face).Map(static moment => Fin.Succ(moment)).IfNone(() =>
        (Normals(mesh: probe.Mesh)
            ? from normal in Rasm.Numerics.Direction.Of(value: new Vector3d(probe.Mesh.FaceNormals[face]), context: context, key: key).Map(static direction => direction.Value)
              from area in Ring<double>(metric: VectorCloudMetric.Area, probe: probe.AtFace(face: face), context: context, key: key)
              select (Normal: normal, Area: area)
            : Fin.Fail<(Vector3d Normal, double Area)>(key.InvalidResult()))
        .Map(moment => probe.Moments.FindOrMaybeAdd(face, () => Some(moment)).IfNone(moment)));
private static bool Normals(Mesh mesh) => mesh.FaceNormals.Count >= mesh.Faces.Count || mesh.FaceNormals.ComputeFaceNormals();
```

To

```csharp
{ ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < probe.Mesh.Ngons.Count =>
    FaceIndicesOf(mesh: probe.Mesh, ngon: ngon, key: key)
        .Bind(faces => probe.Mesh.FaceNormals.Count >= probe.Mesh.Faces.Count
            ? faces.TraverseM(face => FaceMomentOf(probe: probe, face: face, context: context, key: key)).As()
                .Bind(moments => Rasm.Numerics.Direction.Of(value: moments.Fold(initialState: Vector3d.Zero, f: static (sum, moment) => sum + (moment.Normal * moment.Area)), context: context, key: key).Map(static direction => direction.Value))
            : Ring<Vector3d>(metric: VectorCloudMetric.Normal, probe: probe, context: context, key: key)),
private static Fin<(Vector3d Normal, double Area)> FaceMomentOf(PolygonProbe probe, int face, Context context, Op key) =>
    probe.Moments.Find(face).Map(static moment => Fin.Succ(moment)).IfNone(() =>
        (probe.Mesh.FaceNormals.Count >= probe.Mesh.Faces.Count
            ? from normal in Rasm.Numerics.Direction.Of(value: new Vector3d(probe.Mesh.FaceNormals[face]), context: context, key: key).Map(static direction => direction.Value)
              from area in Ring<double>(metric: VectorCloudMetric.Area, probe: probe.AtFace(face: face), context: context, key: key)
              select (Normal: normal, Area: area)
            : (Ring<Vector3d>(metric: VectorCloudMetric.Normal, probe: probe.AtFace(face: face), context: context, key: key),
               Ring<double>(metric: VectorCloudMetric.Area, probe: probe.AtFace(face: face), context: context, key: key))
                .Apply(static (normal, area) => (Normal: normal, Area: area)).As())
        .Map(moment => probe.Moments.FindOrMaybeAdd(face, () => Some(moment)).IfNone(moment)));
// Normals DELETED
```

Why

`Normals` is a predicate-shaped read that calls `ComputeFaceNormals`, mutating the caller's mesh during inspection. The ring metric already supplies the lossless normal and area fallback when stored host normals are absent.

Change

Treat the existing normal buffer as the host fast path, derive missing face moments through the ring metrics, and delete the mutating helper.

Delta

+2 LOC and -1 module-level member.

# 19. Dispatch edge aspect by polygon kind

`inspect.md:518-522` — `MeshMetric.EdgeAspectOf` host fast path and fallback.

From

```csharp
private static Fin<double> EdgeAspectOf(PolygonProbe probe, Context context, Op key) =>
    (probe.Source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index } when index >= 0 && index < probe.Mesh.Faces.Count => Fin.Succ(probe.Mesh.Faces.GetFaceAspectRatio(index: index)),
        _ => Fin.Fail<double>(key.InvalidInput()),
    }).BindFail(_ => Ring<double>(metric: VectorCloudMetric.EdgeAspect, probe: probe, context: context, key: key));
```

To

```csharp
private static Fin<double> EdgeAspectOf(PolygonProbe probe, Context context, Op key) => probe.Source switch {
    { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < probe.Mesh.Faces.Count =>
        Fin.Succ(probe.Mesh.Faces.GetFaceAspectRatio(index: face)),
    { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < probe.Mesh.Ngons.Count =>
        Ring<double>(metric: VectorCloudMetric.EdgeAspect, probe: probe, context: context, key: key),
    _ => Fin.Fail<double>(key.InvalidInput()),
};
```

Why

`BindFail` treats an invalid component address as permission to try a different algorithm. The structural discriminant, not the failure channel, selects the face fast path or ngon ring path.

Change

Make face and ngon cases explicit and preserve invalid addressing as the original typed fault.

Delta

+2 LOC; neutral module-level type/member count, required to keep the larger reduction from changing failure semantics.

# 20. Refuse non-integral genus

`inspect.md:117-122` — the orientable-surface genus derivation.

From

```csharp
? (EulerOf(geometry: native, op: op), BoundaryLoopsOf(geometry: native, op: op), PieceCount(geometry: native, op: op))
    .Apply(static (euler, boundaries, components) => ((2 * components) - euler - boundaries) / 2).As()
: Fin.Fail<int>(op.Unsupported(inputType: native.GetType(), outputType: typeof(int)))));
```

To

```csharp
? (EulerOf(geometry: native, op: op), BoundaryLoopsOf(geometry: native, op: op), PieceCount(geometry: native, op: op))
    .Apply(static (euler, boundaries, components) => (2L * components) - euler - boundaries).As()
    .Bind(numerator => guard(numerator >= 0 && numerator % 2 == 0 && numerator / 2 <= int.MaxValue, op.InvalidResult()).ToFin().Map(_ => (int)(numerator / 2)))
: Fin.Fail<int>(op.Unsupported(inputType: native.GetType(), outputType: typeof(int)))));
```

Why

Integer division silently truncates an odd Euler numerator and can manufacture a genus from inconsistent topology evidence; the `int` intermediate can also overflow before that admission runs.

Change

Keep the three independent measurements applicative, derive in `long`, then admit parity, sign, and output range before narrowing.

Delta

+1 LOC; neutral module-level type/member count, required for truthful typed output.

# 21. Delete the planar-only hole formula

`inspect.md:123-126,228` — `HoleCountOf` and its topology row.

From

```csharp
internal static Fin<int> HoleCountOf<TG>(TG geometry, Op op) where TG : notnull =>
    OnGeometry(geometry: geometry, op: op, onAny: native =>
        (BoundaryLoopsOf(geometry: native, op: op), PieceCount(geometry: native, op: op))
            .Apply(static (boundaries, components) => Math.Max(val1: 0, val2: boundaries - components)).As());
public static readonly TopologyScalar HoleCount = new(key: 4, label: nameof(HoleCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.HoleCountOf(geometry: g, op: op).Map(MeasuredValue.Count));
```

To

```csharp
// HoleCountOf DELETED
// TopologyScalar.HoleCount DELETED
```

Why

`boundary components - connected components` counts interior rings only for planar regions with one distinguished exterior ring per component. The operation admits arbitrary orientable Breps and meshes, where handles are genus and boundary components have no such exterior/interior partition; clamping the planar formula manufactures a plausible but false invariant.

Change

Delete the invalid derived scalar. Callers retain the lossless `BoundaryLoops`, `Genus`, and component queries instead of a topology-dependent quantity named as a universal hole count.

Delta

-5 LOC and -2 module-level members.

Ripples

Remove `HoleCount` from the topology owner, scalar roster description, and density count in `inspect.md`; the direct-consumer search found no external construction.

# 22. Keep primitive topology counts truthful

`inspect.md:105-112,194-195` — Brep Euler admission, mesh boundary-loop handling, and the one-call Brep boundary helper.

From

```csharp
onBrep: b => guard(b.IsManifold, op.Unsupported(typeof(Brep), typeof(int))).ToFin().Map(_ => b.Vertices.Count - b.Edges.Count + b.Faces.Count));
onMesh: m => Optional(m.GetNakedEdges()).ToFin(op.InvalidResult()).Map(static loops => loops.Length),
onBrep: static b => Fin.Succ(BoundaryCount(brep: b, predicate: static loop => loop.LoopType is BrepLoopType.Outer or BrepLoopType.Inner)));
private static int BoundaryCount(Brep brep, Func<BrepLoop, bool> predicate) =>
    toSeq(brep.Loops).Filter(loop => predicate(arg: loop) && toSeq(loop.Trims).Exists(static trim => trim.Edge is { Valence: EdgeAdjacency.Naked })).Count;
```

To

```csharp
onBrep: static b => Fin.Succ(b.Vertices.Count - b.Edges.Count + b.Faces.Count));
onMesh: m => m.IsClosed ? Fin.Succ(0) : Optional(m.GetNakedEdges()).ToFin(op.InvalidResult()).Map(static loops => loops.Length),
onBrep: static b => Fin.Succ(toSeq(b.Loops).Filter(static loop =>
    (loop.LoopType is BrepLoopType.Outer or BrepLoopType.Inner) && toSeq(loop.Trims).Exists(static trim => trim.Edge is { Valence: EdgeAdjacency.Naked })).Count));
// BoundaryCount DELETED
```

Why

Euler characteristic is defined by the cell counts independently of manifoldness, so the Brep arm's manifold guard is unrelated double admission. A closed mesh has zero boundary loops even when RhinoCommon represents the absent naked-edge array as null; rejecting that absence makes genus fail on the canonical closed case. `BoundaryCount` then has one fixed predicate and no independent policy.

Change

Return the Brep Euler count directly, establish the closed-mesh zero before reading naked edges, inline the sole Brep boundary predicate, and delete the helper.

Delta

-1 LOC and -1 module-level member.

# 23. Count welded topology vertices

`inspect.md:231` — the mesh arm of `TopologyScalar.VertexCount`.

From

```csharp
public static readonly TopologyScalar VertexCount = new(key: 7, label: nameof(VertexCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.Vertices.Count, brepCount: static b => b.Vertices.Count).Map(MeasuredValue.Count));
```

To

```csharp
public static readonly TopologyScalar VertexCount = new(key: nameof(VertexCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.TopologyVertices.Count, brepCount: static b => b.Vertices.Count).Map(MeasuredValue.Count));
```

Why

The topology scalar already counts mesh topology edges and uses topology vertices in Euler characteristic, but its standalone vertex row reads the unwelded storage buffer. That makes the same owner publish two vertex regimes.

Change

Read the welded `TopologyVertices` count so the vertex, edge, and Euler rows share one topological complex.

Delta

0 LOC and neutral module-level type/member count.

# 24. Remove structural topology construction forwarders

`inspect.md:48-52` — the policy-free structural factories on `Topologies`.

From

```csharp
public static Topologies Kind => new KindCase();
public static Topologies Domains => new DomainsCase();
public static Topologies SolidOrientation => new SolidOrientationCase();
public static Topologies Components => new ComponentsCase();
public static Topologies ContainsPoint(Point3d point) => new ContainsPointCase(Point: point);
```

To

```csharp
// Topologies.Kind DELETED
// Topologies.Domains DELETED
// Topologies.SolidOrientation DELETED
// Topologies.Components DELETED
// Topologies.ContainsPoint DELETED
```

Why

Each member forwards unchanged arguments to a public union case and owns no default, admission, normalization, or shared policy.

Change

Construct the corresponding `Topologies` case directly and delete the parallel construction surface.

Delta

-5 LOC and -5 module-level members.

Ripples

Update the topology owner and growth prose to name public cases as the construction surface. The current planning corpus has no call site for these five factories.

# 25. Remove remaining mesh construction forwarders

`inspect.md:296-300` — the remaining policy-free factories on `Meshes`.

From

```csharp
public static Meshes FaceShape => new FaceShapeCase();
public static Meshes AtVisiblePolygon(Option<int> index = default) => new AtVisiblePolygonCase(Value: index);
public static Meshes VisiblePolygonCount => new VisiblePolygonCountCase();
public static Meshes NakedEdges => new NakedEdgesCase();
public static Meshes Outline(Plane plane) => new OutlineCase(Plane: plane);
```

To

```csharp
// Meshes.FaceShape DELETED
// Meshes.AtVisiblePolygon DELETED
// Meshes.VisiblePolygonCount DELETED
// Meshes.NakedEdges DELETED
// Meshes.Outline DELETED
```

Why

These members only rename public case constructors; even the optional index is passed through without validation or canonicalization.

Change

Construct each `Meshes` case directly and delete the forwarding surface.

Delta

-5 LOC and -5 module-level members.

Ripples

Update the mesh owner and growth prose to name public cases as the construction surface. The current planning corpus has no call site for these five factories.

# 26. Make mesh metrics a keyless behavior roster

`inspect.md:429-435` — `MeshMetric` declaration and rows.

From

```csharp
[SmartEnum<int>]
public static readonly MeshMetric EdgeAspect = new(key: 0, measure: EdgeAspectOf);
public static readonly MeshMetric Area = new(key: 1, measure: AreaOf);
public static readonly MeshMetric Perimeter = new(key: 2, measure: static (probe, context, key) => Ring<double>(metric: VectorCloudMetric.Perimeter, probe: probe, context: context, key: key));
public static readonly MeshMetric Skewness = new(key: 3, measure: static (probe, context, key) => Ring<double>(metric: VectorCloudMetric.Skewness, probe: probe, context: context, key: key));
public static readonly MeshMetric DihedralAngle = new(key: 4, measure: DihedralOf);
```

To

```csharp
[SmartEnum]
public static readonly MeshMetric EdgeAspect = new(measure: EdgeAspectOf);
public static readonly MeshMetric Area = new(measure: AreaOf);
public static readonly MeshMetric Perimeter = new(measure: static (probe, context, key) => Ring<double>(metric: VectorCloudMetric.Perimeter, probe: probe, context: context, key: key));
public static readonly MeshMetric Skewness = new(measure: static (probe, context, key) => Ring<double>(metric: VectorCloudMetric.Skewness, probe: probe, context: context, key: key));
public static readonly MeshMetric DihedralAngle = new(measure: DihedralOf);
```

Why

No operation, consumer, wire, or persistence surface reads the integer key. The five rows are a process-local behavior vocabulary, which Thinktecture models directly as a keyless smart enum.

Change

Remove the invented ordinals and their generated keyed lookup and conversion surface while retaining the row identities and measure delegates.

Delta

0 LOC and neutral declared module-level type/member count; removes the generated key, lookup, and conversion members.

Ripples

`inspect.md:243,250,598` changes `MeshMetric` from `[SmartEnum<int>]` to a keyless behavior roster and removes key-driven growth language.

# 27. Carry signed measurements

`inspect.md:203-220` — the `MeasuredValue` carrier, which models no signed integral measurement.

From

```csharp
    public sealed record CountCase(int Value) : MeasuredValue;
    public sealed record StatisticCase(double Value) : MeasuredValue;
    public static MeasuredValue Count(int tally) => new CountCase(Value: tally);
    public static MeasuredValue Statistic(double value) => new StatisticCase(Value: value);
    internal object Boxed => Switch(
        flagCase: static row => (object)row.Value,
        countCase: static row => (object)row.Value,
        statisticCase: static row => (object)row.Value);
    internal ValidityClaim Admissible => Switch(
        flagCase: static _ => new ValidityClaim(Holds: true),
        countCase: static row => ValidityClaim.CountAtLeast(count: row.Value, floor: 0),
        statisticCase: static row => ValidityClaim.Finite(row.Value));
```

To

```csharp
    public sealed record CountCase(int Value) : MeasuredValue;
    public sealed record SignedCase(int Value) : MeasuredValue;
    public sealed record StatisticCase(double Value) : MeasuredValue;
    public static MeasuredValue Count(int tally) => new CountCase(Value: tally);
    public static MeasuredValue Signed(int value) => new SignedCase(Value: value);
    public static MeasuredValue Statistic(double value) => new StatisticCase(Value: value);
    internal object Boxed => Switch(
        flagCase: static row => (object)row.Value,
        countCase: static row => (object)row.Value,
        signedCase: static row => (object)row.Value,
        statisticCase: static row => (object)row.Value);
    internal ValidityClaim Admissible => Switch(
        flagCase: static _ => new ValidityClaim(Holds: true),
        countCase: static row => ValidityClaim.CountAtLeast(count: row.Value, floor: 0),
        signedCase: static _ => new ValidityClaim(Holds: true),
        statisticCase: static row => ValidityClaim.Finite(row.Value));
```

`inspect.md:225` — the topology Euler row, the ONE source both Euler surfaces read. The `From` below is the state AFTER Task 9 has landed.

From

```csharp
public static readonly TopologyScalar Euler = new(key: nameof(Euler), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.EulerOf(geometry: g, op: op).Map(MeasuredValue.Count));
```

To

```csharp
public static readonly TopologyScalar Euler = new(key: nameof(Euler), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.EulerOf(geometry: g, op: op).Map(MeasuredValue.Signed));
```

Why

Euler characteristic is a signed integer — a genus-2 closed mesh answers `-2` — and the carrier models no signed integral measurement, so the row spells `MeasuredValue.Count`, whose `Admissible` claim floors at zero. Every valid high-genus topology therefore fails admission as an invalid count. `Statistic` is not the escape: it retypes an exact integer as `double` and would break the row's `OutputBinding.Of<int>()`. The missing axis on `MeasuredValue` is the defect; `SignedCase` keeps the value integral, so `Boxed` still yields an `int` and the output binding is unchanged.

Change

Add the signed integral case, its mint, and its two generated `Switch` arms to `MeasuredValue`, then read it from the Euler row. `MeshSampleKind.Euler` already delegates to `TopologyScalar.Euler.Extract`, so the mesh census inherits the correction with no edit of its own — one authority, both surfaces.

Delta

+4 LOC, +1 generated union case, +1 mint; module-level type count neutral. Every other scalar and sample row stays on `Count`, whose nonnegative floor is correct for them.

Ripples

`inspect.md:15` describes `MeasuredValue` as the "flag, count, or statistic" carrier and `inspect.md:599` counts it at three cases with a `flag/count/statistic` label; both become flag, count, signed, or statistic at four cases. No consumer outside this spec names `MeasuredValue`.
