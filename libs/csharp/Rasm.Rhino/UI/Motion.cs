using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AppKit;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using Rasm.Rhino.Events;
using DrawingColor = System.Drawing.Color;
using DrawingPointF = System.Drawing.PointF;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
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

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record MotionClock {
    private MotionClock() { }
    public sealed record IdleLoopCase : MotionClock;
    public sealed record DisplayLinkCase(Option<CAFrameRateRange> Rate) : MotionClock;
    public sealed record TimerCase(double IntervalSeconds) : MotionClock { internal bool IsValid => double.IsFinite(IntervalSeconds) && IntervalSeconds >= 1.0 / 60.0; }

    public static readonly MotionClock IdleLoop = new IdleLoopCase();
    public static MotionClock DisplayLink(Option<CAFrameRateRange> rate = default) => new DisplayLinkCase(Rate: rate);
    [SupportedOSPlatform("macos14.0")]
    public static Fin<MotionClock> DisplayLink(float min, float max, float preferred) =>
        from _ in guard(min > 0f && max >= min && preferred >= min && preferred <= max, Op.Of(name: nameof(DisplayLink)).InvalidInput())
        from clock in Fin.Succ<MotionClock>(value: new DisplayLinkCase(Rate: Some(CAFrameRateRange.Create(minimum: min, maximum: max, preferred: preferred))))
        select clock;
    public static Fin<MotionClock> UITimer(double intervalSeconds = 1.0 / 60.0) =>
        new TimerCase(IntervalSeconds: intervalSeconds) switch {
            { IsValid: true } c => Fin.Succ<MotionClock>(value: c),
            _ => Fin.Fail<MotionClock>(error: Op.Of(name: nameof(UITimer)).InvalidInput()),
        };
}

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record MotionSpec<TValue, TVelocity> {
    private MotionSpec() { }
    public sealed record Spring(TValue From, TValue To, SpringConfig Config, IMotionVector<TValue, TVelocity> Vector, Option<TVelocity> InitialVelocity = default) : MotionSpec<TValue, TVelocity>;
    public sealed record Tween(TValue From, TValue To, TimeSpan Duration, Easing Easing, IMotionVector<TValue, TVelocity> Vector) : MotionSpec<TValue, TVelocity>;
    public sealed record Pulse(TValue From, TValue To, TimeSpan Duration, Easing Easing, IMotionVector<TValue, TVelocity> Vector, int Cycles = 1, bool Yoyo = false, bool Infinite = false) : MotionSpec<TValue, TVelocity>;
    public sealed record Sequence(TValue Start, Seq<(TValue Target, TimeSpan Duration, Easing Easing)> Steps, IMotionVector<TValue, TVelocity> Vector) : MotionSpec<TValue, TVelocity>;
    public sealed record Decay(TValue From, TVelocity Velocity, double Friction, IMotionVector<TValue, TVelocity> Vector) : MotionSpec<TValue, TVelocity>;
}

[Union]
public abstract partial record RedrawTarget {
    private RedrawTarget() { }
    public sealed record View(RhinoView Value) : RedrawTarget;
    public sealed record Document(RhinoDoc Value, Option<RhinoView> Only = default) : RedrawTarget;
    public sealed record Canvas(Action Invalidate) : RedrawTarget;
    internal Unit Repaint() => Switch(
        view: static v => Op.Side(() => v.Value.Redraw()),
        document: static d => (d.Only | Optional(d.Value.Views.ActiveView)).Case switch {
            RhinoView one => Op.Side(() => one.Redraw()),
            _ => Op.Side(() => d.Value.Views.Redraw(deferred: true)),
        },
        canvas: static c => Op.Side(() => c.Invalidate()));
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

internal interface IMotionState<TSelf> where TSelf : struct, IMotionState<TSelf> {
    public TSelf Step();
    public Unit Emit();
    public bool IsActive { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct MotionColor(double A, double R, double G, double B) {
    public static MotionColor FromDrawing(DrawingColor c) => new(A: c.A, R: c.R, G: c.G, B: c.B);

    public DrawingColor Project() =>
        DrawingColor.FromArgb(alpha: Channel(value: A), red: Channel(value: R), green: Channel(value: G), blue: Channel(value: B));

    private static int Channel(double value) =>
        Math.Clamp(value: (int)Math.Round(value, MidpointRounding.ToEven), min: 0, max: 255);
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

internal readonly record struct TimedRunnerState<TValue, TVelocity>(TValue From, TValue To, TimeSpan Duration, Easing Easing, Seq<(TValue Target, TimeSpan Duration, Easing Easing)> Rest, bool Yoyo, bool Infinite, int CyclesRemaining, IMotionVector<TValue, TVelocity> Vector, Action<TValue> Sink, TimeProvider Clock, long Start) : IMotionState<TimedRunnerState<TValue, TVelocity>> {
    public TimedRunnerState<TValue, TVelocity> Step() =>
        (Elapsed, Rest.IsEmpty, Infinite || CyclesRemaining > 0) switch {
            (true, false, _) => this with { From = To, To = Rest[0].Target, Duration = Rest[0].Duration, Easing = Rest[0].Easing, Rest = Rest.Tail, Start = Clock.GetTimestamp() },
            (true, true, true) => this with { From = Yoyo ? To : From, To = Yoyo ? From : To, CyclesRemaining = Infinite ? CyclesRemaining : CyclesRemaining - 1, Start = Clock.GetTimestamp() },
            _ => this,
        };
    public Unit Emit() {
        double total = Duration.TotalSeconds;
        double t = total <= 0d ? 1d : Math.Clamp(value: Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds / total, min: 0d, max: 1d);
        Sink(Vector.Lerp(a: From, b: To, t: Easing.Apply(t: t)));
        return unit;
    }
    public bool IsActive => !Rest.IsEmpty || Infinite || CyclesRemaining > 0 || !Elapsed;
    private bool Elapsed => Clock.GetElapsedTime(startingTimestamp: Start).TotalSeconds >= Duration.TotalSeconds;
}

// --- [SERVICES] ---------------------------------------------------------------------------
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

internal sealed class MotionPoseVector : IMotionVector<Transform, (Quaternion R, Vector3d T)> {
    private static readonly MotionRotationVector Q = new();
    public (Quaternion R, Vector3d T) ZeroVelocity => (Quaternion.Zero, Vector3d.Zero);
    public double RestEpsilon => 1e-4;
    public (Quaternion R, Vector3d T) Delta(Transform a, Transform b) {
        (Quaternion ra, Vector3d ta) = Split(a);
        (Quaternion rb, Vector3d tb) = Split(b);
        return (rb - ra, tb - ta);
    }
    public (Quaternion R, Vector3d T) Add((Quaternion R, Vector3d T) a, (Quaternion R, Vector3d T) b) => (a.R + b.R, a.T + b.T);
    public (Quaternion R, Vector3d T) Scale((Quaternion R, Vector3d T) v, double s) => (v.R * s, v.T * s);
    public Transform Move(Transform value, (Quaternion R, Vector3d T) d) {
        (Quaternion r, Vector3d t) = Split(value);
        return Compose(Q.Move(r, d.R), t + d.T);
    }
    public double Norm((Quaternion R, Vector3d T) v) => Math.Max(v.R.Length, v.T.Length);
    public Transform Lerp(Transform a, Transform b, double t) {
        (Quaternion ra, Vector3d ta) = Split(a);
        (Quaternion rb, Vector3d tb) = Split(b);
        return Compose(Q.Lerp(ra, rb, t), new Vector3d(ta.X + ((tb.X - ta.X) * t), ta.Y + ((tb.Y - ta.Y) * t), ta.Z + ((tb.Z - ta.Z) * t)));
    }
    private static (Quaternion R, Vector3d T) Split(Transform m) => (m.GetQuaternion(out Quaternion q) ? q : Quaternion.Identity, new Vector3d(m[0, 3], m[1, 3], m[2, 3]));
    private static Transform Compose(Quaternion r, Vector3d t) { Transform m = r.MatrixForm(); m[0, 3] = t.X; m[1, 3] = t.Y; m[2, 3] = t.Z; return m; }
}

internal sealed class MotionRotationVector : IMotionVector<Quaternion> {
    public Quaternion ZeroVelocity { get; } = Quaternion.Zero;
    public double RestEpsilon => 1e-4;
    public Quaternion Delta(Quaternion from, Quaternion target) => target - from;
    public Quaternion Add(Quaternion a, Quaternion b) => a + b;
    public Quaternion Scale(Quaternion v, double s) => v * s;
    public Quaternion Move(Quaternion v, Quaternion d) => Unit(v + d);
    public double Norm(Quaternion v) => v.Length;
    public Quaternion Lerp(Quaternion a, Quaternion b, double t) => Quaternion.Slerp(a, b, t);
    private static Quaternion Unit(Quaternion q) { Quaternion r = q; _ = r.Unitize(); return r; }
}

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

// --- [OPERATIONS] -------------------------------------------------------------------------
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

public static class MotionSpec {   // inference-friendly factories: MotionSpec.Spring(...) infers TValue
    public static MotionSpec<T, TV>.Spring Spring<T, TV>(T from, T to, SpringConfig config, IMotionVector<T, TV> vector, Option<TV> initialVelocity = default) => new(From: from, To: to, Config: config, Vector: vector, InitialVelocity: initialVelocity);
    public static MotionSpec<T, TV>.Tween Tween<T, TV>(T from, T to, TimeSpan duration, Easing easing, IMotionVector<T, TV> vector) => new(From: from, To: to, Duration: duration, Easing: easing, Vector: vector);
    public static MotionSpec<T, TV>.Pulse Pulse<T, TV>(T from, T to, TimeSpan duration, Easing easing, IMotionVector<T, TV> vector, int cycles = 1, bool yoyo = false, bool infinite = false) => new(From: from, To: to, Duration: duration, Easing: easing, Vector: vector, Cycles: cycles, Yoyo: yoyo, Infinite: infinite);
    public static MotionSpec<T, TV>.Sequence Sequence<T, TV>(T start, Seq<(T Target, TimeSpan Duration, Easing Easing)> steps, IMotionVector<T, TV> vector) => new(Start: start, Steps: steps, Vector: vector);
    public static MotionSpec<T, TV>.Decay Decay<T, TV>(T from, TV velocity, double friction, IMotionVector<T, TV> vector) => new(From: from, Velocity: velocity, Friction: friction, Vector: vector);
}

public static class MotionVector {
    private const double Rest = 0.001;   // unitless convergence threshold shared by scalar and spatial channels
    private const double PixelRest = 0.5;
    private const double MatrixRest = 1e-4;
    private static MotionVectorImpl<T> Of<T>(T zero, double restEpsilon, Func<T, T, T> add, Func<T, T, T> sub, Func<T, double, T> scale, Func<T, double> norm, Func<T, T, double, T> lerp) =>
        new(zero: zero, restEpsilon: restEpsilon, add: add, subtract: sub, scale: scale, norm: norm, lerp: lerp);

    public static readonly IMotionVector<double> Double = Of(
        zero: 0.0, restEpsilon: Rest,
        add: static (a, b) => a + b, sub: static (a, b) => a - b, scale: static (v, s) => v * s,
        norm: static v => Math.Abs(value: v), lerp: static (a, b, t) => a + ((b - a) * t));
    public static readonly IMotionVector<float> Float = Of(
        zero: 0f, restEpsilon: Rest,
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
        zero: Point3d.Origin, restEpsilon: Rest,
        add: static (a, b) => new Point3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z),
        sub: static (a, b) => new Point3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z),
        scale: static (v, s) => new Point3d(v.X * s, v.Y * s, v.Z * s),
        norm: static v => Math.Sqrt((v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z)),
        lerp: static (a, b, t) => new Point3d(a.X + ((b.X - a.X) * t), a.Y + ((b.Y - a.Y) * t), a.Z + ((b.Z - a.Z) * t)));
    public static readonly IMotionVector<Vector3d> Vector = Of(
        zero: Vector3d.Zero, restEpsilon: Rest,
        add: static (a, b) => a + b, sub: static (a, b) => a - b, scale: static (v, s) => v * s,
        norm: static v => v.Length,
        lerp: static (a, b, t) => a + ((b - a) * t));
    public static readonly IMotionVector<Transform> Matrix = Of(
        zero: Transform.ZeroTransformation, restEpsilon: MatrixRest,
        add: static (a, b) => Zip(a, b, static (x, y) => x + y),
        sub: static (a, b) => Zip(a, b, static (x, y) => x - y),
        scale: static (v, s) => Zip(v, v, (x, _) => x * s),
        norm: static v => toSeq(Enumerable.Range(start: 0, count: 16)).Fold(0.0, (acc, i) => Math.Max(val1: acc, val2: Math.Abs(value: v[i / 4, i % 4]))),
        lerp: static (a, b, t) => Zip(a, b, (x, y) => x + ((y - x) * t)));
    public static readonly IMotionVector<MotionColor> Color = new MotionVectorImpl<MotionColor>(
        zero: new MotionColor(A: 0.0, R: 0.0, G: 0.0, B: 0.0),
        restEpsilon: 0.5,   // half a code value in linear 0..255
        add: static (a, b) => new MotionColor(A: a.A + b.A, R: a.R + b.R, G: a.G + b.G, B: a.B + b.B),
        subtract: static (a, b) => new MotionColor(A: a.A - b.A, R: a.R - b.R, G: a.G - b.G, B: a.B - b.B),
        scale: static (v, s) => new MotionColor(A: v.A * s, R: v.R * s, G: v.G * s, B: v.B * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
        lerp: static (a, b, t) => MotionColor.FromDrawing(OklabInterpolate(a: a.Project(), b: b.Project(), factor: t)));
    public static readonly IMotionVector<Quaternion> Rotation = new MotionRotationVector();
    public static readonly IMotionVector<Transform, (Quaternion R, Vector3d T)> Pose = new MotionPoseVector();
    private static Transform Zip(Transform a, Transform b, Func<double, double, double> f) =>
        toSeq(Enumerable.Range(start: 0, count: 16)).Fold(Transform.ZeroTransformation, (acc, i) => { acc[i / 4, i % 4] = f(arg1: a[i / 4, i % 4], arg2: b[i / 4, i % 4]); return acc; });
    private static int Channel(float normalized) => Math.Clamp(value: (int)Math.Round(normalized * 255f, MidpointRounding.ToEven), min: 0, max: 255);
    internal static DrawingColor OklabInterpolate(DrawingColor a, DrawingColor b, double factor) {
        (float la, float aa, float ba) = SrgbToOklab(a);
        (float lb, float ab, float bb) = SrgbToOklab(b);
        float tf = (float)factor;
        (float L, float A, float B) mix = (L: la + ((lb - la) * tf), A: aa + ((ab - aa) * tf), B: ba + ((bb - ba) * tf));
        return OklabToSrgb(mix, alpha: (int)Math.Round(a.A + ((b.A - a.A) * factor), MidpointRounding.ToEven));
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

public static partial class UiViewportRequest {
    public static UiIntent<MotionHandle<TValue, TVelocity>> Animate<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, RhinoView view, Action<TValue> sink, MotionClock? clock = null, TimeProvider? timeSource = null) =>
        UiIntent.OfScope(run: _ => Motion.Run(spec: spec, sink: sink, target: new RedrawTarget.View(view), clock: clock ?? MotionClock.IdleLoop, timeSource: timeSource), interactive: false);
}

internal static class Motion {
    internal static Fin<MotionHandle<TValue, TVelocity>> Run<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, Action<TValue> sink, RedrawTarget target, MotionClock clock, TimeProvider? timeSource = null) {
        TimeProvider fallback = timeSource ?? TimeProvider.System;
        Option<PacerTimeProvider> vsync = clock is MotionClock.DisplayLinkCase && OperatingSystem.IsMacOSVersionAtLeast(major: 14)
            ? Some(MakeVsync(fallback: fallback))   // vsync-anchored clock baked into the runner; the Pacer advances it from CADisplayLink.TargetTimestamp
            : Option<PacerTimeProvider>.None;
        TimeProvider resolved = vsync.Map(static provider => (TimeProvider)provider).IfNone(fallback);
        return from validSink in Op.Of(name: nameof(Run)).Need(sink)
               from validSpec in Op.Of(name: nameof(Run)).Need(spec)
               from _ in Admit(spec: validSpec)
               from validClock in clock switch {
                   MotionClock.TimerCase { IsValid: false } => Fin.Fail<MotionClock>(error: Op.Of(name: nameof(Run)).InvalidInput()),
                   _ => Fin.Succ(value: clock),
               }
               from handle in ReduceMotion
                   ? Settle(spec: validSpec, sink: validSink, target: target)
                   : Dispatch(spec: validSpec, sink: validSink, target: target, clock: validClock, resolved: resolved, vsync: vsync)
               select handle;
    }

    private static Fin<Unit> Admit<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec) =>
        spec.Switch(
            spring: static _ => Fin.Succ(value: unit),
            tween: static t => guard(t.Duration > TimeSpan.Zero, Op.Of(name: nameof(Run)).InvalidInput()).ToFin(),
            pulse: static p => guard(p.Duration > TimeSpan.Zero && (p.Infinite || p.Cycles > 0), Op.Of(name: nameof(Run)).InvalidInput()).ToFin(),
            sequence: static q => guard(q.Steps.ForAll(static step => step.Duration > TimeSpan.Zero), Op.Of(name: nameof(Run)).InvalidInput()).ToFin(),
            decay: static d => guard(double.IsFinite(d.Friction) && d.Friction > 0d, Op.Of(name: nameof(Run)).InvalidInput()).ToFin());

    private static bool ReduceMotion =>
        OperatingSystem.IsMacOSVersionAtLeast(major: 14) && ShouldReduceMotion();

    [SupportedOSPlatform("macos14.0")]
    private static bool ShouldReduceMotion() => NSWorkspace.SharedWorkspace.AccessibilityDisplayShouldReduceMotion;

    [SupportedOSPlatform("macos14.0")]
    private static PacerTimeProvider MakeVsync(TimeProvider fallback) => new(fallback: fallback);

    private static Fin<MotionHandle<TValue, TVelocity>> Dispatch<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, Action<TValue> sink, RedrawTarget target, MotionClock clock, TimeProvider resolved, Option<PacerTimeProvider> vsync) =>
        spec.Switch(
            state: (Sink: sink, Target: target, Clock: clock, Resolved: resolved, Vsync: vsync),
            spring: static (state, s) => Drive(
                initial: new SpringRunnerState<TValue, TVelocity>(
                    Value: s.From, Velocity: s.InitialVelocity.IfNone(s.Vector.ZeroVelocity), Target: s.To,
                    Config: s.Config, Vector: s.Vector, Sink: state.Sink, Clock: state.Resolved,
                    Timestamp: state.Resolved.GetTimestamp()),
                target: state.Target, clock: SpringClock(clock: state.Clock, config: s.Config, target: state.Target), vsync: state.Vsync,
                steering: cell => new MotionHandle<TValue, TVelocity>.Steering(
                    Retarget: (toValue, velocity) => Fin.Succ(value: Op.Side(() => cell.Swap(state => state with { Target = toValue, Velocity = velocity.IfNone(state.Velocity) }))),
                    Velocity: () => cell.Value.Velocity)),
            decay: static (state, d) => Drive(
                initial: new DecayRunnerState<TValue, TVelocity>(
                    Value: d.From, Velocity: d.Velocity, Friction: d.Friction, Vector: d.Vector,
                    Sink: state.Sink, Clock: state.Resolved, Timestamp: state.Resolved.GetTimestamp()),
                target: state.Target, clock: state.Clock, vsync: state.Vsync,
                steering: cell => new MotionHandle<TValue, TVelocity>.Steering(
                    Retarget: (_, velocity) => Fin.Succ(value: Op.Side(() => cell.Swap(state => state with { Velocity = velocity.IfNone(state.Velocity) }))),
                    Velocity: () => cell.Value.Velocity)),
            tween: static (state, t) => Drive(
                initial: MakeTimed(from: t.From, to: t.To, duration: t.Duration, easing: t.Easing, rest: Seq<(TValue, TimeSpan, Easing)>(), yoyo: false, infinite: false, cycles: 0, vector: t.Vector, sink: state.Sink, clock: state.Resolved), target: state.Target, clock: state.Clock, vsync: state.Vsync, steering: ReseedTimed(vector: t.Vector)),
            pulse: static (state, p) => Drive(
                initial: MakeTimed(from: p.From, to: p.To, duration: p.Duration, easing: p.Easing, rest: Seq<(TValue, TimeSpan, Easing)>(), yoyo: p.Yoyo, infinite: p.Infinite, cycles: Math.Max(val1: 0, val2: p.Cycles - 1), vector: p.Vector, sink: state.Sink, clock: state.Resolved), target: state.Target, clock: state.Clock, vsync: state.Vsync, steering: ReseedTimed(vector: p.Vector)),
            sequence: static (state, q) => Drive(
                initial: q.Steps.IsEmpty
                    ? MakeTimed(from: q.Start, to: q.Start, duration: TimeSpan.Zero, easing: Easing.Linear,
                        rest: Seq<(TValue, TimeSpan, Easing)>(), yoyo: false, infinite: false, cycles: 0,
                        vector: q.Vector, sink: state.Sink, clock: state.Resolved)
                    : MakeTimed(from: q.Start, to: q.Steps[0].Target, duration: q.Steps[0].Duration,
                        easing: q.Steps[0].Easing, rest: q.Steps.Tail, yoyo: false, infinite: false, cycles: 0,
                        vector: q.Vector, sink: state.Sink, clock: state.Resolved),
                target: state.Target, clock: state.Clock, vsync: state.Vsync, steering: ReseedTimed(vector: q.Vector)));

    // Over-sampling waste: a slow spring (Sluggish naturalHz≈1Hz) drove DisplayLink at preferred=max (120Hz). Derive preferred from physics so the link samples ~4× the natural period — only when the caller left Rate unset.
    private static MotionClock SpringClock(MotionClock clock, SpringConfig config, RedrawTarget target) =>
        (clock, OperatingSystem.IsMacOSVersionAtLeast(major: 14), ResolveView(target: target)) switch {
            (MotionClock.DisplayLinkCase { Rate.IsNone: true }, true, { IsSome: true, Case: RhinoView rv }) =>
                Optional(Runtime.GetNSObject<NSView>(ptr: rv.Handle)).Case switch {
                    NSView native => MotionClock.DisplayLink(rate: Some(SpringRate(view: native, config: config))),
                    _ => clock,
                },
            _ => clock,
        };

    [SupportedOSPlatform("macos14.0")]
    private static CAFrameRateRange SpringRate(NSView view, SpringConfig config) {
        float max = Optional(view.Window?.Screen).Map(static screen => (float)screen.MaximumFramesPerSecond).Filter(static rate => rate > 0f).IfNone(120f);
        float floor = MathF.Min(x: 30f, y: max);
        float naturalHz = MathF.Sqrt(config.Stiffness / config.Mass) / MathF.Tau;
        return CAFrameRateRange.Create(minimum: floor, maximum: max, preferred: Math.Clamp(value: naturalHz * 4f, min: floor, max: max));
    }

    private static Fin<MotionHandle<TValue, TVelocity>> Settle<TValue, TVelocity>(MotionSpec<TValue, TVelocity> spec, Action<TValue> sink, RedrawTarget target) {
        Fin<(TValue Terminal, IMotionVector<TValue, TVelocity> Vector)> resolved = spec.Switch(
            spring: static s => Fin.Succ(value: (s.To, s.Vector)),
            tween: static t => Fin.Succ(value: (t.To, t.Vector)),
            pulse: static p => Fin.Succ(value: ((p.Yoyo, p.Infinite, p.Cycles % 2) switch { (true, false, 0) => p.From, _ => p.To }, p.Vector)),
            sequence: static q => Fin.Succ(value: (q.Steps.IsEmpty ? q.Start : q.Steps[q.Steps.Count - 1].Target, q.Vector)),
            // analytic rest under ReduceMotion: ∫₀^∞ v₀ e^{-f t} dt = v₀/f — snapping to From throws away the whole gesture
            decay: static d => Fin.Succ(value: (d.Vector.Move(value: d.From, delta: d.Vector.Scale(value: d.Velocity, scalar: 1.0 / d.Friction)), d.Vector)));
        return resolved.Map(pair => {
            sink(pair.Terminal);
            _ = target.Repaint();
            return new MotionHandle<TValue, TVelocity>(
                disposer: Subscription.Nothing,
                wake: static () => Fin.Succ(value: unit),
                steering: new MotionHandle<TValue, TVelocity>.Steering(
                    Retarget: (toValue, _) => Fin.Succ(value: Op.Side(() => { sink(toValue); _ = target.Repaint(); })),
                    Velocity: () => pair.Vector.ZeroVelocity));
        });
    }

    private static TimedRunnerState<T, TV> MakeTimed<T, TV>(
        T from,
        T to,
        TimeSpan duration,
        Easing easing,
        Seq<(T Target, TimeSpan Duration, Easing Easing)> rest,
        bool yoyo,
        bool infinite,
        int cycles,
        IMotionVector<T, TV> vector,
        Action<T> sink,
        TimeProvider clock) =>
        new(From: from, To: to, Duration: duration, Easing: easing, Rest: rest, Yoyo: yoyo, Infinite: infinite, CyclesRemaining: cycles, Vector: vector, Sink: sink, Clock: clock, Start: clock.GetTimestamp());

    private static Func<Atom<TimedRunnerState<T, TV>>, MotionHandle<T, TV>.Steering> ReseedTimed<T, TV>(IMotionVector<T, TV> vector) =>
        cell => new MotionHandle<T, TV>.Steering(
            Retarget: (toValue, _) => Fin.Succ(value: Op.Side(() => cell.Swap(state => state with {
                From = state.Vector.Lerp(
                    a: state.From, b: state.To,
                    t: state.Easing.Apply(
                        t: state.Duration.TotalSeconds <= 0d
                            ? 1d
                            : Math.Clamp(
                                value: state.Clock.GetElapsedTime(startingTimestamp: state.Start).TotalSeconds / state.Duration.TotalSeconds,
                                min: 0d, max: 1d))),
                To = toValue,
                Rest = Seq<(T, TimeSpan, Easing)>(),
                Start = state.Clock.GetTimestamp(),
            }))),
            Velocity: () => vector.ZeroVelocity);

    private static Fin<MotionHandle<TValue, TVelocity>> Drive<TState, TValue, TVelocity>(TState initial, RedrawTarget target, MotionClock clock, Option<PacerTimeProvider> vsync, Func<Atom<TState>, MotionHandle<TValue, TVelocity>.Steering> steering) where TState : struct, IMotionState<TState> {
        Atom<TState> cell = Atom(initial);   // runner + steering share one cell; Retarget CASes it, pump re-attaches on the rested edge
        MotionRunner<TState> runner = MotionRunner<TState>.Of(cell: cell);
        MotionPump? pump = new();
        pump.Bind(attachFactory: p => clock.Switch(
            state: (Runner: runner, Target: target, Pump: p, Vsync: vsync),
            idleLoopCase: static (state, _) => IdleDriver(runner: state.Runner, target: state.Target, pump: state.Pump),
            displayLinkCase: static (state, link) => DisplayOrFallback(runner: state.Runner, target: state.Target, rate: link.Rate, pump: state.Pump, vsync: state.Vsync),
            timerCase: static (state, timer) => UiTimerDriver(runner: state.Runner, target: state.Target, interval: timer.IntervalSeconds, pump: state.Pump)));
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
    private static Action Pulse<TState>(MotionRunner<TState> runner, RedrawTarget target, MotionPump pump) where TState : struct, IMotionState<TState> =>
        () => _ = RhinoUi.Protect(valid: () => {
            bool active = runner.Tick();
            _ = target.Repaint();
            _ = active ? unit : pump.Rest();
            return Fin.Succ(value: unit);
        }).IfFail(error => {
            _ = Op.Side(() => RhinoApp.WriteLine(message: error.Message));
            return pump.Rest();
        });

    private static Fin<IDisposable> IdleDriver<TState>(MotionRunner<TState> runner, RedrawTarget target, MotionPump pump) where TState : struct, IMotionState<TState> =>
        Op.Of(name: nameof(Run)).Catch(() => {
            Action pulse = Pulse(runner: runner, target: target, pump: pump);
            void Tick(object? sender, EventArgs args) => pulse();
            EventHandler handler = Tick;   // single delegate instance for symmetric +=/-=
            RhinoApp.Idle += handler;
            RhinoApp.MainLoop += handler;
            Subscription box = Subscription.Of(detachers: Seq(() => RhinoApp.Idle -= handler, () => RhinoApp.MainLoop -= handler));
            pump.Adopt(detacher: box);   // store + mark active BEFORE the initial tick can rest
            _ = runner.Tick();   // initial emit of the start value
            return Fin.Succ<IDisposable>(value: box);
        });

    private static Fin<IDisposable> UiTimerDriver<TState>(MotionRunner<TState> runner, RedrawTarget target, double interval, MotionPump pump) where TState : struct, IMotionState<TState> =>
        Op.Of(name: nameof(Run)).Catch(() => {
            Eto.Forms.UITimer timer = new() { Interval = interval };
            Action pulse = Pulse(runner: runner, target: target, pump: pump);
            void Tick(object? sender, EventArgs args) => pulse();
            timer.Elapsed += Tick;
            Subscription box = Subscription.Of(detach: () => { timer.Elapsed -= Tick; timer.Stop(); timer.Dispose(); });
            pump.Adopt(detacher: box);   // store + mark active BEFORE the initial tick can rest
            timer.Start();
            _ = runner.Tick();   // initial emit of the start value
            return Fin.Succ<IDisposable>(value: box);
        });

    private static Fin<IDisposable> DisplayOrFallback<TState>(MotionRunner<TState> runner, RedrawTarget target, Option<CAFrameRateRange> rate, MotionPump pump, Option<PacerTimeProvider> vsync) where TState : struct, IMotionState<TState> =>
        (OperatingSystem.IsMacOSVersionAtLeast(major: 14), ResolveView(target: target)) switch {
            (true, { IsSome: true, Case: RhinoView rv }) => Optional(Runtime.GetNSObject<NSView>(ptr: rv.Handle)).Case switch {
                NSView native => DisplayDriver(view: native, rate: rate, runner: runner, target: target, pump: pump, vsync: vsync),
                _ => Caution(runner: runner, target: target, concern: "DisplayLink view exposes no native NSView", pump: pump),
            },
            _ => Caution(runner: runner, target: target, concern: "DisplayLink requires a resolvable RhinoView on macOS 14+", pump: pump),
        };
    private static Option<RhinoView> ResolveView(RedrawTarget target) => target switch {
        RedrawTarget.View v => Some(v.Value),
        RedrawTarget.Document d => d.Only | Optional(d.Value.Views.ActiveView),
        _ => Option<RhinoView>.None,
    };

    private static Fin<IDisposable> Caution<TState>(MotionRunner<TState> runner, RedrawTarget target, string concern, MotionPump pump) where TState : struct, IMotionState<TState> {
        _ = Op.Side(() => RhinoApp.WriteLine(message: Op.Of(name: nameof(Run)).Caution(concern: concern).Message));
        return IdleDriver(runner: runner, target: target, pump: pump);
    }

    [SupportedOSPlatform("macos14.0")]
    private static Fin<IDisposable> DisplayDriver<TState>(NSView view, Option<CAFrameRateRange> rate, MotionRunner<TState> runner, RedrawTarget target, MotionPump pump, Option<PacerTimeProvider> vsync) where TState : struct, IMotionState<TState> =>
        Op.Of(name: nameof(Run)).Catch(() => {
            Pacer pacer = Pacer.Start(view: view, onTick: Pulse(runner: runner, target: target, pump: pump), rate: rate);
            _ = vsync.Iter(provider => pacer.VsyncClock = provider);
            Subscription box = Subscription.Of(detach: pacer.Dispose);
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
    private sealed class MotionPump : IDisposable {
        private Func<MotionPump, Fin<IDisposable>>? attach;   // takes the pump (not a captured local) so reattach survives ownership-transfer null-out
        private IDisposable? box;
        private int state;   // 0 = rested/detached, 1 = active (driver subscribed), 2 = disposed (terminal)

        internal void Bind(Func<MotionPump, Fin<IDisposable>> attachFactory) => attach = attachFactory;
        internal Fin<Unit> Begin() => Op.Of(name: nameof(Run)).Need(attach).Bind(valid => valid(arg: this).Map(static _ => unit));
        internal void Adopt(Subscription detacher) {
            _ = Volatile.Read(ref state) == 2
                ? Op.Side(detacher.Dispose)
                : Op.Side(() => {
                    box = detacher;
                    _ = Interlocked.CompareExchange(ref state, 1, 0);
                    _ = Volatile.Read(ref state) == 2 && ReferenceEquals(objA: box, objB: detacher)
                        ? Op.Side(() => {
                            box = null;
                            detacher.Dispose();
                        })
                        : unit;
                });
        }
        internal Unit Rest() =>
            Interlocked.CompareExchange(ref state, 0, 1) == 1
                ? Op.Side(() => {
                    IDisposable? live = Interlocked.Exchange(location1: ref box, value: null);
                    live?.Dispose();
                })
                : unit;
        internal Fin<Unit> Wake() =>
            attach is { } restart && Interlocked.CompareExchange(ref state, 1, 0) == 0
                ? restart(arg: this).BiBind(
                    Succ: _ => guard(Volatile.Read(ref state) != 2, Op.Of(name: nameof(Wake)).InvalidResult()).ToFin(),
                    Fail: error => {
                        _ = Interlocked.CompareExchange(ref state, 0, 1);
                        return Fin.Fail<Unit>(error: error);
                    })
                : Fin.Succ(value: unit);
        public void Dispose() {
            _ = Interlocked.Exchange(ref state, 2);
            _ = Op.Side(() => {
                IDisposable? live = Interlocked.Exchange(location1: ref box, value: null);
                live?.Dispose();
            });
        }
    }
    [SupportedOSPlatform("macos14.0")]
    private sealed class PacerTimeProvider(TimeProvider fallback) : TimeProvider {   // returns the last vsync target timestamp so spring/decay integrate present-to-present, removing one ProMotion frame of forward bias
        private double lastTarget;
        internal void Advance(double targetTimestamp) => Volatile.Write(ref lastTarget, targetTimestamp);
        public override long GetTimestamp() =>
            Volatile.Read(ref lastTarget) switch {
                double t when t > 0d => (long)(t * global::System.Diagnostics.Stopwatch.Frequency),
                _ => fallback.GetTimestamp(),
            };
        public override long TimestampFrequency => global::System.Diagnostics.Stopwatch.Frequency;
    }

    // Per-view vsync via NSView.GetDisplayLink. Pool keys are weak so dead views evict naturally; the static screen-change observer holds zero Pacer refs and walks the live pool (GH2 canvas parity).
    [SupportedOSPlatform("macos14.0")]
    private sealed class Pacer : NSObject {
        private static readonly Selector TickSelector = new("tick:");
#pragma warning disable IDE0028 // ConditionalWeakTable has no collection-expression target.
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<NSView, Pacer> Pool = new();
#pragma warning restore IDE0028
        private static readonly Lock PoolGate = new();
        private static readonly Lock ObserverGate = new();
        private static NSObject? screenChangeObserver;

        private readonly NSView view;
        private readonly Action onTick;
        private readonly CAFrameRateRange? explicitRate;
        internal PacerTimeProvider? VsyncClock;   // assigned by DisplayDriver to thread CADisplayLink.TargetTimestamp into the runner clock
        // BOUNDARY ADAPTER — view-vended CADisplayLink is owns:false; teardown is Invalidate(), never Dispose.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "view-vended CADisplayLink (owns:false) is invalidated, never disposed; disposing double-frees the run-loop handle.")]
        private CADisplayLink link;
        private int disposed;

        private Pacer(NSView view, Action onTick, CAFrameRateRange? explicitRate) {
            this.view = view;
            this.onTick = onTick;
            this.explicitRate = explicitRate;
            link = BindLink(view: view, range: ResolveRate(view: view, explicitRate: explicitRate));
            EnsureObserver();
        }

        // AppDomain-lifetime observer (RhinoWIP never tears down plugins). No Pacer refs held; the live pool is walked on each parameter change.
        private static void EnsureObserver() {
            using (ObserverGate.EnterScope()) {
                screenChangeObserver ??= NSApplication.Notifications.ObserveDidChangeScreenParameters(
                    static (_, _) => { using (PoolGate.EnterScope()) { _ = toSeq(Pool).Iter(static entry => entry.Value.RebindLink()); } });
            }
        }

        internal static Pacer Start(NSView view, Action onTick, Option<CAFrameRateRange> rate) {
            Pacer pacer = new(view: view, onTick: onTick, explicitRate: rate.Case as CAFrameRateRange?);
            using (PoolGate.EnterScope()) { Pool.AddOrUpdate(view, pacer); }
            return pacer;
        }

        [Export("tick:")]
        public void Tick(CADisplayLink sender) {
            VsyncClock?.Advance(sender.TargetTimestamp);
            onTick();
        }

        private CADisplayLink BindLink(NSView view, CAFrameRateRange range) {
            CADisplayLink bound = view.GetDisplayLink(target: this, selector: TickSelector);
            bound.PreferredFrameRateRange = range;
            bound.Paused = false;
            bound.AddToRunLoop(runloop: NSRunLoop.Main, mode: NSRunLoopMode.Common);
            return bound;
        }

        // Topology change renegotiates the panel rate; swap the live link atomically so the old one is invalidated without tearing the tick stream.
        private void RebindLink() =>
            _ = Volatile.Read(ref disposed) == 0
                ? Op.Side(() => {
                    CADisplayLink previous = Interlocked.Exchange(ref link, BindLink(view: view, range: ResolveRate(view: view, explicitRate: explicitRate)));
                    VsyncClock?.Advance(0d);   // reset the stale vsync anchor across the rebind boundary
                    previous.Invalidate();
                })
                : unit;

        // NSScreen.MaximumFramesPerSecond is the actual panel ceiling (60/120/144Hz vary); MaximumFramesPerSecond is nint, hence the (float) cast.
        private static CAFrameRateRange ResolveRate(NSView view, CAFrameRateRange? explicitRate) =>
            explicitRate ?? ResolveRate(view: view);

        private static CAFrameRateRange ResolveRate(NSView view) {
            float max = Optional(view.Window?.Screen).Map(static screen => (float)screen.MaximumFramesPerSecond).Filter(static rate => rate > 0f).IfNone(120f);
            float floor = MathF.Min(x: 30f, y: max);
            return CAFrameRateRange.Create(minimum: floor, maximum: max, preferred: max);
        }

        protected override void Dispose(bool disposing) {
            if (Interlocked.Exchange(ref disposed, 1) == 0 && disposing) {
                using (PoolGate.EnterScope()) { _ = Pool.Remove(view); }
                link.Paused = true;
                link.Invalidate();
            }
            base.Dispose(disposing);
        }
    }
}
