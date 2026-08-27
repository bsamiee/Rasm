# [RASM_FIELDS]

One implicit-field algebra over three closed field unions — `ScalarField`, `VectorField`, `TensorField` — each sampled anywhere in space through one per-union dispatch, composed through flattening operators, and constructed so a case's payload types are its admission structure. Raw ingress is one-expression admitting factories and multi-knob ingress rides a policy record; everything else constructs from already-admitted material, so no re-validation switch stands beside the case list. This page samples and never meshes: iso-surface extraction is `reconstruct.md`'s.

`NoiseTrait` rides the landed capability idiom rather than bool columns, carrying the lattice-caution and differentiability facts a `NoiseKind` row publishes, admitted against its own legal-corner law. Parameterless arms inside a payload-bearing `[Union]` stay marker cases — `shapes.md [02]-[COLLAPSE_FUNCTIONS]` `MergeSamePayload` preserves marker cases and row [05] fixes the owner by the FAMILY's payload timing, so `HardCase`, `ReflectCase`, and `HalfSpaceCase` are not vocabulary rows in disguise.

`calculus.md` owns the sample-anywhere math this page composes — the `Nabla` stencil, the `Falloff`/`KernelKind` weight vocabularies, and the `FieldNoise` lattices the `NoiseKind` rows point at. `reconstruct.md` owns the reconstruction kernels and mesh-SDF policy; its solvers mint the fitted payloads and `SignedDistanceFromMesh` delegates to its `MeshSdf`. Mesh-aware cases delegate through the `mesh.md` `MeshSpace` boundary. `SampleDetailed` and `SampleSdfDetailed` are the public tagged sampling entry reporting how a value was produced, and `pack.md` binds `SampleDetailed` for its scalar facet.

## [01]-[INDEX]

- [02]-[FIELD_VOCAB]: `BlendKind`, `CsgKind`, `NoiseKind` with its `NoiseTrait` set, and the ray, bounce, and provenance vocabularies, each owning its policy columns.
- [03]-[SDF_PRIMITIVES]: `SdfKind` exact analytic primitives as typed parameter cases carrying `Lipschitz` and `Distance`.
- [04]-[SCALAR_FIELD]: `ScalarField` algebra, its one total sample dispatch, the Lipschitz fold, the fitted and evaluated reconstruction cases, and the public tagged entry.
- [05]-[VECTOR_FIELD]: `VectorField` algebra over three shared radial, rotational, and closest-directed folds.
- [06]-[TENSOR_FIELD]: `TensorField` symmetric-tensor algebra, congruence transforms, and principal directions.

## [02]-[FIELD_VOCAB]

- Owner: each `BlendKind` case supplies the `ErosionFactor` column through the private base constructor the Lipschitz-erosion fold reads, so the erosion multiplier is a policy value on the row rather than a table beside the union; `HardCase` derives its factor from `min`'s own partials and the seven smooth rows carry sampled bounds tracked as a measurement owed, never as tuning literals. `NoiseKind` rows live here by the `calculus.md` split — lattices are mathematics, field rows policy: each row publishes ONE `CapabilitySet<NoiseTrait>` rather than a bool pair, so `Perlin` holds `Cautioned` for visible lattice anisotropy beside the `Differentiable` a caution presupposes, `Worley` holds neither, and the cautioned-but-non-differentiable corner no lattice produces is what `NoiseTrait.Law` refuses; `CurlNoise` admission is then one membership read. `SdfStatus` cases are the provenance the tagged samples carry, each holding exactly the evidence its species produced — the mesh solve, the reconstruction fit, or the profile feature-and-containment pair — and `SampleSdfDetailed` faults a non-distance species with a typed fault rather than mislabeling a value.
- Growth: a new lattice is one `NoiseKind` row with its trait set, admitted by the same law; a new trait is one `NoiseTrait` row with its corners on `Law`; a second seeded field family is one `FieldLane` row and the lattices it addresses decorrelate by declaration.
- Boundary: `Falloff` and `KernelKind` own their weight-profile and kernel math at `calculus.md`, composed here never re-derived; `NoiseKind` rows point at its `FieldNoise` lattices and read `NoisePolicy.Lattice`, the lane-folded seed, so no bare caller integer keys a lattice; each `BlendKind` case declares its own `ErosionFactor`, and `RayPolicy.Project` resolves through typed `ProjectionRow` entries.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record BlendKind {
    private BlendKind(double erosionFactor) => ErosionFactor = erosionFactor;
    public sealed record HardCase() : BlendKind(1.00);
    public sealed record PolynomialCase(PositiveMagnitude K) : BlendKind(1.25);
    public sealed record ExponentialCase(PositiveMagnitude K) : BlendKind(1.15);
    public sealed record RootCase(PositiveMagnitude K) : BlendKind(1.10);
    public sealed record CubicCase(PositiveMagnitude K) : BlendKind(1.30);
    public sealed record ChamferCase(PositiveMagnitude K) : BlendKind(1.50);
    public sealed record GrooveCase(PositiveMagnitude K, PositiveMagnitude D) : BlendKind(1.40);
    public sealed record RoundCase(PositiveMagnitude R) : BlendKind(1.20);

    public double ErosionFactor { get; }
    public static BlendKind Hard { get; } = new HardCase();
    public static Fin<BlendKind> Polynomial(double k, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: k).Map(static v => (BlendKind)new PolynomialCase(K: v));

    internal double Smin(double a, double b) => Switch(state: (A: a, B: b),
        hardCase: static (s, _) => Math.Min(s.A, s.B),
        polynomialCase: static (s, c) => { double h = Math.Max(c.K.Value - Math.Abs(s.A - s.B), 0.0) / c.K.Value; return Math.Min(s.A, s.B) - (h * h * h * c.K.Value / 6.0); },
        exponentialCase: static (s, c) => { double ax = -c.K.Value * s.A, bx = -c.K.Value * s.B, m = Math.Max(ax, bx); return -(m + Math.Log(Math.Exp(ax - m) + Math.Exp(bx - m))) / c.K.Value; },
        rootCase: static (s, c) => { double h = Math.Max(c.K.Value - Math.Abs(s.A - s.B), 0.0); return Math.Min(s.A, s.B) - (h * h * 0.25 / c.K.Value); },
        cubicCase: static (s, c) => { double h = Math.Max(c.K.Value - Math.Abs(s.A - s.B), 0.0) / c.K.Value; return Math.Min(s.A, s.B) - (h * h * c.K.Value * 0.25); },
        chamferCase: static (s, c) => Math.Min(Math.Min(s.A, s.B), (s.A + s.B - c.K.Value) * Math.Sqrt(0.5)),
        grooveCase: static (s, c) => Math.Max(s.A, Math.Min(c.D.Value, Math.Min(s.A - c.K.Value, s.B - c.K.Value))),
        roundCase: static (s, c) => { double ax = Math.Max(c.R.Value - s.A, 0.0), bx = Math.Max(c.R.Value - s.B, 0.0); return Math.Max(c.R.Value, Math.Min(s.A, s.B)) - Math.Sqrt((ax * ax) + (bx * bx)); });
}

[SmartEnum]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(combine: static (a, b, blend) => blend.Smin(a, b));
    public static readonly CsgKind Intersect = new(combine: static (a, b, blend) => -blend.Smin(-a, -b));
    public static readonly CsgKind Difference = new(combine: static (a, b, blend) => -blend.Smin(-a, b));
    [UseDelegateFromConstructor] internal partial double Combine(double left, double right, BlendKind blend);
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
    internal Fin<TOut> Project<TOut>(Point3d origin, Direction direction, Context context, Op key);
}

[Union]
public abstract partial record BouncePolicy {
    private BouncePolicy() { }
    public sealed record ReflectCase : BouncePolicy;
    public sealed record RefractCase(PositiveMagnitude EtaIncident, PositiveMagnitude EtaTransmitted) : BouncePolicy;
    public static BouncePolicy Reflect { get; } = new ReflectCase();
    public static Fin<BouncePolicy> Refract(double etaIncident, double etaTransmitted, Op? key = null) {
        Op op = key.OrDefault();
        return (op.AcceptValidated<PositiveMagnitude>(etaIncident).ToValidation(),
                op.AcceptValidated<PositiveMagnitude>(etaTransmitted).ToValidation())
            .Apply(static (i, t) => (BouncePolicy)new RefractCase(i, t)).As().ToFin();
    }
    internal Fin<Direction> Apply(Direction incident, Direction normal, Op key) => Switch(
        state: (Incident: incident, Normal: normal, Key: key),
        reflectCase: static (s, _) => Fin.Succ(s.Incident.Reflect(normal: s.Normal)),
        refractCase: static (s, r) => Direction.Refract(incident: s.Incident, normal: s.Normal,
            etaIncident: r.EtaIncident.Value, etaTransmitted: r.EtaTransmitted.Value, key: s.Key));
}

[SmartEnum<long>(KeyMemberName = nameof(IDrawLane<FieldLane>.Lane))]
public sealed partial class FieldLane : IDrawLane<FieldLane> {
    public static readonly FieldLane Noise = new(0L);
}

[Union]
public abstract partial record SdfStatus {
    private SdfStatus() { }
    public sealed record AnalyticCase(bool Composed) : SdfStatus;
    public sealed record NativeProfileCase(ProfileExtrusionFeature Feature, PointContainment Containment) : SdfStatus;
    public sealed record MeshApproximateCase(SdfSolve Solve) : SdfStatus;
    public sealed record ReconstructionCase(SampleFit Fit) : SdfStatus;
}

[SmartEnum]
public sealed partial class ProfileExtrusionFeature {
    public static readonly ProfileExtrusionFeature Interior = new();
    public static readonly ProfileExtrusionFeature ProfileBoundary = new();
    public static readonly ProfileExtrusionFeature Cap = new();
    public static readonly ProfileExtrusionFeature Rim = new();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NoiseTrait : ICapability<NoiseTrait> {
    public static readonly NoiseTrait Cautioned = new(key: "cautioned", rank: 0);
    public static readonly NoiseTrait Differentiable = new(key: "differentiable", rank: 1);
    public int Rank { get; }
    internal static readonly CapabilityLaw<NoiseTrait> Law = new(Legal: Seq(
        CapabilitySet<NoiseTrait>.None,
        CapabilitySet<NoiseTrait>.Of(Differentiable),
        CapabilitySet<NoiseTrait>.Of(Cautioned, Differentiable)));
}

[SmartEnum]
public sealed partial class NoiseKind {
    public static readonly NoiseKind Perlin = new(declared: CapabilitySet<NoiseTrait>.Of(NoiseTrait.Cautioned, NoiseTrait.Differentiable),
        sample: static (p, seed, f) => FieldNoise.PerlinAt(point: p, seed: seed, frequency: f));
    public static readonly NoiseKind Simplex = new(declared: CapabilitySet<NoiseTrait>.Of(NoiseTrait.Differentiable),
        sample: static (p, seed, f) => FieldNoise.SimplexAt(p, seed, f, rotationMix: 0.0));
    public static readonly NoiseKind RotatedSimplex = new(declared: CapabilitySet<NoiseTrait>.Of(NoiseTrait.Differentiable),
        sample: static (p, seed, f) => FieldNoise.SimplexAt(p, seed, f, rotationMix: 0.5));
    public static readonly NoiseKind Worley = new(declared: CapabilitySet<NoiseTrait>.None,
        sample: static (p, seed, f) => FieldNoise.WorleyAt(point: p, seed: seed, frequency: f));
    private CapabilitySet<NoiseTrait> Declared { get; }
    [UseDelegateFromConstructor] internal partial double Sample(Point3d point, int seed, double frequency);
    public CapabilitySet<NoiseTrait> Traits { get { _ = Lawful.Value; return Declared; } }
    private static readonly Lazy<Unit> Lawful = new(static () =>
        toSeq(Items).Fold(unit, static (_, row) => NoiseTrait.Law.Admit(held: row.Declared).Match(
            Succ: static _ => unit,
            Fail: static _ => throw new InvalidOperationException("Noise traits violate their capability law."))));
}
```

## [03]-[SDF_PRIMITIVES]

- Owner: `SdfKind` `[Union]` — the exact analytic signed-distance primitives, each a typed parameter record carrying its own `Lipschitz` bound column and `Distance(Point3d local)` member. Typed records make a wrong parameter a compile error and a missing one unconstructible.
- Entry: `SignedDistance` remaps world to pose space once through `Plane.RemapToPlaneSpace`, then dispatches the case's `Distance`, computing the Inigo Quilez exact forms. Cross-payload guards ride the factories: `CappedCone` demands one positive radius, and `Cone` proves `HalfAngle < π/2` because `tan` flips sign past it and the derived base radius goes negative.
- Growth: a new primitive is one typed case handing its `Lipschitz` bound to the base constructor and overriding `Distance`; the `ScalarField.PrimitiveCase` payload, the Lipschitz fold, and the tagged sampler pick it up through the union.
- Boundary: `Distance` bodies are pure local-frame math — pose handling happens once at `SignedDistance`, never inside a case. `Lipschitz` is load-bearing — the ray-march step bound and the CSG erosion fold read the column — so a case without an honest bound is inadmissible.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record SdfKind {
    private SdfKind(double lipschitz) => Lipschitz = lipschitz;
    public sealed record SphereCase(PositiveMagnitude Radius) : SdfKind(1.0) {
        internal override double Distance(Point3d p) => Math.Sqrt((p.X * p.X) + (p.Y * p.Y) + (p.Z * p.Z)) - Radius.Value;
    }
    public sealed record BoxCase(PositiveMagnitude X, PositiveMagnitude Y, PositiveMagnitude Z) : SdfKind(1.0) {
        internal override double Distance(Point3d p) {
            (double qx, double qy, double qz) = (Math.Abs(p.X) - X.Value, Math.Abs(p.Y) - Y.Value, Math.Abs(p.Z) - Z.Value);
            (double ox, double oy, double oz) = (Math.Max(qx, 0.0), Math.Max(qy, 0.0), Math.Max(qz, 0.0));
            return Math.Sqrt((ox * ox) + (oy * oy) + (oz * oz)) + Math.Min(Math.Max(qx, Math.Max(qy, qz)), 0.0);
        }
    }
    public sealed record CapsuleCase(PositiveMagnitude HalfHeight, PositiveMagnitude Radius) : SdfKind(1.0) {
        internal override double Distance(Point3d p) {
            double pz = p.Z - Math.Clamp(p.Z, -HalfHeight.Value, HalfHeight.Value);
            return Math.Sqrt((p.X * p.X) + (p.Y * p.Y) + (pz * pz)) - Radius.Value;
        }
    }
    public sealed record CylinderCase(PositiveMagnitude HalfHeight, PositiveMagnitude Radius) : SdfKind(1.0) { internal override double Distance(Point3d p) => CappedProfile(dxy: Math.Sqrt((p.X * p.X) + (p.Y * p.Y)) - Radius.Value, dz: Math.Abs(p.Z) - HalfHeight.Value); }
    public sealed record ConeCase(PositiveMagnitude Height, VectorAngle HalfAngle) : SdfKind(1.0) { internal override double Distance(Point3d p) => CappedCone(p: new Point3d(p.X, p.Y, p.Z + (0.5 * Height.Value)), halfHeight: 0.5 * Height.Value, r1: Height.Value * Math.Tan(HalfAngle.Value), r2: 0.0); }
    public sealed record HalfSpaceCase() : SdfKind(1.0) { internal override double Distance(Point3d p) => p.Z; }
    public sealed record CappedConeCase(PositiveMagnitude HalfHeight, double R1, double R2) : SdfKind(1.2) {
        internal override double Distance(Point3d p) => CappedCone(p, HalfHeight.Value, R1, R2);
    }
    public sealed record TorusCase(PositiveMagnitude Major, PositiveMagnitude Minor) : SdfKind(1.0) { internal override double Distance(Point3d p) { double qx = Math.Sqrt((p.X * p.X) + (p.Y * p.Y)) - Major.Value; return Math.Sqrt((qx * qx) + (p.Z * p.Z)) - Minor.Value; } }
    public sealed record HexPrismCase(PositiveMagnitude HalfHeight, PositiveMagnitude Circumradius) : SdfKind(1.0) {
        internal override double Distance(Point3d p) {
            const double kx = -0.8660254037844386, ky = 0.5, kz = 0.5773502691896258;
            double h = -kx * Circumradius.Value;
            (double ax, double ay) = (Math.Abs(p.X), Math.Abs(p.Y));
            double fold = 2.0 * Math.Min((kx * ax) + (ky * ay), 0.0);
            (ax, ay) = (ax - (fold * kx), ay - (fold * ky));
            (double ex, double ey) = (ax - Math.Clamp(ax, -kz * h, kz * h), ay - h);
            double dxy = Math.Sqrt((ex * ex) + (ey * ey)) * Math.Sign(ey);
            return CappedProfile(dxy: dxy, dz: Math.Abs(p.Z) - HalfHeight.Value);
        }
    }
    public sealed record OctahedronCase(PositiveMagnitude S) : SdfKind(1.0) { internal override double Distance(Point3d p) => ExactOctahedron(p: p, s: S.Value); }
    public sealed record EllipsoidCase(PositiveMagnitude X, PositiveMagnitude Y, PositiveMagnitude Z) : SdfKind(2.0) { internal override double Distance(Point3d p) { double k0 = new Vector3d(x: p.X / X.Value, y: p.Y / Y.Value, z: p.Z / Z.Value).Length; double k1 = new Vector3d(x: p.X / (X.Value * X.Value), y: p.Y / (Y.Value * Y.Value), z: p.Z / (Z.Value * Z.Value)).Length; return k1 > EpsilonPolicy.ZeroTolerance ? k0 * (k0 - 1.0) / k1 : -Math.Min(val1: X.Value, val2: Math.Min(val1: Y.Value, val2: Z.Value)); } }
    public sealed record SlabCase(PositiveMagnitude HalfHeight) : SdfKind(1.0) { internal override double Distance(Point3d p) => Math.Abs(p.Z) - HalfHeight.Value; }

    public double Lipschitz { get; }
    internal abstract double Distance(Point3d local);
    public static Fin<SdfKind> CappedCone(double halfHeight, double r1, double r2, Op? key = null) {
        Op op = key.OrDefault();
        return (op.AcceptValidated<PositiveMagnitude>(halfHeight).ToValidation(),
                guard(r1 >= 0.0 && r2 >= 0.0 && (r1 > 0.0 || r2 > 0.0) && double.IsFinite(r1) && double.IsFinite(r2), op.InvalidInput()).ToFin().ToValidation())
            .Apply((h, _) => (SdfKind)new CappedConeCase(h, r1, r2)).As().ToFin();
    }
    public static Fin<SdfKind> Cone(double height, double halfAngleRadians, Op? key = null) {
        Op op = key.OrDefault();
        return (op.AcceptValidated<PositiveMagnitude>(height).ToValidation(),
                op.AcceptValidated<VectorAngle>(halfAngleRadians).ToValidation())
            .Apply(static (h, a) => (Height: h, HalfAngle: a)).As().ToFin()
            .Bind(pair => guard(pair.HalfAngle.Value < Math.PI / 2.0, op.InvalidInput()).ToFin()
                .Map(_ => (SdfKind)new ConeCase(pair.Height, pair.HalfAngle)));
    }

    internal Fin<double> SignedDistance(Point3d worldPoint, Plane pose, Op key) =>
        pose.RemapToPlaneSpace(ptSample: worldPoint, ptPlane: out Point3d local)
            ? key.AcceptValue(value: Distance(local: local))
            : Fin.Fail<double>(key.InvalidResult());
}
```

## [04]-[SCALAR_FIELD]

- Owner: `ScalarField` `[Union]` — the scalar algebra in case families spanning analytic sources, combinators, domain warps, differential operators, mesh-aware solvers, reconstruction, and the lattice-backed `LatticeCase` that makes a baked or imported plane a first-class field. Mesh-aware and reconstruction cases construct only through their admitting factories, never `new`, so the factory proves sources against the `MeshSpace` range, the fitted payload against its `reconstruct.md` minter, or the value census against the admitting `CellLattice`. Reconstruction cases split by payload timing, not by kernel: a FITTED case (`Rbf`, `Poisson`) carries the solved coefficients its minter produced, an EVALUATED case (`Mls`, `LevinMls`, `Apss`, `Sibson`) carries the admitted sample set and its support or tolerance and solves per query, so a coefficient array on an evaluated case names a solve that never ran. `Noise` takes the `NoisePolicy` record.
- Auto: `SampleScalar` is the one total generated `Switch` over the union — analytic sources evaluate closed forms, combinators recurse, warps pre-transform the sample and recurse, differential arms delegate to the `calculus.md` `Nabla` stencil with sampler closures the stencil never learns the union from, mesh-aware arms delegate through the `MeshSpace` boundary, and reconstruction arms evaluate the fitted payload through `reconstruct.md`. One shared `SampleMapped` body collapses the map-only warps, and one `ReconstructLattice` body is the sample reconstruction every lattice-backed arm (`LatticeCase`, `PoissonCase`) reads through its `LatticeInterpolation` row's own `CenterOffset` and `Axis` columns, so the body carries no per-row branch. `SampleLattice` is the one batch sweep — every cell centre through the same fold, the first failure carrying its cell coordinate. `LipschitzBound` is a per-case column the private base constructor demands, not a fold with a catch-all: an over-claimed bound overshoots ray-march steps into silently missed surfaces and an unstated one silently disables the bound, so every case hands one to the base — `Twist`, `Bend`, `Periodic`, the sampled `LatticeCase`, and every mesh and reconstruction species answer `None` by decision, and a new case cannot compile without deciding.
- Output: `SampleDetailed → Fin<FieldSample>` is the public tagged result carrying value and `SdfStatus` provenance, whose case holds the mesh solve or reconstruction fit that produced it. `SampleSdfDetailed → Fin<SdfSample>` refuses a species with no distance semantics, faulting `Unsupported` rather than mislabeling a value as a distance; the profile-extrusion feature and containment pair rides `SdfStatus.NativeProfileCase` alone.
- Packages: `RhinoCommon`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`.
- Growth: a new scalar species is one case, one `Switch` arm, and one `LipschitzBound` declaration the compiler demands, a factory only when raw material enters; a new CSG mode is a vocabulary row; a new provenance species is one `SdfStatus` case carrying its evidence.
- Boundary: mesh-aware arms are one-line delegations, and any solver math here is a mis-homed body. `SampleScalar` assumes admitted fields, so an in-arm re-validation is double admission. Tagged sampling is the one public entry; a second `Evaluate` or `Probe` family is the rejected surface.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct NoisePolicy(int Seed, FieldLane Lane, Dimension Octaves, PositiveMagnitude Persistence, PositiveMagnitude Lacunarity, PositiveMagnitude Frequency) {
    internal int Lattice => unchecked((int)Deterministic.Stream(lanes: [Seed, Lane.Lane]));
    public static Fin<NoisePolicy> Of(int seed, int octaves, double persistence, double lacunarity, double frequency, Option<FieldLane> lane = default, Op? key = null) {
        Op op = key.OrDefault();
        return (op.AcceptValidated<Dimension>(octaves).ToValidation(),
                op.AcceptValidated<PositiveMagnitude>(persistence).ToValidation(),
                op.AcceptValidated<PositiveMagnitude>(lacunarity).ToValidation(),
                op.AcceptValidated<PositiveMagnitude>(frequency).ToValidation())
            .Apply((count, gain, gap, rate) => new NoisePolicy(seed, lane.IfNone(FieldLane.Noise), count, gain, gap, rate)).As().ToFin();
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FieldSample(double Value, SdfStatus Status);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SdfSample(double Value, SdfStatus Status, Option<double> LipschitzBound);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union]
public abstract partial record ScalarField {
    private ScalarField(Option<double> lipschitzBound) => LipschitzBound = lipschitzBound;
    public Option<double> LipschitzBound { get; }
    // --- [ANALYTIC_SOURCES]
    public sealed record ConstantCase(double Value) : ScalarField(Some(0.0));
    public sealed record DistanceCase(SupportSpace Source, BoundarySense Sense) : ScalarField(Some(1.0));
    public sealed record PrimitiveCase(SdfKind Shape, Plane Pose) : ScalarField(Some(Shape.Lipschitz));
    public sealed record ProfileExtrusionCase(Curve Profile, Plane Plane, PositiveMagnitude HalfHeight) : ScalarField(Some(1.0));
    public sealed record WorleyCase(Seq<Point3d> Seeds, Dimension Order) : ScalarField(Some(1.0));
    public sealed record MorseCase(Point3d Center, PositiveMagnitude Depth, PositiveMagnitude Width) : ScalarField(Some(Depth.Value / (2.0 * Width.Value)));
    public sealed record DensityCase(Point3d Center, PositiveMagnitude Spread, double Strength) : ScalarField(Some(Math.Abs(Strength) * Math.Exp(-0.5) / Spread.Value));
    public sealed record PotentialCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : ScalarField(Falloff.SlopeBound.Map(slope => Charges.Fold(0.0, static (sum, charge) => sum + Math.Abs(charge.Charge)) * slope));
    public sealed record MollifierCase(Point3d Center, PositiveMagnitude Radius) : ScalarField(None);

    // --- [COMBINATORS]
    public sealed record BlendCase(Seq<ScalarField> Fields, bool Average) : ScalarField(
        Fields.TraverseM(static f => f.LipschitzBound).As().Filter(static bounds => !bounds.IsEmpty)
            .Map(bounds => bounds.Fold(0.0, static (sum, bound) => sum + bound) / (Average ? bounds.Count : 1)));
    public sealed record ScaledCase(ScalarField Source, double Scale) : ScalarField(Source.LipschitzBound.Map(l => Math.Abs(Scale) * l));
    public sealed record CsgCase(ScalarField Left, ScalarField Right, CsgKind Op, BlendKind Smoothing) : ScalarField(from l in Left.LipschitzBound from r in Right.LipschitzBound select Smoothing.ErosionFactor * Math.Max(l, r));
    public sealed record DisplaceCase(ScalarField Source, ScalarField Displacement) : ScalarField(from l in Source.LipschitzBound from r in Displacement.LipschitzBound select l + r);
    public sealed record ClampCase(ScalarField Source, double Minimum, double Maximum) : ScalarField(Source.LipschitzBound);
    public sealed record OnionCase(ScalarField Source, PositiveMagnitude Thickness) : ScalarField(Source.LipschitzBound);
    public sealed record SdfRoundCase(ScalarField Source, PositiveMagnitude Radius) : ScalarField(Source.LipschitzBound);
    public sealed record ElongateCase(ScalarField Source, Vector3d Extent) : ScalarField(Source.LipschitzBound);
    public sealed record PowerCase(ScalarField Source, double Exponent) : ScalarField(None);

    // --- [DOMAIN_WARPS]
    public sealed record PeriodicCase(ScalarField Source, Vector3d Period) : ScalarField(None);
    public sealed record TwistCase(ScalarField Source, double AnglePerUnit, Direction Axis) : ScalarField(None);
    public sealed record BendCase(ScalarField Source, double Curvature, Direction Axis) : ScalarField(None);

    // --- [DIFFERENTIAL]
    public sealed record MagnitudeCase(VectorField Source) : ScalarField(None);
    public sealed record DivergenceCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField(None);
    public sealed record LaplacianCase(ScalarField Source, PositiveMagnitude Epsilon) : ScalarField(None);
    public sealed record StrainMagnitudeCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField(None);

    // --- [SAMPLED]
    public sealed record NoiseCase(NoiseKind Kind, NoisePolicy Policy) : ScalarField(None);
    public sealed record LatticeCase(CellLattice Grid, Arr<double> Values, LatticeInterpolation Interp) : ScalarField(None);

    // --- [MESH_SOLVERS]
    public sealed record GeodesicCase(MeshSpace Space, Seq<int> Sources) : ScalarField(None);
    public sealed record MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, Dimension Iterations) : ScalarField(None);
    public sealed record SpectralDistanceCase(MeshSpace Space, SpectralFilter Filter, Seq<int> Sources, Dimension Pairs) : ScalarField(None);
    public sealed record StripeCase(MeshSpace Space, VectorField CrossField, PositiveMagnitude Frequency) : ScalarField(None);
    public sealed record SignedDistanceFromMeshCase(MeshSpace Space, SdfMeshPolicy Policy) : ScalarField(None);

    // --- [RECONSTRUCTION]
    public sealed record RbfCase(Seq<(Point3d Position, double Value)> Samples, KernelKind Kernel, PositiveMagnitude Radius, Arr<double> Coefficients, ReconstructionFit Fit) : ScalarField(None);
    public sealed record MlsCase(Seq<MlsSample> Samples, KernelKind Kernel, PositiveMagnitude Radius, ReconstructionFit Fit) : ScalarField(None);
    public sealed record LevinMlsCase(Seq<MlsSample> Samples, LevinMlsPolicy Policy, ReconstructionFit Fit) : ScalarField(None);
    public sealed record ApssCase(Seq<MlsSample> Samples, ApssPolicy Policy, ReconstructionFit Fit) : ScalarField(None);
    public sealed record SibsonCase(NaturalNeighborField Field, Arr<double> Values, ReconstructionFit Fit) : ScalarField(None);
    public sealed record PoissonCase(PoissonGrid Grid, double Gamma, PoissonSolve Solve) : ScalarField(None);

    public static Fin<ScalarField> Lattice(CellLattice grid, Arr<double> values, Option<LatticeInterpolation> interp = default, Op? key = null) =>
        from _ in guard(values.Count == grid.CellCount && values.ForAll(double.IsFinite), key.OrDefault().InvalidInput()).ToFin()
        select (ScalarField)new LatticeCase(Grid: grid, Values: values, Interp: interp.IfNone(LatticeInterpolation.Linear));
    public static Fin<ScalarField> Density(Point3d center, double spread, double strength, Op? key = null) {
        Op op = key.OrDefault();
        return (op.AcceptValidated<PositiveMagnitude>(spread).ToValidation(),
                guard(double.IsFinite(strength) && center.IsValid, op.InvalidInput()).ToFin().ToValidation())
            .Apply((s, _) => (ScalarField)new DensityCase(center, s, strength)).As().ToFin();
    }
    public static Fin<ScalarField> Geodesic(MeshSpace space, Seq<int> sources, Op? key = null) =>
        guard(!sources.IsEmpty && sources.ForAll(v => v >= 0 && v < space.Native.Vertices.Count), key.OrDefault().InvalidInput())
            .ToFin().Map(_ => (ScalarField)new GeodesicCase(Space: space, Sources: sources));

    public static ScalarField operator +(ScalarField left, ScalarField right) =>
        new BlendCase(Fields: (left is BlendCase { Average: false } lb ? lb.Fields : Seq(left))
            .Concat(right is BlendCase { Average: false } rb ? rb.Fields : Seq(right)), Average: false);
    public static ScalarField operator -(ScalarField left, ScalarField right) => left + (-right);
    public static ScalarField operator -(ScalarField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static ScalarField operator *(ScalarField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static ScalarField operator *(double scale, ScalarField field) => new ScaledCase(Source: field, Scale: scale);

    internal Fin<double> SampleScalar(Point3d sample, Context context, Op key) =>
        key.AcceptValue(value: sample).Bind(_ => Switch(state: (Sample: sample, Context: context, Key: key),
            constantCase: static (s, c) => s.Key.AcceptValue(value: c.Value),
            distanceCase: static (s, c) =>
                from hit in c.Source.Closest(sample: s.Sample, key: s.Key)
                from raw in c.Source.SignedReach(hit: hit)
                    ? c.Source.SignedDistance(sample: s.Sample, key: s.Key)
                    : hit.Distance.ToFin(Fail: s.Key.InvalidResult())
                select c.Sense.Sign * raw,
            csgCase: static (s, c) =>
                (c.Left.SampleScalar(s.Sample, s.Context, s.Key).ToValidation(),
                 c.Right.SampleScalar(s.Sample, s.Context, s.Key).ToValidation())
                .Apply((l, r) => c.Op.Combine(l, r, c.Smoothing)).As().ToFin(),
            primitiveCase: static (s, c) => c.Shape.SignedDistance(worldPoint: s.Sample, pose: c.Pose, key: s.Key),
            laplacianCase: static (s, c) => Nabla.LaplacianAt(
                sampler: p => c.Source.SampleScalar(sample: p, context: s.Context, key: s.Key),
                point: s.Sample, eps: c.Epsilon.Value, key: s.Key),
            geodesicCase: static (s, c) => GeodesicKernel.HeatGeodesicAt(space: c.Space, sources: c.Sources, sample: s.Sample, key: s.Key),
            signedDistanceFromMeshCase: static (s, c) => MeshSdf.SignedDistanceDetailed(space: c.Space, policy: c.Policy, sample: s.Sample, key: s.Key).Map(static r => r.Distance),
            latticeCase: static (s, c) => s.Key.AcceptValue(value: ReconstructLattice(grid: c.Grid, values: c.Values, interp: c.Interp, local: c.Grid.Locate(sample: s.Sample))),
            poissonCase: static (s, c) => s.Key.AcceptValue(value: ReconstructLattice(grid: c.Grid.Grid, values: c.Grid.Chi, interp: LatticeInterpolation.Linear, local: c.Grid.Grid.Locate(sample: s.Sample)) - c.Gamma)));

    private static double ReconstructLattice(CellLattice grid, Arr<double> values, LatticeInterpolation interp, Point3d local) {
        (int columns, int rows, int layers) = (grid.Columns.Value, grid.Rows.Value, grid.Layers.Value);
        static int Clamp(int index, int count) => Math.Clamp(value: index, min: 0, max: count - 1);
        double At(int c, int r, int l) => values[(int)grid.Linear(column: Clamp(index: c, count: columns), row: Clamp(index: r, count: rows), layer: Clamp(index: l, count: layers))];
        (double x, double y, double z) = (local.X - interp.CenterOffset, local.Y - interp.CenterOffset,
            grid.Rank is 3 ? local.Z - interp.CenterOffset : 0.0);
        (int cx, int cy, int cz) = ((int)Math.Floor(d: x), (int)Math.Floor(d: y), (int)Math.Floor(d: z));
        (double fx, double fy, double fz) = (x - cx, y - cy, z - cz);
        double Plane(int dz) => interp.Axis(tap: dy => interp.Axis(tap: dx => At(c: cx + dx, r: cy + dy, l: cz + dz), t: fx), t: fy);
        return grid.Rank is 3 ? interp.Axis(tap: Plane, t: fz) : Plane(dz: 0);
    }

    public Fin<Arr<double>> SampleLattice(CellLattice grid, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(grid.CellCount <= int.MaxValue, op.InvalidInput()).ToFin()
               from values in toSeq(Enumerable.Range(0, (int)grid.CellCount)).TraverseM(index => {
                   (int column, int row, int layer) = grid.Coordinate(linear: index);
                   return SampleScalar(sample: grid.Center(column: column, row: row, layer: layer), context: context, key: op)
                       .MapFail(cause => cause + op.InvalidResult(detail: $"lattice-cell:{column},{row},{layer}"));
               }).As()
               select Arr.createRange(values);
    }

    public Fin<FieldSample> SampleDetailed(Point3d sample, Context context, Op? key = null);
    public Fin<SdfSample> SampleSdfDetailed(Point3d sample, Context context, Op? key = null);
}
```

## [05]-[VECTOR_FIELD]

- Owner: `VectorField` `[Union]` — vector algebra in families spanning analytic sources, proximity-driven fields, combinators and warps, differential operators, and mesh-aware solvers. Same admission law as the scalar union: mesh-aware cases construct only through their admitting factories, proving symmetry and vertex ranges once.
- Entry: same construction law as the scalar union. `Ring` and `ClusterField` derive a default `Gaussian(radius/3)` falloff; `HitField` gates on `SupportProjection.CanProject<Vector3d>`; `CrossField` proves symmetry in {1,2,4,6}.
- Auto: `SampleVector` is one total `Switch` over three shared folds — `RotationalField` (one swirl body serving `Vortex`, `Ring`, and `Helical`, where `Ring.Radius` drives only its default falloff), `RadialContribution` (the per-source radial term `Coulomb` and `ClusterField` traverse applicatively, then sum in one pure fold), and `ClosestDirected` (the closest-hit query feeding `Influence` shell residuals and `HitField` projections). Closed-form cases evaluate directly; differential arms delegate to the `calculus.md` `Nabla` stencil; mesh-aware arms delegate through the `MeshSpace` boundary.
- Growth: a vector sample is a plain value; a new field species is one case and one arm, absorbing into a shared fold when it is a swirl, radial, or closest variant, and a provenance-tagged arm or a vector Lipschitz fold rides the existing `SdfStatus` and `Falloff.SlopeBound` columns.
- Boundary: the three shared folds are the collapse law — a new analytic case re-implementing swirl, radial accumulation, or closest-directed shaping is the rejected duplication. On-source behavior is deliberately asymmetric: `ClosestDirected` faults on a sample coincident with its support, because a hit-directed vector is undefined at its own source and a silent zero corrupts a streamline, while `RadialContribution` answers a zero term for a coincident charge, whose remaining terms stay well-defined. `CurlNoise` refuses a potential whose noise rows do not all hold `NoiseTrait.Differentiable`, at construction, through a recursive fold over the payload tree — a `Worley` buried inside a `Blend` or `Csg` still refuses — so the sampler never guards.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
[Union]
public abstract partial record VectorField {
    private VectorField() { }

    internal Fin<Vector3d> SampleVector(Point3d sample, Context context, Op key) =>
        key.AcceptValue(value: sample).Bind(_ => Switch(state: (Sample: sample, Context: context, Key: key),
            vortexCase: static (s, c) => RotationalField(anchor: c.Anchor, axis: c.Axis, falloff: c.Falloff, axial: 0.0, swirl: 1.0, state: s),
            helicalCase: static (s, c) => RotationalField(anchor: c.Anchor, axis: c.Axis, falloff: c.Falloff, axial: c.Axial, swirl: c.Swirl, state: s),
            influenceCase: static (s, c) => ClosestDirected(source: c.Source, sample: s.Sample, sense: c.Sense, context: s.Context, key: s.Key,
                hitToScaled: (hit, op) =>
                    from distance in hit.Distance.ToFin(Fail: op.InvalidResult())
                    let residual = c.ShellRadius.Map(r => Math.Abs(distance - r.Value)).IfNone(distance)
                    let shellSign = c.ShellRadius.Map(r => distance >= r.Value ? 1.0 : -1.0).IfNone(1.0)
                    from weight in c.Falloff.Weight(offset: hit.Point - s.Sample, sample: s.Sample, tolerance: s.Context.For(lane: ToleranceLane.Duplicate).Value, key: op)
                    select (Raw: shellSign * (hit.Point - s.Sample), Scale: c.ShellRadius.IsSome ? residual * weight : weight)),
            hitFieldCase: static (s, c) => ClosestDirected(source: c.Source, sample: s.Sample, sense: c.Sense, context: s.Context, key: s.Key,
                hitToScaled: (hit, op) => c.Projection
                    .Project<Vector3d>(space: c.Source, hit: hit, sample: s.Sample, context: s.Context, key: op)
                    .Map(static raw => (Raw: raw, Scale: 1.0))),
            coulombCase: static (s, c) => c.Charges
                .Traverse(charge => RadialContribution(charge.Position, charge.Charge, s, c.Falloff).ToValidation()).As()
                .Map(static terms => terms.Fold(Vector3d.Zero, static (sum, term) => sum + term)).ToFin(),
            clusterFieldCase: static (s, c) =>
                from index in NeighborIndex.Of(source: new NeighborSource.ClusterCase(Cloud: c.Source), key: s.Key)
                from answer in index.Query(query: new NeighborQuery.RadiusCase(R: c.Radius, Cap: Option<Dimension>.None, Metric: NeighborMetric.Euclidean), anchor: s.Sample, key: s.Key)
                from ids in answer switch {
                    NeighborAnswer.Graph { Value.Ids: [var row] } => Fin.Succ(toSeq(row)),
                    _ => Fin.Fail<Seq<int>>(error: s.Key.InvalidResult()),
                }
                from terms in ids.Traverse(i => RadialContribution(c.Source.Vertices[i], c.Sense.Sign, s, c.Falloff).ToValidation()).As().ToFin()
                select terms.Fold(Vector3d.Zero, static (sum, term) => sum + term),
            gradientCase: static (s, c) => Nabla.GradientAt(
                sampler: p => c.Source.SampleScalar(sample: p, context: s.Context, key: s.Key),
                point: s.Sample, eps: c.Epsilon.Value, key: s.Key),
            crossFieldCase: static (s, c) => SegmentKernel.CrossFieldAt(space: c.Space, symmetry: c.Symmetry.Value, constraints: c.Constraints, cones: c.Cones, sample: s.Sample, key: s.Key),
            tangentLogMapCase: static (s, c) => GeodesicKernel.TangentLogMapAt(space: c.Space, source: c.Source, sample: s.Sample, time: c.Time.Value, algorithm: c.Algorithm, trace: c.Trace, windows: c.Windows, key: s.Key).Map(static r => r.Tangent)
            ));

    private static Fin<Vector3d> RotationalField(Point3d anchor, Direction axis, Falloff falloff, double axial, double swirl, (Point3d Sample, Context Context, Op Key) state) {
        Vector3d r = state.Sample - anchor;
        Vector3d rPerp = r - ((r * axis.Value) * axis.Value);
        return falloff.Weight(offset: rPerp, sample: state.Sample, tolerance: state.Context.For(lane: ToleranceLane.Duplicate).Value, key: state.Key)
            .Map(w => w * ((axial * axis.Value) + (swirl * Vector3d.CrossProduct(a: axis.Value, b: rPerp))));
    }
    private static Fin<Vector3d> RadialContribution(Point3d source, double scale, (Point3d Sample, Context Context, Op Key) state, Falloff falloff) {
        Vector3d r = state.Sample - source;
        return r.Length <= state.Context.For(ToleranceLane.Duplicate).Value
            ? Fin.Succ(Vector3d.Zero)
            : falloff.Weight(r, state.Sample, state.Context.For(ToleranceLane.Duplicate).Value, state.Key).Map(w => scale * w / r.Length * r);
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

- Owner: `TensorField` `[Union]` — symmetric-tensor cases, with `LiftCase` the one opaque-closure ingress guarded under `key.Catch` at sample time.
- Entry: `SampleTensor → Fin<SymmetricMatrix>` is the case `Switch`; `PrincipalDirections` decomposes the sample through `matrix.md` eigen; `Sampler` is the closure bridge `calculus.md` `Falloff.Metric` takes, so the anisotropic decay samples this union without calculus naming a field type.
- Auto: the `Curvature` arm is the single second-fundamental-form consumer — it reads `projections.md`'s `SurfaceProjection.ShapeOperator` at the recovered `(u,v)` and never re-derives principal curvatures. `Warp` transforms by congruence `R·M·Rᵀ` through `matrix.md`; `Blend` traverses its dimension-agreeing tensors applicatively and sums component-wise, dividing by count when `Average` holds.
- Growth: a new tensor species is one case and one arm; a curvature variant delegates to its owning page, never local differential geometry.
- Boundary: `LiftCase` is the only closure-carrying case, its sampler running inside `key.Catch` with an `IsValid` gate — the one foreign-code boundary. Congruence requires an invertible spatial map and dimension-3 tensors, both admission facts faulted not defaulted.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
[Union]
public abstract partial record TensorField {
    private TensorField() { }
    public sealed record ConstantCase(SymmetricMatrix Value) : TensorField;
    public sealed record CurvatureCase(SurfaceSpace Space) : TensorField;
    public sealed record LiftCase(Func<Point3d, SymmetricMatrix> Source) : TensorField;
    public sealed record WarpCase(TensorField Source, Transform Map) : TensorField;
    public sealed record ScaledCase(TensorField Source, double Scale) : TensorField;
    public sealed record BlendCase(Seq<TensorField> Fields, bool Average) : TensorField;

    public static Fin<TensorField> Constant(SymmetricMatrix value, Op? key = null) =>
        guard(value.Dimension.Value == 3, key.OrDefault().InvalidInput()).ToFin().Map(_ => (TensorField)new ConstantCase(Value: value));
    public static Fin<TensorField> Warp(TensorField source, Transform map, Op? key = null) =>
        guard(map.TryGetInverse(out _), key.OrDefault().InvalidInput()).ToFin().Map(_ => (TensorField)new WarpCase(Source: source, Map: map));

    internal Fin<SymmetricMatrix> SampleTensor(Point3d sample, Context context, Op key) =>
        key.AcceptValue(value: sample).Bind(_ => Switch(state: (Sample: sample, Context: context, Key: key),
            constantCase: static (s, c) => Fin.Succ(c.Value),
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
                from samples in c.Fields.Traverse(f => f.SampleTensor(s.Sample, s.Context, s.Key).ToValidation()).As().ToFin()
                from head in samples.Head.ToFin(s.Key.InvalidResult())
                from _ in guard(samples.ForAll(m => m.Dimension == head.Dimension), s.Key.InvalidResult())
                let scale = c.Average ? 1.0 / samples.Count : 1.0
                let upper = Arr.createRange(toSeq(Enumerable.Range(0, head.Upper.Count))
                    .Map(i => scale * samples.Fold(0.0, (sum, matrix) => sum + matrix.Upper[i])))
                from blended in SymmetricMatrix.Of(head.Dimension, upper, s.Key)
                select blended));

    public Fin<Seq<(double Eigenvalue, Direction Axis)>> PrincipalDirections(Point3d sample, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return SampleTensor(sample: sample, context: context, key: op)
            .Bind(tensor => tensor.DecomposeEigenDetailed(key: op)).Bind(solved => solved.PairsIn(expected: EigenOrder.DescendingMagnitude, key: op))
            .Bind(pairs => pairs.TraverseM(pair =>
                Direction.Of(value: new Vector3d(x: pair.Eigenvector[0], y: pair.Eigenvector[1], z: pair.Eigenvector[2]), context: context, key: op)
                    .Map(axis => (pair.Eigenvalue, Axis: axis))).As());
    }

    public Func<Point3d, Fin<SymmetricMatrix>> Sampler(Context context, Op? key = null) {
        TensorField self = this;
        Op op = key.OrDefault();
        return point => self.SampleTensor(sample: point, context: context, key: op);
    }

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
-->

(none)
