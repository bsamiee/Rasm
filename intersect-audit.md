# 1. Remove policy payloads from operations that never read them

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:196-199`, anchored at the four `IntersectOp` cases before `MeshMesh`.

From:

```csharp
public sealed record SegmentSegment(Line A, Line B, Axis Plane, IntersectPolicy Policy) : IntersectOp;
public sealed record SegmentTriangle(Line Edge, Point3d Ta, Point3d Tb, Point3d Tc, IntersectPolicy Policy) : IntersectOp;
public sealed record TriangleTriangle(Point3d Pa, Point3d Pb, Point3d Pc, Point3d Qa, Point3d Qb, Point3d Qc, IntersectPolicy Policy) : IntersectOp;
public sealed record RayMesh(Ray3d Ray, double MaxT, MeshSpace Mesh, IntersectPolicy Policy) : IntersectOp;
```

To:

```csharp
// SegmentSegment.Policy DELETED
// SegmentTriangle.Policy DELETED
// TriangleTriangle.Policy DELETED
// RayMesh.Policy DELETED

public sealed record SegmentSegment(Line A, Line B, Axis Plane) : IntersectOp;
public sealed record SegmentTriangle(Line Edge, Point3d Ta, Point3d Tb, Point3d Tc) : IntersectOp;
public sealed record TriangleTriangle(Point3d Pa, Point3d Pb, Point3d Pc, Point3d Qa, Point3d Qb, Point3d Qc) : IntersectOp;
public sealed record RayMesh(Ray3d Ray, double MaxT, MeshSpace Mesh) : IntersectOp;
```

Why: none of these four dispatch arms reads `Policy`; only `MeshMesh`, `SelfMesh`, and `PlaneMesh` consume seed capacity or coplanar posture. Removing the dead column shrinks four generated union cases and prevents callers from believing a supplied policy changes their result.

Ripples: remove the final policy argument at `Drawing/hatch.md:419`, `Drawing/view.md:562`, `Drawing/view.md:609`, and `Meshing/offset.md:331`. That leaves `HatchPolicy.Narrow` with no reader: delete its constructor column and canonical argument at `Drawing/hatch.md:238-245`, delete the now-readerless `IValidityEvidence` conformance and `IsValid` member there, delete the `Narrow` assignment from `Rasm.Fabrication/.planning/Documentation/projection.md:380-385`, and align those two pages' owner/package/density prose. Remove the now-false `IntersectPolicy` association with `IntersectOp.SegmentSegment` from `Parametric/curve.md:21`. Exact search finds no constructed `TriangleTriangle` or `RayMesh` consumer outside the target.

# 2. Make crossing capacity valid by construction

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:74-78` and `:157`, anchored at `public sealed record IntersectPolicy` and the `CrossingStore` constructor.

From:

```csharp
public sealed record IntersectPolicy(int SeedCapacity, bool KeepCoplanar) : IValidityEvidence {
    public static readonly IntersectPolicy Canonical = new(SeedCapacity: 256, KeepCoplanar: true);

    public bool IsValid => ValidityClaim.Positive(value: SeedCapacity);
}

public CrossingStore(int seed) { rows = new Crossing[seed]; }
```

To:

```csharp
public sealed record IntersectPolicy(Dimension SeedCapacity, bool KeepCoplanar) {
    public static readonly IntersectPolicy Canonical = new(Dimension.Create(value: 256), KeepCoplanar: true);

    // IntersectPolicy.IsValid DELETED
}

public CrossingStore(Dimension seed) { rows = new Crossing[seed.Value]; }
```

Why: `Dimension` is the existing Thinktecture value object for positive counts. It removes the evidence member and makes a negative or zero array seed unrepresentable instead of publishing an `IsValid` property that `Intersection.Apply` never checks. The private store retains that admitted value through its constructor and unwraps it only at the BCL array-allocation edge, so the three callers keep passing `op.Policy.SeedCapacity` without repeating `.Value`.

Ripples: delete the now-impossible `!op.Policy.Intersect.IsValid` refusal at `Meshing/slice.md:306`. At `Processing/repair.md:105-114`, all `RepairPolicy` columns are then admitted owners, so delete its `IValidityEvidence` conformance and readerless `IsValid` member; reduce `HealPlan.IsValid` at line 129 to the operation-count claim alone. The `Drawing/hatch.md` validity read disappears with the readerless policy column in move 1.

# 3. Route chain diagnostics by the primitive pair they actually report

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:228-230` and `:595-597`, anchored at the three chain-producing dispatch arms and `Intersection.Chains`.

From:

```csharp
meshMesh:         m => Cross(m, site).Bind(store => Chains(store.Freeze(), m.Kind)),
selfMesh:         sm => SelfCross(sm, site).Bind(store => Chains(store.Freeze(), sm.Kind)),
planeMesh:        p => Section(p, site).Bind(store => Chains(store.Freeze(), p.Kind))));

static Fin<IntersectResult> Chains(CrossTable table, IntersectKind kind) =>
    ChainWalk.Of(table.Segments.Select(static s => (s.A, s.B)), slot => table.Rows[slot].Point.Round(), kind.A, kind.B)
```

To:

```csharp
meshMesh:         m => Cross(m, site).Bind(store => Chains(store.Freeze(), PrimitiveKind.Mesh, PrimitiveKind.Mesh)),
selfMesh:         sm => SelfCross(sm, site).Bind(store => Chains(store.Freeze(), PrimitiveKind.Mesh, PrimitiveKind.Mesh)),
planeMesh:        p => Section(p, site).Bind(store => Chains(store.Freeze(), PrimitiveKind.Plane, PrimitiveKind.Mesh))));

static Fin<IntersectResult> Chains(CrossTable table, PrimitiveKind a, PrimitiveKind b) =>
    ChainWalk.Of(table.Segments.Select(static s => (s.A, s.B)), slot => table.Rows[slot].Point.Round(), a, b)
```

Why: the three fault leaves and their messages report only the primitive pair, and `Meshing/arrangement` already uses the same vocabulary for its triangle rim. Passing that pair outright removes a second operation roster from the chain path without weakening the evidence any consumer reads; distinguishing `MeshMesh` from `SelfMesh` here was purely semantic because both lower to the same `mesh`/`mesh` fault payload.

# 4. Delete the duplicate operation-kind roster and projection

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:57-71` and `:204-212`, anchored at `[SmartEnum<string>] public sealed partial class IntersectKind` and `IntersectOp.Kind`.

From:

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IntersectKind {
    public static readonly IntersectKind SegmentSegment   = new("segment-segment", PrimitiveKind.Segment, PrimitiveKind.Segment);
    public static readonly IntersectKind SegmentTriangle  = new("segment-triangle", PrimitiveKind.Segment, PrimitiveKind.Triangle);
    public static readonly IntersectKind TriangleTriangle = new("triangle-triangle", PrimitiveKind.Triangle, PrimitiveKind.Triangle);
    public static readonly IntersectKind RayMesh          = new("ray-mesh", PrimitiveKind.Ray, PrimitiveKind.Mesh);
    public static readonly IntersectKind MeshMesh         = new("mesh-mesh", PrimitiveKind.Mesh, PrimitiveKind.Mesh);
    public static readonly IntersectKind SelfMesh         = new("self-mesh", PrimitiveKind.Mesh, PrimitiveKind.Mesh);
    public static readonly IntersectKind PlaneMesh        = new("plane-mesh", PrimitiveKind.Plane, PrimitiveKind.Mesh);

    public PrimitiveKind A { get; }
    public PrimitiveKind B { get; }
}
```

```csharp
public IntersectKind Kind =>
    Switch(
        segmentSegment:   static _ => IntersectKind.SegmentSegment,
        segmentTriangle:  static _ => IntersectKind.SegmentTriangle,
        triangleTriangle: static _ => IntersectKind.TriangleTriangle,
        rayMesh:          static _ => IntersectKind.RayMesh,
        meshMesh:         static _ => IntersectKind.MeshMesh,
        selfMesh:         static _ => IntersectKind.SelfMesh,
        planeMesh:        static _ => IntersectKind.PlaneMesh);
```

To:

```csharp
// IntersectKind DELETED
```

```csharp
// IntersectOp.Kind DELETED
```

Why: `IntersectOp` already closes the seven operation cases, while `IntersectKind` mirrors those cases only to recover the five-row `PrimitiveKind` payload consumed by chain faults. After move 3 passes that payload directly, deleting the mirror removes one module-level type, seven keyed rows, two generated comparer surfaces, two columns, one property, and a seven-arm forwarding dispatch.

# 5. Derive chain closure from the polyline

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:89` and `:113-117`, anchored at the `Chain` declaration and the closed-run append in `ChainWalk.Of`.

From:

```csharp
public sealed record Chain(Polyline Points, bool Closed);

bool closed = graph.InDegree(run[0].Source) == 1;
Fin<Polyline> walked = Corners(run, corner, a, b);
if (walked.Case is not Polyline points) { return walked.Map(static _ => Seq<Chain>()); }
if (closed) { points.Add(points[0]); }
chains.Add(new Chain(points, closed));
```

To:

```csharp
// Chain.Closed DELETED

public sealed record Chain(Polyline Points);

bool closed = graph.InDegree(run[0].Source) == 1;
Fin<Polyline> walked = Corners(run, corner, a, b);
if (walked.Case is not Polyline points) { return walked.Map(static _ => Seq<Chain>()); }
if (closed) { points.Add(points[0]); }
chains.Add(new Chain(points));
```

Why: Rhino's `Polyline.IsClosed` already derives closedness from the endpoints, and every closed run here appends its first point before construction. The second record column can disagree with the geometry and expands the generated record surface; deleting it leaves one authoritative representation without losing open/closed capability.

Ripples: drop the Boolean constructor argument at `Meshing/slice.md:231` and `Meshing/offset.md:586`. At `Processing/flatten.md:173-180`, preserve `points.Count >= 3` as the distinct-vertex admission gate, then append `points[0]` and construct `new Chain(points)`; `Cycles.Of` intentionally returns each cycle without repeating its seed. Replace the removed member with `Points.IsClosed` at `Drawing/hatch.md:301,498`, `Meshing/slice.md:334-347`, `Drawing/view.md:690`, `Rasm.Fabrication/.planning/Nesting/nfp.md:1363`, `Additive/slicing.md:852,1200`, `Additive/production.md:1413-1414`, `Verify/removal.md:877`, `Verify/audit.md:894`, `Forming/tube.md:1154`, and `Geometry2D/algebra.md:713-717`; align the explicit `Chain(Closed: false)` prose at `Meshing/slice.md:5` to endpoint-derived closure.

# 6. Put chain construction on the chain owner

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:89-92` and `:132`, anchored at `Chain` followed by `internal static class ChainWalk`.

From:

```csharp
public sealed record Chain(Polyline Points);

internal static class ChainWalk {
    internal static Fin<Seq<Chain>> Of(
```

To:

```csharp
public sealed record Chain(Polyline Points) {
    internal static Fin<Seq<Chain>> Of(
```

```csharp
// ChainWalk DELETED
```

Why: `ChainWalk` has one operation and produces only `Chain`; keeping it as a sibling type adds a module-level owner without a distinct identity, admission path, payload timing, or consumer. The move deletes that type while retaining one shared construction method.

Ripples: replace `ChainWalk.Of` with `Chain.Of` at `Meshing/arrangement.md:485` and align that page's auto/package/diagram references to the nested owner.

# 7. Build the bidirectional graph directly

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:41` and `:93-99`, anchored at the `IndexSet` alias and the graph construction in `Chain.Of`.

From:

```csharp
using IndexSet = System.Collections.Generic.HashSet<int>;

IndexSet incoming = [.. rows.Select(static row => row.To)];
BidirectionalGraph<int, SEdge<int>> graph = rows
    .OrderBy(row => incoming.Contains(row.From)).ThenBy(static row => row.From)
    .Select(static row => new SEdge<int>(row.From, row.To))
    .ToAdjacencyGraph<int, SEdge<int>>(allowParallelEdges: true)
    .ToBidirectionalGraph();
```

To:

```csharp
// IndexSet DELETED
HashSet<int> incoming = [.. rows.Select(static row => row.To)];
BidirectionalGraph<int, SEdge<int>> graph = rows
    .OrderBy(row => incoming.Contains(row.From)).ThenBy(static row => row.From)
    .Select(static row => new SEdge<int>(row.From, row.To))
    .ToBidirectionalGraph<int, SEdge<int>>(allowParallelEdges: true);
```

Why: QuikGraph already materializes a bidirectional graph directly from an edge stream. The adjacency-graph intermediate builds the same outgoing index only to copy it into the required container, and the one-use alias adds a second name without shortening the read.

# 8. Count the closing arc when proving cycle coverage

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:108-119`, anchored at the `covered` accumulation and the `closed` classification in `Chain.Of`.

From:

```csharp
covered += run.Length;
bool closed = graph.InDegree(run[0].Source) == 1;
Fin<Polyline> walked = Corners(run, corner, a, b);
if (walked.Case is not Polyline points) { return walked.Map(static _ => Seq<Chain>()); }
if (closed) { points.Add(points[0]); }
chains.Add(new Chain(points));
```

To:

```csharp
bool closed = graph.InDegree(run[0].Source) == 1;
covered += run.Length + (closed ? 1 : 0);
Fin<Polyline> walked = Corners(run, corner, a, b);
if (walked.Case is not Polyline points) { return walked.Map(static _ => Seq<Chain>()); }
if (closed) { points.Add(points[0]); }
chains.Add(new Chain(points));
```

Why: QuikGraph's predecessor observer records DFS tree edges. An open component's tree path contains every edge, but a cycle's closing back-edge is deliberately absent from that path and is represented by the appended first point. Counting only `run.Length` therefore makes every valid closed loop fall through to `IncompleteIntersectionWalk`; adding the one proven closing arc makes the completeness check agree with the emitted chain without weakening the typed fallback for genuinely missing coverage.

# 9. Traverse optional corners directly into the failure carrier

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:114-117` and `:124-131`, anchored at the sole `Corners` call and the private `Corners` helper.

From:

```csharp
Fin<Polyline> walked = Corners(run, corner, a, b);
if (walked.Case is not Polyline points) { return walked.Map(static _ => Seq<Chain>()); }
if (closed) { points.Add(points[0]); }
chains.Add(new Chain(points));

static Fin<Polyline> Corners(SEdge<int>[] run, Func<int, Option<Point3d>> corner, PrimitiveKind a, PrimitiveKind b) {
    Polyline line = new();
    foreach (int slot in run.Select(static edge => edge.Source).Append(run[^1].Target)) {
        if (corner(slot).Case is not Point3d at) { return Fin.Fail<Polyline>(new GeometryFault.MissingIntersectionVertex(a, b, slot)); }
        line.Add(at);
    }
    return Fin.Succ(line);
}
```

To:

```csharp
Fin<Polyline> walked = toSeq(run.Select(static edge => edge.Source).Append(run[^1].Target))
    .TraverseM(slot => corner(slot).ToFin(new GeometryFault.MissingIntersectionVertex(a, b, slot)))
    .As()
    .Map(static points => new Polyline(points));
if (walked.Case is not Polyline points) { return walked.Map(static _ => Seq<Chain>()); }
if (closed) { points.Add(points[0]); }
chains.Add(new Chain(points));

// Corners DELETED
```

Why: `Option.ToFin` owns absence-to-fault admission and `TraverseM` owns dependent sequence inversion. Their composition removes the hand-rolled early-return loop and the single-call private member while preserving the first missing slot as the exact failure.

# 10. Make the frozen crossing table structurally immutable

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:134-143` and `:595-597`, anchored at the `CrossTable` primary constructor, its lookup materialization, and the segment projection in `Chains`.

From:

```csharp
public sealed record CrossTable(
    Crossing[] Rows,
    (int A, int B, int FaceA, int FaceB)[] Segments,
    (int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)[] Coplanar) {
```

```csharp
readonly ILookup<int, (int A, int B, int FaceA, int FaceB)>[] onFace =
    [Segments.ToLookup(static s => s.FaceA), Segments.ToLookup(static s => s.FaceB)];
readonly ILookup<int, (int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)>[] onCoplanar =
    [Coplanar.ToLookup(static s => s.FaceA), Coplanar.ToLookup(static s => s.FaceB)];

Chain.Of(table.Segments.Select(static s => (s.A, s.B)), slot => table.Rows[slot].Point.Round(), a, b)
```

To:

```csharp
public sealed record CrossTable(
    Arr<Crossing> Rows,
    Arr<(int A, int B, int FaceA, int FaceB)> Segments,
    Arr<(int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)> Coplanar) {
```

```csharp
readonly ILookup<int, (int A, int B, int FaceA, int FaceB)>[] onFace =
    [toSeq(Segments).ToLookup(static s => s.FaceA), toSeq(Segments).ToLookup(static s => s.FaceB)];
readonly ILookup<int, (int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)>[] onCoplanar =
    [toSeq(Coplanar).ToLookup(static s => s.FaceA), toSeq(Coplanar).ToLookup(static s => s.FaceB)];

Chain.Of(table.Segments.Map(static s => (s.A, s.B)), slot => table.Rows[slot].Point.Round(), a, b)
```

Why: the page promises a frozen projection, but public arrays let every consumer mutate the run after lookup indexes are built. LanguageExt `Arr<T>` is the admitted immutable indexed carrier, retains direct slot reads and enumeration, and lets `Freeze` keep the same target-typed collection expressions. Its deliberately narrow member surface requires explicit `toSeq` re-entry before LINQ's `ToLookup`, while its own `Map` keeps the segment projection on the carrier; preserving array-style `Select` calls would make the replacement invalid.

Ripples: replace array `Length` with carrier `Count` at `Processing/repair.md:383` and `Rasm.Fabrication/.planning/Forming/tube.md:1030-1031`. The existing `foreach`, indexer, and explicit `toSeq` consumers remain valid.

# 11. Seat the crossing store inside its sole operation owner

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:149-180` and `:215-216`, anchored at the complete `CrossingStore` declaration and the opening of `Intersection`.

From:

```csharp
public sealed class CrossingStore {
    Crossing[] rows;
```

```csharp
    void Grow(int needed) {
        if (needed <= rows.Length) { return; }
        Array.Resize(ref rows, int.Max(needed, rows.Length << 1));
    }
}
```

```csharp
public static class Intersection {
    public static Fin<IntersectResult> Apply(IntersectOp op, Op? key = null) {
```

To:

```csharp
public static class Intersection {
    private sealed class CrossingStore {
        Crossing[] rows;
```

```csharp
        void Grow(int needed) {
            if (needed <= rows.Length) { return; }
            Array.Resize(ref rows, int.Max(needed, rows.Length << 1));
        }
    }
```

```csharp
    public static Fin<IntersectResult> Apply(IntersectOp op, Op? key = null) {
```

Why: the mutable store is reached only by `Intersection` private methods and exists only to freeze one `CrossTable`. Nesting it under that operation removes one module-level type and makes the store lifecycle structurally private without introducing a forwarding owner or qualified uses inside the algorithm.

# 12. Delete the nested store's forwarding surface

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:159-160`, `:171-172`, `:430`, `:434`, `:483-484`, `:513`, `:517`, `:550`, and `:565`, anchored at the four forwarding members on `CrossingStore` and their calls inside `Intersection`.

From:

```csharp
public int Count => count;
public Crossing Row(int slot) => rows[slot];
public void Segment(int a, int b, int faceA, int faceB) => segments.Add((a, b, faceA, faceB));
public void CoplanarRow(int a, int b, int faceA, int faceB, int carrierU, int carrierV, int carrierSide) => coplanar.Add((a, b, faceA, faceB, carrierU, carrierV, carrierSide));

(Implicit left, Implicit right) = (store.Row(l).Point, store.Row(r).Point);
store.Segment(ends[k], ends[k + 1], fa, fb);
Implicit p0 = store.Row(e0).Point;
Implicit p1 = store.Row(e1).Point;
(Implicit left, Implicit right) = (store.Row(l).Point, store.Row(r).Point);
store.CoplanarRow(kept[k], kept[k + 1], fa, fb, u, v, carrierSide);
store.Segment(from, to, f, -1);
```

To:

```csharp
// CrossingStore.Count DELETED
// CrossingStore.Row DELETED
// CrossingStore.Segment DELETED
// CrossingStore.CoplanarRow DELETED

(Implicit left, Implicit right) = (store.rows[l].Point, store.rows[r].Point);
store.segments.Add((ends[k], ends[k + 1], fa, fb));
Implicit p0 = store.rows[e0].Point;
Implicit p1 = store.rows[e1].Point;
(Implicit left, Implicit right) = (store.rows[l].Point, store.rows[r].Point);
store.coplanar.Add((kept[k], kept[k + 1], fa, fb, u, v, carrierSide));
store.segments.Add((from, to, f, -1));
```

Why: `Count` has no reader anywhere in `libs/dotnet`; `Row` only forwards an indexer; and `Segment`/`CoplanarRow` only rename `List.Add`. After move 11 makes `CrossingStore` a private implementation type of `Intersection`, every read and write remains inside the same owner, and the already-semantic `rows`, `segments`, and `coplanar` columns resolve the operation in one hop. Deleting all four members narrows the store before move 13 removes its backing count.

# 13. Delegate row capacity and append state to `List`

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:149-157` and `:165-179`, anchored at the store fields and constructor, the new-row tail of `Intern`, `Freeze`, and `Grow`.

From:

```csharp
Crossing[] rows;
readonly Dictionary<CrossKey, int> interned = [];
readonly Dictionary<(long X, long Y, long Z), int> byBits = [];
readonly List<(int A, int B, int FaceA, int FaceB)> segments = [];
readonly List<(int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)> coplanar = [];
int count;

public CrossingStore(Dimension seed) { rows = new Crossing[seed.Value]; }

Grow(count + 1);
rows[count] = new Crossing(point, key);
if (point.IsExplicit) { byBits[Axis.BitKey(point.AsExplicit)] = count; }
return interned[key] = count++;

public CrossTable Freeze() => new([.. rows.AsSpan(0, count)], [.. segments], [.. coplanar]);

void Grow(int needed) {
    if (needed <= rows.Length) { return; }
    Array.Resize(ref rows, int.Max(needed, rows.Length << 1));
}
```

To:

```csharp
readonly List<Crossing> rows;
readonly Dictionary<CrossKey, int> interned = [];
readonly Dictionary<(long X, long Y, long Z), int> byBits = [];
readonly List<(int A, int B, int FaceA, int FaceB)> segments = [];
readonly List<(int A, int B, int FaceA, int FaceB, int CarrierU, int CarrierV, int CarrierSide)> coplanar = [];

// count DELETED

public CrossingStore(Dimension seed) { rows = new(seed.Value); }

int slot = rows.Count;
rows.Add(new Crossing(point, key));
if (point.IsExplicit) { byBits[Axis.BitKey(point.AsExplicit)] = slot; }
return interned[key] = slot;

public CrossTable Freeze() => new([.. rows], [.. segments], [.. coplanar]);

// Grow DELETED
```

Why: `List<Crossing>` already owns capacity seeding, indexed access, amortized growth, and enumeration. The explicit local preserves the exact pre-append slot used by both indexes while deleting the manually synchronized array/count protocol and its private `Grow` member without changing frozen output or insertion order.

# 14. Return the constructed point from direct segment crossing

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:255-263`, anchored at `static Option<Crossing> CrossSegments2D`.

From:

```csharp
static Option<Crossing> CrossSegments2D(Line a, Line b, Axis plane) {
    Sign d1 = Predicate.Orient2D(a.From, a.To, b.From, plane);
    Sign d2 = Predicate.Orient2D(a.From, a.To, b.To, plane);
    Sign d3 = Predicate.Orient2D(b.From, b.To, a.From, plane);
    Sign d4 = Predicate.Orient2D(b.From, b.To, a.To, plane);
    return d1.Times(d2) == Sign.Negative && d3.Times(d4) == Sign.Negative
        ? Some(new Crossing(new Implicit.SegmentIntersection(a.From, a.To, b.From, b.To, plane), CrossKey.Of(0, 0, 1, -1)))
        : None;
}
```

To:

```csharp
static Option<Implicit> CrossSegments2D(Line a, Line b, Axis plane) {
    Sign d1 = Predicate.Orient2D(a.From, a.To, b.From, plane);
    Sign d2 = Predicate.Orient2D(a.From, a.To, b.To, plane);
    Sign d3 = Predicate.Orient2D(b.From, b.To, a.From, plane);
    Sign d4 = Predicate.Orient2D(b.From, b.To, a.To, plane);
    return d1.Times(d2) == Sign.Negative && d3.Times(d4) == Sign.Negative
        ? Some<Implicit>(new Implicit.SegmentIntersection(a.From, a.To, b.From, b.To, plane))
        : None;
}
```

Why: a direct segment query has no arena entities, so `CrossKey.Of(0, 0, 1, -1)` is fabricated identity that no caller can interpret. `Crossing` belongs only after a real key is classified and interned; the primitive predicate should return its exact constructed point.

# 15. Project optional direct hits with `Map` and `ToSeq`

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:219-226`, anchored at the three direct-result dispatch arms in `Intersection.Apply`.

From:

```csharp
segmentSegment: s => Fin.Succ(CrossSegments2D(s.A, s.B, s.Plane)
    .Match(Some: c => (IntersectResult)new IntersectResult.Points(Seq(c.Point.Round())), None: () => new IntersectResult.Points(Seq<Point3d>()))),
segmentTriangle: s => Fin.Succ((IntersectResult)new IntersectResult.Points(
    EdgePierce(s.Edge.From, s.Edge.To, s.Ta, s.Tb, s.Tc).Match(Some: p => Seq(p.Round()), None: () => Seq<Point3d>()))),
triangleTriangle: t => Fin.Succ((IntersectResult)new IntersectResult.Segments(
    TriTriSegment(t.Pa, t.Pb, t.Pc, t.Qa, t.Qb, t.Qc).Match(
        Some: seg => Seq(new Line(seg.A.Round(), seg.B.Round())),
        None: () => Seq<Line>()))),
```

To:

```csharp
segmentSegment: s => Fin.Succ((IntersectResult)new IntersectResult.Points(
    CrossSegments2D(s.A, s.B, s.Plane).Map(static point => point.Round()).ToSeq())),
segmentTriangle: s => Fin.Succ((IntersectResult)new IntersectResult.Points(
    EdgePierce(s.Edge.From, s.Edge.To, s.Ta, s.Tb, s.Tc).Map(static point => point.Round()).ToSeq())),
triangleTriangle: t => Fin.Succ((IntersectResult)new IntersectResult.Segments(
    TriTriSegment(t.Pa, t.Pb, t.Pc, t.Qa, t.Qb, t.Qc)
        .Map(static segment => new Line(segment.A.Round(), segment.B.Round())).ToSeq())),
```

Why: `Option.Map` owns present-value projection and `Option.ToSeq` owns the zero-or-one collection egress. The direct composition removes six hand-written Some/None arms and consumes the corrected `Option<Implicit>` without a fake `Crossing` wrapper.

# 16. Consume exact segment points without rebuilding a crossing wrapper

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:354-356`, `:462-465`, and `:505-508`, anchored at the three internal `CrossSegments2D` consumers.

From:

```csharp
if (CrossSegments2D(new Line(u, v), new Line(s, t), plane).Case is Crossing hit) { kept.Add(hit.Point); }
if (CrossSegments2D(new Line(soup.Position(u), soup.Position(v)), new Line(other.Position(s2), other.Position(t2)), plane).Case is Crossing cross) {
    Keep(ends, store.Intern(cross.Point, CoplanarKey(side, otherSide, u, v, s2, t2)));
}
if (CrossSegments2D(new Line(pu, pv), new Line(other.Position(s), other.Position(t)), plane).Case is Crossing hit) {
    Keep(kept, store.Intern(hit.Point, CoplanarKey(carrierSide, otherSide, u, v, s, t)));
}
```

To:

```csharp
if (CrossSegments2D(new Line(u, v), new Line(s, t), plane).Case is Implicit hit) { kept.Add(hit); }
if (CrossSegments2D(new Line(soup.Position(u), soup.Position(v)), new Line(other.Position(s2), other.Position(t2)), plane).Case is Implicit hit) {
    Keep(ends, store.Intern(in hit, CoplanarKey(side, otherSide, u, v, s2, t2)));
}
if (CrossSegments2D(new Line(pu, pv), new Line(other.Position(s), other.Position(t)), plane).Case is Implicit hit) {
    Keep(kept, store.Intern(in hit, CoplanarKey(carrierSide, otherSide, u, v, s, t)));
}
```

Why: after move 14 returns the exact constructed point, these consumers can add or intern it directly. Removing the temporary `Crossing` shape at all three sites prevents the fabricated direct-query key from surviving indirectly and deletes the `.Point` forwarding reads.

# 17. Nest crossing rows under their frozen table

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:87`, `:134-137`, `:150`, and `:166`, anchored at the module-level `Crossing` record, the `CrossTable` constructor, and the store's row declaration and append.

From:

```csharp
public readonly record struct Crossing(Implicit Point, CrossKey Key);

public sealed record CrossTable(
    Arr<Crossing> Rows,
```

```csharp
readonly List<Crossing> rows;

rows.Add(new Crossing(point, key));
```

To:

```csharp
// Crossing DELETED

public sealed record CrossTable(
    Arr<CrossTable.Row> Rows,
```

```csharp
public readonly record struct Row(Implicit Point, CrossKey Key);
```

```csharp
readonly List<CrossTable.Row> rows;

rows.Add(new CrossTable.Row(point, key));
```

Why: after moves 14-16 remove the fabricated direct-query wrapper, every `Crossing` instance is a row of one `CrossTable` and has no independent admission path or consumer. Nesting it as `CrossTable.Row` keeps the real carrier while removing one module-level type and making its ownership resolve in one hop.

Ripples: change the explicit row annotations to `CrossTable.Row` at `Meshing/arrangement.md:343`, `Processing/repair.md:442`, and `Rasm.Fabrication/.planning/Forming/tube.md:1053`; member access remains unchanged.

# 18. Convert the ray's optional best hit directly

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:586-590`, anchored at `best = best.Match` and the returned `IntersectResult.Points`.

From:

```csharp
best = best.Match(
    Some: held => Predicate.Compare(in hit, in held, axis).Times(forward) == Sign.Negative ? Some(hit) : Some(held),
    None: () => Some(hit));
}
return (IntersectResult)new IntersectResult.Points(best.Match(Some: static h => Seq(h.Round()), None: static () => Seq<Point3d>()));
```

To:

```csharp
best = best.Match(
    Some: held => Predicate.Compare(in hit, in held, axis).Times(forward) == Sign.Negative ? Some(hit) : Some(held),
    None: () => Some(hit));
}
return (IntersectResult)new IntersectResult.Points(best.Map(static hit => hit.Round()).ToSeq());
```

Why: selection still needs the total two-arm fold because the absent case installs the first hit; the final projection does not. `Map().ToSeq()` states the zero-or-one result directly and removes the duplicate collection constructors.

# 19. Reuse the exact zero-pair classifier in sectioning

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:537-541`, anchored at the `flat` search in `Section`.

From:

```csharp
Option<int> flat = None;
for (int e = 0; e < 3 && flat.IsNone; e++) {
    if (s[e] == Sign.Zero && s[(e + 1) % 3] == Sign.Zero) { flat = Some(e); }
}
if (flat.Case is int lying) {
```

To:

```csharp
if (ZeroPair(s).Case is int lying) {
```

Why: the all-zero face has already continued, so `ZeroPair` returns exactly the same lying-edge ordinal for the remaining three two-zero patterns. Reusing it deletes mutable optional state and a bounded search loop without adding a helper.

# 20. Return sectioning's infallible result directly

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:230` and `:522-568`, anchored at the `Section` dispatch arm, method declaration, and terminal return.

From:

```csharp
planeMesh: p => Section(p, site).Bind(store => Chains(store.Freeze(), p.Kind))));

static Fin<CrossingStore> Section(IntersectOp.PlaneMesh op, Op key) {
```

```csharp
return Fin.Succ(store);
```

To:

```csharp
planeMesh: p => Chains(Section(p).Freeze(), PrimitiveKind.Plane, PrimitiveKind.Mesh))));

static CrossingStore Section(IntersectOp.PlaneMesh op) {
```

```csharp
return store;
```

Why: `Section` never reads `key` and has no failure arm; wrapping its store in `Fin.Succ` makes the caller bind a failure channel the operation cannot produce. Returning the store directly removes the unused parameter, one carrier construction, and one `Bind` while `Chains` remains the real fallible boundary.

# 21. Use the exact predicate's projection axis in sliver admission

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:247-252`, anchored at `Sliver` and its sole private projection helper `Swap`.

From:

```csharp
static bool Sliver(Point3d a, Point3d b, Point3d c) =>
    Predicate.Orient2D(a, b, c) == Sign.Zero
    && Predicate.Orient2D(Swap(a, Axis.X), Swap(b, Axis.X), Swap(c, Axis.X)) == Sign.Zero
    && Predicate.Orient2D(Swap(a, Axis.Y), Swap(b, Axis.Y), Swap(c, Axis.Y)) == Sign.Zero;

static Point3d Swap(Point3d p, Axis axis) => new(axis.U.Read(p), axis.V.Read(p), 0.0);
```

To:

```csharp
static bool Sliver(Point3d a, Point3d b, Point3d c) =>
    Predicate.Orient2D(a, b, c, Axis.Z) == Sign.Zero
    && Predicate.Orient2D(a, b, c, Axis.X) == Sign.Zero
    && Predicate.Orient2D(a, b, c, Axis.Y) == Sign.Zero;

// Swap DELETED
```

Why: the exact predicate already accepts a projection `Axis`, and its `Implicit` conversion absorbs each explicit `Point3d`. Calling that owner directly preserves the same XY, YZ, and ZX sign tests while deleting the one-call projection helper and six temporary `Point3d` constructions.

# 22. Inline the one-hop sign projection

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:428`, `:485`, `:575`, and `:600`, anchored at all three `Along` calls and the private wrapper.

From:

```csharp
Sign forward = Along(material, axis);
Sign order = Predicate.Compare(in p0, in p1, axis).Times(Along(material, axis));
Sign forward = Along(op.Ray.Direction, axis);

static Sign Along(Vector3d d, Axis axis) => Sign.Of(axis.Along(d));
```

To:

```csharp
Sign forward = Sign.Of(axis.Along(material));
Sign order = Predicate.Compare(in p0, in p1, axis).Times(Sign.Of(axis.Along(material)));
Sign forward = Sign.Of(axis.Along(op.Ray.Direction));

// Along DELETED
```

Why: `Along` only renames the composition of the existing `Axis.Along` projection and `Sign.Of` classifier. Direct use resolves both owners in one hop and deletes one private member with no repeated logic beyond the already-public composition.

# 23. Inline the sole shared-vertex census

Location: `libs/dotnet/Rasm/.planning/Meshing/intersect.md:392-410`, anchored at the `SelfCross` fold and private `SharedVertices` helper.

From:

```csharp
.Map(pairs => pairs.Fold(new CrossingStore(op.Policy.SeedCapacity), (store, pair) =>
    pair.Left < pair.Right && SharedVertices(soup, pair.Left, pair.Right) < 2
        ? PairCrossings(store, soup, soup, pair.Left, pair.Right, op.Policy, sideA: 0, sideB: 0)
        : store));

static int SharedVertices(MeshEdit soup, int fa, int fb) {
    (int a0, int a1, int a2) = soup.Face(fa);
    (int b0, int b1, int b2) = soup.Face(fb);
    int shared = 0;
    foreach (int v in (ReadOnlySpan<int>)[a0, a1, a2]) {
        if (v == b0 || v == b1 || v == b2) { shared++; }
    }
    return shared;
}
```

To:

```csharp
.Map(pairs => pairs.Fold(new CrossingStore(op.Policy.SeedCapacity), (store, pair) => {
    if (pair.Left >= pair.Right) { return store; }
    (int a0, int a1, int a2) = soup.Face(pair.Left);
    (int b0, int b1, int b2) = soup.Face(pair.Right);
    int shared = 0;
    foreach (int vertex in (ReadOnlySpan<int>)[a0, a1, a2]) {
        if (vertex == b0 || vertex == b1 || vertex == b2) { shared++; }
    }
    return shared < 2 ? PairCrossings(store, soup, soup, pair.Left, pair.Right, op.Policy, sideA: 0, sideB: 0) : store;
}));

// SharedVertices DELETED
```

Why: the census has one caller and no meaning outside the self-intersection pair gate. Inlining its compact three-vertex loop deletes one private member without replacing it with repeated per-vertex Boolean arithmetic, and preserves the exact count for admitted triangular faces.
