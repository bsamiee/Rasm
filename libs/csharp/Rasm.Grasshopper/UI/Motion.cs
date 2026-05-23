using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using AppKit;
using CoreAnimation;
using Foundation;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Animation;
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
public interface IMotionVector<T> {
    public T Zero { get; }
    // Per-type rest threshold. Color channels are [0,1] (1/255 = 1 visual quanta); pixel-quantized
    // PointF/SizeF/RectangleF cross visual rest at half a logical pixel; scalars at 1e-3 normalized.
    public float RestEpsilon { get; }
    public T Add(T a, T b);
    public T Subtract(T a, T b);
    public T Scale(T value, float scalar);
    public float Norm(T value);
    public T Interpolate(T value0, T value1, double factor);
}

// Polymorphic runner contract. SpringRunnerState and PulseRunnerState implement this so a single
// MotionRunner<TState> drives both spring physics and pulse tweens through static dispatch.
internal interface IMotionState<TSelf> where TSelf : struct, IMotionState<TSelf> {
    public TSelf Step();              // pure compute: integrate / reissue / no-op
    public Unit Emit();               // side effect: invoke sink with current value
    public bool IsActive { get; }     // continue ticking next frame?
    public TSelf MergeInto(TSelf live); // preserve concurrent mutator updates (e.g., SpringHandle.Retarget)
}

// Polymorphic easing surface — 16 GH2-native cases delegate to MotionEquations.Blend; 30 closed-form
// Penner cases (Sine/Quad/Cubic/Quart/Quint/Expo/Circ/Back/Elastic/Bounce × In/Out/InOut) implement the
// canonical formulae inline. Pre-multiplied to a per-case `Func<double, double>` at registration
// time so the hot 60 Hz tick path pays only the partial method invoke + a single virtual delegate dispatch.
[SmartEnum<int>]
public sealed partial class Easing {
    private delegate double EasingCurve(double t);

    // Penner Back parameters (overshoot magnitudes). c2 = c1 * 1.525 for the In/Out variant.
    private const double BackC1 = 1.70158;
    private const double BackC3 = BackC1 + 1.0;
    private const double BackC2 = BackC1 * 1.525;
    // Bounce piecewise normalization constants.
    private const double BounceN1 = 7.5625;
    private const double BounceD1 = 2.75;

    // --- [GH2_NATIVE] (delegate to MotionEquations.Blend) -----------------------------------
    public static readonly Easing Linear = Native(key: 0, motion: GhMotion.Linear);
    public static readonly Easing LinearDelayed = Native(key: 1, motion: GhMotion.LinearDelayed);
    public static readonly Easing EaseIn = Native(key: 2, motion: GhMotion.EaseIn);
    public static readonly Easing EaseInDelayed = Native(key: 3, motion: GhMotion.EaseInDelayed);
    public static readonly Easing EaseOut = Native(key: 4, motion: GhMotion.EaseOut);
    public static readonly Easing EaseOutDelayed = Native(key: 5, motion: GhMotion.EaseOutDelayed);
    public static readonly Easing EaseInOut = Native(key: 6, motion: GhMotion.EaseInOut);
    public static readonly Easing EaseInOutDelayed = Native(key: 7, motion: GhMotion.EaseInOutDelayed);
    public static readonly Easing SnapIn = Native(key: 8, motion: GhMotion.SnapIn);
    public static readonly Easing SnapInDelayed = Native(key: 9, motion: GhMotion.SnapInDelayed);
    public static readonly Easing SnapOut = Native(key: 10, motion: GhMotion.SnapOut);
    public static readonly Easing SnapOutDelayed = Native(key: 11, motion: GhMotion.SnapOutDelayed);
    public static readonly Easing Bounce = Native(key: 12, motion: GhMotion.Bounce);
    public static readonly Easing BounceDelayed = Native(key: 13, motion: GhMotion.BounceDelayed);
    public static readonly Easing Twang = Native(key: 14, motion: GhMotion.Twang);
    public static readonly Easing TwangDelayed = Native(key: 15, motion: GhMotion.TwangDelayed);

    // --- [CLOSED_FORM] (Penner equations: 10 families × In/Out/InOut) -----------------------
    public static readonly Easing SineIn = Closed(key: 16, compute: static t => 1.0 - Math.Cos(t * Math.PI / 2.0));
    public static readonly Easing SineOut = Closed(key: 17, compute: static t => Math.Sin(t * Math.PI / 2.0));
    public static readonly Easing SineInOut = Closed(key: 18, compute: static t => -(Math.Cos(Math.PI * t) - 1.0) / 2.0);
    public static readonly Easing QuadIn = Closed(key: 19, compute: static t => t * t);
    public static readonly Easing QuadOut = Closed(key: 20, compute: static t => 1.0 - ((1.0 - t) * (1.0 - t)));
    public static readonly Easing QuadInOut = Closed(key: 21, compute: static t =>
        t < 0.5 ? 2.0 * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 2.0) / 2.0));
    public static readonly Easing CubicIn = Closed(key: 22, compute: static t => t * t * t);
    public static readonly Easing CubicOut = Closed(key: 23, compute: static t => 1.0 - Math.Pow(1.0 - t, 3.0));
    public static readonly Easing CubicInOut = Closed(key: 24, compute: static t =>
        t < 0.5 ? 4.0 * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 3.0) / 2.0));
    public static readonly Easing QuartIn = Closed(key: 25, compute: static t => t * t * t * t);
    public static readonly Easing QuartOut = Closed(key: 26, compute: static t => 1.0 - Math.Pow(1.0 - t, 4.0));
    public static readonly Easing QuartInOut = Closed(key: 27, compute: static t =>
        t < 0.5 ? 8.0 * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 4.0) / 2.0));
    public static readonly Easing QuintIn = Closed(key: 28, compute: static t => t * t * t * t * t);
    public static readonly Easing QuintOut = Closed(key: 29, compute: static t => 1.0 - Math.Pow(1.0 - t, 5.0));
    public static readonly Easing QuintInOut = Closed(key: 30, compute: static t =>
        t < 0.5 ? 16.0 * t * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 5.0) / 2.0));
    public static readonly Easing ExpoIn = Closed(key: 31, compute: static t => t == 0.0 ? 0.0 : Math.Pow(2.0, (10.0 * t) - 10.0));
    public static readonly Easing ExpoOut = Closed(key: 32, compute: static t => t == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * t));
    public static readonly Easing ExpoInOut = Closed(key: 33, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        t < 0.5 ? Math.Pow(2.0, (20.0 * t) - 10.0) / 2.0 : (2.0 - Math.Pow(2.0, (-20.0 * t) + 10.0)) / 2.0);
    public static readonly Easing CircIn = Closed(key: 34, compute: static t => 1.0 - Math.Sqrt(1.0 - (t * t)));
    public static readonly Easing CircOut = Closed(key: 35, compute: static t => Math.Sqrt(1.0 - ((t - 1.0) * (t - 1.0))));
    public static readonly Easing CircInOut = Closed(key: 36, compute: static t =>
        t < 0.5
            ? (1.0 - Math.Sqrt(1.0 - (4.0 * t * t))) / 2.0
            : (Math.Sqrt(1.0 - (((-2.0 * t) + 2.0) * ((-2.0 * t) + 2.0))) + 1.0) / 2.0);
    public static readonly Easing BackIn = Closed(key: 37, compute: static t => (BackC3 * t * t * t) - (BackC1 * t * t));
    public static readonly Easing BackOut = Closed(key: 38, compute: static t =>
        1.0 + (BackC3 * Math.Pow(t - 1.0, 3.0)) + (BackC1 * Math.Pow(t - 1.0, 2.0)));
    public static readonly Easing BackInOut = Closed(key: 39, compute: static t =>
        t < 0.5
            ? Math.Pow(2.0 * t, 2.0) * (((BackC2 + 1.0) * 2.0 * t) - BackC2) / 2.0
            : ((Math.Pow((2.0 * t) - 2.0, 2.0) * (((BackC2 + 1.0) * ((t * 2.0) - 2.0)) + BackC2)) + 2.0) / 2.0);
    public static readonly Easing ElasticIn = Closed(key: 40, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        -Math.Pow(2.0, (10.0 * t) - 10.0) * Math.Sin(((t * 10.0) - 10.75) * (2.0 * Math.PI / 3.0)));
    public static readonly Easing ElasticOut = Closed(key: 41, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        (Math.Pow(2.0, -10.0 * t) * Math.Sin(((t * 10.0) - 0.75) * (2.0 * Math.PI / 3.0))) + 1.0);
    public static readonly Easing ElasticInOut = Closed(key: 42, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        t < 0.5
            ? -(Math.Pow(2.0, (20.0 * t) - 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5))) / 2.0
            : (Math.Pow(2.0, (-20.0 * t) + 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5)) / 2.0) + 1.0);
    public static readonly Easing BounceIn = Closed(key: 43, compute: static t => 1.0 - BounceOutFormula(1.0 - t));
    public static readonly Easing BounceOut = Closed(key: 44, compute: BounceOutFormula);
    public static readonly Easing BounceInOut = Closed(key: 45, compute: static t =>
        t < 0.5
            ? (1.0 - BounceOutFormula(1.0 - (2.0 * t))) / 2.0
            : (1.0 + BounceOutFormula((2.0 * t) - 1.0)) / 2.0);

    [UseDelegateFromConstructor]
    public partial double Apply(double t);

    private static Easing Native(int key, GhMotion motion) =>
        new(key: key, apply: t => MotionEquations.Blend(motion: motion, parameter: t));

    private static Easing Closed(int key, EasingCurve compute) => new(key: key, apply: t => compute(t));

    private static double BounceOutFormula(double t) =>
        t switch {
            < 1.0 / BounceD1 => BounceN1 * t * t,
            < 2.0 / BounceD1 => (BounceN1 * (t - (1.5 / BounceD1)) * (t - (1.5 / BounceD1))) + 0.75,
            < 2.5 / BounceD1 => (BounceN1 * (t - (2.25 / BounceD1)) * (t - (2.25 / BounceD1))) + 0.9375,
            _ => (BounceN1 * (t - (2.625 / BounceD1)) * (t - (2.625 / BounceD1))) + 0.984375,
        };
}

public abstract record MotionRequest<T> : GhUiRequest<T> {
    public sealed record Tween<TValue>(Animated<TValue> Animated, Action<TValue> Sink) : MotionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Tween(animated: Animated, sink: Sink).Run(scope: scope);
    }
    public sealed record Spring<TValue>(TValue From, TValue To, SpringConfig Config, IMotionVector<TValue> Vector, Action<TValue> Sink, Option<TValue> InitialVelocity = default, TimeProvider? Clock = null, bool UseDisplayLink = false) : MotionRequest<SpringHandle<TValue>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<SpringHandle<TValue>> Apply(GrasshopperUi.Scope scope) => Motion.Spring(start: From, target: To, config: Config, vector: Vector, sink: Sink, initialVelocity: InitialVelocity, clock: Clock, useDisplayLink: UseDisplayLink).Run(scope: scope);
    }
    public sealed record Pulse<TValue>(TValue From, TValue To, GhDuration Duration, GhMotion Easing, IMotionVector<TValue> Vector, Action<TValue> Sink, int Cycles = 1, bool Yoyo = false, bool Infinite = false) : MotionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Pulse(start: From, target: To, duration: Duration, easing: Easing, cycles: Cycles, yoyo: Yoyo, infinite: Infinite, vector: Vector, sink: Sink).Run(scope: scope);
    }
    public sealed record Stroke(AnimatedPath Path, GhDuration Duration, GhMotion Easing, PaintStyle Style, PointF Origin, float Scale = 1f, float Angle = 0f) : MotionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Stroke(path: Path, duration: Duration, easing: Easing, style: Style, origin: Origin, scale: Scale, angle: Angle).Run(scope: scope);
    }
    public sealed record Sparkle(ISparkle Instance) : MotionRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Motion.Sparkle(instance: Instance).Run(scope: scope);
    }
    public sealed record Theme(Skin From, Skin To, GhDuration Duration, GhMotion Easing, Action<Skin> Sink) : MotionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Theme(start: From, target: To, duration: Duration, easing: Easing, sink: Sink).Run(scope: scope);
    }
    public sealed record Navigate(PointF Centre, GhDuration Duration, float MinZoom = CanvasViewPolicy.DefaultMinimumZoom, float MaxZoom = CanvasViewPolicy.DefaultMaximumZoom) : MotionRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Motion.Navigate(centre: Centre, duration: Duration, minZoom: MinZoom, maxZoom: MaxZoom).Run(scope: scope);
    }
    public sealed record ZoomGate(ZoomThreshold Threshold, Action<float> Sink) : MotionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.ZoomGate(threshold: Threshold, sink: Sink).Run(scope: scope);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
// Mass-spring-damper coefficients with construction-time finite+positivity invariants. Generated
// Create(...) factory enforces physics admission so the semi-implicit Euler step at
// SpringRunnerState.Step cannot receive NaN/zero/negative values that would explode the integrator.
[ComplexValueObject]
[ValidationError<UiFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SpringConfig {
    public float Stiffness { get; }
    public float Damping { get; }
    public float Mass { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float stiffness, ref float damping, ref float mass) =>
        validationError = (float.IsFinite(stiffness) && stiffness > 0f, float.IsFinite(damping) && damping >= 0f, float.IsFinite(mass) && mass > 0f) switch {
            (true, true, true) => null,
            (false, _, _) => UiFault.Create(message: $"SpringConfig.Stiffness must be finite and > 0 (got {stiffness:R})."),
            (_, false, _) => UiFault.Create(message: $"SpringConfig.Damping must be finite and >= 0 (got {damping:R})."),
            (_, _, false) => UiFault.Create(message: $"SpringConfig.Mass must be finite and > 0 (got {mass:R})."),
        };

    public static SpringConfig Response(float response, float dampingFraction, float mass = 1f) {
        float twoPiOverR = (float)(2.0 * Math.PI) / response;
        return Create(
            stiffness: twoPiOverR * twoPiOverR * mass,
            damping: 4f * (float)Math.PI * dampingFraction * mass / response,
            mass: mass);
    }

    public static readonly SpringConfig Snappy = Response(response: 0.30f, dampingFraction: 0.85f);
    public static readonly SpringConfig Bouncy = Response(response: 0.55f, dampingFraction: 0.50f);
    public static readonly SpringConfig Smooth = Response(response: 0.50f, dampingFraction: 1.00f);
    public static readonly SpringConfig Sluggish = Response(response: 1.00f, dampingFraction: 1.20f);
}

[StructLayout(LayoutKind.Auto)]
// `Clock` is the injectable time source (defaults to `TimeProvider.System` at construction). Tests
// can substitute `FakeTimeProvider` to advance time deterministically — the integrator never reads
// the wall clock directly. `Timestamp` is the last-frame timestamp in `Clock.TimestampFrequency`
// units (matches Stopwatch.Frequency for the default System provider).
internal readonly record struct SpringRunnerState<T>(
    T Value, T Velocity, T Target,
    SpringConfig Config, IMotionVector<T> Vector,
    Action<T> Sink, TimeProvider Clock, long Timestamp) : IMotionState<SpringRunnerState<T>> {
    // Semi-implicit Euler step with frame-rate clamp. Displacement = current - target;
    // force = -k*displacement - b*velocity; a = F / m.
    public SpringRunnerState<T> Step() {
        long now = Clock.GetTimestamp();
        float dt = Math.Min(val1: (float)Clock.GetElapsedTime(startingTimestamp: Timestamp, endingTimestamp: now).TotalSeconds, val2: Motion.MaxFrameDelta);
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

    // Preserve any concurrent Target/Sink updates from SpringHandle.Retarget by merging only the
    // integrator-owned fields back into the live cell value.
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
internal readonly record struct PulseRunnerState<T>(
    Animated<T> Animated,
    T From, T To,
    GhDuration Duration, GhMotion Easing,
    bool Yoyo, bool Infinite,
    int CyclesRemaining,
    IMotionVector<T> Vector,
    Action<T> Sink) : IMotionState<PulseRunnerState<T>> {
    // Reissue a fresh animator when the current cycle finishes and more remain; yoyo reverses
    // endpoints on each reissue. Non-cycling completion is a no-op (IsActive returns false next).
    public PulseRunnerState<T> Step() =>
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
    // Rest-epsilon vocabulary: scalars at 1e-3 normalized; pixel-quantized geometry rests at half a
    // logical pixel; channel-quantized colors rest at one byte of visual delta (1/255).
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
    public static readonly IMotionVector<Color> Color = new MotionVectorImpl<Color>(
        zero: new Color(red: 0f, green: 0f, blue: 0f, alpha: 0f),
        restEpsilon: ChannelRest,
        add: static (a, b) => new Color(red: a.R + b.R, green: a.G + b.G, blue: a.B + b.B, alpha: a.A + b.A),
        subtract: static (a, b) => new Color(red: a.R - b.R, green: a.G - b.G, blue: a.B - b.B, alpha: a.A - b.A),
        scale: static (v, s) => new Color(red: v.R * s, green: v.G * s, blue: v.B * s, alpha: v.A * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    // Process-wide phase clock. `Phase(period)` returns a value in [0, 1) advancing linearly over
    // `period`. Multiple animations sharing the same period stay phase-locked (e.g., two hover halos
    // pulsing together) — they sample the same wall clock divided by the same period. The clock is
    // driven by TimeProvider.System (mach_absolute_time on macOS, monotonic, no NTP drift).
    public static double Phase(TimeSpan period, TimeProvider? clock = null) {
        TimeProvider source = clock ?? TimeProvider.System;
        double seconds = (double)source.GetTimestamp() / source.TimestampFrequency;
        double cycle = period.TotalSeconds;
        return cycle <= 0d ? 0d : seconds % cycle / cycle;
    }

    public static readonly IMotionVector<Color> ColorHSL = new MotionVectorImpl<Color>(
        zero: new Color(red: 0f, green: 0f, blue: 0f, alpha: 0f),
        restEpsilon: ChannelRest,
        add: static (a, b) => new Color(red: a.R + b.R, green: a.G + b.G, blue: a.B + b.B, alpha: a.A + b.A),
        subtract: static (a, b) => new Color(red: a.R - b.R, green: a.G - b.G, blue: a.B - b.B, alpha: a.A - b.A),
        scale: static (v, s) => new Color(red: v.R * s, green: v.G * s, blue: v.B * s, alpha: v.A * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
        interpolate: HslInterpolate);

    // Perceptual OKLab interpolation (Björn Ottosson, CSS Color Level 4). Saturated transitions
    // avoid the dead-grey midpoint of sRGB lerp — red → green passes through olive/yellow-green
    // instead of muddy grey. Hue, lightness, and chroma move on perceptually uniform axes.
    public static readonly IMotionVector<Color> ColorOklab = new MotionVectorImpl<Color>(
        zero: new Color(red: 0f, green: 0f, blue: 0f, alpha: 0f),
        restEpsilon: ChannelRest,
        add: static (a, b) => new Color(red: a.R + b.R, green: a.G + b.G, blue: a.B + b.B, alpha: a.A + b.A),
        subtract: static (a, b) => new Color(red: a.R - b.R, green: a.G - b.G, blue: a.B - b.B, alpha: a.A - b.A),
        scale: static (v, s) => new Color(red: v.R * s, green: v.G * s, blue: v.B * s, alpha: v.A * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
        interpolate: OklabInterpolate);

    // Cylindrical OKLab (OKLCH): perceptual lightness + chroma + hue. Hue uses shortest-arc
    // wrap like ColorHSL, but lightness/chroma travel in OKLab space — gradients stay vivid.
    public static readonly IMotionVector<Color> ColorOklch = new MotionVectorImpl<Color>(
        zero: new Color(red: 0f, green: 0f, blue: 0f, alpha: 0f),
        restEpsilon: ChannelRest,
        add: static (a, b) => new Color(red: a.R + b.R, green: a.G + b.G, blue: a.B + b.B, alpha: a.A + b.A),
        subtract: static (a, b) => new Color(red: a.R - b.R, green: a.G - b.G, blue: a.B - b.B, alpha: a.A - b.A),
        scale: static (v, s) => new Color(red: v.R * s, green: v.G * s, blue: v.B * s, alpha: v.A * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
        interpolate: OklchInterpolate);

    private static Color HslInterpolate(Color a, Color b, double factor) {
        ColorHSL ha = a;
        ColorHSL hb = b;
        float tf = (float)factor;
        // Wrap hue delta into [-180, +180] to take the shortest arc around the colour wheel.
        float hueDelta = ((hb.H - ha.H + 540f) % 360f) - 180f;
        return new ColorHSL(
            hue: ha.H + (hueDelta * tf),
            saturation: ha.S + ((hb.S - ha.S) * tf),
            luminance: ha.L + ((hb.L - ha.L) * tf),
            alpha: ha.A + ((hb.A - ha.A) * tf));
    }

    // --- [OKLAB CONVERSION] ----------------------------------------------------------------
    // Björn Ottosson 2020: https://bottosson.github.io/posts/oklab/
    // sRGB → linear → LMS → OKLab and back. Each conversion is 3 multiplies + a sum per channel;
    // total per-interpolation cost is ~12 cube roots + ~24 multiply-adds. Negligible at UI rates.
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
        // Convert to polar (L, C, H); interpolate hue with shortest-arc wrap.
        float ca = MathF.Sqrt((aa * aa) + (ba * ba));
        float cb = MathF.Sqrt((ab * ab) + (bb * bb));
        float ha = MathF.Atan2(ba, aa);
        float hb = MathF.Atan2(bb, ab);
        float hueDelta = ((hb - ha + (3f * MathF.PI)) % (2f * MathF.PI)) - MathF.PI;
        float tf = (float)factor;
        float hMix = ha + (hueDelta * tf);
        float lMix = la + ((lb - la) * tf);
        float cMix = ca + ((cb - ca) * tf);
        // Back to rectangular (a, b) and into sRGB.
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

// Internal delegate-driven IMotionVector<T> carrier. Named *Impl to avoid clashing with
// System.Numerics.Vector<T> SIMD primitive — collision matters once any caller imports
// System.Numerics for vectorized math.
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
    public static ISparkle Face(RectangleF face, bool attachedToContent = false) =>
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
    // Frame-rate clamp on the integrator dt. Prevents spring instability when the window stalls
    // (e.g., debugger break, long GC pause). Equivalent to "minimum 30 fps" assumption.
    internal const float MaxFrameDelta = 1f / 30f;

    internal static GrasshopperUiIntent<Subscription> Tween<TValue>(Animated<TValue> animated, Action<TValue> sink) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Tween)), detail: "sink delegate is required"))
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: _ => Try.lift(f: () => { validSink(canvas.Animate(animated: animated)); return unit; })
                    .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Tween)), detail: $"tick threw: {error.Message}"))).Run(scope: scope)
            select sub);

    internal static GrasshopperUiIntent<SpringHandle<TValue>> Spring<TValue>(
        TValue start, TValue target, SpringConfig config, IMotionVector<TValue> vector,
        Action<TValue> sink, Option<TValue> initialVelocity, TimeProvider? clock = null, bool useDisplayLink = false) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "sink delegate is required"))
            from pacer in useDisplayLink
                ? Pacer.Of(canvas: canvas).Map(p => Some(p))
                : Fin.Succ(Option<Pacer>.None)
            let resolvedClock = clock ?? TimeProvider.System
            let cell = Atom(new SpringRunnerState<TValue>(
                Value: start,
                Velocity: initialVelocity.IfNone(validVector.Zero),
                Target: target,
                Config: config,
                Vector: validVector,
                Sink: validSink,
                Clock: resolvedClock,
                Timestamp: resolvedClock.GetTimestamp()))
            let runner = MotionRunner<SpringRunnerState<TValue>>.Of(cell: cell, canvas: canvas, pacer: pacer)
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: _ => Try.lift(runner.Tick)
                    .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Spring)), detail: $"tick threw: {error.Message}"))).Run(scope: scope)
            let bundle = SpringBundleOf(pacer: pacer, sub: sub, canvas: canvas)
            from initial in Op.Of(name: nameof(Spring)).Attempt(
                body: bundle.Wake,
                what: "Spring initial wake")
                .MapFail(error => UiFault.ThreadMarshal(detail: error.Message))
            select new SpringHandle<TValue>(
                cell: cell,
                subscription: bundle.Subscription,
                wake: bundle.Wake));

    // Composes the Spring subscription + wake closure given an optional Pacer. Centralizing the
    // Pacer-vs-canvas routing here keeps the Spring expression-bodied without Option.Match
    // mid-pipeline (CSP0705 forbidden). Subscription.Atom returns a Subscription that is owned
    // through the `|` operator by the composite — no separate Dispose; CA2000 is a false positive.
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "Subscription.Atom is consumed by the Subscription `|` operator and disposed via the returned composite.")]
    private static (Subscription Subscription, Action Wake) SpringBundleOf(
        Option<Pacer> pacer, Subscription sub, Grasshopper2.UI.Canvas.Canvas canvas) {
        if (pacer is not { IsSome: true, Case: Pacer p }) {
            return (Subscription: sub, Wake: canvas.ScheduleRedraw);
        }
        Subscription composite = sub | Subscription.Atom(detach: p.Dispose);
        return (Subscription: composite, WakeFn);
        void WakeFn() => p.Resume().Ignore();
    }

    internal static GrasshopperUiIntent<Subscription> Pulse<TValue>(
        TValue start, TValue target, GhDuration duration, GhMotion easing, int cycles, bool yoyo, bool infinite,
        IMotionVector<TValue> vector, Action<TValue> sink) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "sink delegate is required"))
            from validCycles in infinite
                ? Fin.Succ(1)
                : Optional(cycles).Filter(static c => c >= 1)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Pulse)), detail: "cycles must be positive when not infinite"))
            let initial = Animated<TValue>.CreateUnfinished(
                value0: start, value1: target,
                duration: Animators.DurationToTimeSpan(duration: duration),
                motion: easing,
                interpolator: validVector.Interpolate)
            let cell = Atom(new PulseRunnerState<TValue>(
                Animated: initial,
                From: start, To: target,
                Duration: duration, Easing: easing,
                Yoyo: yoyo, Infinite: infinite,
                CyclesRemaining: validCycles - 1,
                Vector: validVector,
                Sink: validSink))
            let runner = MotionRunner<PulseRunnerState<TValue>>.Of(cell: cell, canvas: canvas)
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: _ => Try.lift(runner.Tick)
                    .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Pulse)), detail: $"tick threw: {error.Message}"))).Run(scope: scope)
            from kickoff in Try.lift(f: () => { canvas.ScheduleRedraw(); return unit; }).Run()
                .MapFail(error => UiFault.ThreadMarshal(detail: $"{nameof(Pulse)} initial redraw threw: {error.Message}"))
            select sub);

    internal static GrasshopperUiIntent<Subscription> Stroke(
        AnimatedPath path, GhDuration duration, GhMotion easing, PaintStyle style,
        PointF origin, float scale, float angle) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validPath in Optional(path).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Stroke)), detail: "path is required"))
            from validOrigin in Op.Of(name: nameof(Stroke)).AcceptPoint(value: origin, detail: "non-finite origin")
            let progress = Animators.Unfinished(value0: 0.0, value1: 1.0, duration: duration, motion: easing)
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.AfterObjects,
                paint: paintScope => Try.lift(f: () => {
                    using Pen pen = style.Pen();
                    validPath.Draw(paintScope.Graphics.Content, pen, canvas.Animate(animated: progress), validOrigin, scale, angle);
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Stroke)), detail: $"draw threw: {error.Message}"))).Run(scope: scope)
            select sub);

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
        Skin start, Skin target, GhDuration duration, GhMotion easing, Action<Skin> sink) =>
        Tween(
            animated: Animated<Skin>.CreateUnfinished(
                value0: start, value1: target,
                duration: Animators.DurationToTimeSpan(duration: duration),
                motion: easing,
                interpolator: static (a, b, t) => a.Interpolate(b, (float)t)),
            sink: sink);

    internal static GrasshopperUiIntent<Unit> Navigate(PointF centre, GhDuration duration, float minZoom, float maxZoom) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validCentre in Op.Of(name: nameof(Navigate)).AcceptPoint(value: centre, detail: "non-finite centre")
            from validMin in Op.Of(name: nameof(Navigate)).AcceptFinite(value: minZoom, detail: "min zoom must be finite and positive", requirePositive: true)
            from validMax in Op.Of(name: nameof(Navigate)).AcceptFinite(value: maxZoom, detail: "max zoom must be finite and positive", requirePositive: true)
            from _ in validMax >= validMin
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Navigate)), detail: "max zoom below min zoom"))
            from __ in Try.lift(f: () => { canvas.Navigate(point: validCentre, zoomLimits: (validMin, validMax), duration: duration); return unit; }).Run()
                .MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Navigate)), detail: $"navigate threw: {error.Message}"))
            select unit);

    internal static GrasshopperUiIntent<Subscription> ZoomGate(ZoomThreshold threshold, Action<float> sink) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ZoomGate)), detail: "sink delegate is required"))
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: paintScope => Try.lift(f: () => {
                    validSink(canvas.AnimatedZoomFactor(threshold: threshold));
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(ZoomGate)), detail: $"tick threw: {error.Message}"))).Run(scope: scope)
            select sub);

    // Unified motion capsule: state owns Step/Emit/IsActive/MergeInto; runner owns the cell+canvas
    // plumbing, the cached applyScratch closure, and the redraw schedule. Per-tick allocations are
    // limited to the unavoidable Atom Box<A> per swap — no per-frame closure capture. The optional
    // `pacer` routes wake/rest through CADisplayLink (vsync-aligned 60/120Hz adaptive); when None
    // the runner falls back to `canvas.ScheduleRedraw()` (message-loop coalesced).
    internal sealed class MotionRunner<TState> where TState : struct, IMotionState<TState> {
        private readonly Atom<TState> cell;
        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly Option<Pacer> pacer;
        private readonly Func<TState, TState> applyScratch;
        private TState scratch;

        private MotionRunner(Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer) {
            this.cell = cell;
            this.canvas = canvas;
            this.pacer = pacer;
            // Block body re-reads the mutable `scratch` field on every invocation. A method-group form
            // `scratch.MergeInto` would bind the struct receiver at construction (to default(TState))
            // and freeze the merge; the explicit read here is load-bearing.
            applyScratch = live => {
                TState current = scratch;
                return current.MergeInto(live: live);
            };
        }

        internal static MotionRunner<TState> Of(Atom<TState> cell, Grasshopper2.UI.Canvas.Canvas canvas, Option<Pacer> pacer = default) =>
            new(cell: cell, canvas: canvas, pacer: pacer);

        internal Unit Tick() {
            TState next = cell.Value.Step();
            _ = next.Emit();
            scratch = next;
            _ = cell.Swap(applyScratch);
            _ = next.IsActive ? Wake() : Sleep();
            return unit;
        }

        // Pacer-aware wake: CADisplayLink resumes vsync ticks (preferred=120Hz adaptive on ProMotion);
        // falls back to ScheduleRedraw when no Pacer is attached. Null-routing instead of Option.Match
        // because CSP0705 forbids Match mid-pipeline; the IsSome+Case pattern keeps the analyzer happy.
        private Unit Wake() {
            if (pacer is { IsSome: true, Case: Pacer p }) {
                return p.Resume();
            }
            canvas.ScheduleRedraw();
            return unit;
        }

        // Pacer-aware quiescence: Paused link consumes 0 CPU until next Retarget wakes it. Without
        // Pacer we simply stop calling ScheduleRedraw (Eto paint loop naturally idles).
        private Unit Sleep() => pacer is { IsSome: true, Case: Pacer p } ? p.Pause() : unit;
    }

    // CADisplayLink-driven scheduler. Wraps a view-bound display link that auto-tracks the screen the
    // canvas is on (per WWDC23: NSView.GetDisplayLink retunes preferredFrameRateRange across multi-
    // monitor moves). Tick handler is an instance method exported via [Export("tick:")] so AppKit
    // dispatches the selector without a managed-delegate trampoline. Tick body just calls
    // canvas.ScheduleRedraw() — the actual integration runs in the paint hook (CanvasPaintPhase.
    // BeforeBackground), preserving the MotionRunner architecture and TimeProvider-driven dt.
    [SupportedOSPlatform("macos14.0")]
    internal sealed class Pacer : NSObject {
        private static readonly Selector TickSelector = new("tick:");

        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly CADisplayLink link;
        private int active;

        private Pacer(Grasshopper2.UI.Canvas.Canvas canvas, NSView view, CAFrameRateRange range) {
            this.canvas = canvas;
            link = view.GetDisplayLink(target: this, selector: TickSelector);
            link.PreferredFrameRateRange = range;
            link.Paused = true;
            link.AddToRunLoop(runloop: NSRunLoop.Main, mode: NSRunLoopMode.Common.GetConstant()!);
        }

        // Construct via factory so the NSView cast failure surfaces on the Fin rail instead of
        // throwing into the caller. Default frame-rate range is 30-120 preferred 120 — covers
        // ProMotion adaptive on M-series MacBook Pro / Studio Display without forcing 60Hz floors.
        internal static Fin<Pacer> Of(Grasshopper2.UI.Canvas.Canvas canvas, CAFrameRateRange? rate = null) =>
            Optional(canvas.ControlObject as NSView)
                .ToFin(Fail: UiFault.MutationRejected(
                    op: Op.Of(name: nameof(Pacer)),
                    detail: "canvas.ControlObject is not an NSView"))
                .Bind(view => Op.Of(name: nameof(Pacer)).Attempt(
                    body: () => new Pacer(
                        canvas: canvas,
                        view: view,
                        range: rate ?? CAFrameRateRange.Create(minimum: 30f, maximum: 120f, preferred: 120f)),
                    what: "Pacer construction"));

        [Export("tick:")]
        public void Tick(CADisplayLink sender) => canvas.ScheduleRedraw();

        internal Unit Resume() {
            _ = Interlocked.Exchange(ref active, 1) == 0 ? Apply(paused: false) : unit;
            return unit;
        }

        internal Unit Pause() {
            _ = Interlocked.Exchange(ref active, 0) == 1 ? Apply(paused: true) : unit;
            return unit;
        }

        private Unit Apply(bool paused) { link.Paused = paused; return unit; }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                link.Paused = true;
                link.Invalidate();
                link.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
