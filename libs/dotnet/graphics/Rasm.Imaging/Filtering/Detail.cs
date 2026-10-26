using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using TinyEXR;

namespace Rasm.Imaging.Filtering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sharpening {
    public static Fin<Unit> Laplacian(PixelFrame frame, float amount, double exposure, Gamut gamut, IProgress<int> progress) => frame.Filter(header => {
        (int width, (double R, double G, double B) luminance) = (frame.Size.Width, gamut.Luminance);
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
            int y = frame.RowIndex(line);
            ReadOnlySpan<Vector256<double>> deltas = laplacian.View<Vector256<double>>().GetRowSpan(y);
            ReadOnlySpan<Vector128<double>> steps = rise.View<Vector128<double>>().GetRowSpan(y);
            for (int x = 0; x < row.Length; x++) {
                if (row[x].W <= 0f) continue;
                double factor = double.Max(1d - double.Exp2(exposure + double.Log2(double.Max(steps[x].GetElement(0), steps[x].GetElement(1)))), 0d) * amount / 6d / row[x].W;
                row[x] = new((float)(row[x].X - (factor * deltas[x].GetElement(0))), (float)(row[x].Y - (factor * deltas[x].GetElement(1))), (float)(row[x].Z - (factor * deltas[x].GetElement(2))), row[x].W);
            }
        }).Run(frame, progress);
    });
}

public static class ContrastGrid {
    private const float LogFloor = -8f;
    private const float Spacing = 12f / 31f;

    public static Fin<Unit> Graded(PixelFrame frame, float highlights, float shadows, float detail, double cell, double surround, Gamut gamut, IProgress<int> progress) {
        int pitch = (int)double.Ceiling(cell);
        return (Spread(cell), Spread(surround)).Apply((fine, broad) => Filter(frame, highlights, shadows, detail, pitch, fine, broad, gamut, progress)).As().Flatten();

        Fin<(double Sigma, int Taps)> Spread(double sigma) =>
            (double.Sqrt(double.Max((sigma * sigma) - ((double)pitch * pitch / 4d), 0d)) / pitch) switch { var spread => PixelSampling.GaussianTaps<int>(spread).Map(taps => (spread, taps)) };
    }

    private static Fin<Unit> Filter(
        PixelFrame frame, float highlights, float shadows, float detail, int pitch, (double Sigma, int Taps) fine, (double Sigma, int Taps) broad, Gamut gamut, IProgress<int> progress) =>
        frame.Filter(header => {
            (int width, double gray, (WrapMode, WrapMode) edge) = (frame.Size.Width, double.Log2(Exposure.MiddleGray), (WrapMode.Clamp, WrapMode.Clamp));
            (int across, int down) = ((width + pitch - 1) / pitch, (frame.Size.Height + pitch - 1) / pitch);
            using Mat linear = new();
            using Mat weights = new(1, 4, DepthType.Cv64F, 1);
            using Mat luma = new();
            using Mat alpha = new();
            using Mat mask = new();
            using ScalarArray zero = new(0d);
            weights.SetTo([gamut.Luminance.R, gamut.Luminance.G, gamut.Luminance.B, 0d]);
            header.ConvertTo(linear, DepthType.Cv64F);
            CvInvoke.Transform(linear, luma, weights);
            CvInvoke.ExtractChannel(linear, alpha, 3);
            CvInvoke.Compare(alpha, zero, mask, CmpType.GreaterThan);
            CvInvoke.MinMaxIdx(luma, out double low, out double high, minIdx: null, maxIdx: null, mask);
            (int first, int slices) = Place(low, 1d, 0).Slice switch { var lowest => (lowest, Place(high, 1d, 0).Slice - lowest + 2) };
            using VectorOfMat lattice = new(slices);
            Mat[] planes = [.. Enumerable.Range(0, slices).Select(index => lattice[index])];
            foreach (Mat plane in planes) {
                plane.Create(down, across, DepthType.Cv64F, 4);
                plane.SetTo(default(MCvScalar));
            }
            using Mat cells = Mat.Zeros(down, across, DepthType.Cv64F, 4);
            ReadOnlySpan2D<double> lumas = luma.View<double>();
            ReadOnlySpan2D<double> coverage = alpha.View<double>();
            Span2D<Vector256<double>> marginals = cells.View<Vector256<double>>();
            for (int y = 0; y < lumas.Height; y++) {
                for (int x = 0; x < lumas.Width; x++) {
                    if (coverage[y, x] <= 0f) continue;
                    ((Vector256<double> sample, int slice, double upper), int cellY, int cellX) = (Place(lumas[y, x], coverage[y, x], first), y / pitch, x / pitch);
                    planes[slice].View<Vector256<double>>()[cellY, cellX] += (1d - upper) * sample;
                    planes[slice + 1].View<Vector256<double>>()[cellY, cellX] += upper * sample;
                    marginals[cellY, cellX] += sample;
                }
            }
            foreach (Mat plane in planes)
                CvInvoke.GaussianBlur(plane, plane, new Size(fine.Taps, fine.Taps), fine.Sigma, fine.Sigma, BorderType.Constant);
            CvInvoke.GaussianBlur(cells, cells, new Size(broad.Taps, broad.Taps), broad.Sigma, broad.Sigma, BorderType.Constant);
            return new PixelPass.Pointwise((row, _, line) => {
                int y = frame.RowIndex(line);
                ReadOnlySpan<double> rowLumas = luma.View<double>().GetRowSpan(y);
                for (int x = 0; x < row.Length; x++) {
                    if (row[x].W <= 0f) continue;
                    ((Vector256<double> own, int slice, double upper), (double X, double Y) at) = (Place(rowLumas[x], 1d, first), ((x + 0.5d) / pitch, (y + 0.5d) / pitch));
                    Vector256<double> graded = ((1d - upper) * Smooth(planes[slice], at)) + (upper * Smooth(planes[slice + 1], at));
                    double around = Smooth(cells, at) switch { var smooth => smooth.GetElement(0) / smooth.GetElement(1) };
                    double centered = double.Lerp(graded.GetElement(1) > 0d ? graded.GetElement(0) / graded.GetElement(1) : around, around, 0.6d) - gray;
                    double gain = double.Exp2(gray + (centered * (centered > 0d ? highlights : shadows)) + ((own.GetElement(0) - gray - centered) * detail) - own.GetElement(0));
                    row[x] = new((float)(row[x].X * gain), (float)(row[x].Y * gain), (float)(row[x].Z * gain), row[x].W);
                }
            }).Run(frame, progress);

            Vector256<double> Smooth(Mat plane, (double X, double Y) at) => ((ReadOnlySpan2D<Vector256<double>>)plane.View<Vector256<double>>()).Sample(at, edge, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel * weight));
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
        Vector2 random = CoordinateHash.White(Vector256<double>.Zero, field).AsVector2();
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
            ? new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"grain scan tile {width}x{height}"), ImagingReason.Oversize)
            : Try.lift(() => {
                Size dimensions = new((int)width, (int)height);
                int pixels = dimensions.Width * dimensions.Height;
                using Mat resized = new(dimensions.Height, dimensions.Width, DepthType.Cv32F, 4);
                using Mat source = new();
                using Mat mixed = new();
                using Mat mean = new(4, 4, DepthType.Cv64F, 1);
                const double third = 1d / 3d;
                ImageProcessing.Resize(scan.Block, scan.Size.Width, scan.Size.Height, resized.GetSpan<float>(), dimensions.Width, dimensions.Height, 4,
                    footprint < scan.Size.Width ? ResizeFilter.Box : ResizeFilter.Triangle);
                resized.ConvertTo(source, DepthType.Cv64F);
                mean.SetTo([1d, 0d, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 0d, 1d, 0d, third, third, third, 0d]);
                CvInvoke.Transform(source, mixed, mean);
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

    public static Fin<Unit> Multiplied(PixelFrame frame, Action<Span<Vector4>, long, long> draw, Option<double> sigma, float deviation, float chroma, Func<double, double> weight, Gamut gamut, IProgress<int> progress) {
        (float bias, Vector4 mix, (double R, double G, double B) luminance) = (deviation * deviation / 2f, deviation * Mix(chroma), gamut.Luminance);
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
    public static PixelPass.Pointwise Pass(float shot, float read, float chroma, double exposure, Gamut gamut, uint field) {
        double shotStops = double.Log2(shot) - (0.5d * (exposure + double.Log2(Exposure.MiddleGray)));
        double readStops = double.Log2(read) - exposure;
        (Vector4 mix, (double R, double G, double B) luminance) = (GrainNoise.Mix(chroma), gamut.Luminance);
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

public static class Kuwahara {
    public static Fin<Unit> Quadrants(PixelFrame frame, double radius, IProgress<int> progress) => frame.Filter(header => {
        (int width, int height) = (frame.Size.Width, frame.Size.Height);
        using Mat sums = new();
        using Mat squares = new();
        CvInvoke.Integral(header, sums, squares, tiltedSum: null, DepthType.Cv64F, DepthType.Cv64F);
        return new PixelPass.Pointwise((row, _, line) => {
            int y = frame.RowIndex(line);
            ReadOnlySpan2D<Vector256<double>> totals = sums.View<Vector256<double>>();
            ReadOnlySpan2D<Vector256<double>> squared = squares.View<Vector256<double>>();
            for (int x = 0; x < width; x++) {
                (Vector256<double> mean, double variance) = (Vector256<double>.Zero, 0d);
                for (int quadrant = 0; quadrant < 4; quadrant++) {
                    (int left, int right) = (quadrant & 1) is 0 ? ((int)double.Max(0d, x - radius), x) : (x, (int)double.Min(width - 1, x + radius));
                    (int top, int bottom) = quadrant < 2 ? (y, (int)double.Min(height - 1, y + radius)) : ((int)double.Max(0d, y - radius), y);
                    double count = (double)(right - left + 1) * (bottom - top + 1);
                    Vector256<double> sum = Box(totals, left, top, right, bottom);
                    double spread = Vector256.Sum((Box(squared, left, top, right, bottom) - (sum * sum / count)).WithElement(3, 0d)) / count;
                    if (quadrant is 0 || spread < variance)
                        (mean, variance) = (sum / count, spread);
                }
                row[x] = new Vector4((float)mean.GetElement(0), (float)mean.GetElement(1), (float)mean.GetElement(2), row[x].W);
            }
        }).Run(frame, progress);

        static Vector256<double> Box(ReadOnlySpan2D<Vector256<double>> table, int left, int top, int right, int bottom) =>
            table[bottom + 1, right + 1] - table[top, right + 1] - table[bottom + 1, left] + table[top, left];
    });

    public static Fin<Unit> Sectors(PixelFrame frame, double radius, double uniformity, float sharpness, float eccentricity, IProgress<int> progress) {
        double reach = double.Ceiling((1d + eccentricity) * radius) + 1d;
        return
            from bounded in guard<Error>(reach <= int.MaxValue, new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Kuwahara reach of {reach} pixels"), ImagingReason.Oversize))
            from filtered in frame.Filter(source => {
                const int count = 8;
                const double corner = 0.182d;
                (int width, int height) = (frame.Size.Width, frame.Size.Height);
                (double focus, double overlap) = (16d * sharpness * sharpness, 2d / radius);
                (double sine, double cosine) = double.SinCos(1.5d * double.Pi / count);
                double envelope = (overlap + cosine) / (sine * sine);
                using Mat copy = new();
                using Mat across = new(3, 3, DepthType.Cv64F, 1);
                using Mat down = new();
                using Mat rgb = ChannelMask.Rgb();
                using Mat horizontal = new(source.Size, DepthType.Cv64F, source.NumberOfChannels);
                using Mat vertical = new(source.Size, DepthType.Cv64F, source.NumberOfChannels);
                using Mat product = new();
                using Mat xx = new();
                using Mat xy = new();
                using Mat yy = new();
                source.ConvertTo(copy, DepthType.Cv64F);
                across.SetTo([-corner, 0d, corner, (2d * corner) - 1d, 0d, 1d - (2d * corner), -corner, 0d, corner]);
                CvInvoke.Transpose(across, down);
                CvInvoke.Filter2D(copy, horizontal, across, new Point(-1, -1), 0d, BorderType.Replicate);
                CvInvoke.Filter2D(copy, vertical, down, new Point(-1, -1), 0d, BorderType.Replicate);
                return Seq((horizontal, horizontal, xx), (horizontal, vertical, xy), (vertical, vertical, yy)).TraverseM(pair => {
                    (Mat first, Mat second, Mat tensor) = pair;
                    CvInvoke.Multiply(first, second, product);
                    CvInvoke.Transform(product, tensor, rgb);
                    return tensor.Blur(tensor, uniformity);
                }).As().Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                    int y = frame.RowIndex(line);
                    ReadOnlySpan2D<Vector256<double>> image = copy.View<Vector256<double>>();
                    ReadOnlySpan<double> dxx = xx.View<double>().GetRowSpan(y);
                    ReadOnlySpan<double> dxy = xy.View<double>().GetRowSpan(y);
                    ReadOnlySpan<double> dyy = yy.View<double>().GetRowSpan(y);
                    Span<Vector256<double>> means = stackalloc Vector256<double>[count];
                    Span<Vector256<double>> spreads = stackalloc Vector256<double>[count];
                    Span<double> deviations = stackalloc double[count];
                    for (int x = 0; x < width; x++) {
                        (double half, double root) = ((dxx[x] + dyy[x]) / 2d, double.Hypot(dxx[x] - dyy[x], 2d * dxy[x]) / 2d);
                        (double axisY, double axisX) = double.SinCos(double.Atan2(-2d * dxy[x], dyy[x] - dxx[x]) / 2d);
                        double stretch = 1d + (eccentricity * (half > 0d ? root / half : 0d));
                        (double major, double minor) = (stretch * radius, radius / stretch);
                        (int reachX, int reachY) = ((int)double.Ceiling(double.Hypot(major * axisX, minor * axisY)), (int)double.Ceiling(double.Hypot(major * axisY, minor * axisX)));
                        means.Fill(image[y, x].WithElement(3, 1d / count));
                        spreads.Clear();
                        for (int j = 0; j <= reachY; j++) {
                            for (int i = j is 0 ? 1 : -reachX; i <= reachX; i++)
                                Sampled(means, spreads, image, (x, y), (i, j), (axisX, axisY), (major, minor), (overlap, envelope));
                        }
                        for (int k = 0; k < count; k++)
                            deviations[k] = double.Max(0.02d, Vector256.Sum(Vector256.Sqrt(spreads[k] / means[k].GetElement(3))));
                        TensorPrimitives.Divide(TensorPrimitives.Min(deviations), deviations, deviations);
                        TensorPrimitives.Pow(deviations, focus, deviations);
                        Vector256<double> total = Vector256<double>.Zero;
                        for (int k = 0; k < count; k++)
                            total += means[k] * deviations[k];
                        Vector256<double> mean = total / TensorPrimitives.Sum(deviations);
                        row[x] = new Vector4((float)mean.GetElement(0), (float)mean.GetElement(1), (float)mean.GetElement(2), row[x].W);
                    }
                }).Run(frame, progress));
            })
            select filtered;
    }

    private static void Sampled(
        Span<Vector256<double>> means, Span<Vector256<double>> spreads, ReadOnlySpan2D<Vector256<double>> image, (int X, int Y) pixel, (int I, int J) offset,
        (double X, double Y) axis, (double Major, double Minor) scale, (double Overlap, double Envelope) shape) {
        (double diskX, double diskY) = (((axis.X * offset.I) + (axis.Y * offset.J)) / scale.Major, ((axis.X * offset.J) - (axis.Y * offset.I)) / scale.Minor);
        double distance = (diskX * diskX) + (diskY * diskY);
        if (distance > 1d)
            return;
        double diagonal = double.Sqrt(2d);
        Vector256<double> projections = Vector256.Create(diskX, (diskX - diskY) / diagonal, diskY, (diskX + diskY) / diagonal);
        Vector256<double> bend = Vector256.Create(shape.Overlap) - (shape.Envelope * projections * projections);
        Vector256<double> swing = Vector256.Create(projections.GetElement(2), projections.GetElement(3), -projections.GetElement(0), -projections.GetElement(1));
        (Vector256<double> low, Vector256<double> high) = (Vector256.Max(bend + swing, Vector256<double>.Zero), Vector256.Max(bend - swing, Vector256<double>.Zero));
        double radial = double.Exp(-double.Pi * distance) / Vector256.Sum((low * low) + (high * high));
        (low, high) = (low * low * radial, high * high * radial);
        (int right, int bottom) = (image.Width - 1, image.Height - 1);
        Vector256<double> upper = image[(int)long.Clamp((long)pixel.Y + offset.J, 0, bottom), (int)long.Clamp((long)pixel.X + offset.I, 0, right)];
        Vector256<double> lower = image[(int)long.Clamp((long)pixel.Y - offset.J, 0, bottom), (int)long.Clamp((long)pixel.X - offset.I, 0, right)];
        for (int k = 0, m = means.Length / 2; k < means.Length / 2; k++, m++) {
            (means[k], spreads[k]) = Accumulated(means[k], spreads[k], upper, low.GetElement(k));
            (means[k], spreads[k]) = Accumulated(means[k], spreads[k], lower, high.GetElement(k));
            (means[m], spreads[m]) = Accumulated(means[m], spreads[m], upper, high.GetElement(k));
            (means[m], spreads[m]) = Accumulated(means[m], spreads[m], lower, low.GetElement(k));
        }
    }

    private static (Vector256<double> Mean, Vector256<double> Spread) Accumulated(Vector256<double> mean, Vector256<double> spread, Vector256<double> sample, double share) =>
        (mean.GetElement(3), (sample - mean).WithElement(3, 0d)) switch {
            var (weight, delta) => ((mean + (delta * (share / (weight + share)))).WithElement(3, weight + share), spread + (delta * delta * (weight * share / (weight + share)))),
        };
}
