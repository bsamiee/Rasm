using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Filters;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameHeaders {
    extension(PixelFrame frame) {
        public Mat Header() =>
            new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4, frame.Address, step: 0);

        public Image<RgbaVector> Image() {
            Image<RgbaVector> image = SixLabors.ImageSharp.Image.LoadPixelData(MemoryMarshal.Cast<float, RgbaVector>(frame.Block), frame.Size.Width, frame.Size.Height);
            image.ProcessPixelRows(static rows => {
                for (int y = 0; y < rows.Height; y++)
                    foreach (ref RgbaVector pixel in rows.GetRowSpan(y)) pixel.A = 1f;
            });
            return image;
        }

        public void Write<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel> =>
            image.ProcessPixelRows(rows => {
                using SpanOwner<Vector4> converted = SpanOwner<Vector4>.Allocate(rows.Width);
                for (int y = 0; y < rows.Height; y++) {
                    Span<Vector4> pixels = frame.View.Span.GetRowSpan(y);
                    PixelOperations<TPixel>.Instance.ToVector4(image.Configuration, rows.GetRowSpan(y), converted.Span, PixelConversionModifiers.Scale);
                    for (int x = 0; x < pixels.Length; x++) pixels[x] = converted.Span[x] with { W = pixels[x].W };
                }
            });
    }
}
