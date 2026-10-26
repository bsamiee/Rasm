using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
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
using Rasm.Rhino.UI.Surfaces;
using Rasm.Rhino.Viewport.Printing;

[assembly: ExportInitializer(typeof(Rasm.Rhino.UI.Mac.AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WorkspaceTheme : IThemeHandler {
    // --- [CONDITIONS]
    private static readonly Seq<(Func<NSWorkspace, bool> Should, Rasm.UI.Styling.Condition Condition)> Displays = [
        (static workspace => workspace.AccessibilityDisplayShouldReduceMotion, Rasm.UI.Styling.Condition.ReduceMotion),
        (static workspace => workspace.AccessibilityDisplayShouldIncreaseContrast, Rasm.UI.Styling.Condition.IncreaseContrast),
        (static workspace => workspace.AccessibilityDisplayShouldDifferentiateWithoutColor, Rasm.UI.Styling.Condition.DifferentiateWithoutColor),
        (static workspace => workspace.AccessibilityDisplayShouldReduceTransparency, Rasm.UI.Styling.Condition.ReduceTransparency),
    ];

    public LanguageExt.HashSet<Rasm.UI.Styling.Condition> Conditions =>
        toHashSet(Displays.Filter(static row => row.Should(NSWorkspace.SharedWorkspace)).Map(static row => row.Condition));

    // --- [INKS]
    public Color Faulted => Eto.Mac.MacConversions.ToEto(NSColor.SystemRed);

    public Color Warning => Eto.Mac.MacConversions.ToEto(NSColor.SystemOrange);

    // --- [CHANGE]
    public HostEvent<EventArgs> Changed { get; } =
        new(typeof(NSWorkspace), nameof(NSWorkspace.DisplayOptionsDidChangeNotification), static (deliver, site, retain) =>
            DisposalOps.Acquire(owned =>
                from callback in IO.pure(Callbacks.Handler<NSNotificationEventArgs>(_ => deliver(EventArgs.Empty), site))
                from accessibility in DisposalOps.Acquire(IO.lift(() => NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                    NSWorkspace.DisplayOptionsDidChangeNotification, notification => callback(notification.Object, new NSNotificationEventArgs(notification)))), owned)
                from colors in DisposalOps.Acquire(IO.lift(() => NSColor.Notifications.ObserveSystemColorsChanged(callback)), owned)
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
    private static readonly AtomHashMap<nint, (CADisplayLink Link, Sink<Duration> Ticks, CallbackSite Site)> Links = AtomHashMap<nint, (CADisplayLink Link, Sink<Duration> Ticks, CallbackSite Site)>();

    private static Frame Deliver(CallbackSite site) =>
        (_, _, link) => Callbacks.Run(IO.lift(() => Links.Find(link).Iter(static held => Callbacks.Run(held.Ticks.Post(Duration.FromSeconds(held.Link.TargetTimestamp)), held.Site))), site);

    // --- [SOURCES]
    internal static Func<Sink<Duration>, Action<IDisposable>, IO<Func<bool, IO<Unit>>>> Of(Control view, IPlugInSink sink) =>
        (ticks, retain) =>
            from host in IO.lift(() => Refused.UnlessFound(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
            let site = new CallbackSite(sink, typeof(CADisplayLink), nameof(NSView.GetDisplayLink))
            from added in IO.lift(() => {
                nint target = Class.GetHandle(host.GetType());
                return Targets.FindOrAdd(target, () => Refused.Unless(Eto.Mac.ObjCExtensions.AddMethod(target, Tick.Handle, Deliver(site), "v@:@"), nameof(Eto.Mac.ObjCExtensions.AddMethod)));
            })
            from held in IO.lift(static () => Atom(Option<CADisplayLink>.None))
            from attached in NativeControls.Lifetime(view, owned =>
                from link in DisposalOps.Acquire(IO.lift(() => host.GetDisplayLink(host, Tick)), shown =>
                    Callbacks.Each(Seq(IO.lift(shown.Invalidate), IO.lift(() => { _ = Links.Remove(shown.Handle); }), held.SwapIO(static _ => None).Map(unit), IO.lift(shown.Dispose))).Map(unit), owned)
                from linked in IO.lift(() => {
                    _ = Links.Add(link.Handle, (link, ticks, site));
                    _ = held.Swap(_ => Some(link));
                    link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
                })
                select unit,
                site, retain)
            select fun((bool running) => IO.lift(() => held.Value.Iter(link => link.Paused = !running)));
}

internal static class PdfPages {
    // --- [PRINT]
    internal static Fin<Unit> Print(Graphics graphics, SkiaSharp.SKData page) =>
        Conversions.Cast<CGContext>(graphics.ControlObject).Bind(context => {
            using CGDataProvider provider = new(page.ToArray());
            using CGPDFDocument document = new(provider);
            return Refused.UnlessFound(document.GetPage(1), nameof(CGPDFDocument.GetPage)).Map(sheet => {
                using (sheet) {
                    context.SaveState();
                    context.TranslateCTM(0, sheet.GetBoxRect(CGPDFBox.Media).Height);
                    context.ScaleCTM(1, -1);
                    context.DrawPDFPage(sheet);
                    context.RestoreState();
                }
                return unit;
            });
        });
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) {
        platform.Add<FrameLink>(static () => DisplayLinks.Of);
        platform.Add<PdfPagePrinter>(static () => PdfPages.Print);
        platform.Add<IThemeHandler>(static () => new WorkspaceTheme());
        platform.Add<PointerTravel>(static () => static (_, _) => Refused.UnlessFound(NSApplication.SharedApplication.CurrentEvent, nameof(NSApplication.CurrentEvent)).Map(static current => (float)current.DeltaX));
        platform.Add<Render.UI.DisplayHeadroom>(static () => DrawnRasters.Headroom);
        platform.Add<ExtendedPresent>(static () => DrawnRasters.Present);
        platform.Add<BitmapOrder>(static () => static () => SkiaSharp.SKColorType.Rgba8888);
        platform.Add<EventTime>(static () => static () => Refused.UnlessFound(NSApplication.SharedApplication.CurrentEvent, nameof(NSApplication.CurrentEvent)).Map(static current => Duration.FromSeconds(current.Timestamp)));
        platform.Add<EventClicks>(static () => static () => Refused.UnlessFound(NSApplication.SharedApplication.CurrentEvent, nameof(NSApplication.CurrentEvent)).Map(static current => (int)current.ClickCount));
        platform.Add<ClosedHand>(static () => static () => new Cursor(new CursorHandler(NSCursor.ClosedHandCursor)));
        platform.Add<SlotSeated>(static () => NativeControls.Seated);
        platform.Add<NativeScene>(static () => DrawnViews.Projection);
        platform.Add<OpaqueTargets>(static () => Metal.Targets);
        Style.Add<DrawableHandler>(SceneView.HandlerStyle, DrawnViews.Scene);
        Style.Add<DrawableHandler>(ComponentControl.HandlerStyle, DrawnViews.Attach);
        Style.Add<TextBoxHandler>(NumberField.HandlerStyle, NativeControls.Field);
        Style.Add<RowGrid>(RowGrid.HandlerStyle, NativeControls.Rows);
    }
}
