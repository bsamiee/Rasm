using System.Windows.Media;
using Eto;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Surfaces;

[assembly: ExportInitializer(typeof(Rasm.Rhino.UI.WpfInitializer), PlatformID = "Wpf")]

namespace Rasm.Rhino.UI;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class WpfInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) {
        platform.Add<FrameLink>(static () => static (_, tick, sink, retain) =>
            from running in IO.lift(static () => Atom(true))
            let site = new CallbackSite(sink, typeof(CompositionTarget), nameof(CompositionTarget.Rendering))
            from attached in Subscriptions.Attach<EventHandler>(
                static handler => CompositionTarget.Rendering += handler,
                static handler => CompositionTarget.Rendering -= handler,
                (_, args) => Callbacks.Run(
                    when(running.Value, IO.lift(() => Conversions.Cast<RenderingEventArgs>(args)).Bind(rendering => tick(Duration.FromTimeSpan(rendering.RenderingTime)))).As(),
                    site),
                retain)
            select fun((bool shown) => running.SwapIO(_ => shown).Map(static _ => unit)));
        platform.Add<PointerTravel>(static () => static (at, last) => at.X - last.X);
        platform.Add<BitmapOrder>(static () => static () => SkiaSharp.SKColorType.Bgra8888);
        platform.Add<OpaqueTargets>(static () => static (_, _) => IO.pure<OpaqueTarget>(SceneView.Bitmapped));
    }
}
