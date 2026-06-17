# [PERSISTENCE_SYNC_COLLABORATION]

Rasm.Persistence owns local-first synchronization and collaboration: the op-log changefeed with HLC stamping and the logical-replication decode-fold that materializes a running pgoutput stream into op-log entries, the merge law that adjudicates LWW per column family and dispatches the `crdt` column family into the settled `Crdt.Apply` join-semilattice, the three-case `SyncTransport` axis widened by topology and direction fields, ephemeral presence, the dedicated lossy awareness lane, partial-replication working-set checkout, and blob transfer composing the settled `BlobRemote` contract under the ArtifactSync frame constants. AppHost vocabulary — the `ReceiptSinkPort` `Advance` algebra, `ClockPolicy`, `ScheduleEntry`, `DeadlineClass`, `DataClassification`, `CorrelationId` — arrives settled and composes inside the fences. Transport variance lands as case rows and two fields, so the merge law and the op-log shape stay transport-invariant and a new transport touches zero entity or query surface; the pgoutput decode is the one path a running PostgreSQL changefeed travels into the one op-log shape, never a second feed.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                          |
| :-----: | :---------------- | :------------------------------------------------------------------------------ |
|   [1]   | OPLOG_CHANGEFEED  | Op-log shape, HLC stamping, transfer manifests, tag-transition cursor mechanics |
|   [2]   | MERGE_LAW         | LWW adjudication, conflict receipts, idempotent apply fold, crdt dispatch       |
|   [3]   | TRANSPORT_AXIS    | Three transport cases widened by topology/direction; pgoutput decode-fold pump  |
|   [4]   | PRESENCE_AND_BLOB | Presence rows, awareness lane, working-set checkout, settled blob-contract framing |
|   [5]   | TS_PROJECTION     | MessagePack wire shapes for segments, conflicts, presence, crdt ops, rejection  |

## [2]-[OPLOG_CHANGEFEED]

- Owner: `SyncOpKind` `[SmartEnum<string>]` under the `SyncKeyPolicy` ordinal accessor; `OpLogEntry` — the one op-log record serving every synced entity kind; `SyncCursor` carrying both the HLC cell and the WAL LSN watermark; the `OpLog` stamp-and-diff surface.
- Cases: 3 kind rows — upsert, delete (tombstone), presence; entity-kind variance rides the `EntityKind` column, never a second table shape; before-image variance rides the `Image` column, never a second record.
- Entry: `public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ClockPolicy clocks, Func<(Instant Physical, ulong Logical), OpLogEntry> build)` — `IO` carries the HLC cell swap; the op-log stamp rides the same `Advance` algebra and the same `Hlc` atom as every receipt envelope.
- Auto: the SaveChanges changefeed hook and the bulk lane's self-emission append rows with store-assigned `Sequence` in the same transaction as the entity rows; peer processes fold `TagTransitions` over entries past their cursor on the replay `ScheduleEntry` row — applying those transitions to the cache surface is the L2 contribution row's consequence; the logical-replication pump folds the running pgoutput stream into the same op-log shape so a change committed by any writer reaches every peer over the one feed.
- Receipt: cursor position, queue depth, and the acknowledged LSN ride `SyncApplyReceipt`; appended-segment evidence rides `ReceiptSinkPort` kinds through the package wire context.
- Packages: Npgsql, NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new synced concern is one `SyncOpKind` row or one `EntityKind` value on the same op-log shape, zero new surface; per-entity-kind outbox tables are the deleted form.
- Boundary: op-log and entity rows commit in one transaction — a trigger-based second write path is the rejected form; tombstone rows carry the age-bound retention class and hide behind the sync-tombstone named query filter; `Closure` carries the graph's descendant content-key manifest so transfer is set-difference, never tree-walk; a multi-segment pull decodes through the settled `MessagePackStreamReader` segment reader one length-delimited frame at a time, so a large catch-up never buffers the whole transfer set contiguously; the before-image `Image` column carries the table's replica-identity tuple so the merge law has the held content key without a read-before-write round trip, and a missing-replica-identity update decodes as an empty image folded as a `Merged` adjudication.

```csharp signature
public sealed class SyncKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class SyncOpKind {
    public static readonly SyncOpKind Upsert = new("upsert", tombstone: false);
    public static readonly SyncOpKind Delete = new("delete", tombstone: true);
    public static readonly SyncOpKind Presence = new("presence", tombstone: false);

    public bool Tombstone { get; }
}

public sealed record OpLogEntry(
    long Sequence, string EntityKind, string EntityKey, string ColumnFamily,
    SyncOpKind Kind, SnapshotCodec Codec, ReadOnlyMemory<byte> Payload, ReadOnlyMemory<byte> Image,
    UInt128 ContentKey, Seq<UInt128> Closure,
    string Actor, Guid OriginStoreId, Instant Physical, ulong Logical);

public sealed record SyncCursor(Guid OriginStoreId, long Sequence, NpgsqlLogSequenceNumber Lsn, Instant Physical, ulong Logical) {
    public static readonly SyncCursor Genesis =
        new(Guid.Empty, 0L, NpgsqlLogSequenceNumber.Invalid, Instant.MinValue, 0UL);

    public ulong Stamp => ((ulong)Physical.ToUnixTimeMilliseconds() << 16) | (Logical & 0xFFFF);
}

public static class OpLog {
    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ClockPolicy clocks, Func<(Instant Physical, ulong Logical), OpLogEntry> build) =>
        IO.lift(() => clocks.Now)
            .Map(wall => sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, wall)))
            .Map(build);

    public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds) =>
        entry.Closure.Add(entry.ContentKey).Filter(key => !holds(key));

    public static Seq<string> TagTransitions(Seq<OpLogEntry> entries) =>
        entries.Map(static entry => entry.EntityKind).Distinct();

    public static async IAsyncEnumerable<OpLogEntry> Segments(Stream lane, Func<ReadOnlySequence<byte>, OpLogEntry> decode, [EnumeratorCancellation] CancellationToken token = default) {
        using var reader = new MessagePackStreamReader(lane, leaveOpen: true);
        while (await reader.ReadAsync(token).ConfigureAwait(false) is { } frame) {
            yield return decode(frame);
        }
    }
}
```

| [INDEX] | [POLICY]              | [VALUE]                        | [BINDING]                                                |
| :-----: | :-------------------- | :----------------------------- | :------------------------------------------------------- |
|   [1]   | cursor replay cadence | `Every` 15 s                   | one `ScheduleEntry` row per peer process, lease none     |
|   [2]   | tombstone retention   | age-bound class on delete rows | retention sweep rows fold over the op-log artifact class |
|   [3]   | LSN watermark         | `SyncCursor.Lsn`               | the idempotency key the pump acknowledges after apply    |

## [3]-[MERGE_LAW]

- Owner: `ConflictReceipt`, `ConflictOutcome` `[Union]`, `SyncApplyReceipt`, `SyncSession` — the one session capsule of policy values and delegate rows; the `SyncMerge` fold surface routing LWW per column family and the `crdt` column family into the settled `Crdt.Apply`.
- Cases: 4 outcome cases — LocalWin, RemoteWin, Merged, Rejected — every case carrying its `ConflictReceipt`; the convergent `crdt` family supersedes the scalar adjudication on its column family and converges by merge, never by discarding the loser.
- Entry: `public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming)` — `IO` carries commit effects; replay converges idempotently and the receipt proves applied, skipped, conflicted, merged, and pushed counts.
- Receipt: `ConflictReceipt` is the typed conflict evidence projected to the inspector surface — one projection record, no second rail; `SyncApplyReceipt` is the per-run apply evidence carrying queue depth, the advanced cursor, and the acknowledged LSN.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one policy value on the column-family axis feeding `Held` resolution, zero new surface; a fifth outcome case is the named defect; a new replicated data type is a `version-control#CRDT_ALGEBRA` `CrdtField` case dispatched by this fold, never a fifth scalar arm.
- Boundary: LWW per column family is the default — `Held` resolves the competing local entry per entity key and column family, content-key equality adjudicates LocalWin (idempotent replay), an absent competitor adjudicates Merged, and equal stamps with divergent content adjudicate Rejected; HLC ordering ties break on origin store id so adjudication is deterministic across peers; a SyncEngine service class is the rejected form — the fold and the dispatch rows own the engine; the `crdt` column family routes its `OpLogEntry.Payload` through `version-control#CRDT_ALGEBRA` `Crdt.Apply` so a concurrent edit converges by the join-semilattice least-upper-bound rather than a scalar last-writer-wins, the LWW `Adjudicate` surviving only as the `LwwRegister` arm — a cross-package wire-vocabulary amendment recorded as a ledger seam-split, never a parallel sync engine, and the same one IMMEDIATE apply transaction holds both the scalar register mutation and the crdt-field merge so crash recovery is re-delivery for both.

```csharp signature
public readonly record struct ConflictReceipt(
    string EntityKind, string EntityKey, string ColumnFamily,
    Instant HeldPhysical, ulong HeldLogical, string HeldActor,
    Instant IncomingPhysical, ulong IncomingLogical, string IncomingActor,
    CorrelationId Correlation, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConflictOutcome {
    private ConflictOutcome() { }

    public sealed record LocalWin(ConflictReceipt Receipt) : ConflictOutcome;

    public sealed record RemoteWin(ConflictReceipt Receipt) : ConflictOutcome;

    public sealed record Merged(ConflictReceipt Receipt) : ConflictOutcome;

    public sealed record Rejected(ConflictReceipt Receipt) : ConflictOutcome;
}

public readonly record struct SyncApplyReceipt(
    long Applied, long Skipped, long Conflicted, long Converged, long Pushed, long QueueDepth,
    Seq<ConflictReceipt> Conflicts, SyncCursor Cursor, CorrelationId Correlation, Instant At);

public sealed record SyncSession(
    ClockPolicy Clocks, ReceiptSinkPort Sink, CorrelationId Correlation,
    Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, CancellationToken Token,
    Func<UInt128, bool> Holds,
    Func<UInt128, IO<OpLogEntry>> Fetch,
    Func<OpLogEntry, Option<OpLogEntry>> Held,
    Func<OpLogEntry, IO<Unit>> Commit,
    Func<OpLogEntry, IO<Unit>> Converge,
    Func<SyncCursor, Seq<OpLogEntry>> Pending,
    Func<long> QueueDepth,
    Func<string, SyncCursor, IO<(ulong SchemaFingerprint, Seq<OpLogEntry> Entries, SyncCursor Cursor)>> Pull,
    Func<string, Seq<OpLogEntry>, IO<SyncCursor>> Push,
    Func<string, Seq<UInt128>, IO<Seq<UInt128>>> HasObjects,
    Func<string, CancellationToken, IO<Seq<OpLogEntry>>> Decode);

public static class SyncMerge {
    public static ConflictReceipt Receipt(SyncSession session, OpLogEntry held, OpLogEntry incoming) =>
        new(incoming.EntityKind, incoming.EntityKey, incoming.ColumnFamily,
            held.Physical, held.Logical, held.Actor,
            incoming.Physical, incoming.Logical, incoming.Actor,
            session.Correlation, session.Clocks.Now);

    public static ConflictOutcome Adjudicate(SyncSession session, OpLogEntry incoming) =>
        session.Held(incoming) is { IsSome: true, Case: OpLogEntry held }
            ? incoming.ContentKey == held.ContentKey
                ? new ConflictOutcome.LocalWin(Receipt(session, held, incoming))
                : (incoming.Physical, incoming.Logical, incoming.OriginStoreId)
                    .CompareTo((held.Physical, held.Logical, held.OriginStoreId)) switch {
                        > 0 => new ConflictOutcome.RemoteWin(Receipt(session, held, incoming)),
                        < 0 => new ConflictOutcome.LocalWin(Receipt(session, held, incoming)),
                        _ => new ConflictOutcome.Rejected(Receipt(session, held, incoming)),
                    }
            : new ConflictOutcome.Merged(Receipt(session, incoming, incoming));

    public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming) =>
        incoming.Fold(
            IO.pure((Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Conflicts: Seq<ConflictReceipt>())),
            (acc, entry) => acc.Bind(counts => entry.ColumnFamily == "crdt"
                ? session.Converge(entry).Map(_ => counts with { Converged = counts.Converged + 1L })
                : Adjudicate(session, entry).Switch(
                    state: (Session: session, Entry: entry, Counts: counts),
                    localWin: static (s, _) => IO.pure(s.Counts with { Skipped = s.Counts.Skipped + 1L }),
                    remoteWin: static (s, _) => s.Session.Commit(s.Entry).Map(_ => s.Counts with { Applied = s.Counts.Applied + 1L }),
                    merged: static (s, _) => s.Session.Commit(s.Entry).Map(_ => s.Counts with { Applied = s.Counts.Applied + 1L }),
                    rejected: static (s, outcome) => IO.pure(s.Counts with {
                        Conflicted = s.Counts.Conflicted + 1L,
                        Conflicts = s.Counts.Conflicts.Add(outcome.Receipt),
                    }))))
            .Map(counts => new SyncApplyReceipt(
                counts.Applied, counts.Skipped, counts.Conflicted, counts.Converged, Pushed: 0L,
                session.QueueDepth(), counts.Conflicts, session.Cursor, session.Correlation, session.Clocks.Now));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                | [BINDING]                                             |
| :-----: | :-------------------- | :------------------------------------- | :---------------------------------------------------- |
|   [1]   | scalar default        | LWW by `(Physical, Logical, OriginStoreId)` | total order on the stamp tuple, deterministic across peers |
|   [2]   | crdt column family    | `version-control#CRDT_ALGEBRA` `Crdt.Apply` | join-semilattice merge, superset of `Adjudicate` |
|   [3]   | conservation identity | `batch = applied + skipped + conflicted + converged` | a breach is a typed merge fault on the receipt stream |

## [4]-[TRANSPORT_AXIS]

- Owner: `SyncTopology` and `SyncDirection` keyless vocabularies; `SyncTransport` `[Union]`; the `SyncPump` dispatch surface with the `CopyGraph` transport bridge; the `ReplicationPump` decode-fold owning the running pgoutput message family; `GraphDiff` is the named set-difference diff-algebra `CopyGraph` and `SubtreeFetch` both dial — the closure-manifest set-difference over `Holds` membership that yields the missing content-key set; `SubtreeFetch` is the partial-graph checkout route fetching only the descendants of a chosen root rather than the whole graph.
- Cases: 3 transport cases — PgLogicalReplication, HttpDelta, SpeckleLikeDiff — widened by the `Topology` and `Direction` fields; fan-in, fan-out, and capture direction are field values, never new cases; `GraphDiff` carries the two diff operands — the source closure manifest and the target `Holds` predicate — never a new transport case.
- Entry: `public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport)` — one total state-threaded dispatch; `public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds)` is the set-difference diff-algebra projecting the missing-key set; `public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root)` is the partial-graph checkout over the diff; `public static async Task<Fin<SyncApplyReceipt>> Pump(LogicalReplicationConnection link, SyncTransport.PgLogicalReplication row, SyncSession session, Func<CommitBatch, IO<SyncApplyReceipt>> apply, CancellationToken token)` is the decode-fold that drains the running pgoutput stream into commit-ordered op-log batches and acknowledges the applied LSN.
- Auto: `Options` fixes the non-obsolete pgoutput spelling — protocol V4, parallel streaming, binary wire; `Open` is the slot-attach stream with `StartReplication` taking a non-optional cancellation token; `CreatePgOutputReplicationSlot` creates the slot and acknowledgement rides `SetReplicationStatus` with the applied-and-flushed LSN after each committed apply, flushed by `SendStatusUpdate` or the 10-second feedback cadence; `RelationMessage` re-arrives on schema change as an in-stream schema fact, streamed in-progress transactions stage keyed by xid and release on `StreamCommitMessage`, abort drops the stage, and commit order — not arrival order — is the op-log order.
- Receipt: every pump run yields one `SyncApplyReceipt` carrying the acknowledged LSN; slot lifecycle and relation-schema evidence ride `ReceiptSinkPort` kinds.
- Packages: Npgsql, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new transport is one case row plus one dispatch arm, zero new surface; future PowerSync lands as one case behind its admission gate; future cr-sqlite lands as native-sqlite extension rows plus merge-law rows, never a case here; a new graph-checkout shape is one entry over `GraphDiff`, never a second diff algebra; a new pgoutput leaf decodes as one `Filed` arm on the same message switch.
- Boundary: HttpDelta rides the AppHost `OutboundHop` http-api keyed pipeline — retry, backoff, and hop deadlines are owned there and the database stays excluded from the hop law; the document-granular fallback is the RFC 6902 patch payload, subordinate to the op-log changefeed, and RFC 7386 merge-patch is the rejected form; SSE and WebSocket transports are rejected — server-stream verbs own streaming; the obsolete ulong-protocol `PgOutputReplicationOptions` constructor is the rejected spelling, and `TestDecodingOptions` is the named rejected alternative output plugin — its human-readable text frames carry no typed message family, so pgoutput's binary `PgOutputReplicationMessage` leaves stay the only decode path; `Decode` folds the pgoutput message family — `InsertMessage`, the `DefaultUpdateMessage`/`FullUpdateMessage`/`IndexUpdateMessage` update leaves, the `KeyDeleteMessage`/`FullDeleteMessage` delete leaves, `TruncateMessage`, the `RelationMessage` schema fact, and the stream control frames — over `RelationMessage` schema context into op-log entries behind the session delegate, the `ReplicationTuple` drained one value at a time so a large row never buffers contiguously, and the unchanged-TOAST third state decodes as a carried-image cell, never null; a transport bridge is two sessions composed through `CopyGraph` — transport state and op-log mechanics live here, the wire leg is the settled proto vocabulary; `GraphDiff` is the one set-difference diff-algebra — the closure-manifest minus the target `Holds` set yields the missing content keys — and both `CopyGraph` (whole-graph bridge from the root) and `SubtreeFetch` (partial-graph checkout of a chosen sub-root's descendants) dial it, so a second walk-and-diff implementation is the deleted form and `OpLog.TransferSet` is the leaf the algebra composes; `SubtreeFetch` fetches only the chosen sub-root entry and traverses its `Closure` manifest, so a partial checkout never pulls the full graph and a client that holds a parent reuses its held keys through the same diff; the wire leg of `GraphDiff`/`SubtreeFetch` is owned at `Compute/remote-lane#PROTO_VOCABULARY` as the `GraphDiff`/`SubtreeFetch` message family — the walk-and-diff computation lives here and the wire shape lives there, so the remote-lane rpc dials this algebra over the transport and never re-implements the set-difference; the acknowledgement contract is at-least-once — apply, then `SetReplicationStatus` with only the durably-applied LSN, so redelivery after crash is the normal path and a lagging acknowledger is server-disk liability the provisioning verification fold gauges.

```csharp signature
[SmartEnum]
public sealed partial class SyncTopology {
    public static readonly SyncTopology FanIn = new();
    public static readonly SyncTopology FanOut = new();
    public static readonly SyncTopology Bidirectional = new();
}

[SmartEnum]
public sealed partial class SyncDirection {
    public static readonly SyncDirection Pull = new();
    public static readonly SyncDirection Push = new();
    public static readonly SyncDirection Bidi = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SyncTransport {
    private SyncTransport(SyncTopology topology, SyncDirection direction) {
        Topology = topology;
        Direction = direction;
    }

    public SyncTopology Topology { get; }

    public SyncDirection Direction { get; }

    public sealed record PgLogicalReplication(string Publication, string Slot, bool PublishGeneratedColumns, Duration IdleSlotTimeout, SyncTopology Topology, SyncDirection Direction) : SyncTransport(Topology, Direction);

    public sealed record HttpDelta(string Peer, SyncTopology Topology, SyncDirection Direction) : SyncTransport(Topology, Direction);

    public sealed record SpeckleLikeDiff(string Peer, SyncTopology Topology, SyncDirection Direction) : SyncTransport(Topology, Direction);
}

public readonly record struct CommitBatch(NpgsqlLogSequenceNumber Cursor, Seq<OpLogEntry> Entries);

public static class SyncPump {
    public static readonly Duration StatusInterval = Duration.FromSeconds(10);

    public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport) =>
        transport.Switch(
            state: session,
            pgLogicalReplication: static (s, row) => s.Decode(row.Slot, s.Token).Bind(entries => SyncMerge.Apply(s, entries)),
            httpDelta: static (s, row) => Exchange(s, row),
            speckleLikeDiff: static (s, row) => Offer(s, row));

    public static PgOutputReplicationOptions Options(SyncTransport.PgLogicalReplication row) =>
        new(row.Publication, PgOutputProtocolVersion.V4, binary: true, streamingMode: PgOutputStreamingMode.Parallel, messages: false, twoPhase: false);

    public static IAsyncEnumerable<PgOutputReplicationMessage> Open(LogicalReplicationConnection connection, SyncTransport.PgLogicalReplication row, CancellationToken token) =>
        connection.StartReplication(new PgOutputReplicationSlot(row.Slot), Options(row), token);

    public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds) =>
        OpLog.TransferSet(root, holds);

    public static IO<SyncApplyReceipt> CopyGraph(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry =>
            GraphDiff(entry, target.Holds)
                .TraverseM(source.Fetch).As()
                .Bind(missing => SyncMerge.Apply(target, missing)));

    public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry =>
            GraphDiff(entry, target.Holds)
                .TraverseM(source.Fetch).As()
                .Bind(missing => SyncMerge.Apply(target, missing)));

    private static IO<SyncApplyReceipt> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        s.Pull(row.Peer, s.Cursor).Bind(segment =>
            segment.SchemaFingerprint == s.SchemaFingerprint
                ? SyncMerge.Apply(s, segment.Entries).Bind(receipt =>
                    row.Direction == SyncDirection.Pull
                        ? IO.pure(receipt with { Cursor = segment.Cursor })
                        : IO.pure(s.Pending(segment.Cursor)).Bind(pending =>
                            s.Push(row.Peer, pending).Map(cursor =>
                                receipt with { Pushed = pending.Count, Cursor = cursor })))
                : IO.fail<SyncApplyReceipt>(Error.New($"<fingerprint-mismatch:{segment.SchemaFingerprint}>")));

    private static IO<SyncApplyReceipt> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        IO.pure(s.Pending(s.Cursor)).Bind(pending =>
            s.HasObjects(row.Peer, pending.Bind(static entry => entry.Closure.Add(entry.ContentKey)).Distinct())
                .Map(held => pending.Filter(entry => !held.Contains(entry.ContentKey)))
                .Bind(missing => s.Push(row.Peer, missing)
                    .Map(cursor => new SyncApplyReceipt(
                        Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Pushed: missing.Count,
                        QueueDepth: s.QueueDepth(), Conflicts: Seq<ConflictReceipt>(),
                        Cursor: cursor, Correlation: s.Correlation, At: s.Clocks.Now))));
}

public static class ReplicationPump {
    public static async Task<Fin<SyncApplyReceipt>> Pump(
        LogicalReplicationConnection link, SyncTransport.PgLogicalReplication row, SyncSession session,
        Func<CommitBatch, IO<SyncApplyReceipt>> apply, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(session);
        var (staged, open) = (HashMap<uint, Seq<OpLogEntry>>(), Seq<OpLogEntry>());
        var receipt = new SyncApplyReceipt(0L, 0L, 0L, 0L, 0L, session.QueueDepth(), Seq<ConflictReceipt>(), session.Cursor, session.Correlation, session.Clocks.Now);
        try {
            await foreach (var message in SyncPump.Open(link, row, token).ConfigureAwait(false)) {
                switch (message) {
                    case InsertMessage insert:
                        (staged, open) = Filed(staged, open, insert.TransactionXid,
                            await Entry(session, insert.Relation, SyncOpKind.Upsert, insert.NewRow, image: null, token).ConfigureAwait(false)); break;
                    case FullUpdateMessage full:
                        (staged, open) = Filed(staged, open, full.TransactionXid,
                            await Entry(session, full.Relation, SyncOpKind.Upsert, full.NewRow, full.OldRow, token).ConfigureAwait(false)); break;
                    case IndexUpdateMessage keyed:
                        (staged, open) = Filed(staged, open, keyed.TransactionXid,
                            await Entry(session, keyed.Relation, SyncOpKind.Upsert, keyed.NewRow, keyed.Key, token).ConfigureAwait(false)); break;
                    case UpdateMessage update:
                        (staged, open) = Filed(staged, open, update.TransactionXid,
                            await Entry(session, update.Relation, SyncOpKind.Upsert, update.NewRow, image: null, token).ConfigureAwait(false)); break;
                    case KeyDeleteMessage sparse:
                        (staged, open) = Filed(staged, open, sparse.TransactionXid,
                            await Entry(session, sparse.Relation, SyncOpKind.Delete, sparse.Key, image: null, token).ConfigureAwait(false)); break;
                    case FullDeleteMessage dense:
                        (staged, open) = Filed(staged, open, dense.TransactionXid,
                            await Entry(session, dense.Relation, SyncOpKind.Delete, dense.OldRow, image: null, token).ConfigureAwait(false)); break;
                    case StreamCommitMessage streamed:
                        receipt = await apply(new CommitBatch(streamed.TransactionEndLsn, staged.Find(streamed.TransactionXid).IfNone([]))).Run(EnvIO.New(token: token)).ConfigureAwait(false);
                        staged = staged.Remove(streamed.TransactionXid);
                        link.SetReplicationStatus(streamed.TransactionEndLsn);
                        await link.SendStatusUpdate(token).ConfigureAwait(false); break;
                    case StreamAbortMessage aborted:
                        staged = staged.Remove(aborted.TransactionXid); break;
                    case CommitMessage committed:
                        receipt = await apply(new CommitBatch(committed.TransactionEndLsn, open)).Run(EnvIO.New(token: token)).ConfigureAwait(false);
                        open = [];
                        link.SetReplicationStatus(committed.TransactionEndLsn);
                        await link.SendStatusUpdate(token).ConfigureAwait(false); break;
                }
            }
            return Fin.Succ(receipt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            return Fin.Fail<SyncApplyReceipt>(Error.New(8251, $"<replication-faulted:{row.Slot}:{ex.Message}>"));
        }
    }

    private static (HashMap<uint, Seq<OpLogEntry>> Staged, Seq<OpLogEntry> Open) Filed(
        HashMap<uint, Seq<OpLogEntry>> staged, Seq<OpLogEntry> open, uint? xid, OpLogEntry entry) =>
        xid is uint id ? (staged.AddOrUpdate(id, held => held.Add(entry), Seq(entry)), open) : (staged, open.Add(entry));

    private static async ValueTask<OpLogEntry> Entry(
        SyncSession session, RelationMessage relation, SyncOpKind kind, ReplicationTuple tuple, ReplicationTuple? image, CancellationToken token) {
        var payload = await Drained(tuple, token).ConfigureAwait(false);
        var imageBytes = image is null ? ReadOnlyMemory<byte>.Empty : await Drained(image, token).ConfigureAwait(false);
        return new OpLogEntry(
            Sequence: 0L, EntityKind: $"{relation.Namespace}.{relation.RelationName}", EntityKey: relation.RelationName,
            ColumnFamily: relation.ReplicaIdentity.ToString(), Kind: kind, Codec: SnapshotCodec.Binary,
            Payload: payload, Image: imageBytes, ContentKey: XxHash128.HashToUInt128(payload.Span),
            Closure: Seq<UInt128>(), Actor: session.StoreId.ToString(), OriginStoreId: session.StoreId,
            Physical: session.Clocks.Now, Logical: 0UL);
    }

    private static async ValueTask<ReadOnlyMemory<byte>> Drained(ReplicationTuple tuple, CancellationToken token) {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        await foreach (var value in tuple.ConfigureAwait(false)) {
            switch (value) {
                case { IsDBNull: true }: writer.WriteNil(); break;
                case { IsUnchangedToastedValue: true }: writer.Write("<carried>"); break;
                default: writer.Write(await value.Get<string>(token).ConfigureAwait(false)); break;
            }
        }
        writer.Flush();
        return buffer.WrittenMemory;
    }
}
```

| [INDEX] | [POLICY]                        | [VALUE]                                          | [BINDING]                                              |
| :-----: | :------------------------------ | :----------------------------------------------- | :----------------------------------------------------- |
|   [1]   | generated-column replication    | `PublishGeneratedColumns` on STORED columns only | pairs the STORED generated-column schema law           |
|   [2]   | idle slot reclamation           | config-sourced `IdleSlotTimeout`                 | server setting verified by the provisioning probe rows |
|   [3]   | replication conflict statistics | subscription-stats read view                     | the live-replication research gate carries the columns |
|   [4]   | session admission               | schema-fingerprint equality                      | mismatch is the typed `SyncRejectionWire` refusal      |
|   [5]   | LSN acknowledgement cadence     | per-commit `SetReplicationStatus` + 10 s feedback | at-least-once apply-then-ack; redelivery is normal     |
|   [6]   | graph checkout granularity      | `GraphDiff` set-difference over `Holds`          | whole-graph `CopyGraph` and partial `SubtreeFetch` dial one algebra; wire family at `Compute/remote-lane#PROTO_VOCABULARY` |

## [5]-[PRESENCE_AND_BLOB]

- Owner: `PresenceRow` and the `Presence` surface — ephemeral collaboration rows on the op-log shape; `AwarenessBeat` and the `Awareness` surface — the dedicated low-latency lossy awareness channel carrying cursor, selection, camera-frustum, active-node-focus, and follow-mode beats off the durable op-log; `WorkingSet` and the `Replication` surface — the partial-replication subgraph-checkout and working-set op-stream subscription; blob transfer composes the settled `BlobRemote` contract record, never a second blob shape.
- Entry: `public static IO<OpLogEntry> Beat(ReceiptSinkPort sink, ClockPolicy clocks, Guid storeId, SnapshotCodec codec, PresenceRow row, ReadOnlyMemory<byte> payload)` — presence is one changefeed row, never a transport; `public static Channel<AwarenessBeat> AwarenessLane()` is the dedicated lossy fan-out channel and `public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch, SyncCursor cursor, ClockPolicy clocks)` materializes a subgraph working set.
- Auto: presence rows expire at stamp plus `Ttl` and sweep on the heartbeat `ScheduleEntry` cadence; the offline queue is op-log accumulation draining on reconnect, with queue depth on every apply receipt; the awareness channel is a separate lossy lane from the durable op-log — cursor moves, selection halos, and camera frusta beat at a high cadence through the DropOldest channel and never touch the durable store, so a 60-Hz cursor stream never appends an op-log row, while `AwarenessKind` discriminates cursor, selection, camera, focus, and follow beats so one lossy lane carries every awareness signal; the working-set checkout resolves a `ReplicationQuery` (region/layer/view/type/closure-depth) into a content-key set then fetches only those entries, so a peer materializes one subgraph rather than the whole graph and subscribes its working-set op-stream to receive only changes touching its checked-out keys.
- Receipt: `Put` returns the stored `BlobRemote.Descriptor` as write evidence; sweep counts ride `ReceiptSinkPort` kinds; an awareness beat carries no receipt (lossy); a checkout rides `store.replication.checkout` carrying the resolved key count and the closure depth.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new presence attribute is one `PresenceRow` field on the same wire shape; a new awareness signal is one `AwarenessKind` row on the same lossy lane; a new offline-queue bound is one policy value; a new frame size is one `FrameBytes` policy value; a new checkout dimension is one column on `ReplicationQuery`; zero new surface — a durable cursor table, a per-signal awareness channel, or an eager full-graph replication is the deleted form.
- Boundary: blob streams frame at 64 KiB with Crc32 per frame and XxHash128 whole-artifact identity — the settled ArtifactSync frame constants, and a second framing law is the rejected form; the whole-artifact identity accumulates incrementally — `FrameHash` feeds each 64 KiB frame into one `XxHash128` instance through `Append` and reads the final digest through `GetCurrentHash`, so a multi-gigabyte blob never materializes contiguously and `XxHash128.HashToUInt128` over a whole span is the deleted pattern for streamed payloads; every `BlobRemote.Descriptor` carries classification and retention columns, so an unclassified blob write is a typed rejection; presence (a durable-TTL editing-surface claim) rides the DropOldest lane and is never durable beyond its TTL, while awareness (cursor/selection/camera/focus/follow) rides a SEPARATE lossy channel that never appends a durable op-log row — so a high-cadence cursor stream never pressures the durable store and an awareness beat lost under backpressure is correct-by-design (the next beat supersedes it), distinct from a durable op that must converge; the `AwarenessKind` axis carries the five collaboration signals on one lossy lane so a per-signal channel is the deleted form, and follow-mode is one peer subscribing another's camera beats; the working-set checkout is the partial-replication query-shape algebra — `ReplicationQuery` selects by region (spatial envelope), layer, view, type, and closure-depth so a peer checks out a subgraph rather than the whole graph, and the working-set manager subscribes the op-stream filtered to the checked-out keys so a peer receives only changes touching its working set, an eager full-graph replication or an unfiltered op-stream being the deleted form; the checkout rides the settled `SyncPump.SubtreeFetch`/`GraphDiff` set-difference so it fetches only the keys the peer lacks within the closure depth, and the `ReplicationQuery` lowers to a `federation#ELEMENT_SET_ALGEBRA` `SetExpr` so the checkout selection is the one element-set currency, never a second query shape.

```csharp signature
public sealed record PresenceRow(string Actor, string EntityKind, string EntityKey, Instant ExpiresAt);

public static class Presence {
    public static readonly Duration Ttl = Duration.FromSeconds(45);

    public const int FrameBytes = 65536;

    public static IO<(UInt128 Identity, Seq<uint> FrameChecks)> FrameHash(Stream source) =>
        IO.lift(() => {
            var identity = new XxHash128();
            var checks = Seq<uint>();
            var frame = new byte[FrameBytes];
            for (var read = source.Read(frame); read > 0; read = source.Read(frame)) {
                identity.Append(frame.AsSpan(0, read));
                checks = checks.Add(Crc32.HashToUInt32(frame.AsSpan(0, read)));
            }
            return (BinaryPrimitives.ReadUInt128LittleEndian(identity.GetCurrentHash()), checks);
        });

    public static Channel<OpLogEntry> Lane() =>
        Channel.CreateBounded<OpLogEntry>(new BoundedChannelOptions(64) {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });

    public static IO<OpLogEntry> Beat(ReceiptSinkPort sink, ClockPolicy clocks, Guid storeId, SnapshotCodec codec, PresenceRow row, ReadOnlyMemory<byte> payload) =>
        OpLog.Stamp(sink, clocks, cell => new OpLogEntry(
            Sequence: 0L,
            EntityKind: row.EntityKind,
            EntityKey: row.EntityKey,
            ColumnFamily: "presence",
            Kind: SyncOpKind.Presence,
            Codec: codec,
            Payload: payload,
            Image: ReadOnlyMemory<byte>.Empty,
            ContentKey: XxHash128.HashToUInt128(payload.Span),
            Closure: Seq<UInt128>(),
            Actor: row.Actor,
            OriginStoreId: storeId,
            Physical: cell.Physical,
            Logical: cell.Logical));
}

[SmartEnum]
public sealed partial class AwarenessKind {
    public static readonly AwarenessKind Cursor = new();
    public static readonly AwarenessKind Selection = new();
    public static readonly AwarenessKind Camera = new();
    public static readonly AwarenessKind Focus = new();
    public static readonly AwarenessKind Follow = new();
}

public readonly record struct AwarenessBeat(
    string Actor,
    AwarenessKind Kind,
    ReadOnlyMemory<byte> Payload,
    Option<string> FollowTarget,
    Instant At);

public static class Awareness {
    public const int LaneCapacity = 256;

    public static Channel<AwarenessBeat> AwarenessLane() =>
        Channel.CreateBounded<AwarenessBeat>(new BoundedChannelOptions(LaneCapacity) {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false,
        });

    public static AwarenessBeat Cursor(string actor, ReadOnlyMemory<byte> point, ClockPolicy clocks) =>
        new(actor, AwarenessKind.Cursor, point, None, clocks.Now);

    public static AwarenessBeat Selection(string actor, ReadOnlyMemory<byte> halo, ClockPolicy clocks) =>
        new(actor, AwarenessKind.Selection, halo, None, clocks.Now);

    public static AwarenessBeat Camera(string actor, ReadOnlyMemory<byte> frustum, ClockPolicy clocks) =>
        new(actor, AwarenessKind.Camera, frustum, None, clocks.Now);

    public static AwarenessBeat Follow(string actor, string target, ClockPolicy clocks) =>
        new(actor, AwarenessKind.Follow, ReadOnlyMemory<byte>.Empty, Some(target), clocks.Now);
}

[SmartEnum]
public sealed partial class CheckoutDimension {
    public static readonly CheckoutDimension Region = new();
    public static readonly CheckoutDimension Layer = new();
    public static readonly CheckoutDimension View = new();
    public static readonly CheckoutDimension Type = new();
    public static readonly CheckoutDimension Closure = new();
}

public sealed record ReplicationQuery(
    Option<byte[]> RegionEnvelope,
    Seq<string> Layers,
    Option<string> View,
    Seq<string> Types,
    int ClosureDepth);

public sealed record WorkingSet(
    Seq<UInt128> Keys,
    ReplicationQuery Query,
    SyncCursor Cursor,
    int ClosureDepth,
    Instant At) {
    public bool Subscribes(OpLogEntry entry) => Keys.Contains(entry.ContentKey);
}

public static class Replication {
    public static IO<WorkingSet> Checkout(
        ReplicationQuery query,
        Func<ReplicationQuery, IO<Seq<UInt128>>> resolve,
        Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch,
        SyncCursor cursor,
        ClockPolicy clocks) =>
        from keys in resolve(query)
        from _ in fetch(keys)
        select new WorkingSet(keys, query, cursor, query.ClosureDepth, clocks.Now);

    public static Seq<OpLogEntry> Filter(WorkingSet working, Seq<OpLogEntry> incoming) =>
        incoming.Filter(working.Subscribes);

    public static IO<WorkingSet> Expand(WorkingSet working, Seq<UInt128> additional, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch) =>
        fetch(additional.Filter(key => !working.Keys.Contains(key)))
            .Map(_ => working with { Keys = toSeq((working.Keys + additional).Distinct()) });
}
```

| [INDEX] | [POLICY]           | [VALUE]                                         | [BINDING]                                          |
| :-----: | :----------------- | :---------------------------------------------- | :------------------------------------------------- |
|   [1]   | presence heartbeat | `Every` 15 s                                    | one `ScheduleEntry` row per active editing surface |
|   [2]   | presence TTL       | 45 s — 3 × heartbeat                            | `Ttl`                                              |
|   [3]   | presence lane      | capacity 64, DropOldest, single reader          | `Lane`                                             |
|   [4]   | awareness lane     | capacity 256, DropOldest, multi reader/writer   | `AwarenessLane` — lossy, never durable             |
|   [5]   | blob framing       | 64 KiB frames, Crc32 per frame, XxHash128 whole | `FrameHash` incremental `Append`/`GetCurrentHash`  |

## [6]-[TS_PROJECTION]

- Owner: `OpLogEntryWire`, `SyncCursorWire`, `SyncSegmentWire`, `SyncRejectionWire`, `ConflictOutcomeKind`, `ConflictReceiptWire`, `PresenceRowWire`, `CrdtOpWire` — the sync wire surface the dashboard and companion peers decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface row decoded through the same decoder options, zero new surface; the `crdt` op carries its `CrdtOpWire` discriminated union on the same `OpLogEntryWire.payload` slot, never a second envelope.
- Boundary: 64-bit fields decode as bigint under useBigInt64; instants cross as ISO-8601 extended strings in the persisted temporal grammar and content keys cross as 16-byte binary — every shape on this surface is primitive-mapped with zero custom ext bytes, so no ExtensionCodec row exists; outcome discriminators cross as the case names and reconstruct as the literal union; the LSN crosses as the `pg_lsn` text spelling under the same wire grammar; `SyncRejectionWire` is the typed session-gate refusal for fingerprint mismatch; the `crdt` column-family op carries the `CrdtOpWire` discriminated union projected from `version-control#CRDT_ALGEBRA`, so the TS-web and Python legs decode the amended op-log payload on the one wire vocabulary, a breaking amendment recorded at the suite `CROSS_PACKAGE_LAWS` wire-law owner.

```ts contract
interface SyncCursorWire {
  origin: string;
  sequence: bigint;
  lsn: string;
  physical: string;
  logical: bigint;
}

interface OpLogEntryWire {
  sequence: bigint;
  entityKind: string;
  entityKey: string;
  columnFamily: string;
  kind: "upsert" | "delete" | "presence";
  codec: string;
  payload: Uint8Array;
  image: Uint8Array;
  contentKey: Uint8Array;
  closure: Uint8Array[];
  actor: string;
  origin: string;
  physical: string;
  logical: bigint;
}

interface SyncSegmentWire {
  schemaFingerprint: bigint;
  entries: OpLogEntryWire[];
  cursor: SyncCursorWire;
}

interface SyncRejectionWire {
  reason: "fingerprint-mismatch";
  localFingerprint: bigint;
  remoteFingerprint: bigint;
}

type ConflictOutcomeKind = "LocalWin" | "RemoteWin" | "Merged" | "Rejected";

interface ConflictReceiptWire {
  outcome: ConflictOutcomeKind;
  entityKind: string;
  entityKey: string;
  columnFamily: string;
  heldPhysical: string;
  heldLogical: bigint;
  heldActor: string;
  incomingPhysical: string;
  incomingLogical: bigint;
  incomingActor: string;
  correlation: string;
  at: string;
}

interface PresenceRowWire {
  actor: string;
  entityKind: string;
  entityKey: string;
  expiresAt: string;
}

type CrdtOpWire =
  | { kind: "set"; field: string; value: Uint8Array; physical: string; logical: bigint; origin: string }
  | { kind: "add"; field: string; element: Uint8Array; tagOrigin: string; tagLogical: bigint }
  | { kind: "remove"; field: string; element: Uint8Array; observedTags: { origin: string; logical: bigint }[] }
  | { kind: "insertAfter"; field: string; predecessor: { origin: string; logical: bigint }; id: { origin: string; logical: bigint }; value: Uint8Array }
  | { kind: "delete"; field: string; id: { origin: string; logical: bigint } };
```

## [7]-[RESEARCH]

- [LIVE_REPLICATION]: the tier-2 rasm-spike-stack PG18 harness finalized the pgoutput message-family decode-fold, the `PgOutputProtocolVersion.V4` + `PgOutputStreamingMode.Parallel` + `binary: true` non-obsolete `Options` spelling, the `ReplicationTuple` drain over the `IsDBNull`/`IsUnchangedToastedValue` three-state, the xid-keyed staging with `StreamCommitMessage`/`StreamAbortMessage` release, and the `SetReplicationStatus`/`SendStatusUpdate` LSN acknowledgement. The genuine tier-3 residue is a live PG18 root with a configured publication and subscription emitting a running stream end-to-end: `publish_generated_columns` on STORED columns, the idle-slot timeout setting, and the subscription conflict-stat read-view columns observed against a second live writer, with the applied-LSN watermark verified durable across a forced reconnect window — the one probe that strictly requires a long-lived live-server replication session.

