using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using TinyEXR;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class HighlightKey {
    public float Threshold { get; }
    public float Softness { get; }

    public Vector4 Extract(Vector4 color) {
        Vector3 light = color.AsVector3() * color.W;
        float v = float.Max(light.X, float.Max(light.Y, light.Z));
        float k = float.Min(Softness, Threshold);
        float h = float.Max(k - float.Abs(v - Threshold), 0f);
        return v > 0f ? new Vector4(light * ((float.Max(v, Threshold) - Threshold + (k > 0f ? h * h / (4f * k) : 0f)) / v), 0f) : Vector4.Zero;
    }

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref float threshold, ref float softness) =>
        validationError = float.IsFinite(threshold) && threshold >= 0f && float.IsFinite(softness) && softness >= 0f ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"highlight threshold {threshold} and softness {softness}"), ImagingReason.Range);

    public static Fin<HighlightKey> Validated(float threshold, float softness) =>
        TryCreate(threshold, softness, out HighlightKey? key, out ImagingFault? fault) ? key : fault;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PointSpread : VectorOfMat {
    private readonly Arr<int> planes;

    private PointSpread(Arr<int> planes, PixelExtent size, Action<PointSpread> fill) : base(planes.Fold(0, int.Max) + 1) {
        this.planes = planes;
        for (int index = 0; index < Size; index++) {
            using Mat plane = this[index];
            plane.Create((2 * size.Height) - 1, (2 * size.Width) - 1, DepthType.Cv32F, 1);
            plane.SetTo(default(MCvScalar));
        }
        fill(this);
    }

    public static PointSpread Veiling(PixelExtent size, double degreesPerPixel) => new([0, 0, 0], size, spread => {
        using Mat kernel = spread[0];
        Span2D<float> cells = kernel.View<float>();
        for (int y = 0; y < cells.Height; y++) {
            for (int x = 0; x < cells.Width; x++)
                cells[y, x] = (float)Spencer(degreesPerPixel * double.Hypot(x - (size.Width - 1), y - (size.Height - 1)));
        }
        kernel.ConvertTo(kernel, DepthType.Cv32F, 1d / CvInvoke.Sum(kernel).V0);

        static double Spencer(double degrees) =>
            (0.384d * 2.61e6d * double.Exp(-double.Pow(degrees / 0.02d, 2d))) + (0.478d * 20.91d / double.Pow(degrees + 0.02d, 3d)) + (0.138d * 72.37d / double.Pow(degrees + 0.02d, 2d));
    });

    public static PointSpread Diffraction(Iris pupil, double airy, PixelExtent size, PixelExtent canvas, Gamut working) => new([0, 1, 2], size, spread => {
        const double reference = 550d;
        const double firstZero = 1.22d;
        Arr<(float Wavelength, Vector3 Matching)> rows = StandardObserver.Matching(working);
        Vector3 total = rows.Fold(Vector3.Zero, static (sum, row) => sum + row.Matching);
        int grid = 2 * CvInvoke.GetOptimalDFTSize(int.Max(canvas.Width, canvas.Height));
        (int middle, double half) = (grid / 2, grid / (2d * pupil.Circumradius));
        Size kernel = new((2 * size.Width) - 1, (2 * size.Height) - 1);
        Point center = new(size.Width - 1, size.Height - 1);
        using Mat energy = new(grid, grid, DepthType.Cv64F, 1);
        using Mat spectrum = new();
        using Mat power = new();
        using Mat dilated = new(kernel.Height, kernel.Width, DepthType.Cv64F, 1);
        using Mat normalized = new();
        Func<(double X, double Y), double> transmission = pupil.Transmission(half);
        Span2D<double> cells = energy.View<double>();
        for (int y = 0; y < grid; y++) {
            for (int x = 0; x < grid; x++)
                cells[y, x] = (1 - (2 * ((x + y) & 1))) * transmission((x - middle + 0.5d, y - middle + 0.5d));
        }
        CvInvoke.Dft(energy, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.MulSpectrums(spectrum, spectrum, power, MulSpectrumsType.Default, conjB: true);
        CvInvoke.ExtractChannel(power, energy, 0);
        ReadOnlySpan2D<double> samples = energy.View<double>();
        Span2D<double> light = dilated.View<double>();
        foreach ((float wavelength, Vector3 matching) in rows) {
            double step = reference * firstZero * pupil.Circumradius / airy / wavelength / grid;
            (double Phase, double Footprint)[] across = [.. Enumerable.Range(0, kernel.Width).Select(x => Cell(step, x - center.X))];
            (double Phase, double Footprint)[] down = [.. Enumerable.Range(0, kernel.Height).Select(y => Cell(step, y - center.Y))];
            for (int y = 0; y < light.Height; y++) {
                for (int x = 0; x < light.Width; x++) {
                    double cell = across[x].Footprint * down[y].Footprint;
                    light[y, x] = cell * cell * samples.Sample(
                        (middle + (grid * across[x].Phase) + 0.5d, middle + (grid * down[y].Phase) + 0.5d),
                        (WrapMode.Periodic, WrapMode.Periodic), 0d, static (sum, pixel, weight) => sum + (pixel * weight));
                }
            }
            dilated.ConvertTo(normalized, DepthType.Cv32F, 1d / CvInvoke.Sum(dilated).V0);
            for (int component = 0; component < 3; component++) {
                using Mat plane = spread[component];
                CvInvoke.ScaleAdd(normalized, (double)matching[component] / total[component], plane, plane);
            }
        }

        static (double Phase, double Footprint) Cell(double step, int offset) {
            if (offset is 0)
                return (0d, 1d);
            double frequency = step * int.Abs(offset);
            double phase = frequency - double.Floor(frequency + 0.5d);
            return (offset < 0 ? -phase : phase, double.SinPi(phase) / (double.Pi * frequency));
        }
    });

    public void Convolve(Mat light) {
        for (int component = 0; component < planes.Count; component++) {
            using Mat channel = new();
            using Mat kernel = this[planes[component]];
            CvInvoke.ExtractChannel(light, channel, component);
            CvInvoke.Filter2D(channel, channel, kernel, new Point(-1, -1), 0d, BorderType.Constant);
            CvInvoke.InsertChannel(channel, light, component);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Gaussian {
    extension(Mat plane) {
        public Fin<Unit> Blur(Mat target, double radius) => PixelSampling.KernelTaps<int>(radius).Map(taps => {
            CvInvoke.GaussianBlur(plane, target, new Size(taps, taps), radius / 3d, radius / 3d, BorderType.Replicate);
            return unit;
        });
    }

    public static Fin<Mat> Kernel(double sigma) =>
        PixelSampling.GaussianTaps<int>(sigma).Map(taps => CvInvoke.GetGaussianKernel(taps, sigma, DepthType.Cv64F));
}

public static class LightSpread {
    private static readonly Progress<int> Unreported = new();

    public static Vector4 Light(Vector4 pixel) => new(pixel.AsVector3() * pixel.W, 0f);

    extension(PixelFrame frame) {
        public Fin<Unit> Spread(Func<Vector4, Vector4> fill, Func<Mat, Fin<Unit>> spread, BlendingMode mode, float weight, Gamut gamut, IProgress<int> progress) => frame.Filter(_ => {
            using Mat light = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            return new PixelPass.Pointwise((row, _, line) => {
                Span<Vector4> plane = LightRow(line);
                for (int x = 0; x < row.Length; x++)
                    plane[x] = fill(row[x]);
            }).Run(frame, Unreported).Bind(_ => spread(light)).Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                Span<Vector4> layer = LightRow(line);
                for (int x = 0; x < layer.Length; x++)
                    layer[x] = new(row[x].W > 0f ? layer[x].AsVector3() / row[x].W : Vector3.Zero, 0f);
                using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                weights.Span.Fill(weight);
                mode.Mixed(row, layer, weights.Span, gamut);
                layer.CopyTo(row);
            }).Run(frame, progress));

            Span<Vector4> LightRow(long line) => light.View<Vector4>().GetRowSpan(frame.RowIndex(line));
        });
    }
}

public static class TentPyramid {
    public static void Spread(Mat light, int halvings) {
        using Mat tent = new(1, 3, DepthType.Cv32F, 1);
        tent.SetTo([0.25f, 0.5f, 0.25f]);
        Chained(light, halvings, static tap => 1d / (1d + double.Max(0d, double.Max(tap.X, double.Max(tap.Y, tap.Z)))), tent);
    }

    public static void Soiled(Mat light, PixelFrame frame, PixelFrame dirt, float gain) {
        double cover = double.Max((double)frame.Extent.Width / dirt.Size.Width, (double)frame.Extent.Height / dirt.Size.Height);
        (int width, int height) = ((int)double.Round(dirt.Size.Width * cover, MidpointRounding.ToEven), (int)double.Round(dirt.Size.Height * cover, MidpointRounding.ToEven));
        using SpanOwner<float> fitted = SpanOwner<float>.Allocate(4 * width * height);
        ImageProcessing.Resize(dirt.Block, dirt.Size.Width, dirt.Size.Height, fitted.Span, width, height, 4, cover < 1d ? ResizeFilter.Box : ResizeFilter.Triangle);
        ReadOnlySpan2D<Vector4> soil = fitted.Span.Cast<float, Vector4>().AsSpan2D(height, width);
        Span2D<Vector4> cells = light.View<Vector4>();
        for (int y = 0; y < cells.Height; y++) {
            for (int x = 0; x < cells.Width; x++)
                cells[y, x] *= Vector4.One + (gain * soil.Sample(
                    (((width - (double)frame.Extent.Width) / 2d) + frame.Origin.X + x + 0.5d, ((height - (double)frame.Extent.Height) / 2d) + frame.Origin.Y + y + 0.5d),
                    (WrapMode.Black, WrapMode.Black), Vector4.Zero, static (sum, pixel, weight) => sum + ((float)weight * pixel)));
        }
    }

    private static void Chained(Mat level, int halvings, Func<Vector4, double> weight, Mat tent) {
        if (halvings <= 0)
            return;
        using Mat next = new(level.Rows / 2, level.Cols / 2, DepthType.Cv32F, 4);
        using Mat up = new(level.Rows, level.Cols, DepthType.Cv32F, 4);
        ReadOnlySpan2D<Vector4> taps = level.View<Vector4>();
        Span2D<Vector4> cells = next.View<Vector4>();
        for (int y = 0; y < cells.Height; y++) {
            for (int x = 0; x < cells.Width; x++) {
                (double u, double v) = ((x + 0.5d) * level.Cols / next.Cols, (y + 0.5d) * level.Rows / next.Rows);
                Vector256<double> sum = (0.5d * Group(taps, u, v, weight)) + (0.125d * (Group(taps, u - 1d, v - 1d, weight) + Group(taps, u + 1d, v - 1d, weight)
                    + Group(taps, u - 1d, v + 1d, weight) + Group(taps, u + 1d, v + 1d, weight)));
                cells[y, x] = sum.Narrowed.AsVector4();
            }
        }
        Chained(next, halvings - 1, static _ => 1d, tent);
        ImageProcessing.Resize(next.GetSpan<float>(), next.Cols, next.Rows, up.GetSpan<float>(), up.Cols, up.Rows, 4, ResizeFilter.Triangle);
        CvInvoke.SepFilter2D(up, up, DepthType.Cv32F, tent, tent, new Point(-1, -1), 0d, BorderType.Replicate);
        CvInvoke.AddWeighted(level, 1d / (halvings + 1), up, (double)halvings / (halvings + 1), 0d, level);
    }

    private static Vector256<double> Group(ReadOnlySpan2D<Vector4> taps, double x, double y, Func<Vector4, double> weight) {
        (Vector256<double> sum, double total) = (Vector256<double>.Zero, 0d);
        for (int corner = 0; corner < 4; corner++) {
            Vector256<double> sample = taps.Sample((x + (2 * (corner & 1)) - 1d, y + (corner & 2) - 1d),
                (WrapMode.Clamp, WrapMode.Clamp), Vector256<double>.Zero, static (sum, pixel, factor) => sum + (factor * pixel.Wide));
            Vector4 tap = sample.Narrowed.AsVector4();
            (sum, total) = weight(tap) switch { var w => (sum + (w * sample), total + w) };
        }
        return sum / total;
    }
}

public static class GhostReflections {
    private static readonly Arr<Vector3> Modulated = [Vector3.Zero, new(0f, 1f, 1f), new(1f, 1f, 0f), new(1f, 0f, 1f)];

    public static Fin<Unit> Reflected(Mat key, PixelFrame frame, float radius, int iterations, float modulation) {
        using Mat big = new();
        using Mat input = Mat.Zeros(key.Rows, key.Cols, DepthType.Cv32F, 4);
        using Mat reach = new(key.Rows, key.Cols, DepthType.Cv32F, 4);
        (double x, double y) = (frame.Extent.Width / 2d, frame.Extent.Height / 2d);
        Span2D<Vector4> cells = reach.View<Vector4>();
        for (int row = 0; row < cells.Height; row++) {
            for (int column = 0; column < cells.Width; column++)
                cells[row, column] = new Vector4((float)double.Hypot((frame.Origin.X + column + 0.5d - x) / x, (frame.Origin.Y + row + 0.5d - y) / y));
        }
        PointF center = new((float)(x - frame.Origin.X - 0.5d), (float)(y - frame.Origin.Y - 0.5d));
        return key.Blur(big, 2f * radius).Bind(_ => key.Blur(key, radius)).Map(_ => {
            Accumulate(key, input, reach, center, [(2.13f, Vector4.One)]);
            Accumulate(big, input, reach, center, [(-0.97f, Vector4.One)]);
            key.SetTo(default(MCvScalar));
            for (int i = 1; i < iterations; i++) {
                Accumulate(input, key, reach, center, Copies(i, iterations, modulation));
                key.CopyTo(input);
            }
            return unit;
        });
    }

    public static (float Scale, Vector4 Gain)[] Copies(int iteration, int count, float modulation) =>
        [.. Enumerable.Range(0, Modulated.Count).Select(k => (
            (2.1f * (1f - (((4 * iteration) + k + (count % 2 is 1 ? 0.5f : 0f)) / (4f * count)))) switch { var s => k % 2 is 1 ? -0.99f / s : s },
            new Vector4((Vector3.One - (Modulated[k] * modulation)) / Modulated.Count, 0f)))];

    private static void Accumulate(Mat source, Mat target, Mat reach, PointF center, ReadOnlySpan<(float Scale, Vector4 Gain)> copies) {
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
            CvInvoke.Multiply(warped, weight, warped);
            CvInvoke.Add(target, warped, target);
        }
    }
}
