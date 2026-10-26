using System.Globalization;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<Invalid>]
public sealed partial class DampingRatio {
    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0d ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"damping ratio {value} is not finite and positive"));
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<Invalid>]
public sealed partial class VisibleChange {
    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref double value) =>
        validationError = value is > 0d and <= 1d ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"visible change {value} lies outside (0, 1]"));
}

[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Easing {
    public static Easing Linear { get; } = Create(0d, 0d, 1d, 1d);
    public static Easing Ease { get; } = Create(0.25d, 0.1d, 0.25d, 1d);
    public static Easing EaseIn { get; } = Create(0.42d, 0d, 1d, 1d);
    public static Easing EaseOut { get; } = Create(0d, 0d, 0.58d, 1d);
    public static Easing EaseInOut { get; } = Create(0.42d, 0d, 0.58d, 1d);

    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public double Progress(double time) => Bezier(Solved(time), Y1, Y2);

    public double Slope(double time) => Solved(time) switch {
        var solved => Tangent(solved, Y1, Y2) / Tangent(solved, X1, X2),
    };

    public static Fin<Easing> Validated(double x1, double y1, double x2, double y2) => TryCreate(x1, y1, x2, y2, out Easing? easing, out Invalid? fault) ? easing : fault;

    private double Solved(double time) => Brent.FindRoot(s => Bezier(s, X1, X2) - time, 0d, 1d, 1e-12);

    private static double Bezier(double s, double first, double second) => (3d * (1d - s) * (1d - s) * s * first) + (3d * (1d - s) * s * s * second) + (s * s * s);

    private static double Tangent(double s, double first, double second) => (3d * (1d - s) * (1d - s) * first) + (6d * (1d - s) * s * (second - first)) + (3d * s * s * (1d - second));

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref double x1, ref double y1, ref double x2, ref double y2) =>
        validationError = x1 is >= 0d and <= 1d && x2 is >= 0d and <= 1d && double.IsFinite(y1) && double.IsFinite(y2)
            ? null
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"easing ({x1}, {y1}, {x2}, {y2}) holds an abscissa outside [0, 1] or an unbounded ordinate"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimingCurve {
    // --- [SAMPLE]
    public static Next<double, double> End { get; } = Next.Done<double, double>(1d);

    public abstract Next<double, double> Sample(Duration elapsed);

    public abstract double Velocity(Duration elapsed);

    public virtual TimingCurve From(double velocity) => this;

    internal bool Moves => Map(instant: false, eased: true, spring: true, glide: true);

    private static Next<double, double> Sampled(Option<(double Progress, double Rate)> moved) =>
        moved.Match(Some: static held => Next.Loop<double, double>(held.Progress), None: static () => End);

    private static Option<(double Progress, double Rate)> Oscillated(double damping, double phase, double start, VisibleChange rest) =>
        Modes(damping, phase) switch {
            var (even, odd) => (Remaining: even + ((damping - start) * odd), Rate: (start * even) + ((1d - (damping * start)) * odd)) switch {
                var moved when double.Abs(moved.Remaining) < rest && double.Abs(moved.Rate) < rest => None,
                var moved => Some((1d - moved.Remaining, moved.Rate)),
            },
        };

    private static (double Even, double Odd) Modes(double damping, double phase) =>
        (damping - 1d) switch {
            < 0d => (Root: double.Sqrt((1d - damping) * (1d + damping)), Decay: double.Exp(-damping * phase)) switch {
                var under => (under.Decay * double.Cos(under.Root * phase), under.Decay * double.Sin(under.Root * phase) / under.Root),
            },
            > 0d => double.Sqrt((damping - 1d) * (damping + 1d)) switch {
                var root => (Slow: double.Exp(-phase / (damping + root)), Gap: double.ExpM1(-2d * root * phase)) switch {
                    var over => (0.5d * over.Slow * (2d + over.Gap), -0.5d * over.Slow * over.Gap / root),
                },
            },
            _ => double.Exp(-phase) switch {
                var decay => (decay, phase * decay),
            },
        };

    // --- [CASES]
    public sealed record Instant : TimingCurve {
        public override Next<double, double> Sample(Duration elapsed) => End;

        public override double Velocity(Duration elapsed) => 0d;
    }

    public sealed record Eased(Duration Length, Easing Easing) : TimingCurve {
        public override Next<double, double> Sample(Duration elapsed) => elapsed >= Length ? End : Next.Loop<double, double>(Easing.Progress(elapsed / Length));

        public override double Velocity(Duration elapsed) => elapsed >= Length ? 0d : Easing.Slope(elapsed / Length) / Length.TotalSeconds;
    }

    public sealed record Spring(Duration Response, DampingRatio Damping, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; }

        private double Frequency => double.Tau / Response.TotalSeconds;

        public override Next<double, double> Sample(Duration elapsed) => Sampled(Moved(elapsed));

        public override double Velocity(Duration elapsed) => Moved(elapsed).Map(moved => Frequency * moved.Rate).IfNone(0d);

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Option<(double Progress, double Rate)> Moved(Duration elapsed) => Oscillated((double)Damping, double.Tau * (elapsed / Response), Initial / Frequency, Rest);
    }

    public sealed record Glide(Duration TimeConstant, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; } = 1d / TimeConstant.TotalSeconds;

        public override Next<double, double> Sample(Duration elapsed) => Sampled(Moved(elapsed));

        public override double Velocity(Duration elapsed) => Moved(elapsed).Map(moved => moved.Rate / TimeConstant.TotalSeconds).IfNone(0d);

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Option<(double Progress, double Rate)> Moved(Duration elapsed) => Oscillated(1d, elapsed / TimeConstant, Initial * TimeConstant.TotalSeconds, Rest);
    }
}

public sealed record Animated(float From, float To, Duration Start, TimingCurve Curve) {
    public Next<float, float> At(Duration now) =>
        Curve.Sample(now - Start).Match(Loop: progress => Next.Loop<float, float>(Lerped(progress)), Done: progress => Next.Done<float, float>(Lerped(progress)));

    public float Value(Duration now) => At(now).Match(Loop: static value => value, Done: static value => value);

    public Animated Toward(float target, Duration now, TimingCurve curve) =>
        To.CompareTo(target) is 0
            ? this
            : Value(now) switch {
                var at => new(at, target, now, curve.From(at.CompareTo(target) is 0 ? 0d : Curve.Velocity(now - Start) * (To - From) / (target - at))),
            };

    internal static Animated Rest(float value, Duration now) => new(value, value, now, new TimingCurve.Instant());

    private float Lerped(double progress) => float.Lerp(From, To, (float)progress);
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

    internal bool Moving(Duration now) => Seq(X, Y, Width, Height, Opacity).Exists(animated => animated.At(now).IsLoop);

    internal SKPoint Shift(Laid laid, Duration now) => new(X.Value(now) - laid.Frame.Left, Y.Value(now) - laid.Frame.Top);

    internal (SKPoint Shift, SKSize Size, float Alpha) Tweened(Laid laid, Duration now) => (Shift(laid, now), new SKSize(Width.Value(now), Height.Value(now)), Opacity.Value(now));
}

internal sealed record Departure(Laid Last, Place Place, Animated Opacity) {
    internal SKPoint At => Last.Frame.Location + Place.Origin;

    internal SKRect Reach => SKRect.Intersect(SKRect.Create(Last.Reach.Location + Place.Origin, Last.Reach.Size), Place.Clip);
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

internal sealed record Animations(
    HashMap<(Seq<Key> Path, Interaction Interaction), Animated> Fades,
    HashMap<Seq<Key>, Transition> Transitions,
    HashMap<Seq<Key>, Departure> Leaving,
    HashMap<Seq<Key>, Countdown> Timers) {
    // --- [DERIVED]
    internal static Animations Idle { get; } = new([], [], [], []);

    internal bool Moving => !Fades.IsEmpty || !Transitions.IsEmpty || !Leaving.IsEmpty;

    internal Option<Duration> Due =>
        toSeq(Timers.Values).Choose(static countdown => countdown.Deadline).Fold(Option<Duration>.None, static (least, deadline) => Some(least.Match(Some: held => Duration.Min(held, deadline), None: () => deadline)));

    internal HashMap<(Seq<Key> Path, Interaction Interaction), float> Washes(LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> active, Duration now) =>
        toHashMap(active.ToSeq().Map(key => (Key: key, Weight: Fades.Find(key).Map(fade => fade.Value(now)).IfNone(1f)))
            .Concat(toSeq(Fades.Filter((key, _) => !active.Contains(key)).AsIterable()).Map(entry => (entry.Key, Weight: entry.Value.Value(now))))
            .Map(static entry => (entry.Key, Weight: float.Clamp(entry.Weight, 0f, 1f)))
            .Filter(static entry => entry.Weight > 0f));

    internal Seq<Hosted> Hosted(Scene scene, Duration now) =>
        scene.Hosted.Map(hosted => Shift(scene, hosted.Path, now) switch {
            var shift => hosted with { Frame = Offset(hosted.Frame, shift) },
        });

    private SKPoint Shift(Scene scene, Seq<Key> path, Duration now) =>
        Scene.Above(path).Choose(above => Transitions.Find(above).Bind(transition => scene.Index.Find(above).Map(laid => transition.Shift(laid, now)))).Fold(SKPoint.Empty, static (sum, shift) => sum + shift);

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
    internal (Animations Animations, Seq<SKRect> Damage) Fading(Scene prior, Scene scene, LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> was,
                                                                LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> active, Duration now) =>
        was.SymmetricExcept(active) switch {
            var changed => (this with { Fades = changed.ToSeq().Fold(Fades.Filter((key, _) => scene.Index.ContainsKey(key.Path)), (fades, key) => Faded(scene, fades, key, active, now)) },
                Repainted(prior, scene, was.Union(active).Union(toHashSet(Fades.Filter(fade => fade.At(now).IsLoop).Keys)), changed)),
        };

    private static Seq<SKRect> Repainted(Scene prior, Scene scene, LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> washed, LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> changed) =>
        from key in toSeq(washed)
        let now = Washed(scene, key)
        let was = Washed(prior, key).Bind(static held => held.Beyond)
        let beyond = now.Bind(static held => held.Beyond)
        from damage in toSeq(((changed.Contains(key) ? now.Map(static held => held.Damage).ToSeq() : []) + (was == beyond ? [] : Seq(was, beyond).Somes())).Distinct())
        select damage;

    private static HashMap<(Seq<Key> Path, Interaction Interaction), Animated> Faded(Scene scene, HashMap<(Seq<Key> Path, Interaction Interaction), Animated> fades, (Seq<Key> Path, Interaction Interaction) key,
                                                                                LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> active, Duration now) =>
        (Curve: Washed(scene, key).Map(static held => held.Motion).IfNone(new TimingCurve.Instant()), Target: active.Contains(key) ? 1f : 0f) switch {
            var (curve, target) => fades.Find(key).Map(fade => fade.Toward(target, now, curve)).IfNone(new Animated(1f - target, target, now, curve)) switch {
                var fade when fade.At(now).IsLoop => fades.AddOrUpdate(key, fade),
                _ => fades.Remove(key),
            },
        };

    private static Option<(SKRect Damage, Option<SKRect> Beyond, TimingCurve Motion)> Washed(Scene scene, (Seq<Key> Path, Interaction Interaction) key) =>
        key.Interaction.Ring
            ? scene.Ring(key.Path).Map(static ring => SKRect.Intersect(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), ring.Clip) switch {
                var reach => (reach, Some(reach), ring.Style.Motion),
            })
            : from laid in scene.Index.Find(key.Path)
              from bounds in scene.Bounds(key.Path)
              select (bounds, Option<SKRect>.None, laid.Style.Motion);

    // --- [TIMERS]
    internal Animations Timed(Scene scene, Option<SKPoint> pointer, Duration now) =>
        this with {
            Timers = toHashMap(scene.Order.Choose(laid => laid.Node.Expires.Map(expiry => (laid.Path,
                Timers.Find(laid.Path).Map(held => held with { Expiry = expiry }).IfNone(() => Countdown.Started(expiry, now))
                    .Held(pointer.Exists(at => scene.Visible(laid.Path).Exists(shown => shown.Contains(at))), now))))),
        };

    // --- [TICK]
    internal (Animations Animations, Seq<SKRect> Damage, Seq<IO<Unit>> Elapsed) Tick(Scene scene, Duration prior, Duration now) =>
        Timers.Filter(countdown => countdown.Deadline.Exists(deadline => deadline <= now)) switch {
            var elapsed => (
                new Animations(
                    Fades.Filter(fade => fade.At(now).IsLoop),
                    Transitions.Filter(transition => transition.Moving(now)),
                    Leaving.Filter(left => left.Opacity.At(now).IsLoop),
                    Timers.AddOrUpdateRange(elapsed.Map(static countdown => countdown.Spent).AsIterable())),
                Fades.Keys.ToSeq().Choose(key => Washed(scene, key).Map(static held => held.Damage))
                    .Concat(Transitions.Keys.ToSeq().Choose(path => Reached(scene, path).Map(held =>
                        SKRect.Intersect(SKRect.Union(Offset(held.Reach, Shift(scene, path, prior)), Offset(held.Reach, Shift(scene, path, now))), held.Clip))))
                    .Concat(Leaving.Values.ToSeq().Map(static left => left.Reach)),
                elapsed.Values.ToSeq().Map(static countdown => countdown.Expiry.Elapsed)),
        };

    private static Option<(SKRect Reach, SKRect Clip)> Reached(Scene scene, Seq<Key> path) =>
        from laid in scene.Index.Find(path)
        from place in scene.Places.Find(path)
        select (SKRect.Create(laid.Reach.Location + place.Origin, laid.Reach.Size), place.Clip);

    private static SKRect Offset(SKRect rect, SKPoint by) => SKRect.Create(rect.Location + by, rect.Size);
}
