using System.Numerics;
using CommunityToolkit.HighPerformance;
using UnitsNet;
using UnitsNet.Units;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Filters.Optics;

namespace Rasm.Imaging.Filters.Generators;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct RayDensity : IMinMaxValue<RayDensity> {
    public static RayDensity MinValue { get; } = new(1f);
    public static RayDensity MaxValue { get; } = new(100f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct RayJitter : IMinMaxValue<RayJitter> {
    public static RayJitter MinValue => Neutral;
    public static RayJitter MaxValue { get; } = new(float.BitDecrement(1f));

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<Guid>(SkipIParsable = true)]
[ValidationError<InvalidGenerator>]
public readonly partial struct LightTarget {
    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new InvalidGenerator() : null;
}

internal readonly record struct Occluder(PixelFrame Depth, float Source) {
    public bool Hides(Vector2 point) => Depth.Row(Depth.Extent.Height - 1 - (int)point.Y)[(int)point.X - Depth.Origin.X].X < Source;

    public float Visible(Vector2 source, ShortSideExtent radius, PixelExtent extent) {
        (float reach, Occluder occluder) = (radius.Pixels(extent), this);
        (int left, int right) = (int.Max(0, (int)float.Ceiling(source.X - reach - 0.5f)), int.Min(extent.Width - 1, (int)float.Floor(source.X + reach - 0.5f)));
        (int top, int bottom) = (int.Max(0, (int)float.Ceiling(source.Y - reach - 0.5f)), int.Min(extent.Height - 1, (int)float.Floor(source.Y + reach - 0.5f)));
        Seq<Vector2> disc = toSeq(
            from y in Enumerable.Range(top, int.Max(0, bottom - top + 1))
            from x in Enumerable.Range(left, int.Max(0, right - left + 1))
            let centre = new Vector2(x + 0.5f, y + 0.5f)
            where Vector2.Distance(centre, source) <= reach
            select centre);
        return disc.IsEmpty ? 1f : disc.Filter(point => !occluder.Hides(point)).Count / (float)disc.Count;
    }
}

public readonly record struct FlareSource(Vector2 Point, Option<float> Depth) {
    internal Option<Option<Occluder>> Gate(bool occlusion, PassContext context) =>
        occlusion
            ? Depth.Traverse(depth => context.Guides.Find(GuideChannel.Depth).Map(guide => new Occluder(guide, depth))).As()
            : Some(Option<Occluder>.None);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareAnchor {
    public static readonly FlareAnchor Frame = new("frame", [], static (position, _, context) =>
        Some(new FlareSource(position.Point(context.Extent), None)));
    public static readonly FlareAnchor Sun = new("sun", [GuideChannel.Depth], static (_, _, context) =>
        from camera in context.Camera
        from sun in context.Lights.Sun
        from located in camera.Frustum.Switch(
            (Camera: camera, Sun: sun),
            perspective: static (held, _) => held.Camera.Pixel(held.Camera.Location + held.Sun),
            parallel: static (_, _) => Option<(Vector2 Point, float Depth)>.None)
        select new FlareSource(located.Point, Some(float.PositiveInfinity)));
    public static readonly FlareAnchor Light = new("light", [GuideChannel.Depth], static (_, light, context) =>
        from target in light
        from point in context.Lights.Points.Find(target)
        from camera in context.Camera
        from located in camera.Pixel(point)
        select new FlareSource(located.Point, Some(located.Depth)));

    public Seq<GuideChannel> Occluders { get; }

    public Seq<GuideChannel> Channels(bool occlusion) => occlusion ? Occluders : [];

    [UseDelegateFromConstructor]
    public partial Option<FlareSource> Locate(FramePosition position, Option<LightTarget> light, PassContext context);
}

public sealed record LensFlare(
    FlareAnchor Anchor, FramePosition Source, Option<LightTarget> Light, bool Occlusion,
    Option<Exposure> Hotspot, ShortSideExtent HotspotSize, Option<Exposure> Starburst, ShortSideExtent StarburstLength,
    NoiseBasis Basis, RayDensity StarburstDensity, Frequency Shimmer, Option<Exposure> Bar, ShortSideExtent BarLength,
    Option<Exposure> Ring, ShortSideLength RingRadius, Option<Exposure> Ghosts, ShortSideExtent GhostSize, GhostIterations Iterations,
    Mix Modulation, Iris Iris, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LensFlare, LensFlareParameter, InvalidGenerator>, IPixelStage<LensFlare> {
    internal const float GreaterSide = 1920f / 1080f;
    private static readonly ShortSideExtent SourceRadius = ShortSideExtent.Create(0.2f * 0.3f / 2f * GreaterSide);

    public static LensFlare Default { get; } = new(
        FlareAnchor.Frame, new FramePosition(FrameAxis.Create(0.3f), FrameAxis.Create(1f - 0.6f)), None, Occlusion: true,
        Some(Exposure.Neutral), SourceRadius, Some(Exposure.Neutral), ShortSideExtent.Create(0.5f),
        NoiseBasis.Default with { Octaves = Octaves.Standard with { Roughness = AxisFraction.Create(0.563f) } }, RayDensity.Create(30f), Frequency.Neutral, Some(Exposure.Neutral), ShortSideExtent.Create(0.972f / 2f * GreaterSide),
        Some(Exposure.Neutral), ShortSideLength.Create(1f / 2.1f), Some(Exposure.Neutral), SourceRadius, GhostIterations.Three,
        Optics.Ghosts.Default.Modulation, Iris.Hexagon, GeneratedLayer.Added, Seed.MinValue, Timing.Standard);
    public static LensFlare NachoLens { get; } = Default with {
        Iris = new(ApertureBlades.Create(3), SignedAngle.Create(0.179f), AxisFraction.Create(0.332f), AxisFraction.MinValue, AnamorphicSqueeze.Spherical, ColorFringe.Create(-0.1f)),
    };
    public static LensFlare RingLens { get; } = Default with {
        Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.Create(0.796f), AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical, ColorFringe.Create(0.189f)),
    };
    public static LensFlare HexLens { get; } = Default with {
        Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.MinValue, AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical, ColorFringe.Create(0.052f)),
    };
    public static LensFlare RoundLens { get; } = Default with {
        Iris = new(ApertureBlades.Create(16), SignedAngle.Neutral, AxisFraction.MaxValue, AxisFraction.MinValue, AnamorphicSqueeze.Spherical, ColorFringe.Create(0.012f)),
    };

    public static Seq<GuideChannel> Channels(LensFlare state) => state.Anchor.Channels(state.Occlusion);

    public static Option<PixelPass> Pass(LensFlare state, PassContext context) =>
        Seq(state.Hotspot, state.Starburst, state.Bar, state.Ring, state.Ghosts).Exists(static element => element.IsSome)
            ? from source in state.Anchor.Locate(state.Source, state.Light, context)
              from gate in source.Gate(state.Occlusion, context)
              select gate.Match<PixelPass>(
                  Some: occluder => new PixelPass.Frame((frame, progress) =>
                      new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context, source.Point, occluder.Visible(source.Point, state.HotspotSize, context.Extent))))
                          .Run(frame, progress)),
                  None: () => new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context, source.Point, 1f))))
            : None;

    private static Action<Span<Vector4>, int, int> Fill(LensFlare state, PassContext context, Vector2 source, float visible) {
        (Vector2 frame, float side) = (new(context.Extent.Width, context.Extent.Height), int.Min(context.Extent.Width, context.Extent.Height));
        (Vector2 half, float spread) = (frame / 2f, 2f * (state.HotspotSize / 3f) * (state.HotspotSize / 3f));
        Vector2 c = (source - half) / side;
        (float burstLength, float barLength, float ringRadius, float density) = (state.StarburstLength, state.BarLength, state.RingRadius, state.StarburstDensity / float.Tau);
        (NoiseSampler basis, uint field, float shimmer) = (state.Basis.Sampler, CoordinateHash.Field(NoiseStream.LensFlare, state.Seed, 0u), state.Shimmer * (float)state.Timing.At(context));
        Iris iris = state.Iris;
        (Vector2 Centre, float Flip, float Radius, float Bound, float Scale, Vector3 Gain)[] copies = [..
            from i in Enumerable.Range(1, state.Iterations - 1)
            from copy in Optics.Ghosts.Copies(i, state.Iterations, state.Modulation)
            let scale = float.Abs(copy.Scale)
            let radius = state.GhostSize * side / scale
            select (c / copy.Scale, float.CopySign(1f, copy.Scale), radius, (iris.Circumradius * radius) + 1f, scale, copy.Gain.AsVector3())];
        Seq<(Option<Exposure> Gain, Func<Vector2, Vector3> Element)> elements =
            [(state.Hotspot, Hotspot), (state.Starburst, Starburst), (state.Bar, Bar), (state.Ring, Ring), (state.Ghosts, Chain)];
        Func<Vector2, Vector3>[] terms = [.. elements.Map(static element =>
            element.Gain.Map(static gain => gain.Scale).Map(scale => (Func<Vector2, Vector3>)(q => scale * element.Element(q)))).Somes()];
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                Vector2 q = (new Vector2(column + i + 0.5f, frame.Y - line - 0.5f) - half) / side;
                Vector3 light = Vector3.Zero;
                foreach (Func<Vector2, Vector3> term in terms)
                    light += term(q);
                row[i] = new Vector4(visible * light, 1f);
            }
        };

        Vector3 Hotspot(Vector2 q) => new(float.Exp(-Vector2.DistanceSquared(q, c) / spread));

        Vector3 Starburst(Vector2 q) {
            Vector2 d = q - c;
            float fall = 1f - (d.Length() / burstLength);
            return fall > 0f ? new Vector3(Burst(d) * fall * fall) : Vector3.Zero;
        }

        float Burst(Vector2 d) =>
            float.SinCos(float.Atan2(-d.Y, d.X)) switch {
                var (sin, cos) => basis.Sample(new Vector4(density * cos, density * sin, shimmer, 0f), field) switch { var n => n * n },
            };

        Vector3 Bar(Vector2 q) =>
            (q - c) switch {
                var d => float.Max(0f, 1f - (float.Abs(d.X) / barLength)) switch { var fall => new Vector3(float.Exp(-d.Y * d.Y / spread) * fall * fall) },
            };

        Vector3 Ring(Vector2 q) => (q.Length() - ringRadius) switch { var gap => new Vector3(float.Exp(-gap * gap / spread)) };

        Vector3 Chain(Vector2 q) {
            float reach = 2f * (q * side / frame).Length();
            Vector3 sum = Vector3.Zero;
            foreach ((Vector2 centre, float flip, float radius, float bound, float scale, Vector3 gain) in copies) {
                sum += ((q - centre) * side) switch {
                    var v when v.Length() <= bound => iris.Outline(flip * v) switch {
                        var (outline, edge) => iris.Weights(outline, edge, radius).AsVector3() * float.Max(0f, 1f - (reach * scale)) * gain,
                    },
                    _ => Vector3.Zero,
                };
            }
            return sum;
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFlareParameter : IStateParameter<LensFlare> {
    private static readonly (StateParameter<LensFlare> Clock, StateParameter<LensFlare> Pace) Time =
        Timing.Kinds(Lens<LensFlare, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    private static readonly (StateParameter<LensFlare> X, StateParameter<LensFlare> Y) Position =
        FramePosition.Kinds(Lens<LensFlare, FramePosition>.New(static flare => flare.Source, static source => flare => flare with { Source = source }));
    private static readonly Lens<LensFlare, Iris> Diaphragm = Lens<LensFlare, Iris>.New(static flare => flare.Iris, static iris => flare => flare with { Iris = iris });
    private static readonly (StateParameter<LensFlare> Blades, StateParameter<LensFlare> Rotation, StateParameter<LensFlare> Roundness, StateParameter<LensFlare> Obstruction, StateParameter<LensFlare> Squeeze)
        Outline = Iris.Kinds(Diaphragm);
    private static readonly (StateParameter<LensFlare> Mode, StateParameter<LensFlare> Exposure) Layer =
        GeneratedLayer.Kinds(Lens<LensFlare, GeneratedLayer>.New(static flare => flare.Layer, static layer => flare => flare with { Layer = layer }));
    private static readonly Presentation<ShortSideExtent, float> Spot = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.001f, 0.25f), Scale = TrackScale.Log };
    private static readonly Presentation<ShortSideExtent, float> Reach = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.01f, 2f), Scale = TrackScale.Log };

    public static readonly LensFlareParameter Anchor = new("anchor", new StateParameter<LensFlare>.Choice<FlareAnchor, InvalidGenerator>(
        Lens<LensFlare, FlareAnchor>.New(static flare => flare.Anchor, static anchor => flare => flare with { Anchor = anchor })));
    public static readonly LensFlareParameter SourceX = new("source-x", Position.X);
    public static readonly LensFlareParameter SourceY = new("source-y", Position.Y);
    public static readonly LensFlareParameter Light = new("light", new StateParameter<LensFlare>.OptionalKeyed<LightTarget, Guid, InvalidGenerator>(
        Lens<LensFlare, Option<LightTarget>>.New(static flare => flare.Light, static light => flare => flare with { Light = light })));
    public static readonly LensFlareParameter Occlusion = new("occlusion", new StateParameter<LensFlare>.Toggle(
        Lens<LensFlare, bool>.New(static flare => flare.Occlusion, static occlusion => flare => flare with { Occlusion = occlusion })));
    public static readonly LensFlareParameter Hotspot = new("hotspot", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Option<Exposure>>.New(static flare => flare.Hotspot, static gain => flare => flare with { Hotspot = gain }), Tone.Exposure.Presentation));
    public static readonly LensFlareParameter HotspotSize = new("hotspot-size", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static flare => flare.HotspotSize, static size => flare => flare with { HotspotSize = size }), Spot));
    public static readonly LensFlareParameter Starburst = new("starburst", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Option<Exposure>>.New(static flare => flare.Starburst, static gain => flare => flare with { Starburst = gain }), Tone.Exposure.Presentation));
    public static readonly LensFlareParameter StarburstLength = new("starburst-length", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static flare => flare.StarburstLength, static length => flare => flare with { StarburstLength = length }), Reach));
    public static readonly LensFlareParameter Basis = new("basis", new StateParameter<LensFlare>.Record<NoiseBasis>(
        Lens<LensFlare, NoiseBasis>.New(static state => state.Basis, static basis => state => state with { Basis = basis })));
    public static readonly LensFlareParameter StarburstDensity = new("starburst-density", new StateParameter<LensFlare>.Bounded<RayDensity, float, InvalidGenerator>(
        Lens<LensFlare, RayDensity>.New(static flare => flare.StarburstDensity, static density => flare => flare with { StarburstDensity = density }), new() { Scale = TrackScale.Log }));
    public static readonly LensFlareParameter Shimmer = new("shimmer", new StateParameter<LensFlare>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<LensFlare, Frequency>.New(static flare => flare.Shimmer, static shimmer => flare => flare with { Shimmer = shimmer }),
        Frequency.Presentation with { Soft = (0f, 10f) }));
    public static readonly LensFlareParameter Bar = new("bar", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Option<Exposure>>.New(static flare => flare.Bar, static gain => flare => flare with { Bar = gain }), Tone.Exposure.Presentation));
    public static readonly LensFlareParameter BarLength = new("bar-length", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static flare => flare.BarLength, static length => flare => flare with { BarLength = length }), Reach));
    public static readonly LensFlareParameter Ring = new("ring", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Option<Exposure>>.New(static flare => flare.Ring, static gain => flare => flare with { Ring = gain }), Tone.Exposure.Presentation));
    public static readonly LensFlareParameter RingRadius = new("ring-radius", new StateParameter<LensFlare>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideLength>.New(static flare => flare.RingRadius, static radius => flare => flare with { RingRadius = radius }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) }));
    public static readonly LensFlareParameter Ghosts = new("ghosts", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Option<Exposure>>.New(static flare => flare.Ghosts, static gain => flare => flare with { Ghosts = gain }), Tone.Exposure.Presentation));
    public static readonly LensFlareParameter GhostSize = new("ghost-size", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static flare => flare.GhostSize, static size => flare => flare with { GhostSize = size }), Spot));
    public static readonly LensFlareParameter Iterations = new("iterations", new StateParameter<LensFlare>.Bounded<GhostIterations, int, InvalidOptics>(
        Lens<LensFlare, GhostIterations>.New(static flare => flare.Iterations, static iterations => flare => flare with { Iterations = iterations }), new()));
    public static readonly LensFlareParameter ColorModulation = new("color-modulation", new StateParameter<LensFlare>.Bounded<Mix, float, InvalidGrade>(
        Lens<LensFlare, Mix>.New(static flare => flare.Modulation, static modulation => flare => flare with { Modulation = modulation }), new()));
    public static readonly LensFlareParameter Blades = new("blades", Outline.Blades);
    public static readonly LensFlareParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly LensFlareParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly LensFlareParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly LensFlareParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly LensFlareParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));
    public static readonly LensFlareParameter Mode = new("mode", Layer.Mode);
    public static readonly LensFlareParameter Exposure = new("exposure", Layer.Exposure);
    public static readonly LensFlareParameter Seed = new("seed", new StateParameter<LensFlare>.Bounded<Seed, int, InvalidGenerator>(
        Lens<LensFlare, Seed>.New(static flare => flare.Seed, static seed => flare => flare with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly LensFlareParameter Clock = new("clock", Time.Clock);
    public static readonly LensFlareParameter Pace = new("pace", Time.Pace);

    public StateParameter<LensFlare> Kind { get; }
}

public sealed record LightRays(
    HighlightKey Key, FlareAnchor Anchor, FramePosition Source, Option<LightTarget> Light, bool Occlusion,
    ShortSideLength Size, RayJitter Jitter, Swatch Tint, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LightRays, LightRaysParameter, InvalidGenerator>, IPixelStage<LightRays> {
    internal static float Diagonal => float.Hypot(LensFlare.GreaterSide, 1f);

    public static LightRays Default { get; } = new(
        Ghosts.Default.Key, FlareAnchor.Frame, FramePosition.Center, None, Occlusion: true,
        ShortSideLength.Create(0.5f * Diagonal), RayJitter.Neutral, Swatch.White, GeneratedLayer.Added, Seed.MinValue, Timing.Standard);

    public static Seq<GuideChannel> Channels(LightRays state) => state.Anchor.Channels(state.Occlusion);

    public static Option<PixelPass> Pass(LightRays state, PassContext context) =>
        from source in state.Anchor.Locate(state.Source, state.Light, context)
        from gate in source.Gate(state.Occlusion, context)
        select (PixelPass)new PixelPass.Frame((frame, progress) => {
            Func<Vector2, bool> hidden = gate.Match(Some: static occluder => (Func<Vector2, bool>)occluder.Hides, None: static () => static _ => false);
            PixelFrame key = new(frame.Origin, frame.Size, frame.Extent, block => {
                Span2D<Vector4> keys = block.AsSpan().Cast<float, Vector4>().AsSpan2D(frame.Size.Height, frame.Size.Width);
                for (int i = 0; i < keys.Height; i++) {
                    Span<Vector4> row = keys.GetRowSpan(i);
                    ReadOnlySpan<Vector4> light = frame.Row(frame.Line(i));
                    for (int x = 0; x < row.Length; x++)
                        row[x] = hidden(new Vector2(frame.Origin.X + x + 0.5f, frame.Origin.Y + i + 0.5f)) ? Vector4.Zero : state.Key.Extract(light[x]);
                }
            });
            return new PixelPass.Pointwise(state.Layer.Kernel(Fill(state, context, source.Point, key))).Run(frame, progress);
        });

    private static Action<Span<Vector4>, int, int> Fill(LightRays state, PassContext context, Vector2 source, PixelFrame key) {
        (float width, float height) = (context.Extent.Width, context.Extent.Height);
        (int reach, float jitter, bool jittered) = ((int)state.Size.Pixels(context.Extent), state.Jitter, state.Jitter != RayJitter.Neutral);
        (uint field, Vector3 tint) = (CoordinateHash.Field(NoiseStream.LightRays, state.Seed, state.Timing.At(context).Term), state.Tint.SceneLight(context.Working));
        ReadOnlyMemory2D<Vector4> texels = key.View;
        (WrapMode Across, WrapMode Down) wrap = (WrapMode.Black, WrapMode.Black);
        return (row, column, line) => {
            ReadOnlySpan2D<Vector4> plane = texels.Span;
            for (int i = 0; i < row.Length; i++) {
                Vector2 p = new(column + i + 0.5f, height - line - 0.5f);
                row[i] = new Vector4(tint * (reach == 0 ? plane.Sample(p, wrap) : Rays(plane, p, NoiseFunctions.White(new Vector4(column + i, line, 0f, 0f), field).X)).AsVector3(), 1f);
            }
        };

        Vector4 Rays(ReadOnlySpan2D<Vector4> plane, Vector2 p, float offset) {
            float distance = float.Max(1f, Vector2.Distance(source, p));
            (int steps, Vector2 step) = (int.Min(reach, (int)distance), (source - p) / distance);
            (Vector4 sum, float total) = (Vector4.Zero, 0f);
            for (int k = 0; k <= (int)((1f - jitter) * steps); k++) {
                float at = jittered ? (k + offset) / (1f - jitter) : k;
                Vector2 t = p + (at * step);
                if (t.X < 0f || t.Y < 0f || t.X > width || t.Y > height)
                    break;
                float weight = (1f - (at / steps)) * (1f - (at / steps));
                (sum, total) = (sum + (weight * plane.Sample(t, wrap)), total + weight);
            }
            return total != 0f ? sum / total : plane.Sample(p, wrap);
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightRaysParameter : IStateParameter<LightRays> {
    private static readonly (StateParameter<LightRays> Clock, StateParameter<LightRays> Pace) Time =
        Timing.Kinds(Lens<LightRays, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    private static readonly (StateParameter<LightRays> Threshold, StateParameter<LightRays> Softness) Highlight =
        HighlightKey.Kinds(Lens<LightRays, HighlightKey>.New(static rays => rays.Key, static key => rays => rays with { Key = key }));
    private static readonly (StateParameter<LightRays> X, StateParameter<LightRays> Y) Position =
        FramePosition.Kinds(Lens<LightRays, FramePosition>.New(static rays => rays.Source, static source => rays => rays with { Source = source }));
    private static readonly (StateParameter<LightRays> Mode, StateParameter<LightRays> Exposure) Layer =
        GeneratedLayer.Kinds(Lens<LightRays, GeneratedLayer>.New(static rays => rays.Layer, static layer => rays => rays with { Layer = layer }));

    public static readonly LightRaysParameter Threshold = new("threshold", Highlight.Threshold);
    public static readonly LightRaysParameter Softness = new("softness", Highlight.Softness);
    public static readonly LightRaysParameter Anchor = new("anchor", new StateParameter<LightRays>.Choice<FlareAnchor, InvalidGenerator>(
        Lens<LightRays, FlareAnchor>.New(static rays => rays.Anchor, static anchor => rays => rays with { Anchor = anchor })));
    public static readonly LightRaysParameter SourceX = new("source-x", Position.X);
    public static readonly LightRaysParameter SourceY = new("source-y", Position.Y);
    public static readonly LightRaysParameter Light = new("light", new StateParameter<LightRays>.OptionalKeyed<LightTarget, Guid, InvalidGenerator>(
        Lens<LightRays, Option<LightTarget>>.New(static rays => rays.Light, static light => rays => rays with { Light = light })));
    public static readonly LightRaysParameter Occlusion = new("occlusion", new StateParameter<LightRays>.Toggle(
        Lens<LightRays, bool>.New(static rays => rays.Occlusion, static occlusion => rays => rays with { Occlusion = occlusion })));
    public static readonly LightRaysParameter Size = new("size", new StateParameter<LightRays>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LightRays, ShortSideLength>.New(static rays => rays.Size, static size => rays => rays with { Size = size }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, LightRays.Diagonal) }));
    public static readonly LightRaysParameter Jitter = new("jitter", new StateParameter<LightRays>.Bounded<RayJitter, float, InvalidGenerator>(
        Lens<LightRays, RayJitter>.New(static rays => rays.Jitter, static jitter => rays => rays with { Jitter = jitter }), new()));
    public static readonly LightRaysParameter Tint = new("tint", new StateParameter<LightRays>.Color(
        Lens<LightRays, Swatch>.New(static rays => rays.Tint, static tint => rays => rays with { Tint = tint })));
    public static readonly LightRaysParameter Mode = new("mode", Layer.Mode);
    public static readonly LightRaysParameter Exposure = new("exposure", Layer.Exposure);
    public static readonly LightRaysParameter Seed = new("seed", new StateParameter<LightRays>.Bounded<Seed, int, InvalidGenerator>(
        Lens<LightRays, Seed>.New(static rays => rays.Seed, static seed => rays => rays with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly LightRaysParameter Clock = new("clock", Time.Clock);
    public static readonly LightRaysParameter Pace = new("pace", Time.Pace);

    public StateParameter<LightRays> Kind { get; }
}
