using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record PlacedLayer(LayoutNode Layout, Node.Layer Layer) {
    internal bool Holds(Seq<Key> path) => path.Take(Layout.Path.Count) == Layout.Path;
}

public sealed record Mount {
    // --- [STATE]
    private Mount(Host host, Scene scene) => (Host, Scene, Routing, Tree) = (host, scene, Routing.Idle, Access.Project(scene, host.Culture));

    public Host Host { get; internal init; }
    public Scene Scene { get; internal init; }
    public Seq<Seq<Key>> Hover { get; internal init; }
    public Option<Nearest> Nearest { get; internal init; }
    public HashSet<Seq<Key>> Opened { get; internal init; }

    public Seq<Element> Tree {
        get;
        internal init {
            field = value;
            Elements = Access.Indexed(value);
        }
    }

    internal HashMap<Seq<Key>, Element> Elements { get; private init; }
    internal Routing Routing { get; init; }
    internal Animations Animations { get; init; } = Animations.Idle;
    internal HashMap<Seq<Key>, Focus> Returns { get; init; }
    internal Duration LastTick { get; init; }

    // --- [DERIVED]
    public Option<Seq<Key>> Captured => Routing.Capture.Map(static capture => capture.Switch(part: static part => part.Owner, handle: static handle => handle.Adjuster));

    public Option<Seq<Key>> Focused => Routing.Focus.Map(static focus => focus.Element);

    public CursorShape Cursor =>
        Routing.Capture.Match(
            Some: static capture => capture.Switch(part: static part => part.Track.Cursor, handle: static _ => CursorShape.Default),
            None: () => (from path in Hover.Head from pointer in Routing.Pointer select Prospective(path, pointer)).IfNone(CursorShape.Default));

    public Seq<Hosted> Hosted => Animations.Hosted(Scene, LastTick);

    internal bool Animating(Seq<Seq<Key>> animated) => Host.Visibility.Drawn && (Animations.Animating || Scene.Timed.Map(static layout => layout.Path).Concat(animated).Exists(Clocked));

    internal HashMap<InteractionState, float> StateLayers(Duration now) => Animations.StateLayers(Active(), now);

    internal Seq<PlacedLayer> Layers => Scene.Layers.Choose(Layered);

    // --- [TRANSITIONS]
    public static Fin<Mounted> Create(Host host, DefaultStyles defaults, Node view, HashMap<Seq<Key>, float> adjusted, Shaping shaping) =>
        Scene.Of(Styles.Root(host.Theme, defaults, host.Placement.Root), view, host, adjusted, [], shaping).Map(static scene => scene.Revealed(None).Scene).Map(scene => {
            Mount mount = new(host, scene);
            return new Mounted(mount, new Output(Seq(SKRect.Create(host.Size)), [], Sequenced(mount.WithEffects(mount.Watched(None)).Effects)));
        });

    public Fin<Mounted> Show(Node view, Duration now) =>
        Scene.Reconcile(view).Map(moved => Completed(new Routed(this with { Scene = moved.Scene, Animations = Animations.Started(Scene, moved.Scene, now) }, moved.Damage, None), now));

    public Mounted Rehost(Host host, Duration now) =>
        Completed(Restyled(host, now).Then(mount => Host.Visibility.HasKeyFocus && !host.Visibility.HasKeyFocus ? mount.Cancel() : mount.Unhandled), now);

    public Mounted Receive(Input input, Duration now) => Completed(this.Dispatched(input), now);

    internal Mounted Tick(Duration now, Seq<Seq<Key>> animated) {
        Animations.Ticked ticked = Animations.Tick(Scene, LastTick, now);
        Routed moved = new(this with { Animations = ticked.Animations, LastTick = now },
            ticked.Damage.Concat(Scene.Timed.Map(static layout => layout.Path).Concat(animated).Filter(Clocked).Choose(Scene.Bounds)),
            Some(ticked.Elapsed).Filter(static effects => !effects.IsEmpty));
        return ticked.Opened.IsEmpty
            ? new(moved.Mount with { Animations = ticked.Animations.WithTimers(Scene, Routing.Pointer, Pending, now) }, new Output(Scene.Sampled(moved.Damage), [], Sequenced(moved.Effects)))
            : Completed(moved.Then(next => next.Reopened(next.Opened.Union(ticked.Opened))), now);
    }

    private Routed Restyled(Host host, Duration now) {
        SceneChange styled = Host.Theme == host.Theme && Host.Placement.Root == host.Placement.Root && Host.Culture.Equals(host.Culture) && Host.Blending == host.Blending
            ? Scene.Unchanged
            : Scene.Restyle(host);
        SceneChange sized = styled.Scene.Layout.Frame.Size == host.Size ? styled.Scene.Unchanged : styled.Scene.Resize(host);
        return new Routed(this with { Host = host, Scene = sized.Scene, Animations = Animations.Restyled(Scene, styled.Scene, now) },
            styled.Damage.Concat(sized.Damage).Concat(Host.Visibility == host.Visibility ? [] : Focused.Bind(sized.Scene.Bounds).ToSeq()), None);
    }

    // --- [COMPLETION]
    private Mounted Completed(Routed routed, Duration now) {
        Routed done = routed.Then(static next => next.Opening()).Then(next => next.Revealing(this)).Then(next => next.Pruning(this)).Then(next => next.Layering(this)).Then(static next => next.Hovering())
            .Then(next => next.Nearing(this)).Then(next => next.Fragmenting(this)).Then(next => next.Watching(this)).Then(next => next.Tweening(this, now));
        Option<Vector2> detent = done.Mount.Routing.ActiveTrack.Bind(static track => track.Detent);
        return new(done.Mount, new Output(
            done.Mount.Scene.Sampled(done.Damage.Concat(done.Mount.Routing.Focus != Routing.Focus ? Seq(Focused, done.Mount.Focused).Somes().Choose(done.Mount.Scene.Bounds) : [])),
            (done.Mount.Projected(this) ? Access.Changes(Elements, done.Mount.Elements, done.Mount.Host.Culture) : [])
                .Concat(done.Mount.Focused != Focused ? done.Mount.Focused.Map(static path => (AccessibilityNotice)new AccessibilityNotice.FocusChanged(path)).ToSeq() : [])
                .Concat(detent.IsSome && detent != Routing.ActiveTrack.Bind(static track => track.Detent) ? Seq<AccessibilityNotice>(new AccessibilityNotice.Detent()) : [])
                .Concat(done.Notices),
            Sequenced(done.Effects)) { Lifted = done.Lifted });
    }

    private bool Projected(Mount prior) => !ReferenceEquals(prior.Scene, Scene) || !prior.Host.Culture.Equals(Host.Culture);

    private Routed Opening() =>
        toHashSet(Delayed.Filter(held => Opened.Contains(held.Layout.Path) && Over(held)).Map(static held => held.Layout.Path)) switch {
            var opened when opened == Opened => Unhandled,
            var opened => Reopened(opened),
        };

    private Routed Reopened(HashSet<Seq<Key>> opened) => (this with { Opened = opened }).Applied(Scene.Opened(opened));

    private Routed Revealing(Mount prior) {
        if (!Projected(prior)) return Unhandled;
        SceneChange revealed = Scene.Revealed(Some(prior.Scene));
        return new Routed(this with { Scene = revealed.Scene, Tree = Access.Project(revealed.Scene, Host.Culture) }, revealed.Damage, None);
    }

    private Routed Pruning(Mount prior) {
        Option<(Option<PointerCapture> Kept, Seq<IO<Unit>> Cancelled)> capture = Routing.Capture.Map(held => held.Switch<Mount, (Option<PointerCapture> Kept, Seq<IO<Unit>> Cancelled)>(
            this,
            part: static (mount, part) => mount.Scene.EnabledPartAt(part.Owner).IsSome ? (Some<PointerCapture>(part), []) : (None, part.Track.Cancelled()),
            handle: static (mount, handle) => (mount.Scene.Adjustment(handle.Adjuster).Map(_ => (PointerCapture)handle), [])));
        Option<(Option<Latch> Kept, Seq<IO<Unit>> Cancelled)> latch = Routing.Latch.Map(held => held.Switch<Mount, (Option<Latch> Kept, Seq<IO<Unit>> Cancelled)>(
            this,
            part: static (mount, part) => mount.Scene.EnabledPartAt(part.Owner).IsSome ? (Some<Latch>(part), []) : (None, part.Active.Map(static live => live.Track.Cancelled()).IfNone([])),
            scroll: static (mount, scroll) => (mount.Scene.Adjustment(scroll.Adjuster).Map(_ => (Latch)scroll), [])));
        return (this with {
            Routing = Routing with {
                Capture = capture.Bind(static kept => kept.Kept),
                Latch = latch.Bind(static kept => kept.Kept),
                Focus = Routing.Focus.Map(focus => Followed(prior, focus)).Filter(followed => Elements.ContainsKey(followed.Element) && (Scene.Owner(followed.Element).IsSome || Scene.Adjustment(followed.Element).IsSome)),
            },
        }).WithEffects(capture.Map(static kept => kept.Cancelled).IfNone([]) + latch.Map(static kept => kept.Cancelled).IfNone([]));
    }

    private Focus Followed(Mount prior, Focus focus) =>
        (from owner in Scene.Owner(focus.Element)
         from lead in Scene.Lead(owner.Layout)
         where lead != focus.Element && Elements.ContainsKey(lead) && (prior.Scene.Index.Find(owner.Path).Bind(Scene.Lead) != Some(lead) || !prior.Elements.ContainsKey(lead))
         select focus with { Element = lead }).IfNone(focus);

    private Routed Layering(Mount prior) {
        Seq<PlacedLayer> layers = Layers;
        HashSet<Seq<Key>> was = toHashSet(prior.Layers.Map(static held => held.Layout.Path));
        HashSet<Seq<Key>> now = toHashSet(layers.Map(static held => held.Layout.Path));
        Seq<PlacedLayer> opened = layers.Filter(held => !was.Contains(held.Layout.Path) && held.Layer.Hover.IsNone);
        Seq<Seq<Key>> closed = toSeq(was).Filter(path => !now.Contains(path));
        Option<Focus> entered = opened.Last.Bind(held => Focusing(Routing.Traversed(Routing.Stops(Tree).Filter(stop => stop.Exists(element => held.Holds(element.Id))), None, 1, cycles: false), traversed: false));
        Option<Focus> returned = Focused.IsNone ? closed.Choose(Returns.Find).Filter(focus => Elements.ContainsKey(focus.Element)).Head : None;
        Seq<IO<Unit>> left = layers.Filter(held => was.Contains(held.Layout.Path) && !held.Layer.Modal && prior.Focused.Exists(held.Holds) && Focused.Exists(focus => !held.Holds(focus))).Choose(static held => held.Layer.Dismissed);
        return (this with { Returns = Returns.RemoveRange(closed).AddOrUpdateRange(opened.Choose(held => prior.Routing.Focus.Map(focus => (held.Layout.Path, focus)))) })
            .Refocused(entered | returned | Routing.Focus).Then(next => next.WithEffects(left));
    }

    private Routed Hovering() =>
        (this with {
            Hover = Routing.Pointer.Map(Hits).Map(hits => Routing.Pressed.Match(
                Some: pressing => hits.Filter(hit => hit == pressing || (Routing.ActiveTrack.Exists(static track => track.Lifted.IsSome) && Scene.AncestorsAndSelf(hit).Exists(prefix => Scene.Index.Find(prefix).Exists(static layout => layout.Node.Drop.IsSome)))),
                None: () => {
                    HashSet<Seq<Key>> anchors = toHashSet(Delayed.Choose(static held => held.Layer.Anchor));
                    return hits.Filter(hit => Handle(hit).IsSome || Scene.AncestorsAndSelf(hit).Exists(prefix =>
                        Scene.PartAt(prefix).IsSome || anchors.Contains(prefix) || Scene.Index.Find(prefix).Exists(static layout => layout.Node.Hovering.IsSome)));
                })).IfNone([]),
        }).Unhandled;

    private Routed Nearing(Mount prior) =>
        (this with {
            Nearest = prior.Routing.Pointer == Routing.Pointer
                ? Nearest.Filter(near => Scene.Index.Find(near.Plot).Exists(static layout => layout.Node.PlotNode.IsSome))
                : from at in Routing.Pointer
                  from layout in Scene.Order.Rev().Find(layout => layout.Node.PlotNode.Exists(static plot => plot.Pointed.IsSome || plot.Crosshair.IsSome) && !Inert(layout.Path) && Scene.Visible(layout.Path).Exists(shown => shown.Contains(at)))
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
        Animations.StateLayerChange faded = Animations.Pruned(Scene).Fading(prior.Scene, Scene, prior.Active(), Active(), now);
        return new Routed(this with { Animations = faded.Animations.WithTimers(Scene, Routing.Pointer, Pending, now), LastTick = now }, faded.Damage, None);
    }

    private Seq<IO<Unit>> Watched(Option<Mount> prior) =>
        Scene.Order.Bind(layout => Watched(
                from held in prior
                from was in held.Scene.Index.Find(layout.Path)
                select new HoverState(held.Scene, was, held.Hover, held.Routing.Pointer),
                new HoverState(Scene, layout, Hover, Routing.Pointer)))
        + Scene.Order.Choose(layout =>
            from plot in layout.Node.PlotNode
            from pointed in plot.Pointed
            let near = Nearest.Filter(held => held.Plot == layout.Path)
            where prior.Bind(static held => held.Nearest).Filter(held => held.Plot == layout.Path) != near
            select pointed(near));

    private static Seq<IO<Unit>> Watched(Option<HoverState> before, HoverState after) =>
        after.Layout.Node.Part.Map(part => part.Watched(before.Filter(static held => held.Layout.Node.Part.IsSome), after)).IfNone([])
        + (from hovered in after.Layout.Node.Hovering
           let now = after.Plotted
           where before.Bind(static held => held.Plotted) != now
           select hovered(now)).ToSeq();

    private static Option<IO<Unit>> Sequenced(Option<Seq<IO<Unit>>> answer) => answer.Map(static effects => effects.TraverseM(static effect => effect).As().Map(static _ => unit));

    // --- [ROUTE]
    internal Routed Unhandled => new(this, [], None);

    internal Routed WithEffects(Seq<IO<Unit>> effects) => new(this, [], effects.IsEmpty ? None : Some(effects));

    internal Routed Handled(Seq<IO<Unit>> effects) => new(this, [], Some(effects));

    internal Routed Applied(SceneChange change) => new(this with { Scene = change.Scene }, change.Damage, None);

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
    internal Seq<Seq<Key>> Hits(SKPoint at) => Scene.Hit(at).Map(static hit => hit.Layout.Path);

    internal Option<Adjustment> Handle(Seq<Key> path) => Scene.Adjustment(path).Filter(adjuster => adjuster.Owner != path);

    internal bool Inert(Seq<Key> path) => Scene.AncestorsAndSelf(path).Exists(Scene.Inert.Contains);

    private static Option<PlacedLayer> Layered(LayoutNode layout) =>
        layout.Node.LayerNode.Map(layer => new PlacedLayer(layout, layer));

    private Seq<PlacedLayer> Delayed => toSeq(Scene.Index.Values).Choose(Layered).Filter(static held => held.Layer.Hover.IsSome);

    private HashMap<Seq<Key>, Duration> Pending => toHashMap(Delayed.Filter(held => !Opened.Contains(held.Layout.Path) && Over(held)).Choose(static held => held.Layer.Hover.Map(delay => (held.Layout.Path, delay))));

    private bool Over(PlacedLayer held) =>
        Routing.Pointer.Exists(at => held.Layer.Anchor.Match(Some: anchor => Scene.Visible(anchor).Exists(shown => shown.Contains(at)), None: static () => true)
            || (Opened.Contains(held.Layout.Path) && Scene.Visible(held.Layout.Path).Exists(shown => shown.Contains(at))));

    private bool Clocked(Seq<Key> path) =>
        !Host.Theme.Conditions.Contains(Condition.ReduceMotion) || Scene.AncestorsAndSelf(path).Exists(above => Scene.Index.Find(above).Exists(static layout => layout.Style.Conditions.Contains(Condition.Busy)));

    private HashMap<Seq<Key>, Node> Overlays =>
        toHashMap((from near in Nearest
                   from layout in Scene.Index.Find(near.Plot)
                   from plot in layout.Node.PlotNode
                   from crosshair in plot.Crosshair
                   select (near.Plot, crosshair(near))).ToSeq())
            .AddOrUpdateRange((from capture in Routing.Capture
                               from part in capture.Switch(part: static part => Some(part), handle: static _ => Option<PointerCapture.Part>.None)
                               from overlay in Previewed(part) | part.Track.Preview.Map(static figure => (Node)new Node.Mark(figure) { Role = Role.Selection })
                               select (part.Owner, overlay)).ToSeq());

    private Option<Node> Previewed(PointerCapture.Part part) =>
        from lifting in part.Track.Lifted
        from layout in Scene.Index.Find(part.Owner)
        from lift in layout.Node.Lift
        from preview in lift.Preview
        from frame in Scene.Frame(part.Owner)
        select preview((Vector2)(part.Press.At - frame.Location));

    private CursorShape Prospective(Seq<Key> path, SKPoint pointer) =>
        Handle(path).IsSome ? CursorShape.Default
        : Scene.AncestorsAndSelf(path).Choose(Scene.PartAt).Head.Exists(static owner => !owner.Enabled) ? CursorShape.NotAllowed
        : Scene.Owners(path).Choose(owner => owner.Part.Began(new Gesture.Press(pointer, None, 1) { Modifiers = Routing.Modifiers }, Scene.HitTarget(owner, path))
                .Switch(ignored: static _ => Option<CursorShape>.None, accepted: static accepted => Some(accepted.Track.Map(static track => track.Cursor).IfNone(CursorShape.Default))))
            .Head.IfNone(CursorShape.Default);

    private HashSet<InteractionState> Active() =>
        toHashSet(Hover.Head.Filter(path => Scene.Index.Find(path).Exists(static layout => !layout.Style.Conditions.Contains(Condition.Disabled))).Map(static path => new InteractionState(path, Interaction.Hover)).ToSeq()
            .Concat(Routing.Pressed.Map(static path => new InteractionState(path, Interaction.Pressed)).ToSeq())
            .Concat(Routing.Focus.Filter(focus => Host.Visibility.HasKeyFocus && focus.Traversed).Map(static focus => new InteractionState(focus.Element, Interaction.Focused)).ToSeq()));
}

public sealed record Mounted(Mount Mount, Output Output);
