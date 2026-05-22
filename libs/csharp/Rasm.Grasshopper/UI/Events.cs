using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Doc;
using Grasshopper2.Undo;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public partial record UiEvent {
    private UiEvent() { }
    public sealed record PaintCase(CanvasPaintPhase Phase, Func<PaintScope, Fin<Unit>> Handler) : UiEvent;
    public sealed record DocumentCase(DocumentEventKind Kind, Func<DocumentEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record SolutionCase(SolutionEventKind Kind, Func<DocumentSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record UndoCase(UndoEventKind Kind, Func<DocumentHistorySnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record TimerCase(TimeSpan Interval, Func<Fin<Unit>> Handler) : UiEvent;
    public sealed record ControlCase(Control Source, ControlEventKind Kind, Func<ControlEventSnapshot, Fin<Unit>> Handler) : UiEvent;

    public static UiEvent Paint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler) =>
        new PaintCase(Phase: phase, Handler: handler);
    public static UiEvent Document(DocumentEventKind kind, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        new DocumentCase(Kind: kind, Handler: handler);
    public static UiEvent DocumentChanged(Func<DocumentSnapshot, Fin<Unit>> handler) =>
        new DocumentCase(Kind: DocumentEventKind.Changed, Handler: e => handler(arg: e.Document));
    public static UiEvent SelectionChanged(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) =>
        new DocumentCase(Kind: DocumentEventKind.Selection, Handler: e => handler(arg: e.Objects));
    public static UiEvent Solution(SolutionEventKind kind, Func<DocumentSnapshot, Fin<Unit>> handler) =>
        new SolutionCase(Kind: kind, Handler: handler);
    public static UiEvent Undo(UndoEventKind kind, Func<DocumentHistorySnapshot, Fin<Unit>> handler) =>
        new UndoCase(Kind: kind, Handler: handler);
    public static UiEvent Timer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        new TimerCase(Interval: interval, Handler: handler);
    public static UiEvent Control(Control source, ControlEventKind kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        new ControlCase(Source: source, Kind: kind, Handler: handler);
}

internal readonly record struct DocumentEventHandlers(
    EventHandler<DocumentModifiedEventArgs> Modified,
    EventHandler<DocumentStateEventArgs> State,
    EventHandler<AfterAddObjectEventArgs> Added,
    EventHandler<AfterRemoveObjectEventArgs> Removed,
    EventHandler<ObjectEventArgs> Expired,
    EventHandler<ObjectEventArgs> Selection,
    EventHandler<ObjectEventArgs> Enabled,
    EventHandler<ObjectEventArgs> Relevance,
    EventHandler<ObjectEventArgs> Layout,
    EventHandler<ObjectEventArgs> Display,
    EventHandler<ObjectNameEventArgs> Name,
    EventHandler<ObjectGuidEventArgs> Id);

[SmartEnum<int>]
public sealed partial class DocumentEventKind {
    private delegate Unit DocumentEventWire(GhDocument document, GhObjectList objects, DocumentEventHandlers events);

    public static readonly DocumentEventKind Changed = new(key: 0, attach: static (document, objects, events) => {
        document.ModifiedChanged += events.Modified; document.StateChanged += events.State;
        objects.ObjectAdded += events.Added; objects.ObjectRemoved += events.Removed; objects.ObjectExpired += events.Expired;
        objects.ObjectNameChanged += events.Name; objects.ObjectSelectionChanged += events.Selection; objects.ObjectEnabledChanged += events.Enabled;
        objects.ObjectRelevanceChanged += events.Relevance; objects.ObjectLayoutChanged += events.Layout; objects.ObjectDisplayChanged += events.Display;
        objects.ObjectInstanceIdChanged += events.Id; return unit;
    }, detach: static (document, objects, events) => {
        document.ModifiedChanged -= events.Modified; document.StateChanged -= events.State;
        objects.ObjectAdded -= events.Added; objects.ObjectRemoved -= events.Removed; objects.ObjectExpired -= events.Expired;
        objects.ObjectNameChanged -= events.Name; objects.ObjectSelectionChanged -= events.Selection; objects.ObjectEnabledChanged -= events.Enabled;
        objects.ObjectRelevanceChanged -= events.Relevance; objects.ObjectLayoutChanged -= events.Layout; objects.ObjectDisplayChanged -= events.Display;
        objects.ObjectInstanceIdChanged -= events.Id; return unit;
    });
    public static readonly DocumentEventKind Modified = new(key: 1,
        attach: static (document, _, events) => { document.ModifiedChanged += events.Modified; return unit; },
        detach: static (document, _, events) => { document.ModifiedChanged -= events.Modified; return unit; });
    public static readonly DocumentEventKind StateChanged = new(key: 2,
        attach: static (document, _, events) => { document.StateChanged += events.State; return unit; },
        detach: static (document, _, events) => { document.StateChanged -= events.State; return unit; });
    public static readonly DocumentEventKind ObjectAdded = new(key: 3,
        attach: static (_, objects, events) => { objects.ObjectAdded += events.Added; return unit; },
        detach: static (_, objects, events) => { objects.ObjectAdded -= events.Added; return unit; });
    public static readonly DocumentEventKind ObjectRemoved = new(key: 4,
        attach: static (_, objects, events) => { objects.ObjectRemoved += events.Removed; return unit; },
        detach: static (_, objects, events) => { objects.ObjectRemoved -= events.Removed; return unit; });
    public static readonly DocumentEventKind ObjectExpired = new(key: 5,
        attach: static (_, objects, events) => { objects.ObjectExpired += events.Expired; return unit; },
        detach: static (_, objects, events) => { objects.ObjectExpired -= events.Expired; return unit; });
    public static readonly DocumentEventKind ObjectName = new(key: 6,
        attach: static (_, objects, events) => { objects.ObjectNameChanged += events.Name; return unit; },
        detach: static (_, objects, events) => { objects.ObjectNameChanged -= events.Name; return unit; });
    public static readonly DocumentEventKind Selection = new(key: 7,
        attach: static (_, objects, events) => { objects.ObjectSelectionChanged += events.Selection; return unit; },
        detach: static (_, objects, events) => { objects.ObjectSelectionChanged -= events.Selection; return unit; });
    public static readonly DocumentEventKind ObjectEnabled = new(key: 8,
        attach: static (_, objects, events) => { objects.ObjectEnabledChanged += events.Enabled; return unit; },
        detach: static (_, objects, events) => { objects.ObjectEnabledChanged -= events.Enabled; return unit; });
    public static readonly DocumentEventKind ObjectRelevance = new(key: 9,
        attach: static (_, objects, events) => { objects.ObjectRelevanceChanged += events.Relevance; return unit; },
        detach: static (_, objects, events) => { objects.ObjectRelevanceChanged -= events.Relevance; return unit; });
    public static readonly DocumentEventKind ObjectLayout = new(key: 10,
        attach: static (_, objects, events) => { objects.ObjectLayoutChanged += events.Layout; return unit; },
        detach: static (_, objects, events) => { objects.ObjectLayoutChanged -= events.Layout; return unit; });
    public static readonly DocumentEventKind ObjectDisplay = new(key: 11,
        attach: static (_, objects, events) => { objects.ObjectDisplayChanged += events.Display; return unit; },
        detach: static (_, objects, events) => { objects.ObjectDisplayChanged -= events.Display; return unit; });
    public static readonly DocumentEventKind ObjectInstanceId = new(key: 12,
        attach: static (_, objects, events) => { objects.ObjectInstanceIdChanged += events.Id; return unit; },
        detach: static (_, objects, events) => { objects.ObjectInstanceIdChanged -= events.Id; return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Attach(GhDocument document, GhObjectList objects, DocumentEventHandlers events);
    [UseDelegateFromConstructor]
    internal partial Unit Detach(GhDocument document, GhObjectList objects, DocumentEventHandlers events);
}

internal readonly record struct SolutionEventHandlers(
    EventHandler<SolutionIdEventArgs> About,
    EventHandler<SolutionEventArgs> Plain,
    EventHandler<SolutionExceptionEventArgs> Faulted);

[SmartEnum<int>]
public sealed partial class SolutionEventKind {
    private delegate Unit SolutionEventWire(SolutionServer solution, SolutionEventHandlers events);

    public static readonly SolutionEventKind AboutToStart = new(key: 0,
        attach: static (solution, events) => { solution.SolutionAboutToStart += events.About; return unit; },
        detach: static (solution, events) => { solution.SolutionAboutToStart -= events.About; return unit; });
    public static readonly SolutionEventKind Started = new(key: 1,
        attach: static (solution, events) => { solution.SolutionStarted += events.Plain; return unit; },
        detach: static (solution, events) => { solution.SolutionStarted -= events.Plain; return unit; });
    public static readonly SolutionEventKind Stopped = new(key: 2,
        attach: static (solution, events) => { solution.SolutionStopped += events.Plain; return unit; },
        detach: static (solution, events) => { solution.SolutionStopped -= events.Plain; return unit; });
    public static readonly SolutionEventKind Cancelled = new(key: 3,
        attach: static (solution, events) => { solution.SolutionCancelled += events.Plain; return unit; },
        detach: static (solution, events) => { solution.SolutionCancelled -= events.Plain; return unit; });
    public static readonly SolutionEventKind Completed = new(key: 4,
        attach: static (solution, events) => { solution.SolutionCompleted += events.Plain; return unit; },
        detach: static (solution, events) => { solution.SolutionCompleted -= events.Plain; return unit; });
    public static readonly SolutionEventKind Faulted = new(key: 5,
        attach: static (solution, events) => { solution.SolutionFaulted += events.Faulted; return unit; },
        detach: static (solution, events) => { solution.SolutionFaulted -= events.Faulted; return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Attach(SolutionServer solution, SolutionEventHandlers events);
    [UseDelegateFromConstructor]
    internal partial Unit Detach(SolutionServer solution, SolutionEventHandlers events);
}

internal readonly record struct UndoEventHandlers(
    EventHandler<UndoEventArgs> Plain,
    EventHandler<UndoNodeEventArgs> Node,
    EventHandler<UndoNodeMovedEventArgs> Moved);

[SmartEnum<int>]
public sealed partial class UndoEventKind {
    private delegate Unit UndoEventWire(History history, UndoEventHandlers events);

    public static readonly UndoEventKind Undone = new(key: 0,
        attach: static (history, events) => { history.Undone += events.Plain; return unit; },
        detach: static (history, events) => { history.Undone -= events.Plain; return unit; });
    public static readonly UndoEventKind Redone = new(key: 1,
        attach: static (history, events) => { history.Redone += events.Plain; return unit; },
        detach: static (history, events) => { history.Redone -= events.Plain; return unit; });
    public static readonly UndoEventKind Modified = new(key: 2,
        attach: static (history, events) => { history.Modified += events.Plain; return unit; },
        detach: static (history, events) => { history.Modified -= events.Plain; return unit; });
    public static readonly UndoEventKind NodeAdded = new(key: 3,
        attach: static (history, events) => { history.NodeAdded += events.Node; return unit; },
        detach: static (history, events) => { history.NodeAdded -= events.Node; return unit; });
    public static readonly UndoEventKind NodeRemoved = new(key: 4,
        attach: static (history, events) => { history.NodeRemoved += events.Node; return unit; },
        detach: static (history, events) => { history.NodeRemoved -= events.Node; return unit; });
    public static readonly UndoEventKind NodeMerged = new(key: 5,
        attach: static (history, events) => { history.NodeMerged += events.Node; return unit; },
        detach: static (history, events) => { history.NodeMerged -= events.Node; return unit; });
    public static readonly UndoEventKind NodeMoved = new(key: 6,
        attach: static (history, events) => { history.NodeMoved += events.Moved; return unit; },
        detach: static (history, events) => { history.NodeMoved -= events.Moved; return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Attach(History history, UndoEventHandlers events);
    [UseDelegateFromConstructor]
    internal partial Unit Detach(History history, UndoEventHandlers events);
}

internal readonly record struct ControlEventHandlers(
    EventHandler<EventArgs> Plain,
    EventHandler<KeyEventArgs> Key,
    EventHandler<TextInputEventArgs> Text,
    EventHandler<MouseEventArgs> Mouse,
    EventHandler<DragEventArgs> Drag);

[SmartEnum<int>]
public sealed partial class ControlEventKind {
    private delegate Unit ControlEventWire(Control source, ControlEventHandlers events);

    public static readonly ControlEventKind PreLoad = Plain(key: 0, attach: static (s, h) => s.PreLoad += h, detach: static (s, h) => s.PreLoad -= h);
    public static readonly ControlEventKind Load = Plain(key: 1, attach: static (s, h) => s.Load += h, detach: static (s, h) => s.Load -= h);
    public static readonly ControlEventKind LoadComplete = Plain(key: 2, attach: static (s, h) => s.LoadComplete += h, detach: static (s, h) => s.LoadComplete -= h);
    public static readonly ControlEventKind UnLoad = Plain(key: 3, attach: static (s, h) => s.UnLoad += h, detach: static (s, h) => s.UnLoad -= h);
    public static readonly ControlEventKind Shown = Plain(key: 4, attach: static (s, h) => s.Shown += h, detach: static (s, h) => s.Shown -= h);
    public static readonly ControlEventKind GotFocus = Plain(key: 5, attach: static (s, h) => s.GotFocus += h, detach: static (s, h) => s.GotFocus -= h);
    public static readonly ControlEventKind LostFocus = Plain(key: 6, attach: static (s, h) => s.LostFocus += h, detach: static (s, h) => s.LostFocus -= h);
    public static readonly ControlEventKind SizeChanged = Plain(key: 7, attach: static (s, h) => s.SizeChanged += h, detach: static (s, h) => s.SizeChanged -= h);
    public static readonly ControlEventKind EnabledChanged = Plain(key: 8, attach: static (s, h) => s.EnabledChanged += h, detach: static (s, h) => s.EnabledChanged -= h);
    public static readonly ControlEventKind KeyDown = Keyed(key: 9, attach: static (s, h) => s.KeyDown += h, detach: static (s, h) => s.KeyDown -= h);
    public static readonly ControlEventKind KeyUp = Keyed(key: 10, attach: static (s, h) => s.KeyUp += h, detach: static (s, h) => s.KeyUp -= h);
    public static readonly ControlEventKind TextInput = Textual(key: 11, attach: static (s, h) => s.TextInput += h, detach: static (s, h) => s.TextInput -= h);
    public static readonly ControlEventKind MouseDown = Moused(key: 12, attach: static (s, h) => s.MouseDown += h, detach: static (s, h) => s.MouseDown -= h);
    public static readonly ControlEventKind MouseUp = Moused(key: 13, attach: static (s, h) => s.MouseUp += h, detach: static (s, h) => s.MouseUp -= h);
    public static readonly ControlEventKind MouseMove = Moused(key: 14, attach: static (s, h) => s.MouseMove += h, detach: static (s, h) => s.MouseMove -= h);
    public static readonly ControlEventKind MouseEnter = Moused(key: 15, attach: static (s, h) => s.MouseEnter += h, detach: static (s, h) => s.MouseEnter -= h);
    public static readonly ControlEventKind MouseLeave = Moused(key: 16, attach: static (s, h) => s.MouseLeave += h, detach: static (s, h) => s.MouseLeave -= h);
    public static readonly ControlEventKind MouseDoubleClick = Moused(key: 17, attach: static (s, h) => s.MouseDoubleClick += h, detach: static (s, h) => s.MouseDoubleClick -= h);
    public static readonly ControlEventKind MouseWheel = Moused(key: 18, attach: static (s, h) => s.MouseWheel += h, detach: static (s, h) => s.MouseWheel -= h);
    public static readonly ControlEventKind DragDrop = Dragged(key: 19, attach: static (s, h) => s.DragDrop += h, detach: static (s, h) => s.DragDrop -= h);
    public static readonly ControlEventKind DragOver = Dragged(key: 20, attach: static (s, h) => s.DragOver += h, detach: static (s, h) => s.DragOver -= h);
    public static readonly ControlEventKind DragEnter = Dragged(key: 21, attach: static (s, h) => s.DragEnter += h, detach: static (s, h) => s.DragEnter -= h);
    public static readonly ControlEventKind DragLeave = Dragged(key: 22, attach: static (s, h) => s.DragLeave += h, detach: static (s, h) => s.DragLeave -= h);
    public static readonly ControlEventKind DragEnd = Dragged(key: 23, attach: static (s, h) => s.DragEnd += h, detach: static (s, h) => s.DragEnd -= h);

    private static ControlEventKind Plain(int key, Action<Control, EventHandler<EventArgs>> attach, Action<Control, EventHandler<EventArgs>> detach) =>
        new(key: key, attach: (source, events) => { attach(arg1: source, arg2: events.Plain); return unit; }, detach: (source, events) => { detach(arg1: source, arg2: events.Plain); return unit; });
    private static ControlEventKind Keyed(int key, Action<Control, EventHandler<KeyEventArgs>> attach, Action<Control, EventHandler<KeyEventArgs>> detach) =>
        new(key: key, attach: (source, events) => { attach(arg1: source, arg2: events.Key); return unit; }, detach: (source, events) => { detach(arg1: source, arg2: events.Key); return unit; });
    private static ControlEventKind Textual(int key, Action<Control, EventHandler<TextInputEventArgs>> attach, Action<Control, EventHandler<TextInputEventArgs>> detach) =>
        new(key: key, attach: (source, events) => { attach(arg1: source, arg2: events.Text); return unit; }, detach: (source, events) => { detach(arg1: source, arg2: events.Text); return unit; });
    private static ControlEventKind Moused(int key, Action<Control, EventHandler<MouseEventArgs>> attach, Action<Control, EventHandler<MouseEventArgs>> detach) =>
        new(key: key, attach: (source, events) => { attach(arg1: source, arg2: events.Mouse); return unit; }, detach: (source, events) => { detach(arg1: source, arg2: events.Mouse); return unit; });
    private static ControlEventKind Dragged(int key, Action<Control, EventHandler<DragEventArgs>> attach, Action<Control, EventHandler<DragEventArgs>> detach) =>
        new(key: key, attach: (source, events) => { attach(arg1: source, arg2: events.Drag); return unit; }, detach: (source, events) => { detach(arg1: source, arg2: events.Drag); return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Attach(Control source, ControlEventHandlers events);
    [UseDelegateFromConstructor]
    internal partial Unit Detach(Control source, ControlEventHandlers events);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentEventSnapshot(
    DocumentEventKind Kind,
    DocumentSnapshot Document,
    Seq<DocumentObjectSnapshot> Objects,
    Seq<WireSnapshot.ConnectedCase> Wires,
    Option<string> Detail);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ControlEventSnapshot(
    ControlEventKind Kind,
    bool Enabled,
    bool Visible,
    bool HasFocus,
    Option<PointF> Point = default,
    Option<MouseButtons> Buttons = default,
    Option<Keys> Keys = default,
    Option<string> Text = default,
    Option<SizeF> Delta = default,
    Option<float> Pressure = default,
    Option<DragEffects> DragEffects = default,
    Option<DragEffects> AllowedDragEffects = default,
    Option<IDataObject> DragData = default);

internal sealed record EventRequest(UiEvent Event) : GhUiRequest<Subscription> {
    internal override GrasshopperUiPolicy Policy => Events.PolicyOf(uiEvent: Event);
    internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Events.Subscribe(uiEvent: Event).Run(scope: scope);
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Events {
    internal static GrasshopperUiPolicy PolicyOf(UiEvent uiEvent) =>
        uiEvent.Switch(
            paintCase: static _ => GrasshopperUiPolicy.Canvas(),
            documentCase: static _ => GrasshopperUiPolicy.Document(),
            solutionCase: static _ => GrasshopperUiPolicy.Document(),
            undoCase: static _ => GrasshopperUiPolicy.Document(),
            timerCase: static _ => GrasshopperUiPolicy.Read,
            controlCase: static _ => GrasshopperUiPolicy.Read);

    internal static GrasshopperUiIntent<Subscription> Subscribe(UiEvent uiEvent) =>
        uiEvent.Switch(
            paintCase: static p => Paint.Hook(phase: p.Phase, paint: p.Handler),
            documentCase: static d => SubscribeDocument(kind: d.Kind, handler: d.Handler),
            solutionCase: static s => SubscribeSolution(kind: s.Kind, handler: s.Handler),
            undoCase: static u => SubscribeUndo(kind: u.Kind, handler: u.Handler),
            timerCase: static t => SubscribeTimer(interval: t.Interval, handler: t.Handler),
            controlCase: static c => SubscribeControl(source: c.Source, kind: c.Kind, handler: c.Handler));

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static GrasshopperUiIntent<Subscription> SubscribeDocument(DocumentEventKind kind, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            let events = BuildDocumentHandlers(doc: doc, objs: objs, handler: handler)
            from sub in Subscription.Bind(
                attach: () => kind.Attach(document: doc, objects: objs, events: events),
                detach: () => kind.Detach(document: doc, objects: objs, events: events),
                marshalToUi: true)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeSolution(SolutionEventKind kind, Func<DocumentSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeSolution)), detail: "null handler"))
            let publish = (System.Action)(() => GrasshopperUi.Protect(valid: () => valid(arg: UiRail.DocumentSnapshotOf(document: doc, objects: objs))).Ignore())
            let events = new SolutionEventHandlers(About: (_, _) => publish(), Plain: (_, _) => publish(), Faulted: (_, _) => publish())
            from sub in Subscription.Bind(
                attach: () => kind.Attach(solution: doc.Solution, events: events),
                detach: () => kind.Detach(solution: doc.Solution, events: events),
                marshalToUi: true)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeUndo(UndoEventKind kind, Func<DocumentHistorySnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeUndo)), detail: "null handler"))
            let publish = (System.Action)(() => GrasshopperUi.Protect(valid: () => valid(arg: UiRail.HistorySnapshotOf(document: doc))).Ignore())
            let events = new UndoEventHandlers(Plain: (_, _) => publish(), Node: (_, _) => publish(), Moved: (_, _) => publish())
            from sub in Subscription.Bind(
                attach: () => kind.Attach(history: doc.Undo, events: events),
                detach: () => kind.Detach(history: doc.Undo, events: events),
                marshalToUi: true)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeTimer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        GhUi.Read(run: scope =>
            from validInterval in Optional(interval).Filter(static value => value > TimeSpan.Zero)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "interval must be positive"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "null handler"))
            let timer = new UITimer { Interval = validInterval.TotalSeconds }
            let elapsed = (EventHandler<EventArgs>)((_, _) => GrasshopperUi.Protect(valid: valid).Ignore())
            from sub in Subscription.Bind(
                attach: () => { timer.Elapsed += elapsed; timer.Start(); },
                detach: () => { timer.Elapsed -= elapsed; timer.Stop(); timer.Dispose(); },
                marshalToUi: true)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeControl(Control source, ControlEventKind kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Read(run: scope =>
            from validSource in Optional(source).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null control"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null handler"))
            let events = BuildControlHandlers(source: validSource, kind: kind, handler: valid)
            from sub in Subscription.Bind(
                attach: () => kind.Attach(source: validSource, events: events),
                detach: () => kind.Detach(source: validSource, events: events),
                marshalToUi: true)
            select sub);

    private static DocumentEventHandlers BuildDocumentHandlers(GhDocument doc, GhObjectList objs, Func<DocumentEventSnapshot, Fin<Unit>> handler) {
        DocumentEventSnapshot Snapshot(DocumentEventKind k, Option<IDocumentObject> changed = default, Option<string> detail = default) =>
            new(
                Kind: k,
                Document: UiRail.DocumentSnapshotOf(document: doc, objects: objs),
                Objects: (k == DocumentEventKind.Selection) switch {
                    true => toSeq(objs.SelectedObjects.Select(UiRail.DocumentObjectSnapshotOf)),
                    false => changed.Map(obj => Seq(UiRail.DocumentObjectSnapshotOf(obj))).IfNone(Seq<DocumentObjectSnapshot>()),
                },
                Wires: (k == DocumentEventKind.Selection) switch {
                    true => Optional(objs.SelectedWires).Map(wires => toSeq(wires).Map(wire => Wire.SnapshotConnected(objects: objs, wire: wire))).IfNone(Seq<WireSnapshot.ConnectedCase>()),
                    false => Seq<WireSnapshot.ConnectedCase>(),
                },
                Detail: detail);
        Unit Publish(DocumentEventKind k, Option<IDocumentObject> changed = default, Option<string> detail = default) {
            _ = GrasshopperUi.Protect(valid: () => handler(arg: Snapshot(k, changed, detail)));
            return unit;
        }
        return new DocumentEventHandlers(
            Modified: (_, e) => Publish(DocumentEventKind.Modified, detail: Some($"{e.Oldstate}->{e.NewState}")),
            State: (_, e) => Publish(DocumentEventKind.StateChanged, detail: Some($"{e.Oldstate}->{e.NewState}")),
            Added: (_, e) => Publish(DocumentEventKind.ObjectAdded, changed: Optional(e.Object)),
            Removed: (_, e) => Publish(DocumentEventKind.ObjectRemoved, changed: Optional(e.Object)),
            Expired: (_, e) => Publish(DocumentEventKind.ObjectExpired, changed: Optional(e.Object)),
            Selection: (_, e) => Publish(DocumentEventKind.Selection, changed: Optional(e.Object)),
            Enabled: (_, e) => Publish(DocumentEventKind.ObjectEnabled, changed: Optional(e.Object)),
            Relevance: (_, e) => Publish(DocumentEventKind.ObjectRelevance, changed: Optional(e.Object)),
            Layout: (_, e) => Publish(DocumentEventKind.ObjectLayout, changed: Optional(e.Object)),
            Display: (_, e) => Publish(DocumentEventKind.ObjectDisplay, changed: Optional(e.Object)),
            Name: (_, e) => Publish(DocumentEventKind.ObjectName, changed: Optional(e.Owner), detail: Some($"{e.Old}->{e.New}")),
            Id: (_, e) => Publish(DocumentEventKind.ObjectInstanceId, changed: Optional(e.Object), detail: Some($"{e.OldId}->{e.NewId}")));
    }

    private static ControlEventHandlers BuildControlHandlers(Control source, ControlEventKind kind, Func<ControlEventSnapshot, Fin<Unit>> handler) {
        ControlEventSnapshot Snapshot(
            Option<PointF> point = default,
            Option<MouseButtons> buttons = default,
            Option<Keys> keys = default,
            Option<string> text = default,
            Option<SizeF> delta = default,
            Option<float> pressure = default,
            Option<DragEffects> dragEffects = default,
            Option<DragEffects> allowedDragEffects = default,
            Option<IDataObject> dragData = default) =>
            new(Kind: kind, Enabled: source.Enabled, Visible: source.Visible, HasFocus: source.HasFocus,
                Point: point, Buttons: buttons, Keys: keys, Text: text, Delta: delta, Pressure: pressure,
                DragEffects: dragEffects, AllowedDragEffects: allowedDragEffects, DragData: dragData);
        Unit Publish(ControlEventSnapshot snapshot) {
            _ = GrasshopperUi.Protect(valid: () => handler(arg: snapshot));
            return unit;
        }
        return new ControlEventHandlers(
            Plain: (_, _) => Publish(Snapshot()),
            Key: (_, e) => Publish(Snapshot(keys: Some(e.KeyData))),
            Text: (_, e) => Publish(Snapshot(text: Optional(e.Text))),
            Mouse: (_, e) => Publish(Snapshot(point: Some(e.Location), buttons: Some(e.Buttons), keys: Some(e.Modifiers), delta: Some(e.Delta), pressure: Some(e.Pressure))),
            Drag: (_, e) => Publish(Snapshot(point: Some(e.Location), keys: Some(e.Modifiers), dragEffects: Some(e.Effects), allowedDragEffects: Some(e.AllowedEffects), dragData: Optional<IDataObject>(value: e.Data))));
    }
}
