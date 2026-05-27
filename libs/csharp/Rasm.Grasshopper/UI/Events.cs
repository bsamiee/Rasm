using System.Diagnostics.CodeAnalysis;
using Eto.Forms;
using Grasshopper2.Doc;
using Grasshopper2.Undo;
using ControlEvent = Rasm.Grasshopper.UI.EventSource<Eto.Forms.Control, Rasm.Grasshopper.UI.ControlEventHandlers>;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;
using SolutionEvent = Rasm.Grasshopper.UI.EventSource<Grasshopper2.Doc.SolutionServer, Rasm.Grasshopper.UI.SolutionEventHandlers>;
using UndoEvent = Rasm.Grasshopper.UI.EventSource<Grasshopper2.Undo.History, Rasm.Grasshopper.UI.UndoEventHandlers>;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record UiEvent {
    private UiEvent() { }
    public sealed record PaintCase(CanvasPaintPhase Phase, Func<PaintScope, Fin<Unit>> Handler, MotionClock? Clock = null) : UiEvent;
    public sealed record DocumentCase(DocumentEvent Kind, Func<DocumentEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record SolutionCase(SolutionEvent Kind, Func<SolutionEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record UndoCase(UndoEvent Kind, Func<UndoEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record TimerCase(TimeSpan Interval, Func<Fin<Unit>> Handler) : UiEvent;
    public sealed record ControlCase(Control Source, ControlEvent Kind, Func<ControlEventSnapshot, Fin<Unit>> Handler) : UiEvent;

    public static UiEvent Paint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler, MotionClock? clock = null) =>
        new PaintCase(Phase: phase, Handler: handler, Clock: clock);
    public static UiEvent Document(DocumentEvent kind, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        new DocumentCase(Kind: kind, Handler: handler);
    public static UiEvent DocumentChanged(Func<DocumentSnapshot, Fin<Unit>> handler) =>
        new DocumentCase(Kind: DocumentEventKind.Changed, Handler: e => handler(arg: e.Document));
    public static UiEvent SelectionChanged(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) =>
        new DocumentCase(Kind: DocumentEventKind.Selection, Handler: e => handler(arg: e.Objects));
    public static UiEvent Solution(SolutionEvent kind, Func<SolutionEventSnapshot, Fin<Unit>> handler) =>
        new SolutionCase(Kind: kind, Handler: handler);
    public static UiEvent Undo(UndoEvent kind, Func<UndoEventSnapshot, Fin<Unit>> handler) =>
        new UndoCase(Kind: kind, Handler: handler);
    public static UiEvent Timer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        new TimerCase(Interval: interval, Handler: handler);
    public static UiEvent Control(Control source, ControlEvent kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        new ControlCase(Source: source, Kind: kind, Handler: handler);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        paintCase: static _ => GrasshopperUiPolicy.Canvas(),
        documentCase: static _ => GrasshopperUiPolicy.Document(),
        solutionCase: static _ => GrasshopperUiPolicy.Document(),
        undoCase: static _ => GrasshopperUiPolicy.Document(),
        timerCase: static _ => GrasshopperUiPolicy.Read,
        controlCase: static _ => GrasshopperUiPolicy.Read);
}

[StructLayout(LayoutKind.Auto)]
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Generic carrier with closed-generic factory On<TArgs>; moving to companion class breaks lambda type inference at call sites.")]
public readonly record struct EventSource<TOwner, THandlers>(string Name, Action<TOwner, THandlers> Attach, Action<TOwner, THandlers> Detach) {
    public static EventSource<TOwner, THandlers> On<TArgs>(
        string name,
        Action<TOwner, EventHandler<TArgs>> attach,
        Action<TOwner, EventHandler<TArgs>> detach,
        Func<THandlers, EventHandler<TArgs>> selector) =>
        new(
            Name: name,
            Attach: (owner, handlers) => attach(arg1: owner, arg2: selector(arg: handlers)),
            Detach: (owner, handlers) => detach(arg1: owner, arg2: selector(arg: handlers)));
}

public readonly record struct DocumentEventHandlers(EventHandler<DocumentModifiedEventArgs> Modified, EventHandler<DocumentStateEventArgs> State, EventHandler<AfterAddObjectEventArgs> Added, EventHandler<AfterRemoveObjectEventArgs> Removed, EventHandler<ObjectEventArgs> Expired, EventHandler<ObjectEventArgs> Selection, EventHandler<ObjectEventArgs> Enabled, EventHandler<ObjectEventArgs> Relevance, EventHandler<ObjectEventArgs> Layout, EventHandler<ObjectEventArgs> Display, EventHandler<ObjectNameEventArgs> Name, EventHandler<ObjectGuidEventArgs> Id) {
    internal static DocumentEventHandlers Combine(Seq<DocumentEventHandlers> handlers) =>
        handlers.Fold(
            initialState: Empty,
            f: static (left, right) => new DocumentEventHandlers(
                Modified: left.Modified + right.Modified,
                State: left.State + right.State,
                Added: left.Added + right.Added,
                Removed: left.Removed + right.Removed,
                Expired: left.Expired + right.Expired,
                Selection: left.Selection + right.Selection,
                Enabled: left.Enabled + right.Enabled,
                Relevance: left.Relevance + right.Relevance,
                Layout: left.Layout + right.Layout,
                Display: left.Display + right.Display,
                Name: left.Name + right.Name,
                Id: left.Id + right.Id));
    internal static DocumentEventHandlers Empty => default;
}

public readonly record struct DocumentEvent(string Name, Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers> Attach, Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers> Detach, Func<DocumentEvent, DocumentEventPipe, DocumentEventHandlers> Build) {
    internal DocumentEventHandlers Handlers(GhDocument doc, GhObjectList objs, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        Build(arg1: this, arg2: new DocumentEventPipe(Document: doc, Objects: objs, Handler: handler));

    internal static DocumentEvent Composite(string name, params DocumentEvent[] members) =>
        new(
            Name: name,
            Attach: (owner, handlers) => toSeq(members).Iter(member => member.Attach(owner, handlers)),
            Detach: (owner, handlers) => toSeq(members).Iter(member => member.Detach(owner, handlers)),
            Build: (_, pipe) => DocumentEventHandlers.Combine(handlers: toSeq(members).Map(member => member.Build(arg1: member, arg2: pipe))));
}

public sealed record DocumentEventPipe(
    GhDocument Document,
    GhObjectList Objects,
    Func<DocumentEventSnapshot, Fin<Unit>> Handler) {
    internal Unit Publish(DocumentEvent kind, Option<IDocumentObject> changed = default, Option<string> detail = default) {
        bool selection = string.Equals(kind.Name, nameof(DocumentEventKind.Selection), StringComparison.Ordinal);
        _ = GrasshopperUi.Handler(valid: () => Handler(arg: new DocumentEventSnapshot(
            Kind: kind,
            Document: UiRail.DocumentSnapshotOf(document: Document, objects: Objects),
            Objects: selection
                ? toSeq(Objects.SelectedObjects.Select(UiRail.DocumentObjectSnapshotOf))
                : changed.Map(obj => Seq(UiRail.DocumentObjectSnapshotOf(obj))).IfNone(Seq<DocumentObjectSnapshot>()),
            Wires: selection
                ? Optional(Objects.SelectedWires).Map(wires => toSeq(wires).Map(wire => Wire.SnapshotConnected(objects: Objects, wire: wire))).IfNone(Seq<WireSnapshot.ConnectedCase>())
                : Seq<WireSnapshot.ConnectedCase>(),
            Detail: detail)));
        return unit;
    }
}

public readonly record struct SolutionEventHandlers(EventHandler<SolutionIdEventArgs> About, EventHandler<SolutionEventArgs> Plain, EventHandler<SolutionExceptionEventArgs> Faulted);

public readonly record struct UndoEventHandlers(EventHandler<UndoEventArgs> Plain, EventHandler<UndoNodeEventArgs> Node, EventHandler<UndoNodeMovedEventArgs> Moved);

public readonly record struct ControlEventHandlers(EventHandler<EventArgs> Plain, EventHandler<KeyEventArgs> Key, EventHandler<TextInputEventArgs> Text, EventHandler<MouseEventArgs> Mouse, EventHandler<DragEventArgs> Drag);

public static class DocumentEventKind {
    public static readonly DocumentEvent Modified = OnModified(nameof(Modified), static (d, h) => d.ModifiedChanged += h, static (d, h) => d.ModifiedChanged -= h);
    public static readonly DocumentEvent StateChanged = OnState(nameof(StateChanged), static (d, h) => d.StateChanged += h, static (d, h) => d.StateChanged -= h);
    public static readonly DocumentEvent ObjectAdded = OnObject(nameof(ObjectAdded), static (o, h) => o.ObjectAdded += h, static (o, h) => o.ObjectAdded -= h, static h => h.Added, static (h, next) => h with { Added = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectRemoved = OnObject(nameof(ObjectRemoved), static (o, h) => o.ObjectRemoved += h, static (o, h) => o.ObjectRemoved -= h, static h => h.Removed, static (h, next) => h with { Removed = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectExpired = OnObject(nameof(ObjectExpired), static (o, h) => o.ObjectExpired += h, static (o, h) => o.ObjectExpired -= h, static h => h.Expired, static (h, next) => h with { Expired = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectName = OnName(nameof(ObjectName), static (o, h) => o.ObjectNameChanged += h, static (o, h) => o.ObjectNameChanged -= h);
    public static readonly DocumentEvent Selection = OnObject(nameof(Selection), static (o, h) => o.ObjectSelectionChanged += h, static (o, h) => o.ObjectSelectionChanged -= h, static h => h.Selection, static (h, next) => h with { Selection = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectEnabled = OnObject(nameof(ObjectEnabled), static (o, h) => o.ObjectEnabledChanged += h, static (o, h) => o.ObjectEnabledChanged -= h, static h => h.Enabled, static (h, next) => h with { Enabled = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectRelevance = OnObject(nameof(ObjectRelevance), static (o, h) => o.ObjectRelevanceChanged += h, static (o, h) => o.ObjectRelevanceChanged -= h, static h => h.Relevance, static (h, next) => h with { Relevance = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectLayout = OnObject(nameof(ObjectLayout), static (o, h) => o.ObjectLayoutChanged += h, static (o, h) => o.ObjectLayoutChanged -= h, static h => h.Layout, static (h, next) => h with { Layout = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectDisplay = OnObject(nameof(ObjectDisplay), static (o, h) => o.ObjectDisplayChanged += h, static (o, h) => o.ObjectDisplayChanged -= h, static h => h.Display, static (h, next) => h with { Display = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectInstanceId = OnId(nameof(ObjectInstanceId), static (o, h) => o.ObjectInstanceIdChanged += h, static (o, h) => o.ObjectInstanceIdChanged -= h);
    public static readonly DocumentEvent Changed = DocumentEvent.Composite(
        name: nameof(Changed),
        Modified, StateChanged, ObjectAdded, ObjectRemoved, ObjectExpired, ObjectName, Selection,
        ObjectEnabled, ObjectRelevance, ObjectLayout, ObjectDisplay, ObjectInstanceId);

    private static DocumentEvent OnModified(string name, Action<GhDocument, EventHandler<DocumentModifiedEventArgs>> attach, Action<GhDocument, EventHandler<DocumentModifiedEventArgs>> detach) =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Doc, arg2: handlers.Modified),
            Detach: (owner, handlers) => detach(arg1: owner.Doc, arg2: handlers.Modified),
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with { Modified = (_, e) => pipe.Publish(kind: kind, detail: Some($"{e.Oldstate}->{e.NewState}")) });
    private static DocumentEvent OnState(string name, Action<GhDocument, EventHandler<DocumentStateEventArgs>> attach, Action<GhDocument, EventHandler<DocumentStateEventArgs>> detach) =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Doc, arg2: handlers.State),
            Detach: (owner, handlers) => detach(arg1: owner.Doc, arg2: handlers.State),
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with { State = (_, e) => pipe.Publish(kind: kind, detail: Some($"{e.Oldstate}->{e.NewState}")) });
    private static DocumentEvent OnObject<TArgs>(string name, Action<GhObjectList, EventHandler<TArgs>> attach, Action<GhObjectList, EventHandler<TArgs>> detach, Func<DocumentEventHandlers, EventHandler<TArgs>> select, Func<DocumentEventHandlers, EventHandler<TArgs>, DocumentEventHandlers> assign, Func<TArgs, IDocumentObject?> changed) where TArgs : EventArgs =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Objs, arg2: select(arg: handlers)),
            Detach: (owner, handlers) => detach(arg1: owner.Objs, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(DocumentEventHandlers.Empty, (_, e) => pipe.Publish(kind: kind, changed: Optional(changed(arg: e)))));
    private static DocumentEvent OnName(string name, Action<GhObjectList, EventHandler<ObjectNameEventArgs>> attach, Action<GhObjectList, EventHandler<ObjectNameEventArgs>> detach) =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Objs, arg2: handlers.Name),
            Detach: (owner, handlers) => detach(arg1: owner.Objs, arg2: handlers.Name),
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with { Name = (_, e) => pipe.Publish(kind: kind, changed: Optional(e.Owner), detail: Some($"{e.Old}->{e.New}")) });
    private static DocumentEvent OnId(string name, Action<GhObjectList, EventHandler<ObjectGuidEventArgs>> attach, Action<GhObjectList, EventHandler<ObjectGuidEventArgs>> detach) =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Objs, arg2: handlers.Id),
            Detach: (owner, handlers) => detach(arg1: owner.Objs, arg2: handlers.Id),
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with { Id = (_, e) => pipe.Publish(kind: kind, changed: Optional(e.Object), detail: Some($"{e.OldId}->{e.NewId}")) });
}

public static class SolutionEventKind {
    public static readonly SolutionEvent AboutToStart = SolutionEvent.On(nameof(AboutToStart), static (s, h) => s.SolutionAboutToStart += h, static (s, h) => s.SolutionAboutToStart -= h, static e => e.About);
    public static readonly SolutionEvent Started = SolutionEvent.On(nameof(Started), static (s, h) => s.SolutionStarted += h, static (s, h) => s.SolutionStarted -= h, static e => e.Plain);
    public static readonly SolutionEvent Stopped = SolutionEvent.On(nameof(Stopped), static (s, h) => s.SolutionStopped += h, static (s, h) => s.SolutionStopped -= h, static e => e.Plain);
    public static readonly SolutionEvent Cancelled = SolutionEvent.On(nameof(Cancelled), static (s, h) => s.SolutionCancelled += h, static (s, h) => s.SolutionCancelled -= h, static e => e.Plain);
    public static readonly SolutionEvent Completed = SolutionEvent.On(nameof(Completed), static (s, h) => s.SolutionCompleted += h, static (s, h) => s.SolutionCompleted -= h, static e => e.Plain);
    public static readonly SolutionEvent Faulted = SolutionEvent.On(nameof(Faulted), static (s, h) => s.SolutionFaulted += h, static (s, h) => s.SolutionFaulted -= h, static e => e.Faulted);
}

public static class UndoEventKind {
    public static readonly UndoEvent Undone = UndoEvent.On(nameof(Undone), static (h, x) => h.Undone += x, static (h, x) => h.Undone -= x, static e => e.Plain);
    public static readonly UndoEvent Redone = UndoEvent.On(nameof(Redone), static (h, x) => h.Redone += x, static (h, x) => h.Redone -= x, static e => e.Plain);
    public static readonly UndoEvent Modified = UndoEvent.On(nameof(Modified), static (h, x) => h.Modified += x, static (h, x) => h.Modified -= x, static e => e.Plain);
    public static readonly UndoEvent NodeAdded = UndoEvent.On(nameof(NodeAdded), static (h, x) => h.NodeAdded += x, static (h, x) => h.NodeAdded -= x, static e => e.Node);
    public static readonly UndoEvent NodeRemoved = UndoEvent.On(nameof(NodeRemoved), static (h, x) => h.NodeRemoved += x, static (h, x) => h.NodeRemoved -= x, static e => e.Node);
    public static readonly UndoEvent NodeMerged = UndoEvent.On(nameof(NodeMerged), static (h, x) => h.NodeMerged += x, static (h, x) => h.NodeMerged -= x, static e => e.Node);
    public static readonly UndoEvent NodeMoved = UndoEvent.On(nameof(NodeMoved), static (h, x) => h.NodeMoved += x, static (h, x) => h.NodeMoved -= x, static e => e.Moved);
}

public static class ControlEventKind {
    public static readonly ControlEvent PreLoad = OnPlain(nameof(PreLoad), static (s, h) => s.PreLoad += h, static (s, h) => s.PreLoad -= h);
    public static readonly ControlEvent Load = OnPlain(nameof(Load), static (s, h) => s.Load += h, static (s, h) => s.Load -= h);
    public static readonly ControlEvent LoadComplete = OnPlain(nameof(LoadComplete), static (s, h) => s.LoadComplete += h, static (s, h) => s.LoadComplete -= h);
    public static readonly ControlEvent UnLoad = OnPlain(nameof(UnLoad), static (s, h) => s.UnLoad += h, static (s, h) => s.UnLoad -= h);
    public static readonly ControlEvent Shown = OnPlain(nameof(Shown), static (s, h) => s.Shown += h, static (s, h) => s.Shown -= h);
    public static readonly ControlEvent GotFocus = OnPlain(nameof(GotFocus), static (s, h) => s.GotFocus += h, static (s, h) => s.GotFocus -= h);
    public static readonly ControlEvent LostFocus = OnPlain(nameof(LostFocus), static (s, h) => s.LostFocus += h, static (s, h) => s.LostFocus -= h);
    public static readonly ControlEvent SizeChanged = OnPlain(nameof(SizeChanged), static (s, h) => s.SizeChanged += h, static (s, h) => s.SizeChanged -= h);
    public static readonly ControlEvent EnabledChanged = OnPlain(nameof(EnabledChanged), static (s, h) => s.EnabledChanged += h, static (s, h) => s.EnabledChanged -= h);
    public static readonly ControlEvent KeyDown = OnKeyed(nameof(KeyDown), static (s, h) => s.KeyDown += h, static (s, h) => s.KeyDown -= h);
    public static readonly ControlEvent KeyUp = OnKeyed(nameof(KeyUp), static (s, h) => s.KeyUp += h, static (s, h) => s.KeyUp -= h);
    public static readonly ControlEvent TextInput = OnTextual(nameof(TextInput), static (s, h) => s.TextInput += h, static (s, h) => s.TextInput -= h);
    public static readonly ControlEvent MouseDown = OnMoused(nameof(MouseDown), static (s, h) => s.MouseDown += h, static (s, h) => s.MouseDown -= h);
    public static readonly ControlEvent MouseUp = OnMoused(nameof(MouseUp), static (s, h) => s.MouseUp += h, static (s, h) => s.MouseUp -= h);
    public static readonly ControlEvent MouseMove = OnMoused(nameof(MouseMove), static (s, h) => s.MouseMove += h, static (s, h) => s.MouseMove -= h);
    public static readonly ControlEvent MouseEnter = OnMoused(nameof(MouseEnter), static (s, h) => s.MouseEnter += h, static (s, h) => s.MouseEnter -= h);
    public static readonly ControlEvent MouseLeave = OnMoused(nameof(MouseLeave), static (s, h) => s.MouseLeave += h, static (s, h) => s.MouseLeave -= h);
    public static readonly ControlEvent MouseDoubleClick = OnMoused(nameof(MouseDoubleClick), static (s, h) => s.MouseDoubleClick += h, static (s, h) => s.MouseDoubleClick -= h);
    public static readonly ControlEvent MouseWheel = OnMoused(nameof(MouseWheel), static (s, h) => s.MouseWheel += h, static (s, h) => s.MouseWheel -= h);
    public static readonly ControlEvent DragDrop = OnDragged(nameof(DragDrop), static (s, h) => s.DragDrop += h, static (s, h) => s.DragDrop -= h);
    public static readonly ControlEvent DragOver = OnDragged(nameof(DragOver), static (s, h) => s.DragOver += h, static (s, h) => s.DragOver -= h);
    public static readonly ControlEvent DragEnter = OnDragged(nameof(DragEnter), static (s, h) => s.DragEnter += h, static (s, h) => s.DragEnter -= h);
    public static readonly ControlEvent DragLeave = OnDragged(nameof(DragLeave), static (s, h) => s.DragLeave += h, static (s, h) => s.DragLeave -= h);
    public static readonly ControlEvent DragEnd = OnDragged(nameof(DragEnd), static (s, h) => s.DragEnd += h, static (s, h) => s.DragEnd -= h);

    private static ControlEvent OnPlain(string name, Action<Control, EventHandler<EventArgs>> attach, Action<Control, EventHandler<EventArgs>> detach) =>
        ControlEvent.On(name, attach, detach, static h => h.Plain);
    private static ControlEvent OnKeyed(string name, Action<Control, EventHandler<KeyEventArgs>> attach, Action<Control, EventHandler<KeyEventArgs>> detach) =>
        ControlEvent.On(name, attach, detach, static h => h.Key);
    private static ControlEvent OnTextual(string name, Action<Control, EventHandler<TextInputEventArgs>> attach, Action<Control, EventHandler<TextInputEventArgs>> detach) =>
        ControlEvent.On(name, attach, detach, static h => h.Text);
    private static ControlEvent OnMoused(string name, Action<Control, EventHandler<MouseEventArgs>> attach, Action<Control, EventHandler<MouseEventArgs>> detach) =>
        ControlEvent.On(name, attach, detach, static h => h.Mouse);
    private static ControlEvent OnDragged(string name, Action<Control, EventHandler<DragEventArgs>> attach, Action<Control, EventHandler<DragEventArgs>> detach) =>
        ControlEvent.On(name, attach, detach, static h => h.Drag);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentEventSnapshot(DocumentEvent Kind, DocumentSnapshot Document, Seq<DocumentObjectSnapshot> Objects, Seq<WireSnapshot.ConnectedCase> Wires, Option<string> Detail);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SolutionEventSnapshot(SolutionEvent Kind, DocumentSnapshot Document);

[StructLayout(LayoutKind.Auto)]
public readonly record struct UndoEventSnapshot(UndoEvent Kind, DocumentHistorySnapshot History);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ControlEventSnapshot(ControlEvent Kind, bool Enabled, bool Visible, bool HasFocus, Option<PointF> Point = default, Option<MouseButtons> Buttons = default, Option<Keys> Keys = default, Option<string> Text = default, Option<SizeF> Delta = default, Option<float> Pressure = default, Option<DragEffects> DragEffects = default, Option<DragEffects> AllowedDragEffects = default, Option<IDataObject> DragData = default);

internal abstract record EventRequest : GhUiRequest<Subscription> {
    internal sealed record Run(UiEvent Event) : EventRequest { internal override GrasshopperUiPolicy Policy => Event.UiPolicy; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Events.Subscribe(uiEvent: Event).Run(scope: scope); }
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Events {
    internal static GrasshopperUiIntent<Subscription> Subscribe(UiEvent uiEvent) =>
        uiEvent.Switch(
            paintCase: static p => SubscribePaint(phase: p.Phase, handler: p.Handler, clock: p.Clock),
            documentCase: static d => SubscribeDocument(kind: d.Kind, handler: d.Handler),
            solutionCase: static s => SubscribeSolution(kind: s.Kind, handler: s.Handler),
            undoCase: static u => SubscribeUndo(kind: u.Kind, handler: u.Handler),
            timerCase: static t => SubscribeTimer(interval: t.Interval, handler: t.Handler),
            controlCase: static c => SubscribeControl(source: c.Source, kind: c.Kind, handler: c.Handler));

    private static GrasshopperUiIntent<Subscription> SubscribePaint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler, MotionClock? clock) =>
        Paint.Hook(phase: phase, paint: handler, clock: clock ?? MotionClock.MessageLoop);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static GrasshopperUiIntent<Subscription> SubscribePublish<TSnapshot, TOwner, THandlers>(
        string opName,
        EventSource<TOwner, THandlers> kind,
        Func<TSnapshot, Fin<Unit>> handler,
        Func<GhDocument, GhObjectList, TSnapshot> snapshot,
        Func<GhDocument, TOwner> owner,
        Func<System.Action, THandlers> handlers) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: opName), detail: "null handler"))
            let publish = (System.Action)(() => GrasshopperUi.Handler(valid: () => valid(arg: snapshot(doc, objs))).Ignore())
            let events = handlers(publish)
            let attachOwner = owner(doc)
            from sub in Subscription.Bind(
                attach: () => kind.Attach(attachOwner, events),
                detach: () => kind.Detach(attachOwner, events),
                marshalToUi: true)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeDocument(DocumentEvent kind, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            let owner = (Doc: doc, Objs: objs)
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeDocument)), detail: "null handler"))
            let events = kind.Handlers(doc: owner.Doc, objs: owner.Objs, handler: valid)
            from sub in Subscription.Bind(
                attach: () => kind.Attach(owner, events),
                detach: () => kind.Detach(owner, events),
                marshalToUi: true)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeSolution(SolutionEvent kind, Func<SolutionEventSnapshot, Fin<Unit>> handler) =>
        SubscribePublish(
            opName: nameof(SubscribeSolution),
            kind: kind,
            handler: handler,
            snapshot: (doc, objs) => new SolutionEventSnapshot(Kind: kind, Document: UiRail.DocumentSnapshotOf(document: doc, objects: objs)),
            owner: static doc => doc.Solution,
            handlers: static publish => new SolutionEventHandlers(About: (_, _) => publish(), Plain: (_, _) => publish(), Faulted: (_, _) => publish()));

    private static GrasshopperUiIntent<Subscription> SubscribeUndo(UndoEvent kind, Func<UndoEventSnapshot, Fin<Unit>> handler) =>
        SubscribePublish(
            opName: nameof(SubscribeUndo),
            kind: kind,
            handler: handler,
            snapshot: (doc, _) => new UndoEventSnapshot(Kind: kind, History: UiRail.HistorySnapshotOf(document: doc)),
            owner: static doc => doc.Undo,
            handlers: static publish => new UndoEventHandlers(Plain: (_, _) => publish(), Node: (_, _) => publish(), Moved: (_, _) => publish()));

    private static GrasshopperUiIntent<Subscription> SubscribeTimer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        GhUi.Read(run: scope =>
            from validInterval in Optional(interval).Filter(static value => value > TimeSpan.Zero)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "interval must be positive"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "null handler"))
            let scope2 = new TimerScope(interval: validInterval, tick: () => GrasshopperUi.Handler(valid: valid).Ignore())
            from sub in Subscription.Bind(
                attach: scope2.Start,
                detach: scope2.Dispose,
                marshalToUi: true,
                detachOnce: true)
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

    private static GrasshopperUiIntent<Subscription> SubscribeControl(Control source, ControlEvent kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Read(run: scope =>
            from validSource in Optional(source).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null control"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null handler"))
            from events in BuildControlHandlers(source: validSource, kind: kind, handler: valid)
            from sub in Subscription.Bind(
                attach: () => kind.Attach(validSource, events),
                detach: () => kind.Detach(validSource, events),
                marshalToUi: true)
            select sub);

    private static Fin<ControlEventHandlers> BuildControlHandlers(Control source, ControlEvent kind, Func<ControlEventSnapshot, Fin<Unit>> handler) {
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
            _ = GrasshopperUi.Handler(valid: () => handler(arg: snapshot));
            return unit;
        }
        return kind.Name switch {
            nameof(ControlEventKind.PreLoad) or nameof(ControlEventKind.Load) or nameof(ControlEventKind.LoadComplete) or nameof(ControlEventKind.UnLoad) or nameof(ControlEventKind.Shown) or nameof(ControlEventKind.GotFocus) or nameof(ControlEventKind.LostFocus) or nameof(ControlEventKind.SizeChanged) or nameof(ControlEventKind.EnabledChanged) => Fin.Succ(IgnoreControlHandlers with { Plain = (_, _) => Publish(Snapshot()) }),
            nameof(ControlEventKind.KeyDown) or nameof(ControlEventKind.KeyUp) => Fin.Succ(IgnoreControlHandlers with { Key = (_, e) => Publish(Snapshot(keys: Some(e.KeyData))) }),
            nameof(ControlEventKind.TextInput) => Fin.Succ(IgnoreControlHandlers with { Text = (_, e) => Publish(Snapshot(text: Optional(e.Text))) }),
            nameof(ControlEventKind.MouseDown) or nameof(ControlEventKind.MouseUp) or nameof(ControlEventKind.MouseMove) or nameof(ControlEventKind.MouseEnter) or nameof(ControlEventKind.MouseLeave) or nameof(ControlEventKind.MouseDoubleClick) or nameof(ControlEventKind.MouseWheel) => Fin.Succ(IgnoreControlHandlers with { Mouse = (_, e) => Publish(Snapshot(point: Some(e.Location), buttons: Some(e.Buttons), keys: Some(e.Modifiers), delta: Some(e.Delta), pressure: Some(e.Pressure))) }),
            nameof(ControlEventKind.DragDrop) or nameof(ControlEventKind.DragOver) or nameof(ControlEventKind.DragEnter) or nameof(ControlEventKind.DragLeave) or nameof(ControlEventKind.DragEnd) => Fin.Succ(IgnoreControlHandlers with { Drag = (_, e) => Publish(Snapshot(point: Some(e.Location), keys: Some(e.Modifiers), dragEffects: Some(e.Effects), allowedDragEffects: Some(e.AllowedEffects), dragData: Optional<IDataObject>(value: e.Data))) }),
            _ => Fin.Fail<ControlEventHandlers>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(BuildControlHandlers)), detail: $"unsupported control event '{kind.Name}'")),
        };
    }

    private static ControlEventHandlers IgnoreControlHandlers => default;
}
