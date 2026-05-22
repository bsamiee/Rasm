using System.Diagnostics;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Sparkles;
using GhDuration = Grasshopper2.UI.Animation.Duration;
using GhMotion = Grasshopper2.UI.Animation.Motion;
using GhState = Grasshopper2.UI.Animation.State;
using Op = Rasm.Domain.Op;
using ZoomThreshold = Grasshopper2.UI.ZoomThreshold;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public interface IMotionVector<T> {
    public T Zero { get; }
    public T Add(T a, T b);
    public T Subtract(T a, T b);
    public T Scale(T value, float scalar);
    public float Norm(T value);
    public T Interpolate(T value0, T value1, double factor);
}

// Polymorphic easing surface — 16 GH2-native cases delegate to MotionEquations.Blend; 9 closed-form
// families (Sine/Expo/Circ/Elastic/Back/BounceOut/Cubic/Quart/Quint × In/Out/InOut) implement the
// canonical Penner formulae inline. Pre-multiplied to a per-case `Func<double, double>` at registration
// time so the hot 60 Hz tick path pays only the partial method invoke + a single virtual delegate dispatch.
[SmartEnum<int>]
public sealed partial class Easing {
    private delegate double EasingCurve(double t);

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

    // --- [CLOSED_FORM] (Penner equations) ---------------------------------------------------
    public static readonly Easing SineIn = Closed(key: 16, compute: static t => 1.0 - Math.Cos(t * Math.PI / 2.0));
    public static readonly Easing SineOut = Closed(key: 17, compute: static t => Math.Sin(t * Math.PI / 2.0));
    public static readonly Easing SineInOut = Closed(key: 18, compute: static t => -(Math.Cos(Math.PI * t) - 1.0) / 2.0);
    public static readonly Easing ExpoIn = Closed(key: 19, compute: static t => t == 0.0 ? 0.0 : Math.Pow(2.0, (10.0 * t) - 10.0));
    public static readonly Easing ExpoOut = Closed(key: 20, compute: static t => t == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * t));
    public static readonly Easing ExpoInOut = Closed(key: 21, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        t < 0.5 ? Math.Pow(2.0, (20.0 * t) - 10.0) / 2.0 : (2.0 - Math.Pow(2.0, (-20.0 * t) + 10.0)) / 2.0);
    public static readonly Easing CircIn = Closed(key: 22, compute: static t => 1.0 - Math.Sqrt(1.0 - (t * t)));
    public static readonly Easing CircOut = Closed(key: 23, compute: static t => Math.Sqrt(1.0 - ((t - 1.0) * (t - 1.0))));
    public static readonly Easing CircInOut = Closed(key: 24, compute: static t =>
        t < 0.5
            ? (1.0 - Math.Sqrt(1.0 - (4.0 * t * t))) / 2.0
            : (Math.Sqrt(1.0 - (((-2.0 * t) + 2.0) * ((-2.0 * t) + 2.0))) + 1.0) / 2.0);
    public static readonly Easing ElasticIn = Closed(key: 25, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        -Math.Pow(2.0, (10.0 * t) - 10.0) * Math.Sin(((t * 10.0) - 10.75) * (2.0 * Math.PI / 3.0)));
    public static readonly Easing ElasticOut = Closed(key: 26, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        (Math.Pow(2.0, -10.0 * t) * Math.Sin(((t * 10.0) - 0.75) * (2.0 * Math.PI / 3.0))) + 1.0);
    public static readonly Easing ElasticInOut = Closed(key: 27, compute: static t =>
        t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
        t < 0.5
            ? -(Math.Pow(2.0, (20.0 * t) - 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5))) / 2.0
            : (Math.Pow(2.0, (-20.0 * t) + 10.0) * Math.Sin(((20.0 * t) - 11.125) * (2.0 * Math.PI / 4.5)) / 2.0) + 1.0);
    public static readonly Easing BackIn = Closed(key: 28, compute: static t => (2.70158 * t * t * t) - (1.70158 * t * t));
    public static readonly Easing BackOut = Closed(key: 29, compute: static t =>
        1.0 + (2.70158 * Math.Pow(t - 1.0, 3.0)) + (1.70158 * Math.Pow(t - 1.0, 2.0)));
    public static readonly Easing BackInOut = Closed(key: 30, compute: static t =>
        t < 0.5
            ? Math.Pow(2.0 * t, 2.0) * ((((1.70158 * 1.525) + 1.0) * 2.0 * t) - (1.70158 * 1.525)) / 2.0
            : ((Math.Pow((2.0 * t) - 2.0, 2.0) * ((((1.70158 * 1.525) + 1.0) * ((t * 2.0) - 2.0)) + (1.70158 * 1.525))) + 2.0) / 2.0);
    public static readonly Easing BounceOut = Closed(key: 31, compute: BounceOutFormula);
    public static readonly Easing CubicIn = Closed(key: 32, compute: static t => t * t * t);
    public static readonly Easing CubicOut = Closed(key: 33, compute: static t => 1.0 - Math.Pow(1.0 - t, 3.0));
    public static readonly Easing CubicInOut = Closed(key: 34, compute: static t =>
        t < 0.5 ? 4.0 * t * t * t : 1.0 - (Math.Pow((-2.0 * t) + 2.0, 3.0) / 2.0));
    public static readonly Easing QuartIn = Closed(key: 35, compute: static t => t * t * t * t);
    public static readonly Easing QuartOut = Closed(key: 36, compute: static t => 1.0 - Math.Pow(1.0 - t, 4.0));
    public static readonly Easing QuintIn = Closed(key: 37, compute: static t => t * t * t * t * t);
    public static readonly Easing QuintOut = Closed(key: 38, compute: static t => 1.0 - Math.Pow(1.0 - t, 5.0));

    [UseDelegateFromConstructor]
    public partial double Apply(double t);

    private static Easing Native(int key, GhMotion motion) =>
        new(key: key, apply: t => MotionEquations.Blend(motion: motion, parameter: t));

    private static Easing Closed(int key, EasingCurve compute) => new(key: key, apply: t => compute(t));

    private static double BounceOutFormula(double t) {
        const double N1 = 7.5625;
        const double D1 = 2.75;
        return t switch {
            < 1.0 / D1 => N1 * t * t,
            < 2.0 / D1 => (N1 * (t - (1.5 / D1)) * (t - (1.5 / D1))) + 0.75,
            < 2.5 / D1 => (N1 * (t - (2.25 / D1)) * (t - (2.25 / D1))) + 0.9375,
            _ => (N1 * (t - (2.625 / D1)) * (t - (2.625 / D1))) + 0.984375,
        };
    }
}

public abstract record MotionRequest<T> : GhUiRequest<T> {
    public sealed record Tween<TValue>(Animated<TValue> Animated, Action<TValue> Sink) : MotionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.Tween(animated: Animated, sink: Sink).Run(scope: scope);
    }
    public sealed record Spring<TValue>(TValue From, TValue To, SpringConfig Config, IMotionVector<TValue> Vector, Action<TValue> Sink, Option<TValue> InitialVelocity = default) : MotionRequest<SpringHandle<TValue>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<SpringHandle<TValue>> Apply(GrasshopperUi.Scope scope) => Motion.Spring(start: From, target: To, config: Config, vector: Vector, sink: Sink, initialVelocity: InitialVelocity).Run(scope: scope);
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
    public sealed record Navigate(PointF Centre, GhDuration Duration, float MinZoom = 0.05f, float MaxZoom = 2f) : MotionRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Motion.Navigate(centre: Centre, duration: Duration, minZoom: MinZoom, maxZoom: MaxZoom).Run(scope: scope);
    }
    public sealed record ZoomGate(ZoomThreshold Threshold, Action<float> Sink) : MotionRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Motion.ZoomGate(threshold: Threshold, sink: Sink).Run(scope: scope);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpringConfig(float Stiffness, float Damping, float Mass = 1f) {
    public static SpringConfig Response(float response, float dampingFraction, float mass = 1f) {
        float twoPiOverR = (float)(2.0 * Math.PI) / response;
        return new SpringConfig(
            Stiffness: twoPiOverR * twoPiOverR * mass,
            Damping: 4f * (float)Math.PI * dampingFraction * mass / response,
            Mass: mass);
    }
    public static readonly SpringConfig Snappy = Response(response: 0.30f, dampingFraction: 0.85f);
    public static readonly SpringConfig Bouncy = Response(response: 0.55f, dampingFraction: 0.50f);
    public static readonly SpringConfig Smooth = Response(response: 0.50f, dampingFraction: 1.00f);
    public static readonly SpringConfig Sluggish = Response(response: 1.00f, dampingFraction: 1.20f);
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct SpringRunnerState<T>(
    T Value, T Velocity, T Target,
    SpringConfig Config, IMotionVector<T> Vector,
    Action<T> Sink, long Clock);

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
    IMotionVector<T> Vector);

public static class MotionVector {
    public static readonly IMotionVector<float> Float = new Vector<float>(
        zero: 0f,
        add: static (a, b) => a + b,
        subtract: static (a, b) => a - b,
        scale: static (v, s) => v * s,
        norm: static v => Math.Abs(v),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<double> Double = new Vector<double>(
        zero: 0.0,
        add: static (a, b) => a + b,
        subtract: static (a, b) => a - b,
        scale: static (v, s) => v * s,
        norm: static v => (float)Math.Abs(v),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<PointF> PointF = new Vector<PointF>(
        zero: Eto.Drawing.PointF.Empty,
        add: static (a, b) => new PointF(a.X + b.X, a.Y + b.Y),
        subtract: static (a, b) => new PointF(a.X - b.X, a.Y - b.Y),
        scale: static (v, s) => new PointF(v.X * s, v.Y * s),
        norm: static v => MathF.Sqrt((v.X * v.X) + (v.Y * v.Y)),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<SizeF> SizeF = new Vector<SizeF>(
        zero: Eto.Drawing.SizeF.Empty,
        add: static (a, b) => new SizeF(a.Width + b.Width, a.Height + b.Height),
        subtract: static (a, b) => new SizeF(a.Width - b.Width, a.Height - b.Height),
        scale: static (v, s) => new SizeF(v.Width * s, v.Height * s),
        norm: static v => MathF.Sqrt((v.Width * v.Width) + (v.Height * v.Height)),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<RectangleF> RectangleF = new Vector<RectangleF>(
        zero: Eto.Drawing.RectangleF.Empty,
        add: static (a, b) => new RectangleF(a.X + b.X, a.Y + b.Y, a.Width + b.Width, a.Height + b.Height),
        subtract: static (a, b) => new RectangleF(a.X - b.X, a.Y - b.Y, a.Width - b.Width, a.Height - b.Height),
        scale: static (v, s) => new RectangleF(v.X * s, v.Y * s, v.Width * s, v.Height * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.X), Math.Abs(v.Y)), Math.Max(Math.Abs(v.Width), Math.Abs(v.Height))),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<Color> Color = new Vector<Color>(
        zero: new Color(red: 0f, green: 0f, blue: 0f, alpha: 0f),
        add: static (a, b) => new Color(red: a.R + b.R, green: a.G + b.G, blue: a.B + b.B, alpha: a.A + b.A),
        subtract: static (a, b) => new Color(red: a.R - b.R, green: a.G - b.G, blue: a.B - b.B, alpha: a.A - b.A),
        scale: static (v, s) => new Color(red: v.R * s, green: v.G * s, blue: v.B * s, alpha: v.A * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
        interpolate: static (a, b, t) => Interpolators.Interpolate(value0: a, value1: b, factor: t));
    public static readonly IMotionVector<Color> ColorHSL = new Vector<Color>(
        zero: new Color(red: 0f, green: 0f, blue: 0f, alpha: 0f),
        add: static (a, b) => new Color(red: a.R + b.R, green: a.G + b.G, blue: a.B + b.B, alpha: a.A + b.A),
        subtract: static (a, b) => new Color(red: a.R - b.R, green: a.G - b.G, blue: a.B - b.B, alpha: a.A - b.A),
        scale: static (v, s) => new Color(red: v.R * s, green: v.G * s, blue: v.B * s, alpha: v.A * s),
        norm: static v => Math.Max(Math.Max(Math.Abs(v.R), Math.Abs(v.G)), Math.Max(Math.Abs(v.B), Math.Abs(v.A))),
        interpolate: HslInterpolate);

    private static Color HslInterpolate(Color a, Color b, double factor) {
        ColorHSL ha = a;
        ColorHSL hb = b;
        float tf = (float)factor;
        float hueDelta = ((hb.H - ha.H + 540f) % 360f) - 180f;
        return new ColorHSL(
            hue: ha.H + (hueDelta * tf),
            saturation: ha.S + ((hb.S - ha.S) * tf),
            luminance: ha.L + ((hb.L - ha.L) * tf),
            alpha: ha.A + ((hb.A - ha.A) * tf));
    }
}

internal sealed class Vector<T>(
    T zero,
    Func<T, T, T> add,
    Func<T, T, T> subtract,
    Func<T, float, T> scale,
    Func<T, float> norm,
    Func<T, T, double, T> interpolate) : IMotionVector<T> {
    public T Zero { get; } = zero;
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
    private const float SpringRestThreshold = 0.001f;
    private const float MaxFrameDelta = 1f / 30f;

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
        Action<TValue> sink, Option<TValue> initialVelocity) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from validVector in Optional(vector).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "vector is required"))
            from validSink in Optional(sink).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Spring)), detail: "sink delegate is required"))
            let cell = Atom(new SpringRunnerState<TValue>(
                Value: start,
                Velocity: initialVelocity.IfNone(validVector.Zero),
                Target: target,
                Config: config,
                Vector: validVector,
                Sink: validSink,
                Clock: Stopwatch.GetTimestamp()))
            let ticker = new SpringTicker<TValue>(cell: cell, canvas: canvas)
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: paintScope => Try.lift(ticker.Tick)
                    .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Spring)), detail: $"tick threw: {error.Message}"))).Run(scope: scope)
            from initial in Try.lift(f: () => { canvas.ScheduleRedraw(); return unit; }).Run()
                .MapFail(error => UiFault.ThreadMarshal(detail: $"{nameof(Spring)} initial redraw threw: {error.Message}"))
            select new SpringHandle<TValue>(cell: cell, subscription: sub, wake: canvas.ScheduleRedraw));

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
                interpolator: (a, b, t) => validVector.Interpolate(a, b, t))
            let cell = Atom(new PulseRunnerState<TValue>(
                Animated: initial,
                From: start, To: target,
                Duration: duration, Easing: easing,
                Yoyo: yoyo, Infinite: infinite,
                CyclesRemaining: validCycles - 1,
                Vector: validVector))
            let ticker = new PulseTicker<TValue>(cell: cell, sink: validSink, canvas: canvas)
            from sub in Paint.Hook(
                phase: CanvasPaintPhase.BeforeBackground,
                paint: paintScope => Try.lift(ticker.Tick)
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

    // Cached applyScratch delegate reads instance fields without per-frame capture; the 60 Hz
    // path allocates only the unavoidable Atom Box<A> per swap.
    private sealed class SpringTicker<T> {
        private readonly Atom<SpringRunnerState<T>> cell;
        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly Func<SpringRunnerState<T>, SpringRunnerState<T>> applyScratch;

        private T scratchValue = default!;
        private T scratchVelocity = default!;
        private long scratchClock;

        internal SpringTicker(Atom<SpringRunnerState<T>> cell, Grasshopper2.UI.Canvas.Canvas canvas) {
            this.cell = cell;
            this.canvas = canvas;
            applyScratch = state => state with { Value = scratchValue, Velocity = scratchVelocity, Clock = scratchClock };
        }

        internal Unit Tick() {
            SpringRunnerState<T> s = cell.Value;
            long now = Stopwatch.GetTimestamp();
            float dt = Math.Min(val1: (float)Stopwatch.GetElapsedTime(startingTimestamp: s.Clock, endingTimestamp: now).TotalSeconds, val2: MaxFrameDelta);

            T displacement = s.Vector.Subtract(s.Value, s.Target);
            T accel = s.Vector.Scale(
                s.Vector.Add(
                    s.Vector.Scale(displacement, -s.Config.Stiffness),
                    s.Vector.Scale(s.Velocity, -s.Config.Damping)),
                1f / s.Config.Mass);
            T newVelocity = s.Vector.Add(s.Velocity, s.Vector.Scale(accel, dt));
            T newValue = s.Vector.Add(s.Value, s.Vector.Scale(newVelocity, dt));

            scratchValue = newValue;
            scratchVelocity = newVelocity;
            scratchClock = now;
            _ = cell.Swap(applyScratch);
            s.Sink(newValue);

            bool atRest = (s.Vector.Norm(displacement) < SpringRestThreshold) && (s.Vector.Norm(newVelocity) < SpringRestThreshold);
            _ = atRest ? unit : Wake();
            return unit;
        }

        private Unit Wake() { canvas.ScheduleRedraw(); return unit; }
    }

    private sealed class PulseTicker<T> {
        private readonly Atom<PulseRunnerState<T>> cell;
        private readonly Action<T> sink;
        private readonly Grasshopper2.UI.Canvas.Canvas canvas;
        private readonly Func<PulseRunnerState<T>, PulseRunnerState<T>> applyScratch;

        private Animated<T> scratchAnimated = default!;
        private int scratchCyclesRemaining;

        internal PulseTicker(Atom<PulseRunnerState<T>> cell, Action<T> sink, Grasshopper2.UI.Canvas.Canvas canvas) {
            this.cell = cell;
            this.sink = sink;
            this.canvas = canvas;
            applyScratch = state => state with { Animated = scratchAnimated, CyclesRemaining = scratchCyclesRemaining };
        }

        internal Unit Tick() {
            PulseRunnerState<T> s = cell.Value;
            sink(s.Animated.ValueNow);
            bool finished = s.Animated.State == GhState.Finished;
            bool moreCycles = s.Infinite || s.CyclesRemaining > 0;
            _ = (finished, moreCycles) switch {
                (true, true) => Reissue(s),
                (false, _) => Wake(),
                _ => unit,
            };
            return unit;
        }

        private Unit Reissue(PulseRunnerState<T> s) {
            scratchAnimated = Animated<T>.CreateUnfinished(
                value0: s.Yoyo ? s.Animated.Value1 : s.From,
                value1: s.Yoyo ? s.Animated.Value0 : s.To,
                duration: Animators.DurationToTimeSpan(duration: s.Duration),
                motion: s.Easing,
                interpolator: s.Vector.Interpolate);
            scratchCyclesRemaining = s.Infinite ? s.CyclesRemaining : s.CyclesRemaining - 1;
            _ = cell.Swap(applyScratch);
            canvas.ScheduleRedraw();
            return unit;
        }

        private Unit Wake() { canvas.ScheduleRedraw(); return unit; }
    }
}
