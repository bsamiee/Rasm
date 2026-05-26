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
    public static readonly DocumentEventKind Modified = OnDocument(key: 1, attach: static (d, h) => d.ModifiedChanged += h, detach: static (d, h) => d.ModifiedChanged -= h, selector: static e => e.Modified);
    public static readonly DocumentEventKind StateChanged = OnDocument(key: 2, attach: static (d, h) => d.StateChanged += h, detach: static (d, h) => d.StateChanged -= h, selector: static e => e.State);
    public static readonly DocumentEventKind ObjectAdded = OnObjects(key: 3, attach: static (o, h) => o.ObjectAdded += h, detach: static (o, h) => o.ObjectAdded -= h, selector: static e => e.Added);
    public static readonly DocumentEventKind ObjectRemoved = OnObjects(key: 4, attach: static (o, h) => o.ObjectRemoved += h, detach: static (o, h) => o.ObjectRemoved -= h, selector: static e => e.Removed);
    public static readonly DocumentEventKind ObjectExpired = OnObjects(key: 5, attach: static (o, h) => o.ObjectExpired += h, detach: static (o, h) => o.ObjectExpired -= h, selector: static e => e.Expired);
    public static readonly DocumentEventKind ObjectName = OnObjects(key: 6, attach: static (o, h) => o.ObjectNameChanged += h, detach: static (o, h) => o.ObjectNameChanged -= h, selector: static e => e.Name);
    public static readonly DocumentEventKind Selection = OnObjects(key: 7, attach: static (o, h) => o.ObjectSelectionChanged += h, detach: static (o, h) => o.ObjectSelectionChanged -= h, selector: static e => e.Selection);
    public static readonly DocumentEventKind ObjectEnabled = OnObjects(key: 8, attach: static (o, h) => o.ObjectEnabledChanged += h, detach: static (o, h) => o.ObjectEnabledChanged -= h, selector: static e => e.Enabled);
    public static readonly DocumentEventKind ObjectRelevance = OnObjects(key: 9, attach: static (o, h) => o.ObjectRelevanceChanged += h, detach: static (o, h) => o.ObjectRelevanceChanged -= h, selector: static e => e.Relevance);
    public static readonly DocumentEventKind ObjectLayout = OnObjects(key: 10, attach: static (o, h) => o.ObjectLayoutChanged += h, detach: static (o, h) => o.ObjectLayoutChanged -= h, selector: static e => e.Layout);
    public static readonly DocumentEventKind ObjectDisplay = OnObjects(key: 11, attach: static (o, h) => o.ObjectDisplayChanged += h, detach: static (o, h) => o.ObjectDisplayChanged -= h, selector: static e => e.Display);
    public static readonly DocumentEventKind ObjectInstanceId = OnObjects(key: 12, attach: static (o, h) => o.ObjectInstanceIdChanged += h, detach: static (o, h) => o.ObjectInstanceIdChanged -= h, selector: static e => e.Id);

    [UseDelegateFromConstructor]
    internal partial Unit Attach(GhDocument document, GhObjectList objects, DocumentEventHandlers events);
    [UseDelegateFromConstructor]
    internal partial Unit Detach(GhDocument document, GhObjectList objects, DocumentEventHandlers events);

    private static DocumentEventKind OnDocument<TArgs>(int key, Action<GhDocument, EventHandler<TArgs>> attach, Action<GhDocument, EventHandler<TArgs>> detach, Func<DocumentEventHandlers, EventHandler<TArgs>> selector) =>
        new(
            key: key,
            attach: (d, _, e) => { attach(arg1: d, arg2: selector(arg: e)); return unit; },
            detach: (d, _, e) => { detach(arg1: d, arg2: selector(arg: e)); return unit; });

    private static DocumentEventKind OnObjects<TArgs>(int key, Action<GhObjectList, EventHandler<TArgs>> attach, Action<GhObjectList, EventHandler<TArgs>> detach, Func<DocumentEventHandlers, EventHandler<TArgs>> selector) =>
        new(
            key: key,
            attach: (_, o, e) => { attach(arg1: o, arg2: selector(arg: e)); return unit; },
            detach: (_, o, e) => { detach(arg1: o, arg2: selector(arg: e)); return unit; });
}

internal readonly record struct SolutionEventHandlers(
    EventHandler<SolutionIdEventArgs> About,
    EventHandler<SolutionEventArgs> Plain,
    EventHandler<SolutionExceptionEventArgs> Faulted);

[SmartEnum<int>]
public sealed partial class SolutionEventKind {
    private delegate Unit SolutionEventWire(SolutionServer solution, SolutionEventHandlers events);

    public static readonly SolutionEventKind AboutToStart = Of(key: 0, attach: static (s, h) => s.SolutionAboutToStart += h, detach: static (s, h) => s.SolutionAboutToStart -= h, selector: static e => e.About);
    public static readonly SolutionEventKind Started = Of(key: 1, attach: static (s, h) => s.SolutionStarted += h, detach: static (s, h) => s.SolutionStarted -= h, selector: static e => e.Plain);
    public static readonly SolutionEventKind Stopped = Of(key: 2, attach: static (s, h) => s.SolutionStopped += h, detach: static (s, h) => s.SolutionStopped -= h, selector: static e => e.Plain);
    public static readonly SolutionEventKind Cancelled = Of(key: 3, attach: static (s, h) => s.SolutionCancelled += h, detach: static (s, h) => s.SolutionCancelled -= h, selector: static e => e.Plain);
    public static readonly SolutionEventKind Completed = Of(key: 4, attach: static (s, h) => s.SolutionCompleted += h, detach: static (s, h) => s.SolutionCompleted -= h, selector: static e => e.Plain);
    public static readonly SolutionEventKind Faulted = Of(key: 5, attach: static (s, h) => s.SolutionFaulted += h, detach: static (s, h) => s.SolutionFaulted -= h, selector: static e => e.Faulted);

    [UseDelegateFromConstructor]
    internal partial Unit Attach(SolutionServer solution, SolutionEventHandlers events);
    [UseDelegateFromConstructor]
    internal partial Unit Detach(SolutionServer solution, SolutionEventHandlers events);

    private static SolutionEventKind Of<TArgs>(int key, Action<SolutionServer, EventHandler<TArgs>> attach, Action<SolutionServer, EventHandler<TArgs>> detach, Func<SolutionEventHandlers, EventHandler<TArgs>> selector) =>
        new(
            key: key,
            attach: (s, e) => { attach(arg1: s, arg2: selector(arg: e)); return unit; },
            detach: (s, e) => { detach(arg1: s, arg2: selector(arg: e)); return unit; });
}

internal readonly record struct UndoEventHandlers(
    EventHandler<UndoEventArgs> Plain,
    EventHandler<UndoNodeEventArgs> Node,
    EventHandler<UndoNodeMovedEventArgs> Moved);

[SmartEnum<int>]
public sealed partial class UndoEventKind {
    private delegate Unit UndoEventWire(History history, UndoEventHandlers events);

    public static readonly UndoEventKind Undone = Of(key: 0, attach: static (h, x) => h.Undone += x, detach: static (h, x) => h.Undone -= x, selector: static e => e.Plain);
    public static readonly UndoEventKind Redone = Of(key: 1, attach: static (h, x) => h.Redone += x, detach: static (h, x) => h.Redone -= x, selector: static e => e.Plain);
    public static readonly UndoEventKind Modified = Of(key: 2, attach: static (h, x) => h.Modified += x, detach: static (h, x) => h.Modified -= x, selector: static e => e.Plain);
    public static readonly UndoEventKind NodeAdded = Of(key: 3, attach: static (h, x) => h.NodeAdded += x, detach: static (h, x) => h.NodeAdded -= x, selector: static e => e.Node);
    public static readonly UndoEventKind NodeRemoved = Of(key: 4, attach: static (h, x) => h.NodeRemoved += x, detach: static (h, x) => h.NodeRemoved -= x, selector: static e => e.Node);
    public static readonly UndoEventKind NodeMerged = Of(key: 5, attach: static (h, x) => h.NodeMerged += x, detach: static (h, x) => h.NodeMerged -= x, selector: static e => e.Node);
    public static readonly UndoEventKind NodeMoved = Of(key: 6, attach: static (h, x) => h.NodeMoved += x, detach: static (h, x) => h.NodeMoved -= x, selector: static e => e.Moved);

    [UseDelegateFromConstructor]
    internal partial Unit Attach(History history, UndoEventHandlers events);
    [UseDelegateFromConstructor]
    internal partial Unit Detach(History history, UndoEventHandlers events);

    private static UndoEventKind Of<TArgs>(int key, Action<History, EventHandler<TArgs>> attach, Action<History, EventHandler<TArgs>> detach, Func<UndoEventHandlers, EventHandler<TArgs>> selector) =>
        new(
            key: key,
            attach: (h, e) => { attach(arg1: h, arg2: selector(arg: e)); return unit; },
            detach: (h, e) => { detach(arg1: h, arg2: selector(arg: e)); return unit; });
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
            paintCase: static p => Paint.Hook(phase: p.Phase, paint: p.Handler, clock: MotionClock.MessageLoop),
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
            let scope2 = new TimerScope(interval: validInterval, tick: () => GrasshopperUi.Protect(valid: valid).Ignore())
            from sub in Subscription.Bind(
                attach: scope2.Start,
                detach: scope2.Dispose,
                marshalToUi: true)
            select sub);

    // Idempotent UITimer lifecycle wrapper. Attaches Elapsed handler on Start; Dispose is gated by
    // Interlocked.Exchange so a double Subscription.Dispose (or rollback after Start success) is safe.
    private sealed class TimerScope : IDisposable {
        private readonly UITimer timer;
        private readonly EventHandler<EventArgs> tick;
        private int disposed;

        internal TimerScope(TimeSpan interval, System.Action tick) {
            timer = new UITimer { Interval = interval.TotalSeconds };
            this.tick = (_, _) => tick();
        }

        internal void Start() { timer.Elapsed += tick; timer.Start(); }

        public void Dispose() {
            if (Interlocked.Exchange(ref disposed, 1) == 1) {
                return;
            }
            timer.Stop();
            timer.Elapsed -= tick;
            timer.Dispose();
        }
    }

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
