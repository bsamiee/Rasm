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
        edge.Switch(
            wasGeneratedBy:    static g => Some(g.Activity),
            used:              static _ => Option<UInt128>.None,
            wasDerivedFrom:    static d => Some(d.Source),
            wasAttributedTo:   static _ => Option<UInt128>.None,
            wasAssociatedWith: static _ => Option<UInt128>.None,
            wasInformedBy:     static i => Some(i.Informant));

    private static Option<UInt128> Head(ProvEdge edge) =>
        edge.Switch(
            wasGeneratedBy:    static g => Some(g.Entity),
            used:              static u => Some(u.Entity),
            wasDerivedFrom:    static _ => Option<UInt128>.None,
            wasAttributedTo:   static _ => Option<UInt128>.None,
            wasAssociatedWith: static _ => Option<UInt128>.None,
            wasInformedBy:     static _ => Option<UInt128>.None);
}
```

## [3]-[ATTESTED_LEDGER]

- Owner: `AttestedEntry` the hash-chained ledger row; `LedgerHead` the chain-head proof; `AttestedLedger` the static surface owning the chain-append fold, the head-digest computation, the chain verification, and the redaction-preserving entry projection; `TransparencyProof` the RFC 9162 Merkle transparency-log surface owning the leaf/node digest, the `InclusionProof` for one entry, the `ConsistencyProof` between two `LedgerHead` seals, and the detached-signed `HeadSeal` an external auditor verifies in O(log n) without replaying the chain.
- Cases: each entry chains `PriorDigest` into its own `Digest` over `XxHash128(PriorDigest || ContentKey || ProviderFingerprint || Classification || AuditCategory)`, so the head digest attests every prior entry and a graduated-surrogate entry's `(checksum, OrtEpDevice)` provider fingerprint is bound into the chain; a non-graduation row carries `ProviderFingerprint = None` folding the zero term so the digest term is uniform; a redacted entry preserves its `Digest` while replacing the payload reference so the chain verifies even after a redaction; `TransparencyProof` builds a Merkle tree over the `AttestedEntry` leaf digests so an `InclusionProof` proves one entry was logged in O(log n), a `ConsistencyProof` proves the new tree extends the old append-only between two `HeadSeal` sizes, and the `HeadSeal` binds an `ECDsa` detached signature over the periodic `(Root, At)` so non-repudiation attaches to the signed seal, not the tree.
- Entry: `public static AttestedEntry Append(LedgerHead head, OpLogEntry op, string auditCategory, DataClassification classification)` — pure chain-append linking the prior head digest into the new entry over the op content key, audit category, and classification; `public static Fin<LedgerHead> Verify(Seq<AttestedEntry> chain, LedgerHead start)` recomputes every digest and aborts on the first break.
- Auto: every op-log row plus its bound pgaudit category (`AuditBinding.Bind`) appends one `AttestedEntry` whose digest chains the prior digest, so the head digest is a tamper-evident proof of the whole history — a single altered prior row breaks every subsequent digest and `Verify` names the first break; the chain rides the op-log so it carries no second store; a graduated offline-science surrogate enters as one `AttestedEntry` keyed by its `ContentIdentity` carrying the `(checksum, OrtEpDevice)` provider fingerprint in the dedicated `ProviderFingerprint` field, so the `ONE_GRADUATION_EVIDENCE` `HandoffAxis` result chains into the tamper-evident ledger and the determinism closure (`AppHost/determinism/determinism-and-replay#EVENT_LOG`) references one durable content key, the surrogate ONNX bytes residing on the `cache/indexes#ARTIFACT_BLOB_INDEX` `GraduationAsset` row; a re-import re-verifies the fingerprint at admission so a provider-divergent fit is caught before inference; redaction is preserving — a redacted entry keeps its original `Digest` (computed over the pre-redaction content key) and replaces the payload reference with the `ExportProof` content hash so the chain still verifies and an auditor proves the row existed without seeing the redacted content; the head digest is periodically sealed as a content-addressed snapshot (`Snapshots.ContentAddress`) so an external witness anchors the chain.
- Receipt: an append rides `store.ledger.append` carrying the new head digest; a verification rides `store.ledger.verify` carrying the verified count or the first break index; a redaction-preserving projection rides the settled `ExportProof`.
- Packages: System.IO.Hashing, System.Security.Cryptography (BCL inbox), NodaTime, LanguageExt.Core, Microsoft.Extensions.Compliance.Redaction, Rasm.AppHost (project), BCL inbox.
- Growth: a new attestation field is one term in the digest tuple AND one matching `AttestedEntry` field AND one symmetric write in both `Append` and `Recompute` (a one-sided edit breaks `Verify` on the first entry); the `ProviderFingerprint` graduation term is exactly this — a dedicated `Option<UInt128>` field plus the digest term plus the symmetric buffer growth, never the live `RedactedPayloadProof` slot whose reuse would collide with redaction and corrupt the leaf digest; extending the digest tuple is a FORWARD-ONLY ledger format epoch with a genesis-anchored re-seal, never a retroactive rewrite of existing attested entries; a new seal cadence is one schedule policy value; the transparency log is a projection over the existing `AttestedEntry` rows so an inclusion or consistency proof is one `TransparencyProof` fold, never a second ledger store; zero new surface — a separate Merkle-tree audit store, a blockchain ledger, a sibling graduation-evidence record, or a second tamper-evident log is the deleted form because the chain is one hash-chained projection over the op-log and the pgaudit binding, the Merkle tree is one projection over the same leaf digests, and the head digest plus the detached-signed seal is the one external proof; the existing pgaudit/legal-hold/RLS/CDC machinery (`redaction-retention`, `server-tier`) stays the substrate and this owner adds the hash-chain attestation, the lineage scoping, the RFC 9162 transparency proof, and the graduation-evidence chaining net-new.
- Boundary: the chain is hash-linked so tamper-evidence is structural — `Digest_n = XxHash128(Digest_{n-1} || ContentKey || ProviderFingerprint || Classification || AuditCategory)` so altering any entry invalidates every later digest and `Verify` returns the first break index, never a silent pass; the `ProviderFingerprint` term is a DEDICATED `Option<UInt128>` field folding `UInt128.Zero` when `None`, never the live `RedactedPayloadProof` slot — reusing that slot would collide with redaction (a redacted graduation entry would overwrite the fingerprint and corrupt the `LeafDigest`); the digest extension grows the `Append`/`Recompute` `stackalloc byte[80]` buffer (16 prior digest + 16 content key + 16 fingerprint + the UTF8 `classification|category` tail) and BOTH `Append` and `Recompute` write the identical tuple layout or `Verify` breaks on the first entry; the tuple-term addition is a FORWARD-ONLY format epoch with a genesis-anchored re-seal, never a retroactive rewrite — existing attested entries keep their original digests under the prior epoch and the new epoch chains forward from a re-sealed `LedgerHead`; the Merkle node hash stays `XxHash128` for tamper-evidence (the `0x00` leaf-prefix and `0x01` node-prefix domain-separate the RFC 9162 tree so a leaf digest can never collide with an interior node) while the `HeadSeal` `ECDsa` detached signature is the only cryptographic surface — so the non-repudiation claim attaches to the signed seal, not the tree, and a cryptographic-signature claim on the chain digests themselves is the named defect; `TransparencyProof.Prove` emits an `InclusionProof` whose `AuditPath` an auditor walks in O(log n) to recompute the `Root` without the chain, `Consistent` emits a `ConsistencyProof` so a split-history attack surfaces as a failed extension proof between two `HeadSeal` sizes, and the support-bundle export carries the proof through `retention/redaction-retention.md#EXPORT_PROOF` as a self-verifying artifact rather than a server-trusted digest, never a second ledger store; the redaction-preserving leaf keeps its pre-redaction `Digest` and folds the `RedactedPayloadProof` content hash into the `LeafDigest` so an inclusion proof still verifies after a redaction, matching the existing chain invariant; external non-repudiation rides the periodic detached-signed `HeadSeal` (its `Root` is the Merkle root and its `At` rides `Snapshots.ContentAddress` as the content-addressed head-seal anchor) an outside witness anchors; redaction is preserving — a redacted entry keeps its pre-redaction digest and swaps the payload reference for the `ExportProof` content hash, so the chain verifies after redaction and the redacted row's existence and classification stay provable while its content is erased; the pgaudit category per entry comes from the settled `AuditBinding.Bind` (`redaction-retention#AUDIT_BINDING`) so the attested ledger and the server-side audit log carry the same category; legal hold reads the lineage subtree (`#CAUSAL_DAG`) so a held entry's whole derivation chain is retained; the chain rides the op-log changefeed so it converges across peers and a fork in the chain (two entries claiming the same prior digest) is a typed conflict the merge surfaces, never a silent overwrite.

```csharp signature
public readonly record struct AttestedEntry(
    long Sequence,
    UInt128 ContentKey,
    UInt128 PriorDigest,
    UInt128 Digest,
    DataClassification Classification,
    string AuditCategory,
    Option<UInt128> ProviderFingerprint,
    Option<UInt128> RedactedPayloadProof,
    Instant At);

public readonly record struct LedgerHead(long Sequence, UInt128 Digest, Instant At);

public static class AttestedLedger {
    public static readonly LedgerHead Genesis = new(0L, UInt128.Zero, Instant.MinValue);

    public static AttestedEntry Append(LedgerHead head, OpLogEntry op, string auditCategory, DataClassification classification, Option<UInt128> providerFingerprint = default) {
        Span<byte> tuple = stackalloc byte[80];
        BinaryPrimitives.WriteUInt128LittleEndian(tuple, head.Digest);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[16..], op.ContentKey);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[32..], providerFingerprint.IfNone(UInt128.Zero));
        var written = 48 + Encoding.UTF8.GetBytes($"{classification.Key}|{auditCategory}", tuple[48..]);
        return new AttestedEntry(
            head.Sequence + 1L, op.ContentKey, head.Digest,
            XxHash128.HashToUInt128(tuple[..written]),
            classification, auditCategory, providerFingerprint, None, op.Physical);
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
        Span<byte> tuple = stackalloc byte[80];
        BinaryPrimitives.WriteUInt128LittleEndian(tuple, head.Digest);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[16..], entry.ContentKey);
        BinaryPrimitives.WriteUInt128LittleEndian(tuple[32..], entry.ProviderFingerprint.IfNone(UInt128.Zero));
        var written = 48 + Encoding.UTF8.GetBytes($"{entry.Classification.Key}|{entry.AuditCategory}", tuple[48..]);
        return XxHash128.HashToUInt128(tuple[..written]);
    }
}

public readonly record struct InclusionProof(int LeafIndex, int TreeSize, UInt128 LeafDigest, Seq<UInt128> AuditPath, UInt128 Root);

public readonly record struct ConsistencyProof(int OldSize, int NewSize, Seq<UInt128> Path, UInt128 OldRoot, UInt128 NewRoot);

public readonly record struct HeadSeal(int TreeSize, UInt128 Root, Instant At, byte[] Signature);

public static class TransparencyProof {
    private static readonly byte[] LeafPrefix = [0x00];

    private static readonly byte[] NodePrefix = [0x01];

    public static UInt128 LeafDigest(AttestedEntry entry) {
        Span<byte> span = stackalloc byte[33];
        LeafPrefix.CopyTo(span);
        BinaryPrimitives.WriteUInt128LittleEndian(span[1..], entry.Digest);
        BinaryPrimitives.WriteUInt128LittleEndian(span[17..], entry.RedactedPayloadProof.IfNone(entry.ContentKey));
        return XxHash128.HashToUInt128(span);
    }

    public static UInt128 Root(Seq<AttestedEntry> chain) =>
        MerkleRoot(toSeq(chain.Map(LeafDigest)));

    public static InclusionProof Prove(Seq<AttestedEntry> chain, int leafIndex) {
        var leaves = toSeq(chain.Map(LeafDigest));
        return new InclusionProof(leafIndex, leaves.Count, leaves[leafIndex], AuditPath(leaves, leafIndex), MerkleRoot(leaves));
    }

    public static bool Verify(InclusionProof proof) {
        var node = proof.LeafDigest;
        var index = proof.LeafIndex;
        var size = proof.TreeSize;
        foreach (var sibling in proof.AuditPath) {
            (node, index, size) = SiblingFold(node, sibling, index, size);
        }
        return index == 0 && node == proof.Root;
    }

    private static (UInt128 Node, int Index, int Size) SiblingFold(UInt128 node, UInt128 sibling, int index, int size) {
        while ((index & 1) == 0 && index != size - 1) {
            index >>= 1;
            size = (size + 1) >> 1;
        }
        var folded = (index & 1) == 1 ? Node(sibling, node) : Node(node, sibling);
        return (folded, index >> 1, (size + 1) >> 1);
    }

    public static ConsistencyProof Consistent(Seq<AttestedEntry> chain, int oldSize) {
        var leaves = toSeq(chain.Map(LeafDigest));
        return new ConsistencyProof(
            oldSize, leaves.Count,
            ConsistencyPath(leaves, oldSize),
            MerkleRoot(toSeq(leaves.Take(oldSize))),
            MerkleRoot(leaves));
    }

    public static HeadSeal Seal(Seq<AttestedEntry> chain, Instant at, ECDsa key) {
        var root = Root(chain);
        Span<byte> message = stackalloc byte[24];
        BinaryPrimitives.WriteUInt128LittleEndian(message, root);
        BinaryPrimitives.WriteInt64LittleEndian(message[16..], at.ToUnixTimeTicks());
        return new HeadSeal(chain.Count, root, at, key.SignData(message, HashAlgorithmName.SHA256));
    }

    public static bool VerifySeal(HeadSeal seal, ECDsa key) {
        Span<byte> message = stackalloc byte[24];
        BinaryPrimitives.WriteUInt128LittleEndian(message, seal.Root);
        BinaryPrimitives.WriteInt64LittleEndian(message[16..], seal.At.ToUnixTimeTicks());
        return key.VerifyData(message, seal.Signature, HashAlgorithmName.SHA256);
    }

    private static UInt128 MerkleRoot(Seq<UInt128> leaves) =>
        leaves.Count switch {
            0 => XxHash128.HashToUInt128(ReadOnlySpan<byte>.Empty),
            1 => leaves[0],
            var n => Node(MerkleRoot(toSeq(leaves.Take(Split(n)))), MerkleRoot(toSeq(leaves.Skip(Split(n))))),
        };

    private static Seq<UInt128> AuditPath(Seq<UInt128> leaves, int index) {
        if (leaves.Count <= 1)
            return Seq<UInt128>();
        var k = Split(leaves.Count);
        return index < k
            ? AuditPath(toSeq(leaves.Take(k)), index).Add(MerkleRoot(toSeq(leaves.Skip(k))))
            : AuditPath(toSeq(leaves.Skip(k)), index - k).Add(MerkleRoot(toSeq(leaves.Take(k))));
    }

    private static Seq<UInt128> ConsistencyPath(Seq<UInt128> leaves, int oldSize) =>
        oldSize == 0 || oldSize == leaves.Count
            ? Seq<UInt128>()
            : Subproof(leaves, oldSize, true);

    private static Seq<UInt128> Subproof(Seq<UInt128> leaves, int m, bool onPath) {
        var n = leaves.Count;
        if (m == n)
            return onPath ? Seq<UInt128>() : Seq(MerkleRoot(leaves));
        var k = Split(n);
        return m <= k
            ? Subproof(toSeq(leaves.Take(k)), m, onPath).Add(MerkleRoot(toSeq(leaves.Skip(k))))
            : Subproof(toSeq(leaves.Skip(k)), m - k, false).Add(MerkleRoot(toSeq(leaves.Take(k))));
    }

    private static UInt128 Node(UInt128 left, UInt128 right) {
        Span<byte> span = stackalloc byte[33];
        NodePrefix.CopyTo(span);
        BinaryPrimitives.WriteUInt128LittleEndian(span[1..], left);
        BinaryPrimitives.WriteUInt128LittleEndian(span[17..], right);
        return XxHash128.HashToUInt128(span);
    }

    private static int Split(int n) {
        var k = 1;
        while (k << 1 < n)
            k <<= 1;
        return k;
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                          | [BINDING]                                       |
| :-----: | :------------------ | :------------------------------- | :---------------------------------------------- |
|   [1]   | digest algorithm    | `XxHash128(prior‖content‖fingerprint‖class‖category)` | non-cryptographic; signature claim is rejected; `stackalloc byte[80]` symmetric in `Append`/`Recompute` |
|   [2]   | seal cadence        | content-addressed snapshot seal   | external-witness anchor on the schedule lease   |
|   [8]   | graduation evidence | `ProviderFingerprint` dedicated `Option<UInt128>` | `(checksum, OrtEpDevice)` bound into the digest term, never the `RedactedPayloadProof` slot; forward-only format epoch with genesis re-seal |
|   [3]   | audit category      | `AuditBinding.Bind` result        | one category vocabulary, never a second         |
|   [4]   | merkle tree         | `XxHash128` `0x00`/`0x01`-prefixed | RFC 9162 leaf/node domain separation; `TransparencyProof.Root` |
|   [5]   | inclusion proof     | O(log n) `AuditPath` recompute    | one entry proven without the chain; export-bundle artifact |
|   [6]   | consistency proof   | append-only extension between seals | split-history attack is a failed `ConsistencyProof` |
|   [7]   | head-seal signature | `ECDsa` detached over `(Root, At)` | the one cryptographic surface; non-repudiation on the seal |

## [4]-[LINEAGE_CDC]

- Owner: `CdcScope` the lineage-filter scope; `CdcEnvelope` the redaction-aware change record carrying the originating `TraceContext` slot beside `Redacted` so a lineage-CDC row inherits the span the op was committed under; `LineageCdc` the static surface owning the lineage-scoped CDC filter, the redaction-preserving payload projection, and the per-consumer feed fold.
- Cases: a scope filters the changefeed to a lineage subtree (every entity derived from a root), to a classification ceiling, and to a tenant; the feed projects each in-scope change with its payload redacted to the consumer's classification authority.
- Entry: `public static IO<Seq<CdcEnvelope>> Feed(CdcScope scope, Func<SyncCursor, Seq<OpLogEntry>> changefeed, Func<UInt128, LineageSlice> lineage, Func<OpLogEntry, DataClassification, IO<ReadOnlyMemory<byte>>> redact)` — folds the changefeed past the cursor, filters to the lineage scope and classification ceiling, and projects each in-scope change with its payload redacted.
- Auto: lineage-filtered CDC is the changefeed restricted to a derivation subtree — a consumer subscribes to a root entity and receives only changes to entities the lineage DAG derives from it, so a multi-tenant or discipline-scoped consumer never sees out-of-scope rows; the filter composes the lineage slice (`#CAUSAL_DAG`) so the scope is the forward slice of the root; redaction is preserving — a change whose classification exceeds the consumer's authority emits with its payload redacted through the settled `ExportProof`/`RedactorKind` so the change's existence and metadata stay visible while its content is masked, never a silent drop; the feed rides the op-log cursor so it is the same changefeed the sync transport drains, scoped per consumer.
- Receipt: a feed rides `store.cdc.feed` carrying the in-scope count, the redacted count, and the advanced cursor.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Microsoft.Extensions.Compliance.Redaction, Rasm.AppHost (project), BCL inbox.
- Growth: a new scope dimension is one column on `CdcScope`; a new redaction policy is one `RedactorKind` row (owned at AppHost); zero new surface — a per-consumer CDC trigger, a second change-data-capture pipeline, or a row-level filter table is the deleted form because the feed is the one changefeed scoped by the lineage slice and the classification ceiling, and redaction rides the settled export-proof redactor.
- Boundary: lineage-scoped CDC is the changefeed filtered by the forward lineage slice so a consumer receives exactly its derivation subtree — a coarse table-level CDC or an unscoped firehose is the deleted form; the classification ceiling is the consumer's authority so an out-of-authority change emits redaction-preserving (existence and metadata visible, content masked) rather than dropped, because a silent drop hides that a change occurred and a redaction-preserving feed proves a change occurred while protecting its content; redaction routes through the settled `redaction-retention#EXPORT_PROOF` redactor resolution (`RedactorKind.Hmac` join-preserving, `RedactorKind.None` pass-through, unmapped fail-closed by erasure) so the CDC and the support-bundle export share one redactor, never a second masking path; the feed rides the op-log cursor (`SyncCursor`) so it is the same changefeed the sync transport drains and a consumer's cursor advances exactly past its in-scope changes; tenant scope composes the RLS predicate (`provisioning#TENANCY_RLS`) so a per-tenant CDC feed is RLS-scoped at the source, and the lineage scope refines within the tenant; the `CdcEnvelope` carries the originating `OpLogEntry.Trace` W3C trace-context unchanged through the `Feed` projection so a lineage-CDC consumer extract-and-continues the same span the op was committed under — realizing the `ONE_DISTRIBUTED_TRACE` seam on the CDC face — never minting a second tracer, and an op committed under no span carries `TraceContext.Empty`.

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
    TraceContext Trace,
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
                    projection.Bytes, projection.Redacted, entry.Trace, projection.Effective, entry.Physical)))
            .As();
    }

    private static Seq<UInt128> Touched(ProvEdge edge) =>
        edge.Switch(
            wasGeneratedBy:    static g => Seq(g.Entity),
            used:              static u => Seq(u.Entity),
            wasDerivedFrom:    static d => Seq(d.Entity),
            wasAttributedTo:   static _ => Seq<UInt128>(),
            wasAssociatedWith: static _ => Seq<UInt128>(),
            wasInformedBy:     static _ => Seq<UInt128>());
}
```

## [5]-[TS_PROJECTION]

- Owner: `ProvEdgeWire`, `ProvNodeKind`, `LineageSliceWire`, `LineageDirectionKind`, `AttestedEntryWire`, `LedgerHeadWire`, `CdcEnvelopeWire` — the provenance wire surface the audit dashboard and lineage explorer decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: content keys and digests cross as 16-byte binary; instants cross as ISO-8601 extended strings; 64-bit sequences decode as bigint under useBigInt64; the `ProvEdge` discriminator crosses as the W3C-PROV relation name and reconstructs as the literal union; the `CdcEnvelopeWire` carries the `redacted` flag so the explorer renders a redaction-preserving change distinctly and an APPEND-ONLY optional `trace` slot (16-byte `traceparent` + opaque `tracestate`) beside it so a lineage-CDC consumer extract-and-continues the originating span, the slot's optionality preserving the existing field sequence; the `AttestedEntryWire` carries the prior and current digests so the explorer verifies a chain segment client-side, plus the optional `providerFingerprint` so a graduated-surrogate entry surfaces its `(checksum, OrtEpDevice)` binding distinctly from a redaction.

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
  providerFingerprint: Uint8Array | null;
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
  trace?: { traceparent: Uint8Array; tracestate: Uint8Array };
  classification: string;
  at: string;
}
```

## [6]-[RESEARCH]

- [LINEAGE_SLICE_COST]: the bounded backward/forward slice cost over a 10^7-edge causal DAG projected from the op-log — whether the projected adjacency index (a content-key-to-edge GIN index on the op-log `Closure` column) keeps a depth-bounded slice linear in the reachable-edge count, and whether the derivation source resolution reads the `Closure` manifest without a recursive CTE on the hot path.
- [REDACTION_PRESERVING_VERIFY]: the attested-chain verification after a redaction — confirming that a redacted entry keeping its pre-redaction `Digest` and swapping the payload reference for the `ExportProof` content hash leaves `Verify` green, and the periodic content-addressed seal an external witness anchors against the live PG18 pgaudit category record.
- [MERKLE_PROOF_CONSTRUCTION]: the RFC 9162 inclusion-and-consistency-proof construction the `TransparencyProof.AuditPath`/`Subproof` folds emit — confirming the audit-path sibling ordering and the consistency-proof subproof recursion verify against the reference RFC 9162 test vectors, the `ECDsa` curve and `HashAlgorithmName` the `HeadSeal` signs under (the key arrives from the AppHost identity seam, never minted here), and the `o.IsExact` tree-size boundary where `n` is not a power of two so the `Split` largest-power-of-two split matches the spec; the redaction-preserving `LeafDigest` folding `RedactedPayloadProof` confirms an inclusion proof verifies before and after a redaction over a real chain.
