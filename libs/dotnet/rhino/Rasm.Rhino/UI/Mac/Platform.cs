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
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Mac;

[assembly: ExportInitializer(typeof(AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class FrameTarget(Func<Duration, IO<Unit>> tick, IPlugInLifetime owner) : NSObject {
    private const string Frame = "frame:";

    internal static readonly Selector Ticked = new(Frame);

    [Export(Frame)]
    public void Tick(CADisplayLink link) => Callbacks.Run(owner, tick(Duration.FromSeconds(link.TargetTimestamp)));
}

internal sealed class AppKitPlatform : IHostPlatform {
    // --- [CHANGE]
    public HostEvent<EventArgs> Changed { get; } =
        Subscriptions.Merged(
                Observed(NSColor.Notifications.ObserveSystemColorsChanged),
                Observed(NSApplication.Notifications.ObserveDidChangeScreenParameters))
            .Choose(static _ => Some(EventArgs.Empty));

    private static HostEvent<NSNotificationEventArgs> Observed(Func<EventHandler<NSNotificationEventArgs>, NSObject> observe, [CallerArgumentExpression(nameof(observe))] string label = "") =>
        new(label, (deliver, owner) => use(() => observe((_, args) => owner.Start(deliver(args), label))).Map(unit));

    // --- [FONTS]
    private readonly AtomHashMap<(float Size, bool Bold), Font> faces = AtomHashMap<(float Size, bool Bold), Font>();

    public Font Digits(Font font) =>
        faces.FindOrAdd((font.Size, font.Bold), () => new Font(new FontHandler(
            NSFont.MonospacedDigitSystemFontOfSize(font.Size, font.Bold ? NSFontWeight.Bold : NSFontWeight.Regular))));

    // --- [FRAMES]
    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInLifetime owner) =>
        from host in IO.lift(() => Conversions.Found(view.GetContainerView()))
        from target in use(() => new FrameTarget(tick, owner))
        from link in use(IO.lift(() => host.GetDisplayLink(target, FrameTarget.Ticked)), static held => Callbacks.Each(Seq(IO.lift(held.Invalidate), IO.lift(held.Dispose))).Map(unit))
        from scheduled in IO.lift(() => link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common))
        select fun((bool running) => IO.lift(() => { link.Paused = !running; }));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) => platform.Add<IHostPlatform>(static () => new AppKitPlatform());
}
