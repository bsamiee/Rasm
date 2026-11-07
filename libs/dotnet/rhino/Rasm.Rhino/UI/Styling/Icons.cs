using System.Drawing;
using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Persistence;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record IconResource(Assembly Assembly, string Id) {
    private const string PlugInMaster = "icon.svg";

    internal static IconResource Presets { get; } = new(typeof(IconResource).Assembly, "presets.svg");

    internal static IconResource Options { get; } = new(typeof(IconResource).Assembly, "options.svg");

    public static Option<IconResource> PlugIn(Assembly plugIn) =>
        Optional(plugIn.GetManifestResourceInfo(PlugInMaster)).Map(_ => new IconResource(plugIn, PlugInMaster));
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconSlot {
    public static readonly IconSlot PanelTab = new(IO.lift(static () => Panels.IconSize.Width), Some(IO.lift(static () => Panels.IconSizeInPixels)));
    public static readonly IconSlot PanelButton = new(Appearance.PanelButtonSize.Read, None);
    public static readonly IconSlot Small = new(IO.pure(16), None);
    public static readonly IconSlot DropDownItem = new(IO.lift(static () => HostUtils.RunningOnWindows ? 16 : 14), None);
    public static readonly IconSlot Banner = new(IO.pure(44), None);
    public static readonly IconSlot RenderTab = new(IO.lift(static () => HostUtils.RunningOnWindows ? 16 : 20), None);
    public static readonly IconSlot Page = new(IO.pure(128), None);
    public static readonly IconSlot Splash = new(IO.pure(128), None);
    public static readonly IconSlot Application = new(IO.pure(256), None);

    public IO<int> Extent { get; }

    private Option<IO<Size>> Native { get; }

    public IO<Size> Square => Extent.Map(static extent => new Size(extent, extent));

    public IO<Size> Pixels => Device(None);

    public IO<Size> Device(Option<Control> owner) =>
        Native.Filter(_ => owner.IsNone).IfNone(() =>
            from extent in Extent
            from scale in IO.lift(() => owner.Bind(static control => Optional(control.ParentWindow)).IfNone(static () => RhinoEtoApp.MainWindow).LogicalPixelSize)
            select Eto.Drawing.Size.Ceiling(new Eto.Drawing.Size(extent, extent) * scale) switch { var device => new Size(device.Width, device.Height) });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Icons {

    // --- [ETO]

    public static IO<Eto.Drawing.Icon> Frames(IconResource icon, Eto.Drawing.Size logical) =>
        IO.lift(() => Conversions.Required(icon.Assembly.EtoAssets().Icons.TryGet(icon.Id, logical), call: icon.Id));

    public static IO<Eto.Drawing.Icon> Frames(IconResource icon, IconSlot slot) =>
        slot.Extent.Bind(size => Frames(icon, new Eto.Drawing.Size(size, size)));

    public static IO<Unit> Themed(IconResource icon, IconSlot slot, Option<Control> owner, Action<Eto.Drawing.Icon> write) =>
        HostTheme.Applied(owner, Frames(icon, slot).Bind(frames => IO.lift(() => write(frames))));

    // --- [DRAWING]

    public static IO<Bitmap> Raster(IconResource icon, IconSlot slot, Option<Control> owner) => slot.Device(owner).Bind(pixels => Raster(icon, pixels));

    public static IO<Bitmap> Raster(IconResource icon, Size pixels) =>
        IO.lift(() => Conversions.Required(icon.Assembly.SystemDrawingAssets().Bitmaps.TryGet(icon.Id, pixels), call: icon.Id));

    public static IO<Icon> DrawingIcon(IconResource icon, IconSlot slot) => slot.Pixels.Bind(pixels => DrawingIcon(icon, pixels));

    public static IO<Icon> DrawingIcon(IconResource icon, Size pixels) =>
        IO.lift(() => Conversions.Required(icon.Assembly.SystemDrawingAssets().Icons.TryGet(icon.Id, pixels), call: icon.Id));
}
