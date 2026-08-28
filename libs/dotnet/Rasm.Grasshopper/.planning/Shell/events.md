# [RASM_GRASSHOPPER_SHELL_EVENTS]

Boundary's UI event module is the kernel input module instantiated: `GhFact` is the folder's closed fact band riding the kernel `IUiFact` floor with a `Kernel(UiFact)` wrapping case, the GH2 source rows are one `IUiSource<GhFact>` roster over two generic wire folds, and subscription, ordering, bounded evidence, and completion are all `Rasm/Interaction/input.md`'s — `UiEvents.Observe`, `UiEvent<GhFact>` with its drain-minted `Ordinal`, `EvidenceDrain<GhFact>`, and `UiSubscription<GhFact>`. Every Eto fact case, anchor case, and source row this page once declared is DELETED onto the kernel band; what stays is the Grasshopper2 host truth the kernel cannot name.

Drain's single compare-and-swap mints the stamp and the ordinal together (kernel law), so the sink-serialized total order `Shell/journal.md`'s replay depends on survives as `UiEvent<GhFact>.Ordinal` — no local ordinal cell, saturation guard, or `EventSink` exists here.

## [01]-[INDEX]

- [02]-[FACTS]: `CanvasSignal`/`DocumentSignal`/`GraphSignal`/`SolutionSignal`/`UndoSignal` + `GhFact` — the GH2 signal vocabularies and the folder fact band.
- [03]-[SOURCES]: `GhSource` — the GH2 source roster over the two wire folds, each row pairing attach with its exact inverse.
- [04]-[BRIDGES]: `HookBridge` — the two `CancelEventArgs` veto bridges writing a hook refusal back into the host.

## [02]-[FACTS]

- Owner: `GhFact` `[Union]` realizing the kernel `IUiFact` floor — five GH2 payload cases beside the `Kernel(UiFact)` wrapping case every Eto raise rides, so one drain, one subscription, and one total order carry both bands. Signal vocabularies are keyless behavior-free `[SmartEnum<int>]` rows over the catalogued host families.
- Law: facts are evidence, never live resources — a `DocumentCase` carries the host-published `Document.Identity` Guid, a `GraphCase` carries `IDocumentObject.InstanceId`, a `SolutionCase` the host `SolutionId` value identity; a consumer needing the live object re-enters through `GhSession.Run`, so a stale fact can never hand out a disposed host reference.
- Law: `Kind` is the one wire token the floor demands, projected through the generated total `Switch`; the wrapping case delegates to the kernel fact's own `Kind`, so a journal keying on kind reads one vocabulary across both bands.
- Law: the former Eto cases — pointer, key, text, drag, focus, bounds, density, window state, lifecycle, modifiers, beat, notice, fault — are the kernel `UiFact` band and enter ONLY as `Kernel` wraps; `LifecycleStage` is the kernel roster (its ordinals differ from the deleted local set — no wire carried them, so the re-key is free), and the beat is the kernel `PulseBeat` on `UiFact.BeatCase`.
- Boundary: sparse projection is the contract by decision — a fact carries what its source row verifiably reads, and a consumer needing more evidence extends the CASE with a field, never mints a sibling snapshot record.
- Packages: Grasshopper2 (`SolutionId`), Eto.Drawing (`PointF`, prelude-bare per the csproj global using), `Rasm.Interaction` (`IUiFact`, `UiFact`, `LifecycleStage`, `PulseBeat`), `Rasm.Domain`.
- Growth: a new GH2 host signal is one row on its signal vocabulary and, where the payload is new, one `GhFact` case breaking every total dispatch loudly; a new Eto fact is the kernel's one case and costs this page nothing.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;

namespace Rasm.Grasshopper.Shell;

// --- [TYPES] ---------------------------------------------------------------------------
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

[Union]
public abstract partial record GhFact : IUiFact {
    private GhFact() { }
    public sealed record CanvasCase(CanvasSignal Signal, Option<PointF> Location) : GhFact;
    public sealed record DocumentCase(DocumentSignal Signal, Option<Guid> DocumentId) : GhFact;
    public sealed record GraphCase(GraphSignal Signal, Option<Guid> SubjectId) : GhFact;
    public sealed record SolutionCase(SolutionSignal Signal, Option<SolutionId> Id, Option<Error> Failure) : GhFact;
    public sealed record UndoCase(UndoSignal Signal) : GhFact;
    public sealed record Kernel(UiFact Fact) : GhFact;

    public string Kind => Switch(
        canvasCase:   static _ => "canvas",
        documentCase: static _ => "document",
        graphCase:    static _ => "graph",
        solutionCase: static _ => "solution",
        undoCase:     static _ => "undo",
        kernel:       static fact => fact.Fact.Kind);
}
```

## [03]-[SOURCES]

- Owner: `GhSource` — one `IUiSource<GhFact>` row per GH2 host stream, every row a one-line seat over TWO generic folds: `Row<THost, TArgs>` names the host family and its typed args, `EventTable<THost, TArgs>` carries the add-and-remove pair as one value so a subscription a row cannot undo is unspellable. Former seven per-anchor factories and eight per-args sub-folds are these two.
- Law: a GH2 SUBJECT rides its ROW — the kernel `EventAnchor` union is Eto-shaped and closed, so a row over a `Document`, `SolutionServer`, or `History` closes over the subject at its mint (`GhSource.Of(document)` answers that subject's row set) and admits `EventAnchor.Ambient` alone, the spelling that states no Eto surface is touched; the six canvas rows demand `OnControl` whose control IS the GH2 `Canvas` and refuse any other typed. Anchor agreement stays admission, never documentation.
- Law: every wire spells its host delegate exactly — the flex-interface four carry typed args (`ProjectionChangedEventArgs`, `WindowSelectionEventArgs`, `MouseDwellEventArgs` with `ContentPoint`, `ControlDrawEventArgs`); the document three carry `DocumentModifiedEventArgs`/`DocumentStateEventArgs`/`BeforeAfterEventArgs<Document, IDocumentParent>`; the object-list ten carry the `ObjectEventArgs` family; the solution six carry `SolutionIdEventArgs`/`SolutionEventArgs`/`SolutionExceptionEventArgs`; the undo seven carry `UndoEventArgs`/`UndoNodeEventArgs`/`UndoNodeMovedEventArgs`. Wire assuming a wrong delegate family fails at compile.
- Law: the emit thunk publishes into the kernel drain through `UiEvents.Observe` — projection runs inside the drain's own admission, a refused projection counts on `Refused`, a dropped event on `Shed`, and the ordinal mints under the drain's one compare-and-swap. No row touches an ordinal, a fault cell, or a log; the loss accounting is the kernel's.
- Law: subscription is `UiEvents.Observe(anchor, drain, Atomicity.AllOrNothing, rows)` — the folder's ruled posture: a refused row detaches every seated sibling and refuses whole, because the journal reading the drain is replayable only over a complete row set. Diagnostic consumer wanting partial attach names `Atomicity.Partial` at its own call site; both are kernel rows, not folder forks.
- Boundary: native-monitor streams stay `Platform/native.md`'s — the platform owner projects its gated monitors into the same drain from above; the eight canvas paint fences are `Canvas/paint.md`'s executor and never rows here.
- Packages: Grasshopper2 (the canvas/document/object-list/solution/history event families and args types), `Rasm.Interaction` (`IUiSource`, `EventAnchor`, `UiEvents`, `EvidenceDrain`, `Atomicity`), `Rasm.Domain` (`Fault`).
- Growth: a new host stream is one row through an existing fold; a new args family is one `EventTable` instantiation — the roster's two folds and the kernel gate never change.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;

namespace Rasm.Grasshopper.Shell;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record GhSource(string Key, Func<EventAnchor, Action<Func<Fin<GhFact>>>, Fin<IDisposable>> Bind) : IUiSource<GhFact> {
    // --- [CANVAS]
    public static readonly GhSource CanvasDocumentChanged = Canvas(key: "canvas.document-changed",
        wired: new EventTable<Canvas, EventArgs>(Add: static (c, h) => c.DocumentChanged += h, Drop: static (c, h) => c.DocumentChanged -= h),
        project: static (_, _) => new GhFact.CanvasCase(Signal: CanvasSignal.DocumentChanged, Location: None));
    public static readonly GhSource CanvasDocumentModified = Canvas(key: "canvas.document-modified",
        wired: new EventTable<Canvas, EventArgs>(Add: static (c, h) => c.DocumentModified += h, Drop: static (c, h) => c.DocumentModified -= h),
        project: static (_, _) => new GhFact.CanvasCase(Signal: CanvasSignal.DocumentModified, Location: None));
    public static readonly GhSource CanvasProjectionChanged = Canvas(key: "canvas.projection-changed",
        wired: new EventTable<Canvas, ProjectionChangedEventArgs>(Add: static (c, h) => c.ProjectionChanged += h, Drop: static (c, h) => c.ProjectionChanged -= h),
        project: static (_, _) => new GhFact.CanvasCase(Signal: CanvasSignal.ProjectionChanged, Location: None));
    public static readonly GhSource CanvasWindowSelection = Canvas(key: "canvas.window-selection",
        wired: new EventTable<Canvas, WindowSelectionEventArgs>(Add: static (c, h) => c.WindowSelection += h, Drop: static (c, h) => c.WindowSelection -= h),
        project: static (_, _) => new GhFact.CanvasCase(Signal: CanvasSignal.WindowSelection, Location: None));
    public static readonly GhSource CanvasMouseDwell = Canvas(key: "canvas.mouse-dwell",
        wired: new EventTable<Canvas, MouseDwellEventArgs>(Add: static (c, h) => c.MouseDwell += h, Drop: static (c, h) => c.MouseDwell -= h),
        project: static (_, args) => new GhFact.CanvasCase(Signal: CanvasSignal.MouseDwell, Location: Some(args.ContentPoint)));
    public static readonly GhSource CanvasDraw = Canvas(key: "canvas.draw",
        wired: new EventTable<Canvas, ControlDrawEventArgs>(Add: static (c, h) => c.Draw += h, Drop: static (c, h) => c.Draw -= h),
        project: static (_, _) => new GhFact.CanvasCase(Signal: CanvasSignal.Draw, Location: None));

    public static Seq<GhSource> Of(Document graph) =>
        Seq(Subject(graph, key: "document.modified", new EventTable<Document, DocumentModifiedEventArgs>(Add: static (d, h) => d.ModifiedChanged += h, Drop: static (d, h) => d.ModifiedChanged -= h),
                project: static (d, _) => new GhFact.DocumentCase(Signal: DocumentSignal.Modified, DocumentId: Some(d.Identity))),
            Subject(graph, key: "document.state", new EventTable<Document, DocumentStateEventArgs>(Add: static (d, h) => d.StateChanged += h, Drop: static (d, h) => d.StateChanged -= h),
                project: static (d, _) => new GhFact.DocumentCase(Signal: DocumentSignal.State, DocumentId: Some(d.Identity))),
            Subject(graph, key: "document.parent", new EventTable<Document, BeforeAfterEventArgs<Document, IDocumentParent>>(Add: static (d, h) => d.ParentChanged += h, Drop: static (d, h) => d.ParentChanged -= h),
                project: static (d, _) => new GhFact.DocumentCase(Signal: DocumentSignal.Parent, DocumentId: Some(d.Identity))),
            Subject(graph, key: "graph.object-added", new EventTable<Document, AfterAddObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectAdded += h, Drop: static (d, h) => d.Objects.ObjectAdded -= h),
                project: static (_, args) => new GhFact.GraphCase(Signal: GraphSignal.ObjectAdded, SubjectId: Some(args.Object.InstanceId))),
            Subject(graph, key: "graph.object-removed", new EventTable<Document, AfterRemoveObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectRemoved += h, Drop: static (d, h) => d.Objects.ObjectRemoved -= h),
                project: static (_, args) => new GhFact.GraphCase(Signal: GraphSignal.ObjectRemoved, SubjectId: Some(args.Object.InstanceId))),
            Listed(graph, key: "graph.selection", signal: GraphSignal.SelectionChanged, new EventTable<Document, ObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectSelectionChanged += h, Drop: static (d, h) => d.Objects.ObjectSelectionChanged -= h)),
            Listed(graph, key: "graph.expired", signal: GraphSignal.Expired, new EventTable<Document, ObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectExpired += h, Drop: static (d, h) => d.Objects.ObjectExpired -= h)),
            Listed(graph, key: "graph.enabled", signal: GraphSignal.EnabledChanged, new EventTable<Document, ObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectEnabledChanged += h, Drop: static (d, h) => d.Objects.ObjectEnabledChanged -= h)),
            Listed(graph, key: "graph.relevance", signal: GraphSignal.RelevanceChanged, new EventTable<Document, ObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectRelevanceChanged += h, Drop: static (d, h) => d.Objects.ObjectRelevanceChanged -= h)),
            Listed(graph, key: "graph.layout", signal: GraphSignal.LayoutChanged, new EventTable<Document, ObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectLayoutChanged += h, Drop: static (d, h) => d.Objects.ObjectLayoutChanged -= h)),
            Listed(graph, key: "graph.display", signal: GraphSignal.DisplayChanged, new EventTable<Document, ObjectEventArgs>(Add: static (d, h) => d.Objects.ObjectDisplayChanged += h, Drop: static (d, h) => d.Objects.ObjectDisplayChanged -= h)),
            Subject(graph, key: "graph.name-changed", new EventTable<Document, ObjectNameEventArgs>(Add: static (d, h) => d.Objects.ObjectNameChanged += h, Drop: static (d, h) => d.Objects.ObjectNameChanged -= h),
                project: static (_, args) => new GhFact.GraphCase(Signal: GraphSignal.NameChanged, SubjectId: Some(args.Owner.InstanceId))),
            Subject(graph, key: "graph.instance-id", new EventTable<Document, ObjectGuidEventArgs>(Add: static (d, h) => d.Objects.ObjectInstanceIdChanged += h, Drop: static (d, h) => d.Objects.ObjectInstanceIdChanged -= h),
                project: static (_, args) => new GhFact.GraphCase(Signal: GraphSignal.InstanceIdChanged, SubjectId: Some(args.NewId))));

    public static Seq<GhSource> Of(SolutionServer server) =>
        Seq(Subject(server, key: "solution.about-to-start", new EventTable<SolutionServer, SolutionIdEventArgs>(Add: static (s, h) => s.SolutionAboutToStart += h, Drop: static (s, h) => s.SolutionAboutToStart -= h),
                project: static (_, args) => new GhFact.SolutionCase(Signal: SolutionSignal.AboutToStart, Id: Some(args.Id), Failure: None)),
            Pulsed(server, key: "solution.started", signal: SolutionSignal.Started, new EventTable<SolutionServer, SolutionEventArgs>(Add: static (s, h) => s.SolutionStarted += h, Drop: static (s, h) => s.SolutionStarted -= h)),
            Pulsed(server, key: "solution.stopped", signal: SolutionSignal.Stopped, new EventTable<SolutionServer, SolutionEventArgs>(Add: static (s, h) => s.SolutionStopped += h, Drop: static (s, h) => s.SolutionStopped -= h)),
            Pulsed(server, key: "solution.cancelled", signal: SolutionSignal.Cancelled, new EventTable<SolutionServer, SolutionEventArgs>(Add: static (s, h) => s.SolutionCancelled += h, Drop: static (s, h) => s.SolutionCancelled -= h)),
            Pulsed(server, key: "solution.completed", signal: SolutionSignal.Completed, new EventTable<SolutionServer, SolutionEventArgs>(Add: static (s, h) => s.SolutionCompleted += h, Drop: static (s, h) => s.SolutionCompleted -= h)),
            Subject(server, key: "solution.faulted", new EventTable<SolutionServer, SolutionExceptionEventArgs>(Add: static (s, h) => s.SolutionFaulted += h, Drop: static (s, h) => s.SolutionFaulted -= h),
                project: static (_, args) => new GhFact.SolutionCase(
                    Signal: SolutionSignal.Faulted,
                    Id: Some(args.SolutionId),
                    Failure: Some(Error.New(args.Exception.Message, args.Exception)))));

    public static Seq<GhSource> Of(History ledger) =>
        Seq(Sealed(ledger, key: "history.undone", signal: UndoSignal.Undone, new EventTable<History, UndoEventArgs>(Add: static (l, h) => l.Undone += h, Drop: static (l, h) => l.Undone -= h)),
            Sealed(ledger, key: "history.redone", signal: UndoSignal.Redone, new EventTable<History, UndoEventArgs>(Add: static (l, h) => l.Redone += h, Drop: static (l, h) => l.Redone -= h)),
            Sealed(ledger, key: "history.modified", signal: UndoSignal.Modified, new EventTable<History, UndoEventArgs>(Add: static (l, h) => l.Modified += h, Drop: static (l, h) => l.Modified -= h)),
            Sealed(ledger, key: "history.node-added", signal: UndoSignal.NodeAdded, new EventTable<History, UndoNodeEventArgs>(Add: static (l, h) => l.NodeAdded += h, Drop: static (l, h) => l.NodeAdded -= h)),
            Sealed(ledger, key: "history.node-removed", signal: UndoSignal.NodeRemoved, new EventTable<History, UndoNodeEventArgs>(Add: static (l, h) => l.NodeRemoved += h, Drop: static (l, h) => l.NodeRemoved -= h)),
            Sealed(ledger, key: "history.node-merged", signal: UndoSignal.NodeMerged, new EventTable<History, UndoNodeEventArgs>(Add: static (l, h) => l.NodeMerged += h, Drop: static (l, h) => l.NodeMerged -= h)),
            Sealed(ledger, key: "history.node-moved", signal: UndoSignal.NodeMoved, new EventTable<History, UndoNodeMovedEventArgs>(Add: static (l, h) => l.NodeMoved += h, Drop: static (l, h) => l.NodeMoved -= h)));

    string IUiSource<GhFact>.Key => Key;
    public Fin<IDisposable> Attach(EventAnchor anchor, Action<Func<Fin<GhFact>>> emit) => Bind(anchor, emit);

    private static GhSource Canvas<TArgs>(string key, EventTable<Canvas, TArgs> wired, Func<Canvas, TArgs, GhFact> project)
        where TArgs : EventArgs =>
        new(Bind: (anchor, emit, op) => anchor switch {
            EventAnchor.OnControl { Value: Canvas surface } => Try.lift(() => Hook(surface, wired, project, emit)).Run(),
            _ => Fin.Fail<IDisposable>(new KernelFault.InvalidInput()),
        });

    private static GhSource Subject<THost, TArgs>(THost host, string key, EventTable<THost, TArgs> wired, Func<THost, TArgs, GhFact> project)
        where TArgs : EventArgs =>
        new(Bind: (anchor, emit, op) => anchor switch {
            EventAnchor.Ambient => Try.lift(() => Hook(host, wired, project, emit)).Run(),
            _ => Fin.Fail<IDisposable>(new KernelFault.InvalidInput()),
        });

    private static GhSource Listed(Document graph, string key, GraphSignal signal, EventTable<Document, ObjectEventArgs> wired) =>
        Subject(graph, key, wired, project: (_, args) => new GhFact.GraphCase(Signal: signal, SubjectId: Some(args.Object.InstanceId)));
    private static GhSource Pulsed(SolutionServer server, string key, SolutionSignal signal, EventTable<SolutionServer, SolutionEventArgs> wired) =>
        Subject(server, key, wired, project: (_, args) => new GhFact.SolutionCase(Signal: signal, Id: Some(args.SolutionId), Failure: None));
    private static GhSource Sealed<TArgs>(History ledger, string key, UndoSignal signal, EventTable<History, TArgs> wired) where TArgs : EventArgs =>
        Subject(ledger, key, wired, project: (_, _) => new GhFact.UndoCase(Signal: signal));

    private static IDisposable Hook<THost, TArgs>(
        THost host, EventTable<THost, TArgs> wired, Func<THost, TArgs, GhFact> project, Action<Func<Fin<GhFact>>> emit)
        where TArgs : EventArgs {
        EventHandler<TArgs> handler = (_, args) => emit(() => Fin.Succ(project(host, args)));
        wired.Add(host, handler);
        return new HookDetacher(Detach: () => wired.Drop(host, handler));
    }
}
```

## [04]-[BRIDGES]

- Owner: `HookBridge` — the two veto bridges the hook census names as this page's fire sites: `Window.Closing` and `Application.Terminating` carry `CancelEventArgs` the kernel fact projection cannot write back, so each bridge attaches the raw handler, fires its own point — `hooks.Fire(at: GrasshopperPoint.WindowClose, fact: new HookSignal.IntentCase(Operation: key, DocumentId: None))` at the closing bridge, `ShellTerminate` at the terminating one — and writes `args.Cancel = true` on the `Fail` leg — the one host readback in the module, stated rather than hidden in a source row.
- Law: the bridge takes the hooks as a REQUIRED parameter (minted at `Platform/composition.md`); a mount with no hooks has no veto to consult and does not exist.
- Law: the bridge is not an event row — the same host events also ride the kernel `UiSource.Closing`/`Terminating` rows as facts; the bridge exists only for the verdict write-back, so observation and governance stay two boundaries with two shapes.
- Packages: Eto.Forms (`Window`, `Application`, `CancelEventArgs`), `Rasm.Domain` (`HookSet`, `Lease<T>`).
- Growth: a third `CancelEventArgs` surface is one mount arm; the write-back law never widens.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.ComponentModel;
using Rasm.Domain;

namespace Rasm.Grasshopper.Shell;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HookBridge {
    public static Fin<Lease<IDisposable>> Closing(
        Window window, HookSet<GrasshopperPoint, HookSignal, HookScope> hooks);

    public static Fin<Lease<IDisposable>> Terminating(
        HookSet<GrasshopperPoint, HookSignal, HookScope> hooks);

    private static void Consult(
        HookSet<GrasshopperPoint, HookSignal, HookScope> hooks, GrasshopperPoint at, CancelEventArgs args) =>
        hooks.Fire(at: at, fact: new HookSignal.IntentCase(DocumentId: None))
            .IfFail(_ => { args.Cancel = true; });
}
```

## [05]-[DENSITY_BAR]

| [INDEX] | [CONCERN]           | [OWNER]                        | [RESULT]                                     | [CASES] |
| :-----: | :------------------ | :----------------------------- | :------------------------------------------- | :-----: |
|  [01]   | signal vocabularies | `CanvasSignal`…`UndoSignal`    | five keyed smart-enum row sets               |   32    |
|  [02]   | fact band           | `GhFact : IUiFact`             | closed union + the kernel wrap case          |    6    |
|  [03]   | source roster       | `GhSource : IUiSource<GhFact>` | 32 one-line rows over two generic wire folds |   32    |
|  [04]   | veto bridges        | `HookBridge`                   | hook verdict → `CancelEventArgs` write-back  |    2    |

Subscription, atomicity, ordinal, bounded drain, shed/refused accounting, and completion are the kernel input module's (`UiEvents`, `UiSubscription<GhFact>`, `EvidenceDrain<GhFact>`, `DrainPolicy`); the 29 Eto source rows, 13 Eto fact cases, 4 Eto anchors, `EventSink`, `Detachment`, the local ordinal CAS, and both `[LoggerMessage]` partials deleted onto it.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
