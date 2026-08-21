# [PERSISTENCE_VERSION_LEDGER]

`Rasm.Persistence.Version` projects Marten events into one durable `OpLogEntry` feed and owns every convergence decision over that feed. `ColumnFamily` binds payload codec and merge stance; `ReplayWindow` parameterizes every bounded read; `SyncMerge` folds scalar, first-writer, and CRDT entries through one closed fault rail; `SyncTransport` carries cross-store exchange; `Awareness` owns ephemeral collaboration. `ProjectionContext` supplies time, correlation, and tenant evidence, while `ContentHash.Of` supplies payload identity.

## [01]-[INDEX]

- [02]-[CHANGEFEED]: `OpLogEntry` projects Marten events, with the `OpSlot` sign boundary, HLC stamping, the trace slot, the closure manifest, and the `ReplayWindow` windowed read.
- [03]-[MERGE_LAW]: LWW adjudication, conflict receipts, the idempotent apply fold, CRDT dispatch, and the conservation invariant.
- [04]-[SYNC_TRANSPORTS]: `SyncTransport` closes the transport family over its `SyncFlow` capability set, the subtree-checkout diff algebra, and the Speckle marshal seam.
- [05]-[PRESENCE]: ephemeral presence rows, the lossy awareness lane, and the working-set checkout.

## [02]-[CHANGEFEED]

- Owner: `SyncCapability` the write-verb capability vocabulary and `SyncOps` its legal corners; `SyncOpKind` the write-verb axis carrying its capability set and its `Fact` announcement spelling; `OpCapability`/`OpLaws` and `OpLaw` the estate's one commutation vocabulary; `ColumnFamily` the merge-lane axis carrying `MergeStance` AND `SnapshotCodec` as one row, so the lane selects adjudication algebra and payload codec together; `TraceSlot` the changefeed W3C trace-id carrier; `OpSlot` the ONE sign boundary between the `long` version-vector slot space and the `ulong` dot space; `OperationId` the dot-plus-frontier operation identity; `DotSource` the store's one dot minter; `EventFacts` the admitted Marten-event evidence; `OpLogMapper` the generated `EventFacts`→`OpLogEntry` transcription; `OpLogEntry` the changefeed record; `ReplayWindow` the one windowed-read parameterization; `ChangefeedSubscription` the `SubscriptionBase` batched drain; the `OpLog` project-stamp-replay surface.
- Cases: `SyncOpKind` is `Upsert | Delete | Truncate | Presence`, each carrying its past-tense `Fact` and its held `SyncCapability` set — `{}`, `{Tombstone}`, `{Tombstone, WholeRelation}` are the three legal corners, so a whole-relation verb that is not a tombstone is unrepresentable rather than merely unwritten. `OpLaw` is `Ordered | Commutative | Semilattice`, the same triple `Version/commits#CRDT_ALGEBRA` `Crdt.Law` returns and `typescript:core/state/merge` `Merge.Law` spells, each row holding its `OpCapability` corner. `MergeStance` is `Lww(Ordered) | Crdt(Semilattice) | FirstWriter(Ordered)` and derives `Convergent` off the law. `ColumnFamily` closes at `Scalar | Crdt | Geometry | Presence | Commit | Branch | Attest`, each carrying stance, codec, and durability, so a consumer dispatches on the row and never a string compare.
- Entry: `ProcessEventsAsync` is the `SubscriptionBase` override reserving the range's dots in one swap, projecting the WHOLE delivered range into one `Seq<OpLogEntry>`, and draining it once — per-event awaits inside a range are the deleted form; `DotSource.Reserve(count)` is the ONE identity mint both the projection and the authoring path take and the ONE place a `long` vector slot is admitted; `OperationId.Mint(origin, prior, context)` derives the dot as the frontier's next slot and `Order(left, right)` answers happened-before from two ids alone; `OpLog.Project(dots, events)` accumulates the range's admissions applicatively; `Replay(feed, window)` is the ONE windowed changefeed read; `Stamp(sink, dots, frame, build)` is the authoring-path mint; `Appended(deltas, basis, dots, carrier, frame)` composes the seam `GraphCrossing.Mint` per crossing; `TransferSet(entry, holds)` projects the closure-minus-held missing-key set.
- Auto: a Marten async subscription projects each committed model event into the one feed; triggers, secondary op-log tables, and per-payload records are inadmissible (`H11`). Every projected `GraphEvent` is a structural `geometry`-lane change carrying the codec-encoded `GraphDelta`, adjudicated by `(Hlc, OriginStoreId)` LWW; `crdt` and `commit` payloads enter through their owners' `Stamp`. `Closure` carries descendant geometry content keys, so transfer is set difference rather than a tree walk. Every foreign column of a Marten event admits ONCE into `EventFacts` — stream key, actor header, correlation, and stream version each on the `Validation` rail — so a malformed event reports every bad column at once and the generated mapper builds the entry from evidence alone.
- Law: message envelopes ANNOUNCE an entry and gain no authority over it. `OpLogEntry` and its codec-encoded `Payload` stay the evidence truth the corpus `tests/contracts` `OPLOG_ENTRY` entry freezes — the thirteen-slot record in declaration order under its lane's `SnapshotCodec` — while a `CloudEvent` PROJECTS that entry: `subject` re-renders the payload content key and `id` the dot. Envelope columns never enter the corpus preimage and never re-key an entry; a divergence convicts the projection, never the ledger, because only one of the two holds bytes anyone signed.
- Receipt: changefeed position and queue depth ride `SyncApplyReceipt`; the projected-segment evidence rides `ReceiptSinkPort`.
- Packages: Marten (`SubscriptionBase`/`ProcessEventsAsync`/`EventRange`/`ISubscriptionController`/`IDocumentOperations`/`IChangeListener`/`NullChangeListener`/`IEvent<T>`), Rasm.Element (`GraphDelta.Address` — the streamed order-independent delta address; `Node`/`Node.Object`/`RepresentationContentHash`; `Graph/wire#EVENT_ENVELOPE` `GraphCrossing.Mint`/`GraphEventType`/`CrossingPorts`), Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher; `CapabilitySet`/`CapabilityLaw`; `FaultBand`), Riok.Mapperly (`[Mapper]` + `[MapperConstructor]` + `[MapProperty]`/`[MapValue]` — the entry transcription), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, System.Diagnostics.DiagnosticSource, BCL inbox.
- Growth: a new synced concern is one `SyncOpKind` verb with its capability corner, one `ColumnFamily` lane carrying its stance and codec columns, or one payload kind keyed by the lane row's `Codec`; a new windowed-read consumer is one `ReplayWindow` parameterization; a new admitted event column is one `EventFacts` member and one `[MapProperty]` row. Zero new surface — a per-entity-kind outbox table, a bespoke op-log store beneath Marten, a per-payload-kind parallel record, a second dot minter, a second sign boundary, or a per-lane string literal in the merge fold is the deleted form.
- Boundary: `OperationId` is the entry key and `ContentKey` the payload key, and the two never merge — two peers stamping the identical `Set("name", "North Wing")` share one `ContentKey` and carry two dots, so the second edit survives where a content-keyed log drops it and reads the drop as successful dedup. `Counter` is the origin's own `VersionVector` slot rather than a second counter, and `Context` is the pre-mint frontier, so `Order` is total from two ids with no feed walk and `Applied` is the exact replay test the merge fold takes. `Sequence` survives as the store-local drain cursor ALONE — a resumable position, never an identity, because two stores mint sequence 41 and one entry cannot answer to both. `DotSource` is the store's single minter, so the gap-free dot law holds across the projection and the authoring path, and a restart re-seeds its atom from the durable head joined with the tail past the cursor.
- Boundary: `OpSlot` is the ONE sign boundary this feed carries. Version vectors publish `long` slots while every dot is `ulong`, and an unchecked `(long)Counter` past `long.MaxValue` reads NEGATIVE — a dominance test then answers "already applied" for an operation nobody applied, silently dropping it. Construction caps the carrier at `long.MaxValue`, so `Signed` is total and no cast survives anywhere on the page; the mirror of the AppHost port-side `[ValueObject<ulong>]` admission, landed store-side because no AppHost type crosses down. `Counter` still projects the identical `ulong` onto the wire and the identical eight little-endian bytes into the preimage, so the frozen thirteen-slot roster is untouched.
- Boundary: Marten's `origin` header no longer reaches the entry — `OriginStoreId` reads the dot's own `Id.Origin`, so the LWW tie-break is deterministic across peers and no missing header fabricates the `Guid.Empty` bucket that collapsed every origin into one. Marten events PROJECT the changefeed (`H11`): a projected `GraphEvent` is the structural `geometry`-lane `GraphDelta`, never a `crdt` op, while the `crdt` lane carries a `CrdtOp`, the `commit` lane a `CommitNode`, and the `attest` lane a `WitnessedHead` its owner `Stamp`s. Persistence only READS `Activity.Current` and projects to the `TraceSlot` VALUE, never re-minting the propagator the AppHost `TraceContext` fold owns; the 16-byte trace-id admits once through the TOTAL `TraceSlot.FromHex`, so an arbitrary correlation string yields `Empty` rather than faulting the subscription daemon. `OpLogEntry` carries no correlation field: correlation rides the session frame and receipts.
- Boundary: the durable lanes (`Family.Durable`) are the exactly-once CDC row source the `Version/egress` pump drains past the `Store/coordination#OUTBOX_CURSOR`, and `ReplayWindow.DurableOps` is that drain's parameterization; the presence/awareness lane (`durable: false`) stays the lossy `DrainSurface` channel and NEVER the exactly-once CDC envelope.

```csharp signature
// --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using LanguageExt;
using Marten.Events;
using Marten.Subscriptions;
using NodaTime;
using Rasm.Domain;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Version;

// --- [TYPES] -----------------------------------------------------------------------------
// `{WholeRelation}` without `{Tombstone}` is the corner two bool columns could hold while no merge arm answered it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SyncCapability : ICapability<SyncCapability> {
    public static readonly SyncCapability Tombstone = new("tombstone");
    public static readonly SyncCapability WholeRelation = new("whole-relation");
}

public static class SyncOps {
    public static readonly CapabilitySet<SyncCapability> Live = CapabilitySet<SyncCapability>.None;
    public static readonly CapabilitySet<SyncCapability> Retires = CapabilitySet<SyncCapability>.Of(SyncCapability.Tombstone);
    public static readonly CapabilitySet<SyncCapability> Clears = CapabilitySet<SyncCapability>.All;
    public static readonly CapabilityLaw<SyncCapability> Law = new(Seq(Live, Retires, Clears));
}

// `Fact` is a COLUMN, not a derivation: no suffix rule turns every verb here into its own past tense.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SyncOpKind {
    public static readonly SyncOpKind Upsert = new("upsert", "upserted", SyncOps.Live);
    public static readonly SyncOpKind Delete = new("delete", "deleted", SyncOps.Retires);
    public static readonly SyncOpKind Truncate = new("truncate", "truncated", SyncOps.Clears);
    public static readonly SyncOpKind Presence = new("presence", "observed", SyncOps.Live);
    public string Fact { get; }
    public CapabilitySet<SyncCapability> Ops { get; }
    private SyncOpKind(string key, string fact, CapabilitySet<SyncCapability> ops) : this(key) => (Fact, Ops) = (fact, ops);

    public static readonly Fin<Unit> Lawful =
        toSeq(Items).Traverse(static row => SyncOps.Law.Admit(row.Ops)).As().Map(static _ => unit);
}

// `Absorb` presupposes `Commute`, so `{Absorb}` alone is barred — what a `(bool, bool)` pair left to inspection.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OpCapability : ICapability<OpCapability> {
    public static readonly OpCapability Commute = new("commute");
    public static readonly OpCapability Absorb = new("absorb");
}

public static class OpLaws {
    public static readonly CapabilitySet<OpCapability> Sequential = CapabilitySet<OpCapability>.None;
    public static readonly CapabilitySet<OpCapability> Reorderable = CapabilitySet<OpCapability>.Of(OpCapability.Commute);
    public static readonly CapabilitySet<OpCapability> Absorbing = CapabilitySet<OpCapability>.All;
    public static readonly CapabilityLaw<OpCapability> Law = new(Seq(Sequential, Reorderable, Absorbing));
}

// ONE commutation vocabulary estate-wide. The ROW names are the cross-runtime contract (`Version/commits` `Crdt.Law`,
// `typescript:core/state/merge` `Merge.Law`); the capability set is the C#-side column shape.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OpLaw {
    public static readonly OpLaw Ordered = new("ordered", OpLaws.Sequential);
    public static readonly OpLaw Commutative = new("commutative", OpLaws.Reorderable);
    public static readonly OpLaw Semilattice = new("semilattice", OpLaws.Absorbing);
    public CapabilitySet<OpCapability> Ops { get; }
    private OpLaw(string key, CapabilitySet<OpCapability> ops) : this(key) => Ops = ops;

    public static readonly Fin<Unit> Lawful =
        toSeq(Items).Traverse(static row => OpLaws.Law.Admit(row.Ops)).As().Map(static _ => unit);
}

[SmartEnum]
public sealed partial class MergeStance {
    public static readonly MergeStance Lww = new(OpLaw.Ordered);
    public static readonly MergeStance Crdt = new(OpLaw.Semilattice);
    public static readonly MergeStance FirstWriter = new(OpLaw.Ordered);
    public OpLaw Law { get; }
    // Derived: a lane carrying both states a stance the fold and the receipt then disagree about.
    public bool Convergent => Law.Ops.Admits(OpCapability.Absorb);
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
    // One durable row carries a `Version/provenance#ATTESTED_LEDGER` `WitnessedHead`, so the ORDINARY pump carries
    // it.
    public static readonly ColumnFamily Attest = new("attest", MergeStance.Lww, SnapshotCodec.MessagePackBinary, durable: true);
    public MergeStance Stance { get; }
    public SnapshotCodec Codec { get; }
    public bool Durable { get; }
    private ColumnFamily(string key, MergeStance stance, SnapshotCodec codec, bool durable) : this(key) => (Stance, Codec, Durable) = (stance, codec, durable);
}

// One sign boundary spans the vector's `long` slots and the feed's `ulong` dots, mirroring the AppHost port-side
// admission store-side. The cap makes `Signed` TOTAL: the unchecked cast it replaces read NEGATIVE past
// `long.MaxValue`, so a dominance test answered "applied" for an operation nobody applied.
[ValueObject<ulong>]
public readonly partial struct OpSlot {
    public static readonly OpSlot Zero = Create(0UL);
    public long Signed => (long)Value;
    public OpSlot Next => Create(Value + 1UL);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ulong value) {
        if (value > (ulong)long.MaxValue) validationError = new ValidationError($"<op-slot-unsigned:{value}>");
    }

    // Total by construction: a non-negative `long` cast to `ulong` is always inside the validator's cap, which is
    // exactly why the cap is `long.MaxValue` and not `ulong.MaxValue`.
    public static Validation<Error, OpSlot> Of(long signed) =>
        signed >= 0L
            ? Success<Error, OpSlot>(Create((ulong)signed))
            : Fail<Error, OpSlot>(new SyncFault.SlotOutOfSpace(signed));
}

// This SLOT stays distinct from the AppHost `Observability/telemetry#CORRELATION_SPINE` PROPAGATION fold: `TraceId`
// holds the 16-byte id, not the 55-char `traceparent` header.
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

    // TOTAL: the span `OperationStatus` overload reports a partial decode as a VALUE, where the throwing
    // array-returning overload faults the daemon on a legitimately arbitrary 32-char correlation.
    public static TraceSlot FromHex(string? correlation) {
        if (correlation is not { Length: 32 }) { return Empty; }
        byte[] span = new byte[16];
        return Convert.FromHexString(correlation, span, out _, out _) == OperationStatus.Done
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

// --- [MODELS] ----------------------------------------------------------------------------
// Identity derived apart from payload identity. Keying on `ContentKey` makes two peers stamping the identical
// `Set(field, value)` ONE operation and drops the second edit; keying on a store-local `Sequence` makes two stores
// mint one id; keying on `(Hlc, Origin)` binds identity to a clock a restart can rewind.
[ComplexValueObject]
public sealed partial class OperationId {
    public static readonly OperationId Genesis = new(Guid.Empty, OpSlot.Zero, VersionVector.Empty);
    public Guid Origin { get; }
    public OpSlot Counter { get; }
    public VersionVector Context { get; }

    // Gap-free: a skipped counter is a lost operation refused at admission, never a hole a later test reads as met.
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid origin, ref OpSlot counter, ref VersionVector context) {
        validationError = OpSlot.Of(context.At(origin)).Match(
            Succ: prior => counter == prior.Next || counter == OpSlot.Zero ? null : new ValidationError($"<operation-dot-gap:{counter.Value}>"),
            Fail: faults => new ValidationError(string.Join(" | ", faults.Map(static fault => fault.Message))));
    }

    public static OperationId Mint(Guid origin, OpSlot prior, VersionVector context) => Create(origin, prior.Next, context);

    // Frontier AFTER this operation: the next id's context, and what a receiver joins once the entry lands.
    public VersionVector Frontier => Context.Advance(Origin, 1L);
    public bool Applied(VersionVector frontier) => frontier.At(Origin) >= Counter.Signed;

    public static VectorOrder Order(OperationId left, OperationId right) =>
        (left.Applied(right.Context), right.Applied(left.Context)) switch {
            (true, true) => VectorOrder.Equal,
            (true, false) => VectorOrder.Before,
            (false, true) => VectorOrder.After,
            _ => VectorOrder.Concurrent,
        };

    // `Key` is the INDEX projection and `Wire` the envelope `id`; a digest cannot answer which origin minted.
    public UInt128 Key => ContentHash.Of(this, static (id, writer) => id.CanonicalBytes(writer));
    public string Wire => string.Create(CultureInfo.InvariantCulture, $"{Origin:N}.{Counter.Value}");

    // Framed origin, LE counter, then the vector through the SAME writer the commit preimage takes, so a slot-order
    // edit cannot land on one key and not the other. `Counter.Value` is the frozen eight bytes.
    public void CanonicalBytes(CanonicalWriter writer) {
        writer.String(Origin.ToString("N")).U128(Counter.Value);
        Context.CanonicalBytes(writer);
    }

    public void WriteTo(IBufferWriter<byte> sink) {
        CommitGraph.Framed(sink, Origin.ToString("N"));
        BinaryPrimitives.WriteUInt64LittleEndian(sink.GetSpan(8), Counter.Value);
        sink.Advance(8);
        Context.WriteTo(sink);
    }
}

// Admitted Marten evidence: every foreign column crosses ONCE on the `Validation` rail, so a malformed event reports
// every bad column at once and the interior never re-reads a nullable substrate field.
public readonly record struct EventFacts(
    long Sequence, OperationId Dot, ModelId Model, string EntityKey, SyncOpKind Kind,
    ReadOnlyMemory<byte> Payload, UInt128 ContentKey, TraceSlot Trace, Seq<UInt128> Closure,
    string Actor, Instant Physical, OpSlot Logical);

// One changefeed record carries every projected event. `(Family, Codec)` discriminates the payload shape, so the
// merge fold reads the lane row and never a per-payload record; `Id` is the operation identity and `ContentKey` the
// payload identity, and collapsing them turns a repeated edit into a dropped one. No before-image field: the receipt
// IS the evidence.
[method: MapperConstructor]
public sealed record OpLogEntry(
    long Sequence, OperationId Id, ModelId Model, string EntityKey, ColumnFamily Family, SyncOpKind Kind,
    ReadOnlyMemory<byte> Payload, UInt128 ContentKey, TraceSlot Trace, Seq<UInt128> Closure,
    string Actor, Instant Physical, ulong Logical) {
    public Hlc Stamp => new(Physical, Logical);
    // Identity-derived: a peer resuming a feed and a peer ordering an operation read different columns on purpose.
    public Guid OriginStoreId => Id.Origin;
    // Family-derived: a payload can never carry a codec its lane disagrees with.
    public SnapshotCodec Codec => Family.Codec;
}

public sealed record SyncCursor(Guid OriginStoreId, long Sequence, Instant Physical, ulong Logical) {
    public static readonly SyncCursor Genesis = new(Guid.Empty, 0L, Instant.MinValue, 0UL);
}

// Three consumers, one case: the AppUi per-doc replay, the AppHost neutral-log read, and the egress CDC drain.
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

// --- [SERVICES] --------------------------------------------------------------------------
// One dot minter per store: the projection and the authoring `Stamp` both reserve through it. The atom seeds from the
// durable head joined with the tail past the drain cursor, so a restart resumes instead of re-minting dots a peer
// holds; a caller keeping its own frontier copy diverges on first mint.
public sealed class DotSource(Guid origin, Atom<VersionVector> frontier) {
    public VersionVector Frontier => frontier.Value;

    // ONE swap per range: a per-entry swap lets the other path interleave a dot into the middle, which is exactly the
    // gap the id refuses. This is also the ONE place a `long` vector slot admits into the dot space, and BOTH
    // endpoints admit, so `Next` is total across the run and exhaustion refuses at the boundary.
    public Validation<Error, Seq<OperationId>> Reserve(int count) {
        VersionVector opening = frontier.Swap(held => held.Advance(origin, count)).Advance(origin, -count);
        return (OpSlot.Of(opening.At(origin)), OpSlot.Of(opening.At(origin) + count))
            .Apply(static (low, _) => low)
            .Map(prior => toSeq(Enumerable.Range(0, count)).Fold(
                (Slot: prior, Ids: Seq<OperationId>()),
                (state, step) => (state.Slot.Next, state.Ids.Add(OperationId.Mint(origin, state.Slot, opening.Advance(origin, step))))).Ids)
            .As();
    }
}

public sealed class ChangefeedSubscription(DotSource dots, Func<Seq<OpLogEntry>, IO<Unit>> drain) : SubscriptionBase {
    // ONE batched fold per range; the admission `Validation` collapses to `Fin` HERE, at the daemon seam.
    public override async Task<IChangeListener> ProcessEventsAsync(EventRange range, ISubscriptionController controller, IDocumentOperations operations, CancellationToken token) {
        Seq<IEvent<GraphEvent>> events = toSeq(range.Events.OfType<IEvent<GraphEvent>>());
        IO<Unit> landing = (from reserved in dots.Reserve(events.Count)
                            from entries in OpLog.Project(reserved, events)
                            select entries).ToFin().Match(Succ: drain, Fail: IO.fail<Unit>);
        await landing.RunAsync(EnvIO.New(token: token)).ConfigureAwait(false);
        return NullChangeListener.Instance;
    }
}

// --- [OPERATIONS] ------------------------------------------------------------------------
// Every divergence is one attribute row and zero assignment statements survive. The positional ctor is PINNED
// because the msgpack roster is the canonical thirteen-slot order three runtimes decode positionally.
[Mapper]
[MapperRequiredMapping(RequiredMappingStrategy.Both)]
public static partial class OpLogMapper {
    [MapProperty(nameof(EventFacts.Dot), nameof(OpLogEntry.Id))]
    [MapValue(nameof(OpLogEntry.Family), Use = nameof(StructuralLane))]
    [MapProperty(nameof(EventFacts.Logical), nameof(OpLogEntry.Logical))]
    public static partial OpLogEntry Entry(EventFacts facts);

    private static ColumnFamily StructuralLane() => ColumnFamily.Geometry;
    [UserMapping] private static ulong Slot(OpSlot slot) => slot.Value;
}

public static class OpLog {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.presence.beat"), StoreSlot.Create("store.replication.checkout"));

    // Dots zip positionally so no entry borrows a neighbour's identity; admissions ACCUMULATE across the range.
    public static Validation<Error, Seq<OpLogEntry>> Project(Seq<OperationId> dots, Seq<IEvent<GraphEvent>> events) =>
        events.Zip(dots).Traverse(static pair => Project(pair.Item2, pair.Item1)).As();

    // Every body is a `Element/graph#STREAM_GRAIN` `GraphDelta`, so `Project` produces STRUCTURAL `geometry`-lane
    // entries alone and never a `crdt` op. That lane encodes `JsonStj`, NOT `MessagePackBinary`: the seam delta is
    // source-gen-registered on the STJ context and carries no `[MessagePackObject]` for a resolver to find.
    public static Validation<Error, OpLogEntry> Project(OperationId id, IEvent<GraphEvent> e) =>
        OpSlot.Of(e.Version).Map(logical => {
            ReadOnlyMemory<byte> payload = ColumnFamily.Geometry.Codec.Serialize(typeof(GraphDelta), e.Data.Body);
            return OpLogMapper.Entry(new EventFacts(
                e.Sequence, id, ModelId.Create(e.StreamId), EntityKey(e),
                e.Data.Lifecycle == EventLifecycle.Retired ? SyncOpKind.Delete : SyncOpKind.Upsert,
                payload, ContentHash.Of(payload.Span), TraceSlot.FromHex(e.CorrelationId),
                Closure(e.Data.Body), HeaderValue(e, "actor"),
                Instant.FromDateTimeOffset(e.Timestamp), logical));
        });

    // Nullable substrate columns admit here and nowhere else.
    static string EntityKey(IEvent<GraphEvent> e) => e.StreamKey ?? e.StreamId.ToString();

    static string HeaderValue(IEvent<GraphEvent> e, string key) =>
        e.Headers is { } h && h.TryGetValue(key, out object? value) ? value?.ToString() ?? string.Empty : string.Empty;

    // This SEAM mints the envelope over the SAME delta the `Payload` encodes, so `subject` IS the identity this feed
    // and the broker lane agree on; `id` takes a reserved DOT, because two peers appending the identical delta share
    // one `subject` and carry two operations. `Address` streams — no byte buffer materializes.
    public static Fin<Seq<CloudEvent>> Appended(
        Seq<GraphDelta> deltas, Header basis, DotSource dots, Func<ActivityContext, TraceCarrier> carrier, ProjectionContext frame) =>
        from reserved in dots.Reserve(deltas.Count).ToFin()
        from envelopes in deltas.Zip(reserved).Traverse(pair => GraphCrossing.Mint(
                GraphEventType.Delta,
                pair.Item1.Address(basis.Tolerance),
                frame.Now(),
                ElementWire.Encode(pair.Item1, basis),
                new CrossingPorts(pair.Item2.Wire, None, TraceSlot.Capture().Continue().Map(carrier).IfNone(default(TraceCarrier)), Seq<(EventExtension, object)>()),
                Op.Of())).As()
        // Batch dedup keys on `subject` ALONE — a re-published crossing of identical content IS one announcement —
        // and keeps the first, because the changefeed already arrives sequence-ordered.
        select toSeq(envelopes.AsEnumerable().DistinctBy(static envelope => envelope.Subject));

    // Composition binds `feed` to the durable row source; every consumer dials a `ReplayWindow` value.
    public static Seq<OpLogEntry> Replay(Seq<OpLogEntry> feed, ReplayWindow window) =>
        toSeq(feed.Filter(window.Admits).OrderBy(static entry => entry.Sequence).Take(window.Take));

    // Closure names the geometry a peer must hold to materialize the change: a non-`Object` node contributes nothing,
    // so the closure is the BLOB set, never the node set.
    static Seq<UInt128> Closure(GraphDelta delta) =>
        toSeq((delta.AddedNodes + delta.RevisedNodes.Map(static r => r.After))
            .Choose(static n => Optional(n as Node.Object))
            .SelectMany(static o => o.Representations.ByIdentifier.Values)
            .Distinct());

    // Authoring mints a NON-structural lane entry: one HLC stamp, one dot, one captured trace, the `build`
    // continuation closing over its owner's lane and payload. A caller minting its own dot invents a causal position.
    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, DotSource dots, ProjectionContext frame, Func<(OperationId Id, Instant Physical, ulong Logical, TraceSlot Trace), OpLogEntry> build) =>
        dots.Reserve(1).ToFin().Match(
            Succ: reserved => IO.lift(() => (Wall: frame.Now(), Trace: TraceSlot.Capture()))
                .Map(captured => (Cell: sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, captured.Wall)), Id: reserved[0], captured.Trace))
                .Map(stamped => build((stamped.Id, stamped.Cell.Physical, stamped.Cell.Logical, stamped.Trace))),
            Fail: IO.fail<OpLogEntry>);

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
|  [06]   | HLC cell            | event `Timestamp` + `Version`                | physical Unix-tick first, logical second, both LE (kernel law)      |
|  [07]   | sign boundary       | `OpSlot` — the one `long`↔`ulong` admission  | `Signed` total by the cap; zero unchecked casts on the page         |
|  [08]   | origin tie-break    | `Id.Origin` — the dot's own store id         | LWW `(Hlc, OriginStoreId)` deterministic; never a zero              |
|  [09]   | trace slot          | top-level `TraceSlot` field                  | never inside `Payload`; distinct from AppHost `TraceContext` fold   |
|  [10]   | crossing envelope   | seam `GraphCrossing.Mint`                    | `subject` is the content key; `id` the reserved dot                 |
|  [11]   | operation identity  | `OperationId` dot over its pre-mint frontier | equal payloads stay distinct; `Order` needs no feed walk            |
|  [12]   | payload identity    | `ContentKey` — `ContentHash.Of` the payload  | transfer set difference and blob dedup; never the entry key         |
|  [13]   | drain position      | `Sequence` — the store-local Marten cursor   | resumable read alone; two stores mint one sequence value            |
|  [14]   | dot custody         | one `DotSource` atom per store               | projection and authoring reserve together; gap-free by law          |
|  [15]   | entry transcription | `[Mapper]` over admitted `EventFacts`        | pinned positional ctor; the thirteen-slot roster cannot re-order    |

## [03]-[MERGE_LAW]

- Owner: `ConflictReceipt` with option-carried `ConflictSide` evidence; `ConflictVerdict`; `ConflictResult`; `SyncApplyReceipt` the `IValidityEvidence` conservation receipt; `SyncFault` the closed `[Union]` deriving from the KERNEL `Rasm.Domain.Fault` in the 825x band; `SyncSession` the one session capsule carrying the injected `ProjectionContext` frame with its delegate rows; `SyncMerge` the fold surface routing each entry by its `ColumnFamily.Stance` — `Lww`/`FirstWriter` through `Adjudicate`, `Crdt` into `Crdt.Apply`, a winning whole-relation entry through the `Truncate` delegate.
- Cases: four verdict rows — `LocalWin | RemoteWin | Merged | Rejected` — collapse into one `ConflictResult(Verdict, Receipt, Conflicted, Held)` where `Conflicted` separates a genuine divergence from an idempotent-replay `LocalWin` or a fresh `Merged`, and `Held` carries the held content key the fork fault reads without a second lookup. `Rejected` is reachable only on an equal `(stamp, origin)` over divergent content — the causal fork `Apply` lifts to `SyncFault.Forked` and halts on — never a soft conflict bucket. Faults close at `SchemaMismatch | ReplicationFaulted | SpeckleMarshal | TransferDecode | Unconserved | Forked | Unobserved | SlotOutOfSpace`, the last two carrying a compaction whose minter never observed its horizon and a vector slot outside the dot space.
- Entry: `SyncMerge.Apply(session, incoming)` carries commit effects; replay skips on IDENTITY against the fold's advancing frontier, and the receipt proves applied, skipped, conflicted, converged, and pushed counts under its own `IValidityEvidence` conservation fold, an auto-resolved LWW divergence counting as `Conflicted` and recording its receipt whether the winner committed or the local kept, with a non-closing batch failing `SyncFault.Unconserved`; `Admissible(id, op)` is the causal gate the `Converge` binding runs over its decoded op, refusing a `Maintain` whose quiescence vector its own context fails to dominate.
- Receipt: `ConflictReceipt` is the typed fork evidence the `Forked` halt carries and the inspector projects; `SyncApplyReceipt` is the per-run apply evidence, self-attesting through the kernel `ValidityClaim.All` fold over its own carried `Batch` — the parameterized `Conserves(long)` knob and a hand `&&` chain are the deleted forms, since the receipt reconstructs the check from its own fields.
- Packages: Rasm (`Rasm.Domain` `Fault` — the federation fault base; `IValidityEvidence`/`ValidityClaim` — the receipt-validity floor; `FaultBand`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one `MergeStance` row carrying its `OpLaw`; a fifth `ConflictVerdict` row is the named defect; a new fault cause is one `SyncFault` case; a new replicated data type is a `Version/commits#CRDT_ALGEBRA` `CrdtField` case with its `Crdt.Law` row, dispatched by this fold, never a fifth scalar arm.
- Boundary: replay dedup is IDENTITY-proven and never content-proven — `entry.Id.Applied(frontier)` is the whole test, so two operations carrying identical bytes both land and a redelivery lands once, where a content-equality test reports the second real edit as a replay and skips it. Each landed entry joins its own `Frontier` inside the fold, so a batch carrying its own duplicate settles without a second lookup. Commutation is per mutation kind and reads ONE vocabulary: the lane answers `Stance.Law` and the crdt op answers `Crdt.Law`, both `OpLaw` rows, so an `Ordered` arm losing its total order counts `Conflicted` while an absorbing arm counts `Skipped` — a distinction one `Convergent` boolean cannot carry, and the reason the whole crdt lane once counted `Converged` over its genuine `set` conflicts.
- Boundary: LWW per column family is the default. `Held` resolves the competing local entry per model and family; content-key equality adjudicates `LocalWin` as an idempotent replay; an ABSENT competitor adjudicates `Merged` through `Fresh`, whose held half is `None` rather than a zero-stamp sentinel — the HLC's own physical half is a Unix-tick `long` whose minimum lies outside the wire domain, so absence rides the option and never a value any writer may carry. Any HLC-resolved win over differing content is a genuine divergence the fold counts and receipts, and an equal `(stamp, origin)` over divergent content is the fork `Apply` halts on. Lane `FirstWriter` (`Presence`) is EARLIEST-wins, the INVERSE direction of the LWW default, so the tuple switch flips both arms for it rather than silently keeping a later row over the genuine first writer.
- Boundary: `Maintain` is the one arm whose admissibility outlives its fold — compaction commutes and absorbs as a filter, yet it is a MEET where every sibling is a JOIN, so a horizon its minter never observed reclaims a tombstone a concurrent insert still needs. Only the entry's own `OperationId.Context` evidences that check, so the gate lives at the entry: `Crdt.Apply`, holding no frontier, structurally cannot run it. Family `crdt` routes its `Payload` through `Crdt.Apply` so a concurrent edit converges by the join-semilattice least-upper-bound rather than scalar LWW — the multi-writer offline and IFC three-way merge substrate. Winning whole-relation entries commit through the session `Truncate` delegate, their capability row selecting the relation-wide lane rather than a dead flag.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------
// Conflict-side evidence rides an OPTION: `Hlc.Zero`'s physical half is `Instant.MinValue`'s ticks, NEGATIVE and
// outside the I63 domain the packed slot admits, so absence spelled as a stamp is a value no writer may carry.
public readonly record struct ConflictSide(Hlc Stamp, string Actor);

public readonly record struct ConflictReceipt(
    ModelId Model, string EntityKey, ColumnFamily Family, Option<ConflictSide> Held,
    Option<ConflictSide> Incoming, CorrelationId Correlation, Instant At);

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

// Both an idempotent-replay `LocalWin` and a competitor-free `Merged` carry `Conflicted: false`, keeping the audit
// exact.
public readonly record struct ConflictResult(ConflictVerdict Verdict, ConflictReceipt Receipt, bool Conflicted, UInt128 Held);

// --- [ERRORS] ---------------------------------------------------------------------------
// `SyncFault` derives directly from `Rasm.Domain.Fault`; generated case identity proves each offset in-band.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SyncFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Sync;
    private SyncFault() { }
    [FaultCase(0)]
    public sealed partial record SchemaMismatch(ulong Local, ulong Remote) : SyncFault();
    [FaultCase(1)]
    public sealed partial record ReplicationFaulted(string Slot, Error Cause) : SyncFault(), ICausedFault;
    [FaultCase(2)]
    public sealed partial record SpeckleMarshal(string Peer, string Class) : SyncFault();
    [FaultCase(3)]
    public sealed partial record TransferDecode(string Peer, Error Cause) : SyncFault(), ICausedFault;
    [FaultCase(4)]
    public sealed partial record Unconserved(long Batch, long Settled) : SyncFault();
    [FaultCase(5)]
    public sealed partial record Forked(ConflictReceipt Receipt, UInt128 Held, UInt128 Incoming) : SyncFault();
    [FaultCase(6)]
    public sealed partial record Unobserved(string Field, Guid Origin) : SyncFault();
    [FaultCase(7)]
    public sealed partial record SlotOutOfSpace(long Slot) : SyncFault();

    public override string Message => Switch(
        schemaMismatch:     static c => $"{c.Local}:{c.Remote}",
        replicationFaulted: static c => $"{c.Slot}:{c.Cause.Message}",
        speckleMarshal:     static c => $"{c.Peer}:{c.Class}",
        transferDecode:     static c => $"{c.Peer}:{c.Cause.Message}",
        unconserved:        static c => $"{c.Batch}!={c.Settled}",
        forked:             static c => $"{c.Receipt.EntityKey}@{c.Receipt.Incoming.Map(static side => side.Stamp.Physical.ToString()).IfNone("<unstamped>")}:{c.Held}!={c.Incoming}",
        unobserved:         static c => $"{c.Field}@{c.Origin:N}",
        slotOutOfSpace:     static c => $"<vector-slot:{c.Slot}>");
}

// --- [SERVICES] --------------------------------------------------------------------------
// Self-attesting through the kernel receipt-validity floor: a consumer re-proves conservation without the caller's
// count.
public readonly record struct SyncApplyReceipt(
    long Batch, long Applied, long Skipped, long Conflicted, long Converged, long Pushed, long QueueDepth,
    Seq<ConflictReceipt> Conflicts, SyncCursor Cursor, SyncCursor Acked, CorrelationId Correlation, Instant At) : IValidityEvidence {
    public long Settled => Applied + Skipped + Conflicted + Converged;
    public bool IsValid => ValidityClaim.All(Batch == Settled, Conflicts.Count == Conflicted);
}

// Sessions take the injected `Element/graph#STORE_RAIL` frame, carrying clock, correlation, and tenant as VALUES.
// `Cursor`/`Acked` are TWO cursor spaces — our position in the PEER's feed, and the peer's confirmed position in OURS
// — and one slot carrying both skips or re-pulls remote entries. `Spool`/`Unspool`/`LocalHas` bind the durable KV
// floor, because a session whose pending rows live only in memory loses the buffer on process exit.
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

// Named once, so the fold's three halves state one type instead of a six-slot tuple at every arrow.
internal readonly record struct Counts(VersionVector Frontier, long Applied, long Skipped, long Conflicted, long Converged, Seq<ConflictReceipt> Conflicts);

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SyncMerge {
    public static ConflictReceipt Receipt(SyncSession session, OpLogEntry held, OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, Some(new ConflictSide(held.Stamp, held.Actor)),
            Some(new ConflictSide(incoming.Stamp, incoming.Actor)), session.Frame.Correlation, session.Frame.Now());

    static ConflictReceipt Fresh(SyncSession session, OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, None,
            Some(new ConflictSide(incoming.Stamp, incoming.Actor)), session.Frame.Correlation, session.Frame.Now());

    public static ConflictResult Adjudicate(SyncSession session, OpLogEntry incoming) =>
        session.Held(incoming) is { IsSome: true, Case: OpLogEntry held }
            ? incoming.ContentKey == held.ContentKey
                ? new ConflictResult(ConflictVerdict.LocalWin, Receipt(session, held, incoming), Conflicted: false, held.ContentKey)
                : new ConflictResult(Resolve(incoming, held), Receipt(session, held, incoming), Conflicted: true, held.ContentKey)
            : new ConflictResult(ConflictVerdict.Merged, Fresh(session, incoming), Conflicted: false, UInt128.Zero);

    // FirstWriter is earliest-wins, the INVERSE of the LWW default, so the direction flips: the OLDER stamp wins
    // regardless of arrival. An equal `(stamp, origin)` over divergent content is the fork either stance halts on.
    static ConflictVerdict Resolve(OpLogEntry incoming, OpLogEntry held) =>
        ((incoming.Stamp, incoming.OriginStoreId).CompareTo((held.Stamp, held.OriginStoreId)),
         incoming.Family.Stance == MergeStance.FirstWriter) switch {
            (> 0, true) or (< 0, false) => ConflictVerdict.LocalWin,
            (< 0, true) or (> 0, false) => ConflictVerdict.RemoteWin,
            _ => ConflictVerdict.Rejected,
        };

    // Compaction refuses where an entry's causal context fails to dominate the quiescence vector claims a horizon
    // nobody saw, and applying it resurrects a deleted element on whichever peer folds it.
    public static Fin<Unit> Admissible(OperationId id, CrdtOp op) =>
        op is CrdtOp.Maintain compaction && !id.Context.Dominates(compaction.Quiescent)
            ? Fin.Fail<Unit>(new SyncFault.Unobserved(compaction.Field, id.Origin))
            : Fin.Succ(unit);

    // Dots under the applied frontier settle `Skipped`, identity-proven: content equality cannot separate a
    // redelivery from a second real edit carrying identical bytes. Every surviving entry lands through ONE
    // `ConflictResult` and in EXACTLY ONE counter, which is what makes the conservation fold exact.
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

    // Only a non-convergent applying verdict reaches `Commit`/`Truncate`: `Converge` already landed its own merge.
    static IO<Counts> Landed(SyncSession session, OpLogEntry entry, ConflictResult result, Counts counts) =>
        result.Verdict == ConflictVerdict.Rejected
            ? IO.fail<Counts>(new SyncFault.Forked(result.Receipt, result.Held, entry.ContentKey))
            : (result.Verdict.Applies && !entry.Family.Stance.Convergent
                    ? entry.Kind.Ops.Admits(SyncCapability.WholeRelation) ? session.Truncate(entry) : session.Commit(entry)
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
|  [05]   | whole-relation truncate | `Kind.Ops` holds `WholeRelation` → `Truncate`           | clears `(Model, Family)`; `Held` answers the head  |
|  [06]   | replay dedup            | `Id.Applied(frontier)` — identity, never content        | equal payloads both land; a redelivery lands once  |
|  [07]   | commutation source      | lane `Stance.Law`; crdt arm `Crdt.Law`                  | one `OpLaw` triple, three runtime transcriptions   |
|  [08]   | compaction admission    | `Id.Context.Dominates(Maintain.Quiescent)`              | else `SyncFault.Unobserved`; the fold cannot check |
|  [09]   | absent conflict side    | corresponding `ConflictReceipt` side is `None`          | never a zero stamp whose ticks leave the domain    |

## [04]-[SYNC_TRANSPORTS]

- Owner: `FlowCapability`/`SyncFlows` the exchange-direction vocabulary and `SyncFlow` its keyless disposition; `SyncTransport` the closed transport family; the `SyncPump` dispatch surface with the `SubtreeFetch` graph-checkout bridge and the `Offer` Speckle-diff arm; `GraphDiff` the named set-difference algebra both dial.
- Cases: three transport cases — `HttpDelta`, `SpeckleLikeDiff`, `SubtreeCheckout` — widened by the one `SyncFlow` field whose capability set the `Exchange` fold reads; fan-in, fan-out, and bidirectional are `SyncFlow` rows, never new transport cases, and the empty corner is barred because a transport that neither pulls nor pushes exchanges nothing.
- Entry: `SyncPump.Run(session, transport)` is one total state-threaded dispatch; `GraphDiff(root, holds)` projects the missing geometry-BLOB-key manifest — the closure with the root payload key, minus held; `SubtreeFetch(source, target, root)` fetches the root entry, applies it onto the target, and accounts the blob manifest on the receipt.
- Auto: intra-cluster replication is Marten's own daemon over the shared PostgreSQL, so this axis is the CROSS-store and offline lane — a disconnected editor, a Speckle hub, a peer holding a subgraph — never a re-implementation of single-cluster replication. `HttpDelta` pulls a cursor-bounded segment of the peer's feed and pushes the pending set past the peer-acked frontier; `SubtreeCheckout` fetches the root entry, APPLIES it, and accounts its closure as the blob-transfer set; `SpeckleLikeDiff` folds the pending set through `GraphDiff` over the peer's membership answer and hands the missing set to the marshal.
- Receipt: every transport run yields one `SyncApplyReceipt`; the subtree-checkout transfer count rides the same receipt.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (`CapabilitySet`/`CapabilityLaw`), NodaTime, Speckle.Sdk (companion, outside-Rhino), BCL inbox.
- Growth: a new transport is one case with one dispatch arm; a new exchange direction is one `SyncFlows` corner; a new graph-checkout shape is one entry over `GraphDiff`, never a second diff algebra.
- Boundary: `HttpDelta` rides the AppHost `OutboundHop` keyed pipeline, so retry, backoff, and hop deadlines are owned there and the database stays excluded from the hop law. `GraphDiff` is the ONE set-difference algebra — the closure GEOMETRY-blob manifest with the root payload key, minus what the target holds — and it is the BLOB-transfer set the content-addressed store moves, NOT an op-log-entry fetch input; feeding that manifest to `Fetch`, which resolves an entry and never a geometry blob, or running a second walk-and-diff, is the deleted form. `Rasm.Compute/Runtime/wire#PROTO_VOCABULARY`, so the remote rpc dials it rather than re-implementing the difference.
- Boundary: the two cursor SPACES thread the exchange — the pull leg advances this store's position in the PEER's feed and the push leg the peer-returned confirmation in OURS — and overwriting one with the other resumes the next pull from this store's push frontier inside the PEER's sequence space, silently skipping every entry between. Speckle's wire leg lives OUTSIDE-RHINO on the companion target, so the in-Rhino assembly composes only the case and the marshal delegate slot and never references the SDK; the DI-resolved INSTANCE `IOperations.Send` returns a root id that projects onto the offered root `ContentKey` with zero second identity, and a drift between the two faults the run.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
// Barring the EMPTY corner is the law here: a transport holding neither capability exchanges nothing, which two bools
// could spell.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlowCapability : ICapability<FlowCapability> {
    public static readonly FlowCapability Pull = new("pull");
    public static readonly FlowCapability Push = new("push");
}

public static class SyncFlows {
    public static readonly CapabilitySet<FlowCapability> Inbound = CapabilitySet<FlowCapability>.Of(FlowCapability.Pull);
    public static readonly CapabilitySet<FlowCapability> Outbound = CapabilitySet<FlowCapability>.Of(FlowCapability.Push);
    public static readonly CapabilitySet<FlowCapability> Duplex = CapabilitySet<FlowCapability>.All;
    public static readonly CapabilityLaw<FlowCapability> Law = new(Seq(Inbound, Outbound, Duplex));
}

[SmartEnum]
public sealed partial class SyncFlow {
    public static readonly SyncFlow FanIn = new(SyncFlows.Inbound);
    public static readonly SyncFlow FanOut = new(SyncFlows.Outbound);
    public static readonly SyncFlow Bidirectional = new(SyncFlows.Duplex);
    public CapabilitySet<FlowCapability> Legs { get; }
    public bool Pulls => Legs.Admits(FlowCapability.Pull);
    public bool Pushes => Legs.Admits(FlowCapability.Push);

    public static readonly Fin<Unit> Lawful =
        toSeq(Items).Traverse(static row => SyncFlows.Law.Admit(row.Legs)).As().Map(static _ => unit);
}

// --- [MODELS] ----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SyncTransport {
    private SyncTransport(SyncFlow flow) => Flow = flow;
    public SyncFlow Flow { get; }
    public sealed record HttpDelta(string Peer, SyncFlow Flow) : SyncTransport(Flow);
    public sealed record SpeckleLikeDiff(string Peer, SyncFlow Flow) : SyncTransport(Flow);
    public sealed record SubtreeCheckout(string Peer, UInt128 Root, SyncFlow Flow) : SyncTransport(Flow);
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SyncPump {
    public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport) =>
        transport.Switch(
            state: session,
            httpDelta: static (s, row) => Exchange(s, row),
            speckleLikeDiff: static (s, row) => Offer(s, row),
            // Deltas ARE the change, so the root entry APPLIES; its closure blobs ride the content-keyed store, and
            // feeding that manifest to `Fetch` — the ENTRY resolver — would resolve nothing.
            subtreeCheckout: static (s, row) => s.Fetch(row.Root).Bind(entry =>
                SyncMerge.Apply(s, Seq(entry)).Map(receipt => receipt with { Pushed = GraphDiff(entry, s.Holds).Count })));

    public static Seq<UInt128> GraphDiff(OpLogEntry root, Func<UInt128, bool> holds) => OpLog.TransferSet(root, holds);

    public static IO<SyncApplyReceipt> SubtreeFetch(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry =>
            SyncMerge.Apply(target, Seq(entry)).Map(receipt => receipt with { Pushed = GraphDiff(entry, target.Holds).Count }));

    static IO<SyncApplyReceipt> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        from pulled in row.Flow.Pulls
            ? s.Pull(row.Peer, s.Cursor).Bind(segment => segment.SchemaFingerprint == s.SchemaFingerprint
                ? SyncMerge.Apply(s, segment.Entries).Map(receipt => receipt with { Cursor = segment.Cursor })
                : IO.fail<SyncApplyReceipt>(new SyncFault.SchemaMismatch(s.SchemaFingerprint, segment.SchemaFingerprint)))
            : IO.pure(Idle(s))
        let pending = s.Pending(s.Acked)
        from receipt in row.Flow.Pushes
            ? s.Push(row.Peer, pending).Map(acked => pulled with { Pushed = pending.Count, Acked = acked })
            : IO.pure(pulled)
        select receipt;

    // `Acked` advances to the LAST offered entry's real coordinates, never a `Sequence + count` advance.
    static IO<SyncApplyReceipt> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        from pending in IO.pure(s.Pending(s.Acked))
        from held in s.HasObjects(row.Peer, toSeq(pending.Fold(Seq<UInt128>(), static (set, entry) => set + GraphDiff(entry, static _ => false)).Distinct()))
        let missing = pending.Filter(entry => !held.Contains(entry.ContentKey))
        from sent in s.SpeckleSend(row.Peer, missing)
        from receipt in missing.Head.Map(h => h.ContentKey) is { IsSome: true, Case: UInt128 root } && root != sent.RootContentKey
            ? IO.fail<SyncApplyReceipt>(new SyncFault.SpeckleMarshal(row.Peer, $"root-key-drift:{root}!={sent.RootContentKey}:refs={sent.ConvertedReferences}"))
            : IO.pure(Idle(s) with {
                Pushed = missing.Count,
                Acked = pending.Last.Map(last => s.Acked with { Sequence = last.Sequence, Physical = last.Physical, Logical = last.Logical }).IfNone(s.Acked),
            })
        select receipt;

    static SyncApplyReceipt Idle(SyncSession s) =>
        new(0L, 0L, 0L, 0L, 0L, 0L, s.QueueDepth(), Seq<ConflictReceipt>(), s.Cursor, s.Acked, s.Frame.Correlation, s.Frame.Now());
}
```

| [INDEX] | [POLICY]                  | [VALUE]                                            | [BINDING]                                               |
| :-----: | :------------------------ | :------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | intra-cluster replication | Marten daemon `HotCold`                            | this axis is the cross-store/offline lane only          |
|  [02]   | graph checkout            | fetch+apply root; `GraphDiff` is the BLOB manifest | closure set via the blob store, not an op-log `Fetch`   |
|  [03]   | Speckle marshal           | DI-resolved instance `IOperations`                 | outside-Rhino; `rootObjId` → `ContentKey`; drift faults |
|  [04]   | http delta                | AppHost `OutboundHop` pipeline                     | database excluded from the hop law                      |
|  [05]   | exchange direction        | `CapabilitySet<FlowCapability>` under its law      | the empty corner is barred, never merely unwritten      |

Per-transport descriptor: the sentence a row is chosen on, its guarantee, and what settles a leg. `admit` is `SyncPump.Run` for all three and `tenancy` is the peer identity the session carries, so both stay uniform and only the differing coordinates earn columns.

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

- Owner: `PresenceRow` the ephemeral collaboration row on the changefeed shape; `AwarenessBeat`/`AwarenessKind` the dedicated low-latency lossy signal carrying cursor, selection, camera-frustum, focus, and follow beats off the durable changefeed; `WorkingSet`/`ReplicationQuery` the partial-replication subgraph checkout; `Awareness` the ONE static surface owning the lossy beat lane, the durable presence-row mint and sweep, and the working-set checkout — one deep surface over the two presence forms with the checkout, never three shallow services.
- Entry: `AwarenessLane(spec, dropped)` opens the declared `DropOldest` row through the AppHost `DrainSurface`, so the lane carries its own drop receipt and never hand-rolls channel options, and a `DropOldest` row opening without that receipt fails on the `Fin` rail; `Beat(actor, kind, payload, seq, frame, session)` is the one polymorphic awareness constructor, the kind discriminating payload meaning so per-signal factories are the deleted form; `Present(actor, state, ttl, frame)` mints the durable ephemeral presence row and `Live(rows, now)` is the per-actor add-wins-LWW sweep; `Checkout(query, resolve, fetch, cursor, frame)` materializes a subgraph working set.
- Auto: presence rows expire at stamp offset by `Ttl` and sweep on the heartbeat cadence. Awareness beats ride a SEPARATE lossy lane — cursor moves, selection halos, and camera frusta beat at high cadence and never touch the durable store, while `AwarenessKind` discriminates them and `Supersedes` lets a slow reader discard a reordered beat by per-actor lamport. Dropped beats receipt through the drain surface's own callback into the loss atom. Working-set checkout resolves a query into a content-key set then fetches only those entries, so a peer materializes one subgraph rather than the whole graph.
- Receipt: a presence beat rides `store.presence.beat`; an awareness drop rides the drain surface's drop receipt into the loss atom; a working-set checkout rides `store.replication.checkout` carrying the subgraph size.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, System.Threading.Channels, BCL inbox.
- Growth: a new awareness signal is one `AwarenessKind` row; a new checkout dimension is one field on `ReplicationQuery`; zero new surface — a per-signal awareness factory, a presence row written to the DURABLE event stream, or a second lossy lane is the deleted form.
- Boundary: presence is one ephemeral `Presence`-lane row (`durable: false`, `FirstWriter` stance) that `Present` mints and `Live` sweeps, never a durable event-stream write and never a transport. Awareness rides the fire-and-forget channel that never appends a durable entry, while the converging `Version/commits#CRDT_ALGEBRA` `EphemeralMap` is the durable self-expiring map a late-joining peer reconstructs — two distinct presence forms the one `Awareness` surface owns together, so the durable projection's liveness horizon agrees with the convergent map's. Working-set checkout subscribes its op-stream to changes touching its checked-out keys alone.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AwarenessKind {
    public static readonly AwarenessKind Cursor = new("cursor");
    public static readonly AwarenessKind Selection = new("selection");
    public static readonly AwarenessKind Camera = new("camera");
    public static readonly AwarenessKind Focus = new("focus");
    public static readonly AwarenessKind Follow = new("follow");
}

// --- [MODELS] ----------------------------------------------------------------------------
public readonly record struct AwarenessBeat(string Actor, AwarenessKind Kind, ReadOnlyMemory<byte> Payload, ulong Seq, Instant At, Option<string> Session) {
    public bool Supersedes(AwarenessBeat prior) => Actor == prior.Actor && Kind == prior.Kind && Seq > prior.Seq;
}

public readonly record struct PresenceRow(string Actor, ReadOnlyMemory<byte> State, Instant At, Duration Ttl) {
    public bool Live(Instant now) => now - At < Ttl;
}

public readonly record struct ReplicationQuery(Option<string> Region, Option<string> Layer, Option<string> View, Option<string> Kind, int ClosureDepth);

public readonly record struct WorkingSet(Seq<UInt128> Keys, Seq<OpLogEntry> Entries, SyncCursor Cursor, Instant At);

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class Awareness {
    public static Fin<DrainQueue<AwarenessBeat>> AwarenessLane(DrainSpec spec, Atom<Seq<AwarenessBeat>> dropped) =>
        spec.Open<AwarenessBeat>(Some<Action<AwarenessBeat>>(beat => dropped.Swap(seq => seq.Add(beat))));

    public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ProjectionContext frame, Option<string> session = default) =>
        new(actor, kind, payload, seq, frame.Now(), session);

    // Presence seats one durable lane row, distinct from the lossy beat that never appends.
    public static PresenceRow Present(string actor, ReadOnlyMemory<byte> state, Duration ttl, ProjectionContext frame) =>
        new(actor, state, frame.Now(), ttl);

    // One row per actor (the add-wins-LWW the convergent `EphemeralMap` resolves to), every lapsed `Ttl` dropped,
    // so the durable projection agrees with that map's physical-time liveness horizon.
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
