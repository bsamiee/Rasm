# [RASM_RHINO_PERSISTENCE_USERDATA]

`ArchiveIo` owns schema and integrity framing for every `ArchiveMap` crossing, and `IArchiveCodec` is the ONE codec root every archive participant conforms — `TypedUserData<TSelf>` on the host's `UserData` seam and `ParticipantSpec.Codec` on the snapshot seam. `Custody` moves attached user data on the document's own mutation spine: reads answer detached facts, mutations run inside `DocumentCommit.Sealed` and settle as `FactStream<UserDataSlot, UserDataBody>`. Every fallible host crossing rides the kernel `Op.Catch` funnel onto typed faults.

## [01]-[INDEX]

- [02]-[ARCHIVE_FRAME]: `ArchiveVersion`, `ArchiveSchema`, `ArchiveIntegrity`, `ArchiveEnvelope`, `ArchiveIo`, `IArchiveCodec` — the chunk frame, its integrity evidence, the direction-typed crossings, and the codec root.
- [03]-[TYPED_PARTICIPATION]: `TypedUserData<TSelf>` — the sealed `UserData` override lifecycle over one linearized payload cell.
- [04]-[CUSTODY_ALGEBRA]: `DisposalPolicy`, `TransferPlacement`, `WritePosture`, `SharedOrigin`, `CustodyPresence`, `UserDataFact`, `UserDataKind`, `UserDataSlot`, `UserDataBody`, `UserDataReceipt`, `CustodyQuery`, `CustodyAnswer`, `CustodyStep`, `CustodyProgram`, `Custody` — the roster reads and the spine-routed custody mutations.

## [02]-[ARCHIVE_FRAME]

- Owner: `ArchiveSchema` — the chunk typecode with its current and readable versions; `ArchiveIntegrity` — the crossing's own evidence, closed over the write and read cases and answering the kernel validity fold; `ArchiveIo` — the two direction-typed crossings; `IArchiveCodec` — the schema-plus-upgrade contract every archive participant realizes.
- Entry: `ArchiveIo.Cross` discriminates on the HOST HANDLE, so a write hands a `BinaryArchiveWriter` with its payload and reads back written integrity, and a read hands a `BinaryArchiveReader` and reads back an envelope. One name, one owner, forward and inverse together.
- Law: the caller's request already determines direction, so the crossing answers that direction's product and no consumer folds a union solely to recover the case it selected.
- Law: the chunk bracket is total on both crossings — CRC enabled inside the frame, close and policy restoration settle through `Custody` on both paths, and the version gate refuses BEFORE any payload is detached, so an unreadable frame never reaches `ArchiveMap.Detach` and cleanup cannot replace the primary fault.
- Law: integrity is EVIDENCE, not a pair of hand gates — `ArchiveIntegrity` carries the shared typecode, host archive version, and schema trio on its base and answers `IValidityEvidence` through one total fold, so both crossings gate on `IsValid` and neither re-spells its own predicate.
- Law: `IArchiveCodec` is an INTERFACE, not a base class: `TypedUserData<TSelf>` already derives the host's `Rhino.DocObjects.Custom.UserData`, which forecloses a second base, so the crossing defaults ride default interface members and both conformers inherit them for one declaration each.
- Growth: a new participant is one `IArchiveCodec` conformer; a new integrity dimension is one column on the owning case with every reader loudly broken.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[ComplexValueObject]`, `[Union]`, `[ValidationError]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `HashSet`); kernel `Domain/rails` (`Op`, `Op.Catch`, `Op.Need`, `Op.InvalidResult`), `Domain/validation` (`IValidityEvidence`, `ValidityClaim`); `Persistence/presets` (`PersistenceFault`), `Persistence/dictionary` (`ArchiveMap`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[ARCHIVE_IO]` — `WriteDictionary`, `ReadDictionary`, `BeginWrite3dmChunk`/`EndWrite3dmChunk`, `BeginRead3dmChunk`/`EndRead3dmChunk`, `EnableCRCCalculation`, `WriteEmptyCheckSum`, `ReadCheckSum`, `WriteErrorOccured`, `ReadErrorOccured`, `Archive3dmVersion`), RhinoCommon file I/O (`api-rhinocommon-fileio.md` — `BinaryArchiveWriter`, `BinaryArchiveReader`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.FileIO;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] ---------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public readonly partial record struct ArchiveVersion(int Major, int Minor) {
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int major,
        ref int minor) {
        Seq<string> violated = Seq((Broken: major < 0, Clause: "a non-negative major"), (Broken: minor < 0, Clause: "a non-negative minor"))
            .Filter(static row => row.Broken)
            .Map(static row => row.Clause);
        validationError = violated.IsEmpty
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { $"Archive version requires {string.Join(" and ", violated)}." }));
    }
}

[ComplexValueObject]
[ValidationError]
public sealed partial record ArchiveSchema(
    uint TypeCode,
    ArchiveVersion Current,
    LanguageExt.HashSet<ArchiveVersion> Readable) {
    public bool Reads(ArchiveVersion observed) => observed == Current || Readable.Contains(observed);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref uint typeCode,
        ref ArchiveVersion current,
        ref LanguageExt.HashSet<ArchiveVersion> readable) {
        Seq<string> violated = Seq(
                (Broken: typeCode == 0u, Clause: "a non-zero chunk typecode"),
                (Broken: readable.Contains(current), Clause: "a readable set excluding the current version"))
            .Filter(static row => row.Broken)
            .Map(static row => row.Clause);
        validationError = violated.IsEmpty
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { $"Archive schema requires {string.Join(" and ", violated)}." }));
    }
}

// The shared trio rides the base, so the two cases differ only in the evidence their own direction can observe: a
// writer publishes an error flag, a reader publishes that flag beside a checksum verdict.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveIntegrity(uint TypeCode, int Archive3dmVersion, ArchiveVersion Schema)
    : IValidityEvidence {
    public sealed record WrittenCase(uint TypeCode, int Archive3dmVersion, ArchiveVersion Schema, bool ErrorObserved)
        : ArchiveIntegrity(TypeCode, Archive3dmVersion, Schema);

    public sealed record ReadCase(
        uint TypeCode,
        int Archive3dmVersion,
        ArchiveVersion Schema,
        bool ChecksumVerified,
        bool ErrorObserved) : ArchiveIntegrity(TypeCode, Archive3dmVersion, Schema);

    public bool IsValid => Switch<ValidityClaim>(
        writtenCase: static written => ValidityClaim.All(!written.ErrorObserved),
        readCase: static read => ValidityClaim.All(read.ChecksumVerified, !read.ErrorObserved));
}

public sealed record ArchiveEnvelope(ArchiveMap Payload, ArchiveIntegrity.ReadCase Integrity);

// --- [SERVICES] -------------------------------------------------------------------------------
public interface IArchiveCodec {
    ArchiveSchema Schema { get; }

    Fin<ArchiveMap> Upgrade(ArchiveEnvelope envelope);

    Fin<ArchiveIntegrity.WrittenCase> Write(BinaryArchiveWriter archive, ArchiveMap payload, Op op) =>
        ArchiveIo.Cross(archive: archive, schema: Schema, payload: payload, key: op);

    Fin<ArchiveMap> Read(BinaryArchiveReader archive, Op op) =>
        ArchiveIo.Cross(archive: archive, schema: Schema, key: op)
            .Bind(envelope => op.Catch(() => Upgrade(envelope)));
}

// --- [OPERATIONS] -----------------------------------------------------------------------------
public static class ArchiveIo {
    public static Fin<ArchiveIntegrity.WrittenCase> Cross(
        BinaryArchiveWriter archive,
        ArchiveSchema schema,
        ArchiveMap payload,
        Op? key = null) {
        Op op = key.OrDefault();
        return from writer in op.Need(archive)
               from frame in op.Need(schema)
               from detached in op.Need(payload)
               from native in detached.Mint(op)
               from integrity in op.Catch(() => {
                   if (!writer.BeginWrite3dmChunk(frame.TypeCode, frame.Current.Major, frame.Current.Minor)) {
                       return Fin.Fail<ArchiveIntegrity.WrittenCase>(
                           op.InvalidResult(detail: "Binary archive writer refused the chunk frame."));
                   }

                   bool priorCrc = writer.EnableCRCCalculation(true);
                   Fin<Unit> body = op.Catch(() => {
                       writer.WriteDictionary(native);
                       writer.WriteEmptyCheckSum();
                       return Fin.Succ(unit);
                   });
                   return body.Settled(
                           held: Seq<Func<Fin<Unit>>>(
                               () => op.Catch(() => Fin.Succ(value: Op.Side(() => writer.EnableCRCCalculation(priorCrc)))),
                               () => op.Catch(() => op.Confirm(success: writer.EndWrite3dmChunk()))),
                           release: static settle => settle(),
                           key: op)
                       .Map(_ => new ArchiveIntegrity.WrittenCase(
                           frame.TypeCode,
                           writer.Archive3dmVersion,
                           frame.Current,
                           writer.WriteErrorOccured));
               })
               from _sound in guard(
                   integrity.IsValid,
                   op.InvalidResult(detail: "Binary archive writer reported an integrity fault.")).ToFin()
               select integrity;
    }

    public static Fin<ArchiveEnvelope> Cross(BinaryArchiveReader archive, ArchiveSchema schema, Op? key = null) {
        Op op = key.OrDefault();
        return from reader in op.Need(archive)
               from frame in op.Need(schema)
               from captured in op.Catch(() => {
                   if (!reader.BeginRead3dmChunk(frame.TypeCode, out int major, out int minor)) {
                       return Fin.Fail<(ArchivableDictionary Native, ArchiveIntegrity.ReadCase Integrity)>(
                           op.InvalidResult(detail: "Binary archive reader refused the chunk frame."));
                   }

                   ArchiveVersion observed = ArchiveVersion.Create(major, minor);
                   if (!frame.Reads(observed)) {
                       return Fin.Fail<(ArchivableDictionary Native, ArchiveIntegrity.ReadCase Integrity)>(
                               op.InvalidResult(detail: $"Archive schema '{observed.Major}.{observed.Minor}' is not readable."))
                           .Settled(
                               held: Seq(unit),
                               release: _ => op.Catch(() => op.Confirm(
                                   success: reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true))),
                               key: op);
                   }

                   bool priorCrc = reader.EnableCRCCalculation(true);
                   Fin<(ArchivableDictionary Native, bool Checksum)> body = op.Catch(() => Fin.Succ(value: (
                       Native: reader.ReadDictionary(),
                       Checksum: reader.ReadCheckSum())));
                   return body.Settled(
                           held: Seq<Func<Fin<Unit>>>(
                               () => op.Catch(() => Fin.Succ(value: Op.Side(() => reader.EnableCRCCalculation(priorCrc)))),
                               () => op.Catch(() => op.Confirm(
                                   success: reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: false)))),
                           release: static settle => settle(),
                           key: op)
                       .Map(read => (
                           read.Native,
                           new ArchiveIntegrity.ReadCase(
                               frame.TypeCode,
                               reader.Archive3dmVersion,
                               observed,
                               read.Checksum,
                               reader.ReadErrorOccured)));
               })
               from _sound in guard(
                   captured.Integrity.IsValid,
                   op.InvalidResult(detail: "Binary archive checksum or reader state is invalid.")).ToFin()
               from payload in ArchiveMap.Detach(captured.Native, op)
               select new ArchiveEnvelope(payload, captured.Integrity);
    }
}
```

## [03]-[TYPED_PARTICIPATION]

- Owner: `TypedUserData<TSelf>` — the sealed override lifecycle a foreign participant derives, holding live archive handles inside the host's own `bool`/`void` seams and publishing exactly one `IArchiveCodec` conformance.
- Entry: a participant supplies `Schema`, `Initial`, `Upgrade`, an optional `TransformPayload`, and a mandatory `Report` sink; every host override is sealed, so no derivation can bypass the rail or the poison discipline.
- Law: the template is FOREIGN-DERIVATION surface, exactly the class `RULINGS.md [02]` exempts from the caller census — its landed siblings are `Plugin/document`'s `IParticipant` and the `IArchiveCodec` conformer `Persistence/snapshots`' `ParticipantSpec.Codec` demands, and a zero-derivation corpus proves altitude rather than a missing producer.
- Law: both crossings gate on the `[ClassId]` pin. The schema's whole thesis is archive-version tolerance, which a class RENAME defeats outright, so a read-only participant proves the same resolution key it will be resolved by; proving it on write alone leaves it unproven for the participant's whole life.
- Law: the payload cell is an `Atom` because Rhino serializes NONE of the five writing callbacks against each other — `Write`, `Read`, `OnDuplicate`, `OnTransform`, and the caller-facing `Replace` — so the cell is its own linearization point and a stored payload can never interleave with a poisoned rail into a torn read.
- Law: `Adopt` re-seats authoritatively — a fresh archive read, a caller `Replace`, or a duplicate's snapshot replaces whatever the cell held, poison included — while `Derive` refuses: `OnTransform` stores a payload COMPUTED from a snapshot it already read, so a poison landing in between wins and the stale product never un-poisons the rail. Both read the transition; a discarded swap is the deleted spelling.
- Law: `Description` is sealed onto the closed type's own name, because `UserData.Description` is virtual and defaults to the literal "RhinoCommon UserData" — every unoverridden participant otherwise publishes the framework's string as its census identity.
- Law: one guarded sink owns every report — a reporter that itself faults leaves the ORIGINAL error on the rail rather than replacing it, and the host scalar collapses only after archive, duplicate, or transform work has finished.
- Growth: a new host callback is one sealed override over the same `Adopt`/`Derive`/`Poison` trio; a new participant is one derivation.
- Packages: LanguageExt.Core (`Fin`, `Option`, `Atom`); kernel `Domain/rails` (`Op`, `Op.Catch`, `Cell.Step`, `Transition`); `Persistence/dictionary` (`ArchiveMap`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[USERDATA_CUSTODY]` — `UserData.Description`/`ShouldWrite`/`Transform`/`Write`/`Read`/`OnTransform`/`OnDuplicate`, `ClassIdAttribute`), RhinoCommon file I/O (`api-rhinocommon-fileio.md` — `BinaryArchiveWriter`, `BinaryArchiveReader`), RhinoCommon geometry (`api-rhinocommon-geometry.md` — `Transform`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [SERVICES] -------------------------------------------------------------------------------
public abstract class TypedUserData<TSelf> : UserData, IArchiveCodec
    where TSelf : TypedUserData<TSelf> {
    private readonly Atom<Fin<Option<ArchiveMap>>> held = Atom(Fin.Succ<Option<ArchiveMap>>(None));

    public abstract ArchiveSchema Schema { get; }
    public abstract Fin<ArchiveMap> Upgrade(ArchiveEnvelope envelope);
    protected abstract Fin<ArchiveMap> Initial { get; }
    protected virtual Fin<ArchiveMap> TransformPayload(ArchiveMap payload, Transform transform) => Fin.Succ(payload);
    protected abstract void Report(Error error);

    public sealed override string Description => typeof(TSelf).Name;

    public sealed override bool ShouldWrite => held.Value.Match(
        Succ: state => state.Exists(static payload => payload.Entries.Count > 0),
        Fail: static _ => false);

    public Fin<ArchiveMap> Snapshot(Op? key = null) {
        Op op = key.OrDefault();
        return held.Value.Bind(state => state.Match(Succ, () => Initial.Bind(payload => Adopt(payload, op))));
    }

    public Fin<Unit> Replace(ArchiveMap payload, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(payload).Bind(admitted => Adopt(admitted, op)).Map(static _ => unit);
    }

    // Host truth: `ClassIdAttribute(string) : { Id : Guid }` pins a stable resolution key onto a `UserData` subclass so an
    // archive written under a prior class name keeps resolving it.
    private static Fin<Guid> Pinned(Op op) =>
        typeof(TSelf).GetCustomAttributes(typeof(ClassIdAttribute), inherit: false) is [ClassIdAttribute pin]
        && pin.Id != Guid.Empty
            ? Fin.Succ(value: pin.Id)
            : Fin.Fail<Guid>(error: op.InvalidResult(detail: $"'{typeof(TSelf).Name}' carries no [ClassId] pin."));

    protected sealed override bool Write(BinaryArchiveWriter archive) {
        Op op = Op.Of();
        return op.Catch(() => Pinned(op)
            .Bind(_ => Snapshot(op))
            .Bind(payload => ((IArchiveCodec)this).Write(archive, payload, op))
            .Map(static _ => unit))
            .Match(Succ: static _ => true, Fail: error => (Poison(error, op), false).Item2);
    }

    protected sealed override bool Read(BinaryArchiveReader archive) {
        Op op = Op.Of();
        return op.Catch(() => Pinned(op)
            .Bind(_ => ((IArchiveCodec)this).Read(archive, op))
            .Bind(payload => Adopt(payload, op))
            .Map(static _ => unit))
            .Match(Succ: static _ => true, Fail: error => (Poison(error, op), false).Item2);
    }

    protected sealed override void OnDuplicate(UserData source) {
        Op op = Op.Of();
        op.Catch(() => source is TSelf typed
            ? typed.Snapshot(op).Bind(payload => Adopt(payload, op)).Map(static _ => unit)
            : Fin.Fail<Unit>(op.Unsupported(inputType: source.GetType(), outputType: typeof(TSelf))))
            .Match(Succ: static _ => unit, Fail: error => Poison(error, op));
    }

    protected sealed override void OnTransform(Transform transform) {
        Op op = Op.Of();
        op.Catch(() => {
            base.OnTransform(transform);
            return Snapshot(op)
                .Bind(payload => TransformPayload(payload, transform))
                .Bind(payload => Derive(payload, op));
        }).Match(Succ: static _ => unit, Fail: error => Poison(error, op));
    }

    private Fin<ArchiveMap> Adopt(ArchiveMap payload, Op op) =>
        Settle(step: _ => Some(payload), payload: payload, op: op);

    private Fin<ArchiveMap> Derive(ArchiveMap payload, Op op) =>
        Settle(step: state => state.IsSucc ? Some(payload) : None, payload: payload, op: op);

    private Fin<ArchiveMap> Settle(Func<Fin<Option<ArchiveMap>>, Option<ArchiveMap>> step, ArchiveMap payload, Op op) =>
        Cell.Step(
                cell: held,
                step: state => step(state).Map(static next => Fin.Succ(Some(next))),
                declined: op.InvalidContext())
            .Switch(
                state: (Payload: payload, Op: op),
                committed: static (ctx, _) => Fin.Succ(value: ctx.Payload),
                ceded: static (ctx, _) => Fin.Fail<ArchiveMap>(error: ctx.Op.InvalidContext()),
                refused: static (_, row) => Fin.Fail<ArchiveMap>(error: row.Cause),
                contended: static (ctx, _) => Fin.Fail<ArchiveMap>(error: ctx.Op.InvalidResult()));

    // The poison step never declines, so its verdict is always `Committed` and `ignore` discards nothing a caller
    // could act on; the rail it seats is what every later read collapses on.
    private Unit Poison(Error error, Op op) {
        ignore(Cell.Step(cell: held, step: _ => Some(Fin.Fail<Option<ArchiveMap>>(error)), declined: op.InvalidContext()));
        return Reported(error, op);
    }

    private Unit Reported(Error error, Op op) => op.Catch(() => Report(error))
        .Match(Succ: static _ => unit, Fail: static _ => unit);
}
```

## [04]-[CUSTODY_ALGEBRA]

- Owner: `Custody` — the one attached-user-data entry, split by custody side: `Ask` answers detached roster facts and shared payloads, `Commit` runs a `CustodyProgram` inside the document's sealed record; `UserDataSlot`/`UserDataBody` — the folder's fact vocabulary; `UserDataReceipt` — the closed `FactStream` instantiation every mutation settles into.
- Entry: a mutation program names its steps and its redraw posture; `Custody.Commit` demands the session's mutation capability, opens ONE `DocumentCommit.Sealed` record, folds every step's facts, and stamps the undo serial through the stream's own projection.
- Law: user-data custody IS a document mutation — attaching, removing, purging, transferring, and replacing a shared dictionary all reshape resident objects the host records — so it runs on the ONE host-mutation path `ARCHITECTURE.md` declares and mints no second commit envelope, no second receipt timing class, and no local undo bracket.
- Law: the receipt is `FactStream<UserDataSlot, UserDataBody>` — commit-scoped, sealed by the undo stamp — and NOT a build receipt, because these facts are consequences inside one commit rather than evidence bound to a produced value. A `CustodyReceipt` beside it was the third receipt timing class the fact-stream owner's own law names deleted.
- Law: a post-mutation failure is a `Residue` FACT beside the roster fact, never a rail failure — the mutation already landed, so the removal's failed disposal, the move's failed placement, and a failed closing census each report while the step's own landing stands; `Fin.Fail` is reserved for a refusal BEFORE committed mutation, and the sealed record rolls the whole program back on one.
- Law: `DisposalPolicy` belongs to removal alone. Decompile-proven: `UserDataList.Purge` is `ON_UserData_PurgeUserData` on the parent, and the native delete fires `UserData.OnDelete` — it zeroes `m_native_pointer`, suppresses the finalizer, and drops the runtime-list entry, so `UserData.Dispose(true)` early-returns on the zero pointer and a purge-side policy names a choice the host does not publish. `Remove` does not release, because `ON_Object_DetachUserData` hands custody back to the caller.
- Law: the attach gate is ACCESSIBILITY, not nesting — `Type.IsPublic` is FALSE for a nested public type, so testing it refuses a participant the host itself accepts; `IsVisible` is the whole gate, public and publicly enclosed at every level, beside the parameterless-constructor requirement `UserDataList.Add` enforces by throwing.
- Law: shared-dictionary replacement avoids `ReplaceContentsWith` and its exact-runtime-type reflection fault: it captures the prior map, clears, writes typed, re-detaches, proves the postcondition, and restores on any failure — detaching and releasing a carrier the read itself MINTED, or rewriting a pre-existing carrier's prior map — riding the kernel `Custody.Rollback` delegate arm, which appends the rollback fault onto the primary.
- Law: an id-keyed containment probe answers a typed `CustodyPresence`, never a fact, because `UserDataList.Contains(Guid)` answers `bool` and `UserData` publishes no readable id — the type-keyed `Describe` is the arm that can answer a description.
- Boundary: custody on geometry that has not entered the document belongs to the Modeling lease (`ModelGate`), not this owner — this owner moves custody on document-resident objects under the record that makes it undoable.
- Growth: a new custody verb is one `CustodyStep` case beside its `UserDataSlot` row; a new fact is one `UserDataBody` case with its kind, and every reader breaks loudly.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<TKey>]`, `[Union]`, `[UseDelegateFromConstructor]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Validation` accumulation, `TraverseM`); kernel `Domain/rails` (`Op`, `Op.Catch`, `Op.Need`, `Op.Confirm`, `Custody.Rollback`), `Domain/validation` (`ICapability`, `CapabilitySet`); `Document/facts` (`IFactSlot<TBody, TKind>`, `IFactBody<TKind>`, `FactStream`, `UndoSerial`), `Document/commit` (`DocumentCommit.Sealed`, `RedrawPolicy`), `Document/session` (`DocumentSession`, `SessionNeed`, `UndoCustody`); `Persistence/dictionary` (`ArchiveMap`, `ArchiveChange`, `ArchiveMerge`), `Persistence/presets` (`PersistenceFault`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[USERDATA_CUSTODY]` — `UserDataList.Add`/`Remove`/`Find`/`Contains`/`Purge`/`Count`, `UserData.Copy`/`MoveUserDataFrom`/`MoveUserDataTo`/`Dispose`, `CommonObject.UserData`/`UserDictionary`, `ArchivableDictionary.ParentUserData`/`Clear`).

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
global using UserDataReceipt = Rasm.Rhino.Document.FactStream<Rasm.Rhino.Persistence.UserDataSlot, Rasm.Rhino.Persistence.UserDataBody>;

using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ----------------------------------------------------------------------------------
// Each row's key IS the host argument it lowers to, so no member re-states the bool the row already is.
[SmartEnum<bool>]
public sealed partial class DisposalPolicy {
    public static readonly DisposalPolicy Detach = new(key: false);
    public static readonly DisposalPolicy Dispose = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class TransferPlacement {
    public static readonly TransferPlacement Replace = new(key: false);
    public static readonly TransferPlacement Append = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class WritePosture {
    public static readonly WritePosture Skipped = new(key: false);
    public static readonly WritePosture Serialized = new(key: true);
}

// Whether the shared-dictionary read found a carrier or made the host mint one: `CommonObject.UserDictionary` is a
// lazy MUTATING accessor that attaches its own carrier when absent, so a read can grow the roster.
[SmartEnum<bool>]
public sealed partial class SharedOrigin {
    public static readonly SharedOrigin Existing = new(key: false);
    public static readonly SharedOrigin Minted = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class CustodyPresence {
    public static readonly CustodyPresence Absent = new(key: false);
    public static readonly CustodyPresence Present = new(key: true);
}

[SmartEnum<string>]
public sealed partial class UserDataKind : ICapability<UserDataKind> {
    public static readonly UserDataKind Roster = new("roster");
    public static readonly UserDataKind Transfer = new("transfer");
    public static readonly UserDataKind Shared = new("shared");
    public static readonly UserDataKind Residue = new("residue");
    public static readonly UserDataKind Record = new("record");
}

[SmartEnum<int>]
public sealed partial class UserDataSlot : IFactSlot<UserDataBody, UserDataKind> {
    public static readonly UserDataSlot Attached = new(key: 0, bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Roster));
    public static readonly UserDataSlot Removed = new(
        key: 1,
        bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Roster, UserDataKind.Residue));
    public static readonly UserDataSlot Purged = new(key: 2, bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Roster));
    public static readonly UserDataSlot Copied = new(key: 3, bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Roster));
    public static readonly UserDataSlot Moved = new(
        key: 4,
        bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Roster, UserDataKind.Transfer, UserDataKind.Residue));
    public static readonly UserDataSlot Replaced = new(key: 5, bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Shared));
    public static readonly UserDataSlot Merged = new(key: 6, bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Shared));
    public static readonly UserDataSlot UndoRecord = new(key: 7, bodies: CapabilitySet<UserDataKind>.Of(UserDataKind.Record));

    public CapabilitySet<UserDataKind> Bodies { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UserDataBody : IFactBody<UserDataKind> {
    private UserDataBody() { }

    public sealed record Roster(int Before, int After) : UserDataBody;
    public sealed record Transfer(Guid Id) : UserDataBody;
    public sealed record Shared(
        ArchiveMap Prior,
        ArchiveMap Current,
        Seq<ArchiveChange> Changes,
        SharedOrigin Origin) : UserDataBody;
    public sealed record Residue(Error Fault) : UserDataBody;
    public sealed record UndoRecord(UndoSerial Serial) : UserDataBody;

    public UserDataKind Kind => Switch<UserDataKind>(
        roster: static _ => UserDataKind.Roster,
        transfer: static _ => UserDataKind.Transfer,
        shared: static _ => UserDataKind.Shared,
        residue: static _ => UserDataKind.Residue,
        undoRecord: static _ => UserDataKind.Record);
}

public static class UserDataReceipts {
    public static UserDataReceipt Stamp(this UserDataReceipt receipt, uint serial) => receipt.Stamped(
        slot: UserDataSlot.UndoRecord,
        record: static minted => new UserDataBody.UndoRecord(Serial: minted),
        serial: serial);

    public static Seq<Error> Residue(this UserDataReceipt receipt, UserDataSlot slot) => receipt.Project(
        slot: slot,
        select: static body => body is UserDataBody.Residue residue ? Some(residue.Fault) : Option<Error>.None);

    public static Option<UndoSerial> Serial(this UserDataReceipt receipt) => receipt
        .Project(
            slot: UserDataSlot.UndoRecord,
            select: static body => body is UserDataBody.UndoRecord undo ? Some(undo.Serial) : Option<UndoSerial>.None)
        .HeadOrNone();
}

public sealed record UserDataFact(string RuntimeType, string Description, WritePosture Posture, Transform Transform);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UserDataRef {
    private UserDataRef() { }

    public sealed record IdCase(Guid Value) : UserDataRef;
    public sealed record TypeCase(Type Value) : UserDataRef;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodyQuery {
    private CustodyQuery() { }

    public sealed record CensusCase(CommonObject Target) : CustodyQuery;
    public sealed record ProbeCase(CommonObject Target, UserDataRef Reference) : CustodyQuery;
    public sealed record SharedCase(CommonObject Target) : CustodyQuery;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodyAnswer {
    private CustodyAnswer() { }

    public sealed record CensusCase(Seq<UserDataFact> Values) : CustodyAnswer;
    public sealed record PresenceCase(CustodyPresence Presence) : CustodyAnswer;
    public sealed record DescriptionCase(Option<UserDataFact> Value) : CustodyAnswer;
    public sealed record SharedCase(ArchiveMap Payload, SharedOrigin Origin) : CustodyAnswer;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodyStep {
    private CustodyStep() { }

    public sealed record AttachCase(CommonObject Target, UserData Value) : CustodyStep;
    public sealed record RemoveCase(CommonObject Target, UserData Value, DisposalPolicy Disposal) : CustodyStep;
    public sealed record PurgeCase(CommonObject Target) : CustodyStep;
    public sealed record CopyCase(CommonObject Source, CommonObject Destination) : CustodyStep;
    public sealed record MoveCase(CommonObject Source, CommonObject Destination, TransferPlacement Placement) : CustodyStep;
    public sealed record ReplaceCase(CommonObject Target, ArchiveMap Payload) : CustodyStep;
    public sealed record MergeCase(CommonObject Target, ArchiveMap Payload, ArchiveMerge Merge) : CustodyStep;

    // The slot and the census subject are the step's OWN facts, so the interpreter reads them off the case rather
    // than re-deciding both in a second parallel fold.
    internal UserDataSlot Slot => Switch<UserDataSlot>(
        attachCase: static _ => UserDataSlot.Attached,
        removeCase: static _ => UserDataSlot.Removed,
        purgeCase: static _ => UserDataSlot.Purged,
        copyCase: static _ => UserDataSlot.Copied,
        moveCase: static _ => UserDataSlot.Moved,
        replaceCase: static _ => UserDataSlot.Replaced,
        mergeCase: static _ => UserDataSlot.Merged);

    internal CommonObject Subject => Switch<CommonObject>(
        attachCase: static row => row.Target,
        removeCase: static row => row.Target,
        purgeCase: static row => row.Target,
        copyCase: static row => row.Destination,
        moveCase: static row => row.Destination,
        replaceCase: static row => row.Target,
        mergeCase: static row => row.Target);
}

// One named program with its redraw posture: the commit envelope reads both, so a program is the WHOLE ask.
public sealed record CustodyProgram(Seq<CustodyStep> Steps, RedrawPolicy Redraw, Option<string> RecordName);

// --- [OPERATIONS] -----------------------------------------------------------------------------
public static class Custody {
    public static Fin<CustodyAnswer> Ask(CustodyQuery query, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(query)
            .Bind(active => Admit(active, op))
            .Bind(active => active.Switch<Op, Fin<CustodyAnswer>>(
                state: op,
                censusCase: static (op, census) => op.Catch(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.CensusCase(
                    census.Target.UserData.Map(Describe).ToSeq()))),
                probeCase: static (op, probe) => probe.Reference.Switch<(CommonObject Target, Op Op), Fin<CustodyAnswer>>(
                    state: (probe.Target, op),
                    idCase: static (ctx, row) => ctx.Op.Catch(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.PresenceCase(
                        (CustodyPresence)ctx.Target.UserData.Contains(row.Value)))),
                    typeCase: static (ctx, row) => ctx.Op.Catch(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.DescriptionCase(
                        Optional(ctx.Target.UserData.Find(row.Value)).Map(Describe))))),
                sharedCase: static (op, read) => Open(read.Target, op)
                    .Bind(opened => ArchiveMap.Detach(opened.Dictionary, op)
                        .Map<CustodyAnswer>(map => new CustodyAnswer.SharedCase(map, opened.Origin)))));
    }

    public static Fin<UserDataReceipt> Commit(DocumentSession session, CustodyProgram program, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(program)
            .Bind(request => Admit(request, op))
            .Bind(admitted => session.Demand(
                use: document => DocumentCommit.Sealed(
                    document: document,
                    name: admitted.RecordName.IfNone(nameof(Custody)),
                    recordsUndo: true,
                    redraw: admitted.Redraw,
                    run: () => admitted.Steps
                        .TraverseM(step => Land(step, op))
                        .As()
                        .Map(static receipts => receipts.Fold(
                            UserDataReceipt.Empty,
                            static (state, value) => state + value)),
                    stamp: static (receipt, serial) => receipt.Stamp(serial: serial),
                    project: Fin.Succ,
                    op: op),
                key: op,
                needs: SessionNeed.Mutation(custody: UndoCustody.Recorded, redraw: admitted.Redraw).ToArray()));
    }

    // Independent clauses accumulate: a program whose steps carry several defects reports every one of them.
    private static Fin<CustodyProgram> Admit(CustodyProgram program, Op op) =>
        from steps in program.Steps
            .Map(step => Admit(step, op).ToValidation())
            .Traverse(static step => step)
            .As()
            .ToFin()
        from _redraw in op.Need(program.Redraw)
        from _nonEmpty in guard(!steps.IsEmpty, op.InvalidInput()).ToFin()
        select program with { Steps = steps };

    private static Fin<CustodyQuery> Admit(CustodyQuery query, Op op) => query.Switch<Op, Fin<CustodyQuery>>(
        state: op,
        censusCase: static (op, census) => op.Need(census.Target).Map(_ => (CustodyQuery)census),
        probeCase: static (op, probe) => (
                op.Need(probe.Target).ToValidation(),
                probe.Reference.Switch<Op, Fin<UserDataRef>>(
                    state: op,
                    idCase: static (op, row) => guard(row.Value != Guid.Empty, op.InvalidInput())
                        .ToFin()
                        .Map<UserDataRef>(_ => row),
                    typeCase: static (op, row) => op.Need(row.Value)
                        .Bind(type => guard(typeof(UserData).IsAssignableFrom(type), op.InvalidInput()).ToFin())
                        .Map<UserDataRef>(_ => row)).ToValidation())
            .Apply(static (target, reference) => (CustodyQuery)new CustodyQuery.ProbeCase(target, reference))
            .As()
            .ToFin(),
        sharedCase: static (op, read) => op.Need(read.Target).Map(_ => (CustodyQuery)read));

    private static Fin<CustodyStep> Admit(CustodyStep step, Op op) => step.Switch<Op, Fin<CustodyStep>>(
        state: op,
        attachCase: static (op, attach) => (op.Need(attach.Target).ToValidation(), AdmitAttach(attach.Value, op).ToValidation())
            .Apply(static (target, value) => (CustodyStep)new CustodyStep.AttachCase(target, value))
            .As()
            .ToFin(),
        removeCase: static (op, remove) => (
                op.Need(remove.Target).ToValidation(),
                op.Need(remove.Value).ToValidation(),
                op.Need(remove.Disposal).ToValidation())
            .Apply(static (target, value, disposal) => (CustodyStep)new CustodyStep.RemoveCase(target, value, disposal))
            .As()
            .ToFin(),
        purgeCase: static (op, purge) => op.Need(purge.Target).Map(_ => (CustodyStep)purge),
        copyCase: static (op, copy) => (op.Need(copy.Source).ToValidation(), op.Need(copy.Destination).ToValidation())
            .Apply(static (source, destination) => (CustodyStep)new CustodyStep.CopyCase(source, destination))
            .As()
            .ToFin(),
        moveCase: static (op, move) => (
                op.Need(move.Source).ToValidation(),
                op.Need(move.Destination).ToValidation(),
                op.Need(move.Placement).ToValidation())
            .Apply(static (source, destination, placement) => (CustodyStep)new CustodyStep.MoveCase(source, destination, placement))
            .As()
            .ToFin(),
        replaceCase: static (op, replace) => (op.Need(replace.Target).ToValidation(), op.Need(replace.Payload).ToValidation())
            .Apply(static (target, payload) => (CustodyStep)new CustodyStep.ReplaceCase(target, payload))
            .As()
            .ToFin(),
        mergeCase: static (op, merge) => (
                op.Need(merge.Target).ToValidation(),
                op.Need(merge.Payload).ToValidation(),
                op.Need(merge.Merge).ToValidation())
            .Apply(static (target, payload, policy) => (CustodyStep)new CustodyStep.MergeCase(target, payload, policy))
            .As()
            .ToFin());

    private static Fin<UserData> AdmitAttach(UserData value, Op op) => op.Need(value)
        .Bind(active => active.GetType() is { IsClass: true, IsVisible: true } type
            && type.GetConstructor(Type.EmptyTypes) is not null
                ? Fin.Succ(value: active)
                : Fin.Fail<UserData>(error: op.InvalidInput()));

    private static Fin<UserDataReceipt> Land(CustodyStep step, Op op) => step.Switch<Op, Fin<UserDataReceipt>>(
        state: op,
        attachCase: static (op, attach) => Rostered(
            step: attach,
            commit: () => op.Catch(() => op.Confirm(success: attach.Target.UserData.Add(attach.Value))).Map(static _ => UserDataReceipt.Empty),
            op: op),
        removeCase: static (op, remove) => Rostered(
            step: remove,
            commit: () => op.Catch(() => op.Confirm(success: remove.Target.UserData.Remove(remove.Value)))
                .Bind(_ => remove.Disposal.Key
                    ? Residue(remove.Slot, op.Catch(remove.Value.Dispose), op)
                    : Fin.Succ(value: UserDataReceipt.Empty)),
            op: op),
        purgeCase: static (op, purge) => Rostered(
            step: purge,
            commit: () => op.Catch(() => purge.Target.UserData.Purge()).Map(static _ => UserDataReceipt.Empty),
            op: op),
        copyCase: static (op, copy) => Rostered(
            step: copy,
            commit: () => op.Catch(() => UserData.Copy(copy.Source, copy.Destination)).Map(static _ => UserDataReceipt.Empty),
            op: op),
        moveCase: static (op, move) => Rostered(
            step: move,
            commit: () => op.Catch(() => Fin.Succ(value: UserData.MoveUserDataFrom(move.Source)))
                .Bind(id => id == Guid.Empty
                    ? Fin.Fail<UserDataReceipt>(error: op.InvalidResult(detail: "User-data move found no transferable custody."))
                    : from transfer in UserDataReceipt.Of(move.Slot, new UserDataBody.Transfer(id), op)
                      from placed in Residue(
                          move.Slot,
                          op.Catch(() => UserData.MoveUserDataTo(move.Destination, id, move.Placement.Key)),
                          op)
                      select transfer + placed),
            op: op),
        replaceCase: static (op, replace) => Open(replace.Target, op)
            .Bind(opened => Reseat(replace.Slot, opened, replace.Payload, op)),
        mergeCase: static (op, merge) => Open(merge.Target, op)
            .Bind(opened => ArchiveMap.Detach(opened.Dictionary, op)
                .Bind(current => current.Merge(merge.Payload, merge.Merge, op))
                .Bind(payload => Reseat(merge.Slot, opened, payload, op))));

    // ONE roster fold: the before/after census brackets every roster mutation, and a failed CLOSING census is a
    // residue fact — the mutation landed, so refusing the whole step would report a rollback that never happened.
    private static Fin<UserDataReceipt> Rostered(CustodyStep step, Func<Fin<UserDataReceipt>> commit, Op op) =>
        from before in op.Catch(() => Fin.Succ(value: step.Subject.UserData.Count))
        from tail in commit()
        from rostered in op.Catch(() => Fin.Succ(value: step.Subject.UserData.Count)).Match(
            Succ: after => UserDataReceipt.Of(step.Slot, new UserDataBody.Roster(before, after), op),
            Fail: fault => from seed in UserDataReceipt.Of(step.Slot, new UserDataBody.Roster(before, before), op)
                           from missed in Residue(step.Slot, Fin.Fail<Unit>(fault), op)
                           select seed + missed)
        select rostered + tail;

    private static Fin<UserDataReceipt> Residue(UserDataSlot slot, Fin<Unit> outcome, Op op) => outcome.Match(
        Succ: static _ => Fin.Succ(value: UserDataReceipt.Empty),
        Fail: fault => UserDataReceipt.Of(slot, new UserDataBody.Residue(fault), op));

    private static Fin<UserDataReceipt> Reseat(
        UserDataSlot slot,
        (CommonObject Target, ArchivableDictionary Dictionary, SharedOrigin Origin) opened,
        ArchiveMap payload,
        Op op) =>
        from prior in ArchiveMap.Detach(opened.Dictionary, op)
        from _schema in prior.Diff(payload, op).Map(static _ => unit)
        from settled in (
            from _clear in op.Catch(opened.Dictionary.Clear)
            from _write in payload.WriteTo(opened.Dictionary, op)
            from current in ArchiveMap.Detach(opened.Dictionary, op)
            from changes in prior.Diff(current, op)
            from _proof in guard(
                current.SameContent(payload),
                op.InvalidResult(detail: "Shared user dictionary postcondition failed.")).ToFin()
            from receipt in UserDataReceipt.Of(
                slot,
                new UserDataBody.Shared(prior, current, changes, opened.Origin),
                op)
            select receipt)
            // Kernel `Custody.Rollback` delegate arm: the release is a keyed compensation fold no span can carry —
            // restore the prior shared dictionary, or detach and dispose the parent this reseat created.
            .Rollback(() => RestoreShared(opened, prior, op))
        select settled;

    private static Fin<Unit> RestoreShared(
        (CommonObject Target, ArchivableDictionary Dictionary, SharedOrigin Origin) opened,
        ArchiveMap prior,
        Op op) => opened.Origin.Key
        ? from parent in Optional(opened.Dictionary.ParentUserData).ToFin(Fail: op.InvalidResult(
                detail: "Created shared user dictionary has no attached custody owner."))
          from _removed in op.Catch(() => op.Confirm(success: opened.Target.UserData.Remove(parent)))
          from _released in op.Catch(parent.Dispose)
          select unit
        : op.Catch(opened.Dictionary.Clear).Bind(_ => prior.WriteTo(opened.Dictionary, op));

    private static Fin<(CommonObject Target, ArchivableDictionary Dictionary, SharedOrigin Origin)> Open(
        CommonObject target,
        Op op) =>
        op.Catch(() => {
            int before = target.UserData.Count;
            ArchivableDictionary? dictionary = target.UserDictionary;
            return dictionary is null
                ? Fin.Fail<(CommonObject, ArchivableDictionary, SharedOrigin)>(error: op.InvalidResult(
                    detail: "Shared user dictionary could not be attached."))
                : Fin.Succ(value: (target, dictionary, (SharedOrigin)(target.UserData.Count > before)));
        });

    private static UserDataFact Describe(UserData value) => new(
        value.GetType().AssemblyQualifiedName ?? value.GetType().FullName ?? value.GetType().Name,
        value.Description,
        (WritePosture)value.ShouldWrite,
        value.Transform);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
