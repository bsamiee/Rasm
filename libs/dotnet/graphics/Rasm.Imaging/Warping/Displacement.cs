using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Rasm.Imaging.Color;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Warping;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class BulgeMode {
    public static readonly BulgeMode Round = new("round", Vector2.One);
    public static readonly BulgeMode Horizontal = new("horizontal", Vector2.UnitX);
    public static readonly BulgeMode Vertical = new("vertical", Vector2.UnitY);

    public Vector2 Axes { get; }
}

[SmartEnum<string>]
public sealed partial class WaveFront {
    public static readonly WaveFront Linear = new("linear", None);
    public static readonly WaveFront Radial = new("radial", Some(Vector2.UnitX));
    public static readonly WaveFront Zigzag = new("zigzag", Some(Vector2.UnitY));
    public static readonly WaveFront Pond = new("pond", Some(new Vector2(float.Sqrt(0.5f))));

    public Option<Vector2> Ring { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Displacement {
    private Displacement(FramePosition center) => Center = center;

    public FramePosition Center { get; }

    public CoordinateMap.Analytic Map(PixelExtent extent, Sampling sampling, (WrapMode Across, WrapMode Down) wrap) {
        ((double X, double Y) origin, Func<(double X, double Y), (double X, double Y)> field) = (Center.Point(extent), Offset(extent));
        return new((points, _, _) => {
            foreach (ref (double X, double Y) point in points) point = field((point.X - origin.X, point.Y - origin.Y)) switch { var at => (origin.X + at.X, origin.Y + at.Y) };
        }, sampling, wrap);
    }

    private protected abstract Func<(double X, double Y), (double X, double Y)> Offset(PixelExtent extent);

    public sealed record Twirl(double Angle, ShortSideLength Radius, double Falloff, int Arms, double ArmOffset, FramePosition Center) : Displacement(Center) {
        private protected override Func<(double X, double Y), (double X, double Y)> Offset(PixelExtent extent) =>
            (Radius.Pixels(extent), 1d / Falloff) switch {
                var (radius, exponent) => d => double.SinCos(Angle * ShapeEdge.Smooth(double.Clamp(1d - double.Pow(double.Hypot(d.X, d.Y) / radius, exponent), 0d, 1d))
                    * (0.5d + (0.5d * double.Cos(Arms * (double.Atan2(-d.Y, d.X) - ArmOffset))))) switch {
                        var (sin, cos) => ((d.X * cos) - (d.Y * sin), (d.X * sin) + (d.Y * cos)),
                    },
            };
    }

    public sealed record Bulge(double Amount, BulgeMode Mode, ShortSideLength Radius, FramePosition Center) : Displacement(Center) {
        private protected override Func<(double X, double Y), (double X, double Y)> Offset(PixelExtent extent) =>
            Radius.Pixels(extent) switch {
                var radius => d => (X: d.X * Mode.Axes.X, Y: d.Y * Mode.Axes.Y) switch {
                    var v when double.Hypot(v.X, v.Y) / radius is var u and > 0d and < 1d =>
                        (((Amount >= 0d ? double.Lerp(u, 2d / double.Pi * double.Asin(u), Amount) : double.Lerp(u, double.SinPi(u / 2d), -Amount)) / u) - 1d) switch {
                            var scale => (d.X + (scale * v.X), d.Y + (scale * v.Y)),
                        },
                    _ => d,
                },
            };
    }

    public sealed record Wave(
        WaveFront Front, WaveProfile Profile, int Count, (ShortSideExtent Low, ShortSideExtent High) Wavelength, (float Low, float High) Amplitude,
        (float X, float Y) Amount, float Direction, float Spread, float Shift, ShortSideLength Radius, FramePosition Center, uint Field) : Displacement(Center) {
        private protected override Func<(double X, double Y), (double X, double Y)> Offset(PixelExtent extent) {
            ((double X, double Y) amounts, double radius, Matrix3x2 across) = (((double)Amount.X * extent.ShortSide, (double)Amount.Y * extent.ShortSide), Radius.Pixels(extent), Matrix3x2.CreateRotation(-float.Pi / 2f));
            (bool ring, Matrix3x2 turn) = Front.Ring.Match(Some: weights => (true, (Matrix3x2.Identity * weights.X) + (across * weights.Y)), None: () => (false, across));
            (Vector2 Along, double Length, float Amplitude, float Phase)[] waves = [.. Enumerable.Range(0, Count).Select(i =>
                (CoordinateHash.White(Vector256<double>.Zero, CoordinateHash.Branch(Field, (uint)i)), Direction + (Spread * (i - ((Count - 1) / 2f)) / int.Max(Count - 1, 1))) switch {
                    var (draw, angle) => (Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(-angle)), double.Lerp(Wavelength.Low.Pixels(extent), Wavelength.High.Pixels(extent), draw.X),
                        float.Lerp(Amplitude.Low, Amplitude.High, draw.Y), draw.Z + Shift),
                })];
            return d => {
                double reach = double.Hypot(d.X, d.Y);
                ((double X, double Y) spoke, (double X, double Y) sum) = (reach > 0d ? (d.X / reach, d.Y / reach) : (1d, 0d), (0d, 0d));
                foreach ((Vector2 along, double length, float amplitude, float phase) in waves) {
                    (double x, double y) = Crest(d, ring ? spoke : (along.X, along.Y), length, amplitude, phase);
                    sum = (sum.X + x, sum.Y + y);
                }
                double falloff = (ring ? double.Max(0d, 1d - (reach / radius)) : 1d) / Count;
                return (d.X + (amounts.X * sum.X * falloff), d.Y + (amounts.Y * sum.Y * falloff));
            };

            (double X, double Y) Crest((double X, double Y) d, (double X, double Y) axis, double length, float amplitude, float phase) =>
                (amplitude * ((2f * Profile.Level((float)((((d.X * axis.X) + (d.Y * axis.Y)) / length) + phase))) - 1f)) switch {
                    var level => (level * ((axis.X * turn.M11) + (axis.Y * turn.M21)), level * ((axis.X * turn.M12) + (axis.Y * turn.M22))),
                };
        }
    }

    public sealed record Turbulence(Func<Vector256<double>, uint, float> Noise, (ShortSideExtent X, ShortSideExtent Y) Scale, (float X, float Y) Amount, double Depth, uint Field)
        : Displacement(FramePosition.Center) {
        private protected override Func<(double X, double Y), (double X, double Y)> Offset(PixelExtent extent) {
            ((double X, double Y) scale, (double X, double Y) amounts) =
                ((Scale.X.Pixels(extent), -Scale.Y.Pixels(extent)), ((double)Amount.X * extent.ShortSide, (double)Amount.Y * extent.ShortSide));
            (uint right, uint left, uint down, uint up) = (CoordinateHash.Branch(Field, 0u), CoordinateHash.Branch(Field, 1u), CoordinateHash.Branch(Field, 2u), CoordinateHash.Branch(Field, 3u));
            return d => Vector256.Create(d.X / scale.X, d.Y / scale.Y, Depth, 0d) switch {
                var at => (d.X + (amounts.X * (Noise(at, right) - Noise(at, left))), d.Y + (amounts.Y * (Noise(at, down) - Noise(at, up)))),
            };
        }
    }
}

[SmartEnum<string>]
public sealed partial class MapReading {
    public static readonly MapReading Gradient = new("gradient", static (map, plane, sigma, luminance, exposure) =>
        Height(map, sigma, luminance, exposure, height => { HeightPlanes.Slope(height, plane, -double.Sqrt(double.Tau) * sigma / 8d); return unit; }));
    public static readonly MapReading Luma = new("luma", static (map, plane, sigma, luminance, exposure) =>
        Height(map, sigma, luminance, exposure, height => { HeightPlanes.Weighted(height, plane, 2, [2d, -1d, 2d, -1d]); return unit; }));
    public static readonly MapReading Offsets = new("offsets", static (map, plane, _, _, _) => { HeightPlanes.Weighted(map, plane, 2, [2d, 0d, 0d, 0d, -1d, 0d, -2d, 0d, 0d, 1d]); return unit; });

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Plane(Mat map, Mat plane, double sigma, Vector256<double> luminance, double exposure);

    private static Fin<Unit> Height(Mat map, double sigma, Vector256<double> luminance, double exposure, Func<Mat, Fin<Unit>> read) {
        double logGray = double.Log(Exposure.MiddleGray) - (exposure * double.Log(2d));
        using Mat linear = new();
        using Mat height = new();
        using Mat plane = new();
        map.ConvertTo(linear, DepthType.Cv64F);
        HeightPlanes.Weighted(linear, height, 1, [luminance[0], luminance[1], luminance[2], 0d]);
        foreach (ref double luma in height.GetSpan<double>())
            luma = double.Log(double.Max(luma, 0d)) switch {
                var logLuma => double.Exp(logLuma - double.Max(logLuma, logGray) - double.LogP1(double.Exp(-double.Abs(logLuma - logGray)))),
            };
        height.ConvertTo(plane, DepthType.Cv32F);
        return plane.Blur(plane, 3d * sigma).Bind(_ => read(plane));
    }
}

public sealed record DisplacementMap(MapReading Reading, Option<ShortSideExtent> Tile, ShortSideExtent Softness, (float X, float Y) Amount, double Rotation) {
    public PixelPass.Frame Pass(Option<PixelFrame> map, Gamut gamut, double exposure, Sampling sampling, WrapMode wrap) =>
        new((frame, progress) => Kernel(map.Match(Some: image => Fitted(image, frame), None: frame), gamut, exposure, sampling, wrap, frame, progress));

    private Fin<Unit> Kernel(PixelFrame source, Gamut gamut, double exposure, Sampling sampling, WrapMode wrap, PixelFrame frame, IProgress<int> progress) =>
        source.Filter(header => {
            using Mat offsets = new();
            ((double X, double Y) amount, (double Sin, double Cos) turn, Point corner) =
                (((double)Amount.X * frame.Extent.ShortSide, (double)Amount.Y * frame.Extent.ShortSide), double.SinCos(-Rotation), frame.Origin);
            return Reading.Plane(header, offsets, Softness.Pixels(frame.Extent), gamut.Luminance, exposure).Bind(_ => new CoordinateMap.Analytic((points, _, _) => {
                ReadOnlySpan2D<Vector2> read = offsets.View<Vector2>();
                foreach (ref (double X, double Y) point in points) {
                    Vector2 offset = read[(int)double.Floor(point.Y - corner.Y), (int)double.Floor(point.X - corner.X)];
                    (double x, double y) = (offset.X * amount.X, offset.Y * amount.Y);
                    point = (point.X - (x * turn.Cos) + (y * turn.Sin), point.Y - (x * turn.Sin) - (y * turn.Cos));
                }
            }, sampling, (wrap, wrap)).Apply(frame, progress));
        });

    private PixelFrame Fitted(PixelFrame image, PixelFrame frame) =>
        Tile.Match(
            Some: tile => (image.Extent.Height / tile.Pixels(frame.Extent)) switch { var scale => ((X: scale, Y: scale), WrapMode.Periodic) },
            None: () => ((X: (double)image.Extent.Width / frame.Extent.Width, Y: (double)image.Extent.Height / frame.Extent.Height), WrapMode.Clamp)) switch {
                var (scale, wrap) => new CoordinateMap.Analytic((points, _, _) => {
                    foreach (ref (double X, double Y) point in points) point = ((point.X + frame.Origin.X) * scale.X, (point.Y + frame.Origin.Y) * scale.Y);
                }, Sampling.Linear, (wrap, wrap)).Resample(image, frame.Size),
            };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RefractionSurface {
    internal abstract Func<Vector2, Vector4> Sampler(float scale, uint field);

    private static Func<Vector2, Vector4> Glass(float scale, Func<Vector2, float> height) => q => new Vector4(Vector2.Zero, height(q / scale), 1f);

    private static Vector2 Fraction(Vector2 cell) => cell - Vector2.Round(cell, MidpointRounding.ToNegativeInfinity);

    public sealed record Raindrops(float Density, float Size, float Variation, float Softness, float Lens, float Trails, float Direction, float Fall) : RefractionSurface {
        internal override Func<Vector2, Vector4> Sampler(float scale, uint field) {
            (Matrix3x2 fall, Matrix3x2 rise) = (Matrix3x2.CreateRotation(Direction + (float.Pi / 2f)), Matrix3x2.CreateRotation(-Direction - (float.Pi / 2f)));
            return q => {
                Vector2 at = (Vector2.Transform(q, fall) - new Vector2(0f, Fall)) / scale;
                Vector2 cell = Vector2.Round(at, MidpointRounding.ToNegativeInfinity);
                Vector4 draw = CoordinateHash.White(Vector256.Create(cell.X, cell.Y, 0d, 0d), field);
                Vector2 off = at - cell - new Vector2(0.25f) - (0.5f * new Vector2(draw.Y, draw.Z));
                (float radius, float lag) = (0.5f * Size * (1f - (Variation * draw.W)), off.Y <= 0f ? Trails * (1f + (off.Y / (0.25f + (0.5f * draw.Z)))) : 0f);
                float drop = ShapeEdge.Smooth(float.Clamp((radius - off.Length()) / (radius * Softness), 0f, 1f));
                float trail = ShapeEdge.Smooth(float.Clamp(((0.5f * radius) - float.Abs(off.X)) / (0.5f * radius * Softness), 0f, 1f));
                return draw.X < Density
                    ? new Vector4(drop > 0f ? -(1f + Lens) * scale * Vector2.Transform(off, rise) : Vector2.Zero,
                        lag * (1f - drop) * float.Sqrt(float.Max(0f, 1f - float.Pow(off.X / (0.5f * radius), 2f))), float.Max(drop, lag * trail))
                    : Vector4.Zero;
            };
        }
    }

    public sealed record Blocks : RefractionSurface {
        internal override Func<Vector2, Vector4> Sampler(float scale, uint field) => Glass(scale, static cell =>
            Vector2.Abs((2f * Fraction(cell)) - Vector2.One) switch { var edge => (Vector2.One - (edge * edge * edge * edge)) switch { var bevel => bevel.X * bevel.Y } });
    }

    public sealed record Canvas : RefractionSurface {
        internal override Func<Vector2, Vector4> Sampler(float scale, uint field) => Glass(scale, static cell => 0.5f + (0.5f * float.SinPi(2f * cell.X) * float.SinPi(2f * cell.Y)));
    }

    public sealed record Frosted(Octaves Octaves) : RefractionSurface {
        internal override Func<Vector2, Vector4> Sampler(float scale, uint field) =>
            Glass(scale, cell => 0.5f + (0.5f * NoiseDimensions.Two.Fbm(Vector256.Create(cell.X, cell.Y, 0d, 0d), Octaves, CoordinateHash.Branch(field, 2u))));
    }

    public sealed record TinyLens : RefractionSurface {
        internal override Func<Vector2, Vector4> Sampler(float scale, uint field) =>
            Glass(scale, static cell => float.Sqrt(float.Max(0f, 1f - (4f * (Fraction(cell) - new Vector2(0.5f)).LengthSquared()))));
    }
}

public sealed record RefractionField(RefractionSurface Surface, ShortSideExtent Scale, float Strength, ShortSideExtent Smoothness, ShortSideLength Blur, uint Field) {
    public PixelPass.Frame Pass(Sampling sampling, WrapMode wrap, Gamut gamut) =>
        new((frame, progress) => Kernel(Surface.Sampler(Scale, Field), sampling, wrap, gamut, frame, progress));

    private Fin<Unit> Kernel(Func<Vector2, Vector4> sample, Sampling sampling, WrapMode wrap, Gamut gamut, PixelFrame frame, IProgress<int> progress) {
        (PixelExtent extent, (double X, double Y) half) = (frame.Extent, FramePosition.Center.Point(frame.Extent));
        (PixelFrame surface, PixelFrame refracted) = (new(frame.Origin, frame.Size, frame.Extent), frame.Copy());
        return from painted in new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) row[x] = new Vector4(extent.ShortSide, extent.ShortSide, 1f, 1f) * sample(new Vector2(
                (float)(((double)column + x + 0.5d - half.X) / extent.ShortSide), (float)(((double)extent.Height - line - 0.5d - half.Y) / extent.ShortSide)));
        }).Run(surface, new Progress<int>())
               from displaced in surface.Filter(drawn => {
                   using Mat level = new();
                   using Mat slope = new();
                   using Mat source = refracted.Header();
                   CvInvoke.ExtractChannel(drawn, level, 2);
                   return from prepared in (level.Blur(level, 3d * Smoothness.Pixels(extent)).Map(_ => { HeightPlanes.Slope(level, slope, (double)Strength * extent.ShortSide * Scale.Pixels(extent) / 8d); return unit; }),
                              source.Blur(source, 3d * Blur.Pixels(extent))).Apply(static (_, _) => unit).As()
                          from applied in new CoordinateMap.Analytic((points, coverage, _) => {
                              ReadOnlySpan2D<Vector2> bend = slope.View<Vector2>();
                              ReadOnlySpan2D<Vector4> drops = drawn.View<Vector4>();
                              for (int i = 0; i < points.Length; i++) {
                                  (int y, int x) = ((int)double.Floor(points[i].Y - surface.Origin.Y), (int)double.Floor(points[i].X - surface.Origin.X));
                                  (points[i], coverage[i]) = ((points[i].X + drops[y, x].X - bend[y, x].X, points[i].Y + drops[y, x].Y - bend[y, x].Y), coverage[i] * drops[y, x].W);
                              }
                          }, sampling, (wrap, wrap)).Apply(refracted, progress)
                          select applied;
               })
               from composed in new PixelPass.Pointwise((row, _, line) => {
                   using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                   weights.Span.Fill(1f);
                   BlendingMode.Mix.Composite(row, refracted.Row(line), weights.Span, PixelAlphaRepresentation.Unassociated, gamut);
               }).Run(frame, new Progress<int>())
               select composed;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class HeightPlanes {
    public static void Slope(Mat level, Mat plane, double scale) {
        using Mat across = new();
        using Mat down = new();
        CvInvoke.Sobel(level, across, DepthType.Cv32F, 1, 0, 3, scale, 0d, BorderType.Replicate);
        CvInvoke.Sobel(level, down, DepthType.Cv32F, 0, 1, 3, scale, 0d, BorderType.Replicate);
        using VectorOfMat pair = new(across, down);
        CvInvoke.Merge(pair, plane);
    }

    public static void Weighted(Mat source, Mat target, int rows, double[] weights) {
        using Mat matrix = new(rows, weights.Length / rows, DepthType.Cv64F, 1);
        matrix.SetTo(weights);
        CvInvoke.Transform(source, target, matrix);
    }
}
