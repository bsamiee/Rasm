# 1. Admit stencil width once and accumulate axis failures

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:37`
```csharp
public static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, double eps) =>
    from _ in guard(double.IsFinite(eps) && eps > EpsilonPolicy.ZeroTolerance, new KernelFault.InvalidInput()).ToFin()
    from xp in sampler(arg: point + (eps * Vector3d.XAxis))
    from xm in sampler(arg: point - (eps * Vector3d.XAxis))
    from yp in sampler(arg: point + (eps * Vector3d.YAxis))
    from ym in sampler(arg: point - (eps * Vector3d.YAxis))
    from zp in sampler(arg: point + (eps * Vector3d.ZAxis))
    from zm in sampler(arg: point - (eps * Vector3d.ZAxis))
    select (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm);
```

To:
```csharp
private static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, PositiveMagnitude epsilon) =>
    (sampler(point + (epsilon.Value * Vector3d.XAxis)).ToValidation(),
     sampler(point - (epsilon.Value * Vector3d.XAxis)).ToValidation(),
     sampler(point + (epsilon.Value * Vector3d.YAxis)).ToValidation(),
     sampler(point - (epsilon.Value * Vector3d.YAxis)).ToValidation(),
     sampler(point + (epsilon.Value * Vector3d.ZAxis)).ToValidation(),
     sampler(point - (epsilon.Value * Vector3d.ZAxis)).ToValidation())
        .Apply(static (xp, xm, yp, ym, zp, zm) => (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm)).As().ToFin();
```

Why: Current field cases already carry `PositiveMagnitude`, whose gate is finite and above `EpsilonPolicy.ZeroTolerance`; projecting it to `double` reopens admission. The six pure taps are independent and should accumulate typed failures, while their tuple is private implementation material.

Change: Change every ambient operator to `PositiveMagnitude epsilon`, read `epsilon.Value` in arithmetic, make `SampleAxes` private, and combine its taps with arity-six `Validation.Apply`.

Delta: -1 LOC; 0 module-level members or types; -1 public API member and six raw primitive parameters removed.

Ripples: `libs/dotnet/Rasm/.planning/Spatial/fields.md` — pass `c.Epsilon`, not `c.Epsilon.Value`, to `Nabla.GradientAt` and `Nabla.LaplacianAt`.

# 2. Accumulate independent curl-noise gradients

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:58`
```csharp
public static Fin<Vector3d> CurlNoiseAt(Func<Point3d, Fin<double>> sampler, Point3d point, double eps) =>
    from g1 in GradientAt(sampler, point, eps)
    let offset = new Vector3d(eps, 1.3 * eps, 0.7 * eps)
    from g2 in GradientAt(sampler, point + (offset * 137.0), eps)
    from g3 in GradientAt(sampler, point - (offset * 311.0), eps)
    from raw in Acceptance.Value(new Vector3d(g3.Y - g2.Z, g1.Z - g3.X, g2.X - g1.Y))
    select raw;
```

To:
```csharp
public static Fin<Vector3d> CurlNoiseAt(Func<Point3d, Fin<double>> sampler, Point3d point, PositiveMagnitude epsilon) =>
    new Vector3d(epsilon.Value, 1.3 * epsilon.Value, 0.7 * epsilon.Value) switch {
        var offset => (GradientAt(sampler, point, epsilon).ToValidation(),
                       GradientAt(sampler, point + (offset * 137.0), epsilon).ToValidation(),
                       GradientAt(sampler, point - (offset * 311.0), epsilon).ToValidation())
            .Apply(static (g1, g2, g3) => new Vector3d(g3.Y - g2.Z, g1.Z - g3.X, g2.X - g1.Y))
            .As().ToFin().Bind(static value => Acceptance.Value(value)),
    };
```

Why: The three admitted-width gradient reads are independent; sequential binding suppresses later sampler faults.

Change: Combine the gradients applicatively and validate only their dependent vector result.

Delta: +2 LOC; 0 module-level members or types.

# 3. Make reflected indexing total

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:97`
```csharp
private static int Reflect(int index, int count) =>
    count is 1 ? 0 : index < 0 ? -index : index >= count ? (2 * count) - index - 2 : index;
```

To:
```csharp
private static int Reflect(long index, int count) {
    if (count is 1) { return 0; }
    long period = 2L * (count - 1L);
    long folded = ((index % period) + period) % period;
    return (int)(folded < count ? folded : period - folded);
}
```

Why: The current expression reflects only one cell beyond an edge. Widening before offset arithmetic also prevents extreme public coordinates from wrapping before reflection.

Change: Use triangular-wave reduction in `long` and perform every `column`, `row`, and `layer` ±1 tap offset in `long`.

Delta: +4 LOC; 0 module-level members or types.

# 4. Delete the toroidal forwarding helper

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:87`
```csharp
public static Point3d ToroidalWrap(Point3d sample, Vector3d period) =>
    new(x: Reduce.Centred(value: sample.X, period: period.X),
        y: Reduce.Centred(value: sample.Y, period: period.Y),
        z: Reduce.Centred(value: sample.Z, period: period.Z));
```

To:
```csharp
// Nabla.ToroidalWrap DELETED
```

Why: The unused member only projects the already-owned `Reduce.Centred` operation over three public components.

Change: Delete `ToroidalWrap` and its prose; apply `Reduce.Centred` at the eventual coordinate projection.

Delta: -4 LOC; -1 module-level member; 0 types.

# 5. Delete the detached lattice gate

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:93`
```csharp
public static Fin<Unit> AdmitLattice(ReadOnlySpan<double> values, CellLattice grid) =>
    Admit.Claims((values.Length == grid.CellCount, "value-extent"),
        (grid.CellCount >= 1L, "lattice-census"));
```

To:
```csharp
// Nabla.AdmitLattice DELETED
```

Why: No caller invokes this gate, `CellLattice` already proves its census, and raster consumers intentionally pass halo windows rather than full-census spans.

Change: Delete `AdmitLattice` and the false claim that it proves later calls.

Delta: -3 LOC; -1 module-level member; 0 types.

# 6. Hide the lattice tap tuple

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:104`
```csharp
public static (double X1, double X0, double Y1, double Y0, double Z1, double Z0) LatticeAxes(
```

To:
```csharp
private static (double X1, double X0, double Y1, double Y0, double Z1, double Z0) LatticeAxes(
```

Why: Only the three lattice operators consume this ordering and border-policy tuple.

Change: Make `LatticeAxes` private.

Delta: 0 LOC; 0 module-level members or types; -1 public API member.

# 7. Hide the kernel origin policy

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:237`
```csharp
public KernelStatus Origin { get; }
```

To:
```csharp
private KernelStatus Origin { get; }
```

Why: Only `Profiled` reads this constructor policy; callers receive the complete `KernelProfile.Status` result.

Change: Make the smart-enum column private.

Delta: 0 LOC; 0 module-level members or types; -1 public API member.

# 8. Delete the radial-kernel span forwarding member

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:248`
```csharp
public Fin<Unit> Weights(ReadOnlySpan<double> distances, double radius, Span<double> destination) =>
    SpanProfile.Fill(distances: distances, scale: radius, destination: destination, row: q => Profiled(q: q, radius: radius).Value);
```

To:
```csharp
// KernelKind.Weights DELETED
```

Why: No consumer calls this closure-forwarding member, and it opens a second raw-radius admission path beside `Weight(double, PositiveMagnitude)`.

Change: Delete `KernelKind.Weights` and its advertised span modality.

Delta: -2 LOC; -1 module-level member; 0 types.

# 9. Use canonical reconstruction-weight names

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:272`
```csharp
public static readonly WeightKernel SmoothPoly = new(profile: static t => (1.0 - (t * t)) * (1.0 - (t * t)));
public static readonly WeightKernel CompactExp = new(profile: static t => Math.Exp(-(t * t) / Math.Max(1.0 - (t * t), EpsilonPolicy.ZeroTolerance)));
public static readonly WeightKernel Singular = new(profile: static t => 1.0 / Math.Max(t * t, EpsilonPolicy.SqrtEpsilon));
```

To:
```csharp
public static readonly WeightKernel Biweight = new(profile: static t => (1.0 - (t * t)) * (1.0 - (t * t)));
public static readonly WeightKernel Bump = new(profile: static t => Math.Exp(-(t * t) / Math.Max(1.0 - (t * t), EpsilonPolicy.ZeroTolerance)));
public static readonly WeightKernel RegularizedInverseSquare = new(profile: static t => 1.0 / Math.Max(t * t, EpsilonPolicy.SqrtEpsilon));
```

Why: The old names describe implementation fragments, and `Singular` is false after explicit epsilon regularization.

Change: Rename the rows without changing their formulas.

Delta: 0 LOC; 0 module-level members or types.

Ripples: `libs/dotnet/Rasm.Materials/.planning/Raster/plane.md` — replace the three names in the `WeightKernel` roster.

# 10. Delete the reconstruction-weight span forwarding member

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:281`
```csharp
public Fin<Unit> Weights(ReadOnlySpan<double> distances, double support, Span<double> destination) =>
    SpanProfile.Fill(distances: distances, scale: support, destination: destination, row: t => t >= 1.0 ? 0.0 : Profile(t: t));
```

To:
```csharp
// WeightKernel.Weights DELETED
```

Why: No consumer calls this forwarding member, and it duplicates support admission beside the scalar entry.

Change: Delete `WeightKernel.Weights`.

Delta: -2 LOC; -1 module-level member; 0 types.

# 11. Delete the orphaned span-profile owner

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:285`
```csharp
internal static class SpanProfile {
    internal static Fin<Unit> Fill(ReadOnlySpan<double> distances, double scale, Span<double> destination, Func<double, double> row) {
        Fin<Unit> admitted = Admit.Claims((distances.Length >= 1 && TensorPrimitives.Min<double>(distances) >= 0.0, "distances"),
            (destination.Length >= distances.Length, "destination-extent"),
            (ValidityClaim.Positive(scale), "scale"),
            (ValidityClaim.Finite(distances), "distances-finite"));
        if (admitted.IsFail) { return admitted; }
        Span<double> lane = destination[..distances.Length];
        TensorPrimitives.Divide(distances, scale, lane);
        for (int i = 0; i < lane.Length; i++) { lane[i] = row(arg: lane[i]); }
        return TensorPrimitives.IsFiniteAll<double>(lane) ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: new KernelFault.InvalidResult());
    }
}
```

To:
```csharp
// SpanProfile DELETED
```

Why: Removing its two forwarding callers leaves this divide-plus-loop shell orphaned.

Change: Delete `SpanProfile` and its Output prose.

Delta: -13 LOC; -1 module-level type and -1 module-level member.

# 12. Derive slope bounds through generated union dispatch

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:302`
```csharp
public sealed record ConstantCase : Falloff { internal ConstantCase() { } public override Option<double> SlopeBound => Some(0.0); }
public sealed record PowerCase : Falloff { internal PowerCase(double Exponent) => this.Exponent = Exponent; public double Exponent { get; } public override Option<double> SlopeBound => None; }
public sealed record GaussianCase : Falloff { internal GaussianCase(PositiveMagnitude Spread) => this.Spread = Spread; public PositiveMagnitude Spread { get; } public override Option<double> SlopeBound => Some(Math.Exp(-0.5) / Spread.Value); }
public sealed record KernelCase : Falloff { internal KernelCase(KernelKind Kind, PositiveMagnitude Radius) { this.Kind = Kind; this.Radius = Radius; } public KernelKind Kind { get; } public PositiveMagnitude Radius { get; } public override Option<double> SlopeBound => Some(Kind.DerivativeSupremum / Radius.Value); }
public sealed record MetricCase : Falloff { internal MetricCase(KernelKind Kind, Func<Point3d, Fin<SymmetricMatrix>> Metric, PositiveMagnitude Radius) { this.Kind = Kind; this.Metric = Metric; this.Radius = Radius; } public KernelKind Kind { get; } public Func<Point3d, Fin<SymmetricMatrix>> Metric { get; } public PositiveMagnitude Radius { get; } public override Option<double> SlopeBound => None; }
public abstract Option<double> SlopeBound { get; }
```

To:
```csharp
public sealed record ConstantCase : Falloff { internal ConstantCase() { } }
public sealed record PowerCase : Falloff { internal PowerCase(double Exponent) => this.Exponent = Exponent; public double Exponent { get; } }
public sealed record GaussianCase : Falloff { internal GaussianCase(PositiveMagnitude Spread) => this.Spread = Spread; public PositiveMagnitude Spread { get; } }
public sealed record KernelCase : Falloff { internal KernelCase(KernelKind Kind, PositiveMagnitude Radius) { this.Kind = Kind; this.Radius = Radius; } public KernelKind Kind { get; } public PositiveMagnitude Radius { get; } }
public sealed record MetricCase : Falloff { internal MetricCase(KernelKind Kind, Func<Point3d, Fin<SymmetricMatrix>> Metric, PositiveMagnitude Radius) { this.Kind = Kind; this.Metric = Metric; this.Radius = Radius; } public KernelKind Kind { get; } public Func<Point3d, Fin<SymmetricMatrix>> Metric { get; } public PositiveMagnitude Radius { get; } }
public Option<double> SlopeBound => Map<Option<double>>(
    constantCase: static _ => Some(0.0),
    powerCase: static p => p.Exponent switch { 0.0 => Some(0.0), 1.0 => Some(1.0), _ => Option<double>.None },
    gaussianCase: static g => Some(Math.Exp(-0.5) / g.Spread.Value),
    kernelCase: static k => double.IsFinite(k.Kind.DerivativeSupremum) ? Some(k.Kind.DerivativeSupremum / k.Radius.Value) : Option<double>.None,
    metricCase: static _ => Option<double>.None);
```

Why: Five overrides scatter one total projection. They also lose exact constant/linear power bounds and wrap infinite kernel suprema in `Some`.

Change: Replace the overrides with one exhaustive generated `Map`.

Delta: +5 LOC; -5 module-level members; 0 types.

# 13. Delete literal power-law aliases

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:312`
```csharp
public static Falloff Inverse => new PowerCase(Exponent: -1.0);
public static Falloff InverseSquare => new PowerCase(Exponent: -2.0);
```

To:
```csharp
// Falloff.Inverse and Falloff.InverseSquare DELETED
```

Why: Both unused properties expose fixed literals through extra names and bypass the carrier-typed `Power(double)` gate.

Change: Delete both aliases; callers express them as `Falloff.Power(-1.0)` and `Falloff.Power(-2.0)`.

Delta: -2 LOC; -2 module-level members; 0 types.

# 14. Correct power-law and kernel slope dispatch

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:329`
```csharp
powerCase: static (s, p) => s.Distance > s.Tolerance
    ? Acceptance.Value(value: Math.Abs(value: p.Exponent) * Math.Pow(x: s.Distance, y: p.Exponent - 1.0))
    : Fin.Fail<double>(new KernelFault.InvalidInput()),
kernelCase: static (s, k) => k.Kind.Profile(s.Distance, k.Radius.Value, s.Key)
    .Map(static profile => Math.Abs(profile.FirstDerivative)),
```

To:
```csharp
powerCase: static (s, p) => p.Exponent switch {
    0.0 => Fin.Succ(0.0),
    _ when p.Exponent >= 1.0 || s.Distance > s.Tolerance =>
        Acceptance.Value(Math.Abs(p.Exponent) * Math.Pow(s.Distance, p.Exponent - 1.0)),
    _ => Fin.Fail<double>(new KernelFault.InvalidInput()),
},
kernelCase: static (s, k) => k.Kind.Profile(s.Distance, k.Radius.Value)
    .Map(static profile => Math.Abs(profile.FirstDerivative)),
```

Why: The blanket gate rejects finite derivatives at zero for zero, linear, and superlinear powers. The kernel arm calls a nonexistent overload with a nonexistent `s.Key` field.

Change: Restrict the tolerance refusal to singular derivatives and call the catalogued two-argument `Profile`.

Delta: +4 LOC; 0 module-level members or types.

# 15. Delete the unused scalar falloff entry

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:325`
```csharp
public Fin<double> Weight(double distance, double tolerance) =>
    WeightCore(distance: distance, distanceSquared: distance * distance, offset: Option<(Vector3d Offset, Point3d Sample)>.None, tolerance: tolerance);
```

To:
```csharp
// Falloff.Weight(double, double) DELETED
```

Why: Every consumer has an offset and sample point. This convenience entry exists only to create an absent metric payload that the core later rejects.

Change: Delete the scalar overload and bare-distance modality.

Delta: -2 LOC; -1 module-level member; 0 types.

# 16. Remove the optional metric payload and forwarding core

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:327`
```csharp
public Fin<double> Weight(Vector3d offset, Point3d sample, double tolerance) =>
    WeightCore(distance: offset.Length, distanceSquared: offset.SquareLength, offset: Some((Offset: offset, Sample: sample)), tolerance: tolerance);
private Fin<double> WeightCore(double distance, double distanceSquared, Option<(Vector3d Offset, Point3d Sample)> offset, double tolerance) =>
    Admit.FalloffInput(distance: distance, distanceSquared: distanceSquared, tolerance: tolerance).Bind(_ => Switch(
        state: (Distance: distance, DistanceSquared: distanceSquared, Offset: offset, Tolerance: tolerance),
```

To:
```csharp
public Fin<double> Weight(Vector3d offset, Point3d sample, double tolerance) =>
    Admit.FalloffInput(distance: offset.Length, distanceSquared: offset.SquareLength, tolerance: tolerance).Bind(_ => Switch(
        state: (Distance: offset.Length, DistanceSquared: offset.SquareLength, Offset: offset, Sample: sample, Tolerance: tolerance),
```

Why: After the scalar entry is removed, the option is always `Some`; `WeightCore` is a one-call hop and the metric absence failure is unreachable.

Change: Promote the core body into `Weight`, delete `s.Offset.ToFin`, and read `s.Offset`/`s.Sample` directly in the metric arm.

Delta: -4 LOC; -1 module-level member; 0 types.

# 17. Stop rejecting regular power weights at zero

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:370`
```csharp
powerCase: static (s, p) => s.Distance > s.Tolerance
    ? Acceptance.Value(value: Math.Pow(x: s.Distance, y: p.Exponent))
    : Fin.Fail<double>(new KernelFault.InvalidInput()),
```

To:
```csharp
powerCase: static (s, p) => p.Exponent >= 0.0 || s.Distance > s.Tolerance
    ? Acceptance.Value(value: Math.Pow(x: s.Distance, y: p.Exponent))
    : Fin.Fail<double>(new KernelFault.InvalidInput()),
```

Why: Zero and positive powers are defined at zero; only negative powers require the exclusion band.

Change: Apply the tolerance check only to negative exponents.

Delta: 0 LOC; 0 module-level members or types.

# 18. Delete the falloff span dispatcher

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:341`
```csharp
public Fin<Unit> Weights(ReadOnlySpan<double> distances, double tolerance, Span<double> destination) {
    if (!ValidityClaim.All(distances.Length >= 1 && TensorPrimitives.Min<double>(distances) >= 0.0,
            ValidityClaim.CountAtLeast(destination.Length, distances.Length),
            ValidityClaim.Nonnegative(tolerance), ValidityClaim.Finite(distances))) {
        return Fin.Fail<Unit>(error: new KernelFault.InvalidInput());
    }
    Span<double> lane = destination[..distances.Length];
    Fin<Unit> filled = Fin.Fail<Unit>(error: new KernelFault.Unsupported(InputType: GetType(), OutputType: typeof(Span<double>)));
    switch (this) {
        case ConstantCase: lane.Fill(1.0); filled = Fin.Succ(value: unit); break;
        case PowerCase power when TensorPrimitives.Min<double>(distances) > tolerance:
            TensorPrimitives.Pow(distances, power.Exponent, lane); filled = Fin.Succ(value: unit); break;
        case PowerCase: filled = Fin.Fail<Unit>(error: new KernelFault.InvalidInput()); break;
        case GaussianCase gaussian:
            TensorPrimitives.Multiply(distances, distances, lane);
            TensorPrimitives.Multiply<double>(lane, -1.0 / (2.0 * gaussian.Spread.Value * gaussian.Spread.Value), lane);
            TensorPrimitives.Exp<double>(lane, lane); filled = Fin.Succ(value: unit); break;
        case KernelCase kernel:
            filled = kernel.Kind.Weights(distances: distances, radius: kernel.Radius.Value, destination: lane); break;
        case MetricCase:
            filled = Fin.Fail<Unit>(error: new KernelFault.Unsupported(InputType: typeof(MetricCase), OutputType: typeof(Span<double>))); break;
    }
    bool finite = TensorPrimitives.IsFiniteAll<double>(lane);
    return filled.Bind(_ => finite ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: new KernelFault.InvalidResult()));
}
```

To:
```csharp
// Falloff.Weights DELETED
```

Why: The unused member hand-assembles a second union algorithm, calls the deleted `KernelKind.Weights`, and applies the wrong near-zero rule to every power.

Change: Delete `Falloff.Weights` and the span contract.

Delta: -25 LOC; -1 module-level member; 0 types.

# 19. Remove the orphaned tensor import

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:169`
```csharp
using System.Numerics.Tensors;
```

To:
```csharp
// System.Numerics.Tensors using directive DELETED
```

Why: No tensor primitive remains after the unused span surfaces are removed.

Change: Remove the import and the package from this section's package list.

Delta: -1 LOC; 0 module-level members or types.

# 20. Remove stringly validation-detail assembly

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:522`
```csharp
Seq<string> invalid = toSeq<(bool Valid, string Name)>([
    (double.IsFinite(latitudeDeg) && latitudeDeg is >= -90.0 and <= 90.0, nameof(LatitudeDeg)),
    (double.IsFinite(longitudeDeg), nameof(LongitudeDeg)),
    (double.IsFinite(elevationM) && elevationM is > -500.0 and <= 10000.0, nameof(ElevationM)),
]).Choose(static clause => clause.Valid ? Option<string>.None : Some(clause.Name));
validationError = invalid.IsEmpty ? null
    : ValidationError.Create($"{nameof(SolarSite)}: invalid {string.Join(", ", invalid)}");
```

To:
```csharp
validationError = double.IsFinite(latitudeDeg) && latitudeDeg is >= -90.0 and <= 90.0
    && double.IsFinite(longitudeDeg)
    && double.IsFinite(elevationM) && elevationM is > -500.0 and <= 10000.0
        ? null : ValidationError.Create($"{nameof(SolarSite)} is invalid.");
```

Why: The Thinktecture hook returns one `ValidationError`; collecting property names into a `Seq<string>` does not create matchable independent failures, and no consumer parses the rendered list.

Change: Keep the same predicates and longitude normalization, but assign the generated verdict directly.

Delta: -3 LOC; 0 module-level members or types.

# 21. Delete the derived offset-hours property

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:517`
```csharp
public double OffsetHours => StandardOffset.Seconds / (double)NodaConstants.SecondsPerHour;
```

To:
```csharp
// SolarSite.OffsetHours DELETED
```

Why: This is a direct unit conversion from the public `Offset` and stores no independent fact.

Change: Derive hours only at host projections requiring a scalar.

Delta: -1 LOC; -1 module-level member; 0 types.

Ripples: `libs/dotnet/Rasm.Compute/.planning/Analysis/assessment.md` — derive the fingerprint scalar from `StandardOffset.Seconds`; `libs/dotnet/Rasm.Rhino/.planning/Objects/lights.md` — derive `TimeZoneHours`; `libs/dotnet/Rasm.Rhino/.planning/Render/settings.md` — derive one local hour value for both range clauses.

# 22. Delete the derived zenith property

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:533`
```csharp
public double ZenithDeg => 90.0 - AltitudeDeg;
```

To:
```csharp
// SunPosition.ZenithDeg DELETED
```

Why: Zenith is the direct complement of public altitude.

Change: Derive one local complement where daylight formulas need it.

Delta: -1 LOC; -1 module-level member; 0 types.

Ripples: `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md` — derive and reuse `90.0 - sun.AltitudeDeg`.

# 23. Delete the derived horizon predicate

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:534`
```csharp
public bool AboveHorizon => AltitudeDeg > 0.0;
```

To:
```csharp
// SunPosition.AboveHorizon DELETED
```

Why: The comparison owns no twilight or horizon policy; each filtering consumer should state its threshold.

Change: Replace each use with the consumer's explicit altitude comparison.

Delta: -1 LOC; -1 module-level member; 0 types.

Ripples: `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md` and `libs/dotnet/Rasm.AppUi/.planning/Charts/climate.md` — replace the horizon filters.

# 24. Delete the unused direction projection

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:536`
```csharp
public Vector3d Direction {
    get {
        double alt = AltitudeDeg * Math.PI / 180.0, az = AzimuthDeg * Math.PI / 180.0;
        return new Vector3d(Math.Cos(alt) * Math.Sin(az), Math.Cos(alt) * Math.Cos(az), Math.Sin(alt));
    }
}
```

To:
```csharp
// SunPosition.Direction DELETED
```

Why: No consumer reads this projection, while world-frame consumers already apply their own axis and sign conventions to the two angles.

Change: Delete `Direction` and the claimed bijection; retain `OfDirection` as the used, admitted Rhino survey-frame ingress.

Delta: -6 LOC; -1 module-level member; 0 types.

Ripples: `libs/dotnet/Rasm/.api/api-rhino.md` — remove the `Direction` projection claim while retaining `OfDirection`.

# 25. Delete the derivable sun-path wrapper

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:598`
```csharp
public static Seq<(Instant Instant, SunPosition Sun)> SunPath(SolarSite site, Instant midnight, Duration step, Dimension samples) =>
    from i in toSeq(Enumerable.Range(0, samples.Value))
    let at = midnight + (step * i)
    select (at, At(site, at));
```

To:
```csharp
// SolarPosition.SunPath DELETED
```

Why: This is a range projection over `At`; civil-day resolution and sample cadence already belong to its schedule-owning callers.

Change: Delete `SunPath` and remove `Seq`, `Duration`, and the second ephemeris entry from this section's contract.

Delta: -4 LOC; -1 module-level member; 0 types.

Ripples: `libs/dotnet/Rasm.AppUi/.planning/Analysis/context.md` — map its admitted count and step; `libs/dotnet/Rasm.AppUi/.planning/Charts/climate.md` — map each design-day schedule; `libs/dotnet/Rasm.AppUi/.planning/Render/pathtrace.md` — project inside `SunStudy.Sweep`; `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md` — project at the daylight schedule owner.

# 26. Collapse the ephemeris facade into its result owner

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:554`
```csharp
public static class SolarPosition {
    public static SunPosition At(SolarSite site, Instant instant) {
```

To:
```csharp
public readonly record struct SunPosition(double AzimuthDeg, double AltitudeDeg) {
    public static SunPosition At(SolarSite site, Instant instant) {
```

Why: After `SunPath` is removed, `SolarPosition` is a static shell around one factory returning `SunPosition`.

Change: Move `At` into `SunPosition`, delete the class wrapper, and qualify every call as `SunPosition.At`.

Delta: -2 LOC; -1 module-level type; 0 members.

Ripples: `libs/dotnet/Rasm.AppUi/.planning/Analysis/context.md`, `libs/dotnet/Rasm.AppUi/.planning/Charts/climate.md`, `libs/dotnet/Rasm.AppUi/.planning/Render/pathtrace.md`, `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md`, `libs/dotnet/Rasm.Materials/.planning/Appearance/environment.md`, and `libs/dotnet/Rasm.Rhino/.planning/Render/settings.md` — replace `SolarPosition.At`; `libs/dotnet/.planning/RULINGS.md` and `libs/dotnet/Rasm.Materials/RULINGS.md` — update the owner name without an alias.

# 27. Use MathNet polynomial evaluation directly

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:558`
```csharp
static double Polynomial(double t, params ReadOnlySpan<double> coefficients) {
    double value = 0.0;
    for (int i = coefficients.Length - 1; i >= 0; i--) { value = coefficients[i] + (t * value); }
    return value;
}
```

To:
```csharp
// SunPosition.At.Polynomial DELETED
```

Why: The admitted MathNet package already publishes `Polynomial.Evaluate`; the local Horner loop reimplements it.

Change: Replace every local call with `MathNet.Numerics.Polynomial.Evaluate(t, coefficients)`.

Delta: -5 LOC; 0 module-level members or types; -1 local function.

# 28. Delete the angle-conversion constant

From:
`libs/dotnet/Rasm/.planning/Numerics/calculus.md:555`
```csharp
private const double Radians = Math.PI / 180.0;
```

To:
```csharp
// SunPosition.Radians DELETED
```

Why: `Radians` is a vague rename whose multiplication direction must be inferred; MathNet owns both explicit angle conversions.

Change: Replace degree-to-radian products with `MathNet.Numerics.Trig.DegreeToRadian`, radian-to-degree quotients with `MathNet.Numerics.Trig.RadianToDegree`, and equation-of-time conversion with `4.0 * Trig.RadianToDegree(value)`.

Delta: -1 LOC; -1 module-level member; 0 types.
