# [RASM_MESHING_EDIT]

`MeshEdit` owns the mutable-arena tier of the mesh substrate — the single-writer SoA build arena every mesh-rewriting owner constructs into, and `Kernels`, the weld/diagonal primitive family over it. `MeshSpace` and `MeshEdit` are the kernel's only two mesh carriers: the immutable admission snapshot `mesh.md` owns and this page's predicate-gated build arena. Algorithms admit a `MeshSpace`, mutate one arena in place under the single-writer contract, and publish by freeze — `ToSpace` re-enters admission through `MeshSpace.Of`, the only path from build state to composable truth.

`MeshEdit`'s arena namespace is total — `Of`, every mutation verb, and every `Kernels` member mint no fault union; a defective build surfaces once, at the freeze seam, through `MeshSpace.Of`'s `Fin` rail behind one bulk finiteness pre-gate. Storage is pooled struct-of-arrays: per-column arrays rented from `ArrayPool<T>.Shared` and grown by amortized doubling, so a million-vertex rewrite leases a handful of pooled columns, not a persistent-collection copy per operation. `CommunityToolkit.HighPerformance` composes the span planes, pooled staging, struct-action folds, and packed bitsets.

## [01]-[INDEX]

- [02]-[ARENA]: `ArenaPolicy` the policy row; `MeshEdit` the single-writer SoA build arena over one polymorphic `Of`, span reads, dirty-bitset mutation verbs, partition-disjoint folds, and the `ToSpace` freeze; `Kernels` the weld/transform/diagonal primitive family over the arena columns.
- [03]-[ARENA_LAW]: store-mutability and arena-concurrency contract sibling stores compose by name.

## [02]-[ARENA]

- Owner: `ArenaPolicy` the arena policy row — capacity seed, the weld-tolerance knob, and the parallel floor every arena fold derives from; `MeshEdit` the `sealed class` single-writer arena over pooled SoA columns rented from `ArrayPool<T>.Shared` and grown by amortized doubling; `Kernels` the static primitive family operating on the arena — union-find tolerance-grid weld, the determinant-derived affine transform pass carrying the orientation repair a reversing map owes, and the exact quad-diagonal split gate the mesh-ingress triangulation rides, its projection plane read off the Numerics `Axis.DominantOf` admission.
- Cases: `Of` discriminates on argument type — a `MeshSpace` or a raw triangle soup — one owner, not a name pair; the soup modality is the kernel's one triangle-soup adapter, folding per-page `Soup(MeshSpace)` copies into a single `DuplicateNative` with quad faces split through the exact diagonal gate. Mutation verbs dirty-mark their slots — `SetFace` is the corner rewrite the decimate edge-collapse and remesh edge-flip land on, face indices stable under mutation; read projections return frozen span views, never copies.
- Entry: arena admission is total, no rail — a `MeshSpace` is already-admitted truth and a raw soup's validity is decided at freeze. `ToSpace(Context, Op?)` is the one publish seam: a one-pass `TensorPrimitives.IsFiniteAll<double>` bulk gate per coordinate column, live faces rebuilt into a native `Mesh`, orphaned vertices compacted, then re-admission through `MeshSpace.Of`; the finiteness gate routes `GeometryFault.DegenerateInput` with the offending column, every other failure `MeshSpace.Of`'s own rail. `Kernels.WeldDuplicates` welds in place at the arena's `WeldTolerance`, sweeping to a merge-free pass, total.
- Auto: `Of(MeshSpace)` calls `DuplicateNative` once, triangulates quads through the exact diagonal gate, and bulk-fills the columns; capacity grows by doubling every column together, so the offset-page under-allocation class — a store sized `2n` where the algorithm writes past it — is structurally impossible. `KillFace` tombstones a row and `ToSpace` compacts, the sentinel arena-internal and never observable past the freeze; dirty tracking is two packed bitsets replacing persistent `Set<int>` accumulation, enumerated for incremental consumers and cleared once the admission fill completes so a set bit names a kernel edit and never the build; `Parallel` runs a caller struct action over a caller-named index extent via `ParallelHelper`, allocation-free with the floor a policy row; the per-corner UV pair rents lazily on first `SetCornerUv`, rides faces so a weld never disturbs it, ingests per-vertex native texture coordinates at `Of(MeshSpace)`, and publishes wedge-faithfully at the freeze — a seam vertex whose corners disagree splits once per distinct UV, so no island's UV overwrites another's.
- Receipt: none — the arena is build state, not evidence; the `MeshSpace` the freeze publishes is the receipt-bearing artifact, and dirty bitsets are working state a consumer projects.
- Packages: CommunityToolkit.HighPerformance (span planes, `ArrayPool` rent-resize, pooled kernel staging, `ParallelHelper` struct-action folds, packed bitsets), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll<double>` the freeze gate), `Rasm.Meshing` (`MeshSpace`/`MeshSpace.Of` freeze re-admission and native `Mesh` rebuild), `Rasm.Numerics` (`Predicate.Orient2D` + `Axis` the exact quad-diagonal gate), Rasm.Domain (`Context`, `Op`, `Kind`), LanguageExt.Core (`Fin` the freeze rail), Rhino.Geometry (`Mesh`/`MeshFace`/`Point3d` native seam, `Transform`/`Transform.Determinant` the transform pass's map and its orientation discriminant), BCL inbox (`ArrayPool<T>`).
- Growth: a new bulk mutation — an edge-split pass, a tangential-relax sweep, a further per-vertex or per-corner attribute column — is one arena verb or one further SoA column on the same rent/resize/dirty machinery the UV pair already rides; a new build primitive is one `Kernels` member over the same columns; a new parallel fold is one struct action; zero new carriers.
- Boundary: `MeshEdit.Of` owns the kernel's one triangle-soup adapter, every consumer composing it rather than a per-page `Soup(MeshSpace)` copy; the weld kernel and its `WeldTolerance` knob live here — dedup-on-arena is an arena op, reached through no healing policy; the transform pass owns MIRRORED geometry estate-wide, so a consumer needing a reflected part builds it as an admitted mesh through `Kernels.Apply(MeshEdit.Of(space), Transform.Mirror(plane))` and never places an admitted mesh under a reversing transform, which silently inverts the orientation its admission just proved; in-place span kernels inside `MeshEdit`/`Kernels` are the arena tier's statement exemption, never leaking past the freeze, so every public egress is a span view, a value, or the `Fin<MeshSpace>` rail.

```csharp signature
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
// WeldTolerance is THE weld knob, sited on the arena so dedup is an arena op; ParallelFloor feeds every ParallelHelper fold as minimumActionsPerThread.
// The freeze publishes float32 vertices — the native Mesh vertex list is Point3f — so re-admission quantizes every
// coordinate onto a lattice whose spacing at magnitude 1e3 is ~6e-5, COARSER than the 1e-6 band. Two consequences are
// page law: a re-admitted vertex can land across a weld class the pre-freeze arena resolved, and a band tightened
// below the float32 spacing at working magnitude welds nothing a freeze has already touched.
public sealed record ArenaPolicy(int Capacity, double WeldTolerance, int ParallelFloor) {
    public static readonly ArenaPolicy Canonical = new(Capacity: 1_024, WeldTolerance: 1e-6, ParallelFloor: 4_096);
}

// --- [MODELS] ---------------------------------------------------------------------------------
public sealed class MeshEdit : IDisposable {
    // Single-writer pooled SoA; columns rent from ArrayPool<T>.Shared and grow together by doubling. Statement bodies are the arena-tier exemption.
    double[] x, y, z;
    int[] tri;
    // Per-corner (wedge) UV — rides the FACE corner, never the vertex, so a seam vertex carries one UV per island.
    // Rented on first write; a UV-free arena allocates neither column.
    double[]? uvU, uvV;
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

    public static MeshEdit Of(ReadOnlySpan<Point3d> vertices, ReadOnlySpan<(int A, int B, int C)> faces, ArenaPolicy? policy = null) {
        MeshEdit edit = new(policy ?? ArenaPolicy.Canonical);
        foreach (Point3d p in vertices) edit.AddVertex(p);
        foreach ((int a, int b, int c) in faces) edit.AddFace(a, b, c);
        edit.Baseline();
        return edit;
    }

    // Admission is not mutation: the fill runs through AddVertex/AddFace, which dirty every slot they write, so both
    // bitsets clear once the fill completes. A set bit therefore always names a kernel edit and never the build, and a
    // consumer's affected seed carries information instead of the whole mesh.
    void Baseline() { Array.Clear(dirtyVertex); Array.Clear(dirtyFace); }

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
    public bool HasUv => uvU is not null;
    // Meaningful only under HasUv — the read projection mirrors Face, one triple per live face.
    public (Point2d A, Point2d B, Point2d C) CornerUv(int f) =>
        (new Point2d(uvU![3 * f], uvV![3 * f]), new Point2d(uvU[3 * f + 1], uvV[3 * f + 1]), new Point2d(uvU[3 * f + 2], uvV[3 * f + 2]));
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

    // Corner rewrite: edge-collapse re-point and edge-flip land here; face indices stay stable, so a face-keyed queue survives the mutation.
    public void SetFace(int f, int a, int b, int c) {
        (tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]) = (a, b, c);
        BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    }

    // Wedge attribute write — the whole corner triple in one verb so a face never holds a partial UV row; columns
    // rent lazily and grow with tri on the same doubling machinery.
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
    public void Parallel<TAction>(int extent, in TAction action) where TAction : struct, IAction =>
        ParallelHelper.For(0, extent, in action, policy.ParallelFloor);

    // --- [FREEZE] — publish-by-freeze: the ONE admission re-entry
    public Fin<MeshSpace> ToSpace(Context context, Op? key = null) {
        if (!TensorPrimitives.IsFiniteAll<double>(X) || !TensorPrimitives.IsFiniteAll<double>(Y) || !TensorPrimitives.IsFiniteAll<double>(Z)) {
            return Fin.Fail<MeshSpace>(new GeometryFault.DegenerateInput(
                Kind.Mesh, FirstNonFinite(), "non-finite arena coordinate").ToError());
        }
        Mesh mesh = new();
        if (uvU is null) {
            for (int v = 0; v < vertexCount; v++) mesh.Vertices.Add(x[v], y[v], z[v]);
            for (int f = 0; f < faceCount; f++) {
                if (Alive(f)) mesh.Faces.AddFace(tri[3 * f], tri[3 * f + 1], tri[3 * f + 2]);
            }
        }
        else {
            SplitWedges(mesh, context);
        }
        mesh.Compact();  // culls orphaned vertices (tombstoned-face residue) before re-admission
        mesh.RebuildNormals();
        return MeshSpace.Of(mesh, context, key: key);
    }

    // Wedge-faithful publish: native texture coordinates are per-vertex, so a vertex whose incident live corners
    // disagree past Context.Absolute splits at the freeze — one duplicate per distinct corner UV, faces re-pointed,
    // TextureCoordinates filled per published vertex. The last-island-wins seam loss is unrepresentable past this
    // gate; a vertex whose corners agree publishes once.
    void SplitWedges(Mesh mesh, Context context);

    public void Dispose() {
        ArrayPool<double>.Shared.Return(x); ArrayPool<double>.Shared.Return(y); ArrayPool<double>.Shared.Return(z);
        ArrayPool<int>.Shared.Return(tri);
        if (uvU is not null) { ArrayPool<double>.Shared.Return(uvU); ArrayPool<double>.Shared.Return(uvV!); }
        ArrayPool<ulong>.Shared.Return(dirtyVertex); ArrayPool<ulong>.Shared.Return(dirtyFace);
    }

    // In-place weld compaction: class centroids overwrite the live columns (write head trails read head), every face triple remaps, a triple
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
    // Union-find over a tolerance grid, compacted IN PLACE on the SoA columns — no scratch copy of the store. The weld
    // band reads off the arena's ArenaPolicy knob. ONE pass is not a fixpoint: a class collapses to its centroid, which
    // migrates up to half the band toward a neighbour and can chain two classes the same pass measured apart, so the
    // sweep repeats until a pass merges nothing. Every merging pass strictly shrinks the vertex extent, so the loop is
    // bounded by the vertex count, and the published arena IS idempotent — re-welding re-runs one merge-free pass.
    public static MeshEdit WeldDuplicates(MeshEdit edit) {
        while (Merged(edit) > 0) { }
        return edit;

        static int Merged(MeshEdit edit) {
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

            static (long, long, long) Cell(MeshEdit edit, int v, double tolerance) => (
                (long)Math.Floor(edit.X[v] / tolerance), (long)Math.Floor(edit.Y[v] / tolerance), (long)Math.Floor(edit.Z[v] / tolerance));

            static int Find(Span<int> parent, int i) { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; }

            static void Union(Span<int> parent, int a, int b) {
                int ra = Find(parent, a), rb = Find(parent, b);
                if (ra != rb) parent[int.Max(ra, rb)] = int.Min(ra, rb);
            }
        }
    }

    // --- [TRANSFORM]
    // One affine pass over the coordinate columns whose ORIENTATION repair is DERIVED, never requested: an
    // orientation-reversing map is exactly a negative determinant, so every live face's corner order — and its
    // wedge UV triple with it — reverses in the same sweep and the published winding still faces outward. A plane
    // mirror, a handedness swap, and a negative-axis scale are all `Transform` values, so the mirror capability is
    // this one primitive rather than a plane-shaped sibling, and no arm carries a mirror flag beside the map. A
    // non-finite map needs no verdict here: the arena is total and the freeze's finiteness gate owns it. Normals
    // re-derive at the freeze, so no consumer re-winds downstream.
    public static MeshEdit Apply(MeshEdit edit, Transform xform) {
        for (int v = 0; v < edit.VertexCount; v++) edit.SetPosition(v, xform * edit.Position(v));
        if (xform.Determinant >= 0.0) return edit;
        for (int f = 0; f < edit.FaceCount; f++) {
            if (!edit.Alive(f)) continue;
            (int a, int b, int c) = edit.Face(f);
            edit.SetFace(f, c, b, a);
            if (!edit.HasUv) continue;
            (Point2d ua, Point2d ub, Point2d uc) = edit.CornerUv(f);
            edit.SetCornerUv(f, uc, ub, ua);
        }
        return edit;
    }

    // --- [QUAD_DIAGONAL]
    // Exact quad-split gate the mesh-ingress triangulation rides: true selects the A-C diagonal, false the B-D.
    // One diagonal is interior when it separates the other two corners on the quad's dominant-axis projection
    // (Axis.DominantOf); the axis choice is float, the separation signs exact. A degenerate quad has no dominant
    // axis and collapses to the canonical B-D false. The bool verdict is frozen — cross-tier consumers bind it,
    // so the refusal resolves here rather than widening the ingress rail to Fin<bool>.
    public static bool QuadDiagonal(Point3d a, Point3d b, Point3d c, Point3d d) =>
        Axis.DominantOf(a, b, c, d).Case is Axis axis
        && Predicate.Orient2D(a, c, b, axis).Times(Predicate.Orient2D(a, c, d, axis)) == Sign.Negative;
}
```

## [03]-[ARENA_LAW]

One contract carries store mutability and arena concurrency; a sibling store composes it by name.

- Single-writer: an arena has exactly one mutating owner for its lifetime — no lock, no CAS, no interior synchronization. Concurrency enters only through the two sanctioned read modes: a frozen post-freeze projection, or partition-disjoint spans each parallel worker owns through `ParallelHelper` struct actions at the policy floor.
- Publish-by-freeze: build state becomes composable truth only through the freeze seam — `ToSpace` → `MeshSpace.Of` here, the analogous emission projection on every sibling arena. Consumers hold the frozen artifact, never a live arena across an ownership boundary.
- Hash-eligibility: content addressing — the reconciliation `Encode` chain over `XxHash128` — binds only frozen projections; a mid-build arena is never hashed, cached by content, or interned, and no arena mints a span-combine hash of its own.
- Derived-state caching: derived solver state keys on the frozen snapshot reference and dies with it; `mesh.md`'s `LaplacianCache` owns that pattern, and an arena mints no second derived-state cache.
- Capacity: every arena column grows by amortized doubling — `MeshEdit` through the pooled `ArrayPoolExtensions.Resize` verb, the heap-array sibling stores through `Array.Resize` at the same doubling law; the law is the doubling, the verb follows the store's storage class, and a store sized once from an input-derived `2n` guess is the rejected under-allocation.
- Fault surface: arena mutation is total; failure surfaces at freeze and re-admission through the publishing seam's existing rail, and no arena mints a fault union.

## [04]-[DENSITY_BAR]

One arena, one policy row, one kernel family; capability is a row, case, or fold arm, never a sibling surface. Each `[RAIL]` cell names the owner's one return rail, the per-axis kind on the indexed notes below.

| [INDEX] | [AXIS_CONCERN] | [OWNER]       | [RAIL]                                                  | [CASES] |
| :-----: | :------------- | :------------ | :------------------------------------------------------ | :-----: |
|  [01]   | Arena policy   | `ArenaPolicy` | value (composed by healing/arrangement policies)        |    —    |
|  [02]   | Build arena    | `MeshEdit`    | `ToSpace(Context, Op?) → Fin<MeshSpace>` (the ONE rail) |    2    |
|  [03]   | Arena kernels  | `Kernels`     | total (mutates the arena; no rail)                      |    3    |

- [01]-[ARENA_POLICY]: `record` policy row — capacity seed, weld-tolerance knob, parallel floor.
- [02]-[BUILD_ARENA]: `sealed class` single-writer pooled SoA — one polymorphic `Of` (space | soup), mutation verbs, dirty bitsets, partition-disjoint `Parallel`, freeze.
- [03]-[ARENA_KERNELS]: static primitive family — union-find tolerance-grid weld (in-place, swept to a merge-free pass), determinant-derived affine transform with its orientation repair, exact quad-diagonal gate over the `Axis.DominantOf` plane admission.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
