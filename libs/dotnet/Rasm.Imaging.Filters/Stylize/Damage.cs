using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct Likelihood : IMinMaxValue<Likelihood> {
    public static Likelihood MinValue => Off;
    public static Likelihood MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct DamageDensity : IMinMaxValue<DamageDensity> {
    public static DamageDensity MinValue { get; } = new(-0.5f);
    public static DamageDensity MaxValue { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DamageInk {
    public static readonly DamageInk Light = new("light", BlendingMode.Add, Swatch.White);
    public static readonly DamageInk Dark = new("dark", BlendingMode.Multiply, Swatch.Black);

    public BlendingMode Mode { get; }
    public Swatch Layer { get; }
}

public sealed record DamageLayer(Likelihood Likelihood, Hold Hold, DamageDensity Density, ShortSideExtent Size, DamageInk Ink);

public sealed record FilmDamage(
    DamageLayer Sparkle, DamageLayer Dirt, DamageLayer Blotches, DamageLayer Scratches, DamageLayer Hairs,
    ShortSideLength WeaveX, ShortSideLength WeaveY, Frequency WeaveRate, Seed Seed, Timing Timing)
    : IStateRecord<FilmDamage, FilmDamageParameter, InvalidStylize>, IPixelStage<FilmDamage> {
    public static FilmDamage Default { get; } = new(
        new(Likelihood.Off, Hold.Damage, DamageDensity.Neutral, ShortSideExtent.Create(1f / 5f), DamageInk.Light),
        new(Likelihood.Off, Hold.Damage, DamageDensity.Neutral, ShortSideExtent.Create(1f / 5f), DamageInk.Dark),
        new(Likelihood.Off, Hold.Damage, DamageDensity.Neutral, ShortSideExtent.Create(1f / (6f * 2f)), DamageInk.Light),
        new(Likelihood.Off, Hold.Damage, DamageDensity.Neutral, ShortSideExtent.Create(1f / (6.5f * 3f * 2f)), DamageInk.Light),
        new(Likelihood.Off, Hold.Damage, DamageDensity.Neutral, ShortSideExtent.Create(1f / (1.5f * 1f * 2f)), DamageInk.Dark),
        ShortSideLength.Neutral, ShortSideLength.Neutral, Frequency.Weave, Seed.MinValue, Timing.Standard);

    public static Option<PixelPass> Pass(FilmDamage state, PassContext context) {
        Clock clock = state.Timing.At(context);
        Seq<(DamageLayer Layer, Func<DamageLayer, uint, Clock, Func<Vector2, float>> Field)> members =
            [(state.Sparkle, Speckle), (state.Dirt, Speckle), (state.Blotches, Blotch), (state.Scratches, Scratch), (state.Hairs, Hair)];
        (Func<Vector2, float> Matte, DamageInk Ink)[] layers = [.. members
            .Map((member, index) => (member.Layer, member.Field, Draw: CoordinateHash.Branch(CoordinateHash.Field(NoiseStream.FilmDamage, state.Seed, member.Layer.Hold.Period(clock)), (uint)index)))
            .Filter(static drawn => NoiseFunctions.White(Vector4.Zero, CoordinateHash.Branch(drawn.Draw, 0u)).X < drawn.Layer.Likelihood)
            .Map(drawn => (drawn.Field(drawn.Layer, drawn.Draw, clock), drawn.Layer.Ink))];
        Option<CoordinateMap> weave = state.WeaveX == ShortSideLength.Neutral && state.WeaveY == ShortSideLength.Neutral
            ? None
            : Some<CoordinateMap>(Weave(state, context.Extent, clock, (uint)members.Count));
        return weave.Match(
            Some: map => Some<PixelPass>(layers is []
                ? new PixelPass.Frame(map.Apply)
                : new PixelPass.Frame((frame, progress) => Composite(layers, context.Extent).Run(frame, new Progress<int>()).Bind(_ => map.Apply(frame, progress)))),
            None: () => layers is [] ? None : Some<PixelPass>(Composite(layers, context.Extent)));
    }

    private static PixelPass.Pointwise Composite((Func<Vector2, float> Matte, DamageInk Ink)[] layers, PixelExtent extent) {
        Vector2 center = new Vector2(extent.Width, extent.Height) / 2f;
        float side = int.Min(extent.Width, extent.Height);
        return new PixelPass.Pointwise((row, column, line) => {
            using SpanOwner<Vector4> ink = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> matte = SpanOwner<float>.Allocate(row.Length);
            foreach ((Func<Vector2, float> field, DamageInk layer) in layers) {
                for (int x = 0; x < row.Length; x++)
                    matte.Span[x] = field((new Vector2(column + x + 0.5f, extent.Height - line - 0.5f) - center) / side);
                ink.Span.Fill(layer.Layer.Display);
                layer.Mode.Mixed(row, ink.Span, matte.Span);
                ink.Span.CopyTo(row);
            }
        });
    }

    private static CoordinateMap.Analytic Weave(FilmDamage state, PixelExtent extent, Clock clock, uint axes) {
        uint field = CoordinateHash.Field(NoiseStream.FilmDamage, state.Seed, 0u);
        Vector4 term = new(clock * state.WeaveRate, 0f, 0f, 0f);
        Vector2 shift = new Vector2(state.WeaveX.Pixels(extent), state.WeaveY.Pixels(extent))
            * new Vector2(NoiseDimensions.One.Fbm(term, Octaves.Standard, CoordinateHash.Branch(field, axes)), NoiseDimensions.One.Fbm(term, Octaves.Standard, CoordinateHash.Branch(field, axes + 1u)));
        return new CoordinateMap.Analytic((points, _, _) => {
            foreach (ref Vector2 point in points)
                point -= shift;
        }, Sampling.Linear, EdgeMode.Clamp);
    }

    private static Func<Vector2, float> Speckle(DamageLayer layer, uint draw, Clock clock) {
        const float floor = 0.6012f;
        const float ceiling = 0.8882f;
        Octaves color = Octaves.Standard with { Detail = FractalDetail.Create(4.4f) };
        GaborFrequency frequency = GaborFrequency.Create(1.1f);
        (uint red, uint green, uint blue, uint gabor) = (CoordinateHash.Branch(draw, 1u), CoordinateHash.Branch(draw, 2u), CoordinateHash.Branch(draw, 3u), CoordinateHash.Branch(draw, 4u));
        return q => {
            Vector4 p = new(q / layer.Size, 0f, 0f);
            Vector3 tone = new(NoiseDimensions.Two.Fbm(p, color, red), NoiseDimensions.Two.Fbm(p, color, green), NoiseDimensions.Two.Fbm(p, color, blue));
            Vector2 g = NoiseDimensions.Three.Gabor(new Vector4(13.6f * (Vector3.One + tone) / 2f, 0f), frequency, AxisFraction.MinValue, GaborOrientation.Standard, gabor);
            return Easing.SmoothStep(Easing.Saturate(((g.Length() * (0.5f + (0.5f * g.Y))) + layer.Density - floor) / (ceiling - floor)));
        };
    }

    private static Func<Vector2, float> Blotch(DamageLayer layer, uint draw, Clock clock) {
        const float density = 0.35f;
        (NoiseDistortion distortion, Octaves octaves) = (NoiseDistortion.Create(0.1f), new Octaves(FractalDetail.Create(4f), AxisFraction.Create(0.562241f), FractalLacunarity.Standard));
        (uint warp, uint blot) = (CoordinateHash.Branch(draw, 1u), CoordinateHash.Branch(draw, 2u));
        return q => Easing.Saturate((density + layer.Density
            - (0.5f + (0.5f * NoiseDimensions.Two.Fbm(NoiseDimensions.Two.Distort(new Vector4(q / layer.Size, 0f, 0f), distortion, warp), octaves, blot)))) / density);
    }

    private static Func<Vector2, float> Scratch(DamageLayer layer, uint draw, Clock clock) {
        const float floor = 0.725f;
        Octaves octaves = Octaves.Standard with { Detail = FractalDetail.Create(4f) };
        uint lines = CoordinateHash.Branch(draw, 1u);
        float drift = layer.Size * NoiseDimensions.One.Fbm(new Vector4(clock / layer.Hold, 0f, 0f, 0f), Octaves.Standard, CoordinateHash.Branch(draw, 2u));
        return q => Easing.Saturate((0.5f + (0.5f * NoiseDimensions.One.Fbm(new Vector4((q.X + drift) / layer.Size, 0f, 0f, 0f), octaves, lines)) + layer.Density - floor) / (1f - floor));
    }

    private static Func<Vector2, float> Hair(DamageLayer layer, uint draw, Clock clock) {
        const float band = 0.28f;
        const float floor = 0.67f;
        (NoiseDistortion strand, NoiseDistortion spread) = (NoiseDistortion.Create(2f), NoiseDistortion.Create(0.6f));
        (FractalOffset offset, Octaves region) = (FractalOffset.Create(-0.5f), new Octaves(FractalDetail.Create(1f), AxisFraction.MaxValue, FractalLacunarity.Create(1f)));
        (uint bend, uint ridge, uint sway, uint patch) = (CoordinateHash.Branch(draw, 1u), CoordinateHash.Branch(draw, 2u), CoordinateHash.Branch(draw, 3u), CoordinateHash.Branch(draw, 4u));
        return q => new Vector4(q / layer.Size, 0f, 0f) switch {
            var p => Easing.Saturate((band - NoiseDimensions.Two.RidgedMultifractal(NoiseDimensions.Two.Distort(p, strand, bend), Octaves.Plain, offset, FractalGain.MinValue, ridge)) / (band - 0.25f))
                * Easing.Saturate((0.5f + (0.5f * NoiseDimensions.Two.Fbm(NoiseDimensions.Two.Distort(8f * p / 3f, spread, sway), region, patch)) + layer.Density - floor) / (0.77f - floor)),
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmDamageParameter : IStateParameter<FilmDamage> {
    private static readonly (StateParameter<FilmDamage> Clock, StateParameter<FilmDamage> Pace) Time =
        Timing.Kinds(Lens<FilmDamage, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    private static readonly Lens<FilmDamage, DamageLayer> SparkleOf =
        Lens<FilmDamage, DamageLayer>.New(static damage => damage.Sparkle, static layer => damage => damage with { Sparkle = layer });
    private static readonly Lens<FilmDamage, DamageLayer> DirtOf =
        Lens<FilmDamage, DamageLayer>.New(static damage => damage.Dirt, static layer => damage => damage with { Dirt = layer });
    private static readonly Lens<FilmDamage, DamageLayer> BlotchesOf =
        Lens<FilmDamage, DamageLayer>.New(static damage => damage.Blotches, static layer => damage => damage with { Blotches = layer });
    private static readonly Lens<FilmDamage, DamageLayer> ScratchesOf =
        Lens<FilmDamage, DamageLayer>.New(static damage => damage.Scratches, static layer => damage => damage with { Scratches = layer });
    private static readonly Lens<FilmDamage, DamageLayer> HairsOf =
        Lens<FilmDamage, DamageLayer>.New(static damage => damage.Hairs, static layer => damage => damage with { Hairs = layer });
    private static readonly Lens<DamageLayer, Likelihood> LikelihoodOf =
        Lens<DamageLayer, Likelihood>.New(static layer => layer.Likelihood, static likelihood => layer => layer with { Likelihood = likelihood });
    private static readonly Lens<DamageLayer, Hold> HoldOf =
        Lens<DamageLayer, Hold>.New(static layer => layer.Hold, static hold => layer => layer with { Hold = hold });
    private static readonly Lens<DamageLayer, DamageDensity> DensityOf =
        Lens<DamageLayer, DamageDensity>.New(static layer => layer.Density, static density => layer => layer with { Density = density });
    private static readonly Lens<DamageLayer, ShortSideExtent> SizeOf =
        Lens<DamageLayer, ShortSideExtent>.New(static layer => layer.Size, static size => layer => layer with { Size = size });
    private static readonly Lens<DamageLayer, DamageInk> InkOf =
        Lens<DamageLayer, DamageInk>.New(static layer => layer.Ink, static ink => layer => layer with { Ink = ink });
    private static readonly Presentation<Likelihood, float> Share = new();
    private static readonly Presentation<DamageDensity, float> Shift = new() { Origin = (float)DamageDensity.Neutral };
    private static readonly Presentation<ShortSideExtent, float> Feature =
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.005f, 0.5f), Scale = TrackScale.Log };
    private static readonly Presentation<ShortSideLength, float> Sway = new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.01f) };

    public static readonly FilmDamageParameter SparkleLikelihood = new("sparkle-likelihood", new StateParameter<FilmDamage>.Bounded<Likelihood, float, InvalidStylize>(lens(SparkleOf, LikelihoodOf), Share));
    public static readonly FilmDamageParameter SparkleHold = new("sparkle-hold", new StateParameter<FilmDamage>.Bounded<Hold, float, InvalidGenerator>(lens(SparkleOf, HoldOf), Hold.Presentation));
    public static readonly FilmDamageParameter SparkleDensity = new("sparkle-density", new StateParameter<FilmDamage>.Bounded<DamageDensity, float, InvalidStylize>(lens(SparkleOf, DensityOf), Shift));
    public static readonly FilmDamageParameter SparkleSize = new("sparkle-size", new StateParameter<FilmDamage>.Bounded<ShortSideExtent, float, InvalidPixelValue>(lens(SparkleOf, SizeOf), Feature));
    public static readonly FilmDamageParameter SparkleInk = new("sparkle-ink", new StateParameter<FilmDamage>.Choice<DamageInk, InvalidStylize>(lens(SparkleOf, InkOf)));
    public static readonly FilmDamageParameter DirtLikelihood = new("dirt-likelihood", new StateParameter<FilmDamage>.Bounded<Likelihood, float, InvalidStylize>(lens(DirtOf, LikelihoodOf), Share));
    public static readonly FilmDamageParameter DirtHold = new("dirt-hold", new StateParameter<FilmDamage>.Bounded<Hold, float, InvalidGenerator>(lens(DirtOf, HoldOf), Hold.Presentation));
    public static readonly FilmDamageParameter DirtDensity = new("dirt-density", new StateParameter<FilmDamage>.Bounded<DamageDensity, float, InvalidStylize>(lens(DirtOf, DensityOf), Shift));
    public static readonly FilmDamageParameter DirtSize = new("dirt-size", new StateParameter<FilmDamage>.Bounded<ShortSideExtent, float, InvalidPixelValue>(lens(DirtOf, SizeOf), Feature));
    public static readonly FilmDamageParameter DirtInk = new("dirt-ink", new StateParameter<FilmDamage>.Choice<DamageInk, InvalidStylize>(lens(DirtOf, InkOf)));
    public static readonly FilmDamageParameter BlotchesLikelihood = new("blotches-likelihood", new StateParameter<FilmDamage>.Bounded<Likelihood, float, InvalidStylize>(lens(BlotchesOf, LikelihoodOf), Share));
    public static readonly FilmDamageParameter BlotchesHold = new("blotches-hold", new StateParameter<FilmDamage>.Bounded<Hold, float, InvalidGenerator>(lens(BlotchesOf, HoldOf), Hold.Presentation));
    public static readonly FilmDamageParameter BlotchesDensity = new("blotches-density", new StateParameter<FilmDamage>.Bounded<DamageDensity, float, InvalidStylize>(lens(BlotchesOf, DensityOf), Shift));
    public static readonly FilmDamageParameter BlotchesSize = new("blotches-size", new StateParameter<FilmDamage>.Bounded<ShortSideExtent, float, InvalidPixelValue>(lens(BlotchesOf, SizeOf), Feature));
    public static readonly FilmDamageParameter BlotchesInk = new("blotches-ink", new StateParameter<FilmDamage>.Choice<DamageInk, InvalidStylize>(lens(BlotchesOf, InkOf)));
    public static readonly FilmDamageParameter ScratchesLikelihood = new("scratches-likelihood", new StateParameter<FilmDamage>.Bounded<Likelihood, float, InvalidStylize>(lens(ScratchesOf, LikelihoodOf), Share));
    public static readonly FilmDamageParameter ScratchesHold = new("scratches-hold", new StateParameter<FilmDamage>.Bounded<Hold, float, InvalidGenerator>(lens(ScratchesOf, HoldOf), Hold.Presentation));
    public static readonly FilmDamageParameter ScratchesDensity = new("scratches-density", new StateParameter<FilmDamage>.Bounded<DamageDensity, float, InvalidStylize>(lens(ScratchesOf, DensityOf), Shift));
    public static readonly FilmDamageParameter ScratchesSize = new("scratches-size", new StateParameter<FilmDamage>.Bounded<ShortSideExtent, float, InvalidPixelValue>(lens(ScratchesOf, SizeOf), Feature));
    public static readonly FilmDamageParameter ScratchesInk = new("scratches-ink", new StateParameter<FilmDamage>.Choice<DamageInk, InvalidStylize>(lens(ScratchesOf, InkOf)));
    public static readonly FilmDamageParameter HairsLikelihood = new("hairs-likelihood", new StateParameter<FilmDamage>.Bounded<Likelihood, float, InvalidStylize>(lens(HairsOf, LikelihoodOf), Share));
    public static readonly FilmDamageParameter HairsHold = new("hairs-hold", new StateParameter<FilmDamage>.Bounded<Hold, float, InvalidGenerator>(lens(HairsOf, HoldOf), Hold.Presentation));
    public static readonly FilmDamageParameter HairsDensity = new("hairs-density", new StateParameter<FilmDamage>.Bounded<DamageDensity, float, InvalidStylize>(lens(HairsOf, DensityOf), Shift));
    public static readonly FilmDamageParameter HairsSize = new("hairs-size", new StateParameter<FilmDamage>.Bounded<ShortSideExtent, float, InvalidPixelValue>(lens(HairsOf, SizeOf), Feature));
    public static readonly FilmDamageParameter HairsInk = new("hairs-ink", new StateParameter<FilmDamage>.Choice<DamageInk, InvalidStylize>(lens(HairsOf, InkOf)));
    public static readonly FilmDamageParameter WeaveX = new("weave-x", new StateParameter<FilmDamage>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveX, static weave => damage => damage with { WeaveX = weave }), Sway));
    public static readonly FilmDamageParameter WeaveY = new("weave-y", new StateParameter<FilmDamage>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveY, static weave => damage => damage with { WeaveY = weave }), Sway));
    public static readonly FilmDamageParameter WeaveRate = new("weave-rate", new StateParameter<FilmDamage>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<FilmDamage, Frequency>.New(static damage => damage.WeaveRate, static rate => damage => damage with { WeaveRate = rate }),
        Frequency.Presentation with { Soft = (0.1f, 20f), Scale = TrackScale.Log }));
    public static readonly FilmDamageParameter Seed = new("seed", new StateParameter<FilmDamage>.Bounded<Seed, int, InvalidGenerator>(
        Lens<FilmDamage, Seed>.New(static damage => damage.Seed, static seed => damage => damage with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmDamageParameter Clock = new("clock", Time.Clock);
    public static readonly FilmDamageParameter Pace = new("pace", Time.Pace);

    public StateParameter<FilmDamage> Kind { get; }
}

public sealed record FilmFlicker(Exposure Depth, Frequency Rate, Seed Seed, Timing Timing)
    : IStateRecord<FilmFlicker, FilmFlickerParameter, InvalidStylize>, IPixelStage<FilmFlicker> {
    public static FilmFlicker Default { get; } = new(Exposure.Neutral, Frequency.Flicker, Seed.MinValue, Timing.Standard);

    public static Option<PixelPass> Pass(FilmFlicker state, PassContext context) =>
        state.Depth == Exposure.Neutral
            ? None
            : new Vector4(new Vector3(float.Exp2(state.Depth * NoiseDimensions.One.Fbm(
                new Vector4(state.Timing.At(context) * state.Rate, 0f, 0f, 0f), Octaves.Standard, CoordinateHash.Field(NoiseStream.FilmFlicker, state.Seed, 0u)))), 1f) switch {
                    var gain => Some<PixelPass>(new PixelPass.Color(row => {
                        foreach (ref Vector4 pixel in row)
                            pixel *= gain;
                    })),
                };
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmFlickerParameter : IStateParameter<FilmFlicker> {
    private static readonly (StateParameter<FilmFlicker> Clock, StateParameter<FilmFlicker> Pace) Time =
        Timing.Kinds(Lens<FilmFlicker, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly FilmFlickerParameter Depth = new("depth", new StateParameter<FilmFlicker>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<FilmFlicker, Exposure>.New(static flicker => flicker.Depth, static depth => flicker => flicker with { Depth = depth }), Exposure.Presentation with { Soft = (0f, 1f) }));
    public static readonly FilmFlickerParameter Rate = new("rate", new StateParameter<FilmFlicker>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<FilmFlicker, Frequency>.New(static flicker => flicker.Rate, static rate => flicker => flicker with { Rate = rate }),
        Frequency.Presentation with { Soft = (1f, 60f), Scale = TrackScale.Log }));
    public static readonly FilmFlickerParameter Seed = new("seed", new StateParameter<FilmFlicker>.Bounded<Seed, int, InvalidGenerator>(
        Lens<FilmFlicker, Seed>.New(static flicker => flicker.Seed, static seed => flicker => flicker with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmFlickerParameter Clock = new("clock", Time.Clock);
    public static readonly FilmFlickerParameter Pace = new("pace", Time.Pace);

    public StateParameter<FilmFlicker> Kind { get; }
}
