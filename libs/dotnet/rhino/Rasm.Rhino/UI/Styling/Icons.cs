using System.Drawing;
using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Persistence;
using Rhino.Resources;
using Rhino.UI;

namespace Rasm.Rhino.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IconResource(Assembly Assembly, string Id) {
    internal static IconResource Presets { get; } = new(typeof(IconResource).Assembly, "presets.svg");

    internal static IconResource Options { get; } = new(typeof(IconResource).Assembly, "options.svg");
}

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PlugInImageAttribute(string resource) : Attribute {
    public string Resource { get; } = resource;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconSlot {
    public static readonly IconSlot PanelTab = new(static () => IO.lift(static () => Panels.IconSize.Width));
    public static readonly IconSlot PanelButton = new(static () => Appearance.PanelButtonSize.Read);
    public static readonly IconSlot Small = new(static () => IO.pure(16));
    public static readonly IconSlot DropDownItem = new(static () => IO.lift(static () => HostPlatform.Current.DropDownIconSize));
    public static readonly IconSlot Banner = new(static () => IO.pure(44));
    public static readonly IconSlot RenderTab = new(static () => IO.lift(static () => HostPlatform.Current.RenderTabIconSize));

    public IO<Size> Pixels => Device(None);

    [UseDelegateFromConstructor]
    public partial IO<int> Extent();

    public IO<Size> Device(Option<Control> owner) =>
        from extent in Extent()
        from scale in IO.lift(() => owner.Bind(static control => Optional(control.ParentWindow)).IfNone(static () => RhinoEtoApp.MainWindow).LogicalPixelSize)
        select Eto.Drawing.Size.Ceiling(new Eto.Drawing.Size(extent, extent) * scale) switch { var device => new Size(device.Width, device.Height) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Icons {
    // --- [EMBEDDED]
    public static IO<Option<IconResource>> PlugInImage() =>
        IO.lift(static () => Session.Assembly switch {
            var assembly => Optional(assembly.GetCustomAttribute<PlugInImageAttribute>()).Map(image => new IconResource(assembly, image.Resource)),
        });

    // --- [ETO]
    public static IO<Eto.Drawing.Icon> Frames(IconResource icon, Eto.Drawing.Size logical) =>
        IO.lift(() => Conversions.Required(icon.Assembly.EtoAssets().Icons.TryGet(icon.Id, logical), call: icon.Id));

    public static IO<Eto.Drawing.Icon> Frames(IconResource icon, IconSlot slot) =>
        slot.Extent().Bind(size => Frames(icon, new Eto.Drawing.Size(size, size)));

    public static IO<Unit> Themed(IconResource icon, IconSlot slot, Action<Eto.Drawing.Icon> write) =>
        HostTheme.Applied(Frames(icon, slot).Bind(frames => IO.lift(() => write(frames))));

    public static IO<Unit> Themed(IconResource icon, IconSlot slot, Control control, Action<Eto.Drawing.Icon> write) =>
        HostTheme.Applied(control, Frames(icon, slot).Bind(frames => IO.lift(() => write(frames))));

    // --- [DRAWING]
    public static IO<Bitmap> Raster(IconResource icon, IconSlot slot, Option<Control> owner) => slot.Device(owner).Bind(pixels => Raster(icon, pixels));

    public static IO<Bitmap> Raster(IconResource icon, Size pixels) =>
        IO.lift(() => Conversions.Required(icon.Assembly.SystemDrawingAssets().Bitmaps.TryGet(icon.Id, pixels), call: icon.Id));

    public static IO<Icon> DrawingIcon(IconResource icon, Size pixels) =>
        IO.lift(() => Conversions.Required(icon.Assembly.SystemDrawingAssets().Icons.TryGet(icon.Id, pixels), call: icon.Id));
}
