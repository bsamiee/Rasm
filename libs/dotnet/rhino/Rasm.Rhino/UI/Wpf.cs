using System.Runtime.CompilerServices;
using System.Windows.Media;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using SkiaSharp;

[assembly: ExportInitializer(typeof(Rasm.RhinoCommon.UI.WpfInitializer), PlatformID = "Wpf")]

namespace Rasm.RhinoCommon.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WpfPlatform : IDrawnPlatform {
    public Fin<Duration> EventTime => Unheld();

    public Fin<int> EventClicks => Unheld();

    public Fin<Cursor> ClosedHand => Unheld();

    public Fin<float> PointerTravel(PointF at, PointF last) => at.X - last.X;

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInLifetime owner) =>
        from running in IO.lift(static () => Atom(true))
        from attached in Subscriptions.During<EventHandler, EventArgs>(static h => CompositionTarget.Rendering += h, static h => CompositionTarget.Rendering -= h, static handler => handler.Invoke)
            .Inline(args => when(running.Value, IO.lift(() => Conversions.Cast<RenderingEventArgs>(args)).Bind(rendering => tick(Duration.FromTimeSpan(rendering.RenderingTime)))).As(), owner)
        select fun((bool shown) => running.SwapIO(_ => shown).Map(static _ => unit));

    public IO<Unit> SlotSeated(Control slot, Option<string> label) => IO.pure(unit);

    public IO<Func<Mount, Output, IO<Unit>>> NativeScene(SceneView view) => IO.fail<Func<Mount, Output, IO<Unit>>>(Unheld());

    public IO<OpaqueTarget> OpaqueTargets(IPlugInLifetime owner) => IO.pure(SceneView.Bitmapped);

    public Fin<Unit> ExtendedPresent(Control view, Graphics graphics, SKImage frame, RectangleF destination) => Unheld();

    private static RhinoFault Unheld([CallerMemberName] string member = "") => new(member, RhinoReason.Unheld);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class WpfInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) => platform.Add<IDrawnPlatform>(static () => new WpfPlatform());
}
