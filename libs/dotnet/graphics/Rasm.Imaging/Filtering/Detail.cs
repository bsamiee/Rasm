using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filtering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static unsafe class Sharpening {
    public static Fin<Unit> Laplacian(PixelFrame frame, float amount, double exposure, (double R, double G, double B) luminance, IProgress<int> progress) => frame.Filter(header => {
        int width = frame.Size.Width;
        using Mat linear = new();
        using Mat light = new(frame.Size.Height, width, DepthType.Cv64F, 4);
        using Mat weights = new(2, 4, DepthType.Cv64F, 1);
        using Mat laplacian = new();
        using Mat luma = new();
        using Mat rise = new();
        using Mat cross = CvInvoke.GetStructuringElement(MorphShapes.Cross, new Size(3, 3), new Point(-1, -1));
        header.ConvertTo(linear, DepthType.Cv64F);
        CvInvoke.MixChannels(linear, light, [3, 0, 3, 1, 3, 2, 3, 3]);
        CvInvoke.Multiply(linear, light, light);
        CvInvoke.Laplacian(light, laplacian, DepthType.Cv64F, 1, 1d, 0d, BorderType.Replicate);
        weights.SetTo([luminance.R, luminance.G, luminance.B, 0d, -luminance.R, -luminance.G, -luminance.B, 0d]);
        CvInvoke.Transform(light, luma, weights);
        CvInvoke.Dilate(luma, rise, cross, new Point(-1, -1), 1, BorderType.Replicate, default);
        CvInvoke.Subtract(rise, luma, rise);
        return new PixelPass.Pointwise((row, _, line) => {
            int start = frame.RowIndex(line) * width;
            ReadOnlySpan<Vector256<double>> deltas = new Span<Vector256<double>>(laplacian.DataPointer.ToPointer(), laplacian.Rows * laplacian.Cols).Slice(start, width);
            ReadOnlySpan<Vector128<double>> steps = new Span<Vector128<double>>(rise.DataPointer.ToPointer(), rise.Rows * rise.Cols).Slice(start, width);
            for (int x = 0; x < row.Length; x++) {
                if (row[x].W <= 0f) continue;
                double factor = double.Max(1d - double.Exp2(exposure + double.Log2(double.Max(steps[x].GetElement(0), steps[x].GetElement(1)))), 0d) * amount / 6d / row[x].W;
                row[x] = new((float)(row[x].X - (factor * deltas[x].GetElement(0))), (float)(row[x].Y - (factor * deltas[x].GetElement(1))), (float)(row[x].Z - (factor * deltas[x].GetElement(2))), row[x].W);
            }
        }).Run(frame, progress);
    });
}

public static unsafe class ContrastGrid {
    private const float LogFloor = -8f;
    private const float Spacing = 12f / 31f;

    public static Fin<Unit> Graded(PixelFrame frame, float highlights, float shadows, float detail, double cell, double surround, (double R, double G, double B) luminance, IProgress<int> progress) {
        int pitch = (int)double.Ceiling(cell);
        return (Spread(cell), Spread(surround)).Apply((fine, broad) => Filter(frame, highlights, shadows, detail, pitch, fine, broad, luminance, progress)).As().Flatten();

        Fin<(double Sigma, int Taps)> Spread(double sigma) =>
            (double.Sqrt(double.Max((sigma * sigma) - ((double)pitch * pitch / 4d), 0d)) / pitch) switch { var spread => PixelSampling.GaussianTaps<int>(spread).Map(taps => (spread, taps)) };
    }

    private static Fin<Unit> Filter(
        PixelFrame frame, float highlights, float shadows, float detail, int pitch, (double Sigma, int Taps) fine, (double Sigma, int Taps) broad, (double R, double G, double B) luminance, IProgress<int> progress) =>
        frame.Filter(header => {
            (int width, double gray, (WrapMode, WrapMode) edge) = (frame.Size.Width, double.Log2(Exposure.MiddleGray), (WrapMode.Clamp, WrapMode.Clamp));
            (int across, int down) = ((width + pitch - 1) / pitch, (frame.Size.Height + pitch - 1) / pitch);
            using Mat linear = new();
            using Mat weights = new(1, 4, DepthType.Cv64F, 1);
            using Mat luma = new();
            using Mat alpha = new();
            using Mat mask = new();
            using ScalarArray zero = new(0d);
            weights.SetTo([luminance.R, luminance.G, luminance.B, 0d]);
            header.ConvertTo(linear, DepthType.Cv64F);
            CvInvoke.Transform(linear, luma, weights);
            CvInvoke.ExtractChannel(linear, alpha, 3);
            CvInvoke.Compare(alpha, zero, mask, CmpType.GreaterThan);
            CvInvoke.MinMaxIdx(luma, out double low, out double high, minIdx: null, maxIdx: null, mask);
            (int first, int slices) = Place(low, 1d, 0).Slice switch { var lowest => (lowest, Place(high, 1d, 0).Slice - lowest + 2) };
            int cellCount = down * across;
            using VectorOfMat lattice = new(slices);
            Mat[] planes = [.. Enumerable.Range(0, slices).Select(index => lattice[index])];
            foreach (Mat plane in planes) {
                plane.Create(down, across, DepthType.Cv64F, 4);
                new Span<Vector256<double>>(plane.DataPointer.ToPointer(), cellCount).Clear();
            }
            using Mat cells = Mat.Zeros(down, across, DepthType.Cv64F, 4);
            ReadOnlySpan<double> lumas = new(luma.DataPointer.ToPointer(), luma.Rows * luma.Cols);
            ReadOnlySpan<double> coverage = new(alpha.DataPointer.ToPointer(), alpha.Rows * alpha.Cols);
            Span2D<Vector256<double>> marginals = new Span<Vector256<double>>(cells.DataPointer.ToPointer(), cellCount).AsSpan2D(down, across);
            for (int index = 0; index < lumas.Length; index++) {
                if (coverage[index] <= 0f) continue;
                ((Vector256<double> sample, int slice, double upper), int cellY, int cellX) = (Place(lumas[index], coverage[index], first), index / width / pitch, index % width / pitch);
                new Span<Vector256<double>>(planes[slice].DataPointer.ToPointer(), cellCount)[(cellY * across) + cellX] += (1d - upper) * sample;
                new Span<Vector256<double>>(planes[slice + 1].DataPointer.ToPointer(), cellCount)[(cellY * across) + cellX] += upper * sample;
                marginals[cellY, cellX] += sample;
            }
            foreach (Mat plane in planes)
                CvInvoke.GaussianBlur(plane, plane, new Size(fine.Taps, fine.Taps), fine.Sigma, fine.Sigma, BorderType.Constant);
            CvInvoke.GaussianBlur(cells, cells, new Size(broad.Taps, broad.Taps), broad.Sigma, broad.Sigma, BorderType.Constant);
            return new PixelPass.Pointwise((row, _, line) => {
                int y = frame.RowIndex(line);
                ReadOnlySpan<double> rowLumas = new Span<double>(luma.DataPointer.ToPointer(), luma.Rows * luma.Cols).Slice(y * width, width);
                ReadOnlySpan2D<Vector256<double>> blurred = new Span<Vector256<double>>(cells.DataPointer.ToPointer(), cellCount).AsSpan2D(down, across);
                for (int x = 0; x < row.Length; x++) {
                    if (row[x].W <= 0f) continue;
                    ((Vector256<double> own, int slice, double upper), (double X, double Y) at) = (Place(rowLumas[x], 1d, first), ((x + 0.5d) / pitch, (y + 0.5d) / pitch));
                    Vector256<double> graded = ((1d - upper) * new ReadOnlySpan<Vector256<double>>(planes[slice].DataPointer.ToPointer(), cellCount).AsSpan2D(down, across).Sample(at, edge, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel * weight)))
                        + (upper * new ReadOnlySpan<Vector256<double>>(planes[slice + 1].DataPointer.ToPointer(), cellCount).AsSpan2D(down, across).Sample(at, edge, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel * weight)));
                    double around = blurred.Sample(at, edge, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel * weight)) switch { var smooth => smooth.GetElement(0) / smooth.GetElement(1) };
                    double centered = double.Lerp(graded.GetElement(1) > 0d ? graded.GetElement(0) / graded.GetElement(1) : around, around, 0.6d) - gray;
                    double gain = double.Exp2(gray + (centered * (centered > 0d ? highlights : shadows)) + ((own.GetElement(0) - gray - centered) * detail) - own.GetElement(0));
                    row[x] = new((float)(row[x].X * gain), (float)(row[x].Y * gain), (float)(row[x].Z * gain), row[x].W);
                }
            }).Run(frame, progress);
        });

    private static (Vector256<double> Sample, int Slice, double Upper) Place(double luma, double weight, int first) =>
        double.Log2(double.Max(luma, LuminanceSamples.Floor)) switch {
            var level => ((level - LogFloor) / Spacing) switch { var stop => (int)double.Floor(stop) switch { var lower => (weight * Vector256.Create(level, 1d, 0d, 0d), lower - first, stop - lower) } },
        };
}

public static class GrainNoise {
    public static Vector4 Mix(float chroma) => new(new Vector3(MathF.Sqrt(chroma)), MathF.Sqrt(1f - chroma));

    public static Action<Span<Vector4>, long, long> White(uint field) =>
        (row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = CoordinateHash.Normals(column + x, line, field);
        };

    public static Action<Span<Vector4>, long, long> Tiled(Memory2D<Vector4> tile, uint field) {
        Vector2 random = CoordinateHash.White(Vector4.Zero, field).AsVector2();
        (double X, double Y) = ((double)random.X * tile.Width, -(double)random.Y * tile.Height);
        return (row, column, line) => {
            ReadOnlySpan2D<Vector4> texels = tile.Span;
            for (int x = 0; x < row.Length; x++)
                row[x] = texels.Sample((column + x + 0.5d + X, -line - 0.5d + Y), (WrapMode.Periodic, WrapMode.Periodic), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight));
        };
    }

    public static Fin<Unit> Shaped(Mat draws, double sigma) => Gaussian.Kernel(sigma).Map(kernel => {
        const double unitVariance = 1d;
        using (kernel) {
            using Mat squared = new();
            using Mat filtered = new();
            using Mat deviation = new(draws.Rows, draws.Cols, DepthType.Cv64F, draws.NumberOfChannels);
            CvInvoke.Multiply(kernel, kernel, squared);
            deviation.SetTo(new MCvScalar(unitVariance, unitVariance, unitVariance, unitVariance));
            CvInvoke.SepFilter2D(draws, filtered, DepthType.Cv64F, kernel, kernel, new Point(-1, -1), 0d, BorderType.Constant);
            CvInvoke.SepFilter2D(deviation, deviation, DepthType.Cv64F, squared, squared, new Point(-1, -1), 0d, BorderType.Constant);
            CvInvoke.Sqrt(deviation, deviation);
            CvInvoke.Divide(filtered, deviation, filtered);
            filtered.ConvertTo(draws, DepthType.Cv32F);
            return unit;
        }
    });

    public static Fin<Memory2D<Vector4>> Scores(PixelFrame scan, double footprint) {
        (double width, double height) = (double.Ceiling(footprint), double.Ceiling(scan.Size.Height * footprint / scan.Size.Width));
        return width * height > System.Array.MaxLength
            ? Error.New(string.Create(CultureInfo.InvariantCulture, $"Grain scan tile {width}x{height} exceeds {System.Array.MaxLength} texels"))
            : Try.lift(() => {
                Size dimensions = new((int)width, (int)height);
                int pixels = dimensions.Width * dimensions.Height;
                using Mat header = scan.Header();
                using Mat source = new();
                using Mat resized = new();
                using Mat mixed = new();
                using Mat mean = new(4, 4, DepthType.Cv64F, 1);
                const double third = 1d / 3d;
                header.ConvertTo(source, DepthType.Cv64F);
                CvInvoke.Resize(source, resized, dimensions, 0d, 0d, footprint < header.Cols ? Inter.Area : Inter.Linear);
                mean.SetTo([1d, 0d, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 0d, 1d, 0d, third, third, third, 0d]);
                CvInvoke.Transform(resized, mixed, mean);
                float[][] lanes = [.. Enumerable.Range(0, 4).Select(lane => {
                    using Mat channel = new();
                    CvInvoke.ExtractChannel(mixed, channel, lane);
                    double[] values = new double[pixels];
                    channel.CopyTo(values);
                    return values.Ranks(RankDefinition.Min).Zip(ArrayStatistics.RanksInplace(values, RankDefinition.Max), (lower, upper) => (float)Normal.InvCDF(0d, 1d, (lower + upper - 1d) / (2d * pixels))).ToArray();
                })];
                Vector4[] scores = [.. Enumerable.Range(0, pixels).Select(texel => new Vector4(lanes[0][texel], lanes[1][texel], lanes[2][texel], lanes[3][texel]))];
                return scores.AsMemory().AsMemory2D(dimensions.Height, dimensions.Width);
            }).Run();
    }

    public static Func<double, double> Bands((float Shadows, float Midtones, float Highlights) weights, double edge, double span, double width, double exposure) {
        double scale = width > 0d ? width : double.Max(edge, span);
        return luma => {
            double stops = double.Log2(double.Max(luma, 0d)) + exposure;
            double dark = edge is 0d ? luma > 0d ? 0d : 1d : 1d - ShapeEdge.Smooth(double.Clamp(double.Exp2(stops - double.Log2(edge)), 0d, 1d));
            double distance = scale is 0d ? luma : double.Exp2(stops - double.Log2(scale)) - (edge / scale) - (span / scale);
            double bright = width is 0d ? distance > 0d ? 1d : 0d : ShapeEdge.Smooth(double.Clamp(distance, 0d, 1d));
            return (weights.Shadows * dark) + (weights.Midtones * (1d - dark - bright)) + (weights.Highlights * bright);
        };
    }

    public static Fin<Unit> Multiplied(PixelFrame frame, Action<Span<Vector4>, long, long> draw, Option<double> sigma, float deviation, float chroma, Func<double, double> weight, (double R, double G, double B) luminance, IProgress<int> progress) {
        (float bias, Vector4 mix) = (deviation * deviation / 2f, deviation * Mix(chroma));
        PixelFrame plane = new(frame.Origin, frame.Size, frame.Extent);
        return from drawn in new PixelPass.Pointwise(draw).Run(plane, new Progress<int>())
               from shaped in sigma.Traverse(spread => plane.Filter(draws => Shaped(draws, spread))).As()
               from applied in new PixelPass.Pointwise((row, _, line) => {
                   ReadOnlySpan<Vector4> draws = plane.Row(line);
                   for (int x = 0; x < row.Length; x++) {
                       Vector4 log = mix * draws[x];
                       float share = (float)weight(LuminanceSamples.Luminance(row[x].X, row[x].Y, row[x].Z, luminance));
                       row[x] *= new Vector4(Vector3.Lerp(Vector3.One, Vector3.Exp(log.AsVector3() + new Vector3(log.W - bias)), share), 1f);
                   }
               }).Run(frame, progress)
               select applied;
    }
}

public static class PhotonNoise {
    public static PixelPass.Pointwise Pass(float shot, float read, float chroma, double exposure, (double R, double G, double B) luminance, uint field) {
        double shotStops = double.Log2(shot) - (0.5d * (exposure + double.Log2(Exposure.MiddleGray)));
        double readStops = double.Log2(read) - exposure;
        Vector4 mix = GrainNoise.Mix(chroma);
        return new((row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                Vector4 draws = mix * CoordinateHash.Normals(column + x, line, field);
                double shared = Deviation(LuminanceSamples.Luminance(row[x].X, row[x].Y, row[x].Z, luminance), draws.W);
                row[x] = new Vector4(
                    (float)(row[x].X + Deviation(row[x].X, draws.X) + shared), (float)(row[x].Y + Deviation(row[x].Y, draws.Y) + shared), (float)(row[x].Z + Deviation(row[x].Z, draws.Z) + shared), row[x].W);
            }
        });

        double Deviation(double signal, double draw) =>
            double.Log2(double.Abs(draw)) switch {
                var amplitude => double.CopySign(double.Hypot(double.Exp2(amplitude + shotStops + (0.5d * double.Log2(double.Max(signal, 0d)))), double.Exp2(amplitude + readStops)), draw),
            };
    }
}
