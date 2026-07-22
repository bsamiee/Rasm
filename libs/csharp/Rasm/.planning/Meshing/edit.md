# [RASM_MESHING_EDIT]

The mutable-arena tier of the mesh substrate — `MeshEdit`, the single-writer struct-of-arrays build arena every rewriting owner (intersect, arrangement, view, decimate, receipts, repair, remesh, slice, skeleton, subdivide) constructs into, plus `Kernels`, the weld/diagonal primitive family that operates ON the arena. The two-carrier seam is the page's first law: `MeshSpace` (the immutable admission snapshot `mesh.md` owns) and `MeshEdit` (this page's predicate-gated build arena) are the ONLY two mesh carriers in the kernel — a third carrier, an immutable copy-per-op edit record, or a page-local vertex/face buffer beside these two is the deleted form. An algorithm admits a `MeshSpace`, opens ONE arena, mutates in place under the single-writer contract, and publishes by freeze: `ToSpace` re-enters admission through the landed `MeshSpace.Of`, the only path from build state back to hashable, composable truth.

The arena namespace is TOTAL — `MeshEdit.Of`, every mutation verb, and every `Kernels` member are total operations minting no fault union of their own: a defective build surfaces exactly once, at the freeze or re-admission seam, through `MeshSpace.Of`'s existing `Fin` rail plus this page's one bulk finiteness pre-gate. Storage is pooled SoA — per-column rented arrays (`X`/`Y`/`Z` coordinate columns, a row-major face-index column, packed dirty bitsets) grown by amortized doubling through `ArrayPoolExtensions.Resize` — so a million-vertex rewrite allocates a handful of pool leases instead of a persistent-collection copy per operation. `CommunityToolkit.HighPerformance` is ADMITTED on exactly these arena shapes (`Span2D`/`Memory2D` planes, the `ArrayPoolExtensions` rent-resize verbs on raw pool leases with `MemoryOwner`/`SpanOwner` for fixed-extent staging, `ParallelHelper` struct-action folds, `BitHelper` packed flags); the interior composes its span planes and never re-derives a plane abstraction.

## [01]-[INDEX]

- [01]-[ARENA]: `ArenaPolicy` policy row (capacity seed · the weld-tolerance knob · the parallel floor); `MeshEdit` the single-writer SoA arena — ONE polymorphic `Of` over `MeshSpace` | raw soup, span read surface, dirty-bitset mutation verbs, `ParallelHelper` partition-disjoint folds, and the `ToSpace(Context, Op?) → Fin<MeshSpace>` freeze; `Kernels` the weld/diagonal primitive family (`WeldDuplicates` union-find over a tolerance grid, the exact quad-diagonal gate composing the Numerics `Axis.DominantOf` plane admission).
- [02]-[ARENA_LAW]: the corpus-wide store-mutability + arena-concurrency contract every mutable geometry store obeys.

## [02]-[ARENA]

- Owner: `ArenaPolicy` the arena policy record — `Capacity` (the column seed extent doubling absorbs), `WeldTolerance` (THE weld knob, sited here so dedup-on-arena is an arena op and no consumer reaches into a healing policy for it), `ParallelFloor` (the `minimumActionsPerThread` floor every arena parallel fold derives from); `MeshEdit` the `sealed class` single-writer arena over pooled SoA columns — `double[]` `X`/`Y`/`Z` coordinate columns, one row-major `int[]` face-triple column viewed as a `ReadOnlyMemory2D<int>` F×3 plane (`Span2D` at the read site), and two `ulong[]` packed dirty bitsets (vertex/face) driven by `BitHelper` — all rented from `ArrayPool<T>.Shared`, grown by amortized doubling through `ArrayPoolExtensions.Resize`, returned on `Dispose`; `Kernels` the static primitive family operating on the arena: `WeldDuplicates` (union-find duplicate weld over a tolerance grid, in-place SoA compaction, idempotent) and the exact quad-diagonal split gate the mesh-ingress triangulation rides — its projection plane read off the Numerics `Axis.DominantOf` admission, never a page-local dominant-axis copy.
- Cases: `MeshEdit.Of` modalities `MeshSpace` · raw soup (`ReadOnlySpan<Point3d>` + `ReadOnlySpan<(int,int,int)>`) — the argument TYPE is the discriminant, one owner, never an `OfMesh`/`OfSoup` name pair; the soup modality is the ONE triangle-soup adapter for the whole kernel (the per-page `Soup(MeshSpace)` copies in intersect/view/arrangement die into this owner — one `DuplicateNative` per admission, quad faces split through the exact diagonal gate, never three private re-derivations); mutation verbs `AddVertex` · `AddFace` · `SetPosition` · `SetFace` · `KillFace` · `Touch` (6, each dirty-marking its slots — `SetFace` is the corner rewrite the decimate edge-collapse re-point and the remesh edge-flip land on, face indices stable under mutation); read projections `Policy` · `VertexCount`/`FaceCount` · `X`/`Y`/`Z` · `Faces` · `Position` · `Face` · `Alive` · `Bounds` · `DirtyVertices`/`DirtyFaces` — frozen span views, never copies.
- Entry: `public static MeshEdit Of(MeshSpace space, ArenaPolicy? policy = null)` and `public static MeshEdit Of(ReadOnlySpan<Point3d> vertices, ReadOnlySpan<(int A, int B, int C)> faces, ArenaPolicy? policy = null)` — total, no rail: a `MeshSpace` is already-admitted truth and a raw soup is build material whose validity is decided at freeze, so arena admission cannot fail; `public Fin<MeshSpace> ToSpace(Context context, Op? key = null)` — the ONE publish seam: a one-pass `TensorPrimitives.IsFiniteAll<double>` bulk gate per coordinate column (the whole-arena validity check, never a per-vertex scan), live faces rebuilt into a native `Mesh` (`Vertices.Add`/`Faces.AddFace`, `Compact` culling orphaned vertices, `RebuildNormals`), then re-admission through the landed `MeshSpace.Of(mesh, context, key)` — the arena mints no `ArenaFault`; the finiteness pre-gate routes `GeometryFault.DegenerateInput` with the offending column index, everything else is `MeshSpace.Of`'s own rail; `public static MeshEdit Kernels.WeldDuplicates(MeshEdit edit)` — in-place weld at the arena's `WeldTolerance` band, total (the knob lives on `ArenaPolicy`, never a parameter a caller re-supplies).
- Auto: `Of(MeshSpace)` calls `DuplicateNative()` exactly once, triangulates quads through the exact diagonal gate (the reflex-separation test on the quad's dominant-axis projection — exact `Predicate.Orient2D` signs decide which diagonal is interior; a shortest-diagonal float heuristic is the rejected non-robust form), and bulk-fills the columns; capacity growth is `pool.Resize(ref column, extent << 1)` on every column together, so the offset-page under-allocation class (a store sized `2n` where the algorithm writes past it) is structurally impossible — capacity is one doubling law, never a per-site size guess; `KillFace` tombstones the row (`-1` triple) and `ToSpace` compacts — the sentinel is arena-INTERNAL and never observable past the freeze; dirty tracking is two packed bitsets (`BitHelper.SetFlag`/`HasFlag` on `ulong` words) replacing persistent `Set<int>` accumulation — `DirtyVertices()`/`DirtyFaces()` enumerate set bits for incremental consumers (the receipts `Affected` seed, decimate's re-queue set); `Parallel<TAction>` runs a caller struct action over a caller-named index extent (vertex count, face count, or a sub-range bound) via `ParallelHelper.For(0, extent, in action, policy.ParallelFloor)` — allocation-free struct invocation, degree clamped by the library, the floor a policy row.
- Receipt: none — the arena is build state, not evidence; the receipt-bearing artifact is the `MeshSpace` the freeze publishes (and downstream, the `TopologyReceipt` family `mesh.md` owns). Dirty bitsets are working state a consumer projects, never a receipt.
- Packages: CommunityToolkit.HighPerformance (`Span2D<T>`/`Memory2D<T>` face plane, `ArrayPoolExtensions.Resize` amortized doubling, `SpanOwner<T>` pooled kernel staging, `ParallelHelper.For` + `IAction` struct actions, `BitHelper.SetFlag`/`HasFlag` packed bitsets — ADMITTED on these arena shapes), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll<double>` the one-pass freeze gate, span-fed from the SoA columns directly), `Rasm.Meshing` (`MeshSpace`/`MeshSpace.Of` the freeze re-admission + `Mesh` native rebuild surface), `Rasm.Numerics` (`Predicate.Orient2D` + `Axis` — the exact quad-diagonal gate), Rasm.Domain (`Context` tolerance model, `Op` key threading, `Kind`), LanguageExt.Core (`Fin<MeshSpace>` the freeze rail), Rhino.Geometry (`Mesh`/`MeshFace`/`Point3d` at the native seam), BCL inbox (`ArrayPool<T>`).
- Growth: a new bulk mutation (an edge-split pass, a tangential-relax sweep, a vertex-attribute column) is one arena verb or one further SoA column riding the same rent/resize/dirty machinery — never a sibling store; a new build-time primitive (a collapse kernel, a flip kernel) is one `Kernels` member over the same columns; a new parallel fold is one struct action, never a raw `Parallel.For` beside the budgeted surface; zero new carriers.
- Boundary: `MeshSpace` ⟂ `MeshEdit` is the closed two-carrier seam and a third mesh carrier anywhere in the kernel is the named defect — an immutable `record` edit state with copy-per-op `with` mutation (the shape this page's arena replaced) re-pays a full-store copy per operation and is the rejected form; the arena is SINGLE-WRITER — no lock, no CAS, no concurrent mutation: one owner thread mutates, parallel reads run only over frozen or partition-disjoint spans, and a second writer is a race by construction, not a supported mode; content addressing binds ONLY frozen projections — the reconciliation `Encode` hashes the `MeshSpace` the freeze publishes, a mid-build arena is NEVER hashed, and a span-combine helper hash (`HashCode<T>`) stays rejected under the `XxHash128` identity monopoly; the soup adapter lives HERE once and a per-consumer `Soup(MeshSpace)` re-derivation is the deleted triple; the weld kernel and its `WeldTolerance` knob live HERE (dedup-on-arena is an arena op) and a healing-policy reach-through for the weld band is the dead edge this siting removed; in-place span kernels inside `MeshEdit`/`Kernels` are the named statement exemption of the arena tier — they never leak past the freeze, and every public egress is a span view, a value, or the `Fin<MeshSpace>` rail.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
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

namespace Rasm.Meshing;

// --- [CONSTANTS] ------------------------------------------------------------------------------
// The arena policy row: Capacity seeds every column (doubling absorbs growth), WeldTolerance is THE
// weld knob (sited on the arena, composed by healing/arrangement policies), ParallelFloor feeds
// every ParallelHelper fold as minimumActionsPerThread.
public sealed record ArenaPolicy(int Capacity, double WeldTolerance, int ParallelFloor) {
    public static readonly ArenaPolicy Canonical = new(Capacity: 1_024, WeldTolerance: 1e-6, ParallelFloor: 4_096);
}

// --- [MODELS] ---------------------------------------------------------------------------------
public sealed class MeshEdit : IDisposable {
    // Single-writer pooled SoA. Columns rent from ArrayPool<T>.Shared and grow together by doubling;
    // dirty state is one bit per slot on ulong words. Statement bodies are the arena-tier exemption.
    double[] x, y, z;
    int[] tri;
    ulong[] dirtyVertex, dirtyFace;
    int vertexCount, faceCount;
    readonly ArenaPolicy policy;

    MeshEdit(ArenaPolicy policy) {
        this.policy = policy;
        x = ArrayPool<double>.Shared.Rent(policy.Capacity);
        y = ArrayPool<double>.Shared.Rent(policy.Capacity);
        z = ArrayPool<double>.Shared.Rent(policy.Capacity);
        tri = ArrayPool<int>.Shared.Rent(3 * policy.Capacity);
        dirtyVertex = ArrayPool<ulong>.Shared.Rent((policy.Capacity >> 6) + 1);
        dirtyFace = ArrayPool<ulong>.Shared.Rent((policy.Capacity >> 6) + 1);
        Array.Clear(dirtyVertex);
        Array.Clear(dirtyFace);
    }

    // --- [ADMISSION] — one polymorphic Of; the argument type is the modality. Total: no rail.
    public static MeshEdit Of(MeshSpace space, ArenaPolicy? policy = null) {
        Mesh native = space.DuplicateNative();
        MeshEdit edit = new(policy ?? ArenaPolicy.Canonical);
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
        return edit;
    }

    public static MeshEdit Of(ReadOnlySpan<Point3d> vertices, ReadOnlySpan<(int A, int B, int C)> faces, ArenaPolicy? policy = null) {
        MeshEdit edit = new(policy ?? ArenaPolicy.Canonical);
        foreach (Point3d p in vertices) edit.AddVertex(p);
        foreach ((int a, int b, int c) in faces) edit.AddFace(a, b, c);
        return edit;
    }

    // --- [READ_SURFACE] — frozen/partition-disjoint span views; never a copy
    public ArenaPolicy Policy => policy;
    public int VertexCount => vertexCount;
    public int FaceCount => faceCount;
    public ReadOnlySpan<double> X => x.AsSpan(0, vertexCount);
    public ReadOnlySpan<double> Y => y.AsSpan(0, vertexCount);
    public ReadOnlySpan<double> Z => z.AsSpan(0, vertexCount);
    public ReadOnlyMemory2D<int> Faces => tri.AsMemory(0, 3 * faceCount).AsMemory2D(faceCount, 3);
    public Point3d Position(int v) => new(x[v], y[v], z[v]);
    public (int A, int B, int C) Face(int f) => (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]);
    public bool Alive(int f) => tri[3 * f] >= 0;
    public BoundingBox Bounds(int f) =>
        new([Position(tri[3 * f]), Position(tri[3 * f + 1]), Position(tri[3 * f + 2])]);

    // --- [MUTATION] — single-writer; every verb marks its dirty slots
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

    // Corner rewrite: the edge-collapse re-point and the edge-flip both land here — face indices stay
    // stable, so a consumer's face-keyed queue survives the mutation.
    public void SetFace(int f, int a, int b, int c) {
        (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]) = (a, b, c);
        BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    }

    public void KillFace(int f) {
        (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]) = (-1, -1, -1);  // arena-internal tombstone; compacted at freeze
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

    // --- [PARALLEL] — struct actions over partition-disjoint index slots; the floor is policy.
    // The extent is caller data: vertex count, face count, or a sub-range bound.
    public void Parallel<TAction>(int extent, in TAction action) where TAction : struct, IAction =>
        ParallelHelper.For(0, extent, in action, policy.ParallelFloor);

    // --- [FREEZE] — publish-by-freeze: the ONE admission re-entry
    public Fin<MeshSpace> ToSpace(Context context, Op? key = null) {
        if (!TensorPrimitives.IsFiniteAll<double>(X) || !TensorPrimitives.IsFiniteAll<double>(Y) || !TensorPrimitives.IsFiniteAll<double>(Z)) {
            return Fin.Fail<MeshSpace>(new GeometryFault.DegenerateInput(
                Kind.Mesh, FirstNonFinite(), "non-finite arena coordinate").ToError());
        }
        Mesh mesh = new();
        for (int v = 0; v < vertexCount; v++) mesh.Vertices.Add(x[v], y[v], z[v]);
        for (int f = 0; f < faceCount; f++) {
            if (Alive(f)) mesh.Faces.AddFace(tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]);
        }
        mesh.Compact();  // culls orphaned vertices (tombstoned-face residue) before re-admission
        mesh.RebuildNormals();
        return MeshSpace.Of(mesh, context, key: key);
    }

    public void Dispose() {
        ArrayPool<double>.Shared.Return(x); ArrayPool<double>.Shared.Return(y); ArrayPool<double>.Shared.Return(z);
        ArrayPool<int>.Shared.Return(tri);
        ArrayPool<ulong>.Shared.Return(dirtyVertex); ArrayPool<ulong>.Shared.Return(dirtyFace);
    }

    // In-place weld compaction: class centroids overwrite the live columns (write head trails read
    // head — the centroid staging is read, never the columns), every face triple remaps, a triple
    // whose remapped corners collide tombstones, and the vertex extent shrinks to the class count.
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

    int FirstNonFinite() {
        for (int v = 0; v < vertexCount; v++) {
            if (!double.IsFinite(x[v]) || !double.IsFinite(y[v]) || !double.IsFinite(z[v])) return v;
        }
        return -1;
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

// --- [OPERATIONS] -------------------------------------------------------------------------------
public static class Kernels {
    // --- [WELD]
    // Union-find over a tolerance grid, compacted IN PLACE on the SoA columns (write head trails the
    // read head, so no scratch copy of the store). Idempotent: a welded arena re-welds to itself.
    // The weld band is THE ArenaPolicy knob, read off the arena — never a parameter a caller re-supplies.
    public static MeshEdit WeldDuplicates(MeshEdit edit) {
        double tolerance = double.Max(edit.Policy.WeldTolerance, double.Epsilon);
        int n = edit.VertexCount;
        using SpanOwner<int> parentOwner = SpanOwner<int>.Allocate(n);
        Span<int> parent = parentOwner.Span;
        for (int v = 0; v < n; v++) parent[v] = v;

        Dictionary<(long, long, long), List<int>> grid = new();
        for (int v = 0; v < n; v++) {
            (long cx, long cy, long cz) = Cell(edit, v, tolerance);
            for (long dx = -1; dx <= 1; dx++) for (long dy = -1; dy <= 1; dy++) for (long dz = -1; dz <= 1; dz++) {
                if (!grid.TryGetValue((cx + dx, cy + dy, cz + dz), out List<int>? bucket)) continue;
                foreach (int u in bucket) {
                    if (edit.Position(v).DistanceTo(edit.Position(u)) <= tolerance) Union(parent, v, u);
                }
            }
            (grid.TryGetValue((cx, cy, cz), out List<int>? own) ? own : grid[(cx, cy, cz)] = []).Add(v);
        }

        // class centroids accumulate per root over pooled staging; Compact writes them over the live columns
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
        return edit;

        static (long, long, long) Cell(MeshEdit edit, int v, double tolerance) => (
            (long)Math.Floor(edit.X[v] / tolerance), (long)Math.Floor(edit.Y[v] / tolerance), (long)Math.Floor(edit.Z[v] / tolerance));

        static int Find(Span<int> parent, int i) { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; }

        static void Union(Span<int> parent, int a, int b) {
            int ra = Find(parent, a), rb = Find(parent, b);
            if (ra != rb) parent[int.Max(ra, rb)] = int.Min(ra, rb);
        }
    }

    // --- [QUAD_DIAGONAL]
    // The exact quad-split gate the mesh-ingress triangulation rides: true selects the A-C diagonal,
    // false the B-D. A diagonal is interior exactly when it separates the other two corners on the
    // quad's dominant-axis projection (Axis.DominantOf — the Numerics vocabulary's own `Fin<Axis>`
    // admission); the axis choice is float, the separation signs are exact. A degenerate quad has
    // no dominant axis and no interior diagonal, so the gate refusal binds to the canonical B-D
    // `false` — the exact `Orient2D` collapses that quad to `false` regardless — never a fabricated
    // projection plane; the `bool` verdict is frozen (cross-tier triangulation consumers bind it),
    // so the refusal resolves here rather than widening the whole ingress rail to `Fin<bool>`.
    public static bool QuadDiagonal(Point3d a, Point3d b, Point3d c, Point3d d) =>
        Axis.DominantOf(a, b, c, d).Case is Axis axis
        && Predicate.Orient2D(a, c, b, axis).Times(Predicate.Orient2D(a, c, d, axis)) == Sign.Negative;
}
```

The dictionary grid and the pooled `SpanOwner` union-find/centroid staging inside `WeldDuplicates` are kernel-local scratch under the arena statement exemption; they never escape the call.

## [03]-[ARENA_LAW]

The corpus-wide store-mutability and arena-concurrency contract. Every mutable geometry store in the kernel — this page's `MeshEdit`, the tessellation `SimplexStore`, the offsetting `WavefrontStore`, the intersection chain store, the spatial `NodeStore` — is an ARENA under one law; a store page re-labeling itself immutable while mutating in place is the named prose defect this contract closes.

- Single-writer: an arena has exactly one mutating owner for its lifetime; no lock, no CAS, no interior synchronization. Concurrency enters only through the two sanctioned read modes — a FROZEN projection (post-freeze, immutable) or PARTITION-DISJOINT spans (each parallel worker owns a disjoint index window of a live column, composed through `ParallelHelper` struct actions with the policy floor).
- Publish-by-freeze: build state becomes composable truth only through the freeze seam (`ToSpace` → `MeshSpace.Of` here; the analogous emission projection on every sibling arena). A consumer never holds a live arena across an ownership boundary; it holds the frozen artifact.
- Hash-eligibility: content addressing (the reconciliation `Encode` chain, `XxHash128`) binds ONLY frozen projections. A mid-build arena is never hashed, cached by content, or interned — a content key over mutable state is a stale key by construction.
- Derived-state caching: the exemplar is `mesh.md`'s `LaplacianCache` — derived solver state keys on the FROZEN snapshot reference (`ConditionalWeakTable` dying with its snapshot) and memoizes through `Atom<HashMap>` success-only swaps. An arena page never mints a second derived-state cache pattern.
- Capacity: every arena column grows by amortized doubling — `MeshEdit` through the pooled `ArrayPoolExtensions.Resize` verb, the sibling heap-array stores (`SimplexStore`/`WavefrontStore`/crossing/node columns) through `Array.Resize` at the same doubling law; the law is the doubling, the verb follows the store's storage class, and a store sized once from an input-derived guess (the `2n` class) is the rejected under-allocation.
- Fault surface: arena mutation is total; failure surfaces at freeze/re-admission through the publishing seam's existing rail. No arena mints a fault union.

## [04]-[DENSITY_BAR]

One arena, one policy row, one kernel family. The `[RAIL]` cell names the one return rail each owner exposes; the per-axis kind rides the indexed notes below.

| [INDEX] | [AXIS_CONCERN] | [OWNER]       | [RAIL]                                                  | [CASES] |
| :-----: | :------------- | :------------ | :------------------------------------------------------ | :-----: |
|  [01]   | Arena policy   | `ArenaPolicy` | value (composed by healing/arrangement policies)        |    —    |
|  [02]   | Build arena    | `MeshEdit`    | `ToSpace(Context, Op?) → Fin<MeshSpace>` (the ONE rail) |    2    |
|  [03]   | Arena kernels  | `Kernels`     | total (mutates the arena; no rail)                      |    2    |

- [01]-[ARENA_POLICY]: `record` policy row — capacity seed, weld-tolerance knob, parallel floor.
- [02]-[BUILD_ARENA]: `sealed class` single-writer pooled SoA — one polymorphic `Of` (space | soup), mutation verbs, dirty bitsets, partition-disjoint `Parallel`, freeze.
- [03]-[ARENA_KERNELS]: static primitive family — union-find tolerance-grid weld (idempotent, in-place), exact quad-diagonal gate over the `Axis.DominantOf` plane admission.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
