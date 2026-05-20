using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public partial record UiEvent {
    private UiEvent() { }
    public sealed record PaintCase(CanvasPaintPhase Phase, Func<PaintScope, Fin<Unit>> Handler) : UiEvent;
    public sealed record DocumentChangedCase(Func<DocumentSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record SelectionChangedCase(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> Handler) : UiEvent;
    public sealed record TimerCase(TimeSpan Interval, Func<Fin<Unit>> Handler) : UiEvent;
    public sealed record ControlCase(Control Source, ControlEventKind Kind, Func<ControlEventSnapshot, Fin<Unit>> Handler) : UiEvent;

    public static UiEvent Paint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler) =>
        new PaintCase(Phase: phase, Handler: handler);
    public static UiEvent DocumentChanged(Func<DocumentSnapshot, Fin<Unit>> handler) =>
        new DocumentChangedCase(Handler: handler);
    public static UiEvent SelectionChanged(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) =>
        new SelectionChangedCase(Handler: handler);
    public static UiEvent Timer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        new TimerCase(Interval: interval, Handler: handler);
    public static UiEvent Control(Control source, ControlEventKind kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        new ControlCase(Source: source, Kind: kind, Handler: handler);
}

public enum ControlEventKind {
    PreLoad,
    Load,
    LoadComplete,
    UnLoad,
    Shown,
    GotFocus,
    LostFocus,
    SizeChanged,
    EnabledChanged,
    KeyDown,
    KeyUp,
    TextInput,
    MouseDown,
    MouseUp,
    MouseMove,
    MouseEnter,
    MouseLeave,
    MouseDoubleClick,
    MouseWheel,
    DragDrop,
    DragOver,
    DragEnter,
    DragLeave,
    DragEnd,
}

// --- [MODELS] -----------------------------------------------------------------------------
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
    Option<float> Pressure = default);

internal sealed record EventRequest(UiEvent Event) : GhUiRequest<IDisposable> {
    internal override GrasshopperUiPolicy Policy => Events.PolicyOf(uiEvent: Event);
    internal override Fin<IDisposable> Apply(GrasshopperUi.Scope scope) => Events.Subscribe(uiEvent: Event).Run(scope: scope);
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Events {
    internal static GrasshopperUiPolicy PolicyOf(UiEvent uiEvent) =>
        uiEvent switch {
            UiEvent.PaintCase => GrasshopperUiPolicy.Canvas(),
            UiEvent.DocumentChangedCase or UiEvent.SelectionChangedCase => GrasshopperUiPolicy.Document(),
            UiEvent.TimerCase or UiEvent.ControlCase => GrasshopperUiPolicy.Read,
            _ => GrasshopperUiPolicy.Read,
        };

    internal static GrasshopperUiIntent<IDisposable> Subscribe(UiEvent uiEvent) =>
        uiEvent switch {
            UiEvent.PaintCase p => Paint.Hook(phase: p.Phase, paint: p.Handler),
            UiEvent.DocumentChangedCase d => SubscribeDocumentChange(handler: d.Handler),
            UiEvent.SelectionChangedCase s => SubscribeSelectionChange(handler: s.Handler),
            UiEvent.TimerCase t => SubscribeTimer(interval: t.Interval, handler: t.Handler),
            UiEvent.ControlCase c => SubscribeControl(source: c.Source, kind: c.Kind, handler: c.Handler),
            _ => GhUi.Document<IDisposable>(run: _ => Fin.Fail<IDisposable>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Subscribe)), detail: $"event kind not supported: {uiEvent.GetType().Name}"))),
        };

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private static GrasshopperUiIntent<IDisposable> SubscribeDocumentChange(Func<DocumentSnapshot, Fin<Unit>> handler) =>
        GhUi.Document<IDisposable>(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeDocumentChange)), detail: "null handler"))
            select (IDisposable)DocumentChangeWatcher.Attach(document: doc, objects: objs, handler: valid));

    private static GrasshopperUiIntent<IDisposable> SubscribeSelectionChange(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) =>
        GhUi.Document<IDisposable>(run: scope =>
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeSelectionChange)), detail: "null handler"))
            select (IDisposable)SelectionChangeWatcher.Attach(objects: objs, handler: valid));

    private static GrasshopperUiIntent<IDisposable> SubscribeTimer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        GhUi.Read<IDisposable>(run: _ =>
            from validInterval in Optional(interval).Filter(static value => value > TimeSpan.Zero)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "interval must be positive"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeTimer)), detail: "null handler"))
            select (IDisposable)TimerWatcher.Attach(interval: validInterval, handler: valid));

    private static GrasshopperUiIntent<IDisposable> SubscribeControl(Control source, ControlEventKind kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
        GhUi.Read<IDisposable>(run: _ =>
            from validSource in Optional(source).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null control"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null handler"))
            select (IDisposable)ControlEventWatcher.Attach(source: validSource, kind: kind, handler: valid));

    private sealed class DocumentChangeWatcher : IDisposable {
        private readonly GhDocument document;
        private readonly GhObjectList objects;
        private readonly Func<DocumentSnapshot, Fin<Unit>> handler;
        private readonly EventHandler<DocumentModifiedEventArgs> modified;
        private readonly EventHandler<DocumentStateEventArgs> state;
        private readonly EventHandler<AfterAddObjectEventArgs> added;
        private readonly EventHandler<AfterRemoveObjectEventArgs> removed;
        private readonly EventHandler<ObjectEventArgs> objectChanged;
        private DocumentChangeWatcher(GhDocument document, GhObjectList objects, Func<DocumentSnapshot, Fin<Unit>> handler) {
            this.document = document;
            this.objects = objects;
            this.handler = handler;
            modified = (_, _) => Publish();
            state = (_, _) => Publish();
            added = (_, _) => Publish();
            removed = (_, _) => Publish();
            objectChanged = (_, _) => Publish();
            document.ModifiedChanged += modified;
            document.StateChanged += state;
            objects.ObjectAdded += added;
            objects.ObjectRemoved += removed;
            objects.ObjectExpired += objectChanged;
            objects.ObjectEnabledChanged += objectChanged;
            objects.ObjectLayoutChanged += objectChanged;
            objects.ObjectDisplayChanged += objectChanged;
        }
        internal static DocumentChangeWatcher Attach(GhDocument document, GhObjectList objects, Func<DocumentSnapshot, Fin<Unit>> handler) =>
            new(document: document, objects: objects, handler: handler);
        private Unit Publish() {
            _ = GrasshopperUi.Protect(valid: () => handler(arg: UiRail.DocumentSnapshotOf(document: document, objects: objects)));
            return unit;
        }
        public void Dispose() {
            document.ModifiedChanged -= modified;
            document.StateChanged -= state;
            objects.ObjectAdded -= added;
            objects.ObjectRemoved -= removed;
            objects.ObjectExpired -= objectChanged;
            objects.ObjectEnabledChanged -= objectChanged;
            objects.ObjectLayoutChanged -= objectChanged;
            objects.ObjectDisplayChanged -= objectChanged;
        }
    }

    private sealed class SelectionChangeWatcher : IDisposable {
        private readonly GhObjectList objects;
        private readonly Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler;
        private readonly EventHandler<ObjectEventArgs> selectedChanged;
        private SelectionChangeWatcher(GhObjectList objects, Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) {
            this.objects = objects;
            this.handler = handler;
            selectedChanged = (_, _) => Publish();
            objects.ObjectSelectionChanged += selectedChanged;
        }
        internal static SelectionChangeWatcher Attach(GhObjectList objects, Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) =>
            new(objects: objects, handler: handler);
        private Unit Publish() {
            Seq<DocumentObjectSnapshot> selected = toSeq(objects.SelectedObjects.Select(UiRail.DocumentObjectSnapshotOf));
            _ = GrasshopperUi.Protect(valid: () => handler(arg: selected));
            return unit;
        }
        public void Dispose() =>
            objects.ObjectSelectionChanged -= selectedChanged;
    }

    private sealed class TimerWatcher : IDisposable {
        private readonly UITimer timer;
        private readonly EventHandler<EventArgs> elapsed;
        private TimerWatcher(TimeSpan interval, Func<Fin<Unit>> handler) {
            timer = new UITimer { Interval = interval.TotalSeconds };
            elapsed = (_, _) => _ = GrasshopperUi.Protect(valid: handler);
            timer.Elapsed += elapsed;
            timer.Start();
        }
        internal static TimerWatcher Attach(TimeSpan interval, Func<Fin<Unit>> handler) =>
            new(interval: interval, handler: handler);
        public void Dispose() {
            timer.Elapsed -= elapsed;
            timer.Stop();
            timer.Dispose();
        }
    }

    private sealed class ControlEventWatcher : IDisposable {
        private readonly Control source;
        private readonly ControlEventKind kind;
        private readonly Func<ControlEventSnapshot, Fin<Unit>> publish;
        private readonly EventHandler<EventArgs> plain;
        private readonly EventHandler<KeyEventArgs> key;
        private readonly EventHandler<TextInputEventArgs> text;
        private readonly EventHandler<MouseEventArgs> mouse;
        private readonly EventHandler<DragEventArgs> drag;
        private readonly record struct ControlEventCase(
            ControlEventKind Kind,
            Func<ControlEventWatcher, Unit> Attach,
            Func<ControlEventWatcher, Unit> Detach);
        private static readonly Seq<ControlEventCase> EventCases = Seq(
            PlainCase(kind: ControlEventKind.PreLoad, attach: static (s, h) => s.PreLoad += h, detach: static (s, h) => s.PreLoad -= h),
            PlainCase(kind: ControlEventKind.Load, attach: static (s, h) => s.Load += h, detach: static (s, h) => s.Load -= h),
            PlainCase(kind: ControlEventKind.LoadComplete, attach: static (s, h) => s.LoadComplete += h, detach: static (s, h) => s.LoadComplete -= h),
            PlainCase(kind: ControlEventKind.UnLoad, attach: static (s, h) => s.UnLoad += h, detach: static (s, h) => s.UnLoad -= h),
            PlainCase(kind: ControlEventKind.Shown, attach: static (s, h) => s.Shown += h, detach: static (s, h) => s.Shown -= h),
            PlainCase(kind: ControlEventKind.GotFocus, attach: static (s, h) => s.GotFocus += h, detach: static (s, h) => s.GotFocus -= h),
            PlainCase(kind: ControlEventKind.LostFocus, attach: static (s, h) => s.LostFocus += h, detach: static (s, h) => s.LostFocus -= h),
            PlainCase(kind: ControlEventKind.SizeChanged, attach: static (s, h) => s.SizeChanged += h, detach: static (s, h) => s.SizeChanged -= h),
            PlainCase(kind: ControlEventKind.EnabledChanged, attach: static (s, h) => s.EnabledChanged += h, detach: static (s, h) => s.EnabledChanged -= h),
            KeyCase(kind: ControlEventKind.KeyDown, attach: static (s, h) => s.KeyDown += h, detach: static (s, h) => s.KeyDown -= h),
            KeyCase(kind: ControlEventKind.KeyUp, attach: static (s, h) => s.KeyUp += h, detach: static (s, h) => s.KeyUp -= h),
            TextCase(kind: ControlEventKind.TextInput, attach: static (s, h) => s.TextInput += h, detach: static (s, h) => s.TextInput -= h),
            MouseCase(kind: ControlEventKind.MouseDown, attach: static (s, h) => s.MouseDown += h, detach: static (s, h) => s.MouseDown -= h),
            MouseCase(kind: ControlEventKind.MouseUp, attach: static (s, h) => s.MouseUp += h, detach: static (s, h) => s.MouseUp -= h),
            MouseCase(kind: ControlEventKind.MouseMove, attach: static (s, h) => s.MouseMove += h, detach: static (s, h) => s.MouseMove -= h),
            MouseCase(kind: ControlEventKind.MouseEnter, attach: static (s, h) => s.MouseEnter += h, detach: static (s, h) => s.MouseEnter -= h),
            MouseCase(kind: ControlEventKind.MouseLeave, attach: static (s, h) => s.MouseLeave += h, detach: static (s, h) => s.MouseLeave -= h),
            MouseCase(kind: ControlEventKind.MouseDoubleClick, attach: static (s, h) => s.MouseDoubleClick += h, detach: static (s, h) => s.MouseDoubleClick -= h),
            MouseCase(kind: ControlEventKind.MouseWheel, attach: static (s, h) => s.MouseWheel += h, detach: static (s, h) => s.MouseWheel -= h),
            DragCase(kind: ControlEventKind.DragDrop, attach: static (s, h) => s.DragDrop += h, detach: static (s, h) => s.DragDrop -= h),
            DragCase(kind: ControlEventKind.DragOver, attach: static (s, h) => s.DragOver += h, detach: static (s, h) => s.DragOver -= h),
            DragCase(kind: ControlEventKind.DragEnter, attach: static (s, h) => s.DragEnter += h, detach: static (s, h) => s.DragEnter -= h),
            DragCase(kind: ControlEventKind.DragLeave, attach: static (s, h) => s.DragLeave += h, detach: static (s, h) => s.DragLeave -= h),
            DragCase(kind: ControlEventKind.DragEnd, attach: static (s, h) => s.DragEnd += h, detach: static (s, h) => s.DragEnd -= h));
        private ControlEventWatcher(Control source, ControlEventKind kind, Func<ControlEventSnapshot, Fin<Unit>> publish) {
            this.source = source;
            this.kind = kind;
            this.publish = publish;
            plain = (_, _) => Publish(snapshot: SnapshotOf(kind: kind));
            key = (_, e) => Publish(snapshot: SnapshotOf(kind: kind, keys: Some(e.KeyData)));
            text = (_, e) => Publish(snapshot: SnapshotOf(kind: kind, text: Optional(e.Text)));
            mouse = (_, e) => Publish(snapshot: SnapshotOf(kind: kind, point: Some(e.Location), buttons: Some(e.Buttons), keys: Some(e.Modifiers), delta: Some(e.Delta), pressure: Some(e.Pressure)));
            drag = (_, _) => Publish(snapshot: SnapshotOf(kind: kind));
            _ = Toggle(attach: true);
        }
        internal static ControlEventWatcher Attach(Control source, ControlEventKind kind, Func<ControlEventSnapshot, Fin<Unit>> handler) =>
            new(source: source, kind: kind, publish: handler);
        private Unit Publish(ControlEventSnapshot snapshot) {
            _ = GrasshopperUi.Protect(valid: () => publish(arg: snapshot));
            return unit;
        }
        private ControlEventSnapshot SnapshotOf(
            ControlEventKind kind,
            Option<PointF> point = default,
            Option<MouseButtons> buttons = default,
            Option<Keys> keys = default,
            Option<string> text = default,
            Option<SizeF> delta = default,
            Option<float> pressure = default) =>
            new(Kind: kind, Enabled: source.Enabled, Visible: source.Visible, HasFocus: source.HasFocus, Point: point, Buttons: buttons, Keys: keys, Text: text, Delta: delta, Pressure: pressure);
        private Unit Toggle(bool attach) =>
            EventCases
                .Find(c => c.Kind == kind)
                .Map(c => attach ? c.Attach(arg: this) : c.Detach(arg: this))
                .IfNone(unit);
        private static ControlEventCase PlainCase(ControlEventKind kind, Action<Control, EventHandler<EventArgs>> attach, Action<Control, EventHandler<EventArgs>> detach) =>
            new(Kind: kind, Attach: watcher => watcher.Plain(add: attach), Detach: watcher => watcher.Plain(add: detach));
        private static ControlEventCase KeyCase(ControlEventKind kind, Action<Control, EventHandler<KeyEventArgs>> attach, Action<Control, EventHandler<KeyEventArgs>> detach) =>
            new(Kind: kind, Attach: watcher => watcher.Key(add: attach), Detach: watcher => watcher.Key(add: detach));
        private static ControlEventCase TextCase(ControlEventKind kind, Action<Control, EventHandler<TextInputEventArgs>> attach, Action<Control, EventHandler<TextInputEventArgs>> detach) =>
            new(Kind: kind, Attach: watcher => watcher.Text(add: attach), Detach: watcher => watcher.Text(add: detach));
        private static ControlEventCase MouseCase(ControlEventKind kind, Action<Control, EventHandler<MouseEventArgs>> attach, Action<Control, EventHandler<MouseEventArgs>> detach) =>
            new(Kind: kind, Attach: watcher => watcher.Mouse(add: attach), Detach: watcher => watcher.Mouse(add: detach));
        private static ControlEventCase DragCase(ControlEventKind kind, Action<Control, EventHandler<DragEventArgs>> attach, Action<Control, EventHandler<DragEventArgs>> detach) =>
            new(Kind: kind, Attach: watcher => watcher.Drag(add: attach), Detach: watcher => watcher.Drag(add: detach));
        private Unit Plain(Action<Control, EventHandler<EventArgs>> add) {
            add(arg1: source, arg2: plain);
            return unit;
        }
        private Unit Key(Action<Control, EventHandler<KeyEventArgs>> add) {
            add(arg1: source, arg2: key);
            return unit;
        }
        private Unit Text(Action<Control, EventHandler<TextInputEventArgs>> add) {
            add(arg1: source, arg2: text);
            return unit;
        }
        private Unit Mouse(Action<Control, EventHandler<MouseEventArgs>> add) {
            add(arg1: source, arg2: mouse);
            return unit;
        }
        private Unit Drag(Action<Control, EventHandler<DragEventArgs>> add) {
            add(arg1: source, arg2: drag);
            return unit;
        }
        public void Dispose() =>
            _ = Toggle(attach: false);
    }
}
