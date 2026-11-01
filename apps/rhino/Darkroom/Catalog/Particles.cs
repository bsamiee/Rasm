using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance.Buffers;
using Darkroom.Chain;
using MathNet.Numerics.Integration;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PointCells(ShortSideLength Spacing, float Density, float Size, float Variation, float Spread, float Softness) {
    private const float Randomness = 0.5f;
    private const float Largest = (1f - Randomness) / 2f;

    public static PointCells Default { get; } = new(ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide), 0.5f, 2f * 0.2f, 0.5f, 1f, 0.5f);

    public bool Empty => Size is 0f || Density is 0f;

    public (double Radius, double Gain) Draw(Vector4 draw) => (Size * (1d - (Variation * (double)draw.Y)) * Largest, double.Exp2(-Spread * draw.Z));

    public (float Coverage, float Value) Disc((double X, double Y) point, double side, uint field, Func<float, float> tone) {
        (double spacing, uint sites, uint colors) = ((float)Spacing, CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        double width = 1d / (spacing * side);
        (double x, double y, double half) = (point.X / spacing, point.Y / spacing, width / 2d);
        (int left, int top) = ((int)double.Floor(x - half - Randomness - Largest), (int)double.Floor(y - half - Randomness - Largest));
        (int right, int bottom) = ((int)double.Floor(x + half + Largest), (int)double.Floor(y + half + Largest));
        (double coverage, double value) = (0d, 0d);
        for (int line = top; !Empty && line <= bottom; line++) {
            for (int column = left; column <= right; column++) {
                Vector128<int> cell = Vector128.Create(column, line, 0, 0);
                Vector4 draw = CoordinateHash.Uniforms(cell, colors);
                (double radius, double gain) = Draw(draw);
                Vector2 site = new Vector2(column, line) + (CoordinateHash.Uniforms(cell, sites).AsVector2() * Randomness);
                double weight = draw.X < Density && radius > 0d
                    ? Iris.Circular.Mass(radius, Softness * radius, new RectangleF((float)(x - half - site.X), (float)(y - half - site.Y), (float)width, (float)width)) * gain / (width * width)
                    : 0d;
                (coverage, value) = (coverage + weight, value + (weight * tone(draw.X / Density)));
            }
        }
        return ((float)coverage, (float)value);
    }
}

public sealed record ParticleField(
    PointCells Cells, int Layers, Angle Heading, float Speed, NoiseBasis Basis, ShortSideLength Eddy, float Evolution, float Depth, float DepthVariation, float Focus, Iris Aperture,
    float Fringe, Duration Shutter, float Fade, bool Covering, Seq<RampStop> Coating, Seq<RampStop> Colors, Blackbody Emission, float Incandescence,
    float Twinkle, float TwinkleRate, GeneratedLayer Layer, Seed Seed, Timing Timing) : IPixelStage<ParticleField> {
    private const int Band = 16;
    private static readonly GaussLegendreRule Shutters = new(0d, 1d, 32);

    public Option<ShortSideLength> Turbulence { get; init; }
    public Option<ShortSideLength> Defocus { get; init; }
    public Option<float> Radiance { get; init; }

    public static ParticleField Default { get; } = new(
        new(ShortSideLength.Create(ReferenceFrame.GreaterSide / 2f / 10f), 1f, 2f * 0.2f, 0.5f, 1f, 1f / 1.8f), 4, Angle.FromRadians(double.Atan2(0.5d, 0.4d)),
        24f * 0.05f * 0.5f * float.Hypot(0.4f, 0.5f) * (ReferenceFrame.GreaterSide / 2f), NoiseBasis.Perlin.Default, ShortSideLength.Create(ReferenceFrame.GreaterSide / 2f / 1.6f),
        1.6f * (24f * 0.05f * 0.5f) / 3200f, 4f, 1f, 1f, Iris.Create(6, double.Pi / 3d, 0d, 0d, 1d), 0f, Duration.FromSeconds(0.5d / 24d), 0f, Covering: false, Presets.White,
        Presets.White, Blackbody.Default, 0f, 0f, 0f, GeneratedLayer.Added with { Exposure = 2f }, Seed.MinValue, Timing.Default) {
        Turbulence = ShortSideLength.Create(0.12f / 2f * (ReferenceFrame.GreaterSide / 2f)),
        Defocus = ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide),
        Radiance = 0f,
    };

    public static ParticleField Rain { get; } = Default with {
        Cells = PointCells.Default,
        Heading = Angle.FromDegrees(-90d),
        Speed = 2.5f * ReferenceFrame.GreaterSide,
        Turbulence = None,
        Aperture = Iris.Create(6, double.Pi / 3d, 1d, 0d, 1d),
        Layer = GeneratedLayer.Default,
        Covering = true,
        Radiance = None,
    };

    public static ParticleField Snow { get; } = Rain with {
        Cells = new(ShortSideLength.Create(0.03f * ReferenceFrame.GreaterSide), 0.5f, 2f * 0.3f, 0.6f, 1.3f, 0.6f),
        Speed = 0.3f * ReferenceFrame.GreaterSide,
        Turbulence = ShortSideLength.Create(0.02f / 2f * ReferenceFrame.GreaterSide),
        Colors = Presets.Solid(new Vector3(0.8f)),
        Coating = Presets.Solid(new Vector3(0.8f)),
    };

    public static ParticleField Sparks { get; } = Default with {
        Cells = Default.Cells with { Density = 0.3f, Size = 2f * 0.1f, Variation = 0.8f, Spread = 2.3f, Softness = 0.3f },
        Heading = Angle.FromDegrees(90d),
        Speed = 0.4f * ReferenceFrame.GreaterSide,
        Turbulence = ShortSideLength.Create(0.03f / 2f * ReferenceFrame.GreaterSide),
        Emission = Blackbody.Default with { Cool = 1000d },
        Incandescence = 1f,
    };

    public static ParticleField CellRain { get; } = Default with {
        Covering = true,
        Radiance = None,
        Cells = Default.Cells with { Variation = 0f, Spread = 0f, Softness = 0f },
        Heading = Angle.FromDegrees(-90d),
        Defocus = None,
        Aperture = Iris.Create(4, double.Pi / 4d, 0d, 0d, 1d),
    };

    public static ParticleField Starfield { get; } = Rain with {
        Cells = new(ShortSideLength.Create(0.005f * ReferenceFrame.GreaterSide), 0.12f, 2f * 0.1f, 0.5f, 4f, 1f),
        Layers = 1,
        Depth = 1f,
        Speed = 0f,
        Defocus = None,
        Emission = new(3000d, 12_000d),
        Incandescence = 1f,
        Layer = GeneratedLayer.Added,
        Covering = false,
        Radiance = 0f,
    };

    public static Seq<(string Name, Func<ParticleField, ParticleField> Look)> Looks { get; } = [
        (nameof(Rain), static _ => Rain), (nameof(Snow), static _ => Snow), (nameof(Sparks), static _ => Sparks), (nameof(CellRain), static _ => CellRain), (nameof(Starfield), static _ => Starfield)];

    public static Fin<Option<PixelPass>> Pass(ParticleField state, PassContext context) =>
        state.Cells.Empty || state.Aperture.Obstruction is 1d || (!state.Covering && state.Radiance.IsNone) ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Layer.Run(
                (state.Basis.Sampler, state.Turbulence.IsNone ? (Lower: 0f, Upper: 0f) : state.Basis.Range, (NoiseBasis.Perlin.Plain with { Dimensions = NoiseDimensions.Four }).Sampler)
                    .Apply((wind, range, plain) => Swarm(state, context, frame, wind, range, plain)).As(), frame, progress)));

    private static Action<Span<Vector4>, int, int> Swarm(
        ParticleField state, PassContext context, PixelFrame frame, Func<Vector4, uint, (float Value, Vector4 Site)> wind, (float Lower, float Upper) range,
        Func<Vector4, uint, (float Value, Vector4 Site)> plain) {
        uint field = state.Seed.Field(15u, 0u);
        (uint presence, uint tone, uint swayX, uint swayY, uint glint) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (RampLight colors, RampLight coating, RampLight thermal) =
            (RampLight.Of(state.Colors, context), RampLight.Of(state.Coating, context), context.Derivations.Derived((context.Working, state.Emission), static key => key.Emission.Glow(key.Working)));
        (PointCells cells, Iris iris, Vector2 middle, float side) = (state.Cells, state.Aperture, context.Extent.Point(new Vector2(0.5f)), context.Extent.ShortSide);
        (double time, double shutter, float softness) = (state.Timing.At(context), state.Shutter.TotalSeconds, state.Cells.Softness);
        float turbulence = state.Turbulence.Share;
        float swing = turbulence * float.Max(float.Abs((2f * range.Lower) - 1f), float.Abs((2f * range.Upper) - 1f));
        (double defocus, double focus, double radiance) =
            (state.Defocus.Share, double.Pow(state.Depth, -(double)state.Focus), state.Radiance.Map(static exposure => (double)float.Exp2(exposure)).IfNone(0d));
        Vector2 heading = float.SinCos((float)state.Heading.Radians) switch { var (sin, cos) => new Vector2(state.Speed * cos, -state.Speed * sin) };
        Mote[] ordered = [.. Enumerable.Range(0, state.Layers).SelectMany(Layer)
            .OrderByDescending(static mote => mote.Depth).ThenBy(static mote => mote.Layer).ThenBy(static mote => mote.Point.Y).ThenBy(static mote => mote.Point.X)];
        int first = frame.Origin.Y;
        Arr<Arr<Mote>> bands = [.. Enumerable.Range(0, ((frame.Size.Height - 1) / Band) + 1).Select(band => (Low: first + (band * Band), High: first + ((band + 1) * Band)) switch {
            var rows => toArray(ordered.Where(mote => mote.Rows.High >= rows.Low && mote.Rows.Low < rows.High)),
        })];
        return (row, x, y) => {
            Arr<Mote> bin = bands[(y - first) / Band];
            for (int i = 0; i < row.Length; i++)
                row[i] = bin.IsEmpty ? Vector4.Zero : Integrated(bin, x + i, y);
        };

        Vector4 Integrated(Arr<Mote> bin, int column, int line) {
            Vector4 sum = Vector4.Zero;
            for (int sample = 0; sample < Shutters.Order; sample++) {
                double u = Shutters.GetAbscissa(sample);
                double weight = Shutters.GetWeight(sample) * (1d - state.Fade + (state.Fade * u)) / (1d - (state.Fade / 2d));
                sum += Spatial(bin, column, line, time - (shutter * (1d - u))) * (float)weight;
            }
            return sum;
        }

        IEnumerable<Mote> Layer(int layer) {
            double middleDepth = (layer + 0.5d) / state.Layers;
            double depthWidth = state.DepthVariation / 2d / state.Layers;
            (double reference, double nearest, double farthest) =
                (double.Pow(state.Depth, -middleDepth), double.Pow(state.Depth, -(middleDepth - depthWidth)), double.Pow(state.Depth, -(middleDepth + depthWidth)));
            double cell = (float)cells.Spacing * reference;
            double support = Grown(side * (((float)cells.Spacing * nearest * cells.Draw(Vector4.Zero).Radius) + (defocus * double.Max(double.Abs(nearest - focus), double.Abs(farthest - focus))))) / side;
            double[] gusts = [((2d * range.Lower) - 1d) * nearest, ((2d * range.Lower) - 1d) * farthest, ((2d * range.Upper) - 1d) * nearest, ((2d * range.Upper) - 1d) * farthest];
            (double windLow, double windHigh) = (gusts.Min() * turbulence, gusts.Max() * turbulence);
            ((double Low, double High) across, (double Low, double High) down) = (Travel(heading.X, nearest, farthest), Travel(heading.Y, nearest, farthest));
            (int left, int top, int right, int bottom) =
                (Lattice(frame.Window.Left, middle.X, across.High + windHigh + support, cell) - 1, Lattice(frame.Window.Top, middle.Y, down.High + windHigh + support, cell) - 1,
                    Lattice(frame.Window.Right, middle.X, across.Low + windLow - support, cell), Lattice(frame.Window.Bottom, middle.Y, down.Low + windLow - support, cell));
            return from line in Enumerable.Range(top, bottom - top + 1)
                   from column in Enumerable.Range(left, right - left + 1)
                   let point = new Vector4(column, line, layer, 0f)
                   let position = NoiseDimensions.Four.White(point, presence)
                   where position.X < cells.Density
                   select Placed(point, position);

            Mote Placed(Vector4 point, Vector4 position) {
                (Vector4 draw, double depth) = (NoiseDimensions.Four.White(point, tone), middleDepth + ((position.W - 0.5d) * state.DepthVariation / state.Layers));
                (double scale, (double radius, double gain)) = (double.Pow(state.Depth, -depth), cells.Draw(draw));
                double core = side * (float)cells.Spacing * scale * radius;
                double optical = core + (side * defocus * double.Abs(scale - focus));
                (Vector4 spectrum, Vector3 tint) = (colors.Sample(draw.X), Vector3.Lerp(Vector3.One, thermal.Sample(draw.X).AsVector3(), state.Incandescence));
                Vector4 area = iris.Integral(optical, softness * optical, state.Fringe);
                double energy = radiance * spectrum.W * gain * double.Pi * core * core;
                Vector2 rest = new((float)((point.X + (double)position.Y) * cell), (float)((point.Y + (double)position.Z) * cell));
                return new(depth, layer, point, rest, (float)scale, (float)optical, (float)gain, state.Covering ? coating.Sample(draw.X) : Vector4.Zero,
                    new Vector4(Lit(spectrum.X * tint.X, area.X, energy), Lit(spectrum.Y * tint.Y, area.Y, energy), Lit(spectrum.Z * tint.Z, area.Z, energy), 0f),
                    Rows(rest, (float)scale, (float)optical));
            }

            static float Lit(float channel, float covered, double energy) => covered is 0f ? 0f : (float)(channel * energy / covered);
        }

        int Lattice(int pixel, float center, double displacement, double size) => (int)double.Floor((((pixel - center) / side) - displacement) / size);

        (double Low, double High) Travel(double rate, double near, double far) {
            double[] endpoints = [rate * near * time, rate * far * time, rate * near * (time - shutter), rate * far * (time - shutter)];
            return (endpoints.Min(), endpoints.Max());
        }

        (int Low, int High) Rows(Vector2 rest, float scale, float radius) {
            (float early, float late) = (Carried(rest, scale, time - shutter).Y, Carried(rest, scale, time).Y);
            float slack = Grown(radius) + (swing * scale * side);
            return ((int)float.Floor(float.Min(early, late) - slack), (int)float.Ceiling(float.Max(early, late) + slack));
        }

        Vector2 Carried(Vector2 rest, float scale, double tau) => rest + (heading * (float)(scale * tau));

        Vector2 Moved(Mote mote, double tau) {
            Vector2 at = Carried(mote.Rest, mote.Scale, tau);
            if (turbulence is 0f)
                return middle + (at * side);
            Vector4 sample = new(at / (mote.Scale * (float)state.Eddy), (float)(state.Evolution * tau), 0f);
            return middle + ((at + (turbulence * mote.Scale * new Vector2((2f * wind(sample, swayX).Value) - 1f, (2f * wind(sample, swayY).Value) - 1f))) * side);
        }

        Vector4 Spatial(Arr<Mote> bin, int column, int line, double tau) {
            (float left, float top, float right, float bottom) = (column, line, column + 1f, line + 1f);
            (Mote Mote, Vector2 Point, float LowX, float HighX, float LowY, float HighY, float Modulation)[] active = [..
                from mote in bin
                let point = Moved(mote, tau)
                let reach = Grown(mote.Radius)
                let lowX = float.Max(left, point.X - reach)
                let highX = float.Min(right, point.X + reach)
                let lowY = float.Max(top, point.Y - reach)
                let highY = float.Min(bottom, point.Y + reach)
                where lowX < highX && lowY < highY
                select (mote, point, lowX, highX, lowY, highY, 1f - (state.Twinkle * plain(mote.Point with { W = (float)(state.TwinkleRate * tau) }, glint).Value))];
            if (active.Length is 0)
                return Vector4.Zero;
            int count = (2 * active.Length) + 2;
            using SpanOwner<float> edges = SpanOwner<float>.Allocate(2 * count);
            Span<float> xs = edges.Span[..count];
            Span<float> ys = edges.Span[count..];
            for (int i = 0; i < active.Length; i++)
                (xs[2 * i], xs[(2 * i) + 1], ys[2 * i], ys[(2 * i) + 1]) = (active[i].LowX, active[i].HighX, active[i].LowY, active[i].HighY);
            (xs[^2], xs[^1], ys[^2], ys[^1]) = (left, right, top, bottom);
            xs.Sort();
            ys.Sort();
            Vector4 total = Vector4.Zero;
            for (int y = 0; y < count - 1; y++) {
                for (int x = 0; x < count - 1; x++)
                    total += xs[x + 1] > xs[x] && ys[y + 1] > ys[y] ? Cell(new RectangleF(xs[x], ys[y], xs[x + 1] - xs[x], ys[y + 1] - ys[y])) : Vector4.Zero;
            }
            return total;

            Vector4 Cell(RectangleF area) {
                Vector4 sum = Vector4.Zero;
                for (int down = 0; down < Shutters.Order; down++) {
                    for (int across = 0; across < Shutters.Order; across++) {
                        Vector2 at = new(area.X + (area.Width * (float)Shutters.GetAbscissa(across)), area.Y + (area.Height * (float)Shutters.GetAbscissa(down)));
                        sum += Folded(at) * (float)(area.Width * area.Height * Shutters.GetWeight(across) * Shutters.GetWeight(down));
                    }
                }
                return sum;
            }

            Vector4 Folded(Vector2 at) {
                Vector4 folded = Vector4.Zero;
                foreach ((Mote mote, Vector2 point, _, _, _, _, float modulation) in active) {
                    (double reach, double edge) = iris.Outline(at - point);
                    Vector4 profile = iris.Weights(reach, edge, mote.Radius, softness * mote.Radius, state.Fringe);
                    float opacity = profile.W * mote.Gain * mote.Coating.W;
                    folded = ((1f - opacity) * folded) + (opacity * new Vector4(mote.Coating.AsVector3(), 1f)) + (profile * mote.Light * modulation);
                }
                return folded;
            }
        }

        float Grown(double radius) => (float)((iris.Circumradius * radius) + (softness * radius / 2d));
    }

    private sealed record Mote(double Depth, int Layer, Vector4 Point, Vector2 Rest, float Scale, float Radius, float Gain, Vector4 Coating, Vector4 Light, (int Low, int High) Rows);
}
