using System.Numerics;
using System.Runtime.Intrinsics;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record DamageMark(float Likelihood, Hold Hold, float White) {
    public static DamageMark Default { get; } = new(0f, Hold.Create(Duration.FromSeconds(6d / 24d)), 0f);
}

internal sealed record FilmSpots(DamageMark Mark, PointCells Cells) {
    public static FilmSpots Default { get; } = new(DamageMark.Default, new(ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), 0.02f, 2f * 0.2f, 1f, 0f, 0.3f));
}

internal sealed record FilmDamage(
    FilmSpots Specks, FilmSpots Dirt, FilmSpots Blotches, DamageMark Scratches, float ScratchCount, ShortSideLength ScratchWidth, float ScratchRate, DamageMark Hairs,
    float HairCount, ShortSideLength HairWidth, ShortSideLength HairLength, float HairCurl, float HairRate, float WeaveRate, int Seed, Timing Timing) : IPixelStage<FilmDamage> {
    public static FilmDamage Default { get; } = new(
        FilmSpots.Default with { Mark = DamageMark.Default with { White = 0.5f } }, FilmSpots.Default, FilmSpots.Default,
        DamageMark.Default with { White = 0.7f }, 3f, ShortSideLength.Create(0.0008f * ReferenceFrame.GreaterSide), 0.05f * 24f,
        DamageMark.Default, 1f, ShortSideLength.Create(0.0012f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.08f * ReferenceFrame.GreaterSide), 1f,
        0.1f * 24f, 5f * 0.05f * 0.5f * 24f, 0, Timing.Default);

    public Option<ShortSideLength> WeaveX { get; init; }
    public Option<ShortSideLength> WeaveY { get; init; }

    public static Fin<Option<PixelPass>> Pass(FilmDamage state, PassContext context) {
        Duration time = state.Timing.At(context);
        (float clock, PixelExtent extent) = ((float)time.TotalSeconds, context.Extent);
        double denominator = 2d * extent.ShortSide;
        Seq<(DamageMark Mark, bool Held, Func<uint, Fin<Func<int, int, (float Coverage, float Value)>>> Field)> marks = [
            .. Seq(state.Specks, state.Dirt, state.Blotches).Map(spots => (spots.Mark, true, new Func<uint, Fin<Func<int, int, (float Coverage, float Value)>>>(field => new Func<int, int, (float Coverage, float Value)>((x, y) =>
                spots.Cells.Disc((((2d * x) + 1d - extent.Width) / denominator, ((2d * y) + 1d - extent.Height) / denominator), extent.ShortSide, field,
                    tone => tone < spots.Mark.White ? 1f : 0f))))),
            (state.Scratches, false, field => Scratch(state, extent, clock, field)),
            (state.Hairs, false, field => Hair(state, extent, clock, field))];
        return marks
            .Map((mark, index) => (Mark: mark, Period: mark.Mark.Hold.Period(time), Index: (uint)index))
            .Filter(drawn => CoordinateHash.Uniforms(Vector128<int>.Zero, Shot(drawn.Period, drawn.Index)).X < drawn.Mark.Mark.Likelihood)
            .Traverse(drawn => drawn.Mark.Field(Shot(drawn.Mark.Held ? drawn.Period : 0u, drawn.Index))).As().Map(shown => {
                Func<int, int, (float Coverage, float Value)>[] fields = [.. shown];
                Option<PixelPass> composite = fields is [] ? None : Some<PixelPass>(Composite(fields));
                return state.WeaveX.IsNone && state.WeaveY.IsNone ? composite : Some<PixelPass>(Woven(composite, Weave(state, extent, clock)));
            });

        uint Shot(Int128 term, uint index) => CoordinateHash.Branch(CoordinateHash.Branch(state.Seed, term, 6u), index);
    }

    private static PixelPass.Positional Composite(Func<int, int, (float Coverage, float Value)>[] marks) => new((row, x, y) => {
        foreach (Func<int, int, (float Coverage, float Value)> field in marks) {
            for (int at = 0; at < row.Length; at++) {
                (float coverage, float value) = field(x + at, y);
                row[at] = coverage switch {
                    0f => row[at],
                    1f => new Vector4(new Vector3(value), row[at].W),
                    _ => new Vector4((row[at].AsVector3() * (1f - coverage)) + new Vector3(value), row[at].W),
                };
            }
        }
    });

    private static PixelPass.Frame Woven(Option<PixelPass> composite, Fin<CoordinateMap> weave) =>
        new((frame, progress) => composite.Traverse(pass => pass.Run(frame)).As().Bind(_ => weave.Bind(map => map.Apply(frame, progress))));

    private static Fin<CoordinateMap> Weave(FilmDamage state, PixelExtent extent, float clock) =>
        Pose.Shaken(extent, clock, state.WeaveRate, new Vector2(state.WeaveX.Share, state.WeaveY.Share), 0f, 1f, NoiseBasis.Perlin.Default.Octaves, CoordinateHash.Branch(state.Seed, 0u, 22u))
            .Map(static pose => (CoordinateMap)new CoordinateMap.Planar(pose.Inverse, Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp)));

    private static Fin<Func<int, int, (float Coverage, float Value)>> Scratch(FilmDamage state, PixelExtent extent, float clock, uint field) {
        (float count, float drift, float width) = (state.ScratchCount, clock * state.ScratchRate, (float)state.ScratchWidth.Pixels(extent));
        (uint sites, uint tones) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        return VoronoiFeature.F1.Sampler(NoiseDimensions.Two, NoiseBasis.Perlin.Plain.Octaves, NoiseBasis.Voronoi.Default.Exponent, NoiseBasis.Voronoi.Default.Randomness, 0f)
            .Map(sampling => new Func<int, int, (float Coverage, float Value)>((x, _) => {
                float across = ((2f * x) + 1f - extent.Width) / (2f * extent.Width) * count;
                Vector4 site = sampling.Sample(new Vector4(across, drift, 0f, 0f), sites).Site;
                float coverage = DistanceField.Stroke(float.Abs(site.X - across) / count * extent.Width, width, 1f);
                return (coverage, CoordinateHash.Uniforms(Lattice(site), tones).X < state.Scratches.White ? coverage : 0f);
            }));
    }

    private static Fin<Func<int, int, (float Coverage, float Value)>> Hair(FilmDamage state, PixelExtent extent, float clock, uint field) {
        (float pixels, float width, float curl, float drift) = ((float)state.HairLength.Pixels(extent), (float)state.HairWidth.Pixels(extent), state.HairCurl, clock * state.HairRate);
        float share = state.HairCount * pixels * pixels / ((float)extent.Width * extent.Height);
        (uint across, uint down, uint sites, uint tones, uint strands) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        return VoronoiFeature.DistanceToEdge.Sampler(NoiseDimensions.Three, NoiseBasis.Perlin.Plain.Octaves, NoiseBasis.Voronoi.Default.Exponent, NoiseBasis.Voronoi.Default.Randomness, 0f)
            .Map(sampling => new Func<int, int, (float Coverage, float Value)>((x, y) => {
                Vector4 strand = new(((2f * x) + 1f - extent.Width) / 2f / pixels, ((2f * y) + 1f - extent.Height) / 2f / pixels, 0f, 0f);
                Vector4 bent = new(
                    strand.X + (curl * Fractal.Fbm.Sample(NoiseDimensions.Two, strand, NoiseBasis.Perlin.Default.Octaves, 0f, 0f, across)),
                    strand.Y + (curl * Fractal.Fbm.Sample(NoiseDimensions.Two, strand, NoiseBasis.Perlin.Default.Octaves, 0f, 0f, down)), drift, 0f);
                (float edgeDistance, Vector4 site) = sampling.Sample(bent, sites);
                Vector4 draw = CoordinateHash.Uniforms(Lattice(site), tones);
                float open = DistanceField.SmoothStep(0f, 1f, (0.5f + (0.5f * Fractal.Fbm.Sample(NoiseDimensions.Two, strand, NoiseBasis.Perlin.Default.Octaves, 0f, 0f, strands)) - 0.45f) / 0.1f);
                float coverage = draw.X < share ? DistanceField.Stroke(edgeDistance * pixels, width, 1f) * open : 0f;
                return (coverage, draw.Y < state.Hairs.White ? coverage : 0f);
            }));
    }

    private static Vector128<int> Lattice(Vector4 site) => Vector128.ConvertToInt32(Vector128.Floor(site.AsVector128()));
}

internal sealed record FilmFlicker(float Depth, float Rate, int Seed, Timing Timing) : IPixelStage<FilmFlicker> {
    public static FilmFlicker Default { get; } = new(0f, 5f * 0.5f * 24f, 0, Timing.Default);

    public static Fin<Option<PixelPass>> Pass(FilmFlicker state, PassContext context) =>
        state.Depth is 0f ? None
            : Some<PixelPass>(new PixelPass.Point(new LutTable.Affine(Matrix4x4.CreateScale(float.Exp2(state.Depth * Fractal.Fbm.Sample(
                NoiseDimensions.One, new Vector4((float)state.Timing.At(context).TotalSeconds * state.Rate, 0f, 0f, 0f), NoiseBasis.Perlin.Default.Octaves, 0f, 0f, CoordinateHash.Branch(state.Seed, 0u, 7u))))).Apply));
}
