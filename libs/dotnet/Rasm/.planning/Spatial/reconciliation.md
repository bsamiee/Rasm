# [RASM_TOPOLOGY_RECONCILIATION]

`Rasm.Spatial` reconciliation owns the one naming↔hash fence: the lineage-stable reference axis reconciled against the change-sensitive content axis through the single `Reconciliation.Apply` entry, over the frozen canonical byte layouts every content key hashes. `GeometryHash` and `TopoName` are type-distinct, so a cross-axis compare is a compile error, and the canonical bytes cross only the in-process boundary, never sitting between wire and result.

## [01]-[INDEX]

- [02]-[RECONCILIATION_BRIDGE]: `Reconciliation.Apply` folds one `ReconcileOp` into `GeometryHash` content keys and the `NamingHash` over the frozen canonical byte streams.

## [02]-[RECONCILIATION_BRIDGE]

- Owner: `GeometryHash` mints the content-axis identity only through the kernel `ContentHash.Of`, framing every field through the kernel `CanonicalWriter` rather than a page-local byte emitter; `CanonicalTopology` mints the immutable adjacency every encode and re-anchor reads.
- Cases: each `EncodeForm` stream freezes its own canonical order — a `ClusterCase` sorts vertices lexicographically and hashes any mass column as content, a `PolylineCase` stores order as content, a `RingCase` rotates to its least rotation with winding preserved under the rotation law the mesh face cycles carry, and `Parametric` takes the direction count as the curve/surface/volume generator. Each cloud stream leads with the frozen wire ordinal its generated `VectorCloud` arm names — cluster `0`, polyline `1`, ring `2` — so the discriminant sits on the exhaustive dispatch that owns the wire. `CanonicalTopology.OfMesh` is the one native admission.
- Entry: `EncodeForm.Of` discriminates admission on input shape, its raw-array parametric head the one validated ingress; that head CANONICALIZES each direction's knots onto `[0,1]` before it gates, so a producer's divide-normalized vector admits instead of forking identical geometry out of the corpus, and a refusal routes an typed admission fault rather than throwing.
- Auto: `Mesh` encoding re-hashes identically under a morph and distinctly under a topology break; every arm gates input and answer through the acceptance oracle, so consumers never re-check the `IValidityEvidence` claims.
- Law: `NamingHash` is the reconciliation evidence the Persistence structural merge consumes per node, registering into the `Acceptance.ValidityOf` oracle like every kernel result — no parallel reconciliation ledger. Its `IsValid` claims the one law an instance can witness alone: `Addresses` is a BIJECTION, the map key holding name→content and the claim holding content→name, so a content key two names address refuses at `AcceptValue` — the survived-plus-born twin of the `NameCollision` the naming owner refuses on the born path. Every other construction fact — each content key present in the rebuilt roster, one row per prior entry, `Whole` the rebuilt digest — is proved by the reconcile arm and never restated as a claim.
- Law: `RebuiltEntity.Canonical` is a run of int WORDS, never a pre-serialized block, so the framing decision stays at `CanonicalWriter` and the entity keeps structural equality — a `byte[]` column compared by reference, which is precisely what a record whose identity IS its content cannot afford. Three `EncodeForm` streams stay BYTE-UNCHANGED under the writer, and the correspondence is member-for-member: `Word` was int32-LE and `Ordinal` is; `Real` wrote the raw IEEE754 pattern little-endian and `Bits` writes exactly that pattern, never `Double`, whose quantization is a different identity space; and every hand `Word(count)` preceding a run is precisely the count frame `Rows` writes, so the mesh-adjacency digest and the python/ts `XxHash128` peers agree unchanged — the digest rides `Digest(EncodeForm)`'s mesh arm, which no member below touched. RIPPLE: the per-entity `Content` key gains the count frame `Rows` writes ahead of its word run, so `NamingHash.Addresses` and the `Rasm.Persistence` structural merge re-baseline their stored per-node keys once. Cross-runtime agreement is the proof — one mesh encoded through the peer writers digests identically, so a divergence names the member that broke framing rather than a baseline anyone may move. NAMED LOSS: the deleted `Real` collapsed `-0.0` to `+0.0` and `Bits` writes the pattern it is handed, so a `-0.0` coordinate addresses a distinct key (escalated to `Domain/identity.md`).
- Packages: `Rasm.Meshing` `MeshSpace` with the `RhinoCommon` welded-topology read behind `MeshSpace.DuplicateNative`, `VectorCloud`, `Rasm.Domain` for the seed-zero `ContentHash.Of`, the `CanonicalWriter` framing, and the `IValidityEvidence` types, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`.
- Growth: a new geometry modality is one `EncodeForm` case with its own frozen stream, its wire ordinal named on the dispatch arm where it needs a discriminant; a new per-case content column is one counted layout block on the owning case's stream, the cluster mass block the precedent; a new reconciliation projection is one column on the `NamingHash.Addresses` value tuple; a native-brep adjacency source is one `CanonicalTopology.Of*` factory under the same canonical-order law.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReconcileOp {
    private ReconcileOp() { }

    public sealed record Encode(EncodeForm Form) : ReconcileOp;
    public sealed record Reconcile(NameTable Prior, CanonicalTopology Rebuilt) : ReconcileOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EncodeForm : IValidityEvidence {
    private EncodeForm() { }

    public sealed record Mesh(CanonicalTopology Topology) : EncodeForm;
    public sealed record Cloud(VectorCloud Source) : EncodeForm;
    public sealed record Parametric : EncodeForm {
        internal Parametric(Arr<(int Degree, Arr<double> Knots)> directions,
            Arr<double> weights, Arr<Point3d> controlNet) {
            Directions = directions; Weights = weights; ControlNet = controlNet;
        }
        public Arr<(int Degree, Arr<double> Knots)> Directions { get; }
        public Arr<double> Weights { get; }
        public Arr<Point3d> ControlNet { get; }
    }

    public bool IsValid => Switch(
        mesh: static m => m.Topology.IsValid,
        cloud: static c => c.Source.Switch(
            ringCase: static _ => true,
            polylineCase: static _ => true,
            clusterCase: static cluster => cluster.Admission.IsValid),
        parametric: static _ => true);

    public static EncodeForm Of(MeshSpace space) => new Mesh(CanonicalTopology.OfMesh(space));
    public static EncodeForm Of(VectorCloud cloud) => new Cloud(cloud);

    public static Fin<EncodeForm> Of(Arr<(int Degree, Arr<double> Knots)> directions,
        Arr<double> weights, Arr<Point3d> controlNet) {
        return directions.Count is >= 1 and <= 3
            ? toSeq(directions.AsIterable()).TraverseM(direction => {
                (int degree, Arr<double> knots) = direction;
                bool shaped = degree >= 1 && degree <= (knots.Count - 2) / 2
                    && knots.All(static knot => ValidityClaim.Finite(knot))
                    && Enumerable.Range(1, knots.Count - 1).All(i => knots[i - 1] <= knots[i]);
                double span = shaped ? knots[^1] - knots[0] : 0.0;
                return guard(shaped && ValidityClaim.Positive(span), new KernelFault.InvalidInput()).ToFin()
                    .Map(_ => new Arr<double>([.. knots.AsIterable().Select(knot => (knot - knots[0]) / span)]))
                    .Bind(normalized => guard(Enumerable.Range(0, degree + 1).All(i =>
                            normalized[i] == 0.0 && normalized[normalized.Count - 1 - i] == 1.0), new KernelFault.InvalidInput()).ToFin()
                        .Map(_ => (Degree: degree, Knots: normalized)));
            }).As().Bind(admitted => {
                long controls = admitted.Fold(1L, (product, direction) => {
                    int extent = direction.Knots.Count - direction.Degree - 1;
                    return product > controlNet.Count / extent ? controlNet.Count + 1L : product * extent;
                });
                return guard(controls == controlNet.Count && weights.Count == controlNet.Count
                        && weights.All(static w => ValidityClaim.Positive(w)) && controlNet.All(static p => ValidityClaim.Finite(p)),
                    new KernelFault.InvalidInput()).ToFin()
                    .Map(_ => (EncodeForm)new Parametric(
                        new Arr<(int Degree, Arr<double> Knots)>([.. admitted]), weights, controlNet));
            }) : Fin.Fail<EncodeForm>(new KernelFault.InvalidInput());
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<UInt128>]
public readonly partial struct GeometryHash;

public sealed record CanonicalTopology(
    int VertexCount, Arr<(int Min, int Max)> Edges,
    Arr<Seq<int>> Faces, Seq<RebuiltEntity> Entities) : IValidityEvidence {

    public bool IsValid => ValidityClaim.All(
        VertexCount >= 0,
        Edges.All(e => e.Min >= 0 && e.Min < e.Max && e.Max < VertexCount),
        Enumerable.Range(1, int.Max(Edges.Count - 1, 0)).All(i => Edges[i - 1].CompareTo(Edges[i]) < 0),
        Faces.All(cycle => cycle.Count >= 3 && cycle.All(vertex => vertex >= 0 && vertex < VertexCount)
            && Enumerable.Range(0, cycle.Count).All(i => cycle[i] != cycle[(i + 1) % cycle.Count])
            && LeastRotationIndex(cycle.AsSpan()) == 0),
        Enumerable.Range(1, int.Max(Faces.Count - 1, 0)).All(i =>
            Faces[i - 1].AsSpan().SequenceCompareTo(Faces[i].AsSpan()) <= 0),
        toSet(Edges) is var edgeSet && Faces.All(cycle =>
            Enumerable.Range(0, cycle.Count).All(i => edgeSet.Contains(Sorted(cycle[i], cycle[(i + 1) % cycle.Count])))),
        ValidityClaim.CountExactly(count: Entities.Count, expected: VertexCount + Edges.Count + Faces.Count),
        Entities.Map((entity, index) => index < VertexCount
            ? entity.Kind == EntityKind.Vertex && entity.Self == index
            : index < VertexCount + Edges.Count
                ? entity.Kind == EntityKind.Edge && entity.Self == index - VertexCount
                : entity.Kind == EntityKind.Face && entity.Self == index - VertexCount - Edges.Count).ForAll(static holds => holds));

    public static CanonicalTopology OfMesh(MeshSpace space) {
        using Mesh mesh = space.DuplicateNative();
        int vertices = mesh.TopologyVertices.Count;
        Arr<(int Min, int Max)> edges = [.. Enumerable.Range(0, mesh.TopologyEdges.Count)
            .Select(edge => mesh.TopologyEdges.GetTopologyVertices(edge))
            .Select(static pair => Sorted(pair.I, pair.J))
            .OrderBy(static edge => edge.Min).ThenBy(static edge => edge.Max)];
        Arr<Seq<int>> faces = [.. Enumerable.Range(0, mesh.Faces.Count)
            .Select(face => Rotated(mesh.TopologyVertices.IndicesFromFace(face)))
            .Order(Comparer<Seq<int>>.Create(static (a, b) => a.AsSpan().SequenceCompareTo(b.AsSpan())))];
        return new CanonicalTopology(vertices, edges, faces, Entities(vertices, edges, faces));

        static Seq<int> Rotated(int[] cycle) {
            int pivot = LeastRotationIndex(cycle);
            return pivot == 0 ? toSeq(cycle) : toSeq([.. cycle[pivot..], .. cycle[..pivot]]);
        }

        static Seq<RebuiltEntity> Entities(int vertices, Arr<(int Min, int Max)> edges, Arr<Seq<int>> faces) {
            HashMap<int, Set<int>> neighbors = toSeq(edges).Fold(HashMap<int, Set<int>>(), static (map, edge) => map
                .AddOrUpdate(edge.Min, ring => ring.Add(edge.Max), Set(edge.Max))
                .AddOrUpdate(edge.Max, ring => ring.Add(edge.Min), Set(edge.Min)));
            HashMap<int, int> faceDegree = toSeq(faces).Fold(HashMap<int, int>(), static (map, cycle) =>
                cycle.Distinct().Aggregate(map, static (fold, vertex) => fold.AddOrUpdate(vertex, static n => n + 1, 1)));
            HashMap<(int Min, int Max), int> edgeFaces = toSeq(faces).Fold(HashMap<(int Min, int Max), int>(), static (map, cycle) =>
                Enumerable.Range(0, cycle.Count).Aggregate(map, (fold, i) =>
                    fold.AddOrUpdate(Sorted(cycle[i], cycle[(i + 1) % cycle.Count]), static n => n + 1, 1)));
            Set<int> Ring(int vertex) => neighbors.Find(vertex).IfNone(Set<int>());
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
                KindHistogram: new Arr<int>([cycle.Distinct().Count, cycle.Count,
                    Enumerable.Range(0, cycle.Count).Sum(i => edgeFaces.Find(Sorted(cycle[i], cycle[(i + 1) % cycle.Count])).IfNone(1) - 1)])));
            return vertexRows + edgeRows + faceRows;
        }
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

    static (int Min, int Max) Sorted(int a, int b) => a <= b ? (a, b) : (b, a);
}

public sealed record NamingHash(GeometryHash Whole,
    HashMap<TopoName, (EntityKind Kind, GeometryHash ContentHash)> Addresses) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountExactly(
            count: toSet(Addresses.Values.Map(static address => address.ContentHash)).Count,
            expected: Addresses.Count));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReconcileAnswer : IValidityEvidence {
    private ReconcileAnswer() { }

    public sealed record Digest(GeometryHash Value) : ReconcileAnswer;
    public sealed record Reconciled(NamingHash Value) : ReconcileAnswer;

    public bool IsValid => Switch(
        digest: static _ => true,
        reconciled: static r => r.Value.IsValid);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Reconciliation {
    public static Fin<ReconcileAnswer> Apply(ReconcileOp op) => op.Switch(
        encode: static e => Acceptance.Input(e.Form)
            .Map(static form => (ReconcileAnswer)new ReconcileAnswer.Digest(Digest(form)))
            .Bind(answer => Acceptance.Value(answer)),
        reconcile: static r => (Acceptance.Input(r.Prior), Acceptance.Input(r.Rebuilt))
            .Apply(static (prior, rebuilt) => (Prior: prior, Rebuilt: rebuilt)).As()
            .Bind(admitted => {
                Set<GeometryHash> live = admitted.Rebuilt.Entities.Fold(Set<GeometryHash>(),
                    static (set, entity) => set.TryAdd(Content(entity.Kind, entity.Canonical)));
                return toSeq(admitted.Prior.Entries.Values).Traverse(entry => {
                    GeometryHash digest = Content(entry.Kind, entry.Canonical);
                    return live.Contains(digest)
                        ? Validation.Success<Error, (TopoName Name, EntityKind Kind, GeometryHash ContentHash)>(
                            (entry.Name, entry.Kind, digest))
                        : Validation.Fail<Error, (TopoName Name, EntityKind Kind, GeometryHash ContentHash)>(
                            new GeometryFault.TopologyContentMissing(entry.Name, entry.Kind));
                }).As().Map(rows => (ReconcileAnswer)new ReconcileAnswer.Reconciled(new NamingHash(
                    Digest(new EncodeForm.Mesh(admitted.Rebuilt)),
                    rows.Fold(HashMap<TopoName, (EntityKind Kind, GeometryHash ContentHash)>(),
                        static (map, row) => map.AddOrUpdate(row.Name, (row.Kind, row.ContentHash)))))).ToFin();
            })
            .Bind(answer => Acceptance.Value(answer)));

    static GeometryHash Content(EntityKind kind, Arr<int> canonical) =>
        GeometryHash.Create(ContentHash.Of(state: (Kind: kind, Canonical: canonical),
            chunks: static (row, sink) => sink.Ordinal(value: row.Kind.Key)
                .Rows(rows: toSeq(row.Canonical.AsIterable()), field: static (word, field) => field.Ordinal(value: word))));

    static GeometryHash Digest(EncodeForm form) =>
        GeometryHash.Create(ContentHash.Of(state: form, chunks: static (shape, sink) => shape.Switch(
            state: sink,
            mesh: static (writer, m) => writer.Ordinal(m.Topology.VertexCount)
                .Rows(toSeq(m.Topology.Edges.AsIterable()),
                    static (edge, field) => field.Ordinal(edge.Min).Ordinal(edge.Max))
                .Rows(toSeq(m.Topology.Faces.AsIterable()),
                    static (cycle, field) => field.Rows(cycle, static (vertex, slot) => slot.Ordinal(vertex))),
            cloud: static (writer, c) => {
                (int Ordinal, Seq<Point3d> Points, Seq<double> Mass) canonical = c.Source.Switch(
                    ringCase: static ring => (2, LeastRotation(ring.Vertices), Seq<double>()),
                    polylineCase: static chain => (1, chain.Vertices, Seq<double>()),
                    clusterCase: static cluster => cluster.Mass.Match(
                        Some: mass => {
                            Seq<(Point3d Point, double Mass)> rows = toSeq(cluster.Vertices
                                .Map((point, index) => (Point: point, Mass: mass[index]))
                                .OrderBy(static row => row.Point.X).ThenBy(static row => row.Point.Y)
                                .ThenBy(static row => row.Point.Z).ThenBy(static row => row.Mass));
                            return (0, rows.Map(static row => row.Point), rows.Map(static row => row.Mass));
                        },
                        None: () => (0, toSeq(cluster.Vertices.OrderBy(static point => point.X)
                            .ThenBy(static point => point.Y).ThenBy(static point => point.Z)), Seq<double>())));
                CanonicalWriter result = writer.Ordinal(canonical.Ordinal)
                    .Rows(canonical.Points, static (point, field) => field.Bits(point.X).Bits(point.Y).Bits(point.Z))
                    .Rows(canonical.Mass, static (mass, field) => field.Bits(mass));

                static Seq<Point3d> LeastRotation(Seq<Point3d> ring) {
                    Point3d least = ring.Fold(ring[0], static (min, point) =>
                        (point.X, point.Y, point.Z).CompareTo((min.X, min.Y, min.Z)) < 0 ? point : min);
                    return ring.Map(static (point, index) => (Point: point, Index: index))
                        .Filter(row => row.Point == least)
                        .Map(row => row.Index == 0 ? ring : ring.Skip(row.Index) + ring.Take(row.Index))
                        .Fold(ring, static (best, candidate) => Enumerable.Range(0, candidate.Count)
                            .Where(i => candidate[i] != best[i])
                            .Select(i => (candidate[i].X, candidate[i].Y, candidate[i].Z)
                                .CompareTo((best[i].X, best[i].Y, best[i].Z))).FirstOrDefault() < 0 ? candidate : best);
                }
                return result;
            },
            parametric: static (writer, p) => writer.Rows(toSeq(p.Directions.AsIterable()),
                    static (direction, field) => field.Ordinal(direction.Degree)
                        .Rows(toSeq(direction.Knots.AsIterable()), static (knot, slot) => slot.Bits(knot)))
                .Rows(toSeq(p.Weights.AsIterable()), static (weight, field) => field.Bits(weight))
                .Rows(toSeq(p.ControlNet.AsIterable()), static (point, field) =>
                    field.Bits(point.X).Bits(point.Y).Bits(point.Z)))));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
