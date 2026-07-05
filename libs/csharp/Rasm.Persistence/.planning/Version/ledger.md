# [PERSISTENCE_VERSION_LEDGER]

Rasm.Persistence projects the durable changefeed, the multi-writer convergence, and the offline sync engine FROM the Marten event substrate — Marten owns the append and the rebuildable views, this owner owns the merge semantics. The `OpLogEntry` is the changefeed projection of a Marten `IEvent<GraphEvent>`: a Marten `SubscriptionBase` lifts each committed event into one `OpLogEntry` carrying the `ColumnFamily` merge-lane discriminant, the `Hlc` cell read off the event `Timestamp`/`Version`, the codec-encoded `Payload` whose shape the `(Family, Codec)` pair discriminates (a structural `GraphDelta` on the `geometry` lane that `Project` produces; a `CrdtOp` on the `crdt` lane and a `CommitNode` on the `commit` lane their owners `Stamp`), the content key over the encoded payload, the `Closure` descendant geometry manifest, and the `TraceSlot` W3C trace slot (the stored 16-byte trace-id, distinct from the AppHost `TraceContext` propagation fold) — so the bespoke `OpLogEntry` store the prior engine kept is RETIRED beneath Marten and TimeTravel, StructuralMerge, the commit-DAG, and the CRDT all read this one projected feed. The merge law adjudicates each entry by its `ColumnFamily.Stance` — scalar `Lww`/`FirstWriter` through `Adjudicate`, the convergent `Crdt` lane into the settled `Version/commits#CRDT_ALGEBRA` `Crdt.Apply` — under the closed `SyncFault` rail; the three-case `SyncTransport` axis (cross-store HTTP delta, Speckle diff, partial-graph subtree checkout) widened by one `SyncFlow` disposition carries offline multi-writer convergence and IFC 3-way merge; presence rides an ephemeral lane and the dedicated lossy awareness channel; the durable lanes are the CDC row source the `Version/egress` pump drains past the `Store/coordination#OUTBOX_CURSOR`, and one `ReplayWindow` windowed read serves the AppUi edit-intent replay, the AppHost determinism neutral-log read, and that egress drain as three parameterizations of one case. `ReceiptSinkPort`, the `DrainSpec`/`DrainQueue` bounded-lane vocabulary (the `DrainSurface.Open` lossy `DropOldest` channel with its `onDrop` drop receipt), and the `CORRELATION_SPINE` propagation fold arrive settled from AppHost; the clock marks, correlation `Guid`, and tenant ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame — a `ClockPolicy`/`CorrelationId` parameter on any signature here is the named strata inversion; `Hlc`, `Crdt`, `CrdtWire`, `CommitNode` arrive from `Version/commits`; `ModelId`, `NodeId`, `GraphDelta` arrive from `Rasm.Element`; `ContentHash.Of` (the one federation hasher) arrives from the `Rasm` kernel.

## [01]-[INDEX]

- [01]-[CHANGEFEED]: the `OpLogEntry` projection of Marten events, HLC stamping, the trace slot, the closure manifest, and the `ReplayWindow` windowed read.
- [02]-[MERGE_LAW]: LWW adjudication, conflict receipts, the idempotent apply fold, CRDT dispatch, and the conservation invariant.
- [03]-[SYNC_TRANSPORTS]: the three transport cases widened by one `SyncFlow` disposition, the subtree-checkout diff algebra, and the Speckle marshal seam.
- [04]-[PRESENCE]: ephemeral presence rows, the lossy awareness lane, and the working-set checkout.

## [02]-[CHANGEFEED]

- Owner: `SyncOpKind` `[SmartEnum<string>]` the write-verb axis; `ColumnFamily` `[SmartEnum<string>]` the merge-lane axis carrying its `MergeStance` AND its `SnapshotCodec` policy columns so the lane selects its adjudication algebra and its payload codec as one vocabulary row (`OpLogEntry.Codec` is the `Family.Codec` accessor — a stored codec field beside the lane row is the deleted split-brain); `TraceSlot` `[ComplexValueObject]` the changefeed W3C trace-id carrier (the slot value, NOT the AppHost `TraceContext` propagation fold); `OpLogEntry` the changefeed record one Marten event projects to; `ReplayWindow` the one windowed-read parameterization (origin/entity/model/family/sequence); `ChangefeedSubscription` the `Marten.Subscriptions.SubscriptionBase` folding each delivered `EventRange` into ONE batched `Seq<OpLogEntry>` drain; the `OpLog` project-stamp-replay surface.
- Cases: `SyncOpKind` is `Upsert | Delete | Truncate | Presence`; `ColumnFamily` is the six-row lane family `Scalar(Lww, JsonStj) | Crdt(Crdt, MessagePackBinary) | Geometry(Lww, JsonStj) | Presence(FirstWriter, MessagePackBinary) | Commit(Lww, MessagePackBinary) | Branch(Lww, MessagePackBinary)`, each row carrying its `MergeStance` and `SnapshotCodec` so a consumer dispatches on `Family.Stance` and decodes by `Family.Codec`, never a string compare; a new lane is one row carrying both columns; the trace slot is a top-level envelope field, never inside `Payload`.
- Entry: `public override Task<IChangeListener> ProcessEventsAsync(EventRange range, ISubscriptionController controller, IDocumentOperations operations, CancellationToken token)` is the `SubscriptionBase` override that folds the WHOLE delivered range into one `Seq<OpLogEntry>` and drains it once (per-event awaits inside the range are the deleted form), returning `NullChangeListener.Instance`; `public static OpLogEntry Project(IEvent<GraphEvent> e)` lifts one Marten event — the seam `GraphDelta` body is the codec-encoded `Payload` on the structural `geometry` lane, the event `Timestamp`/`Version` are the `Hlc` cell, the `origin`/`actor` headers populate `OriginStoreId`/`Actor`, a carried 16-byte trace-id populates the trace slot, and the content key is `ContentHash.Of` over the encoded payload; `public static Seq<OpLogEntry> Replay(Seq<OpLogEntry> feed, ReplayWindow window)` is the ONE windowed changefeed read — the AppUi `Collab/Editing` per-doc edit-intent replay (`ReplayWindow.ForEntity`), the AppHost `Runtime/determinism` neutral-log read (`ReplayWindow.ForOrigin`), and the `Version/egress` durable-ops CDC drain (`ReplayWindow.DurableOps`) are three parameterizations of one case, never three read surfaces; `public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds)` projects the closure-minus-held missing-key set.
- Auto: the changefeed is a Marten async subscription so a commit on any model stream reaches every peer over the one feed, NEVER a trigger-based second write path and never a bespoke op-log table — the prior `OpLogEntry` SQLite store is RETIRED beneath Marten (`H11`); a `Project`ed entry is a STRUCTURAL `geometry`-lane change carrying the codec-encoded `GraphDelta`, adjudicated by `(Hlc, OriginStoreId)` LWW, while the convergent `crdt` lane and the `commit` lane carry a `CrdtOp`/`CommitNode` `Payload` minted through `Stamp` by their owners (`Version/commits#CRDT_ALGEBRA`/`#COMMIT_DAG`), so the changefeed is one `OpLogEntry` shape over three payload kinds the `(Family, Codec)` pair discriminates, never a `GraphDelta` mis-routed into the CRDT convergence path; the `Closure` folds the delta's object-node `RepresentationContentHash` values so transfer is set-difference over the descendant geometry manifest, never a tree-walk; the trace slot reads `Activity.Current` once at the originating write and a Marten-projected entry carries the event's stored correlation context.
- Receipt: changefeed position and queue depth ride `SyncApplyReceipt`; the projected-segment evidence rides `ReceiptSinkPort`.
- Packages: Marten (`SubscriptionBase`/`ProcessEventsAsync`/`EventRange`/`ISubscriptionController`/`IDocumentOperations`/`IChangeListener`/`NullChangeListener`/`IEvent<T>`), Rasm.Element (`GraphDelta`/`Node`/`Node.Object`/`RepresentationContentHash`), Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher; a direct `XxHash128` call site is the deleted spelling), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Diagnostics.DiagnosticSource, BCL inbox.
- Growth: a new synced concern is one `SyncOpKind` verb, one `ColumnFamily` lane carrying its `MergeStance`/`SnapshotCodec` columns, or one payload kind keyed by the lane row's `Codec`; a new windowed-read consumer is one `ReplayWindow` parameterization, never a new read surface; zero new surface — a per-entity-kind outbox table, a bespoke op-log store beneath Marten, a per-payload-kind parallel record, or a per-lane string literal in the merge fold is the deleted form.
- Boundary: the changefeed is PROJECTED from Marten events — the op-log IS the audit artifact, the change feed, and the sync feed as folds over the one Marten stream, never a second store (`H11` — Marten is the append substrate beneath, the engine projects from its events); a `Project`ed `GraphEvent` entry is the structural `geometry`-lane `GraphDelta` (the durable graph change is an LWW structural delta, NOT a CRDT op — `Project` produces no `crdt`-lane entry), while the `crdt` lane's `Payload` is the `Version/commits#CRDT_WIRE` `CrdtOp` delta a CRDT mutation `Stamp`s and the `commit` lane's a `CommitNode`, so the trace slot is a top-level envelope field beside `ContentKey`, NOT inside `Payload`, and triggers no `CrdtOpWire` schema fork; `OriginStoreId` reads the Marten `origin` header (the SAME slot `Version/timetravel#TIME_TRAVEL` `OriginOf` reads) so the LWW `(Hlc, OriginStoreId)` tie-break is deterministic across peers and never a fabricated zero collapsing every origin to one bucket; Persistence only READS `Activity.Current` and projects to the `TraceSlot` value, never re-minting the propagator (the AppHost `TraceContext` correlation-spine fold owns `Inject`/`Extract`/`Continue` — the `TraceSlot` is named distinctly so the Persistence trace SLOT never collides with that propagation surface); the 16-byte trace-id admits once through the TOTAL `TraceSlot.FromHex` (the span `Convert.FromHexString(source, destination, out charsConsumed, out bytesWritten)` `OperationStatus` overload gated on `Done`, never the throwing array-returning `Convert.FromHexString(string)` that would fault the projection fold on a 32-char non-hex correlation) so the interior never re-parses; `OpLogEntry` carries NO correlation field — correlation rides the sync session frame and receipts, so the trace slot is a genuinely new envelope field; a Marten-projected entry whose stored `CorrelationId` is absent, wrong-length, OR not valid hex carries `TraceSlot.Empty` (Persistence never fabricates a span the substrate did not carry and never throws on an arbitrary correlation value), and the apply continues the parent only when one exists; the durable lanes (`Family.Durable`) are the exactly-once CDC row source the `Version/egress` pump drains past the `Store/coordination#OUTBOX_CURSOR` — `ReplayWindow.DurableOps` is that drain's parameterization, and the presence/awareness lane (`durable: false`) stays the lossy `DrainSurface` channel, NEVER the exactly-once CDC envelope.

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
    public static readonly ColumnFamily Scalar = new("scalar", MergeStance.Lww, SnapshotCodec.JsonStj, durable: true);
    public static readonly ColumnFamily Crdt = new("crdt", MergeStance.Crdt, SnapshotCodec.MessagePackBinary, durable: true);
    public static readonly ColumnFamily Geometry = new("geometry", MergeStance.Lww, SnapshotCodec.JsonStj, durable: true);
    public static readonly ColumnFamily Presence = new("presence", MergeStance.FirstWriter, SnapshotCodec.MessagePackBinary, durable: false);
    public static readonly ColumnFamily Commit = new("commit", MergeStance.Lww, SnapshotCodec.MessagePackBinary, durable: true);
    public static readonly ColumnFamily Branch = new("branch", MergeStance.Lww, SnapshotCodec.MessagePackBinary, durable: true);
    public MergeStance Stance { get; }
    public SnapshotCodec Codec { get; }
    public bool Durable { get; }
    private ColumnFamily(string key, MergeStance stance, SnapshotCodec codec, bool durable) : this(key) => (Stance, Codec, Durable) = (stance, codec, durable);
}

// The changefeed trace SLOT — the 16-byte W3C trace-id plus the tracestate string the `OpLogEntry` envelope
// carries, distinct from the AppHost `Observability/telemetry#CORRELATION_SPINE` `TraceContext` PROPAGATION fold
// (the `Inject`/`Extract`/`Continue` gRPC-metadata surface): this owner only READS `Activity.Current` and stores
// the slot, never re-minting the propagator. `TraceId` holds the 16-byte trace-id (not the full 55-char W3C
// `traceparent` header), so `HasParent` is the 16-byte presence test the `Continue` reconstruction reads.
[ComplexValueObject]
public sealed partial class TraceSlot {
    public static readonly TraceSlot Empty = new(ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty);
    public ReadOnlyMemory<byte> TraceId { get; }
    public ReadOnlyMemory<byte> Tracestate { get; }
    public bool HasParent => TraceId.Length == 16;
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ReadOnlyMemory<byte> traceId, ref ReadOnlyMemory<byte> tracestate) {
        if (traceId.Length is not (0 or 16)) validationError = new ValidationError($"<trace-id-length:{traceId.Length}>");
    }
    public static TraceSlot Capture() =>
        Activity.Current is { } a && a.Context.TraceId != default ? From(a.Context, a.TraceStateString) : Empty;
    // The TOTAL hex admission a daemon-projected entry takes — the stored Marten `CorrelationId` is decoded ONLY when it
    // is a valid 32-char (16-byte) hex trace-id; a wrong-length OR a 32-char NON-hex correlation string yields `Empty`,
    // never the throwing array-returning `Convert.FromHexString(string)` inside the projection fold (which would fault
    // the subscription daemon on a legitimately-arbitrary correlation value). The span `Convert.FromHexString(source,
    // destination, out charsConsumed, out bytesWritten)` `OperationStatus` overload is the non-throwing decode.
    public static TraceSlot FromHex(string? correlation) {
        if (correlation is not { Length: 32 }) { return Empty; }
        var span = new byte[16];
        // `OperationStatus.Done` holds iff every one of the 32 chars decoded into the 16-byte span; `InvalidData`
        // reports a partial decode through the status value, never a throw.
        return Convert.FromHexString(correlation, span, out _, out _) == System.Buffers.OperationStatus.Done
            ? Create(span, ReadOnlyMemory<byte>.Empty)
            : Empty;
    }
    public static TraceSlot From(ActivityContext context, string? traceState) {
        var span = new byte[16];
        context.TraceId.CopyTo(span.AsSpan(0, 16));
        return Create(span, Encoding.ASCII.GetBytes(traceState ?? string.Empty));
    }
    public Option<ActivityContext> Continue() =>
        HasParent ? Some(new ActivityContext(ActivityTraceId.CreateFromBytes(TraceId.Span), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded,
            Encoding.ASCII.GetString(Tracestate.Span) is { Length: > 0 } s ? s : null, isRemote: true)) : None;
}

// The one changefeed record every Marten event projects to. `Payload` is the codec-encoded change whose shape the
// `(Family, Codec)` pair discriminates — a `geometry`/`scalar`-lane `GraphDelta`, a `crdt`-lane `CrdtOp`, or a
// `commit`-lane `CommitNode` — so the merge fold reads the lane row, never a second per-payload record. The `Hlc`
// `Stamp` orders adjudication and `(Hlc, OriginStoreId)` breaks ties deterministically across peers; `Closure` is
// the descendant geometry content-key manifest the transfer set-difference reads. No before-image field: the merge
// adjudicates on the `(Hlc, OriginStoreId)` stamp and content-key equality, and the conflict evidence is the typed
// `ConflictReceipt`, so a stored before-image is dead weight the delta log never reads.
public sealed record OpLogEntry(
    long Sequence, ModelId Model, string EntityKey, ColumnFamily Family, SyncOpKind Kind,
    ReadOnlyMemory<byte> Payload, UInt128 ContentKey, TraceSlot Trace, Seq<UInt128> Closure,
    string Actor, Guid OriginStoreId, Instant Physical, ulong Logical) {
    public Hlc Stamp => new(Physical, Logical);
    // Family-derived, never stored: the lane row IS the codec authority, so a payload can never carry a codec
    // its lane disagrees with — the stored-field split-brain is the deleted form.
    public SnapshotCodec Codec => Family.Codec;
}

public sealed record SyncCursor(Guid OriginStoreId, long Sequence, Instant Physical, ulong Logical) {
    public static readonly SyncCursor Genesis = new(Guid.Empty, 0L, Instant.MinValue, 0UL);
}

// The ONE windowed changefeed read: origin/entity/model/family/sequence-parameterized so the AppUi
// `Collab/Editing` per-doc edit-intent replay (`ForEntity`), the AppHost `Runtime/determinism` neutral-log
// read (`ForOrigin`), and the `Version/egress` durable-ops CDC drain (`DurableOps` — every `Family.Durable`
// lane past the outbox cursor) are three parameterizations of one case, never three read surfaces.
public readonly record struct ReplayWindow(Option<Guid> Origin, Option<string> EntityKey, Option<ModelId> Model, Seq<ColumnFamily> Families, long AfterSequence, int Take) {
    public static ReplayWindow ForEntity(string entityKey, long afterSequence, int take) => new(None, Some(entityKey), None, Seq<ColumnFamily>(), afterSequence, take);
    public static ReplayWindow ForOrigin(Guid origin, long afterSequence, int take) => new(Some(origin), None, None, Seq<ColumnFamily>(), afterSequence, take);
    public static ReplayWindow DurableOps(long afterSequence, int take) => new(None, None, None, toSeq(ColumnFamily.Items.Filter(static f => f.Durable)), afterSequence, take);
    public bool Admits(OpLogEntry entry) =>
        (entry.Sequence > AfterSequence)
        && Origin.Map(o => o == entry.OriginStoreId).IfNone(true)
        && EntityKey.Map(k => string.Equals(k, entry.EntityKey, StringComparison.Ordinal)).IfNone(true)
        && Model.Map(m => m == entry.Model).IfNone(true)
        && (Families.IsEmpty || Families.Contains(entry.Family));
}

public sealed class ChangefeedSubscription(Func<Seq<OpLogEntry>, IO<Unit>> drain) : SubscriptionBase {
    // ONE batched fold per delivered range: project every committed event into one Seq, drain once — the
    // per-event await inside the range (one RunAsync per event) is the deleted form.
    public override async Task<IChangeListener> ProcessEventsAsync(EventRange range, ISubscriptionController controller, IDocumentOperations operations, CancellationToken token) {
        await drain(toSeq(range.Events.OfType<IEvent<GraphEvent>>()).Map(OpLog.Project)).RunAsync(EnvIO.New(token: token)).ConfigureAwait(false);
        return NullChangeListener.Instance;
    }
}

public static class OpLog {
    // The structural changefeed projection of one Marten `GraphEvent`. The body is a `Element/graph#STREAM_GRAIN`
    // `GraphDelta` (the durable graph change), NOT a `CrdtOp` — so the `Payload` is the codec-encoded delta on the
    // STRUCTURAL `geometry` lane (`MergeStance.Lww`, adjudicated by `(Hlc, OriginStoreId)`), never the `crdt`
    // convergence lane. The `crdt`/`commit` lanes carry a `CrdtOp`/`CommitNode` `Payload` minted through `Stamp`
    // by their owners (`Version/commits#CRDT_ALGEBRA`/`#COMMIT_DAG`), so `Project` produces ONLY `geometry`-lane
    // entries — `GraphCreated`/`GraphRevised` -> `Upsert`, `GraphRetired` -> `Delete`. The HLC cell rides the
    // Marten event `Timestamp` (physical) and `Version` (logical, the stream-monotone counter), matching the
    // `Version/timetravel#TIME_TRAVEL` blame reconstruction; `OriginStoreId` reads the `origin` header (the SAME
    // slot `TimeTravel.OriginOf` reads, never a fabricated zero that would collapse the LWW tie-break); `Actor`
    // the `actor` header; the trace slot continues a 16-byte trace-id the event carried, else `TraceSlot.Empty`.
    // `Closure` folds the delta's object-node `RepresentationContentHash` values (every `Body`/`Axis`/`FootPrint`
    // geometry key the delta references) so `TransferSet`/`GraphDiff` resolve the descendant geometry manifest a
    // `SubtreeCheckout` transfers — never a tree-walk, never an empty manifest.
    // The `geometry`-lane `Payload` encodes the SEAM `GraphDelta` through the `Element/codec#CODEC_AXIS` `JsonStj`
    // row, NOT `MessagePackBinary`: the seam `GraphDelta` (and its `Seq<Node>`/`Option<Header>`/`Node`/`Relationship`
    // `[Union]` members + LanguageExt `Seq`/`Option` + NodaTime) is SOURCE-GEN-registered on the STJ `ElementJson`
    // context (reachable transitively from `[JsonSerializable(typeof(GraphEvent))]`) and carries NO `[MessagePackObject]`
    // attribute (the seam stays library-neutral), so the `messagepack` row — whose `GeneratedMessagePackResolver` finds
    // only `[MessagePackObject]` owners and whose `StandardResolver` rejects an attribute-free `Seq<Node>` — cannot
    // encode it; MessagePack on the seam graph types is the deleted phantom (`Element/codec#CODEC_AXIS`). The CRDT lane's
    // `CrdtOpWire` is the `[MessagePack.Union]` Persistence-owned wire type the `messagepack` row DOES cover.
    public static OpLogEntry Project(IEvent<GraphEvent> e) {
        ReadOnlyMemory<byte> payload = ColumnFamily.Geometry.Codec.Serialize(typeof(GraphDelta), e.Data.Body);
        return new OpLogEntry(e.Sequence, ModelId.Create(e.StreamId), e.StreamKey ?? e.StreamId.ToString(),
            ColumnFamily.Geometry,
            e.Data.Lifecycle == EventLifecycle.Retired ? SyncOpKind.Delete : SyncOpKind.Upsert,
            payload, ContentHash.Of(payload.Span),
            TraceSlot.FromHex(e.CorrelationId),
            Closure(e.Data.Body), HeaderValue(e, "actor"),
            HeaderValue(e, "origin") is { Length: > 0 } o && Guid.TryParse(o, out var origin) ? origin : Guid.Empty,
            Instant.FromDateTimeOffset(e.Timestamp), (ulong)e.Version);
    }

    // The ONE windowed changefeed read (DECISION seam :187): filter by the window's origin/entity/model/family
    // slots past the sequence cursor, ordered, bounded — the composition root binds `feed` to the durable row
    // source (a cursor-bounded Marten segment), and every windowed consumer dials a `ReplayWindow` value.
    public static Seq<OpLogEntry> Replay(Seq<OpLogEntry> feed, ReplayWindow window) =>
        toSeq(feed.Filter(window.Admits).OrderBy(static entry => entry.Sequence).Take(window.Take));

    // The descendant content-key manifest of one delta: every representation content hash on the added/revised
    // object nodes (the `Body`/`Axis`/`Box`/`FootPrint` geometry the delta introduces), distinct so the transfer
    // set is the geometry a peer must hold to materialize the change. A non-`Object` node (PropertySet/Material/…)
    // carries no geometry, so it contributes nothing; the closure is the geometry blob set, not the node set.
    static Seq<UInt128> Closure(GraphDelta delta) =>
        toSeq((delta.AddedNodes + delta.RevisedNodes.Map(static r => r.After))
            .Choose(static n => Optional(n as Node.Object))
            .SelectMany(static o => o.Representations.ByIdentifier.Values)
            .Distinct());

    static string HeaderValue(IEvent<GraphEvent> e, string key) =>
        e.Headers is { } h && h.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;

    // The authoring-path mint for a NON-structural lane entry (a `crdt`-lane `CrdtOp` payload, a `commit`-lane
    // `CommitNode` payload): the ONE HLC stamp from the `ReceiptSinkPort.Hlc` atom (no second clock) plus the
    // captured trace context, the `build` continuation closing over the lane/payload its owner supplies. The
    // wall clock rides the injected `ProjectionContext` frame ([A.1]), never a `ClockPolicy` parameter.
    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, ProjectionContext frame, Func<(Instant Physical, ulong Logical, TraceSlot Trace), OpLogEntry> build) =>
        IO.lift(() => (Wall: frame.Now(), Trace: TraceSlot.Capture()))
            .Map(captured => (Cell: sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, captured.Wall)), captured.Trace))
            .Map(stamped => build((stamped.Cell.Physical, stamped.Cell.Logical, stamped.Trace)));

    public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds) =>
        entry.Closure.Add(entry.ContentKey).Filter(key => !holds(key));
}
```

| [INDEX] | [POLICY]              | [VALUE]                          | [BINDING]                                                  |
| :-----: | :-------------------- | :------------------------------- | :-------------------------------------------------------- |
|  [01]   | changefeed source     | Marten `SubscriptionBase.ProcessEventsAsync` | the bespoke op-log store is retired beneath Marten        |
|  [02]   | projected lane        | `geometry` (LWW) for a `GraphEvent` | `Project` produces a structural delta, never a `crdt` op  |
|  [03]   | payload shape         | `(Family, Codec)` discriminates  | `Codec` is the lane row's column, `Family`-derived, never stored |
|  [04]   | windowed read         | `ReplayWindow` origin/entity/family/window | AppUi edit-intent, AppHost determinism, egress CDC drain — one case |
|  [05]   | lane → merge stance   | `ColumnFamily.Stance`            | dispatch reads the lane row, never a `"crdt"` string compare |
|  [06]   | HLC cell              | event `Timestamp` + `Version`    | one stamp for op-log, CRDT merge, commit cell, wire       |
|  [07]   | origin tie-break      | the Marten `origin` header       | LWW `(Hlc, OriginStoreId)` deterministic; never a zero    |
|  [08]   | trace slot            | top-level `TraceSlot` field      | never inside `Payload`; distinct from AppHost `TraceContext` fold |

## [03]-[MERGE_LAW]

- Owner: `ConflictReceipt`, `ConflictVerdict` `[SmartEnum<string>]`, `ConflictResult`, `SyncApplyReceipt` (the `IValidityEvidence` conservation receipt — the kernel `ValidityClaim.All` fold over its own carried `Batch`), `SyncFault` the closed `[Union]` fault family deriving from the KERNEL `Rasm.Domain.Expected` (parameterless `: base()` + per-case `Code`/`Message`/`Category` `Switch`, NOT `LanguageExt.Common.Expected`; no `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in) in the 825x band, `SyncSession` the one session capsule carrying the injected `ProjectionContext` frame plus the delegate rows (`Commit`/`Truncate`/`Converge`/`Held`/…); `SyncMerge` the fold surface routing each `OpLogEntry` by its `ColumnFamily.Stance` — `Lww`/`FirstWriter` through `Adjudicate`, `Crdt` into `Crdt.Apply`, a winning whole-relation `Truncate` through the `Truncate` delegate.
- Cases: 4 verdict rows on `ConflictVerdict` — `LocalWin | RemoteWin | Merged | Rejected` — collapsed into one `ConflictResult(Verdict, Receipt, Conflicted, Held)` where `Conflicted` distinguishes a genuine divergence (an HLC-resolved `LocalWin`/`RemoteWin` over differing content) from an idempotent-replay `LocalWin` (content-equal) or a fresh `Merged`, and `Held` carries the held content key the fork fault reads without a second lookup; `Rejected` is reachable only on an equal `(stamp, origin)` with divergent content (the causal fork the `Apply` fold lifts to `SyncFault.Forked` and halts on), never a soft conflict bucket; the `SyncFault` family is `SchemaMismatch | ReplicationFaulted | SpeckleMarshal | TransferDecode | Unconserved | Forked`.
- Entry: `public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming)` carries commit effects; replay converges idempotently and the receipt proves applied, skipped, conflicted, converged, and pushed counts under the receipt's own `IValidityEvidence` conservation fold (the carried `Batch` plus exactly-one-counter-per-entry accounting make `IsValid` the exact settled-sum proof), an auto-resolved LWW divergence counting as `Conflicted` and recording its `ConflictReceipt` into `Conflicts` (whether the winner was committed or the local kept), an `IO.fail` `SyncFault.Unconserved` when the batch does not close.
- Receipt: `ConflictReceipt` is the typed fork evidence the `SyncFault.Forked` halt carries and the inspector projects; `SyncApplyReceipt` is the per-run apply evidence.
- Packages: Rasm (`Rasm.Domain` `Expected` — the federation fault base; `IValidityEvidence`/`ValidityClaim` — the receipt-validity floor `SyncApplyReceipt` registers through), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one `MergeStance` row feeding `Held` resolution; a fifth `ConflictVerdict` row is the named defect; a new fault cause is one `SyncFault` case; a new replicated data type is a `Version/commits#CRDT_ALGEBRA` `CrdtField` case dispatched by this fold, never a fifth scalar arm.
- Boundary: LWW per column family is the default — `Held` resolves the competing local entry per model and family, content-key equality adjudicates `LocalWin` (idempotent replay — `Conflicted: false`, a pure skip), an absent competitor adjudicates `Merged` through `Fresh` whose held slots carry the `Hlc.Zero` absence sentinel, an HLC-resolved `LocalWin`/`RemoteWin` over differing content is a genuine divergence (`Conflicted: true`) the fold counts as `Conflicted` and whose `ConflictReceipt` it records even when the winner commits, and an equal `(stamp, origin)` with divergent content is the causal fork which `Apply` halts as the epoch-class `SyncFault.Forked` carrying the two divergent content keys, never a soft conflict that counts and continues; the `FirstWriter` (`Presence`) lane is EARLIEST-wins, the INVERSE comparison direction of the LWW latest-wins default, so the older `(stamp, origin)` wins regardless of arrival order — the `Adjudicate` `(comparison, isFirstWriter)` tuple-`switch` flips both the newer-incoming and the older-incoming arm for FirstWriter, never the LWW-only direction that would silently keep a later first-writer-lane row over the genuine first writer; the `Conflicted`/`Conflicts` audit fields are thus exact (every auto-resolved divergence recorded, an idempotent replay never miscounted as a conflict), not an always-empty placeholder; HLC ordering ties break on origin store id so adjudication is deterministic across peers; the `crdt` column family routes its `Payload` through `Crdt.Apply` so a concurrent edit converges by the join-semilattice least-upper-bound rather than scalar LWW (the LWW `Adjudicate` surviving only as the `LwwRegister` arm) — the multi-writer offline + IFC 3-way merge substrate; the `SpeckleSend`/`SpeckleReceive` delegates are the marshal seam binding the DI-resolved instance `IOperations.Send`/`Receive`, projecting the returned `rootObjId` content hash onto the `ContentKey` (zero second identity) and mapping the inbound `Base`/`DataObject` graph to closed Rasm op-log entries at the seam, the SDK boundary faults lifting once into `SyncFault.SpeckleMarshal`; a winning whole-relation entry (`Kind.WholeRelation` — the `Truncate` verb) commits through the session `Truncate` delegate clearing the whole `(Model, Family)` relation (the `Held` resolver answers the relation's LATEST entry for a whole-relation verb, so the truncate still adjudicates `(Hlc, OriginStoreId)` LWW against the relation head — the policy bit selects the relation-wide commit lane, never a dead flag); a `SyncEngine` service class is the rejected form — the fold and the dispatch rows own the engine.

```csharp signature
public readonly record struct ConflictReceipt(ModelId Model, string EntityKey, ColumnFamily Family, Hlc Held, string HeldActor, Hlc Incoming, string IncomingActor, Guid Correlation, Instant At);

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

// The adjudication outcome: the verdict, its typed receipt, whether the resolution arose from a genuine
// divergence (`Conflicted` — a `LocalWin`/`RemoteWin` won by HLC over differing content, or a `Rejected` fork),
// and the held content key the `Forked` fault reads without a second `Held` lookup. An idempotent-replay
// `LocalWin` (content-equal) and a `Merged` (no competitor) carry `Conflicted: false` — the apply fold counts
// them as a skip/apply, never a conflict, so the `Conflicted`/`Conflicts` audit fields stay exact.
public readonly record struct ConflictResult(ConflictVerdict Verdict, ConflictReceipt Receipt, bool Conflicted, UInt128 Held);

// --- [ERRORS] --------------------------------------------------------------------------
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected

// The merge-engine fault band (825x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND`
// `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)`
// ctor (no `Category` to override) is the deleted form. No `[GenerateUnionOps]` — the kernel union-ops generator is
// strictly opt-in, so the band carries no per-case `SelfOp` while the `[Union]`-generated `Switch`/`Map` is
// untouched, the `Expected` derivation making a bare case an `Error` directly so it lifts onto `Fin<T>`/`Validation`
// with no `.ToError()` hop; band membership derives `Code => FaultBand.Sync + n` through the registry row
// (`Element/graph#FAULT_TABLES` — a bare integer literal is the deleted form), `Message` projects the case
// detail, and `Category` the telemetry label, so a recovery reads `error.IsType<SyncFault.Forked>()` /
// `error.HasCode(8256)` / `error.Category()`, never a message substring. (`using Expected = Rasm.Domain.Expected;`
// aliases the bare `Expected` to the kernel base across the page.)
[Union]
public abstract partial record SyncFault : Expected, IValidationError<SyncFault> {
    private SyncFault() : base() { }
    public sealed record SchemaMismatch(ulong Local, ulong Remote) : SyncFault;
    public sealed record ReplicationFaulted(string Slot, string Cause) : SyncFault;
    public sealed record SpeckleMarshal(string Peer, string Class) : SyncFault;
    public sealed record TransferDecode(string Peer, string Cause) : SyncFault;
    public sealed record Unconserved(long Batch, long Settled) : SyncFault;
    public sealed record Forked(ConflictReceipt Receipt, UInt128 Held, UInt128 Incoming) : SyncFault;

    public override int Code => FaultBand.Sync + Switch(
        schemaMismatch:     static _ => 1,
        replicationFaulted: static _ => 2,
        speckleMarshal:     static _ => 3,
        transferDecode:     static _ => 4,
        unconserved:        static _ => 5,
        forked:             static _ => 6);

    public override string Message => Switch(
        schemaMismatch:     static c => $"{c.Local}:{c.Remote}",
        replicationFaulted: static c => $"{c.Slot}:{c.Cause}",
        speckleMarshal:     static c => $"{c.Peer}:{c.Class}",
        transferDecode:     static c => $"{c.Peer}:{c.Cause}",
        unconserved:        static c => $"{c.Batch}!={c.Settled}",
        forked:             static c => $"{c.Receipt.EntityKey}@{c.Receipt.Incoming.Physical}:{c.Held}!={c.Incoming}");

    public override string Category => Switch(
        schemaMismatch:     static _ => "Schema",
        replicationFaulted: static _ => "Replication",
        speckleMarshal:     static _ => "Speckle",
        transferDecode:     static _ => "Transfer",
        unconserved:        static _ => "Conserve",
        forked:             static _ => "Fork");

    public static SyncFault Create(string message) => new ReplicationFaulted(string.Empty, message);
}

// The per-run apply evidence, self-attesting through the kernel receipt-validity floor: `Batch` carries the run's
// input count so `IsValid` is the parameterless `ValidityClaim.All` conservation fold — the settled sum equals the
// batch AND every counted conflict carries its recorded receipt — and `OpAcceptance.ValidityOf` reads the same
// `IValidityEvidence` arm. The parameterized `Conserves(long)` knob and a hand-rolled `&&` chain are the deleted
// forms: the receipt reconstructs the check from its own fields, so a downstream consumer re-proves conservation
// without the caller's batch count.
public readonly record struct SyncApplyReceipt(long Batch, long Applied, long Skipped, long Conflicted, long Converged, long Pushed, long QueueDepth, Seq<ConflictReceipt> Conflicts, SyncCursor Cursor, Guid Correlation, Instant At) : IValidityEvidence {
    public long Settled => Applied + Skipped + Conflicted + Converged;
    public bool IsValid => ValidityClaim.All(ValidityClaim.Of(Batch == Settled), ValidityClaim.Of(Conflicts.Count == Conflicted));
}

// The session capsule: the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame carries clock,
// correlation, and tenant as VALUES ([A.1] — a `ClockPolicy`/`CorrelationId` field is the deleted strata
// inversion); `Truncate` is the relation-wide commit lane a winning `Kind.WholeRelation` entry takes.
public sealed record SyncSession(
    ProjectionContext Frame, ReceiptSinkPort Sink, Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, CancellationToken Token,
    Func<UInt128, bool> Holds, Func<OpLogEntry, Option<OpLogEntry>> Held, Func<OpLogEntry, IO<Unit>> Commit, Func<OpLogEntry, IO<Unit>> Truncate, Func<OpLogEntry, IO<Unit>> Converge,
    Func<SyncCursor, Seq<OpLogEntry>> Pending, Func<long> QueueDepth, Func<UInt128, IO<OpLogEntry>> Fetch,
    Func<string, SyncCursor, IO<(ulong SchemaFingerprint, Seq<OpLogEntry> Entries, SyncCursor Cursor)>> Pull,
    Func<string, Seq<OpLogEntry>, IO<SyncCursor>> Push, Func<string, Seq<UInt128>, IO<Seq<UInt128>>> HasObjects,
    Func<string, Seq<OpLogEntry>, IO<(UInt128 RootContentKey, long ConvertedReferences)>> SpeckleSend,
    Func<string, UInt128, IO<Seq<OpLogEntry>>> SpeckleReceive);

public static class SyncMerge {
    public static ConflictReceipt Receipt(SyncSession session, OpLogEntry held, OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, held.Stamp, held.Actor, incoming.Stamp, incoming.Actor, session.Frame.Correlation, session.Frame.Now());

    static ConflictReceipt Fresh(SyncSession session, OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, Hlc.Zero, string.Empty, incoming.Stamp, incoming.Actor, session.Frame.Correlation, session.Frame.Now());

    public static ConflictResult Adjudicate(SyncSession session, OpLogEntry incoming) =>
        session.Held(incoming) is { IsSome: true, Case: OpLogEntry held }
            ? incoming.ContentKey == held.ContentKey
                ? new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming), Conflicted: false, held.ContentKey)
                : ((incoming.Stamp, incoming.OriginStoreId).CompareTo((held.Stamp, held.OriginStoreId)), incoming.Family.Stance == MergeStance.FirstWriter) switch {
                    // FirstWriter (Presence lane) is earliest-wins, the INVERSE of LWW latest-wins, so the comparison
                    // direction flips: the OLDER stamp wins regardless of arrival. LWW keeps the newer; an equal
                    // (stamp, origin) over divergent content is the causal fork either stance halts on.
                    (> 0, true) => new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming), Conflicted: true, held.ContentKey),
                    (< 0, true) => new ConflictResult(ConflictVerdict.RemoteWin, Receipt(session, held, incoming), Conflicted: true, held.ContentKey),
                    (> 0, false) => new ConflictResult(ConflictVerdict.RemoteWin, Receipt(session, held, incoming), Conflicted: true, held.ContentKey),
                    (< 0, false) => new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming), Conflicted: true, held.ContentKey),
                    _ => new ConflictResult(ConflictVerdict.Rejected, Receipt(session, held, incoming), Conflicted: true, held.ContentKey),
                }
            : new ConflictResult(ConflictVerdict.Merged, Fresh(session, incoming), Conflicted: false, UInt128.Zero);

    // The idempotent-convergent apply fold. Each entry lands in EXACTLY ONE counter so the receipt's `IsValid`
    // conservation fold is exact:
    // a convergent `crdt`-lane entry is `Converged`; a fresh `Merged` is `Applied`; a `RemoteWin` commits the
    // incoming winner AND counts `Conflicted` (an auto-resolved divergence is still a conflict for audit), its
    // receipt recorded; a conflict `LocalWin` keeps the local entry (no commit) and counts `Conflicted`, its
    // receipt recorded; an idempotent-replay `LocalWin` (content-equal) is a pure `Skipped`, no receipt; a
    // `Rejected` causal fork halts the whole apply on the epoch-class `SyncFault.Forked` carrying both divergent
    // content keys read off the result (never a second `Held` lookup). A winning `Kind.WholeRelation` entry
    // commits through `Truncate` (relation-wide clear) instead of `Commit` — the one consumer of the verb's
    // policy bit. So the `Conflicts`/`Conflicted` audit fields carry every genuine divergence the run resolved.
    public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming) =>
        incoming.Fold(
            IO.pure((Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Conflicts: Seq<ConflictReceipt>())),
            (acc, entry) => acc.Bind(counts => entry.Family.Stance.Convergent
                ? session.Converge(entry).Map(_ => counts with { Converged = counts.Converged + 1L })
                : Adjudicate(session, entry) switch {
                    var fork when fork.Verdict == ConflictVerdict.Rejected => IO.fail<(long Applied, long Skipped, long Conflicted, long Converged, Seq<ConflictReceipt> Conflicts)>(new SyncFault.Forked(fork.Receipt, fork.Held, entry.ContentKey)),
                    { Verdict.Applies: true } result => (entry.Kind.WholeRelation ? session.Truncate(entry) : session.Commit(entry)).Map(_ => result.Conflicted
                        ? counts with { Conflicted = counts.Conflicted + 1L, Conflicts = counts.Conflicts.Add(result.Receipt) }
                        : counts with { Applied = counts.Applied + 1L }),
                    { Conflicted: true } result => IO.pure(counts with { Conflicted = counts.Conflicted + 1L, Conflicts = counts.Conflicts.Add(result.Receipt) }),
                    _ => IO.pure(counts with { Skipped = counts.Skipped + 1L }),
                }))
            .Map(c => new SyncApplyReceipt(incoming.Count, c.Applied, c.Skipped, c.Conflicted, c.Converged, Pushed: 0L, session.QueueDepth(), c.Conflicts, session.Cursor, session.Frame.Correlation, session.Frame.Now()))
            .Bind(receipt => receipt.IsValid ? IO.pure(receipt) : IO.fail<SyncApplyReceipt>(new SyncFault.Unconserved(receipt.Batch, receipt.Settled)));
}
```

| [INDEX] | [POLICY]              | [VALUE]                                              | [BINDING]                                                  |
| :-----: | :-------------------- | :-------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | scalar default        | LWW by `(Hlc, OriginStoreId)`; `FirstWriter` inverts to earliest-wins | total order, deterministic across peers; the `(cmp, isFirstWriter)` switch flips both arms |
|  [02]   | crdt lane             | `Crdt.Apply` join-semilattice                       | converges by merge; the multi-writer offline substrate    |
|  [03]   | causal fork           | equal `(stamp, origin)` divergent content           | `SyncFault.Forked` halts merge; never a soft conflict     |
|  [04]   | conservation          | receipt `IsValid` — `ValidityClaim.All` over the carried `Batch` | a breach is `SyncFault.Unconserved` failing the rail      |
|  [05]   | whole-relation truncate | `Kind.WholeRelation` → session `Truncate` delegate | the winning truncate clears the `(Model, Family)` relation; `Held` answers the relation head |

## [04]-[SYNC_TRANSPORTS]

- Owner: `SyncFlow` the keyless disposition carrying the `(Pulls, Pushes)` policy pair; `SyncTransport` `[Union]`; the `SyncPump` dispatch surface with the `SubtreeFetch` graph-checkout bridge and the `Offer` Speckle-diff arm; `GraphDiff` the named set-difference diff-algebra `SubtreeFetch` and `Offer` both dial.
- Cases: 3 transport cases — `HttpDelta`, `SpeckleLikeDiff`, `SubtreeCheckout` — widened by the one `SyncFlow` field whose `Pulls`/`Pushes` policy pair the `Exchange` fold reads; fan-in/fan-out/bidirectional are `SyncFlow` rows, never new transport cases.
- Entry: `public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport)` is one total state-threaded dispatch; `public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds)` projects the missing geometry-BLOB-key manifest (the `Closure` plus the root payload key, minus held); `public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root)` fetches the root entry, applies it onto the target, and accounts the blob manifest on the receipt.
- Auto: intra-cluster replication is Marten's own daemon (`DaemonMode.HotCold`) over the shared PostgreSQL, so this transport axis is the CROSS-store / offline lane (a disconnected editor, a Speckle hub, a peer holding a subgraph), never a re-implementation of single-cluster replication; `HttpDelta` pulls a cursor-bounded segment and pushes the pending set gated by `SyncFlow`; `SubtreeCheckout` fetches the root op-log entry, APPLIES it (the delta is the change), and accounts its `Closure` geometry-blob manifest as the blob-transfer set the content-addressed blob store moves — the `Closure` is a representation-content-hash blob manifest, never an op-log-entry fetch input, so a checkout applies the one entry and the blobs ride the blob store, never a `Fetch` of a blob key as an entry; `SpeckleLikeDiff` folds the pending set through `GraphDiff` over the peer `HasObjects` membership and hands the missing set to the `SpeckleSend` marshal.
- Receipt: every transport run yields one `SyncApplyReceipt`; the subtree-checkout transfer count rides the same receipt.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, Speckle.Sdk (companion, outside-Rhino), BCL inbox.
- Growth: a new transport is one case plus one dispatch arm; a new graph-checkout shape is one entry over `GraphDiff`, never a second diff algebra; zero new surface.
- Boundary: `HttpDelta` rides the AppHost `OutboundHop` keyed pipeline — retry, backoff, and hop deadlines are owned there and the database stays excluded from the hop law; the document-granular fallback is the RFC 6902 patch payload subordinate to the changefeed; `GraphDiff` is the one set-difference diff-algebra (the `Closure` GEOMETRY-blob manifest plus the root payload key, minus the target `Holds` set) — the BLOB-transfer set the content-addressed blob store moves, NOT an op-log-entry fetch input; `SubtreeFetch` fetches the root entry, applies it, and accounts that blob set on the receipt, its `root` argument discriminating which subtree's entry+blobs transfer — feeding the blob manifest to the op-log `Fetch` (which resolves an entry, never a geometry blob) or a second walk-and-diff being the deleted form; the `SpeckleLikeDiff` `Offer` rides the same one diff algebra and the wire leg lives OUTSIDE-RHINO on the companion target where `Speckle.Sdk.Dependencies` repacks the closure, so the in-Rhino assembly composes only the case and the marshal delegate slot and never references `Speckle.Sdk`/`Speckle.Objects`, the DI-resolved INSTANCE `IOperations.Send` (never the non-existent static `Operations.Send`) returning a `(rootObjId, convertedReferences)` tuple whose `rootObjId` projects onto the offered root `ContentKey` at the marshal seam with zero second identity (`Offer` faults `SyncFault.SpeckleMarshal` when the returned root key drifts from the offered head's local key); the wire leg of `GraphDiff`/`SubtreeFetch` is owned at `Compute/Runtime/wire#PROTO_VOCABULARY` so the remote rpc dials this algebra and never re-implements the set-difference.

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
            // The subtree checkout fetches the root OpLogEntry and APPLIES it (the delta IS the change), then the
            // geometry BLOBS its `Closure` references ride the content-keyed blob store separately — the `Closure` is
            // the BLOB-transfer manifest (representation-content-hash keys), NOT op-log-entry keys, so feeding it to
            // `s.Fetch` (the entry resolver) would resolve no entry. The missing-blob count rides the receipt's
            // `Pushed` accounting; the blob fetch itself is the `Store/blobstore` content-addressed transfer the peer
            // dedups by content key, never an `Apply` of a blob as an op-log entry.
            subtreeCheckout: static (s, row) => s.Fetch(row.Root).Bind(entry =>
                SyncMerge.Apply(s, Seq(entry)).Map(receipt => receipt with { Pushed = GraphDiff(entry, s.Holds).Count })));

    // The BLOB-transfer manifest a subtree checkout / Speckle offer dials: the descendant geometry content-key set
    // (the `Closure`) plus the root entry's own payload key, minus what the peer holds — the keys the content-addressed
    // blob store must transfer to materialize the change, NEVER an op-log-entry fetch input (the entry rides `Fetch`).
    public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds) => OpLog.TransferSet(root, holds);

    // Source->target subtree checkout: fetch the root entry, apply it onto the target, and account the source-side
    // geometry-blob transfer set (the `Closure` minus what the target holds) on the receipt — the blobs themselves
    // transfer through the content-addressed blob store, never re-fetched as op-log entries through `source.Fetch`.
    public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry =>
            SyncMerge.Apply(target, Seq(entry)).Map(receipt => receipt with { Pushed = GraphDiff(entry, target.Holds).Count }));

    static IO<SyncApplyReceipt> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        (row.Flow.Pulls
            ? s.Pull(row.Peer, s.Cursor).Bind(segment => segment.SchemaFingerprint == s.SchemaFingerprint
                ? SyncMerge.Apply(s, segment.Entries).Map(receipt => receipt with { Cursor = segment.Cursor })
                : IO.fail<SyncApplyReceipt>(new SyncFault.SchemaMismatch(s.SchemaFingerprint, segment.SchemaFingerprint)))
            : IO.pure(new SyncApplyReceipt(0L, 0L, 0L, 0L, 0L, 0L, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor, s.Frame.Correlation, s.Frame.Now())))
        .Bind(receipt => row.Flow.Pushes
            ? IO.pure(s.Pending(receipt.Cursor)).Bind(pending => s.Push(row.Peer, pending).Map(cursor => receipt with { Pushed = pending.Count, Cursor = cursor }))
            : IO.pure(receipt));

    static IO<SyncApplyReceipt> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        IO.pure(s.Pending(s.Cursor)).Bind(pending =>
            s.HasObjects(row.Peer, toSeq(pending.Fold(Seq<UInt128>(), static (set, entry) => set + GraphDiff(entry, static _ => false)).Distinct()))
                .Map(held => pending.Filter(entry => !held.Contains(entry.ContentKey)))
                .Bind(missing => s.SpeckleSend(row.Peer, missing).Bind(sent =>
                    missing.Head.Map(h => h.ContentKey) is { IsSome: true, Case: UInt128 root } && root != sent.RootContentKey
                        ? IO.fail<SyncApplyReceipt>(new SyncFault.SpeckleMarshal(row.Peer, $"root-key-drift:{root}!={sent.RootContentKey}:refs={sent.ConvertedReferences}"))
                        : IO.pure(new SyncApplyReceipt(0L, 0L, 0L, 0L, 0L, missing.Count, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor with { Sequence = s.Cursor.Sequence + missing.Count }, s.Frame.Correlation, s.Frame.Now())))));
}
```

| [INDEX] | [POLICY]                 | [VALUE]                                | [BINDING]                                                  |
| :-----: | :----------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | intra-cluster replication | Marten daemon `HotCold`               | this axis is the cross-store/offline lane only            |
|  [02]   | graph checkout           | fetch+apply the root entry; `GraphDiff` is the BLOB manifest | the `Closure` blob set transfers through the blob store, never an op-log `Fetch` |
|  [03]   | Speckle marshal          | DI-resolved instance `IOperations`     | runs outside-Rhino; `rootObjId` → `ContentKey`; drift faults |
|  [04]   | http delta               | AppHost `OutboundHop` pipeline         | database excluded from the hop law                        |

## [05]-[PRESENCE]

- Owner: `PresenceRow` the ephemeral collaboration row on the changefeed shape (the `ColumnFamily.Presence` lane value); `AwarenessBeat`/`AwarenessKind` the dedicated low-latency lossy awareness signal carrying cursor, selection, camera-frustum, focus, and follow beats off the durable changefeed; `WorkingSet`/`ReplicationQuery` the partial-replication subgraph checkout; `Awareness` the ONE static surface owning the lossy beat lane, the durable presence-row mint and sweep, and the working-set checkout — one deep surface over the two presence forms plus the checkout, never three shallow services.
- Entry: `public static Fin<DrainQueue<AwarenessBeat>> AwarenessLane(DrainSpec spec, Atom<Seq<AwarenessBeat>> dropped)` opens the declared `DropOldest` `DrainSpec` row through the AppHost `DrainSurface.Open<AwarenessBeat>` so the lane carries its `onDrop` drop receipt and never hand-rolls `BoundedChannelOptions` — the `DrainQueue<AwarenessBeat>.Pipe` case carries the `Channel<AwarenessBeat>` the awareness writer drives, and a `DropOldest` row that opens without an `onDrop` receipt fails on the `Fin` rail; `public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ProjectionContext frame, Option<string> session = default)` is the one polymorphic awareness constructor (the kind discriminates payload meaning, the per-signal factories being the deleted form); `public static PresenceRow Present(string actor, ReadOnlyMemory<byte> state, Duration ttl, ProjectionContext frame)` mints the durable ephemeral presence row and `public static Seq<PresenceRow> Live(Seq<PresenceRow> rows, Instant now)` is the per-actor add-wins-LWW sweep keeping only unexpired rows; `public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch, SyncCursor cursor, ProjectionContext frame)` materializes a subgraph working set.
- Auto: presence rows expire at stamp plus `Ttl` and sweep on the heartbeat `ScheduleEntry` cadence; the awareness channel is a separate lossy lane from the durable changefeed — cursor moves, selection halos, and camera frusta beat at high cadence through the `DropOldest` channel and never touch the durable store, while `AwarenessKind` discriminates the beats and `AwarenessBeat.Supersedes` lets a slow reader discard a reordered beat by per-actor `Seq` lamport; a dropped beat is receipted through the `DrainSurface.Open` `onDrop` callback into the loss atom, distinct from the converging `Version/commits#CRDT_ALGEBRA` `EphemeralMap` a late-joining peer reconstructs from the changefeed prefix; the working-set checkout resolves a `ReplicationQuery` (region/layer/view/type/closure-depth) into a content-key set then fetches only those entries so a peer materializes one subgraph rather than the whole graph.
- Receipt: a presence beat rides `store.presence.beat`; an awareness drop rides the `DrainSurface.Open` `onDrop` receipt into the loss atom; a working-set checkout rides `store.replication.checkout` carrying the subgraph size.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, System.Threading.Channels, BCL inbox.
- Growth: a new awareness signal is one `AwarenessKind` row; a new checkout dimension is one field on `ReplicationQuery`; zero new surface — a per-signal awareness factory, a presence row written to the DURABLE event stream (the `Presence` lane is `durable: false`), or a second lossy lane is the deleted form.
- Boundary: presence is one ephemeral `Presence`-lane changefeed row (`durable: false`, `FirstWriter` stance) that `Present` mints and `Live` sweeps per-actor add-wins-LWW, never a durable event-stream write and never a transport; the lossy awareness lane is the 60-Hz fire-and-forget channel that never appends a durable entry, while the converging `EphemeralMap` is the durable self-expiring presence map a late-joining peer reconstructs — two distinct presence forms (lossy live vs convergent reconstructible) the one `Awareness` surface owns together so the durable presence projection's physical-time liveness horizon agrees with the convergent map's; the working-set checkout subscribes its op-stream to receive only changes touching its checked-out keys so a partial-replication peer never pulls the whole graph.

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

    public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ProjectionContext frame, Option<string> session = default) =>
        new(actor, kind, payload, seq, frame.Now(), session);

    // The one presence-row mint: the ephemeral collaboration row stamped at `frame.Now()` with its `Ttl`, the
    // value the `ColumnFamily.Presence` (`FirstWriter`) lane carries on the changefeed shape — distinct from the
    // lossy `AwarenessBeat` (the 60-Hz fire-and-forget lane that never appends). A second per-actor presence
    // constructor is the deleted form.
    public static PresenceRow Present(string actor, ReadOnlyMemory<byte> state, Duration ttl, ProjectionContext frame) =>
        new(actor, state, frame.Now(), ttl);

    // The presence sweep: keep one row per actor (the latest by stamp, the add-wins-LWW the convergent
    // `Version/commits#CRDT_ALGEBRA` `EphemeralMap` resolves to) and drop every row whose `Ttl` lapsed at `now` —
    // a peer that stopped beating expires, so the durable presence projection agrees with the convergent map's
    // physical-time liveness horizon. The sweep is the heartbeat-cadence fold the Auto claims, never an unbounded
    // accumulation of stale rows.
    public static Seq<PresenceRow> Live(Seq<PresenceRow> rows, Instant now) =>
        toSeq(rows.Filter(row => row.Live(now)).GroupBy(static row => row.Actor).Select(static g => g.MaxBy(static row => row.At)));

    public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch, SyncCursor cursor, ProjectionContext frame) =>
        from keys in resolve(query)
        from entries in fetch(keys)
        select new WorkingSet(keys, entries, cursor, frame.Now());
}
```

| [INDEX] | [POLICY]            | [VALUE]                            | [BINDING]                                                  |
| :-----: | :------------------ | :--------------------------------- | :-------------------------------------------------------- |
|  [01]   | lossy awareness     | `DropOldest` `DrainSpec` lane, `onDrop` receipt | never a durable changefeed row; distinct from `EphemeralMap` |
|  [02]   | presence ttl        | stamp + `Ttl`, heartbeat sweep     | one ephemeral row, never a transport                      |
|  [03]   | working-set checkout| `ReplicationQuery` → key set       | one subgraph, never the whole graph                       |
