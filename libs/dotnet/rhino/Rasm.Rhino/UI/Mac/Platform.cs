using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using AppKit;
using CoreAnimation;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Foundation;
using Rasm.Rhino.UI.Mac;

[assembly: ExportInitializer(typeof(AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------

[SupportedOSPlatform("macos27.0")]
internal sealed class AppKitPlatform : IHostPlatform {

    // --- [CHANGES]

    public HostEvent<Unit> Changed { get; } =
        Subscriptions.Merged(Observed(NSColor.Notifications.ObserveSystemColorsChanged), Observed(NSApplication.Notifications.ObserveDidChangeScreenParameters));

    private static HostEvent<Unit> Observed(Func<EventHandler<NSNotificationEventArgs>, NSObject> observe, [CallerArgumentExpression(nameof(observe))] string label = "") =>
        new(label, deliver => use(() => observe((_, _) => Session.Start(deliver(unit), label))).Map(unit));

    // --- [FONTS]

    private readonly AtomHashMap<(float Size, bool Bold), Font> faces = AtomHashMap<(float Size, bool Bold), Font>();

    public IO<Unit> Digits(Control control) =>
        IO.lift(() => Optional(control as CommonControl).Iter(common => common.Font = faces.FindOrAdd((common.Font.Size, common.Font.Bold), () => {
            Font.IHandler face = Platform.Instance.Create<Font.IHandler>();
            ((WidgetHandler<NSFont, Font>)face).Control = Conversions.Required(NSFont.MonospacedDigitSystemFontOfSize(common.Font.Size, common.Font.Bold ? NSFontWeight.Bold : NSFontWeight.Regular)).ThrowIfFail();
            return new Font(face);
        })));

    // --- [FRAMES]

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, bool> tick) =>
        from host in IO.lift(() => Conversions.Required(view.ControlObject as NSView))
        from relay in use(() => new NSMenuItem("", (sender, _) => Session.Run(IO.lift(() => Conversions.Required(sender as CADisplayLink).Map(ticked => ticked.Paused = !tick(Duration.FromSeconds(ticked.TargetTimestamp)))))))
        from link in use(IO.lift(() => (Conversions.Required(relay.Target), Conversions.Required(relay.Action)).Apply((target, action) => host.GetDisplayLink(target, action)).As()), static held => Products.Each(Seq(IO.lift(held.Invalidate), IO.lift(held.Dispose))).Map(unit))
        from scheduled in IO.lift(() => link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common))
        select fun((bool running) => IO.lift(() => { link.Paused = !running; }));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

[SupportedOSPlatform("macos27.0")]
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) => platform.Add<IHostPlatform>(static () => new AppKitPlatform());
}
