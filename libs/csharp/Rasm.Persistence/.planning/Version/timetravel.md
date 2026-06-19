# [PERSISTENCE_TIME_TRAVEL]

Rasm.Persistence AS-OF time travel: an engine reconstructing, diffing, blaming, scrubbing, checkpointing, and branching-from-past over the HLC op-log and the content-addressed snapshots. Every reconstruction folds the `OpLogEntry` stream up to an HLC bound against the nearest content-addressed checkpoint; `ClockPolicy`, `CorrelationId`, and `TenantContext` arrive from AppHost.

## [01]-[INDEX]

- [01]-[TIME_TRAVEL]: AS-OF reconstruction, checkpoint, range diff, blame, scrub, and branch-from-past.

## [02]-[TIME_TRAVEL]

- Owner: `AsOfQuery` reconstruction request; `Checkpoint` a sealed materialized-state fold anchor; `RangeDiff` two-instant delta record; `BlameRow` per-node authorship attribution; `ScrubFrame` a replay frame; `TimeTravel` the static surface owning AS-OF materialization, checkpoint sealing, range diff, blame fold, scrub iteration, and branch-from-past.
- Cases: `AsOfQuery` reconstructs at an `Instant` by folding every op whose HLC `Physical` precedes the cut; `Checkpoint` seals the materialized state content address at a commit boundary so a deep history folds from the nearest checkpoint; `RangeDiff` projects added/removed/changed entity keys between two cuts; `BlameRow` attributes the surviving value of one entity-key column-family to the winning `OpLogEntry`; `ScrubFrame` is one step in the ordered replay sequence.
- Entry: `public static IO<HashMap<string, CrdtField>> Reconstruct(AsOfQuery query, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<AsOfQuery, IO<Option<Checkpoint>>> nearest)` — `IO` carries the op-log read; the fold replays the prefix forward from the nearest checkpoint through `Crdt.Apply` into the as-of materialized state; `public static IO<BranchRef> BranchFromPast(AsOfQuery query, string newBranch, BranchAcl acl, Guid origin, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<CommitNode, IO<Unit>> appendCommit, ClockPolicy clocks)` forks a new branch whose head is the reconstructed commit at the cut.
- Auto: AS-OF reconstruction reads the op-log prefix up to the cut and folds it through the same `Crdt.Apply` the live path uses so a historical materialization is bit-identical to the live state at that instant — no second materializer; range diff reconstructs both cuts and set-differences the materialized maps by content-key inequality; blame folds the winning op per `(EntityKey, ColumnFamily)` by the `(Hlc, origin)` stamp the merge selected so authorship is the same ordering the convergence uses; scrub iterates the ordered op prefix as `ScrubFrame` values so a debugger steps the history; branch-from-past mints a `CommitNode` whose parents are the commit at the cut and appends one `branch` op so a past state becomes a live branch head; a checkpoint seals the materialized-state content address at a commit boundary so reconstruction folds from the nearest checkpoint forward rather than from genesis.
- Receipt: a reconstruction rides `store.timetravel.asof` carrying the op count folded, the checkpoint hit, and the cut instant; a branch-from-past rides `store.branch`; a checkpoint seal rides `store.timetravel.checkpoint`.
- Packages: NodaTime, LanguageExt.Core, System.IO.Hashing, BCL inbox.
- Growth: a new replay projection is one method on `TimeTravel`; a new attribution dimension is one column on `BlameRow`; zero new surface — a temporal-table mirror, a second history store, or a snapshot-per-instant materialization is the deleted form because reconstruction folds the op-log prefix the changefeed already holds and pins the heavy cuts to the content-addressed snapshots (`Snapshots.ContentAddress`) as `Checkpoint` fold anchors.
- Boundary: reconstruction is a pure left-fold of the op-log prefix through `Crdt.Apply`, so the AS-OF state at any instant is reproducible from the changefeed and the `Checkpoint` anchors — a checkpoint is a sealed snapshot whose `Hash` is the materialized-state content address at a commit boundary, so a deep history folds from the nearest checkpoint forward; `RangeDiff` reconstructs both endpoints and set-differences the materialized maps so a range diff is two AS-OF folds, never a stored delta chain, and a `Changed` key is one whose live `CrdtField` content key differs at the two cuts; `BlameRow` reads the same `(Hlc, origin)` winner the convergence selected so blame never disagrees with the materialized value; scrub frames are read-only op projections and a scrub never mutates the live state; branch-from-past consumes `CommitGraph.Commit` so a forked head is a real commit node on the DAG, and the cut instant rides `ClockPolicy`, never `DateTime.UtcNow`.

```csharp signature
public readonly record struct AsOfQuery(Instant Cut, Option<string> Branch, Option<string> EntityKeyPrefix) {
    public Hlc Ceiling => new(Cut, ulong.MaxValue);
}

public readonly record struct Checkpoint(Instant At, UInt128 Hash, HashMap<string, CrdtField> State, long OpCount);

public readonly record struct RangeDiff(
    Instant From, Instant To,
    ImmutableArray<string> Added, ImmutableArray<string> Removed, ImmutableArray<string> Changed);

public readonly record struct BlameRow(
    string EntityKey, string ColumnFamily, string Actor, Guid Origin, Hlc Cell, UInt128 ContentKey);

public readonly record struct ScrubFrame(long Index, OpLogEntry Entry, Hlc At);

public static class TimeTravel {
    public static IO<HashMap<string, CrdtField>> Reconstruct(
        AsOfQuery query, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<AsOfQuery, IO<Option<Checkpoint>>> nearest) =>
        from anchor in nearest(query)
        from entries in upTo(query.Ceiling)
        let seed = anchor.Map(static c => c.State).IfNone(HashMap<string, CrdtField>())
        let floor = anchor.Map(static c => new Hlc(c.At, 0UL)).IfNone(Hlc.Zero)
        select entries
            .Filter(entry => query.Branch.Map(b => entry.ColumnFamily == b).IfNone(true) && entry.Physical <= query.Cut && new Hlc(entry.Physical, entry.Logical).CompareTo(floor) > 0)
            .Filter(entry => query.EntityKeyPrefix.Map(p => entry.EntityKey.StartsWith(p, StringComparison.Ordinal)).IfNone(true))
            .Fold(seed, static (state, entry) =>
                state.AddOrUpdate(
                    $"{entry.EntityKey}:{entry.ColumnFamily}",
                    existing => Fold(existing, entry),
                    Fold(new CrdtField.LwwRegister(default, Hlc.Zero, Guid.Empty), entry)));

    public static Checkpoint Seal(Instant at, HashMap<string, CrdtField> state, long opCount) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var slot in state.OrderBy(static s => s.Key, StringComparer.Ordinal))
            buffer.Write(MessagePackSerializer.Serialize(slot.Value));
        return new Checkpoint(at, XxHash128.HashToUInt128(buffer.WrittenSpan), state, opCount);
    }

    public static IO<RangeDiff> Diff(AsOfQuery from, AsOfQuery to, Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<AsOfQuery, IO<Option<Checkpoint>>> nearest) =>
        from a in Reconstruct(from, upTo, nearest)
        from b in Reconstruct(to, upTo, nearest)
        select new RangeDiff(
            from.Cut, to.Cut,
            [.. b.Keys.Where(k => !a.ContainsKey(k))],
            [.. a.Keys.Where(k => !b.ContainsKey(k))],
            [.. b.Keys.Where(k => a.Find(k).Map(v => Identity(v) != Identity(b[k])).IfNone(false))]);

    public static Seq<BlameRow> Blame(Seq<OpLogEntry> entries) =>
        toSeq(entries
            .GroupBy(static entry => (entry.EntityKey, entry.ColumnFamily))
            .Select(static group => group
                .OrderByDescending(static entry => (entry.Physical, entry.Logical, entry.OriginStoreId))
                .First())
            .Select(static winner => new BlameRow(
                winner.EntityKey, winner.ColumnFamily, winner.Actor, winner.OriginStoreId,
                new Hlc(winner.Physical, winner.Logical), winner.ContentKey)));

    public static IO<Seq<ScrubFrame>> Scrub(AsOfQuery query, Func<Hlc, IO<Seq<OpLogEntry>>> upTo) =>
        upTo(query.Ceiling).Map(entries => toSeq(entries
            .OrderBy(static entry => (entry.Physical, entry.Logical, entry.OriginStoreId))
            .Select((entry, index) => new ScrubFrame(index, entry, new Hlc(entry.Physical, entry.Logical)))));

    public static IO<BranchRef> BranchFromPast(
        AsOfQuery query, string newBranch, BranchAcl acl, Guid origin,
        Func<Hlc, IO<Seq<OpLogEntry>>> upTo, Func<CommitNode, IO<Unit>> appendCommit, ClockPolicy clocks) =>
        from entries in upTo(query.Ceiling)
        let opKeys = toSeq(entries.Where(e => e.Physical <= query.Cut).Select(static e => e.ContentKey))
        let cell = new Hlc(clocks.Now, 0UL)
        let seed = new BranchRef(newBranch, RefKind.Branch, default, acl, origin, clocks.Now)
        let commit = CommitGraph.Commit(VersionVector.Empty, opKeys, seed, "branch-from-past", cell)
        from _ in appendCommit(commit)
        select seed with { Head = commit.ContentKey };

    private static UInt128 Identity(CrdtField field) =>
        XxHash128.HashToUInt128(MessagePackSerializer.Serialize(field).AsSpan());

    private static CrdtField Fold(CrdtField state, OpLogEntry entry) =>
        CrdtWire.Decode(entry.Payload).Match(
            Succ: op => Crdt.Apply(state, op),
            Fail: _ => Crdt.Apply(state, new CrdtOp.Set(entry.ColumnFamily, entry.Payload, new Hlc(entry.Physical, entry.Logical), entry.OriginStoreId)));
}
```

| [INDEX] | [POLICY]          | [VALUE]                            | [BINDING]                                   |
| :-----: | :---------------- | :--------------------------------- | :------------------------------------------ |
|  [01]   | checkpoint anchor | sealed snapshot content address    | `Snapshots.ContentAddress`; fold floor      |
|  [02]   | as-of cut ceiling | `(Cut, ulong.MaxValue)` HLC        | every op at-or-before the instant folds     |
|  [03]   | change-key diff   | `CrdtField` content-key inequality | two AS-OF folds, never a stored delta chain |

