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

internal sealed record SpringState(double Progress, double Rate);

internal sealed record Mode(double Even, double Odd);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimingCurve {
    // --- [SAMPLE]
    private static readonly Sample End = new(1d, Settled: true);

    public abstract Sample Sample(Duration elapsed);

    public abstract double Velocity(Duration elapsed);

    public virtual TimingCurve WithVelocity(double velocity) => this;

    internal bool Animates => Map(instant: false, eased: true, spring: true, decay: true);

    private static Sample Sampled(Option<SpringState> moved) => moved.Match(Some: static held => new Sample(held.Progress, Settled: false), None: static () => End);

    private static Option<SpringState> Oscillated(double damping, double phase, double start, VisibleChange rest) {
        Mode mode = Modes(damping, phase);
        double remaining = mode.Even + ((damping - start) * mode.Odd);
        double rate = (start * mode.Even) + ((1d - (damping * start)) * mode.Odd);
        return double.Abs(remaining) < rest && double.Abs(rate) < rest ? None : Some(new SpringState(1d - remaining, rate));
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

        public override TimingCurve WithVelocity(double velocity) => this with { Initial = velocity };

        private Option<SpringState> Moved(Duration elapsed) => Oscillated((double)Damping, double.Tau * (elapsed / Response), Initial / Frequency, Rest);
    }

    public sealed record Decay(Duration TimeConstant, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; } = 1d / TimeConstant.TotalSeconds;

        public override Sample Sample(Duration elapsed) => Sampled(Moved(elapsed));

        public override double Velocity(Duration elapsed) => Moved(elapsed).Map(moved => moved.Rate / TimeConstant.TotalSeconds).IfNone(0d);

        public override TimingCurve WithVelocity(double velocity) => this with { Initial = velocity };

        private Option<SpringState> Moved(Duration elapsed) => Oscillated(1d, elapsed / TimeConstant, Initial * TimeConstant.TotalSeconds, Rest);
    }
}

public sealed record Animated(float From, float To, Duration Start, TimingCurve Curve) {
    public float Value(Duration now) => float.Lerp(From, To, (float)Curve.Sample(now - Start).Value);

    public bool Settled(Duration now) => Curve.Sample(now - Start).Settled;

    public Animated Retargeted(float target, Duration now, TimingCurve curve) {
        if (To.CompareTo(target) is 0) return this;
        float at = Value(now);
        return new(at, target, now, curve.WithVelocity(at.CompareTo(target) is 0 ? 0d : Curve.Velocity(now - Start) * (To - From) / (target - at)));
    }

    internal static Animated AtRest(float value, Duration now) => new(value, value, now, new TimingCurve.Instant());
}

internal sealed record Transition(Animated X, Animated Y, Animated Width, Animated Height, Animated Opacity) {
    internal static Transition AtRest(LayoutNode layout, float opacity, Duration now) =>
        new(Animated.AtRest(layout.Frame.Left, now), Animated.AtRest(layout.Frame.Top, now), Animated.AtRest(layout.Frame.Width, now), Animated.AtRest(layout.Frame.Height, now), Animated.AtRest(opacity, now));

    internal Transition Retargeted(LayoutNode layout, Duration now, TimingCurve curve) =>
        new(X.Retargeted(layout.Frame.Left, now, curve), Y.Retargeted(layout.Frame.Top, now, curve), Width.Retargeted(layout.Frame.Width, now, curve), Height.Retargeted(layout.Frame.Height, now, curve),
            Opacity.Retargeted(layout.Alpha, now, curve));

    internal bool Targets(LayoutNode layout) =>
        X.To.CompareTo(layout.Frame.Left) is 0 && Y.To.CompareTo(layout.Frame.Top) is 0 && Width.To.CompareTo(layout.Frame.Width) is 0 && Height.To.CompareTo(layout.Frame.Height) is 0
        && Opacity.To.CompareTo(layout.Alpha) is 0;

    internal bool Animating(Duration now) => Seq(X, Y, Width, Height, Opacity).Exists(animated => !animated.Settled(now));

    internal SKPoint Shift(LayoutNode layout, Duration now) => new(X.Value(now) - layout.Frame.Left, Y.Value(now) - layout.Frame.Top);

    internal Tween Tweened(LayoutNode layout, Duration now) => new(Shift(layout, now), new SKSize(Width.Value(now), Height.Value(now)), Opacity.Value(now));
}

internal sealed record Tween(SKPoint Shift, SKSize Size, float Alpha);

internal sealed record ExitTransition(LayoutNode Last, Place Place, Animated Opacity) {
    internal SKPoint At => Last.Frame.Location + Place.Origin;

    internal SKRect Bounds => SKRect.Intersect(Scene.Absolute(Last.SubtreeBounds, Place.Origin), Place.Clip);
}

internal sealed record Countdown(Expiry Expiry, Duration Left, Option<Duration> Since) {
    internal static Countdown Started(Expiry expiry, Duration now) => new(expiry, expiry.Lifetime, Some(now));

    internal Option<Duration> Deadline => Since.Map(since => since + Left);

    internal Countdown Expired => this with { Left = Duration.Zero, Since = None };

    internal Countdown Paused(bool hovered, Duration now) =>
        Since.Match(
            Some: since => hovered && Left - (now - since) > Duration.Zero ? this with { Left = Left - (now - since), Since = None } : this,
            None: () => !hovered && Left > Duration.Zero ? this with { Since = Some(now) } : this);
}

public sealed record InteractionState(Seq<Key> Path, Interaction Interaction);

internal sealed record Animations(
    HashMap<InteractionState, Animated> Fades,
    HashMap<Seq<Key>, Transition> Transitions,
    HashMap<Seq<Key>, ExitTransition> Exiting,
    HashMap<Seq<Key>, Countdown> Timers) {
    // --- [DERIVED]
    internal static Animations Idle { get; } = new([], [], [], []);

    internal bool Animating => !Fades.IsEmpty || !Transitions.IsEmpty || !Exiting.IsEmpty;

    internal Option<Duration> Due =>
        toSeq(Timers.Values).Choose(static countdown => countdown.Deadline).Fold(Option<Duration>.None, static (least, deadline) => Some(least.Match(Some: held => Duration.Min(held, deadline), None: () => deadline)));

    internal HashMap<InteractionState, float> StateLayers(HashSet<InteractionState> active, Duration now) =>
        toHashMap(active.ToSeq().Map(key => (Key: key, Weight: Fades.Find(key).Map(fade => fade.Value(now)).IfNone(1f)))
            .Concat(toSeq(Fades.Filter((key, _) => !active.Contains(key)).AsIterable()).Map(entry => (entry.Key, Weight: entry.Value.Value(now))))
            .Map(static entry => (entry.Key, Weight: float.Clamp(entry.Weight, 0f, 1f)))
            .Filter(static entry => entry.Weight > 0f));

    internal Seq<Hosted> Hosted(Scene scene, Duration now) =>
        scene.Hosted.Map(hosted => hosted with { Frame = Scene.Absolute(hosted.Frame, Shift(scene, hosted.Path, now)) });

    private SKPoint Shift(Scene scene, Seq<Key> path, Duration now) =>
        Scene.AncestorsAndSelf(path).Choose(above =>
            from transition in Transitions.Find(above)
            from layout in scene.Index.Find(above)
            select transition.Shift(layout, now)).Fold(SKPoint.Empty, static (sum, shift) => sum + shift);

    // --- [OPEN]
    internal Animations Started(Scene prior, Scene next, Duration now) =>
        this with {
            Transitions = toHashMap(next.Order.Filter(static layout => layout.Node.Key.IsSome).Choose(layout => Transited(prior, layout, now).Map(transition => (layout.Path, transition)))),
            Exiting = Exiting.Filter((path, _) => !next.Index.ContainsKey(path)).AddOrUpdateRange(prior.Order
                .Filter(layout => layout.Node.Key.IsSome && layout.Style.Motion.Animates && !next.Index.ContainsKey(layout.Path) && next.Index.ContainsKey(layout.Path.Init))
                .Choose(layout => prior.Places.Find(layout.Path).Map(place => (layout.Path, new ExitTransition(layout, place,
                    new Animated(Transitions.Find(layout.Path).Map(transition => transition.Opacity.Value(now)).IfNone(layout.Alpha), 0f, now, layout.Style.Motion)))))),
        };

    internal Animations Pruned(Scene scene) =>
        this with { Transitions = Transitions.Filter((path, transition) => scene.Index.Find(path).Exists(transition.Targets)) };

    private Option<Transition> Transited(Scene prior, LayoutNode layout, Duration now) =>
        from curve in Some(layout.Style.Motion).Filter(static motion => motion.Animates)
        from transition in Transitions.Find(layout.Path).Match(
            Some: open => Some(open.Retargeted(layout, now, curve)),
            None: () => prior.Index.Find(layout.Path).Match(
                Some: was => was.Frame != layout.Frame || was.Node.Alpha.CompareTo(layout.Node.Alpha) is not 0 ? Some(Transition.AtRest(was, was.Alpha, now).Retargeted(layout, now, curve)) : None,
                None: () => layout.Path.Count > 1 && prior.Index.ContainsKey(layout.Path.Init)
                    ? Some(Transition.AtRest(layout, Exiting.Find(layout.Path).Map(left => left.Opacity.Value(now)).IfNone(0f), now).Retargeted(layout, now, curve))
                    : None))
        select transition;

    // --- [FADE]
    internal sealed record StateLayerChange(Animations Animations, Seq<SKRect> Damage);

    private sealed record StateLayerBounds(SKRect Damage, Option<SKRect> Beyond, TimingCurve Motion);

    internal StateLayerChange Fading(Scene prior, Scene scene, HashSet<InteractionState> was, HashSet<InteractionState> active, Duration now) {
        HashSet<InteractionState> changed = was.SymmetricExcept(active);
        return new(this with { Fades = changed.ToSeq().Fold(Fades.Filter((key, _) => scene.Index.ContainsKey(key.Path)), (fades, key) => Faded(scene, fades, key, active, now)) },
            Repainted(prior, scene, was.Union(active).Union(toHashSet(Fades.Filter(fade => !fade.Settled(now)).Keys)), changed));
    }

    private static Seq<SKRect> Repainted(Scene prior, Scene scene, HashSet<InteractionState> states, HashSet<InteractionState> changed) =>
        from key in toSeq(states)
        let now = Bounds(scene, key)
        let was = Bounds(prior, key).Bind(static held => held.Beyond)
        let beyond = now.Bind(static held => held.Beyond)
        from damage in toSeq(((changed.Contains(key) ? now.Map(static held => held.Damage).ToSeq() : []) + (was == beyond ? [] : Seq(was, beyond).Somes())).Distinct())
        select damage;

    private static HashMap<InteractionState, Animated> Faded(Scene scene, HashMap<InteractionState, Animated> fades, InteractionState key, HashSet<InteractionState> active, Duration now) {
        TimingCurve curve = Bounds(scene, key).Map(static held => held.Motion).IfNone(new TimingCurve.Instant());
        float target = active.Contains(key) ? 1f : 0f;
        Animated fade = fades.Find(key).Map(open => open.Retargeted(target, now, curve)).IfNone(new Animated(1f - target, target, now, curve));
        return fade.Settled(now) ? fades.Remove(key) : fades.AddOrUpdate(key, fade);
    }

    private static Option<StateLayerBounds> Bounds(Scene scene, InteractionState key) =>
        key.Interaction.ShowsFocusRing
            ? scene.FocusRing(key.Path).Map(static ring => {
                SKRect reach = SKRect.Intersect(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), ring.Clip);
                return new StateLayerBounds(reach, Some(reach), ring.Style.Motion);
            })
            : from layout in scene.Index.Find(key.Path)
              from bounds in scene.Bounds(key.Path)
              select new StateLayerBounds(bounds, None, layout.Style.Motion);

    // --- [TIMERS]
    internal Animations WithTimers(Scene scene, Option<SKPoint> pointer, Duration now) =>
        this with {
            Timers = toHashMap(scene.Order.Choose(layout => layout.Node.Expires.Map(expiry => (layout.Path,
                Timers.Find(layout.Path).Map(held => held with { Expiry = expiry }).IfNone(() => Countdown.Started(expiry, now))
                    .Paused(pointer.Exists(at => scene.Visible(layout.Path).Exists(shown => shown.Contains(at))), now))))),
        };

    // --- [TICK]
    internal sealed record Ticked(Animations Animations, Seq<SKRect> Damage, Seq<IO<Unit>> Elapsed);

    private sealed record ClippedBounds(SKRect Reach, SKRect Clip);

    internal Ticked Tick(Scene scene, Duration prior, Duration now) {
        HashMap<Seq<Key>, Countdown> elapsed = Timers.Filter(countdown => countdown.Deadline.Exists(deadline => deadline <= now));
        return new(
            new Animations(
                Fades.Filter(fade => !fade.Settled(now)),
                Transitions.Filter(transition => transition.Animating(now)),
                Exiting.Filter(left => !left.Opacity.Settled(now)),
                Timers.AddOrUpdateRange(elapsed.Map(static countdown => countdown.Expired).AsIterable())),
            Fades.Keys.ToSeq().Choose(key => Bounds(scene, key).Map(static held => held.Damage))
                .Concat(Transitions.Keys.ToSeq().Choose(path => TransitionBounds(scene, path).Map(held =>
                    SKRect.Intersect(SKRect.Union(Scene.Absolute(held.Reach, Shift(scene, path, prior)), Scene.Absolute(held.Reach, Shift(scene, path, now))), held.Clip))))
                .Concat(Exiting.Values.ToSeq().Map(static left => left.Bounds)),
            elapsed.Values.ToSeq().Map(static countdown => countdown.Expiry.Elapsed));
    }

    private static Option<ClippedBounds> TransitionBounds(Scene scene, Seq<Key> path) =>
        from layout in scene.Index.Find(path)
        from place in scene.Places.Find(path)
        select new ClippedBounds(Scene.Absolute(layout.SubtreeBounds, place.Origin), place.Clip);
}
