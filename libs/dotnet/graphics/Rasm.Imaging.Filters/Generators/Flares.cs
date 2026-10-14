using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using PeterO.Numbers;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Filters.Optics;

namespace Rasm.Imaging.Filters.Generators;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct RayDensity : IMinMaxValue<RayDensity> {
    public static RayDensity MinValue { get; } = new(0f);
    public static RayDensity MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct RaySharpness : IMinMaxValue<RaySharpness> {
    public static RaySharpness MinValue { get; } = new(0f);
    public static RaySharpness MaxValue { get; } = new(float.MaxValue);
    public static RaySharpness Standard { get; } = new(3f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct RayJitter : IMinMaxValue<RayJitter> {
    public static RayJitter MinValue => Neutral;
    public static RayJitter MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct AxisMagnification : IMinMaxValue<AxisMagnification> {
    public static AxisMagnification MinValue { get; } = new(float.MinValue);
    public static AxisMagnification MaxValue { get; } = new(float.MaxValue);
    public static Presentation<float> Presentation { get; } = new() { Soft = (-2f, 2f), Origin = 0f };

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct GhostCount : IMinMaxValue<GhostCount> {
    public static GhostCount MinValue { get; } = new(1);
    public static GhostCount MaxValue { get; } = new(int.MaxValue);
    public static GhostCount Standard { get; } = new(5);

    static partial void ValidateFactoryArguments(ref InvalidGenerator? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGenerator();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGenerator>]
public readonly partial struct GhostVariation : IMinMaxValue<GhostVariation> {
    public static GhostVariation MinValue { get; } = new(0f);
    public static GhostVariation MaxValue { get; } = new(float.BitDecrement(1f));
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
    public bool Hides((double X, double Y) point) =>
        Depth.Sample((double.Floor(point.X) + 0.5d, double.Floor(point.Y) + 0.5d), (WrapMode.Black, WrapMode.Black),
            float.PositiveInfinity, static (_, sample, _) => sample.X) < Source;

    public float Visible((double X, double Y) source, ShortSideLength radius, PixelExtent extent) {
        if (radius == ShortSideLength.Neutral) return 1f;
        (double reach, Occluder occluder) = ((double)(float)radius * extent.ShortSide, this);
        Seq<(double X, double Y)> disc = toSeq(
            from y in Window(source.Y, extent.Height)
            from x in Window(source.X, extent.Width)
            let centre = (X: x + 0.5d, Y: y + 0.5d)
            where double.Hypot(centre.X - source.X, centre.Y - source.Y) <= reach
            select centre);
        return disc.IsEmpty ? 1f : disc.Filter(point => !occluder.Hides(point)).Count / (float)disc.Count;

        IEnumerable<int> Window(double at, int size) =>
            (int)double.Clamp(double.Floor(at - reach), 0d, size) switch { var low => Enumerable.Range(low, (int)double.Clamp(double.Ceiling(at + reach), 0d, size) - low) };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGenerator>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareAnchor {
    public static readonly FlareAnchor Frame = new("frame", [], static (position, _, context) =>
        Fin.Succ(Some((position.Point(context.Extent), Option<float>.None))));
    public static readonly FlareAnchor Sun = new("sun", [GuideChannel.Depth], static (_, _, context) =>
        context.Lights.ToFin(new SceneLightsUnavailable()).Map(lights => lights.Sun.Bind(direction => Distant(direction, context))));
    public static readonly FlareAnchor Light = new("light", [GuideChannel.Depth], static (_, light, context) =>
        light.Match(
            Some: target => context.Lights.ToFin(new SceneLightsUnavailable()).Map(lights =>
                (from point in lights.Points.Find(target) from camera in context.Camera from located in camera.Pixel(point) select (located.Point, Depth: Some(located.Depth)))
                | lights.Directions.Find(target).Bind(direction => Distant(direction, context))),
            None: static () => Fin.Succ(Option<((double X, double Y) Point, Option<float> Depth)>.None)));

    public Seq<GuideChannel> Occluders { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Option<((double X, double Y) Point, Option<float> Depth)>> Locate(FramePosition position, Option<LightTarget> light, PassContext context);

    private static Option<((double X, double Y) Point, Option<float> Depth)> Distant(Vector3 direction, PassContext context) =>
        from camera in context.Camera
        from located in camera.Frustum.Map(perspective: camera.Pixel(camera.Location + direction), parallel: Option<((double X, double Y) Point, float Depth)>.None)
        select (located.Point, Some(float.PositiveInfinity));
}

public sealed record FlareSource(FlareAnchor Anchor, FramePosition Position, Option<LightTarget> Light, bool Occlusion) : IStateRecord<FlareSource, FlareSourceParameter, InvalidGenerator> {
    public static FlareSource Default { get; } = new(FlareAnchor.Frame, FramePosition.Center, None, Occlusion: true);

    internal Seq<GuideChannel> Channels => Occlusion ? Anchor.Occluders : [];

    internal Fin<Option<((double X, double Y) Point, Option<Occluder> Occluder)>> Locate(PassContext context) =>
        Anchor.Locate(Position, Light, context).Map(location =>
            from source in location
            from occluder in Occlusion ? source.Depth.Traverse(depth => context.Guides.Find(GuideChannel.Depth).Map(guide => new Occluder(guide, depth))).As() : Some(Option<Occluder>.None)
            select (source.Point, occluder));
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
    FlareSource Source, Gated<Exposure> Hotspot, Ramp HotspotColors, ShortSideLength HotspotSize, ShortSideLength HotspotSoftness,
    Gated<Exposure> Starburst, Ramp StarburstColors, ShortSideLength StarburstLength, NoiseBasis Basis, RayDensity StarburstDensity, RaySharpness StarburstSharpness, Frequency Shimmer, Hold Hold,
    Gated<Exposure> Bar, Ramp BarColors, ShortSideLength BarLength, ShortSideLength BarThickness, SignedAngle BarAngle,
    Gated<Exposure> Ring, Ramp RingColors, ShortSideLength RingRadius, ShortSideLength RingWidth, ShortSideLength RingSoftness, AxisMagnification RingMagnification,
    Gated<Exposure> Ghosts, Ramp GhostColors, ShortSideLength GhostSize, GhostCount GhostCount, AxisMagnification GhostFirst, AxisMagnification GhostLast, GhostVariation GhostVariation,
    AxisFraction GhostSoftness, Iris Iris, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LensFlare, LensFlareParameter, InvalidGenerator>, IPixelStage<LensFlare> {
    private static readonly ShortSideLength SourceRadius = ShortSideLength.Create(0.2f * 0.3f / 2f * ReferenceFrame.GreaterSide);

    public static LensFlare Default { get; } = new(
        FlareSource.Default with { Position = new(FrameAxis.Create(0.3f), FrameAxis.Create(1f - 0.6f)) },
        new(Enabled: true, Exposure.Neutral), Ramp.White, SourceRadius, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide),
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideLength.Create(0.5f), NoiseBasis.Default with { Octaves = Octaves.Default with { Roughness = AxisFraction.Create(0.563f) } },
        RayDensity.Create(30f), RaySharpness.Standard, Frequency.Neutral, Hold.MinValue,
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideLength.Create(0.972f / 2f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), SignedAngle.Neutral,
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideLength.Create(1f / 2.1f), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide),
        AxisMagnification.Neutral, new(Enabled: true, Exposure.Neutral), Ramp.White, SourceRadius, GhostCount.Standard, AxisMagnification.Create(0.6f), AxisMagnification.Create(-1f), GhostVariation.Standard,
        AxisFraction.Create(0.3f), Iris.Hexagon, GeneratedLayer.Added, Seed.MinValue, Timing.Default);
    public static LensFlare NachoLens { get; } = Default with { Iris = new(ApertureBlades.Create(3), SignedAngle.Create(0.179f), AxisFraction.Create(0.332f), AxisFraction.MinValue, AnamorphicSqueeze.Spherical, ColorFringe.Create(-0.1f)) };
    public static LensFlare RingLens { get; } = Default with { Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.Create(0.796f), AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical, ColorFringe.Create(0.189f)) };
    public static LensFlare HexLens { get; } = Default with { Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.MinValue, AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical, ColorFringe.Create(0.052f)) };
    public static LensFlare RoundLens { get; } = Default with { Iris = new(ApertureBlades.Create(16), SignedAngle.Neutral, AxisFraction.MaxValue, AxisFraction.MinValue, AnamorphicSqueeze.Spherical, ColorFringe.Create(0.012f)) };

    public static Seq<GuideChannel> Channels(LensFlare state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LensFlare state, PassContext context) =>
        state.Source.Locate(context).Match(
            Succ: location => location.Map(source => (PixelPass)new PixelPass.Frame((frame, progress) => Try.lift(() => Fill(state, context, source.Point).Match(
                Some: fill => state.Layer.Pass(fill(source.Occluder.Match(
                    Some: occluder => occluder.Visible(source.Point, state.HotspotSize, context.Extent), None: static () => 1f))).Run(frame, progress),
                None: static () => Fin.Succ(unit))).Run())),
            Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => Fin.Fail<Unit>(error))));

    private static Option<Func<float, Action<Span<Vector4>, long, long>>> Fill(LensFlare state, PassContext context, (double X, double Y) source) {
        (FieldFrame frame, double side, Iris iris) = (Placement.Default.Frame(context.Extent), context.Extent.ShortSide, state.Iris);
        (double X, double Y) = frame.Local(source) switch { var (x, y) => (x, -y) };
        (double first, double last, uint draws) = (state.GhostFirst, state.GhostLast, CoordinateHash.Field(NoiseStream.LensFlare, state.Seed, 1u));
        (double hotspot, double edge, double burst, double density, float sharpness) =
            (state.HotspotSize, ShapeEdge.Band(state.HotspotSoftness, context.Extent), state.StarburstLength, state.StarburstDensity / double.Tau, state.StarburstSharpness);
        Func<(ERational X, ERational Y, ERational Z, ERational W), uint, NoiseSample<(ERational X, ERational Y, ERational Z, ERational W)>> basis = state.Basis.Sampler;
        (uint field, ERational shimmer) =
            (CoordinateHash.Field(NoiseStream.LensFlare, state.Seed, 0u), ERational.FromSingle(state.Shimmer).Multiply(ERational.FromSingle(state.Hold.Held(state.Timing.At(context)))));
        (Matrix3x2 turn, (double X, double Y) halo, double width, double rim) =
            (Matrix3x2.CreateRotation(state.BarAngle), (state.RingMagnification * X, state.RingMagnification * Y), state.RingWidth, ShapeEdge.Band(state.RingSoftness, context.Extent));
        RampTable ghostColors = state.GhostColors.Tabulate(context.Working);
        ((double X, double Y) Centre, float Flip, double Radius, double Ramp, double Bound, Vector3 Color)[] ghosts = [..
            from k in state.Ghosts.Enabled ? Enumerable.Range(0, state.GhostCount) : []
            let u = CoordinateHash.Uniforms(k, 0L, draws)
            let position = (k + 0.5d + (state.GhostVariation * (u.Y - 0.5d))) / state.GhostCount
            let m = double.Lerp(first, last, position)
            let shrink = 1d - (state.GhostVariation * (double)u.X)
            let radius = state.GhostSize.Pixels(context.Extent) * shrink
            let ramp = (state.GhostSoftness * radius) + 1d
            let color = ghostColors.Sample(position)
            select ((m * X, m * Y), (float)double.CopySign(1d, m), radius, ramp, (iris.Circumradius * radius) + (ramp / 2d), color.AsVector3() * (float)(color.W / (shrink * shrink)))];
        Seq<(Gated<Exposure> Gain, Ramp Colors, Func<(double X, double Y), (float Tone, Vector3 Weight)> Shape)> elements =
            [(state.Hotspot, state.HotspotColors, Disc), (state.Starburst, state.StarburstColors, Streaks), (state.Bar, state.BarColors, Anamorphic), (state.Ring, state.RingColors, Halo)];
        Func<(double X, double Y), Vector3>[] terms = [..
            from element in elements
            from gain in element.Gain.Active.ToSeq()
            let table = element.Colors.Tabulate(context.Working)
            select (Func<(double X, double Y), Vector3>)(q => element.Shape(q) switch { var (tone, weight) => gain.Scale * weight * (table.Sample(tone) switch { var color => color.AsVector3() * color.W }) }),
            .. from gain in state.Ghosts.Active.ToSeq() select (Func<(double X, double Y), Vector3>)(q => gain.Scale * Chain(q))];
        return terms.Length == 0 ? None : Some<Func<float, Action<Span<Vector4>, long, long>>>(visible => (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                (double X, double Y) q = frame.Local((column + i + 0.5d, context.Extent.Height - line - 0.5d)) switch { var (x, y) => (x, -y) };
                row[i] = new Vector4(visible * terms.Aggregate(Vector3.Zero, (light, term) => light + term(q)), 1f);
            }
        });

        (float, Vector3) Disc((double X, double Y) q) => double.Hypot(q.X - X, q.Y - Y) switch {
            var r => ((float)(hotspot == 0d ? 0d : double.Min(r / hotspot, 1d)), new Vector3(float.Min(ShapeEdge.Coverage((r - hotspot) * side, edge), (float)(2d * hotspot * side / edge)))),
        };

        (float, Vector3) Streaks((double X, double Y) q) => burst == 0d ? (0f, Vector3.Zero) : ((X: q.X - X, Y: q.Y - Y), double.Min(double.Hypot(q.X - X, q.Y - Y) / burst, 1d)) switch {
            (var d, < 1d and var u) => ((float)u, new Vector3(MathF.Pow(float.Max(0f, (2f * Noise(d)) - 1f), sharpness) * (float)((1d - u) * (1d - u)))),
            (_, var u) => ((float)u, Vector3.Zero),
        };

        float Noise((double X, double Y) d) => double.SinCos(double.Atan2(d.Y, d.X)) switch { var (sin, cos) => basis((ERational.FromDouble(density * cos), ERational.FromDouble(density * sin), shimmer, ERational.Zero), field).Value };

        (float, Vector3) Anamorphic((double X, double Y) q) => state.BarLength == ShortSideLength.Neutral ? (0f, Vector3.Zero) : (q.X - X, q.Y - Y) switch {
            var (x, y) => double.Min(double.Abs((x * turn.M11) + (y * turn.M21)) / state.BarLength, 1d) switch {
                var u => ((float)u, new Vector3((float)((1d - u) * (1d - u)) * ShapeEdge.Strip(double.Abs((x * turn.M12) + (y * turn.M22)) * side, state.BarThickness.Pixels(context.Extent), 1d))),
            },
        };

        (float, Vector3) Halo((double X, double Y) q) => (double.Hypot(q.X - halo.X, q.Y - halo.Y) - state.RingRadius) switch {
            var delta => (Easing.Saturate((float)(width == 0d ? (double.Sign(delta) + 1d) / 2d : (delta / width) + 0.5d)), new Vector3(ShapeEdge.Strip(double.Abs(delta) * side, width * side, rim))),
        };

        Vector3 Chain((double X, double Y) q) {
            Vector256<double> sum = Vector256<double>.Zero;
            foreach (((double X, double Y) centre, float flip, double radius, double ramp, double bound, Vector3 color) in ghosts) {
                (double x, double y) = ((q.X - centre.X) * side, (q.Y - centre.Y) * side);
                if (double.Hypot(x, y) > bound) continue;
                (double reach, double outline) = iris.Outline((flip * x, -flip * y));
                sum += Vector256.Create(color.X, color.Y, color.Z, 0d) * iris.Weights(reach, outline, radius, ramp);
            }
            return new((float)sum[0], (float)sum[1], (float)sum[2]);
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
    private static readonly Presentation<float> Spot = ShortSideLength.Presentation with { Soft = (0.001f, 0.25f) };
    private static readonly Presentation<float> Reach = ShortSideLength.Presentation with { Soft = (0.01f, 2f) };
    private static readonly Presentation<float> Thin = ShortSideLength.Presentation with { Soft = (0.001f, 0.25f), Scale = TrackScale.Cubic };

    public static readonly LensFlareParameter Source = new("source", new StateParameter<LensFlare>.Record<FlareSource>(Lens<LensFlare, FlareSource>.New(static s => s.Source, static v => s => s with { Source = v })));
    public static readonly LensFlareParameter Hotspot = new("hotspot", StateParameter.OptionalBounded<LensFlare, Exposure, float, InvalidToneValue>(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Hotspot, static v => s => s with { Hotspot = v }), Exposure.Presentation));
    public static readonly LensFlareParameter HotspotColors = new("hotspot-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.HotspotColors, static v => s => s with { HotspotColors = v })));
    public static readonly LensFlareParameter HotspotSize = new("hotspot-size", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.HotspotSize, static v => s => s with { HotspotSize = v }), Spot));
    public static readonly LensFlareParameter HotspotSoftness = new("hotspot-softness", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.HotspotSoftness, static v => s => s with { HotspotSoftness = v }), Thin));
    public static readonly LensFlareParameter Starburst = new("starburst", StateParameter.OptionalBounded<LensFlare, Exposure, float, InvalidToneValue>(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Starburst, static v => s => s with { Starburst = v }), Exposure.Presentation));
    public static readonly LensFlareParameter StarburstColors = new("starburst-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.StarburstColors, static v => s => s with { StarburstColors = v })));
    public static readonly LensFlareParameter StarburstLength = new("starburst-length", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.StarburstLength, static v => s => s with { StarburstLength = v }), Reach));
    public static readonly LensFlareParameter Basis = new("basis", new StateParameter<LensFlare>.Record<NoiseBasis>(Lens<LensFlare, NoiseBasis>.New(static s => s.Basis, static v => s => s with { Basis = v })));
    public static readonly LensFlareParameter StarburstDensity = new("starburst-density", StateParameter.Bounded<LensFlare, RayDensity, float, InvalidGenerator>(Lens<LensFlare, RayDensity>.New(static s => s.StarburstDensity, static v => s => s with { StarburstDensity = v }), new() { Scale = TrackScale.Cubic, Soft = (1f, 100f) }));
    public static readonly LensFlareParameter StarburstSharpness = new("starburst-sharpness", StateParameter.Bounded<LensFlare, RaySharpness, float, InvalidGenerator>(Lens<LensFlare, RaySharpness>.New(static s => s.StarburstSharpness, static v => s => s with { StarburstSharpness = v }), new() { Soft = (1f, 8f) }));
    public static readonly LensFlareParameter Shimmer = new("shimmer", StateParameter.Bounded<LensFlare, Frequency, float, InvalidGenerator>(Lens<LensFlare, Frequency>.New(static s => s.Shimmer, static v => s => s with { Shimmer = v }), Frequency.Presentation with { Soft = (0f, 10f) }));
    public static readonly LensFlareParameter Hold = new("hold", StateParameter.Bounded<LensFlare, Hold, float, InvalidGenerator>(Lens<LensFlare, Hold>.New(static s => s.Hold, static v => s => s with { Hold = v }), Generators.Hold.Presentation));
    public static readonly LensFlareParameter Bar = new("bar", StateParameter.OptionalBounded<LensFlare, Exposure, float, InvalidToneValue>(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Bar, static v => s => s with { Bar = v }), Exposure.Presentation));
    public static readonly LensFlareParameter BarColors = new("bar-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.BarColors, static v => s => s with { BarColors = v })));
    public static readonly LensFlareParameter BarLength = new("bar-length", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.BarLength, static v => s => s with { BarLength = v }), Reach));
    public static readonly LensFlareParameter BarThickness = new("bar-thickness", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.BarThickness, static v => s => s with { BarThickness = v }), Thin));
    public static readonly LensFlareParameter BarAngle = new("bar-angle", StateParameter.Bounded<LensFlare, SignedAngle, float, InvalidPixelValue>(Lens<LensFlare, SignedAngle>.New(static s => s.BarAngle, static v => s => s with { BarAngle = v }), SignedAngle.Presentation));
    public static readonly LensFlareParameter Ring = new("ring", StateParameter.OptionalBounded<LensFlare, Exposure, float, InvalidToneValue>(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Ring, static v => s => s with { Ring = v }), Exposure.Presentation));
    public static readonly LensFlareParameter RingColors = new("ring-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.RingColors, static v => s => s with { RingColors = v })));
    public static readonly LensFlareParameter RingRadius = new("ring-radius", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.RingRadius, static v => s => s with { RingRadius = v }), ShortSideLength.Presentation));
    public static readonly LensFlareParameter RingWidth = new("ring-width", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.RingWidth, static v => s => s with { RingWidth = v }), Thin));
    public static readonly LensFlareParameter RingSoftness = new("ring-softness", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.RingSoftness, static v => s => s with { RingSoftness = v }), Thin));
    public static readonly LensFlareParameter RingMagnification = new("ring-magnification", StateParameter.Bounded<LensFlare, AxisMagnification, float, InvalidGenerator>(Lens<LensFlare, AxisMagnification>.New(static s => s.RingMagnification, static v => s => s with { RingMagnification = v }), AxisMagnification.Presentation));
    public static readonly LensFlareParameter Ghosts = new("ghosts", StateParameter.OptionalBounded<LensFlare, Exposure, float, InvalidToneValue>(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Ghosts, static v => s => s with { Ghosts = v }), Exposure.Presentation));
    public static readonly LensFlareParameter GhostColors = new("ghost-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.GhostColors, static v => s => s with { GhostColors = v })));
    public static readonly LensFlareParameter GhostSize = new("ghost-size", StateParameter.Bounded<LensFlare, ShortSideLength, float, InvalidPixelValue>(Lens<LensFlare, ShortSideLength>.New(static s => s.GhostSize, static v => s => s with { GhostSize = v }), Spot));
    public static readonly LensFlareParameter GhostCount = new("ghost-count", StateParameter.Bounded<LensFlare, GhostCount, int, InvalidGenerator>(Lens<LensFlare, GhostCount>.New(static s => s.GhostCount, static v => s => s with { GhostCount = v }), new() { Step = 1, Soft = (1, 16) }));
    public static readonly LensFlareParameter GhostFirst = new("ghost-first", StateParameter.Bounded<LensFlare, AxisMagnification, float, InvalidGenerator>(Lens<LensFlare, AxisMagnification>.New(static s => s.GhostFirst, static v => s => s with { GhostFirst = v }), AxisMagnification.Presentation));
    public static readonly LensFlareParameter GhostLast = new("ghost-last", StateParameter.Bounded<LensFlare, AxisMagnification, float, InvalidGenerator>(Lens<LensFlare, AxisMagnification>.New(static s => s.GhostLast, static v => s => s with { GhostLast = v }), AxisMagnification.Presentation));
    public static readonly LensFlareParameter GhostVariation = new("ghost-variation", StateParameter.Bounded<LensFlare, GhostVariation, float, InvalidGenerator>(Lens<LensFlare, GhostVariation>.New(static s => s.GhostVariation, static v => s => s with { GhostVariation = v }), new() { Soft = (0f, 0.9f) }));
    public static readonly LensFlareParameter GhostSoftness = new("ghost-softness", StateParameter.Bounded<LensFlare, AxisFraction, float, InvalidGrade>(Lens<LensFlare, AxisFraction>.New(static s => s.GhostSoftness, static v => s => s with { GhostSoftness = v }), new()));
    public static readonly LensFlareParameter Blades = new("blades", Outline.Blades);
    public static readonly LensFlareParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly LensFlareParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly LensFlareParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly LensFlareParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly LensFlareParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));
    public static readonly LensFlareParameter Layer = new("layer", new StateParameter<LensFlare>.Record<GeneratedLayer>(Lens<LensFlare, GeneratedLayer>.New(static s => s.Layer, static v => s => s with { Layer = v })));
    public static readonly LensFlareParameter Seed = new("seed", StateParameter.Bounded<LensFlare, Seed, int, InvalidGenerator>(Lens<LensFlare, Seed>.New(static s => s.Seed, static v => s => s with { Seed = v }), Generators.Seed.Presentation));
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
        state.Source.Locate(context).Match(
            Succ: location => location.Map(source => (PixelPass)new PixelPass.Frame((frame, progress) => {
                Func<(double X, double Y), bool> hidden = source.Occluder.Match(Some: static occluder => (Func<(double X, double Y), bool>)occluder.Hides, None: static () => static _ => false);
                PixelFrame key = new(frame.Origin, frame.Size, frame.Extent, block => {
                    Span2D<Vector4> keys = block.AsSpan().Cast<float, Vector4>().AsSpan2D(frame.Size.Height, frame.Size.Width);
                    ReadOnlySpan2D<Vector4> light = frame.View.Span;
                    for (int y = 0; y < keys.Height; y++) {
                        for (int x = 0; x < keys.Width; x++)
                            keys[y, x] = hidden(((long)frame.Origin.X + x + 0.5d, (long)frame.Origin.Y + y + 0.5d)) ? Vector4.Zero : state.Key.Extract(light[y, x]);
                    }
                });
                return state.Layer.Pass(Fill(state, context, source.Point, key)).Run(frame, progress);
            })),
            Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => Fin.Fail<Unit>(error))));

    private static Action<Span<Vector4>, long, long> Fill(LightRays state, PassContext context, (double X, double Y) source, PixelFrame key) {
        (double reach, float jitter) = (double.Truncate((double)(float)state.Size * context.Extent.ShortSide), state.Jitter);
        (uint field, Vector3 tint) = (CoordinateHash.Field(NoiseStream.LightRays, state.Seed, state.Timing.At(context).Term), state.Tint.SceneLight(context.Working));
        (WrapMode Across, WrapMode Down) wrap = (WrapMode.Black, WrapMode.Black);
        Func<long, long, float> shift = state.Jitter == RayJitter.Neutral ? static (_, _) => 0f : (column, line) => CoordinateHash.Uniforms(column, line, field).X;
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                (double X, double Y) p = (column + i + 0.5d, context.Extent.Height - line - 0.5d);
                row[i] = new Vector4(tint * (reach == 0d ? Sample(p) : Rays(p, shift(column + i, line))).AsVector3(), 1f);
            }
        };

        Vector4 Sample((double X, double Y) p) => key.Sample(p, wrap, Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight));

        Vector4 Rays((double X, double Y) p, float offset) {
            double distance = double.Max(1d, double.Hypot(source.X - p.X, source.Y - p.Y));
            (double steps, (double X, double Y) step) = (double.Floor(double.Min(reach, distance)), ((source.X - p.X) / distance, (source.Y - p.Y) / distance));
            (Vector4 sum, float total) = (Vector4.Zero, 0f);
            for (long k = 0; k <= (1d - jitter) * steps; k++) {
                double at = jitter == 1f ? 0d : (k + (double)offset) / (1d - jitter);
                (double X, double Y) t = (p.X + (at * step.X), p.Y + (at * step.Y));
                if (t.X < 0d || t.X > context.Extent.Width || t.Y < 0d || t.Y > context.Extent.Height)
                    break;
                float fall = (float)(1d - (at / steps));
                (sum, total) = (sum + (fall * fall * Sample(t)), total + (fall * fall));
            }
            return total != 0f ? sum / total : Sample(p);
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
    public static readonly LightRaysParameter Size = new("size", StateParameter.Bounded<LightRays, ShortSideLength, float, InvalidPixelValue>(Lens<LightRays, ShortSideLength>.New(static s => s.Size, static v => s => s with { Size = v }), ShortSideLength.Presentation with { Soft = (0f, LightRays.Diagonal) }));
    public static readonly LightRaysParameter Jitter = new("jitter", StateParameter.Bounded<LightRays, RayJitter, float, InvalidGenerator>(Lens<LightRays, RayJitter>.New(static s => s.Jitter, static v => s => s with { Jitter = v }), new()));
    public static readonly LightRaysParameter Tint = new("tint", new StateParameter<LightRays>.Color(Lens<LightRays, Swatch>.New(static s => s.Tint, static v => s => s with { Tint = v })));
    public static readonly LightRaysParameter Layer = new("layer", new StateParameter<LightRays>.Record<GeneratedLayer>(Lens<LightRays, GeneratedLayer>.New(static s => s.Layer, static v => s => s with { Layer = v })));
    public static readonly LightRaysParameter Seed = new("seed", StateParameter.Bounded<LightRays, Seed, int, InvalidGenerator>(Lens<LightRays, Seed>.New(static s => s.Seed, static v => s => s with { Seed = v }), Generators.Seed.Presentation));
    public static readonly LightRaysParameter Timing = new("timing", new StateParameter<LightRays>.Record<Timing>(Lens<LightRays, Timing>.New(static s => s.Timing, static v => s => s with { Timing = v })));

    public StateParameter<LightRays> Kind { get; }
}
