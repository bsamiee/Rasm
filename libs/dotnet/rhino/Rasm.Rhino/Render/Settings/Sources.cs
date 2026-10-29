using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Views;
using Rhino;
using Rhino.FileIO;
using Rhino.Render;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderSettingsScope(RenderSettings Settings, LengthUnit Units);

[Union]
public abstract partial record RenderSettingsSource {
    private RenderSettingsSource(Option<RhinoDoc> document) => Document = document;

    public Option<RhinoDoc> Document { get; }

    public sealed record Live(RhinoDoc Doc) : RenderSettingsSource(Some(Doc));

    public sealed record Section(RhinoDoc Doc, ICollapsibleSection Host) : RenderSettingsSource(Some(Doc));

    public sealed record Archive(File3dm File) : RenderSettingsSource(None);

    public sealed record Detached(RenderSettingsScope Scope) : RenderSettingsSource(None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RenderSettingsSources {
    // --- [SCOPES]
    public static IO<T> Read<T>(RenderSettingsSource source, Func<RenderSettingsScope, IO<T>> read) =>
        source.Switch(
            read,
            live: static (body, held) => Opened(held.Doc, body),
            section: static (body, held) => Opened(held.Doc, body),
            archive: static (body, held) => IO.lift(() => new RenderSettingsScope(held.File.Settings.RenderSettings, held.File.Settings.ModelUnits)).Bind(body),
            detached: static (body, held) => body(held.Scope));

    public static IO<T> Edit<T>(RenderSettingsSource source, Func<RenderSettingsScope, IO<T>> edit) =>
        source.Switch(
            edit,
            live: static (body, held) => Opened(held.Doc, live =>
                (from staged in use(live.Settings.Duplicate)
                 from edited in body(live with { Settings = staged })
                 from committed in Committed(held.Doc, staged)
                 select edited).Bracket()),
            section: static (body, held) => SectionModels.Write(held.Host, Provider.RhinoSettings, data =>
                (from lent in Products.Of(data.GetRenderSettings)
                 from edited in body(new RenderSettingsScope(lent, held.Doc.ModelUnits))
                 from committed in IO.lift(() => data.SetRenderSettings(lent))
                 select edited).Bracket()),
            archive: static (body, held) => Read(held, body),
            detached: static (body, held) => Read(held, body));

    public static IO<Unit> Replace(RhinoDoc doc, RenderSettings replacement) =>
        Commits.Commit(doc, nameof(RhinoDoc.RenderSettings), new RedrawPolicy.Silent(), Committed(doc, replacement)).Map(static _ => unit);

    private static IO<T> Opened<T>(RhinoDoc doc, Func<RenderSettingsScope, IO<T>> body) =>
        use(() => doc.RenderSettings).Bind(settings => body(new RenderSettingsScope(settings, doc.ModelUnits))).Bracket();

    private static IO<Unit> Committed(RhinoDoc doc, RenderSettings next) =>
        IO.lift(() => doc.UndoRecordingIsActive).Bind(open => open
            ? Commits.Undoable(doc, nameof(RhinoDoc.RenderSettings), IO.lift(() => doc.RenderSettings), settings => IO.lift(() => doc.RenderSettings = settings).Map(static _ => unit), next.Duplicate())
            : IO.lift(() => doc.RenderSettings = next).Map(static _ => unit));

    // --- [DICTIONARY]
    public static DictionaryOwner Dictionary(RenderSettingsSource source) =>
        new(Read(source, static scope => IO.lift(() => scope.Settings.UserDictionary.Clone())),
            edit => Edit(source, scope => IO.lift(() => edit(scope.Settings.UserDictionary)))) {
            Document = source.Document,
        };
}
