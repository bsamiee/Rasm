using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Capture {
    // --- [TARGETS]
    public static SKSurfaceProperties Aligned { get; } = new(SKPixelGeometry.Unknown);

    public static SKSurfaceProperties Scaled { get; } = new(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.Unknown);

    internal static SKImageInfo HalfFloat(SKSizeI pixels) => new(pixels.Width, pixels.Height, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgbLinear());

    internal static Fin<SKSurface> Surface(Func<SKSurface?> created, [CallerArgumentExpression(nameof(created))] string subject = "") =>
        created() is { } made ? made : new UIFault(subject, UIReason.Refused);

    // --- [RASTER]
    public static Fin<SKImage> Image(DefaultStyles defaults, ColorScheme theme, Node node, SKSizeI pixels) {
        using Shaping shaping = new();
        return from made in Mount.Create(new Host(new SKSize(pixels.Width, pixels.Height), theme, Visibility.Shown, HostPlacement.Panel, CultureInfo.InvariantCulture), defaults, node, [], shaping)
               from image in Painted(made.Mount, made.Mount.Scene.Extended ? HalfFloat(pixels) : new SKImageInfo(pixels.Width, pixels.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb()))
               select image;
    }

    private static Fin<SKImage> Painted(Mount mount, SKImageInfo info) {
        using PaintCache cache = new(info, Scaled);
        using SKRegion whole = new(SKRectI.Create(info.Width, info.Height));
        return Surface(() => SKSurface.Create(info, Scaled)).Bind(surface => {
            using SKSurface owned = surface;
            return Painter.Paint(owned.Canvas, cache, mount, whole, Duration.Zero).Map(_ => owned.Snapshot());
        });
    }
}
