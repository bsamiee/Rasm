using System.Collections.Generic;
using System.Drawing;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.PlugIns;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FileFormats {
    // --- [VIEWS]
    public static Seq<View> Dialogs<TOptions>(IterableNE<FileFormat<TOptions>> formats) where TOptions : class =>
        toSeq(formats).Choose(static format => format.Options.Map<View>(static dialog => dialog));

    // --- [LIST]
    public static FileTypeList AddFileTypes<TOptions>(IterableNE<FileFormat<TOptions>> formats) where TOptions : class =>
        Session.Run(
            from list in IO.lift(static () => new FileTypeList())
            from added in Products.Each(toSeq(formats).Map((format, index) => IO.lift(() => RhinoFault.Unless(
                list.AddFileType(LocalText.Of(format.Description).Local, format.Extensions.Map(static extension => (string)extension), format.Options.IsSome) >= 0,
                RhinoReason.Declined,
                format.Description,
                index))))
            select list,
            new FileTypeList());

    // --- [RUN]
    public static T Run<TOptions, T>(IterableNE<FileFormat<TOptions>> formats, int index, string path, RhinoDoc doc, TOptions options, T done, T cancelled, T failed) where TOptions : class =>
        Session.Run(
            (from format in IO.lift(() => Conversions.Required(toSeq(formats).At(index), index))
             from ran in format.Run(path, doc, options)
             select done)
                .Catch(static error => error.Is(Errors.Cancelled), _ => IO.pure(cancelled)),
            failed);

    // --- [OPTIONS]
    public static Unit DisplayOptionsDialog<TOptions>(IterableNE<FileFormat<TOptions>> formats, string description, string extension) where TOptions : class =>
        Session.Run(
            (from dialog in IO.lift(() => toSeq(formats)
                 .Find(format =>
                     string.Equals(LocalText.Of(format.Description).Local, description, StringComparison.Ordinal)
                     && string.Equals(((string)format.Extensions.Head)[1..], extension, StringComparison.Ordinal))
                 .Bind(static format => format.Options)
                 .ToFin(new RhinoFault($"{description} {extension}", RhinoReason.Missing)))
             from shown in Documents.With(new DocumentSource.Active(), document => Showing.ShowSemiModal(dialog, document))
             select unit)
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedPlugIn<TSelf> : PlugIn where TSelf : DefinedPlugIn<TSelf>, IDefined<PlugInDefinition> {
    private readonly PlugInBody body;

    protected DefinedPlugIn() => body = new(this, TSelf.Definition, []);

    // --- [LIFECYCLE]
    public sealed override PlugInLoadTime LoadTime => body.LoadTime;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => body.Load(GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() => _ = body.Commands(RegisterCommand);

    protected sealed override void OnShutdown() => _ = Session.Run(Session.Close);

    protected sealed override void ResetMessageBoxes() => _ = Session.Run(HostDialogs.ResetMessageBoxes(ViewCatalog.Current));

    public sealed override object? GetPlugInObject() => TSelf.Definition.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => TSelf.Definition.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => body.DisplayHelp();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.OptionsDialogPages(pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.DocumentPropertiesDialogPages(doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = Session.Run(ViewRegistration.ObjectPropertiesPages(collection));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => body.ShouldCallWriteDocument(options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => _ = body.WriteDocument(doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => _ = body.ReadDocument(doc, archive, options);
}

public abstract class DefinedImportPlugIn<TSelf> : FileImportPlugIn where TSelf : DefinedImportPlugIn<TSelf>, IDefined<FileDefinition<FileReadOptions>> {
    private readonly PlugInBody body;

    protected DefinedImportPlugIn() => body = new(this, TSelf.Definition.PlugIn, FileFormats.Dialogs(TSelf.Definition.Formats));

    // --- [LIFECYCLE]
    public sealed override PlugInLoadTime LoadTime => body.LoadTime;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => body.Load(GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() => _ = body.Commands(RegisterCommand);

    protected sealed override void OnShutdown() => _ = Session.Run(Session.Close);

    protected sealed override void ResetMessageBoxes() => _ = Session.Run(HostDialogs.ResetMessageBoxes(ViewCatalog.Current));

    public sealed override object? GetPlugInObject() => TSelf.Definition.PlugIn.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => TSelf.Definition.PlugIn.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => body.DisplayHelp();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.OptionsDialogPages(pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.DocumentPropertiesDialogPages(doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = Session.Run(ViewRegistration.ObjectPropertiesPages(collection));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => body.ShouldCallWriteDocument(options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => _ = body.WriteDocument(doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => _ = body.ReadDocument(doc, archive, options);

    // --- [FILES]
    protected sealed override FileTypeList AddFileTypes(FileReadOptions options) => FileFormats.AddFileTypes(TSelf.Definition.Formats);

    protected sealed override bool ReadFile(string filename, int index, RhinoDoc doc, FileReadOptions options) =>
        FileFormats.Run(TSelf.Definition.Formats, index, filename, doc, options, done: true, cancelled: false, failed: false);

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) => _ = FileFormats.DisplayOptionsDialog(TSelf.Definition.Formats, description, extension);
}

public abstract class DefinedExportPlugIn<TSelf> : FileExportPlugIn where TSelf : DefinedExportPlugIn<TSelf>, IDefined<FileDefinition<FileWriteOptions>> {
    private readonly PlugInBody body;

    protected DefinedExportPlugIn() => body = new(this, TSelf.Definition.PlugIn, FileFormats.Dialogs(TSelf.Definition.Formats));

    // --- [LIFECYCLE]
    public sealed override PlugInLoadTime LoadTime => body.LoadTime;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => body.Load(GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() => _ = body.Commands(RegisterCommand);

    protected sealed override void OnShutdown() => _ = Session.Run(Session.Close);

    protected sealed override void ResetMessageBoxes() => _ = Session.Run(HostDialogs.ResetMessageBoxes(ViewCatalog.Current));

    public sealed override object? GetPlugInObject() => TSelf.Definition.PlugIn.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => TSelf.Definition.PlugIn.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => body.DisplayHelp();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.OptionsDialogPages(pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.DocumentPropertiesDialogPages(doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = Session.Run(ViewRegistration.ObjectPropertiesPages(collection));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => body.ShouldCallWriteDocument(options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => _ = body.WriteDocument(doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => _ = body.ReadDocument(doc, archive, options);

    // --- [FILES]
    protected sealed override bool ShouldDisplayOptionsDialog => TSelf.Definition.Formats.Exists(static format => format.Options.IsSome);

    protected sealed override FileTypeList AddFileTypes(FileWriteOptions options) => FileFormats.AddFileTypes(TSelf.Definition.Formats);

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc doc, FileWriteOptions options) =>
        FileFormats.Run(TSelf.Definition.Formats, index, filename, doc, options, WriteFileResult.Success, WriteFileResult.Cancel, WriteFileResult.Failure);

    protected sealed override void DisplayOptionsDialog(nint parent, string description, string extension) => _ = FileFormats.DisplayOptionsDialog(TSelf.Definition.Formats, description, extension);
}

public abstract class DefinedRenderPlugIn<TSelf> : RenderPlugIn where TSelf : DefinedRenderPlugIn<TSelf>, IDefined<RendererDefinition> {
    private readonly PlugInBody body;

    protected DefinedRenderPlugIn() =>
        body = new(this, TSelf.Definition.PlugIn, [.. TSelf.Definition.Tabs, .. TSelf.Definition.Panes, .. TSelf.Definition.SettingsSections, .. TSelf.Definition.SunSections]);

    // --- [LIFECYCLE]
    public sealed override PlugInLoadTime LoadTime => body.LoadTime;

    protected sealed override LoadReturnCode OnLoad(ref string errorMessage) => body.Load(GetLicense, ref errorMessage);

    protected sealed override void CreateCommands() => _ = body.Commands(RegisterCommand);

    protected sealed override void OnShutdown() => _ = Session.Run(Session.Close);

    protected sealed override void ResetMessageBoxes() => _ = Session.Run(HostDialogs.ResetMessageBoxes(ViewCatalog.Current));

    public sealed override object? GetPlugInObject() => TSelf.Definition.PlugIn.Published.ValueUnsafe();

    // --- [HELP]
    public sealed override bool AddToHelpMenu => TSelf.Definition.PlugIn.Help.IsSome;

    public sealed override bool DisplayHelp(nint windowHandle) => body.DisplayHelp();

    // --- [PAGES]
    protected sealed override void OptionsDialogPages(List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.OptionsDialogPages(pages));

    protected sealed override void DocumentPropertiesDialogPages(RhinoDoc doc, List<OptionsDialogPage> pages) => _ = Session.Run(ViewRegistration.DocumentPropertiesDialogPages(doc, pages));

    protected sealed override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection) => _ = Session.Run(ViewRegistration.ObjectPropertiesPages(collection));

    protected sealed override OptionsDialogPage? RenderOptionsDialogPage(RhinoDoc doc) =>
        Session.Run(TSelf.Definition.RenderPage.Map(row => ViewRegistration.RenderOptionsDialogPage(row, doc)).IfNone(IO.pure(Option<OptionsDialogPage>.None)), Option<OptionsDialogPage>.None)
            .IfNone(() => base.RenderOptionsDialogPage(doc));

    // --- [ARCHIVE]
    protected sealed override bool ShouldCallWriteDocument(FileWriteOptions options) => body.ShouldCallWriteDocument(options);

    protected sealed override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options) => _ = body.WriteDocument(doc, archive, options);

    protected sealed override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options) => _ = body.ReadDocument(doc, archive, options);

    // --- [RENDER]
    protected sealed override Result Render(RhinoDoc doc, RunMode mode, bool fastPreview) =>
        Session.Run(Batches.Render(doc, mode, TSelf.Definition.Engine, new RenderStart.Full(fastPreview)), Result.Failure);

    protected sealed override Result RenderWindow(RhinoDoc doc, RunMode modes, bool fastPreview, RhinoView view, Rectangle rect, bool inWindow, bool blowup) =>
        Session.Run(Batches.Render(doc, modes, TSelf.Definition.Engine, new RenderStart.Region(fastPreview, view, rect, inWindow)), Result.Failure);

    protected sealed override bool SupportsFeature(RenderFeature feature) => TSelf.Definition.Features.Contains(feature);

    protected sealed override void OnSetCurrent(bool current) => TSelf.Definition.Engine.OnSetCurrent(current);

    protected sealed override List<FileType> SupportedOutputTypes() => TSelf.Definition.Engine.SupportedOutputTypes(base.SupportedOutputTypes());

    protected sealed override IEnumerable<RenderContentSerializer> RenderContentSerializers() => TSelf.Definition.Engine.RenderContentSerializers();

    // --- [CHANNELS]
    protected sealed override Guid[] SupportedChannels => [.. TSelf.Definition.Engine.SupportedChannels];

    protected sealed override Guid InitialChannelToDisplay => TSelf.Definition.Engine.InitialChannelId.IfNone(base.InitialChannelToDisplay);

    protected sealed override string CustomChannelName(Guid id) => TSelf.Definition.Engine.CustomChannelName(id);

    // --- [DECALS]
    protected sealed override bool SupportsEditProperties => TSelf.Definition.Engine.EditDecal.IsSome;

    protected sealed override bool ShowDecalProperties(ref List<NamedValue> properties) => TSelf.Definition.Engine.ShowDecalProperties(ref properties);

    protected sealed override void InitializeDecalProperties(ref List<NamedValue> properties) => TSelf.Definition.Engine.InitializeDecalProperties(ref properties);

    // --- [MATERIALS]
    public sealed override bool EnableAssignMaterialButton() => TSelf.Definition.Engine.AssignMaterial.IsSome;

    public sealed override bool OnAssignMaterial(nint parent, RhinoDoc doc, ref Material material) => RenderEngine.Pressed(TSelf.Definition.Engine.AssignMaterial, doc, ref material);

    public sealed override bool EnableEditMaterialButton(RhinoDoc doc, Material material) => TSelf.Definition.Engine.EditMaterial.IsSome;

    public sealed override bool OnEditMaterial(nint parent, RhinoDoc doc, ref Material material) => RenderEngine.Pressed(TSelf.Definition.Engine.EditMaterial, doc, ref material);

    public sealed override bool EnableCreateMaterialButton() => TSelf.Definition.Engine.CreateMaterial.IsSome;

    public sealed override bool OnCreateMaterial(nint parent, RhinoDoc doc, ref Material material) => RenderEngine.Pressed(TSelf.Definition.Engine.CreateMaterial, doc, ref material);

    protected sealed override bool AllowChooseContent(RenderContent content) => TSelf.Definition.Engine.Choosable(content);

    // --- [RENDER_WINDOW]
    protected sealed override void RegisterRenderPanels(RenderPanels panels) => _ = Session.Run(ViewRegistration.RegisterRenderPanels(panels));

    protected sealed override void RegisterRenderTabs(RenderTabs tabs) => _ = Session.Run(ViewRegistration.RegisterRenderTabs(tabs));

    // --- [SECTIONS]
    public sealed override void RenderSettingsCustomSections(List<ICollapsibleSection> sections) => _ = Session.Run(ViewRegistration.CustomSections(TSelf.Definition.SettingsSections, sections));

    public sealed override void SunCustomSections(List<ICollapsibleSection> sections) => _ = Session.Run(ViewRegistration.CustomSections(TSelf.Definition.SunSections, sections));

    // --- [SAVES]
    protected sealed override void RegisterCustomRenderSaveFileTypes(CustomRenderSaveFileTypes saveFileTypes) =>
        _ = Session.Run(IO.lift(() => TSelf.Definition.SaveTypes.Iter(type => saveFileTypes.RegisterFileType(
            Seq(type.Extension),
            LocalText.Of(type.Description).Local,
            (fileName, includeAlpha, window) => Session.Run(type.Write(window, fileName, includeAlpha).Map(value: true), failed: false)))));

    // --- [CONTENT]
    protected sealed override List<Guid> UiContentTypes() =>
        [
            .. base.UiContentTypes(),
            .. Session.Run(Products.Many(RenderContentType.GetAllAvailableTypes).Map(types => types.Filter(type => type.PlugInId == Id).Map(static type => type.Id).Strict()), Seq<Guid>()),
        ];

    protected sealed override PreviewRenderTypes PreviewRenderType() => TSelf.Definition.Preview.Map(static row => row.Type).IfNone(base.PreviewRenderType);

    protected sealed override void CreatePreview(CreatePreviewEventArgs args) =>
        _ = Session.Run(TSelf.Definition.Preview.Traverse(row => row.Content(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))).As());

    protected sealed override void CreateTexture2dPreview(CreateTexture2dPreviewEventArgs args) =>
        _ = Session.Run(TSelf.Definition.Preview.Traverse(row => row.Texture(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))).As());
}
