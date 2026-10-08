using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.Events;

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

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Debouncer<A> {
    private readonly Atom<(Option<(long Since, Seq<A> Changes)> Pending, Option<Seq<A>> Taken)> held = Atom((Pending: Option<(long Since, Seq<A> Changes)>.None, Taken: Option<Seq<A>>.None));
    private readonly TimeProvider clock;
    private readonly TimeSpan quiet;

    public Debouncer(TimeProvider clock, Duration quiet, Func<Seq<A>, IO<Unit>> deliver, CallbackSite site) {
        this.clock = clock;
        this.quiet = quiet.ToTimeSpan();
        Timer = clock.CreateTimer(_ => _ = Callbacks.Answer(Tick(deliver), static () => unit, site), state: null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public ITimer Timer { get; }

    public IO<Unit> Post(A change) =>
        from since in IO.lift(clock.GetTimestamp)
        from posted in held.SwapIO(state => (Some((since, state.Pending.Map(static batch => batch.Changes).IfNone(Seq<A>()).Add(change))), Option<Seq<A>>.None))
        from armed in IO.lift(() => Timer.Change(quiet, Timeout.InfiniteTimeSpan))
        select unit;

    private IO<Unit> Tick(Func<Seq<A>, IO<Unit>> deliver) =>
        from now in IO.lift(clock.GetTimestamp)
        from swapped in held.SwapIO(state => state.Pending.Exists(batch => clock.GetElapsedTime(batch.Since, now) >= quiet)
            ? (Option<(long Since, Seq<A> Changes)>.None, state.Pending.Map(static batch => batch.Changes))
            : state with { Taken = None })
        from delivered in swapped.Taken.Traverse(deliver).As()
        from rearmed in swapped.Pending.Traverse(batch => IO.lift(() => Timer.Change(quiet - clock.GetElapsedTime(batch.Since, now), Timeout.InfiniteTimeSpan))).As()
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EventKind {
    // --- [DOCUMENTS]
    public static readonly DocumentEvent<DocumentOpenEventArgs> BeginOpenDocument = Subscriptions.Watched<DocumentOpenEventArgs>(static (watcher, h) => watcher.BeginOpenDocument += h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentOpenEventArgs> EndOpenDocument = Subscriptions.Watched<DocumentOpenEventArgs>(static (watcher, h) => watcher.EndOpenDocument += h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentOpenEventArgs> EndOpenDocumentInitialViewUpdate =
        Subscriptions.Host<DocumentOpenEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.EndOpenDocumentInitialViewUpdate += h, static h => RhinoDoc.EndOpenDocumentInitialViewUpdate -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentSaveEventArgs> BeginSaveDocument = Subscriptions.Host<DocumentSaveEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.BeginSaveDocument += h, static h => RhinoDoc.BeginSaveDocument -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentSaveEventArgs> EndSaveDocument = Subscriptions.Host<DocumentSaveEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.EndSaveDocument += h, static h => RhinoDoc.EndSaveDocument -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentEventArgs> CloseDocument = Subscriptions.Watched<DocumentEventArgs>(static (watcher, h) => watcher.CloseDocument += h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentEventArgs> NewDocument = Subscriptions.Watched<DocumentEventArgs>(static (watcher, h) => watcher.NewDocument += h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentEventArgs> ActiveDocumentChanged = Subscriptions.Host<DocumentEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.ActiveDocumentChanged += h, static h => RhinoDoc.ActiveDocumentChanged -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DocumentEventArgs> DocumentPropertiesChanged = Subscriptions.Host<DocumentEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<UnitsChangedWithScalingEventArgs> UnitsChangedWithScaling =
        Subscriptions.Host<UnitsChangedWithScalingEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.UnitsChangedWithScaling += h, static h => RhinoDoc.UnitsChangedWithScaling -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<RhinoDoc.UserStringChangedArgs> UserStringChanged =
        Subscriptions.Host<RhinoDoc.UserStringChangedArgs>(typeof(RhinoDoc), static h => RhinoDoc.UserStringChanged += h, static h => RhinoDoc.UserStringChanged -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RhinoDoc.WorksessionFileChangedEventArgs> WorksessionFileChanged =
        Subscriptions.Host<RhinoDoc.WorksessionFileChangedEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.WorksessionFileChanged += h, static h => RhinoDoc.WorksessionFileChanged -= h, static args => Conversions.Serial(args.Document));
    private static readonly HostEvent<uint> LiveNewDocument =
        Subscriptions.Host<DocumentEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.NewDocument += h, static h => RhinoDoc.NewDocument -= h, nameof(RhinoDoc.NewDocument))
            .Choose(static args => Some(args.DocumentSerialNumber));
    private static readonly HostEvent<uint> LiveEndOpenDocument =
        Subscriptions.Host<DocumentOpenEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.EndOpenDocument += h, static h => RhinoDoc.EndOpenDocument -= h, nameof(RhinoDoc.EndOpenDocument))
            .Choose(static args => Callbacks.Found(!args.Merge && !args.Reference, args.DocumentSerialNumber));
    public static readonly HostEvent<uint> DocumentArrived = new(
        typeof(RhinoDoc),
        nameof(DocumentArrived),
        static (deliver, site) =>
            from attached in DisposalOps.AcquireAll(Seq(LiveNewDocument, LiveEndOpenDocument).Map(row => row.Inline(deliver, site.Sink)), DisposalOps.Release)
            from delivered in DisposalOps.OnFailure(
                IO.lift(() => Conversions.Rows(RhinoDoc.OpenDocuments(includeHeadless: false)).Iter(doc => Callbacks.Answer(doc.RuntimeSerialNumber, deliver, static () => unit, site))),
                DisposalOps.Release(attached))
            select DisposalOps.Composite(attached, site));

    // --- [OBJECTS]
    public static readonly DocumentEvent<RhinoObjectEventArgs> AddRhinoObject = Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.AddRhinoObject += h, static args => Conversions.Serial(args.TheObject?.Document));
    public static readonly DocumentEvent<RhinoObjectEventArgs> DeleteRhinoObject = Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.DeleteRhinoObject += h, static args => Conversions.Serial(args.TheObject?.Document));
    public static readonly DocumentEvent<RhinoReplaceObjectEventArgs> ReplaceRhinoObject = Subscriptions.Watched<RhinoReplaceObjectEventArgs>(static (watcher, h) => watcher.ReplaceRhinoObject += h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<RhinoObjectEventArgs> UndeleteRhinoObject = Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.UndeleteRhinoObject += h, static args => Conversions.Serial(args.TheObject?.Document));
    public static readonly DocumentEvent<RhinoObjectEventArgs> PurgeRhinoObject = Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.PurgeRhinoObject += h, static args => Conversions.Serial(args.TheObject?.Document));
    public static readonly DocumentEvent<RhinoModifyObjectAttributesEventArgs> ModifyObjectAttributes =
        Subscriptions.Host<RhinoModifyObjectAttributesEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.ModifyObjectAttributes += h, static h => RhinoDoc.ModifyObjectAttributes -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<RhinoObjectSelectionEventArgs> SelectObjects = Subscriptions.Host<RhinoObjectSelectionEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.SelectObjects += h, static h => RhinoDoc.SelectObjects -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RhinoObjectSelectionEventArgs> DeselectObjects = Subscriptions.Host<RhinoObjectSelectionEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.DeselectObjects += h, static h => RhinoDoc.DeselectObjects -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RhinoDeselectAllObjectsEventArgs> DeselectAllObjects =
        Subscriptions.Host<RhinoDeselectAllObjectsEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.DeselectAllObjects += h, static h => RhinoDoc.DeselectAllObjects -= h, static args => Conversions.Serial(args.Document));
    private static readonly HostEvent<RhinoTransformObjectsEventArgs> BeforeTransformObjects =
        Subscriptions.Host<RhinoTransformObjectsEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.BeforeTransformObjects += h, static h => RhinoDoc.BeforeTransformObjects -= h);
    private static readonly HostEvent<RhinoAfterTransformObjectsEventArgs> AfterTransformObjects =
        Subscriptions.Host<RhinoAfterTransformObjectsEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.AfterTransformObjects += h, static h => RhinoDoc.AfterTransformObjects -= h);
    public static readonly DocumentEvent<TransformedObjects> TransformObjects = new(
        typeof(RhinoDoc),
        nameof(TransformObjects),
        static (deliver, site) =>
            from held in IO.lift(static () => Atom((Pending: HashMap<uint, TransformedObjects>(), Taken: Option<TransformedObjects>.None)))
            from attached in DisposalOps.AcquireAll(
                Seq(
                    BeforeTransformObjects.Inline(
                        args =>
                            from raised in IO.lift(() => (Id: args.TransformEventId, Objects: TransformedObjects.Of(args)))
                            from swapped in held.SwapIO(state => state with { Pending = state.Pending.AddOrUpdate(raised.Id, raised.Objects) })
                            select unit,
                        site.Sink),
                    AfterTransformObjects.Inline(
                        args =>
                            from id in IO.lift(() => args.TransformEventId)
                            from swapped in held.SwapIO(state => (state.Pending.Remove(id), state.Pending.Find(id)))
                            from delivered in swapped.Taken.Traverse(deliver).As()
                            select unit,
                        site.Sink)), DisposalOps.Release)
            select DisposalOps.Composite(attached, site),
        static args => args.Document);

    // --- [TABLES]
    public static readonly DocumentEvent<LayerTableEventArgs> LayerTableEvent = Subscriptions.Host<LayerTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.LayerTableEvent += h, static h => RhinoDoc.LayerTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<MaterialTableEventArgs> MaterialTableEvent = Subscriptions.Host<MaterialTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.MaterialTableEvent += h, static h => RhinoDoc.MaterialTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<GroupTableEventArgs> GroupTableEvent = Subscriptions.Host<GroupTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.GroupTableEvent += h, static h => RhinoDoc.GroupTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<LinetypeTableEventArgs> LinetypeTableEvent = Subscriptions.Host<LinetypeTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.LinetypeTableEvent += h, static h => RhinoDoc.LinetypeTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<LightTableEventArgs> LightTableEvent = Subscriptions.Host<LightTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.LightTableEvent += h, static h => RhinoDoc.LightTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<DimStyleTableEventArgs> DimensionStyleTableEvent =
        Subscriptions.Host<DimStyleTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.DimensionStyleTableEvent += h, static h => RhinoDoc.DimensionStyleTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<InstanceDefinitionTableEventArgs> InstanceDefinitionTableEvent =
        Subscriptions.Host<InstanceDefinitionTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.InstanceDefinitionTableEvent += h, static h => RhinoDoc.InstanceDefinitionTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<SectionStyleTableEventArgs> SectionStyleTableEvent =
        Subscriptions.Host<SectionStyleTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.SectionStyleTableEvent += h, static h => RhinoDoc.SectionStyleTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<MarkupTableEventArgs> MarkupTableEvent = Subscriptions.Host<MarkupTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.MarkupTableEvent += h, static h => RhinoDoc.MarkupTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<MarkupViewChangedEventArgs> MarkupViewChanged = Subscriptions.Host<MarkupViewChangedEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.MarkupViewChanged += h, static h => RhinoDoc.MarkupViewChanged -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<PageViewGroupTableEventArgs> PageViewGroupTableEvent =
        Subscriptions.Host<PageViewGroupTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.PageViewGroupTableEvent += h, static h => RhinoDoc.PageViewGroupTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<HatchPatternTableEventArgs> HatchPatternTableEvent =
        Subscriptions.Host<HatchPatternTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.HatchPatternTableEvent += h, static h => RhinoDoc.HatchPatternTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RhinoDoc.TextureMappingEventArgs> TextureMappingEvent =
        Subscriptions.Host<RhinoDoc.TextureMappingEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.TextureMappingEvent += h, static h => RhinoDoc.TextureMappingEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RhinoDoc.RenderContentTableEventArgs> RenderMaterialsTableEvent =
        Subscriptions.Host<RhinoDoc.RenderContentTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.RenderMaterialsTableEvent += h, static h => RhinoDoc.RenderMaterialsTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RhinoDoc.RenderContentTableEventArgs> RenderEnvironmentTableEvent =
        Subscriptions.Host<RhinoDoc.RenderContentTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.RenderEnvironmentTableEvent += h, static h => RhinoDoc.RenderEnvironmentTableEvent -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RhinoDoc.RenderContentTableEventArgs> RenderTextureTableEvent =
        Subscriptions.Host<RhinoDoc.RenderContentTableEventArgs>(typeof(RhinoDoc), static h => RhinoDoc.RenderTextureTableEvent += h, static h => RhinoDoc.RenderTextureTableEvent -= h, static args => Conversions.Serial(args.Document));

    // --- [CONTENT]
    public static readonly DocumentEvent<RenderContentEventArgs> ContentAdded = Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.ContentAdded += h, static h => RenderContent.ContentAdded -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentEventArgs> ContentRenamed = Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.ContentRenamed += h, static h => RenderContent.ContentRenamed -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentEventArgs> ContentDeleting = Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.ContentDeleting += h, static h => RenderContent.ContentDeleting -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentEventArgs> ContentDeleted = Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.ContentDeleted += h, static h => RenderContent.ContentDeleted -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentEventArgs> ContentReplacing = Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.ContentReplacing += h, static h => RenderContent.ContentReplacing -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentEventArgs> ContentReplaced = Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.ContentReplaced += h, static h => RenderContent.ContentReplaced -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentEventArgs> ContentUpdatePreview =
        Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.ContentUpdatePreview += h, static h => RenderContent.ContentUpdatePreview -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentEventArgs> CurrentEnvironmentChanged =
        Subscriptions.Host<RenderContentEventArgs>(typeof(RenderContent), static h => RenderContent.CurrentEnvironmentChanged += h, static h => RenderContent.CurrentEnvironmentChanged -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentChangedEventArgs> ContentChanged =
        Subscriptions.Host<RenderContentChangedEventArgs>(typeof(RenderContent), static h => RenderContent.ContentChanged += h, static h => RenderContent.ContentChanged -= h, static args => Conversions.Serial(args.Document));
    public static readonly DocumentEvent<RenderContentFieldChangedEventArgs> ContentFieldChanged =
        Subscriptions.Host<RenderContentFieldChangedEventArgs>(typeof(RenderContent), static h => RenderContent.ContentFieldChanged += h, static h => RenderContent.ContentFieldChanged -= h, static args => Conversions.Serial(args.Document));
    public static readonly HostEvent<PreviewRenderedEventArgs> PreviewRendered = Subscriptions.Host<PreviewRenderedEventArgs>(typeof(RenderContent), static h => RenderContent.PreviewRendered += h, static h => RenderContent.PreviewRendered -= h);
    public static readonly DocumentEvent<RenderPropertyChangedEvent> GroundPlaneChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(typeof(GroundPlane), static h => GroundPlane.Changed += h, static h => GroundPlane.Changed -= h, static args => Conversions.Serial(args.Document), nameof(GroundPlane.Changed));
    public static readonly DocumentEvent<RenderPropertyChangedEvent> SkylightChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(typeof(Skylight), static h => Skylight.Changed += h, static h => Skylight.Changed -= h, static args => Conversions.Serial(args.Document), nameof(Skylight.Changed));
    public static readonly DocumentEvent<RenderPropertyChangedEvent> SunChanged = Subscriptions.Host<RenderPropertyChangedEvent>(typeof(Sun), static h => Sun.Changed += h, static h => Sun.Changed -= h, static args => Conversions.Serial(args.Document), nameof(Sun.Changed));
    public static readonly DocumentEvent<RenderPropertyChangedEvent> SafeFrameChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(typeof(SafeFrame), static h => SafeFrame.Changed += h, static h => SafeFrame.Changed -= h, static args => Conversions.Serial(args.Document), nameof(SafeFrame.Changed));
    public static readonly DocumentEvent<RenderPropertyChangedEvent> RenderChannelsChanged =
        Subscriptions.Host<RenderPropertyChangedEvent>(typeof(RenderChannels), static h => RenderChannels.Changed += h, static h => RenderChannels.Changed -= h, static args => Conversions.Serial(args.Document), nameof(RenderChannels.Changed));
    public static readonly HostEvent<AddCustomUISectionsEventArgs> OnAddCustomUISections =
        Subscriptions.Host<AddCustomUISectionsEventArgs>(typeof(AddCustomUISections), static h => AddCustomUISections.OnAddCustomUISections += h, static h => AddCustomUISections.OnAddCustomUISections -= h);
    public static readonly HostEvent<ImageFileEventArgs> ImageFileSaved = Subscriptions.Host<ImageFileEventArgs>(typeof(ImageFile), static h => ImageFile.Saved += h, static h => ImageFile.Saved -= h, nameof(ImageFile.Saved));
    public static readonly HostEvent<ImageFileEventArgs> ImageFileLoaded = Subscriptions.Host<ImageFileEventArgs>(typeof(ImageFile), static h => ImageFile.Loaded += h, static h => ImageFile.Loaded -= h, nameof(ImageFile.Loaded));
    public static readonly HostEvent<ImageFileEventArgs> ImageFileDeleted = Subscriptions.Host<ImageFileEventArgs>(typeof(ImageFile), static h => ImageFile.Deleted += h, static h => ImageFile.Deleted -= h, nameof(ImageFile.Deleted));

    // --- [VIEWS]
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewCreate = Subscriptions.Host<ViewEventArgs>(typeof(RhinoView), static h => RhinoView.Create += h, static h => RhinoView.Create -= h, static args => Conversions.Serial(args.View.Document), nameof(RhinoView.Create));
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewDestroy = Subscriptions.Host<ViewEventArgs>(typeof(RhinoView), static h => RhinoView.Destroy += h, static h => RhinoView.Destroy -= h, static args => Conversions.Serial(args.View.Document), nameof(RhinoView.Destroy));
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewSetActive = Subscriptions.Host<ViewEventArgs>(typeof(RhinoView), static h => RhinoView.SetActive += h, static h => RhinoView.SetActive -= h, static args => Conversions.Serial(args.View.Document), nameof(RhinoView.SetActive));
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewRename = Subscriptions.Host<ViewEventArgs>(typeof(RhinoView), static h => RhinoView.Rename += h, static h => RhinoView.Rename -= h, static args => Conversions.Serial(args.View.Document), nameof(RhinoView.Rename));
    public static readonly DocumentEvent<ViewEventArgs> RhinoViewModified = Subscriptions.Host<ViewEventArgs>(typeof(RhinoView), static h => RhinoView.Modified += h, static h => RhinoView.Modified -= h, static args => Conversions.Serial(args.View.Document), nameof(RhinoView.Modified));
    public static readonly DocumentEvent<ViewEnableDrawingEventArgs> EnableDrawingChanged =
        Subscriptions.Host<ViewEnableDrawingEventArgs>(typeof(RhinoView), static h => RhinoView.EnableDrawingChanged += h, static h => RhinoView.EnableDrawingChanged -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly DocumentEvent<DisplayModeChangedEventArgs> DisplayModeChanged =
        Subscriptions.Host<DisplayModeChangedEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.DisplayModeChanged += h, static h => DisplayPipeline.DisplayModeChanged -= h, static args => Conversions.Serial(args.RhinoDoc));
    private static readonly HostEvent<DrawEventArgs> ProjectionRaised = Subscriptions.Host<DrawEventArgs>(
        typeof(DisplayPipeline), static h => DisplayPipeline.ViewportProjectionChanged += h, static h => DisplayPipeline.ViewportProjectionChanged -= h, nameof(DisplayPipeline.ViewportProjectionChanged));
    public static readonly DocumentEvent<DrawEventArgs> ViewportProjectionChanged = new(
        typeof(DisplayPipeline),
        nameof(DisplayPipeline.ViewportProjectionChanged),
        static (deliver, site) =>
            from counters in IO.lift(static () => Atom((Counters: HashMap<Guid, uint>(), Repeat: false)))
            from attached in ProjectionRaised.Inline(
                args =>
                    from raised in IO.lift(() => (Viewport: args.Viewport.Id, Counter: args.Viewport.ChangeCounter))
                    from swapped in counters.SwapIO(state => (state.Counters.AddOrUpdate(raised.Viewport, raised.Counter), state.Counters.Find(raised.Viewport) == Some(raised.Counter)))
                    from delivered in unless(swapped.Repeat, IO.pure(args).Bind(deliver)).As()
                    select unit,
                site.Sink)
            select attached,
        static args => Conversions.Serial(args.RhinoDoc));

    // --- [DRAW]
    public static readonly DocumentEvent<DrawEventArgs> DrawForeground = Subscriptions.Host<DrawEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.DrawForeground += h, static h => DisplayPipeline.DrawForeground -= h, static args => Conversions.Serial(args.RhinoDoc));
    public static readonly DocumentEvent<DrawEventArgs> DrawOverlay = Subscriptions.Host<DrawEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.DrawOverlay += h, static h => DisplayPipeline.DrawOverlay -= h, static args => Conversions.Serial(args.RhinoDoc));
    public static readonly DocumentEvent<CullObjectEventArgs> ObjectCulling = Subscriptions.Host<CullObjectEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.ObjectCulling += h, static h => DisplayPipeline.ObjectCulling -= h, static args => Conversions.Serial(args.RhinoDoc));
    public static readonly HostEvent<InitFrameBufferEventArgs> InitFrameBuffer = Subscriptions.Host<InitFrameBufferEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.InitFrameBuffer += h, static h => DisplayPipeline.InitFrameBuffer -= h);
    public static readonly DocumentEvent<DrawEventArgs> PreDrawObjects = Subscriptions.Host<DrawEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.PreDrawObjects += h, static h => DisplayPipeline.PreDrawObjects -= h, static args => Conversions.Serial(args.RhinoDoc));
    public static readonly DocumentEvent<DrawEventArgs> PreDrawTransparentObjects =
        Subscriptions.Host<DrawEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.PreDrawTransparentObjects += h, static h => DisplayPipeline.PreDrawTransparentObjects -= h, static args => Conversions.Serial(args.RhinoDoc));
    public static readonly DocumentEvent<DrawObjectEventArgs> PreDrawObject = Subscriptions.Host<DrawObjectEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.PreDrawObject += h, static h => DisplayPipeline.PreDrawObject -= h, static args => Conversions.Serial(args.RhinoDoc));
    public static readonly DocumentEvent<DrawObjectEventArgs> PostDrawObject = Subscriptions.Host<DrawObjectEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.PostDrawObject += h, static h => DisplayPipeline.PostDrawObject -= h, static args => Conversions.Serial(args.RhinoDoc));
    public static readonly DocumentEvent<DrawEventArgs> PostDrawObjects = Subscriptions.Host<DrawEventArgs>(typeof(DisplayPipeline), static h => DisplayPipeline.PostDrawObjects += h, static h => DisplayPipeline.PostDrawObjects -= h, static args => Conversions.Serial(args.RhinoDoc));

    // --- [SHELL]
    public static readonly DocumentEvent<ShowPanelEventArgs> PanelsShow = Subscriptions.Host<ShowPanelEventArgs>(typeof(Panels), static h => Panels.Show += h, static h => Panels.Show -= h, static args => Conversions.Present(args.DocumentSerialNumber), nameof(Panels.Show));
    public static readonly DocumentEvent<PanelEventArgs> PanelsClosed = Subscriptions.Host<PanelEventArgs>(typeof(Panels), static h => Panels.Closed += h, static h => Panels.Closed -= h, static args => Conversions.Present(args.DocumentSerialNumber), nameof(Panels.Closed));
    public static readonly DocumentEvent<CommandEventArgs> BeginCommand = Subscriptions.Host<CommandEventArgs>(typeof(Command), static h => Command.BeginCommand += h, static h => Command.BeginCommand -= h, static args => Conversions.Present(args.DocumentRuntimeSerialNumber));
    public static readonly DocumentEvent<CommandEventArgs> EndCommand = Subscriptions.Host<CommandEventArgs>(typeof(Command), static h => Command.EndCommand += h, static h => Command.EndCommand -= h, static args => Conversions.Present(args.DocumentRuntimeSerialNumber));
    public static readonly DocumentEvent<UndoRedoEventArgs> UndoRedo = Subscriptions.Host<UndoRedoEventArgs>(typeof(Command), static h => Command.UndoRedo += h, static h => Command.UndoRedo -= h, static args => Conversions.Present(args.DocumentSerialNumber));
    public static readonly HostEvent<CommandPromptChangedEventArgs> CommandPromptChanged =
        Subscriptions.Host<CommandPromptChangedEventArgs>(typeof(RhinoApp), static h => RhinoApp.CommandPromptChanged += h, static h => RhinoApp.CommandPromptChanged -= h);
    public static readonly HostEvent<EventArgs> EscapeKeyPressed = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.EscapeKeyPressed += h, static h => RhinoApp.EscapeKeyPressed -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> ThemeChanged = Subscriptions.Host<EventHandler, EventArgs>(typeof(ThemeSettings), static h => ThemeSettings.ThemeChanged += h, static h => ThemeSettings.ThemeChanged -= h, static handler => handler.Invoke);

    // --- [APPLICATION]
    public static readonly HostEvent<LicenseStateChangedEventArgs> LicenseStateChanged =
        Subscriptions.Host<LicenseStateChangedEventArgs>(typeof(RhinoApp), static h => RhinoApp.LicenseStateChanged += h, static h => RhinoApp.LicenseStateChanged -= h);
    public static readonly HostEvent<EventArgs> RhinoAppInitialized = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.Initialized += h, static h => RhinoApp.Initialized -= h, static handler => handler.Invoke, nameof(RhinoApp.Initialized));
    public static readonly HostEvent<EventArgs> RhinoAppClosing = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.Closing += h, static h => RhinoApp.Closing -= h, static handler => handler.Invoke, nameof(RhinoApp.Closing));
    public static readonly HostEvent<EventArgs> AppSettingsChanged = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.AppSettingsChanged += h, static h => RhinoApp.AppSettingsChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RendererChanged = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.RendererChanged += h, static h => RhinoApp.RendererChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkNewDocument = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.RdkNewDocument += h, static h => RhinoApp.RdkNewDocument -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkGlobalSettingsChanged = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.RdkGlobalSettingsChanged += h, static h => RhinoApp.RdkGlobalSettingsChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkUpdateAllPreviews = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.RdkUpdateAllPreviews += h, static h => RhinoApp.RdkUpdateAllPreviews -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkCacheImageChanged = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.RdkCacheImageChanged += h, static h => RhinoApp.RdkCacheImageChanged -= h, static handler => handler.Invoke);
    public static readonly HostEvent<EventArgs> RdkPlugInUnloading = Subscriptions.Host<EventHandler, EventArgs>(typeof(RhinoApp), static h => RhinoApp.RdkPlugInUnloading += h, static h => RhinoApp.RdkPlugInUnloading -= h, static handler => handler.Invoke);
    public static readonly HostEvent<(string Source, Exception Exception)> OnExceptionReport = Subscriptions.Host<HostUtils.ExceptionReportDelegate, (string Source, Exception Exception)>(
        typeof(HostUtils), static h => HostUtils.OnExceptionReport += h, static h => HostUtils.OnExceptionReport -= h, static handler => (source, exception) => handler(sender: null, (source, exception)));
    public static readonly HostEvent<(HostUtils.LogMessageType MessageType, string Class, string Description, string Message)> OnSendLogMessageToCloud =
        Subscriptions.Host<HostUtils.SendLogMessageToCloudDelegate, (HostUtils.LogMessageType MessageType, string Class, string Description, string Message)>(
            typeof(HostUtils), static h => HostUtils.OnSendLogMessageToCloud += h, static h => HostUtils.OnSendLogMessageToCloud -= h,
            static handler => (messageType, area, description, message) => handler(sender: null, (messageType, area, description, message)));

    // --- [PLUGIN]
    public static HostEvent<PersistentSettingsSavedEventArgs> SettingsSaved(PlugIn plugIn) =>
        Subscriptions.Host<PersistentSettingsSavedEventArgs>(typeof(PlugIn), h => plugIn.SettingsSaved += h, h => plugIn.SettingsSaved -= h);

    // --- [FILES]
    public static HostEvent<Seq<Either<ErrorEventArgs, FileSystemEventArgs>>> FileChanges(DirectoryInfo directory, Seq<string> filters, bool subdirectories, Duration quiet, TimeProvider clock) =>
        new(
            typeof(FileSystemWatcher),
            nameof(FileChanges),
            (deliver, site) =>
                from watcher in IO.lift(() => new FileSystemWatcher(directory.FullName) { NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size, IncludeSubdirectories = subdirectories })
                    .Catch(static error => error.HasException<ArgumentException>() || error.HasException<FileNotFoundException>(), static _ => IO.fail<FileSystemWatcher>(new Missing(nameof(FileSystemWatcher))))
                from debouncer in DisposalOps.OnFailure(IO.lift(() => new Debouncer<Either<ErrorEventArgs, FileSystemEventArgs>>(clock, quiet, deliver, site)), IO.lift(watcher.Dispose))
                let changed = Callbacks.Handler<FileSystemEventArgs>(args => debouncer.Post(Right(args)), site)
                from held in DisposalOps.AcquireAll(
                    Seq(
                        IO.pure<IDisposable>(debouncer.Timer),
                        IO.pure<IDisposable>(watcher),
                        Subscriptions.Attach<FileSystemEventHandler>(h => watcher.Changed += h, h => watcher.Changed -= h, changed.Invoke),
                        Subscriptions.Attach<FileSystemEventHandler>(h => watcher.Created += h, h => watcher.Created -= h, changed.Invoke),
                        Subscriptions.Attach<FileSystemEventHandler>(h => watcher.Deleted += h, h => watcher.Deleted -= h, changed.Invoke),
                        Subscriptions.Attach<RenamedEventHandler>(h => watcher.Renamed += h, h => watcher.Renamed -= h, changed.Invoke),
                        Subscriptions.Attach<ErrorEventHandler>(h => watcher.Error += h, h => watcher.Error -= h, Callbacks.Handler<ErrorEventArgs>(args => debouncer.Post(Left(args)), site).Invoke)), DisposalOps.Release)
                from enabled in DisposalOps.OnFailure(
                    IO.lift(() => {
                        _ = filters.Iter(watcher.Filters.Add);
                        watcher.EnableRaisingEvents = true;
                    }),
                    DisposalOps.Release(held))
                select DisposalOps.Composite(held, site));
}
