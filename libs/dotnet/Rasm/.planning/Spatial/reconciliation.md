# [RASM_TOPOLOGY_RECONCILIATION]

`Rasm.Spatial` reconciliation owns the one naming↔hash fence: the lineage-stable reference axis reconciled against the change-sensitive content axis through the single `Reconciliation.Apply` entry, over the frozen canonical byte layouts every content key hashes. `GeometryHash` and `TopoName` are type-distinct, so a cross-axis compare is a compile error, and the canonical bytes cross only the in-process seam, never sitting between wire and rail.

## [01]-[INDEX]

- [02]-[RECONCILIATION_BRIDGE]: `Reconciliation.Apply` folds one `ReconcileOp` into `GeometryHash` content keys and the `NamingHash` over the frozen canonical byte streams.

## [02]-[RECONCILIATION_BRIDGE]

- Owner: `GeometryHash` mints the content-axis identity only through the kernel `ContentHash.Of`, framing every field through the kernel `CanonicalWriter` rather than a page-local byte emitter; `CanonicalTopology` mints the immutable adjacency every encode, re-anchor, and entity build reads.
- Cases: each `EncodeForm` stream freezes its own canonical order — a `ClusterCase` sorts vertices lexicographically and hashes any mass column as content, a `PolylineCase` stores order as content, a `RingCase` rotates to its least rotation with winding preserved under the rotation law the mesh face cycles carry, and `Parametric` takes the direction count as the curve/surface/volume generator. `CloudForm` rows carry the frozen wire ordinal each cloud stream leads with, so the discriminant has a named owner on the page that owns the wire. `CanonicalTopology.OfMesh` is the one native admission.
- Entry: `EncodeForm.Of` discriminates admission on input shape, its raw-array parametric head the one validated ingress; that head CANONICALIZES each direction's knots onto `[0,1]` before it gates, so a producer's divide-normalized vector admits instead of forking identical geometry out of the corpus, and a refusal routes an `Op`-keyed admission fault rather than throwing.
- Auto: `Mesh` encoding re-hashes identically under a morph and distinctly under a topology break; every arm gates input and answer through the acceptance oracle, so consumers never re-check the `IValidityEvidence` claims.
- Law: `NamingHash` is the reconciliation evidence the Persistence structural merge consumes per node, registering into the `OpAcceptance.ValidityOf` oracle like every kernel result — no parallel reconciliation ledger.
- Law: `RebuiltEntity.Canonical` is a run of int WORDS, never a pre-serialized block, so the framing decision stays at `CanonicalWriter` and the entity keeps structural equality — a `byte[]` column compared by reference, which is precisely what a record whose identity IS its content cannot afford. Three `EncodeForm` streams stay BYTE-UNCHANGED under the writer, and the correspondence is member-for-member: `Word` was int32-LE and `Ordinal` is; `Real` wrote the raw IEEE754 pattern little-endian and `Bits` writes exactly that pattern, never `Double`, whose quantization is a different identity space; and every hand `Word(count)` preceding a run is precisely the count frame `Rows` writes, so the `MESH_ADJACENCY_GOLDEN` vector and the python/ts `XxHash128` peers re-verify unchanged — the vector rides `Digest(EncodeForm)` through `MeshStream`, which no member below touched. RIPPLE: the per-entity `Content` key gains the count frame `Rows` writes ahead of its word run, so `NamingHash.Addresses` and the `Rasm.Persistence` structural merge re-baseline their stored per-node keys once. Probe is the golden itself — encode the pinned mesh and compare the digest to the frozen vector; a re-baseline THERE is the defect, not the pass. NAMED LOSS: the deleted `Real` collapsed `-0.0` to `+0.0` and `Bits` writes the pattern it is handed, so a `-0.0` coordinate addresses a distinct key (escalated to `Domain/identity.md`).
- Packages: `Rasm.Meshing` `MeshSpace` with the `RhinoCommon` welded-topology read behind `MeshSpace.DuplicateNative`, `VectorCloud`, `Rasm.Domain` for the seed-zero `ContentHash.Of`, the `CanonicalWriter` framing, and the `Op`/`Context`/`IValidityEvidence` rails, `Thinktecture.Runtime.Extensions`, `Generator.Equals`, `LanguageExt.Core`.
- Growth: a new geometry modality is one `EncodeForm` case with its own frozen stream and one `CloudForm`-style wire row where it needs a discriminant; a new per-case content column is one counted layout block on the owning case's stream, the cluster mass block the precedent; a new reconciliation projection is one column on `NameAddress`; a native-brep adjacency source is one `CanonicalTopology.Of*` factory under the same canonical-order law.
- Boundary: `EncodeForm` owns three frozen canonical byte layouts — `Mesh`, `Cloud`, `Parametric` — contiguous and unpadded, non-finite values refused upstream, each framed under the `libs/contracts/manifest.json` `content-identity` framing and seed law this owner mints, and the `Mesh` stream alone produces the `MESH_ADJACENCY_GOLDEN` vector peers decode. Digests are meaningful only under their form, so every seam carries `(form, digest)`, and Persistence reads this identical mesh layout rather than a second encoding, so a drifted byte order is a caught defect. `CanonicalTopology` is immutable and compares by CONTENT — `Generator.Equals` with a stated cycle comparer, because `Arr<int[]>` would otherwise compare its rows by reference and two identical topologies would key apart — and `IsValid` claims the canonical row order and the `Self`-sequential kind partition, so a `with`-mutated or permuted record fails at the `Apply` gate before it forks one content into two keys. `EncodeForm.Of` canonicalizes knots and then claims the clamped end multiplicity a rescale cannot supply; weight-scale canonicalization of a rational carrier is the parametric producer's projection obligation.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CloudForm {
    public static readonly CloudForm Cluster = new(key: 0);
    public static readonly CloudForm Polyline = new(key: 1);
    public static readonly CloudForm Ring = new(key: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReconcileOp {
    private ReconcileOp() { }

    public sealed record Encode(EncodeForm Form) : ReconcileOp;
    public sealed record Reconcile(NameTable Prior, CanonicalTopology Rebuilt) : ReconcileOp;
    public sealed record BuildEntities(MeshSpace Space) : ReconcileOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EncodeForm : IValidityEvidence {
    private EncodeForm() { }

    public sealed record Mesh(CanonicalTopology Topology) : EncodeForm;
    public sealed record Cloud(VectorCloud Source) : EncodeForm;
    public sealed record Parametric : EncodeForm {
        internal Parametric(Arr<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet) {
            Directions = directions; Weights = weights; ControlNet = controlNet;
        }
        public Arr<Direction> Directions { get; }
        public Arr<double> Weights { get; }
        public Arr<Point3d> ControlNet { get; }
    }

    public readonly record struct Direction(int Degree, Arr<double> Knots);

    public bool IsValid => Switch(
        mesh: static m => m.Topology.IsValid,
        cloud: static c => c.Source is VectorCloud.ClusterCase cluster ? cluster.Admission.IsValid : true,
        parametric: static _ => true);

    public static EncodeForm Of(MeshSpace space) => new Mesh(CanonicalTopology.OfMesh(space));
    public static EncodeForm Of(CanonicalTopology topology) => new Mesh(topology);
    public static EncodeForm Of(VectorCloud cloud) => new Cloud(cloud);

    public static Fin<EncodeForm> Of(Arr<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return directions.Count >= 1
            ? toSeq(directions.AsIterable())
                .TraverseM(direction => Canonicalize(direction: direction, context: context, key: op)).As()
                .Bind(admitted => Admit(directions: admitted, weights: weights, controlNet: controlNet, key: op))
            : Fin.Fail<EncodeForm>(op.InvalidInput());
    }

    static Fin<Direction> Canonicalize(Direction direction, Context context, Op key) {
        Arr<double> knots = direction.Knots;
        bool shaped = direction.Degree >= 1 && knots.Count >= (2 * direction.Degree) + 2
            && knots.All(static knot => ValidityClaim.Finite(knot))
            && Enumerable.Range(1, knots.Count - 1).All(i => knots[i - 1] <= knots[i]);
        double span = shaped ? knots[^1] - knots[0] : 0.0;
        return guard(shaped && span > context.For(lane: ToleranceLane.Fraction).Value, key.InvalidInput()).ToFin()
            .Map(_ => direction with {
                Knots = new Arr<double>([.. knots.AsIterable().Select(knot => (knot - knots[0]) / span)]),
            })
            .Bind(remapped => guard(Clamped(remapped), key.InvalidInput()).ToFin().Map(_ => remapped));
    }

    static bool Clamped(Direction direction) =>
        Enumerable.Range(0, direction.Degree + 1).All(i =>
            direction.Knots[i] == 0.0 && direction.Knots[direction.Knots.Count - 1 - i] == 1.0);

    static Fin<EncodeForm> Admit(Seq<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet, Op key) {
        long controls = directions.Fold(1L, static (product, d) => unchecked(product * (d.Knots.Count - d.Degree - 1)));
        return guard(
                controlNet.Count == controls && weights.Count == controls
                    && weights.All(static w => ValidityClaim.Positive(w)) && controlNet.All(static p => ValidityClaim.Finite(p)),
                key.InvalidInput()).ToFin()
            .Map(_ => (EncodeForm)new Parametric(new Arr<Direction>([.. directions]), weights, controlNet));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct GeometryHash;

public sealed class FaceCycles : IEqualityComparer<Arr<int[]>> {
    public static readonly FaceCycles Default = new();
    public bool Equals(Arr<int[]> left, Arr<int[]> right) =>
        left.Count == right.Count && Enumerable.Range(0, left.Count).All(i => left[i].AsSpan().SequenceEqual(right[i]));
    public int GetHashCode(Arr<int[]> value) {
        HashCode hash = new();
        foreach (int[] cycle in value.AsIterable()) {
            hash.Add(cycle.Length);
            foreach (int vertex in cycle) { hash.Add(vertex); }
        }
        return hash.ToHashCode();
    }
}

[Equatable]
public sealed partial record CanonicalTopology(
    int VertexCount, Arr<(int Min, int Max)> Edges,
    [property: CustomEquality(typeof(FaceCycles))] Arr<int[]> Faces,
    Seq<RebuiltEntity> Entities) : IValidityEvidence {

    public bool IsValid => ValidityClaim.All(
        VertexCount >= 0,
        Edges.All(e => e.Min >= 0 && e.Min < e.Max && e.Max < VertexCount),
        Enumerable.Range(1, int.Max(Edges.Count - 1, 0)).All(i => Edges[i - 1].CompareTo(Edges[i]) < 0),
        Faces.All(cycle => cycle.Length >= 3 && cycle.All(v => v >= 0 && v < VertexCount)
            && Enumerable.Range(0, cycle.Length).All(i => cycle[i] != cycle[(i + 1) % cycle.Length])
            && LeastRotationIndex(cycle) == 0),
        Enumerable.Range(1, int.Max(Faces.Count - 1, 0)).All(i => Faces[i - 1].AsSpan().SequenceCompareTo(Faces[i]) <= 0),
        toSet(Edges) is var edgeSet && Faces.All(cycle =>
            Enumerable.Range(0, cycle.Length).All(i => edgeSet.Contains(Sorted(cycle[i], cycle[(i + 1) % cycle.Length])))),
        ValidityClaim.CountExactly(count: Entities.Count, expected: VertexCount + Edges.Count + Faces.Count),
        Entities.Map((entity, index) => index < VertexCount
            ? entity.Kind == EntityKind.Vertex && entity.Self == index
            : index < VertexCount + Edges.Count
                ? entity.Kind == EntityKind.Edge && entity.Self == index - VertexCount
                : entity.Kind == EntityKind.Face && entity.Self == index - VertexCount - Edges.Count).ForAll(static holds => holds));

    [BoundaryAdapter]
    public static CanonicalTopology OfMesh(MeshSpace space) {
        Mesh mesh = space.DuplicateNative();
        int vertices = mesh.TopologyVertices.Count;
        Arr<(int Min, int Max)> edges = [.. Enumerable.Range(0, mesh.TopologyEdges.Count)
            .Select(edge => mesh.TopologyEdges.GetTopologyVertices(edge))
            .Select(static pair => Sorted(pair.I, pair.J))
            .OrderBy(static edge => edge.Min).ThenBy(static edge => edge.Max)];
        Arr<int[]> faces = [.. Enumerable.Range(0, mesh.Faces.Count)
            .Select(face => Rotated(mesh.TopologyVertices.IndicesFromFace(face)))
            .Order(Comparer<int[]>.Create(static (a, b) => a.AsSpan().SequenceCompareTo(b)))];
        return new CanonicalTopology(vertices, edges, faces, Entities(vertices, edges, faces));
    }

    static int[] Rotated(int[] cycle) {
        int pivot = LeastRotationIndex(cycle);
        return pivot == 0 ? cycle : [.. cycle[pivot..], .. cycle[..pivot]];
    }

    static int LeastRotationIndex(ReadOnlySpan<int> cycle) {
        int n = cycle.Length;
        if (n == 0) { return 0; }
        (int i, int j, int k) = (0, 1, 0);
        while (i < n && j < n && k < n) {
            int order = cycle[(i + k) % n].CompareTo(cycle[(j + k) % n]);
            if (order == 0) { k++; continue; }
            if (order > 0) { i += k + 1; } else { j += k + 1; }
            if (i == j) { j++; }
            k = 0;
        }
        return Math.Min(i, j);
    }

    static Seq<RebuiltEntity> Entities(int vertices, Arr<(int Min, int Max)> edges, Arr<int[]> faces) {
        HashMap<int, Set<int>> neighbors = toSeq(edges).Fold(HashMap<int, Set<int>>.Empty, static (map, edge) => map
            .AddOrUpdate(edge.Min, ring => ring.Add(edge.Max), Set(edge.Max))
            .AddOrUpdate(edge.Max, ring => ring.Add(edge.Min), Set(edge.Min)));
        HashMap<int, int> faceDegree = toSeq(faces).Fold(HashMap<int, int>.Empty, static (map, cycle) =>
            cycle.Distinct().Aggregate(map, static (fold, vertex) => fold.AddOrUpdate(vertex, static n => n + 1, 1)));
        HashMap<(int Min, int Max), int> edgeFaces = toSeq(faces).Fold(HashMap<(int Min, int Max), int>.Empty, static (map, cycle) =>
            Enumerable.Range(0, cycle.Length).Aggregate(map, (fold, i) =>
                fold.AddOrUpdate(Sorted(cycle[i], cycle[(i + 1) % cycle.Length]), static n => n + 1, 1)));
        Set<int> Ring(int vertex) => neighbors.Find(vertex).IfNone(Set<int>.Empty);
        Seq<RebuiltEntity> vertexRows = toSeq(Enumerable.Range(0, vertices)).Map(vertex => new RebuiltEntity(
            Kind: EntityKind.Vertex, Self: vertex, Canonical: new Arr<int>([vertex]),
            IncidentVertices: new Arr<int>([.. Ring(vertex)]),
            KindHistogram: new Arr<int>([Ring(vertex).Count, Ring(vertex).Count, faceDegree.Find(vertex).IfNone(0)])));
        Seq<RebuiltEntity> edgeRows = toSeq(edges).Map((edge, self) => new RebuiltEntity(
            Kind: EntityKind.Edge, Self: self, Canonical: new Arr<int>([edge.Min, edge.Max]),
            IncidentVertices: new Arr<int>([edge.Min, edge.Max]),
            KindHistogram: new Arr<int>([2, Ring(edge.Min).Count + Ring(edge.Max).Count - 2, edgeFaces.Find((edge.Min, edge.Max)).IfNone(0)])));
        Seq<RebuiltEntity> faceRows = toSeq(faces).Map((cycle, self) => new RebuiltEntity(
            Kind: EntityKind.Face, Self: self, Canonical: new Arr<int>([.. cycle]),
            IncidentVertices: new Arr<int>([.. cycle]),
            KindHistogram: new Arr<int>([cycle.Distinct().Count(), cycle.Length,
                Enumerable.Range(0, cycle.Length).Sum(i => edgeFaces.Find(Sorted(cycle[i], cycle[(i + 1) % cycle.Length])).IfNone(1) - 1)])));
        return vertexRows + edgeRows + faceRows;
    }

    static (int Min, int Max) Sorted(int a, int b) => a <= b ? (a, b) : (b, a);
}

public readonly record struct NameAddress(TopoName Name, EntityKind Kind, GeometryHash ContentHash);

public sealed record NamingHash(GeometryHash Whole, HashMap<TopoName, NameAddress> Addresses) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Addresses.AsIterable().ForAll(static pair => pair.Key == pair.Value.Name));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReconcileAnswer : IValidityEvidence {
    private ReconcileAnswer() { }

    public sealed record Digest(GeometryHash Value) : ReconcileAnswer;
    public sealed record Reconciled(NamingHash Value) : ReconcileAnswer;
    public sealed record Topology(CanonicalTopology Value) : ReconcileAnswer;

    public bool IsValid => Switch(
        digest: static _ => true,
        reconciled: static r => r.Value.IsValid,
        topology: static t => t.Value.IsValid);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Reconciliation {
    public static Fin<ReconcileAnswer> Apply(ReconcileOp op, Op? key = null) => op.Switch(
        state: key.OrDefault(),
        encode: static (k, e) => k.AcceptInput(e.Form)
            .Map(static form => (ReconcileAnswer)new ReconcileAnswer.Digest(Digest(form)))
            .Bind(answer => k.AcceptValue(answer)),
        reconcile: static (k, r) => (k.AcceptInput(r.Prior), k.AcceptInput(r.Rebuilt))
            .Apply(static (prior, rebuilt) => (Prior: prior, Rebuilt: rebuilt)).As()
            .Bind(admitted => Addresses(admitted.Prior, admitted.Rebuilt)
                .Map(addresses => (ReconcileAnswer)new ReconcileAnswer.Reconciled(new NamingHash(Digest(EncodeForm.Of(admitted.Rebuilt)), addresses))))
            .Bind(answer => k.AcceptValue(answer)),
        buildEntities: static (k, b) => k.AcceptValue((ReconcileAnswer)new ReconcileAnswer.Topology(CanonicalTopology.OfMesh(b.Space))));

    static Fin<HashMap<TopoName, NameAddress>> Addresses(NameTable prior, CanonicalTopology rebuilt) {
        Set<UInt128> live = rebuilt.Entities.Fold(Set<UInt128>.Empty,
            static (set, entity) => set.Add(Content(kind: entity.Kind, canonical: entity.Canonical)));
        return toSeq(prior.Entries.Values)
            .Traverse(entry => Content(kind: entry.Kind, canonical: entry.Canonical) switch {
                UInt128 digest when live.Contains(digest) =>
                    Validation.Success<Error, NameAddress>(new NameAddress(entry.Name, entry.Kind, GeometryHash.Create(digest))),
                _ => Validation.Fail<Error, NameAddress>(new GeometryFault.HashMismatch(entry.Name, entry.Kind)),
            })
            .As()
            .Map(static addresses => addresses.Fold(HashMap<TopoName, NameAddress>.Empty,
                static (map, address) => map.AddOrUpdate(address.Name, address)))
            .ToFin();
    }

    static UInt128 Content(EntityKind kind, Arr<int> canonical) =>
        ContentHash.Of(state: (Kind: kind, Canonical: canonical),
            chunks: static (row, sink) => sink.Ordinal(value: row.Kind.Key)
                .Rows(rows: toSeq(row.Canonical.AsIterable()), field: static (word, field) => field.Ordinal(value: word)));

    static GeometryHash Digest(EncodeForm form) =>
        GeometryHash.Create(ContentHash.Of(state: form, chunks: static (shape, sink) => shape.Switch(
            state: sink,
            mesh: static (writer, m) => MeshStream(topology: m.Topology, sink: writer),
            cloud: static (writer, c) => CloudStream(source: c.Source, sink: writer),
            parametric: static (writer, p) => ParametricStream(form: p, sink: writer))));

    static CanonicalWriter MeshStream(CanonicalTopology topology, CanonicalWriter sink) =>
        sink.Ordinal(value: topology.VertexCount)
            .Rows(rows: toSeq(topology.Edges.AsIterable()),
                field: static (edge, field) => field.Ordinal(value: edge.Min).Ordinal(value: edge.Max))
            .Rows(rows: toSeq(topology.Faces.AsIterable()),
                field: static (cycle, field) => field.Rows(rows: toSeq(cycle), field: static (vertex, slot) => slot.Ordinal(value: vertex)));

    static CanonicalWriter CloudStream(VectorCloud source, CanonicalWriter sink) {
        (CloudForm Form, Seq<Point3d> Points, Seq<double> Mass) canonical = source.Switch(
            ringCase: static ring => (CloudForm.Ring, LeastRotation(ring.Vertices), Seq<double>.Empty),
            polylineCase: static chain => (CloudForm.Polyline, chain.Vertices, Seq<double>.Empty),
            clusterCase: static cluster => cluster.Mass.Match(
                Some: mass => Weighted(cluster.Vertices, mass),
                None: () => (CloudForm.Cluster, Lexicographic(cluster.Vertices), Seq<double>.Empty)));
        return sink.Ordinal(value: canonical.Form.Key)
            .Rows(rows: canonical.Points, field: static (point, field) => field.Bits(value: point.X).Bits(value: point.Y).Bits(value: point.Z))
            .Rows(rows: canonical.Mass, field: static (mass, field) => field.Bits(value: mass));
    }

    static (CloudForm Form, Seq<Point3d> Points, Seq<double> Mass) Weighted(Seq<Point3d> points, Arr<double> mass) {
        Seq<(Point3d Point, double Mass)> rows = toSeq(points
            .Map((point, index) => (Point: point, Mass: mass[index]))
            .OrderBy(static row => row.Point.X).ThenBy(static row => row.Point.Y)
            .ThenBy(static row => row.Point.Z).ThenBy(static row => row.Mass));
        return (CloudForm.Cluster, rows.Map(static row => row.Point), rows.Map(static row => row.Mass));
    }

    static CanonicalWriter ParametricStream(EncodeForm.Parametric form, CanonicalWriter sink) =>
        sink.Rows(rows: toSeq(form.Directions.AsIterable()), field: static (direction, field) => field
                .Ordinal(value: direction.Degree)
                .Rows(rows: toSeq(direction.Knots.AsIterable()), field: static (knot, slot) => slot.Bits(value: knot)))
            .Rows(rows: toSeq(form.Weights.AsIterable()), field: static (weight, field) => field.Bits(value: weight))
            .Rows(rows: toSeq(form.ControlNet.AsIterable()),
                field: static (point, field) => field.Bits(value: point.X).Bits(value: point.Y).Bits(value: point.Z));

    static Seq<Point3d> Lexicographic(Seq<Point3d> points) =>
        toSeq(points.OrderBy(static p => p.X).ThenBy(static p => p.Y).ThenBy(static p => p.Z));

    static Seq<Point3d> LeastRotation(Seq<Point3d> ring) {
        Point3d least = ring.Fold(ring[0], static (min, point) => Precedes(point, min) ? point : min);
        return ring.Map(static (point, index) => (Point: point, Index: index))
            .Filter(row => row.Point == least)
            .Map(row => row.Index == 0 ? ring : ring.Skip(row.Index) + ring.Take(row.Index))
            .Fold(ring, static (best, candidate) => Precedes(candidate, best) ? candidate : best);
    }

    static bool Precedes(Seq<Point3d> left, Seq<Point3d> right) =>
        Enumerable.Range(0, left.Count).Where(i => left[i] != right[i]).Select(i => Precedes(left[i], right[i])).FirstOrDefault(false);

    static bool Precedes(Point3d left, Point3d right) =>
        left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z < right.Z;
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
