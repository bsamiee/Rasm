using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rhino.Display;
using Rhino.UI;

namespace Rasm.Rhino.Display.Feedback;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PointerHooks {
    // --- [HOOKS]
    private sealed class BeginMouseMoveHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnMouseMove(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class EndMouseMoveHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnEndMouseMove(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class BeginMouseDownHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnMouseDown(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class EndMouseDownHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnEndMouseDown(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class BeginMouseUpHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnMouseUp(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class EndMouseUpHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnEndMouseUp(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class BeginMouseDoubleClickHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnMouseDoubleClick(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class MouseEnterHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnMouseEnter(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class MouseHoverHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnMouseHover(MouseCallbackEventArgs e) => handler(this, e);
    }

    private sealed class MouseLeaveHook(EventHandler<MouseCallbackEventArgs> handler) : MouseCallback {
        protected override void OnMouseLeave(MouseCallbackEventArgs e) => handler(this, e);
    }

    private static DocumentEvent<MouseCallbackEventArgs> Row(Func<EventHandler<MouseCallbackEventArgs>, MouseCallback> hook, [CallerMemberName] string member = "") =>
        new(
            typeof(RhinoView),
            member,
            (deliver, site) => IO.lift<IDisposable>(() => {
                MouseCallback callback = hook(Callbacks.Handler(deliver, site));
                callback.Enabled = true;
                return new Disposal<MouseCallback>(callback, static held => held.Enabled = false);
            }),
            static args => Conversions.Serial(args.View.Document));

    // --- [ROWS]
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseMove = Row(static handler => new BeginMouseMoveHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseMove = Row(static handler => new EndMouseMoveHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseDown = Row(static handler => new BeginMouseDownHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseDown = Row(static handler => new EndMouseDownHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseUp = Row(static handler => new BeginMouseUpHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseUp = Row(static handler => new EndMouseUpHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseDoubleClick = Row(static handler => new BeginMouseDoubleClickHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseEnter = Row(static handler => new MouseEnterHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseHover = Row(static handler => new MouseHoverHook(handler));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseLeave = Row(static handler => new MouseLeaveHook(handler));
}
