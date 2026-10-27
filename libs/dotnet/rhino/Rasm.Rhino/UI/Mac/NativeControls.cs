using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AppKit;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using ObjCRuntime;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.UI.Numeric;
using Rasm.RhinoCommon.UI.Styling;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]

namespace Rasm.RhinoCommon.UI.Mac;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct MethodDescription(nint Name, nint Types);

// --- [SERVICES] ------------------------------------------------------------------------
[SupportedOSPlatform("macos")]
internal static partial class SafeNativeMethods {
    // --- [POINTER]
    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    internal static partial int CGAssociateMouseAndMouseCursorPosition([MarshalAs(UnmanagedType.Bool)] bool connected);

    // --- [OBJECTIVE_C]
    private const string ObjectiveCLibrary = "/usr/lib/libobjc.dylib";

    [LibraryImport(ObjectiveCLibrary, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint objc_allocateClassPair(nint superclass, string name, nuint extraBytes);

    [LibraryImport(ObjectiveCLibrary)]
    internal static partial void objc_registerClassPair(nint cls);

    [LibraryImport(ObjectiveCLibrary)]
    internal static partial void objc_disposeClassPair(nint cls);

    [LibraryImport(ObjectiveCLibrary)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool class_addMethod(nint cls, nint selector, nint implementation, nint encoding);

    [LibraryImport(ObjectiveCLibrary)]
    internal static partial nint method_getTypeEncoding(nint method);

    [LibraryImport(ObjectiveCLibrary)]
    internal static partial MethodDescription protocol_getMethodDescription(nint protocol, nint selector, [MarshalAs(UnmanagedType.I1)] bool required, [MarshalAs(UnmanagedType.I1)] bool instance);

    [LibraryImport(ObjectiveCLibrary)]
    internal static partial nint method_getImplementation(nint method);

    [LibraryImport(ObjectiveCLibrary)]
    internal static partial nint object_setClass(nint instance, nint cls);

    [LibraryImport(ObjectiveCLibrary)]
    internal static partial nint object_getClass(nint instance);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NativeControls {
    // --- [INPUT]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeEvent(nint view, nint selector, nint current);

    private static readonly Seq<nint> EventSelectors = Seq(Selector.GetHandle("scrollWheel:"), Selector.GetHandle("pressureChangeWithEvent:"));
    private static readonly ConditionalWeakTable<NSView, EventResponder> EventViews = [];

    public static IO<Unit> Events(Control control, Func<NSView, NSEvent, IO<bool>> receive, object owner) =>
        Lifetime(control,
            from handler in IO.lift(() => Conversions.Found(control.GetMacControl()))
            let view = handler.EventControl
            from responder in IO.lift(() => EventViews.TryGetValue(view, out EventResponder? existing)
                ? existing
                : new EventResponder(SafeNativeMethods.object_getClass(view.Handle), owner).Attach(view))
            from attached in use(() => responder.Attached(new EventReceiver(receive, owner)), responder.Detach)
            select unit,
            owner);

    public static IO<Unit> Gestures(Control control, Seq<NSGestureRecognizer> recognizers, object owner) =>
        Lifetime(control,
            from handler in IO.lift(() => Conversions.Found(control.GetMacControl()))
            let view = handler.EventControl
            from attached in recognizers.TraverseM(recognizer => use(() => {
                view.AddGestureRecognizer(recognizer);
                return recognizer;
            }, view.RemoveGestureRecognizer)).As()
            select unit,
            owner);

    private sealed class EventReceiver(Func<NSView, NSEvent, IO<bool>> receive, object owner) {
        internal bool Consumed(nint view, nint current) =>
            owner.Answer(
                from input in IO.lift(() => (Conversions.Found(Runtime.GetNSObject<NSView>(view)), Conversions.Found(Runtime.GetNSObject<NSEvent>(current)))
                    .Apply(static (host, native) => (Host: host, Native: native)).As())
                from consumed in receive(input.Host, input.Native)
                select consumed,
                failed: false);
    }

    private sealed class EventResponder {
        private readonly nint parent;
        private readonly object owner;
        private readonly NativeEvent receive;
        private readonly Atom<Seq<EventReceiver>> receivers = Atom(Seq<EventReceiver>());

        internal EventResponder(nint parent, object owner) {
            (this.parent, this.owner) = (parent, owner);
            receive = Dispatch;
        }

        internal Fin<EventResponder> Attach(NSView view) =>
            Subclass(parent, EventSelectors.Map(selector => (selector, (Delegate)receive)), []).Map(subclass => {
                _ = GCHandle.Alloc(receive);
                EventViews.Add(view, this);
                _ = SafeNativeMethods.object_setClass(view.Handle, subclass);
                return this;
            });

        internal EventReceiver Attached(EventReceiver receiver) {
            _ = receivers.Swap(held => held.Add(receiver));
            return receiver;
        }

        internal void Detach(EventReceiver receiver) => _ = receivers.Swap(held => held.Filter(row => !ReferenceEquals(row, receiver)));

        private void Dispatch(nint view, nint selector, nint current) =>
            _ = owner.Run(IO.lift(() => {
                if (!receivers.Value.Exists(receiver => receiver.Consumed(view, current)))
                    Marshal.GetDelegateForFunctionPointer<NativeEvent>(SafeNativeMethods.method_getImplementation(ObjCExtensions.GetInstanceMethod(parent, selector)))(view, selector, current);
            }));
    }

    internal static Fin<nint> Subclass(nint parent, Seq<(nint Selector, Delegate Implementation)> methods, Seq<nint> protocols) {
        nint nativeClass = SafeNativeMethods.objc_allocateClassPair(parent, $"{typeof(NativeControls).Assembly.GetName().Name}.{Guid.NewGuid():N}", 0);
        return (from allocated in RhinoFault.Unless(nativeClass != 0)
                from adopted in protocols.TraverseM(protocol => RhinoFault.Unless(ObjCExtensions.ClassAddProtocol(nativeClass, protocol))).As()
                from added in methods.TraverseM(method =>
                    from types in Encoded(parent, method.Selector, protocols)
                    from bound in RhinoFault.Unless(SafeNativeMethods.class_addMethod(nativeClass, method.Selector, Marshal.GetFunctionPointerForDelegate(method.Implementation), types))
                    select bound).As()
                    .MapFail(error => {
                        SafeNativeMethods.objc_disposeClassPair(nativeClass);
                        return error;
                    })
                select nativeClass).Map(static created => {
                    SafeNativeMethods.objc_registerClassPair(created);
                    return created;
                });
    }

    private static Fin<nint> Encoded(nint parent, nint selector, Seq<nint> protocols) =>
        Conversions.Found(Inherited(parent, selector) | Declared(selector, protocols));

    private static Option<nint> Inherited(nint parent, nint selector) =>
        Optional(ObjCExtensions.GetInstanceMethod(parent, selector)).Filter(static method => method != 0).Map(SafeNativeMethods.method_getTypeEncoding);

    private static Option<nint> Declared(nint selector, Seq<nint> protocols) =>
        protocols.Bind(protocol => Seq(
                SafeNativeMethods.protocol_getMethodDescription(protocol, selector, required: true, instance: true),
                SafeNativeMethods.protocol_getMethodDescription(protocol, selector, required: false, instance: true)))
            .Map(static description => description.Types).Find(static types => types != 0);

    // --- [LIFETIME]
    internal static IO<Unit> Lifetime(Control control, IO<Unit> acquire, object owner) =>
        from scope in IO.env
        from mounted in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
        from leased in Products.Swapped(mounted,
            from unloaded in Subscriptions.Host<EventArgs>(handler => control.UnLoad += handler, handler => control.UnLoad -= handler)
                .Inline(_ => Products.Cleared(mounted).Under(scope), owner)
            from acquired in acquire
            select unit)
        select unit;

    // --- [STYLES]
    internal static void Field(TextBoxHandler handler) => Scrubbed((NumberBox)handler.Widget, handler);

    // --- [SCRUB]
    private static void Scrubbed(NumberBox field, TextBoxHandler handler) =>
        _ = field.Owner.Run(
            from scope in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
            from held in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
            from attached in (
                from started in Subscriptions.Host<EventArgs>(h => field.ScrubStarted += h, h => field.ScrubStarted -= h)
                    .Inline(_ => Products.Swapped(held, Hidden(handler, field.Owner)).Under(scope), field.Owner)
                from ended in Subscriptions.Host<EventArgs>(h => field.ScrubEnded += h, h => field.ScrubEnded -= h)
                    .Inline(_ => Products.Cleared(held).Under(scope), field.Owner)
                from loaded in Subscriptions.Host<EventArgs>(h => field.LoadComplete += h, h => field.LoadComplete -= h)
                    .Inline(_ => IO.lift(() => { field.Font = HostTheme.Digits(field.Font); }), field.Owner)
                select unit).Under(scope)
            from described in IO.lift(() => {
                handler.Control.AccessibilityCustomActions = Actions(Seq<(NSString, Func<bool>)>(
                    (NSAccessibilityActions.IncrementAction, () => field.Adjust(1)), (NSAccessibilityActions.DecrementAction, () => field.Adjust(-1))));
            })
            select unit);

    private static IO<Unit> Hidden(TextBoxHandler handler, object owner) =>
        from monitor in use(() => {
            NSApplication.EnsureUIThread();
            return NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown, current => Escaped(handler, current, owner)!);
        }, static monitor => NSEvent.RemoveMonitor(monitor))
        from hidden in IO.lift(NSCursor.Hide) >> Products.Released(NSCursor.Unhide)
        from pointer in use(IO.lift(static () => Associated(connected: false)).Map(static _ => new object()), static _ => IO.lift(static () => Associated(connected: true)))
        select unit;

    private static Fin<Unit> Associated(bool connected) =>
        RhinoFault.Unless(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition(connected) is 0);

    private static NSEvent? Escaped(TextBoxHandler handler, NSEvent current, object owner) =>
        owner.Answer(
            IO.lift(NSEvent? () => {
                KeyEventArgs escape = current.ToEtoKeyEventArgs();
                if (escape.Key is not Keys.Escape)
                    return current;
                handler.OnKeyDown(escape);
                return escape.Handled ? null : current;
            }),
            failed: current);

    // --- [SIZES]
    internal static IO<Unit> Seated(Control slot, Option<string> label) =>
        from handler in IO.lift(() => Conversions.Found(slot.Handler as IMacViewHandler))
        from sized in Sized(handler, label)
        select sized;

    private static IO<Unit> Sized(IMacViewHandler handler, Option<string> label) =>
        IO.lift(() => {
            NSView native = handler.ContainerControl;
            NSControlSize size = HostTheme.Compact(handler.Widget) ? NSControlSize.Small : NSControlSize.Regular;
            if (native is NSControl control)
                control.ControlSize = size;
            else if (native is NSProgressIndicator progress)
                progress.ControlSize = size;
            if (handler.Widget is ProgressBar)
                handler.Widget.Height = (int)Math.Ceiling(handler.GetAlignmentSizeForSize(native.FittingSize).Height);
            _ = label.Iter(caption => native.AccessibilityLabel = caption);
        });

    // --- [ACCESSIBILITY]
    internal static NSAccessibilityCustomAction[] Actions(Seq<(NSString Action, Func<bool> Perform)> rows) =>
        [.. rows.Choose(static row => Optional(NSAccessibility.GetActionDescription(row.Action)).Map(name => new NSAccessibilityCustomAction { Name = name, Handler = row.Perform }))];
}
