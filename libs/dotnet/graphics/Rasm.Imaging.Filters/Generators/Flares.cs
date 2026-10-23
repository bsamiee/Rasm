using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using PeterO.Numbers;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Filters.Optics;

namespace Rasm.Imaging.Filters.Generators;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<Guid>(SkipIParsable = true)]
public readonly partial struct LightTarget {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError("Light target identifier is empty") : null;
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
            let center = (X: x + 0.5d, Y: y + 0.5d)
            where double.Hypot(center.X - source.X, center.Y - source.Y) <= reach
            select center);
        return disc.IsEmpty ? 1f : disc.Filter(point => !occluder.Hides(point)).Count / (float)disc.Count;

        IEnumerable<int> Window(double at, int size) =>
            (int)double.Clamp(double.Floor(at - reach), 0d, size) switch { var low => Enumerable.Range(low, (int)double.Clamp(double.Ceiling(at + reach), 0d, size) - low) };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareAnchor : ICaptioned {
    public static readonly FlareAnchor Frame = new("frame", "Frame position", [], static (position, _, context) =>
        Some((position.Point(context.Extent), Option<float>.None)));
    public static readonly FlareAnchor Sun = new("sun", "Sun", [GuideChannel.Depth], static (_, _, context) =>
        context.Lights.ToFin(Error.New("Sun flare anchor needs scene lights")).Map(lights => lights.Sun.Bind(direction => Distant(direction, context))));
    public static readonly FlareAnchor Light = new("light", "Light", [GuideChannel.Depth], static (_, light, context) =>
        light.Traverse(target => context.Lights.ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"Light flare anchor {target} needs scene lights"))).Map(lights =>
            (from point in lights.Points.Find(target) from camera in context.Camera from located in camera.Pixel(point) select (located.Point, Depth: Some(located.Depth)))
            | lights.Directions.Find(target).Bind(direction => Distant(direction, context)))).As().Map(static located => located.Flatten()));

    public string Caption { get; }
    public Seq<GuideChannel> Occluders { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Option<((double X, double Y) Point, Option<float> Depth)>> Locate(FramePosition position, Option<LightTarget> light, PassContext context);

    private static Option<((double X, double Y) Point, Option<float> Depth)> Distant(Vector3 direction, PassContext context) =>
        from camera in context.Camera
        from located in camera.Frustum.Map(perspective: camera.Pixel(camera.Location + direction), parallel: Option<((double X, double Y) Point, float Depth)>.None)
        select (located.Point, Some(float.PositiveInfinity));
}

public sealed record FlareSource(FlareAnchor Anchor, FramePosition Position, Option<LightTarget> Light, bool Occlusion) : IStateRecord<FlareSource, FlareSourceParameter> {
    public static FlareSource Default { get; } = new(FlareAnchor.Frame, FramePosition.Center, None, Occlusion: true);

    internal Seq<GuideChannel> Channels => Occlusion ? Anchor.Occluders : [];

    internal Option<PixelPass> Pass(PassContext context, Func<(double X, double Y), Option<Occluder>, Func<PixelFrame, IProgress<int>, Fin<Unit>>> kernel) =>
        Anchor.Locate(Position, Light, context).Match(
            Succ: location =>
                from source in location
                from occluder in Occlusion ? source.Depth.Traverse(depth => context.Guides.Find(GuideChannel.Depth).Map(guide => new Occluder(guide, depth))).As() : Some(Option<Occluder>.None)
                select (PixelPass)new PixelPass.Frame(kernel(source.Point, occluder)),
            Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlareSourceParameter : IStateParameter<FlareSource> {
    public static readonly FlareSourceParameter Anchor = new("anchor", new StateParameter<FlareSource>.Choice<FlareAnchor>(Lens<FlareSource, FlareAnchor>.New(static s => s.Anchor, static v => s => s with { Anchor = v })),
        new("Source", "Point generated light converges on"));
    public static readonly FlareSourceParameter Position = new("position", new StateParameter<FlareSource>.Record<FramePosition>(Lens<FlareSource, FramePosition>.New(static s => s.Position, static v => s => s with { Position = v })),
        new("Position", "Frame point to converge on when Source is Frame position"));
    public static readonly FlareSourceParameter Light = new("light", new StateParameter<FlareSource>.OptionalKeyed<LightTarget, Guid>(Lens<FlareSource, Option<LightTarget>>.New(static s => s.Light, static v => s => s with { Light = v })),
        new("Light", "Document light to converge on when Source is Light"));
    public static readonly FlareSourceParameter Occlusion = new("occlusion", new StateParameter<FlareSource>.Toggle(Lens<FlareSource, bool>.New(static s => s.Occlusion, static v => s => s with { Occlusion = v })),
        new("Occlusion", "Surfaces nearer camera than source hide its light"));

    public StateParameter<FlareSource> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LensFlare(
    FlareSource Source, Gated<Exposure> Hotspot, Ramp HotspotColors, ShortSideLength HotspotSize, ShortSideLength HotspotSoftness,
    Gated<Exposure> Starburst, Ramp StarburstColors, ShortSideLength StarburstLength, NoiseBasis Basis, Magnitude StarburstDensity, Magnitude StarburstSharpness, Frequency Shimmer, Hold Hold,
    Gated<Exposure> Bar, Ramp BarColors, ShortSideLength BarLength, ShortSideLength BarThickness, SignedAngle BarAngle,
    Gated<Exposure> Ring, Ramp RingColors, ShortSideLength RingRadius, ShortSideLength RingWidth, ShortSideLength RingSoftness, SignedMagnitude RingMagnification,
    Gated<Exposure> Ghosts, Ramp GhostColors, ShortSideLength GhostSize, PositiveCount GhostCount, SignedMagnitude GhostFirst, SignedMagnitude GhostLast, ProperFraction GhostVariation,
    AxisFraction GhostSoftness, Iris Iris, SignedFraction Fringe, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LensFlare, LensFlareParameter>, IPixelStage<LensFlare> {
    private static readonly ShortSideLength SourceRadius = ShortSideLength.Create(0.2f * 0.3f / 2f * ReferenceFrame.GreaterSide);

    public static LensFlare Default { get; } = new(
        FlareSource.Default with { Position = new(FrameAxis.Create(0.3f), FrameAxis.Create(1f - 0.6f)) },
        new(Enabled: true, Exposure.Neutral), Ramp.White, SourceRadius, ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide),
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideLength.Create(0.5f), NoiseBasis.Fbm.Default with { Octaves = Octaves.Default with { Roughness = AxisFraction.Create(0.563f) } },
        Magnitude.Create(30f), Magnitude.Create(3f), Frequency.Neutral, Hold.MinValue,
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideLength.Create(0.972f / 2f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), SignedAngle.Neutral,
        new(Enabled: true, Exposure.Neutral), Ramp.White, ShortSideLength.Create(1f / 2.1f), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide), ShortSideLength.Create(0.02f * ReferenceFrame.GreaterSide),
        SignedMagnitude.Zero, new(Enabled: true, Exposure.Neutral), Ramp.White, SourceRadius, PositiveCount.Create(5), SignedMagnitude.Create(0.6f), SignedMagnitude.Create(-1f), ProperFraction.Create(0.5f),
        AxisFraction.Create(0.3f), Iris.Default, SignedFraction.Neutral, GeneratedLayer.Added, Seed.MinValue, Timing.Default);
    public static LensFlare NachoLens { get; } = Default with { Iris = new(ApertureBlades.Create(3), SignedAngle.Create(0.179f), AxisFraction.Create(0.332f), AxisFraction.MinValue, AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(-0.1f) };
    public static LensFlare RingLens { get; } = Default with { Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.Create(0.796f), AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(0.189f) };
    public static LensFlare HexLens { get; } = Default with { Iris = new(ApertureBlades.Six, SignedAngle.Neutral, AxisFraction.MinValue, AxisFraction.Create(0.902f), AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(0.052f) };
    public static LensFlare RoundLens { get; } = Default with { Iris = new(ApertureBlades.Create(16), SignedAngle.Neutral, AxisFraction.MaxValue, AxisFraction.MinValue, AnamorphicSqueeze.Spherical), Fringe = SignedFraction.Create(0.012f) };

    public static Seq<GuideChannel> Channels(LensFlare state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LensFlare state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) => Try.lift(() => Fill(state, context, point).Match(
            Some: fill => state.Layer.Pass(fill(occlusion.Match(
                Some: occluder => occluder.Visible(point, state.HotspotSize, context.Extent), None: 1f))).Run(frame, progress),
            None: static () => unit)).Run());

    private static Option<Func<float, Action<Span<Vector4>, long, long>>> Fill(LensFlare state, PassContext context, (double X, double Y) source) {
        (FieldFrame frame, double side, Iris iris) = (Placement.Default.Frame(context.Extent), context.Extent.ShortSide, state.Iris);
        (double X, double Y) = frame.Local(source) switch { var (x, y) => (x, -y) };
        (double first, double last, uint draws) = (state.GhostFirst, state.GhostLast, CoordinateHash.Field(NoiseStream.LensFlare, state.Seed, 1u));
        (double hotspot, double edge, double burst, double density, float sharpness) =
            (state.HotspotSize, ShapeEdge.Band(state.HotspotSoftness, context.Extent), state.StarburstLength, state.StarburstDensity / double.Tau, state.StarburstSharpness);
        Func<LatticePoint, uint, NoiseSample<LatticePoint>> basis = state.Basis.Sampler;
        (uint field, ERational shimmer) =
            (CoordinateHash.Field(NoiseStream.LensFlare, state.Seed, 0u), ERational.FromSingle(state.Shimmer).Multiply(ERational.FromSingle(state.Hold.Held(state.Timing.At(context)))));
        (Matrix3x2 turn, (double X, double Y) halo, double width, double rim) =
            (Matrix3x2.CreateRotation(state.BarAngle), (state.RingMagnification * X, state.RingMagnification * Y), state.RingWidth, ShapeEdge.Band(state.RingSoftness, context.Extent));
        RampTable ghostColors = state.GhostColors.Tabulate(context.Working);
        ((double X, double Y) Center, float Flip, double Radius, double Ramp, double Bound, Vector3 Color)[] ghosts = [..
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
        return terms.Length is 0 ? None : Some<Func<float, Action<Span<Vector4>, long, long>>>(visible => frame.Fill(q => new Vector4(visible * terms.Aggregate(Vector3.Zero, (light, term) => light + term(q)), 1f)));

        (float, Vector3) Disc((double X, double Y) q) => double.Hypot(q.X - X, q.Y - Y) switch {
            var r => ((float)(hotspot is 0d ? 0d : double.Min(r / hotspot, 1d)), new Vector3(float.Min(ShapeEdge.Coverage((r - hotspot) * side, edge), (float)(2d * hotspot * side / edge)))),
        };

        (float, Vector3) Streaks((double X, double Y) q) => burst is 0d ? (0f, Vector3.Zero) : ((X: q.X - X, Y: q.Y - Y), double.Min(double.Hypot(q.X - X, q.Y - Y) / burst, 1d)) switch {
            (var d, < 1d and var u) => ((float)u, new Vector3(MathF.Pow(float.Max(0f, (2f * Noise(d)) - 1f), sharpness) * (float)((1d - u) * (1d - u)))),
            (_, var u) => ((float)u, Vector3.Zero),
        };

        float Noise((double X, double Y) d) => double.SinCos(double.Atan2(d.Y, d.X)) switch { var (sin, cos) => basis(new(ERational.FromDouble(density * cos), ERational.FromDouble(density * sin), shimmer, ERational.Zero), field).Value };

        (float, Vector3) Anamorphic((double X, double Y) q) => state.BarLength == ShortSideLength.Neutral ? (0f, Vector3.Zero) : (q.X - X, q.Y - Y) switch {
            var (x, y) => double.Min(double.Abs((x * turn.M11) + (y * turn.M21)) / state.BarLength, 1d) switch {
                var u => ((float)u, new Vector3((float)((1d - u) * (1d - u)) * ShapeEdge.Strip(double.Abs((x * turn.M12) + (y * turn.M22)) * side, state.BarThickness.Pixels(context.Extent), 1d))),
            },
        };

        (float, Vector3) Halo((double X, double Y) q) => (double.Hypot(q.X - halo.X, q.Y - halo.Y) - state.RingRadius) switch {
            var delta => (Easing.Saturate((float)(width is 0d ? (double.Sign(delta) + 1d) / 2d : (delta / width) + 0.5d)), new Vector3(ShapeEdge.Strip(double.Abs(delta) * side, width * side, rim))),
        };

        Vector3 Chain((double X, double Y) q) {
            Vector256<double> sum = Vector256<double>.Zero;
            foreach (((double X, double Y) center, float flip, double radius, double ramp, double bound, Vector3 color) in ghosts) {
                (double x, double y) = ((q.X - center.X) * side, (q.Y - center.Y) * side);
                if (double.Hypot(x, y) > bound) continue;
                (double reach, double outline) = iris.Outline((flip * x, -flip * y));
                sum += Vector256.Create(color.X, color.Y, color.Z, 0d) * iris.Weights(reach, outline, radius, ramp, state.Fringe);
            }
            return new((float)sum[0], (float)sum[1], (float)sum[2]);
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFlareParameter : IStateParameter<LensFlare> {
    private static readonly Presentation<float> Spot = ShortSideLength.Presentation with { Soft = (0.001f, 0.25f) };
    private static readonly Presentation<float> Reach = ShortSideLength.Presentation with { Soft = (0.01f, 2f) };
    private static readonly Presentation<float> Thin = ShortSideLength.Presentation with { Soft = (0.001f, 0.25f), Scale = TrackScale.Cubic };
    private static readonly Presentation<float> Magnification = new() { Soft = (-2f, 2f), Origin = (float)SignedMagnitude.Zero };

    public static readonly LensFlareParameter Source = new("source", new StateParameter<LensFlare>.Record<FlareSource>(Lens<LensFlare, FlareSource>.New(static s => s.Source, static v => s => s with { Source = v })),
        new("Source", "Frame position or scene light generated light follows"));
    public static readonly LensFlareParameter Hotspot = new("hotspot", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Hotspot, static v => s => s with { Hotspot = v })),
        new("Hotspot", "Glow brightness at source in stops"));
    public static readonly LensFlareParameter HotspotColors = new("hotspot-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.HotspotColors, static v => s => s with { HotspotColors = v })),
        new("Hotspot colors", "Color and coverage from center to hotspot edge"));
    public static readonly LensFlareParameter HotspotSize = new("hotspot-size", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.HotspotSize, static v => s => s with { HotspotSize = v }), Spot),
        new("Hotspot size", "Hotspot radius in frame short sides"));
    public static readonly LensFlareParameter HotspotSoftness = new("hotspot-softness", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.HotspotSoftness, static v => s => s with { HotspotSoftness = v }), Thin),
        new("Hotspot softness", "Hotspot edge softness in frame short sides"));
    public static readonly LensFlareParameter Starburst = new("starburst", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Starburst, static v => s => s with { Starburst = v })),
        new("Starburst", "Ray brightness around source in stops"));
    public static readonly LensFlareParameter StarburstColors = new("starburst-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.StarburstColors, static v => s => s with { StarburstColors = v })),
        new("Starburst colors", "Color and coverage from source along each ray"));
    public static readonly LensFlareParameter StarburstLength = new("starburst-length", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.StarburstLength, static v => s => s with { StarburstLength = v }), Reach),
        new("Starburst length", "Ray reach in frame short sides"));
    public static readonly LensFlareParameter Basis = new("basis", new StateParameter<LensFlare>.Variant<NoiseBasis>(Lens<LensFlare, NoiseBasis>.New(static s => s.Basis, static v => s => s with { Basis = v })),
        new("Starburst noise", "Ray pattern around source"));
    public static readonly LensFlareParameter StarburstDensity = new("starburst-density", StateParameter.Bounded(Lens<LensFlare, Magnitude>.New(static s => s.StarburstDensity, static v => s => s with { StarburstDensity = v }), new Presentation<float> { Scale = TrackScale.Cubic, Soft = (1f, 100f) }),
        new("Starburst density", "Noise periods per turn around source"));
    public static readonly LensFlareParameter StarburstSharpness = new("starburst-sharpness", StateParameter.Bounded(Lens<LensFlare, Magnitude>.New(static s => s.StarburstSharpness, static v => s => s with { StarburstSharpness = v }), new Presentation<float> { Soft = (1f, 8f) }),
        new("Starburst sharpness", "Power narrowing each ray around its brightest axis"));
    public static readonly LensFlareParameter Shimmer = new("shimmer", StateParameter.Bounded(Lens<LensFlare, Frequency>.New(static s => s.Shimmer, static v => s => s with { Shimmer = v }), Frequency.Presentation with { Soft = (0f, 10f) }),
        new("Shimmer", "Noise units per second the rays change through"));
    public static readonly LensFlareParameter Hold = new("hold", StateParameter.Bounded(Lens<LensFlare, Hold>.New(static s => s.Hold, static v => s => s with { Hold = v })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly LensFlareParameter Bar = new("bar", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Bar, static v => s => s with { Bar = v })),
        new("Anamorphic bar", "Bar brightness through source in stops"));
    public static readonly LensFlareParameter BarColors = new("bar-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.BarColors, static v => s => s with { BarColors = v })),
        new("Bar colors", "Color and coverage from source along bar"));
    public static readonly LensFlareParameter BarLength = new("bar-length", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.BarLength, static v => s => s with { BarLength = v }), Reach),
        new("Bar length", "Bar half length in frame short sides"));
    public static readonly LensFlareParameter BarThickness = new("bar-thickness", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.BarThickness, static v => s => s with { BarThickness = v }), Thin),
        new("Bar thickness", "Bar width in frame short sides"));
    public static readonly LensFlareParameter BarAngle = new("bar-angle", StateParameter.Bounded(Lens<LensFlare, SignedAngle>.New(static s => s.BarAngle, static v => s => s with { BarAngle = v })),
        new("Bar angle", "Counterclockwise turn from horizontal"));
    public static readonly LensFlareParameter Ring = new("ring", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Ring, static v => s => s with { Ring = v })),
        new("Halo", "Halo brightness in stops"));
    public static readonly LensFlareParameter RingColors = new("ring-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.RingColors, static v => s => s with { RingColors = v })),
        new("Halo colors", "Color and coverage across halo band"));
    public static readonly LensFlareParameter RingRadius = new("ring-radius", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.RingRadius, static v => s => s with { RingRadius = v })),
        new("Halo radius", "Halo radius in frame short sides"));
    public static readonly LensFlareParameter RingWidth = new("ring-width", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.RingWidth, static v => s => s with { RingWidth = v }), Thin),
        new("Halo width", "Halo band width in frame short sides"));
    public static readonly LensFlareParameter RingSoftness = new("ring-softness", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.RingSoftness, static v => s => s with { RingSoftness = v }), Thin),
        new("Halo softness", "Halo edge softness in frame short sides"));
    public static readonly LensFlareParameter RingMagnification = new("ring-magnification", StateParameter.Bounded(Lens<LensFlare, SignedMagnitude>.New(static s => s.RingMagnification, static v => s => s with { RingMagnification = v }), Magnification),
        new("Halo position", "Halo center along source axis, 0 at frame center, 1 at source"));
    public static readonly LensFlareParameter Ghosts = new("ghosts", StateParameter.OptionalBounded(Lens<LensFlare, Gated<Exposure>>.New(static s => s.Ghosts, static v => s => s with { Ghosts = v })),
        new("Ghosts", "Iris image brightness along source axis in stops"));
    public static readonly LensFlareParameter GhostColors = new("ghost-colors", new StateParameter<LensFlare>.Gradient(Lens<LensFlare, Ramp>.New(static s => s.GhostColors, static v => s => s with { GhostColors = v })),
        new("Ghost colors", "Color and coverage along ghost chain"));
    public static readonly LensFlareParameter GhostSize = new("ghost-size", StateParameter.Bounded(Lens<LensFlare, ShortSideLength>.New(static s => s.GhostSize, static v => s => s with { GhostSize = v }), Spot),
        new("Ghost size", "Largest ghost radius in frame short sides"));
    public static readonly LensFlareParameter GhostCount = new("ghost-count", StateParameter.Bounded(Lens<LensFlare, PositiveCount>.New(static s => s.GhostCount, static v => s => s with { GhostCount = v }), new Presentation<int> { Step = 1, Soft = (1, 16) }),
        new("Ghost count", "Iris images along source axis"));
    public static readonly LensFlareParameter GhostFirst = new("ghost-first", StateParameter.Bounded(Lens<LensFlare, SignedMagnitude>.New(static s => s.GhostFirst, static v => s => s with { GhostFirst = v }), Magnification),
        new("First ghost", "Ghost chain start, 0 at frame center, 1 at source"));
    public static readonly LensFlareParameter GhostLast = new("ghost-last", StateParameter.Bounded(Lens<LensFlare, SignedMagnitude>.New(static s => s.GhostLast, static v => s => s with { GhostLast = v }), Magnification),
        new("Last ghost", "Ghost chain end, 0 at frame center, 1 at source"));
    public static readonly LensFlareParameter GhostVariation = new("ghost-variation", StateParameter.Bounded(Lens<LensFlare, ProperFraction>.New(static s => s.GhostVariation, static v => s => s with { GhostVariation = v }), ProperFraction.Presentation with { Soft = (0f, 0.9f) }),
        new("Ghost variation", "Variation in each ghost's position and radius"));
    public static readonly LensFlareParameter GhostSoftness = new("ghost-softness", StateParameter.Bounded(Lens<LensFlare, AxisFraction>.New(static s => s.GhostSoftness, static v => s => s with { GhostSoftness = v })),
        new("Ghost softness", "Share of each ghost's radius feathered at its edge"));
    public static readonly LensFlareParameter Iris = new("iris", new StateParameter<LensFlare>.Record<Iris>(Lens<LensFlare, Iris>.New(static s => s.Iris, static v => s => s with { Iris = v })),
        new("Aperture", "Lens opening shaping bokeh and diffraction spikes"));
    public static readonly LensFlareParameter Fringe = new("fringe", StateParameter.Bounded(Lens<LensFlare, SignedFraction>.New(static s => s.Fringe, static v => s => s with { Fringe = v })),
        new("Color shift", "Red and blue bokeh sizes apart from green"));
    public static readonly LensFlareParameter Layer = new("layer", new StateParameter<LensFlare>.Record<GeneratedLayer>(Lens<LensFlare, GeneratedLayer>.New(static s => s.Layer, static v => s => s with { Layer = v })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly LensFlareParameter Seed = new("seed", StateParameter.Bounded(Lens<LensFlare, Seed>.New(static s => s.Seed, static v => s => s with { Seed = v })),
        new("Seed", "Number selecting pattern"));
    public static readonly LensFlareParameter Timing = new("timing", new StateParameter<LensFlare>.Record<Timing>(Lens<LensFlare, Timing>.New(static s => s.Timing, static v => s => s with { Timing = v })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<LensFlare> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LightRays(HighlightKey Highlight, FlareSource Source, ShortSideLength Size, AxisFraction Jitter, Swatch Tint, GeneratedLayer Layer, Seed Seed, Timing Timing)
    : IStateRecord<LightRays, LightRaysParameter>, IPixelStage<LightRays> {
    internal static float Diagonal => float.Hypot(ReferenceFrame.GreaterSide, 1f);

    public static LightRays Default { get; } =
        new(HighlightKey.Default, FlareSource.Default, ShortSideLength.Create(0.5f * Diagonal), AxisFraction.MinValue, Swatch.White, GeneratedLayer.Added, Seed.MinValue, Timing.Default);

    public static Seq<GuideChannel> Channels(LightRays state) => state.Source.Channels;

    public static Option<PixelPass> Pass(LightRays state, PassContext context) =>
        state.Source.Pass(context, (point, occlusion) => (frame, progress) => {
            Func<(double X, double Y), bool> hidden = occlusion.Match(Some: static occluder => (Func<(double X, double Y), bool>)occluder.Hides, None: static () => static _ => false);
            PixelFrame key = new(frame.Origin, frame.Size, frame.Extent, block => {
                Span2D<Vector4> keys = block.AsSpan().Cast<float, Vector4>().AsSpan2D(frame.Size.Height, frame.Size.Width);
                ReadOnlySpan2D<Vector4> light = frame.View.Span;
                for (int y = 0; y < keys.Height; y++) {
                    for (int x = 0; x < keys.Width; x++)
                        keys[y, x] = hidden(((long)frame.Origin.X + x + 0.5d, (long)frame.Origin.Y + y + 0.5d)) ? Vector4.Zero : state.Highlight.Extract(light[y, x]);
                }
            });
            return state.Layer.Pass(Fill(state, context, point, key)).Run(frame, progress);
        });

    private static Action<Span<Vector4>, long, long> Fill(LightRays state, PassContext context, (double X, double Y) source, PixelFrame key) {
        (double reach, float jitter) = (double.Truncate((double)(float)state.Size * context.Extent.ShortSide), state.Jitter);
        (uint field, Vector3 tint) = (CoordinateHash.Field(NoiseStream.LightRays, state.Seed, state.Timing.At(context).Term), state.Tint.SceneLight(context.Working));
        (WrapMode Across, WrapMode Down) wrap = (WrapMode.Black, WrapMode.Black);
        Func<long, long, float> shift = state.Jitter == AxisFraction.MinValue ? static (_, _) => 0f : (column, line) => CoordinateHash.Uniforms(column, line, field).X;
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++) {
                (double X, double Y) p = (column + i + 0.5d, context.Extent.Height - line - 0.5d);
                row[i] = new Vector4(tint * (reach is 0d ? Sample(p) : Rays(p, shift(column + i, line))).AsVector3(), 1f);
            }
        };

        Vector4 Sample((double X, double Y) p) => key.Sample(p, wrap, Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight));

        Vector4 Rays((double X, double Y) p, float offset) {
            double distance = double.Max(1d, double.Hypot(source.X - p.X, source.Y - p.Y));
            (double steps, (double X, double Y) step) = (double.Floor(double.Min(reach, distance)), ((source.X - p.X) / distance, (source.Y - p.Y) / distance));
            (Vector4 sum, float total) = (Vector4.Zero, 0f);
            for (long k = 0; k <= (1d - jitter) * steps; k++) {
                double at = jitter is 1f ? 0d : (k + (double)offset) / (1d - jitter);
                (double X, double Y) t = (p.X + (at * step.X), p.Y + (at * step.Y));
                if (t.X < 0d || t.X > context.Extent.Width || t.Y < 0d || t.Y > context.Extent.Height)
                    break;
                float fall = (float)(1d - (at / steps));
                (sum, total) = (sum + (fall * fall * Sample(t)), total + (fall * fall));
            }
            return total is not 0f ? sum / total : Sample(p);
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightRaysParameter : IStateParameter<LightRays> {
    public static readonly LightRaysParameter Highlight = new("highlight", new StateParameter<LightRays>.Record<HighlightKey>(Lens<LightRays, HighlightKey>.New(static s => s.Highlight, static v => s => s with { Highlight = v })),
        new("Highlights", "Highlights that cast rays"));
    public static readonly LightRaysParameter Source = new("source", new StateParameter<LightRays>.Record<FlareSource>(Lens<LightRays, FlareSource>.New(static s => s.Source, static v => s => s with { Source = v })),
        new("Source", "Frame position or scene light generated light follows"));
    public static readonly LightRaysParameter Size = new("size", StateParameter.Bounded(Lens<LightRays, ShortSideLength>.New(static s => s.Size, static v => s => s with { Size = v }), ShortSideLength.Presentation with { Soft = (0f, LightRays.Diagonal) }),
        new("Size", "Longest ray march in frame short sides"));
    public static readonly LightRaysParameter Jitter = new("jitter", StateParameter.Bounded(Lens<LightRays, AxisFraction>.New(static s => s.Jitter, static v => s => s with { Jitter = v })),
        new("Jitter", "Share of march steps traded for random sample shifts"));
    public static readonly LightRaysParameter Tint = new("tint", new StateParameter<LightRays>.Color(Lens<LightRays, Swatch>.New(static s => s.Tint, static v => s => s with { Tint = v })),
        new("Tint", "Scene-linear ray color"));
    public static readonly LightRaysParameter Layer = new("layer", new StateParameter<LightRays>.Record<GeneratedLayer>(Lens<LightRays, GeneratedLayer>.New(static s => s.Layer, static v => s => s with { Layer = v })),
        new("Layer", "Blend and brightness of generated layer"));
    public static readonly LightRaysParameter Seed = new("seed", StateParameter.Bounded(Lens<LightRays, Seed>.New(static s => s.Seed, static v => s => s with { Seed = v })),
        new("Seed", "Number selecting pattern"));
    public static readonly LightRaysParameter Timing = new("timing", new StateParameter<LightRays>.Record<Timing>(Lens<LightRays, Timing>.New(static s => s.Timing, static v => s => s with { Timing = v })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<LightRays> Kind { get; }
    public ParameterText Text { get; }
}
