# [PERSISTENCE_VERSION_TIMETRAVEL]

`TimeTravel` reconstructs every historical graph through Marten's `AggregateStreamAsync` fold over the same `GraphDelta.ReplayOnto` algebra as the live projection. `TimeCut` unifies causal, instant, and stream-version bounds. `RangeDiff`, `Blame`, `Scrub`, and `Bisect` derive evidence from reconstructed graphs and stored event metadata. `Checkpoint` binds the graph's `ContentAddress.OfGraph` identity to a rolling chain, while `BranchFromPast` mints a new commit over the reconstructed address.

## [01]-[INDEX]

- [02]-[TIME_TRAVEL]: cut algebra, the content-addressed checkpoint chain over the Marten snapshot, AS-OF `ElementGraph` reconstruction, member-level range diff, per-node blame, scrub, bisect, and branch-from-past.

## [02]-[TIME_TRAVEL]

- Owner: `TimeCut` the `[ComplexValueObject]` AS-OF boundary carrying a precise `Hlc` ceiling, its `CutKind` modality, and the optional Marten stream version; `AsOfQuery` the one reconstruction-request shape (cut, optional branch, optional node-key prefix); `Checkpoint` a sealed reconstructed-graph fold anchor that hash-chains the prior checkpoint's `Hash` over the reconstructed graph's `ContentAddress`; `KeyDelta`/`RangeDiff` the two-cut member-level delta carrying per-`(NodeId, member)` content-address change and a `ChangeKind` class; `BlameRow`/`BlameContributor` per `(NodeId, change-kind, axis)` authorship attribution carrying the winning event AND its superseded contributor lineage (the forward-log lifecycle granularity, not the property-member granularity `RangeDiff` owns); `ScrubFrame`/`ScrubReel` the ordered event-replay reel over reconstructed graph addresses; `BisectVerdict`/`BisectOutcome` the closed three-case answer of a MONOTONE history predicate over reconstructed snapshots beside the probe cost; `SealLink` the four checkpoint-chain links a `Verify` accumulates as the held `CapabilitySet`; `TimeLog` the one read/closure port over the Marten event stream and the AS-OF fold, carrying the injected `ProjectionContext` frame and the kernel `ReceiptSinkPort`; `TimeTravel` the static surface.
- Cases: `CutKind` is `Precise | Instant | Version` — `TimeCut.Precise(Hlc)`, `Of(Instant)`, and `AtVersion(long, Hlc)` collapsed into one value-object whose `Ceiling` is the inclusive HLC bound and whose `StreamVersion` binds the Marten fold; `ChangeKind` is `Added | Removed | Replaced` keyed by the `(NodeId, member)` content address across two reconstructions (no `Converged` — convergence is the `Version/commits#CRDT_ALGEBRA` `crdt`-lane merge concept, never an AS-OF cell delta between two settled graphs); `SeekDirection` is `Forward | Backward`; `BlameAxis` is `Node | Edge` discriminating whether a member change is keyed on a node's canonical member path or an incident edge.
- Entry: `public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log)` folds the model stream to the cut into the AS-OF `ElementGraph` and returns the receipt; `public static IO<ElementGraph> Graph(AsOfQuery query, TimeLog log)` is the bare reconstructed snapshot; `public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log)` content-address-differences two reconstructed snapshots by `(NodeId, member)`; `public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log)` folds the winning-plus-superseded authorship per changed cell from the event metadata; `public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log)` materializes the ordered event-replay reel; `public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<ElementGraph, bool> holds, TimeLog log)` binary-searches the first version where a MONOTONE predicate flips; `public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, GrantSet acl, Guid origin, TimeLog log, Func<string, Guid, ContentAddress, CommitMessage, IO<CommitNode>> mintBranchCommit)` forks a new branch over the reconstructed graph's content address (the `acl` is the `Element/authority#GRANT_ALGEBRA` `GrantSet` the new `BranchRef` carries, the object-authorization vocabulary the commit-DAG branch lane reuses, never the AppHost effect-gating `Capability`); `public static IO<Checkpoint> Anchor(AsOfQuery query, Option<Checkpoint> prior, TimeLog log)` seals the AS-OF graph against the nearest prior checkpoint; `public static CapabilitySet<SealLink> Verify(Checkpoint checkpoint, Option<Checkpoint> prior, ContentAddress reconstructed)` re-folds the chain and returns the links that held, a consumer gating on the whole seal demanding it through `Require` under that consumer's own band.
- Auto: `AggregateStreamAsync<GraphProjection>(model, version|timestamp)` folds the `GraphDelta` prefix through `GraphDelta.ReplayOnto`, seeded from Marten's nearest snapshot. `Seal` chains the reconstructed graph's order-independent `ContentAddress.OfGraph` value onto the prior rolling hash. Range diff projects node-member inequalities and a separate content-keyed edge set difference, so topology changes remain visible. Blame groups stored event touches by `(NodeId, change-kind, axis)` and retains superseded contributors. Scrub replays each event once and steps one seam `GraphMembers` accumulator per event, addressing each resulting graph through the seam's own sorted fold. Bisect reconstructs only logarithmic candidate versions for a monotone predicate.
- Receipt: the AS-OF reconstruction folds a typed `TimeTravelReceipt` and emits it through the kernel `ReceiptSinkPort` the `TimeLog` carries, under its `store.timetravel.asof` slot (the `Element/codec#SNAPSHOT_SPINE` / `Version/ledger#CHANGEFEED` emission law), returning the same receipt to the caller; a checkpoint seal carries its own evidence in the returned `Checkpoint`, and `Verify` returns the pure `CapabilitySet<SealLink>` of links that held, naming every break at once rather than the first.
- Packages: Marten (`AggregateStreamAsync`/`FetchStreamAsync`/`IEvent<GraphEvent>`), Rasm (`Rasm/Domain/validation#CAPABILITY` `ICapability`/`CapabilitySet.Of`/`With`/`Missing`/`Require` — the `SealLink` accumulation and its consumer-side demand; `Rasm.Domain` `ContentHash.Of<TState>` + `CanonicalWriter.Optional`/`U128` — the `ChainHash` link digest, a defensible-local tamper chain minted on the one alphabet and never a second hasher), Rasm.Element (`ElementGraph`/`GraphDelta.ReplayOnto`/`ContentAddress.Of`/`ContentAddress.OfGraph`/`GraphMembers.Of`/`Advance`/`GraphMemberStep.Resolve`/`EqualityComparer.Inequalities`/`Node`/`NodeId`/`Relationship` — the seam owns every content-key mint here), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new replay projection is one method on `TimeTravel`; a new attribution dimension is one field on `BlameRow`; a new change classification is one `ChangeKind` row; a new cut modality is one `CutKind` row and one `TimeCut` factory; zero new surface — a temporal-table mirror, a second history store, a snapshot-per-instant materialization, a bespoke `OpLogEntry` replay engine, or a parallel bisect walker is the deleted form because reconstruction is `AggregateStreamAsync` over the events Marten already holds and pins the heavy cuts to the periodic Marten snapshot as the fold floor, the lightweight `Checkpoint` carrying only the reproducibility chain.
- Boundary: reconstruction is the SAME `GraphDelta.ReplayOnto` fold the `Element/graph#GRAPH_PROJECTION` inline projection and the live `Graph/delta#GRAPH_DELTA` `WorkingGraph` produce-and-replay run, surfaced through `Marten` `AggregateStreamAsync(version|timestamp)`, so the AS-OF `ElementGraph` at any cut is reproducible from the model stream and there is exactly ONE materializer — a second hand-rolled prefix fold (the retired `OpLogEntry`/`Crdt.Apply` replay) is the deleted form because the op-log is itself a `Version/ledger#CHANGEFEED` projection of these same events and TimeTravel reconstructs the durable graph, not a CRDT cell map; `TimeCut.Ceiling` is the inclusive `Hlc` bound and `StreamVersion` the Marten fold key — `Of(Instant)` binds `AggregateStreamAsync(timestamp:)`, `AtVersion` binds `AggregateStreamAsync(version:)`, and `Precise(Hlc)` resolves the version through `TimeLog.VersionAt` (the HLC→version map off the event `Timestamp`) so a precise causal cut still folds a deterministic version, never a wall-clock window; the `Checkpoint.Address` is the reconstructed graph's `ContentAddress.OfGraph` (the order-independent snapshot identity the `Element/codec#CONTENT_ADDRESS` owner mints, never a re-implemented hash) and `AsOfKey` names it EXPLICITLY as the S3 cross-runtime seam key — the ONE content digest the `python:data/gridded/virtual` icechunk snapshot identity keys AS-OF snapshots by AND `recovery.md` `ReAttest` proves a reconstructed restore against (`RecoveryPoint.AsCut()` resolves to this cut, the reconstructed `OfGraph` must equal the checkpoint `AsOfKey`) — while its `Hash` is the rolling content address chain above the floor, a NON-cryptographic tamper-evidence link NEVER the cross-runtime key (not reproducible from graph content alone; `Version/provenance#ATTESTED_LEDGER` owns authenticity and the attested chain explicitly defers reproducibility here), so `Verify` re-folds the prior `Hash` over the reconstructed `Address` and ACCUMULATES four independent links — the rolling address, the chain hash, the `Prior` back-link, and the cumulative `Version` — as the `CapabilitySet<SealLink>` that held, because a tamper report naming only the first break sends an operator back to the probe once per remaining link; `RangeDiff` reconstructs both endpoints (two AS-OF folds, never a stored delta chain) and projects TWO axes so NO change escapes: the NODE axis is the `Generator.Equals` `Inequalities` member change-set (a property/material/quantity member moved between two cuts surfaces by its `(NodeId, member)` content-address delta, the `Added`/`Removed`/`Replaced` class read from whether the node existed at each cut), and the EDGE axis is a content-keyed set-difference over the two edge arrays (because the `Edges` member is `[OrderedEquality]`, its inequality paths carry an `Index`/`Added`/`Removed` segment that is never NodeId-valued, so an `Inequalities`-only diff SILENTLY DROPS every topology rewire — the deleted thin slice; each changed edge attributes to both endpoint `NodeId`s on the Edge axis, the same both-endpoint attribution blame uses, and the node-presence `Added`/`Removed`/`Changed` accessors filter to the Node axis so an incident edge change never mis-reports an existing node) — DISTINCT in altitude from the `Version/merge#STRUCTURAL_DIFF` base-relative 3-way forest merge (that is the merge conflict surface; this is the AS-OF member delta between two settled snapshots) and from the `Version/ledger#SYNC_TRANSPORTS` `OpLog.TransferSet` transport set-difference; `BlameRow` reads the same `(Hlc, actor)` the changefeed stamp carries — the winning `GraphEvent` is the highest-version event whose `GraphDelta` touched the `(NodeId, change-kind, axis)` cell (an edge touch keyed on every node the edge `Members` involves, not a 2-tuple `Endpoints` that has no `Map`), the superseded contributors the prior touching events — so blame is event-stream authorship at forward-log lifecycle granularity, never a re-derived guess, and the property-member narrowing composes with the `RangeDiff` Node axis; bisect's `holds` is MONOTONE so the first-flip locus is a lower-bound binary search over the stream version range, the `BisectVerdict.HeldAtFloor` case short-circuiting an already-broken floor and staying DISTINCT from `NeverFlipped` (a predicate broken before the window and a predicate never broken inside it are two findings an operator acts on differently, which one `Option` beside one flag cannot tell apart), each probe a `AggregateStreamAsync(version:mid)` reconstruction and the bound itself never folded because nothing reads it; branch-from-past mints a root `CommitNode` over the reconstructed graph's `ContentAddress` through the `mintBranchCommit` seam, the commit cell riding the event stream's `Hlc` stamp, never `DateTime.UtcNow`; there is no redaction stance — the `Version/retention#RETENTION_CLASSES` lifecycle is append-only with reachability GC over every AS-OF cut (history is never mutated and a blob a historical cut references is never collected), so a "fold erased bytes / mask a redacted op" reconstruction has no owner and is the deleted form.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// DISTINCT-BY-DESIGN (E-P6 allowlist): storage AUDIT tokens for the AS-OF cell delta — never Bim `Review/diff`'s
// wire-frozen six-row `[JsonDerivedType]` ChangeKind discriminator roster; the rosters are disjoint.
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

// Each direction carries BOTH its behaviours as rows — how it lays a reel and which face of a frame a seek reads —
// so no `Rewind` bool sits between the case and its meaning, and a third direction is one row no consumer edits.
[SmartEnum]
public sealed partial class SeekDirection {
    public static readonly SeekDirection Forward = new(lay: static frames => frames, face: static frame => frame.After);
    public static readonly SeekDirection Backward = new(lay: static frames => frames.Rev(), face: static frame => frame.Before);
    private readonly Func<Seq<ScrubFrame>, Seq<ScrubFrame>> lay;
    private readonly Func<ScrubFrame, UInt128> face;
    public Seq<ScrubFrame> Lay(Seq<ScrubFrame> forward) => lay(forward);
    public UInt128 Face(ScrubFrame frame) => face(frame);
}

[SmartEnum]
public sealed partial class BlameAxis {
    public static readonly BlameAxis Node = new();
    public static readonly BlameAxis Edge = new();
}

// Four checkpoint-chain links state INDEPENDENT facts about one seal, so `Verify` ACCUMULATES the set that HELD:
// `Missing(All)` names every break at once where an `&&` ladder answered one bit and sent an operator back to the
// probe per remaining link. Refusal POSTURE belongs to whichever consumer gates on the seal — it demands through
// `Require(All, …)` and lands the refusal in its OWN band — which is why this no-band algebra mints no `Error`.
// `Version/recovery` `ReAttest` is that consumer: its commit point demands `Require(All, …)` and lands the
// break in the `RecoveryFault.VerifyFailed` band, running BESIDE `AttestedLedger.Verify` because the two
// absorb absence oppositely — that chain grades authenticity and passes an unsigned ledger as `Unsigned`,
// while these four links grade the checkpoint chain's integrity and admit no absence at all.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SealLink : ICapability<SealLink> {
    public static readonly SealLink Address  = new("address");
    public static readonly SealLink Chain    = new("chain");
    public static readonly SealLink BackLink = new("back-link");
    public static readonly SealLink Monotone = new("monotone");
}

// --- [MODELS] --------------------------------------------------------------------------
// TimeCut bounds the AS-OF fold with an inclusive Hlc ceiling, its modality, and the optional Marten stream version.
// Of(Instant) binds AggregateStreamAsync(timestamp:), AtVersion binds AggregateStreamAsync(version:), and Precise(Hlc)
// resolves the version through TimeLog.VersionAt. Version/recovery#RECOVERY_ROUTES imports TimeCut.Of, pinning that
// name and factory as public contract.
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

// AsOfQuery requests one reconstruction: the cut, the optional branch scope, and an optional rooted-node-key prefix
// narrowing the diff and blame folds. Version/retention#RETENTION_CLASSES keeps history append-only, so redaction has
// no stance here.
public readonly record struct AsOfQuery(TimeCut Cut, Option<string> Branch, Option<NodeId> NodeKeyPrefix) {
    public static AsOfQuery At(Instant cut) => new(TimeCut.Of(cut), None, None);
    public static AsOfQuery AtVersion(long version, Hlc ceiling) => new(TimeCut.AtVersion(version, ceiling), None, None);
    public bool Selects(NodeId key) => NodeKeyPrefix.Map(p => key.Value.StartsWith(p.Value, StringComparison.Ordinal)).IfNone(true);
}

// Checkpoint seals reproducibility: Element/codec's ONE snapshot hasher mints the reconstructed ElementGraph's
// order-independent ContentAddress, chained against the prior checkpoint's rolling Hash. Chaining proves reproduction
// from the stream and claims no cryptographic authenticity — Version/provenance#ATTESTED_LEDGER owns authenticity and
// defers reproducibility here. Version/recovery#RECOVERY_ROUTES seals it to cold storage,
// Version/retention#RETENTION_CLASSES governs it as the `snapshot` class, and At/Hash/Prior stay public contract.
public readonly record struct Checkpoint(Hlc At, long Version, ContentAddress Address, UInt128 Hash, Option<UInt128> Prior) {
    // AsOfKey publishes the S3 cross-runtime seam key: seam ContentAddress.OfGraph at the cut, the ONE digest the python
    // icechunk snapshot identity keys by AND Version/recovery ReAttest proves against; Hash stays tamper evidence only.
    public ContentAddress AsOfKey => Address;
}

// KeyDelta carries one per-(NodeId, member) AS-OF delta between two reconstructions: canonical member path, axis (Node
// for a `Nodes[id]` member, Edge for an incident topology change attributed to an endpoint), class, and from/to content
// address — id-inclusive node address on the Node axis, the edge's own content key on the Edge axis.
public readonly record struct KeyDelta(NodeId Node, string Member, BlameAxis Axis, ChangeKind Kind, Option<UInt128> From, Option<UInt128> To);

// RangeDiff filters its node-presence accessors to the NODE axis so an incident edge change never mis-reports an
// existing node as added/removed; `EdgesChanged` names the nodes whose incident topology shifted (Edge-axis deltas).
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
    public Option<UInt128> StateAt(Hlc at, SeekDirection direction) => Seek(at).Map(direction.Face);
}

// Bisect answers a closed THREE-case verdict, never a frame-and-flag product: `(Some frame, floor:true)` was
// representable and meant nothing, and `(None, false)` conflated "no flip inside the range" with "the predicate was
// already broken before the window opened" — two findings an operator acts on differently. `Probes` rides beside the
// verdict as the search's cost, the one fact every case shares.
[Union]
public abstract partial record BisectVerdict {
    private BisectVerdict() { }

    public sealed record HeldAtFloor : BisectVerdict;
    public sealed record Flipped(ScrubFrame Frame) : BisectVerdict;
    public sealed record NeverFlipped : BisectVerdict;
}

public readonly record struct BisectOutcome(BisectVerdict Verdict, long Probes);

// `SnapshotFloor` is the VERSION Marten's periodic snapshot seeded the fold from, absent when the fold ran from an
// empty stream — the option carrier where a `SnapshotHit` bool answered that a floor existed while erasing which,
// so the fold's real cost (`Version - floor`) was unrecoverable from the receipt that claimed to measure it.
// `EventsToCut` renames what the field always held: the stream census up to the cut, NOT the count folded past the
// floor — the two differ by exactly the snapshot's reach, and one name over both certified a census as a workload.
public readonly record struct TimeTravelReceipt(string Slot, TimeCut Cut, long Version, long EventsToCut, Option<long> SnapshotFloor, UInt128 Address, Duration Elapsed, Instant At, CorrelationId Correlation);

// TimeLog ports every read over the Marten event stream and the AS-OF fold. Reconstruct wraps AggregateStreamAsync (the
// periodic Marten snapshot is the fold floor Marten seeds from internally — Floor names the VERSION it seeded at and
// answers None on an unsnapshotted stream) and OWNS branch scoping: it receives the whole AsOfQuery, so an AsOfQuery.Branch=Some restricts the
// fold to the branch head's commit closure off the commit-DAG (the composition root wires the BranchRef-head and
// reachable-commit resolution into this one delegate), an AsOfQuery.Branch=None folds the global model stream;
// ReconstructAt is the by-version probe scrub/bisect drive; Events wraps FetchStreamAsync; VersionAt resolves a
// precise Hlc cut to a stream version. The injected ProjectionContext frame seats the clock marks beside the
// kernel causal pair — CorrelationId and TenantContext, the signal capsule's own S0 types, so this port names
// one type family and a ClockPolicy or Principal field stays deleted; the kernel ReceiptSinkPort emits the
// folded TimeTravelReceipt under its own Slot (the codec Write / ledger Stamp
// emission law — a Slot-carrying receipt returned bare with no sink is the deleted split-brain). NO redaction
// delegate, and NO standalone branch-head/closure port field — the branch scope rides AsOfQuery.Branch into
// Reconstruct rather than a second unwired delegate beside it.
public sealed record TimeLog(
    Func<AsOfQuery, IO<(ElementGraph Graph, long Version, Option<long> Floor)>> Reconstruct,
    Func<long, IO<ElementGraph>> ReconstructAt,
    Func<TimeCut, IO<Seq<IEvent<GraphEvent>>>> Events,
    Func<Hlc, IO<long>> VersionAt,
    ModelId Model, ProjectionContext Frame, ReceiptSinkPort Sink);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TimeTravel {
    // Slots mounts this rail's one receipt kind into the registry census (`Store/observability#SLOT_REGISTRY`).
    public static readonly Seq<StoreSlot> Slots = Seq(StoreSlot.Create("store.timetravel.asof"));

    public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log) => Folded(query, log).Map(static fold => fold.Receipt);

    public static IO<ElementGraph> Graph(AsOfQuery query, TimeLog log) => log.Reconstruct(query).Map(static r => r.Graph);

    public static IO<Checkpoint> Anchor(AsOfQuery query, Option<Checkpoint> prior, TimeLog log) =>
        log.Reconstruct(query with { Branch = None }).Map(reconstructed => Seal(prior, query.Cut.Ceiling, reconstructed.Version, ContentAddress.OfGraph(reconstructed.Graph)));

    public static Checkpoint Seal(Option<Checkpoint> prior, Hlc at, long version, ContentAddress address) =>
        new(at, version, address, ChainHash(prior.Map(static p => p.Hash), address), prior.Map(static p => p.Hash));

    // Verify re-folds the chain and answers the set of links that HELD. The four probes are independent, so they
    // accumulate rather than short-circuit: a tampered seal names its address, chain, back-link, and monotonicity
    // breaks in one read. A caller gating on the whole seal demands `Require(CapabilitySet<SealLink>.All, …)` under
    // its OWN band, and a caller reporting damage reads `Missing(All)`.
    public static CapabilitySet<SealLink> Verify(Checkpoint checkpoint, Option<Checkpoint> prior, ContentAddress reconstructed) =>
        Seq((Link: SealLink.Address,  Held: checkpoint.Address == reconstructed),
            (Link: SealLink.Chain,    Held: checkpoint.Hash == ChainHash(prior.Map(static p => p.Hash), reconstructed)),
            (Link: SealLink.BackLink, Held: checkpoint.Prior == prior.Map(static p => p.Hash)),
            (Link: SealLink.Monotone, Held: checkpoint.Version >= prior.Map(static p => p.Version).IfNone(0L)))
            .Filter(static row => row.Held)
            .Fold(CapabilitySet<SealLink>.None, static (held, row) => held.With(row.Link));

    // One link on the kernel alphabet: `Optional` writes the presence byte before the prior hash, so a genesis seal
    // and a seal whose prior happened to digest to zero can never alias — the absence the bare LE append left open.
    static UInt128 ChainHash(Option<UInt128> prior, ContentAddress address) =>
        ContentHash.Of((Prior: prior, Address: address), static (link, w) =>
            w.Optional(link.Prior, static (held, x) => { x.U128(held); }).U128(link.Address.Value));

    public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log) =>
        from a in log.Reconstruct(from)
        from b in log.Reconstruct(to)
        select new RangeDiff(from.Cut, to.Cut, Deltas(from, a.Graph, b.Graph));

    // Deltas projects the AS-OF member delta from the Generator.Equals Inequalities change-set over the [Equatable] ElementGraph
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

    // EdgeDeltas owns the EDGE axis of the AS-OF diff — the topology delta node-keyed Inequalities cannot recover. Each edge is keyed
    // through the SEAM `ContentAddress.Of(edge, tolerance)` STREAMING arm under its OWN graph's
    // `Header.Tolerance` (the ONE seam hasher, and the only edge-key entry there is: the seam retired
    // `Relationship.ToCanonicalBytes`, so no materialized edge-byte array exists to hash beside it and a raw
    // `XxHash128` call site is the deleted spelling; the SAME edge content
    // key Version/merge#STRUCTURAL_DIFF composes, the SAME grid the node-side
    // `ContentAddress.Of(n, a.Header.Tolerance)` keying above uses), so an edge in `b` not in `a` is Added, in `a` not `b`
    // Removed (a rewired endpoint is one Removed + one Added — the content key changed). Threading the model tolerance
    // keeps a `Generic` edge's `PropertyValue.Measure` attributes quantized on the same grid (the five typed edge cases
    // carry no Measure and are tolerance-insensitive), so a sub-tolerance measure jitter never reads as a phantom edge
    // Added+Removed. Each changed edge attributes to BOTH endpoint NodeIds — the SAME both-endpoint attribution Blame uses —
    // carrying the edge's own content key as the from/to address and honoring NodeKeyPrefix.
    static Seq<KeyDelta> EdgeDeltas(AsOfQuery query, ElementGraph a, ElementGraph b) {
        HashMap<UInt128, Relationship> fromEdges = toHashMap(a.Edges.Select(e => (ContentAddress.Of(e, a.Header.Tolerance).Value, e)));
        HashMap<UInt128, Relationship> toEdges = toHashMap(b.Edges.Select(e => (ContentAddress.Of(e, b.Header.Tolerance).Value, e)));
        Seq<(Relationship Edge, ChangeKind Kind, Option<UInt128> Key)> changed =
            toSeq(toEdges.Filter((key, _) => !fromEdges.ContainsKey(key)).Map(static (key, e) => (e, ChangeKind.Added, Some(key))).Values)
            + toSeq(fromEdges.Filter((key, _) => !toEdges.ContainsKey(key)).Map(static (key, e) => (e, ChangeKind.Removed, Some(key))).Values);
        return changed.Bind(row => row.Edge.Members.Filter(query.Selects)
            .Map(node => new KeyDelta(node, nameof(ElementGraph.Edges), BlameAxis.Edge, row.Kind,
                row.Kind == ChangeKind.Removed ? row.Key : None, row.Kind == ChangeKind.Added ? row.Key : None)));
    }

    public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log) =>
        log.Events(query.Cut).Bind(events => IO.liftFin(toSeq(events
            .Bind(e => Touched(e).Filter(cell => query.Selects(cell.Node)).Map(cell => (Cell: cell, Event: e)))
            .GroupBy(static row => (row.Cell.Node, row.Cell.Member, row.Cell.Axis))
            .Select(group => {
                // GroupBy yields a non-empty group, so `ordered[0]` reads the highest-version winner directly —
                // LanguageExt Seq.Head returns Option<A>, never the bare row.
                Seq<((NodeId Node, string Member, BlameAxis Axis) Cell, IEvent<GraphEvent> Event)> ordered = toSeq(group.OrderByDescending(static r => r.Event.Version));
                ((NodeId Node, string Member, BlameAxis Axis) Cell, IEvent<GraphEvent> Event) winner = ordered[0];
                return from authored in AuthorshipOf(winner.Event, log.Frame.Key)
                       from superseded in ordered.Tail.Traverse(row => AuthorshipOf(row.Event, log.Frame.Key).Map(author =>
                           new BlameContributor(author.Actor, author.Origin,
                               new Hlc(Instant.FromDateTimeOffset(row.Event.Timestamp), (ulong)row.Event.Version), row.Event.Version))).As()
                       select new BlameRow(group.Key.Node, group.Key.Member, group.Key.Axis,
                           authored.Actor, authored.Origin,
                           new Hlc(Instant.FromDateTimeOffset(winner.Event.Timestamp), (ulong)winner.Event.Version), winner.Event.Version,
                           ordered.Count, superseded);
            })).Traverse(static row => row).As()));

    public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log) =>
        from events in log.Events(query.Cut)
        let windowed = toSeq(events.Filter(e => window.Includes(Instant.FromDateTimeOffset(e.Timestamp))).OrderBy(static e => e.Version))
        from seeded in log.ReconstructAt(windowed.Head.Map(static e => e.Version - 1L).IfNone(0L))
        // Each frame replays one event's GraphDelta through the SAME raw ReplayOnto the inline projection folds, never
        // through the re-validating Apply — a stream-resident delta is total by construction, so re-validation here is
        // deleted. Frame addresses ride the seam GraphMembers accumulator seeded ONCE per window: Advance steps the
        // member sets by one delta and OfGraph(members) re-enters the seam's own private sorted fold, so a frame
        // address is byte-identical to the recompute while the reel pays one member step per event instead of one
        // whole-graph fold. A tolerance reheader returns the seam's explicit `Refold` outcome; only that outcome
        // re-seeds from the replayed graph while every genuine advance failure stays on the rail.
        from reel in IO.liftFin(windowed.Fold(
            Fin.Succ((Frames: Seq<ScrubFrame>(), Graph: seeded, Members: GraphMembers.Of(seeded), Address: ContentAddress.OfGraph(seeded))),
            (held, e) => held.Bind(acc => {
                ElementGraph next = e.Data.Body.ReplayOnto(acc.Graph);
                return AuthorshipOf(e, log.Frame.Key).Bind(author =>
                    acc.Members.Advance(e.Data.Body, log.Frame.Key).Map(step => {
                        GraphMembers members = step.Resolve(_ => GraphMembers.Of(next));
                        ContentAddress nextAddress = ContentAddress.OfGraph(members);
                        return (acc.Frames.Add(new ScrubFrame(acc.Frames.Count, e.Version, e.Data.Lifecycle,
                            new Hlc(Instant.FromDateTimeOffset(e.Timestamp), (ulong)e.Version), author.Actor,
                            acc.Address.Value, nextAddress.Value)), next, members, nextAddress);
                    }));
            })))
        select new ScrubReel(window.Direction.Lay(reel.Frames), reel.Graph, window.Span);

    // Bisect never folds the bound's own reconstruction: the floor graph and each probe are what `holds` reads, so a
    // whole AS-OF fold at the bound was one discarded snapshot per bisect. The versions come off the event census.
    public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<ElementGraph, bool> holds, TimeLog log) =>
        from events in log.Events(bound.Cut)
        let versions = toSeq(events.Map(static e => e.Version).OrderBy(static v => v))
        from floorGraph in log.ReconstructAt(versions.Head.Map(static v => v - 1L).IfNone(0L))
        from outcome in Descend(holds, floorGraph, versions, toHashMap(events.Map(static e => (e.Version, e))), log)
        select outcome;

    static IO<BisectOutcome> Descend(Func<ElementGraph, bool> holds, ElementGraph floor, Seq<long> versions, HashMap<long, IEvent<GraphEvent>> byVersion, TimeLog log) {
        IO<(int Flip, long Probes)> Search(int lo, int hi, long probes) {
            if (lo >= hi) { return IO.pure((lo, probes)); }
            int mid = lo + ((hi - lo) >> 1);
            long version = versions[mid];
            return log.ReconstructAt(version).Bind(graph => holds(graph) ? Search(lo, mid, probes + 1) : Search(mid + 1, hi, probes + 1));
        }
        return holds(floor)
            ? IO.pure(new BisectOutcome(new BisectVerdict.HeldAtFloor(), 0L))
            : Search(0, versions.Count, 0L).Bind(found => found.Flip < versions.Count && byVersion.Find(versions[found.Flip]) is { IsSome: true, Case: IEvent<GraphEvent> flipEvent }
                ? from before in log.ReconstructAt(versions[found.Flip] - 1L)
                  from after in log.ReconstructAt(versions[found.Flip])
                  from frame in IO.liftFin(FlipFrame(found.Flip, flipEvent, before, after, log.Frame.Key))
                  select new BisectOutcome(new BisectVerdict.Flipped(frame), found.Probes)
                : IO.pure(new BisectOutcome(new BisectVerdict.NeverFlipped(), found.Probes)));
    }

    // Each side of the flip receives the canonical full-graph address every other AS-OF consumer reads. The AFTER side
    // rides the seam accumulator seeded off the BEFORE graph and stepped by the flip event's own delta — one member
    // step instead of a second whole-graph fold, and byte-identical to it — only the seam's tolerance-reheader
    // `Refold` outcome re-seeds off the reconstructed after-graph.
    static Fin<ScrubFrame> FlipFrame(int index, IEvent<GraphEvent> e, ElementGraph before, ElementGraph after, Op key) {
        GraphMembers members = GraphMembers.Of(before);
        ContentAddress priorAddress = ContentAddress.OfGraph(members);
        return from author in AuthorshipOf(e, key)
               from step in members.Advance(e.Data.Body, key)
               let afterAddress = ContentAddress.OfGraph(step.Resolve(_ => GraphMembers.Of(after)))
               select new ScrubFrame(index, e.Version, e.Data.Lifecycle,
                   new Hlc(Instant.FromDateTimeOffset(e.Timestamp), (ulong)e.Version), author.Actor,
                   priorAddress.Value, afterAddress.Value);
    }

    public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, GrantSet acl, Guid origin, TimeLog log, Func<string, Guid, ContentAddress, CommitMessage, IO<CommitNode>> mintBranchCommit) =>
        from reconstructed in log.Reconstruct(query)
        let address = ContentAddress.OfGraph(reconstructed.Graph)
        from commit in mintBranchCommit(newBranch, origin, address, new CommitMessage("branch-from-past", string.Empty))
        select new BranchRef(newBranch, RefKind.Branch, commit.ContentKey, acl, origin, commit.Cell.Physical, None, None, CommitMessage.Empty, string.Empty);

    // `ElementJson` source-gen-registers the receipt (the codec seal-evidence precedent) so the sink message
    // envelope payload crosses the strict resolver TYPED, mirroring `Element/codec#SNAPSHOT_SPINE` exactly.
    // `ReceiptSinkPort.Send` takes the kernel causal pair and the frame SEATS it, so emission passes the frame's
    // own values straight through: no edge lift, and no `TenantContext.Current` ambient read attributing this
    // receipt to whatever tenancy the calling thread happens to hold rather than the one the fold ran under.
    static IO<(ElementGraph Graph, TimeTravelReceipt Receipt)> Folded(AsOfQuery query, TimeLog log) =>
        from mark in IO.lift(log.Frame.Mark)
        from reconstructed in log.Reconstruct(query)
        from events in log.Events(query.Cut)
        let address = ContentAddress.OfGraph(reconstructed.Graph)
        let receipt = new TimeTravelReceipt("store.timetravel.asof", query.Cut, reconstructed.Version, events.Count, reconstructed.Floor, address.Value, log.Frame.Elapsed(mark), log.Frame.Now(), log.Frame.Correlation)
        from _ in log.Sink.Send(log.Frame.Correlation, log.Frame.Tenant, TelemetrySource.Persistence.Key, receipt.Slot, JsonSerializer.SerializeToElement(receipt, ElementJson.Options))
        select (reconstructed.Graph, receipt);

    // CellOf resolves the (NodeId, member) cell a Generator.Equals NODE-axis MemberPath addresses: the `Nodes` `[UnorderedEquality]` map
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
        GraphDelta delta = e.Data.Body;
        return delta.AddedNodes.Map(static n => (n.Id, nameof(GraphDelta.AddedNodes), BlameAxis.Node))
            + delta.RevisedNodes.Map(static n => (n.After.Id, nameof(GraphDelta.RevisedNodes), BlameAxis.Node))
            + delta.RemovedNodes.Map(static id => (id, nameof(GraphDelta.RemovedNodes), BlameAxis.Node))
            + delta.AddedEdges.Bind(static r => r.Members.Map(ep => (ep, nameof(GraphDelta.AddedEdges), BlameAxis.Edge)))
            + delta.RemovedEdges.Bind(static r => r.Members.Map(ep => (ep, nameof(GraphDelta.RemovedEdges), BlameAxis.Edge)));
    }

    // Authorship rides the Marten event headers `GraphStore.Blame` stamps. Both members are required durable facts;
    // absence or malformed origin rails as the kernel invalid-input fault instead of minting an anonymous actor or
    // zero origin that collapses unrelated authors into one bucket.
    static Fin<(string Actor, Guid Origin)> AuthorshipOf(IEvent<GraphEvent> e, Op key) =>
        e.Headers is { } headers
        && headers.TryGetValue("actor", out object? actor)
        && actor?.ToString() is { Length: > 0 } subject
        && headers.TryGetValue("origin", out object? origin)
        && Guid.TryParse(origin?.ToString(), out Guid store)
            ? Fin.Succ((subject, store))
            : Fin.Fail<(string Actor, Guid Origin)>(key.InvalidInput());
}
```

| [INDEX] | [POLICY]         | [VALUE]                                                     | [BINDING]                                               |
| :-----: | :--------------- | :---------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | one materializer | `GraphDelta.ReplayOnto`                                     | reconstruction equals the live `ElementGraph`           |
|  [02]   | as-of fold       | `AggregateStreamAsync(version\|timestamp)`                  | Marten folds from the periodic snapshot floor           |
|  [03]   | as-of cut        | `TimeCut.Ceiling` precise/instant/version                   | the Marten fold binds version or ceiling instant        |
|  [04]   | checkpoint chain | `ContentAddress.OfGraph` + `ChainHash` on `CanonicalWriter` | `Anchor` seals, `Verify` re-folds; reproducibility only |
|  [05]   | seal proof       | `CapabilitySet<SealLink>` of the links that held            | links accumulate; refusal posture is the caller's       |
|  [06]   | as-of key (S3)   | `Checkpoint.AsOfKey` = `Address` (icechunk + `ReAttest`)    | the ONE cross-runtime digest; `Hash` never the key      |
|  [07]   | range diff       | node `Inequalities` + edge `ContentAddress.Of` set-diff     | both axes; topology rewire never dropped                |
|  [08]   | edge key         | seam `ContentAddress.Of(edge, tolerance)` streaming         | the retired `ToCanonicalBytes` twin materialized bytes  |
|  [09]   | bisect descent   | `O(log n)` probes of a MONOTONE predicate                   | three-case verdict; the bound is never folded           |
|  [10]   | frame address    | seam `GraphMembers` step + `OfGraph(members)`               | byte-identical to the recompute; reheader re-seeds      |
|  [11]   | blame            | touching `GraphEvent` by version                            | `(NodeId, change-kind, axis)` cell, forward-log grain   |
|  [12]   | fold floor       | `Option<long> SnapshotFloor` beside `EventsToCut`           | the folded cost is `Version - floor`, never a bool      |

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
