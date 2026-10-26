using AppKit;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using SkiaSharp;

[assembly: ExportInitializer(typeof(Rasm.Rhino.UI.Mac.AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class AppKitPlatform : IDrawnPlatform {
    private static Fin<NSEvent> Current => Refused.UnlessFound(NSApplication.SharedApplication.CurrentEvent, nameof(NSApplication.CurrentEvent));

    public Fin<Duration> EventTime => Current.Map(static current => Duration.FromSeconds(current.Timestamp));

    public Fin<int> EventClicks => Current.Map(static current => (int)current.ClickCount);

    public Fin<Cursor> ClosedHand => new Cursor(new CursorHandler(NSCursor.ClosedHandCursor));

    public Fin<float> PointerTravel(PointF at, PointF last) => Current.Map(static current => (float)current.DeltaX);

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInSink sink, Action<IDisposable> retain) => DisplayLinks.Of(view, tick, sink, retain);

    public IO<Unit> SlotSeated(Control slot, Option<string> label) => NativeControls.Seated(slot, label);

    public IO<Func<Mount, Output, IO<Unit>>> NativeScene(SceneView view, Action<IDisposable> retain) => DrawnViews.Projection(view, retain);

    public IO<OpaqueTarget> OpaqueTargets(IPlugInSink owner, Action<IDisposable> retain) => Metal.Targets(owner, retain);

    public Fin<Unit> ExtendedPresent(Control view, Graphics graphics, SKImage frame, RectangleF destination) => DrawnRasters.Present(view, graphics, frame, destination);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) {
        platform.Add<IDrawnPlatform>(static () => new AppKitPlatform());
        platform.Add<IThemeHandler>(static () => new WorkspaceTheme());
        Style.Add<DrawableHandler>(SceneView.HandlerStyle, DrawnViews.Scene);
        Style.Add<TextBoxHandler>(NumberBox.HandlerStyle, NativeControls.Field);
    }
}
