using System.Globalization;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LayerStateMask(RestoreLayerProperties Properties, bool ModelProperties, bool ViewportProperties) {
    private const RestoreLayerProperties ViewportGroup =
        RestoreLayerProperties.ViewportVisible | RestoreLayerProperties.ViewportColor | RestoreLayerProperties.ViewportPrintColor | RestoreLayerProperties.ViewportPrintWidth;

    private const string PropertiesKey = nameof(RestoreLayerProperties);
    private const string ModelKey = "ModelPropertiesChecked";
    private const string ViewportKey = "ViewportPropertiesChecked";
    private const string SettingsNode = "LayerStates";

    public static LayerStateMask Default { get; } = new(RestoreLayerProperties.All, ModelProperties: true, ViewportProperties: true);

    public RestoreLayerProperties Effective =>
        Properties & ((ModelProperties ? ~ViewportGroup : RestoreLayerProperties.None) | (ViewportProperties ? ViewportGroup : RestoreLayerProperties.None));

    // --- [PANEL]
    public static IO<LayerStateMask> Read(RhinoDoc doc) =>
        Panel.Map(owner => (owner.Section, Settings: owner.Settings.TryGetChild(SettingsNode, out PersistentSettings node) ? Some(node) : None) switch {
            var (section, settings) => new LayerStateMask(
                (RestoreLayerProperties)Stored(doc, section, PropertiesKey,
                    static text => uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out uint raw) ? Some(raw) : None,
                    settings.Bind(static held => Conversions.Present(held.TryGetUnsignedInteger(PropertiesKey, out uint raw), raw)), (uint)Default.Properties),
                Stored(doc, section, ModelKey, static text => bool.TryParse(text, out bool flag) ? Some(flag) : None,
                    settings.Bind(static held => Conversions.Present(held.TryGetBool(ModelKey, out bool flag), flag)), Default.ModelProperties),
                Stored(doc, section, ViewportKey, static text => bool.TryParse(text, out bool flag) ? Some(flag) : None,
                    settings.Bind(static held => Conversions.Present(held.TryGetBool(ViewportKey, out bool flag), flag)), Default.ViewportProperties)),
        });

    public IO<Unit> Write(Option<RhinoDoc> doc) =>
        Panel.Map(owner => doc.Match(
            Some: document => {
                _ = document.Strings.SetString(owner.Section, PropertiesKey, ((uint)Properties).ToString(CultureInfo.InvariantCulture));
                _ = document.Strings.SetString(owner.Section, ModelKey, ModelProperties.ToString(CultureInfo.InvariantCulture));
                _ = document.Strings.SetString(owner.Section, ViewportKey, ViewportProperties.ToString(CultureInfo.InvariantCulture));
            },
            None: () => {
                PersistentSettings node = owner.Settings.AddChild(SettingsNode);
                node.SetUnsignedInteger(PropertiesKey, (uint)Properties);
                node.SetBool(ModelKey, ModelProperties);
                node.SetBool(ViewportKey, ViewportProperties);
            }));

    private static IO<(string Section, PersistentSettings Settings)> Panel =>
        IO.lift(static () => Conversions.Required(Conversions.Present(PlugIn.IdFromName("Commands")))
            .Map(static commands => (Section: commands.ToString(), Settings: PersistentSettings.FromPlugInId(commands))));

    private static T Stored<T>(RhinoDoc doc, string section, string key, Func<string, Option<T>> parse, Option<T> setting, T fallback) =>
        Conversions.Present(doc.Strings.GetValue(section, key)).Bind(parse).IfNone(() => setting.IfNone(fallback));
}

public sealed record LayerState(StateName Name, Option<Guid> Viewport, Seq<(LayerRef Layer, Seq<RowEdit<Layer>> Edits)> Edits) : NamedState(LayerStates.Table, Name) {
    internal override IO<NamedWrite> Save(RhinoDoc doc) =>
        Edits.IsEmpty
            ? LayerStates.Saved(doc, Name, Viewport)
            : NamedTable.Within(doc, [LayerStates.Table], Viewport,
                Edits.TraverseM(edit => Layers.Resolve(doc, edit.Layer).Bind(row => Rows.Edit(doc, Layers.Kind, new ComponentRef<Layer>.ById(row.Id), edit.Edits))).As()
                    .Bind(_ => LayerStates.Saved(doc, Name, Viewport)));
}

public sealed record LayerStateRestore(StateName Name, Option<LayerStateMask> Mask, Option<Seq<Guid>> Viewports) : NamedRestore(LayerStates.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) =>
        from found in LayerStates.Index(doc, Name)
        from mask in Mask.Match(Some: static held => IO.pure(held), None: () => LayerStateMask.Read(doc))
        from restored in Viewports.Match(
            Some: ids => ids.TraverseM(id => LayerStates.Restored(doc, Name, mask.Effective, Some(id))).As().Map(static _ => unit),
            None: () => LayerStates.Restored(doc, Name, mask.Effective, None))
        select NamedWrite.Undoable;
}

// --- [TABLES] --------------------------------------------------------------------------
public static class LayerStates {
    public static NamedTable Table { get; } = new(nameof(LayerStates), StringComparer.Ordinal) {
        Entries = Entries,
        Rename = Rename,
        Delete = Delete,
        Hold = Hold,
        Import = Some(Import),
    };

    // --- [ROWS]
    private static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names).Map(static name => new NamedEntry(Table, name, None)).Strict());

    private static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name) =>
        from found in Index(doc, entry.Name)
        from renamed in Commits.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => Renamed(doc, names.From, names.To), (From: entry.Name, To: (string)name))
        select NamedWrite.Undoable;

    private static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry) =>
        from found in Index(doc, entry.Name)
        from held in Aside(doc, entry.Name)
        select new NamedWrite(Renamed(doc, held, entry.Name), Deleted(doc, held));

    // --- [SAVE]
    internal static IO<NamedWrite> Saved(RhinoDoc doc, string name, Option<Guid> viewport) =>
        from prior in IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)))
        from held in prior.Traverse(_ => Aside(doc, name)).As()
        from saved in IO.lift(() => Conversions.Required(Conversions.Present(doc.NamedLayerStates.Save(name, Conversions.Unset(viewport)))))
            .Catch(error => held.Traverse(stored => Renamed(doc, stored, name)).As() >> IO.fail<int>(error))
        select new NamedWrite(
            Products.Each(Seq(Deleted(doc, name), held.Traverse(stored => Renamed(doc, stored, name)).As().Map(static _ => unit))).Map(static _ => unit),
            held.Traverse(stored => Deleted(doc, stored)).As().Map(static _ => unit));

    // --- [HOLD]
    private static IO<NamedWrite> Hold(RhinoDoc doc, Option<Guid> viewport) =>
        from name in IO.lift(static () => Guid.NewGuid().ToString())
        from saved in IO.lift(() => Conversions.Required(Conversions.Present(doc.NamedLayerStates.Save(name, Conversions.Unset(viewport)))))
        select new NamedWrite(Restored(doc, name, RestoreLayerProperties.All, viewport), Deleted(doc, name));

    // --- [IMPORT]
    private static IO<NamedWrite> Import(RhinoDoc doc, InputPath path, Option<Seq<StateName>> names) =>
        from whole in IO.lift(() => RhinoFault.Unless(names.IsNone, RhinoReason.Unsupported, names))
        from filed in (from headless in Products.Of(static () => RhinoDoc.CreateHeadless(file3dmTemplatePath: null))
                       from imported in IO.lift(() => headless.NamedLayerStates.Import(path))
                       select Conversions.Rows(headless.NamedLayerStates.Names)).Bracket()
        from live in IO.lift(() => Conversions.Rows(doc.NamedLayerStates.Names))
        from held in filed.Filter(name => live.Exists(present => string.Equals(present, name, StringComparison.Ordinal)))
            .TraverseM(name => Aside(doc, name).Map(stored => (Name: name, Aside: stored))).As()
        from imported in IO.lift(() => doc.NamedLayerStates.Import(path))
            .Catch(error => Products.Each(held.Map(stored => Renamed(doc, stored.Aside, stored.Name))) >> IO.fail<int>(error))
        select new NamedWrite(
            Products.Each(filed.Map(name => Deleted(doc, name)) + held.Map(stored => Renamed(doc, stored.Aside, stored.Name))).Map(static _ => unit),
            Products.Each(held.Map(stored => Deleted(doc, stored.Aside))).Map(static _ => unit));

    // --- [HOST]
    internal static IO<int> Index(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedLayerStates.FindName(name)).ToFin(Table.Missing(name)));

    internal static IO<Unit> Restored(RhinoDoc doc, string name, RestoreLayerProperties properties, Option<Guid> viewport) =>
        Products.Accepted(() => viewport.Match(
            Some: id => doc.NamedLayerStates.Restore(name, properties, id),
            None: () => doc.NamedLayerStates.Restore(name, properties)));

    private static IO<string> Aside(RhinoDoc doc, string name) =>
        from held in IO.lift(static () => Guid.NewGuid().ToString())
        from renamed in Renamed(doc, name, held)
        select held;

    private static IO<Unit> Renamed(RhinoDoc doc, string name, string newName) =>
        Products.Accepted(() => doc.NamedLayerStates.Rename(name, newName));

    private static IO<Unit> Deleted(RhinoDoc doc, string name) =>
        Products.Accepted(() => doc.NamedLayerStates.Delete(name));
}
