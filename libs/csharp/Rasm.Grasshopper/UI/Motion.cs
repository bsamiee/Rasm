using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
using Foundation;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Sparkles;
using ObjCRuntime;
using GhDuration = Grasshopper2.UI.Animation.Duration;
using GhMotion = Grasshopper2.UI.Animation.Motion;
using GhState = Grasshopper2.UI.Animation.State;
using GhTextAnchor = Grasshopper2.Extensions.TextAnchor;
using Op = Rasm.Domain.Op;
using ZoomThreshold = Grasshopper2.UI.ZoomThreshold;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
internal static class MotionAccessibility {
    internal static bool ShouldReduceMotion =>
        NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceMotion;

    internal static bool ShouldDifferentiateWithoutColor =>
        NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldDifferentiateWithoutColor;

    internal static bool ShouldReduceTransparency =>
        NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceTransparency;

    internal static bool ShouldSkipDecorativeMotion =>
        ShouldReduceMotion || ShouldDifferentiateWithoutColor || ShouldReduceTransparency;
}

// MessageLoop = Eto coalesced ~60Hz (default). DisplayLink = CADisplayLink vsync (macOS 14+).
[SkipUnionOps]
[Union]
public partial record MotionClock {
    private MotionClock() { }
    public sealed record MessageLoopCase : MotionClock;
    public sealed record DisplayLinkCase(Option<CAFrameRateRange> Rate) : MotionClock;

    public static readonly MotionClock MessageLoop = new MessageLoopCase();
    public static MotionClock DisplayLink(Option<CAFrameRateRange> rate = default) => new DisplayLinkCase(Rate: rate);
}

[SmartEnum<int>]
public sealed partial class GradientKind {
    public static readonly GradientKind Axial = new(key: 0);
    public static readonly GradientKind Radial = new(key: 1);
    public static readonly GradientKind Conic = new(key: 2);

    internal int ProfileIndex => Key;
}

// CAEmitterLayer geometry. Each case lazily yields the kCAEmitterLayer* NSString so type-load never
// touches CoreAnimation off the GPU path (Resolve fires only inside the macOS14-gated layer builder).
[SmartEnum<int>]
public sealed partial class EmitterShape {
    private delegate NSString NameSource();

    public static readonly EmitterShape Point = new(key: 0, resolve: static () => CAEmitterLayer.ShapePoint);
    public static readonly EmitterShape Line = new(key: 1, resolve: static () => CAEmitterLayer.ShapeLine);
    public static readonly EmitterShape Rectangle = new(key: 2, resolve: static () => CAEmitterLayer.ShapeRectangle);
    public static readonly EmitterShape Cuboid = new(key: 3, resolve: static () => CAEmitterLayer.ShapeCuboid);
    public static readonly EmitterShape Circle = new(key: 4, resolve: static () => CAEmitterLayer.ShapeCircle);
    public static readonly EmitterShape Sphere = new(key: 5, resolve: static () => CAEmitterLayer.ShapeSphere);

    [UseDelegateFromConstructor]
    internal partial NSString Resolve();
}

// CALayer gradient points are normalized to the layer frame (0,0)–(1,1). Omit Start/End to use the profile
// row for Kind. Locations (when present) must match the colour count and ascend through [0,1].
[StructLayout(LayoutKind.Auto)]
public readonly record struct CosmeticGradientPoints(Option<PointF> Start = default, Option<PointF> End = default, Option<ReadOnlyMemory<float>> Locations = default) {
    public static CosmeticGradientPoints ConicSweep(PointF end) => new(End: Some(end));
    public static CosmeticGradientPoints Conic(PointF center, PointF sweep) => new(Start: Some(center), End: Some(sweep));
    public static CosmeticGradientPoints Stops(ReadOnlyMemory<float> locations) => new(Locations: Some(locations));
}

// Downstream-tunable snap-guide presentation. Mirrors SnappingGecko's per-mode axes (colour / thickness /
// line-pattern); the label font + magnitude format are our Figma/Sketch-style superset (SnappingGecko draws
// no labels). Empty Dashes renders a SOLID line (GH2's own native snap feedback is solid); the dashed factory
// is the default — our deliberate additive distinction from the native solid drag-feedback, so the two never
// read as the same guide when both are visible.
public readonly record struct SnapGuideStyle(
    Color Tint,
    float Thickness = 1f,
    float LabelFontSize = 11f,
    string MagnitudeFormat = "0.#",
    GhDuration Duration = GhDuration.Normal,
    GhMotion Easing = GhMotion.EaseInOut,
    ReadOnlyMemory<float> Dashes = default) {
    internal static readonly ReadOnlyMemory<float> DashedPattern = new[] { 4f, 4f };
    public static SnapGuideStyle Dashed(Color tint) => new(Tint: tint, Dashes: DashedPattern);
    public static SnapGuideStyle Solid(Color tint) => new(Tint: tint);
}

// Fire-and-forget GPU chrome on CALayer; no mid-flight retarget — dispose and re-issue to change bounds/tint/path.
// Easing picks the Core Animation media-timing curve for the fade/stroke lifecycle (TimingName); set Keyframe to
// drive the full 46-curve Easing via a sampled CAKeyFrameAnimation instead. Completion fires once on natural
// animation end (never on explicit dispose), after the layer strips.
[SkipUnionOps]
[Union]
public partial record CosmeticIntent {
    private CosmeticIntent() { }
    public Option<Func<Unit>> Completion { get; init; }
    public Option<Easing> Keyframe { get; init; }
    // GPU spring opt-in (additive, default None): when present the property animation is a CASpringAnimation
    // driven by the SAME SpringConfig as the CPU spring, unifying CPU+GPU spring on one parameterization.
    public Option<SpringConfig> Spring { get; init; }
    // Co-animation opt-in (additive, default None): the child intents' animations are grouped onto the SAME
    // layer as this intent under ONE completion delegate — children must target DISTINCT key-paths.
    public Option<Seq<CosmeticIntent>> CoAnimate { get; init; }
    public sealed record PulseCase(RectangleF Bounds, Color Tint, GhDuration Duration, int Cycles = 1, bool Yoyo = true, bool Infinite = false, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record GlowCase(RectangleF Bounds, Color Tint, float CornerRadius, float ShadowRadius, GhDuration Duration, int Cycles = 1, bool Yoyo = true, bool Infinite = false, GhMotion Easing = GhMotion.EaseInOut, float BorderWidth = 0f, Option<Color> BorderColor = default, float BlurRadius = 0f) : CosmeticIntent;
    public sealed record StrokeOnCase(ReadOnlyMemory<PointF> Polyline, Color Tint, float Thickness, GhDuration Duration, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record GradientCase(
        RectangleF Bounds,
        Seq<Color> Colors,
        GradientKind Kind,
        GhDuration Duration,
        CosmeticGradientPoints Points = default,
        int Cycles = 1,
        bool Yoyo = true,
        bool Infinite = false,
        GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record TextLayerCase(string Text, PointF Origin, Color Tint, float FontSize, GhDuration Duration, int Cycles = 1, bool Yoyo = true, bool Infinite = false, Option<string> FontFamily = default, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record ReplicatorCase(RectangleF SourceBounds, int Count, float Spacing, Color Tint, GhDuration Duration, float InstanceDelay = 0f, float InstanceAlphaOffset = 0f, Option<Color> InstanceColour = default, float Rotation = 0f, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record EmitterCase(
        RectangleF Bounds,
        Color Tint,
        GhDuration Duration,
        EmitterShape Shape,
        float BirthRate = 20f,
        float Lifetime = 1.4f,
        float Velocity = 40f,
        float Scale = 0.2f,
        float ScaleSpeed = -0.12f,
        float AlphaSpeed = -0.7f,
        float ColorRange = 0.1f,
        float LifetimeRangeRatio = 0.3f,
        float VelocityRangeRatio = 0.5f,
        float ScaleRangeRatio = 0.5f,
        float EmissionRange = MathF.Tau,
        GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record SnapGuideCase(SnappingSnapshot Snapshot, SnapGuideStyle Style) : CosmeticIntent;

    // Common entry: two-colour gradient. Seq overload below carries arbitrary multi-stop palettes.
    public static CosmeticIntent Gradient(RectangleF bounds, Color start, Color end, GradientKind kind, GhDuration duration, CosmeticGradientPoints points = default, GhMotion easing = GhMotion.EaseInOut, int cycles = 1, bool yoyo = true, bool infinite = false) =>
        new GradientCase(Bounds: bounds, Colors: Seq(start, end), Kind: kind, Duration: duration, Points: points, Cycles: cycles, Yoyo: yoyo, Infinite: infinite, Easing: easing);
    public static CosmeticIntent Gradient(RectangleF bounds, Seq<Color> colors, GradientKind kind, GhDuration duration, CosmeticGradientPoints points = default, GhMotion easing = GhMotion.EaseInOut, int cycles = 1, bool yoyo = true, bool infinite = false) =>
        new GradientCase(Bounds: bounds, Colors: colors, Kind: kind, Duration: duration, Points: points, Cycles: cycles, Yoyo: yoyo, Infinite: infinite, Easing: easing);
    public static CosmeticIntent Emitter(RectangleF bounds, Color tint, GhDuration duration, EmitterShape? shape = null, float birthRate = 20f, float lifetime = 1.4f, float velocity = 40f, GhMotion easing = GhMotion.EaseInOut) =>
        new EmitterCase(Bounds: bounds, Tint: tint, Duration: duration, Shape: shape ?? EmitterShape.Circle, BirthRate: birthRate, Lifetime: lifetime, Velocity: velocity, Easing: easing);
}

public interface IMotionVector<T> {
    public T Zero { get; }
    public float RestEpsilon { get; }
    public T Add(T a, T b);
    public T Subtract(T a, T b);
    public T Scale(T value, float scalar);
    public float Norm(T value);
    public T Interpolate(T value0, T value1, double factor);
    // Opaque-T springs cannot otherwise reject a NaN target (NaN < eps is always false → infinite pump);
    // the type owns per-component finiteness so the generic entrypoints can gate start/target.
    public bool IsFinite(T value);
}

internal interface IMotionState<TSelf> where TSelf : struct, IMotionState<TSelf> {
    public TSelf Step(float frameDeltaSeconds);
    public Unit Emit();
    public bool IsActive { get; }
}

// Robert Penner closed-form curves (easings.net).
[SmartEnum<int>]
public sealed partial class Easing {
    private const double BackC1 = 1.70158;
    private const double BackC3 = BackC1 + 1.0;
    private const double BackC2 = BackC1 * 1.525;
    private const double BounceN1 = 7.5625;
    private const double BounceD1 = 2.75;

    public static readonly Easing Linear = Native(key: 0, motion: GhMotion.Linear), LinearDelayed = Native(key: 1, motion: GhMotion.LinearDelayed), EaseIn = Native(key: 2, motion: GhMotion.EaseIn), EaseInDelayed = Native(key: 3, motion: GhMotion.EaseInDelayed);
    public static readonly Easing EaseOut = Native(key: 4, motion: GhMotion.EaseOut), EaseOutDelayed = Native(key: 5, motion: GhMotion.EaseOutDelayed), EaseInOut = Native(key: 6, motion: GhMotion.EaseInOut), EaseInOutDelayed = Native(key: 7, motion: GhMotion.EaseInOutDelayed);
    public static readonly Easing SnapIn = Native(key: 8, motion: GhMotion.SnapIn), SnapInDelayed = Native(key: 9, motion: GhMotion.SnapInDelayed), SnapOut = Native(key: 10, motion: GhMotion.SnapOut), SnapOutDelayed = Native(key: 11, motion: GhMotion.SnapOutDelayed);
    public static readonly Easing Bounce = Native(key: 12, motion: GhMotion.Bounce), BounceDelayed = Native(key: 13, motion: GhMotion.BounceDelayed), Twang = Native(key: 14, motion: GhMotion.Twang), TwangDelayed = Native(key: 15, motion: GhMotion.TwangDelayed);
    public static readonly Easing SineIn = new(key: 16, apply: static t => 1.0 - Math.Cos(t * Math.PI / 2.0)), SineOut = new(key: 17, apply: static t => Math.Sin(t * Math.PI / 2.0)), SineInOut = new(key: 18, apply: static t => -(Math.Cos(Math.PI * t) - 1.0) / 2.0);
    public static readonly Easing QuadIn = new(key: 19, apply: static t => t * t), QuadOut = new(key: 20, apply: static t => 1.0 - ((1.0 - t) * (1.0 - t))), QuadInOut = new(key: 21, apply: static t => t < 0.5 ? 2.0 * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 2.0) / 2.0));
    public static readonly Easing CubicIn = new(key: 22, apply: static t => t * t * t), CubicOut = new(key: 23, apply: static t => 1.0 - Math.Pow(1.0 - t, 3.0)), CubicInOut = new(key: 24, apply: static t => t < 0.5 ? 4.0 * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 3.0) / 2.0));
    public static readonly Easing QuartIn = new(key: 25, apply: static t => t * t * t * t), QuartOut = new(key: 26, apply: static t => 1.0 - Math.Pow(1.0 - t, 4.0)), QuartInOut = new(key: 27, apply: static t => t < 0.5 ? 8.0 * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 4.0) / 2.0));
    public static readonly Easing QuintIn = new(key: 28, apply: static t => t * t * t * t * t), QuintOut = new(key: 29, apply: static t => 1.0 - Math.Pow(1.0 - t, 5.0)), QuintInOut = new(key: 30, apply: static t => t < 0.5 ? 16.0 * t * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 5.0) / 2.0));
    public static readonly Easing ExpoIn = new(key: 31, apply: static t => t == 0.0 ? 0.0 : Math.Pow(2.0, (10.0 * t) - 10.0)), ExpoOut = new(key: 32, apply: static t => t == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * t)), ExpoInOut = new(key: 33, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : t < 0.5 ? Math.Pow(2.0, (20.0 * t) - 10.0) / 2.0 : (2.0 - Math.Pow(2.0, (-20.0 * t) + 10.0)) / 2.0);
    public static readonly Easing CircIn = new(key: 34, apply: static t => 1.0 - Math.Sqrt(1.0 - (t * t))), CircOut = new(key: 35, apply: static t => Math.Sqrt(1.0 - ((t - 1.0) * (t - 1.0)))), CircInOut = new(key: 36, apply: static t => t < 0.5 ? (1.0 - Math.Sqrt(1.0 - (4.0 * t * t))) / 2.0 : (Math.Sqrt(1.0 - (((-2.0 * t) + 2.0) * ((-2.0 * t) + 2.0))) + 1.0) / 2.0);
    public static readonly Easing BackIn = new(key: 37, apply: static t => (BackC3 * t * t * t) - (BackC1 * t * t)), BackOut = new(key: 38, apply: static t => 1.0 + (BackC3 * Math.Pow(t - 1.0, 3.0)) + (BackC1 * Math.Pow(t - 1.0, 2.0))), BackInOut = new(key: 39, apply: static t => t < 0.5 ? Math.Pow(2.0 * t, 2.0) * (((BackC2 + 1.0) * 2.0 * t) - BackC2) / 2.0 : ((Math.Pow((2.0 * t) - 2.0, 2.0) * (((BackC2 + 1.0) * ((t * 2.0) - 2.0)) + BackC2)) + 2.0) / 2.0);
    public static readonly Easing ElasticIn = new(key: 40, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : -Math.Pow(2.0, (10.0 * t) - 10.0) * Math.Sin(((t * 10.0) - 10.75) * (2.0 * Math.PI / 3.0))), ElasticOut = new(key: 41, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : (Math.Pow(2.0, -10.0 * t) * Math.Sin(((t * 10.0) - 0.75) * (2.0 * Math.PI / 3.0))) + 1.0), ElasticInOut = new(key: 42, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : t < 0.5 ? -(Math.Pow(2.0, (20.0 * t) - 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5))) / 2.0 : (Math.Pow(2.0, (-20.0 * t) + 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5)) / 2.0) + 1.0);
    public static readonly Easing BounceIn = new(key: 43, apply: static t => 1.0 - BounceOutFormula(1.0 - t)), BounceOut = new(key: 44, apply: static t => BounceOutFormula(t)), BounceInOut = new(key: 45, apply: static t => t < 0.5 ? (1.0 - BounceOutFormula(1.0 - (2.0 * t))) / 2.0 : (1.0 + BounceOutFormula((2.0 * t) - 1.0)) / 2.0);

    [UseDelegateFromConstructor]
    public partial double Apply(double t);

    private static Easing Native(int key, GhMotion motion) =>
        new(key: key, apply: t => MotionEquations.Blend(motion: motion, parameter: t));

    private static double BounceOutFormula(double t) =>
        t switch {
            < 1.0 / BounceD1 => BounceN1 * t * t,
            < 2.0 / BounceD1 => (BounceN1 * (t - (1.5 / BounceD1)) * (t - (1.5 / BounceD1))) + 0.75,
            < 2.5 / BounceD1 => (BounceN1 * (t - (2.25 / BounceD1)) * (t - (2.25 / BounceD1))) + 0.9375,
            _ => (BounceN1 * (t - (2.625 / BounceD1)) * (t - (2.625 / BounceD1))) + 0.984375,
        };
}

public abstract record MotionRequest<T> : GhUiRequest<T> {
    protected static GrasshopperUiPolicy ScheduledCanvas => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);

    public sealed record Tween<TValue>(Animated<TValue> Animated, Action<TValue> Sink, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Tween(animated: Animated, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Spring<TValue>(TValue From, TValue To, SpringConfig Config, IMotionVector<TValue> Vector, Action<TValue> Sink, Option<TValue> InitialVelocity = default, TimeProvider? TimeSource = null, MotionClock? Clock = null) : MotionRequest<SpringHandle<TValue>> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<SpringHandle<TValue>> Apply(GrasshopperUi.Scope scope) => Motion.Spring(start: From, target: To, config: Config, vector: Vector, sink: Sink, initialVelocity: InitialVelocity, timeSource: TimeSource, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Pulse<TValue>(TValue From, TValue To, GhDuration Duration, GhMotion Easing, IMotionVector<TValue> Vector, Action<TValue> Sink, int Cycles = 1, bool Yoyo = false, bool Infinite = false, MotionClock? Clock = null) : MotionRequest<PulseHandle<TValue>> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<PulseHandle<TValue>> Apply(GrasshopperUi.Scope scope) => Motion.Pulse(start: From, target: To, duration: Duration, easing: Easing, cycles: Cycles, yoyo: Yoyo, infinite: Infinite, vector: Vector, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Stroke(AnimatedPath Path, GhDuration Duration, GhMotion Easing, PaintStyle Style, PointF Origin, float Scale = 1f, float Angle = 0f, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Stroke(path: Path, duration: Duration, easing: Easing, style: Style, origin: Origin, scale: Scale, angle: Angle, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Sparkle(ISparkle Instance) : MotionRequest<Unit> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Motion.Sparkle(instance: Instance).Run(scope: scope); }
    public sealed record Theme(Skin From, Skin To, GhDuration Duration, GhMotion Easing, Action<Skin> Sink, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Theme(start: From, target: To, duration: Duration, easing: Easing, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Navigate(PointF Centre, GhDuration Duration, float MinZoom = CanvasViewPolicy.DefaultMinimumZoom, float MaxZoom = CanvasViewPolicy.DefaultMaximumZoom) : MotionRequest<Unit> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(); internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Motion.Navigate(centre: Centre, duration: Duration, minZoom: MinZoom, maxZoom: MaxZoom).Run(scope: scope); }
    public sealed record ZoomGate(ZoomThreshold Threshold, Action<float> Sink, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.ZoomGate(threshold: Threshold, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Cosmetic(CosmeticIntent Intent) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(); internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Cosmetic(intent: Intent).Run(scope: scope); }
    // Multi-step tween: folds (target, duration, motion) steps through Animated<T>.Chain into one
    // Animated curve, then drives it as a single Motion.Tween (no per-step subscription chaining; GH2
    // exposes Chain, not Then).
    public sealed record Sequence<TValue>(TValue Start, Seq<(TValue Target, GhDuration Duration, GhMotion Motion)> Steps, IMotionVector<TValue> Vector, Action<TValue> Sink, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Sequence(start: Start, steps: Steps, vector: Vector, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<UiFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SpringConfig {
    public float Stiffness { get; }
    public float Damping { get; }
    public float Mass { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float stiffness, ref float damping, ref float mass) {
        Op op = Op.Of(name: nameof(SpringConfig));
        float stiffnessValue = stiffness;
        float dampingValue = damping;
        float massValue = mass;
        UiFault? fault = null;
        _ = op.AcceptAll(
                value: unit,
                o => float.IsFinite(stiffnessValue) && stiffnessValue > 0f
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: o, detail: $"Stiffness must be finite and > 0 (got {stiffnessValue:R}).")),
                o => float.IsFinite(dampingValue) && dampingValue >= 0f
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: o, detail: $"Damping must be finite and >= 0 (got {dampingValue:R}).")),
                o => float.IsFinite(massValue) && massValue > 0f
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: o, detail: $"Mass must be finite and > 0 (got {massValue:R}).")))
            .IfFail(err => { fault = (UiFault)err; return unit; });
        validationError = fault;
    }

    public static SpringConfig Response(float response, float dampingFraction, float mass = 1f) {
        float twoPiOverR = (float)(2.0 * Math.PI) / response;
        return Create(
            stiffness: twoPiOverR * twoPiOverR * mass,
            damping: 4f * (float)Math.PI * dampingFraction * mass / response,
            mass: mass);
    }

}
[SmartEnum<int>]
public sealed partial class SpringPreset {
    public SpringConfig Config { get; }

    public static readonly SpringPreset Relaxed = Of(key: 0, response: 0.65f, dampingFraction: 1.00f);
    public static readonly SpringPreset Standard = Of(key: 1, response: 0.35f, dampingFraction: 0.90f);
    public static readonly SpringPreset Expressive = Of(key: 2, response: 0.45f, dampingFraction: 0.65f);
    public static readonly SpringPreset Snappy = Of(key: 3, response: 0.30f, dampingFraction: 0.85f);
    public static readonly SpringPreset Smooth = Of(key: 4, response: 0.50f, dampingFraction: 1.00f);
    public static readonly SpringPreset Bouncy = Of(key: 5, response: 0.55f, dampingFraction: 0.50f);
    public static readonly SpringPreset Snappier = Of(key: 6, response: 0.25f, dampingFraction: 0.85f);
    public static readonly SpringPreset Sluggish = Of(key: 7, response: 1.00f, dampingFraction: 1.20f);

    private static SpringPreset Of(int key, float response, float dampingFraction) =>
        new(key: key, config: SpringConfig.Response(response: response, dampingFraction: dampingFraction));
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct SpringRunnerState<T>(T Value, T Velocity, T Target, SpringConfig Config, IMotionVector<T> Vector, Action<T> Sink, TimeProvider Clock, long Timestamp) : IMotionState<SpringRunnerState<T>> {
    // Analytic damped-spring step (Juckett): exact cached coefficients for the current dt, so the integration
    // is frame-rate-independent and unconditionally stable for any stiffness/mass — no dt clamp, and a single
    // large dt teleports to the exact analytic state. The coefficients ARE ∂(pos,vel)/∂(pos0,vel0) at t=dt.
    public SpringRunnerState<T> Step(float frameDeltaSeconds) {
        long now = Clock.GetTimestamp();
        float dt = frameDeltaSeconds > 0f
            ? frameDeltaSeconds
            : (float)Clock.GetElapsedTime(startingTimestamp: Timestamp, endingTimestamp: now).TotalSeconds;
        float omega0 = MathF.Sqrt(Config.Stiffness / Config.Mass);
        float zeta = Config.Damping / (2f * MathF.Sqrt(Config.Stiffness * Config.Mass));
        (float posPos, float posVel, float velPos, float velVel) = StepCoefficients(omega0: omega0, zeta: zeta, dt: dt);
        T displacement = Vector.Subtract(Value, Target);
        T newDisplacement = Vector.Add(Vector.Scale(displacement, posPos), Vector.Scale(Velocity, posVel));
        T newVelocity = Vector.Add(Vector.Scale(displacement, velPos), Vector.Scale(Velocity, velVel));
        return this with { Value = Vector.Add(Target, newDisplacement), Velocity = newVelocity, Timestamp = now };
    }

    // Juckett's cached step coefficients per regime; an epsilon band on ζ classifies under/critical/over and
    // a near-zero ω0 short-circuits to the identity step. Verified to machine precision against the continuous
    // solution's partial derivatives (the overdamped velPos trailing *z2 is correct, not a *z1 typo).
    private static (float PosPos, float PosVel, float VelPos, float VelVel) StepCoefficients(float omega0, float zeta, float dt) {
        const float epsilon = 1e-4f;
        float w0 = MathF.Max(0f, omega0);
        float z = MathF.Max(0f, zeta);

        static (float, float, float, float) Underdamped(float w0, float z, float dt) {
            float oz = w0 * z;
            float al = w0 * MathF.Sqrt(1f - (z * z));
            float e = MathF.Exp(-oz * dt);
            float cs = MathF.Cos(al * dt);
            float sn = MathF.Sin(al * dt);
            float ia = 1f / al;
            float expSin = e * sn;
            float expCos = e * cs;
            float eOzSinOa = e * oz * sn * ia;
            return (expCos + eOzSinOa, expSin * ia, (-expSin * al) - (oz * eOzSinOa), expCos - eOzSinOa);
        }

        static (float, float, float, float) Critical(float w0, float dt) {
            float e = MathF.Exp(-w0 * dt);
            float te = dt * e;
            float tef = te * w0;
            return (tef + e, te, -w0 * tef, e - tef);
        }

        static (float, float, float, float) Overdamped(float w0, float z, float dt) {
            float za = -w0 * z;
            float zb = w0 * MathF.Sqrt((z * z) - 1f);
            float z1 = za - zb;
            float z2 = za + zb;
            float e1 = MathF.Exp(z1 * dt);
            float e2 = MathF.Exp(z2 * dt);
            float invTwoZb = 1f / (2f * zb);
            float e1o = e1 * invTwoZb;
            float e2o = e2 * invTwoZb;
            float z1e1o = z1 * e1o;
            float z2e2o = z2 * e2o;
            return ((e1o * z2) - z2e2o + e2, e2o - e1o, (z1e1o - z2e2o + e2) * z2, z2e2o - z1e1o);
        }

        return (w0 < epsilon, z < 1f - epsilon, z > 1f + epsilon) switch {
            (true, _, _) => (1f, 0f, 0f, 1f),
            (_, true, _) => Underdamped(w0: w0, z: z, dt: dt),
            (_, _, true) => Overdamped(w0: w0, z: z, dt: dt),
            _ => Critical(w0: w0, dt: dt),
        };
    }

    public Unit Emit() { Sink(Value); return unit; }

    public bool IsActive =>
        !((Vector.Norm(Vector.Subtract(Value, Target)) < Vector.RestEpsilon) && (Vector.Norm(Velocity) < Vector.RestEpsilon));
}

public sealed class SpringHandle<T> : IDisposable {
    private readonly Atom<SpringRunnerState<T>> cell;
    private readonly Subscription subscription;
    private readonly Action wake;
    private readonly bool settled;

    internal SpringHandle(Atom<SpringRunnerState<T>> cell, Subscription subscription, Action wake, bool settled = false) {
        this.cell = cell;
        this.subscription = subscription;
        this.wake = wake;
        this.settled = settled;
    }

    public T CurrentValue => cell.Value.Value;
    public T CurrentVelocity => cell.Value.Velocity;
    public T CurrentTarget => cell.Value.Target;
    public bool IsConverged => !cell.Value.IsActive;

    public Fin<Unit> Retarget(T target, Option<T> initialVelocity = default) {
        SpringRunnerState<T> snapshot = cell.Value;
        return from validTarget in Op.Of(name: nameof(Retarget)).AcceptFinite(value: target, vector: snapshot.Vector, detail: "spring target/velocity must be finite")
               from validVelocity in initialVelocity
                   .Map(value => Op.Of(name: nameof(Retarget)).AcceptFinite(value: value, vector: snapshot.Vector, detail: "spring target/velocity must be finite").Map(Some))
                   .IfNone(Fin.Succ(value: Option<T>.None))
               select settled
                   ? Op.Side(() => {
                       _ = cell.Swap(state => state with {
                           Value = validTarget,
                           Target = validTarget,
                           Velocity = validVelocity.IfNone(state.Vector.Zero),
                           Timestamp = state.Clock.GetTimestamp(),
                       });
                       snapshot.Sink(validTarget);
                   })
                   : Op.Side(() => {
                       _ = cell.Swap(state => state with {
                           Target = validTarget,
                           Velocity = validVelocity.IfNone(state.Velocity),
                       });
                       wake();
                   });
    }

    public Fin<Unit> RetargetWhen(T target, Func<T, bool> shouldUpdate, Option<T> initialVelocity = default) {
        SpringRunnerState<T> snapshot = cell.Value;
        return from validUpdate in Optional(shouldUpdate).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(RetargetWhen)), detail: "predicate is required"))
               from validTarget in Op.Of(name: nameof(RetargetWhen)).AcceptFinite(value: target, vector: snapshot.Vector, detail: "spring target/velocity must be finite")
               from validVelocity in initialVelocity
                   .Map(value => Op.Of(name: nameof(RetargetWhen)).AcceptFinite(value: value, vector: snapshot.Vector, detail: "spring target/velocity must be finite").Map(Some))
                   .IfNone(Fin.Succ(value: Option<T>.None))
               from result in validUpdate(arg: snapshot.Target) switch {
                   false => Fin.Succ(value: unit),
                   true => Fin.Succ(value: settled
                       ? Op.Side(() => {
                           _ = cell.Swap(state => state with {
                               Value = validTarget,
                               Target = validTarget,
                               Velocity = validVelocity.IfNone(state.Vector.Zero),
                               Timestamp = state.Clock.GetTimestamp(),
                           });
                           snapshot.Sink(validTarget);
                       })
                       : Op.Side(() => {
                           _ = cell.Swap(state => state with {
                               Target = validTarget,
                               Velocity = validVelocity.IfNone(state.Velocity),
                           });
                           wake();
                       })),
               }
               select result;
    }

    public void Dispose() => subscription.Dispose();
}

// Mirror of SpringHandle<T>: an Atom-confined runner cell exposed for mid-flight Retarget. The
// MotionRunner re-reads the cell each CAS tick, so a concurrent Retarget re-seeds the Animated curve.
public sealed class PulseHandle<T> : IDisposable {
    private readonly Atom<PulseRunnerState<T>> cell;
    private readonly Subscription subscription;
    private readonly Action wake;
    private readonly bool settled;

    internal PulseHandle(Atom<PulseRunnerState<T>> cell, Subscription subscription, Action wake, bool settled = false) {
        this.cell = cell;
        this.subscription = subscription;
        this.wake = wake;
        this.settled = settled;
    }

    public T CurrentValue => cell.Value.Animated.ValueNow;
    public T CurrentTarget => cell.Value.To;
    public bool IsCycling => cell.Value.IsActive;

    public Fin<Unit> Retarget(T target, Option<int> cycles = default) {
        PulseRunnerState<T> snapshot = cell.Value;
        return from validTarget in Op.Of(name: nameof(Retarget)).AcceptFinite(value: target, vector: snapshot.Vector, detail: "pulse target must be finite")
               select settled
                   ? Op.Side(() => SettlePulse(snapshot: snapshot, target: validTarget))
                   : Op.Side(() => {
                       _ = cell.Swap(state => state with {
                           From = state.Animated.ValueNow,
                           To = validTarget,
                           CyclesRemaining = cycles.IfNone(state.CyclesRemaining),
                           Animated = Animated<T>.CreateUnfinished(
                               value0: state.Animated.ValueNow,
                               value1: validTarget,
                               duration: Animators.DurationToTimeSpan(duration: state.Duration),
                               motion: state.Easing,
                               interpolator: state.Vector.Interpolate),
                       });
                       wake();
                   });
    }

    // Symmetric with SpringHandle.RetargetWhen: only re-seed the curve when the predicate accepts the current
    // target, so a stale retarget is a no-op rather than a restart.
    public Fin<Unit> RetargetWhen(T target, Func<T, bool> shouldUpdate, Option<int> cycles = default) {
        PulseRunnerState<T> snapshot = cell.Value;
        return from validUpdate in Optional(shouldUpdate).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(RetargetWhen)), detail: "predicate is required"))
               from validTarget in Op.Of(name: nameof(RetargetWhen)).AcceptFinite(value: target, vector: snapshot.Vector, detail: "pulse target must be finite")
               from result in validUpdate(arg: snapshot.To) switch {
                   false => Fin.Succ(value: unit),
                   true => Fin.Succ(value: settled
                       ? Op.Side(() => SettlePulse(snapshot: snapshot, target: validTarget))
                       : Op.Side(() => {
                           _ = cell.Swap(state => state with {
                               From = state.Animated.ValueNow,
                               To = validTarget,
                               CyclesRemaining = cycles.IfNone(state.CyclesRemaining),
                               Animated = Animated<T>.CreateUnfinished(
                                   value0: state.Animated.ValueNow,
                                   value1: validTarget,
                                   duration: Animators.DurationToTimeSpan(duration: state.Duration),
                                   motion: state.Easing,
                                   interpolator: state.Vector.Interpolate),
                           });
                           wake();
                       })),
               }
               select result;
    }

    private void SettlePulse(PulseRunnerState<T> snapshot, T target) {
        T rest = snapshot.Yoyo ? snapshot.From : target;
        _ = cell.Swap(state => state with {
            From = rest,
            To = rest,
            CyclesRemaining = 0,
            Animated = Animated<T>.CreateUnfinished(
                value0: rest,
                value1: rest,
                duration: TimeSpan.Zero,
                motion: state.Easing,
                interpolator: state.Vector.Interpolate),
        });
        snapshot.Sink(rest);
    }

    public void Dispose() => subscription.Dispose();
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct PulseRunnerState<T>(Animated<T> Animated, T From, T To, GhDuration Duration, GhMotion Easing, bool Yoyo, bool Infinite, int CyclesRemaining, IMotionVector<T> Vector, Action<T> Sink) : IMotionState<PulseRunnerState<T>> {
    public PulseRunnerState<T> Step(float _) =>
        (Animated.State == GhState.Finished, Infinite || CyclesRemaining > 0) switch {
            (true, true) => this with {
                Animated = Animated<T>.CreateUnfinished(
                    value0: Yoyo ? Animated.Value1 : From,
                    value1: Yoyo ? Animated.Value0 : To,
                    duration: Animators.DurationToTimeSpan(duration: Duration),
                    motion: Easing,
                    interpolator: Vector.Interpolate),
                CyclesRemaining = Infinite ? CyclesRemaining : CyclesRemaining - 1,
            },
            _ => this,
        };

    public Unit Emit() { Sink(Animated.ValueNow); return unit; }

    public bool IsActive => Animated.State != GhState.Finished || Infinite || CyclesRemaining > 0;
}

public static class MotionVector {
    private const float ScalarRest = 0.001f;
    private const float PixelRest = 0.5f;
    private const float ChannelRest = 1f / 255f;

    public static readonly IMotionVector<float> Float = new MotionVectorImpl<float>(
        zero: 0f,
        restEpsilon: ScalarRest,
        add: static (a, b) => a + b,
        subtract: static (a, b) => a - b,
        scale: static (v, s) => v * s,
        norm: static v => Math.Abs(v),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t),
        isFinite: static v => float.IsFinite(v));
    public static readonly IMotionVector<double> Double = new MotionVectorImpl<double>(
        zero: 0.0,
        restEpsilon: ScalarRest,
        add: static (a, b) => a + b,
        subtract: static (a, b) => a - b,
        scale: static (v, s) => v * s,
        norm: static v => (float)Math.Abs(v),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t),
        isFinite: static v => double.IsFinite(v));
    public static readonly IMotionVector<PointF> PointF = new MotionVectorImpl<PointF>(
        zero: Eto.Drawing.PointF.Empty,
        restEpsilon: PixelRest,
        add: static (a, b) => a + b,
        subtract: static (a, b) => a - b,
        scale: static (v, s) => v * s,
        norm: static v => v.Length,
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t),
        isFinite: static v => float.IsFinite(v.X) && float.IsFinite(v.Y));
    public static readonly IMotionVector<SizeF> SizeF = new MotionVectorImpl<SizeF>(
        zero: Eto.Drawing.SizeF.Empty,
        restEpsilon: PixelRest,
        add: static (a, b) => a + b,
        subtract: static (a, b) => a - b,
        scale: static (v, s) => v * s,
        norm: static v => MathF.Sqrt((v.Width * v.Width) + (v.Height * v.Height)),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t),
        isFinite: static v => float.IsFinite(v.Width) && float.IsFinite(v.Height));
    public static readonly IMotionVector<RectangleF> RectangleF = new MotionVectorImpl<RectangleF>(
        zero: Eto.Drawing.RectangleF.Empty,
        restEpsilon: PixelRest,
        add: static (a, b) => new RectangleF(a.X + b.X, a.Y + b.Y, a.Width + b.Width, a.Height + b.Height),
        subtract: static (a, b) => new RectangleF(a.X - b.X, a.Y - b.Y, a.Width - b.Width, a.Height - b.Height),
        scale: static (v, s) => new RectangleF(v.X * s, v.Y * s, v.Width * s, v.Height * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.X), Math.Abs(v.Y)), Math.Max(Math.Abs(v.Width), Math.Abs(v.Height))),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t),
        isFinite: static v => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Width) && float.IsFinite(v.Height));
    public static readonly IMotionVector<Color> Color = ColorChannels(static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static double Phase(TimeSpan period, TimeProvider? clock = null) {
        TimeProvider source = clock ?? TimeProvider.System;
        double seconds = (double)source.GetTimestamp() / source.TimestampFrequency;
        double cycle = period.TotalSeconds;
        return cycle <= 0d ? 0d : seconds % cycle / cycle;
    }

    // Shortest-arc HSL interpolation.
    public static readonly IMotionVector<Color> ColorHSL = ColorChannels(HslInterpolate);

    // OKLab perceptual interpolation (Ottosson 2020 / CSS Color Level 4). Avoids sRGB dead-grey midpoint.
    public static readonly IMotionVector<Color> ColorOklab = ColorChannels(OklabInterpolate);

    // OKLCH: cylindrical OKLab. Hue takes shortest arc; L/C interpolate in OKLab.
    public static readonly IMotionVector<Color> ColorOklch = ColorChannels(OklchInterpolate);

    private static MotionVectorImpl<Color> ColorChannels(Func<Color, Color, double, Color> interpolate) =>
        new(
            zero: new Color(red: 0f, green: 0f, blue: 0f, alpha: 0f),
            restEpsilon: ChannelRest,
            add: static (a, b) => new Color(red: a.R + b.R, green: a.G + b.G, blue: a.B + b.B, alpha: a.A + b.A),
            subtract: static (a, b) => new Color(red: a.R - b.R, green: a.G - b.G, blue: a.B - b.B, alpha: a.A - b.A),
            scale: static (v, s) => new Color(red: v.R * s, green: v.G * s, blue: v.B * s, alpha: v.A * s),
            norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
            interpolate: interpolate,
            isFinite: static v => float.IsFinite(v.R) && float.IsFinite(v.G) && float.IsFinite(v.B) && float.IsFinite(v.A));

    private static Color HslInterpolate(Color a, Color b, double factor) {
        ColorHSL ha = a;
        ColorHSL hb = b;
        float tf = (float)factor;
        // Shortest hue arc in [-180, +180].
        float hueDelta = ((hb.H - ha.H + 540f) % 360f) - 180f;
        float alphaA = ha.A;
        float alphaB = hb.A;
        float aMix = alphaA + ((alphaB - alphaA) * tf);
        // Premultiply S/L by alpha (hue is angular), mix, un-premultiply — no bleed from a transparent endpoint.
        float sMix = (ha.S * alphaA) + (((hb.S * alphaB) - (ha.S * alphaA)) * tf);
        float lMix = (ha.L * alphaA) + (((hb.L * alphaB) - (ha.L * alphaA)) * tf);
        return new ColorHSL(
            hue: ha.H + (hueDelta * tf),
            saturation: aMix > 0f ? sMix / aMix : 0f,
            luminance: aMix > 0f ? lMix / aMix : 0f,
            alpha: aMix);
    }

    // --- [OKLAB CONVERSION] ----------------------------------------------------------------
    // Ottosson 2020: sRGB → linear → LMS → OKLab and back.
    // Premultiplied-alpha mixing (CSS Color Level 4 §12.4): premultiply L/a/b by alpha before the mix and
    // un-premultiply after, so a transparent endpoint contributes no colour (no hue bleed at aMix==0).
    private static Color OklabInterpolate(Color a, Color b, double factor) {
        (float la, float aa, float ba) = SrgbToOklab(a);
        (float lb, float ab, float bb) = SrgbToOklab(b);
        float tf = (float)factor;
        float alphaA = a.A;
        float alphaB = b.A;
        float aMix = alphaA + ((alphaB - alphaA) * tf);
        float lMix = (la * alphaA) + (((lb * alphaB) - (la * alphaA)) * tf);
        float aChMix = (aa * alphaA) + (((ab * alphaB) - (aa * alphaA)) * tf);
        float bMix = (ba * alphaA) + (((bb * alphaB) - (ba * alphaA)) * tf);
        (float L, float A, float B) mix = aMix > 0f
            ? (L: lMix / aMix, A: aChMix / aMix, B: bMix / aMix)
            : (L: 0f, A: 0f, B: 0f);
        return OklabToSrgb(mix, alpha: aMix);
    }

    private static Color OklchInterpolate(Color a, Color b, double factor) {
        (float la, float aa, float ba) = SrgbToOklab(a);
        (float lb, float ab, float bb) = SrgbToOklab(b);
        float ca = MathF.Sqrt((aa * aa) + (ba * ba));
        float cb = MathF.Sqrt((ab * ab) + (bb * bb));
        float ha = MathF.Atan2(ba, aa);
        float hb = MathF.Atan2(bb, ab);
        // Shortest hue arc in radians.
        float hueDelta = ((hb - ha + (3f * MathF.PI)) % (2f * MathF.PI)) - MathF.PI;
        float tf = (float)factor;
        float alphaA = a.A;
        float alphaB = b.A;
        float aMix = alphaA + ((alphaB - alphaA) * tf);
        float hMix = ha + (hueDelta * tf);
        // Premultiply L and chroma by alpha (hue is angular, interpolated directly), then un-premultiply.
        float lMix = (la * alphaA) + (((lb * alphaB) - (la * alphaA)) * tf);
        float cMix = (ca * alphaA) + (((cb * alphaB) - (ca * alphaA)) * tf);
        (float lcL, float lcC) = aMix > 0f ? (lMix / aMix, cMix / aMix) : (0f, 0f);
        (float L, float A, float B) mix = (lcL, lcC * MathF.Cos(hMix), lcC * MathF.Sin(hMix));
        return OklabToSrgb(mix, alpha: aMix);
    }

    private static (float L, float A, float B) SrgbToOklab(Color rgb) {
        float r = SrgbToLinear(rgb.R);
        float g = SrgbToLinear(rgb.G);
        float bl = SrgbToLinear(rgb.B);
        float l = (0.4122214708f * r) + (0.5363325363f * g) + (0.0514459929f * bl);
        float m = (0.2119034982f * r) + (0.6806995451f * g) + (0.1073969566f * bl);
        float s = (0.0883024619f * r) + (0.2817188376f * g) + (0.6299787005f * bl);
        float l_ = MathF.Cbrt(l);
        float m_ = MathF.Cbrt(m);
        float s_ = MathF.Cbrt(s);
        return (
            L: (0.2104542553f * l_) + (0.7936177850f * m_) - (0.0040720468f * s_),
            A: (1.9779984951f * l_) - (2.4285922050f * m_) + (0.4505937099f * s_),
            B: (0.0259040371f * l_) + (0.7827717662f * m_) - (0.8086757660f * s_));
    }

    private static Color OklabToSrgb((float L, float A, float B) lab, float alpha) {
        float l_ = lab.L + (0.3963377774f * lab.A) + (0.2158037573f * lab.B);
        float m_ = lab.L - (0.1055613458f * lab.A) - (0.0638541728f * lab.B);
        float s_ = lab.L - (0.0894841775f * lab.A) - (1.2914855480f * lab.B);
        float l = l_ * l_ * l_;
        float m = m_ * m_ * m_;
        float s = s_ * s_ * s_;
        float r = (4.0767416621f * l) - (3.3077115913f * m) + (0.2309699292f * s);
        float g = (-1.2684380046f * l) + (2.6097574011f * m) - (0.3413193965f * s);
        float bl = (-0.0041960863f * l) - (0.7034186147f * m) + (1.7076147010f * s);
        return new Color(
            red: LinearToSrgb(Math.Clamp(r, 0f, 1f)),
            green: LinearToSrgb(Math.Clamp(g, 0f, 1f)),
            blue: LinearToSrgb(Math.Clamp(bl, 0f, 1f)),
            alpha: alpha);
    }

    private static float SrgbToLinear(float c) =>
        c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);

    private static float LinearToSrgb(float c) =>
        c <= 0.0031308f ? 12.92f * c : (1.055f * MathF.Pow(c, 1f / 2.4f)) - 0.055f;
}

internal sealed class MotionVectorImpl<T>(
    T zero,
    float restEpsilon,
    Func<T, T, T> add,
    Func<T, T, T> subtract,
    Func<T, float, T> scale,
    Func<T, float> norm,
    Func<T, T, double, T> interpolate,
    Func<T, bool> isFinite) : IMotionVector<T> {
    public T Zero { get; } = zero;
    public float RestEpsilon { get; } = restEpsilon;
    public T Add(T a, T b) => add(arg1: a, arg2: b);
    public T Subtract(T a, T b) => subtract(arg1: a, arg2: b);
    public T Scale(T value, float scalar) => scale(arg1: value, arg2: scalar);
    public float Norm(T value) => norm(arg: value);
    public T Interpolate(T value0, T value1, double factor) => interpolate(arg1: value0, arg2: value1, arg3: factor);
    public bool IsFinite(T value) => isFinite(arg: value);
}

public static class Spark {
    public static ISparkle Blast(BlastRadius radius, PointF location, Color colour, bool attachedToContent = false) =>
        new BlastSparkle(radius: radius, location: location, colour: colour, attachedToContent: attachedToContent);
    public static ISparkle Edge(PointF point0, PointF point1, bool attachedToContent = false) =>
        new EdgeSparkle(edge0: point0, edge1: point1, attachedToContent: attachedToContent);
    public static ISparkle EdgeAnchored(Func<PointF> edge0, Func<PointF> edge1, bool attachedToContent = true) =>
        new EdgeSparkle(edge0: edge0, edge1: edge1, attachedToContent: attachedToContent);
    public static ISparkle Face(RectangleF face, bool attachedToContent = false) =>
        new FaceSparkle(face: face, attachedToContent: attachedToContent);
    public static ISparkle FaceAnchored(Func<GraphicsPath> face, bool attachedToContent = true) =>
        new FaceSparkle(face: face, attachedToContent: attachedToContent);
    public static ISparkle Notice(NoticeType notice, PointF location, bool attachedToContent = false) =>
        new NoticeSparkle(notice: notice, location: location, attachedToContent: attachedToContent);
    public static ISparkle NoticeAnchored(NoticeType notice, Func<PointF> location, bool attachedToContent = true) =>
        new NoticeSparkle(notice: notice, location: location, attachedToContent: attachedToContent);
}

public static class Glyph {
    public static AnimatedPath Error(float size = 1f) => AnimatedPath.CreateErrorPath(size: size);
    public static AnimatedPath Warning(float size = 1f) => AnimatedPath.CreateWarningPath(size: size);
    public static AnimatedPath Success(float size = 1f) => AnimatedPath.CreateSuccessPath(size: size);
    public static AnimatedPath Message(float size = 1f) => AnimatedPath.CreateMessagePath(size: size);
    public static AnimatedPath Arrow(float size, float angle = 0f) => AnimatedPath.CreateArrowPath(size: size, angle: angle);
    public static AnimatedPath Of(IEnumerable<IAnimatedStroke> strokes) => new(strokes: strokes);
    public static IAnimatedStroke Line(PointF point0, PointF point1) => new LineStroke(point0: point0, point1: point1);
    public static IAnimatedStroke Arc(ArcF arc) => new ArcStroke(arc: arc);
    public static IAnimatedStroke Gap(PointF point0, PointF point1) => new GapStroke(point0: point0, point1: point1);
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class Motion {
    // Default retina backing scale when a screen cannot be resolved.
    private const float RetinaScaleDefault = 2f;

    // Shared scaffold: NeedCanvas + PacerOption + Paint.Hook + bundle + initial wake. Variant services
    // (Spring exposing a cell, etc.) consume RunPipeline directly with their own canvas/pacer setup.
    internal static GrasshopperUiIntent<Subscription> Pipeline(
        string opName, CanvasPaintPhase phase, MotionClock clock,
        Func<Grasshopper2.UI.Canvas.Canvas, Option<Pacer>, Func<PaintScope, Fin<Unit>>> paintFactory) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from pacer in PacerOption(canvas: canvas, clock: clock)
            from bundle in RunPipeline(canvas: canvas, pacer: pacer, opName: opName, phase: phase, clock: clock,
                paint: paintFactory(arg1: canvas, arg2: pacer)).Run(scope: scope)
            select bundle.Subscription);

    internal static GrasshopperUiIntent<(Subscription Subscription, Action Wake)> RunPipeline(
        Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer,
        string opName, CanvasPaintPhase phase, MotionClock clock, Func<PaintScope, Fin<Unit>> paint) =>
        GhUi.Canvas(run: scope =>
            from sub in Paint.Hook(phase: phase, paint: paint, clock: clock, adoptedPacer: pacer).Run(scope: scope)
            let bundle = MotionBundleOf(pacer: pacer, sub: sub, canvas: canvas)
            from kickoff in Op.Of(name: opName).Attempt(body: bundle.Wake, what: $"{opName} initial wake")
                .MapFail(error => UiFault.ThreadMarshal(detail: error.Message))
            select (bundle.Subscription, bundle.Wake));

    internal static GrasshopperUiIntent<Subscription> Tween<TValue>(
        Animated<TValue> animated, Action<TValue> sink, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Tween)), detail: "sink delegate is required"))
            from result in Pipeline(opName: nameof(Tween), phase: CanvasPaintPhase.BeforeBackground, clock: clock,
                paintFactory: (canvas, pacer) => _ => Try.lift(f: () => {
                    if (MotionAccessibility.ShouldReduceMotion) {
                        validSink(animated.Value1);
                        return unit;
                    }
                    validSink(canvas.Animate(animated: animated));
                    return PauseWhenFinished(state: animated.State, pacer: pacer);
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Tween)), detail: $"tick threw: {error.Message}"))).Run(scope: scope)
            select result);

    internal static GrasshopperUiIntent<SpringHandle<TValue>> Spring<TValue>(
        TValue start, TValue target, SpringConfig config, IMotionVector<TValue> vector,
        Action<TValue> sink, Option<TValue> initialVelocity, TimeProvider? timeSource, MotionClock clock) =>
        MotionAccessibility.ShouldReduceMotion
            ? SettledSpring(
                target: target, config: config, vector: vector, sink: sink, timeSource: timeSource)
            : AnimatedSpring(
                start: start, target: target, config: config, vector: vector, sink: sink,
                initialVelocity: initialVelocity, timeSource: timeSource, clock: clock);

    private static GrasshopperUiIntent<SpringHandle<TValue>> SettledSpring<TValue>(
        TValue target, SpringConfig config, IMotionVector<TValue> vector,
        Action<TValue> sink, TimeProvider? timeSource) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "sink delegate is required"))
            from _finite in validVector.IsFinite(target)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "spring target must be finite"))
            from canvas in scope.NeedCanvas()
            from _ in Op.Of(name: nameof(Spring)).Attempt(body: () => { validSink(target); return unit; }, what: "spring reduce-motion settle")
            let resolvedClock = timeSource ?? TimeProvider.System
            select new SpringHandle<TValue>(
                cell: Atom(new SpringRunnerState<TValue>(
                    Value: target,
                    Velocity: validVector.Zero,
                    Target: target, Config: config, Vector: validVector, Sink: validSink,
                    Clock: resolvedClock, Timestamp: resolvedClock.GetTimestamp())),
                subscription: Subscription.Empty,
                wake: canvas.ScheduleRedraw,
                settled: true));

    private static GrasshopperUiIntent<SpringHandle<TValue>> AnimatedSpring<TValue>(
        TValue start, TValue target, SpringConfig config, IMotionVector<TValue> vector,
        Action<TValue> sink, Option<TValue> initialVelocity, TimeProvider? timeSource, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "sink delegate is required"))
            from _finite in validVector.IsFinite(start) && validVector.IsFinite(target) && initialVelocity.Map(value => validVector.IsFinite(value)).IfNone(true)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "spring start/target/velocity must be finite"))
            from canvas in scope.NeedCanvas()
            from pacer in PacerOption(canvas: canvas, clock: clock)
            let resolvedClock = timeSource ?? TimeProvider.System
            let cell = Atom(new SpringRunnerState<TValue>(
                Value: start,
                Velocity: initialVelocity.IfNone(validVector.Zero),
                Target: target, Config: config, Vector: validVector, Sink: validSink,
                Clock: resolvedClock, Timestamp: resolvedClock.GetTimestamp()))
            from bundle in RunPipeline(canvas: canvas, pacer: pacer, opName: nameof(Spring), phase: CanvasPaintPhase.BeforeBackground, clock: clock,
                paint: RunnerPaint(cell: cell, canvas: canvas, pacer: pacer, opName: nameof(Spring))).Run(scope: scope)
            select new SpringHandle<TValue>(cell: cell, subscription: bundle.Subscription, wake: bundle.Wake));

    internal static (Subscription Subscription, Action Wake) MotionBundleOf(
        Option<Pacer> pacer, Subscription sub, Grasshopper2.UI.Canvas.Canvas canvas) {
        Action wake = canvas.ScheduleRedraw;
        return (Subscription: Subscription.DisposeOnce(sub), Wake: wake);
    }

    // DisplayLink demotes to None under AccessibilityShouldReduceMotion — caller falls through
    // to message-loop coalesced 60Hz so the transition still lands (never silent skip).
    internal static Fin<Option<Pacer>> PacerOption(Grasshopper2.UI.Canvas.Canvas canvas, MotionClock clock) =>
        clock.Switch(state: canvas,
            messageLoopCase: static (_, _) => Fin.Succ(Option<Pacer>.None),
            displayLinkCase: static (c, d) => MotionAccessibility.ShouldReduceMotion
                ? Fin.Succ(Option<Pacer>.None)
                : Pacer.For(canvas: c, rate: d.Rate.ToNullable()).Map(Some));

    internal static Unit PacerResume(Option<Pacer> pacer, Grasshopper2.UI.Canvas.Canvas canvas) =>
        pacer is { IsSome: true, Case: Pacer p } ? Op.Side(() => p.Resume()) : Op.Side(canvas.ScheduleRedraw);

    internal static Unit PacerRelease(Option<Pacer> pacer) =>
        pacer is { IsSome: true, Case: Pacer p } ? Op.Side(() => { _ = p.Pause(); _ = p.Release(); }) : unit;

    internal static Unit PauseWhenFinished(GhState state, Option<Pacer> pacer) =>
        state == GhState.Finished && pacer is { IsSome: true, Case: Pacer p }
            ? p.Pause()
            : unit;

    private static Func<PaintScope, Fin<Unit>> RunnerPaint<TState>(
        Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer, string opName)
        where TState : struct, IMotionState<TState> {
        // One runner per pipeline, hoisted into the factory closure: wantingTicks / lastVsyncTimestamp
        // must persist across frames or the Pacer.LastTargetTimestamp vsync-dt path never accumulates and
        // Resume re-fires every frame. The per-frame paint lambda only calls Tick on the captured runner.
        MotionRunner<TState> runner = MotionRunner<TState>.Of(cell: cell, canvas: canvas, pacer: pacer);
        return paintScope => Op.Of(name: opName).Attempt(
            body: () => { _ = runner.Tick(); return unit; },
            what: "motion tick");
    }

    internal static GrasshopperUiIntent<PulseHandle<TValue>> Pulse<TValue>(
        TValue start, TValue target, GhDuration duration, GhMotion easing, int cycles, bool yoyo, bool infinite,
        IMotionVector<TValue> vector, Action<TValue> sink, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "sink delegate is required"))
            from _finite in validVector.IsFinite(start) && validVector.IsFinite(target)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "pulse start/target must be finite"))
            from validCycles in infinite
                ? Fin.Succ(1)
                : Optional(cycles).Filter(static c => c >= 1)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "cycles must be positive when not infinite"))
            from handle in (MotionAccessibility.ShouldReduceMotion
                ? SettledPulse(start: start, target: target, duration: duration, easing: easing, yoyo: yoyo, infinite: infinite, vector: validVector, sink: validSink)
                : AnimatedPulse(start: start, target: target, duration: duration, easing: easing, yoyo: yoyo, infinite: infinite, cycles: validCycles, vector: validVector, sink: validSink, clock: clock)).Run(scope: scope)
            select handle);

    private static GrasshopperUiIntent<PulseHandle<TValue>> SettledPulse<TValue>(
        TValue start, TValue target, GhDuration duration, GhMotion easing, bool yoyo, bool infinite,
        IMotionVector<TValue> vector, Action<TValue> sink) =>
        // A yoyo's resting pose is its From (it returns to start); a one-way pulse rests at target.
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            let rest = yoyo ? start : target
            from _ in Op.Of(name: nameof(Pulse)).Attempt(body: () => { sink(rest); return unit; }, what: "pulse reduce-motion settle")
            select new PulseHandle<TValue>(
                cell: Atom(new PulseRunnerState<TValue>(
                    Animated: Animated<TValue>.CreateUnfinished(
                        value0: rest, value1: rest, duration: TimeSpan.Zero, motion: easing, interpolator: vector.Interpolate),
                    From: rest, To: rest, Duration: duration, Easing: easing, Yoyo: yoyo, Infinite: infinite,
                    CyclesRemaining: 0, Vector: vector, Sink: sink)),
                subscription: Subscription.Empty,
                wake: canvas.ScheduleRedraw,
                settled: true));

    private static GrasshopperUiIntent<PulseHandle<TValue>> AnimatedPulse<TValue>(
        TValue start, TValue target, GhDuration duration, GhMotion easing, bool yoyo, bool infinite, int cycles,
        IMotionVector<TValue> vector, Action<TValue> sink, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from pacer in PacerOption(canvas: canvas, clock: clock)
            let cell = Atom(new PulseRunnerState<TValue>(
                Animated: Animated<TValue>.CreateUnfinished(
                    value0: start, value1: target,
                    duration: Animators.DurationToTimeSpan(duration: duration),
                    motion: easing, interpolator: vector.Interpolate),
                From: start, To: target, Duration: duration, Easing: easing, Yoyo: yoyo, Infinite: infinite,
                CyclesRemaining: cycles - 1, Vector: vector, Sink: sink))
            from bundle in RunPipeline(canvas: canvas, pacer: pacer, opName: nameof(Pulse), phase: CanvasPaintPhase.BeforeBackground, clock: clock,
                paint: RunnerPaint(cell: cell, canvas: canvas, pacer: pacer, opName: nameof(Pulse))).Run(scope: scope)
            select new PulseHandle<TValue>(cell: cell, subscription: bundle.Subscription, wake: bundle.Wake));

    internal static GrasshopperUiIntent<Subscription> Sequence<TValue>(
        TValue start, Seq<(TValue Target, GhDuration Duration, GhMotion Motion)> steps,
        IMotionVector<TValue> vector, Action<TValue> sink, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Sequence)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Sequence)), detail: "sink delegate is required"))
            from validSteps in Optional(steps).Filter(static s => s.Count > 0)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Sequence)), detail: "at least one step is required"))
            from _finite in validVector.IsFinite(start) && validSteps.ForAll(s => validVector.IsFinite(s.Target))
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Sequence)), detail: "sequence start/targets must be finite"))
                // Boundary invariant: GH2 Animated.Chain short-circuits a step whose Target equals the prior value
                // (Value1.Equals(target)), so a dwell step targeting the prior pose is silently dropped — epsilon
                // perturbation is non-trivial for non-numeric TValue, so this is documented rather than worked around.
            let animated = validSteps.Fold(
                initialState: Animated<TValue>.CreateFinished(start, validVector.Interpolate),
                f: static (acc, step) => acc.Chain(step.Target, step.Duration, step.Motion))
            from sub in Tween(animated: animated, sink: validSink, clock: clock).Run(scope: scope)
            select sub);

    internal static GrasshopperUiIntent<Subscription> Stroke(
        AnimatedPath path, GhDuration duration, GhMotion easing, PaintStyle style,
        PointF origin, float scale, float angle, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from validPath in Optional(path).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Stroke)), detail: "path is required"))
            from validOrigin in Op.Of(name: nameof(Stroke)).AcceptPoint(value: origin, detail: "non-finite origin")
            from validScale in Op.Of(name: nameof(Stroke)).AcceptFinite(value: scale, detail: "non-finite scale")
            from validAngle in Op.Of(name: nameof(Stroke)).AcceptFinite(value: angle, detail: "non-finite angle")
            from result in Pipeline(opName: nameof(Stroke), phase: CanvasPaintPhase.AfterObjects, clock: clock,
                paintFactory: (canvas, pacer) => {
                    Animated<double> progress = Animated<double>.CreateUnfinished(
                        value0: 0.0, value1: 1.0,
                        duration: Animators.DurationToTimeSpan(duration: duration),
                        motion: easing,
                        interpolator: static (value0, value1, factor) => value0 + ((value1 - value0) * factor));
                    Animated<double> settled = Animated<double>.CreateUnfinished(
                        value0: 1.0, value1: 1.0, duration: TimeSpan.Zero, motion: easing,
                        interpolator: static (value0, value1, factor) => 1.0);
                    return paintScope => Try.lift(f: () => {
                        using Pen pen = style.Pen();
                        Animated<double> animated = MotionAccessibility.ShouldReduceMotion ? settled : progress;
                        validPath.Draw(paintScope.Graphics.Content, pen, canvas.Animate(animated: animated), validOrigin, validScale, validAngle);
                        return MotionAccessibility.ShouldReduceMotion ? unit : PauseWhenFinished(state: progress.State, pacer: pacer);
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Stroke)), detail: $"draw threw: {error.Message}"));
                }).Run(scope: scope)
            select result);

    internal static GrasshopperUiIntent<Unit> Sparkle(ISparkle instance) =>
        GhUi.Canvas(
            repaint: RepaintRequest.Scheduled,
            run: scope =>
                from canvas in scope.NeedCanvas()
                from valid in Optional(instance).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Sparkle)), detail: "sparkle is required"))
                from _ in MotionAccessibility.ShouldSkipDecorativeMotion
                    ? Fin.Succ(value: unit)
                    : Try.lift(f: () => { canvas.AddSparkle(sparkle: valid); return unit; }).Run()
                        .MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Sparkle)), detail: $"AddSparkle threw: {error.Message}"))
                select unit);

    internal static GrasshopperUiIntent<Subscription> Theme(
        Skin start, Skin target, GhDuration duration, GhMotion easing, Action<Skin> sink, MotionClock clock) =>
        Tween(
            animated: Animated<Skin>.CreateUnfinished(
                value0: start, value1: target,
                duration: Animators.DurationToTimeSpan(duration: duration),
                motion: easing,
                interpolator: static (a, b, t) => a.Interpolate(b, (float)t)),
            sink: sink,
            clock: clock);

    internal static GrasshopperUiIntent<Unit> Navigate(PointF centre, GhDuration duration, float minZoom, float maxZoom) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from document in scope.NeedDocument()
            from validCentre in Op.Of(name: nameof(Navigate)).AcceptPoint(value: centre, detail: "non-finite centre")
            from validMin in Op.Of(name: nameof(Navigate)).AcceptFinite(value: minZoom, detail: "min zoom must be finite and positive", requirePositive: true)
            from validMax in Op.Of(name: nameof(Navigate)).AcceptFinite(value: maxZoom, detail: "max zoom must be finite and positive", requirePositive: true)
            from _ in validMax >= validMin
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Navigate)), detail: "max zoom below min zoom"))
            from __ in MotionAccessibility.ShouldReduceMotion
                ? Op.Of(name: nameof(Navigate)).Attempt(body: () => {
                    float zoom = Math.Clamp(value: document.Projection.zoom, min: validMin, max: validMax);
                    canvas.Projection = canvas.Projection.SetZoom(zoom: zoom).SetCentre(centre: validCentre, frame: canvas.VisibleFrame);
                    document.Projection = (validCentre, zoom);
                    return unit;
                }, what: "navigate reduce-motion snap")
                : Try.lift(f: () => {
                    canvas.Navigate(point: validCentre, zoomLimits: (validMin, validMax), duration: duration);
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Navigate)), detail: $"navigate threw: {error.Message}"))
            select unit);

    internal static GrasshopperUiIntent<Subscription> ZoomGate(ZoomThreshold threshold, Action<float> sink, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ZoomGate)), detail: "sink delegate is required"))
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: paintScope => Try.lift(f: () => {
                    validSink(canvas.AnimatedZoomFactor(threshold: threshold));
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(ZoomGate)), detail: $"tick threw: {error.Message}")),
                clock: clock).Run(scope: scope)
            select sub);

    internal static GrasshopperUiIntent<Subscription> Cosmetic(CosmeticIntent intent) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from view in Optional(canvas.ControlObject as NSView).ToFin(Fail: UiFault.MutationRejected(
                op: Op.Of(name: nameof(Cosmetic)), detail: "canvas.ControlObject is not an NSView"))
            from mapped in PrepareCosmetic(canvas: canvas, intent: intent)
            let key = (NSString)$"rasm.cosmetic.{Guid.NewGuid():N}"
            from sub in Subscription.Bind(
                attach: () => CosmeticAttach(view: view, intent: mapped, key: key),
                detach: () => CosmeticStrip(view: view, key: key),
                marshalToUi: true)
            select sub);

    private static readonly (CAGradientLayerType Type, CGPoint Start, CGPoint End)[] CosmeticGradientProfile = [
        (CAGradientLayerType.Axial, CGPoint.Empty, new CGPoint(x: 1, y: 0)),
        (CAGradientLayerType.Radial, new CGPoint(x: 0.5, y: 0.5), new CGPoint(x: 1, y: 1)),
        (CAGradientLayerType.Conic, new CGPoint(x: 0.5, y: 0.5), new CGPoint(x: 1, y: 0.5)),
    ];

    private static Fin<CosmeticIntent> PrepareCosmetic(Grasshopper2.UI.Canvas.Canvas canvas, CosmeticIntent intent) =>
        from mapped in PrepareCosmeticCore(canvas: canvas, intent: intent)
        from children in mapped.CoAnimate.Match(
            Some: pending => pending.TraverseM(child => PrepareCosmetic(canvas: canvas, intent: child)).As().Map(prepared => Some(FlattenCoAnimations(children: prepared))),
            None: () => Fin.Succ(value: Option<Seq<CosmeticIntent>>.None))
        let prepared = mapped with { CoAnimate = children }
        from __ in AcceptCoAnimationLayer(intent: prepared)
        from _ in AcceptDistinctAnimationKeyPaths(intent: prepared)
        select prepared;

    private static Seq<CosmeticIntent> FlattenCoAnimations(Seq<CosmeticIntent> children) =>
        children.Bind(child => {
            CosmeticIntent current = child with { CoAnimate = Option<Seq<CosmeticIntent>>.None };
            Seq<CosmeticIntent> nested = child.CoAnimate.IfNone(Seq<CosmeticIntent>());
            return Seq(current) + FlattenCoAnimations(children: nested);
        });

    private static Fin<CosmeticIntent> PrepareCosmeticCore(Grasshopper2.UI.Canvas.Canvas canvas, CosmeticIntent intent) =>
        intent.Switch(
            pulseCase: static (c, p) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.PulseCase));
                return op.AcceptRect(value: p.Bounds, detail: "non-finite pulse bounds")
                    .Map<CosmeticIntent>(_ => p with { Bounds = MapCosmeticRect(canvas: c, bounds: p.Bounds) });
            },
            glowCase: static (c, g) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.GlowCase));
                return op.AcceptRect(value: g.Bounds, detail: "non-finite glow bounds")
                    .Bind(_ => op.AcceptFinite(value: g.CornerRadius, detail: "non-finite corner radius"))
                    .Bind(_ => op.AcceptFinite(value: g.ShadowRadius, detail: "non-finite shadow radius"))
                    .Bind(_ => op.AcceptFinite(value: g.BorderWidth, detail: "non-finite border width", nonNegative: true))
                    .Bind(_ => op.AcceptFinite(value: g.BlurRadius, detail: "non-finite blur radius", nonNegative: true))
                    .Map<CosmeticIntent>(_ => g with { Bounds = MapCosmeticRect(canvas: c, bounds: g.Bounds) });
            },
            strokeOnCase: static (c, s) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.StrokeOnCase));
                return op.AcceptFinite(value: s.Thickness, detail: "non-positive thickness", requirePositive: true)
                    .Map<CosmeticIntent>(_ => s with { Polyline = MapCosmeticPolyline(canvas: c, polyline: s.Polyline) });
            },
            gradientCase: static (c, g) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.GradientCase));
                return op.AcceptRect(value: g.Bounds, detail: "non-finite gradient bounds")
                    .Bind(_ => g.Colors.Count >= 2
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: op, detail: $"gradient requires at least two colours (got {g.Colors.Count})")))
                    .Bind(_ => AcceptGradientPoints(op: op, points: g.Points, colourCount: g.Colors.Count))
                    .Map<CosmeticIntent>(_ => g with { Bounds = MapCosmeticRect(canvas: c, bounds: g.Bounds) });
            },
            textLayerCase: static (c, t) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.TextLayerCase));
                return Optional(t.Text)
                    .Filter(static text => text.Length > 0)
                    .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "text is required"))
                    .Bind(_ => op.AcceptFinite(value: t.FontSize, detail: "non-positive font size", requirePositive: true))
                    .Map<CosmeticIntent>(_ => t with { Origin = c.Map(point: t.Origin, from: CoordinateSystem.Content, to: CoordinateSystem.Control) });
            },
            replicatorCase: static (c, r) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.ReplicatorCase));
                return r.Count > 0
                    ? op.AcceptRect(value: r.SourceBounds, detail: "non-finite replicator bounds")
                        .Bind(_ => op.AcceptFinite(value: r.Spacing, detail: "non-finite replicator spacing"))
                        .Map<CosmeticIntent>(_ => r with { SourceBounds = MapCosmeticRect(canvas: c, bounds: r.SourceBounds) })
                    : Fin.Fail<CosmeticIntent>(error: UiFault.InvalidInput(op: op, detail: "replicator count must be positive"));
            },
            emitterCase: static (c, e) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.EmitterCase));
                return op.AcceptRect(value: e.Bounds, detail: "non-finite emitter bounds")
                    .Bind(_ => op.AcceptFinite(value: e.BirthRate, detail: "birth rate must be finite and positive", requirePositive: true))
                    .Bind(_ => op.AcceptFinite(value: e.Lifetime, detail: "lifetime must be finite and positive", requirePositive: true))
                    .Map<CosmeticIntent>(_ => e with { Bounds = MapCosmeticRect(canvas: c, bounds: e.Bounds) });
            },
            snapGuideCase: static (c, sg) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.SnapGuideCase));
                return op.AcceptFinite(value: sg.Style.Thickness, detail: "thickness must be finite and positive", requirePositive: true)
                    .Map<CosmeticIntent>(_ => sg with {
                        Snapshot = sg.Snapshot with {
                            Lines = sg.Snapshot.Lines.Map(line => new LineF(
                                start: c.Map(point: line.Start, from: CoordinateSystem.Content, to: CoordinateSystem.Control),
                                end: c.Map(point: line.End, from: CoordinateSystem.Content, to: CoordinateSystem.Control))),
                            XLabel = sg.Snapshot.XLabel.Map(l => l with { Point = c.Map(point: l.Point, from: CoordinateSystem.Content, to: CoordinateSystem.Control) }),
                            YLabel = sg.Snapshot.YLabel.Map(l => l with { Point = c.Map(point: l.Point, from: CoordinateSystem.Content, to: CoordinateSystem.Control) }),
                        },
                    });
            },
            state: canvas);

    private static Fin<Unit> AcceptDistinctAnimationKeyPaths(CosmeticIntent intent) {
        Seq<string> paths = Seq(AnimationKeyPathOf(intent: intent)) + intent.CoAnimate.IfNone(Seq<CosmeticIntent>()).Map(AnimationKeyPathOf);
        Seq<string> duplicates = toSeq(paths
            .GroupBy(keySelector: static path => path, comparer: StringComparer.Ordinal)
            .Where(static group => group.Skip(1).Any())
            .Select(static group => group.Key));
        return duplicates.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: UiFault.InvalidInput(
                op: Op.Of(name: nameof(CosmeticIntent.CoAnimate)),
                detail: $"cosmetic co-animation key paths must be distinct: {string.Join(separator: ", ", values: duplicates.AsIterable())}"));
    }

    private static Fin<Unit> AcceptCoAnimationLayer(CosmeticIntent intent) {
        string parent = AnimationKeyPathOf(intent: intent);
        Seq<string> invalid = intent.CoAnimate.IfNone(Seq<CosmeticIntent>())
            .Map(child => (Intent: child, Path: AnimationKeyPathOf(intent: child)))
            .Filter(child => string.Equals(a: child.Path, b: "strokeEnd", comparisonType: StringComparison.Ordinal) && !string.Equals(a: parent, b: "strokeEnd", comparisonType: StringComparison.Ordinal))
            .Map(static child => child.Intent.GetType().Name);
        return invalid.IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: UiFault.InvalidInput(
                op: Op.Of(name: nameof(CosmeticIntent.CoAnimate)),
                detail: $"stroke co-animations require a stroke parent layer: {string.Join(separator: ", ", values: invalid.AsIterable())}"));
    }

    private static string AnimationKeyPathOf(CosmeticIntent intent) =>
        intent.Switch(
            pulseCase: static _ => "opacity",
            glowCase: static _ => "shadowOpacity",
            strokeOnCase: static _ => "strokeEnd",
            gradientCase: static _ => "opacity",
            textLayerCase: static _ => "opacity",
            replicatorCase: static _ => "opacity",
            emitterCase: static _ => "opacity",
            snapGuideCase: static _ => "opacity");

    private static Fin<Unit> AcceptGradientPoints(Op op, CosmeticGradientPoints points, int colourCount) =>
        toSeq([
            points.Start.Map(start => AcceptNormalizedGradientPoint(op: op, point: start, label: nameof(CosmeticGradientPoints.Start))),
            points.End.Map(end => AcceptNormalizedGradientPoint(op: op, point: end, label: nameof(CosmeticGradientPoints.End))),
            points.Locations.Map(locations => AcceptGradientStops(op: op, locations: locations, colourCount: colourCount)),
        ])
            .Somes()
            .Fold(
                initialState: Fin.Succ(unit),
                f: static (acc, step) => acc.Bind(_ => step));

    // CAGradientLayer.Locations must align 1:1 with Colors and ascend monotonically through [0,1].
    private static Fin<Unit> AcceptGradientStops(Op op, ReadOnlyMemory<float> locations, int colourCount) {
        float[] stops = locations.Span.ToArray();
        return stops.Length == colourCount
            ? toSeq(stops)
                .Fold(
                    initialState: Fin.Succ(float.NegativeInfinity),
                    f: (acc, stop) => acc.Bind(previous => stop is >= 0f and <= 1f && stop >= previous
                        ? Fin.Succ(stop)
                        : Fin.Fail<float>(error: UiFault.InvalidInput(op: op, detail: $"gradient stops must ascend through [0,1] (got {stop:R} after {previous:R})"))))
                .Map(static _ => unit)
            : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: op, detail: $"gradient stop count {stops.Length} must equal colour count {colourCount}"));
    }

    private static Fin<Unit> AcceptNormalizedGradientPoint(Op op, PointF point, string label) =>
        op.AcceptPoint(value: point, detail: $"non-finite {label}")
            .Bind(valid => valid is { X: >= 0f and <= 1f, Y: >= 0f and <= 1f }
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: op, detail: $"{label} must lie in [0,1] layer space (got {valid.X:R},{valid.Y:R})")));

    private static (CGPoint Start, CGPoint End) ResolveGradientPoints(GradientKind kind, CosmeticGradientPoints points) {
        (_, CGPoint profileStart, CGPoint profileEnd) = CosmeticGradientProfile[kind.ProfileIndex];
        return (
            Start: points.Start.Map(static value => ToCGPoint(value)).IfNone(profileStart),
            End: points.End.Map(static value => ToCGPoint(value)).IfNone(profileEnd));
    }

    // Exact CoreText metrics via NSAttributedString (AppKit/Foundation already imported) — replaces the prior
    // 0.62/1.4 glyph-advance/line-height estimate pairs. Main-thread/AppKit-bound, same as the CALayer path.
    [SupportedOSPlatform("macos14.0")]
    private static (float Width, float Height) MeasureText(string text, float fontSize, Option<string> family) {
        NSFont font = (family is { IsSome: true, Case: string name } ? NSFont.FromFontName(name, fontSize) : null) ?? NSFont.SystemFontOfSize(fontSize)!;
        using NSAttributedString attributed = new(str: text, font: font);
        CGSize size = attributed.Size;
        return (Width: MathF.Max(1f, (float)size.Width), Height: MathF.Max(1f, (float)size.Height));
    }

    private static RectangleF MapCosmeticRect(Grasshopper2.UI.Canvas.Canvas canvas, RectangleF bounds) =>
        canvas.Map(rectangle: bounds, from: CoordinateSystem.Content, to: CoordinateSystem.Control);

    private static ReadOnlyMemory<PointF> MapCosmeticPolyline(Grasshopper2.UI.Canvas.Canvas canvas, ReadOnlyMemory<PointF> polyline) =>
        toSeq(polyline.Span.ToArray())
            .Map(point => canvas.Map(point: point, from: CoordinateSystem.Content, to: CoordinateSystem.Control))
            .ToArray();

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CAShapeLayer ownership transfers to host CALayer via AddSublayer; disposal happens on Subscription detach or animation completion delegate.")]
    private static Unit CosmeticAttach(NSView view, CosmeticIntent intent, NSString key) {
        view.WantsLayer = true;
        if (view.Layer is not CALayer host) {
            return unit;
        }
        if (MotionAccessibility.ShouldSkipDecorativeMotion) {
            return intent.Completion.IfSome(static run => run());
        }
        CALayer layer = BuildCosmeticLayer(intent: intent);
        NFloat scale = Optional(view.Window?.Screen).Map(static screen => screen.BackingScaleFactor).IfNone((NFloat)RetinaScaleDefault);
        layer.ContentsScale = scale;
        _ = (layer.Sublayers is { } sublayers ? toSeq(sublayers) : Seq<CALayer>()).Iter(sub => sub.ContentsScale = scale);
        layer.Name = key.ToString();
        return WithoutAnimation(() => {
            host.AddSublayer(layer: layer);
            CAAnimation animation = GroupOf(main: BuildCosmeticAnimation(intent: intent), intent: intent);
            CosmeticRemove remove = new(layer: layer, key: key, completion: intent.Completion);
            _ = CosmeticDelegateStore.Retain(layer: layer, key: remove.Key, remove: remove);
            animation.WeakDelegate = remove;
            layer.AddAnimation(animation: animation, key: key);
        });
    }

    // Mutual-exclusion claim: TryClaim wins exactly once across explicit-dispose vs AnimationStopped.
    [SupportedOSPlatform("macos14.0")]
    private static Unit CosmeticStrip(NSView view, NSString key) {
        if (view.Layer is not CALayer host || host.Sublayers is not CALayer[] sublayers) {
            return unit;
        }
        string keyString = key.ToString();
        return WithoutAnimation(() => _ = toSeq(sublayers)
            .Filter(sub => string.Equals(a: sub.Name, b: keyString, comparisonType: StringComparison.Ordinal))
            .Filter(sub => CosmeticDelegateStore.Find(layer: sub, key: keyString)
                .Map(remove => remove.TryClaim())
                .IfNone(true))
            .Iter(sub => {
                sub.RemoveAnimation(key: keyString);
                _ = CosmeticDelegateStore.Release(layer: sub, key: keyString);
                sub.RemoveFromSuperLayer();
            }));
    }

    [SupportedOSPlatform("macos14.0")]
    private static CALayer BuildCosmeticLayer(CosmeticIntent intent) =>
        intent.Switch<CALayer>(
            pulseCase: CosmeticPulseLayer,
            glowCase: CosmeticGlowLayer,
            strokeOnCase: CosmeticStrokeLayer,
            gradientCase: CosmeticGradientLayer,
            textLayerCase: CosmeticTextLayer,
            replicatorCase: CosmeticReplicatorLayer,
            emitterCase: CosmeticEmitterLayer,
            snapGuideCase: CosmeticSnapGuideLayer);

    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticPulseLayer(CosmeticIntent.PulseCase pulse) {
        CGRect frame = ToCGRect(pulse.Bounds);
        CGRect local = LocalCGRect(frame: frame);
        CAShapeLayer shape = new() { Frame = frame };
        CGPath path = new();
        path.AddRect(rect: local);
        shape.Path = path;
        shape.FillColor = ToCGColor(pulse.Tint);
        shape.Opacity = 0f;
        return shape;
    }

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CGColor/CGPath/CIFilter ownership transfers to the CAShapeLayer property graph for the animation lifetime.")]
    private static CAShapeLayer CosmeticGlowLayer(CosmeticIntent.GlowCase glow) {
        CGRect rect = ToCGRect(glow.Bounds);
        CGRect local = LocalCGRect(frame: rect);
        CGPath path = new();
        path.AddRoundedRect(transform: CGAffineTransform.MakeIdentity(), rect: local, cornerWidth: glow.CornerRadius, cornerHeight: glow.CornerRadius);
        CAShapeLayer shape = new() {
            Frame = rect,
            Path = path,
            StrokeColor = ToCGColor(glow.Tint),
            LineWidth = 0f,
            CornerRadius = glow.CornerRadius,
            BorderWidth = glow.BorderWidth,
            ShadowColor = ToCGColor(glow.Tint),
            ShadowRadius = glow.ShadowRadius,
            ShadowOffset = CGSize.Empty,
            ShadowPath = path,
            ShadowOpacity = 0f,
        };
        _ = glow.BorderColor.IfSome(colour => shape.BorderColor = ToCGColor(colour));
        _ = Optional(glow.BlurRadius).Filter(static radius => radius > 0f).IfSome(radius => shape.Filters = [new CIGaussianBlur { Radius = radius }]);
        return shape;
    }

    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticStrokeLayer(CosmeticIntent.StrokeOnCase stroke) {
        CGPath path = new();
        // Fold the polyline into the CGPath: first point opens the subpath (MoveTo), the rest extend it (LineTo).
        _ = toSeq(stroke.Polyline.ToArray()).Fold(true, (first, point) => {
            CGPoint cg = ToCGPoint(point);
            _ = first ? Op.Side(() => path.MoveToPoint(point: cg)) : Op.Side(() => path.AddLineToPoint(point: cg));
            return false;
        });
        return new CAShapeLayer {
            Path = path,
            StrokeColor = ToCGColor(stroke.Tint),
            LineWidth = stroke.Thickness,
            StrokeStart = 0f,
            StrokeEnd = 0f,
        };
    }

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CGColor/NSString ownership transfers to CALayer property graph for animation lifetime.")]
    private static CAGradientLayer CosmeticGradientLayer(CosmeticIntent.GradientCase gradient) {
        (CAGradientLayerType type, _, _) = CosmeticGradientProfile[gradient.Kind.ProfileIndex];
        (CGPoint start, CGPoint end) = ResolveGradientPoints(kind: gradient.Kind, points: gradient.Points);
        CAGradientLayer layer = new() {
            Frame = ToCGRect(gradient.Bounds),
            Colors = [.. gradient.Colors.Map(static c => ToCGColor(c))],
            LayerType = type,
            StartPoint = start,
            EndPoint = end,
            Opacity = 0f,
        };
        _ = gradient.Points.Locations.IfSome(locations =>
            layer.Locations = [.. toSeq(locations.Span.ToArray()).Map(static stop => NSNumber.FromFloat(stop))]);
        return layer;
    }

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CGColor/NSString ownership transfers to CALayer property graph for animation lifetime.")]
    private static CATextLayer CosmeticTextLayer(CosmeticIntent.TextLayerCase text) {
        (float width, float height) = MeasureText(text: text.Text, fontSize: text.FontSize, family: text.FontFamily);
        CATextLayer layer = new() {
            String = new NSString(text.Text),
            ForegroundColor = ToCGColor(text.Tint),
            FontSize = text.FontSize,
            Opacity = 0f,
            Frame = new CGRect(x: text.Origin.X, y: text.Origin.Y, width: width, height: height),
        };
        // CATextLayer exposes no managed Font setter; KVC "font" accepts a family name resolved at size FontSize.
        _ = text.FontFamily.IfSome(family => layer.SetValueForKey(value: new NSString(family), key: (NSString)"font"));
        return layer;
    }

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CGPath ownership transfers to CAShapeLayer hosted by CAReplicatorLayer.")]
    private static CAReplicatorLayer CosmeticReplicatorLayer(CosmeticIntent.ReplicatorCase replicate) {
        CGRect rect = ToCGRect(replicate.SourceBounds);
        CGRect local = LocalCGRect(frame: rect);
        CAShapeLayer source = new() { Frame = local };
        CGPath path = new();
        path.AddRect(rect: local);
        source.Path = path;
        source.FillColor = ToCGColor(replicate.Tint);
        // Per-instance transform composes translation (spacing along X) with rotation about Z; either degenerates
        // to identity when its parameter is zero, so the Concat covers spaced, fanned, and spiral replications.
        CAReplicatorLayer replicator = new() {
            Frame = rect,
            InstanceCount = replicate.Count,
            InstanceDelay = replicate.InstanceDelay,
            InstanceAlphaOffset = replicate.InstanceAlphaOffset,
            InstanceTransform = CATransform3D
                .MakeTranslation(tx: replicate.Spacing, ty: 0, tz: 0)
                .Concat(b: CATransform3D.MakeRotation(angle: replicate.Rotation, x: 0, y: 0, z: 1)),
            Opacity = 0f,
        };
        _ = replicate.InstanceColour.IfSome(colour => replicator.InstanceColor = ToCGColor(colour));
        replicator.AddSublayer(layer: source);
        return replicator;
    }

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CGColor/CAEmitterCell ownership transfers to the host CAEmitterLayer property graph for animation lifetime.")]
    private static CAEmitterLayer CosmeticEmitterLayer(CosmeticIntent.EmitterCase emit) {
        CGRect rect = ToCGRect(emit.Bounds);
        CAEmitterCell cell = new() {
            BirthRate = emit.BirthRate,
            LifeTime = emit.Lifetime,
            LifetimeRange = emit.Lifetime * emit.LifetimeRangeRatio,
            Velocity = emit.Velocity,
            VelocityRange = emit.Velocity * emit.VelocityRangeRatio,
            EmissionRange = emit.EmissionRange,
            Scale = emit.Scale,
            ScaleRange = emit.Scale * emit.ScaleRangeRatio,
            ScaleSpeed = emit.ScaleSpeed,
            AlphaSpeed = emit.AlphaSpeed,
            Color = ToCGColor(emit.Tint),
            RedRange = emit.ColorRange,
            GreenRange = emit.ColorRange,
            BlueRange = emit.ColorRange,
        };
        CAEmitterLayer layer = new() {
            Frame = rect,
            Shape = emit.Shape.Resolve(),
            Mode = CAEmitterLayer.ModeSurface,
            Size = new CGSize(width: rect.Width, height: rect.Height),
            Cells = [cell],
            Opacity = 0f,
        };
        // emitterPosition has no managed setter; centre it in the layer via KVC so particles spawn across Bounds.
        layer.SetValueForKey(value: NSValue.FromCGPoint(new CGPoint(x: rect.Width / 2.0, y: rect.Height / 2.0)), key: (NSString)"emitterPosition");
        return layer;
    }

    // Bridges Layout.Snap output to a fire-and-forget overlay: dashed guide lines as a CAShapeLayer plus an
    // optional distance label sublayer at LabelPoint (lines are already mapped to control space upstream).
    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CGPath/CGColor/NSNumber ownership transfers to the CAShapeLayer (and its text sublayer) for the animation lifetime.")]
    private static CAShapeLayer CosmeticSnapGuideLayer(CosmeticIntent.SnapGuideCase guide) {
        SnapGuideStyle style = guide.Style;
        CGPath path = new();
        _ = guide.Snapshot.Lines.Iter(line => {
            path.MoveToPoint(point: ToCGPoint(line.Start));
            path.AddLineToPoint(point: ToCGPoint(line.End));
        });
        CAShapeLayer shape = new() {
            Path = path,
            StrokeColor = ToCGColor(style.Tint),
            FillColor = null,
            LineWidth = style.Thickness,
            Opacity = 0f,
        };
        // Empty Dashes => solid line (matches GH2's native solid feedback + SnappingGecko 'Solid'); a non-empty
        // pattern dashes the guide. Hoisting the dash to the style is the downstream-tunable line-pattern axis.
        _ = style.Dashes.Length > 0
            ? Op.Side(() => shape.LineDashPattern = [.. toSeq(style.Dashes.ToArray()).Map(static d => NSNumber.FromFloat(d))])
            : unit;
        // GH2 keeps X/Y as two independent labelled actions; render one label sublayer per present channel,
        // each at its own LabelPoint. The carried Anchor (a 3x3 compass) drives both the text AlignmentMode and
        // the frame-origin shift so the anchor corner sits at the channel's point. Each label falls back to the
        // snap magnitude when its text is blank. Sublayer opacity stays 1 so the parent's opacity drives the fade.
        _ = Seq(guide.Snapshot.XLabel, guide.Snapshot.YLabel).Somes().Iter(label => {
            string text = label.Text.Length > 0
                ? label.Text
                : guide.Snapshot.Magnitude.ToString(format: style.MagnitudeFormat, provider: CultureInfo.InvariantCulture);
            (float width, float height) = MeasureText(text: text, fontSize: style.LabelFontSize, family: Option<string>.None);
            (CATextLayerAlignmentMode mode, float dx, float dy) = SnapLabelPlacement(anchor: label.Anchor, width: width, height: height);
            shape.AddSublayer(layer: new CATextLayer {
                String = new NSString(text),
                ForegroundColor = ToCGColor(style.Tint),
                FontSize = style.LabelFontSize,
                TextAlignmentMode = mode,
                Frame = new CGRect(x: label.Point.X + dx, y: label.Point.Y + dy, width: width, height: height),
            });
        });
        return shape;
    }

    // TextAnchor (3x3 compass) -> (AlignmentMode, frame-origin shift) so the label's anchor corner lands on
    // LabelPoint. Horizontal {Left|Middle|Right} picks the alignment + x-shift {0,-w/2,-w}; vertical
    // {Upper|Centre|Lower} shifts y {0,-h/2,-h} (control space, y grows down). No anchor => top-left at origin.
    private static (CATextLayerAlignmentMode Mode, float Dx, float Dy) SnapLabelPlacement(Option<GhTextAnchor> anchor, float width, float height) =>
        anchor is { IsSome: true, Case: GhTextAnchor value }
            ? value switch {
                GhTextAnchor.UpperLeft => (CATextLayerAlignmentMode.Left, 0f, 0f),
                GhTextAnchor.CentreLeft => (CATextLayerAlignmentMode.Left, 0f, -height / 2f),
                GhTextAnchor.LowerLeft => (CATextLayerAlignmentMode.Left, 0f, -height),
                GhTextAnchor.UpperMiddle => (CATextLayerAlignmentMode.Center, -width / 2f, 0f),
                GhTextAnchor.CentreMiddle => (CATextLayerAlignmentMode.Center, -width / 2f, -height / 2f),
                GhTextAnchor.LowerMiddle => (CATextLayerAlignmentMode.Center, -width / 2f, -height),
                GhTextAnchor.UpperRight => (CATextLayerAlignmentMode.Right, -width, 0f),
                GhTextAnchor.CentreRight => (CATextLayerAlignmentMode.Right, -width, -height / 2f),
                GhTextAnchor.LowerRight => (CATextLayerAlignmentMode.Right, -width, -height),
                _ => (CATextLayerAlignmentMode.Left, 0f, 0f),
            }
            : (CATextLayerAlignmentMode.Left, 0f, 0f);

    // Reduce-motion is short-circuited upstream in CosmeticAttach (ShouldSkipDecorativeMotion). The default
    // fade/stroke eases via the five Core Animation media-timing curves (TimingName) with the in/out fade and
    // repetition from ApplyRepeat's AutoReverses (yoyo) + RepeatCount. A CosmeticIntent.Keyframe upgrades the
    // lifecycle to a CAKeyFrameAnimation sampling the full 46-curve Easing over [0,1] — additive, not default.
    // Only the five plain curves map cleanly onto a CAMediaTimingFunction; the rich curve set (Bounce/Twang/
    // Snap*/Delayed) auto-routes to a sampled keyframe animation that evaluates the exact GhMotion curve
    // (MotionEquations.Blend) rather than folding to ease-in-ease-out. A manual Keyframe overrides the easing.
    [SupportedOSPlatform("macos14.0")]
    private static CAPropertyAnimation PropertyAnimation(string path, GhDuration duration, float from, float to, GhMotion easing, Option<Easing> keyframe, Option<SpringConfig> spring) =>
        spring is { IsSome: true, Case: SpringConfig config }
            ? SpringAnimation(path: path, from: from, to: to, config: config)
            : keyframe is { IsSome: true, Case: Easing curve }
                ? KeyframeAnimation(path: path, duration: duration, from: from, to: to, easing: curve)
                : BasicTiming(easing: easing)
                    ? BasicAnimation(path: path, duration: duration, from: from, to: to, easing: easing)
                    : KeyframeFromMotion(path: path, duration: duration, from: from, to: to, motion: easing);

    [SupportedOSPlatform("macos14.0")]
    private static CASpringAnimation SpringAnimation(string path, float from, float to, SpringConfig config) {
        CASpringAnimation anim = new() {
            KeyPath = path,
            Mass = config.Mass,
            Stiffness = config.Stiffness,
            Damping = config.Damping,
        };
        anim.Duration = anim.SettlingDuration;
        anim.SetFrom(value: NSNumber.FromFloat(from));
        anim.SetTo(value: NSNumber.FromFloat(to));
        return anim;
    }

    private static bool BasicTiming(GhMotion easing) =>
        easing is GhMotion.Linear or GhMotion.EaseIn or GhMotion.EaseOut or GhMotion.EaseInOut;

    [SupportedOSPlatform("macos14.0")]
    private static CAKeyFrameAnimation KeyframeFromMotion(string path, GhDuration duration, float from, float to, GhMotion motion) {
        CAKeyFrameAnimation anim = CAKeyFrameAnimation.GetFromKeyPath(path: path);
        anim.Duration = Animators.DurationToTimeSpan(duration: duration).TotalSeconds;
        anim.CalculationMode = CAAnimation.AnimationLinear;
        anim.Values = [.. Enumerable.Range(start: 0, count: KeyframeSamples + 1).Select(frame => {
            double t = (double)frame / KeyframeSamples;
            return (NSObject)NSNumber.FromFloat(from + ((to - from) * (float)MotionEquations.Blend(motion: motion, parameter: t)));
        })];
        return anim;
    }

    [SupportedOSPlatform("macos14.0")]
    private static CABasicAnimation BasicAnimation(string path, GhDuration duration, float from, float to, GhMotion easing) {
        CABasicAnimation anim = CABasicAnimation.FromKeyPath(path: path);
        anim.Duration = Animators.DurationToTimeSpan(duration: duration).TotalSeconds;
        anim.SetFrom(value: NSNumber.FromFloat(from));
        anim.SetTo(value: NSNumber.FromFloat(to));
        anim.TimingFunction = CAMediaTimingFunction.FromName(name: TimingName(easing: easing));
        return anim;
    }

    private const int KeyframeSamples = 60;

    // Penner closed-form sampled at KeyframeSamples steps, linearly interpolated between samples; KeyTimes are
    // left unset so Core Animation spreads the values uniformly across [0,1] — the curve shape is the Values.
    [SupportedOSPlatform("macos14.0")]
    private static CAKeyFrameAnimation KeyframeAnimation(string path, GhDuration duration, float from, float to, Easing easing) {
        CAKeyFrameAnimation anim = CAKeyFrameAnimation.GetFromKeyPath(path: path);
        anim.Duration = Animators.DurationToTimeSpan(duration: duration).TotalSeconds;
        anim.CalculationMode = CAAnimation.AnimationLinear;
        anim.Values = [.. Enumerable.Range(start: 0, count: KeyframeSamples + 1).Select(frame => {
            double t = (double)frame / KeyframeSamples;
            return (NSObject)NSNumber.FromFloat(from + ((to - from) * (float)easing.Apply(t: t)));
        })];
        return anim;
    }

    // GH2's rich easing vocabulary collapses onto the five media-timing curves; the GPU layer cannot host
    // Penner closed-forms, so non-linear in/out variants fold to ease-in-ease-out (Keyframe escapes this).
    private static NSString TimingName(GhMotion easing) =>
        easing switch {
            GhMotion.Linear or GhMotion.LinearDelayed => CAMediaTimingFunction.Linear,
            GhMotion.EaseIn or GhMotion.EaseInDelayed or GhMotion.SnapIn or GhMotion.SnapInDelayed => CAMediaTimingFunction.EaseIn,
            GhMotion.EaseOut or GhMotion.EaseOutDelayed or GhMotion.SnapOut or GhMotion.SnapOutDelayed => CAMediaTimingFunction.EaseOut,
            _ => CAMediaTimingFunction.EaseInEaseOut,
        };

    // RepeatCount is float on CAMediaTiming; infinite => PositiveInfinity. AutoReverses gives the
    // fade-out half of each cycle (default yoyo=true preserves the prior 0->alpha->0 pulse shape).
    [SupportedOSPlatform("macos14.0")]
    private static T ApplyRepeat<T>(T animation, int cycles, bool yoyo, bool infinite) where T : CAAnimation {
        animation.RepeatCount = infinite ? float.PositiveInfinity : Math.Max(val1: 1, val2: cycles);
        animation.AutoReverses = yoyo;
        return animation;
    }

    // Wrap the main animation + each CoAnimate child's animation in a CAAnimationGroup on the same layer; the
    // group's Duration is the longest child (CA clips to it) and one CosmeticRemove (attached upstream to the
    // group) owns teardown, so RemovedOnCompletion stays false to avoid a truncated-spring flash.
    [SupportedOSPlatform("macos14.0")]
    private static CAAnimation GroupOf(CAAnimation main, CosmeticIntent intent) =>
        intent.CoAnimate is { IsSome: true, Case: Seq<CosmeticIntent> children } && !children.IsEmpty
            ? GroupAnimations(main: main, children: children)
            : main;

    [SupportedOSPlatform("macos14.0")]
    private static CAAnimationGroup GroupAnimations(CAAnimation main, Seq<CosmeticIntent> children) {
        Seq<CAAnimation> all = Seq(main) + children.Map(static child => BuildCosmeticAnimation(intent: child));
        bool infinite = all.Exists(static animation => animation.RepeatCount == float.PositiveInfinity);
        return new CAAnimationGroup {
            Animations = [.. all],
            Duration = all.Fold(0.0, (m, a) => Math.Max(m, infinite ? a.Duration : EffectiveDuration(animation: a))),
            RepeatCount = infinite ? float.PositiveInfinity : 0f,
            RemovedOnCompletion = false,
        };
    }

    [SupportedOSPlatform("macos14.0")]
    private static double EffectiveDuration(CAAnimation animation) =>
        animation.RepeatCount == float.PositiveInfinity
            ? double.PositiveInfinity
            : animation.Duration * Math.Max(val1: 1d, val2: animation.RepeatCount <= 0f ? 1d : animation.RepeatCount) * (animation.AutoReverses ? 2d : 1d);

    [SupportedOSPlatform("macos14.0")]
    private static CAAnimation BuildCosmeticAnimation(CosmeticIntent intent) =>
        intent.Switch<CAAnimation>(
            pulseCase: static p => ApplyRepeat(PropertyAnimation(path: "opacity", duration: p.Duration, from: 0f, to: p.Tint.A, easing: p.Easing, keyframe: p.Keyframe, spring: p.Spring), cycles: p.Cycles, yoyo: p.Yoyo, infinite: p.Infinite),
            glowCase: static g => ApplyRepeat(PropertyAnimation(path: "shadowOpacity", duration: g.Duration, from: 0f, to: g.Tint.A, easing: g.Easing, keyframe: g.Keyframe, spring: g.Spring), cycles: g.Cycles, yoyo: g.Yoyo, infinite: g.Infinite),
            strokeOnCase: static s => PropertyAnimation(path: "strokeEnd", duration: s.Duration, from: 0f, to: 1f, easing: s.Easing, keyframe: s.Keyframe, spring: s.Spring),
            gradientCase: static g => ApplyRepeat(PropertyAnimation(path: "opacity", duration: g.Duration, from: 0f, to: g.Colors.Map(static c => c.A).Fold(0f, static (m, a) => Math.Max(m, a)), easing: g.Easing, keyframe: g.Keyframe, spring: g.Spring), cycles: g.Cycles, yoyo: g.Yoyo, infinite: g.Infinite),
            textLayerCase: static t => ApplyRepeat(PropertyAnimation(path: "opacity", duration: t.Duration, from: 0f, to: t.Tint.A, easing: t.Easing, keyframe: t.Keyframe, spring: t.Spring), cycles: t.Cycles, yoyo: t.Yoyo, infinite: t.Infinite),
            replicatorCase: static r => ApplyRepeat(PropertyAnimation(path: "opacity", duration: r.Duration, from: 0f, to: r.Tint.A, easing: r.Easing, keyframe: r.Keyframe, spring: r.Spring), cycles: 1, yoyo: true, infinite: false),
            emitterCase: static e => ApplyRepeat(PropertyAnimation(path: "opacity", duration: e.Duration, from: 0f, to: e.Tint.A, easing: e.Easing, keyframe: e.Keyframe, spring: e.Spring), cycles: 1, yoyo: true, infinite: false),
            snapGuideCase: static g => ApplyRepeat(PropertyAnimation(path: "opacity", duration: g.Style.Duration, from: 0f, to: g.Style.Tint.A, easing: g.Style.Easing, keyframe: g.Keyframe, spring: g.Spring), cycles: 1, yoyo: true, infinite: false));

    private static CGRect ToCGRect(RectangleF r) => new(x: r.X, y: r.Y, width: r.Width, height: r.Height);

    private static CGRect LocalCGRect(CGRect frame) => new(x: 0, y: 0, width: frame.Width, height: frame.Height);

    private static CGPoint ToCGPoint(PointF p) => new(x: p.X, y: p.Y);

    private static CGColor ToCGColor(Color c) => new(red: c.R, green: c.G, blue: c.B, alpha: c.A);

    // Implicit CATransaction with actions disabled; Dispose commits. Lets WithoutAnimation be a using-scope
    // rather than an explicit try/finally.
    private readonly struct CATransactionScope : IDisposable {
        internal static CATransactionScope Begin() {
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            return default;
        }
        public void Dispose() => CATransaction.Commit();
    }

    private static Unit WithoutAnimation(Action body) {
        using CATransactionScope _ = CATransactionScope.Begin();
        return Op.Side(body);
    }

    // Exchange-on-stripped is symmetric with CosmeticStrip.TryClaim — whichever path wins owns the strip.
    [SupportedOSPlatform("macos14.0")]
    private sealed class CosmeticRemove : CAAnimationDelegate {
        private readonly CALayer layer;
        private readonly Option<Func<Unit>> completion;
        private int stripped;

        internal CosmeticRemove(CALayer layer, NSString key, Option<Func<Unit>> completion) {
            this.layer = layer;
            Key = key.ToString();
            this.completion = completion;
        }

        internal string Key { get; }

        public override void AnimationStopped(CAAnimation animation, bool finished) {
            if (Interlocked.Exchange(ref stripped, 1) == 1) {
                return;
            }
            _ = WithoutAnimation(() => {
                layer.RemoveAnimation(key: Key);
                _ = CosmeticDelegateStore.Release(layer: layer, key: Key);
                layer.RemoveFromSuperLayer();
            });
            _ = completion.IfSome(run => run());
        }

        internal bool TryClaim() => Interlocked.Exchange(ref stripped, 1) == 0;
    }

    private static class CosmeticDelegateStore {
        private static readonly Atom<HashMap<(int Layer, string Key), CosmeticRemove>> Delegates = Atom(value: HashMap<(int Layer, string Key), CosmeticRemove>());

        internal static Option<CosmeticRemove> Find(CALayer layer, string key) =>
            Delegates.Value.Find(key: StoreKey(layer: layer, key: key));

        internal static Unit Retain(CALayer layer, string key, CosmeticRemove remove) {
            _ = Delegates.Swap(map => map.SetItem(StoreKey(layer: layer, key: key), remove));
            return unit;
        }

        internal static Unit Release(CALayer layer, string key) {
            _ = Delegates.Swap(map => map.Remove(key: StoreKey(layer: layer, key: key)));
            return unit;
        }

        private static (int Layer, string Key) StoreKey(CALayer layer, string key) =>
            (Layer: RuntimeHelpers.GetHashCode(layer), Key: key);
    }

    internal sealed class MotionRunner<TState> where TState : struct, IMotionState<TState> {
        private readonly Atom<TState> cell;
        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly Option<Pacer> pacer;
        private readonly Func<TState, TState> stepCell;
        private float pendingDt;
        private int wantingTicks;
        private double lastVsyncTimestamp;

        private MotionRunner(Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer) {
            this.cell = cell;
            this.canvas = canvas;
            this.pacer = pacer;
            // Re-reads mutable pendingDt each CAS attempt so a concurrent Retarget re-integrates against the
            // latest committed Target; a method-group form would freeze the receiver (IDE0200) and reallocate.
            stepCell = live => live.Step(frameDeltaSeconds: pendingDt);
        }

        internal static MotionRunner<TState> Of(Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer = default) =>
            new(cell: cell, canvas: canvas, pacer: pacer);

        // Step folded inside the CAS body; Emit/Wake act on the committed value Swap returns. Teardown is
        // Subscription-owned (the pump self-detaches on the rested edge), so no per-tick cancellation guard.
        internal Unit Tick() {
            pendingDt = ResolveFrameDelta();
            TState committed = cell.Swap(stepCell);
            _ = committed.Emit();
            _ = committed.IsActive ? Wake() : Sleep();
            return unit;
        }

        private float ResolveFrameDelta() {
            const float fallback = 1f / 60f;
            if (pacer is not { IsSome: true, Case: Pacer paced }) {
                return 0f;
            }
            double timestamp = paced.LastTargetTimestamp;
            if (!double.IsFinite(timestamp) || timestamp <= 0d) {
                return fallback;
            }
            double delta = lastVsyncTimestamp > 0d ? timestamp - lastVsyncTimestamp : fallback;
            lastVsyncTimestamp = timestamp;
            return double.IsFinite(delta) && delta > 0d && delta < 1d ? (float)delta : fallback;
        }

        // Pacer Resume only on 0→1 edge; ScheduleRedraw fallback relies on Eto coalescing.
        private Unit Wake() {
            if (pacer is { IsSome: true, Case: Pacer p }) {
                if (Interlocked.Exchange(ref wantingTicks, 1) == 0) {
                    lastVsyncTimestamp = 0d;
                    return p.Resume();
                }
                return unit;
            }
            canvas.ScheduleRedraw();
            return unit;
        }

        private Unit Sleep() =>
            pacer is { IsSome: true, Case: Pacer p } && Interlocked.Exchange(ref wantingTicks, 0) == 1
                ? p.Pause()
                : unit;
    }

    // Per-canvas vsync via NSView.GetDisplayLink (WWDC23). Pool keys are weak so dead canvases evict
    // naturally; the static screen-change observer holds zero Pacer refs and walks the live pool.
    [SupportedOSPlatform("macos14.0")]
    internal sealed class Pacer : NSObject {
        private static readonly Selector TickSelector = new("tick:");
#pragma warning disable IDE0028 // ConditionalWeakTable has no collection-expression target.
        private static readonly ConditionalWeakTable<Grasshopper2.UI.Canvas.Canvas, Pacer> Pool = new();
#pragma warning restore IDE0028
        private static readonly Lock PoolGate = new();
        private static readonly Lock ObserverGate = new();
        private static IDisposable? screenChangeObserver;

        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly NSView view;
        private readonly CAFrameRateRange? explicitRate;
        private CADisplayLink link;
        private double lastFrameTimestamp;
        private double lastTargetTimestamp;
        private int refCount;
        private int active;
        private int disposed;

        private Pacer(Grasshopper2.UI.Canvas.Canvas canvas, NSView view, CAFrameRateRange? explicitRate) {
            this.canvas = canvas;
            this.view = view;
            this.explicitRate = explicitRate;
            link = BindLink(view: view, range: ResolveRange(view: view, explicitRate: explicitRate));
            EnsureScreenChangeObserver();
        }

        // AppDomain-lifetime observer (RhinoWIP never tears down plugins). No Pacer refs held.
        private static void EnsureScreenChangeObserver() {
            using (ObserverGate.EnterScope()) {
                if (screenChangeObserver is not null) {
                    return;
                }
                _ = NotificationSubscription.Of(
                    (NSApplication.DidChangeScreenParametersNotification, static _ => OnScreenParametersChanged(), NSOperationQueue.MainQueue))
                    .IfSucc(static observer => screenChangeObserver = observer);
            }
        }

        private static void OnScreenParametersChanged() {
            using (PoolGate.EnterScope()) {
                _ = toSeq(Pool).Iter(static entry => entry.Value.RebindLink());
            }
        }

        private CADisplayLink BindLink(NSView view, CAFrameRateRange range) {
            CADisplayLink bound = view.GetDisplayLink(target: this, selector: TickSelector);
            bound.PreferredFrameRateRange = range;
            bound.Paused = true;
            bound.AddToRunLoop(runloop: NSRunLoop.Main, mode: NSRunLoopMode.Common);
            return bound;
        }

        // NSScreen.MaximumFramesPerSecond honours the actual panel ceiling (60/120/144Hz vary).
        private const float FallbackRefreshHz = 120f;
        private const float FloorRefreshHz = 30f;

        private static CAFrameRateRange ResolveRange(NSView view, CAFrameRateRange? explicitRate) {
            if (explicitRate is CAFrameRateRange supplied) {
                return supplied;
            }
            NSScreen? screen = view.Window?.Screen;
            float maximum = screen is NSScreen active ? active.MaximumFramesPerSecond : FallbackRefreshHz;
            float floor = MathF.Min(x: FloorRefreshHz, y: maximum);
            return CAFrameRateRange.Create(minimum: floor, maximum: maximum, preferred: maximum);
        }

        // CAS-style adopt: construct outside the gate, then re-check inside to win-or-adopt.
        internal static Fin<Pacer> For(Grasshopper2.UI.Canvas.Canvas canvas, CAFrameRateRange? rate = null) {
            using (PoolGate.EnterScope()) {
                if (Pool.TryGetValue(canvas, out Pacer? existing)) {
                    _ = Interlocked.Increment(ref existing.refCount);
                    return Fin.Succ(existing);
                }
            }
            return Optional(canvas.ControlObject as NSView)
                .ToFin(Fail: UiFault.MutationRejected(
                    op: Op.Of(name: nameof(Pacer)),
                    detail: "canvas.ControlObject is not an NSView"))
                .Bind(view => Op.Of(name: nameof(Pacer)).Attempt(
                    body: () => CreateOrAdopt(canvas: canvas, view: view, rate: rate),
                    what: "Pacer construction"));
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The fresh Pacer either transfers ownership to the static Pool (lifecycle-managed via refCount/Release) or invalidates+disposes its link explicitly when it loses the race.")]
        private static Pacer CreateOrAdopt(Grasshopper2.UI.Canvas.Canvas canvas, NSView view, CAFrameRateRange? rate) {
            Pacer fresh = new(canvas: canvas, view: view, explicitRate: rate);
            using (PoolGate.EnterScope()) {
                if (Pool.TryGetValue(canvas, out Pacer? adopted)) {
                    fresh.TearDownLink();
                    _ = Interlocked.Increment(ref adopted.refCount);
                    return adopted;
                }
                Pool.Add(canvas, fresh);
                _ = Interlocked.Increment(ref fresh.refCount);
                return fresh;
            }
        }

        // Exchange the live link FIRST so Resume/Pause race onto the replacement, then set its pause state
        // from a re-read of active, then invalidate the previous. The view-vended link is not owned here,
        // so Invalidate is the whole teardown — an explicit Dispose would double-free the run-loop handle.
        private void RebindLink() {
            CADisplayLink replacement = BindLink(view: view, range: ResolveRange(view: view, explicitRate: explicitRate));
            CADisplayLink previous = Interlocked.Exchange(ref link, replacement);
            Volatile.Write(ref lastFrameTimestamp, 0d);
            Volatile.Write(ref lastTargetTimestamp, 0d);
            replacement.Paused = active <= 0;
            previous.Invalidate();
        }

        // sender.Timestamp = just-displayed host clock; sender.TargetTimestamp = the host clock for the frame
        // about to be displayed. Integrating dt across the TARGET pair (present-to-present) removes one frame
        // of latency on ProMotion. Volatile fences AArch64 reordering; 64-bit double is atomic on Apple Silicon.
        [Export("tick:")]
        public void Tick(CADisplayLink sender) {
            Volatile.Write(ref lastFrameTimestamp, sender.Timestamp);
            Volatile.Write(ref lastTargetTimestamp, sender.TargetTimestamp);
            canvas.ScheduleRedraw();
        }

        internal double LastFrameTimestamp => Volatile.Read(ref lastFrameTimestamp);
        internal double LastTargetTimestamp => Volatile.Read(ref lastTargetTimestamp);

        internal Unit Resume() {
            if (Interlocked.Increment(ref active) == 1) {
                link.Paused = false;
            }
            return unit;
        }

        internal Unit Pause() {
            int next = Interlocked.Decrement(ref active);
            if (next < 0) {
                _ = Interlocked.CompareExchange(ref active, 0, next);
                next = 0;
            }
            if (next <= 0) {
                link.Paused = true;
            }
            return unit;
        }

        // PoolGate prevents adoption mid-teardown.
        internal Unit Release() {
            using (PoolGate.EnterScope()) {
                if (Interlocked.Decrement(ref refCount) > 0) {
                    return unit;
                }
                _ = Pool.Remove(canvas);
            }
            Dispose();
            return unit;
        }

        private void TearDownLink() {
            link.Paused = true;
            link.Invalidate();
            link.Dispose();
        }

        // Defensive Pool.Remove catches the finalizer path where Release was never invoked.
        protected override void Dispose(bool disposing) {
            if (Interlocked.Exchange(ref disposed, 1) == 1) {
                base.Dispose(disposing);
                return;
            }
            if (disposing) {
                using (PoolGate.EnterScope()) {
                    _ = Pool.Remove(canvas);
                }
                TearDownLink();
            }
            base.Dispose(disposing);
        }
    }

    // NSNotificationCenter retains the observer until `removeObserver:` lands; Dispose alone leaks it.
    [SupportedOSPlatform("macos14.0")]
    internal sealed class NotificationSubscription : IDisposable {
        private readonly Seq<Action> detachers;
        private int disposed;

        private NotificationSubscription(Seq<Action> detachers) => this.detachers = detachers;

        internal static Fin<IDisposable> Of(params (NSString Name, Action<NSNotification> Handler, NSOperationQueue? Queue)[] subscriptions) =>
            Op.Of(name: nameof(NotificationSubscription)).Attempt(
                body: () => (IDisposable)new NotificationSubscription(
                    detachers: toSeq(subscriptions).Map(Attach).Strict()),
                what: "observer attach");

        private static Action Attach((NSString Name, Action<NSNotification> Handler, NSOperationQueue? Queue) sub) {
            NSObject observer = NSNotificationCenter.DefaultCenter.AddObserver(
                name: sub.Name.ToString(),
                obj: null,
                queue: sub.Queue ?? NSOperationQueue.MainQueue,
                handler: sub.Handler);
            return () => {
                NSNotificationCenter.DefaultCenter.RemoveObserver(observer: observer);
                observer.Dispose();
            };
        }

        public void Dispose() {
            if (Interlocked.Exchange(ref disposed, 1) == 1) {
                return;
            }
            _ = detachers.Iter(detach => detach());
        }
    }
}
