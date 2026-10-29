using System.Numerics;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Warping;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class MapReading {
    public static readonly MapReading Gradient = new("gradient");
    public static readonly MapReading Luma = new("luma");
    public static readonly MapReading Offsets = new("offsets");

    public PixelPass.Frame Pass(Option<PixelFrame> map, Option<ShortSideLength> tile, ShortSideLength softness, Vector2 amount, float rotation, Vector3 luminance, float exposure, Sampling sampling, WrapMode wrap) =>
        new((frame, progress) =>
            from source in map.Match(Some: image => Fitted(image, tile, frame), None: () => frame)
            from offsets in Try.lift(() => Read(source, frame, softness.Pixels(frame.Extent), amount * frame.Extent.ShortSide, rotation, luminance, exposure)).Run()
            from applied in new CoordinateMap.Offsets(offsets, sampling, (wrap, wrap)).Apply(frame, progress)
            select applied);

    internal static void Slope(Mat level, Mat plane, double scale) {
        using Mat across = new();
        using Mat down = new();
        CvInvoke.Sobel(level, across, DepthType.Cv32F, 1, 0, 3, scale, 0d, BorderType.Replicate);
        CvInvoke.Sobel(level, down, DepthType.Cv32F, 0, 1, 3, scale, 0d, BorderType.Replicate);
        using VectorOfMat pair = new(across, down);
        CvInvoke.Merge(pair, plane);
    }

    private PixelFrame Read(PixelFrame source, PixelFrame frame, double softness, Vector2 reach, float rotation, Vector3 luminance, float exposure) {
        using Mat map = source.Header();
        using Mat plane = new();
        Switch((Map: map, Plane: plane, Softness: softness, Luminance: luminance, Exposure: exposure),
            gradient: static state => Slope(Relief(state.Map, state.Plane, state.Softness, state.Luminance, state.Exposure), state.Plane, -double.Sqrt(double.Tau) * state.Softness / 8d),
            luma: static state => Transformed(Relief(state.Map, state.Plane, state.Softness, state.Luminance, state.Exposure), state.Plane, 2, 2d, -1d, 2d, -1d),
            offsets: static state => Transformed(state.Map, state.Plane, 2, 2d, 0d, 0d, 0d, -1d, 0d, -2d, 0d, 0d, 1d));
        PixelFrame offsets = new(frame.Origin, frame.Size, frame.Extent);
        using Mat header = offsets.Header();
        (float sin, float cos) = float.SinCos(-rotation);
        Transformed(plane, header, 4, -reach.X * cos, reach.Y * sin, 0d, -reach.X * sin, -reach.Y * cos, 0d, 0d, 0d, 0d, 0d, 0d, 1d);
        return offsets;
    }

    private static Mat Relief(Mat map, Mat height, double softness, Vector3 luminance, float exposure) {
        Transformed(map, height, 1, luminance.X, luminance.Y, luminance.Z, 0d);
        Span<float> levels = height.GetSpan<float>();
        float gray = Metering.MiddleGray * float.Exp2(-exposure);
        TensorPrimitives.Max(levels, 0f, levels);
        TensorPrimitives.Add(levels, gray, levels);
        TensorPrimitives.Divide(gray, levels, levels);
        TensorPrimitives.Subtract(1f, levels, levels);
        height.Blur(height, softness);
        return height;
    }

    private static void Transformed(Mat source, Mat target, int rows, params double[] weights) {
        using Mat matrix = new(rows, weights.Length / rows, DepthType.Cv64F, 1);
        matrix.SetTo(weights);
        CvInvoke.Transform(source, target, matrix);
    }

    private static Fin<PixelFrame> Fitted(PixelFrame image, Option<ShortSideLength> tile, PixelFrame frame) =>
        tile.Match(
            Some: length => (Scale: new Vector2((float)(image.Extent.Height / length.Pixels(frame.Extent))), Edge: WrapMode.Periodic),
            None: () => (Scale: new Vector2(image.Extent.Width, image.Extent.Height) / new Vector2(frame.Extent.Width, frame.Extent.Height), Edge: WrapMode.Clamp)) switch {
                var (scale, edge) => new CoordinateMap.Planar(Matrix3x2.CreateTranslation(frame.Origin.X, frame.Origin.Y) * Matrix3x2.CreateScale(scale), Sampling.Linear, (edge, edge)).Resample(image, frame.Size),
            };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Refraction {
    public static Func<Vector2, Vector4> Blocks { get; } = Glass(static cell =>
        Vector2.Abs((2f * Fraction(cell)) - Vector2.One) switch { var edge => (Vector2.One - (edge * edge * edge * edge)) switch { var bevel => bevel.X * bevel.Y } });
    public static Func<Vector2, Vector4> Canvas { get; } = Glass(static cell => 0.5f + (0.5f * float.SinPi(2f * cell.X) * float.SinPi(2f * cell.Y)));
    public static Func<Vector2, Vector4> TinyLens { get; } = Glass(static cell => float.Sqrt(float.Max(0f, 1f - (4f * (Fraction(cell) - new Vector2(0.5f)).LengthSquared()))));

    public static Func<Vector2, Vector4> Frosted(Octaves octaves, uint field) =>
        Glass(cell => 0.5f + (0.5f * Fractal.Fbm.Sample(NoiseDimensions.Two, new Vector4(cell, 0f, 0f), octaves, 0f, 0f, CoordinateHash.Branch(field, 2u))));

    public static Func<Vector2, Vector4> Raindrops(float density, float size, float variation, float softness, float lens, float trails, float direction, float fall, uint field) {
        (Matrix3x2 down, Matrix3x2 up) = (Matrix3x2.CreateRotation(direction + (float.Pi / 2f)), Matrix3x2.CreateRotation(-direction - (float.Pi / 2f)));
        return cell => {
            Vector2 at = Vector2.Transform(cell, down) - new Vector2(0f, fall);
            Vector4 draw = NoiseDimensions.Two.White(new Vector4(at, 0f, 0f), field);
            Vector2 off = Fraction(at) - new Vector2(0.25f) - (0.5f * new Vector2(draw.Y, draw.Z));
            (float radius, float lag) = (0.5f * size * (1f - (variation * draw.W)), off.Y <= 0f ? trails * (1f + (off.Y / (0.25f + (0.5f * draw.Z)))) : 0f);
            float drop = DistanceField.SmoothStep(0f, radius * softness, radius - off.Length());
            float trail = DistanceField.SmoothStep(0f, 0.5f * radius * softness, (0.5f * radius) - float.Abs(off.X));
            return draw.X < density
                ? new Vector4(drop > 0f ? -(1f + lens) * Vector2.Transform(off, up) : Vector2.Zero,
                    lag * (1f - drop) * float.Sqrt(float.Max(0f, 1f - float.Pow(off.X / (0.5f * radius), 2f))), float.Max(drop, lag * trail))
                : Vector4.Zero;
        };
    }

    public static PixelPass.Frame Pass(Func<Vector2, Vector4> surface, ShortSideLength scale, float strength, ShortSideLength smoothness, Option<ShortSideLength> blur, Sampling sampling, WrapMode wrap) =>
        new((frame, progress) => {
            (PixelExtent extent, PixelFrame plane, PixelFrame refracted, float reach) = (frame.Extent, new PixelFrame(frame.Origin, frame.Size, frame.Extent), frame.Copy(), frame.Extent.ShortSide * (float)scale);
            Vector2 center = extent.Point(new Vector2(0.5f));
            plane.Rows((row, x, y) => {
                for (int i = 0; i < row.Length; i++) row[i] = new Vector4(reach, reach, 1f, 1f) * surface((new Vector2(x + i + 0.5f, y + 0.5f) - center) / reach);
            });
            Fin<Unit> bent = plane.Filter(header => {
                using Mat level = new();
                using Mat slope = new();
                CvInvoke.ExtractChannel(header, level, 2);
                level.Blur(level, smoothness.Pixels(extent));
                MapReading.Slope(level, slope, strength * reach * extent.ShortSide / 8d);
                Span2D<Vector4> drops = header.View<Vector4>();
                ReadOnlySpan2D<Vector2> bend = slope.View<Vector2>();
                for (int y = 0; y < drops.Height; y++)
                    for (int x = 0; x < drops.Width; x++)
                        drops[y, x] -= new Vector4(bend[y, x], 0f, 0f);
                return unit;
            });
            Fin<PixelFrame> composed =
                from slope in bent
                from blurred in refracted.Filter(header => blur.Iter(length => header.Blur(header, length.Pixels(extent))))
                from applied in new CoordinateMap.Offsets(plane, sampling, (wrap, wrap)).Apply(refracted, progress)
                select BlendingMode.Normal.Composite(frame, refracted, 1f, Seq<Mask>(), PixelAlphaRepresentation.Unassociated);
            return composed.Map(composite => {
                composite.View.Span.CopyTo(frame.View.Span);
                return unit;
            });
        });

    private static Func<Vector2, Vector4> Glass(Func<Vector2, float> height) => cell => new Vector4(Vector2.Zero, height(cell), 1f);

    private static Vector2 Fraction(Vector2 cell) => cell - Vector2.Round(cell, MidpointRounding.ToNegativeInfinity);
}
