using CoreAnimation;
using CoreGraphics;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Forms;
using SkiaSharp;

namespace Rasm.RhinoCommon.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DrawnRasters {
    internal static Fin<Unit> Present(Control view, Graphics graphics, SKImage frame, RectangleF destination) =>
        (from host in Conversions.Found(view.GetContainerView())
         from layer in Conversions.Found(host.Layer)
         from context in Conversions.Cast<CGContext>(graphics.ControlObject)
         from pixels in Conversions.Found(frame.PeekPixels())
         from space in Conversions.Found(CGColorSpace.CreateWithName(CGColorSpaceNames.ExtendedLinearSrgb))
         select (Layer: layer, Context: context, Pixels: pixels, Space: space))
        .MapFail(error => {
            frame.Dispose();
            return error;
        })
        .Map(held => {
            using SKPixmap pixmap = held.Pixels;
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
