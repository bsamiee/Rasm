using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AppKit;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using ObjCRuntime;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino.UI;

namespace Rasm.Rhino.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NativeControls {
    // --- [INPUT]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeEvent(nint view, nint selector, nint current);

    private static readonly Seq<nint> EventSelectors = Seq(Selector.GetHandle("scrollWheel:"), Selector.GetHandle("pressureChangeWithEvent:"));
    private static readonly ConditionalWeakTable<NSView, EventResponder> EventViews = [];
    private static Seq<NativeEvent> EventCallbacks = [];

    public static IO<Unit> Events(Control control, Func<NSView, NSEvent, IO<bool>> receive, IPlugInSink sink, Action<IDisposable> retain) =>
        Lifetime(control, owned =>
            from handler in IO.lift(() => Missing.Unless(control.GetMacControl(), nameof(MacControlExtensions.GetMacControl)))
            let view = handler.EventControl
            let site = new CallbackSite(sink, typeof(NativeControls), nameof(Events))
            from responder in IO.lift(() => EventViews.TryGetValue(view, out EventResponder? existing)
                ? Fin.Succ(existing)
                : new EventResponder(SafeNativeMethods.object_getClass(view.Handle)).Attach(view))
            from id in IO.lift(Guid.NewGuid)
            from attached in Subscriptions.Attach(
                receiver => responder.Receivers.Swap(receivers => receivers.Add((id, receiver, site))),
                _ => responder.Detach(id), receive, owned)
            select unit,
            new CallbackSite(sink, typeof(NativeControls), nameof(Events)), retain);

    public static IO<Unit> Gestures(Control control, Seq<NSGestureRecognizer> recognizers, IPlugInSink sink, Action<IDisposable> retain) =>
        Lifetime(control, owned =>
            from handler in IO.lift(() => Missing.Unless(control.GetMacControl(), nameof(MacControlExtensions.GetMacControl)))
            let view = handler.EventControl
            from attached in recognizers.TraverseM(recognizer =>
                DisposalOps.Acquire(IO.lift(() => new Disposal<NSGestureRecognizer>(recognizer, view.RemoveGestureRecognizer)), static lease => IO.lift(lease.Dispose), lease => {
                    view.AddGestureRecognizer(recognizer);
                    owned(lease);
                })).As()
            select unit,
            new CallbackSite(sink, typeof(NativeControls), nameof(Gestures)), retain);

    private sealed class EventResponder {
        private readonly nint parent;
        private readonly NativeEvent receive;

        internal EventResponder(nint parent) {
            this.parent = parent;
            receive = Dispatch;
        }

        internal Atom<Seq<(Guid Id, Func<NSView, NSEvent, IO<bool>> Receive, CallbackSite Site)>> Receivers { get; } =
            Atom<Seq<(Guid Id, Func<NSView, NSEvent, IO<bool>> Receive, CallbackSite Site)>>([]);

        internal Fin<EventResponder> Attach(NSView view) =>
            Subclass(parent, EventSelectors.Map(selector => (selector, (Delegate)receive))).Map(subclass => {
                EventCallbacks = EventCallbacks.Add(receive);
                EventViews.Add(view, this);
                _ = SafeNativeMethods.object_setClass(view.Handle, subclass);
                return this;
            });

        internal void Detach(Guid id) => Receivers.Swap(receivers => receivers.Filter(receiver => receiver.Id != id));

        private void Dispatch(nint view, nint selector, nint current) {
            bool handled = Receivers.Value.Exists(receiver => Callbacks.Answer(
                from input in IO.lift(() => (
                    Missing.Unless(Runtime.GetNSObject<NSView>(view), nameof(Runtime.GetNSObject)),
                    Missing.Unless(Runtime.GetNSObject<NSEvent>(current), nameof(Runtime.GetNSObject)))
                    .Apply(static (host, native) => (Host: host, Native: native)).As())
                from consumed in receiver.Receive(input.Host, input.Native)
                select consumed, static () => false, receiver.Site));
            if (!handled)
                Marshal.GetDelegateForFunctionPointer<NativeEvent>(SafeNativeMethods.method_getImplementation(ObjCExtensions.GetInstanceMethod(parent, selector)))(view, selector, current);
        }
    }

    internal static Fin<nint> Subclass(nint parent, Seq<(nint Selector, Delegate Implementation)> methods) {
        nint nativeClass = SafeNativeMethods.objc_allocateClassPair(parent, $"{typeof(NativeControls).Assembly.GetName().Name}.{Guid.NewGuid():N}", 0);
        return (from allocated in Refused.Unless(nativeClass != 0, nameof(SafeNativeMethods.objc_allocateClassPair))
                from added in methods.TraverseM(method => Refused.Unless(SafeNativeMethods.class_addMethod(nativeClass, method.Selector,
                    Marshal.GetFunctionPointerForDelegate(method.Implementation), SafeNativeMethods.method_getTypeEncoding(ObjCExtensions.GetInstanceMethod(parent, method.Selector))),
                    nameof(SafeNativeMethods.class_addMethod))).As()
                    .MapFail(error => {
                        SafeNativeMethods.objc_disposeClassPair(nativeClass);
                        return error;
                    })
                select nativeClass).Map(static created => {
                    SafeNativeMethods.objc_registerClassPair(created);
                    return created;
                });
    }

    // --- [LIFETIME]
    internal static IO<Unit> Lifetime(Control control, Func<Action<IDisposable>, IO<Unit>> acquire, CallbackSite site, Action<IDisposable> retain) =>
        IO.lift(static () => Ref(Option<IDisposable>.None)).Bind(mounted =>
            DisposalOps.Acquire(owned =>
                from unload in IO.pure(Callbacks.Handler<EventArgs>(_ => Freed(mounted), site))
                from subscription in Subscriptions.Attach(
                    handler => control.UnLoad += handler, handler => control.UnLoad -= handler, unload, owned)
                from acquired in IO.pure(owned).Bind(acquire)
                select unit,
                lease => {
                    _ = atomic(() => mounted.Value = Some(lease));
                    retain(lease);
                }));

    // --- [STYLES]
    internal static void Field(TextBoxHandler handler) => Scrubbed((NumberField)handler.Widget, handler);

    internal static void Rows(RowGrid grid) =>
        Callbacks.Run(
            IO.lift(() => grid.Styles.Add<object>(style: null, target =>
                Callbacks.Run(
                    Optional(target as IMacViewHandler).Map(handler => Sized(grid, handler)),
                    new CallbackSite(grid.Sink, typeof(RowGrid), nameof(Sized))))),
            new CallbackSite(grid.Sink, typeof(RowGrid), nameof(Rows)));

    // --- [SCRUB]
    private static unsafe NSObject? Monitor(Func<NativeHandle, NativeHandle> receive) {
        NSApplication.EnsureUIThread();
        BlockLiteral block = new((delegate* unmanaged<nint, NativeHandle, NativeHandle>)&Monitored,
            receive, typeof(NativeControls), nameof(Monitored));
        using (block)
            return Runtime.GetNSObject(SafeNativeMethods.objc_msgSend(Class.GetHandle(typeof(NSEvent)),
                Selector.GetHandle("addLocalMonitorForEventsMatchingMask:handler:"), (ulong)NSEventMask.KeyDown, &block));
    }

    [UnmanagedCallersOnly]
    [UserDelegateType(typeof(Func<NSEvent, NSEvent?>))]
    private static NativeHandle Monitored(nint block, NativeHandle current) =>
        BlockLiteral.GetTarget<Func<NativeHandle, NativeHandle>>(block)(current);

    private static void Scrubbed(NumberField field, TextBoxHandler handler) =>
        Callbacks.Run(
            from held in IO.lift(static () => Ref(Option<IDisposable>.None))
            let site = new CallbackSite(field.Sink, typeof(NumberField), nameof(Scrubbed))
            from attached in IO.lift(() => {
                field.ScrubStarted += Callbacks.Handler<EventArgs>(_ => Held(handler, held, site), site);
                field.ScrubEnded += Callbacks.Handler<EventArgs>(_ => Freed(held), site);
                field.LoadComplete += Callbacks.Handler<EventArgs>(_ => IO.lift(() => { field.Font = HostTheme.Digits(field.Font); }), site);
                handler.Control.AccessibilityCustomActions = Steps(field.Adjust);
            })
            select unit,
            new CallbackSite(field.Sink, typeof(TextBoxHandler), nameof(Field)));

    private static IO<Unit> Held(TextBoxHandler handler, Ref<Option<IDisposable>> held, CallbackSite site) {
        Action<NSObject> release = static token => DisposalOps.Release(
            Seq<IDisposable>(token, new Disposal<NSObject>(token, NSEvent.RemoveMonitor))).Run();
        return DisposalOps.Acquire(owned =>
            from monitor in DisposalOps.Acquire(IO.lift(() => Monitor(key => Escaped(handler, key, site))), token => IO.lift(() => Optional(token).Iter(release)), token => Optional(token).Iter(value => owned(new Disposal<NSObject>(value, release))))
            from present in IO.lift(() => Missing.Unless(monitor, nameof(Monitor)))
            from hidden in DisposalOps.Acquire(IO.lift(static () => new Disposal<Unit>(unit, static _ => NSCursor.Unhide())), static lease => IO.lift(lease.Dispose), lease => {
                NSCursor.Hide();
                owned(lease);
            })
            from pointer in DisposalOps.Acquire(IO.lift(static () => new Disposal<Unit>(unit, static _ => IO.lift(static () =>
                Refused.Unless(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition(connected: true) == 0,
                    nameof(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition))).Run())), static lease => IO.lift(lease.Dispose), lease => {
                        _ = Refused.Unless(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition(connected: false) == 0,
                            nameof(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition)).ThrowIfFail();
                        owned(lease);
                    })
            select unit,
            lease => _ = atomic(() => held.Value = Some(lease)));
    }

    private static IO<Unit> Freed(Ref<Option<IDisposable>> held) =>
        from released in atomicIO(() => {
            Option<IDisposable> lease = held.Value;
            held.Value = None;
            return lease;
        })
        from disposed in IO.lift(() => released.Iter(static lease => lease.Dispose()))
        select unit;

    private static NativeHandle Escaped(TextBoxHandler handler, NativeHandle current, CallbackSite site) =>
        Callbacks.Answer(
            from key in IO.lift(() => Missing.Unless(Runtime.GetNSObject<NSEvent>(current), nameof(Runtime.GetNSObject)))
            from result in IO.lift(() => {
                KeyEventArgs escape = key.ToEtoKeyEventArgs();
                bool escaped = escape.Key == Keys.Escape;
                if (escaped)
                    handler.OnKeyDown(escape);
                return Runtime.RetainAndAutoreleaseNSObject(escaped && escape.Handled ? null : key);
            })
            select result,
            () => current, site);

    // --- [SIZES]
    private static IO<Unit> Sized(RowGrid grid, IMacViewHandler handler) =>
        IO.lift(() => {
            NSView native = handler.ContainerControl;
            NSControlSize size = SizeOf(handler.Widget);
            _ = native switch {
                NSControl control => control.ControlSize = size,
                NSProgressIndicator progress => progress.ControlSize = size,
                _ => size,
            };
            if (handler.Widget is ProgressBar)
                handler.Widget.Height = (int)Math.Ceiling(handler.GetAlignmentSizeForSize(native.FittingSize).Height);
            _ = grid.Caption(handler.Widget).Iter(label => native.AccessibilityTitleUIElement = label.GetContainerView());
        });

    private static NSControlSize SizeOf(Control control) =>
        toSeq(control.Parents).Exists(static parent =>
            Optional(parent.Style).Exists(static style => style.Split(' ').Contains(Panels.EtoPanelStyleName, StringComparer.Ordinal)))
            ? NSControlSize.Small
            : NSControlSize.Regular;

    // --- [STEPS]
    private static NSAccessibilityCustomAction[] Steps(Func<int, bool> adjust) =>
        [.. Seq((Action: NSAccessibilityActions.IncrementAction, Steps: 1), (Action: NSAccessibilityActions.DecrementAction, Steps: -1))
            .Choose(row => Optional(NSAccessibility.GetActionDescription(row.Action)).Map(name => new NSAccessibilityCustomAction { Name = name, Handler = () => adjust(row.Steps) }))];
}
