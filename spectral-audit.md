# 1. Remove redundant and inapplicable descriptor distances

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:71-76`
```csharp
public static readonly DescriptorDistance Hamming = new(compute: static (a, b) => MathNet.Numerics.Distance.Hamming(a.Raw, b.Raw));
public static readonly DescriptorDistance Jaccard = new(compute: static (a, b) => MathNet.Numerics.Distance.Jaccard(a.Raw, b.Raw));
public static readonly DescriptorDistance MeanAbsolute = new(compute: static (a, b) => MathNet.Numerics.Distance.MAE(a.Dense, b.Dense));
public static readonly DescriptorDistance MeanSquared = new(compute: static (a, b) => MathNet.Numerics.Distance.MSE(a.Dense, b.Dense));
public static readonly DescriptorDistance SumAbsolute = new(compute: static (a, b) => MathNet.Numerics.Distance.SAD(a.Dense, b.Dense));
public static readonly DescriptorDistance SumSquared = new(compute: static (a, b) => MathNet.Numerics.Distance.SSD(a.Dense, b.Dense));
```

To:
```csharp
// DescriptorDistance.Hamming/DescriptorDistance.Jaccard/DescriptorDistance.MeanAbsolute/DescriptorDistance.MeanSquared/DescriptorDistance.SumAbsolute/DescriptorDistance.SumSquared DELETED
```

Why: Hamming and Jaccard encode exact-value or set membership and are not continuous spectral-descriptor metrics. `SAD` equals the retained Manhattan distance, while `MAE`, `MSE`, and `SSD` are fixed count or square transforms of retained L1 or L2 distances and therefore add no ranking capability.

Change: Delete the six rows and narrow the filter-algebra owner, package, and growth prose to the retained continuous metrics.

Delta: −6 LOC; −6 smart-enum members; 0 types.

# 2. Delete the dual-representation distance wrapper

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:78,81-84`
```csharp
[UseDelegateFromConstructor] internal partial double Compute(DistanceOperand a, DistanceOperand b);
[StructLayout(LayoutKind.Auto)]
internal readonly record struct DistanceOperand(double[] Raw) {
    internal MathNet.Numerics.LinearAlgebra.Vector<double> Dense { get; } = MathNet.Numerics.LinearAlgebra.CreateVector.DenseOfArray(Raw);
}
```

To:
```csharp
[UseDelegateFromConstructor] internal partial double Compute(
    (double[] Raw, MathNet.Numerics.LinearAlgebra.Vector<double> Dense) a,
    (double[] Raw, MathNet.Numerics.LinearAlgebra.Vector<double> Dense) b);
// DistanceOperand DELETED
```

Why: `DistanceOperand` owns no invariant and only names a method-local pair of representations. A named tuple preserves one-time MathNet lifting without a module type or a four-argument delegate.

Change: Change the delegate operands to named tuples, construct each raw-array/vector pair once in `RankNormalized`, and delete `DistanceOperand`.

Delta: −2 LOC; −1 internal type; −2 record members.

# 3. Seat z-score behavior directly on its policy row

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:43,53-60`
```csharp
public static readonly DescriptorNormalization ZScore = new(apply: static values => Centered(values: values));
private static Option<double[]> Centered(double[] values) {
    double sigma = TensorPrimitives.StdDev<double>(values);
    if (!double.IsFinite(sigma) || sigma <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
    double[] destination = new double[values.Length];
    TensorPrimitives.Subtract<double>(values, TensorPrimitives.Average<double>(values), destination);
    TensorPrimitives.Divide<double>(destination, sigma, destination);
    return Some(destination);
}
```

To:
```csharp
public static readonly DescriptorNormalization ZScore = new(apply: static values => {
    double sigma = TensorPrimitives.StdDev<double>(values);
    if (!double.IsFinite(sigma) || sigma <= EpsilonPolicy.SqrtEpsilon) return Option<double[]>.None;
    double[] destination = new double[values.Length];
    TensorPrimitives.Subtract<double>(values, TensorPrimitives.Average<double>(values), destination);
    TensorPrimitives.Divide<double>(destination, sigma, destination);
    return Some(destination);
});
```

Why: `Centered` has one caller and owns neither a reusable invariant nor a second policy; the smart-enum row is already the behavior owner.

Change: Inline the vectorized z-score body into the `ZScore` constructor delegate and delete `Centered`.

Delta: −1 LOC; −1 private method; 0 types.

# 4. Admit finite transfer parameters at filter construction

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:88,90,93,97,105,107,112,118`
```csharp
public sealed record ExponentialCase(double Rate) : SpectralFilter;
public sealed record PowerCase(double Exponent) : SpectralFilter;
public static SpectralFilter Exponential(double rate) => rate == 0.0 ? Identity : new ExponentialCase(Rate: rate);
public static SpectralFilter Power(double exponent) => exponent == 0.0 ? Identity : new PowerCase(Exponent: exponent);
exponentialCase: static (o, c) => o is ExponentialCase b ? Some(Exponential(rate: c.Rate + b.Rate)) : Option<SpectralFilter>.None,
powerCase: static (o, c) => o is PowerCase b ? Some(Power(exponent: c.Exponent + b.Exponent)) : Option<SpectralFilter>.None,
exponentialCase: static (lambda, c) => Math.Exp(d: c.Rate * lambda),
powerCase: static (lambda, c) => lambda > EpsilonPolicy.SqrtEpsilon ? Math.Pow(x: lambda, y: c.Exponent) : 0.0,
```

To:
```csharp
public sealed record ExponentialCase(Scalar Rate) : SpectralFilter;
public sealed record PowerCase(Scalar Exponent) : SpectralFilter;
public static SpectralFilter Exponential(Scalar rate) => rate.To() == 0.0 ? Identity : new ExponentialCase(Rate: rate);
public static SpectralFilter Power(Scalar exponent) => exponent.To() == 0.0 ? Identity : new PowerCase(Exponent: exponent);
exponentialCase: static (o, c) => o is ExponentialCase b ? Scalar.From(c.Rate.To() + b.Rate.To()).ToOption().Map(Exponential) : Option<SpectralFilter>.None,
powerCase: static (o, c) => o is PowerCase b ? Scalar.From(c.Exponent.To() + b.Exponent.To()).ToOption().Map(Power) : Option<SpectralFilter>.None,
exponentialCase: static (lambda, c) => Math.Exp(d: c.Rate.To() * lambda),
powerCase: static (lambda, c) => lambda > EpsilonPolicy.SqrtEpsilon ? Math.Pow(x: lambda, y: c.Exponent.To()) : 0.0,
```

Why: Raw doubles allow NaN and infinities into supposedly admitted filter cases, postponing refusal until evaluation. The existing finite `Scalar` owner makes invalid parameters unrepresentable, and `Scalar.From(...).ToOption()` keeps overflow during composition on the existing partial-composition carrier.

Change: Replace both signed raw parameter fields and factory arguments with `Scalar`, compose sums through `Scalar.From`, and project with `To()` only inside the numeric weight kernel.

Delta: 0 LOC; 0 members; 0 types; removes two invalid constructor domains and the non-finite composed-filter state.

Ripples: `libs/dotnet/Rasm.Compute/.planning/Tensor/blas.md:571` must spell illustrative power and exponential constructions with admitted `Scalar` values; no consumer fence currently constructs either parameterized case.

# 5. Delete derivable transfer-function factories

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:94-96,98-100`
```csharp
public static SpectralFilter Heat(PositiveMagnitude time) => Exponential(rate: -time.Value);
public static SpectralFilter Wave(PositiveMagnitude energy, PositiveMagnitude bandwidth) => new WaveCase(Energy: energy, Bandwidth: bandwidth);
public static SpectralFilter Diffusion(PositiveMagnitude time) => Exponential(rate: -2.0 * time.Value);
public static SpectralFilter Amplify(PositiveMagnitude rate) => Exponential(rate: rate.Value);
public static SpectralFilter Biharmonic => Power(exponent: -2.0);
public static SpectralFilter CommuteTime => Power(exponent: -1.0);
```

To:
```csharp
// SpectralFilter.Heat/SpectralFilter.Wave/SpectralFilter.Diffusion/SpectralFilter.Amplify/SpectralFilter.Biharmonic/SpectralFilter.CommuteTime DELETED
```

Why: Five members are one-expression aliases of `Exponential` or `Power`, and `Wave` only forwards two already-admitted values into its public case. None owns admission or behavior.

Change: Retain `Exponential`, `Power`, and `Identity` because they canonicalize zero; construct `WaveCase` directly; remove all six forwarding members from owner, case, entry, and growth prose.

Delta: −6 LOC; −6 public members; 0 types.

Ripples: `libs/dotnet/Rasm.Compute/.planning/Tensor/blas.md:571` must describe heat, amplification, inverse, and inverse-square filters as direct `Exponential` or `Power` values and wave filters as direct `WaveCase` values; no consumer fence calls a deleted factory.

# 6. Remove unrelated global tolerances from filter weights

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:113-118`
```csharp
waveCase: static (lambda, c) => lambda < EpsilonPolicy.SqrtEpsilon
    ? 0.0
    : ((Math.Log(d: c.Energy.Value) - Math.Log(d: lambda)) / Math.Max(val1: c.Bandwidth.Value, val2: EpsilonPolicy.ZeroTolerance)) switch {
        double ratio => Math.Exp(d: -0.5 * ratio * ratio),
    },
powerCase: static (lambda, c) => lambda > EpsilonPolicy.SqrtEpsilon ? Math.Pow(x: lambda, y: c.Exponent.To()) : 0.0,
```

To:
```csharp
waveCase: static (lambda, c) => lambda <= 0.0
    ? 0.0
    : ((Math.Log(d: c.Energy.Value) - Math.Log(d: lambda)) / c.Bandwidth.Value) switch {
        double ratio => Math.Exp(d: -0.5 * ratio * ratio),
    },
powerCase: static (lambda, c) => lambda > 0.0 ? Math.Pow(x: lambda, y: c.Exponent.To()) : 0.0,
```

Why: `Weight` is a transfer function, not the owner of spectrum-scale zero classification. The evaluation path already classifies zero modes with `SpectralBasis.ZeroBand`, `PositiveMagnitude` already makes zero bandwidth unrepresentable, and the tensor consumer owns its explicit `zeroFloor`.

Change: Make the logarithmic and negative-power domains depend only on mathematical positivity and divide by the admitted bandwidth directly.

Delta: −2 LOC; 0 members; 0 types; removes two duplicated tolerance policies.

Ripples: `libs/dotnet/Rasm.Compute/.planning/Tensor/blas.md:568-573` retains its explicit `zeroFloor` admission and receives no hidden absolute cutoff from `SpectralFilter.Weight`.

# 7. Encode operator assembly evidence as explicit cases

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:152-161`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralAssembly(
    int VertexCount, int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
    bool FlippedIntrinsicLifted, int MatrixRows, int MatrixCols, int NonZeros,
    int PositiveStar0Count, int PositiveStar1Count, int PositiveStar2Count,
    double BoundaryCompositionResidual, Option<int> Genus, bool EdgeConnection,
    Option<int> BoundaryEdgeCount = default, int BoundaryComponentCount = 0, Option<int> NonManifoldEdgeCount = default,
    Option<int> EulerCharacteristic = default, Option<int> PositiveMassCount = default,
    Option<(double Residual, double Tolerance)> Symmetry = default, Option<int> FactorNonZeros = default,
    Option<double> BoundaryCompositionTolerance = default) : IValidityEvidence {
```

To:
```csharp
[Thinktecture.Union(ConversionFromValue = Thinktecture.ConversionOperatorsGeneration.None)]
public abstract partial record DiscreteOperatorAssembly : IValidityEvidence {
    public sealed record ExteriorCalculusCase(
        int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
        double BoundaryCompositionResidual, double BoundaryCompositionTolerance,
        Option<int> Genus, int BoundaryComponentCount) : DiscreteOperatorAssembly;
    public sealed record ConnectionLaplacianCase(
        int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
        bool FlippedIntrinsicLifted, double SymmetryResidual, double SymmetryTolerance,
        int PositiveMassCount) : DiscreteOperatorAssembly;
    private DiscreteOperatorAssembly() { }
```

Why: `EdgeConnection` selects disjoint payloads while optional fields simulate the cases. Matrix dimensions, nonzeros, star counts, and topology diagnostics already belong to the paired operators, `DiscreteCalculus`, or `Topology`; `FactorNonZeros` is never populated. The new name states that this is construction evidence for two different discrete operators rather than a spectrum.

Change: Replace the flag record with generated `ExteriorCalculusCase` and `ConnectionLaplacianCase` payloads, retain only irreducible construction measurements, and delete all mirrored and never-populated fields.

Delta: +3 LOC; −7 record fields; +2 nested case types; 0 module-level types.

Ripples: `libs/dotnet/Rasm/.planning/Meshing/dec.md:15-22,177-184,257-298,407-411` must rename the carrier and construct the matching case; `libs/dotnet/Rasm/.planning/Meshing/mesh.md:514,568` must consume `ExteriorCalculusCase` and `ConnectionLaplacianCase`; `libs/dotnet/Rasm/.planning/Processing/reconstruct.md:271` must narrow its optional assembly input to `ConnectionLaplacianCase`.

# 8. Dispatch assembly validity over the generated cases

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:162-170,177-184`
```csharp
public bool IsValid => ValidityClaim.All(
    VertexCount >= 0 && EdgeCount >= 0 && FaceCount >= 0 && MatrixRows >= 0 && MatrixCols >= 0 && NonZeros >= 0,
    ValidityClaim.Nonnegative(value: BoundaryCompositionResidual),
    AdmittedFaceCount >= 0 && SkippedDegenerateFaces >= 0 && SkippedMissingEdges >= 0
        && (long)AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount,
    Symmetry is not { IsSome: true, Case: (double Residual, double Tolerance) measured } || (ValidityClaim.Nonnegative(measured.Residual) && ValidityClaim.Positive(measured.Tolerance) && measured.Residual <= measured.Tolerance),
    BoundaryCompositionTolerance is not { IsSome: true, Case: double tolerance } || (ValidityClaim.Positive(tolerance) && BoundaryCompositionResidual <= tolerance),
    FactorNonZeros.Map(static (int value) => value > 0).IfNone(noneValue: true),
    !FlippedIntrinsicLifted || EdgeConnection,
    BoundaryComponentCount >= 0,
    Genus is not { IsSome: true, Case: int heldGenus } || heldGenus >= 0,
    EdgeConnection
        ? MatrixRows == (long)EdgeCount * 2L && MatrixCols == MatrixRows
          && (PositiveMassCount is not { IsSome: true, Case: int positiveMass } || (positiveMass >= 0 && positiveMass <= EdgeCount))
        : PositiveStar0Count >= 0 && PositiveStar0Count <= VertexCount
          && PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount
          && PositiveStar2Count >= 0 && PositiveStar2Count <= FaceCount);
```

To:
```csharp
public bool IsValid => Switch(
    exteriorCalculusCase: static c => ValidityClaim.All(
        c.FaceCount >= 0 && c.AdmittedFaceCount >= 0 && c.SkippedDegenerateFaces >= 0 && c.SkippedMissingEdges >= 0,
        (long)c.AdmittedFaceCount + c.SkippedDegenerateFaces + c.SkippedMissingEdges <= c.FaceCount,
        ValidityClaim.Nonnegative(c.BoundaryCompositionResidual),
        ValidityClaim.Positive(c.BoundaryCompositionTolerance),
        c.BoundaryCompositionResidual <= c.BoundaryCompositionTolerance,
        c.BoundaryComponentCount >= 0,
        c.Genus.Map(static genus => genus >= 0).IfNone(true)),
    connectionLaplacianCase: static c => ValidityClaim.All(
        c.EdgeCount >= 0 && c.FaceCount >= 0 && c.AdmittedFaceCount >= 0 && c.SkippedDegenerateFaces >= 0 && c.SkippedMissingEdges >= 0,
        (long)c.AdmittedFaceCount + c.SkippedDegenerateFaces + c.SkippedMissingEdges <= c.FaceCount,
        ValidityClaim.Nonnegative(c.SymmetryResidual), ValidityClaim.Positive(c.SymmetryTolerance),
        c.SymmetryResidual <= c.SymmetryTolerance,
        c.PositiveMassCount >= 0 && c.PositiveMassCount <= c.EdgeCount));
```

Why: Each validity arm should read only evidence its producer supplies. Total generated dispatch removes the option probes, flag ternary, and half-populated mixed states.

Change: Replace the flat conjunct list with `DiscreteOperatorAssembly.Switch` and validate each case against only its own payload.

Delta: −3 LOC; 0 members; 0 types; removes 7 option- or flag-selected validity branches.

# 9. Require exterior-calculus assembly evidence in `DiscreteCalculus`

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:236`
```csharp
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, SpectralAssembly Assembly, Evidence<SignpostTransport> Transport, Option<HarmonicOneFormBasis> Harmonic = default) : IValidityEvidence {
```

To:
```csharp
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, DiscreteOperatorAssembly.ExteriorCalculusCase Assembly, Evidence<SignpostTransport> Transport, Option<HarmonicOneFormBasis> Harmonic = default) : IValidityEvidence {
```

Why: A discrete calculus can never carry connection-Laplacian evidence. Typing the field to the generated case removes that invalid state without a runtime case test and leaves genus and boundary-component derivation direct.

Change: Narrow the `Assembly` field to `DiscreteOperatorAssembly.ExteriorCalculusCase` and update the `DecAssembly.Operators` construction accordingly.

Delta: 0 LOC; 0 members; 0 types; removes one cross-case construction path.

Ripples: `libs/dotnet/Rasm/.planning/Meshing/dec.md:177-180` must return the exterior-calculus case directly when constructing `DiscreteCalculus`.

# 10. Remove duplicated inputs and provider payload from the harmonic census

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:188-193`
```csharp
public readonly record struct HarmonicCensus(
    Option<int> Genus, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double EpsRank, double SpectralRadius,
    double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual,
    double Star1OrthonormalResidual, int PositiveStar1Count, EigenSolution<double, Arr<double>> Eigen,
    int BoundaryComponentCount = 0) : IValidityEvidence {
```

To:
```csharp
public readonly record struct HarmonicCensus(
    int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double SpectralRadius,
    double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual,
    double Star1OrthonormalResidual, int PositiveStar1Count) : IValidityEvidence {
```

Why: Genus and boundary components already live on the exterior-calculus assembly, and its owner validates the harmonic dimension. `EpsRank` is only the policy input used to derive the retained SVD threshold, while the provider `EigenSolution` is revalidated after its rank, nullity, interval, and residual evidence have already been projected.

Change: Delete the four fields and stop supplying them when constructing the census.

Delta: −1 LOC; −4 record members; 0 types.

Ripples: `libs/dotnet/Rasm/.planning/Meshing/dec.md:192` must stop supplying topology, rank-policy input, and `EigenSolution` when `HarmonicForms` constructs the census; `libs/dotnet/Rasm/.planning/Meshing/dec.md:55-67` and `libs/dotnet/Rasm/.planning/Processing/extract.md:398-400` retain the smaller census unchanged.

# 11. Validate only intrinsic harmonic-census evidence

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:194-218`
```csharp
private const double ResidualSlack = 1.0e3;
public bool IsValid {
    get {
        int boundaryComponentCount = BoundaryComponentCount;
        long expected = Genus.Map((int genus) => (2L * genus) + Math.Max(0L, (long)boundaryComponentCount - 1L)).IfNone(0L);
        double residualTolerance = Math.Max(val1: SvdTolerance, val2: EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: SpectralRadius)) * ResidualSlack;
        return ValidityClaim.All(
            EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0 && boundaryComponentCount >= 0,
            (long)Rank + Nullity == EdgeCount,
            BasisCount == expected,
            Nullity >= expected,
            PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount,
            Genus.Map(static (int genus) => genus >= 0).IfNone(noneValue: true),
            ValidityClaim.Positive(value: SvdTolerance),
            ValidityClaim.Positive(value: EpsRank),
            ValidityClaim.Nonnegative(value: SpectralRadius),
            ValidityClaim.Finite(value: residualTolerance),
            SvdTolerance <= (EpsRank * Math.Max(val1: 1.0, val2: SpectralRadius)) + EpsilonPolicy.SqrtEpsilon,
            ValidityClaim.Finite(value: MinNullEigenvalue) && MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon,
            ValidityClaim.Finite(value: MaxNullEigenvalue) && MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon,
            ValidityClaim.Nonnegative(value: MaxClosedResidual) && MaxClosedResidual <= residualTolerance,
            ValidityClaim.Nonnegative(value: MaxCoClosedResidual) && MaxCoClosedResidual <= residualTolerance,
            ValidityClaim.Nonnegative(value: Star1OrthonormalResidual) && Star1OrthonormalResidual <= residualTolerance,
            ValidityClaim.Evidence(evidence: Some(Eigen)));
    }
}
```

To:
```csharp
private const double ResidualSlack = 1.0e3;
public bool IsValid {
    get {
        double residualTolerance = Math.Max(SvdTolerance, EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, SpectralRadius)) * ResidualSlack;
        return ValidityClaim.All(
            EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0,
            (long)Rank + Nullity == EdgeCount,
            BasisCount <= Nullity,
            PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount,
            ValidityClaim.Positive(SvdTolerance),
            ValidityClaim.Nonnegative(SpectralRadius),
            ValidityClaim.Finite(residualTolerance),
            ValidityClaim.Finite(MinNullEigenvalue) && MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon,
            ValidityClaim.Finite(MaxNullEigenvalue) && MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon,
            ValidityClaim.Nonnegative(MaxClosedResidual) && MaxClosedResidual <= residualTolerance,
            ValidityClaim.Nonnegative(MaxCoClosedResidual) && MaxCoClosedResidual <= residualTolerance,
            ValidityClaim.Nonnegative(Star1OrthonormalResidual) && Star1OrthonormalResidual <= residualTolerance);
    }
}
```

Why: Expected harmonic dimension depends on topology owned by `DiscreteCalculus`, and rank-policy provenance belongs to the assembly operation. The census should validate only its retained measurements and the intrinsic relation that a stored basis cannot exceed the measured nullity.

Change: Remove the topology-derived equality, policy-input comparison, and nested provider validation; retain count identities, value domains, spectral interval, and residual bounds.

Delta: −7 LOC; 0 members; 0 types; removes five duplicated validity conditions.

# 12. Delete the unused generic projection roster

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:257-267`
```csharp
internal Fin<TOut> Project<TOut>() {
    DiscreteCalculus self = this;
    return ResultProjection.Rows<DiscreteCalculus, TOut>(self: self,
        ProjectionRow.Of<SpectralAssembly>(() => Fin.Succ(self.Assembly)),
        ProjectionRow.Of<SignpostTransport>(() => self.Transport.Switch(
            measured: static row => Fin.Succ(row.Value),
            refused: static row => Fin.Fail<SignpostTransport>(row.Cause),
            absent: _ => Fin.Fail<SignpostTransport>(new KernelFault.InvalidResult()))),
        ProjectionRow.Of<HarmonicOneFormBasis>(() => self.Harmonic.ToFin(new KernelFault.InvalidResult())),
        ProjectionRow.Of<HarmonicCensus>(() => self.Harmonic.Map(static basis => basis.Census).ToFin(new KernelFault.InvalidResult())));
}
```

To:
```csharp
// DiscreteCalculus.Project DELETED
```

Why: No repository consumer calls the method; every projected value is already a named field or one `Option`/`Evidence` fold away. The type-directed roster adds a generic dispatch surface over direct data access.

Change: Delete `Project<TOut>` and remove projection claims from the DEC-carrier prose.

Delta: −11 LOC; −1 internal method; 0 types.

# 13. Delete the unused and non-total basis truncation helper

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:287-288`
```csharp
public SpectralBasis Truncate(int k) =>
    k <= 0 || k >= Eigenvalues.Count ? this : new SpectralBasis(Eigenvalues: new Arr<double>([.. Eigenvalues.AsIterable().Take(k)]), Eigenvectors: new Arr<Arr<double>>([.. Eigenvectors.AsIterable().Take(k)]));
```

To:
```csharp
// SpectralBasis.Truncate DELETED
```

Why: No consumer calls the member, and `k <= 0` silently returns the full basis rather than refusing. Basis width is already admitted as `Dimension` and applied at the eigenpair construction boundary.

Change: Delete `Truncate` and remove it from the DEC-carrier owner prose.

Delta: −2 LOC; −1 public member; 0 types.

# 14. Remove duplicate crop-count admission

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:321-323`
```csharp
public bool IsValid => ValidityClaim.All(
    Normalization is not null,
    CropCount.Map(static (Dimension count) => count.Value > 0).IfNone(noneValue: true));
```

To:
```csharp
public bool IsValid => Normalization is not null;
```

Why: `Dimension` already admits positive counts through its generated factory; rechecking its key duplicates value-object admission and adds no aggregate invariant.

Change: Retain only the smart-enum presence check required to reject a default-constructed policy.

Delta: −2 LOC; 0 members; 0 types.

# 15. Delete the unused wave-profile payload

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:327-336`
```csharp
public readonly record struct WaveProfile(
    double FirstNonZeroScale, int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum,
    double MinLogEigenvalue, double MaxLogEigenvalue) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        NonZeroEigenpairCount > 0,
        ValidityClaim.Positive(value: RawWeightSum),
        Math.Abs(value: NormalizedWeightSum - 1.0) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1, val2: NonZeroEigenpairCount),
        ValidityClaim.Positive(value: FirstNonZeroScale),
        ValidityClaim.Finite(value: MinLogEigenvalue),
        ValidityClaim.Finite(value: MaxLogEigenvalue) && MinLogEigenvalue <= MaxLogEigenvalue);
}
```

To:
```csharp
// WaveProfile DELETED
```

Why: No consumer reads any wave diagnostic. The only operational value is the normalized weight array, while the six-field profile causes an extra log-eigenvalue array, reductions, a public type, and option plumbing through evaluation.

Change: Return only `double[]` from `WeightsOf` and `WaveWeightsOf`, validate finiteness and positive sum before normalization, remove `positiveLogs` and profile construction, and delete `WaveProfile`.

Delta: −20 LOC; −1 public type; −7 public members; removes one per-evaluation array allocation.

# 16. Keep only non-derivable descriptor provenance

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:340-350`
```csharp
public readonly record struct DescriptorProfile(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount,
    SpectralDescriptorPolicy Policy, int ZeroModeCount = 0, int CroppedEigenpairCount = 0, Option<WaveProfile> Wave = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Filter is not null && (Filter is SpectralFilter.WaveCase) == Wave.IsSome && VertexCount > 0 && EigenpairCount > 0,
        ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount && CroppedEigenpairCount > 0
            && CroppedEigenpairCount == Math.Min(
                Policy.CropCount.Map(static (Dimension count) => count.Value).IfNone(EigenpairCount),
                Policy.IncludeZeroModes ? EigenpairCount : EigenpairCount - ZeroModeCount)
            && (!Policy.NormalizeScale || ZeroModeCount < EigenpairCount)
            && (Wave is not { IsSome: true, Case: WaveProfile wave }
                || wave.NonZeroEigenpairCount == (Policy.IncludeZeroModes ? Math.Max(0, CroppedEigenpairCount - ZeroModeCount) : CroppedEigenpairCount)),
```

To:
```csharp
public readonly record struct DescriptorProfile(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount,
    SpectralDescriptorPolicy Policy, int ZeroModeCount = 0) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Filter is not null && VertexCount > 0 && EigenpairCount > 0,
        ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount
            && (Policy.IncludeZeroModes || ZeroModeCount < EigenpairCount)
            && (!Policy.NormalizeScale || ZeroModeCount < EigenpairCount),
```

Why: `CroppedEigenpairCount` is exactly the crop policy applied to the eligible eigenpair count, and `Wave` is deleted as unused diagnostics. Storing either permits redundant state to disagree with the policy and filter that derive it.

Change: Delete `CroppedEigenpairCount` and `Wave`, simplify validity to retained provenance and zero-mode relations, and stop passing both fields from `Evaluate`.

Delta: −4 LOC; −2 record members; 0 types.

# 17. Remove basis checks duplicated below the evaluation gate

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:382-390`
```csharp
int n = basis.VertexCount;
int[] sourceSet = sources.Map(static values => values.AsIterable().ToArray()).IfNone([]);
if (n == 0 || (sources.IsSome && sourceSet.Length == 0) || toSet(sourceSet).Count != sourceSet.Length
    || (sourceSet.Length > 0 && (TensorPrimitives.Min<int>(sourceSet) < 0 || TensorPrimitives.Max<int>(sourceSet) >= n))) {
    return Fin.Fail<SpectralDescriptor>(error: new KernelFault.InvalidInput());
}
if (!basis.Eigenvectors.ForAll(phi => phi.Count == n)) { return Fin.Fail<SpectralDescriptor>(error: new KernelFault.InvalidResult()); }
double zeroBand = basis.ZeroBand;
```

To:
```csharp
int n = basis.VertexCount;
int[] sourceSet = sources.Map(static values => values.AsIterable().ToArray()).IfNone([]);
if ((sources.IsSome && sourceSet.Length == 0) || toSet(sourceSet).Count != sourceSet.Length
    || (sourceSet.Length > 0 && (TensorPrimitives.Min<int>(sourceSet) < 0 || TensorPrimitives.Max<int>(sourceSet) >= n)))
    return Fin.Fail<SpectralDescriptor>(new KernelFault.InvalidInput());
double zeroBand = basis.ZeroBand;
```

Why: `SpectralFilter.Evaluate` proves `basis.IsValid` before entering the kernel, and that gate already proves a positive vertex count and equal finite eigenvector widths. Repeating those checks creates a second admission path.

Change: Keep only source-set admission in the evaluation body.

Delta: −3 LOC; 0 members; 0 types.

# 18. Return only ranking information the caller cannot derive

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:374-378`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRank(int Index, double Distance, SpectralDescriptor Descriptor);
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRanking(SpectralDescriptor Query, Seq<SpectralRank> Items, SpectralRankingPolicy Policy);
```

To:
```csharp
// SpectralRank/SpectralRanking DELETED
```

Why: The caller already owns the query, candidates, and policy; `SpectralRanking` echoes all three, and each `SpectralRank.Descriptor` is recoverable from its index. The irreducible result is the ordered index-distance sequence.

Change: Change `SpectralDescriptor.Rank`, `RankDescriptors`, and `RankNormalized` to return `Fin<Seq<(int Index, double Distance)>>`; construct, validate, and order those pairs directly, then delete both payload-only records.

Delta: −4 LOC; −2 public types; −6 generated record members.

# 19. Delete the redundant ranking-policy carrier

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:368-372`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, DescriptorDistance Distance) : IValidityEvidence {
    public static SpectralRankingPolicy Default => new(Descriptor: SpectralDescriptorPolicy.Raw, Distance: DescriptorDistance.Euclidean);
    public bool IsValid => Distance is not null && ValidityClaim.Evidence(evidence: Some(Descriptor));
}
```

To:
```csharp
// SpectralRankingPolicy DELETED
```

Why: The descriptor policy is already retained in each descriptor profile; accepting a second copy permits provenance to be overwritten during ranking. Normalization and distance are the only caller choices at this boundary and need no two-field carrier.

Change: Make `SpectralDescriptor.Rank` accept `DescriptorNormalization` and `DescriptorDistance` directly, thread those values through ranking, and delete `SpectralRankingPolicy`.

Delta: −4 LOC; −1 public type; −2 record fields and 2 public members.

# 20. Refuse ranking across incompatible descriptor provenance

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:443-445`
```csharp
!policy.IsValid || !query.IsValid || candidates.IsEmpty || !candidates.ForAll(static candidate => candidate.IsValid)
    ? Fin.Fail<SpectralRanking>(new KernelFault.InvalidInput())
```

To:
```csharp
!query.IsValid || normalization is null || distance is null || candidates.IsEmpty
    || !candidates.ForAll(candidate => candidate.IsValid
        && candidate.Profile.Filter.Equals(query.Profile.Filter)
        && (candidate.Profile.Policy with { Normalization = DescriptorNormalization.Raw })
            .Equals(query.Profile.Policy with { Normalization = DescriptorNormalization.Raw }))
    ? Fin.Fail<Seq<(int Index, double Distance)>>(new KernelFault.InvalidInput())
```

Why: A requested normalization cannot make descriptors comparable when their transfer function, scale normalization, zero-mode inclusion, or crop policy differs. The current path overwrites that provenance with a caller-supplied descriptor policy and can silently rank unlike signatures.

Change: Compare each candidate's filter and policy after normalizing only the normalization field to `Raw`; refuse mismatches before normalization or distance evaluation.

Delta: +4 LOC; 0 members; 0 types; removes four invalid cross-provenance ranking paths.

# 21. Move descriptor behavior to its values and delete the kernel shell

From: `libs/dotnet/Rasm/.planning/Numerics/spectral.md:381`
```csharp
internal static class SpectralKernel {
```

To:
```csharp
// SpectralKernel DELETED
```

Why: The class only receives one-call forwarding members from `SpectralFilter` and `SpectralDescriptor`; it owns no state, boundary, or independent result. Its remaining helpers are exclusive to one public operation except normalization behavior, which belongs to `DescriptorNormalization`.

Change: Move the evaluation expression onto `SpectralFilter.Evaluate` with accumulation and weight functions local to it; move normalization and ranking expressions onto `SpectralDescriptor` with ranking functions local to it; call `DescriptorNormalization.Apply` directly and delete `Rescaled`; delete `SpectralKernel`.

Delta: −2 LOC; −1 internal module-level type; −9 module-level methods, with five retained as operation-local functions.
