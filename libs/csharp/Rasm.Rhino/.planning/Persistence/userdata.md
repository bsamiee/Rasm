# [RASM_RHINO_PERSISTENCE_USERDATA]

`ArchiveIo` owns schema and integrity framing for every `ArchiveMap` crossing. `TypedUserData<TSelf>` seals the Rhino override lifecycle, while `CustodyOperation` closes roster, transfer, and shared-dictionary behavior behind one entrypoint. Every fallible host crossing rides the kernel `Op.Catch` funnel onto typed faults.

## [01]-[ARCHIVE_FRAME]

`ArchiveSchema` owns the chunk typecode and readable versions. `ArchiveIo` brackets each payload with the matching host chunk, restores CRC policy, and closes the chunk before detached integrity evidence escapes.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [ARCHIVE_FRAME] ------------------------------------------------------------------------

[ComplexValueObject]
public readonly partial record struct ArchiveVersion(int Major, int Minor) {
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int major,
        ref int minor) =>
        validationError = major < 0 || minor < 0
            ? new ValidationError("Archive version components must be non-negative.")
            : null;
}

[ComplexValueObject]
public sealed partial record ArchiveSchema(
    uint TypeCode,
    ArchiveVersion Current,
    LanguageExt.HashSet<ArchiveVersion> Readable) {
    public bool Reads(ArchiveVersion observed) => observed == Current || Readable.Contains(observed);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref uint typeCode,
        ref ArchiveVersion current,
        ref LanguageExt.HashSet<ArchiveVersion> readable) =>
        validationError = typeCode == 0u
            ? new ValidationError("Archive chunk typecode is zero.")
            : readable.Contains(current)
                ? new ValidationError("Readable archive versions must not duplicate the current version.")
                : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveIntegrity {
    private ArchiveIntegrity() { }

    public sealed record WrittenCase(
        uint TypeCode,
        int Archive3dmVersion,
        ArchiveVersion Schema,
        bool ErrorObserved) : ArchiveIntegrity;

    public sealed record ReadCase(
        uint TypeCode,
        int Archive3dmVersion,
        ArchiveVersion Schema,
        bool ChecksumVerified,
        bool ErrorObserved) : ArchiveIntegrity;
}

public sealed record ArchiveEnvelope(
    ArchiveMap Payload,
    ArchiveIntegrity.ReadCase Integrity);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveExchange {
    private ArchiveExchange() { }

    public sealed record WriteCase(
        BinaryArchiveWriter Writer,
        ArchiveSchema Schema,
        ArchiveMap Payload) : ArchiveExchange;

    public sealed record ReadCase(
        BinaryArchiveReader Reader,
        ArchiveSchema Schema) : ArchiveExchange;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveExchangeResult {
    private ArchiveExchangeResult() { }

    public sealed record WrittenCase(ArchiveIntegrity.WrittenCase Integrity) : ArchiveExchangeResult;
    public sealed record ReadCase(ArchiveEnvelope Envelope) : ArchiveExchangeResult;
}

public static class ArchiveIo {
    public static Fin<ArchiveExchangeResult> Cross(ArchiveExchange exchange, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(exchange)
            .Bind(active => Admit(active, op))
            .Bind(active => op.Catch(() => active.Switch<Op, Fin<ArchiveExchangeResult>>(
                state: op,
                writeCase: static (op, write) => Write(write, op),
                readCase: static (op, read) => Read(read, op))));
    }

    private static Fin<ArchiveExchange> Admit(ArchiveExchange exchange, Op op) => exchange.Switch<Op, Fin<ArchiveExchange>>(
        state: op,
        writeCase: static (op, request) =>
            from writer in op.Need(request.Writer)
            from schema in op.Need(request.Schema)
            from payload in op.Need(request.Payload)
            select (ArchiveExchange)new ArchiveExchange.WriteCase(writer, schema, payload),
        readCase: static (op, request) =>
            from reader in op.Need(request.Reader)
            from schema in op.Need(request.Schema)
            select (ArchiveExchange)new ArchiveExchange.ReadCase(reader, schema));

    private static Fin<ArchiveExchangeResult> Write(ArchiveExchange.WriteCase request, Op op) =>
        from native in request.Payload.Mint(op)
        from integrity in op.Catch(() => {
            bool opened = request.Writer.BeginWrite3dmChunk(
                request.Schema.TypeCode,
                request.Schema.Current.Major,
                request.Schema.Current.Minor);
            if (!opened) {
                return Fin.Fail<ArchiveIntegrity.WrittenCase>(
                    op.InvalidResult(detail: "Binary archive writer refused the chunk frame."));
            }

            bool priorCrc = request.Writer.EnableCRCCalculation(true);
            bool closed = false;
            try {
                request.Writer.WriteDictionary(native);
                request.Writer.WriteEmptyCheckSum();
            } finally {
                try {
                    closed = request.Writer.EndWrite3dmChunk();
                } finally {
                    request.Writer.EnableCRCCalculation(priorCrc);
                }
            }

            return closed
                ? Fin.Succ(value: new ArchiveIntegrity.WrittenCase(
                    request.Schema.TypeCode,
                    request.Writer.Archive3dmVersion,
                    request.Schema.Current,
                    request.Writer.WriteErrorOccured))
                : Fin.Fail<ArchiveIntegrity.WrittenCase>(
                    op.InvalidResult(detail: "Binary archive writer did not close the chunk frame."));
        })
        from accepted in integrity.ErrorObserved
            ? Fin.Fail<ArchiveExchangeResult>(error: op.InvalidResult(detail: "Binary archive writer reported an integrity fault."))
            : Fin.Succ<ArchiveExchangeResult>(value: new ArchiveExchangeResult.WrittenCase(integrity))
        select accepted;

    private static Fin<ArchiveExchangeResult> Read(ArchiveExchange.ReadCase request, Op op) =>
        from captured in op.Catch(() => {
            bool opened = request.Reader.BeginRead3dmChunk(
                request.Schema.TypeCode,
                out int major,
                out int minor);
            if (!opened) {
                return Fin.Fail<(ArchivableDictionary Native, ArchiveIntegrity.ReadCase Integrity)>(
                    op.InvalidResult(detail: "Binary archive reader refused the chunk frame."));
            }

            ArchiveVersion frame = ArchiveVersion.Create(major, minor);
            if (!request.Schema.Reads(frame)) {
                request.Reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true);
                return Fin.Fail<(ArchivableDictionary Native, ArchiveIntegrity.ReadCase Integrity)>(
                    op.InvalidResult(detail: $"Archive schema '{frame.Major}.{frame.Minor}' is not readable."));
            }

            bool priorCrc = request.Reader.EnableCRCCalculation(true);
            bool closed = false;
            ArchivableDictionary native;
            bool checksum;
            try {
                native = request.Reader.ReadDictionary();
                checksum = request.Reader.ReadCheckSum();
            } finally {
                try {
                    closed = request.Reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: false);
                } finally {
                    request.Reader.EnableCRCCalculation(priorCrc);
                }
            }

            bool errorObserved = request.Reader.ReadErrorOccured;

            return closed
                ? Fin.Succ(value: (
                    native,
                    new ArchiveIntegrity.ReadCase(
                        request.Schema.TypeCode,
                        request.Reader.Archive3dmVersion,
                        frame,
                        checksum,
                        errorObserved)))
                : Fin.Fail<(ArchivableDictionary Native, ArchiveIntegrity.ReadCase Integrity)>(
                    op.InvalidResult(detail: "Binary archive reader did not close the chunk frame."));
        })
        from _integrity in guard(
            captured.Integrity.ChecksumVerified && !captured.Integrity.ErrorObserved,
            op.InvalidResult(detail: "Binary archive checksum or reader state is invalid.")).ToFin()
        from payload in ArchiveMap.Detach(captured.Native, op)
        select (ArchiveExchangeResult)new ArchiveExchangeResult.ReadCase(new ArchiveEnvelope(payload, captured.Integrity));
}
```

## [02]-[TYPED_PARTICIPATION]

`TypedUserData<TSelf>` keeps live archive handles inside sealed overrides. Derived participants provide schema upgrades, initial payloads, payload-transform policy, and a mandatory failure sink; duplicate custody accepts only the identical closed participant type.

Rhino's `bool` and `void` override contracts form the platform-forced statement seam. Every override mints its own `Op.Of()`, collapses its rail only after archive, duplicate, or transform work finishes, and lands every failure through one sink. Both archive crossings gate on the `[ClassId]` pin, so a read-only participant proves its resolution key too. A failed payload transform poisons the detached rail after Rhino's required base call, so no stale payload can write or re-enter transformation, and the payload cell is an `Atom` because the five writing callbacks share no host lock.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

public abstract class TypedUserData<TSelf> : UserData
    where TSelf : TypedUserData<TSelf> {
    // Five host callbacks write this cell — `Write`, `Read`, `OnDuplicate`, `OnTransform`, and the caller-facing
    // `Replace` — and Rhino serializes none of them against each other. The Atom is the cell's own linearization
    // point, so a stored payload and a poisoned rail can never interleave into a torn read.
    private readonly Atom<Fin<Option<ArchiveMap>>> held = Atom(Fin.Succ<Option<ArchiveMap>>(None));

    protected abstract ArchiveSchema Schema { get; }
    protected abstract Fin<ArchiveMap> Initial { get; }
    protected abstract Fin<ArchiveMap> Upgrade(ArchiveEnvelope envelope);
    protected virtual Fin<ArchiveMap> TransformPayload(ArchiveMap payload, Transform transform) => Fin.Succ(payload);
    protected abstract void Report(Error error);

    // Host truth: `UserData.Description` is virtual and defaults to the literal "RhinoCommon UserData", so every
    // unoverridden participant publishes the framework's own string as its census identity.
    public sealed override string Description => typeof(TSelf).Name;

    public sealed override bool ShouldWrite => held.Value.Match(
        Succ: state => state.Exists(static payload => payload.Entries.Count > 0),
        Fail: static _ => false);

    public Fin<ArchiveMap> Snapshot() => held.Value.Bind(state => state.Match(
        Succ,
        () => Initial.Map(Store)));

    public Fin<Unit> Replace(ArchiveMap payload, Op? key = null) =>
        key.OrDefault().Need(payload).Map(Store).Map(static _ => unit);

    // Host truth: `ClassIdAttribute(string) : { Id : Guid }` pins a stable resolution key onto a `UserData` subclass so an
    // archive written under a prior class name keeps resolving it. The schema's whole thesis is archive-version tolerance,
    // which a class RENAME defeats outright, so the pin proves on BOTH crossings: a read-only participant gates on the same
    // key it will be resolved by, where a write-only proof would leave it unproven for the whole of its life.
    private static Fin<Guid> Pinned(Op op) =>
        typeof(TSelf).GetCustomAttributes(typeof(ClassIdAttribute), inherit: false) is [ClassIdAttribute pin]
        && pin.Id != Guid.Empty
            ? Fin.Succ(value: pin.Id)
            : Fin.Fail<Guid>(error: op.InvalidResult(detail: $"'{typeof(TSelf).Name}' carries no [ClassId] pin."));

    protected sealed override bool Write(BinaryArchiveWriter archive) {
        Op op = Op.Of();
        return op.Catch(() => Pinned(op)
            .Bind(_ => Snapshot())
            .Bind(payload => ArchiveIo.Cross(new ArchiveExchange.WriteCase(archive, Schema, payload), op))
            .Map(static _ => unit))
            .Match(
                Succ: static _ => true,
                Fail: error => (Poison(error, op), false).Item2);
    }

    protected sealed override bool Read(BinaryArchiveReader archive) {
        Op op = Op.Of();
        return op.Catch(() => Pinned(op)
            .Bind(_ => ArchiveIo.Cross(new ArchiveExchange.ReadCase(archive, Schema), op))
            .Bind(result => result.Switch<Fin<ArchiveMap>>(
                writtenCase: _ => Fin.Fail<ArchiveMap>(error: op.InvalidResult(detail: "Archive read returned a write receipt.")),
                readCase: read => Upgrade(read.Envelope)))
            .Map(Store))
            .Match(
                Succ: static _ => true,
                Fail: error => (Poison(error, op), false).Item2);
    }

    protected sealed override void OnDuplicate(UserData source) {
        Op op = Op.Of();
        op.Catch(() => source is TSelf typed
            ? typed.Snapshot().Map(Store).Map(static _ => unit)
            : Fin.Fail<Unit>(op.Unsupported(geometryType: source.GetType(), outputType: typeof(TSelf))))
            .Match(Succ: static _ => unit, Fail: error => Poison(error, op));
    }

    protected sealed override void OnTransform(Transform transform) {
        Op op = Op.Of();
        op.Catch(() => {
            base.OnTransform(transform);
            return Snapshot()
                .Bind(payload => TransformPayload(payload, transform))
                .Map(Store);
        }).Match(
            Succ: static _ => unit,
            Fail: error => Poison(error, op));
    }

    private ArchiveMap Store(ArchiveMap payload) {
        held.Swap(_ => Fin.Succ(Some(payload)));
        return payload;
    }

    private Unit Poison(Error error, Op op) {
        held.Swap(_ => Fin.Fail<Option<ArchiveMap>>(error));
        return Reported(error, op);
    }

    private Unit Reported(Error error, Op op) => op.Catch(() => Report(error))
        .Match(Succ: static _ => unit, Fail: static _ => unit);
}
```

## [03]-[CUSTODY_ALGEBRA]

`CustodyOperation` admits every nested operand through one total fold before retaining query and mutation identity through execution. Shared-dictionary replacement captures the prior map and restores it if any typed write fails, avoiding `ReplaceContentsWith` and its exact-runtime-type reflection fault.

Rhino's mutable roster, transfer, and shared-dictionary calls form the platform-forced statement seam. One `Mutated` fold owns the before/after census around every roster mutation, so attach, remove, purge, copy, and move differ only in their commit arrow.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

[SmartEnum<string>]
public sealed partial class DisposalPolicy {
    public static readonly DisposalPolicy Detach = new("detach", false);
    public static readonly DisposalPolicy Dispose = new("dispose", true);
    public bool Releases { get; }
}

[SmartEnum<string>]
public sealed partial class TransferPlacement {
    public static readonly TransferPlacement Replace = new("replace", false);
    public static readonly TransferPlacement Append = new("append", true);
    public bool Appends { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodyOperation {
    private CustodyOperation() { }

    public sealed record CensusCase(CommonObject Target) : CustodyOperation;
    public sealed record ContainsCase(CommonObject Target, Guid UserDataId) : CustodyOperation;
    public sealed record DescribeCase(CommonObject Target, Type UserDataType) : CustodyOperation;
    public sealed record AttachCase(CommonObject Target, UserData Value) : CustodyOperation;
    public sealed record RemoveCase(CommonObject Target, UserData Value, DisposalPolicy Disposal) : CustodyOperation;
    public sealed record PurgeCase(CommonObject Target) : CustodyOperation;
    public sealed record CopyCase(CommonObject Source, CommonObject Destination) : CustodyOperation;
    public sealed record MoveCase(
        CommonObject Source,
        CommonObject Destination,
        TransferPlacement Placement) : CustodyOperation;
    public sealed record SharedReadCase(CommonObject Target) : CustodyOperation;
    public sealed record SharedReplaceCase(CommonObject Target, ArchiveMap Payload) : CustodyOperation;
    public sealed record SharedMergeCase(CommonObject Target, ArchiveMap Payload, ArchiveMerge Merge) : CustodyOperation;
}

public sealed record UserDataFact(
    string RuntimeType,
    string Description,
    bool ShouldWrite,
    Transform Transform);

public sealed record CustodyReceipt(
    CustodyKind Operation,
    int BeforeCount,
    Option<int> AfterCount,
    bool SharedDictionaryCreated,
    Option<Guid> TransferId,
    CustodySettlement Settlement);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodySettlement {
    private CustodySettlement() { }

    public sealed record CommittedCase : CustodySettlement;
    public sealed record PartialCase(Error Fault) : CustodySettlement;
}

[SmartEnum<string>]
public sealed partial class CustodyKind {
    public static readonly CustodyKind Attach = new("attach");
    public static readonly CustodyKind Remove = new("remove");
    public static readonly CustodyKind Purge = new("purge");
    public static readonly CustodyKind Copy = new("copy");
    public static readonly CustodyKind Move = new("move");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodyAnswer {
    private CustodyAnswer() { }

    public sealed record CensusCase(Seq<UserDataFact> Values) : CustodyAnswer;
    public sealed record PresenceCase(bool Present) : CustodyAnswer;
    public sealed record DescriptionCase(Option<UserDataFact> Value) : CustodyAnswer;
    public sealed record SharedCase(ArchiveMap Payload, bool Created) : CustodyAnswer;
    public sealed record SharedMutationCase(
        ArchiveMap Prior,
        ArchiveMap Current,
        Seq<ArchiveChange> Changes,
        bool Created) : CustodyAnswer;
    public sealed record MutationCase(CustodyReceipt Receipt) : CustodyAnswer;
}

public static class Custody {
    public static Fin<CustodyAnswer> Commit(CustodyOperation operation, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(operation)
            .Bind(active => Admit(active, op))
            .Bind(active => active.Switch<Op, Fin<CustodyAnswer>>(
                state: op,
                censusCase: static (op, census) => op.Catch(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.CensusCase(
                    census.Target.UserData.Map(Describe).ToSeq()))),
                containsCase: static (op, contains) => op.Catch(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.PresenceCase(
                    contains.Target.UserData.Contains(contains.UserDataId)))),
                describeCase: static (op, describe) => op.Catch(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.DescriptionCase(
                    Optional(describe.Target.UserData.Find(describe.UserDataType)).Map(Describe)))),
                attachCase: static (op, attach) => Mutated(
                    CustodyKind.Attach,
                    () => attach.Target.UserData.Count,
                    () => op.Catch(() => op.Confirm(success: attach.Target.UserData.Add(attach.Value)))
                        .Map(static _ => (Option<Guid>.None, (CustodySettlement)new CustodySettlement.CommittedCase())),
                    op),
                removeCase: static (op, remove) => Mutated(
                    CustodyKind.Remove,
                    () => remove.Target.UserData.Count,
                    () => op.Catch(() => op.Confirm(success: remove.Target.UserData.Remove(remove.Value)))
                        .Bind(_ => Settle(
                            remove.Disposal.Releases ? op.Catch(remove.Value.Dispose) : Fin.Succ(value: unit),
                            None)),
                    op),
                purgeCase: static (op, purge) => Mutated(
                    CustodyKind.Purge,
                    () => purge.Target.UserData.Count,
                    // Decompile-proven: `UserDataList.Purge` is `ON_UserData_PurgeUserData` on the parent, and the native
                    // delete fires the `UserData.OnDelete` callback — it zeroes `m_native_pointer`, suppresses the
                    // finalizer, and drops the runtime-list entry. `UserData.Dispose(true)` then early-returns on the zero
                    // pointer, so a capture-then-dispose leg is dead work and a `DisposalPolicy` here would name a choice
                    // the host does not offer. Purge releases every managed participant itself; `Remove` does not, because
                    // `ON_Object_DetachUserData` hands custody back to the caller.
                    () => op.Catch(() => purge.Target.UserData.Purge())
                        .Map(static _ => (Option<Guid>.None, (CustodySettlement)new CustodySettlement.CommittedCase())),
                    op),
                copyCase: static (op, copy) => Mutated(
                    CustodyKind.Copy,
                    () => copy.Destination.UserData.Count,
                    () => op.Catch(() => UserData.Copy(copy.Source, copy.Destination))
                        .Map(static _ => (Option<Guid>.None, (CustodySettlement)new CustodySettlement.CommittedCase())),
                    op),
                moveCase: static (op, move) => Mutated(
                    CustodyKind.Move,
                    () => move.Destination.UserData.Count,
                    () => op.Catch(() => Fin.Succ(value: UserData.MoveUserDataFrom(move.Source)))
                        .Bind(id => id == Guid.Empty
                            ? Fin.Fail<(Option<Guid>, CustodySettlement)>(
                                error: op.InvalidResult(detail: "User-data move found no transferable custody."))
                            : Settle(
                                op.Catch(() => UserData.MoveUserDataTo(move.Destination, id, move.Placement.Appends)),
                                Some(id))),
                    op),
                sharedReadCase: static (op, read) => Open(read.Target, op)
                    .Bind(opened => ArchiveMap.Detach(opened.Dictionary, op)
                        .Map<CustodyAnswer>(map => new CustodyAnswer.SharedCase(map, opened.Created))),
                sharedReplaceCase: static (op, replace) => Open(replace.Target, op)
                    .Bind(opened => ReplaceShared(opened, replace.Payload, op)),
                sharedMergeCase: static (op, merge) => Open(merge.Target, op)
                    .Bind(opened => ArchiveMap.Detach(opened.Dictionary, op)
                        .Bind(current => current.Merge(merge.Payload, merge.Merge, op)
                            .Bind(payload => ReplaceShared(opened, payload, op))))));
    }

    private static Fin<CustodyOperation> Admit(CustodyOperation operation, Op op) =>
        operation.Switch<Op, Fin<CustodyOperation>>(
            state: op,
            censusCase: static (op, census) => op.Need(census.Target).Map(_ => (CustodyOperation)census),
            containsCase: static (op, contains) =>
                from _target in op.Need(contains.Target)
                from _id in guard(contains.UserDataId != Guid.Empty, op.InvalidInput()).ToFin()
                select (CustodyOperation)contains,
            describeCase: static (op, describe) =>
                from _target in op.Need(describe.Target)
                from type in op.Need(describe.UserDataType)
                from _type in guard(typeof(UserData).IsAssignableFrom(type), op.InvalidInput()).ToFin()
                select (CustodyOperation)describe,
            attachCase: static (op, attach) =>
                from _target in op.Need(attach.Target)
                from _value in AdmitAttach(attach.Value, op)
                select (CustodyOperation)attach,
            removeCase: static (op, remove) =>
                from _target in op.Need(remove.Target)
                from _value in op.Need(remove.Value)
                from _disposal in op.Need(remove.Disposal)
                select (CustodyOperation)remove,
            purgeCase: static (op, purge) => op.Need(purge.Target).Map(_ => (CustodyOperation)purge),
            copyCase: static (op, copy) =>
                from _source in op.Need(copy.Source)
                from _destination in op.Need(copy.Destination)
                select (CustodyOperation)copy,
            moveCase: static (op, move) =>
                from _source in op.Need(move.Source)
                from _destination in op.Need(move.Destination)
                from _placement in op.Need(move.Placement)
                select (CustodyOperation)move,
            sharedReadCase: static (op, read) => op.Need(read.Target).Map(_ => (CustodyOperation)read),
            sharedReplaceCase: static (op, replace) =>
                from _target in op.Need(replace.Target)
                from _payload in op.Need(replace.Payload)
                select (CustodyOperation)replace,
            sharedMergeCase: static (op, merge) =>
                from _target in op.Need(merge.Target)
                from _payload in op.Need(merge.Payload)
                from _policy in op.Need(merge.Merge)
                select (CustodyOperation)merge);

    // The host gate is accessibility, not nesting: `Type.IsPublic` is FALSE for a nested public type, so testing it refuses a
    // participant the host itself accepts. `IsVisible` is the whole gate — public and publicly enclosed at every level.
    private static Fin<UserData> AdmitAttach(UserData value, Op op) => op.Need(value)
        .Bind(active => active.GetType() is { IsClass: true, IsVisible: true } type
            && type.GetConstructor(Type.EmptyTypes) is not null
                ? Fin.Succ(value: active)
                : Fin.Fail<UserData>(error: op.InvalidInput()));

    private static Fin<CustodyAnswer> Mutated(
        CustodyKind kind,
        Func<int> count,
        Func<Fin<(Option<Guid> TransferId, CustodySettlement Settlement)>> commit,
        Op op) =>
        from before in op.Catch(() => Fin.Succ(value: count()))
        from committed in commit()
        select op.Catch(() => Fin.Succ(value: count())).Match(
            Succ: after => (CustodyAnswer)new CustodyAnswer.MutationCase(new CustodyReceipt(
                kind,
                before,
                Some(after),
                false,
                committed.TransferId,
                committed.Settlement)),
            Fail: fault => new CustodyAnswer.MutationCase(new CustodyReceipt(
                kind,
                before,
                None,
                false,
                committed.TransferId,
                Partial(committed.Settlement, fault))));

    private static Fin<(Option<Guid> TransferId, CustodySettlement Settlement)> Settle(
        Fin<Unit> tail,
        Option<Guid> transferId) => tail.Match(
        Succ: _ => Fin.Succ(value: (transferId, (CustodySettlement)new CustodySettlement.CommittedCase())),
        Fail: fault => Fin.Succ(value: (transferId, (CustodySettlement)new CustodySettlement.PartialCase(fault))));

    private static CustodySettlement Partial(CustodySettlement settlement, Error fault) => settlement.Switch<CustodySettlement>(
        committedCase: static (fault, _) => new CustodySettlement.PartialCase(fault),
        partialCase: static (fault, partial) => new CustodySettlement.PartialCase(partial.Fault + fault),
        state: fault);

    private static Fin<CustodyAnswer> ReplaceShared(
        (CommonObject Target, ArchivableDictionary Dictionary, bool Created) opened,
        ArchiveMap payload,
        Op op) =>
        from prior in ArchiveMap.Detach(opened.Dictionary, op)
        from _schema in prior.Diff(payload, op).Map(static _ => unit)
        from answer in (
            from _clear in op.Catch(opened.Dictionary.Clear)
            from _write in payload.WriteTo(opened.Dictionary, op)
            from current in ArchiveMap.Detach(opened.Dictionary, op)
            from changes in prior.Diff(current, op)
            from _proof in guard(
                current.SameContent(payload),
                op.InvalidResult(detail: "Shared user dictionary postcondition failed.")).ToFin()
            select (CustodyAnswer)new CustodyAnswer.SharedMutationCase(
                prior,
                current,
                changes,
                opened.Created))
            .BindFail(error => Rollback<CustodyAnswer>(opened, prior, error, op))
        select answer;

    private static Fin<T> Rollback<T>(
        (CommonObject Target, ArchivableDictionary Dictionary, bool Created) opened,
        ArchiveMap prior,
        Error primary,
        Op op) =>
        RestoreShared(opened, prior, op)
            .Match(
                Succ: _ => Fin.Fail<T>(error: primary),
                Fail: rollback => Fin.Fail<T>(error: primary + rollback));

    private static Fin<Unit> RestoreShared(
        (CommonObject Target, ArchivableDictionary Dictionary, bool Created) opened,
        ArchiveMap prior,
        Op op) => opened.Created
        ? from parent in Optional(opened.Dictionary.ParentUserData).ToFin(Fail: op.InvalidResult(
                detail: "Created shared user dictionary has no attached custody owner."))
          from _removed in op.Catch(() => op.Confirm(success: opened.Target.UserData.Remove(parent)))
          from _released in op.Catch(parent.Dispose)
          select unit
        : op.Catch(opened.Dictionary.Clear)
            .Bind(_ => prior.WriteTo(opened.Dictionary, op));

    private static Fin<(CommonObject Target, ArchivableDictionary Dictionary, bool Created)> Open(CommonObject target, Op op) {
        return op.Catch(() => {
            int before = target.UserData.Count;
            ArchivableDictionary? dictionary = target.UserDictionary;
            return dictionary is null
                ? Fin.Fail<(CommonObject, ArchivableDictionary, bool)>(error: op.InvalidResult(
                    detail: "Shared user dictionary could not be attached."))
                : Fin.Succ(value: (target, dictionary, target.UserData.Count > before));
        });
    }

    private static UserDataFact Describe(UserData value) => new(
        value.GetType().AssemblyQualifiedName ?? value.GetType().FullName ?? value.GetType().Name,
        value.Description,
        value.ShouldWrite,
        value.Transform);
}
```

## [04]-[LIFECYCLE]

Archive admission follows `ArchiveExchange` → schema check → checksum check → `ArchiveMap.Detach` → participant upgrade. Archive egress follows participant snapshot → `ArchiveMap.Mint` → schema frame → checksum marker → writer-state receipt. Each sealed override captures derived hooks and Rhino's base transform seam before collapsing to the host scalar, and one guarded report sink preserves the original fault after reporter failure. Success advances both states, while failure poisons the detached rail before reporting.

Roster mutations capture both censuses on the exception rail. Removal and move convert failed post-mutation disposal or transfer into `CustodySettlement.PartialCase`, and every roster mutation converts a failed closing census the same way; `Fin.Fail` remains reserved for a refusal before committed mutation. `DisposalPolicy` belongs to removal alone, because detach hands custody back while purge releases it at the host. Shared reads report whether Rhino created its internal carrier. Replacement and merge return schema-compatible prior/current maps plus their structural diff. Rollback detaches and releases a newly created carrier or restores a pre-existing carrier's prior typed map before returning the original failure with any rollback fault appended.

`SnapshotCodec` uses `ArchiveIo.Cross` and receives the same `ArchiveEnvelope` as `TypedUserData<TSelf>`. `ArchiveMap` remains the only payload currency; live `BinaryArchiveReader`, `BinaryArchiveWriter`, `UserDataList`, and `ArchivableDictionary` values never cross the boundary.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
