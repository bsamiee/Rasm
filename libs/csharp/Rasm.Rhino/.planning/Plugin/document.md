# [RASM_RHINO_PLUGIN_DOCUMENT]

`Participation.Cross` carries per-plug-in document data across the host's write and read callbacks, and `PluginSettings.Commit` bridges the host's plug-in settings members onto the settings rail. Both are seams, not stores: the plug-in's archive payload is an `ArchiveMap` framed by `Persistence/userdata#ARCHIVE_FRAME`'s `ArchiveIo.Cross`, and its settings tree is addressed by `Persistence/settings#REQUEST_ALGEBRA`'s `SettingPath`, so plug-in document state rides the same schema, integrity, and mutation discipline as every other crossing in the boundary.

`lifecycle#ADAPTER` seats the three host overrides and hands them here as a `ParticipationAsk`; the live `RhinoDoc`, `BinaryArchiveWriter`, `BinaryArchiveReader`, `FileWriteOptions`, and `FileReadOptions` handles terminate at that boundary and a program signature sees only `DocKey`, the detached intent, and the archive payload. `ArchiveSchema` is the program's own chunk typecode and readable-version set, so a plug-in that widens its payload declares a version rather than re-deriving a framing.

## [01]-[INDEX]

- [02]-[INTENT]: `WriteToggle`, `ReadToggle`, `WriteIntent`, and `ReadIntent` detach the host's dispatch options before any program sees them.
- [03]-[PROGRAM]: `ParticipationProgram` binds the schema, the write predicate, the payload composer, and the payload adopter as one admitted value.
- [04]-[CROSSING]: `ParticipationAsk`, `ParticipationAnswer`, and `Participation.Cross` run the three callbacks over `ArchiveIo.Cross`.
- [05]-[SETTINGS]: `SettingsLoad`, `SettingsBridge`, `SettingsBridgeAnswer`, and `PluginSettings.Commit` bridge the plug-in settings members onto the settings rail.

## [02]-[INTENT]

- Owner: `WriteToggle` and `ReadToggle` are keyed vocabularies over the host's boolean dispatch flags, each row carrying its own read; `WriteIntent` and `ReadIntent` carry the true rows as a set beside the scalar coordinates.
- Law: a boolean flag is a ROW, never a record field — twenty parallel `bool` members would make every consumer positional and every new host flag a record signature change, while a row is one line and every existing consumer keeps compiling.
- Law: the per-format option payload crosses as `ArchiveMap`, so the dialog lane's `ArchivableDictionary` is detached once here and never handed on live.
- Boundary: `FileWriteOptions.RhinoDoc` is not detached into the intent — the crossing already carries the document as a `DocKey`, and a second document coordinate could disagree with it.
- Boundary: `GetFileName()` is not detached either; `DestinationFileName` is the declared target and the host's derived name is a presentation of it.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.PlugIns;

namespace Rasm.Rhino.Plugin;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class WriteToggle {
    public static readonly WriteToggle UpdateDocumentPath = new("update-document-path", reads: static o => o.UpdateDocumentPath);
    public static readonly WriteToggle SelectedOnly = new("selected-only", reads: static o => o.WriteSelectedObjectsOnly);
    public static readonly WriteToggle RenderMeshes = new("render-meshes", reads: static o => o.IncludeRenderMeshes);
    public static readonly WriteToggle PreviewImage = new("preview-image", reads: static o => o.IncludePreviewImage);
    public static readonly WriteToggle BitmapTable = new("bitmap-table", reads: static o => o.IncludeBitmapTable);
    public static readonly WriteToggle History = new("history", reads: static o => o.IncludeHistory);
    public static readonly WriteToggle AsTemplate = new("as-template", reads: static o => o.WriteAsTemplate);
    public static readonly WriteToggle SuppressDialogs = new("suppress-dialogs", reads: static o => o.SuppressDialogBoxes);
    public static readonly WriteToggle SuppressInput = new("suppress-input", reads: static o => o.SuppressAllInput);
    public static readonly WriteToggle HeadlessUi = new("headless-ui", reads: static o => o.AllowUserInterfaceWithHeadlessDocument);
    public static readonly WriteToggle GeometryOnly = new("geometry-only", reads: static o => o.WriteGeometryOnly);
    public static readonly WriteToggle UserData = new("user-data", reads: static o => o.WriteUserData);
    public static readonly WriteToggle BackupFiles = new("backup-files", reads: static o => o.CreateBackupFiles);
    public static readonly WriteToggle OtherBackupFiles = new("other-backup-files", reads: static o => o.CreateOtherBackupFiles);
    public static readonly WriteToggle Compression = new("compression", reads: static o => o.UseCompression);
    public static readonly WriteToggle Is3dm = new("is-3dm", reads: static o => o.FileTypeIs3dm);
    public static readonly WriteToggle IsComplete3dm = new("is-complete-3dm", reads: static o => o.FileTypeIsComplete3dm);

    [UseDelegateFromConstructor]
    internal partial bool Reads(FileWriteOptions value);
}

[SmartEnum<string>]
public sealed partial class ReadToggle {
    public static readonly ReadToggle Import = new("import", reads: static o => o.ImportMode);
    public static readonly ReadToggle Open = new("open", reads: static o => o.OpenMode);
    public static readonly ReadToggle New = new("new", reads: static o => o.NewMode);
    public static readonly ReadToggle Insert = new("insert", reads: static o => o.InsertMode);
    public static readonly ReadToggle ImportReference = new("import-reference", reads: static o => o.ImportReferenceMode);
    public static readonly ReadToggle Batch = new("batch", reads: static o => o.BatchMode);
    public static readonly ReadToggle UseScale = new("use-scale", reads: static o => o.UseScaleGeometry);
    public static readonly ReadToggle Scale = new("scale", reads: static o => o.ScaleGeometry);

    [UseDelegateFromConstructor]
    internal partial bool Reads(FileReadOptions value);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record WriteIntent(
    FrozenSet<WriteToggle> Toggles,
    int FileVersion,
    int Rhino3dmVersion,
    int TypeIndex,
    Option<Guid> TypeId,
    Option<string> Destination,
    Option<string> BackupFolder,
    Transform Placement,
    ArchiveMap Options) {
    public bool Holds(WriteToggle toggle) => Toggles.Contains(toggle);

    internal static Fin<WriteIntent> Detach(FileWriteOptions options, Op op) =>
        from row in op.Need(options)
        from payload in op.Catch(() => ArchiveMap.Detach(row.OptionsDictionary, op))
        from intent in op.Catch(() => Fin.Succ(value: new WriteIntent(
            Toggles: toSeq(WriteToggle.Items).Filter(toggle => toggle.Reads(row)).ToFrozenSet(),
            FileVersion: row.FileVersion,
            Rhino3dmVersion: row.Rhino3dmVersion,
            TypeIndex: row.FileTypeIndex,
            TypeId: Optional(row.FileTypeId).Filter(static value => value != Guid.Empty),
            Destination: Op.Text(row.DestinationFileName),
            BackupFolder: Op.Text(row.BackupFileFolder),
            Placement: row.Xform,
            Options: payload)))
        select intent;
}

public sealed record ReadIntent(
    FrozenSet<ReadToggle> Toggles,
    uint WorkSessionReference,
    uint LinkedDefinition,
    Option<Guid> ReferenceGrandParentLayer,
    ArchiveMap Options) {
    public bool Holds(ReadToggle toggle) => Toggles.Contains(toggle);

    internal static Fin<ReadIntent> Detach(FileReadOptions options, Op op) =>
        from row in op.Need(options)
        from payload in op.Catch(() => ArchiveMap.Detach(row.OptionsDictionary, op))
        from intent in op.Catch(() => Fin.Succ(value: new ReadIntent(
            Toggles: toSeq(ReadToggle.Items).Filter(toggle => toggle.Reads(row)).ToFrozenSet(),
            WorkSessionReference: row.WorkSessionReferenceModelSerialNumber,
            LinkedDefinition: row.LinkedInstanceDefinitionSerialNumber,
            ReferenceGrandParentLayer: Optional(row.ReferenceModelGrandParentLayerId).Filter(static value => value != Guid.Empty),
            Options: payload)))
        select intent;
}
```

## [03]-[PROGRAM]

- Owner: `ParticipationProgram` is the plug-in's whole document-participation declaration — one `ArchiveSchema`, one write predicate, one payload composer, one payload adopter.
- Law: the predicate is DECLARED, not derived — the host asks `ShouldCallWriteDocument` before it asks for bytes, so a program that answers false there must never be asked to compose, and a composer that refuses after a true predicate is a program fault, not a host one.
- Law: `ArchiveSchema` belongs to the program, so schema versioning is per-plug-in: the current version is what a write stamps, the readable set is what a read accepts, and `ArchiveIo` refuses an out-of-set frame before any payload is detached.
- Boundary: the adopter receives the whole `ArchiveEnvelope` — payload beside `ArchiveIntegrity.ReadCase` — so a consumer that cares about checksum or archive version reads its own evidence rather than trusting an unverified payload.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ParticipationProgram {
    public ArchiveSchema Schema { get; }
    public Func<WriteIntent, bool> Declares { get; }
    public Func<DocKey, WriteIntent, Fin<ArchiveMap>> Compose { get; }
    public Func<DocKey, ReadIntent, ArchiveEnvelope, Fin<Unit>> Adopt { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ArchiveSchema schema,
        ref Func<WriteIntent, bool> declares,
        ref Func<DocKey, WriteIntent, Fin<ArchiveMap>> compose,
        ref Func<DocKey, ReadIntent, ArchiveEnvelope, Fin<Unit>> adopt) =>
        validationError = schema is null || declares is null || compose is null || adopt is null
            ? new ValidationError(message: "Participation program is incomplete.")
            : null;
}
```

## [04]-[CROSSING]

- Entry: `Participation.Cross(ParticipationAsk, Op?)` is the one document-participation entry; the three host callbacks are three cases on it.
- Law: every case detaches its intent BEFORE the program runs, so a program never observes a live host options object and a detach fault refuses the crossing rather than half-composing it.
- Law: the write case composes the payload first and frames it second, so a composer refusal never opens a chunk the boundary would then have to abandon mid-frame.
- Law: the read case adopts AFTER `ArchiveIo` has proved schema readability, checksum, and reader state, so a program never sees a payload the frame did not verify.
- Receipt: both crossings answer their `ArchiveIntegrity` evidence, so a caller records what the archive actually reported rather than inferring success from the absence of a fault.
- Boundary: this rail runs inside the host's own save and open sequence and opens no document session; the document is already the host's and the crossing mutates no table.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParticipationAsk {
    private ParticipationAsk() { }
    public sealed record Declared(ParticipationProgram Program, FileWriteOptions Options) : ParticipationAsk;
    public sealed record WriteCase(
        ParticipationProgram Program,
        RhinoDoc Document,
        BinaryArchiveWriter Writer,
        FileWriteOptions Options) : ParticipationAsk;
    public sealed record ReadCase(
        ParticipationProgram Program,
        RhinoDoc Document,
        BinaryArchiveReader Reader,
        FileReadOptions Options) : ParticipationAsk;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParticipationAnswer {
    private ParticipationAnswer() { }
    public sealed record DeclaredCase(bool Writes) : ParticipationAnswer;
    public sealed record WrittenCase(ArchiveIntegrity.WrittenCase Integrity) : ParticipationAnswer;
    public sealed record ReadCase(ArchiveIntegrity.ReadCase Integrity) : ParticipationAnswer;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Participation {
    public static Fin<ParticipationAnswer> Cross(ParticipationAsk ask, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(ask).Bind(request => request.Switch(
            op,
            declared: static (held, row) =>
                from program in held.Need(row.Program)
                from intent in WriteIntent.Detach(row.Options, held)
                from writes in held.Catch(() => Fin.Succ(value: program.Declares(arg: intent)))
                select (ParticipationAnswer)new ParticipationAnswer.DeclaredCase(Writes: writes),
            writeCase: static (held, row) =>
                from program in held.Need(row.Program)
                from document in DocKey.Of(document: row.Document, key: held)
                from intent in WriteIntent.Detach(row.Options, held)
                from payload in held.Catch(() => program.Compose(arg1: document, arg2: intent))
                from crossed in ArchiveIo.Cross(
                    exchange: new ArchiveExchange.WriteCase(
                        Writer: row.Writer,
                        Schema: program.Schema,
                        Payload: payload),
                    key: held)
                from integrity in crossed is ArchiveExchangeResult.WrittenCase written
                    ? Fin.Succ(value: written.Integrity)
                    : Fin.Fail<ArchiveIntegrity.WrittenCase>(error: held.InvalidResult(detail: nameof(ArchiveIo)))
                select (ParticipationAnswer)new ParticipationAnswer.WrittenCase(Integrity: integrity),
            readCase: static (held, row) =>
                from program in held.Need(row.Program)
                from document in DocKey.Of(document: row.Document, key: held)
                from intent in ReadIntent.Detach(row.Options, held)
                from crossed in ArchiveIo.Cross(
                    exchange: new ArchiveExchange.ReadCase(Reader: row.Reader, Schema: program.Schema),
                    key: held)
                from envelope in crossed is ArchiveExchangeResult.ReadCase read
                    ? Fin.Succ(value: read.Envelope)
                    : Fin.Fail<ArchiveEnvelope>(error: held.InvalidResult(detail: nameof(ArchiveIo)))
                from _ in held.Catch(() => program.Adopt(arg1: document, arg2: intent, arg3: envelope))
                select (ParticipationAnswer)new ParticipationAnswer.ReadCase(Integrity: envelope.Integrity)));
    }
}
```

## [05]-[SETTINGS]

- Owner: `SettingsBridge` closes the host's plug-in settings members; every read and write of a value inside the resolved node is `Persistence/settings#INTERPRETER`'s `Settings.Commit`, and this bridge answers only the address, the persist, and the observation.
- Law: `SettingsLoad.Forced` LOADS the plug-in as a side effect — `GetPluginSettings(id, load: true)` calls `LoadPlugIn` and runs its `OnLoad` when the settings are absent — so the row names a lifecycle act and `Deferred` is the read-only default.
- Law: `SavePluginSettings` is silent on an unknown plug-in, so the persist case proves presence through `census#CENSUS` first and refuses typed rather than reporting a write the host never made.
- Law: observation is `Settings.Observe` over `PlugIn.SettingsSaved` — the instance event on the owning derivation — so this page mints no second event lifecycle and the `SavedSettingsRoot` vocabulary is composed, never re-declared.
- Law: the two drain cases are distinct host queues — `FlushSettingsSavedQueue` drains the saved-event queue and `RaiseOnPlugInSettingsSavedEvent` drains the changed-event queue — so one case cannot stand for both.
- Boundary: a command-scoped node has no `SettingPath` — `SettingsRoot` addresses the application and plug-in roots alone — so command settings reach only through `SavedSettingsRoot.CommandCase` observation, and an imperative command-node path widens `SettingsRoot` at its owner rather than seating a second addressing family here.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class SettingsLoad {
    public static readonly SettingsLoad Deferred = new(false);
    public static readonly SettingsLoad Forced = new(true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsBridge {
    private SettingsBridge() { }
    public sealed record Root(PluginKey Plugin, SettingsLoad Load, Seq<SettingKey> Children) : SettingsBridge;
    public sealed record Persist(PluginKey Plugin) : SettingsBridge;
    public sealed record PersistOwner(RasmPlugIn Owner) : SettingsBridge;
    public sealed record Watch(
        RasmPlugIn Owner,
        SavedSettingsRoot Source,
        SettingPath Path,
        Action<Fin<SettingsTree>> Sink) : SettingsBridge;
    public sealed record DrainSaved : SettingsBridge;
    public sealed record DrainChanged : SettingsBridge;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsBridgeAnswer {
    private SettingsBridgeAnswer() { }
    public sealed record PathCase(SettingPath Path) : SettingsBridgeAnswer;
    public sealed record PersistedCase(PluginKey Plugin) : SettingsBridgeAnswer;
    public sealed record WatchCase(Subscription Watch) : SettingsBridgeAnswer;
    public sealed record DrainedCase : SettingsBridgeAnswer;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PluginSettings {
    public static Fin<SettingsBridgeAnswer> Commit(SettingsBridge bridge, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(bridge).Bind(request => request.Switch(
            op,
            // The host answers null for an unloaded plug-in under `Deferred`; that absence is the whole point of
            // the member, so it refuses here and the caller either forces the load or asks the census instead.
            // `Children` rows are already admitted `SettingKey` values, so admission here is null-absence alone.
            root: static (held, row) =>
                from _ in row.Plugin.Admit(held)
                from load in held.Need(row.Load)
                from __ in guard(row.Children.ForAll(static child => child is not null), held.InvalidInput()).ToFin()
                from ___ in held.Catch(() => Optional(PlugIn.GetPluginSettings(plugInId: row.Plugin.ToValue(), load: load.Key))
                    .ToFin(Fail: held.MissingContext()))
                select (SettingsBridgeAnswer)new SettingsBridgeAnswer.PathCase(Path: new SettingPath(
                    Root: new SettingsRoot.PlugInCase(PlugInId: row.Plugin.ToValue()),
                    Children: row.Children.Strict())),
            persist: static (held, row) =>
                from _ in row.Plugin.Admit(held)
                from answer in PluginCensus.Ask(query: new PluginQuery.Presence(Plugin: row.Plugin), key: held)
                from presence in answer is PluginAnswer.Presence found
                    ? Fin.Succ(value: found.Value)
                    : Fin.Fail<PluginPresence>(error: held.InvalidResult(detail: nameof(PluginPresence)))
                from __ in guard(presence.Installed && presence.Loaded, held.MissingContext()).ToFin()
                from ___ in held.Catch(() => {
                    PlugIn.SavePluginSettings(plugInId: row.Plugin.ToValue());
                    return Fin.Succ(value: unit);
                })
                select (SettingsBridgeAnswer)new SettingsBridgeAnswer.PersistedCase(Plugin: row.Plugin),
            persistOwner: static (held, row) =>
                from owner in held.Need(row.Owner)
                from plugin in held.AcceptValidated<PluginKey>(owner.Id)
                from _ in held.Catch(() => {
                    owner.SaveSettings();
                    return Fin.Succ(value: unit);
                })
                select (SettingsBridgeAnswer)new SettingsBridgeAnswer.PersistedCase(Plugin: plugin),
            watch: static (held, row) =>
                from _ in guard(
                    row.Owner is not null && row.Source is not null && row.Path is not null && row.Sink is not null,
                    held.InvalidInput()).ToFin()
                from subscription in Settings.Observe(
                    plugIn: row.Owner,
                    source: row.Source,
                    path: row.Path,
                    sink: row.Sink,
                    key: held)
                select (SettingsBridgeAnswer)new SettingsBridgeAnswer.WatchCase(Watch: subscription),
            drainSaved: static (held, _) => held.Catch(() => {
                PlugIn.FlushSettingsSavedQueue();
                return Fin.Succ<SettingsBridgeAnswer>(value: new SettingsBridgeAnswer.DrainedCase());
            }),
            drainChanged: static (held, _) => held.Catch(() => {
                PlugIn.RaiseOnPlugInSettingsSavedEvent();
                return Fin.Succ<SettingsBridgeAnswer>(value: new SettingsBridgeAnswer.DrainedCase());
            })));
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
