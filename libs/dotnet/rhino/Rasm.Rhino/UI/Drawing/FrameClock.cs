using Eto;
using Eto.Forms;

namespace Rasm.Rhino.UI.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate Func<Sink<Duration>, Action<IDisposable>, IO<Func<bool, IO<Unit>>>> FrameLink(Control view, IPlugInSink sink);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record FrameStep(IO<Unit> Step, CallbackSite Site) : Sink<Duration> {
    public override IO<Unit> Post(Duration value) => IO.lift(() => Callbacks.Started(Step, Site));

    public override IO<Unit> Complete() => IO.pure(unit);

    public override IO<Unit> Fail(Error Error) => IO.pure(unit);

    public override Sink<X> Comap<X>(Func<X, Duration> f) => Sink<X>.Void.Combine((X x) => (x, f(x)), this);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameClocks {
    // --- [SOURCES]
    public static Func<Sink<Duration>, Action<IDisposable>, IO<Unit>> Frames(Control view, IPlugInSink sink) =>
        (ticks, retain) => IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<FrameLink>(), nameof(FrameLink)))
            .Bind(link => link()(view, sink)(ticks, retain)).Map(static _ => unit);

    // --- [SAMPLING]
    public static IO<Unit> Sample(Control view, IO<Unit> step, CallbackSite site, Action<IDisposable> retain) =>
        Frames(view, site.Sink)(new FrameStep(step, site), retain);
}
