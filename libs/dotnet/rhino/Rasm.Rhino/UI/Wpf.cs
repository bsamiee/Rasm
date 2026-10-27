using System.Windows.Media;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Microsoft.Win32;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;

[assembly: ExportInitializer(typeof(WpfInitializer), PlatformID = "Wpf")]

namespace Rasm.Rhino.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WpfPlatform : IHostPlatform {
    // --- [CHANGE]
    public HostEvent<EventArgs> Changed { get; } =
        Subscriptions.Host<UserPreferenceChangedEventHandler, UserPreferenceChangedEventArgs>(
                static handler => SystemEvents.UserPreferenceChanged += handler, static handler => SystemEvents.UserPreferenceChanged -= handler, static handler => handler.Invoke)
            .Choose(static _ => Some(EventArgs.Empty));

    // --- [FONTS]
    public Font Digits(Font font) => font;

    // --- [FRAMES]
    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInLifetime owner) =>
        from running in IO.lift(static () => Ref(value: true))
        from attached in Subscriptions.During<EventHandler, EventArgs>(static h => CompositionTarget.Rendering += h, static h => CompositionTarget.Rendering -= h, static handler => handler.Invoke)
            .Inline(args => when(running.Value, IO.lift(() => Conversions.Cast<RenderingEventArgs>(args)).Bind(rendering => tick(Duration.FromTimeSpan(rendering.RenderingTime)))).As(), owner)
        select fun((bool shown) => atomicIO(() => running.Value = shown).Map(static _ => unit));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class WpfInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) => platform.Add<IHostPlatform>(static () => new WpfPlatform());
}
