using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Rhino.UI;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Surface<TState> : Drawable where TState : notnull {
    private readonly Atom<TState> state;
    private readonly Action<Graphics, RectangleF, TState> paint;

    public Surface(TState initial, Action<Graphics, RectangleF, TState> paint) => (state, this.paint) = (Atom(initial), paint);

    public TState State => state.Value;

    public IO<Unit> Show(TState next) => state.SwapIO(_ => next).Bind(_ => IO.lift(() => Invalidate()));

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);
        paint(e.Graphics, new RectangleF(PointF.Empty, Size), state.Value);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Surfaces {
    // --- [SURFACES]
    public static IO<Surface<TState>> Of<TState>(TState initial, Action<Graphics, RectangleF, TState> paint, IPlugInLifetime owner) where TState : notnull =>
        from surface in use(() => new Surface<TState>(initial, paint))
        from themed in HostTheme.In(surface).Inline(_ => IO.lift(() => surface.Invalidate()), owner)
        select surface;

    // --- [FRAMES]
    public static IO<IO<Unit>> Animated<TState>(Surface<TState> surface, Func<Duration, TState, Option<TState>> step, IPlugInLifetime owner) where TState : notnull =>
        from running in IO.lift(static () => Ref(Option<Func<bool, IO<Unit>>>.None))
        from linked in ViewOps.WhileLoaded(surface,
            from link in HostPlatform.Current.FrameLink(surface, now => IO.lift(() => step(now, surface.State)).Bind(next => next.Match(Some: surface.Show, None: () => Running(running, active: false))), owner)
            from held in atomicIO(() => running.Value = Some(link))
            from cleared in Products.Released(() => atomic(() => running.Value = None))
            select unit,
            owner)
        select Running(running, active: true);

    private static IO<Unit> Running(Ref<Option<Func<bool, IO<Unit>>>> running, bool active) =>
        running.ValueIO.Bind(held => held.Match(Some: run => run(active), None: static () => IO.pure(unit)));
}
