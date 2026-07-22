# [RASM_FIELDS]

One implicit-field algebra over three closed field unions — `ScalarField`, `VectorField`, `TensorField` — each sampled anywhere in space through one per-union dispatch, composed through flattening operators, and constructed so a case's payload types are its admission structure. Raw ingress is one-expression admitting factories and multi-knob ingress rides a policy record; everything else constructs from already-admitted material, so no re-validation switch stands beside the case list. This page samples and never meshes: iso-surface extraction is `reconstruct.md`'s.

`calculus.md` owns the sample-anywhere math this page composes — the `Nabla` stencil, the `Falloff`/`KernelKind` weight vocabularies, and the `FieldNoise` lattices the `NoiseKind` rows point at. `reconstruct.md` owns the reconstruction kernels and mesh-SDF policy; its solvers mint the fitted payloads, and `SignedDistanceFromMesh` delegates to its `MeshSdf` with no second SDF evaluator here. Mesh-aware cases delegate through the `mesh.md` `MeshSpace` seam. `SampleDetailed` and `SampleSdfDetailed` are the public tagged sampling seam reporting how a value was produced, and `pack.md` binds `SampleDetailed` for its scalar facet.

## [01]-[INDEX]

- [02]-[FIELD_VOCAB]: `BlendKind`, `CsgKind`, `NoiseKind`, and the ray, bounce, and provenance vocabularies, each owning its policy columns.
- [03]-[SDF_PRIMITIVES]: `SdfKind` exact analytic primitives as typed parameter cases carrying `Lipschitz` and `Distance`.
- [04]-[SCALAR_FIELD]: `ScalarField` algebra, its one total sample dispatch, the Lipschitz fold, and the public tagged rail.
- [05]-[VECTOR_FIELD]: `VectorField` algebra over three shared radial, rotational, and closest-directed folds.
- [06]-[TENSOR_FIELD]: `TensorField` symmetric-tensor algebra, congruence transforms, and principal directions.

## [02]-[FIELD_VOCAB]

- Owner: each `BlendKind` case overrides the abstract `ErosionFactor` column the Lipschitz-erosion fold reads, so the erosion multiplier is a policy value on the row rather than a table beside the union. `NoiseKind` rows live here by the `calculus.md` seam — lattices are mathematics, field rows policy: `Perlin` sets `RaisesCaution` for visible lattice anisotropy and `Worley` clears `ContinuouslyDifferentiable`, gating `CurlNoise` admission. `SdfStatus` rows are the provenance the tagged samples carry, and `SampleSdfDetailed` faults a non-distance species with a typed fault rather than mislabeling a value.
- Boundary: `Falloff` and `KernelKind` own their weight-profile and kernel math at `calculus.md`, composed here never re-derived; `NoiseKind` rows point at its `FieldNoise` lattices; each `BlendKind` case declares its own `ErosionFactor`, and `RayPolicy.Project` resolves through typed `ProjectionRow` entries.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record BlendKind {
    private BlendKind() { }
    public sealed record HardCase() : BlendKind { public override double ErosionFactor => 1.00; }
    public sealed record PolynomialCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.25; }
    public sealed record ExponentialCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.15; }
    public sealed record RootCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.10; }
    public sealed record CubicCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.30; }
    public sealed record ChamferCase(PositiveMagnitude K) : BlendKind { public override double ErosionFactor => 1.50; }
    public sealed record GrooveCase(PositiveMagnitude K, PositiveMagnitude D) : BlendKind { public override double ErosionFactor => 1.40; }
    public sealed record RoundCase(PositiveMagnitude R) : BlendKind { public override double ErosionFactor => 1.20; }

    public abstract double ErosionFactor { get; }
    public static BlendKind Hard { get; } = new HardCase();
    public static Fin<BlendKind> Polynomial(double k, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: k).Map(static v => (BlendKind)new PolynomialCase(K: v));
    // Exponential/Root/Cubic/Chamfer/Round: the identical one-expression admission; Groove admits its (k, d) pair.

    internal double Smin(double a, double b) => Switch(state: (A: a, B: b),
        hardCase: static (s, _) => Math.Min(s.A, s.B),
        polynomialCase: static (s, c) => { double h = Math.Max(c.K.Value - Math.Abs(s.A - s.B), 0.0) / c.K.Value; return Math.Min(s.A, s.B) - (h * h * h * c.K.Value / 6.0); },
        exponentialCase: static (s, c) => { double ax = -c.K.Value * s.A, bx = -c.K.Value * s.B, m = Math.Max(ax, bx); return -(m + Math.Log(Math.Exp(ax - m) + Math.Exp(bx - m))) / c.K.Value; },
        rootCase: static (s, c) => { double h = Math.Max(c.K.Value - Math.Abs(s.A - s.B), 0.0); return Math.Min(s.A, s.B) - (h * h * 0.25 / c.K.Value); },
        cubicCase: static (s, c) => { double h = Math.Max(c.K.Value - Math.Abs(s.A - s.B), 0.0) / c.K.Value; return Math.Min(s.A, s.B) - (h * h * c.K.Value * 0.25); },
        chamferCase: static (s, c) => Math.Min(Math.Min(s.A, s.B), (s.A + s.B - c.K.Value) * 0.7071067811865475),
        grooveCase: static (s, c) => Math.Max(s.A, Math.Min(c.D.Value, Math.Min(s.A - c.K.Value, s.B - c.K.Value))),
        roundCase: static (s, c) => { double ax = Math.Max(c.R.Value - s.A, 0.0), bx = Math.Max(c.R.Value - s.B, 0.0); return Math.Max(c.R.Value, Math.Min(s.A, s.B)) - Math.Sqrt((ax * ax) + (bx * bx)); });

    internal double Erode(double leftLip, double rightLip) => ErosionFactor * Math.Max(leftLip, rightLip);
}

[SmartEnum<int>]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(key: 0, combine: static (a, b, blend) => blend.Smin(a: a, b: b));
    public static readonly CsgKind Intersect = new(key: 1, combine: static (a, b, blend) => -blend.Smin(a: -a, b: -b));
    public static readonly CsgKind Difference = new(key: 2, combine: static (a, b, blend) => -blend.Smin(a: -a, b: b));
    [UseDelegateFromConstructor] internal partial double Combine(double left, double right, BlendKind blend);
}

[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static count => 1.0 / count);
    [UseDelegateFromConstructor] private partial double Scale(int count);
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) => CombineCore(values: vectors, zero: Vector3d.Zero, add: static (s, v) => s + v, scale: static (s, f) => s * f, key: key);
    internal Fin<double> CombineScalar(Seq<double> values, Op key) => CombineCore(values: values, zero: 0.0, add: static (s, v) => s + v, scale: static (s, f) => s * f, key: key);
    private Fin<T> CombineCore<T>(Seq<T> values, T zero, Func<T, T, T> add, Func<T, double, T> scale, Op key) =>
        from _ in guard(!values.IsEmpty, key.InvalidResult())
        from value in key.AcceptValue(value: scale(values.Fold(zero, add), Scale(count: values.Count)))
        select value;
}

[Union]
public abstract partial record RayPolicy {
    private RayPolicy() { }
    public sealed record InfiniteCase(BoundarySense Sense) : RayPolicy;
    public sealed record SegmentCase(BoundarySense Sense, PositiveMagnitude Length) : RayPolicy;
    public static RayPolicy Forward { get; } = new InfiniteCase(Sense: BoundarySense.Toward);
    public static RayPolicy Reverse { get; } = new InfiniteCase(Sense: BoundarySense.Away);
    public static Fin<RayPolicy> Segment(double length, Option<BoundarySense> sense = default, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: length)
            .Map(l => (RayPolicy)new SegmentCase(Sense: sense.IfNone(BoundarySense.Toward), Length: l));
    internal Fin<TOut> Project<TOut>(Point3d origin, Direction direction, Context context, Op key) { /* typed rows:
        Ray3d | Plane | Direction | Vector3d always; Line | VectorSpan gated on SegmentCase.Length —
        AtomProjection.Rows over the sense-signed vector, never a typeof ladder. */ return default!; }
}

[Union]
public abstract partial record BouncePolicy {
    private BouncePolicy() { }
    public sealed record ReflectCase : BouncePolicy;
    public sealed record RefractCase(PositiveMagnitude EtaIncident, PositiveMagnitude EtaTransmitted) : BouncePolicy;
    public static BouncePolicy Reflect { get; } = new ReflectCase();
    public static Fin<BouncePolicy> Refract(double etaIncident, double etaTransmitted, Op? key = null) =>
        from i in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: etaIncident)
        from t in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: etaTransmitted)
        select (BouncePolicy)new RefractCase(EtaIncident: i, EtaTransmitted: t);
    internal Fin<Direction> Apply(Direction incident, Direction normal, Op key) => Switch(
        state: (Incident: incident, Normal: normal, Key: key),
        reflectCase: static (s, _) => Fin.Succ(s.Incident.Reflect(normal: s.Normal)),
        refractCase: static (s, r) => Direction.Refract(incident: s.Incident, normal: s.Normal,
            etaIncident: r.EtaIncident.Value, etaTransmitted: r.EtaTransmitted.Value, key: s.Key));
}

[SmartEnum<int>]
public sealed partial class SdfStatus {
    public static readonly SdfStatus Analytic = new(key: 0);
    public static readonly SdfStatus ComposedAnalytic = new(key: 1);
    public static readonly SdfStatus NativeProfile = new(key: 2);
    public static readonly SdfStatus MeshApproximate = new(key: 3);
    public static readonly SdfStatus Reconstruction = new(key: 4);
    public static readonly SdfStatus TetSignedHeat = new(key: 5);
}

[SmartEnum<int>]
public sealed partial class ProfileExtrusionFeature {
    public static readonly ProfileExtrusionFeature Interior = new(key: 0);
    public static readonly ProfileExtrusionFeature ProfileBoundary = new(key: 1);
    public static readonly ProfileExtrusionFeature Cap = new(key: 2);
    public static readonly ProfileExtrusionFeature Rim = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class NoiseKind {
    public static readonly NoiseKind Perlin = new(key: 0, raisesCaution: true, continuouslyDifferentiable: true,
        sample: static (p, seed, f) => FieldNoise.PerlinAt(point: p, seed: seed, frequency: f));
    public static readonly NoiseKind Simplex = new(key: 1, raisesCaution: false, continuouslyDifferentiable: true,
        sample: static (p, seed, f) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: f, smooth: false));
    public static readonly NoiseKind SmoothSimplex = new(key: 2, raisesCaution: false, continuouslyDifferentiable: true,
        sample: static (p, seed, f) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: f, smooth: true));
    public static readonly NoiseKind Worley = new(key: 3, raisesCaution: false, continuouslyDifferentiable: false,
        sample: static (p, seed, f) => FieldNoise.WorleyAt(point: p, seed: seed, frequency: f));
    public bool RaisesCaution { get; }
    public bool ContinuouslyDifferentiable { get; }
    [UseDelegateFromConstructor] internal partial double Sample(Point3d point, int seed, double frequency);
}
```

## [03]-[SDF_PRIMITIVES]

- Owner: `SdfKind` `[Union]` — the exact analytic signed-distance primitives, each a typed parameter record carrying its own `Lipschitz` bound column and `Distance(Point3d local)` member. Typed records make a wrong parameter a compile error and a missing one unconstructible.
- Entry: `SignedDistance` remaps world to pose space once through `Plane.RemapToPlaneSpace`, then dispatches the case's `Distance`, computing the Inigo Quilez exact forms. Cross-payload guards ride the factories: `CappedCone` demands one positive radius, and `Cone` proves `HalfAngle < π/2` because `tan` flips sign past it and the derived base radius goes negative.
- Growth: a new primitive is one typed case with its `Lipschitz` and `Distance` members; the `ScalarField.PrimitiveCase` payload, the Lipschitz fold, and the tagged sampler pick it up through the union.
- Boundary: `Distance` bodies are pure local-frame math — pose handling happens once at `SignedDistance`, never inside a case. `Lipschitz` is load-bearing — the ray-march step bound and the CSG erosion fold read the column — so a case without an honest bound is inadmissible.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record SdfKind {
    private SdfKind() { }
    public sealed record SphereCase(PositiveMagnitude Radius) : SdfKind {
        public override double Lipschitz => 1.0;
        internal override double Distance(Point3d p) => Math.Sqrt((p.X * p.X) + (p.Y * p.Y) + (p.Z * p.Z)) - Radius.Value;
    }
    public sealed record BoxCase(PositiveMagnitude X, PositiveMagnitude Y, PositiveMagnitude Z) : SdfKind {
        public override double Lipschitz => 1.0;
        internal override double Distance(Point3d p) {
            (double qx, double qy, double qz) = (Math.Abs(p.X) - X.Value, Math.Abs(p.Y) - Y.Value, Math.Abs(p.Z) - Z.Value);
            (double ox, double oy, double oz) = (Math.Max(qx, 0.0), Math.Max(qy, 0.0), Math.Max(qz, 0.0));
            return Math.Sqrt((ox * ox) + (oy * oy) + (oz * oz)) + Math.Min(Math.Max(qx, Math.Max(qy, qz)), 0.0);
        }
    }
    public sealed record CapsuleCase(PositiveMagnitude HalfHeight, PositiveMagnitude Radius) : SdfKind {
        public override double Lipschitz => 1.0;
        internal override double Distance(Point3d p) {
            double pz = p.Z - Math.Clamp(p.Z, -HalfHeight.Value, HalfHeight.Value);
            return Math.Sqrt((p.X * p.X) + (p.Y * p.Y) + (pz * pz)) - Radius.Value;
        }
    }
    public sealed record CylinderCase(PositiveMagnitude HalfHeight, PositiveMagnitude Radius) : SdfKind { public override double Lipschitz => 1.0; internal override double Distance(Point3d p) => CappedProfile(dxy: Math.Sqrt((p.X * p.X) + (p.Y * p.Y)) - Radius.Value, dz: Math.Abs(p.Z) - HalfHeight.Value); }
    public sealed record ConeCase(PositiveMagnitude Height, VectorAngle HalfAngle) : SdfKind { public override double Lipschitz => 1.0; internal override double Distance(Point3d p) => CappedCone(p: new Point3d(p.X, p.Y, p.Z + (0.5 * Height.Value)), halfHeight: 0.5 * Height.Value, r1: Height.Value * Math.Tan(HalfAngle.Value), r2: 0.0); }
    public sealed record HalfSpaceCase() : SdfKind { public override double Lipschitz => 1.0; internal override double Distance(Point3d p) => p.Z; }
    public sealed record CappedConeCase(PositiveMagnitude HalfHeight, double R1, double R2) : SdfKind {
        public override double Lipschitz => 1.2;
        internal override double Distance(Point3d p) => CappedCone(p: p, halfHeight: HalfHeight.Value, r1: R1, r2: R2);
    }
    public sealed record TorusCase(PositiveMagnitude Major, PositiveMagnitude Minor) : SdfKind { public override double Lipschitz => 1.0; internal override double Distance(Point3d p) { double qx = Math.Sqrt((p.X * p.X) + (p.Y * p.Y)) - Major.Value; return Math.Sqrt((qx * qx) + (p.Z * p.Z)) - Minor.Value; } }
    public sealed record HexPrismCase(PositiveMagnitude HalfHeight, PositiveMagnitude Circumradius) : SdfKind {
        public override double Lipschitz => 1.0;
        // Exact hex: k-fold to the fundamental sector then vertex-corrected edge distance (IQ); a max-of-half-planes underestimates at the corners.
        internal override double Distance(Point3d p) {
            const double kx = -0.8660254037844386, ky = 0.5, kz = 0.5773502691896258;
            double h = 0.8660254037844386 * Circumradius.Value;
            (double ax, double ay) = (Math.Abs(p.X), Math.Abs(p.Y));
            double fold = 2.0 * Math.Min((kx * ax) + (ky * ay), 0.0);
            (ax, ay) = (ax - (fold * kx), ay - (fold * ky));
            (double ex, double ey) = (ax - Math.Clamp(ax, -kz * h, kz * h), ay - h);
            double dxy = Math.Sqrt((ex * ex) + (ey * ey)) * Math.Sign(ey);
            return CappedProfile(dxy: dxy, dz: Math.Abs(p.Z) - HalfHeight.Value);
        }
    }
    public sealed record OctahedronCase(PositiveMagnitude S) : SdfKind { public override double Lipschitz => 1.0; internal override double Distance(Point3d p) => ExactOctahedron(p: p, s: S.Value); }
    public sealed record EllipsoidCase(PositiveMagnitude X, PositiveMagnitude Y, PositiveMagnitude Z) : SdfKind { public override double Lipschitz => 2.0; internal override double Distance(Point3d p) { /* k0*(k0-1)/k1 first-order normalized estimate */ return default; } }
    public sealed record SlabCase(PositiveMagnitude HalfHeight) : SdfKind { public override double Lipschitz => 1.0; internal override double Distance(Point3d p) => Math.Abs(p.Z) - HalfHeight.Value; }

    public abstract double Lipschitz { get; }
    internal abstract double Distance(Point3d local);
    public static Fin<SdfKind> CappedCone(double halfHeight, double r1, double r2, Op? key = null) =>
        from h in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: halfHeight)
        from _ in guard(r1 >= 0.0 && r2 >= 0.0 && (r1 > 0.0 || r2 > 0.0) && double.IsFinite(r1) && double.IsFinite(r2), key.OrDefault().InvalidInput())
        select (SdfKind)new CappedConeCase(HalfHeight: h, R1: r1, R2: r2);
    public static Fin<SdfKind> Cone(double height, double halfAngleRadians, Op? key = null) =>
        from h in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: height)
        from a in key.OrDefault().AcceptValidated<VectorAngle>(candidate: halfAngleRadians)
        from _ in guard(a.Value < Math.PI / 2.0, key.OrDefault().InvalidInput())
        select (SdfKind)new ConeCase(Height: h, HalfAngle: a);

    internal Fin<double> SignedDistance(Point3d worldPoint, Plane pose, Op key) =>
        pose.RemapToPlaneSpace(ptSample: worldPoint, ptPlane: out Point3d local)
            ? key.AcceptValue(value: Distance(local: local))
            : Fin.Fail<double>(key.InvalidResult());
    // CappedProfile / CappedCone / ExactOctahedron: shared exact-form kernels (IQ two-segment cone, octant-folded octahedron), private statics on the union.
}
```

## [04]-[SCALAR_FIELD]

- Owner: `ScalarField` `[Union]` — the scalar algebra in case families spanning analytic sources, combinators, domain warps, differential operators, mesh-aware solvers, and reconstruction. Mesh-aware and reconstruction cases construct only through their admitting factories, never `new`, so the factory proves sources against the `MeshSpace` range or the fitted payload against its `reconstruct.md` minter. `Noise` takes the `NoisePolicy` record.
- Auto: `SampleScalar` is the one total generated `Switch` over the union — analytic sources evaluate closed forms, combinators recurse, warps pre-transform the sample and recurse, differential arms delegate to the `calculus.md` `Nabla` stencil with sampler closures the stencil never learns the union from, mesh-aware arms delegate through the `MeshSpace` seam, and reconstruction arms evaluate the fitted payload through `reconstruct.md`. One shared `SampleMapped` body collapses the map-only warps. `LipschitzBound` folds only the analytically bounded species; an over-claimed bound overshoots ray-march steps into silently missed surfaces, so `Twist`, `Bend`, and `Periodic` return `None` by decision.
- Receipt: `SampleDetailed → Fin<FieldSample>` is the public tagged rail carrying value, `SdfStatus` provenance, and nested evidence for mesh, reconstruction, and tet species. `SampleSdfDetailed → Fin<SdfSample>` refuses a species with no distance semantics, faulting `Unsupported` rather than mislabeling a value as a distance; its `SdfSample` carries the profile-extrusion `ProfileFeature` and `ProfileContainment` columns only on the `NativeProfile` species.
- Packages: `RhinoCommon`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`.
- Growth: a new scalar species is one case and one `Switch` arm, a factory only when raw material enters; a new blend or CSG mode is a vocabulary row; a new provenance species is one `SdfStatus` row.
- Boundary: mesh-aware arms are one-line delegations, and any solver math here is a mis-homed body. `SampleScalar` assumes admitted fields, so an in-arm re-validation is double admission. Tagged sampling is the one public seam; a second `Evaluate` or `Probe` family is the rejected surface.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct NoisePolicy(int Seed, Dimension Octaves, double Persistence, double Lacunarity, double Frequency) {
    public static Fin<NoisePolicy> Of(int seed, int octaves, double persistence, double lacunarity, double frequency, Op? key = null) =>
        from count in key.OrDefault().AcceptValidated<Dimension>(candidate: octaves)
        from _ in guard(double.IsFinite(persistence) && persistence > 0.0 && double.IsFinite(lacunarity) && lacunarity > 0.0 && double.IsFinite(frequency) && frequency > 0.0, key.OrDefault().InvalidInput())
        select new NoisePolicy(Seed: seed, Octaves: count, Persistence: persistence, Lacunarity: lacunarity, Frequency: frequency);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct FieldSample(
    double Value, SdfStatus Status, Option<SdfMeshReceipt> Mesh,
    Option<ReconstructionSampleReceipt> Reconstruction, Option<TetSignedHeatReceipt> Tet);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfSample(
    double Value, SdfStatus Status, Option<double> LipschitzBound, Option<SdfMeshReceipt> Mesh, Option<TetSignedHeatReceipt> Tet,
    Option<ProfileExtrusionFeature> ProfileFeature, Option<PointContainment> ProfileContainment);

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union]
public abstract partial record ScalarField {
    private ScalarField() { }
    public sealed record ConstantCase(double Value) : ScalarField;
    public sealed record BlendCase(Seq<ScalarField> Fields, FieldBlend Mode) : ScalarField;
    public sealed record ScaledCase(ScalarField Source, double Scale) : ScalarField;
    public sealed record DistanceCase(SupportSpace Source, BoundarySense Sense) : ScalarField;
    public sealed record PotentialCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : ScalarField;
    public sealed record DensityCase(Point3d Center, PositiveMagnitude Spread, double Strength) : ScalarField;
    public sealed record MagnitudeCase(VectorField Source) : ScalarField;
    public sealed record DivergenceCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record LaplacianCase(ScalarField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record StrainMagnitudeCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record WorleyCase(Seq<Point3d> Seeds, Dimension Order) : ScalarField;
    public sealed record MorseCase(Point3d Center, PositiveMagnitude Depth, PositiveMagnitude Width) : ScalarField;
    public sealed record MollifierCase(Point3d Center, PositiveMagnitude Radius) : ScalarField;
    public sealed record NoiseCase(NoiseKind Kind, NoisePolicy Policy) : ScalarField;
    public sealed record PowerCase(ScalarField Source, double Exponent) : ScalarField;
    public sealed record CsgCase(ScalarField Left, ScalarField Right, CsgKind Op, BlendKind Smoothing) : ScalarField;
    public sealed record PeriodicCase(ScalarField Source, Vector3d Period) : ScalarField;
    public sealed record ClampCase(ScalarField Source, double Minimum, double Maximum) : ScalarField;
    public sealed record PrimitiveCase(SdfKind Shape, Plane Pose) : ScalarField;
    public sealed record ProfileExtrusionCase(Curve Profile, Plane Plane, PositiveMagnitude HalfHeight) : ScalarField;
    public sealed record OnionCase(ScalarField Source, PositiveMagnitude Thickness) : ScalarField;
    public sealed record SdfRoundCase(ScalarField Source, PositiveMagnitude Radius) : ScalarField;
    public sealed record ElongateCase(ScalarField Source, Vector3d Extent) : ScalarField;
    public sealed record DisplaceCase(ScalarField Source, ScalarField Displacement) : ScalarField;
    public sealed record TwistCase(ScalarField Source, double AnglePerUnit, Direction Axis) : ScalarField;
    public sealed record BendCase(ScalarField Source, double Curvature, Direction Axis) : ScalarField;
    public sealed record GeodesicCase(MeshSpace Space, Seq<int> Sources) : ScalarField;
    public sealed record MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, Dimension Iterations) : ScalarField;
    public sealed record SpectralDistanceCase(MeshSpace Space, SpectralFilter Filter, Seq<int> Sources, Dimension Pairs) : ScalarField;
    public sealed record StripeCase(MeshSpace Space, VectorField CrossField, PositiveMagnitude Frequency) : ScalarField;
    public sealed record SignedDistanceFromMeshCase(MeshSpace Space, SdfMeshPolicy Policy) : ScalarField;
    public sealed record RbfCase(Seq<(Point3d Position, double Value)> Samples, KernelKind Kernel, PositiveMagnitude Radius, Arr<double> Coefficients, ReconstructionReceipt Receipt) : ScalarField;
    public sealed record MlsCase(Seq<MlsSample> Samples, KernelKind Kernel, PositiveMagnitude Radius, ReconstructionReceipt Receipt) : ScalarField;
    public sealed record LevinMlsCase(Seq<MlsSample> Samples, LevinMlsPolicy Policy, ReconstructionReceipt Receipt) : ScalarField;
    public sealed record ApssCase(Seq<MlsSample> Samples, ApssPolicy Policy, ReconstructionReceipt Receipt) : ScalarField;
    public sealed record TetSignedHeatCase(TetMeshDomain Domain, TetSignedHeatPolicy Policy, Arr<double> Values, TetSignedHeatReceipt Receipt) : ScalarField;
    public sealed record PoissonCase(PoissonGrid Grid, double Gamma, PoissonReceipt Receipt) : ScalarField;

    public static ScalarField Constant(double value) => new ConstantCase(Value: value);
    public static Fin<ScalarField> Density(Point3d center, double spread, double strength, Op? key = null) =>
        from s in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: spread)
        from _ in guard(double.IsFinite(strength) && center.IsValid, key.OrDefault().InvalidInput())
        select (ScalarField)new DensityCase(Center: center, Spread: s, Strength: strength);
    public static Fin<ScalarField> Geodesic(MeshSpace space, Seq<int> sources, Op? key = null) =>
        guard(!sources.IsEmpty && sources.ForAll(v => v >= 0 && v < space.Native.Vertices.Count), key.OrDefault().InvalidInput())
            .ToFin().Map(_ => (ScalarField)new GeodesicCase(Space: space, Sources: sources));
    // Remaining raw-ingress factories: one admission expression each; reconstruction cases are minted ONLY by reconstruct.md fitting.

    public static ScalarField operator +(ScalarField left, ScalarField right) =>
        new BlendCase(Fields: (left is BlendCase { Mode: var lm } lb && lm.Equals(FieldBlend.Sum) ? lb.Fields : Seq(left))
            .Concat(right is BlendCase { Mode: var rm } rb && rm.Equals(FieldBlend.Sum) ? rb.Fields : Seq(right)).ToSeq(), Mode: FieldBlend.Sum);
    public static ScalarField operator -(ScalarField left, ScalarField right) => left + (-right);
    public static ScalarField operator -(ScalarField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static ScalarField operator *(ScalarField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static ScalarField operator *(double scale, ScalarField field) => new ScaledCase(Source: field, Scale: scale);

    public Option<double> LipschitzBound() => this switch {
        ConstantCase => Some(0.0),
        PrimitiveCase p => Some(p.Shape.Lipschitz),
        ProfileExtrusionCase => Some(1.0),
        // Closest-point distance to ANY support is 1-Lipschitz; the sense sign preserves |∇|.
        DistanceCase => Some(1.0),
        // Order-th sorted seed distance is an order statistic of 1-Lipschitz distances — still 1-Lipschitz.
        WorleyCase => Some(1.0),
        // Exact radial slope maxima: Morse |V′| peaks at u = e^{−r/W} = ½ → D/(2W); Gaussian density |∇| peaks at r = σ → |S|e^{−1/2}/σ.
        MorseCase m => Some(m.Depth.Value / (2.0 * m.Width.Value)),
        DensityCase d => Some(Math.Abs(d.Strength) * Math.Exp(-0.5) / d.Spread.Value),
        // |∇Σqᵢ·w| ≤ Σ|qᵢ|·sup|w′| — the calculus.md Falloff.SlopeBound column; a None-bounded decay law keeps the composite None.
        PotentialCase p => p.Falloff.SlopeBound.Map(slope => p.Charges.Fold(0.0, static (acc, c) => acc + Math.Abs(c.Charge)) * slope),
        CsgCase c => from l in c.Left.LipschitzBound() from r in c.Right.LipschitzBound() select c.Smoothing.Erode(leftLip: l, rightLip: r),
        ScaledCase s => s.Source.LipschitzBound().Map(l => Math.Abs(s.Scale) * l),
        DisplaceCase d => from l in d.Source.LipschitzBound() from r in d.Displacement.LipschitzBound() select l + r,
        BlendCase b => b.Fields.TraverseM(static f => f.LipschitzBound()).As().Map(bounds =>
            b.Mode.Equals(FieldBlend.Average) ? bounds.Sum() / bounds.Count : bounds.Sum()),
        OnionCase o => o.Source.LipschitzBound(),
        SdfRoundCase r => r.Source.LipschitzBound(),
        ElongateCase e => e.Source.LipschitzBound(),
        ClampCase c => c.Source.LipschitzBound(),
        // Twist/Bend stretch tangentially with radius; Periodic's wrap seam breaks the modulus of continuity for asymmetric sources — no honest global bound.
        _ => Option<double>.None,
    };

    internal Fin<double> SampleScalar(Point3d sample, Context context, Op key) =>
        key.AcceptValue(value: sample).Bind(_ => Switch(state: (Sample: sample, Context: context, Key: key),
            constantCase: static (s, c) => s.Key.AcceptValue(value: c.Value),
            distanceCase: static (s, c) =>
                from hit in c.Source.Closest(sample: s.Sample, key: s.Key)
                from raw in c.Source.AdmitsSignedDistance(hit: hit)
                    ? c.Source.SignedDistance(hit: hit, sample: s.Sample, key: s.Key)
                    : hit.Distance.ToFin(Fail: s.Key.InvalidResult())
                select c.Sense.Sign * raw,
            csgCase: static (s, c) =>
                from l in c.Left.SampleScalar(sample: s.Sample, context: s.Context, key: s.Key)
                from r in c.Right.SampleScalar(sample: s.Sample, context: s.Context, key: s.Key)
                select c.Op.Combine(left: l, right: r, blend: c.Smoothing),
            primitiveCase: static (s, c) => c.Shape.SignedDistance(worldPoint: s.Sample, pose: c.Pose, key: s.Key),
            laplacianCase: static (s, c) => Nabla.LaplacianAt(
                sampler: p => c.Source.SampleScalar(sample: p, context: s.Context, key: s.Key),
                point: s.Sample, eps: c.Epsilon.Value, key: s.Key),
            geodesicCase: static (s, c) => GeodesicKernel.HeatGeodesicAt(space: c.Space, sources: c.Sources, sample: s.Sample, key: s.Key),
            signedDistanceFromMeshCase: static (s, c) => MeshSdf.SignedDistanceDetailed(space: c.Space, policy: c.Policy, sample: s.Sample, key: s.Key).Map(static r => r.Distance),
            /* … remaining arms: potential/density/blend/magnitude, divergence/strain via Nabla with SampleVector
               samplers, noise (NoiseKind.Sample + the NoisePolicy fBm octave fold), worley/morse/mollifier,
               profileExtrusion (plane remap + Curve.ClosestPoint/Contains + cap/profile max-fold -> NativeProfile),
               scaled/power/clamp/onion/sdfRound via ONE SampleMapped recurse-then-map body,
               periodic (Nabla.ToroidalWrap)/twist/bend/elongate/displace domain warps,
               meanCurvatureFlow -> GeodesicKernel.MeanCurvatureMagnitudeAt, spectralDistance ->
               SegmentKernel.SpectralDistanceAt, stripe -> SegmentKernel.StripeAt, rbf/mls/levinMls/apss/tet/poisson
               reconstruct.md evaluators — every arm total, every scalar exits through key.AcceptValue. */
            poissonCase: static (s, c) => s.Key.AcceptValue(value: c.Grid.SampleTrilinear(point: s.Sample) - c.Gamma)));

    public Fin<FieldSample> SampleDetailed(Point3d sample, Context context, Op? key = null) { /* status-tagged rail:
        Primitive -> Analytic; Lipschitz-bounded composite -> ComposedAnalytic; ProfileExtrusion -> NativeProfile;
        SignedDistanceFromMesh -> MeshApproximate + SdfMeshReceipt; Rbf/Mls/LevinMls/Apss/Poisson ->
        Reconstruction + sample receipt; TetSignedHeat -> TetSignedHeat + receipts; everything else ->
        ComposedAnalytic with no distance claim. */ return default!; }
    public Fin<SdfSample> SampleSdfDetailed(Point3d sample, Context context, Op? key = null) { /* the SDF-restricted
        form: refuses (Unsupported) any species that cannot claim distance semantics; NativeProfile samples carry
        the ProfileFeature/ProfileContainment columns. */ return default!; }
}
```

## [05]-[VECTOR_FIELD]

- Owner: `VectorField` `[Union]` — vector algebra in families spanning analytic sources, proximity-driven fields, combinators and warps, differential operators, and mesh-aware solvers. Same admission law as the scalar union: mesh-aware cases construct only through their admitting factories, proving symmetry and vertex ranges once.
- Entry: same construction law as the scalar union. `Ring` and `ClusterField` derive a default `Gaussian(radius/3)` falloff; `HitField` gates on `SupportProjection.CanProjectVector`; `CrossField` proves symmetry in {1,2,4,6}.
- Auto: `SampleVector` is one total `Switch` over three shared folds — `RotationalField` (one swirl body serving `Vortex`, `Ring`, and `Helical`, where `Ring.Radius` drives only its default falloff), `RadialContribution` (the accumulating charge fold `Coulomb` and `ClusterField` share), and `ClosestDirected` (the closest-hit query feeding `Influence` shell residuals and `HitField` projections). Closed-form cases evaluate directly; differential arms delegate to the `calculus.md` `Nabla` stencil; mesh-aware arms delegate through the `MeshSpace` seam.
- Growth: a vector sample is a plain value; a new field species is one case and one arm, absorbing into a shared fold when it is a swirl, radial, or closest variant, and a provenance-tagged arm or a vector Lipschitz fold rides the existing `SdfStatus` and `Falloff.SlopeBound` columns.
- Boundary: the three shared folds are the collapse law — a new analytic case re-implementing swirl, radial accumulation, or closest-directed shaping is the rejected duplication. On-source behavior is deliberately asymmetric: `ClosestDirected` faults on a sample coincident with its support, because a hit-directed vector is undefined at its own source and a silent zero corrupts a streamline, while `RadialContribution` skips a coincident charge whose sum's remaining terms stay well-defined. `CurlNoise` refuses a non-differentiable potential at construction through a recursive fold over the payload tree — a `Worley` buried inside a `Blend` or `Csg` still refuses — so the sampler never guards.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
[Union]
public abstract partial record VectorField {
    private VectorField() { }
    // Cases as charted: Constant/Blend/Scaled/Influence/HitField/Vortex/Ring/Helical/Coulomb/ClusterField/Dipole/Harmonic/Projected/Warp/ClampMagnitude/
    // Gradient/Curl/CurlNoise/CrossProduct/BiotSavart/Saddle/CrossField/Hodge/VectorHeat/GeodesicTangent/TangentLogMap — admitted payloads throughout.

    internal Fin<Vector3d> SampleVector(Point3d sample, Context context, Op key) =>
        key.AcceptValue(value: sample).Bind(_ => Switch(state: (Sample: sample, Context: context, Key: key),
            vortexCase: static (s, c) => RotationalField(anchor: c.Anchor, axis: c.Axis, falloff: c.Falloff, axial: 0.0, swirl: 1.0, state: s),
            helicalCase: static (s, c) => RotationalField(anchor: c.Anchor, axis: c.Axis, falloff: c.Falloff, axial: c.Axial, swirl: c.Swirl, state: s),
            influenceCase: static (s, c) => ClosestDirected(source: c.Source, sample: s.Sample, sense: c.Sense, context: s.Context, key: s.Key,
                hitToScaled: (hit, op) =>
                    from distance in hit.Distance.ToFin(Fail: op.InvalidResult())
                    let residual = c.ShellRadius.Map(r => Math.Abs(distance - r.Value)).IfNone(distance)
                    let shellSign = c.ShellRadius.Map(r => distance >= r.Value ? 1.0 : -1.0).IfNone(1.0)
                    from weight in c.Falloff.Weight(offset: hit.Point - s.Sample, sample: s.Sample, tolerance: s.Context.Absolute.Value, key: op)
                    select (Raw: shellSign * (hit.Point - s.Sample), Scale: c.ShellRadius.IsSome ? residual * weight : weight)),
            hitFieldCase: static (s, c) => ClosestDirected(source: c.Source, sample: s.Sample, sense: c.Sense, context: s.Context, key: s.Key,
                hitToScaled: (hit, op) => c.Projection.Equals(SupportProjection.Span) || c.Projection.Equals(SupportProjection.SignedSpanAway)
                    ? c.Projection.Project<VectorSpan>(space: c.Source, hit: hit, sample: s.Sample, context: s.Context, key: op)
                        .Map(span => (Raw: span.Direction.Value, Scale: span.Magnitude.Value))
                    : c.Projection.Project<Vector3d>(space: c.Source, hit: hit, sample: s.Sample, context: s.Context, key: op)
                        .Map(raw => (Raw: raw, Scale: 1.0))),
            coulombCase: static (s, c) => c.Charges.Fold(Fin.Succ(Vector3d.Zero),
                (acc, charge) => acc.Bind(sum => RadialContribution(sum: sum, source: charge.Position, scale: charge.Charge, state: s, falloff: c.Falloff))),
            clusterFieldCase: static (s, c) =>
                from index in NeighborIndex.Of(source: new NeighborSource.ClusterCase(Cloud: c.Source), key: s.Key)
                from answer in index.Query(query: new NeighborQuery.RadiusCase(R: c.Radius, Cap: Option<Dimension>.None), anchor: s.Sample, key: s.Key)
                from ids in answer switch {
                    NeighborAnswer.Graph { Value.Ids: [var row] } => Fin.Succ(toSeq(row)),
                    _ => Fin.Fail<Seq<int>>(error: s.Key.InvalidResult()),
                }
                from field in ids.Fold(Fin.Succ(Vector3d.Zero),
                    (acc, i) => acc.Bind(sum => RadialContribution(sum: sum, source: c.Source.Vertices[i], scale: c.Sense.Sign, state: s, falloff: c.Falloff)))
                select field,
            gradientCase: static (s, c) => Nabla.GradientAt(
                sampler: p => c.Source.SampleScalar(sample: p, context: s.Context, key: s.Key),
                point: s.Sample, eps: c.Epsilon.Value, key: s.Key),
            crossFieldCase: static (s, c) => SegmentKernel.CrossFieldAt(space: c.Space, symmetry: c.Symmetry.Value, constraints: c.Constraints, cones: c.Cones, sample: s.Sample, key: s.Key),
            tangentLogMapCase: static (s, c) => GeodesicKernel.TangentLogMapAt(space: c.Space, source: c.Source, sample: s.Sample, time: c.Time.Value, algorithm: c.Algorithm, trace: c.Trace, windows: c.Windows, key: s.Key).Map(static r => r.Tangent)
            /* … remaining arms: constant/blend/scaled/projected/warp/clampMagnitude/crossProduct combinators,
               ring (the same RotationalField fold — Radius drives only the default falloff),
               dipole/harmonic/biotSavart/saddle closed forms,
               curl/curlNoise via Nabla.CurlAt/CurlNoiseAt with sampler closures,
               vectorHeat -> GeodesicKernel.VectorHeatAt, geodesicTangent -> GeodesicKernel.GeodesicTangentAt,
               hodge -> ONE-LINE delegation to DecAssembly.HodgeVectorAt(c.Source, c.Space, c.Sense, s.Sample,
               s.Context, s.Key) — dec.md's point-evaluation seat (edge-integrate the sampled field into a
               1-form -> HodgeDecomposeDetailed, memoized through the mesh.md Memoized slot under dec.md's
               HodgeSolutionKey -> Whitney-evaluate the sense-selected component) — never this arm's. */));

    private static Fin<Vector3d> RotationalField(Point3d anchor, Direction axis, Falloff falloff, double axial, double swirl, (Point3d Sample, Context Context, Op Key) state) {
        Vector3d r = state.Sample - anchor;
        Vector3d rPerp = r - ((r * axis.Value) * axis.Value);
        return falloff.Weight(offset: rPerp, sample: state.Sample, tolerance: state.Context.Absolute.Value, key: state.Key)
            .Map(w => w * ((axial * axis.Value) + (swirl * Vector3d.CrossProduct(a: axis.Value, b: rPerp))));
    }
    // Sample rides every Weight call so the metric (anisotropic) falloff case works on all three folds.
    private static Fin<Vector3d> RadialContribution(Vector3d sum, Point3d source, double scale, (Point3d Sample, Context Context, Op Key) state, Falloff falloff) {
        Vector3d r = state.Sample - source;
        return r.Length <= state.Context.Absolute.Value
            ? Fin.Succ(sum)
            : falloff.Weight(offset: r, sample: state.Sample, tolerance: state.Context.Absolute.Value, key: state.Key).Map(w => sum + (scale * w / r.Length * r));
    }
    private static Fin<Vector3d> ClosestDirected(SupportSpace source, Point3d sample, BoundarySense sense, Context context, Op key,
        Func<ClosestHit, Op, Fin<(Vector3d Raw, double Scale)>> hitToScaled) =>
        from hit in source.Closest(sample: sample, key: key)
        from scaled in hitToScaled(hit, key)
        from direction in Direction.Of(value: sense.Sign * scaled.Raw, context: context, key: key)
        select direction.Value * scaled.Scale;
}
```

## [06]-[TENSOR_FIELD]

- Owner: `TensorField` `[Union]` — symmetric-tensor cases, with `Lift` the one opaque-closure ingress guarded under `key.Catch` at sample time.
- Entry: `SampleTensor → Fin<SymmetricMatrix>` is the case `Switch`; `PrincipalDirections` decomposes the sample through `matrix.md` eigen; `Sampler` is the closure bridge `calculus.md` `Falloff.Metric` takes, so the anisotropic decay samples this union without calculus naming a field type.
- Auto: the `Curvature` arm is the single second-fundamental-form consumer — it reads `projections.md`'s `SurfaceProjection.ShapeOperator` at the recovered `(u,v)` and never re-derives principal curvatures. `Warp` transforms by congruence `R·M·Rᵀ` through `matrix.md`; `Blend` combines component-wise through `FieldBlend.CombineScalar` over dimension-agreeing tensors.
- Growth: a new tensor species is one case and one arm; a curvature variant delegates to its owning page, never local differential geometry.
- Boundary: `Lift` is the only closure-carrying case, its sampler running inside `key.Catch` with an `IsValid` gate — the one foreign-code seam. Congruence requires an invertible spatial map and dimension-3 tensors, both admission facts faulted not defaulted.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
[Union]
public abstract partial record TensorField {
    private TensorField() { }
    public sealed record ConstantCase(SymmetricMatrix Value) : TensorField;
    public sealed record CurvatureCase(SurfaceSpace Space) : TensorField;
    public sealed record LiftCase(Func<Point3d, SymmetricMatrix> Source) : TensorField;
    public sealed record WarpCase(TensorField Source, Transform Map) : TensorField;
    public sealed record ScaledCase(TensorField Source, double Scale) : TensorField;
    public sealed record BlendCase(Seq<TensorField> Fields, FieldBlend Mode) : TensorField;

    // Admission gates at construction: Constant proves dim 3, Warp invertibility; Lift's opaque closure is the one foreign-code seam, guarded at sample time.
    public static Fin<TensorField> Constant(SymmetricMatrix value, Op? key = null) =>
        guard(value.Dimension.Value == 3, key.OrDefault().InvalidInput()).ToFin().Map(_ => (TensorField)new ConstantCase(Value: value));
    public static TensorField Lift(Func<Point3d, SymmetricMatrix> source) => new LiftCase(Source: source);
    public static Fin<TensorField> Warp(TensorField source, Transform map, Op? key = null) =>
        guard(map.TryGetInverse(out _), key.OrDefault().InvalidInput()).ToFin().Map(_ => (TensorField)new WarpCase(Source: source, Map: map));

    internal Fin<SymmetricMatrix> SampleTensor(Point3d sample, Context context, Op key) =>
        key.AcceptValue(value: sample).Bind(_ => Switch(state: (Sample: sample, Context: context, Key: key),
            constantCase: static (s, c) => Fin.Succ(c.Value),
            // SINGLE second-fundamental-form consumer: projections.md owns the shape-operator assembly.
            curvatureCase: static (s, c) => c.Space.Native.ClosestPoint(testPoint: s.Sample, u: out double u, v: out double v)
                ? c.Space.Sample<SymmetricMatrix>(SurfaceProjection.ShapeOperator, u: u, v: v, key: s.Key)
                : Fin.Fail<SymmetricMatrix>(s.Key.InvalidResult()),
            liftCase: static (s, c) => s.Key.Catch(() => Fin.Succ(c.Source(s.Sample)))
                .Bind(raw => guard(raw.IsValid && raw.Dimension.Value == 3, s.Key.InvalidResult()).ToFin().Map(_ => raw)),
            warpCase: static (s, c) => c.Map.TryGetInverse(out Transform inverse)
                ? c.Source.SampleTensor(sample: inverse * s.Sample, context: s.Context, key: s.Key)
                    .Bind(tensor => Congruence(tensor: tensor, map: c.Map, key: s.Key))
                : Fin.Fail<SymmetricMatrix>(s.Key.InvalidInput()),
            scaledCase: static (s, c) => c.Source.SampleTensor(sample: s.Sample, context: s.Context, key: s.Key)
                .Bind(tensor => SymmetricMatrix.Of(dim: tensor.Dimension, upper: tensor.Upper.Map(v => v * c.Scale), key: s.Key)),
            blendCase: static (s, c) =>
                from samples in c.Fields.TraverseM(f => f.SampleTensor(sample: s.Sample, context: s.Context, key: s.Key)).As()
                from _ in guard(!samples.IsEmpty && samples.ForAll(m => m.Dimension == samples.Head.Dimension), s.Key.InvalidResult()).ToFin()
                from upper in toSeq(Enumerable.Range(0, samples.Head.Upper.Count))
                    .TraverseM(i => c.Mode.CombineScalar(values: samples.Map(m => m.Upper[i]), key: s.Key)).As()
                from blended in SymmetricMatrix.Of(dim: samples.Head.Dimension, upper: new Arr<double>([.. upper]), key: s.Key)
                select blended));

    public Fin<Seq<(double Eigenvalue, Direction Axis)>> PrincipalDirections(Point3d sample, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return SampleTensor(sample: sample, context: context, key: op)
            .Bind(tensor => tensor.DecomposeEigen(key: op))
            .Bind(pairs => pairs.TraverseM(pair =>
                Direction.Of(value: new Vector3d(x: pair.Eigenvector[0], y: pair.Eigenvector[1], z: pair.Eigenvector[2]), context: context, key: op)
                    .Map(axis => (pair.Eigenvalue, Axis: axis))).As());
    }

    // Closure bridge calculus.md Falloff.Metric consumes — anisotropic decay samples THIS union without calculus naming a field type.
    public Func<Point3d, Fin<SymmetricMatrix>> Sampler(Context context, Op? key = null) {
        TensorField self = this;
        Op op = key.OrDefault();
        return point => self.SampleTensor(sample: point, context: context, key: op);
    }

    // Congruence R·M·Rᵀ over the rotation block (row-major M00..M22) through matrix.md, repacked upper-triangular; dim 3 by admission.
    private static Fin<SymmetricMatrix> Congruence(SymmetricMatrix tensor, Transform map, Op key) =>
        from dim in key.AcceptValidated<Dimension>(candidate: 3)
        from rotation in Matrix.Of(rows: dim, cols: dim, entries: new Arr<double>([
            map.M00, map.M01, map.M02, map.M10, map.M11, map.M12, map.M20, map.M21, map.M22]), key: key)
        from half in rotation.Multiply(other: tensor.ToDense(), key: key)
        from full in half.Multiply(other: rotation.Transpose(), key: key)
        from packed in SymmetricMatrix.Of(dim: dim, upper: new Arr<double>([
            full.At(i: 0, j: 0), full.At(i: 0, j: 1), full.At(i: 0, j: 2),
            full.At(i: 1, j: 1), full.At(i: 1, j: 2), full.At(i: 2, j: 2)]), key: key)
        select packed;
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
