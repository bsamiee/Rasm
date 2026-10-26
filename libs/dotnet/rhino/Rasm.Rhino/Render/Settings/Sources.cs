using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.FileIO;
using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Settings;

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

    public static IO<T> Edit<T>(RenderSettingsSource source, Func<RenderSettingsScope, IO<T>> edit) =>
        source.Switch(
            edit,
            live: static (body, held) => Opened(held.Doc, live =>
                DisposalOps.Within(live.Settings.Duplicate, staged =>
                    from edited in body(live with { Settings = staged })
                    from committed in Committed(held.Doc, live.Settings, staged)
                    select edited)),
            section: static (body, held) => SectionModels.Write(held.Host, Provider.RhinoSettings, data =>
                DisposalOps.Found(data.GetRenderSettings, nameof(RhinoSettings.GetRenderSettings), lent =>
                    from edited in body(new RenderSettingsScope(lent, held.Doc.ModelUnits))
                    from committed in IO.lift(() => data.SetRenderSettings(lent))
                    select edited)),
            archive: static (body, held) => Read(held, body),
            detached: static (body, held) => Read(held, body));

    public static IO<Unit> Edit(RhinoDoc doc, RenderSettings replacement) =>
        Commits.Commit(doc, nameof(RhinoDoc.RenderSettings), new RedrawPolicy.Silent(), Opened(doc, live => Committed(doc, live.Settings, replacement))).Map(static _ => unit);

    private static IO<T> Opened<T>(RhinoDoc doc, Func<RenderSettingsScope, IO<T>> body) =>
        DisposalOps.Within(() => doc.RenderSettings, settings => body(new RenderSettingsScope(settings, doc.ModelUnits)));

    private static IO<Unit> Committed(RhinoDoc doc, RenderSettings prior, RenderSettings next) =>
        from recorded in Recorded(doc, prior, next)
        from assigned in IO.lift(() => { doc.RenderSettings = next; })
        select assigned;

    private static IO<Unit> Recorded(RhinoDoc doc, RenderSettings prior, RenderSettings next) =>
        from open in IO.lift(() => doc.UndoRecordingIsActive)
        from recorded in when(open, IO.lift(() => (Prior: prior.Duplicate(), Next: next.Duplicate())).Bind(held => Undoable(doc, held.Prior, held.Next))).As()
        select recorded;

    private static IO<Unit> Undoable(RhinoDoc doc, RenderSettings restored, RenderSettings replaced) =>
        IO.lift(() => Refused.Unless(
            doc.AddCustomUndoEvent(nameof(RhinoDoc.RenderSettings), (_, args) => Restored(args.Document, restored, replaced).Run()),
            nameof(RhinoDoc.AddCustomUndoEvent)));

    private static IO<Unit> Restored(RhinoDoc doc, RenderSettings restored, RenderSettings replaced) =>
        from registered in Undoable(doc, replaced, restored)
        from assigned in IO.lift(() => { doc.RenderSettings = restored; })
        select assigned;

    // --- [DICTIONARY]
    public static DictionaryOwner Dictionary(RenderSettingsSource source) =>
        new(Read(source, static scope => ArchivableDictionaries.UserDictionary(scope.Settings).Map(static held => held.Clone())),
            edit => Edit(source, scope => ArchivableDictionaries.UserDictionary(scope.Settings).Bind(target => IO.lift(() => edit(target))))) {
            Document = source.Switch(
                live: static held => Some(held.Doc),
                section: static _ => Option<RhinoDoc>.None,
                archive: static _ => Option<RhinoDoc>.None,
                detached: static _ => Option<RhinoDoc>.None),
        };
}
