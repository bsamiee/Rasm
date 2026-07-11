# [RASM_RHINO_EVENTS]

The document observation stream (`Rasm.Rhino.Document`). `EventFamily` binds the document, object, selection, component-table, view, page, display, draw, and panel event surface into detached facts. Every row declares its band, cadence, and typed binding policy; every band projection derives from those columns. `DocumentStream` admits one host or file observation, attaches the whole handler set transactionally, schedules delivery, bounds delivery evidence, and owns symmetric detachment. Callback-scoped native handles never cross the bind.

## [01]-[INDEX]

- [02]-[FAMILY_ROSTER]: `EventBand`, `Cadence`, and `EventFamily` rows with derived band projection.
- [03]-[PAYLOAD_PROJECTION]: detached `EventPayload` cases and the `DocEvent` carrier.
- [04]-[DELIVERY_POLICY]: `StreamLane`, `Delivery`, and bounded `StreamReceipt` evidence.
- [05]-[STREAM_OWNER]: the `Observation` source family, `DocumentStream`, `Watch`, and subscription capsule.
- [06]-[SURFACE_LEDGER]: the page owner table.

## [02]-[FAMILY_ROSTER]

- Owner: `EventBand` is the grouping vocabulary. `Cadence` owns delivery admission. `EventFamily` is the keyed host-event table; each row carries both columns and one typed bind delegate.
- Law: `EventFamily.In(EventBand)` admits the band and derives every group from `EventFamily.Items`. A new family is one row, and no secondary roster can drift.
- Law: draw rows carry only a viewport id, change counter, and optional parent-view serial. The drawing owner receives and uses `DisplayPipeline` inside its own callback; an observation never dereferences a draw handle after callback return.
- Law: the before-transform row detaches the transform and object identities while the native pointers are live. The after-transform row correlates its sole host field, `TransformEventId`, through a private before-transform listener and never invents missing document evidence.
- Law: `ProjectionChanged` suppresses a repeated `(document, viewport, change-counter)` product at the bind. Draw rows remain unsuppressed and rely on an admitted dropping lane.
- Boundary: table events carry stable table kind, index, transition, and detached prior/current component facts. Full live state resolves through `DocKey` at consumption time; callback-owned component handles never leave the callback.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO;
using System.Threading;
using System.Threading.Channels;
using Rasm.Domain;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;

namespace Rasm.Rhino.Document;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EventBand {
    public static readonly EventBand Lifecycle = new(key: 0);
    public static readonly EventBand Structure = new(key: 1);
    public static readonly EventBand Selection = new(key: 2);
    public static readonly EventBand Tables = new(key: 3);
    public static readonly EventBand Screen = new(key: 4);
    public static readonly EventBand Draw = new(key: 5);
    public static readonly EventBand Panels = new(key: 6);
}

[SmartEnum]
public sealed partial class Cadence {
    public static readonly Cadence Changed = new(static _ => true);
    public static readonly Cadence PerFrame = new(static delivery => delivery is Delivery.Paced paced && paced.Lane.Dropping);

    [UseDelegateFromConstructor]
    public partial bool Admits(Delivery delivery);
}

[SmartEnum<int>]
public sealed partial class EventFamily {
    public static readonly EventFamily BeginOpen = new(key: 0, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentOpenEventArgs>(
        subscribe: h => RhinoDoc.BeginOpenDocument += h,
        unsubscribe: h => RhinoDoc.BeginOpenDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Opened.Of(a))));
    public static readonly EventFamily EndOpen = new(key: 1, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentOpenEventArgs>(
        subscribe: h => RhinoDoc.EndOpenDocument += h,
        unsubscribe: h => RhinoDoc.EndOpenDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Opened.Of(a))));
    public static readonly EventFamily ViewSettled = new(key: 2, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentOpenEventArgs>(
        subscribe: h => RhinoDoc.EndOpenDocumentInitialViewUpdate += h,
        unsubscribe: h => RhinoDoc.EndOpenDocumentInitialViewUpdate -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Opened.Of(a))));
    public static readonly EventFamily BeginSave = new(key: 3, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentSaveEventArgs>(
        subscribe: h => RhinoDoc.BeginSaveDocument += h,
        unsubscribe: h => RhinoDoc.BeginSaveDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Saved.Of(a))));
    public static readonly EventFamily EndSave = new(key: 4, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentSaveEventArgs>(
        subscribe: h => RhinoDoc.EndSaveDocument += h,
        unsubscribe: h => RhinoDoc.EndSaveDocument -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: EventPayload.Saved.Of(a))));
    public static readonly EventFamily Closed = new(key: 5, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: Signal(
        subscribe: h => RhinoDoc.CloseDocument += h, unsubscribe: h => RhinoDoc.CloseDocument -= h));
    public static readonly EventFamily Created = new(key: 6, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: Signal(
        subscribe: h => RhinoDoc.NewDocument += h, unsubscribe: h => RhinoDoc.NewDocument -= h));
    public static readonly EventFamily ActiveChanged = new(key: 7, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<DocumentEventArgs>(
        subscribe: h => RhinoDoc.ActiveDocumentChanged += h,
        unsubscribe: h => RhinoDoc.ActiveDocumentChanged -= h,
        project: static (_, a, scope) => GateActive(serial: a.DocumentSerialNumber, scope: scope)));
    public static readonly EventFamily PropertiesChanged = new(key: 8, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: Signal(
        subscribe: h => RhinoDoc.DocumentPropertiesChanged += h, unsubscribe: h => RhinoDoc.DocumentPropertiesChanged -= h));
    public static readonly EventFamily UnitsChanged = new(key: 9, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<UnitsChangedWithScalingEventArgs>(
        subscribe: h => RhinoDoc.UnitsChangedWithScaling += h,
        unsubscribe: h => RhinoDoc.UnitsChangedWithScaling -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: new EventPayload.UnitsScaled(Scale: a.Scale))));
    public static readonly EventFamily UserStringChanged = new(key: 10, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<RhinoDoc.UserStringChangedArgs>(
        subscribe: h => RhinoDoc.UserStringChanged += h,
        unsubscribe: h => RhinoDoc.UserStringChanged -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.UserString(Key: a.Key))));
    public static readonly EventFamily WorksessionFile = new(key: 11, band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: OnFallible<RhinoDoc.WorksessionFileChangedEventArgs>(
        subscribe: h => RhinoDoc.WorksessionFileChanged += h,
        unsubscribe: h => RhinoDoc.WorksessionFileChanged -= h,
        project: static (_, a, scope) => WorksessionChange.Of(a.ChangeKind).Map(change => Gate(
            document: a.Document,
            scope: scope,
            payload: new EventPayload.Worksession(ModelSerial: a.WorksessionModelRuntimeSerialNumber, File: a.FilePath, Change: change)))));

    public static readonly EventFamily ObjectAdded = new(key: 20, band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.AddRhinoObject += h, unsubscribe: h => RhinoDoc.AddRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily ObjectDeleted = new(key: 21, band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.DeleteRhinoObject += h, unsubscribe: h => RhinoDoc.DeleteRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily ObjectReplaced = new(key: 22, band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoReplaceObjectEventArgs>(
        subscribe: h => RhinoDoc.ReplaceRhinoObject += h,
        unsubscribe: h => RhinoDoc.ReplaceRhinoObject -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.Replaced(
            Old: a.ObjectId, New: Optional(a.NewRhinoObject).Map(static o => o.Id).Filter(static id => id != Guid.Empty)))));
    public static readonly EventFamily ObjectUndeleted = new(key: 23, band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.UndeleteRhinoObject += h, unsubscribe: h => RhinoDoc.UndeleteRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily ObjectPurged = new(key: 24, band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoObjectEventArgs>(
        subscribe: h => RhinoDoc.PurgeRhinoObject += h, unsubscribe: h => RhinoDoc.PurgeRhinoObject -= h, project: ObjectFact));
    public static readonly EventFamily AttributesAmended = new(key: 25, band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoModifyObjectAttributesEventArgs>(
        subscribe: h => RhinoDoc.ModifyObjectAttributes += h,
        unsubscribe: h => RhinoDoc.ModifyObjectAttributes -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.Attributes(
            Object: Optional(a.RhinoObject).Map(static o => o.Id).Filter(static id => id != Guid.Empty)))));
    public static readonly EventFamily BeforeTransform = new(key: 26, band: EventBand.Structure, cadence: Cadence.Changed, bind: On<RhinoTransformObjectsEventArgs>(
        subscribe: h => RhinoDoc.BeforeTransformObjects += h,
        unsubscribe: h => RhinoDoc.BeforeTransformObjects -= h,
        project: TransformFact));
    public static readonly EventFamily AfterTransform = new(key: 27, band: EventBand.Structure, cadence: Cadence.Changed, bind: AfterTransformFact());

    public static readonly EventFamily SelectionAdded = new(key: 40, band: EventBand.Selection, cadence: Cadence.Changed, bind: On<RhinoObjectSelectionEventArgs>(
        subscribe: h => RhinoDoc.SelectObjects += h, unsubscribe: h => RhinoDoc.SelectObjects -= h, project: SelectionFact));
    public static readonly EventFamily SelectionRemoved = new(key: 41, band: EventBand.Selection, cadence: Cadence.Changed, bind: On<RhinoObjectSelectionEventArgs>(
        subscribe: h => RhinoDoc.DeselectObjects += h, unsubscribe: h => RhinoDoc.DeselectObjects -= h, project: SelectionFact));
    public static readonly EventFamily SelectionCleared = new(key: 42, band: EventBand.Selection, cadence: Cadence.Changed, bind: On<RhinoDeselectAllObjectsEventArgs>(
        subscribe: h => RhinoDoc.DeselectAllObjects += h,
        unsubscribe: h => RhinoDoc.DeselectAllObjects -= h,
        project: static (_, a, scope) => Gate(document: a.Document, scope: scope, payload: new EventPayload.Selection(Ids: Seq<Guid>(), Count: a.ObjectCount))));

    public static readonly EventFamily LayerTable = new(key: 60, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<LayerTableEventArgs>(
        subscribe: h => RhinoDoc.LayerTableEvent += h, unsubscribe: h => RhinoDoc.LayerTableEvent -= h, kind: TableKind.Layers,
        document: static a => a.Document, index: static a => a.LayerIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily MaterialTable = new(key: 61, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<MaterialTableEventArgs>(
        subscribe: h => RhinoDoc.MaterialTableEvent += h, unsubscribe: h => RhinoDoc.MaterialTableEvent -= h, kind: TableKind.Materials,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldSettings, current: static a => a.Document.Materials[a.Index]));
    public static readonly EventFamily GroupTable = new(key: 62, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<GroupTableEventArgs>(
        subscribe: h => RhinoDoc.GroupTableEvent += h, unsubscribe: h => RhinoDoc.GroupTableEvent -= h, kind: TableKind.Groups,
        document: static a => a.Document, index: static a => a.GroupIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily LinetypeTable = new(key: 63, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<LinetypeTableEventArgs>(
        subscribe: h => RhinoDoc.LinetypeTableEvent += h, unsubscribe: h => RhinoDoc.LinetypeTableEvent -= h, kind: TableKind.Linetypes,
        document: static a => a.Document, index: static a => a.LinetypeIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily LightTable = new(key: 64, band: EventBand.Tables, cadence: Cadence.Changed, bind: OnFallible<LightTableEventArgs>(
        subscribe: h => RhinoDoc.LightTableEvent += h,
        unsubscribe: h => RhinoDoc.LightTableEvent -= h,
        project: static (_, a, scope) => ComponentTransition.Of(a.EventType).Map(transition => Gate(
            document: a.Document,
            scope: scope,
            payload: new EventPayload.Component(
                Kind: TableKind.Lights,
                Index: a.LightIndex,
                Transition: transition,
                Previous: transition.CarriesPrevious ? ComponentState.Of(a.OldState) : Option<ComponentState>.None,
                Current: transition.CarriesCurrent ? ComponentState.Of(a.NewState) : Option<ComponentState>.None)))));
    public static readonly EventFamily DimensionStyleTable = new(key: 65, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<DimStyleTableEventArgs>(
        subscribe: h => RhinoDoc.DimensionStyleTableEvent += h, unsubscribe: h => RhinoDoc.DimensionStyleTableEvent -= h, kind: TableKind.DimStyles,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily InstanceDefinitionTable = new(key: 66, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<InstanceDefinitionTableEventArgs>(
        subscribe: h => RhinoDoc.InstanceDefinitionTableEvent += h, unsubscribe: h => RhinoDoc.InstanceDefinitionTableEvent -= h, kind: TableKind.InstanceDefinitions,
        document: static a => a.Document, index: static a => a.InstanceDefinitionIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily SectionStyleTable = new(key: 67, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<SectionStyleTableEventArgs>(
        subscribe: h => RhinoDoc.SectionStyleTableEvent += h, unsubscribe: h => RhinoDoc.SectionStyleTableEvent -= h, kind: TableKind.SectionStyles,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily MarkupTable = new(key: 68, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<MarkupTableEventArgs>(
        subscribe: h => RhinoDoc.MarkupTableEvent += h, unsubscribe: h => RhinoDoc.MarkupTableEvent -= h, kind: TableKind.Markups,
        document: static a => a.Document, index: static a => a.Index, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily PageViewGroupTable = new(key: 69, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<PageViewGroupTableEventArgs>(
        subscribe: h => RhinoDoc.PageViewGroupTableEvent += h, unsubscribe: h => RhinoDoc.PageViewGroupTableEvent -= h, kind: TableKind.PageViewGroups,
        document: static a => a.Document, index: static a => a.PageViewGroupIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily HatchPatternTable = new(key: 70, band: EventBand.Tables, cadence: Cadence.Changed, bind: Table<HatchPatternTableEventArgs>(
        subscribe: h => RhinoDoc.HatchPatternTableEvent += h, unsubscribe: h => RhinoDoc.HatchPatternTableEvent -= h, kind: TableKind.HatchPatterns,
        document: static a => a.Document, index: static a => a.HatchPatternIndex, transition: static a => ComponentTransition.Of(a.EventType),
        previous: static a => a.OldState, current: static a => a.NewState));
    public static readonly EventFamily RenderMaterialTable = new(key: 71, band: EventBand.Tables, cadence: Cadence.Changed, bind: Render(
        subscribe: h => RhinoDoc.RenderMaterialsTableEvent += h, unsubscribe: h => RhinoDoc.RenderMaterialsTableEvent -= h, kind: TableKind.RenderMaterials));
    public static readonly EventFamily RenderEnvironmentTable = new(key: 72, band: EventBand.Tables, cadence: Cadence.Changed, bind: Render(
        subscribe: h => RhinoDoc.RenderEnvironmentTableEvent += h, unsubscribe: h => RhinoDoc.RenderEnvironmentTableEvent -= h, kind: TableKind.RenderEnvironments));
    public static readonly EventFamily RenderTextureTable = new(key: 73, band: EventBand.Tables, cadence: Cadence.Changed, bind: Render(
        subscribe: h => RhinoDoc.RenderTextureTableEvent += h, unsubscribe: h => RhinoDoc.RenderTextureTableEvent -= h, kind: TableKind.RenderTextures));
    public static readonly EventFamily TextureMappingTable = new(key: 74, band: EventBand.Tables, cadence: Cadence.Changed, bind: OnFallible<RhinoDoc.TextureMappingEventArgs>(
        subscribe: h => RhinoDoc.TextureMappingEvent += h,
        unsubscribe: h => RhinoDoc.TextureMappingEvent -= h,
        project: static (_, a, scope) => ComponentTransition.Of(a.EventType).Map(transition => Gate(
            document: a.Document,
            scope: scope,
            payload: new EventPayload.TextureMapping(
                Transition: transition,
                Current: transition.CarriesCurrent ? Optional(a.NewMapping).Map(static mapping => mapping.Id) : Option<Guid>.None))));

    public static readonly EventFamily ViewModified = new(key: 80, band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Modified += h, unsubscribe: h => RhinoView.Modified -= h));
    public static readonly EventFamily ViewCreated = new(key: 81, band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Create += h, unsubscribe: h => RhinoView.Create -= h));
    public static readonly EventFamily ViewDestroyed = new(key: 82, band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Destroy += h, unsubscribe: h => RhinoView.Destroy -= h));
    public static readonly EventFamily ViewActivated = new(key: 83, band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.SetActive += h, unsubscribe: h => RhinoView.SetActive -= h));
    public static readonly EventFamily ViewRenamed = new(key: 84, band: EventBand.Screen, cadence: Cadence.Changed, bind: ViewFact(
        subscribe: h => RhinoView.Rename += h, unsubscribe: h => RhinoView.Rename -= h));
    public static readonly EventFamily ViewDrawingChanged = new(key: 85, band: EventBand.Screen, cadence: Cadence.Changed, bind: On<ViewEnableDrawingEventArgs>(
        subscribe: h => RhinoView.EnableDrawingChanged += h,
        unsubscribe: h => RhinoView.EnableDrawingChanged -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: new EventPayload.Drawing(Enabled: a.DrawingEnabled))));
    public static readonly EventFamily PageSpaceChanged = new(key: 90, band: EventBand.Screen, cadence: Cadence.Changed, bind: On<PageViewSpaceChangeEventArgs>(
        subscribe: h => RhinoPageView.PageViewSpaceChange += h,
        unsubscribe: h => RhinoPageView.PageViewSpaceChange -= h,
        project: static (_, a, scope) => Optional(a.PageView).Bind(page => Gate(document: page.Document, scope: scope, payload: new EventPayload.Page(
            PageSerial: page.RuntimeSerialNumber,
            OldDetail: Optional(a.OldActiveDetailId).Filter(static id => id != Guid.Empty),
            NewDetail: Optional(a.NewActiveDetailId).Filter(static id => id != Guid.Empty))))));
    public static readonly EventFamily PagePropertiesChanged = new(key: 91, band: EventBand.Screen, cadence: Cadence.Changed, bind: On<PageViewPropertiesChangeEventArgs>(
        subscribe: h => RhinoPageView.PageViewPropertiesChange += h,
        unsubscribe: h => RhinoPageView.PageViewPropertiesChange -= h,
        project: static (_, a, scope) => Gate(serial: a.DocumentSerialNumber, scope: scope, payload: new EventPayload.Page(
            PageSerial: a.PageViewSerialNumber, OldDetail: Option<Guid>.None, NewDetail: Option<Guid>.None))));
    public static readonly EventFamily ProjectionChanged = new(key: 100, band: EventBand.Screen, cadence: Cadence.Changed, bind: ProjectionFact(
        subscribe: h => DisplayPipeline.ViewportProjectionChanged += h, unsubscribe: h => DisplayPipeline.ViewportProjectionChanged -= h));
    public static readonly EventFamily DisplayModeChanged = new(key: 101, band: EventBand.Screen, cadence: Cadence.Changed, bind: On<DisplayModeChangedEventArgs>(
        subscribe: h => DisplayPipeline.DisplayModeChanged += h,
        unsubscribe: h => DisplayPipeline.DisplayModeChanged -= h,
        project: static (_, a, scope) => Optional(a.Viewport).Bind(viewport => Gate(document: a.RhinoDoc, scope: scope, payload: new EventPayload.DisplayMode(
            ViewportId: viewport.Id, Old: a.OldDisplayModeId, Next: a.ChangedDisplayModeId)))));

    public static readonly EventFamily DrawForeground = new(key: 110, band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.DrawForeground += h, unsubscribe: h => DisplayPipeline.DrawForeground -= h));
    public static readonly EventFamily DrawOverlay = new(key: 111, band: EventBand.Draw, cadence: Cadence.PerFrame, bind: DrawFact(
        subscribe: h => DisplayPipeline.DrawOverlay += h, unsubscribe: h => DisplayPipeline.DrawOverlay -= h));

    public static readonly EventFamily PanelVisibility = new(key: 120, band: EventBand.Panels, cadence: Cadence.Changed, bind: On<global::Rhino.UI.ShowPanelEventArgs>(
        subscribe: h => global::Rhino.UI.Panels.Show += h,
        unsubscribe: h => global::Rhino.UI.Panels.Show -= h,
        project: static (_, a, scope) => Gate(
            serial: a.DocumentSerialNumber,
            scope: scope,
            payload: new EventPayload.Panel(PanelId: a.PanelId, State: a.Show ? PanelState.Shown : PanelState.Hidden))));
    public static readonly EventFamily PanelClosed = new(key: 121, band: EventBand.Panels, cadence: Cadence.Changed, bind: On<global::Rhino.UI.PanelEventArgs>(
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
            EventHandler<TArgs> handler = (sender, args) => ignore(
                Op.Of(name: nameof(EventFamily)).Catch(() => project(sender, args, scope).Bind(projected => projected.Match(
                    Some: deliver, None: static () => Fin.Succ(value: unit)))).MapFail(error => { reject(obj: error); return error; }));
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
        Func<TArgs, ModelComponent?> previous,
        Func<TArgs, ModelComponent?> current) where TArgs : EventArgs =>
        OnFallible(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, args, scope) => transition(arg: args).Map(change => Gate(
                document: document(arg: args),
                scope: scope,
                payload: new EventPayload.Component(
                    Kind: kind,
                    Index: index(arg: args),
                    Transition: change,
                    Previous: change.CarriesPrevious ? ComponentState.Of(previous(arg: args)) : Option<ComponentState>.None,
                    Current: change.CarriesCurrent ? ComponentState.Of(current(arg: args)) : Option<ComponentState>.None))));

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

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> DrawFact(
        Action<EventHandler<DrawEventArgs>> subscribe,
        Action<EventHandler<DrawEventArgs>> unsubscribe) =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: static (_, a, scope) => Optional(a.Viewport).Bind(viewport =>
            Gate(document: a.RhinoDoc, scope: scope, payload: new EventPayload.Frame(
                ViewportId: viewport.Id,
                ChangeCounter: viewport.ChangeCounter,
                ViewSerial: Optional(viewport.ParentView).Map(static view => view.RuntimeSerialNumber)))));

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> ProjectionFact(
        Action<EventHandler<DrawEventArgs>> subscribe,
        Action<EventHandler<DrawEventArgs>> unsubscribe) =>
        (scope, journal, deliver, reject) => {
            System.Collections.Concurrent.ConcurrentDictionary<(Guid Viewport, uint Document), uint> seen = new();
            return On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, a, watched) =>
                Optional(a.RhinoDoc).Bind(document => Optional(a.Viewport).Bind(viewport => {
                    (Guid Viewport, uint Document) key = (viewport.Id, document.RuntimeSerialNumber);
                    uint counter = viewport.ChangeCounter;
                    return Advance(seen: seen, key: key, counter: counter, capacity: journal.Policy.CorrelationCapacity)
                        ? Gate(document: document, scope: watched, payload: new EventPayload.Projection(ViewportId: viewport.Id, ChangeCounter: counter))
                        : Option<EventEnvelope>.None;
                })))(scope, journal, deliver, reject);
        };

    private static Func<EventScope, ReceiptJournal, Func<EventEnvelope, Fin<Unit>>, Action<Error>, Fin<Subscription>> AfterTransformFact() =>
        (scope, journal, deliver, reject) => {
            System.Collections.Concurrent.ConcurrentDictionary<uint, TransformLink> documents = new();
            long sequence = 0;
            EventHandler<RhinoTransformObjectsEventArgs> before = (_, args) => ignore(
                Op.Of(name: nameof(BeforeTransform)).Catch(() => Fin.Succ(value: TransformDocument(args).Map(key => Op.Side(() => {
                    documents[key: args.TransformEventId] = new TransformLink(Key: key, Sequence: Interlocked.Increment(ref sequence));
                    while (documents.Count > journal.Policy.CorrelationCapacity) {
                        KeyValuePair<uint, TransformLink>[] oldest = documents.OrderBy(static pair => pair.Value.Sequence).Take(1).ToArray();
                        if (oldest.Length == 0 || !documents.TryRemove(key: oldest[0].Key, value: out _)) {
                            break;
                        }
                        _ = journal.Post(new StreamReceipt.TransformEvicted(Watch: journal.Watch, EventId: oldest[0].Key));
                    }
                })))).MapFail(error => { reject(obj: error); return error; }));
            EventHandler<RhinoAfterTransformObjectsEventArgs> after = (_, args) => ignore(
                Op.Of(name: nameof(AfterTransform)).Catch(() => {
                    Option<EventEnvelope> envelope;
                    if (documents.TryRemove(key: args.TransformEventId, value: out TransformLink link)) {
                        envelope = Gate(key: link.Key, scope: scope, payload: new EventPayload.TransformEnded(EventId: args.TransformEventId));
                    } else {
                        _ = journal.Post(new StreamReceipt.TransformUnmatched(Watch: journal.Watch, EventId: args.TransformEventId));
                        envelope = Unkeyed(scope: scope, payload: new EventPayload.TransformEnded(EventId: args.TransformEventId));
                    }
                    return envelope.Match(Some: deliver, None: static () => Fin.Succ(value: unit));
                }).MapFail(error => { reject(obj: error); return error; }));
            return Subscription.AttachAll(Seq<Func<Fin<Subscription>>>(
                () => Subscription.Attach(
                    subscribe: h => RhinoDoc.BeforeTransformObjects += h,
                    unsubscribe: h => RhinoDoc.BeforeTransformObjects -= h,
                    handler: before),
                () => Subscription.Attach(
                    subscribe: h => RhinoDoc.AfterTransformObjects += h,
                    unsubscribe: h => RhinoDoc.AfterTransformObjects -= h,
                    handler: after)));
        };

    private readonly record struct TransformLink(DocKey Key, long Sequence);

    private static Option<EventEnvelope> ObjectFact(object? sender, RhinoObjectEventArgs args, EventScope scope) =>
        Gate(document: (sender as RhinoDoc) ?? args.TheObject?.Document, scope: scope, payload: new EventPayload.Objects(Ids: Seq(args.ObjectId)));

    private static Option<EventEnvelope> SelectionFact(object? sender, RhinoObjectSelectionEventArgs args, EventScope scope) =>
        Gate(document: args.Document, scope: scope, payload: new EventPayload.Selection(
            Ids: toSeq(args.RhinoObjects).Choose(static item => Optional(item).Map(static value => value.Id)),
            Count: args.RhinoObjectCount));

    private static Option<EventEnvelope> TransformFact(object? sender, RhinoTransformObjectsEventArgs args, EventScope scope) =>
        TransformDocument(args).Bind(key => Gate(key: key, scope: scope, payload: new EventPayload.TransformStarted(
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
            .HeadOrNone();

    private static Seq<(Guid Id, uint Serial)> ObjectRefs(IEnumerable<RhinoObject?> objects) =>
        toSeq(objects).Choose(static item => Optional(item).Map(static value => (value.Id, value.RuntimeSerialNumber)));

    private static bool Advance(
        System.Collections.Concurrent.ConcurrentDictionary<(Guid Viewport, uint Document), uint> seen,
        (Guid Viewport, uint Document) key,
        uint counter,
        int capacity) {
        if (seen.Count >= capacity && !seen.ContainsKey(key: key)) {
            seen.Clear();
        }
        while (true) {
            if (!seen.TryGetValue(key: key, value: out uint prior)) {
                if (seen.TryAdd(key: key, value: counter)) {
                    return true;
                }
                continue;
            }
            if (prior == counter) {
                return false;
            }
            if (seen.TryUpdate(key: key, newValue: counter, comparisonValue: prior)) {
                return true;
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
            state: (Key: key, Payload: payload),
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
            state: payload,
            document: static (_, _) => Option<EventEnvelope>.None,
            anyDocument: static (fact, _) => Some(new EventEnvelope(Key: Option<DocKey>.None, Payload: fact)));
}
```

## [03]-[PAYLOAD_PROJECTION]

- Owner: `EventPayload` is the closed detached-evidence family. `EventEnvelope` is the internal projection product; `DocEvent` adds its family row at delivery.
- Law: every reference-like host member is projected inside the callback. Documents become `DocKey`, views and viewports become runtime identities, transform participants become `(Guid, runtime serial)` products, and component states become detached identity/name/deletion facts.
- Law: `Active` preserves the no-active-document transition. `AfterTransform` is the sole conditionally unkeyed host event because its argument contains only `TransformEventId`; an `AnyDocument` observer still receives an unmatched completion.
- Law: render-material assignment preserves its layer-or-object target and old/new material identities; loaded, clearing, and cleared render-table transitions remain distinct typed rows.
- Law: object-id projection is a total generated `Switch`. A new payload case fails the projection at compile time until its contribution is declared.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class WorksessionChange {
    public static readonly WorksessionChange Attached = new(key: 0);
    public static readonly WorksessionChange Detached = new(key: 1);
    public static readonly WorksessionChange BeforeDetach = new(key: 2);

    internal static Fin<WorksessionChange> Of(RhinoDoc.WorksessionFileChangeKind value) => value switch {
        RhinoDoc.WorksessionFileChangeKind.Attached => Attached,
        RhinoDoc.WorksessionFileChangeKind.Detached => Detached,
        RhinoDoc.WorksessionFileChangeKind.BeforeDetach => BeforeDetach,
        _ => Fin.Fail<WorksessionChange>(error: Op.Of(name: nameof(WorksessionChange)).InvalidResult()),
    };
}

[SmartEnum<int>]
public sealed partial class ComponentTransition {
    public static readonly ComponentTransition Added = new(key: 0, carriesPrevious: false, carriesCurrent: true);
    public static readonly ComponentTransition Deleted = new(key: 1, carriesPrevious: true, carriesCurrent: false);
    public static readonly ComponentTransition Undeleted = new(key: 2, carriesPrevious: true, carriesCurrent: true);
    public static readonly ComponentTransition Modified = new(key: 3, carriesPrevious: true, carriesCurrent: true);
    public static readonly ComponentTransition Sorted = new(key: 4, carriesPrevious: false, carriesCurrent: false);
    public static readonly ComponentTransition Current = new(key: 5, carriesPrevious: true, carriesCurrent: true);

    public bool CarriesPrevious { get; }
    public bool CarriesCurrent { get; }

    internal static Fin<ComponentTransition> Of(LayerTableEventType value) => value switch {
        LayerTableEventType.Added => Added, LayerTableEventType.Deleted => Deleted, LayerTableEventType.Undeleted => Undeleted,
        LayerTableEventType.Modified => Modified, LayerTableEventType.Sorted => Sorted, LayerTableEventType.Current => Current,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(MaterialTableEventType value) => value switch {
        MaterialTableEventType.Added => Added, MaterialTableEventType.Deleted => Deleted, MaterialTableEventType.Undeleted => Undeleted,
        MaterialTableEventType.Modified => Modified, MaterialTableEventType.Sorted => Sorted, MaterialTableEventType.Current => Current,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(GroupTableEventType value) => value switch {
        GroupTableEventType.Added => Added, GroupTableEventType.Deleted => Deleted, GroupTableEventType.Undeleted => Undeleted,
        GroupTableEventType.Modified => Modified, GroupTableEventType.Sorted => Sorted,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(LinetypeTableEventType value) => value switch {
        LinetypeTableEventType.Added => Added, LinetypeTableEventType.Deleted => Deleted, LinetypeTableEventType.Undeleted => Undeleted,
        LinetypeTableEventType.Modified => Modified, LinetypeTableEventType.Sorted => Sorted, LinetypeTableEventType.Current => Current,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(LightTableEventType value) => value switch {
        LightTableEventType.Added => Added, LightTableEventType.Deleted => Deleted, LightTableEventType.Undeleted => Undeleted,
        LightTableEventType.Modified => Modified, LightTableEventType.Sorted => Sorted,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(DimStyleTableEventType value) => value switch {
        DimStyleTableEventType.Added => Added, DimStyleTableEventType.Deleted => Deleted, DimStyleTableEventType.Undeleted => Undeleted,
        DimStyleTableEventType.Modified => Modified, DimStyleTableEventType.Sorted => Sorted, DimStyleTableEventType.Current => Current,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(InstanceDefinitionTableEventType value) => value switch {
        InstanceDefinitionTableEventType.Added => Added, InstanceDefinitionTableEventType.Deleted => Deleted,
        InstanceDefinitionTableEventType.Undeleted => Undeleted, InstanceDefinitionTableEventType.Modified => Modified,
        InstanceDefinitionTableEventType.Sorted => Sorted, _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(SectionStyleTableEventType value) => value switch {
        SectionStyleTableEventType.Added => Added, SectionStyleTableEventType.Deleted => Deleted,
        SectionStyleTableEventType.Undeleted => Undeleted, SectionStyleTableEventType.Modified => Modified,
        SectionStyleTableEventType.Sorted => Sorted, _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(MarkupTableEventType value) => value switch {
        MarkupTableEventType.Added => Added, MarkupTableEventType.Deleted => Deleted, MarkupTableEventType.Undeleted => Undeleted,
        MarkupTableEventType.Modified => Modified, MarkupTableEventType.Sorted => Sorted,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(PageViewGroupTableEventType value) => value switch {
        PageViewGroupTableEventType.Added => Added, PageViewGroupTableEventType.Deleted => Deleted,
        PageViewGroupTableEventType.Undeleted => Undeleted, PageViewGroupTableEventType.Modified => Modified,
        PageViewGroupTableEventType.Sorted => Sorted, _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(HatchPatternTableEventType value) => value switch {
        HatchPatternTableEventType.Added => Added, HatchPatternTableEventType.Deleted => Deleted,
        HatchPatternTableEventType.Undeleted => Undeleted, HatchPatternTableEventType.Modified => Modified,
        HatchPatternTableEventType.Sorted => Sorted, HatchPatternTableEventType.Current => Current,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
    internal static Fin<ComponentTransition> Of(RhinoDoc.TextureMappingEventType value) => value switch {
        RhinoDoc.TextureMappingEventType.Added => Added, RhinoDoc.TextureMappingEventType.Deleted => Deleted,
        RhinoDoc.TextureMappingEventType.Undeleted => Undeleted, RhinoDoc.TextureMappingEventType.Modified => Modified,
        _ => Fin.Fail<ComponentTransition>(error: Op.Of(name: nameof(ComponentTransition)).InvalidResult()),
    };
}

[SmartEnum<int>]
public sealed partial class RenderTransition {
    public static readonly RenderTransition Loaded = new(key: 0);
    public static readonly RenderTransition Clearing = new(key: 1);
    public static readonly RenderTransition Cleared = new(key: 2);
    public static readonly RenderTransition MaterialAssignmentChanged = new(key: 3);

    internal static Fin<RenderTransition> Of(RhinoDoc.RenderContentTableEventType value) => value switch {
        RhinoDoc.RenderContentTableEventType.Loaded => Loaded,
        RhinoDoc.RenderContentTableEventType.Clearing => Clearing,
        RhinoDoc.RenderContentTableEventType.Cleared => Cleared,
        RhinoDoc.RenderContentTableEventType.MaterialAssignmentChanged => MaterialAssignmentChanged,
        _ => Fin.Fail<RenderTransition>(error: Op.Of(name: nameof(RenderTransition)).InvalidResult()),
    };
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
    public sealed record Objects(Seq<Guid> Ids) : EventPayload;
    public sealed record Replaced(Guid Old, Option<Guid> New) : EventPayload;
    public sealed record Attributes(Option<Guid> Object) : EventPayload;
    public sealed record TransformStarted(
        uint EventId,
        Transform Motion,
        bool Copies,
        Seq<(Guid Id, uint Serial)> Objects,
        Seq<(Guid Id, uint Serial)> Grips,
        Seq<(Guid Id, uint Serial)> GripOwners) : EventPayload;
    public sealed record TransformEnded(uint EventId) : EventPayload;
    public sealed record Selection(Seq<Guid> Ids, int Count) : EventPayload;
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
    public sealed record Drawing(bool Enabled) : EventPayload;
    public sealed record Page(uint PageSerial, Option<Guid> OldDetail, Option<Guid> NewDetail) : EventPayload;
    public sealed record Projection(Guid ViewportId, uint ChangeCounter) : EventPayload;
    public sealed record DisplayMode(Guid ViewportId, Guid Old, Guid Next) : EventPayload;
    public sealed record Frame(Guid ViewportId, uint ChangeCounter, Option<uint> ViewSerial) : EventPayload;
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
    public Seq<Guid> ObjectIds => Payload.Switch(
        signal: static _ => Seq<Guid>(),
        opened: static _ => Seq<Guid>(),
        saved: static _ => Seq<Guid>(),
        active: static _ => Seq<Guid>(),
        unitsScaled: static _ => Seq<Guid>(),
        userString: static _ => Seq<Guid>(),
        worksession: static _ => Seq<Guid>(),
        objects: static fact => fact.Ids,
        replaced: static fact => fact.New.ToSeq().Cons(value: fact.Old),
        attributes: static fact => fact.Object.ToSeq(),
        transformStarted: static fact => fact.Objects
            .Concat(fact.Grips)
            .Concat(fact.GripOwners)
            .Map(static item => item.Id)
            .Distinct(),
        transformEnded: static _ => Seq<Guid>(),
        selection: static fact => fact.Ids,
        component: static _ => Seq<Guid>(),
        renderContent: static _ => Seq<Guid>(),
        textureMapping: static _ => Seq<Guid>(),
        view: static _ => Seq<Guid>(),
        drawing: static _ => Seq<Guid>(),
        page: static _ => Seq<Guid>(),
        projection: static _ => Seq<Guid>(),
        displayMode: static _ => Seq<Guid>(),
        frame: static _ => Seq<Guid>(),
        panel: static _ => Seq<Guid>(),
        files: static _ => Seq<Guid>());
}

[SmartEnum<int>]
public sealed partial class PanelState {
    public static readonly PanelState Shown = new(key: 0);
    public static readonly PanelState Hidden = new(key: 1);
    public static readonly PanelState Closed = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class FileChangeKind {
    public static readonly FileChangeKind Created = new(key: (int)WatcherChangeTypes.Created);
    public static readonly FileChangeKind Deleted = new(key: (int)WatcherChangeTypes.Deleted);
    public static readonly FileChangeKind Changed = new(key: (int)WatcherChangeTypes.Changed);
    public static readonly FileChangeKind Renamed = new(key: (int)WatcherChangeTypes.Renamed);

    internal static Fin<FileChangeKind> Of(WatcherChangeTypes native, Op key) =>
        toSeq(Items).Find(kind => kind.Key == (int)native).ToFin(key.InvalidInput());
}

public readonly record struct FileEdge(FileChangeKind Kind, string Path, Option<string> PreviousPath);
```

## [04]-[DELIVERY_POLICY]

- Owner: `StreamLane` rows own channel construction over the full bounded-policy product. `Mailbox` is a one-value latest-state register, `Shed` a bounded newest-window that drops the oldest fact, `Ordered` a bounded accepted-prefix FIFO that refuses the incoming fact, and `Firehose` an unbounded lossless lane. `Delivery` carries its sink for direct modes or its lane for paced mode, so sink/lane mismatch cannot be constructed.
- Law: a host callback never waits for a reader. A full ordered lane rejects the incoming fact and preserves the accepted prefix; the drop callback records the loss without blocking the host. `Dropping` is the `Cadence.PerFrame` admission column: a per-frame family refuses `Firehose` at observation admission because an unbounded lane under frame cadence grows without a shed edge.
- Law: every drop, refused write, deferred overflow, reentrant suppression, callback fault, sink fault, cancellation, file-watch fault, file-batch overflow, transform-correlation loss, and journal eviction lands in `StreamReceipt`. Each `Watch` owns the journal identified by its `WatchKey`, and its `ReceiptPolicy` bounds every retained queue.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class StreamLane {
    private const int Window = 1024;

    public static readonly StreamLane Mailbox = new(
        key: 0,
        dropping: true,
        open: static dropped => Channel.CreateBounded<DocEvent>(
            new BoundedChannelOptions(capacity: 1) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true },
            itemDropped: dropped));
    public static readonly StreamLane Shed = new(
        key: 1,
        dropping: true,
        open: static dropped => Channel.CreateBounded<DocEvent>(
            new BoundedChannelOptions(capacity: Window) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true },
            itemDropped: dropped));
    public static readonly StreamLane Ordered = new(
        key: 2,
        dropping: true,
        open: static dropped => Channel.CreateBounded<DocEvent>(
            new BoundedChannelOptions(capacity: Window) { FullMode = BoundedChannelFullMode.DropWrite, SingleReader = true },
            itemDropped: dropped));
    public static readonly StreamLane Firehose = new(
        key: 3,
        dropping: false,
        open: static _ => Channel.CreateUnbounded<DocEvent>(
            new UnboundedChannelOptions { SingleReader = true }));

    public bool Dropping { get; }

    [UseDelegateFromConstructor]
    internal partial Channel<DocEvent> Open(Action<DocEvent> dropped);
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
    public sealed record Dropped(WatchKey Watch, StreamLane Lane, EventOrigin Origin) : StreamReceipt;
    public sealed record Refused(WatchKey Watch, StreamLane Lane, EventOrigin Origin) : StreamReceipt;
    public sealed record DeferredOverflow(WatchKey Watch, EventOrigin Origin) : StreamReceipt;
    public sealed record Reentrant(WatchKey Watch, EventOrigin Origin) : StreamReceipt;
    public sealed record CallbackFault(WatchKey Watch, EventOrigin Origin, string Detail) : StreamReceipt;
    public sealed record SinkFault(WatchKey Watch, EventOrigin Origin, string Detail) : StreamReceipt;
    public sealed record Cancelled(WatchKey Watch, EventOrigin Origin) : StreamReceipt;
    public sealed record FileOverflow(WatchKey Watch, string WatchedPath) : StreamReceipt;
    public sealed record FileFault(WatchKey Watch, string WatchedPath, string Detail) : StreamReceipt;
    public sealed record TransformEvicted(WatchKey Watch, uint EventId) : StreamReceipt;
    public sealed record TransformUnmatched(WatchKey Watch, uint EventId) : StreamReceipt;
    public sealed record JournalOverflow(WatchKey Watch, long Lost) : StreamReceipt;
}

// --- [STATE] ------------------------------------------------------------------------------
[ValueObject<long>]
public readonly partial struct WatchKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref long value) =>
        validationError = value > 0 ? null : new ValidationError(message: "Watch identity is not positive.");
}

[SmartEnum<int>]
public sealed partial class ReceiptPolicy {
    public static readonly ReceiptPolicy Operational = new(
        key: 0, receiptCapacity: 512, deferredCapacity: 512, fileCapacity: 256, correlationCapacity: 128);
    public static readonly ReceiptPolicy Diagnostic = new(
        key: 1, receiptCapacity: 4096, deferredCapacity: 2048, fileCapacity: 1024, correlationCapacity: 512);

    public int ReceiptCapacity { get; }
    public int DeferredCapacity { get; }
    public int FileCapacity { get; }
    public int CorrelationCapacity { get; }
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

    private readonly record struct ReceiptState(Seq<StreamReceipt> Items, long Lost);
}
```

## [05]-[STREAM_OWNER]

- Owner: `Observation` closes the source family as `Host` and `File`. Both cases carry the same `Delivery` and `ReceiptPolicy`; the host case names scope and families, while the file case names path, trailing debounce window, and clock. `DocumentStream.Observe` is the one admission and attachment entry.
- Law: every request validates source, delivery, receipt policy, and source-specific discriminants before the first attachment. `Subscription.AttachAll` rolls back every earlier attachment if a later attach fails.
- Law: `Watch.Dispose` detaches handlers before completing its paced writer. A reader therefore terminates after consuming the accepted prefix.
- Law: reentrancy state belongs to one observation. Recursive delivery from that observation is receipted without suppressing any sibling observation on the same thread.
- Law: file delivery uses `TimeProvider.CreateTimer` as a resettable one-shot. Every file-system edge moves the deadline, and the trailing edge emits a bounded `Files` batch preserving change kind, path, previous rename path, and overflow count through the same inline, deferred, or paced spine as host facts.
- Law: deferred delivery swaps the complete queued prefix to an empty successor on idle. Capacity applies per watch; cancellation suppresses queued sink work after teardown and leaves a receipt.

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
    private Subscription? subscription;
    private Emission? emission;
    private readonly ReceiptJournal journal;

    private Watch(Subscription subscription, Emission emission, ReceiptJournal journal) {
        this.subscription = subscription;
        this.emission = emission;
        this.journal = journal;
        Reader = emission.Reader;
    }

    public WatchKey Key => journal.Watch;
    public Seq<StreamReceipt> Receipts => journal.Snapshot;
    public Option<ChannelReader<DocEvent>> Reader { get; }

    internal static Watch Of(Subscription subscription, Emission emission, ReceiptJournal journal) =>
        new(subscription: subscription, emission: emission, journal: journal);

    public void Dispose() {
        Emission? active = Interlocked.Exchange(location1: ref emission, value: null);
        active?.Cancel();
        Subscription? captured = Interlocked.Exchange(location1: ref subscription, value: null);
        captured?.Dispose();
        active?.Complete();
    }
}

internal sealed class Emission {
    private readonly Delivery delivery;
    private readonly ReceiptJournal journal;
    private readonly Reentrancy gate = new();
    private readonly Option<Channel<DocEvent>> channel;
    private int active = 1;

    private Emission(Delivery delivery, ReceiptJournal journal, Option<Channel<DocEvent>> channel) {
        this.delivery = delivery;
        this.journal = journal;
        this.channel = channel;
    }

    internal Option<ChannelReader<DocEvent>> Reader => channel.Map(static value => value.Reader);
    private bool IsActive => Volatile.Read(location: ref active) != 0;

    internal static Fin<Emission> Open(Delivery delivery, ReceiptJournal journal, Op key) =>
        Optional(delivery).ToFin(Fail: key.InvalidInput()).Bind(active => active.Switch(
            state: (Journal: journal, Op: key),
            inline: static (state, mode) => Optional(mode.Sink).ToFin(Fail: state.Op.InvalidInput())
                .Map(_ => new Emission(delivery: mode, journal: state.Journal, channel: Option<Channel<DocEvent>>.None)),
            deferred: static (state, mode) => Optional(mode.Sink).ToFin(Fail: state.Op.InvalidInput())
                .Map(_ => new Emission(delivery: mode, journal: state.Journal, channel: Option<Channel<DocEvent>>.None)),
            paced: static (state, mode) => Optional(mode.Lane).ToFin(Fail: state.Op.InvalidInput()).Bind(lane =>
                state.Op.Catch(() => {
                    Channel<DocEvent> opened = lane.Open(dropped => ignore(state.Journal.Post(new StreamReceipt.Dropped(
                        Watch: state.Journal.Watch, Lane: lane, Origin: dropped.Origin))));
                    return Fin.Succ(value: new Emission(delivery: mode, journal: state.Journal, channel: Some(opened)));
                }))));

    internal Fin<Unit> Emit(DocEvent fact) =>
        !IsActive
            ? Fin.Succ(value: journal.Post(new StreamReceipt.Cancelled(Watch: journal.Watch, Origin: fact.Origin)))
            : gate.Active
                ? Fin.Succ(value: journal.Post(new StreamReceipt.Reentrant(Watch: journal.Watch, Origin: fact.Origin)))
                : delivery.Switch(
                    state: (Owner: this, Fact: fact),
                    inline: static (state, mode) => state.Owner.gate.Guarded(
                        journal: state.Owner.journal, origin: state.Fact.Origin, run: () => mode.Sink(arg: state.Fact)),
                    deferred: static (state, mode) => IdlePump.Enqueue(
                        journal: state.Owner.journal,
                        origin: state.Fact.Origin,
                        alive: () => state.Owner.IsActive,
                        run: () => state.Owner.gate.Guarded(
                            journal: state.Owner.journal, origin: state.Fact.Origin, run: () => mode.Sink(arg: state.Fact))),
                    paced: static (state, mode) => state.Owner.channel
                        .ToFin(Op.Of(name: nameof(Emission)).InvalidResult())
                        .Map(opened => opened.Writer.TryWrite(item: state.Fact)
                            ? unit
                            : state.Owner.journal.Post(new StreamReceipt.Refused(
                                Watch: state.Owner.journal.Watch, Lane: mode.Lane, Origin: state.Fact.Origin))));

    internal void Cancel() => Interlocked.Exchange(location1: ref active, value: 0);

    internal void Complete() =>
        ignore(channel.Map(static opened => opened.Writer.TryComplete()));
}

// --- [SERVICES] ---------------------------------------------------------------------------
public static class DocumentStream {
    private static long sequence;

    public static Fin<Watch> Observe(Observation request) {
        Op op = Op.Of();
        return Optional(request).ToFin(Fail: op.InvalidInput()).Bind(active => active.Switch(
            state: op,
            host: static (key, observation) => ObserveHost(request: observation, key: key),
            file: static (key, observation) => ObserveFile(request: observation, key: key)));
    }

    private static Fin<Watch> ObserveHost(Observation.Host request, Op key) =>
        from scope in Optional(request.Scope).ToFin(Fail: key.InvalidInput())
        from delivery in Optional(request.Delivery).ToFin(Fail: key.InvalidInput())
        from policy in Optional(request.Receipts).ToFin(Fail: key.InvalidInput())
        from families in request.Families
            .TraverseM(family => Optional(family).ToFin(Fail: key.InvalidInput()))
            .As()
            .Map(static named => named.Distinct())
            .Bind(named => named.IsEmpty ? Fin.Fail<Seq<EventFamily>>(error: key.InvalidInput()) : Fin.Succ(value: named))
        from _ in families.TraverseM(family => family.Cadence.Admits(delivery: delivery)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: key.Unsupported(geometryType: typeof(EventFamily), outputType: typeof(Delivery)))).As()
        let journal = new ReceiptJournal(
            watch: WatchKey.Create(value: Interlocked.Increment(location: ref sequence)),
            policy: policy)
        from emission in Emission.Open(delivery: delivery, journal: journal, key: key)
        from handlers in Attach(scope: scope, families: families, emission: emission, journal: journal)
            .MapFail(error => { emission.Cancel(); emission.Complete(); return error; })
        select Watch.Of(subscription: handlers, emission: emission, journal: journal);

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
        from delivery in Optional(request.Delivery).ToFin(Fail: key.InvalidInput())
        from policy in Optional(request.Receipts).ToFin(Fail: key.InvalidInput())
        from _ in guard(request.Debounce > TimeSpan.Zero, key.InvalidInput())
        let journal = new ReceiptJournal(
            watch: WatchKey.Create(value: Interlocked.Increment(location: ref sequence)),
            policy: policy)
        from emission in Emission.Open(delivery: delivery, journal: journal, key: key)
        from subscription in AttachFile(
            path: path, debounce: request.Debounce, clock: clock, emission: emission, journal: journal, key: key)
            .MapFail(error => { emission.Cancel(); emission.Complete(); return error; })
        select Watch.Of(subscription: subscription, emission: emission, journal: journal);

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
                FileSystemEventHandler change = (_, args) => ignore(Capture(args).MapFail(error => {
                    _ = journal.Post(new StreamReceipt.SinkFault(Watch: journal.Watch, Origin: origin, Detail: error.Message));
                    return error;
                }));
                RenamedEventHandler rename = (_, args) => ignore(Capture(new FileEdge(
                    Kind: FileChangeKind.Renamed,
                    Path: args.FullPath,
                    PreviousPath: Optional(args.OldFullPath))).MapFail(error => {
                        _ = journal.Post(new StreamReceipt.SinkFault(Watch: journal.Watch, Origin: origin, Detail: error.Message));
                        return error;
                    }));
                ErrorEventHandler failure = (_, args) => ignore(CaptureOverflow(failure: args.GetException()).MapFail(error => {
                    _ = journal.Post(new StreamReceipt.SinkFault(Watch: journal.Watch, Origin: origin, Detail: error.Message));
                    return error;
                }));
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
                    .Map(handle => owner | handle)
                    .MapFail(error => { owner.Dispose(); return error; });
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
    private Action? detach;

    private Subscription(Action? detach) => this.detach = detach;

    public static Subscription Of(Action detach) {
        ArgumentNullException.ThrowIfNull(detach);
        return new(detach: detach);
    }

    public static Fin<Subscription> Attach<THandler>(Action<THandler> subscribe, Action<THandler> unsubscribe, THandler handler)
        where THandler : Delegate {
        Op key = Op.Of(name: nameof(Subscription));
        return key.Catch(() => { subscribe(obj: handler); return Fin.Succ(value: Of(detach: () => unsubscribe(obj: handler))); })
            .MapFail(error => {
                _ = key.Catch(() => { unsubscribe(obj: handler); return Fin.Succ(value: unit); });
                return error;
            });
    }

    public static Fin<Subscription> Acquire(Action acquire, Action release) {
        ArgumentNullException.ThrowIfNull(acquire);
        ArgumentNullException.ThrowIfNull(release);
        Op key = Op.Of(name: nameof(Subscription));
        return key.Catch(() => { acquire(); return Fin.Succ(value: Of(detach: release)); })
            .MapFail(error => {
                _ = key.Catch(() => { release(); return Fin.Succ(value: unit); });
                return error;
            });
    }

    public static Fin<Subscription> AttachAll(Seq<Func<Fin<Subscription>>> attach) =>
        attach.Fold(
            Fin.Succ(value: new Subscription(detach: null)),
            static (rail, start) => rail.Bind(held => start()
                .Map(next => held | next)
                .MapFail(error => { held.Dispose(); return error; })));

    public static Subscription operator |(Subscription first, Subscription second) {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return new(detach: () => { second.Dispose(); first.Dispose(); });
    }

    public void Dispose() {
        Action? captured = Interlocked.Exchange(location1: ref detach, value: null);
        _ = captured is null
            ? unit
            : ignore(Op.Of(name: nameof(Subscription)).Catch(() => { captured(); return Fin.Succ(value: unit); }));
    }
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

internal static class IdlePump {
    private static long sequence;
    private static readonly Atom<Seq<DeferredWork>> Queue = Atom(value: Seq<DeferredWork>());
    private static readonly Lazy<Fin<Unit>> Hook = new(
        valueFactory: static () => Op.Of(name: nameof(IdlePump)).Catch(() => {
            RhinoApp.Idle += static (_, _) => Drain();
            return Fin.Succ(value: unit);
        }),
        mode: LazyThreadSafetyMode.ExecutionAndPublication);

    internal static Fin<Unit> Enqueue(
        ReceiptJournal journal,
        EventOrigin origin,
        Func<bool> alive,
        Func<Fin<Unit>> run) => Hook.Value.Map(_ => {
            DeferredWork work = new(
                Id: Interlocked.Increment(ref sequence),
                Journal: journal,
                Origin: origin,
                Alive: alive,
                Run: run);
            Seq<DeferredWork> accepted = Queue.Swap(f: current =>
                current.Filter(item => item.Journal.Watch == journal.Watch).Count < journal.Policy.DeferredCapacity
                    ? current.Add(value: work)
                    : current);
            return accepted.Exists(candidate => candidate.Id == work.Id)
                ? unit
                : journal.Post(new StreamReceipt.DeferredOverflow(Watch: journal.Watch, Origin: origin));
        });

    private static void Drain() {
        Seq<DeferredWork> pending = Seq<DeferredWork>();
        _ = Queue.Swap(f: current => (pending = current, Seq<DeferredWork>()).Item2);
        _ = pending.Iter(static work => ignore(work.Alive()
            ? Op.Of(name: nameof(IdlePump)).Catch(work.Run)
            : Fin.Succ(value: work.Journal.Post(new StreamReceipt.Cancelled(Watch: work.Journal.Watch, Origin: work.Origin)))));
    }

    private readonly record struct DeferredWork(
        long Id,
        ReceiptJournal Journal,
        EventOrigin Origin,
        Func<bool> Alive,
        Func<Fin<Unit>> Run);
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]          | [FORM]                                               | [ENTRY]                         |
| :-----: | :---------------- | :--------------- | :--------------------------------------------------- | :------------------------------ |
|  [01]   | event vocabulary  | `EventFamily`    | band/cadence/bind rows with derived band projection  | `In` / `Bind`                   |
|  [02]   | evidence carriage | `EventPayload`   | detached closed family                               | `DocEvent.Payload`              |
|  [03]   | delivery policy   | `Delivery`       | sink-bearing direct cases or lane-bearing paced case | `Inline` / `Deferred` / `Paced` |
|  [04]   | back-pressure     | `StreamLane`     | nonblocking channel-construction rows                | `Open`                          |
|  [05]   | observation spine | `DocumentStream` | one source-polymorphic admission and attachment rail | `Observe(Observation)`          |
|  [06]   | lifetime          | `Watch`          | handler detachment plus channel completion           | `Dispose`                       |
|  [07]   | diagnostics       | `StreamReceipt`  | watch-keyed, policy-bounded delivery evidence        | `Watch.Receipts`                |
