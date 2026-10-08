using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
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

        public void Mutate(Action<Image<RgbaVector>> edit) {
            using Image<RgbaVector> image = new(frame.Size.Width, frame.Size.Height);
            Memory2D<Vector4> view = frame.View;
            Lanes(static (pixels, lanes) => { for (int x = 0; x < pixels.Length; x++) pixels[x] = lanes[x] with { W = 1f }; });
            edit(image);
            Lanes(static (pixels, lanes) => { for (int x = 0; x < pixels.Length; x++) lanes[x] = pixels[x] with { W = lanes[x].W }; });
            void Lanes(Action<Span<Vector4>, Span<Vector4>> copy) =>
                image.ProcessPixelRows(rows => { for (int y = 0; y < rows.Height; y++) copy(MemoryMarshal.Cast<RgbaVector, Vector4>(rows.GetRowSpan(y)), view.Span.GetRowSpan(y)); });
        }
    }
}
