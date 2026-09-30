using System.Globalization;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.SnapShots;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Runtime.InteropWrappers;
using Rhino.UI;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
public enum LayerStateStore { Document = 0, Application = 1 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NamedPosition(Guid Id, string Name, Seq<(Guid ObjectId, Transform Xform)> Objects);

public sealed record LayerStateRestore(RestoreLayerProperties RestoreLayerProperties, bool ModelPropertiesChecked, bool ViewportPropertiesChecked) {
    private const RestoreLayerProperties ViewportGroup =
        RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.ViewportColor | RestoreLayerProperties.ViewportPrintColor | RestoreLayerProperties.ViewportPrintWidth;

    public RestoreLayerProperties Effective =>
        RestoreLayerProperties & ((ModelPropertiesChecked ? ~ViewportGroup : RestoreLayerProperties.None) | (ViewportPropertiesChecked ? ViewportGroup : RestoreLayerProperties.None));
}

[ValueObject<string>]
[ValidationError<ValidationFailure>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SnapshotName {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref string value) =>
        validationError = Answers.FirstInvalid(((value.Length == 0) || value.ContainsAny(['"', '\r', '\n']), nameof(SnapshotName)));
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SnapshotOption {
    public static readonly SnapshotOption Save = new(existing: false, present: true, static name => $"_-Snapshots _Save _Parameters=_All \"{name}\" _Enter");

    public static readonly SnapshotOption Restore = new(existing: true, present: true, static name => $"_-Snapshots _Restore \"{name}\" _Enter _Enter");

    public static readonly SnapshotOption Delete = new(existing: true, present: false, static name => $"_-Snapshots _Delete \"{name}\" _Enter");

    public bool Existing { get; }

    public bool Present { get; }

    [UseDelegateFromConstructor]
    public partial string Script(SnapshotName name);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SavedState {
    private SavedState(SavedTable table, string name) => (Table, Name) = (table, name);

    public SavedTable Table { get; }

    public string Name { get; }

    public sealed record LayerState(string Name, Option<ViewportTarget> Viewport, Seq<LayerOp.Modify> Described) : SavedState(SavedTable.NamedLayerStates, Name);

    public sealed record View(string Name, ViewportTarget Source, Option<CameraPose> Pose) : SavedState(SavedTable.NamedViews, Name);

    public sealed record ConstructionPlane(string Name, Plane Frame) : SavedState(SavedTable.NamedConstructionPlanes, Name);

    public sealed record Position(string Name, Seq<Guid> Objects) : SavedState(SavedTable.NamedPositions, Name);

    public sealed record Snapshot(SnapshotName Value) : SavedState(SavedTable.Snapshots, Value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SavedChange {
    public sealed record Save(SavedState Row) : SavedChange;

    public sealed record Delete(SavedTable Table, string Name) : SavedChange;
}

public sealed record SnapshotAnimationCallbacks(
    Func<RhinoDoc, BinaryArchiveReader, BinaryArchiveReader, BoundingBox, IO<BoundingBox>> ExtendBoundingBoxForDocumentAnimation,
    Func<RhinoDoc, RhinoObject, Transform, BinaryArchiveReader, BinaryArchiveReader, IO<Transform>> PrepareForObjectAnimation,
    Func<RhinoDoc, RhinoObject, Transform, BinaryArchiveReader, BinaryArchiveReader, BoundingBox, IO<BoundingBox>> ExtendBoundingBoxForObjectAnimation,
    Func<RhinoDoc, RhinoObject, Transform, double, BinaryArchiveReader, BinaryArchiveReader, IO<Transform>> AnimateObject);

public sealed record SnapshotDocumentCallbacks(Func<RhinoDoc, IO<ArchivableDictionary>> Save, Func<RhinoDoc, ChunkFrame, ArchivableDictionary, IO<Unit>> Restore);

public sealed record SnapshotObjectCallbacks(
    Func<RhinoDoc, RhinoObject, Transform, IO<ArchivableDictionary>> Save,
    Func<RhinoDoc, RhinoObject, Transform, ChunkFrame, ArchivableDictionary, IO<Transform>> Restore);

public sealed record SnapshotCallbacks(Action<Error> Reject, ChunkFrame Frame, Func<int, int, bool> SupportsVersion) {
    public Option<SnapshotDocumentCallbacks> Document { get; init; }

    public Option<SnapshotObjectCallbacks> Objects { get; init; }

    public Option<Func<RhinoDoc, RhinoObject, Transform, BinaryArchiveReader, IO<Transform>>> ObjectTransformNotification { get; init; }

    public Option<Func<RhinoDoc, Option<RhinoObject>, BinaryArchiveReader, SimpleArrayBinaryArchiveReader, Option<TextLog>, IO<bool>>> IsCurrentModelStateInAnySnapshot { get; init; }

    public Option<SnapshotAnimationCallbacks> Animation { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackSnapshotClient : SnapShotsClient {
    protected abstract SnapshotCallbacks Callbacks { get; }

    public sealed override Guid PlugInId() => Answers.Unset(Optional(PlugIn.Find(GetType().Assembly)).Map(static plugIn => plugIn.Id));

    public sealed override bool SaveDocument(RhinoDoc doc, BinaryArchiveWriter archive) =>
        Succeeded(Callbacks.Document.Map(document => document.Save(doc).Bind(dictionary => AttachedData.WriteChunk(archive, Callbacks.Frame, dictionary))));

    public sealed override bool RestoreDocument(RhinoDoc doc, BinaryArchiveReader archive) =>
        Succeeded(Callbacks.Document.Map(document => Chunk(archive).Bind(read => document.Restore(doc, read.Frame, read.Dictionary))));

    public sealed override bool SaveObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveWriter archive) {
        Transform current = transform;
        return Succeeded(Callbacks.Objects.Map(objects => objects.Save(doc, doc_object, current).Bind(dictionary => AttachedData.WriteChunk(archive, Callbacks.Frame, dictionary))));
    }

    public sealed override bool RestoreObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) =>
        Transformed(ref transform, Callbacks.Objects.Map(objects => fun((Transform current) => Chunk(archive).Bind(read => objects.Restore(doc, doc_object, current, read.Frame, read.Dictionary)))));

    public sealed override void ExtendBoundingBoxForDocumentAnimation(RhinoDoc doc, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop, ref BoundingBox bbox) =>
        bbox = Extended(bbox, Callbacks.Animation.Map(animation => fun((BoundingBox current) => animation.ExtendBoundingBoxForDocumentAnimation(doc, archive_start, archive_stop, current))));

    public sealed override bool PrepareForObjectAnimation(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Transformed(ref transform, Callbacks.Animation.Map(animation => fun((Transform current) => animation.PrepareForObjectAnimation(doc, doc_object, current, archive_start, archive_stop))));

    public sealed override void ExtendBoundingBoxForObjectAnimation(
        RhinoDoc doc,
        RhinoObject doc_object,
        ref Transform transform,
        BinaryArchiveReader archive_start,
        BinaryArchiveReader archive_stop,
        ref BoundingBox bbox) {
        Transform current = transform;
        bbox = Extended(bbox, Callbacks.Animation.Map(animation => fun((BoundingBox box) => animation.ExtendBoundingBoxForObjectAnimation(doc, doc_object, current, archive_start, archive_stop, box))));
    }

    public sealed override bool AnimateObject(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, double dPos, BinaryArchiveReader archive_start, BinaryArchiveReader archive_stop) =>
        Transformed(ref transform, Callbacks.Animation.Map(animation => fun((Transform current) => animation.AnimateObject(doc, doc_object, current, dPos, archive_start, archive_stop))));

    public sealed override bool ObjectTransformNotification(RhinoDoc doc, RhinoObject doc_object, ref Transform transform, BinaryArchiveReader archive) =>
        Transformed(ref transform, Callbacks.ObjectTransformNotification.Map(notify => fun((Transform current) => notify(doc, doc_object, current, archive))));

    public sealed override bool IsCurrentModelStateInAnySnapshot(RhinoDoc doc, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        InAnySnapshot(doc, Option<RhinoObject>.None, archive, archive_array, text_log);

    public sealed override bool IsCurrentModelStateInAnySnapshot(RhinoDoc doc, RhinoObject doc_object, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader archive_array, TextLog? text_log = null) =>
        InAnySnapshot(doc, Some(doc_object), archive, archive_array, text_log);

    private bool InAnySnapshot(RhinoDoc doc, Option<RhinoObject> subject, BinaryArchiveReader archive, SimpleArrayBinaryArchiveReader array, TextLog? log) =>
        Answers.Answer(Callbacks.IsCurrentModelStateInAnySnapshot.Map(check => check(doc, subject, archive, array, Optional(log))), Callbacks.Reject, static () => false);

    private IO<(ChunkFrame Frame, ArchivableDictionary Dictionary)> Chunk(BinaryArchiveReader archive) =>
        AttachedData.ReadChunk(archive, Callbacks.Frame.TypeCode, Callbacks.SupportsVersion);

    private bool Succeeded(Option<IO<Unit>> effect) =>
        Answers.Succeeded(effect, Callbacks.Reject, static () => false);

    private bool Transformed(ref Transform transform, Option<Func<Transform, IO<Transform>>> step) {
        Transform current = transform;
        Option<Transform> next = Answers.Answer(step.Map(run => run(current).Map(Some)), Callbacks.Reject, static () => Option<Transform>.None);
        transform = next.IfNone(current);
        return next.IsSome;
    }

    private BoundingBox Extended(BoundingBox box, Option<Func<BoundingBox, IO<BoundingBox>>> step) =>
        Answers.Answer(step.Map(run => run(box)), Callbacks.Reject, () => box);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedStates {
    // --- [CONSTRUCTION_PLANES]
    public static IO<Option<ConstructionPlane>> FindConstructionPlane(RhinoDoc doc, string name) =>
        IO.lift(() => Answers.Present(doc.NamedConstructionPlanes.Find(name)).Map(index => doc.NamedConstructionPlanes[index]));

    public static IO<int> Add(RhinoDoc doc, ConstructionPlane plane) =>
        IO.lift(() => Answers.Required(doc.NamedConstructionPlanes.Add(plane), nameof(NamedConstructionPlaneTable.Add)));

    public static IO<Unit> RestoreConstructionPlane(RhinoDoc doc, string name, ViewportTarget target) =>
        from found in FindConstructionPlane(doc, name)
        from plane in IO.lift(found.ToFin(new Missing(nameof(NamedConstructionPlaneTable.Find))))
        from pushed in DisposalOps.Using(
            Viewports.ResolveViewport(doc, target),
            row => Navigation.ApplyToRows(doc, Seq(row), port => IO.lift(() => port.PushConstructionPlane(plane)), new RedrawPolicy.Silent()))
        select unit;

    // --- [POSITIONS]
    public static IO<Seq<NamedPosition>> Positions(RhinoDoc doc) =>
        IO.lift(() => toSeq(doc.NamedPositions.Ids)
            .Traverse(id => toSeq(doc.NamedPositions.Objects(id))
                .Map((target, index) => {
                    Transform xform = Transform.Identity;
                    return RefusedElement.Unless(doc.NamedPositions.ObjectXform(id, target, ref xform), (ObjectId: target.Id, Xform: xform), nameof(NamedPositionTable.ObjectXform), index);
                })
                .Traverse(static placed => placed)
                .As()
                .Map(objects => new NamedPosition(id, doc.NamedPositions.Name(id), objects)))
            .As());

    public static IO<Guid> SavePosition(RhinoDoc doc, string name, Seq<Guid> objectIds) =>
        from named in IO.lift(() => Invalid.Unless(name.Length > 0, nameof(name)))
        from objects in Queries.Evaluate(doc, new ObjectTarget.Ids(objectIds))
        from id in IO.lift(() => Answers.Required(doc.NamedPositions.Save(name, objects), nameof(NamedPositionTable.Save)))
        select id;

    public static IO<Unit> RestorePosition(RhinoDoc doc, Guid id) =>
        IO.lift(() => Refused.Unless(doc.NamedPositions.Restore(id), nameof(NamedPositionTable.Restore)));

    public static IO<Unit> UpdatePosition(RhinoDoc doc, Guid id) =>
        IO.lift(() => Refused.Unless(doc.NamedPositions.Update(id), nameof(NamedPositionTable.Update)));

    public static IO<Unit> RenamePosition(RhinoDoc doc, Guid id, string name) =>
        from named in IO.lift(() => Invalid.Unless(name.Length > 0, nameof(name)))
        from renamed in IO.lift(() => Refused.Unless(doc.NamedPositions.Rename(id, name), nameof(NamedPositionTable.Rename)))
        select renamed;

    public static IO<Unit> AppendPosition(RhinoDoc doc, Guid id, Seq<Guid> objectIds) =>
        from objects in Queries.Evaluate(doc, new ObjectTarget.Ids(objectIds))
        from appended in IO.lift(() => Refused.Unless(doc.NamedPositions.Append(id, objects), nameof(NamedPositionTable.Append)))
        select appended;

    // --- [LAYER_STATES]
    public static IO<Unit> SaveLayerState(RhinoDoc doc, string name, Option<ViewportTarget> viewport, Seq<LayerOp.Modify> described) =>
        from scope in Scope(doc, viewport)
        from restorable in IO.lift(() => described
            .Bind(static modify => modify.Edits)
            .Traverse(edit => UnrestorableEdit.Unless(edit.Restored.IsSome && (edit.Viewport.IsNone || (edit.Viewport == scope)), edit.Name))
            .As())
        from saved in (
            from held in IO.lift(static () => Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture))
            from temporary in Saved(doc, held, scope)
            select held).Bracket(
            Use: _ => described.TraverseM(modify => Layers.ApplyLayerOp(doc, modify)).As().Bind(_ => Saved(doc, name, scope)),
            Fin: held => Restored(doc, held, RestoreLayerProperties.All, scope).Finally(SavedTable.NamedLayerStates.Delete(doc, held)))
        select saved;

    public static IO<Unit> RestoreLayerState(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<ViewportTarget> viewport) =>
        Scope(doc, viewport).Bind(scope => Restored(doc, name, properties, scope));

    public static IO<Unit> RenameLayerState(RhinoDoc doc, string name, string newName) =>
        IO.lift(() => Refused.Unless(doc.NamedLayerStates.Rename(name, newName), nameof(NamedLayerStateTable.Rename)));

    public static IO<int> ImportLayerStates(RhinoDoc doc, string path) =>
        Answers.ExistingPath(path).Bind(existing => IO.lift(() => doc.NamedLayerStates.Import(existing)));

    private static IO<Option<Guid>> Scope(RhinoDoc doc, Option<ViewportTarget> viewport) =>
        viewport.Traverse(target => DisposalOps.Using(Viewports.ResolveViewport(doc, target), static row => IO.lift(() => row.Viewport.Id))).As();

    private static IO<Unit> Saved(RhinoDoc doc, string name, Option<Guid> scope) =>
        IO.lift(() => Answers.Required(doc.NamedLayerStates.Save(name, Answers.Unset(scope)), nameof(NamedLayerStateTable.Save)).Map(static _ => unit));

    private static IO<Unit> Restored(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<Guid> scope) =>
        IO.lift(() => Refused.Unless(doc.NamedLayerStates.Restore(name, properties, Answers.Unset(scope)), nameof(NamedLayerStateTable.Restore)));

    // --- [LAYER_STATE_PANEL]
    private const string CommandsPlugIn = "Commands";

    private const string LayerStatesChild = "LayerStates";

    private static readonly SettingKey<uint> RestoreKey = new(nameof(LayerStateRestore.RestoreLayerProperties), SettingType.UnsignedInteger);

    private static readonly SettingKey<bool> ModelKey = new(nameof(LayerStateRestore.ModelPropertiesChecked), SettingType.Bool);

    private static readonly SettingKey<bool> ViewportKey = new(nameof(LayerStateRestore.ViewportPropertiesChecked), SettingType.Bool);

    public static IO<LayerStateRestore> PanelRestore(RhinoDoc doc) =>
        from commands in CommandsId()
        from root in IO.lift(() => PersistentSettings.FromPlugInId(commands))
        from node in PlugInSettings.TryGetChild(root, Seq(LayerStatesChild))
        select new LayerStateRestore(
            (RestoreLayerProperties)PanelValue(doc, commands, node, RestoreKey, (uint)RestoreLayerProperties.All),
            PanelValue(doc, commands, node, ModelKey, factory: true),
            PanelValue(doc, commands, node, ViewportKey, factory: true));

    public static IO<Unit> SetPanelRestore(RhinoDoc doc, LayerStateStore store, LayerStateRestore value) =>
        from commands in CommandsId()
        let rows = Seq(PanelRow(RestoreKey, (uint)value.RestoreLayerProperties), PanelRow(ModelKey, value.ModelPropertiesChecked), PanelRow(ViewportKey, value.ViewportPropertiesChecked))
        from written in store switch {
            LayerStateStore.Document =>
                UserTexts.WriteDocumentText(doc, toHashMap(rows.Map(row => ((DocumentTextKey)new DocumentTextKey.Section(commands.ToString(), row.Key), Some(row.Text))))),
            LayerStateStore.Application =>
                from root in IO.lift(() => PersistentSettings.FromPlugInId(commands))
                from node in PlugInSettings.AddChild(root, Seq(LayerStatesChild))
                from set in rows.TraverseM(row => row.Write(node)).As()
                from flushed in IO.lift(PlugIn.FlushSettingsSavedQueue)
                select unit,
        }
        select written;

    private static IO<Guid> CommandsId() =>
        IO.lift(static () => Answers.Required(PlugIn.IdFromName(CommandsPlugIn), nameof(PlugIn.IdFromName)));

    private static T PanelValue<T>(RhinoDoc doc, Guid commands, Option<PersistentSettings> node, SettingKey<T> key, T factory) =>
        (Optional(doc.Strings.GetValue(commands.ToString(), key.Name)).Filter(static text => !string.IsNullOrWhiteSpace(text))
            | node.Bind(settings => SettingType.Text(settings, key.Name)))
        .Bind(key.Type.Parse)
        .IfNone(factory);

    private static (string Key, string Text, Func<PersistentSettings, IO<Unit>> Write) PanelRow<T>(SettingKey<T> key, T value) =>
        (key.Name, $"{value}", node => PlugInSettings.Set(node, key, value));

    // --- [SNAPSHOTS]
    public static IO<Unit> RunSnapshot(RhinoDoc doc, SnapshotOption option, SnapshotName name) =>
        from runner in IO.lift(static () => Refused.Unless(Command.InScriptRunnerCommand(), nameof(Command.InScriptRunnerCommand)))
        from before in Stored(doc, name)
        from expected in IO.lift(() => option.Existing
            ? Missing.Unless(before, nameof(SnapshotTable.Names))
            : Taken.Unless(!before, nameof(RhinoDoc.Snapshots), name))
        from ran in DocumentHandles.Scripted(doc, option.Script(name))
        from after in Stored(doc, name)
        from applied in IO.lift(() => Refused.Unless(after == option.Present, nameof(SnapshotTable.Names)))
        select applied;

    public static IO<TValue> WithSnapshot<TValue>(RhinoDoc doc, SnapshotName name, IO<TValue> body) =>
        RunSnapshot(doc, SnapshotOption.Save, name).Bracket(
            Use: _ => body,
            Fin: _ => RunSnapshot(doc, SnapshotOption.Restore, name).Finally(RunSnapshot(doc, SnapshotOption.Delete, name)));

    public static IO<Unit> RegisterSnapShotClient(SnapShotsClient client) =>
        from plugged in IO.lift(() => Missing.Unless(PlugIn.Find(client.GetType().Assembly), nameof(PlugIn.Find)))
        from registered in IO.lift(() => Refused.Unless(SnapShotsClient.RegisterSnapShotClient(client), nameof(SnapShotsClient.RegisterSnapShotClient)))
        select registered;

    private static IO<bool> Stored(RhinoDoc doc, SnapshotName name) =>
        IO.lift(() => doc.Snapshots.Names.Contains(name, SavedTable.Snapshots.Comparer));

    // --- [RECONCILE]
    public static IO<Seq<(SavedTable Table, string Name)>> Held(RhinoDoc doc) =>
        toSeq(SavedTable.Items)
            .TraverseM(table => table.Names(doc).Map(names => names.Map(name => (Table: table, Name: name))))
            .As()
            .Map(static rows => rows.Flatten());

    public static Fin<Seq<SavedChange>> Plan(Seq<(SavedTable Table, string Name)> held, Seq<SavedState> desired, Seq<SavedTable> prune) =>
        toSeq(desired.GroupBy(static row => row.Table))
            .Traverse(static rows => Answers.Unique(toSeq(rows).Map(static row => row.Name), rows.Key.Comparer, rows.Key))
            .As()
            .ToFin()
            .Map(_ => held
                .Filter(row => prune.Exists(row.Table.Equals) && !desired.Exists(wanted => (wanted.Table == row.Table) && row.Table.Comparer.Equals(wanted.Name, row.Name)))
                .Map<SavedChange>(static row => new SavedChange.Delete(row.Table, row.Name))
                + desired.Map<SavedChange>(static row => new SavedChange.Save(row)));

    public static IO<Unit> Apply(RhinoDoc doc, Seq<SavedChange> changes) =>
        Commits.Commit(doc, LOC.STR("Reconcile saved states"), new RedrawPolicy.Silent(), changes
            .TraverseM(change => change.Switch(
                doc,
                save: static (document, save) => Save(document, save.Row),
                delete: static (document, delete) => delete.Table.Delete(document, delete.Name)))
            .As()
            .Map(static _ => unit));

    private static IO<Unit> Save(RhinoDoc doc, SavedState row) =>
        row.Switch(
            doc,
            layerState: static (document, state) => SaveLayerState(document, state.Name, state.Viewport, state.Described),
            view: static (document, view) => SaveView(document, view),
            constructionPlane: static (document, plane) =>
                IO.lift(() => Answers.Required(document.NamedConstructionPlanes.Add(plane.Name, plane.Frame), nameof(NamedConstructionPlaneTable.Add)).Map(static _ => unit)),
            position: static (document, position) => SavePosition(document, position.Name, position.Objects).Map(static _ => unit),
            snapshot: static (document, snapshot) =>
                from held in Stored(document, snapshot.Value)
                from cleared in when(held, RunSnapshot(document, SnapshotOption.Delete, snapshot.Value)).As()
                from saved in RunSnapshot(document, SnapshotOption.Save, snapshot.Value)
                select saved);

    private static IO<Unit> SaveView(RhinoDoc doc, SavedState.View view) =>
        DisposalOps.Using(Viewports.ResolveViewport(doc, view.Source), row => Posed(row.Viewport, view.Pose, NamedViews.Add(doc, row.Viewport, view.Name).Map(static _ => unit)));

    private static IO<Unit> Posed(RhinoViewport viewport, Option<CameraPose> pose, IO<Unit> body) =>
        pose.Match(
            Some: held => Navigation.ApplyStack(viewport, new StackOp.PushViewProjection()).Bracket(
                Use: _ => from posed in Cameras.WritePose(viewport, held) from saved in body select saved,
                Fin: _ => Navigation.ApplyStack(viewport, new StackOp.PopViewProjection())),
            None: () => body);
}

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SavedTable {
    public static readonly SavedTable NamedLayerStates = new(
        nameof(RhinoDoc.NamedLayerStates),
        StringComparer.Ordinal,
        static doc => IO.lift(() => toSeq(doc.NamedLayerStates.Names)),
        static (doc, name) => IO.lift(() => Refused.Unless(doc.NamedLayerStates.Delete(name), nameof(NamedLayerStateTable.Delete))));

    public static readonly SavedTable NamedViews = new(
        nameof(RhinoDoc.NamedViews),
        StringComparer.OrdinalIgnoreCase,
        static doc => TableOps.ReadRows<ViewInfo, string>(() => [.. doc.NamedViews], static view => IO.pure(view.Name)),
        static (doc, name) => IO.lift(() => Refused.Unless(doc.NamedViews.Delete(name), nameof(NamedViewTable.Delete))));

    public static readonly SavedTable NamedConstructionPlanes = new(
        nameof(RhinoDoc.NamedConstructionPlanes),
        StringComparer.OrdinalIgnoreCase,
        static doc => IO.lift(() => toSeq(doc.NamedConstructionPlanes).Map(static plane => plane.Name).Strict()),
        static (doc, name) => IO.lift(() => Refused.Unless(doc.NamedConstructionPlanes.Delete(name), nameof(NamedConstructionPlaneTable.Delete))));

    public static readonly SavedTable NamedPositions = new(
        nameof(RhinoDoc.NamedPositions),
        StringComparer.Ordinal,
        static doc => IO.lift(() => toSeq(doc.NamedPositions.Names)),
        static (doc, name) => IO.lift(() => Refused.Unless(doc.NamedPositions.Delete(name), nameof(NamedPositionTable.Delete))));

    public static readonly SavedTable Snapshots = new(
        nameof(RhinoDoc.Snapshots),
        StringComparer.OrdinalIgnoreCase,
        static doc => IO.lift(() => toSeq(doc.Snapshots.Names)),
        static (doc, name) => IO.lift(Answers.Validated<SnapshotName, string>(name)).Bind(valid => NamedStates.RunSnapshot(doc, SnapshotOption.Delete, valid)));

    public IEqualityComparer<string> Comparer { get; }

    [UseDelegateFromConstructor]
    public partial IO<Seq<string>> Names(RhinoDoc doc);

    [UseDelegateFromConstructor]
    public partial IO<Unit> Delete(RhinoDoc doc, string name);
}
