using System.Drawing;
using Rasm.Imaging.Output;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Plugin;
using Rasm.Rhino.Render.Content;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Slots;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Rasm.Rhino.Conversions))]

namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInRendering : IPlugInSink {
    public Seq<EffectKind> Effects { get; }
    public Atom<Option<RenderHistory>> History { get; }
    public Atom<Option<PreviewTiles>> Tiles { get; }
    public Atom<Option<SlotComparison>> Comparison { get; }
    public Atom<Option<RenderSets>> Sets { get; }
    public Atom<Option<QueueContext>> QueueContext { get; }

    public static Fin<T> Holding<T>(Atom<Option<T>> cell, string member) => cell.Value.ToFin(new Missing(member));

    public static IO<T> Served<T>(Atom<Option<T>> cell, string member) => IO.lift(() => Holding(cell, member));

    public static PlugInDefinition Defined(PlugInDefinition definition, Seq<EffectKind> effects) =>
        definition with {
            Acquisitions = [
                EffectRegistry.Register(effects),
                static (_, sink, retain) => Framing.Extent.Attach(sink, retain),
                static (_, sink, retain) => TargetRows.Shown.Attach(sink, retain),
                .. definition.Acquisitions,
            ],
            DocumentFrames = LockSets.Frame.Cons(definition.DocumentFrames),
        };
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RendererDefinition {
    // --- [ROWS]
    public required RenderEngine Engine { get; init; }
    public Seq<EffectKind> Effects { get; init; }
    public LanguageExt.HashSet<RenderPlugIn.RenderFeature> Features { get; init; }
    public Seq<(string Description, OutputTarget Target)> SaveTypes { get; init; }
    public Seq<View.Tab> Tabs { get; init; }
    public Seq<View.Pane> Panes { get; init; }
    public Seq<View.Section> SettingsSections { get; init; }
    public Seq<View.Section> SunSections { get; init; }
    public Option<View.DocumentPage> RenderPage { get; init; }
    public Option<ContentPreviewRow> Preview { get; init; }

    // --- [DERIVED]
    public Seq<View> Views => [.. Tabs, .. Panes, .. SettingsSections, .. SunSections];
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedRenderingPlugIn(PlugInDefinition definition, Seq<EffectKind> effects) : DefinedPlugIn(IPlugInRendering.Defined(definition, effects)), IPlugInRendering {
    // --- [RENDERING]
    public Seq<EffectKind> Effects { get; } = effects;

    public Atom<Option<RenderHistory>> History { get; } = Atom(Option<RenderHistory>.None);

    public Atom<Option<PreviewTiles>> Tiles { get; } = Atom(Option<PreviewTiles>.None);

    public Atom<Option<SlotComparison>> Comparison { get; } = Atom(Option<SlotComparison>.None);

    public Atom<Option<RenderSets>> Sets { get; } = Atom(Option<RenderSets>.None);

    public Atom<Option<QueueContext>> QueueContext { get; } = Atom(Option<QueueContext>.None);
}

public abstract class DefinedRenderPlugIn : RenderPlugIn, IPlugInViews, IPlugInRendering, IPlugInLifetime {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition;
    private readonly RendererDefinition renderer;
    private readonly PlugInLifetime lifetime;

    protected DefinedRenderPlugIn(PlugInDefinition definition, RendererDefinition renderer) {
        (this.definition, this.renderer) = (IPlugInRendering.Defined(definition, renderer.Effects), renderer);
        lifetime = new(this.definition, renderer.Views);
    }

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) {
        (LoadReturnCode code, errorMessage) = lifetime.Load(this, CallbackSite.Of(this));
        return code;
    }

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        lifetime.Register(RegisterCommand, CallbackSite.Of(this));
    }

    protected sealed override void OnShutdown() {
        base.OnShutdown();
        lifetime.Shutdown(CallbackSite.Of(this));
    }

    protected sealed override void ResetMessageBoxes() {
        base.ResetMessageBoxes();
        Callbacks.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    public IO<Unit> Close => lifetime.Close;

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => definition.History(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [RENDERING]
    public Seq<EffectKind> Effects => renderer.Effects;

    public Atom<Option<RenderHistory>> History { get; } = Atom(Option<RenderHistory>.None);

    public Atom<Option<PreviewTiles>> Tiles { get; } = Atom(Option<PreviewTiles>.None);

    public Atom<Option<SlotComparison>> Comparison { get; } = Atom(Option<SlotComparison>.None);

    public Atom<Option<RenderSets>> Sets { get; } = Atom(Option<RenderSets>.None);

    public Atom<Option<QueueContext>> QueueContext { get; } = Atom(Option<QueueContext>.None);

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        Callbacks.Run(ViewRegistration.OptionsDialogPages(this, pages), CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        Callbacks.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        Callbacks.Run(ViewRegistration.ObjectPropertiesPages(this, collection), CallbackSite.Of(this));
    }

    protected sealed override OptionsDialogPage? RenderOptionsDialogPage(RhinoDoc doc) =>
        Callbacks.Answer(
                renderer.RenderPage.Map(row => ViewRegistration.RenderOptionsDialogPage(this, row, doc)),
                static () => Option<OptionsDialogPage>.None,
                static () => Option<OptionsDialogPage>.None,
                CallbackSite.Of(this))
            .IfNone(() => base.RenderOptionsDialogPage(doc));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    // --- [RENDER]
    protected sealed override Result Render(RhinoDoc doc, RunMode mode, bool fastPreview) =>
        Callbacks.Answer(Conversions.ToResult(Batches.Render(doc, mode, this, renderer.Engine, new RenderStart.Full(fastPreview))), static () => Result.Failure, CallbackSite.Of(this));

    protected sealed override Result RenderWindow(RhinoDoc doc, RunMode modes, bool fastPreview, RhinoView view, Rectangle rect, bool inWindow, bool blowup) =>
        Callbacks.Answer(
            Conversions.ToResult(Batches.Render(doc, modes, this, renderer.Engine, new RenderStart.Region(fastPreview, view, rect, inWindow))),
            static () => Result.Failure,
            CallbackSite.Of(this));

    protected sealed override bool SupportsFeature(RenderFeature feature) => renderer.Features.Contains(feature);

    protected sealed override Guid[] SupportedChannels => [.. renderer.Engine.SupportedChannels];

    // --- [RENDER_WINDOW]
    protected sealed override void RegisterRenderPanels(RenderPanels panels) {
        base.RegisterRenderPanels(panels);
        Callbacks.Run(ViewRegistration.RegisterRenderPanels(this, panels), CallbackSite.Of(this));
    }

    protected sealed override void RegisterRenderTabs(RenderTabs tabs) {
        base.RegisterRenderTabs(tabs);
        Callbacks.Run(ViewRegistration.RegisterRenderTabs(this, tabs), CallbackSite.Of(this));
    }

    // --- [SECTIONS]
    public sealed override void RenderSettingsCustomSections(List<ICollapsibleSection> sections) {
        base.RenderSettingsCustomSections(sections);
        Callbacks.Run(ViewRegistration.CustomSections(this, renderer.SettingsSections, sections), CallbackSite.Of(this));
    }

    public sealed override void SunCustomSections(List<ICollapsibleSection> sections) {
        base.SunCustomSections(sections);
        Callbacks.Run(ViewRegistration.CustomSections(this, renderer.SunSections, sections), CallbackSite.Of(this));
    }

    // --- [SAVES]
    protected sealed override void RegisterCustomRenderSaveFileTypes(CustomRenderSaveFileTypes saveFileTypes) {
        base.RegisterCustomRenderSaveFileTypes(saveFileTypes);
        Callbacks.Run(IO.lift(() => renderer.SaveTypes.Iter(type => saveFileTypes.RegisterFileType(
            Seq(type.Target.FileFormat.Extension[1..]),
            RowText.Localize(type.Description, table: Some<object>(this)).Local,
            (fileName, includeAlpha, window) => SaveFile(type.Target with { Alpha = includeAlpha }, fileName, window)))), CallbackSite.Of(this));
    }

    private bool SaveFile(OutputTarget target, string fileName, RenderWindow window) =>
        Callbacks.Succeeded(
            Documents.WithDocument(new DocumentSource.Active(), document =>
                from path in IO.lift(Conversions.Validated<OutputPath, string, InvalidOutput>(fileName))
                from origin in FileOrigin.At(this)
                from whole in Saves.Whole(window)
                from written in Saves.Write(new SaveSource(document, window, None), origin, Seq((target, path, Option<RenderRegion>.None, whole)))
                select unit),
            CallbackSite.Of(this, nameof(CustomRenderSaveFileTypes.SaveFileHandler)));

    // --- [CONTENT]
    protected sealed override List<Guid> UiContentTypes() =>
        [
            .. base.UiContentTypes(),
            .. Callbacks.Answer(
                ContentRegistry.Types.Map(types => types.Filter(type => type.PlugInId == Id).Map(static type => type.Id)),
                static () => Seq<Guid>(),
                CallbackSite.Of(this)),
        ];

    protected sealed override PreviewRenderTypes PreviewRenderType() =>
        renderer.Preview.Map(static row => row.Type).IfNone(base.PreviewRenderType);

    protected sealed override void CreatePreview(CreatePreviewEventArgs args) {
        base.CreatePreview(args);
        Callbacks.Run(
            renderer.Preview.Map(row => row.Content(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))),
            CallbackSite.Of(this));
    }

    protected sealed override void CreateTexture2dPreview(CreateTexture2dPreviewEventArgs args) {
        base.CreateTexture2dPreview(args);
        Callbacks.Run(
            renderer.Preview.Map(row => row.Texture(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))),
            CallbackSite.Of(this));
    }
}
