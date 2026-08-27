# 1. Replace cache provenance vocabulary with its true-arm fact

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:44-49`, anchor `[SmartEnum<int>] public sealed partial class AssemblyOrigin`; `:94-97`, anchor `public readonly record struct SpectralBasisBundle`; `:390-401`, anchor `ComputeSpectralBasisDetailed`.

From:

```csharp
[SmartEnum<int>]
public sealed partial class AssemblyOrigin {
    public static readonly AssemblyOrigin Assembled = new(key: 0);
    public static readonly AssemblyOrigin Cached    = new(key: 1);
}
```

```csharp
public readonly record struct SpectralBasisBundle(
    SpectralBasis Basis, EigenSolution<double, Arr<double>> Eigen,
    AssemblyOrigin Origin, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default);
```

```csharp
Eigen: eigen, Origin: AssemblyOrigin.Assembled,
```

To:

```csharp
// AssemblyOrigin DELETED
```

```csharp
public readonly record struct SpectralBasisBundle(
    SpectralBasis Basis, EigenSolution<double, Arr<double>> Eigen,
    bool Cached, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default);
```

```csharp
Eigen: eigen, Cached: false,
```

Why: `AssemblyOrigin` is a two-case payload-free family with no behavior, keyed lookup, independent identity, or boundary representation. Its consumers ask only whether the bundle was cached. The `Cached` column carries that true-arm fact directly and deletes one public type, two public rows, and their generated key, roster, conversion, parse, and dispatch surface.

Ripples: `libs/dotnet/Rasm/.planning/Meshing/mesh.md:536-595` stamps the memoized return with `Cached: true`; `libs/dotnet/Rasm/.planning/Processing/segment.md:70-82,121-124` replaces `DescriptorSolve.Origin` with `Cached`. Delete `Segmentation.SpectralOrigin` and its `ResultOf` argument at `segment.md:434-440,778-784` instead of copying the same fact a second time; `Segmentation.Descriptor.Map(static (DescriptorSolve solve) => solve.Cached)` remains the one derivation if a consumer needs it. Remove `AssemblyOrigin` from `Processing/segment` prose.

# 2. Read factorization evidence from the eigen solution that owns it

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:94-97`, anchor `public readonly record struct SpectralBasisBundle`; `:396-401`, anchor the `SpectralBasisBundle` construction in `ComputeSpectralBasisDetailed`.

From:

```csharp
public readonly record struct SpectralBasisBundle(
    SpectralBasis Basis, EigenSolution<double, Arr<double>> Eigen,
    bool Cached, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default);
```

```csharp
Eigen: eigen, Cached: false,
SkippedDegenerateFaces: laplacian.SkippedDegenerateFaces, FactorNonZeros: eigen.Evidence.FactorNonZeros),
```

To:

```csharp
public readonly record struct SpectralBasisBundle(
    SpectralBasis Basis, EigenSolution<double, Arr<double>> Eigen,
    bool Cached, int SkippedDegenerateFaces = 0);
```

```csharp
Eigen: eigen, Cached: false,
SkippedDegenerateFaces: laplacian.SkippedDegenerateFaces),
// SpectralBasisBundle.FactorNonZeros DELETED
```

Why: `EigenSolution.Evidence` is the route-owned `PathEvidence`; its `FactorNonZeros` projection is the authoritative optional factor count. Copying that value onto the bundle creates a second independently carried public slot which can disagree with the eigen solve it allegedly describes. Reading `bundle.Eigen.Evidence.FactorNonZeros` preserves the capability while deleting the mirror.

Ripples: in `libs/dotnet/Rasm/.planning/Processing/segment.md:70-81,121-124`, delete `DescriptorSolve.FactorNonZeros`, its duplicate validity claim, and its constructor argument; read `solve.Eigen.Evidence.FactorNonZeros` when needed. At `segment.md:434-462,514,778-784`, delete the derived `Segmentation.FactorNonZeros` and `SegmentationRun.FactorNonZeros` slots and their assignments: both spectral routes already carry the owning `EigenSolution` through `Descriptor` or `Eigen`, so the factor evidence remains reachable without a third mirror.

# 3. Delete the empty edge-factor carrier

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:99`, anchor `internal readonly record struct EdgeConnectionFactor`.

From:

```csharp
internal readonly record struct EdgeConnectionFactor(CholeskySparse Factor, SpectralAssembly Assembly);
```

To:

```csharp
// EdgeConnectionFactor DELETED
```

Why: the record adds no admission, behavior, validity law, identity, or representation to the already named `(Factor, Assembly)` product. A named tuple preserves both projections and their correlation without paying a module-level type for a cache-local pair.

Ripples: in `libs/dotnet/Rasm/.planning/Meshing/mesh.md:542,595`, change the memo and return carrier to `(CholeskySparse Factor, SpectralAssembly Assembly)`. The inferred `heatFactor.Factor` and `heatFactor.Assembly` reads in `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:730-733` remain unchanged.

# 4. Complete the Hodge memo key without deleting its slot identity

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:261-263`, anchor `// --- [HODGE_POINT_EVALUATION]` and `HodgeSolutionKey`.

From:

```csharp
[StructLayout(LayoutKind.Auto)] internal readonly record struct HodgeSolutionKey(VectorField Source);
internal static Fin<HodgeDecomposition> HodgeSolutionOf(VectorField source, MeshSpace space, Context context, Op key);
```

To:

```csharp
[StructLayout(LayoutKind.Auto)]
internal readonly record struct HodgeSolutionKey(VectorField Source, Context Context);
internal static Fin<HodgeDecomposition> HodgeSolutionOf(VectorField source, MeshSpace space, Context context, Op key);
```

Why: the result depends on `context`: `HodgeDecomposeDetailed` threads it into the gauge solve and records its `ToleranceLane.Drift` row in the witness, so a source-only key can return a decomposition proved under another context. The named key must stay because `LaplacianCache.Memoized<TKey,T>` uses `typeof(TKey)` with `typeof(T)` to isolate each generic memo slot; replacing it with a structural tuple would erase that domain slot identity and could alias another memo using the same tuple/result types. Adding `Context` repairs equality without naively deleting a genuine cache owner.

# 5. Leave harmonic dimension evidence on HarmonicCensus

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:58-80`, anchor `public readonly record struct HodgeWitness`, `ExpectedDimension`, and the harmonic-presence claims.

From:

```csharp
public readonly record struct HodgeWitness(
    int ExpectedGenus, int ExpectedBoundaryComponents, int EdgeCount, int FiniteVectorCount,
    double ReconstructionResidual, double HarmonicEnergy, Tolerance ResidualSlack,
    GaugeFix ExactGauge, Option<HarmonicCensus> Harmonic) : IValidityEvidence {
    public int ExpectedDimension => (2 * ExpectedGenus) + Math.Max(val1: 0, val2: ExpectedBoundaryComponents - 1);
    public bool IsValid {
        get {
            int expected = ExpectedDimension; int edgeCount = EdgeCount;
```

```csharp
Harmonic.IsSome == (expected > 0),
```

```csharp
Harmonic.Map(h => h.IsValid && h.BasisCount == expected && h.EdgeCount == edgeCount)
```

To:

```csharp
// HodgeWitness.ExpectedGenus / ExpectedBoundaryComponents DELETED
public readonly record struct HodgeWitness(
    int EdgeCount, int FiniteVectorCount,
    double ReconstructionResidual, double HarmonicEnergy, Tolerance ResidualSlack,
    GaugeFix ExactGauge, Option<HarmonicCensus> Harmonic) : IValidityEvidence {
    public bool IsValid {
        get {
            int edgeCount = EdgeCount;
```

```csharp
// HodgeWitness.ExpectedDimension DELETED
// HodgeWitness.IsValid harmonic-presence claim DELETED
```

```csharp
Harmonic.Map((HarmonicCensus h) => h.IsValid && h.EdgeCount == edgeCount)
```

Why: `Numerics/spectral` makes `HarmonicCensus` the one owner of genus, boundary components, expected harmonic dimension, basis count, and their coupling. Re-deriving that dimension on `HodgeWitness` creates a second independently filled authority. The admitted `DiscreteCalculus` should prove whether harmonic evidence is required; this witness then validates only its Hodge-specific edge coupling. This deletes two public constructor fields, one public property, and the duplicate basis-dimension comparison.

Ripples: in `libs/dotnet/Rasm/.planning/Numerics/spectral.md:236-250`, make `DiscreteCalculus.IsValid` couple `Harmonic.IsSome` to the dimension derived from `Assembly.Genus` and `Assembly.BoundaryComponentCount`; that is the owner-level gate every consumer receives.

# 6. Validate component shape and finiteness on the component carrier

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:58-91`, anchors `EdgeCount`, `FiniteVectorCount`, their witness claims, and `HodgeDecomposition.IsValid`; `:242-254`, anchors the `HodgeWitnessOf` call and signature.

From:

```csharp
int EdgeCount, int FiniteVectorCount,
```

```csharp
ValidityClaim.CountAtLeast(count: EdgeCount, floor: 1),
ValidityClaim.CountExactly(count: FiniteVectorCount, expected: EdgeCount),
```

```csharp
Harmonic.Map(h => h.IsValid && h.EdgeCount == edgeCount)
    .IfNone(noneValue: true),
```

```csharp
ValidityClaim.CountExactly(count: Exact.Count, expected: Witness.EdgeCount),
ValidityClaim.CountExactly(count: Harmonic.Count, expected: Witness.EdgeCount),
ValidityClaim.CountExactly(count: CoExact.Count, expected: Witness.EdgeCount),
```

```csharp
HodgeWitness witness = HodgeWitnessOf(calculus: calculus, edgeCount: edgeCount, dAlpha: dAlpha,
```

```csharp
private static HodgeWitness HodgeWitnessOf(DiscreteCalculus calculus, int edgeCount, ReadOnlySpan<double> dAlpha,
```

To:

```csharp
// HodgeWitness.EdgeCount / FiniteVectorCount DELETED
public readonly record struct HodgeWitness(
    double ReconstructionResidual, double HarmonicEnergy, Tolerance ResidualSlack,
    GaugeFix ExactGauge, Option<HarmonicCensus> Harmonic) : IValidityEvidence {
```

```csharp
// HodgeWitness.IsValid edge-count and finite-count claims DELETED
Harmonic.Map(static (HarmonicCensus h) => h.IsValid).IfNone(noneValue: true),
```

```csharp
ValidityClaim.CountAtLeast(count: Exact.Count, floor: 1) && TensorPrimitives.IsFiniteAll<double>(Exact.AsSpan()),
ValidityClaim.CountExactly(count: Harmonic.Count, expected: Exact.Count)
    && (Harmonic.IsEmpty || TensorPrimitives.IsFiniteAll<double>(Harmonic.AsSpan())),
ValidityClaim.CountExactly(count: CoExact.Count, expected: Exact.Count) && TensorPrimitives.IsFiniteAll<double>(CoExact.AsSpan()),
Witness.Harmonic is not { IsSome: true, Case: HarmonicCensus census } || census.EdgeCount == Exact.Count,
```

```csharp
// HodgeWitnessOf.edgeCount DELETED
HodgeWitness witness = HodgeWitnessOf(calculus: calculus, dAlpha: dAlpha,
```

```csharp
private static HodgeWitness HodgeWitnessOf(DiscreteCalculus calculus, ReadOnlySpan<double> dAlpha,
```

Why: both witness integers are lossy mirrors of arrays the `HodgeDecomposition` already owns. The component carrier can derive the edge count from `Exact.Count`, require the other planes to match it, validate each zero-copy span directly, and couple a present `HarmonicCensus.EdgeCount` there. This removes two public witness members, the hidden finite-count fold, and the extra `HodgeWitnessOf` argument while making the evidence impossible to stale.

# 7. Collapse the default DEC overload into one Option-shaped entry

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:151-166`, anchor `// --- [DEC_OPERATORS]` and the two `Build` overloads.

From:

```csharp
internal static Fin<DiscreteCalculus> Build(MeshSpace space, Op key) =>
    Build(space: space, kind: MeshLaplacian.IntrinsicDelaunay, key: key);
internal static Fin<DiscreteCalculus> Build(MeshSpace space, MeshLaplacian kind, Op key) =>
    from activeKind in Optional(kind).ToFin(key.InvalidInput())
    from imesh in activeKind.Snapshot(cache: space.Cache, key: key)
```

To:

```csharp
// DecAssembly.Build(MeshSpace, Op) DELETED
internal static Fin<DiscreteCalculus> Build(
    MeshSpace space, Op key, Option<MeshLaplacian> kind = default) {
    MeshLaplacian activeKind = kind.IfNone(MeshLaplacian.IntrinsicDelaunay);
    return from imesh in activeKind.Snapshot(cache: space.Cache, key: key)
```

Why: the first overload is a forwarding member whose only job is to inject a default row. `Option<MeshLaplacian>` carries that modality on the one entrypoint, and `IfNone` resolves it before the dependent `Fin` query begins. This removes one module member without wrapping a pure default-selection step in a fake result carrier.

Ripples: `libs/dotnet/Rasm/.planning/Processing/intent.md:406` passes `kind: Some(intent.Kind)`; default callers in `Meshing/mesh` and `Processing/segment` continue to omit `kind`.

# 8. Traverse the optional harmonic solve instead of branching around the carrier

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:160-163`, anchor `from harmonic in topology.Genus.Exists` inside `Build`.

From:

```csharp
from harmonic in topology.Genus.Exists(genus => ((2 * genus) + Math.Max(0, topology.BoundaryComponents - 1)) > 0)
    ? HarmonicForms(calculus: dec, topology: topology, context: space.Tolerance, key: key).Map(Some)
    : Fin.Succ(Option<HarmonicOneFormBasis>.None)
```

To:

```csharp
from harmonic in topology.Genus
    .Filter((int genus) => ((2 * genus) + Math.Max(0, topology.BoundaryComponents - 1)) > 0)
    .TraverseM((int _) => HarmonicForms(calculus: dec, topology: topology, context: space.Tolerance, key: key)).As()
```

Why: `Option<T>.TraverseM` is the admitted inversion for an effect that runs only on the present arm: `None` lifts directly to `Fin<Option<HarmonicOneFormBasis>>`, while a positive-dimensional genus sequences the existing `Fin` solve. It removes the hand-built `Some`/`Fin.Succ(None)` branch and keeps absence semantics on the carrier that owns them.

# 9. Delete the one-use TripletStencil count projection

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:102-125`, anchors `internal int Count` and the loop in `Triplets`.

From:

```csharp
internal int Count => values.WrittenCount;
```

```csharp
for (int t = 0; t < Count; t++) { yield return (row[t], col[t], value[t]); }
```

To:

```csharp
// TripletStencil.Count DELETED
```

```csharp
for (int t = 0; t < values.WrittenCount; t++) { yield return (row[t], col[t], value[t]); }
```

Why: `Count` has one reader and only renames the buffer writer's authoritative committed-count property. Reading `WrittenCount` at the loop removes one internal member and keeps the bound attached to its actual owner.

# 10. Stop copying the input one-form before the Hodge kernel

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:224-245`, anchors `double[] omegaEdges`, the first `TensorPrimitives.Subtract`, and the `HodgeWitnessOf` call.

From:

```csharp
double[] omegaEdges = [.. omega];
```

```csharp
TensorPrimitives.Subtract<double>(x: omegaEdges, y: dAlpha, destination: exactRemoved);
```

```csharp
harmonic: harmonic, coExact: coExact, omega: omegaEdges,
```

To:

```csharp
// omegaEdges DELETED
```

```csharp
TensorPrimitives.Subtract<double>(x: omega.AsSpan(), y: dAlpha, destination: exactRemoved);
```

```csharp
harmonic: harmonic, coExact: coExact, omega: omega.AsSpan(),
```

Why: `Arr<double>.AsSpan()` is the direct contiguous read consumed by both `TensorPrimitives.Subtract` and the witness's `ReadOnlySpan<double>` parameter. Materializing the admitted immutable input as a second array adds one edge-plane allocation and one local with no ownership or lifetime benefit.

# 11. Keep harmonic basis rows on their immutable carrier

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:227-239`, anchors `double[][] basis`, `foreach (double[] form in basis)`, and both reads of `form` in `HodgeDecomposeDetailed`.

From:

```csharp
double[][] basis = [.. calculus.Harmonic.Map(static b => b.Forms).IfNone(noneValue: Arr<Arr<double>>.Empty)
    .AsIterable().Select(static form => (double[])[.. form])];
```

```csharp
foreach (double[] form in basis) {
    double coefficient = Star1Inner(left: exactRemoved, right: form, star1: calculus.Star1);
    harmonicEnergySquared += coefficient * coefficient;
    TensorPrimitives.MultiplyAdd<double>(x: form, y: coefficient, addend: harmonic, destination: harmonic);
}
```

To:

```csharp
Arr<Arr<double>> basis = calculus.Harmonic.Map(static (HarmonicOneFormBasis b) => b.Forms).IfNone(Arr<Arr<double>>.Empty);
```

```csharp
foreach (Arr<double> form in basis) {
    double coefficient = Star1Inner(left: exactRemoved, right: form.AsSpan(), star1: calculus.Star1);
    harmonicEnergySquared += coefficient * coefficient;
    TensorPrimitives.MultiplyAdd<double>(x: form.AsSpan(), y: coefficient, addend: harmonic, destination: harmonic);
}
```

Why: `HarmonicOneFormBasis.Forms` already owns every basis vector as `Arr<double>`, and both numeric consumers accept a zero-copy `ReadOnlySpan<double>`. Preserving that carrier removes one outer array and one edge-sized array allocation per harmonic form without changing the modified Gram-Schmidt result or minting an adapter.

# 12. Remove the second SolenoidalOf input copy

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:270-275`, anchor `private static Arr<double> SolenoidalOf`.

From:

```csharp
private static Arr<double> SolenoidalOf(HodgeDecomposition solved) {
    double[] plane = [.. solved.CoExact];
    double[] harmonic = [.. solved.Harmonic];
    TensorPrimitives.Add<double>(x: plane, y: harmonic, destination: plane);
    return new Arr<double>(plane);
}
```

To:

```csharp
private static Arr<double> SolenoidalOf(HodgeDecomposition solved) {
    double[] plane = [.. solved.CoExact];
    TensorPrimitives.Add<double>(x: plane, y: solved.Harmonic.AsSpan(), destination: plane);
    return new Arr<double>(plane);
}
```

Why: `CoExact` must be copied because it is the in-place destination returned from the fold; `Harmonic` is read-only input and already exposes the span `TensorPrimitives.Add` requires. Removing its materialization saves one edge-sized allocation and one fence line.

# 13. Delete the Hermitian matrix-kernel forwarding member

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:113-114`, anchor `TripletStencil.HermitianBlock`; `:320-321`, anchor the emission inside `EmitCrouzeixRaviartPair`.

From:

```csharp
internal void HermitianBlock(int order, int i, int j, double real, double imaginary, double diagonal) =>
    MatrixKernel.AddHermitianRealBlockTriplets(add: At, order: order, i: i, j: j, real: real, imaginary: imaginary, diagonal: diagonal);
```

```csharp
system.HermitianBlock(order: eCount, i: pair.I, j: pair.J,
    real: weight * pair.Sign * cosTheta * time, imaginary: -weight * pair.Sign * sinTheta * time, diagonal: weight * time);
```

To:

```csharp
// TripletStencil.HermitianBlock DELETED
```

```csharp
MatrixKernel.AddHermitianRealBlockTriplets(add: system.At, order: eCount, i: pair.I, j: pair.J,
    real: weight * pair.Sign * cosTheta * time,
    imaginary: -weight * pair.Sign * sinTheta * time, diagonal: weight * time);
```

Why: `HermitianBlock` has one reader and only renames the settled matrix-kernel operation. Passing `system.At` directly keeps `TripletStencil` as the accumulator while deleting an internal member and one resolution hop.

# 14. Inline the one-use Crouzeix-Raviart pair helper

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:302-305`, anchor the side loop in `HeatSystem`; `:314-322`, anchor `EmitCrouzeixRaviartPair`.

From:

```csharp
for (int side = 0; side < 3; side++)
    EmitCrouzeixRaviartPair(system: system, eCount: eCount, pair: face.CrouzeixPair(imesh: mesh, side: side), area: face.Area, time: time);
```

```csharp
private static void EmitCrouzeixRaviartPair(TripletStencil system, int eCount,
    (int I, int J, double Sign, double LA, double LB, double LOpp) pair, double area, double time) {
    double dot = (pair.LA * pair.LA) + (pair.LB * pair.LB) - (pair.LOpp * pair.LOpp);
    double weight = dot / (2.0 * area);
    double cosTheta = dot / (2.0 * pair.LA * pair.LB);
    double sinTheta = 2.0 * area / (pair.LA * pair.LB);
    MatrixKernel.AddHermitianRealBlockTriplets(add: system.At, order: eCount, i: pair.I, j: pair.J,
        real: weight * pair.Sign * cosTheta * time,
        imaginary: -weight * pair.Sign * sinTheta * time, diagonal: weight * time);
}
```

To:

```csharp
for (int side = 0; side < 3; side++) {
    (int i, int j, double sign, double la, double lb, double opposite) = face.CrouzeixPair(imesh: mesh, side: side);
    double dot = (la * la) + (lb * lb) - (opposite * opposite);
    double weight = dot / (2.0 * face.Area);
    double cosTheta = dot / (2.0 * la * lb);
    double sinTheta = 2.0 * face.Area / (la * lb);
    MatrixKernel.AddHermitianRealBlockTriplets(add: system.At, order: eCount, i: i, j: j,
        real: weight * sign * cosTheta * time,
        imaginary: -weight * sign * sinTheta * time, diagonal: weight * time);
}
// EmitCrouzeixRaviartPair DELETED
```

Why: the helper has one caller and no meaning outside the CR assembly loop; it only derives four scalars and forwards them to the matrix owner. Keeping that arithmetic inside the named statement kernel deletes one private member and makes the face-area authority visible at its use.

# 15. Express holonomy negation on the existing Arr carrier

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:276-280`, anchor `private static Arr<double> Negated`; `:374-378`, anchor the final `dBeta` projection in `DistributeHolonomy`.

From:

```csharp
private static Arr<double> Negated(Arr<double> values) {
    double[] plane = [.. values];
    TensorPrimitives.Negate<double>(x: plane, destination: plane);
    return new Arr<double>(plane);
}
```

```csharp
let dBeta = IntrinsicEdgeGradient(imesh: imesh, beta: beta)
select Negated(values: dBeta);
```

To:

```csharp
// Negated DELETED
```

```csharp
let dBeta = IntrinsicEdgeGradient(imesh: imesh, beta: beta)
select dBeta.Map(static (double value) => -value);
```

Why: `Negated` is a one-call allocation wrapper around the elementwise projection `Arr<T>.Map` already owns. Mapping on the immutable carrier preserves the result type and semantics while deleting one private member and four fence lines.

# 16. Bind invariant gates through LanguageExt guard

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:164-166`, anchor the final validity gate in `Build`; `:365-368`, anchor the topology gate in `DistributeHolonomy`.

From:

```csharp
from valid in calculus.IsValid ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
select calculus;
```

```csharp
from _ in topology.Traits.Admits(MeshTrait.Closed) && topology.BoundaryComponents == 0 && topology.Genus is { IsSome: true, Case: 0 }
    ? Fin.Succ(unit)
    : Fin.Fail<Unit>(key.Unsupported(inputType: typeof(MeshSpace), outputType: typeof(Arr<double>)))
```

To:

```csharp
from _ in guard(calculus.IsValid, key.InvalidResult())
select calculus;
```

```csharp
from _ in guard(topology.Traits.Admits(MeshTrait.Closed) && topology.BoundaryComponents == 0
    && topology.Genus is { IsSome: true, Case: 0 }, key.Unsupported(inputType: typeof(MeshSpace), outputType: typeof(Arr<double>)))
```

Why: both clauses hand-build the same `Fin<Unit>` success/failure gate that `Guard<Error,Unit>` is admitted to bind directly inside a `Fin` query. `guard` preserves the exact fault and dependent short-circuit semantics while deleting the unused `valid` binding and three branch-construction lines.

# 17. Project eigenpairs on the Seq carrier before constructing the basis

Location: `libs/dotnet/Rasm/.planning/Meshing/dec.md:396-400`, anchor the `SpectralBasis` construction in `ComputeSpectralBasisDetailed`.

From:

```csharp
Basis: new SpectralBasis(
    Eigenvalues: new Arr<double>([.. pairs.AsIterable().Select(static p => p.Eigenvalue)]),
    Eigenvectors: new Arr<Arr<double>>([.. pairs.AsIterable().Select(static p => p.Eigenvector)])),
```

To:

```csharp
Basis: new SpectralBasis(
    Eigenvalues: new Arr<double>([.. pairs.Map(static pair => pair.Eigenvalue)]),
    Eigenvectors: new Arr<Arr<double>>([.. pairs.Map(static pair => pair.Eigenvector)])),
```

Why: `pairs` is already a `Seq<(double Eigenvalue, Arr<double> Eigenvector)>`. Its carrier-native `Map` projects each column directly; lifting to `Iterable`, falling through to LINQ `Select`, and then materializing immediately adds two resolution hops without changing shape or evaluation order.
