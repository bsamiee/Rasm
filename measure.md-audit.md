# 1. Remove `Measure` construction forwarders

`measure.md` lines 57-67, the factory block directly below the three union cases.

From

```csharp
public static Measure Length => new LengthCase();
public static Measure SpatialMidpoint => new SpatialMidpointCase();
public static Measure Area => new MassPropertyCase(Mass: MassKind.Area, Property: MassProperty.Magnitude);
public static Measure Volume => new MassPropertyCase(Mass: MassKind.Volume, Property: MassProperty.Magnitude);
public static Measure MassError(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.MagnitudeError);
public static Measure Centroid(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.Centroid);
public static Measure CentroidError(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.CentroidError);
public static Measure Radii(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.Radii);
public static Measure PrincipalAxes(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.PrincipalAxes);
public static Measure Inertia(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.Inertia);
public static Measure InertiaProducts(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.InertiaProducts);
```

To

```csharp
// Measure.Length DELETED
// Measure.SpatialMidpoint DELETED
// Measure.Area DELETED
// Measure.Volume DELETED
// Measure.MassError DELETED
// Measure.Centroid DELETED
// Measure.CentroidError DELETED
// Measure.Radii DELETED
// Measure.PrincipalAxes DELETED
// Measure.Inertia DELETED
// Measure.InertiaProducts DELETED
```

Why: Each member is a policy-free alias for a public union-case constructor. The parallel family makes three request shapes appear to be eleven separate operations.

Change: Construct `LengthCase`, `SpatialMidpointCase`, or `MassPropertyCase` directly and retain every measure coordinate.

Delta: -11 LOC; -11 module-level members; type count neutral.

# 2. Name scalar measures `Length` and `Center`

`measure.md` lines 54-95, the scalar cases, generated dispatch arms, operation keys, and builders.

From

```csharp
public sealed record SpatialMidpointCase : Measure;
lengthCase: static _ => Extent<TGeometry, TOut>(),
spatialMidpointCase: static _ => Midpoint<TGeometry, TOut>(),
private static readonly Op ExtentKey = Op.Of(name: nameof(Extent)), MidpointKey = Op.Of(name: nameof(Midpoint));
private static Operation<TGeometry, TOut> Extent<TGeometry, TOut>() where TGeometry : notnull =>
private static Operation<TGeometry, TOut> Midpoint<TGeometry, TOut>() where TGeometry : notnull =>
```

To

```csharp
public sealed record CenterCase : Measure;
lengthCase: static _ => Length<TGeometry, TOut>(),
centerCase: static _ => Center<TGeometry, TOut>(),
private static readonly Op LengthKey = Op.Of(name: nameof(Length)), CenterKey = Op.Of(name: nameof(Center));
private static Operation<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull =>
private static Operation<TGeometry, TOut> Center<TGeometry, TOut>() where TGeometry : notnull =>
```

Why: `Extent` conventionally names a bounding span, but this operation computes curve length. The point operation mixes analytic centers, bounding-box centers, curve centers, and mass centroids, so `Center` is accurate without falsely claiming every branch is a centroid or midpoint.

Change: Rename the case, generated switch arm, builders, keys, and their local reads together; rename the local `centroid` result to `center`.

Delta: LOC and module-level symbol/member/type counts neutral.

# 3. Make generated keys the mass-row identity

`measure.md` lines 110-151 and 275-310, the `MassKind` and `MassProperty` owners and their duplicate labels.

From

```csharp
[SmartEnum<int>]
public static readonly MassKind Length = new(key: 1, label: nameof(Length), requirement: Requirement.CurveLength,
public string Label { get; }
string ICapability<MassKind>.Key => Label;
[SmartEnum<int>]
public static readonly MassProperty Magnitude = new(key: 0, label: nameof(Magnitude), output: OutputBinding.Of<double>(),
public string Label { get; }
```

To

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public static readonly MassKind Length = new(key: nameof(Length), requirement: Requirement.CurveLength,
// MassKind.Label DELETED
// ICapability<MassKind>.Key DELETED
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public static readonly MassProperty Magnitude = new(key: nameof(Magnitude), output: OutputBinding.Of<double>(),
// MassProperty.Label DELETED
```

Why: The numeric keys have no semantic consumer, while every identity read uses the parallel label. Thinktecture already generates the public key, equality, item roster, and lookup surface from one string-key declaration.

Change: Convert every row key to `nameof(Row)`, remove both label columns and the explicit capability-key forwarder, and read generated `.Key` at remaining operation-name and diagnostic sites.

Delta: -3 LOC; -3 module-level members; type count neutral.

Ripples: Change `row.Mass.Label`, `row.Property.Label`, and `mass.Label` in this spec to `.Key`. No external numeric-key or label consumer exists.

# 4. Derive each mass operation key at use

`measure.md` lines 73-75 and 97, the `MassKeys` cross-product and its sole read in `Measure.Mass`.

From

```csharp
private static readonly Lazy<FrozenDictionary<(MassKind Mass, MassProperty Property), Op>> MassKeys = new(static () =>
    MassKind.Items.SelectMany(static _ => MassProperty.Items, static (mass, property) => (Mass: mass, Property: property))
        .ToFrozenDictionary(static row => row, static row => Op.Of(name: $"{row.Mass.Label}{row.Property.Label}")));
Op key = MassKeys.Value[(mass, property)];
```

To

```csharp
// MassKeys DELETED
Op key = Op.Of(name: $"{mass.Key}{property.Key}");
```

Why: The frozen dictionary stores the full Cartesian product of values derived only from the requested rows' generated keys, then serves one read site. `Op` identity is its string value, not object or cache identity, so equal direct construction preserves operation identity without a second roster.

Change: Delete `MassKeys` and derive the requested operation key directly from the admitted `MassKind` and `MassProperty` rows.

Delta: -3 LOC; -1 module-level field; type count neutral.

Ripples: Remove the `MassKeys` operation-identity law from the measure prose. After the bounds-gate roster is removed, delete the now-unused `System.Collections.Frozen` import.

# 5. Let Thinktecture own mass computation dispatch

`measure.md` lines 112-151 and 235-271, the raw compute/aggregate fields, forwarding aggregate method, row delegates, and batch helpers.

From

```csharp
aggregate: static (self, geometry, context, demands, op) => SumBatch<AreaMassProperties>(geometry: geometry, context: context, mass: self, demands: demands, op: op, sum: static (total, summands) => total.Sum(summands: summands, bAddTo: true)),
private readonly Func<object, Context, CapabilitySet<MomentDemand>, Op, Fin<IDisposable>> compute;
private readonly Func<MassKind, IEnumerable<object>, Context, CapabilitySet<MomentDemand>, Op, Fin<IDisposable>> aggregate;
internal Fin<IDisposable> Aggregate(IEnumerable<object> geometry, Context context, CapabilitySet<MomentDemand> demands, Op op) =>
    aggregate(this, geometry, context, demands, op);
private static Fin<IDisposable> LengthBatch(MassKind self, IEnumerable<object> geometry, Context context, CapabilitySet<MomentDemand> demands, Op op) =>
private static Fin<IDisposable> SumBatch<TMass>(IEnumerable<object> geometry, Context context, MassKind mass, CapabilitySet<MomentDemand> demands, Op op, Func<TMass, IEnumerable<TMass>, bool> sum) where TMass : class, IDisposable {
private static (Seq<IDisposable> Owned, Option<Error> Refused) Acquire(IEnumerable<object> geometry, Context context, MassKind mass, CapabilitySet<MomentDemand> demands, Op op) =>
mass.compute(item, context, demands, op).Match(
```

To

```csharp
aggregate: static (geometry, context, demands, op) => SumBatch<AreaMassProperties>(geometry: geometry, context: context, compute: AreaMass, demands: demands, op: op, sum: static (total, summands) => total.Sum(summands: summands, bAddTo: true)),
[UseDelegateFromConstructor] internal partial Fin<IDisposable> Compute(object geometry, Context context, CapabilitySet<MomentDemand> demands, Op op);
[UseDelegateFromConstructor] internal partial Fin<IDisposable> Aggregate(IEnumerable<object> geometry, Context context, CapabilitySet<MomentDemand> demands, Op op);
private static Fin<IDisposable> LengthBatch(IEnumerable<object> geometry, Context context, CapabilitySet<MomentDemand> demands, Op op) =>
private static Fin<IDisposable> SumBatch<TMass>(IEnumerable<object> geometry, Context context, Func<object, Context, CapabilitySet<MomentDemand>, Op, Fin<IDisposable>> compute, CapabilitySet<MomentDemand> demands, Op op, Func<TMass, IEnumerable<TMass>, bool> sum) where TMass : class, IDisposable {
private static (Seq<IDisposable> Owned, Option<Error> Refused) Acquire(IEnumerable<object> geometry, Context context, Func<object, Context, CapabilitySet<MomentDemand>, Op, Fin<IDisposable>> compute, CapabilitySet<MomentDemand> demands, Op op) =>
compute(item, context, demands, op).Match(
```

Why: Two raw delegate fields plus a forwarding method duplicate the generated delegate-backed method shape already used by the same owner. Passing `MassKind self` into aggregation exists only to recover the compute delegate selected by that row.

Change: Generate `Compute` and `Aggregate` from constructor delegates; pass `LengthMass`, `AreaMass`, or `VolumeMass` directly into `SumBatch` and `Acquire`; remove the `MassKind` parameter from `LengthBatch`, `SumBatch`, `Acquire`, and `Summed`; replace remaining `.compute` reads with `.Compute` and use `typeof(TMass).Name` for the failed-sum detail.

Delta: -2 LOC; -1 module-level member; type count neutral.

# 6. Stop re-admitting non-null measure inputs

`measure.md` lines 174-203, the outer nullable lifts in `LengthOf` and `CentroidOf`.

From

```csharp
Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
    _ => Fin.Fail<double>(op.Unsupported(g.GetType(), typeof(double))),
});
Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
    _ => Fin.Fail<Point3d>(op.Unsupported(g.GetType(), typeof(Point3d))),
});
```

To

```csharp
geometry switch {
    _ => Fin.Fail<double>(op.Unsupported(geometry.GetType(), typeof(double))),
};
geometry switch {
    _ => Fin.Fail<Point3d>(op.Unsupported(geometry.GetType(), typeof(Point3d))),
};
```

Why: Both methods constrain `TGeometry : notnull`; the nullable lift cannot refuse and only re-wraps an already-admitted value before shape dispatch.

Change: Switch directly on `geometry`, update the interior arm expressions from `g` to `geometry`, and preserve the unsupported-type failure.

Delta: -2 LOC; module-level symbol/member/type counts neutral.

# 7. Repair native mass-handle admission

`measure.md` lines 212-238, `Done<TMass>` and every length, area, volume, and homogeneous-batch compute call.

From

```csharp
private static Fin<IDisposable> Done<TMass>(TMass? mass) where TMass : class, IDisposable =>
    Optional(mass).ToFin(op.InvalidResult($"mass properties unavailable for {typeof(TMass).Name}")).Map(static handle => (IDisposable)handle);
Done(LengthMassProperties.Compute(curve, length: true, firstMoments: demands.Admits(MomentDemand.First), secondMoments: demands.Admits(MomentDemand.Second), productMoments: demands.Admits(MomentDemand.Product)))
```

To

```csharp
private static Fin<IDisposable> AdmitHandle<TMass>(TMass? mass, Op op) where TMass : class, IDisposable =>
    Optional(mass).ToFin(op.InvalidResult(detail: $"mass properties unavailable for {typeof(TMass).Name}")).Map(static handle => (IDisposable)handle);
AdmitHandle(LengthMassProperties.Compute(curve, length: true, firstMoments: demands.Admits(MomentDemand.First), secondMoments: demands.Admits(MomentDemand.Second), productMoments: demands.Admits(MomentDemand.Product)), op: op)
```

Why: The static helper reads `op` outside its scope, so the fence cannot carry operation identity into a host-null refusal. `Done` also does not name the admission it performs.

Change: Rename the helper, add the trailing `Op` parameter, and pass the already-carried operation from every native mass-property compute call.

Delta: LOC and module-level symbol/member/type counts neutral.

# 8. Remove fabricated capability ranks

`measure.md` lines 43-49 and 643-649, the `MomentDemand` and `ResidualTrait` row arguments and properties.

From

```csharp
public static readonly MomentDemand First = new(key: "first", rank: 0);
public static readonly MomentDemand Second = new(key: "second", rank: 1);
public static readonly MomentDemand Product = new(key: "product", rank: 2);
public int Rank { get; }
public static readonly ResidualTrait Signed = new(key: "signed", rank: 0);
public static readonly ResidualTrait Containment = new(key: "containment", rank: 1);
public static readonly ResidualTrait Exact = new(key: "exact", rank: 2);
public int Rank { get; }
```

To

```csharp
public static readonly MomentDemand First = new(key: "first");
public static readonly MomentDemand Second = new(key: "second");
public static readonly MomentDemand Product = new(key: "product");
// MomentDemand.Rank DELETED
public static readonly ResidualTrait Signed = new(key: "signed");
public static readonly ResidualTrait Containment = new(key: "containment");
public static readonly ResidualTrait Exact = new(key: "exact");
// ResidualTrait.Rank DELETED
```

Why: First, second, and product moments are selectable demands, not a precedence chain; signed, containment, and exact are orthogonal residual traits. No consumer reads either rank, and capability wire order already comes from keys.

Change: Remove both rank columns and all constructor arguments.

Delta: -2 LOC; -2 module-level members; type count neutral.

# 9. Remove simple `Bounds` construction forwarders

`measure.md` lines 430-441, the parameter-free and one-argument `Bounds` factories.

From

```csharp
public static Bounds AxisAligned => new AxisAlignedCase();
public static Bounds Oriented(Plane plane) => new InPlaneCase(Plane: plane);
public static Bounds Transformed(Transform transform) => new TransformedCase(Xform: transform);
public static Bounds Principal => new PrincipalFrameCase();
public static Bounds Center => new CenterCase();
public static Bounds Edges => new EdgesCase();
public static Bounds Area => new AreaCase();
public static Bounds Volume => new VolumeCase();
public static Bounds Diagonal => new DiagonalCase();
public static Bounds AspectRatio => new AspectRatioCase();
public static Bounds Tightness => new TightnessCase();
```

To

```csharp
// Bounds.AxisAligned DELETED
// Bounds.Oriented DELETED
// Bounds.Transformed DELETED
// Bounds.Principal DELETED
// Bounds.Center DELETED
// Bounds.Edges DELETED
// Bounds.Area DELETED
// Bounds.Volume DELETED
// Bounds.Diagonal DELETED
// Bounds.AspectRatio DELETED
// Bounds.Tightness DELETED
```

Why: Each member is a policy-free alias for a public case constructor. `Corners` and the enclosing builders remain because they normalize optional policy.

Change: Construct the corresponding case directly and retain `Corners`, `EnclosingSphere`, `EnclosingCircle`, and `EnclosingCylinder`.

Delta: -11 LOC; -11 module-level members; type count neutral.

Ripples: In `libs/dotnet/Rasm/.planning/Analysis/query.md`, replace `Analysis.Bounds.AxisAligned` with `new Analysis.Bounds.AxisAlignedCase()` and update its defaulting prose. In `libs/dotnet/Rasm.Fabrication/.planning/Kinematics/fleet.md`, replace `Bounds.AxisAligned` with `new Bounds.AxisAlignedCase()`. No other deleted builder has a consumer.

# 10. Canonicalize oriented bounds

`measure.md` lines 416, 453, and 481-483, the oriented case, its admission row, and generated switch arm.

From

```csharp
public sealed record InPlaneCase(Plane Plane) : Bounds;
[typeof(InPlaneCase)] = Gate<Box>(name: nameof(InPlaneCase), ingress: static type => typeof(GeometryBase).IsAssignableFrom(c: type)),
inPlaneCase: static p => p.Admitted<TGeometry, TOut>(build: key =>
```

To

```csharp
public sealed record OrientedCase(Plane Plane) : Bounds;
[typeof(OrientedCase)] = Gate<Box>(name: nameof(OrientedCase), ingress: static type => typeof(GeometryBase).IsAssignableFrom(c: type)),
orientedCase: static p => p.Admitted<TGeometry, TOut>(build: key =>
```

Why: The public construction vocabulary says oriented while the case and derived operation key say in-plane. Oriented bounding box is the established domain term and keeps one modality under one name.

Change: Rename the case, generated switch arm, admission row, and resulting operation key together.

Delta: LOC and module-level symbol/member/type counts neutral.

# 11. Replace fake sampling provenance with a typed corner fallback

`measure.md` lines 405-448 and 524-588, `SampleSource`, the enclosing cases/builders, and the sampling fold.

From

```csharp
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SampleSource : ICapability<SampleSource> {
    public static readonly SampleSource Measured = new(key: "measured", rank: 0);
    public static readonly SampleSource BoxCorners = new(key: "box-corners", rank: 1);
    public int Rank { get; }
}
public sealed record EnclosingSphereCase(Option<int> Count, CapabilitySet<SampleSource> Sources) : Bounds;
public static Bounds EnclosingSphere(Option<int> count = default, Option<CapabilitySet<SampleSource>> sources = default) =>
    new EnclosingSphereCase(Count: count, Sources: sources.IfNone(MeasuredOnly));
private static CapabilitySet<SampleSource> MeasuredOnly => CapabilitySet<SampleSource>.Of(SampleSource.Measured);
private static Fin<(Seq<Point3d> Sites, SampleSource Source)> Enclosing<TGeometry>(TGeometry geometry, Option<int> count, CapabilitySet<SampleSource> sources, Context context, Op key) where TGeometry : notnull =>
    geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: count.IfNone(() => Budget(box: box, context: context)), Model: context), key: key)
        .Map(static sites => (Sites: sites, Source: SampleSource.Measured))
        .BindFail(error => (error, sources.Admits(capability: SampleSource.BoxCorners)) switch {
            (KernelFault.Unsupported, true) => Fin.Succ((Sites: toSeq(box.GetCorners()), Source: SampleSource.BoxCorners)),
            _ => Fin.Fail<(Seq<Point3d> Sites, SampleSource Source)>(error),
        });
```

To

```csharp
// SampleSource DELETED
public sealed record EnclosingSphereCase(Option<int> Count, Option<CornerSet> Fallback) : Bounds;
public static Bounds EnclosingSphere(Option<int> count = default, Option<CornerSet> fallback = default) =>
    new EnclosingSphereCase(Count: count, Fallback: fallback);
// Bounds.MeasuredOnly DELETED
private static Fin<Seq<Point3d>> Enclosing<TGeometry>(TGeometry geometry, Option<int> count, Option<CornerSet> fallback, Context context, Op key) where TGeometry : notnull =>
    geometry.Evaluate<Seq<Point3d>>(request: new EvaluationRequest.Sample(Count: count.IfNone(() => Budget(box: box, context: context)), Model: context), key: key)
        .BindFail(error => (error, fallback.Case) switch {
            (KernelFault.Unsupported, CornerSet set) => Fin.Succ(set.Read(box: box, band: context.For(lane: ToleranceLane.Weld))),
            _ => Fin.Fail<Seq<Point3d>>(error),
        });
```

Why: Measured evaluation always runs, so `Measured` controls nothing; only box-corner membership is inspected. The returned `Source` is discarded by every fit, making the claimed provenance a dead carrier. `Option<CornerSet>` directly models whether and how unsupported sampling may fall back.

Change: Delete `SampleSource` and `MeasuredOnly`; change all three enclosing cases and builders from `Sources` to `Fallback`; pass the option through each switch arm; return sites directly and replace every `sites.Sites` read with `sites`.

Delta: -8 LOC; -1 module-level type; -4 module-level members; remaining case-member count neutral.

Ripples: Rewrite the bounds owner, auto, growth, and boundary prose to describe optional typed corner fallback and remove claims that sampling provenance exits the operation. No external enclosing builder or `SampleSource` consumer exists.

# 12. Collapse the mirrored bounds-gate roster

`measure.md` lines 450-475 and 477-574, `BoundsGate`, `Gates`, `Gate`, `Admitted`, and their switch-arm call sites.

From

```csharp
private readonly record struct BoundsGate(OutputBinding Output, Func<Type, bool> Ingress, Op Key);
private static readonly Lazy<FrozenDictionary<Type, BoundsGate>> Gates = new(static () => new Dictionary<Type, BoundsGate> {
    [typeof(AxisAlignedCase)] = Gate<BoundingBox>(name: nameof(AxisAlignedCase), ingress: static type => Capability.Bound.Admits(type: type)),
    [typeof(AreaCase)] = Gate<double>(name: nameof(AreaCase), ingress: BoxValue),
    [typeof(EnclosingCylinderCase)] = Gate<Cylinder>(name: nameof(EnclosingCylinderCase), ingress: static type => Capability.Bound.Admits(type: type)),
}.ToFrozenDictionary());
private static BoundsGate Gate<TOut>(string name, Func<Type, bool> ingress) =>
    new(Output: OutputBinding.Of<TOut>(), Ingress: ingress, Key: Op.Of(name: name[..^"Case".Length]));
private Operation<TGeometry, TOut> Admitted<TGeometry, TOut>(Func<Op, Operation<TGeometry, TOut>> build) where TGeometry : notnull =>
    Gates.Value[GetType()] switch {
        BoundsGate gate when gate.Output.Serves<TOut>() && gate.Ingress(arg: typeof(TGeometry)) => build(arg: gate.Key),
        BoundsGate gate => gate.Key.Unsupported<TGeometry, TOut>(),
    };
axisAlignedCase: static c => c.Admitted<TGeometry, TOut>(build: static key =>
areaCase: static c => c.Metric<TGeometry, TOut>(boundingBox: static (box, _) => box.Area, box: static (box, _) => box.Area),
```

To

```csharp
private static Operation<TGeometry, TOut> Admit<TGeometry, TOut>(string caseName, Func<Type, bool> ingress, Func<Op, Operation<TGeometry, TOut>> build) where TGeometry : notnull {
    Op key = Op.Of(name: caseName[..^"Case".Length]);
    return ingress(arg: typeof(TGeometry)) ? build(arg: key) : key.Unsupported<TGeometry, TOut>();
}
axisAlignedCase: static _ => Admit<TGeometry, TOut>(caseName: nameof(AxisAlignedCase), ingress: static type => Capability.Bound.Admits(type: type), build: static key =>
areaCase: static _ => Metric<TGeometry, TOut>(caseName: nameof(AreaCase), boundingBox: static (box, _) => box.Area, box: static (box, _) => box.Area),
```

Why: The frozen table mirrors every generated union case, yet each arm already knows its case, ingress rule, concrete output builder, and exhaustive dispatch position. `OperationLift.As<TGeometry,TOut>` is the existing output-admission boundary, so the roster's output bindings are duplicate authority.

Change: Delete `BoundsGate`, `Gates`, `Gate`, and `Admitted`; pass `nameof(Case)` and the existing ingress predicate from each generated switch arm into `Admit`; add `caseName` to `Metric` and route it through `Admit`; retain `BoxValue` for the four box metrics.

Delta: -20 LOC; -1 nested type; -2 module-level members.

Ripples: Remove the frozen-roster and duplicate output-binding claims from the bounds entry prose. The operation keys remain the case names without the generated `Case` suffix.

# 13. Inline the single-use sampling budget

`measure.md` lines 579-590, the absent-count projection inside `Enclosing` and `Budget`.

From

```csharp
Count: count.IfNone(() => Budget(box: box, context: context))
private static int Budget(BoundingBox box, Context context) =>
    (int)Math.Clamp(value: Math.Ceiling(a: box.Diagonal.Length / context.For(lane: ToleranceLane.Chord).Value), min: SampleFloor, max: SampleCeiling);
```

To

```csharp
Count: count.IfNone(() => (int)Math.Clamp(value: Math.Ceiling(a: box.Diagonal.Length / context.For(lane: ToleranceLane.Chord).Value), min: SampleFloor, max: SampleCeiling))
// Bounds.Budget DELETED
```

Why: `Budget` is called once and only forwards its two inputs into the expression that supplies the missing sample count.

Change: Inline the chord-band derivation at `count.IfNone` and retain the named floor and ceiling constants.

Delta: -1 LOC; -1 module-level method; type count neutral.

# 14. Let Thinktecture own conformance projection dispatch

`measure.md` lines 653-684, the row delegate arguments, hand-declared delegate type/property, and projection call.

From

```csharp
public static readonly ConformanceMetric Distance = new(key: 0, output: OutputBinding.Of<double>(), traits: CapabilitySet<ResidualTrait>.None,
    projection: static (residuals, _, _, _) => Fin.Succ(residuals.Map(static sample => (object)sample.Distance)));
internal delegate Fin<Seq<object>> ConformanceProjection(Seq<ResidualSample> residuals, Seq<double> percentiles, Tolerance band, Op key);
internal ConformanceProjection Projection { get; }
.Bind(stream => Projection(residuals: stream.Samples, percentiles: percentiles, band: stream.Band, key: key))
```

To

```csharp
public static readonly ConformanceMetric Distance = new(key: 0, output: OutputBinding.Of<double>(), traits: CapabilitySet<ResidualTrait>.None,
    project: static (residuals, _, _, _) => Fin.Succ(residuals.Map(static sample => (object)sample.Distance)));
[UseDelegateFromConstructor] private partial Fin<Seq<object>> Project(Seq<ResidualSample> residuals, Seq<double> percentiles, Tolerance band, Op key);
.Bind(stream => Project(residuals: stream.Samples, percentiles: percentiles, band: stream.Band, key: key))
```

Why: The hand-declared delegate type and stored property reproduce Thinktecture's generated delegate-backed method surface already used by other policy owners on this page.

Change: Rename every row argument to `project:`, replace the delegate type/property with one `[UseDelegateFromConstructor]` partial method, and call it.

Delta: -1 LOC; -1 nested delegate type; member count neutral.

# 15. Inline the single-use band admission

`measure.md` lines 680-703, `ConformanceMetric.Project<TOut>` and `Banded`.

From

```csharp
.Bind(admitted => Banded(samples: admitted, key: key).Map(band => (Samples: admitted, Band: band)))
.Bind(stream => Project(residuals: stream.Samples, percentiles: percentiles, band: stream.Band, key: key))
private static Fin<Tolerance> Banded(Seq<ResidualSample> samples, Op key) =>
    samples.Map(static sample => sample.Band).Distinct() switch {
        Seq<Tolerance> bands when bands.Count == 1 => Fin.Succ(bands[0]),
        _ => Fin.Fail<Tolerance>(key.InvalidInput()),
    };
```

To

```csharp
.Bind(admitted => admitted.Map(static sample => sample.Band).Distinct() switch {
    Seq<Tolerance> bands when bands.Count == 1 => Project(residuals: admitted, percentiles: percentiles, band: bands[0], key: key),
    _ => Fin.Fail<Seq<object>>(key.InvalidInput()),
})
// ConformanceMetric.Banded DELETED
```

Why: The admitted band exists only to feed the immediately following projection. The inline switch keeps the one-band invariant and its dependent continuation in one result pipeline.

Change: Bind the delegate-backed projection directly from the distinct-band switch and delete the helper and intermediate tuple.

Delta: -2 LOC; -1 module-level method; type count neutral.

# 16. Inline row-local conformance projections

`measure.md` lines 661-668 and 706-710, the `Maximum` and `Distribution` rows plus `Worst` and `Spread`.

From

```csharp
project: static (residuals, _, band, key) => Worst(samples: residuals, band: band, key: key).Map(static sample => Seq((object)sample)));
project: static (residuals, percentiles, band, key) => Spread(samples: residuals, percentiles: percentiles, band: band, key: key).Map(static result => Seq((object)result)));
private static Fin<ResidualSample> Worst(Seq<ResidualSample> samples, Tolerance band, Op key) =>
    Stat.Extrema(items: samples, projection: static sample => Math.Abs(sample.Distance), band: band, direction: ExtremumDirection.Maximum)
        .Head.ToFin(key.InvalidResult());
private static Fin<Distribution<Scalar>> Spread(Seq<ResidualSample> samples, Seq<double> percentiles, Tolerance band, Op key) =>
    Distribution<Scalar>.Of(values: samples.Map(static sample => (Scalar)sample.Distance), percentiles: percentiles, key: key, context: Some((StatContext)band));
```

To

```csharp
project: static (residuals, _, band, key) =>
    Stat.Extrema(items: residuals, projection: static sample => Math.Abs(sample.Distance), band: band, direction: ExtremumDirection.Maximum)
        .Head.ToFin(key.InvalidResult()).Map(static sample => Seq((object)sample)));
project: static (residuals, percentiles, band, key) =>
    Distribution<Scalar>.Of(values: residuals.Map(static sample => (Scalar)sample.Distance), percentiles: percentiles, key: key, context: Some((StatContext)band))
        .Map(static result => Seq((object)result)));
// ConformanceMetric.Worst DELETED
// ConformanceMetric.Spread DELETED
```

Why: Each helper is called by exactly one policy row and owns no behavior outside that row.

Change: Put each package-level projection directly on its `ConformanceMetric` row and delete both helpers; retain `Moments`, which serves three rows.

Delta: -2 LOC; -2 module-level methods; type count neutral.

# 17. Collapse conformance admission onto its policy row

`measure.md` lines 673-677, 686-690, and 711-714, `AcceptsTarget`, the sampled admission call, and the one-call `Admits` helper.

From

```csharp
internal bool AcceptsTarget(Type geometry, Type target) =>
    (Traits.Admits(ResidualTrait.Containment) && Capability.EvaluateTopology.Admits(type: target))
    || (Traits.Admits(ResidualTrait.Signed) && !Traits.Admits(ResidualTrait.Containment) && Capability.SignedDistance.Admits(type: target))
    || (!Traits.Admits(ResidualTrait.Signed) && (Capability.Closest.Admits(type: target)
        || (Capability.CurveForm.Admits(type: geometry) && Capability.CurveForm.Admits(type: target))));
(count.Filter(static budget => budget > 0).Case, Admits(metric: metric, geometry: typeof(TGeometry), target: typeof(TTarget)) && metric.Output.Serves<TOut>()) switch {
private static bool Admits(ConformanceMetric metric, Type geometry, Type target) =>
    Capability.Universal(type: geometry) || Capability.Universal(type: target)
    || (Capability.CurveForm.Admits(type: geometry) && metric.AcceptsTarget(geometry: geometry, target: target))
    || (Capability.SurfaceForm.Admits(type: geometry) && metric.AcceptsTarget(geometry: geometry, target: target));
```

To

```csharp
internal bool Admits(Type geometry, Type target) =>
    Capability.Universal(type: geometry) || Capability.Universal(type: target)
    || ((Capability.CurveForm.Admits(type: geometry) || Capability.SurfaceForm.Admits(type: geometry))
        && ((Traits.Admits(ResidualTrait.Containment) && Capability.EvaluateTopology.Admits(type: target))
            || (Traits.Admits(ResidualTrait.Signed) && !Traits.Admits(ResidualTrait.Containment) && Capability.SignedDistance.Admits(type: target))
            || (!Traits.Admits(ResidualTrait.Signed) && (Capability.Closest.Admits(type: target)
                || (Capability.CurveForm.Admits(type: geometry) && Capability.CurveForm.Admits(type: target))))));
(count.Filter(static budget => budget > 0).Case, metric.Admits(geometry: typeof(TGeometry), target: typeof(TTarget)) && metric.Output.Serves<TOut>()) switch {
```

Why: The static helper is called once and only gates geometry shape before forwarding to the metric row. Keeping the full admission predicate on the row removes one hop without changing universal, curve, surface, target, or trait semantics.

Change: Fold the geometry gate into a single row-owned `Admits` method, update `Sampled`, and delete `AcceptsTarget` plus the static helper.

Delta: -1 LOC; -1 module-level method; type count neutral.
