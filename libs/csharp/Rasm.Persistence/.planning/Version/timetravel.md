# [PERSISTENCE_VERSION_TIMETRAVEL]

Rasm.Persistence AS-OF time travel over the Marten event substrate: one `TimeTravel` engine that reconstructs, diffs, blames, scrubs, bisects, checkpoints, and branches-from-past over the `Element/graph#STREAM_GRAIN` `GraphEvent` stream and the periodic Marten snapshot, every operation folding the model stream's `GraphDelta` prefix up to a `TimeCut` through the IDENTICAL `Rasm.Element` `GraphDelta.ReplayOnto` the inline `Element/graph#GRAPH_PROJECTION` projection runs, so a historical materialization is bit-identical to the live `ElementGraph` at that cut — there is no second materializer (`H11`: TimeTravel is the read layer that PROJECTS from the Marten events, re-keyed to `NodeId`/`Relationship`, never a bespoke `OpLogEntry` replay). The cut is a precise `Hlc`, an `Instant`, or a Marten stream version — one value object, three modalities binding `Marten` `AggregateStreamAsync(version|timestamp)`; the branch scope resolves through the `Version/commits#COMMIT_DAG` `BranchRef` head the cut reconstructs to. The `Checkpoint` is the content-addressed seal of a reconstructed `ElementGraph` (`Element/codec#CONTENT_ADDRESS` `ContentAddress.OfGraph`) plus a hash-chained content address, so a checkpoint sequence is itself a verifiable reproducibility chain the `Version/recovery#POINT_IN_TIME_RESTORE` ladder archives and the `Version/retention#RETENTION_CLASSES` `snapshot` class governs — its `AsOfKey` (= `Address`) is the S3 cross-runtime seam key the `python:data/gridded/virtual` icechunk snapshot identity and the `Version/recovery#ReAttest` content-identity oracle BOTH read, while the prior-linked `Hash` chain stays tamper evidence only, never the cross-runtime key. `ReceiptSinkPort` arrives from AppHost and the clock/correlation ingredients ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame ([A.1]); `Hlc`, `CommitNode`, `BranchRef`, `CommitMessage`, `RefKind` arrive from `Version/commits`; `ModelId`, `GraphEvent`, `GraphDelta`, `EventLifecycle` arrive from `Element/graph`; `ElementGraph`, `Node`, `NodeId`, `Relationship`, `Header`, `ContentAddress` arrive from `Rasm.Element`; the branch `GrantSet` (the object-authorization vocabulary a from-past fork's `BranchRef.Acl` carries) arrives from `Element/authority#GRANT_ALGEBRA`, never the disjoint AppHost effect-gating `Capability`.

## [01]-[INDEX]

- [01]-[TIME_TRAVEL]: cut algebra, the content-addressed checkpoint chain over the Marten snapshot, AS-OF `ElementGraph` reconstruction, member-level range diff, per-node blame, scrub, bisect, and branch-from-past.

## [02]-[TIME_TRAVEL]

- Owner: `TimeCut` the `[ComplexValueObject]` AS-OF boundary carrying a precise `Hlc` ceiling, its `CutKind` modality, and the optional Marten stream version; `AsOfQuery` the one reconstruction-request shape (cut, optional branch, optional node-key prefix); `Checkpoint` a sealed reconstructed-graph fold anchor that hash-chains the prior checkpoint's `Hash` over the reconstructed graph's `ContentAddress`; `KeyDelta`/`RangeDiff` the two-cut member-level delta carrying per-`(NodeId, member)` content-address change and a `ChangeKind` class; `BlameRow`/`BlameContributor` per `(NodeId, change-kind, axis)` authorship attribution carrying the winning event AND its superseded contributor lineage (the forward-log lifecycle granularity, not the property-member granularity `RangeDiff` owns); `ScrubFrame`/`ScrubReel` the ordered event-replay reel over reconstructed graph addresses; `BisectOutcome` the first-flip locus of a MONOTONE history predicate over reconstructed snapshots; `TimeLog` the one read/closure port over the Marten event stream and the AS-OF fold, carrying the injected `ProjectionContext` frame and the AppHost `ReceiptSinkPort`; `TimeTravel` the static surface.
- Cases: `CutKind` is `Precise | Instant | Version` — `TimeCut.Precise(Hlc)`, `Of(Instant)`, and `AtVersion(long, Hlc)` collapsed into one value-object whose `Ceiling` is the inclusive HLC bound and whose `StreamVersion` binds the Marten fold; `ChangeKind` is `Added | Removed | Replaced` keyed by the `(NodeId, member)` content address across two reconstructions (no `Converged` — convergence is the `Version/commits#CRDT_ALGEBRA` `crdt`-lane merge concept, never an AS-OF cell delta between two settled graphs); `SeekDirection` is `Forward | Backward`; `BlameAxis` is `Node | Edge` discriminating whether a member change is keyed on a node's canonical member path or an incident edge.
- Entry: `public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log)` folds the model stream to the cut into the AS-OF `ElementGraph` and returns the receipt; `public static IO<ElementGraph> Graph(AsOfQuery query, TimeLog log)` is the bare reconstructed snapshot; `public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log)` content-address-differences two reconstructed snapshots by `(NodeId, member)`; `public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log)` folds the winning-plus-superseded authorship per changed cell from the event metadata; `public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log)` materializes the ordered event-replay reel; `public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<ElementGraph, bool> holds, TimeLog log)` binary-searches the first version where a MONOTONE predicate flips; `public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, GrantSet acl, Guid origin, TimeLog log, Func<string, Guid, ContentAddress, CommitMessage, IO<CommitNode>> mintBranchCommit)` forks a new branch over the reconstructed graph's content address (the `acl` is the `Element/authority#GRANT_ALGEBRA` `GrantSet` the new `BranchRef` carries, the object-authorization vocabulary the commit-DAG branch lane reuses, never the AppHost effect-gating `Capability`); `public static IO<Checkpoint> Anchor(AsOfQuery query, Option<Checkpoint> prior, TimeLog log)` seals the AS-OF graph against the nearest prior checkpoint; `public static bool Verify(Checkpoint checkpoint, Option<Checkpoint> prior, ContentAddress reconstructed)` re-folds the chain and confirms the rolling address.
- Auto: AS-OF reconstruction is `Marten` `AggregateStreamAsync<GraphProjection>(model, version|timestamp)` folding the `GraphDelta` prefix through the SAME `GraphDelta.ReplayOnto` the inline projection runs (the periodic Marten snapshot is the fold floor Marten seeds the aggregation from, so a deep history folds the suffix from the snapshot rather than genesis), so the historical `ElementGraph` equals the live state field-for-field; a checkpoint seal hash-chains the prior `Hash` into a fresh `XxHash128` then appends the reconstructed graph's order-independent `ContentAddress.OfGraph`, so a checkpoint's `Hash` is the rolling content address of the AS-OF graph above the floor; range diff reconstructs both cuts and projects TWO axes — the NODE axis from the `ElementGraph.EqualityComparer.Default.Inequalities` member change-set (the `Nodes` `[UnorderedEquality]` map's NodeId-keyed paths) into per-`(NodeId, member)` `KeyDelta` rows keyed by the node's `ContentAddress`, plus the EDGE axis from a content-keyed set-difference over the two `[OrderedEquality]` edge arrays (the `Edges[i]` inequality paths carry no NodeId segment, so a topology change would otherwise vanish) attributing each changed edge to both endpoint nodes; blame folds the windowed `GraphEvent` prefix by `(NodeId, change-kind, axis)` (the forward-log touch bucket, since the seam `Node` is not `[Equatable]` for sub-member localization here) and retains the superseded contributors so authorship carries the full lineage the `Version/provenance#CAUSAL_DAG` reconciles against; scrub folds the windowed event prefix as `ScrubFrame` values carrying BOTH the pre-event `Before` and post-event `After` graph content address, the address threading through the fold and advancing ONCE per frame through `Advance` — the seam incremental `ContentAddress.OfGraph(prior, delta)` contract (`Element/codec` Growth row), whose documented interim body is the whole-graph `OfGraph` recompute, so the fold shape is already incremental and the seam landing swaps ONE body, never the folds; bisect binary-searches the version range re-reconstructing each candidate version through the SAME `AggregateStreamAsync(version:)` fold, its flip frame minted through the same `Advance` step.
- Receipt: the AS-OF reconstruction folds a typed `TimeTravelReceipt` and emits it through the AppHost `ReceiptSinkPort` the `TimeLog` carries, under its `store.timetravel.asof` slot (the `Element/codec#Write` / `Version/ledger#Stamp` emission law), returning the same receipt to the caller; a checkpoint seal carries its own evidence in the returned `Checkpoint` and `Verify` returns the pure chain-validity probe.
- Packages: Marten (`AggregateStreamAsync`/`FetchStreamAsync`/`IEvent<GraphEvent>`), Rasm.Element (`ElementGraph`/`GraphDelta.ReplayOnto`/`ContentAddress.Of`/`ContentAddress.OfGraph`/`EqualityComparer.Inequalities`/`Node`/`NodeId`/`Relationship` — the seam owns every content-key mint here), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.IO.Hashing (the `ChainHash` rolling accumulator ONLY — a defensible-local tamper chain, never a content-key mint), BCL inbox.
- Growth: a new replay projection is one method on `TimeTravel`; a new attribution dimension is one field on `BlameRow`; a new change classification is one `ChangeKind` row; a new cut modality is one `CutKind` row plus one `TimeCut` factory; zero new surface — a temporal-table mirror, a second history store, a snapshot-per-instant materialization, a bespoke `OpLogEntry` replay engine, or a parallel bisect walker is the deleted form because reconstruction is `AggregateStreamAsync` over the events Marten already holds and pins the heavy cuts to the periodic Marten snapshot as the fold floor, the lightweight `Checkpoint` carrying only the reproducibility chain.
- Boundary: reconstruction is the SAME `GraphDelta.ReplayOnto` fold the `Element/graph#GRAPH_PROJECTION` inline projection and the live `Graph/delta#GRAPH_DELTA` `WorkingGraph` produce-and-replay run, surfaced through `Marten` `AggregateStreamAsync(version|timestamp)`, so the AS-OF `ElementGraph` at any cut is reproducible from the model stream and there is exactly ONE materializer — a second hand-rolled prefix fold (the retired `OpLogEntry`/`Crdt.Apply` replay) is the deleted form because the op-log is itself a `Version/ledger#CHANGEFEED` projection of these same events and TimeTravel reconstructs the durable graph, not a CRDT cell map; `TimeCut.Ceiling` is the inclusive `Hlc` bound and `StreamVersion` the Marten fold key — `Of(Instant)` binds `AggregateStreamAsync(timestamp:)`, `AtVersion` binds `AggregateStreamAsync(version:)`, and `Precise(Hlc)` resolves the version through `TimeLog.VersionAt` (the HLC→version map off the event `Timestamp`) so a precise causal cut still folds a deterministic version, never a wall-clock window; the `Checkpoint.Address` is the reconstructed graph's `ContentAddress.OfGraph` (the order-independent snapshot identity the `Element/codec#CONTENT_ADDRESS` owner mints, never a re-implemented hash) and `AsOfKey` names it EXPLICITLY as the S3 cross-runtime seam key — the ONE content digest the `python:data/gridded/virtual` icechunk snapshot identity keys AS-OF snapshots by AND `recovery.md` `ReAttest` proves a reconstructed restore against (`RecoveryPoint.AsCut()` resolves to this cut, the reconstructed `OfGraph` must equal the checkpoint `AsOfKey`) — while its `Hash` is the rolling content address chain above the floor, a NON-cryptographic tamper-evidence link NEVER the cross-runtime key (not reproducible from graph content alone; `Version/provenance#ATTESTED_LEDGER` owns authenticity and the attested chain explicitly defers reproducibility here), so `Verify` re-folds the prior `Hash` over the reconstructed `Address` confirming the rolling address, the `Prior` back-link, and the cumulative `Version`; `RangeDiff` reconstructs both endpoints (two AS-OF folds, never a stored delta chain) and projects TWO axes so NO change escapes: the NODE axis is the `Generator.Equals` `Inequalities` member change-set (a property/material/quantity member moved between two cuts surfaces by its `(NodeId, member)` content-address delta, the `Added`/`Removed`/`Replaced` class read from whether the node existed at each cut), and the EDGE axis is a content-keyed set-difference over the two edge arrays (because the `Edges` member is `[OrderedEquality]`, its inequality paths carry an `Index`/`Added`/`Removed` segment that is never NodeId-valued, so an `Inequalities`-only diff would SILENTLY DROP every topology rewire — the deleted thin slice; each changed edge attributes to both endpoint `NodeId`s on the Edge axis, the same both-endpoint attribution blame uses, and the node-presence `Added`/`Removed`/`Changed` accessors filter to the Node axis so an incident edge change never mis-reports an existing node) — DISTINCT in altitude from the `Version/merge#STRUCTURAL_DIFF` base-relative 3-way forest merge (that is the merge conflict surface; this is the AS-OF member delta between two settled snapshots) and from the `Version/ledger#SYNC_TRANSPORTS` `GraphDiff` transport set-difference; `BlameRow` reads the same `(Hlc, actor)` the changefeed stamp carries — the winning `GraphEvent` is the highest-version event whose `GraphDelta` touched the `(NodeId, change-kind, axis)` cell (an edge touch keyed on every node the edge `Members` involves, not a 2-tuple `Endpoints` that has no `Map`), the superseded contributors the prior touching events — so blame is event-stream authorship at forward-log lifecycle granularity, never a re-derived guess, and the property-member narrowing composes with the `RangeDiff` Node axis; bisect's `holds` is MONOTONE so the first-flip locus is a lower-bound binary search over the stream version range, `HeldAtFloor` short-circuiting an already-broken floor, each probe a `AggregateStreamAsync(version:mid)` reconstruction; branch-from-past mints a root `CommitNode` over the reconstructed graph's `ContentAddress` through the `mintBranchCommit` seam, the commit cell riding the event stream's `Hlc` stamp, never `DateTime.UtcNow`; there is no redaction stance — the `Version/retention#RETENTION_CLASSES` lifecycle is append-only with reachability GC over every AS-OF cut (history is never mutated and a blob a historical cut references is never collected), so a "fold erased bytes / mask a redacted op" reconstruction has no owner and is the deleted form.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CutKind {
    public static readonly CutKind Precise = new("precise");
    public static readonly CutKind Instant = new("instant");
    public static readonly CutKind Version = new("version");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChangeKind {
    public static readonly ChangeKind Added = new("added");
    public static readonly ChangeKind Removed = new("removed");
    public static readonly ChangeKind Replaced = new("replaced");
}

[SmartEnum]
public sealed partial class SeekDirection {
    public static readonly SeekDirection Forward = new(rewind: false);
    public static readonly SeekDirection Backward = new(rewind: true);
    public bool Rewind { get; }
    public Seq<ScrubFrame> Lay(Seq<ScrubFrame> forward) => Rewind ? forward.Rev() : forward;
}

[SmartEnum]
public sealed partial class BlameAxis {
    public static readonly BlameAxis Node = new();
    public static readonly BlameAxis Edge = new();
}

// --- [MODELS] --------------------------------------------------------------------------
// The AS-OF boundary: an inclusive Hlc ceiling, its modality, and the optional Marten stream version the fold binds.
// Of(Instant) binds AggregateStreamAsync(timestamp:), AtVersion binds AggregateStreamAsync(version:), Precise(Hlc)
// resolves the version through TimeLog.VersionAt. recovery.md imports TimeCut.Of, so the name and the Of(Instant)
// factory are load-bearing public contract.
[ComplexValueObject]
public sealed partial class TimeCut {
    public Hlc Ceiling { get; }
    public CutKind Source { get; }
    public Option<long> StreamVersion { get; }
    public Instant At => Ceiling.Physical;
    public static TimeCut Precise(Hlc cell) => Create(cell, CutKind.Precise, None);
    public static TimeCut Of(Instant at) => Create(new Hlc(at, ulong.MaxValue), CutKind.Instant, None);
    public static TimeCut AtVersion(long version, Hlc ceiling) => Create(ceiling, CutKind.Version, Some(version));
    public bool Admits(Hlc cell) => cell.CompareTo(Ceiling) <= 0;
}

// The reconstruction request: the cut, the optional branch scope, and an optional rooted-node-key prefix the diff/blame
// folds narrow on. No redaction stance — the append-only retention model (Version/retention) never masks history.
public readonly record struct AsOfQuery(TimeCut Cut, Option<string> Branch, Option<NodeId> NodeKeyPrefix) {
    public static AsOfQuery At(Instant cut) => new(TimeCut.Of(cut), None, None);
    public static AsOfQuery AtVersion(long version, Hlc ceiling) => new(TimeCut.AtVersion(version, ceiling), None, None);
    public bool Selects(NodeId key) => NodeKeyPrefix.Map(p => key.Value.StartsWith(p.Value, StringComparison.Ordinal)).IfNone(true);
}

// The content-addressed reproducibility checkpoint: the reconstructed ElementGraph's order-independent ContentAddress
// (Element/codec, the ONE snapshot hasher) sealed against the prior checkpoint's rolling Hash. NON-cryptographic — it
// proves the checkpoint reproduces from the stream; Version/provenance#ATTESTED_LEDGER owns authenticity and defers
// reproducibility here. recovery.md (snapshot-archive) seals it to cold storage and retention.md governs it as the
// `snapshot` class, so At/Hash/Prior are load-bearing public contract.
public readonly record struct Checkpoint(Hlc At, long Version, ContentAddress Address, UInt128 Hash, Option<UInt128> Prior) {
    // The S3 cross-runtime seam key: the ONE content digest (seam ContentAddress.OfGraph at the cut) the python
    // icechunk snapshot identity keys by AND recovery ReAttest proves against; Hash stays tamper evidence only.
    public ContentAddress AsOfKey => Address;
}

// The per-(NodeId, member) AS-OF delta between two reconstructions: the canonical member path, its axis (Node for a
// `Nodes[id]` member, Edge for an incident topology change attributed to an endpoint), the class, and the from/to
// content address (the node's id-inclusive address on the Node axis, the edge's own content key on the Edge axis).
public readonly record struct KeyDelta(NodeId Node, string Member, BlameAxis Axis, ChangeKind Kind, Option<UInt128> From, Option<UInt128> To);

// The node-presence accessors filter to the NODE axis so an incident edge change never mis-reports an existing node as
// added/removed; `EdgesChanged` is the distinct set of nodes whose incident topology shifted (the Edge-axis deltas).
public readonly record struct RangeDiff(TimeCut From, TimeCut To, Seq<KeyDelta> Deltas) {
    public Seq<NodeId> Added => Deltas.Filter(static d => d.Axis == BlameAxis.Node && d.Kind == ChangeKind.Added).Map(static d => d.Node).Distinct().ToSeq();
    public Seq<NodeId> Removed => Deltas.Filter(static d => d.Axis == BlameAxis.Node && d.Kind == ChangeKind.Removed).Map(static d => d.Node).Distinct().ToSeq();
    public Seq<NodeId> Changed => Deltas.Filter(static d => d.Axis == BlameAxis.Node && d.Kind == ChangeKind.Replaced).Map(static d => d.Node).Distinct().ToSeq();
    public Seq<NodeId> EdgesChanged => Deltas.Filter(static d => d.Axis == BlameAxis.Edge).Map(static d => d.Node).Distinct().ToSeq();
}

public readonly record struct BlameContributor(string Actor, Guid Origin, Hlc Cell, long Version);
public readonly record struct BlameRow(NodeId Node, string Member, BlameAxis Axis, string Actor, Guid Origin, Hlc Cell, long Version, int Contributors, Seq<BlameContributor> Superseded);

// One scrub frame is one Marten GraphEvent replayed: the index, the event, its Hlc, the pre/post reconstructed graph
// content address. The reel is the ordered (direction-laid) sequence plus the terminal reconstructed graph.
public readonly record struct ScrubFrame(long Index, long Version, EventLifecycle Lifecycle, Hlc At, string Actor, UInt128 Before, UInt128 After);

public readonly record struct ScrubWindow(Interval Span, SeekDirection Direction) {
    public static ScrubWindow Forward(Interval span) => new(span, SeekDirection.Forward);
    public bool Includes(Instant at) => (!Span.HasStart || Span.Start <= at) && (!Span.HasEnd || at <= Span.End);
}

public readonly record struct ScrubReel(Seq<ScrubFrame> Frames, ElementGraph Terminal, Interval Span) {
    public Option<ScrubFrame> Seek(Hlc at) =>
        Frames.Filter(f => f.At.CompareTo(at) >= 0).Fold(Option<ScrubFrame>.None, static (best, f) => Some(best.Filter(b => b.At.CompareTo(f.At) <= 0).IfNone(f)));
    public Option<UInt128> StateAt(Hlc at, SeekDirection direction) => Seek(at).Map(f => direction.Rewind ? f.Before : f.After);
}

public readonly record struct BisectOutcome(Option<ScrubFrame> FirstFlip, long Probes, bool HeldAtFloor);
public readonly record struct TimeTravelReceipt(string Slot, TimeCut Cut, long Version, long EventsFolded, bool SnapshotHit, UInt128 Address, Duration Elapsed, Instant At, Guid Correlation);

// The one read port over the Marten event stream and the AS-OF fold. Reconstruct wraps AggregateStreamAsync (the
// periodic Marten snapshot is the fold floor Marten seeds from internally — SnapshotHit reads whether the head state
// carried one) and OWNS branch scoping: it receives the whole AsOfQuery, so an AsOfQuery.Branch=Some restricts the
// fold to the branch head's commit closure off the commit-DAG (the composition root wires the BranchRef-head and
// reachable-commit resolution into this one delegate), an AsOfQuery.Branch=None folds the global model stream;
// ReconstructAt is the by-version probe scrub/bisect drive; Events wraps FetchStreamAsync; VersionAt resolves a
// precise Hlc cut to a stream version. The injected ProjectionContext frame carries the clock marks and the
// correlation Guid ([A.1] — a ClockPolicy/CorrelationId field is the deleted strata inversion); the AppHost
// ReceiptSinkPort emits the folded TimeTravelReceipt under its own Slot (the codec Write / ledger Stamp
// emission law — a Slot-carrying receipt returned bare with no sink is the deleted split-brain). NO redaction
// delegate, and NO standalone branch-head/closure port field — the branch scope rides AsOfQuery.Branch into
// Reconstruct rather than a second unwired delegate beside it.
public sealed record TimeLog(
    Func<AsOfQuery, IO<(ElementGraph Graph, long Version, bool SnapshotHit)>> Reconstruct,
    Func<long, IO<ElementGraph>> ReconstructAt,
    Func<TimeCut, IO<Seq<IEvent<GraphEvent>>>> Events,
    Func<Hlc, IO<long>> VersionAt,
    ModelId Model, ProjectionContext Frame, ReceiptSinkPort Sink);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TimeTravel {
    public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log) => Folded(query, log).Map(static fold => fold.Receipt);

    public static IO<ElementGraph> Graph(AsOfQuery query, TimeLog log) => log.Reconstruct(query).Map(static r => r.Graph);

    public static IO<Checkpoint> Anchor(AsOfQuery query, Option<Checkpoint> prior, TimeLog log) =>
        log.Reconstruct(query with { Branch = None }).Map(reconstructed => Seal(prior, query.Cut.Ceiling, reconstructed.Version, ContentAddress.OfGraph(reconstructed.Graph)));

    public static Checkpoint Seal(Option<Checkpoint> prior, Hlc at, long version, ContentAddress address) =>
        new(at, version, address, ChainHash(prior.Map(static p => p.Hash), address), prior.Map(static p => p.Hash));

    public static bool Verify(Checkpoint checkpoint, Option<Checkpoint> prior, ContentAddress reconstructed) =>
        checkpoint.Address == reconstructed
        && checkpoint.Hash == ChainHash(prior.Map(static p => p.Hash), reconstructed)
        && checkpoint.Prior == prior.Map(static p => p.Hash)
        && checkpoint.Version >= prior.Map(static p => p.Version).IfNone(0L);

    static UInt128 ChainHash(Option<UInt128> prior, ContentAddress address) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        if (prior.IsSome) { BinaryPrimitives.WriteUInt128LittleEndian(word, prior.ValueUnsafe()); rolling.Append(word); }
        BinaryPrimitives.WriteUInt128LittleEndian(word, address.Value);
        rolling.Append(word);
        return rolling.GetCurrentHashAsUInt128();
    }

    public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log) =>
        from a in log.Reconstruct(from)
        from b in log.Reconstruct(to)
        select new RangeDiff(from.Cut, to.Cut, Deltas(from, a.Graph, b.Graph));

    // The AS-OF member delta: the Generator.Equals Inequalities member change-set over the [Equatable] ElementGraph
    // (the SAME authoritative member diff Version/merge#STRUCTURAL_DIFF gates conflicts on). TWO axes, because the seam
    // graph is `[UnorderedEquality]` Nodes + `[OrderedEquality]` Edges: the NODE axis projects each `Nodes[<NodeId>]`
    // member inequality (whose dictionary-key segment IS NodeId-valued) per (NodeId, member); the EDGE axis is a SEPARATE
    // content-keyed set-difference over the two edge arrays (an `Edges[i]` inequality carries an `Index`/`Added`/`Removed`
    // segment that is NEVER NodeId-valued, so an Inequalities-only diff would SILENTLY DROP every topology change — the
    // deleted thin slice) attributing each changed edge to BOTH its endpoint nodes by canonical-byte presence. Node
    // presence classifies the node axis; edge canonical-byte presence classifies the edge axis. The from/to content
    // address is the node's id-INCLUSIVE ContentAddress at each cut (the ONE seam hasher, never a re-digest). Honors the
    // optional NodeKeyPrefix so a scoped diff over one rooted subtree never folds the whole graph.
    static Seq<KeyDelta> Deltas(AsOfQuery query, ElementGraph a, ElementGraph b) =>
        toSeq(ElementGraph.EqualityComparer.Default.Inequalities(a, b)
            .Choose(ineq => CellOf(ineq.Path).Filter(cell => query.Selects(cell.Node))
                .Map(cell => new KeyDelta(cell.Node, cell.Member, BlameAxis.Node,
                    (a.Find(cell.Node).IsSome, b.Find(cell.Node).IsSome) switch {
                        (false, true) => ChangeKind.Added,
                        (true, false) => ChangeKind.Removed,
                        _ => ChangeKind.Replaced,
                    },
                    a.Find(cell.Node).Map(n => ContentAddress.Of(n, a.Header.Tolerance).Value),
                    b.Find(cell.Node).Map(n => ContentAddress.Of(n, b.Header.Tolerance).Value))))
            .GroupBy(static d => (d.Node, d.Member))
            .Select(static g => g.First()))
            + EdgeDeltas(query, a, b);

    // The EDGE axis of the AS-OF diff — the topology delta the node-keyed Inequalities cannot recover. Each edge is keyed
    // through the SEAM `ContentAddress.Of` over its standalone `ToCanonicalBytes(tolerance)` bytes under its OWN graph's
    // `Header.Tolerance` (the ONE seam hasher — a raw `XxHash128` call site is the deleted spelling; the SAME edge content
    // key Version/merge#STRUCTURAL_DIFF composes, the SAME grid the node-side
    // `ContentAddress.Of(n, a.Header.Tolerance)` keying above uses), so an edge in `b` not in `a` is Added, in `a` not `b`
    // Removed (a rewired endpoint is one Removed + one Added — the content key changed). Threading the model tolerance
    // keeps a `Generic` edge's `PropertyValue.Measure` attributes quantized on the same grid (the five typed edge cases
    // carry no Measure and are tolerance-insensitive), so a sub-tolerance measure jitter never reads as a phantom edge
    // Added+Removed. Each changed edge attributes to BOTH endpoint NodeIds (the SAME both-endpoint attribution Blame uses),
    // the from/to address the edge's own content key, honoring NodeKeyPrefix.
    static Seq<KeyDelta> EdgeDeltas(AsOfQuery query, ElementGraph a, ElementGraph b) {
        HashMap<UInt128, Relationship> fromEdges = toHashMap(a.Edges.Select(e => (ContentAddress.Of(e.ToCanonicalBytes(a.Header.Tolerance).Span).Value, e)));
        HashMap<UInt128, Relationship> toEdges = toHashMap(b.Edges.Select(e => (ContentAddress.Of(e.ToCanonicalBytes(b.Header.Tolerance).Span).Value, e)));
        Seq<(Relationship Edge, ChangeKind Kind, Option<UInt128> Key)> changed =
            toSeq(toEdges.Filter((key, _) => !fromEdges.ContainsKey(key)).Map(static (key, e) => (e, ChangeKind.Added, Some(key))).Values)
            + toSeq(fromEdges.Filter((key, _) => !toEdges.ContainsKey(key)).Map(static (key, e) => (e, ChangeKind.Removed, Some(key))).Values);
        return changed.Bind(row => row.Edge.Members.Filter(query.Selects)
            .Map(node => new KeyDelta(node, nameof(ElementGraph.Edges), BlameAxis.Edge, row.Kind,
                row.Kind == ChangeKind.Removed ? row.Key : None, row.Kind == ChangeKind.Added ? row.Key : None)));
    }

    public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log) =>
        log.Events(query.Cut).Map(events => toSeq(events
            .Bind(e => Touched(e).Filter(cell => query.Selects(cell.Node)).Map(cell => (Cell: cell, Event: e)))
            .GroupBy(static row => (row.Cell.Node, row.Cell.Member, row.Cell.Axis))
            .Select(static group => {
                // A GroupBy group is non-empty, so `ordered[0]` reads the highest-version winner directly — LanguageExt v5
                // Seq.Head is Option<A>, so ordered[0] (not .Head) reads the bare row.
                var ordered = toSeq(group.OrderByDescending(static r => r.Event.Version));
                var winner = ordered[0];
                return new BlameRow(group.Key.Node, group.Key.Member, group.Key.Axis,
                    ActorOf(winner.Event), OriginOf(winner.Event), new Hlc(Instant.FromDateTimeOffset(winner.Event.Timestamp), (ulong)winner.Event.Version), winner.Event.Version,
                    ordered.Count, ordered.Tail.Map(static r => new BlameContributor(ActorOf(r.Event), OriginOf(r.Event), new Hlc(Instant.FromDateTimeOffset(r.Event.Timestamp), (ulong)r.Event.Version), r.Event.Version)));
            })));

    public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log) =>
        from events in log.Events(query.Cut)
        let windowed = toSeq(events.Filter(e => window.Includes(Instant.FromDateTimeOffset(e.Timestamp))).OrderBy(static e => e.Version))
        from seeded in log.ReconstructAt(windowed.Head.Map(static e => e.Version - 1L).IfNone(0L))
        // Each frame replays one event's GraphDelta through the SAME raw ReplayOnto the inline projection folds (never
        // the re-validating Apply — a stream-resident delta is total by construction, re-validation here is the deleted
        // form). The address THREADS through the fold: one seed OfGraph, then ONE Advance per frame — the prior frame's
        // After IS the next frame's Before, so the per-frame double whole-graph re-hash is the deleted form.
        let reel = windowed.Fold((Frames: Seq<ScrubFrame>(), Graph: seeded, Address: ContentAddress.OfGraph(seeded)), static (acc, e) =>
            e.Data.Body.ReplayOnto(acc.Graph) is var next && Advance(acc.Address, e.Data.Body, next) is var sealedNext
                ? (acc.Frames.Add(new ScrubFrame(acc.Frames.Count, e.Version, e.Data.Lifecycle, new Hlc(Instant.FromDateTimeOffset(e.Timestamp), (ulong)e.Version), ActorOf(e), acc.Address.Value, sealedNext.Value)), next, sealedNext)
                : acc)
        select new ScrubReel(window.Direction.Lay(reel.Frames), reel.Graph, window.Span);

    // The incremental address advance — the seam `ContentAddress.OfGraph(prior, delta)` contract (`Element/codec`
    // Growth row [04]): one delta-composed address per replay frame. Until the seam member lands, the documented
    // interim recomputes the whole-graph OfGraph(next); the Scrub/Bisect folds are already incremental in shape,
    // so the landing swaps THIS one body and no fold changes.
    static ContentAddress Advance(ContentAddress prior, GraphDelta delta, ElementGraph next) => ContentAddress.OfGraph(next);

    public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<ElementGraph, bool> holds, TimeLog log) =>
        from reconstructed in log.Reconstruct(bound with { Branch = None })
        from events in log.Events(bound.Cut)
        let versions = toSeq(events.Map(static e => e.Version).OrderBy(static v => v))
        from floorGraph in log.ReconstructAt(versions.Head.Map(static v => v - 1L).IfNone(0L))
        from outcome in Descend(holds, floorGraph, versions, events.ToHashMap(static e => e.Version), log)
        select outcome;

    static IO<BisectOutcome> Descend(Func<ElementGraph, bool> holds, ElementGraph floor, Seq<long> versions, HashMap<long, IEvent<GraphEvent>> byVersion, TimeLog log) {
        IO<(int Flip, long Probes)> Search(int lo, int hi, long probes) =>
            lo >= hi
                ? IO.pure((lo, probes))
                : (lo + ((hi - lo) >> 1)) is var mid && versions[mid] is var v
                    ? log.ReconstructAt(v).Bind(g => holds(g) ? Search(lo, mid, probes + 1) : Search(mid + 1, hi, probes + 1))
                    : IO.pure((lo, probes));
        return holds(floor)
            ? IO.pure(new BisectOutcome(None, 0L, HeldAtFloor: true))
            : Search(0, versions.Count, 0L).Bind(found => found.Flip < versions.Count && byVersion.Find(versions[found.Flip]) is { IsSome: true, Case: IEvent<GraphEvent> flipEvent }
                ? log.ReconstructAt(versions[found.Flip] - 1L).Bind(before => log.ReconstructAt(versions[found.Flip]).Map(after =>
                    new BisectOutcome(Some(FlipFrame(found.Flip, flipEvent, before, after)), found.Probes, HeldAtFloor: false)))
                : IO.pure(new BisectOutcome(None, found.Probes, HeldAtFloor: false)));
    }

    // The flip frame mints through the SAME Advance step the scrub fold threads: one seed OfGraph on the
    // pre-flip graph, the After address delta-composed off it.
    static ScrubFrame FlipFrame(int index, IEvent<GraphEvent> e, ElementGraph before, ElementGraph after) {
        ContentAddress prior = ContentAddress.OfGraph(before);
        return new ScrubFrame(index, e.Version, e.Data.Lifecycle, new Hlc(Instant.FromDateTimeOffset(e.Timestamp), (ulong)e.Version), ActorOf(e), prior.Value, Advance(prior, e.Data.Body, after).Value);
    }

    public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, GrantSet acl, Guid origin, TimeLog log, Func<string, Guid, ContentAddress, CommitMessage, IO<CommitNode>> mintBranchCommit) =>
        from reconstructed in log.Reconstruct(query)
        let address = ContentAddress.OfGraph(reconstructed.Graph)
        from commit in mintBranchCommit(newBranch, origin, address, new CommitMessage("branch-from-past", string.Empty))
        select new BranchRef(newBranch, RefKind.Branch, commit.ContentKey, acl, origin, commit.Cell.Physical, None, None, CommitMessage.Empty, string.Empty);

    // The receipt is source-gen-registered on `ElementJson` (the codec seal-evidence precedent) so the sink
    // envelope payload crosses the strict resolver TYPED; the emission mirrors `Element/codec#Write` exactly.
    static IO<(ElementGraph Graph, TimeTravelReceipt Receipt)> Folded(AsOfQuery query, TimeLog log) =>
        from mark in IO.lift(log.Frame.Mark)
        from reconstructed in log.Reconstruct(query)
        from events in log.Events(query.Cut)
        let address = ContentAddress.OfGraph(reconstructed.Graph)
        let receipt = new TimeTravelReceipt("store.timetravel.asof", query.Cut, reconstructed.Version, events.Count, reconstructed.SnapshotHit, address.Value, log.Frame.Elapsed(mark), log.Frame.Now(), log.Frame.Correlation)
        from _ in log.Sink.Send(log.Frame.Correlation, log.Frame.Tenant, "Rasm.Persistence", receipt.Slot, JsonSerializer.SerializeToElement(receipt, ElementJson.Options))
        select (reconstructed.Graph, receipt);

    // The (NodeId, member) cell a Generator.Equals NODE-axis MemberPath addresses: the `Nodes` `[UnorderedEquality]` map
    // emits a NodeId-valued `Key(<NodeId>)` segment, so the first NodeId-valued segment keys the owning node (the SAME
    // `seg.Value is NodeId` pattern Version/merge#STRUCTURAL_DIFF `OwningNode` reads — NodeId is a readonly struct so the
    // cast is a pattern match, never `as`; MemberPathSegment carries a `Kind` + `Value`, never a `.Name`), and
    // MemberPath.ToString() is the dotted/bracketed canonical member path. An `Edges[i]` path carries NO NodeId-valued
    // segment (the `[OrderedEquality]` array emits `Index`/`Added`/`Removed`), so it yields None HERE and the EDGE axis is
    // owned wholly by `EdgeDeltas` — a `seg.Value is nameof(Edges)` axis flag here would be dead (no node key to pair it with).
    static Option<(NodeId Node, string Member)> CellOf(MemberPath path) =>
        toSeq(path.Segments).Choose(static seg => seg.Value is NodeId key ? Some(key) : None).Head
            .Map(node => (node, path.ToString()));

    // Every cell a GraphEvent's GraphDelta touched, keyed by `(NodeId, change-kind, axis)`. The `member` is the forward-log
    // CHANGE-KIND bucket (the `GraphDelta` array a touch landed in — added/revised/removed node, added/removed edge), NOT a
    // property-member path: the seam `Node` union is `[Union]` but not `[Equatable]`, so a `RevisedNodes` `(Before, After)`
    // pair cannot be member-localized HERE (no `Node.EqualityComparer.Inequalities`) — property-member granularity is the
    // `RangeDiff` Node axis's job (the `[Equatable]` `ElementGraph.Inequalities` over the two reconstructed snapshots). So
    // blame answers "which event last touched this node, and how" at lifecycle granularity; pairing it with `RangeDiff`
    // narrows to the member. A node touch carries the Node axis; an edge touch the Edge axis keyed on BOTH endpoints
    // (`Members`, every node the edge involves) so a relationship edit attributes to every node it joins.
    static Seq<(NodeId Node, string Member, BlameAxis Axis)> Touched(IEvent<GraphEvent> e) {
        var delta = e.Data.Body;
        return delta.AddedNodes.Map(static n => (n.Id, nameof(GraphDelta.AddedNodes), BlameAxis.Node))
            + delta.RevisedNodes.Map(static n => (n.After.Id, nameof(GraphDelta.RevisedNodes), BlameAxis.Node))
            + delta.RemovedNodes.Map(static id => (id, nameof(GraphDelta.RemovedNodes), BlameAxis.Node))
            + delta.AddedEdges.Bind(static r => r.Members.Map(ep => (ep, nameof(GraphDelta.AddedEdges), BlameAxis.Edge)))
            + delta.RemovedEdges.Bind(static r => r.Members.Map(ep => (ep, nameof(GraphDelta.RemovedEdges), BlameAxis.Edge)));
    }

    // Authorship rides the Marten event Headers (the Element/graph#STREAM_GRAIN Append stamps `actor`/`origin` via
    // SetHeader, the SAME header slot Version/ledger#CHANGEFEED OpLog.Project reads) — never re-derived. An origin
    // header absent (a pre-origin event) defaults to Guid.Empty, matching the changefeed projection's own default.
    static string ActorOf(IEvent<GraphEvent> e) => e.Headers is { } h && h.TryGetValue("actor", out var actor) ? actor?.ToString() ?? string.Empty : string.Empty;

    static Guid OriginOf(IEvent<GraphEvent> e) => e.Headers is { } h && h.TryGetValue("origin", out var origin) && Guid.TryParse(origin?.ToString(), out var id) ? id : Guid.Empty;
}
```

| [INDEX] | [POLICY]          | [VALUE]                                       | [BINDING]                                                          |
| :-----: | :---------------- | :-------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | one materializer  | `GraphDelta.ReplayOnto`                      | reconstruction equals the live `ElementGraph` field-for-field    |
|  [02]   | as-of fold        | `AggregateStreamAsync(version\|timestamp)`    | Marten folds the prefix from the periodic snapshot floor         |
|  [03]   | as-of cut         | `TimeCut.Ceiling` precise/instant/version     | the Marten fold binds the version or the ceiling instant         |
|  [04]   | checkpoint chain  | `ContentAddress.OfGraph` + rolling hash       | `Anchor` seals, `Verify` re-folds `Prior`+`Version`; reproducibility, not authenticity |
|  [05]   | as-of key (S3)    | `Checkpoint.AsOfKey` = `Address`              | the ONE cross-runtime digest: icechunk snapshot identity + recovery `ReAttest` oracle; `Hash` never the key |
|  [06]   | range diff        | node axis `Inequalities` + edge axis seam `ContentAddress.Of` set-diff | both axes; a topology rewire never dropped; distinct from the 3-way merge |
|  [07]   | bisect descent    | `O(log n)` probes of a MONOTONE predicate     | each probe re-reconstructs through `AggregateStreamAsync(version:)`|
|  [08]   | frame address     | ONE `Advance` per replay frame                | the seam `OfGraph(prior, delta)` contract; whole-graph recompute is the documented interim |
|  [09]   | blame             | touching `GraphEvent` by version, `(NodeId, change-kind, axis)` cell | forward-log lifecycle granularity; member narrowing composes with `RangeDiff` |
