# [RASM_FIELDS]

The implicit-field algebra: THREE closed field unions — `ScalarField` (~35 cases), `VectorField` (~25 cases), `TensorField` (6 cases) — each sampled anywhere in space through ONE per-union dispatch (`SampleScalar` / `SampleVector` / `SampleTensor`), composed through operators that flatten, and constructed through admitted case payloads so the union's case set IS the admission structure: a case's payload types (value objects, `Direction`, validated `MeshSpace` sources, fitted reconstruction receipts) prove the invariants at construction, and no parallel recursive re-validation switch exists beside the case list. The retired corpus carried fifty-eight raw factories plus two mirror 35/25-arm admission switches; here raw ingress is a small set of one-expression admitting factories, multi-knob ingress rides policy records, and everything else constructs directly from already-admitted material.

Ownership seams are explicit. `Numerics/calculus.md` owns the sample-anywhere math this page composes: the `Nabla` sampler-generic central-difference stencil behind the differential cases, the `Falloff`/`KernelKind` weight-profile vocabularies, and the `FieldNoise` procedural lattices the `NoiseKind` rows declared HERE point at. `Meshing/reconstruct.md` owns the reconstruction kernels and the SDF-from-mesh policy vocabulary — the `Rbf`/`Mls`/`LevinMls`/`Apss`/`TetSignedHeat`/`Poisson` cases here carry FITTED payloads that page's solvers mint, and iso-surface extraction lives there, not here. The mesh-aware case names are FROZEN contract — `Geodesic`/`MeanCurvatureFlow`/`SpectralDistance`/`Stripe`/`SignedDistanceFromMesh` on the scalar union, `CrossField`/`Hodge`/`VectorHeat`/`GeodesicTangent`/`TangentLogMap` on the vector union — each arm delegating to the owning solver page (`Processing/geodesics.md`, `Processing/segment.md`, `Meshing/dec.md`, `Meshing/reconstruct.md`) through the `mesh.md` `MeshSpace` seam. The status-tagged sampling rail — `SampleDetailed` and `SampleSdfDetailed` — is PUBLIC: every consumer that needs to know HOW a value was produced (analytic, composed, mesh-approximate, reconstruction, tet) reads the tagged sample, and the settled `Drawing/pack.md` seam binds `SampleDetailed` by name (its frozen call site consumes the scalar facet — the geometry-campaign re-anchor is one `.Map(s => s.Value)` over `FieldSample`, never a second scalar-only rail here).

## [01]-[INDEX]

- [02]-[FIELD_VOCAB]: the field-local vocabularies — smin blend algebra with erosion columns, CSG combine rows, blend scaling, noise rows, ray and bounce policies, sample statuses and profile features.
- [03]-[SDF_PRIMITIVES]: the twelve exact analytic signed-distance primitives as TYPED cases.
- [04]-[SCALAR_FIELD]: the scalar union; operators and Lipschitz bounds; the 35-arm sample dispatch; the public tagged sampling rail.
- [05]-[VECTOR_FIELD]: the vector union; the 25-arm sample dispatch; the shared radial/rotational/closest-directed folds.
- [06]-[TENSOR_FIELD]: the symmetric-tensor union; congruence transforms; principal directions.

## [02]-[FIELD_VOCAB]

- Owner: `BlendKind` `[Union]` — `Hard`/`Polynomial`/`Exponential`/`Root`/`Cubic`/`Chamfer`/`Groove`/`Round` smooth-minimum species, each case carrying its knobs AND overriding the union's abstract `ErosionFactor` column (the Lipschitz-erosion multiplier the bound fold reads — a named policy value on the row, never a private constant table beside the union); `Smin(a,b)` the per-case smooth-minimum; `Erode(leftLip, rightLip)` the ONE-expression bound erosion `ErosionFactor·max(l,r)` — the abstract column makes a per-case switch unnecessary. `CsgKind` `[SmartEnum<int>]` — `Union`/`Intersect`/`Difference`, one `Combine(left, right, blend)` delegate column each (`min`, `−smin(−a,−b)`, `−smin(−a,b)`). `FieldBlend` `[SmartEnum<int>]` — `Sum`/`Average` with a `Scale(count)` column; one generic `CombineCore` serves scalars, vectors, and tensor components. `NoiseKind` `[SmartEnum<int>]` — `Perlin`/`Simplex`/`SmoothSimplex`/`Worley` lattice rows, each a `Sample(point, seed, frequency)` delegate column onto the `calculus.md` `FieldNoise` lattices plus a `RaisesCaution` column (`Perlin` true — visible lattice anisotropy) and a `ContinuouslyDifferentiable` column (`Worley` false — the `CurlNoise` admission gate); the row vocabulary lives HERE by the calculus.md seam — lattices are mathematics, rows are field policy. `RayPolicy` `[Union]` — `Infinite(BoundarySense)`/`Segment(BoundarySense, PositiveMagnitude)` with a `ProjectionRow`-typed `Project<TOut>` over `Ray3d`/`Plane`/`Direction`/`Vector3d`/`Line`/`VectorSpan` (finite outputs gated on the segment case). `BouncePolicy` `[Union]` — `Reflect`/`Refract(etaIncident, etaTransmitted)` delegating to `atoms.md` `Direction.Reflect`/`Direction.Refract`. `SdfStatus` `[SmartEnum<int>]` — `Analytic`/`ComposedAnalytic`/`NativeProfile`/`MeshApproximate`/`Reconstruction`/`TetSignedHeat` — the provenance rows the tagged samples carry; the mature `LossyFallback` row is DROPPED with a standing reason: the mature enum declared it and never emitted it (its only echo was a receipt bool derived back from the status), and `SampleSdfDetailed` refuses non-distance species with a typed fault instead of mislabeling a value, so the row marks nothing. `ProfileExtrusionFeature` `[SmartEnum<int>]` — `Interior`/`ProfileBoundary`/`Cap`/`Rim`, the closest-feature classification the profile-extrusion tagged sample carries.
- Boundary: `Falloff` and `KernelKind` are `Numerics/calculus.md` OWNERS composed here — the weight-profile math and kernel support/derivative profiles never re-derive on this page, and the `NoiseKind` rows POINT AT calculus.md's `FieldNoise` lattices, never re-implement them; the erosion factors ride the `BlendKind` cases so a new blend species declares its factor in its own declaration — the detached constant table is the deleted form; `RayPolicy.Project` resolves through typed `ProjectionRow` entries and a `typeof` ladder is the killed dispatch.

```csharp signature
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

    // The abstract column IS the dispatch — no per-case Erode switch exists.
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

- Owner: `SdfKind` `[Union]` — the twelve exact analytic signed-distance primitives, each case a TYPED parameter record carrying its own `Lipschitz` bound column and its `Distance(Point3d local)` member: `SphereCase(PositiveMagnitude Radius)`, `BoxCase(PositiveMagnitude X, Y, Z)`, `CapsuleCase(PositiveMagnitude HalfHeight, Radius)`, `CylinderCase(PositiveMagnitude HalfHeight, Radius)`, `ConeCase(PositiveMagnitude Height, VectorAngle HalfAngle)`, `HalfSpaceCase()`, `CappedConeCase(PositiveMagnitude HalfHeight, R1, R2)` (at least one radius positive — a factory guard), `TorusCase(PositiveMagnitude Major, Minor)`, `HexPrismCase(PositiveMagnitude HalfHeight, Circumradius)`, `OctahedronCase(PositiveMagnitude S)`, `EllipsoidCase(PositiveMagnitude X, Y, Z)` (Lipschitz 2 — the normalized-gradient estimate, not exact), `SlabCase(PositiveMagnitude HalfHeight)`. The string-keyed parameter dictionary of the retired source is DEAD: a wrong parameter name is now a compile error, a missing parameter is unconstructible, and the per-kind `Validate`/`RequiredKeys` machinery has nothing left to check.
- Entry: `internal Fin<double> SignedDistance(Point3d worldPoint, Plane pose, Op key)` — remap to pose space through `Plane.RemapToPlaneSpace`, then the case's `Distance` member; distances are the Inigo Quilez exact forms (box overflow+interior split, capped-cone two-segment closest with sign, exact octahedron octant fold, ellipsoid first-order normalization). Cross-payload guards live on the factories: `CappedCone` demands one positive radius; `Cone` proves `HalfAngle < π/2` — `tan` flips sign past it and the derived base radius goes negative.
- Growth: a new primitive is one typed case with its `Lipschitz` and `Distance` members — nothing else changes; the `ScalarField.PrimitiveCase` payload, the Lipschitz fold, and the tagged sampler pick it up through the union.
- Boundary: `Distance` bodies are pure local-frame math — pose handling happens ONCE at `SignedDistance`, never inside a case; the `Lipschitz` column is load-bearing (the ray-march step bound and the CSG erosion fold read it) and a case without an honest bound is inadmissible.

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
        // Exact hex cross-section: k-fold to the fundamental sector, then vertex-corrected edge distance (IQ);
        // a bare max-of-half-planes underestimates outside the corners and is the rejected bound form.
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
    // CappedProfile / CappedCone / ExactOctahedron: the shared exact-form kernels (IQ two-segment cone,
    // octant-folded octahedron) — private statics on the union, one body each.
}
```

## [04]-[SCALAR_FIELD]

- Owner: `ScalarField` `[Union]` — the ~35-case scalar algebra in five case families: ANALYTIC SOURCES `Constant(double)` · `Density(Point3d, PositiveMagnitude Spread, double Strength)` · `Potential(Seq<(Point3d, double)> Charges, Falloff)` · `Worley(Seq<Point3d> Seeds, Dimension Order)` · `Morse(Point3d, PositiveMagnitude Depth, PositiveMagnitude Width)` · `Mollifier(Point3d, PositiveMagnitude Radius)` · `Noise(NoiseKind, NoisePolicy)` · `Distance(SupportSpace, BoundarySense)` · `Primitive(SdfKind, Plane Pose)` · `ProfileExtrusion(Curve, Plane, PositiveMagnitude HalfHeight)`; COMBINATORS `Blend(Seq<ScalarField>, FieldBlend)` · `Csg(Left, Right, CsgKind, BlendKind)` · `Scaled` · `Power` · `Clamp` · `Displace`; DOMAIN WARPS `Periodic(Vector3d Period)` · `Twist(double AnglePerUnit, Direction Axis)` · `Bend(double Curvature, Direction Axis)` · `Elongate(Vector3d Extent)` · `Onion(PositiveMagnitude Thickness)` · `SdfRound(PositiveMagnitude Radius)`; DIFFERENTIAL `Magnitude(VectorField)` · `Divergence(VectorField, PositiveMagnitude Epsilon)` · `Laplacian(ScalarField, PositiveMagnitude Epsilon)` · `StrainMagnitude(VectorField, PositiveMagnitude Epsilon)`; MESH-AWARE (names frozen) `Geodesic(MeshSpace, Seq<int> Sources)` · `MeanCurvatureFlow(MeshSpace, PositiveMagnitude TimeStep, Dimension Iterations)` · `SpectralDistance(MeshSpace, SpectralFilter, Seq<int> Sources, Dimension Pairs)` · `Stripe(MeshSpace, VectorField CrossField, PositiveMagnitude Frequency)` · `SignedDistanceFromMesh(MeshSpace, SdfMeshPolicy)`; RECONSTRUCTION (fitted payloads minted by `reconstruct.md`) `Rbf` · `Mls` · `LevinMls` · `Apss` · `TetSignedHeat` · `Poisson`.
- Entry: case constructors take ADMITTED payloads — the admission structure IS the case set; raw ingress is the one-expression factory per family (`Density(center, spread: double, strength, key)` chains `AcceptValidated<PositiveMagnitude>`; `Noise(kind, policy)` takes the `NoisePolicy` record — `Seed`/`Octaves: Dimension`/`Persistence`/`Lacunarity`/`Frequency` admitted once — replacing the six-knob factory; mesh-aware factories prove sources against the `MeshSpace` vertex range once). The recursive 35-arm re-validation switch of the retired source DOES NOT EXIST: a `ScalarField` in hand is valid by construction.
- Auto: `SampleScalar(sample, context, key)` is the ONE total 35-arm generated `Switch` — analytic sources evaluate closed forms (potential folds charge×`Falloff.Weight`; noise folds octaves under the persistence-normalized fBm sum; Worley reads the order-th sorted seed distance); `Distance` routes `support.md` (`Closest` then signed or unsigned by admission); combinators recurse (`Csg` through `CsgKind.Combine`; `Blend` through `FieldBlend.CombineScalar`; shared `SampleMapped` collapses `Scaled`/`Power`/`Clamp`/`Onion`/`SdfRound` to one recurse-then-map body); warps pre-transform the sample (toroidal wrap, axis-angle twist, bend rotation, per-axis elongate clamp) and recurse; differential arms delegate to the `calculus.md` stencil owner `Nabla`, plugging `SampleScalar`/`SampleVector` closures in as the sampler (`Nabla.DivergenceAt`/`LaplacianAt`/`StrainMagnitudeAt` — the stencil never learns the union); the profile-extrusion arm evaluates the native-curve profile (plane remap, `Curve.ClosestPoint` + `Contains`, cap/profile max-fold) and classifies the answering `ProfileExtrusionFeature`; mesh-aware arms delegate through the `MeshSpace` seam to their owning solver pages; reconstruction arms evaluate the fitted payload through `reconstruct.md` evaluators. Operators flatten: `+` merges `Sum`-mode `Blend` chains flat (never a tree of binary blends), `-`/unary-`-`/`*` ride `Scaled`. `LipschitzBound()` is the analytic-species fold — `Constant` is 0, primitives read the case column, `ProfileExtrusion` and `Distance` are 1 (closest-point distance to any support is 1-Lipschitz; the sense sign preserves the gradient magnitude), `Worley` is 1 (the order-th sorted seed distance is an order statistic of 1-Lipschitz distances), `Morse` and `Density` carry their exact radial slope maxima (`Depth/(2·Width)` at `e^{−r/W} = ½`; `|Strength|·e^{−1/2}/Spread` at `r = σ`) so analytic wells and bumps ray-march, `Csg` erodes through its `BlendKind`, `Scaled` multiplies by `|scale|`, `Displace` adds its operands' bounds, `Blend` folds `Sum` to the bound sum and `Average` to the mean, value-contractive species (`Onion`/`SdfRound`/`Elongate`/`Clamp`) pass through, everything else is `None` — `Twist`/`Bend` stretch tangentially with radius, and `Periodic` is `None` by decision: the wrap seam breaks the modulus of continuity for any asymmetric source, and an over-claimed bound overshoots ray-march steps into silently missed surfaces.
- Receipt: `SampleDetailed(sample, context, key) → Fin<FieldSample>` — the PUBLIC tagged rail: value + `SdfStatus` provenance + optional nested evidence (`SdfMeshReceipt` for mesh-backed — signed-heat and volume-grid receipts already nest inside it, `ReconstructionSampleReceipt` for fitted species, the case's `SignedHeatReceipt` for the tet FEM species); `SampleSdfDetailed(sample, context, key) → Fin<SdfSample>` — the SDF-restricted form that REFUSES species with no distance semantics (a `LipschitzBound().IsNone` composite that is not mesh/tet/reconstruction/profile-backed faults with `Unsupported` rather than mislabeling a value as a distance — the refusal is the standing replacement for the mature `LossyFallback` status). `SdfSample` carries the profile-extrusion evidence columns — `ProfileFeature` (which feature answered: interior, profile boundary, cap, rim) and `ProfileContainment` (the `PointContainment` verdict that signed the in-plane distance) — as `Option` columns present only on the `NativeProfile` species.
- Packages: RhinoCommon (`Point3d`/`Vector3d`/`Transform.Rotation`/`Plane`/`Curve`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new scalar species is ONE case + one `Switch` arm (+ one factory only if raw material enters); a new blend/CSG mode is a vocabulary row; a new provenance species is one `SdfStatus` row carried by the tagged rail.
- Boundary: iso-surface extraction is `Meshing/reconstruct.md`'s — this page samples, never meshes; the mesh-aware arms are one-line delegations and any solver math appearing here is a mis-homed body; `SampleScalar` assumes admitted fields and a defensive re-validation inside an arm is the deleted double-admission; the tagged rail is THE public sampling seam (`pack.md` binds `SampleDetailed`) and a second `Evaluate`/`Probe` sibling family is the rejected surface.

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
    Option<ReconstructionSampleReceipt> Reconstruction, Option<SignedHeatReceipt> Tet);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfSample(
    double Value, SdfStatus Status, Option<double> LipschitzBound, Option<SdfMeshReceipt> Mesh, Option<SignedHeatReceipt> Tet,
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
    public sealed record TetSignedHeatCase(TetMeshDomain Domain, TetSignedHeatPolicy Policy, Arr<double> Values, SignedHeatReceipt Receipt) : ScalarField;
    public sealed record PoissonCase(PoissonGrid Grid, double Gamma, PoissonReceipt Receipt) : ScalarField;

    // Raw-ingress factories: one expression each; cases with admitted payloads construct directly.
    public static ScalarField Constant(double value) => new ConstantCase(Value: value);
    public static Fin<ScalarField> Density(Point3d center, double spread, double strength, Op? key = null) =>
        from s in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: spread)
        from _ in guard(double.IsFinite(strength) && center.IsValid, key.OrDefault().InvalidInput())
        select (ScalarField)new DensityCase(Center: center, Spread: s, Strength: strength);
    public static Fin<ScalarField> Geodesic(MeshSpace space, Seq<int> sources, Op? key = null) =>
        guard(!sources.IsEmpty && sources.ForAll(v => v >= 0 && v < space.Native.Vertices.Count), key.OrDefault().InvalidInput())
            .ToFin().Map(_ => (ScalarField)new GeodesicCase(Space: space, Sources: sources));
    // Worley/Morse/Mollifier/Noise/Primitive/ProfileExtrusion/warp/differential/mesh factories: same shape,
    // one admission expression per raw ingress; reconstruction cases are minted ONLY by reconstruct.md fitting.

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
        // The order-th sorted seed distance is an order statistic of 1-Lipschitz distances — still 1-Lipschitz.
        WorleyCase => Some(1.0),
        // Exact radial slope maxima: Morse |V′| = 2D(1−u)u/W peaks at u = e^{−r/W} = ½ → D/(2W);
        // Gaussian density |∇| = |S|(r/σ²)e^{−r²/2σ²} peaks at r = σ → |S|e^{−1/2}/σ.
        MorseCase m => Some(m.Depth.Value / (2.0 * m.Width.Value)),
        DensityCase d => Some(Math.Abs(d.Strength) * Math.Exp(-0.5) / d.Spread.Value),
        CsgCase c => from l in c.Left.LipschitzBound() from r in c.Right.LipschitzBound() select c.Smoothing.Erode(leftLip: l, rightLip: r),
        ScaledCase s => s.Source.LipschitzBound().Map(l => Math.Abs(s.Scale) * l),
        DisplaceCase d => from l in d.Source.LipschitzBound() from r in d.Displacement.LipschitzBound() select l + r,
        BlendCase b => b.Fields.TraverseM(static f => f.LipschitzBound()).As().Map(bounds =>
            b.Mode.Equals(FieldBlend.Average) ? bounds.Sum() / bounds.Count : bounds.Sum()),
        OnionCase o => o.Source.LipschitzBound(),
        SdfRoundCase r => r.Source.LipschitzBound(),
        ElongateCase e => e.Source.LipschitzBound(),
        ClampCase c => c.Source.LipschitzBound(),
        // Twist/Bend stretch tangentially with radius; Periodic's wrap seam breaks the modulus of
        // continuity for asymmetric sources: no honest global bound for any of the three.
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
        form: refuses (Unsupported) any species that cannot claim distance semantics — the standing replacement
        for the mature LossyFallback status; NativeProfile samples carry the ProfileFeature/ProfileContainment
        columns. */ return default!; }
}
```

## [05]-[VECTOR_FIELD]

- Owner: `VectorField` `[Union]` — ~25 cases in four families: ANALYTIC `Constant(Vector3d)` · `Vortex(Point3d Anchor, Direction Axis, Falloff)` · `Ring(Point3d Center, Direction Axis, PositiveMagnitude Radius, Falloff)` · `Helical(Point3d Anchor, Direction Axis, double Axial, double Swirl, Falloff)` · `Coulomb(Seq<(Point3d, double)> Charges, Falloff)` · `Dipole(Point3d Origin, Direction Moment, PositiveMagnitude Strength)` · `Harmonic(Seq<(Direction, double Frequency, double Phase, double Amplitude)>)` · `BiotSavart(Point3d Start, Point3d End, double Current)` · `Saddle(Point3d Anchor, Plane Basis, double Strength)`; PROXIMITY-DRIVEN `Influence(SupportSpace, Falloff, BoundarySense, Option<PositiveMagnitude> ShellRadius)` · `HitField(SupportSpace, SupportProjection, BoundarySense)` · `ClusterField(VectorCloud.ClusterCase, Falloff, PositiveMagnitude Radius, BoundarySense)`; COMBINATORS/WARPS `Blend` · `Scaled` · `Projected(Plane)` · `Warp(Transform)` · `ClampMagnitude(Min, Max)` · `CrossProduct(Left, Right)`; DIFFERENTIAL `Gradient(ScalarField, Epsilon)` · `Curl(VectorField, Epsilon)` · `CurlNoise(ScalarField Potential, Epsilon)` (refuses a Worley potential — cell noise has no continuous curl); MESH-AWARE (names frozen) `CrossField(MeshSpace, Dimension Symmetry, Option<Seq<(int, Direction)>> Constraints, Option<Seq<(int, double)>> Cones)` · `Hodge(VectorField, MeshSpace, BoundarySense)` · `VectorHeat(MeshSpace, Seq<(int, Vector3d)>, PositiveMagnitude Time)` · `GeodesicTangent(MeshSpace, Seq<int>)` · `TangentLogMap(MeshSpace, int Source, PositiveMagnitude Time, TangentLogMapAlgorithm, GeodesicTracePolicy, WindowPropagationPolicy)`.
- Entry: same construction law as the scalar union — admitted case payloads, one-expression raw-ingress factories (`Ring` and `ClusterField` derive the default falloff `Gaussian(radius/3)` when none is given; `HitField` gates the projection on `SupportProjection.CanProjectVector`; `CrossField` proves symmetry ∈ {1,2,4,6} and vertex ranges once), operators identical to the scalar operators (Sum-flattening `+`, `Scaled` scaling).
- Auto: `SampleVector(sample, context, key)` is the ONE total 25-arm `Switch` built on three shared folds — `ClosestDirected(source, sample, sense, hitToScaled)` (one closest-hit query feeding `Influence` shell residuals and `HitField` projections, sense-signed and admitted through `Direction.Of`), `RotationalField(anchor, axis, falloff, axial, swirl)` (the ONE swirl body: `Vortex` and `Ring` are `(0,1)` — `Ring.Radius` parameterizes only its default falloff, never a kernel offset — and `Helical` is `(axial, swirl)`: three cases, one kernel), and `RadialContribution(sum, source, scale, falloff)` (the accumulating charge fold `Coulomb` and `ClusterField` share, the cluster arm restricted to `WithinRadius` indices from `cloud.md`). Dipole/harmonic/Biot-Savart/saddle evaluate their closed forms (finite-wire Biot-Savart with perpendicular-component regularization; saddle `u·X − v·Y` in the basis); differential arms delegate to the `calculus.md` `Nabla` stencil with `SampleScalar`/`SampleVector` closures as the sampler; mesh-aware arms delegate through the `MeshSpace` seam.
- Receipt: vector samples are values; provenance-tagged vector sampling rides `SampleDetailed`'s vector sibling arm on demand (growth case, same `SdfStatus` vocabulary).
- Growth: a new field species is one case + one arm (+ a shared-fold parameter when it is a swirl/radial/closest variant — the three folds absorb before a new body is admitted).
- Boundary: the three shared folds are the collapse law — a new analytic case re-implementing swirl, radial accumulation, or closest-directed shaping is the rejected duplication; the on-source law is deliberate and asymmetric — `ClosestDirected` FAULTS on a sample coincident with its support (`Direction.Of` refuses the zero displacement: a hit-directed vector is undefined at its own source, and a silent zero would corrupt a streamline), while `RadialContribution` SKIPS a coincident charge (the sum's remaining terms stay well-defined); `CurlNoise` admission refuses the non-differentiable potential at CONSTRUCTION via a recursive differentiability fold over the payload tree — a `Worley` or `Noise(Worley)` hiding inside a `Blend`/`Csg` composite still refuses, and the sampler never guards; mesh-aware arms are one-line delegations.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
[Union]
public abstract partial record VectorField {
    private VectorField() { }
    // Case declarations as charted: Constant/Blend/Scaled/Influence/HitField/Vortex/Ring/Helical/Coulomb/
    // ClusterField/Dipole/Harmonic/Projected/Warp/ClampMagnitude/Gradient/Curl/CurlNoise/CrossProduct/
    // BiotSavart/Saddle/CrossField/Hodge/VectorHeat/GeodesicTangent/TangentLogMap — admitted payloads throughout.

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
            clusterFieldCase: static (s, c) => c.Source.WithinRadius(sample: s.Sample, radius: c.Radius.Value, key: s.Key)
                .Bind(ids => ids.Fold(Fin.Succ(Vector3d.Zero),
                    (acc, i) => acc.Bind(sum => RadialContribution(sum: sum, source: c.Source.Vertices[i], scale: c.Sense.Sign, state: s, falloff: c.Falloff)))),
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

    // The three shared folds every analytic case composes:
    private static Fin<Vector3d> RotationalField(Point3d anchor, Direction axis, Falloff falloff, double axial, double swirl, (Point3d Sample, Context Context, Op Key) state) {
        Vector3d r = state.Sample - anchor;
        Vector3d rPerp = r - ((r * axis.Value) * axis.Value);
        return falloff.Weight(offset: rPerp, sample: state.Sample, tolerance: state.Context.Absolute.Value, key: state.Key)
            .Map(w => w * ((axial * axis.Value) + (swirl * Vector3d.CrossProduct(a: axis.Value, b: rPerp))));
    }
    // The sample rides every Weight call so the metric (anisotropic) falloff case works on all three folds.
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

- Owner: `TensorField` `[Union]` — `Constant(SymmetricMatrix)` · `Curvature(SurfaceSpace)` · `Lift(Func<Point3d, SymmetricMatrix>)` (the one opaque-closure ingress, guarded under `key.Catch` at sample time) · `Warp(TensorField, Transform)` · `Scaled(TensorField, double)` · `Blend(Seq<TensorField>, FieldBlend)`.
- Entry: `SampleTensor(sample, context, key) → Fin<SymmetricMatrix>` — the six-arm `Switch`; `PrincipalDirections(sample, context, key)` decomposes the 3×3 sample through `matrix.md` eigen into `(eigenvalue, Direction)` rows; `Sampler(context, key?) → Func<Point3d, Fin<SymmetricMatrix>>` — the closure bridge `calculus.md` `Falloff.Metric` takes, so the anisotropic decay samples THIS union without calculus ever naming a field type.
- Auto: the `Curvature` arm is the SINGLE second-fundamental-form consumer — it projects the sample onto the surface (`Surface.ClosestPoint`) and reads `Parametric/projections.md`'s `SurfaceProjection.ShapeOperator` at the recovered `(u,v)`; the shape-operator TENSOR is owned there and this case never re-derives principal curvatures. `Warp` transforms by congruence `R·M·Rᵀ` (rotation block lifted from the `Transform`, product through `matrix.md` `Matrix.Multiply`, repacked upper-triangular); `Blend` combines component-wise through `FieldBlend.CombineScalar` over dimension-agreeing tensors; anisotropic falloff metrics (`calculus.md` `Falloff.Metric`) take `tensorField.Sampler(context)` as their metric sampler — the composition runs downward into calculus, never a second metric type.
- Growth: a new tensor species is one case + one arm; a curvature variant (e.g. mesh shape operator) is a new case delegating to its owning page, never local differential geometry.
- Boundary: `Lift` is the only closure-carrying case and its sampler runs inside `key.Catch` with an `IsValid` gate — an unguarded user closure is the named foreign-code seam; the congruence transform requires an invertible spatial map (`TryGetInverse` gates the pre-image sample) and dimension-3 tensors — both admission facts, faulted not defaulted.

## [07]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]          | [OWNER]                       | [KIND]                                                              | [RAIL]                                        | [CASES] |
| :-----: | :---------------------- | :---------------------------- | :------------------------------------------------------------------ | :--------------------------------------------- | :-----: |
|  [01]   | Smooth-minimum species  | `BlendKind`                   | `[Union]` + `ErosionFactor` case column                              | `Smin`/`Erode` (pure)                          |    8    |
|  [02]   | CSG combination         | `CsgKind`                     | `[SmartEnum<int>]` + `Combine` delegate column                       | dispatch row                                   |    3    |
|  [03]   | SDF primitive           | `SdfKind`                     | `[Union]` typed parameter cases + `Lipschitz`/`Distance` members     | `SignedDistance → Fin<double>`                 |   12    |
|  [04]   | Scalar field algebra    | `ScalarField`                 | `[Union]` ~35 cases, five families, flattening operators             | `SampleScalar → Fin<double>`                   |   ~35   |
|  [05]   | Tagged sampling rail    | `SampleDetailed`/`SampleSdfDetailed` | status-tagged public seam (`SdfStatus` + nested receipts)      | `→ Fin<FieldSample>` / `Fin<SdfSample>`        |    6    |
|  [06]   | Vector field algebra    | `VectorField`                 | `[Union]` ~25 cases over three shared folds                          | `SampleVector → Fin<Vector3d>`                 |   ~25   |
|  [07]   | Tensor field algebra    | `TensorField`                 | `[Union]` 6 cases; congruence transform; one shape-operator consumer | `SampleTensor → Fin<SymmetricMatrix>`          |    6    |
|  [08]   | Ray/bounce policy       | `RayPolicy`/`BouncePolicy`    | `[Union]` pairs over `atoms.md` optics                               | `Project<TOut>` / `Apply → Fin<Direction>`     |   2+2   |
|  [09]   | Noise vocabulary        | `NoiseKind`                   | `[SmartEnum<int>]` lattice rows + caution/differentiability columns  | `Sample` delegate onto `FieldNoise`            |    4    |
