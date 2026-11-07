using System.Numerics;
using Rasm.UI.Hosting;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Motion;

// --- [TYPES] ---------------------------------------------------------------------------

internal interface IInterpolated<TSelf> where TSelf : IInterpolated<TSelf> {
    public Option<TSelf> Toward(TSelf to, float progress);
}

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record Morph<T>(T From, T To, Duration Start, TimingCurve Curve) where T : IInterpolated<T> {
    internal T At(Duration now) => From.Toward(To, (float)Curve.At(now - Start).Map(static at => at.Progress).IfNone(1d)).IfNone(To);

    internal bool Settled(Duration now) => Curve.At(now - Start).IsNone;

    internal Morph<T> Retargeted(T to, Duration now, TimingCurve curve, Func<T, T, float> distance) {
        if (To.Equals(to)) return this;
        T at = At(now);
        float remaining = distance(at, to);
        return new(at, to, now, curve.WithVelocity(remaining is 0f ? 0d : Curve.At(now - Start).Map(static held => held.Rate).IfNone(0d) * distance(From, To) / remaining));
    }
}

internal sealed record Tween(SKRect Frame, Fraction Alpha) : IInterpolated<Tween> {
    private Vector4 Edges => new(Frame.Left, Frame.Top, Frame.Right, Frame.Bottom);

    public Option<Tween> Toward(Tween to, float progress) =>
        Alpha.Toward(to.Alpha, progress).Map(alpha => Vector4.Lerp(Edges, to.Edges, progress) switch { var edges => new Tween(new SKRect(edges.X, edges.Y, edges.Z, edges.W), alpha) });

    internal SKPoint Shift(LayoutNode layout) => Frame.Location - layout.Frame.Location;

    internal static Tween Of(LayoutNode layout) => new(layout.Frame, layout.View.Opacity);

    internal static float Distance(Tween from, Tween to) => float.Hypot(Vector4.Distance(from.Edges, to.Edges), (float)to.Alpha - (float)from.Alpha);
}

internal sealed record Decomposition(Vector3 Scale, Quaternion Rotation, Vector3 Translation) : IInterpolated<Decomposition> {
    internal Matrix4x4 Matrix => Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Translation);

    public Option<Decomposition> Toward(Decomposition to, float progress) =>
        new Decomposition(Vector3.Lerp(Scale, to.Scale, progress), Quaternion.Slerp(Rotation, to.Rotation, progress), Vector3.Lerp(Translation, to.Translation, progress));

    internal static Option<Decomposition> Of(Option<Matrix4x4> transform) =>
        transform.IfNone(Matrix4x4.Identity) switch {
            { M14: 0f, M24: 0f, M34: 0f, M44: 1f } affine when Matrix4x4.Decompose(affine, out Vector3 scale, out Quaternion rotation, out Vector3 translation) => new Decomposition(scale, rotation, translation),
            _ => None,
        };
}

internal sealed record ExitTransition(LayoutNode Last, Place Place, Morph<Fraction> Opacity) {
    internal SKPoint At => Last.Frame.Location + Place.Origin;

    internal SKRect Bounds => SKRect.Intersect(Place.Absolute(Last.SubtreeBounds, Place.Origin), Place.Clip);
}

internal sealed record InteractionState(Seq<Key> Path, Interaction Interaction);

internal sealed record Animations {

    // --- [STATE]

    internal static Animations Idle { get; } = new();

    internal HashMap<InteractionState, Morph<Fraction>> Fades { get; init; }
    internal HashMap<Seq<Key>, Morph<Tween>> Transitions { get; init; }
    internal HashMap<Seq<Key>, Morph<Decomposition>> Transforms { get; init; }
    internal HashMap<Seq<Key>, Morph<PlotPlane>> Planes { get; init; }
    internal HashMap<Seq<Key>, Morph<Figure>> Figures { get; init; }
    internal HashMap<Seq<Key>, ExitTransition> Exiting { get; init; }
    internal HashMap<Seq<Key>, ExitTransition> Departing { get; init; }

    // --- [CHANNELS]

    private static readonly Func<LayoutNode, Option<Tween>> Framed = static layout => Some(Tween.Of(layout));
    private static readonly Func<LayoutNode, Option<Decomposition>> Turned = static layout => Decomposition.Of(layout.View.Transform);
    private static readonly Func<LayoutNode, Option<PlotPlane>> Plotted = static layout => layout.View.Plotted;
    private static readonly Func<LayoutNode, Option<Figure>> Traced = static layout => layout.View.Traced;
    private static readonly Func<Fraction, Fraction, float> Signed = static (from, to) => (float)to - (float)from;

    // --- [DERIVED]

    internal bool Animating => !Fades.IsEmpty || !Morphing.IsEmpty || !Leaving.IsEmpty;

    internal Seq<Seq<Key>> Morphing => Transitions.Keys.ToSeq() + Transforms.Keys.ToSeq() + Planes.Keys.ToSeq() + Figures.Keys.ToSeq();

    internal Seq<ExitTransition> Leaving => toSeq(Exiting.Values) + toSeq(Departing.Values);

    internal HashMap<InteractionState, float> StateLayers(HashSet<InteractionState> active, Duration now) =>
        toHashMap(active.ToSeq().Map(key => (Key: key, Weight: Fades.Find(key).Map(fade => (float)fade.At(now)).IfNone(1f)))
            .Concat(toSeq(Fades.Filter((key, _) => !active.Contains(key)).AsIterable()).Map(entry => (entry.Key, Weight: (float)entry.Value.At(now))))
            .Filter(static entry => entry.Weight > 0f));

    internal Seq<Hosted> Hosted(Scene scene, Duration now) =>
        scene.Hosted.Map(hosted => hosted with { Frame = Place.Absolute(hosted.Frame, Shift(scene, hosted.Path, now)) });

    private Seq<SKRect> Reach(Scene scene, Duration at) =>
        Fades.Keys.ToSeq().Choose(key => Bounds(scene, key).Map(static held => held.Damage))
        + (Transitions.Keys.ToSeq() + Planes.Keys.ToSeq() + Figures.Keys.ToSeq()).Choose(path =>
            from layout in scene.Index.Find(path)
            from place in scene.Places.Find(path)
            select SKRect.Intersect(Place.Absolute(Place.Absolute(layout.SubtreeBounds, place.Origin), Shift(scene, path, at)), place.Clip))
        + Transforms.Keys.ToSeq().Choose(path => scene.Places.Find(path).Map(static place => place.Clip))
        + Leaving.Map(static left => left.Bounds);

    private SKPoint Shift(Scene scene, Seq<Key> path, Duration now) =>
        path.Inits.Choose(above =>
            from transition in Transitions.Find(above)
            from layout in scene.Index.Find(above)
            select transition.At(now).Shift(layout)).Fold(SKPoint.Empty, static (sum, shift) => sum + shift);

    private static bool Reduced(Scene scene) => scene.Layout.Cascade.Theme.Conditions.Contains(Condition.ReduceMotion);

    // --- [TRANSITIONS]

    internal Animations Started(Scene prior, Scene next, Duration now) =>
        this with {
            Transitions = toHashMap(next.Order.Filter(static layout => layout.View.Key.IsSome).Choose(layout => Transited(prior, layout, now, Reduced(next)).Map(transition => (layout.Path, transition)))),
            Transforms = Morphs(prior, next, Transforms, now, Turned),
            Planes = Morphs(prior, next, Planes, now, Plotted),
            Figures = Morphs(prior, next, Figures, now, Traced),
            Exiting = Exiting.Filter((path, _) => !next.Index.ContainsKey(path)).AddOrUpdateRange(prior.Order
                .Filter(layout => layout.View.Key.IsSome && layout.Style.Motion.Animates && !next.Index.ContainsKey(layout.Path) && next.Index.ContainsKey(layout.Path.Init))
                .Choose(layout => prior.Places.Find(layout.Path).Map(place => (layout.Path, new ExitTransition(layout, place,
                    new Morph<Fraction>(Transitions.Find(layout.Path).Map(transition => transition.At(now).Alpha).IfNone(layout.View.Opacity), Fraction.MinValue, now, layout.Style.Motion)))))),
            Departing = Departed(prior, next, now),
        };

    internal Animations Restyled(Scene prior, Scene next, Duration now) =>
        ReferenceEquals(prior, next) ? this : (Reduced(next) && !Reduced(prior) ? Snapped : this) with { Departing = Departed(prior, next, now) };

    internal Animations Pruned(Scene scene) =>
        this with {
            Transitions = Kept(Transitions, scene, Framed),
            Transforms = Kept(Transforms, scene, Turned),
            Planes = Kept(Planes, scene, Plotted),
            Figures = Kept(Figures, scene, Traced),
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
        Tween Aligned(Tween held) => settled ? held with { Frame = target.Frame } : held;
        Option<Morph<Tween>> Moved(Morph<Tween> open, TimingCurve curve) =>
            Some((open with { From = Aligned(open.From), To = Aligned(open.To) }).Retargeted(target, now, curve, Tween.Distance)).Filter(static moved => moved.From != moved.To);
        Morph<Tween> Rested(Tween from) => new(from, from, now, new TimingCurve.Instant());
        return from curve in Some(layout.Style.Motion).Filter(static motion => motion.Animates)
               from transition in Transitions.Find(layout.Path).Match(
                   Some: open => Moved(open, curve),
                   None: () => prior.Index.Find(layout.Path).Match(
                       Some: was => Tween.Of(was) != target ? Moved(Rested(Tween.Of(was)), curve) : None,
                       None: () => layout.Path.Count > 1 && prior.Index.ContainsKey(layout.Path.Init)
                           ? Moved(Rested(target with { Alpha = Exiting.Find(layout.Path).Map(left => left.Opacity.At(now)).IfNone(Fraction.MinValue) }), curve)
                           : None))
               select transition;
    }

    private HashMap<Seq<Key>, ExitTransition> Departed(Scene prior, Scene next, Duration now) {
        bool Restyled(Seq<Key> path) =>
            prior.Index.Find(path).Exists(was => next.Index.Find(path).Exists(kept => kept.View.Key.IsSome && kept.Style.Motion.Animates && was.Style.Conditions != kept.Style.Conditions));
        return Departing.Filter((path, _) => next.Index.ContainsKey(path)).AddOrUpdateRange(next.Order
            .Filter(layout => Restyled(layout.Path) && !layout.Path.Inits.Init.Exists(Restyled))
            .Choose(layout =>
                from was in prior.Index.Find(layout.Path)
                from place in prior.Places.Find(layout.Path)
                select (layout.Path, new ExitTransition(was, place, new Morph<Fraction>(Departing.Find(layout.Path).Map(held => held.Opacity.At(now)).IfNone(Fraction.MaxValue), Fraction.MinValue, now, layout.Style.Motion)))));
    }

    private static HashMap<Seq<Key>, Morph<T>> Morphs<T>(Scene prior, Scene next, HashMap<Seq<Key>, Morph<T>> open, Duration now, Func<LayoutNode, Option<T>> value) where T : IInterpolated<T> =>
        Reduced(next) ? [] : toHashMap(next.Order.Filter(static layout => layout.Style.Motion.Animates).Choose(layout =>
            from to in value(layout)
            from was in prior.Index.Find(layout.Path).Bind(value)
            let held = open.Find(layout.Path)
            from morph in held.Filter(running => running.To.Equals(to))
                | Some(held.Map(running => running.At(now)).IfNone(was)).Filter(start => !start.Equals(to) && start.Toward(to, 0f).IsSome).Map(start => new Morph<T>(start, to, now, layout.Style.Motion))
            select (layout.Path, morph)));

    private static HashMap<Seq<Key>, Morph<T>> Kept<T>(HashMap<Seq<Key>, Morph<T>> open, Scene scene, Func<LayoutNode, Option<T>> value) where T : IInterpolated<T> =>
        open.Filter((path, morph) => scene.Index.Find(path).Bind(value) == Some(morph.To));

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

    private static HashMap<InteractionState, Morph<Fraction>> Faded(Scene scene, HashMap<InteractionState, Morph<Fraction>> fades, InteractionState key, HashSet<InteractionState> active, Duration now) {
        TimingCurve curve = Bounds(scene, key).Map(static held => held.Motion).IfNone(new TimingCurve.Instant());
        Fraction target = active.Contains(key) ? Fraction.MaxValue : Fraction.MinValue;
        Morph<Fraction> fade = fades.Find(key).Map(open => open.Retargeted(target, now, curve, Signed)).IfNone(new Morph<Fraction>(target.Complement, target, now, curve));
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

    // --- [TICK]

    internal (Animations Animations, Seq<SKRect> Damage) Tick(Scene scene, Duration prior, Duration now) =>
        (this with {
            Fades = Fades.Filter(fade => !fade.Settled(now)),
            Transitions = Transitions.Filter(morph => !morph.Settled(now)),
            Transforms = Transforms.Filter(morph => !morph.Settled(now)),
            Planes = Planes.Filter(morph => !morph.Settled(now)),
            Figures = Figures.Filter(morph => !morph.Settled(now)),
            Exiting = Exiting.Filter(left => !left.Opacity.Settled(now)),
            Departing = Departing.Filter(left => !left.Opacity.Settled(now)),
        }, Reach(scene, prior) + Reach(scene, now));
}
