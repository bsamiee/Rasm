using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filtering;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PointSpread(Seq<Mat> kernels) : IDisposable {
    public static PointSpread Veiling(PixelExtent size, double degreesPerPixel) {
        using Mat glare = new((2 * size.Height) - 1, (2 * size.Width) - 1, DepthType.Cv32F, 1);
        Span2D<float> cells = glare.View<float>();
        for (int y = 0; y < cells.Height; y++) {
            for (int x = 0; x < cells.Width; x++)
                cells[y, x] = (float)Spencer(degreesPerPixel * double.Hypot(x - (size.Width - 1), y - (size.Height - 1)));
        }
        return new(Seq(glare * (1d / CvInvoke.Sum(glare).V0)));

        static double Spencer(double degrees) =>
            (0.384d * 2.61e6d * double.Exp(-double.Pow(degrees / 0.02d, 2d))) + (0.478d * 20.91d / double.Pow(degrees + 0.02d, 3d)) + (0.138d * 72.37d / double.Pow(degrees + 0.02d, 2d));
    }

    public static PointSpread Diffraction(Iris pupil, double airy, PixelExtent size, PixelExtent canvas, Gamut working) {
        const double reference = 550d;
        const double firstZero = 1.22d;
        Arr<(float Wavelength, Vector3 Matching)> rows = StandardObserver.Matching(working);
        Vector3 total = rows.Fold(Vector3.Zero, static (sum, row) => sum + row.Matching);
        int grid = 2 * CvInvoke.GetOptimalDFTSize(int.Max(canvas.Width, canvas.Height));
        (int middle, double half, int width, int height) = (grid / 2, grid / (2d * pupil.Circumradius), (2 * size.Width) - 1, (2 * size.Height) - 1);
        Mat[] planes = [.. Enumerable.Range(0, 3).Select(_ => Mat.Zeros(height, width, DepthType.Cv64F, 1))];
        using Mat energy = new(grid, grid, DepthType.Cv64F, 1);
        using Mat spectrum = new();
        using Mat light = new();
        using Mat shift = new(2, 3, DepthType.Cv64F, 1);
        using Mat footprint = new(height, width, DepthType.Cv64F, 1);
        Span2D<double> cells = energy.View<double>();
        for (int y = 0; y < grid; y++) {
            for (int x = 0; x < grid; x++)
                cells[y, x] = (1 - (2 * ((x + y) & 1))) * pupil.Mass(half, 0d, new RectangleF(x - middle, y - middle, 1f, 1f));
        }
        CvInvoke.Dft(energy, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.MulSpectrums(spectrum, spectrum, spectrum, MulSpectrumsType.Default, conjB: true);
        CvInvoke.ExtractChannel(spectrum, energy, 0);
        foreach ((float wavelength, Vector3 matching) in rows) {
            double scale = reference * firstZero * pupil.Circumradius / airy / wavelength;
            double[] columns = [.. Enumerable.Range(0, width).Select(x => Sinc(scale / grid * (x - (size.Width - 1))))];
            double[] lines = [.. Enumerable.Range(0, height).Select(y => Sinc(scale / grid * (y - (size.Height - 1))))];
            Span2D<double> weights = footprint.View<double>();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++)
                    weights[y, x] = double.Pow(columns[x] * lines[y], 2d);
            }
            shift.SetTo([scale, 0d, middle - (scale * (size.Width - 1)), 0d, scale, middle - (scale * (size.Height - 1))]);
            CvInvoke.WarpAffine(energy, light, shift, footprint.Size, Inter.Linear, Warp.InverseMap, BorderType.Wrap, default);
            CvInvoke.Multiply(light, footprint, light);
            CvInvoke.Normalize(light, light, 1d, 0d, NormType.L1);
            for (int component = 0; component < planes.Length; component++)
                CvInvoke.ScaleAdd(light, (double)matching[component] / total[component], planes[component], planes[component]);
        }
        return new(toSeq(planes));

        static double Sinc(double t) => t is 0d ? 1d : double.SinPi(t) / (double.Pi * t);
    }

    public void Convolve(Mat light, BorderType border = BorderType.Constant) {
        if (kernels is [var shared]) {
            CvInvoke.Filter2D(light, light, shared, new Point(-1, -1), 0d, border);
            return;
        }
        using Mat channel = new();
        for (int index = 0; index < kernels.Count; index++) {
            CvInvoke.ExtractChannel(light, channel, index);
            CvInvoke.Filter2D(channel, channel, kernels[index], new Point(-1, -1), 0d, border);
            CvInvoke.InsertChannel(channel, light, index);
        }
    }

    public void Dispose() {
        foreach (Mat kernel in kernels)
            kernel.Dispose();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Gaussian {
    extension(Mat plane) {
        public void Blur(Mat target, double sigma, BorderType border = BorderType.Replicate) =>
            CvInvoke.GaussianBlur(plane, target, sigma > 0d ? Size.Empty : new Size(1, 1), sigma, sigma, border);
    }

    public static Fin<int> Taps(double radius) =>
        double.Ceiling(radius) switch {
            var reach when double.IsFinite(reach) && reach >= 0d && (2d * reach) + 1d <= int.MaxValue => (2 * (int)reach) + 1,
            _ => ImagingFault.Of(ImagingReason.Range, radius),
        };

    public static Fin<Mat> Kernel(double sigma, NormType norm = NormType.L1) =>
        Taps(3d * sigma).Map(taps => {
            Mat kernel = CvInvoke.GetGaussianKernel(taps, sigma, DepthType.Cv64F);
            CvInvoke.Normalize(kernel, kernel, 1d, 0d, norm);
            return kernel;
        });
}

public static class Glare {
    private static readonly Arr<Vector3> Tints = [Vector3.Zero, new(0f, 1f, 1f), new(1f, 1f, 0f), new(1f, 0f, 1f)];

    extension(PixelFrame frame) {
        public Fin<Unit> Spread(float threshold, float knee, Action<Mat> spread, BlendingMode mode, float weight, Seq<Mask> masks, Option<IProgress<int>> progress = default) =>
            mode.Mixed(new PixelPass.Frame((staged, report) => staged.Filter(header => {
                using Mat light = new(header.Size, DepthType.Cv32F, 4);
                staged.Rows((row, _, y) => {
                    Span<Vector4> lit = light.View<Vector4>().GetRowSpan(y - staged.Origin.Y);
                    for (int x = 0; x < row.Length; x++)
                        lit[x] = BrightPass(row[x], threshold, knee);
                });
                spread(light);
                staged.Rows((row, _, y) => {
                    ReadOnlySpan<Vector4> layer = light.View<Vector4>().GetRowSpan(y - staged.Origin.Y);
                    for (int x = 0; x < row.Length; x++)
                        row[x] = new Vector4(row[x].W > 0f ? layer[x].AsVector3() / row[x].W : Vector3.Zero, row[x].W);
                }, report);
                return unit;
            })), weight, masks).Match(glare => glare.Run(frame, progress), static () => unit);
    }

    public static void Bloom(Mat light, int levels) {
        using Mat weighted = light.Clone();
        using Mat first = new();
        using VectorOfMat pyramid = new();
        Span2D<Vector4> pixels = weighted.View<Vector4>();
        for (int y = 0; y < pixels.Height; y++) {
            for (int x = 0; x < pixels.Width; x++)
                pixels[y, x] = (1f / (1f + float.Max(0f, float.Max(pixels[y, x].X, float.Max(pixels[y, x].Y, pixels[y, x].Z))))) switch { var w => new(pixels[y, x].AsVector3() * w, w) };
        }
        CvInvoke.PyrDown(weighted, first, BorderType.Replicate);
        Span2D<Vector4> means = first.View<Vector4>();
        for (int y = 0; y < means.Height; y++) {
            for (int x = 0; x < means.Width; x++)
                means[y, x] = means[y, x].W > 0f ? new(means[y, x].AsVector3() / means[y, x].W, 0f) : Vector4.Zero;
        }
        CvInvoke.BuildPyramid(first, pyramid, int.Max(levels - 1, 0), BorderType.Replicate);
        for (int level = levels - 1; level >= 0; level--) {
            using Mat coarse = pyramid[level];
            using Mat fine = level > 0 ? pyramid[level - 1] : new Mat(light, new Rectangle(Point.Empty, light.Size));
            using Mat up = new();
            CvInvoke.PyrUp(coarse, up, BorderType.Replicate);
            using Mat cropped = new(up, new Rectangle(Point.Empty, fine.Size));
            CvInvoke.AddWeighted(fine, 1d / (levels - level + 1), cropped, (double)(levels - level) / (levels - level + 1), 0d, fine);
        }
    }

    public static void Dirt(Mat light, PixelFrame frame, PixelFrame dirt, float gain) {
        double cover = double.Max((double)frame.Extent.Width / dirt.Size.Width, (double)frame.Extent.Height / dirt.Size.Height);
        Size fit = new((int)double.Round(dirt.Size.Width * cover, MidpointRounding.ToEven), (int)double.Round(dirt.Size.Height * cover, MidpointRounding.ToEven));
        using Mat source = dirt.Header();
        using Mat fitted = new();
        using Mat soil = new();
        using Mat shift = new(2, 3, DepthType.Cv64F, 1);
        CvInvoke.Resize(source, fitted, fit, 0d, 0d, cover < 1d ? Inter.Area : Inter.Linear);
        shift.SetTo([1d, 0d, ((fit.Width - (double)frame.Extent.Width) / 2d) + frame.Origin.X, 0d, 1d, ((fit.Height - (double)frame.Extent.Height) / 2d) + frame.Origin.Y]);
        CvInvoke.WarpAffine(fitted, soil, shift, light.Size, Inter.Linear, Warp.InverseMap, BorderType.Constant, default);
        CvInvoke.Multiply(light, soil, soil, gain);
        CvInvoke.Add(light, soil, light);
    }

    public static void Ghosts(Mat key, PixelFrame frame, float sigma, int iterations, float modulation) {
        using Mat wide = new();
        using Mat input = Mat.Zeros(key.Rows, key.Cols, DepthType.Cv32F, 4);
        using Mat reach = new(key.Size, DepthType.Cv32F, 4);
        Vector2 middle = frame.Extent.Point(new Vector2(0.5f));
        Span2D<Vector4> cells = reach.View<Vector4>();
        for (int row = 0; row < cells.Height; row++) {
            for (int column = 0; column < cells.Width; column++)
                cells[row, column] = new Vector4(((new Vector2(frame.Origin.X + column + 0.5f, frame.Origin.Y + row + 0.5f) - middle) / middle).Length());
        }
        PointF center = new(middle.X - frame.Origin.X - 0.5f, middle.Y - frame.Origin.Y - 0.5f);
        key.Blur(wide, 2d * sigma);
        key.Blur(key, sigma);
        Accumulated(key, input, reach, center, Seq((2.13f, Vector4.One)));
        Accumulated(wide, input, reach, center, Seq((-0.97f, Vector4.One)));
        key.SetTo(default(MCvScalar));
        for (int pass = 1; pass < iterations; pass++) {
            Accumulated(input, key, reach, center, toSeq(Tints.Select((tint, k) =>
                ((2.1f * (1f - (((4 * pass) + k + (iterations % 2 is 1 ? 0.5f : 0f)) / (4f * iterations)))) switch { var s => k % 2 is 1 ? -0.99f / s : s },
                    new Vector4((Vector3.One - (tint * modulation)) / Tints.Count, 0f)))));
            key.CopyTo(input);
        }
    }

    private static Vector4 BrightPass(Vector4 color, float threshold, float knee) {
        (Vector3 light, float soft) = (color.AsVector3() * color.W, float.Min(knee, threshold));
        float peak = float.Max(light.X, float.Max(light.Y, light.Z));
        float rise = float.Max(soft - float.Abs(peak - threshold), 0f);
        return peak > 0f ? new Vector4(light * ((float.Max(peak, threshold) - threshold + (soft > 0f ? rise * rise / (4f * soft) : 0f)) / peak), 0f) : Vector4.Zero;
    }

    private static void Accumulated(Mat source, Mat target, Mat reach, PointF center, Seq<(float Scale, Vector4 Gain)> copies) {
        using Mat rotation = new();
        using Mat warped = new();
        using Mat weight = new();
        using ScalarArray floor = new(0d);
        foreach ((float scale, Vector4 gain) in copies) {
            using ScalarArray gains = new(new MCvScalar(gain.X, gain.Y, gain.Z, gain.W));
            CvInvoke.GetRotationMatrix2D(center, 0d, 1d / scale, rotation);
            CvInvoke.WarpAffine(source, warped, rotation, source.Size, Inter.Linear, Warp.Default, BorderType.Constant, default);
            reach.ConvertTo(weight, DepthType.Cv32F, -float.Abs(scale), 1d);
            CvInvoke.Max(weight, floor, weight);
            CvInvoke.Multiply(weight, gains, weight);
            CvInvoke.AccumulateProduct(warped, weight, target);
        }
    }
}
