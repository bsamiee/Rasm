using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PhosphorMask {
    public static readonly PhosphorMask ApertureGrille = new("aperture-grille", gap: 0d, period: (2d, 1d), shift: (1d, 0d));
    public static readonly PhosphorMask SlotMask = new("slot-mask", gap: 1d / 3d, period: (2d, 1d), shift: (1d, 0.5d));
    public static readonly PhosphorMask ShadowMask = new("shadow-mask", gap: 1d / 3d, period: (1d, 2d), shift: (0.5d, 1d));

    private readonly double gap;
    private readonly (double Across, double Down) period;
    private readonly (double Across, double Down) shift;

    public Vector3 Coverage(long column, long row, double pitch) {
        const double third = 1d / 3d;
        double span = 1d / pitch;
        (double u, double v, double width) = (column * span, row * span, 1d - gap);
        return new((float)Channel(0), (float)Channel(1), (float)Channel(2));

        double Channel(int channel) =>
            ((Area(u, period.Across, third * channel, third) * Area(v, period.Down, 0d, width)) + (Area(u + shift.Across, period.Across, third * channel, third) * Area(v + shift.Down, period.Down, 0d, width)))
            * 3d / (span * span * width);

        double Area(double t, double cycle, double offset, double open) => Primitive(t + span, cycle, offset, open) - Primitive(t, cycle, offset, open);

        static double Primitive(double point, double cycle, double offset, double open) =>
            double.Floor(point / cycle) switch { var cell => (cell * open) + double.Clamp(point - (cell * cycle) - offset, 0d, open) };
    }
}

[SmartEnum<string>]
public sealed partial class TelevisionSystem {
    public static readonly TelevisionSystem Ntsc = new("ntsc", lines: 525, lineRate: 4.5e6d / 286d, cycles: 455d / 2d, alternates: false);
    public static readonly TelevisionSystem Pal = new("pal", lines: 625, lineRate: 15625d, cycles: (1135d / 4d) + (1d / 625d), alternates: true);

    private const double ActiveSamples = 720d;
    private const double SamplingRate = 13.5e6d;

    private readonly int lines;
    private readonly double lineRate;
    private readonly double cycles;
    private readonly bool alternates;

    public long Frame(double seconds) => (long)double.Floor(seconds * lineRate / lines);

    public void CrossColor(Mat luma, Mat blue, Mat red, PixelFrame frame, int raster, long count, float amount) {
        double step = cycles * ActiveSamples * lineRate / SamplingRate / frame.Extent.Width;
        using Mat detail = new();
        using SpanOwner<float> waves = SpanOwner<float>.Allocate(2 * frame.Size.Width);
        Span<float> sine = waves.Span[..frame.Size.Width];
        Span<float> cosine = waves.Span[frame.Size.Width..];
        CvInvoke.GaussianBlur(luma, detail, new System.Drawing.Size(0, 1), 1d / (6d * step), 0d, BorderType.Replicate);
        CvInvoke.Subtract(luma, detail, detail);
        for (int x = 0; x < sine.Length; x++)
            sine[x] = (float)(2d * double.Ieee754Remainder(step * (frame.Origin.X + x), 1d));
        TensorPrimitives.SinCosPi(sine, sine, cosine);
        ReadOnlySpan2D<float> leak = detail.View<float>();
        Span2D<float> cb = blue.View<float>();
        Span2D<float> cr = red.View<float>();
        for (int y = 0; y < frame.Size.Height; y++) {
            long line = (count * lines) + Video.Line(frame.Origin.Y + y, raster, frame.Extent.Height);
            (double sin, double cos) = double.SinCosPi(2d * double.Ieee754Remainder(cycles * line, 1d));
            float sign = alternates && long.IsOddInteger(line) ? -amount : amount;
            for (int x = 0; x < frame.Size.Width; x++)
                (cb[y, x], cr[y, x]) = (cb[y, x] + (amount * leak[y, x] * (float)((cosine[x] * cos) - (sine[x] * sin))), cr[y, x] + (sign * leak[y, x] * (float)((sine[x] * cos) + (cosine[x] * sin))));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Video {
    public static long Line(long row, int lines, int height) => (long)CellGrid.Floored((Int128)row * lines, height).Quotient;

    public static Fin<Mat> Echo(double sigma, double delay, float share) => Gaussian.Taps(double.Abs(delay) + (3d * sigma)).Map(count => {
        double nearest = double.Abs(delay - double.Round(delay, MidpointRounding.ToEven));
        double[] shape = [.. Enumerable.Range(0, count).Select(tap => double.Abs(tap - (count / 2) + delay))
            .Select(distance => sigma > 0d ? double.Exp(((nearest * nearest) - (distance * distance)) / (2d * sigma * sigma)) : double.Max(1d - distance, 0d))];
        TensorPrimitives.Multiply(shape, share / TensorPrimitives.Sum(shape), shape);
        shape[count / 2] += 1d - share;
        Mat taps = new(count, 1, DepthType.Cv64F, 1);
        taps.SetTo(shape);
        return taps;
    });

    public static void Filter(Mat plane, Mat taps, bool vertical = false) {
        using Mat kernel = taps.Reshape(1, vertical ? taps.Rows : 1);
        CvInvoke.Filter2D(plane, plane, kernel, new System.Drawing.Point(-1, -1), 0d, BorderType.Replicate);
    }

    public static Fin<Unit> Decomposed(PixelFrame frame, YbrConfiguration ybr, Transfer encoding, Func<Mat, Mat, Mat, Fin<Unit>> planes) {
        Wacton.Unicolour.Configuration signal = new(ybrConfig: ybr);
        LutTable into = LutTable.Of([encoding.Encode, new LutTable.Affine(Gamut.Columns(signal, ColourSpace.Rgb, signal, ColourSpace.Ypbpr))]);
        LutTable back = LutTable.Of([new LutTable.Affine(Gamut.Columns(signal, ColourSpace.Ypbpr, signal, ColourSpace.Rgb)), encoding.Decode]);
        frame.Rows((row, _, _) => into.Apply(row));
        Fin<Unit> processed = frame.Filter(header => {
            using VectorOfMat channels = new();
            CvInvoke.Split(header, channels);
            using Mat luma = channels[0];
            using Mat blue = channels[1];
            using Mat red = channels[2];
            return planes(luma, blue, red).Map(_ => { CvInvoke.Merge(channels, header); return unit; });
        });
        frame.Rows((row, _, _) => back.Apply(row));
        return processed;
    }

    public static PixelPass Raster(PixelExtent extent, int lines, bool interlaced, long count, Vector2 beam, PhosphorMask mask, double pitch, float strength, double depth) {
        double middle = extent.Height / 2d;
        double field = (double)extent.Height * (interlaced ? 2 : 1);
        double span = lines / (field * beam.Y);
        double phase = (middle * lines / field) + ((interlaced ? count & 1L : 0L) / 2d);
        (double whole, double fraction) = double.Floor(span) switch { var floor => (floor, span - floor) };
        double sinc = (double.IsEvenInteger(whole) ? 1d : -1d) * double.SinPi(double.Min(fraction, 1d - fraction)) / (2d * double.Pi * span);
        return new PixelPass.Positional((row, column, y) => {
            float lit = (float)((1d - (depth * (0.5d + (double.CosPi(2d * (((y + 0.5d - middle) * span) + phase)) * sinc)))) / (beam.X * beam.Y * (1d - (depth / 2d))));
            for (int x = 0; x < row.Length; x++)
                row[x] *= new Vector4(lit * (strength is 0f ? Vector3.One : Vector3.Lerp(Vector3.One, mask.Coverage(column + x, y, pitch), strength)), 1f);
        });
    }

    public static Fin<Unit> Halation(PixelFrame frame, Seq<(Vector2 Sigma, float Weight)> lobes) => frame.Filter(header => {
        using Mat sum = Mat.Zeros(header.Rows, header.Cols, DepthType.Cv32F, 4);
        using Mat lobe = new();
        foreach ((Vector2 sigma, float weight) in lobes) {
            CvInvoke.GaussianBlur(header, lobe, new System.Drawing.Size(sigma.X > 0f ? 0 : 1, sigma.Y > 0f ? 0 : 1), sigma.X, sigma.Y, BorderType.Replicate);
            CvInvoke.ScaleAdd(lobe, weight, sum, sum);
        }
        CvInvoke.MixChannels(sum, header, [0, 0, 1, 1, 2, 2]);
        return unit;
    });

    public static Fin<Unit> LineNoise(Mat luma, Mat blue, Mat red, PixelFrame frame, int lines, double length, float lumaLevel, float chromaLevel, uint field) =>
        Gaussian.Kernel(length, NormType.L2).Map(taps => {
            using (taps) {
                (int width, int reach) = (taps.Rows + frame.Size.Width - 1, taps.Rows / 2);
                using Mat draws = new(frame.Size.Height, width, DepthType.Cv32F, 4);
                Span2D<Vector4> cells = draws.View<Vector4>();
                for (int y = 0; y < frame.Size.Height; y++) {
                    int scan = (int)Line(frame.Origin.Y + y, lines, frame.Extent.Height);
                    for (int column = 0; column < width; column++)
                        cells[y, column] = CoordinateHash.Normals(Vector128.Create(frame.Origin.X - reach + column, scan, 0, 0), field);
                }
                Filter(draws, taps);
                foreach ((Mat plane, float level, int channel) in Seq((luma, lumaLevel, 0), (blue, chromaLevel, 1), (red, chromaLevel, 2))) {
                    Span2D<float> target = plane.View<float>();
                    for (int y = 0; y < frame.Size.Height; y++) {
                        for (int x = 0; x < frame.Size.Width; x++)
                            target[y, x] += level * cells[y, x + reach][channel];
                    }
                }
            }
            return unit;
        });

    public static PixelPass Snow(PixelExtent extent, int lines, double length, float snow, float density, float hum, double humPhase, Vector3 luminance, uint field) =>
        new PixelPass.Positional((row, column, y) => {
            int scan = (int)Line(y, lines, extent.Height);
            float dim = (float)(1d - (hum * (1d + double.CosPi(2d * (((y + 0.5d) / extent.Height) + humPhase))) / 2d));
            for (int x = 0; x < row.Length; x++) {
                double along = (column + x + 0.5d) / length;
                double cell = double.Floor(along);
                Vector3 lit = Vector3.Lerp(row[x].AsVector3(), Vector3.One, CoordinateHash.Uniforms(Vector128.Create((int)cell, scan, 0, 0), field).X < density ? (float)(snow * (1d - (along - cell))) : 0f);
                row[x] = new Vector4(lit + new Vector3((dim - 1f) * Vector3.Dot(luminance, lit)), row[x].W);
            }
        });

    public static void Corrupted(Mat luma, Mat blue, Mat red, PixelFrame frame, float lumaLevel, float chromaLevel, Func<Vector2, Option<System.Drawing.Point>> site, uint field) {
        Span2D<float> y = luma.View<float>();
        Span2D<float> cb = blue.View<float>();
        Span2D<float> cr = red.View<float>();
        for (int row = 0; row < frame.Size.Height; row++) {
            for (int x = 0; x < frame.Size.Width; x++) {
                if (site(new Vector2(frame.Origin.X + x + 0.5f - (frame.Extent.Width / 2f), frame.Origin.Y + row + 0.5f - (frame.Extent.Height / 2f))).ToSpan() is not [var cell])
                    continue;
                Vector4 exponents = CoordinateHash.Uniforms(Vector128.Create(cell.X, cell.Y, 0, 0), field);
                (float lumaPower, float chromaPower) = (float.Exp2((4f * exponents.X) - 2f), float.Exp2((4f * exponents.Y) - 2f));
                (y[row, x], cb[row, x], cr[row, x]) = (float.Lerp(y[row, x], Bent(y[row, x], lumaPower), lumaLevel),
                    float.Lerp(cb[row, x], Bent(2f * cb[row, x], chromaPower) / 2f, chromaLevel), float.Lerp(cr[row, x], Bent(2f * cr[row, x], chromaPower) / 2f, chromaLevel));
            }
        }

        static float Bent(float value, float power) => float.CopySign(float.Pow(float.Abs(value), power), value);
    }

    public static Fin<Unit> Recompressed(PixelFrame frame, Transfer encoding, JpegEncoder encoder, int generations, Option<PixelExtent> coded) {
        const int Tile = ushort.MaxValue & ~15;
        PixelExtent size = coded.IfNone(frame.Size);
        frame.Rows((row, _, _) => encoding.Apply(row, inverted: false));
        PixelFrame working = new(System.Drawing.Point.Empty, size, size, block =>
            ImageProcessing.Resize(frame.Block, frame.Size.Width, frame.Size.Height, block, size.Width, size.Height, 4, ResizeFilter.Box, alphaChannel: 3));
        Fin<Unit> recoded = working.Mutate(image => {
            foreach (Rectangle tile in
                     from top in Enumerable.Range(0, ((size.Height - 1) / Tile) + 1)
                     from left in Enumerable.Range(0, ((size.Width - 1) / Tile) + 1)
                     select Rectangle.Intersect(new Rectangle(left * Tile, top * Tile, Tile, Tile), image.Bounds)) {
                for (int generation = 0; generation < generations; generation++) {
                    using MemoryStream stream = new();
                    using (Image<RgbaVector> piece = image.Clone(x => x.Crop(tile)))
                        piece.Save(stream, encoder);
                    stream.Position = 0;
                    using Image<Rgb24> decoded = JpegDecoder.Instance.Decode<Rgb24>(new DecoderOptions { SkipMetadata = true }, stream);
                    image.Mutate(x => x.DrawImage(decoded, tile.Location, 1f));
                }
            }
        }).Map(_ => {
            PixelFrame restored = new(frame.Origin, frame.Size, frame.Extent, block =>
                ImageProcessing.Resize(working.Block, size.Width, size.Height, block, frame.Size.Width, frame.Size.Height, 4, ResizeFilter.Triangle, alphaChannel: 3));
            frame.Rows((row, _, y) => {
                ReadOnlySpan<Vector4> scaled = restored.View.Span.GetRowSpan(y - frame.Origin.Y);
                for (int x = 0; x < row.Length; x++)
                    row[x] = new Vector4(scaled[x].AsVector3(), row[x].W);
            });
            return unit;
        });
        frame.Rows((row, _, _) => encoding.Apply(row, inverted: true));
        return recoded;
    }
}
