using System.Numerics;
using CommunityToolkit.HighPerformance;
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

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct RaySharpness : IMinMaxValue<RaySharpness> {
    public static RaySharpness MinValue { get; } = new(1f);
    public static RaySharpness MaxValue { get; } = new(8f);
    public static RaySharpness Standard { get; } = new(3f);

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

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct AxisMagnification : IMinMaxValue<AxisMagnification> {
    public static AxisMagnification MinValue { get; } = new(float.MinValue);
    public static AxisMagnification MaxValue { get; } = new(float.MaxValue);
    public static Presentation<AxisMagnification, float> Presentation { get; } = new() { Soft = (-2f, 2f), Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct GhostCount : IMinMaxValue<GhostCount> {
    public static GhostCount MinValue { get; } = new(1);
    public static GhostCount MaxValue { get; } = new(16);
    public static GhostCount Standard { get; } = new(5);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct GhostVariation : IMinMaxValue<GhostVariation> {
    public static GhostVariation MinValue { get; } = new(0f);
    public static GhostVariation MaxValue { get; } = new(0.9f);
    public static GhostVariation Standard { get; } = new(0.5f);

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
        Seq<Vector2> disc = toSeq(
            from y in Window(source.Y, extent.Height)
            from x in Window(source.X, extent.Width)
            let centre = new Vector2(x + 0.5f, y + 0.5f)
            where Vector2.Distance(centre, source) <= reach
            select centre);
        return disc.IsEmpty ? 1f : disc.Filter(point => !occluder.Hides(point)).Count / (float)disc.Count;

        IEnumerable<int> Window(float at, int size) =>
            (int)float.Clamp(float.Floor(at - reach), 0f, size) switch { var low => Enumerable.Range(low, (int)float.Clamp(float.Ceiling(at + reach), 0f, size) - low) };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareAnchor {
    public static readonly FlareAnchor Frame = new("frame", [], static (position, _, context) => Some((position.Point(context.Extent), Option<float>.None)));
    public static readonly FlareAnchor Sun = new("sun", [GuideChannel.Depth], static (_, _, context) => context.Lights.Sun.Bind(direction => Distant(direction, context)));
    public static readonly FlareAnchor Light = new("light", [GuideChannel.Depth], static (_, light, context) =>
        from target in light
        from source in (from point in context.Lights.Points.Find(target) from camera in context.Camera from located in camera.Pixel(point) select (located.Point, Depth: Some(located.Depth)))
            | context.Lights.Directions.Find(target).Bind(direction => Distant(direction, context))
        select source);

    public Seq<GuideChannel> Occluders { get; }

    [UseDelegateFromConstructor]
    public partial Option<(Vector2 Point, Option<float> Depth)> Locate(FramePosition position, Option<LightTarget> light, PassContext context);

    private static Option<(Vector2 Point, Option<float> Depth)> Distant(Vector3 direction, PassContext context) =>
        from camera in context.Camera
        from located in camera.Frustum.Map(perspective: camera.Pixel(camera.Location + direction), parallel: Option<(Vector2 Point, float Depth)>.None)
        select (located.Point, Some(float.PositiveInfinity));
}

public sealed record FlareSource(FlareAnchor Anchor, FramePosition Position, Option<LightTarget> Light, bool Occlusion) : IStateRecord<FlareSource, FlareSourceParameter, InvalidGenerator> {
    public static FlareSource Default { get; } = new(FlareAnchor.Frame, FramePosition.Center, None, Occlusion: true);

    internal Seq<GuideChannel> Channels => Occlusion ? Anchor.Occluders : [];

    internal Option<(Vector2 Point, Option<Occluder> Occluder)> Locate(PassContext context) =>
        from source in Anchor.Locate(Position, Light, context)
        from occluder in Occlusion ? source.Depth.Traverse(depth => context.Guides.Find(GuideChannel.Depth).Map(guide => new Occluder(guide, depth))).As() : Some(Option<Occluder>.None)
        select (source.Point, occluder);
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareSourceParameter : IStateParameter<FlareSource> {
    private static readonly (StateParameter<FlareSource> X, StateParameter<FlareSource> Y) Point =
        FramePosition.Kinds(Lens<FlareSource, FramePosition>.New(static s => s.Position, static v => s => s with { Position = v }));

    public static readonly FlareSourceParameter Anchor = new("anchor", new StateParameter<FlareSource>.Choice<FlareAnchor, InvalidGenerator>(Lens<FlareSource, FlareAnchor>.New(static s => s.Anchor, static v => s => s with { Anchor = v })));
    public static readonly FlareSourceParameter PositionX = new("position-x", Point.X);
    public static readonly FlareSourceParameter PositionY = new("position-y", Point.Y);
    public static readonly FlareSourceParameter Light = new("light", new StateParameter<FlareSource>.OptionalKeyed<LightTarget, Guid, InvalidGenerator>(
        Lens<FlareSource, Option<LightTarget>>.New(static s => s.Light, static v => s => s with { Light = v })));
    public static readonly FlareSourceParameter Occlusion = new("occlusion", new StateParameter<FlareSource>.Toggle(Lens<FlareSource, bool>.New(static s => s.Occlusion, static v => s => s with { Occlusion = v })));

    public StateParameter<FlareSource> Kind { get; }
}

public sealed record LensFlare(
    FlareSource Source, Gated<Exposure> Hotspot, Ramp HotspotColors, ShortSideExtent HotspotSize, ShortSideLength HotspotSoftness,
    Gated<Exposure> Starburst, Ramp StarburstColors, ShortSideExtent StarburstLength, NoiseBasis Basis, RayDensity StarburstDensity, RaySharpness StarburstSharpness, Frequency Shimmer, Hold Hold,
    Gated<Exposure> Bar, Ramp BarColors, ShortSideExtent BarLength, ShortSideLength BarThickness, SignedAngle BarAngle,
    Gated<Exposure> Ring, Ramp RingColors, ShortSideLength RingRadius, ShortSideLength RingWidth, ShortSideLength RingSoftness, AxisMagnification RingMagnification,
    Gated<Exposure> Ghosts, Ramp GhostColors, ShortSideExtent GhostSize, GhostCount GhostCount, AxisMagnification GhostFirst, AxisMagnification GhostLast, GhostVariation GhostVariation,
    AxisFraction GhostSoftness, Iris Iris, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LensFlare, LensFlareParameter, InvalidGenerator>, IPixelStage<LensFlare> {
    private static readonly ShortSideExtent SourceRadius = ShortSideExtent.Create(0.2f * 0.3f / 2f * ReferenceFrame.GreaterSide);

    public static LensFlare Default { get; } = new(
        FlareSource.Default with { Position = new(FrameAxis.Create(0.3f), FrameAxis.Create(1f - 0.6f)) },
        new(Enabled: true, Exposure.Neutral), Ramp.White, SourceRadius, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide),
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideExtent.Create(0.5f), NoiseBasis.Default with { Octaves = Octaves.Default with { Roughness = AxisFraction.Create(0.563f) } },
        RayDensity.Create(30f), RaySharpness.Standard, Frequency.Neutral, Hold.MinValue,
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideExtent.Create(0.972f / 2f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), SignedAngle.Neutral,
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideLength.Create(1f / 2.1f), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide),
        AxisMagnification.Neutral, new(Enabled: true, Exposure.Neutral), Ramp.White, SourceRadius, GhostCount.Standard, AxisMagnification.Create(0.6f), AxisMagnification.Create(-1f), GhostVariation.Standard,
        AxisFraction.Create(0.3f), Iris.Hexagon, GeneratedLayer.Added, Seed.MinValue, Timing.Default);
    public static LensFlare NachoLens { get; } = Default with { Iris = new(ApertureBlades.Create(3), SignedAngle.Create(0.179f), AxisFraction.Create(0.332f), AxisFraction.MinValue, AnamorphicSqueeze.Spherical, ColorFringe.Create(-0.1f)) };
    public static LensFlare RingLens { get; } = Default with { Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.Create(0.796f), AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical, ColorFringe.Create(0.189f)) };
    public static LensFlare HexLens { get; } = Default with { Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.MinValue, AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical, ColorFringe.Create(0.052f)) };
    public static LensFlare RoundLens { get; } = Default with { Iris = new(ApertureBlades.Create(16), SignedAngle.Neutral, AxisFraction.MaxValue, AxisFraction.MinValue, AnamorphicSqueeze.Spherical, ColorFringe.Create(0.012f)) };

    public static Seq<GuideChannel> Channels(LensFlare state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LensFlare state, PassContext context) =>
        from source in state.Source.Locate(context)
        from fill in Fill(state, context, source.Point)
        select source.Occluder.Match(
            Some: occluder => new PixelPass.Frame((frame, progress) => state.Layer.Pass(fill(occluder.Visible(source.Point, state.HotspotSize, context.Extent))).Run(frame, progress)),
            None: () => state.Layer.Pass(fill(1f)));

    private static Option<Func<float, Action<Span<Vector4>, int, int>>> Fill(LensFlare state, PassContext context, Vector2 source) {
        (FieldFrame frame, float side, Iris iris) = (Placement.Default.Frame(context.Extent), context.Extent.ShortSide, state.Iris);
        (Vector2 c, float first, float last, uint draws) = ((source - frame.Half) * frame.Scale * new Vector2(1f, -1f), state.GhostFirst, state.GhostLast, CoordinateHash.Field(NoiseStream.LensFlare, state.Seed, 1u));
        (float hotspot, float edge, float burst, float density, float sharpness) =
            (state.HotspotSize, ShapeEdge.Band(state.HotspotSoftness, context.Extent), state.StarburstLength, state.StarburstDensity / float.Tau, state.StarburstSharpness);
        (Func<Vector4, uint, NoiseSample> basis, uint field, float shimmer) =
            (state.Basis.Sampler, CoordinateHash.Field(NoiseStream.LensFlare, state.Seed, 0u), state.Shimmer * state.Hold.Held(state.Timing.At(context)));
        (Matrix3x2 turn, Vector2 halo, float width, float rim) = (Matrix3x2.CreateRotation(state.BarAngle), state.RingMagnification * c, state.RingWidth, ShapeEdge.Band(state.RingSoftness, context.Extent));
        (Vector2 Centre, float Flip, float Radius, float Ramp, float Bound, float Weight)[] ghosts = [..
            from k in Enumerable.Range(0, state.GhostCount)
            let u = NoiseFunctions.White(new Vector4(k, 0f, 0f, 0f), draws)
            let m = float.Lerp(first, last, (k + 0.5f + (state.GhostVariation * (u.Y - 0.5f))) / state.GhostCount)
            let shrink = 1f - (state.GhostVariation * u.X)
            let radius = state.GhostSize * shrink * side
            let ramp = (state.GhostSoftness * radius) + 1f
            select (m * c, float.CopySign(1f, m), radius, ramp, (iris.Circumradius * radius) + (ramp / 2f), 1f / (shrink * shrink))];
        Seq<(Gated<Exposure> Gain, Ramp Colors, Func<Vector2, (float Tone, Vector3 Weight)> Shape)> elements =
            [(state.Hotspot, state.HotspotColors, Disc), (state.Starburst, state.StarburstColors, Streaks), (state.Bar, state.BarColors, Anamorphic), (state.Ring, state.RingColors, Halo), (state.Ghosts, state.GhostColors, Chain)];
        Func<Vector2, Vector3>[] terms = [..
            from element in elements
            from gain in element.Gain.Active.ToSeq()
            let table = element.Colors.Tabulate(context.Working)
            select (Func<Vector2, Vector3>)(q => element.Shape(q) switch { var (tone, weight) => gain.Scale * weight * (table.Sample(tone) switch { var color => color.AsVector3() * color.W }) })];
        return terms.Length == 0 ? None : Some<Func<float, Action<Span<Vector4>, int, int>>>(visible => frame.Fill(q => {
            Vector3 light = Vector3.Zero;
            foreach (Func<Vector2, Vector3> term in terms)
                light += term(q);
            return new Vector4(visible * light, 1f);
        }));

        (float, Vector3) Disc(Vector2 q) => Vector2.Distance(q, c) switch {
            var r => (float.Min(r / hotspot, 1f), new Vector3(ShapeEdge.Coverage((r - hotspot) * side, edge))),
        };

        (float, Vector3) Streaks(Vector2 q) => (q - c, float.Min(Vector2.Distance(q, c) / burst, 1f)) switch {
            (var d, < 1f and var u) => (u, new Vector3(MathF.Pow(float.Max(0f, (2f * Noise(d)) - 1f), sharpness) * (1f - u) * (1f - u))),
            (_, var u) => (u, Vector3.Zero),
        };

        float Noise(Vector2 d) => float.SinCos(float.Atan2(d.Y, d.X)) switch { var (sin, cos) => basis(new Vector4(density * cos, density * sin, shimmer, 0f), field).Value };

        (float, Vector3) Anamorphic(Vector2 q) => Vector2.Transform(q - c, turn) switch {
            var along => float.Min(float.Abs(along.X) / state.BarLength, 1f) switch {
                var u => (u, new Vector3((1f - u) * (1f - u) * ShapeEdge.Coverage((float.Abs(along.Y) - (state.BarThickness / 2f)) * side, 1f))),
            },
        };

        (float, Vector3) Halo(Vector2 q) => (Vector2.Distance(q, halo) - state.RingRadius) switch {
            var delta => (Easing.Saturate((delta / width) + 0.5f), new Vector3(ShapeEdge.Coverage((float.Abs(delta) - (width / 2f)) * side, rim))),
        };

        (float, Vector3) Chain(Vector2 q) {
            Vector3 sum = Vector3.Zero;
            foreach ((Vector2 centre, float flip, float radius, float ramp, float bound, float weight) in ghosts) {
                sum += ((q - centre) * side) switch {
                    var v when v.Length() <= bound => iris.Outline(flip * v) switch { var (reach, outline) => weight * iris.Weights(reach, outline, radius, ramp).AsVector3() },
                    _ => Vector3.Zero,
                };
            }
            return (Easing.Saturate(((Vector2.Dot(q, c) / c.LengthSquared()) - first) / (last - first)), sum);
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFlareParameter : IStateParameter<LensFlare> {
    private static readonly Lens<LensFlare, Iris> Diaphragm = Lens<LensFlare, Iris>.New(static s => s.Iris, static v => s => s with { Iris = v });
    private static readonly (StateParameter<LensFlare> Blades, StateParameter<LensFlare> Rotation, StateParameter<LensFlare> Roundness, StateParameter<LensFlare> Obstruction, StateParameter<LensFlare> Squeeze)
        Outline = Iris.Kinds(Diaphragm);
    private static readonly Presentation<ShortSideExtent, float> Spot = ShortSideExtent.Presentation with { Soft = (0.001f, 0.25f) };
    private static readonly Presentation<ShortSideExtent, float> Reach = ShortSideExtent.Presentation with { Soft = (0.01f, 2f) };
    private static readonly Presentation<ShortSideLength, float> Thin = ShortSideLength.Presentation with { Soft = (0.001f, 0.25f), Scale = TrackScale.Log };

    public static readonly LensFlareParameter Source = new("source", new StateParameter<LensFlare>.Record<FlareSource>(Lens<LensFlare, FlareSource>.New(static s => s.Source, static v => s => s with { Source = v })));
    public static readonly LensFlareParameter Hotspot = new("hotspot", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Gated<Exposure>>.New(static s => s.Hotspot, static v => s => s with { Hotspot = v }), Exposure.Presentation));
    public static readonly LensFlareParameter HotspotColors = new("hotspot-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.HotspotColors, static v => s => s with { HotspotColors = v })));
    public static readonly LensFlareParameter HotspotSize = new("hotspot-size", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static s => s.HotspotSize, static v => s => s with { HotspotSize = v }), Spot));
    public static readonly LensFlareParameter HotspotSoftness = new("hotspot-softness", new StateParameter<LensFlare>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideLength>.New(static s => s.HotspotSoftness, static v => s => s with { HotspotSoftness = v }), Thin));
    public static readonly LensFlareParameter Starburst = new("starburst", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Gated<Exposure>>.New(static s => s.Starburst, static v => s => s with { Starburst = v }), Exposure.Presentation));
    public static readonly LensFlareParameter StarburstColors = new("starburst-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.StarburstColors, static v => s => s with { StarburstColors = v })));
    public static readonly LensFlareParameter StarburstLength = new("starburst-length", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static s => s.StarburstLength, static v => s => s with { StarburstLength = v }), Reach));
    public static readonly LensFlareParameter Basis = new("basis", new StateParameter<LensFlare>.Record<NoiseBasis>(Lens<LensFlare, NoiseBasis>.New(static s => s.Basis, static v => s => s with { Basis = v })));
    public static readonly LensFlareParameter StarburstDensity = new("starburst-density", new StateParameter<LensFlare>.Bounded<RayDensity, float, InvalidGenerator>(
        Lens<LensFlare, RayDensity>.New(static s => s.StarburstDensity, static v => s => s with { StarburstDensity = v }), new() { Scale = TrackScale.Log }));
    public static readonly LensFlareParameter StarburstSharpness = new("starburst-sharpness", new StateParameter<LensFlare>.Bounded<RaySharpness, float, InvalidGenerator>(
        Lens<LensFlare, RaySharpness>.New(static s => s.StarburstSharpness, static v => s => s with { StarburstSharpness = v }), new()));
    public static readonly LensFlareParameter Shimmer = new("shimmer", new StateParameter<LensFlare>.Bounded<Frequency, float, InvalidGenerator>(
        Lens<LensFlare, Frequency>.New(static s => s.Shimmer, static v => s => s with { Shimmer = v }), Frequency.Presentation with { Soft = (0f, 10f) }));
    public static readonly LensFlareParameter Hold = new("hold", new StateParameter<LensFlare>.Bounded<Hold, float, InvalidGenerator>(
        Lens<LensFlare, Hold>.New(static s => s.Hold, static v => s => s with { Hold = v }), Generators.Hold.Presentation));
    public static readonly LensFlareParameter Bar = new("bar", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Gated<Exposure>>.New(static s => s.Bar, static v => s => s with { Bar = v }), Exposure.Presentation));
    public static readonly LensFlareParameter BarColors = new("bar-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.BarColors, static v => s => s with { BarColors = v })));
    public static readonly LensFlareParameter BarLength = new("bar-length", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static s => s.BarLength, static v => s => s with { BarLength = v }), Reach));
    public static readonly LensFlareParameter BarThickness = new("bar-thickness", new StateParameter<LensFlare>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideLength>.New(static s => s.BarThickness, static v => s => s with { BarThickness = v }), Thin));
    public static readonly LensFlareParameter BarAngle = new("bar-angle", new StateParameter<LensFlare>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<LensFlare, SignedAngle>.New(static s => s.BarAngle, static v => s => s with { BarAngle = v }), SignedAngle.Presentation));
    public static readonly LensFlareParameter Ring = new("ring", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Gated<Exposure>>.New(static s => s.Ring, static v => s => s with { Ring = v }), Exposure.Presentation));
    public static readonly LensFlareParameter RingColors = new("ring-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.RingColors, static v => s => s with { RingColors = v })));
    public static readonly LensFlareParameter RingRadius = new("ring-radius", new StateParameter<LensFlare>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideLength>.New(static s => s.RingRadius, static v => s => s with { RingRadius = v }), ShortSideLength.Presentation));
    public static readonly LensFlareParameter RingWidth = new("ring-width", new StateParameter<LensFlare>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideLength>.New(static s => s.RingWidth, static v => s => s with { RingWidth = v }), Thin));
    public static readonly LensFlareParameter RingSoftness = new("ring-softness", new StateParameter<LensFlare>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideLength>.New(static s => s.RingSoftness, static v => s => s with { RingSoftness = v }), Thin));
    public static readonly LensFlareParameter RingMagnification = new("ring-magnification", new StateParameter<LensFlare>.Bounded<AxisMagnification, float, InvalidGenerator>(
        Lens<LensFlare, AxisMagnification>.New(static s => s.RingMagnification, static v => s => s with { RingMagnification = v }), AxisMagnification.Presentation));
    public static readonly LensFlareParameter Ghosts = new("ghosts", new StateParameter<LensFlare>.OptionalBounded<Exposure, float, InvalidToneValue>(
        Lens<LensFlare, Gated<Exposure>>.New(static s => s.Ghosts, static v => s => s with { Ghosts = v }), Exposure.Presentation));
    public static readonly LensFlareParameter GhostColors = new("ghost-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.GhostColors, static v => s => s with { GhostColors = v })));
    public static readonly LensFlareParameter GhostSize = new("ghost-size", new StateParameter<LensFlare>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<LensFlare, ShortSideExtent>.New(static s => s.GhostSize, static v => s => s with { GhostSize = v }), Spot));
    public static readonly LensFlareParameter GhostCount = new("ghost-count", new StateParameter<LensFlare>.Bounded<GhostCount, int, InvalidGenerator>(
        Lens<LensFlare, GhostCount>.New(static s => s.GhostCount, static v => s => s with { GhostCount = v }), new() { Step = 1 }));
    public static readonly LensFlareParameter GhostFirst = new("ghost-first", new StateParameter<LensFlare>.Bounded<AxisMagnification, float, InvalidGenerator>(
        Lens<LensFlare, AxisMagnification>.New(static s => s.GhostFirst, static v => s => s with { GhostFirst = v }), AxisMagnification.Presentation));
    public static readonly LensFlareParameter GhostLast = new("ghost-last", new StateParameter<LensFlare>.Bounded<AxisMagnification, float, InvalidGenerator>(
        Lens<LensFlare, AxisMagnification>.New(static s => s.GhostLast, static v => s => s with { GhostLast = v }), AxisMagnification.Presentation));
    public static readonly LensFlareParameter GhostVariation = new("ghost-variation", new StateParameter<LensFlare>.Bounded<GhostVariation, float, InvalidGenerator>(
        Lens<LensFlare, GhostVariation>.New(static s => s.GhostVariation, static v => s => s with { GhostVariation = v }), new()));
    public static readonly LensFlareParameter GhostSoftness = new("ghost-softness", new StateParameter<LensFlare>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<LensFlare, AxisFraction>.New(static s => s.GhostSoftness, static v => s => s with { GhostSoftness = v }), new()));
    public static readonly LensFlareParameter Blades = new("blades", Outline.Blades);
    public static readonly LensFlareParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly LensFlareParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly LensFlareParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly LensFlareParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly LensFlareParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));
    public static readonly LensFlareParameter Layer = new("layer", new StateParameter<LensFlare>.Record<GeneratedLayer>(Lens<LensFlare, GeneratedLayer>.New(static s => s.Layer, static v => s => s with { Layer = v })));
    public static readonly LensFlareParameter Seed = new("seed", new StateParameter<LensFlare>.Bounded<Seed, int, InvalidGenerator>(
        Lens<LensFlare, Seed>.New(static s => s.Seed, static v => s => s with { Seed = v }), Generators.Seed.Presentation));
    public static readonly LensFlareParameter Timing = new("timing", new StateParameter<LensFlare>.Record<Timing>(Lens<LensFlare, Timing>.New(static s => s.Timing, static v => s => s with { Timing = v })));

    public StateParameter<LensFlare> Kind { get; }
}

public sealed record LightRays(HighlightKey Key, FlareSource Source, ShortSideLength Size, RayJitter Jitter, Swatch Tint, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LightRays, LightRaysParameter, InvalidGenerator>, IPixelStage<LightRays> {
    internal static float Diagonal => float.Hypot(ReferenceFrame.GreaterSide, 1f);

    public static LightRays Default { get; } =
        new(Ghosts.Default.Key, FlareSource.Default, ShortSideLength.Create(0.5f * Diagonal), RayJitter.Neutral, Swatch.White, GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Seq<GuideChannel> Channels(LightRays state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LightRays state, PassContext context) =>
        from source in state.Source.Locate(context)
        select (PixelPass)new PixelPass.Frame((frame, progress) => {
            Func<Vector2, bool> hidden = source.Occluder.Match(Some: static occluder => (Func<Vector2, bool>)occluder.Hides, None: static () => static _ => false);
            PixelFrame key = new(frame.Origin, frame.Size, frame.Extent, block => {
                Span2D<Vector4> keys = block.AsSpan().Cast<float, Vector4>().AsSpan2D(frame.Size.Height, frame.Size.Width);
                ReadOnlySpan2D<Vector4> light = frame.View.Span;
                for (int y = 0; y < keys.Height; y++) {
                    for (int x = 0; x < keys.Width; x++)
                        keys[y, x] = hidden(new Vector2(frame.Origin.X + x + 0.5f, frame.Origin.Y + y + 0.5f)) ? Vector4.Zero : state.Key.Extract(light[y, x]);
                }
            });
            return state.Layer.Pass(Fill(state, context, source.Point, key)).Run(frame, progress);
        });

    private static Action<Span<Vector4>, int, int> Fill(LightRays state, PassContext context, Vector2 source, PixelFrame key) {
        (Vector2 frame, int reach, float jitter) = (new Vector2(context.Extent.Width, context.Extent.Height), (int)state.Size.Pixels(context.Extent), state.Jitter);
        (uint field, Vector3 tint, ReadOnlyMemory2D<Vector4> texels) = (CoordinateHash.Field(NoiseStream.LightRays, state.Seed, state.Timing.At(context).Term), state.Tint.SceneLight(context.Working), key.View);
        (WrapMode Across, WrapMode Down) wrap = (WrapMode.Black, WrapMode.Black);
        Func<int, int, float> shift = state.Jitter == RayJitter.Neutral ? static (_, _) => 0f : (column, line) => NoiseFunctions.White(new Vector4(column, line, 0f, 0f), field).X;
        return (row, column, line) => {
            ReadOnlySpan2D<Vector4> plane = texels.Span;
            for (int i = 0; i < row.Length; i++) {
                Vector2 p = new(column + i + 0.5f, frame.Y - line - 0.5f);
                row[i] = new Vector4(tint * (reach == 0 ? plane.Sample(p, wrap) : Rays(plane, p, shift(column + i, line))).AsVector3(), 1f);
            }
        };

        Vector4 Rays(ReadOnlySpan2D<Vector4> plane, Vector2 p, float offset) {
            float distance = float.Max(1f, Vector2.Distance(source, p));
            (float steps, Vector2 step) = (float.Floor(float.Min(reach, distance)), (source - p) / distance);
            (Vector4 sum, float total) = (Vector4.Zero, 0f);
            for (int k = 0; k <= (int)((1f - jitter) * steps); k++) {
                float at = (k + offset) / (1f - jitter);
                Vector2 t = p + (at * step);
                if (Vector2.Clamp(t, Vector2.Zero, frame) != t)
                    break;
                float fall = 1f - (at / steps);
                (sum, total) = (sum + (fall * fall * plane.Sample(t, wrap)), total + (fall * fall));
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
    private static readonly (StateParameter<LightRays> Threshold, StateParameter<LightRays> Softness) Highlight =
        HighlightKey.Kinds(Lens<LightRays, HighlightKey>.New(static s => s.Key, static v => s => s with { Key = v }));

    public static readonly LightRaysParameter Threshold = new("threshold", Highlight.Threshold);
    public static readonly LightRaysParameter Softness = new("softness", Highlight.Softness);
    public static readonly LightRaysParameter Source = new("source", new StateParameter<LightRays>.Record<FlareSource>(Lens<LightRays, FlareSource>.New(static s => s.Source, static v => s => s with { Source = v })));
    public static readonly LightRaysParameter Size = new("size", new StateParameter<LightRays>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<LightRays, ShortSideLength>.New(static s => s.Size, static v => s => s with { Size = v }), ShortSideLength.Presentation with { Soft = (0f, LightRays.Diagonal) }));
    public static readonly LightRaysParameter Jitter = new("jitter", new StateParameter<LightRays>.Bounded<RayJitter, float, InvalidGenerator>(
        Lens<LightRays, RayJitter>.New(static s => s.Jitter, static v => s => s with { Jitter = v }), new()));
    public static readonly LightRaysParameter Tint = new("tint", new StateParameter<LightRays>.Color(Lens<LightRays, Swatch>.New(static s => s.Tint, static v => s => s with { Tint = v })));
    public static readonly LightRaysParameter Layer = new("layer", new StateParameter<LightRays>.Record<GeneratedLayer>(Lens<LightRays, GeneratedLayer>.New(static s => s.Layer, static v => s => s with { Layer = v })));
    public static readonly LightRaysParameter Seed = new("seed", new StateParameter<LightRays>.Bounded<Seed, int, InvalidGenerator>(
        Lens<LightRays, Seed>.New(static s => s.Seed, static v => s => s with { Seed = v }), Generators.Seed.Presentation));
    public static readonly LightRaysParameter Timing = new("timing", new StateParameter<LightRays>.Record<Timing>(Lens<LightRays, Timing>.New(static s => s.Timing, static v => s => s with { Timing = v })));

    public StateParameter<LightRays> Kind { get; }
}
