using System.Diagnostics;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipFactoryMethods = true)]
public sealed partial class DampingRatio {
    public static Fin<DampingRatio> Validated(double value) => UIFault.Unless(double.IsFinite(value) && value > 0d, UIReason.Range, value).Map(_ => new DampingRatio(value));
}

[ValueObject<double>(SkipFactoryMethods = true)]
public sealed partial class VisibleChange {
    public static Fin<VisibleChange> Validated(double value) => UIFault.Unless(value is > 0d and <= 1d, UIReason.Range, value).Map(_ => new VisibleChange(value));
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Easing {
    public static Easing Linear { get; } = new(0d, 0d, 1d, 1d);
    public static Easing Ease { get; } = new(0.25d, 0.1d, 0.25d, 1d);
    public static Easing EaseIn { get; } = new(0.42d, 0d, 1d, 1d);
    public static Easing EaseOut { get; } = new(0d, 0d, 0.58d, 1d);
    public static Easing EaseInOut { get; } = new(0.42d, 0d, 0.58d, 1d);

    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public double Progress(double time) => Bezier(Solved(time), Y1, Y2);

    public double Slope(double time) {
        double solved = Solved(time);
        return Tangent(solved, Y1, Y2) / Tangent(solved, X1, X2);
    }

    public static Fin<Easing> Validated(double x1, double y1, double x2, double y2) =>
        UIFault.Unless(x1 is >= 0d and <= 1d && x2 is >= 0d and <= 1d && double.IsFinite(y1) && double.IsFinite(y2), UIReason.Range, (x1, y1, x2, y2)).Map(_ => new Easing(x1, y1, x2, y2));

    private double Solved(double time) => Brent.TryFindRoot(s => Bezier(s, X1, X2) - time, 0d, 1d, 1e-12, 100, out double root) ? root : throw new UnreachableException();

    private static double Bezier(double s, double first, double second) => (3d * (1d - s) * (1d - s) * s * first) + (3d * (1d - s) * s * s * second) + (s * s * s);

    private static double Tangent(double s, double first, double second) => (3d * (1d - s) * (1d - s) * first) + (6d * (1d - s) * s * (second - first)) + (3d * s * s * (1d - second));
}

public sealed record Sample(double Value, bool Settled);

internal sealed record Moving(double Progress, double Rate);

internal sealed record Mode(double Even, double Odd);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimingCurve {
    // --- [SAMPLE]
    private static readonly Sample End = new(1d, Settled: true);

    public abstract Sample Sample(Duration elapsed);

    public abstract double Velocity(Duration elapsed);

    public virtual TimingCurve From(double velocity) => this;

    internal bool Moves => Map(instant: false, eased: true, spring: true, glide: true);

    private static Sample Sampled(Option<Moving> moved) => moved.Match(Some: static held => new Sample(held.Progress, Settled: false), None: static () => End);

    private static Option<Moving> Oscillated(double damping, double phase, double start, VisibleChange rest) {
        Mode mode = Modes(damping, phase);
        double remaining = mode.Even + ((damping - start) * mode.Odd);
        double rate = (start * mode.Even) + ((1d - (damping * start)) * mode.Odd);
        return double.Abs(remaining) < rest && double.Abs(rate) < rest ? None : Some(new Moving(1d - remaining, rate));
    }

    private static Mode Modes(double damping, double phase) {
        double root = double.Sqrt(double.Abs((1d - damping) * (1d + damping)));
        double decay = double.Exp(-damping * phase);
        double slow = double.Exp(-phase / (damping + root));
        double gap = double.ExpM1(-2d * root * phase);
        return (damping - 1d) switch {
            < 0d => new Mode(decay * double.Cos(root * phase), decay * double.Sin(root * phase) / root),
            > 0d => new Mode(0.5d * slow * (2d + gap), -0.5d * slow * gap / root),
            _ => new Mode(decay, phase * decay),
        };
    }

    // --- [CASES]
    public sealed record Instant : TimingCurve {
        public override Sample Sample(Duration elapsed) => End;

        public override double Velocity(Duration elapsed) => 0d;
    }

    public sealed record Eased(Duration Length, Easing Easing) : TimingCurve {
        public override Sample Sample(Duration elapsed) => elapsed >= Length ? End : new Sample(Easing.Progress(elapsed / Length), Settled: false);

        public override double Velocity(Duration elapsed) => elapsed >= Length ? 0d : Easing.Slope(elapsed / Length) / Length.TotalSeconds;
    }

    public sealed record Spring(Duration Response, DampingRatio Damping, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; }

        private double Frequency => double.Tau / Response.TotalSeconds;

        public override Sample Sample(Duration elapsed) => Sampled(Moved(elapsed));

        public override double Velocity(Duration elapsed) => Moved(elapsed).Map(moved => Frequency * moved.Rate).IfNone(0d);

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Option<Moving> Moved(Duration elapsed) => Oscillated((double)Damping, double.Tau * (elapsed / Response), Initial / Frequency, Rest);
    }

    public sealed record Glide(Duration TimeConstant, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; } = 1d / TimeConstant.TotalSeconds;

        public override Sample Sample(Duration elapsed) => Sampled(Moved(elapsed));

        public override double Velocity(Duration elapsed) => Moved(elapsed).Map(moved => moved.Rate / TimeConstant.TotalSeconds).IfNone(0d);

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Option<Moving> Moved(Duration elapsed) => Oscillated(1d, elapsed / TimeConstant, Initial * TimeConstant.TotalSeconds, Rest);
    }
}

public sealed record Animated(float From, float To, Duration Start, TimingCurve Curve) {
    public float Value(Duration now) => float.Lerp(From, To, (float)Curve.Sample(now - Start).Value);

    public bool Settled(Duration now) => Curve.Sample(now - Start).Settled;

    public Animated Toward(float target, Duration now, TimingCurve curve) {
        if (To.CompareTo(target) is 0) return this;
        float at = Value(now);
        return new(at, target, now, curve.From(at.CompareTo(target) is 0 ? 0d : Curve.Velocity(now - Start) * (To - From) / (target - at)));
    }

    internal static Animated Rest(float value, Duration now) => new(value, value, now, new TimingCurve.Instant());
}

internal sealed record Transition(Animated X, Animated Y, Animated Width, Animated Height, Animated Opacity) {
    internal static Transition Settled(Laid laid, float opacity, Duration now) =>
        new(Animated.Rest(laid.Frame.Left, now), Animated.Rest(laid.Frame.Top, now), Animated.Rest(laid.Frame.Width, now), Animated.Rest(laid.Frame.Height, now), Animated.Rest(opacity, now));

    internal Transition Toward(Laid laid, Duration now, TimingCurve curve) =>
        new(X.Toward(laid.Frame.Left, now, curve), Y.Toward(laid.Frame.Top, now, curve), Width.Toward(laid.Frame.Width, now, curve), Height.Toward(laid.Frame.Height, now, curve),
            Opacity.Toward(laid.Alpha, now, curve));

    internal bool Holds(Laid laid) =>
        X.To.CompareTo(laid.Frame.Left) is 0 && Y.To.CompareTo(laid.Frame.Top) is 0 && Width.To.CompareTo(laid.Frame.Width) is 0 && Height.To.CompareTo(laid.Frame.Height) is 0
        && Opacity.To.CompareTo(laid.Alpha) is 0;

    internal bool Moving(Duration now) => Seq(X, Y, Width, Height, Opacity).Exists(animated => !animated.Settled(now));

    internal SKPoint Shift(Laid laid, Duration now) => new(X.Value(now) - laid.Frame.Left, Y.Value(now) - laid.Frame.Top);

    internal Tween Tweened(Laid laid, Duration now) => new(Shift(laid, now), new SKSize(Width.Value(now), Height.Value(now)), Opacity.Value(now));
}

internal sealed record Tween(SKPoint Shift, SKSize Size, float Alpha);

internal sealed record Departure(Laid Last, Place Place, Animated Opacity) {
    internal SKPoint At => Last.Frame.Location + Place.Origin;

    internal SKRect Reach => SKRect.Intersect(Scene.Absolute(Last.Reach, Place.Origin), Place.Clip);
}

internal sealed record Countdown(Expiry Expiry, Duration Left, Option<Duration> Since) {
    internal static Countdown Started(Expiry expiry, Duration now) => new(expiry, expiry.Lifetime, Some(now));

    internal Option<Duration> Deadline => Since.Map(since => since + Left);

    internal Countdown Spent => this with { Left = Duration.Zero, Since = None };

    internal Countdown Held(bool hovered, Duration now) =>
        Since.Match(
            Some: since => hovered && Left - (now - since) > Duration.Zero ? this with { Left = Left - (now - since), Since = None } : this,
            None: () => !hovered && Left > Duration.Zero ? this with { Since = Some(now) } : this);
}

public sealed record Washed(Seq<Key> Path, Interaction Interaction);

internal sealed record Animations(
    HashMap<Washed, Animated> Fades,
    HashMap<Seq<Key>, Transition> Transitions,
    HashMap<Seq<Key>, Departure> Leaving,
    HashMap<Seq<Key>, Countdown> Timers) {
    // --- [DERIVED]
    internal static Animations Idle { get; } = new([], [], [], []);

    internal bool Moving => !Fades.IsEmpty || !Transitions.IsEmpty || !Leaving.IsEmpty;

    internal Option<Duration> Due =>
        toSeq(Timers.Values).Choose(static countdown => countdown.Deadline).Fold(Option<Duration>.None, static (least, deadline) => Some(least.Match(Some: held => Duration.Min(held, deadline), None: () => deadline)));

    internal HashMap<Washed, float> Washes(HashSet<Washed> active, Duration now) =>
        toHashMap(active.ToSeq().Map(key => (Key: key, Weight: Fades.Find(key).Map(fade => fade.Value(now)).IfNone(1f)))
            .Concat(toSeq(Fades.Filter((key, _) => !active.Contains(key)).AsIterable()).Map(entry => (entry.Key, Weight: entry.Value.Value(now))))
            .Map(static entry => (entry.Key, Weight: float.Clamp(entry.Weight, 0f, 1f)))
            .Filter(static entry => entry.Weight > 0f));

    internal Seq<Hosted> Hosted(Scene scene, Duration now) =>
        scene.Hosted.Map(hosted => hosted with { Frame = Scene.Absolute(hosted.Frame, Shift(scene, hosted.Path, now)) });

    private SKPoint Shift(Scene scene, Seq<Key> path, Duration now) =>
        Scene.Above(path).Choose(above =>
            from transition in Transitions.Find(above)
            from laid in scene.Index.Find(above)
            select transition.Shift(laid, now)).Fold(SKPoint.Empty, static (sum, shift) => sum + shift);

    // --- [OPEN]
    internal Animations Opened(Scene prior, Scene next, Duration now) =>
        this with {
            Transitions = toHashMap(next.Order.Filter(static laid => laid.Node.Key.IsSome).Choose(laid => Transited(prior, laid, now).Map(transition => (laid.Path, transition)))),
            Leaving = Leaving.Filter((path, _) => !next.Index.ContainsKey(path)).AddOrUpdateRange(prior.Order
                .Filter(laid => laid.Node.Key.IsSome && laid.Style.Motion.Moves && !next.Index.ContainsKey(laid.Path) && next.Index.ContainsKey(laid.Path.Init))
                .Choose(laid => prior.Places.Find(laid.Path).Map(place => (laid.Path, new Departure(laid, place,
                    new Animated(Transitions.Find(laid.Path).Map(transition => transition.Opacity.Value(now)).IfNone(laid.Alpha), 0f, now, laid.Style.Motion)))))),
        };

    internal Animations Settled(Scene scene) =>
        this with { Transitions = Transitions.Filter((path, transition) => scene.Index.Find(path).Exists(transition.Holds)) };

    private Option<Transition> Transited(Scene prior, Laid laid, Duration now) =>
        from curve in Some(laid.Style.Motion).Filter(static motion => motion.Moves)
        from transition in Transitions.Find(laid.Path).Match(
            Some: open => Some(open.Toward(laid, now, curve)),
            None: () => prior.Index.Find(laid.Path).Match(
                Some: was => was.Frame != laid.Frame || was.Node.Alpha.CompareTo(laid.Node.Alpha) is not 0 ? Some(Transition.Settled(was, was.Alpha, now).Toward(laid, now, curve)) : None,
                None: () => laid.Path.Count > 1 && prior.Index.ContainsKey(laid.Path.Init)
                    ? Some(Transition.Settled(laid, Leaving.Find(laid.Path).Map(left => left.Opacity.Value(now)).IfNone(0f), now).Toward(laid, now, curve))
                    : None))
        select transition;

    // --- [FADE]
    internal sealed record Washing(Animations Animations, Seq<SKRect> Damage);

    private sealed record Swept(SKRect Damage, Option<SKRect> Beyond, TimingCurve Motion);

    internal Washing Fading(Scene prior, Scene scene, HashSet<Washed> was, HashSet<Washed> active, Duration now) {
        HashSet<Washed> changed = was.SymmetricExcept(active);
        return new(this with { Fades = changed.ToSeq().Fold(Fades.Filter((key, _) => scene.Index.ContainsKey(key.Path)), (fades, key) => Faded(scene, fades, key, active, now)) },
            Repainted(prior, scene, was.Union(active).Union(toHashSet(Fades.Filter(fade => !fade.Settled(now)).Keys)), changed));
    }

    private static Seq<SKRect> Repainted(Scene prior, Scene scene, HashSet<Washed> washed, HashSet<Washed> changed) =>
        from key in toSeq(washed)
        let now = Sweep(scene, key)
        let was = Sweep(prior, key).Bind(static held => held.Beyond)
        let beyond = now.Bind(static held => held.Beyond)
        from damage in toSeq(((changed.Contains(key) ? now.Map(static held => held.Damage).ToSeq() : []) + (was == beyond ? [] : Seq(was, beyond).Somes())).Distinct())
        select damage;

    private static HashMap<Washed, Animated> Faded(Scene scene, HashMap<Washed, Animated> fades, Washed key, HashSet<Washed> active, Duration now) {
        TimingCurve curve = Sweep(scene, key).Map(static held => held.Motion).IfNone(new TimingCurve.Instant());
        float target = active.Contains(key) ? 1f : 0f;
        Animated fade = fades.Find(key).Map(open => open.Toward(target, now, curve)).IfNone(new Animated(1f - target, target, now, curve));
        return fade.Settled(now) ? fades.Remove(key) : fades.AddOrUpdate(key, fade);
    }

    private static Option<Swept> Sweep(Scene scene, Washed key) =>
        key.Interaction.Ring
            ? scene.Ring(key.Path).Map(static ring => {
                SKRect reach = SKRect.Intersect(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), ring.Clip);
                return new Swept(reach, Some(reach), ring.Style.Motion);
            })
            : from laid in scene.Index.Find(key.Path)
              from bounds in scene.Bounds(key.Path)
              select new Swept(bounds, None, laid.Style.Motion);

    // --- [TIMERS]
    internal Animations Timed(Scene scene, Option<SKPoint> pointer, Duration now) =>
        this with {
            Timers = toHashMap(scene.Order.Choose(laid => laid.Node.Expires.Map(expiry => (laid.Path,
                Timers.Find(laid.Path).Map(held => held with { Expiry = expiry }).IfNone(() => Countdown.Started(expiry, now))
                    .Held(pointer.Exists(at => scene.Visible(laid.Path).Exists(shown => shown.Contains(at))), now))))),
        };

    // --- [TICK]
    internal sealed record Ticked(Animations Animations, Seq<SKRect> Damage, Seq<IO<Unit>> Elapsed);

    private sealed record Bounded(SKRect Reach, SKRect Clip);

    internal Ticked Tick(Scene scene, Duration prior, Duration now) {
        HashMap<Seq<Key>, Countdown> elapsed = Timers.Filter(countdown => countdown.Deadline.Exists(deadline => deadline <= now));
        return new(
            new Animations(
                Fades.Filter(fade => !fade.Settled(now)),
                Transitions.Filter(transition => transition.Moving(now)),
                Leaving.Filter(left => !left.Opacity.Settled(now)),
                Timers.AddOrUpdateRange(elapsed.Map(static countdown => countdown.Spent).AsIterable())),
            Fades.Keys.ToSeq().Choose(key => Sweep(scene, key).Map(static held => held.Damage))
                .Concat(Transitions.Keys.ToSeq().Choose(path => Reached(scene, path).Map(held =>
                    SKRect.Intersect(SKRect.Union(Scene.Absolute(held.Reach, Shift(scene, path, prior)), Scene.Absolute(held.Reach, Shift(scene, path, now))), held.Clip))))
                .Concat(Leaving.Values.ToSeq().Map(static left => left.Reach)),
            elapsed.Values.ToSeq().Map(static countdown => countdown.Expiry.Elapsed));
    }

    private static Option<Bounded> Reached(Scene scene, Seq<Key> path) =>
        from laid in scene.Index.Find(path)
        from place in scene.Places.Find(path)
        select new Bounded(Scene.Absolute(laid.Reach, place.Origin), place.Clip);
}
