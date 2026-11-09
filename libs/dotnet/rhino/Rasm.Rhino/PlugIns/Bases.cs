using System.Reflection;
using Eto;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.PlugIns.Licenses;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.CustomRenderMeshes;
using Rhino.Render.PostEffects;
using Rhino.Runtime;
using Rhino.UI;
using LicenseRequest = System.Func<Rhino.PlugIns.LicenseCapabilities, string?, Rhino.PlugIns.ValidateProductKeyDelegate, Rhino.PlugIns.OnLeaseChangedDelegate, bool>;
using PlugInMembers = (
    Rhino.PlugIns.PlugInLoadTime LoadTime,
    System.Func<(Rhino.PlugIns.LoadReturnCode Code, string Message)> OnLoad,
    System.Action CreateCommands,
    System.Action OnShutdown,
    System.Action ResetMessageBoxes,
    object? PlugInObject,
    bool AddToHelpMenu,
    System.Func<bool> DisplayHelp,
    System.Action<System.Collections.Generic.List<Rhino.UI.OptionsDialogPage>> OptionsDialogPages,
    System.Action<Rhino.RhinoDoc, System.Collections.Generic.List<Rhino.UI.OptionsDialogPage>> DocumentPropertiesDialogPages,
    System.Action<Rhino.UI.ObjectPropertiesPageCollection> ObjectPropertiesPages,
    System.Func<Rhino.FileIO.FileWriteOptions, bool> ShouldCallWriteDocument,
    System.Action<Rhino.RhinoDoc, Rhino.FileIO.BinaryArchiveWriter, Rhino.FileIO.FileWriteOptions> WriteDocument,
    System.Action<Rhino.RhinoDoc, Rhino.FileIO.BinaryArchiveReader, Rhino.FileIO.FileReadOptions> ReadDocument);
using RenderMembers = (
    System.Func<Rhino.RhinoDoc, Rhino.UI.OptionsDialogPage?> RenderOptionsDialogPage,
    System.Func<Rhino.RhinoDoc, Rhino.Commands.RunMode, bool, Rhino.Commands.Result> Render,
    System.Func<Rhino.RhinoDoc, Rhino.Commands.RunMode, bool, Rhino.Display.RhinoView, System.Drawing.Rectangle, bool, Rhino.Commands.Result> RenderWindow,
    System.Func<Rhino.PlugIns.RenderPlugIn.RenderFeature, bool> SupportsFeature,
    System.Action<bool> OnSetCurrent,
    System.Func<System.Collections.Generic.List<Rhino.FileIO.FileType>, System.Collections.Generic.List<Rhino.FileIO.FileType>> SupportedOutputTypes,
    System.Func<System.Collections.Generic.IEnumerable<Rhino.Render.RenderContentSerializer>> RenderContentSerializers,
    System.Guid[] SupportedChannels,
    System.Guid? InitialChannelToDisplay,
    System.Func<System.Guid, string> CustomChannelName,
    bool SupportsEditProperties,
    System.Func<System.Collections.Generic.List<Rhino.Render.NamedValue>, bool> ShowDecalProperties,
    System.Action<System.Collections.Generic.List<Rhino.Render.NamedValue>> InitializeDecalProperties,
    bool EnableAssignMaterialButton,
    System.Func<Rhino.RhinoDoc, Rhino.DocObjects.Material, (bool Pressed, Rhino.DocObjects.Material Material)> OnAssignMaterial,
    bool EnableEditMaterialButton,
    System.Func<Rhino.RhinoDoc, Rhino.DocObjects.Material, (bool Pressed, Rhino.DocObjects.Material Material)> OnEditMaterial,
    bool EnableCreateMaterialButton,
    System.Func<Rhino.RhinoDoc, Rhino.DocObjects.Material, (bool Pressed, Rhino.DocObjects.Material Material)> OnCreateMaterial,
    System.Func<Rhino.Render.RenderContent, bool> AllowChooseContent,
    System.Action<Rhino.Render.RenderPanels> RegisterRenderPanels,
    System.Action<Rhino.Render.RenderTabs> RegisterRenderTabs,
    System.Action<System.Collections.Generic.List<Rhino.UI.Controls.ICollapsibleSection>> RenderSettingsCustomSections,
    System.Action<System.Collections.Generic.List<Rhino.UI.Controls.ICollapsibleSection>> SunCustomSections,
    System.Action<Rhino.PlugIns.CustomRenderSaveFileTypes> RegisterCustomRenderSaveFileTypes,
    System.Func<System.Collections.Generic.IEnumerable<System.Guid>> UiContentTypes,
    Rhino.PlugIns.RenderPlugIn.PreviewRenderTypes? PreviewRenderType,
    System.Action<Rhino.Render.CreatePreviewEventArgs> CreatePreview,
    System.Action<Rhino.Render.CreateTexture2dPreviewEventArgs> CreateTexture2dPreview);

namespace Rasm.Rhino.PlugIns;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class PlugInBody {

    // --- [STATE]

    private readonly PlugIn host;
    private readonly Assembly assembly;
    private readonly PlugInDefinition definition;
    private readonly ViewCatalog catalog;
    private readonly LicenseRequest license;
    private readonly Func<Command, bool> register;

    public PlugInBody(PlugIn host, Type defined, PlugInDefinition definition, Seq<View> kindViews, Func<Command, bool> register, LicenseRequest license) {
        (this.host, assembly, this.definition, this.register, this.license) = (host, defined.Assembly, definition, register, license);
        catalog = new ViewCatalog(definition, definition.Views.Concat(kindViews));
        Platform.Instance.LoadAssembly(typeof(PlugInBody).Assembly);
        _ = Session.RunSafe(Session.Bind(host, defined, definition.Logging), CancellationToken.None).ThrowIfFail();
    }

    // --- [MEMBERS]

    public PlugInMembers Members =>
        (LoadTime, Load, () => _ = Commands(), static () => _ = Session.Run(Session.Close), static () => _ = Session.Run(HostDialogs.ResetMessageBoxes(ViewCatalog.Current)),
            definition.Published.ValueUnsafe(), definition.Help.IsSome, DisplayHelp,
            static pages => _ = Session.Run(ViewRegistration.OptionsDialogPages(pages)),
            static (doc, pages) => _ = Session.Run(ViewRegistration.DocumentPropertiesDialogPages(doc, pages)),
            static collection => _ = Session.Run(ViewRegistration.ObjectPropertiesPages(collection)),
            options => Session.Run(DocumentArchive.ShouldWrite(definition.Frames, options), failed: false),
            (doc, archive, options) => _ = Session.Run(DocumentArchive.Write(definition.Frames, doc, archive, options)),
            (doc, archive, options) => _ = Session.Run(DocumentArchive.Read(definition.Frames, doc, archive, options)));

    private PlugInLoadTime LoadTime =>
        definition.LoadTime.IfNone(() =>
            definition.Modes.IsSome || !definition.Themes.IsEmpty ? PlugInLoadTime.AtStartup
            : catalog.Rows.Exists(static view => view is View.Panel) ? PlugInLoadTime.WhenNeededOrTabbedDockBar
            : catalog.Rows.Exists(static view => view is View.Page) ? PlugInLoadTime.WhenNeededOrOptionsDialog
            : PlugInLoadTime.WhenNeeded);

    // --- [LIFECYCLE]

    private (LoadReturnCode Code, string Message) Load() =>
        Session.RunSafe(
                Loading.Scoped().Map(static _ => (LoadReturnCode.Success, ""))
                    .Catch(static error => IO.lift(() => Session.Failed(error)) >> Session.Close
                        >> IO.pure(Session.Cancellation(error) ? (LoadReturnCode.ErrorNoDialog, "") : (LoadReturnCode.ErrorShowDialog, Session.Messages(error))))
                    .As(),
                CancellationToken.None)
            .IfFail(static error => (LoadReturnCode.ErrorShowDialog, Session.Messages(error)));

    private IO<Unit> Loading =>
        from licensed in definition.License.TraverseM(held => Licensing.Request(held, license)).As()
        from dependencies in PlugInRegistry.LoadPlugIns(definition.Dependencies)
        from views in ViewCatalog.Load(catalog)
        from rewrites in DocumentArchive.Rewrites
        from following in EffectList.Following
        from endpoints in IO.lift(() => definition.Endpoints.Iter(static endpoint => HostUtils.RegisterComputeEndpoint(endpoint.Path, endpoint.Type)))
        from modes in definition.Modes.Traverse(DisplayModes.Declared).As()
        from themes in when(!definition.Themes.IsEmpty, RhinoTheme.Following(definition.Themes)).As()
        from rendering in Rendering
        from loaded in definition.Load.TraverseM(identity).As()
        select unit;

    private IO<Unit> Rendering =>
        IO.lift(() => new ScannedAssembly(assembly)).Bind(scanned => Products.Each(Seq(
            IO.lift(() => RenderContent.RegisterContent(scanned, host.Id)).Map(unit),
            IO.lift(() => PostEffect.RegisterPostEffect(scanned, host.Id)).Map(unit),
            IO.lift(() => RenderMeshProvider.RegisterProviders(scanned, host)),
            IO.lift(() => RealtimeDisplayMode.RegisterDisplayModes(scanned, host.Id)).Map(unit),
            IO.lift(() => LightManagerSupport.RegisterProviders(scanned, host.Id)))).Map(unit));

    private Unit Commands() =>
        Session.Run(Products.Each(definition.Commands.Map((row, index) =>
            from command in IO.lift(row.Realized)
            from registered in IO.lift(RhinoFault.Unless(register(command), RhinoReason.Declined, row.EnglishName, index))
            from history in when(row.History.Exists(static history => history.ReplayOnAttributeChange), IO.lift(() => {
                command.HistoryReplayOnObjectAttributeChange = true;
            })).As()
            select unit)));

    private bool DisplayHelp() => Session.Run(definition.Help.Map(static topic => topic.Show.Map(value: true)).IfNone(IO.pure(value: false)), failed: false);
}

internal sealed class ScannedAssembly(Assembly assembly) : Assembly {
    public override string? FullName => assembly.FullName;

    public override Type[] GetExportedTypes() => assembly.GetTypes();
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class FileFormats {

    // --- [MEMBERS]

    public static (Func<TOptions, FileTypeList> AddFileTypes, Func<string, int, RhinoDoc, TOptions, TResult> File, Action<string, string> DisplayOptionsDialog) Members<TOptions, TResult>(
        IterableNE<FileFormat<TOptions>> formats, TResult done, TResult cancelled, TResult failed) where TOptions : class =>
        (_ => AddFileTypes(formats), (path, index, doc, options) => Run(formats, index, path, doc, options, done, cancelled, failed), (description, extension) => _ = DisplayOptionsDialog(formats, description, extension));

    public static Seq<View> Dialogs<TOptions>(IterableNE<FileFormat<TOptions>> formats) where TOptions : class =>
        toSeq(formats).Choose(static format => format.Options.Map<View>(static dialog => dialog));

    // --- [FILES]

    private static FileTypeList AddFileTypes<TOptions>(IterableNE<FileFormat<TOptions>> formats) where TOptions : class =>
        Session.Run(
            from list in IO.lift(static () => new FileTypeList())
            from added in Products.Each(toSeq(formats).Map((format, index) => IO.lift(() => RhinoFault.Unless(
                list.AddFileType(LocalText.Of(format.Description).Local, format.Extensions.Map(static extension => (string)extension), format.Options.IsSome) >= 0,
                RhinoReason.Declined,
                format.Description,
                index))))
            select list,
            new FileTypeList());

    private static T Run<TOptions, T>(IterableNE<FileFormat<TOptions>> formats, int index, string path, RhinoDoc doc, TOptions options, T done, T cancelled, T failed) where TOptions : class =>
        Session.Run(
            (from format in IO.lift(() => Conversions.Required(toSeq(formats).At(index), index))
             from ran in format.Run(path, doc, options)
             select done)
                | @catch(static error => error.Is(Errors.Cancelled), _ => IO.pure(cancelled)),
            failed);

    private static Unit DisplayOptionsDialog<TOptions>(IterableNE<FileFormat<TOptions>> formats, string description, string extension) where TOptions : class =>
        Session.Run(
            (from dialog in IO.lift(() => toSeq(formats)
                 .Find(format =>
                     string.Equals(LocalText.Of(format.Description).Local, description, StringComparison.Ordinal)
                     && format.Extensions.Any(candidate => string.Equals(((string)candidate)[1..], extension, StringComparison.Ordinal)))
                 .Bind(static format => format.Options)
                 .ToFin(new RhinoFault((description, extension), RhinoReason.Missing)))
             from shown in Documents.With(new DocumentSource.Active(), document => Showing.ShowSemiModal(dialog, document))
             select unit)
                | @catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public static partial class PlugInShell {

    // --- [PLUG_INS]

    public static PlugInMembers PlugIn<TDefinition>(PlugIn host, Func<Command, bool> register, LicenseRequest license) where TDefinition : IDefined<PlugInDefinition> =>
        new PlugInBody(host, typeof(TDefinition), TDefinition.Definition, [], register, license).Members;

    public static (PlugInMembers PlugIn, (Func<FileReadOptions, FileTypeList> AddFileTypes, Func<string, int, RhinoDoc, FileReadOptions, bool> File, Action<string, string> DisplayOptionsDialog) Formats) Import<TDefinition>(
        FileImportPlugIn host, Func<Command, bool> register, LicenseRequest license) where TDefinition : IDefined<FileDefinition<FileReadOptions>> =>
        (new PlugInBody(host, typeof(TDefinition), TDefinition.Definition.PlugIn, FileFormats.Dialogs(TDefinition.Definition.Formats), register, license).Members,
            FileFormats.Members(TDefinition.Definition.Formats, done: true, cancelled: false, failed: false));

    public static (PlugInMembers PlugIn, (Func<FileWriteOptions, FileTypeList> AddFileTypes, Func<string, int, RhinoDoc, FileWriteOptions, WriteFileResult> File, Action<string, string> DisplayOptionsDialog) Formats, bool ShouldDisplayOptionsDialog) Export<TDefinition>(
        FileExportPlugIn host, Func<Command, bool> register, LicenseRequest license) where TDefinition : IDefined<FileDefinition<FileWriteOptions>> =>
        (new PlugInBody(host, typeof(TDefinition), TDefinition.Definition.PlugIn, FileFormats.Dialogs(TDefinition.Definition.Formats), register, license).Members,
            FileFormats.Members(TDefinition.Definition.Formats, WriteFileResult.Success, WriteFileResult.Cancel, WriteFileResult.Failure),
            TDefinition.Definition.Formats.Exists(static format => format.Options.IsSome));

    // --- [RENDERERS]

    public static (PlugInMembers PlugIn, RenderMembers Render) Render<TDefinition>(
        RenderPlugIn host, Func<Command, bool> register, LicenseRequest license) where TDefinition : IDefined<RendererDefinition> =>
        (new PlugInBody(host, typeof(TDefinition), TDefinition.Definition.PlugIn,
                [.. TDefinition.Definition.Tabs, .. TDefinition.Definition.Panes, .. TDefinition.Definition.SettingsSections, .. TDefinition.Definition.SunSections], register, license).Members,
            Rendered(host, TDefinition.Definition));

    private static RenderMembers Rendered(RenderPlugIn host, RendererDefinition definition) =>
        (doc => Session.Run(definition.RenderPage.Map(row => ViewRegistration.RenderOptionsDialogPage(row, doc)).IfNone(IO.pure(Option<OptionsDialogPage>.None)), Option<OptionsDialogPage>.None).ValueUnsafe(),
            (doc, mode, fastPreview) => Session.Run(Batches.Render(doc, mode, definition.Engine, new RenderStart.Full(fastPreview)), Result.Failure),
            (doc, mode, fastPreview, view, rect, inWindow) => Session.Run(Batches.Render(doc, mode, definition.Engine, new RenderStart.Region(fastPreview, view, rect, inWindow)), Result.Failure),
            definition.Features.Contains,
            definition.Engine.OnSetCurrent,
            definition.Engine.SupportedOutputTypes,
            definition.Engine.RenderContentSerializers,
            [.. definition.Engine.SupportedChannels],
            definition.Engine.InitialChannelId.Map<Guid?>(static id => id).ValueUnsafe(),
            definition.Engine.CustomChannelName,
            definition.Engine.EditDecal.IsSome,
            definition.Engine.ShowDecalProperties,
            definition.Engine.InitializeDecalProperties,
            definition.Engine.AssignMaterial.IsSome,
            (doc, material) => RenderEngine.Pressed(definition.Engine.AssignMaterial, doc, material),
            definition.Engine.EditMaterial.IsSome,
            (doc, material) => RenderEngine.Pressed(definition.Engine.EditMaterial, doc, material),
            definition.Engine.CreateMaterial.IsSome,
            (doc, material) => RenderEngine.Pressed(definition.Engine.CreateMaterial, doc, material),
            definition.Engine.Choosable,
            static panels => _ = Session.Run(ViewRegistration.RegisterRenderPanels(panels)),
            static tabs => _ = Session.Run(ViewRegistration.RegisterRenderTabs(tabs)),
            sections => _ = Session.Run(ViewRegistration.CustomSections(definition.SettingsSections, sections)),
            sections => _ = Session.Run(ViewRegistration.CustomSections(definition.SunSections, sections)),
            types => _ = Session.Run(IO.lift(() => definition.SaveTypes.Iter(type => types.RegisterFileType(
                Seq(type.Extension),
                LocalText.Of(type.Description).Local,
                (fileName, includeAlpha, window) => Session.Run(type.Write(window, fileName, includeAlpha).Map(value: true), failed: false))))),
            () => Session.Run(Products.Many(RenderContentType.GetAllAvailableTypes).Map(types => types.Filter(type => type.PlugInId == host.Id).Map(static type => type.Id).Strict()), Seq<Guid>()),
            definition.Preview.Map<RenderPlugIn.PreviewRenderTypes?>(static row => row.Type).ValueUnsafe(),
            args => _ = Session.Run(definition.Preview.Traverse(row => row.Content(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))).As()),
            args => _ = Session.Run(definition.Preview.Traverse(row => row.Texture(args).Bind(image => IO.lift(() => image.Iter(bitmap => args.PreviewImage = bitmap)))).As()));
}
