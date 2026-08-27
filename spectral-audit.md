# `spectral.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/spectral.md`

This audit counts nonblank authored C# lines in the affected fence fragments. Required consumer edits outside the target are named but excluded from the target LOC total. The queue is ordered so shape and generated-surface reductions land before carrier corrections and kernel reductions.

Evidence basis: the full target; `CLAUDE.md`; the repository and .NET branch planning laws; the C# stack standards, with direct application of the language, shape, surface, result, algorithm, system-API, validation, and compute laws; both checked-in `.api` tiers, with full attention to LanguageExt, Thinktecture, `System.Numerics.Tensors`, MathNet.Numerics, CSparse, and CommunityToolkit.HighPerformance; every current `libs/dotnet/` consumer of the affected symbols; and the prior root audit form at commit `f17b2d8521806b567232dd8c28167e4cbe294da4`. This is a source-only audit; no build, test, formatter, or validation lane was run.

Accepted total for target fences: **-15 LOC, -3 authored type symbols, -23 authored member symbols**, plus removal of unearned generated keyed lookup/conversion surfaces from the two surviving process-local policy rosters. The queue also replaces the vague `EnergyNormalization`/`Kind`/`Detailed` names with the mathematical roles the surviving surfaces actually serve.

## 1. Collapse the assembly tag and its two derived census mirrors into the assembly row

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:38-42`, anchor `[SmartEnum<int>] public sealed partial class SpectralAssemblyKind`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:171-201`, anchors `public readonly record struct SpectralAssembly` and `Kind.Equals(SpectralAssemblyKind.EdgeConnection)`

### From

```csharp
[SmartEnum<int>]
public sealed partial class SpectralAssemblyKind {
    public static readonly SpectralAssemblyKind Dec = new(key: 0);
    public static readonly SpectralAssemblyKind EdgeConnection = new(key: 1);
}
```

```csharp
    double BoundaryCompositionResidual, Option<int> Genus, int HarmonicDimension, SpectralAssemblyKind Kind,
    Option<int> BoundaryEdgeCount = default, int BoundaryComponentCount = 0, Option<int> NonManifoldEdgeCount = default,
    Option<int> EulerCharacteristic = default, int ComponentCount = 1, Option<int> PositiveMassCount = default,
```

```csharp
        VertexCount >= 0 && EdgeCount >= 0 && FaceCount >= 0 && MatrixRows >= 0 && MatrixCols >= 0 && NonZeros >= 0,
        AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount,
        !FlippedIntrinsicLifted || Kind.Equals(SpectralAssemblyKind.EdgeConnection),
        BoundaryEdgeCount.Map(count =>
            count >= 0 && count <= EdgeCount && (count == 0) == (BoundaryComponentCount == 0) && count >= 3 * BoundaryComponentCount).IfNone(noneValue: true),
        NonManifoldEdgeCount.Map(count => count >= 0 && count <= EdgeCount).IfNone(noneValue: true),
        EulerCharacteristic.Map(chi =>
            chi == VertexCount - EdgeCount + FaceCount
            && NonManifoldEdgeCount.IfNone(noneValue: 0) == 0
            && Genus.Map(genus => chi == 2 - (2 * genus) - BoundaryComponentCount).IfNone(noneValue: true)).IfNone(noneValue: true),
        Kind.Equals(SpectralAssemblyKind.EdgeConnection)
            ? ComponentCount == 2 && MatrixRows == EdgeCount * ComponentCount && MatrixCols == MatrixRows
              && PositiveMassCount.Map(count => count >= 0 && count <= EdgeCount).IfNone(noneValue: true)
            : ComponentCount == 1 && PositiveStar0Count <= VertexCount && PositiveStar1Count <= EdgeCount && PositiveStar2Count <= FaceCount
              && (Genus is { IsSome: true, Case: int genus }
                  ? genus >= 0 && HarmonicDimension == (2 * genus) + Math.Max(0, BoundaryComponentCount - 1)
                  : HarmonicDimension == 0));
```

### To

```csharp
    double BoundaryCompositionResidual, Option<int> Genus, bool EdgeConnection,
    Option<int> BoundaryEdgeCount = default, int BoundaryComponentCount = 0, Option<int> NonManifoldEdgeCount = default,
    Option<int> EulerCharacteristic = default, Option<int> PositiveMassCount = default,
```

```csharp
        VertexCount >= 0 && EdgeCount >= 0 && FaceCount >= 0 && MatrixRows >= 0 && MatrixCols >= 0 && NonZeros >= 0,
        ValidityClaim.Nonnegative(value: BoundaryCompositionResidual),
        AdmittedFaceCount >= 0 && SkippedDegenerateFaces >= 0 && SkippedMissingEdges >= 0
            && (long)AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount,
        Symmetry is not { IsSome: true, Case: (double Residual, double Tolerance) measured } || (ValidityClaim.Nonnegative(measured.Residual) && ValidityClaim.Positive(measured.Tolerance) && measured.Residual <= measured.Tolerance),
        BoundaryCompositionTolerance <= 0.0 || BoundaryCompositionResidual <= BoundaryCompositionTolerance,
        FactorNonZeros.Map(static (int value) => value > 0).IfNone(noneValue: true),
        !FlippedIntrinsicLifted || EdgeConnection,
        BoundaryEdgeCount is not { IsSome: true, Case: int boundaryEdges }
            || (boundaryEdges >= 0 && boundaryEdges <= EdgeCount && (boundaryEdges == 0) == (BoundaryComponentCount == 0) && boundaryEdges >= 3L * BoundaryComponentCount),
        NonManifoldEdgeCount is not { IsSome: true, Case: int nonManifoldEdges } || (nonManifoldEdges >= 0 && nonManifoldEdges <= EdgeCount),
        EulerCharacteristic is not { IsSome: true, Case: int chi } || ((long)chi == (long)VertexCount - EdgeCount + FaceCount
            && NonManifoldEdgeCount.IfNone(noneValue: 0) == 0
            && (Genus is not { IsSome: true, Case: int genus } || chi == 2L - (2L * genus) - BoundaryComponentCount)),
        BoundaryComponentCount >= 0,
        Genus is not { IsSome: true, Case: int heldGenus } || heldGenus >= 0,
        EdgeConnection
            ? MatrixRows == (long)EdgeCount * 2L && MatrixCols == MatrixRows
              && (PositiveMassCount is not { IsSome: true, Case: int positiveMass } || (positiveMass >= 0 && positiveMass <= EdgeCount))
            : PositiveStar0Count >= 0 && PositiveStar0Count <= VertexCount
              && PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount
              && PositiveStar2Count >= 0 && PositiveStar2Count <= FaceCount);
```

### Effect

- Target fenced LOC: `25 -> 25` (**0**). The collapse removes authored and generated surface; it does not buy LOC by dropping evidence gates.
- Authored symbols: **-1 type** (`SpectralAssemblyKind`) and **-4 members** (`Dec`, `EdgeConnection` item, `ComponentCount`, `HarmonicDimension`); the `Kind` positional property becomes the true-arm `EdgeConnection` column.
- Generated surface: the assembly no longer pays a smart-enum roster, key, lookup, conversion, item list, or dispatch surface for one boolean discriminator.

### API and consumer proof

The C# shape law explicitly collapses a two-row vocabulary with no behavior column to a `bool` column on its owner named for the true arm. No consumer performs assembly-kind lookup, key projection, serialization, or generated dispatch. `ComponentCount` is reconstructed exactly from `EdgeConnection` (`2` versus `1`), and `HarmonicDimension` is reconstructed exactly from `Genus` plus `BoundaryComponentCount`; neither has an independent reader anywhere in `libs/dotnet/`. The replacement retains the nonnegative genus and boundary-component gates, closes the previously fail-open negative face/skip/star counts, widens every count/topology product before arithmetic under checked overflow, requires both members of a present symmetry witness to be finite with a positive tolerance, and uses `Option` property patterns instead of lambdas that would capture the enclosing record struct.

### Ripples

- Same file: the filter and DEC cards replace `SpectralAssemblyKind` with the `SpectralAssembly.EdgeConnection` column and stop listing `HarmonicDimension`/`ComponentCount` as carried evidence.
- `libs/dotnet/Rasm/.planning/Meshing/dec.md`: delete the `harmonicDimension` local, remove that argument from the `DecAssemblyOf` call and signature, and have its result pass `EdgeConnection: false`. `EdgeConnectionAssemblyOf` passes `EdgeConnection: true`; neither constructor supplies `ComponentCount` or `HarmonicDimension`. The page card replaces the incoming assembly-kind vocabulary with the assembly's true-arm column.
- No other code fence reads the removed members.

## 2. Collapse the two behaviorless descriptor axes to policy booleans

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:44-49`, anchor `SpectralScaleNormalization`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:75-79`, anchor `SpectralZeroModePolicy`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:332-339`, anchor `SpectralDescriptorPolicy`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:364`, anchor `ScaleNormalized`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:412-417`, anchors `scaleNormalized` and `ZeroModePolicy`

### From

```csharp
[SmartEnum<int>]
public sealed partial class SpectralScaleNormalization {
    public static readonly SpectralScaleNormalization Raw = new(key: 0);
    public static readonly SpectralScaleNormalization FirstNonZeroEigenvalue = new(key: 1);
}
```

```csharp
[SmartEnum<int>]
public sealed partial class SpectralZeroModePolicy {
    public static readonly SpectralZeroModePolicy Keep = new(key: 0);
    public static readonly SpectralZeroModePolicy Drop = new(key: 1);
}
```

```csharp
public readonly record struct SpectralDescriptorPolicy(SpectralScaleNormalization ScaleNormalization, SpectralEnergyNormalization EnergyNormalization, SpectralZeroModePolicy ZeroModePolicy, Option<Dimension> CropCount) : IValidityEvidence {
    public static SpectralDescriptorPolicy Raw => new(ScaleNormalization: SpectralScaleNormalization.Raw, EnergyNormalization: SpectralEnergyNormalization.Raw, ZeroModePolicy: SpectralZeroModePolicy.Keep, CropCount: None);
    public bool IsValid => ValidityClaim.All(
        ScaleNormalization is not null && EnergyNormalization is not null && ZeroModePolicy is not null,
        CropCount.Map(static (Dimension count) => count.Value > 0).IfNone(noneValue: true));
```

```csharp
        bool scaleNormalized = policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue);
```

```csharp
    public bool ScaleNormalized => !Policy.ScaleNormalization.Equals(SpectralScaleNormalization.Raw);
```

```csharp
            .Where(i => policy.ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) || basis.Eigenvalues[index: i] > zeroBand)
```

### To

```csharp
public readonly record struct SpectralDescriptorPolicy(bool NormalizeScale, SpectralEnergyNormalization EnergyNormalization, bool IncludeZeroModes, Option<Dimension> CropCount) : IValidityEvidence {
    public static SpectralDescriptorPolicy Raw => new(NormalizeScale: false, EnergyNormalization: SpectralEnergyNormalization.Raw, IncludeZeroModes: true, CropCount: None);
    public bool IsValid => ValidityClaim.All(
        EnergyNormalization is not null,
        CropCount.Map(static count => count.Value > 0).IfNone(noneValue: true));
```

```csharp
        bool scaleNormalized = policy.NormalizeScale;
```

```csharp
    public bool ScaleNormalized => Policy.NormalizeScale;
```

```csharp
            .Where((int i) => policy.IncludeZeroModes || basis.Eigenvalues[index: i] > zeroBand)
```

### Effect

- Target fenced LOC: `20 -> 10` (**-10**).
- Authored symbols: **-2 types** and **-4 static item members**; the policy retains the two independent decisions as ordinary value columns.
- Logic: the hot kernel reads the decisions directly instead of reference-comparing generated singleton rows.

### API and consumer proof

Both deleted vocabularies have exactly two rows, no payload, no behavior column, no lookup, no key read, no dispatch, and no boundary identity. They are the exact deleted form in `shapes.md`: a boolean column on the owner named for the true arm. Every current consumer receives the whole `SpectralDescriptorPolicy`; no external fence constructs either deleted type directly.

### Ripples

- Same file: cards and profile prose name `NormalizeScale` and `IncludeZeroModes`; move 9 deletes the now-obsolete `ScaleNormalized` mirror.
- `libs/dotnet/Rasm/.planning/Processing/segment.md`: no signature change beyond recompiling its carried `SpectralDescriptorPolicy`; it constructs no deleted row.
- No other `libs/dotnet/` code fence names either deleted vocabulary.

## 3. Make the two surviving process-local behavior rosters keyless

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:50-73`, anchor `[SmartEnum<int>] public sealed partial class SpectralEnergyNormalization`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:81-97`, anchor `[SmartEnum<int>] public sealed partial class SpectralDistanceKind`

### From

```csharp
[SmartEnum<int>]
public sealed partial class SpectralEnergyNormalization {
    public static readonly SpectralEnergyNormalization Raw = new(key: 0, rescale: static values => Some(values));
    public static readonly SpectralEnergyNormalization UnitL1 = new(key: 1, rescale: static values => Scaled(values: values, scale: TensorPrimitives.SumOfMagnitudes<double>(values)));
    public static readonly SpectralEnergyNormalization UnitL2 = new(key: 2, rescale: static values => Scaled(values: values, scale: TensorPrimitives.Norm<double>(values)));
    public static readonly SpectralEnergyNormalization ZScore = new(key: 3, rescale: static values => Centered(values: values));
```

```csharp
[SmartEnum<int>]
public sealed partial class SpectralDistanceKind {
    public static readonly SpectralDistanceKind Euclidean = new(key: 0, compute: static (a, b) => MathNet.Numerics.Distance.Euclidean(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Manhattan = new(key: 1, compute: static (a, b) => MathNet.Numerics.Distance.Manhattan(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Cosine = new(key: 2, compute: static (a, b) => MathNet.Numerics.Distance.Cosine(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Chebyshev = new(key: 3, compute: static (a, b) => MathNet.Numerics.Distance.Chebyshev(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Canberra = new(key: 4, compute: static (a, b) => MathNet.Numerics.Distance.Canberra(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Minkowski3 = new(key: 5, compute: static (a, b) => MathNet.Numerics.Distance.Minkowski(3.0, a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Hamming = new(key: 6, compute: static (a, b) => MathNet.Numerics.Distance.Hamming(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Jaccard = new(key: 7, compute: static (a, b) => MathNet.Numerics.Distance.Jaccard(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind MeanAbsolute = new(key: 8, compute: static (a, b) => MathNet.Numerics.Distance.MAE(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind MeanSquared = new(key: 9, compute: static (a, b) => MathNet.Numerics.Distance.MSE(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind SumAbsolute = new(key: 10, compute: static (a, b) => MathNet.Numerics.Distance.SAD(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind SumSquared = new(key: 11, compute: static (a, b) => MathNet.Numerics.Distance.SSD(a.Dense, b.Dense));
```

### To

```csharp
[SmartEnum]
public sealed partial class SpectralEnergyNormalization {
    public static readonly SpectralEnergyNormalization Raw = new(rescale: static values => Some(values));
    public static readonly SpectralEnergyNormalization UnitL1 = new(rescale: static values => Scaled(values: values, scale: TensorPrimitives.SumOfMagnitudes<double>(values)));
    public static readonly SpectralEnergyNormalization UnitL2 = new(rescale: static values => Scaled(values: values, scale: TensorPrimitives.Norm<double>(values)));
    public static readonly SpectralEnergyNormalization ZScore = new(rescale: static values => Centered(values: values));
```

```csharp
[SmartEnum]
public sealed partial class SpectralDistanceKind {
    public static readonly SpectralDistanceKind Euclidean = new(compute: static (a, b) => MathNet.Numerics.Distance.Euclidean(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Manhattan = new(compute: static (a, b) => MathNet.Numerics.Distance.Manhattan(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Cosine = new(compute: static (a, b) => MathNet.Numerics.Distance.Cosine(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Chebyshev = new(compute: static (a, b) => MathNet.Numerics.Distance.Chebyshev(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Canberra = new(compute: static (a, b) => MathNet.Numerics.Distance.Canberra(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Minkowski3 = new(compute: static (a, b) => MathNet.Numerics.Distance.Minkowski(3.0, a.Dense, b.Dense));
    public static readonly SpectralDistanceKind Hamming = new(compute: static (a, b) => MathNet.Numerics.Distance.Hamming(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind Jaccard = new(compute: static (a, b) => MathNet.Numerics.Distance.Jaccard(a.Raw, b.Raw));
    public static readonly SpectralDistanceKind MeanAbsolute = new(compute: static (a, b) => MathNet.Numerics.Distance.MAE(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind MeanSquared = new(compute: static (a, b) => MathNet.Numerics.Distance.MSE(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind SumAbsolute = new(compute: static (a, b) => MathNet.Numerics.Distance.SAD(a.Dense, b.Dense));
    public static readonly SpectralDistanceKind SumSquared = new(compute: static (a, b) => MathNet.Numerics.Distance.SSD(a.Dense, b.Dense));
```

### Effect

- Target fenced LOC: unchanged (**0**); sixteen meaningless key arguments disappear.
- Authored symbols: unchanged.
- Generated surface: both owners retain `Items`, behavior columns, reference identity, and total `Switch`/`Map`; their unused key member, lookup, keyed conversion operators, and keyed-owner conformance disappear.

### API and consumer proof

Thinktecture keyless `[SmartEnum]` is the documented process-local behavior vocabulary. No `libs/dotnet/` consumer reads a key, calls `Get`/`TryGet`, parses, serializes, persists, or crosses either owner on a wire. Energy normalization is selected by its `Rescale` behavior and distance by `Compute`; those columns are their identities at the use site.

### Ripples

- Same file: the owner and package cards say keyless `[SmartEnum]`.
- Outside target: none.

## 4. Name normalization, distance, and evaluation by their actual roles

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:50-57`, anchor `SpectralEnergyNormalization`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:81-96`, anchor `SpectralDistanceKind`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:138-141`, anchor `ApplyDetailed`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:332-339`, anchor `EnergyNormalization`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:381-390`, anchors `Normalize(SpectralEnergyNormalization energy` and `SpectralDistanceKind Distance`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:401`, anchor `EvaluateFilteredDetailed`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:455-470`, anchors `NormalizeDescriptor` and `energy`

### From

```csharp
public sealed partial class SpectralEnergyNormalization {
    [UseDelegateFromConstructor] internal partial Option<double[]> Rescale(double[] values);
```

```csharp
public sealed partial class SpectralDistanceKind {
```

```csharp
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, Op key, Option<SpectralDescriptorPolicy> policy = default) =>
```

```csharp
        from descriptor in SpectralKernel.EvaluateFilteredDetailed(basis: basis, sources: sources, filter: this, policy: active, key: key)
```

```csharp
public readonly record struct SpectralDescriptorPolicy(
    bool NormalizeScale,
    SpectralEnergyNormalization EnergyNormalization,
    bool IncludeZeroModes,
    Option<Dimension> CropCount)
```

```csharp
    public Fin<SpectralDescriptor> Normalize(SpectralEnergyNormalization energy, Op key) =>
        SpectralKernel.NormalizeDescriptor(descriptor: this, energy: energy, key: key);
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, SpectralDistanceKind Distance)
```

### To

```csharp
public sealed partial class DescriptorNormalization {
    [UseDelegateFromConstructor] internal partial Option<double[]> Apply(double[] values);
```

```csharp
public sealed partial class DescriptorDistance {
```

```csharp
    internal Fin<SpectralDescriptor> Evaluate(SpectralBasis basis, Option<Seq<int>> sources, Op key, Option<SpectralDescriptorPolicy> policy = default) =>
```

```csharp
        from descriptor in SpectralKernel.Evaluate(basis: basis, sources: sources, filter: this, policy: active, key: key)
```

```csharp
public readonly record struct SpectralDescriptorPolicy(
    bool NormalizeScale,
    DescriptorNormalization Normalization,
    bool IncludeZeroModes,
    Option<Dimension> CropCount)
```

```csharp
    public Fin<SpectralDescriptor> Normalize(DescriptorNormalization normalization, Op key) =>
        SpectralKernel.Normalize(descriptor: this, normalization: normalization, key: key);
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, DescriptorDistance Distance)
```

Apply the same exact token replacements through the owner rows and their private calls: `EnergyNormalization` -> `Normalization`, `energy` -> `normalization`, generated delegate argument `rescale:` -> `apply:`, `Rescale` -> `Apply`, `NormalizeDescriptor` -> `Normalize`, and `EvaluateFilteredDetailed` -> `Evaluate`.

### Effect

- Target fenced LOC: unchanged (**0**).
- Authored symbols: unchanged; the existing symbols are renamed in place.
- Naming: L1, L2, and z-score are descriptor-value normalizations, not three forms of energy; the distance roster is the descriptor distance itself, not a metatype called `Kind`; and the only evaluation surface no longer carries a false `Detailed` contrast.

### API and consumer proof

`UnitL1`, `UnitL2`, and `ZScore` all transform the descriptor value vector, while only one row relates directly to squared energy; `DescriptorNormalization` is therefore the stable whole-family term. `DescriptorDistance` owns the actual MathNet distance delegate. `ApplyDetailed` has no `Apply` or non-detailed sibling and returns a spectral descriptor, so `Evaluate` states its operation directly. `SpectralKernel.Normalize` and `Evaluate` are unambiguous inside their owner and remove the redundant `Descriptor`/`FilteredDetailed` restatement.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/segment.md`: both `filter.ApplyDetailed(...)` calls become `filter.Evaluate(...)`.
- Same-file cards and package prose adopt `DescriptorNormalization`, `DescriptorDistance`, `Normalization`, `Apply`, and `Evaluate`; no other code-fence consumer names the two renamed types.

## 5. Rename the distance bridge and delete its forwarding factory

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:96-102`, anchors `Compute(Lifted a, Lifted b)` and `internal readonly record struct Lifted`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:502-504`, anchor `Lifted lifted = Lifted.Of`

### From

```csharp
    [UseDelegateFromConstructor] internal partial double Compute(Lifted a, Lifted b);
```

```csharp
[StructLayout(LayoutKind.Auto)]
internal readonly record struct Lifted(double[] Raw, MathNet.Numerics.LinearAlgebra.Vector<double> Dense) {
    internal static Lifted Of(double[] values) => new(Raw: values, Dense: MathNet.Numerics.LinearAlgebra.CreateVector.DenseOfArray(values));
}
```

```csharp
        Lifted lifted = Lifted.Of([.. query.Values.AsIterable()]);
        SpectralRank[] ranks = [.. candidates.AsIterable()
            .Select((candidate, index) => new SpectralRank(Index: index, Distance: policy.Distance.Compute(a: lifted, b: Lifted.Of([.. candidate.Values.AsIterable()])), Descriptor: candidate))
```

### To

```csharp
    [UseDelegateFromConstructor] internal partial double Compute(DistanceOperand a, DistanceOperand b);
```

```csharp
[StructLayout(LayoutKind.Auto)]
internal readonly record struct DistanceOperand(double[] Raw) {
    internal MathNet.Numerics.LinearAlgebra.Vector<double> Dense { get; } = MathNet.Numerics.LinearAlgebra.CreateVector.DenseOfArray(Raw);
}
```

```csharp
        DistanceOperand queryOperand = new([.. query.Values.AsIterable()]);
        SpectralRank[] ranks = [.. candidates.AsIterable()
            .Select((SpectralDescriptor candidate, int index) => new SpectralRank(Index: index, Distance: policy.Distance.Compute(a: queryOperand, b: new([.. candidate.Values.AsIterable()])), Descriptor: candidate))
```

### Effect

- Target fenced LOC: unchanged (**0**).
- Authored symbols: **-1 internal member** (`Lifted.Of`); the type is renamed in place.
- Naming: `DistanceOperand` states the exact role of the dual raw/MathNet representation; `Lifted` no longer forces a reader to discover what was lifted or why.

### API and consumer proof

MathNet exposes eight selected distance families on `Vector<double>` and four on `double[]`, so the bridge legitimately holds both forms. Its `Of` method forwarded directly to the primary construction and had two same-file calls; the primary constructor is the one-hop spelling. No external consumer names the internal type.

### Ripples

- Same-file filter and descriptor package cards replace `Lifted` with `DistanceOperand`.
- Outside target: none.

## 6. Discharge the first nonzero eigenvalue once and keep only irreducible wave evidence

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:342-357`, anchor `public readonly record struct WaveProfile`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:411-421`, anchors `Option<double> firstNonZero` and `return WeightsOf`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:471-496`, anchors `WeightsOf`, `WaveWeightsOf`, `FirstNonZeroScale`, and the log-eigenvalue extrema

### From

```csharp
public readonly record struct WaveProfile(
    double Energy, double Bandwidth, Option<double> FirstNonZeroScale, int ZeroModeCount, int CroppedEigenpairCount,
    int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum,
    Option<double> MinLogEigenvalue, Option<double> MaxLogEigenvalue) : IValidityEvidence {
```

```csharp
            Option<double> maxLogEigenvalue = MaxLogEigenvalue;
```

```csharp
                ValidityClaim.Positive(value: Energy),
                ValidityClaim.Positive(value: Bandwidth),
                ZeroModeCount >= 0 && CroppedEigenpairCount >= NonZeroEigenpairCount && NonZeroEigenpairCount > 0,
                ValidityClaim.Positive(value: RawWeightSum),
                Math.Abs(value: NormalizedWeightSum - 1.0) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1, val2: CroppedEigenpairCount),
                FirstNonZeroScale.Map(static first => first > 0.0).IfNone(noneValue: true),
                MinLogEigenvalue.Map(min => maxLogEigenvalue.Map(max => min <= max).IfNone(noneValue: true)).IfNone(noneValue: true));
```

```csharp
    private static Fin<(double[] Weights, Option<WaveProfile> Wave)> WeightsOf(
        SpectralFilter filter, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount, Op key) =>
```

```csharp
    private static Option<(double[] Weights, WaveProfile Profile)> WaveWeightsOf(SpectralFilter.WaveCase wave, double[] eigenvalues, double firstNonZero, double zeroBand, int zeroModeCount, int croppedCount) {
```

```csharp
        return WeightsOf(filter: filter, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero,
                zeroBand: scaleNormalized ? zeroBand / scale : zeroBand, zeroModeCount: zeroModeCount, croppedCount: eigenIndices.Length, key: key)
```

```csharp
        double[] raw = [.. eigenvalues.Select(wave.Weight)];
        double sum = TensorPrimitives.Sum<double>(raw);
        if (!double.IsFinite(sum) || sum <= EpsilonPolicy.SqrtEpsilon) { return Option<(double[], WaveProfile)>.None; }
        double[] normalized = new double[raw.Length];
        TensorPrimitives.Divide<double>(raw, sum, normalized);
        double[] positiveLogs = [.. eigenvalues.Where(lambda => lambda > zeroBand).Select(static lambda => Math.Log(d: lambda))];
```

```csharp
        WaveProfile profile = new(
            Energy: wave.Energy.Value,
            Bandwidth: Math.Max(val1: wave.Bandwidth.Value, val2: EpsilonPolicy.ZeroTolerance),
            FirstNonZeroScale: firstNonZero > 0.0 ? Some(firstNonZero) : Option<double>.None,
            ZeroModeCount: zeroModeCount,
            CroppedEigenpairCount: croppedCount,
            NonZeroEigenpairCount: positiveLogs.Length,
            RawWeightSum: sum,
            NormalizedWeightSum: TensorPrimitives.Sum<double>(normalized),
            MinLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(TensorPrimitives.Min<double>(positiveLogs)),
            MaxLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(TensorPrimitives.Max<double>(positiveLogs)));
```

### To

```csharp
public readonly record struct WaveProfile(
    double FirstNonZeroScale, int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum,
    double MinLogEigenvalue, double MaxLogEigenvalue) : IValidityEvidence {
```

Delete the `maxLogEigenvalue` local; both extrema are required scalars after this move.

```csharp
                NonZeroEigenpairCount > 0,
                ValidityClaim.Positive(value: RawWeightSum),
                Math.Abs(value: NormalizedWeightSum - 1.0) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1, val2: NonZeroEigenpairCount),
                ValidityClaim.Positive(value: FirstNonZeroScale),
                ValidityClaim.Finite(value: MinLogEigenvalue),
                ValidityClaim.Finite(value: MaxLogEigenvalue) && MinLogEigenvalue <= MaxLogEigenvalue);
```

```csharp
    private static Fin<(double[] Weights, Option<WaveProfile> Wave)> WeightsOf(
        SpectralFilter filter, double[] eigenvalues, Option<double> firstNonZero, double zeroBand, Op key) =>
        filter is SpectralFilter.WaveCase wave
            ? firstNonZero.Bind((double first) => WaveWeightsOf(wave, eigenvalues, first, zeroBand))
                .Map(static ((double[] Weights, WaveProfile Profile) held) => (held.Weights, Wave: Some(held.Profile))).ToFin(key.InvalidResult())
            : ((double[])[.. eigenvalues.Select(filter.Weight)]) switch {
                double[] weights when TensorPrimitives.IsFiniteAll<double>(weights) => Fin.Succ<(double[], Option<WaveProfile>)>((weights, Option<WaveProfile>.None)),
                _ => Fin.Fail<(double[], Option<WaveProfile>)>(key.InvalidResult()),
            };
```

```csharp
    private static Option<(double[] Weights, WaveProfile Profile)> WaveWeightsOf(
        SpectralFilter.WaveCase wave, double[] eigenvalues, double firstNonZero, double zeroBand) {
```

```csharp
        return WeightsOf(filter: filter, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero,
                zeroBand: scaleNormalized ? zeroBand / scale : zeroBand, key: key)
```

```csharp
        double[] raw = [.. eigenvalues.Select((double lambda) => lambda > zeroBand ? wave.Weight(lambda) : 0.0)];
        double sum = TensorPrimitives.Sum<double>(raw);
        double[] positiveLogs = [.. eigenvalues.Where((double lambda) => lambda > zeroBand).Select(static (double lambda) => Math.Log(d: lambda))];
        if (!double.IsFinite(sum) || sum <= EpsilonPolicy.SqrtEpsilon || positiveLogs.Length == 0) { return Option<(double[], WaveProfile)>.None; }
        double[] normalized = new double[raw.Length];
        TensorPrimitives.Divide<double>(raw, sum, normalized);
```

```csharp
        WaveProfile profile = new(
            FirstNonZeroScale: firstNonZero,
            NonZeroEigenpairCount: positiveLogs.Length,
            RawWeightSum: sum,
            NormalizedWeightSum: TensorPrimitives.Sum<double>(normalized),
            MinLogEigenvalue: TensorPrimitives.Min<double>(positiveLogs),
            MaxLogEigenvalue: TensorPrimitives.Max<double>(positiveLogs));
```

### Effect

- Target fenced LOC: `29 -> 24` (**-5**).
- Authored symbols: **-4 public positional members** (`Energy`, `Bandwidth`, `ZeroModeCount`, `CroppedEigenpairCount`).
- Correctness: the existing `Option<double>` no longer crosses an impossible `double` parameter boundary. The wave arm discharges presence once, zeros descriptor-classified zero modes against the same scale-relative band used for its log census, explicitly refuses a selected band with no eigenvalue above that band, and a valid `WaveProfile` can no longer carry absent scale or log extrema beside a positive nonzero-mode count.
- Surface: wave energy/bandwidth remain on the owning `WaveCase`; zero-mode and crop counts remain on the enclosing `DescriptorProfile`. The nested evidence no longer stores four mirrors that could disagree with those owners.

### API and consumer proof

LanguageExt supplies no implicit `Option<double> -> double` egress. The caller holds `Option<double>` from `Find`, so `Option.Bind` discharges the scale at the wave boundary; the non-wave arm's typed array switch binds its one computed buffer without a forbidden `var` pattern and gates it before success. Positive total wave weight alone does **not** prove `positiveLogs` nonempty in the old body: `SpectralFilter.Weight` uses the absolute `SqrtEpsilon` cutoff while the descriptor uses the scale-relative `zeroBand`, leaving a real interval where a weight may be positive but the eigenvalue is still classified as a zero mode. The replacement gates the raw wave vector and the log extrema with the same `lambda > zeroBand` predicate; the explicit `positiveLogs.Length` check remains required before total `Min`/`Max` reads. Scaling the unit-sum tolerance by `NonZeroEigenpairCount` is then correct because every selected zero-mode weight is exactly zero. The `1.0` fallback in `firstNonZero.IfNone(1.0)` can survive only on a non-wave, non-scale-normalized path: scale normalization rejects absence first, and the wave arm discharges it through `Bind`.

### Ripples

- Same file: the `WeightsOf` call drops `zeroModeCount` and `croppedCount`; the enclosing `DescriptorProfile` construction retains both once.
- Outside target: none; no consumer reads any removed `WaveProfile` member.

## 7. Derive harmonic dimension and use the one nullspace threshold

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:204-233`, anchor `public readonly record struct HarmonicCensus`

### From

```csharp
public readonly record struct HarmonicCensus(
    Option<int> Genus, int ExpectedDimension, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double EpsRank, double SpectralRadius, double NullspaceThreshold,
```

```csharp
            int expected = ExpectedDimension;
            int boundaryComponentCount = BoundaryComponentCount;
```

```csharp
                EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0,
                Rank + Nullity == EdgeCount,
                ValidityClaim.CountExactly(count: BasisCount, expected: expected),
                Nullity >= expected,
```

```csharp
                PositiveStar1Count <= EdgeCount,
                Genus.Map(genus => expected == (2 * genus) + Math.Max(0, boundaryComponentCount - 1)).IfNone(expected == 0),
```

```csharp
                Math.Abs(value: SvdTolerance - NullspaceThreshold) <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: NullspaceThreshold),
                NullspaceThreshold <= (EpsRank * Math.Max(val1: 1.0, val2: SpectralRadius)) + EpsilonPolicy.SqrtEpsilon,
                MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon,
                MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon,
                MaxClosedResidual <= residualTolerance,
                MaxCoClosedResidual <= residualTolerance,
                Star1OrthonormalResidual <= residualTolerance,
```

### To

```csharp
public readonly record struct HarmonicCensus(
    Option<int> Genus, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount,
    double SvdTolerance, double EpsRank, double SpectralRadius,
```

```csharp
            int boundaryComponentCount = BoundaryComponentCount;
            long expected = Genus.Map((int genus) => (2L * genus) + Math.Max(0L, (long)boundaryComponentCount - 1L)).IfNone(0L);
```

```csharp
                EdgeCount >= 0 && Rank >= 0 && Nullity >= 0 && BasisCount >= 0 && ConstraintRows >= 0 && boundaryComponentCount >= 0,
                (long)Rank + Nullity == EdgeCount,
                BasisCount == expected,
                Nullity >= expected,
```

```csharp
                PositiveStar1Count >= 0 && PositiveStar1Count <= EdgeCount,
                Genus.Map(static (int genus) => genus >= 0).IfNone(noneValue: true),
```

```csharp
                ValidityClaim.Nonnegative(value: SpectralRadius),
                ValidityClaim.Finite(value: residualTolerance),
                SvdTolerance <= (EpsRank * Math.Max(val1: 1.0, val2: SpectralRadius)) + EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Finite(value: MinNullEigenvalue) && MinNullEigenvalue >= -EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Finite(value: MaxNullEigenvalue) && MaxNullEigenvalue >= MinNullEigenvalue - EpsilonPolicy.SqrtEpsilon,
                ValidityClaim.Nonnegative(value: MaxClosedResidual) && MaxClosedResidual <= residualTolerance,
                ValidityClaim.Nonnegative(value: MaxCoClosedResidual) && MaxCoClosedResidual <= residualTolerance,
                ValidityClaim.Nonnegative(value: Star1OrthonormalResidual) && Star1OrthonormalResidual <= residualTolerance,
```

### Effect

- Target fenced LOC: `12 -> 13` (**+1**); the two deleted fields fund most of the finite/nonnegative gates, and the remaining line proves the derived residual tolerance itself did not overflow.
- Authored symbols: **-2 public positional members** (`ExpectedDimension`, `NullspaceThreshold`).
- Semantics: topology is the sole authority for expected harmonic dimension, and `SvdTolerance` is the sole nullspace threshold instead of a second value constrained to equal it within another tolerance. The topology derivation and rank/nullity partition widen to `long` before arithmetic, so a malformed public record cannot throw under the branch's checked-overflow policy. Spectral radius, null-eigenvalue extrema, and all three residuals must now be finite, with residuals additionally nonnegative.

### API and consumer proof

No `libs/dotnet/` consumer reads either removed member. The current validity body already defines `ExpectedDimension = 2g + max(0,b-1)` and explicitly requires `SvdTolerance` to equal `NullspaceThreshold`; storing both values therefore permits disagreement only so the validator can reject it. `Meshing/dec` already carries the actual basis count and the topology inputs needed for the derivation. `ValidityClaim.Positive` already proves the two input tolerances finite; the added `Nonnegative`/`Finite` claims close the remaining NaN, infinity, negative-residual, and derived-tolerance-overflow paths before the scale-relative ladder is evaluated.

### Ripples

- Same file: the DEC card stops listing expected dimension and nullspace threshold as independent measurements.
- `libs/dotnet/Rasm/.planning/Meshing/dec.md`: the harmonic-census construction drops the two arguments; `HodgeWitness.IsValid` continues deriving its residual gate from `SvdTolerance` and `SpectralRadius` unchanged.

## 8. Admit the eigenvalue ordering every prefix operation assumes

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:293-296`, anchor `SpectralBasis.IsValid`

### From

```csharp
                TensorPrimitives.IsFiniteAll<double>(eigenvalues) && TensorPrimitives.Min<double>(eigenvalues) >= -zeroBand,
                Eigenvectors.ForAll(phi => vertexCount > 0 && phi.Count == vertexCount && TensorPrimitives.IsFiniteAll<double>(phi.AsSpan())));
```

### To

```csharp
                TensorPrimitives.IsFiniteAll<double>(eigenvalues) && TensorPrimitives.Min<double>(eigenvalues) >= -zeroBand,
                Eigenvalues.AsIterable().Zip(Eigenvalues.AsIterable().Skip(1)).All(static ((double First, double Second) pair) => pair.First <= pair.Second),
                Eigenvectors.ForAll(phi => vertexCount > 0 && phi.Count == vertexCount && TensorPrimitives.IsFiniteAll<double>(phi.AsSpan())));
```

### Effect

- Target fenced LOC: `2 -> 3` (**+1**).
- Authored symbols: unchanged.
- Correctness: `Find(first nonzero)`, prefix crop, zero-mode census, and the descriptor/eigenvector pairing now consume an admitted ascending spectrum rather than an undocumented producer convention.

### API and consumer proof

`SpectralBasis` is publicly constructible from two arrays and therefore cannot inherit `EigenSolution.Order` by implication. The descriptor kernel selects the first nonzero value and takes a prefix; both operations are wrong on an unsorted but otherwise finite carrier. LanguageExt supplies `Arr.AsIterable`; the branch's enabled implicit usings already supply LINQ `Zip`, `Skip`, and `All` (the same fence currently calls `Take` without an explicit import), so no helper, materialized comparison array, or redundant `using` is introduced.

### Ripples

- `libs/dotnet/Rasm/.planning/Meshing/dec.md`: no code change; its generalized eigensolve already supplies the ascending basis this gate documents.

## 9. Delete readiness mirrors and refuse non-normalization policy rewrites during ranking

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:337`, anchor `internal bool IsRaw`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:360-372`, anchor `public readonly record struct DescriptorProfile`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:467-470`, anchor `NormalizeForRanking`

### From

```csharp
    internal bool IsRaw => Equals(Raw);
```

```csharp
    public bool Pairwise => SourceCount > 0;
    public bool EnergyNormalized => !Policy.Normalization.Equals(DescriptorNormalization.Raw);
    public bool ScaleNormalized => Policy.NormalizeScale;
    public bool ComparisonReady => !Policy.IsRaw || Wave.IsSome;
    public bool IsValid => ValidityClaim.All(
        VertexCount > 0 && EigenpairCount > 0,
        CroppedEigenpairCount > 0 && CroppedEigenpairCount <= EigenpairCount,
        ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount,
        SourceCount >= 0 && SourceCount <= VertexCount,
        ValidityClaim.Evidence(evidence: Some(Policy)),
        ValidityClaim.Evidence(evidence: Wave));
```

```csharp
    private static Fin<SpectralDescriptor> NormalizeForRanking(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        descriptor.Profile.ComparisonReady && descriptor.Profile.Policy.Equals(policy)
            ? Fin.Succ(descriptor)
            : Normalize(descriptor: descriptor, normalization: policy.Normalization, key: key);
```

### To

```csharp
    public bool IsValid => ValidityClaim.All(
        Filter is not null && (Filter is SpectralFilter.WaveCase) == Wave.IsSome && VertexCount > 0 && EigenpairCount > 0,
        ZeroModeCount >= 0 && ZeroModeCount <= EigenpairCount && CroppedEigenpairCount > 0
            && CroppedEigenpairCount == Math.Min(
                Policy.CropCount.Map(static (Dimension count) => count.Value).IfNone(EigenpairCount),
                Policy.IncludeZeroModes ? EigenpairCount : EigenpairCount - ZeroModeCount)
            && (!Policy.NormalizeScale || ZeroModeCount < EigenpairCount)
            && (Wave is not { IsSome: true, Case: WaveProfile wave }
                || wave.NonZeroEigenpairCount == (Policy.IncludeZeroModes ? Math.Max(0, CroppedEigenpairCount - ZeroModeCount) : CroppedEigenpairCount)),
        SourceCount >= 0 && SourceCount <= VertexCount,
        ValidityClaim.Evidence(evidence: Some(Policy)),
        ValidityClaim.Evidence(evidence: Wave));
```

```csharp
    private static Fin<SpectralDescriptor> NormalizeForRanking(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        !(descriptor.Profile.Policy with { Normalization = policy.Normalization }).Equals(policy)
            ? Fin.Fail<SpectralDescriptor>(key.InvalidInput())
            : descriptor.Profile.Policy.Normalization.Equals(policy.Normalization)
                ? Fin.Succ(descriptor)
                : Normalize(descriptor: descriptor, normalization: policy.Normalization, key: key);
```

### Effect

- Target fenced LOC: `16 -> 18` (**+2**); five dead readiness members fund most of the policy/count and wave-prefix coupling, leaving two net evidence lines.
- Authored symbols: **-5 members** (`IsRaw`, `Pairwise`, `EnergyNormalized`, `ScaleNormalized`, `ComparisonReady`).
- Correctness: the profile now rejects a default/null filter, requires wave evidence exactly for the wave case, derives the selected prefix count from the policy, proves a requested scale normalization has a nonzero mode, and ties the wave profile's nonzero census to that prefix. Ranking may transform only the value-normalization axis whose values it still holds and refuses a scale, zero-mode, or crop mismatch that cannot be reconstructed after the eigenbasis has been discarded.

### API and consumer proof

The four public profile mirrors have no reader anywhere in `libs/dotnet/`; `ComparisonReady` has one private reader and changes only whether a raw descriptor is pointlessly copied before ranking. `SpectralFilter` is a generated class-shaped union, so its default is null and the previous profile fold never admitted that required slot; `Wave` is optional storage whose presence must coincide with `WaveCase`, not merely validate when supplied. Move 6 leaves no repeated energy, bandwidth, zero-count, or crop-count value to cross-check; the only surviving nested count, `NonZeroEigenpairCount`, is derived exactly from the sorted selected prefix (`crop - zero` when zero modes are included, otherwise the whole crop). The property pattern avoids a record-struct lambda capture, and keeping every dependent subtraction behind the same short-circuit count guard prevents checked-overflow on a malformed public record. More importantly, the old fallback calls `Normalize`, which changes only `Normalization`, yet then ranks descriptors even when the target requests different scale, zero-mode, or crop semantics. Record `with` derives the one compatibility probe from the policy itself and stores no second readiness fact.

### Ripples

- Same file: the descriptor card removes the four derived-property claims and states the ranking compatibility rule directly.
- Outside target: none.

## 10. Make descriptor normalization one-way and finite on both sides

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:59-71`, anchors `Scaled` and `Centered`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:455-459`, anchor `NormalizeDescriptor` (renamed `Normalize` by move 4)
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:510-513`, anchor `Rescaled`

### From

```csharp
        if (scale <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
```

```csharp
        if (sigma <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
```

```csharp
    internal static Fin<SpectralDescriptor> Normalize(SpectralDescriptor descriptor, DescriptorNormalization normalization, Op key) =>
        from valid in guard(descriptor.IsValid, key.InvalidInput()).ToFin()
        from values in Rescaled(values: [.. descriptor.Values.AsIterable()], normalization: normalization, key: key)
        let merged = descriptor.Profile.Policy with { Normalization = normalization }
        select new SpectralDescriptor(Values: new Arr<double>(values), Profile: descriptor.Profile with { Policy = merged });
```

```csharp
    private static Fin<double[]> Rescaled(double[] values, DescriptorNormalization normalization, Op key) =>
        TensorPrimitives.IsFiniteAll<double>(values)
            ? normalization.Apply(values: values).ToFin(key.InvalidResult())
            : Fin.Fail<double[]>(key.InvalidResult());
```

### To

```csharp
        if (!double.IsFinite(scale) || scale <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
```

```csharp
        if (!double.IsFinite(sigma) || sigma <= EpsilonPolicy.SqrtEpsilon) { return Option<double[]>.None; }
```

```csharp
    internal static Fin<SpectralDescriptor> Normalize(SpectralDescriptor descriptor, DescriptorNormalization normalization, Op key) =>
        !descriptor.IsValid || normalization is null ? Fin.Fail<SpectralDescriptor>(key.InvalidInput())
        : descriptor.Profile.Policy.Normalization.Equals(normalization) ? Fin.Succ(descriptor)
        : !descriptor.Profile.Policy.Normalization.Equals(DescriptorNormalization.Raw) ? Fin.Fail<SpectralDescriptor>(key.InvalidInput())
        : from values in Rescaled(values: [.. descriptor.Values.AsIterable()], normalization: normalization, key: key)
          let merged = descriptor.Profile.Policy with { Normalization = normalization }
          select new SpectralDescriptor(Values: new Arr<double>(values), Profile: descriptor.Profile with { Policy = merged });
```

```csharp
    private static Fin<double[]> Rescaled(double[] values, DescriptorNormalization normalization, Op key) =>
        TensorPrimitives.IsFiniteAll<double>(values)
            ? normalization.Apply(values: values).Filter(static (double[] normalized) => TensorPrimitives.IsFiniteAll<double>(normalized)).ToFin(key.InvalidResult())
            : Fin.Fail<double[]>(key.InvalidResult());
```

### Effect

- Target fenced LOC: `11 -> 13` (**+2**).
- Authored symbols: unchanged.
- Correctness: a normalized descriptor can no longer be relabeled `Raw` or transformed through a second noncommuting normalization; overflowed L1/L2/standard-deviation reductions and non-finite outputs refuse instead of producing a plausible zero vector.

### API and consumer proof

`Raw.Apply` is identity, so the previous method could stamp `Raw` onto already-normalized values without reconstructing the lost scale. Z-score followed by L1/L2 is likewise not equivalent to applying that target normalization to the original values. LanguageExt `Option.Filter` retains the existing absence carrier while adding the output gate; `TensorPrimitives.IsFiniteAll` is the catalogued vectorized predicate on the actual returned buffer. The same move preserves allocation-free `TensorPrimitives.Divide`/`Subtract` kernels.

### Ripples

- `SpectralDescriptor.Normalize` keeps its public shape but takes the move-4 `DescriptorNormalization`; it returns the existing instance on an idempotent request and a typed invalid-input fault on a forbidden second normalization.
- `Rank` inherits the same one-way rule through move 9; no external signature changes.

## 11. Inline the two one-hop admission/normalization helpers

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:138-142`, anchor `ApplyDetailed` (renamed `Evaluate` by move 4)
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:338-339`, anchor `SpectralDescriptorPolicy.Admit`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:422-424`, anchor `NormalizeValues`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:508-509`, anchor `private static Fin<double[]> NormalizeValues`

### From

```csharp
    internal Fin<SpectralDescriptor> Evaluate(SpectralBasis basis, Option<Seq<int>> sources, Op key, Option<SpectralDescriptorPolicy> policy = default) =>
        from _ in guard(basis.IsValid, key.InvalidInput()).ToFin()
        from active in SpectralDescriptorPolicy.Admit(policy: policy.IfNone(noneValue: SpectralDescriptorPolicy.Raw), key: key)
        from descriptor in SpectralKernel.Evaluate(basis: basis, sources: sources, filter: this, policy: active, key: key)
        select descriptor;
```

```csharp
    internal static Fin<SpectralDescriptorPolicy> Admit(SpectralDescriptorPolicy policy, Op key) =>
        guard(policy.IsValid, key.InvalidInput()).ToFin().Map(_ => policy);
```

```csharp
            .Bind(((double[] Weights, Option<WaveProfile> Wave) weighted) => Accumulated(basis: basis, eigenIndices: eigenIndices, weights: weighted.Weights, sourceSet: sourceSet, vertexCount: n, key: key)
                .Bind(values => NormalizeValues(values: values, policy: policy, key: key))
```

```csharp
    private static Fin<double[]> NormalizeValues(double[] values, SpectralDescriptorPolicy policy, Op key) =>
        Rescaled(values: values, normalization: policy.Normalization, key: key);
```

### To

```csharp
    internal Fin<SpectralDescriptor> Evaluate(SpectralBasis basis, Option<Seq<int>> sources, Op key, Option<SpectralDescriptorPolicy> policy = default) =>
        policy.IfNone(noneValue: SpectralDescriptorPolicy.Raw) switch {
            SpectralDescriptorPolicy active when basis.IsValid && active.IsValid => SpectralKernel.Evaluate(basis: basis, sources: sources, filter: this, policy: active, key: key),
            _ => Fin.Fail<SpectralDescriptor>(key.InvalidInput()),
        };
```

```csharp
            .Bind(weighted => Accumulated(basis: basis, eigenIndices: eigenIndices, weights: weighted.Weights, sourceSet: sourceSet, vertexCount: n, key: key)
                .Bind((double[] values) => Rescaled(values: values, normalization: policy.Normalization, key: key))
```

### Effect

- Target fenced LOC: `11 -> 7` (**-4**).
- Authored symbols: **-2 internal/private members** (`SpectralDescriptorPolicy.Admit`, `NormalizeValues`).
- Resolution: evaluation reaches the kernel directly from its one switch-based admission, and accumulation reaches the shared `Rescaled` owner in one hop.

### API and consumer proof

`Admit` had one caller and performed only `guard(...).ToFin().Map(_ => policy)`; the switch arm combines the two invalid-input predicates already owed at the same boundary and hands the scoped `active` value directly to the kernel, without a pure-value `Map` or a second range variable. A query expression cannot begin with `let`, so the replacement uses a legal switch expression. `NormalizeValues` had one caller and forwarded every argument unchanged to `Rescaled`. Neither helper carries independent semantics, reuse, policy, or boundary custody.

### Ripples

None outside the target.

## 12. Validate the distance row and remove the distance-check allocation

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:387-391`, anchor `SpectralRankingPolicy.IsValid`
- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:499-506`, anchor `RankNormalized`

### From

```csharp
    public bool IsValid => ValidityClaim.Evidence(evidence: Some(Descriptor));
```

```csharp
        return TensorPrimitives.IsFiniteAll<double>([.. ranks.Select(static rank => rank.Distance)]) ? Fin.Succ(toSeq(ranks)) : Fin.Fail<Seq<SpectralRank>>(key.InvalidResult());
```

### To

```csharp
    public bool IsValid => Distance is not null && ValidityClaim.Evidence(evidence: Some(Descriptor));
```

```csharp
        return Array.TrueForAll(ranks, static (SpectralRank rank) => double.IsFinite(rank.Distance)) ? Fin.Succ(toSeq(ranks)) : Fin.Fail<Seq<SpectralRank>>(key.InvalidResult());
```

### Effect

- Target fenced LOC: unchanged (**0**).
- Authored symbols: unchanged.
- Logic: a default/null smart-enum row refuses at policy admission, and finiteness checking no longer allocates a second `double[]` projection after the ranked array already exists.

### API and consumer proof

`DescriptorDistance` is a generated reference type; the current validity fold checks only the descriptor half and can let `default(SpectralRankingPolicy)` reach `policy.Distance.Compute`. `Array.TrueForAll` tests the existing `SpectralRank[]` in place. `TensorPrimitives.IsFiniteAll` remains the correct choice when the data is already a contiguous `double` span, but materializing such a span solely to invoke it is strictly more work here.

### Ripples

None outside the target.

## 13. Replace the optional composition-tolerance sentinel with `Option<double>`

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:178-185`, anchors `BoundaryCompositionTolerance = 0.0` and `BoundaryCompositionTolerance <= 0.0`

### From

```csharp
    double BoundaryCompositionTolerance = 0.0) : IValidityEvidence {
```

```csharp
        BoundaryCompositionTolerance <= 0.0 || BoundaryCompositionResidual <= BoundaryCompositionTolerance,
```

### To

```csharp
    Option<double> BoundaryCompositionTolerance = default) : IValidityEvidence {
```

```csharp
        BoundaryCompositionTolerance is not { IsSome: true, Case: double tolerance } || (ValidityClaim.Positive(tolerance) && BoundaryCompositionResidual <= tolerance),
```

### Effect

- Target fenced LOC: unchanged (**0**).
- Authored symbols: unchanged.
- Semantics: absence is no longer encoded as every nonpositive `double`, and a negative, NaN, or infinite supplied tolerance can no longer silently disable or vacuously satisfy the gate.

### API and consumer proof

LanguageExt `Option<double>` is the branch's settled absence carrier. DEC assembly has a real positive scale-derived tolerance and supplies `Some(compositionTolerance)`; edge-connection assembly has a separate `Symmetry` witness and supplies `None`. `ValidityClaim.Positive` includes the owning finite-value gate, unlike a bare `> 0.0` comparison that admits positive infinity. No consumer performs arithmetic on `BoundaryCompositionTolerance` outside the target validity fold.

### Ripples

- `libs/dotnet/Rasm/.planning/Meshing/dec.md`: `DecAssemblyOf` wraps its measured `compositionTolerance` in `Some`; `EdgeConnectionAssemblyOf` leaves the slot absent.

## 14. Cache the stateless monoid unit

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:119`, anchor `public static SpectralFilter Identity`

### From

```csharp
    public static SpectralFilter Identity => new IdentityCase();
```

### To

```csharp
    public static SpectralFilter Identity { get; } = new IdentityCase();
```

### Effect

- Target fenced LOC: unchanged (**0**).
- Authored symbols: unchanged.
- Runtime shape: the stateless partial-monoid unit is one process value rather than one allocation per empty fold or composition identity read.

### API and consumer proof

`IdentityCase` carries no payload and record equality already treats every instance identically. `Rasm.Compute/Tensor/blas` seeds every filter-chain fold with `SpectralFilter.Identity`; caching changes neither dispatch nor equality and removes repeated allocation at the live consumer.

### Ripples

None.

## 15. Inline the one-hop spectral-radius mirror into the zero-mode band

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:284-286`, anchors `internal double SpectralRadius` and `internal double ZeroBand`

### From

```csharp
    internal double SpectralRadius => Eigenvalues.IsEmpty ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(Eigenvalues.AsSpan()));
    internal double ZeroBand => EpsilonPolicy.SqrtEpsilon * Math.Max(val1: SpectralRadius, val2: EpsilonPolicy.ZeroTolerance);
```

### To

```csharp
    internal double ZeroBand => EpsilonPolicy.SqrtEpsilon * Math.Max(val1: Eigenvalues.IsEmpty ? 0.0 : Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(Eigenvalues.AsSpan())), val2: EpsilonPolicy.ZeroTolerance);
```

### Effect

- Target fenced LOC: `2 -> 1` (**-1**).
- Authored symbols: **-1 internal member** (`SpectralRadius`).
- Resolution: the only consumer reaches the scale-relative zero-mode band directly instead of traversing a private derived-value chain.

### API and consumer proof

`SpectralRadius` has exactly one reader, `ZeroBand`, and no independent semantic or consumer. `TensorPrimitives.MaxMagnitude` remains the one vectorized reduction, and the empty-basis branch remains intact so the reduction never sees an empty span. Every descriptor and validity reader already consumes `ZeroBand`, which is the domain result the radius exists solely to derive.

### Ripples

- Same file: the DEC-carrier card continues naming `ZeroBand` as the one scale-relative zero-mode threshold and stops treating spectral radius as a carrier surface.
- Outside target: none.

## 16. Remove the descriptor fence's unused generic-collection import

### Location

- `libs/dotnet/Rasm/.planning/Numerics/spectral.md:319`, anchor `using System.Collections.Generic;`

### From

```csharp
using System.Collections.Generic;
```

### To

Delete the import.

### Effect

- Target fenced LOC: `1 -> 0` (**-1**).
- Authored symbols: unchanged.
- Imports: the descriptor fence retains `System.Linq` for `Enumerable`, selection, ordering, and materialization; no move adds an import, and the filter fence still requires `Thinktecture` for its surviving keyless smart enums and generated union.

### API and consumer proof

No declaration or body in the descriptor fence names a type from `System.Collections.Generic`; every collection is an array, `Seq`, `Arr`, or `MemoryOwner`. Removing the unused namespace changes neither binding nor the implicit-using baseline.

### Ripples

None.

## Protected non-moves

- Keep `SpectralBasis.Truncate`: it is a valid basis operation with distinct materialized-result semantics; zero current consumers do not make the capability invalid.
- Keep `SpectralFilter.Biharmonic` and `SpectralFilter.CommuteTime`: both are standard named spectral operators, not coined aliases. Their canonical `Power` representation is an implementation fact, not a reason to erase the public mathematical vocabulary.
- Keep `SpectralFilter.Diffusion`: it is the admitted diffusion construction boundary over a positive time, not a redundant raw-rate spelling.
