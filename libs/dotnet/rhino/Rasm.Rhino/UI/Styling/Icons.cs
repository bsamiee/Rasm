using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.UI.Surfaces;
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
    public static Fin<SKImage> Rastered(GlyphSource glyph, SKSizeI pixels) =>
        SceneRaster.Of(PanelRoles.Defaults, HostTheme.Projected(compact: false), new Node.Glyph(glyph) { Role = Role.Label, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }, pixels);

    private static Fin<Bitmap> Drawn(SKImage image) {
        using (image) {
            using Bitmap drawn = new(image.Width, image.Height, PixelFormat.Format32bppPArgb);
            BitmapData data = drawn.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            Fin<Unit> read;
            using (SKPixmap pixmap = new(new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul, HostPixels.Srgb), data.Scan0, data.Stride))
                read = Refused.Unless(image.ReadPixels(pixmap), nameof(SKImage.ReadPixels));
            drawn.UnlockBits(data);
            return read.Map(_ => new Bitmap(drawn));
        }
    }

    private static Fin<Eto.Drawing.Bitmap> Pixeled(SKImage image) {
        using (image) {
            return BitmapPixels.Allocated(new SKSizeI(image.Width, image.Height)).Bind(held => {
                Fin<Unit> read;
                using (held.Pixmap)
                    read = Refused.Unless(image.ReadPixels(held.Pixmap), nameof(SKImage.ReadPixels));
                held.Pixels.Data.Dispose();
                if (read.IsFail) held.Pixels.Bitmap.Dispose();
                return read.Map(_ => held.Pixels.Bitmap);
            });
        }
    }

    // --- [ETO]
    public static Eto.Drawing.Icon Frame(GlyphSource glyph, Eto.Drawing.Size logical, float scale) =>
        Eto.Drawing.Size.Ceiling(logical * scale) switch {
            var device => Rastered(glyph, new SKSizeI(device.Width, device.Height)).Bind(Pixeled).Map(bitmap => new Eto.Drawing.Icon(scale, bitmap)).ThrowIfFail(),
        };

    public static IO<Eto.Drawing.Icon> Frames(GlyphSource glyph, Eto.Drawing.Size logical) =>
        IconSlot.Scale.Bind(scale => IO.lift(() => Frame(glyph, logical, scale)));

    public static IO<Eto.Drawing.Icon> Frames(GlyphSource glyph, IconSlot slot) =>
        slot.Extent().Bind(size => Frames(glyph, new Eto.Drawing.Size(size, size)));

    public static IO<Unit> Themed(IPlugInSink plugIn, GlyphSource glyph, IconSlot slot, HostEvent<Unit> changes, Action<Eto.Drawing.Icon> write, Action<IDisposable> retain) =>
        Frames(glyph, slot).Bind(icon => IO.lift(() => write(icon))) switch {
            var written => written.Bind(_ => changes.Inline(_ => written, plugIn, retain)),
        };

    // --- [DRAWING]
    public static IO<Bitmap> Raster(GlyphSource glyph, IconSlot slot) => slot.Pixels.Bind(pixels => Raster(glyph, pixels));

    public static IO<Bitmap> Raster(GlyphSource glyph, Size pixels) => IO.lift(() => Rastered(glyph, new SKSizeI(pixels.Width, pixels.Height)).Bind(Drawn));

    public static IO<Icon> DrawingIcon(GlyphSource glyph, Size pixels) =>
        IO.lift(() => Rastered(glyph, new SKSizeI(pixels.Width, pixels.Height)).Bind(static image => {
            using (image)
                return Refused.UnlessFound(image.Encode(SKEncodedImageFormat.Png, 100), nameof(SKImage.Encode));
        }).Map(encoded => {
            using (encoded)
            using (Stream stream = encoded.AsStream())
                return new Icon(stream, pixels);
        }));
}
