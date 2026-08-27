# 1. Put welding on the arena that owns the mutated state

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:259-264,270,276,295,297,300-301`, anchors `Kernels.WeldDuplicates`, local `Merged`, and its arena reads.

**From:**

```csharp
public static MeshEdit WeldDuplicates(MeshEdit edit) {
    while (Merged(edit) > 0) { }
    return edit;

    static int Merged(MeshEdit edit) {
        double band = edit.Tolerance.For(edit.Policy.Weld).Value;
        int n = edit.VertexCount;
```

```csharp
(long cx, long cy, long cz) = Cell(edit, v, band);
if (edit.Position(v).DistanceTo(edit.Position(u)) <= band) Union(parent, v, u);
(sumX[w], sumY[w], sumZ[w], classSize[w]) =
    (sumX[w] + edit.X[v], sumY[w] + edit.Y[v], sumZ[w] + edit.Z[v], classSize[w] + 1);
edit.Compact(classes, sumX, sumY, sumZ, classSize, remap);
```

```csharp
static (long, long, long) Cell(MeshEdit edit, int v, double band) => (
    (long)Math.Floor(edit.X[v] / band), (long)Math.Floor(edit.Y[v] / band), (long)Math.Floor(edit.Z[v] / band));
```

**To:**

```csharp
public MeshEdit Weld() {
    while (Merged() > 0) { }
    return this;

    int Merged() {
        double band = context.For(policy.Weld).Value;
        int n = vertexCount;
```

```csharp
(long cx, long cy, long cz) = Cell(v, band);
if (Position(v).DistanceTo(Position(u)) <= band) Union(parent, v, u);
(sumX[w], sumY[w], sumZ[w], classSize[w]) =
    (sumX[w] + x[v], sumY[w] + y[v], sumZ[w] + z[v], classSize[w] + 1);
Compact(classes, sumX, sumY, sumZ, classSize, remap);
```

```csharp
(long, long, long) Cell(int v, double band) => (
    (long)Math.Floor(x[v] / band), (long)Math.Floor(y[v] / band), (long)Math.Floor(z[v] / band));
```

**Why:** welding mutates one `MeshEdit`, reads only that arena's context, policy, and columns, and returns the same instance. Seating the verb on its state owner removes the receiver parameter and lets its implementation use private storage directly; the later `Kernels` deletion then removes a module-level type without replacing it. The fixed-point sweep and fluent result remain unchanged.

**Ripples:** replace `Kernels.WeldDuplicates(edit)` with `edit.Weld()` in `libs/dotnet/Rasm/.planning/Meshing/arrangement.md:420`; replace `Kernels.WeldDuplicates(s.Edit)` with `s.Edit.Weld()` in `libs/dotnet/Rasm/.planning/Processing/repair.md:172`; update the matching owner, package, boundary, and flow prose in those pages and the target.

# 2. Replace the hand-rolled weld partition with QuikGraph

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:26-37,266-268,276,282-283,300-310`, anchors the import block, weld parent rental, union calls, representative reads, and local `Find`/`Union` functions.

**From:**

```csharp
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
```

```csharp
using SpanOwner<int> parentOwner = SpanOwner<int>.Allocate(n);
Span<int> parent = parentOwner.Span;
for (int v = 0; v < n; v++) parent[v] = v;
```

```csharp
if (Position(v).DistanceTo(Position(u)) <= band) Union(parent, v, u);
```

```csharp
for (int v = 0; v < n; v++) if (Find(parent, v) == v) remap[v] = classes++;
for (int v = 0; v < n; v++) remap[v] = remap[Find(parent, v)];
```

```csharp
static int Find(Span<int> parent, int i) { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; }

static void Union(Span<int> parent, int a, int b) {
    int ra = Find(parent, a), rb = Find(parent, b);
    if (ra != rb) parent[int.Max(ra, rb)] = int.Min(ra, rb);
}
```

**To:**

```csharp
using QuikGraph.Collections;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.Geometry;
```

```csharp
ForestDisjointSet<int> partition = new(capacity: n);
for (int v = 0; v < n; v++) partition.MakeSet(v);
```

```csharp
if (Position(v).DistanceTo(Position(u)) <= band) partition.Union(v, u);
```

```csharp
for (int v = 0; v < n; v++) if (partition.FindSet(v) == v) remap[v] = classes++;
for (int v = 0; v < n; v++) remap[v] = remap[partition.FindSet(v)];
```

```csharp
// Find DELETED
// Union DELETED
```

**Why:** the checked-in QuikGraph catalogue defines `ForestDisjointSet<int>` with `MakeSet`, `Union`, and `FindSet`, and QuikGraph is already a direct Rasm dependency. It replaces one pooled parent rental plus two hand-written local algorithms with the admitted partition owner while preserving the same representative-to-dense-remap fold. Update the target's package row to name QuikGraph for weld partitioning; no manifest change is needed.

# 3. Fold weld sums into one pooled row and narrow compaction

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:219-232,283-298`, anchors `MeshEdit.Compact` and the four weld accumulator rentals.

**From:**

```csharp
internal void Compact(int classes, ReadOnlySpan<double> sumX, ReadOnlySpan<double> sumY,
    ReadOnlySpan<double> sumZ, ReadOnlySpan<int> classSize, ReadOnlySpan<int> remap) {
    for (int w = 0; w < classes; w++) {
        (x[w], y[w], z[w]) =
            (sumX[w] / classSize[w], sumY[w] / classSize[w], sumZ[w] / classSize[w]);
```

```csharp
using SpanOwner<int> sizeOwner = SpanOwner<int>.Allocate(classes, AllocationMode.Clear);
using SpanOwner<double> sxOwner = SpanOwner<double>.Allocate(classes, AllocationMode.Clear);
using SpanOwner<double> syOwner = SpanOwner<double>.Allocate(classes, AllocationMode.Clear);
using SpanOwner<double> szOwner = SpanOwner<double>.Allocate(classes, AllocationMode.Clear);
(Span<int> classSize, Span<double> sumX, Span<double> sumY, Span<double> sumZ) =
    (sizeOwner.Span, sxOwner.Span, syOwner.Span, szOwner.Span);
for (int v = 0; v < n; v++) {
    int w = remap[v];
    (sumX[w], sumY[w], sumZ[w], classSize[w]) =
        (sumX[w] + x[v], sumY[w] + y[v], sumZ[w] + z[v], classSize[w] + 1);
}
Compact(classes, sumX, sumY, sumZ, classSize, remap);
```

**To:**

```csharp
void Compact(int classes, ReadOnlySpan<(double X, double Y, double Z, int Count)> sums,
    ReadOnlySpan<int> remap) {
    for (int w = 0; w < classes; w++) {
        (double sx, double sy, double sz, int count) = sums[w];
        (x[w], y[w], z[w]) = (sx / count, sy / count, sz / count);
```

```csharp
using SpanOwner<(double X, double Y, double Z, int Count)> sumsOwner =
    SpanOwner<(double X, double Y, double Z, int Count)>.Allocate(classes, AllocationMode.Clear);
Span<(double X, double Y, double Z, int Count)> sums = sumsOwner.Span;
for (int v = 0; v < n; v++) {
    ref (double X, double Y, double Z, int Count) sum = ref sums[remap[v]];
    sum = (sum.X + x[v], sum.Y + y[v], sum.Z + z[v], sum.Count + 1);
}
Compact(classes, sums, remap);
```

**Why:** the coordinate totals and class count are one accumulator row: they share an index, lifetime, clear requirement, update, and read. One pooled tuple span replaces four rentals, and `Compact` drops three parameters. Moving welding onto `MeshEdit` also makes `Compact` owner-private; no assembly consumer remains.

# 4. Delete the weld-only policy projection

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:114`, anchor `MeshEdit.Policy`.

**From:**

```csharp
public ArenaPolicy Policy => policy;
```

**To:**

```csharp
// MeshEdit.Policy DELETED
```

**Why:** after task 1, welding reads the policy field on its owning arena and no repository consumer reads `MeshEdit.Policy`. `ArenaPolicy` remains a real shared policy type, remains configurable at both `Of` entries, and remains readable where higher policies carry it; only this one-hop arena forwarding member disappears.

# 5. Put affine mutation on the arena and mark dirty words after the parallel pass

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:312-327`, anchors `TransformAction` and `Kernels.Apply`.

**From:**

```csharp
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
```

**To:**

```csharp
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
        if (CornerUv(f).Case is (Point2d ua, Point2d ub, Point2d uc)) SetCornerUv(f, uc, ub, ua);
    }
    return this;
}
```

**Why:** affine application is an in-place arena verb, so the receiver parameter and sibling owner are unnecessary. More importantly, the current parallel action calls `SetPosition`, causing adjacent workers to read-modify-write the same packed `ulong` dirty word; partition-disjoint vertex indices do not make those word writes disjoint. The action now writes only its three index-disjoint coordinate slots, and the owning thread marks the live dirty-word range after `ParallelHelper` returns. Padding bits are unobservable because both dirty enumerators stop at the live counts.

**Ripples:** replace `Kernels.Apply(MeshEdit.Of(part.Model), orientation.ModelToBuild)` with `MeshEdit.Of(part.Model).Apply(orientation.ModelToBuild)` in `libs/dotnet/Rasm.Fabrication/.planning/Additive/production.md:1524`; replace `Kernels.Apply(arena, pose)` with `arena.Apply(pose)` in `libs/dotnet/Rasm/.planning/Drawing/view.md:391`; update every matching package, boundary, flow, and diagram mention in those pages and the target.

# 6. Store each corner UV as one pooled value

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:51,125-128,159-165,196,215`, anchors the UV fields, `CornerUv`, `SetCornerUv`, freeze branch, and disposal.

**From:**

```csharp
double[]? uvU, uvV;
```

```csharp
public Option<(Point2d A, Point2d B, Point2d C)> CornerUv(int f) =>
    uvU is null ? None
        : Some((new Point2d(uvU[3 * f], uvV![3 * f]), new Point2d(uvU[3 * f + 1], uvV[3 * f + 1]),
                new Point2d(uvU[3 * f + 2], uvV[3 * f + 2])));
```

```csharp
uvU ??= ArrayPool<double>.Shared.Rent(tri.Length);
uvV ??= ArrayPool<double>.Shared.Rent(tri.Length);
Grow(ref uvU, 3 * (f + 1)); Grow(ref uvV, 3 * (f + 1));
(uvU[3 * f], uvV[3 * f]) = (a.X, a.Y);
(uvU[3 * f + 1], uvV[3 * f + 1]) = (b.X, b.Y);
(uvU[3 * f + 2], uvV[3 * f + 2]) = (c.X, c.Y);
```

```csharp
if (uvU is null) {
```

```csharp
if (uvU is not null) { ArrayPool<double>.Shared.Return(uvU); ArrayPool<double>.Shared.Return(uvV!); }
```

**To:**

```csharp
Point2d[]? uv;
```

```csharp
public Option<(Point2d A, Point2d B, Point2d C)> CornerUv(int f) =>
    uv is null ? None : Some((uv[3 * f], uv[3 * f + 1], uv[3 * f + 2]));
```

```csharp
uv ??= ArrayPool<Point2d>.Shared.Rent(tri.Length);
Grow(ref uv, 3 * (f + 1));
(uv[3 * f], uv[3 * f + 1], uv[3 * f + 2]) = (a, b, c);
```

```csharp
if (uv is null) {
```

```csharp
if (uv is not null) ArrayPool<Point2d>.Shared.Return(uv);
```

**Why:** U and V are never read, written, resized, or released independently; the stored unit is a `Point2d` corner. One pooled value column removes a private field, one rental, one resize, coordinate reconstruction at every read, and paired disposal while preserving lazy allocation, wedge-local UV identity, and the genuine `CornerUv` read capability. The target's UV and `SplitWedges` prose must name the single `uv` column.

# 7. Reverse UV corners in place

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:324`, anchor the reversing-transform UV arm in `Kernels.Apply`.

**From:**

```csharp
if (CornerUv(f).Case is (Point2d ua, Point2d ub, Point2d uc)) SetCornerUv(f, uc, ub, ua);
```

**To:**

```csharp
if (uv is not null) (uv[3 * f], uv[3 * f + 2]) = (uv[3 * f + 2], uv[3 * f]);
```

**Why:** once `Apply` is an arena member and UVs occupy one value column, reversing a face needs one first/third tuple swap. The current route constructs an `Option`, reconstructs three points, writes all three values back, and dirty-marks the face a second time after `SetFace` already marked it. `CornerUv` remains as the arena's genuine read projection; only the mutation path's round trip disappears.

# 8. Move the quad split gate onto the existing mesh kernel and delete `Kernels`

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:81,257,329-333`, anchors the `Of(MeshSpace)` quad call, `public static class Kernels`, and `QuadDiagonal`.

**From:**

```csharp
if (Kernels.QuadDiagonal(edit.Position(face.A), edit.Position(face.B), edit.Position(face.C), edit.Position(face.D))) {
```

```csharp
public static class Kernels {
```

```csharp
public static bool QuadDiagonal(Point3d a, Point3d b, Point3d c, Point3d d) =>
    Axis.DominantOf(a, b, c, d).Case is Axis axis
    && Predicate.Orient2D(a, c, b, axis).Times(Predicate.Orient2D(a, c, d, axis)) == Sign.Negative;
```

**To:**

```csharp
if (MeshKernel.QuadDiagonal(edit.Position(face.A), edit.Position(face.B), edit.Position(face.C), edit.Position(face.D))) {
```

```csharp
// Kernels DELETED
// Kernels.QuadDiagonal DELETED
```

**Why:** tasks 1 and 5 move both mutating operations onto their state owner, leaving `Kernels` as a one-member shell. The exact split gate does not read or mutate an arena, and one of its four callers is already inside `MeshKernel.AssembleCotangent`; placing it on `MeshEdit` would turn the arena into a static utility host. Relocating the unchanged predicate body onto the existing mesh-substrate kernel removes one module-level type without adding a type, wrapper, or second implementation.

**Ripples:** add the unchanged gate to the existing `MeshKernel` declaration in `libs/dotnet/Rasm/.planning/Meshing/mesh.md:644`:

```csharp
internal static bool QuadDiagonal(Point3d a, Point3d b, Point3d c, Point3d d) =>
    Axis.DominantOf(a, b, c, d).Case is Axis axis
    && Predicate.Orient2D(a, c, b, axis).Times(Predicate.Orient2D(a, c, d, axis)) == Sign.Negative;
```

Replace `Kernels.QuadDiagonal` with `MeshKernel.QuadDiagonal` at `libs/dotnet/Rasm/.planning/Meshing/mesh.md:689` and `libs/dotnet/Rasm/.planning/Processing/remesh.md:330,517`; update matching prose in those pages and `libs/dotnet/Rasm/.api/api-rhino.md:113`. Rewrite the target's owner, index, package, boundary, density, and flow prose around `MeshEdit` as its sole owner, then remove every remaining `Kernels` name from the target.

# 9. Build face bounds from verified RhinoCommon extent operations

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:129-130`, anchor `MeshEdit.Bounds`.

**From:**

```csharp
public BoundingBox Bounds(int f) =>
    new([Position(tri[3 * f]), Position(tri[3 * f + 1]), Position(tri[3 * f + 2])]);
```

**To:**

```csharp
public BoundingBox Bounds(int f) => BoundingBox.Union(
    new(Position(tri[3 * f]), Position(tri[3 * f + 1])), new(Position(tri[3 * f + 2]), Position(tri[3 * f + 2])));
```

**Why:** the checked-in RhinoCommon catalogue exposes `BoundingBox(Point3d, Point3d)` and static `BoundingBox.Union(BoundingBox, BoundingBox)`, not the one-argument point-collection construction in the current fence. The verified value-type composition avoids allocating a three-point collection without adding fenced LOC or another helper.

# 10. Delete the mutation-free manual dirty marker

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:174-177`, anchor `MeshEdit.Touch`.

**From:**

```csharp
public void Touch(ReadOnlySpan<int> faces, ReadOnlySpan<int> vertices) {
    foreach (int f in faces) BitHelper.SetFlag(ref dirtyFace[f >> 6], f & 63, true);
    foreach (int v in vertices) BitHelper.SetFlag(ref dirtyVertex[v >> 6], v & 63, true);
}
```

**To:**

```csharp
// MeshEdit.Touch DELETED
```

**Why:** every writable arena path marks the exact state it changes: vertex and face insertion, position and face replacement, corner UV writes, face death, compaction, and the bulk transform. The arena publishes no writable raw projection that would require mark-after-write, no consumer calls `Touch`, and this member can manufacture dirty evidence without any mutation. The complete dirty enumerators remain as the genuine read capability.

# 11. Inline the per-slot dirty wrappers into their only readers

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:179-183`, anchors `DirtyVertex`, `DirtyFace`, `DirtyVertices`, and `DirtyFaces`.

**From:**

```csharp
public bool DirtyVertex(int v) => BitHelper.HasFlag(dirtyVertex[v >> 6], v & 63);
public bool DirtyFace(int f) => BitHelper.HasFlag(dirtyFace[f >> 6], f & 63);

public IEnumerable<int> DirtyVertices() { for (int v = 0; v < vertexCount; v++) if (DirtyVertex(v)) yield return v; }
public IEnumerable<int> DirtyFaces() { for (int f = 0; f < faceCount; f++) if (DirtyFace(f)) yield return f; }
```

**To:**

```csharp
// MeshEdit.DirtyVertex DELETED
// MeshEdit.DirtyFace DELETED
public IEnumerable<int> DirtyVertices() { for (int v = 0; v < vertexCount; v++) if (BitHelper.HasFlag(dirtyVertex[v >> 6], v & 63)) yield return v; }
public IEnumerable<int> DirtyFaces() { for (int f = 0; f < faceCount; f++) if (BitHelper.HasFlag(dirtyFace[f >> 6], f & 63)) yield return f; }
```

**Why:** the complete consumer read finds only the two full dirty enumerators outside this fence; each per-slot predicate is a one-expression forwarding member called only by its matching enumerator. Inlining the packed-bit read removes two public members while preserving the genuine complete dirty projections consumed by `Processing/session`.

# 12. Inline the one-use finiteness locator at freeze

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:190-194,234-240`, anchors `ToSpace` and private `NonFinite`.

**From:**

```csharp
if (NonFinite().Case is int slot) {
    return Fin.Fail<MeshSpace>(new GeometryFault.DegenerateInput(
        Kind.Mesh, slot, "non-finite arena coordinate"));
}
```

```csharp
Option<int> NonFinite() {
    if (TensorPrimitives.IsFiniteAll<double>(X) && TensorPrimitives.IsFiniteAll<double>(Y)
        && TensorPrimitives.IsFiniteAll<double>(Z)) return None;
    for (int v = 0; v < vertexCount; v++) {
        if (!double.IsFinite(x[v]) || !double.IsFinite(y[v]) || !double.IsFinite(z[v])) return Some(v);
    }
    return None;
}
```

**To:**

```csharp
if (vertexCount > 0 && !(TensorPrimitives.IsFiniteAll<double>(X)
    && TensorPrimitives.IsFiniteAll<double>(Y) && TensorPrimitives.IsFiniteAll<double>(Z))) {
    int slot = 0;
    while (double.IsFinite(x[slot]) && double.IsFinite(y[slot]) && double.IsFinite(z[slot])) slot++;
    return new GeometryFault.DegenerateInput(Kind.Mesh, slot, "non-finite arena coordinate");
}

// MeshEdit.NonFinite DELETED
```

**Why:** `NonFinite` has one caller and only shapes this freeze branch. Inlining removes a private member and temporary `Option<int>` while retaining the vectorized all-finite fast path and running the scalar locator only after that gate proves a bad coordinate exists. The explicit non-empty guard preserves empty-arena behavior because the tensors catalogue states that `IsFiniteAll` returns false for an empty span; the bare fault is the standards-owned target-typed `Error -> Fin<MeshSpace>` lift.

# 13. Use ceiling division for packed dirty-word capacity

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:64-65,246-247`, anchors the constructor dirty rentals and `GrowBits`.

**From:**

```csharp
dirtyVertex = ArrayPool<ulong>.Shared.Rent((seed >> 6) + 1);
dirtyFace = ArrayPool<ulong>.Shared.Rent((seed >> 6) + 1);
```

```csharp
int needed = (slots >> 6) + 1;
```

**To:**

```csharp
dirtyVertex = ArrayPool<ulong>.Shared.Rent(((seed - 1) >> 6) + 1);
dirtyFace = ArrayPool<ulong>.Shared.Rent(((seed - 1) >> 6) + 1);
```

```csharp
int needed = ((slots - 1) >> 6) + 1;
```

**Why:** one `ulong` holds 64 slots, so the required word count is `ceil(slots / 64)`. The current formula rents an extra word whenever a positive count is an exact multiple of 64, including the canonical 1,024-slot seed. `Dimension` admits positive capacity and both grow callers pass `count + 1`, so subtract-before-shift is defined and avoids the checked-overflow defect of `(slots + 63) >> 6`.

# 14. Inline the native UV projection at its only call site

**Location:** `libs/dotnet/Rasm/.planning/Meshing/edit.md:90-100`, anchors the texture-coordinate admission block and private `UvAt` helper.

**From:**

```csharp
if (native.TextureCoordinates.Count == native.Vertices.Count) {
    for (int f = 0; f < edit.faceCount; f++) {
        (int a, int b, int c) = edit.Face(f);
        edit.SetCornerUv(f, UvAt(native, a), UvAt(native, b), UvAt(native, c));
    }
}
```

```csharp
static Point2d UvAt(Mesh mesh, int v) => new(mesh.TextureCoordinates[v].X, mesh.TextureCoordinates[v].Y);
```

**To:**

```csharp
if (native.TextureCoordinates.Count == native.Vertices.Count) {
    for (int f = 0; f < edit.faceCount; f++) {
        (int a, int b, int c) = edit.Face(f);
        edit.SetCornerUv(f, new(native.TextureCoordinates[a].X, native.TextureCoordinates[a].Y), new(native.TextureCoordinates[b].X, native.TextureCoordinates[b].Y), new(native.TextureCoordinates[c].X, native.TextureCoordinates[c].Y));
    }
}

// MeshEdit.UvAt DELETED
```

**Why:** `UvAt` is a private one-expression conversion used at one admission call site and has no meaning outside that loop. Keeping the explicit `Point2d` construction at the native `Point2f` boundary removes a private member without inventing a conversion wrapper or relying on an uncatalogued implicit conversion; the three corner reads remain exactly the values passed to `SetCornerUv`.
