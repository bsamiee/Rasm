# [RASM_RHINO_PERSISTENCE_USERDATA]

Attached-data custody (`Rasm.Rhino.Persistence`). `ArchiveIo` owns schema-framed `ArchiveMap` IO. `TypedUserData` seals every required `UserData` override onto that seam and forces each derived type to interpret stored schema before accepting payload. `DataRoster` projects live lists and owns internal mutation adapters. `CustodyMove` closes whole-roster copy and move operations. `SharedMap` composes the lazy `CommonObject.UserDictionary` accessor without pretending its getter is pure.

## [01]-[INDEX]

- [02]-[ARCHIVE_IO]: schema identity and dictionary archive crossings.
- [03]-[DATA_TEMPLATE]: complete custom-user-data override template.
- [04]-[ROSTER_TRANSFER]: roster projection, lifecycle, transfer, and shared-map access.
- [05]-[SURFACE_LEDGER]: ownership and entry points.

## [02]-[ARCHIVE_IO]

- Owner: `ArchiveSchema` admits positive major and non-negative minor values; `ArchiveEnvelope` keeps the stored schema beside its detached payload.
- Write: schema framing and dictionary writing are separate checked legs. `WriteErrorOccured` after either leg fails the operation.
- Read: schema framing and dictionary reading are separate checked legs. `ReadErrorOccured` after either leg fails before payload admission.
- Compatibility: `ArchiveIo` never guesses version compatibility. `TypedUserData.Upgrade` receives the complete envelope and returns the current payload or a typed refusal.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [ARCHIVE_MODELS] -----------------------------------------------------------------------
public sealed record ArchiveSchema {
    private ArchiveSchema(int major, int minor) => (Major, Minor) = (major, minor);

    public int Major { get; }
    public int Minor { get; }

    public static Fin<ArchiveSchema> Create(int major, int minor, Op? key = null) {
        Op op = key.OrDefault();
        return major > 0 && minor >= 0
            ? Fin.Succ(value: new ArchiveSchema(major: major, minor: minor))
            : Fin.Fail<ArchiveSchema>(error: op.InvalidInput());
    }

    public bool Reads(ArchiveSchema stored) => stored.Major == Major && stored.Minor <= Minor;
}

public sealed record ArchiveEnvelope(ArchiveSchema Schema, ArchiveMap Payload) : IDetachedDocumentResult;

// --- [ARCHIVE_IO] ---------------------------------------------------------------------------
public static class ArchiveIo {
    public static Fin<Unit> Write(BinaryArchiveWriter archive, ArchiveSchema schema, ArchiveMap payload, Op? key = null) {
        Op op = key.OrDefault();
        return from sink in Optional(archive).ToFin(Fail: op.InvalidInput())
               from version in Optional(schema).ToFin(Fail: op.InvalidInput())
               from map in Optional(payload).ToFin(Fail: op.InvalidInput())
               from minted in map.Mint(key: op)
               from _frame in op.Catch(() => {
                   sink.Write3dmChunkVersion(major: version.Major, minor: version.Minor);
                   return sink.WriteErrorOccured
                       ? Fin.Fail<Unit>(error: op.InvalidResult(detail: nameof(BinaryArchiveWriter.WriteErrorOccured)))
                       : Fin.Succ(value: unit);
               })
               from _payload in op.Catch(() => {
                   sink.WriteDictionary(dictionary: minted);
                   return sink.WriteErrorOccured
                       ? Fin.Fail<Unit>(error: op.InvalidResult(detail: nameof(BinaryArchiveWriter.WriteErrorOccured)))
                       : Fin.Succ(value: unit);
               })
               select unit;
    }

    public static Fin<ArchiveEnvelope> Read(BinaryArchiveReader archive, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Optional(archive).ToFin(Fail: op.InvalidInput())
               from schema in op.Catch(() => {
                   source.Read3dmChunkVersion(major: out int major, minor: out int minor);
                   return source.ReadErrorOccured
                       ? Fin.Fail<ArchiveSchema>(error: op.InvalidResult(detail: nameof(BinaryArchiveReader.ReadErrorOccured)))
                       : ArchiveSchema.Create(major: major, minor: minor, key: op);
               })
               from dictionary in op.Catch(() => {
                   ArchivableDictionary read = source.ReadDictionary();
                   return source.ReadErrorOccured
                       ? Fin.Fail<ArchivableDictionary>(error: op.InvalidResult(detail: nameof(BinaryArchiveReader.ReadErrorOccured)))
                       : Optional(read).ToFin(Fail: op.InvalidResult());
               })
               from payload in ArchiveMap.Detach(source: dictionary, key: op)
               select new ArchiveEnvelope(Schema: schema, Payload: payload);
    }
}
```

## [03]-[DATA_TEMPLATE]

- Owner: `TypedUserData` seals `Description`, `ShouldWrite`, `Write`, `Read`, `OnTransform`, and `OnDuplicate`; no derived type can opt into writing without a real IO body.
- Construction: every concrete derived type remains public with a public parameterless constructor because `UserDataList.Add` enforces that exact reflection gate. Renames retain archive identity through `[ClassId("<guid>")]` on the concrete type.
- Schema: `CurrentSchema` supplies the write identity, while `Upgrade` handles every admitted stored version and returns an `ArchiveMap`. No future minor version is silently accepted by the base.
- Payload: `Option<ArchiveMap>` represents an uninitialized entry. `ShouldWrite` requires a present non-empty map, `Absorb` replaces the immutable payload, and `Clear` returns to `None`.
- Transform: sealed `OnTransform` always calls the host base so `UserData.Transform` remains current. Payload transformation remains a derived read concern rather than an event-time mutation.
- Duplication: `OnDuplicate` copies the immutable map value and preserves absence; no live `UserData` handle escapes.

```csharp signature
// --- [DATA_TEMPLATE] ------------------------------------------------------------------------
public abstract class TypedUserData : UserData {
    private Option<ArchiveMap> _payload;

    public sealed override string Description => Label;
    public sealed override bool ShouldWrite => _payload.Exists(static map => map.Count > 0);

    protected abstract string Label { get; }
    protected abstract ArchiveSchema CurrentSchema { get; }
    protected abstract Fin<ArchiveMap> Upgrade(ArchiveEnvelope stored, Op op);

    public Option<ArchiveMap> Payload => _payload;

    protected Fin<Unit> Absorb(ArchiveMap payload, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(payload).ToFin(Fail: op.InvalidInput()).Map(map => Op.Side(() => _payload = Some(map)));
    }

    protected Unit Clear() => Op.Side(() => _payload = None);

    protected sealed override bool Write(BinaryArchiveWriter archive) =>
        _payload.Match(
            Some: map => ArchiveIo.Write(
                archive: archive,
                schema: CurrentSchema,
                payload: map,
                key: Op.Of(name: Label)).IsSucc,
            None: static () => false);

    protected sealed override bool Read(BinaryArchiveReader archive) {
        Op op = Op.Of(name: Label);
        return ArchiveIo.Read(archive: archive, key: op)
            .Bind(stored => Upgrade(stored: stored, op: op))
            .Match(
                Succ: map => (Absorb(payload: map, key: op).IsSucc),
                Fail: static _ => false);
    }

    protected sealed override void OnTransform(Transform transform) => base.OnTransform(transform: transform);

    protected sealed override void OnDuplicate(UserData source) =>
        _payload = source is TypedUserData typed ? typed.Payload : None;
}
```

## [04]-[ROSTER_TRANSFER]

- Owner: `DataRoster` projects each live entry to `DataMark` — managed type, description, write opt-in, opacity, and the accumulated host `Transform` — including opaque `UnknownUserData`, and exposes typed payload lookup without returning a host handle.
- Admission: `Attach` enforces the host reflection gate — a public class with a public parameterless constructor, which host `Add` throws `ArgumentException` against — and additionally rejects abstract types that gate admits but cannot instantiate. Host `Add` detaches the entry from a previous roster before attaching it.
- Lifecycle: `Remove` requires an explicit keep-or-dispose disposition because host `UserDataList.Remove` detaches without disposing the managed/native handle. `Purge` remains an explicit destructive operation that also removes opaque data.
- Transfer: `CustodyMove` maps total dispatch onto `UserData.Copy`, `MoveUserDataFrom`, and `MoveUserDataTo`; batch execution aborts on the first failed move and returns holding ids in operation order.
- Shared map: `CommonObject.UserDictionary : ArchivableDictionary` finds the internal `SharedUserDictionary` — the one type host `Add` admits past its reflection gate — lazily creates and attaches it when absent, and returns null when `UserData.Add` fails. `Ensure` names that mutation; `Overwrite` preflights a mint before clearing and rewriting through exact codec dispatch.
- Seam: roster and shared-map mutations remain internal adapters. Document-owned objects invoke them only inside the `DocumentSession` and `Tables.Commit` mutation spine; detached objects need no session.

```csharp signature
// --- [ROSTER_MODELS] ------------------------------------------------------------------------
public readonly record struct DataMark(
    Type ManagedType,
    string Description,
    bool ShouldWrite,
    bool Opaque,
    Transform Accumulated) : IDetachedDocumentResult;

[SmartEnum<int>]
internal sealed partial class DataRelease {
    public static readonly DataRelease Keep = new(key: 0);
    public static readonly DataRelease Dispose = new(key: 1);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record CustodyMove {
    private CustodyMove() { }
    public sealed record Duplicate(CommonObject Source, CommonObject Destination) : CustodyMove;
    public sealed record Harvest(CommonObject Source) : CustodyMove;
    public sealed record Deposit(CommonObject Target, Guid HoldingId, bool Append) : CustodyMove;

    internal Fin<Option<Guid>> Apply(Op op) =>
        Switch(
            op,
            duplicate: static (key, move) =>
                from source in Optional(move.Source).ToFin(Fail: key.InvalidInput())
                from destination in Optional(move.Destination).ToFin(Fail: key.InvalidInput())
                from _ in key.Catch(() => UserData.Copy(source, destination))
                select Option<Guid>.None,
            harvest: static (key, move) =>
                from source in Optional(move.Source).ToFin(Fail: key.InvalidInput())
                from holding in key.Catch(() => Fin.Succ(value: UserData.MoveUserDataFrom(objectWithUserData: source)))
                from admitted in holding != Guid.Empty
                    ? Fin.Succ(value: Some(holding))
                    : Fin.Fail<Option<Guid>>(error: key.InvalidResult())
                select admitted,
            deposit: static (key, move) =>
                from target in Optional(move.Target).ToFin(Fail: key.InvalidInput())
                from holding in move.HoldingId != Guid.Empty
                    ? Fin.Succ(value: move.HoldingId)
                    : Fin.Fail<Guid>(error: key.InvalidInput())
                from _ in key.Catch(() => UserData.MoveUserDataTo(target, holding, move.Append))
                select Option<Guid>.None);
}

// --- [ROSTER_OPERATIONS] --------------------------------------------------------------------
public static class DataRoster {
    public static Fin<Seq<DataMark>> Census(UserDataList roster, Op? key = null) {
        Op op = key.OrDefault();
        return from list in Optional(roster).ToFin(Fail: op.InvalidInput())
               from marks in op.Catch(() => Fin.Succ(value: toSeq(list).Map(static entry => new DataMark(
                   ManagedType: entry.GetType(),
                   Description: entry.Description,
                   ShouldWrite: entry.ShouldWrite,
                   Opaque: entry is UnknownUserData,
                   Accumulated: entry.Transform))))
               select marks;
    }

    public static Fin<bool> Contains(UserDataList roster, Guid classId, Op? key = null) {
        Op op = key.OrDefault();
        return from list in Optional(roster).ToFin(Fail: op.InvalidInput())
               from id in classId != Guid.Empty ? Fin.Succ(value: classId) : Fin.Fail<Guid>(error: op.InvalidInput())
               from found in op.Catch(() => Fin.Succ(value: list.Contains(id)))
               select found;
    }

    public static Fin<Option<ArchiveMap>> PayloadOf<TData>(UserDataList roster, Op? key = null) where TData : TypedUserData {
        Op op = key.OrDefault();
        return from list in Optional(roster).ToFin(Fail: op.InvalidInput())
               from payload in op.Catch(() => Fin.Succ(value: Optional(list.Find(userdataType: typeof(TData)))
                   .Bind(static entry => entry is TypedUserData typed ? typed.Payload : None)))
               select payload;
    }

    internal static Fin<Unit> Attach(UserDataList roster, UserData entry, Op? key = null) {
        Op op = key.OrDefault();
        return from list in Optional(roster).ToFin(Fail: op.InvalidInput())
               from value in Optional(entry).ToFin(Fail: op.InvalidInput())
               let type = value.GetType()
               from _gate in guard(type.IsPublic && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) is not null, op.InvalidInput()).ToFin()
               from _add in op.Catch(() => op.Confirm(success: list.Add(userdata: value)))
               select unit;
    }

    internal static Fin<Unit> Remove(UserDataList roster, UserData entry, DataRelease release, Op? key = null) {
        Op op = key.OrDefault();
        return from list in Optional(roster).ToFin(Fail: op.InvalidInput())
               from value in Optional(entry).ToFin(Fail: op.InvalidInput())
               from disposition in Optional(release).ToFin(Fail: op.InvalidInput())
               from _remove in op.Catch(() => op.Confirm(success: list.Remove(userdata: value)))
               from _dispose in disposition == DataRelease.Dispose
                   ? op.Catch(value.Dispose)
                   : Fin.Succ(value: unit)
               select unit;
    }

    internal static Fin<Unit> Purge(UserDataList roster, Op? key = null) {
        Op op = key.OrDefault();
        return from list in Optional(roster).ToFin(Fail: op.InvalidInput())
               from _ in op.Catch(list.Purge)
               select unit;
    }
}

internal static class Custody {
    internal static Fin<Seq<Option<Guid>>> Execute(params ReadOnlySpan<CustodyMove> moves) {
        Op op = Op.Of();
        return from program in toSeq(moves.ToArray()).TraverseM(move => Optional(move).ToFin(Fail: op.InvalidInput())).As()
               from _ in guard(!program.IsEmpty, op.InvalidInput()).ToFin()
               from outcomes in program.TraverseM(move => move.Apply(op: op)).As()
               select outcomes;
    }
}

internal static class SharedMap {
    internal static Fin<ArchiveMap> Ensure(CommonObject owner, Op? key = null) {
        Op op = key.OrDefault();
        return from target in Optional(owner).ToFin(Fail: op.InvalidInput())
               from dictionary in op.Catch(() => Optional(target.UserDictionary).ToFin(Fail: op.InvalidResult()))
               from map in ArchiveMap.Detach(source: dictionary, key: op)
               select map;
    }

    internal static Fin<Unit> Overwrite(CommonObject owner, ArchiveMap payload, Op? key = null) {
        Op op = key.OrDefault();
        return from target in Optional(owner).ToFin(Fail: op.InvalidInput())
               from map in Optional(payload).ToFin(Fail: op.InvalidInput())
               from _preflight in map.Mint(key: op)
               from dictionary in op.Catch(() => Optional(target.UserDictionary).ToFin(Fail: op.InvalidResult()))
               from written in op.Catch(() => {
                   dictionary.Clear();
                   return map.Entries.AsIterable()
                       .TraverseM(pair => pair.Value.Write(target: dictionary, name: pair.Key, op: op))
                       .As()
                       .Map(static _ => unit);
               })
               select written;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]         | [FORM]                                            | [ENTRY]                             |
| :-----: | :---------------- | :-------------- | :------------------------------------------------ | :---------------------------------- |
|  [01]   | archive schema    | `ArchiveSchema` | admitted major and minor identity                 | `Create` / `Reads`                  |
|  [02]   | archive crossing  | `ArchiveIo`     | independently checked version and dictionary legs | `Write` / `Read`                    |
|  [03]   | custom data       | `TypedUserData` | sealed overrides plus required upgrade            | derive protected members            |
|  [04]   | roster query      | `DataRoster`    | detached census, presence, and typed payload      | `Census` / `Contains` / `PayloadOf` |
|  [05]   | roster mutation   | `DataRoster`    | internal attach, removal, and purge               | `Attach` / `Remove` / `Purge`       |
|  [06]   | custody transfer  | `CustodyMove`   | total copy, harvest, and deposit dispatch         | `Custody.Execute`                   |
|  [07]   | shared dictionary | `SharedMap`     | lazy host accessor plus exact typed overwrite     | `Ensure` / `Overwrite`              |
