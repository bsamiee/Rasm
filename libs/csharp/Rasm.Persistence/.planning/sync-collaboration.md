# [PERSISTENCE_SYNC_COLLABORATION]

Rasm.Persistence owns local-first synchronization and collaboration: the op-log changefeed with HLC stamping, the LWW merge law with conflict receipts, the three-case `SyncTransport` axis widened by topology and direction fields, ephemeral presence, and blob transfer composing the settled `BlobRemote` contract under the ArtifactSync frame constants. AppHost vocabulary — the `ReceiptSinkPort` `Advance` algebra, `ClockPolicy`, `ScheduleEntry`, `DeadlineClass`, `DataClassification`, `CorrelationId` — arrives settled and composes inside the fences. Transport variance lands as case rows and two fields, so the merge law and the op-log shape stay transport-invariant and a new transport touches zero entity or query surface.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                          |
| :-----: | :---------------- | :------------------------------------------------------------------------------ |
|   [1]   | OPLOG_CHANGEFEED  | Op-log shape, HLC stamping, transfer manifests, tag-transition cursor mechanics |
|   [2]   | MERGE_LAW         | LWW adjudication, conflict receipts, idempotent apply fold                      |
|   [3]   | TRANSPORT_AXIS    | Three transport cases widened by topology and direction fields                  |
|   [4]   | PRESENCE_AND_BLOB | Presence rows, DropOldest lane, settled blob-contract framing, offline queue    |
|   [5]   | TS_PROJECTION     | MessagePack wire shapes for segments, conflicts, presence, rejection            |

## [2]-[OPLOG_CHANGEFEED]

- Owner: `SyncOpKind` `[SmartEnum<string>]` under the `SyncKeyPolicy` ordinal accessor; `OpLogEntry` — the one op-log record serving every synced entity kind; `SyncCursor`; the `OpLog` stamp-and-diff surface.
- Cases: 3 kind rows — upsert, delete (tombstone), presence; entity-kind variance rides the `EntityKind` column, never a second table shape.
- Entry: `public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ClockPolicy clocks, Func<(Instant Physical, ulong Logical), OpLogEntry> build)` — `IO` carries the HLC cell swap; the op-log stamp rides the same `Advance` algebra and the same `Hlc` atom as every receipt envelope.
- Auto: the SaveChanges changefeed hook and the bulk lane's self-emission append rows with store-assigned `Sequence` in the same transaction as the entity rows; peer processes fold `TagTransitions` over entries past their cursor on the replay `ScheduleEntry` row — applying those transitions to the cache surface is the L2 contribution row's consequence.
- Receipt: cursor position and queue depth ride `SyncApplyReceipt`; appended-segment evidence rides `ReceiptSinkPort` kinds through the package wire context.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new synced concern is one `SyncOpKind` row or one `EntityKind` value on the same op-log shape, zero new surface; per-entity-kind outbox tables are the deleted form.
- Boundary: op-log and entity rows commit in one transaction — a trigger-based second write path is the rejected form; tombstone rows carry the age-bound retention class and hide behind the sync-tombstone named query filter; `Closure` carries the graph's descendant content-key manifest so transfer is set-difference, never tree-walk; a multi-segment pull decodes through the settled `MessagePackStreamReader` segment reader one length-delimited frame at a time, so a large catch-up never buffers the whole transfer set contiguously.

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
    SyncOpKind Kind, SnapshotCodec Codec, ReadOnlyMemory<byte> Payload,
    UInt128 ContentKey, Seq<UInt128> Closure,
    string Actor, Guid OriginStoreId, Instant Physical, ulong Logical);

public sealed record SyncCursor(Guid OriginStoreId, long Sequence, Instant Physical, ulong Logical);

public static class OpLog {
    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ClockPolicy clocks, Func<(Instant Physical, ulong Logical), OpLogEntry> build) =>
        IO.lift(() => clocks.Now)
            .Map(wall => sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, wall)))
            .Map(build);

    public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds) =>
        entry.Closure.Add(entry.ContentKey).Filter(key => !holds(key));

    public static Seq<string> TagTransitions(Seq<OpLogEntry> entries) =>
        entries.Map(static entry => entry.EntityKind).Distinct();
}
```

| [INDEX] | [POLICY]              | [VALUE]                        | [BINDING]                                                |
| :-----: | :-------------------- | :----------------------------- | :------------------------------------------------------- |
|   [1]   | cursor replay cadence | `Every` 15 s                   | one `ScheduleEntry` row per peer process, lease none     |
|   [2]   | tombstone retention   | age-bound class on delete rows | retention sweep rows fold over the op-log artifact class |

## [3]-[MERGE_LAW]

- Owner: `ConflictReceipt`, `ConflictOutcome` `[Union]`, `SyncApplyReceipt`, `SyncSession` — the one session capsule of policy values and delegate rows; the `SyncMerge` fold surface.
- Cases: 4 outcome cases — LocalWin, RemoteWin, Merged, Rejected — every case carrying its `ConflictReceipt`.
- Entry: `public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming)` — `IO` carries commit effects; replay converges idempotently and the receipt proves applied, skipped, conflicted, and pushed counts.
- Receipt: `ConflictReceipt` is the typed conflict evidence projected to the inspector surface — one projection record, no second rail; `SyncApplyReceipt` is the per-run apply evidence carrying queue depth and the advanced cursor.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one policy value on the column-family axis feeding `Held` resolution, zero new surface; a fifth outcome case is the named defect.
- Boundary: LWW per column family is the default — `Held` resolves the competing local entry per entity key and column family, content-key equality adjudicates LocalWin (idempotent replay), an absent competitor adjudicates Merged, and equal stamps with divergent content adjudicate Rejected; HLC ordering ties break on origin store id so adjudication is deterministic across peers; a SyncEngine service class is the rejected form — the fold and the dispatch rows own the engine.

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
    long Applied, long Skipped, long Conflicted, long Pushed, long QueueDepth,
    Seq<ConflictReceipt> Conflicts, SyncCursor Cursor, CorrelationId Correlation, Instant At);

public sealed record SyncSession(
    ClockPolicy Clocks, ReceiptSinkPort Sink, CorrelationId Correlation,
    Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, CancellationToken Token,
    Func<UInt128, bool> Holds,
    Func<UInt128, IO<OpLogEntry>> Fetch,
    Func<OpLogEntry, Option<OpLogEntry>> Held,
    Func<OpLogEntry, IO<Unit>> Commit,
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
            IO.pure((Applied: 0L, Skipped: 0L, Conflicted: 0L, Conflicts: Seq<ConflictReceipt>())),
            (acc, entry) => acc.Bind(counts => Adjudicate(session, entry).Switch(
                state: (Session: session, Entry: entry, Counts: counts),
                localWin: static (s, _) => IO.pure(s.Counts with { Skipped = s.Counts.Skipped + 1L }),
                remoteWin: static (s, outcome) => s.Session.Commit(s.Entry).Map(_ => s.Counts with { Applied = s.Counts.Applied + 1L }),
                merged: static (s, outcome) => s.Session.Commit(s.Entry).Map(_ => s.Counts with { Applied = s.Counts.Applied + 1L }),
                rejected: static (s, outcome) => IO.pure(s.Counts with {
                    Conflicted = s.Counts.Conflicted + 1L,
                    Conflicts = s.Counts.Conflicts.Add(outcome.Receipt),
                }))))
            .Map(counts => new SyncApplyReceipt(
                counts.Applied, counts.Skipped, counts.Conflicted, Pushed: 0L,
                session.QueueDepth(), counts.Conflicts, session.Cursor, session.Correlation, session.Clocks.Now));
}
```

## [4]-[TRANSPORT_AXIS]

- Owner: `SyncTopology` and `SyncDirection` keyless vocabularies; `SyncTransport` `[Union]`; the `SyncPump` dispatch surface with the `CopyGraph` transport bridge; `GraphDiff` is the named set-difference diff-algebra `CopyGraph` and `SubtreeFetch` both dial — the closure-manifest set-difference over `Holds` membership that yields the missing content-key set; `SubtreeFetch` is the partial-graph checkout route fetching only the descendants of a chosen root rather than the whole graph.
- Cases: 3 transport cases — PgLogicalReplication, HttpDelta, SpeckleLikeDiff — widened by the `Topology` and `Direction` fields; fan-in, fan-out, and capture direction are field values, never new cases; `GraphDiff` carries the two diff operands — the source closure manifest and the target `Holds` predicate — never a new transport case.
- Entry: `public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport)` — one total state-threaded dispatch; `public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds)` is the set-difference diff-algebra projecting the missing-key set; `public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root)` is the partial-graph checkout over the diff.
- Auto: `Options` fixes the non-obsolete pgoutput spelling — protocol V4, parallel streaming, binary wire; `Open` is the slot-attach stream with `StartReplication` taking a non-optional cancellation token; `CreatePgOutputReplicationSlot` creates the slot and acknowledgement rides `SetReplicationStatus` with the applied-and-flushed LSN after each committed apply, flushed by `SendStatusUpdate` or the 10-second `WalReceiverStatusInterval` feedback cadence.
- Receipt: every pump run yields one `SyncApplyReceipt`; slot lifecycle evidence rides `ReceiptSinkPort` kinds.
- Packages: Npgsql, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new transport is one case row plus one dispatch arm, zero new surface; future PowerSync lands as one case behind its admission gate; future cr-sqlite lands as native-sqlite extension rows plus merge-law rows, never a case here; a new graph-checkout shape is one entry over `GraphDiff`, never a second diff algebra.
- Boundary: HttpDelta rides the AppHost `OutboundHop` http-api keyed pipeline — retry, backoff, and hop deadlines are owned there and the database stays excluded from the hop law; the document-granular fallback is the RFC 6902 patch payload, subordinate to the op-log changefeed, and RFC 7386 merge-patch is the rejected form; SSE and WebSocket transports are rejected — server-stream verbs own streaming; the obsolete ulong-protocol `PgOutputReplicationOptions` constructor is the rejected spelling, and `TestDecodingOptions` is the named rejected alternative output plugin — its human-readable text frames carry no typed message family, so pgoutput's binary `PgOutputReplicationMessage` leaves stay the only decode path; `Decode` folds the pgoutput message family — `InsertMessage`, the `DefaultUpdateMessage`/`FullUpdateMessage`/`IndexUpdateMessage` update leaves, the `KeyDeleteMessage`/`FullDeleteMessage` delete leaves, `TruncateMessage`, and the stream control frames — over `RelationMessage` schema context into op-log entries behind the session delegate; a transport bridge is two sessions composed through `CopyGraph` — transport state and op-log mechanics live here, the wire leg is the settled proto vocabulary; `GraphDiff` is the one set-difference diff-algebra — the closure-manifest minus the target `Holds` set yields the missing content keys — and both `CopyGraph` (whole-graph bridge from the root) and `SubtreeFetch` (partial-graph checkout of a chosen sub-root's descendants) dial it, so a second walk-and-diff implementation is the deleted form and `OpLog.TransferSet` is the leaf the algebra composes; `SubtreeFetch` fetches only the chosen sub-root entry and traverses its `Closure` manifest, so a partial checkout never pulls the full graph and a client that holds a parent reuses its held keys through the same diff; the wire leg of `GraphDiff`/`SubtreeFetch` is owned at `Compute/remote-lane#PROTO_VOCABULARY` as the `GraphDiff`/`SubtreeFetch` message family — the walk-and-diff computation lives here and the wire shape lives there, so the remote-lane rpc dials this algebra over the transport and never re-implements the set-difference.

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

public static class SyncPump {
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
                        Applied: 0L, Skipped: 0L, Conflicted: 0L, Pushed: missing.Count,
                        QueueDepth: s.QueueDepth(), Conflicts: Seq<ConflictReceipt>(),
                        Cursor: cursor, Correlation: s.Correlation, At: s.Clocks.Now))));
}
```

| [INDEX] | [POLICY]                        | [VALUE]                                          | [BINDING]                                              |
| :-----: | :------------------------------ | :----------------------------------------------- | :----------------------------------------------------- |
|   [1]   | generated-column replication    | `PublishGeneratedColumns` on STORED columns only | pairs the STORED generated-column schema law           |
|   [2]   | idle slot reclamation           | config-sourced `IdleSlotTimeout`                 | server setting verified by the provisioning probe rows |
|   [3]   | replication conflict statistics | subscription-stats read view                     | the live-replication research gate carries the columns |
|   [4]   | session admission               | schema-fingerprint equality                      | mismatch is the typed `SyncRejectionWire` refusal      |
|   [5]   | graph checkout granularity      | `GraphDiff` set-difference over `Holds`          | whole-graph `CopyGraph` and partial `SubtreeFetch` dial one algebra; wire family at `Compute/remote-lane#PROTO_VOCABULARY` |

## [5]-[PRESENCE_AND_BLOB]

- Owner: `PresenceRow` and the `Presence` surface — ephemeral collaboration rows on the op-log shape; `AwarenessBeat` and the `Awareness` surface — the dedicated low-latency lossy awareness channel carrying cursor, selection, camera-frustum, active-node-focus, and follow-mode beats off the durable op-log; `WorkingSet` and the `Replication` surface — the partial-replication subgraph-checkout and working-set op-stream subscription; blob transfer composes the settled `BlobRemote` contract record, never a second blob shape.
- Entry: `public static IO<OpLogEntry> Beat(ReceiptSinkPort sink, ClockPolicy clocks, Guid storeId, SnapshotCodec codec, PresenceRow row, ReadOnlyMemory<byte> payload)` — presence is one changefeed row, never a transport; `public static Channel<AwarenessBeat> AwarenessLane()` is the dedicated lossy fan-out channel and `public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch)` materializes a subgraph working set.
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
|   [4]   | blob framing       | 64 KiB frames, Crc32 per frame, XxHash128 whole | `FrameHash` incremental `Append`/`GetCurrentHash`  |

## [6]-[TS_PROJECTION]

- Owner: `OpLogEntryWire`, `SyncCursorWire`, `SyncSegmentWire`, `SyncRejectionWire`, `ConflictOutcomeKind`, `ConflictReceiptWire`, `PresenceRowWire` — the sync wire surface the dashboard and companion peers decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface row decoded through the same decoder options, zero new surface.
- Boundary: 64-bit fields decode as bigint under useBigInt64; instants cross as ISO-8601 extended strings in the persisted temporal grammar and content keys cross as 16-byte binary — every shape on this surface is primitive-mapped with zero custom ext bytes, so no ExtensionCodec row exists; outcome discriminators cross as the case names and reconstruct as the literal union; `SyncRejectionWire` is the typed session-gate refusal for fingerprint mismatch.

```ts contract
interface SyncCursorWire {
  origin: string;
  sequence: bigint;
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
```

## [7]-[RESEARCH]

- [LIVE_REPLICATION]: publish_generated_columns publication parameter, idle-slot timeout setting, and subscription conflict-stat columns on a live PG18 server; the live decode-fold confirmation that a running pgoutput stream emits the catalogued leaf set (`InsertMessage`, the `DefaultUpdateMessage`/`FullUpdateMessage`/`IndexUpdateMessage` update leaves, the `KeyDeleteMessage`/`FullDeleteMessage` delete leaves, `TruncateMessage`, `RelationMessage`, and the stream-control frames) under `PgOutputProtocolVersion.V4` + `PgOutputStreamingMode.Parallel`, with `SetReplicationStatus`/`SendStatusUpdate` LSN acknowledgement against the live slot.
