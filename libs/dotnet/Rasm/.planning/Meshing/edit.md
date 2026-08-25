# [RASM_MESHING_EDIT]

`MeshEdit` owns the mutable-arena tier of the mesh substrate — the single-writer SoA build arena every mesh-rewriting owner constructs into, and `Kernels`, the weld/transform/diagonal primitive family over it. `MeshSpace` and `MeshEdit` are the kernel's only two mesh carriers: the immutable admission snapshot `mesh.md` owns and this page's predicate-gated build arena. Algorithms admit a `MeshSpace`, mutate one arena in place under the single-writer contract, and publish by freeze — `ToSpace` re-enters admission through `MeshSpace.Of`, the only path from build state to composable truth.

`MeshEdit`'s arena namespace is total — `Of`, every mutation verb, and every `Kernels` member mint no fault union; a defective build surfaces once, at the freeze seam, through `MeshSpace.Of`'s `Fin` rail behind one bulk finiteness pre-gate. Arenas bind ONE `Context` at admission and read every band off it through `ToleranceLane`, so none carries a tolerance number of its own. Storage is pooled struct-of-arrays: per-column arrays rented from `ArrayPool<T>.Shared` and grown by amortized doubling, so a million-vertex rewrite leases a handful of pooled columns, not a persistent-collection copy per operation. `CommunityToolkit.HighPerformance` composes the span planes, pooled staging, struct-action folds, and packed bitsets.

## [01]-[INDEX]

- [02]-[ARENA]: `ArenaPolicy` the policy row; `MeshEdit` the single-writer SoA build arena over one polymorphic `Of`, span reads, dirty-bitset mutation verbs, partition-disjoint folds, and the `ToSpace` freeze; `Kernels` the weld/transform/diagonal primitive family over the arena columns.
- [03]-[ARENA_LAW]: store-mutability and arena-concurrency contract sibling stores compose by name.
- [04]-[DENSITY_BAR]: one arena, one policy row, one kernel family — the owner/rail/case partition.

## [02]-[ARENA]

- Owner: `ArenaPolicy` the arena policy row — capacity seed, the weld LANE, and the parallel floor every arena fold derives from; `MeshEdit` the `sealed class` single-writer arena over pooled SoA columns rented from `ArrayPool<T>.Shared` and grown by amortized doubling; `Kernels` the static primitive family operating on the arena — union-find tolerance-grid weld, the determinant-derived affine transform pass carrying the orientation repair a reversing map owes, and the exact quad-diagonal split gate the mesh-ingress triangulation rides, its projection plane read off the Numerics `Axis.DominantOf` admission.
- Cases: `Of` discriminates on argument type — an already-admitted `MeshSpace` or a raw triangle soup with its own `Context` — one owner, not a name pair; the soup modality is the kernel's one triangle-soup adapter, folding per-page `Soup(MeshSpace)` copies into a single `DuplicateNative` with quad faces split through the exact diagonal gate. Mutation verbs dirty-mark their slots — `SetFace` is the corner rewrite the decimate edge-collapse and remesh edge-flip land on, face indices stable under mutation; read projections return frozen span views, never copies.
- Entry: arena admission is total, no rail — a `MeshSpace` is already-admitted truth and a raw soup's validity is decided at freeze. `ToSpace(Op key)` is the one publish seam: a one-pass `TensorPrimitives.IsFiniteAll<double>` bulk gate per coordinate column, live faces rebuilt into a native `Mesh`, orphaned vertices compacted, then re-admission through `MeshSpace.Of` under the arena's OWN bound context; the finiteness gate routes `GeometryFault.DegenerateInput` with the offending slot, every other failure `MeshSpace.Of`'s own rail. `Kernels.WeldDuplicates` welds in place at `Context.For(ArenaPolicy.Weld)`, sweeping to a merge-free pass, total.
- Auto: `Of(MeshSpace)` calls `DuplicateNative` once, triangulates quads through the exact diagonal gate, and bulk-fills the columns; capacity grows by doubling every column together, so the offset-page under-allocation class — a store sized `2n` where the algorithm writes past it — is structurally impossible. `KillFace` tombstones a row and `ToSpace` compacts, the sentinel arena-internal and never observable past the freeze; dirty tracking is two packed bitsets replacing persistent `Set<int>` accumulation, enumerated for incremental consumers and cleared once the admission fill completes so a set bit names a kernel edit and never the build; `Parallel` runs a caller struct action over a caller-named index extent via `ParallelHelper`, allocation-free with the floor a policy row; the per-corner UV pair rents lazily on first `SetCornerUv`, rides faces so a weld never disturbs it, ingests per-vertex native texture coordinates at `Of(MeshSpace)`, and publishes wedge-faithfully at the freeze — a seam vertex whose corners disagree splits once per distinct UV, so no island's UV overwrites another's.
- Law: the arena is build state, never evidence — the `MeshSpace` the freeze publishes is the hash-eligible artifact, and dirty bitsets are working state a consumer projects.
- Packages: CommunityToolkit.HighPerformance (span planes, `ArrayPool` rent-resize, pooled kernel staging, `ParallelHelper` struct-action folds, packed bitsets), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll<double>` the freeze gate), `Rasm.Meshing` (`MeshSpace`/`MeshSpace.Of` freeze re-admission and native `Mesh` rebuild), `Rasm.Numerics` (`Predicate.Orient2D` + `Axis` the exact quad-diagonal gate, `Dimension` the count carrier, `GeometryFault` the freeze fault), Rasm.Domain (`Context`, `ToleranceLane`, `Op`, `Kind`), LanguageExt.Core (`Fin`/`Option` the freeze rails), Rhino.Geometry (`Mesh`/`MeshFace`/`Point3d` native seam, `Transform`/`Transform.Determinant` the transform pass's map and its orientation discriminant), BCL inbox (`ArrayPool<T>`).
- Growth: a new bulk mutation — an edge-split pass, a tangential-relax sweep, a further per-vertex or per-corner attribute column — is one arena verb or one further SoA column on the same rent/resize/dirty machinery the UV pair already rides; a new build primitive is one `Kernels` member over the same columns; a new parallel fold is one struct action; a new band is one `ToleranceLane` row read off the bound context; zero new carriers.
- Boundary: `MeshEdit.Of` owns the kernel's one triangle-soup adapter, every consumer composing it rather than a per-page `Soup(MeshSpace)` copy; the weld kernel lives here and its band is `ToleranceLane.Weld` read off the arena's bound `Context` — dedup-on-arena is an arena op, reached through no healing policy and carrying no tolerance number of its own; the transform pass owns MIRRORED geometry estate-wide, so a consumer needing a reflected part builds it as an admitted mesh through `Kernels.Apply(MeshEdit.Of(space), Transform.Mirror(plane))` and never places an admitted mesh under a reversing transform, which silently inverts the orientation its admission just proved; the arena binds ONE context for its lifetime, so a freeze under a different tolerance regime is a second `MeshSpace.Of` at the mesh owner, never a second context on this seam; in-place span kernels inside `MeshEdit`/`Kernels` are the arena tier's statement exemption, never leaking past the freeze, so every public egress is a span view, a value, or the `Fin<MeshSpace>` rail.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Meshing;

// --- [CONSTANTS] -----------------------------------------------------------------------
public sealed record ArenaPolicy(Dimension Capacity, ToleranceLane Weld, Dimension ParallelFloor) {
    public static readonly ArenaPolicy Canonical = new(
        Capacity: Dimension.Create(value: 1_024), Weld: ToleranceLane.Weld, ParallelFloor: Dimension.Create(value: 4_096));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed class MeshEdit : IDisposable {
    double[] x, y, z;
    int[] tri;
    double[]? uvU, uvV;
    ulong[] dirtyVertex, dirtyFace;
    int vertexCount, faceCount;
    readonly ArenaPolicy policy;
    readonly Context context;

    MeshEdit(Context context, ArenaPolicy policy) {
        (this.context, this.policy) = (context, policy);
        int seed = policy.Capacity.Value;
        x = ArrayPool<double>.Shared.Rent(seed);
        y = ArrayPool<double>.Shared.Rent(seed);
        z = ArrayPool<double>.Shared.Rent(seed);
        tri = ArrayPool<int>.Shared.Rent(3 * seed);
        dirtyVertex = ArrayPool<ulong>.Shared.Rent((seed >> 6) + 1);
        dirtyFace = ArrayPool<ulong>.Shared.Rent((seed >> 6) + 1);
        Array.Clear(dirtyVertex);
        Array.Clear(dirtyFace);
    }

    // --- [ADMISSION]
    public static MeshEdit Of(MeshSpace space, Option<ArenaPolicy> policy = default) {
        Mesh native = space.DuplicateNative();
        MeshEdit edit = new(context: space.Tolerance, policy: policy.IfNone(noneValue: ArenaPolicy.Canonical));
        for (int v = 0; v < native.Vertices.Count; v++) {
            Point3f p = native.Vertices[v];
            edit.AddVertex(new Point3d(p.X, p.Y, p.Z));
        }
        for (int f = 0; f < native.Faces.Count; f++) {
            MeshFace face = native.Faces.GetFace(f);
            if (face.IsTriangle) { edit.AddFace(face.A, face.B, face.C); continue; }
            if (Kernels.QuadDiagonal(edit.Position(face.A), edit.Position(face.B), edit.Position(face.C), edit.Position(face.D))) {
                edit.AddFace(face.A, face.B, face.C);
                edit.AddFace(face.A, face.C, face.D);
            }
            else {
                edit.AddFace(face.A, face.B, face.D);
                edit.AddFace(face.B, face.C, face.D);
            }
        }
        if (native.TextureCoordinates.Count == native.Vertices.Count) {
            for (int f = 0; f < edit.faceCount; f++) {
                (int a, int b, int c) = edit.Face(f);
                edit.SetCornerUv(f, UvAt(native, a), UvAt(native, b), UvAt(native, c));
            }
        }
        edit.Baseline();
        return edit;
    }

    static Point2d UvAt(Mesh mesh, int v) => new(mesh.TextureCoordinates[v].X, mesh.TextureCoordinates[v].Y);

    public static MeshEdit Of(ReadOnlySpan<Point3d> vertices, ReadOnlySpan<(int A, int B, int C)> faces,
        Context context, Option<ArenaPolicy> policy = default) {
        MeshEdit edit = new(context: context, policy: policy.IfNone(noneValue: ArenaPolicy.Canonical));
        foreach (Point3d p in vertices) edit.AddVertex(p);
        foreach ((int a, int b, int c) in faces) edit.AddFace(a, b, c);
        edit.Baseline();
        return edit;
    }

    void Baseline() { Array.Clear(dirtyVertex); Array.Clear(dirtyFace); }

    // --- [READ_SURFACE]
    public ArenaPolicy Policy => policy;
    public Context Tolerance => context;
    public int VertexCount => vertexCount;
    public int FaceCount => faceCount;
    public ReadOnlySpan<double> X => x.AsSpan(0, vertexCount);
    public ReadOnlySpan<double> Y => y.AsSpan(0, vertexCount);
    public ReadOnlySpan<double> Z => z.AsSpan(0, vertexCount);
    public ReadOnlyMemory2D<int> Faces => tri.AsMemory(0, 3 * faceCount).AsMemory2D(faceCount, 3);
    public Point3d Position(int v) => new(x[v], y[v], z[v]);
    public (int A, int B, int C) Face(int f) => (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]);
    public bool Alive(int f) => tri[3 * f] >= 0;
    public Option<(Point2d A, Point2d B, Point2d C)> CornerUv(int f) =>
        uvU is null ? None
            : Some((new Point2d(uvU[3 * f], uvV![3 * f]), new Point2d(uvU[3 * f + 1], uvV[3 * f + 1]),
                    new Point2d(uvU[3 * f + 2], uvV[3 * f + 2])));
    public BoundingBox Bounds(int f) =>
        new([Position(tri[3 * f]), Position(tri[3 * f + 1]), Position(tri[3 * f + 2])]);

    // --- [MUTATION]
    public int AddVertex(Point3d p) {
        Grow(ref x, vertexCount + 1); Grow(ref y, vertexCount + 1); Grow(ref z, vertexCount + 1);
        GrowBits(ref dirtyVertex, vertexCount + 1);
        (x[vertexCount], y[vertexCount], z[vertexCount]) = (p.X, p.Y, p.Z);
        BitHelper.SetFlag(ref dirtyVertex[vertexCount >> 6], vertexCount & 63, true);
        return vertexCount++;
    }

    public int AddFace(int a, int b, int c) {
        Grow(ref tri, 3 * (faceCount + 1));
        GrowBits(ref dirtyFace, faceCount + 1);
        (tri[3 * faceCount], tri[3 * faceCount + 1], tri[3 * faceCount + 2]) = (a, b, c);
        BitHelper.SetFlag(ref dirtyFace[faceCount >> 6], faceCount & 63, true);
        return faceCount++;
    }

    public void SetPosition(int v, Point3d p) {
        (x[v], y[v], z[v]) = (p.X, p.Y, p.Z);
        BitHelper.SetFlag(ref dirtyVertex[v >> 6], v & 63, true);
    }

    public void SetFace(int f, int a, int b, int c) {
        (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]) = (a, b, c);
        BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    }

    public void SetCornerUv(int f, Point2d a, Point2d b, Point2d c) {
        uvU ??= ArrayPool<double>.Shared.Rent(tri.Length);
        uvV ??= ArrayPool<double>.Shared.Rent(tri.Length);
        Grow(ref uvU, 3 * (f + 1)); Grow(ref uvV, 3 * (f + 1));
        (uvU[3 * f], uvV[3 * f]) = (a.X, a.Y);
        (uvU[3 * f + 1], uvV[3 * f + 1]) = (b.X, b.Y);
        (uvU[3 * f + 2], uvV[3 * f + 2]) = (c.X, c.Y);
        BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    }

    public void KillFace(int f) {
        (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]) = (-1, -1, -1);
        BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    }

    public void Touch(ReadOnlySpan<int> faces, ReadOnlySpan<int> vertices) {
        foreach (int f in faces) BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
        foreach (int v in vertices) BitHelper.SetFlag(ref dirtyVertex[v >> 6], v & 63, true);
    }

    public bool DirtyVertex(int v) => BitHelper.HasFlag(dirtyVertex[v >> 6], v & 63);
    public bool DirtyFace(int f) => BitHelper.HasFlag(dirtyFace[f >> 6], f & 63);

    public IEnumerable<int> DirtyVertices() { for (int v = 0; v < vertexCount; v++) if (DirtyVertex(v)) yield return v; }
    public IEnumerable<int> DirtyFaces() { for (int f = 0; f < faceCount; f++) if (DirtyFace(f)) yield return f; }

    // --- [PARALLEL]
    public void Parallel<TAction>(int extent, in TAction action) where TAction : struct, IAction =>
        ParallelHelper.For(0, extent, in action, policy.ParallelFloor.Value);

    // --- [FREEZE]
    public Fin<MeshSpace> ToSpace(Op key) {
        if (NonFinite().Case is int slot) {
            return Fin.Fail<MeshSpace>(new GeometryFault.DegenerateInput(
                Kind.Mesh, slot, "non-finite arena coordinate"));
        }
        Mesh mesh = new();
        if (uvU is null) {
            for (int v = 0; v < vertexCount; v++) mesh.Vertices.Add(x[v], y[v], z[v]);
            for (int f = 0; f < faceCount; f++) {
                if (Alive(f)) mesh.Faces.AddFace(tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]);
            }
        }
        else {
            SplitWedges(mesh);
        }
        mesh.Compact();
        mesh.RebuildNormals();
        return MeshSpace.Of(native: mesh, context: context, key: key);
    }

    void SplitWedges(Mesh mesh);

    public void Dispose() {
        ArrayPool<double>.Shared.Return(x); ArrayPool<double>.Shared.Return(y); ArrayPool<double>.Shared.Return(z);
        ArrayPool<int>.Shared.Return(tri);
        if (uvU is not null) { ArrayPool<double>.Shared.Return(uvU); ArrayPool<double>.Shared.Return(uvV!); }
        ArrayPool<ulong>.Shared.Return(dirtyVertex); ArrayPool<ulong>.Shared.Return(dirtyFace);
    }

    internal void Compact(int classes, ReadOnlySpan<double> sumX, ReadOnlySpan<double> sumY, ReadOnlySpan<double> sumZ, ReadOnlySpan<int> classSize, ReadOnlySpan<int> remap) {
        for (int w = 0; w < classes; w++) {
            (x[w], y[w], z[w]) = (sumX[w] / classSize[w], sumY[w] / classSize[w], sumZ[w] / classSize[w]);
            BitHelper.SetFlag(ref dirtyVertex[w >> 6], w & 63, true);
        }
        for (int f = 0; f < faceCount; f++) {
            if (!Alive(f)) continue;
            (int a, int b, int c) = (remap[tri[3 * f]], remap[tri[3 * f + 1]], remap[tri[3 * f + 2]]);
            if (a == b || b == c || c == a) { KillFace(f); continue; }
            (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]) = (a, b, c);
            BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
        }
        vertexCount = classes;
    }

    Option<int> NonFinite() {
        if (TensorPrimitives.IsFiniteAll<double>(X) && TensorPrimitives.IsFiniteAll<double>(Y) && TensorPrimitives.IsFiniteAll<double>(Z)) return None;
        for (int v = 0; v < vertexCount; v++) {
            if (!double.IsFinite(x[v]) || !double.IsFinite(y[v]) || !double.IsFinite(z[v])) return Some(v);
        }
        return None;
    }

    static void Grow<T>(ref T[] column, int needed) {
        if (needed > column.Length) ArrayPool<T>.Shared.Resize(ref column, int.Max(needed, column.Length << 1));
    }

    static void GrowBits(ref ulong[] words, int slots) {
        int needed = (slots >> 6) + 1;
        if (needed > words.Length) {
            int prior = words.Length;
            ArrayPool<ulong>.Shared.Resize(ref words, int.Max(needed, words.Length << 1));
            words.AsSpan(prior).Clear();
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Kernels {
    // --- [WELD]
    public static MeshEdit WeldDuplicates(MeshEdit edit) {
        while (Merged(edit) > 0) { }
        return edit;

        static int Merged(MeshEdit edit) {
            double band = edit.Tolerance.For(edit.Policy.Weld).Value;
            int n = edit.VertexCount;
            using SpanOwner<int> parentOwner = SpanOwner<int>.Allocate(n);
            Span<int> parent = parentOwner.Span;
            for (int v = 0; v < n; v++) parent[v] = v;

            Dictionary<(long, long, long), List<int>> grid = new();
            for (int v = 0; v < n; v++) {
                (long cx, long cy, long cz) = Cell(edit, v, band);
                for (long dx = -1; dx <= 1; dx++) for (long dy = -1; dy <= 1; dy++) for (long dz = -1; dz <= 1; dz++) {
                    if (!grid.TryGetValue((cx + dx, cy + dy, cz + dz), out List<int>? bucket)) continue;
                    foreach (int u in bucket) {
                        if (edit.Position(v).DistanceTo(edit.Position(u)) <= band) Union(parent, v, u);
                    }
                }
                (grid.TryGetValue((cx, cy, cz), out List<int>? own) ? own : grid[(cx, cy, cz)] = []).Add(v);
            }

            using SpanOwner<int> remapOwner = SpanOwner<int>.Allocate(n);
            Span<int> remap = remapOwner.Span;
            int classes = 0;
            for (int v = 0; v < n; v++) if (Find(parent, v) == v) remap[v] = classes++;
            for (int v = 0; v < n; v++) remap[v] = remap[Find(parent, v)];
            using SpanOwner<int> sizeOwner = SpanOwner<int>.Allocate(classes, AllocationMode.Clear);
            using SpanOwner<double> sxOwner = SpanOwner<double>.Allocate(classes, AllocationMode.Clear);
            using SpanOwner<double> syOwner = SpanOwner<double>.Allocate(classes, AllocationMode.Clear);
            using SpanOwner<double> szOwner = SpanOwner<double>.Allocate(classes, AllocationMode.Clear);
            (Span<int> classSize, Span<double> sumX, Span<double> sumY, Span<double> sumZ) =
                (sizeOwner.Span, sxOwner.Span, syOwner.Span, szOwner.Span);
            for (int v = 0; v < n; v++) {
                int w = remap[v];
                (sumX[w], sumY[w], sumZ[w], classSize[w]) = (sumX[w] + edit.X[v], sumY[w] + edit.Y[v], sumZ[w] + edit.Z[v], classSize[w] + 1);
            }
            edit.Compact(classes, sumX, sumY, sumZ, classSize, remap);
            return n - classes;

            static (long, long, long) Cell(MeshEdit edit, int v, double band) => (
                (long)Math.Floor(edit.X[v] / band), (long)Math.Floor(edit.Y[v] / band), (long)Math.Floor(edit.Z[v] / band));

            static int Find(Span<int> parent, int i) { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; }

            static void Union(Span<int> parent, int a, int b) {
                int ra = Find(parent, a), rb = Find(parent, b);
                if (ra != rb) parent[int.Max(ra, rb)] = int.Min(ra, rb);
            }
        }
    }

    // --- [TRANSFORM]
    readonly struct TransformAction(MeshEdit edit, Transform xform) : IAction {
        public void Invoke(int v) => edit.SetPosition(v, xform * edit.Position(v));
    }

    public static MeshEdit Apply(MeshEdit edit, Transform xform) {
        edit.Parallel(edit.VertexCount, new TransformAction(edit, xform));
        if (xform.Determinant >= 0.0) return edit;
        for (int f = 0; f < edit.FaceCount; f++) {
            if (!edit.Alive(f)) continue;
            (int a, int b, int c) = edit.Face(f);
            edit.SetFace(f, c, b, a);
            if (edit.CornerUv(f).Case is (Point2d ua, Point2d ub, Point2d uc)) edit.SetCornerUv(f, uc, ub, ua);
        }
        return edit;
    }

    // --- [QUAD_DIAGONAL]
    public static bool QuadDiagonal(Point3d a, Point3d b, Point3d c, Point3d d) =>
        Axis.DominantOf(a, b, c, d).Case is Axis axis
        && Predicate.Orient2D(a, c, b, axis).Times(Predicate.Orient2D(a, c, d, axis)) == Sign.Negative;
}
```

## [03]-[ARENA_LAW]

One contract carries store mutability and arena concurrency; a sibling store composes it by name.

- Single-writer: an arena has exactly one mutating owner for its lifetime — no lock, no CAS, no interior synchronization. Concurrency enters only through the two sanctioned read modes: a frozen post-freeze projection, or partition-disjoint spans each parallel worker owns through `ParallelHelper` struct actions at the policy floor.
- Publish-by-freeze: build state becomes composable truth only through the freeze seam — `ToSpace` → `MeshSpace.Of` here, the analogous emission projection on every sibling arena. Consumers hold the frozen artifact, never a live arena across an ownership boundary.
- Context binding: an arena binds ONE `Context` at admission and every band it reads is a `ToleranceLane` off that context; a store carrying its own tolerance number forks the lane vocabulary and re-admits under a regime its snapshot never proved.
- Hash-eligibility: content addressing — the reconciliation `Encode` chain over `CanonicalWriter` — binds only frozen projections; a mid-build arena is never hashed, cached by content, or interned, and no arena mints a span-combine hash of its own.
- Derived-state caching: derived solver state keys on the frozen snapshot reference and dies with it; `mesh.md`'s `LaplacianCache` owns that pattern, and an arena mints no second derived-state cache.
- Capacity: every arena column grows by amortized doubling — `MeshEdit` through the pooled `ArrayPoolExtensions.Resize` verb, the heap-array sibling stores through `Array.Resize` at the same doubling law; the law is the doubling, the verb follows the store's storage class, and a store sized once from an input-derived `2n` guess is the rejected under-allocation.
- Fault surface: arena mutation is total; failure surfaces at freeze and re-admission through the publishing seam's existing rail, and no arena mints a fault union.

## [04]-[DENSITY_BAR]

One arena, one policy row, one kernel family; capability is a row, case, or fold arm, never a sibling surface. Each `[RAIL]` cell names the owner's one return rail, the per-axis kind on the indexed notes below.

| [INDEX] | [AXIS_CONCERN] | [OWNER]       | [RAIL]                                           | [CASES] |
| :-----: | :------------- | :------------ | :----------------------------------------------- | :-----: |
|  [01]   | Arena policy   | `ArenaPolicy` | value (composed by healing/arrangement policies) |    —    |
|  [02]   | Build arena    | `MeshEdit`    | `ToSpace(Op) → Fin<MeshSpace>` (the ONE rail)    |    2    |
|  [03]   | Arena kernels  | `Kernels`     | total (mutates the arena; no rail)               |    3    |

- [01]-[ARENA_POLICY]: `record` policy row — capacity seed, weld lane, parallel floor.
- [02]-[BUILD_ARENA]: `sealed class` single-writer pooled SoA — one polymorphic `Of` (space | soup), mutation verbs, dirty bitsets, partition-disjoint `Parallel`, freeze.
- [03]-[ARENA_KERNELS]: static primitive family — union-find tolerance-grid weld (in-place, swept to a merge-free pass), determinant-derived affine transform with its orientation repair, exact quad-diagonal gate over the `Axis.DominantOf` plane admission.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
