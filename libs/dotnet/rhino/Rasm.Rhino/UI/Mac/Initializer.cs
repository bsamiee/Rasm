using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Foundation;
using ObjCRuntime;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.Viewport;

[assembly: ExportInitializer(typeof(Rasm.Rhino.UI.Mac.AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WorkspaceTheme : IThemeHandler {
    // --- [ACCESSIBILITY]
    public DisplayOptions Accessibility {
        get {
            NSWorkspace workspace = NSWorkspace.SharedWorkspace;
            return new(
                ReduceMotion: workspace.AccessibilityDisplayShouldReduceMotion,
                IncreaseContrast: workspace.AccessibilityDisplayShouldIncreaseContrast,
                DifferentiateWithoutColor: workspace.AccessibilityDisplayShouldDifferentiateWithoutColor,
                ReduceTransparency: workspace.AccessibilityDisplayShouldReduceTransparency,
                InvertColors: workspace.AccessibilityDisplayShouldInvertColors);
        }
    }

    public HostEvent<EventArgs> Changed { get; } =
        new(typeof(NSWorkspace), nameof(NSWorkspace.DisplayOptionsDidChangeNotification), static (deliver, site, retain) =>
            DisposalOps.Acquire(owned =>
                from callback in IO.pure(Callbacks.Handler<NSNotificationEventArgs>(_ => deliver(EventArgs.Empty), site))
                from accessibility in DisposalOps.Acquire(IO.lift(() => NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                    NSWorkspace.DisplayOptionsDidChangeNotification, notification => callback(notification.Object, new NSNotificationEventArgs(notification)))), owned)
                from displays in DisposalOps.Acquire(IO.lift(() => NSApplication.Notifications.ObserveDidChangeScreenParameters(callback)), owned)
                select unit, retain));

    // --- [FONTS]
    private readonly AtomHashMap<(float Size, bool Bold), Font> faces = AtomHashMap<(float Size, bool Bold), Font>();

    public Font Digits(Font font) =>
        faces.FindOrAdd((font.Size, font.Bold), () => new Font(new FontHandler(
            NSFont.MonospacedDigitSystemFontOfSize(font.Size, font.Bold ? NSFontWeight.Bold : NSFontWeight.Regular))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DisplayLinks {
    // --- [DELIVERY]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Frame(nint view, nint selector, nint link);

    private static readonly Selector Tick = new($"{typeof(DisplayLinks).Assembly.GetName().Name}.frame:");
    private static readonly AtomHashMap<nint, Fin<Unit>> Targets = AtomHashMap<nint, Fin<Unit>>();
    private static readonly AtomHashMap<nint, (Sink<FrameTick> Ticks, CallbackSite Site)> Links = AtomHashMap<nint, (Sink<FrameTick> Ticks, CallbackSite Site)>();
    private static readonly Frame Deliver = static (_, _, link) => Links.Find(link).Iter(static held => Callbacks.Run(held.Ticks.Post(FrameTick.Next), held.Site));

    // --- [SOURCES]
    internal static Func<Sink<FrameTick>, Action<IDisposable>, IO<Unit>> Of(Control view, IPlugInSink sink) =>
        (ticks, retain) => {
            Action<CADisplayLink> release = static held => DisposalOps.Release<IDisposable>(held,
                new Disposal<CADisplayLink>(held, static link => _ = Links.Remove(link.Handle)),
                new Disposal<CADisplayLink>(held, static link => link.Invalidate())).Run();
            return from host in IO.lift(() => Refused.UnlessFound(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
                   from added in IO.lift(() => {
                       nint target = Class.GetHandle(host.GetType());
                       return Targets.FindOrAdd(target, () => Refused.Unless(Eto.Mac.ObjCExtensions.AddMethod(target, Tick.Handle, Deliver, "v@:@"), nameof(Eto.Mac.ObjCExtensions.AddMethod)));
                   })
                   from attached in NativeControls.Lifetime(view, owned =>
                       DisposalOps.Acquire(IO.lift(() => host.GetDisplayLink(host, Tick)), held => IO.lift(() => release(held)), held => {
                           _ = Links.Add(held.Handle, (ticks, new CallbackSite(sink, typeof(CADisplayLink), nameof(NSView.GetDisplayLink))));
                           held.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
                           owned(new Disposal<CADisplayLink>(held, release));
                       }).Map(unit),
                       new CallbackSite(sink, typeof(CADisplayLink), nameof(NSView.GetDisplayLink)), retain)
                   select attached;
        };
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) {
        platform.Add<FrameLink>(static () => DisplayLinks.Of);
        platform.Add<IThemeHandler>(static () => new WorkspaceTheme());
        platform.Add<PointerTravel>(static () => static () => Optional(NSApplication.SharedApplication.CurrentEvent).Map(static current => (float)current.DeltaX).IfNone(0f));
        platform.Add<Render.UI.FrameRaster>(static () => DrawnRasters.Raster);
        platform.Add<Render.UI.DisplayHeadroom>(static () => DrawnRasters.Headroom);
        Style.Add<DrawableHandler>(ComponentControl.HandlerStyle, DrawnViews.Attach);
        Style.Add<TextBoxHandler>(NumberField.HandlerStyle, NativeControls.Field);
        Style.Add<RowGrid>(RowGrid.HandlerStyle, NativeControls.Rows);
    }
}
