# [RASM_RHINO_PERSISTENCE_USERDATA]

`ArchiveIo` owns schema and integrity framing for every `ArchiveMap` crossing, and `IArchiveCodec` is the ONE codec root every archive participant conforms — `TypedUserData<TSelf>` on the host's `UserData` boundary and `ParticipantSpec.Codec` on the snapshot boundary. `Custody` answers detached snapshots and moves attached user data inside `DocumentCommit.Sealed`. Every fallible host crossing rides the kernel `Try.lift` funnel onto typed faults.

## [01]-[INDEX]

- [02]-[ARCHIVE_FRAME]: `ArchiveVersion`, `ArchiveSchema`, `ArchiveIntegrity`, `ArchiveEnvelope`, `ArchiveIo`, `IArchiveCodec` — the chunk frame, its integrity evidence, the direction-typed crossings, and the codec root.
- [03]-[TYPED_PARTICIPATION]: `TypedUserData<TSelf>` — the sealed `UserData` override lifecycle over one linearized payload cell.
- [04]-[CUSTODY_ALGEBRA]: `DisposalPolicy`, `TransferPlacement`, `WritePosture`, `SharedOrigin`, `CustodyPresence`, `UserDataSnapshot`, `CustodyQuery`, `CustodyAnswer`, `CustodyStep`, `CustodyProgram`, `Custody` — the roster reads and custody mutations.

## [02]-[ARCHIVE_FRAME]

- Owner: `ArchiveSchema` — the chunk typecode with its current and readable versions; `ArchiveIntegrity` — the crossing's own evidence, closed over the write and read cases and answering the kernel validity fold; `ArchiveIo` — the two direction-typed crossings; `IArchiveCodec` — the schema-plus-upgrade contract every archive participant realizes.
- Entry: `ArchiveIo.Cross` discriminates on the HOST HANDLE, so a write hands a `BinaryArchiveWriter` with its payload and reads back written integrity, and a read hands a `BinaryArchiveReader` and reads back an envelope. One name, one owner, forward and inverse together.
- Law: the caller's request already determines direction, so the crossing answers that direction's product and no consumer folds a union solely to recover the case it selected.
- Law: the chunk bracket is total on both crossings — CRC enabled inside the frame, close and policy restoration settle through `Custody` on both paths, and the version gate refuses BEFORE any payload is detached, so an unreadable frame never reaches `ArchiveMap.Detach` and cleanup cannot replace the primary fault.
- Law: integrity is EVIDENCE, not a pair of hand gates — `ArchiveIntegrity` carries the shared typecode, host archive version, and schema trio on its base and answers `IValidityEvidence` through one total fold, so both crossings gate on `IsValid` and neither re-spells its own predicate.
- Law: `IArchiveCodec` is an INTERFACE, not a base class: `TypedUserData<TSelf>` already derives the host's `Rhino.DocObjects.Custom.UserData`, which forecloses a second base, so the crossing defaults ride default interface members and both conformers inherit them for one declaration each.
- Growth: a new participant is one `IArchiveCodec` conformer; a new integrity dimension is one column on the owning case with every reader loudly broken.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[ComplexValueObject]`, `[Union]`, `[ValidationError]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `HashSet`); kernel `Domain/results` (`Try.lift`, `Admit.Need`, `KernelFault.InvalidResult`), `Domain/validation` (`IValidityEvidence`, `ValidityClaim`); `Persistence/presets` (`PersistenceFault`), `Persistence/dictionary` (`ArchiveMap`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[ARCHIVE_IO]` — `WriteDictionary`, `ReadDictionary`, `BeginWrite3dmChunk`/`EndWrite3dmChunk`, `BeginRead3dmChunk`/`EndRead3dmChunk`, `EnableCRCCalculation`, `WriteEmptyCheckSum`, `ReadCheckSum`, `WriteErrorOccured`, `ReadErrorOccured`, `Archive3dmVersion`), RhinoCommon file I/O (`api-rhinocommon-fileio.md` — `BinaryArchiveWriter`, `BinaryArchiveReader`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.FileIO;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
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

// --- [SERVICES] ------------------------------------------------------------------------
public interface IArchiveCodec {
    ArchiveSchema Schema { get; }

    Fin<ArchiveMap> Upgrade(ArchiveEnvelope envelope);

    Fin<ArchiveIntegrity.WrittenCase> Write(BinaryArchiveWriter archive, ArchiveMap payload) =>
        ArchiveIo.Cross(archive: archive, schema: Schema, payload: payload);

    Fin<ArchiveMap> Read(BinaryArchiveReader archive) =>
        ArchiveIo.Cross(archive: archive, schema: Schema)
            .Bind(envelope => Try.lift(() => Upgrade(envelope)).Run().Bind(static inner => inner));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArchiveIo {
    public static Fin<ArchiveIntegrity.WrittenCase> Cross(
        BinaryArchiveWriter archive,
        ArchiveSchema schema,
        ArchiveMap payload) {
        return from writer in Admit.Need(archive)
               from frame in Admit.Need(schema)
               from detached in Admit.Need(payload)
               from native in detached.Mint()
               from integrity in Try.lift(() => {
                   if (!writer.BeginWrite3dmChunk(frame.TypeCode, frame.Current.Major, frame.Current.Minor)) {
                       return Fin.Fail<ArchiveIntegrity.WrittenCase>(
                           new KernelFault.InvalidResult(Detail: Some("Binary archive writer refused the chunk frame.")));
                   }

                   bool priorCrc = writer.EnableCRCCalculation(true);
                   Fin<Unit> body = Try.lift(() => {
                       writer.WriteDictionary(native);
                       writer.WriteEmptyCheckSum();
                       return Fin.Succ(unit);
                   }).Run().Bind(static inner => inner);
                   return body.Settled(
                           held: Seq<Func<Fin<Unit>>>(
                               () => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => writer.EnableCRCCalculation(priorCrc)))).Run().Bind(static inner => inner),
                               () => Try.lift(() => Admit.Confirm(success: writer.EndWrite3dmChunk())).Run().Bind(static inner => inner)),
                           release: static settle => settle())
                       .Map(_ => new ArchiveIntegrity.WrittenCase(
                           frame.TypeCode,
                           writer.Archive3dmVersion,
                           frame.Current,
                           writer.WriteErrorOccured));
               }).Run().Bind(static inner => inner)
               from _sound in guard(
                   integrity.IsValid,
                   new KernelFault.InvalidResult(Detail: Some("Binary archive writer reported an integrity fault.")))
               select integrity;
    }

    public static Fin<ArchiveEnvelope> Cross(BinaryArchiveReader archive, ArchiveSchema schema) {
        return from reader in Admit.Need(archive)
               from frame in Admit.Need(schema)
               from captured in Try.lift(() => {
                   if (!reader.BeginRead3dmChunk(frame.TypeCode, out int major, out int minor)) {
                       return Fin.Fail<(ArchivableDictionary Native, ArchiveIntegrity.ReadCase Integrity)>(
                           new KernelFault.InvalidResult(Detail: Some("Binary archive reader refused the chunk frame.")));
                   }

                   ArchiveVersion observed = ArchiveVersion.Create(major, minor);
                   if (!frame.Reads(observed)) {
                       return Fin.Fail<(ArchivableDictionary Native, ArchiveIntegrity.ReadCase Integrity)>(
                               new KernelFault.InvalidResult(Detail: Some($"Archive schema '{observed.Major}.{observed.Minor}' is not readable.")))
                           .Settled(
                               held: Seq(unit),
                               release: _ => Try.lift(() => Admit.Confirm(
                                   success: reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true))).Run().Bind(static inner => inner));
                   }

                   bool priorCrc = reader.EnableCRCCalculation(true);
                   Fin<(ArchivableDictionary Native, bool Checksum)> body = Try.lift(() => Fin.Succ(value: (
                       Native: reader.ReadDictionary(),
                       Checksum: reader.ReadCheckSum()))).Run().Bind(static inner => inner);
                   return body.Settled(
                           held: Seq<Func<Fin<Unit>>>(
                               () => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => reader.EnableCRCCalculation(priorCrc)))).Run().Bind(static inner => inner),
                               () => Try.lift(() => Admit.Confirm(
                                   success: reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: false))).Run().Bind(static inner => inner)),
                           release: static settle => settle())
                       .Map(read => (
                           read.Native,
                           new ArchiveIntegrity.ReadCase(
                               frame.TypeCode,
                               reader.Archive3dmVersion,
                               observed,
                               read.Checksum,
                               reader.ReadErrorOccured)));
               }).Run().Bind(static inner => inner)
               from _sound in guard(
                   captured.Integrity.IsValid,
                   new KernelFault.InvalidResult(Detail: Some("Binary archive checksum or reader state is invalid.")))
               from payload in ArchiveMap.Detach(captured.Native)
               select new ArchiveEnvelope(payload, captured.Integrity);
    }
}
```

## [03]-[TYPED_PARTICIPATION]

- Owner: `TypedUserData<TSelf>` — the sealed override lifecycle a foreign participant derives, holding live archive handles inside the host's own `bool`/`void` overrides and publishing exactly one `IArchiveCodec` conformance.
- Entry: a participant supplies `Schema`, `Initial`, `Upgrade`, an optional `TransformPayload`, and a mandatory `Report` sink; every host override is sealed, so no derivation can bypass the carrier or the poison discipline.
- Law: the template is FOREIGN-DERIVATION surface, exactly the class `RULINGS.md [02]` exempts from the caller census — its landed siblings are `Plugin/document`'s `IParticipant` and the `IArchiveCodec` conformer `Persistence/snapshots`' `ParticipantSpec.Codec` demands, and a zero-derivation corpus proves altitude rather than a missing producer.
- Law: both crossings gate on the `[ClassId]` pin. The schema's whole thesis is archive-version tolerance, which a class RENAME defeats outright, so a read-only participant proves the same resolution key it will be resolved by; proving it on write alone leaves it unproven for the participant's whole life.
- Law: the payload cell is an `Atom` because Rhino serializes NONE of the five writing callbacks against each other — `Write`, `Read`, `OnDuplicate`, `OnTransform`, and the caller-facing `Replace` — so the cell is its own linearization point and a stored payload can never interleave with a poisoned carrier into a torn read.
- Law: `Adopt` re-seats authoritatively — a fresh archive read, a caller `Replace`, or a duplicate's snapshot replaces whatever the cell held, poison included — while `Derive` refuses: `OnTransform` stores a payload COMPUTED from a snapshot it already read, so a poison landing in between wins and the stale product never un-poisons the carrier. Both read the transition; a discarded swap is the deleted spelling.
- Law: `Description` is sealed onto the closed type's own name, because `UserData.Description` is virtual and defaults to the literal "RhinoCommon UserData" — every unoverridden participant otherwise publishes the framework's string as its census identity.
- Law: one guarded sink owns every report — a reporter that itself faults leaves the ORIGINAL error on the carrier rather than replacing it, and the host scalar collapses only after archive, duplicate, or transform work has finished.
- Growth: a new host callback is one sealed override over the same `Adopt`/`Derive`/`Poison` trio; a new participant is one derivation.
- Packages: LanguageExt.Core (`Fin`, `Option`, `Atom`); kernel `Domain/results` (`Try.lift`, `Cell.Step`, `Transition`); `Persistence/dictionary` (`ArchiveMap`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[USERDATA_CUSTODY]` — `UserData.Description`/`ShouldWrite`/`Transform`/`Write`/`Read`/`OnTransform`/`OnDuplicate`, `ClassIdAttribute`), RhinoCommon file I/O (`api-rhinocommon-fileio.md` — `BinaryArchiveWriter`, `BinaryArchiveReader`), RhinoCommon geometry (`api-rhinocommon-geometry.md` — `Transform`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [SERVICES] ------------------------------------------------------------------------
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

    public Fin<ArchiveMap> Snapshot() {
        return held.Value.Bind(state => state.Match(Succ, () => Initial.Bind(payload => Adopt(payload))));
    }

    public Fin<Unit> Replace(ArchiveMap payload) {
        return Admit.Need(payload).Bind(admitted => Adopt(admitted)).Map(static _ => unit);
    }

    private static Fin<Guid> Pinned() =>
        typeof(TSelf).GetCustomAttributes(typeof(ClassIdAttribute), inherit: false) is [ClassIdAttribute pin]
        && pin.Id != Guid.Empty
            ? Fin.Succ(value: pin.Id)
            : Fin.Fail<Guid>(error: new KernelFault.InvalidResult(Detail: Some($"'{typeof(TSelf).Name}' carries no [ClassId] pin.")));

    protected sealed override bool Write(BinaryArchiveWriter archive) {
        return Try.lift(() => Pinned()
            .Bind(_ => Snapshot())
            .Bind(payload => ((IArchiveCodec)this).Write(archive, payload))
            .Map(static _ => unit)).Run().Bind(static inner => inner)
            .Match(Succ: static _ => true, Fail: error => (Poison(error), false).Item2);
    }

    protected sealed override bool Read(BinaryArchiveReader archive) {
        return Try.lift(() => Pinned()
            .Bind(_ => ((IArchiveCodec)this).Read(archive))
            .Bind(payload => Adopt(payload))
            .Map(static _ => unit)).Run().Bind(static inner => inner)
            .Match(Succ: static _ => true, Fail: error => (Poison(error), false).Item2);
    }

    protected sealed override void OnDuplicate(UserData source) {
        Try.lift(() => source is TSelf typed
            ? typed.Snapshot(op).Bind(payload => Adopt(payload)).Map(static _ => unit)
            : Fin.Fail<Unit>(new KernelFault.Unsupported(InputType: source.GetType(), OutputType: typeof(TSelf)))).Run().Bind(static inner => inner)
            .Match(Succ: static _ => unit, Fail: error => Poison(error));
    }

    protected sealed override void OnTransform(Transform transform) {
        Try.lift(() => {
            base.OnTransform(transform);
            return Snapshot()
                .Bind(payload => TransformPayload(payload, transform))
                .Bind(payload => Derive(payload, op));
        }).Run().Bind(static inner => inner).Match(Succ: static _ => unit, Fail: error => Poison(error));
    }

    private Fin<ArchiveMap> Adopt(ArchiveMap payload) =>
        Settle(step: _ => Some(payload), payload: payload);

    private Fin<ArchiveMap> Derive(ArchiveMap payload) =>
        Settle(step: state => state.IsSucc ? Some(payload) : None, payload: payload);

    private Fin<ArchiveMap> Settle(Func<Fin<Option<ArchiveMap>>, Option<ArchiveMap>> step, ArchiveMap payload) =>
        Cell.Step(
                cell: held,
                step: state => step(state).Map(static next => Fin.Succ(Some(next))),
                declined: new KernelFault.InvalidContext())
            .Switch(
                state: payload,
                committed: static (ctx, _) => Fin.Succ(value: ctx),
                ceded: static (ctx, _) => Fin.Fail<ArchiveMap>(error: new KernelFault.InvalidContext()),
                refused: static (_, row) => Fin.Fail<ArchiveMap>(error: row.Cause),
                contended: static (ctx, _) => Fin.Fail<ArchiveMap>(error: new KernelFault.InvalidResult()));

    private Unit Poison(Error error) {
        ignore(Cell.Step(cell: held, step: _ => Some(Fin.Fail<Option<ArchiveMap>>(error)), declined: new KernelFault.InvalidContext()));
        return Reported(error);
    }

    private Unit Reported(Error error) => Try.lift(() => Report(error)).Run().Bind(static inner => inner)
        .Match(Succ: static _ => unit, Fail: static _ => unit);
}
```

## [04]-[CUSTODY_ALGEBRA]

- Owner: `Custody` — the one attached-user-data entry, split by custody side: `Ask` answers detached roster snapshots and shared payloads, while `Commit` runs a `CustodyProgram` inside the document's sealed record.
- Entry: a mutation program names its steps and its redraw posture; `Custody.Commit` demands the session's mutation capability and opens one `DocumentCommit.Sealed` record.
- Law: user-data custody is a document mutation, so every step runs on the host-mutation path and a failure remains on the typed result for the commit envelope to settle.
- Law: `DisposalPolicy` belongs to removal alone. Decompile-proven: `UserDataList.Purge` is `ON_UserData_PurgeUserData` on the parent, and the native delete fires `UserData.OnDelete` — it zeroes `m_native_pointer`, suppresses the finalizer, and drops the runtime-list entry, so `UserData.Dispose(true)` early-returns on the zero pointer and a purge-side policy names a choice the host does not publish. `Remove` does not release, because `ON_Object_DetachUserData` hands custody back to the caller.
- Law: the attach gate is ACCESSIBILITY, not nesting — `Type.IsPublic` is FALSE for a nested public type, so testing it refuses a participant the host itself accepts; `IsVisible` is the whole gate, public and publicly enclosed at every level, beside the parameterless-constructor requirement `UserDataList.Add` enforces by throwing.
- Law: shared-dictionary replacement avoids `ReplaceContentsWith` and its exact-runtime-type reflection fault: it captures the prior map, clears, writes typed, re-detaches, proves the postcondition, and restores on any failure — detaching and releasing a carrier the read itself MINTED, or rewriting a pre-existing carrier's prior map — riding the kernel `Custody.Rollback` delegate arm, which appends the rollback fault onto the primary.
- Law: an id-keyed containment probe answers a typed `CustodyPresence`, never a fact, because `UserDataList.Contains(Guid)` answers `bool` and `UserData` publishes no readable id — the type-keyed `Describe` is the arm that can answer a description.
- Boundary: custody on geometry that has not entered the document belongs to the Modeling lease (`ModelGate`), not this owner — this owner moves custody on document-resident objects under the record that makes it undoable.
- Growth: a new custody verb is one `CustodyStep` case and one interpreter arm.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<TKey>]`, `[Union]`, `[UseDelegateFromConstructor]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Validation` accumulation, `TraverseM`); kernel `Domain/results` (`Try.lift`, `Admit.Need`, `Admit.Confirm`, `Custody.Rollback`); `Document/commit` (`DocumentCommit.Sealed`, `RedrawPolicy`), `Document/session` (`DocumentSession`, `SessionNeed`, `UndoCustody`); `Persistence/dictionary` (`ArchiveMap`, `ArchiveMerge`), `Persistence/presets` (`PersistenceFault`); RhinoCommon persistence (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` `[USERDATA_CUSTODY]` — `UserDataList.Add`/`Remove`/`Find`/`Contains`/`Purge`, `UserData.Copy`/`MoveUserDataFrom`/`MoveUserDataTo`/`Dispose`, `CommonObject.UserData`/`UserDictionary`, `ArchivableDictionary.ParentUserData`/`Clear`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record UserDataSnapshot(string RuntimeType, string Description, WritePosture Posture, Transform Transform);

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

    public sealed record CensusCase(Seq<UserDataSnapshot> Values) : CustodyAnswer;
    public sealed record PresenceCase(CustodyPresence Presence) : CustodyAnswer;
    public sealed record DescriptionCase(Option<UserDataSnapshot> Value) : CustodyAnswer;
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

}

public sealed record CustodyProgram(Seq<CustodyStep> Steps, RedrawPolicy Redraw, Option<string> RecordName);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Custody {
    public static Fin<CustodyAnswer> Ask(CustodyQuery query) {
        return Admit.Need(query)
            .Bind(active => Admit(active))
            .Bind(active => active.Switch< Fin<CustodyAnswer>>(
                censusCase: static (census) => Try.lift(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.CensusCase(
                    census.Target.UserData.Map(Describe).ToSeq()))).Run().Bind(static inner => inner),
                probeCase: static (probe) => probe.Reference.Switch<(CommonObject Target), Fin<CustodyAnswer>>(
                    state: (probe.Target),
                    idCase: static row => Try.lift(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.PresenceCase(
                        (CustodyPresence)ctx.Target.UserData.Contains(row.Value)))).Run().Bind(static inner => inner),
                    typeCase: static row => Try.lift(() => Fin.Succ<CustodyAnswer>(value: new CustodyAnswer.DescriptionCase(
                        Optional(ctx.Target.UserData.Find(row.Value)).Map(Describe)))).Run().Bind(static inner => inner)),
                sharedCase: static (read) => Open(read.Target)
                    .Bind(opened => ArchiveMap.Detach(opened.Dictionary)
                        .Map<CustodyAnswer>(map => new CustodyAnswer.SharedCase(map, opened.Origin)))));
    }

    public static Fin<Unit> Commit(DocumentSession session, CustodyProgram program) {
        return Admit.Need(program)
            .Bind(request => Admit(request))
            .Bind(admitted => session.Demand(
                use: document => DocumentCommit.Sealed(
                    document: document,
                    name: admitted.RecordName.IfNone(nameof(Custody)),
                    recordsUndo: true,
                    redraw: admitted.Redraw,
                    run: () => admitted.Steps
                        .TraverseM(step => Land(step))
                        .As()
                        .Map(static _ => unit),
                    project: Fin.Succ),
                needs: SessionNeed.Mutation(custody: UndoCustody.Recorded, redraw: admitted.Redraw).ToArray()));
    }

    private static Fin<CustodyProgram> Admit(CustodyProgram program) =>
        from steps in program.Steps
            .Map(step => Admit(step).ToValidation())
            .Traverse(static step => step)
            .As()
            .ToFin()
        from _redraw in Admit.Need(program.Redraw)
        from _nonEmpty in guard(!steps.IsEmpty, new KernelFault.InvalidInput())
        select program with { Steps = steps };

    private static Fin<CustodyQuery> Admit(CustodyQuery query) => query.Switch< Fin<CustodyQuery>>(
        censusCase: static (census) => Admit.Need(census.Target).Map(_ => (CustodyQuery)census),
        probeCase: static (probe) => (
                Admit.Need(probe.Target).ToValidation(),
                probe.Reference.Switch< Fin<UserDataRef>>(
                    idCase: static (row) => guard(row.Value != Guid.Empty, new KernelFault.InvalidInput())
                        .ToFin()
                        .Map<UserDataRef>(_ => row),
                    typeCase: static (row) => Admit.Need(row.Value)
                        .Bind(type => guard(typeof(UserData).IsAssignableFrom(type), new KernelFault.InvalidInput()).ToFin())
                        .Map<UserDataRef>(_ => row)).ToValidation())
            .Apply(static reference => (CustodyQuery)new CustodyQuery.ProbeCase(reference))
            .As()
            .ToFin(),
        sharedCase: static (read) => Admit.Need(read.Target).Map(_ => (CustodyQuery)read));

    private static Fin<CustodyStep> Admit(CustodyStep step) => step.Switch< Fin<CustodyStep>>(
        attachCase: static (attach) => (Admit.Need(attach.Target).ToValidation(), AdmitAttach(attach.Value).ToValidation())
            .Apply(static value => (CustodyStep)new CustodyStep.AttachCase(value))
            .As()
            .ToFin(),
        removeCase: static (remove) => (
                Admit.Need(remove.Target).ToValidation(),
                Admit.Need(remove.Value).ToValidation(),
                Admit.Need(remove.Disposal).ToValidation())
            .Apply(static (value, disposal) => (CustodyStep)new CustodyStep.RemoveCase(value, disposal))
            .As()
            .ToFin(),
        purgeCase: static (purge) => Admit.Need(purge.Target).Map(_ => (CustodyStep)purge),
        copyCase: static (copy) => (Admit.Need(copy.Source).ToValidation(), Admit.Need(copy.Destination).ToValidation())
            .Apply(static (source, destination) => (CustodyStep)new CustodyStep.CopyCase(source, destination))
            .As()
            .ToFin(),
        moveCase: static (move) => (
                Admit.Need(move.Source).ToValidation(),
                Admit.Need(move.Destination).ToValidation(),
                Admit.Need(move.Placement).ToValidation())
            .Apply(static (source, destination, placement) => (CustodyStep)new CustodyStep.MoveCase(source, destination, placement))
            .As()
            .ToFin(),
        replaceCase: static (replace) => (Admit.Need(replace.Target).ToValidation(), Admit.Need(replace.Payload).ToValidation())
            .Apply(static payload => (CustodyStep)new CustodyStep.ReplaceCase(payload))
            .As()
            .ToFin(),
        mergeCase: static (merge) => (
                Admit.Need(merge.Target).ToValidation(),
                Admit.Need(merge.Payload).ToValidation(),
                Admit.Need(merge.Merge).ToValidation())
            .Apply(static (payload, policy) => (CustodyStep)new CustodyStep.MergeCase(payload, policy))
            .As()
            .ToFin());

    private static Fin<UserData> AdmitAttach(UserData value) => Admit.Need(value)
        .Bind(active => active.GetType() is { IsClass: true, IsVisible: true } type
            && type.GetConstructor(Type.EmptyTypes) is not null
                ? Fin.Succ(value: active)
                : Fin.Fail<UserData>(error: new KernelFault.InvalidInput()));

    private static Fin<Unit> Land(CustodyStep step) => step.Switch< Fin<Unit>>(
        attachCase: static (attach) => Try.lift(() => Admit.Confirm(success: attach.Target.UserData.Add(attach.Value))).Run().Bind(static inner => inner),
        removeCase: static (remove) => Try.lift(() => Admit.Confirm(success: remove.Target.UserData.Remove(remove.Value))).Run().Bind(static inner => inner)
            .Bind(_ => remove.Disposal.Key ? Try.lift(remove.Value.Dispose).Run().Bind(static inner => inner) : Fin.Succ(value: unit)),
        purgeCase: static (purge) => Try.lift(() => purge.Target.UserData.Purge()).Run().Bind(static inner => inner),
        copyCase: static (copy) => Try.lift(() => UserData.Copy(copy.Source, copy.Destination)).Run().Bind(static inner => inner),
        moveCase: static (move) =>
            from id in Try.lift(() => Fin.Succ(value: UserData.MoveUserDataFrom(move.Source))).Run().Bind(static inner => inner)
            from _present in guard(id != Guid.Empty, new KernelFault.InvalidResult(Detail: Some("User-data move found no transferable custody.")))
            from _placed in Try.lift(() => UserData.MoveUserDataTo(move.Destination, id, move.Placement.Key)).Run().Bind(static inner => inner)
            select unit,
        replaceCase: static (replace) => Open(replace.Target)
            .Bind(opened => Reseat(opened, replace.Payload)),
        mergeCase: static (merge) => Open(merge.Target)
            .Bind(opened => ArchiveMap.Detach(opened.Dictionary)
                .Bind(current => current.Merge(merge.Payload, merge.Merge))
                .Bind(payload => Reseat(opened, payload))));

    private static Fin<Unit> Reseat(
        (CommonObject Target, ArchivableDictionary Dictionary, SharedOrigin Origin) opened,
        ArchiveMap payload) =>
        from prior in ArchiveMap.Detach(opened.Dictionary)
        from _schema in prior.Diff(payload).Map(static _ => unit)
        from settled in (
            from _clear in Try.lift(opened.Dictionary.Clear).Run().Bind(static inner => inner)
            from _write in payload.WriteTo(opened.Dictionary)
            from current in ArchiveMap.Detach(opened.Dictionary)
            from _proof in guard(
                current.SameContent(payload),
                new KernelFault.InvalidResult(Detail: Some("Shared user dictionary postcondition failed.")))
            select unit)
            .Rollback(() => RestoreShared(opened, prior))
        select settled;

    private static Fin<Unit> RestoreShared(
        (CommonObject Target, ArchivableDictionary Dictionary, SharedOrigin Origin) opened,
        ArchiveMap prior) => opened.Origin.Key
        ? from parent in Optional(opened.Dictionary.ParentUserData).ToFin(Fail: new KernelFault.InvalidResult(Detail: Some("Created shared user dictionary has no attached custody owner.")))
          from _removed in Try.lift(() => Admit.Confirm(success: opened.Target.UserData.Remove(parent))).Run().Bind(static inner => inner)
          from _released in Try.lift(parent.Dispose).Run().Bind(static inner => inner)
          select unit
        : Try.lift(opened.Dictionary.Clear).Run().Bind(static inner => inner).Bind(_ => prior.WriteTo(opened.Dictionary));

    private static Fin<(CommonObject Target, ArchivableDictionary Dictionary, SharedOrigin Origin)> Open(
        CommonObject target) =>
        Try.lift(() => {
            int before = target.UserData.Count;
            ArchivableDictionary? dictionary = target.UserDictionary;
            return dictionary is null
                ? Fin.Fail<(CommonObject, ArchivableDictionary, SharedOrigin)>(error: new KernelFault.InvalidResult(Detail: Some("Shared user dictionary could not be attached.")))
                : Fin.Succ(value: (target, dictionary, (SharedOrigin)(target.UserData.Count > before)));
        }).Run().Bind(static inner => inner);

    private static UserDataSnapshot Describe(UserData value) => new(
        value.GetType().AssemblyQualifiedName ?? value.GetType().FullName ?? value.GetType().Name,
        value.Description,
        (WritePosture)value.ShouldWrite,
        value.Transform);
}
```
