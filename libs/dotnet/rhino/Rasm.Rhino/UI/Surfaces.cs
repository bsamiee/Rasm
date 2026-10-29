using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Styling;
using Rhino.UI;
using Rhino.UI.Theme;

namespace Rasm.Rhino.UI;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Surface<TState> : Drawable where TState : notnull {
    private readonly Atom<TState> state;
    private readonly Action<Graphics, RectangleF, TState, ThemeZone> paint;

    public Surface(TState initial, Action<Graphics, RectangleF, TState, ThemeZone> paint) => (state, this.paint) = (Atom(initial), paint);

    public TState State => state.Value;

    public IO<Unit> Show(TState next) => state.SwapIO(_ => next).Bind(_ => IO.lift(() => Invalidate()));

    internal bool Stepped(Duration now, Func<Duration, TState, Option<TState>> step) =>
        step(now, state.Value).Match(
            Some: next => {
                _ = state.Swap(_ => next);
                Invalidate();
                return true;
            },
            None: static () => false);

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        paint(e.Graphics, new RectangleF(PointF.Empty, Size), state.Value, ThemeSettings.Content);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Surfaces {
    // --- [SURFACES]
    public static IO<Surface<TState>> Of<TState>(TState initial, Action<Graphics, RectangleF, TState, ThemeZone> paint) where TState : notnull =>
        from surface in use(() => new Surface<TState>(initial, paint))
        from themed in HostTheme.Applied(surface, IO.lift(surface.Invalidate))
        select surface;

    // --- [FRAMES]
    public static IO<IO<Unit>> Animated<TState>(Surface<TState> surface, Func<Duration, TState, Option<TState>> step) where TState : notnull =>
        from running in IO.lift(static () => Ref(Option<Func<bool, IO<Unit>>>.None))
        from linked in ViewOps.WhileLoaded(surface,
            from link in HostPlatform.Current.FrameLink(surface, now => surface.Stepped(now, step))
            from held in atomicIO(() => running.Value = Some(link))
            from cleared in Products.Released(() => atomic(() => running.Value = None))
            select unit)
        select running.ValueIO.Bind(static held => held.Match(Some: static run => run(true), None: static () => IO.pure(unit)));
}
