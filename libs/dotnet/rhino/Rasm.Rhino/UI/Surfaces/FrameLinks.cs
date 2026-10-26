using Eto;
using Eto.Forms;

namespace Rasm.Rhino.UI.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInSink sink, Action<IDisposable> retain);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameLinks {
    // --- [SOURCES]
    public static IO<Func<bool, IO<Unit>>> Linked(Control view, Func<Duration, IO<Unit>> tick, IPlugInSink sink, Action<IDisposable> retain) =>
        IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<FrameLink>(), nameof(FrameLink))).Bind(link => link()(view, tick, sink, retain));
}
