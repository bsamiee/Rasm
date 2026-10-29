using System.Drawing;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filtering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LocalContrast {
    private const float Spacing = 12f / 31f;

    public static Fin<Unit> Sharpened(PixelFrame frame, float amount, double exposure, Gamut gamut, Option<IProgress<int>> progress = default) => frame.Filter(header => {
        (Vector3 luminance, float scale) = (gamut.Luminance, float.Exp2((float)exposure));
        using Mat light = new(header.Size, DepthType.Cv32F, 4);
        using Mat weights = new(2, 4, DepthType.Cv32F, 1);
        using Mat luma = new();
        using Mat rise = new();
        using Mat cross = CvInvoke.GetStructuringElement(MorphShapes.Cross, new Size(3, 3), new Point(-1, -1));
        CvInvoke.MixChannels(header, light, [3, 0, 3, 1, 3, 2, 3, 3]);
        CvInvoke.Multiply(header, light, light);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z, 0f, -luminance.X, -luminance.Y, -luminance.Z, 0f]);
        CvInvoke.Transform(light, luma, weights);
        CvInvoke.Laplacian(light, light, DepthType.Cv32F, 1, 1d, 0d, BorderType.Replicate);
        CvInvoke.Dilate(luma, rise, cross, new Point(-1, -1), 1, BorderType.Replicate, default);
        CvInvoke.Subtract(rise, luma, rise);
        return new PixelPass.Positional((row, _, y) => {
            ReadOnlySpan<Vector4> deltas = light.View<Vector4>().GetRowSpan(y - frame.Origin.Y);
            ReadOnlySpan<Vector2> steps = rise.View<Vector2>().GetRowSpan(y - frame.Origin.Y);
            for (int x = 0; x < row.Length; x++)
                row[x] = row[x].W > 0f ? row[x] - (deltas[x] with { W = 0f } * (float.Max(1f - (scale * float.Max(steps[x].X, steps[x].Y)), 0f) * amount / 6f / row[x].W)) : row[x];
        }).Run(frame, progress);
    });

    public static Fin<Unit> Graded(PixelFrame frame, float highlights, float shadows, float detail, double cell, double surround, Gamut gamut, Option<IProgress<int>> progress = default) =>
        frame.Filter(header => {
            (int pitch, float gray) = ((int)double.Ceiling(cell), float.Log2(Metering.MiddleGray));
            (int across, int down) = ((frame.Size.Width + pitch - 1) / pitch, (frame.Size.Height + pitch - 1) / pitch);
            using Mat levels = new();
            using Mat alpha = new();
            using Mat mask = new();
            using ScalarArray zero = new(0d);
            EdgeSource.Luma(gamut.Luminance)(header, levels);
            TensorPrimitives.Max(levels.GetSpan<float>(), LuminanceSamples.Floor, levels.GetSpan<float>());
            TensorPrimitives.Log2(levels.GetSpan<float>(), levels.GetSpan<float>());
            CvInvoke.ExtractChannel(header, alpha, 3);
            CvInvoke.Compare(alpha, zero, mask, CmpType.GreaterThan);
            CvInvoke.MinMaxIdx(levels, out double low, out double high, minIdx: null, maxIdx: null, mask);
            int slices = (int)double.Floor((high - low) / Spacing) + 2;
            using Mat lattice = Mat.Zeros(slices * down, across, DepthType.Cv32F, 4);
            using Mat cells = Mat.Zeros(down, across, DepthType.Cv32F, 4);
            Span2D<Vector4> grid = lattice.View<Vector4>();
            ReadOnlySpan2D<float> logs = levels.View<float>();
            ReadOnlySpan2D<Vector4> pixels = header.View<Vector4>();
            for (int y = 0; y < logs.Height; y++) {
                for (int x = 0; x < logs.Width; x++) {
                    (int slice, float upper) = Placed(logs[y, x], (float)low, slices);
                    Vector4 sample = float.Max(pixels[y, x].W, 0f) * new Vector4(logs[y, x], 1f, 0f, 0f);
                    grid[(slice * down) + (y / pitch), x / pitch] += (1f - upper) * sample;
                    grid[((slice + 1) * down) + (y / pitch), x / pitch] += upper * sample;
                }
            }
            for (int slice = 0; slice < slices; slice++) {
                using Mat plane = new(lattice, new Rectangle(0, slice * down, across, down));
                CvInvoke.Add(cells, plane, cells);
                plane.Blur(plane, Spread(cell), BorderType.Isolated);
            }
            cells.Blur(cells, Spread(surround), BorderType.Constant);
            return new PixelPass.Positional((row, _, y) => {
                (int stored, (WrapMode, WrapMode) edge) = (y - frame.Origin.Y, (WrapMode.Clamp, WrapMode.Clamp));
                ReadOnlySpan<float> own = levels.View<float>().GetRowSpan(stored);
                ReadOnlySpan2D<Vector4> lanes = lattice.View<Vector4>();
                ReadOnlySpan2D<Vector4> broad = cells.View<Vector4>();
                for (int x = 0; x < row.Length; x++) {
                    (int slice, float upper) = Placed(own[x], (float)low, slices);
                    Vector2 at = new Vector2(x + 0.5f, stored + 0.5f) / pitch;
                    Vector4 graded = Vector4.Lerp(lanes.Slice(slice * down, 0, down, across).Sample(at, edge), lanes.Slice((slice + 1) * down, 0, down, across).Sample(at, edge), upper);
                    float around = broad.Sample(at, edge) switch { var smooth => smooth.X / smooth.Y };
                    float centered = float.Lerp(graded.Y > 0f ? graded.X / graded.Y : around, around, 0.6f) - gray;
                    float gain = float.Exp2(gray + (centered * (centered > 0f ? highlights : shadows)) + ((own[x] - gray - centered) * detail) - own[x]);
                    row[x] = row[x].W > 0f ? new Vector4(row[x].AsVector3() * gain, row[x].W) : row[x];
                }
            }).Run(frame, progress);

            double Spread(double sigma) => double.Sqrt(double.Max((sigma * sigma) - ((double)pitch * pitch / 4d), 0d)) / pitch;
        });

    private static (int Slice, float Upper) Placed(float level, float low, int slices) =>
        float.Clamp((level - low) / Spacing, 0f, slices - 2) switch { var stop => ((int)stop, stop - (int)stop) };
}

public static class GrainNoise {
    public static Action<Span<Vector4>, int, int> White(uint field) =>
        (row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = CoordinateHash.Normals(Vector128.Create(column + x, line, 0, 0), field);
        };

    public static Action<Span<Vector4>, int, int> Tiled(Memory2D<Vector4> tile, uint field) {
        Vector2 offset = CoordinateHash.Uniforms(Vector128<int>.Zero, field).AsVector2() * new Vector2(tile.Width, tile.Height);
        return (row, column, line) => {
            ReadOnlySpan2D<Vector4> texels = tile.Span;
            for (int x = 0; x < row.Length; x++)
                row[x] = texels.Sample(new Vector2(column + x + 0.5f, line + 0.5f) + offset, (WrapMode.Periodic, WrapMode.Periodic));
        };
    }

    public static Fin<Memory2D<Vector4>> NormalScores(PixelFrame scan, double footprint) {
        (double width, double height) = (double.Ceiling(footprint), double.Ceiling(scan.Size.Height * footprint / scan.Size.Width));
        return width * height > System.Array.MaxLength
            ? ImagingFault.Of(ImagingReason.Oversize, (width, height))
            : Try.lift(() => {
                int pixels = (int)width * (int)height;
                using Mat source = scan.Header();
                using Mat mixed = new();
                using Mat channel = new();
                using Mat mean = new(4, 4, DepthType.Cv32F, 1);
                mean.SetTo([1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 1f / 3f, 1f / 3f, 1f / 3f, 0f]);
                CvInvoke.Resize(source, channel, new Size((int)width, (int)height), 0d, 0d, footprint < scan.Size.Width ? Inter.Area : Inter.Linear);
                CvInvoke.Transform(channel, mixed, mean);
                Vector4[] scores = GC.AllocateUninitializedArray<Vector4>(pixels);
                Span2D<float> lanes = scores.AsSpan().Cast<Vector4, float>().AsSpan2D(pixels, 4);
                double[] values = new double[pixels];
                for (int lane = 0; lane < 4; lane++) {
                    CvInvoke.ExtractChannel(mixed, channel, lane);
                    TensorPrimitives.ConvertChecked(channel.GetSpan<float>(), values);
                    ReadOnlySpan<float> probits = [.. values.Ranks(RankDefinition.Min).Zip(ArrayStatistics.RanksInplace(values, RankDefinition.Max), (low, high) => (float)Normal.InvCDF(0d, 1d, (low + high - 1d) / (2d * pixels)))];
                    probits.CopyTo(lanes.GetColumn(lane));
                }
                return scores.AsMemory().AsMemory2D((int)height, (int)width);
            }).Run();
    }

    public static Func<float, float> Bands((float Shadows, float Midtones, float Highlights) weights, float edge, float span, float width, float exposure) {
        float scale = width > 0f ? width : float.Max(edge, span);
        return luma => {
            float stops = float.Log2(float.Max(luma, 0f)) + exposure;
            float dark = edge is 0f ? luma > 0f ? 0f : 1f : 1f - DistanceField.SmoothStep(0f, 1f, float.Exp2(stops - float.Log2(edge)));
            float distance = scale is 0f ? luma : float.Exp2(stops - float.Log2(scale)) - (edge / scale) - (span / scale);
            float bright = width is 0f ? distance > 0f ? 1f : 0f : DistanceField.SmoothStep(0f, 1f, distance);
            return (weights.Shadows * dark) + (weights.Midtones * (1f - dark - bright)) + (weights.Highlights * bright);
        };
    }

    public static Fin<Unit> Multiplied(
        PixelFrame frame, Action<Span<Vector4>, int, int> draw, double sigma, float deviation, float chroma, Func<float, float> weight, Gamut gamut, Option<IProgress<int>> progress = default) =>
        Gaussian.Kernel(sigma, NormType.L2).Bind(kernel => {
            using (kernel) {
                (int reach, float bias, Vector4 mix, Vector3 luminance) = (kernel.Rows / 2, deviation * deviation / 2f, deviation * Mix(chroma), gamut.Luminance);
                return from padded in PixelExtent.Validated(frame.Size.Width + (2 * reach), frame.Size.Height + (2 * reach))
                       let plane = new PixelFrame(new Point(frame.Origin.X - reach, frame.Origin.Y - reach), padded, frame.Extent)
                       from drawn in new PixelPass.Positional(draw).Run(plane)
                       from shaped in plane.Filter(draws => {
                           CvInvoke.SepFilter2D(draws, draws, DepthType.Cv32F, kernel, kernel, new Point(-1, -1), 0d, BorderType.Reflect101);
                           return unit;
                       })
                       from applied in new PixelPass.Positional((row, _, y) => {
                           ReadOnlySpan<Vector4> draws = plane.View.Span.GetRowSpan(y - frame.Origin.Y + reach)[reach..];
                           for (int x = 0; x < row.Length; x++) {
                               Vector4 log = mix * draws[x];
                               row[x] *= new Vector4(Vector3.Lerp(Vector3.One, Vector3.Exp(log.AsVector3() + new Vector3(log.W - bias)), weight(Vector3.Dot(luminance, row[x].AsVector3()))), 1f);
                           }
                       }).Run(frame, progress)
                       select applied;
            }
        });

    public static PixelPass.Positional Photon(float shot, float read, float chroma, double exposure, Gamut gamut, uint field) {
        (float shotGain, float readGain) = (shot * shot / (float.Exp2((float)exposure) * Metering.MiddleGray), read * read / float.Exp2(2f * (float)exposure));
        (Vector4 mix, Vector3 luminance) = (Mix(chroma), gamut.Luminance);
        return new((row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                Vector4 draws = mix * CoordinateHash.Normals(Vector128.Create(column + x, line, 0, 0), field);
                Vector3 color = row[x].AsVector3();
                Vector3 shared = new(draws.W * float.Sqrt((shotGain * float.Max(Vector3.Dot(luminance, color), 0f)) + readGain));
                row[x] = new Vector4(color + (draws.AsVector3() * Vector3.SquareRoot((shotGain * Vector3.Max(color, Vector3.Zero)) + new Vector3(readGain))) + shared, row[x].W);
            }
        });
    }

    private static Vector4 Mix(float chroma) => new(new Vector3(float.Sqrt(chroma)), float.Sqrt(1f - chroma));
}

public static class Kuwahara {
    private const int Count = 8;

    public static Fin<Unit> Quadrants(PixelFrame frame, double radius) => frame.Filter(header => {
        (int reach, Size side) = ((int)radius, new Size((int)radius + 1, (int)radius + 1));
        using Mat rgb = new(1, 4, DepthType.Cv32F, 1);
        using Mat best = new();
        using Mat spread = new();
        using Mat squares = new();
        using Mat mean = new();
        using Mat chosen = new();
        using Mat closer = new();
        rgb.SetTo([1f, 1f, 1f, 0f]);
        for (int quadrant = 0; quadrant < 4; quadrant++) {
            Point anchor = new((quadrant & 1) is 0 ? reach : 0, quadrant < 2 ? 0 : reach);
            CvInvoke.BoxFilter(header, mean, DepthType.Cv32F, side, anchor, normalize: true, BorderType.Replicate);
            CvInvoke.SqrBoxFilter(header, squares, DepthType.Cv32F, side, anchor, normalize: true, BorderType.Replicate);
            CvInvoke.Multiply(mean, mean, spread);
            CvInvoke.Subtract(squares, spread, squares);
            CvInvoke.Transform(squares, spread, rgb);
            if (quadrant is 0) {
                mean.CopyTo(chosen);
                spread.CopyTo(best);
            } else {
                CvInvoke.Compare(spread, best, closer, CmpType.LessThan);
                mean.CopyTo(chosen, closer);
                CvInvoke.Min(spread, best, best);
            }
        }
        CvInvoke.MixChannels(chosen, header, [0, 0, 1, 1, 2, 2]);
        return unit;
    });

    public static Fin<Unit> Sectors(PixelFrame frame, double radius, double uniformity, float sharpness, float eccentricity, Option<IProgress<int>> progress = default) =>
        from bounded in ImagingFault.Unless(double.Ceiling((1d + eccentricity) * radius) + 1d <= int.MaxValue, ImagingReason.Oversize, (radius, eccentricity))
        from filtered in frame.Filter(source => {
            (float focus, float overlap, (float sine, float cosine)) = (16f * sharpness * sharpness, 2f / (float)radius, float.SinCos(1.5f * float.Pi / Count));
            float envelope = (overlap + cosine) / (sine * sine);
            using Mat copy = source.Clone();
            using Mat rgb = new(1, 4, DepthType.Cv32F, 1);
            using Mat horizontal = new();
            using Mat vertical = new();
            using Mat product = new();
            using Mat xx = new();
            using Mat xy = new();
            using Mat yy = new();
            rgb.SetTo([1f, 1f, 1f, 0f]);
            CvInvoke.Scharr(copy, horizontal, DepthType.Cv32F, 1, 0, 1d / 16d, 0d, BorderType.Replicate);
            CvInvoke.Scharr(copy, vertical, DepthType.Cv32F, 0, 1, 1d / 16d, 0d, BorderType.Replicate);
            foreach ((Mat first, Mat second, Mat tensor) in Seq((horizontal, horizontal, xx), (horizontal, vertical, xy), (vertical, vertical, yy))) {
                CvInvoke.Multiply(first, second, product);
                CvInvoke.Transform(product, tensor, rgb);
                tensor.Blur(tensor, uniformity);
            }
            return new PixelPass.Positional((row, _, y) => {
                int stored = y - frame.Origin.Y;
                ReadOnlySpan2D<Vector4> image = copy.View<Vector4>();
                ReadOnlySpan<float> dxx = xx.View<float>().GetRowSpan(stored);
                ReadOnlySpan<float> dxy = xy.View<float>().GetRowSpan(stored);
                ReadOnlySpan<float> dyy = yy.View<float>().GetRowSpan(stored);
                Span<Vector4> sums = stackalloc Vector4[Count];
                Span<Vector4> squares = stackalloc Vector4[Count];
                Span<float> deviations = stackalloc float[Count];
                for (int x = 0; x < row.Length; x++) {
                    (float half, float root) = ((dxx[x] + dyy[x]) / 2f, float.Hypot(dxx[x] - dyy[x], 2f * dxy[x]) / 2f);
                    (float axisY, float axisX) = float.SinCos(float.Atan2(-2f * dxy[x], dyy[x] - dxx[x]) / 2f);
                    Vector2 scale = (1f + (eccentricity * (half > 0f ? root / half : 0f))) switch { var stretch => new((float)radius * stretch, (float)radius / stretch) };
                    (int reachX, int reachY) = ((int)float.Ceiling(float.Hypot(scale.X * axisX, scale.Y * axisY)), (int)float.Ceiling(float.Hypot(scale.X * axisY, scale.Y * axisX)));
                    Vector4 center = image[stored, x] with { W = 1f };
                    sums.Fill(center / Count);
                    squares.Fill(center * center / Count);
                    for (int j = 0; j <= reachY; j++) {
                        for (int i = j is 0 ? 1 : -reachX; i <= reachX; i++)
                            Sampled(sums, squares, image, (x, stored), (i, j), new Vector2(axisX, axisY), scale, (overlap, envelope));
                    }
                    Vector4 total = Vector4.Zero;
                    for (int k = 0; k < Count; k++) {
                        (squares[k], sums[k]) = (squares[k] / sums[k].W, sums[k] / sums[k].W);
                        deviations[k] = float.Max(0.02f, Vector3.Sum(Vector3.SquareRoot(Vector3.Max((squares[k] - (sums[k] * sums[k])).AsVector3(), Vector3.Zero))));
                    }
                    TensorPrimitives.Divide(TensorPrimitives.Min(deviations), deviations, deviations);
                    TensorPrimitives.Pow(deviations, focus, deviations);
                    for (int k = 0; k < Count; k++)
                        total += sums[k] * deviations[k];
                    row[x] = new Vector4(total.AsVector3() / TensorPrimitives.Sum(deviations), row[x].W);
                }
            }).Run(frame, progress);
        })
        select filtered;

    private static void Sampled(
        Span<Vector4> sums, Span<Vector4> squares, ReadOnlySpan2D<Vector4> image, (int X, int Y) pixel, (int I, int J) offset, Vector2 axis, Vector2 scale, (float Overlap, float Envelope) shape) {
        Vector2 disk = new Vector2((axis.X * offset.I) + (axis.Y * offset.J), (axis.X * offset.J) - (axis.Y * offset.I)) / scale;
        float distance = disk.LengthSquared();
        if (distance > 1f)
            return;
        float diagonal = float.Sqrt(2f);
        Vector4 projections = new(disk.X, (disk.X - disk.Y) / diagonal, disk.Y, (disk.X + disk.Y) / diagonal);
        Vector4 bend = new Vector4(shape.Overlap) - (shape.Envelope * projections * projections);
        Vector4 swing = new(projections.Z, projections.W, -projections.X, -projections.Y);
        (Vector4 low, Vector4 high) = (Vector4.Max(bend + swing, Vector4.Zero), Vector4.Max(bend - swing, Vector4.Zero));
        float radial = float.Exp(-float.Pi * distance) / Vector4.Sum((low * low) + (high * high));
        (low, high) = (low * low * radial, high * high * radial);
        (int right, int bottom) = (image.Width - 1, image.Height - 1);
        Vector4 upper = image[int.Clamp(pixel.Y + offset.J, 0, bottom), int.Clamp(pixel.X + offset.I, 0, right)] with { W = 1f };
        Vector4 lower = image[int.Clamp(pixel.Y - offset.J, 0, bottom), int.Clamp(pixel.X - offset.I, 0, right)] with { W = 1f };
        for (int k = 0; k < Count / 2; k++) {
            (sums[k], squares[k]) = (sums[k] + (low[k] * upper) + (high[k] * lower), squares[k] + (low[k] * upper * upper) + (high[k] * lower * lower));
            (sums[k + (Count / 2)], squares[k + (Count / 2)]) = (sums[k + (Count / 2)] + (high[k] * upper) + (low[k] * lower), squares[k + (Count / 2)] + (high[k] * upper * upper) + (low[k] * lower * lower));
        }
    }
}
