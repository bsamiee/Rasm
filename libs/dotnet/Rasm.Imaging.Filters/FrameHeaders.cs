using System.Numerics;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Filters;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameHeaders {
    extension(PixelFrame frame) {
        public Mat Header() => new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4, frame.Address, step: 0);

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
                for (int y = 0; y < rows.Height; y++) {
                    Span<TPixel> row = rows.GetRowSpan(y);
                    Span<Vector4> pixels = frame.View.Span.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++) pixels[x] = row[x].ToScaledVector4() with { W = pixels[x].W };
                }
            });
    }
}
