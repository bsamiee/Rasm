using System.Runtime.CompilerServices;
using AppKit;
using CoreAnimation;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using Eto.Mac.Forms;
using Foundation;
using ObjCRuntime;
using Rasm.Rhino.UI.Mac;

[assembly: ExportInitializer(typeof(AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class FrameTarget(Func<Duration, bool> tick) : NSObject {
    private const string Frame = "frame:";

    internal static readonly Selector Ticked = new(Frame);

    [Export(Frame)]
    public void Tick(CADisplayLink link) => Session.Run(IO.lift(() => link.Paused = !tick(Duration.FromSeconds(link.TargetTimestamp))));
}

internal sealed class AppKitPlatform : IHostPlatform {
    // --- [CHANGE]
    public HostEvent<Unit> Changed { get; } =
        Subscriptions.Merged(Observed(NSColor.Notifications.ObserveSystemColorsChanged), Observed(NSApplication.Notifications.ObserveDidChangeScreenParameters));

    private static HostEvent<Unit> Observed(Func<EventHandler<NSNotificationEventArgs>, NSObject> observe, [CallerArgumentExpression(nameof(observe))] string label = "") =>
        new(label, deliver => use(() => observe((_, _) => Session.Start(deliver(unit), label))).Map(unit));

    // --- [ICONS]
    public int DropDownIconSize => 14;

    public int RenderTabIconSize => 20;

    // --- [FONTS]
    private readonly AtomHashMap<(float Size, bool Bold), Font> faces = AtomHashMap<(float Size, bool Bold), Font>();

    public IO<Unit> Digits(Control control) =>
        IO.lift(() => Optional(control as CommonControl).Iter(common => common.Font = faces.FindOrAdd((common.Font.Size, common.Font.Bold), () => new Font(new FontHandler(
            NSFont.MonospacedDigitSystemFontOfSize(common.Font.Size, common.Font.Bold ? NSFontWeight.Bold : NSFontWeight.Regular))))));

    // --- [FRAMES]
    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, bool> tick) =>
        from host in IO.lift(() => Conversions.Required(view.GetContainerView()))
        from target in use(() => new FrameTarget(tick))
        from link in use(IO.lift(() => host.GetDisplayLink(target, FrameTarget.Ticked)), static held => Products.Each(Seq(IO.lift(held.Invalidate), IO.lift(held.Dispose))).Map(unit))
        from scheduled in IO.lift(() => link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common))
        select fun((bool running) => IO.lift(() => { link.Paused = !running; }));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) => platform.Add<IHostPlatform>(static () => new AppKitPlatform());
}
