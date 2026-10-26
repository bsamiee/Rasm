using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.UI;
using Rhino.PlugIns;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.UI;
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

    public IO<Size> Pixels => Size().Bind(static size => HostTheme.Device(new(size, size)));

    [UseDelegateFromConstructor]
    public partial IO<int> Size();
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

    // --- [ETO]
    public static Eto.Drawing.Icon Frame(GlyphSource glyph, Eto.Drawing.Size logical, float scale) =>
        glyph.Switch(
            (Logical: logical, Scale: scale),
            tinted: static (at, held) => ImageResources.CreateEtoIcon(Inked(held), at.Logical.Width, at.Logical.Height, adjustForDarkMode: false)
                ?? throw new InvalidOperationException($"{held.Resource} in {held.Assembly.GetName().Name} draws nothing"),
            paired: static (at, held) => Assets.Get(held.Assembly).Eto.Icons.Get(held.Light, at.Logical),
            picture: static (at, held) => Eto.Drawing.Size.Ceiling(at.Logical * at.Scale) switch {
                var device => new Eto.Drawing.Icon(at.Scale, new Eto.Drawing.Bitmap(Encoded(held, device.Width, device.Height))),
            });

    public static IO<Eto.Drawing.Icon> Frames(GlyphSource glyph, Eto.Drawing.Size logical) =>
        HostTheme.Scale.Bind(scale => IO.lift(() => Frame(glyph, logical, scale)));

    public static IO<Eto.Drawing.Icon> Frames(GlyphSource glyph, IconSlot slot) =>
        slot.Size().Bind(size => Frames(glyph, new Eto.Drawing.Size(size, size)));

    public static IO<Unit> Themed(IPlugInSink plugIn, GlyphSource glyph, IconSlot slot, HostEvent<Unit> changes, Action<Eto.Drawing.Icon> write, Action<IDisposable> retain) =>
        Frames(glyph, slot).Bind(icon => IO.lift(() => write(icon))) switch {
            var written => written.Bind(_ => changes.Inline(_ => written, plugIn, retain)),
        };

    // --- [DRAWING]
    public static IO<Bitmap> Raster(GlyphSource glyph, IconSlot slot) => slot.Pixels.Bind(pixels => Raster(glyph, pixels));

    public static IO<Bitmap> Raster(GlyphSource glyph, Size pixels) =>
        IO.lift(() => glyph.Switch(
            pixels,
            tinted: static (at, held) => DrawingUtilities.BitmapFromSvg(Inked(held), at.Width, at.Height)
                ?? throw new InvalidOperationException($"{held.Resource} in {held.Assembly.GetName().Name} draws nothing"),
            paired: static (at, held) => Assets.Get(held.Assembly).SystemDrawing.Bitmaps.Get(held.Light, at),
            picture: static (at, held) => Decoded(Encoded(held, at.Width, at.Height))));

    public static IO<Icon> DrawingIcon(GlyphSource glyph, Size pixels) =>
        glyph.Switch(
            (Glyph: glyph, Pixels: pixels),
            tinted: static (at, _) => Drawn(at.Glyph, at.Pixels),
            paired: static (at, held) => IO.lift(() => Assets.Get(held.Assembly).SystemDrawing.Icons.Get(held.Light, at.Pixels)),
            picture: static (at, _) => Drawn(at.Glyph, at.Pixels));

    private static IO<Icon> Drawn(GlyphSource glyph, Size pixels) =>
        Raster(glyph, pixels).Map(static bitmap => Icon.FromHandle(bitmap.GetHicon()));

    // --- [ART]
    public static byte[] Encoded(GlyphSource.Picture art, int width, int height) {
        SKRect cull = art.Art.CullRect;
        SKRect frame = ImageFit.Contain.Fitted(SKRect.Create(width, height), cull.Size);
        (float x, float y) = (frame.Width / cull.Width, frame.Height / cull.Height);
        using SKColorFilter ink = SKColorFilter.CreateBlendMode(new SKColor((uint)PaintSlot.Text.Read().ToArgb()), SKBlendMode.SrcIn);
        using SKPaint paint = new() { ColorFilter = ink };
        using SKImage image = SKImage.FromPicture(art.Art, new SKSizeI(width, height), SKMatrix.CreateScaleTranslation(x, y, frame.Left - (cull.Left * x), frame.Top - (cull.Top * y)), paint);
        using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    private static string Inked(GlyphSource.Tinted art) {
        using Stream stream = art.Assembly.GetManifestResourceStream(art.Resource) ?? throw new InvalidOperationException($"{art.Assembly.GetName().Name} embeds no {art.Resource}");
        XElement svg = XElement.Load(stream);
        svg.SetAttributeValue("color", PaintSlot.Text.Read().ToHex(includeAlpha: false));
        return svg.ToString(SaveOptions.DisableFormatting);
    }

    private static Bitmap Decoded(byte[] encoded) {
        using MemoryStream stream = new(encoded);
        using Bitmap decoded = new(stream);
        return new Bitmap(decoded);
    }
}
