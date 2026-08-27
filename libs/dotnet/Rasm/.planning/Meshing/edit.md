# [RASM_MESHING_EDIT]

`MeshEdit` owns the mutable-arena tier of the mesh substrate — the single-writer SoA build arena every mesh-rewriting owner constructs into. `MeshSpace` and `MeshEdit` are the kernel's only two mesh carriers: the immutable admission snapshot `mesh.md` owns and this page's predicate-gated build arena. Algorithms admit a `MeshSpace`, mutate one arena in place under the single-writer contract, and publish by freeze — `ToSpace` re-enters admission through `MeshSpace.Of`, the only path from build state to composable truth.

`MeshEdit`'s arena namespace is total — `Of` and every mutation verb mint no fault union; a defective build surfaces once, at the freeze boundary, through `MeshSpace.Of`'s `Fin` result behind one bulk finiteness pre-gate. Arenas bind ONE `Context` at admission and read every band off it through `ToleranceLane`, so none carries a tolerance number of its own. Storage is pooled struct-of-arrays: per-column arrays rented from `ArrayPool<T>.Shared` and grown by amortized doubling, so a million-vertex rewrite leases a handful of pooled columns, not a persistent-collection copy per operation. `CommunityToolkit.HighPerformance` composes the span planes, pooled staging, struct-action folds, and packed bitsets.

## [01]-[INDEX]

- [02]-[ARENA]: `ArenaPolicy` the policy row; `MeshEdit` the single-writer SoA build arena over one polymorphic `Of`, span reads, dirty-bitset mutation verbs, partition-disjoint folds, the in-place `Weld` and `Apply`, and the `ToSpace` freeze.
- [03]-[ARENA_LAW]: store-mutability and arena-concurrency contract sibling stores compose by name.
- [04]-[DENSITY_BAR]: one arena, one policy row — the owner/result/case partition.

## [02]-[ARENA]

- Owner: `ArenaPolicy` the arena policy row — capacity seed, the weld LANE, and the parallel floor every arena fold derives from; `MeshEdit` the `sealed class` single-writer arena over pooled SoA columns rented from `ArrayPool<T>.Shared` and grown by amortized doubling, carrying the union-find tolerance-grid weld and the determinant-derived affine transform — with the orientation repair a reversing map owes — as in-place verbs over its own columns, and composing `mesh.md`'s `MeshKernel.QuadDiagonal` for the exact split the mesh-ingress triangulation rides.
- Cases: `Of` discriminates on argument type — an already-admitted `MeshSpace` or a raw triangle soup with its own `Context` — one owner, not a name pair; the soup modality is the kernel's one triangle-soup adapter, folding per-page `Soup(MeshSpace)` copies into a single `DuplicateNative` with quad faces split through the exact diagonal gate. Mutation verbs dirty-mark their slots — `SetFace` is the corner rewrite the decimate edge-collapse and remesh edge-flip land on, face indices stable under mutation; read projections return frozen span views, never copies.
- Entry: arena admission is total, no gate — a `MeshSpace` is already-admitted truth and a raw soup's validity is decided at freeze. `ToSpace(Op key)` is the one publish boundary: a one-pass `TensorPrimitives.IsFiniteAll<double>` bulk gate per coordinate column, live faces rebuilt into a native `Mesh`, orphaned vertices compacted, then re-admission through `MeshSpace.Of` under the arena's OWN bound context; the finiteness gate routes `GeometryFault.DegenerateInput` with the offending slot, every other failure `MeshSpace.Of`'s own result. `Weld` welds in place at `Context.For(ArenaPolicy.Weld)`, sweeping to a merge-free pass, total.
- Auto: `Of(MeshSpace)` calls `DuplicateNative` once, triangulates quads through the exact diagonal gate, and bulk-fills the columns; capacity grows by doubling every column together, so the offset-page under-allocation class — a store sized `2n` where the algorithm writes past it — is structurally impossible. `KillFace` tombstones a row and `ToSpace` compacts, the sentinel arena-internal and never observable past the freeze; dirty tracking is two packed bitsets replacing persistent `Set<int>` accumulation, enumerated for incremental consumers and cleared once the admission fill completes so a set bit names a kernel edit and never the build; `Parallel` runs a caller struct action over a caller-named index extent via `ParallelHelper`, allocation-free with the floor a policy row; the per-corner `uv` column — one pooled `Point2d` per corner — rents lazily on first `SetCornerUv`, rides faces so a weld never disturbs it, ingests per-vertex native texture coordinates at `Of(MeshSpace)`, and publishes wedge-faithfully at the freeze — a shared vertex whose corners disagree splits once per distinct UV, so no island's UV overwrites another's.
- Law: the arena is build state, never evidence — the `MeshSpace` the freeze publishes is the hash-eligible artifact, and dirty bitsets are working state a consumer projects.
- Packages: CommunityToolkit.HighPerformance (span planes, `ArrayPool` rent-resize, pooled kernel staging, `ParallelHelper` struct-action folds, packed bitsets), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll<double>` the freeze gate), `Rasm.Meshing` (`MeshSpace`/`MeshSpace.Of` freeze re-admission and native `Mesh` rebuild, `MeshKernel.QuadDiagonal` the exact ingress split gate — composed, never re-minted), `Rasm.Numerics` (`Dimension` the count carrier, `GeometryFault` the freeze fault), Rasm.Domain (`Context`, `ToleranceLane`, `Op`, `Kind`), QuikGraph (`ForestDisjointSet<int>` the weld partition — `MakeSet`/`Union`/`FindSet`, no page-local union-find), LanguageExt.Core (`Fin`/`Option` the freeze types), Rhino.Geometry (`Mesh`/`MeshFace`/`Point3d` native types, `Transform`/`Transform.Determinant` the transform pass's map and its orientation discriminant), BCL inbox (`ArrayPool<T>`).
- Growth: a new bulk mutation — an edge-split pass, a tangential-relax sweep, a further per-vertex or per-corner attribute column — is one arena verb or one further SoA column on the same rent/resize/dirty machinery the `uv` column already rides; a new build primitive is one further arena verb over the same columns; a new parallel fold is one struct action; a new band is one `ToleranceLane` row read off the bound context; zero new carriers.
- Boundary: `MeshEdit.Of` owns the kernel's one triangle-soup adapter, every consumer composing it rather than a per-page `Soup(MeshSpace)` copy; the weld kernel lives here and its band is `ToleranceLane.Weld` read off the arena's bound `Context` — dedup-on-arena is an arena op, reached through no healing policy and carrying no tolerance number of its own, its partition the admitted QuikGraph `ForestDisjointSet<int>` and a page-local union-find deleted; the transform pass owns MIRRORED geometry solution-wide, so a consumer needing a reflected part builds it as an admitted mesh through `MeshEdit.Of(space).Apply(Transform.Mirror(plane))` and never places an admitted mesh under a reversing transform, which silently inverts the orientation its admission just proved; the arena binds ONE context for its lifetime, so a freeze under a different tolerance regime is a second `MeshSpace.Of` at the mesh owner, never a second context on this boundary; in-place span kernels inside `MeshEdit` are the arena tier's statement exemption, never leaking past the freeze, so every public egress is a span view, a value, or the `Fin<MeshSpace>` result.

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
using QuikGraph.Collections;
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
    Point2d[]? uv;
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
        dirtyVertex = ArrayPool<ulong>.Shared.Rent(((seed - 1) >> 6) + 1);
        dirtyFace = ArrayPool<ulong>.Shared.Rent(((seed - 1) >> 6) + 1);
        System.Array.Clear(dirtyVertex);
        System.Array.Clear(dirtyFace);
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
            if (MeshKernel.QuadDiagonal(edit.Position(face.A), edit.Position(face.B), edit.Position(face.C), edit.Position(face.D))) {
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
                edit.SetCornerUv(f, new(native.TextureCoordinates[a].X, native.TextureCoordinates[a].Y),
                    new(native.TextureCoordinates[b].X, native.TextureCoordinates[b].Y),
                    new(native.TextureCoordinates[c].X, native.TextureCoordinates[c].Y));
            }
        }
        edit.Baseline();
        return edit;
    }

    public static MeshEdit Of(ReadOnlySpan<Point3d> vertices, ReadOnlySpan<(int A, int B, int C)> faces,
        Context context, Option<ArenaPolicy> policy = default) {
        MeshEdit edit = new(context: context, policy: policy.IfNone(noneValue: ArenaPolicy.Canonical));
        foreach (Point3d p in vertices) edit.AddVertex(p);
        foreach ((int a, int b, int c) in faces) edit.AddFace(a, b, c);
        edit.Baseline();
        return edit;
    }

    void Baseline() { System.Array.Clear(dirtyVertex); System.Array.Clear(dirtyFace); }

    // --- [READ_SURFACE]
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
        uv is null ? None : Some((uv[3 * f], uv[3 * f + 1], uv[3 * f + 2]));
    public BoundingBox Bounds(int f) => BoundingBox.Union(BoundingBox.Union(
        new(Position(tri[3 * f]), Position(tri[3 * f])), new(Position(tri[3 * f + 1]), Position(tri[3 * f + 1]))),
        new(Position(tri[3 * f + 2]), Position(tri[3 * f + 2])));

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
        uv ??= ArrayPool<Point2d>.Shared.Rent(tri.Length);
        Grow(ref uv, 3 * (f + 1));
        (uv[3 * f], uv[3 * f + 1], uv[3 * f + 2]) = (a, b, c);
        BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    }

    public void KillFace(int f) {
        (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]) = (-1, -1, -1);
        BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    }

    public IEnumerable<int> DirtyVertices() { for (int v = 0; v < vertexCount; v++) if (BitHelper.HasFlag(dirtyVertex[v >> 6], v & 63)) yield return v; }
    public IEnumerable<int> DirtyFaces() { for (int f = 0; f < faceCount; f++) if (BitHelper.HasFlag(dirtyFace[f >> 6], f & 63)) yield return f; }

    // --- [PARALLEL]
    public void Parallel<TAction>(int extent, in TAction action) where TAction : struct, IAction =>
        ParallelHelper.For(0, extent, in action, policy.ParallelFloor.Value);

    // --- [TRANSFORM]
    readonly struct TransformAction(MeshEdit edit, Transform xform) : IAction {
        public void Invoke(int v) {
            Point3d p = xform * edit.Position(v);
            (edit.x[v], edit.y[v], edit.z[v]) = (p.X, p.Y, p.Z);
        }
    }

    public MeshEdit Apply(Transform xform) {
        Parallel(vertexCount, new TransformAction(this, xform));
        dirtyVertex.AsSpan(0, ((vertexCount - 1) >> 6) + 1).Fill(ulong.MaxValue);
        if (xform.Determinant >= 0.0) return this;
        for (int f = 0; f < faceCount; f++) {
            if (!Alive(f)) continue;
            (int a, int b, int c) = Face(f);
            SetFace(f, c, b, a);
            if (uv is not null) (uv[3 * f], uv[3 * f + 2]) = (uv[3 * f + 2], uv[3 * f]);
        }
        return this;
    }

    // --- [WELD]
    public MeshEdit Weld() {
        while (Merged() > 0) { }
        return this;

        int Merged() {
            double band = context.For(policy.Weld).Value;
            int n = vertexCount;
            ForestDisjointSet<int> partition = new(capacity: n);
            for (int v = 0; v < n; v++) partition.MakeSet(v);

            Dictionary<(long, long, long), List<int>> grid = new();
            for (int v = 0; v < n; v++) {
                (long cx, long cy, long cz) = Cell(v);
                for (long dx = -1; dx <= 1; dx++) for (long dy = -1; dy <= 1; dy++) for (long dz = -1; dz <= 1; dz++) {
                    if (!grid.TryGetValue((cx + dx, cy + dy, cz + dz), out List<int>? bucket)) continue;
                    foreach (int u in bucket) {
                        if (Position(v).DistanceTo(Position(u)) <= band) partition.Union(v, u);
                    }
                }
                (grid.TryGetValue((cx, cy, cz), out List<int>? own) ? own : grid[(cx, cy, cz)] = []).Add(v);
            }

            using SpanOwner<int> remapOwner = SpanOwner<int>.Allocate(n);
            Span<int> remap = remapOwner.Span;
            int classes = 0;
            for (int v = 0; v < n; v++) if (partition.FindSet(v) == v) remap[v] = classes++;
            for (int v = 0; v < n; v++) remap[v] = remap[partition.FindSet(v)];
            using SpanOwner<(double X, double Y, double Z, int Count)> sumsOwner =
                SpanOwner<(double X, double Y, double Z, int Count)>.Allocate(classes, AllocationMode.Clear);
            Span<(double X, double Y, double Z, int Count)> sums = sumsOwner.Span;
            for (int v = 0; v < n; v++) {
                ref (double X, double Y, double Z, int Count) sum = ref sums[remap[v]];
                sum = (sum.X + x[v], sum.Y + y[v], sum.Z + z[v], sum.Count + 1);
            }
            Compact(classes, sums, remap);
            return n - classes;

            (long, long, long) Cell(int v) => (
                (long)Math.Floor(x[v] / band), (long)Math.Floor(y[v] / band), (long)Math.Floor(z[v] / band));
        }
    }

    // --- [FREEZE]
    public Fin<MeshSpace> ToSpace(Op key) {
        if (vertexCount > 0 && !(TensorPrimitives.IsFiniteAll<double>(X)
            && TensorPrimitives.IsFiniteAll<double>(Y) && TensorPrimitives.IsFiniteAll<double>(Z))) {
            int slot = 0;
            while (double.IsFinite(x[slot]) && double.IsFinite(y[slot]) && double.IsFinite(z[slot])) slot++;
            return new GeometryFault.DegenerateInput(Kind.Mesh, slot, "non-finite arena coordinate");
        }
        Mesh mesh = new();
        if (uv is null) {
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
        if (uv is not null) ArrayPool<Point2d>.Shared.Return(uv);
        ArrayPool<ulong>.Shared.Return(dirtyVertex); ArrayPool<ulong>.Shared.Return(dirtyFace);
    }

    void Compact(int classes, ReadOnlySpan<(double X, double Y, double Z, int Count)> sums, ReadOnlySpan<int> remap) {
        for (int w = 0; w < classes; w++) {
            (double sx, double sy, double sz, int count) = sums[w];
            (x[w], y[w], z[w]) = (sx / count, sy / count, sz / count);
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

    static void Grow<T>(ref T[] column, int needed) {
        if (needed > column.Length) ArrayPool<T>.Shared.Resize(ref column, int.Max(needed, column.Length << 1));
    }

    static void GrowBits(ref ulong[] words, int slots) {
        int needed = ((slots - 1) >> 6) + 1;
        if (needed > words.Length) {
            int prior = words.Length;
            ArrayPool<ulong>.Shared.Resize(ref words, int.Max(needed, words.Length << 1));
            words.AsSpan(prior).Clear();
        }
    }
}
```

## [03]-[ARENA_LAW]

One contract carries store mutability and arena concurrency; a sibling store composes it by name.

- Single-writer: an arena has exactly one mutating owner for its lifetime — no lock, no CAS, no interior synchronization. Concurrency enters only through the two sanctioned read modes: a frozen post-freeze projection, or partition-disjoint spans each parallel worker owns through `ParallelHelper` struct actions at the policy floor.
- Publish-by-freeze: build state becomes composable truth only through the freeze boundary — `ToSpace` → `MeshSpace.Of` here, the analogous emission projection on every sibling arena. Consumers hold the frozen artifact, never a live arena across an ownership boundary.
- Context binding: an arena binds ONE `Context` at admission and every band it reads is a `ToleranceLane` off that context; a store carrying its own tolerance number forks the lane vocabulary and re-admits under a regime its snapshot never proved.
- Hash-eligibility: content addressing — the reconciliation `Encode` chain over `CanonicalWriter` — binds only frozen projections; a mid-build arena is never hashed, cached by content, or interned, and no arena mints a span-combine hash of its own.
- Derived-state caching: derived solver state keys on the frozen snapshot reference and dies with it; `mesh.md`'s `LaplacianCache` owns that pattern, and an arena mints no second derived-state cache.
- Capacity: every arena column grows by amortized doubling — `MeshEdit` through the pooled `ArrayPoolExtensions.Resize` verb, the heap-array sibling stores through `Array.Resize` at the same doubling law; the law is the doubling, the verb follows the store's storage class, and a store sized once from an input-derived `2n` guess is the rejected under-allocation.
- Fault surface: arena mutation is total; failure surfaces at freeze and re-admission through the publishing boundary's existing result, and no arena mints a fault union.

## [04]-[DENSITY_BAR]

One arena, one policy row; capability is a row, case, or fold arm, never a sibling surface. Each `[RESULT]` cell names the owner's one return type, the per-axis kind on the indexed notes below.

| [INDEX] | [AXIS_CONCERN] | [OWNER]       | [RESULT]                                         | [CASES] |
| :-----: | :------------- | :------------ | :----------------------------------------------- | :-----: |
|  [01]   | Arena policy   | `ArenaPolicy` | value (composed by healing/arrangement policies) |    —    |
|  [02]   | Build arena    | `MeshEdit`    | `ToSpace(Op) → Fin<MeshSpace>` (the ONE exit)    |    2    |

- [01]-[ARENA_POLICY]: `record` policy row — capacity seed, weld lane, parallel floor.
- [02]-[BUILD_ARENA]: `sealed class` single-writer pooled SoA — one polymorphic `Of` (space | soup), mutation verbs, dirty bitsets, partition-disjoint `Parallel`, in-place union-find tolerance-grid `Weld` swept to a merge-free pass, in-place determinant-derived `Apply` with its orientation repair, freeze.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
