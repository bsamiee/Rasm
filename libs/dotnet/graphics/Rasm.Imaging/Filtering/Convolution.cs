using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using TinyEXR;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class Iris {
    public int Blades { get; }
    public double Rotation { get; }
    public double Roundness { get; }
    public double Obstruction { get; }
    public double Squeeze { get; }

    public float Circumradius =>
        double.Asin(Roundness * double.Sin(double.Pi / Blades)) switch {
            var beta => (float)double.Sqrt(double.Pi / (Blades / 2d * (double.Sin(double.Tau / Blades) + (Roundness is 0d ? 0d : ((2d * beta) - double.Sin(2d * beta)) / (Roundness * Roundness))))),
        };

    public (double Reach, double Edge) Outline((double X, double Y) q) {
        (double x, double y) = (Squeeze * q.X, q.Y);
        double edge = Boundary(double.Atan2(-y, x), Circumradius);
        return (double.Hypot(x, y) / edge, edge);
    }

    public Func<(double X, double Y), double> Transmission(double radius) {
        (double obstruction, double squeeze, double circumradius) = (Obstruction, Squeeze, Circumradius);
        double margin = double.Hypot(squeeze, 1d) / (2d * circumradius * double.Cos(double.Pi / Blades));
        double[] blades = [.. Enumerable.Range(0, Blades).Select(i => (((-Rotation - (double.Tau * i / Blades)) % double.Tau) + double.Tau) % double.Tau)];
        return point => {
            (double x, double y) = (squeeze * point.X, point.Y);
            double reach = double.Hypot(x, y) / Boundary(double.Atan2(-y, x), circumradius);
            if (reach - margin >= radius || reach + margin <= obstruction * radius)
                return 0d;
            if (reach + margin <= radius && reach - margin >= obstruction * radius)
                return 1d / (1d - obstruction);
            (double left, double right, double top, double bottom) = (x - (squeeze / 2d), x + (squeeze / 2d), y - 0.5d, y + 0.5d);
            double[] angles = [0d, double.Pi / 2d, double.Pi, 3d * double.Pi / 2d, double.Tau, .. blades, Bearing(left, top), Bearing(left, bottom), Bearing(right, top), Bearing(right, bottom)];
            angles.AsSpan().Sort();
            double area = 0d;
            for (int i = 1; i < angles.Length; i++)
                area += Integrate.GaussKronrod(Section, angles[i - 1], angles[i]);
            return area / squeeze;

            double Section(double angle) {
                (double sin, double cos) = double.SinCos(angle);
                (double nearX, double farX) = cos is 0d ? left <= 0d && right >= 0d ? (0d, double.PositiveInfinity) : (double.PositiveInfinity, double.NegativeInfinity)
                    : (double.Min(left / cos, right / cos), double.Max(left / cos, right / cos));
                (double nearY, double farY) = sin is 0d ? top <= 0d && bottom >= 0d ? (0d, double.PositiveInfinity) : (double.PositiveInfinity, double.NegativeInfinity)
                    : (double.Min(top / sin, bottom / sin), double.Max(top / sin, bottom / sin));
                double edge = radius * Boundary(-angle, circumradius);
                (double near, double far) = (double.Max(0d, double.Max(double.Max(nearX, nearY), obstruction * edge)), double.Min(edge, double.Min(farX, farY)));
                return far > near ? (far - near) * (far + near) / (2d * (1d - obstruction)) : 0d;
            }

            static double Bearing(double x, double y) => (double.Atan2(y, x) + double.Tau) % double.Tau;
        };
    }

    public Vector256<double> Weights(double reach, double edge, double radius, double ramp, double fringe) {
        Vector256<double> r = Radii(radius, fringe);
        Vector256<double> distance = Vector256.Create(reach);
        Vector256<double> one = Vector256.Create(1d);
        Vector256<double> obstruction = Vector256.Create(Obstruction) * r;
        Vector256<double> w = Obstruction is 1d ? Vector256<double>.Zero : ramp is 0d
            ? Vector256.ConditionalSelect(Vector256.LessThanOrEqual(distance, r) & Vector256.GreaterThanOrEqual(distance, obstruction), one, Vector256<double>.Zero)
            : Vector256.Create(edge / ramp) switch {
                var slope => Vector256.Min(Vector256.Clamp((slope * (r - distance)) + Vector256.Create(0.5d), Vector256<double>.Zero, one), Vector256.Clamp(one + (slope * (distance - obstruction)), Vector256<double>.Zero, one)),
            };
        w = Vector256.ConditionalSelect(Vector256.GreaterThan(r, Vector256<double>.Zero), w, Vector256<double>.Zero);
        return Vector256.Create(w[0], w[1], w[2], (w[0] + w[1] + w[2]) / 3d);
    }

    public Vector256<double> Integral(double radius, double ramp, double fringe) {
        (Vector256<double> r, double circumradius) = (Radii(radius, fringe), Circumradius);
        double[] areas = [.. Enumerable.Range(0, 3).Select(component => r[component] is 0d || Obstruction is 1d ? 0d
            : Blades / Squeeze * Integrate.GaussKronrod(angle => {
                double boundary = r[component] * Boundary(angle, circumradius);
                double inner = Obstruction * boundary;
                if (ramp is 0d)
                    return (boundary - inner) * (boundary + inner) / 2d;
                (double high, double low) = (boundary + (ramp / 2d), inner - ramp);
                double upper = double.Min(1d, double.Min(high / ramp, (high - low) / (2d * ramp)));
                double width = ramp * upper;
                return low >= 0d ? upper * (high + low) * (high - low - width) / 2d
                    : ((upper * ((high * high) - (high * width) + (width * width / 3d))) - (double.Pow(double.Max(0d, low + width), 3d) / (3d * ramp))) / 2d;
            }, Rotation, Rotation + (double.Tau / Blades)))];
        return Vector256.Create(areas[0], areas[1], areas[2], (areas[0] + areas[1] + areas[2]) / 3d);
    }

    private double Boundary(double angle, double circumradius) {
        (int n, double kappa) = (Blades, Roundness);
        (double s1, double c1) = double.SinCos(double.Pi / n);
        (double sa, double ca) = double.SinCos(double.Ieee754Remainder(angle - Rotation - (double.Pi / n), double.Tau / n));
        double root = double.Sqrt(1d - (kappa * kappa * s1 * s1));
        double delta = root - (kappa * c1);
        return circumradius * ((2d * c1 * root) - (kappa * (1d - (2d * s1 * s1)))) / ((delta * ca) + double.Sqrt(1d - (delta * delta * sa * sa)));
    }

    private static Vector256<double> Radii(double radius, double fringe) =>
        Vector256.Create(radius * (1d - double.Max(fringe, 0d)), radius * (1d - (double.Abs(fringe) / 2d)), radius * (1d + double.Min(fringe, 0d)), 0d);

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref int blades, ref double rotation, ref double roundness, ref double obstruction, ref double squeeze) =>
        validationError = blades >= 3 && double.IsFinite(rotation) && roundness is >= 0d and <= 1d && obstruction is >= 0d and <= 1d && double.IsFinite(squeeze) && squeeze > 0d ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture,
                $"iris of {blades} blades, roundness {roundness}, obstruction {obstruction}, rotation {rotation}, and squeeze {squeeze}"), ImagingReason.Range);

    public static Fin<Iris> Validated(int blades, double rotation, double roundness, double obstruction, double squeeze) =>
        TryCreate(blades, rotation, roundness, obstruction, squeeze, out Iris? iris, out ImagingFault? fault) ? iris : fault;
}

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

    public Fin<Unit> Convolve(Mat light) {
        for (int component = 0; component < planes.Count; component++) {
            using Mat channel = new();
            using Mat kernel = this[planes[component]];
            CvInvoke.ExtractChannel(light, channel, component);
            CvInvoke.Filter2D(channel, channel, kernel, new Point(-1, -1), 0d, BorderType.Constant);
            CvInvoke.InsertChannel(channel, light, component);
        }
        return unit;
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
    public static Unit Spread(Mat light, int halvings) {
        using Mat tent = new(1, 3, DepthType.Cv32F, 1);
        tent.SetTo([0.25f, 0.5f, 0.25f]);
        Chained(light, halvings, static tap => 1d / (1d + double.Max(0d, double.Max(tap.X, double.Max(tap.Y, tap.Z)))), tent);
        return unit;
    }

    public static Unit Soiled(Mat light, PixelFrame frame, PixelFrame dirt, float gain) {
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
        return unit;
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
                cells[y, x] = Vector256.Narrow(sum, Vector256<double>.Zero).GetLower().AsVector4();
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
                (WrapMode.Clamp, WrapMode.Clamp), Vector256<double>.Zero, static (sum, pixel, factor) => sum + (factor * Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W)));
            Vector4 tap = Vector256.Narrow(sample, Vector256<double>.Zero).GetLower().AsVector4();
            (sum, total) = weight(tap) switch { var w => (sum + (w * sample), total + w) };
        }
        return sum / total;
    }
}

public static class PinkSpecks {
    public static PixelFrame Field(PixelExtent extent, double scale, double coverage, uint field) {
        const int frequencyPair = 2;
        static double Frequency(int index, int length) => (index <= length / 2 ? index : index - length) / (double)length;
        Size size = new(CvInvoke.GetOptimalDFTSize(int.Max(frequencyPair, extent.Width)), CvInvoke.GetOptimalDFTSize(int.Max(frequencyPair, extent.Height)));
        using Mat draw = new(size.Height, size.Width, DepthType.Cv64F, 1);
        using Mat pink = new(size.Height, size.Width, DepthType.Cv64F, 2);
        using Mat spectrum = new();
        double knee = 1d / scale;
        double reference = double.Hypot(1d / int.Max(size.Width, size.Height), knee);
        Span2D<double> cells = draw.View<double>();
        Span2D<Complex> gains = pink.View<Complex>();
        for (int y = 0; y < size.Height; y++) {
            for (int x = 0; x < size.Width; x++) {
                double gain = x is 0 && y is 0 ? 0d : reference / double.Hypot(double.Hypot(Frequency(x, size.Width), Frequency(y, size.Height)), knee);
                (cells[y, x], gains[y, x]) = (CoordinateHash.Normals(x, y, field).X, new Complex(gain, gain));
            }
        }
        CvInvoke.Dft(draw, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.Multiply(spectrum, pink, spectrum);
        CvInvoke.Dft(spectrum, draw, DxtType.Inverse | DxtType.RealOutput | DxtType.Scale, 0);
        double floor = ArrayStatistics.QuantileCustomInplace(draw.GetSpan<double>().ToArray(), 1d - coverage, QuantileDefinition.R7);
        draw.ConvertTo(draw, DepthType.Cv64F, 1d, -floor);
        _ = CvInvoke.Threshold(draw, draw, 0d, 0d, ThresholdType.ToZero);
        CvInvoke.Normalize(draw, draw, 0d, 1d, NormType.MinMax);
        PixelFrame specks = new(Point.Empty, extent, extent);
        Span2D<Vector4> plane = specks.View.Span;
        ReadOnlySpan2D<double> values = draw.View<double>();
        for (int y = 0; y < plane.Height; y++) {
            for (int x = 0; x < plane.Width; x++)
                plane[y, x] = new Vector4((float)values[y, x]);
        }
        return specks;
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

public static class Bokeh {
    public static Func<Mat, Mat, PixelFrame, Fin<Unit>> Uniform(Iris iris, double fringe, double radius) =>
        (light, _, _) => Correlate(light, iris, fringe, Vector256.Create(radius));

    public static Fin<Unit> Correlate(Mat image, Iris iris, double fringe, Vector256<double> radii) =>
        toSeq(Range(0, image.NumberOfChannels)).Traverse(channel => PixelSampling.KernelTaps<int>(radii[channel] * iris.Circumradius).Map(side => (Channel: channel, Side: side))).As().Map(kernels => {
            foreach ((int channel, int side) in kernels) {
                using Mat plane = new();
                using Mat kernel = new(side, side, DepthType.Cv32F, 1);
                Span2D<float> cells = kernel.View<float>();
                for (int y = 0; y < side; y++) {
                    for (int x = 0; x < side; x++)
                        cells[y, x] = (float)Tap(iris, fringe, iris.Outline(((side / 2) - x, (side / 2) - y)), radii[channel])[channel];
                }
                CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
                CvInvoke.ExtractChannel(image, plane, channel);
                CvInvoke.Filter2D(plane, plane, kernel, new Point(-1, -1), 0d, BorderType.Replicate);
                CvInvoke.InsertChannel(plane, image, channel);
            }
            return unit;
        });

    public static Func<Mat, Mat, PixelFrame, Fin<Unit>> Focal(FieldFrame field, double sharp, double feather, double aspect, int power, double cap) =>
        (_, radii, frame) => {
            Span2D<float> radius = radii.View<float>();
            for (int y = 0; y < radius.Height; y++) {
                for (int x = 0; x < radius.Width; x++) {
                    (double u, double v) = field.Local(((double)frame.Origin.X + x + 0.5d, (double)frame.Origin.Y + y + 0.5d));
                    radius[y, x] = (float)(cap * double.Pow(double.Clamp((double.Hypot(aspect * u, v) - sharp) / feather, 0d, 1d), power));
                }
            }
            return unit;
        };

    public static Option<Func<Mat, Mat, PixelFrame, Fin<Unit>>> Depth(PixelFrame depth, Camera camera, double fStop, double cap, PixelExtent extent) =>
        from lens in camera.Lens
        from window in camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None)
        let focus = lens.FocusDistance.Meters.ToDouble()
        let focal = lens.FocalLength.Meters.ToDouble()
        let k = focal * extent.ShortSide / (2d * fStop * (focus - focal))
        let span = double.Min(window.Width, window.Height)
        select new Func<Mat, Mat, PixelFrame, Fin<Unit>>((_, radii, frame) => {
            using Mat magnitude = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            Span2D<float> radius = radii.View<float>();
            Span2D<float> blurred = magnitude.View<float>();
            ReadOnlySpan2D<Vector4> distance = depth.View.Span;
            for (int y = 0; y < radius.Height; y++) {
                for (int x = 0; x < radius.Width; x++) {
                    (int u, int v) = ((int)long.Clamp((long)frame.Origin.X + x - depth.Origin.X, 0, distance.Width - 1), (int)long.Clamp((long)frame.Origin.Y + y - depth.Origin.Y, 0, distance.Height - 1));
                    (radius[y, x], blurred[y, x]) = (float)double.Clamp(k * (1d - (focus / distance[v, u].X)) / span, -cap, cap) switch { var r => (r, float.Abs(r)) };
                }
            }
            return magnitude.Blur(magnitude, cap).Map(_ => {
                Span<float> values = radii.GetSpan<float>();
                using SpanOwner<float> limits = SpanOwner<float>.Allocate(values.Length);
                TensorPrimitives.Abs(values, limits.Span);
                TensorPrimitives.Min(magnitude.GetSpan<float>(), limits.Span, limits.Span);
                TensorPrimitives.CopySign(limits.Span, values, values);
                return unit;
            });
        });

    public static PixelPass.Frame Gathered(Iris iris, double fringe, Func<Mat, Mat, PixelFrame, Fin<Unit>> field) =>
        new((frame, progress) => frame.Filter(_ => {
            using Mat source = frame.Premultiplied();
            using Mat radii = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            return field(source, radii, frame).Bind(_ => {
                radii.MinMax(out double[] least, out double[] most, out Point[] _, out Point[] _);
                (float circumradius, float near) = (iris.Circumradius, float.Max(0f, -(float)least[0]));
                int side = Window(float.Max(near, (float)most[0]) * circumradius);
                (double Reach, double Edge)[] outlines = [.. from y in Enumerable.Range(0, side) from x in Enumerable.Range(0, side) select iris.Outline(((side / 2) - x, (side / 2) - y))];
                return new PixelPass.Pointwise((row, _, line) => {
                    ReadOnlySpan2D<Vector4> light = source.View<Vector4>();
                    ReadOnlySpan2D<float> radius = radii.View<float>();
                    ReadOnlySpan2D<(double Reach, double Edge)> shapes = new(outlines, side, side);
                    int y = frame.RowIndex(line);
                    for (int x = 0; x < row.Length; x++) {
                        float own = float.Abs(radius[y, x]);
                        int reach = Window(float.Max(own, near) * circumradius) / 2;
                        (Vector256<double> sum, Vector256<double> total) = (Vector256<double>.Zero, Vector256<double>.Zero);
                        for (int dy = -reach; dy <= reach; dy++) {
                            for (int dx = -reach; dx <= reach; dx++) {
                                (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                                Vector256<double> weights = Tap(iris, fringe, shapes[dy + (side / 2), dx + (side / 2)], float.Abs(float.Min(radius[v, u], own)));
                                Vector4 pixel = light[v, u];
                                (sum, total) = (sum + (weights * Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W)), total + weights);
                            }
                        }
                        row[x] = Vector256.Narrow(sum / total, Vector256<double>.Zero).GetLower().AsVector4().Unassociated;
                    }
                }).Run(frame, progress);
            });
        }));

    private static Vector256<double> Tap(Iris iris, double fringe, (double Reach, double Edge) outline, double radius) =>
        outline.Reach is 0d ? Vector256.Create(1d) : iris.Weights(outline.Reach, outline.Edge, radius, 1d, fringe);

    private static int Window(float reach) => (2 * (int)float.Ceiling(reach)) + 1;
}
