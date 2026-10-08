using AppKit;
using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino.UI;

namespace Rasm.Rhino.UI.macOS;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NativeControls {
    // --- [STYLES]
    internal static void Field(TextBoxHandler handler) => _ = Scrubbed((NumberField)handler.Widget, handler);

    internal static void Rows(RowGrid grid) =>
        _ = Callbacks.Answer(
            IO.lift(() => grid.Styles.Add<object>(style: null, target =>
                _ = Callbacks.Answer(
                    Optional(target as IMacViewHandler).Map(handler => Sized(grid, handler)),
                    static () => unit,
                    static () => unit,
                    new CallbackSite(grid.Sink, typeof(RowGrid), nameof(Sized))))),
            static () => unit,
            new CallbackSite(grid.Sink, typeof(RowGrid), nameof(Rows)));

    // --- [SCRUB]
    private static Unit Scrubbed(NumberField field, TextBoxHandler handler) =>
        Callbacks.Answer(
            from held in IO.lift(static () => Atom((Held: Option<NSObject>.None, Taken: Option<NSObject>.None)))
            let site = new CallbackSite(field.Sink, typeof(NumberField), nameof(Scrubbed))
            from attached in IO.lift(() => {
                field.ScrubStarted += Callbacks.Handler<EventArgs>(_ => Held(handler, held, site), site);
                field.ScrubEnded += Callbacks.Handler<EventArgs>(_ => Freed(held), site);
                field.LoadComplete += Callbacks.Handler<EventArgs>(_ => IO.lift(() => { field.Font = HostTheme.Digits(field.Font); }), site);
                handler.Control.AccessibilityCustomActions = Steps(field.Adjust);
            })
            select unit,
            static () => unit,
            new CallbackSite(field.Sink, typeof(TextBoxHandler), nameof(Field)));

    private static IO<Unit> Held(TextBoxHandler handler, Atom<(Option<NSObject> Held, Option<NSObject> Taken)> held, CallbackSite site) =>
        from monitor in IO.lift(() => Missing.Unless(
            NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown, key => Escaped(handler, key, site)),
            nameof(NSEvent.AddLocalMonitorForEventsMatchingMask)))
        from stored in held.SwapIO(_ => (Some(monitor), Option<NSObject>.None))
        from hidden in IO.lift(NSCursor.Hide)
        from detached in IO.lift(static () => Refused.Unless(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition(connected: false) == 0, nameof(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition)))
        select unit;

    private static IO<Unit> Freed(Atom<(Option<NSObject> Held, Option<NSObject> Taken)> held) =>
        from cleared in held.SwapIO(static state => (Option<NSObject>.None, state.Held))
        from released in cleared.Taken.Match(
            Some: static token =>
                from removed in IO.lift(() => NSEvent.RemoveMonitor(token))
                from shown in IO.lift(NSCursor.Unhide)
                from attached in IO.lift(static () => Refused.Unless(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition(connected: true) == 0, nameof(SafeNativeMethods.CGAssociateMouseAndMouseCursorPosition)))
                select unit,
            None: static () => IO.pure(unit))
        select unit;

    private static NSEvent Escaped(TextBoxHandler handler, NSEvent key, CallbackSite site) =>
        Callbacks.Answer(IO.lift(() => key.ToEtoKeyEventArgs() is { Key: Keys.Escape } escape ? Forwarded(handler, escape, key) : key), () => key, site);

    private static NSEvent Forwarded(TextBoxHandler handler, KeyEventArgs escape, NSEvent key) {
        handler.OnKeyDown(escape);
#pragma warning disable MA0191
        return escape.Handled ? null! : key;
#pragma warning restore MA0191
    }

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
