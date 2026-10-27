using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rhino.UI;

namespace Rasm.Rhino.Display;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MouseCallbacks {
    // --- [HOOKS]
    private sealed class BeginMouseMoveHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnMouseMove(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class EndMouseMoveHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnEndMouseMove(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class BeginMouseDownHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnMouseDown(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class EndMouseDownHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnEndMouseDown(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class BeginMouseUpHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnMouseUp(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class EndMouseUpHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnEndMouseUp(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class BeginMouseDoubleClickHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnMouseDoubleClick(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class MouseEnterHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnMouseEnter(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class MouseHoverHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnMouseHover(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private sealed class MouseLeaveHook(Func<MouseCallbackEventArgs, IO<Unit>> deliver, object owner) : MouseCallback {
        protected override void OnMouseLeave(MouseCallbackEventArgs e) => _ = owner.Run(IO.pure(e).Bind(deliver));
    }

    private static DocumentEvent<MouseCallbackEventArgs> Row(
        Func<Func<MouseCallbackEventArgs, IO<Unit>>, object, MouseCallback> hook, [CallerArgumentExpression(nameof(hook))] string label = "") =>
        new(
            label,
            (deliver, owner) => use(() => {
                MouseCallback callback = hook(deliver, owner);
                callback.Enabled = true;
                return callback;
            }, static callback => callback.Enabled = false).Map(unit),
            static args => Conversions.Serial(args.View.Document));

    // --- [ROWS]
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseMove = Row(static (deliver, owner) => new BeginMouseMoveHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseMove = Row(static (deliver, owner) => new EndMouseMoveHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseDown = Row(static (deliver, owner) => new BeginMouseDownHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseDown = Row(static (deliver, owner) => new EndMouseDownHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseUp = Row(static (deliver, owner) => new BeginMouseUpHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> EndMouseUp = Row(static (deliver, owner) => new EndMouseUpHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> BeginMouseDoubleClick = Row(static (deliver, owner) => new BeginMouseDoubleClickHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseEnter = Row(static (deliver, owner) => new MouseEnterHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseHover = Row(static (deliver, owner) => new MouseHoverHook(deliver, owner));
    public static readonly DocumentEvent<MouseCallbackEventArgs> MouseLeave = Row(static (deliver, owner) => new MouseLeaveHook(deliver, owner));
}
