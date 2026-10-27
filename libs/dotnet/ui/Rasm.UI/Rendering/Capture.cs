using System.Globalization;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Capture {
    // --- [TARGETS]
    private const float PointsPerInch = 72f;

    public static SKSurfaceProperties Aligned { get; } = new(SKPixelGeometry.Unknown);

    public static SKSurfaceProperties Scaled { get; } = new(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.Unknown);

    private static readonly Func<SKImageInfo, Option<SKSurface>> Offscreen = static info => Optional(SKSurface.Create(info, Scaled));

    internal static SKImageInfo HalfFloat(SKSizeI pixels) => new(pixels.Width, pixels.Height, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear());

    // --- [RASTER]
    public static Fin<SKImage> Image(DefaultStyles defaults, ColorScheme theme, Node node, SKSizeI pixels) {
        using Shaping shaping = new();
        return from made in Mount.Create(new Host(new SKSize(pixels.Width, pixels.Height), theme, Visibility.Shown, HostPlacement.Panel, CultureInfo.InvariantCulture), defaults, node, [], shaping)
               from image in Rastered(made.Mount, shaping, made.Mount.Scene.Extended ? HalfFloat(pixels) : new SKImageInfo(pixels.Width, pixels.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb()))
               select image;
    }

    public static Fin<SKData> Encoded(SKImage image, SKEncodedImageFormat format, int quality) =>
        Optional(image.Encode(format, quality)).ToFin(new UIFault(string.Create(CultureInfo.InvariantCulture, $"{format} at quality {quality}"), UIReason.Refused));

    // --- [DOCUMENTS]
    public static IO<Unit> Pdf(Mount mount, Stream stream, SKDocumentPdfMetadata metadata, float scale) =>
        IO.lift(() => {
            SKSize page = mount.Scene.Layout.Frame.Size;
            using SKDocument document = SKDocument.CreatePdf(stream, metadata with { RasterDpi = PointsPerInch * scale });
            Fin<Unit> painted = Painted(document.BeginPage(page.Width, page.Height), mount, scale);
            document.EndPage();
            document.Close();
            return painted;
        });

    public static IO<Unit> Svg(Mount mount, Stream stream) =>
        IO.lift(() => {
            using SKCanvas canvas = SKSvgCanvas.Create(SKRect.Create(mount.Scene.Layout.Frame.Size), stream);
            return Painted(canvas, mount, 1f);
        });

    public static Fin<SKData> Picture(Mount mount) {
        using SKPictureRecorder recorder = new();
        return Painted(recorder.BeginRecording(SKRect.Create(mount.Scene.Layout.Frame.Size)), mount, 1f).Map(_ => {
            using SKPicture recorded = recorder.EndRecording();
            return recorded.Serialize();
        });
    }

    // --- [PAINT]
    private static Fin<SKImage> Rastered(Mount mount, Shaping shaping, SKImageInfo info) =>
        Offscreen(info).ToFin(new UIFault(string.Create(CultureInfo.InvariantCulture, $"{info.Width}x{info.Height} {info.ColorType}"), UIReason.Refused)).Bind(surface => {
            using SKSurface owned = surface;
            using PaintCache cache = new(info, Scaled, shaping, Offscreen);
            using SKRegion whole = new(SKRectI.Create(info.Width, info.Height));
            return Painter.Paint(owned.Canvas, cache, mount, whole, mount.LastTick).Map(_ => owned.Snapshot());
        });

    private static Fin<Unit> Painted(SKCanvas canvas, Mount mount, float scale) {
        SKSizeI pixels = SKRectI.Ceiling(SKRect.Create(mount.Scene.Layout.Frame.Width * scale, mount.Scene.Layout.Frame.Height * scale), outwards: true).Size;
        using PaintCache cache = new(new SKImageInfo(pixels.Width, pixels.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb()), Scaled, mount.Scene.Shaping, Offscreen);
        using SKRegion whole = new(SKRectI.Ceiling(SKRect.Create(mount.Scene.Layout.Frame.Size), outwards: true));
        return Painter.Paint(canvas, cache, mount, whole, mount.LastTick);
    }
}
