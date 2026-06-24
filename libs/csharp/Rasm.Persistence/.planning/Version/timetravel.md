# [PERSISTENCE_TIME_TRAVEL]

Rasm.Persistence AS-OF time travel: one `TimeTravel` engine that reconstructs, diffs, blames, scrubs, bisects, checkpoints, and branches-from-past over the HLC op-log and the content-addressed snapshot spine. Every operation folds the `OpLogEntry` prefix up to a `TimeCut` against the nearest chained `Checkpoint`, replaying the prefix through the IDENTICAL `Crdt.Apply` the live merge uses so a historical materialization is bit-identical to the live state at that cut — there is no second materializer. The cut is a precise `Hlc` or an `Instant` (one value object, two modalities); the branch scope resolves through the commit-DAG `BranchRef` head the cut reconstructs to, never the op-log column family; reconstruction crossing a redacted op carries the `ExportProof` mask, never silently folding erased bytes. `ClockPolicy`, `CorrelationId`, `TenantContext`, and `ReceiptSinkPort` arrive settled from AppHost.

## [01]-[INDEX]

- [01]-[TIME_TRAVEL]: cut algebra, chained checkpoint anchor, AS-OF reconstruction, range diff, blame, scrub, bisect, and branch-from-past.

## [02]-[TIME_TRAVEL]

- Owner: `TimeCut` the `[ComplexValueObject]` AS-OF boundary carrying a precise `Hlc` ceiling and its `CutKind` modality (precise vs instant-derived); `AsOfQuery` the one reconstruction-request shape (cut, optional branch, entity-key prefix, redaction stance); `Checkpoint` a sealed materialized-state fold anchor that hash-chains the prior checkpoint's `Hash` so a checkpoint sequence is itself a verifiable content chain; `RangeDiff` the two-cut delta carrying per-key `(from, to)` content keys and a `ChangeKind`-classified change set; `BlameRow` per `(EntityKey, Field)` authorship attribution carrying the winning op AND its superseded contributor lineage; `ScrubFrame` one ordered replay step carrying the op, the `Hlc` at that step, and BOTH the pre-op `Before` and post-op `StateKey` content keys so a forward step reads the post-state and a rewind reads the pre-state; `ScrubWindow`/`ScrubReel` the closed-`Interval`-and-`SeekDirection` scrub bound (`Includes` is `[start, end]` to match the inclusive cut) and its `SeekDirection.Lay`-oriented frame reel with terminal state and a `StateAt` boundary read; `BisectOutcome` the first-flip locus of a MONOTONE history predicate; `TimeTravelReceipt` the typed AS-OF evidence; `TimeLog` the one read/closure/redaction port; `TimeTravel` the static surface owning the cut-bounded prefix fold, hash-chained checkpoint sealing and chain verification, range diff, blame fold, scrub iteration, bisection, and branch-from-past.
- Cases: `TimeCut` is `Precise(Hlc)` or `Of(Instant)` collapsed into one `[ComplexValueObject]` whose `Ceiling` is the inclusive HLC bound (`(at, ulong.MaxValue)` for an instant cut, the exact cell for a precise cut) so two ops at one `Instant` resolve by the logical tiebreak; `ChangeKind` is `Added | Removed | Replaced | Converged` over a diffed key — `Replaced` is an LWW/MV content-key flip, `Converged` is a set/counter/sequence merge that changed materialized value without a single-writer replacement; `RedactionStance` is `Fold | Mask | Reject` deciding how a redacted op crossing the cut resolves; `SeekDirection` is `Forward | Backward` over a scrub reel; reconstruction seeds each new `(EntityKey, Field)` cell with the EMPTY `CrdtField` arm the decoded op's data type demands (the engine-local `Seed` total switch over the `CrdtOp` family — the inverse of `Crdt.Apply`, which `Crdt` itself does not own) so a first `Add`/`Increment`/`InsertAfter`/`Write` is folded, never dropped against a wrong `LwwRegister` seed.
- Entry: `public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log)` folds the prefix forward from the nearest checkpoint through `Crdt.Apply` into the AS-OF materialized state and stamps the typed receipt; the bare materialized map is the pure `Materialize` core the effect wraps. `public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log)` reconstructs both cuts and content-key-differences the materialized maps. `public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log)` folds the winning-plus-superseded authorship per cell. `public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log)` materializes the ordered replay reel over an `Interval` and direction. `public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<HashMap<CellKey, CrdtField>, bool> holds, TimeLog log)` binary-searches the checkpoint-anchored branch-scoped prefix for the first cut where a MONOTONE state predicate flips, short-circuiting on a floor that already holds. `public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, BranchAcl acl, Guid origin, TimeLog log, Func<string, Guid, Seq<UInt128>, CommitMessage, IO<CommitNode>> mintBranchCommit)` forks a new branch whose head is the root commit the `mintBranchCommit` op-log seam mints over the reconstructed op-key set at the cut (the seam owns HLC stamping and the `branch`-op append; the engine owns the op-key reconstruction and the `BranchRef` projection). `public static IO<Checkpoint> Anchor(AsOfQuery query, TimeLog log)` reconstructs the AS-OF state at the cut and seals it against the nearest prior checkpoint into a fresh fold anchor; `public static bool Verify(Checkpoint checkpoint, Option<Checkpoint> prior, Seq<OpLogEntry> suffix)` re-folds the suffix content keys over the prior `Hash` and confirms the rolling address, the `Prior` back-link, and the cumulative `OpCount` all agree, making the chain's verifiability a real probe rather than a prose claim. The carrier is `IO<T>` package-wide; the modality (reconstruct, diff, blame, scrub, bisect, branch, anchor) is the entrypoint, the request shape is one `AsOfQuery`, and `TimeLog` is the one read/redaction/closure port the engine sources from.
- Auto: AS-OF reconstruction reads the op-log prefix bounded by `TimeCut.Ceiling` and folds it through the same `Crdt.Apply` the live path runs, seeding each absent cell with the empty `CrdtField` arm the op's decoded data type requires (the engine-local `Seed`), so a historical materialization equals the live state at that cut field-for-field; the nearest `Checkpoint` is the fold floor and its `State` the seed, so a deep history folds the suffix from the checkpoint rather than from genesis; a checkpoint seal hash-chains the prior checkpoint's `Hash` into a fresh `XxHash128` then appends each suffix op's `ContentKey` (the cross-runtime-reproducible `XxHash128` of the op's companion bytes the changefeed already minted) in `(Physical, Logical, OriginStoreId)` order so a checkpoint's `Hash` is the rolling content address of the whole prefix and the checkpoint chain (`Prior` back-link plus cumulative `OpCount`) verifies against the op-log without re-folding from genesis; range diff reconstructs both cuts and content-key-differences the materialized maps through the engine-local `CellContent` observable-value projection, classifying each changed key by its `CrdtField` arm into `Replaced` (LWW/MV single-writer flip) vs `Converged` (set/counter/sequence merge); blame groups the prefix by `(EntityKey, Field)`, selects the `(Hlc, origin)` winner the convergence selected, AND retains the superseded contributors so authorship carries the full causal lineage the `Version/provenance#CAUSAL_DAG` `WasAttributedTo` edge reconciles against; scrub seeds from the nearest `Checkpoint.State` at the floor and folds the windowed op prefix as `ScrubFrame` values each carrying BOTH the pre-op `Before` and the post-op `StateKey` (the `CellContent`-folded cumulative state) so a debugger steps forward reading `StateKey` and rewinds reading `Before` without re-indexing, and the closed `[start, end]` `ScrubWindow.Includes` admits the cut-ceiling op the inclusive `TimeCut.Admits` selects (the half-open `Interval.Contains` would drop it); bisect binary-searches the sorted op prefix for the first cut whose folded state flips a MONOTONE history predicate so "when did this entity first break" is `O(log n)` predicate probes rather than a frame-by-frame linear scan, each probe re-folding its candidate sub-prefix from the SAME checkpoint anchor seed `Materialize` uses (never from genesis), and `HeldAtFloor` tests the seed state itself so a predicate already true at the fold floor short-circuits to zero probes; branch scoping resolves the `BranchRef` head the cut reconstructs to (`TimeLog.HeadAt`) and restricts the fold to that head commit's op-key `Closure` so a branched AS-OF read sees exactly the branch's ancestry, never every op-log row; branch-from-past mints a root `CommitNode` over the reconstructed op-key set through the `mintBranchCommit` op-log seam (which stamps the HLC cell and appends the `branch` op) so a past state becomes a live branch head on the DAG; a redacted op crossing the cut resolves through the `AsOfQuery.Redaction` stance — `Fold` admits the pre-redaction content key, `Mask` force-sets the masked cell to an opaque `LwwRegister` of the `ExportProof` content-hash bytes regardless of the cell's CRDT arm, `Reject` aborts the reconstruction with a typed retention fault.
- Receipt: every reconstruction-shaped operation folds the typed `TimeTravelReceipt` carrying the cut, op count folded, checkpoint hit, redaction count, and elapsed `Duration`; a reconstruction rides `store.timetravel.asof`, a range diff `store.timetravel.diff`, a blame `store.timetravel.blame`, a scrub `store.timetravel.scrub`, a bisect `store.timetravel.bisect`, and a branch-from-past `store.branch`; a checkpoint seal (`Anchor`) carries its own evidence in the returned `Checkpoint` (`Hash`, `OpCount`, `Prior`) under `store.timetravel.checkpoint` and `Verify` returns the pure chain-validity probe, so neither re-stamps a `TimeTravelReceipt`.
- Packages: NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.IO.Hashing, BCL inbox.
- Growth: a new replay projection is one method on `TimeTravel`; a new attribution dimension is one field on `BlameRow`; a new change classification is one `ChangeKind` row; a new cut modality is one `TimeCut` case; a new redaction stance is one `RedactionStance` row; zero new surface — a temporal-table mirror, a second history store, a snapshot-per-instant materialization, or a parallel bisect walker is the deleted form because reconstruction folds the op-log prefix the changefeed already holds and pins the heavy cuts to the content-addressed snapshots (`Snapshots.ContentAddress`) as chained `Checkpoint` fold anchors.
- Boundary: reconstruction is a pure left-fold of the op-log prefix through `Crdt.Apply`, so the AS-OF state at any cut is reproducible from the changefeed and the `Checkpoint` anchors — a checkpoint splits two facts — its `State` is the branch-scoped materialization a same-query reconstruction seeds from, and its `Hash` is the BRANCH-AGNOSTIC running content address of the global crdt prefix above the floor, hash-chained off the prior checkpoint's `Hash` (a non-cryptographic content chain, never an authenticity claim — the `Version/provenance#ATTESTED_LEDGER` `AttestedEntry` owns tamper-evidence) — so a branched read never over-seeds from a global state while the chain stays reproducible from the one op stream a Python or TS replica folds, a deep history folds from the nearest checkpoint forward, and `Verify` re-folds the same global suffix content keys over the prior `Hash` and confirms the rolling address, the `Prior` back-link, and the cumulative `OpCount` agree — the chain is verifiable as a real probe, never re-folding from genesis; the empty-seed dispatch is load-bearing and engine-local (`Crdt` exposes `Merge`/`Apply`/`Compact` plus the `Value`/`Materialize`/`Live` projections but no seed inverse, so `Seed` is owned here) — seeding every cell as an `LwwRegister` silently drops a first `Add`/`Increment`/`InsertAfter`/`Write` against the `Crdt.Apply` `_ => state` default, so `Seed` reads the decoded op's data-type arm through the GENERATED total `CrdtOp.Switch` (a new `CrdtOp` case breaks the build at `Seed`, never a runtime-silent default) and the reconstruction is bit-identical to the live state rather than approximately so; the cell key is `(EntityKey, Field)` where `Field` is the `CrdtOp.Field` the live merge keys on, never the column family — conflating the column family with the field collapses distinct CRDT fields into one slot and is the deleted form; the branch scope is the commit-DAG `BranchRef` head the cut reconstructs to, resolved through `TimeLog.HeadAt` then restricted to that head's op-key `TimeLog.Closure`, never `OpLogEntry.ColumnFamily` (the column family names the CRDT data-type lane `crdt`/`commit`/`branch`/`presence`, not a branch) — a `ColumnFamily == branchName` filter is a phantom that never matches a real branch and is the deleted form; `RangeDiff` reconstructs both endpoints and content-key-differences the materialized maps so a range diff is two AS-OF folds, never a stored delta chain, and a `Replaced`/`Converged` distinction reads the `CrdtField` arm of the changed value rather than re-deriving a delta; `BlameRow` reads the same `(Hlc, origin)` winner the convergence selected and carries the superseded contributors so blame never disagrees with the materialized value and the authorship lineage reconciles with the `Version/provenance#CAUSAL_DAG` attribution; a materialized cell's content identity is the engine-local `CellContent(CrdtField)` — the canonical projection of the cell's observable value composing the owned `Crdt` projections (the `LwwRegister` value bytes, the `OrSet` live element keys, `Crdt.Value` over the `PnCounter`, `Crdt.Materialize` over the `RgaSequence` order, the `MvRegister` value-set, `Crdt.Live` over the `EphemeralMap`), an internal equality probe a diff and a state key fold over, NOT a MessagePack round-trip of the in-memory `[Union]` (which carries no wire contract); the checkpoint chain folds each op's `ContentKey` — the cross-runtime-reproducible content key `commits.md#CRDT_WIRE` mints over the op's `None`-companion bytes (never the LZ4 at-rest framing) — so the rolling checkpoint address is reproducible from the same op stream a Python or TS replica folds, without re-encoding a payload; scrub frames are read-only op projections carrying the pre-op `Before` and post-op `StateKey` (the rolling `XxHash128` over each cell's `CellContent`) seeded from the nearest `Checkpoint.State` so the windowed `StateKey` is the true cut state rather than an empty-seeded approximation, and a scrub never mutates the live state — `SeekDirection.Lay` reverses the reel for a rewind and `ScrubReel.StateAt` reads `Before` for a backward step versus `StateKey` for a forward one; `BisectOutcome` is the first-flip locus over the sorted op prefix so a regression search costs `O(log n)` predicate probes against a frame-by-frame linear scan, each probe re-folding its candidate sub-prefix FROM the checkpoint anchor seed (the load-bearing precondition is a MONOTONE `holds` — once a cut satisfies the property every later cut does, so the lower-bound search is sound; a non-monotone predicate is a caller contract breach), bisect folds the branch-scoped prefix under `RedactionStance.Fold` so the predicate reads real branch state, and `HeldAtFloor` probes the seed itself so an already-broken floor returns the absent locus with zero probes rather than a false first-op flip; branch-from-past consumes the `mintBranchCommit` op-log seam (which mints the root `CommitNode` over the reconstructed op-key set through `CommitGraph.Commit`, stamps the HLC cell off the op-log `ReceiptSinkPort`, and appends the `branch` op) so a forked head is a real commit node on the DAG, the reconstructed op-key set is the EXACT prefix `Reconstruct` folded (branch closure, prefix, and floor filters identical), and the commit cell rides the op-log HLC stamp, never `DateTime.UtcNow`.

```csharp signature
[SmartEnum]
public sealed partial class CutKind {
    public static readonly CutKind Precise = new();
    public static readonly CutKind Instant = new();
}

[ComplexValueObject]
public sealed partial class TimeCut {
    public Hlc Ceiling { get; }

    public CutKind Source { get; }

    public Instant At => Ceiling.Physical;

    public static TimeCut Precise(Hlc cell) => Create(cell, CutKind.Precise);

    public static TimeCut Of(Instant at) => Create(new Hlc(at, ulong.MaxValue), CutKind.Instant);

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
    public string EntityKey { get; }

    public string Field { get; }

    public override string ToString() => $"{EntityKey}:{Field}";
}

public readonly record struct AsOfQuery(
    TimeCut Cut,
    Option<string> Branch,
    Option<string> EntityKeyPrefix,
    RedactionStance Redaction) {
    public static AsOfQuery At(Instant cut) =>
        new(TimeCut.Of(cut), None, None, RedactionStance.Fold);

    public bool Selects(OpLogEntry entry, Hlc floor) =>
        entry.ColumnFamily == "crdt"
        && Cut.Admits(new Hlc(entry.Physical, entry.Logical))
        && new Hlc(entry.Physical, entry.Logical).CompareTo(floor) > 0
        && EntityKeyPrefix.Map(p => entry.EntityKey.StartsWith(p, StringComparison.Ordinal)).IfNone(true);
}

public readonly record struct Checkpoint(
    Hlc At, UInt128 Hash, HashMap<CellKey, CrdtField> State, long OpCount, Option<UInt128> Prior);

public readonly record struct KeyDelta(CellKey Key, ChangeKind Kind, Option<UInt128> From, Option<UInt128> To);

public readonly record struct RangeDiff(TimeCut From, TimeCut To, Seq<KeyDelta> Deltas) {
    public Seq<CellKey> Added => Deltas.Filter(static d => d.Kind == ChangeKind.Added).Map(static d => d.Key);
    public Seq<CellKey> Removed => Deltas.Filter(static d => d.Kind == ChangeKind.Removed).Map(static d => d.Key);
    public Seq<CellKey> Changed => Deltas.Filter(static d => d.Kind == ChangeKind.Replaced || d.Kind == ChangeKind.Converged).Map(static d => d.Key);
}

public readonly record struct BlameContributor(string Actor, Guid Origin, Hlc Cell, UInt128 ContentKey);

public readonly record struct BlameRow(
    CellKey Key, string Actor, Guid Origin, Hlc Cell, UInt128 ContentKey,
    int Contributors, Seq<BlameContributor> Superseded);

// `Before` is the cumulative state key the fold held BEFORE this step, `StateKey` the one it holds AFTER —
// so a forward step reads `StateKey` and a backward (rewind) step reads `Before` without re-indexing the reel.
public readonly record struct ScrubFrame(long Index, OpLogEntry Entry, Hlc At, UInt128 Before, UInt128 StateKey, bool Redacted);

public readonly record struct ScrubWindow(Interval Span, SeekDirection Direction) {
    public static ScrubWindow Forward(Interval span) => new(span, SeekDirection.Forward);

    public static ScrubWindow UpTo(TimeCut cut, SeekDirection direction) =>
        new(new Interval(Instant.MinValue, cut.At), direction);

    // Closed `[Start, End]` membership: `Interval.Contains` is half-open `[Start, End)`, so an op exactly at the cut
    // ceiling instant — admitted by the inclusive `TimeCut.Admits` — would drop from a window ending at the cut.
    public bool Includes(Instant at) =>
        (Span.HasStart is false || Span.Start <= at) && (Span.HasEnd is false || at <= Span.End);
}

public readonly record struct ScrubReel(Seq<ScrubFrame> Frames, HashMap<CellKey, CrdtField> Terminal, Interval Span) {
    // Chronological seek independent of the reel's display orientation (a backward reel lists frames descending):
    // the qualifying frame is the one with the smallest `At` at-or-after the target, not the first in list order.
    public Option<ScrubFrame> Seek(Hlc at) =>
        Frames.Filter(frame => frame.At.CompareTo(at) >= 0)
            .Fold(Option<ScrubFrame>.None, static (best, frame) =>
                Some(best.Filter(b => b.At.CompareTo(frame.At) <= 0).IfNone(frame)));

    // The state key the reel exposes at one cell-step boundary: the post-op `StateKey` for a forward read,
    // the pre-op `Before` for a rewind — so a debugger scrubbing either direction reads the boundary state directly.
    public Option<UInt128> StateAt(Hlc at, SeekDirection direction) =>
        Seek(at).Map(frame => direction.Rewind ? frame.Before : frame.StateKey);
}

public readonly record struct BisectOutcome(Option<ScrubFrame> FirstFlip, long Probes, bool HeldAtFloor);

public readonly record struct TimeTravelReceipt(
    string Slot, TimeCut Cut, long OpsFolded, bool CheckpointHit, int Redactions,
    Duration Elapsed, Instant At, CorrelationId Correlation);

public sealed record TimeLog(
    Func<Hlc, IO<Seq<OpLogEntry>>> UpTo,
    Func<AsOfQuery, IO<Option<Checkpoint>>> Nearest,
    Func<TimeCut, IO<Option<BranchRef>>> HeadAt,
    Func<BranchRef, IO<LanguageExt.HashSet<UInt128>>> Closure,
    Func<OpLogEntry, IO<Option<ExportProof>>> Redacted,
    CorrelationId Correlation,
    ClockPolicy Clocks);

public static class TimeTravel {
    public static IO<TimeTravelReceipt> Reconstruct(AsOfQuery query, TimeLog log) =>
        Folded(query, log).Map(static fold => fold.Receipt);

    public static HashMap<CellKey, CrdtField> Materialize(
        Option<Checkpoint> anchor, AsOfQuery query, Seq<OpLogEntry> prefix, LanguageExt.HashSet<UInt128> scope, HashMap<UInt128, ExportProof> masks) {
        var seed = anchor.Map(static c => c.State).IfNone(HashMap<CellKey, CrdtField>());
        var floor = anchor.Map(static c => c.At).IfNone(Hlc.Zero);
        return Within(prefix, query, scope, floor)
            .Fold(seed, (state, entry) => Decode(entry, query.Redaction, masks).Match(
                Some: step => Step(state, entry.EntityKey, step.Op, step.Masked),
                None: () => state));
    }

    public static Checkpoint Seal(Option<Checkpoint> prior, Hlc at, HashMap<CellKey, CrdtField> state, Seq<OpLogEntry> suffix) {
        var folded = toSeq(suffix.Filter(static e => e.ColumnFamily == "crdt").OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId)));
        return new Checkpoint(at, ChainHash(prior.Map(static p => p.Hash), folded), state,
            prior.Map(static p => p.OpCount).IfNone(0L) + folded.Count, prior.Map(static p => p.Hash));
    }

    // A checkpoint splits two facts. Its STATE is the seed a SAME-QUERY reconstruction expects — materialized under the
    // query's own branch scope (a branched checkpoint anchors that branch's ancestry, never a global state a branch read
    // would over-seed) and always under `Fold` (a checkpoint stores cleartext; redaction is a read stance, not a stored
    // mutation). Its hash CHAIN is the branch-AGNOSTIC rolling content address of the global crdt prefix above the floor
    // — the exact set `Verify` re-folds from a raw suffix (crdt-filtered, ordered, scope-free) — so the chain stays
    // reproducible from the one op stream a Python or TS replica folds while the seed stays branch-correct.
    public static IO<Checkpoint> Anchor(AsOfQuery query, TimeLog log) =>
        from prefix in Prefix(query with { Redaction = RedactionStance.Fold }, log)
        let floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)
        let above = prefix.Entries.Filter(e => new Hlc(e.Physical, e.Logical).CompareTo(floor) > 0)
        let state = Materialize(prefix.Anchor, query with { Redaction = RedactionStance.Fold }, prefix.Entries, prefix.Scope, HashMap<UInt128, ExportProof>())
        select Seal(prefix.Anchor, query.Cut.Ceiling, state, above);

    // Chain verification re-folds the suffix's content keys over the prior Hash and confirms the recomputed rolling
    // address, the Prior back-link, and the cumulative OpCount all agree — the Boundary's verifiability claim made real.
    public static bool Verify(Checkpoint checkpoint, Option<Checkpoint> prior, Seq<OpLogEntry> suffix) =>
        toSeq(suffix.Filter(static e => e.ColumnFamily == "crdt").OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId))) is var folded
        && checkpoint.Hash == ChainHash(prior.Map(static p => p.Hash), folded)
        && checkpoint.Prior == prior.Map(static p => p.Hash)
        && checkpoint.OpCount == prior.Map(static p => p.OpCount).IfNone(0L) + folded.Count;

    private static UInt128 ChainHash(Option<UInt128> prior, Seq<OpLogEntry> folded) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        if (prior.IsSome) {
            BinaryPrimitives.WriteUInt128LittleEndian(word, prior.ValueUnsafe());
            rolling.Append(word);
        }
        foreach (var entry in folded) {
            BinaryPrimitives.WriteUInt128LittleEndian(word, entry.ContentKey);
            rolling.Append(word);
        }
        return rolling.GetCurrentHashAsUInt128();
    }

    public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, TimeLog log) =>
        from a in MaterializedAt(from, log)
        from b in MaterializedAt(to, log)
        select new RangeDiff(from.Cut, to.Cut, Deltas(a, b));

    private static IO<HashMap<CellKey, CrdtField>> MaterializedAt(AsOfQuery query, TimeLog log) =>
        Prefix(query, log).Map(prefix => Materialize(prefix.Anchor, query, prefix.Entries, prefix.Scope, prefix.Masks));

    // Blame folds from genesis (Hlc.Zero), NOT the checkpoint floor: a checkpoint seals the converged WINNER per cell
    // and discards the superseded contributors, so authorship lineage is reconstructible only from every contributing op.
    public static IO<Seq<BlameRow>> Blame(AsOfQuery query, TimeLog log) =>
        Prefix(query, log).Map(prefix => toSeq(Within(prefix.Entries, query, prefix.Scope, Hlc.Zero)
            .Choose(entry => Decode(entry, RedactionStance.Fold, prefix.Masks).Map(step => (Key: new CellKey(entry.EntityKey, step.Op.Field), entry)))
            .GroupBy(static row => row.Key)
            .Select(static group => {
                var ordered = toSeq(group
                    .Select(static row => row.entry)
                    .OrderByDescending(static e => (e.Physical, e.Logical, e.OriginStoreId)));
                var winner = ordered.Head;
                return new BlameRow(
                    group.Key, winner.Actor, winner.OriginStoreId, new Hlc(winner.Physical, winner.Logical), winner.ContentKey,
                    ordered.Count,
                    ordered.Tail.Map(static e => new BlameContributor(e.Actor, e.OriginStoreId, new Hlc(e.Physical, e.Logical), e.ContentKey)));
            })));

    public static IO<ScrubReel> Scrub(AsOfQuery query, ScrubWindow window, TimeLog log) =>
        Prefix(query, log).Map(prefix => {
            var floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero);
            var seed = prefix.Anchor.Map(static c => c.State).IfNone(HashMap<CellKey, CrdtField>());
            var ordered = toSeq(Within(prefix.Entries, query, prefix.Scope, floor)
                .Filter(entry => window.Span.Includes(entry.Physical))
                .OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId)));
            var (frames, terminal) = ordered.Fold(
                (Frames: Seq<ScrubFrame>(), State: seed),
                (acc, entry) => Decode(entry, query.Redaction, prefix.Masks).Match(
                    Some: step => Step(acc.State, entry.EntityKey, step.Op, step.Masked) is var next
                        ? (acc.Frames.Add(new ScrubFrame(acc.Frames.Count, entry, new Hlc(entry.Physical, entry.Logical), StateKey(acc.State), StateKey(next), step.Masked)), next)
                        : acc,
                    None: () => (acc.Frames.Add(new ScrubFrame(acc.Frames.Count, entry, new Hlc(entry.Physical, entry.Logical), StateKey(acc.State), StateKey(acc.State), Redacted: true)), acc.State)));
            return new ScrubReel(window.Direction.Lay(frames), terminal, window.Span);
        });

    // `holds` is a MONOTONE history property — once the cut's state satisfies it, every later cut does too — so the
    // first-flip locus is a lower-bound binary search: O(log n) anchor-seeded sub-prefix re-folds, not a linear scan.
    // A non-monotone predicate is the caller's contract breach; bisect answers "the earliest cut that first holds".
    public static IO<BisectOutcome> Bisect(AsOfQuery bound, Func<HashMap<CellKey, CrdtField>, bool> holds, TimeLog log) =>
        from prefix in Prefix(bound with { Redaction = RedactionStance.Fold }, log)
        let floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)
        let seed = prefix.Anchor.Map(static c => c.State).IfNone(HashMap<CellKey, CrdtField>())
        let ops = toSeq(Within(prefix.Entries, bound, prefix.Scope, floor)
            .OrderBy(static e => (e.Physical, e.Logical, e.OriginStoreId)))
        select Descend(holds, seed, ops);

    private static BisectOutcome Descend(Func<HashMap<CellKey, CrdtField>, bool> holds, HashMap<CellKey, CrdtField> seed, Seq<OpLogEntry> ops) {
        (long Flip, long Probes) Search(long lo, long hi, long probes) =>
            lo >= hi ? (lo, probes)
            : (lo + ((hi - lo) >> 1)) is var mid && holds(Replay(seed, ops.Take((int)mid + 1)))
                ? Search(lo, mid, probes + 1)
                : Search(mid + 1, hi, probes + 1);
        var heldAtFloor = holds(seed);
        var (flip, probes) = heldAtFloor ? (0L, 0L) : Search(0L, ops.Count, 0L);
        return new BisectOutcome(
            !heldAtFloor && flip < ops.Count && ops[(int)flip] is var op
                ? Some(new ScrubFrame(
                    flip, op, new Hlc(op.Physical, op.Logical),
                    StateKey(Replay(seed, ops.Take((int)flip))), StateKey(Replay(seed, ops.Take((int)flip + 1))), Redacted: false))
                : None,
            probes,
            heldAtFloor);
    }

    public static IO<BranchRef> BranchFromPast(
        AsOfQuery query, string newBranch, BranchAcl acl, Guid origin, TimeLog log,
        Func<string, Guid, Seq<UInt128>, CommitMessage, IO<CommitNode>> mintBranchCommit) =>
        from prefix in Prefix(query, log)
        let floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)
        let opKeys = Within(prefix.Entries, query, prefix.Scope, floor).Map(static e => e.ContentKey)
        from commit in mintBranchCommit(newBranch, origin, opKeys, new CommitMessage("branch-from-past", string.Empty))
        select new BranchRef(
            newBranch, RefKind.Branch, commit.ContentKey, acl, origin, commit.Cell.Physical,
            None, None, CommitMessage.Empty, string.Empty);

    private static IO<(HashMap<CellKey, CrdtField> State, TimeTravelReceipt Receipt)> Folded(AsOfQuery query, TimeLog log) =>
        from mark in IO.lift(log.Clocks.Mark)
        from prefix in Prefix(query, log)
        let floor = prefix.Anchor.Map(static c => c.At).IfNone(Hlc.Zero)
        let folded = Within(prefix.Entries, query, prefix.Scope, floor).Count
        let state = Materialize(prefix.Anchor, query, prefix.Entries, prefix.Scope, prefix.Masks)
        select (state, new TimeTravelReceipt(
            "store.timetravel.asof", query.Cut, folded, prefix.Anchor.IsSome, prefix.Masks.Count,
            log.Clocks.Elapsed(mark), log.Clocks.Now, log.Correlation));

    private static IO<(Option<Checkpoint> Anchor, Seq<OpLogEntry> Entries, LanguageExt.HashSet<UInt128> Scope, HashMap<UInt128, ExportProof> Masks)> Prefix(AsOfQuery query, TimeLog log) =>
        from anchor in log.Nearest(query)
        from entries in log.UpTo(query.Cut.Ceiling)
        from scope in query.Branch.Match(
            Some: _ => log.HeadAt(query.Cut).Bind(head => head.Match(Some: log.Closure, None: () => IO.pure(LanguageExt.HashSet<UInt128>.Empty))),
            None: () => IO.pure(LanguageExt.HashSet<UInt128>.Empty))
        from masks in query.Redaction == RedactionStance.Fold
            ? IO.pure(HashMap<UInt128, ExportProof>())
            : entries.TraverseM(e => log.Redacted(e).Map(p => (e.ContentKey, Proof: p))).As()
                .Map(pairs => toHashMap(pairs.Choose(static row => row.Proof.Map(proof => (row.ContentKey, proof)))))
        from _ in query.Redaction == RedactionStance.Reject && !masks.IsEmpty
            ? IO.fail<Unit>(Error.New(8262, $"<timetravel-redaction-hold:{masks.Count}>"))
            : IO.pure(unit)
        select (anchor, entries, scope, masks);

    // The one selection predicate every fold shares: cut-admitted, above the checkpoint floor, inside the branch closure.
    private static Seq<OpLogEntry> Within(Seq<OpLogEntry> entries, AsOfQuery query, LanguageExt.HashSet<UInt128> scope, Hlc floor) =>
        entries.Filter(e => query.Selects(e, floor) && (query.Branch.IsNone || scope.Contains(e.ContentKey)));

    private static Seq<KeyDelta> Deltas(HashMap<CellKey, CrdtField> a, HashMap<CellKey, CrdtField> b) =>
        toSeq(b.Keys.Where(k => !a.ContainsKey(k)).Map(k => new KeyDelta(k, ChangeKind.Added, None, Some(CellContent(b[k])))))
            .Append(a.Keys.Where(k => !b.ContainsKey(k)).Map(k => new KeyDelta(k, ChangeKind.Removed, Some(CellContent(a[k])), None)))
            .Append(a.Keys.Where(b.ContainsKey).Choose(k => (CellContent(a[k]), CellContent(b[k])) is var (from, to) && from != to
                ? Some(new KeyDelta(k, Classify(b[k]), Some(from), Some(to)))
                : None));

    // Generated total Switch over CrdtField: a single-writer register flip is `Replaced`, a merge-converged
    // collection/counter/presence change is `Converged` — a NEW CrdtField arm must declare its classification here
    // rather than silently defaulting to `Converged`.
    private static ChangeKind Classify(CrdtField field) =>
        field.Switch(
            lwwRegister:  static _ => ChangeKind.Replaced,
            mvRegister:   static _ => ChangeKind.Replaced,
            orSet:        static _ => ChangeKind.Converged,
            pnCounter:    static _ => ChangeKind.Converged,
            rgaSequence:  static _ => ChangeKind.Converged,
            ephemeralMap: static _ => ChangeKind.Converged);

    // The bisect sub-prefix re-fold from the checkpoint anchor seed (the SAME floor `Materialize` seeds from): Bisect
    // forces RedactionStance.Fold, so every op folds raw (no mask), and the candidate ops are already branch-scoped.
    private static HashMap<CellKey, CrdtField> Replay(HashMap<CellKey, CrdtField> seed, Seq<OpLogEntry> prefix) =>
        prefix.Fold(seed, static (state, entry) => CrdtWire.Decode(entry.Payload).Match(
            Succ: op => Step(state, entry.EntityKey, op, masked: false),
            Fail: _ => state));

    // A masked op (always the Set Decode minted from the proof bytes) force-sets its cell to an opaque LwwRegister —
    // redaction destroys the prior CRDT structure, so routing the mask through Crdt.Apply would no-op against a
    // non-LWW cell and leak cleartext.
    private static HashMap<CellKey, CrdtField> Step(HashMap<CellKey, CrdtField> state, string entityKey, CrdtOp op, bool masked) =>
        (masked, op) switch {
            (true, CrdtOp.Set set) => state.AddOrUpdate(new CellKey(entityKey, set.Field), new CrdtField.LwwRegister(set.Value, set.Cell, set.Origin)),
            _ => state.AddOrUpdate(new CellKey(entityKey, op.Field), existing => Crdt.Apply(existing, op), Crdt.Apply(Seed(op), op)),
        };

    private static Option<(CrdtOp Op, bool Masked)> Decode(OpLogEntry entry, RedactionStance stance, HashMap<UInt128, ExportProof> masks) =>
        CrdtWire.Decode(entry.Payload).ToOption().Map(op =>
            stance == RedactionStance.Mask && masks.Find(entry.ContentKey) is { IsSome: true, Case: ExportProof proof }
                ? (new CrdtOp.Set(op.Field, MaskBytes(proof), new Hlc(entry.Physical, entry.Logical), entry.OriginStoreId), true)
                : (op, false));

    private static ReadOnlyMemory<byte> MaskBytes(ExportProof proof) {
        var span = new byte[16];
        BinaryPrimitives.WriteUInt128LittleEndian(span, proof.ContentHash);
        return span;
    }

    // The empty CrdtField arm the live Crdt.Apply total-switch demands for a first op: seeding a wrong arm
    // (always LwwRegister) silently drops a first Add/Increment/InsertAfter/Write against the `_ => state` default.
    // This rides the GENERATED total `CrdtOp.Switch` (not a `_`-defaulted language switch), so a NEW CrdtOp case
    // breaks the build here rather than silently seeding the EphemeralMap arm — Seed is the engine-local inverse of
    // Crdt.Apply, which Crdt itself does not own. `Maintain` is the one op valid on two arms (RgaSequence/EphemeralMap);
    // it seeds RgaSequence and a Compact over either empty seed is a no-op, so the choice never drops a first op.
    private static CrdtField Seed(CrdtOp op) =>
        op.Switch(
            set:         static _ => (CrdtField)new CrdtField.LwwRegister(default, Hlc.Zero, default),
            write:       static _ => new CrdtField.MvRegister(Seq<(ReadOnlyMemory<byte>, VersionVector, Hlc)>()),
            add:         static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
            remove:      static _ => new CrdtField.OrSet(HashMap<UInt128, Set<ElementId>>(), Set<ElementId>()),
            increment:   static _ => new CrdtField.PnCounter(HashMap<Guid, long>(), HashMap<Guid, long>()),
            insertAfter: static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
            delete:      static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
            maintain:    static _ => new CrdtField.RgaSequence(Seq<RgaCell>()),
            beat:        static _ => new CrdtField.EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte>, Hlc)>()),
            leave:       static _ => new CrdtField.EphemeralMap(HashMap<Guid, (ReadOnlyMemory<byte>, Hlc)>()));

    // The cell's content identity is the canonical projection of its OBSERVABLE value through the owned Crdt
    // projections (`Value`/`Materialize`/`Live`), never a MessagePack round-trip of the in-memory `[Union]`. This is a
    // measured span-append kernel (stackalloc/Append exemption); arm exhaustiveness is enforced upstream by the
    // generated `CrdtOp.Switch` in `Seed` — a new CrdtField arm requires a new CrdtOp arm that breaks the build there.
    private static UInt128 CellContent(CrdtField field) {
        var rolling = new XxHash128();
        Span<byte> word = stackalloc byte[16];
        switch (field) {
            case CrdtField.LwwRegister reg:
                rolling.Append(reg.Value.Span);
                break;
            case CrdtField.MvRegister mv:
                foreach (var value in mv.Values.OrderBy(static v => (v.Cell.Physical, v.Cell.Logical))) rolling.Append(value.Value.Span);
                break;
            case CrdtField.OrSet set:
                foreach (var element in set.Live.Keys.OrderBy(static k => k)) {
                    BinaryPrimitives.WriteUInt128LittleEndian(word, element);
                    rolling.Append(word);
                }
                break;
            case CrdtField.PnCounter counter:
                BinaryPrimitives.WriteInt64LittleEndian(word, Crdt.Value(counter));
                rolling.Append(word[..8]);
                break;
            case CrdtField.RgaSequence seq:
                foreach (var value in Crdt.Materialize(seq)) rolling.Append(value.Span);
                break;
            case CrdtField.EphemeralMap map:
                foreach (var (origin, state) in Crdt.Live(map).OrderBy(static slot => slot.Origin)) rolling.Append(state.Span);
                break;
        }
        return rolling.GetCurrentHashAsUInt128();
    }

    private static UInt128 StateKey(HashMap<CellKey, CrdtField> state) {
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
|  [01]   | checkpoint anchor | hash-chained rolling content address      | `Snapshots.ContentAddress`; `Anchor` seals, `Verify` re-folds `Prior`+`OpCount` |
|  [02]   | as-of cut ceiling | `TimeCut.Ceiling` precise or `MaxValue`   | every op at-or-before the cut folds; precise pins the logical cell |
|  [03]   | cell key          | `(EntityKey, CrdtOp.Field)`               | the field the live merge keys on, never the column family         |
|  [04]   | change classify   | `CrdtField` arm → `Replaced`/`Converged`  | two AS-OF folds; LWW/MV replace, set/counter/RGA converge          |
|  [05]   | cell identity     | engine-local `CellContent(CrdtField)`     | observable-value projection over `Crdt.Value`/`Materialize`/`Live`, never a `[Union]` MessagePack round-trip |
|  [06]   | checkpoint chain  | rolling fold of each op's `ContentKey`     | reproducible companion-byte content key; never LZ4 at-rest framing |
|  [07]   | branch scope      | `HeadAt` + `Closure` commit-DAG ancestry  | resolved head's op-key closure, never `ColumnFamily`              |
|  [08]   | redaction stance  | `Fold` \| `Mask` \| `Reject`              | `ExportProof` mask on a crossed redacted op; `Reject` aborts      |
|  [09]   | bisect descent    | `O(log n)` probes of a MONOTONE predicate | first-flip locus under `Fold`; each probe re-folds its sub-prefix FROM the checkpoint anchor seed; `HeldAtFloor` short-circuits |
|  [10]   | scrub window      | closed `[start, end]` `Includes`          | admits the cut-ceiling op the half-open `Interval.Contains` drops; `Before`/`StateKey` per frame for rewind |
