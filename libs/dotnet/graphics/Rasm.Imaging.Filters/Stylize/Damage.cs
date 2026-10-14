using System.Numerics;
using PeterO.Numbers;
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

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct HairCount : IMinMaxValue<HairCount> {
    public static HairCount MinValue => Zero;
    public static HairCount MaxValue { get; } = new(float.MaxValue);

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
    public static readonly DamageMarkParameter Likelihood = new("likelihood", StateParameter.Bounded<DamageMark, Likelihood, float, InvalidStylize>(Lens<DamageMark, Likelihood>.New(static mark => mark.Likelihood, static likelihood => mark => mark with { Likelihood = likelihood }), new()));
    public static readonly DamageMarkParameter Hold = new("hold", StateParameter.Bounded<DamageMark, Hold, float, InvalidGenerator>(Lens<DamageMark, Hold>.New(static mark => mark.Hold, static hold => mark => mark with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly DamageMarkParameter White = new("white", StateParameter.Bounded<DamageMark, Likelihood, float, InvalidStylize>(Lens<DamageMark, Likelihood>.New(static mark => mark.White, static white => mark => mark with { White = white }), new()));

    public StateParameter<DamageMark> Kind { get; }
}

public sealed record FilmSpots(DamageMark Mark, PointCells Cells) : IStateRecord<FilmSpots, FilmSpotsParameter, InvalidStylize> {
    public static FilmSpots Default { get; } = new(DamageMark.Default,
        new(ShortSideExtent.Create(0.02f * ReferenceFrame.GreaterSide), AxisFraction.Create(0.02f), AxisFraction.Create(2f * 0.2f), AxisFraction.MaxValue, ParticleSpread.Neutral, AxisFraction.Create(0.3f)));
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmSpotsParameter : IStateParameter<FilmSpots> {
    public static readonly FilmSpotsParameter Mark = new("mark", new StateParameter<FilmSpots>.Record<DamageMark>(
        Lens<FilmSpots, DamageMark>.New(static spots => spots.Mark, static mark => spots => spots with { Mark = mark })));
    public static readonly FilmSpotsParameter Cells = new("cells", new StateParameter<FilmSpots>.Record<PointCells>(
        Lens<FilmSpots, PointCells>.New(static spots => spots.Cells, static cells => spots => spots with { Cells = cells })));

    public StateParameter<FilmSpots> Kind { get; }
}

public sealed record FilmDamage(
    FilmSpots Specks, FilmSpots Dirt, FilmSpots Blotches, DamageMark Scratches, MarkCount ScratchCount, ShortSideLength ScratchWidth, Frequency ScratchRate,
    DamageMark Hairs, HairCount HairCount, ShortSideLength HairWidth, ShortSideExtent HairLength, NoiseDistortion HairCurl, Frequency HairRate,
    ShortSideLength WeaveX, ShortSideLength WeaveY, Frequency WeaveRate, Seed Seed, Timing Timing)
    : IStateRecord<FilmDamage, FilmDamageParameter, InvalidStylize>, IPixelStage<FilmDamage> {
    public static FilmDamage Default { get; } = new(
        FilmSpots.Default with { Mark = DamageMark.Default with { White = Likelihood.Create(0.5f) } }, FilmSpots.Default, FilmSpots.Default,
        DamageMark.Default with { White = Likelihood.Create(0.7f) }, MarkCount.Create(3f), ShortSideLength.Create(0.0008f * ReferenceFrame.GreaterSide), Frequency.Create(0.05f * 24f),
        DamageMark.Default, HairCount.Create(1f), ShortSideLength.Create(0.0012f * ReferenceFrame.GreaterSide), ShortSideExtent.Create(0.08f * ReferenceFrame.GreaterSide), NoiseDistortion.Create(1f),
        Frequency.Create(0.1f * 24f), ShortSideLength.Neutral, ShortSideLength.Neutral, Frequency.Create(5f * 0.05f * 0.5f * 24f), Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmDamage state, PassContext context) {
        (Clock clock, PixelExtent extent) = (state.Timing.At(context), context.Extent);
        long pixelDenominator = 2L * extent.ShortSide;
        Seq<(DamageMark Mark, bool Held, Func<uint, Func<long, long, (float Coverage, float Value)>> Field)> marks = [
            .. Seq(state.Specks, state.Dirt, state.Blotches).Map(spots =>
                (spots.Mark, true, new Func<uint, Func<long, long, (float Coverage, float Value)>>(field => (column, line) =>
                    spots.Cells.Disc((ERational.Create((2L * column) + 1 - extent.Width, pixelDenominator), ERational.Create((2L * line) + 1 - extent.Height, pixelDenominator)), extent.ShortSide, field, tone => tone < spots.Mark.White ? 1f : 0f)))),
            (state.Scratches, false, field => Scratch(state, extent, clock, field)),
            (state.Hairs, false, field => Hair(state, extent, clock, field))];
        Func<long, long, (float Coverage, float Value)>[] shown = [.. marks
            .Map((mark, index) => (mark, Period: mark.Mark.Hold.Period(clock), Index: (uint)index))
            .Filter(drawn => NoiseFunctions.White(Vector4.Zero, Shot(drawn.Period, drawn.Index)).X < drawn.mark.Mark.Likelihood)
            .Map(drawn => drawn.mark.Field(Shot(drawn.mark.Held ? drawn.Period : 0u, drawn.Index)))];
        Option<PixelPass> composite = shown is [] ? None : Some<PixelPass>(Composite(shown, extent));
        return (state.WeaveX, state.WeaveY) == (ShortSideLength.Neutral, ShortSideLength.Neutral) ? composite : Weave(state, extent, clock, (uint)marks.Count) switch {
            var map => Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                composite.Map(pass => pass.Run(frame, new Progress<int>())).IfNone(Fin.Succ(unit)).Bind(_ => map.Apply(frame, progress)))),
        };

        uint Shot(uint term, uint index) => CoordinateHash.Branch(CoordinateHash.Field(NoiseStream.FilmDamage, state.Seed, term), index);
    }

    private static PixelPass.Pointwise Composite(Func<long, long, (float Coverage, float Value)>[] marks, PixelExtent extent) => new((row, column, line) => {
        foreach (Func<long, long, (float Coverage, float Value)> field in marks) {
            for (int x = 0; x < row.Length; x++) {
                (float coverage, float value) = field(column + x, (long)extent.Height - 1 - line);
                row[x] = coverage switch {
                    0f => row[x],
                    1f => new Vector4(new Vector3(value), row[x].W),
                    _ => new Vector4((row[x].AsVector3() * (1f - coverage)) + new Vector3(value), row[x].W),
                };
            }
        }
    });

    private static CoordinateMap.Analytic Weave(FilmDamage state, PixelExtent extent, Clock clock, uint axes) {
        uint field = CoordinateHash.Field(NoiseStream.FilmDamage, state.Seed, 0u);
        (ERational, ERational, ERational, ERational) term = (ERational.FromSingle(clock).Multiply(ERational.FromSingle(state.WeaveRate)), ERational.Zero, ERational.Zero, ERational.Zero);
        (double X, double Y) = (
            state.WeaveX.Pixels(extent) * NoiseDimensions.One.Fbm(term, Octaves.Default, CoordinateHash.Branch(field, axes)),
            state.WeaveY.Pixels(extent) * NoiseDimensions.One.Fbm(term, Octaves.Default, CoordinateHash.Branch(field, axes + 1u)));
        return new((points, _, _) => { foreach (ref (double X, double Y) point in points) point = (point.X - X, point.Y - Y); }, Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp));
    }

    private static Func<long, long, (float Coverage, float Value)> Scratch(FilmDamage state, PixelExtent extent, Clock clock, uint field) {
        ERational count = ERational.FromSingle(state.ScratchCount);
        ERational lane = ERational.FromSingle(clock).Multiply(ERational.FromSingle(state.ScratchRate));
        double width = state.ScratchWidth.Pixels(extent);
        (uint sites, uint tones) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        return (column, _) => NoiseFunctions.F1(ERational.Create((2L * column) + 1 - extent.Width, 2L * extent.Width).Multiply(count), lane, Cellular.Default, sites) switch {
            var feature => ShapeEdge.Strip(feature.Delta.X.Abs().Divide(count).Multiply(extent.Width).ToDouble(), width, 1d) switch {
                var coverage => (coverage, CoordinateHash.Uniforms(feature.Cell.X, feature.Cell.Y, tones).X < state.Scratches.White ? coverage : 0f),
            },
        };
    }

    private static Func<long, long, (float Coverage, float Value)> Hair(FilmDamage state, PixelExtent extent, Clock clock, uint field) {
        (double pixels, double width) = (state.HairLength.Pixels(extent), state.HairWidth.Pixels(extent));
        ERational length = ERational.FromSingle(state.HairLength).Multiply(extent.ShortSide);
        ERational curl = ERational.FromSingle(state.HairCurl);
        ERational lane = ERational.FromSingle(clock).Multiply(ERational.FromSingle(state.HairRate));
        double share = state.HairCount * pixels * pixels / ((double)extent.Width * extent.Height);
        (uint across, uint down, uint sites, uint tones, uint strands) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        return (column, line) => {
            (ERational X, ERational Y, ERational Z, ERational W) strand = (X: ERational.Create((2L * column) + 1 - extent.Width, 2).Divide(length),
                Y: ERational.Create((2L * line) + 1 - extent.Height, 2).Divide(length), Z: ERational.Zero, W: ERational.Zero);
            (ERational, ERational, ERational lane, ERational Zero) bent = (strand.X.Add(curl.Multiply(ERational.FromSingle(NoiseDimensions.Two.Fbm(strand, Octaves.Default, across)))),
                strand.Y.Add(curl.Multiply(ERational.FromSingle(NoiseDimensions.Two.Fbm(strand, Octaves.Default, down)))), lane, ERational.Zero);
            (float Value, float EdgeDistance, (EInteger X, EInteger Y, EInteger Z, EInteger W) Cell) = NoiseDimensions.Three.Voronoi(bent, Cellular.Default, sites);
            Vector4 cell = NoiseFunctions.White(Cell, tones);
            float edge = ShapeEdge.Strip(EdgeDistance * pixels, width, 1d);
            float open = (float)RampInterpolation.Ease.Weight(Easing.Saturate((0.5f + (0.5f * NoiseDimensions.Two.Fbm(strand, Octaves.Default, strands)) - 0.45f) / 0.1f));
            float coverage = cell.X < share ? edge * open : 0f;
            return (coverage, cell.Y < state.Hairs.White ? coverage : 0f);
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmDamageParameter : IStateParameter<FilmDamage> {
    private static readonly Presentation<float> Width = ShortSideLength.Presentation with { Soft = (0f, 0.005f * ReferenceFrame.GreaterSide), Decimals = 4 };
    private static readonly Presentation<float> Rate = Frequency.Presentation with { Soft = (0f, 24f) };
    private static readonly Presentation<float> Sway = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };

    public static readonly FilmDamageParameter Specks = new("specks", new StateParameter<FilmDamage>.Record<FilmSpots>(
        Lens<FilmDamage, FilmSpots>.New(static damage => damage.Specks, static specks => damage => damage with { Specks = specks })));
    public static readonly FilmDamageParameter Dirt = new("dirt", new StateParameter<FilmDamage>.Record<FilmSpots>(
        Lens<FilmDamage, FilmSpots>.New(static damage => damage.Dirt, static dirt => damage => damage with { Dirt = dirt })));
    public static readonly FilmDamageParameter Blotches = new("blotches", new StateParameter<FilmDamage>.Record<FilmSpots>(
        Lens<FilmDamage, FilmSpots>.New(static damage => damage.Blotches, static blotches => damage => damage with { Blotches = blotches })));
    public static readonly FilmDamageParameter Scratches = new("scratches", new StateParameter<FilmDamage>.Record<DamageMark>(
        Lens<FilmDamage, DamageMark>.New(static damage => damage.Scratches, static scratches => damage => damage with { Scratches = scratches })));
    public static readonly FilmDamageParameter ScratchCount = new("scratches-count", StateParameter.Bounded<FilmDamage, MarkCount, float, InvalidStylize>(Lens<FilmDamage, MarkCount>.New(static damage => damage.ScratchCount, static count => damage => damage with { ScratchCount = count }), new() { Soft = (1f, 20f) }));
    public static readonly FilmDamageParameter ScratchWidth = new("scratches-width", StateParameter.Bounded<FilmDamage, ShortSideLength, float, InvalidPixelValue>(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.ScratchWidth, static width => damage => damage with { ScratchWidth = width }), Width));
    public static readonly FilmDamageParameter ScratchRate = new("scratches-rate", StateParameter.Bounded<FilmDamage, Frequency, float, InvalidGenerator>(Lens<FilmDamage, Frequency>.New(static damage => damage.ScratchRate, static rate => damage => damage with { ScratchRate = rate }), Rate));
    public static readonly FilmDamageParameter Hairs = new("hairs", new StateParameter<FilmDamage>.Record<DamageMark>(
        Lens<FilmDamage, DamageMark>.New(static damage => damage.Hairs, static hairs => damage => damage with { Hairs = hairs })));
    public static readonly FilmDamageParameter HairCount = new("hairs-count", StateParameter.Bounded<FilmDamage, HairCount, float, InvalidStylize>(Lens<FilmDamage, HairCount>.New(static damage => damage.HairCount, static count => damage => damage with { HairCount = count }), new() { Soft = (0f, 10f) }));
    public static readonly FilmDamageParameter HairWidth = new("hairs-width", StateParameter.Bounded<FilmDamage, ShortSideLength, float, InvalidPixelValue>(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.HairWidth, static width => damage => damage with { HairWidth = width }), Width));
    public static readonly FilmDamageParameter HairLength = new("hairs-length", StateParameter.Bounded<FilmDamage, ShortSideExtent, float, InvalidPixelValue>(Lens<FilmDamage, ShortSideExtent>.New(static damage => damage.HairLength, static length => damage => damage with { HairLength = length }),
    new() { Unit = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f * ReferenceFrame.GreaterSide, 0.3f * ReferenceFrame.GreaterSide) }));
    public static readonly FilmDamageParameter HairCurl = new("hairs-curl", StateParameter.Bounded<FilmDamage, NoiseDistortion, float, InvalidGenerator>(Lens<FilmDamage, NoiseDistortion>.New(static damage => damage.HairCurl, static curl => damage => damage with { HairCurl = curl }), new() { Soft = (0f, 4f) }));
    public static readonly FilmDamageParameter HairRate = new("hairs-rate", StateParameter.Bounded<FilmDamage, Frequency, float, InvalidGenerator>(Lens<FilmDamage, Frequency>.New(static damage => damage.HairRate, static rate => damage => damage with { HairRate = rate }), Rate));
    public static readonly FilmDamageParameter WeaveX = new("weave-x", StateParameter.Bounded<FilmDamage, ShortSideLength, float, InvalidPixelValue>(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveX, static weave => damage => damage with { WeaveX = weave }), Sway));
    public static readonly FilmDamageParameter WeaveY = new("weave-y", StateParameter.Bounded<FilmDamage, ShortSideLength, float, InvalidPixelValue>(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveY, static weave => damage => damage with { WeaveY = weave }), Sway));
    public static readonly FilmDamageParameter WeaveRate = new("weave-rate", StateParameter.Bounded<FilmDamage, Frequency, float, InvalidGenerator>(Lens<FilmDamage, Frequency>.New(static damage => damage.WeaveRate, static rate => damage => damage with { WeaveRate = rate }), Frequency.Presentation with { Soft = (0.1f, 20f) }));
    public static readonly FilmDamageParameter Seed = new("seed", StateParameter.Bounded<FilmDamage, Seed, int, InvalidGenerator>(Lens<FilmDamage, Seed>.New(static damage => damage.Seed, static seed => damage => damage with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmDamageParameter Timing = new("timing", new StateParameter<FilmDamage>.Record<Timing>(
        Lens<FilmDamage, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));

    public StateParameter<FilmDamage> Kind { get; }
}

public sealed record FilmFlicker(Exposure Depth, Frequency Rate, Seed Seed, Timing Timing)
    : IStateRecord<FilmFlicker, FilmFlickerParameter, InvalidStylize>, IPixelStage<FilmFlicker> {
    public static FilmFlicker Default { get; } = new(Exposure.Neutral, Frequency.Create(5f * 0.5f * 24f), Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmFlicker state, PassContext context) {
        Vector3 gain = new(float.Exp2(state.Depth * NoiseDimensions.One.Fbm(
            (ERational.FromSingle(state.Timing.At(context)).Multiply(ERational.FromSingle(state.Rate)), ERational.Zero, ERational.Zero, ERational.Zero),
            Octaves.Default, CoordinateHash.Field(NoiseStream.FilmFlicker, state.Seed, 0u))));
        return state.Depth == Exposure.Neutral ? None : Some<PixelPass>(new PixelPass.Color(row => { foreach (ref Vector4 pixel in row) pixel = new Vector4(pixel.AsVector3() * gain, pixel.W); }));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmFlickerParameter : IStateParameter<FilmFlicker> {
    public static readonly FilmFlickerParameter Depth = new("depth", StateParameter.Bounded<FilmFlicker, Exposure, float, InvalidToneValue>(Lens<FilmFlicker, Exposure>.New(static flicker => flicker.Depth, static depth => flicker => flicker with { Depth = depth }), Exposure.Presentation with { Soft = (0f, 1f) }));
    public static readonly FilmFlickerParameter Rate = new("rate", StateParameter.Bounded<FilmFlicker, Frequency, float, InvalidGenerator>(Lens<FilmFlicker, Frequency>.New(static flicker => flicker.Rate, static rate => flicker => flicker with { Rate = rate }),
    Frequency.Presentation with { Soft = (1f, 60f) }));
    public static readonly FilmFlickerParameter Seed = new("seed", StateParameter.Bounded<FilmFlicker, Seed, int, InvalidGenerator>(Lens<FilmFlicker, Seed>.New(static flicker => flicker.Seed, static seed => flicker => flicker with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmFlickerParameter Timing = new("timing", new StateParameter<FilmFlicker>.Record<Timing>(
        Lens<FilmFlicker, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));

    public StateParameter<FilmFlicker> Kind { get; }
}
