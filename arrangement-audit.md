# 1. Make the behavior-only vocabularies keyless

### Location

- `arrangement.md:47-84`, anchors `[SmartEnum<int>] public sealed partial class BooleanOp` and `[SmartEnum<int>] public sealed partial class PolygonFill`

### From

```csharp
[SmartEnum<int>]
public sealed partial class BooleanOp {
    public static readonly BooleanOp Union        = new(0, native: 0, static (inA, inB) => inA || inB);
    public static readonly BooleanOp Difference   = new(1, native: 1, static (inA, inB) => inA && !inB);
    public static readonly BooleanOp Intersection = new(2, native: 2, static (inA, inB) => inA && inB);
    public static readonly BooleanOp Xor          = new(3, native: -1, static (inA, inB) => inA ^ inB);
```

```csharp
[SmartEnum<int>]
public sealed partial class PolygonFill {
    public static readonly PolygonFill NonZero  = new(key: 0, static winding => winding != 0);
    public static readonly PolygonFill EvenOdd  = new(key: 1, static winding => (winding & 1) != 0);
    public static readonly PolygonFill Positive = new(key: 2, static winding => winding > 0);
    public static readonly PolygonFill Negative = new(key: 3, static winding => winding < 0);
```

### To

```csharp
[SmartEnum]
public sealed partial class BooleanOp {
    public static readonly BooleanOp Union        = new(native: 0, region: static (inA, inB) => inA || inB);
    public static readonly BooleanOp Difference   = new(native: 1, region: static (inA, inB) => inA && !inB);
    public static readonly BooleanOp Intersection = new(native: 2, region: static (inA, inB) => inA && inB);
    public static readonly BooleanOp Xor          = new(native: -1, region: static (inA, inB) => inA ^ inB);
```

```csharp
[SmartEnum]
public sealed partial class PolygonFill {
    public static readonly PolygonFill NonZero  = new(inside: static winding => winding != 0);
    public static readonly PolygonFill EvenOdd  = new(inside: static winding => (winding & 1) != 0);
    public static readonly PolygonFill Positive = new(inside: static winding => winding > 0);
    public static readonly PolygonFill Negative = new(inside: static winding => winding < 0);
```

Change the target owner and density descriptions for both vocabularies from `[SmartEnum<int>]` to `[SmartEnum]`; case counts do not change.

### Why

No code or consumer reads, parses, serializes, converts, or looks up either integer key. Keyless Thinktecture smart enums retain `Items`, identity, delegate columns, and exhaustive dispatch while deleting eight authored keys and the generated key, conversion, parse, and lookup surface.

# 2. Delete the native-operation sentinel and its one-call xor wrapper

### Location

- `arrangement.md:49-64`, anchors `BooleanOp.Union` through `BooleanOp.Xor`, `public int Native`, `Keep`, and `Flip`
- `arrangement.md:557-559`, anchor `raw = op.Native >= 0`
- `arrangement.md:613-615`, anchor `static nint Subtract`

### From

```csharp
public static readonly BooleanOp Union        = new(native: 0, region: static (inA, inB) => inA || inB);
public static readonly BooleanOp Difference   = new(native: 1, region: static (inA, inB) => inA && !inB);
public static readonly BooleanOp Intersection = new(native: 2, region: static (inA, inB) => inA && inB);
public static readonly BooleanOp Xor          = new(native: -1, region: static (inA, inB) => inA ^ inB);

public int Native { get; }
```

```csharp
raw = op.Native >= 0
    ? BatchBoolean(raised, op.Native)
    : Subtract(BatchBoolean(raised, BooleanOp.Union.Native), BatchBoolean(raised, BooleanOp.Intersection.Native));
```

```csharp
static nint Subtract(nint left, nint right) {
    try { return manifold_boolean(manifold_alloc_manifold(), left, right, BooleanOp.Difference.Native); }
```

### To

```csharp
public static readonly BooleanOp Union        = new(region: static (inA, inB) => inA || inB);
public static readonly BooleanOp Difference   = new(region: static (inA, inB) => inA && !inB);
public static readonly BooleanOp Intersection = new(region: static (inA, inB) => inA && inB);
public static readonly BooleanOp Xor          = new(region: static (inA, inB) => inA ^ inB);

// BooleanOp.Native DELETED
```

```csharp
raw = op.Switch(
    state: raised,
    union:        static xs => BatchBoolean(xs, op: 0),
    difference:   static xs => BatchBoolean(xs, op: 1),
    intersection: static xs => BatchBoolean(xs, op: 2),
    xor: static xs => {
        nint union = 0, intersection = 0;
        try {
            union = BatchBoolean(xs, op: 0);
            intersection = BatchBoolean(xs, op: 2);
            return manifold_boolean(manifold_alloc_manifold(), union, intersection, op: 1);
        }
        finally {
            if (union != 0) { manifold_delete_manifold(union); }
            if (intersection != 0) { manifold_delete_manifold(intersection); }
        }
    });
```

```csharp
// ManifoldGate.Subtract DELETED
```

Remove the target prose claims that `BooleanOp` carries a `Native` column and `Xor` carries `-1`; describe the exhaustive native-boundary map instead.

### Why

`-1` is an absence sentinel whose only reader re-discovers the `Xor` case. The generated exhaustive `Switch` is the proper correspondence from the four domain rows to the three native ordinals plus the derived xor operation. Keeping that derived arm's custody in the arm also removes the one-call `Subtract` hop. The result loses one public member and one private member, and a fifth boolean case becomes a compile-time break at the native boundary.

# 3. Nest the patch arena under `Arrangement`

### Location

- `arrangement.md:152-197`, anchor module-level `public sealed class PatchStore`
- `arrangement.md:218`, anchor `public static class Arrangement`

### From

```csharp
public sealed class PatchStore {
    (Point3d A, Point3d B, Point3d C)[] patches;
    bool[] fromA, insideA, insideB;
    int count;
```

### To

```csharp
public static class Arrangement {
    private sealed class PatchStore {
        (Point3d A, Point3d B, Point3d C)[] patches;
        bool[] fromA, insideA, insideB;
        int count;
```

Move the existing constructor, reads, mutation methods, `Centroid`, `Freeze`, and `Grow` body under this nested declaration without changing their logic; remove public/internal modifiers from members because the enclosing owner can read private nested members directly.

Update the target owner, exemption, and density descriptions to call `PatchStore` private arrangement state while retaining `CellSet` as the published projection.

### Why

The arena never crosses `Arrangement.Apply`; only its frozen projection does. A public module-level mutable store exposes an unearned construction and mutation API. Nesting removes one module-level type and its public surface while preserving the deliberate SoA arena and single-writer lifetime.

# 4. Delete the redundant route vocabulary

### Location

- `arrangement.md:67-73`, anchor `public sealed partial class BooleanRoute`
- `arrangement.md:137-147`, anchors `BooleanCensus.Route`, `Empty`, and `operator +`
- `arrangement.md:227-238`, anchor `gate.Route == BooleanRoute.Native`
- `arrangement.md:254-260`, anchor `Gate`
- `arrangement.md:421-422`, managed census construction
- `arrangement.md:566-569`, native census construction

### From

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BooleanRoute {
    public static readonly BooleanRoute Managed = new("managed");
    public static readonly BooleanRoute Native  = new("native");
}
```

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, BooleanRoute Route, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default, Option<ManifoldProvenance> Source = default) {
    public static readonly BooleanCensus Empty = new(0L, 0L, 0L, BooleanRoute.Managed);
```

```csharp
Gate(operands, policy).Bind(gate => (Native: gate.Route == BooleanRoute.Native, keep.Case) switch {
```

```csharp
static Fin<(BooleanRoute Route, long Faces)> Gate(Seq<MeshSpace> operands, ArrangementPolicy policy) {
```

### To

```csharp
// BooleanRoute DELETED
```

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default, Option<ManifoldProvenance> Source = default) {
    public static readonly BooleanCensus Empty = new(0L, 0L, 0L);

    // BooleanCensus.Route DELETED
```

```csharp
Gate(operands, policy).Bind(gate => (gate.Native, keep.Case) switch {
```

```csharp
static Fin<(bool Native, long Faces)> Gate(Seq<MeshSpace> operands, ArrangementPolicy policy) {
    long faces = operands.Sum(static space => (long)space.Native.Faces.Count);
    return operands.Count < 2 || operands.Exists(static space => space.Native.Vertices.Count == 0)
        ? Fin.Fail<(bool, long)>(new GeometryFault.DegenerateInput(Kind.Mesh, operands.Count, "fewer than two operands or an empty operand"))
        : !policy.BeyondManaged(faces) ? Fin.Succ((Native: false, Faces: faces))
        : ManifoldGate.AssetResolves() ? Fin.Succ((Native: true, Faces: faces))
        : Fin.Fail<(bool, long)>(new GeometryFault.ManifoldLibraryUnavailable(RuntimeInformation.RuntimeIdentifier, faces, policy.ScaleCeiling));
}
```

Remove the `Route:` argument from every `BooleanCensus` construction and the `right.Route` argument from `operator +`; `Native.IsSome` is the persistent route evidence.

Remove `BooleanRoute` from the target owner, cases, output, packages, growth, density, and diagram descriptions.

### Why

`BooleanRoute` is a two-case payloadless family, and `BooleanCensus.Native` already distinguishes the arms: every successful native construction supplies `Some(ManifoldEvidence)`, while every managed construction supplies `None`. Keeping both creates two authorities that can disagree and adds a type, two rows, one census member, string keys, and generated lookup/conversion surface.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/session.md`: remove the package prose claim that `BooleanRoute` sits inside `BooleanCensus`; no fence change is required because no consumer reads it.
- `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md`: remove the analogy to the deleted `BooleanRoute` type; retain the managed/native route concept in that page's own owner.

# 5. Seat the surviving native evidence under `BooleanCensus`

### Location

- `arrangement.md:117`, anchor module-level `ManifoldEvidence`
- `arrangement.md:137-147`, anchor `BooleanCensus`
- `arrangement.md:583`, anchor `static ManifoldEvidence Evidence`

### From

```csharp
public sealed record ManifoldEvidence(int Genus, int Vertices, int Edges, int Triangles, double Volume, double SurfaceArea);
```

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default, Option<ManifoldProvenance> Source = default) {
```

```csharp
static ManifoldEvidence Evidence(nint result) =>
```

### To

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default, Option<ManifoldProvenance> Source = default) {
    public sealed record ManifoldEvidence(
        int Genus, int Vertices, int Edges, int Triangles, double Volume, double SurfaceArea);
```

```csharp
static BooleanCensus.ManifoldEvidence Evidence(nint result) =>
```

Qualify the target prose and density row through `BooleanCensus`.

### Why

`ManifoldEvidence` is created only for `BooleanCensus.Native` and no other fence names the type directly. Seating the surviving evidence type at that owner removes one module-level symbol without changing its payload or adding an indirection; the next move can then delete, rather than relocate, the redundant provenance carrier.

# 6. Fold provenance into the native evidence owner

### Location

- `arrangement.md:117-147`, anchors `ManifoldEvidence`, module-level `ManifoldProvenance`, `BooleanCensus.Source`, and `operator +`
- `arrangement.md:564-569`, anchor the successful native census construction
- `arrangement.md:583-599`, anchors `Evidence` and `Provenance`

### From

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default, Option<ManifoldProvenance> Source = default) {
    public sealed record ManifoldEvidence(
        int Genus, int Vertices, int Edges, int Triangles, double Volume, double SurfaceArea);
```

```csharp
public sealed record ManifoldProvenance(int[] OperandIds, uint[] RunIds, (int From, int To)[] RunFaces, ulong[] FaceIds) {
    readonly int[] starts = [.. RunFaces.Select(static window => window.From)];
    readonly Lazy<FrozenDictionary<uint, int>> operandOfRun = new(() =>
        RunIds.Index()
            .Select(row => (Run: row.Item, At: Array.IndexOf(OperandIds, (int)row.Item)))
            .Where(static row => row.At >= 0)
            .ToFrozenDictionary(static row => row.Run, static row => row.At));

    public Option<int> OperandOf(int face) {
        int probe = Array.BinarySearch(starts, face);
        int run = probe >= 0 ? probe : ~probe - 1;
        return run >= 0 && run < RunFaces.Length && face < RunFaces[run].To
            && operandOfRun.Value.TryGetValue(RunIds[run], out int operand)
                ? Some(operand)
                : None;
    }
}
```

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default, Option<ManifoldProvenance> Source = default) {
    public static readonly BooleanCensus Empty = new(0L, 0L, 0L);

    public static BooleanCensus operator +(BooleanCensus left, BooleanCensus right) =>
        new(left.Classified + right.Classified, left.Kept + right.Kept, left.Welded + right.Welded,
            Last(left.ShellCount, right.ShellCount), Last(left.Native, right.Native),
            Last(left.Source, right.Source));
```

```csharp
0 => Shells(observed, context, policy, key)
        .Map(shells => (Shells: shells, Evidence: Evidence(observed), Source: Provenance(observed, seated)))
        .Map(read => (ArrangementResult)new ArrangementResult.Boolean(read.Shells.Solids, new BooleanCensus(
            Classified: classified, Kept: read.Evidence.Triangles, Welded: 0,
            ShellCount: Some(read.Shells.ShellCount),
            Native: Some(read.Evidence), Source: Some(read.Source)))),
```

```csharp
static BooleanCensus.ManifoldEvidence Evidence(nint result) =>
    new(Genus: manifold_genus(result), Vertices: (int)manifold_num_vert(result), Edges: (int)manifold_num_edge(result),
        Triangles: (int)manifold_num_tri(result), Volume: manifold_volume(result), SurfaceArea: manifold_surface_area(result));
```

```csharp
static ManifoldProvenance Provenance(nint result, int[] seated) {
    nint mesh = manifold_get_meshgl64(manifold_alloc_meshgl64(), result);
    try {
        int runs = (int)manifold_meshgl64_num_run(mesh);
        ulong[] bounds = new ulong[(int)manifold_meshgl64_run_index_length(mesh)];
        uint[] ids = new uint[(int)manifold_meshgl64_run_original_id_length(mesh)];
        ulong[] faces = new ulong[(int)manifold_meshgl64_face_id_length(mesh)];
        _ = manifold_meshgl64_run_index(bounds, mesh);
        _ = manifold_meshgl64_run_original_id(ids, mesh);
        _ = manifold_meshgl64_face_id(faces, mesh);
        (int From, int To)[] windows = new (int, int)[runs];
        for (int r = 0; r < runs; r++) { windows[r] = ((int)(bounds[r] / 3), (int)(bounds[r + 1] / 3)); }
        return new ManifoldProvenance(seated, ids, windows, faces);
    }
    finally { manifold_delete_meshgl64(mesh); }
}
```

### To

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default) {
    public sealed record ManifoldEvidence(
        int Genus, int Vertices, int Edges, int Triangles, double Volume, double SurfaceArea,
        Seq<int> OperandIds, Seq<uint> RunIds, Seq<(int From, int To)> RunFaces, Seq<ulong> FaceIds) {
        readonly int[] starts = [.. RunFaces.Select(static window => window.From)];
        readonly Lazy<FrozenDictionary<uint, int>> operandOfRun = new(() =>
            OperandIds.Index().ToFrozenDictionary(static row => (uint)row.Item, static row => row.Index));

        public Option<int> OperandOf(int face) {
            int probe = Array.BinarySearch(starts, face);
            int run = probe >= 0 ? probe : ~probe - 1;
            return run >= 0 && run < RunFaces.Count && face < RunFaces[run].To
                && operandOfRun.Value.TryGetValue(RunIds[run], out int operand)
                    ? Some(operand)
                    : None;
        }
    }
}

// ManifoldProvenance DELETED
```

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default) {
    public static readonly BooleanCensus Empty = new(0L, 0L, 0L);

    public static BooleanCensus operator +(BooleanCensus left, BooleanCensus right) =>
        new(left.Classified + right.Classified, left.Kept + right.Kept, left.Welded + right.Welded,
            Last(left.ShellCount, right.ShellCount), Last(left.Native, right.Native));

    // BooleanCensus.Source DELETED
```

```csharp
0 => Shells(observed, context, policy, key)
        .Map(shells => (Shells: shells, Evidence: Evidence(observed, seated)))
        .Map(read => (ArrangementResult)new ArrangementResult.Boolean(read.Shells.Solids, new BooleanCensus(
            Classified: classified, Kept: read.Evidence.Triangles, Welded: 0,
            ShellCount: Some(read.Shells.ShellCount), Native: Some(read.Evidence)))),
```

```csharp
static BooleanCensus.ManifoldEvidence Evidence(nint result, int[] seated) {
    nint mesh = manifold_get_meshgl64(manifold_alloc_meshgl64(), result);
    try {
        int runs = (int)manifold_meshgl64_num_run(mesh);
        ulong[] bounds = new ulong[(int)manifold_meshgl64_run_index_length(mesh)];
        uint[] ids = new uint[(int)manifold_meshgl64_run_original_id_length(mesh)];
        ulong[] faces = new ulong[(int)manifold_meshgl64_face_id_length(mesh)];
        _ = manifold_meshgl64_run_index(bounds, mesh);
        _ = manifold_meshgl64_run_original_id(ids, mesh);
        _ = manifold_meshgl64_face_id(faces, mesh);
        (int From, int To)[] windows = new (int, int)[runs];
        for (int r = 0; r < runs; r++) { windows[r] = ((int)(bounds[r] / 3), (int)(bounds[r + 1] / 3)); }
        return new BooleanCensus.ManifoldEvidence(
            manifold_genus(result), (int)manifold_num_vert(result), (int)manifold_num_edge(result),
            (int)manifold_num_tri(result), manifold_volume(result), manifold_surface_area(result),
            toSeq(seated), toSeq(ids), toSeq(windows), toSeq(faces));
    }
    finally { manifold_delete_meshgl64(mesh); }
}

// ManifoldGate.Provenance DELETED
```

Update the target owner and output prose to describe one `Option<ManifoldEvidence>` native axis carrying both guarantee and source attribution; remove the independently absent provenance claim and the separate provenance density row.

### Why

The native lane seats every operand before evaluation and constructs guarantee plus attribution together on every successful result; managed results construct neither. Two independent options encode impossible half-native states, and a second provenance record only bundles fields consumed by `ManifoldEvidence.OperandOf`. Folding those fields and the extraction into the native evidence owner removes one declared type, one public census member, one merge projection, and the single-use `Provenance` helper. `Seq<T>` prevents mutation of the published attribution columns, while building the frozen lookup from the seated operand roster removes the repeated `Array.IndexOf`; unknown run ids naturally miss it.

### Ripples

- `libs/dotnet/Rasm.Fabrication/.planning/Documentation/projection.md`: change `Census.Source.Bind(provenance => provenance.OperandOf(face))` to `Census.Native.Bind(evidence => evidence.OperandOf(face))`; update the Law and Packages prose from `ManifoldProvenance` to `BooleanCensus.ManifoldEvidence`.

# 7. Keep census accumulation local to the managed fold

### Location

- `arrangement.md:140-147`, anchors `BooleanCensus.Empty`, `operator +`, and `Last`
- `arrangement.md:231-234`, anchor the only `BooleanCensus` addition

### From

```csharp
public static readonly BooleanCensus Empty = new(0L, 0L, 0L);

public static BooleanCensus operator +(BooleanCensus left, BooleanCensus right) =>
    new(left.Classified + right.Classified, left.Kept + right.Kept, left.Welded + right.Welded,
        Last(left.ShellCount, right.ShellCount), Last(left.Native, right.Native));

static Option<T> Last<T>(Option<T> left, Option<T> right) => right.IsSome ? right : left;
```

```csharp
.Map(step => (step.Solid, Census: state.Census + step.Census))
```

### To

```csharp
public static readonly BooleanCensus Empty = new(0L, 0L, 0L);

// BooleanCensus.operator+ DELETED
// BooleanCensus.Last DELETED
```

```csharp
.Map(step => (step.Solid, Census: new BooleanCensus(
    Classified: state.Census.Classified + step.Census.Classified,
    Kept: state.Census.Kept + step.Census.Kept,
    Welded: state.Census.Welded + step.Census.Welded)))
```

### Why

Only the managed N-solid fold combines censuses, and every intermediate census has no shell or native evidence. The public operator pretends to define a general algebra while right-biasing optional terminal evidence, so `a + b` can combine counts from both with provenance from only `b`. Accumulating the three genuinely additive columns at the one fold deletes the misleading operator and its hand-written option chooser; terminal evidence remains constructed once by the route that measured it.

# 8. Delete the two-case operand vocabulary

### Location

- `arrangement.md:86-95`, anchor module-level `internal sealed partial class Operand`
- `arrangement.md:169-172`, anchor `PatchStore.Add`
- `arrangement.md:312-360`, anchors the two `Subdivided` calls and its `side` reads

### From

```csharp
[SmartEnum<int>]
internal sealed partial class Operand {
    public static readonly Operand A = new(key: 0, progress: UnitInterval.Create(value: 0.00), static cut => cut.FaceB);
    public static readonly Operand B = new(key: 1, progress: UnitInterval.Create(value: 0.25), static cut => cut.FaceA);

    public UnitInterval Progress { get; }

    [UseDelegateFromConstructor]
    public partial int Facing((int A, int B, int FaceA, int FaceB) cut);
}
```

```csharp
int Add((Point3d A, Point3d B, Point3d C) patch, Operand from) {
    Grow(count + 1);
    (patches[count], fromA[count]) = (patch, from == Operand.A);
```

```csharp
.Bind(table => Subdivided(store, ea, table, Operand.A, eb, policy, key)
    .Bind(_ => Subdivided(store, eb, table, Operand.B, ea, policy, key)))
```

```csharp
static Fin<Unit> Subdivided(PatchStore store, MeshEdit soup, CrossTable table, Operand side, MeshEdit other, ArrangementPolicy policy, Op key) {
    GeometryFault cancelled = new GeometryFault.SubdivisionCancelled(side.Key, side.Progress);
```

```csharp
static Fin<Unit> FaceBuild(PatchStore store, CrossTable table,
    (int A, int B, int FaceA, int FaceB)[] cuts,
    (int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)[] flush,
    Operand side, (Point3d A, Point3d B, Point3d C) face, int faceId,
    MeshEdit soup, MeshEdit other, ArrangementPolicy policy, Op key) {
```

```csharp
(int o0, int o1, int o2) = other.Face(side.Facing(cut));
```

```csharp
MeshEdit carrier = row.CarrierSide == side.Key ? soup : other;
```

```csharp
.MapFail(fail => new GeometryFault.ArrangementSubdivisionFailed(side.Key, faceId, fail))
```

```csharp
foreach ((int a, int b, int c) in tris.Faces) { store.Add((tris.Corners[a], tris.Corners[b], tris.Corners[c]), side); }
```

### To

```csharp
// Operand DELETED
```

```csharp
int Add((Point3d A, Point3d B, Point3d C) patch, bool fromA) {
    Grow(count + 1);
    (patches[count], this.fromA[count]) = (patch, fromA);
```

```csharp
.Bind(table => Subdivided(store, ea, table, fromA: true, eb, policy, key)
    .Bind(_ => Subdivided(store, eb, table, fromA: false, ea, policy, key)))
```

```csharp
static Fin<Unit> Subdivided(PatchStore store, MeshEdit soup, CrossTable table, bool fromA, MeshEdit other, ArrangementPolicy policy, Op key) {
    int side = fromA ? 0 : 1;
    UnitInterval progress = UnitInterval.Create(value: fromA ? 0.00 : 0.25);
    GeometryFault cancelled = new GeometryFault.SubdivisionCancelled(side, progress);
```

```csharp
static Fin<Unit> FaceBuild(PatchStore store, CrossTable table,
    (int A, int B, int FaceA, int FaceB)[] cuts,
    (int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)[] flush,
    bool fromA, (Point3d A, Point3d B, Point3d C) face, int faceId,
    MeshEdit soup, MeshEdit other, ArrangementPolicy policy, Op key) {
    int side = fromA ? 0 : 1;
```

```csharp
(int o0, int o1, int o2) = other.Face(fromA ? cut.FaceB : cut.FaceA);
```

```csharp
MeshEdit carrier = row.CarrierSide == side ? soup : other;
```

```csharp
.MapFail(fail => new GeometryFault.ArrangementSubdivisionFailed(side, faceId, fail))
```

```csharp
foreach ((int a, int b, int c) in tris.Faces) { store.Add((tris.Corners[a], tris.Corners[b], tris.Corners[c]), fromA); }
```

Within `Subdivided`, replace the remaining `side.Key` and `side.Progress` reads with the shown `side` and `progress` locals and pass `fromA` into `FaceBuild`. Remove `Operand` from the target owner, cases, growth, and density prose.

### Why

`Operand` is exactly a two-case payloadless family. Its key is `fromA ? 0 : 1`, its progress is `fromA ? 0.00 : 0.25`, and its facing projection is the same boolean selecting `FaceB` or `FaceA`; no independent information survives the flag deletion test. Carrying the named `fromA` fact and translating it to the crossing-table ordinal only at that boundary removes one type, two rows, a generated smart-enum surface, a delegate column, and two guarded value-object instances without scattering a second authority.

# 9. Derive keep and orientation at their only consumer

### Location

- `arrangement.md:56-64`, anchors `Region`, `Keep`, and `Flip`
- `arrangement.md:164-167`, anchors `PatchStore.Patch`, `FromA`, and `InsideOther`
- `arrangement.md:410-416`, anchor the `KeepAndWeld` patch loop

### From

```csharp
[UseDelegateFromConstructor]
public partial bool Region(bool inA, bool inB);

public bool Keep(bool fromA, bool insideOther) =>
    fromA ? Region(true, insideOther) != Region(false, insideOther)
          : Region(insideOther, true) != Region(insideOther, false);

public bool Flip(bool fromA, bool insideOther) =>
    fromA ? Region(false, insideOther) : Region(insideOther, false);
```

```csharp
(Point3d A, Point3d B, Point3d C) Patch(int row) => patches[row];
bool FromA(int row) => fromA[row];
bool InsideOther(int row) => fromA[row] ? insideB[row] : insideA[row];
```

```csharp
if (!op.Keep(store.FromA(p), store.InsideOther(p))) { continue; }
(Point3d a, Point3d b, Point3d c) = store.Patch(p);
int at = vertices.Count;
vertices.AddRange([a, b, c]);
faces.Add(op.Flip(store.FromA(p), store.InsideOther(p)) ? (at, at + 2, at + 1) : (at, at + 1, at + 2));
```

### To

```csharp
[UseDelegateFromConstructor]
internal partial bool Region(bool inA, bool inB);

// BooleanOp.Keep DELETED
// BooleanOp.Flip DELETED
```

```csharp
// PatchStore.Patch DELETED
// PatchStore.FromA DELETED
// PatchStore.InsideOther DELETED
```

```csharp
bool fromA = store.fromA[p];
bool insideOther = fromA ? store.insideB[p] : store.insideA[p];
bool outside = fromA ? op.Region(false, insideOther) : op.Region(insideOther, false);
bool inside = fromA ? op.Region(true, insideOther) : op.Region(insideOther, true);
if (inside == outside) { continue; }
(Point3d a, Point3d b, Point3d c) = store.patches[p];
int at = vertices.Count;
vertices.AddRange([a, b, c]);
faces.Add(outside ? (at, at + 2, at + 1) : (at, at + 1, at + 2));
```

Replace the target prose's separate `Keep`/`Flip` references with the one region transition derived at `KeepAndWeld`.

### Why

Keep and orientation are two projections of the same outside/inside region pair, and `KeepAndWeld` is their only consumer. Deriving the pair in place deletes both public helpers instead of replacing them with another one-call member, removes three one-call arena projections, and evaluates `Region` twice rather than three times for every kept patch without changing any boolean truth table.

# 10. Nest the native gate under `Arrangement`

### Location

- `arrangement.md:218`, anchor `public static class Arrangement`
- `arrangement.md:489-667`, anchor module-level `file static partial class ManifoldGate`

### From

```csharp
public static class Arrangement {
```

```csharp
file static partial class ManifoldGate {
```

### To

```csharp
public static partial class Arrangement {
```

```csharp
    private static partial class ManifoldGate {
```

Move the exact existing `ManifoldGate` body under this declaration and close it before `Arrangement`'s closing brace; no P/Invoke signature or native fold changes in this move.

Remove `ManifoldGate` as a module-level density owner; describe it as `Arrangement`'s private native lane.

### Why

Every `manifoldc` call belongs to the arrangement scale lane and no sibling fence calls `ManifoldGate`. Nesting deletes a module-level type while retaining the explicit native custody boundary; marking the containing type partial is required for the nested `LibraryImport` source generation.

# 11. Fold the immutable cell payload into its result case

### Location

- `arrangement.md:150`, anchor module-level `public sealed record CellSet`
- `arrangement.md:187`, anchor `PatchStore.Freeze`
- `arrangement.md:215`, anchor `ArrangementResult.Complex`
- `arrangement.md:246-251`, anchor the sole `store.Freeze()` call

### From

```csharp
public sealed record CellSet((Point3d A, Point3d B, Point3d C)[] Patches, bool[] FromA, bool[] InsideA, bool[] InsideB);
```

```csharp
CellSet Freeze() => new([.. patches.AsSpan(0, count)], [.. fromA.AsSpan(0, count)], [.. insideA.AsSpan(0, count)], [.. insideB.AsSpan(0, count)]);
```

```csharp
public sealed record Complex(CellSet Cells, BooleanCensus Census) : ArrangementResult;
```

```csharp
return Arrange(operands.Head, operands[1], ea, eb, policy, key).Map(store =>
    (ArrangementResult)new ArrangementResult.Complex(
        store.Freeze(), BooleanCensus.Empty with { Classified = store.Count, Kept = store.Count }));
```

### To

```csharp
// CellSet DELETED
```

```csharp
// PatchStore.Freeze DELETED
```

```csharp
public sealed record Complex(
    Arr<(Point3d A, Point3d B, Point3d C)> Patches,
    Arr<bool> FromA, Arr<bool> InsideA, Arr<bool> InsideB,
    BooleanCensus Census) : ArrangementResult;
```

```csharp
return Arrange(operands.Head, operands[1], ea, eb, policy, key).Map(store =>
    (ArrangementResult)new ArrangementResult.Complex(
        new([.. store.patches.AsSpan(0, store.count)]), new([.. store.fromA.AsSpan(0, store.count)]),
        new([.. store.insideA.AsSpan(0, store.count)]), new([.. store.insideB.AsSpan(0, store.count)]),
        new BooleanCensus(store.Count, store.Count, 0L)));
```

### Why

The current projection copies the arena but publishes four mutable arrays, so a consumer can mutate the supposedly frozen arrangement after admission. `Arr<T>` is the branch's indexed immutable carrier. `ArrangementResult.Complex` is already the closed cell-complex payload owner, and no consumer names `CellSet`; nesting it would preserve an unneeded second carrier. Folding the four immutable columns directly into the case removes the type entirely and deletes the one-call `Freeze` member without losing the classified-cell capability.

# 12. Grow the patch arena at its only mutation door

### Location

- `arrangement.md:169-173`, anchor `PatchStore.Add`
- `arrangement.md:189-196`, anchor the one-call `PatchStore.Grow`

### From

```csharp
int Add((Point3d A, Point3d B, Point3d C) patch, bool fromA) {
    Grow(count + 1);
    (patches[count], this.fromA[count]) = (patch, fromA);
    return count++;
}
```

```csharp
void Grow(int needed) {
    if (needed <= patches.Length) { return; }
    int extent = int.Max(needed, patches.Length << 1);
    Array.Resize(ref patches, extent);
    Array.Resize(ref fromA, extent);
    Array.Resize(ref insideA, extent);
    Array.Resize(ref insideB, extent);
}
```

### To

```csharp
int Add((Point3d A, Point3d B, Point3d C) patch, bool fromA) {
    if (count == patches.Length) {
        int extent = int.Max(count + 1, patches.Length << 1);
        Array.Resize(ref patches, extent);
        Array.Resize(ref fromA, extent);
        Array.Resize(ref insideA, extent);
        Array.Resize(ref insideB, extent);
    }
    (patches[count], this.fromA[count]) = (patch, fromA);
    return count++;
}

// PatchStore.Grow DELETED
```

### Why

`Grow` has exactly one caller and expresses no policy beyond ensuring capacity for the row `Add` is about to write. Keeping the capacity transition at that mutation door removes a private member and one hop while retaining geometric doubling and all four structure-of-arrays columns in the same resize branch.

# 13. Write the one-call classification projection at its fold

### Location

- `arrangement.md:175`, anchor `PatchStore.Classify`
- `arrangement.md:379-381`, anchor the only `store.Classify` call

### From

```csharp
void Classify(int row, bool inA, bool inB) => (insideA[row], insideB[row]) = (inA, inB);
```

```csharp
for (int p = 0; p < store.Count; p++) {
    store.Classify(p, t.wa[p] > policy.WindingThreshold.Value, t.wb[p] > policy.WindingThreshold.Value);
}
```

### To

```csharp
// PatchStore.Classify DELETED
```

```csharp
for (int p = 0; p < store.Count; p++) {
    store.insideA[p] = t.wa[p] > policy.WindingThreshold.Value;
    store.insideB[p] = t.wb[p] > policy.WindingThreshold.Value;
}
```

### Why

`Classify` has one caller and only forwards two booleans into two private arena columns. After `PatchStore` is nested, the classification fold can write those columns directly, removing another private member without exposing mutable state outside `Arrangement`.

# 14. Build patch probes at their only consumer

### Location

- `arrangement.md:177-182`, anchor `PatchStore.Interior`
- `arrangement.md:374-376`, anchor the sole `store.Interior` call

### From

```csharp
Point3d Interior(int row, double offset) {
    (Point3d a, Point3d b, Point3d c) = patches[row];
    Point3d centroid = Centroid(a, b, c);
    Vector3d n = Vector3d.CrossProduct(b - a, c - a);
    return n.IsTiny() ? centroid : centroid + (offset * (n / n.Length));
}
```

```csharp
for (int p = 0; p < store.Count; p++) { probes[p] = store.Interior(p, nudge); }
```

### To

```csharp
// PatchStore.Interior DELETED
```

```csharp
for (int p = 0; p < store.Count; p++) {
    (Point3d a, Point3d b, Point3d c) = store.patches[p];
    Point3d centroid = PatchStore.Centroid(a, b, c);
    Vector3d normal = Vector3d.CrossProduct(b - a, c - a);
    probes[p] = normal.IsTiny() ? centroid : centroid + (nudge * (normal / normal.Length));
}
```

### Why

`Interior` is a one-call projection whose offset is supplied entirely by `Classify`. Building the probe in that batch loop removes another private arena member and leaves `Centroid` as the shared primitive used here and by planar overlay, without introducing a replacement helper.

# 15. Use the generated state-threaded dispatch at the entry

### Location

- `arrangement.md:218-225`, anchor `Arrangement.Apply`

### From

```csharp
public static Fin<ArrangementResult> Apply(ArrangementOp op, Op? key = null) {
    Op site = key.OrDefault();
    return op.Switch(
        meshBoolean:   m => Volumetric(m.Operands, Some(m.Op), m.Policy, site),
        planarOverlay: p => Overlay(p, site),
        cellComplex:   c => Volumetric(Seq(c.A, c.B), None, c.Policy, site));
}
```

### To

```csharp
public static Fin<ArrangementResult> Apply(ArrangementOp op, Op? key = null) =>
    op.Switch(
        state: key.OrDefault(),
        meshBoolean:   static (site, m) => Volumetric(m.Operands, Some(m.Op), m.Policy, site),
        planarOverlay: static (site, p) => Overlay(p, site),
        cellComplex:   static (site, c) => Volumetric(Seq(c.A, c.B), None, c.Policy, site));
```

### Why

Thinktecture generates the state-bearing overload precisely to carry shared dispatch context. It removes the method body/local and three closures while keeping exhaustive case pressure.

# 16. Admit the leading pair once and seat mesh leases at `Arrange`

### Location

- `arrangement.md:227-238`, anchors the three `operands.Head` reads in `Volumetric`
- `arrangement.md:240-251`, anchors the duplicate mesh leases in `Pairwise` and `Complex`
- `arrangement.md:254-260`, anchor `Gate`
- `arrangement.md:309-317`, anchor `Arrange`

### From

```csharp
(true, BooleanOp op) => ManifoldGate.Boolean(operands, op, operands.Head.Tolerance, policy, key),
```

```csharp
.Fold(Fin.Succ((Solid: operands.Head, Census: BooleanCensus.Empty)),
```

```csharp
static Fin<(MeshSpace Solid, BooleanCensus Census)> Pairwise(MeshSpace a, MeshSpace b, BooleanOp op, ArrangementPolicy policy, Op key) {
    using MeshEdit ea = MeshEdit.Of(a);
    using MeshEdit eb = MeshEdit.Of(b);
    return Arrange(a, b, ea, eb, policy, key).Bind(store => KeepAndWeld(store, op, a.Tolerance, policy, key));
}
```

```csharp
static Fin<ArrangementResult> Complex(Seq<MeshSpace> operands, ArrangementPolicy policy, Op key) {
    using MeshEdit ea = MeshEdit.Of(operands.Head);
    using MeshEdit eb = MeshEdit.Of(operands[1]);
    return Arrange(operands.Head, operands[1], ea, eb, policy, key).Map(store =>
        (ArrangementResult)new ArrangementResult.Complex(
            new([.. store.patches.AsSpan(0, store.count)]), new([.. store.fromA.AsSpan(0, store.count)]),
            new([.. store.insideA.AsSpan(0, store.count)]), new([.. store.insideB.AsSpan(0, store.count)]),
            new BooleanCensus(store.Count, store.Count, 0L)));
}
```

```csharp
static Fin<(bool Native, long Faces)> Gate(Seq<MeshSpace> operands, ArrangementPolicy policy) {
    long faces = operands.Sum(static space => (long)space.Native.Faces.Count);
    return operands.Count < 2 || operands.Exists(static space => space.Native.Vertices.Count == 0)
        ? Fin.Fail<(bool, long)>(new GeometryFault.DegenerateInput(Kind.Mesh, operands.Count, "fewer than two operands or an empty operand"))
        : !policy.BeyondManaged(faces) ? Fin.Succ((Native: false, Faces: faces))
        : ManifoldGate.AssetResolves() ? Fin.Succ((Native: true, Faces: faces))
        : Fin.Fail<(bool, long)>(new GeometryFault.ManifoldLibraryUnavailable(RuntimeInformation.RuntimeIdentifier, faces, policy.ScaleCeiling));
}
```

```csharp
static Fin<PatchStore> Arrange(MeshSpace a, MeshSpace b, MeshEdit ea, MeshEdit eb, ArrangementPolicy policy, Op key) {
    PatchStore store = new(int.Max(ea.FaceCount + eb.FaceCount, 16));
```

### To

```csharp
(true, BooleanOp op) => ManifoldGate.Boolean(operands, op, gate.First.Tolerance, policy, key),
```

```csharp
.Fold(Fin.Succ((Solid: gate.First, Census: BooleanCensus.Empty)),
    (acc, next) => acc.Bind(state => Arrange(state.Solid, next, policy, key)
        .Bind(store => KeepAndWeld(store, op, state.Solid.Tolerance, policy, key))
        .Map(step => (step.Solid, Census: new BooleanCensus(
            Classified: state.Census.Classified + step.Census.Classified,
            Kept: state.Census.Kept + step.Census.Kept,
            Welded: state.Census.Welded + step.Census.Welded)))))
```

```csharp
// Arrangement.Pairwise DELETED
```

```csharp
(false, _) => Arrange(gate.First, gate.Second, policy, key).Map(store =>
    (ArrangementResult)new ArrangementResult.Complex(
        new([.. store.patches.AsSpan(0, store.count)]), new([.. store.fromA.AsSpan(0, store.count)]),
        new([.. store.insideA.AsSpan(0, store.count)]), new([.. store.insideB.AsSpan(0, store.count)]),
        new BooleanCensus(store.Count, store.Count, 0L))),

// Arrangement.Complex DELETED
```

```csharp
static Fin<(bool Native, long Faces, MeshSpace First, MeshSpace Second)> Gate(
    Seq<MeshSpace> operands, ArrangementPolicy policy) {
    if (operands.Count < 2 || operands.Exists(static space => space.Native.Vertices.Count == 0)) {
        return Fin.Fail<(bool, long, MeshSpace, MeshSpace)>(
            new GeometryFault.DegenerateInput(Kind.Mesh, operands.Count, "fewer than two operands or an empty operand"));
    }
    MeshSpace first = operands[0], second = operands[1];
    long faces = operands.Sum(static space => (long)space.Native.Faces.Count);
    return !policy.BeyondManaged(faces)
        ? Fin.Succ((Native: false, Faces: faces, First: first, Second: second))
        : ManifoldGate.AssetResolves()
            ? Fin.Succ((Native: true, Faces: faces, First: first, Second: second))
            : Fin.Fail<(bool, long, MeshSpace, MeshSpace)>(new GeometryFault.ManifoldLibraryUnavailable(
                RuntimeInformation.RuntimeIdentifier, faces, policy.ScaleCeiling));
}
```

```csharp
static Fin<PatchStore> Arrange(MeshSpace a, MeshSpace b, ArrangementPolicy policy, Op key) {
    using MeshEdit ea = MeshEdit.Of(a);
    using MeshEdit eb = MeshEdit.Of(b);
    PatchStore store = new(int.Max(ea.FaceCount + eb.FaceCount, 16));
```

### Why

`Seq.Head` is an `Option<MeshSpace>` property, not a bare mesh; the current member reads do not type-check against the catalogued LanguageExt surface. The gate is the one admission door, so it can validate the count and return the leading pair once. Both managed callers open identical `MeshEdit` leases solely for `Arrange`; moving that custody to the consuming operation removes the duplication and lets both one-call wrappers disappear immediately. The cell arm inlines its result projection, while the boolean fold composes `Arrange` with `KeepAndWeld` at the only former `Pairwise` call site, preserving lease lifetime without retaining an intermediate forwarding symbol.

# 17. Use `FoldM` for the dependent pairwise boolean

### Location

- `arrangement.md:231-236`, anchor the managed `operands.Tail.Fold` branch
- `arrangement.md:140`, anchor the now-single-use `BooleanCensus.Empty`

### From

```csharp
operands.Tail
    .Fold(Fin.Succ((Solid: gate.First, Census: BooleanCensus.Empty)),
        (acc, next) => acc.Bind(state => Arrange(state.Solid, next, policy, key)
            .Bind(store => KeepAndWeld(store, op, state.Solid.Tolerance, policy, key))
            .Map(step => (step.Solid, Census: new BooleanCensus(
                Classified: state.Census.Classified + step.Census.Classified,
                Kept: state.Census.Kept + step.Census.Kept,
                Welded: state.Census.Welded + step.Census.Welded)))))
    .Bind(final => Severed(final.Solid, policy, key).Map(shells =>
```

### To

```csharp
operands.Tail
    .FoldM<Fin, (MeshSpace Solid, BooleanCensus Census)>(
        (gate.First, new BooleanCensus(0L, 0L, 0L)),
        (state, next) => Arrange(state.Solid, next, policy, key)
            .Bind(store => KeepAndWeld(store, op, state.Solid.Tolerance, policy, key))
            .Map(step => (step.Solid, Census: new BooleanCensus(
                Classified: state.Census.Classified + step.Census.Classified,
                Kept: state.Census.Kept + step.Census.Kept,
                Welded: state.Census.Welded + step.Census.Welded))))
    .As()
    .Bind(final => Severed(final.Solid, policy, key).Map(shells =>
```

```csharp
// BooleanCensus.Empty DELETED
```

### Why

This is a dependent monadic state fold. LanguageExt already owns that algebra; seeding a pure `Fold` with `Fin.Succ` and binding the accumulator by hand duplicates `FoldM`, adds a nested carrier, and obscures short-circuiting. The explicit carrier/state arguments match the catalogued `FoldM<M,S>` surface, and inlining the one remaining use of `Empty` deletes another public member rather than retaining a named zero no general census algebra consumes.

# 18. Inline the one-call scale predicate

### Location

- `arrangement.md:110`, anchor `ArrangementPolicy.BeyondManaged`
- `arrangement.md:258`, anchor `!policy.BeyondManaged(faces)`

### From

```csharp
public bool BeyondManaged(long operandFaces) => operandFaces > ScaleCeiling.Value;
```

```csharp
return !policy.BeyondManaged(faces)
    ? Fin.Succ((Native: false, Faces: faces, First: first, Second: second))
```

### To

```csharp
// ArrangementPolicy.BeyondManaged DELETED
```

```csharp
return faces <= policy.ScaleCeiling.Value
    ? Fin.Succ((Native: false, Faces: faces, First: first, Second: second))
```

### Why

The helper has one caller, adds no domain operation beyond a comparison, and forces a double negation at that caller. The gate is the scale-decision owner; reading the guarded ceiling there is one-hop logic and removes a public policy member.

# 19. Keep cancellation checkpoints boolean until the fault boundary

### Location

- `arrangement.md:263-270`, anchors `Opened` and `Cancelled`
- `arrangement.md:321-324`, anchors subdivision cancellation reads
- `arrangement.md:371` and `arrangement.md:406`, anchors classification and weld checkpoint reads

### From

```csharp
static Option<Error> Opened(GeometryFault fault, UnitInterval progress, ArrangementPolicy policy) {
    policy.Progress.Iter(sink => sink.Report(progress.Value));
    return Cancelled(fault, policy);
}

static Option<Error> Cancelled(GeometryFault fault, ArrangementPolicy policy) =>
    policy.Cancel.IsCancellationRequested ? Some<Error>(fault) : None;
```

```csharp
if (Cancelled(cancelled, policy).Case is Error beat) { return Fin.Fail<Unit>(beat); }
```

```csharp
if (Opened(cancelled, progress, policy).Case is Error head) { return Fin.Fail<Unit>(head); }
```

```csharp
if (Opened(new GeometryFault.ClassificationCancelled(progress), progress, policy).Case is Error head) { return Fin.Fail<PatchStore>(head); }
```

```csharp
if (Opened(new GeometryFault.WeldCancelled(progress), progress, policy).Case is Error head) { return Fin.Fail<(MeshSpace, BooleanCensus)>(head); }
```

### To

```csharp
static bool Opened(UnitInterval progress, ArrangementPolicy policy) {
    policy.Progress.Iter(sink => sink.Report(progress.Value));
    return policy.Cancel.IsCancellationRequested;
}

// Arrangement.Cancelled DELETED
```

```csharp
if (policy.Cancel.IsCancellationRequested) { return Fin.Fail<Unit>(cancelled); }
```

```csharp
if (Opened(progress, policy)) { return Fin.Fail<Unit>(cancelled); }
```

```csharp
if (Opened(progress, policy)) { return Fin.Fail<PatchStore>(new GeometryFault.ClassificationCancelled(progress)); }
```

```csharp
if (Opened(progress, policy)) { return Fin.Fail<(MeshSpace, BooleanCensus)>(new GeometryFault.WeldCancelled(progress)); }
```

### Why

`Cancelled` only renames `CancellationToken.IsCancellationRequested`, and `Opened` turns the same boolean fact into `Option<Error>` only for each caller to turn it immediately back into `Fin`. Letting the shared checkpoint report cancellation as `bool` keeps progress publication centralized while each typed stage constructs its own fault exactly at its carrier boundary. This removes one private member and all checkpoint `Some`/`None` construction and `Case` matching.

# 20. Build the shell graph from used vertices and edges

### Location

- `arrangement.md:273-281`, anchor the opening of `Severed`

### From

```csharp
UndirectedGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
graph.AddVertexRange(Enumerable.Range(0, welded.VertexCount));
for (int f = 0; f < welded.FaceCount; f++) {
    (int a, int b, int c) = welded.Face(f);
    foreach ((int u, int v) in (ReadOnlySpan<(int, int)>)[(a, b), (b, c), (c, a)]) {
        if (u != v) { graph.AddEdge(new SEdge<int>(int.Min(u, v), int.Max(u, v))); }
    }
}
```

### To

```csharp
UndirectedGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
for (int f = 0; f < welded.FaceCount; f++) {
    (int a, int b, int c) = welded.Face(f);
    foreach ((int u, int v) in (ReadOnlySpan<(int, int)>)[(a, b), (b, c), (c, a)]) {
        if (u != v) { graph.AddVerticesAndEdge(new SEdge<int>(int.Min(u, v), int.Max(u, v))); }
    }
}
```

### Why

QuikGraph's `AddVerticesAndEdge` owns endpoint admission. Preloading every mesh vertex creates components for unreferenced vertices, then empty face buckets that the shell lowerer attempts to emit as meshes. Admitting only endpoints used by faces removes one full vertex pass and makes every labelled component correspond to at least one face.

# 21. Delete the duplicate shell-count axis

### Location

- `arrangement.md:137-145`, anchors `BooleanCensus.ShellCount` and its merge projection
- `arrangement.md:235-236`, anchor the managed result construction
- `arrangement.md:564-569`, anchors the native `Shells` projection
- `arrangement.md:618-624`, anchor `static Fin<(Seq<MeshSpace> Solids, int ShellCount)> Shells`

### From

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded, Option<int> ShellCount = default,
    Option<ManifoldEvidence> Native = default) {
```

```csharp
.Bind(final => Severed(final.Solid, policy, key).Map(shells =>
    (ArrangementResult)new ArrangementResult.Boolean(shells, final.Census with { ShellCount = Some(shells.Count) }))),
```

```csharp
0 => Shells(observed, context, policy, key)
        .Map(shells => (Shells: shells, Evidence: Evidence(observed, seated)))
        .Map(read => (ArrangementResult)new ArrangementResult.Boolean(read.Shells.Solids, new BooleanCensus(
            Classified: classified, Kept: read.Evidence.Triangles, Welded: 0,
            ShellCount: Some(read.Shells.ShellCount), Native: Some(read.Evidence)))),
```

```csharp
static Fin<(Seq<MeshSpace> Solids, int ShellCount)> Shells(nint result, Context context, ArrangementPolicy policy, Op key) {
    nint vec = manifold_decompose(manifold_alloc_manifold_vec(), result);
    try {
        int count = (int)manifold_manifold_vec_length(vec);
        return toSeq(Enumerable.Range(0, count)).TraverseM(at => Lower(vec, at, context, policy, key)).As()
            .Map(solids => (Solids: solids.Strict(), ShellCount: count));
    }
```

### To

```csharp
public sealed record BooleanCensus(
    long Classified, long Kept, long Welded,
    Option<ManifoldEvidence> Native = default) {
    // BooleanCensus.ShellCount DELETED
```

```csharp
.Bind(final => Severed(final.Solid, policy, key).Map(shells =>
    (ArrangementResult)new ArrangementResult.Boolean(shells, final.Census))),
```

```csharp
0 => Shells(observed, context, policy, key)
        .Map(shells => {
            BooleanCensus.ManifoldEvidence evidence = Evidence(observed, seated);
            return (ArrangementResult)new ArrangementResult.Boolean(shells, new BooleanCensus(
                Classified: classified, Kept: evidence.Triangles, Welded: 0, Native: Some(evidence)));
        }),
```

```csharp
static Fin<Seq<MeshSpace>> Shells(nint result, Context context, ArrangementPolicy policy, Op key) {
    nint vec = manifold_decompose(manifold_alloc_manifold_vec(), result);
    try {
        int count = (int)manifold_manifold_vec_length(vec);
        return toSeq(Enumerable.Range(0, count)).TraverseM(at => Lower(vec, at, context, policy, key)).As()
            .Map(static solids => solids.Strict());
    }
```

Remove only the `ShellCount` positional member from the final `BooleanCensus` shape; retain its nested native evidence declarations. Remove `ShellCount` from the target owner, output, growth, and density prose, and make `ArrangementResult.Boolean.Shells` the sole severance census.

### Why

`ShellCount` is exactly `ArrangementResult.Boolean.Shells.Count` on both routes, and no consumer reads the duplicate option. Intermediate pairwise legs are not published results, so their lack of a shell count does not require a public absence state. Returning the one real shell product and making it the sole count authority removes the helper tuple, the transient native mapping tuple, one public census member, its merge projection, two option constructions, and the possibility of count/carrier disagreement.

# 22. Fold the native-library probe into the scale gate

### Location

- `arrangement.md:254-260`, anchor the native availability arm in `Gate`
- `arrangement.md:535-537`, anchors `ManifoldGate.AssetResolves` and `Free`

### From

```csharp
return faces <= policy.ScaleCeiling.Value
    ? Fin.Succ((Native: false, Faces: faces, First: first, Second: second))
    : ManifoldGate.AssetResolves()
        ? Fin.Succ((Native: true, Faces: faces, First: first, Second: second))
        : Fin.Fail<(bool, long, MeshSpace, MeshSpace)>(new GeometryFault.ManifoldLibraryUnavailable(
            RuntimeInformation.RuntimeIdentifier, faces, policy.ScaleCeiling));
```

```csharp
internal static bool AssetResolves() => NativeLibrary.TryLoad("manifoldc", out nint handle) && Free(handle);

static bool Free(nint handle) { NativeLibrary.Free(handle); return true; }
```

### To

```csharp
if (faces <= policy.ScaleCeiling.Value) {
    return Fin.Succ((Native: false, Faces: faces, First: first, Second: second));
}
if (NativeLibrary.TryLoad("manifoldc", out nint handle)) {
    NativeLibrary.Free(handle);
    return Fin.Succ((Native: true, Faces: faces, First: first, Second: second));
}
return Fin.Fail<(bool, long, MeshSpace, MeshSpace)>(new GeometryFault.ManifoldLibraryUnavailable(
    RuntimeInformation.RuntimeIdentifier, faces, policy.ScaleCeiling));
```

```csharp
// ManifoldGate.AssetResolves DELETED
// ManifoldGate.Free DELETED
```

### Why

`AssetResolves` has one caller and `Free` exists only to make release fit the right operand of `&&`. The gate already owns the over-ceiling route decision, so keeping load/release custody there deletes both private members and makes the acquired handle's release explicit without introducing another availability abstraction.

# 23. Delete the unused governance copy wrapper

### Location

- `arrangement.md:5` and `arrangement.md:14`, anchors the lead and Owner references to `Governed`
- `arrangement.md:112-113`, anchor `ArrangementPolicy.Governed`

### From

```csharp
public ArrangementPolicy Governed(Option<IProgress<double>> progress, CancellationToken cancel) =>
    this with { Progress = progress, Cancel = cancel };
```

### To

```csharp
// ArrangementPolicy.Governed DELETED
```

### Why

No fence calls `Governed`; its only references are prose. Delete those target lead and Owner references while retaining the rule that both routes read the same governance band. The method is a convenience copy wrapper over two public record columns and owns no admission, normalization, or policy. Removing it deletes a public member without losing governance capability: a composition site can set `Progress` and `Cancel` in the same `with` expression it already owns.

### Ripples

- `libs/dotnet/Rasm.AppHost/.planning/Runtime/laneguard.md`: replace the prose-only `ArrangementPolicy.Governed(progress, token)` example with `policy with { Progress = progress, Cancel = token }`.
