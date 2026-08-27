# [PERSISTENCE_VERSION_LEDGER]

## [01]-[INDEX]

- [02]-[CHANGEFEED]: `OpLogEntry` projects Marten events, with the `OpSlot` sign boundary, HLC stamping, the trace slot, the closure manifest, and the `ReplayWindow` windowed read.
- [03]-[MERGE_LAW]: LWW adjudication, conflicts, the idempotent apply fold, CRDT dispatch, and the conservation invariant.
- [04]-[SYNC_TRANSPORTS]: `SyncTransport` closes the transport family over its `SyncFlow` capability set, `SyncWire` and `SyncEndpoint` bind the two ends of the generated `SyncService`, and the Speckle marshal keeps its SDK adapter.
- [05]-[PRESENCE]: ephemeral presence rows, the lossy awareness lane, and the working-set checkout.

## [02]-[CHANGEFEED]

- Owner: `SyncCapability` the write-verb capability vocabulary and `SyncOps` its legal corners; `SyncOpKind` the write-verb axis carrying its capability set and its `Fact` announcement spelling; `OpCapability`/`OpLaws` and `OpLaw` the solution's one commutation vocabulary; `ColumnFamily` the merge-lane axis carrying `MergeStance` AND `SnapshotCodec` as one row, so the lane selects adjudication algebra and payload codec together; `TraceSlot` the changefeed W3C trace-id carrier; `OpSlot` the ONE sign boundary between the `long` version-vector slot space and the `ulong` dot space; `OperationId` the dot-plus-frontier operation identity; `DotSource` the store's one dot minter; `EventFacts` the admitted Marten-event evidence; `OpLogMapper` the generated `EventFacts`→`OpLogEntry` transcription; `OpLogEntry` the interior changefeed record; `OpLogEntryWire` and `OpLogWire` the explicit primitive thirteen-slot MessagePack boundary; `ReplayWindow` the one windowed-read parameterization; `ChangefeedSubscription` the `SubscriptionBase` batched drain; the `OpLog` project-stamp-replay surface.
- Cases: `SyncOpKind` is `Upsert | Delete | Truncate | Presence`, each carrying its past-tense `Fact` and its held `SyncCapability` set — `{}`, `{Tombstone}`, `{Tombstone, WholeRelation}` are the three legal corners, so a whole-relation verb that is not a tombstone is unrepresentable rather than merely unwritten. `OpLaw` is `Ordered | Commutative | Semilattice`, the same triple `Version/commits#CRDT_ALGEBRA` `Crdt.Law` returns and `typescript:core/state/merge` `Merge.Law` spells, each row holding its `OpCapability` corner. `MergeStance` is `Lww(Ordered) | Crdt(Semilattice) | FirstWriter(Ordered)` and derives `Convergent` off the law. `ColumnFamily` closes at `Scalar | Crdt | Geometry | Presence | Commit | Branch | Attest`, each carrying stance, codec, and durability, so a consumer dispatches on the row and never a string compare.
- Entry: `ProcessEventsAsync` reserves a range's dots once, projects the whole delivered range, and drains it once. `DotSource.Reserve(count)` is the one identity mint for projection and authoring. `OpLog.Project(dots, events)` accumulates admissions; `Replay(feed, window)` owns windowed reads; `Stamp(sink, dots, frame, build)` owns authoring; `OpLogWire.Encode` and `OpLogWire.Decode` own the native thirteen-slot MessagePack boundary in singular and frame-accumulating plural form, and `OpLogWire.Uuid`/`OpLogWire.I63` own the store-id byte form and the `ulong`→`long` gate every sync crossing composes; `TransferSet(entry, holds)` projects closure-minus-held keys.
- Auto: a Marten async subscription projects each committed model event into the one feed; triggers, secondary op-log tables, and per-payload records are inadmissible (`H11`). Every connected `GraphEvent` producer is a structural `geometry`-lane change carrying the codec-encoded `GraphDelta`, adjudicated by `(Hlc, OriginStoreId)` LWW. `Closure` carries descendant geometry content keys, so transfer is set difference rather than a tree walk. Every foreign column of a Marten event admits ONCE into `EventFacts` — stream key, actor header, correlation, and stream version each on `Validation` — so a malformed event reports every bad column at once and the generated mapper builds the entry from evidence alone.
- Packages: Marten owns event-range subscription; MessagePack owns the explicit primitive op-log envelope; Rasm.Element supplies native `GraphDelta.Address`, `Node`, and representation keys; Rasm supplies content hashing, capabilities, and faults; Mapperly supplies admitted entry transcription; NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, DiagnosticSource, and BCL inbox complete the substrate.
- Growth: a new synced concern is one `SyncOpKind` verb with its capability corner, one `ColumnFamily` lane carrying its stance and codec columns, or one payload kind keyed by the lane row's `Codec`; a new windowed-read consumer is one `ReplayWindow` parameterization; a new admitted event column is one `EventFacts` member and one `[MapProperty]` row. Zero new surface — a per-entity-kind outbox table, a bespoke op-log store beneath Marten, a per-payload-kind parallel record, a second dot minter, a second sign boundary, or a per-lane string literal in the merge fold is the deleted form.
- Boundary: `OperationId` is the entry key and `ContentKey` the payload key, and the two never merge — two peers stamping the identical `Set("name", "North Wing")` share one `ContentKey` and carry two dots, so the second edit survives where a content-keyed log drops it and reads the drop as successful dedup. `Counter` is the origin's own `VersionVector` slot rather than a second counter, and `Context` is the pre-mint frontier, so `Order` is total from two ids with no feed walk and `Applied` is the exact replay test the merge fold takes. `Sequence` survives as the store-local drain cursor ALONE — a resumable position, never an identity, because two stores mint sequence 41 and one entry cannot answer to both. `DotSource` is the store's single minter, so the gap-free dot law holds across the projection and the authoring path, and a restart re-seeds its atom from the durable head joined with the tail past the cursor.
- Boundary: `OpSlot` is the ONE sign boundary this feed carries. Version vectors publish `long` slots while every dot is `ulong`, and an unchecked `(long)Counter` past `long.MaxValue` reads NEGATIVE — a dominance test then answers "already applied" for an operation nobody applied, silently dropping it. Construction caps the carrier at `long.MaxValue`, so `Signed` is total and no cast survives anywhere on the page; the mirror of the AppHost port-side `[ValueObject<ulong>]` admission, landed store-side because no AppHost type crosses down. `Counter` still projects the identical `ulong` onto the wire and ONE eight-byte `CanonicalWriter.I64` word into the preimage — the sixteen-byte `U128` twin that once disagreed with it is gone — so the frozen thirteen-slot roster is untouched.
- Boundary: Marten's `origin` header no longer reaches the entry — `OriginStoreId` reads the dot's own `Id.Origin`, so the LWW tie-break is deterministic across peers and no missing header fabricates the `Guid.Empty` bucket that collapsed every origin into one. Marten events project the changefeed: the connected `GraphEvent` producer is the structural `geometry`-lane `GraphDelta`, never a `crdt` op. Persistence only reads `Activity.Current` into `TraceSlot`; `OpLogEntry` carries no correlation field because correlation stays on the session frame.
- Boundary: the durable lanes (`Family.Durable`) are the exactly-once CDC row source the `Version/egress` pump drains past the `Store/coordination#OUTBOX_CURSOR`, and `ReplayWindow.DurableOps` is that drain's parameterization; the presence/awareness lane (`durable: false`) stays the lossy `DrainSurface` channel and NEVER the exactly-once CDC envelope.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
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

[ValueObject<ulong>]
public readonly partial struct OpSlot {
    public static readonly OpSlot Zero = Create(0UL);
    public long Signed => (long)Value;
    public OpSlot Next => Create(Value + 1UL);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ulong value) {
        if (value > (ulong)long.MaxValue) validationError = new ValidationError($"<op-slot-unsigned:{value}>");
    }

    public static Validation<Error, OpSlot> Of(long signed) =>
        signed >= 0L
            ? Success<Error, OpSlot>(Create((ulong)signed))
            : Fail<Error, OpSlot>(new SyncFault.SlotOutOfSpace(signed));
}

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

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class OperationId {
    public static readonly OperationId Genesis = new(Guid.Empty, OpSlot.Zero, VersionVector.Empty);
    public Guid Origin { get; }
    public OpSlot Counter { get; }
    public VersionVector Context { get; }

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

    public VersionVector Frontier => Context.Advance(Origin, 1L);
    public bool Applied(VersionVector frontier) => frontier.At(Origin) >= Counter.Signed;

    public static VectorOrder Order(OperationId left, OperationId right) =>
        (left.Applied(right.Context), right.Applied(left.Context)) switch {
            (true, true) => VectorOrder.Equal,
            (true, false) => VectorOrder.Before,
            (false, true) => VectorOrder.After,
            _ => VectorOrder.Concurrent,
        };

    public UInt128 Key => ContentHash.Of(this, static (id, writer) => id.CanonicalBytes(writer));
    public string Wire => string.Create(CultureInfo.InvariantCulture, $"{Origin:N}.{Counter.Value}");

    public void CanonicalBytes(CanonicalWriter writer) {
        writer.String(Origin.ToString("N")).I64(unchecked((long)Counter.Value));
        Context.CanonicalBytes(writer);
    }
}

public readonly record struct EventFacts(
    long Sequence, OperationId Dot, ModelId Model, string EntityKey, SyncOpKind Kind,
    ReadOnlyMemory<byte> Payload, UInt128 ContentKey, TraceSlot Trace, Seq<UInt128> Closure,
    string Actor, Instant Physical, OpSlot Logical);

[method: MapperConstructor]
public sealed record OpLogEntry(
    long Sequence, OperationId Id, ModelId Model, string EntityKey, ColumnFamily Family, SyncOpKind Kind,
    ReadOnlyMemory<byte> Payload, UInt128 ContentKey, TraceSlot Trace, Seq<UInt128> Closure,
    string Actor, Instant Physical, ulong Logical) {
    public Hlc Stamp => new(Physical, Logical);
    public Guid OriginStoreId => Id.Origin;
    public SnapshotCodec Codec => Family.Codec;
}

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
        Try.lift(() => Project(entry)).Run().Bind(static inner => inner)
            .Bind(Admit)
            .Bind(admitted => Try.lift(() => MessagePackSerializer.Serialize(Project(admitted), Options)).Run().Bind(static inner => inner))
            .MapFail(static error => new SyncFault.TransferEncode("oplog", error));

    public static Fin<OpLogEntry> Decode(ReadOnlyMemory<byte> payload) =>
        Try.lift(() => MessagePackSerializer.Deserialize<OpLogEntryWire>(payload, Options)).Run().Bind(static inner => inner)
            .Bind(Admit)
            .MapFail(static error => new SyncFault.TransferDecode("oplog", error));

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
        from admitted in Try.lift(() => new OpLogEntry(
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
                logical.Value)).Run().Bind(static inner => inner)
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

    public static Fin<long> I63(ulong value, string field) => value <= (ulong)long.MaxValue
        ? Fin.Succ((long)value)
        : Fin.Fail<long>(Error.New($"<oplog-{field}-unsigned:{value}>"));

    static Fin<VersionVector> Context(VectorSlotWire[] rows) =>
        rows.Traverse(static row => I63(row.Sequence, "vector").Map(sequence => (Uuid(row.Origin), sequence)))
            .Map(static slots => new VersionVector(toHashMap(slots)));

    static Fin<ColumnFamily> Family(string key) =>
        toSeq(ColumnFamily.Items).Find(row => string.Equals(StringComparison.Ordinal))
            .ToFin(Error.New($"<oplog-family:{key}>"));

    static Fin<SyncOpKind> Kind(string key) =>
        toSeq(SyncOpKind.Items).Find(row => string.Equals(StringComparison.Ordinal))
            .ToFin(Error.New($"<oplog-kind:{key}>"));

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

public readonly record struct ReplayWindow(Option<Guid> Origin, Option<string> EntityKey, Option<ModelId> Model, Seq<ColumnFamily> Families, long AfterSequence, int Take) {
    public static ReplayWindow ForEntity(string entityKey, long afterSequence, int take) => new(None, Some(entityKey), None, Seq<ColumnFamily>(), afterSequence, take);
    public static ReplayWindow ForOrigin(Guid origin, long afterSequence, int take) => new(Some(origin), None, None, Seq<ColumnFamily>(), afterSequence, take);
    public static ReplayWindow DurableOps(long afterSequence, int take) => new(None, None, None, toSeq(ColumnFamily.Items).Filter(static f => f.Durable), afterSequence, take);
    public bool Admits(OpLogEntry entry) =>
        (entry.Sequence > AfterSequence)
        && Origin.Map(o => o == entry.OriginStoreId).IfNone(true)
        && EntityKey.Map(k => string.Equals(k, entry.EntityKey, StringComparison.Ordinal)).IfNone(true)
        && Model.Map(m => m == entry.Model).IfNone(true)
        && (Families.IsEmpty || Families.Contains(entry.Family));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DotSource(Guid origin, Atom<VersionVector> frontier) {
    public VersionVector Frontier => frontier.Value;

    public static Fin<DotSource> Boot(Guid origin, VersionVector durableHead, Seq<OpLogEntry> undrainedTail) =>
        origin == Guid.Empty
            ? Fin.Fail<DotSource>(Error.New("<dot-source-empty-origin>"))
            : Fin.Succ(new DotSource(origin, Atom(undrainedTail.Fold(durableHead, static (held, entry) => held.Join(entry.Id.Frontier)))));

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

// --- [OPERATIONS] ----------------------------------------------------------------------
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
    public static Validation<Error, Seq<OpLogEntry>> Project(Seq<OperationId> dots, Seq<IEvent<GraphEvent>> events) =>
        events.Zip(dots).Traverse(static pair => Project(pair.Item2, pair.Item1)).As();

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

    static string EntityKey(IEvent<GraphEvent> e) => e.StreamKey ?? e.StreamId.ToString();

    static string HeaderValue(IEvent<GraphEvent> e, string key) =>
        e.Headers is { } h && h.TryGetValue(out object? value) ? value?.ToString() ?? string.Empty : string.Empty;

    public static Seq<OpLogEntry> Replay(Seq<OpLogEntry> feed, ReplayWindow window) =>
        toSeq(feed.Filter(window.Admits).OrderBy(static entry => entry.Sequence).Take(window.Take));

    static Seq<UInt128> Closure(GraphDelta delta) =>
        toSeq((delta.AddedNodes + delta.RevisedNodes.Map(static r => r.After))
            .Choose(static n => Optional(n as Node.Object))
            .SelectMany(static o => o.Representations.ByIdentifier.Values)
            .Distinct()
            .OrderBy(static key => key));

    public static IO<OpLogEntry> Stamp(DotSource dots, ProjectionContext frame, Func<(OperationId Id, Instant Physical, ulong Logical, TraceSlot Trace), OpLogEntry> build) =>
        dots.Reserve(1).ToFin().Match(
            Succ: reserved => IO.lift(() => (Wall: frame.Now(), Trace: TraceSlot.Capture()))
                .Map(captured => (Cell: frame.Clock.Stamp(captured.Wall), Id: reserved[0], captured.Trace))
                .Map(stamped => build((stamped.Id, stamped.Cell.Physical, stamped.Cell.Logical, stamped.Trace))),
            Fail: IO.fail<OpLogEntry>);

    public static IO<OpLogEntry> Crdt(
        long sequence, ModelId model, string entityKey, SyncOpKind kind, CrdtOp op, string actor,
        DotSource dots, ProjectionContext frame) =>
        CrdtWire.Encode().Match(
            Succ: payload => Stamp(dots, frame, stamped => new OpLogEntry(
                sequence, stamped.Id, model, entityKey, ColumnFamily.Crdt, kind,
                payload, CrdtWire.ContentKey(payload), stamped.Trace, Seq<UInt128>(), actor,
                stamped.Physical, stamped.Logical)),
            Fail: IO.fail<OpLogEntry>);

    public static Seq<UInt128> TransferSet(OpLogEntry entry, Func<UInt128, bool> holds) =>
        entry.Closure.Add(entry.ContentKey).Filter(key => !holds());
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

- Owner: `Conflict` with option-carried `ConflictSide` evidence; `ConflictVerdict`; `ConflictResult`; `SyncOutcome` the `IValidityEvidence` conservation record of one apply run; `SyncFault` the closed `[Union]` deriving from the KERNEL `Rasm.Domain.Fault` in the 825x band; `SyncSession` the one session capsule carrying the injected `ProjectionContext` frame with its delegate rows; `SyncMerge` the fold surface routing each entry by its `ColumnFamily.Stance` — `Lww`/`FirstWriter` through `Adjudicate`, `Crdt` into `Crdt.Apply`, a winning whole-relation entry through the `Truncate` delegate.
- Cases: four verdict rows — `LocalWin | RemoteWin | Merged | Rejected` — collapse into one `ConflictResult(Verdict, Conflict, Conflicted, Held)` where `Conflicted` separates a genuine divergence from an idempotent-replay `LocalWin` or a fresh `Merged`, and `Held` carries the held content key the fork fault reads without a second lookup. `Rejected` is reachable only on an equal `(stamp, origin)` over divergent content — the causal fork `Apply` lifts to `SyncFault.Forked` and halts on — never a soft conflict bucket. Faults close at `SchemaMismatch | ReplicationFaulted | SpeckleMarshal | TransferDecode | TransferEncode | Unconserved | Forked | Unobserved | SlotOutOfSpace`; the last pair carries a compaction whose minter never observed its horizon and a vector slot outside the dot space, while transfer directions remain distinct.
- Entry: `SyncMerge.Apply(session, incoming)` skips only an identity already applied, refuses a non-redelivery whose causal context the advancing frontier does not dominate, and then dispatches merge. Its `SyncOutcome` proves applied, skipped, conflicted, converged, and pushed counts under `IValidityEvidence`; `Converged(entry, apply)` retains the outer dot through generated-protobuf admission and the state fold; `Settled(session, incoming)` is that same fold with its IO captured onto the value channel, the one shape a gRPC handler binds.
- Output: `Conflict` is the typed fork evidence the `Forked` halt carries and the inspector projects; `SyncOutcome` is the per-run apply record, self-attesting through the kernel `ValidityClaim.All` fold over its own carried `Batch` — the parameterized `Conserves(long)` knob and a hand `&&` chain are the deleted forms, since the record reconstructs the check from its own fields.
- Packages: Rasm (`Rasm.Domain` `Fault` — the federation fault base; `IValidityEvidence`/`ValidityClaim` — the validity floor; `FaultBand`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new merge stance is one `MergeStance` row carrying its `OpLaw`; a fifth `ConflictVerdict` row is the named defect; a new fault cause is one `SyncFault` case; a new replicated data type is a `Version/commits#CRDT_ALGEBRA` `CrdtField` case with its `Crdt.Law` row, dispatched by this fold, never a fifth scalar arm.
- Boundary: replay dedup is IDENTITY-proven and never content-proven — `entry.Id.Applied(frontier)` skips a redelivery, while `frontier.Dominates(entry.Id.Context)` gates every new entry before its dot advances the fold. Without that second gate, receiving same-origin counter two before counter one advances the scalar frontier and later misclassifies counter one as replay. Each landed entry joins its own `Frontier`; commutation still reads the lane's `Stance.Law` and the CRDT arm's `Crdt.Law`.
- Boundary: LWW per column family is the default. `Held` resolves the competing local entry per model and family; content-key equality adjudicates `LocalWin` as an idempotent replay; an absent competitor adjudicates `Merged` through `Fresh`, whose held half is `None` rather than a zero-stamp sentinel. Any HLC-resolved win over differing content is a genuine divergence the fold counts, and an equal `(stamp, origin)` over divergent content is the fork `Apply` halts on. Lane `FirstWriter` is earliest-wins, the inverse direction of the LWW default.
- Boundary: `Maintain` is the one arm whose admissibility outlives its fold — compaction commutes and absorbs as a filter, yet it is a MEET where every sibling is a JOIN, so a horizon its minter never observed reclaims a tombstone a concurrent insert still needs. Only the entry's own `OperationId.Context` evidences that check, so the gate lives at the entry: `Crdt.Apply`, holding no frontier, structurally cannot run it. Family `crdt` routes its `Payload` through `Crdt.Apply` so a concurrent edit converges by the join-semilattice least-upper-bound rather than scalar LWW — the multi-writer offline and IFC three-way merge substrate. Winning whole-relation entries commit through the session `Truncate` delegate, their capability row selecting the relation-wide lane rather than a dead flag.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ConflictSide(Hlc Stamp, string Actor);

public readonly record struct Conflict(
    ModelId Model, string EntityKey, ColumnFamily Family, Option<ConflictSide> Held, Option<ConflictSide> Incoming);

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

public readonly record struct ConflictResult(ConflictVerdict Verdict, Conflict Conflict, bool Conflicted, UInt128 Held);

// --- [ERRORS] --------------------------------------------------------------------------
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
    public sealed partial record Forked(Conflict Conflict, UInt128 Held, UInt128 Incoming) : SyncFault();
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
        forked:             static c => $"{c.Conflict.EntityKey}@{c.Conflict.Incoming.Map(static side => side.Stamp.Physical.ToString()).IfNone("<unstamped>")}:{c.Held}!={c.Incoming}",
        unobserved:         static c => $"{c.Field}@{c.Origin:N}",
        slotOutOfSpace:     static c => $"<vector-slot:{c.Slot}>",
        transferEncode:     static c => $"<transfer-encode:{c.Peer}>:{c.Cause.Message}");
}

// --- [SERVICES] ------------------------------------------------------------------------
public readonly record struct SyncOutcome(
    long Batch, long Applied, long Skipped, long Conflicted, long Converged, long Pushed, long QueueDepth,
    Seq<Conflict> Conflicts, SyncCursor Cursor, SyncCursor Acked) : IValidityEvidence {
    public long Settled => Applied + Skipped + Conflicted + Converged;
    public bool IsValid => ValidityClaim.All(Batch == Settled, Conflicts.Count == Conflicted);
}

public sealed record SyncSession(
    ProjectionContext Frame, Guid StoreId, ulong SchemaFingerprint, SyncCursor Cursor, SyncCursor Acked, CancellationToken Token,
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

internal readonly record struct Counts(VersionVector Frontier, long Applied, long Skipped, long Conflicted, long Converged, Seq<Conflict> Conflicts);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SyncMerge {
    public static Conflict Conflicted(OpLogEntry held, OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, Some(new ConflictSide(held.Stamp, held.Actor)),
            Some(new ConflictSide(incoming.Stamp, incoming.Actor)));

    static Conflict Fresh(OpLogEntry incoming) =>
        new(incoming.Model, incoming.EntityKey, incoming.Family, None, Some(new ConflictSide(incoming.Stamp, incoming.Actor)));

    public static ConflictResult Adjudicate(SyncSession session, OpLogEntry incoming) =>
        session.Held(incoming) is { IsSome: true, Case: OpLogEntry held }
            ? incoming.ContentKey == held.ContentKey
                ? new ConflictResult(ConflictVerdict.LocalWin, Conflicted(held, incoming), Conflicted: false, held.ContentKey)
                : new ConflictResult(Resolve(incoming, held), Conflicted(held, incoming), Conflicted: true, held.ContentKey)
            : new ConflictResult(ConflictVerdict.Merged, Fresh(incoming), Conflicted: false, UInt128.Zero);

    static ConflictVerdict Resolve(OpLogEntry incoming, OpLogEntry held) =>
        ((incoming.Stamp, incoming.OriginStoreId).CompareTo((held.Stamp, held.OriginStoreId)),
         incoming.Family.Stance == MergeStance.FirstWriter) switch {
            (> 0, true) or (< 0, false) => ConflictVerdict.LocalWin,
            (< 0, true) or (> 0, false) => ConflictVerdict.RemoteWin,
            _ => ConflictVerdict.Rejected,
        };

    public static Fin<Unit> Admissible(OperationId id, CrdtOp op) =>
        op is CrdtOp.Maintain compaction && !id.Context.Dominates(compaction.Quiescent)
            ? Fin.Fail<Unit>(new SyncFault.Unobserved(compaction.Field, id.Origin))
            : Fin.Succ(unit);

    public static IO<ConflictResult> Converged(OpLogEntry entry, Func<OperationId, CrdtOp, IO<ConflictResult>> apply) =>
        entry.Family != ColumnFamily.Crdt
            ? IO.fail<ConflictResult>(new SyncFault.TransferDecode("crdt", Error.New($"<crdt-family:{entry.Family.Key}>")))
            : entry.ContentKey != CrdtWire.ContentKey(entry.Payload)
            ? IO.fail<ConflictResult>(new SyncFault.TransferDecode("crdt", Error.New("<crdt-content-key>")))
            : CrdtWire.Decode(entry.Payload)
                .Bind(op => Admissible(entry.Id).Map(_ => op))
                .Match(Succ: op => apply(entry.Id), Fail: IO.fail<ConflictResult>);

    public static IO<SyncOutcome> Apply(SyncSession session, Seq<OpLogEntry> incoming) =>
        incoming.FoldM(
            new Counts(session.Frontier(), Applied: 0L, Skipped: 0L, Conflicted: 0L, Converged: 0L, Conflicts: Seq<Conflict>()),
            (counts, entry) => entry.Id.Applied(counts.Frontier)
                ? IO.pure(counts with { Skipped = counts.Skipped + 1L })
                : !counts.Frontier.Dominates(entry.Id.Context)
                ? IO.fail<Counts>(new SyncFault.ReplicationFaulted(
                    "oplog-context", Error.New($"<causal-gap:{entry.Id.Wire}>")))
                : (entry.Family.Stance.Convergent ? session.Converge(entry) : IO.pure(Adjudicate(session, entry)))
                    .Bind(result => Landed(session, entry, result, counts)))
            .Map(c => new SyncOutcome(incoming.Count, c.Applied, c.Skipped, c.Conflicted, c.Converged, Pushed: 0L, session.QueueDepth(), c.Conflicts, session.Cursor, session.Acked))
            .Bind(outcome => outcome.IsValid ? IO.pure(outcome) : IO.fail<SyncOutcome>(new SyncFault.Unconserved(outcome.Batch, outcome.Settled)))
            .As();

    public static IO<Fin<SyncOutcome>> Settled(SyncSession session, Seq<OpLogEntry> incoming) =>
        Apply(session, incoming).Map(static outcome => Fin.Succ(outcome)).Bracket(
            Use: static answered => IO.pure(answered),
            Catch: static error => IO.pure(Fin.Fail<SyncOutcome>(error)),
            Fin: static _ => IO.pure(unit));

    static IO<Counts> Landed(SyncSession session, OpLogEntry entry, ConflictResult result, Counts counts) =>
        result.Verdict == ConflictVerdict.Rejected
            ? IO.fail<Counts>(new SyncFault.Forked(result.Conflict, result.Held, entry.ContentKey))
            : (result.Verdict.Applies && !entry.Family.Stance.Convergent
                    ? entry.Kind.Ops.Admits(SyncCapability.WholeRelation) ? session.Truncate(entry) : session.Commit(entry)
                    : IO.pure(unit))
                .Map(_ => Counted(counts with { Frontier = counts.Frontier.Join(entry.Id.Frontier) }, entry, result));

    static Counts Counted(Counts counts, OpLogEntry entry, ConflictResult result) =>
        result.Conflicted ? counts with { Conflicted = counts.Conflicted + 1L, Conflicts = counts.Conflicts.Add(result.Conflict) }
        : !result.Verdict.Applies ? counts with { Skipped = counts.Skipped + 1L }
        : entry.Family.Stance.Convergent ? counts with { Converged = counts.Converged + 1L }
        : counts with { Applied = counts.Applied + 1L };
}
```

| [INDEX] | [POLICY]                | [VALUE]                                                  | [BINDING]                                          |
| :-----: | :---------------------- | :------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | scalar default          | LWW `(Hlc, OriginStoreId)`; `FirstWriter` earliest-wins  | deterministic total order across peers             |
|  [02]   | crdt lane               | `Crdt.Apply` join-semilattice                            | converges by merge; multi-writer offline substrate |
|  [03]   | causal fork             | equal `(stamp, origin)` divergent content                | `SyncFault.Forked` halts merge                     |
|  [04]   | conservation            | `SyncOutcome.IsValid` — `ValidityClaim.All` over `Batch` | a breach is `SyncFault.Unconserved`                |
|  [05]   | whole-relation truncate | `Kind.Ops` holds `WholeRelation` → `Truncate`            | clears `(Model, Family)`; `Held` answers the head  |
|  [06]   | replay dedup            | `Id.Applied(frontier)` — identity, never content         | equal payloads both land; a redelivery lands once  |
|  [07]   | commutation source      | lane `Stance.Law`; crdt arm `Crdt.Law`                   | one `OpLaw` triple, three runtime transcriptions   |
|  [08]   | compaction admission    | `Id.Context.Dominates(Maintain.Quiescent)`               | else `SyncFault.Unobserved`; the fold cannot check |
|  [09]   | absent conflict side    | corresponding `Conflict` side is `None`                  | never a zero stamp whose ticks leave the domain    |

## [04]-[SYNC_TRANSPORTS]

- Owner: `FlowCapability`/`SyncFlows` the exchange-direction vocabulary and `SyncFlow` its keyless disposition; `SyncTransport` the closed transport family; the `SyncPump` dispatch surface with the `Materialize` checkout bridge and the `Offer` Speckle-diff arm; `SyncPeer` the authority-and-client seat; `SyncWire` the generated `SyncService.SyncServiceClient` binding filling the session's four dialed ports; `SyncWireMap` the one `SyncCursor` ⇄ `SyncCursorWire` mapper; `SyncRuntime` the server's dependency record; `SyncEndpoint : SyncService.SyncServiceBase` the four server overrides; `OpLog.TransferSet` the ONE set-difference algebra both ends dial.
- Cases: three transport cases — `HttpDelta`, `SpeckleLikeDiff`, `SubtreeCheckout` — widened by the one `SyncFlow` field whose capability set the `Exchange` fold reads; fan-in, fan-out, and bidirectional are `SyncFlow` rows, never new transport cases, and the empty corner is barred because a transport that neither pulls nor pushes exchanges nothing. Four rpcs close the service — `SyncService.Pull`, `SyncService.Push`, `SyncService.TransferSet`, `SyncService.Checkout` — over `PullRequest`/`PullResponse`, `PushRequest`/`PushResponse`, `TransferSetRequest`/`TransferSetResponse`, and `CheckoutRequest`/`CheckoutResponse`, with `SyncCursorWire` the one position carrier all four share.
- Exemption: three statement boundaries, each platform-forced and each stated at its site — the `IAsyncStreamReader<T>` pump (the `ReadAllAsync` drain ships in `Grpc.Net.Common`, a package this folder does not admit), the `IServerStreamWriter<T>` per-message write, and the `FaultWire.Raise` throw inside the generated override, where the typed refusal is sealed on the typed result first and the exception is the transport's egress form.
- Entry: `SyncPump.Run(session, transport)` is one total state-threaded dispatch; `SyncWire.Pull|Push|TransferSet|Checkout` each answer `IO<Fin<…>>` shaped as the session port they fill; `SyncEndpoint.Pull|Push|TransferSet|Checkout` answer the four rpcs; `OpLog.TransferSet(root, holds)` projects the missing geometry-BLOB-key manifest — the closure with the root payload key, minus held; `SyncPump.Checkout(source, target, root)` is the IN-PROCESS cross-session graft over the local `Fetch`, never a dial.
- Auto: intra-cluster replication is Marten's own daemon over the shared PostgreSQL, so this axis is the cross-store and offline lane. `HttpDelta` pulls a cursor-bounded segment and pushes the pending set; `SubtreeCheckout` fetches and applies the root envelope and its closure, then transfers the blob manifest; `SpeckleLikeDiff` hands the missing set to the SDK marshal.
- Output: every transport run yields one `SyncOutcome`; the subtree-checkout transfer count rides its `Pushed` column. Each dial settles its own hop outcome at AppHost `OutboundSurface.Dispatch`, so transport evidence and merge evidence stay two records neither end re-derives.
- Growth: a new transport is one case with one dispatch arm; a new exchange direction is one `SyncFlows` corner; a new rpc is one row on the corpus service with its `SyncWire` method, its `SyncEndpoint` override, and its `sync-rpc` manifest case landing together — a service-only or client-only declaration is deleted rather than padded with an unused adapter; a new graph-checkout shape is one entry over `OpLog.TransferSet`, never a second diff algebra.
- Boundary: every dial rides the AppHost `OutboundHop` keyed pipeline — the three unary rpcs on `OutboundHop.Grpc` under a per-call idempotency key, the checkout stream on `OutboundHop.ServerStream` — so retry, backoff, breaker, rate limit, and hop deadlines are owned there and the database stays excluded from the hop law. `SyncWire` holds NO channel: the composition root mints one per peer and hands the intercepted `CallInvoker` the generated client binds, so a raw `GrpcChannel` here would ride no pipeline and no interceptor. Every reply admits through AppHost `WireAdmission.Admit(reply, WireBoundary.InboundPayload)` — this package holds no `ParseGuard`, and a second validator beside the host's would evaluate one rule graph twice.
- Boundary: `OpLog.TransferSet` is the ONE set-difference algebra — the closure GEOMETRY-blob manifest with the root payload key, minus what the target holds — and it is the BLOB-transfer set the content-addressed store moves, NOT an op-log-entry fetch input; running a second walk-and-diff on the calling side is the deleted form, which is exactly why the manifest is ASKED for: the peer holds the stored root's whole closure where the caller sees only the frames the stream handed it. The checkout stream carries op-log ENVELOPES alone and resolves the root's closure ONE level, because `Closure` is already the transitive descendant set the projection sealed; the blob BYTES those envelopes reference ride the content-addressed store under the `TransferSet` answer and never this stream.
- Boundary: the two cursor SPACES thread the exchange — the pull leg advances this store's position in the PEER's feed and the push leg the peer-returned confirmation in OURS — and overwriting one with the other resumes the next pull from this store's push frontier inside the PEER's sequence space, silently skipping every entry between. `SyncCursorWire.origin_store` names WHICH feed a position lives in, so `SyncEndpoint.Pull` refuses a cursor naming another store rather than slicing a foreign origin out of this feed, and genesis is the one origin-free spelling a first pull carries. A pulled or streamed generation that differs from this store's stays `SyncFault.SchemaMismatch`, the same refusal on both the segment and the per-frame arm.
- Boundary: Speckle's wire leg lives OUTSIDE-RHINO on the companion target, so the in-Rhino assembly composes only the case and the marshal delegate slot and never references the SDK; the DI-resolved INSTANCE `IOperations.Send` returns a root id that projects onto the offered root `ContentKey` with zero second identity, and a drift between the two faults the run. `SpeckleLikeDiff` keeps `HasObjects` and `SpeckleSend` as SDK-shaped ports precisely because a hub's membership answer is not this service's `TransferSet`, and collapsing the two would make one row's marshal answer for the other's rpc.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using Google.Protobuf;
using Grpc.Core;
using Rasm.AppHost.Runtime;
using Rasm.AppHost.Wire;
// Contracts are retired from this logic.
using Riok.Mapperly.Abstractions;
// Contracts are retired from this logic.

namespace Rasm.Persistence.Version;

// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SyncTransport {
    private SyncTransport(SyncFlow flow) => Flow = flow;
    public SyncFlow Flow { get; }
    public sealed record HttpDelta(string Peer, SyncFlow Flow) : SyncTransport(Flow);
    public sealed record SpeckleLikeDiff(string Peer, SyncFlow Flow) : SyncTransport(Flow);
    public sealed record SubtreeCheckout(string Peer, UInt128 Root, SyncFlow Flow) : SyncTransport(Flow);
}

public sealed record SyncPeer(Uri Address, SyncService.SyncServiceClient Client);

public sealed record SyncRuntime(
    ProjectionContext Frame, Guid StoreId, ulong SchemaFingerprint, int PageCap,
    Func<Seq<OpLogEntry>> Feed, Func<UInt128, IO<Fin<OpLogEntry>>> Fetch,
    Func<Seq<OpLogEntry>, IO<Fin<SyncOutcome>>> Apply) {
    public FaultContext Context() {
        HlcStamp stamp = Frame.Clock.Stamp(Frame.Now());
        return FaultContext.Of(Frame.Correlation, (stamp.Physical, stamp.Logical), Frame.Tenant);
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper]
[MapperRequiredMapping(RequiredMappingStrategy.Both)]
public static partial class SyncWireMap {
    [MapProperty(nameof(SyncCursor.OriginStoreId), nameof(SyncCursorWire.OriginStore))]
    [MapPropertyFromSource(nameof(SyncCursorWire.Stamp), Use = nameof(Cell))]
    [MapperIgnoreSource(nameof(SyncCursor.Physical))]
    [MapperIgnoreSource(nameof(SyncCursor.Logical))]
    public static partial SyncCursorWire ToWire(SyncCursor cursor);

    [UserMapping] private static ByteString Origin(Guid store) => ByteString.CopyFrom(OpLogWire.Uuid(store));

    [UserMapping]
    private static ulong Position(long sequence) =>
        OpSlot.Of(sequence).Match(Succ: static slot => slot.Value, Fail: static _ => OpSlot.Zero.Value);

    private static Clock.Hlc Cell(SyncCursor cursor) => HostWire.Stamp((cursor.Physical, cursor.Logical));

    public static Fin<SyncCursor> Admit(SyncCursorWire? wire) =>
        Optional(wire).ToFin(new KernelFault.InvalidInput(Axis: Some(nameof(SyncCursorWire)))).Bind(stated =>
            (Store(stated.OriginStore).ToValidation(),
             OpLogWire.I63(stated.Sequence, "cursor").ToValidation(),
             Optional(stated.Stamp).ToFin(new KernelFault.InvalidInput(Axis: Some(nameof(Clock.Hlc))))
                 .Bind(held => HostWire.Stamp(held)).ToValidation())
                .Apply(static (origin, sequence, stamp) => new SyncCursor(origin, sequence, stamp.Physical, stamp.Logical))
                .As().ToFin());

    private static Fin<Guid> Store(ByteString origin) =>
        origin.Length == 16
            ? Fin.Succ(OpLogWire.Uuid(origin.Span))
            : Fin.Fail<Guid>(new KernelFault.InvalidInput(Axis: Some("cursor-origin-width")));

    public static Fin<Seq<UInt128>> Keys(IEnumerable<ByteString> wire) =>
        toSeq(wire).Traverse(row => ContentHash.Admit(row.Span).ToValidation()).As().ToFin();
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record SyncWire(
    OutboundRuntime Runtime, ulong SchemaFingerprint, uint PageSize, Func<string, Fin<SyncPeer>> Peers) {
    public IO<Fin<(ulong SchemaFingerprint, Seq<ReadOnlyMemory<byte>> Frames, SyncCursor Cursor)>> Pull(
        string peer, SyncCursor cursor) =>
        Dial(peer, string.Create(CultureInfo.InvariantCulture, $"pull:{cursor.OriginStoreId:N}.{cursor.Sequence}"),
            (client, options) => client
                .PullAsync(new PullRequest { Cursor = SyncWireMap.ToWire(cursor), PageSize = PageSize }, options)
                .ResponseAsync,
            static reply => SyncWireMap.Admit(reply.Cursor).Map(next =>
                (reply.SchemaFingerprint, toSeq(reply.Frames).Map(static frame => frame.Memory), next)));

    public IO<Fin<SyncCursor>> Push(string peer, Seq<ReadOnlyMemory<byte>> frames) =>
        Dial(peer, string.Create(CultureInfo.InvariantCulture,
                $"push:{frames.Count}.{ContentHash.Hex(frames.Last.Map(static frame => ContentHash.Of(frame.Span)).IfNone(UInt128.Zero))}"),
            (client, options) => client.PushAsync(new PushRequest {
                SchemaFingerprint = SchemaFingerprint,
                Frames = { frames.Map(static frame => ByteString.CopyFrom(frame.Span)) },
            }, options).ResponseAsync,
            static reply => SyncWireMap.Admit(reply.Acked));

    public IO<Fin<Seq<UInt128>>> TransferSet(string peer, UInt128 root, Seq<UInt128> held) =>
        Dial(peer, string.Create(CultureInfo.InvariantCulture, $"transfer-set:{ContentHash.Hex(root)}.{held.Count}"),
            (client, options) => client.TransferSetAsync(new TransferSetRequest {
                Root = ContentHash.Wire(root),
                Held = { held.Map(static key => ContentHash.Wire()) },
            }, options).ResponseAsync,
            static reply => SyncWireMap.Keys(reply.Missing));

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

    private IO<Fin<T>> Dial<TReply, T>(
        string peer, string key,
        Func<SyncService.SyncServiceClient, CallOptions, Task<TReply>> call,
        Func<TReply, Fin<T>> admit) where TReply : IMessage<TReply> =>
        Peers(peer).Match(
            Succ: seat => OutboundSurface.Dispatch<Fin<T>>(
                    Runtime,
                    new OutboundHop.Grpc(seat.Address),
                    async token => Stated(WireAdmission.Admit(
                            await call(seat.Client, new CallOptions(cancellationToken: token)).ConfigureAwait(false),
                            WireBoundary.InboundPayload)
                        .Bind(admit)))
                .Map(static settled => settled.Carried.Bind(static held => held)),
            Fail: static error => IO.pure(Fin.Fail<T>(error)));

    private static (HopOutcome Outcome, Fin<T> Value) Stated<T>(Fin<T> admitted) => (
        admitted.Match(
            Succ: static _ => (HopOutcome)new HopOutcome.Delivered(),
            Fail: static error => new HopOutcome.Faulted(error)),
        admitted);

    private static async Task<Fin<Seq<ReadOnlyMemory<byte>>>> Drain(
        IAsyncStreamReader<CheckoutResponse> stream, ulong fingerprint, CancellationToken token) {
        Seq<CheckoutResponse> read = Seq<CheckoutResponse>();
        while (await stream.MoveNext(token).ConfigureAwait(false)) { read = read.Add(stream.Current); }
        return read.Traverse(frame => Frame(frame, fingerprint).ToValidation()).As().ToFin();
    }

    private static Fin<ReadOnlyMemory<byte>> Frame(CheckoutResponse response, ulong fingerprint) =>
        WireAdmission.Admit(response, WireBoundary.InboundPayload)
            .Bind(admitted => admitted.SchemaFingerprint == fingerprint
                ? Fin.Succ(admitted.Frame.Memory)
                : Fin.Fail<ReadOnlyMemory<byte>>(new SyncFault.SchemaMismatch(fingerprint, admitted.SchemaFingerprint)));
}

public sealed class SyncEndpoint(SyncRuntime runtime) : SyncService.SyncServiceBase {
    public override Task<PullResponse> Pull(PullRequest request, ServerCallContext context) =>
        Answer(context, Segment(request));

    public override Task<PushResponse> Push(PushRequest request, ServerCallContext context) =>
        Answer(context, Landed(request));

    public override Task<TransferSetResponse> TransferSet(TransferSetRequest request, ServerCallContext context) =>
        Answer(context, Manifest(request));

    public override async Task Checkout(
        CheckoutRequest request, IServerStreamWriter<CheckoutResponse> responseStream, ServerCallContext context) {
        foreach (ReadOnlyMemory<byte> frame in await Answer(context, Subtree(request)).ConfigureAwait(false)) {
            await responseStream.WriteAsync(new CheckoutResponse {
                SchemaFingerprint = runtime.SchemaFingerprint,
                Frame = ByteString.CopyFrom(frame.Span),
            }).ConfigureAwait(false);
        }
    }

    private async Task<T> Answer<T>(ServerCallContext context, IO<Fin<T>> answered) =>
        (await answered.RunAsync(EnvIO.New(token: context.CancellationToken)).ConfigureAwait(false))
            .Match(Succ: static held => held, Fail: error => throw FaultWire.Raise(error, runtime.Context()));

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

    private IO<Fin<TransferSetResponse>> Manifest(TransferSetRequest request) =>
        IO.lift<Fin<(UInt128 Root, Seq<UInt128> Held)>>(() => Admitted(request).Bind(stated =>
                (ContentHash.Admit(stated.Root.Span).ToValidation(),
                 SyncWireMap.Keys(stated.Held).ToValidation())
                    .Apply(static (root, held) => (root, held)).As().ToFin()))
            .Bind(stated => stated.Match(
                Succ: asked => runtime.Fetch(asked.Root).Map(fetched => fetched.Map(entry => new TransferSetResponse {
                    Missing = { OpLog.TransferSet(entry, asked.Held.Contains).Map(static key => ContentHash.Wire()) },
                })),
                Fail: static error => IO.pure(Fin.Fail<TransferSetResponse>(error))));

    private IO<Fin<Seq<ReadOnlyMemory<byte>>>> Subtree(CheckoutRequest request) =>
        IO.lift<Fin<UInt128>>(() => Admitted(request).Bind(stated => ContentHash.Admit(stated.Root.Span)))
            .Bind(root => root.Match(
                Succ: key => runtime.Fetch().Bind(fetched => fetched.Match(
                    Succ: entry => entry.Closure.Traverse(runtime.Fetch).As().Map(rows =>
                        rows.Traverse(static row => row.ToValidation()).As().ToFin()
                            .Bind(descendants => OpLogWire.Encode(Seq(entry) + descendants))),
                    Fail: static error => IO.pure(Fin.Fail<Seq<ReadOnlyMemory<byte>>>(error)))),
                Fail: static error => IO.pure(Fin.Fail<Seq<ReadOnlyMemory<byte>>>(error))));

    private static Fin<T> Admitted<T>(T request) where T : IMessage =>
        WireAdmission.Admit(request, WireBoundary.InboundPayload);

    private int Bounded(uint stated) => (int)Math.Min(stated, (uint)runtime.PageCap);

    private Fin<SyncCursor> Position(SyncCursorWire? wire) =>
        SyncWireMap.Admit(wire).Bind(cursor =>
            cursor.OriginStoreId == runtime.StoreId || cursor.OriginStoreId == Guid.Empty
                ? Fin.Succ(cursor)
                : Fin.Fail<SyncCursor>(new SyncFault.ReplicationFaulted(
                    "sync-pull-origin", Error.New($"<foreign-origin:{cursor.OriginStoreId:N}>"))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SyncPump {
    public static IO<SyncOutcome> Run(SyncSession session, SyncTransport transport) =>
        transport.Switch(
            state: session,
            httpDelta: static (s, row) => Exchange(s, row),
            speckleLikeDiff: static (s, row) => Offer(s, row),
            subtreeCheckout: static (s, row) => Materialize(s, row));

    public static IO<SyncOutcome> Checkout(SyncSession source, SyncSession target, UInt128 root) =>
        source.Fetch(root).Bind(entry =>
            SyncMerge.Apply(target, Seq(entry)).Map(outcome => outcome with { Pushed = OpLog.TransferSet(entry, target.Holds).Count }));

    static IO<SyncOutcome> Exchange(SyncSession s, SyncTransport.HttpDelta row) =>
        from pulled in row.Flow.Pulls
            ? s.Pull(row.Peer, s.Cursor).Bind(IO.lift).Bind(segment => segment.SchemaFingerprint == s.SchemaFingerprint
                ? IO.lift(OpLogWire.Decode(segment.Frames)).Bind(entries => SyncMerge.Apply(s, entries))
                    .Map(outcome => outcome with { Cursor = segment.Cursor })
                : IO.fail<SyncOutcome>(new SyncFault.SchemaMismatch(s.SchemaFingerprint, segment.SchemaFingerprint)))
            : IO.pure(Idle(s))
        let pending = s.Pending(s.Acked)
        from outcome in row.Flow.Pushes
            ? IO.lift(OpLogWire.Encode(pending)).Bind(frames => s.Push(row.Peer, frames).Bind(IO.lift))
                .Map(acked => pulled with { Pushed = pending.Count, Acked = acked })
            : IO.pure(pulled)
        select outcome;

    static IO<SyncOutcome> Materialize(SyncSession s, SyncTransport.SubtreeCheckout row) =>
        from frames in s.Checkout(row.Peer, row.Root).Bind(IO.lift)
        from entries in IO.lift(OpLogWire.Decode(frames))
        from outcome in SyncMerge.Apply(s, entries)
        from missing in s.Missing(row.Peer, row.Root, Holdings(s, entries)).Bind(IO.lift)
        select outcome with { Pushed = missing.Count };

    static Seq<UInt128> Holdings(SyncSession s, Seq<OpLogEntry> entries) =>
        toSeq(entries.Fold(Seq<UInt128>(), static (set, entry) => set + entry.Closure.Add(entry.ContentKey))
            .Distinct().Filter(s.Holds).OrderBy(static key => key));

    static IO<SyncOutcome> Offer(SyncSession s, SyncTransport.SpeckleLikeDiff row) =>
        from pending in IO.pure(s.Pending(s.Acked))
        from held in s.HasObjects(row.Peer, toSeq(pending.Fold(Seq<UInt128>(), static (set, entry) => set + OpLog.TransferSet(entry, static _ => false)).Distinct()))
        let missing = pending.Filter(entry => !held.Contains(entry.ContentKey))
        from sent in s.SpeckleSend(row.Peer, missing)
        from outcome in missing.Head.Map(h => h.ContentKey) is { IsSome: true, Case: UInt128 root } && root != sent.RootContentKey
            ? IO.fail<SyncOutcome>(new SyncFault.SpeckleMarshal(row.Peer, $"root-key-drift:{root}!={sent.RootContentKey}:refs={sent.ConvertedReferences}"))
            : IO.pure(Idle(s) with {
                Pushed = missing.Count,
                Acked = pending.Last.Map(last => s.Acked with { Sequence = last.Sequence, Physical = last.Physical, Logical = last.Logical }).IfNone(s.Acked),
            })
        select outcome;

    static SyncOutcome Idle(SyncSession s) =>
        new(0L, 0L, 0L, 0L, 0L, 0L, s.QueueDepth(), Seq<Conflict>(), s.Cursor, s.Acked);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                           | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------------------ | :--------------------------------------------------------- |
|  [01]   | cluster replication | Marten daemon `HotCold`                           | this axis is the cross-store/offline lane only             |
|  [02]   | graph checkout      | fetch+apply root; `TransferSet` is the manifest   | closure set via the blob store, not an op-log `Fetch`      |
|  [03]   | Speckle marshal     | DI-resolved instance `IOperations`                | outside-Rhino; `rootObjId` → `ContentKey`; drift faults    |
|  [04]   | http delta          | `SyncServiceClient` through AppHost `OutboundHop` | database excluded from the hop law                         |
|  [05]   | exchange direction  | `CapabilitySet<FlowCapability>` under its law     | the empty corner is barred, never merely unwritten         |
|  [06]   | reply admission     | AppHost `WireAdmission.Admit`                     | one rule graph; this package holds no `ParseGuard`         |
|  [07]   | handler refusal     | AppHost `FaultWire.Raise(fault, FaultContext)`    | the one throw; typed fault seals on the typed result first |
|  [08]   | server page bound   | `min(PullRequest.page_size, SyncRuntime.PageCap)` | a peer states a page; the store caps what it reads         |

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
- Entry: `AwarenessLane(spec, dropped)` opens the declared `DropOldest` row through the AppHost `DrainSurface`, so the lane carries its own drop tally and never hand-rolls channel options, and a `DropOldest` row opening without that tally fails on `Fin`; `Beat(actor, kind, payload, seq, frame, session)` is the one polymorphic awareness constructor, the kind discriminating payload meaning so per-signal factories are the deleted form; `Present(actor, state, ttl, frame)` mints the durable ephemeral presence row and `Live(rows, now)` is the per-actor add-wins-LWW sweep; `Checkout(query, resolve, fetch, cursor, frame)` materializes a subgraph working set.
- Auto: presence rows expire at stamp offset by `Ttl` and sweep on the heartbeat cadence. Awareness beats ride a SEPARATE lossy lane — cursor moves, selection halos, and camera frusta beat at high cadence and never touch the durable store, while `AwarenessKind` discriminates them and `Supersedes` lets a slow reader discard a reordered beat by per-actor lamport. Dropped beats count through the drain surface's own callback into the loss atom. Working-set checkout resolves a query into a content-key set then fetches only those entries, so a peer materializes one subgraph rather than the whole graph.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, System.Threading.Channels, BCL inbox.
- Growth: a new awareness signal is one `AwarenessKind` row; a new checkout dimension is one field on `ReplicationQuery`; zero new surface — a per-signal awareness factory, a presence row written to the DURABLE event stream, or a second lossy lane is the deleted form.
- Boundary: presence is one ephemeral `Presence`-lane row (`durable: false`, `FirstWriter` stance) that `Present` mints and `Live` sweeps, never a durable event-stream write and never a transport. Awareness rides the fire-and-forget channel that never appends a durable entry, while the converging `Version/commits#CRDT_ALGEBRA` `EphemeralMap` is the durable self-expiring map a late-joining peer reconstructs — two distinct presence forms the one `Awareness` surface owns together, so the durable projection's liveness horizon agrees with the convergent map's. Working-set checkout subscribes its op-stream to changes touching its checked-out keys alone.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AwarenessKind {
    public static readonly AwarenessKind Cursor = new("cursor");
    public static readonly AwarenessKind Selection = new("selection");
    public static readonly AwarenessKind Camera = new("camera");
    public static readonly AwarenessKind Focus = new("focus");
    public static readonly AwarenessKind Follow = new("follow");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AwarenessBeat(string Actor, AwarenessKind Kind, ReadOnlyMemory<byte> Payload, ulong Seq, Instant At, Option<string> Session) {
    public bool Supersedes(AwarenessBeat prior) => Actor == prior.Actor && Kind == prior.Kind && Seq > prior.Seq;
}

public readonly record struct PresenceRow(string Actor, ReadOnlyMemory<byte> State, Instant At, Duration Ttl) {
    public bool Live(Instant now) => now - At < Ttl;
}

public readonly record struct ReplicationQuery(Option<string> Region, Option<string> Layer, Option<string> View, Option<string> Kind, int ClosureDepth);

public readonly record struct WorkingSet(Seq<UInt128> Keys, Seq<OpLogEntry> Entries, SyncCursor Cursor, Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Awareness {
    public static Fin<DrainQueue<AwarenessBeat>> AwarenessLane(DrainSpec spec, Atom<Seq<AwarenessBeat>> dropped) =>
        spec.Open<AwarenessBeat>(Some<Action<AwarenessBeat>>(beat => dropped.Swap(seq => seq.Add(beat))));

    public static AwarenessBeat Beat(string actor, AwarenessKind kind, ReadOnlyMemory<byte> payload, ulong seq, ProjectionContext frame, Option<string> session = default) =>
        new(actor, kind, payload, seq, frame.Now(), session);

    public static PresenceRow Present(string actor, ReadOnlyMemory<byte> state, Duration ttl, ProjectionContext frame) =>
        new(actor, state, frame.Now(), ttl);

    public static Seq<PresenceRow> Live(Seq<PresenceRow> rows, Instant now) =>
        toSeq(rows.Filter(row => row.Live(now)).GroupBy(static row => row.Actor).Select(static g => g.MaxBy(static row => row.At)));

    public static IO<WorkingSet> Checkout(ReplicationQuery query, Func<ReplicationQuery, IO<Seq<UInt128>>> resolve, Func<Seq<UInt128>, IO<Seq<OpLogEntry>>> fetch, SyncCursor cursor, ProjectionContext frame) =>
        from keys in resolve(query)
        from entries in fetch(keys)
        select new WorkingSet(keys, entries, cursor, frame.Now());
}
```

| [INDEX] | [POLICY]             | [VALUE]                                       | [BINDING]                                                    |
| :-----: | :------------------- | :-------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | lossy awareness      | `DropOldest` `DrainSpec` lane, `onDrop` tally | never a durable changefeed row; distinct from `EphemeralMap` |
|  [02]   | presence ttl         | stamp + `Ttl`, heartbeat sweep                | one ephemeral row, never a transport                         |
|  [03]   | working-set checkout | `ReplicationQuery` → key set                  | one subgraph, never the whole graph                          |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
