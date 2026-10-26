using Eto;
using Eto.Forms;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.Viewport;
using Rasm.UI.Styling;

namespace Rasm.Rhino.UI.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> FrameLink(Control view, IPlugInSink sink);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record FrameStep(IO<Unit> Step, CallbackSite Site) : Sink<FrameTick> {
    public override IO<Unit> Post(FrameTick value) => IO.lift(() => Callbacks.Started(Step, Site));

    public override IO<Unit> Complete() => IO.pure(unit);

    public override IO<Unit> Fail(Error Error) => IO.pure(unit);

    public override Sink<X> Comap<X>(Func<X, FrameTick> f) => Sink<X>.Void.Combine((X x) => (x, f(x)), this);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameClocks {
    // --- [SOURCES]
    public static Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Frames(Control view, IPlugInSink sink) =>
        (ticks, retain) => IO.lift(static () => Optional(Platform.Instance.Find<FrameLink>()))
            .Bind(found => found.Match(Some: link => link()(view, sink), None: static () => Motions.Idle)(ticks, retain));

    public static Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Paced(Control view, IPlugInSink sink) =>
        (ticks, retain) => IO.lift(static () => HostTheme.Conditions.Contains(Condition.ReduceMotion))
            .Bind(reduced => (reduced ? Motions.Final : Frames(view, sink))(ticks, retain));

    // --- [SAMPLING]
    public static IO<Unit> Sample(Control view, IO<Unit> step, CallbackSite site, Action<IDisposable> retain) =>
        Frames(view, site.Sink)(new FrameStep(step, site), retain);
}
