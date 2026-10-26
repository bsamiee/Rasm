using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using MathNet.Numerics.LinearAlgebra;
using PeterO.Numbers;
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

    public (double R, double G, double B) Coverage(long column, long row, ERational pitch) {
        ERational span = ERational.One.Divide(pitch);
        (ERational u, ERational v, ERational width) =
            (ERational.FromInt64(column).Multiply(span), ERational.FromInt64(row).Multiply(span), ERational.One.Subtract(ERational.FromDouble(_gap)));
        ERational third = ERational.One.Divide(3);
        return (Channel(0), Channel(1), Channel(2));

        double Channel(int channel) => (_rowStagger is 0d
            ? Area(u, 2, channel, 0, third).Multiply(Area(v, 1, 0, 0, width))
                .Add(Area(u, 2, channel, 1, third).Multiply(Area(v.Add(ERational.FromDouble(_columnStagger)), 1, 0, 0, width)))
            : Area(u, 1, channel, 0, third).Multiply(Area(v, 2, 0, 0, width))
                .Add(Area(u.Add(ERational.FromDouble(_rowStagger)), 1, channel, 0, third).Multiply(Area(v, 2, 0, 1, width))))
            .Multiply(3).Divide(span.Multiply(span).Multiply(width)).ToDouble();

        ERational Area(ERational t, int period, int channel, int parity, ERational open) {
            return Primitive(t.Add(span)).Subtract(Primitive(t));

            ERational Primitive(ERational point) => LatticePoint.Split(point.Divide(period)) switch {
                var (cell, local) => ERational.FromEInteger(cell).Multiply(open).Add(
                    ERational.Min(ERational.Max(local.Multiply(period).Subtract(parity).Subtract(third.Multiply(channel)), ERational.Zero), open)),
            };
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SystemM {
    public const double Subcarrier = 188.3d;

    public static long Frame(double seconds) => LatticePoint.Split(ERational.FromDouble(seconds).Multiply(30000).Divide(1001)).Cell.ToInt64Checked();

    public static EInteger Line(long row, int lines, int height) => LatticePoint.Split(ERational.FromInt64(row).Multiply(lines).Divide(height)).Cell;
}

public static unsafe class VideoKernels {
    private const double NearShare = 0.890909d;
    private const int JpegTile = 65504;

    public static Fin<Mat> Unit(double sigma) => Gaussian.Kernel(sigma).Map(static taps => {
        CvInvoke.Normalize(taps, taps, 1d, 0d, NormType.L2);
        return taps;
    });

    public static Fin<Mat> Echo(double sigma, double delay, float share) => PixelSampling.KernelTaps<int>(ERational.FromDouble(delay).Abs().Add(ERational.FromDouble(sigma).Multiply(3))).Map(count => {
        long integral = long.CreateChecked(Math.Round(delay, MidpointRounding.ToEven));
        double fraction = delay - integral;
        double nearest = Math.Abs(fraction);
        double[] shape = [.. Enumerable.Range(0, count)
            .Select(tap => Math.Abs(tap - (count / 2) + integral + fraction))
            .Select(distance => sigma > 0d ? Math.Exp((distance - nearest) * (distance + nearest) * -0.5d / sigma / sigma) : Math.Max(1d - distance, 0d))];
        double total = shape.Sum();
        Mat taps = new(count, 1, DepthType.Cv64F, 1);
        taps.SetTo([.. shape.Select((weight, tap) => (share * weight / total) + (tap == count / 2 ? 1d - share : 0d))]);
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
        PixelExtent extent, int lines, bool interlaced, long count, (ERational Width, ERational Height) scale, PhosphorMask mask, ERational pitch, double maskStrength, double depth) {
        ERational middle = ERational.FromInt32(extent.Height).Divide(2);
        ERational field = ERational.FromInt32(extent.Height).Multiply(interlaced ? 2 : 1);
        ERational parity = ERational.FromInt64(interlaced ? count & 1L : 0L).Divide(2);
        double power = ERational.One.Divide(scale.Width.Multiply(scale.Height)).ToDouble();
        ERational span = ERational.FromInt32(lines).Divide(field.Multiply(scale.Height));
        ERational phase = middle.Multiply(lines).Divide(field).Add(parity);
        (EInteger whole, ERational fraction) = LatticePoint.Split(span);
        double sinc = (whole.IsEven ? 1d : -1d) * double.SinPi(ERational.Min(fraction, ERational.One.Subtract(fraction)).ToDouble()) / (2d * double.Pi * span.ToDouble());
        return (row, column, line) => {
            long y = (long)extent.Height - 1 - line;
            ERational center = ERational.FromInt64((2 * y) + 1).Divide(2).Subtract(middle);
            double beam = LatticePoint.Split(center.Multiply(span).Add(phase)).Local.ToDouble();
            double average = 0.5d + (double.CosPi(2d * beam) * sinc);
            double lit = power * (1d - (depth * average)) / (1d - (depth / 2d));
            for (int x = 0; x < row.Length; x++) {
                (double R, double G, double B) = maskStrength is 0d ? (1d, 1d, 1d) : mask.Coverage(column + x, y, pitch);
                row[x] = new Vector4(
                    (float)(row[x].X * (1d - maskStrength + (maskStrength * R)) * lit),
                    (float)(row[x].Y * (1d - maskStrength + (maskStrength * G)) * lit),
                    (float)(row[x].Z * (1d - maskStrength + (maskStrength * B)) * lit), row[x].W);
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
            ERational cycles = ERational.FromDouble(SystemM.Subcarrier).Divide(frame.Extent.Width);
            (double Sin, double Cos)[] carrier = [.. Enumerable.Range(0, frame.Size.Width).Select(x =>
                double.SinCosPi(2d * LatticePoint.Split(cycles.Multiply((long)frame.Origin.X + x)).Local.ToDouble()))];
            for (int y = 0; y < frame.Size.Height; y++) {
                ReadOnlySpan<double> luminance = new(detail.GetDataPointer(y).ToPointer(), frame.Size.Width);
                Span<double> cb = new(blue.GetDataPointer(y).ToPointer(), frame.Size.Width);
                Span<double> cr = new(red.GetDataPointer(y).ToPointer(), frame.Size.Width);
                float signed = SystemM.Line((long)frame.Origin.Y + y, lines, frame.Extent.Height).Add(count).IsEven ? amount : -amount;
                for (int x = 0; x < luminance.Length; x++)
                    (cb[x], cr[x]) = (cb[x] + (signed * luminance[x] * carrier[x].Cos), cr[x] + (signed * luminance[x] * carrier[x].Sin));
            }
            return unit;
        });
    }

    public static Fin<Unit> LineNoise(Mat luma, Mat blue, Mat red, PixelFrame frame, int lines, double length, float lumaLevel, float chromaLevel, uint field) =>
        Unit(length).Bind(taps => {
            using (taps) {
                if ((long)taps.Rows + frame.Size.Width - 1 > int.MaxValue / 3)
                    return Fin.Fail<Unit>(Error.New(string.Create(CultureInfo.InvariantCulture, $"Line noise of {taps.Rows} taps across {frame.Size.Width} pixels exceeds native image limits")));
                (int width, int reach) = (taps.Rows + frame.Size.Width - 1, taps.Rows / 2);
                using Mat plane = new(frame.Size.Height, width, DepthType.Cv64F, 3);
                using Mat row = taps.Reshape(1, 1);
                for (int line = 0; line < frame.Size.Height; line++) {
                    EInteger scan = SystemM.Line((long)frame.Origin.Y + line, lines, frame.Extent.Height);
                    Span<double> cells = new(plane.GetDataPointer(line).ToPointer(), 3 * width);
                    for (int column = 0; column < width; column++) {
                        Vector4 draw = CoordinateHash.Normals(EInteger.FromInt64(frame.Origin.X - reach + column), scan, field);
                        (cells[3 * column], cells[(3 * column) + 1], cells[(3 * column) + 2]) = (draw.X, draw.Y, draw.Z);
                    }
                }
                CvInvoke.Filter2D(plane, plane, row, new System.Drawing.Point(-1, -1), 0d, BorderType.Replicate);
                for (int y = 0; y < frame.Size.Height; y++) {
                    ReadOnlySpan<double> noise = new ReadOnlySpan<double>(plane.GetDataPointer(y).ToPointer(), 3 * width).Slice(3 * reach, 3 * frame.Size.Width);
                    Span<double> luminance = new(luma.GetDataPointer(y).ToPointer(), frame.Size.Width);
                    Span<double> cb = new(blue.GetDataPointer(y).ToPointer(), frame.Size.Width);
                    Span<double> cr = new(red.GetDataPointer(y).ToPointer(), frame.Size.Width);
                    for (int x = 0; x < luminance.Length; x++)
                        (luminance[x], cb[x], cr[x]) = (luminance[x] + (noise[3 * x] * lumaLevel), cb[x] + (noise[(3 * x) + 1] * chromaLevel), cr[x] + (noise[(3 * x) + 2] * chromaLevel));
                }
                return unit;
            }
        });

    public static Action<Span<Vector4>, long, long> Snow(PixelExtent extent, int lines, ERational length, float snow, float density, float hum, ERational humPhase, (double R, double G, double B) luminance, uint field) =>
        (row, column, line) => {
            long y = (long)extent.Height - 1 - line;
            EInteger scan = SystemM.Line(y, lines, extent.Height);
            double cycle = LatticePoint.Split(ERational.FromInt64((2 * y) + 1).Divide(2).Divide(extent.Height).Add(humPhase)).Local.ToDouble();
            double dim = 1d - (hum * (1d + double.CosPi(2d * cycle)) / 2d);
            for (int x = 0; x < row.Length; x++) {
                double lit = 0d;
                if (snow is not 0f && density is not 0f) {
                    (EInteger cell, ERational local) = LatticePoint.Split(ERational.FromInt64((2 * (column + x)) + 1).Divide(2).Divide(length));
                    lit = CoordinateHash.Uniforms(cell, scan, field).X < density ? snow * ERational.One.Subtract(local).ToDouble() : 0d;
                }
                if (lit is 0d && hum is 0f)
                    continue;
                (double r, double g, double b) = lit is 0d ? (row[x].X, row[x].Y, row[x].Z)
                    : (double.Lerp(row[x].X, 1d, lit), double.Lerp(row[x].Y, 1d, lit), double.Lerp(row[x].Z, 1d, lit));
                double shift = (dim - 1d) * ((r * luminance.R) + (g * luminance.G) + (b * luminance.B));
                row[x] = new Vector4((float)(r + shift), (float)(g + shift), (float)(b + shift), row[x].W);
            }
        };

    public static Action<Span<Vector4>, long, long> Corrupted(Gamut signal, float luma, float chroma, PixelExtent extent, Func<(ERational X, ERational Y), Option<(EInteger X, EInteger Y)>> site, uint field) {
        (Matrix<double> forward, Matrix<double> inverse) = (Gamut.ToYpbpr(signal.Ybr), Gamut.FromYpbpr(signal.Ybr));
        Vector256<double> yAxis = Vector256.Create(forward[0, 0], forward[0, 1], forward[0, 2], 0d);
        Vector256<double> cbAxis = Vector256.Create(forward[1, 0], forward[1, 1], forward[1, 2], 0d);
        Vector256<double> crAxis = Vector256.Create(forward[2, 0], forward[2, 1], forward[2, 2], 0d);
        Vector256<double> rAxis = Vector256.Create(inverse[0, 0], inverse[0, 1], inverse[0, 2], 0d);
        Vector256<double> gAxis = Vector256.Create(inverse[1, 0], inverse[1, 1], inverse[1, 2], 0d);
        Vector256<double> bAxis = Vector256.Create(inverse[2, 0], inverse[2, 1], inverse[2, 2], 0d);
        return (row, column, line) => {
            ERational y = ERational.FromInt64(extent.Height - (2 * line) - 1).Divide(2);
            for (int x = 0; x < row.Length; x++) {
                if (site((ERational.FromInt64((2 * (column + x)) + 1 - extent.Width).Divide(2), y)).Case is not (EInteger across, EInteger down))
                    continue;
                Vector4 exponents = CoordinateHash.Uniforms(across, down, field);
                (double yPower, double cPower) = (double.Exp2((4d * exponents.X) - 2d), double.Exp2((4d * exponents.Y) - 2d));
                Vector256<double> color = Vector256.Create(row[x].X, row[x].Y, row[x].Z, 0d);
                (double yValue, double cb, double cr) = (Vector256.Dot(color, yAxis), Vector256.Dot(color, cbAxis), Vector256.Dot(color, crAxis));
                Vector256<double> delta = Vector256.Create(
                    luma is 0f ? 0d : luma * (double.CopySign(double.Pow(double.Abs(yValue), yPower), yValue) - yValue),
                    chroma is 0f ? 0d : chroma * (double.CopySign(0.5d * double.Pow(double.Abs(2d * cb), cPower), cb) - cb),
                    chroma is 0f ? 0d : chroma * (double.CopySign(0.5d * double.Pow(double.Abs(2d * cr), cPower), cr) - cr), 0d);
                row[x] = new Vector4(
                    (float)(row[x].X + Vector256.Dot(delta, rAxis)),
                    (float)(row[x].Y + Vector256.Dot(delta, gAxis)),
                    (float)(row[x].Z + Vector256.Dot(delta, bAxis)), row[x].W);
            }
        };
    }

    public static Fin<Unit> Recompressed(PixelFrame frame, JpegEncoder encoder, int generations, Option<int> coded, IProgress<int> progress) {
        int height = coded.IfNone(frame.Size.Height);
        double width = double.Max(1d, Math.Round((double)frame.Size.Width * height / frame.Size.Height, MidpointRounding.ToEven));
        return (width > int.MaxValue ? Error.New(string.Create(CultureInfo.InvariantCulture, $"Coded frame {width}x{height} exceeds native image limits")) : PixelExtent.Validated((int)width, height))
            .Bind(size => {
                PixelFrame working = size == frame.Size ? frame : new(System.Drawing.Point.Empty, size, size, block =>
                    ImageProcessing.Resize(frame.Block, frame.Size.Width, frame.Size.Height, block, size.Width, size.Height, 4, ResizeFilter.Box, alphaChannel: 3));
                return working.Mutate(source => {
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
                }).Map(_ => {
                    if (working != frame) {
                        PixelFrame restored = new(frame.Origin, frame.Size, frame.Extent, block =>
                            ImageProcessing.Resize(working.Block, size.Width, size.Height, block, frame.Size.Width, frame.Size.Height, 4, ResizeFilter.Triangle, alphaChannel: 3));
                        for (int y = 0; y < frame.Size.Height; y++) {
                            Span<Vector4> target = frame.View.Span.GetRowSpan(y);
                            Span<Vector4> scaled = restored.View.Span.GetRowSpan(y);
                            for (int x = 0; x < target.Length; x++)
                                target[x] = scaled[x] with { W = target[x].W };
                        }
                    }
                    progress.Report(frame.Size.Height);
                    return unit;
                });
            });
    }

    private static Image<Rgb24> Generation<TPixel>(Image<TPixel> image, JpegEncoder encoder) where TPixel : unmanaged, IPixel<TPixel> {
        using MemoryStream stream = new();
        image.Save(stream, encoder);
        stream.Position = 0;
        return JpegDecoder.Instance.Decode<Rgb24>(new DecoderOptions { SkipMetadata = true }, stream);
    }
}
