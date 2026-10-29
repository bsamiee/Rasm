using System.Windows.Media;
using Eto;
using Eto.Forms;
using Microsoft.Win32;
using Rasm.Rhino.UI.Windows;

[assembly: ExportInitializer(typeof(WpfInitializer), PlatformID = "Wpf")]

namespace Rasm.Rhino.UI.Windows;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WpfPlatform : IHostPlatform {
    // --- [CHANGE]
    public HostEvent<Unit> Changed { get; } =
        Subscriptions.Merged(
            Subscriptions.On<UserPreferenceChangedEventHandler, UserPreferenceChangedEventArgs>(
                    static handler => SystemEvents.UserPreferenceChanged += handler, static handler => SystemEvents.UserPreferenceChanged -= handler, static handler => handler.Invoke)
                .Choose(static _ => Some(unit)),
            Subscriptions.On<EventHandler, EventArgs>(
                    static handler => SystemEvents.DisplaySettingsChanged += handler, static handler => SystemEvents.DisplaySettingsChanged -= handler, static handler => handler.Invoke)
                .Choose(static _ => Some(unit)));

    // --- [ICONS]
    public int DropDownIconSize => 16;

    public int RenderTabIconSize => 16;

    // --- [FONTS]
    public IO<Unit> Digits(Control control) =>
        IO.lift(() => Optional(control.ControlObject as System.Windows.DependencyObject)
            .Iter(static native => System.Windows.Documents.Typography.SetNumeralAlignment(native, System.Windows.FontNumeralAlignment.Tabular)));

    // --- [FRAMES]
    private static readonly Action<EventHandler> Attach = static handler => CompositionTarget.Rendering += handler;
    private static readonly Action<EventHandler> Detach = static handler => CompositionTarget.Rendering -= handler;

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, bool> tick) =>
        from link in IO.lift(() => {
            bool running = false;
            void Rendered(object? sender, EventArgs args) {
                if (!tick(Duration.FromTimeSpan(((RenderingEventArgs)args).RenderingTime))) _ = Run(next: false);
            }
            Unit Run(bool next) {
                if (next == running) return unit;
                running = next;
                (next ? Attach : Detach)(Rendered);
                return unit;
            }
            return (Func<bool, Unit>)Run;
        })
        from stopped in Products.Released(() => link(false))
        from started in IO.lift(() => link(true))
        select fun((bool next) => IO.lift(() => link(next)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class WpfInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) => platform.Add<IHostPlatform>(static () => new WpfPlatform());
}
