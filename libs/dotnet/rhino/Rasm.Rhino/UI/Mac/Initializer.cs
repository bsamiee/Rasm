using AppKit;
using Eto;
using Eto.Forms;
using Eto.Mac.Forms;
using Eto.Mac.Forms.Controls;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;

[assembly: ExportInitializer(typeof(Rasm.Rhino.UI.Mac.AppKitInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.UI.Mac;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class AppKitInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) {
        platform.Add<FrameLink>(static () => DisplayLinks.Of);
        platform.Add<IThemeHandler>(static () => new WorkspaceTheme());
        platform.Add<PointerTravel>(static () => static (_, _) => Refused.UnlessFound(NSApplication.SharedApplication.CurrentEvent, nameof(NSApplication.CurrentEvent)).Map(static current => (float)current.DeltaX));
        platform.Add<ExtendedPresent>(static () => DrawnRasters.Present);
        platform.Add<BitmapOrder>(static () => static () => SkiaSharp.SKColorType.Rgba8888);
        platform.Add<EventTime>(static () => static () => Refused.UnlessFound(NSApplication.SharedApplication.CurrentEvent, nameof(NSApplication.CurrentEvent)).Map(static current => Duration.FromSeconds(current.Timestamp)));
        platform.Add<EventClicks>(static () => static () => Refused.UnlessFound(NSApplication.SharedApplication.CurrentEvent, nameof(NSApplication.CurrentEvent)).Map(static current => (int)current.ClickCount));
        platform.Add<ClosedHand>(static () => static () => new Cursor(new CursorHandler(NSCursor.ClosedHandCursor)));
        platform.Add<SlotSeated>(static () => NativeControls.Seated);
        platform.Add<NativeScene>(static () => DrawnViews.Projection);
        platform.Add<OpaqueTargets>(static () => Metal.Targets);
        Style.Add<DrawableHandler>(SceneView.HandlerStyle, DrawnViews.Scene);
        Style.Add<TextBoxHandler>(NumberField.HandlerStyle, NativeControls.Field);
    }
}
