using System.Globalization;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Capture {
    // --- [TARGETS]
    public static SKSurfaceProperties Aligned { get; } = new(SKPixelGeometry.Unknown);

    public static SKSurfaceProperties Scaled { get; } = new(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.Unknown);

    private static readonly Func<SKImageInfo, Option<SKSurface>> Offscreen = static info => Optional(SKSurface.Create(info, Scaled));

    internal static SKImageInfo Info(SKSizeI pixels, bool extended) =>
        extended
            ? new SKImageInfo(pixels.Width, pixels.Height, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear())
            : new SKImageInfo(pixels.Width, pixels.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

    private static SKImageInfo Info(Mount mount, float scale) =>
        Info(SKRectI.Ceiling(SKRect.Create(mount.Scene.Layout.Frame.Width * scale, mount.Scene.Layout.Frame.Height * scale), outwards: true).Size, mount.Scene.Extended);

    // --- [RASTER]
    public static Fin<SKImage> Image(Host host, DefaultStyles defaults, Node node, float scale) {
        using Shaping shaping = new();
        return from made in Mount.Create(host, defaults, node, [], shaping)
               let info = Info(made.Mount, scale)
               from surface in Offscreen(info).ToFin(new UIFault(string.Create(CultureInfo.InvariantCulture, $"{info.Width}x{info.Height} {info.ColorType}"), UIReason.Refused))
               from image in Snapshot(surface, made.Mount, info, scale)
               select image;
    }

    private static Fin<SKImage> Snapshot(SKSurface surface, Mount mount, SKImageInfo info, float scale) {
        using SKSurface owned = surface;
        owned.Canvas.Scale(scale);
        return Painted(owned.Canvas, mount, info).Map(_ => owned.Snapshot());
    }

    public static Fin<SKData> Encoded(SKImage image, SKEncodedImageFormat format, int quality) =>
        Optional(image.Encode(format, quality)).ToFin(new UIFault(string.Create(CultureInfo.InvariantCulture, $"{format} at quality {quality}"), UIReason.Refused));

    // --- [DOCUMENTS]
    public static IO<Unit> Pdf(Mount mount, Stream stream, SKDocumentPdfMetadata metadata, float scale) =>
        IO.lift(() => {
            SKSize page = mount.Scene.Layout.Frame.Size;
            using SKDocument document = SKDocument.CreatePdf(stream, metadata with { RasterDpi = SKDocument.DefaultRasterDpi * scale });
            Fin<Unit> painted = Painted(document.BeginPage(page.Width, page.Height), mount, Info(mount, scale));
            document.EndPage();
            document.Close();
            return painted;
        });

    public static IO<Unit> Svg(Mount mount, Stream stream) =>
        IO.lift(() => {
            using SKCanvas canvas = SKSvgCanvas.Create(SKRect.Create(mount.Scene.Layout.Frame.Size), stream);
            return Painted(canvas, mount, Info(mount, 1f));
        });

    public static Fin<SKData> Picture(Mount mount) {
        using SKPictureRecorder recorder = new();
        return Painted(recorder.BeginRecording(SKRect.Create(mount.Scene.Layout.Frame.Size)), mount, Info(mount, 1f)).Map(_ => {
            using SKPicture recorded = recorder.EndRecording();
            return recorded.Serialize();
        });
    }

    // --- [PAINT]
    private static Fin<Unit> Painted(SKCanvas canvas, Mount mount, SKImageInfo info) {
        using PaintCache cache = new(info, Scaled, mount.Scene.Shaping, Offscreen);
        using SKRegion whole = new(SKRectI.Ceiling(SKRect.Create(mount.Scene.Layout.Frame.Size), outwards: true));
        return Painter.Paint(canvas, cache, mount, whole, mount.LastTick);
    }
}
