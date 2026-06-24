# [PERSISTENCE_SYNC_COLLABORATION]

Rasm.Persistence owns local-first synchronization and collaboration: the op-log changefeed with HLC stamping and the logical-replication decode-fold that materializes a running pgoutput stream into op-log entries, the merge law that adjudicates each `OpLogEntry` by its `ColumnFamily.Stance` — scalar `Lww`/`FirstWriter` through `Adjudicate`, the convergent `Crdt` lane into the settled `Crdt.Apply` join-semilattice — under the closed `SyncFault` rail, the three-case `SyncTransport` axis widened by one `SyncFlow` disposition field, ephemeral presence, the dedicated lossy awareness lane, partial-replication working-set checkout, and blob transfer composing the settled `BlobRemote` contract under the ArtifactSync frame constants. AppHost vocabulary — the `ReceiptSinkPort` `Advance` algebra, `ClockPolicy`, `ScheduleEntry`, `DeadlineClass`, `DataClassification`, `CorrelationId`, the `LaneRow`/`LaneLoss`/`LossClass` lane-and-drop-receipt vocabulary, and the `AppHost/Observability/telemetry#CORRELATION_SPINE` propagation fold — arrives settled and composes inside the fences. The `OpLogEntry` envelope carries a `TraceContext` W3C trace-context slot so an op committed under an active span anchors the `ONE_DISTRIBUTED_TRACE` concert seam on the op-log; the stamp reads `Activity.Current` once for an in-process write while a pgoutput-decoded row carries `TraceContext.Empty` (the WAL holds no traceparent), and the replication pump extract-and-continues the parent span only when one exists, so a write that crosses runtimes reads end-to-end on one trace id rather than three collector silos. Transport variance lands as case rows and one `SyncFlow` field, so the merge law and the op-log shape stay transport-invariant and a new transport touches zero entity or query surface; the pgoutput decode is the one path a running PostgreSQL changefeed travels into the one op-log shape, never a second feed. The `SpeckleLikeDiff` transport binds the instance `Speckle.Sdk` `IOperations.Send`/`Receive` marshal delegates on `SyncSession`, resolved from the `AddSpeckleSdk` container so the rail never reaches for a static `Operations.Send`; `Operations.Send` returns `(string rootObjId, IReadOnlyDictionary<string, ObjectReference>)` where `rootObjId` is the content hash mapping onto the existing `UInt128 ContentKey` with zero second identity, and `Speckle.Sdk`'s repacked Polly/channel/serialisation-V2 transitive closure runs COMPANION / OUTSIDE-RHINO so the in-Rhino assembly touches only the marshal delegate slot and never loads `Speckle.Sdk` or `Speckle.Objects`.

## [01]-[INDEX]

- [01]-[OPLOG_CHANGEFEED]: op-log shape, HLC stamping, transfer manifests, and tag-transition cursor mechanics.
- [02]-[MERGE_LAW]: LWW adjudication, conflict receipts, idempotent apply fold, and crdt dispatch.
- [03]-[TRANSPORT_AXIS]: three transport cases widened by one `SyncFlow` disposition; pgoutput decode-fold pump.
- [04]-[PRESENCE_AND_BLOB]: presence rows, awareness lane, working-set checkout, and settled blob-contract framing.
- [05]-[TS_PROJECTION]: MessagePack wire shapes for segments, conflicts, presence, crdt ops, and rejection.

## [02]-[OPLOG_CHANGEFEED]

- Owner: `SyncOpKind` `[SmartEnum<string>]` the write-verb axis under the `SyncKeyPolicy` ordinal accessor; `ColumnFamily` `[SmartEnum<string>]` the merge-lane axis carrying its own `MergeStance` policy value (`Lww | Crdt | FirstWriter`) so the lane that an `OpLogEntry` rides selects its adjudication algebra as a vocabulary row, never a `columnFamily == "crdt"` string compare in the fold; `TraceContext` `[ComplexValueObject]` — the W3C trace-context carrier (`traceparent` 16-byte trace-id, opaque `tracestate`) the envelope slot rides, its `Empty` value sentinel the no-active-span CASE; `OpLogEntry` — the one op-log record serving every synced entity kind, carrying the `ColumnFamily` lane discriminant, the `TraceContext` envelope slot beside `ContentKey`, and the replica-identity `Image`; `SyncCursor` carrying both the HLC cell and the WAL LSN watermark; the `OpLog` stamp-and-diff surface.
- Cases: `SyncOpKind` is 4 verb rows — `Upsert`, `Delete` (row tombstone), `Truncate` (relation-wide tombstone, `WholeRelation`), `Presence`; `ColumnFamily` is 6 lane rows — `Scalar` (`Lww`), `Crdt` (`Crdt`, the `Version/commits#CRDT_ALGEBRA` convergent lane), `Geometry` (`Lww`, the `Query/lanes#SEARCH_LANES` geometry op lane), `Presence` (`FirstWriter`, never durable past TTL), `Commit` (`Lww`, the `Version/commits#COMMIT_DAG` ref-stable lane), `Branch` (`Lww`) — and a seventh synced lane is one `ColumnFamily` row carrying its `MergeStance`, never a parallel switch arm or a `"crdt"` string compare in a consumer; entity-kind variance rides the `EntityKind` column, never a second table shape; before-image variance rides the `Image` column, never a second record.
- Entry: `public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ClockPolicy clocks, Func<(Instant Physical, ulong Logical, TraceContext Trace), OpLogEntry> build)` — `IO` carries the HLC cell swap; the op-log stamp rides the same `Advance` algebra and the same `Hlc` atom as every receipt envelope, and captures the ambient `Activity.Current` as a `TraceContext` value ONCE at stamp (or `TraceContext.Empty` when no span is active) so the build delegate never re-mints a root and an op committed under no span carries the empty sentinel, never a synthesized root.
- Auto: the SaveChanges changefeed hook and the bulk lane's self-emission append rows with store-assigned `Sequence` in the same transaction as the entity rows; peer processes fold `TagTransitions` over entries past their cursor on the replay `ScheduleEntry` row — applying those transitions to the cache surface is the L2 contribution row's consequence; the logical-replication pump folds the running pgoutput stream into the same op-log shape so a change committed by any writer reaches every peer over the one feed.
- Receipt: cursor position, queue depth, and the acknowledged LSN ride `SyncApplyReceipt`; appended-segment evidence rides `ReceiptSinkPort` kinds through the package wire context.
- Packages: Npgsql, NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new synced concern is one `SyncOpKind` verb row, one `ColumnFamily` lane row carrying its `MergeStance`, or one `EntityKind` value on the same op-log shape, zero new surface; per-entity-kind outbox tables and a per-lane string literal in the merge fold are the deleted forms.
- Boundary: op-log and entity rows commit in one transaction — a trigger-based second write path is the rejected form; tombstone rows carry the age-bound retention class and hide behind the sync-tombstone named query filter; `Closure` carries the graph's descendant content-key manifest so transfer is set-difference, never tree-walk; a multi-segment pull decodes through the settled `MessagePackStreamReader` segment reader one length-delimited frame at a time, so a large catch-up never buffers the whole transfer set contiguously, each frame lifting to `Fin<OpLogEntry>` so a malformed segment a version-skewed peer emits faults as the typed `SyncFault.TransferDecode` rather than throwing out of the loop; the before-image `Image` column carries the table's replica-identity tuple so the merge law has the held content key without a read-before-write round trip, and a missing-replica-identity update decodes as an empty image folded as a `Merged` adjudication; the `TraceContext` slot is a top-level envelope field beside `ContentKey`, NOT inside `Payload` (Payload is the `Version/commits#CRDT_WIRE` `CrdtOp` delta for crdt rows) so the trace slot is no `CrdtOpWire` schema fork and triggers no `AppHost/runtime-ports#WIRE_LAW` amendment — it realizes the `ONE_DISTRIBUTED_TRACE` concert seam on the op-log by carrying the C#-minted `traceparent`/`tracestate` the `AppHost/Observability/telemetry#CORRELATION_SPINE` propagation fold owns; Persistence only READS `Activity.Current` at stamp (via `System.Diagnostics.DiagnosticSource`) and projects to the value, never re-mints the propagator or a second tracer (ARCHITECTURE NEVER L91: never a second correlation owner), the 16-byte `traceparent` length validating ONCE at `TraceContext` admission so the interior never re-parses, and `OpLogEntry` carries NO `CorrelationId` field — `CorrelationId` rides `SyncSession`/`SyncApplyReceipt` so the trace slot is a genuinely new envelope field the AppHost correlation spine populates, never a sibling of an existing correlation column.
- Exemption: the `OpLog.Segments` reader loop — the `await foreach` over `MessagePackStreamReader.ReadAsync` yielding one length-delimited frame per decode — is the platform-forced stream statement seam; the stamp and transfer-set folds stay expression-shaped.

```csharp signature
public sealed class SyncKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class SyncOpKind {
    public static readonly SyncOpKind Upsert = new("upsert", tombstone: false, wholeRelation: false);
    public static readonly SyncOpKind Delete = new("delete", tombstone: true, wholeRelation: false);
    public static readonly SyncOpKind Truncate = new("truncate", tombstone: true, wholeRelation: true);
    public static readonly SyncOpKind Presence = new("presence", tombstone: false, wholeRelation: false);

    public bool Tombstone { get; }

    public bool WholeRelation { get; }
}

[SmartEnum]
public sealed partial class MergeStance {
    public static readonly MergeStance Lww = new(convergent: false);
    public static readonly MergeStance Crdt = new(convergent: true);
    public static readonly MergeStance FirstWriter = new(convergent: false);

    public bool Convergent { get; }
}

// The merge-lane axis: each OpLogEntry rides one lane whose MergeStance selects the
// adjudication algebra, so SyncMerge.Apply dispatches on Family.Stance, never a string compare.
[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class ColumnFamily {
    public static readonly ColumnFamily Scalar = new("scalar", MergeStance.Lww, durable: true);
    public static readonly ColumnFamily Crdt = new("crdt", MergeStance.Crdt, durable: true);
    public static readonly ColumnFamily Geometry = new("geometry", MergeStance.Lww, durable: true);
    public static readonly ColumnFamily Presence = new("presence", MergeStance.FirstWriter, durable: false);
    public static readonly ColumnFamily Commit = new("commit", MergeStance.Lww, durable: true);
    public static readonly ColumnFamily Branch = new("branch", MergeStance.Lww, durable: true);

    public MergeStance Stance { get; }

    public bool Durable { get; }
}

[ComplexValueObject]
public sealed partial class TraceContext {
    public static readonly TraceContext Empty = new(ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty);

    public ReadOnlyMemory<byte> Traceparent { get; }

    public ReadOnlyMemory<byte> Tracestate { get; }

    public bool HasParent => Traceparent.Length == 16;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ReadOnlyMemory<byte> traceparent, ref ReadOnlyMemory<byte> tracestate) {
        if (traceparent.Length is not (0 or 16))
            validationError = new ValidationError($"<traceparent-length:{traceparent.Length}>");
    }

    public static TraceContext Capture() =>
        Activity.Current is { } activity && activity.Context.TraceId != default
            ? Project(activity.Context, activity.TraceStateString)
            : Empty;

    public static TraceContext Project(ActivityContext context, string? traceState) {
        var span = new byte[16];
        context.TraceId.CopyTo(span.AsSpan(0, 16));
        return Create(span, Encoding.ASCII.GetBytes(traceState ?? string.Empty));
    }

    public Option<ActivityContext> Continue() =>
        HasParent
            ? Some(new ActivityContext(
                ActivityTraceId.CreateFromBytes(Traceparent.Span),
                ActivitySpanId.CreateRandom(),
                ActivityTraceFlags.Recorded,
                Encoding.ASCII.GetString(Tracestate.Span) is { Length: > 0 } state ? state : null,
                isRemote: true))
            : None;
}

public sealed record OpLogEntry(
    long Sequence, string EntityKind, string EntityKey, ColumnFamily Family,
    SyncOpKind Kind, SnapshotCodec Codec, ReadOnlyMemory<byte> Payload, ReadOnlyMemory<byte> Image,
    UInt128 ContentKey, TraceContext Trace, Seq<UInt128> Closure,
    string Actor, Guid OriginStoreId, Instant Physical, ulong Logical) {
    public Hlc Stamp => new(Physical, Logical);
}

public sealed record SyncCursor(Guid OriginStoreId, long Sequence, NpgsqlLogSequenceNumber Lsn, Instant Physical, ulong Logical) {
    public static readonly SyncCursor Genesis =
        new(Guid.Empty, 0L, NpgsqlLogSequenceNumber.Invalid, Instant.MinValue, 0UL);

    public ulong Stamp => ((ulong)Physical.ToUnixTimeMilliseconds() << 16) | (Logical & 0xFFFF);
}

public static class OpLog {
    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ClockPolicy clocks, Func<(Instant Physical, ulong Logical, TraceContext Trace), OpLogEntry> build) =>
        IO.lift(() => (Wall: clocks.Now, Trace: TraceContext.Capture()))
            .Map(captured => (Cell: sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, captured.Wall)), captured.Trace))
            .Map(stamped => build((stamped.Cell.Physical, stamped.Cell.Logical, stamped.Trace)));

    public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds) =>
        entry.Closure.Add(entry.ContentKey).Filter(key => !holds(key));

    public static Seq<string> TagTransitions(Seq<OpLogEntry> entries) =>
        entries.Map(static entry => entry.EntityKind).Distinct();

    // Each length-delimited transfer frame lifts to Fin so a malformed segment a version-skewed or
    // truncated peer emits faults the consumer as the typed SyncFault.TransferDecode rather than
    // throwing raw out of the catch-up loop; the decode delegate's exception is the only failure shape.
    public static async IAsyncEnumerable<Fin<OpLogEntry>> Segments(Stream lane, string peer, Func<ReadOnlySequence<byte>, OpLogEntry> decode, [EnumeratorCancellation] CancellationToken token = default) {
        using var reader = new MessagePackStreamReader(lane, leaveOpen: true);
        while (await reader.ReadAsync(token).ConfigureAwait(false) is { } frame) {
            yield return Try.lift(() => decode(frame)).Run()
                .MapFail(error => (Error)new SyncFault.TransferDecode(peer, error.Message));
        }
    }
}
```

| [INDEX] | [POLICY]              | [VALUE]                        | [BINDING]                                                |
| :-----: | :-------------------- | :----------------------------- | :------------------------------------------------------- |
|  [01]   | cursor replay cadence | `Every` 15 s                   | one `ScheduleEntry` row per peer process, lease none     |
|  [02]   | tombstone retention   | age-bound class on delete rows | retention sweep rows fold over the op-log artifact class |
|  [03]   | LSN watermark         | `SyncCursor.Lsn`               | the idempotency key the pump acknowledges after apply    |

## [03]-[MERGE_LAW]

- Owner: `ConflictReceipt`, `ConflictVerdict` `[SmartEnum<string>]`, `ConflictResult`, `SyncApplyReceipt` (with its `Conserves` conservation invariant), `SyncFault` the closed `[Union]` fault family deriving from `Expected` in the 8250 band (`SchemaMismatch` 8251 | `ReplicationFaulted` 8252 | `SpeckleMarshal` 8253 | `TransferDecode` 8254 | `Unconserved` 8255 | `Forked` 8256), `SyncSession` — the one session capsule of policy values and delegate rows, including the `SpeckleSend`/`SpeckleReceive` marshal delegates that bind the DI-resolved instance `IOperations.Send`/`Receive`; the `SyncMerge` fold surface routing each `OpLogEntry` by its `ColumnFamily.Stance` — `Lww`/`FirstWriter` through `Adjudicate`, `Crdt` into the settled `Crdt.Apply`.
- Cases: 4 verdict rows on `ConflictVerdict` — local-win, remote-win, merged, rejected — collapsed from the prior `ConflictOutcome` 4-case `[Union]` into one `ConflictResult(Verdict, Receipt)` record carrying the verdict discriminant beside its `ConflictReceipt`, so merge resolution and the `Query/transaction#SQLSTATE_CLASSIFIER` deadlock classifier speak one verdict vocabulary and a parallel sync-side outcome union is the deleted form; `rejected` is reachable only on an equal `(stamp, origin)` with divergent content — the causal fork the `Apply` fold lifts to `SyncFault.Forked` and halts on, never a soft conflict bucket LWW cannot produce — so the deterministic winners are local-win/remote-win/merged and the only unresolvable outcome is the fork; merge dispatch reads `entry.Family.Stance` so the convergent `Crdt` lane supersedes scalar adjudication and converges by merge, never by discarding the loser, and the entire fault surface is the closed `SyncFault` union — a bare `Error.New(...)` for fingerprint mismatch, replication fault, SDK marshal failure, or a causal fork is the deleted untyped form.
- Entry: `public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming)` — `IO` carries commit effects; replay converges idempotently and the receipt proves applied, skipped, conflicted, converged, and pushed counts under the `Conserves` invariant, an `IO.fail` `SyncFault.Unconserved` when the batch count does not close.
- Receipt: `ConflictReceipt` is the typed fork evidence the `SyncFault.Forked` halt carries and the inspector surface projects — held/incoming stamps, actors, and correlation under one projection record, no second rail; `SyncApplyReceipt` is the per-run apply evidence carrying queue depth, the advanced cursor, and the acknowledged LSN, its `Conflicts` slot empty on a converged run because a fork halts rather than accumulates.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one `MergeStance` row carried by a `ColumnFamily` lane feeding `Held` resolution, zero new surface; a fifth `ConflictVerdict` row is the named defect; a new fault cause is one `SyncFault` case in the closed family; a new replicated data type is a `Version/commits#CRDT_ALGEBRA` `CrdtField` case dispatched by this fold, never a fifth scalar arm.
- Boundary: LWW per column family is the default — `Held` resolves the competing local entry per entity key and column family, content-key equality adjudicates LocalWin (idempotent replay), an absent competitor adjudicates Merged through `Fresh` whose held slots carry the `Hlc.Zero` absence sentinel and an empty actor rather than a fabricated self-copy of the incoming entry, and an equal `(stamp, origin)` with divergent content is the causal fork — one origin minting the same stamp two ways — which `Apply` halts as the epoch-class `SyncFault.Forked` (8256) carrying the `ConflictReceipt` plus the two divergent content keys, never a soft conflict that counts and continues, so a fork halts merge with that peer per the durability fork law rather than silently merging two registers; HLC ordering ties break on origin store id so adjudication is deterministic across peers; a SyncEngine service class is the rejected form — the fold and the dispatch rows own the engine; the `crdt` column family routes its `OpLogEntry.Payload` through `Version/commits#CRDT_ALGEBRA` `Crdt.Apply` so a concurrent edit converges by the join-semilattice least-upper-bound rather than a scalar last-writer-wins, the LWW `Adjudicate` surviving only as the `LwwRegister` arm — a cross-package wire-vocabulary amendment recorded as a ledger seam-split, never a parallel sync engine, and the same one IMMEDIATE apply transaction holds both the scalar register mutation and the crdt-field merge so crash recovery is re-delivery for both; the `SpeckleSend`/`SpeckleReceive` delegates are the marshal seam for the `SpeckleLikeDiff` transport — `SpeckleSend` binds the instance `Operations.Send(Base, IServerTransport, useDefaultCache, ...)` resolved from the `AddSpeckleSdk` container and projects the returned `rootObjId` content hash onto the `UInt128 RootContentKey` (the existing `ContentKey`, zero second identity) with the `convertedReferences` `ObjectReference` map collapsed to its count, while `SpeckleReceive` binds the instance `Operations.Receive(objectId, remoteTransport, localTransport, ...)` and maps the returned `Base`/`DataObject` graph to closed Rasm op-log entries AT THE SEAM so no `Base`, `ObjectReference`, or `DataObject` shape crosses into the domain; the SDK boundary faults (`SpeckleException`, `TransportException`, `SpeckleDeserializeException`, `HttpRequestException`) lift once at this marshal into the Persistence merge/sync rail as the typed `SyncFault.SpeckleMarshal` (8253) carrying the failing peer and the SDK fault class, never a bare `Error.New` and never the TS-projection wire fault union.

```csharp signature
public readonly record struct ConflictReceipt(
    string EntityKind, string EntityKey, ColumnFamily Family,
    Hlc Held, string HeldActor, Hlc Incoming, string IncomingActor,
    CorrelationId Correlation, Instant At);

[SmartEnum<string>]
[KeyMemberEqualityComparer<SyncKeyPolicy, string>]
[KeyMemberComparer<SyncKeyPolicy, string>]
public sealed partial class ConflictVerdict {
    public static readonly ConflictVerdict LocalWin = new("LocalWin", applies: false);
    public static readonly ConflictVerdict RemoteWin = new("RemoteWin", applies: true);
    public static readonly ConflictVerdict Merged = new("Merged", applies: true);
    public static readonly ConflictVerdict Rejected = new("Rejected", applies: false);

    public bool Applies { get; }
}

public readonly record struct ConflictResult(ConflictVerdict Verdict, ConflictReceipt Receipt);

// The closed sync-fault family deriving from Expected — schema fork, replication fault, SDK marshal
// failure, transfer decode, the merge conservation breach, and the causal fork all carry typed
// evidence, so the merge/sync rail never widens a bare untyped Error. Forked is the epoch-class
// origin-uniqueness breach (one origin minting the same stamp with two content keys), distinct from
// a soft concurrency Rejected — it halts merge with the peer rather than counting one more conflict.
public abstract partial record SyncFault : Expected, IValidationError<SyncFault> {
    private SyncFault(string detail, int code) : base(detail, code, None) { }
    public static SyncFault Create(string message) => new ReplicationFaulted(string.Empty, message);
    public sealed record SchemaMismatch(ulong Local, ulong Remote) : SyncFault($"{Local}:{Remote}", 8251);
    public sealed record ReplicationFaulted(string Slot, string Cause) : SyncFault($"{Slot}:{Cause}", 8252);
    public sealed record SpeckleMarshal(string Peer, string Class) : SyncFault($"{Peer}:{Class}", 8253);
    public sealed record TransferDecode(string Peer, string Cause) : SyncFault($"{Peer}:{Cause}", 8254);
    public sealed record Unconserved(long Batch, long Settled) : SyncFault($"{Batch}!={Settled}", 8255);
    public sealed record Forked(ConflictReceipt Receipt, UInt128 Held, UInt128 Incoming) : SyncFault($"{Receipt.EntityKind}/{Receipt.EntityKey}@{Receipt.Incoming.Physical}:{Held}!={Incoming}", 8256);
}

public readonly record struct SyncApplyReceipt(
    long Applied, long Skipped, long Conflicted, long Converged, long Pushed, long QueueDepth,
    Seq<ConflictReceipt> Conflicts, SyncCursor Cursor, CorrelationId Correlation, Instant At) {
    public long Settled => Applied + Skipped + Conflicted + Converged;

    public bool Conserves(long batch) => batch == Settled;
}

public sealed record SyncSession(
    ClockPolicy Clocks, ReceiptSinkPort Sink, CorrelationId Correlation,
    Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, CancellationToken Token,
    Func<UInt128, bool> Holds,
    Func<RelationMessage, ulong> RelationFingerprint,
    Func<UInt128, IO<OpLogEntry>> Fetch,
    Func<OpLogEntry, Option<OpLogEntry>> Held,
    Func<OpLogEntry, IO<Unit>> Commit,
    Func<OpLogEntry, IO<Unit>> Converge,
    Func<SyncCursor, Seq<OpLogEntry>> Pending,
    Func<long> QueueDepth,
    Func<string, SyncCursor, IO<(ulong SchemaFingerprint, Seq<OpLogEntry> Entries, SyncCursor Cursor)>> Pull,
    Func<string, Seq<OpLogEntry>, IO<SyncCursor>> Push,
    Func<string, Seq<UInt128>, IO<Seq<UInt128>>> HasObjects,
    Func<string, CancellationToken, IO<Seq<OpLogEntry>>> Decode,
    Func<string, Seq<OpLogEntry>, IO<(UInt128 RootContentKey, long ConvertedReferences)>> SpeckleSend,
    Func<string, UInt128, IO<Seq<OpLogEntry>>> SpeckleReceive);

public static class SyncMerge {
    public static ConflictReceipt Receipt(SyncSession session, OpLogEntry held, OpLogEntry incoming) =>
        new(incoming.EntityKind, incoming.EntityKey, incoming.Family,
            held.Stamp, held.Actor, incoming.Stamp, incoming.Actor,
            session.Correlation, session.Clocks.Now);

    // A fresh merge has no held competitor, so the held slots carry the Hlc.Zero absence sentinel
    // and an empty actor rather than a fabricated self-copy of the incoming entry.
    private static ConflictReceipt Fresh(SyncSession session, OpLogEntry incoming) =>
        new(incoming.EntityKind, incoming.EntityKey, incoming.Family,
            Hlc.Zero, string.Empty, incoming.Stamp, incoming.Actor,
            session.Correlation, session.Clocks.Now);

    // Scalar adjudication: content-key equality is idempotent replay (LocalWin), an absent
    // competitor is a fresh merge, an HLC+origin total order breaks the tie, and an equal (stamp,
    // origin) with divergent content is the causal fork (Rejected) — one origin minting the same
    // stamp two ways violates origin-uniqueness, never a recoverable concurrency conflict.
    // FirstWriter degrades the > arm to a held-keep by reading Family.Stance, so the same dispatch
    // serves both non-convergent stances.
    public static ConflictResult Adjudicate(SyncSession session, OpLogEntry incoming) =>
        session.Held(incoming) is { IsSome: true, Case: OpLogEntry held }
            ? incoming.ContentKey == held.ContentKey
                ? new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming))
                : (incoming.Stamp, incoming.OriginStoreId).CompareTo((held.Stamp, held.OriginStoreId)) switch {
                        > 0 when incoming.Family.Stance == MergeStance.FirstWriter =>
                            new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming)),
                        > 0 => new ConflictResult(ConflictVerdict.RemoteWin, Receipt(session, held, incoming)),
                        < 0 => new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming)),
                        _ => new ConflictResult(ConflictVerdict.Rejected, Receipt(session, held, incoming)),
                    }
            : new ConflictResult(ConflictVerdict.Merged, Fresh(session, incoming));

    // A Rejected verdict is the equal-(stamp, origin) divergent-content fork: an epoch-class
    // origin-uniqueness breach that halts merge with the peer as SyncFault.Forked rather than
    // counting one more soft conflict — the held content key recovers from the competitor lookup.
    public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming) =>
        incoming.Fold(
            IO.pure((Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Conflicts: Seq<ConflictReceipt>())),
            (acc, entry) => acc.Bind(counts => entry.Family.Stance.Convergent
                ? session.Converge(entry).Map(_ => counts with { Converged = counts.Converged + 1L })
                : Adjudicate(session, entry) is var result && result.Verdict.Applies
                    ? session.Commit(entry).Map(_ => counts with { Applied = counts.Applied + 1L })
                    : result.Verdict == ConflictVerdict.Rejected
                        ? IO.fail<(long Applied, long Skipped, long Conflicted, long Converged, Seq<ConflictReceipt> Conflicts)>(
                            new SyncFault.Forked(result.Receipt,
                                session.Held(entry).Map(static held => held.ContentKey).IfNone(UInt128.Zero), entry.ContentKey))
                        : IO.pure(counts with { Skipped = counts.Skipped + 1L })))
            .Map(counts => new SyncApplyReceipt(
                counts.Applied, counts.Skipped, counts.Conflicted, counts.Converged, Pushed: 0L,
                session.QueueDepth(), counts.Conflicts, session.Cursor, session.Correlation, session.Clocks.Now))
            .Bind(receipt => receipt.Conserves(incoming.Count)
                ? IO.pure(receipt)
                : IO.fail<SyncApplyReceipt>(new SyncFault.Unconserved(incoming.Count, receipt.Settled)));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                              | [BINDING]                                                  |
| :-----: | :-------------------- | :--------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | lane → merge stance   | `ColumnFamily.Stance` (`Lww`/`Crdt`/`FirstWriter`)   | dispatch reads the lane row, never a `"crdt"` string compare |
|  [02]   | scalar default        | LWW by `(Hlc, OriginStoreId)`                        | total order on the stamp tuple, deterministic across peers |
|  [03]   | crdt lane             | `Version/commits#CRDT_ALGEBRA` `Crdt.Apply`          | `Stance.Convergent` join-semilattice merge, superset of `Adjudicate` |
|  [04]   | conservation identity | `Conserves(batch)` over `Applied+Skipped+Conflicted+Converged` | a breach is `SyncFault.Unconserved` failing the merge rail |

## [04]-[TRANSPORT_AXIS]

- Owner: `SyncFlow` the keyless disposition vocabulary carrying the `(Pulls, Pushes)` policy pair (the prior parallel `SyncTopology`/`SyncDirection` enums collapsed into one read owner); `SyncTransport` `[Union]`; the `SyncPump` dispatch surface with the `SubtreeFetch` graph-checkout bridge and the `Offer` Speckle-diff arm; the `ReplicationPump` decode-fold owning the running pgoutput message family; `GraphDiff` is the named set-difference diff-algebra `SubtreeFetch` and `Offer` both dial — the closure-manifest set-difference over `Holds` membership that yields the missing content-key set; `SubtreeFetch` is the one graph-checkout route whose `root` argument discriminates breadth — a true graph root copies the whole graph, a chosen sub-root checks out only that sub-root's descendants — fetching only the keys the target lacks rather than the whole graph.
- Cases: 3 transport cases — PgLogicalReplication, HttpDelta, SpeckleLikeDiff — widened by the one `SyncFlow` field whose `Pulls`/`Pushes` policy pair the `Exchange` fold reads; fan-in, fan-out, and bidirectional are `SyncFlow` rows, never new transport cases and never a parallel direction enum; `GraphDiff` carries the two diff operands — the source closure manifest and the target `Holds` predicate — never a new transport case.
- Entry: `public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport)` — one total state-threaded dispatch; `public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds)` is the set-difference diff-algebra projecting the missing-key set; `public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root)` is the partial-graph checkout over the diff; `public static async Task<Fin<SyncApplyReceipt>> Pump(LogicalReplicationConnection link, SyncTransport.PgLogicalReplication row, SyncSession session, Func<CommitBatch, IO<SyncApplyReceipt>> apply, CancellationToken token)` is the decode-fold that drains the running pgoutput stream into commit-ordered op-log batches and acknowledges the applied LSN; its faults lift to `SyncFault.ReplicationFaulted`.
- Auto: `Options` fixes the non-obsolete pgoutput spelling — protocol V4, parallel streaming, binary wire; `Open` is the slot-attach stream with `StartReplication` taking a non-optional cancellation token; `CreatePgOutputReplicationSlot` creates the slot and acknowledgement rides `SetReplicationStatus` with the applied-and-flushed LSN after each committed apply, flushed by `SendStatusUpdate` or the 10-second feedback cadence; a decoded row's `Family` is `ColumnFamily.Scalar` (the pgoutput plane is the scalar-domain changefeed; the `crdt` lane rides the in-store `CrdtOp` payload, never a pgoutput leaf) and `ReplicaIdentity` selects which `Image` tuple the row carries (DEFAULT/FULL/USING-INDEX), never the merge lane — a missing replica identity decodes as an empty image; `RelationMessage` re-arrives on schema change as an in-stream schema fact that the pump fingerprints through `session.RelationFingerprint` and rejects as `SyncFault.SchemaMismatch` when it diverges from `session.SchemaFingerprint` (a drifted publication never decodes silently against a stale column map), a `TruncateMessage` emits one relation-wide `SyncOpKind.Truncate` tombstone per cleared relation through `Cleared` (a whole-table clear is a real op the peer must converge, never a dropped frame), streamed in-progress transactions stage keyed by xid and release on `StreamCommitMessage`, abort drops the stage, and commit order — not arrival order — is the op-log order; a `binary: true` tuple value decodes by its `TupleDataKind` — `Null` to a nil cell, `UnchangedToastedValue` to the carried-image sentinel, and `TextValue`/`BinaryValue` to the raw value bytes, so a binary-typed column is never coerced through `Get<string>`; because pgoutput carries no W3C traceparent, every decoded `OpLogEntry.Trace` is `TraceContext.Empty` (Persistence never fabricates a span the WAL did not carry), so `ReplicationPump.Resumed` starts an unparented apply `Activity` for a pgoutput batch while continuing the parent for an in-process batch whose head DID carry a captured `traceparent` (`TraceContext.Continue` re-creates the `ActivityContext` from the 16-byte trace-id with a fresh child span id and `isRemote: true`).
- Receipt: every pump run yields one `SyncApplyReceipt` carrying the acknowledged LSN; slot lifecycle and relation-schema evidence ride `ReceiptSinkPort` kinds.
- Packages: Npgsql, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new transport is one case row plus one dispatch arm, zero new surface; future PowerSync lands as one case behind its admission gate; future cr-sqlite lands as native-sqlite extension rows plus merge-law rows, never a case here; a new graph-checkout shape is one entry over `GraphDiff`, never a second diff algebra; a new pgoutput leaf decodes as one `Filed` arm on the same message switch.
- Boundary: HttpDelta rides the AppHost `OutboundHop` http-api keyed pipeline — retry, backoff, and hop deadlines are owned there and the database stays excluded from the hop law; the document-granular fallback is the RFC 6902 patch payload, subordinate to the op-log changefeed, and RFC 7386 merge-patch is the rejected form; SSE and WebSocket transports are rejected — server-stream verbs own streaming; the obsolete ulong-protocol `PgOutputReplicationOptions` constructor is the rejected spelling, and `TestDecodingOptions` is the named rejected alternative output plugin — its human-readable text frames carry no typed message family, so pgoutput's binary `PgOutputReplicationMessage` leaves stay the only decode path; `Decode` folds the pgoutput message family — `InsertMessage`, the `DefaultUpdateMessage`/`FullUpdateMessage`/`IndexUpdateMessage` update leaves, the `KeyDeleteMessage`/`FullDeleteMessage` delete leaves, `TruncateMessage` to one relation-wide `SyncOpKind.Truncate` tombstone per cleared relation, the `RelationMessage` schema fact `session.RelationFingerprint`-validated against `session.SchemaFingerprint`, and the stream control frames — over `RelationMessage` schema context into op-log entries behind the session delegate, the `ReplicationTuple` drained one value at a time so a large row never buffers contiguously, and the unchanged-TOAST third state decodes as a carried-image cell, never null; a transport bridge is two sessions composed through `SubtreeFetch` — transport state and op-log mechanics live here, the wire leg is the settled proto vocabulary; `GraphDiff` is the one set-difference diff-algebra — the closure-manifest minus the target `Holds` set yields the missing content keys — and the one `SubtreeFetch` entrypoint dials it, its `root` argument discriminating breadth so a true graph root copies the whole graph and a chosen sub-root checks out only that sub-root's descendants, a second walk-and-diff implementation or a parallel whole-graph entrypoint being the deleted form and `OpLog.TransferSet` the leaf the algebra composes; `SubtreeFetch` fetches the root entry and traverses its `Closure` manifest, so a checkout never pulls more than the diff and a client that holds a parent reuses its held keys through the same set-difference; the `SpeckleLikeDiff` arm `Offer` rides the same one diff algebra — it folds the pending set through `GraphDiff`/`OpLog.TransferSet` over the peer `HasObjects` membership to yield the missing content-key set, then hands that set to the `SpeckleSend` marshal delegate, so the inline `Closure.Add(...).Distinct()` fork is the deleted form and the offer-diff is the same set-difference every transport dials; the Speckle wire leg lives OUTSIDE-RHINO on the companion target where `Speckle.Sdk.Dependencies` repacks the Polly/channel/serialisation-V2 closure into one assembly, so the in-Rhino assembly composes only the `SpeckleLikeDiff` case and the marshal delegate slot and never references `Speckle.Sdk` or `Speckle.Objects`, the `Operations.Send` `rootObjId` projecting onto the offered root `UInt128 ContentKey` at the marshal seam with zero second identity — `Offer` enforces that projection, faulting `SyncFault.SpeckleMarshal` when the marshal-returned `RootContentKey` drifts from the offered head's locally-derived `ContentKey` rather than dropping the `SpeckleSend` tuple, and the returned `ConvertedReferences` detached-child count rides the fault detail; the wire leg of `GraphDiff`/`SubtreeFetch` is owned at `Compute/remote#PROTO_VOCABULARY` as the `GraphDiff`/`SubtreeFetch` message family — the walk-and-diff computation lives here and the wire shape lives there, so the remote-lane rpc dials this algebra over the transport and never re-implements the set-difference; the acknowledgement contract is at-least-once — apply, then `SetReplicationStatus` with only the durably-applied LSN, so redelivery after crash is the normal path and a lagging acknowledger is server-disk liability the provisioning verification fold gauges.
- Exemption: the `ReplicationPump.Pump` drain — the `await foreach` over the pgoutput stream, the message `switch`, the xid-keyed `staged`/`open` accumulation, the truncate-relations staging loop, and the `Drained` tuple loop — is the platform-forced replication-stream statement seam; commit-ordered batches re-enter the expression rail through `SyncMerge.Apply`.

```csharp signature
// The flow disposition of a sync exchange is ONE concept: peer-cardinality (fan-in/fan-out)
// and byte-flow initiation (pull/push) are the same axis read two ways, so one keyless owner
// carries both as the (Pulls, Pushes) policy pair the Exchange fold dispatches on — FanIn pulls,
// FanOut pushes, Bidirectional does both. The prior parallel SyncTopology + SyncDirection enums
// (one of them never read) are the collapsed forms; a new disposition is one row, not a second enum.
[SmartEnum]
public sealed partial class SyncFlow {
    public static readonly SyncFlow FanIn = new(pulls: true, pushes: false);
    public static readonly SyncFlow FanOut = new(pulls: false, pushes: true);
    public static readonly SyncFlow Bidirectional = new(pulls: true, pushes: true);

    public bool Pulls { get; }

    public bool Pushes { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SyncTransport {
    private SyncTransport(SyncFlow flow) => Flow = flow;

    public SyncFlow Flow { get; }

    public sealed record PgLogicalReplication(string Publication, string Slot, bool PublishGeneratedColumns, Duration IdleSlotTimeout, SyncFlow Flow) : SyncTransport(Flow);

    public sealed record HttpDelta(string Peer, SyncFlow Flow) : SyncTransport(Flow);

    public sealed record SpeckleLikeDiff(string Peer, SyncFlow Flow) : SyncTransport(Flow);
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

    public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry =>
            GraphDiff(entry, target.Holds)
                .TraverseM(source.Fetch).As()
                .Bind(missing => SyncMerge.Apply(target, missing)));

    // Flow.Pulls gates the pull-apply leg and Flow.Pushes gates the push leg, so FanOut pushes
    // without pulling, FanIn pulls without pushing, and Bidirectional does both in one exchange —
    // the disposition is the SyncFlow policy pair, never two collapsed branches that conflate push and bidi.
    private static IO<SyncApplyReceipt> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        (row.Flow.Pulls
            ? s.Pull(row.Peer, s.Cursor).Bind(segment =>
                segment.SchemaFingerprint == s.SchemaFingerprint
                    ? SyncMerge.Apply(s, segment.Entries).Map(receipt => receipt with { Cursor = segment.Cursor })
                    : IO.fail<SyncApplyReceipt>(new SyncFault.SchemaMismatch(s.SchemaFingerprint, segment.SchemaFingerprint)))
            : IO.pure(new SyncApplyReceipt(0L, 0L, 0L, 0L, 0L, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor, s.Correlation, s.Clocks.Now)))
            .Bind(receipt => row.Flow.Pushes
                ? IO.pure(s.Pending(receipt.Cursor)).Bind(pending =>
                    s.Push(row.Peer, pending).Map(cursor => receipt with { Pushed = pending.Count, Cursor = cursor }))
                : IO.pure(receipt));

    // The offered root's locally-derived content key must equal the rootObjId the marshal returns
    // (zero second identity); a divergence is a SpeckleMarshal fault, never a silently-dropped tuple.
    private static IO<SyncApplyReceipt> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        IO.pure(s.Pending(s.Cursor)).Bind(pending =>
            s.HasObjects(row.Peer, toSeq(pending.Fold(Seq<UInt128>(), static (set, entry) => set + GraphDiff(entry, static _ => false)).Distinct()))
                .Map(held => pending.Filter(entry => !held.Contains(entry.ContentKey)))
                .Bind(missing => s.SpeckleSend(row.Peer, missing)
                    .Bind(sent => missing.HeadOrNone().Map(head => head.ContentKey) is { IsSome: true, Case: UInt128 root } && root != sent.RootContentKey
                        ? IO.fail<SyncApplyReceipt>(new SyncFault.SpeckleMarshal(row.Peer, $"root-key-drift:{root}!={sent.RootContentKey}:refs={sent.ConvertedReferences}"))
                        : IO.pure(new SyncApplyReceipt(
                            Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Pushed: missing.Count,
                            QueueDepth: s.QueueDepth(), Conflicts: Seq<ConflictReceipt>(),
                            Cursor: s.Cursor with { Sequence = s.Cursor.Sequence + missing.Count },
                            Correlation: s.Correlation, At: s.Clocks.Now))));
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
                    case TruncateMessage cleared:
                        foreach (var relation in cleared.Relations) {
                            (staged, open) = Filed(staged, open, cleared.TransactionXid, Cleared(session, relation));
                        } break;
                    case RelationMessage schema when session.RelationFingerprint(schema) != session.SchemaFingerprint:
                        return Fin.Fail<SyncApplyReceipt>(new SyncFault.SchemaMismatch(session.SchemaFingerprint, session.RelationFingerprint(schema)));
                    case StreamCommitMessage streamed:
                        receipt = await Resumed(apply, new CommitBatch(streamed.TransactionEndLsn, staged.Find(streamed.TransactionXid).IfNone([])), token).ConfigureAwait(false);
                        staged = staged.Remove(streamed.TransactionXid);
                        link.SetReplicationStatus(streamed.TransactionEndLsn);
                        await link.SendStatusUpdate(token).ConfigureAwait(false); break;
                    case StreamAbortMessage aborted:
                        staged = staged.Remove(aborted.TransactionXid); break;
                    case CommitMessage committed:
                        receipt = await Resumed(apply, new CommitBatch(committed.TransactionEndLsn, open), token).ConfigureAwait(false);
                        open = [];
                        link.SetReplicationStatus(committed.TransactionEndLsn);
                        await link.SendStatusUpdate(token).ConfigureAwait(false); break;
                }
            }
            return Fin.Succ(receipt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            return Fin.Fail<SyncApplyReceipt>(new SyncFault.ReplicationFaulted(row.Slot, ex.Message));
        }
    }

    private static readonly ActivitySource Source = new("Rasm.Persistence.Replication");

    private static async Task<SyncApplyReceipt> Resumed(Func<CommitBatch, IO<SyncApplyReceipt>> apply, CommitBatch batch, CancellationToken token) {
        using var span = batch.Entries.HeadOrNone()
            .Bind(static head => head.Trace.Continue())
            .Match(
                Some: parent => Source.StartActivity("replication.apply", ActivityKind.Consumer, parent),
                None: () => Source.StartActivity("replication.apply", ActivityKind.Consumer));
        return await apply(batch).Run(EnvIO.New(token: token)).ConfigureAwait(false);
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
            Family: ColumnFamily.Scalar, Kind: kind, Codec: SnapshotCodec.MessagePackBinary,
            Payload: payload, Image: imageBytes, ContentKey: XxHash128.HashToUInt128(payload.Span),
            Trace: TraceContext.Empty,
            Closure: Seq<UInt128>(), Actor: session.StoreId.ToString(), OriginStoreId: session.StoreId,
            Physical: session.Clocks.Now, Logical: 0UL);
    }

    // A TRUNCATE clears a whole relation, so the op carries no row tuple — one relation-wide
    // Truncate tombstone per cleared relation, its content key the empty-payload digest.
    private static OpLogEntry Cleared(SyncSession session, RelationMessage relation) =>
        new(Sequence: 0L, EntityKind: $"{relation.Namespace}.{relation.RelationName}", EntityKey: relation.RelationName,
            Family: ColumnFamily.Scalar, Kind: SyncOpKind.Truncate, Codec: SnapshotCodec.MessagePackBinary,
            Payload: ReadOnlyMemory<byte>.Empty, Image: ReadOnlyMemory<byte>.Empty,
            ContentKey: XxHash128.HashToUInt128(ReadOnlySpan<byte>.Empty), Trace: TraceContext.Empty,
            Closure: Seq<UInt128>(), Actor: session.StoreId.ToString(), OriginStoreId: session.StoreId,
            Physical: session.Clocks.Now, Logical: 0UL);

    // Total over TupleDataKind: a binary-typed column under binary:true is read as raw bytes,
    // never coerced through Get<string>; the unchanged-TOAST third state is the carried-image sentinel.
    private static async ValueTask<ReadOnlyMemory<byte>> Drained(ReplicationTuple tuple, CancellationToken token) {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new MessagePackWriter(buffer);
        await foreach (var value in tuple.ConfigureAwait(false)) {
            switch (value.Kind) {
                case TupleDataKind.Null: writer.WriteNil(); break;
                case TupleDataKind.UnchangedToastedValue: writer.Write("<carried>"); break;
                case TupleDataKind.BinaryValue: writer.Write((await value.Get<ReadOnlyMemory<byte>>(token).ConfigureAwait(false)).Span); break;
                default: writer.Write(await value.Get<string>(token).ConfigureAwait(false)); break;
            }
        }
        writer.Flush();
        return buffer.WrittenMemory;
    }
}
```

| [INDEX] | [POLICY]                        | [VALUE]                                           | [BINDING]                                                                                                                            |
| :-----: | :------------------------------ | :------------------------------------------------ | :----------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | generated-column replication    | `PublishGeneratedColumns` on STORED columns only  | pairs the STORED generated-column schema law                                                                                         |
|  [02]   | idle slot reclamation           | config-sourced `IdleSlotTimeout`                  | server setting verified by the provisioning probe rows                                                                               |
|  [03]   | replication conflict statistics | subscription-stats read view                      | the live-replication research gate carries the columns                                                                               |
|  [04]   | session admission               | schema-fingerprint equality                       | mismatch is `SyncFault.SchemaMismatch` (C#), `SyncRejectionWire` (wire)                                                              |
|  [05]   | LSN acknowledgement cadence     | per-commit `SetReplicationStatus` + 10 s feedback | at-least-once apply-then-ack; redelivery is normal                                                                                   |
|  [06]   | graph checkout granularity      | `GraphDiff` set-difference over `Holds`           | one `SubtreeFetch` dials it; `root` discriminates whole-graph vs sub-root breadth; wire family at `Compute/remote#PROTO_VOCABULARY`  |
|  [07]   | Speckle offer diff              | `GraphDiff`/`OpLog.TransferSet` over `HasObjects` | `Offer` dials the one diff algebra; `SpeckleSend` marshals the missing set; `RootContentKey` drift faults `SpeckleMarshal`; SDK runs OUTSIDE-RHINO |
|  [08]   | Speckle marshal binding         | DI-resolved instance `IOperations.Send`/`Receive` | `AddSpeckleSdk` container; never static `Operations.Send`; `Base`/`DataObject` mapped to closed Rasm types at the seam               |

## [05]-[PRESENCE_AND_BLOB]

- Owner: `PresenceRow` and the `Presence` surface — ephemeral collaboration rows on the op-log shape; `AwarenessBeat` and the `Awareness` surface — the dedicated low-latency lossy awareness channel carrying cursor, selection, camera-frustum, active-node-focus, and follow-mode beats off the durable op-log; `WorkingSet` and the `Replication` surface — the partial-replication subgraph-checkout and working-set op-stream subscription; blob transfer composes the settled `BlobRemote` contract record, never a second blob shape.
- Entry: `public static IO<OpLogEntry> Beat(ReceiptSinkPort sink, ClockPolicy clocks, Guid storeId, SnapshotCodec codec, PresenceRow row, ReadOnlyMemory<byte> payload)` — presence is one changefeed row, never a transport; `public static Channel<AwarenessBeat> AwarenessLane(Atom<Seq<LaneLoss>> receipts)` and `public static Channel<OpLogEntry> Lane(Atom<Seq<LaneLoss>> receipts)` open the declared `AwarenessRow`/`PresenceLane` `LaneRow` rows through the settled `LaneRow.Open<T>` so both carry the `itemDropped` receipt and neither hand-rolls a `BoundedChannelOptions` (the inline-options call site is the deleted form); `public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ClockPolicy clocks, Option<string> session = default)` is the one polymorphic awareness constructor (the kind discriminates payload meaning, the five per-signal factories are the deleted form); `public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch, SyncCursor cursor, ClockPolicy clocks)` materializes a subgraph working set.
- Auto: presence rows expire at stamp plus `Ttl` and sweep on the heartbeat `ScheduleEntry` cadence; the offline queue is op-log accumulation draining on reconnect, with queue depth on every apply receipt; the awareness channel is a separate lossy lane from the durable op-log — cursor moves, selection halos, and camera frusta beat at a high cadence through the DropOldest channel and never touch the durable store, so a 60-Hz cursor stream never appends an op-log row, while `AwarenessKind` discriminates cursor, selection, camera, focus, and follow beats so one lossy lane carries every awareness signal and `AwarenessBeat.Supersedes` lets a slow reader discard a reordered beat by per-actor `Seq` lamport; a dropped beat is receipted `LaneLoss("<lane-awareness>", LossClass.EvictedOldest)` through the `LaneRow.Open<T>` `itemDropped` callback per the concurrency unreceipted-loss law (the `DropOldest` mode derives `LossClass.EvictedOldest` through `LaneRow.Loss`, never a hand-built loss class), distinct from the converging `Version/commits#CRDT_ALGEBRA` `EphemeralMap` a late-joining peer reconstructs from the op-log prefix; the working-set checkout resolves a `ReplicationQuery` (region/layer/view/type/closure-depth) into a content-key set then fetches only those entries, so a peer materializes one subgraph rather than the whole graph and subscribes its working-set op-stream to receive only changes touching its checked-out keys.
- Receipt: `Put` returns the stored `BlobRemote.Descriptor` as write evidence; sweep counts ride `ReceiptSinkPort` kinds; a dropped awareness or presence beat rides one `LaneLoss("<lane-awareness>"/"<lane-presence>", LossClass.EvictedOldest)` on the lane's receipt atom (both lanes lossy-but-accounted through `LaneRow.Open<T>`, never silent); a checkout rides `store.replication.checkout` carrying the resolved key count and the closure depth.
- Packages: NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new presence attribute is one `PresenceRow` field on the same wire shape; a new awareness signal is one `AwarenessKind` row on the same lossy lane; a new offline-queue bound is one policy value; a new frame size is one `FrameBytes` policy value; a new checkout dimension is one column on `ReplicationQuery`; zero new surface — a durable cursor table, a per-signal awareness channel, or an eager full-graph replication is the deleted form.
- Boundary: blob streams frame at 64 KiB with Crc32 per frame and XxHash128 whole-artifact identity — the settled ArtifactSync frame constants, and a second framing law is the rejected form; the whole-artifact identity accumulates incrementally — `FrameHash` feeds each 64 KiB frame into one `XxHash128` instance through `Append` and reads the final digest through `GetCurrentHashAsUInt128`, so a multi-gigabyte blob never materializes contiguously and `XxHash128.HashToUInt128` over a whole span is the deleted pattern for streamed payloads; every `BlobRemote.Descriptor` carries classification and retention columns, so an unclassified blob write is a typed rejection; presence (a durable-TTL editing-surface claim) rides the DropOldest lane and is never durable beyond its TTL, while awareness (cursor/selection/camera/focus/follow) rides a SEPARATE lossy channel that never appends a durable op-log row — so a high-cadence cursor stream never pressures the durable store and an awareness beat lost under backpressure is correct-by-design (a later `Seq` supersedes it via `Supersedes`) yet still receipted `EvictedOldest` so the lane closes its conservation identity, distinct from a durable op that must converge; the `AwarenessKind` axis carries the five collaboration signals on one lossy lane through the one `Awareness.Beat` constructor so a per-signal channel AND a per-signal factory are the deleted forms, and follow-mode is one peer subscribing another's camera beats under a shared `Session`; the working-set checkout is the partial-replication query-shape algebra — `ReplicationQuery` selects by region (spatial envelope), layer, view, type, and closure-depth so a peer checks out a subgraph rather than the whole graph, and the working-set manager subscribes the op-stream filtered to the checked-out keys so a peer receives only changes touching its working set, an eager full-graph replication or an unfiltered op-stream being the deleted form; the checkout rides the settled `SyncPump.SubtreeFetch`/`GraphDiff` set-difference so it fetches only the keys the peer lacks within the closure depth, and the `ReplicationQuery` lowers to a `Query/federation#ELEMENT_SET_ALGEBRA` `SetExpr` so the checkout selection is the one element-set currency, never a second query shape.

```csharp signature
public sealed record PresenceRow(string Actor, string EntityKind, string EntityKey, Instant ExpiresAt);

public static class Presence {
    public static readonly Duration Ttl = Duration.FromSeconds(45);

    public const int FrameBytes = 65536;

    // Presence and awareness reference the AppHost/concurrency LaneRow vocabulary by row, never
    // an inline BoundedChannelOptions — the mailbox-class DropOldest backpressure decision stays
    // recoverable from the declared row, and Open<T> wires the itemDropped receipt for both.
    public static readonly LaneRow PresenceLane =
        new("<lane-presence>", Some(64), BoundedChannelFullMode.DropOldest, SingleReader: true, Inline: false, Band: 2);

    public static IO<(UInt128 Identity, Seq<uint> FrameChecks)> FrameHash(Stream source) =>
        IO.lift(() => {
            var identity = new XxHash128();
            var checks = Seq<uint>();
            var frame = new byte[FrameBytes];
            for (var read = source.Read(frame); read > 0; read = source.Read(frame)) {
                identity.Append(frame.AsSpan(0, read));
                checks = checks.Add(Crc32.HashToUInt32(frame.AsSpan(0, read)));
            }
            return (identity.GetCurrentHashAsUInt128(), checks);
        });

    public static Channel<OpLogEntry> Lane(Atom<Seq<LaneLoss>> receipts) => PresenceLane.Open<OpLogEntry>(receipts);

    public static IO<OpLogEntry> Beat(ReceiptSinkPort sink, ClockPolicy clocks, Guid storeId, SnapshotCodec codec, PresenceRow row, ReadOnlyMemory<byte> payload) =>
        OpLog.Stamp(sink, clocks, cell => new OpLogEntry(
            Sequence: 0L,
            EntityKind: row.EntityKind,
            EntityKey: row.EntityKey,
            Family: ColumnFamily.Presence,
            Kind: SyncOpKind.Presence,
            Codec: codec,
            Payload: payload,
            Image: ReadOnlyMemory<byte>.Empty,
            ContentKey: XxHash128.HashToUInt128(payload.Span),
            Trace: cell.Trace,
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

// Lossy awareness signal: Seq is the per-actor lamport a slow reader uses to discard a
// reordered beat under DropOldest, and Session carries the follow-mode session a Follow beat
// joins (None for the other four kinds), so one record carries every collaboration signal.
public readonly record struct AwarenessBeat(
    string Actor,
    AwarenessKind Kind,
    ReadOnlyMemory<byte> Payload,
    Option<string> Session,
    ulong Seq,
    Instant At) {
    public bool Supersedes(AwarenessBeat held) => Actor == held.Actor && Kind == held.Kind && Seq > held.Seq;
}

public static class Awareness {
    // The lossy fan-out lane is a declared LaneRow — DropOldest derives LossClass.EvictedOldest
    // through LaneRow.Loss, and Open<T>'s itemDropped receipt makes every conflated beat
    // accounted, never silent unreceipted loss; capacity rides the row, never an inline option.
    public static readonly LaneRow AwarenessRow =
        new("<lane-awareness>", Some(256), BoundedChannelFullMode.DropOldest, SingleReader: false, Inline: false, Band: 3);

    public static Channel<AwarenessBeat> AwarenessLane(Atom<Seq<LaneLoss>> receipts) => AwarenessRow.Open<AwarenessBeat>(receipts);

    // One polymorphic constructor over AwarenessKind — the kind selects the payload meaning and
    // whether a follow-session rides, deleting the per-signal Cursor/Selection/Camera/Focus/Follow factories.
    public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ClockPolicy clocks, Option<string> session = default) =>
        new(actor, kind, payload, kind == AwarenessKind.Follow ? session : None, seq, clocks.Now);
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
|  [01]   | presence heartbeat | `Every` 15 s                                    | one `ScheduleEntry` row per active editing surface |
|  [02]   | presence TTL       | 45 s — 3 × heartbeat                            | `Ttl`                                              |
|  [03]   | presence lane      | `LaneRow` capacity 64, DropOldest, single reader | `PresenceLane`/`Lane` — `LaneRow.Open<T>` receipt |
|  [04]   | awareness lane     | `LaneRow` capacity 256, DropOldest, `itemDropped` | `AwarenessRow`/`AwarenessLane` — lossy-but-accounted, never durable |
|  [05]   | blob framing       | 64 KiB frames, Crc32 per frame, XxHash128 whole | `FrameHash` incremental `Append`/`GetCurrentHashAsUInt128` |

## [06]-[TS_PROJECTION]

- Owner: `OpLogEntryWire`, `SyncCursorWire`, `SyncSegmentWire`, `SyncRejectionWire`, `ConflictVerdictKind`, `ConflictReceiptWire`, `PresenceRowWire` — the sync wire surface the dashboard and companion peers decode; `HlcWire` is the `Version/commits#CRDT_WIRE` cell this page consumes (never re-minted) so the `ConflictReceiptWire` held/incoming stamps carry the one causal cell; the lossy `Awareness` lane is process-local (it never crosses the durable op-log, so it mints no wire shape) and cross-runtime live presence rides the converging `Version/commits#CRDT_WIRE` `CrdtOpWire` `beat`/`leave` arms, never a second lossy presence wire; the `crdt` column-family `OpLogEntryWire.payload` carries the `Version/commits#CRDT_WIRE` `CrdtOpWire` union, the single-mint owner this page consumes and never re-declares.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface row decoded through the same decoder options, zero new surface; the `crdt` op carries the owned `Version/commits#CRDT_WIRE` `CrdtOpWire` discriminated union on the same `OpLogEntryWire.payload` slot, never a second envelope and never a re-minted shape on this page.
- Boundary: 64-bit fields decode as bigint under useBigInt64; instants cross as ISO-8601 extended strings in the persisted temporal grammar and content keys cross as 16-byte binary — every shape on this surface is primitive-mapped with zero custom ext bytes, so no ExtensionCodec row exists; the `ConflictVerdict` discriminator crosses as the `verdict` case name and reconstructs as the `ConflictVerdictKind` literal union; the `Hlc` held/incoming stamps cross as the one `HlcWire` cell mirroring the C# `ConflictReceipt`, never split physical/logical scalars; the LSN crosses as the `pg_lsn` text spelling under the same wire grammar; the `trace` slot is an APPEND-ONLY optional field carrying the 16-byte `traceparent` and the opaque `tracestate` so a TS-web peer extract-and-continues the parent span — a browser INP span joins the op-log trace — never minting an independent root, the slot's optionality preserving the existing `[Key]` sequence the durability retirement law governs; `SyncRejectionWire` is the typed session-gate refusal for fingerprint mismatch; the `crdt` column-family op carries the `CrdtOpWire` discriminated union OWNED at `Version/commits#CRDT_WIRE` — the `op`-discriminated union whose HLC rides `HlcWire` and whose presence `beat`/`leave` arms surface the `EphemeralMap` live-multiplayer state — so the TS-web and Python legs decode the amended op-log payload on the one wire vocabulary, and a `kind`-discriminated re-mint on this page is the named cross-language drift defect; the amendment is recorded at the suite `CROSS_PACKAGE_LAWS` wire-law owner.

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
  kind: "upsert" | "delete" | "truncate" | "presence";
  codec: string;
  payload: Uint8Array;
  image: Uint8Array;
  contentKey: Uint8Array;
  closure: Uint8Array[];
  actor: string;
  origin: string;
  physical: string;
  logical: bigint;
  trace?: { traceparent: Uint8Array; tracestate: Uint8Array };
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

type ConflictVerdictKind = "LocalWin" | "RemoteWin" | "Merged" | "Rejected";

interface HlcWire {
  physical: string;
  logical: bigint;
}

interface ConflictReceiptWire {
  verdict: ConflictVerdictKind;
  entityKind: string;
  entityKey: string;
  columnFamily: string;
  held: HlcWire;
  heldActor: string;
  incoming: HlcWire;
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

The `crdt` column-family `OpLogEntryWire.payload` decodes through the `Version/commits#CRDT_WIRE` `CrdtOpWire` union — the single-mint `op`-discriminated owner carrying the `set | write | add | remove | increment | insertAfter | delete | maintain | beat | leave` arms over `HlcWire` cells. This page consumes that shape and declares no parallel `CrdtOpWire`.

## [07]-[RESEARCH]

- [LIVE_REPLICATION]: Assay provision PG18 evidence finalized the pgoutput message-family decode-fold, the `PgOutputProtocolVersion.V4` + `PgOutputStreamingMode.Parallel` + `binary: true` non-obsolete `Options` spelling, the `ReplicationTuple` drain over the `IsDBNull`/`IsUnchangedToastedValue` three-state, the xid-keyed staging with `StreamCommitMessage`/`StreamAbortMessage` release, and the `SetReplicationStatus`/`SendStatusUpdate` LSN acknowledgement. The genuine tier-3 residue is a live PG18 root with a configured publication and subscription emitting a running stream end-to-end: `publish_generated_columns` on STORED columns, the idle-slot timeout setting, and the subscription conflict-stat read-view columns observed against a second live writer, with the applied-LSN watermark verified durable across a forced reconnect window — the one probe that strictly requires a long-lived live-server replication session.
