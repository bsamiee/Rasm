using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Darkroom.Chain;
using Emgu.CV;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FlareAnchor {
    public static readonly FlareAnchor Frame = new("frame", [], static (position, _, context) => Some((context.Extent.Point(position), Option<float>.None)));
    public static readonly FlareAnchor Sun = new("sun", [GuideChannel.Depth], static (_, _, context) =>
        context.Lights.ToFin(new DarkroomFault("Sun flare anchor", DarkroomReason.Lights)).Map(lights => lights.Sun.Bind(direction => Distant(direction, context))));
    public static readonly FlareAnchor Light = new("light", [GuideChannel.Depth], static (_, light, context) =>
        light.Traverse(target => context.Lights.ToFin(new DarkroomFault($"Light flare anchor {target}", DarkroomReason.Lights)).Map(lights =>
            (from point in lights.Points.Find(target) from camera in context.Camera from located in camera.Pixel(point) select (located.Point, Depth: Some(located.Depth)))
            | lights.Directions.Find(target).Bind(direction => Distant(direction, context)))).As().Map(static located => located.Flatten()));

    public Seq<GuideChannel> Occluders { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Option<(Vector2 Point, Option<float> Depth)>> Locate(Vector2 position, Option<Guid> light, PassContext context);

    private static Option<(Vector2 Point, Option<float> Depth)> Distant(Vector3 direction, PassContext context) =>
        context.Camera.Bind(camera => camera.Distant(direction)).Map(static located => (located.Point, Some(located.Depth)));
}

public sealed record FlareSource(FlareAnchor Anchor, Vector2 Position, bool Occlusion) {
    public Option<Guid> Light { get; init; }

    public static FlareSource Default { get; } = new(FlareAnchor.Frame, new Vector2(0.5f), Occlusion: true);

    public Seq<GuideChannel> Channels => Occlusion ? Anchor.Occluders : [];

    public Fin<Option<PixelPass>> Pass(PassContext context, Func<Vector2, Option<Occluder>, Func<PixelFrame, Option<IProgress<int>>, Fin<Unit>>> kernel) =>
        Anchor.Locate(Position, Light, context).Map(location =>
            from source in location
            from occluder in Occlusion ? source.Depth.Traverse(depth => context.Guides.Find(GuideChannel.Depth).Map(guide => new Occluder(guide, depth))).As() : Some(Option<Occluder>.None)
            select (PixelPass)new PixelPass.Frame(kernel(source.Point, occluder)));
}

public sealed record LensFlare(
    FlareSource Source, Seq<RampStop> HotspotColors, ShortSideLength HotspotSize, ShortSideLength HotspotSoftness,
    Seq<RampStop> StarburstColors, ShortSideLength StarburstLength, NoiseBasis Basis, float StarburstDensity, float StarburstSharpness, float Shimmer, Hold Hold,
    Seq<RampStop> BarColors, ShortSideLength BarLength, ShortSideLength BarThickness, Angle BarAngle,
    Seq<RampStop> RingColors, ShortSideLength RingRadius, ShortSideLength RingWidth, ShortSideLength RingSoftness, float RingMagnification,
    Seq<RampStop> GhostColors, ShortSideLength GhostSize, int GhostCount, float GhostFirst, float GhostLast, float GhostVariation, float GhostSoftness,
    Iris Aperture, float Fringe, GeneratedLayer Layer, Seed Seed, Timing Timing) : IPixelStage<LensFlare> {
    private static readonly ShortSideLength SourceRadius = ShortSideLength.Create(0.2f * 0.3f / 2f * ReferenceFrame.GreaterSide);

    public Option<float> Hotspot { get; init; }
    public Option<float> Starburst { get; init; }
    public Option<float> Bar { get; init; }
    public Option<float> Ring { get; init; }
    public Option<float> Ghosts { get; init; }

    public static LensFlare Default { get; } = new(
        FlareSource.Default with { Position = new(0.3f, 1f - 0.6f) }, Presets.White, SourceRadius, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide),
        Presets.White, ShortSideLength.Create(0.5f), NoiseBasis.Perlin.Default with { Octaves = NoiseBasis.Perlin.Default.Octaves with { Roughness = 0.563f } }, 30f, 3f, 0f, Hold.MinValue,
        Presets.White, ShortSideLength.Create(0.972f / 2f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), Angle.Zero,
        Presets.White, ShortSideLength.Create(1f / 2.1f), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), 0f,
        Presets.White, SourceRadius, 5, 0.6f, -1f, 0.5f, 0.3f, Iris.Create(6, double.Pi / 3d, 0d, 0d, 1d), 0f, GeneratedLayer.Added, Seed.MinValue, Timing.Default) {
        Hotspot = 0f, Starburst = 0f, Bar = 0f, Ring = 0f, Ghosts = 0f,
    };

    public static LensFlare NachoLens { get; } = Default with { Aperture = Iris.Create(3, 0.179d, 0.332d, 0d, 1d), Fringe = -0.1f };
    public static LensFlare RingLens { get; } = Default with { Aperture = Iris.Create(6, 0d, 0.796d, 0.902d, 1d), Fringe = 0.189f };
    public static LensFlare HexLens { get; } = Default with { Aperture = Iris.Create(6, 0d, 0d, 0.902d, 1d), Fringe = 0.052f };
    public static LensFlare RoundLens { get; } = Default with { Aperture = Iris.Create(16, 0d, 1d, 0d, 1d), Fringe = 0.012f };

    public static Seq<(string Name, Func<LensFlare, LensFlare> Look)> Looks { get; } = [
        (nameof(NachoLens), static _ => NachoLens), (nameof(RingLens), static _ => RingLens), (nameof(HexLens), static _ => HexLens), (nameof(RoundLens), static _ => RoundLens)];

    public static Seq<GuideChannel> Channels(LensFlare state) => state.Source.Channels;

    public static Fin<Option<PixelPass>> Pass(LensFlare state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) =>
            state.Basis.Sampler.Map(basis => Terms(state, context, point, basis)).Bind(terms => terms is [] ? unit
                : occlusion.Map(occluder => occluder.Visible(point, (float)state.HotspotSize.Pixels(context.Extent))).IfNone(1f) switch {
                    var visible => state.Layer.Run(DistanceField.Fill(FieldPlacement.Default.Frame(context.Extent), q =>
                        new Vector4(visible * terms.Aggregate(Vector3.Zero, (light, term) => light + term(q)), 1f)), frame, progress),
                }));

    private static Func<Vector2, Vector3>[] Terms(LensFlare state, PassContext context, Vector2 source, Func<Vector4, uint, (float Value, Vector4 Site)> basis) {
        (float side, Iris iris, PixelExtent extent) = (context.Extent.ShortSide, state.Aperture, context.Extent);
        Vector2 at = Vector2.Transform(source - new Vector2(0.5f), FieldPlacement.Default.Frame(extent));
        (uint draws, uint field) = (state.Seed.Field(19u, 1u), state.Seed.Field(19u, 0u));
        (float hotspot, float edge, float burst, float density) = (state.HotspotSize, DistanceField.Band(state.HotspotSoftness, extent), state.StarburstLength, state.StarburstDensity / float.Tau);
        float shimmer = state.Shimmer * state.Hold.Held(state.Timing.At(context));
        (Matrix3x2 turn, Vector2 halo, float width, float rim) =
            (Matrix3x2.CreateRotation((float)state.BarAngle.Radians), state.RingMagnification * at, state.RingWidth, DistanceField.Band(state.RingSoftness, extent));
        (float barLength, float barThickness) = (state.BarLength, (float)state.BarThickness.Pixels(extent));
        RampLight ghostTable = RampLight.Of(state.GhostColors, context);
        (Vector2 Center, float Flip, double Radius, double Ramp, double Bound, Vector3 Color)[] ghosts = [..
            from k in state.Ghosts.IsNone ? [] : Enumerable.Range(0, state.GhostCount)
            let u = CoordinateHash.Uniforms(Vector128.Create(k, 0, 0, 0), draws)
            let position = (k + 0.5f + (state.GhostVariation * (u.Y - 0.5f))) / state.GhostCount
            let m = float.Lerp(state.GhostFirst, state.GhostLast, position)
            let shrink = 1f - (state.GhostVariation * u.X)
            let radius = state.GhostSize.Pixels(extent) * shrink
            let ramp = (state.GhostSoftness * radius) + 1d
            let color = ghostTable.Sample(position)
            select (m * at, float.CopySign(1f, m), radius, ramp, (iris.Circumradius * radius) + (ramp / 2d), color.AsVector3() * (color.W / (shrink * shrink)))];
        Seq<(Option<float> Gain, Seq<RampStop> Colors, Func<Vector2, (float Tone, Vector3 Weight)> Shape)> elements =
            [(state.Hotspot, state.HotspotColors, Disc), (state.Starburst, state.StarburstColors, Streaks), (state.Bar, state.BarColors, Anamorphic), (state.Ring, state.RingColors, Halo)];
        return [..
            from element in elements
            from gain in element.Gain.ToSeq()
            let table = RampLight.Of(element.Colors, context)
            select new Func<Vector2, Vector3>(q => element.Shape(q) switch { var (tone, weight) => float.Exp2(gain) * weight * (table.Sample(tone) switch { var color => color.AsVector3() * color.W }) }),
            .. from gain in state.Ghosts.ToSeq() select new Func<Vector2, Vector3>(q => float.Exp2(gain) * Chain(q))];

        (float, Vector3) Disc(Vector2 q) => Vector2.Distance(q, at) switch {
            var r => (float.Min(r / hotspot, 1f), new Vector3(float.Min(DistanceField.Coverage((r - hotspot) * side, edge), 2f * hotspot * side / edge))),
        };

        (float, Vector3) Streaks(Vector2 q) => (q - at) switch {
            var d => float.Min(d.Length() / burst, 1f) switch {
                < 1f and var u => (u, new Vector3(float.Pow(float.Max(0f, (2f * Noise(d)) - 1f), state.StarburstSharpness) * (1f - u) * (1f - u))),
                var u => (u, Vector3.Zero),
            },
        };

        float Noise(Vector2 d) => float.SinCos(float.Atan2(d.Y, d.X)) switch { var (sin, cos) => basis(new Vector4(density * cos, density * sin, shimmer, 0f), field).Value };

        (float, Vector3) Anamorphic(Vector2 q) => (q - at) switch {
            var d => float.Min(float.Abs((d.X * turn.M11) + (d.Y * turn.M21)) / barLength, 1f) switch {
                var u => (u, new Vector3((1f - u) * (1f - u) * DistanceField.Stroke(float.Abs((d.X * turn.M12) + (d.Y * turn.M22)) * side, barThickness, 1f))),
            },
        };

        (float, Vector3) Halo(Vector2 q) => (Vector2.Distance(q, halo) - state.RingRadius) switch {
            var delta => (float.Clamp((delta / width) + 0.5f, 0f, 1f), new Vector3(DistanceField.Stroke(float.Abs(delta) * side, width * side, rim))),
        };

        Vector3 Chain(Vector2 q) {
            Vector3 sum = Vector3.Zero;
            foreach ((Vector2 center, float flip, double radius, double ramp, double bound, Vector3 color) in ghosts) {
                Vector2 offset = (q - center) * side;
                if (offset.Length() <= bound) {
                    (double reach, double outline) = iris.Outline(new Vector2(flip * offset.X, -flip * offset.Y));
                    sum += color * iris.Weights(reach, outline, radius, ramp, state.Fringe).AsVector3();
                }
            }
            return sum;
        }
    }
}

public sealed record LightRays(
    float Threshold, float Softness, FlareSource Source, ShortSideLength Size, float Jitter, Unicolour Tint,
    GeneratedLayer Layer, Seed Seed, Timing Timing) : IPixelStage<LightRays> {
    public static LightRays Default { get; } = new(
        1f, 0.1f, FlareSource.Default, ShortSideLength.Create(0.5f * float.Hypot(ReferenceFrame.GreaterSide, 1f)), 0f, new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d),
        GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Seq<GuideChannel> Channels(LightRays state) => state.Source.Channels;

    public static Fin<Option<PixelPass>> Pass(LightRays state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) =>
            frame.Spread(state.Threshold, state.Softness, light => Marched(state, context, frame, point, occlusion, light), state.Layer.Mode, 1f, [], progress));

    private static void Marched(LightRays state, PassContext context, PixelFrame frame, Vector2 source, Option<Occluder> occlusion, Mat light) {
        Span2D<Vector4> lit = light.View<Vector4>();
        if (occlusion.Case is Occluder occluder)
            Hidden(occluder, frame, lit);
        PixelFrame key = new(frame.Origin, frame.Size, frame.Extent, block => MemoryMarshal.Cast<Vector4, float>(light.GetSpan<Vector4>()).CopyTo(block));
        PixelFrame rays = new(frame.Origin, frame.Size, frame.Extent);
        rays.Rows(Rays(state, context, source, key));
        rays.View.Span.CopyTo(lit);
    }

    private static void Hidden(Occluder occluder, PixelFrame frame, Span2D<Vector4> lit) {
        for (int y = 0; y < lit.Height; y++) {
            for (int x = 0; x < lit.Width; x++)
                lit[y, x] = occluder.Hides(new Vector2(frame.Origin.X + x + 0.5f, frame.Origin.Y + y + 0.5f)) ? Vector4.Zero : lit[y, x];
        }
    }

    private static Action<Span<Vector4>, int, int> Rays(LightRays state, PassContext context, Vector2 source, PixelFrame key) {
        (float reach, float jitter, PixelExtent extent) = (float.Truncate(state.Size * context.Extent.ShortSide), state.Jitter, context.Extent);
        uint field = state.Seed.Field(20u, BitConverter.SingleToUInt32Bits(state.Timing.At(context)));
        Vector3 tint = RampLight.Light(state.Tint, context).AsVector3() * float.Exp2(state.Layer.Exposure);
        Func<int, int, float> shift = jitter is 0f ? static (_, _) => 0f : (x, y) => CoordinateHash.Uniforms(Vector128.Create(x, y, 0, 0), field).X;
        return (row, x, y) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = new Vector2(x + i + 0.5f, y + 0.5f) switch { var p => new Vector4(tint * (reach is 0f ? Sample(p) : Ray(p, shift(x + i, y))).AsVector3(), 0f) };
        };

        Vector4 Sample(Vector2 p) => key.Sample(p, (WrapMode.Black, WrapMode.Black));

        Vector4 Ray(Vector2 p, float offset) {
            float distance = float.Max(1f, Vector2.Distance(source, p));
            (float steps, Vector2 step) = (float.Floor(float.Min(reach, distance)), (source - p) / distance);
            (Vector4 sum, float total) = (Vector4.Zero, 0f);
            for (int k = 0; k <= (1f - jitter) * steps; k++) {
                float along = jitter is 1f ? 0f : (k + offset) / (1f - jitter);
                Vector2 t = p + (along * step);
                if (t.X < 0f || t.X > extent.Width || t.Y < 0f || t.Y > extent.Height)
                    break;
                float fall = (1f - (along / steps)) * (1f - (along / steps));
                (sum, total) = (sum + (fall * Sample(t)), total + fall);
            }
            return total is not 0f ? sum / total : Sample(p);
        }
    }
}
