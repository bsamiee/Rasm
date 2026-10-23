using System.Drawing;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Content;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RendererDefinition {
    // --- [ROWS]
    public required RenderEngine Engine { get; init; }
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
public abstract class DefinedRenderPlugIn(PlugInDefinition definition, RendererDefinition renderer) : RenderPlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly RendererDefinition renderer = renderer;
    private readonly PlugInLifetime lifetime = new(definition, renderer.Views);

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, CallbackSite.Of(this), ref errorMessage);

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
        lifetime.ResetMessageBoxes(this, CallbackSite.Of(this));
    }

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => lifetime.DisplayHelp(CallbackSite.Of(this));

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
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
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
            RowText.Localize(type.Description, this).Local,
            (fileName, includeAlpha, window) => Callbacks.Succeeded(
                Saves.Write(this, type.Target with { Alpha = includeAlpha }, fileName, window), CallbackSite.Of(this, nameof(CustomRenderSaveFileTypes.SaveFileHandler)))))),
            CallbackSite.Of(this));
    }

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
