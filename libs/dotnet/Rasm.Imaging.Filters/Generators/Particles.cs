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

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IParticleProfile<TSelf> where TSelf : struct, IParticleProfile<TSelf> {
    public static abstract TSelf Of(ParticleField state);
    public float Reach(float radius);
    public float Area(float radius);
    public float Floor { get; }
    public Vector4 Coverage(Vector2 offset, float radius);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ParticleSpacing : IMinMaxValue<ParticleSpacing> {
    public static ParticleSpacing MinValue { get; } = new(0.01f);
    public static ParticleSpacing MaxValue { get; } = new(8f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct ParticleLayers : IMinMaxValue<ParticleLayers> {
    public static ParticleLayers MinValue { get; } = new(1);
    public static ParticleLayers MaxValue { get; } = new(8);
    public static ParticleLayers Four { get; } = new(4);

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

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class ParticleShape {
    public static readonly ParticleShape Disc = new Profiled<DiscProfile>("disc");
    public static readonly ParticleShape Flake = new Profiled<FlakeProfile>("flake");

    internal abstract PixelPass Pass(ParticleField state, PassContext context);

    private sealed class Profiled<TProfile>(string key) : ParticleShape(key) where TProfile : struct, IParticleProfile<TProfile> {
        internal override PixelPass Pass(ParticleField state, PassContext context) => ParticleField.Field(state, context, TProfile.Of(state));
    }

    private readonly record struct DiscProfile(Iris Iris) : IParticleProfile<DiscProfile> {
        public static DiscProfile Of(ParticleField state) => new(state.Iris);

        public float Reach(float radius) => Iris.Circumradius * radius;

        public float Area(float radius) => float.Pi * radius * radius;

        public float Floor => 0.5f;

        public Vector4 Coverage(Vector2 offset, float radius) => Iris.Outline(offset) switch { var (reach, edge) => Iris.Weights(reach, edge, radius) };
    }

    private readonly record struct FlakeProfile : IParticleProfile<FlakeProfile> {
        public static FlakeProfile Of(ParticleField state) => default;

        public float Reach(float radius) => radius;

        public float Area(float radius) => float.Pi * radius * radius / 3f;

        public float Floor => 1f;

        public Vector4 Coverage(Vector2 offset, float radius) =>
            float.Max(1f - (offset.LengthSquared() / (radius * radius)), 0f) switch { var s => new Vector4(s * s) };
    }
}

public sealed record ParticleField(
    ParticleSpacing Spacing, AxisFraction Density, ParticleLayers Layers,
    SignedAngle Heading, Drift Speed, NoiseBasis Basis, ShortSideLength Turbulence, ShortSideExtent Eddy, Frequency Evolution,
    DepthRatio Depth, AxisFraction Focus, ShortSideLength Defocus,
    ParticleShape Shape, ShortSideLength Size, Iris Iris, ShutterTime Shutter, Mix Fade,
    Ramp Colors, ColorTemperature Cool, ColorTemperature Hot, Mix Incandescence, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<ParticleField, ParticleFieldParameter, InvalidGenerator>, IPixelStage<ParticleField> {
    private const int Tile = 16;
    private const long CellBits = (1L << 24) - 1;

    public static ParticleField Default { get; } = new(
        Valid.Value(ParticleSpacing.Validate(1f / 10f, provider: null, out ParticleSpacing spacing), spacing), AxisFraction.MaxValue, ParticleLayers.Four,
        Valid.Value(SignedAngle.Validate(float.Atan2(0.5f, 0.4f), provider: null, out SignedAngle heading), heading),
        Valid.Value(Drift.Validate(24f * 0.05f * 0.5f * float.Hypot(0.4f, 0.5f), provider: null, out Drift speed), speed), NoiseBasis.Default,
        Valid.Value(ShortSideLength.Validate(0.12f / 2f, provider: null, out ShortSideLength turbulence), turbulence),
        Valid.Value(ShortSideExtent.Validate(1f / 1.6f, provider: null, out ShortSideExtent eddy), eddy),
        Valid.Value(Frequency.Validate(24f * 0.05f * 0.5f / 3200f, provider: null, out Frequency evolution), evolution),
        DepthRatio.MaxValue, AxisFraction.MaxValue, Valid.Value(ShortSideLength.Validate(0.01f * ReferenceFrame.GreaterSide, provider: null, out ShortSideLength defocus), defocus),
        ParticleShape.Disc, Valid.Value(ShortSideLength.Validate(2f * 0.1f / 10f, provider: null, out ShortSideLength size), size), Iris.Hexagon, ShutterTime.Film, Mix.MinValue,
        Ramp.Grayscale, ColorTemperature.MinValue, ColorTemperature.Flame, Mix.MinValue,
        GeneratedLayer.Added with { Exposure = Valid.Value(Exposure.Validate(2f, provider: null, out Exposure exposure), exposure) }, Seed.MinValue, Timing.Standard);

    public static ParticleField Rain { get; } = Default with {
        Heading = SignedAngle.Down,
        Speed = Valid.Value(Drift.Validate(2.5f * ReferenceFrame.GreaterSide, provider: null, out Drift speed), speed),
        Iris = Iris.Hexagon with { Roundness = AxisFraction.MaxValue },
        Colors = Valid.Value(Ramp.Validate(Seq(RampStop.White.At(RampPosition.MinValue)), RampInterpolation.Linear, out Ramp? colors), colors),
        Layer = GeneratedLayer.Mixed,
    };

    public static ParticleField Snow { get; } = Rain with {
        Spacing = Valid.Value(ParticleSpacing.Validate(0.03f * ReferenceFrame.GreaterSide, provider: null, out ParticleSpacing spacing), spacing),
        Size = Valid.Value(ShortSideLength.Validate(0.3f * 0.03f * ReferenceFrame.GreaterSide, provider: null, out ShortSideLength size), size),
        Speed = Valid.Value(Drift.Validate(0.3f * ReferenceFrame.GreaterSide, provider: null, out Drift speed), speed),
        Turbulence = Valid.Value(ShortSideLength.Validate(0.02f / 2f * ReferenceFrame.GreaterSide, provider: null, out ShortSideLength turbulence), turbulence),
        Shape = ParticleShape.Flake,
        Colors = Valid.Value(Ramp.Validate(
            Seq(Valid.Value(RampStop.Validate(RampPosition.MinValue, new Vector4(0.8f, 0.8f, 0.8f, 1f), out RampStop grey), grey)), RampInterpolation.Linear, out Ramp? colors), colors),
    };

    public static ParticleField Sparks { get; } = Default with {
        Spacing = Valid.Value(ParticleSpacing.Validate(0.05f * ReferenceFrame.GreaterSide, provider: null, out ParticleSpacing spacing), spacing),
        Density = Valid.Value(AxisFraction.Validate(0.3f, provider: null, out AxisFraction density), density),
        Size = Valid.Value(ShortSideLength.Validate(0.1f * 0.05f * ReferenceFrame.GreaterSide, provider: null, out ShortSideLength size), size),
        Heading = SignedAngle.Up,
        Speed = Valid.Value(Drift.Validate(0.4f * ReferenceFrame.GreaterSide, provider: null, out Drift speed), speed),
        Turbulence = Valid.Value(ShortSideLength.Validate(0.03f / 2f * ReferenceFrame.GreaterSide, provider: null, out ShortSideLength turbulence), turbulence),
        Cool = Valid.Value(ColorTemperature.Validate(1000d, provider: null, out ColorTemperature cool), cool),
        Hot = ColorTemperature.Flame,
        Incandescence = Mix.Full,
    };

    public static ParticleField CellRain { get; } = Default with {
        Heading = SignedAngle.Down,
        Defocus = ShortSideLength.Neutral,
        Iris = Iris.Hexagon with {
            Blades = Valid.Value(ApertureBlades.Validate(4, provider: null, out ApertureBlades blades), blades),
            Rotation = SignedAngle.Diagonal,
            Roundness = AxisFraction.MinValue,
        },
    };

    public static Option<PixelPass> Pass(ParticleField state, PassContext context) =>
        state.Density == AxisFraction.MinValue ? None : Some(state.Shape.Pass(state, context));

    internal static PixelPass Field<TProfile>(ParticleField state, PassContext context, TProfile profile) where TProfile : struct, IParticleProfile<TProfile> =>
        new PixelPass.Frame((frame, progress) => Kernel(state, context, profile, frame, progress));

    private static Fin<Unit> Kernel<TProfile>(ParticleField state, PassContext context, TProfile profile, PixelFrame frame, IProgress<int> progress)
        where TProfile : struct, IParticleProfile<TProfile> {
        (int height, float side, Vector2 middle, Rectangle window) =
            (context.Extent.Height, int.Min(context.Extent.Width, context.Extent.Height), new Vector2(context.Extent.Width, context.Extent.Height) / 2f, frame.Window);
        uint field = CoordinateHash.Field(NoiseStream.ParticleField, state.Seed, 0u);
        (uint presence, uint tone, uint across, uint down) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u));
        (NoiseSampler basis, RampTable table, double t) = (state.Basis.Sampler, state.Colors.Tabulate(context.Working), state.Timing.At(context));
        (int layers, float depth, float density, float turbulence, float eddy, float evolution) =
            (state.Layers, state.Depth, state.Density, state.Turbulence, state.Eddy, state.Evolution);
        (float size, float defocus, float shutter, float fade, float incandescence, float focus) =
            (state.Size, state.Defocus, state.Shutter, state.Fade, state.Incandescence, float.Pow(state.Depth, -(float)state.Focus));
        Vector2 heading = float.SinCos(state.Heading) switch { var (sin, cos) => state.Speed * new Vector2(cos, -sin) };
        Func<float, Vector4> color = state.Incandescence == Mix.MinValue
            ? draw => table.Sample(draw)
            : Emission.Span((context.Working, state.Cool, state.Hot)) switch {
                var span => draw => table.Sample(draw) switch { var stop => new Vector4(stop.AsVector3() * Vector3.Lerp(Vector3.One, span.Sample(draw), incandescence), stop.W) },
            };
        float footprint = profile.Reach(float.Max(side * ((size / 2f) + defocus), profile.Floor)) + 1f;
        (int columns, int rows) = ((window.Width + Tile - 1) / Tile, (window.Height + Tile - 1) / Tile);
        int tiles = columns * rows;
        List<Particle> particles = [];
        for (int layer = 0; layer < layers; layer++) {
            float scale = float.Pow(depth, -(layer + 0.5f) / layers);
            (double cell, Vector2 velocity) = ((float)state.Spacing * (double)scale, heading * scale);
            float grown = (footprint / side) + (float.Abs(state.Speed) * scale * shutter) + (2f * turbulence);
            (long left, long top) = ((long)Math.Floor((((window.Left - middle.X) / side) - grown - (velocity.X * t)) / cell), (long)Math.Floor((((window.Top - middle.Y) / side) - grown - (velocity.Y * t)) / cell));
            (long wide, long tall) = (
                (long)Math.Floor((((window.Right - middle.X) / side) + grown - (velocity.X * t)) / cell) - left + 1,
                (long)Math.Floor((((window.Bottom - middle.Y) / side) + grown - (velocity.Y * t)) / cell) - top + 1);
            for (long n = 0; n < wide * tall; n++) {
                (long i, long j) = (left + (n % wide), top + (n / wide));
                Vector4 point = new(i & CellBits, j & CellBits, layer, 0f);
                Vector4 a = NoiseFunctions.White(point, presence);
                if (a.X >= density)
                    continue;
                (double x, double y, float k) = ((i + (double)a.Y) * cell, (j + (double)a.Z) * cell, float.Pow(depth, -(layer + a.W) / layers));
                (Vector2 head, Vector2 tail) = (middle + (side * Moved(x, y, velocity, k, t)), middle + (side * Moved(x, y, velocity, k, t - shutter)));
                float core = size * k / 2f * side;
                Particle drawn = new(tail, head, float.Max(core + (defocus * float.Abs(k - focus) * side), profile.Floor), Vector3.Zero, 0f);
                (Vector2 low, Vector2 high) = drawn.Box(profile);
                if (low.X < window.Right && high.X > window.Left && low.Y < window.Bottom && high.Y > window.Top)
                    particles.Add(drawn.Lit(profile, fade, profile.Area(core), color(NoiseFunctions.White(point, tone).X)));
            }
        }
        int[] starts = new int[tiles + 1];
        foreach (Particle drawn in CollectionsMarshal.AsSpan(particles)) {
            Rectangle covered = Tiles(drawn);
            for (int n = 0; n < covered.Width * covered.Height; n++)
                starts[1 + ((covered.Top + (n / covered.Width)) * columns) + covered.Left + (n % covered.Width)]++;
        }
        for (int at = 1; at < starts.Length; at++)
            starts[at] += starts[at - 1];
        (int[] cursor, int[] lists) = (starts.AsSpan(0, tiles).ToArray(), new int[starts[tiles]]);
        for (int index = 0; index < particles.Count; index++) {
            Rectangle covered = Tiles(particles[index]);
            for (int n = 0; n < covered.Width * covered.Height; n++)
                lists[cursor[((covered.Top + (n / covered.Width)) * columns) + covered.Left + (n % covered.Width)]++] = index;
        }
        return new PixelPass.Pointwise(state.Layer.Kernel((row, column, line) => {
            ReadOnlySpan<Particle> drawn = CollectionsMarshal.AsSpan(particles);
            int y = height - 1 - line;
            int band = (y - window.Top) / Tile * columns;
            for (int i = 0; i < row.Length; i++) {
                int at = band + ((column + i - window.Left) / Tile);
                (Vector2 pixel, Vector3 light, float clear) = (new Vector2(column + i + 0.5f, y + 0.5f), Vector3.Zero, 1f);
                foreach (int index in lists.AsSpan(starts[at], starts[at + 1] - starts[at])) {
                    Vector4 weight = drawn[index].Weight(profile, pixel, fade);
                    (light, clear) = (light + (drawn[index].Light * weight.AsVector3()), clear * (1f - (weight.W * drawn[index].Opacity)));
                }
                row[i] = clear < 1f ? new Vector4(light / (1f - clear), 1f - clear) : Vector4.Zero;
            }
        })).Run(frame, progress);

        Vector2 Moved(double x, double y, Vector2 velocity, float k, double tau) {
            Vector2 rest = new((float)(x + (velocity.X * tau)), (float)(y + (velocity.Y * tau)));
            Vector4 sample = new(rest / eddy, evolution * (float)tau, 0f);
            return rest + (turbulence * k * new Vector2((2f * basis.Sample(sample, across)) - 1f, (2f * basis.Sample(sample, down)) - 1f));
        }

        Rectangle Tiles(Particle drawn) =>
            drawn.Box(profile) switch {
                var (low, high) => Rectangle.FromLTRB(
                    (int)float.Clamp(float.Floor((low.X - window.Left) / Tile), 0f, columns - 1), (int)float.Clamp(float.Floor((low.Y - window.Top) / Tile), 0f, rows - 1),
                    (int)float.Clamp(float.Floor((high.X - window.Left) / Tile), 0f, columns - 1) + 1, (int)float.Clamp(float.Floor((high.Y - window.Top) / Tile), 0f, rows - 1) + 1),
            };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParticleFieldParameter : IStateParameter<ParticleField> {
    private static readonly (StateParameter<ParticleField> Clock, StateParameter<ParticleField> Pace) Time =
        Timing.Kinds(Lens<ParticleField, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    private static readonly Lens<ParticleField, Iris> Diaphragm =
        Lens<ParticleField, Iris>.New(static particles => particles.Iris, static iris => particles => particles with { Iris = iris });
    private static readonly (StateParameter<ParticleField> Blades, StateParameter<ParticleField> Rotation, StateParameter<ParticleField> Roundness, StateParameter<ParticleField> Obstruction, StateParameter<ParticleField> Squeeze)
        Outline = Iris.Kinds(Diaphragm);
    private static readonly (StateParameter<ParticleField> Mode, StateParameter<ParticleField> Exposure) Layer =
        GeneratedLayer.Kinds(Lens<ParticleField, GeneratedLayer>.New(static particles => particles.Layer, static layer => particles => particles with { Layer = layer }));
    private static readonly Presentation<AxisFraction, float> Share = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<Mix, float> Blend = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<ShortSideLength, float> Footprint = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.05f) };

    public static readonly ParticleFieldParameter Spacing = new("spacing", new StateParameter<ParticleField>.Bounded<ParticleSpacing, float, InvalidGenerator>(
        Lens<ParticleField, ParticleSpacing>.New(static particles => particles.Spacing, static spacing => particles => particles with { Spacing = spacing }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 0.5f), Scale = TrackScale.Log }));
    public static readonly ParticleFieldParameter Density = new("density", new StateParameter<ParticleField>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<ParticleField, AxisFraction>.New(static particles => particles.Density, static density => particles => particles with { Density = density }), Share));
    public static readonly ParticleFieldParameter Layers = new("layers", new StateParameter<ParticleField>.Bounded<ParticleLayers, int, InvalidGenerator>(
        Lens<ParticleField, ParticleLayers>.New(static particles => particles.Layers, static layers => particles => particles with { Layers = layers }), new() { Step = 1 }));
    public static readonly ParticleFieldParameter Heading = new("heading", new StateParameter<ParticleField>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<ParticleField, SignedAngle>.New(static particles => particles.Heading, static heading => particles => particles with { Heading = heading }),
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral }));
    public static readonly ParticleFieldParameter Speed = new("speed", new StateParameter<ParticleField>.Bounded<Drift, float, InvalidGenerator>(
        Lens<ParticleField, Drift>.New(static particles => particles.Speed, static speed => particles => particles with { Speed = speed }),
        Drift.Presentation with { Soft = (0f, 2f) }));
    public static readonly ParticleFieldParameter Basis = new("basis", new StateParameter<ParticleField>.Record<NoiseBasis>(
        Lens<ParticleField, NoiseBasis>.New(static state => state.Basis, static basis => state => state with { Basis = basis })));
    public static readonly ParticleFieldParameter Turbulence = new("turbulence", new StateParameter<ParticleField>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ParticleField, ShortSideLength>.New(static particles => particles.Turbulence, static turbulence => particles => particles with { Turbulence = turbulence }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.2f) }));
    public static readonly ParticleFieldParameter Eddy = new("eddy", new StateParameter<ParticleField>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<ParticleField, ShortSideExtent>.New(static particles => particles.Eddy, static eddy => particles => particles with { Eddy = eddy }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log }));
    public static readonly ParticleFieldParameter Evolution = new("evolution", new StateParameter<ParticleField>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<ParticleField, Frequency>.New(static particles => particles.Evolution, static evolution => particles => particles with { Evolution = evolution }),
        Frequency.Presentation with { Soft = (-4f, 4f) }));
    public static readonly ParticleFieldParameter Depth = new("depth", new StateParameter<ParticleField>.Bounded<DepthRatio, float, InvalidGenerator>(
        Lens<ParticleField, DepthRatio>.New(static particles => particles.Depth, static depth => particles => particles with { Depth = depth }), new()));
    public static readonly ParticleFieldParameter Focus = new("focus", new StateParameter<ParticleField>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<ParticleField, AxisFraction>.New(static particles => particles.Focus, static focus => particles => particles with { Focus = focus }), Share));
    public static readonly ParticleFieldParameter Defocus = new("defocus", new StateParameter<ParticleField>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ParticleField, ShortSideLength>.New(static particles => particles.Defocus, static defocus => particles => particles with { Defocus = defocus }), Footprint));
    public static readonly ParticleFieldParameter Shape = new("shape", new StateParameter<ParticleField>.Choice<ParticleShape, InvalidGenerator>(
        Lens<ParticleField, ParticleShape>.New(static particles => particles.Shape, static shape => particles => particles with { Shape = shape })));
    public static readonly ParticleFieldParameter Size = new("size", new StateParameter<ParticleField>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<ParticleField, ShortSideLength>.New(static particles => particles.Size, static size => particles => particles with { Size = size }), Footprint));
    public static readonly ParticleFieldParameter Blades = new("blades", Outline.Blades);
    public static readonly ParticleFieldParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly ParticleFieldParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly ParticleFieldParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly ParticleFieldParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly ParticleFieldParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));
    public static readonly ParticleFieldParameter Shutter = new("shutter", new StateParameter<ParticleField>.Bounded<ShutterTime, float, InvalidGenerator>(
        Lens<ParticleField, ShutterTime>.New(static particles => particles.Shutter, static shutter => particles => particles with { Shutter = shutter }),
        ShutterTime.Presentation with { Soft = (0f, 1f / 24f) }));
    public static readonly ParticleFieldParameter Fade = new("fade", new StateParameter<ParticleField>.Bounded<Mix, float, InvalidGrade>(
        Lens<ParticleField, Mix>.New(static particles => particles.Fade, static fade => particles => particles with { Fade = fade }), Blend));
    public static readonly ParticleFieldParameter Colors = new("colors", new StateParameter<ParticleField>.Gradient(
        Lens<ParticleField, Ramp>.New(static particles => particles.Colors, static colors => particles => particles with { Colors = colors })));
    public static readonly ParticleFieldParameter Cool = new("cool", new StateParameter<ParticleField>.Bounded<ColorTemperature, double, InvalidColor>(
        Lens<ParticleField, ColorTemperature>.New(static particles => particles.Cool, static cool => particles => particles with { Cool = cool }), Emission.Presentation));
    public static readonly ParticleFieldParameter Hot = new("hot", new StateParameter<ParticleField>.Bounded<ColorTemperature, double, InvalidColor>(
        Lens<ParticleField, ColorTemperature>.New(static particles => particles.Hot, static hot => particles => particles with { Hot = hot }), Emission.Presentation));
    public static readonly ParticleFieldParameter Incandescence = new("incandescence", new StateParameter<ParticleField>.Bounded<Mix, float, InvalidGrade>(
        Lens<ParticleField, Mix>.New(static particles => particles.Incandescence, static incandescence => particles => particles with { Incandescence = incandescence }), Blend));
    public static readonly ParticleFieldParameter Mode = new("mode", Layer.Mode);
    public static readonly ParticleFieldParameter Exposure = new("exposure", Layer.Exposure);
    public static readonly ParticleFieldParameter Seed = new("seed", new StateParameter<ParticleField>.Bounded<Seed, int, InvalidGenerator>(
        Lens<ParticleField, Seed>.New(static particles => particles.Seed, static seed => particles => particles with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly ParticleFieldParameter Clock = new("clock", Time.Clock);
    public static readonly ParticleFieldParameter Pace = new("pace", Time.Pace);

    public StateParameter<ParticleField> Kind { get; }
}

file readonly record struct Particle(Vector2 Tail, Vector2 Head, float Radius, Vector3 Light, float Opacity) {
    public (Vector2 Low, Vector2 High) Box<TProfile>(TProfile profile) where TProfile : struct, IParticleProfile<TProfile> =>
        new Vector2(profile.Reach(Radius) + 1f) switch { var grown => (Vector2.Min(Tail, Head) - grown, Vector2.Max(Tail, Head) + grown) };

    public Vector4 Weight<TProfile>(TProfile profile, Vector2 pixel, float fade) where TProfile : struct, IParticleProfile<TProfile> =>
        (Head - Tail) switch {
            var sweep => (sweep == Vector2.Zero ? 1f : Easing.Saturate(Vector2.Dot(pixel - Tail, sweep) / sweep.LengthSquared())) switch {
                var u => profile.Coverage(pixel - Tail - (u * sweep), Radius) * float.Lerp(1f, u, fade),
            },
        };

    public Particle Lit<TProfile>(TProfile profile, float fade, float light, Vector4 color) where TProfile : struct, IParticleProfile<TProfile> {
        (Vector2 low, Vector2 high) = Box(profile);
        (int left, int top) = ((int)float.Ceiling(low.X - 0.5f), (int)float.Ceiling(low.Y - 0.5f));
        (int wide, int tall) = ((int)float.Floor(high.X - 0.5f) - left + 1, (int)float.Floor(high.Y - 0.5f) - top + 1);
        Vector3 mass = Vector3.Zero;
        for (int n = 0; n < wide * tall; n++)
            mass += Weight(profile, new Vector2(left + (n % wide) + 0.5f, top + (n / wide) + 0.5f), fade).AsVector3();
        return this with {
            Light = Vector3.ConditionalSelect(Vector3.GreaterThan(mass, Vector3.Zero), color.AsVector3() * (color.W * light) / mass, Vector3.Zero),
            Opacity = color.W,
        };
    }
}
