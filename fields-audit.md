# 1. Make the draw lane its generated key

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:114-119` — anchor `[SmartEnum<int>] public sealed partial class FieldLane : IDrawLane<FieldLane>`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class FieldLane : IDrawLane<FieldLane> {
    public static readonly FieldLane Noise = new(key: 0, lane: 0L);
    public long Lane { get; }
}
```

### To

```csharp
[SmartEnum<long>(KeyMemberName = nameof(IDrawLane<FieldLane>.Lane))]
public sealed partial class FieldLane : IDrawLane<FieldLane> {
    public static readonly FieldLane Noise = new(0L);
}
```

### Why

The row currently publishes the same deterministic address twice as `Key` and `Lane`. Naming the generated long key `Lane` keeps the real lane vocabulary, `Items`, lookup, ordering, and `IDrawLane<FieldLane>` conformance while deleting the duplicate constructor column and authored property.

# 2. Remove unearned keys from internal vocabularies

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:63-69,130-136` — anchors `CsgKind` and `ProfileExtrusionFeature`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(key: 0, combine: static (a, b, blend) => blend.Smin(a: a, b: b));
    public static readonly CsgKind Intersect = new(key: 1, combine: static (a, b, blend) => -blend.Smin(a: -a, b: -b));
    public static readonly CsgKind Difference = new(key: 2, combine: static (a, b, blend) => -blend.Smin(a: -a, b: b));
}
```

```csharp
[SmartEnum<int>]
public sealed partial class ProfileExtrusionFeature {
    public static readonly ProfileExtrusionFeature Interior = new(key: 0);
    public static readonly ProfileExtrusionFeature ProfileBoundary = new(key: 1);
    public static readonly ProfileExtrusionFeature Cap = new(key: 2);
    public static readonly ProfileExtrusionFeature Rim = new(key: 3);
}
```

### To

```csharp
[SmartEnum]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(combine: static (a, b, blend) => blend.Smin(a, b));
    public static readonly CsgKind Intersect = new(combine: static (a, b, blend) => -blend.Smin(-a, -b));
    public static readonly CsgKind Difference = new(combine: static (a, b, blend) => -blend.Smin(-a, b));
}
```

```csharp
[SmartEnum]
public sealed partial class ProfileExtrusionFeature {
    public static readonly ProfileExtrusionFeature Interior = new();
    public static readonly ProfileExtrusionFeature ProfileBoundary = new();
    public static readonly ProfileExtrusionFeature Cap = new();
    public static readonly ProfileExtrusionFeature Rim = new();
}
```

### Why

Neither family admits, persists, orders, or transmits an integer. `CsgKind` is consumed through its delegate column and `ProfileExtrusionFeature` through singleton identity, so keyless smart enums retain the closed vocabularies while removing unused lookup, parse, format, and conversion surface.

# 3. Carry blend erosion once on the closed base

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:33-45,60,295` — anchors `BlendKind`, its `ErosionFactor` overrides, `BlendKind.Erode`, and `ScalarField.CsgCase.LipschitzBound`.

### From

```csharp
private BlendKind() { }
public sealed record HardCase : BlendKind { public override double ErosionFactor => 1.00; }
public sealed record PolynomialCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.25; }
public sealed record ExponentialCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.15; }
public sealed record RootCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.10; }
```

```csharp
public sealed record CubicCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.30; }
public sealed record ChamferCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.50; }
public sealed record GrooveCase(PositiveMagnitude K, PositiveMagnitude D) : BlendKind { public override double ErosionFactor => 1.40; }
public sealed record RoundCase(PositiveMagnitude R) : BlendKind { public override double ErosionFactor => 1.20; }
public abstract double ErosionFactor { get; }
```

### To

```csharp
private BlendKind(double erosionFactor) => ErosionFactor = erosionFactor;
public sealed record HardCase : BlendKind(1.00);
public sealed record PolynomialCase(PositiveMagnitude K) : BlendKind(1.25);
public sealed record ExponentialCase(PositiveMagnitude K) : BlendKind(1.15);
public sealed record RootCase(PositiveMagnitude K) : BlendKind(1.10);
```

```csharp
public sealed record CubicCase(PositiveMagnitude K) : BlendKind(1.30);
public sealed record ChamferCase(PositiveMagnitude K) : BlendKind(1.50);
public sealed record GrooveCase(PositiveMagnitude K, PositiveMagnitude D) : BlendKind(1.40);
public sealed record RoundCase(PositiveMagnitude R) : BlendKind(1.20);
public double ErosionFactor { get; }
```

```csharp
// BlendKind.Erode DELETED
```

```csharp
select Smoothing.ErosionFactor * Math.Max(l, r);
```

### Why

Erosion is one immutable column every case supplies, not eight behaviors. The private base constructor keeps that column compulsory at case declaration, deletes eight overrides and the abstract slot, and lets the only consumer inline the one-call `Erode` wrapper.

# 4. Carry primitive bounds once on the closed base

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:179-227` — anchor `SdfKind`, every case-level `Lipschitz` override, and the abstract declaration.

### From

```csharp
private SdfKind() { }
public sealed record SphereCase(PositiveMagnitude Radius) : SdfKind {
    public override double Lipschitz => 1.0;
    internal override double Distance(Point3d p) => Math.Sqrt((p.X * p.X) + (p.Y * p.Y) + (p.Z * p.Z)) - Radius.Value;
}
```

```csharp
public sealed record CappedConeCase(PositiveMagnitude HalfHeight, double R1, double R2) : SdfKind {
    public override double Lipschitz => 1.2;
    internal override double Distance(Point3d p) => CappedCone(p: p, halfHeight: HalfHeight.Value, r1: R1, r2: R2);
}
public sealed record EllipsoidCase(PositiveMagnitude X, PositiveMagnitude Y, PositiveMagnitude Z) : SdfKind {
    public override double Lipschitz => 2.0;
```

### To

```csharp
private SdfKind(double lipschitz) => Lipschitz = lipschitz;
public sealed record SphereCase(PositiveMagnitude Radius) : SdfKind(1.0) {
    internal override double Distance(Point3d p) => Math.Sqrt((p.X * p.X) + (p.Y * p.Y) + (p.Z * p.Z)) - Radius.Value;
}
```

```csharp
public sealed record CappedConeCase(PositiveMagnitude HalfHeight, double R1, double R2) : SdfKind(1.2) {
    internal override double Distance(Point3d p) => CappedCone(p, HalfHeight.Value, R1, R2);
}
public sealed record EllipsoidCase(PositiveMagnitude X, PositiveMagnitude Y, PositiveMagnitude Z) : SdfKind(2.0) {
```

```csharp
public sealed record BoxCase(PositiveMagnitude X, PositiveMagnitude Y, PositiveMagnitude Z) : SdfKind(1.0) {
public sealed record CapsuleCase(PositiveMagnitude HalfHeight, PositiveMagnitude Radius) : SdfKind(1.0) {
public sealed record CylinderCase(PositiveMagnitude HalfHeight, PositiveMagnitude Radius) : SdfKind(1.0) {
public sealed record ConeCase(PositiveMagnitude Height, VectorAngle HalfAngle) : SdfKind(1.0) {
public sealed record HalfSpaceCase : SdfKind(1.0) {
```

```csharp
public sealed record TorusCase(PositiveMagnitude Major, PositiveMagnitude Minor) : SdfKind(1.0) {
public sealed record HexPrismCase(PositiveMagnitude HalfHeight, PositiveMagnitude Circumradius) : SdfKind(1.0) {
public sealed record OctahedronCase(PositiveMagnitude S) : SdfKind(1.0) {
public sealed record SlabCase(PositiveMagnitude HalfHeight) : SdfKind(1.0) {
```

```csharp
public double Lipschitz { get; }
internal abstract double Distance(Point3d local);
```

### Why

The twelve cases choose among only three immutable bounds while their distinct distance kernels remain case behavior. Supplying the bound through the sealed base removes every property override and the abstract property without adding an exhaustive mirror; a new primitive still cannot compile until it declares a bound.

# 5. Make provenance cases carry their evidence

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:121-136,267-275` — anchors `SdfStatus`, `FieldSample`, and `SdfSample`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class SdfStatus {
    public static readonly SdfStatus Analytic = new(key: 0);
    public static readonly SdfStatus ComposedAnalytic = new(key: 1);
    public static readonly SdfStatus NativeProfile = new(key: 2);
    public static readonly SdfStatus MeshApproximate = new(key: 3);
    public static readonly SdfStatus Reconstruction = new(key: 4);
}
```

```csharp
public readonly record struct FieldSample(
    double Value, SdfStatus Status, Option<SdfSolve> Mesh,
    Option<SampleFit> Reconstruction);
public readonly record struct SdfSample(
    double Value, SdfStatus Status, Option<double> LipschitzBound, Option<SdfSolve> Mesh,
    Option<ProfileExtrusionFeature> ProfileFeature, Option<PointContainment> ProfileContainment);
```

### To

```csharp
[Union]
public abstract partial record SdfStatus {
    private SdfStatus() { }
    public sealed record AnalyticCase(bool Composed) : SdfStatus;
    public sealed record NativeProfileCase(ProfileExtrusionFeature Feature, PointContainment Containment) : SdfStatus;
    public sealed record MeshApproximateCase(SdfSolve Solve) : SdfStatus;
    public sealed record ReconstructionCase(SampleFit Fit) : SdfStatus;
}
```

```csharp
public readonly record struct FieldSample(double Value, SdfStatus Status);
public readonly record struct SdfSample(
    double Value, SdfStatus Status, Option<double> LipschitzBound);
```

### Why

The flat records permit impossible combinations: a mesh status without a solve, reconstruction evidence on an analytic sample, and half of a profile pair. The provenance family already discriminates those evidence shapes, so moving each payload into its case removes five optional columns and the parallel status/evidence state while retaining the separate general-sample and distance-certified outputs.

# 6. Replace field-blend rows with one case column

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:71-82,293,347-349,496,518-523` — anchors `FieldBlend`, scalar and tensor `BlendCase`, scalar addition, and tensor blend sampling.

### From

```csharp
[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static count => 1.0 / count);
    [UseDelegateFromConstructor] internal partial double Scale(int count);
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) => CombineCore(values: vectors, zero: Vector3d.Zero, add: static (s, v) => s + v, scale: static (s, f) => s * f, key: key);
    internal Fin<double> CombineScalar(Seq<double> values, Op key) => CombineCore(values: values, zero: 0.0, add: static (s, v) => s + v, scale: static (s, f) => s * f, key: key);
    private Fin<T> CombineCore<T>(Seq<T> values, T zero, Func<T, T, T> add, Func<T, double, T> scale, Op key) =>
        from _ in guard(!values.IsEmpty, key.InvalidResult())
        from value in key.AcceptValue(value: scale(values.Fold(zero, add), Scale(values.Count)))
        select value;
}
```

```csharp
public sealed record BlendCase(Seq<ScalarField> Fields, FieldBlend Mode) : ScalarField { public override Option<double> LipschitzBound => Fields.TraverseM(static f => f.LipschitzBound).As().Map(bounds => bounds.Fold(0.0, static (acc, bound) => acc + bound) * Mode.Scale(count: bounds.Count)); }
public sealed record BlendCase(Seq<TensorField> Fields, FieldBlend Mode) : TensorField;
```

### To

```csharp
// FieldBlend DELETED
```

```csharp
public sealed record BlendCase(Seq<ScalarField> Fields, bool Average) : ScalarField { public override Option<double> LipschitzBound => Fields.TraverseM(static f => f.LipschitzBound).As().Filter(static bounds => !bounds.IsEmpty).Map(bounds => bounds.Fold(0.0, static (sum, bound) => sum + bound) / (Average ? bounds.Count : 1)); }
public sealed record BlendCase(Seq<TensorField> Fields, bool Average) : TensorField;
```

```csharp
new BlendCase(Fields: (left is BlendCase { Average: false } lb ? lb.Fields : Seq(left))
    .Concat(right is BlendCase { Average: false } rb ? rb.Fields : Seq(right)), Average: false);
```

```csharp
from samples in c.Fields.Traverse(f => f.SampleTensor(s.Sample, s.Context, s.Key).ToValidation()).As().ToFin()
from _ in guard(!samples.IsEmpty && samples.ForAll(m => m.Dimension == samples.Head.Dimension), s.Key.InvalidResult()).ToFin()
let scale = c.Average ? 1.0 / samples.Count : 1.0
let upper = toArr(toSeq(Enumerable.Range(0, samples.Head.Upper.Count))
    .Map(i => scale * samples.Fold(0.0, (sum, matrix) => sum + matrix.Upper[i])))
from blended in SymmetricMatrix.Of(samples.Head.Dimension, upper, s.Key)
select blended));
```

### Why

The two rows run the same fold and differ only by division by count. A boolean on the two owning cases is the closed two-state column, removing one module type, two rows, two forwarding methods, and the private generic helper. Tensor samples are independent, so applicative `Traverse` accumulates their faults before one component-wise fold instead of running a monadic traversal and a nested `Fin` combine for every component.

# 7. Thread scalar bounds through the closed base

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:278-333` — anchor `ScalarField`, every case-level `LipschitzBound` override, and the abstract declaration.

### From

```csharp
[Union]
public abstract partial record ScalarField {
    private ScalarField() { }
    public sealed record ConstantCase(double Value) : ScalarField { public override Option<double> LipschitzBound => Some(0.0); }
    public sealed record DistanceCase(SupportSpace Source, BoundarySense Sense) : ScalarField { public override Option<double> LipschitzBound => Some(1.0); }
    public sealed record PrimitiveCase(SdfKind Shape, Plane Pose) : ScalarField { public override Option<double> LipschitzBound => Some(Shape.Lipschitz); }
```

```csharp
public sealed record BlendCase(Seq<ScalarField> Fields, FieldBlend Mode) : ScalarField {
    public override Option<double> LipschitzBound => Fields.TraverseM(static f => f.LipschitzBound).As()
        .Map(bounds => bounds.Fold(0.0, static (sum, bound) => sum + bound) * Mode.Scale(count: bounds.Count));
}
public sealed record ScaledCase(ScalarField Source, double Scale) : ScalarField { public override Option<double> LipschitzBound => Source.LipschitzBound.Map(l => Math.Abs(Scale) * l); }
```

```csharp
public sealed record PeriodicCase(ScalarField Source, Vector3d Period) : ScalarField { public override Option<double> LipschitzBound => None; }
public sealed record TwistCase(ScalarField Source, double AnglePerUnit, Direction Axis) : ScalarField { public override Option<double> LipschitzBound => None; }
public sealed record BendCase(ScalarField Source, double Curvature, Direction Axis) : ScalarField { public override Option<double> LipschitzBound => None; }
public abstract Option<double> LipschitzBound { get; }
```

### To

```csharp
[Union]
public abstract partial record ScalarField {
    private ScalarField(Option<double> lipschitzBound) => LipschitzBound = lipschitzBound;
    public Option<double> LipschitzBound { get; }
    public sealed record ConstantCase(double Value) : ScalarField(Some(0.0));
    public sealed record DistanceCase(SupportSpace Source, BoundarySense Sense) : ScalarField(Some(1.0));
    public sealed record PrimitiveCase(SdfKind Shape, Plane Pose) : ScalarField(Some(Shape.Lipschitz));
```

```csharp
public sealed record BlendCase(Seq<ScalarField> Fields, bool Average) : ScalarField(
    Fields.TraverseM(static f => f.LipschitzBound).As().Filter(static bounds => !bounds.IsEmpty)
        .Map(bounds => bounds.Fold(0.0, static (sum, bound) => sum + bound) / (Average ? bounds.Count : 1)));
public sealed record ScaledCase(ScalarField Source, double Scale) : ScalarField(
    Source.LipschitzBound.Map(l => Math.Abs(Scale) * l));
```

```csharp
public sealed record PeriodicCase(ScalarField Source, Vector3d Period) : ScalarField(None);
public sealed record TwistCase(ScalarField Source, double AnglePerUnit, Direction Axis) : ScalarField(None);
public sealed record BendCase(ScalarField Source, double Curvature, Direction Axis) : ScalarField(None);
```

```csharp
public sealed record ProfileExtrusionCase(Curve Profile, Plane Plane, PositiveMagnitude HalfHeight) : ScalarField(Some(1.0));
public sealed record WorleyCase(Seq<Point3d> Seeds, Dimension Order) : ScalarField(Some(1.0));
public sealed record MorseCase(Point3d Center, PositiveMagnitude Depth, PositiveMagnitude Width) : ScalarField(Some(Depth.Value / (2.0 * Width.Value)));
public sealed record DensityCase(Point3d Center, PositiveMagnitude Spread, double Strength) : ScalarField(Some(Math.Abs(Strength) * Math.Exp(-0.5) / Spread.Value));
public sealed record PotentialCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : ScalarField(Falloff.SlopeBound.Map(slope => Charges.Fold(0.0, static (sum, charge) => sum + Math.Abs(charge.Charge)) * slope));
public sealed record MollifierCase(Point3d Center, PositiveMagnitude Radius) : ScalarField(None);
```

```csharp
public sealed record CsgCase(ScalarField Left, ScalarField Right, CsgKind Op, BlendKind Smoothing) : ScalarField(from l in Left.LipschitzBound from r in Right.LipschitzBound select Smoothing.ErosionFactor * Math.Max(l, r));
public sealed record DisplaceCase(ScalarField Source, ScalarField Displacement) : ScalarField(from l in Source.LipschitzBound from r in Displacement.LipschitzBound select l + r);
public sealed record ClampCase(ScalarField Source, double Minimum, double Maximum) : ScalarField(Source.LipschitzBound);
public sealed record OnionCase(ScalarField Source, PositiveMagnitude Thickness) : ScalarField(Source.LipschitzBound);
public sealed record SdfRoundCase(ScalarField Source, PositiveMagnitude Radius) : ScalarField(Source.LipschitzBound);
public sealed record ElongateCase(ScalarField Source, Vector3d Extent) : ScalarField(Source.LipschitzBound);
public sealed record PowerCase(ScalarField Source, double Exponent) : ScalarField(None);
```

```csharp
public sealed record MagnitudeCase(VectorField Source) : ScalarField(None);
public sealed record DivergenceCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField(None);
public sealed record LaplacianCase(ScalarField Source, PositiveMagnitude Epsilon) : ScalarField(None);
public sealed record StrainMagnitudeCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField(None);
public sealed record NoiseCase(NoiseKind Kind, NoisePolicy Policy) : ScalarField(None);
public sealed record LatticeCase(CellLattice Grid, Arr<double> Values, LatticeInterpolation Interp) : ScalarField(None);
```

```csharp
public sealed record GeodesicCase(MeshSpace Space, Seq<int> Sources) : ScalarField(None);
public sealed record MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, Dimension Iterations) : ScalarField(None);
public sealed record SpectralDistanceCase(MeshSpace Space, SpectralFilter Filter, Seq<int> Sources, Dimension Pairs) : ScalarField(None);
public sealed record StripeCase(MeshSpace Space, VectorField CrossField, PositiveMagnitude Frequency) : ScalarField(None);
public sealed record SignedDistanceFromMeshCase(MeshSpace Space, SdfMeshPolicy Policy) : ScalarField(None);
```

```csharp
public sealed record RbfCase(Seq<(Point3d Position, double Value)> Samples, KernelKind Kernel, PositiveMagnitude Radius, Arr<double> Coefficients, ReconstructionFit Fit) : ScalarField(None);
public sealed record MlsCase(Seq<MlsSample> Samples, KernelKind Kernel, PositiveMagnitude Radius, ReconstructionFit Fit) : ScalarField(None);
public sealed record LevinMlsCase(Seq<MlsSample> Samples, LevinMlsPolicy Policy, ReconstructionFit Fit) : ScalarField(None);
public sealed record ApssCase(Seq<MlsSample> Samples, ApssPolicy Policy, ReconstructionFit Fit) : ScalarField(None);
public sealed record SibsonCase(NaturalNeighborField Field, Arr<double> Values, ReconstructionFit Fit) : ScalarField(None);
public sealed record PoissonCase(PoissonGrid Grid, double Gamma, PoissonSolve Solve) : ScalarField(None);
```

### Why

The bound is one immutable column every scalar case must answer. Passing it through the private base constructor removes the abstract slot and one override member from every case while still making omission a compile break. Filtering the derived blend bound also prevents an empty average from manufacturing `0 * Infinity`.

# 8. Accumulate independent construction faults

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:103-106,228-236,257-264,339-342` — anchors `BouncePolicy.Refract`, `SdfKind.CappedCone`, `SdfKind.Cone`, `NoisePolicy.Of`, and `ScalarField.Density`.

### From

```csharp
from i in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: etaIncident)
from t in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: etaTransmitted)
select (BouncePolicy)new RefractCase(EtaIncident: i, EtaTransmitted: t);
```

```csharp
from count in key.OrDefault().AcceptValidated<Dimension>(candidate: octaves)
from gain in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: persistence)
from gap in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: lacunarity)
from rate in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: frequency)
select new NoisePolicy(Seed: seed, Lane: lane.IfNone(FieldLane.Noise), Octaves: count,
    Persistence: gain, Lacunarity: gap, Frequency: rate);
```

```csharp
from h in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: halfHeight)
from _ in guard(r1 >= 0.0 && r2 >= 0.0 && (r1 > 0.0 || r2 > 0.0)
    && double.IsFinite(r1) && double.IsFinite(r2), key.OrDefault().InvalidInput())
select (SdfKind)new CappedConeCase(HalfHeight: h, R1: r1, R2: r2);
```

```csharp
from h in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: height)
from a in key.OrDefault().AcceptValidated<VectorAngle>(candidate: halfAngleRadians)
from _ in guard(a.Value < Math.PI / 2.0, key.OrDefault().InvalidInput())
select (SdfKind)new ConeCase(Height: h, HalfAngle: a);
```

```csharp
from s in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: spread)
from _ in guard(double.IsFinite(strength) && center.IsValid, key.OrDefault().InvalidInput())
select (ScalarField)new DensityCase(Center: center, Spread: s, Strength: strength);
```

### To

```csharp
Op op = key.OrDefault();
return (op.AcceptValidated<PositiveMagnitude>(etaIncident).ToValidation(),
        op.AcceptValidated<PositiveMagnitude>(etaTransmitted).ToValidation())
    .Apply(static (i, t) => (BouncePolicy)new RefractCase(i, t)).As().ToFin();
```

```csharp
Op op = key.OrDefault();
return (op.AcceptValidated<Dimension>(octaves).ToValidation(),
        op.AcceptValidated<PositiveMagnitude>(persistence).ToValidation(),
        op.AcceptValidated<PositiveMagnitude>(lacunarity).ToValidation(),
        op.AcceptValidated<PositiveMagnitude>(frequency).ToValidation())
    .Apply((count, gain, gap, rate) => new NoisePolicy(seed, lane.IfNone(FieldLane.Noise), count, gain, gap, rate)).As().ToFin();
```

```csharp
Op op = key.OrDefault();
return (op.AcceptValidated<PositiveMagnitude>(halfHeight).ToValidation(),
        guard(r1 >= 0.0 && r2 >= 0.0 && (r1 > 0.0 || r2 > 0.0)
            && double.IsFinite(r1) && double.IsFinite(r2), op.InvalidInput()).ToFin().ToValidation())
    .Apply((h, _) => (SdfKind)new CappedConeCase(h, r1, r2)).As().ToFin();
```

```csharp
Op op = key.OrDefault();
return (op.AcceptValidated<PositiveMagnitude>(height).ToValidation(),
        op.AcceptValidated<VectorAngle>(halfAngleRadians).ToValidation())
    .Apply(static (h, a) => (Height: h, HalfAngle: a)).As().ToFin()
    .Bind(pair => guard(pair.HalfAngle.Value < Math.PI / 2.0, op.InvalidInput()).ToFin()
        .Map(_ => (SdfKind)new ConeCase(pair.Height, pair.HalfAngle)));
```

```csharp
Op op = key.OrDefault();
return (op.AcceptValidated<PositiveMagnitude>(spread).ToValidation(),
        guard(double.IsFinite(strength) && center.IsValid, op.InvalidInput()).ToFin().ToValidation())
    .Apply((s, _) => (ScalarField)new DensityCase(center, s, strength)).As().ToFin();
```

### Why

Each pair or tuple of raw facts is independent. Sequential `Fin` comprehensions discard every fault after the first; `Validation<Error,T>` plus tuple `Apply` checks every slot once and converts back to the branch result only after construction. `Cone` follows the same pattern for height and angle admission, then binds the dependent `HalfAngle < π/2` refinement after both succeed.

# 9. Delete case-renaming construction wrappers

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:335,500` — anchors `ScalarField.Constant` and `TensorField.Lift`.

### From

```csharp
public static ScalarField Constant(double value) => new ConstantCase(Value: value);
public static TensorField Lift(Func<Point3d, SymmetricMatrix> source) => new LiftCase(Source: source);
```

### To

```csharp
// ScalarField.Constant DELETED
// TensorField.Lift DELETED
```

### Why

Both public members only rename an already-public case constructor; neither admits, canonicalizes, owns custody, derives policy, or shortens a repeated construction. The real closure and constant capabilities remain as `LiftCase` and `ConstantCase`.

# 10. Let the projection owner erase vector shape

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:432-437` — anchor `hitFieldCase` and its `SupportProjection.Span`/`SignedSpanAway` identity branch.

### From

```csharp
hitToScaled: (hit, op) => c.Projection.Equals(SupportProjection.Span) || c.Projection.Equals(SupportProjection.SignedSpanAway)
    ? c.Projection.Project<VectorSpan>(space: c.Source, hit: hit, sample: s.Sample, context: s.Context, key: op)
        .Map(span => (Raw: span.Direction.Value, Scale: span.Magnitude.Value))
    : c.Projection.Project<Vector3d>(space: c.Source, hit: hit, sample: s.Sample, context: s.Context, key: op)
        .Map(raw => (Raw: raw, Scale: 1.0)))
```

### To

```csharp
hitToScaled: (hit, op) => c.Projection
    .Project<Vector3d>(space: c.Source, hit: hit, sample: s.Sample, context: s.Context, key: op)
    .Map(static raw => (Raw: raw, Scale: 1.0)))
```

### Why

`SupportProjection.SpanOf` already admits `Vector3d` and projects the signed span directly to that output. Re-checking two row identities here mirrors the projection roster, duplicates its vector conversion, and silently misses a future vector-capable row; the generic projection is the one exhaustive owner.

# 11. Accumulate independent CSG branches

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:364-367` — anchor `ScalarField.SampleScalar` `csgCase`.

### From

```csharp
csgCase: static (s, c) =>
    from l in c.Left.SampleScalar(sample: s.Sample, context: s.Context, key: s.Key)
    from r in c.Right.SampleScalar(sample: s.Sample, context: s.Context, key: s.Key)
    select c.Op.Combine(left: l, right: r, blend: c.Smoothing),
```

### To

```csharp
csgCase: static (s, c) =>
    (c.Left.SampleScalar(s.Sample, s.Context, s.Key).ToValidation(),
     c.Right.SampleScalar(s.Sample, s.Context, s.Key).ToValidation())
    .Apply((l, r) => c.Op.Combine(l, r, c.Smoothing)).As().ToFin(),
```

### Why

The two samples depend only on the common point and context, not on each other. Applicative `Apply` preserves both typed failures and constructs the CSG value only after both succeed; the sequential comprehension unnecessarily imposed first-failure semantics.

# 12. Accumulate radial terms before folding

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:438-449,463-468` — anchors the `coulombCase`, `clusterFieldCase`, and `RadialContribution` accumulator parameter.

### From

```csharp
coulombCase: static (s, c) => c.Charges.Fold(Fin.Succ(Vector3d.Zero),
    (acc, charge) => acc.Bind(sum => RadialContribution(sum: sum, source: charge.Position,
        scale: charge.Charge, state: s, falloff: c.Falloff))),
```

```csharp
from field in ids.Fold(Fin.Succ(Vector3d.Zero),
    (acc, i) => acc.Bind(sum => RadialContribution(sum: sum, source: c.Source.Vertices[i],
        scale: c.Sense.Sign, state: s, falloff: c.Falloff)))
select field,
```

```csharp
private static Fin<Vector3d> RadialContribution(Vector3d sum, Point3d source, double scale,
    (Point3d Sample, Context Context, Op Key) state, Falloff falloff) {
    Vector3d r = state.Sample - source;
    return r.Length <= state.Context.For(lane: ToleranceLane.Duplicate).Value
        ? Fin.Succ(sum)
        : falloff.Weight(offset: r, sample: state.Sample,
            tolerance: state.Context.For(lane: ToleranceLane.Duplicate).Value, key: state.Key)
            .Map(w => sum + (scale * w / r.Length * r));
}
```

### To

```csharp
coulombCase: static (s, c) => c.Charges
    .Traverse(charge => RadialContribution(charge.Position, charge.Charge, s, c.Falloff).ToValidation()).As()
    .Map(static terms => terms.Fold(Vector3d.Zero, static (sum, term) => sum + term)).ToFin(),
```

```csharp
from terms in ids.Traverse(i => RadialContribution(c.Source.Vertices[i], c.Sense.Sign, s, c.Falloff)
    .ToValidation()).As().ToFin()
select terms.Fold(Vector3d.Zero, static (sum, term) => sum + term),
```

```csharp
private static Fin<Vector3d> RadialContribution(Point3d source, double scale,
    (Point3d Sample, Context Context, Op Key) state, Falloff falloff) {
    Vector3d r = state.Sample - source;
    return r.Length <= state.Context.For(ToleranceLane.Duplicate).Value
        ? Fin.Succ(Vector3d.Zero)
        : falloff.Weight(r, state.Sample, state.Context.For(ToleranceLane.Duplicate).Value, state.Key)
            .Map(w => scale * w / r.Length * r);
}
```

### Why

Every radial contribution is independent; only the final vector sum depends on the terms. Traversing applicatively reports all failed weights, then performs one pure fold. Removing the running sum from `RadialContribution` narrows the helper to its actual meaning and lets `clusterFieldCase` use the same traverse-then-fold form over its neighbor ids.

# 13. Remove redundant lattice sweep hops

### Location

`libs/dotnet/Rasm/.planning/Spatial/fields.md:389-400` — anchor `ScalarField.SampleLattice`.

### From

```csharp
ScalarField self = this;
return from cells in guard(grid.CellCount is > 0 and <= int.MaxValue, op.InvalidInput()).ToFin().Map(_ => (int)grid.CellCount)
       from values in toSeq(Enumerable.Range(0, cells)).TraverseM(index => {
```

```csharp
return self.SampleScalar(sample: grid.Center(column, row, layer), context: context, key: op)
    .MapFail(cause => cause + op.InvalidResult(detail: $"lattice-cell:{column},{row},{layer}"));
}).As()
from plane in op.AcceptValue(value: toArr(values))
select plane;
```

### To

```csharp
return from _ in guard(grid.CellCount <= int.MaxValue, op.InvalidInput()).ToFin()
       from values in toSeq(Enumerable.Range(0, (int)grid.CellCount)).TraverseM(index => {
```

```csharp
return SampleScalar(sample: grid.Center(column, row, layer), context: context, key: op)
    .MapFail(cause => cause + op.InvalidResult(detail: $"lattice-cell:{column},{row},{layer}"));
}).As()
select toArr(values);
```

### Why

Admitted `Dimension` axes already make `CellCount` positive; only `Enumerable.Range`'s `int` ceiling remains. `TraverseM` deliberately retains the first cell fault, every sampled scalar has already crossed `AcceptValue`, and `toArr` cannot invalidate it, so the instance alias, count projection, and second result admission are redundant.
