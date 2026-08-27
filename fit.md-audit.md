# 1. Use keyless algorithm vocabularies and preserve consensus precision

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:309`

```csharp
[SmartEnum<int>]
public sealed partial class ConsensusScore {
    public static readonly ConsensusScore Msac = new(key: 0, inlierRatio: 1.0, saturating: true, Truncated);
    public static readonly ConsensusScore MaximumLikelihood = new(key: 1, inlierRatio: MixturePrior, saturating: false, MixtureNll);

    public double InlierRatio { get; }
    public bool Saturating { get; }

    [UseDelegateFromConstructor]
    public partial double Cost(double squaredDistance, double squaredThreshold);

    const double MixturePrior = 0.5;
    static readonly ddouble BandNormalization = ddouble.Erf(ddouble.Sqrt(2.0));
    static double Truncated(double d2, double t2) => Math.Min(d2, t2);

    static double MixtureNll(double d2, double t2) {
        double sigma2 = t2 / 4.0;
        ddouble inlier = ddouble.Exp(-(ddouble)d2 / (2.0 * sigma2)) / (ddouble.Sqrt(Math.Tau * sigma2) * BandNormalization);
        ddouble outlier = 0.5 / ddouble.Sqrt(t2);
        return (double)(-ddouble.Log((MixturePrior * inlier) + ((1.0 - MixturePrior) * outlier)));
    }
}

[SmartEnum<int>]
public sealed partial class DrawOrder {
    public static readonly DrawOrder Uniform      = new(key: 0);
    public static readonly DrawOrder QualityFront = new(key: 1);
    public static readonly DrawOrder Neighborhood = new(key: 2);
}
```

**To**

```csharp
[SmartEnum]
public sealed partial class ConsensusCost {
    public static readonly ConsensusCost Msac   = new(saturating: true, Truncated);
    public static readonly ConsensusCost Mlesac = new(saturating: false, MixtureNll);

    // InlierRatio DELETED
    public bool Saturating { get; }

    [UseDelegateFromConstructor]
    public partial ddouble Cost(double squaredDistance, double squaredThreshold);

    const double MixturePrior = 0.5;
    static readonly ddouble BandNormalization = ddouble.Erf(ddouble.Sqrt(2.0));
    static ddouble Truncated(double d2, double t2) => Math.Min(d2, t2);

    static ddouble MixtureNll(double d2, double t2) {
        double sigma2 = t2 / 4.0;
        ddouble inlier = ddouble.Exp(-(ddouble)d2 / (2.0 * sigma2)) / (ddouble.Sqrt(Math.Tau * sigma2) * BandNormalization);
        ddouble outlier = 0.5 / ddouble.Sqrt(t2);
        return -ddouble.Log((MixturePrior * inlier) + ((1.0 - MixturePrior) * outlier));
    }
}

[SmartEnum]
public sealed partial class SampleMode {
    public static readonly SampleMode Random = new();
    public static readonly SampleMode Prosac = new();
    public static readonly SampleMode Napsac = new();
}
```

**Why**

`ConsensusScore` is a minimized cost, `MaximumLikelihood` hides MLESAC, and the sampling rows coin names for PROSAC and NAPSAC. Neither vocabulary crosses a wire or store, so integer keys add generated key, conversion, parsing, and lookup surface without identity value. `InlierRatio` is unread, and narrowing every likelihood term to `double` defeats the stated 106-bit reduction before accumulation.

**Change**

Rename both axes and rows, make both smart enums keyless, delete the unused column, and keep cost terms as `ddouble` through candidate comparison and final narrowing. Propagate the new names through policy, sampling, scoring, prose, cards, and diagrams.

**Delta**

Code-fence LOC: -3. Module-level types: +0/-0/net 0. Module-level members: +0/-3/net -3, counting the deleted source column and two generated key members.

**Ripples**

- `libs/dotnet/Rasm/README.md:83`: replace “the MLESAC sampler” with “MSAC/MLESAC consensus”.

# 2. Name fit metadata precisely and delete the Jacobian-width proof

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:27`

```csharp
using System.Threading;
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:46`

```csharp
public sealed partial class FitKind : IDrawLane<FitKind> {
    public static readonly FitKind Plane    = new("plane",    lane: 0L, minimalSamples: 3, freeParameters: 3, needsNormals: false, carrier: Kind.Plane,    MinimalPlane,    UnpackPlane);
    public static readonly FitKind Sphere   = new("sphere",   lane: 1L, minimalSamples: 4, freeParameters: 4, needsNormals: false, carrier: Kind.Sphere,   MinimalSphere,   UnpackSphere);
    public static readonly FitKind Cylinder = new("cylinder", lane: 2L, minimalSamples: 6, freeParameters: 6, needsNormals: false, carrier: Kind.Cylinder, MinimalCylinder, UnpackCylinder);
    public static readonly FitKind Cone     = new("cone",     lane: 3L, minimalSamples: 7, freeParameters: 6, needsNormals: true,  carrier: Kind.Cone,     MinimalCone,     UnpackCone);
    public static readonly FitKind Torus    = new("torus",    lane: 4L, minimalSamples: 8, freeParameters: 7, needsNormals: true,  carrier: Kind.Torus,    MinimalTorus,    UnpackTorus);
    public static readonly FitKind Line     = new("line",     lane: 5L, minimalSamples: 2, freeParameters: 4, needsNormals: false, carrier: Kind.Line,     MinimalLine,     UnpackLine);
    public static readonly FitKind Circle   = new("circle",   lane: 6L, minimalSamples: 3, freeParameters: 6, needsNormals: false, carrier: Kind.Circle,   MinimalCircle,   UnpackCircle);

    public long Lane { get; }
    public int MinimalSamples { get; }
    public int FreeParameters { get; }
    public bool NeedsNormals { get; }
    public Kind Carrier { get; }

    [UseDelegateFromConstructor]
    public partial Fin<FitPrimitive> Minimal(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance, Op key);

    [UseDelegateFromConstructor]
    public partial FitPrimitive Unpack(ReadOnlySpan<double> parameters);

    static readonly Lazy<Unit> WidestParameters = new(
        static () => Items.Max(static row => row.FreeParameters) == PartialRow.Arity
            ? unit
            : throw new InvalidOperationException($"PartialRow arity {PartialRow.Arity} is under FitKind's widest FreeParameters"),
        LazyThreadSafetyMode.ExecutionAndPublication);
    internal static Unit Widest => WidestParameters.Value;
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:341`

```csharp
[InlineArray(PartialRow.Arity)]
public struct PartialRow {
    internal const int Arity = 7;
    double element0;
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:418`

```csharp
public PartialRow Gradient(Point3d query) {
    _ = FitKind.Widest;
    return Switch(
        state: query,
        plane:    static (q, pl) => PlaneGradient(q, pl),
        sphere:   static (q, s) => SphereGradient(q, s),
        cylinder: static (q, c) => CylinderGradient(q, c),
        cone:     static (q, k) => ConeGradient(q, k),
        torus:    static (q, t) => RevolveGradient(t.Center, t.Axis, t.Major, Some(t.Minor), q),
        line:     static (q, ln) => LineGradient(q, ln),
        circle:   static (q, c) => RevolveGradient(c.Curve.Center, c.Curve.Normal, c.Curve.Radius, None, q));
}
```

**To**

```csharp
// System.Threading DELETED
```

```csharp
public sealed partial class FitKind : IDrawLane<FitKind> {
    public static readonly FitKind Plane    = new("plane",    lane: 0L, minimalSamples: 3, dof: 3, requiresNormals: false, faultKind: Kind.Plane,    SolvePlane,    RebuildPlane);
    public static readonly FitKind Sphere   = new("sphere",   lane: 1L, minimalSamples: 4, dof: 4, requiresNormals: false, faultKind: Kind.Sphere,   SolveSphere,   RebuildSphere);
    public static readonly FitKind Cylinder = new("cylinder", lane: 2L, minimalSamples: 6, dof: 6, requiresNormals: false, faultKind: Kind.Cylinder, SolveCylinder, RebuildCylinder);
    public static readonly FitKind Cone     = new("cone",     lane: 3L, minimalSamples: 7, dof: 6, requiresNormals: true,  faultKind: Kind.Cone,     SolveCone,     RebuildCone);
    public static readonly FitKind Torus    = new("torus",    lane: 4L, minimalSamples: 8, dof: 7, requiresNormals: true,  faultKind: Kind.Torus,    SolveTorus,    RebuildTorus);
    public static readonly FitKind Line     = new("line",     lane: 5L, minimalSamples: 2, dof: 4, requiresNormals: false, faultKind: Kind.Line,     SolveLine,     RebuildLine);
    public static readonly FitKind Circle   = new("circle",   lane: 6L, minimalSamples: 3, dof: 6, requiresNormals: false, faultKind: Kind.Circle,   SolveCircle,   RebuildCircle);

    public long Lane { get; }
    public int MinimalSamples { get; }
    internal int Dof { get; }
    public bool RequiresNormals { get; }
    internal Kind FaultKind { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<FitPrimitive> Solve(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance, Op key);

    [UseDelegateFromConstructor]
    internal partial FitPrimitive Rebuild(ReadOnlySpan<double> parameters);

    // WidestParameters DELETED
    // Widest DELETED
```

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FitPrimitive {
    [InlineArray(JacobianRow.Arity)]
    internal struct JacobianRow {
        internal const int Arity = 7;
        double element0;
    }

    internal JacobianRow Jacobian(Point3d query) =>
        Switch(
            state: query,
            plane:    static (q, pl) => PlaneJacobian(q, pl),
            sphere:   static (q, s) => SphereJacobian(q, s),
            cylinder: static (q, c) => CylinderJacobian(q, c),
            cone:     static (q, k) => ConeJacobian(q, k),
            torus:    static (q, t) => RevolveJacobian(t.Center, t.Axis, t.Major, Some(t.Minor), q),
            line:     static (q, ln) => LineJacobian(q, ln),
            circle:   static (q, c) => RevolveJacobian(c.Curve.Center, c.Curve.Normal, c.Curve.Radius, None, q));
}
```

**Why**

`FreeParameters`, `Carrier`, `Minimal`, `Unpack`, and `Gradient` obscure degrees of freedom, fault classification, minimal solving, chart reconstruction, and a residual Jacobian. `PartialRow` is not an independent public concept. The lazy roster check duplicates a fixed inline-array arity and turns source consistency into delayed runtime initialization plus two members and one import.

**Change**

Rename the columns, delegates, and implementations; reduce solver-only members to `internal`; nest `JacobianRow`; and delete the lazy proof, forwarding property, forced read, and threading import. Update the model, admission, prose, cards, and diagrams.

**Delta**

Code-fence LOC: -8. Module-level types: +0/-1/net -1. Module-level members: +0/-2/net -2.

**Ripples**

- `libs/dotnet/Rasm.Fabrication/.planning/Verify/probing.md:1021,1136`: replace `NeedsNormals` with `RequiresNormals`.

# 3. Carry admitted policy counts and validate relations once

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:348`

```csharp
public sealed record FitPolicy(
    ConsensusScore Score,
    DrawOrder Order,
    UnitInterval InlierFloor,
    UnitInterval Confidence,
    PositiveMagnitude InlierScale,
    UnitInterval NormalBand,
    int MaxTrials,
    long Seed,
    int Neighborhood,
    SolvePolicy Ladder) {
    public static Fin<FitPolicy> Of(Context context, Op key, Option<SolvePolicy> ladder = default) =>
        from floor in key.AcceptValidated<UnitInterval>(candidate: 0.5)
        from confidence in key.AcceptValidated<UnitInterval>(candidate: 0.999)
        from scale in key.AcceptValidated<PositiveMagnitude>(candidate: 2.5)
        from band in key.AcceptValidated<UnitInterval>(candidate: 0.9)
        from refine in ladder.Match(Some: Fin.Succ, None: () => SolvePolicy.Of(context: context, key: key))
        from admitted in new FitPolicy(
            Score: ConsensusScore.Msac, Order: DrawOrder.Uniform,
            InlierFloor: floor, Confidence: confidence, InlierScale: scale, NormalBand: band,
            MaxTrials: TrialCeiling, Seed: 0x5EED, Neighborhood: 32, Ladder: refine).Admit(key: key)
        select admitted;

    const int TrialCeiling = 1 << 16;

    internal Fin<FitPolicy> Admit(Op key) {
        FitPolicy self = this;
        return Rasm.Domain.Admit.Claims(key,
            (self.Confidence.Value < 1.0, nameof(Confidence)),
            (self.InlierFloor.Value > 0.0, nameof(InlierFloor)),
            (self.MaxTrials >= 1, nameof(MaxTrials)),
            (self.Neighborhood >= 1, nameof(Neighborhood))).Map(_ => self);
    }

    public double Threshold(double absolute) => InlierScale.Value * absolute;
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:739`

```csharp
+ (op.Normals.IsNone
    ? op.Kinds.Filter(static kind => kind.NeedsNormals).Map(kind =>
        (Validation<Error, Unit>)new GeometryFault.DegenerateInput(kind.Carrier, None, "no-normal-field"))
    : Seq<Validation<Error, Unit>>());
```

**To**

```csharp
public sealed record FitPolicy(
    ConsensusCost Cost,
    SampleMode Sampling,
    UnitInterval InlierFloor,
    UnitInterval Confidence,
    PositiveMagnitude InlierScale,
    UnitInterval NormalFloor,
    Dimension MaxTrials,
    long Seed,
    Dimension Neighbors,
    SolvePolicy Refine) {
    public static Fin<FitPolicy> Of(Context context, Op key, Option<SolvePolicy> refine = default) =>
        refine.Match(Some: Fin.Succ, None: () => SolvePolicy.Of(context: context, key: key))
            .Map(solve => new FitPolicy(
                ConsensusCost.Msac, SampleMode.Random,
                UnitInterval.Create(0.5), UnitInterval.Create(0.999),
                PositiveMagnitude.Create(2.5), UnitInterval.Create(0.9),
                Dimension.Create(TrialCeiling), 0x5EED, Dimension.Create(32), solve));

    const int TrialCeiling = 1 << 16;

    // Admit DELETED
    // Threshold DELETED
}
```

```csharp
+ (op.Normals.IsNone
    ? op.Kinds.Filter(static kind => kind.RequiresNormals).Map(kind =>
        (Validation<Error, Unit>)new GeometryFault.DegenerateInput(kind.FaultKind, None, "no-normal-field"))
    : Seq<Validation<Error, Unit>>())
+ Seq(
    AdmissionSlots.Gate(op.Policy.Confidence.Value is > 0.0 and < 1.0,
        new GeometryFault.DegenerateInput(Kind.PointCloud, None, "confidence-open-unit")),
    AdmissionSlots.Gate(op.Policy.InlierFloor.Value > 0.0,
        new GeometryFault.DegenerateInput(Kind.PointCloud, None, "inlier-floor-positive")));
```

**Why**

The factory sends trusted constants through four `Fin` admissions and rechecks them, while the public constructor bypasses `Admit`. Trial and neighbor counts already have the kernel's positive `Dimension` owner. Confidence requires an open unit interval, which `UnitInterval` alone cannot prove, and `Threshold` is a two-call multiplication wrapper.

**Change**

Use generated `Create` for trusted constants, carry positive counts as `Dimension`, delete `Admit` and `Threshold`, and place the two remaining relational claims in `Fit.Apply`'s accumulating admission. Propagate renamed fields and inline threshold multiplication.

**Delta**

Code-fence LOC: -9. Module-level types: +0/-0/net 0. Module-level members: +0/-2/net -2.

# 4. Preserve minimal-solver failures and delete arithmetic shells

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:83`

```csharp
new Arr<double>([0.5 * (Sq(b) - Sq(a)), 0.5 * (Sq(c) - Sq(a)), 0.5 * (Sq(d) - Sq(a))])
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:123`

```csharp
static Fin<FitPrimitive> MinimalCone(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance, Op key) =>
    normals.Match(
        Some: field => {
            Point3d apex = ApexFromNormals(cloud, draw, field, key);
            Vector3d u0 = Unit(cloud[draw[0]] - apex), u1 = Unit(cloud[draw[1]] - apex), u2 = Unit(cloud[draw[2]] - apex);
            Vector3d axis = Vector3d.CrossProduct(u1 - u0, u2 - u0);
            if (axis.IsTiny())
                return Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cone, draw[0], "degenerate-axis"));
            double half = HalfAngle(cloud, draw, apex, axis);
            return Fin.Succ((FitPrimitive)new FitPrimitive.Cone(apex, Unit(axis), half));
        },
        None: () => Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cone, None, "no-normal-field")));
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:176`

```csharp
static Vector3d AxisFromNormals(int[] draw, Vector3d[] normals) {
    Vector3d cross = Vector3d.CrossProduct(normals[draw[0]], normals[draw[1]]);
    return cross.IsTiny() ? normals[draw[0]] : cross;
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:199`

```csharp
static Point3d ApexFromNormals(Point3d[] cloud, int[] draw, Vector3d[] normals, Op key) {
    int n = draw.Length;
    double[] lhs = new double[n * 3];
    double[] rhs = new double[n];
    for (int i = 0; i < n; i++) {
        Vector3d nrm = Unit(normals[draw[i]]);
        (lhs[i * 3], lhs[(i * 3) + 1], lhs[(i * 3) + 2]) = (nrm.X, nrm.Y, nrm.Z);
        rhs[i] = nrm.X * cloud[draw[i]].X + nrm.Y * cloud[draw[i]].Y + nrm.Z * cloud[draw[i]].Z;
    }
    return Matrix.Of(Dimension.Create(n), Dimension.Create(3), new Arr<double>(lhs), key)
        .Bind(design => design.LeastSquaresDetailed(new Arr<double>(rhs), key))
        .Match(
            Succ: solved => new Point3d(solved.Solution[0], solved.Solution[1], solved.Solution[2]),
            Fail: _ => Centroid(cloud, draw));
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:306`

```csharp
static double Sq(Point3d p) => p.X * p.X + p.Y * p.Y + p.Z * p.Z;
```

**To**

```csharp
new Arr<double>([
    0.5 * (b.DistanceToSquared(Point3d.Origin) - a.DistanceToSquared(Point3d.Origin)),
    0.5 * (c.DistanceToSquared(Point3d.Origin) - a.DistanceToSquared(Point3d.Origin)),
    0.5 * (d.DistanceToSquared(Point3d.Origin) - a.DistanceToSquared(Point3d.Origin))])
```

```csharp
static Fin<FitPrimitive> SolveCone(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance, Op key) =>
    normals.Match(
        Some: field => ApexFromNormals(cloud, draw, field, key).Bind(apex => {
            Vector3d u0 = Unit(cloud[draw[0]] - apex), u1 = Unit(cloud[draw[1]] - apex), u2 = Unit(cloud[draw[2]] - apex);
            Vector3d axis = Vector3d.CrossProduct(u1 - u0, u2 - u0);
            return axis.IsTiny(EpsilonPolicy.ZeroTolerance)
                ? Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cone, draw[0], "degenerate-axis"))
                : Fin.Succ((FitPrimitive)new FitPrimitive.Cone(apex, Unit(axis), HalfAngle(cloud, draw, apex, axis)));
        }),
        None: () => Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cone, None, "no-normal-field")));
```

```csharp
Vector3d cross = Vector3d.CrossProduct(field[draw[0]], field[draw[1]]);
Vector3d axis = cross.IsTiny(EpsilonPolicy.ZeroTolerance) ? field[draw[0]] : cross;

// AxisFromNormals DELETED
```

```csharp
static Fin<Point3d> ApexFromNormals(Point3d[] cloud, int[] draw, Vector3d[] normals, Op key) {
    int n = draw.Length;
    double[] lhs = new double[n * 3];
    double[] rhs = new double[n];
    for (int i = 0; i < n; i++) {
        Vector3d nrm = Unit(normals[draw[i]]);
        (lhs[i * 3], lhs[(i * 3) + 1], lhs[(i * 3) + 2]) = (nrm.X, nrm.Y, nrm.Z);
        rhs[i] = nrm.X * cloud[draw[i]].X + nrm.Y * cloud[draw[i]].Y + nrm.Z * cloud[draw[i]].Z;
    }
    return Matrix.Of(Dimension.Create(n), Dimension.Create(3), new Arr<double>(lhs), key)
        .Bind(design => design.LeastSquaresDetailed(new Arr<double>(rhs), key))
        .Map(solved => new Point3d(solved.Solution[0], solved.Solution[1], solved.Solution[2]));
}

// Sq DELETED
```

**Why**

The apex solve converts a typed least-squares failure into a fabricated centroid. `AxisFromNormals` is a one-call shell, and `Sq` hand-rolls RhinoCommon's squared-distance projection.

**Change**

Return the apex solve as `Fin<Point3d>` and bind it, inline the torus axis election, replace `Sq` with `Point3d.DistanceToSquared(Point3d.Origin)`, and delete both wrappers.

**Delta**

Code-fence LOC: -3. Module-level types: +0/-0/net 0. Module-level members: +0/-2/net -2.

# 5. Inline one-use primitive seed reducers

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:215`

```csharp
static double HalfAngle(Point3d[] cloud, int[] draw, Point3d apex, Vector3d axis) {
    Vector3d unit = Unit(axis);
    double sum = 0.0;
    for (int i = 0; i < draw.Length; i++) {
        Vector3d rel = cloud[draw[i]] - apex;
        double along = Math.Abs(rel * unit);
        double radial = (rel - (rel * unit) * unit).Length;
        sum += Math.Atan2(radial, along);
    }
    return sum / draw.Length;
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:227`

```csharp
static Fin<double> RadiusAbout(Point3d[] cloud, int[] draw, Point3d origin, Vector3d axis, Op key) {
    Vector3d unit = Unit(axis);
    double sum = 0.0;
    for (int i = 0; i < draw.Length; i++) {
        Vector3d rel = cloud[draw[i]] - origin;
        sum += (rel - (rel * unit) * unit).Length;
    }
    double radius = sum / draw.Length;
    return radius <= 0.0
        ? Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Cylinder, draw[0], "zero-radius"))
        : Fin.Succ(radius);
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:240`

```csharp
static (double Major, double Minor) TorusRadii(Point3d[] cloud, int[] draw, Point3d center, Vector3d axis) {
    Vector3d unit = Unit(axis);
    double majorSum = 0.0;
    double[] radial = new double[draw.Length];
    for (int i = 0; i < draw.Length; i++) {
        Vector3d rel = cloud[draw[i]] - center;
        radial[i] = (rel - (rel * unit) * unit).Length;
        majorSum += radial[i];
    }
    double major = majorSum / draw.Length;
    double minorSum = 0.0;
    for (int i = 0; i < draw.Length; i++) {
        Vector3d rel = cloud[draw[i]] - center;
        double along = rel * unit;
        double inPlane = radial[i] - major;
        minorSum += Math.Sqrt(inPlane * inPlane + along * along);
    }
    return (major, minorSum / draw.Length);
}

static Point3d Centroid(Point3d[] cloud, int[] draw) {
    Vector3d sum = Vector3d.Zero;
    foreach (int i in draw) sum += cloud[i] - Point3d.Origin;
    return Point3d.Origin + (1.0 / draw.Length) * sum;
}
```

**To**

```csharp
// HalfAngle DELETED
// RadiusAbout DELETED
// TorusRadii DELETED
// Centroid DELETED
```

```csharp
Vector3d unit = Unit(axis);
double half = 0.0;
foreach (int index in draw) {
    Vector3d rel = cloud[index] - apex;
    double along = Math.Abs(rel * unit);
    half += Math.Atan2((rel - (rel * unit) * unit).Length, along);
}
return Fin.Succ((FitPrimitive)new FitPrimitive.Cone(apex, unit, half / draw.Length));
```

```csharp
Vector3d unit = Unit(n);
double radius = 0.0;
foreach (int index in draw) {
    Vector3d rel = cloud[index] - anchor;
    radius += (rel - (rel * unit) * unit).Length;
}
radius /= draw.Length;
return radius <= 0.0
    ? Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cylinder, draw[0], "zero-radius"))
    : Fin.Succ((FitPrimitive)new FitPrimitive.Cylinder(
        new Rhino.Geometry.Cylinder(new Circle(new Rhino.Geometry.Plane(anchor, n), radius))));
```

```csharp
Vector3d sum = Vector3d.Zero;
foreach (int index in draw) sum += cloud[index] - Point3d.Origin;
Point3d center = Point3d.Origin + sum / draw.Length;
Vector3d unit = Unit(axis);
double major = 0.0;
double[] radial = new double[draw.Length];
for (int i = 0; i < draw.Length; i++) {
    Vector3d rel = cloud[draw[i]] - center;
    radial[i] = (rel - (rel * unit) * unit).Length;
    major += radial[i];
}
major /= draw.Length;
double minor = 0.0;
for (int i = 0; i < draw.Length; i++) {
    Vector3d rel = cloud[draw[i]] - center;
    double along = rel * unit;
    double inPlane = radial[i] - major;
    minor += Math.Sqrt(inPlane * inPlane + along * along);
}
return Fin.Succ((FitPrimitive)new FitPrimitive.Torus(center, unit, major, minor / draw.Length));
```

**Why**

Each reducer has one caller and exists only to forward that caller's sample, geometry, and local accumulators. Once the failed-apex fallback is removed, `Centroid` also has one caller.

**Change**

Move the half-angle fold into `SolveCone`, the radius fold and refusal into `SolveCylinder`, and the centroid plus two radius folds into `SolveTorus`. Preserve the arithmetic and typed refusal while deleting the four method boundaries.

**Delta**

Code-fence LOC: -10. Module-level types: +0/-0/net 0. Module-level members: +0/-4/net -4.

# 6. Fuse indexed admission and reject unusable normals

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:715`

```csharp
static Fin<FitOp> Validate(FitOp op) {
    int minimal = op.Kinds.Map(static kind => kind.MinimalSamples).Fold(0, Math.Max);
    Seq<int> badPoints = toSeq(op.Cloud).Map(static (point, index) => (Point: point, Index: index))
        .Filter(static row => !row.Point.IsValid).Map(static row => row.Index);
    Seq<int> badNormals = op.Normals.Match(
        Some: field => toSeq(field).Map(static (normal, index) => (Normal: normal, Index: index))
            .Filter(static row => !row.Normal.IsValid).Map(static row => row.Index),
        None: static () => Seq<int>());
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:730`

```csharp
+ badPoints.Map(index =>
    (Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.Point, index, "non-finite"))
+ op.Normals.Match(
    Some: field => field.Length != op.Cloud.Length
        ? Seq((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, None, $"normals-arity:{field.Length}!={op.Cloud.Length}"))
        : Seq<Validation<Error, Unit>>(),
    None: static () => Seq<Validation<Error, Unit>>())
+ badNormals.Map(index =>
    (Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, index, "non-finite-normal"))
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:743`

```csharp
return AdmissionSlots.Accumulate(probes).Map(_ => op).ToFin();
```

**To**

```csharp
static Fin<Unit> Validate(FitOp op) {
    int minimal = op.Kinds.Map(static kind => kind.MinimalSamples).Fold(0, Math.Max);
```

```csharp
+ toSeq(op.Cloud).Choose((index, point) => point.IsValid
    ? Option<Validation<Error, Unit>>.None
    : Some((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.Point, index, "non-finite")))
+ op.Normals.Match(
    Some: field =>
        (field.Length != op.Cloud.Length
            ? Seq((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, None, $"normals-arity:{field.Length}!={op.Cloud.Length}"))
            : Seq<Validation<Error, Unit>>())
        + toSeq(field).Choose((index, normal) => normal.IsValid && !normal.IsTiny(EpsilonPolicy.ZeroTolerance)
            ? Option<Validation<Error, Unit>>.None
            : Some((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, index, "invalid-normal"))),
    None: static () => Seq<Validation<Error, Unit>>())
```

```csharp
return AdmissionSlots.Accumulate(probes).ToFin();
```

**Why**

The two `Map`→`Filter`→`Map` chains allocate tuple carriers and named sequences only to preserve indices. A finite zero normal passes current admission even though agreement and normal-seeded solvers require a direction. Returning the request repeats a value already closed over by `Apply`.

**Change**

Use LanguageExt's indexed `Choose` to emit faults directly, reject non-finite and tiny normals in the same pass, and return `Fin<Unit>` without remapping the request.

**Delta**

Code-fence LOC: -4. Module-level types: +0/-0/net 0. Module-level members: +0/-0/net 0.

# 7. Internalize fit state and rebuild RANSAC as a bounded pure loop

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:638`

```csharp
public readonly record struct Candidate(FitPrimitive Primitive, Arr<int> Inliers, double Cost, int Trials) {
    public int InlierCount => Inliers.Count;
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:660`

```csharp
sealed class FitModel(FitPrimitive template, Point3d[] cloud, Arr<int> inliers) : ILmModel {
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:750`

```csharp
static Option<Candidate> Draw(
    Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] order, int[] whole,
    FitKind kind, FitPolicy policy, Context tolerance, Deterministic.Draw draw, Op key) {
    double threshold = policy.Threshold(tolerance.Absolute.Value);
    double t2 = threshold * threshold;
    Deterministic.Draw lane = draw.At(TrialLane, kind.Lane);
    return IO.pure(value: unit).FoldWhile(
            schedule: Schedule.recurs(times: policy.MaxTrials - 1),
            initialState: (Best: Option<Candidate>.None, Budget: policy.MaxTrials, Trial: 0),
            folder: (acc, _) => {
                (Option<Candidate> best, int budget, int trial) = acc;
                if (Sample(order, cloud, index, kind, policy, trial, lane, key).Case is not int[] sample) { return (best, budget, trial + 1); }
                if (kind.Minimal(cloud, sample, normals, tolerance, key).Case is not FitPrimitive primitive) { return (best, budget, trial + 1); }
                (double cost, Arr<int> inliers) = Score(primitive, cloud, normals, index, whole, policy, t2, threshold, key);
                return best.Case is Candidate held && cost >= held.Cost
                    ? (best, budget, trial + 1)
                    : (Some(new Candidate(primitive, inliers, cost, trial + 1)),
                        AdaptiveBudget(inliers.Count, cloud.Length, kind.MinimalSamples, policy), trial + 1);
            },
            stateIs: static acc => acc.Trial < acc.Budget)
        .Run()
        .Best;
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:803`

```csharp
static int AdaptiveBudget(int inlierCount, int total, int minimalSamples, FitPolicy policy) {
    double fraction = (double)inlierCount / total;
    if (fraction <= 0.0) return policy.MaxTrials;
    double denom = Math.Log(1.0 - Math.Pow(fraction, minimalSamples));
    if (denom >= 0.0) return policy.MaxTrials;
    int estimate = (int)Math.Ceiling(Math.Log(1.0 - policy.Confidence.Value) / denom);
    return Math.Clamp(estimate, minimalSamples, policy.MaxTrials);
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:851`

```csharp
static Fin<int[]> Sample(int[] order, Point3d[] cloud, NeighborIndex index, FitKind kind, FitPolicy policy, int trial, Deterministic.Draw lane, Op key) =>
    policy.Order.Switch(
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:891`

```csharp
static int[] NeighborhoodDraw(int seed, int[] pool, int count, ref ulong state) {
    int[] sample = new int[count];
    sample[0] = seed;
    for (int i = 1; i < count; i++) {
        int pick = Deterministic.NextBelow(state: ref state, exclusiveCeiling: pool.Length - (i - 1));
        sample[i] = pool[pick];
        (pool[pick], pool[pool.Length - i]) = (pool[pool.Length - i], pool[pick]);
    }
    return sample;
}
```

**To**

```csharp
public static class Fit {
    private readonly record struct Candidate(FitPrimitive Primitive, Arr<int> Inliers, ddouble Cost, int Trial);

    // Candidate.InlierCount DELETED

    private sealed class Model(FitPrimitive template, Point3d[] cloud, Arr<int> inliers) : ILmModel {
```

```csharp
static Option<Candidate> Draw(
    Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] order, int[] whole,
    FitKind kind, FitPolicy policy, Context tolerance, Deterministic.Draw draw, Op key) {
    double threshold = policy.InlierScale.Value * tolerance.Absolute.Value;
    double t2 = threshold * threshold;
    Deterministic.Draw lane = draw.At(TrialLane, kind.Lane);
    Option<Candidate> best = None;
    int budget = policy.MaxTrials.Value, support = 0;
    for (int trial = 0; trial < budget; trial++) {
        int[] sample = Sample(order, cloud, index, kind, policy, trial, lane, key);
        if (kind.Solve(cloud, sample, normals, tolerance, key).Case is not FitPrimitive primitive) continue;
        (ddouble cost, Arr<int> inliers) = Score(primitive, cloud, normals, index, whole, policy, t2, threshold, key);
        if (inliers.Count > support) {
            support = inliers.Count;
            double fraction = (double)support / cloud.Length;
            double miss = 1.0 - Math.Pow(fraction, kind.MinimalSamples);
            int estimate = miss <= 0.0 ? 1 : miss >= 1.0 ? policy.MaxTrials.Value
                : (int)Math.Min(policy.MaxTrials.Value, Math.Ceiling(Math.Log(1.0 - policy.Confidence.Value) / Math.Log(miss)));
            budget = Math.Min(budget, Math.Max(1, estimate));
        }
        best = Some(best.Filter(held => held.Cost <= cost).IfNone(new Candidate(primitive, inliers, cost, trial + 1)));
    }
    return best;
}

// AdaptiveBudget DELETED
```

```csharp
static int[] Sample(int[] order, Point3d[] cloud, NeighborIndex index, FitKind kind, FitPolicy policy, int trial, Deterministic.Draw lane, Op key) =>
    policy.Sampling.Switch(
        state: (Order: order, Cloud: cloud, Index: index, Kind: kind, Policy: policy, Trial: trial, State: lane.At(trial).State, Key: key),
        random: static s => {
            ulong draw = s.State;
            return UniformDraw(s.Order, s.Kind.MinimalSamples, ref draw);
        },
        prosac: static s => {
            int window = Math.Min(s.Order.Length, s.Kind.MinimalSamples + s.Trial);
            int[] sample = new int[s.Kind.MinimalSamples];
            sample[0] = s.Order[window - 1];
            ulong draw = s.State;
            for (int i = 1; i < sample.Length; i++) {
                int pick;
                do { pick = s.Order[Deterministic.NextBelow(state: ref draw, exclusiveCeiling: window - 1)]; }
                while (System.Array.IndexOf(sample, pick, 0, i) >= 0);
                sample[i] = pick;
            }
            return sample;
        },
        napsac: static s => {
            ulong draw = s.State;
            int seed = s.Order[Deterministic.NextBelow(state: ref draw, exclusiveCeiling: s.Order.Length)];
            return NeighborKernel.GraphOf(s.Index, [s.Cloud[seed]], Some(s.Policy.Neighbors), Option<PositiveMagnitude>.None, s.Key).Match(
                Succ: graph => {
                    int[] pool = graph.Ids[0].Where(id => id != seed).ToArray();
                    if (pool.Length < s.Kind.MinimalSamples - 1) return UniformDraw(s.Order, s.Kind.MinimalSamples, ref draw);
                    int[] sample = new int[s.Kind.MinimalSamples];
                    sample[0] = seed;
                    for (int i = 1; i < sample.Length; i++) {
                        int pick = Deterministic.NextBelow(state: ref draw, exclusiveCeiling: pool.Length - i + 1);
                        sample[i] = pool[pick];
                        (pool[pick], pool[pool.Length - i]) = (pool[pool.Length - i], pool[pick]);
                    }
                    return sample;
                },
                Fail: _ => UniformDraw(s.Order, s.Kind.MinimalSamples, ref draw));
        });

// NeighborhoodDraw DELETED
```

**Why**

`Candidate` and `FitModel` are implementation state exposed as module-level types, and `InlierCount` forwards one property. The trial is deterministic and pure, so an `IO` scheduler is an effect shell. The budget clamps trial count to sample arity and updates only when cost improves, although RANSAC confidence is governed by the greatest support independently of the cost winner. `Sample` cannot fail because NAPSAC falls back to random sampling, and `NeighborhoodDraw` has one caller.

**Change**

Nest the implementation types, rename the adapter to `Model`, delete the count forwarder, return samples directly, and use one bounded loop. Track maximum support separately from minimum cost, lower the budget monotonically to at least one, cap before narrowing, preserve the winning one-based `Trial`, and inline NAPSAC's partial Fisher-Yates draw. Delete `IO.FoldWhile`, `Schedule.recurs`, `AdaptiveBudget`, and `NeighborhoodDraw`.

**Delta**

Code-fence LOC: -9. Module-level types: +0/-2/net -2. Module-level members: +0/-3/net -3.

# 8. Flatten candidate egress and carry typed fit evidence

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:642`

```csharp
public sealed record Fitted(
    FitPrimitive Primitive,
    Arr<int> Inliers,
    double Residual,
    double Consensus,
    int Trials,
    int Iterations) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Primitive is not null,
        ValidityClaim.CountAtLeast(count: Inliers.Count, floor: 1),
        ValidityClaim.Finite(Residual),
        ValidityClaim.Nonnegative(Residual),
        ValidityClaim.UnitInterval(Consensus),
        ValidityClaim.CountAtLeast(count: Trials, floor: 1),
        ValidityClaim.CountAtLeast(count: Iterations, floor: 0));
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:699`

```csharp
.Bind(order => op.Kinds
    .Fold(Option<Candidate>.None, (best, kind) => Draw(op.Cloud, op.Normals, index, order, whole, kind, op.Policy, tolerance, draw, ok).Match(
        Some: next => Some(best.Case is Candidate held && held.Cost <= next.Cost ? held : next),
        None: () => best))
    .Match(
        Some: best => Consensus(best, op, ok).Bind(fraction => fraction.Value < op.Policy.InlierFloor.Value
            ? Fin.Fail<Fitted>(new GeometryFault.InsufficientInliers(fraction, op.Policy.InlierFloor))
            : Refine(best, op.Cloud, op.Normals, index, whole, op.Policy, tolerance, ok)),
        None: () => Fin.Fail<Fitted>(new GeometryFault.DegenerateInput(Kind.PointCloud, None, "no-candidate")))));
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:712`

```csharp
static Fin<UnitInterval> Consensus(Candidate best, FitOp op, Op key) =>
    key.AcceptValidated<UnitInterval>(candidate: (double)best.InlierCount / op.Cloud.Length);
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:912`

```csharp
static Fin<Fitted> Refine(Candidate seed, Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] whole, FitPolicy policy, Context tolerance, Op key) =>
    Lm.Minimize(new FitModel(seed.Primitive, cloud, seed.Inliers), policy.Ladder, key).Bind(result => {
        FitPrimitive refined = seed.Primitive.Kind.Unpack(result.Parameters.AsSpan());
        double threshold = policy.Threshold(tolerance.Absolute.Value);
        (double _, Arr<int> mask) = Score(refined, cloud, normals, index, whole, policy, threshold * threshold, threshold, key);
        return from residual in Rms(refined, cloud, mask, key)
               from consensus in key.AcceptValidated<UnitInterval>(candidate: (double)mask.Count / cloud.Length)
               select new Fitted(refined, mask, residual, consensus.Value, seed.Trials, result.Iterations);
    });

static Fin<double> Rms(FitPrimitive shape, Point3d[] cloud, Arr<int> inliers, Op key) =>
    inliers.IsEmpty
        ? Fin.Fail<double>(key.InvalidResult())
        : key.AcceptValue((double)ddouble.Sqrt(inliers.Select(i => { double d = shape.Distance(cloud[i]); return (ddouble)d * d; }).Sum()) / Math.Sqrt(inliers.Count));
```

**To**

```csharp
public sealed record Fitted(
    FitPrimitive Primitive,
    Arr<int> Inliers,
    double Rms,
    UnitInterval Consensus,
    int Trial,
    int Iterations) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Primitive is not null,
        ValidityClaim.CountAtLeast(count: Inliers.Count, floor: 1),
        ValidityClaim.Finite(Rms),
        ValidityClaim.Nonnegative(Rms),
        ValidityClaim.CountAtLeast(count: Trial, floor: 1),
        ValidityClaim.CountAtLeast(count: Iterations, floor: 0));
}
```

```csharp
.Bind(order => op.Kinds
    .Fold(Option<Candidate>.None, (best, kind) => Draw(op.Cloud, op.Normals, index, order, whole, kind, op.Policy, tolerance, draw, ok).Match(
        Some: next => Some(best.Filter(held => held.Cost <= next.Cost).IfNone(next)),
        None: () => best))
    .ToFin(new GeometryFault.DegenerateInput(Kind.PointCloud, None, "no-candidate")))
.Bind(best => {
    UnitInterval fraction = UnitInterval.Create((double)best.Inliers.Count / op.Cloud.Length);
    return fraction.Value < op.Policy.InlierFloor.Value
        ? Fin.Fail<Fitted>(new GeometryFault.InsufficientInliers(fraction, op.Policy.InlierFloor))
        : Refine(best, op.Cloud, op.Normals, index, whole, op.Policy, tolerance, ok);
}));
```

```csharp
// Consensus DELETED
```

```csharp
static Fin<Fitted> Refine(Candidate seed, Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] whole, FitPolicy policy, Context tolerance, Op key) =>
    Lm.Minimize(new Model(seed.Primitive, cloud, seed.Inliers), policy.Refine, key).Bind(result => {
        FitPrimitive refined = seed.Primitive.Kind.Rebuild(result.Parameters.AsSpan());
        double threshold = policy.InlierScale.Value * tolerance.Absolute.Value;
        (ddouble _, Arr<int> mask) = Score(refined, cloud, normals, index, whole, policy, threshold * threshold, threshold, key);
        return mask.IsEmpty
            ? Fin.Fail<Fitted>(key.InvalidResult())
            : key.AcceptValue(new Fitted(
                refined, mask,
                (double)ddouble.Sqrt(mask.Select(i => { double d = refined.Distance(cloud[i]); return (ddouble)d * d; }).Sum()) / Math.Sqrt(mask.Count),
                UnitInterval.Create((double)mask.Count / cloud.Length), seed.Trial, result.Iterations));
    });

// Rms DELETED
```

**Why**

The candidate exit hand-matches `Option` and revalidates two mathematically bounded ratios. `Residual` is RMS error; `Consensus` stored as `double` repeats the unit-interval claim; plural `Trials` contains one winning trial ordinal; and both helpers have one caller.

**Change**

Use `Option.ToFin`, `Filter`, and `IfNone`; construct derived ratios with `UnitInterval.Create`; store typed consensus; rename `Residual` to `Rms` and `Trials` to `Trial`; and inline `Consensus` and `Rms`.

**Delta**

Code-fence LOC: -8. Module-level types: +0/-0/net 0. Module-level members: +0/-2/net -2.

**Ripples**

- `libs/dotnet/Rasm.Fabrication/.planning/Verify/probing.md:1089`: replace `fitted.Residual` with `fitted.Rms`.

# 9. Compute only the PROSAC order and delete ranking wrappers

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:694`

```csharp
Order(op.Cloud, op.Normals, op.Policy, tolerance, draw, ok)
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:747`

```csharp
const long ShuffleLane = 0L;
const long TrialLane = 1L;
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:812`

```csharp
static Fin<int[]> Order(Point3d[] cloud, Option<Vector3d[]> normals, FitPolicy policy, Context tolerance, Deterministic.Draw draw, Op key) {
    int[] indices = [.. Enumerable.Range(0, cloud.Length)];
    return policy.Order == DrawOrder.QualityFront
        ? Quality(cloud, normals, tolerance, key).Map(quality => {
            System.Array.Sort(indices, (a, b) => quality[b].CompareTo(quality[a]));
            return indices;
        })
        : key.AcceptValue(Shuffled(indices, draw.At(ShuffleLane).State));
}

static Fin<double[]> Quality(Point3d[] cloud, Option<Vector3d[]> normals, Context tolerance, Op key) =>
    normals.Match(
        Some: field => key.AcceptValue(ModePrior(field)),
        None: () => CloudKernel.CovarianceOf(toSeq(cloud), Option<Arr<double>>.None, key)
            .Bind(stats => stats.Cov.DecomposeEigenDetailed(key).Bind(solved => solved.PairsIn(expected: EigenOrder.DescendingMagnitude, key: key)).Map(eigen => (stats.Mean, Eigen: eigen)))
            .Bind(pca => pca.Eigen.Count >= 3
                ? key.AcceptValue(PlanarityPrior(cloud, pca.Mean, pca.Eigen, tolerance))
                : Fin.Fail<double[]>(key.InvalidResult())));
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:831`

```csharp
static double[] ModePrior(Vector3d[] field) {
    Vector3d mean = Vector3d.Zero;
    foreach (Vector3d n in field) mean += n;
    Vector3d mode = FitKind.Unit(mean);
    double[] quality = new double[field.Length];
    for (int i = 0; i < field.Length; i++) quality[i] = Math.Abs(field[i] * mode);
    return quality;
}

static double[] PlanarityPrior(Point3d[] cloud, Vector3d mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> eigen, Context tolerance) {
    Vector3d axis = new(eigen[2].Eigenvector[0], eigen[2].Eigenvector[1], eigen[2].Eigenvector[2]);
    double floor = Math.Max(Math.Sqrt(Math.Abs(eigen[2].Eigenvalue)), tolerance.Absolute.Value);
    double[] quality = new double[cloud.Length];
    for (int i = 0; i < cloud.Length; i++) {
        Vector3d rel = cloud[i] - (Point3d.Origin + mean);
        quality[i] = 1.0 / (1.0 + Math.Abs(rel * axis) / floor);
    }
    return quality;
}
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:902`

```csharp
static int[] Shuffled(int[] source, ulong state) {
    int[] order = [.. source];
    for (int i = order.Length - 1; i > 0; i--) {
        int j = Deterministic.NextBelow(state: ref state, exclusiveCeiling: i + 1);
        (order[i], order[j]) = (order[j], order[i]);
    }
    return order;
}
```

**To**

```csharp
Order(op.Cloud, op.Normals, op.Policy, tolerance, ok)
```

```csharp
const long TrialLane = 0L;

// ShuffleLane DELETED
```

```csharp
static Fin<int[]> Order(Point3d[] cloud, Option<Vector3d[]> normals, FitPolicy policy, Context tolerance, Op key) {
    int[] indices = [.. Enumerable.Range(0, cloud.Length)];
    if (policy.Sampling != SampleMode.Prosac) return key.AcceptValue(indices);
    return normals.Match(
            Some: field => {
                Vector3d mean = Vector3d.Zero;
                foreach (Vector3d normal in field) mean += normal;
                Vector3d mode = FitKind.Unit(mean);
                double[] rank = new double[field.Length];
                for (int i = 0; i < field.Length; i++) rank[i] = Math.Abs(field[i] * mode);
                return key.AcceptValue(rank);
            },
            None: () => CloudKernel.CovarianceOf(toSeq(cloud), Option<Arr<double>>.None, key)
                .Bind(stats => stats.Cov.DecomposeEigenDetailed(key)
                    .Bind(solved => solved.PairsIn(EigenOrder.DescendingMagnitude, key))
                    .Map(eigen => (stats.Mean, Eigen: eigen)))
                .Bind(pca => {
                    if (pca.Eigen.Count < 3) return Fin.Fail<double[]>(key.InvalidResult());
                    Vector3d axis = new(pca.Eigen[2].Eigenvector[0], pca.Eigen[2].Eigenvector[1], pca.Eigen[2].Eigenvector[2]);
                    double floor = Math.Max(Math.Sqrt(Math.Abs(pca.Eigen[2].Eigenvalue)), tolerance.Absolute.Value);
                    double[] rank = new double[cloud.Length];
                    for (int i = 0; i < cloud.Length; i++) {
                        Vector3d rel = cloud[i] - (Point3d.Origin + pca.Mean);
                        rank[i] = 1.0 / (1.0 + Math.Abs(rel * axis) / floor);
                    }
                    return key.AcceptValue(rank);
                }))
        .Map(rank => {
            System.Array.Sort(indices, (a, b) => rank[b].CompareTo(rank[a]));
            return indices;
        });
}

// Quality DELETED
// ModePrior DELETED
// PlanarityPrior DELETED
// Shuffled DELETED
```

**Why**

Random sampling already draws uniformly from the full index set, and NAPSAC already draws its seed uniformly; pre-shuffling either set changes deterministic decoration, not the distribution. The shuffle lane, copy, and loop are redundant. `Quality`, `ModePrior`, and `PlanarityPrior` each have one caller and split one PROSAC decision across four members.

**Change**

Return natural indices for random and NAPSAC modes, compute and sort ranks only for PROSAC, move both rank folds into `Order`, remove its draw parameter, and delete the shuffle lane plus four helpers. Update deterministic-stream prose and the diagram.

**Delta**

Code-fence LOC: -10. Module-level types: +0/-0/net 0. Module-level members: +0/-5/net -5.

# 10. Keep bounded-support scoring private and compare at 106 bits

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:441`

```csharp
public Option<(Point3d Center, double Reach)> Support(double threshold) =>
    Switch(
        state: threshold,
        plane:    static (_, _) => Option<(Point3d, double)>.None,
        sphere:   static (t, s) => Some((s.Surface.Center, s.Surface.Radius + t)),
        cylinder: static (_, _) => Option<(Point3d, double)>.None,
        cone:     static (_, _) => Option<(Point3d, double)>.None,
        torus:    static (t, r) => Some((r.Center, r.Major + r.Minor + t)),
        line:     static (_, _) => Option<(Point3d, double)>.None,
        circle:   static (t, c) => Some((c.Curve.Center, c.Curve.Radius + t)));
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:774`

```csharp
static (double Cost, Arr<int> Inliers) Score(
    FitPrimitive primitive, Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] whole,
    FitPolicy policy, double t2, double threshold, Op key) =>
    policy.Score.Saturating
        ? primitive.Support(threshold).Match(
            Some: ball => key.AcceptValidated<PositiveMagnitude>(candidate: ball.Reach)
                .Bind(reach => NeighborKernel.GraphOf(index: index, needles: [ball.Center], count: Option<Dimension>.None, radius: Some(reach), key: key)).Match(
                Succ: graph => Scored(primitive, cloud, graph.Ids[0], policy.Score.Cost(t2, t2) * (cloud.Length - graph.Ids[0].Length), normals, policy, t2, threshold),
                Fail: _ => Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold)),
            None: () => Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold))
        : Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold);
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:786`

```csharp
static (double Cost, Arr<int> Inliers) Scored(
```

`libs/dotnet/Rasm/.planning/Solving/fit.md:800`

```csharp
return ((double)cost, new Arr<int>(inliers));
```

**To**

```csharp
// FitPrimitive.Support DELETED
```

```csharp
static (ddouble Cost, Arr<int> Inliers) Score(
    FitPrimitive primitive, Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] whole,
    FitPolicy policy, double t2, double threshold, Op key) {
    if (!policy.Cost.Saturating)
        return Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold);
    Option<(Point3d Center, double Reach)> support = primitive.Switch(
        state: threshold,
        plane:    static (_, _) => Option<(Point3d, double)>.None,
        sphere:   static (t, s) => Some((s.Surface.Center, s.Surface.Radius + t)),
        cylinder: static (_, _) => Option<(Point3d, double)>.None,
        cone:     static (_, _) => Option<(Point3d, double)>.None,
        torus:    static (t, r) => Some((r.Center, r.Major + r.Minor + t)),
        line:     static (_, _) => Option<(Point3d, double)>.None,
        circle:   static (t, c) => Some((c.Curve.Center, c.Curve.Radius + t)));
    return support.Match(
        Some: ball => NeighborKernel.GraphOf(index, [ball.Center], Option<Dimension>.None,
            Some(PositiveMagnitude.Create(ball.Reach)), key).Match(
            Succ: graph => Scored(primitive, cloud, graph.Ids[0],
                policy.Cost.Cost(t2, t2) * (cloud.Length - graph.Ids[0].Length), normals, policy, t2, threshold),
            Fail: _ => Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold)),
        None: () => Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold));
}
```

```csharp
static (ddouble Cost, Arr<int> Inliers) Scored(
```

```csharp
return (cost, new Arr<int>(inliers));
```

**Why**

`Support` is a private saturation optimization with one caller, not analytic-primitive output. The fold accumulates in `ddouble` but narrows before candidates compete, contradicting the 106-bit ordering claim and allowing close costs to collapse to equality.

**Change**

Move support projection into the saturating score branch, use generated `PositiveMagnitude.Create` for the proven reach, delete the public member, and retain `ddouble` through `Scored`, `Score`, `Candidate.Cost`, and cross-kind election.

**Delta**

Code-fence LOC: -1. Module-level types: +0/-0/net 0. Module-level members: +0/-1/net -1.

# 11. Delete the RhinoCommon degeneracy wrapper

**From**

`libs/dotnet/Rasm/.planning/Solving/fit.md:928`

```csharp
file static class FitVectorExtensions {
    public static bool IsTiny(this Vector3d v) => v.SquareLength <= EpsilonPolicy.ZeroTolerance * EpsilonPolicy.ZeroTolerance;
}
```

**To**

```csharp
// FitVectorExtensions DELETED
```

**Why**

The extension hand-rolls the catalogued `Vector3d.IsTiny(double)` operation, adding a module-level type and member solely to hide the package capability.

**Change**

Replace every parameterless `IsTiny()` call with `IsTiny(EpsilonPolicy.ZeroTolerance)` and delete the extension type.

**Delta**

Code-fence LOC: -3. Module-level types: +0/-1/net -1. Module-level members: +0/-1/net -1.
