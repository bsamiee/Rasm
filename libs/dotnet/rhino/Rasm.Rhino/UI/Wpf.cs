using System.Windows.Media;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using SkiaSharp;

[assembly: ExportInitializer(typeof(Rasm.Rhino.UI.WpfInitializer), PlatformID = "Wpf")]

namespace Rasm.Rhino.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WpfPlatform : IDrawnPlatform {
    public Fin<Duration> EventTime => new Refused(nameof(EventTime), Refused.Absent);

    public Fin<int> EventClicks => new Refused(nameof(EventClicks), Refused.Absent);

    public Fin<Cursor> ClosedHand => new Refused(nameof(ClosedHand), Refused.Absent);

    public Fin<float> PointerTravel(PointF at, PointF last) => at.X - last.X;

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInSink sink, Action<IDisposable> retain) =>
        from running in IO.lift(static () => Atom(true))
        let site = new CallbackSite(sink, typeof(CompositionTarget), nameof(CompositionTarget.Rendering))
        from attached in Subscriptions.Attach<EventHandler>(
            static handler => CompositionTarget.Rendering += handler,
            static handler => CompositionTarget.Rendering -= handler,
            (_, args) => Callbacks.Run(
                when(running.Value, IO.lift(() => Conversions.Cast<RenderingEventArgs>(args)).Bind(rendering => tick(Duration.FromTimeSpan(rendering.RenderingTime)))).As(),
                site),
            retain)
        select fun((bool shown) => running.SwapIO(_ => shown).Map(static _ => unit));

    public IO<Unit> SlotSeated(Control slot, Option<string> label) => IO.pure(unit);

    public IO<Func<Mount, Output, IO<Unit>>> NativeScene(SceneView view, Action<IDisposable> retain) => IO.fail<Func<Mount, Output, IO<Unit>>>(new Refused(nameof(NativeScene), Refused.Absent));

    public IO<OpaqueTarget> OpaqueTargets(IPlugInSink owner, Action<IDisposable> retain) => IO.pure(SceneView.Bitmapped);

    public Fin<Unit> ExtendedPresent(Control view, Graphics graphics, SKImage frame, RectangleF destination) => new Refused(nameof(ExtendedPresent), Refused.Absent);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class WpfInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) => platform.Add<IDrawnPlatform>(static () => new WpfPlatform());
}
