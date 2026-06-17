# [PERSISTENCE_PROVENANCE]

Rasm.Persistence owns lineage as a first-class join dimension: `ProvEdge` is the W3C-PROV causal DAG relating agents, activities, and entities over the existing op-log; `LineageSlice` folds backward and forward slices, blame, and impact over that DAG; `AttestedEntry` hash-chains every op-log row plus the pgaudit category record into a tamper-evident attested ledger whose head digest proves the whole chain; and `LineageCdc` filters the change-data-capture stream by lineage scope so a redaction-preserving feed emits only the slice a consumer is authorized to see. The op-log changefeed (`OpLogEntry`, HLC stamp, `ContentKey`), the content-address identity (`XxHash128`), the export-proof redaction (`ExportProof`, `RedactorKind`), and the pgaudit category binding (`AuditBinding`) arrive settled and compose inside the fences; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, and `DataClassification` arrive from AppHost.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                             |
| :-----: | :--------------- | :---------------------------------------------------------------- |
|   [1]   | CAUSAL_DAG       | W3C-PROV agent/activity/entity edges over the op-log; slice fold  |
|   [2]   | ATTESTED_LEDGER  | Hash-chained attested log; redaction-preserving lineage; head proof |
|   [3]   | LINEAGE_CDC      | Lineage-scoped change-data-capture; redaction-aware feed          |
|   [4]   | TS_PROJECTION    | Provenance edge, slice, attestation, CDC envelope wire shapes     |

## [2]-[CAUSAL_DAG]

- Owner: `ProvEdge` `[Union]` the W3C-PROV causal relation family; `ProvNode` the agent/activity/entity vertex; `LineageSlice` the slice result; `Provenance` the static surface owning edge emission over the op-log, backward/forward slicing, blame attribution, impact analysis, and deterministic replay.
- Cases: `WasGeneratedBy | Used | WasDerivedFrom | WasAttributedTo | WasAssociatedWith | WasInformedBy` on `ProvEdge` — the W3C-PROV core relations; `ProvNode` is `Agent | Activity | Entity`.
- Entry: `public static ProvEdge Edge(OpLogEntry op)` — projects the causal edge an op-log row implies (an upsert `WasGeneratedBy` the op activity, the op `WasAssociatedWith` the actor agent, the new entity `WasDerivedFrom` the prior content key); `public static LineageSlice Slice(Func<UInt128, Seq<ProvEdge>> adjacency, UInt128 root, LineageDirection direction, int depth)` walks the DAG backward (every entity/activity/agent that produced the root) or forward (everything the root produced).
- Auto: every op-log row implies a provenance edge with zero extra write — the op's `Actor` is the agent, the op itself is the activity, the `ContentKey` is the generated entity, and the prior content key in the `Closure` is the derivation source — so the causal DAG is a projection over the existing changefeed, never a second store; lineage is a join dimension so a federated search (`federation#FUSION_RANK`) carries each hit's provenance head, a retention sweep holds a row whose lineage is under legal hold, and a sync scope filters by lineage subtree; blame is the backward slice to the originating agent of a content key, and impact is the forward slice to every entity derived from a changed content key; deterministic replay folds the activity edges in HLC order to reproduce an entity's derivation.
- Receipt: a slice rides `store.lineage.slice` carrying the direction, the depth reached, and the vertex count; replay rides `store.lineage.replay`.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new PROV relation is one `ProvEdge` case; a new vertex kind is one `ProvNode` case; a new slice direction is one `LineageDirection` value; zero new surface — a separate lineage store, a trigger-emitted lineage table, or a per-operation provenance logger is the deleted form because every edge projects from the op-log row the changefeed already commits, and the slice is one bounded walk over the projected adjacency.
- Boundary: the causal DAG is a pure projection of the op-log so it carries no second write path — a CDC trigger building a lineage table or a per-op provenance call is the deleted form; the W3C-PROV core relations are the closed vocabulary (`WasGeneratedBy`, `Used`, `WasDerivedFrom`, `WasAttributedTo`, `WasAssociatedWith`, `WasInformedBy`) so the lineage interoperates with any PROV-consuming tool, and a non-standard relation is the named defect; backward slicing answers "what produced this" (blame, audit), forward slicing answers "what does this affect" (impact analysis), and both are bounded breadth-first walks over the projected adjacency so the cost is linear in the reachable-edge count within the depth bound; lineage is the join dimension every reuse surface composes — the federated entity carries a provenance head (`federation#ENTITY_GRAPH`), the structural-diff blame (`version-control#TIME_TRAVEL`) reads the same winning op the lineage attributes, and the retention sweep's legal hold reads the lineage subtree — so a lineage absent from a query result is a gap, not a feature; the derivation source of an entity is the prior content key in its `Closure` manifest so the DAG rides the existing content-address closure, never a re-derived dependency graph.

```csharp signature
public sealed class ProvenanceKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum]
public sealed partial class LineageDirection {
    public static readonly LineageDirection Backward = new();
    public static readonly LineageDirection Forward = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvNode {
    private ProvNode() { }

    public sealed record Agent(string Actor, Guid Origin) : ProvNode;
    public sealed record Activity(UInt128 OpKey, string Kind, Instant Physical, ulong Logical) : ProvNode;
    public sealed record Entity(UInt128 ContentKey, string EntityKind) : ProvNode;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvEdge {
    private ProvEdge() { }

    public sealed record WasGeneratedBy(UInt128 Entity, UInt128 Activity) : ProvEdge;
    public sealed record Used(UInt128 Activity, UInt128 Entity) : ProvEdge;
    public sealed record WasDerivedFrom(UInt128 Entity, UInt128 Source) : ProvEdge;
    public sealed record WasAttributedTo(UInt128 Entity, string Agent) : ProvEdge;
    public sealed record WasAssociatedWith(UInt128 Activity, string Agent) : ProvEdge;
    public sealed record WasInformedBy(UInt128 Activity, UInt128 Informant) : ProvEdge;
}

public readonly record struct LineageSlice(UInt128 Root, LineageDirection Direction, Seq<ProvEdge> Edges, int VertexCount, int Depth);

public static class Provenance {
    public const int MaxSliceDepth = 64;

    public static Seq<ProvEdge> Edges(OpLogEntry op) =>
        Seq<ProvEdge>(
            new ProvEdge.WasGeneratedBy(op.ContentKey, ActivityKey(op)),
            new ProvEdge.WasAssociatedWith(ActivityKey(op), op.Actor),
            new ProvEdge.WasAttributedTo(op.ContentKey, op.Actor))
            .Append(op.Closure.Map(source => (ProvEdge)new ProvEdge.WasDerivedFrom(op.ContentKey, source)));

    public static LineageSlice Slice(Func<UInt128, Seq<ProvEdge>> adjacency, UInt128 root, LineageDirection direction, int depth) {
        var seen = new System.Collections.Generic.HashSet<UInt128> { root };
        var frontier = Seq((Key: root, Depth: 0));
        var edges = Seq<ProvEdge>();
        var maxDepth = 0;
        while (frontier.HeadOrNone() is { IsSome: true, Case: var head } && frontier is { IsEmpty: false }) {
            frontier = frontier.Tail;
            maxDepth = int.Max(maxDepth, head.Depth);
            if (head.Depth >= int.Min(depth, MaxSliceDepth))
                continue;
            foreach (var edge in adjacency(head.Key)) {
                edges = edges.Add(edge);
                var next = direction == LineageDirection.Backward ? Tail(edge) : Head(edge);
                next.Iter(key => { if (seen.Add(key)) frontier = frontier.Add((key, head.Depth + 1)); });
            }
        }
        return new LineageSlice(root, direction, edges, seen.Count, maxDepth);
    }

    private static UInt128 ActivityKey(OpLogEntry op) {
        Span<byte> span = stackalloc byte[24];
        BinaryPrimitives.WriteInt64LittleEndian(span, op.Physical.ToUnixTimeTicks());
        BinaryPrimitives.WriteUInt64LittleEndian(span[8..], op.Logical);
        Encoding.UTF8.GetBytes(op.Actor.AsSpan(0, int.Min(8, op.Actor.Length)), span[16..]);
        return XxHash128.HashToUInt128(span);
    }

    private static Option<UInt128> Tail(ProvEdge edge) =>
        edge switch {
            ProvEdge.WasDerivedFrom d => Some(d.Source),
            ProvEdge.WasGeneratedBy g => Some(g.Activity),
            ProvEdge.WasInformedBy i => Some(i.Informant),
            _ => None,
        };

    private static Option<UInt128> Head(ProvEdge edge) =>
        edge switch {
            ProvEdge.WasGeneratedBy g => Some(g.Entity),
            ProvEdge.Used u => Some(u.Entity),
            _ => None,
        };
}
```

## [3]-[ATTESTED_LEDGER]

- Owner: `AttestedEntry` the hash-chained ledger row; `LedgerHead` the chain-head proof; `AttestedLedger` the static surface owning the chain-append fold, the head-digest computation, the chain verification, and the redaction-preserving entry projection.
- Cases: each entry chains `PriorDigest` into its own `Digest` over `XxHash128(PriorDigest || ContentKey || Classification || AuditCategory)`, so the head digest attests every prior entry; a redacted entry preserves its `Digest` while replacing the payload reference so the chain verifies even after a redaction.
- Entry: `public static AttestedEntry Append(LedgerHead head, OpLogEntry op, string auditCategory)` — pure chain-append linking the prior head digest into the new entry; `public static Fin<LedgerHead> Verify(Seq<AttestedEntry> chain, LedgerHead expected)` recomputes every digest and aborts on the first break.
- Auto: every op-log row plus its bound pgaudit category (`AuditBinding.Bind`) appends one `AttestedEntry` whose digest chains the prior digest, so the head digest is a tamper-evident proof of the whole history — a single altered prior row breaks every subsequent digest and `Verify` names the first break; the chain rides the op-log so it carries no second store; redaction is preserving — a redacted entry keeps its original `Digest` (computed over the pre-redaction content key) and replaces the payload reference with the `ExportProof` content hash so the chain still verifies and an auditor proves the row existed without seeing the redacted content; the head digest is periodically sealed as a content-addressed snapshot (`Snapshots.ContentAddress`) so an external witness anchors the chain.
- Receipt: an append rides `store.ledger.append` carrying the new head digest; a verification rides `store.ledger.verify` carrying the verified count or the first break index; a redaction-preserving projection rides the settled `ExportProof`.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Microsoft.Extensions.Compliance.Redaction, Rasm.AppHost (project), BCL inbox.
- Growth: a new attestation field is one term in the digest tuple; a new seal cadence is one schedule policy value; zero new surface — a Merkle-tree audit store, a blockchain ledger, or a second tamper-evident log is the deleted form because the chain is one hash-chained projection over the op-log and the pgaudit binding, and the head digest is the one proof; the existing pgaudit/legal-hold/RLS/CDC machinery (`redaction-retention`, `server-tier`) stays the substrate and this owner adds the hash-chain attestation plus the lineage scoping net-new.
- Boundary: the chain is hash-linked so tamper-evidence is structural — `Digest_n = XxHash128(Digest_{n-1} || ContentKey || Classification || AuditCategory)` so altering any entry invalidates every later digest and `Verify` returns the first break index, never a silent pass; the attestation is non-cryptographic (XxHash128) so it is a tamper-evidence proof, not a signature — a cryptographic-signature claim is the named defect, and external non-repudiation rides the periodic content-addressed seal an outside witness anchors; redaction is preserving — a redacted entry keeps its pre-redaction digest and swaps the payload reference for the `ExportProof` content hash, so the chain verifies after redaction and the redacted row's existence and classification stay provable while its content is erased; the pgaudit category per entry comes from the settled `AuditBinding.Bind` (`redaction-retention#AUDIT_BINDING`) so the attested ledger and the server-side audit log carry the same category; legal hold reads the lineage subtree (`#CAUSAL_DAG`) so a held entry's whole derivation chain is retained; the chain rides the op-log changefeed so it converges across peers and a fork in the chain (two entries claiming the same prior digest) is a typed conflict the merge surfaces, never a silent overwrite.

```csharp signature
public readonly record struct AttestedEntry(
    long Sequence,
    UInt128 ContentKey,
    UInt128 PriorDigest,
    UInt128 Digest,
    DataClassification Classification,
    string AuditCategory,
    Option<UInt128> RedactedPayloadProof,
    Instant At);

public readonly record struct LedgerHead(long Sequence, UInt128 Digest, Instant At);

public static class AttestedLedger {
    public static readonly LedgerHead Genesis = new(0L, UInt128.Zero, Instant.MinValue);

    public static AttestedEntry Append(LedgerHead head, OpLogEntry op, string auditCategory, DataClassification classification) {
        Span<byte> tuple = stackalloc byte[64];
        BinaryPrimitives.WriteUInt128LittleEndian(tuple, head.Digest);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[16..], op.ContentKey);
        var written = 32 + Encoding.UTF8.GetBytes($"{classification.Key}|{auditCategory}", tuple[32..]);
        return new AttestedEntry(
            head.Sequence + 1L, op.ContentKey, head.Digest,
            XxHash128.HashToUInt128(tuple[..written]),
            classification, auditCategory, None, op.Physical);
    }

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
                entry.PriorDigest != head.Digest
                    ? Fin.Fail<LedgerHead>(Error.New($"<ledger-chain-break:{entry.Sequence}>"))
                    : Recompute(head, entry) != entry.Digest
                        ? Fin.Fail<LedgerHead>(Error.New($"<ledger-digest-mismatch:{entry.Sequence}>"))
                        : Fin.Succ(new LedgerHead(entry.Sequence, entry.Digest, entry.At))));

    private static UInt128 Recompute(LedgerHead head, AttestedEntry entry) {
        Span<byte> tuple = stackalloc byte[64];
        BinaryPrimitives.WriteUInt128LittleEndian(tuple, head.Digest);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[16..], entry.ContentKey);
        var written = 32 + Encoding.UTF8.GetBytes($"{entry.Classification.Key}|{entry.AuditCategory}", tuple[32..]);
        return XxHash128.HashToUInt128(tuple[..written]);
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                          | [BINDING]                                       |
| :-----: | :------------------ | :------------------------------- | :---------------------------------------------- |
|   [1]   | digest algorithm    | `XxHash128` tamper-evidence       | non-cryptographic; signature claim is rejected  |
|   [2]   | seal cadence        | content-addressed snapshot seal   | external-witness anchor on the schedule lease   |
|   [3]   | audit category      | `AuditBinding.Bind` result        | one category vocabulary, never a second         |

## [4]-[LINEAGE_CDC]

- Owner: `CdcScope` the lineage-filter scope; `CdcEnvelope` the redaction-aware change record; `LineageCdc` the static surface owning the lineage-scoped CDC filter, the redaction-preserving payload projection, and the per-consumer feed fold.
- Cases: a scope filters the changefeed to a lineage subtree (every entity derived from a root), to a classification ceiling, and to a tenant; the feed projects each in-scope change with its payload redacted to the consumer's classification authority.
- Entry: `public static IO<Seq<CdcEnvelope>> Feed(CdcScope scope, Func<SyncCursor, Seq<OpLogEntry>> changefeed, Func<UInt128, LineageSlice> lineage, Func<OpLogEntry, DataClassification, IO<ReadOnlyMemory<byte>>> redact)` — folds the changefeed past the cursor, filters to the lineage scope and classification ceiling, and projects each in-scope change with its payload redacted.
- Auto: lineage-filtered CDC is the changefeed restricted to a derivation subtree — a consumer subscribes to a root entity and receives only changes to entities the lineage DAG derives from it, so a multi-tenant or discipline-scoped consumer never sees out-of-scope rows; the filter composes the lineage slice (`#CAUSAL_DAG`) so the scope is the forward slice of the root; redaction is preserving — a change whose classification exceeds the consumer's authority emits with its payload redacted through the settled `ExportProof`/`RedactorKind` so the change's existence and metadata stay visible while its content is masked, never a silent drop; the feed rides the op-log cursor so it is the same changefeed the sync transport drains, scoped per consumer.
- Receipt: a feed rides `store.cdc.feed` carrying the in-scope count, the redacted count, and the advanced cursor.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Microsoft.Extensions.Compliance.Redaction, Rasm.AppHost (project), BCL inbox.
- Growth: a new scope dimension is one column on `CdcScope`; a new redaction policy is one `RedactorKind` row (owned at AppHost); zero new surface — a per-consumer CDC trigger, a second change-data-capture pipeline, or a row-level filter table is the deleted form because the feed is the one changefeed scoped by the lineage slice and the classification ceiling, and redaction rides the settled export-proof redactor.
- Boundary: lineage-scoped CDC is the changefeed filtered by the forward lineage slice so a consumer receives exactly its derivation subtree — a coarse table-level CDC or an unscoped firehose is the deleted form; the classification ceiling is the consumer's authority so an out-of-authority change emits redaction-preserving (existence and metadata visible, content masked) rather than dropped, because a silent drop hides that a change occurred and a redaction-preserving feed proves a change occurred while protecting its content; redaction routes through the settled `redaction-retention#EXPORT_PROOF` redactor resolution (`RedactorKind.Hmac` join-preserving, `RedactorKind.None` pass-through, unmapped fail-closed by erasure) so the CDC and the support-bundle export share one redactor, never a second masking path; the feed rides the op-log cursor (`SyncCursor`) so it is the same changefeed the sync transport drains and a consumer's cursor advances exactly past its in-scope changes; tenant scope composes the RLS predicate (`provisioning#TENANCY_RLS`) so a per-tenant CDC feed is RLS-scoped at the source, and the lineage scope refines within the tenant.

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
    DataClassification Classification,
    Instant At);

public static class LineageCdc {
    public static IO<Seq<CdcEnvelope>> Feed(
        CdcScope scope,
        Func<SyncCursor, Seq<OpLogEntry>> changefeed,
        Func<UInt128, LineageSlice> lineage,
        Func<OpLogEntry, DataClassification, IO<(ReadOnlyMemory<byte> Bytes, bool Redacted, DataClassification Effective)>> redact) {
        var scopeKeys = scope.LineageRoot.Map(root =>
            lineage(root).Edges.Bind(LineageCdc.Touched).ToHashSet()).IfNone([]);
        return toSeq(changefeed(scope.Cursor)
            .Where(entry => scope.LineageRoot.IsNone || scopeKeys.Contains(entry.ContentKey)))
            .TraverseM(entry => redact(entry, scope.ClassificationCeiling).Map(projection =>
                new CdcEnvelope(
                    entry.Sequence, entry.EntityKind, entry.EntityKey, entry.Kind, entry.ContentKey,
                    projection.Bytes, projection.Redacted, projection.Effective, entry.Physical)))
            .As();
    }

    private static Seq<UInt128> Touched(ProvEdge edge) =>
        edge switch {
            ProvEdge.WasGeneratedBy g => Seq(g.Entity),
            ProvEdge.WasDerivedFrom d => Seq(d.Entity),
            ProvEdge.Used u => Seq(u.Entity),
            _ => Seq<UInt128>(),
        };
}
```

## [5]-[TS_PROJECTION]

- Owner: `ProvEdgeWire`, `ProvNodeKind`, `LineageSliceWire`, `LineageDirectionKind`, `AttestedEntryWire`, `LedgerHeadWire`, `CdcEnvelopeWire` — the provenance wire surface the audit dashboard and lineage explorer decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: content keys and digests cross as 16-byte binary; instants cross as ISO-8601 extended strings; 64-bit sequences decode as bigint under useBigInt64; the `ProvEdge` discriminator crosses as the W3C-PROV relation name and reconstructs as the literal union; the `CdcEnvelopeWire` carries the `redacted` flag so the explorer renders a redaction-preserving change distinctly; the `AttestedEntryWire` carries the prior and current digests so the explorer verifies a chain segment client-side.

```ts contract
type ProvNodeKind = "Agent" | "Activity" | "Entity";

type ProvRelation = "WasGeneratedBy" | "Used" | "WasDerivedFrom" | "WasAttributedTo" | "WasAssociatedWith" | "WasInformedBy";

type LineageDirectionKind = "Backward" | "Forward";

interface ProvEdgeWire {
  relation: ProvRelation;
  subject: Uint8Array;
  object: Uint8Array | string;
}

interface LineageSliceWire {
  root: Uint8Array;
  direction: LineageDirectionKind;
  edges: ProvEdgeWire[];
  vertexCount: number;
  depth: number;
}

interface AttestedEntryWire {
  sequence: bigint;
  contentKey: Uint8Array;
  priorDigest: Uint8Array;
  digest: Uint8Array;
  classification: string;
  auditCategory: string;
  redactedPayloadProof: Uint8Array | null;
  at: string;
}

interface LedgerHeadWire {
  sequence: bigint;
  digest: Uint8Array;
  at: string;
}

interface CdcEnvelopeWire {
  sequence: bigint;
  entityKind: string;
  entityKey: string;
  kind: "upsert" | "delete" | "presence";
  contentKey: Uint8Array;
  payload: Uint8Array;
  redacted: boolean;
  classification: string;
  at: string;
}
```

## [6]-[RESEARCH]

- [LINEAGE_SLICE_COST]: the bounded backward/forward slice cost over a 10^7-edge causal DAG projected from the op-log — whether the projected adjacency index (a content-key-to-edge GIN index on the op-log `Closure` column) keeps a depth-bounded slice linear in the reachable-edge count, and whether the derivation source resolution reads the `Closure` manifest without a recursive CTE on the hot path.
- [REDACTION_PRESERVING_VERIFY]: the attested-chain verification after a redaction — confirming that a redacted entry keeping its pre-redaction `Digest` and swapping the payload reference for the `ExportProof` content hash leaves `Verify` green, and the periodic content-addressed seal an external witness anchors against the live PG18 pgaudit category record.
