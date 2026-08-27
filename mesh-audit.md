# 1. Derive quadratic facet tables from their linear rows

## Location

- `mesh.md:49-52`, anchors `CellTopology.Tet4`, `Tet10`, `Hex8`, and `Hex20`

## From

```csharp
public static readonly CellTopology Tet4  = new("tet4",  nodes: 4,  corners: 4, facets: [[0, 2, 1], [0, 1, 3], [1, 2, 3], [2, 0, 3]]);
public static readonly CellTopology Tet10 = new("tet10", nodes: 10, corners: 4, facets: [[0, 2, 1], [0, 1, 3], [1, 2, 3], [2, 0, 3]]);
public static readonly CellTopology Hex8  = new("hex8",  nodes: 8,  corners: 8, facets: [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]]);
public static readonly CellTopology Hex20 = new("hex20", nodes: 20, corners: 8, facets: [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]]);
```

## To

```csharp
public static readonly CellTopology Tet4  = new("tet4",  nodes: 4,  corners: 4, facets: [[0, 2, 1], [0, 1, 3], [1, 2, 3], [2, 0, 3]]);
public static readonly CellTopology Tet10 = new("tet10", nodes: 10, corners: 4, facets: Tet4.Facets);
public static readonly CellTopology Hex8  = new("hex8",  nodes: 8,  corners: 8, facets: [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]]);
public static readonly CellTopology Hex20 = new("hex20", nodes: 20, corners: 8, facets: Hex8.Facets);
```

## Why

The page already declares that a quadratic cell shares its linear parent's corner-facet table. Reading the earlier row makes that law executable and removes two maintained literal copies without adding a helper or changing either row's node/corner semantics.

# 2. Route all admission through the source discriminant

## Location

- `mesh.md:145-157`, anchors the two `MeshSpace.Of` overloads and `source.Map`

## From

```csharp
public static Fin<MeshSpace> Of(Mesh native, Context context, Option<MeshAssemblyPolicy> assembly = default, Op? key = null) =>
    Admit(native: native, provenance: static snapshot => new MeshSource.Native(Value: snapshot),
        context: context, assembly: assembly, key: key.OrDefault());
```

```csharp
return source.Map(
    native: arm => Admit(native: arm.Value, provenance: static snapshot => new MeshSource.Native(Value: snapshot),
        context: context, assembly: assembly, key: op),
    arena: arm => LiftArena(lanes: arm.Lanes, corners: arm.Corners, key: op)
        .Bind(mesh => Admit(native: mesh, provenance: _ => arm, context: context, assembly: assembly, key: op)),
    volume: arm => LiftVolume(lanes: arm.Lanes, cells: arm.Cells, topology: arm.Topology, key: op)
        .Bind(mesh => Admit(native: mesh, provenance: _ => arm, context: context, assembly: assembly, key: op)));
```

## To

```csharp
public static Fin<MeshSpace> Of(Mesh native, Context context, Option<MeshAssemblyPolicy> assembly = default, Op? key = null) =>
    Of(source: new MeshSource.Native(Value: native), context: context, assembly: assembly, key: key);
```

```csharp
return source.Switch(
    state: (Context: context, Assembly: assembly, Key: op),
    native: static (s, arm) => Admit(arm.Value, static snapshot => new MeshSource.Native(snapshot), s.Context, s.Assembly, s.Key),
    arena: static (s, arm) => LiftArena(arm.Lanes, arm.Corners, s.Key)
        .Bind(mesh => Admit(mesh, _ => arm, s.Context, s.Assembly, s.Key)),
    volume: static (s, arm) => LiftVolume(arm.Lanes, arm.Cells, arm.Topology, s.Key)
        .Bind(mesh => Admit(mesh, _ => arm, s.Context, s.Assembly, s.Key)));
```

## Why

The raw-mesh overload currently duplicates one generated-union arm's routing into `Admit`. Wrapping it in the existing discriminant makes `Of(MeshSource, ...)` the sole source router while its native arm still rebinds provenance to the defensive snapshot. The arms duplicate, lift, and admit meshes, so Thinktecture's func-form `Switch` is the correct lazy exhaustive dispatch; `Map` is reserved for already-computed result rows because it evaluates every supplied arm before selection. Its state overload also carries the shared context, policy, and operation into static arms instead of closing over them three times.

## Ripples

- Same file, `mesh.md:16`: replace the claim that `MeshSpace.Of` dispatches through generated `Map` with generated `Switch`.

# 3. Keep one operation identity across draft closure and admission

## Location

- `mesh.md:158-161`, anchor `MeshSpace.Accrue`

## From

```csharp
public static Fin<MeshSpace> Accrue(MeshDraft draft, Context context, Option<MeshAssemblyPolicy> assembly = default, Op? key = null) =>
    draft.Close(key: key.OrDefault())
        .Bind(closed => Of(source: new MeshSource.Arena(Lanes: closed.Lanes, Corners: closed.Corners),
            context: context, assembly: assembly, key: key));
```

## To

```csharp
public static Fin<MeshSpace> Accrue(MeshDraft draft, Context context, Option<MeshAssemblyPolicy> assembly = default, Op? key = null) =>
    draft.Close(key.OrDefault()) >> (closed =>
        Of(new MeshSource.Arena(closed.Lanes, closed.Corners), context, assembly, key.OrDefault()));
```

## Why

With a null key, `key.OrDefault()` in `Accrue` resolves to the `Accrue` caller-member value, while forwarding the original null into `Of` resolves again there as `Of`. Resolve at both call sites while they are still lexically inside `Accrue`; the operation-threading ruling guarantees those resolutions are value-identical, including inside the lambda. LanguageExt's `>>` then states the first-failure dependency directly and removes the `Bind` spelling and one fenced line.

# 4. Seat Laplacian routing on the public entry

## Location

- `mesh.md:179-180`, anchor `MeshSpace.Laplacian`
- `mesh.md:653-660`, anchor `MeshKernel.LaplacianOf`

## From

```csharp
public Fin<SparseLaplacian> Laplacian(MeshLaplacian kind, Op? key = null) =>
    MeshKernel.LaplacianOf(space: this, kind: kind, key: key.OrDefault());
```

```csharp
internal static Fin<SparseLaplacian> LaplacianOf(MeshSpace space, MeshLaplacian kind, Op key) =>
    from active in Optional(kind).ToFin(key.InvalidInput())
    from _ in active.RequiresQualityGate
        ? AspectRatioGuard(mesh: space.Native, ceiling: space.Assembly.AspectRatioCeiling, key: key)
        : Fin.Succ(unit)
    from result in active.Select(cache: space.Cache, key: key)
    select result;
```

## To

```csharp
public Fin<SparseLaplacian> Laplacian(MeshLaplacian kind, Op? key = null) {
    Op op = key.OrDefault();
    return from active in Optional(kind).ToFin(op.InvalidInput())
           from _ in active.RequiresQualityGate
               ? MeshKernel.AspectRatioGuard(Native, Assembly.AspectRatioCeiling, op)
               : Fin.Succ(unit)
           from result in active.Select(Cache, op)
           select result;
}
```

```csharp
// MeshKernel.LaplacianOf DELETED
```

## Why

`MeshKernel.LaplacianOf` has one caller and only sequences policy already owned by `MeshSpace` and the selected row. Inlining it removes a module-level forwarding member and leaves numeric assembly methods, rather than public-entry orchestration, on `MeshKernel`.

# 5. Collapse triangulation source into the selector row

## Location

- `mesh.md:221-241`, anchors `TriangulationSource` and `MeshLaplacian`

## From

```csharp
[SmartEnum<string>]
public sealed partial class TriangulationSource {
    public static readonly TriangulationSource Input = new("input");
    public static readonly TriangulationSource Intrinsic = new("intrinsic");
}

[SmartEnum<int>]
public sealed partial class MeshLaplacian {
    public static readonly MeshLaplacian Cotangent = new(key: 0, triangulation: TriangulationSource.Input,
        select: static (cache, key) => cache.Cotangent(key),
        snapshot: static (cache, key) => cache.EnsureFrozenIntrinsic(kind: MeshLaplacian.Cotangent, key: key));
    public static readonly MeshLaplacian IntrinsicDelaunay = new(key: 1, triangulation: TriangulationSource.Intrinsic,
        select: static (cache, key) => cache.IntrinsicDelaunay(key),
        snapshot: static (cache, key) => cache.IntrinsicMeshSnapshot(key: key));
    public static readonly MeshLaplacian TuftedIntrinsic = new(key: 2, triangulation: TriangulationSource.Intrinsic,
        select: static (cache, key) => cache.TuftedIntrinsic(key),
        snapshot: static (cache, key) => cache.TuftedIntrinsicMeshSnapshot(key: key));
    internal TriangulationSource Triangulation { get; }
    internal bool RequiresQualityGate => Triangulation == TriangulationSource.Input;
```

## To

```csharp
// TriangulationSource DELETED
```

```csharp
[SmartEnum<int>]
public sealed partial class MeshLaplacian {
    public static readonly MeshLaplacian Cotangent = new(key: 0, requiresQualityGate: true,
        select: static (cache, key) => cache.Cotangent(key),
        snapshot: static (cache, key) => cache.InputIntrinsicSnapshot(key));
    public static readonly MeshLaplacian IntrinsicDelaunay = new(key: 1, requiresQualityGate: false,
        select: static (cache, key) => cache.IntrinsicDelaunay(key),
        snapshot: static (cache, key) => cache.IntrinsicMeshSnapshot(key));
    public static readonly MeshLaplacian TuftedIntrinsic = new(key: 2, requiresQualityGate: false,
        select: static (cache, key) => cache.TuftedIntrinsic(key),
        snapshot: static (cache, key) => cache.TuftedIntrinsicMeshSnapshot(key));
    // MeshLaplacian.Triangulation DELETED
    internal bool RequiresQualityGate { get; }
```

## Why

`TriangulationSource` has no consumer outside `MeshLaplacian`; inside it, `Input` is read only to derive the quality boolean and choose the already-row-specific snapshot delegate. Making the boolean an honest row column and letting the delegate choose the snapshot deletes a two-row type, its generated surface, and one derived member without losing a selectable behavior. Keep `MeshLaplacian` keyed: unlike the deleted implementation-only axis, it is the public discretization vocabulary accepted by `Processing/intent`, so its stable keys remain genuine admission and serialization capability even though no in-repository consumer currently projects them.

## Ripples

- Same file, `mesh.md:188-190`: remove `TriangulationSource` from the owner, cases, and entry prose; describe the quality-gate column directly.
- Same file, `mesh.md:1175`: replace the triangulation-source column with the quality-gate and snapshot-delegate columns.

# 6. Give input and settled intrinsic snapshots one canonical cache each

## Location

- `mesh.md:538-539`, anchors `intrinsicMesh`, `tuftedIntrinsicMesh`, and `frozenIntrinsic`
- `mesh.md:583-591`, anchors the three intrinsic snapshot cache members
- `mesh.md:737-740`, anchor `MeshKernel.FrozenIntrinsicFor`

## From

```csharp
private readonly Memo<Unit, MeshKernel.IntrinsicMesh> intrinsicMesh = new(), tuftedIntrinsicMesh = new();
private readonly Memo<MeshLaplacian, MeshKernel.IntrinsicMesh> frozenIntrinsic = new();
```

```csharp
internal Fin<MeshKernel.IntrinsicMesh> EnsureFrozenIntrinsic(MeshLaplacian kind, Op key) =>
    frozenIntrinsic.Of(probe: kind, compute: () => MeshKernel.FrozenIntrinsicFor(mesh: space.Native, kind: kind, assembly: space.Assembly, key: key));
```

```csharp
internal static Fin<IntrinsicMesh> FrozenIntrinsicFor(Mesh mesh, MeshLaplacian kind, MeshAssemblyPolicy assembly, Op key) =>
    kind.Triangulation == TriangulationSource.Input
        ? IntrinsicMesh.FromMesh(mesh: mesh, key: key).Map(static source => source.Freeze())
        : BuildIntrinsicMesh(mesh: mesh, assembly: assembly, key: key);
```

## To

```csharp
private readonly Memo<Unit, MeshKernel.IntrinsicMesh> inputIntrinsic = new(), intrinsicMesh = new(), tuftedIntrinsicMesh = new();
// LaplacianCache.frozenIntrinsic DELETED
```

```csharp
internal Fin<MeshKernel.IntrinsicMesh> InputIntrinsicSnapshot(Op key) =>
    inputIntrinsic.Of(probe: unit, compute: () =>
        MeshKernel.IntrinsicMesh.FromMesh(mesh: space.Native, key: key).Map(static source => source.Freeze()));
// LaplacianCache.EnsureFrozenIntrinsic DELETED
```

```csharp
// MeshKernel.FrozenIntrinsicFor DELETED
```

## Why

The cotangent row is the sole input-triangulation consumer, while both geodesic paths request the settled intrinsic-Delaunay snapshot. Routing those paths through `IntrinsicMeshSnapshot` first removes the duplicate settled build; the remaining frozen-input cache then needs no selector key or branch. One unit-keyed memo per distinct snapshot preserves success-only reuse while deleting `frozenIntrinsic`, `EnsureFrozenIntrinsic`, and `FrozenIntrinsicFor`.

## Ripples

- `Processing/geodesics.md:883` and `Processing/geodesics.md:905`: replace both `space.Cache.EnsureFrozenIntrinsic(kind: MeshLaplacian.IntrinsicDelaunay, key: key)` calls with `space.Cache.IntrinsicMeshSnapshot(key: key)` so every settled-IDT consumer shares the canonical memo.

# 7. Remove the default-only tufted forwarding path

## Location

- `mesh.md:235-237`, anchor `MeshLaplacian.TuftedIntrinsic`
- `mesh.md:569-574`, anchors both `LaplacianCache.TuftedIntrinsic` overloads

## From

```csharp
select: static (cache, key) => cache.TuftedIntrinsic(key),
```

```csharp
internal Fin<SparseLaplacian> TuftedIntrinsic(Op key) => TuftedIntrinsic(policy: TuftedCoverPolicy.Default, key: key);
internal Fin<SparseLaplacian> TuftedIntrinsic(TuftedCoverPolicy policy, Op key) =>
```

## To

```csharp
select: static (cache, key) => cache.TuftedIntrinsic(policy: TuftedCoverPolicy.Default, key: key),
```

```csharp
// LaplacianCache.TuftedIntrinsic(Op) DELETED
internal Fin<SparseLaplacian> TuftedIntrinsic(TuftedCoverPolicy policy, Op key) =>
```

## Why

The parameterless-policy overload has one caller and contributes no admission, caching, or domain behavior. Putting the declared default at the selector row leaves one cache operation and makes the selected policy visible where the route is declared.

# 8. Inline the one-call tufted assembly wrapper

## Location

- `mesh.md:570-574`, anchor `LaplacianCache.TuftedIntrinsic(TuftedCoverPolicy, Op)`
- `mesh.md:728-730`, anchor `MeshKernel.AssembleTuftedCotangentFromIntrinsic`

## From

```csharp
tuftedIntrinsic.Of(probe: policy, compute: () =>
    from imesh in TuftedIntrinsicMeshSnapshot(key: key)
    from laplacian in MeshKernel.AssembleTuftedCotangentFromIntrinsic(imesh: imesh, space: space, policy: policy, key: key)
    select laplacian);
```

```csharp
internal static Fin<SparseLaplacian> AssembleTuftedCotangentFromIntrinsic(IntrinsicMesh imesh, MeshSpace space, TuftedCoverPolicy policy, Op key) =>
    TuftedCoverMesh.Construct(imesh: imesh, space: space, policy: policy, key: key)
        .Bind(cover => cover.Assemble(space: space, policy: policy, key: key));
```

## To

```csharp
tuftedIntrinsic.Of(probe: policy, compute: () =>
    from imesh in TuftedIntrinsicMeshSnapshot(key: key)
    from cover in MeshKernel.TuftedCoverMesh.Construct(imesh, space, policy, key)
    from laplacian in cover.Assemble(space, policy, key)
    select laplacian);
```

```csharp
// MeshKernel.AssembleTuftedCotangentFromIntrinsic DELETED
```

## Why

The helper is a one-call rename of `Construct >> Assemble`. Folding those dependent steps into the cache computation removes one module member and keeps the complete success-only computation at the memo that owns it.

# 9. Use `AtomHashMap` for keyed memo state

## Location

- `mesh.md:521-530`, anchors `LaplacianCache.Table` and nested `Memo<TKey, T>`

## From

```csharp
private static readonly ConditionalWeakTable<object, LaplacianCache> Table = [];
private sealed class Memo<TKey, T> {
    private readonly Atom<HashMap<TKey, T>> cache = Atom(value: HashMap<TKey, T>());
    internal Fin<T> Of(TKey probe, Func<Fin<T>> compute) =>
        cache.Value.Find(key: probe).Map(static value => Fin.Succ(value)).IfNone(() =>
            compute().Map(value =>
                Cell.Claim(cell: cache, key: probe, mint: () => value).Current.Find(key: probe).IfNone(noneValue: value)));
    internal bool Contains(TKey probe) => cache.Value.ContainsKey(key: probe);
}
```

## To

```csharp
private static readonly ConditionalWeakTable<Mesh, LaplacianCache> Table = [];
private sealed class Memo<TKey, T> where TKey : notnull {
    private readonly AtomHashMap<TKey, T> cache = AtomHashMap(HashMap<TKey, T>());
    internal Fin<T> Of(TKey probe, Func<Fin<T>> compute) =>
        cache.Find(probe).Map(static value => Fin.Succ(value)).IfNone(() =>
            compute().Map(value => cache.FindOrMaybeAdd(probe, () => Some(value)).IfNone(value)));
    // Memo.Contains DELETED
}
```

## Why

LanguageExt's keyed cell is the standard owner for shared per-key CAS state; wrapping a whole `HashMap` in `Atom` hand-rolls that grain. Its atomic `FindOrMaybeAdd` directly expresses first-success seating and returns the winner without a statement lambda or a second read. `Contains` has no caller anywhere in `libs/dotnet/`. Typing the weak table to its actual `Mesh` key removes the unrelated `object` erasure.

## Ripples

- Same file, `mesh.md:191`: replace the `Atom<HashMap>` plus `Cell.Claim` memo description with `AtomHashMap.FindOrMaybeAdd` success-only seating.

# 10. Replace the payloadless overlay roster with a policy boolean

## Location

- `mesh.md:282-286`, anchor `OverlayEmit`
- `mesh.md:319-330`, anchors `SignpostPolicy.Emit`, `Default`, and `Of`
- `mesh.md:1051-1055`, anchor the overlay emission choice in `BuildCommonSubdivision`

## From

```csharp
[SmartEnum<string>]
public sealed partial class OverlayEmit {
    public static readonly OverlayEmit Polygons = new("polygons");
    public static readonly OverlayEmit Triangles = new("triangles");
}
```

```csharp
public readonly record struct SignpostPolicy(
    CapabilitySet<TransportHalf> Halves, Option<Dimension> TraceMaxIters, Dimension TraceCapPerEdge,
    ToleranceLane RescaleFloor, SignpostGauge ReferenceDirectionGauge, OverlayEmit Emit) {
    public static readonly SignpostPolicy Default = new(
        Halves: CapabilitySet<TransportHalf>.Of(TransportHalf.Frames), TraceMaxIters: None,
        TraceCapPerEdge: Dimension.Create(value: 16), RescaleFloor: ToleranceLane.Collinear,
        ReferenceDirectionGauge: SignpostGauge.LowestVertexNeighbor, Emit: OverlayEmit.Triangles);
    public static Fin<SignpostPolicy> Of(CapabilitySet<TransportHalf> halves, int traceMaxIters,
        int traceCapPerEdge, ToleranceLane rescaleFloor, SignpostGauge referenceDirectionGauge,
        OverlayEmit emit, Op? key = null);
}
```

```csharp
(List<int[]> emitted, Arr<int> emittedA, Arr<int> emittedB) = policy.Emit == OverlayEmit.Triangles
    ? TriangulateOverlay(faces: faces, sourceA: sourceA, sourceB: sourceB)
    : (faces, sourceA, sourceB);
```

## To

```csharp
// OverlayEmit DELETED
```

```csharp
public readonly record struct SignpostPolicy(
    CapabilitySet<TransportHalf> Halves, Option<Dimension> TraceMaxIters, Dimension TraceCapPerEdge,
    ToleranceLane RescaleFloor, SignpostGauge ReferenceDirectionGauge, bool TriangulateOverlay) {
    public static readonly SignpostPolicy Default = new(
        Halves: CapabilitySet<TransportHalf>.Of(TransportHalf.Frames), TraceMaxIters: None,
        TraceCapPerEdge: Dimension.Create(value: 16), RescaleFloor: ToleranceLane.Collinear,
        ReferenceDirectionGauge: SignpostGauge.LowestVertexNeighbor, TriangulateOverlay: true);
    public static Fin<SignpostPolicy> Of(CapabilitySet<TransportHalf> halves, int traceMaxIters,
        int traceCapPerEdge, ToleranceLane rescaleFloor, SignpostGauge referenceDirectionGauge,
        bool triangulateOverlay, Op? key = null);
}
```

```csharp
(List<int[]> emitted, Arr<int> emittedA, Arr<int> emittedB) = policy.TriangulateOverlay
    ? TriangulateOverlay(faces: faces, sourceA: sourceA, sourceB: sourceB)
    : (faces, sourceA, sourceB);
```

## Why

`OverlayEmit` is a payloadless two-case family with one boolean branch and no key consumer, behavior column, serialization, or independent growth axis. The policy already owns the choice; a named boolean column removes one module-level type, two row symbols, and the generated surface while preserving polygon versus triangulated emission exactly.

## Ripples

- Same file: replace `OverlayEmit` prose with the `TriangulateOverlay` policy fact.

# 11. Replace the density roster with optional quadrature data

## Location

- `mesh.md:297-303`, anchor `PowerDensityPolicy`
- `mesh.md:333-336`, anchor `PowerClipPolicy.Density`
- `mesh.md:485-490`, anchor `PowerCensus.Density`
- `mesh.md:1093-1094`, anchor the density-derived policy argument in `RestrictedPowerCells`

## From

```csharp
[SmartEnum<int>]
public sealed partial class PowerDensityPolicy {
    public static readonly PowerDensityPolicy Constant            = new(key: 0, quadratureNodes: 0);
    public static readonly PowerDensityPolicy ScalarFanQuadrature = new(key: 1, quadratureNodes: 3);
    internal int QuadratureNodes { get; }
    internal bool RequiresField => QuadratureNodes > 0;
}
```

```csharp
internal readonly record struct PowerClipPolicy(
    double ClipBand, double DenomFloor, double AreaFloor, double EdgeBand,
    int KNearest, int MinPolygonVertices, PowerDensityPolicy Density) {
    internal static Fin<PowerClipPolicy> Of(double diagonal, double meanEdge, PowerDensityPolicy density, Op key);
}
```

```csharp
Tolerance AreaBand, Tolerance LengthBand, int KNearest, PowerDensityPolicy Density) : IValidityEvidence {
```

```csharp
from policy in PowerClipPolicy.Of(diagonal: box.Diagonal.Length, meanEdge: MeanEdgeLengthOf(mesh: space.Native),
    density: density.IsSome ? PowerDensityPolicy.ScalarFanQuadrature : PowerDensityPolicy.Constant, key: key)
```

## To

```csharp
// PowerDensityPolicy DELETED
```

```csharp
internal readonly record struct PowerClipPolicy(
    double ClipBand, double DenomFloor, double AreaFloor, double EdgeBand,
    int KNearest, int MinPolygonVertices, Option<Dimension> DensityQuadrature) {
    internal static Fin<PowerClipPolicy> Of(
        double diagonal, double meanEdge, Option<Dimension> densityQuadrature, Op key);
}
```

```csharp
Tolerance AreaBand, Tolerance LengthBand, int KNearest, Option<Dimension> DensityQuadrature) : IValidityEvidence {
```

```csharp
from policy in PowerClipPolicy.Of(diagonal: box.Diagonal.Length, meanEdge: MeanEdgeLengthOf(mesh: space.Native),
    densityQuadrature: density.Map(static _ => Dimension.Create(value: 3)), key: key)
```

## Why

The optional `ScalarField` is already the sole producer of this choice: absence means constant integration and presence means the fixed three-node fan rule. `PowerDensityPolicy` republishes that same discriminant as a two-row generated type, and neither its key nor its identity has a consumer. Carrying the positive quadrature count as `Option<Dimension>` preserves the policy and census evidence while deleting one module-level type, two row symbols, the derived presence member, and the generated smart-enum surface.

## Ripples

- Same file, `mesh.md:189` and `mesh.md:196`: replace the power-density roster and growth claims with the optional `DensityQuadrature` policy/census datum.

# 12. Inline the cover-aware validity predicate

## Location

- `mesh.md:354-375`, anchors `TuftedCover.CoverAware` and its only read in `IsValid`

## From

```csharp
public bool CoverAware => CoverFaces == 2 * IntrinsicFaces;
public bool IsValid => ValidityClaim.All(
```

```csharp
!CoverAware || (Laws.AdmitsAll(CapabilitySet<CoverLaw>.All)
```

## To

```csharp
// TuftedCover.CoverAware DELETED
public bool IsValid => ValidityClaim.All(
```

```csharp
CoverFaces != 2 * IntrinsicFaces || (Laws.AdmitsAll(CapabilitySet<CoverLaw>.All)
```

## Why

`CoverAware` is a public one-use alias for a two-field comparison and carries no independent evidence or consumer. Keeping the predicate at the validity row removes a member and the semantic indirection without weakening the structural guard.

# 13. Delete duplicate subdivision projections and their one-use alias

## Location

- `mesh.md:419-442`, anchors `SignpostTransport.CommonSubdivisionSegments`, `TracedPathEdgeCount`, `ExactCommonSubdivision`, and `IsValid`
- `mesh.md:986-994`, anchor the `SignpostTransport` construction in `TransportOf`

## From

```csharp
public readonly record struct SignpostTransport(
    CapabilitySet<TransportHalf> Halves, int VertexCount, int IntrinsicEdgeCount, int IntrinsicFlipCount,
    int FlipBudgetExhaustedEdges, int NormalCoordinateParityErrors, int SumNormalCoordinates,
    Option<SignpostFrameFacts> Frames, Option<int> CommonSubdivisionSegments, Option<int> TracedPathEdgeCount,
    Option<CommonSubdivision> Subdivision = default) : IValidityEvidence {
    public bool ExactCommonSubdivision =>
        Subdivision.IsSome && NormalCoordinateParityErrors == 0
        && CommonSubdivisionSegments.Exists(segments => segments == SumNormalCoordinates);
```

```csharp
Subdivision.IsSome == CommonSubdivisionSegments.IsSome,
Subdivision.IsSome == TracedPathEdgeCount.IsSome,
Subdivision.IsNone || ExactCommonSubdivision,
```

```csharp
Frames: frames.Map(static f => f.Facts),
CommonSubdivisionSegments: overlay.Map(static sub => sub.SumNormalCoordinates),
TracedPathEdgeCount: overlay.Map(static sub => sub.SourceEdgeCount),
Subdivision: overlay));
```

## To

```csharp
public readonly record struct SignpostTransport(
    CapabilitySet<TransportHalf> Halves, int VertexCount, int IntrinsicEdgeCount, int IntrinsicFlipCount,
    int FlipBudgetExhaustedEdges, int NormalCoordinateParityErrors, int SumNormalCoordinates,
    Option<SignpostFrameFacts> Frames, Option<CommonSubdivision> Subdivision = default) : IValidityEvidence {
    // SignpostTransport.ExactCommonSubdivision DELETED
```

```csharp
Subdivision.Map(sub => NormalCoordinateParityErrors == 0
    && sub.SumNormalCoordinates == SumNormalCoordinates).IfNone(true),
```

```csharp
Frames: frames.Map(static f => f.Facts),
Subdivision: overlay));
```

## Why

`CommonSubdivisionSegments` and `TracedPathEdgeCount` are lossless projections of `Subdivision.SumNormalCoordinates` and `Subdivision.SourceEdgeCount`, and no consumer reads either duplicate. Their only other role is to repeat `Subdivision.IsSome` in `IsValid`. Reading the held subdivision directly deletes two public record columns, the one-use `ExactCommonSubdivision` member, two constructor projections, and both correlated-presence claims while preserving the parity and normal-coordinate equality proof.

# 14. Delete the unconsumed Euler-presence alias

## Location

- `mesh.md:388-403`, anchor `public readonly record struct Topology` and `EulerValidated`

## From

```csharp
public bool EulerValidated => Genus.IsSome;
```

## To

```csharp
// Topology.EulerValidated DELETED
```

## Why

No `libs/dotnet/` consumer reads `EulerValidated`; consumers needing the fact already read the evidence-bearing `Option<int> Genus` directly. The property is only a second name for `Genus.IsSome`, so deleting it removes a public convenience member without losing the genus value, its absence semantics, or the projection rows.

## Ripples

- Same file, `mesh.md:193`: replace the `Euler-validated` derived-read claim with the `Genus` option's presence semantics.

# 15. Inline the one-use trace-cap projection

## Location

- `mesh.md:327-328`, anchor `SignpostPolicy.TraceCapFor`
- `mesh.md:1025`, anchor the `GeodesicTracePolicy.MaxSteps` assignment

## From

```csharp
internal int TraceCapFor(int edgeCount) =>
    TraceMaxIters.Map(static cap => cap.Value).IfNone(noneValue: Math.Max(1, edgeCount) * TraceCapPerEdge.Value);
```

```csharp
MaxSteps = Dimension.Create(value: policy.TraceCapFor(edgeCount: imesh.EdgeCount))
```

## To

```csharp
// SignpostPolicy.TraceCapFor DELETED
```

```csharp
MaxSteps = policy.TraceMaxIters.IfNone(
    Dimension.Create(value: Math.Max(1, imesh.EdgeCount) * policy.TraceCapPerEdge.Value))
```

## Why

The helper only unwraps an `Option<Dimension>` at one construction site. Keeping the carrier typed through `IfNone` removes the projection to `int`, the re-wrapping `Dimension.Create`, and one internal member.

# 16. Fold topology traits at their only construction site

## Location

- `mesh.md:918-929`, anchors `TopologyDetailed`'s `Traits` argument and `MeshKernel.TraitsOf`

## From

```csharp
Traits: TraitsOf(hasBoundary: hasBoundary || boundaryComponents > 0, closed: mesh.IsClosed,
    solid: mesh.IsSolid, manifold: manifold, oriented: oriented),
```

```csharp
private static CapabilitySet<MeshTrait> TraitsOf(bool hasBoundary, bool closed, bool solid, bool manifold, bool oriented) =>
    Seq((hasBoundary, MeshTrait.Boundary), (closed, MeshTrait.Closed), (solid, MeshTrait.Solid),
            (manifold, MeshTrait.Manifold), (oriented, MeshTrait.Oriented))
        .Fold(CapabilitySet<MeshTrait>.None, static (held, row) => row.Item1 ? held.With(row.Item2) : held);
```

## To

```csharp
Traits: Seq((hasBoundary || boundaryComponents > 0, MeshTrait.Boundary), (mesh.IsClosed, MeshTrait.Closed),
        (mesh.IsSolid, MeshTrait.Solid), (manifold, MeshTrait.Manifold), (oriented, MeshTrait.Oriented))
    .Fold(CapabilitySet<MeshTrait>.None, static (held, row) => row.Item1 ? held.With(row.Item2) : held),
```

```csharp
// MeshKernel.TraitsOf DELETED
```

## Why

The helper has one caller and only renames a five-row fold over values already in scope. Inlining retains the single `CapabilitySet` derivation while deleting a private member and the five-boolean helper signature.

## Ripples

- Same file, `mesh.md:194`: remove `TraitsOf` from the prose exemption for private helpers because the fold now sits at its only construction site.

# 17. Inline the input interpolation row

## Location

- `mesh.md:1056`, anchor the first `InterpolationOf` call
- `mesh.md:1076-1078`, anchor `InputRowOf`

## From

```csharp
return from a in InterpolationOf(points: points, columnCount: imesh.VertexCount, row: InputRowOf, key: key)
```

```csharp
private static Seq<(int Column, double Weight)> InputRowOf(OverlayPoint point) => point.Switch(
    sharedCase:   static c => Seq((c.Vertex, 1.0)),
    crossingCase: static c => Seq((c.TailA, 1.0 - c.ParameterA), (c.TipA, c.ParameterA)));
```

## To

```csharp
return from a in InterpolationOf(points: points, columnCount: imesh.VertexCount, row: static point => point.Switch(
               sharedCase: static c => Seq((c.Vertex, 1.0)),
               crossingCase: static c => Seq((c.TailA, 1.0 - c.ParameterA), (c.TipA, c.ParameterA))), key: key)
```

```csharp
// MeshKernel.InputRowOf DELETED
```

## Why

`InputRowOf` is passed once and has no meaning outside that interpolation matrix. Moving the generated-union fold into the row argument deletes a private member and leaves the matrix's source-coordinate law visible at its construction.

# 18. Inline the intrinsic interpolation rows

## Location

- `mesh.md:1057`, anchor the second `InterpolationOf` call
- `mesh.md:1079-1084`, anchors `IntrinsicRowOf` and `CrossingRowOf`

## From

```csharp
from b in InterpolationOf(points: points, columnCount: imesh.VertexCount,
    row: point => IntrinsicRowOf(point: point, imesh: imesh), key: key)
```

```csharp
private static Seq<(int Column, double Weight)> IntrinsicRowOf(OverlayPoint point, IntrinsicMesh imesh) => point.Switch(
    state: imesh,
    sharedCase:   static (_, c) => Seq((c.Vertex, 1.0)),
    crossingCase: static (m, c) => CrossingRowOf(edge: m.EdgeAt(index: c.EdgeB), parameter: c.ParameterB));
private static Seq<(int Column, double Weight)> CrossingRowOf(IntrinsicEdge edge, double parameter) =>
    Seq((edge.Lo, 1.0 - parameter), (edge.Hi, parameter));
```

## To

```csharp
from b in InterpolationOf(points: points, columnCount: imesh.VertexCount, row: point => point.Switch(
    state: imesh,
    sharedCase: static (_, c) => Seq((c.Vertex, 1.0)),
    crossingCase: static (m, c) => {
        IntrinsicEdge edge = m.EdgeAt(index: c.EdgeB);
        return Seq((edge.Lo, 1.0 - c.ParameterB), (edge.Hi, c.ParameterB));
    }), key: key)
```

```csharp
// MeshKernel.IntrinsicRowOf DELETED
// MeshKernel.CrossingRowOf DELETED
```

## Why

Both helpers exist only to serve this one row argument. The generated `Switch` threads `IntrinsicMesh` into a static case arm; projecting the edge there removes two private members and keeps the full intrinsic-coordinate derivation at the interpolation matrix.

# 19. Reuse the snapshot's memoized mean edge length

## Location

- `mesh.md:1092-1095`, anchor `PowerClipPolicy.Of` inside `RestrictedPowerCells`

## From

```csharp
from policy in PowerClipPolicy.Of(diagonal: box.Diagonal.Length, meanEdge: MeanEdgeLengthOf(mesh: space.Native),
    densityQuadrature: density.Map(static _ => Dimension.Create(value: 3)), key: key)
```

## To

```csharp
from policy in PowerClipPolicy.Of(diagonal: box.Diagonal.Length, meanEdge: space.Cache.MeanEdgeLength,
    densityQuadrature: density.Map(static _ => Dimension.Create(value: 3)), key: key)
```

## Why

`LaplacianCache` already owns the lazy mean-edge computation for the same defensive snapshot. Calling the kernel fold again in the power entry bypasses that owner and repeats an O(E) read; using the memo removes the duplicate computation with no new symbol or semantic change.
