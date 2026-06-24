# [PERSISTENCE_PROVENANCE]

Rasm.Persistence owns lineage as a first-class join dimension over the op-log changefeed, separating two hash domains by purpose: `XxHash128` keys identity (content keys, PROV vertices) and SHA-256 carries integrity (the attested chain, the RFC 9162 transparency tree). `ProvEdge` is the W3C-PROV-DM causal influence DAG relating agents, activities, and entities; `ProvNode` is the resolved vertex the slice materializes; `LineageSlice` folds backward and forward slices, projecting blame agents and impact entities directly; `AttestedEntry` SHA-256 hash-chains every op-log row plus its pgaudit category into a tamper-evident ledger whose head digest cryptographically commits the whole chain; `TransparencyProof` builds the RFC 9162 SHA-256 Merkle tree so an inclusion or consistency proof verifies in O(log n) against a detached `ECDsa`-signed tree head; and `LineageCdc` filters the change feed by lineage scope so a redaction-preserving feed emits only the slice a consumer is authorized to see. The op-log changefeed (`OpLogEntry`, HLC stamp, `ContentKey`, `Closure`, `Trace`), the content-address identity (`XxHash128`), the export-proof redaction (`ExportProof`, `RedactorKind`), and the pgaudit category binding (`AuditBinding`) arrive settled and compose inside the fences; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `DataClassification` arrive from AppHost.

The cryptographic boundary is load-bearing and inverts the prior page. `XxHash128` is a non-cryptographic identity hash whose rail law (`api-hashing.md` `[RAIL_LAW]`) forbids any tamper-evidence claim; a hash-chain or Merkle log built on it is forgeable by second-preimage and proves nothing to an adversary. So every attestation digest is `SHA256` over the same tuple `XxHash128` would have keyed, the chain digest and the Merkle root are 32-byte SHA-256 outputs, and the `ECDsa` head-seal signs a cryptographic root — non-repudiation binds to a root that genuinely commits the history. Content identity stays `XxHash128` because a content key describes "which bytes", never "untampered".

## [01]-[INDEX]

- [01]-[CAUSAL_DAG]: W3C-PROV-DM agent/activity/entity influence edges over the op-log; node-resolving slice fold.
- [02]-[ATTESTED_LEDGER]: SHA-256 hash-chained attested log, RFC 9162 transparency tree, redaction-preserving leaf, and signed tree head.
- [03]-[LINEAGE_CDC]: lineage-scoped change-data-capture; redaction-aware feed carrying redactor evidence.
- [04]-[TS_PROJECTION]: provenance edge, node, slice, attestation, proof, and CDC envelope wire shapes.

## [02]-[CAUSAL_DAG]

- Owner: `ProvEdge` `[Union]` the W3C-PROV-DM causal influence family; `ProvNode` `[Union]` the agent/activity/entity vertex the slice resolves; `LineageSlice` the node-and-edge slice result projecting blame/impact; `ReplaySlice` the HLC-ordered derivation sequence of `ReplayStep` rows; `Provenance` the static surface owning edge emission over the op-log row, node resolution, backward/forward slicing, blame attribution, impact analysis, and deterministic replay.
- Cases: nine `ProvEdge` rows — the six PROV core relations `WasGeneratedBy | Used | WasDerivedFrom | WasAttributedTo | WasAssociatedWith | WasInformedBy` plus the three qualified specializations the op-log already carries as data: `WasInvalidatedBy` (a tombstone op destroys an entity), `WasRevisionOf` (an update's before-`Image` is the prior version of the same entity, a revision rather than a fresh cross-entity derivation), and `ActedOnBehalfOf` (agent delegation); `ProvNode` is `Agent | Activity | Entity`. The closed vocabulary is the W3C-PROV-DM influence set so the lineage interoperates with any PROV-consuming tool, and a non-standard relation is the named defect.
- Entry: `public static Seq<ProvEdge> Edges(OpLogEntry op, Option<(Guid Store, string Actor)> applied = default)` projects every causal edge one op-log row implies, discriminating on `op.Kind.Tombstone` so a delete emits `WasInvalidatedBy` (never `WasGeneratedBy`) and on a non-empty before-`Image` so an update emits `WasRevisionOf` keyed by the image content rather than treating its prior version as a foreign derivation, and admitting the optional APPLYING agent `(Store, Actor)` the replication seam supplies so a replayed remote op whose applying store differs from `op.OriginStoreId` emits one `ActedOnBehalfOf(applyingAgent, authoringAgent)` — a single op-log row carries exactly one authoring `(OriginStoreId, Actor)`, so delegation is NOT self-derivable from the row and is admitted once at the federation boundary, never fabricated from a field comparison; `public static LineageSlice Slice(Func<UInt128, Seq<ProvEdge>> adjacency, Func<UInt128, Option<ProvNode>> resolve, UInt128 root, LineageDirection direction, int depth)` walks the influence DAG through one immutable frontier fold and resolves each reached vertex into a typed `ProvNode`, the `LineageDirection.Next` endpoint selector carrying the backward/forward choice as a SmartEnum delegate rather than a mode branch; `public static ReplaySlice Replay(Func<UInt128, Option<OpLogEntry>> op, LineageSlice backward)` reconstructs an entity's derivation by re-resolving the backward slice's `Activities` to their op-log rows and folding them in `(Physical, Logical)` HLC order into one `ReplayStep` sequence — each step naming the activity, the produced entity, the consumed `Closure` inputs, the revised prior image, and the invalidation flag — so two peers replaying the same DAG converge on one byte-identical step list.
- Auto: every op-log row implies its provenance with zero extra write — the op `Actor`/`OriginStoreId` is the agent, the op itself (keyed by its full HLC cell and origin) is the activity, the `ContentKey` is the generated-or-invalidated entity, the `Image` before-image is the prior version of the same entity (`WasRevisionOf`), each `Closure` source is a distinct input entity the activity `Used` and the new entity `WasDerivedFrom`, the op `WasInformedBy` the activities those inputs name, and a replayed remote op whose APPLYING agent (the `applied` context the replication pump supplies at the federation seam) differs from the authoring `OriginStoreId` emits `ActedOnBehalfOf(applyingAgent, authoringAgent)` (the delegation the replication act creates) — so the causal DAG is a projection over the existing changefeed, never a second store. Lineage is a join dimension: a federated search (`Query/federation#FUSION_RANK`) carries each hit's provenance head, a retention sweep holds a row whose lineage is under legal hold, and a sync scope filters by lineage subtree; blame is the backward slice resolving to the originating agents (`slice.Agents`), impact is the forward slice resolving to every derived entity (`slice.Entities`), and deterministic replay (`Replay`) re-resolves the backward slice's `Activities` to their op-log rows and folds them in `(Physical, Logical)` HLC order into one `ReplayStep` sequence, reproducing the entity's derivation as a peer-stable, byte-identical step list rather than a re-executed write path.
- Receipt: a slice rides `store.lineage.slice` carrying the direction, the depth reached, the vertex count, and the blame/impact node counts; a replay rides `store.lineage.replay` carrying the root, the `ReplaySlice.StepCount`, and the HLC span of the first and last step.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new PROV relation is one `ProvEdge` case; a new vertex kind is one `ProvNode` case; a new slice direction is one `LineageDirection` row carrying its own `Next` endpoint selector so a direction never grows a mode branch in the walk; the directed `(Tail, Head)` endpoints derive from one `Endpoints` algebra and `LineageDirection.Next` selects the followed endpoint from that pair, so a new edge case adds one `Endpoints` switch arm and zero direction code, and the forward-reachability projection is `LineageDirection.Forward` over the same pair rather than a parallel downstream-projection helper; a new replay dimension is one field on `ReplayStep`; zero new surface — a separate lineage store, a trigger-emitted lineage table, or a per-operation provenance logger is the deleted form because every edge projects from the op-log row the changefeed already commits, the slice is one bounded immutable frontier fold over the projected adjacency, and replay re-resolves the same activity vertices the slice already reached.
- Boundary: the causal DAG is a pure projection of the op-log so it carries no second write path — a CDC trigger building a lineage table or a per-op provenance call is the deleted form; backward slicing answers "what produced this" (blame, audit) and forward slicing answers "what does this affect" (impact analysis), both bounded breadth-first walks over the projected adjacency so the cost is linear in the reachable-edge count within the depth bound; every vertex is `XxHash128`-keyed so the slice walk is uniform — the activity key is `XxHash128` over the full `(Physical, Logical, OriginStoreId)` HLC-and-origin tuple (the same cross-peer-stable triple the merge law and the commit graph identify ops by, so two peers replaying the same op converge on one activity vertex), the agent key is `XxHash128` over `(OriginStoreId, Actor)` so an agent is a first-class walk vertex and `slice.Agents` populates from the backward slice rather than a string the walk cannot reach, and the actor stays an attribution edge (`WasAssociatedWith`/`WasAttributedTo`), never an entity-identity component; `ActedOnBehalfOf` is the delegation edge whose delegate and responsible parties are TWO distinct agents — derivable only when the applying-agent context is known, so `Edges` mints it solely from the `applied` federation context (`AgentKey(applyingActor, applyingStore)` acting on behalf of the authoring `agent`) and never from a single row's own fields, because a lone op-log row exposes exactly one agent and a name-versus-origin-string comparison that always differs is the illusory form; a tombstone op `WasInvalidatedBy` its activity so a lineage query answers "what destroyed this", and an update carrying a before-`Image` of the same entity emits `WasRevisionOf` keyed by the image content (the prior version of that entity) so the version chain reads as revisions while the `Closure` inputs stay `WasDerivedFrom` cross-entity derivations; lineage is the join dimension every reuse surface composes — the federated entity carries a provenance head (`Query/federation#ENTITY_GRAPH`), the structural-diff blame (`Version/timetravel#TIME_TRAVEL`) reads the same winning op the lineage attributes, and the retention sweep's legal hold reads the lineage subtree — so a lineage absent from a query result is a gap, not a feature; the derivation source of an entity is the prior content key in its `Closure` manifest so the DAG rides the existing content-address closure, never a re-derived dependency graph; the slice resolves vertices through the `resolve` delegate so a `LineageSlice` carries typed `ProvNode` values and projects `Agents`/`Activities`/`Entities` for the caller, never a bare edge bag the consumer must re-walk.

```csharp signature
public sealed class ProvenanceKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum]
public sealed partial class LineageDirection {
    public static readonly LineageDirection Backward = new(static endpoints => endpoints.Tail);
    public static readonly LineageDirection Forward = new(static endpoints => endpoints.Head);

    [UseDelegateFromConstructor]
    public partial Option<UInt128> Next((Option<UInt128> Tail, Option<UInt128> Head) endpoints);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvNode {
    private ProvNode() { }

    public sealed record Agent(UInt128 Key, string Actor, Guid Origin) : ProvNode;
    public sealed record Activity(UInt128 Key, Instant Physical, ulong Logical, Guid Origin) : ProvNode;
    public sealed record Entity(UInt128 ContentKey, string EntityKind, string EntityKey) : ProvNode;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvEdge {
    private ProvEdge() { }

    public sealed record WasGeneratedBy(UInt128 Entity, UInt128 Activity) : ProvEdge;
    public sealed record Used(UInt128 Activity, UInt128 Entity) : ProvEdge;
    public sealed record WasDerivedFrom(UInt128 Entity, UInt128 Source) : ProvEdge;
    public sealed record WasRevisionOf(UInt128 Entity, UInt128 Source) : ProvEdge;
    public sealed record WasInvalidatedBy(UInt128 Entity, UInt128 Activity) : ProvEdge;
    public sealed record WasAttributedTo(UInt128 Entity, UInt128 Agent) : ProvEdge;
    public sealed record WasAssociatedWith(UInt128 Activity, UInt128 Agent) : ProvEdge;
    public sealed record WasInformedBy(UInt128 Activity, UInt128 Informant) : ProvEdge;
    public sealed record ActedOnBehalfOf(UInt128 Delegate, UInt128 Responsible) : ProvEdge;
}

public readonly record struct LineageSlice(
    UInt128 Root, LineageDirection Direction, Seq<ProvEdge> Edges, Seq<ProvNode> Nodes, int Depth) {
    public Seq<ProvNode.Agent> Agents => Nodes.Choose(static n => n is ProvNode.Agent a ? Some(a) : Option<ProvNode.Agent>.None);
    public Seq<ProvNode.Activity> Activities => Nodes.Choose(static n => n is ProvNode.Activity a ? Some(a) : Option<ProvNode.Activity>.None);
    public Seq<ProvNode.Entity> Entities => Nodes.Choose(static n => n is ProvNode.Entity e ? Some(e) : Option<ProvNode.Entity>.None);
    public int VertexCount => Nodes.Count;
}

public readonly record struct ReplayStep(
    ProvNode.Activity Activity, UInt128 Produced, Seq<UInt128> Consumed, Option<UInt128> Revised, bool Invalidated);

public readonly record struct ReplaySlice(UInt128 Root, Seq<ReplayStep> Steps) {
    public int StepCount => Steps.Count;
}

public static class Provenance {
    public const int MaxSliceDepth = 64;

    public static Seq<ProvEdge> Edges(OpLogEntry op, Option<(Guid Store, string Actor)> applied = default) {
        UInt128 activity = ActivityKey(op);
        UInt128 agent = AgentKey(op.Actor, op.OriginStoreId);
        Seq<ProvEdge> head = op.Kind.Tombstone
            ? [new ProvEdge.WasInvalidatedBy(op.ContentKey, activity)]
            : op.Image.IsEmpty
                ? [new ProvEdge.WasGeneratedBy(op.ContentKey, activity)]
                : [new ProvEdge.WasGeneratedBy(op.ContentKey, activity),
                   new ProvEdge.WasRevisionOf(op.ContentKey, XxHash128.HashToUInt128(op.Image.Span))];
        return [
            .. head,
            new ProvEdge.WasAssociatedWith(activity, agent),
            new ProvEdge.WasAttributedTo(op.ContentKey, agent),
            .. applied is { IsSome: true, Case: (Guid store, string actor) } && store != op.OriginStoreId
                ? [new ProvEdge.ActedOnBehalfOf(AgentKey(actor, store), agent)]
                : Seq<ProvEdge>(),
            .. op.Closure.Bind(source => Seq<ProvEdge>(
                new ProvEdge.Used(activity, source),
                new ProvEdge.WasDerivedFrom(op.ContentKey, source),
                new ProvEdge.WasInformedBy(activity, source)))];
    }

    public static UInt128 AgentKey(string actor, Guid origin) {
        Span<byte> span = stackalloc byte[16 + 64];
        origin.TryWriteBytes(span);
        int written = 16 + Encoding.UTF8.GetBytes(actor, span[16..]);
        return XxHash128.HashToUInt128(span[..written]);
    }

    public static LineageSlice Slice(
        Func<UInt128, Seq<ProvEdge>> adjacency, Func<UInt128, Option<ProvNode>> resolve,
        UInt128 root, LineageDirection direction, int depth) {
        Walk walk = Expand(
            adjacency, direction, int.Min(depth, MaxSliceDepth),
            new Walk([], LanguageExt.HashSet(root), Seq((Key: root, Depth: 0)), 0));
        return new LineageSlice(root, direction, walk.Edges, toSeq(walk.Seen).Choose(resolve), walk.Depth);
    }

    public static ReplaySlice Replay(Func<UInt128, Option<OpLogEntry>> op, LineageSlice backward) =>
        new(backward.Root, toSeq(backward.Activities
            .Choose(node => op(node.Key).Map(row => (Node: node, Row: row)))
            .OrderBy(static step => (step.Node.Physical, step.Node.Logical)))
            .Map(static step => new ReplayStep(
                step.Node,
                Produced: step.Row.ContentKey,
                Consumed: step.Row.Closure,
                Revised: step.Row.Image.IsEmpty ? None : Some(XxHash128.HashToUInt128(step.Row.Image.Span)),
                Invalidated: step.Row.Kind.Tombstone)));

    private readonly record struct Walk(
        Seq<ProvEdge> Edges, LanguageExt.HashSet<UInt128> Seen, Seq<(UInt128 Key, int Depth)> Frontier, int Depth);

    private static Walk Expand(Func<UInt128, Seq<ProvEdge>> adjacency, LineageDirection direction, int bound, Walk state) =>
        state.Frontier.Match(
            Empty: () => state,
            More: (node, rest) => Expand(adjacency, direction, bound,
                (node.Depth >= bound
                    ? state with { Frontier = rest }
                    : adjacency(node.Key).Fold(
                        state with { Frontier = rest },
                        (acc, edge) => direction.Next(edge.Endpoints()) is { IsSome: true, Case: UInt128 key } && !acc.Seen.Contains(key)
                            ? acc with { Edges = acc.Edges.Add(edge), Seen = acc.Seen.Add(key), Frontier = acc.Frontier.Add((key, node.Depth + 1)) }
                            : acc with { Edges = acc.Edges.Add(edge) }))
                with { Depth = int.Max(state.Depth, node.Depth) }));

    private static UInt128 ActivityKey(OpLogEntry op) {
        Span<byte> span = stackalloc byte[32];
        BinaryPrimitives.WriteInt64LittleEndian(span, op.Physical.ToUnixTimeTicks());
        BinaryPrimitives.WriteUInt64LittleEndian(span[8..], op.Logical);
        op.OriginStoreId.TryWriteBytes(span[16..]);
        return XxHash128.HashToUInt128(span);
    }
}

public static class ProvEdgeExtensions {
    extension(ProvEdge edge) {
        public (Option<UInt128> Tail, Option<UInt128> Head) Endpoints() =>
            edge switch {
                ProvEdge.WasGeneratedBy g    => (Some(g.Activity), Some(g.Entity)),
                ProvEdge.Used u              => (Some(u.Entity), Some(u.Activity)),
                ProvEdge.WasDerivedFrom d    => (Some(d.Source), Some(d.Entity)),
                ProvEdge.WasRevisionOf r     => (Some(r.Source), Some(r.Entity)),
                ProvEdge.WasInvalidatedBy v  => (Some(v.Activity), Some(v.Entity)),
                ProvEdge.WasAttributedTo a   => (Some(a.Agent), Some(a.Entity)),
                ProvEdge.WasAssociatedWith a => (Some(a.Agent), Some(a.Activity)),
                ProvEdge.WasInformedBy i     => (Some(i.Informant), Some(i.Activity)),
                ProvEdge.ActedOnBehalfOf d   => (Some(d.Responsible), Some(d.Delegate)),
            };
    }
}
```

## [03]-[ATTESTED_LEDGER]

- Owner: `Digest` the 32-byte SHA-256 attestation value the chain and tree carry; `AttestedEntry` the hash-chained ledger row; `LedgerHead` the chain-head proof; `AttestedLedger` the static surface owning the SHA-256 chain-append fold, the head-digest computation, the chain verification, and the redaction-preserving entry projection; `TransparencyProof` the RFC 9162 Merkle transparency-log surface owning the leaf/node digest, the `InclusionProof` for one entry, the `ConsistencyProof` between two `LedgerHead` seals, and the detached-signed `HeadSeal` an external auditor verifies in O(log n) without replaying the chain.
- Cases: each entry chains `PriorDigest` into its own `Digest = SHA256(PriorDigest ‖ ContentKey ‖ ProviderFingerprint ‖ Classification ‖ AuditCategory)`, so the head digest cryptographically commits every prior entry and a graduated-surrogate entry's `(checksum, OrtEpDevice)` provider fingerprint binds into the chain; a non-graduation row folds `UInt128.Zero` so the fingerprint term is uniform; a redacted entry preserves its `Digest` and that preserved digest is the SOLE leaf preimage, so redaction leaves the Merkle root and every prior inclusion proof unchanged while the `RedactedPayloadProof` rides the entry for the audit narrative only; `TransparencyProof` builds an RFC 9162 SHA-256 Merkle tree over the `AttestedEntry` leaf digests so `Prove`/`Verify` prove one entry was logged in O(log n) (the `Verify` recompute is the canonical RFC 9162 §2.1.1 `(fn, sn)` path fold), `Consistent`/`ConsistentWith` prove the new tree extends the old append-only between two `HeadSeal` sizes (the §2.1.2 dual-root fold), and the `HeadSeal` binds an `ECDsa` detached signature over the `(LogId, TreeSize, Root, At)` signed-tree-head so non-repudiation attaches to a root that cryptographically commits the history.
- Entry: `public static AttestedEntry Append(LedgerHead head, OpLogEntry op, string auditCategory, DataClassification classification, Option<UInt128> providerFingerprint = default)` — pure chain-append SHA-256-linking the prior head digest into the new entry over the op content key, audit category, and classification; `public static Fin<LedgerHead> Verify(Seq<AttestedEntry> chain, LedgerHead start)` recomputes every digest and aborts on the first break. On the transparency face `TransparencyProof.Prove`/`Verify` and `Consistent`/`ConsistentWith` are the symmetric build-and-check pairs an external auditor runs against a detached `HeadSeal` — every emitted proof has a verifier, so the inclusion and consistency claims are checkable off-server, never builder-only.
- Auto: every op-log row plus its bound pgaudit category (`AuditBinding.Bind`) appends one `AttestedEntry` whose SHA-256 digest chains the prior digest, so the head digest is a tamper-evident cryptographic proof of the whole history — a single altered prior row breaks every subsequent digest with cryptographic certainty and `Verify` names the first break; the chain rides the op-log so it carries no second store; a graduated offline-science surrogate enters as one `AttestedEntry` keyed by its `ContentIdentity` carrying the `(checksum, OrtEpDevice)` provider fingerprint in the dedicated `ProviderFingerprint` field, so the `ONE_GRADUATION_EVIDENCE` `HandoffAxis` result chains into the tamper-evident ledger and the determinism closure (`AppHost/Runtime/determinism#EVENT_LOG`) references one durable content key, the surrogate ONNX bytes residing on the `Query/cache#ARTIFACT_BLOB_INDEX` `GraduationAsset` row; a re-import re-verifies the fingerprint at admission so a provider-divergent fit is caught before inference; redaction is preserving — a redacted entry keeps its original `Digest` (computed over the pre-redaction content key) and only adds the `ExportProof` content hash to its `RedactedPayloadProof` slot, leaving the leaf preimage (the preserved `Digest`) and therefore the Merkle root untouched, so an inclusion proof minted before the redaction still verifies against the same head after it and an auditor proves the row existed without seeing the redacted content; the head digest is periodically sealed as a content-addressed snapshot (`Snapshots.ContentAddress`) so an external witness anchors the chain.
- Receipt: an append rides `store.ledger.append` carrying the new head digest; a verification rides `store.ledger.verify` carrying the verified count or the first break index; a redaction-preserving projection rides the settled `ExportProof`.
- Packages: System.IO.Hashing, System.Security.Cryptography (BCL inbox), NodaTime, LanguageExt.Core, Microsoft.Extensions.Compliance.Redaction, Rasm.AppHost (project), BCL inbox.
- Growth: a new attestation field is one term in the SHA-256 tuple AND one matching `AttestedEntry` field AND one symmetric write in both `Append` and `Recompute` (a one-sided edit breaks `Verify` on the first entry); the `ProviderFingerprint` graduation term is exactly this — a dedicated `Option<UInt128>` field plus the digest term plus the symmetric buffer growth, never the `RedactedPayloadProof` slot whose reuse would overwrite a graduated surrogate's fingerprint when that entry is later redacted; extending the digest tuple is a FORWARD-ONLY ledger format epoch with a genesis-anchored re-seal, never a retroactive rewrite of existing attested entries; a new seal cadence is one schedule policy value; the transparency log is a projection over the existing `AttestedEntry` rows so an inclusion or consistency proof is one `TransparencyProof` fold, never a second ledger store; zero new surface — a separate Merkle-tree audit store, a blockchain ledger, a sibling graduation-evidence record, or a second tamper-evident log is the deleted form because the chain is one SHA-256 hash-chained projection over the op-log and the pgaudit binding, the Merkle tree is one projection over the same leaf digests, and the head digest plus the detached-signed seal is the one external proof; the existing pgaudit/legal-hold/RLS/CDC machinery (`redaction-retention`, `server-tier`) stays the substrate and this owner adds the SHA-256 hash-chain attestation, the lineage scoping, the RFC 9162 transparency proof, and the graduation-evidence chaining net-new.
- Boundary: the chain is SHA-256 hash-linked so tamper-evidence is cryptographic — `Digest_n = SHA256(Digest_{n-1} ‖ ContentKey ‖ ProviderFingerprint ‖ Classification ‖ AuditCategory)` so altering any entry invalidates every later digest under collision resistance and `Verify` returns the first break index, never a silent pass; SHA-256 (not `XxHash128`) is the digest algorithm because `api-hashing.md` `[RAIL_LAW]` rejects a tamper-evidence claim on a non-cryptographic hash, and a forgeable chain root would defeat the whole attestation — the `XxHash128` content key the entry carries is identity, the SHA-256 chain digest is integrity; the `ProviderFingerprint` term is a DEDICATED `Option<UInt128>` field folding `UInt128.Zero` when `None`, never the `RedactedPayloadProof` slot — the two slots carry orthogonal facts (a graduation fingerprint that binds into the chain digest versus a redaction reference that does not), so aliasing them would overwrite a graduated surrogate's fingerprint the instant that entry is redacted; the digest extension grows the `Append`/`Recompute` `stackalloc byte[128]` tuple buffer (32 prior digest + 16 content key + 16 fingerprint fixed prefix, then the UTF8 `classification|category` tail in the 64-byte headroom) and BOTH `Append` and `Recompute` write the identical tuple layout or `Verify` breaks on the first entry; the tuple-term addition is a FORWARD-ONLY format epoch with a genesis-anchored re-seal, never a retroactive rewrite — existing attested entries keep their original digests under the prior epoch and the new epoch chains forward from a re-sealed `LedgerHead`; the Merkle leaf and node hash are SHA-256 with the RFC 9162 `0x00` leaf-prefix and `0x01` node-prefix domain separation so a leaf digest can never collide with an interior node, and the `HeadSeal` `ECDsa` detached signature binds the `(LogId, TreeSize, Root, At)` signed-tree-head so the non-repudiation claim attaches to a root that cryptographically commits the chain and the `LogId` distinguishes one store's log from another in a federation; `TransparencyProof.Prove` emits an `InclusionProof` whose `AuditPath` `Verify` walks in O(log n) by the canonical RFC 9162 §2.1.1 `(fn, sn)` recompute to rebuild the `Root` without the chain — the prior `Climb`-fold spelling silently rejected every non-power-of-two tree and is the deleted form — and `Consistent` emits a `ConsistencyProof` that `ConsistentWith` checks by the §2.1.2 dual-root recompute so a split-history attack surfaces as a failed extension proof between two `HeadSeal` sizes rather than a builder no auditor can challenge; the support-bundle export carries both proofs through `Version/retention.md#EXPORT_PROOF` as self-verifying artifacts rather than a server-trusted digest, never a second ledger store; the leaf preimage is the entry's preserved 32-byte `Digest` ALONE (RFC `0x00`-prefixed) and carries NO mutable payload reference, so redaction — which only sets `RedactedPayloadProof` — shifts no leaf, holds the Merkle root fixed, and leaves every prior inclusion proof valid, matching the chain invariant; folding the redaction reference into the leaf is the deleted form because it would move the root on every redaction and break the proofs the page promises; external non-repudiation rides the periodic detached-signed `HeadSeal` (its `Root` is the Merkle root and its `At` rides `Snapshots.ContentAddress` as the content-addressed head-seal anchor) an outside witness anchors; the pgaudit category per entry comes from the settled `AuditBinding.Bind` (`Version/retention#AUDIT_BINDING`) so the attested ledger and the server-side audit log carry the same category; legal hold reads the lineage subtree (`#CAUSAL_DAG`) so a held entry's whole derivation chain is retained; the chain rides the op-log changefeed so it converges across peers and a fork in the chain (two entries claiming the same prior digest) is a typed conflict the merge surfaces, never a silent overwrite.

```csharp signature
public readonly record struct AttestedEntry(
    long Sequence,
    UInt128 ContentKey,
    ReadOnlyMemory<byte> PriorDigest,
    ReadOnlyMemory<byte> Digest,
    DataClassification Classification,
    string AuditCategory,
    Option<UInt128> ProviderFingerprint,
    Option<UInt128> RedactedPayloadProof,
    Instant At);

public readonly record struct LedgerHead(long Sequence, ReadOnlyMemory<byte> Digest, Instant At) {
    public bool ChainsTo(AttestedEntry next) => next.PriorDigest.Span.SequenceEqual(Digest.Span);
}

public static class AttestedLedger {
    public static readonly LedgerHead Genesis = new(0L, new byte[SHA256.HashSizeInBytes], Instant.MinValue);

    public static AttestedEntry Append(LedgerHead head, OpLogEntry op, string auditCategory, DataClassification classification, Option<UInt128> providerFingerprint = default) =>
        new(head.Sequence + 1L, op.ContentKey, head.Digest,
            Digest(head.Digest.Span, op.ContentKey, providerFingerprint, classification, auditCategory),
            classification, auditCategory, providerFingerprint, None, op.Physical);

    public static AttestedEntry Redact(AttestedEntry entry, ExportProof proof) =>
        entry with { RedactedPayloadProof = Some(proof.ContentHash) };

    public static LedgerHead Seal(Seq<AttestedEntry> appended, LedgerHead prior) =>
        appended.LastOrNone().Match(
            Some: last => new LedgerHead(last.Sequence, last.Digest, last.At),
            None: () => prior);

    public static Fin<LedgerHead> Verify(Seq<AttestedEntry> chain, LedgerHead start) =>
        chain.Fold(
            Fin.Succ(start),
            static (acc, entry) => acc.Bind(head =>
                !head.ChainsTo(entry)
                    ? Fin.Fail<LedgerHead>(Error.New($"<ledger-chain-break:{entry.Sequence}>"))
                    : !Recompute(head.Digest.Span, entry).Span.SequenceEqual(entry.Digest.Span)
                        ? Fin.Fail<LedgerHead>(Error.New($"<ledger-digest-mismatch:{entry.Sequence}>"))
                        : Fin.Succ(new LedgerHead(entry.Sequence, entry.Digest, entry.At))));

    private static ReadOnlyMemory<byte> Recompute(ReadOnlySpan<byte> prior, AttestedEntry entry) =>
        Digest(prior, entry.ContentKey, entry.ProviderFingerprint, entry.Classification, entry.AuditCategory);

    private static ReadOnlyMemory<byte> Digest(ReadOnlySpan<byte> prior, UInt128 contentKey, Option<UInt128> fingerprint, DataClassification classification, string auditCategory) {
        Span<byte> tuple = stackalloc byte[SHA256.HashSizeInBytes + 32 + 64];
        prior.CopyTo(tuple);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[32..], contentKey);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[48..], fingerprint.IfNone(UInt128.Zero));
        int written = 64 + Encoding.UTF8.GetBytes($"{classification.Key}|{auditCategory}", tuple[64..]);
        byte[] digest = new byte[SHA256.HashSizeInBytes];
        SHA256.HashData(tuple[..written], digest);
        return digest;
    }
}

public readonly record struct InclusionProof(int LeafIndex, int TreeSize, ReadOnlyMemory<byte> LeafDigest, Seq<ReadOnlyMemory<byte>> AuditPath, ReadOnlyMemory<byte> Root);

public readonly record struct ConsistencyProof(int OldSize, int NewSize, Seq<ReadOnlyMemory<byte>> Path, ReadOnlyMemory<byte> OldRoot, ReadOnlyMemory<byte> NewRoot);

public readonly record struct HeadSeal(UInt128 LogId, int TreeSize, ReadOnlyMemory<byte> Root, Instant At, ReadOnlyMemory<byte> Signature);

public static class TransparencyProof {
    public static ReadOnlyMemory<byte> LeafDigest(AttestedEntry entry) {
        Span<byte> span = stackalloc byte[1 + SHA256.HashSizeInBytes];
        span[0] = 0x00;
        entry.Digest.Span.CopyTo(span[1..]);
        return Sha(span);
    }

    public static ReadOnlyMemory<byte> Root(Seq<AttestedEntry> chain) =>
        MerkleRoot(toSeq(chain.Map(LeafDigest)));

    public static InclusionProof Prove(Seq<AttestedEntry> chain, int leafIndex) {
        Seq<ReadOnlyMemory<byte>> leaves = toSeq(chain.Map(LeafDigest));
        return new InclusionProof(leafIndex, leaves.Count, leaves[leafIndex], AuditPath(leaves, leafIndex), MerkleRoot(leaves));
    }

    public static bool Verify(InclusionProof proof) =>
        proof.LeafIndex < proof.TreeSize
        && RecomputeInclusion(proof.LeafDigest.Span, proof.LeafIndex, proof.TreeSize, proof.AuditPath) is { } folded
        && folded.Span.SequenceEqual(proof.Root.Span);

    public static ConsistencyProof Consistent(Seq<AttestedEntry> chain, int oldSize) {
        Seq<ReadOnlyMemory<byte>> leaves = toSeq(chain.Map(LeafDigest));
        return new ConsistencyProof(
            oldSize, leaves.Count, ConsistencyPath(leaves, oldSize),
            MerkleRoot(toSeq(leaves.Take(oldSize))), MerkleRoot(leaves));
    }

    public static bool ConsistentWith(ConsistencyProof proof) =>
        proof.OldSize switch {
            0 => true,
            int old when old == proof.NewSize => proof.Path.IsEmpty && proof.OldRoot.Span.SequenceEqual(proof.NewRoot.Span),
            int old when old > proof.NewSize => false,
            int old => RecomputeConsistency(old, proof.NewSize, proof.OldRoot, proof.Path) is { } pair
                && pair.Old.Span.SequenceEqual(proof.OldRoot.Span) && pair.New.Span.SequenceEqual(proof.NewRoot.Span),
        };

    public static HeadSeal Seal(UInt128 logId, Seq<AttestedEntry> chain, Instant at, ECDsa key) {
        ReadOnlyMemory<byte> root = Root(chain);
        return new HeadSeal(logId, chain.Count, root, at, key.SignData(Sth(logId, chain.Count, root.Span, at), HashAlgorithmName.SHA256));
    }

    public static bool VerifySeal(HeadSeal seal, ECDsa key) =>
        key.VerifyData(Sth(seal.LogId, seal.TreeSize, seal.Root.Span, seal.At), seal.Signature.Span, HashAlgorithmName.SHA256);

    private static byte[] Sth(UInt128 logId, int treeSize, ReadOnlySpan<byte> root, Instant at) {
        Span<byte> message = stackalloc byte[16 + 4 + 32 + 8];
        BinaryPrimitives.WriteUInt128LittleEndian(message, logId);
        BinaryPrimitives.WriteInt32LittleEndian(message[16..], treeSize);
        root.CopyTo(message[20..]);
        BinaryPrimitives.WriteInt64LittleEndian(message[52..], at.ToUnixTimeTicks());
        return message.ToArray();
    }

    private static ReadOnlyMemory<byte> MerkleRoot(Seq<ReadOnlyMemory<byte>> leaves) =>
        leaves.Count switch {
            0 => Sha(ReadOnlySpan<byte>.Empty),
            1 => leaves[0],
            int n => Node(MerkleRoot(toSeq(leaves.Take(Split(n)))).Span, MerkleRoot(toSeq(leaves.Skip(Split(n)))).Span),
        };

    private static Seq<ReadOnlyMemory<byte>> AuditPath(Seq<ReadOnlyMemory<byte>> leaves, int index) =>
        leaves.Count <= 1
            ? []
            : (Split: Split(leaves.Count), index) switch {
                (int k, int i) when i < k => AuditPath(toSeq(leaves.Take(k)), i).Add(MerkleRoot(toSeq(leaves.Skip(k)))),
                (int k, int i) => AuditPath(toSeq(leaves.Skip(k)), i - k).Add(MerkleRoot(toSeq(leaves.Take(k)))),
            };

    private static Seq<ReadOnlyMemory<byte>> ConsistencyPath(Seq<ReadOnlyMemory<byte>> leaves, int oldSize) =>
        oldSize == 0 || oldSize == leaves.Count
            ? []
            : Subproof(leaves, oldSize, onPath: true);

    private static Seq<ReadOnlyMemory<byte>> Subproof(Seq<ReadOnlyMemory<byte>> leaves, int m, bool onPath) =>
        m == leaves.Count
            ? onPath ? [] : Seq(MerkleRoot(leaves))
            : (Split: Split(leaves.Count), m) switch {
                (int k, int count) when count <= k => Subproof(toSeq(leaves.Take(k)), count, onPath).Add(MerkleRoot(toSeq(leaves.Skip(k)))),
                (int k, int count) => Subproof(toSeq(leaves.Skip(k)), count - k, onPath: false).Add(MerkleRoot(toSeq(leaves.Take(k)))),
            };

    private static ReadOnlyMemory<byte> Node(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) {
        Span<byte> span = stackalloc byte[1 + 32 + 32];
        span[0] = 0x01;
        left.CopyTo(span[1..]);
        right.CopyTo(span[33..]);
        return Sha(span);
    }

    private static ReadOnlyMemory<byte> Sha(ReadOnlySpan<byte> source) {
        byte[] digest = new byte[SHA256.HashSizeInBytes];
        SHA256.HashData(source, digest);
        return digest;
    }

    private static int Split(int n) => 1 << BitOperations.Log2((uint)(n - 1));

    // RFC 9162 §2.1.1 inclusion-path recompute: the (fn, sn) bit-ratchet over the audit path is the
    // standard imperative verifier — its asymmetric step (climb while fn is even) is the named statement
    // exemption, total over every tree size where AuditPath is well-formed.
    private static ReadOnlyMemory<byte>? RecomputeInclusion(ReadOnlySpan<byte> leaf, int index, int size, Seq<ReadOnlyMemory<byte>> path) {
        var (fn, sn) = (index, size - 1);
        Span<byte> acc = stackalloc byte[SHA256.HashSizeInBytes];
        leaf.CopyTo(acc);
        foreach (ReadOnlyMemory<byte> sibling in path) {
            if (sn == 0) { return null; }
            if ((fn & 1) == 1 || fn == sn) {
                Node(sibling.Span, acc).Span.CopyTo(acc);
                while ((fn & 1) == 0) { (fn, sn) = (fn >> 1, sn >> 1); }
            } else {
                Node(acc, sibling.Span).Span.CopyTo(acc);
            }
            (fn, sn) = (fn >> 1, sn >> 1);
        }
        return sn == 0 ? acc.ToArray() : null;
    }

    // RFC 9162 §2.1.2 consistency-path recompute: folds the proof into both the old-tree and new-tree roots
    // so a split-history attack surfaces as one of the two recomputed roots failing to match. The power-of-two
    // old-size prepends the carried old root per the spec; the (fn, sn) ratchet is the named statement exemption.
    private static (ReadOnlyMemory<byte> Old, ReadOnlyMemory<byte> New)? RecomputeConsistency(int oldSize, int newSize, ReadOnlyMemory<byte> oldRootSeal, Seq<ReadOnlyMemory<byte>> proof) {
        Seq<ReadOnlyMemory<byte>> path = (oldSize & (oldSize - 1)) == 0 ? oldRootSeal.Cons(proof) : proof;
        if (path.IsEmpty) { return null; }
        var (fn, sn) = (oldSize - 1, newSize - 1);
        while ((fn & 1) == 1) { (fn, sn) = (fn >> 1, sn >> 1); }
        Span<byte> oldRoot = stackalloc byte[SHA256.HashSizeInBytes];
        Span<byte> newRoot = stackalloc byte[SHA256.HashSizeInBytes];
        path[0].Span.CopyTo(oldRoot);
        path[0].Span.CopyTo(newRoot);
        for (int i = 1; i < path.Count; i++) {
            if (sn == 0) { return null; }
            ReadOnlySpan<byte> sibling = path[i].Span;
            if ((fn & 1) == 1 || fn == sn) {
                Node(sibling, oldRoot).Span.CopyTo(oldRoot);
                Node(sibling, newRoot).Span.CopyTo(newRoot);
                while ((fn & 1) == 0 && fn != 0) { (fn, sn) = (fn >> 1, sn >> 1); }
            } else {
                Node(newRoot, sibling).Span.CopyTo(newRoot);
            }
            (fn, sn) = (fn >> 1, sn >> 1);
        }
        return sn == 0 ? (oldRoot.ToArray(), newRoot.ToArray()) : null;
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                                | [BINDING]                                                                                                                                   |
| :-----: | :------------------ | :---------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | digest algorithm    | `SHA256(prior‖content‖fingerprint‖class‖category)`    | cryptographic tamper-evidence; `XxHash128` rejected for the chain per `api-hashing.md` `[RAIL_LAW]`; `stackalloc byte[128]` symmetric        |
|  [02]   | seal cadence        | content-addressed snapshot seal                       | external-witness anchor on the schedule lease                                                                                               |
|  [08]   | graduation evidence | `ProviderFingerprint` dedicated `Option<UInt128>`     | `(checksum, OrtEpDevice)` bound into the digest term, never the `RedactedPayloadProof` slot; forward-only format epoch with genesis re-seal |
|  [03]   | audit category      | `AuditBinding.Bind` result                            | one category vocabulary, never a second                                                                                                     |
|  [04]   | merkle leaf         | `SHA256(0x00 ‖ Digest)` — preserved digest only       | RFC 9162 leaf/node `0x00`/`0x01` domain separation; redaction-stable root (no payload reference in the leaf); `TransparencyProof.Root`      |
|  [05]   | inclusion proof     | O(log n) RFC 9162 §2.1.1 `(fn, sn)` recompute         | `Verify` proves one entry without the chain over every tree size; export-bundle artifact                                                    |
|  [06]   | consistency proof   | RFC 9162 §2.1.2 dual-root recompute                   | `ConsistentWith` verifies append-only extension; split-history is a failed `ConsistencyProof`, not a builder-only claim                     |
|  [07]   | head-seal signature | `ECDsa` detached over `(LogId, TreeSize, Root, At)`   | RFC 9162 signed-tree-head; non-repudiation on a cryptographic root; `LogId` distinguishes federated logs                                    |

## [04]-[LINEAGE_CDC]

- Owner: `CdcScope` the lineage-filter scope; `CdcEnvelope` the redaction-aware change record carrying the originating `TraceContext` slot and the `RedactorKind` evidence so a lineage-CDC row inherits the span the op was committed under and a consumer knows whether a masked field stays join-correlatable; `LineageCdc` the static surface owning the lineage-scoped CDC filter, the redaction-preserving payload projection, and the per-consumer feed fold.
- Cases: a scope filters the changefeed to a lineage subtree (every entity derived from a root), to a classification ceiling, and to a tenant; the feed projects each in-scope change with its payload redacted to the consumer's classification authority and stamps the redactor that masked it.
- Entry: `public static IO<Seq<CdcEnvelope>> Feed(CdcScope scope, Func<SyncCursor, Seq<OpLogEntry>> changefeed, Func<UInt128, LineageSlice> lineage, Func<OpLogEntry, DataClassification, IO<(ReadOnlyMemory<byte> Bytes, bool Redacted, DataClassification Effective, RedactorKind Redactor)>> redact)` — folds the changefeed past the cursor, filters to the lineage scope and classification ceiling, and projects each in-scope change with its payload redacted and its redactor stamped.
- Auto: lineage-filtered CDC is the changefeed restricted to a derivation subtree — a consumer subscribes to a root entity and receives only changes to entities the lineage DAG derives from it, so a multi-tenant or discipline-scoped consumer never sees out-of-scope rows; the filter composes the lineage slice (`#CAUSAL_DAG`) so the scope is the forward slice of the root resolved through the one `Endpoints` algebra; redaction is preserving — a change whose classification exceeds the consumer's authority emits with its payload redacted through the settled `ExportProof`/`RedactorKind` so the change's existence and metadata stay visible while its content is masked, never a silent drop; the feed rides the op-log cursor so it is the same changefeed the sync transport drains, scoped per consumer.
- Receipt: a feed rides `store.cdc.feed` carrying the in-scope count, the redacted count, and the advanced cursor.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Microsoft.Extensions.Compliance.Redaction, Rasm.AppHost (project), BCL inbox.
- Growth: a new scope dimension is one column on `CdcScope`; a new redaction policy is one `RedactorKind` row (owned at AppHost); zero new surface — a per-consumer CDC trigger, a second change-data-capture pipeline, or a row-level filter table is the deleted form because the feed is the one changefeed scoped by the lineage slice and the classification ceiling, and redaction rides the settled export-proof redactor.
- Boundary: lineage-scoped CDC is the changefeed filtered by the forward lineage slice so a consumer receives exactly its derivation subtree — a coarse table-level CDC or an unscoped firehose is the deleted form; the classification ceiling is the consumer's authority so an out-of-authority change emits redaction-preserving (existence and metadata visible, content masked) rather than dropped, because a silent drop hides that a change occurred and a redaction-preserving feed proves a change occurred while protecting its content; redaction routes through the settled `Version/retention#EXPORT_PROOF` redactor resolution (`RedactorKind.Hmac` join-preserving, `RedactorKind.None` pass-through, unmapped fail-closed by erasure) so the CDC and the support-bundle export share one redactor, never a second masking path, and the `CdcEnvelope.Redactor` stamps which one masked the row so a consumer renders an HMAC-pseudonymized (still-correlatable) field distinctly from an erased one; the feed rides the op-log cursor (`SyncCursor`) so it is the same changefeed the sync transport drains and a consumer's cursor advances exactly past its in-scope changes; tenant scope composes the RLS predicate (`Store/tenancy#TENANCY_RLS`) so a per-tenant CDC feed is RLS-scoped at the source, and the lineage scope refines within the tenant; the `CdcEnvelope` carries the originating `OpLogEntry.Trace` W3C trace-context unchanged through the `Feed` projection so a lineage-CDC consumer extract-and-continues the same span the op was committed under — realizing the `ONE_DISTRIBUTED_TRACE` seam on the CDC face — never minting a second tracer, and an op committed under no span carries `TraceContext.Empty`.

```csharp signature
public sealed record CdcScope(
    Option<UInt128> LineageRoot,
    DataClassification ClassificationCeiling,
    Option<UInt128> TenantId,
    SyncCursor Cursor);

public readonly record struct CdcEnvelope(
    long Sequence,
    string EntityKind,
    string EntityKey,
    SyncOpKind Kind,
    UInt128 ContentKey,
    ReadOnlyMemory<byte> Payload,
    bool Redacted,
    RedactorKind Redactor,
    TraceContext Trace,
    DataClassification Classification,
    Instant At);

public static class LineageCdc {
    public static IO<Seq<CdcEnvelope>> Feed(
        CdcScope scope,
        Func<SyncCursor, Seq<OpLogEntry>> changefeed,
        Func<UInt128, LineageSlice> lineage,
        Func<OpLogEntry, DataClassification, IO<(ReadOnlyMemory<byte> Bytes, bool Redacted, DataClassification Effective, RedactorKind Redactor)>> redact) {
        LanguageExt.HashSet<UInt128> scopeKeys = scope.LineageRoot.Map(root =>
            lineage(root) is LineageSlice slice
                ? toHashSet(slice.Entities.Map(static e => e.ContentKey).Add(root))
                : []).IfNone([]);
        return toSeq(changefeed(scope.Cursor)
            .Where(entry => scope.LineageRoot.IsNone || scopeKeys.Contains(entry.ContentKey)))
            .TraverseM(entry => redact(entry, scope.ClassificationCeiling).Map(p =>
                new CdcEnvelope(
                    entry.Sequence, entry.EntityKind, entry.EntityKey, entry.Kind, entry.ContentKey,
                    p.Bytes, p.Redacted, p.Redactor, entry.Trace, p.Effective, entry.Physical)))
            .As();
    }
}
```

## [05]-[TS_PROJECTION]

- Owner: `ProvEdgeWire`, `ProvNodeWire`, `ProvNodeKind`, `ProvRelation`, `LineageSliceWire`, `LineageDirectionKind`, `ReplayStepWire`, `ReplaySliceWire`, `AttestedEntryWire`, `LedgerHeadWire`, `InclusionProofWire`, `HeadSealWire`, `CdcEnvelopeWire` — the provenance wire surface the audit dashboard and lineage explorer decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: content keys cross as 16-byte binary and SHA-256 digests cross as 32-byte binary so the explorer never confuses an identity key with an integrity digest; instants cross as ISO-8601 extended strings; 64-bit sequences decode as bigint under `useBigInt64`; the `ProvEdge` discriminator crosses as the W3C-PROV-DM relation name and reconstructs as the literal union over all nine relations so the explorer renders invalidation and revision edges distinctly from generic derivation; the `LineageSliceWire` carries the resolved `nodes` so the explorer renders blame agents and impact entities without a re-walk, and the `ReplaySliceWire` carries the HLC-ordered `steps` so the explorer renders an entity's derivation timeline — each step's produced/consumed/revised keys and its invalidation flag — without re-folding the op-log; the `CdcEnvelopeWire` carries the `redacted` flag and the `redactor` so the explorer renders an HMAC-pseudonymized field distinctly from an erased one, plus an APPEND-ONLY optional `trace` slot (16-byte `traceparent` + opaque `tracestate`) so a lineage-CDC consumer extract-and-continues the originating span, the slot's optionality preserving the existing field sequence; the `AttestedEntryWire` carries the prior and current 32-byte SHA-256 digests so the explorer verifies a chain segment client-side, plus the optional `providerFingerprint` so a graduated-surrogate entry surfaces its `(checksum, OrtEpDevice)` binding distinctly from a redaction; the `InclusionProofWire` and `HeadSealWire` carry the RFC 9162 audit path and the signed tree head so a browser auditor verifies inclusion in O(log n) against the detached `ECDsa` signature without the chain — the browser recomputes each leaf as `SHA256(0x00 ‖ digest)` over the entry's preserved 32-byte `digest` ALONE (never the `redactedPayloadProof`, which is presentation-only), runs the canonical §2.1.1 `(fn, sn)` path fold, and a redaction therefore leaves a previously fetched proof valid against the unchanged root.

```ts contract
type ProvNodeKind = "Agent" | "Activity" | "Entity";

type ProvRelation =
  | "WasGeneratedBy" | "Used" | "WasDerivedFrom" | "WasRevisionOf" | "WasInvalidatedBy"
  | "WasAttributedTo" | "WasAssociatedWith" | "WasInformedBy" | "ActedOnBehalfOf";

type LineageDirectionKind = "Backward" | "Forward";

interface ProvEdgeWire {
  relation: ProvRelation;
  subject: Uint8Array;
  object: Uint8Array;
}

type ProvNodeWire =
  | { kind: "Agent"; key: Uint8Array; actor: string; origin: string }
  | { kind: "Activity"; key: Uint8Array; physical: string; logical: bigint; origin: string }
  | { kind: "Entity"; contentKey: Uint8Array; entityKind: string; entityKey: string };

interface LineageSliceWire {
  root: Uint8Array;
  direction: LineageDirectionKind;
  edges: ProvEdgeWire[];
  nodes: ProvNodeWire[];
  depth: number;
}

interface ReplayStepWire {
  activity: Uint8Array;
  physical: string;
  logical: bigint;
  produced: Uint8Array;
  consumed: Uint8Array[];
  revised: Uint8Array | null;
  invalidated: boolean;
}

interface ReplaySliceWire {
  root: Uint8Array;
  steps: ReplayStepWire[];
}

interface AttestedEntryWire {
  sequence: bigint;
  contentKey: Uint8Array;
  priorDigest: Uint8Array;
  digest: Uint8Array;
  classification: string;
  auditCategory: string;
  providerFingerprint: Uint8Array | null;
  redactedPayloadProof: Uint8Array | null;
  at: string;
}

interface LedgerHeadWire {
  sequence: bigint;
  digest: Uint8Array;
  at: string;
}

interface InclusionProofWire {
  leafIndex: number;
  treeSize: number;
  leafDigest: Uint8Array;
  auditPath: Uint8Array[];
  root: Uint8Array;
}

interface HeadSealWire {
  logId: Uint8Array;
  treeSize: number;
  root: Uint8Array;
  at: string;
  signature: Uint8Array;
}

interface CdcEnvelopeWire {
  sequence: bigint;
  entityKind: string;
  entityKey: string;
  kind: "upsert" | "delete" | "presence";
  contentKey: Uint8Array;
  payload: Uint8Array;
  redacted: boolean;
  redactor: "none" | "hmac" | "erasing";
  trace?: { traceparent: Uint8Array; tracestate: Uint8Array };
  classification: string;
  at: string;
}
```

## [06]-[RESEARCH]

- [LINEAGE_SLICE_COST]: the bounded backward/forward slice cost over a 10^7-edge causal DAG projected from the op-log — whether the projected adjacency index (a content-key-to-edge GIN index on the op-log `Closure` column) keeps a depth-bounded slice linear in the reachable-edge count, and whether the derivation source resolution reads the `Closure` manifest and the `WasRevisionOf`/`WasInvalidatedBy` discriminants without a recursive CTE on the hot path.
- [ATTESTED_CHAIN_SHA256]: the SHA-256 attested-chain append and verify over a real op-log — confirming the `stackalloc byte[128]` tuple layout is symmetric in `Append`/`Recompute`, the chain breaks under any single-entry mutation with cryptographic certainty, a redacted entry keeping its pre-redaction `Digest` and only setting `RedactedPayloadProof` leaves both the chain `Verify` and the Merkle root unchanged (the chain digest and the leaf both read the preserved `Digest`, never the redaction reference), and the periodic content-addressed seal an external witness anchors against the live PG18 pgaudit category record.
- [MERKLE_PROOF_RFC9162]: HOST-VALIDATION of the RFC 9162 SHA-256 proof folds against the published §2.1.4 reference test vectors — the inclusion `Prove`/`Verify` and consistency `Consistent`/`ConsistentWith` round-trips already verify for every tree size 1..N in the construction harness (the `Verify` recompute is the canonical §2.1.1 `(fn, sn)` fold, NOT the prior `Climb` spelling that rejected non-power-of-two trees, and `ConsistentWith` is the §2.1.2 dual-root fold), so this item only pins the literal digest vectors and the `ECDsa` curve / `HashAlgorithmName.SHA256` the `HeadSeal` signs the `(LogId, TreeSize, Root, At)` signed-tree-head under (the key arrives from the AppHost identity seam, never minted here); the redaction-stable leaf (preserved `Digest` only, no payload reference) confirms one inclusion proof verifies against one head both before and after a redaction over a real chain, and a tampered leaf, a forged audit path, and a forked (non-extension) history each fail their verifier.
