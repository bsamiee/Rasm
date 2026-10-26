using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TinyEXR;

namespace Rasm.Imaging.Filtering;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PhosphorMask {
    public static readonly PhosphorMask ApertureGrille = new("aperture-grille", gap: 0d, columnStagger: 0d, rowStagger: 0d);
    public static readonly PhosphorMask SlotMask = new("slot-mask", gap: 1d / 3d, columnStagger: 0.5d, rowStagger: 0d);
    public static readonly PhosphorMask ShadowMask = new("shadow-mask", gap: 1d / 3d, columnStagger: 0d, rowStagger: 0.5d);

    private readonly double _gap;
    private readonly double _columnStagger;
    private readonly double _rowStagger;

    public (double R, double G, double B) Coverage(long column, long row, double pitch) {
        const double third = 1d / 3d;
        double span = 1d / pitch;
        (double u, double v, double width) = (column * span, row * span, 1d - _gap);
        return (Channel(0), Channel(1), Channel(2));

        double Channel(int channel) => (_rowStagger is 0d
            ? (Area(u, 2, channel, 0, third) * Area(v, 1, 0, 0, width)) + (Area(u, 2, channel, 1, third) * Area(v + _columnStagger, 1, 0, 0, width))
            : (Area(u, 1, channel, 0, third) * Area(v, 2, 0, 0, width)) + (Area(u + _rowStagger, 1, channel, 0, third) * Area(v, 2, 0, 1, width)))
            * 3d / (span * span * width);

        double Area(double t, int period, int channel, int parity, double open) {
            return Primitive(t + span) - Primitive(t);

            double Primitive(double point) => (point / period) switch {
                var scaled => double.Floor(scaled) switch {
                    var cell => (cell * open) + double.Min(double.Max(((scaled - cell) * period) - parity - (third * channel), 0d), open),
                },
            };
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SystemM {
    public const double Subcarrier = 188.3d;

    public static long Frame(double seconds) => (long)double.Floor(seconds * 30000d / 1001d);

    public static long Line(long row, int lines, int height) =>
        Int128.DivRem((Int128)row * lines, height) switch { var (quotient, remainder) => (long)(remainder < 0 ? quotient - 1 : quotient) };
}

public static class VideoKernels {
    private const double NearShare = 0.890909d;
    private const int JpegTile = 65504;

    public static Fin<Mat> Unit(double sigma) => Gaussian.Kernel(sigma).Map(static taps => {
        CvInvoke.Normalize(taps, taps, 1d, 0d, NormType.L2);
        return taps;
    });

    public static Fin<Mat> Echo(double sigma, double delay, float share) => PixelSampling.KernelTaps<int>(double.Abs(delay) + (3d * sigma)).Map(count => {
        long integral = long.CreateChecked(Math.Round(delay, MidpointRounding.ToEven));
        double fraction = delay - integral;
        double nearest = Math.Abs(fraction);
        double[] shape = [.. Enumerable.Range(0, count)
            .Select(tap => Math.Abs(tap - (count / 2) + integral + fraction))
            .Select(distance => sigma > 0d ? Math.Exp((distance - nearest) * (distance + nearest) * -0.5d / sigma / sigma) : Math.Max(1d - distance, 0d))];
        TensorPrimitives.Multiply(shape, share / TensorPrimitives.Sum(shape), shape);
        shape[count / 2] += 1d - share;
        Mat taps = new(count, 1, DepthType.Cv64F, 1);
        taps.SetTo(shape);
        return taps;
    });

    public static Mat Transform(Matrix<double> linear) {
        double[] rows = linear.Resize(4, 4).ToRowMajorArray();
        rows[^1] = 1d;
        Mat matrix = new(4, 4, DepthType.Cv64F, 1);
        matrix.SetTo(rows);
        return matrix;
    }

    public static Fin<Unit> Filter(Mat plane, Fin<Mat> taps, bool vertical = false) => taps.Map(kernel => {
        using (kernel)
        using (Mat impulse = Mat.Ones(1, 1, DepthType.Cv64F, 1))
            CvInvoke.SepFilter2D(plane, plane, DepthType.Cv64F, vertical ? impulse : kernel, vertical ? kernel : impulse, new System.Drawing.Point(-1, -1), 0d, BorderType.Replicate);
        return unit;
    });

    public static Fin<Unit> Decomposed(Mat header, Gamut signal, Func<Mat, Mat, Mat, Fin<Unit>> planes) {
        using Mat alpha = new();
        using Mat light = new();
        using Mat forward = Transform(Gamut.ToYpbpr(signal.Ybr));
        using Mat inverse = Transform(Gamut.FromYpbpr(signal.Ybr));
        using Mat ycc = new();
        using VectorOfMat split = new();
        CvInvoke.ExtractChannel(header, alpha, 3);
        header.ConvertTo(light, DepthType.Cv64F);
        CvInvoke.Transform(light, ycc, forward);
        CvInvoke.Split(ycc, split);
        using Mat luma = split[0];
        using Mat blue = split[1];
        using Mat red = split[2];
        return planes(luma, blue, red).Map(_ => {
            CvInvoke.Merge(split, ycc);
            CvInvoke.Transform(ycc, light, inverse);
            light.ConvertTo(header, DepthType.Cv32F);
            CvInvoke.InsertChannel(alpha, header, 3);
            return unit;
        });
    }

    public static Action<Span<Vector4>, long, long> Raster(
        PixelExtent extent, int lines, bool interlaced, long count, (double Width, double Height) scale, PhosphorMask mask, double pitch, double maskStrength, double depth) {
        double middle = extent.Height / 2d;
        double field = (double)extent.Height * (interlaced ? 2 : 1);
        double parity = (interlaced ? count & 1L : 0L) / 2d;
        double power = 1d / (scale.Width * scale.Height);
        double span = lines / (field * scale.Height);
        double phase = (middle * lines / field) + parity;
        (double whole, double fraction) = double.Floor(span) switch { var floor => (floor, span - floor) };
        double sinc = (double.IsEvenInteger(whole) ? 1d : -1d) * double.SinPi(double.Min(fraction, 1d - fraction)) / (2d * double.Pi * span);
        return (row, column, line) => {
            long y = (long)extent.Height - 1 - line;
            double beam = ((((((2 * y) + 1) / 2d) - middle) * span) + phase) switch { var turns => turns - double.Floor(turns) };
            double average = 0.5d + (double.CosPi(2d * beam) * sinc);
            double lit = power * (1d - (depth * average)) / (1d - (depth / 2d));
            for (int x = 0; x < row.Length; x++) {
                Vector256<double> coverage = maskStrength is 0d ? Vector256.Create(1d) : mask.Coverage(column + x, y, pitch) switch { var (r, g, b) => Vector256.Create(r, g, b, 0d) };
                Vector256<double> shaded = Vector256.Create(row[x].X, row[x].Y, row[x].Z, 0d) * (Vector256.Create(1d - maskStrength) + (maskStrength * coverage)) * lit;
                row[x] = new Vector4((float)shaded[0], (float)shaded[1], (float)shaded[2], row[x].W);
            }
        };
    }

    public static Fin<Unit> Glow(PixelFrame frame, double sigma) =>
        frame.Filter(header => (PixelSampling.GaussianTaps<int>(sigma), PixelSampling.GaussianTaps<int>(4d * sigma)).Apply((taps, wide) => {
            using Mat alpha = new();
            using Mat light = new();
            using Mat near = new();
            using Mat far = new();
            CvInvoke.ExtractChannel(header, alpha, 3);
            header.ConvertTo(light, DepthType.Cv64F);
            CvInvoke.GaussianBlur(light, near, new System.Drawing.Size(taps, taps), sigma, sigma, BorderType.Replicate);
            CvInvoke.GaussianBlur(near, far, new System.Drawing.Size(wide, taps), 4d * sigma, sigma, BorderType.Replicate);
            CvInvoke.AddWeighted(near, NearShare, far, 1d - NearShare, 0d, light);
            light.ConvertTo(header, DepthType.Cv32F);
            CvInvoke.InsertChannel(alpha, header, 3);
            return unit;
        }).As());

    public static Fin<Unit> CrossColor(Mat luma, Mat blue, Mat red, PixelFrame frame, int lines, long count, float amount) {
        using Mat detail = luma.Clone();
        return Filter(detail, Gaussian.Kernel(frame.Extent.Width / (6d * SystemM.Subcarrier))).Map(_ => {
            CvInvoke.Subtract(luma, detail, detail);
            (int width, double cycles) = (frame.Size.Width, SystemM.Subcarrier / frame.Extent.Width);
            using SpanOwner<double> phases = SpanOwner<double>.Allocate(width);
            using SpanOwner<double> sin = SpanOwner<double>.Allocate(width);
            using SpanOwner<double> cos = SpanOwner<double>.Allocate(width);
            using SpanOwner<double> scratch = SpanOwner<double>.Allocate(width);
            for (int x = 0; x < width; x++)
                phases.Span[x] = (cycles * ((long)frame.Origin.X + x)) switch { var turns => 2d * (turns - double.Floor(turns)) };
            TensorPrimitives.SinCosPi(phases.Span, sin.Span, cos.Span);
            ReadOnlySpan2D<double> luminance = detail.View<double>();
            Span2D<double> cb = blue.View<double>();
            Span2D<double> cr = red.View<double>();
            for (int y = 0; y < frame.Size.Height; y++) {
                double signed = long.IsEvenInteger(SystemM.Line((long)frame.Origin.Y + y, lines, frame.Extent.Height) + count) ? amount : -amount;
                TensorPrimitives.Multiply(luminance.GetRowSpan(y), cos.Span, scratch.Span);
                TensorPrimitives.MultiplyAdd(scratch.Span, signed, cb.GetRowSpan(y), cb.GetRowSpan(y));
                TensorPrimitives.Multiply(luminance.GetRowSpan(y), sin.Span, scratch.Span);
                TensorPrimitives.MultiplyAdd(scratch.Span, signed, cr.GetRowSpan(y), cr.GetRowSpan(y));
            }
            return unit;
        });
    }

    public static Fin<Unit> LineNoise(Mat luma, Mat blue, Mat red, PixelFrame frame, int lines, double length, float lumaLevel, float chromaLevel, uint field) =>
        Unit(length).Bind(taps => {
            using (taps) {
                if ((long)taps.Rows + frame.Size.Width - 1 > int.MaxValue)
                    return Fin.Fail<Unit>(new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Line noise of {taps.Rows} taps across {frame.Size.Width} pixels exceeds native image limits")));
                (int width, int reach) = (taps.Rows + frame.Size.Width - 1, taps.Rows / 2);
                using Mat draws = new(frame.Size.Height, width, DepthType.Cv64F, 4);
                using Mat row = taps.Reshape(1, 1);
                using VectorOfMat planes = new();
                Span2D<Vector256<double>> cells = draws.View<Vector256<double>>();
                for (int line = 0; line < frame.Size.Height; line++) {
                    long scan = SystemM.Line((long)frame.Origin.Y + line, lines, frame.Extent.Height);
                    for (int column = 0; column < width; column++)
                        cells[line, column] = Vector256.WidenLower(CoordinateHash.Normals((long)frame.Origin.X - reach + column, scan, field).AsVector128().ToVector256());
                }
                CvInvoke.Filter2D(draws, draws, row, new System.Drawing.Point(-1, -1), 0d, BorderType.Replicate);
                CvInvoke.Split(draws, planes);
                foreach ((int channel, Mat signal, float level) in Seq((0, luma, lumaLevel), (1, blue, chromaLevel), (2, red, chromaLevel))) {
                    using Mat plane = planes[channel];
                    ReadOnlySpan2D<double> noise = plane.View<double>();
                    Span2D<double> target = signal.View<double>();
                    for (int line = 0; line < frame.Size.Height; line++)
                        TensorPrimitives.MultiplyAdd(noise.GetRowSpan(line).Slice(reach, frame.Size.Width), level, target.GetRowSpan(line), target.GetRowSpan(line));
                }
                return unit;
            }
        });

    public static Action<Span<Vector4>, long, long> Snow(PixelExtent extent, int lines, double length, float snow, float density, float hum, double humPhase, Gamut gamut, uint field) =>
        (row, column, line) => {
            (long y, (double R, double G, double B) luminance) = ((long)extent.Height - 1 - line, gamut.Luminance);
            long scan = SystemM.Line(y, lines, extent.Height);
            double cycle = ((((2 * y) + 1) / 2d / extent.Height) + humPhase) switch { var turns => turns - double.Floor(turns) };
            double dim = 1d - (hum * (1d + double.CosPi(2d * cycle)) / 2d);
            for (int x = 0; x < row.Length; x++) {
                double lit = 0d;
                if (snow is not 0f && density is not 0f) {
                    double along = ((2 * (column + x)) + 1) / 2d / length;
                    double cell = double.Floor(along);
                    lit = CoordinateHash.Uniforms((long)cell, scan, field).X < density ? snow * (1d - (along - cell)) : 0d;
                }
                if (lit is 0d && hum is 0f)
                    continue;
                (double r, double g, double b) = lit is 0d ? (row[x].X, row[x].Y, row[x].Z)
                    : (double.Lerp(row[x].X, 1d, lit), double.Lerp(row[x].Y, 1d, lit), double.Lerp(row[x].Z, 1d, lit));
                double shift = (dim - 1d) * ((r * luminance.R) + (g * luminance.G) + (b * luminance.B));
                row[x] = new Vector4((float)(r + shift), (float)(g + shift), (float)(b + shift), row[x].W);
            }
        };

    public static Action<Span<Vector4>, long, long> Corrupted(Gamut signal, float luma, float chroma, PixelExtent extent, Func<(double X, double Y), Option<(long X, long Y)>> site, uint field) {
        (Vector256<double> yAxis, Vector256<double> cbAxis, Vector256<double> crAxis) = Lanes.Of(Gamut.ToYpbpr(signal.Ybr).Transpose());
        (Vector256<double> rAxis, Vector256<double> gAxis, Vector256<double> bAxis) = Lanes.Of(Gamut.FromYpbpr(signal.Ybr).Transpose());
        return (row, column, line) => {
            double y = (extent.Height - (2 * line) - 1) / 2d;
            for (int x = 0; x < row.Length; x++) {
                if (site((((2 * (column + x)) + 1 - extent.Width) / 2d, y)).Case is not (long across, long down))
                    continue;
                Vector4 exponents = CoordinateHash.Uniforms(across, down, field);
                (double yPower, double cPower) = (double.Exp2((4d * exponents.X) - 2d), double.Exp2((4d * exponents.Y) - 2d));
                Vector256<double> color = Vector256.Create(row[x].X, row[x].Y, row[x].Z, 0d);
                (double yValue, double cb, double cr) = (Vector256.Dot(color, yAxis), Vector256.Dot(color, cbAxis), Vector256.Dot(color, crAxis));
                Vector256<double> delta = Vector256.Create(
                    luma is 0f ? 0d : luma * (double.CopySign(double.Pow(double.Abs(yValue), yPower), yValue) - yValue),
                    chroma is 0f ? 0d : chroma * (double.CopySign(0.5d * double.Pow(double.Abs(2d * cb), cPower), cb) - cb),
                    chroma is 0f ? 0d : chroma * (double.CopySign(0.5d * double.Pow(double.Abs(2d * cr), cPower), cr) - cr), 0d);
                Vector256<double> shifted = color + Vector256.Create(Vector256.Dot(delta, rAxis), Vector256.Dot(delta, gAxis), Vector256.Dot(delta, bAxis), 0d);
                row[x] = new Vector4((float)shifted[0], (float)shifted[1], (float)shifted[2], row[x].W);
            }
        };
    }

    public static Fin<Unit> Recompressed(PixelFrame frame, JpegEncoder encoder, int generations, Option<int> coded, IProgress<int> progress) {
        int height = coded.IfNone(frame.Size.Height);
        double width = double.Max(1d, Math.Round((double)frame.Size.Width * height / frame.Size.Height, MidpointRounding.ToEven));
        return from size in width > int.MaxValue ? new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Coded frame {width}x{height} exceeds native image limits")) : PixelExtent.Validated((int)width, height)
               let working = size == frame.Size ? frame : new PixelFrame(System.Drawing.Point.Empty, size, size, block =>
                   ImageProcessing.Resize(frame.Block, frame.Size.Width, frame.Size.Height, block, size.Width, size.Height, 4, ResizeFilter.Box, alphaChannel: 3))
               from _ in working.Mutate(source => {
                   foreach (Rectangle tile in
                            from top in Enumerable.Range(0, ((source.Height - 1) / JpegTile) + 1)
                            from left in Enumerable.Range(0, ((source.Width - 1) / JpegTile) + 1)
                            select new Rectangle(left * JpegTile, top * JpegTile, int.Min(JpegTile, source.Width - (left * JpegTile)), int.Min(JpegTile, source.Height - (top * JpegTile)))) {
                       using Image<RgbaVector> piece = source.Clone(x => x.Crop(tile));
                       using Image<Rgb24> recoded = toSeq(Enumerable.Range(2, generations - 1)).Fold(Generation(piece, encoder), (image, _) => {
                           using (image)
                               return Generation(image, encoder);
                       });
                       source.Mutate(x => x.DrawImage(recoded, tile.Location, 1f));
                   }
               })
               select Restored(frame, working, progress);
    }

    private static Unit Restored(PixelFrame frame, PixelFrame working, IProgress<int> progress) {
        if (working != frame) {
            PixelFrame restored = new(frame.Origin, frame.Size, frame.Extent, block =>
                ImageProcessing.Resize(working.Block, working.Size.Width, working.Size.Height, block, frame.Size.Width, frame.Size.Height, 4, ResizeFilter.Triangle, alphaChannel: 3));
            using Mat target = frame.Header();
            using Mat scaled = restored.Header();
            CvInvoke.MixChannels(scaled, target, [0, 0, 1, 1, 2, 2]);
        }
        progress.Report(frame.Size.Height);
        return unit;
    }

    private static Image<Rgb24> Generation<TPixel>(Image<TPixel> image, JpegEncoder encoder) where TPixel : unmanaged, IPixel<TPixel> {
        using MemoryStream stream = new();
        image.Save(stream, encoder);
        stream.Position = 0;
        return JpegDecoder.Instance.Decode<Rgb24>(new DecoderOptions { SkipMetadata = true }, stream);
    }
}
