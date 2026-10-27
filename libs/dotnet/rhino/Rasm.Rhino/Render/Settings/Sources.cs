using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Persistence;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.FileIO;
using Rhino.Render;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderSettingsScope(RenderSettings Settings, LengthUnit Units);

[Union]
public abstract partial record RenderSettingsSource {
    public sealed record Live(RhinoDoc Doc) : RenderSettingsSource;

    public sealed record Section(RhinoDoc Doc, ICollapsibleSection Host) : RenderSettingsSource;

    public sealed record Archive(File3dm File) : RenderSettingsSource;

    public sealed record Detached(RenderSettingsScope Scope) : RenderSettingsSource;
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

    public static IO<T> Edit<T>(RenderSettingsSource source, Func<RenderSettingsScope, IO<T>> edit, object owner) =>
        source.Switch(
            (Body: edit, Owner: owner),
            live: static (state, held) => Opened(held.Doc, live =>
                (from staged in use(live.Settings.Duplicate)
                 from edited in state.Body(live with { Settings = staged })
                 from committed in Committed(held.Doc, staged, state.Owner)
                 select edited).Bracket()),
            section: static (state, held) => SectionModels.Write(held.Host, Provider.RhinoSettings, data =>
                (from lent in Products.Made(data.GetRenderSettings())
                 from edited in state.Body(new RenderSettingsScope(lent, held.Doc.ModelUnits))
                 from committed in IO.lift(() => data.SetRenderSettings(lent))
                 select edited).Bracket()),
            archive: static (state, held) => Read(held, state.Body),
            detached: static (state, held) => Read(held, state.Body));

    public static IO<Unit> Edit(RhinoDoc doc, RenderSettings replacement, object owner) =>
        Commits.Commit(doc, nameof(RhinoDoc.RenderSettings), new RedrawPolicy.Silent(), Committed(doc, replacement, owner)).Map(static _ => unit);

    private static IO<T> Opened<T>(RhinoDoc doc, Func<RenderSettingsScope, IO<T>> body) =>
        use(() => doc.RenderSettings).Bind(settings => body(new RenderSettingsScope(settings, doc.ModelUnits))).Bracket();

    private static IO<Unit> Committed(RhinoDoc doc, RenderSettings next, object owner) =>
        from open in IO.lift(() => doc.UndoRecordingIsActive)
        from committed in open
            ? TableOps.Undoable(doc, nameof(RhinoDoc.RenderSettings), IO.lift(() => doc.RenderSettings), settings => Assigned(doc, settings), next.Duplicate(), owner)
            : Assigned(doc, next)
        select committed;

    private static IO<Unit> Assigned(RhinoDoc doc, RenderSettings settings) => IO.lift(() => { doc.RenderSettings = settings; });

    // --- [DICTIONARY]
    public static DictionaryOwner Dictionary(RenderSettingsSource source, object owner) =>
        new(Read(source, static scope => ArchivableDictionaries.UserDictionary(scope.Settings).Map(static held => held.Clone())),
            edit => Edit(source, scope => ArchivableDictionaries.UserDictionary(scope.Settings).Bind(target => IO.lift(() => edit(target))), owner)) {
            Document = source.Switch(
                live: static held => Some(held.Doc),
                section: static _ => Option<RhinoDoc>.None,
                archive: static _ => Option<RhinoDoc>.None,
                detached: static _ => Option<RhinoDoc>.None),
        };
}
