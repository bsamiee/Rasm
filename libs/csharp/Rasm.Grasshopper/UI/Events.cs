using System.Globalization;
using System.Reflection;
using AppKit;
using Eto.Forms;
using Foundation;
using Grasshopper2;
using Grasshopper2.Doc;
using Grasshopper2.UI.Flex;
using Grasshopper2.Undo;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record UiEvent : IUiOp {
    private UiEvent() { }
    public sealed record PaintCase(CanvasPaintPhase Phase, Func<PaintScope, Fin<Unit>> Handler, MotionClock? Clock = null, RepaintRequest? Repaint = null) : UiEvent;
    public sealed record CanvasCase(CanvasEvent Kind, Func<CanvasEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record DocumentCase(DocumentEvent Kind, Func<DocumentEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record SolutionCase(SolutionEvent Kind, Func<SolutionEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record UndoCase(UndoEvent Kind, Func<UndoEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record TimerCase(TimeSpan Interval, Func<Fin<Unit>> Handler) : UiEvent;
    public sealed record ControlCase(Control Source, ControlEvent Kind, Func<ControlEventSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record KeyboardModifiersCase(Func<InputModifierSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record LogicalPixelSizeCase(Window Window, Func<float, Fin<Unit>> Handler) : UiEvent;
    public sealed record WindowLifecycleCase(Window Window, WindowLifecycle Kind, Func<bool, Fin<Unit>> Handler) : UiEvent;
    public sealed record NativeInputCase(Func<NativeInputSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record ManyCase(Seq<UiEvent> Events) : UiEvent;

    public static UiEvent Paint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler, MotionClock? clock = null, RepaintRequest? repaint = null) =>
        new PaintCase(Phase: phase, Handler: handler, Clock: clock, Repaint: repaint);
    public static UiEvent Paint(CanvasPaintPhase phase, DrawPlan plan, MotionClock? clock = null, RepaintRequest? repaint = null) =>
        new PaintCase(Phase: phase, Handler: plan.Apply, Clock: clock, Repaint: repaint);
    public static UiEvent Canvas(CanvasEvent kind, Func<CanvasEventSnapshot, Fin<Unit>> handler) =>
        new CanvasCase(Kind: kind, Handler: handler);
    public static UiEvent Document(DocumentEvent kind, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        new DocumentCase(Kind: kind, Handler: handler);
    public static UiEvent Document<TSnapshot>(DocumentEvent kind, Func<DocumentEventSnapshot, TSnapshot> project, Func<TSnapshot, Fin<Unit>> handler) =>
        new DocumentCase(
            Kind: kind,
            Handler: e => Optional(handler)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Document)), detail: "null handler"))
                .Bind(valid => valid(arg: project(arg: e))));
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
    public static UiEvent WindowLifecycle(Window window, WindowLifecycle kind, Func<bool, Fin<Unit>> handler) =>
        new WindowLifecycleCase(Window: window, Kind: kind, Handler: handler);
    public static UiEvent NativeInput(Func<NativeInputSnapshot, Fin<Unit>> handler) =>
        new NativeInputCase(Handler: handler);
    public static UiEvent Many(Seq<UiEvent> events) =>
        new ManyCase(Events: events);

    public GrasshopperUiPolicy UiPolicy => Switch(
        paintCase: static p => GrasshopperUiPolicy.Canvas(repaint: p.Repaint),
        canvasCase: static _ => GrasshopperUiPolicy.Canvas(),
        documentCase: static _ => GrasshopperUiPolicy.Document(),
        solutionCase: static _ => GrasshopperUiPolicy.Document(),
        undoCase: static _ => GrasshopperUiPolicy.Document(),
        timerCase: static _ => GrasshopperUiPolicy.Read,
        controlCase: static _ => GrasshopperUiPolicy.Read,
        keyboardModifiersCase: static _ => GrasshopperUiPolicy.Read,
        logicalPixelSizeCase: static _ => GrasshopperUiPolicy.Read,
        windowLifecycleCase: static _ => GrasshopperUiPolicy.Read,
        nativeInputCase: static _ => GrasshopperUiPolicy.Read,
        manyCase: static m => m.Events.Fold(GrasshopperUiPolicy.Read, static (policy, e) => policy | e.UiPolicy));

    internal static Unit Publish<TSnapshot>(Func<TSnapshot, Fin<Unit>> handler, TSnapshot snapshot) {
        _ = GrasshopperUi.Handler(valid: () => handler(arg: snapshot));
        return unit;
    }
}

internal readonly record struct EventSpec<TOwner, THandlers, TSnapshot>(
    TOwner Owner,
    THandlers Handlers,
    Action<TOwner, THandlers> Attach,
    Action<TOwner, THandlers> Detach,
    Func<TSnapshot, Fin<Unit>> Handler) {
    internal Fin<Subscription> Subscribe(bool marshalToUi = true) {
        TOwner owner = Owner;
        THandlers handlers = Handlers;
        Action<TOwner, THandlers> attach = Attach;
        Action<TOwner, THandlers> detach = Detach;
        return marshalToUi
            ? Events.BindMarshaled(attach: () => attach(owner, handlers), detach: () => detach(owner, handlers))
            : Subscription.Bind(attach: () => attach(owner, handlers), detach: () => detach(owner, handlers));
    }

    internal Unit Publish(TSnapshot snapshot) => UiEvent.Publish(handler: Handler, snapshot: snapshot);
}

[SmartEnum<int>]
public sealed partial class CanvasEvent {
    private delegate Fin<Subscription> SubscribeFn(GhCanvas canvas, Func<CanvasEventSnapshot, Fin<Unit>> handler);

    public static readonly CanvasEvent DocumentLoaded = new(
        key: 0,
        subscribe: static (canvas, handler) => {
            void Changed(object? _, EventArgs __) =>
                _ = UiEvent.Publish(handler: handler, snapshot: new CanvasEventSnapshot(Kind: DocumentLoaded!));
            return Subscription.Bind(
                attach: () => canvas.DocumentChanged += Changed,
                detach: () => canvas.DocumentChanged -= Changed,
                marshalToUi: true);
        });

    public static readonly CanvasEvent ProjectionChanged = new(
        key: 1,
        subscribe: static (canvas, handler) => {
            void Changed(object? _, ProjectionChangedEventArgs e) {
                CanvasProjectionDelta delta = new(
                    OldOrigin: e.OldProjection.Origin,
                    OldZoom: e.OldProjection.Zoom,
                    NewOrigin: e.NewProjection.Origin,
                    NewZoom: e.NewProjection.Zoom);
                _ = UiEvent.Publish(handler: handler, snapshot: new CanvasEventSnapshot(Kind: ProjectionChanged!, Projection: Some(delta)));
            }
            return Subscription.Bind(
                attach: () => canvas.ProjectionChanged += Changed,
                detach: () => canvas.ProjectionChanged -= Changed,
                marshalToUi: true);
        });

    public static readonly CanvasEvent WindowSelection = new(
        key: 2,
        subscribe: static (canvas, handler) => {
            void Selected(object? _, WindowSelectionEventArgs e) =>
                _ = UiEvent.Publish(handler: handler, snapshot: new CanvasEventSnapshot(
                    Kind: WindowSelection!,
                    Window: WindowDeltaOf(args: e)));
            return Subscription.Bind(
                attach: () => canvas.WindowSelection += Selected,
                detach: () => canvas.WindowSelection -= Selected,
                marshalToUi: true);
        });

    public static readonly CanvasEvent DocumentModified = new(
        key: 3,
        subscribe: static (canvas, handler) => {
            void Changed(object? _, EventArgs __) =>
                _ = UiEvent.Publish(handler: handler, snapshot: new CanvasEventSnapshot(Kind: DocumentModified!, Modified: canvas.Document?.Modified ?? false));
            return Subscription.Bind(
                attach: () => canvas.DocumentModified += Changed,
                detach: () => canvas.DocumentModified -= Changed,
                marshalToUi: true);
        });

    public static readonly CanvasEvent MouseHover = new(
        key: 4,
        subscribe: static (canvas, handler) => {
            void Hovered(object? _, MouseHoverEventArgs e) =>
                _ = UiEvent.Publish(handler: handler, snapshot: new CanvasEventSnapshot(Kind: MouseHover!, Hover: Some(e.ControlPoint)));
            return Subscription.Bind(
                attach: () => canvas.MouseHover += Hovered,
                detach: () => canvas.MouseHover -= Hovered,
                marshalToUi: true);
        });

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Subscribe(GhCanvas canvas, Func<CanvasEventSnapshot, Fin<Unit>> handler);

    private static Option<CanvasWindowDelta> WindowDeltaOf(WindowSelectionEventArgs args) =>
        Optional(args)
            .Bind(static e => InputSelectionSource.From(window: e).Mode().ToOption()
                .Map(mode => new CanvasWindowDelta(Window: e.Window, Mode: mode)));
}

[SmartEnum<int>]
public sealed partial class WindowLifecycle {
    private delegate Fin<Subscription> SubscribeFn(Window window, Func<bool, Fin<Unit>> handler);

    // [ASSUMED] Eto.Forms.Window lifecycle events (Closing/Closed/LocationChanged/WindowStateChanged); the published
    // bool is the live Window.Visible state at the moment the event fires.
    public static readonly WindowLifecycle Closing = new(
        key: 0,
        subscribe: static (window, handler) => {
            void On(object? _, System.ComponentModel.CancelEventArgs __) =>
                _ = UiEvent.Publish(handler: handler, snapshot: window.Visible);
            return Subscription.Bind(attach: () => window.Closing += On, detach: () => window.Closing -= On, marshalToUi: true);
        });
    public static readonly WindowLifecycle Closed = new(
        key: 1,
        subscribe: static (window, handler) => {
            void On(object? _, EventArgs __) =>
                _ = UiEvent.Publish(handler: handler, snapshot: window.Visible);
            return Subscription.Bind(attach: () => window.Closed += On, detach: () => window.Closed -= On, marshalToUi: true);
        });
    public static readonly WindowLifecycle LocationChanged = new(
        key: 2,
        subscribe: static (window, handler) => {
            void On(object? _, EventArgs __) =>
                _ = UiEvent.Publish(handler: handler, snapshot: window.Visible);
            return Subscription.Bind(attach: () => window.LocationChanged += On, detach: () => window.LocationChanged -= On, marshalToUi: true);
        });
    public static readonly WindowLifecycle WindowStateChanged = new(
        key: 3,
        subscribe: static (window, handler) => {
            void On(object? _, EventArgs __) =>
                _ = UiEvent.Publish(handler: handler, snapshot: window.Visible);
            return Subscription.Bind(attach: () => window.WindowStateChanged += On, detach: () => window.WindowStateChanged -= On, marshalToUi: true);
        });

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Subscribe(Window window, Func<bool, Fin<Unit>> handler);
}

internal readonly record struct DocumentEventHandlers(EventHandler<DocumentModifiedEventArgs> Modified, EventHandler<DocumentStateEventArgs> State, EventHandler<BeforeAfterEventArgs<GhDocument, IDocumentParent>> Parent, EventHandler<UndoEventArgs> Undo, EventHandler<AfterAddObjectEventArgs> Added, EventHandler<AfterRemoveObjectEventArgs> Removed, EventHandler<ObjectEventArgs> Expired, EventHandler<ObjectEventArgs> Selection, EventHandler<ObjectEventArgs> Enabled, EventHandler<ObjectEventArgs> Relevance, EventHandler<ObjectEventArgs> Layout, EventHandler<ObjectEventArgs> Display, EventHandler<ObjectNameEventArgs> Name, EventHandler<ObjectGuidEventArgs> Id) {
    internal static DocumentEventHandlers Combine(Seq<DocumentEventHandlers> handlers) =>
        handlers.Fold(
            initialState: Empty,
            f: static (left, right) => new DocumentEventHandlers(
                Modified: left.Modified + right.Modified,
                State: left.State + right.State,
                Parent: left.Parent + right.Parent,
                Undo: left.Undo + right.Undo,
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

public readonly record struct DocumentEvent {
    private readonly Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers>? attach;
    private readonly Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers>? detach;
    private readonly Func<DocumentEvent, DocumentEventPipe, DocumentEventHandlers>? build;

    public string Name { get; }

    internal DocumentEvent(string Name, Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers> Attach, Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers> Detach, Func<DocumentEvent, DocumentEventPipe, DocumentEventHandlers> Build) {
        this.Name = Name;
        attach = Attach;
        detach = Detach;
        build = Build;
    }

    internal DocumentEventHandlers Handlers(GhDocument doc, GhObjectList objs, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        build!(arg1: this, arg2: new DocumentEventPipe(Document: doc, Objects: objs, Handler: handler));

    internal Fin<Subscription> Subscribe((GhDocument Doc, GhObjectList Objs) owner, Func<DocumentEventSnapshot, Fin<Unit>> handler) {
        DocumentEvent self = this;
        Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers>? attachLocal = attach;
        Action<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers>? detachLocal = detach;
        Func<DocumentEvent, DocumentEventPipe, DocumentEventHandlers>? buildLocal = build;
        return from validAttach in Optional(attachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentEvent)), detail: "attach missing"))
               from validDetach in Optional(detachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentEvent)), detail: "detach missing"))
               from validBuild in Optional(buildLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DocumentEvent)), detail: "handler builder missing"))
               let spec = new EventSpec<(GhDocument Doc, GhObjectList Objs), DocumentEventHandlers, DocumentEventSnapshot>(
                   Owner: owner,
                   Handlers: validBuild(arg1: self, arg2: new DocumentEventPipe(Document: owner.Doc, Objects: owner.Objs, Handler: handler)),
                   Attach: validAttach,
                   Detach: validDetach,
                   Handler: handler)
               from sub in spec.Subscribe()
               select sub;
    }

    internal static DocumentEvent Composite(string name, params DocumentEvent[] members) =>
        new(
            Name: name,
            Attach: (owner, handlers) => toSeq(members).Iter(member => member.attach!(owner, handlers)),
            Detach: (owner, handlers) => toSeq(members).Iter(member => member.detach!(owner, handlers)),
            Build: (_, pipe) => DocumentEventHandlers.Combine(handlers: toSeq(members).Map(member => member.build!(arg1: member, arg2: pipe))));
}

public sealed record DocumentEventPipe(
    GhDocument Document,
    GhObjectList Objects,
    Func<DocumentEventSnapshot, Fin<Unit>> Handler) {
    internal Unit PublishSelection(DocumentEvent kind) =>
        Publish(
            kind: kind,
            objects: toSeq(Objects.SelectedObjects.Select(UiRail.DocumentObjectSnapshotOf)),
            wires: Wire.SnapshotConnectedBatch(objects: Objects, document: Document, wires: Objects.SelectedWires ?? []));

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
        _ = UiEvent.Publish(handler: Handler, snapshot: new DocumentEventSnapshot(
            Kind: kind,
            Document: UiRail.DocumentSnapshotOf(document: Document, objects: Objects),
            Objects: objects,
            Wires: wires,
            Detail: detail));
        return unit;
    }
}

internal readonly record struct SolutionEventHandlers(EventHandler<SolutionIdEventArgs> About, EventHandler<SolutionEventArgs> Plain, EventHandler<SolutionExceptionEventArgs> Faulted, EventHandler Enabled) {
    internal static SolutionEventHandlers Empty => default;
}

public readonly record struct SolutionEvent {
    private readonly Action<SolutionServer, SolutionEventHandlers>? attach;
    private readonly Action<SolutionServer, SolutionEventHandlers>? detach;
    private readonly Func<SolutionEvent, SolutionEventPipe, SolutionEventHandlers>? build;

    public string Name { get; }

    internal SolutionEvent(string Name, Action<SolutionServer, SolutionEventHandlers> Attach, Action<SolutionServer, SolutionEventHandlers> Detach, Func<SolutionEvent, SolutionEventPipe, SolutionEventHandlers> Build) {
        this.Name = Name;
        attach = Attach;
        detach = Detach;
        build = Build;
    }

    internal SolutionEventHandlers Handlers(SolutionServer server, GhDocument document, GhObjectList objects, Func<SolutionEventSnapshot, Fin<Unit>> handler) =>
        build!(arg1: this, arg2: new SolutionEventPipe(Server: server, Document: document, Objects: objects, Handler: handler));

    internal Fin<Subscription> Subscribe(SolutionServer server, GhDocument document, GhObjectList objects, Func<SolutionEventSnapshot, Fin<Unit>> handler) {
        SolutionEvent self = this;
        Action<SolutionServer, SolutionEventHandlers>? attachLocal = attach;
        Action<SolutionServer, SolutionEventHandlers>? detachLocal = detach;
        Func<SolutionEvent, SolutionEventPipe, SolutionEventHandlers>? buildLocal = build;
        return from validAttach in Optional(attachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SolutionEvent)), detail: "attach missing"))
               from validDetach in Optional(detachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SolutionEvent)), detail: "detach missing"))
               from validBuild in Optional(buildLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SolutionEvent)), detail: "handler builder missing"))
               let events = validBuild(arg1: self, arg2: new SolutionEventPipe(Server: server, Document: document, Objects: objects, Handler: handler))
               from sub in Events.BindMarshaled(
                   attach: () => validAttach(server, events),
                   detach: () => validDetach(server, events))
               select sub;
    }
}

public sealed record SolutionEventPipe(SolutionServer Server, GhDocument Document, GhObjectList Objects, Func<SolutionEventSnapshot, Fin<Unit>> Handler) {
    internal Unit Publish(SolutionEvent kind, Option<string> solutionKey = default, Option<string> faultMessage = default, Option<string> faultType = default) {
        _ = UiEvent.Publish(handler: Handler, snapshot: new SolutionEventSnapshot(
            Kind: kind,
            Document: UiRail.DocumentSnapshotOf(document: Document, objects: Objects),
            SolutionKey: solutionKey,
            FaultMessage: faultMessage,
            FaultType: faultType));
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

internal readonly record struct UndoEventHandlers(EventHandler<UndoEventArgs> Plain, EventHandler<UndoNodeEventArgs> Node, EventHandler<UndoNodeMovedEventArgs> Moved) {
    internal static UndoEventHandlers Empty => default;
}

public readonly record struct UndoEvent {
    private readonly Action<History, UndoEventHandlers>? attach;
    private readonly Action<History, UndoEventHandlers>? detach;
    private readonly Func<UndoEvent, UndoEventPipe, UndoEventHandlers>? build;

    public string Name { get; }

    internal UndoEvent(string Name, Action<History, UndoEventHandlers> Attach, Action<History, UndoEventHandlers> Detach, Func<UndoEvent, UndoEventPipe, UndoEventHandlers> Build) {
        this.Name = Name;
        attach = Attach;
        detach = Detach;
        build = Build;
    }

    internal UndoEventHandlers Handlers(History history, GhDocument document, Func<UndoEventSnapshot, Fin<Unit>> handler) =>
        build!(arg1: this, arg2: new UndoEventPipe(History: history, Document: document, Handler: handler));

    internal Fin<Subscription> Subscribe(History history, GhDocument document, Func<UndoEventSnapshot, Fin<Unit>> handler) {
        UndoEvent self = this;
        Action<History, UndoEventHandlers>? attachLocal = attach;
        Action<History, UndoEventHandlers>? detachLocal = detach;
        Func<UndoEvent, UndoEventPipe, UndoEventHandlers>? buildLocal = build;
        return from validAttach in Optional(attachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(UndoEvent)), detail: "attach missing"))
               from validDetach in Optional(detachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(UndoEvent)), detail: "detach missing"))
               from validBuild in Optional(buildLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(UndoEvent)), detail: "handler builder missing"))
               let events = validBuild(arg1: self, arg2: new UndoEventPipe(History: history, Document: document, Handler: handler))
               from sub in Events.BindMarshaled(
                   attach: () => validAttach(history, events),
                   detach: () => validDetach(history, events))
               select sub;
    }
}

public sealed record UndoEventPipe(History History, GhDocument Document, Func<UndoEventSnapshot, Fin<Unit>> Handler) {
    internal Unit Publish(UndoEvent kind, Option<string> nodeName = default, Option<int> nodeCount = default) {
        _ = UiEvent.Publish(handler: Handler, snapshot: new UndoEventSnapshot(
            Kind: kind,
            History: UiRail.HistorySnapshotOf(document: Document),
            NodeName: nodeName,
            NodeCount: nodeCount));
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

internal readonly record struct ControlEventHandlers(EventHandler<EventArgs> Plain, EventHandler<KeyEventArgs> Key, EventHandler<TextInputEventArgs> Text, EventHandler<MouseEventArgs> Mouse, EventHandler<DragEventArgs> Drag) {
    internal static ControlEventHandlers Empty => default;
}

public readonly record struct ControlEvent {
    private readonly Action<Control, ControlEventHandlers>? attach;
    private readonly Action<Control, ControlEventHandlers>? detach;
    private readonly Func<ControlEvent, ControlEventPipe, ControlEventHandlers>? build;

    public string Name { get; }

    internal ControlEvent(string Name, Action<Control, ControlEventHandlers> Attach, Action<Control, ControlEventHandlers> Detach, Func<ControlEvent, ControlEventPipe, ControlEventHandlers> Build) {
        this.Name = Name;
        attach = Attach;
        detach = Detach;
        build = Build;
    }

    internal ControlEventHandlers Handlers(Control source, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        build!(arg1: this, arg2: new ControlEventPipe(Source: source, Handler: handler));

    internal Fin<Subscription> Subscribe(Control source, Func<ControlEventSnapshot, Fin<Unit>> handler) {
        ControlEvent self = this;
        Action<Control, ControlEventHandlers>? attachLocal = attach;
        Action<Control, ControlEventHandlers>? detachLocal = detach;
        Func<ControlEvent, ControlEventPipe, ControlEventHandlers>? buildLocal = build;
        return from validAttach in Optional(attachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ControlEvent)), detail: "attach missing"))
               from validDetach in Optional(detachLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ControlEvent)), detail: "detach missing"))
               from validBuild in Optional(buildLocal).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ControlEvent)), detail: "handler builder missing"))
               let spec = new EventSpec<Control, ControlEventHandlers, ControlEventSnapshot>(
                   Owner: source,
                   Handlers: validBuild(arg1: self, arg2: new ControlEventPipe(Source: source, Handler: handler)),
                   Attach: validAttach,
                   Detach: validDetach,
                   Handler: handler)
               from sub in spec.Subscribe()
               select sub;
    }
}

public sealed record ControlEventPipe(Control Source, Func<ControlEventSnapshot, Fin<Unit>> Handler) {
    internal Unit Publish(ControlEventSnapshot snapshot) {
        _ = UiEvent.Publish(handler: Handler, snapshot: snapshot);
        return unit;
    }
}

public static class DocumentEventKind {
    public static readonly DocumentEvent Modified = OnDoc(
        name: nameof(Modified),
        attach: static (d, h) => d.ModifiedChanged += h,
        detach: static (d, h) => d.ModifiedChanged -= h,
        select: static h => h.Modified,
        assign: static (left, right) => left with { Modified = right },
        detail: static e => Some($"{e.Oldstate}->{e.NewState}"));
    public static readonly DocumentEvent StateChanged = OnDoc(
        name: nameof(StateChanged),
        attach: static (d, h) => d.StateChanged += h,
        detach: static (d, h) => d.StateChanged -= h,
        select: static h => h.State,
        assign: static (left, right) => left with { State = right },
        detail: static e => Some($"{e.Oldstate}->{e.NewState}"));
    public static readonly DocumentEvent ParentChanged = OnDoc(
        name: nameof(ParentChanged),
        attach: static (d, h) => d.ParentChanged += h,
        detach: static (d, h) => d.ParentChanged -= h,
        select: static h => h.Parent,
        assign: static (left, right) => left with { Parent = right },
        detail: static e => Some($"{e.Old?.GetType().Name}->{e.New?.GetType().Name}"));
    public static readonly DocumentEvent UndoTopologyChanged = OnDoc(
        name: nameof(UndoTopologyChanged),
        attach: static (d, h) => {
            d.Undo.Modified += h;
            d.Undo.Undone += h;
            d.Undo.Redone += h;
        },
        detach: static (d, h) => {
            d.Undo.Modified -= h;
            d.Undo.Undone -= h;
            d.Undo.Redone -= h;
        },
        select: static h => h.Undo,
        assign: static (left, right) => left with { Undo = right },
        detail: static e => Optional(e.History?.GetType().Name));
    public static readonly DocumentEvent ObjectAdded = OnObject(nameof(ObjectAdded), static (o, h) => o.ObjectAdded += h, static (o, h) => o.ObjectAdded -= h, static h => h.Added, static (h, next) => h with { Added = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectRemoved = OnObject(nameof(ObjectRemoved), static (o, h) => o.ObjectRemoved += h, static (o, h) => o.ObjectRemoved -= h, static h => h.Removed, static (h, next) => h with { Removed = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectExpired = OnObject(nameof(ObjectExpired), static (o, h) => o.ObjectExpired += h, static (o, h) => o.ObjectExpired -= h, static h => h.Expired, static (h, next) => h with { Expired = next }, static e => e.Object);
    public static readonly DocumentEvent ObjectName = new(
        Name: nameof(ObjectName),
        Attach: static (owner, handlers) => owner.Objs.ObjectNameChanged += handlers.Name,
        Detach: static (owner, handlers) => owner.Objs.ObjectNameChanged -= handlers.Name,
        Build: static (kind, pipe) => DocumentEventHandlers.Empty with {
            Name = (_, e) => pipe.PublishObject(kind: kind, changed: Optional(e.Owner), detail: Some($"{e.Old}->{e.New}")),
        });
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
    public static readonly DocumentEvent ObjectInstanceId = new(
        Name: nameof(ObjectInstanceId),
        Attach: static (owner, handlers) => owner.Objs.ObjectInstanceIdChanged += handlers.Id,
        Detach: static (owner, handlers) => owner.Objs.ObjectInstanceIdChanged -= handlers.Id,
        Build: static (kind, pipe) => DocumentEventHandlers.Empty with {
            Id = (_, e) => pipe.PublishObject(kind: kind, changed: Optional(e.Object), detail: Some($"{e.OldId}->{e.NewId}")),
        });
    // UndoTopologyChanged is excluded: undo/redo already re-fires Modified, so including it double-publishes on every undo step.
    public static readonly DocumentEvent AnyChanged = DocumentEvent.Composite(
        name: nameof(AnyChanged),
        Modified, StateChanged, ObjectAdded, ObjectRemoved, ObjectExpired, ObjectName, Selection,
        ObjectEnabled, ObjectRelevance, ObjectLayout, ObjectDisplay, ObjectInstanceId, ParentChanged);

    private static DocumentEvent OnDoc<TArgs>(
        string name,
        Action<GhDocument, EventHandler<TArgs>> attach,
        Action<GhDocument, EventHandler<TArgs>> detach,
        Func<DocumentEventHandlers, EventHandler<TArgs>> select,
        Func<DocumentEventHandlers, EventHandler<TArgs>, DocumentEventHandlers> assign,
        Func<TArgs, Option<string>> detail) where TArgs : EventArgs =>
        new(
            Name: name,
            Attach: (owner, handlers) => attach(arg1: owner.Doc, arg2: select(arg: handlers)),
            Detach: (owner, handlers) => detach(arg1: owner.Doc, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(
                arg1: DocumentEventHandlers.Empty,
                arg2: (_, e) => pipe.PublishDetail(kind: kind, detail: detail(arg: e))));
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

    // EnabledChanged is a STATIC plain EventHandler on SolutionServer (sender/args always null) tracking the
    // global SolutionServer.EnableSolutions toggle. It cannot reuse On<TArgs> (which requires
    // EventHandler<TArgs>), so it binds the dedicated Enabled handler slot to the static event.
    public static readonly SolutionEvent EnabledChanged = new(
        Name: nameof(EnabledChanged),
        Attach: static (_, handlers) => SolutionServer.EnabledChanged += handlers.Enabled,
        Detach: static (_, handlers) => SolutionServer.EnabledChanged -= handlers.Enabled,
        Build: static (kind, pipe) => SolutionEventHandlers.Empty with { Enabled = (_, _) => pipe.PublishPlain(kind: kind) });

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
        ControlEvent kind,
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
            Kind: kind,
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
            snapshot: static (kind, pipe, _) => Snapshot(kind: kind, source: pipe.Source));

    private static ControlEvent OnKeyed(string name, Action<Control, EventHandler<KeyEventArgs>> attach, Action<Control, EventHandler<KeyEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Key,
            assign: static (left, right) => left with { Key = right },
            snapshot: static (kind, pipe, e) => Snapshot(kind: kind, source: pipe.Source, keys: Some(e.KeyData)));

    private static ControlEvent OnTextual(string name, Action<Control, EventHandler<TextInputEventArgs>> attach, Action<Control, EventHandler<TextInputEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Text,
            assign: static (left, right) => left with { Text = right },
            snapshot: static (kind, pipe, e) => Snapshot(kind: kind, source: pipe.Source, text: Optional(e.Text)));

    private static ControlEvent OnMoused(string name, Action<Control, EventHandler<MouseEventArgs>> attach, Action<Control, EventHandler<MouseEventArgs>> detach) =>
        On(
            name: name,
            attach: attach,
            detach: detach,
            select: static e => e.Mouse,
            assign: static (left, right) => left with { Mouse = right },
            snapshot: static (kind, pipe, e) => Snapshot(
                kind: kind,
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
            snapshot: static (kind, pipe, e) => Snapshot(
                kind: kind,
                source: pipe.Source,
                point: Some(e.Location),
                buttons: Some(e.Buttons), // [ASSUMED] Eto DragEventArgs.Buttons — mirrors MouseEventArgs.Buttons, unverified by decompile
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
        Func<ControlEvent, ControlEventPipe, TArgs, ControlEventSnapshot> snapshot) where TArgs : EventArgs =>
        new(
            Name: name,
            Attach: (source, handlers) => attach(arg1: source, arg2: select(arg: handlers)),
            Detach: (source, handlers) => detach(arg1: source, arg2: select(arg: handlers)),
            Build: (kind, pipe) => assign(
                arg1: ControlEventHandlers.Empty,
                arg2: (_, e) => pipe.Publish(snapshot: snapshot(arg1: kind, arg2: pipe, arg3: e))));
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

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasProjectionDelta(PointF OldOrigin, float OldZoom, PointF NewOrigin, float NewZoom);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasWindowDelta(WindowSelection Window, SelectionMode Mode);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasEventSnapshot(CanvasEvent Kind, Option<CanvasProjectionDelta> Projection = default, Option<CanvasWindowDelta> Window = default, bool Modified = false, Option<PointF> Hover = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct NativeInputSnapshot(
    string Kind,
    string Phase,
    bool Momentum,
    bool PreciseScrolling,
    InputModifierSnapshot Modifiers,
    Option<double> DeltaX = default,
    Option<double> DeltaY = default,
    Option<double> Magnification = default,
    Option<double> Rotation = default,
    Option<double> Pressure = default,
    Option<ulong> KeyCode = default,
    Option<ulong> RawModifiers = default) {
    private const ulong ShiftMask = 1UL << 17;
    private const ulong OptionMask = 1UL << 19;
    private const ulong CommandMask = 1UL << 20;

    internal static NativeInputSnapshot Of(NSEvent e) {
        Option<ulong> modifiers = UInt(eventArgs: e, name: "ModifierFlags");
        return new(
            Kind: e.Type.ToString(),
            Phase: Text(eventArgs: e, name: "Phase").IfNone(string.Empty),
            Momentum: Text(eventArgs: e, name: "MomentumPhase").Filter(static value => !string.Equals(a: value, b: "None", comparisonType: StringComparison.OrdinalIgnoreCase)).IsSome,
            PreciseScrolling: Bool(eventArgs: e, name: "HasPreciseScrollingDeltas").IfNone(noneValue: false),
            Modifiers: new InputModifierSnapshot(
                Shift: modifiers.Map(static flags => (flags & ShiftMask) != 0UL).IfNone(noneValue: false),
                Command: modifiers.Map(static flags => (flags & CommandMask) != 0UL).IfNone(noneValue: false),
                Option: modifiers.Map(static flags => (flags & OptionMask) != 0UL).IfNone(noneValue: false)),
            DeltaX: Number(eventArgs: e, name: "ScrollingDeltaX"),
            DeltaY: Number(eventArgs: e, name: "ScrollingDeltaY"),
            Magnification: Number(eventArgs: e, name: "Magnification"),
            Rotation: Number(eventArgs: e, name: "Rotation"),
            Pressure: Number(eventArgs: e, name: "Pressure"),
            KeyCode: UInt(eventArgs: e, name: "KeyCode"),
            RawModifiers: modifiers);
    }

    private static Option<object> Value(NSEvent eventArgs, string name) =>
        Optional(eventArgs.GetType().GetProperty(name: name, bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: eventArgs));
    private static Option<string> Text(NSEvent eventArgs, string name) => Value(eventArgs: eventArgs, name: name).Map(static value => value.ToString() ?? string.Empty);
    private static Option<bool> Bool(NSEvent eventArgs, string name) => Value(eventArgs: eventArgs, name: name).Bind(static value => value is bool b ? Some(b) : Option<bool>.None);
    private static Option<double> Number(NSEvent eventArgs, string name) => Value(eventArgs: eventArgs, name: name).Bind(static value => value switch {
        double d => Some(d),
        float f => Some((double)f),
        _ => Option<double>.None,
    });
    private static Option<ulong> UInt(NSEvent eventArgs, string name) => Value(eventArgs: eventArgs, name: name).Bind(static value => value switch {
        ulong raw => Some(raw),
        nuint raw => Some((ulong)raw),
        uint raw => Some((ulong)raw),
        long raw when raw >= 0L => Some((ulong)raw),
        int raw when raw >= 0 => Some((ulong)raw),
        Enum raw => Some(Convert.ToUInt64(value: raw, provider: CultureInfo.InvariantCulture)),
        _ => Option<ulong>.None,
    });
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Events {
    internal static GrasshopperUiIntent<Subscription> Subscribe(UiEvent uiEvent) =>
        uiEvent.Switch(
            paintCase: static p => SubscribePaint(phase: p.Phase, handler: p.Handler, clock: p.Clock),
            canvasCase: static c => SubscribeCanvas(kind: c.Kind, handler: c.Handler),
            documentCase: static d => SubscribeDocument(kind: d.Kind, handler: d.Handler),
            solutionCase: static s => SubscribeSolution(kind: s.Kind, handler: s.Handler),
            undoCase: static u => SubscribeUndo(kind: u.Kind, handler: u.Handler),
            timerCase: static t => SubscribeTimer(interval: t.Interval, handler: t.Handler),
            controlCase: static c => SubscribeControl(source: c.Source, kind: c.Kind, handler: c.Handler),
            keyboardModifiersCase: static k => SubscribeKeyboardModifiers(handler: k.Handler),
            logicalPixelSizeCase: static l => SubscribeLogicalPixelSize(window: l.Window, handler: l.Handler),
            windowLifecycleCase: static w => SubscribeWindowLifecycle(window: w.Window, kind: w.Kind, handler: w.Handler),
            nativeInputCase: static n => SubscribeNativeInput(handler: n.Handler),
            manyCase: static m => SubscribeMany(events: m.Events));

    private static GrasshopperUiIntent<Subscription> SubscribePaint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler, MotionClock? clock) =>
        Paint.Hook(phase: phase, paint: handler, clock: clock ?? MotionClock.MessageLoop);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static GrasshopperUiIntent<Subscription> SubscribeCanvas(CanvasEvent kind, Func<CanvasEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validKind in Optional(kind).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeCanvas)), detail: "null canvas event"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeCanvas)), detail: "null handler"))
            from sub in validKind.Subscribe(canvas: canvas, handler: valid)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeDocument(DocumentEvent kind, Func<DocumentEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            let owner = (Doc: doc, Objs: objs)
            from validKind in Optional(kind).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeDocument)), detail: "null document event"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeDocument)), detail: "null handler"))
            from sub in validKind.Subscribe(owner: owner, handler: valid)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeSolution(SolutionEvent kind, Func<SolutionEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            from validKind in Optional(kind).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeSolution)), detail: "null solution event"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeSolution)), detail: "null handler"))
            from sub in validKind.Subscribe(server: doc.Solution, document: doc, objects: objs, handler: valid)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeUndo(UndoEvent kind, Func<UndoEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Document(run: scope =>
            from doc in scope.NeedDocument()
            from validKind in Optional(kind).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeUndo)), detail: "null undo event"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeUndo)), detail: "null handler"))
            from sub in validKind.Subscribe(history: doc.Undo, document: doc, handler: valid)
            select sub);

    // UITimer.Interval is seconds (sub-ms quantum); intervals below the Eto loop tick coalesce. Lifecycle
    // transfers to the returned Subscription — the detachOnce-guarded detach stops/unsubscribes/disposes the
    // timer exactly once, so Subscription.GuardDetach owns run-once (no per-scope Interlocked gate needed).
    private static GrasshopperUiIntent<Subscription> SubscribeTimer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        GhUi.Read(run: scope =>
            from validInterval in Optional(interval).Filter(static value => value > TimeSpan.Zero)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "interval must be positive"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "null handler"))
            let timer = new UITimer { Interval = validInterval.TotalSeconds }
            let tick = (EventHandler<EventArgs>)((_, _) => GrasshopperUi.Handler(valid: valid).Ignore())
            from sub in BindMarshaled(
                attach: () => { timer.Elapsed += tick; timer.Start(); },
                detach: () => { timer.Stop(); timer.Elapsed -= tick; timer.Dispose(); },
                detachOnce: true)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeControl(Control source, ControlEvent kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Read(run: scope =>
            from validSource in Optional(source).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null control"))
            from validKind in Optional(kind).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null control event"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null handler"))
            from sub in validKind.Subscribe(source: validSource, handler: valid)
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

    private static GrasshopperUiIntent<Subscription> SubscribeWindowLifecycle(Window window, WindowLifecycle kind, Func<bool, Fin<Unit>> handler) =>
        GhUi.Read(run: _ =>
            from validWindow in Optional(window).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeWindowLifecycle)), detail: "null window"))
            from validKind in Optional(kind).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeWindowLifecycle)), detail: "null window lifecycle event"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeWindowLifecycle)), detail: "null handler"))
            from sub in validKind.Subscribe(window: validWindow, handler: valid)
            select sub);

    private static GrasshopperUiIntent<Subscription> SubscribeNativeInput(Func<NativeInputSnapshot, Fin<Unit>> handler) {
        NSObject? monitor = null;
        return GhUi.Read(run: _scope =>
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeNativeInput)), detail: "null handler"))
            from sub in BindMarshaled(
                attach: () => monitor = NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.AnyEvent, e =>
                    Optional(e).Map(evt => UiEvent.Publish(handler: valid, snapshot: NativeInputSnapshot.Of(e: evt))).Map(_ => e).IfNone(e)),
                detach: () => Optional(monitor).Iter(NSEvent.RemoveMonitor),
                detachOnce: true)
            select sub);
    }

    private static GrasshopperUiIntent<Subscription> SubscribeMany(Seq<UiEvent> events) =>
        new(
            run: scope => events.TraverseM(e => Subscribe(uiEvent: e).Run(scope: scope)).As()
                .Map(static subs => subs.Fold(Subscription.Empty, static (composite, sub) => composite | sub)),
            policy: events.Fold(GrasshopperUiPolicy.Read, static (policy, e) => policy | e.UiPolicy));

    internal static Fin<Subscription> BindMarshaled(System.Action attach, System.Action detach, bool detachOnce = false) =>
        Subscription.Bind(attach: attach, detach: detach, marshalToUi: true, detachOnce: detachOnce);

    // Fire-on-subscribe asymmetry: unlike the Document/Solution/Undo subscribers (plain BindMarshaled, no
    // initial publish), the Eto state events (modifiers, logical pixel size) push the current value once at
    // attach so the handler observes the live state immediately rather than only on the next change.
    private static Fin<Subscription> BindEtoChanged(System.Action attach, System.Action detach, System.Action initialTick) =>
        BindMarshaled(
            attach: () => {
                attach();
                initialTick();
            },
            detach: detach);
}
