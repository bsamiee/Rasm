# [RASM_TOPOLOGY_RECONCILIATION]

`Rasm.Spatial` reconciliation owns the one naming↔hash fence: the lineage-stable reference axis reconciled against the change-sensitive content axis through the single `Reconciliation.Apply` entry, over the frozen canonical byte layouts every content key hashes. `GeometryHash` and `TopoName` are type-distinct, so a cross-axis compare is a compile error, and the canonical bytes cross only the in-process seam, never sitting between wire and rail.

## [01]-[INDEX]

- [02]-[RECONCILIATION_BRIDGE]: `Reconciliation.Apply` folds one `ReconcileOp` into `GeometryHash` content keys and the `NamingHash` receipt over the frozen canonical byte streams.

## [02]-[RECONCILIATION_BRIDGE]

- Owner: `GeometryHash` mints the content-axis identity only through the kernel `ContentHash.Of`; `CanonicalTopology` mints the immutable adjacency every encode, re-anchor, and entity build reads.
- Cases: each `EncodeForm` stream freezes its own canonical order — a `ClusterCase` sorts vertices lexicographically and hashes any mass column as content, a `PolylineCase` stores order as content, a `RingCase` rotates to its least rotation with winding preserved under the rotation law the mesh face cycles carry, and `Parametric` takes the direction count as the curve/surface/volume generator. `CanonicalTopology.OfMesh` is the one native admission.
- Entry: `EncodeForm.Of` discriminates admission on input shape, its raw-array parametric head the one validated ingress; a refusal routes an `Op`-keyed admission fault rather than throwing.
- Auto: `Mesh` encoding re-hashes identically under a morph and distinctly under a topology break; every arm gates input and answer through the acceptance oracle, so consumers never re-check the `IValidityEvidence` claims.
- Receipt: `NamingHash` is the reconciliation evidence the Persistence structural merge consumes per node, registering into the `OpAcceptance.ValidityOf` oracle like every kernel receipt — no parallel reconciliation ledger.
- Packages: `Rasm.Meshing` `MeshSpace` with the `RhinoCommon` welded-topology read behind `MeshSpace.DuplicateNative`, `VectorCloud`, `Rasm.Domain` for the seed-zero `ContentHash.Of` and the `Op`/`IValidityEvidence` rails, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`, `System.Buffers.Binary`.
- Growth: a new geometry modality is one `EncodeForm` case with its own frozen stream; a new per-case content column is one counted layout block on the owning case's stream, the cluster mass block the precedent; a new reconciliation projection is one column on `NameAddress`; a native-brep adjacency source is one `CanonicalTopology.Of*` factory under the same canonical-order law.
- Boundary: `EncodeForm` owns three frozen canonical byte layouts — `Mesh`, `Cloud`, `Parametric` — contiguous and unpadded, non-finite values refused upstream, the `Mesh` stream pinned as the `CANONICAL_BYTE_IDENTITY` fixture. A digest is meaningful only under its form, so every seam carries `(form, digest)`, and Persistence reads this identical mesh layout rather than a second encoding, so a drifted byte order is a caught defect. `CanonicalTopology` is immutable, so its bytes are referentially transparent, and `IsValid` claims the canonical row order and the `Self`-sequential kind partition — a `with`-mutated or permuted record fails at the `Apply` gate before it forks one content into two keys. `EncodeForm.Of` validates knot normalization; weight-scale canonicalization of a rational carrier is the parametric producer's projection obligation.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using Rasm.Csp;
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

// --- [TYPES] ----------------------------------------------------------------------------------------
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
        cloud: static _ => true,
        parametric: static _ => true);

    public static EncodeForm Of(MeshSpace space) => new Mesh(CanonicalTopology.OfMesh(space));
    public static EncodeForm Of(CanonicalTopology topology) => new Mesh(topology);
    public static EncodeForm Of(VectorCloud cloud) => new Cloud(cloud);

    // controls computes only under the normalized gate (every factor >= 2), so a hostile pair never traps the checked context — refusal stays railed.
    public static Fin<EncodeForm> Of(Arr<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet, Op? key = null) {
        bool normalized = directions.Count >= 1 && directions.All(static d => Normalized(d.Degree, d.Knots));
        long controls = normalized ? directions.Fold(1L, static (product, d) => unchecked(product * (d.Knots.Count - d.Degree - 1))) : 0L;
        return guard(
                normalized && controlNet.Count == controls && weights.Count == controls
                    && weights.All(static w => ValidityClaim.Positive(w)) && controlNet.All(static p => ValidityClaim.Finite(p)),
                key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (EncodeForm)new Parametric(directions, weights, controlNet));
    }

    // One curve, one content key — a denormalized knot vector forks identical geometry into two digests.
    static bool Normalized(int degree, Arr<double> knots) =>
        degree >= 1 && knots.Count >= (2 * degree) + 2
        && Enumerable.Range(0, degree + 1).All(i => knots[i] == 0.0 && knots[knots.Count - 1 - i] == 1.0)
        && Enumerable.Range(1, knots.Count - 1).All(i => knots[i - 1] <= knots[i])
        && knots.All(static k => ValidityClaim.Finite(k));
}

// --- [MODELS] ---------------------------------------------------------------------------------------
[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct GeometryHash;

public sealed record CanonicalTopology(
    int VertexCount, Arr<(int Min, int Max)> Edges, Arr<int[]> Faces, Seq<RebuiltEntity> Entities) : IValidityEvidence {

    // Canonical row order is claimed, never assumed — a permuted hand-built adjacency forks identical content into two keys.
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(VertexCount >= 0),
        ValidityClaim.Of(Edges.All(e => e.Min >= 0 && e.Min < e.Max && e.Max < VertexCount)),
        ValidityClaim.Of(Enumerable.Range(1, int.Max(Edges.Count - 1, 0)).All(i => Edges[i - 1].CompareTo(Edges[i]) < 0)),
        ValidityClaim.Of(Faces.All(cycle => cycle.Length >= 3 && cycle.All(v => v >= 0 && v < VertexCount)
            && Enumerable.Range(0, cycle.Length).All(i => cycle[i] != cycle[(i + 1) % cycle.Length])
            && cycle.AsSpan().SequenceEqual(Rotated(cycle)))),
        ValidityClaim.Of(Enumerable.Range(1, int.Max(Faces.Count - 1, 0)).All(i => Faces[i - 1].AsSpan().SequenceCompareTo(Faces[i]) <= 0)),
        ValidityClaim.Of(toSet(Edges) is var edgeSet && Faces.All(cycle =>
            Enumerable.Range(0, cycle.Length).All(i => edgeSet.Contains(Sorted(cycle[i], cycle[(i + 1) % cycle.Length]))))),
        ValidityClaim.CountExactly(count: Entities.Count, expected: VertexCount + Edges.Count + Faces.Count),
        ValidityClaim.Of(Entities.Map((entity, index) => index < VertexCount
            ? entity.Kind == EntityKind.Vertex && entity.Self == index
            : index < VertexCount + Edges.Count
                ? entity.Kind == EntityKind.Edge && entity.Self == index - VertexCount
                : entity.Kind == EntityKind.Face && entity.Self == index - VertexCount - Edges.Count).ForAll(static holds => holds)));

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

    // Least ROTATION, not first-min pivot: a duplicated minimum resolves by whole-cycle compare, and a host-refused [] cycle rides through to the oracle.
    static int[] Rotated(int[] cycle) {
        if (cycle.Length == 0) { return cycle; }
        int least = cycle.Min();
        return Enumerable.Range(0, cycle.Length)
            .Where(pivot => cycle[pivot] == least)
            .Select(pivot => (int[])[.. cycle[pivot..], .. cycle[..pivot]])
            .Aggregate(static (best, next) => next.AsSpan().SequenceCompareTo(best) < 0 ? next : best);
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
            Kind: EntityKind.Vertex, Self: vertex, CanonicalBytes: Bytes(vertex),
            IncidentVertices: [.. Ring(vertex)],
            KindHistogram: [Ring(vertex).Count, Ring(vertex).Count, faceDegree.Find(vertex).IfNone(0)]));
        Seq<RebuiltEntity> edgeRows = toSeq(edges).Map((edge, self) => new RebuiltEntity(
            Kind: EntityKind.Edge, Self: self, CanonicalBytes: Bytes(edge.Min, edge.Max),
            IncidentVertices: [edge.Min, edge.Max],
            KindHistogram: [2, Ring(edge.Min).Count + Ring(edge.Max).Count - 2, edgeFaces.Find((edge.Min, edge.Max)).IfNone(0)]));
        Seq<RebuiltEntity> faceRows = toSeq(faces).Map((cycle, self) => new RebuiltEntity(
            Kind: EntityKind.Face, Self: self, CanonicalBytes: Bytes(cycle),
            IncidentVertices: cycle,
            KindHistogram: [cycle.Distinct().Count(), cycle.Length,
                Enumerable.Range(0, cycle.Length).Sum(i => edgeFaces.Find(Sorted(cycle[i], cycle[(i + 1) % cycle.Length])).IfNone(1) - 1)]));
        return vertexRows + edgeRows + faceRows;
    }

    static (int Min, int Max) Sorted(int a, int b) => a <= b ? (a, b) : (b, a);

    static byte[] Bytes(params ReadOnlySpan<int> values) {
        ArrayBufferWriter<byte> stream = new(values.Length * 4);
        foreach (int value in values) stream.Word(value);       // Exemption: byte-emitter kernel
        return stream.WrittenSpan.ToArray();
    }
}

public readonly record struct NameAddress(TopoName Name, EntityKind Kind, GeometryHash ContentHash);

public sealed record NamingHash(GeometryHash Whole, HashMap<TopoName, NameAddress> Addresses) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Addresses.AsIterable().ForAll(static pair => pair.Key == pair.Value.Name)));
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

// --- [OPERATIONS] -----------------------------------------------------------------------------------
// Byte-emitter kernel (named exemption): little-endian words, -0.0 normalized to +0.0.
internal static class CanonicalStream {
    extension(ArrayBufferWriter<byte> stream) {
        public void Word(int value) { BinaryPrimitives.WriteInt32LittleEndian(stream.GetSpan(4)[..4], value); stream.Advance(4); }
        public void Real(double value) { BinaryPrimitives.WriteDoubleLittleEndian(stream.GetSpan(8)[..8], value == 0.0 ? 0.0 : value); stream.Advance(8); }
        public void Coordinate(Point3d point) { stream.Real(point.X); stream.Real(point.Y); stream.Real(point.Z); }
    }
}

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

    // Applicative traverse over the REBUILT address set: every dangling name accumulates before .ToFin() rejoins the rail.
    static Fin<HashMap<TopoName, NameAddress>> Addresses(NameTable prior, CanonicalTopology rebuilt) {
        Set<UInt128> live = rebuilt.Entities.Fold(Set<UInt128>.Empty, static (set, entity) => set.Add(ContentHash.Of(entity.CanonicalBytes)));
        return toSeq(prior.Entries.Values)
            .Traverse(entry => ContentHash.Of(entry.CanonicalBytes) switch {
                UInt128 digest when live.Contains(digest) =>
                    Validation.Success<Error, NameAddress>(new NameAddress(entry.Name, entry.Kind, GeometryHash.Create(digest))),
                _ => Validation.Fail<Error, NameAddress>(new GeometryFault.HashMismatch(entry.Name.Value, entry.Kind.Key).ToError()),
            })
            .As()
            .Map(static addresses => addresses.Fold(HashMap<TopoName, NameAddress>.Empty,
                static (map, address) => map.AddOrUpdate(address.Name, address)))
            .ToFin();
    }

    static GeometryHash Digest(EncodeForm form) => GeometryHash.Create(ContentHash.Of(Stream(form).WrittenSpan));

    static ArrayBufferWriter<byte> Stream(EncodeForm form) => form.Switch(
        mesh: static m => MeshStream(m.Topology),
        cloud: static c => CloudStream(c.Source),
        parametric: static p => ParametricStream(p));

    // Field order is the persisted decode contract the CANONICAL_BYTE_IDENTITY fixture pins.
    static ArrayBufferWriter<byte> MeshStream(CanonicalTopology topology) {
        ArrayBufferWriter<byte> stream = new(12 + (topology.Edges.Count * 8) + topology.Faces.Sum(static cycle => 4 + (cycle.Length * 4)));
        stream.Word(topology.VertexCount);
        stream.Word(topology.Edges.Count);
        foreach ((int min, int max) in topology.Edges) { stream.Word(min); stream.Word(max); }
        stream.Word(topology.Faces.Count);
        foreach (int[] cycle in topology.Faces) { stream.Word(cycle.Length); foreach (int vertex in cycle) stream.Word(vertex); }
        return stream;
    }

    static ArrayBufferWriter<byte> CloudStream(VectorCloud source) {
        (int Kind, Seq<Point3d> Points, Seq<double> Mass) canonical = source.Switch(
            ringCase: static ring => (2, LeastRotation(ring.Vertices), Seq<double>.Empty),
            polylineCase: static chain => (1, chain.Vertices, Seq<double>.Empty),
            clusterCase: static cluster => cluster.Mass.Match(
                Some: mass => Weighted(cluster.Vertices, mass),
                None: () => (0, Lexicographic(cluster.Vertices), Seq<double>.Empty)));
        ArrayBufferWriter<byte> stream = new(12 + (canonical.Points.Count * 24) + (canonical.Mass.Count * 8));
        stream.Word(canonical.Kind);
        stream.Word(canonical.Points.Count);
        foreach (Point3d point in canonical.Points) stream.Coordinate(point);
        stream.Word(canonical.Mass.Count);          // 0 = unweighted; weighted column rides the canonical order
        foreach (double mass in canonical.Mass) stream.Real(mass);
        return stream;
    }

    // Mass IS content: each mass rides its vertex through the sort as the final tiebreak, so divergent weights never share a digest.
    static (int Kind, Seq<Point3d> Points, Seq<double> Mass) Weighted(Seq<Point3d> points, Arr<double> mass) {
        Seq<(Point3d Point, double Mass)> rows = toSeq(points
            .Map((point, index) => (Point: point, Mass: mass[index]))
            .OrderBy(static row => row.Point.X).ThenBy(static row => row.Point.Y)
            .ThenBy(static row => row.Point.Z).ThenBy(static row => row.Mass));
        return (0, rows.Map(static row => row.Point), rows.Map(static row => row.Mass));
    }

    static ArrayBufferWriter<byte> ParametricStream(EncodeForm.Parametric form) {
        ArrayBufferWriter<byte> stream = new(
            12 + form.Directions.Sum(static d => 8 + (d.Knots.Count * 8)) + (form.Weights.Count * 8) + (form.ControlNet.Count * 24));
        stream.Word(form.Directions.Count);
        foreach (EncodeForm.Direction direction in form.Directions) {
            stream.Word(direction.Degree);
            stream.Word(direction.Knots.Count);
            foreach (double knot in direction.Knots) stream.Real(knot);
        }
        stream.Word(form.Weights.Count);
        foreach (double weight in form.Weights) stream.Real(weight);
        stream.Word(form.ControlNet.Count);
        foreach (Point3d point in form.ControlNet) stream.Coordinate(point);
        return stream;
    }

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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
