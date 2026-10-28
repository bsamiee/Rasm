using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Mount {
    // --- [STATE]
    private Mount(Host host, Scene scene) => (Host, Scene, Routing, Tree) = (host, scene, Routing.Idle, Access.Project(scene));

    public Host Host { get; internal init; }
    public Output Output { get; private init; } = new([], [], None);

    public Seq<Element> Tree {
        get;
        internal init {
            field = value;
            (Elements, Stops) = (Access.Indexed(value), Routing.Stops(value));
        }
    }

    internal Scene Scene { get; init; }
    internal Seq<Seq<Key>> Hover { get; init; }
    internal Option<Nearest> Nearest { get; init; }
    internal HashMap<Seq<Key>, Option<SKPoint>> Opened { get; init; }
    internal HashMap<Seq<Key>, Element> Elements { get; private init; }
    internal Seq<Seq<Element>> Stops { get; private init; }
    internal Routing Routing { get; init; }
    internal Animations Animations { get; init; } = Animations.Idle;
    internal HashMap<Seq<Key>, Focus> Returns { get; init; }
    internal Duration LastTick { get; init; }

    // --- [DERIVED]
    public Option<Seq<Key>> Captured => Routing.Capture.Map(static capture => capture.Owner);

    public Option<Seq<Key>> Focused => Routing.Focus.Map(static focus => focus.Element);

    public CursorShape Cursor =>
        Routing.Capture.IsSome
            ? Routing.ActiveTrack.Map(static track => track.Cursor).IfNone(CursorShape.Default)
            : (from path in Hover.Head from pointer in Routing.Pointer select Prospective(path, pointer)).IfNone(CursorShape.Default);

    public Seq<Hosted> Hosted => Animations.Hosted(Scene, LastTick);

    public bool Covered => Routing.Pointer.Exists(Scene.Covers);

    internal bool Animating(Seq<Seq<Key>> animated) => Host.Visibility.Drawn && (Animations.Animating || Scene.Timed.Map(static layout => layout.Path).Concat(animated).Exists(Clocked));

    internal HashMap<InteractionState, float> StateLayers(Duration now) => Animations.StateLayers(Active(), now);

    internal Seq<PlacedLayer> Layers => Scene.Layers.Choose(PlacedLayer.Of);

    internal Seq<PlacedLayer> Dismissible => Layers.Filter(static held => held.Dismisses);

    // --- [TRANSITIONS]
    public static Fin<Mount> Create(Host host, StyleSheet sheet, Node view, Shaping shaping) =>
        Scene.Of(Styles.Root(host.Theme, sheet, host.Root), view, host, shaping).Map(static scene => scene.Revealed(None).Scene).Map(scene => new Mount(host, scene) switch {
            var mount => mount with { Output = new Output([SKRect.Create(host.Size)], [], Sequenced(mount.WithEffects(mount.Watched(None)).Effects)) },
        });

    public Fin<Mount> Show(Node view, Duration now) =>
        Scene.Reconcile(view).Map(moved => Completed(new Routed(this with { Scene = moved.Scene, Animations = Animations.Started(Scene, moved.Scene, now) }, moved.Damage, None), now));

    public Mount Rehost(Host host, StyleSheet sheet, Duration now) =>
        Completed(Restyled(host, sheet, now).Then(mount => Host.Visibility.HasKeyFocus && !host.Visibility.HasKeyFocus ? mount.Cancel() : mount.Unhandled), now);

    public Mount Receive(Input input, Duration now) => Completed(this.Dispatched(input), now);

    internal Mount Tick(Duration now, Seq<Seq<Key>> animated) {
        (Animations Animations, Seq<SKRect> Damage, Seq<IO<Unit>> Elapsed, HashSet<Seq<Key>> Opened) ticked = Animations.Tick(Scene, LastTick, now);
        Routed moved = new(this with { Animations = ticked.Animations, LastTick = now },
            ticked.Damage.Concat(Scene.Timed.Map(static layout => layout.Path).Concat(animated).Filter(Clocked).Choose(Scene.Bounds)),
            Some(ticked.Elapsed).Filter(static effects => !effects.IsEmpty));
        return ticked.Opened.IsEmpty
            ? moved.Mount with { Animations = ticked.Animations.WithTimers(Scene, Routing.Pointer, Pending, now), Output = new Output(Scene.Sampled(moved.Damage), [], Sequenced(moved.Effects)) }
            : Completed(moved.Then(next => next.Reopened(ticked.Opened.Fold(next.Opened, static (held, path) => held.TryAdd(path, None)))), now);
    }

    private Routed Restyled(Host host, StyleSheet sheet, Duration now) {
        SceneChange styled = Host with { Size = host.Size, Visibility = host.Visibility } == host && Scene.Layout.Cascade.Sheet.Equals(sheet) ? Scene.Unchanged : Scene.Restyle(host, sheet);
        SceneChange sized = styled.Scene.Layout.Frame.Size == host.Size ? styled.Scene.Unchanged : styled.Scene.Resize(host);
        return new Routed(this with { Host = host, Scene = sized.Scene, Animations = Animations.Restyled(Scene, styled.Scene, now) },
            styled.Damage.Concat(sized.Damage).Concat(Host.Visibility == host.Visibility ? [] : Focused.Bind(sized.Scene.Bounds).ToSeq()), None);
    }

    // --- [COMPLETION]
    private Mount Completed(Routed routed, Duration now) {
        Routed done = routed.Then(static next => next.Opening()).Then(next => next.Revealing(this)).Then(next => next.Pruning(this)).Then(next => next.Layering(this)).Then(static next => next.Hovering())
            .Then(next => next.Nearing(this)).Then(next => next.Fragmenting(this)).Then(next => next.Watching(this)).Then(next => next.Tweening(this, now));
        Option<Vector2> detent = done.Mount.Routing.ActiveTrack.Bind(static track => track.Detent);
        return done.Mount with {
            Output = new Output(
                done.Mount.Scene.Sampled(done.Damage.Concat(done.Mount.Routing.Focus != Routing.Focus ? Seq(Focused, done.Mount.Focused).Somes().Choose(done.Mount.Scene.Bounds) : [])),
                (done.Mount.Projected(this) ? Access.Changes(Elements, done.Mount.Elements, done.Mount.Host.Culture) : [])
                    .Concat(done.Mount.Focused != Focused ? done.Mount.Focused.Map(static path => (AccessibilityNotice)new AccessibilityNotice.FocusChanged(path)).ToSeq() : [])
                    .Concat(detent.IsSome && detent != Routing.ActiveTrack.Bind(static track => track.Detent) ? Seq<AccessibilityNotice>(new AccessibilityNotice.Detent()) : [])
                    .Concat(done.Notices),
                Sequenced(done.Effects)) { Lifted = done.Lifted },
        };
    }

    private bool Projected(Mount prior) => !ReferenceEquals(prior.Scene, Scene) || !prior.Host.Culture.Equals(Host.Culture);

    private Routed Opening() =>
        Opened.Filter((path, _) => Scene.Triggered.Exists(held => held.Layout.Path == path && (held.Delay.IsNone || Over(held) || (!held.Acts.IsEmpty && Focused.Exists(focus => held.Holds(focus) || held.Anchor.Exists(anchor => focus.Take(anchor.Count) == anchor)))))) switch {
            var opened when opened == Opened => Unhandled,
            var opened => Reopened(opened),
        };

    private Routed Reopened(HashMap<Seq<Key>, Option<SKPoint>> opened) => (this with { Opened = opened }).Applied(Scene.Opened(opened));

    private Routed Revealing(Mount prior) {
        if (!Projected(prior)) return Unhandled;
        SceneChange revealed = Scene.Revealed(Some(prior.Scene));
        return new Routed(this with { Scene = revealed.Scene, Tree = Access.Project(revealed.Scene) }, revealed.Damage, None);
    }

    private Routed Pruning(Mount prior) =>
        (this with {
            Routing = Routing with {
                Capture = Routing.Capture.Filter(held => held.Live(Scene)),
                Latch = Routing.Latch.Filter(held => held.Live(Scene)),
                Focus = Routing.Focus.Map(focus => Followed(prior, focus)).Filter(followed => Elements.ContainsKey(followed.Element) && (Scene.Owner(followed.Element).IsSome || Scene.Scroller(followed.Element).IsSome || !Scene.Anchoring(followed.Element).IsEmpty)),
            },
        }).WithEffects(Routing.Capture.Filter(held => !held.Live(Scene)).ToSeq().Bind(static held => held.Cancelled) + Routing.Latch.Filter(held => !held.Live(Scene)).ToSeq().Bind(static held => held.Cancelled));

    private Focus Followed(Mount prior, Focus focus) =>
        (from owner in Scene.Owner(focus.Element)
         from lead in Scene.Lead(owner.Layout)
         where lead != focus.Element && Elements.ContainsKey(lead) && (prior.Scene.Index.Find(owner.Path).Bind(Scene.Lead) != Some(lead) || !prior.Elements.ContainsKey(lead))
         select focus with { Element = lead }).IfNone(focus);

    private Routed Layering(Mount prior) {
        Seq<PlacedLayer> layers = Layers;
        HashSet<Seq<Key>> was = toHashSet(prior.Layers.Map(static held => held.Layout.Path));
        HashSet<Seq<Key>> now = toHashSet(layers.Map(static held => held.Layout.Path));
        Seq<PlacedLayer> opened = layers.Filter(held => !was.Contains(held.Layout.Path) && (held.Delay.IsNone || !Over(held)));
        Seq<Seq<Key>> closed = toSeq(was).Filter(path => !now.Contains(path));
        Option<Focus> entered = opened.Last.Bind(held => Focusing(Routing.Traversed(Stops.Filter(stop => stop.Exists(element => held.Holds(element.Id))), None, 1, cycles: false), traversed: false));
        Option<Focus> returned = Focused.IsNone ? closed.Choose(Returns.Find).Filter(focus => Elements.ContainsKey(focus.Element)).Head : None;
        Seq<PlacedLayer> left = layers.Filter(held => held.Dismisses && was.Contains(held.Layout.Path) && !held.Layer.Modal && prior.Focused.Exists(held.Holds) && Focused.Exists(focus => !held.Holds(focus)));
        return (this with { Returns = Returns.RemoveRange(closed).AddOrUpdateRange(opened.Choose(held => Routing.Focus.Map(focus => (held.Layout.Path, focus)))) })
            .Refocused(entered | returned | Routing.Focus).Then(next => next.Closed(left));
    }

    private Routed Hovering() =>
        (this with {
            Hover = Routing.Pointer.Map(Scene.Hit).Map(hits => Routing.Pressed.Match(
                Some: pressing => hits.Filter(hit => hit == pressing || (Routing.ActiveTrack.Exists(static track => track.Lifted.IsSome) && Scene.AncestorsAndSelf(hit).Exists(prefix => Scene.Index.Find(prefix).Exists(static layout => layout.Node.Drop.IsSome)))),
                None: () => hits.Filter(hit => Thumb(hit).IsSome || Scene.AncestorsAndSelf(hit).Exists(prefix =>
                    Scene.PartAt(prefix).IsSome || !Scene.Anchoring(prefix).IsEmpty || Scene.Index.Find(prefix).Exists(static layout => layout.Node is Node.Plot { Hovered.IsSome: true }))))).IfNone([]),
        }).Unhandled;

    private Routed Nearing(Mount prior) =>
        (this with {
            Nearest = prior.Routing.Pointer == Routing.Pointer
                ? Nearest.Filter(near => Scene.Index.Find(near.Plot).Exists(static layout => layout.Node is Node.Plot))
                : from at in Routing.Pointer
                  from layout in Scene.Order.Rev().Find(layout => layout.Node is Node.Plot plot && (plot.Pointed.IsSome || plot.Crosshair.IsSome) && !Inert(layout.Path) && Scene.Visible(layout.Path).Exists(shown => shown.Contains(at)))
                  from near in Scene.Nearest(layout.Path, at)
                  select near,
        }).Unhandled;

    private Routed Fragmenting(Mount prior) {
        if (ReferenceEquals(prior.Scene, Scene) && prior.Nearest == Nearest && prior.Routing.Capture.IsNone && Routing.Capture.IsNone) return Unhandled;
        HashMap<Seq<Key>, Node> wanted = Overlays;
        return toSeq(wanted.Keys).Concat(Scene.Fragments.Keys).Distinct().Fold(Unhandled, (routed, path) =>
            wanted.Find(path) == routed.Mount.Scene.Fragments.Find(path).Map(static layout => layout.Node) ? routed : routed.Then(next => next.Applied(next.Scene.Fragment(path, wanted.Find(path)))));
    }

    private Routed Watching(Mount prior) =>
        ReferenceEquals(prior.Scene, Scene) && prior.Hover == Hover && prior.Routing.Pointer == Routing.Pointer && prior.Nearest == Nearest ? Unhandled : WithEffects(Watched(Some(prior)));

    private Routed Tweening(Mount prior, Duration now) {
        (Animations Animations, Seq<SKRect> Damage) faded = Animations.Pruned(Scene).Fading(prior.Scene, Scene, prior.Active(), Active(), now);
        return new Routed(this with { Animations = faded.Animations.WithTimers(Scene, Routing.Pointer, Pending, now), LastTick = now }, faded.Damage, None);
    }

    private Seq<IO<Unit>> Watched(Option<Mount> prior) =>
        Scene.Order.Bind(layout => Watched(
                from held in prior
                from was in held.Scene.Index.Find(layout.Path)
                select new HoverState(held.Scene, was, held.Hover, held.Routing.Pointer),
                new HoverState(Scene, layout, Hover, Routing.Pointer)))
        + Scene.Order.Choose(layout =>
            from plot in Optional(layout.Node as Node.Plot)
            from pointed in plot.Pointed
            let near = Nearest.Filter(held => held.Plot == layout.Path)
            where prior.Bind(static held => held.Nearest).Filter(held => held.Plot == layout.Path) != near
            select pointed(near));

    private static Seq<IO<Unit>> Watched(Option<HoverState> before, HoverState after) =>
        after.Layout.Node.Part.Map(part => part.Watched(before.Filter(static held => held.Layout.Node.Part.IsSome), after)).IfNone([])
        + (from plot in Optional(after.Layout.Node as Node.Plot)
           from hovered in plot.Hovered
           let now = after.Plotted
           where before.Bind(static held => held.Plotted) != now
           select hovered(now)).ToSeq();

    private static Option<IO<Unit>> Sequenced(Option<Seq<IO<Unit>>> answer) => answer.Map(static effects => effects.TraverseM(static effect => effect).As().Map(static _ => unit));

    // --- [ROUTE]
    internal Routed Unhandled => new(this, [], None);

    internal Routed WithEffects(Seq<IO<Unit>> effects) => new(this, [], effects.IsEmpty ? None : Some(effects));

    internal Routed Handled(Seq<IO<Unit>> effects) => new(this, [], Some(effects));

    internal Routed Applied(SceneChange change) => new(this with { Scene = change.Scene }, change.Damage, None);

    // --- [LAYERS]
    internal Routed Toggled(PlacedLayer held, Option<SKPoint> at) =>
        at.Filter(_ => held.Layer.Trigger.Exists(static trigger => trigger.AtPointer)) switch {
            var pointed when Opened.ContainsKey(held.Layout.Path) && pointed.IsNone => Closed([held]).Handled(),
            var pointed => Refocused(held.Anchor.Bind(anchor => Focusing(Element(anchor, anchor), traversed: false)) | Routing.Focus)
                .Then(next => next.Closed(next.Dismissible.Filter(open => !open.Holds(held.Layout.Path))))
                .Then(next => next.Reopened(next.Opened.AddOrUpdate(held.Layout.Path, pointed)))
                .Handled(),
        };

    internal Routed Closed(Seq<PlacedLayer> closing) =>
        Reopened(Opened.RemoveRange(closing.Map(static held => held.Layout.Path))).Then(next => next.WithEffects(closing.Choose(static held => held.Layer.Dismissed)));

    internal Routed Picked(Seq<Key> owner) =>
        Closed(Layers.Filter(held => held.Holds(owner) && Scene.AncestorsAndSelf(owner).TakeWhile(prefix => prefix.Count >= held.Layout.Path.Count)
            .Exists(prefix => Scene.Index.Find(prefix).Exists(static layout => layout.Node.Accessible.Role.Exists(static role => role.Picks)))));

    // --- [FOCUS]
    internal Option<Focus> Focusing(Option<Seq<Key>> element, bool traversed) =>
        element.Map(target => Routing.Focus.Filter(focus => focus.Element == target).IfNone(() => new Focus(target, traversed)));

    internal Routed Refocused(Option<Focus> next) =>
        next.Map(static focus => focus.Element) == Focused ? Unhandled : (this with { Routing = Routing with { Focus = next } }).Unhandled;

    internal Option<Seq<Key>> Element(Seq<Key> owner, Seq<Key> over) => Scene.AncestorsAndSelf(over).TakeWhile(prefix => prefix.Count >= owner.Count).Find(Elements.ContainsKey) | Entry(owner);

    private Option<Seq<Key>> Entry(Seq<Key> stop) =>
        Scene.Index.Find(stop).Bind(layout => (Scene.Lead(layout).ToSeq() + Scene.Preorder(layout).Map(static held => held.Path))
            .Find(entry => Elements.ContainsKey(entry) && Scene.Owner(entry).Exists(owner => owner.Path == stop)));

    // --- [LOOKUP]
    internal Option<Scroller> Thumb(Seq<Key> path) => Scene.Scroller(path).Filter(scroller => scroller.Owner != path);

    internal bool Inert(Seq<Key> path) => Scene.AncestorsAndSelf(path).Exists(Scene.Inert.Contains);

    private HashMap<Seq<Key>, Duration> Pending =>
        toHashMap(Scene.Triggered.Filter(held => held.Delay.IsSome && !Opened.ContainsKey(held.Layout.Path) && Over(held)).Choose(static held => held.Delay.Map(delay => (held.Layout.Path, delay))));

    private bool Over(PlacedLayer held) =>
        Routing.Pointer.Exists(at => held.Anchor.Match(Some: anchor => Scene.Visible(anchor).Exists(shown => shown.Contains(at)), None: static () => true)
            || (Opened.ContainsKey(held.Layout.Path) && Scene.Visible(held.Layout.Path).Exists(shown => shown.Contains(at))));

    private bool Clocked(Seq<Key> path) =>
        !Host.Theme.Conditions.Contains(Condition.ReduceMotion) || Scene.AncestorsAndSelf(path).Exists(above => Scene.Index.Find(above).Exists(static layout => layout.Style.Conditions.Contains(Condition.Busy)));

    private HashMap<Seq<Key>, Node> Overlays =>
        toHashMap((from near in Nearest
                   from layout in Scene.Index.Find(near.Plot)
                   from plot in Optional(layout.Node as Node.Plot)
                   from crosshair in plot.Crosshair
                   select (near.Plot, crosshair(near))).ToSeq())
            .AddOrUpdateRange((from part in Routing.Part
                               from overlay in Previewed(part) | part.Track.Preview.Map(static figure => (Node)new Node.Mark(figure) { Role = Role.Selection })
                               select (part.Owner, overlay)).ToSeq());

    private Option<Node> Previewed(PointerCapture.Part part) =>
        from lifting in part.Track.Lifted
        from layout in Scene.Index.Find(part.Owner)
        from lift in layout.Node.Lift
        from preview in lift.Preview
        from place in Scene.Places.Find(part.Owner)
        from local in place.Local(part.Press.At)
        let frame = Scene.Absolute(layout.Frame, place.Origin)
        from at in (Layouts.Plotted(layout).Map(own => own.Translated(frame.Location)) | place.Plot).Match(Some: plot => plot.At(local), None: () => Some((Vector2)(local - frame.Location)))
        select preview(at);

    private CursorShape Prospective(Seq<Key> path, SKPoint pointer) =>
        Thumb(path).IsSome ? CursorShape.Default
        : Scene.AncestorsAndSelf(path).Choose(Scene.PartAt).Head.Exists(static owner => !owner.Enabled) ? CursorShape.NotAllowed
        : Scene.Owners(path).Choose(owner => owner.Part.Began(new Gesture.Press(pointer, None, 1) { Modifiers = Routing.Modifiers }, Scene.HitTarget(owner, path))
                .Map(static reply => reply.Track.Map(static track => track.Cursor).IfNone(CursorShape.Default)))
            .Head.IfNone(CursorShape.Default);

    private HashSet<InteractionState> Active() =>
        toHashSet((Hover.Head.Filter(path => Scene.Index.Find(path).Exists(static layout => !layout.Style.Conditions.Contains(Condition.Disabled))).ToSeq() + Scene.Highlighted).Map(static path => new InteractionState(path, Interaction.Hover))
            .Concat(Routing.Pressed.Map(static path => new InteractionState(path, Interaction.Pressed)).ToSeq())
            .Concat(Routing.Focus.Filter(focus => Host.Visibility.HasKeyFocus && focus.Traversed).Map(static focus => new InteractionState(focus.Element, Interaction.Focused)).ToSeq()));
}
