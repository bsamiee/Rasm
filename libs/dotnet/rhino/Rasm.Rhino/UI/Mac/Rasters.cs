using AppKit;
using CoreAnimation;
using CoreGraphics;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Forms;

namespace Rasm.Rhino.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DrawnRasters {
    internal static Fin<Unit> Present(Control view, Graphics graphics, SkiaSharp.SKImage frame, RectangleF destination) =>
        (from host in Refused.UnlessFound(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView))
         from layer in Refused.UnlessFound(host.Layer, nameof(NSView.Layer))
         from context in Conversions.Cast<CGContext>(graphics.ControlObject)
         from pixels in Refused.UnlessFound(frame.PeekPixels(), nameof(SkiaSharp.SKImage.PeekPixels))
         from space in Refused.UnlessFound(CGColorSpace.CreateWithName(CGColorSpaceNames.ExtendedLinearSrgb), nameof(CGColorSpace.CreateWithName))
         select (Layer: layer, Context: context, Pixels: pixels, Space: space))
        .MapFail(error => {
            frame.Dispose();
            return error;
        })
        .Map(held => {
            using SkiaSharp.SKPixmap pixmap = held.Pixels;
            using CGColorSpace space = held.Space;
            using CGDataProvider provider = new(pixmap.GetPixels(), pixmap.BytesSize, _ => frame.Dispose());
            using CGImage image = new(pixmap.Width, pixmap.Height, 16, 64, pixmap.RowBytes, space,
                CGBitmapFlags.PremultipliedLast | CGBitmapFlags.FloatComponents | CGBitmapFlags.ByteOrder16Little, provider, decode: null, shouldInterpolate: false, CGColorRenderingIntent.Default);
            held.Layer.ContentsFormat = CAContentsFormat.Rgba16Float;
            if (OperatingSystem.IsMacOSVersionAtLeast(26)) {
                held.Layer.ContentsHeadroom = image.CalculatedContentHeadroom;
                held.Layer.PreferredDynamicRange = CADynamicRange.High;
            } else
                held.Layer.WantsExtendedDynamicRangeContent = true;
            held.Context.SaveState();
            held.Context.TranslateCTM(destination.X, destination.Y + destination.Height);
            held.Context.ScaleCTM(1, -1);
            held.Context.DrawImage(new CGRect(0, 0, destination.Width, destination.Height), image);
            held.Context.RestoreState();
            return unit;
        });
}
