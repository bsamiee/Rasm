# [RASM_GRASSHOPPER_SHELL_EVENTS]

`UiEvents` is the one UI event algebra of the Grasshopper boundary — a single source-row vocabulary (`UiSource`) over every GH2 and Eto event stream, one typed fact family (`UiFact`) carrying every payload as evidence, one anchor union (`EventAnchor`) discriminating what a row attaches to, and one subscription owner (`UiSubscription`) whose lifetime rides the kernel `Lease<T>` rail. A host event family is a set of ROWS in one vocabulary, its payload is a CASE of one fact union, attachment and inverse detachment are one delegate column, and ownership is the kernel lease with no second in-folder lifecycle algebra. Every attachment rides an `Op`-keyed `Fin` rail and marshals through the `Eto/runtime.md` dispatch seam; a batch of rows attaches transactionally — any refused row rolls back every already-attached sibling before the fault surfaces.

Source coverage spans the GH2 canvas signal family (`DocumentChanged`/`DocumentModified` on `Canvas`, `ProjectionChanged`/`WindowSelection`/`MouseDwell`/`Draw` inherited from the `FlexControl` seam), the document family (`ModifiedChanged`/`StateChanged`/`ParentChanged`), the full ten-event object-list family (`ObjectAdded`/`ObjectRemoved`/`ObjectSelectionChanged`/`ObjectExpired`/`ObjectNameChanged`/`ObjectEnabledChanged`/`ObjectRelevanceChanged`/`ObjectLayoutChanged`/`ObjectDisplayChanged`/`ObjectInstanceIdChanged`), the six-event solution lifecycle, the full seven-event undo family (`Undone`/`Redone`/`Modified`/`NodeAdded`/`NodeRemoved`/`NodeMerged`/`NodeMoved`), the Eto control input/lifecycle families, the window family, ambient application/keyboard state, and the `UiClock` beat.

Eight canvas paint fences are `Canvas/paint.md`'s executor and never appear as rows here; `FlexControl.PopulateContextMenu` is `Canvas/interaction.md`'s policy callback, not an observation stream; window-closing veto policy is `Eto/windows.md`'s spec; AppKit local-monitor streams enter this algebra as rows whose attachment is the `Platform/native.md` gated adapter — the platform owner carries the `NSEventMask` vocabulary, the monitor lifetime, and the macOS gate, and this page carries only the row seam.

## [01]-[INDEX]

- [02]-[FACTS]: `CanvasSignal`/`DocumentSignal`/`GraphSignal`/`SolutionSignal`/`UndoSignal`/`LifecycleStage` + `UiFact` + `UiEvent` — the signal vocabularies, the one payload union, and the published envelope.
- [03]-[SOURCES]: `EventAnchor` + `UiSource` — the anchor union and the source-row vocabulary with its attach/detach column and per-anchor row-factory folds.
- [04]-[SUBSCRIPTION]: `UiSubscription` + `UiEvents` — transactional multi-row attachment behind one `Observe` gate returning `Fin<Lease<UiSubscription>>`.
- [05]-[DRAIN]: `DrainPolicy` + `EvidenceDrain` — the bounded off-thread evidence channel with drop accounting and the publish-callback bridge.

## [02]-[FACTS]

- Owner: `UiFact` `[Union]` — the one payload family every source row publishes into. Cases carry typed evidence, never provider argument objects: `PointerCase` (location, buttons, modifiers, wheel delta, pressure — the full `MouseEventArgs` evidence set), `KeyCase` (key, modifiers, pressed, composed character), `TextCase` (composed text), `DragCase` (location, effect mask), `FocusCase` (gained), `EnabledCase` (live enabled state), `BoundsCase` (bounds), `DensityCase` (logical pixel size), `StateCase` (window state), `LifeCase` (a `LifecycleStage` row), `ModifierCase` (live modifier mask), `CanvasCase` (signal row with the dwell content point where the raising event carries one), `DocumentCase` (signal row with the owning document's `DocumentToken` id), `GraphCase` (signal row with the subject's `InstanceId`), `SolutionCase` (signal row, the host `SolutionId` run identity, and the `Faulted` row's `Error`), `UndoCase` (signal row), `BeatCase` (a `ClockBeat`), `NoticeCase` (activation id and user data, both host strings), `FaultCase` (an `Error` from the unhandled-exception stream, lowered through the kernel fault vocabulary). Signal vocabularies are keyless behavior-free `[SmartEnum<int>]` rows — `CanvasSignal` (6), `DocumentSignal` (3), `GraphSignal` (10, the full `ObjectList` event set), `SolutionSignal` (6, in lifecycle order `AboutToStart`→`Started`→`Stopped`→`Cancelled`→`Completed`→`Faulted`), `UndoSignal` (7, the full `History` event set), `LifecycleStage` (Eto `PreLoad`/`Load`/`LoadComplete`/`Shown`/`UnLoad`/`Closing`/`Closed` and app `Initialized`/`Terminating`).
- Owner: `UiEvent` readonly record struct — the published envelope carries the raising `UiSource`, `UiFact`, and sink-local `Stamp` publication ordinal; it implements `IValidityEvidence` through the claim fold and validates nested `ClockBeat` evidence when the fact is `BeatCase`. Consumers receive `UiEvent` values only — a host `EventArgs` object never crosses the algebra. `Stamp` proves serialized event order, carries no elapsed-time meaning, and never competes with `MonotonicTimeline`.
- Law: facts are evidence, never live resources — a `DocumentCase` carries the `DocumentToken` `Guid` (`Shell/session.md`'s per-instance identity mint, because GH2 `Document` carries no cheap host id), never the `Document`; a `GraphCase` carries `IDocumentObject.InstanceId`; a `SolutionCase` carries the host `SolutionId` value identity; a consumer needing the live object re-enters through `GhSession.Run`, so a stale fact can never hand out a disposed host reference.
- Law: sparse projection is the contract by decision — a fact carries what its source row verifiably reads, and a consumer needing more evidence extends the CASE with a field, never mints a sibling snapshot record.
- Boundary: `Fault.Cancelled` and the twelve-case kernel fault band own failure semantics; `FaultCase` transports the `Error` as evidence and adjudicates nothing.
- Packages: Eto (`Keys`, `MouseButtons`, `DragEffects`, `WindowState`, `PointF`, `RectangleF`), `Rasm.Domain` (`ValidityClaim`, `IValidityEvidence`), `Eto/runtime.md` (`ClockBeat`), `Rasm.Parametric` (`MonotonicBeat`).
- Growth: a new host signal is one row on its signal vocabulary and, where the payload is new, one `UiFact` case; consumers' total `Switch` breaks loudly.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rasm.Grasshopper.Eto;

namespace Rasm.Grasshopper.Shell;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CanvasSignal {
    public static readonly CanvasSignal DocumentChanged = new(key: 0);
    public static readonly CanvasSignal ProjectionChanged = new(key: 1);
    public static readonly CanvasSignal WindowSelection = new(key: 2);
    public static readonly CanvasSignal DocumentModified = new(key: 3);
    public static readonly CanvasSignal MouseDwell = new(key: 4);
    public static readonly CanvasSignal Draw = new(key: 5);
}

[SmartEnum<int>]
public sealed partial class DocumentSignal {
    public static readonly DocumentSignal Modified = new(key: 0);
    public static readonly DocumentSignal State = new(key: 1);
    public static readonly DocumentSignal Parent = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class GraphSignal {
    public static readonly GraphSignal ObjectAdded = new(key: 0);
    public static readonly GraphSignal ObjectRemoved = new(key: 1);
    public static readonly GraphSignal SelectionChanged = new(key: 2);
    public static readonly GraphSignal Expired = new(key: 3);
    public static readonly GraphSignal NameChanged = new(key: 4);
    public static readonly GraphSignal EnabledChanged = new(key: 5);
    public static readonly GraphSignal RelevanceChanged = new(key: 6);
    public static readonly GraphSignal LayoutChanged = new(key: 7);
    public static readonly GraphSignal DisplayChanged = new(key: 8);
    public static readonly GraphSignal InstanceIdChanged = new(key: 9);
}

[SmartEnum<int>]
public sealed partial class SolutionSignal {
    public static readonly SolutionSignal AboutToStart = new(key: 0);
    public static readonly SolutionSignal Started = new(key: 1);
    public static readonly SolutionSignal Stopped = new(key: 2);
    public static readonly SolutionSignal Cancelled = new(key: 3);
    public static readonly SolutionSignal Completed = new(key: 4);
    public static readonly SolutionSignal Faulted = new(key: 5);
}

[SmartEnum<int>]
public sealed partial class UndoSignal {
    public static readonly UndoSignal Undone = new(key: 0);
    public static readonly UndoSignal Redone = new(key: 1);
    public static readonly UndoSignal Modified = new(key: 2);
    public static readonly UndoSignal NodeAdded = new(key: 3);
    public static readonly UndoSignal NodeRemoved = new(key: 4);
    public static readonly UndoSignal NodeMerged = new(key: 5);
    public static readonly UndoSignal NodeMoved = new(key: 6);
}

[SmartEnum<int>]
public sealed partial class LifecycleStage {
    public static readonly LifecycleStage PreLoad = new(key: 0);
    public static readonly LifecycleStage Load = new(key: 1);
    public static readonly LifecycleStage LoadComplete = new(key: 2);
    public static readonly LifecycleStage Shown = new(key: 3);
    public static readonly LifecycleStage UnLoad = new(key: 4);
    public static readonly LifecycleStage Closing = new(key: 5);
    public static readonly LifecycleStage Closed = new(key: 6);
    public static readonly LifecycleStage Initialized = new(key: 7);
    public static readonly LifecycleStage Terminating = new(key: 8);
}

[Union]
public abstract partial record UiFact {
    private UiFact() { }
    public sealed record PointerCase(PointF Location, MouseButtons Buttons, Keys Modifiers, SizeF Delta, float Pressure) : UiFact;
    public sealed record KeyCase(Keys Key, Keys Modifiers, bool Pressed, Option<char> Character) : UiFact;
    public sealed record TextCase(string Text) : UiFact;
    public sealed record DragCase(PointF Location, DragEffects Effects) : UiFact;
    public sealed record FocusCase(bool Gained) : UiFact;
    public sealed record EnabledCase(bool Enabled) : UiFact;
    public sealed record BoundsCase(RectangleF Bounds) : UiFact;
    public sealed record DensityCase(float LogicalPixelSize) : UiFact;
    public sealed record StateCase(WindowState State) : UiFact;
    public sealed record LifeCase(LifecycleStage Stage) : UiFact;
    public sealed record ModifierCase(Keys Modifiers) : UiFact;
    public sealed record CanvasCase(CanvasSignal Signal, Option<PointF> Location) : UiFact;
    public sealed record DocumentCase(DocumentSignal Signal, Option<Guid> DocumentId) : UiFact;
    public sealed record GraphCase(GraphSignal Signal, Option<Guid> SubjectId) : UiFact;
    public sealed record SolutionCase(SolutionSignal Signal, Option<SolutionId> Id, Option<Error> Failure) : UiFact;
    public sealed record UndoCase(UndoSignal Signal) : UiFact;
    public sealed record BeatCase(ClockBeat Beat) : UiFact;
    public sealed record NoticeCase(Option<string> Id, Option<string> UserData) : UiFact;
    public sealed record FaultCase(Error Failure) : UiFact;
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct UiEvent(UiSource Source, UiFact Fact, long Stamp) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: Source is not null),
        ValidityClaim.Of(holds: Fact is not null),
        ValidityClaim.Of(holds: Stamp >= 0L),
        ValidityClaim.Of(holds: Fact is not UiFact.BeatCase beat || beat.Beat.IsValid));
    internal static UiEvent Of(UiSource source, UiFact fact, long stamp) => new(Source: source, Fact: fact, Stamp: stamp);
}
```

## [03]-[SOURCES]

- Owner: `EventAnchor` `[Union]` — what a row attaches to: `CanvasCase(Canvas)`, `DocumentCase(Document)`, `SolutionCase(SolutionServer)`, `HistoryCase(History)`, `ControlCase(Control)`, `WindowCase(Window)`, `AmbientCase` (the `Application`/`Keyboard` statics), `ClockCase(UiClock)`. A row demanding one anchor kind refuses every other with `Fault.InvalidInput` — anchor agreement is admission, not documentation.
- Owner: `UiSource` `[SmartEnum<string>]` — the source-row vocabulary, string-keyed for wire/diagnostic identity, over ONE `[UseDelegateFromConstructor]` `Attach(EventAnchor, EventSink, Op)` column returning `Fin<IDisposable>` whose disposable IS the inverse detachment. Rows are constructed by per-anchor factory folds so no row hand-rolls its subscription mechanics: `CanvasRow`/`DocumentRow`/`SolutionRow`/`HistoryRow` wire the GH2 families, `ControlRow`/`WindowRow` wire the Eto families, `AmbientRow` wires application and keyboard statics, and the clock row bridges the `UiClock` beat. Rows whose family differs only in which host event the wire touches collapse through shared-args sub-folds — `Pointer`/`Keystroke`/`DragRow`/`Stage` on the Eto side, `Subject` (the six `ObjectEventArgs` object-list rows), `Pulse` (the four `SolutionEventArgs` lifecycle rows), and `Ledger`/`LedgerNode` (the `UndoEventArgs`/`UndoNodeEventArgs` triplets) on the GH2 side — the event choice is row data, never a sibling body.
- Entry: rows are data — the only executable surface is the internal `Attach` column the `[04]` gate drains; no per-row public subscribe method exists.
- Law: every host-event attach body registers a stored handler and returns a `Detachment` capturing the exact `-=` inverse; the clock row returns `UiClock.Tap`'s token handle directly. A subscription without its inverse is unconstructible.
- Law: `EventSink` contains the entire deferred callback — host-argument projection, fact construction, envelope construction, and consumer publication cross one `Op.Catch`, and the exact failure persists on `UiSubscription.LastFault` without escaping into the raising host while emitting once through the generated `UiEventsLog.PublicationFault` partial under the `GhLog.For` category logger, so a retained fault is observable without polling the cell. Publication never re-enters the host; consumer mutation re-enters through `GhSession`, so event storms cannot recurse into the surface that raised them.
- Law: `ClockBeats` attaches through `UiClock.Tap` and returns that token-addressed observer handle as its sole detachment. Observation never starts or stops the shared clock; the clock lease owner controls lifecycle, multiple subscriptions coexist independently, and `BeatCase` carries the exact `ClockBeat`/`MonotonicBeat` emitted by the runtime without sampling another clock.
- Law: every wire spells its host delegate exactly — `Canvas.DocumentChanged`/`DocumentModified` are bare `EventHandler`; the flex-seam four carry typed args (`ProjectionChangedEventArgs`, `WindowSelectionEventArgs`, `MouseDwellEventArgs` with `ControlPoint`/`ContentPoint`, `ControlDrawEventArgs`); the document three carry `DocumentModifiedEventArgs`/`DocumentStateEventArgs`/`BeforeAfterEventArgs<Document, IDocumentParent>`; the object-list ten carry `AfterAddObjectEventArgs`/`AfterRemoveObjectEventArgs`/`ObjectEventArgs`/`ObjectNameEventArgs`/`ObjectGuidEventArgs`; the solution six carry `SolutionIdEventArgs`/`SolutionEventArgs`/`SolutionExceptionEventArgs`; the undo seven carry `UndoEventArgs`/`UndoNodeEventArgs`/`UndoNodeMovedEventArgs`. A wire assuming a wrong delegate family fails at compile, which is the property the exact spellings buy.
- Boundary: native-monitor streams (`AppKit` local event monitors, `NSWorkspace` accessibility observation) are `Platform/native.md`'s — the platform owner adapts its gated monitors onto this algebra from above, publishing projected facts under the same containment contract the rows carry (`Op.Catch`, exact inverse, fault retention); `UiSource` itself carries no native row because the floor never imports upward, and this page owns no `#if` and no AppKit spelling.
- Packages: Grasshopper2 (the canvas/document/object-list/solution/history event families and their args types), Eto (the control/window/application/keyboard event families), `Rasm.Domain` (`Op`, `Fault`), `Eto/runtime.md` (`UiClock`, `ClockBeat`, `EtoDispatch`), `Shell/session.md` (`DocumentToken`).
- Growth: a new host stream is one row through an existing fold; a new anchor kind is one `EventAnchor` case with one fold — the column and the gate never change.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Rasm.Csp;
using Rasm.Grasshopper.Eto;

namespace Rasm.Grasshopper.Shell;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record EventAnchor {
    private EventAnchor() { }
    public sealed record CanvasCase(Canvas Surface) : EventAnchor;
    public sealed record DocumentCase(Document Graph) : EventAnchor;
    public sealed record SolutionCase(SolutionServer Server) : EventAnchor;
    public sealed record HistoryCase(History Ledger) : EventAnchor;
    public sealed record ControlCase(Control Surface) : EventAnchor;
    public sealed record WindowCase(Window Surface) : EventAnchor;
    public sealed record AmbientCase : EventAnchor;
    public sealed record ClockCase(UiClock Clock) : EventAnchor;
}

// --- [SERVICES] -----------------------------------------------------------------------------
internal sealed class Detachment(Action release) : IDisposable {
    private int released;
    public void Dispose() => Op.SideWhen(condition: Interlocked.Exchange(location1: ref released, value: 1) == 0, action: release);
}

internal static partial class UiEventsLog {
    [LoggerMessage(EventId = 4702, Level = LogLevel.Error, Message = "UI event publication faulted on {Source}: {Detail}")]
    internal static partial void PublicationFault(ILogger logger, string source, string detail);

    [LoggerMessage(EventId = 4712, Level = LogLevel.Error, Message = "UI subscription teardown faulted: {Detail}")]
    internal static partial void TeardownFault(ILogger logger, string detail);
}

internal sealed class EventSink(Action<UiEvent> publish, Atom<Option<Error>> lastFault, Op operation) {
    private readonly object gate = new();
    private long nextStamp;
    private bool exhausted;

    internal void Emit(UiSource source, Func<UiFact> project) {
        Fin<Unit> outcome = operation.Catch(body: () => {
            lock (gate) {
                if (exhausted) return Fin.Fail<Unit>(error: operation.InvalidResult());
                return from fact in operation.Catch(body: () => Fin.Succ(project()))
                       let stamp = nextStamp
                       let advanced = Op.Side(action: () => {
                           if (stamp == long.MaxValue) exhausted = true;
                           else nextStamp = stamp + 1L;
                       })
                       from envelope in operation.AcceptValue(value: UiEvent.Of(source: source, fact: fact, stamp: stamp))
                       from published in operation.Catch(body: () => Fin.Succ(Op.Side(action: () => publish(obj: envelope))))
                       select published;
            }
        });
        outcome.IfFail(error => {
            ignore(lastFault.Swap(_ => Some(error)));
            UiEventsLog.PublicationFault(logger: GhLog.For(category: nameof(UiEvents)), source: source.Key, detail: error.Message);
        });
    }
}

[SmartEnum<string>]
public sealed partial class UiSource {
    // GH2 canvas family — DocumentChanged/DocumentModified are Canvas-declared; the flex four are inherited FlexControl events.
    public static readonly UiSource CanvasDocumentChanged = CanvasRow(key: "canvas.document-changed",
        wire: static (surface, emit) => { EventHandler h = (_, _) => emit(() => new UiFact.CanvasCase(Signal: CanvasSignal.DocumentChanged, Location: Option<PointF>.None)); surface.DocumentChanged += h; return new Detachment(release: () => surface.DocumentChanged -= h); });
    public static readonly UiSource CanvasDocumentModified = CanvasRow(key: "canvas.document-modified",
        wire: static (surface, emit) => { EventHandler h = (_, _) => emit(() => new UiFact.CanvasCase(Signal: CanvasSignal.DocumentModified, Location: Option<PointF>.None)); surface.DocumentModified += h; return new Detachment(release: () => surface.DocumentModified -= h); });
    public static readonly UiSource CanvasProjectionChanged = CanvasRow(key: "canvas.projection-changed",
        wire: static (surface, emit) => { EventHandler<ProjectionChangedEventArgs> h = (_, _) => emit(() => new UiFact.CanvasCase(Signal: CanvasSignal.ProjectionChanged, Location: Option<PointF>.None)); surface.ProjectionChanged += h; return new Detachment(release: () => surface.ProjectionChanged -= h); });
    public static readonly UiSource CanvasWindowSelection = CanvasRow(key: "canvas.window-selection",
        wire: static (surface, emit) => { EventHandler<WindowSelectionEventArgs> h = (_, _) => emit(() => new UiFact.CanvasCase(Signal: CanvasSignal.WindowSelection, Location: Option<PointF>.None)); surface.WindowSelection += h; return new Detachment(release: () => surface.WindowSelection -= h); });
    public static readonly UiSource CanvasMouseDwell = CanvasRow(key: "canvas.mouse-dwell",
        wire: static (surface, emit) => { EventHandler<MouseDwellEventArgs> h = (_, args) => emit(() => new UiFact.CanvasCase(Signal: CanvasSignal.MouseDwell, Location: Some(args.ContentPoint))); surface.MouseDwell += h; return new Detachment(release: () => surface.MouseDwell -= h); });
    public static readonly UiSource CanvasDraw = CanvasRow(key: "canvas.draw",
        wire: static (surface, emit) => { EventHandler<ControlDrawEventArgs> h = (_, _) => emit(() => new UiFact.CanvasCase(Signal: CanvasSignal.Draw, Location: Option<PointF>.None)); surface.Draw += h; return new Detachment(release: () => surface.Draw -= h); });

    // GH2 document family — typed args; document identity is the session DocumentToken mint.
    public static readonly UiSource DocumentModified = DocumentRow(key: "document.modified",
        wire: static (graph, emit) => { EventHandler<DocumentModifiedEventArgs> h = (_, args) => emit(() => new UiFact.DocumentCase(Signal: DocumentSignal.Modified, DocumentId: Some(DocumentToken.Of(graph: args.Document)))); graph.ModifiedChanged += h; return new Detachment(release: () => graph.ModifiedChanged -= h); });
    public static readonly UiSource DocumentState = DocumentRow(key: "document.state",
        wire: static (graph, emit) => { EventHandler<DocumentStateEventArgs> h = (_, args) => emit(() => new UiFact.DocumentCase(Signal: DocumentSignal.State, DocumentId: Some(DocumentToken.Of(graph: args.Document)))); graph.StateChanged += h; return new Detachment(release: () => graph.StateChanged -= h); });
    public static readonly UiSource DocumentParent = DocumentRow(key: "document.parent",
        wire: static (graph, emit) => { EventHandler<BeforeAfterEventArgs<Document, IDocumentParent>> h = (_, args) => emit(() => new UiFact.DocumentCase(Signal: DocumentSignal.Parent, DocumentId: Some(DocumentToken.Of(graph: args.Owner)))); graph.ParentChanged += h; return new Detachment(release: () => graph.ParentChanged -= h); });

    // GH2 object-list family — the full ObjectList event set; shared-args rows ride the Subject fold.
    public static readonly UiSource GraphObjectAdded = DocumentRow(key: "graph.object-added",
        wire: static (graph, emit) => { EventHandler<AfterAddObjectEventArgs> h = (_, args) => emit(() => new UiFact.GraphCase(Signal: GraphSignal.ObjectAdded, SubjectId: Some(args.Object.InstanceId))); graph.Objects.ObjectAdded += h; return new Detachment(release: () => graph.Objects.ObjectAdded -= h); });
    public static readonly UiSource GraphObjectRemoved = DocumentRow(key: "graph.object-removed",
        wire: static (graph, emit) => { EventHandler<AfterRemoveObjectEventArgs> h = (_, args) => emit(() => new UiFact.GraphCase(Signal: GraphSignal.ObjectRemoved, SubjectId: Some(args.Object.InstanceId))); graph.Objects.ObjectRemoved += h; return new Detachment(release: () => graph.Objects.ObjectRemoved -= h); });
    public static readonly UiSource GraphSelection = Subject(key: "graph.selection", signal: GraphSignal.SelectionChanged, pick: static g => (add: h => g.Objects.ObjectSelectionChanged += h, remove: h => g.Objects.ObjectSelectionChanged -= h));
    public static readonly UiSource GraphExpired = Subject(key: "graph.expired", signal: GraphSignal.Expired, pick: static g => (add: h => g.Objects.ObjectExpired += h, remove: h => g.Objects.ObjectExpired -= h));
    public static readonly UiSource GraphEnabled = Subject(key: "graph.enabled", signal: GraphSignal.EnabledChanged, pick: static g => (add: h => g.Objects.ObjectEnabledChanged += h, remove: h => g.Objects.ObjectEnabledChanged -= h));
    public static readonly UiSource GraphRelevance = Subject(key: "graph.relevance", signal: GraphSignal.RelevanceChanged, pick: static g => (add: h => g.Objects.ObjectRelevanceChanged += h, remove: h => g.Objects.ObjectRelevanceChanged -= h));
    public static readonly UiSource GraphLayout = Subject(key: "graph.layout", signal: GraphSignal.LayoutChanged, pick: static g => (add: h => g.Objects.ObjectLayoutChanged += h, remove: h => g.Objects.ObjectLayoutChanged -= h));
    public static readonly UiSource GraphDisplay = Subject(key: "graph.display", signal: GraphSignal.DisplayChanged, pick: static g => (add: h => g.Objects.ObjectDisplayChanged += h, remove: h => g.Objects.ObjectDisplayChanged -= h));
    public static readonly UiSource GraphNameChanged = DocumentRow(key: "graph.name-changed",
        wire: static (graph, emit) => { EventHandler<ObjectNameEventArgs> h = (_, args) => emit(() => new UiFact.GraphCase(Signal: GraphSignal.NameChanged, SubjectId: Some(args.Owner.InstanceId))); graph.Objects.ObjectNameChanged += h; return new Detachment(release: () => graph.Objects.ObjectNameChanged -= h); });
    public static readonly UiSource GraphInstanceId = DocumentRow(key: "graph.instance-id",
        wire: static (graph, emit) => { EventHandler<ObjectGuidEventArgs> h = (_, args) => emit(() => new UiFact.GraphCase(Signal: GraphSignal.InstanceIdChanged, SubjectId: Some(args.NewId))); graph.Objects.ObjectInstanceIdChanged += h; return new Detachment(release: () => graph.Objects.ObjectInstanceIdChanged -= h); });

    // GH2 solution lifecycle family — the SolutionEventArgs four ride the Pulse fold; the id-only opener and the faulted closer wire individually.
    public static readonly UiSource SolutionAboutToStart = SolutionRow(key: "solution.about-to-start",
        wire: static (server, emit) => { EventHandler<SolutionIdEventArgs> h = (_, args) => emit(() => new UiFact.SolutionCase(Signal: SolutionSignal.AboutToStart, Id: Some(args.Id), Failure: Option<Error>.None)); server.SolutionAboutToStart += h; return new Detachment(release: () => server.SolutionAboutToStart -= h); });
    public static readonly UiSource SolutionStarted = Pulse(key: "solution.started", signal: SolutionSignal.Started, pick: static s => (add: h => s.SolutionStarted += h, remove: h => s.SolutionStarted -= h));
    public static readonly UiSource SolutionStopped = Pulse(key: "solution.stopped", signal: SolutionSignal.Stopped, pick: static s => (add: h => s.SolutionStopped += h, remove: h => s.SolutionStopped -= h));
    public static readonly UiSource SolutionCancelled = Pulse(key: "solution.cancelled", signal: SolutionSignal.Cancelled, pick: static s => (add: h => s.SolutionCancelled += h, remove: h => s.SolutionCancelled -= h));
    public static readonly UiSource SolutionCompleted = Pulse(key: "solution.completed", signal: SolutionSignal.Completed, pick: static s => (add: h => s.SolutionCompleted += h, remove: h => s.SolutionCompleted -= h));
    public static readonly UiSource SolutionFaulted = SolutionRow(key: "solution.faulted",
        wire: static (server, emit) => { EventHandler<SolutionExceptionEventArgs> h = (_, args) => emit(() => new UiFact.SolutionCase(Signal: SolutionSignal.Faulted, Id: Some(args.SolutionId), Failure: Some(Error.New(args.Exception)))); server.SolutionFaulted += h; return new Detachment(release: () => server.SolutionFaulted -= h); });

    // GH2 undo family — the full History event set; shared-args triplets ride the Ledger/LedgerNode folds.
    public static readonly UiSource HistoryUndone = Ledger(key: "history.undone", signal: UndoSignal.Undone, pick: static l => (add: h => l.Undone += h, remove: h => l.Undone -= h));
    public static readonly UiSource HistoryRedone = Ledger(key: "history.redone", signal: UndoSignal.Redone, pick: static l => (add: h => l.Redone += h, remove: h => l.Redone -= h));
    public static readonly UiSource HistoryModified = Ledger(key: "history.modified", signal: UndoSignal.Modified, pick: static l => (add: h => l.Modified += h, remove: h => l.Modified -= h));
    public static readonly UiSource HistoryNodeAdded = LedgerNode(key: "history.node-added", signal: UndoSignal.NodeAdded, pick: static l => (add: h => l.NodeAdded += h, remove: h => l.NodeAdded -= h));
    public static readonly UiSource HistoryNodeRemoved = LedgerNode(key: "history.node-removed", signal: UndoSignal.NodeRemoved, pick: static l => (add: h => l.NodeRemoved += h, remove: h => l.NodeRemoved -= h));
    public static readonly UiSource HistoryNodeMerged = LedgerNode(key: "history.node-merged", signal: UndoSignal.NodeMerged, pick: static l => (add: h => l.NodeMerged += h, remove: h => l.NodeMerged -= h));
    public static readonly UiSource HistoryNodeMoved = HistoryRow(key: "history.node-moved",
        wire: static (ledger, emit) => { EventHandler<UndoNodeMovedEventArgs> h = (_, _) => emit(() => new UiFact.UndoCase(Signal: UndoSignal.NodeMoved)); ledger.NodeMoved += h; return new Detachment(release: () => ledger.NodeMoved -= h); });

    // Eto control input family — pointer/key/text sub-folds; the event choice is row data.
    public static readonly UiSource ControlMouseDown = Pointer(key: "control.mouse-down", pick: static c => (add: h => c.MouseDown += h, remove: h => c.MouseDown -= h));
    public static readonly UiSource ControlMouseUp = Pointer(key: "control.mouse-up", pick: static c => (add: h => c.MouseUp += h, remove: h => c.MouseUp -= h));
    public static readonly UiSource ControlMouseMove = Pointer(key: "control.mouse-move", pick: static c => (add: h => c.MouseMove += h, remove: h => c.MouseMove -= h));
    public static readonly UiSource ControlMouseEnter = Pointer(key: "control.mouse-enter", pick: static c => (add: h => c.MouseEnter += h, remove: h => c.MouseEnter -= h));
    public static readonly UiSource ControlMouseLeave = Pointer(key: "control.mouse-leave", pick: static c => (add: h => c.MouseLeave += h, remove: h => c.MouseLeave -= h));
    public static readonly UiSource ControlMouseDoubleClick = Pointer(key: "control.mouse-double-click", pick: static c => (add: h => c.MouseDoubleClick += h, remove: h => c.MouseDoubleClick -= h));
    public static readonly UiSource ControlMouseWheel = Pointer(key: "control.mouse-wheel", pick: static c => (add: h => c.MouseWheel += h, remove: h => c.MouseWheel -= h));
    public static readonly UiSource ControlKeyDown = Keystroke(key: "control.key-down", pressed: true, pick: static c => (add: h => c.KeyDown += h, remove: h => c.KeyDown -= h));
    public static readonly UiSource ControlKeyUp = Keystroke(key: "control.key-up", pressed: false, pick: static c => (add: h => c.KeyUp += h, remove: h => c.KeyUp -= h));
    public static readonly UiSource ControlTextInput = ControlRow(key: "control.text-input",
        wire: static (surface, emit) => { EventHandler<TextInputEventArgs> h = (_, args) => emit(() => new UiFact.TextCase(Text: args.Text)); surface.TextInput += h; return new Detachment(release: () => surface.TextInput -= h); });
    public static readonly UiSource ControlDragEnter = DragRow(key: "control.drag-enter", pick: static c => (add: h => c.DragEnter += h, remove: h => c.DragEnter -= h));
    public static readonly UiSource ControlDragOver = DragRow(key: "control.drag-over", pick: static c => (add: h => c.DragOver += h, remove: h => c.DragOver -= h));
    public static readonly UiSource ControlDragLeave = DragRow(key: "control.drag-leave", pick: static c => (add: h => c.DragLeave += h, remove: h => c.DragLeave -= h));
    public static readonly UiSource ControlDragDrop = DragRow(key: "control.drag-drop", pick: static c => (add: h => c.DragDrop += h, remove: h => c.DragDrop -= h));
    public static readonly UiSource ControlDragEnd = DragRow(key: "control.drag-end", pick: static c => (add: h => c.DragEnd += h, remove: h => c.DragEnd -= h));
    public static readonly UiSource ControlEnabledChanged = ControlRow(key: "control.enabled-changed",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.EnabledCase(Enabled: surface.Enabled)); surface.EnabledChanged += h; return new Detachment(release: () => surface.EnabledChanged -= h); });
    public static readonly UiSource ControlGotFocus = ControlRow(key: "control.got-focus",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.FocusCase(Gained: true)); surface.GotFocus += h; return new Detachment(release: () => surface.GotFocus -= h); });
    public static readonly UiSource ControlLostFocus = ControlRow(key: "control.lost-focus",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.FocusCase(Gained: false)); surface.LostFocus += h; return new Detachment(release: () => surface.LostFocus -= h); });
    public static readonly UiSource ControlSizeChanged = ControlRow(key: "control.size-changed",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.BoundsCase(Bounds: surface.Bounds)); surface.SizeChanged += h; return new Detachment(release: () => surface.SizeChanged -= h); });
    public static readonly UiSource ControlShown = Stage(key: "control.shown", stage: LifecycleStage.Shown, pick: static c => (add: h => c.Shown += h, remove: h => c.Shown -= h));
    public static readonly UiSource ControlLoad = Stage(key: "control.load", stage: LifecycleStage.Load, pick: static c => (add: h => c.Load += h, remove: h => c.Load -= h));
    public static readonly UiSource ControlPreLoad = Stage(key: "control.pre-load", stage: LifecycleStage.PreLoad, pick: static c => (add: h => c.PreLoad += h, remove: h => c.PreLoad -= h));
    public static readonly UiSource ControlLoadComplete = Stage(key: "control.load-complete", stage: LifecycleStage.LoadComplete, pick: static c => (add: h => c.LoadComplete += h, remove: h => c.LoadComplete -= h));
    public static readonly UiSource ControlUnLoad = Stage(key: "control.unload", stage: LifecycleStage.UnLoad, pick: static c => (add: h => c.UnLoad += h, remove: h => c.UnLoad -= h));

    // Eto window family.
    public static readonly UiSource WindowClosing = WindowRow(key: "window.closing",
        wire: static (surface, emit) => { EventHandler<CancelEventArgs> h = (_, _) => emit(() => new UiFact.LifeCase(Stage: LifecycleStage.Closing)); surface.Closing += h; return new Detachment(release: () => surface.Closing -= h); });
    public static readonly UiSource WindowClosed = WindowRow(key: "window.closed",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.LifeCase(Stage: LifecycleStage.Closed)); surface.Closed += h; return new Detachment(release: () => surface.Closed -= h); });
    public static readonly UiSource WindowMoved = WindowRow(key: "window.moved",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.BoundsCase(Bounds: surface.Bounds)); surface.LocationChanged += h; return new Detachment(release: () => surface.LocationChanged -= h); });
    public static readonly UiSource WindowStateChanged = WindowRow(key: "window.state-changed",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.StateCase(State: surface.WindowState)); surface.WindowStateChanged += h; return new Detachment(release: () => surface.WindowStateChanged -= h); });
    public static readonly UiSource WindowDensityChanged = WindowRow(key: "window.density-changed",
        wire: static (surface, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.DensityCase(LogicalPixelSize: surface.LogicalPixelSize)); surface.LogicalPixelSizeChanged += h; return new Detachment(release: () => surface.LogicalPixelSizeChanged -= h); });

    // Ambient application / keyboard family.
    public static readonly UiSource AppInitialized = AmbientRow(key: "app.initialized",
        wire: static (application, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.LifeCase(Stage: LifecycleStage.Initialized)); application.Initialized += h; return new Detachment(release: () => application.Initialized -= h); });
    public static readonly UiSource AppTerminating = AmbientRow(key: "app.terminating",
        wire: static (application, emit) => { EventHandler<CancelEventArgs> h = (_, _) => emit(() => new UiFact.LifeCase(Stage: LifecycleStage.Terminating)); application.Terminating += h; return new Detachment(release: () => application.Terminating -= h); });
    public static readonly UiSource AppUnhandledFault = AmbientRow(key: "app.unhandled-fault",
        wire: static (application, emit) => { EventHandler<UnhandledExceptionEventArgs> h = (_, args) => emit(() => new UiFact.FaultCase(Failure: Error.New(args.ExceptionObject as Exception ?? new InvalidOperationException()))); application.UnhandledException += h; return new Detachment(release: () => application.UnhandledException -= h); });
    public static readonly UiSource AppNotificationActivated = AmbientRow(key: "app.notification-activated",
        wire: static (application, emit) => { EventHandler<NotificationEventArgs> h = (_, args) => emit(() => new UiFact.NoticeCase(Id: Optional(args.ID), UserData: Optional(args.UserData))); application.NotificationActivated += h; return new Detachment(release: () => application.NotificationActivated -= h); });
    public static readonly UiSource KeyboardModifiers = AmbientRow(key: "keyboard.modifiers",
        wire: static (_, emit) => { EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.ModifierCase(Modifiers: Keyboard.Modifiers)); Keyboard.ModifiersChanged += h; return new Detachment(release: () => Keyboard.ModifiersChanged -= h); });

    // UiClock beat bridge.
    public static readonly UiSource ClockBeats = new(key: "clock.beats", attach: static (anchor, sink, key) => anchor switch {
        EventAnchor.ClockCase clock => clock.Clock.Tap(
            observer: beat => sink.Emit(source: ClockBeats, project: () => new UiFact.BeatCase(Beat: beat)), key: key),
        _ => Fin.Fail<IDisposable>(key.InvalidInput()),
    });

    [UseDelegateFromConstructor] internal partial Fin<IDisposable> Attach(EventAnchor anchor, EventSink sink, Op key);

    private static UiSource CanvasRow(string key, Func<Canvas, Action<Func<UiFact>>, IDisposable> wire) =>
        new(key: key, attach: (anchor, sink, op) => anchor switch {
            EventAnchor.CanvasCase c => op.Catch(body: () => Fin.Succ(wire(arg1: c.Surface, arg2: Emit(sink: sink, key: key)))),
            _ => Fin.Fail<IDisposable>(op.InvalidInput()),
        });
    private static UiSource DocumentRow(string key, Func<Document, Action<Func<UiFact>>, IDisposable> wire) =>
        new(key: key, attach: (anchor, sink, op) => anchor switch {
            EventAnchor.DocumentCase c => op.Catch(body: () => Fin.Succ(wire(arg1: c.Graph, arg2: Emit(sink: sink, key: key)))),
            _ => Fin.Fail<IDisposable>(op.InvalidInput()),
        });
    private static UiSource SolutionRow(string key, Func<SolutionServer, Action<Func<UiFact>>, IDisposable> wire) =>
        new(key: key, attach: (anchor, sink, op) => anchor switch {
            EventAnchor.SolutionCase c => op.Catch(body: () => Fin.Succ(wire(arg1: c.Server, arg2: Emit(sink: sink, key: key)))),
            _ => Fin.Fail<IDisposable>(op.InvalidInput()),
        });
    private static UiSource HistoryRow(string key, Func<History, Action<Func<UiFact>>, IDisposable> wire) =>
        new(key: key, attach: (anchor, sink, op) => anchor switch {
            EventAnchor.HistoryCase c => op.Catch(body: () => Fin.Succ(wire(arg1: c.Ledger, arg2: Emit(sink: sink, key: key)))),
            _ => Fin.Fail<IDisposable>(op.InvalidInput()),
        });
    private static UiSource Subject(string key, GraphSignal signal, Func<Document, (Action<EventHandler<ObjectEventArgs>> add, Action<EventHandler<ObjectEventArgs>> remove)> pick) =>
        DocumentRow(key: key, wire: (graph, emit) => {
            (Action<EventHandler<ObjectEventArgs>> add, Action<EventHandler<ObjectEventArgs>> remove) = pick(arg: graph);
            EventHandler<ObjectEventArgs> h = (_, args) => emit(() => new UiFact.GraphCase(Signal: signal, SubjectId: Some(args.Object.InstanceId)));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static UiSource Pulse(string key, SolutionSignal signal, Func<SolutionServer, (Action<EventHandler<SolutionEventArgs>> add, Action<EventHandler<SolutionEventArgs>> remove)> pick) =>
        SolutionRow(key: key, wire: (server, emit) => {
            (Action<EventHandler<SolutionEventArgs>> add, Action<EventHandler<SolutionEventArgs>> remove) = pick(arg: server);
            EventHandler<SolutionEventArgs> h = (_, args) => emit(() => new UiFact.SolutionCase(Signal: signal, Id: Some(args.SolutionId), Failure: Option<Error>.None));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static UiSource Ledger(string key, UndoSignal signal, Func<History, (Action<EventHandler<UndoEventArgs>> add, Action<EventHandler<UndoEventArgs>> remove)> pick) =>
        HistoryRow(key: key, wire: (ledger, emit) => {
            (Action<EventHandler<UndoEventArgs>> add, Action<EventHandler<UndoEventArgs>> remove) = pick(arg: ledger);
            EventHandler<UndoEventArgs> h = (_, _) => emit(() => new UiFact.UndoCase(Signal: signal));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static UiSource LedgerNode(string key, UndoSignal signal, Func<History, (Action<EventHandler<UndoNodeEventArgs>> add, Action<EventHandler<UndoNodeEventArgs>> remove)> pick) =>
        HistoryRow(key: key, wire: (ledger, emit) => {
            (Action<EventHandler<UndoNodeEventArgs>> add, Action<EventHandler<UndoNodeEventArgs>> remove) = pick(arg: ledger);
            EventHandler<UndoNodeEventArgs> h = (_, _) => emit(() => new UiFact.UndoCase(Signal: signal));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static UiSource ControlRow(string key, Func<Control, Action<Func<UiFact>>, IDisposable> wire) =>
        new(key: key, attach: (anchor, sink, op) => anchor switch {
            EventAnchor.ControlCase c => op.Catch(body: () => Fin.Succ(wire(arg1: c.Surface, arg2: Emit(sink: sink, key: key)))),
            EventAnchor.WindowCase w => op.Catch(body: () => Fin.Succ(wire(arg1: w.Surface, arg2: Emit(sink: sink, key: key)))),
            _ => Fin.Fail<IDisposable>(op.InvalidInput()),
        });
    private static UiSource WindowRow(string key, Func<Window, Action<Func<UiFact>>, IDisposable> wire) =>
        new(key: key, attach: (anchor, sink, op) => anchor switch {
            EventAnchor.WindowCase c => op.Catch(body: () => Fin.Succ(wire(arg1: c.Surface, arg2: Emit(sink: sink, key: key)))),
            _ => Fin.Fail<IDisposable>(op.InvalidInput()),
        });
    private static UiSource AmbientRow(string key, Func<Application, Action<Func<UiFact>>, IDisposable> wire) =>
        new(key: key, attach: (anchor, sink, op) => anchor switch {
            EventAnchor.AmbientCase => Optional(Application.Instance).ToFin(op.MissingContext())
                .Bind(application => op.Catch(body: () => Fin.Succ(wire(
                    arg1: application, arg2: Emit(sink: sink, key: key))))),
            _ => Fin.Fail<IDisposable>(op.InvalidInput()),
        });
    private static UiSource Pointer(string key, Func<Control, (Action<EventHandler<MouseEventArgs>> add, Action<EventHandler<MouseEventArgs>> remove)> pick) =>
        ControlRow(key: key, wire: (surface, emit) => {
            (Action<EventHandler<MouseEventArgs>> add, Action<EventHandler<MouseEventArgs>> remove) = pick(arg: surface);
            EventHandler<MouseEventArgs> h = (_, args) => emit(() => new UiFact.PointerCase(Location: args.Location, Buttons: args.Buttons, Modifiers: args.Modifiers, Delta: args.Delta, Pressure: args.Pressure));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static UiSource Keystroke(string key, bool pressed, Func<Control, (Action<EventHandler<KeyEventArgs>> add, Action<EventHandler<KeyEventArgs>> remove)> pick) =>
        ControlRow(key: key, wire: (surface, emit) => {
            (Action<EventHandler<KeyEventArgs>> add, Action<EventHandler<KeyEventArgs>> remove) = pick(arg: surface);
            EventHandler<KeyEventArgs> h = (_, args) => emit(() => new UiFact.KeyCase(Key: args.Key, Modifiers: args.Modifiers, Pressed: pressed, Character: args.IsChar ? Some(args.KeyChar) : Option<char>.None));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static UiSource DragRow(string key, Func<Control, (Action<EventHandler<DragEventArgs>> add, Action<EventHandler<DragEventArgs>> remove)> pick) =>
        ControlRow(key: key, wire: (surface, emit) => {
            (Action<EventHandler<DragEventArgs>> add, Action<EventHandler<DragEventArgs>> remove) = pick(arg: surface);
            EventHandler<DragEventArgs> h = (_, args) => emit(() => new UiFact.DragCase(Location: args.Location, Effects: args.Effects));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static UiSource Stage(string key, LifecycleStage stage, Func<Control, (Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove)> pick) =>
        ControlRow(key: key, wire: (surface, emit) => {
            (Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove) = pick(arg: surface);
            EventHandler<EventArgs> h = (_, _) => emit(() => new UiFact.LifeCase(Stage: stage));
            add(obj: h);
            return new Detachment(release: () => remove(obj: h));
        });
    private static Action<Func<UiFact>> Emit(EventSink sink, string key) {
        UiSource source = Get(key: key);
        return project => sink.Emit(source: source, project: project);
    }
}
```

## [04]-[SUBSCRIPTION]

- Owner: `UiSubscription` sealed class — the one lifecycle carrier over one atomically drained set of per-row detachments and one shared last-fault cell. It is `IDisposable` so ownership rides `Lease<UiSubscription>` — the kernel resource rail IS the subscription lifecycle, and no local `Subscription` algebra, refcount, or per-family teardown owner exists beside it. Disposal is idempotent and UI-affine; it runs every inverse in reverse acquisition order, clears callback ownership after the attempt, preserves the first teardown fault, and records deferred publication faults through the same `LastFault` evidence.
- Entry: `UiEvents.Observe(EventAnchor anchor, Action<UiEvent> publish, Op? key = null, params ReadOnlySpan<UiSource> rows)` → `Fin<Lease<UiSubscription>>` — the single gate; one row and forty rows are the same call, arity absorbed by the span.
- Law: attachment is transactional and marshalled — the whole row set attaches inside ONE `EtoDispatch.Run` window, and a refused row attempts every already-attached inverse in reverse order before the primary fault returns, so a consumer never holds an intentionally half-attached set. An empty row set is `Fault.InvalidInput`, not an empty subscription.
- Law: the returned lease is `Owned` — the consumer's disposal window bounds every handler's lifetime; a consumer wanting host-outlived observation has no spelling here, which is the design: unbounded subscriptions are the leak class this bound forecloses.
- Boundary: consumers fan facts outward (channels, `Atom` cells, command streams) beyond this page; the algebra ends at the `publish` callback.
- Packages: `Rasm.Domain` (`Op`, `Lease<T>`), `Eto/runtime.md` (`EtoDispatch`), LanguageExt.Core (`Seq`, `Fin`).
- Growth: zero — new capability lands as rows and facts; this gate is closed.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Rasm.Csp;
using Rasm.Grasshopper.Eto;

namespace Rasm.Grasshopper.Shell;

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class UiSubscription : IDisposable {
    private readonly Atom<Seq<IDisposable>> detachments;
    private readonly Atom<Option<Error>> lastFault;
    private int releaseState;

    internal UiSubscription(Seq<IDisposable> detachments, Atom<Option<Error>> lastFault) {
        this.detachments = Atom(detachments);
        this.lastFault = lastFault;
    }

    public int Count => detachments.Value.Count;
    public Option<Error> LastFault => lastFault.Value;

    public void Dispose() => ignore(Release(key: Op.Of(name: nameof(Dispose))));

    internal static Fin<Unit> Detach(Seq<IDisposable> rows, Op key) => rows.Rev().Fold(
        initialState: Fin.Succ(unit),
        f: (first, row) => {
            Fin<Unit> next = key.Catch(body: () => Fin.Succ(Op.Side(action: row.Dispose)));
            return first.IsFail ? first : next;
        });

    private Fin<Unit> Release(Op key) {
        if (Interlocked.CompareExchange(location1: ref releaseState, value: 1, comparand: 0) != 0)
            return Fin.Succ(unit);
        bool entered = false;
        Fin<Unit> outcome = EtoDispatch.Run(body: () => {
            entered = true;
            Seq<IDisposable> live = detachments.Value;
            Fin<Unit> detached = Detach(rows: live, key: key);
            ignore(detachments.Swap(_ => Seq<IDisposable>()));
            return detached;
        }, key: key);
        return outcome.Match(
            Succ: _ => { Volatile.Write(location: ref releaseState, value: 2); return Fin.Succ(unit); },
            Fail: error => {
                ignore(lastFault.Swap(_ => Some(error)));
                UiEventsLog.TeardownFault(logger: GhLog.For(category: nameof(UiEvents)), detail: error.Message);
                Volatile.Write(location: ref releaseState, value: entered ? 2 : 0);
                return Fin.Fail<Unit>(error: error);
            });
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static class UiEvents {
    public static Fin<Lease<UiSubscription>> Observe(EventAnchor anchor, Action<UiEvent> publish, Op? key = null, params ReadOnlySpan<UiSource> rows) {
        Op op = key.OrDefault();
        Seq<UiSource> requested = toSeq(rows.ToArray());
        return from valid in op.Need(publish)
               from target in op.Need(anchor)
               from nonEmpty in guard(!requested.IsEmpty, op.InvalidInput()).ToFin()
               let lastFault = Atom(Option<Error>.None)
               let sink = new EventSink(publish: valid, lastFault: lastFault, operation: op)
               from lease in EtoDispatch.Run(body: () => requested.Fold(
                       initialState: Fin.Succ(Seq<IDisposable>()),
                       f: (acc, row) => acc.Bind(attached => op.Catch(body: () => row.Attach(
                           anchor: target, sink: sink, key: op)).BindFail(error => {
                               UiSubscription.Detach(rows: attached, key: op).IfFail(cleanup => ignore(lastFault.Swap(_ => Some(cleanup))));
                               return Fin.Fail<IDisposable>(error: error);
                           }).Map(live => attached.Add(live))))
                   .Map(attached => (Lease<UiSubscription>)new Lease<UiSubscription>.Owned(Value: new UiSubscription(
                       detachments: attached, lastFault: lastFault))), key: op)
               select lease;
    }
}
```

## [05]-[DRAIN]

- Owner: `DrainPolicy` sealed record — capacity and full-mode as one policy value; `Default` bounds the buffer and sheds the oldest element, and admission refuses `BoundedChannelFullMode.Wait` because the publish bridge lives on host callback paths that never park.
- Owner: `EvidenceDrain` sealed `IDisposable` — one bounded `Channel<UiEvent>` minted through `Channel.CreateBounded<UiEvent>(BoundedChannelOptions, Action<UiEvent>? itemDropped)` under `SingleReader = true`, `SingleWriter = false`, and `AllowSynchronousContinuations = false`, so a UI or host callback never runs a consumer continuation inline. The drop observer is the loss-evidence seam: every evicted element counts on `Shed`, keys its `UiSource`, and forwards to the optional shed callback the composition wires into `GhEvidence.DropCase` — `TryWrite` reports admission, never delivery, so loss evidence rides `itemDropped` alone.
- Entry: `EvidenceDrain.Open(Option<DrainPolicy> policy = default, Option<Action<UiEvent>> onShed = default, Op? key = null)` → `Fin<EvidenceDrain>`; `Publish(UiEvent fact)` — the non-blocking bridge handed to `Observe` as its publish callback; `Reader` — the observation half a single consumer drains; `Complete(Op? key = null)` — idempotent writer completion.
- Law: the bridge is one `TryWrite` and returns — a `false` admission (completed channel) counts as shed with the same accounting, and no publish path awaits, locks past the write, or retains the event; `Shell/journal.md` `SessionJournal.Mount` is the one structural reader the `SingleReader` promise names.
- Packages: System.Threading.Channels (`Channel.CreateBounded`, `BoundedChannelOptions`, `BoundedChannelFullMode`, `ChannelReader<T>`, `ChannelWriter<T>`), LanguageExt.Core, `Rasm.Domain` (`Op`).
- Growth: a new consumer is another drain instance over the same rows — the channel, its policy record, and the bridge never widen.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Threading.Channels;
using Rasm.Csp;

namespace Rasm.Grasshopper.Shell;

// --- [CONSTANTS] ----------------------------------------------------------------------------
public sealed record DrainPolicy(int Capacity, BoundedChannelFullMode FullMode) {
    public static readonly DrainPolicy Default = new(Capacity: 1024, FullMode: BoundedChannelFullMode.DropOldest);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class EvidenceDrain : IDisposable {
    private readonly Channel<UiEvent> channel;
    private readonly Atom<long> shedCount = Atom(0L);
    private int releaseState;

    private EvidenceDrain(Channel<UiEvent> channel) => this.channel = channel;

    public ChannelReader<UiEvent> Reader => channel.Reader;
    public long Shed => shedCount.Value;

    public static Fin<EvidenceDrain> Open(
        Option<DrainPolicy> policy = default, Option<Action<UiEvent>> onShed = default, Op? key = null) {
        Op op = key.OrDefault();
        DrainPolicy bound = policy.IfNone(DrainPolicy.Default);
        return from admitted in guard(
                   bound.Capacity > 0 && bound.FullMode != BoundedChannelFullMode.Wait,
                   op.InvalidInput()).ToFin()
               from drain in op.Catch(body: () => {
                   EvidenceDrain? minted = null;
                   Channel<UiEvent> bounded = Channel.CreateBounded<UiEvent>(
                       options: new BoundedChannelOptions(capacity: bound.Capacity) {
                           SingleReader = true,
                           SingleWriter = false,
                           AllowSynchronousContinuations = false,
                           FullMode = bound.FullMode,
                       },
                       itemDropped: dropped => minted?.Account(fact: dropped, onShed: onShed, key: op));
                   minted = new EvidenceDrain(channel: bounded);
                   return Fin.Succ(minted);
               })
               select drain;
    }

    public Unit Publish(UiEvent fact) {
        Op.SideWhen(
            condition: !channel.Writer.TryWrite(item: fact),
            action: () => Account(fact: fact, onShed: Option<Action<UiEvent>>.None, key: Op.Of(name: nameof(EvidenceDrain))));
        return unit;
    }

    public Fin<Unit> Complete(Op? key = null) =>
        key.OrDefault().Catch(body: () => Fin.Succ(Op.Side(action: () => ignore(channel.Writer.TryComplete()))));

    public void Dispose() => Op.SideWhen(
        condition: Interlocked.Exchange(location1: ref releaseState, value: 1) == 0,
        action: () => ignore(channel.Writer.TryComplete()));

    private void Account(UiEvent fact, Option<Action<UiEvent>> onShed, Op key) {
        ignore(shedCount.Swap(static count => count + 1L));
        onShed.Iter(observer => ignore(key.Catch(body: () => Fin.Succ(Op.Side(action: () => observer(obj: fact))))));
    }
}
```

```mermaid
---
title: Grasshopper boundary event flow
config:
  theme: base
  look: classic
  layout: elk
  htmlLabels: true
  markdownAutoWrap: false
  deterministicIds: true
  elk:
    nodePlacementStrategy: NETWORK_SIMPLEX
    considerModelOrder: NODES_AND_EDGES
  flowchart:
    curve: linear
    defaultRenderer: elk
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    mainBkg: "#44475A"
    nodeBorder: "#BD93F9"
    lineColor: "#FF79C6"
    arrowheadColor: "#FF79C6"
    textColor: "#F8F8F2"
    titleColor: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart TB
  accTitle: Grasshopper boundary event flow
  accDescr: Transactional source rows attach to GH2, Eto, native, and UiClock providers; callbacks cross one contained sink and return typed events to consumers.
  Request["Observation request"] -->|"anchor · rows · callback"| Gate["UiEvents.Observe"]
  Gate -->|"one UI marshal"| Rows["UiSource attach column"]
  Rows -->|"exact handler inverses"| GH2["Grasshopper2 events"]
  Rows -->|"exact handler inverses"| EtoHost["Eto events"]
  Rows -->|"token-addressed Tap"| Clock["UiClock"]
  Clock -->|"ClockBeat · MonotonicBeat"| Sink["EventSink · Op.Catch"]
  GH2 -->|"project callback"| Sink
  EtoHost -->|"project callback"| Sink
  Native["Gated native monitors"] -.->|"projected facts from above"| Sink
  Sink -->|"publishes UiEvent · records LastFault"| Subscription["Lease&lt;UiSubscription&gt;"]
  Subscription -->|"typed publication"| Consumer["Boundary consumer"]
  Rows -->|"stores reverse-order inverses"| Subscription
  classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
  classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
  classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
  class Request,Consumer,Subscription data
  class Gate,Rows,Sink primary
  class GH2,EtoHost,Clock,Native external
  linkStyle 9 stroke:#FF5555,stroke-width:2.5px
```

## [06]-[DENSITY_BAR]

| [INDEX] | [CONCERN]           | [OWNER]                         | [SHAPE_RAIL]                                | [CASES] |
| :-----: | :------------------ | :------------------------------ | :------------------------------------------ | :-----: |
|  [01]   | signal vocabularies | `CanvasSignal`…`LifecycleStage` | six keyed smart-enum row sets               |   41    |
|  [02]   | payload evidence    | `UiFact` + `UiEvent`            | closed union → validated event              |   19    |
|  [03]   | attachment anchors  | `EventAnchor`                   | closed union → attach admission             |    8    |
|  [04]   | source rows         | `UiSource`                      | smart-enum rows → `Fin<IDisposable>`        |   67    |
|  [05]   | subscription        | `UiSubscription` + `UiEvents`   | kernel lease → `Fin<Lease<UiSubscription>>` |    1    |
|  [06]   | bounded drain       | `EvidenceDrain` + `DrainPolicy` | drop-mode channel → accounted loss evidence |    1    |

`Op`, `Fault`, `Lease<T>`, `ValidityClaim`, `EtoDispatch`, `UiClock`, `GhLog`, and `DocumentToken` are composed upstream owners. A new host stream lands as one row through an existing fold with zero consumer impact.
