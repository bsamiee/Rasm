# [PERSISTENCE_VERSION_LEDGER]

Rasm.Persistence projects the durable changefeed, the multi-writer convergence, and the offline sync engine FROM the Marten event substrate — Marten owns the append and the rebuildable views, this owner owns the merge semantics. The `OpLogEntry` is the changefeed projection of a Marten `IEvent<GraphEvent>`: a Marten `ISubscription` lifts each committed event into one `OpLogEntry` carrying the `ColumnFamily` merge-lane discriminant, the `Hlc` cell read off the event `Timestamp`, the `GraphDelta`/`CrdtOp` payload, the `ContentAddress` key, the `Closure` descendant manifest, and the `TraceContext` W3C trace slot — so the bespoke `OpLogEntry` store the prior engine kept is RETIRED beneath Marten and TimeTravel, StructuralMerge, the commit-DAG, and the CRDT all read this one projected feed. The merge law adjudicates each entry by its `ColumnFamily.Stance` — scalar `Lww`/`FirstWriter` through `Adjudicate`, the convergent `Crdt` lane into the settled `Version/commits#CRDT_ALGEBRA` `Crdt.Apply` — under the closed `SyncFault` rail; the three-case `SyncTransport` axis (cross-store HTTP delta, Speckle diff, partial-graph subtree checkout) widened by one `SyncFlow` disposition carries offline multi-writer convergence and IFC 3-way merge; presence rides an ephemeral lane and the dedicated lossy awareness channel. `ClockPolicy`, `ReceiptSinkPort`, the `DrainSpec`/`DrainQueue` bounded-lane vocabulary (the `DrainSurface.Open` lossy `DropOldest` channel with its `onDrop` drop receipt), and the `CORRELATION_SPINE` propagation fold arrive settled from AppHost; `Hlc`, `Crdt`, `CrdtWire`, `CommitNode` arrive from `Version/commits`; `ModelId`, `NodeId`, `GraphDelta` arrive from `Rasm.Element`.

## [01]-[INDEX]

- [01]-[CHANGEFEED]: the `OpLogEntry` projection of Marten events, HLC stamping, the trace slot, and the closure manifest.
- [02]-[MERGE_LAW]: LWW adjudication, conflict receipts, the idempotent apply fold, CRDT dispatch, and the conservation invariant.
- [03]-[SYNC_TRANSPORTS]: the three transport cases widened by one `SyncFlow` disposition, the subtree-checkout diff algebra, and the Speckle marshal seam.
- [04]-[PRESENCE]: ephemeral presence rows, the lossy awareness lane, and the working-set checkout.

## [02]-[CHANGEFEED]

- Owner: `SyncOpKind` `[SmartEnum<string>]` the write-verb axis; `ColumnFamily` `[SmartEnum<string>]` the merge-lane axis carrying its `MergeStance` policy so the lane selects its adjudication algebra as a vocabulary row; `TraceContext` `[ComplexValueObject]` the W3C trace-context carrier; `OpLogEntry` the changefeed record one Marten event projects to; `ChangefeedSubscription` the `Marten.Subscriptions.ISubscription` lifting each committed `IEvent<GraphEvent>` into an `OpLogEntry`; the `OpLog` stamp-and-diff surface.
- Cases: `SyncOpKind` is `Upsert | Delete | Truncate | Presence`; `ColumnFamily` is `Scalar(Lww) | Crdt(Crdt) | Geometry(Lww) | Presence(FirstWriter) | Commit(Lww) | Branch(Lww)`, a seventh lane being one row carrying its `MergeStance`, never a string compare in a consumer; the trace slot is a top-level envelope field, never inside `Payload`.
- Entry: `public Task SubscribeAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, ISubscriptionController controller, CancellationToken token)` is the Marten subscription that projects each committed `GraphEvent` into an `OpLogEntry`; `public static OpLogEntry Project(IEvent<GraphEvent> e)` lifts the Marten event — the `GraphDelta` body becomes the `Payload`, the event `Timestamp` becomes the `Hlc` cell, the event `CorrelationId`/`CausationId` populate the trace slot, the `ContentAddress` keys it; `public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds)` projects the closure-minus-held missing-key set.
- Auto: the changefeed is a Marten async projection / subscription so a commit on any model stream reaches every peer over the one feed, NEVER a trigger-based second write path and never a bespoke op-log table — the prior `OpLogEntry` SQLite store is RETIRED beneath Marten (`H11`); a `crdt`-lane entry carries its `CrdtOpWire` `Payload` so the convergent merge reads the delta; the `Closure` is the model's descendant content-key manifest so transfer is set-difference, never a tree-walk; the trace slot reads `Activity.Current` once at the originating write and a Marten-projected entry carries the event's stored correlation context.
- Receipt: changefeed position and queue depth ride `SyncApplyReceipt`; the projected-segment evidence rides `ReceiptSinkPort`.
- Packages: Marten (`ISubscription`/`IEvent`/`ISubscriptionController`/`IDocumentOperations`), NodaTime, System.IO.Hashing, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Diagnostics.DiagnosticSource, BCL inbox.
- Growth: a new synced concern is one `SyncOpKind` verb, one `ColumnFamily` lane carrying its `MergeStance`, or one entity-kind value on the same `OpLogEntry` shape; zero new surface — a per-entity-kind outbox table, a bespoke op-log store beneath Marten, or a per-lane string literal in the merge fold is the deleted form.
- Boundary: the changefeed is PROJECTED from Marten events — the op-log IS the audit artifact, the change feed, and the sync feed as folds over the one Marten stream, never a second store (`H11` — Marten is the append substrate beneath, the engine projects from its events); the `crdt` lane's `Payload` is the `Version/commits#CRDT_WIRE` `CrdtOp` delta, so the trace slot is a top-level envelope field beside `ContentKey`, NOT inside `Payload`, and triggers no `CrdtOpWire` schema fork; Persistence only READS `Activity.Current` and projects to the `TraceContext` value, never re-minting the propagator (the AppHost correlation spine owns it); the 16-byte `traceparent` validates once at admission so the interior never re-parses; `OpLogEntry` carries NO `CorrelationId` field — correlation rides the sync session and receipts, so the trace slot is a genuinely new envelope field; a Marten-projected entry that crossed a runtime carries `TraceContext.Empty` when the event held no traceparent (Persistence never fabricates a span the substrate did not carry), and the apply continues the parent only when one exists.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SyncOpKind {
    public static readonly SyncOpKind Upsert = new("upsert", tombstone: false, wholeRelation: false);
    public static readonly SyncOpKind Delete = new("delete", tombstone: true, wholeRelation: false);
    public static readonly SyncOpKind Truncate = new("truncate", tombstone: true, wholeRelation: true);
    public static readonly SyncOpKind Presence = new("presence", tombstone: false, wholeRelation: false);
    public bool Tombstone { get; }
    public bool WholeRelation { get; }
    private SyncOpKind(string key, bool tombstone, bool wholeRelation) : this(key) => (Tombstone, WholeRelation) = (tombstone, wholeRelation);
}

[SmartEnum]
public sealed partial class MergeStance {
    public static readonly MergeStance Lww = new(convergent: false);
    public static readonly MergeStance Crdt = new(convergent: true);
    public static readonly MergeStance FirstWriter = new(convergent: false);
    public bool Convergent { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnFamily {
    public static readonly ColumnFamily Scalar = new("scalar", MergeStance.Lww, durable: true);
    public static readonly ColumnFamily Crdt = new("crdt", MergeStance.Crdt, durable: true);
    public static readonly ColumnFamily Geometry = new("geometry", MergeStance.Lww, durable: true);
    public static readonly ColumnFamily Presence = new("presence", MergeStance.FirstWriter, durable: false);
    public static readonly ColumnFamily Commit = new("commit", MergeStance.Lww, durable: true);
    public static readonly ColumnFamily Branch = new("branch", MergeStance.Lww, durable: true);
    public MergeStance Stance { get; }
    public bool Durable { get; }
    private ColumnFamily(string key, MergeStance stance, bool durable) : this(key) => (Stance, Durable) = (stance, durable);
}

[ComplexValueObject]
public sealed partial class TraceContext {
    public static readonly TraceContext Empty = new(ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty);
    public ReadOnlyMemory<byte> Traceparent { get; }
    public ReadOnlyMemory<byte> Tracestate { get; }
    public bool HasParent => Traceparent.Length == 16;
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ReadOnlyMemory<byte> traceparent, ref ReadOnlyMemory<byte> tracestate) {
        if (traceparent.Length is not (0 or 16)) validationError = new ValidationError($"<traceparent-length:{traceparent.Length}>");
    }
    public static TraceContext Capture() =>
        Activity.Current is { } a && a.Context.TraceId != default ? Project(a.Context, a.TraceStateString) : Empty;
    public static TraceContext Project(ActivityContext context, string? traceState) {
        var span = new byte[16];
        context.TraceId.CopyTo(span.AsSpan(0, 16));
        return Create(span, Encoding.ASCII.GetBytes(traceState ?? string.Empty));
    }
    public Option<ActivityContext> Continue() =>
        HasParent ? Some(new ActivityContext(ActivityTraceId.CreateFromBytes(Traceparent.Span), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded,
            Encoding.ASCII.GetString(Tracestate.Span) is { Length: > 0 } s ? s : null, isRemote: true)) : None;
}

public sealed record OpLogEntry(
    long Sequence, ModelId Model, string EntityKey, ColumnFamily Family, SyncOpKind Kind, SnapshotCodec Codec,
    ReadOnlyMemory<byte> Payload, ReadOnlyMemory<byte> Image, UInt128 ContentKey, TraceContext Trace, Seq<UInt128> Closure,
    string Actor, Guid OriginStoreId, Instant Physical, ulong Logical) {
    public Hlc Stamp => new(Physical, Logical);
}

public sealed record SyncCursor(Guid OriginStoreId, long Sequence, Instant Physical, ulong Logical) {
    public static readonly SyncCursor Genesis = new(Guid.Empty, 0L, Instant.MinValue, 0UL);
}

public sealed class ChangefeedSubscription(Func<OpLogEntry, IO<Unit>> sink) : SubscriptionBase {
    public override async Task<IChangeListener> ProcessEventsAsync(EventRange range, ISubscriptionController controller, IDocumentOperations operations, CancellationToken token) {
        foreach (var e in range.Events.OfType<IEvent<GraphEvent>>())
            await sink(OpLog.Project(e)).RunAsync(EnvIO.New(token: token)).ConfigureAwait(false);
        return NullChangeListener.Instance;
    }
}

public static class OpLog {
    public static OpLogEntry Project(IEvent<GraphEvent> e) {
        var payload = CrdtWire.EncodeCompanion(e.Data.Body.AsCrdtOp());
        return new OpLogEntry(e.Sequence, ModelId.Create(e.StreamId), e.StreamKey ?? e.StreamId.ToString(),
            e.Data.Lifecycle == EventLifecycle.Retired ? ColumnFamily.Geometry : ColumnFamily.Crdt,
            e.Data.Lifecycle == EventLifecycle.Retired ? SyncOpKind.Delete : SyncOpKind.Upsert,
            SnapshotCodec.MessagePackBinary, payload, ReadOnlyMemory<byte>.Empty,
            XxHash128.HashToUInt128(payload.Span),
            e.CorrelationId is { Length: 32 } cid ? TraceContext.Create(Convert.FromHexString(cid), ReadOnlyMemory<byte>.Empty) : TraceContext.Empty,
            Seq<UInt128>(), e.Headers.TryGetValue("actor", out var actor) ? actor?.ToString() ?? "" : "",
            Guid.Empty, Instant.FromDateTimeOffset(e.Timestamp), (ulong)e.Version);
    }

    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ClockPolicy clocks, Func<(Instant Physical, ulong Logical, TraceContext Trace), OpLogEntry> build) =>
        IO.lift(() => (Wall: clocks.Now, Trace: TraceContext.Capture()))
            .Map(captured => (Cell: sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, captured.Wall)), captured.Trace))
            .Map(stamped => build((stamped.Cell.Physical, stamped.Cell.Logical, stamped.Trace)));

    public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds) =>
        entry.Closure.Add(entry.ContentKey).Filter(key => !holds(key));
}
```

| [INDEX] | [POLICY]              | [VALUE]                          | [BINDING]                                                  |
| :-----: | :-------------------- | :------------------------------- | :-------------------------------------------------------- |
|  [01]   | changefeed source     | Marten `ISubscription` over events | the bespoke op-log store is retired beneath Marten        |
|  [02]   | lane → merge stance   | `ColumnFamily.Stance`            | dispatch reads the lane row, never a `"crdt"` string compare |
|  [03]   | HLC cell              | the Marten event `Timestamp`     | one stamp for op-log, CRDT merge, commit cell, wire       |
|  [04]   | trace slot            | top-level `TraceContext` field   | never inside `Payload`; no `CrdtOpWire` fork              |

## [03]-[MERGE_LAW]

- Owner: `ConflictReceipt`, `ConflictVerdict` `[SmartEnum<string>]`, `ConflictResult`, `SyncApplyReceipt` (with its `Conserves` invariant), `SyncFault` the closed `[Union]` fault family deriving from `Expected` in the 8250 band, `SyncSession` the one session capsule of policy values and delegate rows; `SyncMerge` the fold surface routing each `OpLogEntry` by its `ColumnFamily.Stance` — `Lww`/`FirstWriter` through `Adjudicate`, `Crdt` into `Crdt.Apply`.
- Cases: 4 verdict rows on `ConflictVerdict` — `LocalWin | RemoteWin | Merged | Rejected` — collapsed into one `ConflictResult(Verdict, Receipt)`; `Rejected` is reachable only on an equal `(stamp, origin)` with divergent content (the causal fork the `Apply` fold lifts to `SyncFault.Forked` and halts on), never a soft conflict bucket; the `SyncFault` family is `SchemaMismatch | ReplicationFaulted | SpeckleMarshal | TransferDecode | Unconserved | Forked`.
- Entry: `public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming)` carries commit effects; replay converges idempotently and the receipt proves applied, skipped, conflicted, converged, and pushed counts under `Conserves`, an `IO.fail` `SyncFault.Unconserved` when the batch does not close.
- Receipt: `ConflictReceipt` is the typed fork evidence the `SyncFault.Forked` halt carries and the inspector projects; `SyncApplyReceipt` is the per-run apply evidence.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one `MergeStance` row feeding `Held` resolution; a fifth `ConflictVerdict` row is the named defect; a new fault cause is one `SyncFault` case; a new replicated data type is a `Version/commits#CRDT_ALGEBRA` `CrdtField` case dispatched by this fold, never a fifth scalar arm.
- Boundary: LWW per column family is the default — `Held` resolves the competing local entry per model and family, content-key equality adjudicates `LocalWin` (idempotent replay), an absent competitor adjudicates `Merged` through `Fresh` whose held slots carry the `Hlc.Zero` absence sentinel, and an equal `(stamp, origin)` with divergent content is the causal fork which `Apply` halts as the epoch-class `SyncFault.Forked` carrying the two divergent content keys, never a soft conflict that counts and continues; HLC ordering ties break on origin store id so adjudication is deterministic across peers; the `crdt` column family routes its `Payload` through `Crdt.Apply` so a concurrent edit converges by the join-semilattice least-upper-bound rather than scalar LWW (the LWW `Adjudicate` surviving only as the `LwwRegister` arm) — operator-confirmed KEEP, the multi-writer offline + IFC 3-way merge substrate; the `SpeckleSend`/`SpeckleReceive` delegates are the marshal seam binding the DI-resolved instance `IOperations.Send`/`Receive`, projecting the returned `rootObjId` content hash onto the `ContentKey` (zero second identity) and mapping the inbound `Base`/`DataObject` graph to closed Rasm op-log entries at the seam, the SDK boundary faults lifting once into `SyncFault.SpeckleMarshal`; a `SyncEngine` service class is the rejected form — the fold and the dispatch rows own the engine.

```csharp signature
public readonly record struct ConflictReceipt(ModelId Model, string EntityKey, ColumnFamily Family, Hlc Held, string HeldActor, Hlc Incoming, string IncomingActor, CorrelationId Correlation, Instant At);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConflictVerdict {
    public static readonly ConflictVerdict LocalWin = new("LocalWin", applies: false);
    public static readonly ConflictVerdict RemoteWin = new("RemoteWin", applies: true);
    public static readonly ConflictVerdict Merged = new("Merged", applies: true);
    public static readonly ConflictVerdict Rejected = new("Rejected", applies: false);
    public bool Applies { get; }
}

public readonly record struct ConflictResult(ConflictVerdict Verdict, ConflictReceipt Receipt);

[Union]
public abstract partial record SyncFault : Expected, IValidationError<SyncFault> {
    private SyncFault(string detail, int code) : base(detail, code, None) { }
    public static SyncFault Create(string message) => new ReplicationFaulted(string.Empty, message);
    public sealed record SchemaMismatch(ulong Local, ulong Remote) : SyncFault($"{Local}:{Remote}", 8251);
    public sealed record ReplicationFaulted(string Slot, string Cause) : SyncFault($"{Slot}:{Cause}", 8252);
    public sealed record SpeckleMarshal(string Peer, string Class) : SyncFault($"{Peer}:{Class}", 8253);
    public sealed record TransferDecode(string Peer, string Cause) : SyncFault($"{Peer}:{Cause}", 8254);
    public sealed record Unconserved(long Batch, long Settled) : SyncFault($"{Batch}!={Settled}", 8255);
    public sealed record Forked(ConflictReceipt Receipt, UInt128 Held, UInt128 Incoming) : SyncFault($"{Receipt.EntityKey}@{Receipt.Incoming.Physical}:{Held}!={Incoming}", 8256);
}

public readonly record struct SyncApplyReceipt(long Applied, long Skipped, long Conflicted, long Converged, long Pushed, long QueueDepth, Seq<ConflictReceipt> Conflicts, SyncCursor Cursor, CorrelationId Correlation, Instant At) {
    public long Settled => Applied + Skipped + Conflicted + Converged;
    public bool Conserves(long batch) => batch == Settled;
}

public sealed record SyncSession(
    ClockPolicy Clocks, ReceiptSinkPort Sink, CorrelationId Correlation, Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, CancellationToken Token,
    Func<UInt128, bool> Holds, Func<OpLogEntry, Option<OpLogEntry>> Held, Func<OpLogEntry, IO<Unit>> Commit, Func<OpLogEntry, IO<Unit>> Converge,
    Func<SyncCursor, Seq<OpLogEntry>> Pending, Func<long> QueueDepth, Func<UInt128, IO<OpLogEntry>> Fetch,
    Func<string, SyncCursor, IO<(ulong SchemaFingerprint, Seq<OpLogEntry> Entries, SyncCursor Cursor)>> Pull,
    Func<string, Seq<OpLogEntry>, IO<SyncCursor>> Push, Func<string, Seq<UInt128>, IO<Seq<UInt128>>> HasObjects,
    Func<string, Seq<OpLogEntry>, IO<(UInt128 RootContentKey, long ConvertedReferences)>> SpeckleSend,
    Func<string, UInt128, IO<Seq<OpLogEntry>>> SpeckleReceive);

public static class SyncMerge {
    public static ConflictReceipt Receipt(SyncSession session, OpLogEntry held, OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, held.Stamp, held.Actor, incoming.Stamp, incoming.Actor, session.Correlation, session.Clocks.Now);

    static ConflictReceipt Fresh(SyncSession session, OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, Hlc.Zero, string.Empty, incoming.Stamp, incoming.Actor, session.Correlation, session.Clocks.Now);

    public static ConflictResult Adjudicate(SyncSession session, OpLogEntry incoming) =>
        session.Held(incoming) is { IsSome: true, Case: OpLogEntry held }
            ? incoming.ContentKey == held.ContentKey
                ? new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming))
                : (incoming.Stamp, incoming.OriginStoreId).CompareTo((held.Stamp, held.OriginStoreId)) switch {
                    > 0 when incoming.Family.Stance == MergeStance.FirstWriter => new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming)),
                    > 0 => new ConflictResult(ConflictVerdict.RemoteWin, Receipt(session, held, incoming)),
                    < 0 => new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming)),
                    _ => new ConflictResult(ConflictVerdict.Rejected, Receipt(session, held, incoming)),
                }
            : new ConflictResult(ConflictVerdict.Merged, Fresh(session, incoming));

    public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming) =>
        incoming.Fold(
            IO.pure((Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Conflicts: Seq<ConflictReceipt>())),
            (acc, entry) => acc.Bind(counts => entry.Family.Stance.Convergent
                ? session.Converge(entry).Map(_ => counts with { Converged = counts.Converged + 1L })
                : Adjudicate(session, entry) is var result && result.Verdict.Applies
                    ? session.Commit(entry).Map(_ => counts with { Applied = counts.Applied + 1L })
                    : result.Verdict == ConflictVerdict.Rejected
                        ? IO.fail<(long, long, long, long, Seq<ConflictReceipt>)>(new SyncFault.Forked(result.Receipt, session.Held(entry).Map(static h => h.ContentKey).IfNone(UInt128.Zero), entry.ContentKey))
                        : IO.pure(counts with { Skipped = counts.Skipped + 1L })))
            .Map(c => new SyncApplyReceipt(c.Applied, c.Skipped, c.Conflicted, c.Converged, Pushed: 0L, session.QueueDepth(), c.Conflicts, session.Cursor, session.Correlation, session.Clocks.Now))
            .Bind(receipt => receipt.Conserves(incoming.Count) ? IO.pure(receipt) : IO.fail<SyncApplyReceipt>(new SyncFault.Unconserved(incoming.Count, receipt.Settled)));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                              | [BINDING]                                                  |
| :-----: | :-------------------- | :-------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | scalar default        | LWW by `(Hlc, OriginStoreId)`                       | total order, deterministic across peers                   |
|  [02]   | crdt lane             | `Crdt.Apply` join-semilattice                       | converges by merge; operator-confirmed KEEP               |
|  [03]   | causal fork           | equal `(stamp, origin)` divergent content           | `SyncFault.Forked` halts merge; never a soft conflict     |
|  [04]   | conservation          | `Conserves(batch)` over the settled sum             | a breach is `SyncFault.Unconserved` failing the rail      |

## [04]-[SYNC_TRANSPORTS]

- Owner: `SyncFlow` the keyless disposition carrying the `(Pulls, Pushes)` policy pair; `SyncTransport` `[Union]`; the `SyncPump` dispatch surface with the `SubtreeFetch` graph-checkout bridge and the `Offer` Speckle-diff arm; `GraphDiff` the named set-difference diff-algebra `SubtreeFetch` and `Offer` both dial.
- Cases: 3 transport cases — `HttpDelta`, `SpeckleLikeDiff`, `SubtreeCheckout` — widened by the one `SyncFlow` field whose `Pulls`/`Pushes` policy pair the `Exchange` fold reads; fan-in/fan-out/bidirectional are `SyncFlow` rows, never new transport cases.
- Entry: `public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport)` is one total state-threaded dispatch; `public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds)` projects the missing-key set; `public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root)` is the partial-graph checkout over the diff.
- Auto: intra-cluster replication is Marten's own daemon (`DaemonMode.HotCold`) over the shared PostgreSQL, so this transport axis is the CROSS-store / offline lane (a disconnected editor, a Speckle hub, a peer holding a subgraph), never a re-implementation of single-cluster replication; `HttpDelta` pulls a cursor-bounded segment and pushes the pending set gated by `SyncFlow`; `SubtreeCheckout` reads the root entry and traverses its `Closure` manifest so a checkout never pulls more than the diff; `SpeckleLikeDiff` folds the pending set through `GraphDiff` over the peer `HasObjects` membership and hands the missing set to the `SpeckleSend` marshal.
- Receipt: every transport run yields one `SyncApplyReceipt`; the subtree-checkout transfer count rides the same receipt.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, Speckle.Sdk (companion, outside-Rhino), BCL inbox.
- Growth: a new transport is one case plus one dispatch arm; a new graph-checkout shape is one entry over `GraphDiff`, never a second diff algebra; zero new surface.
- Boundary: `HttpDelta` rides the AppHost `OutboundHop` keyed pipeline — retry, backoff, and hop deadlines are owned there and the database stays excluded from the hop law; the document-granular fallback is the RFC 6902 patch payload subordinate to the changefeed; `GraphDiff` is the one set-difference diff-algebra (the closure manifest minus the target `Holds` set) and `SubtreeFetch` dials it, its `root` argument discriminating breadth so a true graph root copies the whole graph and a chosen sub-root checks out only its descendants, a second walk-and-diff being the deleted form; the `SpeckleLikeDiff` `Offer` rides the same one diff algebra and the wire leg lives OUTSIDE-RHINO on the companion target where `Speckle.Sdk.Dependencies` repacks the closure, so the in-Rhino assembly composes only the case and the marshal delegate slot and never references `Speckle.Sdk`/`Speckle.Objects`, the `Operations.Send` `rootObjId` projecting onto the offered root `ContentKey` at the marshal seam with zero second identity (`Offer` faults `SyncFault.SpeckleMarshal` when the returned root key drifts from the offered head's local key); the wire leg of `GraphDiff`/`SubtreeFetch` is owned at `Compute/Runtime/channels#PROTO_VOCABULARY` so the remote rpc dials this algebra and never re-implements the set-difference.

```csharp signature
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
    public sealed record HttpDelta(string Peer, SyncFlow Flow) : SyncTransport(Flow);
    public sealed record SpeckleLikeDiff(string Peer, SyncFlow Flow) : SyncTransport(Flow);
    public sealed record SubtreeCheckout(string Peer, UInt128 Root, SyncFlow Flow) : SyncTransport(Flow);
}

public static class SyncPump {
    public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport) =>
        transport.Switch(
            state: session,
            httpDelta: static (s, row) => Exchange(s, row),
            speckleLikeDiff: static (s, row) => Offer(s, row),
            subtreeCheckout: static (s, row) => s.Fetch(row.Root).Bind(entry => GraphDiff(entry, s.Holds).TraverseM(s.Fetch).As().Bind(missing => SyncMerge.Apply(s, missing))));

    public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds) => OpLog.TransferSet(root, holds);

    public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry => GraphDiff(entry, target.Holds).TraverseM(source.Fetch).As().Bind(missing => SyncMerge.Apply(target, missing)));

    static IO<SyncApplyReceipt> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        (row.Flow.Pulls
            ? s.Pull(row.Peer, s.Cursor).Bind(segment => segment.SchemaFingerprint == s.SchemaFingerprint
                ? SyncMerge.Apply(s, segment.Entries).Map(receipt => receipt with { Cursor = segment.Cursor })
                : IO.fail<SyncApplyReceipt>(new SyncFault.SchemaMismatch(s.SchemaFingerprint, segment.SchemaFingerprint)))
            : IO.pure(new SyncApplyReceipt(0L, 0L, 0L, 0L, 0L, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor, s.Correlation, s.Clocks.Now)))
        .Bind(receipt => row.Flow.Pushes
            ? IO.pure(s.Pending(receipt.Cursor)).Bind(pending => s.Push(row.Peer, pending).Map(cursor => receipt with { Pushed = pending.Count, Cursor = cursor }))
            : IO.pure(receipt));

    static IO<SyncApplyReceipt> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        IO.pure(s.Pending(s.Cursor)).Bind(pending =>
            s.HasObjects(row.Peer, toSeq(pending.Fold(Seq<UInt128>(), static (set, entry) => set + GraphDiff(entry, static _ => false)).Distinct()))
                .Map(held => pending.Filter(entry => !held.Contains(entry.ContentKey)))
                .Bind(missing => s.SpeckleSend(row.Peer, missing).Bind(sent =>
                    missing.HeadOrNone().Map(h => h.ContentKey) is { IsSome: true, Case: UInt128 root } && root != sent.RootContentKey
                        ? IO.fail<SyncApplyReceipt>(new SyncFault.SpeckleMarshal(row.Peer, $"root-key-drift:{root}!={sent.RootContentKey}:refs={sent.ConvertedReferences}"))
                        : IO.pure(new SyncApplyReceipt(0L, 0L, 0L, 0L, missing.Count, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor with { Sequence = s.Cursor.Sequence + missing.Count }, s.Correlation, s.Clocks.Now)))));
}
```

| [INDEX] | [POLICY]                 | [VALUE]                                | [BINDING]                                                  |
| :-----: | :----------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | intra-cluster replication | Marten daemon `HotCold`               | this axis is the cross-store/offline lane only            |
|  [02]   | graph checkout           | `GraphDiff` set-difference over `Holds` | `SubtreeFetch` dials it; `root` discriminates breadth     |
|  [03]   | Speckle marshal          | DI-resolved instance `IOperations`     | runs outside-Rhino; `rootObjId` → `ContentKey`; drift faults |
|  [04]   | http delta               | AppHost `OutboundHop` pipeline         | database excluded from the hop law                        |

## [05]-[PRESENCE]

- Owner: `PresenceRow` and the `Presence` surface — ephemeral collaboration rows on the changefeed shape; `AwarenessBeat`/`AwarenessKind` and the `Awareness` surface — the dedicated low-latency lossy awareness channel carrying cursor, selection, camera-frustum, focus, and follow beats off the durable changefeed; `WorkingSet`/`ReplicationQuery` and the `Replication` surface — the partial-replication subgraph checkout.
- Entry: `public static Fin<DrainQueue<AwarenessBeat>> AwarenessLane(DrainSpec spec, Atom<Seq<AwarenessBeat>> dropped)` opens the declared `DropOldest` `DrainSpec` row through the AppHost `DrainSurface.Open<AwarenessBeat>` so the lane carries its `onDrop` drop receipt and never hand-rolls `BoundedChannelOptions` — the `DrainQueue<AwarenessBeat>.Pipe` case carries the `Channel<AwarenessBeat>` the awareness writer drives, and a `DropOldest` row that opens without an `onDrop` receipt fails on the `Fin` rail; `public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ClockPolicy clocks, Option<string> session = default)` is the one polymorphic awareness constructor (the kind discriminates payload meaning, the per-signal factories being the deleted form); `public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch, SyncCursor cursor, ClockPolicy clocks)` materializes a subgraph working set.
- Auto: presence rows expire at stamp plus `Ttl` and sweep on the heartbeat `ScheduleEntry` cadence; the awareness channel is a separate lossy lane from the durable changefeed — cursor moves, selection halos, and camera frusta beat at high cadence through the `DropOldest` channel and never touch the durable store, while `AwarenessKind` discriminates the beats and `AwarenessBeat.Supersedes` lets a slow reader discard a reordered beat by per-actor `Seq` lamport; a dropped beat is receipted through the `DrainSurface.Open` `onDrop` callback into the loss atom, distinct from the converging `Version/commits#CRDT_ALGEBRA` `EphemeralMap` a late-joining peer reconstructs from the changefeed prefix; the working-set checkout resolves a `ReplicationQuery` (region/layer/view/type/closure-depth) into a content-key set then fetches only those entries so a peer materializes one subgraph rather than the whole graph.
- Receipt: a presence beat rides `store.presence.beat`; an awareness drop rides the `DrainSurface.Open` `onDrop` receipt into the loss atom; a working-set checkout rides `store.replication.checkout` carrying the subgraph size.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, System.Threading.Channels, BCL inbox.
- Growth: a new awareness signal is one `AwarenessKind` row; a new checkout dimension is one field on `ReplicationQuery`; zero new surface — a per-signal awareness factory, a durable presence write, or a second lossy lane is the deleted form.
- Boundary: presence is one ephemeral changefeed row, never a transport; the lossy awareness lane is the 60-Hz fire-and-forget channel that never appends a durable entry, while the converging `EphemeralMap` is the durable self-expiring presence map a late-joining peer reconstructs — two distinct presence forms (lossy live vs convergent reconstructible); the working-set checkout subscribes its op-stream to receive only changes touching its checked-out keys so a partial-replication peer never pulls the whole graph.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AwarenessKind {
    public static readonly AwarenessKind Cursor = new("cursor");
    public static readonly AwarenessKind Selection = new("selection");
    public static readonly AwarenessKind Camera = new("camera");
    public static readonly AwarenessKind Focus = new("focus");
    public static readonly AwarenessKind Follow = new("follow");
}

public readonly record struct AwarenessBeat(string Actor, AwarenessKind Kind, ReadOnlyMemory<byte> Payload, ulong Seq, Instant At, Option<string> Session) {
    public bool Supersedes(AwarenessBeat prior) => Actor == prior.Actor && Kind == prior.Kind && Seq > prior.Seq;
}

public readonly record struct PresenceRow(string Actor, ReadOnlyMemory<byte> State, Instant At, Duration Ttl) {
    public bool Live(Instant now) => now - At < Ttl;
}

public readonly record struct ReplicationQuery(Option<string> Region, Option<string> Layer, Option<string> View, Option<string> Kind, int ClosureDepth);

public readonly record struct WorkingSet(Seq<UInt128> Keys, Seq<OpLogEntry> Entries, SyncCursor Cursor, Instant At);

public static class Awareness {
    public static Fin<DrainQueue<AwarenessBeat>> AwarenessLane(DrainSpec spec, Atom<Seq<AwarenessBeat>> dropped) =>
        spec.Open<AwarenessBeat>(Some<Action<AwarenessBeat>>(beat => ignore(dropped.Swap(seq => seq.Add(beat)))));

    public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ClockPolicy clocks, Option<string> session = default) =>
        new(actor, kind, payload, seq, clocks.Now, session);

    public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch, SyncCursor cursor, ClockPolicy clocks) =>
        from keys in resolve(query)
        from entries in fetch(keys)
        select new WorkingSet(keys, entries, cursor, clocks.Now);
}
```

| [INDEX] | [POLICY]            | [VALUE]                            | [BINDING]                                                  |
| :-----: | :------------------ | :--------------------------------- | :-------------------------------------------------------- |
|  [01]   | lossy awareness     | `DropOldest` `DrainSpec` lane, `onDrop` receipt | never a durable changefeed row; distinct from `EphemeralMap` |
|  [02]   | presence ttl        | stamp + `Ttl`, heartbeat sweep     | one ephemeral row, never a transport                      |
|  [03]   | working-set checkout| `ReplicationQuery` → key set       | one subgraph, never the whole graph                       |
