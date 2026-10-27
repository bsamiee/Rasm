using System.Globalization;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Capture {
    // --- [TARGETS]
    private static readonly SKSurfaceProperties Scaled = new(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.Unknown);

    internal static SKImageInfo Info(SKSize size, float scale, bool extended) =>
        SKRectI.Ceiling(SKRect.Create(size.Width * scale, size.Height * scale), outwards: true).Size switch {
            var pixels => new SKImageInfo(pixels.Width, pixels.Height, extended ? SKColorType.RgbaF16 : SKImageInfo.PlatformColorType, SKAlphaType.Premul,
                extended ? SKColorSpace.CreateSrgbLinear() : SKColorSpace.CreateSrgb()),
        };

    private static readonly Func<SKImageInfo, Option<SKSurface>> Offscreen = static info => Optional(SKSurface.Create(info, Scaled));

    // --- [RASTER]
    public static Fin<SKImage> Image(Host host, StyleSheet sheet, Node node, float scale, Shaping shaping) =>
        Mount.Create(host, sheet, node, shaping).Bind(mount => {
            SKImageInfo info = Info(mount.Scene.Layout.Frame.Size, scale, mount.Scene.Extended);
            Fin<SKImage> image = Offscreen(info).ToFin(new UIFault(string.Create(CultureInfo.InvariantCulture, $"{info.Width}x{info.Height} {info.ColorType}"), UIReason.Refused)).Bind(surface => {
                using SKSurface owned = surface;
                owned.Canvas.Scale(scale);
                return Painted(owned.Canvas, mount, scale, owned.Snapshot);
            });
            mount.Scene.Outlines.Dispose();
            return image;
        });

    // --- [DOCUMENTS]
    public static IO<Unit> Pdf(Mount mount, Stream stream, SKDocumentPdfMetadata metadata, float scale) =>
        IO.lift(() => {
            SKSize page = mount.Scene.Layout.Frame.Size;
            using SKDocument document = SKDocument.CreatePdf(stream, metadata with { RasterDpi = SKDocument.DefaultRasterDpi * scale });
            Fin<Unit> painted = Painted(document.BeginPage(page.Width, page.Height), mount, scale, static () => unit);
            document.EndPage();
            document.Close();
            return painted;
        });

    public static IO<Unit> Svg(Mount mount, Stream stream) =>
        IO.lift(() => {
            using SKCanvas canvas = SKSvgCanvas.Create(SKRect.Create(mount.Scene.Layout.Frame.Size), stream);
            return Painted(canvas, mount, 1f, static () => unit);
        });

    public static Fin<SKPicture> Picture(Mount mount) {
        using SKPictureRecorder recorder = new();
        return Painted(recorder.BeginRecording(SKRect.Create(mount.Scene.Layout.Frame.Size), useRTree: true), mount, 1f, recorder.EndRecording);
    }

    // --- [PAINT]
    private static Fin<T> Painted<T>(SKCanvas canvas, Mount mount, float scale, Func<T> finished) {
        SKImageInfo info = Info(mount.Scene.Layout.Frame.Size, scale, mount.Scene.Extended);
        using PaintCache cache = new(info, Scaled, mount.Scene.Shaping, Offscreen);
        using SKRegion whole = new(info.Rect);
        return Painter.Paint(canvas, cache, mount, whole, mount.LastTick, None).Map(_ => finished());
    }
}
