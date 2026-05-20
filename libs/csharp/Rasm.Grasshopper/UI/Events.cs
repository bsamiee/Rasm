using System.Runtime.InteropServices;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
[Union]
public partial record UiEvent {
    private UiEvent() { }
    public sealed record PaintCase(CanvasPaintPhase Phase, Func<PaintScope, Fin<Unit>> Handler) : UiEvent;
    public sealed record DocumentChangedCase(Func<DocumentSnapshot, Fin<Unit>> Handler) : UiEvent;
    public sealed record SelectionChangedCase(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> Handler) : UiEvent;
    public sealed record TimerCase(TimeSpan Interval, Func<Fin<Unit>> Handler) : UiEvent;
    public sealed record ControlCase(Control Source, ControlLifecycle Lifecycle, Func<ControlLifecycleSnapshot, Fin<Unit>> Handler) : UiEvent;

    public static UiEvent Paint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler) =>
        new PaintCase(Phase: phase, Handler: handler);
    public static UiEvent DocumentChanged(Func<DocumentSnapshot, Fin<Unit>> handler) =>
        new DocumentChangedCase(Handler: handler);
    public static UiEvent SelectionChanged(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) =>
        new SelectionChangedCase(Handler: handler);
    public static UiEvent Timer(TimeSpan interval, Func<Fin<Unit>> handler) =>
        new TimerCase(Interval: interval, Handler: handler);
    public static UiEvent Control(Control source, ControlLifecycle lifecycle, Func<ControlLifecycleSnapshot, Fin<Unit>> handler) =>
        new ControlCase(Source: source, Lifecycle: lifecycle, Handler: handler);
}

public enum ControlLifecycle { Load, LoadComplete, UnLoad, Shown, GotFocus, LostFocus, SizeChanged, EnabledChanged }

[StructLayout(LayoutKind.Auto)]
public readonly record struct ControlLifecycleSnapshot(ControlLifecycle Lifecycle, bool Enabled, bool Visible);

public sealed record EventRequest(UiEvent Event) : GhUiRequest<IDisposable> {
    internal override GrasshopperUiPolicy Policy => Events.PolicyOf(uiEvent: Event);
    internal override Fin<IDisposable> Apply(GrasshopperUi.Scope scope) => Events.Subscribe(uiEvent: Event).Run(scope: scope);
}

// --- [SERVICES] --------------------------------------------------------------------------
internal static partial class Events {
    internal static GrasshopperUiPolicy PolicyOf(UiEvent uiEvent) =>
        uiEvent switch {
            UiEvent.PaintCase or UiEvent.DocumentChangedCase or UiEvent.SelectionChangedCase => GrasshopperUiPolicy.Document(),
            UiEvent.TimerCase or UiEvent.ControlCase => GrasshopperUiPolicy.Read,
            _ => GrasshopperUiPolicy.Read,
        };

    internal static GrasshopperUiIntent<IDisposable> Subscribe(UiEvent uiEvent) =>
        uiEvent switch {
            UiEvent.PaintCase p => Paint.Hook(phase: p.Phase, paint: p.Handler),
            UiEvent.DocumentChangedCase d => SubscribeDocumentChange(handler: d.Handler),
            UiEvent.SelectionChangedCase s => SubscribeSelectionChange(handler: s.Handler),
            UiEvent.TimerCase t => SubscribeTimer(interval: t.Interval, handler: t.Handler),
            UiEvent.ControlCase c => SubscribeControl(source: c.Source, lifecycle: c.Lifecycle, handler: c.Handler),
            _ => GhUi.Document<IDisposable>(run: _ => Fin.Fail<IDisposable>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Subscribe)), detail: $"event kind not supported: {uiEvent.GetType().Name}"))),
        };

    // --- [OPERATIONS] ----------------------------------------------------------------------
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

    private static GrasshopperUiIntent<IDisposable> SubscribeControl(Control source, ControlLifecycle lifecycle, Func<ControlLifecycleSnapshot, Fin<Unit>> handler) =>
        GhUi.Read<IDisposable>(run: _ =>
            from validSource in Optional(source).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null control"))
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeControl)), detail: "null handler"))
            select (IDisposable)ControlLifecycleWatcher.Attach(source: validSource, lifecycle: lifecycle, handler: valid));

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
            Seq<DocumentObjectSnapshot> selected = toSeq(objects.SelectedObjects.Select(SnapshotObjectFor));
            _ = GrasshopperUi.Protect(valid: () => handler(arg: selected));
            return unit;
        }
        private static DocumentObjectSnapshot SnapshotObjectFor(IDocumentObject obj) =>
            new(Id: obj.InstanceId, Name: obj.Nomen.Name, DisplayName: obj.DisplayName,
                Selected: obj.Selected, Activity: obj.Activity.ToString(),
                Display: obj.Display.ToString(), Phase: obj.Phase.ToString(), State: obj.State.ToString(),
                Bounds: obj.Attributes.Bounds, Pivot: obj.Attributes.Pivot);
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

    private sealed class ControlLifecycleWatcher : IDisposable {
        private readonly Control source;
        private readonly EventHandler<EventArgs> handler;
        private readonly ControlLifecycle lifecycle;
        private ControlLifecycleWatcher(Control source, ControlLifecycle lifecycle, Func<ControlLifecycleSnapshot, Fin<Unit>> publish) {
            this.source = source;
            this.lifecycle = lifecycle;
            handler = (_, _) => _ = GrasshopperUi.Protect(valid: () => publish(arg: new ControlLifecycleSnapshot(Lifecycle: lifecycle, Enabled: source.Enabled, Visible: source.Visible)));
            _ = Attach();
        }
        internal static ControlLifecycleWatcher Attach(Control source, ControlLifecycle lifecycle, Func<ControlLifecycleSnapshot, Fin<Unit>> handler) =>
            new(source: source, lifecycle: lifecycle, publish: handler);
        private Unit Attach() =>
            lifecycle switch {
                ControlLifecycle.Load => AttachTo(add: static (s, h) => s.Load += h),
                ControlLifecycle.LoadComplete => AttachTo(add: static (s, h) => s.LoadComplete += h),
                ControlLifecycle.UnLoad => AttachTo(add: static (s, h) => s.UnLoad += h),
                ControlLifecycle.Shown => AttachTo(add: static (s, h) => s.Shown += h),
                ControlLifecycle.GotFocus => AttachTo(add: static (s, h) => s.GotFocus += h),
                ControlLifecycle.LostFocus => AttachTo(add: static (s, h) => s.LostFocus += h),
                ControlLifecycle.SizeChanged => AttachTo(add: static (s, h) => s.SizeChanged += h),
                ControlLifecycle.EnabledChanged => AttachTo(add: static (s, h) => s.EnabledChanged += h),
                _ => unit,
            };
        private Unit Detach() =>
            lifecycle switch {
                ControlLifecycle.Load => AttachTo(add: static (s, h) => s.Load -= h),
                ControlLifecycle.LoadComplete => AttachTo(add: static (s, h) => s.LoadComplete -= h),
                ControlLifecycle.UnLoad => AttachTo(add: static (s, h) => s.UnLoad -= h),
                ControlLifecycle.Shown => AttachTo(add: static (s, h) => s.Shown -= h),
                ControlLifecycle.GotFocus => AttachTo(add: static (s, h) => s.GotFocus -= h),
                ControlLifecycle.LostFocus => AttachTo(add: static (s, h) => s.LostFocus -= h),
                ControlLifecycle.SizeChanged => AttachTo(add: static (s, h) => s.SizeChanged -= h),
                ControlLifecycle.EnabledChanged => AttachTo(add: static (s, h) => s.EnabledChanged -= h),
                _ => unit,
            };
        private Unit AttachTo(Action<Control, EventHandler<EventArgs>> add) {
            add(arg1: source, arg2: handler);
            return unit;
        }
        public void Dispose() =>
            _ = Detach();
    }
}
