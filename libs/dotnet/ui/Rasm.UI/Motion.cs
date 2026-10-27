using System.Numerics;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
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

    public static Fin<Easing> Validated(double x1, double y1, double x2, double y2) =>
        UIFault.Unless(x1 is >= 0d and <= 1d && x2 is >= 0d and <= 1d && double.IsFinite(y1) && double.IsFinite(y2), UIReason.Range, (x1, y1, x2, y2)).Map(_ => new Easing(x1, y1, x2, y2));

    public Option<(double Progress, double Slope)> At(Fraction time) =>
        Brent.TryFindRoot(at => Bezier(at, X1, X2) - (float)time, 0d, 1d, 1e-12, 64, out double s)
            ? (Tangent(s, X1, X2), Tangent(s, Y1, Y2)) switch {
                var (across, up) => Some((Bezier(s, Y1, Y2), across is 0d && up is 0d ? Acceleration(s, Y1, Y2) / Acceleration(s, X1, X2) : up / across)),
            }
            : None;

    private static double Bezier(double s, double first, double second) => (3d * (1d - s) * (1d - s) * s * first) + (3d * (1d - s) * s * s * second) + (s * s * s);

    private static double Tangent(double s, double first, double second) => (3d * (1d - s) * (1d - s) * first) + (6d * (1d - s) * s * (second - first)) + (3d * s * s * (1d - second));

    private static double Acceleration(double s, double first, double second) => (6d * (1d - s) * (second - (2d * first))) + (6d * s * (1d - (2d * second) + first));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimingCurve {
    // --- [SAMPLE]
    internal abstract Option<(double Progress, double Rate)> At(Duration elapsed);

    internal virtual TimingCurve WithVelocity(double velocity) => this;

    internal bool Animates => Map(instant: false, eased: true, spring: true, decay: true);

    private static Option<(double Progress, double Rate)> Oscillated(double damping, double phase, double start, Factor rest) {
        double root = double.Sqrt(double.Abs((1d - damping) * (1d + damping)));
        double decay = double.Exp(-damping * phase);
        double slow = double.Exp(-phase / (damping + root));
        double gap = double.ExpM1(-2d * root * phase);
        (double even, double odd) = (damping - 1d) switch {
            < 0d => (decay * double.Cos(root * phase), decay * double.Sin(root * phase) / root),
            > 0d => (0.5d * slow * (2d + gap), -0.5d * slow * gap / root),
            _ => (decay, phase * decay),
        };
        double remaining = even + ((damping - start) * odd);
        double rate = (start * even) + ((1d - (damping * start)) * odd);
        return double.Abs(remaining) < (float)rest && double.Abs(rate) < (float)rest ? None : Some((1d - remaining, rate));
    }

    // --- [CASES]
    public sealed record Instant : TimingCurve {
        internal override Option<(double Progress, double Rate)> At(Duration elapsed) => None;
    }

    public sealed record Eased(Duration Length, Easing Easing) : TimingCurve {
        internal override Option<(double Progress, double Rate)> At(Duration elapsed) =>
            Fraction.Validated((float)(elapsed / Length)).ToOption().Filter(_ => elapsed < Length).Bind(Easing.At).Map(sampled => (sampled.Progress, sampled.Slope / Length.TotalSeconds));
    }

    public sealed record Spring(Duration Response, Factor Damping, Factor Rest) : TimingCurve {
        public double Initial { get; init; }

        private double Frequency => double.Tau / Response.TotalSeconds;

        internal override Option<(double Progress, double Rate)> At(Duration elapsed) =>
            Oscillated((float)Damping, double.Tau * (elapsed / Response), Initial / Frequency, Rest).Map(moved => (moved.Progress, Frequency * moved.Rate));

        internal override TimingCurve WithVelocity(double velocity) => this with { Initial = velocity };
    }

    public sealed record Decay(Duration TimeConstant, Factor Rest) : TimingCurve {
        public double Initial { get; init; } = 1d / TimeConstant.TotalSeconds;

        internal override Option<(double Progress, double Rate)> At(Duration elapsed) =>
            Oscillated(1d, elapsed / TimeConstant, Initial * TimeConstant.TotalSeconds, Rest).Map(moved => (moved.Progress, moved.Rate / TimeConstant.TotalSeconds));

        internal override TimingCurve WithVelocity(double velocity) => this with { Initial = velocity };
    }
}

internal sealed record Animated(float From, float To, Duration Start, TimingCurve Curve) {
    internal float Value(Duration now) => float.Lerp(From, To, (float)Curve.At(now - Start).Map(static at => at.Progress).IfNone(1d));

    internal bool Settled(Duration now) => Curve.At(now - Start).IsNone;

    internal double Rate(Duration now) => Curve.At(now - Start).Map(static at => at.Rate).IfNone(0d) * (To - From);

    internal Animated Retargeted(float target, Duration now, TimingCurve curve) {
        if (To.CompareTo(target) is 0) return this;
        float at = Value(now);
        return new(at, target, now, curve.WithVelocity(at.CompareTo(target) is 0 ? 0d : Rate(now) / (target - at)));
    }

    internal static Animated AtRest(float value, Duration now) => new(value, value, now, new TimingCurve.Instant());
}

internal sealed record Tween(SKRect Frame, float Alpha) {
    internal Option<Matrix4x4> Transform { get; init; }
    internal Option<PlotPlane> Plane { get; init; }
    internal Option<Figure> Figure { get; init; }

    internal static Tween Of(LayoutNode layout) => new(layout.Frame, (float)layout.Node.Opacity);

    internal static Option<Tween> Toward(Tween from, Tween to, float progress) =>
        Some(new Tween(new SKRect(float.Lerp(from.Frame.Left, to.Frame.Left, progress), float.Lerp(from.Frame.Top, to.Frame.Top, progress), float.Lerp(from.Frame.Right, to.Frame.Right, progress), float.Lerp(from.Frame.Bottom, to.Frame.Bottom, progress)),
            float.Lerp(from.Alpha, to.Alpha, progress)));

    internal static float Distance(Tween from, Tween to) =>
        float.Hypot(float.Hypot(to.Frame.Left - from.Frame.Left, to.Frame.Top - from.Frame.Top), float.Hypot(float.Hypot(to.Frame.Right - from.Frame.Right, to.Frame.Bottom - from.Frame.Bottom), to.Alpha - from.Alpha));

    internal SKPoint Shift(LayoutNode layout) => Frame.Location - layout.Frame.Location;
}

internal sealed record Morph<T>(T From, T To, Animated Progress) where T : notnull {
    internal T At(Duration now, Func<T, T, float, Option<T>> toward) => toward(From, To, Progress.Value(now)).IfNone(To);

    internal Morph<T> Retargeted(T to, Duration now, TimingCurve curve, Func<T, T, float, Option<T>> toward, Func<T, T, float> distance) {
        if (To.Equals(to)) return this;
        T at = At(now, toward);
        float remaining = distance(at, to);
        return new(at, to, new Animated(0f, 1f, now, curve.WithVelocity(remaining > 0f ? Progress.Rate(now) * distance(From, To) / remaining : 0d)));
    }

    internal static Option<Morph<T>> Retargeted(Option<Morph<T>> open, T was, T to, Duration now, TimingCurve curve, Func<T, T, float, Option<T>> toward) =>
        open.Filter(held => held.To.Equals(to))
        | Some(open.Map(held => held.At(now, toward)).IfNone(was)).Filter(start => !start.Equals(to) && toward(start, to, 0f).IsSome).Map(start => new Morph<T>(start, to, new Animated(0f, 1f, now, curve)));
}

internal sealed record ExitTransition(LayoutNode Last, Place Place, Animated Opacity) {
    internal SKPoint At => Last.Frame.Location + Place.Origin;

    internal SKRect Bounds => SKRect.Intersect(Scene.Absolute(Last.SubtreeBounds, Place.Origin), Place.Clip);
}

internal sealed record Countdown(Duration Left, Option<Duration> Since) {
    internal static Countdown Expired { get; } = new(Duration.Zero, None);

    internal Option<Duration> Deadline => Since.Map(since => since + Left);

    internal static Countdown Started(Duration left, Duration now) => new(left, Some(now));

    internal Countdown Held(bool held, Duration now) =>
        Since.Match(
            Some: since => held && Left - (now - since) > Duration.Zero ? new Countdown(Left - (now - since), None) : this,
            None: () => !held && Left > Duration.Zero ? this with { Since = Some(now) } : this);
}

internal sealed record InteractionState(Seq<Key> Path, Interaction Interaction);

internal sealed record Animations {
    // --- [STATE]
    internal static Animations Idle { get; } = new();

    internal HashMap<InteractionState, Animated> Fades { get; init; }
    internal HashMap<Seq<Key>, Morph<Tween>> Transitions { get; init; }
    internal HashMap<Seq<Key>, Morph<Matrix4x4>> Transforms { get; init; }
    internal HashMap<Seq<Key>, Morph<PlotPlane>> Planes { get; init; }
    internal HashMap<Seq<Key>, Morph<Figure>> Figures { get; init; }
    internal HashMap<Seq<Key>, ExitTransition> Exiting { get; init; }
    internal HashMap<Seq<Key>, ExitTransition> Departing { get; init; }
    internal HashMap<Seq<Key>, Countdown> Expiries { get; init; }
    internal HashMap<Seq<Key>, Countdown> Hovers { get; init; }

    // --- [DERIVED]
    internal bool Animating =>
        !Fades.IsEmpty || !Transitions.IsEmpty || !Transforms.IsEmpty || !Planes.IsEmpty || !Figures.IsEmpty || !Exiting.IsEmpty || !Departing.IsEmpty;

    internal Option<Duration> Due => (toSeq(Expiries.Values) + toSeq(Hovers.Values)).Choose(static countdown => countdown.Deadline).Min<Seq, Duration>();

    internal Seq<ExitTransition> Leaving => toSeq(Exiting.Values) + toSeq(Departing.Values);

    internal HashMap<InteractionState, float> StateLayers(HashSet<InteractionState> active, Duration now) =>
        toHashMap(active.ToSeq().Map(key => (Key: key, Weight: Fades.Find(key).Map(fade => fade.Value(now)).IfNone(1f)))
            .Concat(toSeq(Fades.Filter((key, _) => !active.Contains(key)).AsIterable()).Map(entry => (entry.Key, Weight: entry.Value.Value(now))))
            .Map(static entry => (entry.Key, Weight: float.Clamp(entry.Weight, 0f, 1f)))
            .Filter(static entry => entry.Weight > 0f));

    internal Seq<Hosted> Hosted(Scene scene, Duration now) =>
        scene.Hosted.Map(hosted => hosted with { Frame = Scene.Absolute(hosted.Frame, Shift(scene, hosted.Path, now)) });

    internal Option<Tween> Tweened(LayoutNode layout, Duration now) {
        Option<Tween> framed = Transitions.Find(layout.Path).Map(transition => transition.At(now, Tween.Toward));
        Option<Matrix4x4> transform = Transforms.Find(layout.Path).Map(tween => tween.At(now, Blended));
        Option<PlotPlane> plane = Planes.Find(layout.Path).Map(tween => tween.At(now, static (from, to, progress) => from.Toward(to, progress)));
        Option<Figure> figure = Figures.Find(layout.Path).Map(tween => tween.At(now, static (from, to, progress) => from.Toward(to, progress)));
        return framed.IsSome || transform.IsSome || plane.IsSome || figure.IsSome
            ? Some(framed.IfNone(() => Tween.Of(layout)) with { Transform = transform, Plane = plane, Figure = figure })
            : None;
    }

    private Seq<SKRect> Reach(Scene scene, Duration at) =>
        Fades.Keys.ToSeq().Choose(key => Bounds(scene, key).Map(static held => held.Damage))
        + (Transitions.Keys.ToSeq() + Planes.Keys.ToSeq() + Figures.Keys.ToSeq()).Choose(path =>
            from layout in scene.Index.Find(path)
            from place in scene.Places.Find(path)
            select SKRect.Intersect(Scene.Absolute(Scene.Absolute(layout.SubtreeBounds, place.Origin), Shift(scene, path, at)), place.Clip))
        + Transforms.Keys.ToSeq().Choose(path => scene.Places.Find(path).Map(static place => place.Clip))
        + Leaving.Map(static left => left.Bounds);

    private SKPoint Shift(Scene scene, Seq<Key> path, Duration now) =>
        Scene.AncestorsAndSelf(path).Choose(above =>
            from transition in Transitions.Find(above)
            from layout in scene.Index.Find(above)
            select transition.At(now, Tween.Toward).Shift(layout)).Fold(SKPoint.Empty, static (sum, shift) => sum + shift);

    private static bool Reduced(Scene scene) => scene.Layout.Cascade.Theme.Conditions.Contains(Condition.ReduceMotion);

    // --- [OPEN]
    internal Animations Started(Scene prior, Scene next, Duration now) {
        bool settled = Reduced(next);
        return this with {
            Transitions = toHashMap(next.Order.Filter(static layout => layout.Node.Key.IsSome).Choose(layout => Transited(prior, layout, now, settled).Map(transition => (layout.Path, transition)))),
            Transforms = settled ? [] : Morphs(prior, next, Transforms, now, static layout => Some(layout.Node.Transform.IfNone(Matrix4x4.Identity)), Blended),
            Planes = settled ? [] : Morphs(prior, next, Planes, now, static layout => layout.Node.Plotted, static (from, to, progress) => from.Toward(to, progress)),
            Figures = settled ? [] : Morphs(prior, next, Figures, now, static layout => layout.Node.Traced, static (from, to, progress) => from.Toward(to, progress)),
            Exiting = Exiting.Filter((path, _) => !next.Index.ContainsKey(path)).AddOrUpdateRange(prior.Order
                .Filter(layout => layout.Node.Key.IsSome && layout.Style.Motion.Animates && !next.Index.ContainsKey(layout.Path) && next.Index.ContainsKey(layout.Path.Init))
                .Choose(layout => prior.Places.Find(layout.Path).Map(place => (layout.Path, new ExitTransition(layout, place,
                    new Animated(Transitions.Find(layout.Path).Map(transition => transition.At(now, Tween.Toward).Alpha).IfNone((float)layout.Node.Opacity), 0f, now, layout.Style.Motion)))))),
            Departing = Departed(prior, next, now),
        };
    }

    internal Animations Restyled(Scene prior, Scene next, Duration now) =>
        ReferenceEquals(prior, next) ? this : (Reduced(next) && !Reduced(prior) ? Snapped : this) with { Departing = Departed(prior, next, now) };

    internal Animations Pruned(Scene scene) =>
        this with {
            Transitions = Transitions.Filter((path, transition) => scene.Index.Find(path).Exists(layout => transition.To == Tween.Of(layout))),
            Transforms = Transforms.Filter((path, tween) => scene.Index.Find(path).Exists(layout => layout.Node.Transform.IfNone(Matrix4x4.Identity).Equals(tween.To))),
            Planes = Planes.Filter((path, tween) => scene.Index.Find(path).Exists(layout => layout.Node.Plotted == Some(tween.To))),
            Figures = Figures.Filter((path, tween) => scene.Index.Find(path).Exists(layout => layout.Node.Traced == Some(tween.To))),
            Departing = Departing.Filter((path, _) => scene.Index.ContainsKey(path)),
        };

    private Animations Snapped =>
        this with {
            Transitions = Transitions.Map(static open => open with { From = open.From with { Frame = open.To.Frame } }).Filter(static open => open.From != open.To),
            Transforms = [],
            Planes = [],
            Figures = [],
        };

    private Option<Morph<Tween>> Transited(Scene prior, LayoutNode layout, Duration now, bool settled) {
        Tween target = Tween.Of(layout);
        Tween Framed(Tween held) => settled ? held with { Frame = target.Frame } : held;
        Option<Morph<Tween>> Moved(Morph<Tween> open, TimingCurve curve) =>
            Some((open with { From = Framed(open.From), To = Framed(open.To) }).Retargeted(target, now, curve, Tween.Toward, Tween.Distance)).Filter(static moved => moved.From != moved.To);
        Morph<Tween> Rested(Tween from) => new(from, from, Animated.AtRest(1f, now));
        return from curve in Some(layout.Style.Motion).Filter(static motion => motion.Animates)
               from transition in Transitions.Find(layout.Path).Match(
                   Some: open => Moved(open, curve),
                   None: () => prior.Index.Find(layout.Path).Match(
                       Some: was => Tween.Of(was) != target ? Moved(Rested(Tween.Of(was)), curve) : None,
                       None: () => layout.Path.Count > 1 && prior.Index.ContainsKey(layout.Path.Init)
                           ? Moved(Rested(target with { Alpha = Exiting.Find(layout.Path).Map(left => left.Opacity.Value(now)).IfNone(0f) }), curve)
                           : None))
               select transition;
    }

    private HashMap<Seq<Key>, ExitTransition> Departed(Scene prior, Scene next, Duration now) {
        bool Restyled(Seq<Key> path) =>
            prior.Index.Find(path).Exists(was => next.Index.Find(path).Exists(kept => kept.Node.Key.IsSome && kept.Style.Motion.Animates && was.Style.Conditions != kept.Style.Conditions));
        return Departing.Filter((path, _) => next.Index.ContainsKey(path)).AddOrUpdateRange(next.Order
            .Filter(layout => Restyled(layout.Path) && !Scene.AncestorsAndSelf(layout.Path).Tail.Exists(Restyled))
            .Choose(layout =>
                from was in prior.Index.Find(layout.Path)
                from place in prior.Places.Find(layout.Path)
                select (layout.Path, new ExitTransition(was, place, new Animated(Departing.Find(layout.Path).Map(held => held.Opacity.Value(now)).IfNone(1f), 0f, now, layout.Style.Motion)))));
    }

    private static HashMap<Seq<Key>, Morph<T>> Morphs<T>(Scene prior, Scene next, HashMap<Seq<Key>, Morph<T>> open, Duration now, Func<LayoutNode, Option<T>> value, Func<T, T, float, Option<T>> toward)
        where T : notnull =>
        toHashMap(next.Order.Filter(static layout => layout.Style.Motion.Animates).Choose(layout =>
            from to in value(layout)
            from was in prior.Index.Find(layout.Path).Bind(value)
            from tween in Morph<T>.Retargeted(open.Find(layout.Path), was, to, now, layout.Style.Motion, toward)
            select (layout.Path, tween)));

    private static Option<Matrix4x4> Blended(Matrix4x4 from, Matrix4x4 to, float progress) =>
        Affine(from) && Affine(to)
        && Matrix4x4.Decompose(from, out Vector3 fromScale, out Quaternion fromTurn, out Vector3 fromShift)
        && Matrix4x4.Decompose(to, out Vector3 toScale, out Quaternion toTurn, out Vector3 toShift)
            ? Some(Matrix4x4.CreateScale(Vector3.Lerp(fromScale, toScale, progress)) * Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(fromTurn, toTurn, progress))
                * Matrix4x4.CreateTranslation(Vector3.Lerp(fromShift, toShift, progress)))
            : None;

    private static bool Affine(Matrix4x4 matrix) => matrix is { M14: 0f, M24: 0f, M34: 0f, M44: 1f };

    // --- [FADE]
    internal (Animations Animations, Seq<SKRect> Damage) Fading(Scene prior, Scene scene, HashSet<InteractionState> was, HashSet<InteractionState> active, Duration now) {
        HashSet<InteractionState> changed = was.SymmetricExcept(active);
        return (this with { Fades = changed.ToSeq().Fold(Fades.Filter((key, _) => scene.Index.ContainsKey(key.Path)), (fades, key) => Faded(scene, fades, key, active, now)) },
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

    private static Option<(SKRect Damage, Option<SKRect> Beyond, TimingCurve Motion)> Bounds(Scene scene, InteractionState key) =>
        key.Interaction == Interaction.Focused
            ? scene.FocusRing(key.Path).Map(static ring => SKRect.Intersect(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), ring.Place.Clip) switch {
                var reach => (reach, Some(reach), ring.Style.Motion),
            })
            : from layout in scene.Index.Find(key.Path)
              from bounds in scene.Bounds(key.Path)
              select (bounds, Option<SKRect>.None, layout.Style.Motion);

    // --- [TIMERS]
    internal Animations WithTimers(Scene scene, Option<SKPoint> pointer, HashMap<Seq<Key>, Duration> hovering, Duration now) =>
        this with {
            Expiries = toHashMap(scene.Order.Choose(layout => layout.Node.Expires.Map(expiry => (layout.Path,
                Expiries.Find(layout.Path).IfNone(() => Countdown.Started(expiry.Lifetime, now))
                    .Held(held: pointer.Exists(at => scene.Visible(layout.Path).Exists(shown => shown.Contains(at))), now))))),
            Hovers = hovering.Map((path, delay) => Hovers.Find(path).IfNone(() => Countdown.Started(delay, now)).Held(held: false, now)),
        };

    // --- [TICK]
    internal (Animations Animations, Seq<SKRect> Damage, Seq<IO<Unit>> Elapsed, HashSet<Seq<Key>> Opened) Tick(Scene scene, Duration prior, Duration now) {
        HashMap<Seq<Key>, Countdown> expired = Expiries.Filter(countdown => countdown.Deadline.Exists(deadline => deadline <= now));
        HashMap<Seq<Key>, Countdown> opened = Hovers.Filter(countdown => countdown.Deadline.Exists(deadline => deadline <= now));
        return (
            this with {
                Fades = Fades.Filter(fade => !fade.Settled(now)),
                Transitions = Transitions.Filter(transition => !transition.Progress.Settled(now)),
                Transforms = Transforms.Filter(tween => !tween.Progress.Settled(now)),
                Planes = Planes.Filter(tween => !tween.Progress.Settled(now)),
                Figures = Figures.Filter(tween => !tween.Progress.Settled(now)),
                Exiting = Exiting.Filter(left => !left.Opacity.Settled(now)),
                Departing = Departing.Filter(left => !left.Opacity.Settled(now)),
                Expiries = Expiries.AddOrUpdateRange(expired.Map(static _ => Countdown.Expired).AsIterable()),
                Hovers = Hovers.RemoveRange(opened.Keys),
            },
            Reach(scene, prior) + Reach(scene, now),
            expired.Keys.ToSeq().Choose(path => scene.Index.Find(path).Bind(static layout => layout.Node.Expires)).Map(static expiry => expiry.Elapsed),
            toHashSet(opened.Keys));
    }
}
