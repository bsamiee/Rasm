# [RASM_TOPOLOGY_RECONCILIATION]

The one naming↔hash reconciliation fence (`Rasm.Geometry.Naming`, fault cluster `naming` 2404-2407 shared with `naming.md`). One entry owns every modality: `Fin<ReconcileAnswer> Reconciliation.Apply(ReconcileOp, Op? key = null)` dispatches `Encode(EncodeForm)` · `Reconcile(prior, rebuilt)` · `BuildEntities(MeshSpace)` through a total generated `Switch`. The page owns `GeometryHash` — the `[ValueObject<UInt128>]` content axis, type-distinct from the `TopoName` reference axis; `CanonicalTopology` — the immutable hash-friendly adjacency record built once from the native `Mesh` welded topology under an ORACLE-ENFORCED canonical order (lexicographic edge pairs, least-rotation face cycles in lexicographic order — claimed by `IsValid`, not produced by convention); `EncodeForm` — the closed encode-modality family {`Mesh` · `Cloud` · `Parametric`} whose every case emits its own frozen little-endian stream; and `NamingHash` — the reconciliation receipt projecting every `TopoName` onto the `GeometryHash` of its current canonical entity bytes. The geometry domain mints NO second identity: every case hashes its EXACT canonical bytes through the kernel `Rasm.Domain` `ContentHash.Of` seed-zero entry, and the cross-package seam is ONE anchor — `Rasm.Persistence/Query/topology` (`[CONTENT_KEY]`: the adjacency-derived `GeometryHash` the federation and structural diff read, never re-mint).

The canonical bytes cross only the in-process seam; `CanonicalTopology`, `EncodeForm`, and the `NameAddress` records are interior types that never sit between wire and rail. `Reconcile` keeps the reference axis (`TopoName`, lineage-stable) and the content axis (`GeometryHash`, change-sensitive) orthogonal — two identities reconciled by one fence, never collapsed, and the type distinction makes a cross-axis comparison a compile error rather than a prose rule. The page composes `Rasm.Vectors` `MeshSpace`/`VectorCloud` and the native `Mesh` topology surface as settled vocabulary — read, never re-mint — and feeds the `naming.md` `Track` fold the per-entity `CanonicalTopology` it re-anchors against: this page is the entity-bridge PRODUCER. It also hosts the `ONE_WIRE_FIXTURE_CORPUS` index — the content-addressed golden-fixture corpus every cross-language parity harness reads — because the geometry domain owns its sole host-derived REAL byte fixture (the `CANONICAL_BYTE_IDENTITY` stream and digest).

## [01]-[INDEX]

- [02]-[RECONCILIATION_BRIDGE]: `Reconciliation.Apply` ONE entry over `ReconcileOp`; `EncodeForm` {`Mesh` · `Cloud` · `Parametric`} frozen byte streams; `GeometryHash` content axis; `NamingHash` receipt.
- [03]-[ONE_WIRE_FIXTURE_CORPUS]: The content-addressed golden-fixture index every cross-language parity harness reads; the `CANONICAL_BYTE_IDENTITY` stream+digest is its sole host-derived REAL byte fixture.

## [02]-[RECONCILIATION_BRIDGE]

- Owner: `GeometryHash` `[ValueObject<UInt128>]` the content-axis identity every consumer reads — the same value the Persistence structural diff consumes per node, minted ONLY through the kernel `ContentHash.Of`; `CanonicalTopology` the immutable adjacency record family (`VertexCount` · sorted `(Min, Max)` edge pairs in lexicographic order · least-rotation face cycles in lexicographic order · the `Self`-sequential `RebuiltEntity` seq) — `IValidityEvidence` whose claim set enforces the canonical order itself, so the acceptance oracle refuses BOTH a malformed and a merely PERMUTED hand-built topology (a permuted adjacency passing the gate would mint a second key for identical content — the exact defect the one-content-one-key law forbids); `EncodeForm` the closed `[Union]` encode-modality family with a validated `Of` admission head, `IValidityEvidence` so the `Mesh` arm re-gates a hand-built adjacency at the `Apply` rail; `ReconcileOp` the request `[Union]` and `ReconcileAnswer` the answer `[Union]` (`Digest` · `Reconciled` · `Topology`) registered into the validity fold; `Reconciliation` the static surface owning the ONE `Apply`; `NameAddress`/`NamingHash` the per-name binding and the reconciliation receipt.
- Cases: `EncodeForm.Mesh(CanonicalTopology)` — the FROZEN int32-LE adjacency stream; `EncodeForm.Cloud(VectorCloud)` — kind-tagged IEEE-754-LE coordinates under the per-case canonical order (a `ClusterCase` is an order-free set, sorted lexicographic by `(X, Y, Z)`; a `PolylineCase` is a directed chain, stored order IS content; a `RingCase` is a cycle, rotated to its lexicographically LEAST ROTATION — tied least vertices resolve by whole-sequence comparison so a duplicated least point can never fork one cyclic content into two digests — with winding preserved, the SAME rotation law the mesh face cycles carry), and a weighted `ClusterCase` hashes its mass column beside its coordinates — each mass rides its vertex through the canonical sort, mass the final tiebreak — so equal-point clusters with divergent weights never alias to one digest, while an absent column emits a zero count, byte-distinct from any present column; `EncodeForm.Parametric` — RAW host-neutral arrays (`Direction(int Degree, Arr<double> Knots)` per parametric direction, `Arr<double> Weights`, `Arr<Point3d> ControlNet`), one direction a curve, two a surface, N a volume — the direction count is the generator, never `Curve`/`Surface` sibling cases; knots arrive NORMALIZED to `[0, 1]` and clamped, validated at admission, and WEIGHT-SCALE canonicalization of rational carriers (weights are projectively scale-invariant, so the projecting carrier normalizes them — unit lead weight — before `Of`) is the W4 curve/surface projection obligation stated on their Boundary cards, so one curve yields one content key across the host/engine seam. `ReconcileOp` cases: `Encode(EncodeForm)` · `Reconcile(NameTable Prior, CanonicalTopology Rebuilt)` · `BuildEntities(MeshSpace)`. `CanonicalTopology.OfMesh` is the ONE native admission (topology vertices in topology-vertex order, edges as sorted endpoint pairs in lexicographic order, faces as least-rotation boundary cycles in lexicographic order — host enumeration order never reaches the bytes, so a host-version enumeration drift can never silently re-key the corpus); a native-brep face-edge-vertex factory is growth under the identical canonical-order law — today a brep patch encodes through its meshed patch via `OfMesh`, exactly as the `pack.md` consumer routes it.
- Entry: `public static Fin<ReconcileAnswer> Reconciliation.Apply(ReconcileOp op, Op? key = null)` — the `Encode`/`Reconcile` twin statics are the deleted sibling form. `EncodeForm.Of` discriminates admission on input shape: `Of(MeshSpace)` builds the topology once, `Of(CanonicalTopology)` wraps an already-built adjacency, `Of(VectorCloud)` wraps the admitted cloud, and `Of(Arr<Direction>, Arr<double>, Arr<Point3d>, Op? key)` is the validated parametric ingress returning `Fin<EncodeForm>` — degree ≥ 1, clamped normalized non-decreasing finite knots, `ControlNet.Count` equal to the per-direction `Knots.Count − Degree − 1` product, positive finite weights, finite coordinates; a raw-array refusal routes the `Op`-keyed admission fault, never a thrown exception.
- Auto: the `Mesh` arm re-hashes identically under a morph (moved control points, same adjacency) and distinctly under a topology break (changed adjacency) — exactly the reference/content split `Track` reads from the same adjacency; the `Reconcile` arm admits both inputs applicatively (`(k.AcceptInput(prior), k.AcceptInput(rebuilt)).Apply(…).As()` — two independent evidence gates, one fan-in), folds the rebuilt content-address set once (an immutable `Set<UInt128>` fold over `Entities`), and traverses the prior `NameTable` entries applicatively — `toSeq(Entries.Values).Traverse(entry => …).As()` over `Validation<Error, NameAddress>` — so EVERY dangling reference (a prior name whose current canonical bytes hash to NO rebuilt entity) reports in one verdict (`GeometryFault.HashMismatch` per defect, `Error` the accumulating monoid) before `.ToFin()` rejoins the short-circuit rail, then binds the whole-topology digest beside the per-name addresses; the `BuildEntities` arm emits the `CanonicalTopology` whose `RebuiltEntity` rows carry explicit `Self` (the entity's own index — the `naming.md` `VertexNames` re-key), TRUE incidence (a vertex's edge-adjacent neighbor ring, an edge's endpoints, a face's boundary cycle), and the TRUE `[vertex, edge, face]` incident-degree histogram derived by immutable folds — neighbor rings, per-vertex face degrees, and per-edge face counts accumulate as `HashMap`/`Set` folds in one pass, never a mutable dictionary sweep, never a hand-literal, never an O(F·E) face scan per edge. Every arm admits its input evidence through `key.AcceptInput` (`EncodeForm`, the prior `NameTable`, the rebuilt `CanonicalTopology`) and gates its answer through `key.AcceptValue`, so the `IValidityEvidence` claims are enforced at the one acceptance oracle, not re-checked by consumers.
- Receipt: `NamingHash` (`Whole` — the whole-topology `GeometryHash` — plus the `TopoName → NameAddress` map) IS the reconciliation evidence the Persistence structural merge consumes per node; `IValidityEvidence` with a key-binding claim fold (`Addresses` key equals `NameAddress.Name`), so it registers into the `OpAcceptance.ValidityOf` oracle like every kernel receipt — no parallel reconciliation ledger.
- Packages: `Rasm`/Vectors (`MeshSpace` defensive native snapshot, `VectorCloud` — composed), RhinoCommon (the welded topology read behind `MeshSpace.DuplicateNative` — `Mesh.TopologyVertices.IndicesFromFace`, `Mesh.TopologyEdges.GetTopologyVertices` → `IndexPair.I`/`.J`, `Point3d` coordinates), `Rasm.Domain` (`ContentHash.Of` seed-zero — the ONE federation hasher; `Op`/`OrDefault`/`AcceptValue`; `IValidityEvidence`/`ValidityClaim`), Thinktecture.Runtime.Extensions (`[Union]`, `[ValueObject<UInt128>]`, generated `Switch`), LanguageExt.Core (`Fin`/`Validation`/`Traverse`/`Seq`/`Arr`/`HashMap`/`Set`), System.Buffers (`ArrayBufferWriter<byte>`), System.Buffers.Binary (`BinaryPrimitives` int32/double little-endian), BCL inbox (`MemoryExtensions.SequenceCompareTo`/`SequenceEqual` span order).
- Growth: a new geometry modality is one `EncodeForm` case with its own frozen stream law (never a second hasher, never a per-modality encoder class); a new per-case content column is one counted layout block on the owning case's stream — the cluster mass block is the precedent — never a sibling digest; a new reconciliation projection is one column on `NameAddress`; a native-brep adjacency source is one `CanonicalTopology.Of*` factory under the same canonical-order law; a periodic parametric carrier re-expresses as clamped at the projecting owner's ingress — the raw-array shape here never widens for it.
- Boundary: this page is the SOLE OWNER of the three frozen canonical byte layouts, every integer and float little-endian, contiguous, no padding, hashed by the kernel seed-zero `XxHash128` — (1) `Mesh` (FROZEN, byte-identical to the `CANONICAL_BYTE_IDENTITY` fixture): `int32 VertexCount` · `int32 EdgeCount` · `(int32 Min, int32 Max)` per sorted edge pair, pairs in lexicographic order · `int32 FaceCount` · per least-rotation cycle in lexicographic face order `(int32 CycleLength, int32 Vertex…)`; (2) `Cloud`: `int32 KindOrdinal` (cluster 0 · polyline 1 · ring 2) · `int32 VertexCount` · `(double X, double Y, double Z)` per vertex in the per-case canonical order · `int32 MassCount` (`0` unweighted, else `VertexCount`) · `double Mass…` in the same canonical vertex order; (3) `Parametric` (hashing the NORMALIZED form): `int32 DirectionCount` · per direction `(int32 Degree, int32 KnotCount, double Knot…)` · `int32 WeightCount` · `double Weight…` · `int32 ControlCount` · `(double X, double Y, double Z)` per control point. Doubles emit raw IEEE-754-LE bits with `-0.0` normalized to `+0.0` — the Persistence `CanonicalWriter` discipline — and non-finite values are inadmissible upstream, so the streams never carry a `NaN`; the digest is meaningful only under its form, so every consumer seam carries `(form, digest)` and a digest never crosses a form boundary. Persistence reads the IDENTICAL mesh layout before its `XxHash128` rather than re-deriving a second encoding — the golden-bytes fixture is the cross-package byte-identity proof, so a drifted byte order in either package is a caught defect, not a silent merge corruption. Canonicalization beyond this page's validated gates is the projecting owner's proof (the `identity.md` law): knot normalization is validated HERE, weight-scale canonicalization of a rational carrier is the W4 parametric producer's projection obligation. The two fault families stay orthogonal: admission refusals ride the `Op`-keyed `Rasm.Domain` `Fault` band (the `validation.md` admission vocabulary), geometry-domain failures ride `GeometryFault` band 2400 (`HashMismatch` 2405, cluster `naming`), both lowering into `Error` on the ONE `Fin` rail through `ToError()` — neither family absorbs the other. `CanonicalTopology` is immutable so the emitted bytes are referentially transparent — a mutable adjacency buffer feeding the hash is the named non-determinism defect — and its canonical row order is a CLAIMED invariant, not a construction convention: `IsValid` asserts lexicographic edge order, least-rotation lexicographically ordered face cycles, every face-cycle edge resolved in `Edges` (an inconsistent hand-built adjacency never reaches the hash), and the `Self`-sequential kind partition of `Entities`, so a `with`-mutated or permuted record fails the `Apply` gate instead of silently forking one content into two keys, and a re-keyed `Self` regression (the retired `IncidentVertices[0]` mis-key class) is refused at the oracle before it can corrupt the `naming.md` `VertexNames` row; `OfMesh` is TOTAL — a native face-query refusal surfaces as an empty boundary cycle the oracle refuses with the `Op`-keyed admission fault, never a thrown exception; `TopoName.Value` is NEVER equality-tested against a `GeometryHash` — the `[ValueObject<UInt128>]` promotion makes the cross-axis compare a type error; a name bound directly as a content hash (or a hash treated as a name) is the collapsed defect.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Geometry.Naming;

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

    // Cloud/Parametric are valid by their own admission gates; Mesh re-gates the hand-built adjacency path.
    public bool IsValid => Switch(
        mesh: static m => m.Topology.IsValid,
        cloud: static _ => true,
        parametric: static _ => true);

    public static EncodeForm Of(MeshSpace space) => new Mesh(CanonicalTopology.OfMesh(space));
    public static EncodeForm Of(CanonicalTopology topology) => new Mesh(topology);
    public static EncodeForm Of(VectorCloud cloud) => new Cloud(cloud);

    // controls computes only under the normalized gate: every factor is then >= 2, so the product
    // never traps the checked context on a hostile degree/knot pair — refusal stays railed.
    public static Fin<EncodeForm> Of(Arr<Direction> directions, Arr<double> weights, Arr<Point3d> controlNet, Op? key = null) {
        bool normalized = directions.Count >= 1 && directions.All(static d => Normalized(d.Degree, d.Knots));
        long controls = normalized ? directions.Fold(1L, static (product, d) => unchecked(product * (d.Knots.Count - d.Degree - 1))) : 0L;
        return guard(
                normalized && controlNet.Count == controls && weights.Count == controls
                    && weights.All(static w => ValidityClaim.Positive(w)) && controlNet.All(static p => ValidityClaim.Finite(p)),
                key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (EncodeForm)new Parametric(directions, weights, controlNet));
    }

    // Clamped + normalized + monotone + finite: the R1 one-curve-one-content-key admission gate.
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

    // The canonical order IS a claim set: a hand-built permuted adjacency is refused at the gate,
    // never a second content key for identical content.
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

    // Least ROTATION, not first-min pivot: a duplicated minimum vertex resolves by whole-cycle
    // comparison, and a host-refused face query ([] from IndicesFromFace) passes through for the
    // oracle to refuse — the adapter never throws.
    static int[] Rotated(int[] cycle) {
        if (cycle.Length == 0) { return cycle; }
        int least = cycle.Min();
        return Enumerable.Range(0, cycle.Length)
            .Where(pivot => cycle[pivot] == least)
            .Select(pivot => (int[])[.. cycle[pivot..], .. cycle[..pivot]])
            .Aggregate(static (best, next) => next.AsSpan().SequenceCompareTo(best) < 0 ? next : best);
    }

    // Self = the entity's own index; incidence and histograms are TRUE degrees from immutable folds.
    static Seq<RebuiltEntity> Entities(int vertices, Arr<(int Min, int Max)> edges, Arr<int[]> faces) {
        var neighbors = toSeq(edges).Fold(HashMap<int, Set<int>>.Empty, static (map, edge) => map
            .AddOrUpdate(edge.Min, ring => ring.Add(edge.Max), Set(edge.Max))
            .AddOrUpdate(edge.Max, ring => ring.Add(edge.Min), Set(edge.Min)));
        var faceDegree = toSeq(faces).Fold(HashMap<int, int>.Empty, static (map, cycle) =>
            cycle.Distinct().Aggregate(map, static (fold, vertex) => fold.AddOrUpdate(vertex, static n => n + 1, 1)));
        var edgeFaces = toSeq(faces).Fold(HashMap<(int Min, int Max), int>.Empty, static (map, cycle) =>
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
        var stream = new ArrayBufferWriter<byte>(values.Length * 4);
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
// Byte-emitter kernel (named exemption): little-endian words; -0.0 normalizes to +0.0 before emission.
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

    // Applicative traverse against the REBUILT content-address set: EVERY dangling name — a prior
    // entry whose current bytes hash to no rebuilt entity — reports before .ToFin() rejoins the rail.
    static Fin<HashMap<TopoName, NameAddress>> Addresses(NameTable prior, CanonicalTopology rebuilt) {
        Set<UInt128> live = rebuilt.Entities.Fold(Set<UInt128>.Empty, static (set, entity) => set.Add(ContentHash.Of(entity.CanonicalBytes)));
        return toSeq(prior.Entries.Values)
            .Traverse(entry => ContentHash.Of(entry.CanonicalBytes) switch {
                var digest when live.Contains(digest) =>
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

    // FROZEN layout — byte-identical to the CANONICAL_BYTE_IDENTITY fixture; field order never changes.
    static ArrayBufferWriter<byte> MeshStream(CanonicalTopology topology) {
        var stream = new ArrayBufferWriter<byte>(12 + (topology.Edges.Count * 8) + topology.Faces.Sum(static cycle => 4 + (cycle.Length * 4)));
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
        var stream = new ArrayBufferWriter<byte>(12 + (canonical.Points.Count * 24) + (canonical.Mass.Count * 8));
        stream.Word(canonical.Kind);
        stream.Word(canonical.Points.Count);
        foreach (Point3d point in canonical.Points) stream.Coordinate(point);
        stream.Word(canonical.Mass.Count);          // 0 = unweighted; a weighted column rides the same canonical order
        foreach (double mass in canonical.Mass) stream.Real(mass);
        return stream;
    }

    // The mass column IS content: each mass rides its vertex through the canonical sort (mass the
    // final tiebreak), so equal-point clusters with divergent weights never share a digest.
    static (int Kind, Seq<Point3d> Points, Seq<double> Mass) Weighted(Seq<Point3d> points, Arr<double> mass) {
        Seq<(Point3d Point, double Mass)> rows = toSeq(points
            .Map((point, index) => (Point: point, Mass: mass[index]))
            .OrderBy(static row => row.Point.X).ThenBy(static row => row.Point.Y)
            .ThenBy(static row => row.Point.Z).ThenBy(static row => row.Mass));
        return (0, rows.Map(static row => row.Point), rows.Map(static row => row.Mass));
    }

    static ArrayBufferWriter<byte> ParametricStream(EncodeForm.Parametric form) {
        var stream = new ArrayBufferWriter<byte>(
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

    // Least ROTATION, not least vertex: tied least points resolve by whole-sequence comparison,
    // so a duplicated least vertex can never fork one cyclic content into two digests.
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

## [03]-[ONE_WIRE_FIXTURE_CORPUS]

The single content-addressed golden-fixture corpus every runtime parity harness reads — the missing parent of the `CONTENT_IDENTITY_PARITY`, `FAULT_WIRE_ROUNDTRIP`, and `TRI_LANGUAGE_WIRE_PARITY` cross-language obligations. Every fixture is keyed by the one `XxHash128` seed (seed zero, two-64-bit-half order) the federation mints C#-side, frozen once so a parity drift surfaces as a single corpus mismatch instead of three divergent reproductions. This cluster is the corpus INDEX; the `CANONICAL_BYTE_IDENTITY` stream and digest are the sole host-derived REAL byte fixture the geometry domain owns directly, and each other fixture names its producer owner and its `[SOURCE]` state — REAL when the bytes are host-derived or deterministically derivable from a settled design, DESIGN-PIN when the design input is not yet frozen. A DESIGN-PIN fixture carries no fabricated byte set; its bytes are derived only after the producer page pins the missing input.

- Owner: the corpus is a federation index, not a second store — each fixture lives with its producer page and this cluster binds them under the one seed. No fixture byte set is invented here; a DESIGN-PIN row is a blocking gap on the named producer, never a placeholder byte literal.
- Consumers: `python:runtime/evidence/identity#IDENTITY` reproduces the seed through `xxhash.xxh3_128_intdigest` and reads the corpus to assert byte-identical reproduction; `typescript:wire/frame#CONTENT_HASHING` reproduces it through the 128-bit wasm hash and reads the `tests/contracts` corpus (TS readers in `tests/typescript/_testkit`); the C# shared test corpus (the `[BRANCH_TEST_NODE_PROVISIONING]` shared-corpus task) asserts every producer emits its fixture byte-for-byte. Every consumer reads the one frozen corpus, never re-deriving its own fixtures.
- Boundary: the seed is the single key across all fixtures — a per-fixture digest function or a per-runtime fixture mint is the named drift defect; the corpus is read-only for every consumer and every producer asserts its own fixture against the frozen bytes; a fabricated byte set for an unpinned fixture is the rejected form, replaced by an explicit DESIGN-PIN gap on the producer. The two REAL fixtures are HARNESS-CONFIRMED-ONCE gated: one `XxHash128`/`NodeLinkProjection` reproof runs before any page cites their literal digests as REAL — the layout laws are settled independent of that gate.

The fixture set the corpus carries, each keyed by the one seed:

| [INDEX] | [FIXTURE]               | [PRODUCER_OWNER]                                                              | [SOURCE]   | [SHAPE]                                                                                                                                                         |
| :-----: | :---------------------- | :---------------------------------------------------------------------------- | :--------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | CANONICAL_BYTE_IDENTITY | `Rasm/Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` (this page)    | REAL       | 52-byte int32-LE adjacency stream + `0x9462A71A5DD13DCFA3B1D6D225FCBE70` digest (harness-confirmed-once)                                                        |
|  [02]   | CLASH_GOLDEN            | `Rasm/Geometry/Spatial/index#CLASH_GOLDEN`                                    | REAL       | 8-primitive `BoundingBox(min,max)` set → 160-byte `Bounds`/`Nodes` LE stream re-derived under index's OUTWARD-rounding law (`BitDecrement` min / `BitIncrement` max); topology (`NodeCount == 3`), descriptor block, and the 4-pair clash set `{(0,1),(2,3),(4,5),(6,7)}` stay PINNED — the stream bytes await the one-time harness reproof |
|  [03]   | FAULT_TRIPLES           | `Rasm.Compute/Runtime/channels#FAULT_PROJECTION`                              | DESIGN-PIN | `FaultDetail` `(package, code, case)` triples spanning the disjoint bands (ComputeFault 2200, HopFault 4500, WireFault 4520-4532, store/config app roots)       |
|  [04]   | CRDT_OP_SET             | `Rasm.Persistence/Version/commits#CRDT_ALGEBRA`                               | DESIGN-PIN | the `MvRegister`/`opMerge` op-set whose divergent-delivery folds converge byte-identically                                                                      |
|  [05]   | GLB_BY_KEY              | `Rasm.Compute/Runtime/codecs#TILE_PARTITION` over the GLB tessellation result | DESIGN-PIN | one content-keyed GLB sample keyed by the `ContentIdentity` seed                                                                                                |
|  [06]   | HLC_TWO_HALF            | `Rasm.AppHost/Runtime/ports#HLC_FANIN`                                        | DESIGN-PIN | the two-64-bit-half HLC stamps whose half order an off-by-one-half would corrupt                                                                                |
|  [07]   | MATERIAL_LAYER_GOLDEN   | `Rasm.Element/Projection/address#CONTENT_ADDRESS`                             | DESIGN-PIN | the float-bearing `IfcMaterialLayer`-shaped node (layer thicknesses + properties) whose `CanonicalWriter` IEEE-754-LE bytes the C#/Python/TypeScript `ContentAddress` agree on byte-for-byte                       |

Fixtures [1] and [2] are REAL and frozen against their producer pages — [1] CANONICAL_BYTE_IDENTITY on this page (the 52-byte stream, the `XxHash128.HashToUInt128` digest, the morph and topology-break discriminators), [2] CLASH_GOLDEN in `Rasm/Geometry/Spatial/index#CLASH_GOLDEN` with the 8 `BoundingBox(min,max)` coordinate tuples, the SAH split (axis X, primitives `{0,1,2,3}` left / `{4,5,6,7}` right), the identity `Order` permutation, `NodeCount == 3`, and the 4-pair clash set `{(0,1),(2,3),(4,5),(6,7)}` PINNED; index's OUTWARD-rounding bounds narrowing (`BitDecrement` min / `BitIncrement` max post-cast) re-derives the 160-byte `Bounds`/`Nodes` LE stream, so the stream bytes and both literal digests stand only under the harness-confirmed-once gate above — topology, descriptor block, and clash set are settled independent of that gate. Fixtures [3]–[7] remain DESIGN-PIN: the bytes are derivable only after each producer page (in the `Rasm.Compute`/`Rasm.Persistence`/`Rasm.AppHost`/`Rasm.Element` siblings, outside this folder's write-scope) pins its missing input, and no fabricated byte set stands in for an unpinned fixture. No design-pin gap inside the geometry kernel's write-scope blocks corpus completion; the remaining gaps are owned by the named cross-folder producers and are the synthesis tier's arbitration.

## [04]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes.

| [INDEX] | [AXIS/CONCERN]            | [OWNER]                    | [KIND]                                                                             | [RAIL]                                                   | [CASES] |
| :-----: | :------------------------ | :------------------------- | :---------------------------------------------------------------------------------- | :-------------------------------------------------------- | :-----: |
|  [01]   | Content-axis identity     | `GeometryHash`             | `[ValueObject<UInt128>]` — type-distinct from the `TopoName` reference axis        | pure value                                                |    —    |
|  [02]   | Canonical adjacency       | `CanonicalTopology`        | immutable record + total `OfMesh` encoder + oracle-claimed canonical order + `Self`-keyed entity emitter | `CanonicalTopology.OfMesh → CanonicalTopology` (boundary) |    —    |
|  [03]   | Encode modality           | `EncodeForm`               | `[Union]` Mesh · Cloud · Parametric + shape-discriminating `Of` admission family, `IValidityEvidence` | `EncodeForm.Of → EncodeForm` / `Fin<EncodeForm>` (raw)    |    3    |
|  [04]   | Reconciliation request    | `ReconcileOp`              | `[Union]` Encode · Reconcile · BuildEntities                                       | carrier (dispatched in `Apply`)                           |    3    |
|  [05]   | Reconciliation answer     | `ReconcileAnswer`          | `[Union]` Digest · Reconciled · Topology, `IValidityEvidence` delegation fold      | carrier (returned in the `Apply` rail)                    |    3    |
|  [06]   | Naming↔hash bridge        | `Reconciliation`           | ONE `Apply` entry: total generated `Switch` + accumulating reconcile traverse      | `Reconciliation.Apply → Fin<ReconcileAnswer>`             |    3    |
|  [07]   | Reconciliation receipt    | `NamingHash`/`NameAddress` | whole-topology digest + per-name content addresses, `IValidityEvidence`            | receipt (carried in `Reconciled`)                         |    —    |

## [05]-[RESEARCH]

- [CANONICAL_BYTE_IDENTITY] — the `EncodeForm.Mesh` arm of `Reconciliation.Apply` emits the canonical adjacency bytes (`int32-LE VertexCount` · `int32-LE EdgeCount` · `(int32-LE Min, int32-LE Max)` per sorted edge endpoint pair in lexicographic pair order · `int32-LE FaceCount` · per least-rotation face cycle in lexicographic face order `(int32-LE CycleLength, int32-LE Vertex…)` · every integer little-endian, contiguous, no padding) the Persistence `GeometryHash` reads; this page is the SOLE OWNER of that frozen field order and `Rasm.Persistence/Query/topology` (`[CONTENT_KEY]`) reads the identical layout, never a second encoding. The byte-identity contract is the FROZEN golden-bytes fixture both the geometry domain and the Persistence structural merge assert against — the shared reference is the single-triangle topology (`VertexCount=3`; edges `(0,1),(0,2),(1,2)`; face cycle `[0,1,2]`) whose 52-byte canonical stream is `03 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 02 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 01 00 00 00 02 00 00 00` and whose `XxHash128.HashToUInt128` digest is `0x9462A71A5DD13DCFA3B1D6D225FCBE70` (16-byte LE `70 be fc 25 d2 d6 b1 a3 cf 3d d1 5d 1a a7 62 94`), HARNESS-CONFIRMED-ONCE before any consumer cites it REAL; a tier-2 cross-package byte-equality harness feeds this reference through both the `Reconciliation.Apply` mesh arm and the Persistence `GeometryHash` path and asserts the stream bytes AND the digest, with the morph case (moved control points, same adjacency → equal hash) and the topology-break case (changed adjacency → distinct hash) as the two discriminating laws. The live-host spellings are HOST-VALIDATED against the Rhino 9 WIP `Mesh` surface: `Mesh.TopologyVertices.IndicesFromFace(int)` returns the boundary cycle in CCW winding `[0,1,2]` for the single-triangle reference, `Mesh.TopologyEdges.GetTopologyVertices(int) : IndexPair` yields the sorted endpoint pairs `(0,1),(0,2),(1,2)`, the `IndexPair.I`/`.J` member surface carries the pair endpoints, and the least rotation of `[0,1,2]` is the identity — the rotation law is SETTLED, the reference edges `(0,1),(0,2),(1,2)` are already in lexicographic order and the single face trivially sorted, so the canonical-order claims leave the 52-byte stream and digest byte-identical, and this fact is never re-asserted from a fresh host probe. The `Cloud` and `Parametric` layouts declared in `[02]` are frozen by the same discipline (kind-tagged streams with the counted mass block / normalized-form streams, IEEE-754-LE doubles with `-0.0 → +0.0`); their consumers — `pack.md` cloud keys, the W4 parametric carriers feeding the `Rasm.Persistence` `Element` `ToCanonicalBytes` fold as a COMPONENT, never a sibling SpineRef key — bind `(form, digest)` pairs, and each new-form fixture freezes with its first producer.
