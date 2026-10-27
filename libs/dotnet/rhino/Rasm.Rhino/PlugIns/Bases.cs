using System.Collections.Generic;
using System.Drawing;
using Rasm.RhinoCommon.Commands;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Render.Sessions;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.PlugIns;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FileFormats {
    // --- [LIST]
    public static IO<FileTypeList> AddFileTypes<TOptions>(Seq<FileFormat<TOptions>> formats, IPlugInViews plugIn) where TOptions : class =>
        from list in IO.lift(static () => new FileTypeList())
        from added in IO.lift(() => Callbacks.Each(
            formats,
            (format, index) => RhinoFault.Unless(list.AddFileType(RowText.Localize(format.Description, plugIn).Local, format.Extensions.Map(static extension => (string)extension), format.Options.IsSome) >= 0, index)))
        select list;

    // --- [RUN]
    public static IO<A> Run<TOptions, A>(Seq<FileFormat<TOptions>> formats, int index, string path, RhinoDoc doc, TOptions options, A done, A cancelled) where TOptions : class =>
        (from format in IO.lift(Conversions.Within(index, formats.Count).Map(_ => formats[index]))
         from ran in format.Run(path, doc, options)
         select done)
            .Catch(static error => error.Is(Errors.Cancelled), _ => IO.pure(cancelled));

    // --- [OPTIONS]
    public static IO<Unit> DisplayOptionsDialog<TOptions>(IPlugInViews plugIn, Seq<FileFormat<TOptions>> formats, string description, string extension) where TOptions : class =>
        (from dialog in IO.lift(() => formats
             .Find(format =>
                 string.Equals(RowText.Localize(format.Description, plugIn).Local, description, StringComparison.Ordinal)
                 && string.Equals(((string)format.Extensions.Head)[1..], extension, StringComparison.Ordinal))
             .Bind(static format => format.Options)
             .ToFin(new RhinoFault($"{description} {extension}", RhinoReason.Missing)))
         from shown in Documents.WithDocument(new DocumentSource.Active(), document => Showing.ShowSemiModal(plugIn, dialog, document))
         select unit)
            .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedPlugIn(PlugInDefinition definition) : PlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly PlugInLifetime lifetime = new(definition, Seq<View>());

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        _ = this.Run(lifetime.Commands(RegisterCommand));
    }

    protected sealed override void OnShutdown() => _ = this.Run(lifetime.Session.Close);

    protected sealed override void ResetMessageBoxes() => _ = this.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => this.Answer(lifetime.DisplayHelp, failed: false);

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.OptionsDialogPages(this, pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = this.Run(ViewRegistration.ObjectPropertiesPages(this, collection));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => this.Answer(DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options), failed: false);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => _ = this.Run(DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options));

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => _ = this.Run(DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options));
}

public abstract class DefinedImportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileReadOptions>> formats) : FileImportPlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly Seq<FileFormat<FileReadOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        _ = this.Run(lifetime.Commands(RegisterCommand));
    }

    protected sealed override void OnShutdown() => _ = this.Run(lifetime.Session.Close);

    protected sealed override void ResetMessageBoxes() => _ = this.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => this.Answer(lifetime.DisplayHelp, failed: false);

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.OptionsDialogPages(this, pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = this.Run(ViewRegistration.ObjectPropertiesPages(this, collection));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => this.Answer(DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options), failed: false);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => _ = this.Run(DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options));

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => _ = this.Run(DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options));

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileReadOptions options) => this.Answer(FileFormats.AddFileTypes(formats, this), new FileTypeList());

    protected sealed override bool ReadFile(string filename, int index, RhinoDoc? doc, FileReadOptions options) =>
        this.Answer(IO.lift(() => RhinoFault.Found(doc)).Bind(document => FileFormats.Run(formats, index, filename, document, options, done: true, cancelled: false)), failed: false);

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) => _ = this.Run(FileFormats.DisplayOptionsDialog(this, formats, description, extension));
}

public abstract class DefinedExportPlugIn(PlugInDefinition definition, Seq<FileFormat<FileWriteOptions>> formats) : FileExportPlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly Seq<FileFormat<FileWriteOptions>> formats = formats;
    private readonly PlugInLifetime lifetime = new(definition, formats.Choose(static format => format.Options.Map<View>(static dialog => dialog)));

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        _ = this.Run(lifetime.Commands(RegisterCommand));
    }

    protected sealed override void OnShutdown() => _ = this.Run(lifetime.Session.Close);

    protected sealed override void ResetMessageBoxes() => _ = this.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings));

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => this.Answer(lifetime.DisplayHelp, failed: false);

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.OptionsDialogPages(this, pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = this.Run(ViewRegistration.ObjectPropertiesPages(this, collection));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => this.Answer(DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options), failed: false);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => _ = this.Run(DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options));

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => _ = this.Run(DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options));

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileWriteOptions options) => this.Answer(FileFormats.AddFileTypes(formats, this), new FileTypeList());

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc? doc, FileWriteOptions options) =>
        this.Answer(
            IO.lift(() => RhinoFault.Found(doc)).Bind(document => FileFormats.Run(formats, index, filename, document, options, WriteFileResult.Success, WriteFileResult.Cancel)),
            WriteFileResult.Failure);

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) => _ = this.Run(FileFormats.DisplayOptionsDialog(this, formats, description, extension));
}

public abstract class DefinedRenderPlugIn(PlugInDefinition definition, RendererDefinition renderer) : RenderPlugIn, IDefinedPlugIn {
    // --- [LIFECYCLE]
    private readonly RendererDefinition renderer = renderer;
    private readonly PlugInLifetime lifetime = new(definition, renderer.Views);

    PlugInLifetime IDefinedPlugIn.Lifetime => lifetime;

    public sealed override PlugInLoadTime LoadTime => lifetime.LoadTime(base.LoadTime);

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => lifetime.Load(this, GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() {
        base.CreateCommands();
        _ = this.Run(lifetime.Commands(RegisterCommand));
    }

    protected sealed override void OnShutdown() {
        base.OnShutdown();
        _ = this.Run(lifetime.Session.Close);
    }

    protected sealed override void ResetMessageBoxes() {
        base.ResetMessageBoxes();
        _ = this.Run(HostDialogs.ResetMessageBoxes(lifetime.Views, ((IPlugInViews)this).Settings));
    }

    // --- [IDENTITY]
    public sealed override object? GetPlugInObject() => lifetime.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => lifetime.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => this.Answer(lifetime.DisplayHelp, failed: false);

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.OptionsDialogPages(this, pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = this.Run(ViewRegistration.DocumentPropertiesDialogPages(this, doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = this.Run(ViewRegistration.ObjectPropertiesPages(this, collection));

    protected sealed override OptionsDialogPage? RenderOptionsDialogPage(RhinoDoc doc) =>
        this.Answer(renderer.RenderPage.Map(row => ViewRegistration.RenderOptionsDialogPage(this, row, doc)).IfNone(IO.pure(Option<OptionsDialogPage>.None)), Option<OptionsDialogPage>.None)
            .IfNone(() => base.RenderOptionsDialogPage(doc));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => this.Answer(DocumentArchive.ShouldWrite(lifetime.Definition.DocumentFrames, options), failed: false);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) {
        base.WriteDocument(doc, archive, options);
        _ = this.Run(DocumentArchive.Write(lifetime.Definition.DocumentFrames, doc, archive, options));
    }

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) {
        base.ReadDocument(doc, archive, options);
        _ = this.Run(DocumentArchive.Read(lifetime.Definition.DocumentFrames, doc, archive, options));
    }

    // --- [RENDER]
    protected sealed override Result Render(RhinoDoc doc, RunMode mode, bool fastPreview) =>
        this.Answer(Results.Of(Batches.Render(doc, mode, this, renderer.Engine, new RenderStart.Full(fastPreview))), Result.Failure);

    protected sealed override Result RenderWindow(RhinoDoc doc, RunMode modes, bool fastPreview, RhinoView view, Rectangle rect, bool inWindow, bool blowup) =>
        this.Answer(Results.Of(Batches.Render(doc, modes, this, renderer.Engine, new RenderStart.Region(fastPreview, view, rect, inWindow))), Result.Failure);

    protected sealed override bool SupportsFeature(RenderFeature feature) => renderer.Features.Contains(feature);

    protected sealed override Guid[] SupportedChannels => [.. renderer.Engine.SupportedChannels];

    // --- [RENDER_WINDOW]
    protected sealed override void RegisterRenderPanels(RenderPanels panels) {
        base.RegisterRenderPanels(panels);
        _ = this.Run(ViewRegistration.RegisterRenderPanels(this, panels));
    }

    protected sealed override void RegisterRenderTabs(RenderTabs tabs) {
        base.RegisterRenderTabs(tabs);
        _ = this.Run(ViewRegistration.RegisterRenderTabs(this, tabs));
    }

    // --- [SECTIONS]
    public sealed override void RenderSettingsCustomSections(List<ICollapsibleSection> sections) {
        base.RenderSettingsCustomSections(sections);
        _ = this.Run(ViewRegistration.CustomSections(this, renderer.SettingsSections, sections));
    }

    public sealed override void SunCustomSections(List<ICollapsibleSection> sections) {
        base.SunCustomSections(sections);
        _ = this.Run(ViewRegistration.CustomSections(this, renderer.SunSections, sections));
    }

    // --- [SAVES]
    protected sealed override void RegisterCustomRenderSaveFileTypes(CustomRenderSaveFileTypes saveFileTypes) {
        base.RegisterCustomRenderSaveFileTypes(saveFileTypes);
        _ = this.Run(IO.lift(() => renderer.SaveTypes.Iter(type => saveFileTypes.RegisterFileType(
            Seq(type.Extension),
            RowText.Localize(type.Description, this).Local,
            (fileName, includeAlpha, window) => this.Answer(type.Write(window, fileName, includeAlpha).Map(value: true), failed: false)))));
    }

    // --- [CONTENT]
    protected sealed override List<Guid> UiContentTypes() =>
        [
            .. base.UiContentTypes(),
            .. this.Answer(Products.Made(RenderContentType.GetAllAvailableTypes()).Map(types => types.Filter(type => type.PlugInId == Id).Map(static type => type.Id).Strict()), Seq<Guid>()),
        ];

    protected sealed override PreviewRenderTypes PreviewRenderType() =>
        renderer.Preview.Map(static row => row.Type).IfNone(base.PreviewRenderType);

    protected sealed override void CreatePreview(CreatePreviewEventArgs args) {
        base.CreatePreview(args);
        _ = this.Run(renderer.Preview.Map(row => row.Content(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))).IfNone(IO.pure(unit)));
    }

    protected sealed override void CreateTexture2dPreview(CreateTexture2dPreviewEventArgs args) {
        base.CreateTexture2dPreview(args);
        _ = this.Run(renderer.Preview.Map(row => row.Texture(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))).IfNone(IO.pure(unit)));
    }
}
