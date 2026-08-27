# 1. Remove the projection vocabulary's unused keys

### Location

`support.md:188-217`, anchored at `[SmartEnum<int>] public sealed partial class SupportProjection` and the fourteen row declarations; `support.md:229-245`, anchored at `Hit`, `HitValue`, and `SpanOf`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(key: 0,
```

```csharp
private static SupportProjection Hit(int key, Func<Type, bool> accepts,
    Func<SupportState, Fin<object>> projectRaw,
```

```csharp
private static SupportProjection HitValue<T>(int key, Func<ClosestHit, Option<T>> choose,
    CapabilitySet<SupportCapability>? requires = null) where T : notnull =>
    Hit(key: key, accepts: static output => output == typeof(T), requires: requires,
```

```csharp
private static SupportProjection SpanOf(int key, double sign) =>
    Hit(key: key,
```

### To

```csharp
// SupportProjection.Key DELETED
[SmartEnum]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(
```

```csharp
private static SupportProjection Hit(Func<Type, bool> accepts,
    Func<SupportState, Fin<object>> projectRaw,
```

```csharp
private static SupportProjection HitValue<T>(Func<ClosestHit, Option<T>> choose,
    CapabilitySet<SupportCapability>? requires = null) where T : notnull =>
    Hit(accepts: static output => output == typeof(T), requires: requires,
```

```csharp
private static SupportProjection SpanOf(double sign) =>
    Hit(
```

Remove `key:` from every row declaration between `Closest` and `SignedSpanAway`.

### Why

No target or `libs/dotnet/` consumer reads, converts, parses, orders, serializes, or looks up a projection by its integer key; consumers use the declared row identities. A keyless Thinktecture smart enum retains `Items`, equality, delegate columns, and generated dispatch while deleting fourteen authored ordinals, the generated key member, keyed lookup, conversions, parsing, and the key parameters threaded through three builders.

# 2. Delete the unused analytic-species wrapper

### Location

`support.md:71-93`, anchored at `AnalyticShape`; `support.md:100`, anchored at `SupportSpace.Analytic`; `support.md:114-116`, anchored at `AnalyticShape.Of(source: source).Match`; `support.md:125-130`, anchored at `SupportSpace.Payload`.

### From

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticShape {
    private AnalyticShape() { }

    public sealed record PlaneCase(Plane Value) : AnalyticShape;
    public sealed record SphereCase(Sphere Value) : AnalyticShape;
    public sealed record BoxCase(Box Value) : AnalyticShape;
    public sealed record BoundCase(BoundingBox Value) : AnalyticShape;
```

```csharp
internal static Option<AnalyticShape> Of(object source) => source switch {
    Plane plane => Some((AnalyticShape)new PlaneCase(Value: plane)),
    Sphere sphere => Some((AnalyticShape)new SphereCase(Value: sphere)),
    Box box => Some((AnalyticShape)new BoxCase(Value: box)),
    BoundingBox bound => Some((AnalyticShape)new BoundCase(Value: bound)),
    _ => Option<AnalyticShape>.None,
};
```

```csharp
public sealed record Analytic(AnalyticShape Shape) : SupportSpace;
```

```csharp
select AnalyticShape.Of(source: source).Match(
    Some: shape => (SupportSpace)new Analytic(Shape: shape),
    None: () => source switch {
```

### To

```csharp
// AnalyticShape DELETED
private sealed record Analytic(object Value) : SupportSpace;
```

```csharp
select source switch {
    Plane or Sphere or Box or BoundingBox =>
        (SupportSpace)new Analytic(Value: source),
```

```csharp
analytic: static a => a.Value,
```

### Why

No operation discriminates the four analytic species after admission: every arm immediately re-boxes its value through `Payload`, and the only surviving distinction is the enclosing `Analytic` proximity regime. Keeping the admitted payload on that private case deletes one module-level type, four nested case types, the optional factory, the generated union surface, and a second dispatch without weakening admission—the exhaustive runtime-type arm at `SupportSpace.Of` remains the single gate.

# 3. Fold the native regimes into one private case

### Location

`support.md:60-68`, anchored at `ContainProbe`; `support.md:99-103`, anchored at the five `SupportSpace` cases; `support.md:117-120`, anchored at the native-shape admission arms; `support.md:125-155`, anchored at `Payload`, `Capabilities`, `SignedReach`, and `ContainReach`; `support.md:166-172`, anchored at `ContainmentDistance`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class ContainProbe {
    public static readonly ContainProbe Brep = new(key: 0,
        inside: static (solid, sample, tolerance) => solid is Rhino.Geometry.Brep shell && shell.IsPointInside(sample, tolerance, strictlyIn: false));
    public static readonly ContainProbe Mesh = new(key: 1,
        inside: static (solid, sample, tolerance) => solid is Rhino.Geometry.Mesh shell && shell.IsPointInside(sample, tolerance, strictlyIn: false));
    [UseDelegateFromConstructor] internal partial bool Inside(GeometryBase solid, Point3d sample, double tolerance);
}
```

```csharp
public sealed record Cluster(VectorCloud.ClusterCase Value) : SupportSpace;
public sealed record Region(GeometryBase Value, ContainProbe Probe, CapabilitySet<SupportCapability> Held) : SupportSpace;
public sealed record Sheet(GeometryBase Value, CapabilitySet<SupportCapability> Held) : SupportSpace;
public sealed record Form(object Value, CapabilitySet<SupportCapability> Held) : SupportSpace;
```

```csharp
Brep { IsSolid: true } brep => new Region(Value: brep, Probe: ContainProbe.Brep, Held: held),
Mesh { IsSolid: true } mesh => new Region(Value: mesh, Probe: ContainProbe.Mesh, Held: held),
Brep or Mesh => new Sheet(Value: (GeometryBase)source, Held: held),
_ => new Form(Value: source, Held: held),
```

```csharp
internal bool ContainReach(ClosestHit hit) => Switch(
    state: hit,
    cluster: static (_, _) => false,
    analytic: static (probe, _) => probe.Distance.IsSome,
    region: static (probe, _) => probe.Distance.IsSome,
    sheet: static (_, _) => false,
    form: static (probe, _) => probe.Normal.IsSome);
```

### To

```csharp
// ContainProbe DELETED
// SupportSpace.Region DELETED
// SupportSpace.Sheet DELETED
// SupportSpace.Form DELETED
// SupportSpace.ContainReach DELETED
```

```csharp
private sealed record Cluster(VectorCloud.ClusterCase Value) : SupportSpace;
private sealed record Native(object Value, CapabilitySet<SupportCapability> Held) : SupportSpace;
```

```csharp
Plane or Sphere or Box or BoundingBox =>
    (SupportSpace)new Analytic(Value: source),
_ => new Native(Value: source, Held: held),
```

```csharp
internal object Payload => Switch(
    cluster: static c => (object)c.Value,
    analytic: static a => a.Value,
    native: static n => n.Value);
```

```csharp
internal bool SignedReach(ClosestHit hit) => Switch(
    state: hit,
    cluster: static (_, _) => false,
    analytic: static (probe, _) => probe.Distance.IsSome,
    native: static (probe, _) => probe.Normal.IsSome);
```

```csharp
internal Fin<double> ContainmentDistance(ClosestHit hit, Point3d sample, Context context, Op key) => Switch(
    state: (Hit: hit, Sample: sample, Context: context, Key: key),
    cluster: static (s, c) => Fin.Fail<double>(s.Key.Unsupported(c.Value.GetType(), typeof(double))),
    analytic: static (s, a) => a.Value.Evaluate<double>(new EvaluationRequest.Signed(s.Sample), s.Key),
```

```csharp
native: static (s, n) => n.Value switch {
    Brep or Mesh => (n.Value switch {
        Brep { IsSolid: true } b => Some(b.IsPointInside(s.Sample, s.Context.For(ToleranceLane.Closure).Value, false)),
        Mesh { IsSolid: true } m => Some(m.IsPointInside(s.Sample, s.Context.For(ToleranceLane.Closure).Value, false)),
        _ => Option<bool>.None,
    }).ToFin(s.Key.Unsupported(n.Value.GetType(), typeof(double)))
        .Bind(inside => s.Hit.Distance.ToFin(s.Key.InvalidResult()).Map(d => (inside ? -1.0 : 1.0) * d)),
    _ => guard(s.Hit.Normal.IsSome, s.Key.Unsupported(n.Value.GetType(), typeof(double))).ToFin()
        >> n.Value.Evaluate<double>(new EvaluationRequest.Signed(s.Sample), s.Key),
});
```

### Why

`Region`, `Sheet`, and `Form` share identity, admission, payload timing, capability storage, closest evaluation, and signed-distance evaluation; only containment distinguishes them. Their split adds two union cases and a module-level two-row strategy whose delegates still type-test on every call. One private `Native` case keeps the common regime whole and localizes the `Brep`/`Mesh` distinction to the sole operation that consumes it, deleting one module-level type, two smart-enum rows, two union cases, one generated delegate, and three public construction bypasses while preserving genuine open-shell, closed-solid, and normal-oriented behavior.

# 4. Use the canonical capability vocabulary directly

### Location

`support.md:31-44`, anchored at `SupportCapability`; `support.md:101-139`, anchored at held capability construction and `SupportSpace.Capabilities`; `support.md:196-236`, anchored at projection requirements, `Requires`, `Hit`, and `HitValue`.

### From

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SupportCapability : ICapability<SupportCapability> {
    public static readonly SupportCapability Normal = new(key: "normal", rank: 0, reach: Capability.ClosestNormal);
    public static readonly SupportCapability Tangent = new(key: "tangent", rank: 1, reach: Capability.ClosestTangent);
    public static readonly SupportCapability Frame = new(key: "frame", rank: 2, reach: Capability.ClosestFrame);
    public static readonly SupportCapability Signed = new(key: "signed", rank: 3, reach: Capability.SignedDistance);
```

```csharp
public int Rank { get; }
private Capability Reach { get; }
internal static CapabilitySet<SupportCapability> Of(Type source) =>
    CapabilitySet<SupportCapability>.Of([.. Items.Where(row => row.Reach.Admits(type: source))]);
```

```csharp
let held = SupportCapability.Of(source: type)
```

```csharp
requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Normal)
requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Tangent)
requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Frame)
requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Signed)
```

### To

```csharp
// SupportCapability DELETED
```

```csharp
private sealed record Native(object Value, CapabilitySet<Capability> Held) : SupportSpace;
```

```csharp
let held = CapabilitySet<Capability>.Of([..
    Capability.Items.Where(row => row.Admits(type: type))])
```

```csharp
internal CapabilitySet<Capability> Capabilities => Switch(
    cluster: static _ => CapabilitySet<Capability>.None,
    analytic: static _ => CapabilitySet<Capability>.Of(Capability.SignedDistance),
    native: static n => n.Held);
```

```csharp
requires: CapabilitySet<Capability>.Of(Capability.ClosestNormal)
requires: CapabilitySet<Capability>.Of(Capability.ClosestTangent)
requires: CapabilitySet<Capability>.Of(Capability.ClosestFrame)
requires: CapabilitySet<Capability>.Of(Capability.SignedDistance)
```

```csharp
private CapabilitySet<Capability> Requires { get; }
```

### Why

`SupportCapability` republishes four existing `Domain/normalization` `Capability` rows under a second string-key roster, adds an unread `Rank`, and forwards its only decision to `Capability.Admits`. Caching the canonical capability rows once at `SupportSpace.Of` preserves admission-time classification and `AdmitsAll` set algebra while deleting a module-level type, four duplicate row symbols, four duplicate keys, and a forwarding factory. Private union cases keep the internal canonical vocabulary out of the public API.

# 5. Remove the mostly constant reach delegate

### Location

`support.md:204-236`, anchored at the two signed projection rows, `Reach`, `Admits`, `Hit`, and `HitValue`; `support.md:250-255`, anchored at the `Project<TOut>` gate.

### From

```csharp
reach: Some<Func<SupportSpace, ClosestHit, bool>>(
    static (space, hit) => space.SignedReach(hit: hit)),
```

```csharp
reach: Some<Func<SupportSpace, ClosestHit, bool>>(
    static (space, hit) => space.ContainReach(hit: hit)),
```

```csharp
[UseDelegateFromConstructor] private partial bool Reach(SupportSpace space, ClosestHit hit);
```

```csharp
private bool Admits(SupportSpace space, ClosestHit hit) =>
    space.Capabilities.AdmitsAll(required: Requires) && Reach(space: space, hit: hit);
```

```csharp
private static SupportProjection Hit(int key, Func<Type, bool> accepts, Func<SupportState, Fin<object>> projectRaw,
    CapabilitySet<SupportCapability>? requires = null,
    Option<Func<SupportSpace, ClosestHit, bool>> reach = default) =>
    new(key: key, requires: requires ?? CapabilitySet<SupportCapability>.None, accepts: accepts,
        reach: reach.IfNone(static () => new Func<SupportSpace, ClosestHit, bool>(static (_, _) => true)),
        projectRaw: projectRaw);
```

### To

```csharp
// SupportProjection.Reach DELETED
```

```csharp
public static readonly SupportProjection SignedDistance = Hit(
    accepts: static output => output == typeof(double),
    requires: CapabilitySet<Capability>.Of(Capability.SignedDistance),
    projectRaw: static s => guard(s.Space.SignedReach(hit: s.Hit),
        s.Key.Unsupported(s.Space.SourceType, typeof(double))).ToFin()
        >> s.Space.SignedDistance(s.Sample, s.Key).Map(static d => (object)d));
```

```csharp
public static readonly SupportProjection ContainmentDistance = Hit(
    accepts: static output => output == typeof(double),
    requires: CapabilitySet<Capability>.Of(Capability.SignedDistance),
    projectRaw: static s => s.Space.ContainmentDistance(
        s.Hit, s.Sample, s.Context, s.Key).Map(static d => (object)d));
```

```csharp
private bool Admits(SupportSpace space) =>
    space.Capabilities.AdmitsAll(required: Requires);
```

```csharp
private static SupportProjection Hit(Func<Type, bool> accepts,
    Func<SupportState, Fin<object>> projectRaw,
    CapabilitySet<Capability>? requires = null) =>
    new(requires: requires ?? CapabilitySet<Capability>.None,
        accepts: accepts, projectRaw: projectRaw);
```

```csharp
(hit.IsValid, Admits(space: space), Accepts(output: typeof(TOut))) switch {
```

### Why

Twelve of fourteen rows receive an allocated always-true reach delegate solely to fill a generated column, while the two exceptional rows immediately execute the operations that own their hit-dependent evidence. Moving the signed-hit guard into `SignedDistance` and letting `ContainmentDistance` perform its own total regime dispatch deletes the generated delegate, its optional factory parameter, its fallback allocation, and the redundant pre-dispatch without weakening the declarative capability requirement.

# 6. Replace the one-use projection state type with its tuple

### Location

`support.md:222-224`, anchored at `ProjectRaw` and `SupportState`; `support.md:255`, anchored at the `SupportState` construction in `Project<TOut>`.

### From

```csharp
[UseDelegateFromConstructor] private partial Fin<object> ProjectRaw(SupportState state);
private readonly record struct SupportState(
    SupportSpace Space, ClosestHit Hit, Point3d Sample,
    Context Context, Op Key, Type Output);
```

```csharp
ProjectRaw(state: new SupportState(Space: space, Hit: hit, Sample: sample,
    Context: context, Key: key, Output: typeof(TOut)))
```

### To

```csharp
[UseDelegateFromConstructor] private partial Fin<object> ProjectRaw(
    (SupportSpace Space, ClosestHit Hit, Point3d Sample,
     Context Context, Op Key, Type Output) state);
```

```csharp
// SupportState DELETED
```

```csharp
ProjectRaw(state: (Space: space, Hit: hit, Sample: sample,
    Context: context, Key: key, Output: typeof(TOut)))
```

### Why

`SupportState` is a private one-use transport whose record identity is never observed. The generated delegate already owns the state boundary, and a named tuple preserves every field name and closure-free row lambda while deleting one type and its constructor surface with no logic or allocation change.

# 7. Derive consumer output shape from the projection row

### Location

`support.md:247-248`, anchored at `SupportProjection.CanProjectVector`; `Spatial/fields.md:410`, anchored at the `HitField` admission law; `Spatial/fields.md:433-437`, anchored at the `Span`/`SignedSpanAway` identity branch.

### From

```csharp
internal bool CanProjectVector(SupportSpace space) =>
    Accepts(output: typeof(Vector3d))
    && space.Capabilities.AdmitsAll(required: Requires);
```

```csharp
c.Projection.Equals(SupportProjection.Span)
    || c.Projection.Equals(SupportProjection.SignedSpanAway)
```

### To

```csharp
// SupportProjection.CanProjectVector DELETED
internal bool CanProject<TOut>(SupportSpace space) =>
    Accepts(output: typeof(TOut)) && Admits(space: space);
```

```csharp
c.Projection.CanProject<VectorSpan>(space: c.Source)
```

### Why

The row's `Accepts` column already owns every output shape, so a vector-specific wrapper exposes less than the capability it forwards. A generic probe keeps one member while allowing `HitField` to derive span handling from declared output support instead of mirroring two row identities. A later span-valued projection joins the consumer without another disjunction.

### Ripples

In `Spatial/fields.md:410`, replace the `SupportProjection.CanProjectVector` reference with `SupportProjection.CanProject<Vector3d>`. In `Spatial/fields.md:433-437`, replace the `Span`/`SignedSpanAway` identity disjunction with the `CanProject<VectorSpan>` call above.
