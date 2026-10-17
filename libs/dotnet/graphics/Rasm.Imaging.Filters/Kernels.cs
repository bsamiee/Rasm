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
public static class Kernels {
    // --- [FRAMES]
    extension(PixelFrame frame) {
        public Mat Header() => new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4, frame.Address, step: 0);

        public Fin<Unit> Filter(Func<Mat, Fin<Unit>> apply) => Try.lift(() => {
            using Mat header = frame.Header();
            return apply(header);
        }).Run();

        public Fin<Unit> Mutate(Action<Image<RgbaVector>> edit) => Try.lift(() => {
            using Image<RgbaVector> image = new(frame.Size.Width, frame.Size.Height);
            Memory2D<Vector4> view = frame.View;
            Lanes(static (pixels, lanes) => { for (int x = 0; x < pixels.Length; x++) pixels[x] = lanes[x] with { W = 1f }; });
            edit(image);
            Lanes(static (pixels, lanes) => { for (int x = 0; x < pixels.Length; x++) lanes[x] = pixels[x] with { W = lanes[x].W }; });
            void Lanes(Action<Span<Vector4>, Span<Vector4>> copy) =>
                image.ProcessPixelRows(rows => { for (int y = 0; y < rows.Height; y++) copy(MemoryMarshal.Cast<RgbaVector, Vector4>(rows.GetRowSpan(y)), view.Span.GetRowSpan(y)); });
        }).Run();
    }

    // --- [GAUSSIAN]
    extension(Mat plane) {
        public Fin<Unit> Blur(Mat target, double radius) => PixelSampling.KernelTaps<int>(radius).Map(taps => {
            CvInvoke.GaussianBlur(plane, target, new System.Drawing.Size(taps, taps), radius / 3d, radius / 3d, BorderType.Replicate);
            return unit;
        });
    }

    public static Fin<Mat> GaussianKernel(double sigma) =>
        PixelSampling.GaussianTaps<int>(sigma).Map(taps => CvInvoke.GetGaussianKernel(taps, sigma, DepthType.Cv64F));
}
