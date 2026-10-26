using Rasm.UI;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LayerStateRestore(RestoreLayerProperties Properties, bool ModelProperties, bool ViewportProperties)
    : IStateRecord<LayerStateRestore, LayerStateRestoreParameter> {
    private const RestoreLayerProperties ViewportGroup =
        RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.ViewportColor | RestoreLayerProperties.ViewportPrintColor | RestoreLayerProperties.ViewportPrintWidth;

    private static readonly IO<(KeyName Section, SettingsNode Node)> Owner =
        from commands in IO.lift(static () => Conversions.Required(PlugIn.IdFromName("Commands"), nameof(PlugIn.IdFromName)))
        from section in IO.lift(KeyName.Validated(commands.ToString()))
        select (Section: section, Node: new SettingsNode(commands).Child("LayerStates"));

    public static LayerStateRestore Default { get; } = new(RestoreLayerProperties.All, ModelProperties: true, ViewportProperties: true);

    public RestoreLayerProperties Effective =>
        Properties & ((ModelProperties ? ~ViewportGroup : RestoreLayerProperties.None) | (ViewportProperties ? ViewportGroup : RestoreLayerProperties.None));

    // --- [STORE]
    public static IO<LayerStateRestore> Read(RhinoDoc doc) =>
        from owner in Owner
        from fields in Callbacks.Each(toSeq(LayerStateRestoreParameter.Items).Map(field => field.Read(doc, owner)))
        select fields.Fold(Default, static (mask, set) => set(mask));

    public static IO<Unit> Write(Option<RhinoDoc> doc, Override<LayerStateRestore> mask) =>
        when(mask.Changes,
            from owner in Owner
            let value = mask.Applied(None)
            from written in ValueStore.Commit(toSeq(LayerStateRestoreParameter.Items).Map(field => field.Edit(doc, owner, value)))
            select written).As();
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class LayerStateRestoreParameter : IStateParameter<LayerStateRestore> {
    // --- [FIELDS]
    public static readonly LayerStateRestoreParameter RestoreLayerProperties = new Field<uint>(
        KeyName.Create(nameof(RestoreLayerProperties)),
        Lens<LayerStateRestore, uint>.New(static mask => (uint)mask.Properties, static properties => mask => mask with { Properties = (RestoreLayerProperties)properties }),
        static lens => new StateParameter<LayerStateRestore>.Raw<uint>(lens), SettingType.UnsignedInteger,
        new("Layer settings to restore", "Layer properties a restore applies"));
    public static readonly LayerStateRestoreParameter ModelProperties = new Field<bool>(
        KeyName.Create("ModelPropertiesChecked"), Lens<LayerStateRestore, bool>.New(static mask => mask.ModelProperties, static enabled => mask => mask with { ModelProperties = enabled }),
        static lens => new StateParameter<LayerStateRestore>.Toggle(lens), SettingType.Bool,
        new("Model layer settings", "Restores checked settings in model"));
    public static readonly LayerStateRestoreParameter ViewportProperties = new Field<bool>(
        KeyName.Create("ViewportPropertiesChecked"), Lens<LayerStateRestore, bool>.New(static mask => mask.ViewportProperties, static enabled => mask => mask with { ViewportProperties = enabled }),
        static lens => new StateParameter<LayerStateRestore>.Toggle(lens), SettingType.Bool,
        new("Layout or detail layer settings to restore", "Restores checked settings in layouts and details"));

    public StateParameter<LayerStateRestore> Kind { get; }
    public ParameterText Text { get; }

    // --- [PERSISTENCE]
    internal abstract IO<Func<LayerStateRestore, LayerStateRestore>> Read(RhinoDoc doc, (KeyName Section, SettingsNode Node) owner);
    internal abstract IO<Option<IO<Unit>>> Edit(Option<RhinoDoc> doc, (KeyName Section, SettingsNode Node) owner, Option<LayerStateRestore> value);

    private sealed class Field<TRaw> : LayerStateRestoreParameter where TRaw : notnull, ISpanParsable<TRaw> {
        private readonly KeyName key;
        private readonly Lens<LayerStateRestore, TRaw> lens;
        private readonly PlugInSetting<TRaw, TRaw> row;

        internal Field(KeyName key, Lens<LayerStateRestore, TRaw> lens, Func<Lens<LayerStateRestore, TRaw>, StateParameter<LayerStateRestore>> kind, SettingType<TRaw> setting, ParameterText text)
            : base((string)key, kind(lens), text) {
            (this.key, this.lens) = (key, lens);
            row = new(ValueKey.Raw(Key, Text, lens.Get(LayerStateRestore.Default)), setting) { Applied = Applied.IdleSave };
        }

        internal override IO<Func<LayerStateRestore, LayerStateRestore>> Read(RhinoDoc doc, (KeyName Section, SettingsNode Node) owner) =>
            from held in Store(Some(doc), owner).Read.Catch(static error => error.HasCode(ErrorCodes.ParseError), _ => IO.pure(Some(row.Key.Default)))
            from value in held.Match(Some: static raw => IO.pure(raw), None: () => owner.Node.Current(row))
            select lens.SetF(value);

        internal override IO<Option<IO<Unit>>> Edit(Option<RhinoDoc> doc, (KeyName Section, SettingsNode Node) owner, Option<LayerStateRestore> value) =>
            Store(doc, owner).Edit(value.Map(lens.Get));

        private ValueStore<TRaw> Store(Option<RhinoDoc> doc, (KeyName Section, SettingsNode Node) owner) =>
            doc.Match(
                Some: document => UserTexts.Store(document, new DocumentKey(owner.Section, Some(key)), row.Key),
                None: () => PlugInSettings.Store(owner.Node, row));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class LayerStates {
    // --- [ROWS]
    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names).Map(static name => new NamedEntry(NamedTable.LayerStates, name, None)).Strict());

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name, CallbackSite site) =>
        from found in Found(doc, entry.Name)
        from renamed in TableOps.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => Renamed(doc, names.From, names.To), (From: entry.Name, To: (string)name), site)
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry) =>
        from found in Found(doc, entry.Name)
        from held in Held(doc, entry.Name)
        select (NamedWrite)new NamedWrite.Compensated(Renamed(doc, held, entry.Name), Deleted(doc, held));

    // --- [SAVE]
    internal static IO<NamedWrite> Save(RhinoDoc doc, NamedState.LayerState state) =>
        from scope in state.Scope.Traverse(target => Viewports.With(doc, target, static row => IO.pure(row.Viewport.Id))).As()
        from restorable in IO.lift(state.Edits.Bind(static modify => modify.Edits)
            .Traverse(edit => edit.Restores is not RestoreLayerProperties.None && (edit.Viewport.IsNone || edit.Viewport == scope)
                ? Fin.Succ(unit)
                : Error.New(ErrorCodes.ValidationFailed, $"layer state restore does not revert {edit.Member}")).As())
        let saved = Saved(doc, state.Name, scope)
        from written in state.Edits.IsEmpty
            ? saved
            : NamedTable.Within(doc, [NamedTable.LayerStates], state.Scope.Map(static target => (ViewportSet)new ViewportSet.One(target)),
                state.Edits.TraverseM(modify => Layers.Apply(doc, modify)).As().Bind(_ => saved))
        select written;

    private static IO<NamedWrite> Saved(RhinoDoc doc, string name, Option<Guid> scope) =>
        from prior in IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)))
        from held in prior.Traverse(_ => Held(doc, name)).As()
        from saved in DisposalOps.OnFailure(
            IO.lift(() => Conversions.Required(doc.NamedLayerStates.Save(name, Conversions.Unset(scope)), nameof(NamedLayerStateTable.Save))),
            held.Traverse(stored => Renamed(doc, stored, name)).As().Map(static _ => unit))
        select (NamedWrite)new NamedWrite.Compensated(
            Callbacks.Each(Seq(Deleted(doc, name), held.Traverse(stored => Renamed(doc, stored, name)).As().Map(static _ => unit))).Map(static _ => unit),
            held.Traverse(stored => Deleted(doc, stored)).As().Map(static _ => unit));

    // --- [RESTORE]
    internal static IO<NamedWrite> Restore(RhinoDoc doc, NamedRestore.LayerState target) =>
        from found in Found(doc, target.Name)
        from mask in target.Mask.Match(Some: static mask => IO.pure(mask), None: () => LayerStateRestore.Read(doc))
        from restored in target.Scope.Match(
            Some: set => Ids(doc, set).Bind(ids => ids.TraverseM(id => Restored(doc, target.Name, mask.Effective, Some(id))).As()).Map(static _ => unit),
            None: () => Restored(doc, target.Name, mask.Effective, None))
        select (NamedWrite)new NamedWrite.Recorded();

    // --- [HOLD]
    internal static IO<NamedWrite> Hold(RhinoDoc doc, Option<ViewportSet> scope) =>
        from ids in scope.Match(Some: set => Ids(doc, set).Map(static ids => ids.Map(static id => Some(id))), None: static () => IO.pure(Seq(Option<Guid>.None)))
        from held in ids.TraverseM(id =>
            from name in IO.lift(static () => Guid.NewGuid().ToString())
            from saved in IO.lift(() => Conversions.Required(doc.NamedLayerStates.Save(name, Conversions.Unset(id)), nameof(NamedLayerStateTable.Save)))
            select (Name: name, Id: id)).As()
        select (NamedWrite)new NamedWrite.Compensated(
            Callbacks.Each(held.Map(stored => Restored(doc, stored.Name, RestoreLayerProperties.All, stored.Id))).Map(static _ => unit),
            Callbacks.Each(held.Map(stored => Deleted(doc, stored.Name))).Map(static _ => unit));

    // --- [IMPORT]
    internal static IO<NamedWrite> Import(RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names) =>
        from whole in when(names.IsSome, IO.fail<Unit>(Error.New($"{nameof(NamedLayerStateTable.Import)} reads every layer state of a file"))).As()
        from path in source.Existing()
        from filed in DisposalOps.Found(
            static () => RhinoDoc.CreateHeadless(file3dmTemplatePath: null), nameof(RhinoDoc.CreateHeadless),
            headless => IO.lift(() => headless.NamedLayerStates.Import(path)).Map(_ => Conversions.Rows(headless.NamedLayerStates.Names)))
        from live in IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names))
        from held in filed.Filter(name => live.Exists(present => string.Equals(present, name, StringComparison.Ordinal)))
            .TraverseM(name => Held(doc, name).Map(stored => (Name: name, Held: stored))).As()
        from imported in DisposalOps.OnFailure(
            IO.lift(() => doc.NamedLayerStates.Import(path)),
            Callbacks.Each(held.Map(stored => Renamed(doc, stored.Held, stored.Name))).Map(static _ => unit))
        select (NamedWrite)new NamedWrite.Compensated(
            Callbacks.Each(filed.Map(name => Deleted(doc, name)) + held.Map(stored => Renamed(doc, stored.Held, stored.Name))).Map(static _ => unit),
            Callbacks.Each(held.Map(stored => Deleted(doc, stored.Held))).Map(static _ => unit));

    // --- [HOST]
    private static IO<int> Found(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)).ToFin(new NotFound(nameof(NamedTable.LayerStates), name)));

    private static IO<string> Held(RhinoDoc doc, string name) =>
        from held in IO.lift(static () => Guid.NewGuid().ToString())
        from renamed in Renamed(doc, name, held)
        select held;

    private static IO<Seq<Guid>> Ids(RhinoDoc doc, ViewportSet set) =>
        DisposalOps.Within((Action<IDisposable> retain) => Viewports.Resolve(doc, set, retain).Map(static rows => rows.Map(static row => row.Viewport.Id)), DisposalOps.Release);

    private static IO<Unit> Renamed(RhinoDoc doc, string name, string newName) =>
        IO.lift(() => Refused.Unless(doc.NamedLayerStates.Rename(name, newName), nameof(NamedLayerStateTable.Rename)));

    private static IO<Unit> Deleted(RhinoDoc doc, string name) =>
        IO.lift(() => Refused.Unless(doc.NamedLayerStates.Delete(name), nameof(NamedLayerStateTable.Delete)));

    private static IO<Unit> Restored(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<Guid> scope) =>
        IO.lift(() => Refused.Unless(scope.Match(
            Some: id => doc.NamedLayerStates.Restore(name, properties, id),
            None: () => doc.NamedLayerStates.Restore(name, properties)), nameof(NamedLayerStateTable.Restore)));
}
