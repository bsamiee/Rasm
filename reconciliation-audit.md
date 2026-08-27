# 1. Collapse parametric directions into their owning product

### Location

- `reconciliation.md:58-67`, anchors `Parametric`, `Directions`, and `Direction`
- `reconciliation.md:78`, anchor `Of(Arr<Direction> directions`

### From

```csharp
internal Parametric(Arr<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet) {
    Directions = directions; Weights = weights; ControlNet = controlNet;
}
public Arr<Direction> Directions { get; }
```

```csharp
public readonly record struct Direction(int Degree, Arr<double> Knots);
```

```csharp
public static Fin<EncodeForm> Of(Arr<Direction> directions, Arr<double> weights,
    Arr<Point3d> controlNet, Context context, Op? key = null) {
```

### To

```csharp
internal Parametric(Arr<(int Degree, Arr<double> Knots)> directions,
    Arr<double> weights, Arr<Point3d> controlNet) {
    Directions = directions; Weights = weights; ControlNet = controlNet;
}
public Arr<(int Degree, Arr<double> Knots)> Directions { get; }
```

```csharp
// EncodeForm.Direction DELETED
```

```csharp
public static Fin<EncodeForm> Of(Arr<(int Degree, Arr<double> Knots)> directions,
    Arr<double> weights, Arr<Point3d> controlNet, Op? key = null) {
```

### Why

`Direction` has no invariant, behavior, identity, or independent consumer; it only names the two columns of the enclosing parametric case. A named tuple preserves those column names and structural value semantics while deleting a nested type. Removing `Context` also corrects this identity boundary: a positive finite knot span is normalized to `[0,1]`, so model tolerance neither contributes to the digest nor belongs in admission.

### Ripples

- `libs/dotnet/Rasm/.planning/Parametric/nurbs.md:22`, anchor `EncodeForm.Direction`: describe the named direction rows instead of a nested carrier.
- `libs/dotnet/Rasm/.planning/Parametric/nurbs.md:306-315`, anchor `ToEncodeForm`: replace `Arr<EncodeForm.Direction>` and its constructors with `Arr<(int Degree, Arr<double> Knots)>`; the existing fourth `k` argument then binds to `Op? key`.

# 2. Make parametric admission one bounded traversal

### Location

- `reconciliation.md:78-110`, anchors parametric `EncodeForm.Of`, `Canonicalize`, `Clamped`, and `Admit`

### From

```csharp
return directions.Count >= 1
    ? toSeq(directions.AsIterable())
        .TraverseM(direction => Canonicalize(direction: direction, context: context, key: op)).As()
        .Bind(admitted => Admit(directions: admitted, weights: weights, controlNet: controlNet, key: op))
    : Fin.Fail<EncodeForm>(op.InvalidInput());
```

```csharp
static Fin<Direction> Canonicalize(Direction direction, Context context, Op key) {
    Arr<double> knots = direction.Knots;
```

```csharp
static bool Clamped(Direction direction) =>
```

```csharp
static Fin<EncodeForm> Admit(Seq<Direction> directions, Arr<double> weights,
    Arr<Point3d> controlNet, Op key) {
```

### To

```csharp
return directions.Count is >= 1 and <= 3
    ? toSeq(directions.AsIterable()).TraverseM(direction => {
        (int degree, Arr<double> knots) = direction;
        bool shaped = degree >= 1 && degree <= (knots.Count - 2) / 2
            && knots.All(static knot => ValidityClaim.Finite(knot))
            && Enumerable.Range(1, knots.Count - 1).All(i => knots[i - 1] <= knots[i]);
        double span = shaped ? knots[^1] - knots[0] : 0.0;
```

```csharp
return guard(shaped && ValidityClaim.Positive(span), op.InvalidInput()).ToFin()
    .Map(_ => new Arr<double>([.. knots.AsIterable().Select(knot => (knot - knots[0]) / span)]))
    .Bind(normalized => guard(Enumerable.Range(0, degree + 1).All(i =>
            normalized[i] == 0.0 && normalized[normalized.Count - 1 - i] == 1.0), op.InvalidInput()).ToFin()
        .Map(_ => (Degree: degree, Knots: normalized)));
}).As().Bind(admitted => {
```

```csharp
long controls = admitted.Fold(1L, (product, direction) => {
    int extent = direction.Knots.Count - direction.Degree - 1;
    return product > controlNet.Count / extent ? controlNet.Count + 1L : product * extent;
});
return guard(controls == controlNet.Count && weights.Count == controlNet.Count
```

```csharp
.Map(_ => (EncodeForm)new Parametric(
    new Arr<(int Degree, Arr<double> Knots)>([.. admitted]), weights, controlNet));
}) : Fin.Fail<EncodeForm>(op.InvalidInput());
```

```csharp
// EncodeForm.Canonicalize DELETED
// EncodeForm.Clamped DELETED
// EncodeForm.Admit DELETED
```

### Why

The three private members have one caller and fragment one admission transaction. The traversal now states the page's one-to-three direction domain, keeps shape, normalization, and clamping in the same monadic step, and admits the full tensor in the final bind. The division-before-multiplication guard saturates once the product cannot equal `controlNet.Count`, removing the `unchecked` overflow path without adding another numeric carrier or helper.

# 3. Use structural face rows instead of a comparer type

### Location

- `reconciliation.md:22`, anchor `using Generator.Equals`
- `reconciliation.md:118-170`, anchors `FaceCycles`, `CanonicalTopology`, and `Rotated`
- `reconciliation.md:187-209`, anchor `Entities(int vertices, Arr<(int Min, int Max)> edges, Arr<int[]> faces)`
- `reconciliation.md:277-282`, anchor `MeshStream`

### From

```csharp
using Generator.Equals;
```

```csharp
public sealed class FaceCycles : IEqualityComparer<Arr<int[]>> {
    public static readonly FaceCycles Default = new();
    public bool Equals(Arr<int[]> left, Arr<int[]> right) =>
        left.Count == right.Count && Enumerable.Range(0, left.Count).All(i => left[i].AsSpan().SequenceEqual(right[i]));
```

```csharp
[Equatable]
public sealed partial record CanonicalTopology(
    int VertexCount, Arr<(int Min, int Max)> Edges,
    [property: CustomEquality(typeof(FaceCycles))] Arr<int[]> Faces,
    Seq<RebuiltEntity> Entities) : IValidityEvidence {
```

### To

```csharp
// using Generator.Equals DELETED
// FaceCycles DELETED
```

```csharp
public sealed record CanonicalTopology(
    int VertexCount, Arr<(int Min, int Max)> Edges,
    Arr<Seq<int>> Faces, Seq<RebuiltEntity> Entities) : IValidityEvidence {
```

```csharp
Faces.All(cycle => cycle.Count >= 3 && cycle.All(vertex => vertex >= 0 && vertex < VertexCount)
    && Enumerable.Range(0, cycle.Count).All(i => cycle[i] != cycle[(i + 1) % cycle.Count])
    && LeastRotationIndex(cycle.AsSpan()) == 0),
Enumerable.Range(1, int.Max(Faces.Count - 1, 0)).All(i =>
    Faces[i - 1].AsSpan().SequenceCompareTo(Faces[i].AsSpan()) <= 0),
```

```csharp
Arr<Seq<int>> faces = [.. Enumerable.Range(0, mesh.Faces.Count)
    .Select(face => Rotated(mesh.TopologyVertices.IndicesFromFace(face)))
    .Order(Comparer<Seq<int>>.Create(static (a, b) => a.AsSpan().SequenceCompareTo(b.AsSpan())))];
```

```csharp
static Seq<int> Rotated(int[] cycle) {
    int pivot = LeastRotationIndex(cycle);
    return pivot == 0 ? toSeq(cycle) : toSeq([.. cycle[pivot..], .. cycle[..pivot]]);
}
```

```csharp
static Seq<RebuiltEntity> Entities(int vertices, Arr<(int Min, int Max)> edges,
    Arr<Seq<int>> faces) {
```

```csharp
KindHistogram: new Arr<int>([cycle.Distinct().Count, cycle.Count,
    Enumerable.Range(0, cycle.Count).Sum(i =>
```

```csharp
.Rows(rows: toSeq(topology.Faces.AsIterable()),
    field: static (cycle, field) => field.Rows(rows: cycle,
        field: static (vertex, slot) => slot.Ordinal(value: vertex)));
```

### Why

The inner mutable arrays force a public comparer type, singleton, comparer methods, generator annotation, and import solely to recover value equality. `Seq<int>` already supplies immutable structural equality, indexing, and `AsSpan`; nesting it in `Arr` preserves the ordered face roster without a second equality owner. The stored `Entities` projection remains because naming reads it repeatedly; recomputing it as a property would trade surface reduction for repeated graph reconstruction.

# 4. Localize topology construction and release the duplicate mesh

### Location

- `reconciliation.md:155-170`, anchors `OfMesh`, `Mesh mesh`, and `Rotated`
- `reconciliation.md:187-211`, anchor `Entities`

### From

```csharp
public static CanonicalTopology OfMesh(MeshSpace space) {
    Mesh mesh = space.DuplicateNative();
```

```csharp
static int[] Rotated(int[] cycle) {
```

```csharp
static Seq<RebuiltEntity> Entities(int vertices, Arr<(int Min, int Max)> edges,
    Arr<int[]> faces) {
```

### To

```csharp
public static CanonicalTopology OfMesh(MeshSpace space) {
    using Mesh mesh = space.DuplicateNative();
```

```csharp
return new CanonicalTopology(vertices, edges, faces, Entities(vertices, edges, faces));

static Seq<int> Rotated(int[] cycle) {
```

```csharp
static Seq<RebuiltEntity> Entities(int vertices, Arr<(int Min, int Max)> edges,
    Arr<Seq<int>> faces) {
```

### Why

`DuplicateNative` returns an independently owned Rhino mesh; reading topology without disposing it leaks the duplicate. `Rotated` and `Entities` exist only to build one `CanonicalTopology`, so seating them as local functions under `OfMesh` removes two type-level private members while retaining the stored entity projection and the two reusable class-level helpers, `LeastRotationIndex` and `Sorted`.

# 5. Remove the topology round-trip from the operation union

### Location

- `reconciliation.md:43-50`, anchor `ReconcileOp.BuildEntities`
- `reconciliation.md:74-76`, anchor `EncodeForm.Of(CanonicalTopology topology)`
- `reconciliation.md:222-248`, anchors `ReconcileAnswer.Topology`, `IsValid`, and `buildEntities`

### From

```csharp
public sealed record BuildEntities(MeshSpace Space) : ReconcileOp;
```

```csharp
public static EncodeForm Of(CanonicalTopology topology) => new Mesh(topology);
```

```csharp
public sealed record Topology(CanonicalTopology Value) : ReconcileAnswer;
```

```csharp
reconciled: static r => r.Value.IsValid,
topology: static t => t.Value.IsValid);
```

```csharp
buildEntities: static (k, b) => k.AcceptValue(
    (ReconcileAnswer)new ReconcileAnswer.Topology(CanonicalTopology.OfMesh(b.Space))));
```

### To

```csharp
// ReconcileOp.BuildEntities DELETED
```

```csharp
// EncodeForm.Of(CanonicalTopology) DELETED
```

```csharp
// ReconcileAnswer.Topology DELETED
```

```csharp
reconciled: static r => r.Value.IsValid);
```

```csharp
// Reconciliation.Apply.buildEntities arm DELETED
```

```csharp
new NamingHash(Digest(new EncodeForm.Mesh(admitted.Rebuilt)), addresses)
```

### Why

`BuildEntities` adds an operation case and answer case around `CanonicalTopology.OfMesh` without adding admission, policy, or behavior; the direct factory already preserves the genuine capability. Removing only this round-trip keeps the required one-entrypoint `ReconcileOp` rail for the two real reconciliation verbs while deleting two nested union types, one dispatch arm, one result arm, and the single-use topology overload.

### Ripples

- `libs/dotnet/Rasm/.planning/Drawing/pack.md:423-430`, anchor the `ReconcileAnswer` switch in private `Digest`: remove the `topology` arm after the generated answer union narrows to two cases.
- `libs/dotnet/Rasm.Rhino/.planning/Display/render.md:2926-2932`, anchor the `ReconcileAnswer` switch: remove the `topology` arm after the generated answer union narrows to two cases.

# 6. Hide the generated geometry-hash key member

### Location

- `reconciliation.md:115-116`, anchor `GeometryHash`

### From

```csharp
[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct GeometryHash;
```

### To

```csharp
[ValueObject<UInt128>]
public readonly partial struct GeometryHash;
```

### Why

No current consumer reads a named `GeometryHash.Value`; construction already uses generated `Create`, equality and hashing use the owner, and any future raw-key egress has generated `ToValue()`. Restoring the Thinktecture default private key removes a public member without weakening the typed content identity or inventing a hand-written projection.

# 7. Remove the duplicated name from address values

### Location

- `reconciliation.md:216-220`, anchors `NameAddress` and `NamingHash`
- `reconciliation.md:243-268`, anchors the reconcile arm, `Addresses`, and `Content`

### From

```csharp
public readonly record struct NameAddress(TopoName Name, EntityKind Kind, GeometryHash ContentHash);

public sealed record NamingHash(GeometryHash Whole, HashMap<TopoName, NameAddress> Addresses)
```

```csharp
public bool IsValid => ValidityClaim.All(
    Addresses.AsIterable().ForAll(static pair => pair.Key == pair.Value.Name));
```

```csharp
static Fin<HashMap<TopoName, NameAddress>> Addresses(NameTable prior, CanonicalTopology rebuilt) {
```

```csharp
static UInt128 Content(EntityKind kind, Arr<int> canonical) =>
    ContentHash.Of(state: (Kind: kind, Canonical: canonical),
```

### To

```csharp
// NameAddress DELETED
```

```csharp
public sealed record NamingHash(GeometryHash Whole,
    HashMap<TopoName, (EntityKind Kind, GeometryHash ContentHash)> Addresses)
```

```csharp
public bool IsValid => Addresses.Values.ForAll(static address => address.Kind is not null);
```

```csharp
.Bind(admitted => {
    Set<GeometryHash> live = admitted.Rebuilt.Entities.Fold(Set<GeometryHash>.Empty,
        static (set, entity) => set.Add(Content(entity.Kind, entity.Canonical)));
    return toSeq(admitted.Prior.Entries.Values).Traverse(entry => {
        GeometryHash digest = Content(entry.Kind, entry.Canonical);
```

```csharp
return live.Contains(digest)
    ? Validation.Success<Error, (TopoName Name, EntityKind Kind, GeometryHash ContentHash)>(
        (entry.Name, entry.Kind, digest))
    : Validation.Fail<Error, (TopoName Name, EntityKind Kind, GeometryHash ContentHash)>(
        new GeometryFault.TopologyContentMissing(entry.Name, entry.Kind));
```

```csharp
}).As().Map(rows => (ReconcileAnswer)new ReconcileAnswer.Reconciled(new NamingHash(
    Digest(new EncodeForm.Mesh(admitted.Rebuilt)),
    rows.Fold(HashMap<TopoName, (EntityKind Kind, GeometryHash ContentHash)>.Empty,
        static (map, row) => map.AddOrUpdate(row.Name, (row.Kind, row.ContentHash)))))).ToFin();
})
```

```csharp
// Reconciliation.Addresses DELETED
```

```csharp
static GeometryHash Content(EntityKind kind, Arr<int> canonical) =>
    GeometryHash.Create(ContentHash.Of(state: (Kind: kind, Canonical: canonical),
```

### Why

The `HashMap` key already owns `TopoName`; duplicating it in every value creates the only state that `NameAddress` and its full-map equality walk police. A named tuple retains the independent kind and typed content columns, while the transient traversal row carries the name only until map construction. Returning `GeometryHash` from `Content` also removes the raw `UInt128` live set and its immediate unwrap/rewrap. The one-call `Addresses` helper is folded into the reconcile arm without losing accumulating `Validation`.

# 8. Seat straight-through streams on their generated union arms

### Location

- `reconciliation.md:270-282`, anchors `Digest` and `MeshStream`
- `reconciliation.md:304-310`, anchor `ParametricStream`

### From

```csharp
mesh: static (writer, m) => MeshStream(topology: m.Topology, sink: writer),
cloud: static (writer, c) => CloudStream(source: c.Source, sink: writer),
parametric: static (writer, p) => ParametricStream(form: p, sink: writer))));
```

```csharp
static CanonicalWriter MeshStream(CanonicalTopology topology, CanonicalWriter sink) =>
```

```csharp
static CanonicalWriter ParametricStream(EncodeForm.Parametric form, CanonicalWriter sink) =>
```

### To

```csharp
mesh: static (writer, m) => writer.Ordinal(m.Topology.VertexCount)
    .Rows(toSeq(m.Topology.Edges.AsIterable()),
        static (edge, field) => field.Ordinal(edge.Min).Ordinal(edge.Max))
    .Rows(toSeq(m.Topology.Faces.AsIterable()),
        static (cycle, field) => field.Rows(cycle, static (vertex, slot) => slot.Ordinal(vertex))),
```

```csharp
cloud: static (writer, c) => CloudStream(c.Source, writer),
parametric: static (writer, p) => writer.Rows(toSeq(p.Directions.AsIterable()),
        static (direction, field) => field.Ordinal(direction.Degree)
            .Rows(toSeq(direction.Knots.AsIterable()), static (knot, slot) => slot.Bits(knot)))
```

```csharp
.Rows(toSeq(p.Weights.AsIterable()), static (weight, field) => field.Bits(weight))
.Rows(toSeq(p.ControlNet.AsIterable()), static (point, field) =>
    field.Bits(point.X).Bits(point.Y).Bits(point.Z)))));
```

```csharp
// Reconciliation.MeshStream DELETED
// Reconciliation.ParametricStream DELETED
```

### Why

These helpers each have one caller and only forward a case payload through a short writer chain. Seating the frozen layouts directly on the exhaustive generated `EncodeForm.Switch` arms deletes two private members, keeps the byte framing unchanged, and makes a new case's complete identity layout visible at the dispatch that must handle it.

# 9. Collapse the mirrored cloud roster and its private helper chain

### Location

- `reconciliation.md:35-41`, anchor `CloudForm`
- `reconciliation.md:270-275`, anchor the `EncodeForm.Cloud` digest arm
- `reconciliation.md:284-327`, anchors `CloudStream`, `Weighted`, `Lexicographic`, `LeastRotation`, and `Precedes`

### From

```csharp
[SmartEnum<int>]
public sealed partial class CloudForm {
    public static readonly CloudForm Cluster = new(key: 0);
    public static readonly CloudForm Polyline = new(key: 1);
    public static readonly CloudForm Ring = new(key: 2);
}
```

```csharp
cloud: static (writer, c) => CloudStream(source: c.Source, sink: writer),
```

```csharp
static (CloudForm Form, Seq<Point3d> Points, Seq<double> Mass) Weighted(
    Seq<Point3d> points, Arr<double> mass) {
```

```csharp
static Seq<Point3d> Lexicographic(Seq<Point3d> points) =>
```

```csharp
static CanonicalWriter CloudStream(VectorCloud source, CanonicalWriter sink) {
```

```csharp
static Seq<Point3d> LeastRotation(Seq<Point3d> ring) {
```

```csharp
static bool Precedes(Seq<Point3d> left, Seq<Point3d> right) =>
```

```csharp
static bool Precedes(Point3d left, Point3d right) =>
```

### To

```csharp
// CloudForm DELETED
```

```csharp
cloud: static (writer, c) => {
    (int Ordinal, Seq<Point3d> Points, Seq<double> Mass) canonical = c.Source.Switch(
        ringCase: static ring => (2, LeastRotation(ring.Vertices), Seq<double>.Empty),
        polylineCase: static chain => (1, chain.Vertices, Seq<double>.Empty),
        clusterCase: static cluster => cluster.Mass.Match(
```

```csharp
Some: mass => {
    Seq<(Point3d Point, double Mass)> rows = toSeq(cluster.Vertices
        .Map((point, index) => (Point: point, Mass: mass[index]))
        .OrderBy(static row => row.Point.X).ThenBy(static row => row.Point.Y)
        .ThenBy(static row => row.Point.Z).ThenBy(static row => row.Mass));
    return (0, rows.Map(static row => row.Point), rows.Map(static row => row.Mass));
},
```

```csharp
None: () => (0, toSeq(cluster.Vertices.OrderBy(static point => point.X)
    .ThenBy(static point => point.Y).ThenBy(static point => point.Z)), Seq<double>.Empty)));
CanonicalWriter result = writer.Ordinal(canonical.Ordinal)
    .Rows(canonical.Points, static (point, field) => field.Bits(point.X).Bits(point.Y).Bits(point.Z))
    .Rows(canonical.Mass, static (mass, field) => field.Bits(mass));
```

```csharp
static Seq<Point3d> LeastRotation(Seq<Point3d> ring) {
    Point3d least = ring.Fold(ring[0], static (min, point) =>
        (point.X, point.Y, point.Z).CompareTo((min.X, min.Y, min.Z)) < 0 ? point : min);
    return ring.Map(static (point, index) => (Point: point, Index: index))
        .Filter(row => row.Point == least)
```

```csharp
.Map(row => row.Index == 0 ? ring : ring.Skip(row.Index) + ring.Take(row.Index))
.Fold(ring, static (best, candidate) => Enumerable.Range(0, candidate.Count)
    .Where(i => candidate[i] != best[i])
    .Select(i => (candidate[i].X, candidate[i].Y, candidate[i].Z)
        .CompareTo((best[i].X, best[i].Y, best[i].Z))).FirstOrDefault() < 0 ? candidate : best);
}
return result;
},
```

```csharp
// Reconciliation.CloudStream DELETED
// Reconciliation.Weighted DELETED
// Reconciliation.Lexicographic DELETED
// Reconciliation.LeastRotation DELETED
// Reconciliation.Precedes(Seq<Point3d>, Seq<Point3d>) DELETED
// Reconciliation.Precedes(Point3d, Point3d) DELETED
```

### Why

`CloudForm` mirrors the already-exhaustive `VectorCloud` cases only to emit one frozen integer, so its public SmartEnum, three items, and generated lookup, parse, comparison, and conversion surface add no independent axis. Keeping each ordinal in its named generated union arm preserves the byte contract. The remaining helpers form one private, single-caller cloud pipeline; inlining the two short sorts and localizing ring rotation under the cloud arm removes six module-level symbols, while tuple comparison replaces both hand-written lexicographic comparator overloads.
