# [RASM_RHINO_PLUGIN_DOCUMENT]

`Participation.Cross` carries per-plug-in document data across the host's write and read callbacks, and `PluginSettings.Commit` bridges the host's plug-in settings members onto the settings pipeline. Both are boundaries, not stores: the plug-in's archive payload is an `ArchiveMap` framed by `Persistence/userdata#ARCHIVE_FRAME`'s `ArchiveIo.Cross`, and its settings tree is addressed by `Persistence/settings#REQUEST_ALGEBRA`'s `SettingPath`, so plug-in document state rides the same schema, integrity, and mutation discipline as every other crossing in the boundary.

`lifecycle#ADAPTER` seats the three host overrides and hands them here as a `ParticipationAsk`; the live `RhinoDoc`, `BinaryArchiveWriter`, `BinaryArchiveReader`, `FileWriteOptions`, and `FileReadOptions` handles terminate at that boundary and a participant signature sees only `DocKey`, the detached intent, and the archive payload. `lifecycle#LOAD_ROOT` is the settings bridge's one caller, resolving the plug-in settings node at plug-in load from the children a program declares. `ArchiveSchema` belongs to the participant, so a plug-in that widens its payload declares a version rather than re-deriving a framing.

## [01]-[INDEX]

- [02]-[INTENT]: `WriteToggle`, `ReadToggle`, `WriteIntent`, and `ReadIntent` detach the host's dispatch options before any participant sees them.
- [03]-[PROGRAM]: `IParticipant` is the open floor a foreign plug-in implements — schema, write predicate, payload composer, payload adopter.
- [04]-[CROSSING]: `ParticipationAsk`, `ParticipationAnswer`, and `Participation.Cross` run the three callbacks over `ArchiveIo.Cross`.
- [05]-[SETTINGS]: `SettingsLoad`, `SettingsQueue`, `SettingsBridge`, `SettingsBridgeAnswer`, and `PluginSettings.Commit` bridge the plug-in settings members onto the settings pipeline.
- [06]-[SURFACE_LEDGER]: owner-to-ingress-to-state-to-egress roster across the crossing entry, the settings entry, and the intent vocabularies.

## [02]-[INTENT]

- Owner: `WriteToggle` and `ReadToggle` are keyed vocabularies over the host's boolean dispatch flags, each row carrying its own read and realizing `ICapability<TSelf>`; `WriteIntent` and `ReadIntent` carry the true rows as a kernel `CapabilitySet<T>` beside the scalar coordinates.
- Law: a boolean flag is a ROW, never a record field — twenty-five parallel `bool` members would make every consumer positional and every new host flag a record signature change, while a row is one line and every existing consumer keeps compiling.
- Law: the set is DERIVED by sweeping `Items` against a live host record, so there is no admission moment and the capability law is `Open`; membership reads `Admits`, subset reads `AdmitsAll`, and the canonical text reads `Wire`, none of which a hand `Holds` member could offer.
- Law: canonical intent text is the kernel's ordinal key projection, so neither toggle roster depends on source position or carries an order column it does not semantically own.
- Law: the per-format option payload crosses as `ArchiveMap`, so the dialog lane's `ArchivableDictionary` is detached once here and never handed on live.
- Boundary: `FileWriteOptions.RhinoDoc` is not detached into the intent — the crossing already carries the document as a `DocKey`, and a second document coordinate could disagree with it.
- Boundary: `GetFileName()` is not detached either; `DestinationFileName` is the declared target and the host's derived name is a presentation of it.
- Boundary: `SuppressDialogBoxes`, `SuppressAllInput`, and `AllowUserInterfaceWithHeadlessDocument` are host FILE-OPTIONS facts read at a save or open boundary, not interaction policy — they stay rows on this boundary and reach no kernel interaction owner.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<string>]`, `[UseDelegateFromConstructor]`, `[KeyMemberEqualityComparer<TAccessor, TKey>]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`), `Domain/results` (`Admit.Need`, `Try.lift`, `HostEdge.Text`); `Persistence/dictionary` (`ArchiveMap.Detach`); RhinoCommon file I/O (`Rasm.Rhino/.api/api-rhinocommon-fileio.md` — the seventeen `FileWriteOptions` and eight `FileReadOptions` reads, `ArchivableDictionary`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.PlugIns;

namespace Rasm.Rhino.Plugin;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WriteToggle : ICapability<WriteToggle> {
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReadToggle : ICapability<ReadToggle> {
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record WriteIntent(
    CapabilitySet<WriteToggle> Toggles,
    int FileVersion,
    int Rhino3dmVersion,
    int TypeIndex,
    Option<Guid> TypeId,
    Option<string> Destination,
    Option<string> BackupFolder,
    Transform Placement,
    ArchiveMap Options) {
    internal static Fin<WriteIntent> Detach(FileWriteOptions options) =>
        from row in Admit.Need(options)
        from payload in Try.lift(() => ArchiveMap.Detach(row.OptionsDictionary)).Run().Bind(static inner => inner)
        from intent in Try.lift(() => Fin.Succ(value: new WriteIntent(
            Toggles: CapabilitySet<WriteToggle>.Of(toSeq(WriteToggle.Items)
                .Filter(toggle => toggle.Reads(value: row))
                .ToArray()),
            FileVersion: row.FileVersion,
            Rhino3dmVersion: row.Rhino3dmVersion,
            TypeIndex: row.FileTypeIndex,
            TypeId: Held(row.FileTypeId),
            Destination: HostEdge.Text(row.DestinationFileName),
            BackupFolder: HostEdge.Text(row.BackupFileFolder),
            Placement: row.Xform,
            Options: payload))).Run().Bind(static inner => inner)
        select intent;

    internal static Option<Guid> Held(Guid value) => Optional(value).Filter(static id => id != Guid.Empty);
}

public sealed record ReadIntent(
    CapabilitySet<ReadToggle> Toggles,
    uint WorkSessionReference,
    uint LinkedDefinition,
    Option<Guid> ReferenceGrandParentLayer,
    ArchiveMap Options) {
    internal static Fin<ReadIntent> Detach(FileReadOptions options) =>
        from row in Admit.Need(options)
        from payload in Try.lift(() => ArchiveMap.Detach(row.OptionsDictionary)).Run().Bind(static inner => inner)
        from intent in Try.lift(() => Fin.Succ(value: new ReadIntent(
            Toggles: CapabilitySet<ReadToggle>.Of(toSeq(ReadToggle.Items)
                .Filter(toggle => toggle.Reads(value: row))
                .ToArray()),
            WorkSessionReference: row.WorkSessionReferenceModelSerialNumber,
            LinkedDefinition: row.LinkedInstanceDefinitionSerialNumber,
            ReferenceGrandParentLayer: WriteIntent.Held(row.ReferenceModelGrandParentLayerId),
            Options: payload))).Run().Bind(static inner => inner)
        select intent;
}
```

## [03]-[PROGRAM]

- Owner: `IParticipant` is the plug-in's whole document-participation declaration behind an instance-interface floor — one `ArchiveSchema`, one write predicate, one payload composer, one payload adopter.
- Law: the implementation is FOREIGN code, so the extension point is a floor a plug-in implements, not a bag of delegate columns a null guard re-tests at construction; a non-null implementation is what the type states and the guard has no spelling left (`surfaces-and-dispatch.md [OPEN_FLOOR_DISPATCH]`, folder-wide with `licensing#ACQUISITION`'s `ILicenseProgram`).
- Law: `Declares` answers `Fin<bool>` — the host asks `ShouldCallWriteDocument` before it asks for bytes, and a participant that cannot decide must be able to REFUSE rather than have its throw become an unattributable crossing fault; a participant answering false there is then never asked to compose, and a composer that refuses after a true predicate is a participant fault, not a host one.
- Law: `ArchiveSchema` belongs to the participant, so schema versioning is per-plug-in: the current version is what a write stamps, the readable set is what a read accepts, and `ArchiveIo` refuses an out-of-set frame before any payload is detached.
- Boundary: the adopter receives the whole `ArchiveEnvelope` — payload beside `ArchiveIntegrity.ReadCase` — so a consumer that cares about checksum or archive version reads its own evidence rather than trusting an unverified payload.
- Packages: LanguageExt.Core (`Fin`, `Unit`); kernel `Domain/results` ; `Persistence/userdata` (`ArchiveSchema`, `ArchiveEnvelope`), `Persistence/dictionary` (`ArchiveMap`); `Document/session` (`DocKey`).

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
public interface IParticipant {
    ArchiveSchema Schema { get; }
    Fin<bool> Declares(WriteIntent intent);
    Fin<ArchiveMap> Compose(DocKey document, WriteIntent intent);
    Fin<Unit> Adopt(DocKey document, ReadIntent intent, ArchiveEnvelope envelope);
}
```

## [04]-[CROSSING]

- Entry: `Participation.Cross(ParticipationAsk)` is the one document-participation entry; the three host callbacks are three cases on it.
- Law: every case detaches its intent BEFORE the participant runs, so a participant never observes a live host options object and a detach fault refuses the crossing rather than half-composing it.
- Law: the write case composes the payload first and frames it second, so a composer refusal never opens a chunk the boundary would then have to abandon mid-frame.
- Law: the read case adopts AFTER `ArchiveIo` has proved schema readability, checksum, and reader state, so a participant never sees a payload the frame did not verify.
- Law: the exchange result is read through its own total dispatch — the caller knows which direction it asked for, but the union answers both, so the mismatched arm refuses by CASE rather than by an `is`-probe with a fall-through a third direction would silently take.
- Law: the participant's own members are already carried, so each call crosses through `Try.lift` alone and no arm re-wraps an existing `Fin` in a second success.
- Output: both crossings answer their `ArchiveIntegrity` evidence, so a caller records what the archive actually reported rather than inferring success from the absence of a fault.
- Boundary: the `ParticipationAsk` cases carry LIVE `RhinoDoc`, `BinaryArchiveWriter`, `BinaryArchiveReader`, and file-options handles by design — this pipeline is the boundary where `lifecycle#ADAPTER` terminates them, which is why the detach-first law lives INSIDE the arms rather than at the case constructors.
- Boundary: this pipeline runs inside the host's own save and open sequence and opens no document session; the document is already the host's and the crossing mutates no table. No deferral and no re-drive is expressible here — the host callback owns the thread and a retried write reopens a chunk the host already closed.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` with the generated total `Switch`); LanguageExt.Core (`Fin`, `Seq`); kernel `Domain/results` (`Admit.Need`, `Try.lift`); `Persistence/userdata` (`ArchiveIo.Cross`, `ArchiveIntegrity`, `ArchiveEnvelope`); `Document/session` (`DocKey.Of`); RhinoCommon file I/O (`.api/api-rhinocommon-fileio.md` — `BinaryArchiveWriter`, `BinaryArchiveReader`, `FileWriteOptions`, `FileReadOptions`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParticipationAsk {
    private ParticipationAsk() { }
    public sealed record Declared(IParticipant Participant, FileWriteOptions Options) : ParticipationAsk;
    public sealed record WriteCase(
        IParticipant Participant,
        RhinoDoc Document,
        BinaryArchiveWriter Writer,
        FileWriteOptions Options) : ParticipationAsk;
    public sealed record ReadCase(
        IParticipant Participant,
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Participation {
    public static Fin<ParticipationAnswer> Cross(ParticipationAsk ask) {
        return Admit.Need(ask).Bind(request => request.Switch(declared: static row =>
                from participant in Admit.Need(row.Participant)
                from intent in WriteIntent.Detach(row.Options)
                from writes in Try.lift(() => participant.Declares(intent: intent)).Run().Bind(static inner => inner)
                select (ParticipationAnswer)new ParticipationAnswer.DeclaredCase(Writes: writes),
            writeCase: static row =>
                from participant in Admit.Need(row.Participant)
                from document in DocKey.Of(document: row.Document)
                from intent in WriteIntent.Detach(row.Options)
                from payload in Try.lift(() => participant.Compose(document: document, intent: intent)).Run().Bind(static inner => inner)
                from integrity in ArchiveIo.Cross(
                    archive: row.Writer,
                    schema: participant.Schema,
                    payload: payload)
                select (ParticipationAnswer)new ParticipationAnswer.WrittenCase(Integrity: integrity),
            readCase: static row =>
                from participant in Admit.Need(row.Participant)
                from document in DocKey.Of(document: row.Document)
                from intent in ReadIntent.Detach(row.Options)
                from envelope in ArchiveIo.Cross(
                    archive: row.Reader,
                    schema: participant.Schema)
                from _ in Try.lift(() => participant.Adopt(
                    document: document, intent: intent, envelope: envelope)).Run().Bind(static inner => inner)
                select (ParticipationAnswer)new ParticipationAnswer.ReadCase(Integrity: envelope.Integrity)));
    }
}
```

## [05]-[SETTINGS]

- Owner: `SettingsBridge` closes the host's plug-in settings members; every read and write of a value inside the resolved node is `Persistence/settings#INTERPRETER`'s `SettingStore.Commit`, and this bridge answers only the address, the persist, and the observation.
- Law: `SettingsLoad.Forced` LOADS the plug-in as a side effect — `GetPluginSettings(id, load: true)` calls `LoadPlugIn` and runs its `OnLoad` when the settings are absent — so the row names a lifecycle act and `Deferred` is the read-only default `lifecycle#LOAD_ROOT` asks with.
- Law: `SavePluginSettings` is silent on an unknown plug-in, so the persist case proves presence through `census#CENSUS` first and refuses typed rather than reporting a write the host never made; presence is a CASE, so "installed and loaded" is one `Present` match with one `Admits`, not two boolean reads that admit the corner where an uninstalled plug-in reads as loaded.
- Law: the two drain cases are distinct host queues — `FlushSettingsSavedQueue` drains the saved-event queue and `RaiseOnPlugInSettingsSavedEvent` drains the changed-event queue — so `SettingsQueue` is one row per queue, one case carries it, and the answer names the queue it drained (folder RULINGS `[02]`).
- Law: the owner-addressed persist and watch take the HOST `PlugIn`, not the derivation — `PlugIn.SaveSettings`, `PlugIn.Id`, and `SettingStore.Observe` are all base members, and narrowing to `RasmPlugIn` would couple this boundary to the derivation while adding no capability.
- Law: observation is `SettingStore.Observe` over `PlugIn.SettingsSaved` — the instance event on the owning derivation — so this page mints no second event lifecycle and the `SavedSettingsRoot` vocabulary is composed, never re-declared. The sink runs on the host's own settings-saved thread and no marshal is minted here.
- Law: independent columns ACCUMULATE — the watch case reports every absent argument at once through the applicative join rather than folding four probes into one message a caller cannot decode.
- Boundary: this bridge addresses the plug-in root alone. `SettingsRoot` also carries a command root keyed on the host `Rhino.Commands.Command` instance, which a command holds for itself, while `SavedSettingsRoot.CommandCase` names the command for observation — both live at their owner and this page seats no second addressing family.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<bool>]`, `[SmartEnum<string>]` with `[UseDelegateFromConstructor]`, `[Union]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Validation` tuple `.Apply`); kernel `Domain/results` (`Admit.Need`, `Try.lift`, `FactoryBridge.Accept`); `Persistence/settings` (`SettingKey`, `SettingPath`, `SettingsRoot.PlugInCase`, `SavedSettingsRoot`, `SettingsTree`, `SettingsSaved`, `SettingStore.Observe`), `Document/lifetime` (`Subscription`); `Plugin/census` (`PluginCensus.Ask`, `PluginQuery.Keyed`, `PluginRead.Presence`, `PluginPresence`, `PluginState`); RhinoCommon plug-ins (`.api/api-rhinocommon-plugins.md` — `GetPluginSettings`, `SavePluginSettings`, `SaveSettings`, `FlushSettingsSavedQueue`, `RaiseOnPlugInSettingsSavedEvent`, `SettingsSaved`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public sealed partial class SettingsLoad {
    public static readonly SettingsLoad Deferred = new(false);
    public static readonly SettingsLoad Forced = new(true);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SettingsQueue {
    public static readonly SettingsQueue Saved = new("saved", drain: PlugIn.FlushSettingsSavedQueue);
    public static readonly SettingsQueue Changed = new("changed", drain: PlugIn.RaiseOnPlugInSettingsSavedEvent);

    [UseDelegateFromConstructor] internal partial void Drain();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsBridge {
    private SettingsBridge() { }
    public sealed record Root(PluginKey Plugin, SettingsLoad Load, Seq<SettingKey> Children) : SettingsBridge;
    public sealed record Persist(PluginKey Plugin) : SettingsBridge;
    public sealed record PersistOwner(PlugIn Owner) : SettingsBridge;
    public sealed record Watch(
        PlugIn Owner,
        SavedSettingsRoot Source,
        SettingPath Path,
        Action<Fin<SettingsSaved>> Sink) : SettingsBridge;
    public sealed record Drain(SettingsQueue Queue) : SettingsBridge;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsBridgeAnswer {
    private SettingsBridgeAnswer() { }
    public sealed record PathCase(SettingPath Path) : SettingsBridgeAnswer;
    public sealed record PersistedCase(PluginKey Plugin) : SettingsBridgeAnswer;
    public sealed record WatchCase(Subscription Watch) : SettingsBridgeAnswer;
    public sealed record DrainedCase(SettingsQueue Queue) : SettingsBridgeAnswer;

    public Fin<SettingPath> Path() => Switch(pathCase: static row => Fin.Succ(value: row.Path),
        persistedCase: static _ => Elsewhere(),
        watchCase: static _ => Elsewhere(),
        drainedCase: static _ => Elsewhere());

    private static Fin<SettingPath> Elsewhere() => Fin.Fail<SettingPath>(
        error: new KernelFault.InvalidValue(nameof(SettingPath), "a root request"));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PluginSettings {
    public static Fin<SettingsBridgeAnswer> Commit(SettingsBridge bridge) {
        return Admit.Need(bridge).Bind(request => request.Switch(root: static row =>
                from _ in row.Plugin.Admit()
                from load in Admit.Need(row.Load)
                from __ in Try.lift(() => Optional(PlugIn.GetPluginSettings(
                        plugInId: row.Plugin.ToValue(), load: load))
                    .ToFin(Fail: new PluginFault.Unbound(Key: held, Member: nameof(PlugIn.GetPluginSettings)))).Run().Bind(static inner => inner)
                select (SettingsBridgeAnswer)new SettingsBridgeAnswer.PathCase(Path: new SettingPath(
                    Root: new SettingsRoot.PlugInCase(Plugin: row.Plugin),
                    Children: row.Children.Strict())),
            persist: static row =>
                from _ in row.Plugin.Admit()
                from answer in PluginCensus.Ask(
                    query: new PluginQuery.Keyed(Read: PluginRead.Presence, Plugin: row.Plugin))
                from presence in answer.Presence()
                from __ in presence.Switch(absent: static (_) => Fin.Fail<Unit>(
                        error: new PluginFault.Unbound(Member: nameof(PlugIn.SavePluginSettings))),
                    present: static (found) => found.States.Admits(PluginState.Loaded)
                        ? Fin.Succ(value: unit)
                        : Fin.Fail<Unit>(error: new KernelFault.InvalidValue(nameof(PlugIn.SavePluginSettings), "a loaded plug-in")))
                from ___ in Try.lift(() => PlugIn.SavePluginSettings(plugInId: row.Plugin.ToValue())).Run().Bind(static inner => inner)
                select (SettingsBridgeAnswer)new SettingsBridgeAnswer.PersistedCase(Plugin: row.Plugin),
            persistOwner: static row =>
                from owner in Admit.Need(row.Owner)
                from plugin in FactoryBridge.Accept<PluginKey>(owner.Id)
                from _ in Try.lift(owner.SaveSettings).Run().Bind(static inner => inner)
                select (SettingsBridgeAnswer)new SettingsBridgeAnswer.PersistedCase(Plugin: plugin),
            watch: static row => (
                    Admit.Need(row.Owner).ToValidation(),
                    Admit.Need(row.Source).ToValidation(),
                    Admit.Need(row.Path).ToValidation(),
                    Admit.Need(row.Sink).ToValidation())
                .Apply(static (owner, source, path, sink) => (Owner: owner, Source: source, Path: path, Sink: sink))
                .As()
                .ToFin()
                .Bind(admitted => SettingStore.Observe(
                    plugIn: admitted.Owner,
                    source: admitted.Source,
                    path: admitted.Path,
                    sink: admitted.Sink))
                .Map<SettingsBridgeAnswer>(static subscription => new SettingsBridgeAnswer.WatchCase(Watch: subscription)),
            drain: static row => Admit.Need(row.Queue)
                .Bind(queue => Try.lift(queue.Drain).Run().Bind(static inner => inner).Map(_ => queue))
                .Map<SettingsBridgeAnswer>(static queue => new SettingsBridgeAnswer.DrainedCase(Queue: queue))));
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]          | [INGRESS]                      | [STATE]                          | [EGRESS]                        |
| :-----: | :--------------- | :----------------------------- | :------------------------------- | :------------------------------ |
|  [01]   | `Participation`  | `Cross(ParticipationAsk)`      | none — the host owns the archive | `ParticipationAnswer` integrity |
|  [02]   | `PluginSettings` | `Commit(SettingsBridge)`       | the host settings tree           | `SettingsBridgeAnswer`          |
|  [03]   | `IParticipant`   | foreign plug-in implementation | participant-owned                | schema · predicate · payload    |
|  [04]   | `WriteIntent`    | `FileWriteOptions` detach      | `CapabilitySet<WriteToggle>`     | detached write coordinates      |
|  [05]   | `ReadIntent`     | `FileReadOptions` detach       | `CapabilitySet<ReadToggle>`      | detached read coordinates       |
|  [06]   | `SettingsQueue`  | `SettingsBridge.Drain`         | row-owned host verb              | the drained-queue discriminant  |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
