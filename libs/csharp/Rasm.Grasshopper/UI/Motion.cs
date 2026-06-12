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

// NSHapticFeedbackManager patterns (AppKit). ToNative projects to the native enum at the boundary so the
// case carries a Rasm-stable vocabulary rather than the Foundation type.
[SmartEnum<int>]
public sealed partial class HapticPattern {
    internal NSHapticFeedbackPattern Native { get; }
    // Pattern owns timing: alignment syncs to draw completion, level changes fire immediately, generic uses Default.
    internal NSHapticFeedbackPerformanceTime Time { get; }

    public static readonly HapticPattern Alignment = new(key: 0, native: NSHapticFeedbackPattern.Alignment, time: NSHapticFeedbackPerformanceTime.DrawCompleted);
    public static readonly HapticPattern Level = new(key: 1, native: NSHapticFeedbackPattern.LevelChange, time: NSHapticFeedbackPerformanceTime.Now);
    public static readonly HapticPattern Generic = new(key: 2, native: NSHapticFeedbackPattern.Generic, time: NSHapticFeedbackPerformanceTime.Default);

    internal NSHapticFeedbackPattern ToNative() => Native;
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

// CAEmitterCell archetype: profile owns derived particle dynamics; EmitterCase owns bounds/tint/timing/rates.
// RenderMode resolves lazily so type-load never touches CoreAnimation off the gated GPU path.
[SmartEnum<int>]
public sealed partial class EmitterRenderMode {
    public static readonly EmitterRenderMode Unordered = new(key: 0, resolve: static () => CAEmitterLayer.RenderUnordered);
    public static readonly EmitterRenderMode Additive = new(key: 1, resolve: static () => CAEmitterLayer.RenderAdditive);
    public static readonly EmitterRenderMode BackToFront = new(key: 2, resolve: static () => CAEmitterLayer.RenderBackToFront);

    [UseDelegateFromConstructor]
    internal partial NSString Resolve();
}

[SmartEnum<int>]
public sealed partial class EmitterProfile {
    public float Scale { get; }
    public float ScaleSpeed { get; }
    public float AlphaSpeed { get; }
    public float ColorRange { get; }
    public float LifetimeRangeRatio { get; }
    public float VelocityRangeRatio { get; }
    public float ScaleRangeRatio { get; }
    // SpinRange owns absolute spin spread (rad/s variance); AccelerationX/Y/Z are per-cell gravity (px/s², +Y down in CA layer space).
    // Render is compositing mode; Additive blooms overlapping sparks toward white.
    public float SpinRange { get; }
    public float AccelerationX { get; }
    public float AccelerationY { get; }
    public float AccelerationZ { get; }
    public EmitterRenderMode Render { get; }

    // SpinRange is absolute rad/s variance; per-profile values approximate the prior |Spin|*ratio defaults (Spark 4.8, Smoke 0.4, Confetti 2.4) and should be recalibrated against captured emitter spin spread.
    public static readonly EmitterProfile Spark = new(key: 0, scale: 0.2f, scaleSpeed: -0.12f, alphaSpeed: -0.7f, colorRange: 0.1f, lifetimeRangeRatio: 0.3f, velocityRangeRatio: 0.5f, scaleRangeRatio: 0.5f, spinRange: 4.8f, accelerationX: 0f, accelerationY: 60f, accelerationZ: 0f, render: EmitterRenderMode.Additive);
    public static readonly EmitterProfile Smoke = new(key: 1, scale: 0.5f, scaleSpeed: 0.4f, alphaSpeed: -0.5f, colorRange: 0.05f, lifetimeRangeRatio: 0.5f, velocityRangeRatio: 0.3f, scaleRangeRatio: 0.4f, spinRange: 0.4f, accelerationX: 0f, accelerationY: -20f, accelerationZ: 0f, render: EmitterRenderMode.Unordered);
    public static readonly EmitterProfile Confetti = new(key: 2, scale: 0.3f, scaleSpeed: -0.05f, alphaSpeed: -0.4f, colorRange: 0.4f, lifetimeRangeRatio: 0.4f, velocityRangeRatio: 0.7f, scaleRangeRatio: 0.6f, spinRange: 2.4f, accelerationX: 0f, accelerationY: 120f, accelerationZ: 0f, render: EmitterRenderMode.BackToFront);
}

// CALayer.CompositingFilter blend, resolved via CIFilter.FromName. FilterName is the Core Image filter the
// glow layer composites with against the canvas behind it (screen = additive lighten, multiply = darken, etc.).
[SmartEnum<int>]
public sealed partial class BlendMode {
    public string FilterName { get; }

    public static readonly BlendMode Screen = new(key: 0, filterName: "CIScreenBlendMode");
    public static readonly BlendMode Multiply = new(key: 1, filterName: "CIMultiplyBlendMode");
    public static readonly BlendMode Addition = new(key: 2, filterName: "CIAdditionCompositing");
    public static readonly BlendMode Overlay = new(key: 3, filterName: "CIOverlayBlendMode");
    public static readonly BlendMode SoftLight = new(key: 4, filterName: "CISoftLightBlendMode");
    public static readonly BlendMode HardLight = new(key: 5, filterName: "CIHardLightBlendMode");
    public static readonly BlendMode Lighten = new(key: 6, filterName: "CILightenBlendMode");
    public static readonly BlendMode Divide = new(key: 7, filterName: "CIDivideBlendMode");
    public static readonly BlendMode Luminosity = new(key: 8, filterName: "CILuminosityBlendMode");
}

// CALayer EDR opt-in: Disabled keeps SDR; Automatic/Always request headroom and route colour through Display P3.
// Always pins headroom for HDR-stable glow; Automatic lets the compositor reclaim SDR-only frames.
[SmartEnum<int>]
public sealed partial class EdrPolicy {
    public static readonly EdrPolicy Disabled = new(key: 0);
    public static readonly EdrPolicy Automatic = new(key: 1);
    public static readonly EdrPolicy Always = new(key: 2);
}

// Rasm-stable vibrancy names project to AppKit material/blending enums at the boundary.
[SmartEnum<int>]
public sealed partial class VibrancyMaterial {
    internal NSVisualEffectMaterial Native { get; }
    internal NSVisualEffectBlendingMode Blending { get; }

    public static readonly VibrancyMaterial HudWindow = new(key: 0, native: NSVisualEffectMaterial.HudWindow, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial Popover = new(key: 1, native: NSVisualEffectMaterial.Popover, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial Sheet = new(key: 2, native: NSVisualEffectMaterial.Sheet, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial ContentBackground = new(key: 3, native: NSVisualEffectMaterial.ContentBackground, blending: NSVisualEffectBlendingMode.WithinWindow);
    public static readonly VibrancyMaterial Sidebar = new(key: 4, native: NSVisualEffectMaterial.Sidebar, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial WindowBackground = new(key: 5, native: NSVisualEffectMaterial.WindowBackground, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial Menu = new(key: 6, native: NSVisualEffectMaterial.Menu, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial Titlebar = new(key: 7, native: NSVisualEffectMaterial.Titlebar, blending: NSVisualEffectBlendingMode.WithinWindow);
    public static readonly VibrancyMaterial Selection = new(key: 8, native: NSVisualEffectMaterial.Selection, blending: NSVisualEffectBlendingMode.WithinWindow);
    public static readonly VibrancyMaterial HeaderView = new(key: 9, native: NSVisualEffectMaterial.HeaderView, blending: NSVisualEffectBlendingMode.WithinWindow);
    // [?] ToolTip vibrancy legibility is unverified; flagged for the optional bridge pass.
    public static readonly VibrancyMaterial Tooltip = new(key: 10, native: NSVisualEffectMaterial.ToolTip, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial FullScreenUI = new(key: 11, native: NSVisualEffectMaterial.FullScreenUI, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial UnderWindowBackground = new(key: 12, native: NSVisualEffectMaterial.UnderWindowBackground, blending: NSVisualEffectBlendingMode.BehindWindow);
    public static readonly VibrancyMaterial UnderPageBackground = new(key: 13, native: NSVisualEffectMaterial.UnderPageBackground, blending: NSVisualEffectBlendingMode.BehindWindow);
}

// CALayer gradient points are normalized to the layer frame (0,0)–(1,1). Omit Start/End to use the profile
// row for Kind. Locations (when present) must match the colour count and ascend through [0,1].
[StructLayout(LayoutKind.Auto)]
public readonly record struct CosmeticGradientPoints(Option<PointF> Start = default, Option<PointF> End = default, Option<ReadOnlyMemory<float>> Locations = default) {
    public static CosmeticGradientPoints ConicSweep(PointF end) => new(End: Some(end));
    public static CosmeticGradientPoints Conic(PointF center, PointF sweep) => new(Start: Some(center), End: Some(sweep));
    public static CosmeticGradientPoints Stops(ReadOnlyMemory<float> locations) => new(Locations: Some(locations));
}

// Snap guides mirror per-axis colour/thickness/pattern and add labels; empty dashes intentionally match GH2 solid feedback.
// Dashed is the default additive style so custom guides do not read as native drag feedback.
public readonly record struct SnapGuideStyle(
    Color Tint,
    float Thickness = 1f,
    float LabelFontSize = 11f,
    string MagnitudeFormat = "0.#",
    GhDuration Duration = GhDuration.Normal,
    GhMotion Easing = GhMotion.EaseInOut,
    ReadOnlyMemory<float> Dashes = default,
    // Marching animates lineDashPhase over one pattern period; square caps and round joins keep corners crisp.
    float DashPhase = 0f,
    bool Marching = false) {
    internal static readonly ReadOnlyMemory<float> DashedPattern = new[] { 4f, 4f };
    public static SnapGuideStyle Dashed(Color tint) => new(Tint: tint, Dashes: DashedPattern);
    public static SnapGuideStyle Solid(Color tint) => new(Tint: tint);
    public static SnapGuideStyle MarchingAnts(Color tint) => new(Tint: tint, Dashes: DashedPattern, Marching: true);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct RepeatPolicy(int Cycles = 1, bool Yoyo = true, bool Infinite = false) {
    internal static readonly RepeatPolicy Once = new(Cycles: 1, Yoyo: false);
    internal int Count => Infinite ? 1 : Math.Max(1, Cycles);
    internal bool Reverses => (Cycles <= 0 && !Infinite) || Yoyo;
    internal int RemainingAfterFirst => Infinite ? 0 : Count - 1;
    internal bool Continues(int remaining) => Infinite || remaining > 0;
}

// Fire-and-forget CALayer chrome; retarget by dispose+reissue, not mid-flight mutation.
// Keyframe upgrades beyond Core Animation's five timing curves; Completion fires only on natural end.
[SkipUnionOps]
[Union]
public partial record CosmeticIntent {
    private CosmeticIntent() { }
    public Option<Func<Unit>> Completion { get; init; }
    public Option<Easing> Keyframe { get; init; }
    // GPU spring reuses SpringConfig; SpringInitialVelocity (default None => rest) seeds CASpringAnimation.InitialVelocity
    // so a fling/hand-off can launch the GPU layer with the CPU runner's live velocity instead of from rest.
    public Option<SpringConfig> Spring { get; init; }
    public Option<float> SpringInitialVelocity { get; init; }
    // EDR opt-in requests headroom/tone-mapping and routes glow/gradient colours through Display P3; Disabled stays sRGB.
    public EdrPolicy EdrMode { get; init; } = EdrPolicy.Disabled;
    // Co-animation opt-in (additive, default None): the child intents' animations are grouped onto the SAME
    // layer as this intent under ONE completion delegate — children must target DISTINCT key-paths.
    public Option<Seq<CosmeticIntent>> CoAnimate { get; init; }
    public sealed record PulseCase(RectangleF Bounds, Color Tint, GhDuration Duration, RepeatPolicy Repeat = default, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    // BackdropBlur routes BlurRadius to BackgroundFilters for real frosted backdrop blur and arms host Core Image filters.
    public sealed record GlowCase(RectangleF Bounds, Color Tint, float CornerRadius, float ShadowRadius, GhDuration Duration, RepeatPolicy Repeat = default, GhMotion Easing = GhMotion.EaseInOut, float BorderWidth = 0f, Option<Color> BorderColor = default, float BlurRadius = 0f, Option<BlendMode> Blend = default, bool BackdropBlur = false) : CosmeticIntent;
    public sealed record StrokeOnCase(ReadOnlyMemory<PointF> Polyline, Color Tint, float Thickness, GhDuration Duration, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record GradientCase(
        RectangleF Bounds,
        Seq<Color> Colors,
        GradientKind Kind,
        GhDuration Duration,
        CosmeticGradientPoints Points = default,
        RepeatPolicy Repeat = default,
        GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record TextLayerCase(string Text, PointF Origin, Color Tint, float FontSize, GhDuration Duration, RepeatPolicy Repeat = default, Option<string> FontFamily = default, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    // Count/Spacing replicate along X; CountY/SpacingY (default 1/0 => 1D row, prior behaviour) replicate the X-row
    // along Y by nesting an outer CAReplicatorLayer over the inner, yielding a Count x CountY grid (M-MOT-14b).
    public sealed record ReplicatorCase(RectangleF SourceBounds, int Count, float Spacing, Color Tint, GhDuration Duration, float InstanceDelay = 0f, float InstanceAlphaOffset = 0f, Option<Color> InstanceColour = default, float Rotation = 0f, float SpacingY = 0f, int CountY = 1, RepeatPolicy Repeat = default, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    public sealed record EmitterCase(
        RectangleF Bounds,
        Color Tint,
        GhDuration Duration,
        EmitterShape Shape,
        EmitterProfile Profile,
        float BirthRate = 20f,
        float Lifetime = 1.4f,
        float Velocity = 40f,
        float EmissionRange = MathF.Tau,
        float EmissionLongitude = 0f,
        float Spin = 0f,
        Option<Seq<Color>> Palette = default,
        RepeatPolicy Repeat = default,
        GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    // XStyle owns vertical guides; YStyle owns horizontal guides and falls back to Style.
    public sealed record SnapGuideCase(SnappingSnapshot Snapshot, SnapGuideStyle Style, Option<SnapGuideStyle> YStyle = default, RepeatPolicy Repeat = default) : CosmeticIntent;
    // Live blur uses NSVisualEffectView, not CALayer, so it samples canvas content behind the view.
    public sealed record VibrancyCase(RectangleF Bounds, VibrancyMaterial Material, float CornerRadius, GhDuration Duration, RepeatPolicy Repeat = default, GhMotion Easing = GhMotion.EaseInOut, NSVisualEffectState State = NSVisualEffectState.Active) : CosmeticIntent;
    // Core Image pipeline: Backdrop targets BackgroundFilters and arms host filters; false targets layer Filters.
    // Saturation/Brightness append a CIColorControls stage.
    public sealed record FilterCase(RectangleF Bounds, Seq<CIFilter> Pipeline, Color Tint, GhDuration Duration, bool Backdrop = true, float Saturation = 1f, float Brightness = 0f, RepeatPolicy Repeat = default, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    // Drawing-on glyph folds all IAnimatedStroke endpoints into one CAShapeLayer.Path; arcs degrade to chords at the macOS boundary.
    public sealed record GlyphCase(Seq<IAnimatedStroke> Strokes, PointF Origin, Color Tint, float Thickness, GhDuration Duration, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    // Path morph animates the CGPath-valued "path" key; matched vertex counts produce the clean tween.
    // PathAnimation routes through CAMediaTimingFunction, so only the four basic easings (Linear/EaseIn/EaseOut/
    // EaseInOut) map cleanly — richer Penner curves cannot be sampled on a CGPath key, so validation rejects them.
    public sealed record PathMorphCase(CGPath From, CGPath To, RectangleF Bounds, Color Tint, float Thickness, GhDuration Duration, GhMotion Easing = GhMotion.EaseInOut) : CosmeticIntent;
    // Non-visual tactile feedback on the Force Touch trackpad — no CALayer, fired once at attach (skipped
    // under reduce-motion). Pattern maps to the three NSHapticFeedbackPattern values.
    public sealed record HapticCase(HapticPattern Pattern) : CosmeticIntent;

    // Glyph cosmetic consumes raw Glyph.* strokes; AnimatedPath factories stay on the Stroke drawer.
    public static CosmeticIntent Glyph(Seq<IAnimatedStroke> strokes, PointF origin, Color tint, float thickness, GhDuration duration, GhMotion easing = GhMotion.EaseInOut) =>
        new GlyphCase(Strokes: strokes, Origin: origin, Tint: tint, Thickness: thickness, Duration: duration, Easing: easing);

    public static CosmeticIntent Gradient(RectangleF bounds, Color start, Color end, GradientKind kind, GhDuration duration, CosmeticGradientPoints points = default, GhMotion easing = GhMotion.EaseInOut, RepeatPolicy repeat = default) =>
        new GradientCase(Bounds: bounds, Colors: Seq(start, end), Kind: kind, Duration: duration, Points: points, Repeat: repeat, Easing: easing);
    public static CosmeticIntent Gradient(RectangleF bounds, Seq<Color> colors, GradientKind kind, GhDuration duration, CosmeticGradientPoints points = default, GhMotion easing = GhMotion.EaseInOut, RepeatPolicy repeat = default) =>
        new GradientCase(Bounds: bounds, Colors: colors, Kind: kind, Duration: duration, Points: points, Repeat: repeat, Easing: easing);
    public static CosmeticIntent Emitter(RectangleF bounds, Color tint, GhDuration duration, Option<EmitterShape> shape = default, Option<EmitterProfile> profile = default, float birthRate = 20f, float lifetime = 1.4f, float velocity = 40f, Option<float> emissionRange = default, Option<float> emissionLongitude = default, Option<float> spin = default, Option<Seq<Color>> palette = default, GhMotion easing = GhMotion.EaseInOut, RepeatPolicy repeat = default) =>
        new EmitterCase(
            Bounds: bounds, Tint: tint, Duration: duration,
            Shape: shape.IfNone(EmitterShape.Circle), Profile: profile.IfNone(EmitterProfile.Spark),
            BirthRate: birthRate, Lifetime: lifetime, Velocity: velocity,
            EmissionRange: emissionRange.IfNone(MathF.Tau), EmissionLongitude: emissionLongitude.IfNone(0f), Spin: spin.IfNone(0f),
            Palette: palette, Repeat: repeat, Easing: easing);
}

internal static class CosmeticIntentDiscriminators {
    // HapticCase carries no CALayer; every other case routes through the shared visual-attach path.
    extension(CosmeticIntent intent) {
        internal bool IsHaptic =>
            intent.Map(
                pulseCase: false, glowCase: false, strokeOnCase: false,
                gradientCase: false, textLayerCase: false, replicatorCase: false,
                emitterCase: false, snapGuideCase: false, vibrancyCase: false,
                filterCase: false, glyphCase: false, pathMorphCase: false,
                hapticCase: true);
    }
}

// The three FlexControl.Navigate targets: a point, a frame, or a semantic CanvasPosition. CanvasPosition owns the
// frame projection so Motion and Canvas share the same position semantics instead of mirroring native enum rows.
[SkipUnionOps]
[Union(SwitchMapStateParameterName = "state")]
public partial record NavigateTarget {
    private NavigateTarget() { }
    public sealed record PointCase(PointF Centre) : NavigateTarget;
    public sealed record FrameCase(RectangleF Region) : NavigateTarget;
    public sealed record PositionCase(CanvasPosition Where) : NavigateTarget;

    public static NavigateTarget Point(PointF centre) => new PointCase(Centre: centre);
    public static NavigateTarget Frame(RectangleF frame) => new FrameCase(Region: frame);
    public static NavigateTarget Position(CanvasPosition position) => new PositionCase(Where: position);
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

public interface IMotionState<TSelf> where TSelf : struct, IMotionState<TSelf> {
    public TSelf Advance(float frameDeltaSeconds);
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
                o => guard(float.IsFinite(stiffnessValue) && stiffnessValue > 0f, (Error)UiFault.InvalidInput(op: o, detail: $"Stiffness must be finite and > 0 (got {stiffnessValue:R}).")).ToFin(),
                o => guard(float.IsFinite(dampingValue) && dampingValue >= 0f, (Error)UiFault.InvalidInput(op: o, detail: $"Damping must be finite and >= 0 (got {dampingValue:R}).")).ToFin(),
                o => guard(float.IsFinite(massValue) && massValue > 0f, (Error)UiFault.InvalidInput(op: o, detail: $"Mass must be finite and > 0 (got {massValue:R}).")).ToFin())
            .IfFail(err => { fault = (UiFault)err; return unit; });
        validationError = fault;
    }

    // Response-space validation names response/dampingFraction/mass before deriving stiffness/damping.
    public static Fin<SpringConfig> Response(float response, float dampingFraction, float mass = 1f) =>
        Op.Of(name: nameof(Response))
            .AcceptAll(
                value: unit,
                o => o.AcceptFinite(value: response, detail: "spring response must be finite and > 0", requirePositive: true).Map(static _ => unit),
                o => o.AcceptFinite(value: dampingFraction, detail: "spring damping fraction must be finite and >= 0", nonNegative: true).Map(static _ => unit),
                o => o.AcceptFinite(value: mass, detail: "spring mass must be finite and > 0", requirePositive: true).Map(static _ => unit))
            .Map(_ => Create(
                stiffness: (float)(2.0 * Math.PI) / response * ((float)(2.0 * Math.PI) / response) * mass,
                damping: 4f * (float)Math.PI * dampingFraction * mass / response,
                mass: mass));

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

    // Preset literals are statically valid; the Fin is unwrapped at type-load (a rejected literal is a coding
    // error, not a recoverable runtime fault) via IfFail onto an unreachable identity config.
    private static SpringPreset Of(int key, float response, float dampingFraction) =>
        new(key: key, config: SpringConfig.Response(response: response, dampingFraction: dampingFraction)
            .IfFail(static _ => SpringConfig.Create(stiffness: 1f, damping: 1f, mass: 1f)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpringRunnerState<T>(T Value, T Velocity, T Target, SpringConfig Config, IMotionVector<T> Vector, Action<T> Sink, TimeProvider Clock, long Timestamp) : IMotionState<SpringRunnerState<T>> {
    // Analytic damped-spring step (Juckett): dt-specific coefficients make integration frame-rate-independent and stable.
    // A large dt jumps to the exact analytic state; coefficients are ∂(pos,vel)/∂(pos0,vel0).
    public SpringRunnerState<T> Advance(float frameDeltaSeconds) {
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

    // Juckett coefficients per regime; ζ epsilon selects under/critical/over, near-zero ω0 returns identity.
    // Overdamped velPos trailing *z2 is verified against the continuous partial derivatives.
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

public abstract class MotionHandle<TState, TValue> : IDisposable where TState : struct, IMotionState<TState> {
    protected Atom<TState> Cell { get; }
    protected Subscription Subscription { get; }
    protected Action Wake { get; }
    protected bool Settled { get; }

    private protected MotionHandle(Atom<TState> cell, Subscription subscription, Action wake, bool settled) {
        Cell = cell;
        Subscription = subscription;
        Wake = wake;
        Settled = settled;
    }

    public abstract TValue CurrentValue { get; }
    public abstract bool IsSettled { get; }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(obj: this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            Subscription.Dispose();
        }
    }
}

public sealed class SpringHandle<T> : MotionHandle<SpringRunnerState<T>, T> {
    internal SpringHandle(Atom<SpringRunnerState<T>> cell, Subscription subscription, Action wake, bool settled = false)
        : base(cell: cell, subscription: subscription, wake: wake, settled: settled) { }

    public override T CurrentValue => Cell.Value.Value;
    public T CurrentVelocity => Cell.Value.Velocity;
    public T CurrentTarget => Cell.Value.Target;
    public bool IsConverged => !Cell.Value.IsActive;
    public override bool IsSettled => IsConverged;

    public Fin<Unit> Retarget(T target, Option<T> initialVelocity = default) =>
        RetargetWhen(target: target, shouldUpdate: static _ => true, initialVelocity: initialVelocity);

    public Fin<Unit> RetargetWhen(T target, Func<T, bool> shouldUpdate, Option<T> initialVelocity = default) {
        SpringRunnerState<T> snapshot = Cell.Value;
        return from validUpdate in Optional(shouldUpdate).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(RetargetWhen)), detail: "predicate is required"))
               from validTarget in Op.Of(name: nameof(RetargetWhen)).AcceptFinite(value: target, vector: snapshot.Vector, detail: "spring target/velocity must be finite")
               from validVelocity in initialVelocity
                   .Map(value => Op.Of(name: nameof(RetargetWhen)).AcceptFinite(value: value, vector: snapshot.Vector, detail: "spring target/velocity must be finite").Map(Some))
                   .IfNone(Fin.Succ(value: Option<T>.None))
               from result in validUpdate(arg: snapshot.Target) switch {
                   false => Fin.Succ(value: unit),
                   true => Fin.Succ(value: Settled
                       ? Op.Side(() => {
                           _ = Cell.Swap(state => state with {
                               Value = validTarget,
                               Target = validTarget,
                               Velocity = validVelocity.IfNone(state.Vector.Zero),
                               Timestamp = state.Clock.GetTimestamp(),
                           });
                           snapshot.Sink(validTarget);
                       })
                       : Op.Side(() => {
                           _ = Cell.Swap(state => state with {
                               Target = validTarget,
                               Velocity = validVelocity.IfNone(state.Velocity),
                           });
                           Wake();
                       })),
               }
               select result;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PulseRunnerState<T>(Animated<T> Animated, T From, T To, GhDuration Duration, GhMotion Easing, RepeatPolicy Repeat, int CyclesRemaining, IMotionVector<T> Vector, Action<T> Sink, Grasshopper2.UI.Canvas.Canvas Canvas) : IMotionState<PulseRunnerState<T>> {
    // Animated advances only through Canvas.Animate; tick first, then fold the cycle transition.
    public PulseRunnerState<T> Advance(float frameDeltaSeconds) {
        _ = Canvas.Animate(animated: Animated);
        return (Animated.State == GhState.Finished, Repeat.Continues(remaining: CyclesRemaining)) switch {
            (true, true) => this with {
                Animated = Animated<T>.CreateUnfinished(
                    value0: Repeat.Reverses ? Animated.Value1 : From,
                    value1: Repeat.Reverses ? Animated.Value0 : To,
                    duration: Animators.DurationToTimeSpan(duration: Duration),
                    motion: Easing,
                    interpolator: Vector.Interpolate),
                CyclesRemaining = Repeat.Infinite ? CyclesRemaining : CyclesRemaining - 1,
            },
            _ => this,
        };
    }

    public Unit Emit() { Sink(Animated.ValueNow); return unit; }

    public bool IsActive => Animated.State != GhState.Finished || Repeat.Continues(remaining: CyclesRemaining);
}

// Mirror of SpringHandle<T>: an Atom-confined runner cell exposed for mid-flight Retarget. The
// MotionRunner re-reads the cell each CAS tick, so a concurrent Retarget re-seeds the Animated curve.
public sealed class PulseHandle<T> : MotionHandle<PulseRunnerState<T>, T> {
    internal PulseHandle(Atom<PulseRunnerState<T>> cell, Subscription subscription, Action wake, bool settled = false)
        : base(cell: cell, subscription: subscription, wake: wake, settled: settled) { }

    public override T CurrentValue => Cell.Value.Animated.ValueNow;
    public T CurrentTarget => Cell.Value.To;
    public bool IsCycling => Cell.Value.IsActive;
    public override bool IsSettled => !IsCycling;

    public Fin<Unit> Retarget(T target, Option<RepeatPolicy> repeat = default) =>
        RetargetWhen(target: target, shouldUpdate: static _ => true, repeat: repeat);

    // Symmetric with SpringHandle.RetargetWhen: only re-seed the curve when the predicate accepts the current
    // target, so a stale retarget is a no-op rather than a restart.
    public Fin<Unit> RetargetWhen(T target, Func<T, bool> shouldUpdate, Option<RepeatPolicy> repeat = default) {
        PulseRunnerState<T> snapshot = Cell.Value;
        return from validUpdate in Optional(shouldUpdate).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(RetargetWhen)), detail: "predicate is required"))
               from validTarget in Op.Of(name: nameof(RetargetWhen)).AcceptFinite(value: target, vector: snapshot.Vector, detail: "pulse target must be finite")
               from result in validUpdate(arg: snapshot.To) switch {
                   false => Fin.Succ(value: unit),
                   true => Fin.Succ(value: Settled
                       ? Op.Side(() => SettlePulse(snapshot: snapshot, target: validTarget))
                       : Op.Side(() => {
                           _ = Cell.Swap(state => state with {
                               From = state.Animated.ValueNow,
                               To = validTarget,
                               Repeat = repeat.IfNone(state.Repeat),
                               CyclesRemaining = repeat.Map(static policy => policy.RemainingAfterFirst).IfNone(state.CyclesRemaining),
                               Animated = Animated<T>.CreateUnfinished(
                                   value0: state.Animated.ValueNow,
                                   value1: validTarget,
                                   duration: Animators.DurationToTimeSpan(duration: state.Duration),
                                   motion: state.Easing,
                                   interpolator: state.Vector.Interpolate),
                           });
                           Wake();
                       })),
               }
               select result;
    }

    private void SettlePulse(PulseRunnerState<T> snapshot, T target) {
        T rest = snapshot.Repeat.Reverses ? snapshot.From : target;
        _ = Cell.Swap(state => state with {
            From = rest,
            To = rest,
            Repeat = RepeatPolicy.Once,
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

    // --- [OKLAB_CONVERSION] ----------------------------------------------------------------
    // Ottosson 2020 with CSS premultiplied alpha: transparent endpoints contribute no colour.
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
    public static ISparkle FacePath(GraphicsPath face, bool attachedToContent = false) =>
        new FaceSparkle(face: face, attachedToContent: attachedToContent);
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

public static class Motion {
    // Default retina backing scale when a screen cannot be resolved.
    private const float RetinaScaleDefault = 2f;
    // GPU keyframe sampling density for the sampled-curve CAKeyFrameAnimation path.
    private const int KeyframeSamples = 60;
    // CPU fallback refresh when no Pacer vsync timestamp is available (Eto coalesces near this rate).
    private const float DefaultRefreshHz = 60f;
    // Physics-derived frame-rate ceiling/floor (M-MOT-9). FallbackRefreshHz is the assumed panel ceiling when a
    // screen cannot report its MinimumRefreshInterval; FloorRefreshHz is the lowest preferred Hz a spring requests.
    private const float FallbackRefreshHz = 120f;
    private const float FloorRefreshHz = 30f;
    private static readonly (CAGradientLayerType Type, CGPoint Start, CGPoint End)[] CosmeticGradientProfile = [
        (CAGradientLayerType.Axial, CGPoint.Empty, new CGPoint(x: 1, y: 0)),
        (CAGradientLayerType.Radial, new CGPoint(x: 0.5, y: 0.5), new CGPoint(x: 1, y: 1)),
        (CAGradientLayerType.Conic, new CGPoint(x: 0.5, y: 0.5), new CGPoint(x: 1, y: 0.5)),
    ];

    // Clock selection is internal: reduce-motion uses the message loop; DisplayLink failures demote to message loop.
    // DisplayLink rate stays None so PacerOption resolves the panel ceiling.
    internal static MotionClock ResolveClock(Grasshopper2.UI.Canvas.Canvas canvas) =>
        MotionAccessibility.ShouldReduceMotion ? MotionClock.MessageLoop : MotionClock.DisplayLink();

    // Shared NeedCanvas + PacerOption + Paint.Hook scaffold; variants provide their own runner state.
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

    public static GrasshopperUiIntent<Subscription> Tween<TValue>(
        Animated<TValue> animated, Action<TValue> sink) =>
        GhUi.Canvas(run: scope =>
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Tween)), detail: "sink delegate is required"))
            from canvas0 in scope.NeedCanvas()
            from result in MotionAccessibility.ShouldReduceMotion
                ? from _ in Op.Of(name: nameof(Tween)).Attempt(body: () => { validSink(animated.Value1); return unit; }, what: "tween reduce-motion settle")
                  select Subscription.Empty
                : Pipeline(opName: nameof(Tween), phase: CanvasPaintPhase.BeforeBackground, clock: ResolveClock(canvas: canvas0),
                    paintFactory: (canvas, pacer) => _ => Try.lift(f: () => {
                        validSink(canvas.Animate(animated: animated));
                        return pacer.PauseWhen(finished: animated.State == GhState.Finished);
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Tween)), detail: $"tick threw: {error.Message}"))).Run(scope: scope)
            select result);

    public static GrasshopperUiIntent<SpringHandle<TValue>> Spring<TValue>(
        TValue start, TValue target, SpringConfig config, IMotionVector<TValue> vector,
        Action<TValue> sink, Option<TValue> initialVelocity, TimeProvider? timeSource) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "sink delegate is required"))
            from _finite in Op.Of(name: nameof(Spring)).AcceptAll(
                value: unit,
                o => o.AcceptFinite(value: start, vector: validVector, detail: "spring start must be finite").Map(static _ => unit),
                o => o.AcceptFinite(value: target, vector: validVector, detail: "spring target must be finite").Map(static _ => unit),
                o => initialVelocity.Map(value => o.AcceptFinite(value: value, vector: validVector, detail: "spring velocity must be finite").Map(static _ => unit)).IfNone(Fin.Succ(unit)))
            from canvas in scope.NeedCanvas()
            let resolvedClock = timeSource ?? TimeProvider.System
            let restState = new SpringRunnerState<TValue>(
                Value: target, Velocity: validVector.Zero, Target: target, Config: config, Vector: validVector,
                Sink: validSink, Clock: resolvedClock, Timestamp: resolvedClock.GetTimestamp())
            let liveState = new SpringRunnerState<TValue>(
                Value: start, Velocity: initialVelocity.IfNone(validVector.Zero), Target: target, Config: config,
                Vector: validVector, Sink: validSink, Clock: resolvedClock, Timestamp: resolvedClock.GetTimestamp())
            from handle in RunOrSettle(
                canvas: canvas, opName: nameof(Spring), clock: ResolveClock(canvas: canvas),
                restState: restState, liveState: liveState, restEmit: () => validSink(target),
                handle: static (cell, sub, wake, settled) => new SpringHandle<TValue>(cell: cell, subscription: sub, wake: wake, settled: settled)).Run(scope: scope)
            select handle);

    // Fling projects a rest target from launch velocity using spring decay time, then delegates to Spring.
    // Zero damping has no finite projection, so the decay constant floors at the float rest epsilon.
    public static GrasshopperUiIntent<SpringHandle<TValue>> Fling<TValue>(
        TValue start, TValue velocity, SpringConfig config, IMotionVector<TValue> vector,
        Action<TValue> sink, TimeProvider? timeSource = null) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Fling)), detail: "vector is required"))
            from validVelocity in Op.Of(name: nameof(Fling)).AcceptFinite(value: velocity, vector: validVector, detail: "fling velocity must be finite")
            let settleFactor = config.Mass / MathF.Max(config.Damping, MotionVector.Float.RestEpsilon)
            let target = validVector.Add(start, validVector.Scale(validVelocity, settleFactor))
            from handle in Spring(
                start: start, target: target, config: config, vector: validVector, sink: sink,
                initialVelocity: Some(validVelocity), timeSource: timeSource).Run(scope: scope)
            select handle);

    // Reduce-motion emits the rest pose with a settled handle; otherwise adopt a pacer and run RunnerPaint.
    // Callers supply validation, cell construction, and handle factory.
    private static GrasshopperUiIntent<THandle> RunOrSettle<TState, THandle>(
        Grasshopper2.UI.Canvas.Canvas canvas, string opName, MotionClock clock,
        TState restState, TState liveState, Action restEmit,
        Func<Atom<TState>, Subscription, Action, bool, THandle> handle)
        where TState : struct, IMotionState<TState> =>
        GhUi.Canvas(run: scope =>
            MotionAccessibility.ShouldReduceMotion
                ? from _ in Op.Of(name: opName).Attempt(body: () => { restEmit(); return unit; }, what: $"{opName} reduce-motion settle")
                  select handle(arg1: Atom(restState), arg2: Subscription.Empty, arg3: canvas.ScheduleRedraw, arg4: true)
                : from pacer in PacerOption(canvas: canvas, clock: clock)
                  let cell = Atom(liveState)
                  from bundle in RunPipeline(canvas: canvas, pacer: pacer, opName: opName, phase: CanvasPaintPhase.BeforeBackground, clock: clock,
                      paint: RunnerPaint(cell: cell, canvas: canvas, pacer: pacer, opName: opName)).Run(scope: scope)
                  select handle(arg1: cell, arg2: bundle.Subscription, arg3: bundle.Wake, arg4: false));

    internal static (Subscription Subscription, Action Wake) MotionBundleOf(
        Option<Pacer> pacer, Subscription sub, Grasshopper2.UI.Canvas.Canvas canvas) {
        Action wake = canvas.ScheduleRedraw;
        return (Subscription: Subscription.DisposeOnce(sub), Wake: wake);
    }

    // DisplayLink demotes to message-loop on reduce-motion or missing NSView host, so transitions still land.
    internal static Fin<Option<Pacer>> PacerOption(Grasshopper2.UI.Canvas.Canvas canvas, MotionClock clock) =>
        clock.Switch(state: canvas,
            messageLoopCase: static (_, _) => Fin.Succ(Option<Pacer>.None),
            displayLinkCase: static (c, d) => MotionAccessibility.ShouldReduceMotion
                ? Fin.Succ(Option<Pacer>.None)
                : Fin.Succ(Pacer.For(canvas: c, rate: d.Rate.ToNullable()).Map(static pacer => Some(pacer)).IfFail(Option<Pacer>.None)));

    // Option<Pacer> centralizes manual redraw fallback, pooled release, and rested-edge pause.
    extension(Option<Pacer> pacer) {
        internal Unit ResumeOr(Grasshopper2.UI.Canvas.Canvas canvas) =>
            pacer is { IsSome: true, Case: Pacer p } ? Op.Side(() => p.Resume()) : Op.Side(canvas.ScheduleRedraw);

        internal Unit ReleaseOnce() =>
            pacer is { IsSome: true, Case: Pacer p } ? Op.Side(() => { _ = p.Pause(); _ = p.Release(); }) : unit;

        internal Unit PauseWhen(bool finished) =>
            finished && pacer is { IsSome: true, Case: Pacer p } ? p.Pause() : unit;
    }

    private static Func<PaintScope, Fin<Unit>> RunnerPaint<TState>(
        Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer, string opName)
        where TState : struct, IMotionState<TState> {
        // One runner per pipeline preserves wantingTicks/lastVsyncTimestamp across frames; paint only calls Tick.
        MotionRunner<TState> runner = MotionRunner<TState>.Of(cell: cell, canvas: canvas, pacer: pacer);
        return paintScope => Op.Of(name: opName).Attempt(
            body: () => { _ = runner.Tick(); return unit; },
            what: "motion tick");
    }

    public static GrasshopperUiIntent<PulseHandle<TValue>> Pulse<TValue>(
        TValue start, TValue target, GhDuration duration, GhMotion easing, RepeatPolicy repeat,
        IMotionVector<TValue> vector, Action<TValue> sink) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "sink delegate is required"))
            from _finite in Op.Of(name: nameof(Pulse)).AcceptAll(
                value: unit,
                o => o.AcceptFinite(value: start, vector: validVector, detail: "pulse start must be finite").Map(static _ => unit),
                o => o.AcceptFinite(value: target, vector: validVector, detail: "pulse target must be finite").Map(static _ => unit))
            from canvas in scope.NeedCanvas()
                // A yoyo's resting pose is its From (it returns to start); a one-way pulse rests at target.
            let rest = repeat.Reverses ? start : target
            let restState = new PulseRunnerState<TValue>(
                Animated: Animated<TValue>.CreateUnfinished(value0: rest, value1: rest, duration: TimeSpan.Zero, motion: easing, interpolator: validVector.Interpolate),
                From: rest, To: rest, Duration: duration, Easing: easing, Repeat: RepeatPolicy.Once,
                CyclesRemaining: 0, Vector: validVector, Sink: validSink, Canvas: canvas)
            let liveState = new PulseRunnerState<TValue>(
                Animated: Animated<TValue>.CreateUnfinished(value0: start, value1: target, duration: Animators.DurationToTimeSpan(duration: duration), motion: easing, interpolator: validVector.Interpolate),
                From: start, To: target, Duration: duration, Easing: easing, Repeat: repeat,
                CyclesRemaining: repeat.RemainingAfterFirst, Vector: validVector, Sink: validSink, Canvas: canvas)
            from handle in RunOrSettle(
                canvas: canvas, opName: nameof(Pulse), clock: ResolveClock(canvas: canvas),
                restState: restState, liveState: liveState, restEmit: () => validSink(rest),
                handle: static (cell, sub, wake, settled) => new PulseHandle<TValue>(cell: cell, subscription: sub, wake: wake, settled: settled)).Run(scope: scope)
            select handle);

    public static GrasshopperUiIntent<Subscription> Sequence<TValue>(
        TValue start, Seq<(TValue Target, GhDuration Duration, GhMotion Motion)> steps,
        IMotionVector<TValue> vector, Action<TValue> sink) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Sequence)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Sequence)), detail: "sink delegate is required"))
            from validSteps in Optional(steps).Filter(static s => s.Count > 0)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Sequence)), detail: "at least one step is required"))
            from _finite in Op.Of(name: nameof(Sequence)).AcceptFinite(value: start, vector: validVector, detail: "sequence start must be finite")
                .Bind(_ => validSteps.TraverseM(s => Op.Of(name: nameof(Sequence)).AcceptFinite(value: s.Target, vector: validVector, detail: "sequence targets must be finite")).As().Map(static _ => unit))
                // GH2 Animated.Chain drops dwell steps whose Target equals the prior value; non-numeric epsilon is not general.
            let animated = validSteps.Fold(
                initialState: Animated<TValue>.CreateFinished(start, validVector.Interpolate),
                f: static (acc, step) => acc + (value: step.Target, motion: step.Motion, duration: step.Duration))
            from sub in Tween(animated: animated, sink: validSink).Run(scope: scope)
            select sub);

    public static GrasshopperUiIntent<Subscription> Stroke(
        AnimatedPath path, GhDuration duration, GhMotion easing, PaintStyle style,
        PointF origin, float scale, float angle) =>
        GhUi.Canvas(run: scope =>
            from validPath in Optional(path).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Stroke)), detail: "path is required"))
            from validOrigin in Op.Of(name: nameof(Stroke)).AcceptPoint(value: origin, detail: "non-finite origin")
            from validScale in Op.Of(name: nameof(Stroke)).AcceptFinite(value: scale, detail: "non-finite scale")
            from validAngle in Op.Of(name: nameof(Stroke)).AcceptFinite(value: angle, detail: "non-finite angle")
            from canvas0 in scope.NeedCanvas()
            from result in Pipeline(opName: nameof(Stroke), phase: CanvasPaintPhase.AfterObjects, clock: ResolveClock(canvas: canvas0),
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
                        return MotionAccessibility.ShouldReduceMotion ? unit : pacer.PauseWhen(finished: progress.State == GhState.Finished);
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Stroke)), detail: $"draw threw: {error.Message}"));
                }).Run(scope: scope)
            select result);

    public static GrasshopperUiIntent<Subscription> Theme(
        Skin start, Skin target, GhDuration duration, GhMotion easing, Action<Skin> sink) =>
        Tween(
            animated: Animated<Skin>.CreateUnfinished(
                value0: start, value1: target,
                duration: Animators.DurationToTimeSpan(duration: duration),
                motion: easing,
                interpolator: static (a, b, t) => a.Interpolate(b, (float)t)),
            sink: sink);

    public static GrasshopperUiIntent<Unit> Navigate(NavigateTarget target, GhDuration duration, float minZoom, float maxZoom) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from document in scope.NeedDocument()
            from validMin in Op.Of(name: nameof(Navigate)).AcceptFinite(value: minZoom, detail: "min zoom must be finite and positive", requirePositive: true)
            from validMax in Op.Of(name: nameof(Navigate)).AcceptFinite(value: maxZoom, detail: "max zoom must be finite and positive", requirePositive: true)
            from _ in guard(validMax >= validMin, (Error)UiFault.InvalidInput(op: Op.Of(name: nameof(Navigate)), detail: "max zoom below min zoom")).ToFin()
                // Reduce-motion collapses the tween to an Abrupt (0ms) host frame; the Point case additionally snaps
                // the document projection so the persisted centre/zoom matches the host's framing immediately.
            let effectiveDuration = MotionAccessibility.ShouldReduceMotion ? GhDuration.Abrupt : duration
            let navigation = (Canvas: canvas, Document: document, Min: validMin, Max: validMax, Effective: effectiveDuration)
            from result in target.Switch(
                state: navigation,
                pointCase: static (s, p) =>
                    from validCentre in Op.Of(name: nameof(Navigate)).AcceptPoint(value: p.Centre, detail: "non-finite centre")
                    from __ in MotionAccessibility.ShouldReduceMotion switch {
                        true => Op.Of(name: nameof(Navigate)).Attempt(body: () => {
                            float zoom = Math.Clamp(value: s.Document.Projection.zoom, min: s.Min, max: s.Max);
                            s.Canvas.Projection = s.Canvas.Projection.SetZoom(zoom: zoom).SetCentre(centre: validCentre, frame: s.Canvas.VisibleFrame);
                            s.Document.Projection = (validCentre, zoom);
                            return unit;
                        }, what: "navigate reduce-motion snap")
                        ,
                        false => Op.Of(name: nameof(Navigate)).Attempt(body: () => { s.Canvas.Navigate(point: validCentre, zoomLimits: (s.Min, s.Max), duration: s.Effective); return unit; }, what: "navigate"),
                    }
                    select unit,
                frameCase: static (s, f) =>
                    Op.Of(name: nameof(Navigate)).AcceptRect(value: f.Region, detail: "non-finite frame")
                        .Bind(validFrame => Op.Of(name: nameof(Navigate)).Attempt(body: () => { s.Canvas.Navigate(frame: validFrame, zoomLimits: (s.Min, s.Max), duration: s.Effective); return unit; }, what: "navigate frame")),
                positionCase: static (s, pos) =>
                    from __ in UiRail.NavigateCanvasPosition(
                        canvas: s.Canvas,
                        document: s.Document,
                        position: pos.Where,
                        policy: new CanvasViewPolicy(MinimumZoom: s.Min, MaximumZoom: s.Max, Duration: Animators.DurationToTimeSpan(duration: s.Effective)))
                    select unit)
            select result);

    public static GrasshopperUiIntent<Subscription> ZoomGate(ZoomThreshold threshold, Action<float> sink) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ZoomGate)), detail: "sink delegate is required"))
                // Paint fires every frame; last-value Atom emits only perceptible zoom deltas.
            let lastValue = Atom(Option<float>.None)
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: paintScope => Try.lift(f: () => {
                    float now = canvas.AnimatedZoomFactor(threshold: threshold);
                    Option<float> previous = lastValue.Value;
                    _ = previous.Filter(prev => MathF.Abs(prev - now) <= MotionVector.Float.RestEpsilon).IsSome
                        ? unit
                        : Op.Side(() => { _ = lastValue.Swap(_ => Some(now)); validSink(now); });
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(ZoomGate)), detail: $"tick threw: {error.Message}")),
                clock: ResolveClock(canvas: canvas)).Run(scope: scope)
            select sub);

    public static GrasshopperUiIntent<Subscription> Cosmetic(CosmeticIntent intent) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from view in Optional(canvas.ControlObject as NSView).ToFin(Fail: UiFault.MutationRejected(
                op: Op.Of(name: nameof(Cosmetic)), detail: "canvas.ControlObject is not an NSView"))
            from mapped in PrepareCosmetic(canvas: canvas, intent: intent)
            let key = (NSString)$"rasm.cosmetic.{Guid.NewGuid():N}"
            from sub in Subscription.Bind(
                // BOUNDARY ADAPTER — Subscription.Bind's attach is an Action; ThrowIfFail surfaces the CosmeticAttach
                // rail so Bind's Try.lift re-rails the fault as an attach failure instead of dropping it.
                attach: () => CosmeticAttach(view: view, intent: mapped, key: key).ThrowIfFail(),
                detach: () => CosmeticStrip(view: view, key: key),
                marshalToUi: true)
            select sub);

    // Batch attaches all intents inside one CATransaction and returns one Subscription that strips every keyed layer.
    // Inputs validate up front; one rejection fails the whole batch.
    public static GrasshopperUiIntent<Subscription> CosmeticBatch(Seq<CosmeticIntent> intents, Option<Func<Unit>> completion = default) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from view in Optional(canvas.ControlObject as NSView).ToFin(Fail: UiFault.MutationRejected(
                op: Op.Of(name: nameof(CosmeticBatch)), detail: "canvas.ControlObject is not an NSView"))
            from _nonEmpty in guard(!intents.IsEmpty, (Error)UiFault.InvalidInput(op: Op.Of(name: nameof(CosmeticBatch)), detail: "batch requires at least one intent")).ToFin()
            from prepared in intents.TraverseM(intent => PrepareCosmetic(canvas: canvas, intent: intent)
                .Map(mapped => (Mapped: mapped, Key: (NSString)$"rasm.cosmetic.{Guid.NewGuid():N}"))).As()
            from sub in Subscription.Bind(
                // BOUNDARY ADAPTER — one outer transaction batches every CosmeticAttach onto a single commit; the shared
                // CompletionBlock fires once the batch transaction's render settles. ThrowIfFail re-rails an attach fault.
                attach: () => AttachBatch(view: view, prepared: prepared, completion: completion).ThrowIfFail(),
                detach: () => prepared.Iter(entry => CosmeticStrip(view: view, key: entry.Key)),
                marshalToUi: true)
            select sub);

    [SupportedOSPlatform("macos14.0")]
    private static Fin<Unit> AttachBatch(NSView view, Seq<(CosmeticIntent Mapped, NSString Key)> prepared, Option<Func<Unit>> completion) {
        CATransaction.Begin();
        _ = completion.IfSome(run => CATransaction.CompletionBlock = () => _ = run());
        Fin<Unit> outcome = prepared.Fold(
            initialState: Fin.Succ(unit),
            f: (acc, entry) => acc.Bind(_ => CosmeticAttach(view: view, intent: entry.Mapped, key: entry.Key)));
        CATransaction.Commit();
        return outcome;
    }

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
                    .Bind(_ => guard(g.Colors.Count >= 2, (Error)UiFault.InvalidInput(op: op, detail: $"gradient requires at least two colours (got {g.Colors.Count})")).ToFin())
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
            vibrancyCase: static (c, v) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.VibrancyCase));
                return op.AcceptRect(value: v.Bounds, detail: "non-finite vibrancy bounds")
                    .Bind(_ => op.AcceptFinite(value: v.CornerRadius, detail: "non-finite corner radius", nonNegative: true))
                    .Map<CosmeticIntent>(_ => v with { Bounds = MapCosmeticRect(canvas: c, bounds: v.Bounds) });
            },
            glyphCase: static (c, gl) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.GlyphCase));
                return op.AcceptFinite(value: gl.Thickness, detail: "non-positive thickness", requirePositive: true)
                    .Bind(_ => op.AcceptPoint(value: gl.Origin, detail: "non-finite glyph origin"))
                    .Bind(_ => guard(!gl.Strokes.IsEmpty, (Error)UiFault.InvalidInput(op: op, detail: "glyph requires at least one stroke")).ToFin())
                    .Map<CosmeticIntent>(_ => gl with { Origin = c.Map(point: gl.Origin, from: CoordinateSystem.Content, to: CoordinateSystem.Control) });
            },
            pathMorphCase: static (c, pm) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.PathMorphCase));
                return op.AcceptRect(value: pm.Bounds, detail: "non-finite path-morph bounds")
                    .Bind(_ => op.AcceptFinite(value: pm.Thickness, detail: "non-positive thickness", requirePositive: true))
                    .Bind(_ => guard(pm.Easing is GhMotion.Linear or GhMotion.EaseIn or GhMotion.EaseOut or GhMotion.EaseInOut, (Error)UiFault.InvalidInput(op: op, detail: $"path morph supports only basic timing (Linear/EaseIn/EaseOut/EaseInOut); got {pm.Easing}")).ToFin())
                    .Map<CosmeticIntent>(_ => pm with { Bounds = MapCosmeticRect(canvas: c, bounds: pm.Bounds) });
            },
            filterCase: static (c, f) => {
                Op op = Op.Of(name: nameof(CosmeticIntent.FilterCase));
                return op.AcceptRect(value: f.Bounds, detail: "non-finite filter bounds")
                    .Bind(_ => op.AcceptFinite(value: f.Saturation, detail: "non-finite saturation", nonNegative: true))
                    .Bind(_ => guard(f.Saturation <= 2f, (Error)UiFault.InvalidInput(op: op, detail: $"CIColorControls saturation must lie in [0,2] (got {f.Saturation:R})")).ToFin())
                    .Bind(_ => op.AcceptFinite(value: f.Brightness, detail: "non-finite brightness"))
                    .Bind(_ => guard(f.Brightness is >= -1f and <= 1f, (Error)UiFault.InvalidInput(op: op, detail: $"CIColorControls brightness must lie in [-1,1] (got {f.Brightness:R})")).ToFin())
                    .Map<CosmeticIntent>(_ => f with { Bounds = MapCosmeticRect(canvas: c, bounds: f.Bounds) });
            },
            hapticCase: static (_, h) => Fin.Succ<CosmeticIntent>(h),
            state: canvas);

    // One descriptor drives key-path, target alpha, repeat policy, and keyframe/spring options per case.
    // strokeEnd is non-repeating; haptic never reaches a layer.
    private readonly record struct CosmeticAnimSpec(string KeyPath, float To, RepeatPolicy Repeat, GhDuration Duration, GhMotion Easing, bool Repeating = true);

    private static CosmeticAnimSpec AnimSpecOf(CosmeticIntent intent) =>
        intent.Switch(
            pulseCase: static p => new CosmeticAnimSpec(KeyPath: "opacity", To: p.Tint.A, Repeat: p.Repeat, Duration: p.Duration, Easing: p.Easing),
            glowCase: static g => new CosmeticAnimSpec(KeyPath: "shadowOpacity", To: g.Tint.A, Repeat: g.Repeat, Duration: g.Duration, Easing: g.Easing),
            strokeOnCase: static s => new CosmeticAnimSpec(KeyPath: "strokeEnd", To: 1f, Repeat: RepeatPolicy.Once, Duration: s.Duration, Easing: s.Easing, Repeating: false),
            gradientCase: static g => new CosmeticAnimSpec(KeyPath: "opacity", To: g.Colors.Map(static c => c.A).Fold(0f, static (m, a) => Math.Max(m, a)), Repeat: g.Repeat, Duration: g.Duration, Easing: g.Easing),
            textLayerCase: static t => new CosmeticAnimSpec(KeyPath: "opacity", To: t.Tint.A, Repeat: t.Repeat, Duration: t.Duration, Easing: t.Easing),
            replicatorCase: static r => new CosmeticAnimSpec(KeyPath: "opacity", To: r.Tint.A, Repeat: r.Repeat, Duration: r.Duration, Easing: r.Easing),
            emitterCase: static e => new CosmeticAnimSpec(KeyPath: "opacity", To: e.Tint.A, Repeat: e.Repeat, Duration: e.Duration, Easing: e.Easing),
            snapGuideCase: static g => new CosmeticAnimSpec(KeyPath: "opacity", To: g.Style.Tint.A, Repeat: g.Repeat, Duration: g.Style.Duration, Easing: g.Style.Easing),
            vibrancyCase: static v => new CosmeticAnimSpec(KeyPath: "opacity", To: 1f, Repeat: v.Repeat, Duration: v.Duration, Easing: v.Easing),
            filterCase: static f => new CosmeticAnimSpec(KeyPath: "opacity", To: f.Tint.A, Repeat: f.Repeat, Duration: f.Duration, Easing: f.Easing),
            glyphCase: static gl => new CosmeticAnimSpec(KeyPath: "strokeEnd", To: 1f, Repeat: RepeatPolicy.Once, Duration: gl.Duration, Easing: gl.Easing, Repeating: false),
            // "path" is CGPath-valued; To is a placeholder — BuildCosmeticAnimation routes PathMorphCase to PathAnimation
            // (CABasicAnimation on "path"), never to the float PropertyAnimation that reads spec.To.
            pathMorphCase: static pm => new CosmeticAnimSpec(KeyPath: "path", To: 1f, Repeat: RepeatPolicy.Once, Duration: pm.Duration, Easing: pm.Easing, Repeating: false),
            hapticCase: static _ => new CosmeticAnimSpec(KeyPath: "haptic", To: 0f, Repeat: RepeatPolicy.Once, Duration: GhDuration.Normal, Easing: GhMotion.Linear, Repeating: false));

    private static Fin<Unit> AcceptCoAnimationLayer(CosmeticIntent intent) {
        string parent = AnimSpecOf(intent: intent).KeyPath;
        Seq<string> invalid = intent.CoAnimate.IfNone(Seq<CosmeticIntent>())
            .Map(static child => (Intent: child, Path: AnimSpecOf(intent: child).KeyPath))
            .Filter(child => string.Equals(a: child.Path, b: "strokeEnd", comparisonType: StringComparison.Ordinal) && !string.Equals(a: parent, b: "strokeEnd", comparisonType: StringComparison.Ordinal))
            .Map(static child => child.Intent.GetType().Name);
        return guard(invalid.IsEmpty, (Error)UiFault.InvalidInput(
                op: Op.Of(name: nameof(CosmeticIntent.CoAnimate)),
                detail: $"stroke co-animations require a stroke parent layer: {string.Join(separator: ", ", values: invalid.AsIterable())}"))
            .ToFin();
    }

    private static Fin<Unit> AcceptDistinctAnimationKeyPaths(CosmeticIntent intent) {
        Seq<string> paths = Seq(AnimSpecOf(intent: intent).KeyPath) + intent.CoAnimate.IfNone(Seq<CosmeticIntent>()).Map(static child => AnimSpecOf(intent: child).KeyPath);
        Seq<string> duplicates = toSeq(paths.GroupBy(keySelector: static path => path, comparer: StringComparer.Ordinal))
            .Filter(static group => group.Take(2).Count() == 2)
            .Map(static group => group.Key);
        return guard(duplicates.IsEmpty, (Error)UiFault.InvalidInput(
                op: Op.Of(name: nameof(CosmeticIntent.CoAnimate)),
                detail: $"cosmetic co-animation key paths must be distinct: {string.Join(separator: ", ", values: duplicates.AsIterable())}"))
            .ToFin();
    }

    private static Fin<Unit> AcceptGradientPoints(Op op, CosmeticGradientPoints points, int colourCount) =>
        from _ in points.Start.Map(start => AcceptNormalizedGradientPoint(op: op, point: start, label: nameof(CosmeticGradientPoints.Start))).IfNone(Fin.Succ(unit))
        from __ in points.End.Map(end => AcceptNormalizedGradientPoint(op: op, point: end, label: nameof(CosmeticGradientPoints.End))).IfNone(Fin.Succ(unit))
        from ___ in points.Locations.Map(locations => AcceptGradientStops(op: op, locations: locations, colourCount: colourCount)).IfNone(Fin.Succ(unit))
        select unit;

    // CAGradientLayer.Locations must align 1:1 with Colors and ascend monotonically through [0,1].
    private static Fin<Unit> AcceptGradientStops(Op op, ReadOnlyMemory<float> locations, int colourCount) {
        Seq<float> stops = toSeq(MemoryMarshal.ToEnumerable(locations));
        return stops.Count == colourCount
            ? stops
                .Fold(
                    initialState: Fin.Succ(float.NegativeInfinity),
                    f: (acc, stop) => acc.Bind(previous => guard(stop is >= 0f and <= 1f && stop >= previous, (Error)UiFault.InvalidInput(op: op, detail: $"gradient stops must ascend through [0,1] (got {stop:R} after {previous:R})")).ToFin().Map(_ => stop)))
                .Map(static _ => unit)
            : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: op, detail: $"gradient stop count {stops.Count} must equal colour count {colourCount}"));
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

    // CoreText metrics are AppKit-main-thread-bound; unresolved fonts fall back to the prior estimate.
    [SupportedOSPlatform("macos14.0")]
    private static (float Width, float Height) MeasureText(string text, float fontSize, Option<string> family) {
        Option<NSFont> font = Optional(family is { IsSome: true, Case: string name } ? NSFont.FromFontName(name, fontSize) : null)
            is { IsSome: true, Case: NSFont resolved }
            ? Some(resolved)
            : Optional(NSFont.SystemFontOfSize(fontSize));
        return font is { IsSome: true, Case: NSFont measured }
            ? MeasureAttributed(text: text, font: measured)
            : (Width: MathF.Max(1f, 0.62f * fontSize * text.Length), Height: MathF.Max(1f, 1.4f * fontSize));
    }

    [SupportedOSPlatform("macos14.0")]
    private static (float Width, float Height) MeasureAttributed(string text, NSFont font) {
        using NSAttributedString attributed = new(str: text, font: font);
        CGSize size = attributed.Size;
        return (Width: MathF.Max(1f, (float)size.Width), Height: MathF.Max(1f, (float)size.Height));
    }

    private static RectangleF MapCosmeticRect(Grasshopper2.UI.Canvas.Canvas canvas, RectangleF bounds) =>
        canvas.Map(rectangle: bounds, from: CoordinateSystem.Content, to: CoordinateSystem.Control);

    private static ReadOnlyMemory<PointF> MapCosmeticPolyline(Grasshopper2.UI.Canvas.Canvas canvas, ReadOnlyMemory<PointF> polyline) =>
        toSeq(MemoryMarshal.ToEnumerable(polyline))
            .Map(point => canvas.Map(point: point, from: CoordinateSystem.Content, to: CoordinateSystem.Control))
            .ToArray();

    // Haptic is non-visual; every decorative case resolves the host CALayer or returns a typed fault. IsHaptic collapses
    // the twelve identical visual arms into one boolean dispatch (Motion-Q5).
    [SupportedOSPlatform("macos14.0")]
    private static Fin<Unit> CosmeticAttach(NSView view, CosmeticIntent intent, NSString key) {
        Op op = Op.Of(name: nameof(CosmeticAttach));
        Fin<Unit> runCompletion() =>
            Op.Side(() => intent.Completion.IfSome(static run => run())) switch { _ => Fin.Succ(unit) };
        Fin<Unit> visualPath() {
            _ = Op.Side(() => view.WantsLayer = true);
            return Optional(view.Layer).ToFin(Fail: UiFault.MutationRejected(op: op, detail: "host layer absent"))
                .Bind(host => MotionAccessibility.ShouldSkipDecorativeMotion
                    ? runCompletion()
                    : AttachDecorative(host: host, view: view, intent: intent, key: key));
        }
        // IsHaptic is the single discriminant; the typed pattern surfaces the Pattern payload on the haptic arm only.
        return (intent.IsHaptic, intent) switch {
            (true, CosmeticIntent.HapticCase haptic) => Op.Side(() => PerformHaptic(pattern: haptic.Pattern)) switch { _ => runCompletion() },
            _ => visualPath(),
        };
    }

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CAShapeLayer ownership transfers to host CALayer via AddSublayer; disposal happens on Subscription detach or animation completion delegate.")]
    private static Fin<Unit> AttachDecorative(CALayer host, NSView view, CosmeticIntent intent, NSString key) {
        EdrResolution edr = ResolveEdr(view: view, policy: intent.EdrMode);
        CosmeticAttachment attachment = BuildCosmeticLayer(intent: intent, view: view, space: edr.Space);
        CALayer layer = attachment.Layer;
        NFloat scale = Optional(view.Window?.Screen).Map(static screen => screen.BackingScaleFactor).IfNone((NFloat)RetinaScaleDefault);
        layer.ContentsScale = scale;
        // BOUNDARY ADAPTER — EDR flag moved from WantsExtendedDynamicRangeContent to PreferredDynamicRange on macOS 26.
        // ToneMapMode.Automatic is macOS 15+; Disabled keeps the SDR path unchanged.
        // CA1416 false positive: every EDR write IS runtime-gated by OperatingSystem.IsMacOSVersionAtLeast, but
        // the analyzer cannot trace the version guard through the Op.Side action-lambda boundary.
#pragma warning disable CA1416
        _ = OperatingSystem.IsMacOSVersionAtLeast(26)
            ? Op.Side(() => layer.PreferredDynamicRange = edr.Enabled ? CADynamicRange.High : CADynamicRange.Standard)
            : Op.Side(() => layer.WantsExtendedDynamicRangeContent = edr.Enabled);
        _ = edr.Enabled && OperatingSystem.IsMacOSVersionAtLeast(15)
            ? Op.Side(() => layer.ToneMapMode = CAToneMapMode.Automatic)
            : unit;
#pragma warning restore CA1416
        // Fire-and-forget overlays can draw asynchronously; apply ContentsScale to sublayers too.
        layer.DrawsAsynchronously = true;
        _ = (layer.Sublayers is { } sublayers ? toSeq(sublayers) : Seq<CALayer>()).Iter(sub => { sub.ContentsScale = scale; sub.DrawsAsynchronously = true; });
        layer.Name = key.ToString();
        // GPU spring preferred Hz tracks natural frequency; no spring leaves CA default.
        Option<CAFrameRateRange> rate = AnimationRate(view: view, intent: intent);
        return WithoutAnimation(() => {
            host.AddSublayer(layer: layer);
            CAAnimation animation = GroupOf(main: BuildCosmeticAnimation(intent: intent, rate: rate), intent: intent, view: view, rate: rate);
            CosmeticRemove remove = new(layer: layer, key: key, completion: intent.Completion, ownedSubview: attachment.OwnedSubview);
            _ = CosmeticDelegateStore.Retain(layer: layer, remove: remove);
            animation.WeakDelegate = remove;
            layer.AddAnimation(animation: animation, key: key);
        }) switch { _ => Fin.Succ(unit) };
    }

    // Mutual-exclusion claim: TryClaim wins exactly once across explicit-dispose vs AnimationStopped.
    [SupportedOSPlatform("macos14.0")]
    private static Unit CosmeticStrip(NSView view, NSString key) {
        string keyString = key.ToString();
        return (from host in Optional(view.Layer)
                from subs in Optional(host.Sublayers)
                select WithoutAnimation(() => _ = toSeq(subs)
                    .Filter(sub => string.Equals(a: sub.Name, b: keyString, comparisonType: StringComparison.Ordinal))
                    .Filter(sub => CosmeticDelegateStore.Find(layer: sub)
                        .Map(remove => remove.TryClaim())
                        .IfNone(noneValue: true))
                    .Iter(sub => {
                        sub.RemoveAnimation(key: keyString);
                        _ = CosmeticDelegateStore.Find(layer: sub).IfSome(static remove => remove.RemoveOwnedSubview());
                        _ = CosmeticDelegateStore.Release(layer: sub);
                        sub.RemoveFromSuperLayer();
                    })))
            .IfNone(unit);
    }

    private readonly record struct CosmeticAttachment(CALayer Layer, Option<NSView> OwnedSubview = default);

    private readonly record struct EdrResolution(bool Enabled, NSColorSpace? Space);

    private static EdrResolution ResolveEdr(NSView view, EdrPolicy policy) {
        if (policy == EdrPolicy.Disabled) {
            return new(Enabled: false, Space: null);
        }
        NSScreen? screen = view.Window?.Screen ?? NSScreen.MainScreen;
        bool edr = Optional(screen).Map(static s => s.MaximumExtendedDynamicRangeColorComponentValue > 1.0).IfNone(noneValue: false);
        bool p3 = Optional(screen?.ColorSpace).Map(static space =>
            Equals(space, NSColorSpace.DisplayP3ColorSpace)
            || Optional(space.LocalizedName).Map(static name => name.Contains(value: "P3", comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(noneValue: false)).IfNone(noneValue: false);
        return new(
            Enabled: edr,
            Space: edr && p3 ? NSColorSpace.DisplayP3ColorSpace : null);
    }

    [SupportedOSPlatform("macos14.0")]
    private static CosmeticAttachment BuildCosmeticLayer(CosmeticIntent intent, NSView view, NSColorSpace? space) {
        // EDR/P3 opt-in anchors decorative colours in Display P3 (CGColor-tagged via NSColor.FromDisplayP3); Disabled
        // yields the SDR device-sRGB path. CALayer exposes no colour-space setter — the P3 tag rides each CGColor.
        return intent.Switch(
            state: (View: view, Space: space),
            pulseCase: static (s, pulse) => new CosmeticAttachment(Layer: CosmeticPulseLayer(pulse: pulse, space: s.Space)),
            glowCase: static (s, glow) => new CosmeticAttachment(Layer: CosmeticGlowLayer(glow: glow, view: s.View, space: s.Space)),
            strokeOnCase: static (s, stroke) => new CosmeticAttachment(Layer: CosmeticStrokeLayer(stroke: stroke, space: s.Space)),
            gradientCase: static (s, gradient) => new CosmeticAttachment(Layer: CosmeticGradientLayer(gradient: gradient, space: s.Space)),
            textLayerCase: static (s, text) => new CosmeticAttachment(Layer: CosmeticTextLayer(text: text, space: s.Space)),
            replicatorCase: static (s, replicator) => new CosmeticAttachment(Layer: CosmeticReplicatorLayer(replicate: replicator, space: s.Space)),
            emitterCase: static (s, emitter) => new CosmeticAttachment(Layer: CosmeticEmitterLayer(emit: emitter, space: s.Space)),
            snapGuideCase: static (_, guide) => new CosmeticAttachment(Layer: CosmeticSnapGuideLayer(guide)),
            vibrancyCase: static (s, vibrancy) => CosmeticVibrancyLayer(vibrancy: vibrancy, view: s.View),
            filterCase: static (s, filter) => new CosmeticAttachment(Layer: CosmeticFilterLayer(filter: filter, view: s.View, space: s.Space)),
            glyphCase: static (s, glyph) => new CosmeticAttachment(Layer: CosmeticGlyphLayer(glyph: glyph, space: s.Space)),
            pathMorphCase: static (s, morph) => new CosmeticAttachment(Layer: CosmeticPathMorphLayer(morph: morph, space: s.Space)),
            // HapticCase carries no CALayer; CosmeticAttach short-circuits it before BuildCosmeticLayer is reached.
            hapticCase: static (_, _) => new CosmeticAttachment(Layer: new CALayer()));
    }

    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticPulseLayer(CosmeticIntent.PulseCase pulse, NSColorSpace? space) {
        CGRect frame = ToCGRect(pulse.Bounds);
        CGRect local = LocalCGRect(frame: frame);
        CAShapeLayer shape = new() { Frame = frame };
        CGPath path = new();
        path.AddRect(rect: local);
        shape.Path = path;
        shape.FillColor = ToCGColor(c: pulse.Tint, space: space);
        shape.Opacity = 0f;
        return shape;
    }

    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticGlowLayer(CosmeticIntent.GlowCase glow, NSView view, NSColorSpace? space) {
        CGRect rect = ToCGRect(glow.Bounds);
        CGRect local = LocalCGRect(frame: rect);
        CGPath path = new();
        path.AddRoundedRect(transform: CGAffineTransform.MakeIdentity(), rect: local, cornerWidth: glow.CornerRadius, cornerHeight: glow.CornerRadius);
        CAShapeLayer shape = new() {
            Frame = rect,
            Path = path,
            FillColor = null,
            CornerRadius = glow.CornerRadius,
            BorderWidth = glow.BorderWidth,
            ShadowColor = ToCGColor(c: glow.Tint, space: space),
            ShadowRadius = glow.ShadowRadius,
            ShadowOffset = CGSize.Empty,
            ShadowPath = path,
            ShadowOpacity = 0f,
        };
        _ = glow.BorderColor.IfSome(colour => shape.BorderColor = ToCGColor(c: colour, space: space));
        // BlurRadius routes to BackgroundFilters (frosted backdrop, M-MOT-5) when BackdropBlur is set — which also
        // arms LayerUsesCoreImageFilters on the host view once — else to the prior self-content Filters slot.
        _ = Optional(glow.BlurRadius).Filter(static radius => radius > 0f).IfSome(radius =>
            glow.BackdropBlur
                ? Op.Side(() => {
                    view.LayerUsesCoreImageFilters = true;
                    shape.BackgroundFilters = [new CIGaussianBlur { Radius = radius }];
                })
                : Op.Side(() => shape.Filters = [new CIGaussianBlur { Radius = radius }]));
        // CompositingFilter blends the glow against the canvas behind it; CIFilter.FromName is nullable, so the
        // Choose drops an unresolved filter name rather than nulling the property.
        _ = glow.Blend.Bind(mode => Optional(CIFilter.FromName(name: mode.FilterName))).IfSome(filter => shape.CompositingFilter = filter);
        return shape;
    }

    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticStrokeLayer(CosmeticIntent.StrokeOnCase stroke, NSColorSpace? space) {
        Seq<PointF> points = toSeq(MemoryMarshal.ToEnumerable(stroke.Polyline));
        RectangleF frame = PathFrame(points: points).IfNone(RectangleF.Empty);
        CGPath path = new();
        // Fold the polyline into the CGPath: first point opens the subpath (MoveTo), the rest extend it (LineTo).
        _ = points.Fold(initialState: true, f: (first, point) => {
            CGPoint cg = LocalPoint(point: point, frame: frame);
            _ = first ? Op.Side(() => path.MoveToPoint(point: cg)) : Op.Side(() => path.AddLineToPoint(point: cg));
            return false;
        });
        return new CAShapeLayer {
            Frame = ToCGRect(frame),
            Path = path,
            StrokeColor = ToCGColor(c: stroke.Tint, space: space),
            LineWidth = stroke.Thickness,
            StrokeStart = 0f,
            StrokeEnd = 0f,
        };
    }

    // Glyph strokes fold into one local CGPath: gaps lift the pen, run starts open subpaths, and arcs fold to chords.
    // [?] Eto.macOS exposes the native CGPath for curved AppendStroke, but Microsoft.macOS-only does not.
    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticGlyphLayer(CosmeticIntent.GlyphCase glyph, NSColorSpace? space) {
        RectangleF frame = PathFrame(points: glyph.Strokes.Bind(static stroke => Seq(stroke.Point0, stroke.Point1))).IfNone(RectangleF.Empty);
        CGPath path = new();
        _ = glyph.Strokes.Fold(initialState: true, f: (penUp, stroke) => {
            CGPoint p0 = LocalPoint(point: stroke.Point0, frame: frame);
            CGPoint p1 = LocalPoint(point: stroke.Point1, frame: frame);
            _ = stroke is GapStroke
                ? Op.Side(() => path.MoveToPoint(point: p1))
                : penUp
                    ? Op.Side(() => { path.MoveToPoint(point: p0); path.AddLineToPoint(point: p1); })
                    : Op.Side(() => path.AddLineToPoint(point: p1));
            return stroke is GapStroke;
        });
        return new CAShapeLayer {
            Frame = new CGRect(x: glyph.Origin.X, y: glyph.Origin.Y, width: frame.Width, height: frame.Height),
            Path = path,
            StrokeColor = ToCGColor(c: glyph.Tint, space: space),
            FillColor = null,
            LineWidth = glyph.Thickness,
            LineCap = CAShapeLayer.CapRound,
            LineJoin = CAShapeLayer.JoinRound,
            StrokeStart = 0f,
            StrokeEnd = 0f,
        };
    }

    // CGPath morph: the layer is seeded with From; BuildCosmeticAnimation drives the "path" key From -> To via a
    // CGPath-valued CABasicAnimation. Thickness/Tint stroke the morphing outline (FillColor null keeps it an outline).
    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticPathMorphLayer(CosmeticIntent.PathMorphCase morph, NSColorSpace? space) =>
        new() {
            Frame = ToCGRect(morph.Bounds),
            Path = morph.From,
            StrokeColor = ToCGColor(c: morph.Tint, space: space),
            FillColor = null,
            LineWidth = morph.Thickness,
            LineCap = CAShapeLayer.CapRound,
            LineJoin = CAShapeLayer.JoinRound,
        };

    [SupportedOSPlatform("macos14.0")]
    private static CAGradientLayer CosmeticGradientLayer(CosmeticIntent.GradientCase gradient, NSColorSpace? space) {
        (CAGradientLayerType type, _, _) = CosmeticGradientProfile[gradient.Kind.ProfileIndex];
        (CGPoint start, CGPoint end) = ResolveGradientPoints(kind: gradient.Kind, points: gradient.Points);
        CAGradientLayer layer = new() {
            Frame = ToCGRect(gradient.Bounds),
            Colors = [.. gradient.Colors.Map(c => ToCGColor(c: c, space: space))],
            LayerType = type,
            StartPoint = start,
            EndPoint = end,
            Opacity = 0f,
        };
        _ = gradient.Points.Locations.IfSome(locations =>
            layer.Locations = [.. toSeq(MemoryMarshal.ToEnumerable(locations)).Map(static stop => NSNumber.FromFloat(stop))]);
        return layer;
    }

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CGColor/NSString ownership transfers to CALayer property graph for animation lifetime.")]
    private static CATextLayer CosmeticTextLayer(CosmeticIntent.TextLayerCase text, NSColorSpace? space) {
        (float width, float height) = MeasureText(text: text.Text, fontSize: text.FontSize, family: text.FontFamily);
        CATextLayer layer = new() {
            String = new NSString(text.Text),
            ForegroundColor = ToCGColor(c: text.Tint, space: space),
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
        Justification = "CGPath ownership transfers to CAShapeLayer hosted by CAReplicatorLayer; inner replicator ownership transfers to the outer replicator via AddSublayer.")]
    private static CAReplicatorLayer CosmeticReplicatorLayer(CosmeticIntent.ReplicatorCase replicate, NSColorSpace? space) {
        CGRect rect = ToCGRect(replicate.SourceBounds);
        CGRect local = LocalCGRect(frame: rect);
        CAShapeLayer source = new() { Frame = local };
        CGPath path = new();
        path.AddRect(rect: local);
        source.Path = path;
        source.FillColor = ToCGColor(c: replicate.Tint, space: space);
        // Inner row owns delay/alpha/colour offsets; outer row only tiles Y to avoid double-applying grain.
        CAReplicatorLayer inner = new() {
            Frame = rect,
            InstanceCount = replicate.Count,
            InstanceDelay = replicate.InstanceDelay,
            InstanceAlphaOffset = replicate.InstanceAlphaOffset,
            InstanceTransform = CATransform3D
                .MakeTranslation(tx: replicate.Spacing, ty: 0, tz: 0)
                .Concat(b: CATransform3D.MakeRotation(angle: replicate.Rotation, x: 0, y: 0, z: 1)),
        };
        _ = replicate.InstanceColour.IfSome(colour => inner.InstanceColor = ToCGColor(c: colour, space: space));
        inner.AddSublayer(layer: source);
        // CountY>1 nests the opaque inner row under an outer Y-tiling replicator whose own opacity drives the animation;
        // the 1D row drives opacity directly.
        static CAReplicatorLayer Nest(CAReplicatorLayer outer, CAReplicatorLayer row) {
            row.Opacity = 1f;
            outer.AddSublayer(layer: row);
            return outer;
        }
        return replicate.CountY > 1
            ? Nest(
                new CAReplicatorLayer {
                    Frame = rect,
                    InstanceCount = replicate.CountY,
                    InstanceTransform = CATransform3D.MakeTranslation(tx: 0, ty: replicate.SpacingY, tz: 0),
                    Opacity = 0f,
                },
                inner)
            : Op.Side(() => inner.Opacity = 0f) switch { _ => inner };
    }

    [SupportedOSPlatform("macos14.0")]
    private static CAEmitterLayer CosmeticEmitterLayer(CosmeticIntent.EmitterCase emit, NSColorSpace? space) {
        CGRect rect = ToCGRect(emit.Bounds);
        EmitterProfile profile = emit.Profile;
        // Palette creates one tinted cell per colour; emitterPosition still needs KVC while longitude/spin have setters.
        Seq<Color> tints = emit.Palette.Filter(static p => !p.IsEmpty).IfNone(Seq(emit.Tint));
        Seq<CAEmitterCell> cells = tints.Map(tint => {
            CAEmitterCell cell = new() {
                BirthRate = emit.BirthRate,
                LifeTime = emit.Lifetime,
                LifetimeRange = emit.Lifetime * profile.LifetimeRangeRatio,
                Velocity = emit.Velocity,
                VelocityRange = emit.Velocity * profile.VelocityRangeRatio,
                EmissionRange = emit.EmissionRange,
                EmissionLongitude = emit.EmissionLongitude,
                Spin = emit.Spin,
                SpinRange = profile.SpinRange,
                Scale = profile.Scale,
                ScaleRange = profile.Scale * profile.ScaleRangeRatio,
                ScaleSpeed = profile.ScaleSpeed,
                AlphaSpeed = profile.AlphaSpeed,
                AccelerationX = profile.AccelerationX,
                AccelerationY = profile.AccelerationY,
                AccelerationZ = profile.AccelerationZ,
                Color = ToCGColor(c: tint, space: space),
                RedRange = profile.ColorRange,
                GreenRange = profile.ColorRange,
                BlueRange = profile.ColorRange,
            };
            return cell;
        });
        CAEmitterLayer layer = new() {
            Frame = rect,
            Shape = emit.Shape.Resolve(),
            Mode = CAEmitterLayer.ModeSurface,
            RenderMode = profile.Render.Resolve(),
            Size = new CGSize(width: rect.Width, height: rect.Height),
            Cells = [.. cells],
            Opacity = 0f,
        };
        // emitterPosition has no managed setter; centre it in the layer via KVC so particles spawn across Bounds.
        layer.SetValueForKey(value: NSValue.FromCGPoint(new CGPoint(x: rect.Width / 2.0, y: rect.Height / 2.0)), key: (NSString)"emitterPosition");
        return layer;
    }

    // BOUNDARY ADAPTER — NSVisualEffectView must be a child view, not a CALayer, to sample content behind the canvas.
    // Its backing layer participates in the shared opacity/strip machinery.
    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "NSVisualEffectView ownership transfers to the host NSView via AddSubview; the mask CAShapeLayer transfers to the effect view's layer. Teardown removes the layer (and its host subview) on Subscription detach or completion.")]
    private static CosmeticAttachment CosmeticVibrancyLayer(CosmeticIntent.VibrancyCase vibrancy, NSView view) {
        CGRect rect = ToCGRect(vibrancy.Bounds);
        NSVisualEffectView effect = new(frameRect: rect) {
            Material = vibrancy.Material.Native,
            BlendingMode = vibrancy.Material.Blending,
            State = vibrancy.State,
            WantsLayer = true,
        };
        CGRect local = LocalCGRect(frame: rect);
        CGPath maskPath = new();
        maskPath.AddRoundedRect(transform: CGAffineTransform.MakeIdentity(), rect: local, cornerWidth: vibrancy.CornerRadius, cornerHeight: vibrancy.CornerRadius);
        view.AddSubview(effect);
        CALayer layer = effect.Layer!;
        // Rounded clip via the backing-layer mask (NSVisualEffectView has no managed MaskView; the layer mask is the
        // CALayer-native clip path). MasksToBounds + CornerRadius give the AA-correct corner; the shape mask clips blur.
        layer.Mask = new CAShapeLayer { Path = maskPath, Frame = local };
        layer.MasksToBounds = true;
        layer.CornerRadius = vibrancy.CornerRadius;
        layer.Opacity = 0f;
        return new CosmeticAttachment(Layer: layer, OwnedSubview: Some<NSView>(effect));
    }

    // BOUNDARY ADAPTER — Backdrop uses BackgroundFilters and arms host filters; false uses layer Filters.
    // CIColorControls appends Saturation/Brightness, and unresolved filters drop on the rail.
    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CIFilter array ownership transfers to the CALayer Filters/BackgroundFilters property graph for the animation lifetime.")]
    private static CALayer CosmeticFilterLayer(CosmeticIntent.FilterCase filter, NSView view, NSColorSpace? space) {
        CGRect rect = ToCGRect(filter.Bounds);
        CIColorControls colour = new() { Saturation = filter.Saturation, Brightness = filter.Brightness, Contrast = 1f };
        // Under EDR (space != null) append a headroom tone-map so wide-gamut filter output stays HDR-stable; null-safe.
        Seq<CIFilter> pipeline = filter.Pipeline.Map(static source => FreshFilter(filter: source)) + Seq<CIFilter>(colour)
            + (space is not null ? Optional(CIFilter.FromName(name: "CIToneMapHeadroom")).ToSeq() : Seq<CIFilter>());
        CALayer layer = new() {
            Frame = rect,
            BackgroundColor = ToCGColor(c: filter.Tint, space: space),
            Opacity = 0f,
        };
        _ = filter.Backdrop
            ? Op.Side(() => {
                view.LayerUsesCoreImageFilters = true;
                layer.BackgroundFilters = [.. pipeline];
            })
            : Op.Side(() => layer.Filters = [.. pipeline]);
        return layer;
    }

    private static CIFilter FreshFilter(CIFilter filter) =>
        filter.Copy() as CIFilter ?? Optional(filter.Name).Bind(static name => Optional(CIFilter.FromName(name: name))).IfNone(filter);

    // Layout.Snap output is already control-space; render guide lines plus optional distance labels.
    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer CosmeticSnapGuideLayer(CosmeticIntent.SnapGuideCase guide) {
        // Vertical guide segments use XStyle, horizontal segments use YStyle; parent opacity drives shared fade.
        SnapGuideStyle xStyle = guide.Style;
        SnapGuideStyle yStyle = guide.YStyle.IfNone(guide.Style);
        Seq<SnapLabel> labels = Seq(guide.Snapshot.XLabel, guide.Snapshot.YLabel).Somes();
        Seq<PointF> points = guide.Snapshot.Lines.Bind(static line => Seq(line.Start, line.End)) + labels.Map(static label => label.Point);
        RectangleF frame = PathFrame(points: points).IfNone(RectangleF.Empty);
        Seq<(bool IsX, LineF Line)> axisLines = guide.Snapshot.Lines.Map(static line => (IsX: MathF.Abs(line.Start.X - line.End.X) <= MotionVector.Float.RestEpsilon, Line: line));
        CAShapeLayer shape = new() { Frame = ToCGRect(frame), FillColor = null, Opacity = 0f };
        // One stroke sublayer per axis; the parent stays path-less so parent opacity is the only animated alpha.
        _ = Seq((IsX: true, Style: xStyle), (IsX: false, Style: yStyle))
            .Choose(axis => axisLines.Filter(entry => entry.IsX == axis.IsX).Map(static entry => entry.Line) is { IsEmpty: false } lines
                ? Some(BuildAxisStroke(axis: lines, style: axis.Style, frame: frame))
                : Option<CAShapeLayer>.None)
            .Iter(shape.AddSublayer);
        // X/Y labels stay independent; Anchor drives alignment and frame-origin shift.
        _ = Seq((Label: guide.Snapshot.XLabel, Style: xStyle), (Label: guide.Snapshot.YLabel, Style: yStyle))
            .Map(channel => channel.Label.Map(label => (Label: label, channel.Style)))
            .Somes()
            .Iter(channel => {
                SnapGuideStyle style = channel.Style;
                SnapLabel label = channel.Label;
                string text = label.Text.Length > 0
                    ? label.Text
                    : guide.Snapshot.Magnitude.ToString(format: style.MagnitudeFormat, provider: CultureInfo.InvariantCulture);
                (float width, float height) = MeasureText(text: text, fontSize: style.LabelFontSize, family: Option<string>.None);
                (CATextLayerAlignmentMode mode, float dx, float dy) = SnapLabelPlacement(anchor: label.Anchor, width: width, height: height);
                CGPoint local = LocalPoint(point: label.Point, frame: frame);
                shape.AddSublayer(layer: new CATextLayer {
                    String = new NSString(text),
                    ForegroundColor = ToCGColor(style.Tint),
                    FontSize = style.LabelFontSize,
                    TextAlignmentMode = mode,
                    Frame = new CGRect(x: local.X + dx, y: local.Y + dy, width: width, height: height),
                });
            });
        return shape;
    }

    // Empty dashes render solid; non-empty dashes use square caps, round joins, and optional marching animation.
    // BOUNDARY ADAPTER — the parent snap-guide layer owns returned sublayers until detach/completion.
    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer BuildAxisStroke(Seq<LineF> axis, SnapGuideStyle style, RectangleF frame) {
        CGPath path = new();
        _ = axis.Iter(line => {
            path.MoveToPoint(point: LocalPoint(point: line.Start, frame: frame));
            path.AddLineToPoint(point: LocalPoint(point: line.End, frame: frame));
        });
        CAShapeLayer stroke = new() {
            Frame = ToCGRect(frame),
            Path = path,
            StrokeColor = ToCGColor(style.Tint),
            FillColor = null,
            LineWidth = style.Thickness,
            LineDashPhase = style.DashPhase,
            LineCap = CAShapeLayer.CapSquare,
            LineJoin = CAShapeLayer.JoinRound,
        };
        Seq<float> dashes = toSeq(MemoryMarshal.ToEnumerable(style.Dashes));
        float dashPeriod = dashes.Fold(0f, static (sum, d) => sum + d);
        _ = dashes.IsEmpty
            ? unit
            : Op.Side(() => stroke.LineDashPattern = [.. dashes.Map(static d => NSNumber.FromFloat(d))]);
        // Marching ants: an infinite lineDashPhase animation crawling one full DashedPattern period.
        _ = style.Marching && !dashes.IsEmpty
            ? Op.Side(() => {
                CABasicAnimation march = CABasicAnimation.FromKeyPath(path: "lineDashPhase");
                march.From = NSNumber.FromFloat(style.DashPhase);
                march.To = NSNumber.FromFloat(style.DashPhase + dashPeriod);
                march.Duration = Animators.DurationToTimeSpan(duration: style.Duration).TotalSeconds;
                march.RepeatCount = float.PositiveInfinity;
                stroke.AddAnimation(animation: march, key: "rasm.snap.march");
            })
            : unit;
        return stroke;
    }

    // TextAnchor maps the label anchor corner to LabelPoint; no anchor means top-left at origin.
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

    // Reduce-motion short-circuits upstream; plain curves map to CAMediaTimingFunction.
    // Rich curves and manual Keyframe use sampled CAKeyFrameAnimation over [0,1].
    [SupportedOSPlatform("macos14.0")]
    private static CAPropertyAnimation PropertyAnimation(string path, GhDuration duration, float from, float to, GhMotion easing, Option<Easing> keyframe, Option<SpringConfig> spring, Option<float> initialVelocity, Option<CAFrameRateRange> rate) {
        double seconds = Animators.DurationToTimeSpan(duration: duration).TotalSeconds;
        // Spring wins, then a manual Keyframe curve; otherwise the four media-timing-mappable easings ride the GPU
        // BasicAnimation and every richer Penner curve falls back to a sampled keyframe over [0,1].
        CAPropertyAnimation animation = (spring, keyframe, easing) switch {
            ( { IsSome: true, Case: SpringConfig config }, _, _) => SpringAnimation(path: path, from: from, to: to, config: config, initialVelocity: initialVelocity.IfNone(0f)),
            (_, { IsSome: true, Case: Easing curve }, _) => SampleKeyframe(path: path, duration: seconds, from: from, to: to, curve: curve.Apply),
            (_, _, GhMotion.Linear or GhMotion.EaseIn or GhMotion.EaseOut or GhMotion.EaseInOut) => BasicAnimation(path: path, duration: duration, from: from, to: to, easing: easing),
            _ => SampleKeyframe(path: path, duration: seconds, from: from, to: to, curve: t => MotionEquations.Blend(motion: easing, parameter: t)),
        };
        _ = rate.IfSome(range => animation.PreferredFrameRateRange = range);
        return animation;
    }

    // CAFrameRateRange tracks the panel ceiling; preferred Hz is ~4x spring natural frequency, clamped to floor/max.
    [SupportedOSPlatform("macos14.0")]
    private static CAFrameRateRange ResolveRange(NSScreen screen, SpringConfig spring) {
        double minimumInterval = screen.MinimumRefreshInterval;
        float maximum = minimumInterval > 0d ? (float)(1.0 / minimumInterval) : FallbackRefreshHz;
        float naturalHz = MathF.Sqrt(spring.Stiffness / spring.Mass) / MathF.Tau;
        float preferred = Math.Clamp(value: naturalHz * 4f, min: FloorRefreshHz, max: maximum);
        return CAFrameRateRange.Create(minimum: FloorRefreshHz, maximum: maximum, preferred: preferred);
    }

    [SupportedOSPlatform("macos14.0")]
    private static CASpringAnimation SpringAnimation(string path, float from, float to, SpringConfig config, float initialVelocity) {
        CASpringAnimation anim = new() {
            KeyPath = path,
            Mass = config.Mass,
            Stiffness = config.Stiffness,
            Damping = config.Damping,
            // Seeded from CosmeticIntent.SpringInitialVelocity (default 0 => rest); a fling hand-off launches the GPU layer
            // with the CPU runner's live velocity.
            InitialVelocity = initialVelocity,
        };
        anim.Duration = anim.SettlingDuration;
        anim.SetFrom(value: NSNumber.FromFloat(from));
        anim.SetTo(value: NSNumber.FromFloat(to));
        return anim;
    }

    // KeyTimes stay unset so Core Animation spreads sampled curve values uniformly across [0,1].
    [SupportedOSPlatform("macos14.0")]
    private static CAKeyFrameAnimation SampleKeyframe(string path, double duration, double from, double to, Func<double, double> curve) {
        CAKeyFrameAnimation anim = CAKeyFrameAnimation.GetFromKeyPath(path: path);
        anim.Duration = duration;
        anim.CalculationMode = CAAnimation.AnimationLinear;
        anim.Values = [.. toSeq(Enumerable.Range(start: 0, count: KeyframeSamples + 1)).Map(frame =>
            (NSObject)NSNumber.FromFloat((float)(from + ((to - from) * curve((double)frame / KeyframeSamples)))))];
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
    private static T ApplyRepeat<T>(T animation, RepeatPolicy repeat) where T : CAAnimation {
        animation.RepeatCount = repeat.Infinite ? float.PositiveInfinity : repeat.Count;
        animation.AutoReverses = repeat.Reverses;
        // Hold the terminal frame so a finite repeat does not snap the layer property back to its model value.
        animation.FillMode = CAFillMode.Forwards.ToString();
        return animation;
    }

    // CoAnimate groups same-layer child animations; one upstream CosmeticRemove owns teardown.
    [SupportedOSPlatform("macos14.0")]
    private static CAAnimation GroupOf(CAAnimation main, CosmeticIntent intent, NSView view, Option<CAFrameRateRange> rate) =>
        intent.CoAnimate is { IsSome: true, Case: Seq<CosmeticIntent> children } && !children.IsEmpty
            ? GroupAnimations(main: main, children: children, view: view, parentRate: rate)
            : main;

    [SupportedOSPlatform("macos14.0")]
    private static CAAnimationGroup GroupAnimations(CAAnimation main, Seq<CosmeticIntent> children, NSView view, Option<CAFrameRateRange> parentRate) {
        Seq<(CosmeticIntent Intent, Option<CAFrameRateRange> Rate)> childRates = children.Map(child => (Intent: child, Rate: AnimationRate(view: view, intent: child)));
        Seq<CAAnimation> all = Seq(main) + childRates.Map(child => (CAAnimation)BuildCosmeticAnimation(intent: child.Intent, rate: child.Rate));
        bool infinite = all.Exists(static animation => animation.RepeatCount == float.PositiveInfinity);
        Seq<CAAnimation> finite = all.Filter(static a => a.RepeatCount != float.PositiveInfinity);
        CAAnimationGroup group = new() {
            Animations = [.. all],
            Duration = infinite
                ? finite.Fold(0.0, static (m, a) => Math.Max(m, a.Duration))
                : all.Fold(0.0, static (m, a) => Math.Max(m, EffectiveDuration(animation: a))),
            RepeatCount = infinite ? float.PositiveInfinity : 0f,
            RemovedOnCompletion = false,
            FillMode = CAFillMode.Forwards.ToString(),
        };
        _ = MergeRates(ranges: parentRate.ToSeq() + childRates.Bind(static child => child.Rate.ToSeq()))
            .IfSome(range => group.PreferredFrameRateRange = range);
        return group;
    }

    [SupportedOSPlatform("macos14.0")]
    private static Option<CAFrameRateRange> AnimationRate(NSView view, CosmeticIntent intent) =>
        intent.Spring.Bind(config => Optional(view.Window?.Screen).Map(screen => ResolveRange(screen: screen, spring: config)));

    [SupportedOSPlatform("macos14.0")]
    private static Option<CAFrameRateRange> MergeRates(Seq<CAFrameRateRange> ranges) =>
        ranges.Head.Map(first => {
            Seq<CAFrameRateRange> all = ranges.Tail;
            return all.Fold(
                initialState: first,
                f: static (acc, range) => CAFrameRateRange.Create(
                    minimum: MathF.Min(acc.Minimum, range.Minimum),
                    maximum: MathF.Max(acc.Maximum, range.Maximum),
                    preferred: MathF.Max(acc.Preferred, range.Preferred)));
        });

    [SupportedOSPlatform("macos14.0")]
    private static double EffectiveDuration(CAAnimation animation) =>
        animation.RepeatCount == float.PositiveInfinity
            ? double.PositiveInfinity
            : animation.Duration * Math.Max(val1: 1d, val2: animation.RepeatCount <= 0f ? 1d : animation.RepeatCount) * (animation.AutoReverses ? 2d : 1d);

    // AnimSpecOf drives one PropertyAnimation; Keyframe/Spring preserve per-case curve and GPU-spring opt-ins.
    [SupportedOSPlatform("macos14.0")]
    private static CAPropertyAnimation BuildCosmeticAnimation(CosmeticIntent intent, Option<CAFrameRateRange> rate) {
        CosmeticAnimSpec spec = AnimSpecOf(intent: intent);
        // PathMorphCase routes CGPath endpoints to a CGPath-valued "path" animation; others use the float spec.
        CAPropertyAnimation animation = intent is CosmeticIntent.PathMorphCase morph
            ? PathAnimation(morph: morph, rate: rate)
            : PropertyAnimation(path: spec.KeyPath, duration: spec.Duration, from: 0f, to: spec.To, easing: spec.Easing, keyframe: intent.Keyframe, spring: intent.Spring, initialVelocity: intent.SpringInitialVelocity, rate: rate);
        return spec.Repeating ? ApplyRepeat(animation: animation, repeat: spec.Repeat) : animation;
    }

    // CGPath morph uses the "path" key; GroupOf/AttachDecorative own false-on-completion teardown.
    [SupportedOSPlatform("macos14.0")]
    private static CABasicAnimation PathAnimation(CosmeticIntent.PathMorphCase morph, Option<CAFrameRateRange> rate) {
        CABasicAnimation anim = CABasicAnimation.FromKeyPath(path: "path");
        anim.Duration = Animators.DurationToTimeSpan(duration: morph.Duration).TotalSeconds;
        anim.TimingFunction = CAMediaTimingFunction.FromName(name: TimingName(easing: morph.Easing));
        anim.SetFrom(value: morph.From);
        anim.SetTo(value: morph.To);
        _ = rate.IfSome(range => anim.PreferredFrameRateRange = range);
        return anim;
    }

    private static CGRect ToCGRect(RectangleF r) => new(x: r.X, y: r.Y, width: r.Width, height: r.Height);

    private static CGRect LocalCGRect(CGRect frame) => new(x: 0, y: 0, width: frame.Width, height: frame.Height);

    private static CGPoint ToCGPoint(PointF p) => new(x: p.X, y: p.Y);

    private static CGPoint LocalPoint(PointF point, RectangleF frame) => new(x: point.X - frame.X, y: point.Y - frame.Y);

    private static Option<RectangleF> PathFrame(Seq<PointF> points) =>
        points.Head.Map(first => points.Tail.Fold(
            initialState: new RectangleF(x: first.X, y: first.Y, width: 0f, height: 0f),
            f: static (bounds, point) => RectangleF.Union(
                rect1: bounds,
                rect2: new RectangleF(x: point.X, y: point.Y, width: 0f, height: 0f))));

    // Null space keeps device-sRGB; EDR/P3 space preserves out-of-sRGB values for the compositor.
    private static CGColor ToCGColor(Color c, NSColorSpace? space = null) =>
        space is null
            ? new CGColor(red: c.R, green: c.G, blue: c.B, alpha: c.A)
            : NSColor.FromDisplayP3(red: c.R, green: c.G, blue: c.B, alpha: c.A).CGColor;

    // Dispose commits the disabled-actions CATransaction so WithoutAnimation can be a using-scope.
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

    // BOUNDARY ADAPTER — reduce-motion suppresses Force Touch pulses; missing haptic hardware is a no-op.
    [SupportedOSPlatform("macos14.0")]
    private static Unit PerformHaptic(HapticPattern pattern) =>
        MotionAccessibility.ShouldReduceMotion
            ? unit
            : Optional(NSHapticFeedbackManager.DefaultPerformer).Map(performer =>
                Op.Side(() => performer.PerformFeedback(
                    pattern: pattern.ToNative(),
                    performanceTime: pattern.Time))).IfNone(unit);

    // Exchange-on-stripped is symmetric with CosmeticStrip.TryClaim — whichever path wins owns the strip.
    [SupportedOSPlatform("macos14.0")]
    private sealed class CosmeticRemove : CAAnimationDelegate {
        private readonly CALayer layer;
        private readonly string animationKey;
        private readonly Option<Func<Unit>> completion;
        private readonly Option<NSView> ownedSubview;
        private int stripped;

        internal CosmeticRemove(CALayer layer, NSString key, Option<Func<Unit>> completion, Option<NSView> ownedSubview = default) {
            this.layer = layer;
            animationKey = key.ToString();
            this.completion = completion;
            this.ownedSubview = ownedSubview;
        }

        public override void AnimationStopped(CAAnimation animation, bool finished) {
            if (Interlocked.Exchange(ref stripped, 1) == 1) {
                return;
            }
            _ = WithoutAnimation(() => {
                layer.RemoveAnimation(key: animationKey);
                _ = CosmeticDelegateStore.Release(layer: layer);
                layer.RemoveFromSuperLayer();
                _ = RemoveOwnedSubview();
            });
            // Completion is a natural-end contract: an explicit dispose (RemoveAnimation) stops the animation with
            // finished == false, so the layer always strips but the completion delegate fires only on true end.
            _ = finished ? completion.IfSome(run => run()) : unit;
        }

        internal bool TryClaim() => Interlocked.Exchange(ref stripped, 1) == 0;

        internal Unit RemoveOwnedSubview() =>
            ownedSubview.IfSome(static view => view.RemoveFromSuperview());
    }

    // One delegate per CALayer is the invariant; ConditionalWeakTable keys on layer identity (not a hash) so
    // GetHashCode collisions can no longer cross-link sublayers, and a released layer's delegate is GC-reclaimable.
    private static class CosmeticDelegateStore {
        private static readonly ConditionalWeakTable<CALayer, CosmeticRemove> Store = [];

        // BOUNDARY ADAPTER — ConditionalWeakTable out-param
        internal static Option<CosmeticRemove> Find(CALayer layer) =>
            Store.TryGetValue(key: layer, value: out CosmeticRemove? remove) ? Optional(remove) : None;

        internal static Unit Retain(CALayer layer, CosmeticRemove remove) {
            Store.AddOrUpdate(key: layer, value: remove);
            return unit;
        }

        internal static Unit Release(CALayer layer) {
            _ = Store.Remove(key: layer);
            return unit;
        }
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
            stepCell = live => live.Advance(frameDeltaSeconds: pendingDt);
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
            const float fallback = 1f / DefaultRefreshHz;
            if (pacer is not { IsSome: true, Case: Pacer paced }) {
                return 0f;
            }
            double duration = paced.LastDuration;
            double frame = double.IsFinite(duration) && duration > 0d ? duration : fallback;
            double timestamp = paced.LastTargetTimestamp;
            if (!double.IsFinite(timestamp) || timestamp <= 0d) {
                return (float)frame;
            }
            double delta = lastVsyncTimestamp > 0d ? timestamp - lastVsyncTimestamp : frame;
            lastVsyncTimestamp = timestamp;
            double clamped = double.IsFinite(delta) && delta > 0d ? Math.Min(delta, frame * 3d) : frame;
            return (float)clamped;
        }

        // Pacer Resume only on 0→1 edge; ScheduleRedraw fallback relies on Eto coalescing.
        private Unit Wake() =>
            pacer is { IsSome: true, Case: Pacer p }
                ? Interlocked.Exchange(ref wantingTicks, 1) == 0
                    ? (lastVsyncTimestamp = 0d, p.Resume()).Item2
                    : unit
                : Op.Side(canvas.ScheduleRedraw);

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
        private static NSObject? screenChangeObserver;

        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly NSView view;
        private readonly CAFrameRateRange? explicitRate;
        // BOUNDARY ADAPTER — view-vended CADisplayLink is owns:false; teardown is Invalidate(), never Dispose.
        [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed",
            Justification = "view-vended CADisplayLink (owns:false) is invalidated, never disposed; disposing double-frees the run-loop handle.")]
        private CADisplayLink link;
        private double lastFrameTimestamp;
        private double lastTargetTimestamp;
        private double lastDuration;
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
                screenChangeObserver = NSApplication.Notifications.ObserveDidChangeScreenParameters(static (_, _) => OnScreenParametersChanged());
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

        // NSScreen.MinimumRefreshInterval is ProMotion adaptive-sync aware; 1/interval is the actual panel ceiling (60/120/144Hz vary).
        private static CAFrameRateRange ResolveRange(NSView view, CAFrameRateRange? explicitRate) {
            if (explicitRate is CAFrameRateRange supplied) {
                return supplied;
            }
            float maximum = view.Window?.Screen is NSScreen active && active.MinimumRefreshInterval > 0d
                ? (float)(1.0 / active.MinimumRefreshInterval)
                : FallbackRefreshHz;
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

        // Exchange the live link first so Resume/Pause race onto the replacement; old links are invalidated only.
        private void RebindLink() {
            CADisplayLink replacement = BindLink(view: view, range: ResolveRange(view: view, explicitRate: explicitRate));
            CADisplayLink previous = Interlocked.Exchange(ref link, replacement);
            Volatile.Write(ref lastFrameTimestamp, 0d);
            Volatile.Write(ref lastTargetTimestamp, 0d);
            Volatile.Write(ref lastDuration, 0d);
            replacement.Paused = Volatile.Read(ref active) <= 0;
            previous.Invalidate();
        }

        // TargetTimestamp deltas integrate present-to-present, removing one ProMotion frame of latency.
        // Volatile fences AArch64 reordering; 64-bit double is atomic on Apple Silicon.
        [Export("tick:")]
        public void Tick(CADisplayLink sender) {
            Volatile.Write(ref lastFrameTimestamp, sender.Timestamp);
            Volatile.Write(ref lastTargetTimestamp, sender.TargetTimestamp);
            Volatile.Write(ref lastDuration, sender.Duration);
            canvas.ScheduleRedraw();
        }

        internal double LastFrameTimestamp => Volatile.Read(ref lastFrameTimestamp);
        internal double LastTargetTimestamp => Volatile.Read(ref lastTargetTimestamp);
        internal double LastDuration => Volatile.Read(ref lastDuration);

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

}
