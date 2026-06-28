# [PERSISTENCE_VERSION_TIMETRAVEL]

Rasm.Persistence AS-OF time travel over the Marten event substrate: one `TimeTravel` engine that reconstructs, diffs, blames, scrubs, bisects, checkpoints, and branches-from-past over the HLC op-log prefix (`Version/ledger#CHANGEFEED`) and the Marten `AggregateSnapshot` spine. Every operation folds the `OpLogEntry` prefix up to a `TimeCut` against the nearest chained `Checkpoint`, replaying the prefix through the IDENTICAL `Crdt.Apply` the live merge and the `Element/graph#GRAPH_PROJECTION` inline projection use, so a historical materialization is bit-identical to the live state at that cut — there is no second materializer. The cut is a precise `Hlc`, an `Instant`, or a Marten stream version (one value object, three modalities binding the Marten `AggregateStreamAsync` version/timestamp at the graph altitude and the CRDT-cell HLC fold at the field altitude); the branch scope resolves through the commit-DAG `BranchRef` head the cut reconstructs to; reconstruction crossing a redacted op carries the `ExportProof` mask, never silently folding erased bytes. The `Checkpoint` is the Marten `AggregateSnapshot` re-keyed to the CRDT cell map plus a hash-chained content address so a checkpoint sequence is itself a verifiable chain. `ClockPolicy`, `CorrelationId`, `TenantContext`, and `ReceiptSinkPort` arrive from AppHost; `Hlc`, `Crdt`, `CrdtOp`, `CrdtWire`, `CommitNode`, `BranchRef`, `VersionVector` arrive from `Version/commits`; `NodeId`, `ModelId` arrive from `Rasm.Element`.

## [01]-[INDEX]

- [01]-[TIME_TRAVEL]: cut algebra, chained checkpoint anchor over the Marten snapshot, AS-OF reconstruction, range diff, blame, scrub, bisect, and branch-from-past.

## [02]-[TIME_TRAVEL]

- Owner: `TimeCut` the `[ComplexValueObject]` AS-OF boundary carrying a precise `Hlc` ceiling, its `CutKind` modality, and the optional Marten stream version; `AsOfQuery` the one reconstruction-request shape (cut, optional branch, node-key prefix, redaction stance); `Checkpoint` a sealed materialized-state fold anchor that hash-chains the prior checkpoint's `Hash`; `RangeDiff` the two-cut delta carrying per-cell `(from, to)` content keys and a `ChangeKind`-classified change set; `BlameRow` per `(NodeId, Field)` authorship attribution carrying the winning op AND its superseded contributor lineage; `ScrubFrame`/`ScrubReel` the ordered replay reel; `BisectOutcome` the first-flip locus of a MONOTONE history predicate; `TimeLog` the one read/closure/redaction port over the changefeed and the Marten snapshot; `TimeTravel` the static surface.
- Cases: `TimeCut` is `Precise(Hlc)`, `Of(Instant)`, or `AtVersion(long, Hlc)` collapsed into one value-object whose `Ceiling` is the inclusive HLC bound and whose `StreamVersion` binds the Marten fold; `ChangeKind` is `Added | Removed | Replaced | Converged` — `Replaced` an LWW/MV single-writer flip, `Converged` a set/counter/sequence merge; `RedactionStance` is `Fold | Mask | Reject`; `SeekDirection` is `Forward | Backward`; reconstruction seeds each new `(NodeId, Field)` cell with the EMPTY `CrdtField` arm the decoded op demands (the engine-local `Seed` total switch over `CrdtOp`, the inverse of `Crdt.Apply`).
- Entry: `public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log)` folds the prefix from the nearest checkpoint through `Crdt.Apply` into the AS-OF state; `public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log)` content-key-differences two reconstructed cuts; `public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log)` folds the winning-plus-superseded authorship per cell; `public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log)` materializes the ordered replay reel; `public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<HashMap<CellKey, CrdtField>, bool> holds, TimeLog log)` binary-searches the first cut where a MONOTONE predicate flips; `public static IO<BranchRef> BranchFromPast(...)` forks a new branch over the reconstructed op-key set; `public static IO<Checkpoint> Anchor(AsOfQuery query, TimeLog log)` seals the AS-OF state against the nearest prior checkpoint; `public static bool Verify(Checkpoint checkpoint, Option<Checkpoint> prior, Seq<OpLogEntry> suffix)` re-folds the suffix and confirms the chain.
- Auto: AS-OF reconstruction reads the op-log prefix bounded by `TimeCut.Ceiling` and folds it through the same `Crdt.Apply` the live path runs, seeding each absent cell with the empty arm the op's decoded type requires, so a historical materialization equals the live state field-for-field; the nearest `Checkpoint` is the fold floor and its `State` the seed (re-keyed from the Marten `AggregateSnapshot`), so a deep history folds the suffix from the snapshot rather than genesis; a checkpoint seal hash-chains the prior `Hash` into a fresh `XxHash128` then appends each suffix op's `ContentKey` so a checkpoint's `Hash` is the rolling content address of the whole prefix; range diff reconstructs both cuts and content-key-differences the maps; blame groups the prefix by `(NodeId, Field)` and retains the superseded contributors so authorship carries the full causal lineage the `Version/provenance#CAUSAL_DAG` reconciles against; scrub folds the windowed prefix as `ScrubFrame` values carrying BOTH the pre-op `Before` and post-op `StateKey`; bisect binary-searches the sorted prefix re-folding each candidate sub-prefix from the SAME checkpoint anchor seed.
- Receipt: every reconstruction-shaped operation folds a typed `TimeTravelReceipt`; a checkpoint seal carries its own evidence in the returned `Checkpoint` and `Verify` returns the pure chain-validity probe.
- Packages: Marten (`AggregateStreamAsync`/`AggregateSnapshot`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, BCL inbox.
- Growth: a new replay projection is one method on `TimeTravel`; a new attribution dimension is one field on `BlameRow`; a new change classification is one `ChangeKind` row; a new cut modality is one `TimeCut` case; a new redaction stance is one `RedactionStance` row; zero new surface — a temporal-table mirror, a second history store, a snapshot-per-instant materialization, or a parallel bisect walker is the deleted form because reconstruction folds the changefeed the Marten stream already holds and pins the heavy cuts to the Marten `AggregateSnapshot` as chained `Checkpoint` fold anchors.
- Boundary: reconstruction is a pure left-fold of the op-log prefix through `Crdt.Apply`, so the AS-OF state at any cut is reproducible from the changefeed and the checkpoints — the `Checkpoint.State` is the branch-scoped materialization a same-query reconstruction seeds from and its `Hash` is the branch-agnostic running content address of the global crdt prefix above the floor (a non-cryptographic content chain, never an authenticity claim — `Version/provenance#ATTESTED_LEDGER` owns tamper-evidence), so a branched read never over-seeds and `Verify` re-folds the same global suffix over the prior `Hash` confirming the rolling address, the `Prior` back-link, and the cumulative `OpCount`; the empty-seed dispatch is engine-local (`Crdt` exposes no seed inverse, so `Seed` reads the decoded op's data-type arm through the GENERATED total `CrdtOp.Switch` — a new `CrdtOp` case breaks the build at `Seed`, never a runtime-silent default) so seeding a wrong arm never drops a first `Add`/`Increment`/`InsertAfter`/`Write`; the cell key is `(NodeId, Field)` where `Field` is the `CrdtOp.Field` the live merge keys on, never the column family; the branch scope is the `BranchRef` head the cut reconstructs to (`TimeLog.HeadAt` then `Closure`), never `OpLogEntry`'s column family; `RangeDiff` reconstructs both endpoints (two AS-OF folds, never a stored delta chain) and the `Replaced`/`Converged` distinction reads the `CrdtField` arm; `BlameRow` reads the same `(Hlc, origin)` winner the convergence selected; the checkpoint chain folds each op's `ContentKey` (the cross-runtime-reproducible companion-byte key) so the rolling address is reproducible from the one op stream a Python or TS replica folds; bisect's `holds` is MONOTONE so the first-flip locus is a lower-bound binary search, `HeldAtFloor` short-circuiting an already-broken floor; branch-from-past mints a root `CommitNode` over the reconstructed op-key set through the `mintBranchCommit` seam, the commit cell riding the op-log HLC stamp, never `DateTime.UtcNow`.

```csharp signature
[SmartEnum]
public sealed partial class CutKind {
    public static readonly CutKind Precise = new();
    public static readonly CutKind Instant = new();
    public static readonly CutKind Version = new();
}

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

[SmartEnum]
public sealed partial class RedactionStance {
    public static readonly RedactionStance Fold = new();
    public static readonly RedactionStance Mask = new();
    public static readonly RedactionStance Reject = new();
}

[SmartEnum]
public sealed partial class SeekDirection {
    public static readonly SeekDirection Forward = new(rewind: false);
    public static readonly SeekDirection Backward = new(rewind: true);
    public bool Rewind { get; }
    public Seq<ScrubFrame> Lay(Seq<ScrubFrame> forward) => Rewind ? forward.Rev() : forward;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<VersionKeyPolicy, string>]
[KeyMemberComparer<VersionKeyPolicy, string>]
public sealed partial class ChangeKind {
    public static readonly ChangeKind Added = new("added");
    public static readonly ChangeKind Removed = new("removed");
    public static readonly ChangeKind Replaced = new("replaced");
    public static readonly ChangeKind Converged = new("converged");
}

[ComplexValueObject]
public sealed partial class CellKey {
    public NodeId Node { get; }
    public string Field { get; }
    public override string ToString() => $"{Node.Value}:{Field}";
}

public readonly record struct AsOfQuery(TimeCut Cut, Option<string> Branch, Option<string> NodeKeyPrefix, RedactionStance Redaction) {
    public static AsOfQuery At(Instant cut) => new(TimeCut.Of(cut), None, None, RedactionStance.Fold);
    public bool Selects(OpLogEntry entry, Hlc floor) =>
        entry.Family == ColumnFamily.Crdt && Cut.Admits(new Hlc(entry.Physical, entry.Logical))
        && new Hlc(entry.Physical, entry.Logical).CompareTo(floor) > 0
        && NodeKeyPrefix.Map(p => entry.EntityKey.StartsWith(p, StringComparison.Ordinal)).IfNone(true);
}

public readonly record struct Checkpoint(Hlc At, UInt128 Hash, HashMap<CellKey, CrdtField> State, long OpCount, Option<UInt128> Prior);
public readonly record struct KeyDelta(CellKey Key, ChangeKind Kind, Option<UInt128> From, Option<UInt128> To);
public readonly record struct RangeDiff(TimeCut From, TimeCut To, Seq<KeyDelta> Deltas) {
    public Seq<CellKey> Added => Deltas.Filter(static d => d.Kind == ChangeKind.Added).Map(static d => d.Key);
    public Seq<CellKey> Removed => Deltas.Filter(static d => d.Kind == ChangeKind.Removed).Map(static d => d.Key);
    public Seq<CellKey> Changed => Deltas.Filter(static d => d.Kind == ChangeKind.Replaced || d.Kind == ChangeKind.Converged).Map(static d => d.Key);
}

public readonly record struct BlameContributor(string Actor, Guid Origin, Hlc Cell, UInt128 ContentKey);
public readonly record struct BlameRow(CellKey Key, string Actor, Guid Origin, Hlc Cell, UInt128 ContentKey, int Contributors, Seq<BlameContributor> Superseded);
public readonly record struct ScrubFrame(long Index, OpLogEntry Entry, Hlc At, UInt128 Before, UInt128 StateKey, bool Redacted);

public readonly record struct ScrubWindow(Interval Span, SeekDirection Direction) {
    public static ScrubWindow Forward(Interval span) => new(span, SeekDirection.Forward);
    public bool Includes(Instant at) => (!Span.HasStart || Span.Start <= at) && (!Span.HasEnd || at <= Span.End);
}

public readonly record struct ScrubReel(Seq<ScrubFrame> Frames, HashMap<CellKey, CrdtField> Terminal, Interval Span) {
    public Option<ScrubFrame> Seek(Hlc at) =>
        Frames.Filter(f => f.At.CompareTo(at) >= 0).Fold(Option<ScrubFrame>.None, static (best, f) => Some(best.Filter(b => b.At.CompareTo(f.At) <= 0).IfNone(f)));
    public Option<UInt128> StateAt(Hlc at, SeekDirection direction) => Seek(at).Map(f => direction.Rewind ? f.Before : f.StateKey);
}

public readonly record struct BisectOutcome(Option<ScrubFrame> FirstFlip, long Probes, bool HeldAtFloor);
public readonly record struct TimeTravelReceipt(string Slot, TimeCut Cut, long OpsFolded, bool CheckpointHit, int Redactions, Duration Elapsed, Instant At, CorrelationId Correlation);

public sealed record TimeLog(
    Func<Hlc, IO<Seq<OpLogEntry>>> UpTo, Func<AsOfQuery, IO<Option<Checkpoint>>> Nearest, Func<TimeCut, IO<Option<BranchRef>>> HeadAt,
    Func<BranchRef, IO<LanguageExt.HashSet<UInt128>>> Closure, Func<OpLogEntry, IO<Option<ExportProof>>> Redacted,
    CorrelationId Correlation, ClockPolicy Clocks);

public static class TimeTravel {
    public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log) => Folded(query, log).Map(static fold => fold.Receipt);

    public static HashMap<CellKey, CrdtField> Materialize(Option<Checkpoint> anchor, AsOfQuery query, Seq<OpLogEntry> prefix, LanguageExt.HashSet<UInt128> scope, HashMap<UInt128, ExportProof> masks) {
        var seed = anchor.Map(static c => c.State).IfNone(HashMap<CellKey, CrdtField>());
        var floor = anchor.Map(static c => c.At).IfNone(Hlc.Zero);
        return Within(prefix, query, scope, floor).Fold(seed, (state, entry) => Decode(entry, query.Redaction, masks).Match(
            Some: step => Step(state, entry.EntityKey, step.Op, step.Masked),
            None: () => state));
    }

    public static Checkpoint Seal(Option<Checkpoint> prior, Hlc at, HashMap<CellKey, CrdtField> state, Seq<OpLogEntry> suffix) {
        var folded = toSeq(suffix.Filter(static e => e.Family == ColumnFamily.Crdt).OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId)));
        return new Checkpoint(at, ChainHash(prior.Map(static p => p.Hash), folded), state, prior.Map(static p => p.OpCount).IfNone(0L) + folded.Count, prior.Map(static p => p.Hash));
    }

    public static IO<Checkpoint> Anchor(AsOfQuery query, TimeLog log) =>
        from prefix in Prefix(query with { Redaction = RedactionStance.Fold }, log)
        let above = prefix.Entries.Filter(e => new Hlc(e.Physical, e.Logical).CompareTo(prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)) > 0)
        let state = Materialize(prefix.Anchor, query with { Redaction = RedactionStance.Fold }, prefix.Entries, prefix.Scope, HashMap<UInt128, ExportProof>())
        select Seal(prefix.Anchor, query.Cut.Ceiling, state, above);

    public static bool Verify(Checkpoint checkpoint, Option<Checkpoint> prior, Seq<OpLogEntry> suffix) =>
        toSeq(suffix.Filter(static e => e.Family == ColumnFamily.Crdt).OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId))) is var folded
        && checkpoint.Hash == ChainHash(prior.Map(static p => p.Hash), folded)
        && checkpoint.Prior == prior.Map(static p => p.Hash)
        && checkpoint.OpCount == prior.Map(static p => p.OpCount).IfNone(0L) + folded.Count
        && StateKey(checkpoint.State) == StateKey(Replay(prior.Map(static p => p.State).IfNone(HashMap<CellKey, CrdtField>()), folded));

    static UInt128 ChainHash(Option<UInt128> prior, Seq<OpLogEntry> folded) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        if (prior.IsSome) { BinaryPrimitives.WriteUInt128LittleEndian(word, prior.ValueUnsafe()); rolling.Append(word); }
        foreach (var entry in folded) { BinaryPrimitives.WriteUInt128LittleEndian(word, entry.ContentKey); rolling.Append(word); }
        return rolling.GetCurrentHashAsUInt128();
    }

    public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log) =>
        from a in MaterializedAt(from, log)
        from b in MaterializedAt(to, log)
        select new RangeDiff(from.Cut, to.Cut, Deltas(a, b));

    static IO<HashMap<CellKey, CrdtField>> MaterializedAt(AsOfQuery query, TimeLog log) =>
        Prefix(query, log).Map(p => Materialize(p.Anchor, query, p.Entries, p.Scope, p.Masks));

    public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log) =>
        Prefix(query, log).Map(prefix => toSeq(Within(prefix.Entries, query, prefix.Scope, Hlc.Zero)
            .Choose(entry => Decode(entry, RedactionStance.Fold, prefix.Masks).Map(step => (Key: new CellKey(NodeId.Create(entry.EntityKey), step.Op.Field), entry)))
            .GroupBy(static row => row.Key)
            .Select(static group => {
                var ordered = toSeq(group.Select(static r => r.entry).OrderByDescending(static e => (e.Physical, e.Logical, e.OriginStoreId)));
                var winner = ordered.Head;
                return new BlameRow(group.Key, winner.Actor, winner.OriginStoreId, new Hlc(winner.Physical, winner.Logical), winner.ContentKey, ordered.Count,
                    ordered.Tail.Map(static e => new BlameContributor(e.Actor, e.OriginStoreId, new Hlc(e.Physical, e.Logical), e.ContentKey)));
            })));

    public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log) =>
        Prefix(query, log).Map(prefix => {
            var floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero);
            var seed = prefix.Anchor.Map(static c => c.State).IfNone(HashMap<CellKey, CrdtField>());
            var ordered = toSeq(Within(prefix.Entries, query, prefix.Scope, floor).Filter(e => window.Includes(e.Physical)).OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId)));
            var (frames, terminal) = ordered.Fold((Frames: Seq<ScrubFrame>(), State: seed), (acc, entry) => Decode(entry, query.Redaction, prefix.Masks).Match(
                Some: step => Step(acc.State, entry.EntityKey, step.Op, step.Masked) is var next
                    ? (acc.Frames.Add(new ScrubFrame(acc.Frames.Count, entry, new Hlc(entry.Physical, entry.Logical), StateKey(acc.State), StateKey(next), step.Masked)), next) : acc,
                None: () => (acc.Frames.Add(new ScrubFrame(acc.Frames.Count, entry, new Hlc(entry.Physical, entry.Logical), StateKey(acc.State), StateKey(acc.State), Redacted: true)), acc.State)));
            return new ScrubReel(window.Direction.Lay(frames), terminal, window.Span);
        });

    public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<HashMap<CellKey, CrdtField>, bool> holds, TimeLog log) =>
        from prefix in Prefix(bound with { Redaction = RedactionStance.Fold }, log)
        let floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)
        let seed = prefix.Anchor.Map(static c => c.State).IfNone(HashMap<CellKey, CrdtField>())
        let ops = toSeq(Within(prefix.Entries, bound, prefix.Scope, floor).OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId)))
        select Descend(holds, seed, ops);

    static BisectOutcome Descend(Func<HashMap<CellKey, CrdtField>, bool> holds, HashMap<CellKey, CrdtField> seed, Seq<OpLogEntry> ops) {
        (long Flip, long Probes) Search(long lo, long hi, long probes) =>
            lo >= hi ? (lo, probes)
            : (lo + ((hi - lo) >> 1)) is var mid && holds(Replay(seed, ops.Take((int)mid + 1))) ? Search(lo, mid, probes + 1) : Search(mid + 1, hi, probes + 1);
        var heldAtFloor = holds(seed);
        var (flip, probes) = heldAtFloor ? (0L, 0L) : Search(0L, ops.Count, 0L);
        return new BisectOutcome(
            !heldAtFloor && flip < ops.Count && ops[(int)flip] is var op
                ? Some(new ScrubFrame(flip, op, new Hlc(op.Physical, op.Logical), StateKey(Replay(seed, ops.Take((int)flip))), StateKey(Replay(seed, ops.Take((int)flip + 1))), Redacted: false))
                : None,
            probes, heldAtFloor);
    }

    public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, Capability acl, Guid origin, TimeLog log, Func<string, Guid, Seq<UInt128>, CommitMessage, IO<CommitNode>> mintBranchCommit) =>
        from prefix in Prefix(query, log)
        let floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)
        let opKeys = Within(prefix.Entries, query, prefix.Scope, floor).Map(static e => e.ContentKey)
        from commit in mintBranchCommit(newBranch, origin, opKeys, new CommitMessage("branch-from-past", string.Empty))
        select new BranchRef(newBranch, RefKind.Branch, commit.ContentKey, acl, origin, commit.Cell.Physical, None, None, CommitMessage.Empty, string.Empty);

    static IO<(HashMap<CellKey, CrdtField> State, TimeTravelReceipt Receipt)> Folded(AsOfQuery query, TimeLog log) =>
        from mark in IO.lift(log.Clocks.Mark)
        from prefix in Prefix(query, log)
        let floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)
        let folded = Within(prefix.Entries, query, prefix.Scope, floor).Count
        let state = Materialize(prefix.Anchor, query, prefix.Entries, prefix.Scope, prefix.Masks)
        select (state, new TimeTravelReceipt("store.timetravel.asof", query.Cut, folded, prefix.Anchor.IsSome, prefix.Masks.Count, log.Clocks.Elapsed(mark), log.Clocks.Now, log.Correlation));

    static IO<(Option<Checkpoint> Anchor, Seq<OpLogEntry> Entries, LanguageExt.HashSet<UInt128> Scope, HashMap<UInt128, ExportProof> Masks)> Prefix(AsOfQuery query, TimeLog log) =>
        from anchor in log.Nearest(query)
        from entries in log.UpTo(query.Cut.Ceiling)
        from scope in query.Branch.Match(Some: _ => log.HeadAt(query.Cut).Bind(head => head.Match(Some: log.Closure, None: () => IO.pure(LanguageExt.HashSet<UInt128>.Empty))), None: () => IO.pure(LanguageExt.HashSet<UInt128>.Empty))
        from masks in query.Redaction == RedactionStance.Fold ? IO.pure(HashMap<UInt128, ExportProof>())
            : entries.TraverseM(e => log.Redacted(e).Map(p => (e.ContentKey, Proof: p))).As().Map(pairs => toHashMap(pairs.Choose(static row => row.Proof.Map(proof => (row.ContentKey, proof)))))
        from _ in query.Redaction == RedactionStance.Reject && !masks.IsEmpty ? IO.fail<Unit>(Error.New(8262, $"<timetravel-redaction-hold:{masks.Count}>")) : IO.pure(unit)
        select (anchor, entries, scope, masks);

    static Seq<OpLogEntry> Within(Seq<OpLogEntry> entries, AsOfQuery query, LanguageExt.HashSet<UInt128> scope, Hlc floor) =>
        entries.Filter(e => query.Selects(e, floor) && (query.Branch.IsNone || scope.Contains(e.ContentKey)));

    static Seq<KeyDelta> Deltas(HashMap<CellKey, CrdtField> a, HashMap<CellKey, CrdtField> b) =>
        toSeq(b.Keys.Where(k => !a.ContainsKey(k)).Map(k => new KeyDelta(k, ChangeKind.Added, None, Some(CellContent(b[k])))))
            .Append(a.Keys.Where(k => !b.ContainsKey(k)).Map(k => new KeyDelta(k, ChangeKind.Removed, Some(CellContent(a[k])), None)))
            .Append(a.Keys.Where(b.ContainsKey).Choose(k => (CellContent(a[k]), CellContent(b[k])) is var (from, to) && from != to ? Some(new KeyDelta(k, Classify(b[k]), Some(from), Some(to))) : None));

    static ChangeKind Classify(CrdtField field) => field.Switch(
        lwwRegister: static _ => ChangeKind.Replaced, mvRegister: static _ => ChangeKind.Replaced, orSet: static _ => ChangeKind.Converged,
        pnCounter: static _ => ChangeKind.Converged, rgaSequence: static _ => ChangeKind.Converged, ephemeralMap: static _ => ChangeKind.Converged);

    static HashMap<CellKey, CrdtField> Replay(HashMap<CellKey, CrdtField> seed, Seq<OpLogEntry> prefix) =>
        prefix.Fold(seed, static (state, entry) => CrdtWire.Decode(entry.Payload).Match(Succ: op => Step(state, entry.EntityKey, op, masked: false), Fail: _ => state));

    static HashMap<CellKey, CrdtField> Step(HashMap<CellKey, CrdtField> state, string entityKey, CrdtOp op, bool masked) => (masked, op) switch {
        (true, CrdtOp.Set set) => state.AddOrUpdate(new CellKey(NodeId.Create(entityKey), set.Field), new CrdtField.LwwRegister(set.Value, set.Cell, set.Origin)),
        _ => state.AddOrUpdate(new CellKey(NodeId.Create(entityKey), op.Field), existing => Crdt.Apply(existing, op), Crdt.Apply(Seed(op), op)),
    };

    static Option<(CrdtOp Op, bool Masked)> Decode(OpLogEntry entry, RedactionStance stance, HashMap<UInt128, ExportProof> masks) =>
        CrdtWire.Decode(entry.Payload).ToOption().Map(op => stance == RedactionStance.Mask && masks.Find(entry.ContentKey) is { IsSome: true, Case: ExportProof proof }
            ? (new CrdtOp.Set(op.Field, MaskBytes(proof), new Hlc(entry.Physical, entry.Logical), entry.OriginStoreId), true) : (op, false));

    static ReadOnlyMemory<byte> MaskBytes(ExportProof proof) { var span = new byte[16]; BinaryPrimitives.WriteUInt128LittleEndian(span, proof.ContentHash); return span; }

    static CrdtField Seed(CrdtOp op) => op.Switch(
        set: static _ => (CrdtField)new CrdtField.LwwRegister(default, Hlc.Zero, default),
        write: static _ => new CrdtField.MvRegister(Seq<(ReadOnlyMemory<byte>, VersionVector, Hlc)>()),
        add: static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
        remove: static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
        increment: static _ => new CrdtField.PnCounter(HashMap<Guid, long>(), HashMap<Guid, long>()),
        insertAfter: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        delete: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        maintain: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
        beat: static _ => new CrdtField.EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte>, Hlc)>()),
        leave: static _ => new CrdtField.EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte>, Hlc)>()));

    static UInt128 CellContent(CrdtField field) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        switch (field) {
            case CrdtField.LwwRegister reg: rolling.Append(reg.Value.Span); break;
            case CrdtField.MvRegister mv: foreach (var v in mv.Values.OrderBy(static v => (v.Cell.Physical, v.Cell.Logical))) rolling.Append(v.Value.Span); break;
            case CrdtField.OrSet set: foreach (var e in set.Live.Keys.OrderBy(static k => k)) { BinaryPrimitives.WriteUInt128LittleEndian(word, e); rolling.Append(word); } break;
            case CrdtField.PnCounter counter: BinaryPrimitives.WriteInt64LittleEndian(word, Crdt.Value(counter)); rolling.Append(word[..8]); break;
            case CrdtField.RgaSequence seq: foreach (var v in Crdt.Materialize(seq)) rolling.Append(v.Span); break;
            case CrdtField.EphemeralMap map: foreach (var (origin, state) in Crdt.Live(map).OrderBy(static s => s.Origin)) rolling.Append(state.Span); break;
        }
        return rolling.GetCurrentHashAsUInt128();
    }

    static UInt128 StateKey(HashMap<CellKey, CrdtField> state) {
        var rolling = new XxHash128();
        foreach (var slot in state.OrderBy(static s => s.Key.ToString(), StringComparer.Ordinal)) {
            Span<byte> cell = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128LittleEndian(cell, CellContent(slot.Value));
            rolling.Append(cell);
        }
        return rolling.GetCurrentHashAsUInt128();
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                   | [BINDING]                                                          |
| :-----: | :---------------- | :---------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | checkpoint anchor | Marten `AggregateSnapshot` + chained hash | `Anchor` seals, `Verify` re-folds `Prior`+`OpCount`              |
|  [02]   | as-of cut         | `TimeCut.Ceiling` precise/instant/version | the Marten fold binds the version or the ceiling instant         |
|  [03]   | cell key          | `(NodeId, CrdtOp.Field)`                  | the field the live merge keys on, never the column family        |
|  [04]   | one materializer  | `Crdt.Apply`                             | reconstruction equals the live state field-for-field            |
|  [05]   | bisect descent    | `O(log n)` probes of a MONOTONE predicate | each probe re-folds from the checkpoint anchor seed              |
|  [06]   | redaction stance  | `Fold` \| `Mask` \| `Reject`             | `ExportProof` mask on a crossed redacted op; `Reject` aborts     |
