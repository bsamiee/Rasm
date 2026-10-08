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
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.Viewport;

[assembly: ExportInitializer(typeof(Rasm.Rhino.UI.macOS.AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.macOS;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WorkspaceTheme : IThemeHandler {
    // --- [ACCESSIBILITY]
    private readonly AtomHashMap<EventHandler, NSObject> observers = AtomHashMap<EventHandler, NSObject>();

    public DisplayOptions Accessibility =>
        NSWorkspace.SharedWorkspace switch {
            var workspace => new(
                ReduceMotion: workspace.AccessibilityDisplayShouldReduceMotion,
                IncreaseContrast: workspace.AccessibilityDisplayShouldIncreaseContrast,
                DifferentiateWithoutColor: workspace.AccessibilityDisplayShouldDifferentiateWithoutColor,
                ReduceTransparency: workspace.AccessibilityDisplayShouldReduceTransparency),
        };

    public event EventHandler AccessibilityChanged {
        add => observers.Add(value, NSWorkspace.Notifications.ObserveDisplayOptionsDidChange((_, _) => value(this, EventArgs.Empty)));
        remove => observers.Find(value).Iter(token => {
            NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(token);
            token.Dispose();
            _ = observers.Remove(value);
        });
    }

    // --- [FONTS]
    private readonly AtomHashMap<(float Size, bool Bold), Font> faces = AtomHashMap<(float Size, bool Bold), Font>();

    public Font Digits(Font font) =>
        faces.FindOrAdd((font.Size, font.Bold), () => new Font(new FontHandler(
            NSFont.MonospacedDigitSystemFontOfSize(font.Size, font.Bold ? NSFontWeight.Bold : NSFontWeight.Regular))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DisplayLinks {
    // --- [DELIVERY]
    private delegate void Frame(nint view, nint selector, nint link);

    private static readonly Selector Tick = new($"{typeof(DisplayLinks).Assembly.GetName().Name}.frame:");
    private static readonly AtomHashMap<nint, (Sink<FrameTick> Ticks, CallbackSite Site)> Links = AtomHashMap<nint, (Sink<FrameTick> Ticks, CallbackSite Site)>();
    private static readonly Frame Deliver = static (_, _, link) => Links.Find(link).Iter(static held => Callbacks.Answer(held.Ticks.Post(FrameTick.Next), static () => unit, held.Site));

    // --- [SOURCES]
    internal static Func<Sink<FrameTick>, IO<IDisposable>> Of(Control view, IPlugInSink sink) =>
        ticks =>
            from host in IO.lift(() => Missing.Unless(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
            from link in IO.lift(() => {
                _ = Eto.Mac.ObjCExtensions.AddMethod(host.Class, Tick.Handle, Deliver, "v@:@");
                CADisplayLink created = host.GetDisplayLink(host, Tick);
                _ = Links.Add(created.Handle, (ticks, new CallbackSite(sink, typeof(CADisplayLink), nameof(NSView.GetDisplayLink))));
                created.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
                return created;
            })
            select (IDisposable)new Disposal<CADisplayLink>(link, static held => {
                held.Invalidate();
                _ = Links.Remove(held.Handle);
                held.Dispose();
            });
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) {
        platform.Add<Func<Control, IPlugInSink, Func<Sink<FrameTick>, IO<IDisposable>>>>(static () => DisplayLinks.Of);
        platform.Add<IThemeHandler>(static () => new WorkspaceTheme());
        platform.Add<Func<IPlugInSink, Control, PixelFrame, Imaging.ColorManagement.Transfer, IO<Bitmap>>>(static () => static (_, view, frame, encoding) => DrawnViews.Raster(view, frame, encoding));
        platform.Add<Func<IPlugInSink, Control, IO<float>>>(static () => static (_, view) => DrawnViews.Headroom(view));
        platform.Add<Func<IPlugInSink, MouseEventArgs, PointF, float>>(static () => static (_, _, _) => Optional(NSApplication.SharedApplication.CurrentEvent).Map(static current => (float)current.DeltaX).IfNone(0f));
        Style.Add<DrawableHandler>(ComponentControl.HandlerStyle, DrawnViews.Attach);
        Style.Add<TextBoxHandler>(NumberField.HandlerStyle, NativeControls.Field);
        Style.Add<RowGrid>(RowGrid.HandlerStyle, NativeControls.Rows);
    }
}
