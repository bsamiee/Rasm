using System.Globalization;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StateName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value.Length > 0 ? null : new InvalidRhinoValue();
}

[SmartEnum]
public sealed partial class NamedTable {
    public static readonly NamedTable LayerStates = new(
        StringComparer.Ordinal,
        scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names)),
        static (doc, key) => IO.lift(() => Refused.Unless(doc.NamedLayerStates.Delete(key), nameof(NamedLayerStateTable.Delete))));

    public static readonly NamedTable Views = new(
        StringComparer.OrdinalIgnoreCase,
        scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedViews).Map(static view => view.Name).Strict()),
        NamedViews.Delete);

    public static readonly NamedTable ConstructionPlanes = new(
        StringComparer.OrdinalIgnoreCase,
        scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedConstructionPlanes).Map(static plane => plane.Name).Strict()),
        static (doc, key) => IO.lift(() => Refused.Unless(doc.NamedConstructionPlanes.Delete(key), nameof(NamedConstructionPlaneTable.Delete))));

    public static readonly NamedTable Positions = new(
        StringComparer.Ordinal,
        scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedPositions.Names)),
        static (doc, key) => IO.lift(() => Refused.Unless(doc.NamedPositions.Delete(key), nameof(NamedPositionTable.Delete))));

    public static readonly NamedTable Snapshots = new(
        StringComparer.OrdinalIgnoreCase,
        scripted: true,
        NamedSnapshots.Names,
        static (doc, key) => IO.lift(() => Conversions.Validated<SnapshotName, string, InvalidRhinoValue>(key)).Bind(name => NamedSnapshots.Delete(doc, name)));

    public IEqualityComparer<string> Comparer { get; }

    public bool Scripted { get; }

    [UseDelegateFromConstructor]
    public partial IO<Seq<string>> Names(RhinoDoc doc);

    [UseDelegateFromConstructor]
    public partial IO<Unit> Delete(RhinoDoc doc, string key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NamedState {
    private NamedState(NamedTable table, string key) => (Table, Key) = (table, key);

    public NamedTable Table { get; }

    public string Key { get; }

    public sealed record LayerState(StateName Name, Option<ViewportTarget> Viewport, Seq<LayerOp.Modify> Edits) : NamedState(NamedTable.LayerStates, Name);

    public sealed record View(StateName Name, ViewportTarget Source, Option<CameraPose> Pose) : NamedState(NamedTable.Views, Name);

    public sealed record ConstructionPlane(global::Rhino.DocObjects.ConstructionPlane Value) : NamedState(NamedTable.ConstructionPlanes, Conversions.Unset(Conversions.Present(Value.Name)));

    public sealed record Position(StateName Name, ObjectTarget Objects) : NamedState(NamedTable.Positions, Name);

    public sealed record Snapshot(SnapshotName Name) : NamedState(NamedTable.Snapshots, Name);
}

[Union]
public abstract partial record NamedChange {
    public sealed record Save(NamedState State) : NamedChange;

    public sealed record Delete(NamedTable Table, string Key) : NamedChange;
}

public sealed record NamedPosition(Guid Id, string Name, Seq<(Guid ObjectId, Transform Xform)> Objects);

public sealed record LayerStateRestore(uint Properties, bool ModelProperties, bool ViewportProperties)
    : IStateRecord<LayerStateRestore, LayerStateRestoreParameter, InvalidRhinoValue> {
    private const RestoreLayerProperties ViewportGroup =
        RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.ViewportColor | RestoreLayerProperties.ViewportPrintColor | RestoreLayerProperties.ViewportPrintWidth;

    public static LayerStateRestore Default { get; } = new((uint)RestoreLayerProperties.All, ModelProperties: true, ViewportProperties: true);

    public RestoreLayerProperties Effective =>
        (RestoreLayerProperties)Properties & ((ModelProperties ? ~ViewportGroup : RestoreLayerProperties.None) | (ViewportProperties ? ViewportGroup : RestoreLayerProperties.None));
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerStateRestoreParameter : IStateParameter<LayerStateRestore> {
    public static readonly LayerStateRestoreParameter Properties = new(
        "RestoreLayerProperties",
        new StateParameter<LayerStateRestore>.Raw<uint>(
            Lens<LayerStateRestore, uint>.New(static mask => mask.Properties, static properties => mask => mask with { Properties = properties })));

    public static readonly LayerStateRestoreParameter ModelProperties = new(
        "ModelPropertiesChecked",
        new StateParameter<LayerStateRestore>.Toggle(
            Lens<LayerStateRestore, bool>.New(static mask => mask.ModelProperties, static on => mask => mask with { ModelProperties = on })));

    public static readonly LayerStateRestoreParameter ViewportProperties = new(
        "ViewportPropertiesChecked",
        new StateParameter<LayerStateRestore>.Toggle(
            Lens<LayerStateRestore, bool>.New(static mask => mask.ViewportProperties, static on => mask => mask with { ViewportProperties = on })));

    public StateParameter<LayerStateRestore> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedStates {
    // --- [LAYER_STATES]
    public static IO<Unit> RestoreLayerState(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<ViewportTarget> viewport) =>
        LayerStateIndex(doc, name).Bind(_ => Scope(doc, viewport)).Bind(scope => Restored(doc, name, properties, scope));

    public static IO<Unit> RenameLayerState(RhinoDoc doc, string name, StateName newName) =>
        LayerStateIndex(doc, name).Bind(_ => IO.lift(() => Refused.Unless(doc.NamedLayerStates.Rename(name, newName), nameof(NamedLayerStateTable.Rename))));

    public static IO<int> ImportLayerStates(RhinoDoc doc, string path) =>
        IO.lift(() => Exchange.ExistingPath(path).Map(existing => doc.NamedLayerStates.Import(existing)));

    private static IO<Unit> SaveLayerState(RhinoDoc doc, NamedState.LayerState state) =>
        from scope in Scope(doc, state.Viewport)
        from restorable in IO.lift(state.Edits
            .Bind(static modify => modify.Edits)
            .Traverse(edit => UnrestorableEdit.Unless(edit.Restores != RestoreLayerProperties.None && (edit.Viewport.IsNone || edit.Viewport == scope), edit.Member).ToValidation())
            .As()
            .ToFin())
        from saved in state.Edits.IsEmpty
            ? Saved(doc, state.Name, scope)
            : (from held in IO.lift(static () => Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture))
               from temporary in Saved(doc, held, scope)
               select held).Bracket(
                Use: _ => state.Edits.TraverseM(modify => Layers.Apply(doc, modify)).As().Bind(_ => Saved(doc, state.Name, scope)),
                Fin: held => Restored(doc, held, RestoreLayerProperties.All, scope)
                    .Finally(IO.lift(() => Refused.Unless(doc.NamedLayerStates.Delete(held), nameof(NamedLayerStateTable.Delete)))))
        select saved;

    private static IO<int> LayerStateIndex(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)).ToFin(new Missing(nameof(NamedLayerStateTable.FindName))));

    private static IO<Option<Guid>> Scope(RhinoDoc doc, Option<ViewportTarget> viewport) =>
        viewport.Traverse(target => use(Viewports.ResolveViewport(doc, target)).Bind(static row => IO.lift(() => row.Viewport.Id)).Bracket()).As();

    private static IO<Unit> Saved(RhinoDoc doc, string name, Option<Guid> scope) =>
        IO.lift(() => Conversions.Required(doc.NamedLayerStates.Save(name, Conversions.Unset(scope)), nameof(NamedLayerStateTable.Save)).Map(static _ => unit));

    private static IO<Unit> Restored(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<Guid> scope) =>
        IO.lift(() => Refused.Unless(doc.NamedLayerStates.Restore(name, properties, Conversions.Unset(scope)), nameof(NamedLayerStateTable.Restore)));

    // --- [CONSTRUCTION_PLANES]
    public static IO<Seq<bool>> RestoreConstructionPlane(RhinoDoc doc, string name, ViewportSet viewports, RedrawPolicy redraw) =>
        IO.lift(() => Conversions.Present(doc.NamedConstructionPlanes.Find(name))
                .Map(index => doc.NamedConstructionPlanes[index])
                .ToFin(new Missing(nameof(NamedConstructionPlaneTable.Find))))
            .Bind(plane => Navigation.ApplyToViewports(doc, viewports, port => {
                port.PushConstructionPlane(plane);
                return unit;
            }, redraw));

    // --- [POSITIONS]
    public static IO<Seq<NamedPosition>> Positions(RhinoDoc doc) =>
        IO.lift(() => Callbacks.Each(Conversions.Rows(doc.NamedPositions.Ids), (id, _) =>
            Callbacks.Each(Conversions.Rows(doc.NamedPositions.ObjectIds(id)), (objectId, index) => Placed(doc.NamedPositions, id, objectId, index))
                .Map(objects => new NamedPosition(id, doc.NamedPositions.Name(id), objects))));

    public static IO<Unit> RestorePosition(RhinoDoc doc, string name) =>
        Positioned(doc, name, static (table, id) => table.Restore(id), nameof(NamedPositionTable.Restore));

    public static IO<Unit> UpdatePosition(RhinoDoc doc, string name) =>
        Positioned(doc, name, static (table, id) => table.Update(id), nameof(NamedPositionTable.Update));

    public static IO<Unit> RenamePosition(RhinoDoc doc, string name, StateName newName) =>
        Positioned(doc, name, (table, id) => table.Rename(id, newName), nameof(NamedPositionTable.Rename));

    public static IO<Unit> AppendPosition(RhinoDoc doc, string name, ObjectTarget objects) =>
        objects.Objects(doc).Bind(found => Positioned(doc, name, (table, id) => table.Append(id, found), nameof(NamedPositionTable.Append)));

    private static IO<Unit> Positioned(RhinoDoc doc, string name, Func<NamedPositionTable, Guid, bool> call, string member) =>
        IO.lift(() => Conversions.Present(doc.NamedPositions.Id(name))
            .ToFin(new Missing(nameof(NamedPositionTable.Id)))
            .Bind(id => Refused.Unless(call(doc.NamedPositions, id), member)));

    private static Fin<(Guid ObjectId, Transform Xform)> Placed(NamedPositionTable table, Guid id, Guid objectId, int index) {
        Transform xform = Transform.Identity;
        return RefusedElement.Unless(table.ObjectXform(id, objectId, ref xform), (ObjectId: objectId, Xform: xform), nameof(NamedPositionTable.ObjectXform), index);
    }

    // --- [PROGRAM]
    public static IO<Seq<(NamedTable Table, string Key)>> Entries(RhinoDoc doc) =>
        toSeq(NamedTable.Items)
            .TraverseM(table => table.Names(doc).Map(keys => keys.Map(key => (Table: table, Key: key))))
            .As()
            .Map(static tables => tables.Flatten());

    public static Fin<Seq<NamedChange>> Plan(Seq<(NamedTable Table, string Key)> held, Seq<NamedState> desired, LanguageExt.HashSet<NamedTable> prune) =>
        toSeq(desired.GroupBy(static state => state.Table))
            .Traverse(static group => Callbacks.Unique(toSeq(group).Filter(static state => state.Key.Length > 0), static state => state.Key, nameof(Plan), group.Key.Comparer))
            .As()
            .ToFin()
            .Map(_ => held
                .Filter(entry => prune.Contains(entry.Table) && !desired.Exists(state => state.Table == entry.Table && entry.Table.Comparer.Equals(state.Key, entry.Key)))
                .Map<NamedChange>(static entry => new NamedChange.Delete(entry.Table, entry.Key))
                + desired.Map<NamedChange>(static state => new NamedChange.Save(state)));

    public static IO<Committed<Unit>> Apply(RhinoDoc doc, string name, Seq<NamedChange> changes) =>
        changes.Partition(Scripted) switch {
            var (scripted, recorded) =>
                from committed in Commits.Commit(doc, name, new RedrawPolicy.Silent(), recorded.TraverseM(change => Run(doc, change)).As().Map(static _ => unit))
                from ran in scripted.TraverseM(change => Run(doc, change)).As()
                select committed,
        };

    private static bool Scripted(NamedChange change) =>
        change.Switch(save: static save => save.State.Table.Scripted, delete: static delete => delete.Table.Scripted);

    private static IO<Unit> Run(RhinoDoc doc, NamedChange change) =>
        change.Switch(
            doc,
            save: static (document, save) => Save(document, save.State),
            delete: static (document, delete) => delete.Table.Delete(document, delete.Key));

    private static IO<Unit> Save(RhinoDoc doc, NamedState state) =>
        state.Switch(
            doc,
            layerState: static (document, layer) => SaveLayerState(document, layer),
            view: static (document, view) => SaveView(document, view),
            constructionPlane: static (document, plane) =>
                IO.lift(() => Conversions.Required(document.NamedConstructionPlanes.Add(plane.Value), nameof(NamedConstructionPlaneTable.Add)).Map(static _ => unit)),
            position: static (document, position) => position.Objects.Objects(document).Bind(objects =>
                IO.lift(() => Conversions.Required(document.NamedPositions.Save(position.Name, objects), nameof(NamedPositionTable.Save)).Map(static _ => unit))),
            snapshot: static (document, snapshot) => NamedSnapshots.Save(document, snapshot.Name));

    private static IO<Unit> SaveView(RhinoDoc doc, NamedState.View view) =>
        (from row in use(Viewports.ResolveViewport(doc, view.Source))
         let added = NamedViews.Add(doc, row.Viewport, Some<string>(view.Name))
         from index in view.Pose.Match(
             Some: pose => Cameras.WithPose(row.Viewport, pose, IO.lift(row.CommitViewportChanges).Bind(_ => added)).Finally(IO.lift(row.CommitViewportChanges)),
             None: () => added)
         select unit).Bracket();

    // --- [RESTORE_MASK]
    private static readonly PlugInSetting<uint, uint, InvalidRhinoValue> PropertiesRow = new(
        ValueKey.Raw(LayerStateRestoreParameter.Properties.Key, "Layer settings to restore", "Layer properties a layer state restore applies", LayerStateRestore.Default.Properties),
        SettingType.UnsignedInteger,
        Applied.IdleSave,
        Seq<string>(),
        Hidden: false);

    private static readonly PlugInSetting<bool, bool, InvalidRhinoValue> ModelPropertiesRow = new(
        ValueKey.Raw(LayerStateRestoreParameter.ModelProperties.Key, "Model layer settings", "Restore the checked layer settings", LayerStateRestore.Default.ModelProperties),
        SettingType.Bool,
        Applied.IdleSave,
        Seq<string>(),
        Hidden: false);

    private static readonly PlugInSetting<bool, bool, InvalidRhinoValue> ViewportPropertiesRow = new(
        ValueKey.Raw(LayerStateRestoreParameter.ViewportProperties.Key, "Layout or Detail Layer settings to restore", "Restore the checked layout and detail layer settings", LayerStateRestore.Default.ViewportProperties),
        SettingType.Bool,
        Applied.IdleSave,
        Seq<string>(),
        Hidden: false);

    private static readonly IO<(KeyName Section, SettingsNode Node)> MaskOwner =
        IO.lift(static () =>
            from commands in Conversions.Required(PlugIn.IdFromName("Commands"), nameof(PlugIn.IdFromName))
            from section in Conversions.Validated<KeyName, string, InvalidRhinoValue>(commands.ToString())
            select (Section: section, Node: new SettingsNode(commands).Child("LayerStates")));

    public static IO<LayerStateRestore> RestoreMask(RhinoDoc doc) =>
        MaskOwner.Bind(owner => (Held(doc, owner, PropertiesRow), Held(doc, owner, ModelPropertiesRow), Held(doc, owner, ViewportPropertiesRow))
            .Apply(static (properties, model, viewport) => new LayerStateRestore(properties, model, viewport))
            .As());

    public static IO<Unit> SetRestoreMask(RhinoDoc doc, Override<LayerStateRestore> mask) =>
        when(mask.Changes,
            from owner in MaskOwner
            let value = mask.Applied(None)
            from written in ValueStore.Commit(Seq(
                Documented(doc, owner, PropertiesRow).Bind(store => store.Edit(value.Map(static held => held.Properties))),
                Documented(doc, owner, ModelPropertiesRow).Bind(store => store.Edit(value.Map(static held => held.ModelProperties))),
                Documented(doc, owner, ViewportPropertiesRow).Bind(store => store.Edit(value.Map(static held => held.ViewportProperties)))))
            select written).As();

    public static IO<Unit> SetRestoreMaskDefault(LayerStateRestore mask) =>
        MaskOwner.Bind(owner => ValueStore.Commit(Seq(
            PlugInSettings.Store(owner.Node, PropertiesRow).Edit(Some(mask.Properties)),
            PlugInSettings.Store(owner.Node, ModelPropertiesRow).Edit(Some(mask.ModelProperties)),
            PlugInSettings.Store(owner.Node, ViewportPropertiesRow).Edit(Some(mask.ViewportProperties)))));

    private static IO<TRaw> Held<TRaw>(RhinoDoc doc, (KeyName Section, SettingsNode Node) owner, PlugInSetting<TRaw, TRaw, InvalidRhinoValue> row) where TRaw : notnull =>
        from document in Documented(doc, owner, row)
        from held in document.Read.Catch(static error => error.IsType<UnreadText>(), _ => IO.pure(Some(row.Value.Default)))
        from value in held.Match(Some: static found => IO.pure(found), None: () => PlugInSettings.Current(owner.Node, row))
        select value;

    private static IO<ValueStore<TRaw>> Documented<TRaw>(RhinoDoc doc, (KeyName Section, SettingsNode Node) owner, PlugInSetting<TRaw, TRaw, InvalidRhinoValue> row) where TRaw : notnull =>
        IO.lift(() => Conversions.Validated<KeyName, string, InvalidRhinoValue>(row.Value.Name)
            .Map(entry => UserTexts.Document(doc, new DocumentKey(owner.Section, Some(entry)), row.Value)));
}
