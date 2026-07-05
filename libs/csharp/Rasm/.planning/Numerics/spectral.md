# [RASM_NUMERICS_SPECTRAL]

The mesh-free spectral algebra of `Rasm.Numerics` — the discrete-exterior-calculus carrier layer and the spectral descriptor/filter algebra, owned WITHOUT any mesh coupling so the pure math floor is portable by inspection and the `Rasm.Compute` DDG-adjoint surface binds one clean seam. The page owns the five spectral policy vocabularies, `SpectralFilter` — the closed transfer-function `[Union]` (Heat/Wave/Biharmonic/Diffusion/CommuteTime/Identity) with the eigenvalue weight law and the partial-monoid `Compose` — `DiscreteCalculus`, the frozen-name DEC operator bundle (`D0`/`D1`/`Star0`/`Star1`/`Star2` plus optional transport and harmonic evidence — THE cross-package seam name), `SpectralBasis` the eigenpair carrier with `Truncate`, the assembly/harmonic receipt family (`SpectralAssemblyReceipt`, `HarmonicOneFormReceipt`, `HarmonicOneFormBasis`) with their scale-relative residual gates, and the descriptor algebra — `SpectralDescriptorPolicy`, `SpectralWaveReceipt`, `SpectralDescriptorReceipt`, `SpectralDescriptor` with `Normalize`/`Rank`, `SpectralRankingPolicy`/`SpectralRank`/`SpectralRanking`, and `SpectralKernel`, the dense-buffer descriptor evaluation (HKS/WKS-style filtered signatures, pairwise spectral distances), normalization (L1/L2/z-score via `TensorPrimitives`), and ranking (Euclidean/Manhattan/Cosine via `MathNet.Numerics.Distance`).

Everything mesh-BOUND departs: operator assembly (d0 incidence, d1 curl, star mass/cotan/area), Crouzeix-Raviart connection systems, trivial-connection holonomy, harmonic 1-form construction, Hodge decomposition solves, and mesh-side spectral basis computation are `Meshing/dec`'s — that page PRODUCES the carriers and receipts declared here, healing the mature Spectral↔Mesh receipt fracture by fixing declaration here and construction there. Consumers of this page never touch a `Mesh`.

## [01]-[INDEX]

- [02]-[FILTER_ALGEBRA]: five spectral policy vocabularies + `SpectralFilter` closed transfer-function union (weight law, partial-monoid `Compose`, `ApplyDetailed`).
- [03]-[DEC_CARRIERS]: `DiscreteCalculus` (frozen seam name) · `SpectralBasis` · `SpectralAssemblyReceipt` · `HarmonicOneFormReceipt` · `HarmonicOneFormBasis` — the DEC evidence layer `Meshing/dec` mints and `Rasm.Compute` consumes.
- [04]-[DESCRIPTOR_ALGEBRA]: `SpectralDescriptorPolicy` · `SpectralWaveReceipt` · `SpectralDescriptorReceipt` · `SpectralDescriptor` · `SpectralRankingPolicy`/`SpectralRank`/`SpectralRanking` + `SpectralKernel` evaluation/normalization/ranking.

## [02]-[FILTER_ALGEBRA]

- Owner: `SpectralAssemblyKind` (Dec/EdgeConnection — which operator family a receipt witnesses), `SpectralScaleNormalization` (Raw/FirstNonZeroEigenvalue — eigenvalue rescaling for cross-mesh comparability), `SpectralEnergyNormalization` (Raw/UnitL1/UnitL2/ZScore), `SpectralZeroModePolicy` (Keep/Drop), `SpectralDistanceKind` (Euclidean/Manhattan/Cosine — the distance IS the row's `[UseDelegateFromConstructor]` compute column over `MathNet.Numerics.Distance`) — the five `[SmartEnum<int>]` policy vocabularies; `SpectralFilter` the closed `[Union]` whose `Weight(eigenvalue)` IS the spectral transfer function: heat `e^(−tλ)`, wave the log-normal WKS band `e^(−½((ln e − ln λ)/σ)²)`, biharmonic `1/λ²`, diffusion `e^(−2tλ)`, commute-time `1/λ`, identity `1` — zero-protected where the function pole demands it — and whose `Compose` is a PARTIAL monoid: `Heat(t₁)∘Heat(t₂) = Heat(t₁+t₂)`, `Diffusion∘Diffusion` likewise, `Identity` the unit, every other pair `None` — composability is a property of the semigroup, not a runtime coincidence.
- Cases: `HeatCase(time)` · `WaveCase(energy, bandwidth)` · `BiharmonicCase` · `DiffusionCase(time)` · `CommuteTimeCase` · `IdentityCase` (6).
- Entry: `SpectralFilter.Heat(time)` / `Wave(energy, bandwidth)` / `Biharmonic` / `Diffusion(time)` / `CommuteTime` / `Identity` — parameters arrive as `PositiveMagnitude`, so a filter in hand is admitted; `ApplyDetailed(basis, sources, policy, key)` evaluates the filtered descriptor through `SpectralKernel`.
- Auto: `Weight` carries `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — it runs per eigenpair per vertex in the descriptor kernel's hot loop; the wave weight floors its bandwidth at `SpectralKernel.WaveBandwidthFloor` so a degenerate band never divides to infinity.
- Receipt: none at this layer — the filter is policy; evidence lands on the [04] descriptor receipts.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transfer function (Mexican-hat, band-pass) is one case + one `Weight` arm + at most one `Compose` pair — the descriptor kernel and every consumer are untouched; a new normalization or distance is one vocabulary row.
- Boundary: filters weight EIGENVALUES only — they never see a mesh, a basis matrix, or a vertex; the same filter value drives `Meshing/dec` heat scaffolds, `Processing/segment` HKS/WKS descriptors, and `Spatial/fields` spectral-distance cases, which is why it lives on this floor.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics.Tensors;
using Rasm.Domain;
using Rasm.Meshing;

namespace Rasm.Numerics;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SpectralAssemblyKind {
    public static readonly SpectralAssemblyKind Dec = new(key: 0);
    public static readonly SpectralAssemblyKind EdgeConnection = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralScaleNormalization {
    public static readonly SpectralScaleNormalization Raw = new(key: 0);
    public static readonly SpectralScaleNormalization FirstNonZeroEigenvalue = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralEnergyNormalization {
    public static readonly SpectralEnergyNormalization Raw = new(key: 0);
    public static readonly SpectralEnergyNormalization UnitL1 = new(key: 1);
    public static readonly SpectralEnergyNormalization UnitL2 = new(key: 2);
    public static readonly SpectralEnergyNormalization ZScore = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class SpectralZeroModePolicy {
    public static readonly SpectralZeroModePolicy Keep = new(key: 0);
    public static readonly SpectralZeroModePolicy Drop = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralDistanceKind {
    public static readonly SpectralDistanceKind Euclidean = new(key: 0, compute: static (a, b) => MathNet.Numerics.Distance.Euclidean(a, b));
    public static readonly SpectralDistanceKind Manhattan = new(key: 1, compute: static (a, b) => MathNet.Numerics.Distance.Manhattan(a, b));
    public static readonly SpectralDistanceKind Cosine = new(key: 2, compute: static (a, b) => MathNet.Numerics.Distance.Cosine(a, b));
    [UseDelegateFromConstructor] internal partial double Compute(double[] a, double[] b);
}

[Union]
public abstract partial record SpectralFilter {
    public sealed record HeatCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record WaveCase(PositiveMagnitude Energy, PositiveMagnitude Bandwidth) : SpectralFilter;
    public sealed record BiharmonicCase : SpectralFilter;
    public sealed record DiffusionCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record CommuteTimeCase : SpectralFilter;
    public sealed record IdentityCase : SpectralFilter;
    private SpectralFilter() { }
    public static SpectralFilter Heat(PositiveMagnitude time) => new HeatCase(Time: time);
    public static SpectralFilter Wave(PositiveMagnitude energy, PositiveMagnitude bandwidth) => new WaveCase(Energy: energy, Bandwidth: bandwidth);
    public static SpectralFilter Biharmonic => new BiharmonicCase();
    public static SpectralFilter Diffusion(PositiveMagnitude time) => new DiffusionCase(Time: time);
    public static SpectralFilter CommuteTime => new CommuteTimeCase();
    public static SpectralFilter Identity => new IdentityCase();
    // Partial monoid: composable pairs fuse, Identity is the unit, everything else is None by law.
    public Option<SpectralFilter> Compose(SpectralFilter other) =>
        (this, other) switch {
            (HeatCase a, HeatCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => Heat(time: time)),
            (DiffusionCase a, DiffusionCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => Diffusion(time: time)),
            (IdentityCase, _) when other is not null => Some(other),
            (_, IdentityCase) => Some(this),
            _ => Option<SpectralFilter>.None,
        };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double Weight(double eigenvalue) => Switch(
        state: eigenvalue,
        heatCase: static (lambda, c) => Math.Exp(d: -c.Time.Value * lambda),
        waveCase: static (lambda, c) => RawWaveWeight(eigenvalue: lambda, energy: c.Energy.Value, bandwidth: c.Bandwidth.Value),
        biharmonicCase: static (lambda, _) => lambda > EpsilonPolicy.SqrtEpsilon ? 1.0 / (lambda * lambda) : 0.0,
        diffusionCase: static (lambda, c) => Math.Exp(d: -2.0 * c.Time.Value * lambda),
        commuteTimeCase: static (lambda, _) => lambda > EpsilonPolicy.SqrtEpsilon ? 1.0 / lambda : 0.0,
        identityCase: static (_, _) => 1.0);
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, Op key) =>
        ApplyDetailed(basis: basis, sources: sources, policy: SpectralDescriptorPolicy.Raw, key: key);
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralDescriptorPolicy policy, Op key) =>
        Optional(this).ToFin(key.InvalidInput())
            .Bind(filter => guard(basis.IsValid, key.InvalidInput()).ToFin()
            .Bind(_ => SpectralDescriptorPolicy.Admit(policy: policy, key: key))
            .Bind(activePolicy => SpectralKernel.EvaluateFilteredDetailed(basis: basis, sources: sources, filter: filter, policy: activePolicy, key: key)));
    private static double RawWaveWeight(double eigenvalue, double energy, double bandwidth) =>
        eigenvalue < EpsilonPolicy.SqrtEpsilon ? 0.0 : ((Math.Log(d: energy) - Math.Log(d: eigenvalue)) / Math.Max(val1: bandwidth, val2: SpectralKernel.WaveBandwidthFloor)) switch { double ratio => Math.Exp(d: -0.5 * ratio * ratio) };
    private static Option<PositiveMagnitude> Positive(double value) =>
        PositiveMagnitude.TryCreate(value: value, obj: out PositiveMagnitude magnitude) ? Some(magnitude) : Option<PositiveMagnitude>.None;
}
```

## [03]-[DEC_CARRIERS]

- Owner: `DiscreteCalculus` — the FROZEN-NAME discrete-exterior-calculus operator bundle: `D0` (vertex→edge incidence, `SparseMatrix`), `D1` (edge→face curl), `Star0`/`Star1`/`Star2` (the diagonal Hodge stars: vertex mass, edge cotan weights, inverse face areas — `Arr<double>` diagonals), the `SpectralAssemblyReceipt`, and the optional `Transport` (signpost intrinsic-transport evidence, declared by `Meshing/mesh`) and `Harmonic` (1-form basis) slots; its `Project<TOut>` routes the evidence family through typed `ProjectionRow` rows. `SpectralBasis` the eigenpair carrier (`Eigenvalues` finite and nonnegative within the ONE scale-relative `ZeroBand` — `SqrtEpsilon` × spectral radius, so unit-scaled spectra never fail an absolute gate; the eigen mints deliver them ascending, the gate does not re-derive ordering — `Eigenvectors` uniform-length) with `Truncate(k)`; the SAME band classifies zero modes in the descriptor kernel — one threshold declaration, zero drift. `SpectralAssemblyReceipt` the assembly evidence — vertex/edge/face census, admitted/skipped-degenerate/missing-edge face counts, matrix shape and nnz, positive-star counts, the `∂∂ = 0` boundary-composition residual, genus and harmonic dimension, topology census (boundary edges/components, non-manifold edges, Euler characteristic), and for edge-connection assemblies the symmetry residual against its tolerance. `HarmonicOneFormReceipt` — the harmonic-basis evidence with the dimension law `dim = 2g + max(0, b−1)`, the rank/nullity partition of the edge space, SVD/eps-rank thresholds, nullspace eigenvalue window, and the closed/coclosed/`Star1`-orthonormal residuals gated SCALE-RELATIVE (the gate carries `max(1, spectralRadius)` and a dimensionless `1e3` rounding-accumulation slack — never a bare absolute). `HarmonicOneFormBasis` the forms + receipt pair.
- Entry: carriers are CONSTRUCTED by `Meshing/dec` (assembly) and `Meshing/mesh` (caching); this page owns their shape, validity law, and projection — consumers (`Rasm.Compute` adjoint surface, `Processing/geodesics`, `Processing/segment`, `Spatial/fields` spectral cases) read them from here.
- Auto: `DiscreteCalculus.IsValid` cross-couples the stars to the operator shapes (`Star0.Count == D0.Cols`, `Star1.Count == D0.Rows`, `Star2.Count == D1.Rows`), requires strictly positive vertex/face stars, and admits edge stars down to a scale-relative negative band (intrinsic cotan weights of near-degenerate triangles legitimately dip below zero within roundoff of the Star1 scale).
- Receipt: all three receipts spell the rails fold — `public bool IsValid => ValidityClaim.All(…)` with `IValidityEvidence` registration; the semantic gates preserve the mature couplings verbatim — the harmonic dimension law, the `Rank + Nullity == EdgeCount` partition, the residual-tolerance ladder.
- Packages: `matrix.md` owners (`SparseMatrix`, `EigenSolveReceipt`), LanguageExt.Core, Rasm.Domain (`IValidityEvidence` + `ValidityClaim`, `Op`).
- Growth: a new DEC operator (a primal-dual wedge, a vector-valued star) is one field + one validity coupling + one `ProjectionRow`; a new assembly witness is one receipt field.
- Boundary: `DiscreteCalculus` is the `Rasm.Compute` adjoint seam — the name, field set, and projection rows are the cross-package contract and stay stable; `SignpostTransportReceipt` is DECLARED by `Meshing/mesh` (the intrinsic-triangulation owner) and only carried here as optional evidence, so the DDG receipt family has exactly one declaration site per receipt with this page owning the mesh-free members.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralAssemblyReceipt(
    int VertexCount, int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges,
    bool FlippedIntrinsicRejected, int MatrixRows, int MatrixCols, int NonZeros,
    int PositiveStar0Count, int PositiveStar1Count, int PositiveStar2Count,
    double BoundaryCompositionResidual, Option<int> Genus, int HarmonicDimension, SpectralAssemblyKind Kind,
    int BoundaryEdgeCount = 0, int BoundaryComponentCount = 0, int NonManifoldEdgeCount = 0, int EulerCharacteristic = 0,
    bool TopologyEulerValidated = false, int ComponentCount = 1, int PositiveMassCount = 0,
    double SymmetryResidual = 0.0, double SymmetryTolerance = 0.0, Option<int> FactorNonZeros = default,
    double BoundaryCompositionTolerance = 0.0) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(VertexCount >= 0 && EdgeCount >= 0 && FaceCount >= 0 && MatrixRows >= 0 && MatrixCols >= 0 && NonZeros >= 0),
        ValidityClaim.Nonnegative(value: BoundaryCompositionResidual),
        ValidityClaim.Nonnegative(value: SymmetryResidual),
        ValidityClaim.Of(AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount),
        ValidityClaim.Of(SymmetryResidual <= SymmetryTolerance),
        ValidityClaim.Of(FactorNonZeros.Map(static value => value > 0).IfNone(noneValue: true)),
        ValidityClaim.Of(Kind is not null
            && (Kind.Equals(SpectralAssemblyKind.EdgeConnection)
                ? ComponentCount == 2 && MatrixRows == EdgeCount * ComponentCount && MatrixCols == MatrixRows && PositiveMassCount <= EdgeCount
                : ComponentCount == 1 && PositiveStar0Count <= VertexCount && PositiveStar1Count <= EdgeCount && PositiveStar2Count <= FaceCount
                  && (Genus is { IsSome: true, Case: int genus }
                      ? genus >= 0 && HarmonicDimension == (2 * genus) + Math.Max(0, BoundaryComponentCount - 1)
                      : HarmonicDimension == 0))));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormReceipt(
    Option<int> Genus, int ExpectedDimension, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double EpsRank, double SpectralRadius, double NullspaceThreshold,
    double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual,
    double Star1OrthonormalResidual, int PositiveStar1Count, EigenSolveReceipt<double, Arr<double>> Eigen,
    int BoundaryComponentCount = 0) : IValidityEvidence {
    public bool IsValid {
        get {
            int expected = ExpectedDimension;
            int boundaryComponentCount = BoundaryComponentCount;
            // Residual gate is operator-scale-relative: the eigen tolerance and the sqrt-machineEps floor both
            // carry max(1, spectralRadius); 1e3 is the dimensionless rounding-accumulation slack the applied
            // differential residuals incur above the eigenvalue tolerance — never a bare absolute.
            double residualTolerance = Math.Max(val1: SvdTolerance, val2: EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: SpectralRadius)) * 1.0e3;
            return ValidityClaim.All(
                ValidityClaim.Of(EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0),
                ValidityClaim.Of(Rank + Nullity == EdgeCount),
                ValidityClaim.CountExactly(count: BasisCount, expected: expected),
                ValidityClaim.Of(Nullity >= expected),
                ValidityClaim.Of(PositiveStar1Count <= EdgeCount),
                ValidityClaim.Of(Genus.Map(genus => expected == (2 * genus) + Math.Max(0, boundaryComponentCount - 1)).IfNone(expected == 0)),
                ValidityClaim.Positive(value: SvdTolerance),
                ValidityClaim.Positive(value: EpsRank),
                ValidityClaim.Of(Math.Abs(value: SvdTolerance - NullspaceThreshold) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: NullspaceThreshold)),
                ValidityClaim.Of(NullspaceThreshold <= (EpsRank * Math.Max(val1: 1.0, val2: SpectralRadius)) + EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Of(MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Of(MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon),
                ValidityClaim.Of(MaxClosedResidual <= residualTolerance),
                ValidityClaim.Of(MaxCoClosedResidual <= residualTolerance),
                ValidityClaim.Of(Star1OrthonormalResidual <= residualTolerance),
                ValidityClaim.Evidence(Eigen));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormBasis(Arr<Arr<double>> Forms, HarmonicOneFormReceipt Receipt) {
    public bool IsValid {
        get {
            HarmonicOneFormReceipt receipt = Receipt;
            return receipt.IsValid && Forms.Count == receipt.BasisCount && Forms.ForAll(form => form.Count == receipt.EdgeCount && form.ForAll(double.IsFinite));
        }
    }
}

// THE Rasm.Compute adjoint seam — name, fields, and projection rows are the cross-package contract.
// Transport evidence is declared by Meshing/mesh (the intrinsic-triangulation owner) and carried here.
[StructLayout(LayoutKind.Auto)]
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, SpectralAssemblyReceipt Receipt, Option<SignpostTransportReceipt> Transport = default, Option<HarmonicOneFormBasis> Harmonic = default) {
    public bool IsValid =>
        D0.IsValid && D1.IsValid && Receipt.IsValid
        && Star0.Count == D0.Cols.Value && Star1.Count == D0.Rows.Value && Star2.Count == D1.Rows.Value
        && Star0.ForAll(static v => double.IsFinite(v) && v > 0.0)
        && Star1.Fold(0.0, static (max, v) => Math.Max(max, Math.Abs(v))) is double star1Scale
        && Star1.ForAll(v => double.IsFinite(v) && v >= -(EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, star1Scale)))
        && Star2.ForAll(static v => double.IsFinite(v) && v > 0.0)
        && Transport.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)
        && Harmonic.Map(static basis => basis.IsValid).IfNone(noneValue: true);
    internal Fin<TOut> Project<TOut>(Op key) {
        DiscreteCalculus self = this;
        return AtomProjection.Rows<DiscreteCalculus, TOut>(self: self, key: key,
            ProjectionRow.Of<SpectralAssemblyReceipt>(() => Fin.Succ(self.Receipt)),
            ProjectionRow.Of<SignpostTransportReceipt>(() => self.Transport.ToFin(key.InvalidResult())),
            ProjectionRow.Of<HarmonicOneFormBasis>(() => self.Harmonic.ToFin(key.InvalidResult())),
            ProjectionRow.Of<HarmonicOneFormReceipt>(() => self.Harmonic.Map(static basis => basis.Receipt).ToFin(key.InvalidResult())));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralBasis(Arr<double> Eigenvalues, Arr<Arr<double>> Eigenvectors) {
    internal int VertexCount => Eigenvectors.IsEmpty ? 0 : Eigenvectors[index: 0].Count;
    internal double SpectralRadius => Eigenvalues.Fold(initialState: 0.0, f: static (max, lambda) => Math.Max(val1: max, val2: Math.Abs(value: lambda)));
    // THE one zero band — scale-relative to the spectral radius: eigen error grows with the operator
    // scale, and an absolute SqrtEpsilon band misclassifies every mode of an mm-unit spectrum as zero.
    internal double ZeroBand => EpsilonPolicy.SqrtEpsilon * Math.Max(val1: SpectralRadius, val2: EpsilonPolicy.ZeroTolerance);
    public bool IsValid {
        get {
            int vertexCount = VertexCount;
            double zeroBand = ZeroBand;
            return Eigenvalues.Count > 0
                && Eigenvalues.Count == Eigenvectors.Count
                && Eigenvalues.ForAll(lambda => double.IsFinite(lambda) && lambda >= -zeroBand)
                && Eigenvectors.ForAll(phi => vertexCount > 0 && phi.Count == vertexCount && phi.ForAll(double.IsFinite));
        }
    }
    public SpectralBasis Truncate(int k) =>
        k <= 0 || k >= Eigenvalues.Count ? this : new SpectralBasis(Eigenvalues: new Arr<double>([.. Eigenvalues.AsIterable().Take(k)]), Eigenvectors: new Arr<Arr<double>>([.. Eigenvectors.AsIterable().Take(k)]));
}
```

## [04]-[DESCRIPTOR_ALGEBRA]

- Owner: `SpectralDescriptorPolicy` the normalization bundle (scale × energy × zero-mode × optional crop, `Raw` the canonical no-op row, `IsRaw`/`IsValueOnly` derived predicates, `Admit` the gate); `SpectralWaveReceipt` the WKS normalization evidence (energy/bandwidth, first-nonzero scale, zero-mode and crop censuses, raw/normalized weight sums with the `Σw = 1` gate, log-eigenvalue range); `SpectralDescriptorReceipt` the descriptor evidence (filter, vertex/eigenpair/source censuses, pairwise and comparison-ready flags, the policy applied, optional wave evidence); `SpectralDescriptor` the values + receipt carrier with `Normalize(policy)` (value-only renormalization — a scale/crop change demands re-evaluation and fails typed; only the energy axis merges into the receipt's policy, so evaluation provenance survives) and `Rank(candidates, policy)`; `SpectralRankingPolicy` (`Default` = Raw + Euclidean) / `SpectralRank` / `SpectralRanking`; `SpectralKernel` the `internal static` evaluation owner — `EvaluateFilteredDetailed` (the dense-buffer filtered-signature kernel: point signatures `Σ w(λₖ)·φₖ(v)²` or pairwise distances `√(Σ w(λₖ)·(φₖ(v) − φₖ(s))²/|S|)` over admitted source sets, zero modes classified by the basis' scale-relative `ZeroBand`, crop policy applied to the eigen index set, scale normalization by first nonzero eigenvalue), `NormalizeDescriptor`/`NormalizeValues` (L1/L2 via `TensorPrimitives.SumOfMagnitudes`/`Norm`, z-score), `RankDescriptors` (candidates re-normalized to the query policy, distances read off the `SpectralDistanceKind` compute column, ranked ascending with index tiebreak), and the one named numeric-policy constant `WaveBandwidthFloor = 1e-9` — the harmonic eps-rank default is `Meshing/dec`'s assembly policy, declared beside the construction that applies it, never re-declared here.
- Entry: `filter.ApplyDetailed(basis, sources, policy, key)` is the evaluation entry; `descriptor.Normalize(policy)` and `descriptor.Rank(candidates, policy)` the post-processing entries — one descriptor pipeline, no sibling evaluate/compare surfaces.
- Auto: WKS weights normalize to unit sum with the full `SpectralWaveReceipt` minted inline; `ComparisonReady` derives from policy + wave evidence so a raw HKS never silently ranks against a normalized WKS; `RankDescriptors` re-normalizes every candidate to the query's policy before measuring — policy mismatch is repaired, not ignored.
- Receipt: `SpectralWaveReceipt` and `SpectralDescriptorReceipt` on the rails `ValidityClaim.All` fold with semantic gates (`CroppedEigenpairCount >= NonZeroEigenpairCount`, `RawWeightSum > 0`, WKS unit-sum within `1e-9`; source/eigenpair/vertex couplings).
- Packages: MathNet.Numerics (`Distance.Euclidean/Manhattan/Cosine`), System.Numerics.Tensors (`TensorPrimitives.SumOfMagnitudes`/`Norm`/`IsFiniteAll`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new signature family (scale-invariant HKS, improved WKS variants) is one filter case + policy rows — the kernel loop is already generic over the weight function; a new distance is one `SpectralDistanceKind` row — the compute column IS the arm.
- Boundary: `EvaluateFilteredDetailed`'s dense `double[]` buffer loop is the named statement-kernel exemption — per-eigenpair-per-vertex accumulation over `n·k` (·`|S|` pairwise) terms where a `Seq` fold would churn allocations; the kernel is mesh-free — vertex COUNT is the only topology it sees, so it serves tet bases, grid bases, and mesh bases identically; mesh-side basis computation and caching (`SpectralBasisBundle`) are `Meshing/dec`'s.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorPolicy(SpectralScaleNormalization ScaleNormalization, SpectralEnergyNormalization EnergyNormalization, SpectralZeroModePolicy ZeroModePolicy, Option<Dimension> CropCount) {
    public static SpectralDescriptorPolicy Raw => new(ScaleNormalization: SpectralScaleNormalization.Raw, EnergyNormalization: SpectralEnergyNormalization.Raw, ZeroModePolicy: SpectralZeroModePolicy.Keep, CropCount: None);
    internal bool IsValid => ScaleNormalization is not null && EnergyNormalization is not null && ZeroModePolicy is not null && CropCount.Map(static count => count.Value > 0).IfNone(noneValue: true);
    internal bool IsRaw => ScaleNormalization.Equals(SpectralScaleNormalization.Raw) && EnergyNormalization.Equals(SpectralEnergyNormalization.Raw) && ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) && CropCount.IsNone;
    internal bool IsValueOnly => ScaleNormalization.Equals(SpectralScaleNormalization.Raw) && ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) && CropCount.IsNone;
    internal static Fin<SpectralDescriptorPolicy> Admit(SpectralDescriptorPolicy policy, Op key) =>
        guard(policy.IsValid, key.InvalidInput()).ToFin().Map(_ => policy);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralWaveReceipt(double Energy, double Bandwidth, Option<double> FirstNonZeroScale, int ZeroModeCount, int CroppedEigenpairCount, int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum, Option<double> MinLogEigenvalue, Option<double> MaxLogEigenvalue, bool WksNormalized) : IValidityEvidence {
    public bool IsValid {
        get {
            Option<double> maxLogEigenvalue = MaxLogEigenvalue;
            return ValidityClaim.All(
                ValidityClaim.Positive(value: Energy),
                ValidityClaim.Positive(value: Bandwidth),
                ValidityClaim.Of(ZeroModeCount >= 0 && CroppedEigenpairCount >= NonZeroEigenpairCount && NonZeroEigenpairCount > 0),
                ValidityClaim.Positive(value: RawWeightSum),
                ValidityClaim.Of(!WksNormalized || Math.Abs(value: NormalizedWeightSum - 1.0) <= 1.0e-9),
                ValidityClaim.Of(FirstNonZeroScale.Map(static first => first > 0.0).IfNone(noneValue: true)),
                ValidityClaim.Of(MinLogEigenvalue.Map(min => maxLogEigenvalue.Map(max => min <= max).IfNone(noneValue: true)).IfNone(noneValue: true)));
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorReceipt(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount, bool ComparisonReady, bool Pairwise, bool EnergyNormalized, bool ScaleNormalized, SpectralDescriptorPolicy Policy = default, int ZeroModeCount = 0, int CroppedEigenpairCount = 0, Option<SpectralWaveReceipt> Wave = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Filter is not null),
        ValidityClaim.Of(VertexCount > 0 && EigenpairCount > 0),
        ValidityClaim.Of(CroppedEigenpairCount > 0 && CroppedEigenpairCount <= EigenpairCount),
        ValidityClaim.Of(ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount),
        ValidityClaim.Of(SourceCount >= 0 && SourceCount <= VertexCount),
        ValidityClaim.Of(!Pairwise || SourceCount > 0),
        ValidityClaim.Of(Policy.IsValid),
        ValidityClaim.Of(!ComparisonReady || !Policy.IsRaw || Wave.IsSome),
        ValidityClaim.Of(Wave.Map(static wave => wave.IsValid).IfNone(noneValue: true)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptor(Arr<double> Values, SpectralDescriptorReceipt Receipt) {
    public bool IsValid => Receipt.IsValid && Values.Count == Receipt.VertexCount && Values.ForAll(double.IsFinite);
    public Fin<SpectralDescriptor> Normalize(SpectralDescriptorPolicy policy, Op? key = null) =>
        SpectralKernel.NormalizeDescriptor(descriptor: this, policy: policy, key: key.OrDefault());
    public Fin<SpectralRanking> Rank(Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op? key = null) =>
        SpectralKernel.RankDescriptors(query: this, candidates: candidates, policy: policy, key: key.OrDefault());
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, SpectralDistanceKind Distance) {
    public static SpectralRankingPolicy Default => new(Descriptor: SpectralDescriptorPolicy.Raw, Distance: SpectralDistanceKind.Euclidean);
    internal bool IsValid => Descriptor.IsValid && Distance is not null;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRank(int Index, double Distance, SpectralDescriptor Descriptor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRanking(SpectralDescriptor Query, Seq<SpectralRank> Items, SpectralRankingPolicy Policy);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class SpectralKernel {
    internal const double WaveBandwidthFloor = 1e-9;
    // STATEMENT-KERNEL EXEMPTION — dense per-eigenpair-per-vertex accumulation buffer; a Seq fold
    // would churn allocations over n * k (* |S| pairwise) terms.
    internal static Fin<SpectralDescriptor> EvaluateFilteredDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralFilter filter, SpectralDescriptorPolicy policy, Op key) {
        int n = basis.VertexCount;
        int[] sourceSet = sources is { IsSome: true, Case: Seq<int> values } ? [.. values.AsIterable()] : [];
        if (n == 0 || (sources.IsSome && sourceSet.Length == 0) || sourceSet.Any(s => s < 0 || s >= n) || sourceSet.Distinct().Count() != sourceSet.Length)
            return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput());
        if (!basis.Eigenvectors.ForAll(phi => phi.Count == n)) return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult());
        double zeroBand = basis.ZeroBand;
        int zeroModeCount = basis.Eigenvalues.AsIterable().Count(lambda => lambda <= zeroBand);
        double firstNonZero = basis.Eigenvalues.AsIterable().FirstOrDefault(lambda => lambda > zeroBand);
        if (policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue) && firstNonZero <= zeroBand) return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult());
        int[] eigenIndices = [.. Enumerable.Range(start: 0, count: basis.Eigenvalues.Count)
            .Where(i => policy.ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) || basis.Eigenvalues[index: i] > zeroBand)
            .Take(policy.CropCount.Map(static count => count.Value).IfNone(basis.Eigenvalues.Count))];
        if (eigenIndices.Length == 0) return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput());
        bool scaleNormalized = policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue);
        double[] scaledEigenvalues = [.. eigenIndices.Select(k => scaleNormalized ? basis.Eigenvalues[index: k] / firstNonZero : basis.Eigenvalues[index: k])];
        (double[] weights, Option<SpectralWaveReceipt> wave) = WeightsOf(filter: filter, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero, zeroBand: scaleNormalized ? zeroBand / firstNonZero : zeroBand, zeroModeCount: zeroModeCount, croppedCount: eigenIndices.Length);
        if (weights.Any(static weight => !double.IsFinite(weight)) || (filter is SpectralFilter.WaveCase && wave.IsNone)) return Fin.Fail<SpectralDescriptor>(key.InvalidResult());
        bool isPairwise = sourceSet.Length > 0;
        double normFactor = isPairwise ? 1.0 / sourceSet.Length : 1.0;
        double[] result = new double[n];
        for (int ki = 0; ki < eigenIndices.Length; ki++) {
            int k = eigenIndices[ki];
            double w = weights[ki];
            Arr<double> phi = basis.Eigenvectors[index: k];
            if (isPairwise)
                for (int v = 0; v < n; v++) {
                    double phiV = phi[index: v];
                    for (int s = 0; s < sourceSet.Length; s++) {
                        double delta = phiV - phi[index: sourceSet[s]];
                        result[v] += w * delta * delta;
                    }
                }
            if (!isPairwise)
                for (int v = 0; v < n; v++) result[v] += w * phi[index: v] * phi[index: v];
        }
        if (isPairwise) for (int v = 0; v < n; v++) result[v] = Math.Sqrt(d: Math.Max(val1: 0.0, val2: result[v] * normFactor));
        return NormalizeValues(values: result, policy: policy, key: key).Map(values => new SpectralDescriptor(Values: new Arr<double>(values), Receipt: new SpectralDescriptorReceipt(Filter: filter, VertexCount: n, EigenpairCount: basis.Eigenvalues.Count, SourceCount: sourceSet.Length, ComparisonReady: !policy.IsRaw || wave.IsSome, Pairwise: isPairwise, EnergyNormalized: !policy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw), ScaleNormalized: !policy.ScaleNormalization.Equals(SpectralScaleNormalization.Raw), Policy: policy, ZeroModeCount: zeroModeCount, CroppedEigenpairCount: eigenIndices.Length, Wave: wave)));
    }
    internal static Fin<SpectralDescriptor> NormalizeDescriptor(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        from valid in guard(descriptor.IsValid, key.InvalidInput()).ToFin()
        from activePolicy in SpectralDescriptorPolicy.Admit(policy: policy, key: key)
        from _ in activePolicy.IsValueOnly
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(key.Unsupported(geometryType: typeof(SpectralDescriptor), outputType: typeof(SpectralDescriptorPolicy)))
        from values in NormalizeValues(values: [.. descriptor.Values.AsIterable()], policy: activePolicy, key: key)
        // Energy is the ONLY re-normalizable axis: merge it into the evaluation policy so the receipt
        // keeps its scale/zero-mode/crop provenance instead of overwriting it with the value-only policy.
        let mergedPolicy = descriptor.Receipt.Policy with { EnergyNormalization = activePolicy.EnergyNormalization }
        let receipt = descriptor.Receipt with { ComparisonReady = !mergedPolicy.IsRaw || descriptor.Receipt.Wave.IsSome, EnergyNormalized = !activePolicy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw), Policy = mergedPolicy }
        select new SpectralDescriptor(Values: new Arr<double>(values), Receipt: receipt);
    internal static Fin<SpectralRanking> RankDescriptors(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) =>
        !policy.IsValid || !query.IsValid || candidates.IsEmpty || !candidates.ForAll(static candidate => candidate.IsValid)
            ? Fin.Fail<SpectralRanking>(key.InvalidInput())
            : from normalizedQuery in NormalizeForRanking(descriptor: query, policy: policy.Descriptor, key: key)
              from normalizedCandidates in candidates.TraverseM(candidate => NormalizeForRanking(descriptor: candidate, policy: policy.Descriptor, key: key)).As()
              from ranks in RankNormalized(query: normalizedQuery, candidates: normalizedCandidates, policy: policy, key: key)
              select new SpectralRanking(Query: normalizedQuery, Items: ranks, Policy: policy);
    private static Fin<SpectralDescriptor> NormalizeForRanking(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        descriptor.Receipt.ComparisonReady && SamePolicy(left: descriptor.Receipt.Policy, right: policy)
            ? Fin.Succ(descriptor)
            : NormalizeDescriptor(descriptor: descriptor, policy: policy, key: key);
    private static bool SamePolicy(SpectralDescriptorPolicy left, SpectralDescriptorPolicy right) =>
        left.ScaleNormalization.Equals(right.ScaleNormalization)
        && left.EnergyNormalization.Equals(right.EnergyNormalization)
        && left.ZeroModePolicy.Equals(right.ZeroModePolicy)
        && left.CropCount.Match(
            Some: l => right.CropCount.Match(Some: r => l.Value == r.Value, None: static () => false),
            None: () => right.CropCount.IsNone);
    // zeroBand arrives in the SAME units as the eigenvalue array (scaled by 1/firstNonZero when the
    // policy scale-normalizes), so zero-mode classification is one law across raw and scaled spectra.
    private static (double[] Weights, Option<SpectralWaveReceipt> Wave) WeightsOf(SpectralFilter filter, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount) =>
        filter is SpectralFilter.WaveCase wave
            ? WaveWeightsOf(wave: wave, eigenvalues: eigenvalues, firstNonZero: firstNonZero, zeroBand: zeroBand, zeroModeCount: zeroModeCount, croppedCount: croppedCount)
            : ([.. eigenvalues.Select(filter.Weight)], Option<SpectralWaveReceipt>.None);
    private static (double[] Weights, Option<SpectralWaveReceipt> Wave) WaveWeightsOf(SpectralFilter.WaveCase wave, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount) {
        double[] raw = [.. eigenvalues.Select(wave.Weight)];
        double sum = raw.Sum();
        if (!double.IsFinite(sum) || sum <= EpsilonPolicy.SqrtEpsilon) return (raw, Option<SpectralWaveReceipt>.None);
        double[] normalized = [.. raw.Select(weight => weight / sum)];
        double[] positiveLogs = [.. eigenvalues.Where(lambda => lambda > zeroBand).Select(static lambda => Math.Log(d: lambda))];
        SpectralWaveReceipt receipt = new(
            Energy: wave.Energy.Value,
            Bandwidth: Math.Max(val1: wave.Bandwidth.Value, val2: WaveBandwidthFloor),
            FirstNonZeroScale: firstNonZero > 0.0 ? Some(firstNonZero) : Option<double>.None,
            ZeroModeCount: zeroModeCount,
            CroppedEigenpairCount: croppedCount,
            NonZeroEigenpairCount: positiveLogs.Length,
            RawWeightSum: sum,
            NormalizedWeightSum: normalized.Sum(),
            MinLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(positiveLogs.Min()),
            MaxLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(positiveLogs.Max()),
            WksNormalized: true);
        return (normalized, receipt.IsValid ? Some(receipt) : Option<SpectralWaveReceipt>.None);
    }
    private static Fin<Seq<SpectralRank>> RankNormalized(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) {
        int valueCount = query.Values.Count;
        if (valueCount <= 0 || candidates.Exists(candidate => candidate.Values.Count != valueCount)) return Fin.Fail<Seq<SpectralRank>>(key.InvalidInput());
        double[] queryValues = [.. query.Values.AsIterable()];
        SpectralRank[] ranks = [.. candidates.AsIterable().Select((candidate, index) => new SpectralRank(Index: index, Distance: policy.Distance.Compute(a: queryValues, b: [.. candidate.Values.AsIterable()]), Descriptor: candidate)).OrderBy(static rank => rank.Distance).ThenBy(static rank => rank.Index)];
        return ranks.All(static rank => double.IsFinite(rank.Distance)) ? Fin.Succ(toSeq(ranks)) : Fin.Fail<Seq<SpectralRank>>(key.InvalidResult());
    }
    private static Fin<double[]> NormalizeValues(double[] values, SpectralDescriptorPolicy policy, Op key) {
        if (!TensorPrimitives.IsFiniteAll<double>(values.AsSpan())) return Fin.Fail<double[]>(key.InvalidResult());
        if (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw)) return Fin.Succ(values);
        double[] result = [.. values];
        if (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL1) || policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL2))
            return (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL1) ? TensorPrimitives.SumOfMagnitudes<double>(result) : TensorPrimitives.Norm<double>(result)) switch {
                double scale when scale > EpsilonPolicy.SqrtEpsilon => Fin.Succ<double[]>([.. result.Select(value => value / scale)]),
                _ => Fin.Fail<double[]>(key.InvalidResult()),
            };
        double mean = result.Average();
        double variance = result.Sum(value => (value - mean) * (value - mean)) / result.Length;
        double sigma = Math.Sqrt(d: variance);
        return sigma > EpsilonPolicy.SqrtEpsilon ? Fin.Succ<double[]>([.. result.Select(value => (value - mean) / sigma)]) : Fin.Fail<double[]>(key.InvalidResult());
    }
}
```

## [05]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]        | [OWNER]                                                    | [KIND]                                                      | [CASES] |
| :-----: | :-------------------- | :---------------------------------------------------------- | :----------------------------------------------------------- | :-----: |
|  [01]   | Spectral policy       | `SpectralAssemblyKind` · `SpectralScaleNormalization` · `SpectralEnergyNormalization` · `SpectralZeroModePolicy` · `SpectralDistanceKind` | `[SmartEnum<int>]` vocabularies                             | 2·2·4·2·3 |
|  [02]   | Transfer functions    | `SpectralFilter`                                           | closed `[Union]` + partial-monoid `Compose` + inlined weight |    6    |
|  [03]   | DEC carriers          | `DiscreteCalculus` (frozen seam) · `SpectralBasis`         | operator bundle + eigenpair carrier                          |    2    |
|  [04]   | Assembly evidence     | `SpectralAssemblyReceipt` · `HarmonicOneFormReceipt` · `HarmonicOneFormBasis` | `ValidityClaim.All` fold + scale-relative residual gates    |    3    |
|  [05]   | Descriptor algebra    | `SpectralDescriptorPolicy` · `SpectralWaveReceipt` · `SpectralDescriptorReceipt` · `SpectralDescriptor` · `SpectralRankingPolicy`/`SpectralRank`/`SpectralRanking` | policy + evidence + carrier family                           |    7    |
|  [06]   | Evaluation kernel     | `SpectralKernel`                                           | dense filtered-signature/normalize/rank kernel               |    1    |
