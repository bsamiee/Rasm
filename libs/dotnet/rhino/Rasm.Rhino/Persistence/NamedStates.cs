using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;
using NativeConstructionPlane = Rhino.DocObjects.ConstructionPlane;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StateName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value.Length > 0 ? null : new InvalidRhinoValue();
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class NamedTable {
    public static readonly NamedTable LayerStates = new(
        StringComparer.Ordinal, scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names)),
        static (doc, key) => IO.lift(() => Refused.Unless(doc.NamedLayerStates.Delete(key), nameof(NamedLayerStateTable.Delete))));

    public static readonly NamedTable Views = new(
        StringComparer.OrdinalIgnoreCase, scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedViews).Map(static view => view.Name).Strict()), NamedViews.Delete);

    public static readonly NamedTable ConstructionPlanes = new(
        StringComparer.OrdinalIgnoreCase, scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedConstructionPlanes).Map(static plane => plane.Name).Strict()),
        static (doc, key) => IO.lift(() => Refused.Unless(doc.NamedConstructionPlanes.Delete(key), nameof(NamedConstructionPlaneTable.Delete))));

    public static readonly NamedTable Positions = new(
        StringComparer.Ordinal, scripted: false,
        static doc => IO.lift(() => Conversions.Rows(doc.NamedPositions.Names)),
        static (doc, key) => IO.lift(() => Refused.Unless(doc.NamedPositions.Delete(key), nameof(NamedPositionTable.Delete))));

    public static readonly NamedTable Snapshots = new(
        StringComparer.OrdinalIgnoreCase, scripted: true, NamedSnapshots.Names, DeleteSnapshot);

    public IEqualityComparer<string> Comparer { get; }
    public bool Scripted { get; }
    [UseDelegateFromConstructor]
    public partial IO<Seq<string>> Names(RhinoDoc doc);
    [UseDelegateFromConstructor]
    public partial IO<Unit> Delete(RhinoDoc doc, string key);

    private static IO<Unit> DeleteSnapshot(RhinoDoc doc, string key) =>
        from name in IO.lift(() => Conversions.Validated<SnapshotName, string, InvalidRhinoValue>(key))
        from deleted in NamedSnapshots.RunSnapshot(doc, new SnapshotChange.Delete(name))
        select deleted;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NamedState {
    public (NamedTable Table, Option<string> Key) Address => Switch(
        layerState: static state => (NamedTable.LayerStates, Some((string)state.Name)),
        view: static state => (NamedTable.Views, Some((string)state.Name)),
        constructionPlane: static state => (NamedTable.ConstructionPlanes, Conversions.Present(state.Value.Name)),
        position: static state => (NamedTable.Positions, Some((string)state.Name)),
        snapshot: static state => (NamedTable.Snapshots, Some((string)state.Name)));

    public sealed record LayerState(StateName Name, Option<ViewportTarget> Viewport, Seq<LayerOp.Modify> Edits) : NamedState;
    public sealed record View(StateName Name, ViewportTarget Source, Option<CameraPose> Pose) : NamedState;
    public sealed record ConstructionPlane(NativeConstructionPlane Value) : NamedState;
    public sealed record Position(StateName Name, ObjectTarget Objects) : NamedState;
    public sealed record Snapshot(SnapshotName Name) : NamedState;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
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

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class LayerStateRestoreParameter : IStateParameter<LayerStateRestore> {
    // --- [FIELDS]
    public static readonly LayerStateRestoreParameter Properties = new Field<uint>(
        KeyName.Create("RestoreLayerProperties"), Lens<LayerStateRestore, uint>.New(static mask => mask.Properties, static properties => mask => mask with { Properties = properties }),
        static lens => new StateParameter<LayerStateRestore>.Raw<uint>(lens), SettingType.UnsignedInteger,
        "Layer settings to restore", "Layer properties a layer state restore applies");
    public static readonly LayerStateRestoreParameter ModelProperties = new Field<bool>(
        KeyName.Create("ModelPropertiesChecked"), Lens<LayerStateRestore, bool>.New(static mask => mask.ModelProperties, static enabled => mask => mask with { ModelProperties = enabled }),
        static lens => new StateParameter<LayerStateRestore>.Toggle(lens), SettingType.Bool,
        "Model layer settings", "Restore the checked layer settings");
    public static readonly LayerStateRestoreParameter ViewportProperties = new Field<bool>(
        KeyName.Create("ViewportPropertiesChecked"), Lens<LayerStateRestore, bool>.New(static mask => mask.ViewportProperties, static enabled => mask => mask with { ViewportProperties = enabled }),
        static lens => new StateParameter<LayerStateRestore>.Toggle(lens), SettingType.Bool,
        "Layout or Detail Layer settings to restore", "Restore the checked layout and detail layer settings");
    public StateParameter<LayerStateRestore> Kind { get; }

    // --- [PERSISTENCE]
    internal abstract IO<Func<LayerStateRestore, LayerStateRestore>> Read(RhinoDoc doc, (KeyName Section, SettingsNode Node) owner);
    internal abstract IO<Option<IO<Unit>>> Edit(Option<RhinoDoc> doc, (KeyName Section, SettingsNode Node) owner, Option<LayerStateRestore> value);

    private sealed class Field<TRaw>(KeyName key, Lens<LayerStateRestore, TRaw> lens, Func<Lens<LayerStateRestore, TRaw>, StateParameter<LayerStateRestore>> kind,
        SettingType<TRaw> setting, string caption, string help) : LayerStateRestoreParameter((string)key, kind(lens)) where TRaw : notnull, ISpanParsable<TRaw> {
        private readonly PlugInSetting<TRaw, TRaw> row = new(
            ValueKey.Raw((string)key, caption, help, lens.Get(LayerStateRestore.Default)), setting, Applied.IdleSave, [], Hidden: false);

        internal override IO<Func<LayerStateRestore, LayerStateRestore>> Read(RhinoDoc doc, (KeyName Section, SettingsNode Node) owner) =>
            from held in Store(Some(doc), owner).Read.Catch(static error => error.IsType<UnreadText>(), _ => IO.pure(Some(row.Value.Default)))
            from value in held.Match(Some: IO.pure, None: () => owner.Node.Current(row))
            select lens.SetF(value);

        internal override IO<Option<IO<Unit>>> Edit(Option<RhinoDoc> doc, (KeyName Section, SettingsNode Node) owner, Option<LayerStateRestore> value) =>
            Store(doc, owner).Edit(value.Map(lens.Get));

        private ValueStore<TRaw> Store(Option<RhinoDoc> doc, (KeyName Section, SettingsNode Node) owner) =>
            doc.Match(
                Some: document => UserTexts.Store(document, new DocumentKey(owner.Section, Some(key)), row.Value),
                None: () => PlugInSettings.Store(owner.Node, row));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedStates {
    // --- [LAYER_STATES]
    public static IO<Unit> RestoreLayerState(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<ViewportTarget> viewport) =>
        from found in LayerStateIndex(doc, name)
        from scope in Scope(doc, viewport)
        from restored in Restored(doc, name, properties, scope)
        select restored;

    public static IO<Unit> RenameLayerState(RhinoDoc doc, string name, StateName newName) =>
        from found in LayerStateIndex(doc, name)
        from renamed in IO.lift(() => Refused.Unless(doc.NamedLayerStates.Rename(name, newName), nameof(NamedLayerStateTable.Rename)))
        select renamed;

    public static IO<int> ImportLayerStates(RhinoDoc doc, string path) =>
        IO.lift(() => Exchange.ExistingPath(path).Map(existing => doc.NamedLayerStates.Import(existing)));

    private static IO<Unit> SaveLayerState(RhinoDoc doc, NamedState.LayerState state) =>
        from scope in Scope(doc, state.Viewport)
        from restorable in IO.lift(state.Edits.Bind(static modify => modify.Edits)
            .Traverse(edit => UnrestorableEdit.Unless(edit.Restores is not RestoreLayerProperties.None && (edit.Viewport.IsNone || edit.Viewport == scope), edit.Member).ToValidation()).As().ToFin())
        let edited = from changed in state.Edits.TraverseM(modify => Layers.Apply(doc, modify)).As()
                     from saved in Saved(doc, state.Name, scope)
                     select saved
        from result in state.Edits.IsEmpty
            ? Saved(doc, state.Name, scope)
            : DisposalOps.Within(
                () => Guid.NewGuid().ToString() switch {
                    var held => Conversions.Required(doc.NamedLayerStates.Save(held, Conversions.Unset(scope)), nameof(NamedLayerStateTable.Save)).Map(_ => held),
                },
                answer => IO.lift(() => answer).Bind(_ => edited),
                answer => answer.Match(
                    Succ: held => Callbacks.Each(Seq(Restored(doc, held, RestoreLayerProperties.All, scope),
                        IO.lift(() => Refused.Unless(doc.NamedLayerStates.Delete(held), nameof(NamedLayerStateTable.Delete))))).Map(unit),
                    Fail: static _ => IO.pure(unit)))
        select result;

    private static IO<int> LayerStateIndex(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)).ToFin(new Missing(nameof(NamedLayerStateTable.FindName))));

    private static IO<Option<Guid>> Scope(RhinoDoc doc, Option<ViewportTarget> viewport) =>
        viewport.Traverse(target => DisposalOps.Within<IDisposable, Guid>(retain =>
            Viewports.ResolveViewport(doc, target, retain).Bind(static row => IO.lift(() => {
                using ViewInfo view = new(row.Viewport);
                return view.Viewport.Id;
            })), DisposalOps.Release)).As();

    private static IO<Unit> Saved(RhinoDoc doc, string name, Option<Guid> scope) =>
        IO.lift(() => Conversions.Required(doc.NamedLayerStates.Save(name, Conversions.Unset(scope)), nameof(NamedLayerStateTable.Save)).Map(static _ => unit));

    private static IO<Unit> Restored(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<Guid> scope) =>
        IO.lift(() => Refused.Unless(doc.NamedLayerStates.Restore(name, properties, Conversions.Unset(scope)), nameof(NamedLayerStateTable.Restore)));

    // --- [CONSTRUCTION_PLANES]
    public static IO<Seq<bool>> RestoreConstructionPlane(RhinoDoc doc, string name, ViewportSet viewports, RedrawPolicy redraw) =>
        from plane in IO.lift(() => Conversions.Present(doc.NamedConstructionPlanes.Find(name))
            .Map(index => doc.NamedConstructionPlanes[index]).ToFin(new Missing(nameof(NamedConstructionPlaneTable.Find))))
        from restored in Navigation.ApplyToViewports(doc, viewports, port => IO.lift(() => port.PushConstructionPlane(plane)), redraw)
        select restored;

    // --- [POSITIONS]
    public static IO<Seq<NamedPosition>> Positions(RhinoDoc doc) =>
        IO.lift(() => Callbacks.Each(Conversions.Rows(doc.NamedPositions.Ids), (id, _) =>
            Callbacks.Each(Conversions.Rows(doc.NamedPositions.Objects(id)), (found, index) => {
                Transform xform = Transform.Unset;
                return RefusedElement.Unless(doc.NamedPositions.ObjectXform(id, found, ref xform), (ObjectId: found.Id, Xform: xform), nameof(NamedPositionTable.ObjectXform), index);
            }).Map(objects => new NamedPosition(id, doc.NamedPositions.Name(id), objects))));

    public static IO<Unit> RestorePosition(RhinoDoc doc, string name) =>
        Positioned(doc, name, static (table, id) => table.Restore(id), nameof(NamedPositionTable.Restore));

    public static IO<Unit> UpdatePosition(RhinoDoc doc, string name) =>
        Positioned(doc, name, static (table, id) => table.Update(id), nameof(NamedPositionTable.Update));

    public static IO<Unit> RenamePosition(RhinoDoc doc, string name, StateName newName) =>
        Positioned(doc, name, (table, id) => table.Rename(id, newName), nameof(NamedPositionTable.Rename));

    public static IO<Unit> AppendPosition(RhinoDoc doc, string name, ObjectTarget objects) =>
        from found in objects.Objects(doc)
        from appended in Positioned(doc, name, (table, id) => table.Append(id, found), nameof(NamedPositionTable.Append))
        select appended;

    private static IO<Unit> SavePosition(RhinoDoc doc, NamedState.Position position) =>
        from objects in position.Objects.Objects(doc)
        from saved in IO.lift(() => Conversions.Required(doc.NamedPositions.Save(position.Name, objects), nameof(NamedPositionTable.Save)))
        select unit;

    private static IO<Unit> Positioned(RhinoDoc doc, string name, Func<NamedPositionTable, Guid, bool> call, string member) =>
        from id in IO.lift(() => Conversions.Present(doc.NamedPositions.Id(name)).ToFin(new Missing(nameof(NamedPositionTable.Id))))
        from changed in IO.lift(() => Refused.Unless(call(doc.NamedPositions, id), member))
        select changed;

    // --- [PROGRAM]
    public static IO<Seq<(NamedTable Table, string Key)>> Entries(RhinoDoc doc) =>
        from tables in toSeq(NamedTable.Items).TraverseM(table => table.Names(doc).Map(keys => (Table: table, Keys: keys))).As()
        select (from table in tables
                from key in table.Keys
                select (table.Table, Key: key));

    public static Fin<Seq<NamedChange>> Plan(Seq<(NamedTable Table, string Key)> held, Seq<NamedState> desired, LanguageExt.HashSet<NamedTable> prune) =>
        from addresses in Fin.Succ(desired.Map(static state => state.Address))
        from kept in toSeq(addresses.GroupBy(static address => address.Table))
            .Traverse(static states => Callbacks.Unique(toSeq(states).Choose(static state => state.Key), identity, nameof(Plan), states.Key.Comparer)
                .Map(keys => (states.Key, keys.ToHashSet(states.Key.Comparer)))).As().ToFin()
        let byTable = toHashMap(kept)
        select held.Filter(entry => prune.Contains(entry.Table) && !byTable.Find(entry.Table).Exists(keys => keys.Contains(entry.Key)))
            .Map<NamedChange>(static entry => new NamedChange.Delete(entry.Table, entry.Key)) + desired.Map<NamedChange>(static state => new NamedChange.Save(state));

    public static IO<Committed<Unit>> Apply(RhinoDoc doc, string name, Seq<NamedChange> changes) =>
        changes.Partition(static change => change.Switch(save: static save => save.State.Address.Table.Scripted, delete: static delete => delete.Table.Scripted)) switch {
            var (scripted, recorded) =>
                from committed in Commits.Commit(doc, name, new RedrawPolicy.Silent(), recorded.TraverseM(change => Run(doc, change)).As().Map(unit))
                from ran in scripted.TraverseM(change => Run(doc, change)).As()
                select committed,
        };

    private static IO<Unit> Run(RhinoDoc doc, NamedChange change) =>
        change.Switch(doc, save: static (document, save) => Save(document, save.State), delete: static (document, delete) => delete.Table.Delete(document, delete.Key));

    private static IO<Unit> Save(RhinoDoc doc, NamedState state) =>
        state.Switch(doc,
            layerState: SaveLayerState,
            view: SaveView,
            constructionPlane: static (document, plane) =>
                IO.lift(() => Conversions.Required(document.NamedConstructionPlanes.Add(plane.Value), nameof(NamedConstructionPlaneTable.Add)).Map(static _ => unit)),
            position: SavePosition,
            snapshot: static (document, snapshot) => NamedSnapshots.Save(document, snapshot.Name));

    private static IO<Unit> SaveView(RhinoDoc doc, NamedState.View view) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
         from row in Viewports.ResolveViewport(doc, view.Source, retain)
         let added = NamedViews.Add(doc, row.Viewport, Some((string)view.Name))
         from index in view.Pose.Match(
             Some: pose => DisposalOps.Within(static () => unit,
                 _ => Cameras.WithPose(row.Viewport, pose, IO.lift(row.CommitViewportChanges).Bind(_ => added)),
                 _ => IO.lift(row.CommitViewportChanges)),
             None: () => added)
         select unit, DisposalOps.Release);

    // --- [RESTORE_MASK]
    private static readonly IO<(KeyName Section, SettingsNode Node)> MaskOwner =
        from commands in IO.lift(static () => Conversions.Required(PlugIn.IdFromName("Commands"), nameof(PlugIn.IdFromName)))
        from section in IO.lift(Conversions.Validated<KeyName, string, InvalidRhinoValue>(commands.ToString()))
        select (Section: section, Node: new SettingsNode(commands).Child("LayerStates"));

    public static IO<LayerStateRestore> RestoreMask(RhinoDoc doc) =>
        from owner in MaskOwner
        from fields in Callbacks.Each(toSeq(LayerStateRestoreParameter.Items).Map(field => field.Read(doc, owner)))
        select fields.Fold(LayerStateRestore.Default, static (mask, set) => set(mask));

    public static IO<Unit> SetRestoreMask(Option<RhinoDoc> doc, Override<LayerStateRestore> mask) =>
        when(mask.Changes,
            from owner in MaskOwner
            let value = mask.Applied(None)
            from written in ValueStore.Commit(toSeq(LayerStateRestoreParameter.Items).Map(field => field.Edit(doc, owner, value)))
            select written).As();
}
