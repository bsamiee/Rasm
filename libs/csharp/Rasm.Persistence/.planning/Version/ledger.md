# [PERSISTENCE_VERSION_LEDGER]

`Rasm.Persistence.Version` projects Marten events into one durable `OpLogEntry` feed and owns every convergence decision over that feed. `ColumnFamily` binds payload codec and merge stance; `ReplayWindow` parameterizes every bounded read; `SyncMerge` folds scalar, first-writer, and CRDT entries through one closed fault rail; `SyncTransport` carries cross-store exchange; `Awareness` owns ephemeral collaboration. `ProjectionContext` supplies time, correlation, and tenant evidence, while `ContentHash.Of` supplies payload identity.

## [01]-[INDEX]

- [02]-[CHANGEFEED]: the `OpLogEntry` projection of Marten events, HLC stamping, the trace slot, the closure manifest, and the `ReplayWindow` windowed read.
- [03]-[MERGE_LAW]: LWW adjudication, conflict receipts, the idempotent apply fold, CRDT dispatch, and the conservation invariant.
- [04]-[SYNC_TRANSPORTS]: the closed transport family, its `SyncFlow` disposition, the subtree-checkout diff algebra, and the Speckle marshal seam.
- [05]-[PRESENCE]: ephemeral presence rows, the lossy awareness lane, and the working-set checkout.

## [02]-[CHANGEFEED]

- Owner: `SyncOpKind` `[SmartEnum<string>]` the write-verb axis; `OpLaw` `[SmartEnum<string>]` the estate's one commutation vocabulary carrying its `Commutes`/`Idempotent` columns; `ColumnFamily` `[SmartEnum<string>]` the merge-lane axis carrying its `MergeStance` AND its `SnapshotCodec` policy columns so the lane selects its adjudication algebra and its payload codec as one vocabulary row (`OpLogEntry.Codec` is the `Family.Codec` accessor — a stored codec field beside the lane row is the deleted split-brain); `TraceSlot` `[ComplexValueObject]` the changefeed W3C trace-id carrier (the slot value, NOT the AppHost `TraceContext` propagation fold); `OperationId` `[ComplexValueObject]` the dot-plus-frontier operation identity every entry keys on; `DotSource` the store's ONE dot minter over its frontier atom; `OpLogEntry` the changefeed record one Marten event projects to; `ReplayWindow` the one windowed-read parameterization (origin/entity/model/family/sequence); `ChangefeedSubscription` the `Marten.Subscriptions.SubscriptionBase` folding each delivered `EventRange` into ONE batched `Seq<OpLogEntry>` drain; the `OpLog` project-stamp-replay surface.
- Cases: `SyncOpKind` is `Upsert | Delete | Truncate | Presence`; `OpLaw` is `Ordered | Commutative | Semilattice`, the same triple `Version/commits#CRDT_ALGEBRA` `Crdt.Law` returns per op arm and `typescript:core/state/merge` `Merge.Law` spells; `MergeStance` is `Lww(Ordered) | Crdt(Semilattice) | FirstWriter(Ordered)` and derives `Convergent` off the law rather than carrying a second boolean; `ColumnFamily` is the closed lane family `Scalar(Lww, JsonStj) | Crdt(Crdt, MessagePackBinary) | Geometry(Lww, JsonStj) | Presence(FirstWriter, MessagePackBinary) | Commit(Lww, MessagePackBinary) | Branch(Lww, MessagePackBinary) | Attest(Lww, MessagePackBinary)`, each row carrying its `MergeStance` and `SnapshotCodec` so a consumer dispatches on `Family.Stance` and decodes by `Family.Codec`, never a string compare; a new lane is one row carrying both columns; the trace slot is a top-level envelope field, never inside `Payload`.
- Entry: `public override Task<IChangeListener> ProcessEventsAsync(EventRange range, ISubscriptionController controller, IDocumentOperations operations, CancellationToken token)` is the `SubscriptionBase` override that reserves the range's dots in one swap, folds the WHOLE delivered range into one `Seq<OpLogEntry>`, and drains it once (per-event awaits inside the range are the deleted form), returning `NullChangeListener.Instance`; `public Seq<OperationId> Reserve(int count)` on `DotSource` is the ONE identity mint both the projection and the authoring path take; `public static OperationId Mint(Guid origin, VersionVector frontier)` derives the dot as the frontier's next slot and `public static VectorOrder Order(OperationId left, OperationId right)` answers happened-before from two ids alone; `public static Seq<OpLogEntry> Project(Seq<OperationId> dots, Seq<IEvent<GraphEvent>> events)` zips reserved dots against the ordered range and `public static OpLogEntry Project(OperationId id, IEvent<GraphEvent> e)` lifts one Marten event — the seam `GraphDelta` body is the codec-encoded `Payload` on the structural `geometry` lane, the event `Timestamp`/`Version` are the `Hlc` cell, the `actor` header populates `Actor`, the dot's `Origin` IS the store id, a carried 16-byte trace-id populates the trace slot, and the content key is `ContentHash.Of` over the encoded payload; `public static Seq<OpLogEntry> Replay(Seq<OpLogEntry> feed, ReplayWindow window)` is the ONE windowed changefeed read — the AppUi `Collab/Editing` per-doc edit-intent replay (`ReplayWindow.ForEntity`), the AppHost `Runtime/determinism` neutral-log read (`ReplayWindow.ForOrigin`), and the `Version/egress` durable-ops CDC drain (`ReplayWindow.DurableOps`) are three parameterizations of one case, never three read surfaces; `public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, DotSource dots, ProjectionContext frame, Func<(OperationId Id, Instant Physical, ulong Logical, TraceSlot Trace), OpLogEntry> build)` is the authoring-path mint carrying one HLC cell, one dot, and the captured trace; `public static Fin<Seq<(GraphEventEnvelope Envelope, Seq<(string Key, string Value)> Headers)>> Appended(Seq<GraphDelta> deltas, ProjectionContext frame, string source)` composes the seam `Graph/wire#EVENT_ENVELOPE` `GraphEventEnvelope.For(delta, tolerance, source, at, key)` per appended crossing, dedups the batch by the content-key `Subject`, and reads `Attributes()` for the transport headers; `public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds)` projects the closure-minus-held missing-key set.
- Auto: a Marten async subscription projects each committed model event into the one feed; triggers, secondary op-log tables, and per-payload records are inadmissible (`H11`). A projected `GraphEvent` is a structural `geometry`-lane change carrying the codec-encoded `GraphDelta`, adjudicated by `(Hlc, OriginStoreId)` LWW. `crdt` and `commit` payloads enter through their owners' `Stamp` operations. `Closure` carries descendant geometry content keys, so transfer is set difference rather than a tree walk. `TraceSlot` captures the originating `Activity.Current` context once and persists through the Marten event.
- Receipt: changefeed position and queue depth ride `SyncApplyReceipt`; the projected-segment evidence rides `ReceiptSinkPort`.
- Packages: Marten (`SubscriptionBase`/`ProcessEventsAsync`/`EventRange`/`ISubscriptionController`/`IDocumentOperations`/`IChangeListener`/`NullChangeListener`/`IEvent<T>`), Rasm.Element (`GraphDelta`/`Node`/`Node.Object`/`RepresentationContentHash` + `Graph/wire#EVENT_ENVELOPE` `GraphEventEnvelope.For`/`Attributes`), Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher; a direct `XxHash128` call site is the deleted spelling), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Diagnostics.DiagnosticSource, BCL inbox.
- Growth: a new synced concern is one `SyncOpKind` verb, one `ColumnFamily` lane carrying its `MergeStance`/`SnapshotCodec` columns, or one payload kind keyed by the lane row's `Codec`; a new windowed-read consumer is one `ReplayWindow` parameterization, never a new read surface; zero new surface — a per-entity-kind outbox table, a bespoke op-log store beneath Marten, a per-payload-kind parallel record, a second dot minter beside `DotSource`, or a per-lane string literal in the merge fold is the deleted form.
- Boundary: `OperationId` is the entry key and `ContentKey` the payload key, and the two never merge — the `libs/.planning/RULINGS.md` `[02]-[SHAPE]` derivation lands here as two fields: two peers stamping the identical `Set("name", "North Wing")` share one `ContentKey` and carry two dots, so the second edit survives; a content-keyed log drops it and reads the drop as successful dedup. `Counter` is the origin's own `VersionVector` slot rather than a second counter beside it, and `Context` is the pre-mint frontier, so `Order` is total over `Before | After | Concurrent | Equal` from two ids with no feed walk and `Applied` is the exact replay test the merge fold takes. `Sequence` survives as the store-local drain cursor alone — a resumable position, never an identity, because two stores mint sequence 41 and one entry cannot answer to both. `DotSource` is the store's single minter: the changefeed range and the authoring `Stamp` reserve from one atom, so the gap-free dot law holds across both paths, and a restart re-seeds the atom from the durable head joined with the tail past the cursor rather than restarting the counter into dots a peer already holds. Marten's `origin` header no longer reaches the entry — `OriginStoreId` reads the dot's own `Id.Origin`, so the LWW `(Hlc, OriginStoreId)` tie-break is deterministic across peers and no missing header fabricates the `Guid.Empty` bucket that collapsed every origin into one. The changefeed is PROJECTED from Marten events — the op-log IS the audit artifact, the change feed, and the sync feed as folds over the one Marten stream, never a second store (`H11` — Marten is the append substrate beneath, the engine projects from its events); a `Project`ed `GraphEvent` entry is the structural `geometry`-lane `GraphDelta` (the durable graph change is an LWW structural delta, NOT a CRDT op — `Project` produces no `crdt`-lane entry), while the `crdt` lane's `Payload` is the `Version/commits#CRDT_WIRE` `CrdtOp` delta a CRDT mutation `Stamp`s, the `commit` lane's a `CommitNode`, and the `attest` lane's the `Version/provenance#ATTESTED_LEDGER` `WitnessedHead` the external-witness publication mints through `Stamp` (identity the canonical head bytes' `ContentHash.Of`, the ordinary egress pump the only transport), so the trace slot is a top-level envelope field beside `ContentKey`, NOT inside `Payload`, and triggers no `CrdtOpWire` schema fork; Persistence only READS `Activity.Current` and projects to the `TraceSlot` value, never re-minting the propagator (the AppHost `TraceContext` correlation-spine fold owns `Inject`/`Extract`/`Continue` — the `TraceSlot` is named distinctly so the Persistence trace SLOT never collides with that propagation surface); the 16-byte trace-id admits once through the TOTAL `TraceSlot.FromHex` (the span `Convert.FromHexString(source, destination, out charsConsumed, out bytesWritten)` `OperationStatus` overload gated on `Done`, never the throwing array-returning `Convert.FromHexString(string)` that faults the projection fold on a 32-char non-hex correlation) so the interior never re-parses; `OpLogEntry` carries NO correlation field — correlation rides the sync session frame and receipts, so the trace slot is a genuinely new envelope field; a Marten-projected entry whose stored `CorrelationId` is absent, wrong-length, OR not valid hex carries `TraceSlot.Empty` (Persistence never fabricates a span the substrate did not carry and never throws on an arbitrary correlation value), and the apply continues the parent only when one exists; the durable lanes (`Family.Durable`) are the exactly-once CDC row source the `Version/egress` pump drains past the `Store/coordination#OUTBOX_CURSOR` — `ReplayWindow.DurableOps` is that drain's parameterization, and the presence/awareness lane (`durable: false`) stays the lossy `DrainSurface` channel, NEVER the exactly-once CDC envelope.

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

// One commutation vocabulary estate-wide, spelled here, at `Version/commits#CRDT_ALGEBRA` `Crdt.Law`, and at
// `typescript:core/state/merge` `Merge.Law`. Two columns, both derived from the row: an `Ordered` mutation needs a
// total order to settle a concurrent pair, so exactly one side loses and the loss is evidence; a `Semilattice` row
// absorbs a redelivery into identical state. Reading `Idempotent` as "converges" is what makes `MergeStance` carry
// no second boolean.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OpLaw {
    public static readonly OpLaw Ordered = new("ordered", commutes: false, idempotent: false);
    public static readonly OpLaw Commutative = new("commutative", commutes: true, idempotent: false);
    public static readonly OpLaw Semilattice = new("semilattice", commutes: true, idempotent: true);
    public bool Commutes { get; }
    public bool Idempotent { get; }
    private OpLaw(string key, bool commutes, bool idempotent) : this(key) => (Commutes, Idempotent) = (commutes, idempotent);
}

[SmartEnum]
public sealed partial class MergeStance {
    public static readonly MergeStance Lww = new(OpLaw.Ordered);
    public static readonly MergeStance Crdt = new(OpLaw.Semilattice);
    public static readonly MergeStance FirstWriter = new(OpLaw.Ordered);
    public OpLaw Law { get; }
    // Derived, never a stored twin: convergence IS the law's idempotence, and a lane carrying both columns states a
    // stance the fold and the receipt then disagree about.
    public bool Convergent => Law.Idempotent;
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
    // External-witness publication lane: a `Version/provenance#ATTESTED_LEDGER` `WitnessedHead` rides one
    // durable LWW row (`ContentKey` = `ContentHash.Of` over the canonical head bytes) so the ordinary egress
    // pump carries the signed tree head to the witness's sink — a bespoke witness envelope or a second pump
    // beside the changefeed is the deleted form.
    public static readonly ColumnFamily Attest = new("attest", MergeStance.Lww, SnapshotCodec.MessagePackBinary, durable: true);
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
        byte[] span = new byte[16];
        // `OperationStatus.Done` holds iff every one of the 32 chars decoded into the 16-byte span; `InvalidData`
        // reports a partial decode through the status value, never a throw.
        return Convert.FromHexString(correlation, span, out _, out _) == System.Buffers.OperationStatus.Done
            ? Create(span, ReadOnlyMemory<byte>.Empty)
            : Empty;
    }
    public static TraceSlot From(ActivityContext context, string? traceState) {
        byte[] span = new byte[16];
        context.TraceId.CopyTo(span.AsSpan(0, 16));
        return Create(span, Encoding.ASCII.GetBytes(traceState ?? string.Empty));
    }
    public Option<ActivityContext> Continue() =>
        HasParent ? Some(new ActivityContext(ActivityTraceId.CreateFromBytes(TraceId.Span), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded,
            Encoding.ASCII.GetString(Tracestate.Span) is { Length: > 0 } s ? s : null, isRemote: true)) : None;
}

// Operation identity, derived apart from payload identity. The dot `(Origin, Counter)` is the globally unique
// coordinate — `Counter` is the minting origin's OWN slot advanced by one, gap-free and monotone by the vector's join
// law, so no second counter exists to drift — and `Context` is the frontier that origin had observed, which makes
// `Order` answer happened-before from two ids alone instead of walking the feed. Keying on `ContentKey` instead makes
// two peers stamping the identical `Set(field, value)` ONE operation and silently drops the second edit; keying on a
// store-local `Sequence` makes two stores mint one id; keying on `(Hlc, Origin)` binds identity to a wall clock a
// process restart can rewind. `Key` hashes the canonical preimage through the kernel `ContentHash.Of` so an index
// reads one `UInt128` and the three runtimes derive it byte-identically off `VersionVector.WriteTo`.
[ComplexValueObject]
public sealed partial class OperationId {
    public static readonly OperationId Genesis = new(Guid.Empty, 0UL, VersionVector.Empty);
    public Guid Origin { get; }
    public ulong Counter { get; }
    public VersionVector Context { get; }

    // Gap-free by construction: the dot is exactly the frontier's next slot value, so a skipped counter is a lost
    // operation the causal log refuses at admission rather than a hole a later dominance test reads as satisfied.
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid origin, ref ulong counter, ref VersionVector context) {
        if (counter != (ulong)(context.At(origin) + 1L) && counter != 0UL) validationError = new ValidationError($"<operation-dot-gap:{counter}>");
    }

    public static OperationId Mint(Guid origin, VersionVector frontier) => Create(origin, (ulong)(frontier.At(origin) + 1L), frontier);
    // Frontier AFTER this operation: what the next id at this origin takes as context and what a receiver joins into
    // its own applied vector once the entry lands.
    public VersionVector Frontier => Context.Advance(Origin, 1L);
    public bool Applied(VersionVector frontier) => frontier.At(Origin) >= (long)Counter;
    public static VectorOrder Order(OperationId left, OperationId right) =>
        (left.Applied(right.Context), right.Applied(left.Context)) switch {
            (true, true) => VectorOrder.Equal,
            (true, false) => VectorOrder.Before,
            (false, true) => VectorOrder.After,
            _ => VectorOrder.Concurrent,
        };

    public UInt128 Key {
        get {
            ArrayBufferWriter<byte> canonical = new();
            WriteTo(canonical);
            return ContentHash.Of(canonical.WrittenSpan);
        }
    }

    // Framed origin text, little-endian counter, then the vector through its own canonical writer — the SAME
    // `VersionVector.WriteTo` the commit preimage takes, so a slot-order edit cannot land on one key and not the other.
    public void WriteTo(IBufferWriter<byte> sink) {
        CommitGraph.Framed(sink, Origin.ToString("N"));
        BinaryPrimitives.WriteUInt64LittleEndian(sink.GetSpan(8), Counter);
        sink.Advance(8);
        Context.WriteTo(sink);
    }
}

// The one changefeed record every Marten event projects to. `Payload` is the codec-encoded change whose shape the
// `(Family, Codec)` pair discriminates — a `geometry`/`scalar`-lane `GraphDelta`, a `crdt`-lane `CrdtOp`, a
// `commit`-lane `CommitNode`, or an `attest`-lane `WitnessedHead` — so the merge fold reads the lane row, never a
// second per-payload record. `Id` is the operation identity and `ContentKey` the payload identity: two fields, two
// jobs, and collapsing them is exactly the edit that turns a repeated edit into a dropped one. The `Hlc`
// `Stamp` orders adjudication and `(Hlc, OriginStoreId)` breaks ties deterministically across peers; `Closure` is
// the descendant geometry content-key manifest the transfer set-difference reads. No before-image field: the merge
// adjudicates on the `(Hlc, OriginStoreId)` stamp and content-key equality, and the conflict evidence is the typed
// `ConflictReceipt`, so a stored before-image is dead weight the delta log never reads.
public sealed record OpLogEntry(
    long Sequence, OperationId Id, ModelId Model, string EntityKey, ColumnFamily Family, SyncOpKind Kind,
    ReadOnlyMemory<byte> Payload, UInt128 ContentKey, TraceSlot Trace, Seq<UInt128> Closure,
    string Actor, Instant Physical, ulong Logical) {
    public Hlc Stamp => new(Physical, Logical);
    // Identity-derived, never a second stored origin: `Sequence` stays the store-local drain cursor and the dot stays
    // portable, so a peer resuming a feed and a peer ordering an operation read different columns on purpose.
    public Guid OriginStoreId => Id.Origin;
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

// One dot minter per store: the changefeed projection and the authoring `Stamp` path both reserve through it, so
// no two entries can carry one counter and the gap-free dot law `OperationId` validates holds across both paths. The
// atom seeds at composition from the durable head (`Version/commits#COMMIT_DAG` `CommitNode.Vector` joined with the
// tail past the drain cursor), so a restarted daemon resumes its counter instead of re-minting dots a peer holds. A
// caller keeping its own frontier copy is the deleted form — two holders diverge the instant either mints.
public sealed class DotSource(Guid origin, Atom<VersionVector> frontier) {
    public VersionVector Frontier => frontier.Value;

    // Reserve `count` consecutive dots under ONE swap, then derive each id's own opening context by walking the
    // reserved span. A per-entry swap lets the other path interleave a dot into the middle of a range, which is
    // exactly the gap the id refuses. `Advance(origin, -count)` rewinds the store's OWN slot, which the swap just
    // established, so the subtraction never reaches an absent slot.
    public Seq<OperationId> Reserve(int count) {
        VersionVector opening = frontier.Swap(held => held.Advance(origin, count)).Advance(origin, -count);
        return toSeq(Enumerable.Range(0, count)).Map(step => OperationId.Mint(origin, opening.Advance(origin, step)));
    }
}

public sealed class ChangefeedSubscription(DotSource dots, Func<Seq<OpLogEntry>, IO<Unit>> drain) : SubscriptionBase {
    // ONE batched fold per delivered range: reserve the range's dots in one swap, project every committed event into
    // one Seq, drain once — the per-event await inside the range (one RunAsync per event) is the deleted form.
    public override async Task<IChangeListener> ProcessEventsAsync(EventRange range, ISubscriptionController controller, IDocumentOperations operations, CancellationToken token) {
        Seq<IEvent<GraphEvent>> events = toSeq(range.Events.OfType<IEvent<GraphEvent>>());
        await drain(OpLog.Project(dots.Reserve(events.Count), events)).RunAsync(EnvIO.New(token: token)).ConfigureAwait(false);
        return NullChangeListener.Instance;
    }
}

public static class OpLog {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.presence.beat"), StoreSlot.Create("store.replication.checkout"));

    // The structural changefeed projection of one Marten `GraphEvent`. The body is a `Element/graph#STREAM_GRAIN`
    // `GraphDelta` (the durable graph change), NOT a `CrdtOp` — so the `Payload` is the codec-encoded delta on the
    // STRUCTURAL `geometry` lane (`MergeStance.Lww`, adjudicated by `(Hlc, OriginStoreId)`), never the `crdt`
    // convergence lane. The `crdt`/`commit` lanes carry a `CrdtOp`/`CommitNode` `Payload` minted through `Stamp`
    // by their owners (`Version/commits#CRDT_ALGEBRA`/`#COMMIT_DAG`), so `Project` produces ONLY `geometry`-lane
    // entries — `GraphCreated`/`GraphRevised` -> `Upsert`, `GraphRetired` -> `Delete`. The HLC cell rides the
    // Marten event `Timestamp` (physical) and `Version` (logical, the stream-monotone counter), matching the
    // `Version/timetravel#TIME_TRAVEL` blame reconstruction; `Actor` the `actor` header; the trace slot continues
    // a 16-byte trace-id the event carried, else `TraceSlot.Empty`.
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
    // Dots zip positionally against the range's events — the reservation and the projection walk one ordered span, so
    // entry `n` carries reserved dot `n` and no entry can borrow a neighbour's identity. The Marten `origin` header is
    // gone from the entry: the dot's `Origin` IS the store id, so a header the substrate may not carry can no longer
    // fabricate the `Guid.Empty` bucket that collapsed every peer's LWW tie-break into one.
    public static Seq<OpLogEntry> Project(Seq<OperationId> dots, Seq<IEvent<GraphEvent>> events) =>
        events.Zip(dots).Map(static pair => Project(pair.Item2, pair.Item1));

    public static OpLogEntry Project(OperationId id, IEvent<GraphEvent> e) {
        ReadOnlyMemory<byte> payload = ColumnFamily.Geometry.Codec.Serialize(typeof(GraphDelta), e.Data.Body);
        return new OpLogEntry(e.Sequence, id, ModelId.Create(e.StreamId), e.StreamKey ?? e.StreamId.ToString(),
            ColumnFamily.Geometry,
            e.Data.Lifecycle == EventLifecycle.Retired ? SyncOpKind.Delete : SyncOpKind.Upsert,
            payload, ContentHash.Of(payload.Span),
            TraceSlot.FromHex(e.CorrelationId),
            Closure(e.Data.Body), HeaderValue(e, "actor"),
            Instant.FromDateTimeOffset(e.Timestamp), (ulong)e.Version);
    }

    // The CloudEvents context an appended structural crossing carries. The SEAM mints it (`Graph/wire#EVENT_ENVELOPE`)
    // over the SAME `GraphDelta` canonical bytes the `Payload` encodes, so `Subject` IS the content-key dedup identity
    // this feed and the broker lane agree on — a Persistence-minted event id beside the seam's would fork that identity
    // and strand every partner's dedup. `Attributes()` yields the transport header rows verbatim and the carrier
    // adapter owns its binding prefix, never this leg. Batch dedup keys on `Subject` ALONE — a re-published crossing of
    // identical content IS one event — and keeps the first, because the changefeed already arrives sequence-ordered.
    // The tolerance is the frame `Header`'s, the one grid the delta's measure bytes were quantized against.
    public static Fin<Seq<(GraphEventEnvelope Envelope, Seq<(string Key, string Value)> Headers)>> Appended(
        Seq<GraphDelta> deltas, ProjectionContext frame, string source) =>
        deltas.Traverse(delta => GraphEventEnvelope.For(delta, frame.Header.Tolerance, source, frame.Now(), frame.Key))
            .As()
            .Map(static envelopes => toSeq(envelopes.AsEnumerable().DistinctBy(static envelope => envelope.Subject))
                .Map(static envelope => (envelope, envelope.Attributes())));

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
    // `CommitNode` payload, a host-sourced `Version/ingress` crossing): the ONE HLC stamp from the
    // `ReceiptSinkPort.Hlc` atom (no second clock), the ONE dot from the shared `DotSource`, and the captured trace
    // context, the `build` continuation closing over the lane/payload its owner supplies. The wall clock rides the
    // injected `ProjectionContext` frame ([A.1]), never a `ClockPolicy` parameter; the identity rides the dot source,
    // never a caller-supplied id, because a caller minting its own dot is a caller inventing a causal position.
    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, DotSource dots, ProjectionContext frame, Func<(OperationId Id, Instant Physical, ulong Logical, TraceSlot Trace), OpLogEntry> build) =>
        IO.lift(() => (Wall: frame.Now(), Trace: TraceSlot.Capture()))
            .Map(captured => (Cell: sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, captured.Wall)), Id: dots.Reserve(1)[0], captured.Trace))
            .Map(stamped => build((stamped.Id, stamped.Cell.Physical, stamped.Cell.Logical, stamped.Trace)));

    public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds) =>
        entry.Closure.Add(entry.ContentKey).Filter(key => !holds(key));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                      | [BINDING]                                                           |
| :-----: | :------------------ | :------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | changefeed source   | Marten `SubscriptionBase.ProcessEventsAsync` | the bespoke op-log store is retired beneath Marten                  |
|  [02]   | projected lane      | `geometry` (LWW) for a `GraphEvent`          | `Project` produces a structural delta, never a `crdt` op            |
|  [03]   | payload shape       | `(Family, Codec)` discriminates              | `Codec` is the lane row's column, `Family`-derived, never stored    |
|  [04]   | windowed read       | `ReplayWindow` origin/entity/family/window   | AppUi edit-intent, AppHost determinism, egress CDC drain — one case |
|  [05]   | lane → merge stance | `ColumnFamily.Stance`                        | dispatch reads the lane row, never a `"crdt"` string compare        |
|  [06]   | HLC cell            | event `Timestamp` + `Version`                | one stamp for op-log, CRDT merge, commit cell, wire                 |
|  [07]   | origin tie-break    | `Id.Origin` — the dot's own store id         | LWW `(Hlc, OriginStoreId)` deterministic; never a zero              |
|  [08]   | trace slot          | top-level `TraceSlot` field                  | never inside `Payload`; distinct from AppHost `TraceContext` fold   |
|  [09]   | crossing envelope   | seam `GraphEventEnvelope.For`                | `Subject` is the dedup key; headers ride `Attributes()`             |
|  [10]   | operation identity  | `OperationId` dot over its pre-mint frontier | equal payloads stay distinct; `Order` needs no feed walk            |
|  [11]   | payload identity    | `ContentKey` — `ContentHash.Of` the payload  | transfer set difference and blob dedup; never the entry key         |
|  [12]   | drain position      | `Sequence` — the store-local Marten cursor   | resumable read alone; two stores mint one sequence value            |
|  [13]   | dot custody         | one `DotSource` atom per store               | projection and authoring reserve together; gap-free by law          |

## [03]-[MERGE_LAW]

- Owner: `ConflictReceipt`, `ConflictVerdict` `[SmartEnum<string>]`, `ConflictResult`, `SyncApplyReceipt` (the `IValidityEvidence` conservation receipt — the kernel `ValidityClaim.All` fold over its own carried `Batch`), `SyncFault` the closed `[Union]` fault family deriving from the KERNEL `Rasm.Domain.Expected` (parameterless `: base()` + per-case `Code`/`Message`/`Category` `Switch`, NOT `LanguageExt.Common.Expected`; no `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in) in the 825x band, `SyncSession` the one session capsule carrying the injected `ProjectionContext` frame plus the delegate rows (`Commit`/`Truncate`/`Converge`/`Held`/…); `SyncMerge` the fold surface routing each `OpLogEntry` by its `ColumnFamily.Stance` — `Lww`/`FirstWriter` through `Adjudicate`, `Crdt` into `Crdt.Apply`, a winning whole-relation `Truncate` through the `Truncate` delegate.
- Cases: 4 verdict rows on `ConflictVerdict` — `LocalWin | RemoteWin | Merged | Rejected` — collapsed into one `ConflictResult(Verdict, Receipt, Conflicted, Held)` where `Conflicted` distinguishes a genuine divergence (an HLC-resolved `LocalWin`/`RemoteWin` over differing content) from an idempotent-replay `LocalWin` (content-equal) or a fresh `Merged`, and `Held` carries the held content key the fork fault reads without a second lookup; `Rejected` is reachable only on an equal `(stamp, origin)` with divergent content (the causal fork the `Apply` fold lifts to `SyncFault.Forked` and halts on), never a soft conflict bucket; the `SyncFault` family is `SchemaMismatch | ReplicationFaulted | SpeckleMarshal | TransferDecode | Unconserved | Forked | Unobserved`, the last carrying a compaction whose minter never observed the horizon it claims.
- Entry: `public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming)` carries commit effects; replay skips on IDENTITY against the fold's advancing frontier and the receipt proves applied, skipped, conflicted, converged, and pushed counts under the receipt's own `IValidityEvidence` conservation fold (the carried `Batch` plus exactly-one-counter-per-entry accounting make `IsValid` the exact settled-sum proof), an auto-resolved LWW divergence counting as `Conflicted` and recording its `ConflictReceipt` into `Conflicts` (whether the winner was committed or the local kept), an `IO.fail` `SyncFault.Unconserved` when the batch does not close; `public static Fin<Unit> Admissible(OperationId id, CrdtOp op)` is the causal gate the `Converge` binding runs over its decoded op, refusing a `Maintain` whose quiescence vector its own context fails to dominate.
- Receipt: `ConflictReceipt` is the typed fork evidence the `SyncFault.Forked` halt carries and the inspector projects; `SyncApplyReceipt` is the per-run apply evidence.
- Packages: Rasm (`Rasm.Domain` `Expected` — the federation fault base; `IValidityEvidence`/`ValidityClaim` — the receipt-validity floor `SyncApplyReceipt` registers through), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one `MergeStance` row carrying its `OpLaw` and feeding `Held` resolution; a fifth `ConflictVerdict` row is the named defect; a new fault cause is one `SyncFault` case; a new replicated data type is a `Version/commits#CRDT_ALGEBRA` `CrdtField` case with its `Crdt.Law` row, dispatched by this fold, never a fifth scalar arm.
- Boundary: replay dedup is IDENTITY-proven and never content-proven — `entry.Id.Applied(frontier)` is the whole test, so two operations carrying identical bytes both land and a redelivered operation lands once, where the content-equality test this fold used to run reported the second real edit as an idempotent replay and skipped it; the frontier joins each landed entry's `Frontier` inside the fold, so a batch carrying its own duplicate settles without a second lookup, and a `Converge` binding that commits its own merge is why only a non-convergent applying verdict reaches `Commit`/`Truncate`. Commutation is per mutation kind and reads ONE vocabulary: the lane answers `Stance.Law` and the crdt op answers `Crdt.Law`, both `OpLaw` rows, so an `Ordered` arm losing its total order counts `Conflicted` while a `Semilattice` arm re-absorbing counts `Skipped` — a distinction one `Convergent` boolean cannot carry, which is why the whole crdt lane formerly counted `Converged` over its genuine `set` conflicts. `Maintain` is the one arm whose admissibility outlives its fold: compaction commutes and is idempotent as a filter, yet it is a MEET where every sibling is a JOIN, so a horizon its minter never observed reclaims a tombstone a concurrent insert still needs — `Admissible` refuses it on the entry's own causal context as `SyncFault.Unobserved`, where `Crdt.Apply`, holding no frontier, refuses nothing. LWW per column family is the default — `Held` resolves the competing local entry per model and family, content-key equality adjudicates `LocalWin` (idempotent replay — `Conflicted: false`, a pure skip), an absent competitor adjudicates `Merged` through `Fresh` whose held slots carry the `Hlc.Zero` absence sentinel, an HLC-resolved `LocalWin`/`RemoteWin` over differing content is a genuine divergence (`Conflicted: true`) the fold counts as `Conflicted` and whose `ConflictReceipt` it records even when the winner commits, and an equal `(stamp, origin)` with divergent content is the causal fork which `Apply` halts as the epoch-class `SyncFault.Forked` carrying the two divergent content keys, never a soft conflict that counts and continues; the `FirstWriter` (`Presence`) lane is EARLIEST-wins, the INVERSE comparison direction of the LWW latest-wins default, so the older `(stamp, origin)` wins regardless of arrival order — the `Adjudicate` `(comparison, isFirstWriter)` tuple-`switch` flips both the newer-incoming and the older-incoming arm for FirstWriter, never the LWW-only direction that silently keeps a later first-writer-lane row over the genuine first writer; the `Conflicted`/`Conflicts` audit fields are thus exact (every auto-resolved divergence recorded, an idempotent replay never miscounted as a conflict), not an always-empty placeholder; HLC ordering ties break on origin store id so adjudication is deterministic across peers; the `crdt` column family routes its `Payload` through `Crdt.Apply` so a concurrent edit converges by the join-semilattice least-upper-bound rather than scalar LWW (the LWW `Adjudicate` surviving only as the `LwwRegister` arm) — the multi-writer offline + IFC 3-way merge substrate; the `SpeckleSend`/`SpeckleReceive` delegates are the marshal seam binding the DI-resolved instance `IOperations.Send`/`Receive`, projecting the returned `rootObjId` content hash onto the `ContentKey` (zero second identity) and mapping the inbound `Base`/`DataObject` graph to closed Rasm op-log entries at the seam, the SDK boundary faults lifting once into `SyncFault.SpeckleMarshal`; a winning whole-relation entry (`Kind.WholeRelation` — the `Truncate` verb) commits through the session `Truncate` delegate clearing the whole `(Model, Family)` relation (the `Held` resolver answers the relation's LATEST entry for a whole-relation verb, so the truncate still adjudicates `(Hlc, OriginStoreId)` LWW against the relation head — the policy bit selects the relation-wide commit lane, never a dead flag); a `SyncEngine` service class is the rejected form — the fold and the dispatch rows own the engine.

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

// The adjudication outcome: the verdict, its typed receipt, whether the resolution arose from a genuine
// divergence (`Conflicted` — a `LocalWin`/`RemoteWin` won by HLC over differing content, or a `Rejected` fork),
// and the held content key the `Forked` fault reads without a second `Held` lookup. An idempotent-replay
// `LocalWin` (content-equal) and a `Merged` (no competitor) carry `Conflicted: false` — the apply fold counts
// them as a skip/apply, never a conflict, so the `Conflicted`/`Conflicts` audit fields stay exact.
public readonly record struct ConflictResult(ConflictVerdict Verdict, ConflictReceipt Receipt, bool Conflicted, UInt128 Held);

// --- [ERRORS] --------------------------------------------------------------------------
// The merge-engine fault band (825x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND`
// `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)`
// ctor (no `Category` to override) is the deleted form. No `[GenerateUnionOps]` — the kernel union-ops generator is
// strictly opt-in, so the band carries no per-case `SelfOp` while the `[Union]`-generated `Switch`/`Map` is
// untouched, the `Rasm.Domain.Expected` derivation making a bare case an `Error` directly so it lifts onto `Fin<T>`/`Validation`
// with no `.ToError()` hop; band membership derives `Code => FaultBand.Sync + n` through the registry row
// (`Element/graph#FAULT_TABLES` — a bare integer literal is the deleted form), `Message` projects the case
// detail, and `Category` the telemetry label, so a recovery reads `error.IsType<SyncFault.Forked>()` /
// `error.HasCode(8256)` / `error.Category`, never a message substring.
[Union]
public abstract partial record SyncFault : Rasm.Domain.Expected, IValidationError<SyncFault> {
    private SyncFault() : base() { }
    public sealed record SchemaMismatch(ulong Local, ulong Remote) : SyncFault;
    public sealed record ReplicationFaulted(string Slot, string Cause) : SyncFault;
    public sealed record SpeckleMarshal(string Peer, string Class) : SyncFault;
    public sealed record TransferDecode(string Peer, string Cause) : SyncFault;
    public sealed record Unconserved(long Batch, long Settled) : SyncFault;
    public sealed record Forked(ConflictReceipt Receipt, UInt128 Held, UInt128 Incoming) : SyncFault;
    public sealed record Unobserved(string Field, Guid Origin) : SyncFault;

    public override int Code => FaultBand.Sync + Switch(
        schemaMismatch:     static _ => 1,
        replicationFaulted: static _ => 2,
        speckleMarshal:     static _ => 3,
        transferDecode:     static _ => 4,
        unconserved:        static _ => 5,
        forked:             static _ => 6,
        unobserved:         static _ => 7);

    public override string Message => Switch(
        schemaMismatch:     static c => $"{c.Local}:{c.Remote}",
        replicationFaulted: static c => $"{c.Slot}:{c.Cause}",
        speckleMarshal:     static c => $"{c.Peer}:{c.Class}",
        transferDecode:     static c => $"{c.Peer}:{c.Cause}",
        unconserved:        static c => $"{c.Batch}!={c.Settled}",
        forked:             static c => $"{c.Receipt.EntityKey}@{c.Receipt.Incoming.Physical}:{c.Held}!={c.Incoming}",
        unobserved:         static c => $"{c.Field}@{c.Origin:N}");

    public override string Category => Switch(
        schemaMismatch:     static _ => "Schema",
        replicationFaulted: static _ => "Replication",
        speckleMarshal:     static _ => "Speckle",
        transferDecode:     static _ => "Transfer",
        unconserved:        static _ => "Conserve",
        forked:             static _ => "Fork",
        unobserved:         static _ => "Causality");

    public static SyncFault Create(string message) => new ReplicationFaulted(string.Empty, message);
}

// The per-run apply evidence, self-attesting through the kernel receipt-validity floor: `Batch` carries the run's
// input count so `IsValid` is the parameterless `ValidityClaim.All` conservation fold — the settled sum equals the
// batch AND every counted conflict carries its recorded receipt — and `OpAcceptance.ValidityOf` reads the same
// `IValidityEvidence` arm. The parameterized `Conserves(long)` knob and a hand-rolled `&&` chain are the deleted
// forms: the receipt reconstructs the check from its own fields, so a downstream consumer re-proves conservation
// without the caller's batch count.
public readonly record struct SyncApplyReceipt(long Batch, long Applied, long Skipped, long Conflicted, long Converged, long Pushed, long QueueDepth, Seq<ConflictReceipt> Conflicts, SyncCursor Cursor, SyncCursor Acked, CorrelationId Correlation, Instant At) : IValidityEvidence {
    public long Settled => Applied + Skipped + Conflicted + Converged;
    public bool IsValid => ValidityClaim.All(ValidityClaim.Of(Batch == Settled), ValidityClaim.Of(Conflicts.Count == Conflicted));
}

// The session capsule: the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame carries clock,
// correlation, and tenant as VALUES ([A.1] — a `ClockPolicy` or `Principal` field is the deleted strata
// inversion); `Truncate` is the relation-wide commit lane a winning `Kind.WholeRelation` entry takes.
// `Cursor`/`Acked` are the two per-peer cursor SPACES — `Cursor` our read position in the PEER's feed (what
// `Pull` resumes from), `Acked` the peer's confirmed position in OUR feed (what `Pending` reads) — one slot
// carrying both is the deleted collapse whose bidirectional exchange skips or re-pulls remote entries.
// Local durable legs bind the `Store/provisioning#ENGINE_OPERATIONS` `KvFloor`: `Spool` and the cursor-taking
// `Unspool` the RocksDB (`KvEngine.Lsm`) disconnected-peer pending-op buffer — `Unspool(0)` is the full reconnect
// drain over the snapshot-pinned `Scan`, and a nonzero cursor resumes from the engine's own WAL sequence
// (`KvFloor.Since`) after a partial upload, the returned head being the next call's resume cursor, so a severed
// push never replays rows the peer already acknowledged; `LocalHas` the LMDB (`KvEngine.Mmap`) content-address
// membership probe — a session whose pending rows live only in memory loses the disconnected buffer on process
// exit, the deleted form.
public sealed record SyncSession(
    ProjectionContext Frame, ReceiptSinkPort Sink, Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, SyncCursor Acked, CancellationToken Token,
    Func<VersionVector> Frontier,
    Func<UInt128, bool> Holds, Func<OpLogEntry, Option<OpLogEntry>> Held, Func<OpLogEntry, IO<Unit>> Commit, Func<OpLogEntry, IO<Unit>> Truncate, Func<OpLogEntry, IO<ConflictResult>> Converge,
    Func<SyncCursor, Seq<OpLogEntry>> Pending, Func<long> QueueDepth, Func<UInt128, IO<OpLogEntry>> Fetch,
    Func<Seq<OpLogEntry>, IO<Unit>> Spool, Func<ulong, IO<(ulong Head, Seq<OpLogEntry> Entries)>> Unspool, Func<UInt128, IO<bool>> LocalHas,
    Func<string, SyncCursor, IO<(ulong SchemaFingerprint, Seq<OpLogEntry> Entries, SyncCursor Cursor)>> Pull,
    Func<string, Seq<OpLogEntry>, IO<SyncCursor>> Push, Func<string, Seq<UInt128>, IO<Seq<UInt128>>> HasObjects,
    Func<string, Seq<OpLogEntry>, IO<(UInt128 RootContentKey, long ConvertedReferences)>> SpeckleSend,
    Func<string, UInt128, IO<Seq<OpLogEntry>>> SpeckleReceive);

// Apply-fold accumulator, named once: four settled counters, the recorded receipts, and the applied frontier the
// replay test reads, so the fold's three halves state one type instead of re-spelling a six-slot tuple at every
// arrow.
internal readonly record struct Counts(VersionVector Frontier, long Applied, long Skipped, long Conflicted, long Converged, Seq<ConflictReceipt> Conflicts);

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

    // Compaction retires only what its minter observed. `Maintain` carries a quiescence vector, and an entry whose own
    // causal context fails to dominate it claims a horizon the minter never saw — applying it reclaims a tombstone one
    // concurrent insert still needs and resurrects a deleted element on whichever peer folds it. The entry's
    // `OperationId.Context` is the only evidence that check has ever had: `Crdt.Apply` holds no frontier and
    // structurally cannot run it, so the gate lives at the entry and the composition root's `Converge` binding
    // composes it over the op it already decoded rather than decoding a second time here.
    public static Fin<Unit> Admissible(OperationId id, CrdtOp op) =>
        op is CrdtOp.Maintain compaction && !id.Context.Dominates(compaction.Quiescent)
            ? Fin.Fail<Unit>(new SyncFault.Unobserved(compaction.Field, id.Origin))
            : Fin.Succ(unit);

    // Idempotent-convergent apply fold, three-way by construction. A dot already under the applied frontier is
    // `Skipped` — identity-proven replay, since content equality cannot separate a redelivery from a second real edit
    // carrying identical bytes and would drop the edit. Every surviving entry lands through ONE `ConflictResult`
    // shape, the crdt lane's `Converge` and the scalar lane's `Adjudicate` both producing it, so one switch consumes
    // both lanes where a convergent short-circuit beside a four-arm ladder read the lane twice. Each entry lands in
    // EXACTLY ONE counter, so the receipt's `IsValid` conservation fold is exact: a `Rejected` fork halts the apply on
    // epoch-class `SyncFault.Forked` carrying both divergent content keys read off the result (never a second `Held`
    // lookup); a `Conflicted` result records its receipt whether the winner committed or the local held; a
    // non-applying verdict over unchanged state is `Skipped`; an applying verdict is `Converged` on a convergent lane
    // and `Applied` otherwise, a winning `Kind.WholeRelation` entry committing through `Truncate`. The frontier joins
    // each landed entry's own `Frontier`, so a batch carrying its own duplicate skips the second copy with no lookup.
    public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming) =>
        incoming.FoldM(
            new Counts(session.Frontier(), Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Conflicts: Seq<ConflictReceipt>()),
            (counts, entry) => entry.Id.Applied(counts.Frontier)
                ? IO.pure(counts with { Skipped = counts.Skipped + 1L })
                : (entry.Family.Stance.Convergent ? session.Converge(entry) : IO.pure(Adjudicate(session, entry)))
                    .Bind(result => Landed(session, entry, result, counts)))
            .Map(c => new SyncApplyReceipt(incoming.Count, c.Applied, c.Skipped, c.Conflicted, c.Converged, Pushed: 0L, session.QueueDepth(), c.Conflicts, session.Cursor, session.Acked, session.Frame.Correlation, session.Frame.Now()))
            .Bind(receipt => receipt.IsValid ? IO.pure(receipt) : IO.fail<SyncApplyReceipt>(new SyncFault.Unconserved(receipt.Batch, receipt.Settled)))
            .As();

    // Commit-then-count half. Only a non-convergent applying verdict reaches `Commit`/`Truncate`: the `Converge`
    // delegate has already landed its own merge, so committing again would double-write the crdt lane.
    static IO<Counts> Landed(SyncSession session, OpLogEntry entry, ConflictResult result, Counts counts) =>
        result.Verdict == ConflictVerdict.Rejected
            ? IO.fail<Counts>(new SyncFault.Forked(result.Receipt, result.Held, entry.ContentKey))
            : (result.Verdict.Applies && !entry.Family.Stance.Convergent
                    ? entry.Kind.WholeRelation ? session.Truncate(entry) : session.Commit(entry)
                    : IO.pure(unit))
                .Map(_ => Counted(counts with { Frontier = counts.Frontier.Join(entry.Id.Frontier) }, entry, result));

    static Counts Counted(Counts counts, OpLogEntry entry, ConflictResult result) =>
        result.Conflicted ? counts with { Conflicted = counts.Conflicted + 1L, Conflicts = counts.Conflicts.Add(result.Receipt) }
        : !result.Verdict.Applies ? counts with { Skipped = counts.Skipped + 1L }
        : entry.Family.Stance.Convergent ? counts with { Converged = counts.Converged + 1L }
        : counts with { Applied = counts.Applied + 1L };
}
```

| [INDEX] | [POLICY]                | [VALUE]                                                 | [BINDING]                                          |
| :-----: | :---------------------- | :------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | scalar default          | LWW `(Hlc, OriginStoreId)`; `FirstWriter` earliest-wins | deterministic total order across peers             |
|  [02]   | crdt lane               | `Crdt.Apply` join-semilattice                           | converges by merge; multi-writer offline substrate |
|  [03]   | causal fork             | equal `(stamp, origin)` divergent content               | `SyncFault.Forked` halts merge                     |
|  [04]   | conservation            | receipt `IsValid` — `ValidityClaim.All` over `Batch`    | a breach is `SyncFault.Unconserved`                |
|  [05]   | whole-relation truncate | `Kind.WholeRelation` → session `Truncate` delegate      | clears `(Model, Family)`; `Held` answers the head  |
|  [06]   | replay dedup            | `Id.Applied(frontier)` — identity, never content        | equal payloads both land; a redelivery lands once  |
|  [07]   | commutation source      | lane `Stance.Law`; crdt arm `Crdt.Law`                  | one `OpLaw` triple, three runtime transcriptions   |
|  [08]   | compaction admission    | `Id.Context.Dominates(Maintain.Quiescent)`              | else `SyncFault.Unobserved`; the fold cannot check |

## [04]-[SYNC_TRANSPORTS]

- Owner: `SyncFlow` the keyless disposition carrying the `(Pulls, Pushes)` policy pair; `SyncTransport` `[Union]`; the `SyncPump` dispatch surface with the `SubtreeFetch` graph-checkout bridge and the `Offer` Speckle-diff arm; `GraphDiff` the named set-difference diff-algebra `SubtreeFetch` and `Offer` both dial.
- Cases: 3 transport cases — `HttpDelta`, `SpeckleLikeDiff`, `SubtreeCheckout` — widened by the one `SyncFlow` field whose `Pulls`/`Pushes` policy pair the `Exchange` fold reads; fan-in/fan-out/bidirectional are `SyncFlow` rows, never new transport cases.
- Entry: `public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport)` is one total state-threaded dispatch; `public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds)` projects the missing geometry-BLOB-key manifest (the `Closure` plus the root payload key, minus held); `public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root)` fetches the root entry, applies it onto the target, and accounts the blob manifest on the receipt.
- Auto: intra-cluster replication is Marten's own daemon (`DaemonMode.HotCold`) over the shared PostgreSQL, so this transport axis is the CROSS-store / offline lane (a disconnected editor, a Speckle hub, a peer holding a subgraph), never a re-implementation of single-cluster replication; `HttpDelta` pulls a `Cursor`-bounded segment of the peer's feed and pushes the pending set past the peer-acked `Acked` frontier gated by `SyncFlow` — the session carries BOTH cursor spaces (`Cursor` the pull resume point, `Acked` the push frontier `Pending` reads), and one slot serving both is the deleted collapse whose bidirectional exchange skips or re-pulls remote entries; `SubtreeCheckout` fetches the root op-log entry, APPLIES it (the delta is the change), and accounts its `Closure` geometry-blob manifest as the blob-transfer set the content-addressed blob store moves — the `Closure` is a representation-content-hash blob manifest, never an op-log-entry fetch input, so a checkout applies the one entry and the blobs ride the blob store, never a `Fetch` of a blob key as an entry; `SpeckleLikeDiff` folds the pending set through `GraphDiff` over the peer `HasObjects` membership and hands the missing set to the `SpeckleSend` marshal.
- Receipt: every transport run yields one `SyncApplyReceipt`; the subtree-checkout transfer count rides the same receipt.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, Speckle.Sdk (companion, outside-Rhino), BCL inbox.
- Growth: a new transport is one case plus one dispatch arm; a new graph-checkout shape is one entry over `GraphDiff`, never a second diff algebra; zero new surface.
- Boundary: `HttpDelta` rides the AppHost `OutboundHop` keyed pipeline — retry, backoff, and hop deadlines are owned there and the database stays excluded from the hop law; the document-granular fallback is the RFC 6902 patch payload subordinate to the changefeed; `GraphDiff` is the one set-difference diff-algebra (the `Closure` GEOMETRY-blob manifest plus the root payload key, minus the target `Holds` set) — the BLOB-transfer set the content-addressed blob store moves, NOT an op-log-entry fetch input; `SubtreeFetch` fetches the root entry, applies it, and accounts that blob set on the receipt, its `root` argument discriminating which subtree's entry+blobs transfer — feeding the blob manifest to the op-log `Fetch` (which resolves an entry, never a geometry blob) or a second walk-and-diff being the deleted form; the `SpeckleLikeDiff` `Offer` rides the same one diff algebra and the wire leg lives OUTSIDE-RHINO on the companion target where `Speckle.Sdk.Dependencies` repacks the closure, so the in-Rhino assembly composes only the case and the marshal delegate slot and never references `Speckle.Sdk`/`Speckle.Objects`, the DI-resolved INSTANCE `IOperations.Send` (never the non-existent static `Operations.Send`) returning a `(rootObjId, convertedReferences)` tuple whose `rootObjId` projects onto the offered root `ContentKey` at the marshal seam with zero second identity (`Offer` faults `SyncFault.SpeckleMarshal` when the returned root key drifts from the offered head's local key); the wire leg of `GraphDiff`/`SubtreeFetch` is owned at `Rasm.Compute/Runtime/wire#PROTO_VOCABULARY` so the remote rpc dials this algebra and never re-implements the set-difference.

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

    // TWO cursor spaces thread the exchange: the pull leg advances `Cursor` (our position in the PEER's feed,
    // `segment.Cursor`) and the push leg advances `Acked` (the peer-returned confirmation in OUR feed) — the
    // pending set reads `Acked`, never the pull cursor. Overwriting `Cursor` with the push ack is the deleted
    // collapse: the next bidirectional pull would resume from OUR push frontier inside the PEER's sequence
    // space, silently skipping every peer entry between the two positions.
    static IO<SyncApplyReceipt> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        from pulled in row.Flow.Pulls
            ? s.Pull(row.Peer, s.Cursor).Bind(segment => segment.SchemaFingerprint == s.SchemaFingerprint
                ? SyncMerge.Apply(s, segment.Entries).Map(receipt => receipt with { Cursor = segment.Cursor })
                : IO.fail<SyncApplyReceipt>(new SyncFault.SchemaMismatch(s.SchemaFingerprint, segment.SchemaFingerprint)))
            : IO.pure(new SyncApplyReceipt(0L, 0L, 0L, 0L, 0L, 0L, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor, s.Acked, s.Frame.Correlation, s.Frame.Now()))
        let pending = s.Pending(s.Acked)
        from receipt in row.Flow.Pushes
            ? s.Push(row.Peer, pending).Map(acked => pulled with { Pushed = pending.Count, Acked = acked })
            : IO.pure(pulled)
        select receipt;

    // Offer drains the `Acked` frontier and, on a confirmed send, advances `Acked` to the LAST offered
    // entry's real `(Sequence, Physical, Logical)` stamp — the peer now holds the whole pending set (missing
    // sent, the rest already held), so the frontier is the last entry's own coordinates, never a fabricated
    // `Sequence + count` arithmetic advance in the wrong cursor space.
    static IO<SyncApplyReceipt> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        from pending in IO.pure(s.Pending(s.Acked))
        from held in s.HasObjects(row.Peer, toSeq(pending.Fold(Seq<UInt128>(), static (set, entry) => set + GraphDiff(entry, static _ => false)).Distinct()))
        let missing = pending.Filter(entry => !held.Contains(entry.ContentKey))
        from sent in s.SpeckleSend(row.Peer, missing)
        from receipt in missing.Head.Map(h => h.ContentKey) is { IsSome: true, Case: UInt128 root } && root != sent.RootContentKey
            ? IO.fail<SyncApplyReceipt>(new SyncFault.SpeckleMarshal(row.Peer, $"root-key-drift:{root}!={sent.RootContentKey}:refs={sent.ConvertedReferences}"))
            : IO.pure(new SyncApplyReceipt(0L, 0L, 0L, 0L, 0L, missing.Count, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor,
                pending.Last.Map(last => s.Acked with { Sequence = last.Sequence, Physical = last.Physical, Logical = last.Logical }).IfNone(s.Acked),
                s.Frame.Correlation, s.Frame.Now()))
        select receipt;
}
```

| [INDEX] | [POLICY]                  | [VALUE]                                            | [BINDING]                                               |
| :-----: | :------------------------ | :------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | intra-cluster replication | Marten daemon `HotCold`                            | this axis is the cross-store/offline lane only          |
|  [02]   | graph checkout            | fetch+apply root; `GraphDiff` is the BLOB manifest | `Closure` set via the blob store, not an op-log `Fetch` |
|  [03]   | Speckle marshal           | DI-resolved instance `IOperations`                 | outside-Rhino; `rootObjId` → `ContentKey`; drift faults |
|  [04]   | http delta                | AppHost `OutboundHop` pipeline                     | database excluded from the hop law                      |

Per-transport descriptor: the sentence a row is chosen on, its guarantee, and what settles a leg. `admit` is `SyncPump.Run` for all three and `tenancy` is the peer identity the session carries, so both stay uniform here and only the differing coordinates earn columns.

| [INDEX] | [TRANSPORT]       | [FITS]                              | [DELIVER]                           | [SETTLE]                          |
| :-----: | :---------------- | :---------------------------------- | :---------------------------------- | :-------------------------------- |
|  [01]   | `HttpDelta`       | a peer holding its own feed         | at-least-once past `Acked`          | peer-returned push ack            |
|  [02]   | `SpeckleLikeDiff` | a hub deduplicating by object graph | at-least-once past `Acked`          | `rootObjId` matching the head key |
|  [03]   | `SubtreeCheckout` | a peer materializing one subgraph   | one entry, blobs by content address | applied entry + blob manifest     |

Where a re-drive resumes, and the honest give-up clause each row carries.

| [INDEX] | [TRANSPORT]       | [REPLAY]                        | [DEGRADE]                                               |
| :-----: | :---------------- | :------------------------------ | :------------------------------------------------------ |
|  [01]   | `HttpDelta`       | `Cursor` pull, `Acked` push     | two cursor spaces; collapsing them skips peer entries   |
|  [02]   | `SpeckleLikeDiff` | `Acked` frontier only           | outside-Rhino only; a root-key drift faults the run     |
|  [03]   | `SubtreeCheckout` | none; the caller names the root | blob transfer is the blob store's, never accounted here |

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

| [INDEX] | [POLICY]             | [VALUE]                                         | [BINDING]                                                    |
| :-----: | :------------------- | :---------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | lossy awareness      | `DropOldest` `DrainSpec` lane, `onDrop` receipt | never a durable changefeed row; distinct from `EphemeralMap` |
|  [02]   | presence ttl         | stamp + `Ttl`, heartbeat sweep                  | one ephemeral row, never a transport                         |
|  [03]   | working-set checkout | `ReplicationQuery` → key set                    | one subgraph, never the whole graph                          |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
