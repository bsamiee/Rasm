using System.Diagnostics.CodeAnalysis;
using Eto.Forms;
using Grasshopper2.Doc;
using Grasshopper2.Undo;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

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
    public sealed record KeyboardModifiersCase(Func<InputModifierSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record LogicalPixelSizeCase(Window Window, Func<float, Fin<Unit>> Handler) : UiEvent;

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
    public static UiEvent KeyboardModifiers(Func<InputModifierSnapshot, Fin<Unit>> handler) =>
        new KeyboardModifiersCase(Handler: handler);
    public static UiEvent LogicalPixelSize(Window window, Func<float, Fin<Unit>> handler) =>
        new LogicalPixelSizeCase(Window: window, Handler: handler);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        paintCase: static _ => GrasshopperUiPolicy.Canvas(),
        documentCase: static _ => GrasshopperUiPolicy.Document(),
        solutionCase: static _ => GrasshopperUiPolicy.Document(),
        undoCase: static _ => GrasshopperUiPolicy.Document(),
        timerCase: static _ => GrasshopperUiPolicy.Read,
        controlCase: static _ => GrasshopperUiPolicy.Read,
        keyboardModifiersCase: static _ => GrasshopperUiPolicy.Read,
        logicalPixelSizeCase: static _ => GrasshopperUiPolicy.Read);
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
    internal Unit PublishSelection(DocumentEvent kind) =>
        Publish(
            kind: kind,
            objects: toSeq(Objects.SelectedObjects.Select(UiRail.DocumentObjectSnapshotOf)),
            wires: Optional(Objects.SelectedWires)
                .Map(wires => toSeq(wires).Map(wire => Wire.SnapshotConnected(objects: Objects, wire: wire)))
                .IfNone(Seq<WireSnapshot.ConnectedCase>()));

    internal Unit PublishObject(DocumentEvent kind, Option<IDocumentObject> changed, Option<string> detail = default) =>
        Publish(
            kind: kind,
            objects: changed.Map(obj => Seq(UiRail.DocumentObjectSnapshotOf(obj))).IfNone(Seq<DocumentObjectSnapshot>()),
            wires: Seq<WireSnapshot.ConnectedCase>(),
            detail: detail);

    internal Unit PublishDetail(DocumentEvent kind, Option<string> detail) =>
        PublishObject(kind: kind, changed: default, detail: detail);

    private Unit Publish(
        DocumentEvent kind,
        Seq<DocumentObjectSnapshot> objects,
        Seq<WireSnapshot.ConnectedCase> wires,
        Option<string> detail = default) {
        _ = GrasshopperUi.Handler(valid: () => Handler(arg: new DocumentEventSnapshot(
            Kind: kind,
            Document: UiRail.DocumentSnapshotOf(document: Document, objects: Objects),
            Objects: objects,
            Wires: wires,
            Detail: detail)));
        return unit;
    }
}

public readonly record struct SolutionEventHandlers(EventHandler<SolutionIdEventArgs> About, EventHandler<SolutionEventArgs> Plain, EventHandler<SolutionExceptionEventArgs> Faulted) {
    internal static SolutionEventHandlers Empty => default;
}

public readonly record struct SolutionEvent(
    string Name,
    Action<SolutionServer, SolutionEventHandlers> Attach,
    Action<SolutionServer, SolutionEventHandlers> Detach,
    Func<SolutionEvent, SolutionEventPipe, SolutionEventHandlers> Build) {
    internal SolutionEventHandlers Handlers(SolutionServer server, GhDocument document, GhObjectList objects, Func<SolutionEventSnapshot, Fin<Unit>> handler) =>
        Build(arg1: this, arg2: new SolutionEventPipe(Server: server, Document: document, Objects: objects, Handler: handler));
}

public sealed record SolutionEventPipe(SolutionServer Server, GhDocument Document, GhObjectList Objects, Func<SolutionEventSnapshot, Fin<Unit>> Handler) {
    internal Unit Publish(SolutionEvent kind, Option<string> solutionKey = default, Option<string> faultMessage = default, Option<string> faultType = default) {
        _ = GrasshopperUi.Handler(valid: () => Handler(arg: new SolutionEventSnapshot(
            Kind: kind,
            Document: UiRail.DocumentSnapshotOf(document: Document, objects: Objects),
            SolutionKey: solutionKey,
            FaultMessage: faultMessage,
            FaultType: faultType)));
        return unit;
    }

    internal Unit PublishAbout(SolutionEvent kind, SolutionIdEventArgs args) =>
        Publish(kind: kind, solutionKey: Some(args.Id.ToString()));

    internal Unit PublishPlain(SolutionEvent kind) => Publish(kind: kind);

    internal Unit PublishFaulted(SolutionEvent kind, SolutionExceptionEventArgs args) =>
        Publish(
            kind: kind,
            faultMessage: Optional(args.Exception?.Message),
            faultType: Optional(args.Exception?.GetType().FullName));
}

public readonly record struct UndoEventHandlers(EventHandler<UndoEventArgs> Plain, EventHandler<UndoNodeEventArgs> Node, EventHandler<UndoNodeMovedEventArgs> Moved) {
    internal static UndoEventHandlers Empty => default;
}

public readonly record struct UndoEvent(
    string Name,
    Action<History, UndoEventHandlers> Attach,
    Action<History, UndoEventHandlers> Detach,
    Func<UndoEvent, UndoEventPipe, UndoEventHandlers> Build) {
    internal UndoEventHandlers Handlers(History history, GhDocument document, Func<UndoEventSnapshot, Fin<Unit>> handler) =>
        Build(arg1: this, arg2: new UndoEventPipe(History: history, Document: document, Handler: handler));
}

public sealed record UndoEventPipe(History History, GhDocument Document, Func<UndoEventSnapshot, Fin<Unit>> Handler) {
    internal Unit Publish(UndoEvent kind, Option<string> nodeName = default, Option<int> nodeCount = default) {
        _ = GrasshopperUi.Handler(valid: () => Handler(arg: new UndoEventSnapshot(
            Kind: kind,
            History: UiRail.HistorySnapshotOf(document: Document),
            NodeName: nodeName,
            NodeCount: nodeCount)));
        return unit;
    }

    internal Unit PublishPlain(UndoEvent kind) => Publish(kind: kind);

    internal Unit PublishNode(UndoEvent kind, UndoNodeEventArgs args) =>
        Publish(
            kind: kind,
            nodeName: Optional(args.Node?.Record?.Name.ToString()),
            nodeCount: Optional(args.Node?.Record?.Count));

    internal Unit PublishMoved(UndoEvent kind, UndoNodeMovedEventArgs args) =>
        Publish(
            kind: kind,
            nodeName: Optional(args.Nodes).Bind(static nodes => nodes.Length > 0 ? Optional(nodes[0].Record?.Name.ToString()) : Option<string>.None),
            nodeCount: Optional(args.Nodes).Map(static nodes => nodes.Length));
}

public readonly record struct ControlEventHandlers(EventHandler<EventArgs> Plain, EventHandler<KeyEventArgs> Key, EventHandler<TextInputEventArgs> Text, EventHandler<MouseEventArgs> Mouse, EventHandler<DragEventArgs> Drag) {
    internal static ControlEventHandlers Empty => default;
}

public readonly record struct ControlEvent(
    string Name,
    Action<Control, ControlEventHandlers> Attach,
    Action<Control, ControlEventHandlers> Detach,
    Func<ControlEvent, ControlEventPipe, ControlEventHandlers> Build) {
    internal ControlEventHandlers Handlers(Control source, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        Build(arg1: this, arg2: new ControlEventPipe(Source: source, Handler: handler));
}

public sealed record ControlEventPipe(Control Source, Func<ControlEventSnapshot, Fin<Unit>> Handler) {
    internal Unit Publish(ControlEvent kind, ControlEventSnapshot snapshot) {
        _ = GrasshopperUi.Handler(valid: () => Handler(arg: snapshot with { Kind = kind }));
        return unit;
    }
}

public static class DocumentEventKind {
    public static readonly DocumentEvent Modified = OnModified(nameof(Modified), static (d, h) => d.ModifiedChanged += h, static (d, h) => d.ModifiedChanged -= h);
    public static readonly DocumentEvent StateChanged = OnState(nameof(StateChanged), static (d, h) => d.StateChanged += h, static (d, h) => d.StateChanged -= h);
    public static readonly DocumentEvent ObjectAdded = OnObject(nameof(ObjectAdded), static (o, h) => o.ObjectAdded += h, static (o, h) => o.ObjectAdded -= h, static h => h.Added, static (h, next) => h with { Added = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectRemoved = OnObject(nameof(ObjectRemoved), static (o, h) => o.ObjectRemoved += h, static (o, h) => o.ObjectRemoved -= h, static h => h.Removed, static (h, next) => h with { Removed = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectExpired = OnObject(nameof(ObjectExpired), static (o, h) => o.ObjectExpired += h, static (o, h) => o.ObjectExpired -= h, static h => h.Expired, static (h, next) => h with { Expired = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectName = OnName(nameof(ObjectName), static (o, h) => o.ObjectNameChanged += h, static (o, h) => o.ObjectNameChanged -= h);
    public static readonly DocumentEvent Selection = OnSelection(
        name: nameof(Selection),
        attach: static (o, h) => o.ObjectSelectionChanged += h,
        detach: static (o, h) => o.ObjectSelectionChanged -= h,
        select: static h => h.Selection,
        assign: static (left, right) => left with { Selection = right });
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
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with { Modified = (_, e) => pipe.PublishDetail(kind: kind, detail: Some($"{e.Oldstate}->{e.NewState}")) });
    private static DocumentEvent OnState(string name, Action<GhDocument, EventHandler<DocumentStateEventArgs>> attach, Action<GhDocument, EventHandler<DocumentStateEventArgs>> detach) =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Doc, arg2: handlers.State),
            Detach: (owner, handlers) => detach(arg1: owner.Doc, arg2: handlers.State),
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with { State = (_, e) => pipe.PublishDetail(kind: kind, detail: Some($"{e.Oldstate}->{e.NewState}")) });
    private static DocumentEvent OnObject<TArgs>(
        string name,
        Action<GhObjectList, EventHandler<TArgs>> attach,
        Action<GhObjectList, EventHandler<TArgs>> detach,
        Func<DocumentEventHandlers, EventHandler<TArgs>> select,
        Func<DocumentEventHandlers, EventHandler<TArgs>, DocumentEventHandlers> assign,
        Func<TArgs, IDocumentObject?> changed) where TArgs : EventArgs =>
        new(
            Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Objs, arg2: select(arg: handlers)),
            Detach: (owner, handlers) => detach(arg1: owner.Objs, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(
                arg1: DocumentEventHandlers.Empty,
                arg2: (_, e) => pipe.PublishObject(kind: kind, changed: Optional(changed(arg: e)))));
    private static DocumentEvent OnSelection<TArgs>(
        string name,
        Action<GhObjectList, EventHandler<TArgs>> attach,
        Action<GhObjectList, EventHandler<TArgs>> detach,
        Func<DocumentEventHandlers, EventHandler<TArgs>> select,
        Func<DocumentEventHandlers, EventHandler<TArgs>, DocumentEventHandlers> assign) where TArgs : EventArgs =>
        new(
            Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Objs, arg2: select(arg: handlers)),
            Detach: (owner, handlers) => detach(arg1: owner.Objs, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(
                arg1: DocumentEventHandlers.Empty,
                arg2: (_, _) => pipe.PublishSelection(kind: kind)));
    private static DocumentEvent OnName(string name, Action<GhObjectList, EventHandler<ObjectNameEventArgs>> attach, Action<GhObjectList, EventHandler<ObjectNameEventArgs>> detach) =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Objs, arg2: handlers.Name),
            Detach: (owner, handlers) => detach(arg1: owner.Objs, arg2: handlers.Name),
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with {
                Name = (_, e) => pipe.PublishObject(kind: kind, changed: Optional(e.Owner), detail: Some($"{e.Old}->{e.New}")),
            });
    private static DocumentEvent OnId(string name, Action<GhObjectList, EventHandler<ObjectGuidEventArgs>> attach, Action<GhObjectList, EventHandler<ObjectGuidEventArgs>> detach) =>
        new(Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Objs, arg2: handlers.Id),
            Detach: (owner, handlers) => detach(arg1: owner.Objs, arg2: handlers.Id),
            Build: static (kind, pipe) => DocumentEventHandlers.Empty with {
                Id = (_, e) => pipe.PublishObject(kind: kind, changed: Optional(e.Object), detail: Some($"{e.OldId}->{e.NewId}")),
            });
}

public static class SolutionEventKind {
    public static readonly SolutionEvent AboutToStart = On(
        name: nameof(AboutToStart),
        attach: static (s, h) => s.SolutionAboutToStart += h,
        detach: static (s, h) => s.SolutionAboutToStart -= h,
        select: static e => e.About,
        assign: static (left, right) => left with { About = right },
        handler: static (kind, pipe) => (_, e) => pipe.PublishAbout(kind: kind, args: e));
    public static readonly SolutionEvent Started = OnPlain(name: nameof(Started), attach: static (s, h) => s.SolutionStarted += h, detach: static (s, h) => s.SolutionStarted -= h);
    public static readonly SolutionEvent Stopped = OnPlain(name: nameof(Stopped), attach: static (s, h) => s.SolutionStopped += h, detach: static (s, h) => s.SolutionStopped -= h);
    public static readonly SolutionEvent Cancelled = OnPlain(name: nameof(Cancelled), attach: static (s, h) => s.SolutionCancelled += h, detach: static (s, h) => s.SolutionCancelled -= h);
    public static readonly SolutionEvent Completed = OnPlain(name: nameof(Completed), attach: static (s, h) => s.SolutionCompleted += h, detach: static (s, h) => s.SolutionCompleted -= h);
    public static readonly SolutionEvent Faulted = On(
        name: nameof(Faulted),
        attach: static (s, h) => s.SolutionFaulted += h,
        detach: static (s, h) => s.SolutionFaulted -= h,
        select: static e => e.Faulted,
        assign: static (left, right) => left with { Faulted = right },
        handler: static (kind, pipe) => (_, e) => pipe.PublishFaulted(kind: kind, args: e));

    private static SolutionEvent OnPlain(string name, Action<SolutionServer, EventHandler<SolutionEventArgs>> attach, Action<SolutionServer, EventHandler<SolutionEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Plain,
            assign: static (left, right) => left with { Plain = right },
            handler: static (kind, pipe) => (_, _) => pipe.PublishPlain(kind: kind));

    private static SolutionEvent On<TArgs>(
        string name,
        Action<SolutionServer, EventHandler<TArgs>> attach,
        Action<SolutionServer, EventHandler<TArgs>> detach,
        Func<SolutionEventHandlers, EventHandler<TArgs>> select,
        Func<SolutionEventHandlers, EventHandler<TArgs>, SolutionEventHandlers> assign,
        Func<SolutionEvent, SolutionEventPipe, EventHandler<TArgs>> handler) where TArgs : EventArgs =>
        new(
            Name: name,
            Attach: (server, handlers) => attach(arg1: server, arg2: select(arg: handlers)),
            Detach: (server, handlers) => detach(arg1: server, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(arg1: SolutionEventHandlers.Empty, arg2: handler(arg1: kind, arg2: pipe)));
}

public static class UndoEventKind {
    public static readonly UndoEvent Undone = OnPlain(name: nameof(Undone), attach: static (h, x) => h.Undone += x, detach: static (h, x) => h.Undone -= x);
    public static readonly UndoEvent Redone = OnPlain(name: nameof(Redone), attach: static (h, x) => h.Redone += x, detach: static (h, x) => h.Redone -= x);
    public static readonly UndoEvent Modified = OnPlain(name: nameof(Modified), attach: static (h, x) => h.Modified += x, detach: static (h, x) => h.Modified -= x);
    public static readonly UndoEvent NodeAdded = OnNode(name: nameof(NodeAdded), attach: static (h, x) => h.NodeAdded += x, detach: static (h, x) => h.NodeAdded -= x);
    public static readonly UndoEvent NodeRemoved = OnNode(name: nameof(NodeRemoved), attach: static (h, x) => h.NodeRemoved += x, detach: static (h, x) => h.NodeRemoved -= x);
    public static readonly UndoEvent NodeMerged = OnNode(name: nameof(NodeMerged), attach: static (h, x) => h.NodeMerged += x, detach: static (h, x) => h.NodeMerged -= x);
    public static readonly UndoEvent NodeMoved = On(
        name: nameof(NodeMoved),
        attach: static (h, x) => h.NodeMoved += x,
        detach: static (h, x) => h.NodeMoved -= x,
        select: static e => e.Moved,
        assign: static (left, right) => left with { Moved = right },
        handler: static (kind, pipe) => (_, e) => pipe.PublishMoved(kind: kind, args: e));

    private static UndoEvent OnPlain(string name, Action<History, EventHandler<UndoEventArgs>> attach, Action<History, EventHandler<UndoEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Plain,
            assign: static (left, right) => left with { Plain = right },
            handler: static (kind, pipe) => (_, _) => pipe.PublishPlain(kind: kind));

    private static UndoEvent OnNode(string name, Action<History, EventHandler<UndoNodeEventArgs>> attach, Action<History, EventHandler<UndoNodeEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Node,
            assign: static (left, right) => left with { Node = right },
            handler: static (kind, pipe) => (_, e) => pipe.PublishNode(kind: kind, args: e));

    private static UndoEvent On<TArgs>(
        string name,
        Action<History, EventHandler<TArgs>> attach,
        Action<History, EventHandler<TArgs>> detach,
        Func<UndoEventHandlers, EventHandler<TArgs>> select,
        Func<UndoEventHandlers, EventHandler<TArgs>, UndoEventHandlers> assign,
        Func<UndoEvent, UndoEventPipe, EventHandler<TArgs>> handler) where TArgs : EventArgs =>
        new(
            Name: name,
            Attach: (history, handlers) => attach(arg1: history, arg2: select(arg: handlers)),
            Detach: (history, handlers) => detach(arg1: history, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(arg1: UndoEventHandlers.Empty, arg2: handler(arg1: kind, arg2: pipe)));
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

    private static ControlEventSnapshot Snapshot(
        Control source,
        Option<PointF> point = default,
        Option<MouseButtons> buttons = default,
        Option<Keys> keys = default,
        Option<string> text = default,
        Option<SizeF> delta = default,
        Option<float> pressure = default,
        Option<DragEffects> dragEffects = default,
        Option<DragEffects> allowedDragEffects = default,
        Option<IDataObject> dragData = default) =>
        new(
            Kind: PreLoad,
            Enabled: source.Enabled,
            Visible: source.Visible,
            HasFocus: source.HasFocus,
            Point: point,
            Buttons: buttons,
            Keys: keys,
            Text: text,
            Delta: delta,
            Pressure: pressure,
            DragEffects: dragEffects,
            AllowedDragEffects: allowedDragEffects,
            DragData: dragData);

    private static ControlEvent OnPlain(string name, Action<Control, EventHandler<EventArgs>> attach, Action<Control, EventHandler<EventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Plain,
            assign: static (left, right) => left with { Plain = right },
            snapshot: static (pipe, _) => Snapshot(source: pipe.Source));

    private static ControlEvent OnKeyed(string name, Action<Control, EventHandler<KeyEventArgs>> attach, Action<Control, EventHandler<KeyEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Key,
            assign: static (left, right) => left with { Key = right },
            snapshot: static (pipe, e) => Snapshot(source: pipe.Source, keys: Some(e.KeyData)));

    private static ControlEvent OnTextual(string name, Action<Control, EventHandler<TextInputEventArgs>> attach, Action<Control, EventHandler<TextInputEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Text,
            assign: static (left, right) => left with { Text = right },
            snapshot: static (pipe, e) => Snapshot(source: pipe.Source, text: Optional(e.Text)));

    private static ControlEvent OnMoused(string name, Action<Control, EventHandler<MouseEventArgs>> attach, Action<Control, EventHandler<MouseEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Mouse,
            assign: static (left, right) => left with { Mouse = right },
            snapshot: static (pipe, e) => Snapshot(
                source: pipe.Source,
                point: Some(e.Location),
                buttons: Some(e.Buttons),
                keys: Some(e.Modifiers),
                delta: Some(e.Delta),
                pressure: Some(e.Pressure)));

    private static ControlEvent OnDragged(string name, Action<Control, EventHandler<DragEventArgs>> attach, Action<Control, EventHandler<DragEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Drag,
            assign: static (left, right) => left with { Drag = right },
            snapshot: static (pipe, e) => Snapshot(
                source: pipe.Source,
                point: Some(e.Location),
                keys: Some(e.Modifiers),
                dragEffects: Some(e.Effects),
                allowedDragEffects: Some(e.AllowedEffects),
                dragData: Optional<IDataObject>(value: e.Data)));

    private static ControlEvent On<TArgs>(
        string name,
        Action<Control, EventHandler<TArgs>> attach,
        Action<Control, EventHandler<TArgs>> detach,
        Func<ControlEventHandlers, EventHandler<TArgs>> select,
        Func<ControlEventHandlers, EventHandler<TArgs>, ControlEventHandlers> assign,
        Func<ControlEventPipe, TArgs, ControlEventSnapshot> snapshot) where TArgs : EventArgs =>
        new(
            Name: name,
            Attach: (source, handlers) => attach(arg1: source, arg2: select(arg: handlers)),
            Detach: (source, handlers) => detach(arg1: source, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(
                arg1: ControlEventHandlers.Empty,
                arg2: (_, e) => pipe.Publish(kind: kind, snapshot: snapshot(arg1: pipe, arg2: e))));
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentEventSnapshot(DocumentEvent Kind, DocumentSnapshot Document, Seq<DocumentObjectSnapshot> Objects, Seq<WireSnapshot.ConnectedCase> Wires, Option<string> Detail);

[StructLayout(LayoutKind.Auto)]
public readonly record struct SolutionEventSnapshot(
    SolutionEvent Kind,
    DocumentSnapshot Document,
    Option<string> SolutionKey = default,
    Option<string> FaultMessage = default,
    Option<string> FaultType = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct UndoEventSnapshot(
    UndoEvent Kind,
    DocumentHistorySnapshot History,
    Option<string> NodeName = default,
    Option<int> NodeCount = default);

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
            controlCase: static c => SubscribeControl(source: c.Source, kind: c.Kind, handler: c.Handler),
            keyboardModifiersCase: static k => SubscribeKeyboardModifiers(handler: k.Handler),
            logicalPixelSizeCase: static l => SubscribeLogicalPixelSize(window: l.Window, handler: l.Handler));

    private static GrasshopperUiIntent<Subscription> SubscribePaint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler, MotionClock? clock) =>
        Paint.Hook(phase: phase, paint: handler, clock: clock ?? MotionClock.MessageLoop);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static GrasshopperUiIntent<Subscription> SubscribeDocument(DocumentEvent kind, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            let owner = (Doc: doc, Objs: objs)
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeDocument)), detail: "null handler"))
            let events = kind.Handlers(doc: owner.Doc, objs: owner.Objs, handler: valid)
            from sub in BindMarshaled(
                attach: () => kind.Attach(owner, events),
                detach: () => kind.Detach(owner, events))
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeSolution(SolutionEvent kind, Func<SolutionEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeSolution)), detail: "null handler"))
            let events = kind.Handlers(server: doc.Solution, document: doc, objects: objs, handler: valid)
            from sub in BindMarshaled(
                attach: () => kind.Attach(doc.Solution, events),
                detach: () => kind.Detach(doc.Solution, events))
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeUndo(UndoEvent kind, Func<UndoEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeUndo)), detail: "null handler"))
            let events = kind.Handlers(history: doc.Undo, document: doc, handler: valid)
            from sub in BindMarshaled(
                attach: () => kind.Attach(doc.Undo, events),
                detach: () => kind.Detach(doc.Undo, events))
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeTimer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        GhUi.Read(run: scope =>
            from validInterval in Optional(interval).Filter(static value => value > TimeSpan.Zero)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "interval must be positive"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "null handler"))
            let scope2 = new TimerScope(interval: validInterval, tick: () => GrasshopperUi.Handler(valid: valid).Ignore())
            from sub in BindMarshaled(
                attach: scope2.Start,
                detach: scope2.Dispose,
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
            let events = kind.Handlers(source: validSource, handler: valid)
            from sub in BindMarshaled(
                attach: () => kind.Attach(validSource, events),
                detach: () => kind.Detach(validSource, events))
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeKeyboardModifiers(Func<InputModifierSnapshot, Fin<Unit>> handler) =>
        GhUi.Read(run: _ =>
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeKeyboardModifiers)), detail: "null handler"))
            let tick = (EventHandler<EventArgs>)((_, _) => GrasshopperUi.Handler(valid: () => valid(arg: Input.ModifierOf(keys: Keyboard.Modifiers))).Ignore())
            from sub in BindEtoChanged(
                attach: () => Keyboard.ModifiersChanged += tick,
                detach: () => Keyboard.ModifiersChanged -= tick,
                initialTick: () => GrasshopperUi.Handler(valid: () => valid(arg: Input.ModifierOf(keys: Keyboard.Modifiers))).Ignore())
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeLogicalPixelSize(Window window, Func<float, Fin<Unit>> handler) =>
        GhUi.Read(run: _ =>
            from validWindow in Optional(window).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeLogicalPixelSize)), detail: "null window"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeLogicalPixelSize)), detail: "null handler"))
            let tick = (EventHandler<EventArgs>)((_, _) => GrasshopperUi.Handler(valid: () => valid(arg: validWindow.LogicalPixelSize)).Ignore())
            from sub in BindEtoChanged(
                attach: () => validWindow.LogicalPixelSizeChanged += tick,
                detach: () => validWindow.LogicalPixelSizeChanged -= tick,
                initialTick: () => GrasshopperUi.Handler(valid: () => valid(arg: validWindow.LogicalPixelSize)).Ignore())
            select sub);

    internal static Fin<Subscription> BindMarshaled(System.Action attach, System.Action detach, bool detachOnce = false) =>
        Subscription.Bind(attach: attach, detach: detach, marshalToUi: true, detachOnce: detachOnce);

    private static Fin<Subscription> BindEtoChanged(System.Action attach, System.Action detach, System.Action initialTick) =>
        BindMarshaled(
            attach: () => {
                attach();
                initialTick();
            },
            detach: detach);
}
