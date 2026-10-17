using System.Numerics;
using System.Runtime.CompilerServices;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Drawing;
using Eto.Mac.Forms;
using Foundation;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;

[assembly: ExportInitializer(typeof(Rasm.Rhino.Render.UI.Mac.RasterInitializer), PlatformID = "macOS")]

namespace Rasm.Rhino.Render.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DrawnRasters {
    internal static IO<Bitmap> Raster(Control view, PixelFrame frame, Transfer encoding, Action<Bitmap> retain) =>
        from host in IO.lift(() => Refused.UnlessFound(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
        from layer in IO.lift(() => Refused.UnlessFound(host.Layer, nameof(NSView.Layer)))
        let samples = Samples(frame, encoding)
        from bitmap in Drawn(samples.Frame, samples.Space, layer, retain)
        select bitmap;

    internal static IO<float> Headroom(Control view) =>
        IO.lift(() =>
            from host in Refused.UnlessFound(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView))
            from window in Refused.UnlessFound(host.Window, nameof(NSView.Window))
            from screen in Refused.UnlessFound(window.Screen, nameof(NSWindow.Screen))
            select (float)screen.MaximumExtendedDynamicRangeColorComponentValue);

    private static (PixelFrame Frame, NSString Space) Samples(PixelFrame frame, Transfer encoding) =>
        encoding.Switch(
                standard: static transfer => transfer.Curve.Map<Option<NSString>>(
                    linear: CGColorSpaceNames.ExtendedLinearSrgb,
                    srgb: CGColorSpaceNames.ExtendedSrgb,
                    pq: None,
                    hlg: None,
                    acesCct: None,
                    acesCc: None),
                gamma: static _ => None,
                camera: static _ => None)
            .Match(
                Some: space => (frame, space),
                None: () => (encoding.Light(frame), CGColorSpaceNames.ExtendedLinearSrgb));

    private static IO<Bitmap> Drawn(PixelFrame frame, NSString name, CALayer layer, Action<Bitmap> retain) =>
        DisposalOps.Acquire(owned =>
            DisposalOps.Within<IDisposable, Bitmap>(temporary =>
                from space in DisposalOps.Acquire(IO.lift(() => Refused.UnlessFound(CGColorSpace.CreateWithName(name), nameof(CGColorSpace.CreateWithName))), temporary)
                from provider in DisposalOps.Acquire(IO.lift(() => new CGDataProvider(frame.Address, frame.Block.Length * sizeof(float), _ => GC.KeepAlive(frame))), temporary)
                let pixel = Unsafe.SizeOf<Vector4>()
                from image in DisposalOps.Acquire(IO.lift(() => new CGImage(
                    frame.Size.Width, frame.Size.Height, sizeof(float) * 8, pixel * 8, frame.Size.Width * pixel, space,
                    CGBitmapFlags.Last | CGBitmapFlags.FloatComponents | CGBitmapFlags.ByteOrder32Little,
                    provider, decode: null, shouldInterpolate: true, CGColorRenderingIntent.Default)), temporary)
                from enabled in IO.lift(() => {
                    if (OperatingSystem.IsMacOSVersionAtLeast(26)) {
                        layer.ContentsHeadroom = image.CalculatedContentHeadroom;
                        layer.PreferredDynamicRange = CADynamicRange.High;
                    } else
                        layer.WantsExtendedDynamicRangeContent = true;
                })
                from drawing in DisposalOps.Acquire(IO.lift(() => {
                    NSImage native = new(image, new CGSize(frame.Size.Width, frame.Size.Height));
                    return (Image: native, Ownership: new Disposal<NSImage>(native, static held => held.Dispose()));
                }), static held => IO.lift(held.Ownership.Dispose), held => temporary(held.Ownership))
                from bitmap in DisposalOps.Acquire(IO.lift(() => new Bitmap(new BitmapHandler(drawing.Image))), held => {
                    drawing.Ownership.Relinquish();
                    owned(held);
                })
                select bitmap, DisposalOps.Release), (bitmap, _) => retain(bitmap));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class RasterInitializer : IPlatformInitializer {
    public void Initialize(Platform platform) {
        platform.Add<FrameRaster>(static () => DrawnRasters.Raster);
        platform.Add<DisplayHeadroom>(static () => DrawnRasters.Headroom);
    }
}
