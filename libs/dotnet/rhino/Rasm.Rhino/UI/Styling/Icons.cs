using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.UI;
using Rasm.UI.Styling;
using Rhino.PlugIns;
using Rhino.Resources;
using Rhino.Runtime;
using SkiaSharp;

namespace Rasm.Rhino.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconSlot {
    public static readonly IconSlot PanelTab = new(static () => Accessors.Scoped(nameof(Accessors.TabIconSize), IO.lift(static () => Accessors.TabIconSize(owner: null))));
    public static readonly IconSlot PanelButton = new(static () => Accessors.Scoped(nameof(Accessors.Get), IO.lift(static () => Accessors.Get(Accessors.PrivatePanelButtonSize(Accessors.ToolbarButtons)))));
    public static readonly IconSlot Small = new(static () => IO.pure(16));
    public static readonly IconSlot DropDownItem = new(static () => IO.pure(HostUtils.RunningOnOSX ? 14 : 16));
    public static readonly IconSlot Banner = new(static () => IO.pure(44));
    public static readonly IconSlot SectionHeader = new(static () => IO.pure(20));
    public static readonly IconSlot RenderTab = new(static () => IO.pure(HostUtils.RunningOnOSX ? 20 : 16));

    public IO<Size> Pixels => Size().Bind(static size => Device(new Eto.Drawing.Size(size, size)));

    [UseDelegateFromConstructor]
    public partial IO<int> Size();

    internal static IO<float> Scale => IO.lift(static () => Eto.Forms.Screen.Screens.Max(static screen => screen.LogicalPixelSize));

    internal static IO<Size> Device(Eto.Drawing.Size logical) =>
        Scale.Map(scale => Eto.Drawing.Size.Ceiling(logical * scale)).Map(static device => new Size(device.Width, device.Height));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Icons {
    // --- [EMBEDDED]
    public static GlyphSource.Paired Embedded(Assembly assembly, string light) =>
        new(assembly, light, Optional(((IDictionary)ResourceDictionary(Assets.Get(assembly)))[light])
            .Bind(static file => Optional(GetOsKey(file, darkMode: true)))
            .IfNone(light));

    public static IO<Option<GlyphSource.Paired>> PlugInImage(IPlugInSink plugIn) =>
        IO.lift(() => plugIn.GetType().Assembly switch {
            var assembly => toSeq(assembly.GetCustomAttributes<PlugInDescriptionAttribute>())
                .Find(static description => description.DescriptionType is DescriptionType.Icon)
                .Map(description => Embedded(assembly, description.Value)),
        });

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ResourceDictionary")]
    [return: UnsafeAccessorType("System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[Rhino.Resources.ResourceFile, Rhino.UI]], System.Private.CoreLib")]
    private static extern object ResourceDictionary(Assets assets);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetOsKey")]
    private static extern string? GetOsKey([UnsafeAccessorType("Rhino.Resources.ResourceFile, Rhino.UI")] object file, bool darkMode);

    // --- [RASTER]
    private static readonly Lazy<Fin<Shipped>> Sheet = new(static () => Styles.Embedded.RunSafe());

    public static Fin<SKImage> Rastered(GlyphSource glyph, SKSizeI pixels) =>
        Sheet.Value.Bind(shipped => SceneHost.Rastered(
            shipped, HostTheme.Projected(compact: false), new Node.Glyph(glyph) { Role = Role.Label, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }, pixels));

    private static Fin<byte[]> Encoded(GlyphSource glyph, Size pixels) =>
        Rastered(glyph, new SKSizeI(pixels.Width, pixels.Height)).Bind(static image => {
            using (image) {
                return Refused.UnlessFound(image.Encode(SKEncodedImageFormat.Png, 100), nameof(SKImage.Encode)).Map(static encoded => {
                    using (encoded) {
                        return encoded.ToArray();
                    }
                });
            }
        });

    // --- [ETO]
    public static Eto.Drawing.Icon Frame(GlyphSource glyph, Eto.Drawing.Size logical, float scale) =>
        Eto.Drawing.Size.Ceiling(logical * scale) switch {
            var device => Encoded(glyph, new Size(device.Width, device.Height)).Map(png => new Eto.Drawing.Icon(scale, new Eto.Drawing.Bitmap(png))).ThrowIfFail(),
        };

    public static IO<Eto.Drawing.Icon> Frames(GlyphSource glyph, Eto.Drawing.Size logical) =>
        IconSlot.Scale.Bind(scale => IO.lift(() => Frame(glyph, logical, scale)));

    public static IO<Eto.Drawing.Icon> Frames(GlyphSource glyph, IconSlot slot) =>
        slot.Size().Bind(size => Frames(glyph, new Eto.Drawing.Size(size, size)));

    public static IO<Unit> Themed(IPlugInSink plugIn, GlyphSource glyph, IconSlot slot, HostEvent<Unit> changes, Action<Eto.Drawing.Icon> write, Action<IDisposable> retain) =>
        Frames(glyph, slot).Bind(icon => IO.lift(() => write(icon))) switch {
            var written => written.Bind(_ => changes.Inline(_ => written, plugIn, retain)),
        };

    // --- [DRAWING]
    public static IO<Bitmap> Raster(GlyphSource glyph, IconSlot slot) => slot.Pixels.Bind(pixels => Raster(glyph, pixels));

    public static IO<Bitmap> Raster(GlyphSource glyph, Size pixels) =>
        IO.lift(() => Encoded(glyph, pixels).Map(static png => {
            using MemoryStream stream = new(png);
            using Bitmap decoded = new(stream);
            return new Bitmap(decoded);
        }));

    public static IO<Icon> DrawingIcon(GlyphSource glyph, Size pixels) =>
        IO.lift(() => Encoded(glyph, pixels).Map(png => {
            using MemoryStream stream = new(png);
            return new Icon(stream, pixels);
        }));
}
