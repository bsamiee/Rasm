# [PERSISTENCE_VERSION_PROVENANCE]

Rasm.Persistence owns the causal lineage of every durable change as a W3C-PROV graph over the Marten changefeed plus a tamper-evident attested ledger that is the one authenticity authority the time-travel checkpoint chain and the structural merge defer to. `ProvNode` is the closed PROV vocabulary (`Entity` a content-addressed `ElementGraph`/delta/snapshot, `Activity` a commit/merge/import/solve, `Agent` a signed actor or software), `ProvEdge` the five PROV relations (`WasGeneratedBy`, `Used`, `WasAttributedTo`, `WasDerivedFrom`, `WasInformedBy`), and `CausalDag` the static surface deriving the lineage graph from the changefeed, walking ancestry and descent, and projecting the W3C-PROV-JSON egress a CDE consumes. `AttestedEntry` is the hash-chained, KMS-signed ledger row whose `Prior` back-link plus `SignedAuthorship` make any rewrite detectable — distinct from the non-cryptographic content chain the `Version/timetravel#TIME_TRAVEL` `Checkpoint` carries, because that chain proves reproducibility while THIS chain proves authenticity. The lineage rides the one changefeed (`Version/ledger#CHANGEFEED`) and the one content-address (`Element/codec#CONTENT_ADDRESS`), never a second store; `SignedAuthorship`/`Authority` arrive from `Element/identity#AUTHORITY`; `Hlc`/`CommitNode` arrive from `Version/commits`; `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive from AppHost.

## [01]-[INDEX]

- [01]-[CAUSAL_DAG]: the W3C-PROV node/edge vocabulary, lineage derivation from the changefeed, ancestry/descent walks, and PROV-JSON egress.
- [02]-[ATTESTED_LEDGER]: the hash-chained KMS-signed ledger, chain verification, and the tamper-evidence authority.

## [02]-[CAUSAL_DAG]

- Owner: `ProvKind` the `[SmartEnum<string>]` PROV-node-class axis; `ProvNode` the `[Union]` causal node (`Entity`/`Activity`/`Agent`); `ProvRelation` the `[SmartEnum<string>]` relation vocabulary carrying its PROV term and directionality; `ProvEdge` the typed causal edge; `LineageWalk` the bounded ancestry/descent frontier fold; `CausalDag` the static surface owning lineage derivation from the changefeed, the walks, and the PROV-JSON projection.
- Cases: `ProvNode` is `Entity(ContentAddress, ProvKind, Instant)` (a content-addressed graph/delta/snapshot/blob), `Activity(UInt128 Id, ProvKind Kind, Instant Started, Instant Ended)` (a commit/merge/import/solve span), `Agent(Principal, bool Signed)` (a signed actor or software agent); `ProvRelation` is `WasGeneratedBy | Used | WasAttributedTo | WasDerivedFrom | WasInformedBy` — the five W3C-PROV-O relations, a sixth being one row carrying its term, never a parallel edge family.
- Entry: `public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> resolve)` projects the lineage graph from the changefeed — each delta entry generates an `Entity` `WasGeneratedBy` its commit `Activity`, the activity `WasAttributedTo` its signed `Agent`, and a revised entity `WasDerivedFrom` its prior content key; `public static Seq<ProvNode> Ancestry(Func<ContentAddress, Seq<ProvEdge>> incoming, Func<UInt128, ProvKind> kindOf, ContentAddress entity, int depth)` walks the transitive `WasDerivedFrom`/`Used` ancestry, each reached node carrying its resolved `ProvKind`; `public static Seq<ProvNode> Descent(Func<ContentAddress, Seq<ProvEdge>> outgoing, Func<UInt128, ProvKind> kindOf, ContentAddress entity, int depth)` walks forward impact; `public static JsonElement ProvJson(Seq<ProvEdge> lineage)` projects the W3C-PROV-JSON document.
- Auto: lineage is derived from the changefeed, never a parallel provenance write — a `GraphRevised` delta IS the `WasGeneratedBy` evidence and its `Version/commits#COMMIT_DAG` parent IS the `WasDerivedFrom` source, so the PROV graph is a fold over the events the system already holds; the attribution edge reads the `Element/identity#AUTHORITY` `SignedAuthorship` so a `WasAttributedTo` names a verified actor when the op was KMS-signed and an unsigned `Agent` when the local tier minted it; the activity span reads the commit cell `Hlc` so the PROV `startedAtTime`/`endedAtTime` ride the one causal clock.
- Receipt: a lineage derivation rides `store.prov.derive` carrying the edge count; an ancestry/descent walk rides `store.prov.walk` carrying the reached-node count and depth.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Text.Json, BCL inbox.
- Growth: a new PROV relation is one `ProvRelation` row carrying its term; a new node class is one `ProvNode` case plus one `ProvKind` row; a new walk direction is one entry over `LineageWalk`; zero new surface — a parallel provenance store, a second lineage walker, or a free-string PROV term is the deleted form because the lineage is a fold over the changefeed and the PROV vocabulary is the closed W3C-PROV-O relation set.
- Boundary: the causal DAG is DERIVED from the changefeed — a delta entry's `WasGeneratedBy` edge, its commit's `WasAttributedTo` agent, and its prior content key's `WasDerivedFrom` source are all reads off the events, never a write of record, so the lineage is reconstructible from the one op stream a replica folds; the W3C-PROV vocabulary is the closed five-relation set so a PROV-JSON export is a standards-conformant CDE audit artifact; the attribution edge reconciles with the `Version/timetravel#TIME_TRAVEL` `BlameRow` (the same `(Hlc, origin)` winner the convergence selected) so blame and provenance never disagree; the lineage walk is bounded breadth-first over the typed adjacency so the cost is linear in the reachable-edge count within the depth bound; a software `Agent` (a Compute solver, an IFC importer) is the `WasAttributedTo` target of an automated activity so a derived `Assessment` result names its solver, never an anonymous machine write.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvKind {
    public static readonly ProvKind Graph = new("graph");
    public static readonly ProvKind Delta = new("delta");
    public static readonly ProvKind Snapshot = new("snapshot");
    public static readonly ProvKind Blob = new("blob");
    public static readonly ProvKind Commit = new("commit");
    public static readonly ProvKind Merge = new("merge");
    public static readonly ProvKind Import = new("import");
    public static readonly ProvKind Solve = new("solve");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProvRelation {
    public static readonly ProvRelation WasGeneratedBy = new("wasGeneratedBy", "prov:wasGeneratedBy");
    public static readonly ProvRelation Used = new("used", "prov:used");
    public static readonly ProvRelation WasAttributedTo = new("wasAttributedTo", "prov:wasAttributedTo");
    public static readonly ProvRelation WasDerivedFrom = new("wasDerivedFrom", "prov:wasDerivedFrom");
    public static readonly ProvRelation WasInformedBy = new("wasInformedBy", "prov:wasInformedBy");
    public string Term { get; }
    private ProvRelation(string key, string term) : this(key) => Term = term;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvNode {
    private ProvNode() { }
    public sealed record Entity(ContentAddress Address, ProvKind Kind, Instant At) : ProvNode;
    public sealed record Activity(UInt128 Id, ProvKind Kind, Instant Started, Instant Ended) : ProvNode;
    public sealed record Agent(Principal Principal, bool Signed) : ProvNode;
}

public readonly record struct ProvEdge(ProvRelation Relation, UInt128 From, UInt128 To, Hlc Cell);

public static class CausalDag {
    public static Seq<ProvEdge> Derive(Seq<OpLogEntry> changefeed, Func<UInt128, Option<CommitNode>> resolve) =>
        changefeed.Bind(entry => {
            var commit = CommitOf(entry, resolve);
            var generated = commit.Map(c => new ProvEdge(ProvRelation.WasGeneratedBy, entry.ContentKey, c, entry.Stamp));
            var attributed = commit.Map(c => new ProvEdge(ProvRelation.WasAttributedTo, c, AgentKey(entry.Actor), entry.Stamp));
            var derived = entry.Closure.HeadOrNone().Map(prior => new ProvEdge(ProvRelation.WasDerivedFrom, entry.ContentKey, prior, entry.Stamp));
            return generated.ToSeq() + attributed.ToSeq() + derived.ToSeq();
        });

    public static Seq<ProvNode> Ancestry(Func<ContentAddress, Seq<ProvEdge>> incoming, Func<UInt128, ProvKind> kindOf, ContentAddress entity, int depth) =>
        Walk(incoming, kindOf, entity, depth, static e => e.To, HashSet(entity.Value), Seq<ProvNode>());

    public static Seq<ProvNode> Descent(Func<ContentAddress, Seq<ProvEdge>> outgoing, Func<UInt128, ProvKind> kindOf, ContentAddress entity, int depth) =>
        Walk(outgoing, kindOf, entity, depth, static e => e.From, HashSet(entity.Value), Seq<ProvNode>());

    public static JsonElement ProvJson(Seq<ProvEdge> lineage) {
        var document = lineage.Fold(new System.Collections.Generic.Dictionary<string, object>(),
            (acc, edge) => { acc[$"{edge.From:x32}->{edge.To:x32}"] = new { relation = edge.Relation.Term, at = edge.Cell.Physical.ToString() }; return acc; });
        return JsonSerializer.SerializeToElement(document, ElementJson.Options);
    }

    static Seq<ProvNode> Walk(Func<ContentAddress, Seq<ProvEdge>> adjacency, Func<UInt128, ProvKind> kindOf, ContentAddress root, int depth, Func<ProvEdge, UInt128> step, HashSet<UInt128> seen, Seq<ProvNode> reached) =>
        depth <= 0 ? reached
        : adjacency(root).Fold((Seen: seen, Reached: reached), (acc, edge) => step(edge) is var next && acc.Seen.Contains(next)
            ? acc : (acc.Seen.Add(next), acc.Reached.Add(new ProvNode.Entity(ContentAddress.Create(next), kindOf(next), edge.Cell.Physical))))
            is var folded && folded.Reached.Count > reached.Count
                ? folded.Reached.Bind(node => node is ProvNode.Entity e ? Walk(adjacency, kindOf, e.Address, depth - 1, step, folded.Seen, folded.Reached) : folded.Reached).Distinct().ToSeq()
                : folded.Reached;

    static Option<UInt128> CommitOf(OpLogEntry entry, Func<UInt128, Option<CommitNode>> resolve) => resolve(entry.ContentKey).Map(static c => c.ContentKey);
    static UInt128 AgentKey(string actor) => XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(actor));
}
```

| [INDEX] | [POLICY]            | [VALUE]                            | [BINDING]                                                  |
| :-----: | :------------------ | :--------------------------------- | :-------------------------------------------------------- |
|  [01]   | lineage source      | derived from the changefeed        | never a parallel provenance write                         |
|  [02]   | PROV vocabulary     | closed five-relation W3C-PROV-O set | a PROV-JSON export is a standards CDE audit artifact      |
|  [03]   | attribution         | `Element/identity#AUTHORITY` `SignedAuthorship` | reconciles with the `BlameRow` winner             |
|  [04]   | walk cost           | bounded breadth-first              | linear in reachable edges within the depth bound          |

## [03]-[ATTESTED_LEDGER]

- Owner: `AttestedEntry` the hash-chained, KMS-signed ledger row; `AttestVerdict` the closed chain-validity verdict; `AttestedLedger` the static surface owning the chain append, the rolling content-address fold, and the chain verification that is the one tamper-evidence authority.
- Cases: a `AttestedEntry` carries the entry content key, the `Prior` back-link, the rolling `Chain` address (`XxHash128` over `Prior ++ ContentKey`), and the optional `SignedAuthorship`; `AttestVerdict` is `Authentic | Broken(at) | Unsigned | Forged(at)` — `Authentic` the verified chain, `Broken` a content-address discontinuity, `Unsigned` a local-tier chain with no KMS signature, `Forged` a signature that fails verification.
- Entry: `public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship)` extends the chain with the new rolling address; `public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, OpDigest, IO<bool>> verifySignature)` re-folds the chain and confirms every back-link, rolling address, and signature.
- Auto: the ledger is the AUTHENTICITY authority distinct from the reproducibility chain — the `Version/timetravel#TIME_TRAVEL` `Checkpoint.Hash` is a non-cryptographic content chain that proves a checkpoint reproduces from the op stream, while THIS chain's `SignedAuthorship` proves the entry was authored by a verified actor and not rewritten; verification re-folds the rolling address over the back-links and runs the KMS `verifySignature` per signed entry so a rewritten history breaks the chain at the rewrite point.
- Receipt: a chain append rides `store.attest.append`; a verification rides `store.attest.verify` carrying the verdict and the break locus when broken.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new verdict is one `AttestVerdict` case; zero new surface — a second tamper-evidence scheme, a Merkle-tree audit log beside this chain, or a content chain claiming authenticity is the deleted form because this ledger owns authenticity and the checkpoint chain owns reproducibility, two distinct concerns.
- Boundary: the attested ledger is the ONE tamper-evidence authority — the `Version/timetravel#TIME_TRAVEL` `Checkpoint` hash chain explicitly defers here and carries no authenticity claim, so a content chain standing in for tamper-evidence is the deleted form; the chain is hash-chained off the prior rolling address so any inserted, deleted, or reordered entry breaks every downstream address (a `Broken` verdict naming the discontinuity); the `SignedAuthorship` is the per-entry KMS signature so a verified `Authentic` proves the actor and the order, an `Unsigned` chain (the local/Personal tier) proves order only, and a `Forged` verdict names the entry whose signature fails; verification runs the KMS `verifySignature` per signed entry so the chain's authenticity is a real probe, never a prose claim.

```csharp signature
public readonly record struct AttestedEntry(UInt128 ContentKey, Option<UInt128> Prior, UInt128 Chain, Option<SignedAuthorship> Authorship, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AttestVerdict {
    private AttestVerdict() { }
    public sealed record Authentic(int Entries) : AttestVerdict;
    public sealed record Broken(int At, UInt128 Expected, UInt128 Found) : AttestVerdict;
    public sealed record Unsigned(int Entries) : AttestVerdict;
    public sealed record Forged(int At, Principal Actor) : AttestVerdict;
}

public static class AttestedLedger {
    public static AttestedEntry Append(Option<AttestedEntry> prior, UInt128 contentKey, Option<SignedAuthorship> authorship) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        prior.Iter(p => { BinaryPrimitives.WriteUInt128LittleEndian(word, p.Chain); rolling.Append(word); });
        BinaryPrimitives.WriteUInt128LittleEndian(word, contentKey);
        rolling.Append(word);
        return new AttestedEntry(contentKey, prior.Map(static p => p.Chain), rolling.GetCurrentHashAsUInt128(), authorship, authorship.Map(static a => a.At).IfNone(Instant.MinValue));
    }

    public static IO<AttestVerdict> Verify(Seq<AttestedEntry> chain, Func<SignedAuthorship, OpDigest, IO<bool>> verifySignature) =>
        chain.FoldM(
            (State: Option<AttestedEntry>.None, Verdict: (AttestVerdict)new AttestVerdict.Authentic(0), Index: 0, Signed: 0),
            (acc, entry) => {
                var recomputed = Append(acc.State, entry.ContentKey, entry.Authorship);
                return recomputed.Chain != entry.Chain || acc.State.Map(static s => s.Chain) != entry.Prior
                    ? IO.pure((Some(entry), (AttestVerdict)new AttestVerdict.Broken(acc.Index, recomputed.Chain, entry.Chain), acc.Index + 1, acc.Signed))
                    : entry.Authorship.Match(
                        Some: authorship => verifySignature(authorship, authorship.Digest).Map(valid => valid
                            ? (Some(entry), acc.Verdict, acc.Index + 1, acc.Signed + 1)
                            : (Some(entry), (AttestVerdict)new AttestVerdict.Forged(acc.Index, authorship.Actor), acc.Index + 1, acc.Signed)),
                        None: () => IO.pure((Some(entry), acc.Verdict, acc.Index + 1, acc.Signed)));
            })
            .Map(final => final.Verdict is AttestVerdict.Authentic ? (final.Signed == 0 && chain.Count > 0 ? new AttestVerdict.Unsigned(chain.Count) : new AttestVerdict.Authentic(chain.Count)) : final.Verdict);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | tamper-evidence     | hash-chained + KMS-signed              | the one authenticity authority; checkpoint chain defers   |
|  [02]   | chain break         | rolling-address discontinuity          | any insert/delete/reorder breaks every downstream address |
|  [03]   | signature           | per-entry `SignedAuthorship`           | `Authentic` proves actor + order; `Unsigned` order only   |
|  [04]   | verification        | re-fold + KMS `verifySignature`        | a real probe, never a prose claim                         |
