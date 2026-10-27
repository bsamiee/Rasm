using System.Globalization;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LayerStateRestore(RestoreLayerProperties Properties, bool ModelProperties, bool ViewportProperties) {
    private const RestoreLayerProperties ViewportGroup =
        RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.ViewportColor | RestoreLayerProperties.ViewportPrintColor | RestoreLayerProperties.ViewportPrintWidth;

    private static readonly KeyName PropertiesKey = KeyName.Create(nameof(RestoreLayerProperties));
    private static readonly KeyName ModelKey = KeyName.Create("ModelPropertiesChecked");
    private static readonly KeyName ViewportKey = KeyName.Create("ViewportPropertiesChecked");

    private static readonly IO<(KeyName Section, SettingsNode Node)> Owner =
        from commands in IO.lift(static () => Conversions.Found(Conversions.Present(PlugIn.IdFromName("Commands"))))
        from section in IO.lift(() => KeyName.TryCreate(commands.ToString(), out KeyName? held, out RhinoFault? fault) ? Fin.Succ(held) : fault)
        select (Section: section, Node: new SettingsNode(commands).Child("LayerStates"));

    public static LayerStateRestore Default { get; } = new(RestoreLayerProperties.All, ModelProperties: true, ViewportProperties: true);

    public RestoreLayerProperties Effective =>
        Properties & ((ModelProperties ? ~ViewportGroup : RestoreLayerProperties.None) | (ViewportProperties ? ViewportGroup : RestoreLayerProperties.None));

    // --- [STORE]
    public static IO<LayerStateRestore> Read(RhinoDoc doc) =>
        from owner in Owner
        from properties in Held(doc, owner, PropertiesKey,
            static text => uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out uint raw) ? Some(raw) : None,
            static (settings, key) => Conversions.Found(settings.TryGetUnsignedInteger(key, out uint raw), raw), (uint)Default.Properties)
        from model in Held(doc, owner, ModelKey,
            static text => bool.TryParse(text, out bool flag) ? Some(flag) : None,
            static (settings, key) => Conversions.Found(settings.TryGetBool(key, out bool flag), flag), Default.ModelProperties)
        from viewport in Held(doc, owner, ViewportKey,
            static text => bool.TryParse(text, out bool flag) ? Some(flag) : None,
            static (settings, key) => Conversions.Found(settings.TryGetBool(key, out bool flag), flag), Default.ViewportProperties)
        select new LayerStateRestore((RestoreLayerProperties)properties, model, viewport);

    public static IO<Unit> Write(Option<RhinoDoc> doc, Override<LayerStateRestore> mask) =>
        when(mask.Changes,
            from owner in Owner
            let value = mask.Applied(None)
            from written in doc.Match(
                Some: document => Callbacks.Each(Seq(
                        (Key: PropertiesKey, Text: value.Map(static held => ((uint)held.Properties).ToString(CultureInfo.InvariantCulture))),
                        (Key: ModelKey, Text: value.Map(static held => held.ModelProperties.ToString(CultureInfo.InvariantCulture))),
                        (Key: ViewportKey, Text: value.Map(static held => held.ViewportProperties.ToString(CultureInfo.InvariantCulture))))
                    .Map(row => UserStrings.Write(document, new DocumentKey(owner.Section, Some(row.Key)), row.Text))).Map(static _ => unit),
                None: () => Settled(owner.Node, value.IfNone(Default)))
            select written).As();

    private static IO<T> Held<T>(
        RhinoDoc doc, (KeyName Section, SettingsNode Node) owner, KeyName key, Func<string, Option<T>> parse, Func<PersistentSettings, string, Option<T>> stored, T @default) =>
        from text in UserStrings.Value(doc, new DocumentKey(owner.Section, Some(key)))
        from settings in SettingRoots.TryGetChild(owner.Node)
        select text.Match(Some: held => parse(held).IfNone(@default), None: () => settings.Bind(found => stored(found, key)).IfNone(@default));

    private static IO<Unit> Settled(SettingsNode node, LayerStateRestore value) =>
        from settings in SettingRoots.AddChild(node)
        from writable in IO.lift(() => Seq(PropertiesKey, ModelKey, ViewportKey).Traverse(key => PlugInSettings.Writable(settings, key)).As())
        from written in IO.lift(() => {
            settings.SetUnsignedInteger(PropertiesKey, (uint)value.Properties);
            settings.SetBool(ModelKey, value.ModelProperties);
            settings.SetBool(ViewportKey, value.ViewportProperties);
        })
        from drained in Applied.IdleSave.Drain.Traverse(static drain => drain).As()
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class LayerStates {
    // --- [ROWS]
    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names).Map(static name => new NamedEntry(NamedTable.LayerStates, name, None)).Strict());

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name, object owner) =>
        from found in Found(doc, entry.Name)
        from renamed in TableOps.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => Renamed(doc, names.From, names.To), (From: entry.Name, To: (string)name), owner)
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
                : new RhinoFault($"{edit.Member} in a layer state", RhinoReason.Unheld)).As())
        let saved = Saved(doc, state.Name, scope)
        from written in state.Edits.IsEmpty
            ? saved
            : NamedTable.Within(doc, [NamedTable.LayerStates], state.Scope.Map(static target => (ViewportSet)new ViewportSet.One(target)),
                state.Edits.TraverseM(modify => Layers.Apply(doc, modify)).As().Bind(_ => saved))
        select written;

    private static IO<NamedWrite> Saved(RhinoDoc doc, string name, Option<Guid> scope) =>
        from prior in IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)))
        from held in prior.Traverse(_ => Held(doc, name)).As()
        from saved in IO.lift(() => Conversions.Found(Conversions.Present(doc.NamedLayerStates.Save(name, Conversions.Unset(scope)))))
            .Catch(error => held.Traverse(stored => Renamed(doc, stored, name)).As() >> IO.fail<int>(error))
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
            from saved in IO.lift(() => Conversions.Found(Conversions.Present(doc.NamedLayerStates.Save(name, Conversions.Unset(id)))))
            select (Name: name, Id: id)).As()
        select (NamedWrite)new NamedWrite.Compensated(
            Callbacks.Each(held.Map(stored => Restored(doc, stored.Name, RestoreLayerProperties.All, stored.Id))).Map(static _ => unit),
            Callbacks.Each(held.Map(stored => Deleted(doc, stored.Name))).Map(static _ => unit));

    // --- [IMPORT]
    internal static IO<NamedWrite> Import(RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names) =>
        from whole in IO.lift(() => RhinoFault.Unless(names.IsNone))
        from path in source.Existing()
        from filed in (from headless in Products.Made(RhinoDoc.CreateHeadless(file3dmTemplatePath: null))
                       from imported in IO.lift(() => headless.NamedLayerStates.Import(path))
                       select Conversions.Rows(headless.NamedLayerStates.Names)).Bracket()
        from live in IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names))
        from held in filed.Filter(name => live.Exists(present => string.Equals(present, name, StringComparison.Ordinal)))
            .TraverseM(name => Held(doc, name).Map(stored => (Name: name, Held: stored))).As()
        from imported in IO.lift(() => doc.NamedLayerStates.Import(path))
            .Catch(error => Callbacks.Each(held.Map(stored => Renamed(doc, stored.Held, stored.Name))) >> IO.fail<int>(error))
        select (NamedWrite)new NamedWrite.Compensated(
            Callbacks.Each(filed.Map(name => Deleted(doc, name)) + held.Map(stored => Renamed(doc, stored.Held, stored.Name))).Map(static _ => unit),
            Callbacks.Each(held.Map(stored => Deleted(doc, stored.Held))).Map(static _ => unit));

    // --- [HOST]
    private static IO<int> Found(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)).ToFin(NamedTable.LayerStates.Missing(name)));

    private static IO<string> Held(RhinoDoc doc, string name) =>
        from held in IO.lift(static () => Guid.NewGuid().ToString())
        from renamed in Renamed(doc, name, held)
        select held;

    private static IO<Seq<Guid>> Ids(RhinoDoc doc, ViewportSet set) =>
        Viewports.Resolve(doc, set).Map(static rows => rows.Map(static row => row.Viewport.Id)).Bracket();

    private static IO<Unit> Renamed(RhinoDoc doc, string name, string newName) =>
        IO.lift(() => RhinoFault.Unless(doc.NamedLayerStates.Rename(name, newName)));

    private static IO<Unit> Deleted(RhinoDoc doc, string name) =>
        IO.lift(() => RhinoFault.Unless(doc.NamedLayerStates.Delete(name)));

    private static IO<Unit> Restored(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<Guid> scope) =>
        IO.lift(() => RhinoFault.Unless(scope.Match(
            Some: id => doc.NamedLayerStates.Restore(name, properties, id),
            None: () => doc.NamedLayerStates.Restore(name, properties))));
}
