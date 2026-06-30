# [BIM_MODEL_DIFF]

The GlobalId-stable federation diff over the seam graph: one `ModelDiff` change-set folds two `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` snapshots into the `ElementChange` added/removed/modified/moved arms, joining by the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` (the IFC `GlobalId` [H6] — the ONE identity two federated submissions share, because the neutral kernel `NodeId` is minted afresh per ingest and never coincides across parties) and classifying each matched pair by the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` content and placement keys so an unchanged element dedups by content address and a re-check bakes only the changed elements. The diff consumes two `ElementGraph` snapshots as settled vocabulary and mints no second element shape; the consumer element is the `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Bake(objectNode)` fold, the retired `BimModel`/`BimElement` snapshot pair GONE.

The diff is the cross-party twin of two same-lineage owners and re-derives neither. The `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `StructuralMerge` is the NodeId-keyed (re-ingest `Reconcile`-aligned on `ExternalId`) version-lineage THREE-way merge over one model's history; the `Review/versioning#VERSION_GRAPH` commit-DAG is the branching revision graph. This page is the PAIRWISE two-way federation diff over two INDEPENDENT submissions whose neutral `NodeId`s never coincide, so the join is the IFC `GlobalId` directly (no `Reconcile`); all three compose the one `Generator.Equals` `Inequalities` member-diff substrate and the one seam `ContentAddress` codec, none re-deriving another. Every diff rejection lifts the `Model/faults#FAULT_BAND` `BimFault` band BARE (the `Expected`-derived case IS the `Error`, no `.ToError()` hop, the ctor `(Op key, string detail)`). The page is HOST-LOCAL; the `ModelDiff.Encode`/`Decode` cross-runtime wire payload is HOST-FREE.

## [01]-[INDEX]

- [01]-[MODEL_DIFF]: the `ModelDiff` change-set, the `ElementChange` closed `[Union]` (Added/Removed/Modified/Moved) with the `ChangeKind` token projection, the `ElementFingerprint` content/placement keys the join dedups on, the `Generator.Equals` `Inequalities` `AspectDelta` member-level delta the `Modified` arm carries, and the `ModelDiff.Encode`/`Decode` host-free cross-runtime projection.
- [02]-[AUDIT]: the chained content-addressed `AuditEntry` log folding `ModelDiff` change-sets across a version sequence into a tamper-evident per-element mutation trail, the `AuditTrail.For(globalId)` lifecycle query, and `AuditTrail.Verify()`.

## [02]-[MODEL_DIFF]

- Owner: `ModelDiff` the change-set record carrying the added/modified/removed/moved arms between two `ElementGraph` snapshots; `ElementChange` the closed `[Union]` change family (Added, Removed, Modified, Moved) each carrying its IFC `GlobalId` (the base accessor) plus its typed evidence; `ChangeKind` the `[SmartEnum<string>]` token `ElementChange.Kind` projects (the `Relations/relation#EDGE_ALGEBRA` `Relationship.Kind` idiom) the audit and the wire read without a union switch; `ElementFingerprint` the per-element `(GlobalId, ContentKey, PlacementKey)` carrier the join dedups on; `AspectDelta` the typed member-level delta the `Modified` arm carries.
- Entry: `ModelDiff.Between(ElementGraph baseline, ElementGraph revision, Op key)` folds the two snapshots into one `ModelDiff` — a `GlobalId` present in the revision but not the baseline is `Added`, present in the baseline but not the revision is `Removed`, present in both with a differing content key is `Modified` (or `Moved` when only the placement key differs), and present in both with both keys identical dedups as unchanged; `Fin<T>` because the `Modified` enrichment bakes the changed elements through `Rasm.Element/Graph/element#ELEMENT_GRAPH` `Bake` (which rails `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` on a corrupt subgraph), so an unchanged element never bakes and a re-check costs only the changed elements. `ModelDiff.Encode(diff, key)`/`Decode(json, key)` is the host-free cross-runtime projection and `ModelDiff.Fingerprint(graph, node)` the per-element content fingerprint the `Review/versioning#VERSION_GRAPH` commit-DAG and this diff both key on.
- Auto: `Between` `Federate`s each graph into a `GlobalId`-keyed map over the `ExternalId`-bearing `Object` nodes (the `Review/coordination#COORDINATION` `ExternalId` `Choose`-discard-`None` law — an authored Object with no IFC `GlobalId` is simply not on the federation surface, never a fault), `Fingerprint`s each through the seam `ContentAddress`, then partitions the common set: a differing `ContentKey` is `Modified`, an equal `ContentKey` with a differing `PlacementKey` is `Moved`, both equal is unchanged. The content key folds the `Object`'s semantic head (kind/classification/predefined/name/tag) with the order-independent content addresses of its bound non-`Object` nodes (`PropertySet`/`QuantitySet`/`Material`/`Assessment`/`Appearance`/`Coverage`) and its outgoing-edge structure; the placement key folds the `Object`'s geometry through the `RepresentationContentHash` map ALONE — EVERY geometry content-hashed there, the heavy display `Body` AND the lightweight analytical `Axis`/`FootPrint` the structural/energy disciplines resolve one-hop by content key — so a relocation moves the geometry bytes, the content hashes, and thus the placement key, while the semantic content key stays stable; an inline `BoundaryPolygon`/`Axis` coordinate read is the named seam violation (the seam carries no raw coordinate field — `Graph/element#NODE_MODEL` M2). The `Modified` arm carries `Generator.Equals` `Inequalities` over the two baked `Element`s as `AspectDelta` rows (the `Id`/`ExternalId`/`History` members filtered as local-identity-and-provenance noise), so a downstream consumer reads the exact `Properties[Pset].FireRating` member that moved, not an opaque content-key delta.
- Receipt: the `ModelDiff` change-set is the incremental federation evidence; a `Review/issues#BCF_ARCHIVE` `BcfTopic` anchors a `BcfViewpoint.SelectedGlobalIds` on the `Modified`/`Moved` element `GlobalId`s this diff names, the `Review/coordination#COORDINATION` `Coordination.Between` folds two change-sets into the downstream-affected element/task/cost sets, and the `Review/versioning#VERSION_GRAPH` `BimCommit` keys its `Map<string, ElementFingerprint>` on the SAME `ElementFingerprint` so a commit, a diff, and the audit chain carry one content-key identity; the `ModelDiff.Encode` payload is the one cross-runtime contract the `ts:ui/bcf-anchor` live-binding decodes to highlight the changed `GlobalId`s.
- Packages: Rasm.Element, Generator.Equals, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, LanguageExt.Core, Rasm, BCL `System.Text.Json`
- Growth: a new change kind is one `ElementChange` union arm plus one `ChangeKind` row; a new content dimension is one column folded into the content key over the same seam `ContentAddress` codec; the join keys by `GlobalId` plus the content/placement keys so a new identity dimension is a content-key field, never a second identity scheme; a new delta projection is one richer `AspectDelta` over the same `Inequalities`; never a per-change-kind type and never a parallel diff record.
- Boundary: the federation join is the `Node.Object.ExternalId` IFC `GlobalId` [H6] and a join on the neutral `NodeId` is the deleted form — two independent submissions re-mint rooted `NodeId`s, so only the `GlobalId` is cross-party stable; the content and placement keys are the seam `ContentAddress` over the ONE `Rasm.Element/Projection/address#CONTENT_ADDRESS` codec [H7], and the retired `csharp:Rasm.Compute/Runtime/codecs` `InterchangeIdentity.Key` consumed up-stratum AND a hand-rolled `XxHash128`/`Encoding.UTF8` string-join hasher are the named defects — the diff content bytes and the `NodeId` content hash share the one seam projection; the `Moved` arm is distinguished from `Modified` by the placement-key delta (the geometry bucket — representations plus analytical geometry — moved while the content bucket held), and collapsing the two buckets is the deleted form; the `Modified` delta is the `Generator.Equals` `Inequalities` member-path projection and a string-formatted whole-record diff is the deleted form; the `ElementChange` family is a closed `[Union]` and a per-change-kind class is the deleted form; the consumer element is the `Bake` fold and the retired `BimModel`/`BimElement` snapshot pair is GONE (a diff that re-stores a second element record off the seam graph is the deleted form); the cross-runtime wire rides `ModelDiff.Encode`/`Decode` over the shared `ThinktectureJsonConverterFactory` (HOST-FREE — `GlobalId` strings, the seam `ContentAddress` keys, the typed deltas, never a host geometry type), and the retired `Exchange/wire#WIRE_PROJECTION` `BimWireContext`/`BimWireOptions.Json` (GONE — the strata-leaking generic-model serializer is retired, `wire.md` now owning the `IfcWire` IFC interchange wire) plus a parallel `DiffWire` record duplicating `ModelDiff`'s shape are the deleted forms — the seam-graph snapshot wire is `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s, not minted here; this page is the cross-party PAIRWISE diff and re-deriving the `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` NodeId-keyed three-way merge here is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Generator.Equals;
using LanguageExt;
using Rasm;
using Rasm.Element;
using Thinktecture;
using Thinktecture.Text.Json.Serialization;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The neutral change-kind token an audit row persists and a TS decode switches on — the [SmartEnum] projection
// of the ElementChange union case (the Relations/relation#EDGE_ALGEBRA Relationship.Kind idiom), never a
// stringly "added"/"removed" literal and never a per-call-site discriminant.
[SmartEnum<string>]
public sealed partial class ChangeKind {
    public static readonly ChangeKind Added = new("added");
    public static readonly ChangeKind Removed = new("removed");
    public static readonly ChangeKind Modified = new("modified");
    public static readonly ChangeKind Moved = new("moved");
}

// --- [MODELS] -----------------------------------------------------------------------------
// One member-level delta projected from the Generator.Equals Inequalities member diff: Path is the dotted/bracketed
// MemberPath (e.g. "Properties[Pset_WallCommon].FireRating"), Before/After the old/new rendered values — the typed
// evidence the Modified arm carries so a BCF topic anchors the exact aspect that changed, not "something changed".
public readonly record struct AspectDelta(string Path, string Before, string After);

// The per-element content fingerprint the join dedups on: the IFC GlobalId plus the seam ContentAddress content
// and placement keys (the ONE Projection/address#CANONICAL_WRITER codec hashed by the ONE
// Projection/address#CONTENT_ADDRESS hasher — never a second hasher). Two fingerprints
// are equal iff the same element addresses identically, so an unchanged element never enters the change set; the
// SAME carrier the Review/versioning#VERSION_GRAPH BimCommit keys its fingerprint map on.
public readonly record struct ElementFingerprint(string GlobalId, ContentAddress ContentKey, ContentAddress PlacementKey);

// The closed federation change family — each arm carries the IFC GlobalId through the base accessor and its own
// typed evidence: Added/Removed the Classification + PredefinedType + content key, Modified the baseline/revision
// content keys + the member deltas, Moved the baseline/revision placement keys. Kind projects the neutral token.
[Union]
public abstract partial record ElementChange {
    private ElementChange() { }

    public abstract string GlobalId { get; }

    public ChangeKind Kind => Switch(
        added:    static _ => ChangeKind.Added,
        removed:  static _ => ChangeKind.Removed,
        modified: static _ => ChangeKind.Modified,
        moved:    static _ => ChangeKind.Moved);

    public sealed record Added(string GlobalId, Classification Class, PredefinedType Predefined, ContentAddress Content) : ElementChange;
    public sealed record Removed(string GlobalId, Classification Class, PredefinedType Predefined, ContentAddress Content) : ElementChange;
    public sealed record Modified(string GlobalId, ContentAddress BaselineContent, ContentAddress RevisionContent, ImmutableArray<AspectDelta> Deltas) : ElementChange;
    public sealed record Moved(string GlobalId, ContentAddress BaselinePlacement, ContentAddress RevisionPlacement) : ElementChange;
}

public sealed record ModelDiff(Seq<ElementChange> Changes, int UnchangedCount) {
    // The pairwise federation diff: join two ElementGraph snapshots by the Bim-stored Node.Object.ExternalId (the IFC
    // GlobalId [H6] — the one cross-party identity, the neutral NodeId being local per ingest), classify each matched
    // pair by the content/placement keys (unchanged when both match, Moved when only the placement key moved, Modified
    // when the content key moved), and enrich each Modified arm with the Generator.Equals member delta over the two
    // baked Elements. Fin because the Modified enrichment bakes the changed elements (Bake rails ElementFault on a
    // corrupt subgraph); an unchanged element (matching content key) never bakes, so a re-check costs only the changes.
    public static Fin<ModelDiff> Between(ElementGraph baseline, ElementGraph revision, Op key) {
        var prior = Federate(baseline);
        var next = Federate(revision);
        var added = next.Keys.Filter(id => !prior.ContainsKey(id)).Map(id => Added(id, next[id]));
        var removed = prior.Keys.Filter(id => !next.ContainsKey(id)).Map(id => Removed(id, prior[id]));
        var common = prior.Keys.Filter(next.ContainsKey).ToSeq();
        var moved = common
            .Filter(id => prior[id].Fp.ContentKey == next[id].Fp.ContentKey && prior[id].Fp.PlacementKey != next[id].Fp.PlacementKey)
            .Map(id => (ElementChange)new ElementChange.Moved(id, prior[id].Fp.PlacementKey, next[id].Fp.PlacementKey));
        int unchanged = common.Count(id => prior[id].Fp == next[id].Fp);
        return common
            .Filter(id => prior[id].Fp.ContentKey != next[id].Fp.ContentKey)
            .TraverseM(id =>
                from before in baseline.Bake(prior[id].Obj.Id, key)
                from after in revision.Bake(next[id].Obj.Id, key)
                select (ElementChange)new ElementChange.Modified(id, prior[id].Fp.ContentKey, next[id].Fp.ContentKey, Deltas(before, after)))
            .As()
            .Map(modified => new ModelDiff(toSeq(added) + toSeq(removed) + moved + modified, unchanged));
    }

    // The cross-runtime projection the ts:ui/bcf-anchor live-binding decodes — HOST-FREE (GlobalId strings, the seam
    // ContentAddress keys, the typed member deltas), serialized through the shared ThinktectureJsonConverterFactory
    // (the [Union]/[SmartEnum]/[ComplexValueObject]/[ValueObject] owners round-trip with their case discriminant) so a
    // TS decode switches on the ChangeKind token; a malformed payload faults BimFault.ModelRejected (the
    // Model/faults#FAULT_BAND wire-admission arm) BARE, the Seq projecting through an array at the boundary so no
    // LanguageExt Seq converter is required.
    public static Fin<byte[]> Encode(ModelDiff diff, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(new Payload([.. diff.Changes], diff.UnchangedCount), Wire)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"diff-wire-encode:{error.Message}"));

    public static Fin<ModelDiff> Decode(ReadOnlyMemory<byte> json, Op key) =>
        Try.lift(() => JsonSerializer.Deserialize<Payload>(json.Span, Wire)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"diff-wire-decode:{error.Message}"))
            .Map(payload => new ModelDiff(toSeq(payload.Changes), payload.UnchangedCount));

    // The per-element content fingerprint over the seam ContentAddress codec: the content key folds the Object's
    // non-geometry semantics with its bound nodes, the placement key folds its geometry — the split distinguishing a
    // relocation from a content edit. The SAME content-key the Review/versioning#VERSION_GRAPH commit-DAG keys on, so a
    // commit, a diff, and the audit chain share one identity. GlobalId falls back to the NodeId string only off the
    // federation surface (a never-emitted authored Object), the federation diff itself only fingerprinting ExternalId-bearing nodes.
    public static ElementFingerprint Fingerprint(ElementGraph graph, Node.Object node) =>
        new(node.ExternalId.IfNone(node.Id.Value), ContentKey(graph, node), PlacementKey(node, graph.Header.Tolerance));

    static Map<string, (Node.Object Obj, ElementFingerprint Fp)> Federate(ElementGraph graph) =>
        graph.ObjectNodes
            .Choose(node => node.ExternalId.Map(globalId => (globalId, (Obj: node, Fp: Fingerprint(graph, node)))))
            .ToMap();

    static ElementChange Added(string globalId, (Node.Object Obj, ElementFingerprint Fp) entry) =>
        new ElementChange.Added(globalId, entry.Obj.Classification, entry.Obj.PredefinedType, entry.Fp.ContentKey);

    static ElementChange Removed(string globalId, (Node.Object Obj, ElementFingerprint Fp) entry) =>
        new ElementChange.Removed(globalId, entry.Obj.Classification, entry.Obj.PredefinedType, entry.Fp.ContentKey);

    // The semantic content key: the Object's non-geometry head plus the order-independent fold of its outgoing edges'
    // contributions (each = the edge canonical bytes plus, when the far endpoint is a bound NON-Object node, that
    // node's content address — so a property/material edit moves the key while a part/type Object, diffed
    // independently, contributes only its binding structure). Geometry is EXCLUDED (it rides the placement key).
    static ContentAddress ContentKey(ElementGraph graph, Node.Object node) {
        double tolerance = graph.Header.Tolerance;
        Seq<UInt128> contributions = toSeq(toSeq(graph.EdgesAt(node.Id))
            .Filter(edge => edge.Relating == node.Id)
            .Map(edge => BoundContribution(graph, node.Id, edge, tolerance))
            .OrderBy(static contribution => contribution));
        CanonicalWriter writer = new(tolerance);
        writer.String(node.Kind.Key).String(node.Classification.System).String(node.Classification.Code)
            .String(node.PredefinedType.Token).String(node.Name).String(node.Tag).Ordinal(contributions.Count);
        foreach (UInt128 contribution in contributions) { writer.U128(contribution); }
        return ContentAddress.Of(writer.ToBytes().Span);
    }

    static UInt128 BoundContribution(ElementGraph graph, NodeId self, Relationship edge, double tolerance) {
        CanonicalWriter writer = new(tolerance);
        writer.Raw(edge.ToCanonicalBytes().Span);
        NodeId far = edge.Relating == self ? edge.Related : edge.Relating;
        graph.Find(far).IfSome(node => { if (node is not Node.Object) { writer.U128(ContentAddress.Of(node, tolerance).Value); } });
        return ContentAddress.Of(writer.ToBytes().Span).Value;
    }

    // The geometry/placement key: the Object's content-hashed RepresentationContentHash map ALONE — EVERY geometry
    // hashed there, the heavy display Body AND the lightweight analytical Axis/FootPrint the structural/energy
    // disciplines resolve one-hop by content key — so a relocation re-hashes the geometry bytes, moving this key while
    // the semantic content key stays stable, and a pure move reads as Moved, never Modified. The geometry is referenced
    // BY content key, never inline coordinates (an inline BoundaryPolygon/Axis read is the Graph/element#NODE_MODEL M2
    // seam violation). The projection rides the ONE CanonicalWriter codec, the geometry subset of Node.ToCanonicalBytes.
    static ContentAddress PlacementKey(Node.Object node, double tolerance) {
        CanonicalWriter writer = new(tolerance);
        foreach (var (identifier, hash) in node.Representations.ByIdentifier.OrderBy(static pair => pair.Key, StringComparer.Ordinal)) {
            writer.String(identifier).U128(hash);
        }
        return ContentAddress.Of(writer.ToBytes().Span);
    }

    // The Modified delta: the Generator.Equals member diff over the two baked Elements, the Id/ExternalId/History
    // members (local identity + provenance) and the nested Parts (each part is a rooted Object diffed independently
    // by its own GlobalId, and its NodeId is a fresh Guid per ingest, so a Parts drill is cross-party noise) filtered
    // out so only THIS element's own semantic member moves surface — the bound Material/PropertySet/Quantity nodes are
    // content-addressed (stable across ingests) so they are NOT noise — the SAME Inequalities substrate the
    // csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF three-way merge reads.
    static ImmutableArray<AspectDelta> Deltas(Element baseline, Element revision) =>
        [.. Element.EqualityComparer.Default.Inequalities(baseline, revision)
            .Where(static inequality => inequality.Path.Segments is not [{ Value: "Id" or "ExternalId" or "History" or "Parts" }, ..])
            .Select(static inequality => new AspectDelta(inequality.Path.ToString(), Show(inequality.Left), Show(inequality.Right)))];

    static string Show(object? value) => value?.ToString() ?? "<absent>";

    static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web) {
        Converters = { new ThinktectureJsonConverterFactory() },
    };

    readonly record struct Payload(ElementChange[] Changes, int UnchangedCount);
}
```

## [03]-[AUDIT]

- Owner: `AuditEntry` the immutable mutation-log row carrying the element `GlobalId`, the typed `ChangeKind`, the baseline/revision `ContentAddress` pair, the author, the `Instant`, the version pointer, and the chained `EntryKey` content address keyed on the prior entry's key so a retroactive edit breaks the chain; `AuditVersion` the per-version authoring metadata; `AuditTrail` the append-only log folding the per-version `ModelDiff` change-sets into the chained entry sequence, queryable by element `GlobalId`. The trail is a model-mutation log (who/when/what-changed-semantically), explicitly distinct from the geometry-asset XMP lineage (who-minted-this-GLB) and from the branching commit-DAG.
- Entry: `AuditTrail.Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history)` folds a version sequence of `ModelDiff` change-sets into the chained `AuditTrail` — each `ElementChange` arm in each version's diff projects onto one `AuditEntry` (the typed `ChangeKind`, the version's baseline/revision content keys, the version's author and `Instant`), and the `EntryKey` chains on the prior entry's key through the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` codec so the log is tamper-evident — re-folding a tampered history yields a divergent terminal `EntryKey`; the fold is total, pure, no rail. `AuditTrail.For(string globalId)` folds every entry an element underwent into its lifecycle history in chain order, and `AuditTrail.Verify()` re-derives the chain to witness no retroactive edit broke it.
- Auto: `Fold` threads the `(Prior, Rows)` accumulator across the version sequence — for each version it folds the version's `ModelDiff.Changes` onto `AuditEntry` rows in change order (each carrying the element `GlobalId` and `ChangeKind` the change names through its base accessors, the version content keys decomposed by the one `Keys` switch, the version author/`Instant`, and the prior `EntryKey` as `ParentKey`), computes each entry's `EntryKey` as the seam `ContentAddress` over the prior key concatenated with the entry's canonical content, and threads the new key as the next entry's parent so the chain is a content-addressed Merkle-like sequence — the same content-key idiom the `Review/versioning#VERSION_GRAPH` `BimCommit.CommitKey` and the `csharp:Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` `AttestedEntry.Chain` carry; `For(globalId)` filters the folded entries to the element preserving chain order; `Verify` re-folds the entry contents recomputing each `EntryKey` from the recorded parent through the SAME `EntryKey` projection `Fold` used and compares the recomputed key against the stored one, so a single retroactive field edit diverges every downstream key and the boolean witnesses chain integrity without a separate stored checksum.
- Receipt: the `AuditTrail` chained `Seq<AuditEntry>` is the compliance evidence (who/when/from-what per element) the federation and compliance consumer read, `AuditTrail.For(globalId)` the per-element lifecycle history anchoring a `Review/issues#BCF_ARCHIVE` topic and the `Review/versioning#VERSION_GRAPH` merge, and `AuditTrail.Verify()` the tamper-evidence witness; the durable append-only residence is the `csharp:Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` concern joined at the `Review/diff → csharp:Rasm.Persistence/Version/provenance # [CONTENT_KEY]: AuditEntry chained ElementChange mutation log` seam by the content-key, this owner producing the chained host-neutral log and its content-key identity, the durable signed ledger riding the Persistence ripple.
- Packages: Rasm.Element, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm
- Growth: a new audit field is one column on `AuditEntry` folded into the `EntryKey` content; a new version-metadata dimension is one column on `AuditVersion`; a new lifecycle query is one fold over the same chained entry sequence; never a per-change-kind audit record, never a second mutation store, and never a checksum beside the chained `EntryKey`.
- Boundary: the audit trail keys on the `[02]-[MODEL_DIFF]` `ElementChange` and the version lineage, explicitly distinct from the Wave A tile-XMP geometry-asset provenance — the two stay separate, the audit trail never keying on the export artifact content-key; the chain is the content-addressed `EntryKey` through the seam `Projection/address#CANONICAL_WRITER` `CanonicalWriter` codec hashed by the `Projection/address#CONTENT_ADDRESS` `ContentAddress` (the kernel seed-zero `XxHash128`, the ONE hasher), and the retired hand-rolled `XxHash128.HashToUInt128(Encoding.UTF8.GetBytes($"..."))` string-interpolation chain plus a separate stored checksum or a mutable sequence-number beside the `EntryKey` are the deleted forms; the `ChangeKind` is the typed `[SmartEnum<string>]` projected by `ElementChange.Kind` and a stringly `"added"`/`"removed"` literal is the deleted form; the keys are `ContentAddress` values, never raw `UInt128`; the fold consumes the `[02]-[MODEL_DIFF]` `ModelDiff` change-sets as settled vocabulary and mints no second diff or element shape; the trail is a pure fold over the version-and-diff sequence, never an imperative append loop with mutable accumulation; the audit trail is the LINEAR per-element who/when/what Merkle chain, distinct from the `Review/versioning#VERSION_GRAPH` branching commit-DAG, neither re-derived from the other, both reading the one content-key space; the durable append-only residence is the `csharp:Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` concern joined at the content-key seam and a durable store minted here is the named seam violation; the fold is TOTAL and carries no fault rail — it consumes the `[02]-[MODEL_DIFF]` `ModelDiff` change-sets whose `GlobalId`s the diff already resolved from real graph nodes, so a dangling reference cannot arise at the audit boundary (the diff's `Bake` composition is the one place a corrupt subgraph rails `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault`), and a fabricated `Fin`/`BimFault` rail the body never produces is the illusory form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// The per-version authoring metadata a folded diff stamps onto each entry — the identity the chain binds.
public sealed record AuditVersion(string VersionId, string Author, Instant At);

// One immutable mutation-log row: the element GlobalId, the typed ChangeKind, the baseline/revision content
// addresses, the author/instant, and the chained EntryKey keyed on the prior row's key so a retroactive edit
// diverges every downstream key. Every key is a seam ContentAddress (the ONE codec), never a raw UInt128.
public readonly record struct AuditEntry(
    string GlobalId,
    ChangeKind Kind,
    ContentAddress BaselineKey,
    ContentAddress RevisionKey,
    string Author,
    Instant At,
    ContentAddress ParentKey,
    ContentAddress EntryKey);

public sealed record AuditTrail(Seq<AuditEntry> Entries) {
    public static readonly AuditTrail Empty = new(Seq<AuditEntry>());
    static readonly ContentAddress Genesis = ContentAddress.Of(UInt128.Zero);

    // Fold a version sequence of ModelDiff change-sets into the chained trail: each ElementChange projects onto one
    // AuditEntry whose EntryKey chains on the prior row's key, so re-folding a tampered history yields a divergent
    // terminal key. Pure fold over the version-and-diff sequence, never an imperative append loop.
    public static AuditTrail Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history) =>
        new(history.Fold((Prior: Genesis, Rows: Seq<AuditEntry>()), static (state, step) =>
            step.Diff.Changes.Fold(state, (acc, change) => {
                AuditEntry entry = Chain(acc.Prior, change, step.Version);
                return (entry.EntryKey, acc.Rows.Add(entry));
            })).Rows);

    // Every entry an element underwent in chain order — its lifecycle history (add -> modify -> move -> remove).
    public Seq<AuditEntry> For(string globalId) => Entries.Filter(entry => entry.GlobalId == globalId);

    // Re-derive the chain to witness no retroactive edit broke it: recompute each EntryKey from the recorded parent
    // through the SAME EntryKey projection Chain used, so a single edited field diverges every downstream key.
    public bool Verify() =>
        Entries.Fold((Prior: Genesis, Ok: true), static (state, entry) => (
            entry.EntryKey,
            state.Ok
                && EntryKey(state.Prior, entry.GlobalId, entry.Kind, entry.BaselineKey, entry.RevisionKey, entry.Author, entry.At) == entry.EntryKey
                && entry.ParentKey == state.Prior)).Ok;

    static AuditEntry Chain(ContentAddress prior, ElementChange change, AuditVersion version) {
        var (baseline, revision) = Keys(change);
        ContentAddress entryKey = EntryKey(prior, change.GlobalId, change.Kind, baseline, revision, version.Author, version.At);
        return new AuditEntry(change.GlobalId, change.Kind, baseline, revision, version.Author, version.At, prior, entryKey);
    }

    // The baseline/revision content addresses per arm: an Added has no baseline, a Removed no revision, a Modified its
    // two content keys, a Moved its two placement keys — the one Switch the audit row decomposes through.
    static (ContentAddress Baseline, ContentAddress Revision) Keys(ElementChange change) => change.Switch(
        added:    static c => (Genesis, c.Content),
        removed:  static c => (c.Content, Genesis),
        modified: static c => (c.BaselineContent, c.RevisionContent),
        moved:    static c => (c.BaselinePlacement, c.RevisionPlacement));

    // The content-addressed chain link through the seam ContentAddress codec (the kernel seed-zero XxHash128, the ONE
    // hasher) — the prior key, the element GlobalId, the typed kind, the key pair, and the authorship fold into one
    // canonical projection, so the chain is a Merkle sequence and a separate stored checksum is the deleted form.
    static ContentAddress EntryKey(ContentAddress prior, string globalId, ChangeKind kind, ContentAddress baseline, ContentAddress revision, string author, Instant at) {
        CanonicalWriter writer = new(0.0);
        writer.U128(prior.Value).String(globalId).String(kind.Key).U128(baseline.Value).U128(revision.Value)
            .String(author).U128((UInt128)unchecked((ulong)at.ToUnixTimeTicks()));
        return ContentAddress.Of(writer.ToBytes().Span);
    }
}
```

## [04]-[RESEARCH]

- [FEDERATION_JOIN]: the federation key is the Bim-stored `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` (the compressed IFC `GlobalId` re-emitted at `Emit` [H6]) and NOT the neutral kernel `NodeId`, grounded against `ELEMENT-REBUILD-PLAN.md` §4-RT H6 (rooted `NodeId` is a neutral kernel-minted id; the IFC `GlobalId` is a Bim-stored projection attribute) and the `Review/coordination#COORDINATION` identity law (an IFC-semantic receipt keys on the `ExternalId` the BCF/schedule/cost targets demand, the `NodeId → ExternalId` projection through `Node.Object.ExternalId`, the `ExternalId` `Choose`-discard-`None` idiom dropping an authored Object off the federation surface) — two independent submissions re-mint rooted `NodeId`s per `Exchange/wire#WIRE_PROJECTION` ("a re-admitted wire re-mints rooted ids"), so only the IFC `GlobalId` is cross-party stable; the `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` `Reconcile` instead aligns a re-ingest onto the durable `NodeId` BY the `ExternalId` for the same-lineage three-way merge, the disjoint bounded context this pairwise diff never re-derives.
- [SHARED_CONTENT_CODEC]: the content and placement keys compose the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress.Of(node, tolerance)`/`Of(ReadOnlySpan<byte>)`/`Of(UInt128)` over the one `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` (`String`/`U128`/`Ordinal`/`Bool`/`Double`/`Raw`/`ToBytes`) and the bound nodes fold their own `ContentAddress.Of`, so the diff content bytes and the `NodeId.Content` hash share the one projection [H7] and the seam mints no second hasher — the retired `csharp:Rasm.Compute/Runtime/codecs` `InterchangeIdentity.Key` (up-stratum) and the hand-rolled `XxHash128`/`Encoding.UTF8` string-join are the deleted forms; the content/placement split (content = the `Object` semantic head + the bound `PropertySet`/`QuantitySet`/`Material`/`Assessment`/`Appearance`/`Coverage` node addresses + outgoing-edge structure; placement = the `RepresentationContentHash` map ALONE, EVERY geometry content-hashed there — the display `Body` AND the analytical `Axis`/`FootPrint` content keys, never an inline coordinate read) mirrors the `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` geometry-axis-versus-content-axis split (`Object.Representations.Body` vs `XxHash128` over `Node.ToCanonicalBytes`), distinguishing a relocation (placement moved) from a content edit (content moved).
- [STRUCTURED_DELTA]: the `Modified` arm carries the `Generator.Equals` `Element.EqualityComparer.Default.Inequalities(baseline, revision)` member diff (`Inequality.Path` `MemberPath` dotted/bracketed, `.Left`/`.Right` old→new) projected onto `AspectDelta`, the `Id`/`ExternalId`/`History` and the nested `Parts` segments filtered (the rooted nested `NodeId` is a fresh Guid per ingest so a `Parts` drill is cross-party noise, each part diffed independently by its own `GlobalId`; the bound `Material`/`PropertySet`/`Quantity` nodes are content-addressed and stable, so they are NOT filtered) — the SAME `Inequalities` substrate the `csharp:Rasm.Persistence/Version/merge#STRUCTURAL_DIFF` three-way merge reads (`libs/csharp/.api/api-generator-equals` `Inequalities`/`MemberPath`/`MemberPathSegment`), so a BCF topic anchors the exact `Properties[Pset].FireRating` member that moved rather than an opaque content-key delta; the `Element` baked through `Bake` is `[Equatable]` so its `EqualityComparer.Default` carries both the structural compare and the member diff, the bake-only-changed-elements discipline preserving the incremental federation cost.
- [AUDIT_CHAIN]: the `AuditEntry.EntryKey` chains through the seam `ContentAddress` the same content-addressed idiom the `Review/versioning#VERSION_GRAPH` `BimCommit.CommitKey` and the `csharp:Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` `AttestedEntry(ContentKey, Prior, Chain, …)` carry (`Append` rolling `XxHash128` over `Prior ++ ContentKey`), so the linear per-element audit chain, the branching commit-DAG, and the durable signed ledger read the one content-key space; the audit trail produces the host-neutral chain and its content-key identity, the durable append-only signed residence riding the `csharp:Rasm.Persistence/Version/provenance#ATTESTED_LEDGER` ripple, the `Verify` re-fold the tamper-evidence witness with no stored checksum.
- [WIRE_POSTURE]: the `ModelDiff.Encode`/`Decode` cross-runtime payload rides the shared `libs/csharp/.api/api-thinktecture-json` `ThinktectureJsonConverterFactory` (admitting the `[Union]` `ElementChange`, the `[SmartEnum]` `ChangeKind`, the `[ComplexValueObject]` `Classification`, and the `[ValueObject<UInt128>]` `ContentAddress` with their generated discriminants/key conversion), so a TS decode switches on the `ChangeKind` token — the retired `Exchange/wire#WIRE_PROJECTION` `BimWireContext`/`BimWireOptions.Json` are GONE (the strata-leaking generic-model serializer is retired, `wire.md` now owning the `IfcWire` IFC interchange wire; the seam-graph snapshot wire is `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS`'s) and the parallel `DiffWire` record duplicating `ModelDiff`'s `(Seq<ElementChange>, int)` shape is collapsed onto `ModelDiff.Encode`/`Decode`; the `ContentAddress` crosses through its `[ValueObject<UInt128>]` factory carrying the SAME `UInt128` content-key value the `NodeId.Content` mint renders `"X32"`-hex, so a TS peer decodes the one seam-owned content-key, never a second representation.
