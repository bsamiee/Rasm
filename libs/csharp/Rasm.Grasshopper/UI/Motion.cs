using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using AppKit;
using CoreAnimation;
using CoreGraphics;
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
using Op = Rasm.Domain.Op;
using ZoomThreshold = Grasshopper2.UI.ZoomThreshold;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
internal static class MotionAccessibility {
    internal static bool ShouldReduceMotion =>
        NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceMotion;

    internal static bool ShouldIncreaseContrast =>
        NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldIncreaseContrast;

    internal static bool ShouldDifferentiateWithoutColor =>
        NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldDifferentiateWithoutColor;

    internal static bool ShouldSkipDecorativeMotion =>
        ShouldReduceMotion || ShouldDifferentiateWithoutColor;
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

// Fire-and-forget GPU chrome on CALayer; no mid-flight retarget — dispose and re-issue to change bounds/tint/path.
[SkipUnionOps]
[Union]
public partial record CosmeticIntent {
    private CosmeticIntent() { }
    public sealed record PulseCase(RectangleF Bounds, Color Tint, GhDuration Duration) : CosmeticIntent;
    public sealed record GlowCase(RectangleF Bounds, Color Tint, float CornerRadius, float ShadowRadius, GhDuration Duration) : CosmeticIntent;
    public sealed record StrokeOnCase(ReadOnlyMemory<PointF> Polyline, Color Tint, float Thickness, GhDuration Duration) : CosmeticIntent;
}

public interface IMotionVector<T> {
    public T Zero { get; }
    public float RestEpsilon { get; }
    public T Add(T a, T b);
    public T Subtract(T a, T b);
    public T Scale(T value, float scalar);
    public float Norm(T value);
    public T Interpolate(T value0, T value1, double factor);
}

internal interface IMotionState<TSelf> where TSelf : struct, IMotionState<TSelf> {
    public TSelf Step(float frameDeltaSeconds);
    public Unit Emit();
    public bool IsActive { get; }
    public TSelf MergeInto(TSelf live);
}

// Robert Penner closed-form curves (easings.net).
[SmartEnum<int>]
public sealed partial class Easing {
    private const double BackC1 = 1.70158;
    private const double BackC3 = BackC1 + 1.0;
    private const double BackC2 = BackC1 * 1.525;
    private const double BounceN1 = 7.5625;
    private const double BounceD1 = 2.75;

    private static readonly GhMotion[] NativeCatalog = [
        GhMotion.Linear, GhMotion.LinearDelayed, GhMotion.EaseIn, GhMotion.EaseInDelayed,
        GhMotion.EaseOut, GhMotion.EaseOutDelayed, GhMotion.EaseInOut, GhMotion.EaseInOutDelayed,
        GhMotion.SnapIn, GhMotion.SnapInDelayed, GhMotion.SnapOut, GhMotion.SnapOutDelayed,
        GhMotion.Bounce, GhMotion.BounceDelayed, GhMotion.Twang, GhMotion.TwangDelayed,
    ];

    public static readonly Easing Linear = Native(key: 0, index: 0), LinearDelayed = Native(key: 1, index: 1), EaseIn = Native(key: 2, index: 2), EaseInDelayed = Native(key: 3, index: 3);
    public static readonly Easing EaseOut = Native(key: 4, index: 4), EaseOutDelayed = Native(key: 5, index: 5), EaseInOut = Native(key: 6, index: 6), EaseInOutDelayed = Native(key: 7, index: 7);
    public static readonly Easing SnapIn = Native(key: 8, index: 8), SnapInDelayed = Native(key: 9, index: 9), SnapOut = Native(key: 10, index: 10), SnapOutDelayed = Native(key: 11, index: 11);
    public static readonly Easing Bounce = Native(key: 12, index: 12), BounceDelayed = Native(key: 13, index: 13), Twang = Native(key: 14, index: 14), TwangDelayed = Native(key: 15, index: 15);
    public static readonly Easing SineIn = Closed(key: 16, curve: PennerCurve.SineIn), SineOut = Closed(key: 17, curve: PennerCurve.SineOut), SineInOut = Closed(key: 18, curve: PennerCurve.SineInOut);
    public static readonly Easing QuadIn = Closed(key: 19, curve: PennerCurve.QuadIn), QuadOut = Closed(key: 20, curve: PennerCurve.QuadOut), QuadInOut = Closed(key: 21, curve: PennerCurve.QuadInOut);
    public static readonly Easing CubicIn = Closed(key: 22, curve: PennerCurve.CubicIn), CubicOut = Closed(key: 23, curve: PennerCurve.CubicOut), CubicInOut = Closed(key: 24, curve: PennerCurve.CubicInOut);
    public static readonly Easing QuartIn = Closed(key: 25, curve: PennerCurve.QuartIn), QuartOut = Closed(key: 26, curve: PennerCurve.QuartOut), QuartInOut = Closed(key: 27, curve: PennerCurve.QuartInOut);
    public static readonly Easing QuintIn = Closed(key: 28, curve: PennerCurve.QuintIn), QuintOut = Closed(key: 29, curve: PennerCurve.QuintOut), QuintInOut = Closed(key: 30, curve: PennerCurve.QuintInOut);
    public static readonly Easing ExpoIn = Closed(key: 31, curve: PennerCurve.ExpoIn), ExpoOut = Closed(key: 32, curve: PennerCurve.ExpoOut), ExpoInOut = Closed(key: 33, curve: PennerCurve.ExpoInOut);
    public static readonly Easing CircIn = Closed(key: 34, curve: PennerCurve.CircIn), CircOut = Closed(key: 35, curve: PennerCurve.CircOut), CircInOut = Closed(key: 36, curve: PennerCurve.CircInOut);
    public static readonly Easing BackIn = Closed(key: 37, curve: PennerCurve.BackIn), BackOut = Closed(key: 38, curve: PennerCurve.BackOut), BackInOut = Closed(key: 39, curve: PennerCurve.BackInOut);
    public static readonly Easing ElasticIn = Closed(key: 40, curve: PennerCurve.ElasticIn), ElasticOut = Closed(key: 41, curve: PennerCurve.ElasticOut), ElasticInOut = Closed(key: 42, curve: PennerCurve.ElasticInOut);
    public static readonly Easing BounceIn = Closed(key: 43, curve: PennerCurve.BounceIn), BounceOut = Closed(key: 44, curve: PennerCurve.BounceOut), BounceInOut = Closed(key: 45, curve: PennerCurve.BounceInOut);

    [UseDelegateFromConstructor]
    public partial double Apply(double t);

    private enum PennerCurve : byte {
        SineIn, SineOut, SineInOut, QuadIn, QuadOut, QuadInOut, CubicIn, CubicOut, CubicInOut,
        QuartIn, QuartOut, QuartInOut, QuintIn, QuintOut, QuintInOut, ExpoIn, ExpoOut, ExpoInOut,
        CircIn, CircOut, CircInOut, BackIn, BackOut, BackInOut, ElasticIn, ElasticOut, ElasticInOut,
        BounceIn, BounceOut, BounceInOut,
    }

    private static Easing Native(int key, int index) =>
        new(key: key, apply: t => MotionEquations.Blend(motion: NativeCatalog[index], parameter: t));

    private static Easing Closed(int key, PennerCurve curve) =>
        new(key: key, apply: t => PennerCatalog[(int)curve](t));

    private static readonly Func<double, double>[] PennerCatalog = [
        static t => 1.0 - Math.Cos(t * Math.PI / 2.0),
        static t => Math.Sin(t * Math.PI / 2.0),
        static t => -(Math.Cos(Math.PI * t) - 1.0) / 2.0,
        static t => t * t,
        static t => 1.0 - ((1.0 - t) * (1.0 - t)),
        static t => t < 0.5 ? 2.0 * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 2.0) / 2.0),
        static t => t * t * t,
        static t => 1.0 - Math.Pow(1.0 - t, 3.0),
        static t => t < 0.5 ? 4.0 * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 3.0) / 2.0),
        static t => t * t * t * t,
        static t => 1.0 - Math.Pow(1.0 - t, 4.0),
        static t => t < 0.5 ? 8.0 * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 4.0) / 2.0),
        static t => t * t * t * t * t,
        static t => 1.0 - Math.Pow(1.0 - t, 5.0),
        static t => t < 0.5 ? 16.0 * t * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 5.0) / 2.0),
        static t => t == 0.0 ? 0.0 : Math.Pow(2.0, (10.0 * t) - 10.0),
        static t => t == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * t),
        static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : t < 0.5 ? Math.Pow(2.0, (20.0 * t) - 10.0) / 2.0 : (2.0 - Math.Pow(2.0, (-20.0 * t) + 10.0)) / 2.0,
        static t => 1.0 - Math.Sqrt(1.0 - (t * t)),
        static t => Math.Sqrt(1.0 - ((t - 1.0) * (t - 1.0))),
        static t => t < 0.5 ? (1.0 - Math.Sqrt(1.0 - (4.0 * t * t))) / 2.0 : (Math.Sqrt(1.0 - (((-2.0 * t) + 2.0) * ((-2.0 * t) + 2.0))) + 1.0) / 2.0,
        static t => (BackC3 * t * t * t) - (BackC1 * t * t),
        static t => 1.0 + (BackC3 * Math.Pow(t - 1.0, 3.0)) + (BackC1 * Math.Pow(t - 1.0, 2.0)),
        static t => t < 0.5 ? Math.Pow(2.0 * t, 2.0) * (((BackC2 + 1.0) * 2.0 * t) - BackC2) / 2.0 : ((Math.Pow((2.0 * t) - 2.0, 2.0) * (((BackC2 + 1.0) * ((t * 2.0) - 2.0)) + BackC2)) + 2.0) / 2.0,
        static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : -Math.Pow(2.0, (10.0 * t) - 10.0) * Math.Sin(((t * 10.0) - 10.75) * (2.0 * Math.PI / 3.0)),
        static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : (Math.Pow(2.0, -10.0 * t) * Math.Sin(((t * 10.0) - 0.75) * (2.0 * Math.PI / 3.0))) + 1.0,
        static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : t < 0.5 ? -(Math.Pow(2.0, (20.0 * t) - 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5))) / 2.0 : (Math.Pow(2.0, (-20.0 * t) + 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5)) / 2.0) + 1.0,
        static t => 1.0 - BounceOutFormula(1.0 - t),
        static t => BounceOutFormula(t),
        static t => t < 0.5 ? (1.0 - BounceOutFormula(1.0 - (2.0 * t))) / 2.0 : (1.0 + BounceOutFormula((2.0 * t) - 1.0)) / 2.0,
    ];

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
    public sealed record Pulse<TValue>(TValue From, TValue To, GhDuration Duration, GhMotion Easing, IMotionVector<TValue> Vector, Action<TValue> Sink, int Cycles = 1, bool Yoyo = false, bool Infinite = false, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Pulse(start: From, target: To, duration: Duration, easing: Easing, cycles: Cycles, yoyo: Yoyo, infinite: Infinite, vector: Vector, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Stroke(AnimatedPath Path, GhDuration Duration, GhMotion Easing, PaintStyle Style, PointF Origin, float Scale = 1f, float Angle = 0f, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Stroke(path: Path, duration: Duration, easing: Easing, style: Style, origin: Origin, scale: Scale, angle: Angle, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Sparkle(ISparkle Instance) : MotionRequest<Unit> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Motion.Sparkle(instance: Instance).Run(scope: scope); }
    public sealed record Theme(Skin From, Skin To, GhDuration Duration, GhMotion Easing, Action<Skin> Sink, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Theme(start: From, target: To, duration: Duration, easing: Easing, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Navigate(PointF Centre, GhDuration Duration, float MinZoom = CanvasViewPolicy.DefaultMinimumZoom, float MaxZoom = CanvasViewPolicy.DefaultMaximumZoom) : MotionRequest<Unit> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(); internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Motion.Navigate(centre: Centre, duration: Duration, minZoom: MinZoom, maxZoom: MaxZoom).Run(scope: scope); }
    public sealed record ZoomGate(ZoomThreshold Threshold, Action<float> Sink, MotionClock? Clock = null) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => ScheduledCanvas; internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.ZoomGate(threshold: Threshold, sink: Sink, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record Cosmetic(CosmeticIntent Intent) : MotionRequest<Subscription> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(); internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Cosmetic(intent: Intent).Run(scope: scope); }
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
    // Semi-implicit Euler: F = -k·x - b·v ; a = F/m ; v += a·dt ; x += v·dt.
    public SpringRunnerState<T> Step(float frameDeltaSeconds) {
        long now = Clock.GetTimestamp();
        float dt = frameDeltaSeconds > 0f
            ? Math.Min(val1: frameDeltaSeconds, val2: Motion.MaxFrameDelta)
            : Math.Min(val1: (float)Clock.GetElapsedTime(startingTimestamp: Timestamp, endingTimestamp: now).TotalSeconds, val2: Motion.MaxFrameDelta);
        T displacement = Vector.Subtract(Value, Target);
        T accel = Vector.Scale(
            Vector.Add(
                Vector.Scale(displacement, -Config.Stiffness),
                Vector.Scale(Velocity, -Config.Damping)),
            1f / Config.Mass);
        T newVelocity = Vector.Add(Velocity, Vector.Scale(accel, dt));
        T newValue = Vector.Add(Value, Vector.Scale(newVelocity, dt));
        return this with { Value = newValue, Velocity = newVelocity, Timestamp = now };
    }

    public Unit Emit() { Sink(Value); return unit; }

    public bool IsActive =>
        !((Vector.Norm(Vector.Subtract(Value, Target)) < Vector.RestEpsilon) && (Vector.Norm(Velocity) < Vector.RestEpsilon));

    public SpringRunnerState<T> MergeInto(SpringRunnerState<T> live) =>
        live with { Value = Value, Velocity = Velocity, Timestamp = Timestamp };
}

public sealed class SpringHandle<T> : IDisposable {
    private readonly Atom<SpringRunnerState<T>> cell;
    private readonly Subscription subscription;
    private readonly Action wake;

    internal SpringHandle(Atom<SpringRunnerState<T>> cell, Subscription subscription, Action wake) {
        this.cell = cell;
        this.subscription = subscription;
        this.wake = wake;
    }

    public T CurrentValue => cell.Value.Value;
    public T CurrentVelocity => cell.Value.Velocity;
    public T CurrentTarget => cell.Value.Target;

    public Unit Retarget(T target, Option<T> initialVelocity = default) {
        _ = cell.Swap(state => state with {
            Target = target,
            Velocity = initialVelocity.IfNone(state.Velocity),
        });
        wake();
        return unit;
    }

    public Unit RetargetWhen(T target, Func<T, bool> shouldUpdate, Option<T> initialVelocity = default) {
        _ = cell.SwapMaybe(state => shouldUpdate(state.Target)
            ? Some(state with { Target = target, Velocity = initialVelocity.IfNone(state.Velocity) })
            : Option<SpringRunnerState<T>>.None);
        wake();
        return unit;
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

    public PulseRunnerState<T> MergeInto(PulseRunnerState<T> live) =>
        live with { Animated = Animated, CyclesRemaining = CyclesRemaining };
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
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<double> Double = new MotionVectorImpl<double>(
        zero: 0.0,
        restEpsilon: ScalarRest,
        add: static (a, b) => a + b,
        subtract: static (a, b) => a - b,
        scale: static (v, s) => v * s,
        norm: static v => (float)Math.Abs(v),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<PointF> PointF = new MotionVectorImpl<PointF>(
        zero: Eto.Drawing.PointF.Empty,
        restEpsilon: PixelRest,
        add: static (a, b) => new PointF(a.X + b.X, a.Y + b.Y),
        subtract: static (a, b) => new PointF(a.X - b.X, a.Y - b.Y),
        scale: static (v, s) => new PointF(v.X * s, v.Y * s),
        norm: static v => MathF.Sqrt((v.X * v.X) + (v.Y * v.Y)),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<SizeF> SizeF = new MotionVectorImpl<SizeF>(
        zero: Eto.Drawing.SizeF.Empty,
        restEpsilon: PixelRest,
        add: static (a, b) => new SizeF(a.Width + b.Width, a.Height + b.Height),
        subtract: static (a, b) => new SizeF(a.Width - b.Width, a.Height - b.Height),
        scale: static (v, s) => new SizeF(v.Width * s, v.Height * s),
        norm: static v => MathF.Sqrt((v.Width * v.Width) + (v.Height * v.Height)),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<RectangleF> RectangleF = new MotionVectorImpl<RectangleF>(
        zero: Eto.Drawing.RectangleF.Empty,
        restEpsilon: PixelRest,
        add: static (a, b) => new RectangleF(a.X + b.X, a.Y + b.Y, a.Width + b.Width, a.Height + b.Height),
        subtract: static (a, b) => new RectangleF(a.X - b.X, a.Y - b.Y, a.Width - b.Width, a.Height - b.Height),
        scale: static (v, s) => new RectangleF(v.X * s, v.Y * s, v.Width * s, v.Height * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.X), Math.Abs(v.Y)), Math.Max(Math.Abs(v.Width), Math.Abs(v.Height))),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
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
            interpolate: interpolate);

    private static Color HslInterpolate(Color a, Color b, double factor) {
        ColorHSL ha = a;
        ColorHSL hb = b;
        float tf = (float)factor;
        // Shortest hue arc in [-180, +180].
        float hueDelta = ((hb.H - ha.H + 540f) % 360f) - 180f;
        return new ColorHSL(
            hue: ha.H + (hueDelta * tf),
            saturation: ha.S + ((hb.S - ha.S) * tf),
            luminance: ha.L + ((hb.L - ha.L) * tf),
            alpha: ha.A + ((hb.A - ha.A) * tf));
    }

    // --- [OKLAB CONVERSION] ----------------------------------------------------------------
    // Ottosson 2020: sRGB → linear → LMS → OKLab and back.
    private static Color OklabInterpolate(Color a, Color b, double factor) {
        (float la, float aa, float ba) = SrgbToOklab(a);
        (float lb, float ab, float bb) = SrgbToOklab(b);
        float tf = (float)factor;
        (float L, float A, float B) mix = (
            L: la + ((lb - la) * tf),
            A: aa + ((ab - aa) * tf),
            B: ba + ((bb - ba) * tf));
        return OklabToSrgb(mix, alpha: a.A + ((b.A - a.A) * tf));
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
        float hMix = ha + (hueDelta * tf);
        float lMix = la + ((lb - la) * tf);
        float cMix = ca + ((cb - ca) * tf);
        (float L, float A, float B) mix = (L: lMix, A: cMix * MathF.Cos(hMix), B: cMix * MathF.Sin(hMix));
        return OklabToSrgb(mix, alpha: a.A + ((b.A - a.A) * tf));
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
    Func<T, T, double, T> interpolate) : IMotionVector<T> {
    public T Zero { get; } = zero;
    public float RestEpsilon { get; } = restEpsilon;
    public T Add(T a, T b) => add(arg1: a, arg2: b);
    public T Subtract(T a, T b) => subtract(arg1: a, arg2: b);
    public T Scale(T value, float scalar) => scale(arg1: value, arg2: scalar);
    public float Norm(T value) => norm(arg: value);
    public T Interpolate(T value0, T value1, double factor) => interpolate(arg1: value0, arg2: value1, arg3: factor);
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
    // dt clamp prevents spring blowup on debugger/GC stalls (33ms = 30fps floor).
    internal const float MaxFrameDelta = 1f / 30f;

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
                wake: canvas.ScheduleRedraw));

    private static GrasshopperUiIntent<SpringHandle<TValue>> AnimatedSpring<TValue>(
        TValue start, TValue target, SpringConfig config, IMotionVector<TValue> vector,
        Action<TValue> sink, Option<TValue> initialVelocity, TimeProvider? timeSource, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "sink delegate is required"))
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
        Action wake = pacer is { IsSome: true }
            ? () => _ = PacerResume(pacer: pacer, canvas: canvas)
            : canvas.ScheduleRedraw;
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

    internal static Unit PauseWhenFinished(GhState state, Option<Pacer> pacer) {
        if (state == GhState.Finished && pacer is { IsSome: true, Case: Pacer p }) {
            _ = p.Pause();
        }
        return unit;
    }

    private static Func<PaintScope, Fin<Unit>> RunnerPaint<TState>(
        Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer, string opName)
        where TState : struct, IMotionState<TState> =>
        paintScope => Op.Of(name: opName).Attempt(
            body: () => {
                _ = MotionRunner<TState>.Of(cell: cell, canvas: canvas, pacer: pacer, cancellation: default).Tick();
                return unit;
            },
            what: "motion tick");

    internal static GrasshopperUiIntent<Subscription> Pulse<TValue>(
        TValue start, TValue target, GhDuration duration, GhMotion easing, int cycles, bool yoyo, bool infinite,
        IMotionVector<TValue> vector, Action<TValue> sink, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "sink delegate is required"))
            from validCycles in infinite
                ? Fin.Succ(1)
                : Optional(cycles).Filter(static c => c >= 1)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "cycles must be positive when not infinite"))
            from result in Pipeline(opName: nameof(Pulse), phase: CanvasPaintPhase.BeforeBackground, clock: clock,
                paintFactory: (canvas, pacer) => {
                    Func<PaintScope, Fin<Unit>> animated = RunnerPaint(
                        cell: Atom(new PulseRunnerState<TValue>(
                            Animated: Animated<TValue>.CreateUnfinished(
                                value0: start, value1: target,
                                duration: Animators.DurationToTimeSpan(duration: duration),
                                motion: easing, interpolator: validVector.Interpolate),
                            From: start, To: target, Duration: duration, Easing: easing, Yoyo: yoyo, Infinite: infinite,
                            CyclesRemaining: validCycles - 1, Vector: validVector, Sink: validSink)),
                        canvas: canvas, pacer: pacer, opName: nameof(Pulse));
                    return paintScope => MotionAccessibility.ShouldReduceMotion
                        ? Op.Of(name: nameof(Pulse)).Attempt(body: () => { validSink(target); return unit; }, what: "pulse reduce-motion settle")
                        : animated(paintScope);
                }).Run(scope: scope)
            select result);

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
                from _ in Try.lift(f: () => { canvas.AddSparkle(sparkle: valid); return unit; }).Run()
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
            from validated in ValidateCosmetic(intent: intent)
            from mapped in MapCosmetic(canvas: canvas, intent: validated)
            let key = (NSString)$"rasm.cosmetic.{Guid.NewGuid():N}"
            from sub in Subscription.Bind(
                attach: () => CosmeticAttach(view: view, intent: mapped, key: key),
                detach: () => CosmeticStrip(view: view, key: key),
                marshalToUi: true)
            select sub);

    private static Fin<CosmeticIntent> MapCosmetic(Grasshopper2.UI.Canvas.Canvas canvas, CosmeticIntent intent) =>
        intent.Switch(
            pulseCase: static (c, p) => Fin.Succ<CosmeticIntent>(new CosmeticIntent.PulseCase(
                Bounds: MapCosmeticRect(canvas: c, bounds: p.Bounds), Tint: p.Tint, Duration: p.Duration)),
            glowCase: static (c, g) => Fin.Succ<CosmeticIntent>(new CosmeticIntent.GlowCase(
                Bounds: MapCosmeticRect(canvas: c, bounds: g.Bounds), Tint: g.Tint, CornerRadius: g.CornerRadius, ShadowRadius: g.ShadowRadius, Duration: g.Duration)),
            strokeOnCase: static (c, s) => Fin.Succ<CosmeticIntent>(new CosmeticIntent.StrokeOnCase(
                Polyline: MapCosmeticPolyline(canvas: c, polyline: s.Polyline), Tint: s.Tint, Thickness: s.Thickness, Duration: s.Duration)),
            state: canvas);

    private static RectangleF MapCosmeticRect(Grasshopper2.UI.Canvas.Canvas canvas, RectangleF bounds) =>
        canvas.Map(rectangle: bounds, from: CoordinateSystem.Content, to: CoordinateSystem.Control);

    private static ReadOnlyMemory<PointF> MapCosmeticPolyline(Grasshopper2.UI.Canvas.Canvas canvas, ReadOnlyMemory<PointF> polyline) =>
        toSeq(polyline.Span.ToArray())
            .Map(point => canvas.Map(point: point, from: CoordinateSystem.Content, to: CoordinateSystem.Control))
            .ToArray();

    private static Fin<CosmeticIntent> ValidateCosmetic(CosmeticIntent intent) =>
        intent.Switch(
            pulseCase: p => {
                Op op = Op.Of(name: nameof(CosmeticIntent.PulseCase));
                return op.AcceptRect(value: p.Bounds, detail: "non-finite pulse bounds")
                    .Map<CosmeticIntent>(_ => p);
            },
            glowCase: g => {
                Op op = Op.Of(name: nameof(CosmeticIntent.GlowCase));
                return op.AcceptRect(value: g.Bounds, detail: "non-finite glow bounds")
                    .Bind(_ => op.AcceptFinite(value: g.CornerRadius, detail: "non-finite corner radius"))
                    .Bind(_ => op.AcceptFinite(value: g.ShadowRadius, detail: "non-finite shadow radius"))
                    .Map<CosmeticIntent>(_ => g);
            },
            strokeOnCase: s => {
                Op op = Op.Of(name: nameof(CosmeticIntent.StrokeOnCase));
                return op.AcceptFinite(value: s.Thickness, detail: "non-positive thickness", requirePositive: true)
                    .Map<CosmeticIntent>(_ => s);
            });

    [SupportedOSPlatform("macos14.0")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "CAShapeLayer ownership transfers to host CALayer via AddSublayer; disposal happens on Subscription detach or animation completion delegate.")]
    private static Unit CosmeticAttach(NSView view, CosmeticIntent intent, NSString key) {
        view.WantsLayer = true;
        if (view.Layer is not CALayer host) {
            return unit;
        }
        CAShapeLayer shape = BuildCosmeticShape(intent: intent);
        shape.Name = key.ToString();
        if (MotionAccessibility.ShouldSkipDecorativeMotion) {
            return WithoutAnimation(() => {
                ApplyCosmeticTerminal(shape: shape, intent: intent);
                host.AddSublayer(layer: shape);
            });
        }
        host.AddSublayer(layer: shape);
        CAAnimation animation = BuildCosmeticAnimation(intent: intent);
        animation.WeakDelegate = new CosmeticRemove(shape: shape, key: key);
        shape.AddAnimation(animation: animation, key: key);
        return unit;
    }

    private static void ApplyCosmeticTerminal(CAShapeLayer shape, CosmeticIntent intent) {
        _ = intent is CosmeticIntent.PulseCase ? Op.Side(() => shape.Opacity = 0f)
            : intent is CosmeticIntent.GlowCase ? Op.Side(() => shape.ShadowOpacity = 0f)
            : Op.Side(() => shape.StrokeEnd = 1f);
    }

    // Mutual-exclusion claim: TryClaim wins exactly once across explicit-dispose vs AnimationStopped.
    [SupportedOSPlatform("macos14.0")]
    private static Unit CosmeticStrip(NSView view, NSString key) {
        if (view.Layer is not CALayer host || host.Sublayers is not CALayer[] sublayers) {
            return unit;
        }
        string keyString = key.ToString();
        return WithoutAnimation(() => {
            foreach (CALayer sub in sublayers) {
                if (!string.Equals(a: sub.Name, b: keyString, comparisonType: StringComparison.Ordinal)) {
                    continue;
                }
                if (sub.AnimationForKey(key: keyString) is CAAnimation animation
                    && animation.WeakDelegate is CosmeticRemove remove
                    && !remove.TryClaim()) {
                    continue;
                }
                sub.RemoveAnimation(key: keyString);
                sub.RemoveFromSuperLayer();
            }
        });
    }

    [SupportedOSPlatform("macos14.0")]
    private static CAShapeLayer BuildCosmeticShape(CosmeticIntent intent) =>
        intent.Switch(
            pulseCase: static p => {
                CAShapeLayer shape = new() { Frame = ToCGRect(p.Bounds) };
                CGPath path = new();
                path.AddRect(rect: ToCGRect(p.Bounds));
                shape.Path = path;
                shape.FillColor = ToCGColor(p.Tint);
                shape.Opacity = 0f;
                return shape;
            },
            glowCase: static g => {
                CGRect rect = ToCGRect(g.Bounds);
                CGPath path = new();
                path.AddRoundedRect(transform: CGAffineTransform.MakeIdentity(), rect: rect, cornerWidth: g.CornerRadius, cornerHeight: g.CornerRadius);
                CAShapeLayer shape = new() {
                    Frame = rect,
                    Path = path,
                    StrokeColor = ToCGColor(g.Tint),
                    LineWidth = 0f,
                    ShadowColor = ToCGColor(g.Tint),
                    ShadowRadius = g.ShadowRadius,
                    ShadowOffset = CGSize.Empty,
                    ShadowPath = path,
                    ShadowOpacity = 0f,
                };
                return shape;
            },
            strokeOnCase: static s => {
                CGPath path = new();
                ReadOnlySpan<PointF> points = s.Polyline.Span;
                if (points.Length > 0) {
                    path.MoveToPoint(point: ToCGPoint(points[0]));
                    for (int i = 1; i < points.Length; i++) {
                        path.AddLineToPoint(point: ToCGPoint(points[i]));
                    }
                }
                return new CAShapeLayer {
                    Path = path,
                    StrokeColor = ToCGColor(s.Tint),
                    LineWidth = s.Thickness,
                    StrokeStart = 0f,
                    StrokeEnd = 0f,
                };
            });

    // CABasicAnimation does not auto-honour NSWorkspace reduce-motion; terminal snap is handled in CosmeticAttach.
    [SupportedOSPlatform("macos14.0")]
    private static CAKeyFrameAnimation KeyframeFade(string path, GhDuration duration, float alpha) {
        CAKeyFrameAnimation anim = CAKeyFrameAnimation.FromKeyPath(path: path);
        anim.Duration = Animators.DurationToTimeSpan(duration: duration).TotalSeconds;
        anim.Values = [NSNumber.FromFloat(0f), NSNumber.FromFloat(alpha), NSNumber.FromFloat(0f)];
        anim.KeyTimes = [NSNumber.FromFloat(0f), NSNumber.FromFloat(0.5f), NSNumber.FromFloat(1f)];
        return anim;
    }

    [SupportedOSPlatform("macos14.0")]
    private static CABasicAnimation StrokeEndAnimation(GhDuration duration) {
        CABasicAnimation anim = CABasicAnimation.FromKeyPath(path: "strokeEnd");
        anim.Duration = Animators.DurationToTimeSpan(duration: duration).TotalSeconds;
        anim.SetFrom(value: NSNumber.FromFloat(0f));
        anim.SetTo(value: NSNumber.FromFloat(1f));
        return anim;
    }

    [SupportedOSPlatform("macos14.0")]
    private static CAAnimation BuildCosmeticAnimation(CosmeticIntent intent) =>
        intent is CosmeticIntent.PulseCase pulse ? KeyframeFade(path: "opacity", duration: pulse.Duration, alpha: pulse.Tint.A)
        : intent is CosmeticIntent.GlowCase glow ? KeyframeFade(path: "shadowOpacity", duration: glow.Duration, alpha: glow.Tint.A)
        : intent is CosmeticIntent.StrokeOnCase stroke ? StrokeEndAnimation(duration: stroke.Duration)
        : KeyframeFade(path: "opacity", duration: GhDuration.Fast, alpha: 0f);

    private static CGRect ToCGRect(RectangleF r) => new(x: r.X, y: r.Y, width: r.Width, height: r.Height);

    private static CGPoint ToCGPoint(PointF p) => new(x: p.X, y: p.Y);

    private static CGColor ToCGColor(Color c) => new(red: c.R, green: c.G, blue: c.B, alpha: c.A);

    private static Unit WithoutAnimation(Action body) {
        CATransaction.Begin();
        CATransaction.DisableActions = true;
        try { body(); } finally { CATransaction.Commit(); }
        return unit;
    }

    // Exchange-on-stripped is symmetric with CosmeticStrip.TryClaim — whichever path wins owns the strip.
    [SupportedOSPlatform("macos14.0")]
    private sealed class CosmeticRemove : CAAnimationDelegate {
        private readonly CAShapeLayer shape;
        private readonly string key;
        private int stripped;

        internal CosmeticRemove(CAShapeLayer shape, NSString key) {
            this.shape = shape;
            this.key = key.ToString();
        }

        public override void AnimationStopped(CAAnimation animation, bool finished) {
            if (Interlocked.Exchange(ref stripped, 1) == 1) {
                return;
            }
            _ = WithoutAnimation(() => {
                shape.RemoveAnimation(key: key);
                shape.RemoveFromSuperLayer();
            });
        }

        internal bool TryClaim() => Interlocked.Exchange(ref stripped, 1) == 0;
    }

    internal sealed class MotionRunner<TState> where TState : struct, IMotionState<TState> {
        private readonly Atom<TState> cell;
        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly Option<Pacer> pacer;
        private readonly CancellationToken cancellation;
        private readonly Func<TState, TState> applyScratch;
        private TState scratch;
        private int wantingTicks;
        private double lastVsyncTimestamp;

        private MotionRunner(Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer, CancellationToken cancellation) {
            this.cell = cell;
            this.canvas = canvas;
            this.pacer = pacer;
            this.cancellation = cancellation;
            // Block body re-reads mutable `scratch` each call; a method-group form would freeze the
            // struct receiver to default(TState) at construction (IDE0200 hazard).
            applyScratch = live => {
                TState current = scratch;
                return current.MergeInto(live: live);
            };
        }

        internal static MotionRunner<TState> Of(Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer = default, CancellationToken cancellation = default) =>
            new(cell: cell, canvas: canvas, pacer: pacer, cancellation: cancellation);

        internal Unit Tick() {
            if (cancellation.IsCancellationRequested) {
                return Sleep();
            }
            TState next = cell.Value.Step(frameDeltaSeconds: ResolveFrameDelta());
            _ = next.Emit();
            scratch = next;
            _ = cell.Swap(applyScratch);
            _ = next.IsActive ? Wake() : Sleep();
            return unit;
        }

        private float ResolveFrameDelta() {
            if (pacer is not { IsSome: true, Case: Pacer paced }) {
                return 0f;
            }
            double timestamp = paced.LastFrameTimestamp;
            if (timestamp <= 0d || lastVsyncTimestamp <= 0d) {
                if (timestamp > 0d) {
                    lastVsyncTimestamp = timestamp;
                }
                return 0f;
            }
            float delta = (float)(timestamp - lastVsyncTimestamp);
            lastVsyncTimestamp = timestamp;
            return delta;
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
                foreach (KeyValuePair<Grasshopper2.UI.Canvas.Canvas, Pacer> entry in Pool) {
                    entry.Value.RebindLink();
                }
            }
        }

        private CADisplayLink BindLink(NSView view, CAFrameRateRange range) {
            CADisplayLink bound = view.GetDisplayLink(target: this, selector: TickSelector);
            bound.PreferredFrameRateRange = range;
            bound.Paused = true;
            bound.AddToRunLoop(runloop: NSRunLoop.Main, mode: NSRunLoopMode.Common.GetConstant()!);
            return bound;
        }

        // NSScreen.MaximumFramesPerSecond honours the actual panel ceiling (60/120/144Hz vary).
        private static CAFrameRateRange ResolveRange(NSView view, CAFrameRateRange? explicitRate) {
            if (explicitRate is CAFrameRateRange supplied) {
                return supplied;
            }
            NSScreen? screen = view.Window?.Screen;
            float maximum = screen is NSScreen active ? active.MaximumFramesPerSecond : 120f;
            float floor = MathF.Min(x: 30f, y: maximum);
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

        // Invalidate-prev-then-unpause-new avoids the dual-link overlap window in the main RunLoop.
        private void RebindLink() {
            bool wasActive = active > 0;
            CADisplayLink replacement = BindLink(view: view, range: ResolveRange(view: view, explicitRate: explicitRate));
            CADisplayLink previous = Interlocked.Exchange(ref link, replacement);
            previous.Invalidate();
            previous.Dispose();
            replacement.Paused = !wasActive;
        }

        // sender.Timestamp = just-displayed host clock (mach_absolute_time, seconds). Exposed for
        // integrators that want exact-vsync dt instead of paint-time TimeProvider drift. Volatile
        // fences AArch64 reordering; 64-bit double is atomic on Apple Silicon.
        [Export("tick:")]
        public void Tick(CADisplayLink sender) {
            Volatile.Write(ref lastFrameTimestamp, sender.Timestamp);
            canvas.ScheduleRedraw();
        }

        internal double LastFrameTimestamp => Volatile.Read(ref lastFrameTimestamp);

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
