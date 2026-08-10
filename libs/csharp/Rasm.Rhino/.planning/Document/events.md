# [RASM_RHINO_EVENTS]

`DocumentStream` owns observation from raw host and filesystem callbacks: detached facts, nonblocking delivery, bounded loss evidence, retryable symmetric detachment. `Observation` carries source-specific admission, `EventFamily` carries host wiring as data, and `Watch` retains delivery and release outcomes under one identity. `RhinoPoint` names every detached stream as `rasm.rhino.<domain>.<point>` under the kernel `HookModality` rows, `MountRegistry` owns name-addressed discovery and first-mount-wins custody over the adopter mounts, and `RhinoInstruments` declares the contributed rows.

## [01]-[INDEX]

- [02]-[FAMILY]: `EventFamily` binds host callbacks, cadence, and projection as data.
- [03]-[PAYLOAD_PROJECTION]: `EventPayload` and `DocEvent` carry detached callback evidence.
- [04]-[DELIVERY_POLICY]: `Delivery` and `ReceiptPolicy` close bounded delivery and loss evidence.
- [05]-[STREAM_OWNER]: `DocumentStream`, `Watch`, and `CommitSink` own admission, attachment, delivery, the sealed-commit contributor registry consuming `OPLOG_ENTRY`, and release.
- [06]-[HOOK_REGISTRY]: `RhinoPoint`, `HookMount`, and `MountRegistry` close point addressing, host-truth modality over the kernel rows, mount custody, and multi-plugin arbitration.
- [07]-[TELEMETRY_TAP]: `RhinoInstruments` declares the contributed instrument rows and the string-scoped port.

## [02]-[FAMILY]

- Owner: `EventFamily` binds one symbolic host event key to its band, cadence, attach/detach pair, and callback-scope projection.
- Entry: `EventFamily.In` derives band membership from generated `Items`, while `Bind` retains the exact attached delegate for release.
- Law: draw facts retain phase and viewport evidence without retaining `DisplayPipeline`, and per-object phases add the drawn or culled `RhinoObject` identity.
- Law: a bracketed host pair is one family — `Transform` binds `BeforeTransformObjects` and `AfterTransformObjects` under one `CorrelationWindow` keyed on the host `TransformEventId` both sides publish, so the closing arm resolves the opening arm's `DocKey` and every scope that delivers a start delivers its matching end; the payload case, never the family, discriminates start from end.
- Law: table projections detach transition, index, and prior/current component evidence; later live resolution re-enters through document identity.
- Law: callback projection faults and sink faults remain disjoint receipts; delivery failure never reclassifies as callback failure.
- Exemption: `CorrelationWindow` is a bounded concurrent kernel serving projection deduplication and bracket correlation because callbacks arrive across host threads.
- Growth: a host callback — or a host pair bracketing one fact — lands as one symbolic `EventFamily` row whose projection expires every callback-owned handle before delivery.

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
    public static readonly EventFamily Transform = new(key: nameof(Transform), band: EventBand.Structure, cadence: Cadence.Changed, bind: Bracketed<RhinoTransformObjectsEventArgs, RhinoAfterTransformObjectsEventArgs>(
        subscribeOpen: h => RhinoDoc.BeforeTransformObjects += h,
        unsubscribeOpen: h => RhinoDoc.BeforeTransformObjects -= h,
        subscribeClose: h => RhinoDoc.AfterTransformObjects += h,
        unsubscribeClose: h => RhinoDoc.AfterTransformObjects -= h,
        correlateOpen: static a => a.TransformEventId,
        correlateClose: static a => a.TransformEventId,
        open: TransformFact,
        close: static a => new EventPayload.TransformEnded(EventId: a.TransformEventId),
        family: static () => Transform));

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
                Current: transition.CarriesCurrent ? Optional(a.NewMapping).Map(static mapping => mapping.Id) : Option<Guid>.None)))));

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
        subscribe: h => DisplayPipeline.ViewportProjectionChanged += h,
        unsubscribe: h => DisplayPipeline.ViewportProjectionChanged -= h,
        family: static () => ProjectionChanged));
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

    public static Fin<Seq<EventFamily>> In(EventBand band, Op? key = null) =>
        Optional(band)
            .ToFin(Fail: key.OrDefault().InvalidInput())
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
        Action<EventHandler<DrawEventArgs>> unsubscribe,
        Func<EventFamily> family) =>
        (scope, journal, deliver, reject) => {
            CorrelationWindow<(Guid Viewport, uint Document), uint> seen = new(capacity: journal.Policy.CorrelationCapacity);
            return On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, a, watched) =>
                Optional(a.RhinoDoc).Bind(document => Optional(a.Viewport).Bind(viewport => {
                    uint counter = viewport.ChangeCounter;
                    (bool Advanced, int Cleared) advance = seen.Retain(key: (viewport.Id, document.RuntimeSerialNumber), value: counter);
                    _ = Cleared(journal: journal, family: family, cleared: advance.Cleared);
                    return advance.Advanced
                        ? Gate(document: document, scope: watched, payload: new EventPayload.Projection(ViewportId: viewport.Id, ChangeCounter: counter))
                        : Option<EventEnvelope>.None;
                })))(scope, journal, deliver, reject);
        };

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> Bracketed<TOpen, TClose>(
        Action<EventHandler<TOpen>> subscribeOpen,
        Action<EventHandler<TOpen>> unsubscribeOpen,
        Action<EventHandler<TClose>> subscribeClose,
        Action<EventHandler<TClose>> unsubscribeClose,
        Func<TOpen, uint> correlateOpen,
        Func<TClose, uint> correlateClose,
        Func<TOpen, Option<(DocKey Key, EventPayload Payload)>> open,
        Func<TClose, EventPayload> close,
        Func<EventFamily> family)
        where TOpen : EventArgs
        where TClose : EventArgs =>
        (scope, journal, deliver, reject) => {
            CorrelationWindow<uint, DocKey> bracket = new(capacity: journal.Policy.CorrelationCapacity);
            return Subscription.AttachAll(Seq<Func<Fin<Subscription>>>(
                () => On(subscribe: subscribeOpen, unsubscribe: unsubscribeOpen, project: (_, args, watched) =>
                    open(arg: args).Bind(fact => {
                        _ = Cleared(journal: journal, family: family, cleared: bracket.Retain(key: correlateOpen(arg: args), value: fact.Key).Cleared);
                        return Gate(key: fact.Key, scope: watched, payload: fact.Payload);
                    }))(scope, journal, deliver, reject),
                () => On(subscribe: subscribeClose, unsubscribe: unsubscribeClose, project: (_, args, watched) =>
                    bracket.Release(key: correlateClose(arg: args))
                        .Bind(key => Gate(key: key, scope: watched, payload: close(arg: args))))(scope, journal, deliver, reject)));
        };

    private static Unit Cleared(ReceiptJournal journal, Func<EventFamily> family, int cleared) =>
        cleared > 0
            ? journal.Post(new StreamReceipt.CorrelationReset(Watch: journal.Watch, Family: family(), Cleared: cleared))
            : unit;

    private static Option<EventEnvelope> ObjectFact(object? sender, RhinoObjectEventArgs args, EventScope scope) =>
        Gate(document: (sender as RhinoDoc) ?? args.TheObject?.Document, scope: scope, payload: new EventPayload.Objects(Ids: Seq(args.ObjectId)));

    private static Option<EventEnvelope> SelectionFact(object? sender, RhinoObjectSelectionEventArgs args, EventScope scope) =>
        Gate(document: args.Document, scope: scope, payload: new EventPayload.Selection(
            Ids: toSeq(args.RhinoObjects).Choose(static item => Optional(item).Map(static value => value.Id)),
            Count: args.RhinoObjectCount));

    private static Option<(DocKey Key, EventPayload Payload)> TransformFact(RhinoTransformObjectsEventArgs args) =>
        TransformDocument(args).Map(key => (Key: key, Payload: (EventPayload)new EventPayload.TransformStarted(
            EventId: args.TransformEventId,
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
            .Head;

    private static Seq<(Guid Id, uint Serial)> ObjectRefs(IEnumerable<RhinoObject?> objects) =>
        toSeq(objects).Choose(static item => Optional(item).Map(static value => (value.Id, value.RuntimeSerialNumber)));

    private static Option<(Guid Id, uint Serial)> DrawSubject(RhinoObject? subject) =>
        Optional(subject).Map(static value => (value.Id, value.RuntimeSerialNumber)).Filter(static value => value.Id != Guid.Empty);

    private sealed class CorrelationWindow<TKey, TValue>(int capacity)
        where TKey : notnull
        where TValue : notnull {
        private readonly Lock gate = new();
        private readonly Dictionary<TKey, TValue> held = new();

        internal (bool Advanced, int Cleared) Retain(TKey key, TValue value) {
            lock (gate) {
                int cleared = 0;
                if (held.Count >= capacity && !held.ContainsKey(key: key)) {
                    cleared = held.Count;
                    held.Clear();
                }
                bool advanced = !held.TryGetValue(key: key, value: out TValue? prior)
                    || !EqualityComparer<TValue>.Default.Equals(x: prior, y: value);
                if (advanced) {
                    held[key] = value;
                }
                return (Advanced: advanced, Cleared: cleared);
            }
        }

        internal Option<TValue> Release(TKey key) {
            lock (gate) {
                return held.Remove(key: key, value: out TValue? claimed) ? Some(claimed) : Option<TValue>.None;
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
}
```

## [03]-[PAYLOAD_PROJECTION]

- Owner: `EventPayload` owns detached callback evidence, while `DocEvent` adds source identity and the optional document key.
- Law: every reference-like host member projects inside its callback into stable identity, value, transition, or component evidence.
- Law: an absent active document remains a typed transition; `TransformStarted` and `TransformEnded` both carry the host `TransformEventId`, so a consumer joins the bracket on that id without retaining either callback's arrays.
- Law: name-keyed transition vocabularies admit host enums generically and fail unknown host values on the typed rail.
- Law: `EventPayload.ObjectIds` defaults to no object contribution, and contributing cases override that projection; `DocEvent` delegates without an empty-arm dispatch ladder.
- Law: `EventPayload.Sealed` carries one commit record's mutation roster as `SealedMutation` rows over the SAME `TableKind`/`ComponentTransition` vocabulary the `Component` case already spells, so a consumer folds both through one axis and the sealed case mints no parallel transition family.

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
        Op op = Op.Of(name: typeof(T).Name);
        return Op.Text(Enum.GetName(value: value)).ToFin(Fail: op.InvalidResult()).Bind(key => op.Row<string, T>(key));
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
        uint EventId,
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
    public sealed record TransformEnded(uint EventId) : EventPayload;
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
    // What one sealed commit record mutated, in HOST vocabulary alone. No causal identity and no clock: this boundary
    // holds no store origin slot and no observed frontier, so an operation id minted here is a coordinate two hosts
    // collide on — the store owning the origin mints it at the seam and maps these rows onto its own lanes. `Serial`
    // is the host's undo coordinate and stays host-local evidence, because no peer replays another host's undo stack.
    public sealed record Sealed(string Record, uint Serial, Seq<SealedMutation> Mutations) : EventPayload {
        public override Seq<Guid> ObjectIds => Mutations.Map(static mutation => mutation.Id).Distinct();
    }
}

public readonly record struct SealedMutation(TableKind Kind, Guid Id, ComponentTransition Transition);

// --- [MODELS] -----------------------------------------------------------------------------
internal readonly record struct EventEnvelope(Option<DocKey> Key, EventPayload Payload);

[Union]
public abstract partial record EventOrigin {
    private EventOrigin() { }
    public sealed record Host(EventFamily Family) : EventOrigin;
    public sealed record File(string WatchedPath) : EventOrigin;
    public sealed record Commit(string Record) : EventOrigin;
}

// This host keeps ONE contributor registry over sealed-commit facts, fanned by the
// `Document/tables#TRANSACTION_RAIL` railed `project` slot. Contributors register through `Observation.Commit` attachment, so a closing watch detaches
// its own row under the same symmetric release law every host family obeys; a per-folder sink beside this one
// publishes some commits and not others.
//
// Registration stays PROCESS-STATIC by host law rather than by convenience: RhinoCommon's document tables and
// undo stack are process singletons, so a per-composition registry inside one `Rhino.exe` would let two co-resident
// plugins each hold a partial view of one document's commits — the exact inverse of the per-composition law that
// governs host-free packages, and stated here so the divergence reads as a decision.
//
// These facts are the host-local half of the estate's `OPLOG_ENTRY` contract: a contributor projecting them onto
// that contract is a NAMED consumer of it, and the contract's own envelope — `Rasm/Domain/event#ENVELOPE_MINT`
// minted at the durable owner — is what carries them past this process. This host mints no envelope of its own,
// because a sealed commit crossing a boundary is an announcement the durable owner already publishes.
public delegate Fin<Unit> CommitTap(DocKey Document, string Record, uint Serial, Seq<SealedMutation> Mutations);

public static class CommitSink {
    private static readonly Lock gate = new();
    private static Seq<CommitTap> taps = Seq<CommitTap>();

    internal static void Add(CommitTap tap) { lock (gate) { taps = taps.Add(value: tap); } }
    internal static void Remove(CommitTap tap) { lock (gate) { taps = taps.Filter(held => !ReferenceEquals(objA: held, objB: tap)); } }

    // Composed as the envelope's `project` continuation, so it runs INSIDE the undo bracket after the serial stamp:
    // one refusing tap fails the publication and the sealed record rolls back, which is exactly why the fan runs
    // there rather than beside the envelope, where it would publish a record the seal then discarded.
    public static Func<TReceipt, Fin<TReceipt>> Sealing<TReceipt>(
        DocKey document, string record, Func<TReceipt, (uint Serial, Seq<SealedMutation> Mutations)> read) =>
        receipt => {
            (uint serial, Seq<SealedMutation> mutations) = read(arg: receipt);
            return Snapshot()
                .Traverse(tap => tap(document, record, serial, mutations))
                .As()
                .Map(_ => receipt);
        };

    private static Seq<CommitTap> Snapshot() { lock (gate) { return taps; } }
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
        key.Row<int, FileChangeKind>((int)native);
}

public readonly record struct FileEdge(FileChangeKind Kind, string Path, Option<string> PreviousPath);
```

## [04]-[DELIVERY_POLICY]

- Owner: `Delivery` owns direct, idle-deferred, and paced modalities; `StreamLane` resolves paced channel construction from the admitted `ReceiptPolicy`.
- Law: host callbacks never park — each paced lane either accepts immediately or emits loss evidence through the channel callback and write result.
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
    public sealed record CorrelationReset(WatchKey Watch, EventFamily Family, int Cleared) : StreamReceipt;
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

- Owner: `Observation` carries each source's complete ingress, and `DocumentStream.Observe` owns admission, attachment, rollback, and watch minting; `CommitSink` owns the host's ONE contributor registry over sealed-commit facts and the `Sealing` projection `DocumentCommit.Sealed` composes.
- Law: every source, delivery, policy, and source-specific value admits before the first attachment; sequential attachment rolls back the accumulated prefix on failure.
- Law: sealed-commit facts enter through `CommitSink.Sealing`, composed as the `Document/tables#TRANSACTION_RAIL` railed `project` continuation, so publication runs inside the undo bracket after the serial stamp and a tap refusing the change rolls the record back; a tap spelled beside `DocumentCommit.Sealed` publishes a record the seal then discards. `Observation.Commit` admits `Delivery.Inline` alone, because a deferred or paced arm returns success before any subscriber saw the fact and forfeits exactly that rollback.
- Law: `CommitSink` is a NAMED consumer of the estate's `OPLOG_ENTRY` contract and never a producer of the message envelope carrying it — a contributor projects a sealed commit onto that contract at the durable owner, which mints the CloudEvents announcement through the one branch message envelope algebra, so this host publishes host facts and the durable owner publishes the fact's crossing. A message envelope minted here would announce from a process whose serials no peer replays.
- Law: the contributor registry is PROCESS-STATIC by host law, not by convenience — RhinoCommon's document tables and undo stack are process singletons, so a per-composition registry inside one `Rhino.exe` gives two co-resident plugins a partial view each of one document's commits; the divergence from the per-composition registry law host-free packages obey is a decision this row states rather than an oversight.
- Law: sealed facts cross in HOST vocabulary — `TableKind`, component identity, `ComponentTransition`, and the undo serial — carrying no operation identity, no clock, and no lane. This boundary references `Rasm` alone, holds no store origin slot and no observed frontier, and an identity minted here is a coordinate two hosts collide on; the store owning the origin mints it at the seam and maps these rows onto its own lanes. Serials stay host-local evidence, since no peer replays another host's undo stack.
- Law: `Watch.Close` cancels delivery, combines source and idle-pump detachment evidence, receipts each fault, and retains each failed owner for a later close attempt.
- Law: close claims its owners under the lifecycle lock, executes callbacks after release, and publishes retry custody with one settled result atomically; concurrent callers join that result.
- Law: an empty subscription closes as `Released(0)`; `Open` denotes only unclaimed live custody.
- Law: reentrancy and deferred capacity belong to one watch, so recursive or queued work cannot suppress or exhaust a sibling observation; `Reentrancy.Guarded` is the whole reentrancy decision for both sink-invoking arms, and the paced arm invokes no sink and consults no gate, so `Emit` carries no second guard reading as an owner.
- Law: deferred delivery owns one idle hook per watch; closing the watch detaches that hook and receipts every queued fact as cancelled.
- Law: file callbacks fold into one resettable trailing-edge timer and one bounded batch before entering the same delivery spine as host facts.
- Exemption: native attach/detach, timer ownership, `Lock` scopes, and callback `try/finally` blocks are platform-forced lifetime seams.
- Law: `LifecycleGate` is the package's ONE claims/close/retry lifecycle capsule — every bounded-settle lease across the boundary (pointer leases, content streams, watch custody) composes it from this namespace, and a sibling hand-rolling a `lock`/`Monitor` lifecycle machine beside it is the collapsed form.

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
    // Sealed-commit source: attachment registers a `CommitTap` rather than a host callback, since the fact this
    // source carries is minted by the boundary's own commit envelope and no RhinoCommon event reports it.
    public sealed record Commit(
        EventScope Scope,
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
        key.Need(delivery).Bind(active => active.Switch(
            (Journal: journal, Op: key),
            inline: static (state, mode) => state.Op.Need(mode.Sink)
                .Map(_ => new Emission(
                    delivery: mode,
                    journal: state.Journal,
                    channel: Option<Channel<DocEvent>>.None,
                    idle: Option<IdlePump>.None)),
            deferred: static (state, mode) => state.Op.Need(mode.Sink)
                .Bind(_ => IdlePump.Open(journal: state.Journal))
                .Map(pump => new Emission(
                    delivery: mode,
                    journal: state.Journal,
                    channel: Option<Channel<DocEvent>>.None,
                    idle: Some(pump))),
            paced: static (state, mode) => state.Op.Need(mode.Lane).Bind(lane =>
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
        return op.Need(request).Bind(active => active.Switch(
            op,
            host: static (key, observation) => ObserveHost(request: observation, key: key),
            file: static (key, observation) => ObserveFile(request: observation, key: key),
            commit: static (key, observation) => ObserveCommit(request: observation, key: key)));
    }

    // Commit observation admits `Inline` delivery alone: the tap runs inside the undo bracket, so a deferred or paced
    // arm returns success before any subscriber saw the fact and the bracket's rollback guarantee then means nothing.
    private static Fin<Watch> ObserveCommit(Observation.Commit request, Op key) =>
        from scope in key.Need(request.Scope)
        from delivery in key.Need(request.Delivery)
        from _ in guard(delivery is Delivery.Inline, key.Unsupported(geometryType: typeof(Observation.Commit), outputType: typeof(Delivery))).ToFin()
        from watch in Mount(
            delivery: delivery,
            policy: request.Receipts,
            attach: (emission, _) => AttachCommit(scope: scope, emission: emission),
            key: key)
        select watch;

    // Attachment is registration into `CommitSink`, the symmetric pair `Subscription.Attach` releases on close. The
    // scope filter reads the closed `EventScope` union rather than a nullable key, so an any-document watch and a
    // per-document watch differ by ROW and never by a null test the fan would have to repeat.
    private static Fin<Subscription> AttachCommit(EventScope scope, Emission emission) {
        CommitTap handler = (document, record, serial, mutations) =>
            scope.Switch(document, document: static (key, arm) => arm.Key == key, anyDocument: static (_, _) => true)
                ? emission.Emit(fact: new DocEvent(
                    Origin: new EventOrigin.Commit(Record: record),
                    Key: Some(document),
                    Payload: new EventPayload.Sealed(Record: record, Serial: serial, Mutations: mutations)))
                : Fin.Succ(value: unit);
        return Subscription.Attach(subscribe: CommitSink.Add, unsubscribe: CommitSink.Remove, handler: handler);
    }

    private static Fin<Watch> ObserveHost(Observation.Host request, Op key) =>
        from scope in key.Need(request.Scope)
        from delivery in key.Need(request.Delivery)
        from families in request.Families
            .TraverseM(family => key.Need(family))
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
        from bounds in key.Need(policy)
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
        from clock in key.Need(request.Clock)
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

// Package-wide bounded-lifecycle capsule: one claims/close/retry machine every boundary lease composes.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LeaseState {
    private LeaseState() { }
    internal sealed record Open(int Claims) : LeaseState;
    internal sealed record Closing(int Claims, Guid Token, TaskCompletionSource<Unit> Quiesced, TaskCompletionSource<Fin<Unit>> Completed) : LeaseState;
    internal sealed record Retryable(int Claims) : LeaseState;
    internal sealed record Closed : LeaseState;
}

internal sealed class LifecycleGate {
    private readonly Atom<LeaseState> state = Atom<LeaseState>(new LeaseState.Open(Claims: 0));
    // A claim runs to completion on the thread that took it, so a close issued from a thread already inside a claim would
    // wait on its own release forever. The claiming-thread set is the structural refusal for that re-entrancy, and it is
    // what keeps a bounded blocking close safe on the host callback thread.
    private readonly Atom<Set<int>> claiming = Atom(Set<int>());
    private readonly TimeSpan settleWithin;
    private LifecycleGate(TimeSpan settleWithin) => this.settleWithin = settleWithin;
    internal static Fin<LifecycleGate> Of(TimeSpan settleWithin, Op key) =>
        guard(settleWithin > TimeSpan.Zero, key.InvalidInput()).ToFin().Map(_ => new LifecycleGate(settleWithin));

    internal Fin<T> Within<T>(Func<Fin<T>> body, Func<Fin<T>> refused, Op key) =>
        TryClaim() ? Settle(Marked(body, key)) : key.Catch(refused);

    // The drain is bounded but still BLOCKING, so it never rides the closing caller's thread: `Begin` arms the close,
    // runs `stop` on the caller's own rail so a marshalled arm keeps its seam, and hands back the completion — a host
    // UI-thread owner settles that completion off-thread, because blocking there stalls the very callbacks the drain
    // waits to see released. `Close` is the blocking convenience a pool caller takes over the same one-owner close.
    internal Fin<Unit> Close(Func<Fin<Unit>> stop, Func<Fin<Unit>> settle, Op key) =>
        Begin(stop, settle, key).Bind(completion => Await(completion, key)).Bind(static outcome => outcome);

    internal Fin<Task<Fin<Unit>>> Begin(Func<Fin<Unit>> stop, Func<Fin<Unit>> settle, Op key) {
        if (claiming.Value.Contains(Environment.CurrentManagedThreadId)) { return Fin.Fail<Task<Fin<Unit>>>(key.InvalidContext()); }
        Guid token = Guid.NewGuid();
        TaskCompletionSource<Unit> quiesced = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<Fin<Unit>> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        LeaseState next = state.Swap(current => current.Switch(
            (Token: token, Quiesced: quiesced, Completed: completed),
            open: static (ctx, row) => (LeaseState)new LeaseState.Closing(row.Claims, ctx.Token, ctx.Quiesced, ctx.Completed),
            closing: static (_, row) => row,
            retryable: static (ctx, row) => new LeaseState.Closing(row.Claims, ctx.Token, ctx.Quiesced, ctx.Completed),
            closed: static (_, row) => row));
        return next.Switch(
            (Gate: this, Token: token, Stop: stop, Settle: settle, Key: key),
            open: static (ctx, _) => Fin.Fail<Task<Fin<Unit>>>(ctx.Key.InvalidContext()),
            closing: static (ctx, row) => Fin.Succ(row.Token == ctx.Token
                ? ctx.Gate.Drain(row, ctx.Stop, ctx.Settle, ctx.Key)
                : row.Completed.Task),
            retryable: static (ctx, _) => Fin.Fail<Task<Fin<Unit>>>(ctx.Key.InvalidContext()),
            closed: static (_, _) => Fin.Succ(Task.FromResult(Fin.Succ(unit))));
    }

    private bool TryClaim() => state.Swap(current => current.Switch(
        open: static row => (LeaseState)new LeaseState.Open(row.Claims + 1),
        closing: static row => row,
        retryable: static row => row,
        closed: static row => row)).Switch(
            open: static _ => true,
            closing: static _ => false,
            retryable: static _ => false,
            closed: static _ => false);

    private Fin<T> Marked<T>(Func<Fin<T>> body, Op key) {
        int thread = Environment.CurrentManagedThreadId;
        _ = claiming.Swap(rows => rows.Add(thread));
        try { return key.Catch(body); }
        finally { _ = claiming.Swap(rows => rows.Remove(thread)); }
    }

    private Fin<T> Settle<T>(Fin<T> outcome) => outcome.BiBind(
        Succ: value => (Release(), Fin.Succ(value)).Item2,
        Fail: failure => (Release(), Fin.Fail<T>(failure)).Item2);

    private Unit Release() => state.Swap(current => current.Switch(
        open: static row => (LeaseState)new LeaseState.Open(row.Claims - 1),
        closing: static row => new LeaseState.Closing(row.Claims - 1, row.Token, row.Quiesced, row.Completed),
        retryable: static row => new LeaseState.Retryable(row.Claims - 1),
        closed: static row => row)).Switch(
            open: static _ => unit,
            closing: static row => Op.SideWhen(row.Claims == 0, () => row.Quiesced.TrySetResult(unit)),
            retryable: static _ => unit,
            closed: static _ => unit);

    // The owning close alone drives the drain, and it drives it as a SCHEDULER continuation: `stop` runs inline on the
    // caller's rail, then the bounded wait and the settle ride the pool. A gate whose claims are already zero completes
    // its own quiesce signal here rather than branching, so both paths reach one conclusion member.
    private Task<Fin<Unit>> Drain(LeaseState.Closing row, Func<Fin<Unit>> stop, Func<Fin<Unit>> settle, Op key) {
        Fin<Unit> stopped = key.Catch(stop);
        _ = Op.SideWhen(row.Claims == 0, () => ignore(row.Quiesced.TrySetResult(unit)));
        return row.Quiesced.Task.WaitAsync(settleWithin).ContinueWith(
            drained => Conclude(
                row,
                stopped,
                drained.Status == TaskStatus.RanToCompletion ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidContext()),
                settle,
                key),
            CancellationToken.None,
            TaskContinuationOptions.RunContinuationsAsynchronously,
            TaskScheduler.Default);
    }

    private Fin<Unit> Conclude(LeaseState.Closing row, Fin<Unit> stopped, Fin<Unit> drained, Func<Fin<Unit>> settle, Op key) {
        Fin<Unit> settled = drained.Match(
            Succ: _ => key.Catch(settle),
            Fail: static _ => Fin.Succ(unit));
        Seq<Error> trouble = Seq(
                stopped,
                drained,
                settled)
            .Choose(static step => step.Match(
                Succ: static _ => Option<Error>.None,
                Fail: static failure => Some(failure)));
        Fin<Unit> outcome = trouble.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(trouble.Fold(Errors.None, static (folded, failure) => folded + failure));
        _ = state.Swap(current => current.Switch(
            open: static value => (LeaseState)value,
            closing: value => value.Token == row.Token
                ? trouble.IsEmpty ? new LeaseState.Closed() : new LeaseState.Retryable(value.Claims)
                : value,
            retryable: static value => value,
            closed: static value => value));
        _ = row.Completed.TrySetResult(outcome);
        return outcome;
    }

    private Fin<T> Await<T>(Task<T> signal, Op key) => key.Catch(() =>
        signal.Wait(settleWithin) ? Fin.Succ(signal.Result) : Fin.Fail<T>(key.InvalidContext()));
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

## [06]-[HOOK_REGISTRY]

- Owner: `RhinoPoint` is the closed boundary-wide point vocabulary addressed `rasm.rhino.<domain>.<point>`, its rows ruled by the kernel `HookModality` vocabulary; `HookMount` carries one owner-registered binding as data; `MountRegistry` owns name-addressed discovery, first-mount-wins custody, and typed grant binding — a different concern than the kernel's composition-frozen point mount, so it carries its own name; `PluginKey` is the plugin identity every process-global claim keys on.
- Law: a point name resolves in one hop — a consumer binds `MountRegistry.Bind` on the point key and receives the owning stream's own grant (a `Watch`, `PointerLease`, `WidgetHost`, `Subscription`, or `ContentStream`), so no consumer learns a per-domain stream API and no second delivery path forms beside the owner's bounded lanes.
- Law: modality is host truth, never a registry promise — a `Veto` row exists only where the host callback admits refusal, the veto-truth census citing the exact host member; every other point is post-hoc `Observe`, and `Replay` marks only a point whose owner retains a readable latest-value ledger.
- Law: mount custody is one SEATED BINDING per point with keyed riders — the first mount seats the owning page's binding and registers its plugin as the first rider, every later plugin rides the same seat as a keyed subscriber, and the machinery beneath (the `ObjectsTelemetry` keyed-sink fan, the `HostTap` rider handoff, the per-plugin `Watch`) serves each rider its own grant at `Bind`; a DIVERGENT binding — different ask or grant type against a live seat — faults typed because two machineries under one point fork discovery, a same-plugin duplicate rider faults typed, each detacher retires exactly its own rider, and the seat frees when the last rider leaves; `MountAll` releases an admitted prefix when any later row refuses, scoped to the rolling-back plugin's riders alone.
- Law: telemetry is a tap — the `rasm.rhino.objects.fault` point binds onto the `ObjectsTelemetry` keyed-sink fan, and the `rasm.rhino.host.exception`/`rasm.rhino.host.log` points bind the `HostUtils.OnExceptionReport` and `HostUtils.OnSendLogMessageToCloud` statics onto the same fan through the `HostTap.Mount` seat, so observability subscribes to domain facts and no emit call rides inside domain code.
- Law: process-global custody is a closed census — every collision surface carries its collision class and arbitration row below, and a new process-global surface is one census row with its arbitration named before any fence composes it.
- Growth: a new fact stream is one `RhinoPoint` row with its `HookMount` registration on its owning page; a new plugin-visible custody surface is one census row.

Document point census — every band rides the one stream owner:

| [INDEX] | [POINT]                         | [PAYLOAD]  | [MODALITY] | [OWNER_ENTRY]                                       |
| :-----: | :------------------------------ | :--------- | :--------- | :-------------------------------------------------- |
|  [01]   | `rasm.rhino.document.lifecycle` | `DocEvent` | observe    | `DocumentStream.Observe` over `EventBand.Lifecycle` |
|  [02]   | `rasm.rhino.document.structure` | `DocEvent` | observe    | `DocumentStream.Observe` over `EventBand.Structure` |
|  [03]   | `rasm.rhino.document.selection` | `DocEvent` | observe    | `DocumentStream.Observe` over `EventBand.Selection` |
|  [04]   | `rasm.rhino.document.tables`    | `DocEvent` | observe    | `DocumentStream.Observe` over `EventBand.Tables`    |
|  [05]   | `rasm.rhino.document.screen`    | `DocEvent` | observe    | `DocumentStream.Observe` over `EventBand.Screen`    |
|  [06]   | `rasm.rhino.document.draw`      | `DocEvent` | observe    | `DocumentStream.Observe` over `EventBand.Draw`      |
|  [07]   | `rasm.rhino.document.panels`    | `DocEvent` | observe    | `DocumentStream.Observe` over `EventBand.Panels`    |
|  [08]   | `rasm.rhino.document.file`      | `DocEvent` | observe    | `DocumentStream.Observe` over `Observation.File`    |

Surface point census — display, objects, host, and render streams; each owning page carries the mount:

| [INDEX] | [POINT]                         | [PAYLOAD]     | [MODALITY]      | [OWNER_ENTRY]                                            |
| :-----: | :------------------------------ | :------------ | :-------------- | :------------------------------------------------------- |
|  [01]   | `rasm.rhino.display.pointer`    | `PointerFact` | observe, veto   | `DisplayHooks.Mount` over `Pointers.Configure`           |
|  [02]   | `rasm.rhino.display.widget`     | `WidgetFact`  | observe         | `DisplayHooks.Mount` over `WidgetHost.Of`                |
|  [03]   | `rasm.rhino.display.cull`       | per-object    | veto            | `ConduitHooks.Mount` over `ConduitStep.Cull`             |
|  [04]   | `rasm.rhino.display.drawobject` | per-object    | veto            | `ConduitHooks.Mount` over `ConduitStep.Suppress`         |
|  [05]   | `rasm.rhino.objects.viewable`   | per-object    | veto            | `ObjectsHooks.Mount` admitting `ObjectProgram.Viewable`  |
|  [06]   | `rasm.rhino.objects.pick`       | per-object    | veto            | `ObjectsHooks.Mount` admitting `ObjectProgram.Pick`      |
|  [07]   | `rasm.rhino.objects.regrow`     | per-object    | veto            | `ObjectsHooks.Mount` admitting the `GripProgram` refusal |
|  [08]   | `rasm.rhino.objects.fault`      | fault event   | observe         | `ObjectsHooks.Mount` onto the `ObjectsTelemetry` sink    |
|  [09]   | `rasm.rhino.host.exception`     | fault event   | observe         | `ObjectsHooks.Mount` onto the `HostTap.Mount` seat       |
|  [10]   | `rasm.rhino.host.log`           | `HostLogFact` | observe         | `ObjectsHooks.Mount` onto the `HostTap.Mount` seat       |
|  [11]   | `rasm.rhino.hostui.panel`       | `PanelFact`   | observe, replay | `PanelHooks.Mount` over `PanelHost.Watch`                |
|  [12]   | `rasm.rhino.hostui.skin`        | `SkinPhase`   | observe         | `ShellHooks.Mount` over the `ShellSkin` phase route      |
|  [13]   | `rasm.rhino.render.content`     | `ContentFact` | observe         | `ContentHooks.Mount` over `ContentStream.Of`             |

Veto and replay truth — the exact host member each non-observe row cites:

| [INDEX] | [POINT]                         | [HOST_TRUTH]                                                                           |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `rasm.rhino.display.cull`       | `ConduitStep.Cull` writes `CullObjectEventArgs.CullObject`                             |
|  [02]   | `rasm.rhino.display.drawobject` | `ConduitStep.Suppress` writes `DrawObjectEventArgs.DrawObject`                         |
|  [03]   | `rasm.rhino.objects.viewable`   | `RhinoObject.IsActiveInViewport`                                                       |
|  [04]   | `rasm.rhino.objects.pick`       | `RhinoObject.OnPick` sift                                                              |
|  [05]   | `rasm.rhino.objects.regrow`     | `CustomObjectGrips.NewGeometry` refusal                                                |
|  [06]   | `rasm.rhino.hostui.panel`       | `PanelHost.Facts` latest-per-seat replay ledger, one row per (plugin, panel, document) |

Gumball completion evidence is receipt-pull — `GumballReceipt` returns from `Gumballs.Configure` and no detached stream exists — so gumball earns no point row, and the pointer point already carries gumball occupancy per fact.

Process-global custody census — collision class, arbitration, and seat cardinality per surface; `fan` seats one row per plugin, `single` seats one process-wide owner later callers ride or fault against, `host` defers cardinality to a host-native mechanism:

| [INDEX] | [SURFACE]                                  | [COLLISION_CLASS]                    | [ARBITRATION]                            | [SEATS] |
| :-----: | :----------------------------------------- | :----------------------------------- | :--------------------------------------- | :------ |
|  [01]   | `RhinoDoc`/`RhinoView`/`DisplayPipeline`   | duplicate watches double facts       | per-plugin `Watch`; delegate identity    | fan     |
|  [02]   | `RhinoApp.Idle` (`IdlePump`)               | multicast-safe                       | one pump per deferred watch              | fan     |
|  [03]   | `HostUtils` exception / cloud-log statics  | duplicate mounts double-publish      | `HostTap.Mount`; first mount, later ride | single  |
|  [04]   | `ObjectsTelemetry` sink                    | replacement shadows a prior plugin   | `PluginKey` rows; teardown per caller    | fan     |
|  [05]   | `HostUtils.RegisterNamedCallback`          | re-register replaces the handler     | `PluginKey` claim tokens, keyed per name | single  |
|  [06]   | `Panels.RegisterPanel` / page registration | host isolates; a ledger would cross  | host-native seats; `PluginKey` ledger    | host    |
|  [07]   | `CustomObjectGrips.RegisterGripsEnabler`   | re-register replaces per grips guid  | one enabler per `[Guid]` grips type      | host    |
|  [08]   | `AssemblyResolver` search mutations        | additive process list, unremovable   | rows via `HostAssemblies.Extend`         | fan     |
|  [09]   | `AppSettings.Commit` static families       | last-writer-wins process mutation    | `AppSettings.Mount` writer seat          | single  |
|  [10]   | `MarshalLatency` seat                      | a second provider splits the ledger  | first-mount-wins; detacher returns it    | single  |
|  [11]   | `MountRegistry` mounts                     | divergent bindings fork discovery    | one seated binding; divergence faults    | fan     |
|  [12]   | `RhinoDoc.AddCustomUndoEvent`              | handler graph retained until cleared | record-scoped; no host detach exists     | host    |

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct PluginKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Plugin identity is empty.") : null;

    internal static Option<PluginKey> Maybe(Guid value) =>
        Optional(value).Filter(static id => id != Guid.Empty).Map(Create);

    internal Fin<Unit> Admit(Op op) {
        ValidationError? fault = Validate(value: ToValue(), provider: null, out PluginKey? admitted);
        return op.AcceptValidated<PluginKey>(fault: fault, admitted: admitted).Map(static _ => unit);
    }
}

// Modality rows are the kernel HookModality vocabulary; a point may admit more than one row.
[SmartEnum<string>]
public sealed partial class RhinoPoint {
    public static readonly RhinoPoint DocumentLifecycle = new(key: "rasm.rhino.document.lifecycle", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DocumentStructure = new(key: "rasm.rhino.document.structure", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DocumentSelection = new(key: "rasm.rhino.document.selection", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DocumentTables = new(key: "rasm.rhino.document.tables", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DocumentScreen = new(key: "rasm.rhino.document.screen", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DocumentDraw = new(key: "rasm.rhino.document.draw", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DocumentPanels = new(key: "rasm.rhino.document.panels", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DocumentFile = new(key: "rasm.rhino.document.file", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DisplayPointer = new(key: "rasm.rhino.display.pointer", modalities: Seq(HookModality.Observe, HookModality.Veto));
    public static readonly RhinoPoint DisplayWidget = new(key: "rasm.rhino.display.widget", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint DisplayCull = new(key: "rasm.rhino.display.cull", modalities: Seq(HookModality.Veto));
    public static readonly RhinoPoint DisplayDrawObject = new(key: "rasm.rhino.display.drawobject", modalities: Seq(HookModality.Veto));
    public static readonly RhinoPoint ObjectsViewable = new(key: "rasm.rhino.objects.viewable", modalities: Seq(HookModality.Veto));
    public static readonly RhinoPoint ObjectsPick = new(key: "rasm.rhino.objects.pick", modalities: Seq(HookModality.Veto));
    public static readonly RhinoPoint ObjectsRegrow = new(key: "rasm.rhino.objects.regrow", modalities: Seq(HookModality.Veto));
    public static readonly RhinoPoint ObjectsFault = new(key: "rasm.rhino.objects.fault", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint HostException = new(key: "rasm.rhino.host.exception", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint HostCloudLog = new(key: "rasm.rhino.host.log", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint HostUiPanel = new(key: "rasm.rhino.hostui.panel", modalities: Seq(HookModality.Observe, HookModality.Replay));
    public static readonly RhinoPoint HostUiSkin = new(key: "rasm.rhino.hostui.skin", modalities: Seq(HookModality.Observe));
    public static readonly RhinoPoint RenderContent = new(key: "rasm.rhino.render.content", modalities: Seq(HookModality.Observe));

    public Seq<HookModality> Modalities { get; }

    public bool Admits(HookModality modality) => Modalities.Contains(modality);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record HookMount(
    RhinoPoint Point,
    PluginKey Plugin,
    Type Ask,
    Type Grant,
    Func<object, Fin<object>> Bind);

// --- [SERVICES] ---------------------------------------------------------------------------
// One seated binding per point, keyed riders per plugin: the decision rides in the swapped value — the swap
// returns the verdict case, so a losing writer never proceeds on a seat it did not win.
internal sealed record PointSeat(HookMount Binding, HashMap<Guid, Unit> Riders);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record SeatVerdict {
    private SeatVerdict() { }
    internal sealed record Seated : SeatVerdict;
    internal sealed record Riding : SeatVerdict;
    internal sealed record DuplicateRider : SeatVerdict;
    internal sealed record DivergentBinding : SeatVerdict;
}

public static class MountRegistry {
    private static readonly Atom<(HashMap<string, PointSeat> Seats, SeatVerdict? Last)> Mounts =
        Atom((HashMap<string, PointSeat>(), (SeatVerdict?)null));

    public static Seq<HookMount> Census => toSeq(Mounts.Value.Seats).Map(static row => row.Value.Binding);

    public static Seq<(RhinoPoint Point, Seq<PluginKey> Riders)> RiderCensus => toSeq(Mounts.Value.Seats)
        .Map(static row => (row.Value.Binding.Point, toSeq(row.Value.Riders.Keys).Map(PluginKey.Create).Strict()));

    public static Fin<IDisposable> Mount(HookMount mount, Op? key = null) {
        Op op = key.OrDefault();
        return from row in op.Need(mount)
               from _ in guard(
                   row.Point is not null && row.Ask is not null && row.Grant is not null && row.Bind is not null,
                   op.InvalidInput()).ToFin()
               from __ in row.Plugin.Admit(op)
               let plugin = row.Plugin.ToValue()
               let swapped = Mounts.Swap(held => held.Seats.Find(row.Point.Key).Match(
                   None: () => (held.Seats.Add(row.Point.Key, new PointSeat(
                       Binding: row,
                       Riders: HashMap<Guid, Unit>().Add(plugin, unit))), (SeatVerdict?)new SeatVerdict.Seated()),
                   Some: seat => seat.Binding.Ask != row.Ask || seat.Binding.Grant != row.Grant
                       ? (held.Seats, new SeatVerdict.DivergentBinding())
                       : seat.Riders.ContainsKey(plugin)
                           ? (held.Seats, new SeatVerdict.DuplicateRider())
                           : (held.Seats.SetItem(row.Point.Key, seat with { Riders = seat.Riders.Add(plugin, unit) }),
                              new SeatVerdict.Riding())))
               from ___ in swapped.Last switch {
                   SeatVerdict.Seated or SeatVerdict.Riding => Fin.Succ(value: unit),
                   SeatVerdict.DuplicateRider => Fin.Fail<Unit>(error: op.InvalidContext()),
                   _ => Fin.Fail<Unit>(error: op.Unsupported()),
               }
               select (IDisposable)Subscription.Of(detach: () => ignore(Mounts.Swap(held =>
                   held.Seats.Find(row.Point.Key).Match(
                       None: () => held,
                       Some: seat => {
                           HashMap<Guid, Unit> remaining = seat.Riders.Remove(plugin);
                           return (remaining.IsEmpty
                               ? held.Seats.Remove(row.Point.Key)
                               : held.Seats.SetItem(row.Point.Key, seat with { Riders = remaining }), held.Last);
                       }))));
    }

    public static Fin<Seq<IDisposable>> MountAll(Seq<Func<Fin<IDisposable>>> mounts, Op? key = null) {
        Op op = key.OrDefault();
        return mounts.FoldM<Fin, Seq<IDisposable>>(
            Seq<IDisposable>(),
            (held, mount) => Optional(mount)
                .ToFin(Fail: op.InvalidInput())
                .Bind(run => op.Catch(run))
                .Map(seat => held.Add(seat))
                .MapFail(error => Rollback(held: held, primary: error, op: op)));
    }

    public static Fin<TGrant> Bind<TAsk, TGrant>(RhinoPoint point, TAsk ask, Op? key = null)
        where TAsk : notnull
        where TGrant : class {
        Op op = key.OrDefault();
        return from active in op.Need(point)
               from mount in Mounts.Value.Seats.Find(active.Key).Map(static seat => seat.Binding).ToFin(Fail: op.MissingContext())
               from _ in guard(
                   mount.Ask.IsAssignableFrom(typeof(TAsk)) && typeof(TGrant).IsAssignableFrom(mount.Grant),
                   op.Unsupported(geometryType: typeof(TAsk), outputType: typeof(TGrant))).ToFin()
               from granted in mount.Bind(ask)
               from grant in Optional(granted as TGrant).ToFin(Fail: op.InvalidResult())
               select grant;
    }

    private static Error Rollback(Seq<IDisposable> held, Error primary, Op op) =>
        held.Rev().Fold(primary, (faults, seat) => op.Catch(() => {
            seat.Dispose();
            return Fin.Succ(value: unit);
        }).Match(
            Succ: _ => faults,
            Fail: cleanup => faults + cleanup));
}

public static class DocumentHooks {
    public static Fin<Seq<IDisposable>> Mount(PluginKey plugin, Op? key = null) {
        Op op = key.OrDefault();
        Seq<Func<Fin<IDisposable>>> mounts = Seq(
                (Point: RhinoPoint.DocumentLifecycle, Band: EventBand.Lifecycle),
                (Point: RhinoPoint.DocumentStructure, Band: EventBand.Structure),
                (Point: RhinoPoint.DocumentSelection, Band: EventBand.Selection),
                (Point: RhinoPoint.DocumentTables, Band: EventBand.Tables),
                (Point: RhinoPoint.DocumentScreen, Band: EventBand.Screen),
                (Point: RhinoPoint.DocumentDraw, Band: EventBand.Draw),
                (Point: RhinoPoint.DocumentPanels, Band: EventBand.Panels))
            .Map(row => (Func<Fin<IDisposable>>)(() => MountRegistry.Mount(
                mount: new HookMount(
                    Point: row.Point,
                    Plugin: plugin,
                    Ask: typeof(Observation.Host),
                    Grant: typeof(Watch),
                    Bind: ask => EventFamily.In(band: row.Band, key: op)
                        .Bind(families => DocumentStream.Observe(((Observation.Host)ask) with { Families = families })
                            .Map(static watch => (object)watch))),
                key: op)))
            .Add(() => MountRegistry.Mount(
                mount: new HookMount(
                    Point: RhinoPoint.DocumentFile,
                    Plugin: plugin,
                    Ask: typeof(Observation.File),
                    Grant: typeof(Watch),
                    Bind: static ask => DocumentStream.Observe((Observation.File)ask).Map(static watch => (object)watch)),
                key: op));
        return MountRegistry.MountAll(mounts: mounts, key: op);
    }
}
```

## [07]-[TELEMETRY_TAP]

- Owner: `RhinoInstruments` — the boundary's contributed instrument rows in the kernel `InstrumentSpec` shape and the string-scoped `TelemetryContributorPort` mint under scope `Rasm.Rhino`.
- Cases: stream-loss counts off the `StreamReceipt.PacedLoss` journal evidence by lane and loss kind; delivered document facts by band off each mounted watch; host exception and cloud-log observations off the two `HostTap.Mount` points.
- Entry: `RhinoInstruments.Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl)` — the one contributor port an app composition merges by scope, its coordinate defaulting to the branch semconv pin; the plugin root materializes the rows over its own per-ALC factory meter through `InstrumentSet.Of`, and one custody per composition holds — either the port rides an app fan or the root materializes locally, never both.
- Auto: writes ride observe taps composed at the plugin root — a watch's receipt journal feeds the loss counter, the delivery sink feeds the band counter, and the host-tap points feed the two observation counters — so no stream, projection, or mount fence carries a meter call.
- Packages: `Rasm` (kernel signal capsule), BCL inbox (`System.Diagnostics.Metrics`).
- Growth: one measured boundary concern is one `InstrumentSpec` factory call here and one observe-tap write at the plugin root.
- Boundary: rows carry dotted `rasm.rhino.*` names with UCUM units and closed dimensions; instrument execution over these declarations is app-root altitude, never a second measurement truth inside the boundary, and provider custody stays with the per-ALC factory owner.

```csharp signature
// --- [TABLES] -----------------------------------------------------------------------------
public static class RhinoInstruments {
    public const string Scope = "Rasm.Rhino";
    public const string StreamLoss = "rasm.rhino.stream.loss";
    public const string DocumentEvents = "rasm.rhino.document.events";
    public const string HostExceptions = "rasm.rhino.host.exceptions";
    public const string HostLogs = "rasm.rhino.host.logs";

    public const string LaneSlot = "lane";
    public const string LossSlot = "loss";
    public const string BandSlot = "band";

    public static readonly Seq<InstrumentSpec> Rows = Seq(
        InstrumentSpec.Count(StreamLoss, "{fact}", "paced-lane facts shed by lane and loss kind", MeasureForm.Whole, LaneSlot, LossSlot),
        InstrumentSpec.Count(DocumentEvents, "{event}", "delivered document facts by band", MeasureForm.Whole, BandSlot),
        InstrumentSpec.Count(HostExceptions, "{exception}", "host exception reports observed through the host tap", MeasureForm.Whole),
        InstrumentSpec.Count(HostLogs, "{message}", "host cloud-log messages observed through the host tap", MeasureForm.Whole));

    public static TelemetryContributorPort Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl) =>
        new(Scope: Scope, Version: version, Instruments: Rows, Classifications: HostSensitivity.Values, SchemaUrl: schemaUrl);
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
