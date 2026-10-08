using System.Drawing;
using Rasm.Imaging.Output;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render;
using Rasm.Rhino.Render.Content;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Queue;
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

namespace Rasm.Rhino.Plugin;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FileFormats {
    // --- [LIST]
    public static IO<FileTypeList> AddFileTypes<TOptions>(Seq<FileFormat<TOptions>> formats, IPlugInSink sink) where TOptions : class =>
        from list in IO.lift(static () => new FileTypeList())
        from added in IO.lift(() => Callbacks.Each(
            formats,
            format => list.AddFileType(
                RowText.Localize(format.Description, table: Some<object>(sink)).Local,
                format.Extensions.Map(static extension => (string)extension),
                format.Options.IsSome) >= 0,
            nameof(FileTypeList.AddFileType)))
        select list;

    // --- [RUN]
    public static IO<A> Run<TOptions, A>(Seq<FileFormat<TOptions>> formats, int index, string path, RhinoDoc doc, TOptions options, A done, A cancelled, string member) where TOptions : class =>
        (from format in IO.lift(formats.At(index).ToFin(new IndexOutOfRange(member, index, formats.Count)))
         from ran in format.Run(path, doc, options)
         select done)
            .Catch(static error => error.Is(Errors.Cancelled), _ => IO.pure(cancelled));

    // --- [OPTIONS]
    public static IO<Unit> DisplayOptionsDialog<TOptions>(IPlugInViews plugIn, Seq<FileFormat<TOptions>> formats, string description, string extension) where TOptions : class =>
        (from dialog in IO.lift(() => formats
             .Find(format =>
                 string.Equals(RowText.Localize(format.Description, table: Some<object>(plugIn)).Local, description, StringComparison.Ordinal)
                 && string.Equals(((string)format.Extensions.Head)[1..], extension, StringComparison.Ordinal))
             .Bind(static format => format.Options)
             .ToFin(new UnmatchedFormat(description, extension)))
         from shown in Documents.WithDocument(new DocumentSource.Active(), document => Showing.ShowSemiModal(plugIn, dialog, document))
         select unit)
            .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedPlugIn(PlugInDefinition definition) : PlugIn, IPlugInViews, IPlugInRendering {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition = definition;
    private readonly PlugInLifetime lifetime = new(definition, Seq<View>());

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
        _ = Callbacks.Answer(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), static () => unit, CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => lifetime.Limits(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [RENDERING]
    public Seq<EffectKind> Effects => definition.Effects;

    public Atom<Option<RenderHistory>> History { get; } = Atom(Option<RenderHistory>.None);

    public Atom<Option<PreviewTiles>> Tiles { get; } = Atom(Option<PreviewTiles>.None);

    public Atom<Option<SlotComparison>> Comparison { get; } = Atom(Option<SlotComparison>.None);

    public Atom<Option<RenderSets>> Sets { get; } = Atom(Option<RenderSets>.None);

    public Atom<Option<NoticeSender>> Notices { get; } = Atom(Option<NoticeSender>.None);

    public Atom<Option<QueueContext>> QueueContext { get; } = Atom(Option<QueueContext>.None);

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        _ = Callbacks.Answer(ViewRegistration.OptionsDialogPages(this, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        _ = Callbacks.Answer(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        _ = Callbacks.Answer(ViewRegistration.ObjectPropertiesPages(this, collection), static () => unit, CallbackSite.Of(this));
    }

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        _ = DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        _ = DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }
}

public abstract class DefinedImportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileReadOptions>> formats) : FileImportPlugIn, IPlugInViews, IPlugInRendering {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition = definition;
    private readonly Seq<FileFormat<FileReadOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

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
        _ = Callbacks.Answer(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), static () => unit, CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => lifetime.Limits(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [RENDERING]
    public Seq<EffectKind> Effects => definition.Effects;

    public Atom<Option<RenderHistory>> History { get; } = Atom(Option<RenderHistory>.None);

    public Atom<Option<PreviewTiles>> Tiles { get; } = Atom(Option<PreviewTiles>.None);

    public Atom<Option<SlotComparison>> Comparison { get; } = Atom(Option<SlotComparison>.None);

    public Atom<Option<RenderSets>> Sets { get; } = Atom(Option<RenderSets>.None);

    public Atom<Option<NoticeSender>> Notices { get; } = Atom(Option<NoticeSender>.None);

    public Atom<Option<QueueContext>> QueueContext { get; } = Atom(Option<QueueContext>.None);

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        _ = Callbacks.Answer(ViewRegistration.OptionsDialogPages(this, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        _ = Callbacks.Answer(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        _ = Callbacks.Answer(ViewRegistration.ObjectPropertiesPages(this, collection), static () => unit, CallbackSite.Of(this));
    }

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        _ = DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        _ = DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileReadOptions options) =>
        Callbacks.Answer(FileFormats.AddFileTypes(formats, this), static () => new FileTypeList(), CallbackSite.Of(this));

    protected sealed override bool ReadFile(string filename, int index, RhinoDoc? doc, FileReadOptions options) =>
        Callbacks.Answer(
            IO.lift(Missing.Unless(doc, nameof(RhinoDoc.FromRuntimeSerialNumber)))
                .Bind(document => FileFormats.Run(formats, index, filename, document, options, done: true, cancelled: false, nameof(ReadFile))),
            static () => false,
            CallbackSite.Of(this));

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) {
        base.DisplayOptionsDialog(parent, description, extension);
        _ = Callbacks.Answer(FileFormats.DisplayOptionsDialog(this, formats, description, extension), static () => unit, CallbackSite.Of(this));
    }
}

public abstract class DefinedExportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileWriteOptions>> formats) : FileExportPlugIn, IPlugInViews, IPlugInRendering {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition = definition;
    private readonly Seq<FileFormat<FileWriteOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

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
        _ = Callbacks.Answer(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), static () => unit, CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => lifetime.Limits(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [RENDERING]
    public Seq<EffectKind> Effects => definition.Effects;

    public Atom<Option<RenderHistory>> History { get; } = Atom(Option<RenderHistory>.None);

    public Atom<Option<PreviewTiles>> Tiles { get; } = Atom(Option<PreviewTiles>.None);

    public Atom<Option<SlotComparison>> Comparison { get; } = Atom(Option<SlotComparison>.None);

    public Atom<Option<RenderSets>> Sets { get; } = Atom(Option<RenderSets>.None);

    public Atom<Option<NoticeSender>> Notices { get; } = Atom(Option<NoticeSender>.None);

    public Atom<Option<QueueContext>> QueueContext { get; } = Atom(Option<QueueContext>.None);

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        _ = Callbacks.Answer(ViewRegistration.OptionsDialogPages(this, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        _ = Callbacks.Answer(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        _ = Callbacks.Answer(ViewRegistration.ObjectPropertiesPages(this, collection), static () => unit, CallbackSite.Of(this));
    }

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => DocumentArchive.ShouldWrite(definition.DocumentFrames, options, CallbackSite.Of(this));

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        _ = DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        _ = DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileWriteOptions options) =>
        Callbacks.Answer(FileFormats.AddFileTypes(formats, this), static () => new FileTypeList(), CallbackSite.Of(this));

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc? doc, FileWriteOptions options) =>
        Callbacks.Answer(
            IO.lift(Missing.Unless(doc, nameof(RhinoDoc.FromRuntimeSerialNumber)))
                .Bind(document => FileFormats.Run(formats, index, filename, document, options, WriteFileResult.Success, WriteFileResult.Cancel, nameof(WriteFile))),
            static () => WriteFileResult.Failure,
            CallbackSite.Of(this));

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) {
        base.DisplayOptionsDialog(parent, description, extension);
        _ = Callbacks.Answer(FileFormats.DisplayOptionsDialog(this, formats, description, extension), static () => unit, CallbackSite.Of(this));
    }
}

public abstract class DefinedRenderPlugIn(PlugInDefinition definition, RendererDefinition renderer) : RenderPlugIn, IPlugInViews, IPlugInRendering {
    // --- [LIFECYCLE]
    private readonly PlugInDefinition definition = definition;
    private readonly RendererDefinition renderer = renderer;
    private readonly PlugInLifetime lifetime = new(definition, renderer.Views);

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
        _ = Callbacks.Answer(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings), static () => unit, CallbackSite.Of(this));
    }

    public void Report(Error error, Type owner, string member) => lifetime.Session.Report(error, owner, member);

    // --- [VIEWS]
    public ViewCatalog Views => lifetime.Views;

    public TimeProvider Clock => definition.Clock;

    public AtomHashMap<Guid, ValueHistory> Histories => lifetime.Histories;

    public IO<HistoryLimits> Limits => lifetime.Limits(((IPlugInViews)this).Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));

    // --- [RENDERING]
    public Seq<EffectKind> Effects => definition.Effects;

    public Atom<Option<RenderHistory>> History { get; } = Atom(Option<RenderHistory>.None);

    public Atom<Option<PreviewTiles>> Tiles { get; } = Atom(Option<PreviewTiles>.None);

    public Atom<Option<SlotComparison>> Comparison { get; } = Atom(Option<SlotComparison>.None);

    public Atom<Option<RenderSets>> Sets { get; } = Atom(Option<RenderSets>.None);

    public Atom<Option<NoticeSender>> Notices { get; } = Atom(Option<NoticeSender>.None);

    public Atom<Option<QueueContext>> QueueContext { get; } = Atom(Option<QueueContext>.None);

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) {
        base.OptionsDialogPages(pages);
        _ = Callbacks.Answer(ViewRegistration.OptionsDialogPages(this, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) {
        base.DocumentPropertiesDialogPages(doc, pages);
        _ = Callbacks.Answer(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) {
        base.ObjectPropertiesPages(collection);
        _ = Callbacks.Answer(ViewRegistration.ObjectPropertiesPages(this, collection), static () => unit, CallbackSite.Of(this));
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
        _ = DocumentArchive.Write(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        _ = DocumentArchive.Read(definition.DocumentFrames, doc, archive, options, CallbackSite.Of(this));
    }

    // --- [RENDER]
    private const RenderWindow.StandardChannels DefaultChannels =
        global::Rhino.Render.RenderWindow.StandardChannels.RGB
        | global::Rhino.Render.RenderWindow.StandardChannels.NormalXYZ
        | global::Rhino.Render.RenderWindow.StandardChannels.AlbedoRGB;

    protected sealed override Result Render(RhinoDoc doc, RunMode mode, bool fastPreview) =>
        Callbacks.Answer(Conversions.ToResult(Batches.Render(doc, mode, this, renderer.Engine, new RenderStart.Full(fastPreview))), static () => Result.Failure, CallbackSite.Of(this));

    protected sealed override Result RenderWindow(RhinoDoc doc, RunMode modes, bool fastPreview, RhinoView view, Rectangle rect, bool inWindow, bool blowup) =>
        Callbacks.Answer(
            Conversions.ToResult(Batches.Render(doc, modes, this, renderer.Engine, new RenderStart.Region(fastPreview, view, rect, inWindow))),
            static () => Result.Failure,
            CallbackSite.Of(this));

    protected sealed override bool SupportsFeature(RenderFeature feature) => renderer.Features.Contains(feature);

    protected sealed override Guid[] SupportedChannels =>
        [
            .. toSeq(Enum.GetValues<RenderWindow.StandardChannels>())
                .Filter(channel => int.IsPow2((int)channel) && (renderer.Engine.Channels & ~DefaultChannels).HasFlag(channel))
                .Choose(static channel => Conversions.Present(global::Rhino.Render.RenderWindow.ChannelId(channel))),
        ];

    // --- [RENDER_WINDOW]
    protected sealed override void RegisterRenderPanels(RenderPanels panels) {
        base.RegisterRenderPanels(panels);
        _ = Callbacks.Answer(ViewRegistration.RegisterRenderPanels(this, panels), static () => unit, CallbackSite.Of(this));
    }

    protected sealed override void RegisterRenderTabs(RenderTabs tabs) {
        base.RegisterRenderTabs(tabs);
        _ = Callbacks.Answer(ViewRegistration.RegisterRenderTabs(this, tabs), static () => unit, CallbackSite.Of(this));
    }

    // --- [SECTIONS]
    public sealed override void RenderSettingsCustomSections(List<ICollapsibleSection> sections) {
        base.RenderSettingsCustomSections(sections);
        _ = Callbacks.Answer(ViewRegistration.CustomSections(this, renderer.SettingsSections, sections), static () => unit, CallbackSite.Of(this));
    }

    public sealed override void SunCustomSections(List<ICollapsibleSection> sections) {
        base.SunCustomSections(sections);
        _ = Callbacks.Answer(ViewRegistration.CustomSections(this, renderer.SunSections, sections), static () => unit, CallbackSite.Of(this));
    }

    // --- [SAVES]
    protected sealed override void RegisterCustomRenderSaveFileTypes(CustomRenderSaveFileTypes saveFileTypes) {
        base.RegisterCustomRenderSaveFileTypes(saveFileTypes);
        _ = Callbacks.Answer(
            IO.lift(() => renderer.SaveTypes.Iter(type => saveFileTypes.RegisterFileType(
                Seq(type.Target.FileFormat.Extension[1..]),
                RowText.Localize(type.Description, table: Some<object>(this)).Local,
                (fileName, includeAlpha, window) => SaveFile(type.Target with { Alpha = includeAlpha }, fileName, window)))),
            static () => unit,
            CallbackSite.Of(this));
    }

    private bool SaveFile(OutputTarget target, string fileName, RenderWindow window) =>
        Callbacks.Succeeded(
            Documents.WithDocument(new DocumentSource.Active(), document =>
                from path in IO.lift(Conversions.Validated<OutputPath, string, InvalidOutput>(fileName))
                from origin in FileOrigin.At(this)
                from whole in Saves.Whole(window)
                from written in Saves.Write(new SaveSource(document, window), origin, target, path, None, whole)
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
        _ = Callbacks.Answer(
            renderer.Preview.Map(row => row.Content(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))),
            static () => unit,
            static () => unit,
            CallbackSite.Of(this));
    }

    protected sealed override void CreateTexture2dPreview(CreateTexture2dPreviewEventArgs args) {
        base.CreateTexture2dPreview(args);
        _ = Callbacks.Answer(
            renderer.Preview.Map(row => row.Texture(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))),
            static () => unit,
            static () => unit,
            CallbackSite.Of(this));
    }
}
