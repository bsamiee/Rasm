using System.Numerics;
using Darkroom.Chain;
using Emgu.CV;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Fog(NoiseSource Noise, float Variation, Length Start, float Amount, Seq<RampStop> Colors, GeneratedLayer Layer, int Seed, Timing Timing) : IPixelStage<Fog> {
    public Option<Length> Attenuation { get; init; }
    public Option<Length> Cutoff { get; init; }
    public Option<ShortSideLength> Diffusion { get; init; }

    public static Fog Default { get; } = new(
        NoiseSource.Default with { Basis = NoiseBasis.Perlin.Default with { Dimensions = NoiseDimensions.Four, Octaves = new(4f, 0.2f, 2f) } }, 0f, Length.Zero, 1f,
        [new(new Unicolour(ColourSpace.Rgb, 0.85098d, 0.85098d, 0.85098d), 0f)], GeneratedLayer.Default, 0, Timing.Default) { Attenuation = Length.FromFeet(100) };

    public static Fog Haze { get; } = Default with {
        Attenuation = None,
        Amount = 0.35f,
        Diffusion = ShortSideLength.Create(0.75f * ReferenceFrame.GreaterSide),
        Colors = Presets.Solid(new Vector3(Metering.MiddleGray)),
    };

    public static Seq<(string Name, Func<Fog, Fog> Look)> Looks { get; } = [(nameof(Haze), static _ => Haze)];

    public static Seq<GuideChannel> Channels(Fog state) => state.Attenuation.IsSome ? [GuideChannel.Depth] : [];

    public static Fin<Option<PixelPass>> Pass(Fog state, PassContext context) =>
        state.Attenuation.Match(
            Some: attenuation => from depth in context.Guides.Find(GuideChannel.Depth)
                                 from camera in context.Camera
                                 select Opacity(state, context, depth, camera, attenuation),
            None: static () => Some(Fin.Succ<Func<int, int, float>>(static (_, _) => 1f)))
        .Traverse(static opacity => opacity).As()
        .Bind(found => found.Traverse(opacity => RampLight.Of(state.Colors, context) switch {
            var table => state.Diffusion.Match(
                Some: radius => Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Filter(source => {
                    PixelFrame spread = new(frame.Origin, frame.Size, frame.Extent);
                    using Mat target = spread.Header();
                    source.Blur(target, radius.Pixels(context.Extent) / 3d);
                    return state.Layer.Run(Fill(state, table, opacity, (x, y) => spread.View.Span[y - spread.Origin.Y, x - spread.Origin.X].AsVector3()), frame, progress);
                }))),
                None: () => state.Layer.Pass(Fill(state, table, opacity, static (_, _) => Vector3.One))),
        }).As())
        .Map(static staged => staged.Flatten());

    private static Fin<Func<int, int, float>> Opacity(Fog state, PassContext context, PixelFrame depth, Camera camera, Length attenuation) {
        Matrix3x2 frame = FieldPlacement.Default.Frame(context.Extent);
        Extinction medium = new((float)state.Start.Meters, (float)attenuation.Meters, state.Cutoff.Map(static cutoff => (float)cutoff.Meters));
        return state.Variation is 0f ? medium.Field(depth, camera, static (_, _, _) => 1f)
            : state.Noise.Sampler((float)state.Timing.At(context).TotalSeconds, CoordinateHash.Branch(state.Seed, 0u, 13u)).Map(noise => medium.Field(depth, camera, (x, y, path) =>
                float.Max(1f + (state.Variation * ((2f * noise(new Vector4(Vector2.Transform(new Vector2(x, y), frame), 0f, path))) - 1f)), 0f)));
    }

    private static Action<Span<Vector4>, int, int> Fill(Fog state, ColorRamp table, Func<int, int, float> opacity, Func<int, int, Vector3> light) =>
        (row, x, y) => {
            for (int i = 0; i < row.Length; i++) {
                float coverage = opacity(x + i, y);
                Vector4 medium = table.Sample(coverage);
                row[i] = new Vector4(medium.AsVector3() * light(x + i, y), state.Amount * coverage * medium.W);
            }
        };
}

public sealed record Starfield(
    FieldPlacement Placement, NoiseBasis Stars, ShortSideLength StarScale, ShortSideLength StarRadius, Seq<RampStop> StarColors, float StarExposure, float Twinkle, float TwinkleRate,
    NoiseSource Nebula, float NebulaDensity, Seq<RampStop> NebulaColors, float NebulaExposure, GeneratedLayer Layer, int Seed, Timing Timing) : IPixelStage<Starfield> {
    public static Starfield Default { get; } = new(
        FieldPlacement.Default, NoiseBasis.Voronoi.Default with { Octaves = new(2.56f, 0.165441f, 2.6f), Exponent = 0.4f, Smoothness = 1f },
        ShortSideLength.Create(0.1f), ShortSideLength.Create(0.1f), Presets.Grayscale, 0f, 0f, 0f,
        NoiseSource.Default with { Basis = NoiseBasis.Perlin.Default with { Octaves = new(6f, 0.56f, 2f), Distortion = 0.1f } },
        0.5f, Presets.Grayscale, 0f, GeneratedLayer.Added, 0, Timing.Default);

    public static Fin<Option<PixelPass>> Pass(Starfield state, PassContext context) {
        (ColorRamp starTable, ColorRamp nebulaTable) = (RampLight.Of(state.StarColors, context), RampLight.Of(state.NebulaColors, context));
        (uint field, float time) = (CoordinateHash.Branch(state.Seed, 0u, 14u), (float)state.Timing.At(context).TotalSeconds);
        (uint glint, uint sites, uint clouds, uint draws) = (CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        (float gain, float scale, float reach) = (float.Exp2(state.StarExposure), state.StarScale, state.StarScale / (float)state.StarRadius);
        return state.Layer.Pass((state.Stars.Sampler, Steadied(state, time, glint), Clouded(state, time, clouds, nebulaTable)).Apply((stars, steady, nebula) =>
            DistanceField.Fill(state.Placement.Frame(context.Extent), point => {
                (float value, Vector4 site) = stars(new Vector4(point / scale, 0f, 0f), sites);
                Vector4 draw = NoiseDimensions.Four.White(site, draws);
                return new Vector4((draw.Y * float.Clamp(1f - (value * reach), 0f, 1f) * steady(site) * gain * starTable.Sample(draw.X).AsVector3()) + nebula(point), 1f);
            })).As());
    }

    private static Fin<Func<Vector4, float>> Steadied(Starfield state, float time, uint glint) =>
        state.Twinkle is 0f ? new Func<Vector4, float>(static _ => 1f)
            : (NoiseBasis.Perlin.Plain with { Dimensions = NoiseDimensions.Four }).Sampler.Map(plain => (Phase: state.TwinkleRate * time, Depth: state.Twinkle) switch {
                var held => new Func<Vector4, float>(site => 1f - (held.Depth * plain(site with { W = site.W + held.Phase }, glint).Value)),
            });

    private static Fin<Func<Vector2, Vector3>> Clouded(Starfield state, float time, uint clouds, ColorRamp table) =>
        state.NebulaDensity is 0f ? new Func<Vector2, Vector3>(static _ => Vector3.Zero)
            : state.Nebula.Sampler(time, clouds).Map(sample => (Light: float.Exp2(state.NebulaExposure), Floor: 1f - state.NebulaDensity) switch {
                var held => new Func<Vector2, Vector3>(point => sample(new Vector4(point, 0f, 0f)) switch {
                    var n => table.Sample(n) switch { var cloud => DistanceField.SmootherStep(held.Floor, 1f, n) * cloud.W * held.Light * cloud.AsVector3() },
                }),
            });
}
