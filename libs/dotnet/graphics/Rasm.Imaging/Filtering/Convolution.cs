using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using PeterO.Numbers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using SixLabors.ImageSharp.PixelFormats;

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
        double edge = Boundary(double.Atan2(-y, x));
        return (double.Hypot(x, y) / edge, edge);
    }

    public Func<(double X, double Y), double> Transmission(double radius) {
        (double obstruction, double squeeze, double circumradius) = (Obstruction, Squeeze, Circumradius);
        double margin = double.Hypot(squeeze, 1d) / (2d * circumradius * double.Cos(double.Pi / Blades));
        double[] blades = [.. Enumerable.Range(0, Blades).Select(i => (((-Rotation - (double.Tau * i / Blades)) % double.Tau) + double.Tau) % double.Tau)];
        return point => {
            (double x, double y) = (squeeze * point.X, point.Y);
            double reach = double.Hypot(x, y) / Boundary(double.Atan2(-y, x));
            if (reach - margin >= radius || reach + margin <= obstruction * radius)
                return 0d;
            if (reach + margin <= radius && reach - margin >= obstruction * radius)
                return 1d / (1d - obstruction);
            (double left, double right, double top, double bottom) = (x - (squeeze / 2d), x + (squeeze / 2d), y - 0.5d, y + 0.5d);
            double[] angles = [0d, double.Pi / 2d, double.Pi, 3d * double.Pi / 2d, double.Tau, .. blades, Bearing(left, top), Bearing(left, bottom), Bearing(right, top), Bearing(right, bottom)];
            System.Array.Sort(angles);
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
                double edge = radius * Boundary(-angle);
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
        Vector256<double> r = Radii(radius, fringe);
        double[] areas = [.. Enumerable.Range(0, 3).Select(component => r[component] is 0d || Obstruction is 1d ? 0d
            : Blades / Squeeze * Integrate.GaussKronrod(angle => {
                double boundary = r[component] * Boundary(angle);
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

    private double Boundary(double angle) {
        (int n, double kappa) = (Blades, Roundness);
        (double s1, double c1) = double.SinCos(double.Pi / n);
        (double sa, double ca) = double.SinCos(double.Ieee754Remainder(angle - Rotation - (double.Pi / n), double.Tau / n));
        double root = double.Sqrt(1d - (kappa * kappa * s1 * s1));
        double delta = root - (kappa * c1);
        return Circumradius * ((2d * c1 * root) - (kappa * (1d - (2d * s1 * s1)))) / ((delta * ca) + double.Sqrt(1d - (delta * delta * sa * sa)));
    }

    private static Vector256<double> Radii(double radius, double fringe) =>
        Vector256.Create(radius * (1d - double.Max(fringe, 0d)), radius * (1d - (double.Abs(fringe) / 2d)), radius * (1d + double.Min(fringe, 0d)), 0d);

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref int blades, ref double rotation, ref double roundness, ref double obstruction, ref double squeeze) =>
        validationError = blades >= 3 && double.IsFinite(rotation) && roundness is >= 0d and <= 1d && obstruction is >= 0d and <= 1d && double.IsFinite(squeeze) && squeeze > 0d ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture,
                $"Iris of {blades} blades must have at least 3, roundness {roundness} and obstruction {obstruction} in [0, 1], finite rotation {rotation} and positive squeeze {squeeze}"));
}

[ComplexValueObject]
[ValidationError<ImagingFault>]
public readonly partial struct HighlightKey {
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
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Highlight threshold {threshold} and softness {softness} must be finite and non-negative"));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed unsafe class PointSpread : VectorOfMat {
    private readonly Arr<int> planes;

    private PointSpread(Arr<int> planes, Action<PointSpread> fill) : base(planes.Fold(0, int.Max) + 1) {
        this.planes = planes;
        fill(this);
    }

    public static PointSpread Veiling(PixelExtent size, double degreesPerPixel) => new([0, 0, 0], spread => {
        using Mat kernel = spread[0];
        kernel.Create((2 * size.Height) - 1, (2 * size.Width) - 1, DepthType.Cv32F, 1);
        Span<float> cells = new((void*)kernel.DataPointer, kernel.Rows * kernel.Cols);
        for (int i = 0; i < cells.Length; i++)
            cells[i] = (float)Spencer(degreesPerPixel * int.DivRem(i, kernel.Cols) switch { var (y, x) => double.Hypot(x - (size.Width - 1), y - (size.Height - 1)) });
        kernel.ConvertTo(kernel, DepthType.Cv32F, 1d / CvInvoke.Sum(kernel).V0);

        static double Spencer(double degrees) =>
            (0.384d * 2.61e6d * double.Exp(-double.Pow(degrees / 0.02d, 2d))) + (0.478d * 20.91d / double.Pow(degrees + 0.02d, 3d)) + (0.138d * 72.37d / double.Pow(degrees + 0.02d, 2d));
    });

    public static PointSpread Diffraction(Iris pupil, double airy, PixelExtent size, PixelExtent canvas, Gamut working) => new([0, 1, 2], spread => {
        const decimal reference = 550m;
        const decimal firstZero = 1.22m;
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
        for (int y = 0; y < grid; y++) {
            Span<double> cells = new((void*)energy.GetDataPointer(y), grid);
            for (int x = 0; x < cells.Length; x++)
                cells[x] = (1 - (2 * ((x + y) & 1))) * transmission((x - middle + 0.5d, y - middle + 0.5d));
        }
        CvInvoke.Dft(energy, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.MulSpectrums(spectrum, spectrum, power, MulSpectrumsType.Default, conjB: true);
        CvInvoke.ExtractChannel(power, energy, 0);
        for (int component = 0; component < spread.Size; component++) {
            using Mat plane = spread[component];
            plane.Create(kernel.Height, kernel.Width, DepthType.Cv32F, 1);
            plane.SetTo(default(MCvScalar));
        }
        ReadOnlySpan2D<double> samples = new((void*)energy.DataPointer, grid, grid, 0);
        Span<double> light = new((void*)dilated.DataPointer, kernel.Width * kernel.Height);
        foreach ((float wavelength, Vector3 matching) in rows) {
            ERational step = ERational.FromDecimal(reference * firstZero).Multiply(ERational.FromSingle(pupil.Circumradius))
                .Divide(ERational.FromDouble(airy)).Divide(ERational.FromSingle(wavelength)).Divide(grid);
            (double Phase, double Footprint)[] across = [.. Enumerable.Range(0, kernel.Width).Select(x => Cell(step, x - center.X))];
            (double Phase, double Footprint)[] down = [.. Enumerable.Range(0, kernel.Height).Select(y => Cell(step, y - center.Y))];
            for (int i = 0; i < light.Length; i++) {
                (int y, int x) = int.DivRem(i, kernel.Width);
                double cell = across[x].Footprint * down[y].Footprint;
                light[i] = cell * cell * samples.Sample(
                    (middle + (grid * across[x].Phase) + 0.5d, middle + (grid * down[y].Phase) + 0.5d),
                    (WrapMode.Periodic, WrapMode.Periodic), 0d, static (sum, pixel, weight) => sum + (pixel * weight));
            }
            dilated.ConvertTo(normalized, DepthType.Cv32F, 1d / CvInvoke.Sum(dilated).V0);
            for (int component = 0; component < 3; component++) {
                using Mat plane = spread[component];
                CvInvoke.ScaleAdd(normalized, (double)matching[component] / total[component], plane, plane);
            }
        }

        static (double Phase, double Footprint) Cell(ERational step, int offset) {
            if (offset is 0)
                return (0d, 1d);
            ERational frequency = step.Multiply(int.Abs(offset));
            ERational center = ERational.Create(1, 2);
            double phase = frequency.Add(center).Remainder(ERational.One).Subtract(center).ToDouble();
            return (offset < 0 ? -phase : phase, double.SinPi(phase) / (double.Pi * frequency.ToDouble()));
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

public static unsafe class LightSpread {
    private static readonly Progress<int> Unreported = new();

    public static Vector4 Light(Vector4 pixel) => new(pixel.AsVector3() * pixel.W, 0f);

    extension(PixelFrame frame) {
        public Fin<Unit> Spread(Func<Vector4, Vector4> fill, Func<Mat, Fin<Unit>> spread, BlendingMode mode, float weight, IProgress<int> progress) => Try.lift(() => {
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
                mode.Mixed(row, layer, weights.Span);
                layer.CopyTo(row);
            }).Run(frame, progress));

            Span<Vector4> LightRow(long line) => new((void*)light.GetDataPointer(frame.RowIndex(line)), frame.Size.Width);
        }).Run();
    }
}

public static unsafe class TentPyramid {
    public static Unit Spread(Mat light, int halvings) {
        using Mat tent = new(1, 3, DepthType.Cv32F, 1);
        tent.SetTo([0.25f, 0.5f, 0.25f]);
        Chained(light, halvings, static tap => 1d / (1d + double.Max(0d, double.Max(tap.X, double.Max(tap.Y, tap.Z)))), tent);
        return unit;
    }

    public static Unit Soiled(Mat light, PixelFrame frame, PixelFrame dirt, float gain) {
        using Mat lens = dirt.Header();
        double cover = double.Max((double)frame.Extent.Width / lens.Cols, (double)frame.Extent.Height / lens.Rows);
        using Mat fitted = new();
        CvInvoke.Resize(lens, fitted, default, cover, cover, cover < 1d ? Inter.Area : Inter.Linear);
        ReadOnlySpan2D<Vector4> soil = new((void*)fitted.DataPointer, fitted.Rows, fitted.Cols, 0);
        Span<Vector4> cells = new((void*)light.DataPointer, frame.Block.Length / 4);
        for (int i = 0; i < cells.Length; i++) {
            (int y, int x) = int.DivRem(i, light.Cols);
            cells[i] *= Vector4.One + (gain * soil.Sample(
                (((fitted.Cols - (double)frame.Extent.Width) / 2d) + frame.Origin.X + x + 0.5d, ((fitted.Rows - (double)frame.Extent.Height) / 2d) + frame.Origin.Y + y + 0.5d),
                (WrapMode.Black, WrapMode.Black), Vector4.Zero, static (sum, pixel, weight) => sum + ((float)weight * pixel)));
        }
        return unit;
    }

    private static void Chained(Mat level, int halvings, Func<Vector4, double> weight, Mat tent) {
        if (halvings <= 0)
            return;
        using Mat next = new(level.Rows / 2, level.Cols / 2, DepthType.Cv32F, 4);
        using Mat up = new();
        ReadOnlySpan2D<Vector4> taps = new((void*)level.DataPointer, level.Rows, level.Cols, 0);
        Span<Vector4> cells = new((void*)next.DataPointer, next.Rows * next.Cols);
        for (int n = 0; n < cells.Length; n++) {
            (int y, int x) = int.DivRem(n, next.Cols);
            (double u, double v) = ((x + 0.5d) * level.Cols / next.Cols, (y + 0.5d) * level.Rows / next.Rows);
            Vector256<double> sum = (0.5d * Group(taps, u, v, weight)) + (0.125d * (Group(taps, u - 1d, v - 1d, weight) + Group(taps, u + 1d, v - 1d, weight)
                + Group(taps, u - 1d, v + 1d, weight) + Group(taps, u + 1d, v + 1d, weight)));
            cells[n] = Vector256.Narrow(sum, Vector256<double>.Zero).GetLower().AsVector4();
        }
        Chained(next, halvings - 1, static _ => 1d, tent);
        CvInvoke.Resize(next, up, level.Size, 0d, 0d, Inter.Linear);
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

public static unsafe class PinkSpecks {
    public static PixelFrame Field(PixelExtent extent, double scale, double coverage, uint field) {
        const int frequencyPair = 2;
        static double Frequency(int index, int length) => (index <= length / 2 ? index : index - length) / (double)length;
        Size size = new(CvInvoke.GetOptimalDFTSize(int.Max(frequencyPair, extent.Width)), CvInvoke.GetOptimalDFTSize(int.Max(frequencyPair, extent.Height)));
        using Mat draw = new(size.Height, size.Width, DepthType.Cv64F, 1);
        using Mat pink = new(size.Height, size.Width, DepthType.Cv64F, 2);
        using Mat spectrum = new();
        double knee = 1d / scale;
        double reference = double.Hypot(1d / int.Max(size.Width, size.Height), knee);
        Span<double> cells = new((void*)draw.DataPointer, size.Width * size.Height);
        Span<Complex> gains = new((void*)pink.DataPointer, cells.Length);
        for (int i = 0; i < cells.Length; i++) {
            (int y, int x) = int.DivRem(i, size.Width);
            double gain = x is 0 && y is 0 ? 0d : reference / double.Hypot(double.Hypot(Frequency(x, size.Width), Frequency(y, size.Height)), knee);
            (cells[i], gains[i]) = (CoordinateHash.Normals(x, y, field).X, new Complex(gain, gain));
        }
        CvInvoke.Dft(draw, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.Multiply(spectrum, pink, spectrum);
        CvInvoke.Dft(spectrum, draw, DxtType.Inverse | DxtType.RealOutput | DxtType.Scale, 0);
        double floor = ArrayStatistics.QuantileCustomInplace(cells.ToArray(), 1d - coverage, QuantileDefinition.R7);
        draw.ConvertTo(draw, DepthType.Cv64F, 1d, -floor);
        _ = CvInvoke.Threshold(draw, draw, 0d, 0d, ThresholdType.ToZero);
        CvInvoke.Normalize(draw, draw, 0d, 1d, NormType.MinMax);
        return new PixelFrame(Point.Empty, extent, extent, block => {
            ReadOnlySpan2D<double> values = new((void*)draw.DataPointer, size.Height, size.Width, 0);
            Span<Vector4> plane = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int i = 0; i < plane.Length; i++)
                plane[i] = int.DivRem(i, extent.Width) switch { var (y, x) => new Vector4((float)values[y, x]) };
        });
    }
}

public static unsafe class GhostReflections {
    private static readonly Arr<Vector3> Modulated = [Vector3.Zero, new(0f, 1f, 1f), new(1f, 1f, 0f), new(1f, 0f, 1f)];

    public static Fin<Unit> Reflected(Mat key, PixelFrame frame, float radius, int iterations, float modulation) {
        using Mat big = new();
        using Mat input = Mat.Zeros(key.Rows, key.Cols, DepthType.Cv32F, 4);
        return key.Blur(big, 2f * radius).Bind(_ => key.Blur(key, radius)).Map(_ => {
            Accumulate(key, input, frame, [(2.13f, Vector4.One)]);
            Accumulate(big, input, frame, [(-0.97f, Vector4.One)]);
            key.SetTo(default(MCvScalar));
            for (int i = 1; i < iterations; i++) {
                Accumulate(input, key, frame, Copies(i, iterations, modulation));
                key.CopyTo(input);
            }
            return unit;
        });
    }

    public static (float Scale, Vector4 Gain)[] Copies(int iteration, int count, float modulation) =>
        [.. Enumerable.Range(0, Modulated.Count).Select(k => (
            (2.1f * (1f - (((4 * iteration) + k + (count % 2 is 1 ? 0.5f : 0f)) / (4f * count)))) switch { var s => k % 2 is 1 ? -0.99f / s : s },
            new Vector4((Vector3.One - (Modulated[k] * modulation)) / Modulated.Count, 0f)))];

    private static void Accumulate(Mat source, Mat target, PixelFrame frame, ReadOnlySpan<(float Scale, Vector4 Gain)> copies) {
        ReadOnlySpan2D<Vector4> from = new((void*)source.DataPointer, source.Rows, source.Cols, 0);
        Span<Vector4> to = new((void*)target.DataPointer, frame.Block.Length / 4);
        (double x, double y) = (frame.Extent.Width / 2d, frame.Extent.Height / 2d);
        for (int n = 0; n < to.Length; n++) {
            (double u, double v) = int.DivRem(n, target.Cols) switch { var (row, column) => ((double)frame.Origin.X + column + 0.5d - x, (double)frame.Origin.Y + row + 0.5d - y) };
            double reach = double.Hypot(u / x, v / y);
            foreach ((float scale, Vector4 gain) in copies)
                to[n] += from.Sample((x + (u * scale), y + (v * scale)), (WrapMode.Black, WrapMode.Black), Vector4.Zero,
                    static (sum, pixel, weight) => sum + ((float)weight * pixel), Some((frame.Origin, frame.Extent))) * gain * (float)double.Max(0d, 1d - (reach * float.Abs(scale)));
        }
    }
}

public static unsafe class Bokeh {
    public static Func<Mat, Mat, PixelFrame, Fin<Unit>> Uniform(Iris iris, double fringe, double radius) =>
        (light, _, _) => Correlate(light, iris, fringe, Vector256.Create(radius));

    public static Fin<Unit> Correlate(Mat image, Iris iris, double fringe, Vector256<double> radii) =>
        toSeq(Range(0, image.NumberOfChannels)).Traverse(channel => PixelSampling.KernelTaps<int>(ERational.FromDouble(radii[channel]).Multiply(ERational.FromSingle(iris.Circumradius))).Map(side => (Channel: channel, Side: side))).As().Bind(kernels => Try.lift(() => {
            foreach ((int channel, int side) in kernels) {
                using Mat plane = new();
                using Mat kernel = new(side, side, DepthType.Cv32F, 1);
                for (int y = 0; y < side; y++) {
                    Span<float> cells = new((void*)kernel.GetDataPointer(y), side);
                    for (int x = 0; x < cells.Length; x++)
                        cells[x] = (float)Tap(iris, fringe, iris.Outline(((side / 2) - x, (side / 2) - y)), radii[channel])[channel];
                }
                CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
                CvInvoke.ExtractChannel(image, plane, channel);
                CvInvoke.Filter2D(plane, plane, kernel, new Point(-1, -1), 0d, BorderType.Replicate);
                CvInvoke.InsertChannel(plane, image, channel);
            }
            return unit;
        }).Run());

    public static Func<Mat, Mat, PixelFrame, Fin<Unit>> Focal(FieldFrame field, double sharp, double feather, double aspect, int power, double cap) =>
        (_, radii, frame) => {
            Span<float> radius = new((void*)radii.DataPointer, frame.Block.Length / 4);
            for (int i = 0; i < radius.Length; i++) {
                (double u, double v) = field.Local(((double)frame.Origin.X + (i % frame.Size.Width) + 0.5d, (double)frame.Origin.Y + (i / frame.Size.Width) + 0.5d));
                radius[i] = (float)(cap * double.Pow(double.Clamp((double.Hypot(aspect * u, v) - sharp) / feather, 0d, 1d), power));
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
            Span<float> radius = new((void*)radii.DataPointer, frame.Block.Length / 4);
            using Mat magnitude = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            ReadOnlySpan2D<Vector4> distance = depth.View.Span;
            Span<float> blurred = new((void*)magnitude.DataPointer, radius.Length);
            for (int i = 0; i < radius.Length; i++) {
                (int y, int x) = int.DivRem(i, frame.Size.Width);
                (int u, int v) = ((int)long.Clamp((long)frame.Origin.X + x - depth.Origin.X, 0, distance.Width - 1), (int)long.Clamp((long)frame.Origin.Y + y - depth.Origin.Y, 0, distance.Height - 1));
                (radius[i], blurred[i]) = (float)double.Clamp(k * (1d - (focus / distance[v, u].X)) / span, -cap, cap) switch { var r => (r, float.Abs(r)) };
            }
            return magnitude.Blur(magnitude, cap).Map(_ => {
                Span<float> values = new((void*)radii.DataPointer, frame.Block.Length / 4);
                ReadOnlySpan<float> smooth = new((void*)magnitude.DataPointer, values.Length);
                for (int i = 0; i < values.Length; i++)
                    values[i] = float.CopySign(float.Min(smooth[i], float.Abs(values[i])), values[i]);
                return unit;
            });
        });

    public static PixelPass.Frame Gathered(Iris iris, double fringe, Func<Mat, Mat, PixelFrame, Fin<Unit>> field) =>
        new((frame, progress) => Try.lift(() => {
            using Mat source = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            using Mat radii = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            PixelOperations<RgbaVector>.Instance.ToVector4(SixLabors.ImageSharp.Configuration.Default, frame.Block.Cast<float, RgbaVector>(), new Span<Vector4>((void*)source.DataPointer, frame.Block.Length / 4), PixelConversionModifiers.Premultiply);
            return field(source, radii, frame).Bind(delegate {
                radii.MinMax(out double[] least, out double[] most, out _, out _);
                (float circumradius, float near) = (iris.Circumradius, float.Max(0f, -(float)least[0]));
                int side = Window(float.Max(near, (float)most[0]) * circumradius);
                (double Reach, double Edge)[] outlines = [.. Range(0, side * side).Select(i => iris.Outline(((side / 2) - (i % side), (side / 2) - (i / side))))];
                return new PixelPass.Pointwise((row, _, line) => {
                    ReadOnlySpan2D<Vector4> light = new((void*)source.DataPointer, source.Rows, source.Cols, 0);
                    ReadOnlySpan2D<float> radius = new((void*)radii.DataPointer, radii.Rows, radii.Cols, 0);
                    int y = frame.RowIndex(line);
                    for (int x = 0; x < row.Length; x++) {
                        float own = float.Abs(radius[y, x]);
                        (Vector256<double> sum, Vector256<double> total) = (Vector256<double>.Zero, Vector256<double>.Zero);
                        for (int span = Window(float.Max(own, near) * circumradius), k = 0; k < span * span; k++) {
                            (int dy, int dx) = ((k / span) - (span / 2), (k % span) - (span / 2));
                            (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                            Vector256<double> weights = Tap(iris, fringe, outlines[((dy + (side / 2)) * side) + dx + (side / 2)], float.Abs(float.Min(radius[v, u], own)));
                            Vector4 pixel = light[v, u];
                            (sum, total) = (sum + (weights * Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W)), total + weights);
                        }
                        row[x] = Vector256.Narrow(sum / total, Vector256<double>.Zero).GetLower().AsVector4() switch { { W: > 0f } mean => new(mean.AsVector3() / mean.W, mean.W), _ => Vector4.Zero };
                    }
                }).Run(frame, progress);
            });
        }).Run());

    private static Vector256<double> Tap(Iris iris, double fringe, (double Reach, double Edge) outline, double radius) =>
        outline.Reach is 0d ? Vector256.Create(1d) : iris.Weights(outline.Reach, outline.Edge, radius, 1d, fringe);

    private static int Window(float reach) => (2 * (int)float.Ceiling(reach)) + 1;
}
