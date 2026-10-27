using System.Drawing;
using System.Reflection;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.PlugIns;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IconResource(Assembly Assembly, string Id);

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PlugInImageAttribute(string resource) : Attribute {
    public string Resource { get; } = resource;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconSlot {
    public static readonly IconSlot PanelTab = new(static () => IO.lift(static () => Panels.IconSize.Width));
    public static readonly IconSlot PanelButton = new(static () => Accessors.Scoped(static () => Accessors.Get(Accessors.PrivatePanelButtonSize(Accessors.ToolbarButtons))));
    public static readonly IconSlot Small = new(static () => IO.pure(16));
    public static readonly IconSlot DropDownItem = new(static () => IO.pure(HostUtils.RunningOnOSX ? 14 : 16));
    public static readonly IconSlot Banner = new(static () => IO.pure(44));
    public static readonly IconSlot SectionHeader = new(static () => IO.pure(20));
    public static readonly IconSlot RenderTab = new(static () => IO.pure(HostUtils.RunningOnOSX ? 20 : 16));

    public IO<Size> Pixels => Extent().Bind(static size => Device(new Eto.Drawing.Size(size, size)));

    [UseDelegateFromConstructor]
    public partial IO<int> Extent();

    internal static IO<float> Scale => IO.lift(static () => Eto.Forms.Screen.Screens.Max(static screen => screen.LogicalPixelSize));

    internal static IO<Size> Device(Eto.Drawing.Size logical) =>
        Scale.Map(scale => Eto.Drawing.Size.Ceiling(logical * scale)).Map(static device => new Size(device.Width, device.Height));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Icons {
    // --- [EMBEDDED]
    public static IO<Option<IconResource>> PlugInImage(IPlugInLifetime plugIn) =>
        IO.lift(() => plugIn.GetType().Assembly switch {
            var assembly => Optional(assembly.GetCustomAttribute<PlugInImageAttribute>()).Map(image => new IconResource(assembly, image.Resource)),
        });

    // --- [ETO]
    public static IO<Eto.Drawing.Icon> Frames(IconResource icon, Eto.Drawing.Size logical) =>
        IO.lift(() => Conversions.Found(icon.Assembly.EtoAssets().Icons.TryGet(icon.Id, logical), call: icon.Id));

    public static IO<Eto.Drawing.Icon> Frames(IconResource icon, IconSlot slot) =>
        slot.Extent().Bind(size => Frames(icon, new Eto.Drawing.Size(size, size)));

    public static IO<Unit> Themed(IPlugInLifetime owner, IconResource icon, IconSlot slot, HostEvent<Unit> changes, Action<Eto.Drawing.Icon> write) =>
        Frames(icon, slot).Bind(frames => IO.lift(() => write(frames))) switch {
            var written => written.Bind(_ => changes.Inline(_ => written, owner)),
        };

    // --- [DRAWING]
    public static IO<Bitmap> Raster(IconResource icon, IconSlot slot) => slot.Pixels.Bind(pixels => Raster(icon, pixels));

    public static IO<Bitmap> Raster(IconResource icon, Size pixels) =>
        IO.lift(() => Conversions.Found(icon.Assembly.SystemDrawingAssets().Bitmaps.TryGet(icon.Id, pixels), call: icon.Id));

    public static IO<Icon> DrawingIcon(IconResource icon, Size pixels) =>
        IO.lift(() => Conversions.Found(icon.Assembly.SystemDrawingAssets().Icons.TryGet(icon.Id, pixels), call: icon.Id));
}
