using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rhino.Display;
using Rhino.UI;

namespace Rasm.Rhino.Display.Feedback;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PointerHooks {
    // --- [HOOKS]
    private sealed class BeginMouseMoveHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnMouseMove(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class EndMouseMoveHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnEndMouseMove(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class BeginMouseDownHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnMouseDown(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class EndMouseDownHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnEndMouseDown(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class BeginMouseUpHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnMouseUp(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class EndMouseUpHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnEndMouseUp(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class BeginMouseDoubleClickHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnMouseDoubleClick(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class MouseEnterHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnMouseEnter(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class MouseHoverHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnMouseHover(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private sealed class MouseLeaveHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, CallbackSite site) : MouseCallback {
        protected override void OnMouseLeave(MouseCallbackEventArgs e) => Callbacks.Answer(e, deliver, static () => unit, site);
    }

    private static DocumentEvent<MouseCallbackEventArgs> Row(Func<Func<MouseCallbackEventArgs, IO<Unit>>, CallbackSite, MouseCallback> hook, [CallerMemberName] string member = "") =>
        new(
            typeof(RhinoView),
            member,
            (deliver, site) => IO.lift(IDisposable () => {
                MouseCallback callback = hook(deliver, site);
                callback.Enabled = true;
                return new Disposal<MouseCallback>(callback, static held => held.Enabled = false);
            }),
            static args => Conversions.Serial(args.View.Document));

    // --- [ROWS]
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseMove = Row(static (deliver, site) => new BeginMouseMoveHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseMove = Row(static (deliver, site) => new EndMouseMoveHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseDown = Row(static (deliver, site) => new BeginMouseDownHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseDown = Row(static (deliver, site) => new EndMouseDownHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseUp = Row(static (deliver, site) => new BeginMouseUpHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseUp = Row(static (deliver, site) => new EndMouseUpHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseDoubleClick = Row(static (deliver, site) => new BeginMouseDoubleClickHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseEnter = Row(static (deliver, site) => new MouseEnterHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseHover = Row(static (deliver, site) => new MouseHoverHook(deliver, site));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseLeave = Row(static (deliver, site) => new MouseLeaveHook(deliver, site));
}
