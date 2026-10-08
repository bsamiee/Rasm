using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Filters.Optics;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Generators;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ParticleSpacing : IMinMaxValue<ParticleSpacing> {
    public static ParticleSpacing MinValue { get; } = new(0.005f);
    public static ParticleSpacing MaxValue { get; } = new(8f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ParticleLayers : IMinMaxValue<ParticleLayers> {
    public static ParticleLayers MinValue { get; } = new(1);
    public static ParticleLayers MaxValue { get; } = new(8);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct DepthRatio : IMinMaxValue<DepthRatio> {
    public static DepthRatio MinValue { get; } = new(1f);
    public static DepthRatio MaxValue { get; } = new(4f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ParticleSpread : IMinMaxValue<ParticleSpread> {
    public static ParticleSpread MinValue => Neutral;
    public static ParticleSpread MaxValue { get; } = new(Exposure.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

public sealed record PointCells(ParticleSpacing Spacing, AxisFraction Density, AxisFraction Size, AxisFraction Variation, ParticleSpread Spread, AxisFraction Softness)
    : IStateRecord<PointCells, PointCellsParameter, InvalidGenerator> {
    private static readonly Cellular Sites = Cellular.Default with { Randomness = AxisFraction.Half };
    private static readonly float Largest = (1f - Sites.Randomness) / 2f;

    public static PointCells Default { get; } = new(
        ParticleSpacing.Create(0.01f * ReferenceFrame.GreaterSide), AxisFraction.Half, AxisFraction.Create(2f * 0.2f), AxisFraction.Half, ParticleSpread.Create(1f), AxisFraction.Half);

    internal (float Radius, float Gain) Draw(Vector4 draw) => (Size * (1f - (Variation * draw.Y)) * Largest, float.Exp2(-Spread * draw.Z));

    internal (float Coverage, float Tone) Disc(Vector2 point, float side, uint field) {
        Vector4 cell = new(point / Spacing, 0f, 0f);
        Vector4 site = NoiseDimensions.Two.F1(cell, Octaves.Plain, Sites, AxisFraction.MinValue, CoordinateHash.Branch(field, 0u)).Site;
        (Vector4 draw, float rim) = (NoiseFunctions.White(site, CoordinateHash.Branch(field, 1u)), 1.5f / (Spacing * side));
        (float radius, float gain) = Draw(draw);
        float reach = float.Min(float.Max(radius, rim), Largest);
        float band = float.Min(float.Max(Softness * reach, rim), reach);
        float edge = 1f - (float)RampInterpolation.Ease.Weight(Easing.Saturate((Vector4.Distance(cell, site) - reach + band) / band));
        return draw.X < Density ? (edge * gain * Area(radius, Softness * radius) / Area(reach, band), draw.X / Density) : (0f, 0f);

        static float Area(float radius, float band) => (radius * (radius - band)) + (0.3f * band * band);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointCellsParameter : IStateParameter<PointCells> {
    internal static readonly Presentation<AxisFraction, float> Share = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    public static readonly PointCellsParameter Spacing = new("spacing", new StateParameter<PointCells>.Bounded<ParticleSpacing, float, InvalidGenerator>(
        Lens<PointCells, ParticleSpacing>.New(static cells => cells.Spacing, static spacing => cells => cells with { Spacing = spacing }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 0.5f), Scale = TrackScale.Log }));
    public static readonly PointCellsParameter Density = new("density", new StateParameter<PointCells>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<PointCells, AxisFraction>.New(static cells => cells.Density, static density => cells => cells with { Density = density }), Share));
    public static readonly PointCellsParameter Size = new("size", new StateParameter<PointCells>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<PointCells, AxisFraction>.New(static cells => cells.Size, static size => cells => cells with { Size = size }), Share));
    public static readonly PointCellsParameter Variation = new("variation", new StateParameter<PointCells>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<PointCells, AxisFraction>.New(static cells => cells.Variation, static variation => cells => cells with { Variation = variation }), Share));
    public static readonly PointCellsParameter Spread = new("spread", new StateParameter<PointCells>.Bounded<ParticleSpread, float, InvalidGenerator>(
        Lens<PointCells, ParticleSpread>.New(static cells => cells.Spread, static spread => cells => cells with { Spread = spread }), new() { Soft = (0f, 10f), Step = Exposure.Presentation.Step, Decimals = Exposure.Presentation.Decimals }));
    public static readonly PointCellsParameter Softness = new("softness", new StateParameter<PointCells>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<PointCells, AxisFraction>.New(static cells => cells.Softness, static softness => cells => cells with { Softness = softness }), Share));

    public StateParameter<PointCells> Kind { get; }
}

public sealed record ParticleField(
    PointCells Cells, ParticleLayers Layers,
    SignedAngle Heading, Drift Speed, NoiseBasis Basis, ShortSideLength Turbulence, ShortSideExtent Eddy, Frequency Evolution,
    DepthRatio Depth, AxisFraction Focus, ShortSideLength Defocus, Iris Iris, ShutterTime Shutter, Mix Fade,
    Ramp Colors, Emission Emission, Mix Incandescence, Mix Twinkle, Frequency TwinkleRate, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<ParticleField, ParticleFieldParameter, InvalidGenerator>, IPixelStage<ParticleField> {
    public static ParticleField Default { get; } = (Unit: ReferenceFrame.GreaterSide / 2f, Drift: 24f * 0.05f * 0.5f, Scale: 1.6f) switch {
        var (unit, drift, scale) => new(
            new(ParticleSpacing.Create(unit / 10f), AxisFraction.MaxValue, AxisFraction.Create(2f * 0.2f), AxisFraction.Half, ParticleSpread.Create(1f), AxisFraction.Create(1f / 1.8f)), ParticleLayers.Create(4),
            SignedAngle.Create(float.Atan2(0.5f, 0.4f)), Drift.Create(drift * float.Hypot(0.4f, 0.5f) * unit), NoiseBasis.Default,
            ShortSideLength.Create(0.12f / 2f * unit), ShortSideExtent.Create(unit / scale), Frequency.Create(scale * drift / 3200f),
            DepthRatio.MaxValue, AxisFraction.MaxValue, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide), Iris.Hexagon, ShutterTime.Create(0.5f / 24f), Mix.MinValue,
            Ramp.Create(Seq(RampStop.White.At(RampPosition.MinValue)), RampInterpolation.Linear), Emission.Default, Mix.MinValue, Mix.MinValue, Frequency.Neutral,
            GeneratedLayer.Added with { Exposure = Exposure.Create(2f) }, Seed.MinValue, Timing.Default),
    };

    public static ParticleField Rain { get; } = Default with {
        Cells = PointCells.Default,
        Heading = SignedAngle.Down,
        Speed = Drift.Create(2.5f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Neutral,
        Iris = Iris.Hexagon with { Roundness = AxisFraction.MaxValue },
        Layer = GeneratedLayer.Default,
    };

    public static ParticleField Snow { get; } = Rain with {
        Cells = new(ParticleSpacing.Create(0.03f * ReferenceFrame.GreaterSide), AxisFraction.Half, AxisFraction.Create(2f * 0.3f), AxisFraction.Create(0.6f), ParticleSpread.Create(1.3f), AxisFraction.Create(0.6f)),
        Speed = Drift.Create(0.3f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Create(0.02f / 2f * ReferenceFrame.GreaterSide),
        Colors = Ramp.Create(Seq(RampStop.Create(RampPosition.MinValue, new Vector4(new Vector3(0.8f), 1f))), RampInterpolation.Linear),
    };

    public static ParticleField Sparks { get; } = Default with {
        Cells = Default.Cells with { Density = AxisFraction.Create(0.3f), Size = AxisFraction.Create(2f * 0.1f), Variation = AxisFraction.Create(0.8f), Spread = ParticleSpread.Create(2.3f), Softness = AxisFraction.Create(0.3f) },
        Heading = SignedAngle.Up,
        Speed = Drift.Create(0.4f * ReferenceFrame.GreaterSide),
        Turbulence = ShortSideLength.Create(0.03f / 2f * ReferenceFrame.GreaterSide),
        Emission = Emission.Default with { Cool = ColorTemperature.Create(1000d) },
        Incandescence = Mix.Full,
    };

    public static ParticleField CellRain { get; } = Default with {
        Cells = Default.Cells with { Variation = AxisFraction.MinValue, Spread = ParticleSpread.Neutral, Softness = AxisFraction.MinValue },
        Heading = SignedAngle.Down,
        Defocus = ShortSideLength.Neutral,
        Iris = Iris.Hexagon with { Blades = ApertureBlades.Create(4), Rotation = SignedAngle.Diagonal },
    };

    public static ParticleField Starfield { get; } = Rain with {
        Cells = new(ParticleSpacing.Create(0.005f * ReferenceFrame.GreaterSide), AxisFraction.Create(0.12f), AxisFraction.Create(2f * 0.1f), AxisFraction.Half, ParticleSpread.Create(4f), AxisFraction.MaxValue),
        Layers = ParticleLayers.MinValue,
        Depth = DepthRatio.MinValue,
        Speed = Drift.Neutral,
        Defocus = ShortSideLength.Neutral,
        Emission = new(ColorTemperature.Create(3000d), ColorTemperature.Create(12000d)),
        Incandescence = Mix.Full,
        Layer = GeneratedLayer.Added,
    };

    public static Option<PixelPass> Pass(ParticleField state, PassContext context) =>
        state.Cells.Density == AxisFraction.MinValue ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(ParticleField state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        const int tile = 16;
        const int wrap = (1 << 24) - 1;
        (PointCells cells, float side, Vector2 middle, Rectangle window, double t) = (state.Cells, context.Extent.ShortSide, new Vector2(context.Extent.Width, context.Extent.Height) / 2f, frame.Window, state.Timing.At(context));
        uint field = CoordinateHash.Field(NoiseStream.ParticleField, state.Seed, 0u);
        (uint presence, uint tone, uint across, uint down, uint twinkle) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (Func<Vector4, uint, NoiseSample> basis, Func<Vector4, uint, NoiseSample> plain) = (state.Basis.Sampler, (NoiseBasis.Plain with { Dimensions = NoiseDimensions.Four }).Sampler);
        (RampTable table, Func<float, Vector3> emission) = (state.Colors.Tabulate(context.Working), Emission.Span((context.Working, state.Emission.Cool, state.Emission.Hot)));
        (float circumradius, float focus, int columns, Vector2 heading) = (state.Iris.Circumradius, float.Pow(state.Depth, -(float)state.Focus), (window.Width + tile - 1) / tile,
            float.SinCos(state.Heading) switch { var (sin, cos) => state.Speed * new Vector2(cos, -sin) });
        (List<Particle> particles, List<(int Tile, float Depth, int Index)> entries) = ([], []);
        for (int layer = 0; layer < state.Layers; layer++) {
            float scale = float.Pow(state.Depth, -(layer + 0.5f) / state.Layers);
            (double cell, Vector2 velocity) = (cells.Spacing * (double)scale, heading * scale);
            float reach = (Grown(float.Max(side * ((cells.Spacing * cells.Draw(Vector4.Zero).Radius) + state.Defocus), 0.5f)) / side) + (float.Abs(state.Speed) * scale * state.Shutter) + (2f * state.Turbulence);
            (long left, long top) = (Lattice(window.Left - middle.X, -reach, velocity.X, cell), Lattice(window.Top - middle.Y, -reach, velocity.Y, cell));
            (long wide, long tall) = (Lattice(window.Right - middle.X, reach, velocity.X, cell) - left + 1, Lattice(window.Bottom - middle.Y, reach, velocity.Y, cell) - top + 1);
            for (long n = 0; n < wide * tall; n++) {
                (long i, long j) = (left + (n % wide), top + (n / wide));
                Vector4 point = new(i & wrap, j & wrap, layer, 0f);
                Vector4 a = NoiseFunctions.White(point, presence);
                if (a.X >= cells.Density)
                    continue;
                (double x, double y, float z, Vector4 b) = ((i + (double)a.Y) * cell, (j + (double)a.Z) * cell, (layer + a.W) / state.Layers, NoiseFunctions.White(point, tone));
                (float k, Vector4 stop, (float size, float gain)) = (float.Pow(state.Depth, -z), table.Sample(b.X), cells.Draw(b));
                (Vector2 head, Vector2 tail, float core) = (Moved(x, y, velocity, k, t), Moved(x, y, velocity, k, t - state.Shutter), side * cells.Spacing * k * size);
                (float radius, float shown) = (float.Max(core + (side * state.Defocus * float.Abs(k - focus)), 0.5f), stop.W * gain * (1f - (state.Twinkle * plain(point with { W = state.TwinkleRate * (float)t }, twinkle).Value)));
                Particle drawn = new(tail, head, radius, Vector3.Zero, shown * core * core / ((radius * radius) + (2f / float.Pi * radius * Vector2.Distance(head, tail))));
                (Vector2 low, Vector2 high) = new Vector2(Grown(radius)) switch { var grow => (Vector2.Min(tail, head) - grow, Vector2.Max(tail, head) + grow) };
                Rectangle pixels = Rectangle.FromLTRB((int)float.Ceiling(low.X - 0.5f), (int)float.Ceiling(low.Y - 0.5f), (int)float.Floor(high.X - 0.5f) + 1, (int)float.Floor(high.Y - 0.5f) + 1);
                if (!pixels.IntersectsWith(window))
                    continue;
                Vector3 mass = Enumerable.Range(0, pixels.Width * pixels.Height).Aggregate(Vector3.Zero, (sum, m) => sum + Weight(drawn, new Vector2(pixels.Left + (m % pixels.Width) + 0.5f, pixels.Top + (m / pixels.Width) + 0.5f)).AsVector3());
                Rectangle covered = Rectangle.Intersect(pixels, window);
                Rectangle tiles = Rectangle.FromLTRB((covered.Left - window.Left) / tile, (covered.Top - window.Top) / tile, ((covered.Right - window.Left - 1) / tile) + 1, ((covered.Bottom - window.Top - 1) / tile) + 1);
                entries.AddRange(Enumerable.Range(0, tiles.Width * tiles.Height).Select(m => (((tiles.Top + (m / tiles.Width)) * columns) + tiles.Left + (m % tiles.Width), z, particles.Count)));
                Vector3 light = stop.AsVector3() * Vector3.Lerp(Vector3.One, emission(b.X), state.Incandescence) * (shown * float.Pi * core * core);
                particles.Add(drawn with { Light = Vector3.ConditionalSelect(Vector3.GreaterThan(mass, Vector3.Zero), light / mass, Vector3.Zero) });
            }
        }
        entries.Sort();
        int[] starts = [.. Enumerable.Range(0, (columns * ((window.Height + tile - 1) / tile)) + 1).Select(at => ~entries.BinarySearch((at, float.NegativeInfinity, -1)))];
        return state.Layer.Pass((row, column, line) => {
            int y = context.Extent.Height - 1 - line;
            for (int i = 0; i < row.Length; i++) {
                (int at, Vector2 pixel, Vector3 light, float clear) = (((y - window.Top) / tile * columns) + ((column + i - window.Left) / tile), new Vector2(column + i + 0.5f, y + 0.5f), Vector3.Zero, 1f);
                foreach ((_, _, int index) in CollectionsMarshal.AsSpan(entries)[starts[at]..starts[at + 1]])
                    (light, clear) = Weight(particles[index], pixel) switch { var weight => (light + (clear * particles[index].Light * weight.AsVector3()), clear * (1f - (weight.W * particles[index].Opacity))) };
                row[i] = clear < 1f ? new Vector4(light / (1f - clear), 1f - clear) : Vector4.Zero;
            }
        }).Run(frame, progress);

        long Lattice(float pixel, float margin, double rate, double cell) => (long)Math.Floor(((pixel / side) + margin - (rate * t)) / cell);

        float Grown(float radius) => (circumradius * radius) + (((cells.Softness * radius) + 1f) / 2f);

        Vector2 Moved(double x, double y, Vector2 velocity, float k, double tau) {
            Vector2 rest = new((float)(x + (velocity.X * tau)), (float)(y + (velocity.Y * tau)));
            Vector4 sample = new(rest / (k * state.Eddy), state.Evolution * (float)tau, 0f);
            return middle + (side * (rest + (state.Turbulence * k * new Vector2((2f * basis(sample, across).Value) - 1f, (2f * basis(sample, down).Value) - 1f))));
        }

        Vector4 Weight(Particle drawn, Vector2 pixel) =>
            (drawn.Head - drawn.Tail) switch {
                var sweep => (sweep == Vector2.Zero ? 1f : Easing.Saturate(Vector2.Dot(pixel - drawn.Tail, sweep) / sweep.LengthSquared())) switch {
                    var u => state.Iris.Outline(pixel - drawn.Tail - (u * sweep)) switch { var (outline, edge) => state.Iris.Weights(outline, edge, drawn.Radius, (cells.Softness * drawn.Radius) + 1f) * float.Lerp(1f, u, state.Fade) },
                },
            };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParticleFieldParameter : IStateParameter<ParticleField> {
    private static readonly Lens<ParticleField, Iris> Diaphragm = Lens<ParticleField, Iris>.New(static particles => particles.Iris, static iris => particles => particles with { Iris = iris });
    private static readonly (StateParameter<ParticleField> Blades, StateParameter<ParticleField> Rotation, StateParameter<ParticleField> Roundness, StateParameter<ParticleField> Obstruction, StateParameter<ParticleField> Squeeze) Outline = Iris.Kinds(Diaphragm);
    private static readonly Presentation<Mix, float> Blend = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<Frequency, float> Rate = Frequency.Presentation with { Soft = (-4f, 4f) };

    public static readonly ParticleFieldParameter Cells = new("cells", new StateParameter<ParticleField>.Record<PointCells>(
        Lens<ParticleField, PointCells>.New(static particles => particles.Cells, static cells => particles => particles with { Cells = cells })));
    public static readonly ParticleFieldParameter Layers = new("layers", new StateParameter<ParticleField>.Bounded<ParticleLayers, int, InvalidGenerator>(
        Lens<ParticleField, ParticleLayers>.New(static particles => particles.Layers, static layers => particles => particles with { Layers = layers }), new() { Step = 1 }));
    public static readonly ParticleFieldParameter Heading = new("heading", new StateParameter<ParticleField>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<ParticleField, SignedAngle>.New(static particles => particles.Heading, static heading => particles => particles with { Heading = heading }), SignedAngle.Presentation));
    public static readonly ParticleFieldParameter Speed = new("speed", new StateParameter<ParticleField>.Bounded<Drift, float, InvalidGenerator>(
        Lens<ParticleField, Drift>.New(static particles => particles.Speed, static speed => particles => particles with { Speed = speed }), Drift.Presentation with { Soft = (0f, 2f) }));
    public static readonly ParticleFieldParameter Basis = new("basis", new StateParameter<ParticleField>.Record<NoiseBasis>(
        Lens<ParticleField, NoiseBasis>.New(static particles => particles.Basis, static basis => particles => particles with { Basis = basis })));
    public static readonly ParticleFieldParameter Turbulence = new("turbulence", new StateParameter<ParticleField>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ParticleField, ShortSideLength>.New(static particles => particles.Turbulence, static turbulence => particles => particles with { Turbulence = turbulence }), ShortSideLength.Presentation with { Soft = (0f, 0.2f) }));
    public static readonly ParticleFieldParameter Eddy = new("eddy", new StateParameter<ParticleField>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<ParticleField, ShortSideExtent>.New(static particles => particles.Eddy, static eddy => particles => particles with { Eddy = eddy }), ShortSideExtent.Presentation with { Soft = (0.01f, 2f) }));
    public static readonly ParticleFieldParameter Evolution = new("evolution", new StateParameter<ParticleField>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<ParticleField, Frequency>.New(static particles => particles.Evolution, static evolution => particles => particles with { Evolution = evolution }), Rate));
    public static readonly ParticleFieldParameter Depth = new("depth", new StateParameter<ParticleField>.Bounded<DepthRatio, float, InvalidGenerator>(
        Lens<ParticleField, DepthRatio>.New(static particles => particles.Depth, static depth => particles => particles with { Depth = depth }), new()));
    public static readonly ParticleFieldParameter Focus = new("focus", new StateParameter<ParticleField>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<ParticleField, AxisFraction>.New(static particles => particles.Focus, static focus => particles => particles with { Focus = focus }), PointCellsParameter.Share));
    public static readonly ParticleFieldParameter Defocus = new("defocus", new StateParameter<ParticleField>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ParticleField, ShortSideLength>.New(static particles => particles.Defocus, static defocus => particles => particles with { Defocus = defocus }), ShortSideLength.Presentation with { Soft = (0f, 0.05f) }));
    public static readonly ParticleFieldParameter Blades = new("blades", Outline.Blades);
    public static readonly ParticleFieldParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly ParticleFieldParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly ParticleFieldParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly ParticleFieldParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly ParticleFieldParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));
    public static readonly ParticleFieldParameter Shutter = new("shutter", new StateParameter<ParticleField>.Bounded<ShutterTime, float, InvalidGenerator>(
        Lens<ParticleField, ShutterTime>.New(static particles => particles.Shutter, static shutter => particles => particles with { Shutter = shutter }), ShutterTime.Presentation with { Soft = (0f, 1f / 24f) }));
    public static readonly ParticleFieldParameter Fade = new("fade", new StateParameter<ParticleField>.Bounded<Mix, float, InvalidGrade>(
        Lens<ParticleField, Mix>.New(static particles => particles.Fade, static fade => particles => particles with { Fade = fade }), Blend));
    public static readonly ParticleFieldParameter Colors = new("colors", new StateParameter<ParticleField>.Gradient(
        Lens<ParticleField, Ramp>.New(static particles => particles.Colors, static colors => particles => particles with { Colors = colors })));
    public static readonly ParticleFieldParameter Emission = new("emission", new StateParameter<ParticleField>.Record<Emission>(
        Lens<ParticleField, Emission>.New(static particles => particles.Emission, static emission => particles => particles with { Emission = emission })));
    public static readonly ParticleFieldParameter Incandescence = new("incandescence", new StateParameter<ParticleField>.Bounded<Mix, float, InvalidGrade>(
        Lens<ParticleField, Mix>.New(static particles => particles.Incandescence, static incandescence => particles => particles with { Incandescence = incandescence }), Blend));
    public static readonly ParticleFieldParameter Twinkle = new("twinkle", new StateParameter<ParticleField>.Bounded<Mix, float, InvalidGrade>(
        Lens<ParticleField, Mix>.New(static particles => particles.Twinkle, static twinkle => particles => particles with { Twinkle = twinkle }), Blend));
    public static readonly ParticleFieldParameter TwinkleRate = new("twinkle-rate", new StateParameter<ParticleField>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<ParticleField, Frequency>.New(static particles => particles.TwinkleRate, static rate => particles => particles with { TwinkleRate = rate }), Rate));
    public static readonly ParticleFieldParameter Layer = new("layer", new StateParameter<ParticleField>.Record<GeneratedLayer>(
        Lens<ParticleField, GeneratedLayer>.New(static particles => particles.Layer, static layer => particles => particles with { Layer = layer })));
    public static readonly ParticleFieldParameter Seed = new("seed", new StateParameter<ParticleField>.Bounded<Seed, int, InvalidGenerator>(
        Lens<ParticleField, Seed>.New(static particles => particles.Seed, static seed => particles => particles with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly ParticleFieldParameter Timing = new("timing", new StateParameter<ParticleField>.Record<Timing>(
        Lens<ParticleField, Timing>.New(static particles => particles.Timing, static timing => particles => particles with { Timing = timing })));

    public StateParameter<ParticleField> Kind { get; }
}

file readonly record struct Particle(Vector2 Tail, Vector2 Head, float Radius, Vector3 Light, float Opacity);
