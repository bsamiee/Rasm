using System.Numerics;
using PeterO.Numbers;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DamageMark(AxisFraction Likelihood, Hold Hold, AxisFraction White) : IStateRecord<DamageMark, DamageMarkParameter> {
    public static DamageMark Default { get; } = new(AxisFraction.MinValue, Hold.Create(6f / 24f), AxisFraction.MinValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DamageMarkParameter : IStateParameter<DamageMark> {
    public static readonly DamageMarkParameter Likelihood = new("likelihood", StateParameter.Bounded(Lens<DamageMark, AxisFraction>.New(static mark => mark.Likelihood, static likelihood => mark => mark with { Likelihood = likelihood })),
        new("Likelihood", "Share of held periods showing these marks"));
    public static readonly DamageMarkParameter Hold = new("hold", StateParameter.Bounded(Lens<DamageMark, Hold>.New(static mark => mark.Hold, static hold => mark => mark with { Hold = hold })),
        new("Hold", "Seconds each drawn pattern holds before the next"));
    public static readonly DamageMarkParameter White = new("white", StateParameter.Bounded(Lens<DamageMark, AxisFraction>.New(static mark => mark.White, static white => mark => mark with { White = white })),
        new("White", "Share of marks printing white, the remainder printing black"));

    public StateParameter<DamageMark> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FilmSpots(DamageMark Mark, PointCells Cells) : IStateRecord<FilmSpots, FilmSpotsParameter> {
    public static FilmSpots Default { get; } = new(DamageMark.Default,
        new(ShortSideExtent.Create(0.02f * ReferenceFrame.GreaterSide), AxisFraction.Create(0.02f), AxisFraction.Create(2f * 0.2f), AxisFraction.MaxValue, Magnitude.Zero, AxisFraction.Create(0.3f)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmSpotsParameter : IStateParameter<FilmSpots> {
    public static readonly FilmSpotsParameter Mark = new("mark", new StateParameter<FilmSpots>.Record<DamageMark>(Lens<FilmSpots, DamageMark>.New(static spots => spots.Mark, static mark => spots => spots with { Mark = mark })),
        new("Timing and polarity", "When these marks show and whether they print white or black"));
    public static readonly FilmSpotsParameter Cells = new("cells", new StateParameter<FilmSpots>.Record<PointCells>(Lens<FilmSpots, PointCells>.New(static spots => spots.Cells, static cells => spots => spots with { Cells = cells })),
        new("Cells", "Spacing, density, size, variation, spread, and softness of these marks"));

    public StateParameter<FilmSpots> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FilmDamage(
    FilmSpots Specks, FilmSpots Dirt, FilmSpots Blotches, DamageMark Scratches, PrimaryScale ScratchCount, ShortSideLength ScratchWidth, Frequency ScratchRate,
    DamageMark Hairs, Magnitude HairCount, ShortSideLength HairWidth, ShortSideExtent HairLength, NoiseDistortion HairCurl, Frequency HairRate,
    ShortSideLength WeaveX, ShortSideLength WeaveY, Frequency WeaveRate, Seed Seed, Timing Timing)
    : IStateRecord<FilmDamage, FilmDamageParameter>, IPixelStage<FilmDamage> {
    public static FilmDamage Default { get; } = new(
        FilmSpots.Default with { Mark = DamageMark.Default with { White = AxisFraction.Create(0.5f) } }, FilmSpots.Default, FilmSpots.Default,
        DamageMark.Default with { White = AxisFraction.Create(0.7f) }, PrimaryScale.Create(3f), ShortSideLength.Create(0.0008f * ReferenceFrame.GreaterSide), Frequency.Create(0.05f * 24f),
        DamageMark.Default, Magnitude.Unit, ShortSideLength.Create(0.0012f * ReferenceFrame.GreaterSide), ShortSideExtent.Create(0.08f * ReferenceFrame.GreaterSide), NoiseDistortion.Create(1f),
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
        LatticePoint term = new(ERational.FromSingle(clock).Multiply(ERational.FromSingle(state.WeaveRate)), ERational.Zero, ERational.Zero, ERational.Zero);
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
            LatticePoint strand = new(ERational.Create((2L * column) + 1 - extent.Width, 2).Divide(length),
                ERational.Create((2L * line) + 1 - extent.Height, 2).Divide(length), ERational.Zero, ERational.Zero);
            LatticePoint bent = new(strand.X.Add(curl.Multiply(ERational.FromSingle(NoiseDimensions.Two.Fbm(strand, Octaves.Default, across)))),
                strand.Y.Add(curl.Multiply(ERational.FromSingle(NoiseDimensions.Two.Fbm(strand, Octaves.Default, down)))), lane, ERational.Zero);
            (float Value, float EdgeDistance, LatticeCell Cell) = NoiseDimensions.Three.Voronoi(bent, Cellular.Default, sites);
            Vector4 cell = NoiseFunctions.White(Cell, tones);
            float edge = ShapeEdge.Strip(EdgeDistance * pixels, width, 1d);
            float open = (float)RampInterpolation.Ease.Weight(Easing.Saturate((0.5f + (0.5f * NoiseDimensions.Two.Fbm(strand, Octaves.Default, strands)) - 0.45f) / 0.1f));
            float coverage = cell.X < share ? edge * open : 0f;
            return (coverage, cell.Y < state.Hairs.White ? coverage : 0f);
        };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmDamageParameter : IStateParameter<FilmDamage> {
    private static readonly Presentation<float> Width = ShortSideLength.Presentation with { Soft = (0f, 0.005f * ReferenceFrame.GreaterSide), Decimals = 4 };
    private static readonly Presentation<float> Rate = Frequency.Presentation with { Soft = (0f, 24f) };
    private static readonly Presentation<float> Sway = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };

    public static readonly FilmDamageParameter Specks = new("specks", new StateParameter<FilmDamage>.Record<FilmSpots>(Lens<FilmDamage, FilmSpots>.New(static damage => damage.Specks, static specks => damage => damage with { Specks = specks })),
        new("Specks", "Independent timing, polarity, and point field of these marks"));
    public static readonly FilmDamageParameter Dirt = new("dirt", new StateParameter<FilmDamage>.Record<FilmSpots>(Lens<FilmDamage, FilmSpots>.New(static damage => damage.Dirt, static dirt => damage => damage with { Dirt = dirt })),
        new("Dirt", "Independent timing, polarity, and point field of these marks"));
    public static readonly FilmDamageParameter Blotches = new("blotches", new StateParameter<FilmDamage>.Record<FilmSpots>(Lens<FilmDamage, FilmSpots>.New(static damage => damage.Blotches, static blotches => damage => damage with { Blotches = blotches })),
        new("Blotches", "Independent timing, polarity, and point field of these marks"));
    public static readonly FilmDamageParameter Scratches = new("scratches", new StateParameter<FilmDamage>.Record<DamageMark>(Lens<FilmDamage, DamageMark>.New(static damage => damage.Scratches, static scratches => damage => damage with { Scratches = scratches })),
        new("Scratches", "When these marks show and whether they print white or black"));
    public static readonly FilmDamageParameter ScratchCount = new("scratches-count", StateParameter.Bounded(Lens<FilmDamage, PrimaryScale>.New(static damage => damage.ScratchCount, static count => damage => damage with { ScratchCount = count }), new Presentation<float> { Soft = (1f, 20f) }),
        new("Scratch count", "Scratch lines across the frame width"));
    public static readonly FilmDamageParameter ScratchWidth = new("scratches-width", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.ScratchWidth, static width => damage => damage with { ScratchWidth = width }), Width),
        new("Scratch width", "Scratch stroke width as a share of the frame's short side"));
    public static readonly FilmDamageParameter ScratchRate = new("scratches-rate", StateParameter.Bounded(Lens<FilmDamage, Frequency>.New(static damage => damage.ScratchRate, static rate => damage => damage with { ScratchRate = rate }), Rate),
        new("Scratch rate", "Scratch field evolution per second"));
    public static readonly FilmDamageParameter Hairs = new("hairs", new StateParameter<FilmDamage>.Record<DamageMark>(Lens<FilmDamage, DamageMark>.New(static damage => damage.Hairs, static hairs => damage => damage with { Hairs = hairs })),
        new("Hairs", "When these marks show and whether they print white or black"));
    public static readonly FilmDamageParameter HairCount = new("hairs-count", StateParameter.Bounded(Lens<FilmDamage, Magnitude>.New(static damage => damage.HairCount, static count => damage => damage with { HairCount = count }), new Presentation<float> { Soft = (0f, 10f) }),
        new("Hair count", "Mean hair strands per frame, including fractional counts"));
    public static readonly FilmDamageParameter HairWidth = new("hairs-width", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.HairWidth, static width => damage => damage with { HairWidth = width }), Width),
        new("Hair width", "Hair stroke width as a share of the frame's short side"));
    public static readonly FilmDamageParameter HairLength = new("hairs-length", StateParameter.Bounded(Lens<FilmDamage, ShortSideExtent>.New(static damage => damage.HairLength, static length => damage => damage with { HairLength = length }),
        ShortSideExtent.Presentation with { Soft = (0.01f * ReferenceFrame.GreaterSide, 0.3f * ReferenceFrame.GreaterSide), Scale = TrackScale.Linear }),
        new("Hair length", "Hair cell size as a share of the frame's short side"));
    public static readonly FilmDamageParameter HairCurl = new("hairs-curl", StateParameter.Bounded(Lens<FilmDamage, NoiseDistortion>.New(static damage => damage.HairCurl, static curl => damage => damage with { HairCurl = curl }), new Presentation<float> { Soft = (0f, 4f) }),
        new("Hair curl", "Bend of the hair field in hair cell sizes"));
    public static readonly FilmDamageParameter HairRate = new("hairs-rate", StateParameter.Bounded(Lens<FilmDamage, Frequency>.New(static damage => damage.HairRate, static rate => damage => damage with { HairRate = rate }), Rate),
        new("Hair rate", "Hair field evolution per second"));
    public static readonly FilmDamageParameter WeaveX = new("weave-x", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveX, static weave => damage => damage with { WeaveX = weave }), Sway),
        new("Weave X", "Horizontal gate weave as a share of the frame's short side"));
    public static readonly FilmDamageParameter WeaveY = new("weave-y", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveY, static weave => damage => damage with { WeaveY = weave }), Sway),
        new("Weave Y", "Vertical gate weave as a share of the frame's short side"));
    public static readonly FilmDamageParameter WeaveRate = new("weave-rate", StateParameter.Bounded(Lens<FilmDamage, Frequency>.New(static damage => damage.WeaveRate, static rate => damage => damage with { WeaveRate = rate }), Frequency.Presentation with { Soft = (0.1f, 20f) }),
        new("Weave rate", "Gate weave noise cells per second"));
    public static readonly FilmDamageParameter Seed = new("seed", StateParameter.Bounded(Lens<FilmDamage, Seed>.New(static damage => damage.Seed, static seed => damage => damage with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));
    public static readonly FilmDamageParameter Timing = new("timing", new StateParameter<FilmDamage>.Record<Timing>(Lens<FilmDamage, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));

    public StateParameter<FilmDamage> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FilmFlicker(Exposure Depth, Frequency Rate, Seed Seed, Timing Timing)
    : IStateRecord<FilmFlicker, FilmFlickerParameter>, IPixelStage<FilmFlicker> {
    public static FilmFlicker Default { get; } = new(Exposure.Neutral, Frequency.Create(5f * 0.5f * 24f), Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmFlicker state, PassContext context) {
        Vector3 gain = new(float.Exp2(state.Depth * NoiseDimensions.One.Fbm(
            new LatticePoint(ERational.FromSingle(state.Timing.At(context)).Multiply(ERational.FromSingle(state.Rate)), ERational.Zero, ERational.Zero, ERational.Zero),
            Octaves.Default, CoordinateHash.Field(NoiseStream.FilmFlicker, state.Seed, 0u))));
        return state.Depth == Exposure.Neutral ? None : Some<PixelPass>(new PixelPass.Color(row => { foreach (ref Vector4 pixel in row) pixel = new Vector4(pixel.AsVector3() * gain, pixel.W); }));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmFlickerParameter : IStateParameter<FilmFlicker> {
    public static readonly FilmFlickerParameter Depth = new("depth", StateParameter.Bounded(Lens<FilmFlicker, Exposure>.New(static flicker => flicker.Depth, static depth => flicker => flicker with { Depth = depth }), Exposure.Presentation with { Soft = (0f, 1f) }),
        new("Depth", "Peak exposure swing of the flicker, in stops"));
    public static readonly FilmFlickerParameter Rate = new("rate", StateParameter.Bounded(Lens<FilmFlicker, Frequency>.New(static flicker => flicker.Rate, static rate => flicker => flicker with { Rate = rate }), Frequency.Presentation with { Soft = (1f, 60f) }),
        new("Rate", "Flicker noise cells per second"));
    public static readonly FilmFlickerParameter Seed = new("seed", StateParameter.Bounded(Lens<FilmFlicker, Seed>.New(static flicker => flicker.Seed, static seed => flicker => flicker with { Seed = seed })),
        new("Seed", "Number selecting the field's pattern"));
    public static readonly FilmFlickerParameter Timing = new("timing", new StateParameter<FilmFlicker>.Record<Timing>(Lens<FilmFlicker, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })),
        new("Timing", "Stored time and rate of recorded frame time"));

    public StateParameter<FilmFlicker> Kind { get; }
    public ParameterText Text { get; }
}
