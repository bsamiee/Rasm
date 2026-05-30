using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AppKit;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using DrawingColor = System.Drawing.Color;
using DrawingPointF = System.Drawing.PointF;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public interface IMotionVector<TValue, TVelocity> {
    public TVelocity ZeroVelocity { get; }
    public double RestEpsilon { get; }
    public TVelocity Delta(TValue from, TValue target);
    public TVelocity Add(TVelocity a, TVelocity b);
    public TVelocity Scale(TVelocity value, double scalar);
    public TValue Move(TValue value, TVelocity delta);
    public double Norm(TVelocity value);
    public TValue Lerp(TValue a, TValue b, double t);   // replaces GH2 Interpolate
    public double Distance(TValue a, TValue b) => Norm(Delta(from: a, target: b));   // default: ‖b−a‖ — one call for rest/settle predicates
}

public interface IMotionVector<T> : IMotionVector<T, T> {
}

internal interface IMotionState<TSelf> where TSelf : struct, IMotionState<TSelf> {
    public TSelf Step();
    public Unit Emit();
    public bool IsActive { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// Robert Penner closed-form curves (easings.net). GH2 native-blended curves dropped; pure Func<double,double>.
[SmartEnum<int>]
public sealed partial class Easing {
    private const double BackC1 = 1.70158;
    private const double BackC3 = BackC1 + 1.0;
    private const double BackC2 = BackC1 * 1.525;
    private const double BounceN1 = 7.5625;
    private const double BounceD1 = 2.75;

    public static readonly Easing Linear = new(key: 0, apply: static t => t);
    public static readonly Easing SineIn = new(key: 1, apply: static t => 1.0 - Math.Cos(t * Math.PI / 2.0)), SineOut = new(key: 2, apply: static t => Math.Sin(t * Math.PI / 2.0)), SineInOut = new(key: 3, apply: static t => -(Math.Cos(Math.PI * t) - 1.0) / 2.0);
    public static readonly Easing QuadIn = new(key: 4, apply: static t => t * t), QuadOut = new(key: 5, apply: static t => 1.0 - ((1.0 - t) * (1.0 - t))), QuadInOut = new(key: 6, apply: static t => t < 0.5 ? 2.0 * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 2.0) / 2.0));
    public static readonly Easing CubicIn = new(key: 7, apply: static t => t * t * t), CubicOut = new(key: 8, apply: static t => 1.0 - Math.Pow(1.0 - t, 3.0)), CubicInOut = new(key: 9, apply: static t => t < 0.5 ? 4.0 * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 3.0) / 2.0));
    public static readonly Easing QuartIn = new(key: 10, apply: static t => t * t * t * t), QuartOut = new(key: 11, apply: static t => 1.0 - Math.Pow(1.0 - t, 4.0)), QuartInOut = new(key: 12, apply: static t => t < 0.5 ? 8.0 * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 4.0) / 2.0));
    public static readonly Easing QuintIn = new(key: 13, apply: static t => t * t * t * t * t), QuintOut = new(key: 14, apply: static t => 1.0 - Math.Pow(1.0 - t, 5.0)), QuintInOut = new(key: 15, apply: static t => t < 0.5 ? 16.0 * t * t * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 5.0) / 2.0));
    public static readonly Easing ExpoIn = new(key: 16, apply: static t => t == 0.0 ? 0.0 : Math.Pow(2.0, (10.0 * t) - 10.0)), ExpoOut = new(key: 17, apply: static t => t == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * t)), ExpoInOut = new(key: 18, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : t < 0.5 ? Math.Pow(2.0, (20.0 * t) - 10.0) / 2.0 : (2.0 - Math.Pow(2.0, (-20.0 * t) + 10.0)) / 2.0);
    public static readonly Easing CircIn = new(key: 19, apply: static t => 1.0 - Math.Sqrt(1.0 - (t * t))), CircOut = new(key: 20, apply: static t => Math.Sqrt(1.0 - ((t - 1.0) * (t - 1.0)))), CircInOut = new(key: 21, apply: static t => t < 0.5 ? (1.0 - Math.Sqrt(1.0 - (4.0 * t * t))) / 2.0 : (Math.Sqrt(1.0 - (((-2.0 * t) + 2.0) * ((-2.0 * t) + 2.0))) + 1.0) / 2.0);
    public static readonly Easing BackIn = new(key: 22, apply: static t => (BackC3 * t * t * t) - (BackC1 * t * t)), BackOut = new(key: 23, apply: static t => 1.0 + (BackC3 * Math.Pow(t - 1.0, 3.0)) + (BackC1 * Math.Pow(t - 1.0, 2.0))), BackInOut = new(key: 24, apply: static t => t < 0.5 ? Math.Pow(2.0 * t, 2.0) * (((BackC2 + 1.0) * 2.0 * t) - BackC2) / 2.0 : ((Math.Pow((2.0 * t) - 2.0, 2.0) * (((BackC2 + 1.0) * ((t * 2.0) - 2.0)) + BackC2)) + 2.0) / 2.0);
    public static readonly Easing ElasticIn = new(key: 25, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : -Math.Pow(2.0, (10.0 * t) - 10.0) * Math.Sin(((t * 10.0) - 10.75) * (2.0 * Math.PI / 3.0))), ElasticOut = new(key: 26, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : (Math.Pow(2.0, -10.0 * t) * Math.Sin(((t * 10.0) - 0.75) * (2.0 * Math.PI / 3.0))) + 1.0), ElasticInOut = new(key: 27, apply: static t => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 : t < 0.5 ? -(Math.Pow(2.0, (20.0 * t) - 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5))) / 2.0 : (Math.Pow(2.0, (-20.0 * t) + 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5)) / 2.0) + 1.0);
    public static readonly Easing BounceIn = new(key: 28, apply: static t => 1.0 - BounceOutFormula(1.0 - t)), BounceOut = new(key: 29, apply: static t => BounceOutFormula(t)), BounceInOut = new(key: 30, apply: static t => t < 0.5 ? (1.0 - BounceOutFormula(1.0 - (2.0 * t))) / 2.0 : (1.0 + BounceOutFormula((2.0 * t) - 1.0)) / 2.0);

    [UseDelegateFromConstructor]
    public partial double Apply(double t);

    private static double BounceOutFormula(double t) =>
        t switch {
            < 1.0 / BounceD1 => BounceN1 * t * t,
            < 2.0 / BounceD1 => (BounceN1 * (t - (1.5 / BounceD1)) * (t - (1.5 / BounceD1))) + 0.75,
            < 2.5 / BounceD1 => (BounceN1 * (t - (2.25 / BounceD1)) * (t - (2.25 / BounceD1))) + 0.9375,
            _ => (BounceN1 * (t - (2.625 / BounceD1)) * (t - (2.625 / BounceD1))) + 0.984375,
        };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpringConfig(float Stiffness, float Damping, float Mass) {
    internal const double MaxDt = 1.0 / 30.0;   // dt clamp prevents spring blowup on GC/debugger stalls (30fps floor); single location

    public static Fin<SpringConfig> Response(float response, float dampingFraction, float mass = 1f) =>
        from _ in guard(float.IsFinite(response) && response > 0f && float.IsFinite(dampingFraction) && dampingFraction >= 0f && float.IsFinite(mass) && mass > 0f, Op.Of(name: nameof(Response)).InvalidInput())
        from config in Fin.Succ(value: Tuned(response: response, dampingFraction: dampingFraction, mass: mass))
        select config;

    internal static SpringConfig Tuned(float response, float dampingFraction, float mass) {
        float twoPiOverR = (float)(2.0 * Math.PI) / response;
        return new SpringConfig(Stiffness: twoPiOverR * twoPiOverR * mass, Damping: 4f * (float)Math.PI * dampingFraction * mass / response, Mass: mass);
    }
}

[SmartEnum<int>]
public sealed partial class SpringPreset {
    public SpringConfig Config { get; }
    public string Label { get; }   // user-facing picker metadata; SpringPreset.Items + Label needs no magic strings downstream

    public static readonly SpringPreset Relaxed = Of(key: 0, response: 0.65f, dampingFraction: 1.00f, label: "Relaxed");
    public static readonly SpringPreset Standard = Of(key: 1, response: 0.35f, dampingFraction: 0.90f, label: "Standard");
    public static readonly SpringPreset Expressive = Of(key: 2, response: 0.45f, dampingFraction: 0.65f, label: "Expressive");
    public static readonly SpringPreset Snappy = Of(key: 3, response: 0.30f, dampingFraction: 0.85f, label: "Snappy");
    public static readonly SpringPreset Smooth = Of(key: 4, response: 0.50f, dampingFraction: 1.00f, label: "Smooth");
    public static readonly SpringPreset Bouncy = Of(key: 5, response: 0.55f, dampingFraction: 0.50f, label: "Bouncy");
    public static readonly SpringPreset Snappier = Of(key: 6, response: 0.25f, dampingFraction: 0.85f, label: "Snappier");
    public static readonly SpringPreset Sluggish = Of(key: 7, response: 1.00f, dampingFraction: 1.20f, label: "Sluggish");

    private static SpringPreset Of(int key, float response, float dampingFraction, string label) =>
        new(key: key, config: SpringConfig.Tuned(response: response, dampingFraction: dampingFraction, mass: 1f), label: label);
}

// Semi-implicit Euler: F = -k·x - b·v ; a = F/m ; v += a·dt ; x += v·dt. TimeProvider-injected for FakeTimeProvider tests.
internal readonly record struct SpringRunnerState<TValue, TVelocity>(TValue Value, TVelocity Velocity, TValue Target, SpringConfig Config, IMotionVector<TValue, TVelocity> Vector, Action<TValue> Sink, TimeProvider Clock, long Timestamp) : IMotionState<SpringRunnerState<TValue, TVelocity>> {
    public SpringRunnerState<TValue, TVelocity> Step() {
        long now = Clock.GetTimestamp();
        double dt = Math.Min(val1: Clock.GetElapsedTime(startingTimestamp: Timestamp, endingTimestamp: now).TotalSeconds, val2: SpringConfig.MaxDt);
        TVelocity displacement = Vector.Delta(from: Target, target: Value);
        TVelocity accel = Vector.Scale(Vector.Add(Vector.Scale(displacement, -Config.Stiffness), Vector.Scale(Velocity, -Config.Damping)), 1.0 / Config.Mass);
        TVelocity newVelocity = Vector.Add(Velocity, Vector.Scale(accel, dt));
        TValue newValue = Vector.Move(value: Value, delta: Vector.Scale(newVelocity, dt));
        return this with { Value = newValue, Velocity = newVelocity, Timestamp = now };
    }
    public Unit Emit() { Sink(Value); return unit; }
    public bool IsActive => !((Vector.Distance(Value, Target) < Vector.RestEpsilon) && (Vector.Norm(Velocity) < Vector.RestEpsilon));
}

// One tween type covers both single Tween (empty Rest) and Sequence (Rest = remaining steps, advanced at boundaries).
internal readonly record struct TweenRunnerState<TValue, TVelocity>(TValue From, TValue To, TimeSpan Duration, Easing Easing, Seq<(TValue Target, TimeSpan Duration, Easing Easing)> Rest, IMotionVector<TValue, TVelocity> Vector, Action<TValue> Sink, TimeProvider Clock, long Start) : IMotionState<TweenRunnerState<TValue, TVelocity>> {
    public TweenRunnerState<TValue, TVelocity> Step() =>
        (Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds >= Duration.TotalSeconds, Rest.IsEmpty) switch {
            (true, false) => this with { From = To, To = Rest[0].Target, Duration = Rest[0].Duration, Easing = Rest[0].Easing, Rest = Rest.Tail, Start = Clock.GetTimestamp() },
            _ => this,
        };
    public Unit Emit() {
        double total = Duration.TotalSeconds;
        double t = total <= 0d ? 1d : Math.Clamp(value: Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds / total, min: 0d, max: 1d);
        Sink(Vector.Lerp(a: From, b: To, t: Easing.Apply(t: t)));
        return unit;
    }
    public bool IsActive => !Rest.IsEmpty || Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds < Duration.TotalSeconds;
}

internal readonly record struct PulseRunnerState<TValue, TVelocity>(TValue From, TValue To, TimeSpan Duration, Easing Easing, bool Yoyo, bool Infinite, int CyclesRemaining, IMotionVector<TValue, TVelocity> Vector, Action<TValue> Sink, TimeProvider Clock, long Start) : IMotionState<PulseRunnerState<TValue, TVelocity>> {
    public PulseRunnerState<TValue, TVelocity> Step() =>
        (Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds >= Duration.TotalSeconds && (Infinite || CyclesRemaining > 0)) switch {
            true => this with { From = Yoyo ? To : From, To = Yoyo ? From : To, CyclesRemaining = Infinite ? CyclesRemaining : CyclesRemaining - 1, Start = Clock.GetTimestamp() },
            false => this,
        };
    public Unit Emit() {
        double total = Duration.TotalSeconds;
        double t = total <= 0d ? 1d : Math.Clamp(value: Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds / total, min: 0d, max: 1d);
        Sink(Vector.Lerp(a: From, b: To, t: Easing.Apply(t: t)));
        return unit;
    }
    public bool IsActive => Infinite || CyclesRemaining > 0 || Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds < Duration.TotalSeconds;
}

// Inertial decay: v *= e^(-friction*dt) each step; x += v*dt. Rests once |v| drops below the vector epsilon.
internal readonly record struct DecayRunnerState<TValue, TVelocity>(TValue Value, TVelocity Velocity, double Friction, IMotionVector<TValue, TVelocity> Vector, Action<TValue> Sink, TimeProvider Clock, long Timestamp) : IMotionState<DecayRunnerState<TValue, TVelocity>> {
    public DecayRunnerState<TValue, TVelocity> Step() {
        long now = Clock.GetTimestamp();
        double dt = Math.Min(val1: Clock.GetElapsedTime(startingTimestamp: Timestamp, endingTimestamp: now).TotalSeconds, val2: SpringConfig.MaxDt);
        TVelocity velocity = Vector.Scale(Velocity, Math.Exp(-Friction * dt));
        return this with { Value = Vector.Move(value: Value, delta: Vector.Scale(velocity, dt)), Velocity = velocity, Timestamp = now };
    }
    public Unit Emit() { Sink(Value); return unit; }
    public bool IsActive => Vector.Norm(Velocity) >= Vector.RestEpsilon;
}

// IdleLoop = RhinoApp.Idle+MainLoop coalesced ~60Hz (always available). DisplayLink = CADisplayLink vsync (macOS 14+).
// UITimer = Eto.Forms.UITimer cross-platform fixed-interval clock (deterministic; no macOS-14 floor, no idle coupling).
[Union]
public abstract partial record MotionClock {
    private MotionClock() { }
    public sealed record IdleLoopCase : MotionClock;
    public sealed record DisplayLinkCase(Option<CAFrameRateRange> Rate) : MotionClock;
    public sealed record TimerCase(double IntervalSeconds) : MotionClock;

    public static readonly MotionClock IdleLoop = new IdleLoopCase();
    public static MotionClock DisplayLink(Option<CAFrameRateRange> rate = default) => new DisplayLinkCase(Rate: rate);
    // Validated bounds -> CAFrameRateRange at spec-time (invalid ranges fail the rail); macOS 14+ like the rest of the DisplayLink path.
    [SupportedOSPlatform("macos14.0")]
    public static Fin<MotionClock> DisplayLink(float min, float max, float preferred) =>
        from _ in guard(min > 0f && max >= min && preferred >= min && preferred <= max, Op.Of(name: nameof(DisplayLink)).InvalidInput())
        from clock in Fin.Succ<MotionClock>(value: new DisplayLinkCase(Rate: Some(CAFrameRateRange.Create(minimum: min, maximum: max, preferred: preferred))))
        select clock;
    public static Fin<MotionClock> UITimer(double intervalSeconds = 1.0 / 60.0) =>
        (double.IsFinite(intervalSeconds) && intervalSeconds >= 1.0 / 60.0) switch {
            true => Fin.Succ<MotionClock>(value: new TimerCase(IntervalSeconds: intervalSeconds)),
            false => Fin.Fail<MotionClock>(error: Op.Of(name: nameof(UITimer)).InvalidInput()),
        };
}

[Union]
public abstract partial record RedrawTarget {
    private RedrawTarget() { }
    public sealed record View(RhinoView Value) : RedrawTarget;
    public sealed record Document(RhinoDoc Value) : RedrawTarget;
    public sealed record Canvas(Action Invalidate) : RedrawTarget;

    internal Unit Repaint() => Switch(
        view: static v => Op.Side(() => v.Value.Redraw()),
        document: static d => Op.Side(() => d.Value.Views.Redraw()),
        canvas: static c => Op.Side(() => c.Invalidate()));
}

// Plain abstract record (NOT [Union]): generic union forces `allows ref struct` under the source generator,
// incompatible with record cases (see Commands/Command.cs PromptTransition). Motion.Run matches cases directly.
public abstract record MotionSpec<TValue, TVelocity> {
    private MotionSpec() { }
    public sealed record Spring(TValue From, TValue To, SpringConfig Config, IMotionVector<TValue, TVelocity> Vector, Option<TVelocity> InitialVelocity = default) : MotionSpec<TValue, TVelocity>;
    public sealed record Tween(TValue From, TValue To, TimeSpan Duration, Easing Easing, IMotionVector<TValue, TVelocity> Vector) : MotionSpec<TValue, TVelocity>;
    public sealed record Pulse(TValue From, TValue To, TimeSpan Duration, Easing Easing, IMotionVector<TValue, TVelocity> Vector, int Cycles = 1, bool Yoyo = false, bool Infinite = false) : MotionSpec<TValue, TVelocity>;
    public sealed record Sequence(TValue Start, Seq<(TValue Target, TimeSpan Duration, Easing Easing)> Steps, IMotionVector<TValue, TVelocity> Vector) : MotionSpec<TValue, TVelocity>;
    // Inertial deceleration: no target, friction-driven velocity decay (v *= e^(-friction*dt)); rest when |v| < ε.
    public sealed record Decay(TValue From, TVelocity Velocity, double Friction, IMotionVector<TValue, TVelocity> Vector) : MotionSpec<TValue, TVelocity>;
}

public static class MotionSpec {   // inference-friendly factories: MotionSpec.Spring(...) infers TValue
    public static MotionSpec<T, TV>.Spring Spring<T, TV>(T from, T to, SpringConfig config, IMotionVector<T, TV> vector, Option<TV> initialVelocity = default) => new(From: from, To: to, Config: config, Vector: vector, InitialVelocity: initialVelocity);
    public static MotionSpec<T, TV>.Tween Tween<T, TV>(T from, T to, TimeSpan duration, Easing easing, IMotionVector<T, TV> vector) => new(From: from, To: to, Duration: duration, Easing: easing, Vector: vector);
    public static MotionSpec<T, TV>.Pulse Pulse<T, TV>(T from, T to, TimeSpan duration, Easing easing, IMotionVector<T, TV> vector, int cycles = 1, bool yoyo = false, bool infinite = false) => new(From: from, To: to, Duration: duration, Easing: easing, Vector: vector, Cycles: cycles, Yoyo: yoyo, Infinite: infinite);
    public static MotionSpec<T, TV>.Sequence Sequence<T, TV>(T start, Seq<(T Target, TimeSpan Duration, Easing Easing)> steps, IMotionVector<T, TV> vector) => new(Start: start, Steps: steps, Vector: vector);
    public static MotionSpec<T, TV>.Decay Decay<T, TV>(T from, TV velocity, double friction, IMotionVector<T, TV> vector) => new(From: from, Velocity: velocity, Friction: friction, Vector: vector);
}

// Live-steerable, disposable handle over a running motion. Retarget CASes the runner Atom, then re-attaches the
// driver ONLY on the rested edge (no per-frame re-subscribe while active, no leak). Mirrors the sleep/wake of
// Rasm.Grasshopper.UI.SpringHandle; IDisposable so bare fire-and-forget sites compile.
public sealed class MotionHandle<TValue, TVelocity> : IDisposable {
    internal readonly record struct Steering(Func<TValue, Option<TVelocity>, Fin<Unit>> Retarget, Func<TVelocity> Velocity);

    private readonly IDisposable disposer;
    private readonly Func<Fin<Unit>> wake;
    private readonly Steering steering;

    internal MotionHandle(IDisposable disposer, Func<Fin<Unit>> wake, Steering steering) {
        this.disposer = disposer;
        this.wake = wake;
        this.steering = steering;
    }

    public TVelocity Velocity => steering.Velocity();
    public Fin<Unit> Retarget(TValue target, Option<TVelocity> velocity = default) =>
        steering.Retarget(arg1: target, arg2: velocity).Bind(_ => wake());
    public void Dispose() => disposer.Dispose();
}

// --- [VECTORS] ----------------------------------------------------------------------------
internal sealed class MotionVectorImpl<T>(T zero, double restEpsilon, Func<T, T, T> add, Func<T, T, T> subtract, Func<T, double, T> scale, Func<T, double> norm, Func<T, T, double, T> lerp) : IMotionVector<T> {
    public T ZeroVelocity { get; } = zero;
    public double RestEpsilon { get; } = restEpsilon;
    public T Add(T a, T b) => add(arg1: a, arg2: b);
    public T Delta(T from, T target) => subtract(arg1: target, arg2: from);
    public T Scale(T value, double scalar) => scale(arg1: value, arg2: scalar);
    public T Move(T value, T delta) => add(arg1: value, arg2: delta);
    public double Norm(T value) => norm(arg: value);
    public T Lerp(T a, T b, double t) => lerp(arg1: a, arg2: b, arg3: t);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct MotionColor(double A, double R, double G, double B) {
    public DrawingColor Project() =>
        DrawingColor.FromArgb(alpha: Channel(value: A), red: Channel(value: R), green: Channel(value: G), blue: Channel(value: B));

    private static int Channel(double value) =>
        Math.Clamp(value: (int)Math.Round(value), min: 0, max: 255);
}

internal sealed class MotionColorVector : IMotionVector<DrawingColor, MotionColor> {
    public MotionColor ZeroVelocity { get; } = new(A: 0.0, R: 0.0, G: 0.0, B: 0.0);
    public double RestEpsilon => 1.0;
    public MotionColor Delta(DrawingColor from, DrawingColor target) =>
        new(A: target.A - from.A, R: target.R - from.R, G: target.G - from.G, B: target.B - from.B);
    public MotionColor Add(MotionColor a, MotionColor b) =>
        new(A: a.A + b.A, R: a.R + b.R, G: a.G + b.G, B: a.B + b.B);
    public MotionColor Scale(MotionColor value, double scalar) =>
        new(A: value.A * scalar, R: value.R * scalar, G: value.G * scalar, B: value.B * scalar);
    public DrawingColor Move(DrawingColor value, MotionColor delta) =>
        new MotionColor(A: value.A + delta.A, R: value.R + delta.R, G: value.G + delta.G, B: value.B + delta.B).Project();
    public double Norm(MotionColor value) =>
        Math.Max(Math.Max(Math.Abs(value.R), Math.Abs(value.G)), Math.Max(Math.Abs(value.B), Math.Abs(value.A)));
    public DrawingColor Lerp(DrawingColor a, DrawingColor b, double t) =>
        MotionVector.OklabInterpolate(a: a, b: b, factor: t);
}

public static class MotionVector {
    private const double ScalarRest = 0.001;
    private const double PixelRest = 0.5;
    private const double SpatialRest = 0.001;
    private const double MatrixRest = 1e-4;

    // One inferred factory feeds every per-type field (T fixed by `zero`); concrete return (CA1859) — fields widen to IMotionVector<T>.
    private static MotionVectorImpl<T> Of<T>(T zero, double restEpsilon, Func<T, T, T> add, Func<T, T, T> sub, Func<T, double, T> scale, Func<T, double> norm, Func<T, T, double, T> lerp) =>
        new(zero: zero, restEpsilon: restEpsilon, add: add, subtract: sub, scale: scale, norm: norm, lerp: lerp);

    public static readonly IMotionVector<double> Double = Of(
        zero: 0.0, restEpsilon: ScalarRest,
        add: static (a, b) => a + b, sub: static (a, b) => a - b, scale: static (v, s) => v * s,
        norm: static v => Math.Abs(value: v), lerp: static (a, b, t) => a + ((b - a) * t));
    public static readonly IMotionVector<float> Float = Of(
        zero: 0f, restEpsilon: ScalarRest,
        add: static (a, b) => a + b, sub: static (a, b) => a - b, scale: static (v, s) => (float)(v * s),
        norm: static v => Math.Abs(value: v), lerp: static (a, b, t) => (float)(a + ((b - a) * t)));
    public static readonly IMotionVector<DrawingPointF> PointF = Of(
        zero: DrawingPointF.Empty, restEpsilon: PixelRest,
        add: static (a, b) => new DrawingPointF(a.X + b.X, a.Y + b.Y),
        sub: static (a, b) => new DrawingPointF(a.X - b.X, a.Y - b.Y),
        scale: static (v, s) => new DrawingPointF((float)(v.X * s), (float)(v.Y * s)),
        norm: static v => Math.Sqrt((v.X * v.X) + (v.Y * v.Y)),
        lerp: static (a, b, t) => new DrawingPointF((float)(a.X + ((b.X - a.X) * t)), (float)(a.Y + ((b.Y - a.Y) * t))));
    public static readonly IMotionVector<Point3d> Point = Of(
        zero: Point3d.Origin, restEpsilon: SpatialRest,
        add: static (a, b) => new Point3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z),
        sub: static (a, b) => new Point3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z),
        scale: static (v, s) => new Point3d(v.X * s, v.Y * s, v.Z * s),
        norm: static v => Math.Sqrt((v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z)),
        lerp: static (a, b, t) => new Point3d(a.X + ((b.X - a.X) * t), a.Y + ((b.Y - a.Y) * t), a.Z + ((b.Z - a.Z) * t)));
    public static readonly IMotionVector<Vector3d> Vector = Of(
        zero: Vector3d.Zero, restEpsilon: SpatialRest,
        add: static (a, b) => a + b, sub: static (a, b) => a - b, scale: static (v, s) => v * s,
        norm: static v => v.Length,
        lerp: static (a, b, t) => a + ((b - a) * t));
    public static readonly IMotionVector<Transform> Matrix = Of(
        zero: Transform.ZeroTransformation, restEpsilon: MatrixRest,
        add: static (a, b) => Zip(a, b, static (x, y) => x + y),
        sub: static (a, b) => Zip(a, b, static (x, y) => x - y),
        scale: static (v, s) => Map(v, x => x * s),
        norm: static v => toSeq(Enumerable.Range(start: 0, count: 16)).Fold(0.0, (acc, i) => Math.Max(val1: acc, val2: Math.Abs(value: v[i / 4, i % 4]))),
        lerp: static (a, b, t) => Zip(a, b, (x, y) => x + ((y - x) * t)));
    public static readonly IMotionVector<DrawingColor, MotionColor> Color = new MotionColorVector();

    // BOUNDARY ADAPTER - Transform is a 4x4 value matrix; Fold threads it by value, mutating via indexer per cell.
    private static Transform Zip(Transform a, Transform b, Func<double, double, double> f) =>
        toSeq(Enumerable.Range(start: 0, count: 16)).Fold(Transform.ZeroTransformation, (acc, i) => { acc[i / 4, i % 4] = f(arg1: a[i / 4, i % 4], arg2: b[i / 4, i % 4]); return acc; });
    private static Transform Map(Transform a, Func<double, double> f) =>
        toSeq(Enumerable.Range(start: 0, count: 16)).Fold(Transform.ZeroTransformation, (acc, i) => { acc[i / 4, i % 4] = f(arg: a[i / 4, i % 4]); return acc; });
    private static int Channel(float normalized) => Math.Clamp(value: (int)Math.Round(normalized * 255f), min: 0, max: 255);

    // OKLab constants MUST match Rasm.Grasshopper.UI.MotionVector verbatim (Ottosson 2020 / CSS Color Level 4).
    internal static DrawingColor OklabInterpolate(DrawingColor a, DrawingColor b, double factor) {
        (float la, float aa, float ba) = SrgbToOklab(a);
        (float lb, float ab, float bb) = SrgbToOklab(b);
        float tf = (float)factor;
        (float L, float A, float B) mix = (L: la + ((lb - la) * tf), A: aa + ((ab - aa) * tf), B: ba + ((bb - ba) * tf));
        return OklabToSrgb(mix, alpha: (int)Math.Round(a.A + ((b.A - a.A) * factor)));
    }
    private static (float L, float A, float B) SrgbToOklab(DrawingColor rgb) {
        float r = SrgbToLinear(rgb.R / 255f);
        float g = SrgbToLinear(rgb.G / 255f);
        float bl = SrgbToLinear(rgb.B / 255f);
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
    private static DrawingColor OklabToSrgb((float L, float A, float B) lab, int alpha) {
        float l_ = lab.L + (0.3963377774f * lab.A) + (0.2158037573f * lab.B);
        float m_ = lab.L - (0.1055613458f * lab.A) - (0.0638541728f * lab.B);
        float s_ = lab.L - (0.0894841775f * lab.A) - (1.2914855480f * lab.B);
        float l = l_ * l_ * l_;
        float m = m_ * m_ * m_;
        float s = s_ * s_ * s_;
        float r = (4.0767416621f * l) - (3.3077115913f * m) + (0.2309699292f * s);
        float g = (-1.2684380046f * l) + (2.6097574011f * m) - (0.3413193965f * s);
        float bl = (-0.0041960863f * l) - (0.7034186147f * m) + (1.7076147010f * s);
        return DrawingColor.FromArgb(
            alpha: Math.Clamp(value: alpha, min: 0, max: 255),
            red: Channel(LinearToSrgb(Math.Clamp(r, 0f, 1f))),
            green: Channel(LinearToSrgb(Math.Clamp(g, 0f, 1f))),
            blue: Channel(LinearToSrgb(Math.Clamp(bl, 0f, 1f))));
    }
    private static float SrgbToLinear(float c) => c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    private static float LinearToSrgb(float c) => c <= 0.0031308f ? 12.92f * c : (1.055f * MathF.Pow(c, 1f / 2.4f)) - 0.055f;
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class Motion {
    internal static Fin<MotionHandle<TValue, TVelocity>> Run<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, Action<TValue> sink, RedrawTarget target, MotionClock clock, TimeProvider? timeSource = null) {
        TimeProvider resolved = timeSource ?? TimeProvider.System;
        // WCAG reduced-motion (macOS 14+): snap to the terminal value once and return an inert handle.
        return from validSink in Op.Of(name: nameof(Run)).Need(sink)
               from validSpec in Op.Of(name: nameof(Run)).Need(spec)
               from validClock in clock switch {
                   MotionClock.TimerCase timer when !double.IsFinite(timer.IntervalSeconds) || timer.IntervalSeconds < 1.0 / 60.0 => Fin.Fail<MotionClock>(error: Op.Of(name: nameof(Run)).InvalidInput()),
                   _ => Fin.Succ(value: clock),
               }
               from handle in ReduceMotion
                   ? Settle(spec: validSpec, sink: validSink)
                   : Dispatch(spec: validSpec, sink: validSink, target: target, clock: validClock, resolved: resolved)
               select handle;
    }

    private static bool ReduceMotion =>
        OperatingSystem.IsMacOSVersionAtLeast(major: 14) && ShouldReduceMotion();

    [SupportedOSPlatform("macos14.0")]
    private static bool ShouldReduceMotion() => NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceMotion;

    private static Fin<MotionHandle<TValue, TVelocity>> Dispatch<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, Action<TValue> sink, RedrawTarget target, MotionClock clock, TimeProvider resolved) =>
        spec switch {
            MotionSpec<TValue, TVelocity>.Spring s => Drive(
                initial: MakeSpring(spec: s, sink: sink, clock: resolved), target: target, clock: clock,
                steering: cell => new MotionHandle<TValue, TVelocity>.Steering(
                    Retarget: (toValue, velocity) => Fin.Succ(value: Op.Side(() => cell.Swap(state => state with { Target = toValue, Velocity = velocity.IfNone(state.Velocity) }))),
                    Velocity: () => cell.Value.Velocity)),
            MotionSpec<TValue, TVelocity>.Decay d => Drive(
                initial: MakeDecay(spec: d, sink: sink, clock: resolved), target: target, clock: clock,
                steering: cell => new MotionHandle<TValue, TVelocity>.Steering(
                    Retarget: (_, velocity) => Fin.Succ(value: Op.Side(() => cell.Swap(state => state with { Velocity = velocity.IfNone(state.Velocity) }))),
                    Velocity: () => cell.Value.Velocity)),
            MotionSpec<TValue, TVelocity>.Tween t => Drive(
                initial: MakeTween(spec: t, sink: sink, clock: resolved), target: target, clock: clock, steering: ReseedTween(vector: t.Vector)),
            MotionSpec<TValue, TVelocity>.Pulse p => Drive(
                initial: MakePulse(spec: p, sink: sink, clock: resolved), target: target, clock: clock, steering: ReseedPulse(vector: p.Vector)),
            MotionSpec<TValue, TVelocity>.Sequence q => Drive(
                initial: MakeSequence(spec: q, sink: sink, clock: resolved), target: target, clock: clock, steering: ReseedTween(vector: q.Vector)),
            _ => Fin.Fail<MotionHandle<TValue, TVelocity>>(error: Op.Of(name: nameof(Run)).InvalidInput()),
        };

    private static Fin<MotionHandle<TValue, TVelocity>> Settle<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, Action<TValue> sink) {
        Fin<(TValue Terminal, IMotionVector<TValue, TVelocity> Vector)> resolved = spec switch {
            MotionSpec<TValue, TVelocity>.Spring s => Fin.Succ(value: (s.To, s.Vector)),
            MotionSpec<TValue, TVelocity>.Decay d => Fin.Succ(value: (d.From, d.Vector)),
            MotionSpec<TValue, TVelocity>.Tween t => Fin.Succ(value: (t.To, t.Vector)),
            MotionSpec<TValue, TVelocity>.Pulse p => Fin.Succ(value: (p.To, p.Vector)),
            MotionSpec<TValue, TVelocity>.Sequence q => Fin.Succ(value: (q.Steps.IsEmpty ? q.Start : q.Steps[q.Steps.Count - 1].Target, q.Vector)),
            _ => Fin.Fail<(TValue, IMotionVector<TValue, TVelocity>)>(error: Op.Of(name: nameof(Run)).InvalidInput()),
        };
        return resolved.Map(pair => {
            sink(pair.Terminal);
            return new MotionHandle<TValue, TVelocity>(
                disposer: Subscription.Empty,
                wake: static () => Fin.Succ(value: unit),
                steering: new MotionHandle<TValue, TVelocity>.Steering(
                    Retarget: (toValue, _) => Fin.Succ(value: Op.Side(() => sink(toValue))),
                    Velocity: () => pair.Vector.ZeroVelocity));
        });
    }

    private static SpringRunnerState<T, TV> MakeSpring<T, TV>(MotionSpec<T, TV>.Spring spec, Action<T> sink, TimeProvider clock) =>
        new(Value: spec.From, Velocity: spec.InitialVelocity.IfNone(spec.Vector.ZeroVelocity), Target: spec.To, Config: spec.Config, Vector: spec.Vector, Sink: sink, Clock: clock, Timestamp: clock.GetTimestamp());
    private static TweenRunnerState<T, TV> MakeTween<T, TV>(MotionSpec<T, TV>.Tween spec, Action<T> sink, TimeProvider clock) =>
        new(From: spec.From, To: spec.To, Duration: spec.Duration, Easing: spec.Easing, Rest: Seq<(T, TimeSpan, Easing)>(), Vector: spec.Vector, Sink: sink, Clock: clock, Start: clock.GetTimestamp());
    private static PulseRunnerState<T, TV> MakePulse<T, TV>(MotionSpec<T, TV>.Pulse spec, Action<T> sink, TimeProvider clock) =>
        new(From: spec.From, To: spec.To, Duration: spec.Duration, Easing: spec.Easing, Yoyo: spec.Yoyo, Infinite: spec.Infinite, CyclesRemaining: Math.Max(val1: 0, val2: spec.Cycles - 1), Vector: spec.Vector, Sink: sink, Clock: clock, Start: clock.GetTimestamp());
    private static TweenRunnerState<T, TV> MakeSequence<T, TV>(MotionSpec<T, TV>.Sequence spec, Action<T> sink, TimeProvider clock) =>
        spec.Steps.IsEmpty
            ? new(From: spec.Start, To: spec.Start, Duration: TimeSpan.Zero, Easing: Easing.Linear, Rest: Seq<(T, TimeSpan, Easing)>(), Vector: spec.Vector, Sink: sink, Clock: clock, Start: clock.GetTimestamp())
            : new(From: spec.Start, To: spec.Steps[0].Target, Duration: spec.Steps[0].Duration, Easing: spec.Steps[0].Easing, Rest: spec.Steps.Tail, Vector: spec.Vector, Sink: sink, Clock: clock, Start: clock.GetTimestamp());
    private static DecayRunnerState<T, TV> MakeDecay<T, TV>(MotionSpec<T, TV>.Decay spec, Action<T> sink, TimeProvider clock) =>
        new(Value: spec.From, Velocity: spec.Velocity, Friction: spec.Friction, Vector: spec.Vector, Sink: sink, Clock: clock, Timestamp: clock.GetTimestamp());

    // Tween/Sequence live-steer: re-seed From <- the current interpolated value, To <- the new target, restart the clock.
    private static Func<Atom<TweenRunnerState<T, TV>>, MotionHandle<T, TV>.Steering> ReseedTween<T, TV>(IMotionVector<T, TV> vector) =>
        cell => new MotionHandle<T, TV>.Steering(
            Retarget: (toValue, _) => Fin.Succ(value: Op.Side(() => cell.Swap(state => state with {
                From = state.Vector.Lerp(a: state.From, b: state.To, t: state.Easing.Apply(t: Progress(duration: state.Duration, start: state.Start, clock: state.Clock))),
                To = toValue,
                Rest = Seq<(T, TimeSpan, Easing)>(),
                Start = state.Clock.GetTimestamp(),
            }))),
            Velocity: () => vector.ZeroVelocity);

    private static Func<Atom<PulseRunnerState<T, TV>>, MotionHandle<T, TV>.Steering> ReseedPulse<T, TV>(IMotionVector<T, TV> vector) =>
        cell => new MotionHandle<T, TV>.Steering(
            Retarget: (toValue, _) => Fin.Succ(value: Op.Side(() => cell.Swap(state => state with {
                From = state.Vector.Lerp(a: state.From, b: state.To, t: state.Easing.Apply(t: Progress(duration: state.Duration, start: state.Start, clock: state.Clock))),
                To = toValue,
                Start = state.Clock.GetTimestamp(),
            }))),
            Velocity: () => vector.ZeroVelocity);

    private static double Progress(TimeSpan duration, long start, TimeProvider clock) =>
        duration.TotalSeconds <= 0d ? 1d : Math.Clamp(value: clock.GetElapsedTime(startingTimestamp: start).TotalSeconds / duration.TotalSeconds, min: 0d, max: 1d);

    private static Fin<MotionHandle<TValue, TVelocity>> Drive<TState, TValue, TVelocity>(TState initial, RedrawTarget target, MotionClock clock, Func<Atom<TState>, MotionHandle<TValue, TVelocity>.Steering> steering) where TState : struct, IMotionState<TState> {
        Atom<TState> cell = Atom(initial);   // runner + steering share one cell; Retarget CASes it, pump re-attaches on the rested edge
        MotionRunner<TState> runner = MotionRunner<TState>.Of(cell: cell);
        MotionPump? pump = new();
        pump.Bind(attachFactory: p => clock.Switch(
            idleLoopCase: _ => IdleDriver(runner: runner, target: target, pump: p),
            displayLinkCase: link => DisplayOrFallback(runner: runner, target: target, rate: link.Rate, pump: p),
            timerCase: timer => UiTimerDriver(runner: runner, target: target, interval: timer.IntervalSeconds, pump: p)));
        // BOUNDARY ADAPTER - pump ownership transfers to the handle on success; dispose it if Begin never attached a driver.
        try {
            return pump.Begin().Map(_ => {
                MotionPump owned = pump!;
                pump = null;
                return new MotionHandle<TValue, TVelocity>(disposer: owned, wake: owned.Wake, steering: steering(arg: cell));
            });
        } finally {
            pump?.Dispose();
        }
    }

    private static Fin<IDisposable> IdleDriver<TState>(MotionRunner<TState> runner, RedrawTarget target, MotionPump pump) where TState : struct, IMotionState<TState> =>
        // BOUNDARY ADAPTER - RhinoApp.Idle/MainLoop are native events; the handler self-detaches at rest (pump.Rest).
        Op.Of(name: nameof(Run)).Catch(() => {
            void Tick(object? sender, EventArgs args) {
                bool active = runner.Tick();
                _ = target.Repaint();
                _ = active ? unit : pump.Rest();
            }
            EventHandler handler = Tick;   // single delegate instance for symmetric +=/-=
            RhinoApp.Idle += handler;
            RhinoApp.MainLoop += handler;
            Subscription box = new(detachers: Seq(() => RhinoApp.Idle -= handler, () => RhinoApp.MainLoop -= handler));
            pump.Adopt(detacher: box);   // store + mark active BEFORE the initial tick can rest
            _ = runner.Tick();   // initial emit of the start value
            return Fin.Succ<IDisposable>(value: box);
        });

    private static Fin<IDisposable> UiTimerDriver<TState>(MotionRunner<TState> runner, RedrawTarget target, double interval, MotionPump pump) where TState : struct, IMotionState<TState> =>
        // BOUNDARY ADAPTER - Eto.Forms.UITimer is a native fixed-interval clock; the handler self-detaches at rest (pump.Rest).
        Op.Of(name: nameof(Run)).Catch(() => {
            Eto.Forms.UITimer timer = new() { Interval = interval };
            void Tick(object? sender, EventArgs args) {
                bool active = runner.Tick();
                _ = target.Repaint();
                _ = active ? unit : pump.Rest();
            }
            timer.Elapsed += Tick;
            Subscription box = new(detachers: Seq(() => { timer.Elapsed -= Tick; timer.Stop(); timer.Dispose(); }));
            pump.Adopt(detacher: box);   // store + mark active BEFORE the initial tick can rest
            timer.Start();
            _ = runner.Tick();   // initial emit of the start value
            return Fin.Succ<IDisposable>(value: box);
        });

    private static Fin<IDisposable> DisplayOrFallback<TState>(MotionRunner<TState> runner, RedrawTarget target, Option<CAFrameRateRange> rate, MotionPump pump) where TState : struct, IMotionState<TState> =>
        (OperatingSystem.IsMacOSVersionAtLeast(major: 14), target) switch {
            (true, RedrawTarget.View view) => Optional(Runtime.GetNSObject<NSView>(ptr: view.Value.Handle)).Case switch {
                NSView native => DisplayDriver(view: native, rate: rate, runner: runner, target: target, pump: pump),
                _ => Caution(runner: runner, target: target, concern: "DisplayLink view exposes no native NSView", pump: pump),
            },
            _ => Caution(runner: runner, target: target, concern: "DisplayLink requires a RhinoView on macOS 14+", pump: pump),
        };

    private static Fin<IDisposable> Caution<TState>(MotionRunner<TState> runner, RedrawTarget target, string concern, MotionPump pump) where TState : struct, IMotionState<TState> {
        _ = Op.Side(() => RhinoApp.WriteLine(message: Op.Of(name: nameof(Run)).Caution(concern: concern).Message));
        return IdleDriver(runner: runner, target: target, pump: pump);
    }

    [SupportedOSPlatform("macos14.0")]
    private static Fin<IDisposable> DisplayDriver<TState>(NSView view, Option<CAFrameRateRange> rate, MotionRunner<TState> runner, RedrawTarget target, MotionPump pump) where TState : struct, IMotionState<TState> =>
        // BOUNDARY ADAPTER - the view-bound CADisplayLink Pacer is a native NSObject driving the integration each vsync.
        Op.Of(name: nameof(Run)).Catch(() => {
            Pacer pacer = Pacer.Start(
                view: view,
                onTick: () => {
                    bool active = runner.Tick();
                    _ = target.Repaint();
                    _ = active ? unit : pump.Rest();
                },
                range: rate.IfNone(() => CAFrameRateRange.Create(minimum: 30f, maximum: 120f, preferred: 120f)));
            Subscription box = new(detachers: Seq(pacer.Dispose));
            pump.Adopt(detacher: box);
            _ = runner.Tick();
            return Fin.Succ<IDisposable>(value: box);
        });

    private sealed class MotionRunner<TState> where TState : struct, IMotionState<TState> {
        private readonly Atom<TState> cell;
        private readonly Func<TState, TState> stepCell;   // cached static step — zero closure alloc per tick

        private MotionRunner(Atom<TState> cell) {
            this.cell = cell;
            stepCell = static live => live.Step();
        }

        internal static MotionRunner<TState> Of(Atom<TState> cell) => new(cell: cell);

        internal bool Tick() {
            TState committed = cell.Swap(stepCell);
            _ = committed.Emit();
            return committed.IsActive;
        }
    }

    // Sleep/wake coordinator (mirrors GH SpringHandle): the driver self-detaches at rest because the global
    // RhinoApp.Idle/MainLoop must not fire forever and fire-and-forget motions stay clean; Retarget re-attaches a
    // fresh driver ONLY on the rested edge (CAS 0->1), so an active follow never re-subscribes per frame.
    private sealed class MotionPump : IDisposable {
        private Func<MotionPump, Fin<IDisposable>>? attach;   // takes the pump (not a captured local) so reattach survives ownership-transfer null-out
        private IDisposable? box;
        private int state;   // 0 = rested/detached, 1 = active (driver subscribed), 2 = disposed (terminal)

        internal void Bind(Func<MotionPump, Fin<IDisposable>> attachFactory) => attach = attachFactory;
        internal Fin<Unit> Begin() => Op.Of(name: nameof(Run)).Need(attach).Bind(valid => valid(arg: this).Map(static _ => unit));
        internal void Adopt(IDisposable detacher) { box = detacher; _ = Interlocked.CompareExchange(ref state, 1, 0); }
        internal Unit Rest() => Interlocked.CompareExchange(ref state, 0, 1) == 1 ? Op.Side(() => box?.Dispose()) : unit;
        internal Fin<Unit> Wake() =>
            attach is { } restart && Interlocked.CompareExchange(ref state, 1, 0) == 0
                ? restart(arg: this).BiBind(
                    Succ: static _ => Fin.Succ(value: unit),
                    Fail: error => {
                        _ = Interlocked.CompareExchange(ref state, 0, 1);
                        return Fin.Fail<Unit>(error: error);
                    })
                : Fin.Succ(value: unit);
        public void Dispose() {
            _ = Interlocked.Exchange(ref state, 2);
            _ = Op.Side(() => box?.Dispose());
        }
    }

    // View-bound CADisplayLink (WWDC23 displayLink(target:selector:)) auto-tracks the display; tick drives the runner.
    [SupportedOSPlatform("macos14.0")]
    private sealed class Pacer : NSObject {
        private static readonly Selector TickSelector = new("tick:");
        private readonly Action onTick;
        private readonly CADisplayLink link;
        private int disposed;

        private Pacer(NSView view, Action onTick, CAFrameRateRange range) {
            this.onTick = onTick;
            link = view.GetDisplayLink(target: this, selector: TickSelector);
            link.PreferredFrameRateRange = range;
            link.Paused = false;
            link.AddToRunLoop(runloop: NSRunLoop.Main, mode: NSRunLoopMode.Common);
        }

        internal static Pacer Start(NSView view, Action onTick, CAFrameRateRange range) => new(view: view, onTick: onTick, range: range);

        [Export("tick:")]
        public void Tick(CADisplayLink sender) => onTick();

        // BOUNDARY ADAPTER - invalidate the view-vended link before dispose to break the run-loop retain cycle.
        protected override void Dispose(bool disposing) {
            if (Interlocked.Exchange(ref disposed, 1) == 0 && disposing) {
                link.Paused = true;
                link.Invalidate();
                link.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

// --- [COMPOSITION] ------------------------------------------------------------------------
public static partial class UiViewportRequest {
    // display-only animation -> interactive:false so it drives in Scripted/bridge (see UI display-vs-interaction rule).
    public static UiIntent<MotionHandle<TValue, TVelocity>> Animate<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, RhinoView view, Action<TValue> sink, MotionClock? clock = null, TimeProvider? timeSource = null) =>
        UiIntent.OfScope(run: _ => Motion.Run(spec: spec, sink: sink, target: new RedrawTarget.View(view), clock: clock ?? MotionClock.IdleLoop, timeSource: timeSource), interactive: false);
}

public static class MotionRail {
    public static Fin<MotionHandle<TValue, TVelocity>> Animate<TState, TValue, TVelocity>(this UiCanvas<TState> canvas, MotionSpec<TValue, TVelocity> spec, Action<TValue> sink, MotionClock? clock = null, TimeProvider? timeSource = null) =>
        Op.Of(name: nameof(Animate)).Need(canvas).Bind(valid => Motion.Run(spec: spec, sink: sink, target: new RedrawTarget.Canvas(valid.Invalidate), clock: clock ?? MotionClock.IdleLoop, timeSource: timeSource));

    public static Fin<MotionHandle<TValue, TVelocity>> Animate<TState, TValue, TVelocity>(this RasmOverlay<TState> overlay, RhinoDoc document, MotionSpec<TValue, TVelocity> spec, Func<TState, TValue, TState> transition, MotionClock? clock = null, TimeProvider? timeSource = null) =>
        from validOverlay in Op.Of(name: nameof(Animate)).Need(overlay)
        from validDocument in Op.Of(name: nameof(Animate)).Need(document)
        from validTransition in Op.Of(name: nameof(Animate)).Need(transition)
        from handle in Motion.Run(
            spec: spec,
            sink: value => _ = validOverlay.Transition(transition: state => validTransition(arg1: state, arg2: value), document: validDocument),
            target: new RedrawTarget.Document(Value: validDocument),
            clock: clock ?? MotionClock.IdleLoop,
            timeSource: timeSource)
        select handle;
}
