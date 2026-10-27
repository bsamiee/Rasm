using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.RhinoCommon.Events;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TransformedObjects(Transform Transform, bool ObjectsWillBeCopied, Seq<Guid> ObjectIds, Seq<Guid> GripOwnerIds, int GripCount, Option<uint> Document) {
    public static TransformedObjects Of(RhinoTransformObjectsEventArgs args) =>
        (Conversions.Rows(args.Objects), Conversions.Rows(args.GripOwners)) switch {
            var (objects, owners) => new(
                args.Transform,
                args.ObjectsWillBeCopied,
                objects.Map(static item => item.Id).Strict(),
                owners.Map(static item => item.Id).Strict(),
                args.GripCount,
                (objects + owners).Head.Bind(static item => Conversions.Serial(item.Document))),
        };
}

public sealed record ExceptionReport(string Source, Exception Exception);

public sealed record CloudLogMessage(HostUtils.LogMessageType MessageType, string Class, string Description, string Message);

internal sealed record ViewportMark(Option<uint> Document, Guid Viewport);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EventKind {
    // --- [DOCUMENTS]
    public static readonly DocumentEvent<DocumentOpenEventArgs> BeginOpenDocument = Subscriptions.Watched<DocumentOpenEventArgs>(static (watcher, h) => watcher.BeginOpenDocument += h).Documents();
    public static readonly DocumentEvent<DocumentOpenEventArgs> EndOpenDocument = Subscriptions.Watched<DocumentOpenEventArgs>(static (watcher, h) => watcher.EndOpenDocument += h).Documents();
    public static readonly DocumentEvent<DocumentOpenEventArgs> EndOpenDocumentInitialViewUpdate =
        Subscriptions.Host<DocumentOpenEventArgs>(static h => RhinoDoc.EndOpenDocumentInitialViewUpdate += h, static h => RhinoDoc.EndOpenDocumentInitialViewUpdate -= h).Documents();
    public static readonly DocumentEvent<DocumentSaveEventArgs> BeginSaveDocument = Subscriptions.Host<DocumentSaveEventArgs>(static h => RhinoDoc.BeginSaveDocument += h, static h => RhinoDoc.BeginSaveDocument -= h).Documents();
    public static readonly DocumentEvent<DocumentSaveEventArgs> EndSaveDocument = Subscriptions.Host<DocumentSaveEventArgs>(static h => RhinoDoc.EndSaveDocument += h, static h => RhinoDoc.EndSaveDocument -= h).Documents();
    public static readonly DocumentEvent<DocumentEventArgs> CloseDocument = Subscriptions.Watched<DocumentEventArgs>(static (watcher, h) => watcher.CloseDocument += h).Documents();
    public static readonly DocumentEvent<DocumentEventArgs> NewDocument = Subscriptions.Watched<DocumentEventArgs>(static (watcher, h) => watcher.NewDocument += h).Documents();
    public static readonly DocumentEvent<DocumentEventArgs> ActiveDocumentChanged = Subscriptions.Host<DocumentEventArgs>(static h => RhinoDoc.ActiveDocumentChanged += h, static h => RhinoDoc.ActiveDocumentChanged -= h).Documents();
    public static readonly DocumentEvent<DocumentEventArgs> DocumentPropertiesChanged =
        Subscriptions.Host<DocumentEventArgs>(static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h).Documents();
    public static readonly DocumentEvent<UnitsChangedWithScalingEventArgs> UnitsChangedWithScaling =
        Subscriptions.Host<UnitsChangedWithScalingEventArgs>(static h => RhinoDoc.UnitsChangedWithScaling += h, static h => RhinoDoc.UnitsChangedWithScaling -= h).Documents(static args => args.DocumentSerialNumber);
    public static readonly DocumentEvent<RhinoDoc.UserStringChangedArgs> UserStringChanged =
        Subscriptions.Host<RhinoDoc.UserStringChangedArgs>(static h => RhinoDoc.UserStringChanged += h, static h => RhinoDoc.UserStringChanged -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RhinoDoc.WorksessionFileChangedEventArgs> WorksessionFileChanged =
        Subscriptions.Host<RhinoDoc.WorksessionFileChangedEventArgs>(static h => RhinoDoc.WorksessionFileChanged += h, static h => RhinoDoc.WorksessionFileChanged -= h).Documents(static args => args.Document);
    private static readonly HostEvent<uint> LiveNewDocument =
        Subscriptions.Host<DocumentEventArgs>(static h => RhinoDoc.NewDocument += h, static h => RhinoDoc.NewDocument -= h).Choose(static args => Some(args.DocumentSerialNumber));
    private static readonly HostEvent<uint> LiveEndOpenDocument =
        Subscriptions.Host<DocumentOpenEventArgs>(static h => RhinoDoc.EndOpenDocument += h, static h => RhinoDoc.EndOpenDocument -= h)
            .Choose(static args => Conversions.Found(!args.Merge && !args.Reference, args.DocumentSerialNumber));
    public static readonly HostEvent<uint> DocumentArrived = Subscriptions.Merged(LiveNewDocument, LiveEndOpenDocument) switch {
        var arrivals => arrivals with {
            Inline = (deliver, owner) =>
                from attached in arrivals.Inline(deliver, owner)
                from opened in IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments()))
                from delivered in Callbacks.Each(opened.Map(doc => IO.pure(doc.RuntimeSerialNumber).Bind(deliver)))
                select unit,
        },
    };

    // --- [OBJECTS]
    public static readonly DocumentEvent<RhinoObjectEventArgs> AddRhinoObject = Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.AddRhinoObject += h).Documents(static args => args.TheObject?.Document);
    public static readonly DocumentEvent<RhinoObjectEventArgs> DeleteRhinoObject =
        Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.DeleteRhinoObject += h).Documents(static args => args.TheObject?.Document);
    public static readonly DocumentEvent<RhinoReplaceObjectEventArgs> ReplaceRhinoObject =
        Subscriptions.Watched<RhinoReplaceObjectEventArgs>(static (watcher, h) => watcher.ReplaceRhinoObject += h).Documents(static args => args.DocumentSerialNumber);
    public static readonly DocumentEvent<RhinoObjectEventArgs> UndeleteRhinoObject =
        Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.UndeleteRhinoObject += h).Documents(static args => args.TheObject?.Document);
    public static readonly DocumentEvent<RhinoObjectEventArgs> PurgeRhinoObject =
        Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.PurgeRhinoObject += h).Documents(static args => args.TheObject?.Document);
    public static readonly DocumentEvent<RhinoModifyObjectAttributesEventArgs> ModifyObjectAttributes =
        Subscriptions.Host<RhinoModifyObjectAttributesEventArgs>(static h => RhinoDoc.ModifyObjectAttributes += h, static h => RhinoDoc.ModifyObjectAttributes -= h).Documents(static args => args.DocumentSerialNumber);
    public static readonly DocumentEvent<RhinoObjectSelectionEventArgs> SelectObjects =
        Subscriptions.Host<RhinoObjectSelectionEventArgs>(static h => RhinoDoc.SelectObjects += h, static h => RhinoDoc.SelectObjects -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RhinoObjectSelectionEventArgs> DeselectObjects =
        Subscriptions.Host<RhinoObjectSelectionEventArgs>(static h => RhinoDoc.DeselectObjects += h, static h => RhinoDoc.DeselectObjects -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RhinoDeselectAllObjectsEventArgs> DeselectAllObjects =
        Subscriptions.Host<RhinoDeselectAllObjectsEventArgs>(static h => RhinoDoc.DeselectAllObjects += h, static h => RhinoDoc.DeselectAllObjects -= h).Documents(static args => args.Document);
    private static readonly HostEvent<RhinoTransformObjectsEventArgs> BeforeTransformObjects =
        Subscriptions.Host<RhinoTransformObjectsEventArgs>(static h => RhinoDoc.BeforeTransformObjects += h, static h => RhinoDoc.BeforeTransformObjects -= h);
    private static readonly HostEvent<RhinoAfterTransformObjectsEventArgs> AfterTransformObjects =
        Subscriptions.Host<RhinoAfterTransformObjectsEventArgs>(static h => RhinoDoc.AfterTransformObjects += h, static h => RhinoDoc.AfterTransformObjects -= h);
    public static readonly DocumentEvent<TransformedObjects> TransformObjects = new(
        $"{BeforeTransformObjects.Label} | {AfterTransformObjects.Label}",
        static (deliver, owner) =>
            from pending in IO.lift(static () => Ref(HashMap<uint, TransformedObjects>()))
            from before in BeforeTransformObjects.Inline(
                args => atomicIO(() => pending.Swap(held => held.AddOrUpdate(args.TransformEventId, TransformedObjects.Of(args)))).Map(unit),
                owner)
            from after in AfterTransformObjects.Inline(
                args =>
                    from taken in atomicIO(() => {
                        Option<TransformedObjects> found = pending.Value.Find(args.TransformEventId);
                        pending.Value = pending.Value.Remove(args.TransformEventId);
                        return found;
                    })
                    from delivered in taken.Traverse(deliver).As()
                    select unit,
                owner)
            select unit,
        static args => args.Document);

    // --- [TABLES]
    public static readonly DocumentEvent<LayerTableEventArgs> LayerTableEvent =
        Subscriptions.Host<LayerTableEventArgs>(static h => RhinoDoc.LayerTableEvent += h, static h => RhinoDoc.LayerTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<MaterialTableEventArgs> MaterialTableEvent =
        Subscriptions.Host<MaterialTableEventArgs>(static h => RhinoDoc.MaterialTableEvent += h, static h => RhinoDoc.MaterialTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<GroupTableEventArgs> GroupTableEvent =
        Subscriptions.Host<GroupTableEventArgs>(static h => RhinoDoc.GroupTableEvent += h, static h => RhinoDoc.GroupTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<LinetypeTableEventArgs> LinetypeTableEvent =
        Subscriptions.Host<LinetypeTableEventArgs>(static h => RhinoDoc.LinetypeTableEvent += h, static h => RhinoDoc.LinetypeTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<LightTableEventArgs> LightTableEvent =
        Subscriptions.Host<LightTableEventArgs>(static h => RhinoDoc.LightTableEvent += h, static h => RhinoDoc.LightTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<DimStyleTableEventArgs> DimensionStyleTableEvent =
        Subscriptions.Host<DimStyleTableEventArgs>(static h => RhinoDoc.DimensionStyleTableEvent += h, static h => RhinoDoc.DimensionStyleTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<InstanceDefinitionTableEventArgs> InstanceDefinitionTableEvent =
        Subscriptions.Host<InstanceDefinitionTableEventArgs>(static h => RhinoDoc.InstanceDefinitionTableEvent += h, static h => RhinoDoc.InstanceDefinitionTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<SectionStyleTableEventArgs> SectionStyleTableEvent =
        Subscriptions.Host<SectionStyleTableEventArgs>(static h => RhinoDoc.SectionStyleTableEvent += h, static h => RhinoDoc.SectionStyleTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<MarkupTableEventArgs> MarkupTableEvent =
        Subscriptions.Host<MarkupTableEventArgs>(static h => RhinoDoc.MarkupTableEvent += h, static h => RhinoDoc.MarkupTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<MarkupViewChangedEventArgs> MarkupViewChanged =
        Subscriptions.Host<MarkupViewChangedEventArgs>(static h => RhinoDoc.MarkupViewChanged += h, static h => RhinoDoc.MarkupViewChanged -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<PageViewGroupTableEventArgs> PageViewGroupTableEvent =
        Subscriptions.Host<PageViewGroupTableEventArgs>(static h => RhinoDoc.PageViewGroupTableEvent += h, static h => RhinoDoc.PageViewGroupTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<HatchPatternTableEventArgs> HatchPatternTableEvent =
        Subscriptions.Host<HatchPatternTableEventArgs>(static h => RhinoDoc.HatchPatternTableEvent += h, static h => RhinoDoc.HatchPatternTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RhinoDoc.TextureMappingEventArgs> TextureMappingEvent =
        Subscriptions.Host<RhinoDoc.TextureMappingEventArgs>(static h => RhinoDoc.TextureMappingEvent += h, static h => RhinoDoc.TextureMappingEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RhinoDoc.RenderContentTableEventArgs> RenderMaterialsTableEvent =
        Subscriptions.Host<RhinoDoc.RenderContentTableEventArgs>(static h => RhinoDoc.RenderMaterialsTableEvent += h, static h => RhinoDoc.RenderMaterialsTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RhinoDoc.RenderContentTableEventArgs> RenderEnvironmentTableEvent =
        Subscriptions.Host<RhinoDoc.RenderContentTableEventArgs>(static h => RhinoDoc.RenderEnvironmentTableEvent += h, static h => RhinoDoc.RenderEnvironmentTableEvent -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RhinoDoc.RenderContentTableEventArgs> RenderTextureTableEvent =
        Subscriptions.Host<RhinoDoc.RenderContentTableEventArgs>(static h => RhinoDoc.RenderTextureTableEvent += h, static h => RhinoDoc.RenderTextureTableEvent -= h).Documents(static args => args.Document);

    // --- [CONTENT]
    public static readonly DocumentEvent<RenderContentEventArgs> ContentAdded =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.ContentAdded += h, static h => RenderContent.ContentAdded -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentEventArgs> ContentRenamed =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.ContentRenamed += h, static h => RenderContent.ContentRenamed -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentEventArgs> ContentDeleting =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.ContentDeleting += h, static h => RenderContent.ContentDeleting -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentEventArgs> ContentDeleted =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.ContentDeleted += h, static h => RenderContent.ContentDeleted -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentEventArgs> ContentReplacing =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.ContentReplacing += h, static h => RenderContent.ContentReplacing -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentEventArgs> ContentReplaced =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.ContentReplaced += h, static h => RenderContent.ContentReplaced -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentEventArgs> ContentUpdatePreview =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.ContentUpdatePreview += h, static h => RenderContent.ContentUpdatePreview -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentEventArgs> CurrentEnvironmentChanged =
        Subscriptions.Host<RenderContentEventArgs>(static h => RenderContent.CurrentEnvironmentChanged += h, static h => RenderContent.CurrentEnvironmentChanged -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentChangedEventArgs> ContentChanged =
        Subscriptions.Host<RenderContentChangedEventArgs>(static h => RenderContent.ContentChanged += h, static h => RenderContent.ContentChanged -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderContentFieldChangedEventArgs> ContentFieldChanged =
        Subscriptions.Host<RenderContentFieldChangedEventArgs>(static h => RenderContent.ContentFieldChanged += h, static h => RenderContent.ContentFieldChanged -= h).Documents(static args => args.Document);
    public static readonly HostEvent<PreviewRenderedEventArgs> PreviewRendered = Subscriptions.Host<PreviewRenderedEventArgs>(static h => RenderContent.PreviewRendered += h, static h => RenderContent.PreviewRendered -= h);
    public static readonly DocumentEvent<RenderPropertyChangedEvent> GroundPlaneChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(static h => GroundPlane.Changed += h, static h => GroundPlane.Changed -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderPropertyChangedEvent> SkylightChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(static h => Skylight.Changed += h, static h => Skylight.Changed -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderPropertyChangedEvent> SunChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(static h => Sun.Changed += h, static h => Sun.Changed -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderPropertyChangedEvent> SafeFrameChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(static h => SafeFrame.Changed += h, static h => SafeFrame.Changed -= h).Documents(static args => args.Document);
    public static readonly DocumentEvent<RenderPropertyChangedEvent> RenderChannelsChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(static h => RenderChannels.Changed += h, static h => RenderChannels.Changed -= h).Documents(static args => args.Document);
    public static readonly HostEvent<AddCustomUISectionsEventArgs> OnAddCustomUISections =
        Subscriptions.Host<AddCustomUISectionsEventArgs>(static h => AddCustomUISections.OnAddCustomUISections += h, static h => AddCustomUISections.OnAddCustomUISections -= h);
    public static readonly HostEvent<ImageFileEventArgs> ImageFileSaved = Subscriptions.Host<ImageFileEventArgs>(static h => Rhino.Render.ImageFile.Saved += h, static h => Rhino.Render.ImageFile.Saved -= h);
    public static readonly HostEvent<ImageFileEventArgs> ImageFileLoaded = Subscriptions.Host<ImageFileEventArgs>(static h => Rhino.Render.ImageFile.Loaded += h, static h => Rhino.Render.ImageFile.Loaded -= h);
    public static readonly HostEvent<ImageFileEventArgs> ImageFileDeleted = Subscriptions.Host<ImageFileEventArgs>(static h => Rhino.Render.ImageFile.Deleted += h, static h => Rhino.Render.ImageFile.Deleted -= h);

    // --- [VIEWS]
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewCreate = Subscriptions.Host<ViewEventArgs>(static h => RhinoView.Create += h, static h => RhinoView.Create -= h).Documents(static args => args.View.Document);
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewDestroy = Subscriptions.Host<ViewEventArgs>(static h => RhinoView.Destroy += h, static h => RhinoView.Destroy -= h).Documents(static args => args.View.Document);
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewSetActive = Subscriptions.Host<ViewEventArgs>(static h => RhinoView.SetActive += h, static h => RhinoView.SetActive -= h).Documents(static args => args.View.Document);
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewRename = Subscriptions.Host<ViewEventArgs>(static h => RhinoView.Rename += h, static h => RhinoView.Rename -= h).Documents(static args => args.View.Document);
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewModified = Subscriptions.Host<ViewEventArgs>(static h => RhinoView.Modified += h, static h => RhinoView.Modified -= h).Documents(static args => args.View.Document);
    public static readonly DocumentEvent<ViewEnableDrawingEventArgs> EnableDrawingChanged =
        Subscriptions.Host<ViewEnableDrawingEventArgs>(static h => RhinoView.EnableDrawingChanged += h, static h => RhinoView.EnableDrawingChanged -= h).Documents(static args => args.DocumentSerialNumber);
    public static readonly DocumentEvent<PageViewSpaceChangeEventArgs> PageViewSpaceChange =
        Subscriptions.Host<PageViewSpaceChangeEventArgs>(static h => RhinoPageView.PageViewSpaceChange += h, static h => RhinoPageView.PageViewSpaceChange -= h).Documents(static args => args.PageView?.Document);
    public static readonly DocumentEvent<PageViewPropertiesChangeEventArgs> PageViewPropertiesChange =
        Subscriptions.Host<PageViewPropertiesChangeEventArgs>(static h => RhinoPageView.PageViewPropertiesChange += h, static h => RhinoPageView.PageViewPropertiesChange -= h).Documents(static args => args.DocumentSerialNumber);
    public static readonly DocumentEvent<DisplayModeChangedEventArgs> DisplayModeChanged =
        Subscriptions.During<DisplayModeChangedEventArgs>(static h => DisplayPipeline.DisplayModeChanged += h, static h => DisplayPipeline.DisplayModeChanged -= h).Documents(static args => args.RhinoDoc);
    public static readonly DocumentEvent<DrawEventArgs> ViewportProjectionChanged =
        Subscriptions.During<DrawEventArgs>(static h => DisplayPipeline.ViewportProjectionChanged += h, static h => DisplayPipeline.ViewportProjectionChanged -= h)
            .DistinctUntilChanged(static args => new ViewportMark(Conversions.Serial(args.RhinoDoc), args.Viewport.Id), static args => args.Viewport.ChangeCounter)
            .Documents(static args => args.RhinoDoc);

    // --- [DRAW]
    public static readonly DocumentEvent<DrawForegroundEventArgs> DrawForeground =
        Subscriptions.During<EventHandler<DrawEventArgs>, DrawForegroundEventArgs>(
                static h => DisplayPipeline.DrawForeground += h, static h => DisplayPipeline.DrawForeground -= h, static handler => (sender, args) => handler(sender, (DrawForegroundEventArgs)args))
            .Documents(static args => args.RhinoDoc);
    public static readonly HostEvent<InitFrameBufferEventArgs> InitFrameBuffer = Subscriptions.During<InitFrameBufferEventArgs>(static h => DisplayPipeline.InitFrameBuffer += h, static h => DisplayPipeline.InitFrameBuffer -= h);
    public static readonly DocumentEvent<DrawEventArgs> PreDrawTransparentObjects =
        Subscriptions.During<DrawEventArgs>(static h => DisplayPipeline.PreDrawTransparentObjects += h, static h => DisplayPipeline.PreDrawTransparentObjects -= h).Documents(static args => args.RhinoDoc);
    public static readonly DocumentEvent<DrawObjectEventArgs> PostDrawObject =
        Subscriptions.During<DrawObjectEventArgs>(static h => DisplayPipeline.PostDrawObject += h, static h => DisplayPipeline.PostDrawObject -= h).Documents(static args => args.RhinoDoc);
    public static readonly DocumentEvent<DrawEventArgs> PostDrawObjects =
        Subscriptions.During<DrawEventArgs>(static h => DisplayPipeline.PostDrawObjects += h, static h => DisplayPipeline.PostDrawObjects -= h).Documents(static args => args.RhinoDoc);

    // --- [SHELL]
    public static readonly DocumentEvent<ShowPanelEventArgs> PanelsShow = Subscriptions.Host<ShowPanelEventArgs>(static h => Panels.Show += h, static h => Panels.Show -= h).Documents(static args => args.DocumentSerialNumber);
    public static readonly DocumentEvent<PanelEventArgs> PanelsClosed = Subscriptions.Host<PanelEventArgs>(static h => Panels.Closed += h, static h => Panels.Closed -= h).Documents(static args => args.DocumentSerialNumber);
    public static readonly DocumentEvent<CommandEventArgs> BeginCommand =
        Subscriptions.Host<CommandEventArgs>(static h => Command.BeginCommand += h, static h => Command.BeginCommand -= h).Documents(static args => args.DocumentRuntimeSerialNumber);
    public static readonly DocumentEvent<CommandEventArgs> EndCommand =
        Subscriptions.Host<CommandEventArgs>(static h => Command.EndCommand += h, static h => Command.EndCommand -= h).Documents(static args => args.DocumentRuntimeSerialNumber);
    public static readonly DocumentEvent<UndoRedoEventArgs> UndoRedo = Subscriptions.Host<UndoRedoEventArgs>(static h => Command.UndoRedo += h, static h => Command.UndoRedo -= h).Documents(static args => args.DocumentSerialNumber);
    public static readonly HostEvent<CommandPromptChangedEventArgs> CommandPromptChanged =
        Subscriptions.Host<CommandPromptChangedEventArgs>(static h => RhinoApp.CommandPromptChanged += h, static h => RhinoApp.CommandPromptChanged -= h);
    public static readonly HostEvent<EventArgs> EscapeKeyPressed = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.EscapeKeyPressed += h, static h => RhinoApp.EscapeKeyPressed -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> ThemeChanged = Subscriptions.Host<EventHandler, EventArgs>(static h => ThemeSettings.ThemeChanged += h, static h => ThemeSettings.ThemeChanged -= h, static handler => handler.Invoke);

    // --- [APPLICATION]
    public static readonly HostEvent<LicenseStateChangedEventArgs> LicenseStateChanged =
        Subscriptions.Host<LicenseStateChangedEventArgs>(static h => RhinoApp.LicenseStateChanged += h, static h => RhinoApp.LicenseStateChanged -= h);
    public static readonly HostEvent<EventArgs> RhinoAppInitialized = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.Initialized += h, static h => RhinoApp.Initialized -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RhinoAppClosing = Subscriptions.During<EventHandler, EventArgs>(static h => RhinoApp.Closing += h, static h => RhinoApp.Closing -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> AppSettingsChanged = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.AppSettingsChanged += h, static h => RhinoApp.AppSettingsChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RendererChanged = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.RendererChanged += h, static h => RhinoApp.RendererChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkNewDocument = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.RdkNewDocument += h, static h => RhinoApp.RdkNewDocument -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkGlobalSettingsChanged =
        Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.RdkGlobalSettingsChanged += h, static h => RhinoApp.RdkGlobalSettingsChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkUpdateAllPreviews = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.RdkUpdateAllPreviews += h, static h => RhinoApp.RdkUpdateAllPreviews -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkCacheImageChanged = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.RdkCacheImageChanged += h, static h => RhinoApp.RdkCacheImageChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkPlugInUnloading = Subscriptions.Host<EventHandler, EventArgs>(static h => RhinoApp.RdkPlugInUnloading += h, static h => RhinoApp.RdkPlugInUnloading -= h, static handler => handler.Invoke);
    public static readonly HostEvent<ExceptionReport> OnExceptionReport = Subscriptions.Host<HostUtils.ExceptionReportDelegate, ExceptionReport>(
        static h => HostUtils.OnExceptionReport += h, static h => HostUtils.OnExceptionReport -= h, static handler => (source, exception) => handler(sender: null, new ExceptionReport(source, exception)));
    public static readonly HostEvent<CloudLogMessage> OnSendLogMessageToCloud = Subscriptions.Host<HostUtils.SendLogMessageToCloudDelegate, CloudLogMessage>(
        static h => HostUtils.OnSendLogMessageToCloud += h, static h => HostUtils.OnSendLogMessageToCloud -= h,
        static handler => (messageType, area, description, message) => handler(sender: null, new CloudLogMessage(messageType, area, description, message)));

    // --- [PLUGIN]
    public static HostEvent<PersistentSettingsSavedEventArgs> SettingsSaved(PlugIn plugIn) =>
        Subscriptions.Host<PersistentSettingsSavedEventArgs>(h => plugIn.SettingsSaved += h, h => plugIn.SettingsSaved -= h);

    // --- [FILES]
    public static readonly HostEvent<FilePdfEventArgs> FilePdfPreWrite = Subscriptions.During<FilePdfEventArgs>(static h => FilePdf.PreWrite += h, static h => FilePdf.PreWrite -= h);

    public static HostEvent<Seq<Either<ErrorEventArgs, FileSystemEventArgs>>> FileChanges(DirectoryInfo directory, Seq<string> filters, bool subdirectories, Duration quiet, TimeProvider clock) =>
        new(directory.FullName, (deliver, owner) =>
            from debouncer in use(() => new Debouncer<Either<ErrorEventArgs, FileSystemEventArgs>>(clock, quiet, deliver, owner))
            from watcher in use(() => new FileSystemWatcher(directory.FullName) { NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size, IncludeSubdirectories = subdirectories })
                .Catch(
                    static error => error.HasException<ArgumentException>() || error.HasException<FileNotFoundException>(),
                    error => IO.fail<FileSystemWatcher>(new RhinoFault(directory.FullName, RhinoReason.Missing, Inner: error)))
            from changed in Seq(
                    Subscriptions.Host<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Changed += h, h => watcher.Changed -= h, static handler => handler.Invoke),
                    Subscriptions.Host<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Created += h, h => watcher.Created -= h, static handler => handler.Invoke),
                    Subscriptions.Host<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Deleted += h, h => watcher.Deleted -= h, static handler => handler.Invoke),
                    Subscriptions.Host<RenamedEventHandler, FileSystemEventArgs>(h => watcher.Renamed += h, h => watcher.Renamed -= h, static handler => handler.Invoke))
                .TraverseM(row => row.Inline(args => debouncer.Post(Right(args)), owner)).As()
            from failed in Subscriptions.Host<ErrorEventHandler, ErrorEventArgs>(h => watcher.Error += h, h => watcher.Error -= h, static handler => handler.Invoke)
                .Inline(args => debouncer.Post(Left(args)), owner)
            from enabled in IO.lift(() => {
                _ = filters.Iter(watcher.Filters.Add);
                watcher.EnableRaisingEvents = true;
            })
            select unit);
}
