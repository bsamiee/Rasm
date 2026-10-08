using Eto;
using Eto.Forms;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.UI.Components;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record FrameStep(IO<Unit> Step, CallbackSite Site) : Sink<FrameTick> {
    public override IO<Unit> Post(FrameTick value) => IO.lift(() => Callbacks.Answer(Step, static () => unit, Site));

    public override IO<Unit> Complete() => IO.pure(unit);

    public override IO<Unit> Fail(Error Error) => IO.pure(unit);

    public override Sink<X> Comap<X>(Func<X, FrameTick> f) => Sink<X>.Void.Combine((X x) => (x, f(x)), this);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameClocks {
    // --- [SOURCES]
    public static Func<Sink<FrameTick>, IO<IDisposable>> Frames(Control view, IPlugInSink sink) =>
        ticks => IO.lift(static () => Optional(Platform.Instance.Find<Func<Control, IPlugInSink, Func<Sink<FrameTick>, IO<IDisposable>>>>()))
            .Bind(found => found.Match(Some: link => link()(view, sink), None: static () => Motions.Idle)(ticks));

    public static Func<Sink<FrameTick>, IO<IDisposable>> Paced(Control view, IPlugInSink sink) =>
        ticks => IO.lift(static () => HostTheme.Accessibility.ReduceMotion)
            .Bind(reduced => (reduced ? Motions.Final : Frames(view, sink))(ticks));

    // --- [SAMPLING]
    public static IO<IDisposable> Sample(Control view, IO<Unit> step, CallbackSite site) =>
        Frames(view, site.Sink)(new FrameStep(step, site));
}
