# [RASM_RHINO_EVENTS]

`DocumentStream` owns observation from raw host and filesystem callbacks through detached facts, nonblocking delivery, bounded loss evidence, and retryable symmetric detachment. `Observation` carries source-specific admission, `EventFamily` carries host wiring as data, and `Watch` retains every delivery and release outcome under one identity.

## [01]-[INDEX]

- [02]-[FAMILY]: `EventFamily` binds host callbacks, cadence, and projection as data.
- [03]-[PAYLOAD_PROJECTION]: `EventPayload` and `DocEvent` carry detached callback evidence.
- [04]-[DELIVERY_POLICY]: `Delivery` and `ReceiptPolicy` close bounded delivery and loss evidence.
- [05]-[STREAM_OWNER]: `DocumentStream` and `Watch` own admission, attachment, delivery, and release.

## [02]-[FAMILY]

- Owner: `EventFamily` binds one symbolic host event key to its band, cadence, attach/detach pair, and callback-scope projection.
- Entry: `EventFamily.In` derives band membership from generated `Items`, while `Bind` retains the exact attached delegate for release.
- Law: draw facts retain phase and viewport evidence without retaining `DisplayPipeline`, and per-object phases add the drawn or culled `RhinoObject` identity; transform completion stays unkeyed when the verified host surface supplies no durable correlation identity.
- Law: table projections detach transition, index, and prior/current component evidence; later live resolution re-enters through document identity.
- Law: callback projection faults and sink faults remain disjoint receipts; delivery failure never reclassifies as callback failure.
- Exemption: projection deduplication uses a bounded concurrent kernel because callbacks arrive across host threads.
- Growth: a host callback lands as one symbolic `EventFamily` row whose projection expires every callback-owned handle before delivery.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Rasm.Domain;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;

namespace Rasm.Rhino.Document;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class EventBand {
    public static readonly EventBand Lifecycle = new(key: nameof(Lifecycle));
    public static readonly EventBand Structure = new(key: nameof(Structure));
    public static readonly EventBand Selection = new(key: nameof(Selection));
    public static readonly EventBand Tables = new(key: nameof(Tables));
    public static readonly EventBand Screen = new(key: nameof(Screen));
    public static readonly EventBand Draw = new(key: nameof(Draw));
    public static readonly EventBand Panels = new(key: nameof(Panels));
}

[SmartEnum]
public sealed partial class Cadence {
    public static readonly Cadence Changed = new(static _ => true);
    public static readonly Cadence PerFrame = new(static delivery => delivery is Delivery.Paced paced && paced.Lane.Dropping);

    [UseDelegateFromConstructor]
    public partial bool Admits(Delivery delivery);
}

[SmartEnum<string>]
public sealed partial class EventFamily {
    public static readonly EventFamily BeginOpen = new(key: nameof(BeginOpen), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentOpenEventArgs>(
        subscribe: h => RhinoDoc.BeginOpenDocument += h,
        unsubscribe: h => RhinoDoc.BeginOpenDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Opened.Of(a))));
    public static readonly EventFamily EndOpen = new(key: nameof(EndOpen), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentOpenEventArgs>(
        subscribe: h => RhinoDoc.EndOpenDocument += h,
        unsubscribe: h => RhinoDoc.EndOpenDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Opened.Of(a))));
    public static readonly EventFamily ViewSettled = new(key: nameof(ViewSettled), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentOpenEventArgs>(
        subscribe: h => RhinoDoc.EndOpenDocumentInitialViewUpdate += h,
        unsubscribe: h => RhinoDoc.EndOpenDocumentInitialViewUpdate -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Opened.Of(a))));
    public static readonly EventFamily BeginSave = new(key: nameof(BeginSave), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentSaveEventArgs>(
        subscribe: h => RhinoDoc.BeginSaveDocument += h,
        unsubscribe: h => RhinoDoc.BeginSaveDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Saved.Of(a))));
    public static readonly EventFamily EndSave = new(key: nameof(EndSave), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentSaveEventArgs>(
        subscribe: h => RhinoDoc.EndSaveDocument += h,
        unsubscribe: h => RhinoDoc.EndSaveDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Saved.Of(a))));
    public static readonly EventFamily Closed = new(key: nameof(Closed), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: Signal(
        subscribe: h => RhinoDoc.CloseDocument += h, unsubscribe: h => RhinoDoc.CloseDocument -= h));
    public static readonly EventFamily Created = new(key: nameof(Created), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: Signal(
        subscribe: h => RhinoDoc.NewDocument += h, unsubscribe: h => RhinoDoc.NewDocument -= h));
    public static readonly EventFamily ActiveChanged = new(key: nameof(ActiveChanged), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentEventArgs>(
        subscribe: h => RhinoDoc.ActiveDocumentChanged += h,
        unsubscribe: h => RhinoDoc.ActiveDocumentChanged -= h,
        project: static (_, a, scope) => GateActive(serial: a.DocumentSerialNumber, scope: scope)));
    public static readonly EventFamily PropertiesChanged = new(key: nameof(PropertiesChanged), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: Signal(
        subscribe: h => RhinoDoc.DocumentPropertiesChanged += h, unsubscribe: h => RhinoDoc.DocumentPropertiesChanged -= h));
    public static readonly EventFamily UnitsChanged = new(key: nameof(UnitsChanged), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<UnitsChangedWithScalingEventArgs>(
        subscribe: h => RhinoDoc.UnitsChangedWithScaling += h,
        unsubscribe: h => RhinoDoc.UnitsChangedWithScaling -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: new EventPayload.UnitsScaled(Scale: a.Scale))));
    public static readonly EventFamily UserStringChanged = new(key: nameof(UserStringChanged), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<RhinoDoc.UserStringChangedArgs>(
        subscribe: h => RhinoDoc.UserStringChanged += h,
        unsubscribe: h => RhinoDoc.UserStringChanged -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.UserString(Key: a.Key))));
    public static readonly EventFamily WorksessionFile = new(key: nameof(WorksessionFile), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: OnFallible<RhinoDoc.WorksessionFileChangedEventArgs>(
        subscribe: h => RhinoDoc.WorksessionFileChanged += h,
        unsubscribe: h => RhinoDoc.WorksessionFileChanged -= h,
        project: static (_, a, scope) => WorksessionChange.Of(a.ChangeKind).Map(change => Gate(
            document: a.Document,
            scope: scope,
            payload: new EventPayload.Worksession(ModelSerial: a.WorksessionModelRuntimeSerialNumber, File: a.FilePath, Change: change)))));

    public static readonly EventFamily ObjectAdded = new(key: nameof(ObjectAdded), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.AddRhinoObject += h, unsubscribe: h => RhinoDoc.AddRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily ObjectDeleted = new(key: nameof(ObjectDeleted), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.DeleteRhinoObject += h, unsubscribe: h => RhinoDoc.DeleteRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily ObjectReplaced = new(key: nameof(ObjectReplaced), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoReplaceObjectEventArgs>(
        subscribe: h => RhinoDoc.ReplaceRhinoObject += h,
        unsubscribe: h => RhinoDoc.ReplaceRhinoObject -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.Replaced(
            Old: a.ObjectId, New: Optional(a.NewRhinoObject).Map(static o => o.Id).Filter(static id => id != Guid.Empty)))));
    public static readonly EventFamily ObjectUndeleted = new(key: nameof(ObjectUndeleted), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.UndeleteRhinoObject += h, unsubscribe: h => RhinoDoc.UndeleteRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily ObjectPurged = new(key: nameof(ObjectPurged), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.PurgeRhinoObject += h, unsubscribe: h => RhinoDoc.PurgeRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily AttributesAmended = new(key: nameof(AttributesAmended), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoModifyObjectAttributesEventArgs>(
        subscribe: h => RhinoDoc.ModifyObjectAttributes += h,
        unsubscribe: h => RhinoDoc.ModifyObjectAttributes -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.Attributes(
            Object: Optional(a.RhinoObject).Map(static o => o.Id).Filter(static id => id != Guid.Empty)))));
    public static readonly EventFamily BeforeTransform = new(key: nameof(BeforeTransform), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoTransformObjectsEventArgs>(
        subscribe: h => RhinoDoc.BeforeTransformObjects += h,
        unsubscribe: h => RhinoDoc.BeforeTransformObjects -= h,
        project: TransformFact));
    public static readonly EventFamily AfterTransform = new(key: nameof(AfterTransform), band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoAfterTransformObjectsEventArgs>(
        subscribe: h => RhinoDoc.AfterTransformObjects += h,
        unsubscribe: h => RhinoDoc.AfterTransformObjects -= h,
        project: static (_, _, scope) => Unkeyed(scope: scope, payload: new EventPayload.TransformEnded())));

    public static readonly EventFamily SelectionAdded = new(key: nameof(SelectionAdded), band: EventBand.Selection, cadence: Cadence.Changed, bind: On<RhinoObjectSelectionEventArgs>(
        subscribe: h => RhinoDoc.SelectObjects += h, unsubscribe: h => RhinoDoc.SelectObjects -= h, project: SelectionFact));
    public static readonly EventFamily SelectionRemoved = new(key: nameof(SelectionRemoved), band: EventBand.Selection, cadence: Cadence.Changed, bind: On<RhinoObjectSelectionEventArgs>(
        subscribe: h => RhinoDoc.DeselectObjects += h, unsubscribe: h => RhinoDoc.DeselectObjects -= h, project: SelectionFact));
    public static readonly EventFamily SelectionCleared = new(key: nameof(SelectionCleared), band: EventBand.Selection, cadence: Cadence.Changed, bind: On<RhinoDeselectAllObjectsEventArgs>(
        subscribe: h => RhinoDoc.DeselectAllObjects += h,
        unsubscribe: h => RhinoDoc.DeselectAllObjects -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.Selection(Ids: Seq<Guid>(), Count: a.ObjectCount))));

    public static readonly EventFamily LayerTable = new(key: nameof(LayerTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<LayerTableEventArgs>(
        subscribe: h => RhinoDoc.LayerTableEvent += h, unsubscribe: h => RhinoDoc.LayerTableEvent -= h, kind: TableKind.Layers,
        document: static a => a.Document, index: static a => a.LayerIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily MaterialTable = new(key: nameof(MaterialTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<MaterialTableEventArgs>(
        subscribe: h => RhinoDoc.MaterialTableEvent += h, unsubscribe: h => RhinoDoc.MaterialTableEvent -= h, kind: TableKind.Materials,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldSettings), current: static a => ComponentState.Of(a.Document.Materials[a.Index])));
    public static readonly EventFamily GroupTable = new(key: nameof(GroupTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<GroupTableEventArgs>(
        subscribe: h => RhinoDoc.GroupTableEvent += h, unsubscribe: h => RhinoDoc.GroupTableEvent -= h, kind: TableKind.Groups,
        document: static a => a.Document, index: static a => a.GroupIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily LinetypeTable = new(key: nameof(LinetypeTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<LinetypeTableEventArgs>(
        subscribe: h => RhinoDoc.LinetypeTableEvent += h, unsubscribe: h => RhinoDoc.LinetypeTableEvent -= h, kind: TableKind.Linetypes,
        document: static a => a.Document, index: static a => a.LinetypeIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily LightTable = new(key: nameof(LightTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<LightTableEventArgs>(
        subscribe: h => RhinoDoc.LightTableEvent += h, unsubscribe: h => RhinoDoc.LightTableEvent -= h, kind: TableKind.Lights,
        document: static a => a.Document, index: static a => a.LightIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily DimensionStyleTable = new(key: nameof(DimensionStyleTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<DimStyleTableEventArgs>(
        subscribe: h => RhinoDoc.DimensionStyleTableEvent += h, unsubscribe: h => RhinoDoc.DimensionStyleTableEvent -= h, kind: TableKind.DimStyles,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily InstanceDefinitionTable = new(key: nameof(InstanceDefinitionTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<InstanceDefinitionTableEventArgs>(
        subscribe: h => RhinoDoc.InstanceDefinitionTableEvent += h, unsubscribe: h => RhinoDoc.InstanceDefinitionTableEvent -= h, kind: TableKind.InstanceDefinitions,
        document: static a => a.Document, index: static a => a.InstanceDefinitionIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily SectionStyleTable = new(key: nameof(SectionStyleTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<SectionStyleTableEventArgs>(
        subscribe: h => RhinoDoc.SectionStyleTableEvent += h, unsubscribe: h => RhinoDoc.SectionStyleTableEvent -= h, kind: TableKind.SectionStyles,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily MarkupTable = new(key: nameof(MarkupTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<MarkupTableEventArgs>(
        subscribe: h => RhinoDoc.MarkupTableEvent += h, unsubscribe: h => RhinoDoc.MarkupTableEvent -= h, kind: TableKind.Markups,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily PageViewGroupTable = new(key: nameof(PageViewGroupTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<PageViewGroupTableEventArgs>(
        subscribe: h => RhinoDoc.PageViewGroupTableEvent += h, unsubscribe: h => RhinoDoc.PageViewGroupTableEvent -= h, kind: TableKind.PageViewGroups,
        document: static a => a.Document, index: static a => a.PageViewGroupIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily HatchPatternTable = new(key: nameof(HatchPatternTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<HatchPatternTableEventArgs>(
        subscribe: h => RhinoDoc.HatchPatternTableEvent += h, unsubscribe: h => RhinoDoc.HatchPatternTableEvent -= h, kind: TableKind.HatchPatterns,
        document: static a => a.Document, index: static a => a.HatchPatternIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => ComponentState.Of(a.OldState), current: static a => ComponentState.Of(a.NewState)));
    public static readonly EventFamily RenderMaterialTable = new(key: nameof(RenderMaterialTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Render(
        subscribe: h => RhinoDoc.RenderMaterialsTableEvent += h, unsubscribe: h => RhinoDoc.RenderMaterialsTableEvent -= h, kind: TableKind.RenderMaterials));
    public static readonly EventFamily RenderEnvironmentTable = new(key: nameof(RenderEnvironmentTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Render(
        subscribe: h => RhinoDoc.RenderEnvironmentTableEvent += h, unsubscribe: h => RhinoDoc.RenderEnvironmentTableEvent -= h, kind: TableKind.RenderEnvironments));
    public static readonly EventFamily RenderTextureTable = new(key: nameof(RenderTextureTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: Render(
        subscribe: h => RhinoDoc.RenderTextureTableEvent += h, unsubscribe: h => RhinoDoc.RenderTextureTableEvent -= h, kind: TableKind.RenderTextures));
    public static readonly EventFamily TextureMappingTable = new(key: nameof(TextureMappingTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: OnFallible<RhinoDoc.TextureMappingEventArgs>(
        subscribe: h => RhinoDoc.TextureMappingEvent += h,
        unsubscribe: h => RhinoDoc.TextureMappingEvent -= h,
        project: static (_, a, scope) => ComponentTransition.Of(a.EventType).Map(transition => Gate(
            document: a.Document,
            scope: scope,
            payload: new EventPayload.TextureMapping(
                Transition: transition,
                Current: transition.CarriesCurrent ? Optional(a.NewMapping).Map(static mapping => mapping.Id) : Option<Guid>.None))));

    public static readonly EventFamily ViewModified = new(key: nameof(ViewModified), band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Modified += h, unsubscribe: h => RhinoView.Modified -= h));
    public static readonly EventFamily ViewCreated = new(key: nameof(ViewCreated), band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Create += h, unsubscribe: h => RhinoView.Create -= h));
    public static readonly EventFamily ViewDestroyed = new(key: nameof(ViewDestroyed), band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Destroy += h, unsubscribe: h => RhinoView.Destroy -= h));
    public static readonly EventFamily ViewActivated = new(key: nameof(ViewActivated), band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.SetActive += h, unsubscribe: h => RhinoView.SetActive -= h));
    public static readonly EventFamily ViewRenamed = new(key: nameof(ViewRenamed), band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Rename += h, unsubscribe: h => RhinoView.Rename -= h));
    public static readonly EventFamily ProjectionChanged = new(key: nameof(ProjectionChanged), band: EventBand.Screen, cadence: Cadence.Changed, bind: ProjectionFact(
        subscribe: h => DisplayPipeline.ViewportProjectionChanged += h, unsubscribe: h => DisplayPipeline.ViewportProjectionChanged -= h));
    public static readonly EventFamily DisplayModeChanged = new(key: nameof(DisplayModeChanged), band: EventBand.Screen, cadence: Cadence.Changed, bind: On<DisplayModeChangedEventArgs>(
        subscribe: h => DisplayPipeline.DisplayModeChanged += h,
        unsubscribe: h => DisplayPipeline.DisplayModeChanged -= h,
        project: static (_, a, scope) => Optional(a.Viewport).Bind(viewport => Gate(document: a.RhinoDoc, scope: scope, payload: new EventPayload.DisplayMode(
            ViewportId: viewport.Id, Old: a.OldDisplayModeId, Next: a.ChangedDisplayModeId)))));

    public static readonly EventFamily DrawForeground = new(key: nameof(DrawForeground), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.DrawForeground += h, unsubscribe: h => DisplayPipeline.DrawForeground -= h));
    public static readonly EventFamily DrawOverlay = new(key: nameof(DrawOverlay), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.DrawOverlay += h, unsubscribe: h => DisplayPipeline.DrawOverlay -= h));
    public static readonly EventFamily ObjectCulling = new(key: nameof(ObjectCulling), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact<CullObjectEventArgs>(
        subscribe: h => DisplayPipeline.ObjectCulling += h, unsubscribe: h => DisplayPipeline.ObjectCulling -= h, subject: static a => DrawSubject(a.RhinoObject)));
    public static readonly EventFamily InitFrameBuffer = new(key: nameof(InitFrameBuffer), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.InitFrameBuffer += h, unsubscribe: h => DisplayPipeline.InitFrameBuffer -= h));
    public static readonly EventFamily PreDrawObjects = new(key: nameof(PreDrawObjects), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.PreDrawObjects += h, unsubscribe: h => DisplayPipeline.PreDrawObjects -= h));
    public static readonly EventFamily PreDrawTransparentObjects = new(key: nameof(PreDrawTransparentObjects), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.PreDrawTransparentObjects += h, unsubscribe: h => DisplayPipeline.PreDrawTransparentObjects -= h));
    public static readonly EventFamily PreDrawObject = new(key: nameof(PreDrawObject), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact<DrawObjectEventArgs>(
        subscribe: h => DisplayPipeline.PreDrawObject += h, unsubscribe: h => DisplayPipeline.PreDrawObject -= h, subject: static a => DrawSubject(a.RhinoObject)));
    public static readonly EventFamily PostDrawObject = new(key: nameof(PostDrawObject), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact<DrawObjectEventArgs>(
        subscribe: h => DisplayPipeline.PostDrawObject += h, unsubscribe: h => DisplayPipeline.PostDrawObject -= h, subject: static a => DrawSubject(a.RhinoObject)));
    public static readonly EventFamily PostDrawObjects = new(key: nameof(PostDrawObjects), band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.PostDrawObjects += h, unsubscribe: h => DisplayPipeline.PostDrawObjects -= h));

    public static readonly EventFamily PanelVisibility = new(key: nameof(PanelVisibility), band: EventBand.Panels, cadence: Cadence.Changed, bind: On<global::Rhino.UI.ShowPanelEventArgs>(
        subscribe: h => global::Rhino.UI.Panels.Show += h,
        unsubscribe: h => global::Rhino.UI.Panels.Show -= h,
        project: static (_, a, scope) => Gate(
            serial: a.DocumentSerialNumber,
            scope: scope,
            payload: new EventPayload.Panel(PanelId: a.PanelId, State: a.Show ? PanelState.Shown : PanelState.Hidden))));
    public static readonly EventFamily PanelClosed = new(key: nameof(PanelClosed), band: EventBand.Panels, cadence: Cadence.Changed, bind: On<global::Rhino.UI.PanelEventArgs>(
        subscribe: h => global::Rhino.UI.Panels.Closed += h,
        unsubscribe: h => global::Rhino.UI.Panels.Closed -= h,
        project: static (_, a, scope) => Gate(
            serial: a.DocumentSerialNumber,
            scope: scope,
            payload: new EventPayload.Panel(PanelId: a.PanelId, State: PanelState.Closed))));

    public EventBand Band { get; }
    public Cadence Cadence { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Bind(
        EventScope scope,
        ReceiptJournal journal,
        Func<EventEnvelope, Fin<Unit>> deliver,
        Action<Error> reject);

    public static Fin<Seq<EventFamily>> In(EventBand band) =>
        Optional(band)
            .ToFin(Fail: Op.Of(name: nameof(EventFamily)).InvalidInput())
            .Map(active => toSeq(Items).Filter(family => family.Band == active));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> On<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<object?, TArgs, EventScope, Option<EventEnvelope>> project) where TArgs : EventArgs =>
        OnFallible(
            subscribe: subscribe,
            unsubscribe: unsubscribe,
            project: (sender, args, scope) => Fin.Succ(value: project(arg1: sender, arg2: args, arg3: scope)));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> OnFallible<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<object?, TArgs, EventScope, Fin<Option<EventEnvelope>>> project) where TArgs : EventArgs =>
        (scope, journal, deliver, reject) => {
            EventHandler<TArgs> handler = (sender, args) => {
                Op key = Op.Of(name: nameof(EventFamily));
                Fin<Unit> outcome = key.Catch(() => project(sender, args, scope)).Match(
                    Succ: projected => projected.Match(
                        Some: envelope => key.Catch(() => deliver(arg: envelope)),
                        None: static () => Fin.Succ(value: unit)),
                    Fail: error => {
                        reject(obj: error);
                        return Fin.Fail<Unit>(error: error);
                    });
                ignore(outcome);
            };
            return Subscription.Attach(subscribe: subscribe, unsubscribe: unsubscribe, handler: handler);
        };

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> Signal(
        Action<EventHandler<DocumentEventArgs>> subscribe,
        Action<EventHandler<DocumentEventArgs>> unsubscribe) =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: static (_, a, scope) =>
            Gate(serial: a.DocumentSerialNumber, scope: scope, payload: new EventPayload.Signal()));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> Table<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        TableKind kind,
        Func<TArgs, RhinoDoc> document,
        Func<TArgs, int> index,
        Func<TArgs, Fin<ComponentTransition>> transition,
        Func<TArgs, Option<ComponentState>> previous,
        Func<TArgs, Option<ComponentState>> current) where TArgs : EventArgs =>
        OnFallible(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, args, scope) => transition(arg: args).Map(change => Gate(
                document: document(arg: args),
                scope: scope,
                payload: new EventPayload.Component(
                    Kind: kind,
                    Index: index(arg: args),
                    Transition: change,
                    Previous: change.CarriesPrevious ? previous(arg: args) : Option<ComponentState>.None,
                    Current: change.CarriesCurrent ? current(arg: args) : Option<ComponentState>.None))));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> Render(
        Action<EventHandler<RhinoDoc.RenderContentTableEventArgs>> subscribe,
        Action<EventHandler<RhinoDoc.RenderContentTableEventArgs>> unsubscribe,
        TableKind kind) =>
        OnFallible(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, args, scope) => RenderTransition.Of(args.EventType).Map(change => Gate(
                document: args.Document,
                scope: scope,
                payload: new EventPayload.RenderContent(
                    Kind: kind,
                    Transition: change,
                    Assignment: args is RhinoDoc.RenderMaterialAssignmentChangedEventArgs assignment
                        ? RenderAssignment.Of(assignment)
                        : Option<RenderAssignment>.None))));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> ViewFact(
        Action<EventHandler<ViewEventArgs>> subscribe,
        Action<EventHandler<ViewEventArgs>> unsubscribe) =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: static (_, a, scope) => Optional(a.View).Bind(view =>
            Gate(document: view.Document, scope: scope, payload: new EventPayload.View(
                ViewSerial: view.RuntimeSerialNumber, MainViewportId: view.MainViewport.Id, Page: view is RhinoPageView))));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> DrawFact<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<TArgs, Option<(Guid Id, uint Serial)>>? subject = null) where TArgs : DrawEventArgs =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, a, scope) => Optional(a.Viewport).Bind(viewport =>
            Gate(document: a.RhinoDoc, scope: scope, payload: new EventPayload.Frame(
                ViewportId: viewport.Id,
                ChangeCounter: viewport.ChangeCounter,
                ViewSerial: Optional(viewport.ParentView).Map(static view => view.RuntimeSerialNumber),
                Object: subject is null ? Option<(Guid Id, uint Serial)>.None : subject(arg: a)))));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> ProjectionFact(
        Action<EventHandler<DrawEventArgs>> subscribe,
        Action<EventHandler<DrawEventArgs>> unsubscribe) =>
        (scope, journal, deliver, reject) => {
            ProjectionWindow seen = new(capacity: journal.Policy.CorrelationCapacity);
            return On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, a, watched) =>
                Optional(a.RhinoDoc).Bind(document => Optional(a.Viewport).Bind(viewport => {
                    (Guid Viewport, uint Document) key = (viewport.Id, document.RuntimeSerialNumber);
                    uint counter = viewport.ChangeCounter;
                    (bool Advanced, bool Reset) advance = seen.Advance(key: key, counter: counter);
                    _ = advance.Reset
                        ? journal.Post(new StreamReceipt.ProjectionReset(
                            Watch: journal.Watch, ViewportId: key.Viewport, DocumentSerial: key.Document))
                        : unit;
                    return advance.Advanced
                        ? Gate(document: document, scope: watched, payload: new EventPayload.Projection(ViewportId: viewport.Id, ChangeCounter: counter))
                        : Option<EventEnvelope>.None;
                })))(scope, journal, deliver, reject);
        };

    private static Option<EventEnvelope> ObjectFact(object? sender, RhinoObjectEventArgs args, EventScope scope) =>
        Gate(document: (sender as RhinoDoc) ?? args.TheObject?.Document, scope: scope, payload: new EventPayload.Objects(Ids: Seq(args.ObjectId)));

    private static Option<EventEnvelope> SelectionFact(object? sender, RhinoObjectSelectionEventArgs args, EventScope scope) =>
        Gate(document: args.Document, scope: scope, payload: new EventPayload.Selection(
            Ids: toSeq(args.RhinoObjects).Choose(static item => Optional(item).Map(static value => value.Id)),
            Count: args.RhinoObjectCount));

    private static Option<EventEnvelope> TransformFact(object? sender, RhinoTransformObjectsEventArgs args, EventScope scope) =>
        TransformDocument(args).Bind(key => Gate(key: key, scope: scope, payload: new EventPayload.TransformStarted(
            Motion: args.Transform,
            Copies: args.ObjectsWillBeCopied,
            Objects: ObjectRefs(args.Objects),
            Grips: ObjectRefs(args.Grips),
            GripOwners: ObjectRefs(args.GripOwners))));

    private static Option<DocKey> TransformDocument(RhinoTransformObjectsEventArgs args) =>
        toSeq(args.Objects)
            .Concat(toSeq(args.GripOwners))
            .Concat(toSeq(args.Grips))
            .Choose(static item => Optional(item).Bind(static value => Optional(value.Document)))
            .Choose(static document => DocKey.Of(document: document, key: Op.Of(name: nameof(TransformDocument))).ToOption())
            .HeadOrNone();

    private static Seq<(Guid Id, uint Serial)> ObjectRefs(IEnumerable<RhinoObject?> objects) =>
        toSeq(objects).Choose(static item => Optional(item).Map(static value => (value.Id, value.RuntimeSerialNumber)));

    private static Option<(Guid Id, uint Serial)> DrawSubject(RhinoObject? subject) =>
        Optional(subject).Map(static value => (value.Id, value.RuntimeSerialNumber)).Filter(static value => value.Id != Guid.Empty);

    private sealed class ProjectionWindow(int capacity) {
        private readonly Lock gate = new();
        private readonly Dictionary<(Guid Viewport, uint Document), uint> seen = new();

        internal (bool Advanced, bool Reset) Advance((Guid Viewport, uint Document) key, uint counter) {
            lock (gate) {
                bool reset = seen.Count >= capacity && !seen.ContainsKey(key: key);
                if (reset) {
                    seen.Clear();
                }
                bool advanced = !seen.TryGetValue(key: key, value: out uint prior) || prior != counter;
                if (advanced) {
                    seen[key] = counter;
                }
                return (Advanced: advanced, Reset: reset);
            }
        }
    }

    private static Option<EventEnvelope> Gate(RhinoDoc? document, EventScope scope, EventPayload payload) =>
        document is RhinoDoc active
            ? Gate(serial: active.RuntimeSerialNumber, scope: scope, payload: payload)
            : Option<EventEnvelope>.None;

    private static Option<EventEnvelope> Gate(uint serial, EventScope scope, EventPayload payload) =>
        serial == 0
            ? Option<EventEnvelope>.None
            : Gate(key: DocKey.Create(value: serial), scope: scope, payload: payload);

    private static Option<EventEnvelope> Gate(DocKey key, EventScope scope, EventPayload payload) =>
        scope.Switch(
            (Key: key, Payload: payload),
            document: static (state, watched) => watched.Key == state.Key
                ? Some(new EventEnvelope(Key: Some(state.Key), Payload: state.Payload))
                : Option<EventEnvelope>.None,
            anyDocument: static (state, _) => Some(new EventEnvelope(Key: Some(state.Key), Payload: state.Payload)));

    private static Option<EventEnvelope> GateActive(uint serial, EventScope scope) =>
        serial > 0
            ? Gate(serial: serial, scope: scope, payload: new EventPayload.Active(ActiveDocument: Some(DocKey.Create(value: serial))))
            : scope.Switch(
                document: static _ => Option<EventEnvelope>.None,
                anyDocument: static _ => Some(new EventEnvelope(
                    Key: Option<DocKey>.None,
                    Payload: new EventPayload.Active(ActiveDocument: Option<DocKey>.None))));

    private static Option<EventEnvelope> Unkeyed(EventScope scope, EventPayload payload) =>
        scope.Switch(
            payload,
            document: static (_, _) => Option<EventEnvelope>.None,
            anyDocument: static (fact, _) => Some(new EventEnvelope(Key: Option<DocKey>.None, Payload: fact)));
}
```

## [03]-[PAYLOAD_PROJECTION]

- Owner: `EventPayload` owns detached callback evidence, while `DocEvent` adds source identity and the optional document key.
- Law: every reference-like host member projects inside its callback into stable identity, value, transition, or component evidence.
- Law: an absent active document remains a typed transition; transform completion remains unkeyed because the verified host surface supplies no durable correlation identity.
- Law: name-keyed transition vocabularies admit host enums generically and fail unknown host values on the typed rail.
- Law: `EventPayload.ObjectIds` defaults to no object contribution, and contributing cases override that projection; `DocEvent` delegates without an empty-arm dispatch ladder.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ComponentTransition {
    public static readonly ComponentTransition Added = new(key: nameof(Added), carriesPrevious: false, carriesCurrent: true);
    public static readonly ComponentTransition Deleted = new(key: nameof(Deleted), carriesPrevious: true, carriesCurrent: false);
    public static readonly ComponentTransition Undeleted = new(key: nameof(Undeleted), carriesPrevious: true, carriesCurrent: true);
    public static readonly ComponentTransition Modified = new(key: nameof(Modified), carriesPrevious: true, carriesCurrent: true);
    public static readonly ComponentTransition Sorted = new(key: nameof(Sorted), carriesPrevious: false, carriesCurrent: false);
    public static readonly ComponentTransition Current = new(key: nameof(Current), carriesPrevious: true, carriesCurrent: true);

    public bool CarriesPrevious { get; }
    public bool CarriesCurrent { get; }

    internal static Fin<ComponentTransition> Of<TEvent>(TEvent value) where TEvent : struct, Enum =>
        Named<ComponentTransition, TEvent>(value: value);

    internal static Fin<T> Named<T, TEvent>(TEvent value)
        where T : class, ISmartEnum<string, T, ValidationError>
        where TEvent : struct, Enum {
        Option<string> name = Optional(Enum.GetName(value: value));
        return name
            .ToFin(Fail: Op.Of(name: typeof(T).Name).InvalidResult())
            .Bind(key => T.TryGet(key, out T? row)
                ? Fin.Succ(value: row)
                : Fin.Fail<T>(error: Op.Of(name: typeof(T).Name).InvalidResult(detail: key)));
    }
}

[SmartEnum<string>]
public sealed partial class WorksessionChange {
    public static readonly WorksessionChange Attached = new(key: nameof(Attached));
    public static readonly WorksessionChange Detached = new(key: nameof(Detached));
    public static readonly WorksessionChange BeforeDetach = new(key: nameof(BeforeDetach));

    internal static Fin<WorksessionChange> Of(RhinoDoc.WorksessionFileChangeKind value) =>
        ComponentTransition.Named<WorksessionChange, RhinoDoc.WorksessionFileChangeKind>(value: value);
}

[SmartEnum<string>]
public sealed partial class RenderTransition {
    public static readonly RenderTransition Loaded = new(key: nameof(Loaded));
    public static readonly RenderTransition Clearing = new(key: nameof(Clearing));
    public static readonly RenderTransition Cleared = new(key: nameof(Cleared));
    public static readonly RenderTransition MaterialAssignmentChanged = new(key: nameof(MaterialAssignmentChanged));

    internal static Fin<RenderTransition> Of(RhinoDoc.RenderContentTableEventType value) =>
        ComponentTransition.Named<RenderTransition, RhinoDoc.RenderContentTableEventType>(value: value);
}

[Union]
public abstract partial record RenderTarget {
    private RenderTarget() { }
    public sealed record Layer(Guid Id) : RenderTarget;
    public sealed record Object(Guid Id) : RenderTarget;
}

public readonly record struct ComponentState(Guid Id, Option<string> Name, bool Deleted) {
    internal static Option<ComponentState> Of(ModelComponent? component) => Optional(component).Map(static value => new ComponentState(
        Id: value.Id,
        Name: Optional(value.IsDeleted ? value.DeletedName : value.Name),
        Deleted: value.IsDeleted));

    internal static Option<ComponentState> Of(Light? light) => Optional(light).Map(static value => new ComponentState(
        Id: value.Id,
        Name: Optional(value.Name),
        Deleted: false));

    internal static Option<ComponentState> Of(LightObject? light) => Optional(light).Bind(static value => Optional(value.LightGeometry).Map(geometry => new ComponentState(
        Id: geometry.Id,
        Name: Optional(geometry.Name),
        Deleted: value.IsDeleted)));
}

public readonly record struct RenderAssignment(RenderTarget Target, Guid PreviousMaterial, Guid CurrentMaterial) {
    internal static Option<RenderAssignment> Of(RhinoDoc.RenderMaterialAssignmentChangedEventArgs change) =>
        (change.IsLayer, change.IsObject) switch {
            (true, false) => Some(new RenderAssignment(
                Target: new RenderTarget.Layer(Id: change.LayerId),
                PreviousMaterial: change.OldRenderMaterial,
                CurrentMaterial: change.NewRenderMaterial)),
            (false, true) => Some(new RenderAssignment(
                Target: new RenderTarget.Object(Id: change.ObjectId),
                PreviousMaterial: change.OldRenderMaterial,
                CurrentMaterial: change.NewRenderMaterial)),
            (false, false) or (true, true) => Option<RenderAssignment>.None,
        };
}

[Union]
public abstract partial record EventPayload {
    private EventPayload() { }

    public virtual Seq<Guid> ObjectIds => Seq<Guid>();

    public sealed record Signal : EventPayload;
    public sealed record Opened(Option<string> File, bool Merge, bool Reference) : EventPayload {
        internal static Opened Of(DocumentOpenEventArgs args) =>
            new(File: Optional(args.FileName), Merge: args.Merge, Reference: args.Reference);
    }
    public sealed record Saved(Option<string> File, bool ExportSelected) : EventPayload {
        internal static Saved Of(DocumentSaveEventArgs args) =>
            new(File: Optional(args.FileName), ExportSelected: args.ExportSelected);
    }
    public sealed record Active(Option<DocKey> ActiveDocument) : EventPayload;
    public sealed record UnitsScaled(double Scale) : EventPayload;
    public sealed record UserString(string Key) : EventPayload;
    public sealed record Worksession(uint ModelSerial, string File, WorksessionChange Change) : EventPayload;
    public sealed record Objects(Seq<Guid> Ids) : EventPayload {
        public override Seq<Guid> ObjectIds => Ids;
    }
    public sealed record Replaced(Guid Old, Option<Guid> New) : EventPayload {
        public override Seq<Guid> ObjectIds => New.ToSeq().Cons(value: Old);
    }
    public sealed record Attributes(Option<Guid> Object) : EventPayload {
        public override Seq<Guid> ObjectIds => Object.ToSeq();
    }
    public sealed record TransformStarted(
        Transform Motion,
        bool Copies,
        Seq<(Guid Id, uint Serial)> Objects,
        Seq<(Guid Id, uint Serial)> Grips,
        Seq<(Guid Id, uint Serial)> GripOwners) : EventPayload {
        public override Seq<Guid> ObjectIds => Objects
            .Concat(Grips)
            .Concat(GripOwners)
            .Map(static item => item.Id)
            .Distinct();
    }
    public sealed record TransformEnded : EventPayload;
    public sealed record Selection(Seq<Guid> Ids, int Count) : EventPayload {
        public override Seq<Guid> ObjectIds => Ids;
    }
    public sealed record Component(
        TableKind Kind,
        int Index,
        ComponentTransition Transition,
        Option<ComponentState> Previous,
        Option<ComponentState> Current) : EventPayload;
    public sealed record RenderContent(
        TableKind Kind,
        RenderTransition Transition,
        Option<RenderAssignment> Assignment) : EventPayload;
    public sealed record TextureMapping(
        ComponentTransition Transition,
        Option<Guid> Current) : EventPayload;
    public sealed record View(uint ViewSerial, Guid MainViewportId, bool Page) : EventPayload;
    public sealed record Projection(Guid ViewportId, uint ChangeCounter) : EventPayload;
    public sealed record DisplayMode(Guid ViewportId, Guid Old, Guid Next) : EventPayload;
    public sealed record Frame(Guid ViewportId, uint ChangeCounter, Option<uint> ViewSerial, Option<(Guid Id, uint Serial)> Object) : EventPayload {
        public override Seq<Guid> ObjectIds => Object.Map(static value => value.Id).ToSeq();
    }
    public sealed record Panel(Guid PanelId, PanelState State) : EventPayload;
    public sealed record Files(Seq<FileEdge> Edges, long Overflow) : EventPayload;
}

// --- [MODELS] -----------------------------------------------------------------------------
internal readonly record struct EventEnvelope(Option<DocKey> Key, EventPayload Payload);

[Union]
public abstract partial record EventOrigin {
    private EventOrigin() { }
    public sealed record Host(EventFamily Family) : EventOrigin;
    public sealed record File(string WatchedPath) : EventOrigin;
}

public readonly record struct DocEvent(EventOrigin Origin, Option<DocKey> Key, EventPayload Payload) {
    public Seq<Guid> ObjectIds => Payload.ObjectIds;
}

[SmartEnum<string>]
public sealed partial class PanelState {
    public static readonly PanelState Shown = new(key: nameof(Shown));
    public static readonly PanelState Hidden = new(key: nameof(Hidden));
    public static readonly PanelState Closed = new(key: nameof(Closed));
}

[SmartEnum<int>]
public sealed partial class FileChangeKind {
    public static readonly FileChangeKind Created = new(key: (int)WatcherChangeTypes.Created);
    public static readonly FileChangeKind Deleted = new(key: (int)WatcherChangeTypes.Deleted);
    public static readonly FileChangeKind Changed = new(key: (int)WatcherChangeTypes.Changed);
    public static readonly FileChangeKind Renamed = new(key: (int)WatcherChangeTypes.Renamed);

    internal static Fin<FileChangeKind> Of(WatcherChangeTypes native, Op key) =>
        TryGet((int)native, out var kind) ? Fin.Succ(value: kind) : Fin.Fail<FileChangeKind>(error: key.InvalidInput());
}

public readonly record struct FileEdge(FileChangeKind Kind, string Path, Option<string> PreviousPath);
```

## [04]-[DELIVERY_POLICY]

- Owner: `Delivery` owns direct, idle-deferred, and paced modalities; `StreamLane` resolves paced channel construction from the admitted `ReceiptPolicy`.
- Law: host callbacks never park. A paced lane either accepts immediately or emits loss evidence through the channel callback and write result.
- Law: channel continuations never execute synchronously on a producing host callback.
- Law: bounded lanes close every nonblocking full-buffer mode; `Coalesced` preserves the queued head and latest arrival by evicting the newest buffered predecessor.
- Law: frame cadence admits only bounded dropping lanes; unbounded accumulation is rejected before attachment.
- Law: `StreamLoss` is the paced-loss vocabulary carried unchanged by `StreamReceipt.PacedLoss`; one parameterized `ReceiptPolicy` bounds every queue and correlation set owned by a `Watch`.
- Law: `ReceiptPolicy` owns named operational and maximum rows; generated admission rejects nonpositive values, individual ceiling breaches, and aggregate overcommit.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class StreamLane {
    public static readonly StreamLane Mailbox = new(
        key: nameof(Mailbox),
        dropping: true,
        open: Bounded(capacity: static _ => 1, mode: BoundedChannelFullMode.DropOldest, loss: StreamLoss.Evicted));
    public static readonly StreamLane Shed = new(
        key: nameof(Shed),
        dropping: true,
        open: Bounded(capacity: static policy => policy.LaneCapacity, mode: BoundedChannelFullMode.DropOldest, loss: StreamLoss.Evicted));
    public static readonly StreamLane Coalesced = new(
        key: nameof(Coalesced),
        dropping: true,
        open: Bounded(capacity: static policy => policy.LaneCapacity, mode: BoundedChannelFullMode.DropNewest, loss: StreamLoss.Evicted));
    public static readonly StreamLane Ordered = new(
        key: nameof(Ordered),
        dropping: true,
        open: Bounded(capacity: static policy => policy.LaneCapacity, mode: BoundedChannelFullMode.DropWrite, loss: StreamLoss.Refused));
    public static readonly StreamLane Firehose = new(
        key: nameof(Firehose),
        dropping: false,
        open: static (_, _) => Channel.CreateUnbounded<DocEvent>(
            new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = false }));

    public bool Dropping { get; }

    [UseDelegateFromConstructor]
    internal partial Channel<DocEvent> Open(ReceiptPolicy policy, Action<StreamLoss, DocEvent> lost);

    private static Func<ReceiptPolicy, Action<StreamLoss, DocEvent>, Channel<DocEvent>> Bounded(
        Func<ReceiptPolicy, int> capacity,
        BoundedChannelFullMode mode,
        StreamLoss loss) =>
        (policy, lost) => Channel.CreateBounded<DocEvent>(
            new BoundedChannelOptions(capacity: capacity(arg: policy)) {
                FullMode = mode,
                SingleReader = true,
                AllowSynchronousContinuations = false,
            },
            itemDropped: item => lost(arg1: loss, arg2: item));
}

[SmartEnum<string>]
public sealed partial class StreamLoss {
    public static readonly StreamLoss Evicted = new(key: nameof(Evicted));
    public static readonly StreamLoss Refused = new(key: nameof(Refused));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Delivery {
    private Delivery() { }
    public sealed record Inline(Func<DocEvent, Fin<Unit>> Sink) : Delivery;
    public sealed record Deferred(Func<DocEvent, Fin<Unit>> Sink) : Delivery;
    public sealed record Paced(StreamLane Lane) : Delivery;
}

[Union]
public abstract partial record StreamReceipt {
    private StreamReceipt() { }
    public sealed record PacedLoss(WatchKey Watch, StreamLane Lane, StreamLoss Loss, EventOrigin Origin) : StreamReceipt;
    public sealed record DeferredOverflow(WatchKey Watch, EventOrigin Origin) : StreamReceipt;
    public sealed record Reentrant(WatchKey Watch, EventOrigin Origin) : StreamReceipt;
    public sealed record CallbackFault(WatchKey Watch, EventOrigin Origin, string Detail) : StreamReceipt;
    public sealed record SinkFault(WatchKey Watch, EventOrigin Origin, string Detail) : StreamReceipt;
    public sealed record Cancelled(WatchKey Watch, EventOrigin Origin) : StreamReceipt;
    public sealed record FileOverflow(WatchKey Watch, string WatchedPath) : StreamReceipt;
    public sealed record FileFault(WatchKey Watch, string WatchedPath, string Detail) : StreamReceipt;
    public sealed record ProjectionReset(WatchKey Watch, Guid ViewportId, uint DocumentSerial) : StreamReceipt;
    public sealed record DetachFault(WatchKey Watch, string Detail) : StreamReceipt;
    public sealed record JournalOverflow(WatchKey Watch, long Lost) : StreamReceipt;
}

// --- [STATE] ------------------------------------------------------------------------------
[ValueObject<long>]
public readonly partial struct WatchKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref long value) =>
        validationError = value > 0 ? null : new ValidationError(message: "Watch identity is not positive.");
}

[ComplexValueObject]
public sealed partial class ReceiptPolicy {
    private static readonly (int Lane, int Receipt, int Deferred, int File, int Correlation) OperationalValues = (
        Lane: 256,
        Receipt: 4_096,
        Deferred: 512,
        File: 512,
        Correlation: 2_048);
    private static readonly (int Lane, int Receipt, int Deferred, int File, int Correlation, long Total) CapacityLimits = (
        Lane: 4_096,
        Receipt: 16_384,
        Deferred: 4_096,
        File: 4_096,
        Correlation: 8_192,
        Total: 24_576L);

    public int LaneCapacity { get; }
    public int ReceiptCapacity { get; }
    public int DeferredCapacity { get; }
    public int FileCapacity { get; }
    public int CorrelationCapacity { get; }

    public static ReceiptPolicy Operational { get; } = Create(
        laneCapacity: OperationalValues.Lane,
        receiptCapacity: OperationalValues.Receipt,
        deferredCapacity: OperationalValues.Deferred,
        fileCapacity: OperationalValues.File,
        correlationCapacity: OperationalValues.Correlation);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int laneCapacity,
        ref int receiptCapacity,
        ref int deferredCapacity,
        ref int fileCapacity,
        ref int correlationCapacity) {
        Seq<(int Value, int Maximum)> capacities = Seq(
            (Value: laneCapacity, Maximum: CapacityLimits.Lane),
            (Value: receiptCapacity, Maximum: CapacityLimits.Receipt),
            (Value: deferredCapacity, Maximum: CapacityLimits.Deferred),
            (Value: fileCapacity, Maximum: CapacityLimits.File),
            (Value: correlationCapacity, Maximum: CapacityLimits.Correlation));
        long total = capacities.Fold(0L, static (sum, row) => sum + row.Value);
        validationError = capacities.ForAll(static row => row.Value > 0 && row.Value <= row.Maximum)
                && total <= CapacityLimits.Total
            ? null
            : new ValidationError(message: "Observation capacities exceed their positive per-capacity or aggregate bounds.");
    }

    public static Fin<ReceiptPolicy> Of(
        int laneCapacity,
        int receiptCapacity,
        int deferredCapacity,
        int fileCapacity,
        int correlationCapacity,
        Op key) =>
        key.AcceptValidated<ReceiptPolicy>(
            fault: Validate(
                laneCapacity,
                receiptCapacity,
                deferredCapacity,
                fileCapacity,
                correlationCapacity,
                out ReceiptPolicy? admitted),
            admitted: admitted);
}

internal sealed class ReceiptJournal(WatchKey watch, ReceiptPolicy policy) {
    private readonly Atom<ReceiptState> state = Atom(value: new ReceiptState(Items: Seq<StreamReceipt>(), Lost: 0));

    internal WatchKey Watch { get; } = watch;
    internal ReceiptPolicy Policy { get; } = policy;
    internal Seq<StreamReceipt> Snapshot {
        get {
            ReceiptState snapshot = state.Value;
            return snapshot.Lost is 0
                ? snapshot.Items
                : snapshot.Items.Count < Policy.ReceiptCapacity
                    ? snapshot.Items.Add(value: new StreamReceipt.JournalOverflow(Watch: Watch, Lost: snapshot.Lost))
                    : snapshot.Items.Tail.Add(value: new StreamReceipt.JournalOverflow(Watch: Watch, Lost: snapshot.Lost));
        }
    }

    internal Unit Post(StreamReceipt receipt) =>
        ignore(state.Swap(f: held => held.Items.Count < Policy.ReceiptCapacity
            ? held with { Items = held.Items.Add(value: receipt) }
            : new ReceiptState(Items: held.Items.Tail.Add(value: receipt), Lost: checked(held.Lost + 1))));

    internal SubscriptionRelease Faults(SubscriptionRelease release) {
        _ = release is SubscriptionRelease.Faulted faulted
            ? faulted.Errors.Fold(unit, (state, error) => (Post(new StreamReceipt.DetachFault(Watch: Watch, Detail: error.Message)), state).Item2)
            : unit;
        return release;
    }

    private readonly record struct ReceiptState(Seq<StreamReceipt> Items, long Lost);
}
```

## [05]-[STREAM_OWNER]

- Owner: `Observation` carries each source's complete ingress, and `DocumentStream.Observe` owns admission, attachment, rollback, and watch minting.
- Law: every source, delivery, policy, and source-specific value admits before the first attachment; sequential attachment rolls back the accumulated prefix on failure.
- Law: `Watch.Close` cancels delivery, combines source and idle-pump detachment evidence, receipts each fault, and retains each failed owner for a later close attempt.
- Law: close claims its owners under the lifecycle lock, executes callbacks after release, and publishes retry custody plus one settled result atomically; concurrent callers join that result.
- Law: an empty subscription closes as `Released(0)`; `Open` denotes only unclaimed live custody.
- Law: reentrancy and deferred capacity belong to one watch, so recursive or queued work cannot suppress or exhaust a sibling observation.
- Law: deferred delivery owns one idle hook per watch; closing the watch detaches that hook and receipts every queued fact as cancelled.
- Law: file callbacks fold into one resettable trailing-edge timer and one bounded batch before entering the same delivery spine as host facts.
- Exemption: native attach/detach, timer ownership, `Lock` scopes, and callback `try/finally` blocks are platform-forced lifetime seams.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EventScope {
    private EventScope() { }
    public sealed record Document(DocKey Key) : EventScope;
    public sealed record AnyDocument : EventScope;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Observation {
    private Observation() { }
    public sealed record Host(
        EventScope Scope,
        Seq<EventFamily> Families,
        Delivery Delivery,
        ReceiptPolicy Receipts) : Observation;
    public sealed record File(
        string Path,
        TimeSpan Debounce,
        TimeProvider Clock,
        Delivery Delivery,
        ReceiptPolicy Receipts) : Observation;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed class Watch : IDisposable {
    private readonly Lock gate = new();
    private readonly ReceiptJournal journal;
    private WatchClosure closure;

    internal Watch(Subscription subscription, Emission emission, ReceiptJournal journal) {
        this.journal = journal;
        closure = new WatchClosure.Ready(
            Subscription: subscription,
            Emission: emission,
            Release: new SubscriptionRelease.Open());
        Reader = emission.Reader;
    }

    public WatchKey Key => journal.Watch;
    public Seq<StreamReceipt> Receipts => journal.Snapshot;
    public Option<ChannelReader<DocEvent>> Reader { get; }
    public SubscriptionRelease Release {
        get {
            Task<SubscriptionRelease>? waiting;
            lock (gate) {
                if (closure is WatchClosure.Ready ready) {
                    return ready.Release;
                }
                waiting = ((WatchClosure.Closing)closure).Settled;
            }
            return SubscriptionRelease.Join(waiting);
        }
    }

    public SubscriptionRelease Close() {
        WatchClosure.Ready? claimed = null;
        Task<SubscriptionRelease>? waiting = null;
        TaskCompletionSource<SubscriptionRelease>? flight = null;
        lock (gate) {
            if (closure is WatchClosure.Closing closing) {
                waiting = closing.Settled;
            } else {
                claimed = (WatchClosure.Ready)closure;
                if (claimed.Subscription is null && claimed.Emission is null) {
                    return claimed.Release;
                }
                flight = SubscriptionRelease.BeginClose();
                closure = new WatchClosure.Closing(Settled: flight.Task);
            }
        }
        if (waiting is not null) {
            return SubscriptionRelease.Join(waiting);
        }
        WatchClosure.Ready owner = claimed!;
        owner.Emission?.Cancel();
        SubscriptionRelease source = owner.Subscription?.Close()
            ?? new SubscriptionRelease.Released(Attempted: 0);
        SubscriptionRelease delivery = owner.Emission?.Complete()
            ?? new SubscriptionRelease.Released(Attempted: 0);
        SubscriptionRelease settled = journal.Faults(release: SubscriptionRelease.All(source, delivery));
        lock (gate) {
            closure = new WatchClosure.Ready(
                Subscription: source is SubscriptionRelease.Faulted ? owner.Subscription : null,
                Emission: delivery is SubscriptionRelease.Faulted ? owner.Emission : null,
                Release: settled);
            return SubscriptionRelease.Publish(pending: flight!, release: settled);
        }
    }

    public void Dispose() => ignore(Close());

    private abstract record WatchClosure {
        private WatchClosure() { }

        internal sealed record Ready(
            Subscription? Subscription,
            Emission? Emission,
            SubscriptionRelease Release) : WatchClosure;

        internal sealed record Closing(Task<SubscriptionRelease> Settled) : WatchClosure;
    }
}

internal sealed class Emission {
    private readonly Delivery delivery;
    private readonly ReceiptJournal journal;
    private readonly Reentrancy gate = new();
    private readonly Option<Channel<DocEvent>> channel;
    private readonly Option<IdlePump> idle;
    private int active = 1;

    private Emission(
        Delivery delivery,
        ReceiptJournal journal,
        Option<Channel<DocEvent>> channel,
        Option<IdlePump> idle) {
        this.delivery = delivery;
        this.journal = journal;
        this.channel = channel;
        this.idle = idle;
    }

    internal Option<ChannelReader<DocEvent>> Reader => channel.Map(static value => value.Reader);
    private bool IsActive => Volatile.Read(location: ref active) != 0;

    internal static Fin<Emission> Open(Delivery delivery, ReceiptJournal journal, Op key) =>
        Optional(delivery).ToFin(Fail: key.InvalidInput()).Bind(active => active.Switch(
            (Journal: journal, Op: key),
            inline: static (state, mode) => Optional(mode.Sink).ToFin(Fail: state.Op.InvalidInput())
                .Map(_ => new Emission(
                    delivery: mode,
                    journal: state.Journal,
                    channel: Option<Channel<DocEvent>>.None,
                    idle: Option<IdlePump>.None)),
            deferred: static (state, mode) => Optional(mode.Sink).ToFin(Fail: state.Op.InvalidInput())
                .Bind(_ => IdlePump.Open(journal: state.Journal))
                .Map(pump => new Emission(
                    delivery: mode,
                    journal: state.Journal,
                    channel: Option<Channel<DocEvent>>.None,
                    idle: Some(pump))),
            paced: static (state, mode) => Optional(mode.Lane).ToFin(Fail: state.Op.InvalidInput()).Bind(lane =>
                state.Op.Catch(() => {
                    Channel<DocEvent> opened = lane.Open(
                        policy: state.Journal.Policy,
                        lost: (loss, fact) => ignore(state.Journal.Post(new StreamReceipt.PacedLoss(
                            Watch: state.Journal.Watch,
                            Lane: lane,
                            Loss: loss,
                            Origin: fact.Origin))));
                    return Fin.Succ(value: new Emission(
                        delivery: mode,
                        journal: state.Journal,
                        channel: Some(opened),
                        idle: Option<IdlePump>.None));
                }))));

    internal Fin<Unit> Emit(DocEvent fact) =>
        !IsActive
            ? Fin.Succ(value: journal.Post(new StreamReceipt.Cancelled(Watch: journal.Watch, Origin: fact.Origin)))
            : gate.Active
                ? Fin.Succ(value: journal.Post(new StreamReceipt.Reentrant(Watch: journal.Watch, Origin: fact.Origin)))
                : delivery.Switch(
                    (Owner: this, Fact: fact),
                    inline: static (state, mode) => state.Owner.gate.Guarded(
                        journal: state.Owner.journal, origin: state.Fact.Origin, run: () => mode.Sink(arg: state.Fact)),
                    deferred: static (state, mode) => state.Owner.idle
                        .ToFin(Op.Of(name: nameof(Emission)).InvalidResult())
                        .Bind(pump => pump.Enqueue(
                            origin: state.Fact.Origin,
                            alive: () => state.Owner.IsActive,
                            run: () => state.Owner.gate.Guarded(
                                journal: state.Owner.journal, origin: state.Fact.Origin, run: () => mode.Sink(arg: state.Fact)))),
                    paced: static (state, mode) => state.Owner.channel
                        .ToFin(Op.Of(name: nameof(Emission)).InvalidResult())
                        .Map(opened => opened.Writer.TryWrite(item: state.Fact)
                            ? unit
                            : state.Owner.journal.Post(new StreamReceipt.Cancelled(
                                Watch: state.Owner.journal.Watch, Origin: state.Fact.Origin))));

    internal void Cancel() => Interlocked.Exchange(location1: ref active, value: 0);

    internal SubscriptionRelease Complete() {
        SubscriptionRelease release = idle.Match(
            Some: static pump => pump.Close(),
            None: static () => new SubscriptionRelease.Released(Attempted: 0));
        ignore(channel.Map(static opened => opened.Writer.TryComplete()));
        return release;
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public static class DocumentStream {
    private static long sequence;

    public static Fin<Watch> Observe(Observation request) {
        Op op = Op.Of();
        return Optional(request).ToFin(Fail: op.InvalidInput()).Bind(active => active.Switch(
            op,
            host: static (key, observation) => ObserveHost(request: observation, key: key),
            file: static (key, observation) => ObserveFile(request: observation, key: key)));
    }

    private static Fin<Watch> ObserveHost(Observation.Host request, Op key) =>
        from scope in Optional(request.Scope).ToFin(Fail: key.InvalidInput())
        from delivery in Optional(request.Delivery).ToFin(Fail: key.InvalidInput())
        from families in request.Families
            .TraverseM(family => Optional(family).ToFin(Fail: key.InvalidInput()))
            .As()
            .Map(static named => named.Distinct())
            .Bind(named => named.IsEmpty ? Fin.Fail<Seq<EventFamily>>(error: key.InvalidInput()) : Fin.Succ(value: named))
        from _ in families.TraverseM(family => family.Cadence.Admits(delivery: delivery)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.Unsupported(geometryType: typeof(EventFamily), outputType: typeof(Delivery)))).As()
        from watch in Mount(
            delivery: delivery,
            policy: request.Receipts,
            attach: (emission, journal) => Attach(scope: scope, families: families, emission: emission, journal: journal),
            key: key)
        select watch;

    private static Fin<Watch> Mount(
        Delivery delivery,
        ReceiptPolicy policy,
        Func<Emission, ReceiptJournal, Fin<Subscription>> attach,
        Op key) =>
        from bounds in Optional(policy).ToFin(Fail: key.InvalidInput())
        let journal = new ReceiptJournal(
            watch: WatchKey.Create(value: Interlocked.Increment(location: ref sequence)),
            policy: bounds)
        from emission in Emission.Open(delivery: delivery, journal: journal, key: key)
        from subscription in attach(emission, journal)
            .MapFail(error => {
                emission.Cancel();
                return SubscriptionRelease.AddTo(
                    primary: error,
                    release: journal.Faults(release: emission.Complete()));
            })
        select new Watch(subscription: subscription, emission: emission, journal: journal);

    private static Fin<Subscription> Attach(
        EventScope scope,
        Seq<EventFamily> families,
        Emission emission,
        ReceiptJournal journal) =>
        Subscription.AttachAll(families.Map(family => (Func<Fin<Subscription>>)(() => family.Bind(
            scope: scope,
            journal: journal,
            deliver: envelope => emission.Emit(fact: new DocEvent(
                Origin: new EventOrigin.Host(Family: family), Key: envelope.Key, Payload: envelope.Payload)),
            reject: error => ignore(journal.Post(new StreamReceipt.CallbackFault(
                Watch: journal.Watch, Origin: new EventOrigin.Host(Family: family), Detail: error.Message)))))));

    private static Fin<Watch> ObserveFile(Observation.File request, Op key) =>
        from path in key.AcceptText(value: request.Path)
        from clock in Optional(request.Clock).ToFin(Fail: key.InvalidInput())
        from _ in guard(request.Debounce > TimeSpan.Zero, key.InvalidInput())
        from watch in Mount(
            delivery: request.Delivery,
            policy: request.Receipts,
            attach: (emission, journal) => AttachFile(
                path: path, debounce: request.Debounce, clock: clock, emission: emission, journal: journal, key: key),
            key: key)
        select watch;

    private static Fin<Subscription> AttachFile(
        string path,
        TimeSpan debounce,
        TimeProvider clock,
        Emission emission,
        ReceiptJournal journal,
        Op key) => key.Catch(() => {
            string fullPath = System.IO.Path.GetFullPath(path: path);
            string directory = System.IO.Path.GetDirectoryName(path: fullPath) ?? string.Empty;
            string filter = System.IO.Path.GetFileName(path: fullPath);
            if (directory.Length is 0 || filter.Length is 0 || !System.IO.Directory.Exists(path: directory)) {
                return Fin.Fail<Subscription>(error: key.InvalidInput());
            }
            FileSystemWatcher? watcher = null;
            ITimer? timer = null;
            try {
                EventOrigin origin = new EventOrigin.File(WatchedPath: fullPath);
                FileSystemWatcher createdWatcher = new(path: directory, filter: filter) {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                };
                watcher = createdWatcher;
                Atom<FileBatch> batch = Atom(value: new FileBatch(Edges: Seq<FileEdge>(), Overflow: 0));
                Fin<Unit> Fire() {
                    FileBatch pending = new(Edges: Seq<FileEdge>(), Overflow: 0);
                    _ = batch.Swap(current => (pending = current, new FileBatch(Edges: Seq<FileEdge>(), Overflow: 0)).Item2);
                    _ = pending.Overflow > 0
                        ? journal.Post(new StreamReceipt.FileOverflow(Watch: journal.Watch, WatchedPath: fullPath))
                        : unit;
                    return pending.Edges.IsEmpty
                        ? Fin.Succ(value: unit)
                        : emission.Emit(new DocEvent(
                            Origin: origin,
                            Key: Option<DocKey>.None,
                            Payload: new EventPayload.Files(Edges: pending.Edges, Overflow: pending.Overflow)));
                }
                ITimer createdTimer = clock.CreateTimer(
                    callback: _ => ignore(Fire()),
                    state: null,
                    dueTime: Timeout.InfiniteTimeSpan,
                    period: Timeout.InfiniteTimeSpan);
                timer = createdTimer;
                Fin<Unit> Schedule() => key.Catch(() => createdTimer.Change(dueTime: debounce, period: Timeout.InfiniteTimeSpan)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: key.InvalidResult()));
                Fin<Unit> Capture(FileEdge edge) {
                    _ = batch.Swap(current => current.Edges.Count < journal.Policy.FileCapacity
                        ? current with { Edges = current.Edges.Add(value: edge) }
                        : new FileBatch(Edges: current.Edges.Tail.Add(value: edge), Overflow: checked(current.Overflow + 1)));
                    return Schedule();
                }
                Fin<Unit> CaptureOverflow(Exception failure) {
                    _ = batch.Swap(current => current with { Overflow = checked(current.Overflow + 1) });
                    _ = journal.Post(new StreamReceipt.FileFault(
                        Watch: journal.Watch,
                        WatchedPath: fullPath,
                        Detail: failure.Message.Length is 0 ? failure.GetType().FullName ?? failure.GetType().Name : failure.Message));
                    return Schedule();
                }
                Fin<Unit> Capture(FileSystemEventArgs args) =>
                    from kind in FileChangeKind.Of(native: args.ChangeType, key: key)
                    from _ in Capture(new FileEdge(Kind: kind, Path: args.FullPath, PreviousPath: Option<string>.None))
                    select unit;
                Fin<Unit> Logged(Fin<Unit> outcome) => outcome.MapFail(error => {
                    _ = journal.Post(new StreamReceipt.SinkFault(Watch: journal.Watch, Origin: origin, Detail: error.Message));
                    return error;
                });
                FileSystemEventHandler change = (_, args) => ignore(Logged(outcome: Capture(args)));
                RenamedEventHandler rename = (_, args) => ignore(Logged(outcome: Capture(new FileEdge(
                    Kind: FileChangeKind.Renamed,
                    Path: args.FullPath,
                    PreviousPath: Optional(args.OldFullPath)))));
                ErrorEventHandler failure = (_, args) => ignore(Logged(outcome: CaptureOverflow(failure: args.GetException())));
                Subscription owner = Subscription.Of(detach: () => { createdTimer.Dispose(); createdWatcher.Dispose(); });
                return Subscription.AttachAll(Seq<Func<Fin<Subscription>>>(
                        () => Subscription.Attach(
                            subscribe: h => createdWatcher.Changed += h,
                            unsubscribe: h => createdWatcher.Changed -= h,
                            handler: change),
                        () => Subscription.Attach(
                            subscribe: h => createdWatcher.Created += h,
                            unsubscribe: h => createdWatcher.Created -= h,
                            handler: change),
                        () => Subscription.Attach(
                            subscribe: h => createdWatcher.Deleted += h,
                            unsubscribe: h => createdWatcher.Deleted -= h,
                            handler: change),
                        () => Subscription.Attach(
                            subscribe: h => createdWatcher.Renamed += h,
                            unsubscribe: h => createdWatcher.Renamed -= h,
                            handler: rename),
                        () => Subscription.Attach(
                            subscribe: h => createdWatcher.Error += h,
                            unsubscribe: h => createdWatcher.Error -= h,
                            handler: failure),
                        () => Subscription.Acquire(
                            acquire: () => createdWatcher.EnableRaisingEvents = true,
                            release: () => createdWatcher.EnableRaisingEvents = false)))
                    .Map(owner.Combine)
                    .MapFail(owner.Rollback);
            } catch {
                timer?.Dispose();
                watcher?.Dispose();
                throw;
            }
        });

    private readonly record struct FileBatch(Seq<FileEdge> Edges, long Overflow);
}

// --- [COMPOSITION] ------------------------------------------------------------------------
public sealed class Subscription : IDisposable {
    private readonly Lock gate = new();
    private SubscriptionClosure closure;

    private Subscription(Seq<Action> detach) =>
        closure = new SubscriptionClosure.Ready(Pending: detach, Release: new SubscriptionRelease.Open());

    public SubscriptionRelease Release {
        get {
            Task<SubscriptionRelease>? waiting;
            lock (gate) {
                if (closure is SubscriptionClosure.Ready ready) {
                    return ready.Release;
                }
                waiting = ((SubscriptionClosure.Closing)closure).Settled;
            }
            return SubscriptionRelease.Join(waiting);
        }
    }

    internal static Subscription Of(Action detach) {
        ArgumentNullException.ThrowIfNull(detach);
        return new(detach: Seq(detach));
    }

    public static Fin<Subscription> Attach<THandler>(Action<THandler> subscribe, Action<THandler> unsubscribe, THandler handler)
        where THandler : Delegate {
        Op key = Op.Of(name: nameof(Subscription));
        return key.Catch(() => { subscribe(obj: handler); return Fin.Succ(value: Of(detach: () => unsubscribe(obj: handler))); })
            .MapFail(error => key.Catch(() => { unsubscribe(obj: handler); return Fin.Succ(value: unit); }).Match(
                Succ: _ => error,
                Fail: cleanup => error + cleanup));
    }

    public static Fin<Subscription> Acquire(Action acquire, Action release) {
        ArgumentNullException.ThrowIfNull(acquire);
        ArgumentNullException.ThrowIfNull(release);
        Op key = Op.Of(name: nameof(Subscription));
        return key.Catch(() => { acquire(); return Fin.Succ(value: Of(detach: release)); })
            .MapFail(error => key.Catch(() => { release(); return Fin.Succ(value: unit); }).Match(
                Succ: _ => error,
                Fail: cleanup => error + cleanup));
    }

    public static Fin<Subscription> AttachAll(Seq<Func<Fin<Subscription>>> attach) =>
        attach.Fold(
            Fin.Succ(value: new Subscription(detach: Seq<Action>())),
            static (rail, start) => rail.Bind(held => start()
                .Map(held.Combine)
                .MapFail(held.Rollback)));

    internal Subscription Combine(Subscription other) {
        ArgumentNullException.ThrowIfNull(other);
        return new(detach: other.Snapshot().Concat(Snapshot()));
    }

    public SubscriptionRelease Close() {
        SubscriptionClosure.Ready? claimed = null;
        Task<SubscriptionRelease>? waiting = null;
        TaskCompletionSource<SubscriptionRelease>? flight = null;
        lock (gate) {
            if (closure is SubscriptionClosure.Closing closing) {
                waiting = closing.Settled;
            } else {
                claimed = (SubscriptionClosure.Ready)closure;
                if (claimed.Pending.IsEmpty) {
                    SubscriptionRelease settled = claimed.Release is SubscriptionRelease.Open
                        ? new SubscriptionRelease.Released(Attempted: 0)
                        : claimed.Release;
                    closure = claimed with { Release = settled };
                    return settled;
                }
                flight = SubscriptionRelease.BeginClose();
                closure = new SubscriptionClosure.Closing(Settled: flight.Task);
            }
        }
        if (waiting is not null) {
            return SubscriptionRelease.Join(waiting);
        }
        SubscriptionClosure.Ready owner = claimed!;
        (Seq<Action> Retry, Seq<Error> Errors) outcome = owner.Pending.Fold(
            (Retry: Seq<Action>(), Errors: Seq<Error>()),
            static (state, action) => Op.Of(name: nameof(Subscription))
                .Catch(() => { action(); return Fin.Succ(value: unit); })
                .Match(
                    Succ: _ => state,
                    Fail: error => (
                        Retry: state.Retry.Add(value: action),
                        Errors: state.Errors.Add(value: error))));
        SubscriptionRelease settled = outcome.Errors.IsEmpty
            ? new SubscriptionRelease.Released(Attempted: owner.Pending.Count)
            : new SubscriptionRelease.Faulted(Attempted: owner.Pending.Count, Errors: outcome.Errors);
        lock (gate) {
            closure = new SubscriptionClosure.Ready(Pending: outcome.Retry, Release: settled);
            return SubscriptionRelease.Publish(pending: flight!, release: settled);
        }
    }

    public void Dispose() => ignore(Close());

    internal Error Rollback(Error primary) => Close() switch {
        SubscriptionRelease.Faulted faulted => faulted.Errors.Fold(primary, static (error, cleanup) => error + cleanup),
        SubscriptionRelease.Open or SubscriptionRelease.Released => primary,
    };

    private Seq<Action> Snapshot() {
        while (true) {
            Task<SubscriptionRelease>? waiting;
            lock (gate) {
                if (closure is SubscriptionClosure.Ready ready) {
                    return ready.Pending;
                }
                waiting = ((SubscriptionClosure.Closing)closure).Settled;
            }
            ignore(SubscriptionRelease.Join(waiting));
        }
    }

    private abstract record SubscriptionClosure {
        private SubscriptionClosure() { }

        internal sealed record Ready(Seq<Action> Pending, SubscriptionRelease Release) : SubscriptionClosure;
        internal sealed record Closing(Task<SubscriptionRelease> Settled) : SubscriptionClosure;
    }
}

[Union]
public abstract partial record SubscriptionRelease {
    private SubscriptionRelease() { }
    public sealed record Open : SubscriptionRelease;
    public sealed record Released(int Attempted) : SubscriptionRelease;
    public sealed record Faulted(int Attempted, Seq<Error> Errors) : SubscriptionRelease;

    internal static TaskCompletionSource<SubscriptionRelease> BeginClose() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal static SubscriptionRelease Join(Task<SubscriptionRelease> pending) =>
        pending.GetAwaiter().GetResult();

    internal static SubscriptionRelease Publish(
        TaskCompletionSource<SubscriptionRelease> pending,
        SubscriptionRelease release) {
        pending.SetResult(release);
        return release;
    }

    internal static SubscriptionRelease All(params ReadOnlySpan<SubscriptionRelease> releases) {
        int attempted = 0;
        bool open = false;
        Seq<Error> errors = Seq<Error>();
        foreach (SubscriptionRelease release in releases) {
            switch (release) {
                case Open:
                    open = true;
                    break;
                case Released ready:
                    attempted = checked(attempted + ready.Attempted);
                    break;
                case Faulted faulted:
                    attempted = checked(attempted + faulted.Attempted);
                    errors = errors.Concat(faulted.Errors);
                    break;
            }
        }
        return !errors.IsEmpty
            ? new Faulted(Attempted: attempted, Errors: errors)
            : open
                ? new Open()
                : new Released(Attempted: attempted);
    }

    internal static Error AddTo(Error primary, SubscriptionRelease release) => release switch {
        Faulted faulted => faulted.Errors.Fold(primary, static (error, cleanup) => error + cleanup),
        Open or Released => primary,
    };
}

internal sealed class Reentrancy {
    private readonly AsyncLocal<int> depth = new();

    internal bool Active => depth.Value > 0;

    internal Fin<Unit> Guarded(ReceiptJournal journal, EventOrigin origin, Func<Fin<Unit>> run) {
        if (Active) {
            return Fin.Succ(value: journal.Post(new StreamReceipt.Reentrant(Watch: journal.Watch, Origin: origin)));
        }
        depth.Value++;
        try {
            return Op.Of(name: nameof(Reentrancy)).Catch(run).MapFail(error => {
                _ = journal.Post(new StreamReceipt.SinkFault(Watch: journal.Watch, Origin: origin, Detail: error.Message));
                return error;
            });
        } finally {
            depth.Value--;
        }
    }
}

internal sealed class IdlePump : IDisposable {
    private static long sequence;
    private readonly Lock gate = new();
    private readonly ReceiptJournal journal;
    private DeferredStage stage = new DeferredStage.Open(Pending: Seq<DeferredWork>());
    private Subscription? subscription;

    private IdlePump(ReceiptJournal journal) => this.journal = journal;

    internal static Fin<IdlePump> Open(ReceiptJournal journal) {
        IdlePump pump = new(journal: journal);
        return Subscription.Attach<EventHandler>(
                subscribe: handler => RhinoApp.Idle += handler,
                unsubscribe: handler => RhinoApp.Idle -= handler,
                handler: pump.OnIdle)
            .Map(attached => {
                pump.subscription = attached;
                return pump;
            });
    }

    internal Fin<Unit> Enqueue(
        EventOrigin origin,
        Func<bool> alive,
        Func<Fin<Unit>> run) {
        DeferredWork work = new(
            Id: Interlocked.Increment(ref sequence),
            Origin: origin,
            Alive: alive,
            Run: run);
        (bool Open, bool Accepted) admission;
        lock (gate) {
            if (stage is DeferredStage.Open open && open.Pending.Count < journal.Policy.DeferredCapacity) {
                stage = new DeferredStage.Open(Pending: open.Pending.Add(value: work));
                admission = (Open: true, Accepted: true);
            } else {
                admission = (Open: stage is DeferredStage.Open, Accepted: false);
            }
        }
        return Fin.Succ(value: admission switch {
            { Accepted: true } => unit,
            { Open: true } => journal.Post(new StreamReceipt.DeferredOverflow(Watch: journal.Watch, Origin: origin)),
            _ => journal.Post(new StreamReceipt.Cancelled(Watch: journal.Watch, Origin: origin)),
        });
    }

    internal SubscriptionRelease Close() {
        Seq<DeferredWork> pending;
        Subscription? claimed;
        lock (gate) {
            pending = stage is DeferredStage.Open open ? open.Pending : Seq<DeferredWork>();
            stage = new DeferredStage.Closed();
            claimed = subscription;
        }
        ignore(pending.Iter(work => ignore(journal.Post(new StreamReceipt.Cancelled(
            Watch: journal.Watch,
            Origin: work.Origin)))));
        SubscriptionRelease release = claimed?.Close() ?? new SubscriptionRelease.Released(Attempted: 0);
        lock (gate) {
            if (release is not SubscriptionRelease.Faulted && ReferenceEquals(subscription, claimed)) {
                subscription = null;
            }
        }
        return release;
    }

    public void Dispose() => ignore(Close());

    private void OnIdle(object? _, EventArgs __) => Drain();

    private void Drain() {
        Seq<DeferredWork> pending;
        lock (gate) {
            pending = stage is DeferredStage.Open open ? open.Pending : Seq<DeferredWork>();
            stage = stage is DeferredStage.Open ? new DeferredStage.Open(Pending: Seq<DeferredWork>()) : stage;
        }
        ignore(pending.Iter(work => ignore(work.Alive()
            ? Op.Of(name: nameof(IdlePump)).Catch(work.Run)
            : Fin.Succ(value: journal.Post(new StreamReceipt.Cancelled(
                Watch: journal.Watch,
                Origin: work.Origin))))));
    }

    [Union]
    private abstract partial record DeferredStage {
        private DeferredStage() { }
        internal sealed record Open(Seq<DeferredWork> Pending) : DeferredStage;
        internal sealed record Closed : DeferredStage;
    }

    private readonly record struct DeferredWork(
        long Id,
        EventOrigin Origin,
        Func<bool> Alive,
        Func<Fin<Unit>> Run);
}
```
