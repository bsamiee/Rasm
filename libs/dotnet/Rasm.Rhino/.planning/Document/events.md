# [RASM_RHINO_EVENTS]

`DocumentStream` owns observation from raw host and filesystem callbacks: detached facts, nonblocking delivery, bounded loss evidence, retryable symmetric detachment. `Observation` carries source-specific admission, `EventFamily` carries host wiring as data, and `Watch` retains delivery and release outcomes under one identity — its journal is the kernel bounded ring, so a fact storm sheds against a declared cap and every loss reads as a number. `RhinoPoint` names every detached stream as `rasm.rhino.<domain>.<point>` realizing the kernel `IHookRoster`, `MountRegistry` owns name-addressed discovery and first-mount-wins custody over the kernel `HookMounts` seat table, and `RhinoInstruments` declares the contributed rows. `LifecycleGate`, `Subscription`, and the idle pump live at `Document/lifetime.md`; release composes kernel `Custody`.

## [01]-[INDEX]

- [02]-[FAMILY]: `EventFamily` binds host callbacks, cadence, and projection as data.
- [03]-[PAYLOAD]: `EventPayload` and `DocEvent` carry detached callback evidence.
- [04]-[DELIVERY]: `Delivery`, `StreamBodyKind`, `StreamSlot`, and `StreamPolicy` close bounded delivery and loss facts over the kernel ring.
- [05]-[STREAM_OWNER]: `DocumentStream` and `Watch` own admission, attachment, delivery, and release.
- [06]-[HOOK_REGISTRY]: `RhinoPoint` and `MountRegistry` close point addressing, host-truth modality over the kernel roster floor, mount custody, and multi-plugin arbitration over the kernel `HookMounts`.
- [07]-[TELEMETRY_TAP]: `RhinoInstruments` declares the contributed instrument rows and the string-scoped port.

## [02]-[FAMILY]

- Owner: `EventFamily` binds one symbolic host event key to its band, cadence, attach/detach pair, and callback-scope projection; `Cadence` carries the admission its rows refuse under as a typed result; `DocumentFault` is the folder's document-stream refusal family on the kernel `FaultBand.HostDocument` row.
- Entry: `EventFamily.In` derives band membership from generated `Items`, while `Bind` retains the exact attached delegate for release. `On` is ONE binder under two arities discriminated by the projection's return shape — a pure projection lifts inside the binder, a fallible one rides its own carrier — so no `OnFallible` sibling name exists.
- Law: draw facts retain phase and viewport evidence without retaining `DisplayPipeline`, and per-object phases add the drawn or culled `RhinoObject` identity.
- Law: a bracketed host pair is one family — `Transform` binds `BeforeTransformObjects` and `AfterTransformObjects` under one `CorrelationWindow` keyed on the host `TransformEventId` both sides publish, so the closing arm resolves the opening arm's `DocKey` and every scope that delivers a start delivers its matching end; the payload case, never the family, discriminates start from end.
- Law: the correlation window rides an atom with snapshot-guarded steps and its verdict rides the transition — a contended retain answers `Contended` and the caller DELIVERS, because deduplication is an optimization and a correctness gate that dropped a fact under contention would trade a duplicate for a loss.
- Law: table projections detach transition, index, and prior/current component evidence; later live resolution re-enters through document identity.
- Law: callback projection faults and sink faults remain disjoint journal rows, and no verdict is silently lost — a projection fault rides `reject` into the journal as `CallbackFault`, a delivery fault posts at the emission's own arm as `SinkFault`, and the handler's terminal discard reads a verdict the journal already holds, because a host event handler returns `void` and the journal row is the only record that can leave it.
- Law: `Cadence.Admits` answers the result — `PerFrame` refuses a non-dropping delivery with `DocumentFault.Cadence` naming the family, so an admission failure carries which family demanded frame cadence rather than a bare boolean a caller re-wraps.
- Growth: a host callback — or a host pair bracketing one fact — lands as one symbolic `EventFamily` row whose projection expires every callback-owned handle before delivery; a new stream refusal is one `DocumentFault` case and one offset row inside the band's span.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Rasm.Domain;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
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
    public static readonly Cadence Changed = new(static (_, _, _) => Fin.Succ(unit));
    public static readonly Cadence PerFrame = new(static (delivery, family, key) =>
        delivery is Delivery.Paced paced && paced.Lane.Dropping
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new DocumentFault.Cadence(Family: family)));

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Admits(Delivery delivery, EventFamily family);
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.HostDocument;
    private DocumentFault() { }

    [FaultCase(0)] public sealed partial record Cadence(EventFamily Family) : DocumentFault;
    [FaultCase(1)] public sealed partial record SeatDiverged(RhinoPoint Point) : DocumentFault;
    [FaultCase(2)] public sealed partial record RiderDuplicate(RhinoPoint Point, PluginKey Plugin) : DocumentFault;

    public sealed override string Message => Switch(
        cadence: static fault => $"Document event family '{fault.Family}' refused the cadence for '{fault.Key}'.",
        seatDiverged: static fault => $"Document event seat '{fault.Point}' diverged for '{fault.Key}'.",
        riderDuplicate: static fault => $"Plugin '{fault.Plugin}' already rides document point '{fault.Point}' for '{fault.Key}'.");
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
    public static readonly EventFamily WorksessionFile = new(key: nameof(WorksessionFile), band: EventBand.Lifecycle, cadence: Cadence.Changed, bind: On<RhinoDoc.WorksessionFileChangedEventArgs>(
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
    public static readonly EventFamily TextureMappingTable = new(key: nameof(TextureMappingTable), band: EventBand.Tables, cadence: Cadence.Changed, bind: On<RhinoDoc.TextureMappingEventArgs>(
        subscribe: h => RhinoDoc.TextureMappingEvent += h,
        unsubscribe: h => RhinoDoc.TextureMappingEvent -= h,
        project: static (_, a, scope) => ComponentTransition.Of(a.EventType).Map(transition => Gate(
            document: a.Document,
            scope: scope,
            payload: new EventPayload.TextureMapping(
                Transition: transition,
                Current: transition.Carries.Admits(TransitionEvidence.Current)
                    ? Optional(a.NewMapping).Map(static mapping => mapping.Id)
                    : Option<Guid>.None)))));

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
        StreamJournal journal,
        Func<Option<DocKey>, EventPayload, Fin<Unit>> deliver,
        Action<Error> reject);

    public static Fin<Seq<EventFamily>> In(EventBand band) =>
        Optional(band)
            .ToFin(Fail: new KernelFault.InvalidInput())
            .Map(active => toSeq(Items).Filter(family => family.Band == active));

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> On<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<object?, TArgs, EventScope, Option<(Option<DocKey> Key, EventPayload Payload)>> project) where TArgs : EventArgs =>
        On(
            subscribe: subscribe,
            unsubscribe: unsubscribe,
            project: (sender, args, scope) => Fin.Succ(value: project(arg1: sender, arg2: args, arg3: scope)));

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> On<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<object?, TArgs, EventScope, Fin<Option<(Option<DocKey> Key, EventPayload Payload)>>> project) where TArgs : EventArgs =>
        (scope, journal, deliver, reject) => {
            EventHandler<TArgs> handler = (sender, args) => {
                Fin<Unit> outcome = Try.lift(() => project(sender, args, scope)).Run().Bind(static inner => inner).Match(
                    Succ: projected => projected
                        .TraverseM(fact => Try.lift(() => deliver(fact.Key, fact.Payload)).Run().Bind(static inner => inner)).As().Map(static _ => unit),
                    Fail: error => {
                        reject(obj: error);
                        return Fin.Fail<Unit>(error: error);
                    });
                _ = outcome;
            };
            return Subscription.Attach(subscribe: subscribe, unsubscribe: unsubscribe, handler: handler);
        };

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> Signal(
        Action<EventHandler<DocumentEventArgs>> subscribe,
        Action<EventHandler<DocumentEventArgs>> unsubscribe) =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: static (_, a, scope) =>
            Gate(serial: a.DocumentSerialNumber, scope: scope, payload: new EventPayload.Signal()));

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> Table<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        TableKind kind,
        Func<TArgs, RhinoDoc> document,
        Func<TArgs, int> index,
        Func<TArgs, Fin<ComponentTransition>> transition,
        Func<TArgs, Option<ComponentState>> previous,
        Func<TArgs, Option<ComponentState>> current) where TArgs : EventArgs =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, args, scope) => transition(arg: args).Map(change => Gate(
                document: document(arg: args),
                scope: scope,
                payload: new EventPayload.Component(
                    Kind: kind,
                    Index: index(arg: args),
                    Transition: change,
                    Previous: change.Carries.Admits(TransitionEvidence.Previous) ? previous(arg: args) : Option<ComponentState>.None,
                    Current: change.Carries.Admits(TransitionEvidence.Current) ? current(arg: args) : Option<ComponentState>.None))));

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> Render(
        Action<EventHandler<RhinoDoc.RenderContentTableEventArgs>> subscribe,
        Action<EventHandler<RhinoDoc.RenderContentTableEventArgs>> unsubscribe,
        TableKind kind) =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, args, scope) => RenderTransition.Of(args.EventType).Map(change => Gate(
                document: args.Document,
                scope: scope,
                payload: new EventPayload.RenderContent(
                    Kind: kind,
                    Transition: change,
                    Assignment: args is RhinoDoc.RenderMaterialAssignmentChangedEventArgs assignment
                        ? RenderAssignment.Of(assignment)
                        : Option<RenderAssignment>.None))));

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> ViewFact(
        Action<EventHandler<ViewEventArgs>> subscribe,
        Action<EventHandler<ViewEventArgs>> unsubscribe) =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: static (_, a, scope) => Optional(a.View).Bind(view =>
            Gate(document: view.Document, scope: scope, payload: new EventPayload.View(
                ViewSerial: view.RuntimeSerialNumber,
                MainViewportId: view.MainViewport.Id,
                Kind: view is RhinoPageView ? ViewKind.Page : ViewKind.Model))));

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> DrawFact<TArgs>(
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<TArgs, Option<(Guid Id, uint Serial)>>? subject = null) where TArgs : DrawEventArgs =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, a, scope) => Optional(a.Viewport).Bind(viewport =>
            Gate(document: a.RhinoDoc, scope: scope, payload: new EventPayload.Frame(
                ViewportId: viewport.Id,
                ChangeCounter: viewport.ChangeCounter,
                ViewSerial: Optional(viewport.ParentView).Map(static view => view.RuntimeSerialNumber),
                Object: subject is null ? Option<(Guid Id, uint Serial)>.None : subject(arg: a)))));

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> ProjectionFact(
        Action<EventHandler<DrawEventArgs>> subscribe,
        Action<EventHandler<DrawEventArgs>> unsubscribe,
        Func<EventFamily> family) =>
        (scope, journal, deliver, reject) => {
            CorrelationWindow<(Guid Viewport, uint Document), uint> seen = new(capacity: journal.Policy.CorrelationCapacity);
            return On(subscribe: subscribe, unsubscribe: unsubscribe, project: (_, a, watched) =>
                Optional(a.RhinoDoc).Bind(document => Optional(a.Viewport).Bind(viewport => {
                    uint counter = viewport.ChangeCounter;
                    CorrelationMove move = seen.Retain(key: (viewport.Id, document.RuntimeSerialNumber), value: counter);
                    _ = Cleared(journal: journal, family: family, move: move);
                    return move is CorrelationMove.Held
                        ? Option<(Option<DocKey> Key, EventPayload Payload)>.None
                        : Gate(document: document, scope: watched, payload: new EventPayload.Projection(ViewportId: viewport.Id, ChangeCounter: counter));
                })))(scope, journal, deliver, reject);
        };

    private static Func<EventScope, StreamJournal, Func<Option<DocKey>, EventPayload, Fin<Unit>>, Action<Error>, Fin<Subscription>> Bracketed<TOpen, TClose>(
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
                        _ = Cleared(journal: journal, family: family, move: bracket.Retain(key: correlateOpen(arg: args), value: fact.Key));
                        return Gate(scope: watched, payload: fact.Payload);
                    }))(scope, journal, deliver, reject),
                () => On(subscribe: subscribeClose, unsubscribe: unsubscribeClose, project: (_, args, watched) =>
                    bracket.Release(key: correlateClose(arg: args))
                        .Bind(key => Gate(scope: watched, payload: close(arg: args))))(scope, journal, deliver, reject)));
        };

    private static Unit Cleared(StreamJournal journal, Func<EventFamily> family, CorrelationMove move) =>
        move is CorrelationMove.Advanced advanced && advanced.Cleared > 0
            ? journal.Post(slot: StreamSlot.CorrelationReset, body: new StreamBody.Reset(Family: family(), Cleared: advanced.Cleared))
            : unit;

    private static Option<(Option<DocKey> Key, EventPayload Payload)> ObjectFact(object? sender, RhinoObjectEventArgs args, EventScope scope) =>
        Gate(document: (sender as RhinoDoc) ?? args.TheObject?.Document, scope: scope, payload: new EventPayload.Objects(Ids: Seq(args.ObjectId)));

    private static Option<(Option<DocKey> Key, EventPayload Payload)> SelectionFact(object? sender, RhinoObjectSelectionEventArgs args, EventScope scope) =>
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
            .Choose(static document => DocKey.Of(document: document).ToOption())
            .Head;

    private static Seq<(Guid Id, uint Serial)> ObjectRefs(IEnumerable<RhinoObject?> objects) =>
        toSeq(objects).Choose(static item => Optional(item).Map(static value => (value.Id, value.RuntimeSerialNumber)));

    private static Option<(Guid Id, uint Serial)> DrawSubject(RhinoObject? subject) =>
        Optional(subject).Map(static value => (value.Id, value.RuntimeSerialNumber)).Filter(static value => value.Id != Guid.Empty);

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    internal abstract partial record CorrelationMove {
        private CorrelationMove() { }
        internal sealed record Advanced(int Cleared) : CorrelationMove;
        internal sealed record Held : CorrelationMove;
        internal sealed record Contended : CorrelationMove;
    }

    private sealed class CorrelationWindow<TKey, TValue>(int capacity)
        where TKey : notnull
        where TValue : notnull {
        private readonly Atom<HashMap<TKey, TValue>> held = Atom(HashMap<TKey, TValue>());

        internal CorrelationMove Retain(TKey key, TValue value) {
            HashMap<TKey, TValue> standing = held.Value;
            bool advanced = standing.Find(key).Map(prior => !EqualityComparer<TValue>.Default.Equals(x: prior, y: value)).IfNone(true);
            if (!advanced) {
                return new CorrelationMove.Held();
            }
            int cleared = standing.Count >= capacity && !standing.ContainsKey() ? standing.Count : 0;
            HashMap<TKey, TValue> next = (cleared > 0 ? HashMap<TKey, TValue>() : standing).AddOrUpdate(key, value);
            return Cell.Step(
                    cell: held,
                    step: current => current == standing ? Some(next) : None,
                    declined: new KernelFault.InvalidResult())
                is Transition<HashMap<TKey, TValue>>.Committed
                    ? new CorrelationMove.Advanced(Cleared: cleared)
                    : new CorrelationMove.Contended();
        }

        internal Option<TValue> Release(TKey key) {
            HashMap<TKey, TValue> standing = held.Value;
            return standing.Find(key).Bind(claimed => Cell.Step(
                    cell: held,
                    step: current => current == standing ? Some(current.Remove(key)) : None,
                    declined: new KernelFault.InvalidResult())
                is Transition<HashMap<TKey, TValue>>.Committed
                    ? Some(claimed)
                    : Option<TValue>.None);
        }
    }

    private static Option<(Option<DocKey> Key, EventPayload Payload)> Gate(RhinoDoc? document, EventScope scope, EventPayload payload) =>
        document is RhinoDoc active
            ? Gate(serial: active.RuntimeSerialNumber, scope: scope, payload: payload)
            : Option<(Option<DocKey> Key, EventPayload Payload)>.None;

    private static Option<(Option<DocKey> Key, EventPayload Payload)> Gate(uint serial, EventScope scope, EventPayload payload) =>
        serial == 0
            ? Option<(Option<DocKey> Key, EventPayload Payload)>.None
            : Gate(key: DocKey.Create(value: serial), scope: scope, payload: payload);

    private static Option<(Option<DocKey> Key, EventPayload Payload)> Gate(DocKey key, EventScope scope, EventPayload payload) =>
        scope.Switch(
            (Key: key, Payload: payload),
            document: static (state, watched) => watched.Key == state.Key
                ? Some((Some(state.Key), state.Payload))
                : Option<(Option<DocKey> Key, EventPayload Payload)>.None,
            anyDocument: static (state, _) => Some((Some(state.Key), state.Payload)));

    private static Option<(Option<DocKey> Key, EventPayload Payload)> GateActive(uint serial, EventScope scope) =>
        serial > 0
            ? Gate(serial: serial, scope: scope, payload: new EventPayload.Active(ActiveDocument: Some(DocKey.Create(value: serial))))
            : scope.Switch(
                document: static _ => Option<(Option<DocKey> Key, EventPayload Payload)>.None,
                anyDocument: static _ => Some((
                    Option<DocKey>.None,
                    (EventPayload)new EventPayload.Active(ActiveDocument: Option<DocKey>.None))));
}
```

## [03]-[PAYLOAD]

- Owner: `EventPayload` owns detached callback evidence, while `DocEvent` adds source identity and the optional document key; `TransitionEvidence` is the capability vocabulary a component transition carries.
- Law: every reference-like host member projects inside its callback into stable identity, value, transition, or component evidence.
- Law: an absent active document remains a typed transition; `TransformStarted` and `TransformEnded` both carry the host `TransformEventId`, so a consumer joins the bracket on that id without retaining either callback's arrays.
- Law: name-keyed transition vocabularies admit host enums generically and fail unknown host values on the typed result.
- Law: a transition's evidence is a SET, not a bool pair — `Carries` names which of prior and current a transition publishes, the four corners are all real rows (`Added` carries current alone, `Deleted` prior alone, `Modified` both, `Sorted` neither), so the law is open and the projection reads set algebra rather than two parallel columns.
- Law: component presence is a CASE, never a flag — `ComponentState.Present` and `Deleted` each carry the name column the host read for that state (`Name` against `DeletedName`), so the discriminant that chose the host member is the case a consumer matches; a view's kind is a `ViewKind` row for the same reason.
- Law: `EventPayload.ObjectIds` defaults to no object contribution, and contributing cases override that projection; `DocEvent` delegates without an empty-arm dispatch ladder.
- Law: no intermediate envelope exists — the projection hands its key-and-payload pair straight to the delivery continuation and `DocEvent` is the ONE carrier adding origin; a second two-field record between them shadowed the kernel `EventEnvelope` inside a namespace whose prelude imports `Rasm.Domain` and carried nothing `DocEvent` does not.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class TransitionEvidence : ICapability<TransitionEvidence> {
    public static readonly TransitionEvidence Previous = new(key: "previous");
    public static readonly TransitionEvidence Current = new(key: "current");

    public static CapabilityLaw<TransitionEvidence> Law => CapabilityLaw<TransitionEvidence>.Open;
}

[SmartEnum<string>]
public sealed partial class ComponentTransition {
    public static readonly ComponentTransition Added = new(key: nameof(Added), carries: CapabilitySet<TransitionEvidence>.Of(TransitionEvidence.Current));
    public static readonly ComponentTransition Deleted = new(key: nameof(Deleted), carries: CapabilitySet<TransitionEvidence>.Of(TransitionEvidence.Previous));
    public static readonly ComponentTransition Undeleted = new(key: nameof(Undeleted), carries: CapabilitySet<TransitionEvidence>.Of(TransitionEvidence.Previous, TransitionEvidence.Current));
    public static readonly ComponentTransition Modified = new(key: nameof(Modified), carries: CapabilitySet<TransitionEvidence>.Of(TransitionEvidence.Previous, TransitionEvidence.Current));
    public static readonly ComponentTransition Sorted = new(key: nameof(Sorted), carries: CapabilitySet<TransitionEvidence>.None);
    public static readonly ComponentTransition Current = new(key: nameof(Current), carries: CapabilitySet<TransitionEvidence>.Of(TransitionEvidence.Previous, TransitionEvidence.Current));

    public CapabilitySet<TransitionEvidence> Carries { get; }

    internal static Fin<ComponentTransition> Of<TEvent>(TEvent value) where TEvent : struct, Enum =>
        Named<ComponentTransition, TEvent>(value: value);

    internal static Fin<T> Named<T, TEvent>(TEvent value)
        where T : class, ISmartEnum<string, T, ValidationError>
        where TEvent : struct, Enum {
        return HostEdge.NonEmpty(Enum.GetName(value: value)).ToFin(Fail: new KernelFault.InvalidResult()).Bind(key => FactoryBridge.Row<string, T>());
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

[SmartEnum<int>]
public sealed partial class ViewKind {
    public static readonly ViewKind Model = new(key: 0);
    public static readonly ViewKind Page = new(key: 1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ComponentState {
    private ComponentState() { }
    public sealed record Present(Guid Id, Option<string> Name) : ComponentState;
    public sealed record Deleted(Guid Id, Option<string> Name) : ComponentState;

    public Guid Id => Switch(
        present: static state => state.Id,
        deleted: static state => state.Id);

    internal static Option<ComponentState> Of(ModelComponent? component) => Optional(component).Map(static value => value.IsDeleted
        ? (ComponentState)new Deleted(Id: value.Id, Name: Optional(value.DeletedName))
        : new Present(Id: value.Id, Name: Optional(value.Name)));

    internal static Option<ComponentState> Of(Light? light) => Optional(light).Map(static value =>
        (ComponentState)new Present(Id: value.Id, Name: Optional(value.Name)));

    internal static Option<ComponentState> Of(LightObject? light) => Optional(light).Bind(static value =>
        Optional(value.LightGeometry).Map(geometry => value.IsDeleted
            ? (ComponentState)new Deleted(Id: geometry.Id, Name: Optional(geometry.Name))
            : new Present(Id: geometry.Id, Name: Optional(geometry.Name))));
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
    public sealed record View(uint ViewSerial, Guid MainViewportId, ViewKind Kind) : EventPayload;
    public sealed record Projection(Guid ViewportId, uint ChangeCounter) : EventPayload;
    public sealed record DisplayMode(Guid ViewportId, Guid Old, Guid Next) : EventPayload;
    public sealed record Frame(Guid ViewportId, uint ChangeCounter, Option<uint> ViewSerial, Option<(Guid Id, uint Serial)> Object) : EventPayload {
        public override Seq<Guid> ObjectIds => Object.Map(static value => value.Id).ToSeq();
    }
    public sealed record Panel(Guid PanelId, PanelState State) : EventPayload;
    public sealed record Files(Seq<FileEdge> Edges, long Overflow) : EventPayload;
}

// --- [MODELS] --------------------------------------------------------------------------
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

    internal static Fin<FileChangeKind> Of(WatcherChangeTypes native) =>
        FactoryBridge.Row<int, FileChangeKind>((int)native);
}

public readonly record struct FileEdge(FileChangeKind Kind, string Path, Option<string> PreviousPath);
```

## [04]-[DELIVERY]

- Owner: `Delivery` owns direct, idle-deferred, and paced modalities; `StreamLane` resolves paced channel construction from the admitted `StreamPolicy`; `StreamBodyKind` is the body-kind capability vocabulary; `StreamSlot` is the watch's consequence vocabulary with `StreamBody` its closed body family; `StreamJournal` is the kernel bounded ring holding those rows for one watch.
- Law: host callbacks never park — each paced lane either accepts immediately or emits loss evidence through the channel callback and write result.
- Law: channel continuations never execute synchronously on a producing host callback.
- Law: bounded lanes close every nonblocking full-buffer mode; `Coalesced` preserves the queued head and latest arrival by evicting the newest buffered predecessor.
- Law: frame cadence admits only bounded dropping lanes; unbounded accumulation is rejected before attachment.
- Law: `StreamSlot.Bodies` names the body kinds each consequence admits, and `StreamJournal.Post` validates the pair before parking it. The journal already identifies one watch, so each row carries only its slot and body.
- Law: one admission on this vocabulary is genuinely value-dependent and stays a clause rather than becoming a kind row — `FileOverflow` and `FileFault` emit the SAME `FileTrouble` body and split on whether its `Cause` carries the host error, a fact the case-keyed `Kind` fold cannot answer without holding a second authority over the value.
- Law: the journal is the kernel `Ring<T>` — capacity, oldest-first eviction, and the shed counter are that owner's, so the hand overflow accounting, the synthesized overflow row, and the `JournalOverflow` case all delete; a reader reads `Shed` and `Lost` beside `Facts` as numbers, and the app root's `rasm.rhino.stream.loss` tap counts the same `PacedLoss` posts by lane and loss kind.
- Law: a fault body carries the typed `Error`, never a rendered detail string — the four `string Detail` columns delete, because the `Error` in hand at every posting site survives classification, code reads, and the monoid where a message does not.
- Law: `StreamPolicy` owns named operational and maximum rows; generated admission rejects nonpositive values, individual ceiling breaches, and aggregate overcommit.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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
    internal partial Channel<DocEvent> Open(StreamPolicy policy, Action<StreamLoss, DocEvent> lost);

    private static Func<StreamPolicy, Action<StreamLoss, DocEvent>, Channel<DocEvent>> Bounded(
        Func<StreamPolicy, int> capacity,
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StreamBodyKind : ICapability<StreamBodyKind> {
    public static readonly StreamBodyKind Shed = new(key: "shed");
    public static readonly StreamBodyKind Dropped = new(key: "dropped");
    public static readonly StreamBodyKind Faulted = new(key: "faulted");
    public static readonly StreamBodyKind FileTrouble = new(key: "trouble");
    public static readonly StreamBodyKind Reset = new(key: "reset");
}

[SmartEnum<int>]
public sealed partial class StreamSlot {
    private static readonly CapabilitySet<StreamBodyKind> Overrun = CapabilitySet<StreamBodyKind>.Of(StreamBodyKind.Shed);
    private static readonly CapabilitySet<StreamBodyKind> Discarded = CapabilitySet<StreamBodyKind>.Of(StreamBodyKind.Dropped);
    private static readonly CapabilitySet<StreamBodyKind> Errored = CapabilitySet<StreamBodyKind>.Of(StreamBodyKind.Faulted);
    private static readonly CapabilitySet<StreamBodyKind> Troubled = CapabilitySet<StreamBodyKind>.Of(StreamBodyKind.FileTrouble);
    private static readonly CapabilitySet<StreamBodyKind> Cleared = CapabilitySet<StreamBodyKind>.Of(StreamBodyKind.Reset);

    public static readonly StreamSlot PacedLoss = new(key: 0, bodies: Overrun);
    public static readonly StreamSlot DeferredOverflow = new(key: 1, bodies: Discarded);
    public static readonly StreamSlot Reentrant = new(key: 2, bodies: Discarded);
    public static readonly StreamSlot CallbackFault = new(key: 3, bodies: Errored);
    public static readonly StreamSlot SinkFault = new(key: 4, bodies: Errored);
    public static readonly StreamSlot Cancelled = new(key: 5, bodies: Discarded);
    public static readonly StreamSlot FileOverflow = new(key: 6, bodies: Troubled);
    public static readonly StreamSlot FileFault = new(key: 7, bodies: Troubled);
    public static readonly StreamSlot CorrelationReset = new(key: 8, bodies: Cleared);
    public static readonly StreamSlot DetachFault = new(key: 9, bodies: Errored);

    public CapabilitySet<StreamBodyKind> Bodies { get; }

    public bool Admits(StreamBody body) =>
        Bodies.Admits(capability: body.Kind)
        && (this != FileFault || body is StreamBody.FileTrouble { Cause.IsSome: true });

}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StreamBody {
    private StreamBody() { }
    public sealed record Shed(StreamLane Lane, StreamLoss Loss, EventOrigin Origin) : StreamBody;
    public sealed record Dropped(EventOrigin Origin) : StreamBody;
    public sealed record Faulted(Option<EventOrigin> Origin, Error Cause) : StreamBody;
    public sealed record FileTrouble(string WatchedPath, Option<Error> Cause) : StreamBody;
    public sealed record Reset(EventFamily Family, int Cleared) : StreamBody;

    public StreamBodyKind Kind => Map(
        shed: StreamBodyKind.Shed,
        dropped: StreamBodyKind.Dropped,
        faulted: StreamBodyKind.Faulted,
        fileTrouble: StreamBodyKind.FileTrouble,
        reset: StreamBodyKind.Reset);
}

// --- [STATE] ---------------------------------------------------------------------------
[ValueObject<long>]
public readonly partial struct WatchKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref long value) =>
        validationError = value > 0 ? null : new ValidationError(message: "Watch identity is not positive.");
}

[ComplexValueObject]
public sealed partial class StreamPolicy {
    private static readonly (int Lane, int Journal, int Deferred, int File, int Correlation) OperationalValues = (
        Lane: 256,
        Journal: 4_096,
        Deferred: 512,
        File: 512,
        Correlation: 2_048);
    private static readonly (int Lane, int Journal, int Deferred, int File, int Correlation, long Total) CapacityLimits = (
        Lane: 4_096,
        Journal: 16_384,
        Deferred: 4_096,
        File: 4_096,
        Correlation: 8_192,
        Total: 24_576L);

    public int LaneCapacity { get; }
    public int JournalCapacity { get; }
    public int DeferredCapacity { get; }
    public int FileCapacity { get; }
    public int CorrelationCapacity { get; }

    public static StreamPolicy Operational { get; } = Create(
        laneCapacity: OperationalValues.Lane,
        journalCapacity: OperationalValues.Journal,
        deferredCapacity: OperationalValues.Deferred,
        fileCapacity: OperationalValues.File,
        correlationCapacity: OperationalValues.Correlation);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int laneCapacity,
        ref int journalCapacity,
        ref int deferredCapacity,
        ref int fileCapacity,
        ref int correlationCapacity) {
        Seq<(int Value, int Maximum)> capacities = Seq(
            (Value: laneCapacity, Maximum: CapacityLimits.Lane),
            (Value: journalCapacity, Maximum: CapacityLimits.Journal),
            (Value: deferredCapacity, Maximum: CapacityLimits.Deferred),
            (Value: fileCapacity, Maximum: CapacityLimits.File),
            (Value: correlationCapacity, Maximum: CapacityLimits.Correlation));
        long total = capacities.Fold(0L, static (sum, row) => sum + row.Value);
        validationError = capacities.ForAll(static row => row.Value > 0 && row.Value <= row.Maximum)
                && total <= CapacityLimits.Total
            ? null
            : new ValidationError(message: "Observation capacities exceed their positive per-capacity or aggregate bounds.");
    }

    public static Fin<StreamPolicy> Of(
        int laneCapacity,
        int journalCapacity,
        int deferredCapacity,
        int fileCapacity,
        int correlationCapacity) =>
        FactoryBridge.Accept<StreamPolicy>(
            fault: Validate(
                laneCapacity,
                journalCapacity,
                deferredCapacity,
                fileCapacity,
                correlationCapacity,
                out StreamPolicy? admitted),
            admitted: admitted);
}

internal sealed class StreamJournal {
    private readonly Ring<(StreamSlot Slot, StreamBody Body)> ring;

    internal StreamJournal(WatchKey watch, StreamPolicy policy) {
        Watch = watch;
        Policy = policy;
        ring = new Ring<(StreamSlot Slot, StreamBody Body)>(cap: Rasm.Numerics.Dimension.Create(value: policy.JournalCapacity));
    }

    internal WatchKey Watch { get; }
    internal StreamPolicy Policy { get; }
    internal Seq<(StreamSlot Slot, StreamBody Body)> Facts => ring.Parked;
    internal long Shed => ring.Shed;
    internal long Lost => ring.Lost;

    internal Unit Post(StreamSlot slot, StreamBody body) => ignore(ring.Park(item: slot.Admits(body: body)
        ? (Slot: slot, Body: body)
        : (
            Slot: StreamSlot.SinkFault,
            Body: new StreamBody.Faulted(Origin: None, Cause: new KernelFault.InvalidInput()))));

    internal SubscriptionRelease Faults(SubscriptionRelease release) {
        _ = release is SubscriptionRelease.Faulted faulted
            ? faulted.Errors.Fold(unit, (state, error) =>
                (Post(slot: StreamSlot.DetachFault, body: new StreamBody.Faulted(Origin: None, Cause: error)), state).Item2)
            : unit;
        return release;
    }
}
```

## [05]-[STREAM_OWNER]

- Owner: `Observation` carries each source's complete ingress, and `DocumentStream.Observe` owns admission, attachment, rollback, and watch minting.
- Law: every source, delivery, policy, and source-specific value admits before the first attachment; sequential attachment rolls back the accumulated prefix on failure.
- Law: `Watch.Close` cancels delivery, combines source and idle-pump detachment evidence, journals each fault as `DetachFault`, and retains each failed owner for a later close attempt.
- Law: close claims its owners under the lifecycle lock, executes callbacks after release, and publishes retry custody with one settled result atomically; concurrent callers join that result.
- Law: an empty subscription closes as `Released(0)`; `Open` denotes only unclaimed live custody.
- Law: reentrancy and deferred capacity belong to one watch, so recursive or queued work cannot suppress or exhaust a sibling observation; the reentrancy guard answers a VERDICT — absence is a suppressed recursive delivery — and the emission posts its own suppression and sink-fault evidence, so the guard stays journal-free.
- Law: deferred delivery owns one idle pump per watch through `IdlePump<EventOrigin>` (`Document/lifetime.md`) — the pump's loss callback posts the watch's own overflow and cancellation rows, closing the watch cancels its pending roster as evidence, and the drain crosses the kernel deferred lane for its gauged budget.
- Law: file callbacks fold into one resettable trailing-edge timer and one bounded batch before entering the same delivery spine as host facts.
- Exemption: native attach/detach, timer ownership, `Lock` scopes, and callback `try/finally` blocks are platform-forced lifetime boundaries.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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
        StreamPolicy Policy) : Observation;
    public sealed record File(
        string Path,
        TimeSpan Debounce,
        TimeProvider Clock,
        Delivery Delivery,
        StreamPolicy Policy) : Observation;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed class Watch : IDisposable {
    private readonly Lock gate = new();
    private readonly StreamJournal journal;
    private WatchClosure closure;

    internal Watch(Subscription subscription, Emission emission, StreamJournal journal) {
        this.journal = journal;
        closure = new WatchClosure.Ready(
            Subscription: subscription,
            Emission: emission,
            Release: new SubscriptionRelease.Open());
        Reader = emission.Reader;
    }

    public WatchKey Key => journal.Watch;
    public Seq<(StreamSlot Slot, StreamBody Body)> Facts => journal.Facts;
    public long Shed => journal.Shed;
    public long Lost => journal.Lost;
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
    private readonly StreamJournal journal;
    private readonly Reentrancy gate = new();
    private readonly Option<Channel<DocEvent>> channel;
    private readonly Option<IdlePump<EventOrigin>> idle;
    private readonly Atom<bool> active = Atom(true);

    private Emission(
        Delivery delivery,
        StreamJournal journal,
        Option<Channel<DocEvent>> channel,
        Option<IdlePump<EventOrigin>> idle) {
        this.delivery = delivery;
        this.journal = journal;
        this.channel = channel;
        this.idle = idle;
    }

    internal Option<ChannelReader<DocEvent>> Reader => channel.Map(static value => value.Reader);
    private bool IsActive => active.Value;

    internal static Fin<Emission> Open(Delivery delivery, StreamJournal journal) =>
        Admit.Need(delivery).Bind(mode => mode.Switch(
            journal,
            inline: static (state, arm) => Admit.Need(arm.Sink)
                .Map(_ => new Emission(
                    delivery: arm,
                    journal: state,
                    channel: Option<Channel<DocEvent>>.None,
                    idle: Option<IdlePump<EventOrigin>>.None)),
            deferred: static (state, arm) => Admit.Need(arm.Sink)
                .Bind(_ => IdlePump<EventOrigin>.Open(
                    capacity: Rasm.Numerics.Dimension.Create(value: state.Policy.DeferredCapacity),
                    lost: (loss, origin) => ignore(state.Post(
                        slot: loss == PumpLoss.Overflow ? StreamSlot.DeferredOverflow : StreamSlot.Cancelled,
                        body: new StreamBody.Dropped(Origin: origin)))))
                .Map(pump => new Emission(
                    delivery: arm,
                    journal: state,
                    channel: Option<Channel<DocEvent>>.None,
                    idle: Some(pump))),
            paced: static (state, arm) => Admit.Need(arm.Lane).Bind(lane =>
                Try.lift(() => {
                    Channel<DocEvent> opened = lane.Open(
                        policy: state.Policy,
                        lost: (loss, fact) => ignore(state.Post(
                            slot: StreamSlot.PacedLoss,
                            body: new StreamBody.Shed(Lane: lane, Loss: loss, Origin: fact.Origin))));
                    return Fin.Succ(value: new Emission(
                        delivery: arm,
                        journal: state,
                        channel: Some(opened),
                        idle: Option<IdlePump<EventOrigin>>.None));
                }).Run().Bind(static inner => inner))));

    internal Fin<Unit> Emit(DocEvent fact) =>
        !IsActive
            ? Fin.Succ(value: journal.Post(slot: StreamSlot.Cancelled, body: new StreamBody.Dropped(Origin: fact.Origin)))
            : delivery.Switch(
                    (Owner: this, Fact: fact),
                    inline: static (state, mode) => state.Owner.Delivered(fact: state.Fact, run: () => mode.Sink(arg: state.Fact)),
                    deferred: static (state, mode) => state.Owner.idle
                        .ToFin(new KernelFault.InvalidResult())
                        .Bind(pump => pump.Enqueue(
                            tag: state.Fact.Origin,
                            alive: () => state.Owner.IsActive,
                            run: () => state.Owner.Delivered(fact: state.Fact, run: () => mode.Sink(arg: state.Fact)))),
                    paced: static (state, _) => state.Owner.channel
                        .ToFin(new KernelFault.InvalidResult())
                        .Map(opened => opened.Writer.TryWrite(item: state.Fact)
                            ? unit
                            : state.Owner.journal.Post(
                                slot: StreamSlot.Cancelled,
                                body: new StreamBody.Dropped(Origin: state.Fact.Origin))));

    private Fin<Unit> Delivered(DocEvent fact, Func<Fin<Unit>> run) => gate.Guarded(key: EmitKey, run: run).Match(
        Some: outcome => outcome.MapFail(cause => {
            _ = journal.Post(slot: StreamSlot.SinkFault, body: new StreamBody.Faulted(Origin: Some(fact.Origin), Cause: cause));
            return cause;
        }),
        None: () => Fin.Succ(value: journal.Post(slot: StreamSlot.Reentrant, body: new StreamBody.Dropped(Origin: fact.Origin))));

    internal void Cancel() => ignore(active.Swap(static _ => false));

    internal SubscriptionRelease Complete() {
        SubscriptionRelease release = idle.Match(
            Some: static pump => pump.Close(),
            None: static () => new SubscriptionRelease.Released(Attempted: 0));
        ignore(channel.Map(static opened => opened.Writer.TryComplete()));
        return release;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class DocumentStream {
    private static readonly Atom<long> Sequence = Atom(0L);

    public static Fin<Watch> Observe(Observation request) {
        return Admit.Need(request).Bind(active => active.Switch(host: static observation => ObserveHost(request: observation),
            file: static observation => ObserveFile(request: observation)));
    }

    private static Fin<Watch> ObserveHost(Observation.Host request) =>
        from scope in Admit.Need(request.Scope)
        from delivery in Admit.Need(request.Delivery)
        from families in request.Families
            .TraverseM(family => Admit.Need(family))
            .As()
            .Map(static named => named.Distinct())
            .Bind(named => named.IsEmpty ? Fin.Fail<Seq<EventFamily>>(error: new KernelFault.InvalidInput()) : Fin.Succ(value: named))
        from _ in families.TraverseM(family => family.Cadence.Admits(delivery: delivery, family: family)).As()
        from watch in Mount(
            delivery: delivery,
            policy: request.Policy,
            attach: (emission, journal) => Attach(scope: scope, families: families, emission: emission, journal: journal))
        select watch;

    private static Fin<Watch> Mount(
        Delivery delivery,
        StreamPolicy policy,
        Func<Emission, StreamJournal, Fin<Subscription>> attach) =>
        from bounds in Admit.Need(policy)
        let journal = new StreamJournal(
            watch: WatchKey.Create(value: Sequence.Swap(static held => held + 1L)),
            policy: bounds)
        from emission in Emission.Open(delivery: delivery, journal: journal)
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
        StreamJournal journal) =>
        Subscription.AttachAll(families.Map(family => (Func<Fin<Subscription>>)(() => family.Bind(
            scope: scope,
            journal: journal,
            deliver: (key, payload) => emission.Emit(fact: new DocEvent(
                Origin: new EventOrigin.Host(Family: family), Payload: payload)),
            reject: error => ignore(journal.Post(
                slot: StreamSlot.CallbackFault,
                body: new StreamBody.Faulted(Origin: Some<EventOrigin>(new EventOrigin.Host(Family: family)), Cause: error)))))));

    private static Fin<Watch> ObserveFile(Observation.File request) =>
        from path in Acceptance.Text(value: request.Path)
        from clock in Admit.Need(request.Clock)
        from _ in guard(request.Debounce > TimeSpan.Zero, new KernelFault.InvalidInput())
        from watch in Mount(
            delivery: request.Delivery,
            policy: request.Policy,
            attach: (emission, journal) => AttachFile(
                path: path, debounce: request.Debounce, clock: clock, emission: emission, journal: journal))
        select watch;

    private static Fin<Subscription> AttachFile(
        string path,
        TimeSpan debounce,
        TimeProvider clock,
        Emission emission,
        StreamJournal journal) => Try.lift(() => {
            string fullPath = System.IO.Path.GetFullPath(path: path);
            string directory = System.IO.Path.GetDirectoryName(path: fullPath) ?? string.Empty;
            string filter = System.IO.Path.GetFileName(path: fullPath);
            if (directory.Length is 0 || filter.Length is 0 || !System.IO.Directory.Exists(path: directory)) {
                return Fin.Fail<Subscription>(error: new KernelFault.InvalidInput());
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
                        ? journal.Post(slot: StreamSlot.FileOverflow, body: new StreamBody.FileTrouble(WatchedPath: fullPath, Cause: None))
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
                Fin<Unit> Schedule() => Try.lift(() => createdTimer.Change(dueTime: debounce, period: Timeout.InfiniteTimeSpan)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: new KernelFault.InvalidResult())).Run().Bind(static inner => inner);
                Fin<Unit> Capture(FileEdge edge) {
                    _ = batch.Swap(current => current.Edges.Count < journal.Policy.FileCapacity
                        ? current with { Edges = current.Edges.Add(value: edge) }
                        : new FileBatch(Edges: current.Edges.Tail.Add(value: edge), Overflow: checked(current.Overflow + 1)));
                    return Schedule();
                }
                Fin<Unit> CaptureOverflow(Exception failure) {
                    _ = batch.Swap(current => current with { Overflow = checked(current.Overflow + 1) });
                    _ = journal.Post(
                        slot: StreamSlot.FileFault,
                        body: new StreamBody.FileTrouble(WatchedPath: fullPath, Cause: Some(Error.New(failure.Message, failure))));
                    return Schedule();
                }
                Fin<Unit> Capture(FileSystemEventArgs args) =>
                    from kind in FileChangeKind.Of(native: args.ChangeType)
                    from _ in Capture(new FileEdge(Kind: kind, Path: args.FullPath, PreviousPath: Option<string>.None))
                    select unit;
                Fin<Unit> Logged(Fin<Unit> outcome) => outcome.MapFail(error => {
                    _ = journal.Post(slot: StreamSlot.SinkFault, body: new StreamBody.Faulted(Origin: Some(origin), Cause: error));
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
            } catch (Exception failure) {
                Error primary = Error.New(failure.Message, failure);
                return Fin.Fail<Subscription>(error: primary).Rollback(
                    release: () => Custody.Release(
                        releases: Seq<Func<Fin<Unit>>>(
                            () => Optional(timer)
                                .TraverseM(live => Try.lift(() => { live.Dispose(); return Fin.Succ(unit); }).Run().Bind(static inner => inner))
                                .As().Map(static _ => unit),
                            () => Optional(watcher)
                                .TraverseM(live => Try.lift(() => { live.Dispose(); return Fin.Succ(unit); }).Run().Bind(static inner => inner))
                                .As().Map(static _ => unit))));
            }
        }).Run().Bind(static inner => inner);

    private readonly record struct FileBatch(Seq<FileEdge> Edges, long Overflow);
}
```

## [06]-[HOOK_REGISTRY]

- Owner: `RhinoPoint` is the closed boundary-wide point vocabulary addressed `rasm.rhino.<domain>.<point>`, realizing the kernel `IHookRoster<RhinoPoint>` so its rows ride the kernel modality capability set, its `HookId`, and its trace plane; `MountRegistry` owns name-addressed discovery, first-mount-wins custody, and multi-plugin arbitration OVER the kernel `HookMounts<RhinoPoint, PluginKey>` — a different concern than the kernel's composition-frozen point mount, so it carries its own name and composes the kernel seat table beneath; `PluginKey` is the plugin identity every process-global claim keys on.
- Law: a point name resolves in one hop — a consumer binds `MountRegistry.Bind` on the point and receives the owning stream's own grant (a `Watch`, `PointerLease`, `WidgetHost`, `Subscription`, or `ContentStream`) through the kernel's TYPED bind, so no consumer learns a per-domain stream API, no second delivery path forms beside the owner's bounded lanes, and no `object`/`Type` cast survives on the resolve path.
- Law: modality is host truth carried on the roster row's kernel capability set — a `Veto` row exists only where the host callback admits refusal, the veto-truth census citing the exact host member; every other point is post-hoc `Observe`, and `Replay` marks only a point whose owner retains a readable latest-value ledger. Modality ADMISSION is the kernel's: the pipeline's own `Veto` and `Observe` gates read the modality columns, so the registry carries no second gate and a binding carries no modality to check.
- Law: mount custody is one SEATED BINDING per point with keyed riders — the first mount seats the owning page's TYPED binding into the kernel seat table and registers its plugin as the first rider, every later plugin rides the same seat as a keyed subscriber, and the machinery beneath (the `ObjectsTelemetry` keyed-sink fan, the `HostTap` rider handoff, the per-plugin `Watch`) serves each rider its own grant at `Bind`. A DIVERGENT binding — a different ask or grant type against a live seat — faults `DocumentFault.SeatDiverged` because two machineries under one point fork discovery, a same-plugin duplicate rider faults `DocumentFault.RiderDuplicate`, each detacher retires exactly its own rider, and the seat frees when the last rider leaves; the type-token pair on the seat is arbitration DATA compared for divergence, never a dispatch a consumer casts through.
- Law: seat custody rides the kernel cell vocabulary — the claim is `Cell.Claim` whose verdict rides the transition, a rider joins through a snapshot-guarded `Cell.Step`, and the surplus kernel mount a ceded claim staged releases on the losing arm; no stored verdict cell exists to go stale.
- Law: `MountAll` releases an admitted prefix when any later row refuses, through the one `Rollback` fold — every disposer runs, reverse order, cleanup faults aggregating into the primary.
- Law: telemetry is a tap — the `rasm.rhino.objects.fault` point binds onto the `ObjectsTelemetry` keyed-sink fan, and the `rasm.rhino.host.exception`/`rasm.rhino.host.log` points bind the `HostUtils.OnExceptionReport` and `HostUtils.OnSendLogMessageToCloud` statics onto the same fan through the `HostTap.Mount` seat, so observability subscribes to domain facts and no emit call rides inside domain code.
- Law: process-global custody is a closed census — every collision surface carries its collision class and arbitration row below, and a new process-global surface is one census row with its arbitration named before any fence composes it.
- Growth: a new fact stream is one `RhinoPoint` row with its typed `HookBinding` registration on its owning page; a new plugin-visible custody surface is one census row.

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
|  [01]   | `rasm.rhino.display.pointer`    | `PointerFact` | observe, veto   | `DisplayHooks.Mount` grants `PointerLease`               |
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

`GumballRig` returns transform evidence directly from move and completion, while the pointer point carries gumball occupancy per fact.

Process-global custody census — collision class, arbitration, and seat cardinality per surface; `fan` seats one row per plugin, `single` seats one process-wide owner later callers ride or fault against, `host` defers cardinality to a host-native mechanism:

| [INDEX] | [SURFACE]                                  | [COLLISION_CLASS]                    | [ARBITRATION]                            | [SEATS] |
| :-----: | :----------------------------------------- | :----------------------------------- | :--------------------------------------- | :------ |
|  [01]   | `RhinoDoc`/`RhinoView`/`DisplayPipeline`   | duplicate watches double facts       | per-plugin `Watch`; delegate identity    | fan     |
|  [02]   | `RhinoApp.Idle` (`IdlePump`)               | multicast-safe                       | one pump per deferred watch              | fan     |
|  [03]   | `HostUtils` exception / cloud-log statics  | duplicate mounts double-publish      | `HostTap.Mount`; first mount, later ride | single  |
|  [04]   | `ObjectsTelemetry` sink                    | replacement shadows a prior plugin   | `PluginKey` rows; teardown per caller    | fan     |
|  [05]   | `HostUtils.RegisterNamedCallback`          | re-register replaces the handler     | `PluginKey` claim tokens, keyed per name | single  |
|  [06]   | `Panels.RegisterPanel` / page registration | host isolates; a ledger crosses      | host-native seats; `PluginKey` ledger    | host    |
|  [07]   | `CustomObjectGrips.RegisterGripsEnabler`   | re-register replaces per grips guid  | one enabler per `[Guid]` grips type      | host    |
|  [08]   | `AssemblyResolver` search mutations        | additive process list, unremovable   | rows via `HostAssemblies.Extend`         | fan     |
|  [09]   | `AppSettings.Commit` static families       | last-writer-wins process mutation    | `AppSettings.Mount` writer seat          | single  |
|  [10]   | `MarshalLatency` seat                      | a second provider splits the ledger  | first-mount-wins; detacher returns it    | single  |
|  [11]   | `MountRegistry` mounts                     | divergent bindings fork discovery    | one seated binding; divergence faults    | fan     |
|  [12]   | `RhinoDoc.AddCustomUndoEvent`              | handler graph retained until cleared | record-scoped; no host detach exists     | host    |

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<Guid>]
public readonly partial struct PluginKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Plugin identity is empty.") : null;

    internal static Option<PluginKey> Maybe(Guid value) =>
        Optional(value).Filter(static id => id != Guid.Empty).Map(Create);

    internal Fin<Unit> Admit() {
        ValidationError? fault = Validate(value: ToValue(), provider: null, out PluginKey? admitted);
        return FactoryBridge.Lift<PluginKey>(fault: fault, admitted: admitted).Map(static _ => unit);
    }
}

[SmartEnum<string>]
public sealed partial class RhinoPoint : IHookRoster<RhinoPoint> {
    public static readonly RhinoPoint DocumentLifecycle = new(key: "rasm.rhino.document.lifecycle", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DocumentStructure = new(key: "rasm.rhino.document.structure", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DocumentSelection = new(key: "rasm.rhino.document.selection", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DocumentTables = new(key: "rasm.rhino.document.tables", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DocumentScreen = new(key: "rasm.rhino.document.screen", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DocumentDraw = new(key: "rasm.rhino.document.draw", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DocumentPanels = new(key: "rasm.rhino.document.panels", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DocumentFile = new(key: "rasm.rhino.document.file", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DisplayPointer = new(key: "rasm.rhino.display.pointer", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe, HookModality.Veto));
    public static readonly RhinoPoint DisplayWidget = new(key: "rasm.rhino.display.widget", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint DisplayCull = new(key: "rasm.rhino.display.cull", modalities: CapabilitySet<HookModality>.Of(HookModality.Veto));
    public static readonly RhinoPoint DisplayDrawObject = new(key: "rasm.rhino.display.drawobject", modalities: CapabilitySet<HookModality>.Of(HookModality.Veto));
    public static readonly RhinoPoint ObjectsViewable = new(key: "rasm.rhino.objects.viewable", modalities: CapabilitySet<HookModality>.Of(HookModality.Veto));
    public static readonly RhinoPoint ObjectsPick = new(key: "rasm.rhino.objects.pick", modalities: CapabilitySet<HookModality>.Of(HookModality.Veto));
    public static readonly RhinoPoint ObjectsRegrow = new(key: "rasm.rhino.objects.regrow", modalities: CapabilitySet<HookModality>.Of(HookModality.Veto));
    public static readonly RhinoPoint ObjectsFault = new(key: "rasm.rhino.objects.fault", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint CommandBegin = new(key: "rasm.rhino.command.begin", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint CommandEnd = new(key: "rasm.rhino.command.end", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint CommandUndo = new(key: "rasm.rhino.command.undo", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint CommandPrompt = new(key: "rasm.rhino.command.prompt", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint CommandEscape = new(key: "rasm.rhino.command.escape", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint HostException = new(key: "rasm.rhino.host.exception", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint HostCloudLog = new(key: "rasm.rhino.host.log", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint HostUiPanel = new(key: "rasm.rhino.hostui.panel", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe, HookModality.Replay));
    public static readonly RhinoPoint HostUiSkin = new(key: "rasm.rhino.hostui.skin", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly RhinoPoint RenderContent = new(key: "rasm.rhino.render.content", modalities: CapabilitySet<HookModality>.Of(HookModality.Observe));

    public CapabilitySet<HookModality> Modalities { get; }

    public HookId Id => Ids.Value[this];

    public Option<TraceScope> Plane => Some(Planes.Value[this]);

    private static readonly Lazy<FrozenDictionary<RhinoPoint, HookId>> Ids =
        new(static () => Items.ToFrozenDictionary(static row => row, static row => HookId.Create(value: row.Key)));
    private static readonly Lazy<FrozenDictionary<RhinoPoint, TraceScope>> Planes =
        new(static () => Items.ToFrozenDictionary(
            static row => row,
            static row => TraceScope.Create(value: string.Join('.', row.Key.Split('.').Take(3)))));
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class MountRegistry {
    private static readonly HookMounts<RhinoPoint, PluginKey> Kernel = new();

    internal sealed record PointSeat(PluginKey Owner, Type Ask, Type Grant, HashMap<Guid, Unit> Riders, Lease<IDisposable> Mounted);

    private static readonly Atom<HashMap<string, PointSeat>> Seats = Atom(HashMap<string, PointSeat>());

    public static Seq<(RhinoPoint Point, PluginKey Owner, Seq<PluginKey> Riders)> Census =>
        Seats.Value.AsIterable().ToSeq().Choose(static row => RhinoPoint.TryGet(row.Key, out RhinoPoint? point)
            ? Some((Point: point!, Owner: row.Value.Owner, Riders: toSeq(row.Value.Riders.Keys).Map(PluginKey.Create).Strict()))
            : None);

    public static Fin<IDisposable> Mount<TAsk, TGrant>(HookBinding<RhinoPoint, PluginKey, TAsk, TGrant> binding)
        where TAsk : notnull
        where TGrant : notnull {
        return from _ in binding.Owner.Admit()
               let plugin = binding.Owner.ToValue()
               from mounted in Kernel.Mount(binding: binding)
               from seat in Cell.Claim(
                       cell: Seats,
                       mint: () => new PointSeat(
                           Owner: binding.Owner,
                           Ask: typeof(TAsk),
                           Grant: typeof(TGrant),
                           Riders: HashMap((plugin, unit)),
                           Mounted: mounted))
                   .Switch(
                       state: (Point: binding.Point, Plugin: plugin, Mounted: mounted),
                       committed: static (ctx, _) => Fin.Succ((IDisposable)Subscription.Of(
                           detach: () => Unseat(pointKey: ctx.Point.Key, plugin: ctx.Plugin))),
                       ceded: static (ctx, held) => (HostEdge.Side(ctx.Mounted.Dispose), Ride<TAsk, TGrant>(
                           seat: held.State[ctx.Point.Key], point: ctx.Point, plugin: ctx.Plugin)).Item2,
                       refused: static (ctx, row) => Fin.Fail<IDisposable>(row.Cause),
                       contended: static (ctx, _) => Fin.Fail<IDisposable>(new KernelFault.InvalidResult()))
               select seat;
    }

    public static Fin<Seq<IDisposable>> MountAll(Seq<Func<Fin<IDisposable>>> mounts) {
        return mounts.FoldM<Fin, Seq<IDisposable>>(
            Seq<IDisposable>(),
            (held, mount) => Admit.Need(mount)
                .Bind(run => Try.lift(run).Run().Bind(static inner => inner))
                .Map(seat => held.Add(seat))
                .Rollback(held: held, release: seat => Try.lift(() => { seat.Dispose(); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner)));
    }

    public static Fin<TGrant> Bind<TAsk, TGrant>(RhinoPoint point, TAsk ask)
        where TAsk : notnull
        where TGrant : notnull {
        return from active in Admit.Need(point)
               from seat in Seats.Value.Find(active.Key).ToFin(Fail: new KernelFault.MissingContext())
               from grant in Kernel.Bind<TAsk, TGrant>(point: active, owner: seat.Owner, ask: ask)
               select grant;
    }

    private static Fin<IDisposable> Ride<TAsk, TGrant>(PointSeat seat, RhinoPoint point, Guid plugin) =>
        seat.Ask != typeof(TAsk) || seat.Grant != typeof(TGrant)
            ? Fin.Fail<IDisposable>(new DocumentFault.SeatDiverged(Point: point))
            : seat.Riders.ContainsKey(plugin)
                ? Fin.Fail<IDisposable>(new DocumentFault.RiderDuplicate(Point: point, Plugin: PluginKey.Create(plugin)))
                : Cell.Step(
                        cell: Seats,
                        step: held => held.Find(point.Key).Bind(current => current.Riders.ContainsKey(plugin)
                            ? Option<HashMap<string, PointSeat>>.None
                            : Some(held.SetItem(point.Key, current with { Riders = current.Riders.Add(plugin, unit) }))),
                        declined: new DocumentFault.RiderDuplicate(Point: point, Plugin: PluginKey.Create(plugin)))
                    .Switch(
                        state: (Point: point, Plugin: plugin),
                        committed: static (ctx, _) => Fin.Succ((IDisposable)Subscription.Of(
                            detach: () => Unseat(pointKey: ctx.Point.Key, plugin: ctx.Plugin))),
                        ceded: static (ctx, _) => Fin.Fail<IDisposable>(new DocumentFault.RiderDuplicate(Point: ctx.Point, Plugin: PluginKey.Create(ctx.Plugin))),
                        refused: static (_, row) => Fin.Fail<IDisposable>(row.Cause),
                        contended: static (ctx, _) => Fin.Fail<IDisposable>(new KernelFault.InvalidResult()));

    private static Unit Unseat(string pointKey, Guid plugin) {
        Option<PointSeat> prior = Seats.Value.Find(pointKey);
        _ = Seats.Swap(held => held.Find(pointKey).Match(
            None: () => held,
            Some: seat => seat.Riders.Remove(plugin) is var remaining && remaining.IsEmpty
                ? held.Remove(pointKey)
                : held.SetItem(pointKey, seat with { Riders = remaining })));
        return prior.Filter(_ => !Seats.Value.ContainsKey(pointKey)).Map(seat => HostEdge.Side(seat.Mounted.Dispose)).IfNone(unit);
    }
}

public static class DocumentHooks {
    public static Fin<Seq<IDisposable>> Mount(PluginKey plugin) {
        Seq<Func<Fin<IDisposable>>> mounts = Seq(
                (Point: RhinoPoint.DocumentLifecycle, Band: EventBand.Lifecycle),
                (Point: RhinoPoint.DocumentStructure, Band: EventBand.Structure),
                (Point: RhinoPoint.DocumentSelection, Band: EventBand.Selection),
                (Point: RhinoPoint.DocumentTables, Band: EventBand.Tables),
                (Point: RhinoPoint.DocumentScreen, Band: EventBand.Screen),
                (Point: RhinoPoint.DocumentDraw, Band: EventBand.Draw),
                (Point: RhinoPoint.DocumentPanels, Band: EventBand.Panels))
            .Map(row => (Func<Fin<IDisposable>>)(() => MountRegistry.Mount(
                binding: new HookBinding<RhinoPoint, PluginKey, Observation.Host, Watch>(
                    Point: row.Point,
                    Owner: plugin,
                    Bind: ask => EventFamily.In(band: row.Band)
                        .Bind(families => DocumentStream.Observe(ask with { Families = families }))))))
            .Add(() => MountRegistry.Mount(
                binding: new HookBinding<RhinoPoint, PluginKey, Observation.File, Watch>(
                    Point: RhinoPoint.DocumentFile,
                    Owner: plugin,
                    Bind: static ask => DocumentStream.Observe(ask))));
        return MountRegistry.MountAll(mounts: mounts);
    }
}
```

## [07]-[TELEMETRY_TAP]

- Owner: `RhinoInstruments` — the boundary's contributed instrument rows in the kernel `InstrumentSpec` shape and the string-scoped `TelemetryContributorPort` mint under scope `Rasm.Rhino`.
- Cases: stream-loss counts off the `StreamSlot.PacedLoss` journal posts by lane and loss kind; delivered document facts by band off each mounted watch; object callback and egress faults by site and code off `ObjectsTelemetry`; host exception and cloud-log observations off the two `HostTap.Mount` points; pointer submissions and rejections off the `PointerLease` counters; panel changes off `PanelHost.Facts`; content pulses and stream failures off `ContentStream`; marshal-boundary durations off `MarshalLatency`; the document census levels off `DocumentCensus`; bench durations and allocations off `BenchEvidence`.
- Entry: `RhinoInstruments.Telemetry(string version)` — the one contributor port an app composition merges by scope, its semconv coordinate the kernel `TelemetryIdentity.SchemaUrl` const the mint stamps; the plugin root materializes the rows over its own per-ALC factory meter through `InstrumentSet.Of`, and one custody per composition holds — either the port rides an app fan or the root materializes locally, never both.
- Auto: writes ride observe taps composed at the plugin root — the loss counter's WRITER is the paced lane's shed arm (the `lost` callback posting `StreamSlot.PacedLoss`, which the tap folds by lane and loss dimension), the delivery sink feeds the band counter, and the host-tap points feed the two observation counters — so no stream, projection, or mount fence carries a meter call and the shed evidence is measured, never inferred; every other row's writer is likewise the producing owner's own hook fact or pulled level, dimensioned by the row's declared slots, so a row names only a dimension its source already carries.
- Packages: `Rasm` (kernel signal capsule — `InstrumentSpec`, `TelemetryContributorPort`, and the `Sensitivity` taxonomy roster this port stamps whole), BCL inbox (`System.Diagnostics.Metrics`).
- Growth: one measured boundary concern is one `InstrumentSpec` factory call here and one observe-tap write at the plugin root.
- Boundary: rows carry dotted `rasm.rhino.*` names with UCUM units and closed dimensions; this roster is the boundary's ONE instrument declaration, instrument execution over it is app-root altitude, never a second measurement truth inside the boundary, and provider custody stays with the per-ALC factory owner. The kernel `Sensitivity.Values` roster rides the port's `Classifications` column whole, so every value `Objects/authoring.md`'s four attach attributes stamp is rostered at composition and a value present at the producer and absent at the root refuses at admission instead of erasing at egress.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
public static class RhinoInstruments {
    public const string Scope = "Rasm.Rhino";
    public const string StreamLoss = "rasm.rhino.stream.loss";
    public const string DocumentEvents = "rasm.rhino.document.events";
    public const string CallbackFaults = "rasm.rhino.objects.callback.faults";
    public const string EgressFaults = "rasm.rhino.objects.egress.faults";
    public const string HostExceptions = "rasm.rhino.host.exceptions";
    public const string HostLogs = "rasm.rhino.host.logs";
    public const string PointerSubmitted = "rasm.rhino.display.pointer.submitted";
    public const string PointerRejected = "rasm.rhino.display.pointer.rejected";
    public const string PanelChanges = "rasm.rhino.hostui.panel.facts";
    public const string MarshalDuration = "rasm.rhino.hostui.marshal.duration";
    public const string ContentPulses = "rasm.rhino.render.content.facts";
    public const string ContentFailures = "rasm.rhino.render.content.failures";
    public const string CensusObjects = "rasm.rhino.document.census.objects";
    public const string CensusSpaces = "rasm.rhino.document.census.spaces";
    public const string CensusLayers = "rasm.rhino.document.census.layers";
    public const string CensusPlacements = "rasm.rhino.document.census.placements";
    public const string CensusArchive = "rasm.rhino.document.census.archive.size";
    public const string BenchDuration = "rasm.rhino.bench.duration";
    public const string BenchAllocated = "rasm.rhino.bench.allocated";

    public const string LaneSlot = "lane";
    public const string LossSlot = "loss";
    public const string BandSlot = "band";
    public const string SiteSlot = "site";
    public const string CodeSlot = "code";
    public const string SourceSlot = "source";
    public const string LevelSlot = "level";
    public const string ClassSlot = "class";
    public const string PluginSlot = "plugin";
    public const string PanelSlot = "panel";
    public const string ChangeSlot = "change";
    public const string DocumentSlot = "document";
    public const string PulseSlot = "pulse";
    public const string WorkSlot = "work";
    public const string OutcomeSlot = "outcome";
    public const string KindSlot = "kind";
    public const string SpaceSlot = "space";
    public const string OperationSlot = "operation";
    public const string ScaleSlot = "scale";

    public static readonly Seq<InstrumentSpec> Rows = Seq(
        InstrumentSpec.Create(StreamLoss, InstrumentKind.Count, MeasureForm.Whole, "{fact}",
            "paced-lane facts shed by lane and loss kind", Seq(LaneSlot, LossSlot), None, None, None),
        InstrumentSpec.Create(DocumentEvents, InstrumentKind.Count, MeasureForm.Whole, "{event}",
            "delivered document facts by band", Seq(BandSlot), None, None, None),
        InstrumentSpec.Create(CallbackFaults, InstrumentKind.Count, MeasureForm.Whole, "{fault}",
            "host-callback faults published by site and code", Seq(SiteSlot, CodeSlot), None, None, None),
        InstrumentSpec.Create(EgressFaults, InstrumentKind.Count, MeasureForm.Whole, "{fault}",
            "telemetry egress sink faults drained by code", Seq(CodeSlot), None, None, None),
        InstrumentSpec.Create(HostExceptions, InstrumentKind.Count, MeasureForm.Whole, "{exception}",
            "host exception reports observed through the host tap", Seq(SiteSlot, SourceSlot), None, None, None),
        InstrumentSpec.Create(HostLogs, InstrumentKind.Count, MeasureForm.Whole, "{message}",
            "host cloud-log messages observed through the host tap", Seq(LevelSlot, ClassSlot), None, None, None),
        InstrumentSpec.Create(PointerSubmitted, InstrumentKind.Count, MeasureForm.Whole, "{fact}",
            "viewport pointer facts submitted to the lease channel", Seq<string>(), None, None, None),
        InstrumentSpec.Create(PointerRejected, InstrumentKind.Count, MeasureForm.Whole, "{fact}",
            "viewport pointer facts the lease channel refused", Seq<string>(), None, None, None),
        InstrumentSpec.Create(PanelChanges, InstrumentKind.Count, MeasureForm.Whole, "{fact}",
            "panel lifecycle facts by plugin, panel, change, and document", Seq(PluginSlot, PanelSlot, ChangeSlot, DocumentSlot), None, None, None),
        InstrumentSpec.Create(MarshalDuration, InstrumentKind.Distribution, MeasureForm.Real, "s",
            "command-thread marshal latency by work, lane, and outcome", Seq(WorkSlot, LaneSlot, OutcomeSlot), None, None, None),
        InstrumentSpec.Create(ContentPulses, InstrumentKind.Count, MeasureForm.Whole, "{fact}",
            "render-content stream facts by pulse and document", Seq(PulseSlot, DocumentSlot), None, None, None),
        InstrumentSpec.Create(ContentFailures, InstrumentKind.Count, MeasureForm.Whole, "{fault}",
            "render-content stream failures by pulse", Seq(PulseSlot), None, None, None),
        InstrumentSpec.Create(CensusObjects, InstrumentKind.Level, MeasureForm.Whole, "{object}",
            "document objects by kind", Seq(KindSlot, DocumentSlot), None, None, None),
        InstrumentSpec.Create(CensusSpaces, InstrumentKind.Level, MeasureForm.Whole, "{object}",
            "document objects by space", Seq(SpaceSlot, DocumentSlot), None, None, None),
        InstrumentSpec.Create(CensusLayers, InstrumentKind.Level, MeasureForm.Whole, "{layer}",
            "document layer count", Seq(DocumentSlot), None, None, None),
        InstrumentSpec.Create(CensusPlacements, InstrumentKind.Level, MeasureForm.Whole, "{placement}",
            "block placements in the document", Seq(DocumentSlot), None, None, None),
        InstrumentSpec.Create(CensusArchive, InstrumentKind.Level, MeasureForm.Whole, "By",
            "on-disk archive size of the document", Seq(DocumentSlot), None, None, None),
        InstrumentSpec.Create(BenchDuration, InstrumentKind.Distribution, MeasureForm.Real, "s",
            "modeling bench duration by operation and input scale", Seq(OperationSlot, ScaleSlot), None, None, None),
        InstrumentSpec.Create(BenchAllocated, InstrumentKind.Distribution, MeasureForm.Whole, "By",
            "modeling bench allocation by operation and input scale", Seq(OperationSlot, ScaleSlot), None, None, None));

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: Scope, Version: version, Instruments: Rows, Classifications: Sensitivity.Values,
            Keyed: toSeq(Items).Map(static row => (row.Key, row.Row.Name)));
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
