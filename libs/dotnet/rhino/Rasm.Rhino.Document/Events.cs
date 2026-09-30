using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EventKind(Func<Func<Option<uint>, IO<Unit>>, Action<Error>, IO<IDisposable>> Attach) {
    // --- [LIFECYCLE]
    public static readonly EventKind BeginOpenDocument = Watched<DocumentOpenEventArgs>(static (w, h) => w.BeginOpenDocument += h, Serial);

    public static readonly EventKind EndOpenDocument = Watched<DocumentOpenEventArgs>(static (w, h) => w.EndOpenDocument += h, Serial);

    public static readonly EventKind EndOpenDocumentInitialViewUpdate =
        Host<DocumentOpenEventArgs>(static h => RhinoDoc.EndOpenDocumentInitialViewUpdate += h, static h => RhinoDoc.EndOpenDocumentInitialViewUpdate -= h, Serial);

    public static readonly EventKind BeginSaveDocument = Host<DocumentSaveEventArgs>(static h => RhinoDoc.BeginSaveDocument += h, static h => RhinoDoc.BeginSaveDocument -= h, Serial);

    public static readonly EventKind EndSaveDocument = Host<DocumentSaveEventArgs>(static h => RhinoDoc.EndSaveDocument += h, static h => RhinoDoc.EndSaveDocument -= h, Serial);

    public static readonly EventKind CloseDocument = Watched<DocumentEventArgs>(static (w, h) => w.CloseDocument += h, Serial);

    public static readonly EventKind NewDocument = Watched<DocumentEventArgs>(static (w, h) => w.NewDocument += h, Serial);

    public static readonly EventKind ActiveDocumentChanged = Host<DocumentEventArgs>(static h => RhinoDoc.ActiveDocumentChanged += h, static h => RhinoDoc.ActiveDocumentChanged -= h, Serial);

    public static readonly EventKind DocumentPropertiesChanged = Host<DocumentEventArgs>(static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h, Serial);

    public static readonly EventKind UnitsChangedWithScaling = Host<UnitsChangedWithScalingEventArgs>(
        static h => RhinoDoc.UnitsChangedWithScaling += h, static h => RhinoDoc.UnitsChangedWithScaling -= h, static a => Answers.Present(a.DocumentSerialNumber));

    public static readonly EventKind UserStringChanged = Host<RhinoDoc.UserStringChangedArgs>(
        static h => RhinoDoc.UserStringChanged += h, static h => RhinoDoc.UserStringChanged -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind WorksessionFileChanged = Host<RhinoDoc.WorksessionFileChangedEventArgs>(
        static h => RhinoDoc.WorksessionFileChanged += h, static h => RhinoDoc.WorksessionFileChanged -= h, static a => DocumentHandles.Serial(a.Document));

    // --- [STRUCTURE]
    public static readonly EventKind AddRhinoObject = Watched<RhinoObjectEventArgs>(static (w, h) => w.AddRhinoObject += h, Serial);

    public static readonly EventKind DeleteRhinoObject = Watched<RhinoObjectEventArgs>(static (w, h) => w.DeleteRhinoObject += h, Serial);

    public static readonly EventKind ReplaceRhinoObject = Watched<RhinoReplaceObjectEventArgs>(static (w, h) => w.ReplaceRhinoObject += h, static a => Answers.Present(a.DocumentSerialNumber));

    public static readonly EventKind UndeleteRhinoObject = Watched<RhinoObjectEventArgs>(static (w, h) => w.UndeleteRhinoObject += h, Serial);

    public static readonly EventKind PurgeRhinoObject = Watched<RhinoObjectEventArgs>(static (w, h) => w.PurgeRhinoObject += h, Serial);

    public static readonly EventKind ModifyObjectAttributes = Host<RhinoModifyObjectAttributesEventArgs>(
        static h => RhinoDoc.ModifyObjectAttributes += h, static h => RhinoDoc.ModifyObjectAttributes -= h, static a => Answers.Present(a.DocumentSerialNumber));

    public static readonly EventKind TransformObjects = new(Transforms);

    // --- [SELECTION]
    public static readonly EventKind SelectObjects = Host<RhinoObjectSelectionEventArgs>(static h => RhinoDoc.SelectObjects += h, static h => RhinoDoc.SelectObjects -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind DeselectObjects = Host<RhinoObjectSelectionEventArgs>(static h => RhinoDoc.DeselectObjects += h, static h => RhinoDoc.DeselectObjects -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind DeselectAllObjects = Host<RhinoDeselectAllObjectsEventArgs>(
        static h => RhinoDoc.DeselectAllObjects += h, static h => RhinoDoc.DeselectAllObjects -= h, static a => DocumentHandles.Serial(a.Document));

    // --- [TABLES]
    public static readonly EventKind LayerTableEvent = Host<LayerTableEventArgs>(static h => RhinoDoc.LayerTableEvent += h, static h => RhinoDoc.LayerTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind MaterialTableEvent = Host<MaterialTableEventArgs>(static h => RhinoDoc.MaterialTableEvent += h, static h => RhinoDoc.MaterialTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind GroupTableEvent = Host<GroupTableEventArgs>(static h => RhinoDoc.GroupTableEvent += h, static h => RhinoDoc.GroupTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind LinetypeTableEvent = Host<LinetypeTableEventArgs>(static h => RhinoDoc.LinetypeTableEvent += h, static h => RhinoDoc.LinetypeTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind LightTableEvent = Host<LightTableEventArgs>(static h => RhinoDoc.LightTableEvent += h, static h => RhinoDoc.LightTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind DimensionStyleTableEvent = Host<DimStyleTableEventArgs>(
        static h => RhinoDoc.DimensionStyleTableEvent += h, static h => RhinoDoc.DimensionStyleTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind InstanceDefinitionTableEvent = Host<InstanceDefinitionTableEventArgs>(
        static h => RhinoDoc.InstanceDefinitionTableEvent += h, static h => RhinoDoc.InstanceDefinitionTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind SectionStyleTableEvent = Host<SectionStyleTableEventArgs>(
        static h => RhinoDoc.SectionStyleTableEvent += h, static h => RhinoDoc.SectionStyleTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind MarkupTableEvent = Host<MarkupTableEventArgs>(static h => RhinoDoc.MarkupTableEvent += h, static h => RhinoDoc.MarkupTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind PageViewGroupTableEvent = Host<PageViewGroupTableEventArgs>(
        static h => RhinoDoc.PageViewGroupTableEvent += h, static h => RhinoDoc.PageViewGroupTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind HatchPatternTableEvent = Host<HatchPatternTableEventArgs>(
        static h => RhinoDoc.HatchPatternTableEvent += h, static h => RhinoDoc.HatchPatternTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind RenderMaterialsTableEvent = Host<RhinoDoc.RenderContentTableEventArgs>(
        static h => RhinoDoc.RenderMaterialsTableEvent += h, static h => RhinoDoc.RenderMaterialsTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind RenderEnvironmentTableEvent = Host<RhinoDoc.RenderContentTableEventArgs>(
        static h => RhinoDoc.RenderEnvironmentTableEvent += h, static h => RhinoDoc.RenderEnvironmentTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind RenderTextureTableEvent = Host<RhinoDoc.RenderContentTableEventArgs>(
        static h => RhinoDoc.RenderTextureTableEvent += h, static h => RhinoDoc.RenderTextureTableEvent -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind TextureMappingEvent = Host<RhinoDoc.TextureMappingEventArgs>(
        static h => RhinoDoc.TextureMappingEvent += h, static h => RhinoDoc.TextureMappingEvent -= h, static a => DocumentHandles.Serial(a.Document));

    // --- [SCREEN]
    public static readonly EventKind RhinoViewCreate = Host<ViewEventArgs>(static h => RhinoView.Create += h, static h => RhinoView.Create -= h, static a => DocumentHandles.Serial(a.View.Document));

    public static readonly EventKind RhinoViewDestroy = Host<ViewEventArgs>(static h => RhinoView.Destroy += h, static h => RhinoView.Destroy -= h, static a => DocumentHandles.Serial(a.View.Document));

    public static readonly EventKind RhinoViewSetActive = Host<ViewEventArgs>(static h => RhinoView.SetActive += h, static h => RhinoView.SetActive -= h, static a => DocumentHandles.Serial(a.View.Document));

    public static readonly EventKind RhinoViewRename = Host<ViewEventArgs>(static h => RhinoView.Rename += h, static h => RhinoView.Rename -= h, static a => DocumentHandles.Serial(a.View.Document));

    public static readonly EventKind RhinoViewModified = Host<ViewEventArgs>(static h => RhinoView.Modified += h, static h => RhinoView.Modified -= h, static a => DocumentHandles.Serial(a.View.Document));

    public static readonly EventKind ViewportProjectionChanged = new(Projections);

    public static readonly EventKind DisplayModeChanged = Host<DisplayModeChangedEventArgs>(
        static h => DisplayPipeline.DisplayModeChanged += h, static h => DisplayPipeline.DisplayModeChanged -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    // --- [DRAW]
    public static readonly EventKind DrawForeground = Host<DrawEventArgs>(static h => DisplayPipeline.DrawForeground += h, static h => DisplayPipeline.DrawForeground -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    public static readonly EventKind DrawOverlay = Host<DrawEventArgs>(static h => DisplayPipeline.DrawOverlay += h, static h => DisplayPipeline.DrawOverlay -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    public static readonly EventKind ObjectCulling = Host<CullObjectEventArgs>(static h => DisplayPipeline.ObjectCulling += h, static h => DisplayPipeline.ObjectCulling -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    public static readonly EventKind InitFrameBuffer = Host<InitFrameBufferEventArgs>(static h => DisplayPipeline.InitFrameBuffer += h, static h => DisplayPipeline.InitFrameBuffer -= h, static _ => None);

    public static readonly EventKind PreDrawObjects = Host<DrawEventArgs>(static h => DisplayPipeline.PreDrawObjects += h, static h => DisplayPipeline.PreDrawObjects -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    public static readonly EventKind PreDrawTransparentObjects = Host<DrawEventArgs>(
        static h => DisplayPipeline.PreDrawTransparentObjects += h, static h => DisplayPipeline.PreDrawTransparentObjects -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    public static readonly EventKind PreDrawObject = Host<DrawObjectEventArgs>(static h => DisplayPipeline.PreDrawObject += h, static h => DisplayPipeline.PreDrawObject -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    public static readonly EventKind PostDrawObject = Host<DrawObjectEventArgs>(static h => DisplayPipeline.PostDrawObject += h, static h => DisplayPipeline.PostDrawObject -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    public static readonly EventKind PostDrawObjects = Host<DrawEventArgs>(static h => DisplayPipeline.PostDrawObjects += h, static h => DisplayPipeline.PostDrawObjects -= h, static a => DocumentHandles.Serial(a.RhinoDoc));

    // --- [PANELS]
    public static readonly EventKind PanelsShow = Host<ShowPanelEventArgs>(static h => Panels.Show += h, static h => Panels.Show -= h, static a => Answers.Present(a.DocumentSerialNumber));

    public static readonly EventKind PanelsClosed = Host<PanelEventArgs>(static h => Panels.Closed += h, static h => Panels.Closed -= h, static a => Answers.Present(a.DocumentSerialNumber));

    // --- [COMMANDS]
    public static readonly EventKind BeginCommand = Host<CommandEventArgs>(static h => Command.BeginCommand += h, static h => Command.BeginCommand -= h, static a => Answers.Present(a.DocumentRuntimeSerialNumber));

    public static readonly EventKind EndCommand = Host<CommandEventArgs>(static h => Command.EndCommand += h, static h => Command.EndCommand -= h, static a => Answers.Present(a.DocumentRuntimeSerialNumber));

    public static readonly EventKind UndoRedo = Host<UndoRedoEventArgs>(static h => Command.UndoRedo += h, static h => Command.UndoRedo -= h, static a => Answers.Present(a.DocumentSerialNumber));

    public static readonly EventKind CommandPromptChanged = Host<CommandPromptChangedEventArgs>(static h => RhinoApp.CommandPromptChanged += h, static h => RhinoApp.CommandPromptChanged -= h, static _ => None);

    public static readonly EventKind EscapeKeyPressed = Host(static h => RhinoApp.EscapeKeyPressed += h, static h => RhinoApp.EscapeKeyPressed -= h);

    // --- [APPLICATION]
    public static readonly EventKind LicenseStateChanged = Host<LicenseStateChangedEventArgs>(static h => RhinoApp.LicenseStateChanged += h, static h => RhinoApp.LicenseStateChanged -= h, static _ => None);

    public static readonly EventKind ThemeChanged = Host(static h => ThemeSettings.ThemeChanged += h, static h => ThemeSettings.ThemeChanged -= h);

    public static readonly EventKind OnExceptionReport = new(
        static (deliver, reject) => Events.Attach<HostUtils.ExceptionReportDelegate>(
            static h => HostUtils.OnExceptionReport += h, static h => HostUtils.OnExceptionReport -= h, (_, _) => _ = Answers.Answer(deliver(None), reject, unit)));

    public static readonly EventKind OnSendLogMessageToCloud = new(
        static (deliver, reject) => Events.Attach<HostUtils.SendLogMessageToCloudDelegate>(
            static h => HostUtils.OnSendLogMessageToCloud += h, static h => HostUtils.OnSendLogMessageToCloud -= h, (_, _, _, _) => _ = Answers.Answer(deliver(None), reject, unit)));

    // --- [RENDER]
    public static readonly EventKind ContentAdded = Host<RenderContentEventArgs>(static h => RenderContent.ContentAdded += h, static h => RenderContent.ContentAdded -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentRenamed = Host<RenderContentEventArgs>(static h => RenderContent.ContentRenamed += h, static h => RenderContent.ContentRenamed -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentDeleting = Host<RenderContentEventArgs>(static h => RenderContent.ContentDeleting += h, static h => RenderContent.ContentDeleting -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentDeleted = Host<RenderContentEventArgs>(static h => RenderContent.ContentDeleted += h, static h => RenderContent.ContentDeleted -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentReplacing = Host<RenderContentEventArgs>(static h => RenderContent.ContentReplacing += h, static h => RenderContent.ContentReplacing -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentReplaced = Host<RenderContentEventArgs>(static h => RenderContent.ContentReplaced += h, static h => RenderContent.ContentReplaced -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentUpdatePreview = Host<RenderContentEventArgs>(
        static h => RenderContent.ContentUpdatePreview += h, static h => RenderContent.ContentUpdatePreview -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind CurrentEnvironmentChanged = Host<RenderContentEventArgs>(
        static h => RenderContent.CurrentEnvironmentChanged += h, static h => RenderContent.CurrentEnvironmentChanged -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentChanged = Host<RenderContentChangedEventArgs>(static h => RenderContent.ContentChanged += h, static h => RenderContent.ContentChanged -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind ContentFieldChanged = Host<RenderContentFieldChangedEventArgs>(
        static h => RenderContent.ContentFieldChanged += h, static h => RenderContent.ContentFieldChanged -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind PreviewRendered = Host<PreviewRenderedEventArgs>(static h => RenderContent.PreviewRendered += h, static h => RenderContent.PreviewRendered -= h, static _ => None);

    public static readonly EventKind GroundPlaneChanged = Host<RenderPropertyChangedEvent>(static h => GroundPlane.Changed += h, static h => GroundPlane.Changed -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind SkylightChanged = Host<RenderPropertyChangedEvent>(static h => Skylight.Changed += h, static h => Skylight.Changed -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind SunChanged = Host<RenderPropertyChangedEvent>(static h => Sun.Changed += h, static h => Sun.Changed -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind SafeFrameChanged = Host<RenderPropertyChangedEvent>(static h => SafeFrame.Changed += h, static h => SafeFrame.Changed -= h, static a => DocumentHandles.Serial(a.Document));

    public static readonly EventKind RenderChannelsChanged = Host<RenderPropertyChangedEvent>(static h => RenderChannels.Changed += h, static h => RenderChannels.Changed -= h, static a => DocumentHandles.Serial(a.Document));

    // --- [ROWS]
    private static EventKind Host<TArgs>(Action<EventHandler<TArgs>> subscribe, Action<EventHandler<TArgs>> unsubscribe, Func<TArgs, Option<uint>> serial) =>
        new((deliver, reject) => Events.Attach(subscribe, unsubscribe, Answers.Handler<TArgs>(args => IO.lift(() => serial(args)).Bind(deliver), reject)));

    private static EventKind Host(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe) =>
        new((deliver, reject) => Events.Attach(subscribe, unsubscribe, new EventHandler((_, _) => _ = Answers.Answer(deliver(None), reject, unit))));

    private static EventKind Watched<TArgs>(Action<EventWatcher, EventHandler<TArgs>> subscribe, Func<TArgs, Option<uint>> serial) =>
        new((deliver, reject) =>
            from watcher in IO.lift(static () => new EventWatcher(includeHeadlessDocuments: true))
            let handler = Answers.Handler<TArgs>(args => IO.lift(() => serial(args)).Bind(deliver), reject)
            from subscribed in IO.lift(() => subscribe(watcher, handler))
            select (IDisposable)watcher);

    private static Option<uint> Serial(DocumentEventArgs args) =>
        Answers.Present(args.DocumentSerialNumber);

    private static Option<uint> Serial(RhinoObjectEventArgs args) =>
        DocumentHandles.Serial(args.TheObject?.Document);

    private static IO<IDisposable> Transforms(Func<Option<uint>, IO<Unit>> deliver, Action<Error> reject) =>
        from log in IO.lift(static () => Atom((Pending: HashMap<uint, Option<uint>>(), Taken: Option<Option<uint>>.None)))
        from attached in Events.AttachAll(
            Seq(
                Events.Attach(
                    static h => RhinoDoc.BeforeTransformObjects += h,
                    static h => RhinoDoc.BeforeTransformObjects -= h,
                    Answers.Handler<RhinoTransformObjectsEventArgs>(
                        args =>
                            from raised in IO.lift(() => (Id: args.TransformEventId, Subject: (toSeq(args.Objects) + toSeq(args.GripOwners)).Head))
                            let serial = raised.Subject.Bind(static subject => DocumentHandles.Serial(subject.Document))
                            from logged in log.SwapIO(held => held with { Pending = held.Pending.AddOrUpdate(raised.Id, serial) })
                            select unit,
                        reject)),
                Events.Attach(
                    static h => RhinoDoc.AfterTransformObjects += h,
                    static h => RhinoDoc.AfterTransformObjects -= h,
                    Answers.Handler<RhinoAfterTransformObjectsEventArgs>(
                        args =>
                            from id in IO.lift(() => args.TransformEventId)
                            from taken in log.SwapIO(held => (held.Pending.Remove(id), held.Pending.Find(id)))
                            from delivered in taken.Taken.Traverse(deliver).As()
                            select unit,
                        reject))),
            reject)
        select attached;

    private static IO<IDisposable> Projections(Func<Option<uint>, IO<Unit>> deliver, Action<Error> reject) =>
        from last in IO.lift(static () => Atom((Last: Option<(Guid Viewport, uint Counter)>.None, Repeat: false)))
        from attached in Events.Attach(
            static h => DisplayPipeline.ViewportProjectionChanged += h,
            static h => DisplayPipeline.ViewportProjectionChanged -= h,
            Answers.Handler<DrawEventArgs>(
                args =>
                    from raised in IO.lift(() => (Viewport: args.Viewport.Id, Counter: args.Viewport.ChangeCounter))
                    from swapped in last.SwapIO(prior => (Some(raised), prior.Last == Some(raised)))
                    from delivered in unless(swapped.Repeat, IO.lift(() => DocumentHandles.Serial(args.RhinoDoc)).Bind(deliver)).As()
                    select delivered,
                reject))
        select attached;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Events {
    public static IO<IDisposable> Attach<THandler>(Action<THandler> subscribe, Action<THandler> unsubscribe, THandler handler) where THandler : Delegate =>
        IO.lift(() => subscribe(handler)).Map<IDisposable>(_ => new Disposal(() => unsubscribe(handler)));

    public static IO<IDisposable> AttachAll(Seq<IO<IDisposable>> attach, Action<Error> reject) =>
        DisposalOps.AcquireAll(attach).Map<IDisposable>(attached => new Disposal(() => _ = Answers.Answer(DisposalOps.Release(attached), reject, unit)));

    public static IO<IDisposable> OnIdle(EventHandler handler) =>
        Attach(static h => RhinoApp.Idle += h, static h => RhinoApp.Idle -= h, handler);
}
