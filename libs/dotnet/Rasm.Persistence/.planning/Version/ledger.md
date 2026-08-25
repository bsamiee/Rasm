# [PERSISTENCE_VERSION_LEDGER]

`Rasm.Persistence.Version` projects Marten events into one durable `OpLogEntry` feed and owns every convergence decision over that feed. `ColumnFamily` binds payload codec and merge stance; `ReplayWindow` parameterizes every bounded read; `SyncMerge` folds scalar, first-writer, and CRDT entries through one closed fault rail; `SyncTransport` carries cross-store exchange over the generated `rasm.contracts.sync.SyncService` both processes bind; `Awareness` owns ephemeral collaboration. `ProjectionContext` supplies time, correlation, and tenant evidence, while `ContentHash.Of` supplies payload identity.

## [01]-[INDEX]

- [02]-[CHANGEFEED]: `OpLogEntry` projects Marten events, with the `OpSlot` sign boundary, HLC stamping, the trace slot, the closure manifest, and the `ReplayWindow` windowed read.
- [03]-[MERGE_LAW]: LWW adjudication, conflict receipts, the idempotent apply fold, CRDT dispatch, and the conservation invariant.
- [04]-[SYNC_TRANSPORTS]: `SyncTransport` closes the transport family over its `SyncFlow` capability set, `SyncWire` and `SyncEndpoint` bind the two ends of the generated `SyncService`, and the Speckle marshal keeps its SDK seam.
- [05]-[PRESENCE]: ephemeral presence rows, the lossy awareness lane, and the working-set checkout.

## [02]-[CHANGEFEED]

- Owner: `SyncCapability` the write-verb capability vocabulary and `SyncOps` its legal corners; `SyncOpKind` the write-verb axis carrying its capability set and its `Fact` announcement spelling; `OpCapability`/`OpLaws` and `OpLaw` the estate's one commutation vocabulary; `ColumnFamily` the merge-lane axis carrying `MergeStance` AND `SnapshotCodec` as one row, so the lane selects adjudication algebra and payload codec together; `TraceSlot` the changefeed W3C trace-id carrier; `OpSlot` the ONE sign boundary between the `long` version-vector slot space and the `ulong` dot space; `OperationId` the dot-plus-frontier operation identity; `DotSource` the store's one dot minter; `EventFacts` the admitted Marten-event evidence; `OpLogMapper` the generated `EventFacts`→`OpLogEntry` transcription; `OpLogEntry` the interior changefeed record; `OpLogEntryWire` and `OpLogWire` the explicit primitive thirteen-slot MessagePack boundary; `ReplayWindow` the one windowed-read parameterization; `ChangefeedSubscription` the `SubscriptionBase` batched drain; the `OpLog` project-stamp-replay surface.
- Cases: `SyncOpKind` is `Upsert | Delete | Truncate | Presence`, each carrying its past-tense `Fact` and its held `SyncCapability` set — `{}`, `{Tombstone}`, `{Tombstone, WholeRelation}` are the three legal corners, so a whole-relation verb that is not a tombstone is unrepresentable rather than merely unwritten. `OpLaw` is `Ordered | Commutative | Semilattice`, the same triple `Version/commits#CRDT_ALGEBRA` `Crdt.Law` returns and `typescript:core/state/merge` `Merge.Law` spells, each row holding its `OpCapability` corner. `MergeStance` is `Lww(Ordered) | Crdt(Semilattice) | FirstWriter(Ordered)` and derives `Convergent` off the law. `ColumnFamily` closes at `Scalar | Crdt | Geometry | Presence | Commit | Branch | Attest`, each carrying stance, codec, and durability, so a consumer dispatches on the row and never a string compare.
- Entry: `ProcessEventsAsync` reserves a range's dots once, projects the whole delivered range, and drains it once. `DotSource.Reserve(count)` is the one identity mint for projection and authoring. `OpLog.Project(dots, events)` accumulates admissions; `Replay(feed, window)` owns windowed reads; `Stamp(sink, dots, frame, build)` owns authoring; `OpLogWire.Encode` and `OpLogWire.Decode` own the native thirteen-slot MessagePack boundary in singular and frame-accumulating plural form, and `OpLogWire.Uuid`/`OpLogWire.I63` own the store-id byte form and the `ulong`→`long` gate every sync crossing composes; `TransferSet(entry, holds)` projects closure-minus-held keys.
- Auto: a Marten async subscription projects each committed model event into the one feed; triggers, secondary op-log tables, and per-payload records are inadmissible (`H11`). Every connected `GraphEvent` producer is a structural `geometry`-lane change carrying the codec-encoded `GraphDelta`, adjudicated by `(Hlc, OriginStoreId)` LWW. `Closure` carries descendant geometry content keys, so transfer is set difference rather than a tree walk. Every foreign column of a Marten event admits ONCE into `EventFacts` — stream key, actor header, correlation, and stream version each on the `Validation` rail — so a malformed event reports every bad column at once and the generated mapper builds the entry from evidence alone.
- Receipt: changefeed position and queue depth ride `SyncApplyReceipt`; the projected-segment evidence rides `ReceiptSinkPort`.
- Packages: Marten owns event-range subscription; MessagePack owns the explicit primitive op-log envelope; Rasm.Element supplies native `GraphDelta.Address`, `Node`, and representation keys; Rasm supplies content hashing, capabilities, and faults; Mapperly supplies admitted entry transcription; NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, DiagnosticSource, and BCL inbox complete the substrate.
- Growth: a new synced concern is one `SyncOpKind` verb with its capability corner, one `ColumnFamily` lane carrying its stance and codec columns, or one payload kind keyed by the lane row's `Codec`; a new windowed-read consumer is one `ReplayWindow` parameterization; a new admitted event column is one `EventFacts` member and one `[MapProperty]` row. Zero new surface — a per-entity-kind outbox table, a bespoke op-log store beneath Marten, a per-payload-kind parallel record, a second dot minter, a second sign boundary, or a per-lane string literal in the merge fold is the deleted form.
- Boundary: `OperationId` is the entry key and `ContentKey` the payload key, and the two never merge — two peers stamping the identical `Set("name", "North Wing")` share one `ContentKey` and carry two dots, so the second edit survives where a content-keyed log drops it and reads the drop as successful dedup. `Counter` is the origin's own `VersionVector` slot rather than a second counter, and `Context` is the pre-mint frontier, so `Order` is total from two ids with no feed walk and `Applied` is the exact replay test the merge fold takes. `Sequence` survives as the store-local drain cursor ALONE — a resumable position, never an identity, because two stores mint sequence 41 and one entry cannot answer to both. `DotSource` is the store's single minter, so the gap-free dot law holds across the projection and the authoring path, and a restart re-seeds its atom from the durable head joined with the tail past the cursor.
- Boundary: `OpSlot` is the ONE sign boundary this feed carries. Version vectors publish `long` slots while every dot is `ulong`, and an unchecked `(long)Counter` past `long.MaxValue` reads NEGATIVE — a dominance test then answers "already applied" for an operation nobody applied, silently dropping it. Construction caps the carrier at `long.MaxValue`, so `Signed` is total and no cast survives anywhere on the page; the mirror of the AppHost port-side `[ValueObject<ulong>]` admission, landed store-side because no AppHost type crosses down. `Counter` still projects the identical `ulong` onto the wire and ONE eight-byte `CanonicalWriter.I64` word into the preimage — the sixteen-byte `U128` twin that once disagreed with it is gone — so the frozen thirteen-slot roster is untouched.
- Boundary: Marten's `origin` header no longer reaches the entry — `OriginStoreId` reads the dot's own `Id.Origin`, so the LWW tie-break is deterministic across peers and no missing header fabricates the `Guid.Empty` bucket that collapsed every origin into one. Marten events PROJECT the changefeed (`H11`): the connected `GraphEvent` producer is the structural `geometry`-lane `GraphDelta`, never a `crdt` op. The remaining closed lane keys reserve protocol space but claim no producer until an application binding supplies one. Persistence only READS `Activity.Current` and projects to the `TraceSlot` VALUE, never re-minting the propagator the AppHost `TraceContext` fold owns; the 16-byte trace-id admits once through the TOTAL `TraceSlot.FromHex`, so an arbitrary correlation string yields `Empty` rather than faulting the subscription daemon. `OpLogEntry` carries no correlation field: correlation rides the session frame and receipts.
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
using MessagePack;
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
// `typescript:core/state/merge` `Merge.Law`); the capability set is the .NET-side column shape.
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
    public static readonly ColumnFamily Crdt = new("crdt", MergeStance.Crdt, SnapshotCodec.ProtoBinary, durable: true);
    public static readonly ColumnFamily Geometry = new("geometry", MergeStance.Lww, SnapshotCodec.JsonStj, durable: true);
    public static readonly ColumnFamily Presence = new("presence", MergeStance.FirstWriter, SnapshotCodec.MessagePackBinary, durable: false);
    public static readonly ColumnFamily Commit = new("commit", MergeStance.Lww, SnapshotCodec.MessagePackBinary, durable: true);
    public static readonly ColumnFamily Branch = new("branch", MergeStance.Lww, SnapshotCodec.MessagePackBinary, durable: true);
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
        if (counter == OpSlot.Zero) {
            validationError = origin == Guid.Empty && context.Slots.IsEmpty
                ? null
                : new ValidationError("<operation-genesis-shape>");
            return;
        }
        validationError = OpSlot.Of(context.At(origin)).Match(
            Succ: prior => counter == prior.Next ? null : new ValidationError($"<operation-dot-gap:{counter.Value}>"),
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

    // Framed origin, ONE eight-byte counter word, then the vector through the SAME writer the commit key takes, so a
    // slot-order edit cannot land on one key and not the other. `Counter.Value` rides `I64` `unchecked` — the
    // `OpSlot` cap already holds it under `long.MaxValue`, so the reinterpretation is identity — and the parity
    // corpus pins THIS eight-byte form; a second width beside it was the live divergence this member deletes.
    public void CanonicalBytes(CanonicalWriter writer) {
        writer.String(Origin.ToString("N")).I64(unchecked((long)Counter.Value));
        Context.CanonicalBytes(writer);
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

// The cross-runtime envelope is an explicit primitive MessagePack DTO, not `OpLogEntry` handed to a contractless
// resolver. Every peer therefore sees the same thirteen positional cells: GUIDs and UInt128 values are sixteen
// network-order bytes, roster members are their string keys, and the payload remains a bin value no envelope
// decoder opens. Domain carriers never leak their generated formatter choices into this seam.
[MessagePackObject]
public sealed record VectorSlotWire(
    [property: Key(0)] byte[] Origin,
    [property: Key(1)] ulong Sequence);

[MessagePackObject]
public sealed record OperationIdWire(
    [property: Key(0)] byte[] Origin,
    [property: Key(1)] ulong Counter,
    [property: Key(2)] VectorSlotWire[] Context);

[MessagePackObject]
public sealed record TraceSlotWire(
    [property: Key(0)] byte[] TraceId,
    [property: Key(1)] byte[] Tracestate);

[MessagePackObject]
public sealed record OpLogEntryWire(
    [property: Key(0)] ulong Sequence,
    [property: Key(1)] OperationIdWire Id,
    [property: Key(2)] byte[] Model,
    [property: Key(3)] string EntityKey,
    [property: Key(4)] string Family,
    [property: Key(5)] string Kind,
    [property: Key(6)] byte[] Payload,
    [property: Key(7)] byte[] ContentKey,
    [property: Key(8)] TraceSlotWire Trace,
    [property: Key(9)] byte[][] Closure,
    [property: Key(10)] string Actor,
    [property: Key(11)] ulong PhysicalTicks,
    [property: Key(12)] ulong Logical);

public static class OpLogWire {
    const int Fixed = 16;
    static readonly MessagePackSerializerOptions Options = SnapshotCodec.Binary.WithCompression(MessagePackCompression.None);

    public static Fin<byte[]> Encode(OpLogEntry entry) =>
        Op.Of().Catch(() => Project(entry))
            .Bind(Admit)
            .Bind(admitted => Op.Of().Catch(() => MessagePackSerializer.Serialize(Project(admitted), Options)))
            .MapFail(static error => new SyncFault.TransferEncode("oplog", error));

    public static Fin<OpLogEntry> Decode(ReadOnlyMemory<byte> payload) =>
        Op.Of().Catch(() => MessagePackSerializer.Deserialize<OpLogEntryWire>(payload, Options))
            .Bind(Admit)
            .MapFail(static error => new SyncFault.TransferDecode("oplog", error));

    // The plural forms ACCUMULATE: a peer offering a hundred frames with two bad ones learns both, where a
    // short-circuiting traverse names the first and hides the rest behind a re-send that fails the same way.
    // Every frame-bearing crossing — the delta exchange, the push handler, the checkout stream — takes these.
    public static Fin<Seq<ReadOnlyMemory<byte>>> Encode(Seq<OpLogEntry> entries) =>
        entries.Traverse(static entry => Encode(entry).ToValidation()).As().ToFin()
            .Map(static frames => frames.Map(static frame => (ReadOnlyMemory<byte>)frame));

    public static Fin<Seq<OpLogEntry>> Decode(Seq<ReadOnlyMemory<byte>> frames) =>
        frames.Traverse(static frame => Decode(frame).ToValidation()).As().ToFin();

    static OpLogEntryWire Project(OpLogEntry entry) => new(
        checked((ulong)entry.Sequence),
        new OperationIdWire(
            Uuid(entry.Id.Origin),
            entry.Id.Counter.Value,
            [.. entry.Id.Context.Ordered.Map(static slot => new VectorSlotWire(Uuid(slot.Origin), checked((ulong)slot.Seq)))]),
        Uuid(entry.Model.Value),
        entry.EntityKey,
        entry.Family.Key,
        entry.Kind.Key,
        entry.Payload.ToArray(),
        Wide(entry.ContentKey),
        new TraceSlotWire(entry.Trace.TraceId.ToArray(), entry.Trace.Tracestate.ToArray()),
        [.. entry.Closure.Map(Wide)],
        entry.Actor,
        checked((ulong)entry.Physical.ToUnixTimeTicks()),
        entry.Logical);

    static Fin<OpLogEntry> Admit(OpLogEntryWire wire) =>
        from _shape in Shape(wire)
        from sequence in I63(wire.Sequence, "sequence")
        from counterValue in I63(wire.Id.Counter, "counter")
        from counter in OpSlot.Of(counterValue).ToFin()
        from context in Context(wire.Id.Context)
        let origin = Uuid(wire.Id.Origin)
        from _ordinary in origin != Guid.Empty && counter != OpSlot.Zero
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Error.New("<oplog-operation-id>"))
        from family in Family(wire.Family)
        from kind in Kind(wire.Kind)
        from physical in I63(wire.PhysicalTicks, "physical")
        from logicalValue in I63(wire.Logical, "logical")
        from logical in OpSlot.Of(logicalValue).ToFin()
        let content = BinaryPrimitives.ReadUInt128BigEndian(wire.ContentKey)
        from _key in content == ContentHash.Of(wire.Payload.AsSpan())
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Error.New("<oplog-content-key>"))
        from admitted in Op.Of().Catch(() => new OpLogEntry(
                sequence,
                OperationId.Create(origin, counter, context),
                ModelId.Create(Uuid(wire.Model)),
                wire.EntityKey,
                family,
                kind,
                wire.Payload,
                content,
                TraceSlot.Create(wire.Trace.TraceId, wire.Trace.Tracestate),
                toSeq(wire.Closure.Select(BinaryPrimitives.ReadUInt128BigEndian)),
                wire.Actor,
                Instant.FromUnixTimeTicks(physical),
                logical.Value))
        select admitted;

    static Fin<Unit> Shape(OpLogEntryWire wire) {
        if (wire is null || wire.Id is null || wire.Id.Origin is null || wire.Id.Context is null || wire.Model is null
            || wire.EntityKey is null || wire.Family is null || wire.Kind is null || wire.Payload is null
            || wire.ContentKey is null || wire.Trace is null || wire.Trace.TraceId is null || wire.Trace.Tracestate is null
            || wire.Closure is null || wire.Actor is null || wire.Id.Context.Any(static row => row is null || row.Origin is null)
            || wire.Closure.Any(static key => key is null)) {
            return Fin.Fail<Unit>(Error.New("<oplog-envelope-null>"));
        }
        bool context = wire.Id.Context.All(static row => row.Origin.Length == Fixed)
            && wire.Id.Context.Zip(wire.Id.Context.Skip(1), static (left, right) => left.Origin.AsSpan().SequenceCompareTo(right.Origin) < 0).All(static ordered => ordered);
        bool fixedWidths = wire.Id.Origin.Length == Fixed && wire.Model.Length == Fixed && wire.ContentKey.Length == Fixed
            && wire.Trace.TraceId.Length is 0 or Fixed && wire.Closure.All(static key => key.Length == Fixed);
        bool closure = wire.Closure.Zip(wire.Closure.Skip(1), static (left, right) => left.AsSpan().SequenceCompareTo(right) < 0)
            .All(static ordered => ordered)
            && !wire.Closure.Any(key => key.AsSpan().SequenceEqual(wire.ContentKey));
        return fixedWidths && context && closure
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(Error.New("<oplog-envelope-contract>"));
    }

    // The ONE `ulong` → `long` gate on this feed: the envelope's own columns and the generated cursor's drain
    // position both cross here, so a second width check beside it cannot admit what this one refuses.
    public static Fin<long> I63(ulong value, string field) => value <= (ulong)long.MaxValue
        ? Fin.Succ((long)value)
        : Fin.Fail<long>(Error.New($"<oplog-{field}-unsigned:{value}>"));

    static Fin<VersionVector> Context(VectorSlotWire[] rows) =>
        rows.Traverse(static row => I63(row.Sequence, "vector").Map(sequence => (Uuid(row.Origin), sequence)))
            .Map(static slots => new VersionVector(toHashMap(slots)));

    static Fin<ColumnFamily> Family(string key) =>
        toSeq(ColumnFamily.Items).Find(row => string.Equals(row.Key, key, StringComparison.Ordinal))
            .ToFin(Error.New($"<oplog-family:{key}>"));

    static Fin<SyncOpKind> Kind(string key) =>
        toSeq(SyncOpKind.Items).Find(row => string.Equals(row.Key, key, StringComparison.Ordinal))
            .ToFin(Error.New($"<oplog-kind:{key}>"));

    // The ONE 16-byte big-endian store-id correspondence this feed publishes. The MessagePack envelope and the
    // generated `SyncCursorWire` both fill from here, so a peer reads one origin spelling off either carrier;
    // the span inverse serves an array column and a `ByteString.Span` with no copy between them.
    public static byte[] Uuid(Guid value) {
        byte[] bytes = new byte[Fixed];
        value.TryWriteBytes(bytes, bigEndian: true, out _);
        return bytes;
    }

    public static Guid Uuid(ReadOnlySpan<byte> value) => new(value, bigEndian: true);
    static byte[] Wide(UInt128 value) { byte[] bytes = new byte[Fixed]; BinaryPrimitives.WriteUInt128BigEndian(bytes, value); return bytes; }
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

    public static Fin<DotSource> Boot(Guid origin, VersionVector durableHead, Seq<OpLogEntry> undrainedTail) =>
        origin == Guid.Empty
            ? Fin.Fail<DotSource>(Error.New("<dot-source-empty-origin>"))
            : Fin.Succ(new DotSource(origin, Atom(undrainedTail.Fold(durableHead, static (held, entry) => held.Join(entry.Id.Frontier)))));

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

public sealed record OpLogRuntime(DotSource Dots, ChangefeedSubscription Subscription) {
    public static Fin<OpLogRuntime> Boot(
        Guid origin, VersionVector durableHead, Seq<OpLogEntry> undrainedTail, Func<Seq<OpLogEntry>, IO<Unit>> drain) =>
        DotSource.Boot(origin, durableHead, undrainedTail)
            .Map(dots => new OpLogRuntime(dots, new ChangefeedSubscription(dots, drain)));
}

// --- [OPERATIONS] ------------------------------------------------------------------------
// Every interior divergence is one attribute row and zero assignment statements survive. The cross-runtime
// positional order belongs to `OpLogEntryWire`; Mapperly never decides a wire slot.
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

    // Composition binds `feed` to the durable row source; every consumer dials a `ReplayWindow` value.
    public static Seq<OpLogEntry> Replay(Seq<OpLogEntry> feed, ReplayWindow window) =>
        toSeq(feed.Filter(window.Admits).OrderBy(static entry => entry.Sequence).Take(window.Take));

    // Closure names the geometry a peer must hold to materialize the change: a non-`Object` node contributes nothing,
    // so the closure is the BLOB set, never the node set.
    static Seq<UInt128> Closure(GraphDelta delta) =>
        toSeq((delta.AddedNodes + delta.RevisedNodes.Map(static r => r.After))
            .Choose(static n => Optional(n as Node.Object))
            .SelectMany(static o => o.Representations.ByIdentifier.Values)
            .Distinct()
            .OrderBy(static key => key));

    // Authoring mints a NON-structural lane entry: one HLC stamp, one dot, one captured trace, the `build`
    // continuation closing over its owner's lane and payload. A caller minting its own dot invents a causal position.
    public static IO<OpLogEntry> Stamp(ReceiptSinkPort sink, DotSource dots, ProjectionContext frame, Func<(OperationId Id, Instant Physical, ulong Logical, TraceSlot Trace), OpLogEntry> build) =>
        dots.Reserve(1).ToFin().Match(
            Succ: reserved => IO.lift(() => (Wall: frame.Now(), Trace: TraceSlot.Capture()))
                .Map(captured => (Cell: sink.Hlc.Swap(last => ReceiptSinkPort.Advance(last, captured.Wall)), Id: reserved[0], captured.Trace))
                .Map(stamped => build((stamped.Id, stamped.Cell.Physical, stamped.Cell.Logical, stamped.Trace))),
            Fail: IO.fail<OpLogEntry>);

    // The sole CRDT authoring path: generated protobuf bytes, their exact seed-zero payload key, then the generic
    // positional envelope. No caller can select the CRDT lane while supplying MessagePack-union bytes.
    public static IO<OpLogEntry> Crdt(
        long sequence, ModelId model, string entityKey, SyncOpKind kind, CrdtOp op, string actor,
        ReceiptSinkPort sink, DotSource dots, ProjectionContext frame) =>
        CrdtWire.Encode(op).Match(
            Succ: payload => Stamp(sink, dots, frame, stamped => new OpLogEntry(
                sequence, stamped.Id, model, entityKey, ColumnFamily.Crdt, kind,
                payload, CrdtWire.ContentKey(payload), stamped.Trace, Seq<UInt128>(), actor,
                stamped.Physical, stamped.Logical)),
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
|  [06]   | HLC cell            | event `Timestamp` + `Version`                | physical Unix-tick first, logical second, two `I64` words           |
|  [07]   | sign boundary       | `OpSlot` — the one `long`↔`ulong` admission  | `Signed` total by the cap; zero unchecked casts on the page         |
|  [08]   | origin tie-break    | `Id.Origin` — the dot's own store id         | LWW `(Hlc, OriginStoreId)` deterministic; never a zero              |
|  [09]   | trace slot          | top-level `TraceSlot` field                  | never inside `Payload`; distinct from AppHost `TraceContext` fold   |
|  [10]   | operation identity  | `OperationId` dot over its pre-mint frontier | equal payloads stay distinct; counter is ONE `I64` word in the key  |
|  [11]   | payload identity    | `ContentKey` — `ContentHash.Of` the payload  | transfer set difference and blob dedup; never the entry key         |
|  [12]   | drain position      | `Sequence` — the store-local Marten cursor   | resumable read alone; two stores mint one sequence value            |
|  [13]   | dot custody         | one `DotSource` atom per store               | projection and authoring reserve together; gap-free by law          |
|  [14]   | entry transcription | `[Mapper]` over admitted `EventFacts`        | interior record only; `OpLogEntryWire` pins the thirteen wire slots |

## [03]-[MERGE_LAW]

- Owner: `ConflictReceipt` with option-carried `ConflictSide` evidence; `ConflictVerdict`; `ConflictResult`; `SyncApplyReceipt` the `IValidityEvidence` conservation receipt; `SyncFault` the closed `[Union]` deriving from the KERNEL `Rasm.Domain.Fault` in the 825x band; `SyncSession` the one session capsule carrying the injected `ProjectionContext` frame with its delegate rows; `SyncMerge` the fold surface routing each entry by its `ColumnFamily.Stance` — `Lww`/`FirstWriter` through `Adjudicate`, `Crdt` into `Crdt.Apply`, a winning whole-relation entry through the `Truncate` delegate.
- Cases: four verdict rows — `LocalWin | RemoteWin | Merged | Rejected` — collapse into one `ConflictResult(Verdict, Receipt, Conflicted, Held)` where `Conflicted` separates a genuine divergence from an idempotent-replay `LocalWin` or a fresh `Merged`, and `Held` carries the held content key the fork fault reads without a second lookup. `Rejected` is reachable only on an equal `(stamp, origin)` over divergent content — the causal fork `Apply` lifts to `SyncFault.Forked` and halts on — never a soft conflict bucket. Faults close at `SchemaMismatch | ReplicationFaulted | SpeckleMarshal | TransferDecode | TransferEncode | Unconserved | Forked | Unobserved | SlotOutOfSpace`; the last pair carries a compaction whose minter never observed its horizon and a vector slot outside the dot space, while transfer directions remain distinct.
- Entry: `SyncMerge.Apply(session, incoming)` skips only an identity already applied, refuses a non-redelivery whose causal context the advancing frontier does not dominate, and then dispatches merge. Its receipt proves applied, skipped, conflicted, converged, and pushed counts under `IValidityEvidence`; `Converged(entry, apply)` retains the outer dot through generated-protobuf admission and the state fold; `Settled(session, incoming)` is that same fold with its IO rail captured onto the value channel, the one shape a gRPC handler binds.
- Receipt: `ConflictReceipt` is the typed fork evidence the `Forked` halt carries and the inspector projects; `SyncApplyReceipt` is the per-run apply evidence, self-attesting through the kernel `ValidityClaim.All` fold over its own carried `Batch` — the parameterized `Conserves(long)` knob and a hand `&&` chain are the deleted forms, since the receipt reconstructs the check from its own fields.
- Packages: Rasm (`Rasm.Domain` `Fault` — the federation fault base; `IValidityEvidence`/`ValidityClaim` — the receipt-validity floor; `FaultBand`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one `MergeStance` row carrying its `OpLaw`; a fifth `ConflictVerdict` row is the named defect; a new fault cause is one `SyncFault` case; a new replicated data type is a `Version/commits#CRDT_ALGEBRA` `CrdtField` case with its `Crdt.Law` row, dispatched by this fold, never a fifth scalar arm.
- Boundary: replay dedup is IDENTITY-proven and never content-proven — `entry.Id.Applied(frontier)` skips a redelivery, while `frontier.Dominates(entry.Id.Context)` gates every new entry before its dot advances the fold. Without that second gate, receiving same-origin counter two before counter one advances the scalar frontier and later misclassifies counter one as replay. Each landed entry joins its own `Frontier`; commutation still reads the lane's `Stance.Law` and the CRDT arm's `Crdt.Law`.
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
    [FaultCase(8)]
    public sealed partial record TransferEncode(string Peer, Error Cause) : SyncFault(), ICausedFault;

    public override string Message => Switch(
        schemaMismatch:     static c => $"{c.Local}:{c.Remote}",
        replicationFaulted: static c => $"{c.Slot}:{c.Cause.Message}",
        speckleMarshal:     static c => $"{c.Peer}:{c.Class}",
        transferDecode:     static c => $"{c.Peer}:{c.Cause.Message}",
        unconserved:        static c => $"{c.Batch}!={c.Settled}",
        forked:             static c => $"{c.Receipt.EntityKey}@{c.Receipt.Incoming.Map(static side => side.Stamp.Physical.ToString()).IfNone("<unstamped>")}:{c.Held}!={c.Incoming}",
        unobserved:         static c => $"{c.Field}@{c.Origin:N}",
        slotOutOfSpace:     static c => $"<vector-slot:{c.Slot}>",
        transferEncode:     static c => $"<transfer-encode:{c.Peer}>:{c.Cause.Message}");
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
// floor, because a session whose pending rows live only in memory loses the buffer on process exit. The four DIALED
// ports carry `Fin` inside the effect because a hop SETTLES rather than throws: the composition root fills each with
// the matching `SyncWire` method group, so the transport verdict and its value arrive as one value the pump collapses
// once and no second port set exists beside the generated client. `Fetch` stays the LOCAL entry resolver — the
// in-process cross-session graft reads it, never the peer-facing checkout.
public sealed record SyncSession(
    ProjectionContext Frame, ReceiptSinkPort Sink, Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, SyncCursor Acked, CancellationToken Token,
    Func<VersionVector> Frontier,
    Func<UInt128, bool> Holds, Func<OpLogEntry, Option<OpLogEntry>> Held, Func<OpLogEntry, IO<Unit>> Commit, Func<OpLogEntry, IO<Unit>> Truncate, Func<OpLogEntry, IO<ConflictResult>> Converge,
    Func<SyncCursor, Seq<OpLogEntry>> Pending, Func<long> QueueDepth, Func<UInt128, IO<OpLogEntry>> Fetch,
    Func<Seq<OpLogEntry>, IO<Unit>> Spool, Func<ulong, IO<(ulong Head, Seq<OpLogEntry> Entries)>> Unspool, Func<UInt128, IO<bool>> LocalHas,
    Func<string, SyncCursor, IO<Fin<(ulong SchemaFingerprint, Seq<ReadOnlyMemory<byte>> Frames, SyncCursor Cursor)>>> Pull,
    Func<string, Seq<ReadOnlyMemory<byte>>, IO<Fin<SyncCursor>>> Push,
    Func<string, UInt128, Seq<UInt128>, IO<Fin<Seq<UInt128>>>> Missing,
    Func<string, UInt128, IO<Fin<Seq<ReadOnlyMemory<byte>>>>> Checkout,
    Func<string, Seq<UInt128>, IO<Seq<UInt128>>> HasObjects,
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

    // Composition binds `SyncSession.Converge` through this adapter. Integrity was verified on the held envelope
    // bytes before parsing; generated-message admission and the entry-level compaction gate both run before the
    // state owner receives an operation.
    public static IO<ConflictResult> Converged(OpLogEntry entry, Func<OperationId, CrdtOp, IO<ConflictResult>> apply) =>
        entry.Family != ColumnFamily.Crdt
            ? IO.fail<ConflictResult>(new SyncFault.TransferDecode("crdt", Error.New($"<crdt-family:{entry.Family.Key}>")))
            : entry.ContentKey != CrdtWire.ContentKey(entry.Payload)
            ? IO.fail<ConflictResult>(new SyncFault.TransferDecode("crdt", Error.New("<crdt-content-key>")))
            : CrdtWire.Decode(entry.Payload)
                .Bind(op => Admissible(entry.Id, op).Map(_ => op))
                .Match(Succ: op => apply(entry.Id, op), Fail: IO.fail<ConflictResult>);

    // Dots under the applied frontier settle `Skipped`, identity-proven: content equality cannot separate a
    // redelivery from a second real edit carrying identical bytes. Every surviving entry lands through ONE
    // `ConflictResult` and in EXACTLY ONE counter, which is what makes the conservation fold exact.
    public static IO<SyncApplyReceipt> Apply(SyncSession session, Seq<OpLogEntry> incoming) =>
        incoming.FoldM(
            new Counts(session.Frontier(), Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Conflicts: Seq<ConflictReceipt>()),
            (counts, entry) => entry.Id.Applied(counts.Frontier)
                ? IO.pure(counts with { Skipped = counts.Skipped + 1L })
                : !counts.Frontier.Dominates(entry.Id.Context)
                ? IO.fail<Counts>(new SyncFault.ReplicationFaulted(
                    "oplog-context", Error.New($"<causal-gap:{entry.Id.Wire}>")))
                : (entry.Family.Stance.Convergent ? session.Converge(entry) : IO.pure(Adjudicate(session, entry)))
                    .Bind(result => Landed(session, entry, result, counts)))
            .Map(c => new SyncApplyReceipt(incoming.Count, c.Applied, c.Skipped, c.Conflicted, c.Converged, Pushed: 0L, session.QueueDepth(), c.Conflicts, session.Cursor, session.Acked, session.Frame.Correlation, session.Frame.Now()))
            .Bind(receipt => receipt.IsValid ? IO.pure(receipt) : IO.fail<SyncApplyReceipt>(new SyncFault.Unconserved(receipt.Batch, receipt.Settled)))
            .As();

    // The ONE capture of the fold's IO rail onto the value channel. A gRPC handler answers every refusal as a
    // status, so the push arm binds THIS method group and the fold keeps its own rail untouched — a second
    // capture at each override, or a merge that returns `Fin` to every in-process caller, are the two forms
    // this seat deletes.
    public static IO<Fin<SyncApplyReceipt>> Settled(SyncSession session, Seq<OpLogEntry> incoming) =>
        Apply(session, incoming).Map(static receipt => Fin.Succ(receipt)).Bracket(
            Use: static answered => IO.pure(answered),
            Catch: static error => IO.pure(Fin.Fail<SyncApplyReceipt>(error)),
            Fin: static _ => IO.pure(unit));

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

- Owner: `FlowCapability`/`SyncFlows` the exchange-direction vocabulary and `SyncFlow` its keyless disposition; `SyncTransport` the closed transport family; the `SyncPump` dispatch surface with the `Materialize` checkout bridge and the `Offer` Speckle-diff arm; `SyncPeer` the authority-and-client seat; `SyncWire` the generated `SyncService.SyncServiceClient` binding filling the session's four dialed ports; `SyncWireMap` the one `SyncCursor` ⇄ `SyncCursorWire` seam; `SyncRuntime` the server's dependency record; `SyncEndpoint : SyncService.SyncServiceBase` the four server overrides; `OpLog.TransferSet` the ONE set-difference algebra both ends dial.
- Cases: three transport cases — `HttpDelta`, `SpeckleLikeDiff`, `SubtreeCheckout` — widened by the one `SyncFlow` field whose capability set the `Exchange` fold reads; fan-in, fan-out, and bidirectional are `SyncFlow` rows, never new transport cases, and the empty corner is barred because a transport that neither pulls nor pushes exchanges nothing. Four rpcs close the service — `SyncService.Pull`, `SyncService.Push`, `SyncService.TransferSet`, `SyncService.Checkout` — over `PullRequest`/`PullResponse`, `PushRequest`/`PushResponse`, `TransferSetRequest`/`TransferSetResponse`, and `CheckoutRequest`/`CheckoutResponse`, with `SyncCursorWire` the one position carrier all four share.
- Law: cross-store sync is a PROCESS crossing between two instances of THIS package, so the client half (`SyncWire.Pull`, `SyncWire.Push`, `SyncWire.TransferSet`, `SyncWire.Checkout`) and the server half (`SyncEndpoint.Pull`, `SyncEndpoint.Push`, `SyncEndpoint.TransferSet`, `SyncEndpoint.Checkout`) seat on this one page and the corpus `libs/contracts/manifest.json` `sync-rpc` seam names both anchors. Frames stay the thirteen-slot MessagePack envelope inside the `bytes` columns, opaque to the service, so the generated messages carry POSITION and GENERATION while `OpLogWire` keeps entry identity.
- Exemption: three statement seams, each platform-forced and each stated at its site — the `IAsyncStreamReader<T>` pump (the `ReadAllAsync` drain ships in `Grpc.Net.Common`, a package this folder does not admit), the `IServerStreamWriter<T>` per-message write, and the `FaultWire.Raise` throw inside the generated override, where the typed refusal is sealed on the rail first and the exception is the transport's egress form.
- Entry: `SyncPump.Run(session, transport)` is one total state-threaded dispatch; `SyncWire.Pull|Push|TransferSet|Checkout` each answer `IO<Fin<…>>` shaped as the session port they fill; `SyncEndpoint.Pull|Push|TransferSet|Checkout` answer the four rpcs; `OpLog.TransferSet(root, holds)` projects the missing geometry-BLOB-key manifest — the closure with the root payload key, minus held; `SyncPump.Checkout(source, target, root)` is the IN-PROCESS cross-session graft over the local `Fetch`, never a dial.
- Auto: intra-cluster replication is Marten's own daemon over the shared PostgreSQL, so this axis is the CROSS-store and offline lane — a disconnected editor, a Speckle hub, a peer holding a subgraph — never a re-implementation of single-cluster replication. `HttpDelta` dials `Pull` for a cursor-bounded segment of the peer's feed and `Push` for the pending set past the peer-acked frontier; `SubtreeCheckout` dials `Checkout` for the root envelope and its closure, applies them, and dials `TransferSet` for the blob manifest the receipt accounts; `SpeckleLikeDiff` folds the pending set through `OpLog.TransferSet` over the peer's membership answer and hands the missing set to the SDK marshal.
- Receipt: every transport run yields one `SyncApplyReceipt`; the subtree-checkout transfer count rides the same receipt. Each dial also fans one AppHost `HopReceipt` from `OutboundSurface.Dispatch`, so transport evidence and merge evidence stay two records neither end re-derives.
- Packages: Rasm.Contracts (project — generated `SyncService`, `PullRequest`/`PullResponse`, `PushRequest`/`PushResponse`, `TransferSetRequest`/`TransferSetResponse`, `CheckoutRequest`/`CheckoutResponse`, `SyncCursorWire`, `Clock.Hlc`), Grpc.Core.Api (`SyncService.SyncServiceBase`, `SyncService.SyncServiceClient`, `CallInvoker`, `CallOptions`, `IAsyncStreamReader<T>.MoveNext`, `IServerStreamWriter<T>.WriteAsync`, `ServerCallContext`), Google.Protobuf (`ByteString.CopyFrom`/`Span`/`Memory`, `RepeatedField<T>`, `IMessage<T>`), Rasm.AppHost (project — `WireAdmission.Admit`, `WireBoundary`, `HostWire.Stamp`, `FaultWire.Raise`, `FaultContext`, `OutboundSurface.Dispatch`, `OutboundRuntime`, `OutboundHop`, `HopOutcome`, `HopSettled<T>`), Riok.Mapperly, Rasm (`ContentHash.Wire`/`Admit`, `CapabilitySet`/`CapabilityLaw`, `ReceiptSinkPort`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, Speckle.Sdk (companion, outside-Rhino), BCL inbox.
- Growth: a new transport is one case with one dispatch arm; a new exchange direction is one `SyncFlows` corner; a new rpc is one row on the corpus service with its `SyncWire` method, its `SyncEndpoint` override, and its `sync-rpc` manifest case landing together — a service-only or client-only declaration is deleted rather than padded with an unused adapter; a new graph-checkout shape is one entry over `OpLog.TransferSet`, never a second diff algebra.
- Boundary: every dial rides the AppHost `OutboundHop` keyed pipeline — the three unary rpcs on `OutboundHop.Grpc` under a per-call idempotency key, the checkout stream on `OutboundHop.ServerStream` — so retry, backoff, breaker, rate limit, and hop deadlines are owned there and the database stays excluded from the hop law. `SyncWire` holds NO channel: the composition root mints one per peer and hands the intercepted `CallInvoker` the generated client binds, so a raw `GrpcChannel` here would ride no pipeline and no interceptor. Every reply admits through AppHost `WireAdmission.Admit(reply, WireBoundary.InboundPayload, Op.Of())` — this package holds no `ParseGuard`, and a second validator beside the host's would evaluate one rule graph twice.
- Boundary: `OpLog.TransferSet` is the ONE set-difference algebra — the closure GEOMETRY-blob manifest with the root payload key, minus what the target holds — and it is the BLOB-transfer set the content-addressed store moves, NOT an op-log-entry fetch input; running a second walk-and-diff on the calling side is the deleted form, which is exactly why the manifest is ASKED for: the peer holds the stored root's whole closure where the caller sees only the frames the stream handed it. The checkout stream carries op-log ENVELOPES alone and resolves the root's closure ONE level, because `Closure` is already the transitive descendant set the projection sealed; the blob BYTES those envelopes reference ride the content-addressed store under the `TransferSet` answer and never this stream.
- Boundary: the two cursor SPACES thread the exchange — the pull leg advances this store's position in the PEER's feed and the push leg the peer-returned confirmation in OURS — and overwriting one with the other resumes the next pull from this store's push frontier inside the PEER's sequence space, silently skipping every entry between. `SyncCursorWire.origin_store` names WHICH feed a position lives in, so `SyncEndpoint.Pull` refuses a cursor naming another store rather than slicing a foreign origin out of this feed, and genesis is the one origin-free spelling a first pull carries. A pulled or streamed generation that differs from this store's stays `SyncFault.SchemaMismatch`, the same refusal on both the segment and the per-frame arm.
- Boundary: Speckle's wire leg lives OUTSIDE-RHINO on the companion target, so the in-Rhino assembly composes only the case and the marshal delegate slot and never references the SDK; the DI-resolved INSTANCE `IOperations.Send` returns a root id that projects onto the offered root `ContentKey` with zero second identity, and a drift between the two faults the run. `SpeckleLikeDiff` keeps `HasObjects` and `SpeckleSend` as SDK-shaped ports precisely because a hub's membership answer is not this service's `TransferSet`, and collapsing the two would make one row's marshal answer for the other's rpc.

```csharp signature
// --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
using System.Globalization;
using Google.Protobuf;
using Grpc.Core;
using Rasm.AppHost.Runtime;
using Rasm.AppHost.Wire;
using Rasm.Contracts.Sync;
using Riok.Mapperly.Abstractions;
using Clock = Rasm.Contracts.Clock;

namespace Rasm.Persistence.Version;

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

// A peer is an AUTHORITY paired with the client bound to it. The hop row admits, credentials, and rate-limits by
// authority, so a client dialed under another peer's row spends a stranger's budget and reads its breaker; the root
// seats one entry per configured peer and an unnamed peer refuses on the rail rather than dialing a fabricated host.
public sealed record SyncPeer(Uri Address, SyncService.SyncServiceClient Client);

// The server's dependency record, filled by the composition root exactly as `SyncSession` is: values and delegates
// alone, so the endpoint holds no container, no store handle, and no clock of its own. Every EFFECTFUL column arrives
// RAILED, because a handler answers every refusal as a status and the capture belongs at the one binding rather than
// at four overrides: the root binds `Apply` to `SyncMerge.Settled` closed over the SERVER's own session, and `Fetch`
// to the store resolver under the same capture. `PageCap` is what THIS store will read, distinct from the page a peer
// may state; `Feed` stays pure because `OpLog.Replay` windows a row source and refuses nothing.
public sealed record SyncRuntime(
    ProjectionContext Frame, ReceiptSinkPort Sink, Guid StoreId, ulong SchemaFingerprint, int PageCap,
    Func<Seq<OpLogEntry>> Feed, Func<UInt128, IO<Fin<OpLogEntry>>> Fetch,
    Func<Seq<OpLogEntry>, IO<Fin<SyncApplyReceipt>>> Apply) {
    // Producer evidence off the ONE frame: the correlation the session carries, the stamp the sink's own HLC mint
    // last issued, and the tenant that frame admitted — never a second clock read at the throw site.
    public FaultContext Context() => FaultContext.Of(Frame.Correlation, Sink.Hlc.Value, Frame.Tenant);
}

// --- [BOUNDARIES] ------------------------------------------------------------------------
// The ONE cursor correspondence. Mapperly owns the OUTBOUND half whole; the inbound half rails, because origin width,
// drain position, and stamp are three foreign columns that refuse independently and a peer sending a malformed cursor
// learns every bad column at once.
[Mapper]
[MapperRequiredMapping(RequiredMappingStrategy.Both)]
public static partial class SyncWireMap {
    // A whole-source reader is the only shape that folds `Physical` and `Logical` into one `Hlc` member, and any
    // `[MapPropertyFromSource(Use = …)]` suppresses RMG020 for EVERY source member — so the two ignore rows are
    // authored inventory, not compiler proof, and a new `SyncCursor` column obligates a row here by inspection.
    [MapProperty(nameof(SyncCursor.OriginStoreId), nameof(SyncCursorWire.OriginStore))]
    [MapPropertyFromSource(nameof(SyncCursorWire.Stamp), Use = nameof(Cell))]
    [MapperIgnoreSource(nameof(SyncCursor.Physical))]
    [MapperIgnoreSource(nameof(SyncCursor.Logical))]
    public static partial SyncCursorWire ToWire(SyncCursor cursor);

    [UserMapping] private static ByteString Origin(Guid store) => ByteString.CopyFrom(OpLogWire.Uuid(store));

    // The drain position crosses the SAME sign boundary every dot takes. A negative is unrepresentable at every
    // producer — Marten's own sequence, `Genesis`, and this seam's own admission — so the refusing arm answers
    // GENESIS. NAMED LOSS: a corrupted position re-pulls the peer's feed from its head, the at-least-once cost this
    // transport already pays, where the unchecked cast it replaces wrapped past the peer's head and skipped the whole
    // tail while reading as a successful resume.
    [UserMapping]
    private static ulong Position(long sequence) =>
        OpSlot.Of(sequence).Match(Succ: static slot => slot.Value, Fail: static _ => OpSlot.Zero.Value);

    private static Clock.Hlc Cell(SyncCursor cursor) => HostWire.Stamp((cursor.Physical, cursor.Logical));

    // `required = true` on both message members already refuses absence at `WireAdmission.Admit`, so the option arms
    // here collapse the nullable reference the generated getter forces and state no second rule.
    public static Fin<SyncCursor> Admit(SyncCursorWire? wire, Op key) =>
        Optional(wire).ToFin(key.InvalidInput(nameof(SyncCursorWire))).Bind(stated =>
            (Store(stated.OriginStore, key).ToValidation(),
             OpLogWire.I63(stated.Sequence, "cursor").ToValidation(),
             Optional(stated.Stamp).ToFin(key.InvalidInput(nameof(Clock.Hlc)))
                 .Bind(held => HostWire.Stamp(held, key)).ToValidation())
                .Apply(static (origin, sequence, stamp) => new SyncCursor(origin, sequence, stamp.Physical, stamp.Logical))
                .As().ToFin());

    private static Fin<Guid> Store(ByteString origin, Op key) =>
        origin.Length == 16
            ? Fin.Succ(OpLogWire.Uuid(origin.Span))
            : Fin.Fail<Guid>(key.InvalidInput(axis: "cursor-origin-width"));

    // Content keys cross as the kernel's own sixteen big-endian bytes in BOTH directions, so no width, order, or
    // hex spelling exists on this page beside `ContentHash`.
    public static Fin<Seq<UInt128>> Keys(IEnumerable<ByteString> wire, Op key) =>
        toSeq(wire).Traverse(row => ContentHash.Admit(row.Span, key).ToValidation()).As().ToFin();
}

// --- [SERVICES] --------------------------------------------------------------------------
// The generated client bound to the AppHost hop law: every rpc leaves through `OutboundSurface.Dispatch`, so retry,
// breaker, rate limit, deadline, and the hop receipt belong to the pipeline and this record owns none of them. The
// three unary verbs take `OutboundHop.Grpc` under a per-call idempotency key; the checkout takes the streaming row —
// the discriminant is the call shape itself, never a knob.
public sealed record SyncWire(
    OutboundRuntime Runtime, ulong SchemaFingerprint, uint PageSize, Func<string, Fin<SyncPeer>> Peers) {
    public IO<Fin<(ulong SchemaFingerprint, Seq<ReadOnlyMemory<byte>> Frames, SyncCursor Cursor)>> Pull(
        string peer, SyncCursor cursor) =>
        Dial(peer, string.Create(CultureInfo.InvariantCulture, $"pull:{cursor.OriginStoreId:N}.{cursor.Sequence}"),
            (client, options) => client
                .PullAsync(new PullRequest { Cursor = SyncWireMap.ToWire(cursor), PageSize = PageSize }, options)
                .ResponseAsync,
            static reply => SyncWireMap.Admit(reply.Cursor, Op.Of()).Map(next =>
                (reply.SchemaFingerprint, toSeq(reply.Frames).Map(static frame => frame.Memory), next)));

    // The batch identity is its extent and its terminal frame: the pump always offers a contiguous run past `Acked`,
    // so that pair names the batch exactly and a re-drive dedups where the next batch does not.
    public IO<Fin<SyncCursor>> Push(string peer, Seq<ReadOnlyMemory<byte>> frames) =>
        Dial(peer, string.Create(CultureInfo.InvariantCulture,
                $"push:{frames.Count}.{ContentHash.Hex(frames.Last.Map(static frame => ContentHash.Of(frame.Span)).IfNone(UInt128.Zero))}"),
            (client, options) => client.PushAsync(new PushRequest {
                SchemaFingerprint = SchemaFingerprint,
                Frames = { frames.Map(static frame => ByteString.CopyFrom(frame.Span)) },
            }, options).ResponseAsync,
            static reply => SyncWireMap.Admit(reply.Acked, Op.Of()));

    public IO<Fin<Seq<UInt128>>> TransferSet(string peer, UInt128 root, Seq<UInt128> held) =>
        Dial(peer, string.Create(CultureInfo.InvariantCulture, $"transfer-set:{ContentHash.Hex(root)}.{held.Count}"),
            (client, options) => client.TransferSetAsync(new TransferSetRequest {
                Root = ContentHash.Wire(root),
                Held = { held.Map(static key => ContentHash.Wire(key)) },
            }, options).ResponseAsync,
            static reply => SyncWireMap.Keys(reply.Missing, Op.Of()));

    // The streaming row, not the keyed one: a checkout is repeat-safe and its body cannot replay concurrently, which
    // is exactly the guarantee `OutboundHop.ServerStream` states.
    public IO<Fin<Seq<ReadOnlyMemory<byte>>>> Checkout(string peer, UInt128 root) =>
        Peers(peer).Match(
            Succ: seat => OutboundSurface.Dispatch<Fin<Seq<ReadOnlyMemory<byte>>>>(
                    Runtime,
                    new OutboundHop.ServerStream(seat.Address),
                    async token => Stated(await Drain(
                        seat.Client
                            .Checkout(new CheckoutRequest { Root = ContentHash.Wire(root) }, new CallOptions(cancellationToken: token))
                            .ResponseStream,
                        SchemaFingerprint, token).ConfigureAwait(false)))
                .Map(static settled => settled.Carried.Bind(static held => held)),
            Fail: static error => IO.pure(Fin.Fail<Seq<ReadOnlyMemory<byte>>>(error)));

    // ONE dial shape for the unary trio: the hop STATES its own outcome from the ADMITTED reply, so a reply the rule
    // graph refuses is a hop fact rather than a delivered frame, and the carried value leaves on the settlement the
    // same run timed — a hop run for its outcome followed by a raw call for its value is the deleted form.
    private IO<Fin<T>> Dial<TReply, T>(
        string peer, string key,
        Func<SyncService.SyncServiceClient, CallOptions, Task<TReply>> call,
        Func<TReply, Fin<T>> admit) where TReply : IMessage<TReply> =>
        Peers(peer).Match(
            Succ: seat => OutboundSurface.Dispatch<Fin<T>>(
                    Runtime,
                    new OutboundHop.Grpc(seat.Address, key),
                    async token => Stated(WireAdmission.Admit(
                            await call(seat.Client, new CallOptions(cancellationToken: token)).ConfigureAwait(false),
                            WireBoundary.InboundPayload, Op.Of())
                        .Bind(admit)))
                .Map(static settled => settled.Carried.Bind(static held => held)),
            Fail: static error => IO.pure(Fin.Fail<T>(error)));

    // The rail IS the hop outcome, folded once: a refusal rides `Faulted` so the pipeline's own transient predicate
    // reads the fault's kernel `Retriability`, and the same value leaves beside it so no caller re-folds the verdict.
    private static (HopOutcome Outcome, Fin<T> Value) Stated<T>(Fin<T> admitted) => (
        admitted.Match(
            Succ: static _ => (HopOutcome)new HopOutcome.Delivered(),
            Fail: static error => new HopOutcome.Faulted(error)),
        admitted);

    // Exemption: `IAsyncStreamReader<T>` is a `Task<bool>` pump and the `ReadAllAsync` drain ships in
    // `Grpc.Net.Common`, a package this folder does not admit — so this is the one client statement body. Frames read
    // whole before they admit, and the refusals then ACCUMULATE, so one malformed checkout names every bad frame.
    private static async Task<Fin<Seq<ReadOnlyMemory<byte>>>> Drain(
        IAsyncStreamReader<CheckoutResponse> stream, ulong fingerprint, CancellationToken token) {
        Seq<CheckoutResponse> read = Seq<CheckoutResponse>();
        while (await stream.MoveNext(token).ConfigureAwait(false)) { read = read.Add(stream.Current); }
        return read.Traverse(frame => Frame(frame, fingerprint).ToValidation()).As().ToFin();
    }

    // Every streamed frame carries the peer's generation, so a stream switching generations mid-checkout refuses on
    // the SAME `SchemaMismatch` a pulled segment takes rather than splicing two schemas into one closure.
    private static Fin<ReadOnlyMemory<byte>> Frame(CheckoutResponse response, ulong fingerprint) =>
        WireAdmission.Admit(response, WireBoundary.InboundPayload, Op.Of())
            .Bind(admitted => admitted.SchemaFingerprint == fingerprint
                ? Fin.Succ(admitted.Frame.Memory)
                : Fin.Fail<ReadOnlyMemory<byte>>(new SyncFault.SchemaMismatch(fingerprint, admitted.SchemaFingerprint)));
}

// The server half of the same service. Every arm seals its answer on the rail before the platform sees it, and the
// dependencies arrive on `SyncRuntime`, so a handler holds no session, no store, and no clock.
public sealed class SyncEndpoint(SyncRuntime runtime) : SyncService.SyncServiceBase {
    public override Task<PullResponse> Pull(PullRequest request, ServerCallContext context) =>
        Answer(context, Segment(request));

    public override Task<PushResponse> Push(PushRequest request, ServerCallContext context) =>
        Answer(context, Landed(request));

    public override Task<TransferSetResponse> TransferSet(TransferSetRequest request, ServerCallContext context) =>
        Answer(context, Manifest(request));

    // Exemption: `IServerStreamWriter<T>.WriteAsync` is the platform's per-message write, so the emission is one
    // sequential await. The closure resolves and encodes WHOLE on the rail first, so a refusal mid-resolution leaves
    // the peer with nothing rather than a spliced partial subtree.
    public override async Task Checkout(
        CheckoutRequest request, IServerStreamWriter<CheckoutResponse> responseStream, ServerCallContext context) {
        foreach (ReadOnlyMemory<byte> frame in await Answer(context, Subtree(request)).ConfigureAwait(false)) {
            await responseStream.WriteAsync(new CheckoutResponse {
                SchemaFingerprint = runtime.SchemaFingerprint,
                Frame = ByteString.CopyFrom(frame.Span),
            }).ConfigureAwait(false);
        }
    }

    // Exemption, stated ONCE for the four overrides: the generated base answers `Task<T>` and gRPC reads a refusal as
    // a THROW, so `FaultWire.Raise` is the one egress on this class and the typed fault is sealed on the rail before
    // it — the transport's egress form, never control flow.
    private async Task<T> Answer<T>(ServerCallContext context, IO<Fin<T>> answered) =>
        (await answered.RunAsync(EnvIO.New(token: context.CancellationToken)).ConfigureAwait(false))
            .Match(Succ: static held => held, Fail: error => throw FaultWire.Raise(error, runtime.Context()));

    // `IO.lift` over a `Fin`-returning thunk resolves to the RAILED overload and lands `IO<T>` with the failure folded
    // onto the error channel, so every pure arm here spells its type argument to carry the `Fin` as its VALUE.
    private IO<Fin<PullResponse>> Segment(PullRequest request) =>
        IO.lift<Fin<PullResponse>>(() => Admitted(request)
            .Bind(stated => Position(stated.Cursor).Map(cursor => (Cursor: cursor, Page: OpLog.Replay(
                runtime.Feed(), ReplayWindow.ForOrigin(runtime.StoreId, cursor.Sequence, Bounded(stated.PageSize))))))
            .Bind(read => OpLogWire.Encode(read.Page).Map(frames => new PullResponse {
                SchemaFingerprint = runtime.SchemaFingerprint,
                Frames = { frames.Map(static frame => ByteString.CopyFrom(frame.Span)) },
                Cursor = SyncWireMap.ToWire(read.Page.Last
                    .Map(last => new SyncCursor(runtime.StoreId, last.Sequence, last.Physical, last.Logical))
                    .IfNone(read.Cursor)),
            })));

    // The acked cursor sits at the last APPLIED entry's real coordinates in the CALLER's own space — its origin, its
    // sequence, its stamp — never a count added to the position the caller stated.
    private IO<Fin<PushResponse>> Landed(PushRequest request) =>
        IO.lift<Fin<Seq<OpLogEntry>>>(() => Admitted(request).Bind(stated =>
                stated.SchemaFingerprint == runtime.SchemaFingerprint
                    ? OpLogWire.Decode(toSeq(stated.Frames).Map(static frame => frame.Memory))
                    : Fin.Fail<Seq<OpLogEntry>>(
                        new SyncFault.SchemaMismatch(runtime.SchemaFingerprint, stated.SchemaFingerprint))))
            .Bind(decoded => decoded.Match(
                Succ: entries => runtime.Apply(entries).Map(applied => applied.Map(_ => new PushResponse {
                    Acked = SyncWireMap.ToWire(entries.Last
                        .Map(static last => new SyncCursor(last.OriginStoreId, last.Sequence, last.Physical, last.Logical))
                        .IfNone(SyncCursor.Genesis)),
                })),
                Fail: static error => IO.pure(Fin.Fail<PushResponse>(error))));

    // The peer resolves the root and runs the ONE set difference against the keys the caller states it holds, so the
    // algebra runs at the end holding the whole closure and never twice on both sides of the wire.
    private IO<Fin<TransferSetResponse>> Manifest(TransferSetRequest request) =>
        IO.lift<Fin<(UInt128 Root, Seq<UInt128> Held)>>(() => Admitted(request).Bind(stated =>
                (ContentHash.Admit(stated.Root.Span, Op.Of()).ToValidation(),
                 SyncWireMap.Keys(stated.Held, Op.Of()).ToValidation())
                    .Apply(static (root, held) => (root, held)).As().ToFin()))
            .Bind(stated => stated.Match(
                Succ: asked => runtime.Fetch(asked.Root).Map(fetched => fetched.Map(entry => new TransferSetResponse {
                    Missing = { OpLog.TransferSet(entry, asked.Held.Contains).Map(static key => ContentHash.Wire(key)) },
                })),
                Fail: static error => IO.pure(Fin.Fail<TransferSetResponse>(error))));

    // Root first, then the entries its closure resolves — ONE level, because `Closure` is already the transitive
    // descendant set the projection sealed and a recursive walk would re-derive what the entry carries. Unresolved
    // keys ACCUMULATE, so a peer whose root outran its own store learns every missing coordinate in one answer.
    private IO<Fin<Seq<ReadOnlyMemory<byte>>>> Subtree(CheckoutRequest request) =>
        IO.lift<Fin<UInt128>>(() => Admitted(request).Bind(stated => ContentHash.Admit(stated.Root.Span, Op.Of())))
            .Bind(root => root.Match(
                Succ: key => runtime.Fetch(key).Bind(fetched => fetched.Match(
                    Succ: entry => entry.Closure.Traverse(runtime.Fetch).As().Map(rows =>
                        rows.Traverse(static row => row.ToValidation()).As().ToFin()
                            .Bind(descendants => OpLogWire.Encode(Seq(entry) + descendants))),
                    Fail: static error => IO.pure(Fin.Fail<Seq<ReadOnlyMemory<byte>>>(error)))),
                Fail: static error => IO.pure(Fin.Fail<Seq<ReadOnlyMemory<byte>>>(error))));

    // ONE admission seat for the four overrides: the boundary row and the op key are stated here and nowhere else,
    // so no arm can admit a request under a different captured-codec site.
    private static Fin<T> Admitted<T>(T request) where T : IMessage =>
        WireAdmission.Admit(request, WireBoundary.InboundPayload, Op.Of());

    // The peer STATES a page and the server CAPS it: the wire rule bounds the request at four thousand ninety-six and
    // this row bounds what this store will read, so a peer cannot size the server's window.
    private int Bounded(uint stated) => (int)Math.Min(stated, (uint)runtime.PageCap);

    // The cursor names WHICH feed the position lives in, so a cursor naming another store would slice a foreign
    // origin out of this feed; genesis is the one origin-free spelling a first pull carries.
    private Fin<SyncCursor> Position(SyncCursorWire? wire) =>
        SyncWireMap.Admit(wire, Op.Of()).Bind(cursor =>
            cursor.OriginStoreId == runtime.StoreId || cursor.OriginStoreId == Guid.Empty
                ? Fin.Succ(cursor)
                : Fin.Fail<SyncCursor>(new SyncFault.ReplicationFaulted(
                    "sync-pull-origin", Error.New($"<foreign-origin:{cursor.OriginStoreId:N}>"))));
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SyncPump {
    public static IO<SyncApplyReceipt> Run(SyncSession session, SyncTransport transport) =>
        transport.Switch(
            state: session,
            httpDelta: static (s, row) => Exchange(s, row),
            speckleLikeDiff: static (s, row) => Offer(s, row),
            subtreeCheckout: static (s, row) => Materialize(s, row));

    // Cross-session checkout: the root entry applies onto the target and the transfer set accounts on the receipt.
    // `OpLog.TransferSet` is dialed directly — a forwarding static under a second name was the deleted alias. This
    // arm is IN-PROCESS and reads the LOCAL `Fetch`; the peer-facing shape is `Materialize`.
    public static IO<SyncApplyReceipt> Checkout(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry =>
            SyncMerge.Apply(target, Seq(entry)).Map(receipt => receipt with { Pushed = OpLog.TransferSet(entry, target.Holds).Count }));

    static IO<SyncApplyReceipt> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        from pulled in row.Flow.Pulls
            ? Dialed(s.Pull(row.Peer, s.Cursor)).Bind(segment => segment.SchemaFingerprint == s.SchemaFingerprint
                ? Railed(OpLogWire.Decode(segment.Frames)).Bind(entries => SyncMerge.Apply(s, entries))
                    .Map(receipt => receipt with { Cursor = segment.Cursor })
                : IO.fail<SyncApplyReceipt>(new SyncFault.SchemaMismatch(s.SchemaFingerprint, segment.SchemaFingerprint)))
            : IO.pure(Idle(s))
        let pending = s.Pending(s.Acked)
        from receipt in row.Flow.Pushes
            ? Railed(OpLogWire.Encode(pending)).Bind(frames => Dialed(s.Push(row.Peer, frames)))
                .Map(acked => pulled with { Pushed = pending.Count, Acked = acked })
            : IO.pure(pulled)
        select receipt;

    // The peer streams the envelopes and the caller applies them; the blob manifest then comes from the PEER, whose
    // stored root carries the whole closure where this side sees only the frames the stream handed it — so the
    // manifest is asked for rather than re-derived from a partial view.
    static IO<SyncApplyReceipt> Materialize(SyncSession s, SyncTransport.SubtreeCheckout row) =>
        from frames in Dialed(s.Checkout(row.Peer, row.Root))
        from entries in Railed(OpLogWire.Decode(frames))
        from receipt in SyncMerge.Apply(s, entries)
        from missing in Dialed(s.Missing(row.Peer, row.Root, Holdings(s, entries)))
        select receipt with { Pushed = missing.Count };

    // What this store already holds among the keys the applied frames name, ordered and distinct as the wire rule
    // demands. An empty statement is a caller holding none of the closure, never a caller declining to answer.
    static Seq<UInt128> Holdings(SyncSession s, Seq<OpLogEntry> entries) =>
        toSeq(entries.Fold(Seq<UInt128>(), static (set, entry) => set + entry.Closure.Add(entry.ContentKey))
            .Distinct().Filter(s.Holds).OrderBy(static key => key));

    // A hop SETTLES rather than throws, so the dialed value carries its own `Fin`; this is the ONE place the pump
    // collapses it back onto the effect rail, and `Railed` is the same collapse for the frame codec.
    static IO<T> Dialed<T>(IO<Fin<T>> dialed) => dialed.Bind(static held => Railed(held));

    static IO<T> Railed<T>(Fin<T> held) => held.Match(Succ: IO.pure, Fail: IO.fail<T>);

    // `Acked` advances to the LAST offered entry's real coordinates, never a `Sequence + count` advance.
    static IO<SyncApplyReceipt> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        from pending in IO.pure(s.Pending(s.Acked))
        from held in s.HasObjects(row.Peer, toSeq(pending.Fold(Seq<UInt128>(), static (set, entry) => set + OpLog.TransferSet(entry, static _ => false)).Distinct()))
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

| [INDEX] | [POLICY]                  | [VALUE]                                           | [BINDING]                                               |
| :-----: | :------------------------ | :------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | intra-cluster replication | Marten daemon `HotCold`                           | this axis is the cross-store/offline lane only          |
|  [02]   | graph checkout            | fetch+apply root; `TransferSet` is the manifest   | closure set via the blob store, not an op-log `Fetch`   |
|  [03]   | Speckle marshal           | DI-resolved instance `IOperations`                | outside-Rhino; `rootObjId` → `ContentKey`; drift faults |
|  [04]   | http delta                | `SyncServiceClient` through AppHost `OutboundHop` | database excluded from the hop law                      |
|  [05]   | exchange direction        | `CapabilitySet<FlowCapability>` under its law     | the empty corner is barred, never merely unwritten      |
|  [06]   | reply admission           | AppHost `WireAdmission.Admit`                     | one rule graph; this package holds no `ParseGuard`      |
|  [07]   | handler refusal           | AppHost `FaultWire.Raise(fault, FaultContext)`    | the one throw; the typed fault seals on the rail first  |
|  [08]   | server page bound         | `min(PullRequest.page_size, SyncRuntime.PageCap)` | a peer states a page; the store caps what it reads      |

Per-transport descriptor: the sentence a row is chosen on, the rpcs it dials, its guarantee, and what settles a leg. `admit` is `SyncPump.Run` for all three and `tenancy` is the peer identity the session carries, so both stay uniform and only the differing coordinates earn columns.

| [INDEX] | [TRANSPORT]       | [FITS]               | [DIALS]                     | [DELIVER]                  | [SETTLE]                          |
| :-----: | :---------------- | :------------------- | :-------------------------- | :------------------------- | :-------------------------------- |
|  [01]   | `HttpDelta`       | feed-owning peer     | `SyncService.Pull` + `Push` | at-least-once past `Acked` | peer-returned push ack            |
|  [02]   | `SpeckleLikeDiff` | object-graph hub     | Speckle SDK marshal         | at-least-once past `Acked` | `rootObjId` matching the head key |
|  [03]   | `SubtreeCheckout` | single-subgraph peer | `Checkout` + `TransferSet`  | content-addressed closure  | applied entries + blob manifest   |

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
