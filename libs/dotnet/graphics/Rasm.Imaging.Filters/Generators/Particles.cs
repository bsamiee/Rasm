using System.Numerics;
using System.Runtime.Intrinsics;
using MathNet.Numerics.Integration;
using PeterO.Numbers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Filters.Optics;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filters.Generators;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PointCells(ShortSideExtent Spacing, AxisFraction Density, AxisFraction Size, AxisFraction Variation, Magnitude Spread, AxisFraction Softness)
    : IStateRecord<PointCells, PointCellsParameter> {
    private static readonly Cellular Sites = Cellular.Default with { Randomness = AxisFraction.Half };
    private static readonly float Largest = (1f - Sites.Randomness) / 2f;
    internal static GaussLegendreRule Quadrature { get; } = new(0d, 1d, 32);

    public static PointCells Default { get; } = new(
        ShortSideExtent.Create(0.01f * ReferenceFrame.GreaterSide), AxisFraction.Half, AxisFraction.Create(2f * 0.2f), AxisFraction.Half, Magnitude.Unit, AxisFraction.Half);

    internal (double Radius, double Gain) Draw(Vector4 draw) => (Size * (1d - (Variation * (double)draw.Y)) * Largest, double.Exp2(-(double)Spread * draw.Z));

    internal (float Coverage, float Value) Disc((ERational X, ERational Y) point, double side, uint field, Func<float, float> tone) {
        if (Size == AxisFraction.MinValue || Density == AxisFraction.MinValue)
            return (0f, 0f);
        ERational spacing = ERational.FromSingle(Spacing);
        ERational width = ERational.One.Divide(spacing.Multiply(ERational.FromDouble(side)));
        ERational half = width.Divide(2);
        (ERational X, ERational Y) center = (point.X.Divide(spacing), point.Y.Divide(spacing));
        (ERational left, ERational top, ERational right, ERational bottom) = (center.X.Subtract(half), center.Y.Subtract(half), center.X.Add(half), center.Y.Add(half));
        (ERational reach, ERational jitter) = (ERational.FromSingle(Largest), ERational.FromSingle(Sites.Randomness));
        (EInteger firstX, EInteger firstY) = (NoiseFunctions.Split(left.Subtract(jitter).Subtract(reach)).Cell, NoiseFunctions.Split(top.Subtract(jitter).Subtract(reach)).Cell);
        (EInteger columns, EInteger rows) = (NoiseFunctions.Split(right.Add(reach)).Cell.Subtract(firstX).Add(1), NoiseFunctions.Split(bottom.Add(reach)).Cell.Subtract(firstY).Add(1));
        (uint sites, uint colors) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        (ERational area, ERational coverage, ERational value) = (width.Multiply(width), ERational.Zero, ERational.Zero);
        (double softness, EInteger count) = (Softness, columns.Multiply(rows));
        for (EInteger index = EInteger.Zero; index.CompareTo(count) < 0; index = index.Add(1)) {
            (EInteger X, EInteger Y) = (firstX.Add(index.Remainder(columns)), firstY.Add(index.Divide(columns)));
            Vector4 draw = CoordinateHash.Uniforms(X, Y, colors);
            (double radiusDraw, double gain) = Draw(draw);
            if (draw.X >= Density || radiusDraw is 0d) continue;
            Vector2 offset = NoiseFunctions.SiteOffset(X, Y, Sites.Randomness, sites);
            (ERational siteX, ERational siteY) = (ERational.FromEInteger(X).Add(ERational.FromSingle(offset.X)), ERational.FromEInteger(Y).Add(ERational.FromSingle(offset.Y)));
            ERational radius = ERational.FromDouble(radiusDraw);
            (ERational lowX, ERational lowY, ERational highX, ERational highY) =
                (ERational.Max(radius.Negate(), left.Subtract(siteX)), ERational.Max(radius.Negate(), top.Subtract(siteY)),
                    ERational.Min(radius, right.Subtract(siteX)), ERational.Min(radius, bottom.Subtract(siteY)));
            if (lowX.CompareTo(highX) >= 0 || lowY.CompareTo(highY) >= 0) continue;
            (ERational nearestX, ERational nearestY) = (ERational.Max(lowX, ERational.Min(ERational.Zero, highX)), ERational.Max(lowY, ERational.Min(ERational.Zero, highY)));
            ERational clearance = radius.Multiply(radius).Subtract(nearestX.Multiply(nearestX)).Subtract(nearestY.Multiply(nearestY));
            if (clearance.Sign <= 0) continue;
            ERational mass = lowX.CompareTo(radius.Negate()) is 0 && lowY.CompareTo(radius.Negate()) is 0 && highX.CompareTo(radius) is 0 && highY.CompareTo(radius) is 0
                ? ERational.FromDouble(double.Pi * radiusDraw * radiusDraw * (1d - softness + (0.3d * softness * softness)))
                : Partial(lowX, lowY, highX, highY, radius, nearestY, clearance);
            ERational weight = mass.Multiply(ERational.FromDouble(gain)).Divide(area);
            (coverage, value) = (coverage.Add(weight), value.Add(weight.Multiply(ERational.FromSingle(tone(draw.X / Density)))));
        }
        return (coverage.ToSingle(), value.ToSingle());

        ERational Partial(ERational lowX, ERational lowY, ERational highX, ERational highY, ERational radius, ERational nearestY, ERational clearance) {
            EContext math = EContext.Binary64.WithUnlimitedExponents().WithPrecision(width.Denominator.GetUnsignedBitLengthAsEInteger()
                .Add(clearance.Denominator.GetUnsignedBitLengthAsEInteger()).Add(EContext.Binary64.Precision).ToInt32Checked());
            EFloat r = radius.ToEFloat(math);
            EFloat squared = r * r;
            EFloat horizontal = radius.Multiply(radius).Subtract(nearestY.Multiply(nearestY)).ToEFloat(math).Sqrt(math);
            (EFloat first, EFloat last, EFloat band) = (EFloat.Max(lowX.ToEFloat(math), -horizontal), EFloat.Min(highX.ToEFloat(math), horizontal), EFloat.FromDouble(softness) * r);
            EFloat extent = last - first;
            EFloat sum = EFloat.Zero;
            for (int across = 0; across < Quadrature.Order; across++) {
                EFloat x = first + (extent * Quadrature.GetAbscissa(across));
                EFloat vertical = (squared - (x * x)).Sqrt(math);
                (EFloat bottom, EFloat top) = (EFloat.Max(lowY.ToEFloat(math), -vertical), EFloat.Min(highY.ToEFloat(math), vertical));
                EFloat height = top - bottom;
                for (int down = 0; down < Quadrature.Order; down++) {
                    EFloat y = bottom + (height * Quadrature.GetAbscissa(down));
                    EFloat fraction = band.IsZero ? EFloat.One : EFloat.Min(EFloat.One, (r - ((x * x) + (y * y)).Sqrt(math)).Divide(band, math));
                    sum += fraction * fraction * (3 - (2 * fraction)) * extent * height * Quadrature.GetWeight(across) * Quadrature.GetWeight(down);
                }
            }
            return ERational.FromEFloat(sum);
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointCellsParameter : IStateParameter<PointCells> {
    public static readonly PointCellsParameter Spacing = new("spacing", StateParameter.Bounded(Lens<PointCells, ShortSideExtent>.New(static cells => cells.Spacing, static spacing => cells => cells with { Spacing = spacing }), ShortSideExtent.Presentation with { Soft = (0.01f, 0.5f) }),
        new("Spacing", "Distance between point cells as a share of the frame's short side"));
    public static readonly PointCellsParameter Density = new("density", StateParameter.Bounded(Lens<PointCells, AxisFraction>.New(static cells => cells.Density, static density => cells => cells with { Density = density })),
        new("Density", "Share of cells holding a point"));
    public static readonly PointCellsParameter Size = new("size", StateParameter.Bounded(Lens<PointCells, AxisFraction>.New(static cells => cells.Size, static size => cells => cells with { Size = size })),
        new("Size", "Disc radius as a share of the largest radius fitting its cell"));
    public static readonly PointCellsParameter Variation = new("variation", StateParameter.Bounded(Lens<PointCells, AxisFraction>.New(static cells => cells.Variation, static variation => cells => cells with { Variation = variation })),
        new("Variation", "Share by which each point's draw reduces its radius"));
    public static readonly PointCellsParameter Spread = new("spread", StateParameter.Bounded(Lens<PointCells, Magnitude>.New(static cells => cells.Spread, static spread => cells => cells with { Spread = spread }), new Presentation<float> { Soft = (0f, 10f), Step = Exposure.Presentation.Step, Decimals = Exposure.Presentation.Decimals }),
        new("Brightness spread", "Stops by which a point's draw dims its brightness"));
    public static readonly PointCellsParameter Softness = new("softness", StateParameter.Bounded(Lens<PointCells, AxisFraction>.New(static cells => cells.Softness, static softness => cells => cells with { Softness = softness })),
        new("Softness", "Share of each disc's radius fading at its edge"));

    public StateParameter<PointCells> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ParticleField(
    PointCells Cells, PositiveCount Layers,
    SignedAngle Heading, Drift Speed, NoiseBasis Basis, ShortSideLength Turbulence, ShortSideExtent Eddy, Frequency Evolution,
    PrimaryScale Depth, AxisFraction DepthVariation, AxisFraction Focus, ShortSideLength Defocus, Iris Iris, SignedFraction Fringe, ShutterTime Shutter, AxisFraction Fade,
    bool Covering, Ramp Coating, Gated<Exposure> Radiance, Ramp Colors, Emission Emission, AxisFraction Incandescence, AxisFraction Twinkle, Frequency TwinkleRate, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<ParticleField, ParticleFieldParameter>, IPixelStage<ParticleField> {
    public static ParticleField Default { get; } = (Unit: ReferenceFrame.GreaterSide / 2f, Drift: 24f * 0.05f * 0.5f, Scale: 1.6f) switch {
        var (unit, drift, scale) => new(
            new(ShortSideExtent.Create(unit / 10f), AxisFraction.MaxValue, AxisFraction.Create(2f * 0.2f), AxisFraction.Half, Magnitude.Unit, AxisFraction.Create(1f / 1.8f)), PositiveCount.Create(4),
            SignedAngle.Create(float.Atan2(0.5f, 0.4f)), Drift.Create(drift * float.Hypot(0.4f, 0.5f) * unit), NoiseBasis.Default,
            ShortSideLength.Create(0.12f / 2f * unit), ShortSideExtent.Create(unit / scale), Frequency.Create(scale * drift / 3200f),
            PrimaryScale.Create(4f), AxisFraction.MaxValue, AxisFraction.MaxValue, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide), Iris.Default, SignedFraction.Neutral, ShutterTime.Create(0.5f / 24f), AxisFraction.MinValue,
            Covering: false, Ramp.White, new(Enabled: true, Exposure.Neutral),
            Ramp.Create(Seq(RampStop.White.At(RampPosition.MinValue)), RampInterpolation.Linear), Emission.Default, AxisFraction.MinValue, AxisFraction.MinValue, Frequency.Neutral,
            GeneratedLayer.Added with { Exposure = Exposure.Create(2f) }, Seed.MinValue, Timing.Default),
    };

    public static ParticleField Rain { get; } = Default with {
        Cells = PointCells.Default,
        Heading = SignedAngle.Down,
        Speed = Drift.Create(2.5f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Neutral,
        Iris = Iris.Default with { Roundness = AxisFraction.MaxValue },
        Layer = GeneratedLayer.Default,
        Covering = true,
        Radiance = new(Enabled: false, Exposure.Neutral),
    };

    public static ParticleField Snow { get; } = (Rain with {
        Cells = new(ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide), AxisFraction.Half, AxisFraction.Create(2f * 0.3f), AxisFraction.Create(0.6f), Magnitude.Create(1.3f), AxisFraction.Create(0.6f)),
        Speed = Drift.Create(0.3f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Create(0.02f / 2f * ReferenceFrame.GreaterSide),
        Colors = Ramp.Create(Seq(RampStop.Create(RampPosition.MinValue, new Vector4(new Vector3(0.8f), 1f))), RampInterpolation.Linear),
    }) switch { var snow => snow with { Coating = snow.Colors }, };

    public static ParticleField Sparks { get; } = Default with {
        Cells = Default.Cells with { Density = AxisFraction.Create(0.3f), Size = AxisFraction.Create(2f * 0.1f), Variation = AxisFraction.Create(0.8f), Spread = Magnitude.Create(2.3f), Softness = AxisFraction.Create(0.3f) },
        Heading = SignedAngle.Up,
        Speed = Drift.Create(0.4f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Create(0.03f / 2f * ReferenceFrame.GreaterSide),
        Emission = Emission.Default with { Cool = ColorTemperature.Create(1000d) },
        Incandescence = AxisFraction.MaxValue,
    };

    public static ParticleField CellRain { get; } = Default with {
        Covering = true,
        Radiance = new(Enabled: false, Exposure.Neutral),
        Cells = Default.Cells with { Variation = AxisFraction.MinValue, Spread = Magnitude.Zero, Softness = AxisFraction.MinValue },
        Heading = SignedAngle.Down,
        Defocus = ShortSideLength.Neutral,
        Iris = Iris.Default with { Blades = ApertureBlades.Create(4), Rotation = SignedAngle.Diagonal },
    };

    public static ParticleField Starfield { get; } = Rain with {
        Cells = new(ShortSideExtent.Create(0.005f * ReferenceFrame.GreaterSide), AxisFraction.Create(0.12f), AxisFraction.Create(2f * 0.1f), AxisFraction.Half, Magnitude.Create(4f), AxisFraction.MaxValue),
        Layers = PositiveCount.MinValue,
        Depth = PrimaryScale.MinValue,
        Speed = Drift.Neutral,
        Defocus = ShortSideLength.Neutral,
        Emission = new(ColorTemperature.Create(3000d), ColorTemperature.Create(12000d)),
        Incandescence = AxisFraction.MaxValue,
        Layer = GeneratedLayer.Added,
        Covering = false,
        Radiance = new(Enabled: true, Exposure.Neutral),
    };

    public static Option<PixelPass> Pass(ParticleField state, PassContext context) =>
        state.Cells.Density == AxisFraction.MinValue || state.Cells.Size == AxisFraction.MinValue || state.Iris.Obstruction == AxisFraction.MaxValue
            || (!state.Covering && !state.Radiance.Enabled) ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(ParticleField state, PassContext context, PixelFrame frame, IProgress<int> progress) =>
        (state.Turbulence == ShortSideLength.Neutral
            ? (Sample: state.Basis.Sampler, Range: (Lower: 0f, Upper: 0f))
            : state.Basis.BoundedSampler).Bind(bounded => Try.lift(() => {
                (PointCells cells, int side, (ERational X, ERational Y) middle, ERational time, ERational shutter) =
                    (state.Cells, context.Extent.ShortSide, (ERational.FromInt32(context.Extent.Width).Divide(2), ERational.FromInt32(context.Extent.Height).Divide(2)),
                        ERational.FromSingle(state.Timing.At(context)), ERational.FromSingle(state.Shutter));
                Func<LatticePoint, uint, NoiseSample<LatticePoint>> plain = (NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler;
                uint field = CoordinateHash.Field(NoiseStream.ParticleField, state.Seed, 0u);
                (uint presence, uint tone, uint across, uint down, uint twinkle) =
                    (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
                (RampTable colors, RampTable coating, Func<float, Vector3> thermal) =
                    (state.Colors.Tabulate(context.Working), state.Coating.Tabulate(context.Working), Emission.Span((context.Working, state.Emission.Cool, state.Emission.Hot)));
                (double focus, double radiance, (ERational X, ERational Y) heading) =
                    (double.Pow(state.Depth, -(double)state.Focus), state.Radiance.Active.Match(Some: static exposure => double.Exp2(exposure), None: static () => 0d),
                        double.SinCos(state.Heading) switch { var (sin, cos) => (ERational.FromSingle(state.Speed).Multiply(ERational.FromDouble(cos)), ERational.FromSingle(state.Speed).Multiply(ERational.FromDouble(-sin))) });
                (ERational lowWind, ERational highWind) = (ERational.FromSingle(bounded.Range.Lower).Multiply(2).Subtract(1), ERational.FromSingle(bounded.Range.Upper).Multiply(2).Subtract(1));
                GaussLegendreRule rule = PointCells.Quadrature;
                List<Particle> particles = [];
                for (int layer = 0; layer < state.Layers; layer++) {
                    ERational middleDepth = ERational.FromInt32(layer).Add(ERational.One.Divide(2)).Divide((int)state.Layers);
                    ERational depthWidth = ERational.FromSingle(state.DepthVariation).Divide(2).Divide((int)state.Layers);
                    (double reference, double nearest, double farthest) =
                        (double.Pow(state.Depth, -middleDepth.ToDouble()), double.Pow(state.Depth, -middleDepth.Subtract(depthWidth).ToDouble()), double.Pow(state.Depth, -middleDepth.Add(depthWidth).ToDouble()));
                    ERational cell = ERational.FromSingle(cells.Spacing).Multiply(ERational.FromDouble(reference));
                    ERational support = ERational.FromDouble(Grown(side * ((cells.Spacing * nearest * cells.Draw(Vector4.Zero).Radius)
                        + (state.Defocus * double.Max(double.Abs(nearest - focus), double.Abs(farthest - focus))))) / side);
                    ERational[] wind = [lowWind.Multiply(ERational.FromDouble(nearest)), lowWind.Multiply(ERational.FromDouble(farthest)), highWind.Multiply(ERational.FromDouble(nearest)), highWind.Multiply(ERational.FromDouble(farthest))];
                    (ERational windLow, ERational windHigh) = (wind.Aggregate(ERational.Min).Multiply(ERational.FromSingle(state.Turbulence)), wind.Aggregate(ERational.Max).Multiply(ERational.FromSingle(state.Turbulence)));
                    ((ERational Low, ERational High) horizontal, (ERational Low, ERational High) vertical) =
                        (Travel(heading.X, nearest, farthest), Travel(heading.Y, nearest, farthest));
                    (EInteger left, EInteger top, EInteger right, EInteger bottom) =
                        (Lattice(frame.Window.Left, middle.X, horizontal.High.Add(windHigh).Add(support), cell).Subtract(1),
                            Lattice(frame.Window.Top, middle.Y, vertical.High.Add(windHigh).Add(support), cell).Subtract(1),
                            Lattice(frame.Window.Right, middle.X, horizontal.Low.Add(windLow).Subtract(support), cell),
                            Lattice(frame.Window.Bottom, middle.Y, vertical.Low.Add(windLow).Subtract(support), cell));
                    EInteger width = right.Subtract(left).Add(1);
                    EInteger count = width.Multiply(bottom.Subtract(top).Add(1));
                    for (EInteger index = EInteger.Zero; index.CompareTo(count) < 0; index = index.Add(1)) {
                        (EInteger column, EInteger line) = (left.Add(index.Remainder(width)), top.Add(index.Divide(width)));
                        LatticePoint point = new(ERational.FromEInteger(column), ERational.FromEInteger(line), ERational.FromInt32(layer), ERational.Zero);
                        Vector4 position = NoiseFunctions.White(point, presence);
                        if (position.X >= cells.Density) continue;
                        (Vector4 draw, ERational depth) = (NoiseFunctions.White(point, tone), middleDepth.Add(ERational.FromSingle(position.W).Subtract(ERational.One.Divide(2)).Multiply(ERational.FromSingle(state.DepthVariation)).Divide((int)state.Layers)));
                        (double scale, (double radius, double gain)) = (double.Pow(state.Depth, -depth.ToDouble()), cells.Draw(draw));
                        double core = (double)side * cells.Spacing * scale * radius;
                        double optical = core + ((double)side * state.Defocus * double.Abs(scale - focus));
                        Vector4 spectrum = colors.Sample(draw.X);
                        Vector3 tint = Vector3.Lerp(Vector3.One, thermal(draw.X), state.Incandescence);
                        Vector256<double> area = state.Iris.Integral(optical, cells.Softness * optical, state.Fringe);
                        double energy = radiance * spectrum.W * gain * double.Pi * core * core;
                        Vector256<double> light = Vector256.Create(area[0] is 0d ? 0d : spectrum.X * (double)tint.X * energy / area[0],
                            area[1] is 0d ? 0d : spectrum.Y * (double)tint.Y * energy / area[1], area[2] is 0d ? 0d : spectrum.Z * (double)tint.Z * energy / area[2], 0d);
                        particles.Add(new(depth, layer, column, line, point,
                            (ERational.FromEInteger(column).Add(ERational.FromSingle(position.Y)).Multiply(cell), ERational.FromEInteger(line).Add(ERational.FromSingle(position.Z)).Multiply(cell)),
                            scale, optical, gain, state.Covering ? coating.Sample(draw.X) : Vector4.Zero, light));
                    }
                }
                Particle[] ordered = [.. particles.OrderByDescending(static particle => particle.Depth).ThenBy(static particle => particle.Layer).ThenBy(static particle => particle.Line).ThenBy(static particle => particle.Column)];
                return state.Layer.Pass((row, column, line) => {
                    long y = context.Extent.Height - 1L - line;
                    for (int x = 0; x < row.Length; x++) {
                        Vector256<double> integrated = Vector256<double>.Zero;
                        for (int sample = 0; sample < rule.Order; sample++) {
                            double u = rule.GetAbscissa(sample);
                            ERational tau = time.Subtract(shutter.Multiply(ERational.FromDouble(1d - u)));
                            double weight = rule.GetWeight(sample) * (1d - state.Fade + (state.Fade * u)) / (1d - (state.Fade / 2d));
                            integrated += Spatial(column + x, y, tau) * weight;
                        }
                        row[x] = new((float)integrated[0], (float)integrated[1], (float)integrated[2], (float)integrated[3]);
                    }
                }).Run(frame, progress);

                EInteger Lattice(long pixel, ERational center, ERational displacement, ERational cell) =>
                    NoiseFunctions.Split(ERational.FromInt64(pixel).Subtract(center).Divide(side).Subtract(displacement).Divide(cell)).Cell;

                (ERational Low, ERational High) Travel(ERational rate, double near, double far) {
                    ERational[] endpoints = [
                        rate.Multiply(ERational.FromDouble(near)).Multiply(time), rate.Multiply(ERational.FromDouble(far)).Multiply(time),
                        rate.Multiply(ERational.FromDouble(near)).Multiply(time.Subtract(shutter)), rate.Multiply(ERational.FromDouble(far)).Multiply(time.Subtract(shutter))];
                    return (endpoints.Aggregate(ERational.Min), endpoints.Aggregate(ERational.Max));
                }

                double Grown(double radius) => (state.Iris.Circumradius * radius) + (cells.Softness * radius / 2d);

                (ERational X, ERational Y) Moved(Particle particle, ERational tau) {
                    ERational scale = ERational.FromDouble(particle.Scale);
                    (ERational X, ERational Y) = (particle.Rest.X.Add(heading.X.Multiply(scale).Multiply(tau)), particle.Rest.Y.Add(heading.Y.Multiply(scale).Multiply(tau)));
                    if (state.Turbulence == ShortSideLength.Neutral)
                        return (middle.X.Add(X.Multiply(side)), middle.Y.Add(Y.Multiply(side)));
                    ERational size = scale.Multiply(ERational.FromSingle(state.Eddy));
                    LatticePoint sample = new(X.Divide(size), Y.Divide(size), ERational.FromSingle(state.Evolution).Multiply(tau), ERational.Zero);
                    ERational gain = ERational.FromSingle(state.Turbulence).Multiply(scale);
                    return (middle.X.Add(X.Add(gain.Multiply(ERational.FromSingle(bounded.Sample(sample, across).Value).Multiply(2).Subtract(1))).Multiply(side)),
                        middle.Y.Add(Y.Add(gain.Multiply(ERational.FromSingle(bounded.Sample(sample, down).Value).Multiply(2).Subtract(1))).Multiply(side)));
                }

                Vector256<double> Spatial(long column, long line, ERational tau) {
                    (ERational left, ERational top, ERational right, ERational bottom) = (ERational.FromInt64(column), ERational.FromInt64(line), ERational.FromInt64(column + 1), ERational.FromInt64(line + 1));
                    (Particle Particle, (ERational X, ERational Y) Point, ERational LowX, ERational HighX, ERational LowY, ERational HighY, double Modulation)[] active = [..
                        from particle in ordered
                        let point = Moved(particle, tau)
                        let reach = ERational.FromDouble(Grown(particle.Radius))
                        let lowX = ERational.Max(left, point.X.Subtract(reach))
                        let highX = ERational.Min(right, point.X.Add(reach))
                        let lowY = ERational.Max(top, point.Y.Subtract(reach))
                        let highY = ERational.Min(bottom, point.Y.Add(reach))
                        where lowX.CompareTo(highX) < 0 && lowY.CompareTo(highY) < 0
                        let modulation = 1d - (state.Twinkle * plain(particle.Point with { W = ERational.FromSingle(state.TwinkleRate).Multiply(tau) }, twinkle).Value)
                        select (Particle: particle, Point: point, LowX: lowX, HighX: highX, LowY: lowY, HighY: highY, Modulation: modulation)];
                    ERational[] xs = [.. new SortedSet<ERational>(active.SelectMany(static item => new[] { item.LowX, item.HighX }).Append(left).Append(right))];
                    ERational[] ys = [.. new SortedSet<ERational>(active.SelectMany(static item => new[] { item.LowY, item.HighY }).Append(top).Append(bottom))];
                    Vector256<double> total = Vector256<double>.Zero;
                    foreach ((int x, int y) in from y in Enumerable.Range(0, ys.Length - 1) from x in Enumerable.Range(0, xs.Length - 1) select (x, y)) {
                        (ERational width, ERational height) = (xs[x + 1].Subtract(xs[x]), ys[y + 1].Subtract(ys[y]));
                        double area = width.Multiply(height).ToDouble();
                        for (int sample = 0; sample < rule.Order * rule.Order; sample++) {
                            (int acrossNode, int downNode) = (sample % rule.Order, sample / rule.Order);
                            (ERational X, ERational Y) = (xs[x].Add(width.Multiply(ERational.FromDouble(rule.GetAbscissa(acrossNode)))), ys[y].Add(height.Multiply(ERational.FromDouble(rule.GetAbscissa(downNode)))));
                            Vector256<double> folded = Vector256<double>.Zero;
                            foreach ((Particle Particle, (ERational X, ERational Y) Point, ERational LowX, ERational HighX, ERational LowY, ERational HighY, double Modulation) item in active) {
                                (double reach, double edge) = state.Iris.Outline((X.Subtract(item.Point.X).ToDouble(), Y.Subtract(item.Point.Y).ToDouble()));
                                Vector256<double> profile = state.Iris.Weights(reach, edge, item.Particle.Radius, cells.Softness * item.Particle.Radius, state.Fringe);
                                double opacity = profile[3] * item.Particle.Gain * item.Particle.Coating.W;
                                Vector256<double> coat = Vector256.Create(item.Particle.Coating.X, item.Particle.Coating.Y, item.Particle.Coating.Z, 1d);
                                folded = ((1d - opacity) * folded) + (opacity * coat) + (profile * item.Particle.Light * item.Modulation);
                            }
                            total += folded * (area * rule.GetWeight(acrossNode) * rule.GetWeight(downNode));
                        }
                    }
                    return total;
                }
            }).Run());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParticleFieldParameter : IStateParameter<ParticleField> {
    public static readonly ParticleFieldParameter Cells = new("cells", new StateParameter<ParticleField>.Record<PointCells>(Lens<ParticleField, PointCells>.New(static particles => particles.Cells, static cells => particles => particles with { Cells = cells })),
        new("Population", "Spacing, presence, size, and light of the particles"));
    public static readonly ParticleFieldParameter Layers = new("layers", StateParameter.Bounded(Lens<ParticleField, PositiveCount>.New(static particles => particles.Layers, static layers => particles => particles with { Layers = layers }), new Presentation<int> { Step = 1, Soft = (1, 8) }),
        new("Layers", "Depth layers of the particle field"));
    public static readonly ParticleFieldParameter Heading = new("heading", StateParameter.Bounded(Lens<ParticleField, SignedAngle>.New(static particles => particles.Heading, static heading => particles => particles with { Heading = heading })),
        new("Heading", "Direction of travel, counterclockwise from the frame's right"));
    public static readonly ParticleFieldParameter Speed = new("speed", StateParameter.Bounded(Lens<ParticleField, Drift>.New(static particles => particles.Speed, static speed => particles => particles with { Speed = speed }), Drift.Presentation with { Soft = (0f, 2f) }),
        new("Speed", "Short sides per second the nearest layer travels"));
    public static readonly ParticleFieldParameter Basis = new("basis", new StateParameter<ParticleField>.Variant<NoiseBasis>(Lens<ParticleField, NoiseBasis>.New(static particles => particles.Basis, static basis => particles => particles with { Basis = basis })),
        new("Wind", "Noise displacing each particle in the wind field"));
    public static readonly ParticleFieldParameter Turbulence = new("turbulence", StateParameter.Bounded(Lens<ParticleField, ShortSideLength>.New(static particles => particles.Turbulence, static turbulence => particles => particles with { Turbulence = turbulence }), ShortSideLength.Presentation with { Soft = (0f, 0.2f) }),
        new("Turbulence", "Sway of each particle in the wind field as a share of the frame's short side"));
    public static readonly ParticleFieldParameter Eddy = new("eddy", StateParameter.Bounded(Lens<ParticleField, ShortSideExtent>.New(static particles => particles.Eddy, static eddy => particles => particles with { Eddy = eddy }), ShortSideExtent.Presentation with { Soft = (0.01f, 2f) }),
        new("Eddy size", "Feature width of the wind field as a share of the frame's short side"));
    public static readonly ParticleFieldParameter Evolution = new("evolution", StateParameter.Bounded(Lens<ParticleField, Frequency>.New(static particles => particles.Evolution, static evolution => particles => particles with { Evolution = evolution }), Tracks.Rate),
        new("Evolution", "Noise units per second the wind field changes through"));
    public static readonly ParticleFieldParameter Depth = new("depth", StateParameter.Bounded(Lens<ParticleField, PrimaryScale>.New(static particles => particles.Depth, static depth => particles => particles with { Depth = depth }), new Presentation<float> { Soft = (1f, 4f) }),
        new("Depth", "Distance of the farthest layer over the nearest"));
    public static readonly ParticleFieldParameter DepthVariation = new("depth-variation", StateParameter.Bounded(Lens<ParticleField, AxisFraction>.New(static particles => particles.DepthVariation, static variation => particles => particles with { DepthVariation = variation })),
        new("Depth variation", "Variation of particle depth within each layer"));
    public static readonly ParticleFieldParameter Focus = new("focus", StateParameter.Bounded(Lens<ParticleField, AxisFraction>.New(static particles => particles.Focus, static focus => particles => particles with { Focus = focus })),
        new("Focus", "Depth in focus, 0% the nearest layer and 100% the farthest"));
    public static readonly ParticleFieldParameter Defocus = new("defocus", StateParameter.Bounded(Lens<ParticleField, ShortSideLength>.New(static particles => particles.Defocus, static defocus => particles => particles with { Defocus = defocus }), ShortSideLength.Presentation with { Soft = (0f, 0.05f) }),
        new("Defocus", "Largest out-of-focus disc radius as a share of the frame's short side"));
    public static readonly ParticleFieldParameter Iris = new("iris", new StateParameter<ParticleField>.Record<Iris>(Lens<ParticleField, Iris>.New(static particles => particles.Iris, static iris => particles => particles with { Iris = iris })),
        new("Aperture", "Outline of the lens opening each out-of-focus disc and diffraction spike takes"));
    public static readonly ParticleFieldParameter Fringe = new("fringe", StateParameter.Bounded(Lens<ParticleField, SignedFraction>.New(static particles => particles.Fringe, static fringe => particles => particles with { Fringe = fringe })),
        new("Color shift", "Red and blue disc sizes apart from green"));
    public static readonly ParticleFieldParameter Shutter = new("shutter", StateParameter.Bounded(Lens<ParticleField, ShutterTime>.New(static particles => particles.Shutter, static shutter => particles => particles with { Shutter = shutter }), ShutterTime.Presentation with { Soft = (0f, 1f / 24f) }),
        new("Shutter", "Seconds the shutter stays open, each particle streaking along its travel"));
    public static readonly ParticleFieldParameter Fade = new("fade", StateParameter.Bounded(Lens<ParticleField, AxisFraction>.New(static particles => particles.Fade, static fade => particles => particles with { Fade = fade })),
        new("Fade", "Dimming of each streak toward its tail"));
    public static readonly ParticleFieldParameter Colors = new("colors", new StateParameter<ParticleField>.Gradient(Lens<ParticleField, Ramp>.New(static particles => particles.Colors, static colors => particles => particles with { Colors = colors })),
        new("Colors", "Colors each particle draws from"));
    public static readonly ParticleFieldParameter Covering = new("covering", new StateParameter<ParticleField>.Toggle(Lens<ParticleField, bool>.New(static particles => particles.Covering, static covering => particles => particles with { Covering = covering })),
        new("Covering", "Particles cover the light behind them"));
    public static readonly ParticleFieldParameter Coating = new("coating", new StateParameter<ParticleField>.Gradient(Lens<ParticleField, Ramp>.New(static particles => particles.Coating, static coating => particles => particles with { Coating = coating })),
        new("Coating", "Covering colors and opacity each particle draws from"));
    public static readonly ParticleFieldParameter Radiance = new("radiance", StateParameter.OptionalBounded(Lens<ParticleField, Gated<Exposure>>.New(static particles => particles.Radiance, static radiance => particles => particles with { Radiance = radiance })),
        new("Radiance", "Stops of emitted particle light, independent of covering"));
    public static readonly ParticleFieldParameter Emission = new("emission", new StateParameter<ParticleField>.Record<Emission>(Lens<ParticleField, Emission>.New(static particles => particles.Emission, static emission => particles => particles with { Emission = emission })),
        new("Blackbody", "Blackbody light between the cool and hot temperatures"));
    public static readonly ParticleFieldParameter Incandescence = new("incandescence", StateParameter.Bounded(Lens<ParticleField, AxisFraction>.New(static particles => particles.Incandescence, static incandescence => particles => particles with { Incandescence = incandescence })),
        new("Incandescence", "Share of each particle's color taken from its blackbody light"));
    public static readonly ParticleFieldParameter Twinkle = new("twinkle", StateParameter.Bounded(Lens<ParticleField, AxisFraction>.New(static particles => particles.Twinkle, static twinkle => particles => particles with { Twinkle = twinkle })),
        new("Twinkle", "Share of each particle's light varied over time"));
    public static readonly ParticleFieldParameter TwinkleRate = new("twinkle-rate", StateParameter.Bounded(Lens<ParticleField, Frequency>.New(static particles => particles.TwinkleRate, static rate => particles => particles with { TwinkleRate = rate }), Tracks.Rate),
        new("Twinkle rate", "Twinkle periods per second"));
    public static readonly ParticleFieldParameter Layer = new("layer", new StateParameter<ParticleField>.Record<GeneratedLayer>(Lens<ParticleField, GeneratedLayer>.New(static particles => particles.Layer, static layer => particles => particles with { Layer = layer })),
        new("Layer", "Composition of the generated layer over the frame"));
    public static readonly ParticleFieldParameter Seed = new("seed", StateParameter.Bounded(Lens<ParticleField, Seed>.New(static particles => particles.Seed, static seed => particles => particles with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));
    public static readonly ParticleFieldParameter Timing = new("timing", new StateParameter<ParticleField>.Record<Timing>(Lens<ParticleField, Timing>.New(static particles => particles.Timing, static timing => particles => particles with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));

    public StateParameter<ParticleField> Kind { get; }
    public ParameterText Text { get; }
}

file readonly record struct Particle(ERational Depth, int Layer, EInteger Column, EInteger Line, LatticePoint Point, (ERational X, ERational Y) Rest, double Scale, double Radius, double Gain, Vector4 Coating, Vector256<double> Light);
