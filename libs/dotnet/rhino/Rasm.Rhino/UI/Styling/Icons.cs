using System.Drawing;
using System.Globalization;
using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rhino.PlugIns;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.UI;
using SkiaSharp;
using Svg.Model;
using Svg.Model.Services;
using Svg.Skia;
using NativeAssets = Rhino.Resources.Assets;

namespace Rasm.Rhino.UI.Styling;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IGlyph {
    public string Key { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphRole : IGlyph {
    public static readonly GlyphRole Reset = new("reset");
    public static readonly GlyphRole Presets = new("presets");
    public static readonly GlyphRole Pick = new("pick");
    public static readonly GlyphRole Load = new("load");
    public static readonly GlyphRole Options = new("options");
    public static readonly GlyphRole LockOpen = new("lock-open");
    public static readonly GlyphRole LockClosed = new("lock-closed");
    public static readonly GlyphRole VerticalWipe = new("vertical-wipe");
    public static readonly GlyphRole HorizontalWipe = new("horizontal-wipe");
    public static readonly GlyphRole SideBySide = new("side-by-side");
    public static readonly GlyphRole Swap = new("swap");
    public static readonly GlyphRole Fit = new("fit");
    public static readonly GlyphRole ActualSize = new("actual-size");
    public static readonly GlyphRole Flip = new("flip");
    public static readonly GlyphRole Grid = new("grid");
    public static readonly GlyphRole List = new("list");
    public static readonly GlyphRole FavouriteOn = new("favourite-on");
    public static readonly GlyphRole FavouriteOff = new("favourite-off");
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class IconSlot {
    private const int RowMaster = 16;
    private const int HeaderMaster = 20;
    private const int ToolbarMaster = 24;
    private const float ButtonCell = 22f / 16f;

    public static readonly IconSlot PanelTab = new(RowMaster, static () => Accessors.Scoped(nameof(Accessors.TabIconSize), IO.lift(static () => Accessors.TabIconSize(owner: null))));
    public static readonly IconSlot PanelButton = new(RowMaster, static () => Accessors.Scoped(nameof(Accessors.Get), IO.lift(static () => Accessors.Get(Accessors.PrivatePanelButtonSize(Accessors.ToolbarButtons)))));
    public static readonly IconSlot PickButton = Drawn(RowMaster);
    public static readonly IconSlot ListCell = Drawn(RowMaster);
    public static readonly IconSlot DropDownItem = new(RowMaster, static () => IO.pure(HostUtils.RunningOnOSX ? 14 : RowMaster));
    public static readonly IconSlot MenuItem = Drawn(RowMaster);
    public static readonly IconSlot Window = Drawn(RowMaster);
    public static readonly IconSlot Banner = new(RowMaster, static () => IO.pure(44));
    public static readonly IconSlot ContentMenu = Drawn(RowMaster);
    public static readonly IconSlot SectionHeader = Drawn(HeaderMaster);
    public static readonly IconSlot RenderTab = Drawn(HostUtils.RunningOnOSX ? HeaderMaster : RowMaster);
    public static readonly IconSlot PropertiesPage = Drawn(ToolbarMaster);
    public static readonly IconSlot Cursor = Drawn(ToolbarMaster);

    public int Master { get; }

    [UseDelegateFromConstructor]
    public partial IO<int> Size();

    public IO<int> Cell() => Size().Map(static size => (int)MathF.Ceiling(size * ButtonCell));

    private static IconSlot Drawn(int master) => new(master, () => IO.pure(master));
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ResourceImages<TImage>(Assembly assembly, Func<string, Size, Fin<TImage>> load, Func<TImage, Size> measure) : ResourceCache<TImage, Size>(assembly) where TImage : class {
    protected override Size EmptySize => Size.Empty;

    protected override TImage TryLoadImage(string fullPathToResource, Size size, bool autoConvertWhenInDarkMode, out int width, out int height) {
        TImage image = load(fullPathToResource, size).ThrowIfFail();
        ParseSize(image, out width, out height);
        return image;
    }

    protected override TImage ResizeImage(string fullPathToResource, TImage image, Size size) =>
        load(fullPathToResource, size).ThrowIfFail();

    protected override void ParseSize(Size size, out int width, out int height) =>
        (width, height) = (size.Width, size.Height);

    protected override void ParseSize(TImage image, out int width, out int height) =>
        ParseSize(measure(image), out width, out height);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Icons {
    // --- [RESOURCES]
    public static string ResourceName(IGlyph glyph, int master) =>
        string.Create(CultureInfo.InvariantCulture, $"{glyph.Key}-{master}.svg");

    // --- [SVG]
    private static Fin<string> SvgSource(Assembly assembly, string resource) =>
        Optional(ImageResources.SvgFromResourceId(resource, assembly)).ToFin(new NotFound("image resource", resource));

    private static Eto.Drawing.Bitmap ToEtoBitmap(SKBitmap bitmap) =>
        new(bitmap.Width, bitmap.Height, Eto.Drawing.PixelFormat.Format32bppRgba,
            bitmap.Pixels.Select(static color => unchecked((int)(uint)color)));

    private static Fin<TImage> RenderSvg<TImage>(string source, Size pixels, Option<Eto.Drawing.Color> currentColor, Func<SKBitmap, TImage> convert) =>
        Try.lift(() => {
            SKSvg svg = new();
            using SKColorSpace srgb = svg.Settings.Srgb;
            using SKColorSpace linear = svg.Settings.SrgbLinear;
            using (svg) {
                SKPicture picture = Refused.UnlessFound(svg.FromSvgDocument(SvgService.FromSvg(source,
                    new SvgParameters(entities: null, css: null, currentColor.Map(static color => color.ToSystemDrawing()).ToNullable()))), nameof(SKSvg.FromSvgDocument)).ThrowIfFail();
                using SKBitmap bitmap = new(pixels.Width, pixels.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                using SKCanvas canvas = new(bitmap);
                canvas.Clear(SKColors.Transparent);
                SKRect fitted = SKRect.Create(pixels.Width, pixels.Height).AspectFit(picture.CullRect.Size);
                canvas.Translate(fitted.Left, fitted.Top);
                canvas.Scale(fitted.Width / picture.CullRect.Width, fitted.Height / picture.CullRect.Height);
                canvas.Translate(-picture.CullRect.Left, -picture.CullRect.Top);
                svg.Draw(canvas);
                canvas.Flush();
                return convert(bitmap);
            }
        }).Run();

    private static Func<(Assembly Assembly, Eto.Drawing.Color Ink), ResourceImages<TImage>> Tinted<TImage>(Func<SKBitmap, TImage> convert, Func<TImage, Size> measure) where TImage : class =>
        memoUnsafe<(Assembly Assembly, Eto.Drawing.Color Ink), ResourceImages<TImage>>(context =>
            new(context.Assembly, (resource, pixels) => SvgSource(context.Assembly, resource).Bind(source => RenderSvg(source, pixels, Some(context.Ink), convert)), measure));

    // --- [ETO]
    private static readonly Func<(Assembly Assembly, Eto.Drawing.Color Ink, Set<float> Scales), ResourceImages<Eto.Drawing.Icon>> EtoIcons = memoUnsafe<(Assembly Assembly, Eto.Drawing.Color Ink, Set<float> Scales), ResourceImages<Eto.Drawing.Icon>>(static context =>
        new(context.Assembly, (resource, size) => SvgSource(context.Assembly, resource).Bind(source => {
            List<Eto.Drawing.Bitmap> held = [];
            Fin<Eto.Drawing.Icon> icon = Try.lift(() => new Eto.Drawing.Icon(toSeq(context.Scales).Map(scale => {
                Eto.Drawing.Size pixels = Eto.Drawing.Size.Ceiling(new Eto.Drawing.SizeF(size.Width * scale, size.Height * scale));
                Eto.Drawing.Bitmap bitmap = RenderSvg(source, new Size(pixels.Width, pixels.Height), Some(context.Ink), ToEtoBitmap).ThrowIfFail();
                held.Add(bitmap);
                return new Eto.Drawing.IconFrame(scale, bitmap);
            }).Strict())).Run();
            return icon.BindFail(error => {
                Validation<Error, Seq<Unit>> released = toSeq(held).Rev().Traverse(static bitmap => Try.lift(() => {
                    bitmap.Dispose();
                    return unit;
                }).Run().ToValidation()).As();
                return released.Match<Fin<Eto.Drawing.Icon>>(Fail: fault => error + fault, Succ: _ => error);
            });
        }), static icon => new(icon.Width, icon.Height)));

    private static readonly Func<(Assembly Assembly, Eto.Drawing.Color Ink), ResourceImages<Eto.Drawing.Bitmap>> EtoRasters =
        Tinted(ToEtoBitmap, static bitmap => new Size(bitmap.Width, bitmap.Height));

    public static IO<Eto.Drawing.Bitmap> Inked(IGlyph glyph, int master, Eto.Drawing.Size pixels, Eto.Drawing.Color ink) =>
        IO.lift(() => EtoRasters((glyph.GetType().Assembly, ink)).Get(ResourceName(glyph, master), new Size(pixels.Width, pixels.Height)));

    public static IO<Eto.Drawing.Icon> Frames((IGlyph Glyph, int Master) source, IconSlot slot) =>
        slot.Size().Bind(size => IO.lift(() => Frame(source.Glyph, source.Master, new(size, size))));

    public static Eto.Drawing.Icon Frame(IGlyph glyph, int master, Size logicalSize) {
        Set<float> scales = toSet(Screen.Screens.Select(static screen => screen.LogicalPixelSize)
            .Prepend(1f).Concat(HostUtils.RunningOnOSX ? [2f] : []));
        return EtoIcons((glyph.GetType().Assembly, PaintSlot.ControlText.Read(), scales)).Get(ResourceName(glyph, master), logicalSize);
    }

    public static IO<Unit> Themed(IPlugInSink plugIn, (IGlyph Glyph, int Master) source, IconSlot slot, HostEvent<Unit> changes, Action<Eto.Drawing.Icon> write, Action<IDisposable> retain) =>
        from written in IO.pure(Frames(source, slot).Bind(icon => IO.lift(() => write(icon))))
        from _ in written
        from subscription in changes.Inline(__ => written, plugIn, retain)
        select subscription;

    public static IO<Cursor> PickCursor(Action<Cursor> retain) =>
        Frames((GlyphRole.Pick, IconSlot.Cursor.Master), IconSlot.Cursor)
            .Bind(icon => DisposalOps.Acquire(IO.lift(() => new Cursor(icon, new(4f, 20f))), retain));

    // --- [DRAWING]
    private static readonly Func<(Assembly Assembly, Eto.Drawing.Color Ink), ResourceImages<Bitmap>> Rasters = Tinted(static bitmap => {
        using Bitmap borrowed = new(bitmap.Width, bitmap.Height, bitmap.RowBytes, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, bitmap.GetPixels());
        return new Bitmap(borrowed);
    }, static bitmap => bitmap.Size);

    private static readonly Func<(Assembly Assembly, Eto.Drawing.Color Ink), ResourceImages<Icon>> DrawingIcons = Tinted(static bitmap => {
        using SKData png = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using MemoryStream encoded = new();
        static byte Side(int pixels) => pixels <= byte.MaxValue ? (byte)pixels : byte.MinValue;
        using (BinaryWriter directory = new(encoded, System.Text.Encoding.UTF8, leaveOpen: true)) {
            directory.Write([0, 0, 1, 0, 1, 0, Side(bitmap.Width), Side(bitmap.Height), 0, 0, 1, 0, (byte)bitmap.Info.BitsPerPixel, 0]);
            directory.Write(int.CreateChecked(png.Size));
            directory.Write(int.CreateChecked(encoded.Position) + sizeof(int));
        }
        png.SaveTo(encoded);
        encoded.Position = 0;
        return new Icon(encoded);
    }, static icon => icon.Size);

    private static readonly Func<Assembly, ResourceImages<Bitmap>> IconBitmaps = memoUnsafe<Assembly, ResourceImages<Bitmap>>(static assembly =>
        new(assembly, (resource, pixels) => Try.lift(() => {
            using Icon icon = new(assembly.SystemDrawingAssets().Icons.Get(resource), pixels);
            return icon.ToBitmap();
        }).Run(), static bitmap => bitmap.Size));

    public static IO<Bitmap> Raster((IGlyph Glyph, int Master) source, IconSlot slot) =>
        from size in slot.Size()
        from pixels in HostTheme.Device(new(size, size))
        from bitmap in Raster(source.Glyph, source.Master, pixels)
        select bitmap;

    public static IO<Bitmap> Raster(IGlyph glyph, int master, Size pixels) =>
        IO.lift(() => Rasters((glyph.GetType().Assembly, PaintSlot.ControlText.Read())).Get(ResourceName(glyph, master), pixels));

    public static IO<Icon> DrawingIcon(IGlyph glyph, int master, Size pixels) =>
        IO.lift(() => DrawingIcons((glyph.GetType().Assembly, PaintSlot.ControlText.Read())).Get(ResourceName(glyph, master), pixels));

    public static IO<Option<TImage>> PlugInImage<TImage>(IPlugInSink plugIn, Func<(NativeAssets Native, ResourceCache<Bitmap, Size> IconBitmaps), string, TImage> read) =>
        IO.lift(() => {
            Assembly assembly = plugIn.GetType().Assembly;
            return toSeq(assembly.GetCustomAttributes<PlugInDescriptionAttribute>())
                .Find(static description => description.DescriptionType is DescriptionType.Icon)
                .Map(description => read((NativeAssets.Get(assembly), IconBitmaps(assembly)), description.Value));
        });
}
