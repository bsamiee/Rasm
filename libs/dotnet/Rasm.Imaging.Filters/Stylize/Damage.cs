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

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct MarkCount : IMinMaxValue<MarkCount> {
    public static MarkCount MinValue { get; } = new(1f);
    public static MarkCount MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

public sealed record DamageMark(Likelihood Likelihood, Hold Hold, Likelihood White) : IStateRecord<DamageMark, DamageMarkParameter, InvalidStylize> {
    public static DamageMark Default { get; } = new(Likelihood.Off, Hold.Create(6f / 24f), Likelihood.Off);
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DamageMarkParameter : IStateParameter<DamageMark> {
    public static readonly DamageMarkParameter Likelihood = new("likelihood", new StateParameter<DamageMark>.Bounded<Likelihood, float, InvalidStylize>(
        Lens<DamageMark, Likelihood>.New(static mark => mark.Likelihood, static likelihood => mark => mark with { Likelihood = likelihood }), new()));
    public static readonly DamageMarkParameter Hold = new("hold", new StateParameter<DamageMark>.Bounded<Hold, float, InvalidGenerator>(
        Lens<DamageMark, Hold>.New(static mark => mark.Hold, static hold => mark => mark with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly DamageMarkParameter White = new("white", new StateParameter<DamageMark>.Bounded<Likelihood, float, InvalidStylize>(
        Lens<DamageMark, Likelihood>.New(static mark => mark.White, static white => mark => mark with { White = white }), new()));

    public StateParameter<DamageMark> Kind { get; }
}

public sealed record FilmDamage(
    DamageMark Specks, PointCells SpeckCells, DamageMark Scratches, MarkCount ScratchCount, ShortSideLength ScratchWidth, Frequency ScratchRate,
    DamageMark Hairs, MarkCount HairCount, ShortSideLength HairWidth, ShortSideExtent HairLength, NoiseDistortion HairCurl, Frequency HairRate,
    ShortSideLength WeaveX, ShortSideLength WeaveY, Frequency WeaveRate, Seed Seed, Timing Timing)
    : IStateRecord<FilmDamage, FilmDamageParameter, InvalidStylize>, IPixelStage<FilmDamage> {
    public static FilmDamage Default { get; } = new(
        DamageMark.Default with { White = Likelihood.Create(0.5f) },
        new(ParticleSpacing.Create(0.02f * ReferenceFrame.GreaterSide), AxisFraction.Create(0.02f), AxisFraction.Create(2f * 0.2f), AxisFraction.MaxValue, ParticleSpread.Neutral, AxisFraction.Create(0.3f)),
        DamageMark.Default with { White = Likelihood.Create(0.7f) }, MarkCount.Create(3f), ShortSideLength.Create(0.0008f * ReferenceFrame.GreaterSide), Frequency.Create(0.05f * 24f),
        DamageMark.Default, MarkCount.Create(1f), ShortSideLength.Create(0.0012f * ReferenceFrame.GreaterSide), ShortSideExtent.Create(0.08f * ReferenceFrame.GreaterSide), NoiseDistortion.Create(1f),
        Frequency.Create(0.1f * 24f), ShortSideLength.Neutral, ShortSideLength.Neutral, Frequency.Create(5f * 0.05f * 0.5f * 24f), Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmDamage state, PassContext context) {
        (Clock clock, PixelExtent extent) = (state.Timing.At(context), context.Extent);
        Seq<(DamageMark Mark, bool Held, Func<uint, Func<Vector2, (float Key, float Tone)>> Field)> marks = [
            (state.Specks, true, field => point => state.SpeckCells.Disc(point, extent.ShortSide, field)),
            (state.Scratches, false, field => Scratch(state, extent, clock, field)),
            (state.Hairs, false, field => Hair(state, extent, clock, field))];
        (Func<Vector2, (float Key, float Tone)> Field, Likelihood White)[] shown = [.. marks
            .Map((mark, index) => (mark, Period: mark.Mark.Hold.Period(clock), Index: (uint)index))
            .Filter(drawn => NoiseFunctions.White(Vector4.Zero, Shot(drawn.Period, drawn.Index)).X < drawn.mark.Mark.Likelihood)
            .Map(drawn => (drawn.mark.Field(Shot(drawn.mark.Held ? drawn.Period : 0u, drawn.Index)), drawn.mark.Mark.White))];
        Option<PixelPass> composite = shown is [] ? None : Some<PixelPass>(Composite(shown, extent));
        return (state.WeaveX, state.WeaveY) == (ShortSideLength.Neutral, ShortSideLength.Neutral) ? composite : Weave(state, extent, clock, (uint)marks.Count) switch {
            var map => Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                composite.Map(pass => pass.Run(frame, new Progress<int>())).IfNone(Fin.Succ(unit)).Bind(_ => map.Apply(frame, progress)))),
        };

        uint Shot(uint term, uint index) => CoordinateHash.Branch(CoordinateHash.Field(NoiseStream.FilmDamage, state.Seed, term), index);
    }

    private static PixelPass.Pointwise Composite((Func<Vector2, (float Key, float Tone)> Field, Likelihood White)[] marks, PixelExtent extent) {
        (Vector2 center, float side) = (new Vector2(extent.Width, extent.Height) / 2f, extent.ShortSide);
        return new((row, column, line) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> key = SpanOwner<float>.Allocate(row.Length);
            foreach ((Func<Vector2, (float Key, float Tone)> field, Likelihood white) in marks) {
                for (int x = 0; x < row.Length; x++) {
                    (key.Span[x], float tone) = field((new Vector2(column + x + 0.5f, extent.Height - line - 0.5f) - center) / side);
                    layer.Span[x] = new Vector4(new Vector3(tone < white ? row[x].W : 0f), row[x].W);
                }
                BlendingMode.Mix.Mixed(row, layer.Span, key.Span);
                layer.Span.CopyTo(row);
            }
        });
    }

    private static CoordinateMap.Analytic Weave(FilmDamage state, PixelExtent extent, Clock clock, uint axes) {
        (uint field, Vector4 term) = (CoordinateHash.Field(NoiseStream.FilmDamage, state.Seed, 0u), new(clock * state.WeaveRate, 0f, 0f, 0f));
        Vector2 shift = new Vector2(state.WeaveX.Pixels(extent), state.WeaveY.Pixels(extent))
            * new Vector2(NoiseDimensions.One.Fbm(term, Octaves.Default, CoordinateHash.Branch(field, axes)), NoiseDimensions.One.Fbm(term, Octaves.Default, CoordinateHash.Branch(field, axes + 1u)));
        return new((points, _, _) => { foreach (ref Vector2 point in points) point -= shift; }, Sampling.Linear, WrapMode.Clamp);
    }

    private static Func<Vector2, (float Key, float Tone)> Scratch(FilmDamage state, PixelExtent extent, Clock clock, uint field) {
        (float cells, float side, float lane, float half) = (state.ScratchCount * extent.ShortSide / extent.Width, extent.ShortSide, clock * state.ScratchRate, state.ScratchWidth.Pixels(extent) / 2f);
        (uint sites, uint tones) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        return point => NoiseDimensions.Two.F1(new Vector4(point.X * cells, lane, 0f, 0f), Octaves.Plain, Cellular.Default, AxisFraction.MinValue, sites).Site switch {
            var site => (ShapeEdge.Coverage((float.Abs(point.X - (site.X / cells)) * side) - half, 1f), NoiseFunctions.White(site, tones).X),
        };
    }

    private static Func<Vector2, (float Key, float Tone)> Hair(FilmDamage state, PixelExtent extent, Clock clock, uint field) {
        (float length, float pixels, float lane, float half) = (state.HairLength, state.HairLength * extent.ShortSide, clock * state.HairRate, state.HairWidth.Pixels(extent) / 2f);
        float share = state.HairCount * pixels * pixels / (extent.Width * (float)extent.Height);
        (uint across, uint down, uint sites, uint tones, uint strands) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        return point => {
            Vector4 strand = new(point / length, 0f, 0f);
            Vector4 bent = new(strand.AsVector2() + (state.HairCurl * new Vector2(NoiseDimensions.Two.Fbm(strand, Octaves.Default, across), NoiseDimensions.Two.Fbm(strand, Octaves.Default, down))), lane, 0f);
            Vector4 cell = NoiseFunctions.White(NoiseDimensions.Three.F1(bent, Octaves.Plain, Cellular.Default, AxisFraction.MinValue, sites).Site, tones);
            float edge = ShapeEdge.Coverage((NoiseDimensions.Three.EdgeDistance(bent, Octaves.Plain, Cellular.Default.Randomness, sites) * pixels) - half, 1f);
            float open = (float)RampInterpolation.Ease.Weight(Easing.Saturate((0.5f + (0.5f * NoiseDimensions.Two.Fbm(strand, Octaves.Default, strands)) - 0.45f) / 0.1f));
            return (cell.X < share ? edge * open : 0f, cell.Y);
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmDamageParameter : IStateParameter<FilmDamage> {
    private static readonly Presentation<ShortSideLength, float> Width = ShortSideLength.Presentation with { Soft = (0f, 0.005f * ReferenceFrame.GreaterSide), Decimals = 4 };
    private static readonly Presentation<Frequency, float> Rate = Frequency.Presentation with { Soft = (0f, 24f) };
    private static readonly Presentation<ShortSideLength, float> Sway = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };

    public static readonly FilmDamageParameter Specks = new("specks", new StateParameter<FilmDamage>.Record<DamageMark>(
        Lens<FilmDamage, DamageMark>.New(static damage => damage.Specks, static specks => damage => damage with { Specks = specks })));
    public static readonly FilmDamageParameter SpeckCells = new("specks-cells", new StateParameter<FilmDamage>.Record<PointCells>(
        Lens<FilmDamage, PointCells>.New(static damage => damage.SpeckCells, static cells => damage => damage with { SpeckCells = cells })));
    public static readonly FilmDamageParameter Scratches = new("scratches", new StateParameter<FilmDamage>.Record<DamageMark>(
        Lens<FilmDamage, DamageMark>.New(static damage => damage.Scratches, static scratches => damage => damage with { Scratches = scratches })));
    public static readonly FilmDamageParameter ScratchCount = new("scratches-count", new StateParameter<FilmDamage>.Bounded<MarkCount, float, InvalidStylize>(
        Lens<FilmDamage, MarkCount>.New(static damage => damage.ScratchCount, static count => damage => damage with { ScratchCount = count }), new() { Soft = (1f, 20f) }));
    public static readonly FilmDamageParameter ScratchWidth = new("scratches-width", new StateParameter<FilmDamage>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<FilmDamage, ShortSideLength>.New(static damage => damage.ScratchWidth, static width => damage => damage with { ScratchWidth = width }), Width));
    public static readonly FilmDamageParameter ScratchRate = new("scratches-rate", new StateParameter<FilmDamage>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<FilmDamage, Frequency>.New(static damage => damage.ScratchRate, static rate => damage => damage with { ScratchRate = rate }), Rate));
    public static readonly FilmDamageParameter Hairs = new("hairs", new StateParameter<FilmDamage>.Record<DamageMark>(
        Lens<FilmDamage, DamageMark>.New(static damage => damage.Hairs, static hairs => damage => damage with { Hairs = hairs })));
    public static readonly FilmDamageParameter HairCount = new("hairs-count", new StateParameter<FilmDamage>.Bounded<MarkCount, float, InvalidStylize>(
        Lens<FilmDamage, MarkCount>.New(static damage => damage.HairCount, static count => damage => damage with { HairCount = count }), new() { Soft = (1f, 10f) }));
    public static readonly FilmDamageParameter HairWidth = new("hairs-width", new StateParameter<FilmDamage>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<FilmDamage, ShortSideLength>.New(static damage => damage.HairWidth, static width => damage => damage with { HairWidth = width }), Width));
    public static readonly FilmDamageParameter HairLength = new("hairs-length", new StateParameter<FilmDamage>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<FilmDamage, ShortSideExtent>.New(static damage => damage.HairLength, static length => damage => damage with { HairLength = length }),
        new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f * ReferenceFrame.GreaterSide, 0.3f * ReferenceFrame.GreaterSide) }));
    public static readonly FilmDamageParameter HairCurl = new("hairs-curl", new StateParameter<FilmDamage>.Bounded<NoiseDistortion, float, InvalidGenerator>(
        Lens<FilmDamage, NoiseDistortion>.New(static damage => damage.HairCurl, static curl => damage => damage with { HairCurl = curl }), new() { Soft = (0f, 4f) }));
    public static readonly FilmDamageParameter HairRate = new("hairs-rate", new StateParameter<FilmDamage>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<FilmDamage, Frequency>.New(static damage => damage.HairRate, static rate => damage => damage with { HairRate = rate }), Rate));
    public static readonly FilmDamageParameter WeaveX = new("weave-x", new StateParameter<FilmDamage>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveX, static weave => damage => damage with { WeaveX = weave }), Sway));
    public static readonly FilmDamageParameter WeaveY = new("weave-y", new StateParameter<FilmDamage>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveY, static weave => damage => damage with { WeaveY = weave }), Sway));
    public static readonly FilmDamageParameter WeaveRate = new("weave-rate", new StateParameter<FilmDamage>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<FilmDamage, Frequency>.New(static damage => damage.WeaveRate, static rate => damage => damage with { WeaveRate = rate }), Frequency.Presentation with { Soft = (0.1f, 20f), Scale = TrackScale.Log }));
    public static readonly FilmDamageParameter Seed = new("seed", new StateParameter<FilmDamage>.Bounded<Seed, int, InvalidGenerator>(
        Lens<FilmDamage, Seed>.New(static damage => damage.Seed, static seed => damage => damage with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmDamageParameter Timing = new("timing", new StateParameter<FilmDamage>.Record<Timing>(
        Lens<FilmDamage, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));

    public StateParameter<FilmDamage> Kind { get; }
}

public sealed record FilmFlicker(Exposure Depth, Frequency Rate, Seed Seed, Timing Timing)
    : IStateRecord<FilmFlicker, FilmFlickerParameter, InvalidStylize>, IPixelStage<FilmFlicker> {
    public static FilmFlicker Default { get; } = new(Exposure.Neutral, Frequency.Create(5f * 0.5f * 24f), Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmFlicker state, PassContext context) {
        Vector4 gain = new(new Vector3(float.Exp2(state.Depth * NoiseDimensions.One.Fbm(
            new Vector4(state.Timing.At(context) * state.Rate, 0f, 0f, 0f), Octaves.Default, CoordinateHash.Field(NoiseStream.FilmFlicker, state.Seed, 0u)))), 1f);
        return state.Depth == Exposure.Neutral ? None : Some<PixelPass>(new PixelPass.Color(row => { foreach (ref Vector4 pixel in row) pixel *= gain; }));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmFlickerParameter : IStateParameter<FilmFlicker> {
    public static readonly FilmFlickerParameter Depth = new("depth", new StateParameter<FilmFlicker>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<FilmFlicker, Exposure>.New(static flicker => flicker.Depth, static depth => flicker => flicker with { Depth = depth }), Exposure.Presentation with { Soft = (0f, 1f) }));
    public static readonly FilmFlickerParameter Rate = new("rate", new StateParameter<FilmFlicker>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<FilmFlicker, Frequency>.New(static flicker => flicker.Rate, static rate => flicker => flicker with { Rate = rate }),
        Frequency.Presentation with { Soft = (1f, 60f), Scale = TrackScale.Log }));
    public static readonly FilmFlickerParameter Seed = new("seed", new StateParameter<FilmFlicker>.Bounded<Seed, int, InvalidGenerator>(
        Lens<FilmFlicker, Seed>.New(static flicker => flicker.Seed, static seed => flicker => flicker with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmFlickerParameter Timing = new("timing", new StateParameter<FilmFlicker>.Record<Timing>(
        Lens<FilmFlicker, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));

    public StateParameter<FilmFlicker> Kind { get; }
}
