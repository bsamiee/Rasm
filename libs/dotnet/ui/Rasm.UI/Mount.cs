using System.Numerics;
using Rasm.UI.Motion;
using Rasm.UI.Automation;
using Rasm.UI.Hosting;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Mount {

    // --- [STATE]

    private Mount(Host host, Scene scene) => (Host, Scene, Routing, Tree) = (host, scene, Routing.Idle, AutomationElement.Projected(scene));

    public Host Host { get; internal init; }
    public Output Output { get; internal init; } = Output.Empty;

    public Seq<AutomationElement> Tree {
        get;
        internal init {
            field = value;
            (Elements, Stops) = (toHashMap(AutomationElement.Flattened(value).Map(static element => (element.Id, element))), Routing.Stops(value));
        }
    }

    internal Scene Scene { get; init; }
    internal Seq<Seq<Key>> Hover { get; init; }
    internal Option<Nearest> Nearest { get; init; }
    internal HashMap<Seq<Key>, Option<SKPoint>> Opened { get; init; }
    internal HashMap<Seq<Key>, AutomationElement> Elements { get; private init; }
    internal Seq<Seq<AutomationElement>> Stops { get; private init; }
    internal Routing Routing { get; init; }
    internal Animations Animations { get; init; } = Animations.Idle;
    internal Timers Timers { get; init; } = Timers.Idle;
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

    public Option<string> Help =>
        Hover.Head.Bind(path => path.Inits.Rev().Choose(Elements.Find) switch {
            var above => AutomationElement.Joined(Seq(
                    above.Choose(static element => element.Accessible.Help).Head
                    | Scene.Index.Find(path).Bind(static layout => layout.Preorder.Choose(static held => held.TextNode.Filter(static text => text.Typeset.Truncated).Map(static text => text.Typeset.Text.Content)).Head),
                    above.Choose(static element => element.Accessible.Shortcut).Head)
                .Somes()),
        });

    internal bool Animating(Seq<Seq<Key>> animated) =>
        Host.Visibility.Drawn && (!Animations.Fades.IsEmpty || !Animations.Morphing.IsEmpty || !Animations.Leaving.IsEmpty || !Clocks(animated).IsEmpty);

    internal HashMap<InteractionState, float> StateLayers(Duration now) => Animations.StateLayers(Active(), now);

    internal Seq<PlacedLayer> Layers => Scene.Layers.Choose(PlacedLayer.Of);

    internal Seq<PlacedLayer> Dismissible => Layers.Filter(static held => held.Dismisses);

    // --- [TRANSITIONS]

    public static Fin<Mount> Create(Host host, StyleSheet sheet, View view, Shaping shaping) =>
        Scene.Of(Cascade.Root(host.Theme, sheet, host.Root), view, host, shaping).Map(static scene => scene.Revealed(None).Scene).Map(scene => new Mount(host, scene) switch {
            var mount => mount with { Output = mount.WithEffects(mount.Watched(None)).Output with { Damage = [SKRect.Create(host.Size)] } },
        });

    public Fin<Mount> Show(View view, Duration now) =>
        Scene.Reconcile(view).Map(moved => Completed((this with { Animations = Animations.Started(Scene, moved.Scene, now) }).Applied(moved), now));

    public Mount Rehost(Host host, StyleSheet sheet, Duration now) =>
        Completed(Restyled(host, sheet, now).Then(mount => Host.Visibility.HasKeyFocus && !host.Visibility.HasKeyFocus ? mount.Cancel() : mount.Unhandled), now);

    public Mount Receive(InputEvent input, Duration now) => Completed(this.Dispatched(input), now);

    internal Mount Tick(Duration now, Seq<Seq<Key>> animated) {
        (Animations Animations, Seq<SKRect> Damage) ticked = Animations.Tick(Scene, LastTick, now);
        HashMap<Seq<Key>, Countdown> expired = Timers.Expiries.Filter(countdown => countdown.Elapsed(now));
        HashMap<Seq<Key>, Countdown> opened = Timers.Hovers.Filter(countdown => countdown.Elapsed(now));
        Timers timers = Timers with { Expiries = Timers.Expiries.AddOrUpdateRange(expired.Map(static _ => Countdown.Expired).AsIterable()), Hovers = Timers.Hovers.RemoveRange(opened.Keys) };
        Mount moved = (this with { Animations = ticked.Animations, Timers = timers, LastTick = now })
            .WithEffects(expired.Keys.ToSeq().Choose(path => Scene.Index.Find(path).Bind(static layout => layout.View.Expires)).Map(static expiry => expiry.Elapsed)) switch {
                var held => held with { Output = held.Output with { Damage = ticked.Damage + Clocks(animated).Choose(Scene.Bounds) } },
            };
        return opened.IsEmpty
            ? moved with { Timers = timers.Rearmed(Scene, Routing.Pointer, Pending, now), Output = moved.Output with { Damage = Scene.Sampled(moved.Output.Damage) } }
            : Completed(moved.Then(next => next.Reopened(opened.Keys.Fold(next.Opened, static (held, path) => held.TryAdd(path, None)))), now);
    }

    private Mount Restyled(Host host, StyleSheet sheet, Duration now) {
        SceneChange styled = Host with { Size = host.Size, Visibility = host.Visibility } == host && Scene.Layout.Cascade.Sheet.Equals(sheet) ? Scene.Unchanged : Scene.Restyle(host, sheet);
        SceneChange sized = styled.Scene.Layout.Frame.Size == host.Size ? styled.Scene.Unchanged : styled.Scene.Resize(host);
        return this with {
            Host = host,
            Scene = sized.Scene,
            Animations = Animations.Restyled(Scene, styled.Scene, now),
            Output = new Output(styled.Damage + sized.Damage + (Host.Visibility == host.Visibility ? [] : Focused.Bind(sized.Scene.Bounds).ToSeq()), [], None),
        };
    }

    // --- [COMPLETION]

    private Mount Completed(Mount routed, Duration now) {
        Mount done = routed.Then(static next => next.Opening()).Then(next => next.Revealing(this)).Then(next => next.Pruning(this)).Then(next => next.Layering(this)).Then(static next => next.Hovering())
            .Then(next => next.Nearing(this)).Then(next => next.Fragmenting(this)).Then(next => next.Watching(this)).Then(next => next.Tweening(this, now));
        Option<Vector2> detent = done.Routing.ActiveTrack.Bind(static track => track.Detent);
        Seq<Change<AutomationElement>> changes = done.Projected(this)
            ? toSeq(toTrackingHashMap(Elements.AsIterable()).Snapshot().RemoveRange(Elements.Keys).AddOrUpdateRange(done.Elements.AsIterable()).Changes.Values)
            : [];
        Seq<EntryMapped<AutomationElement, AutomationElement>> kept = changes.Choose(static change => change is EntryMapped<AutomationElement, AutomationElement> mapped ? Some(mapped) : None);
        return done with {
            Output = done.Output with {
                Damage = done.Scene.Sampled(done.Output.Damage + (done.Routing.Focus != Routing.Focus ? Seq(Focused, done.Focused).Somes().Choose(done.Scene.Bounds) : [])),
                Notices = (from held in kept
                           from change in toSeq(ElementChange.Items)
                           where change.Differs(held.From, held.To)
                           select (AccessibilityNotice)new AccessibilityNotice.Changed(held.To.Id, change))
                    .Concat(changes.Exists(static change => change.HasAdded || change.HasRemoved) || kept.Exists(static held => held.From.Frame != held.To.Frame) ? Seq<AccessibilityNotice>(new AccessibilityNotice.LayoutChanged()) : [])
                    .Concat(changes.Choose(static change => change switch {
                        EntryAdded<AutomationElement> added => Some(added.Value),
                        EntryMapped<AutomationElement, AutomationElement> mapped when mapped.From.Accessible.Label != mapped.To.Accessible.Label || mapped.From.Accessible.Value != mapped.To.Accessible.Value => Some(mapped.To),
                        _ => Option<AutomationElement>.None,
                    })
                        .Choose(element =>
                            from interrupts in element.Accessible.Interrupts
                            from text in element.Accessible.Spoken(done.Host.Culture)
                            select (AccessibilityNotice)new AccessibilityNotice.Announced(text, interrupts)))
                    + (done.Focused != Focused ? done.Focused.Map(static path => (AccessibilityNotice)new AccessibilityNotice.FocusChanged(path)).ToSeq() : [])
                    + (detent.IsSome && detent != Routing.ActiveTrack.Bind(static track => track.Detent) ? Seq<AccessibilityNotice>(new AccessibilityNotice.Detent()) : [])
                    + done.Output.Notices,
            },
        };
    }

    private bool Projected(Mount prior) => !ReferenceEquals(prior.Scene, Scene) || !prior.Host.Culture.Equals(Host.Culture);

    private Mount Opening() =>
        Opened.Filter((path, _) => Scene.Triggered.Exists(held => held.Layout.Path == path && (held.Delay.IsNone || Over(held) || (!held.Acts.IsEmpty && Focused.Exists(focus => held.Holds(focus) || held.Anchor.Exists(anchor => focus.StartsWith(anchor))))))) switch {
            var opened when opened == Opened => Unhandled,
            var opened => Reopened(opened),
        };

    private Mount Reopened(HashMap<Seq<Key>, Option<SKPoint>> opened) => (this with { Opened = opened }).Applied(Scene.Opened(opened));

    private Mount Revealing(Mount prior) =>
        Projected(prior)
            ? Scene.Revealed(Some(prior.Scene)) switch { var revealed => (this with { Tree = AutomationElement.Projected(revealed.Scene) }).Applied(revealed) }
            : Unhandled;

    private Mount Pruning(Mount prior) {
        (Seq<PointerCapture> captured, Seq<PointerCapture> uncaptured) = Routing.Capture.ToSeq().Partition(held =>
            held.Switch(Scene, part: static (scene, part) => scene.EnabledPartAt(part.Owner).IsSome, handle: static (scene, handle) => scene.Scroller(handle.Owner).IsSome));
        (Seq<Latch> latched, Seq<Latch> unlatched) = Routing.Latch.ToSeq().Partition(held =>
            held.Switch(Scene, part: static (scene, part) => scene.EnabledPartAt(part.Owner).IsSome, scroll: static (scene, scroll) => scene.Scroller(scroll.Owner).IsSome));
        return (this with {
            Routing = Routing with {
                Capture = captured.Head,
                Latch = latched.Head,
                Focus = Routing.Focus.Map(focus => Followed(prior, focus)).Filter(followed => Elements.ContainsKey(followed.Element) && (Scene.Owner(followed.Element).IsSome || Scene.Scroller(followed.Element).IsSome || !Scene.Anchoring(followed.Element).IsEmpty)),
            },
        }).WithEffects(uncaptured.Bind(static held => held.Cancelled) + unlatched.Bind(static held => held.Cancelled));
    }

    private Focus Followed(Mount prior, Focus focus) =>
        (from owner in Scene.Owner(focus.Element)
         from lead in owner.Layout.Lead
         where lead != focus.Element && Elements.ContainsKey(lead) && (prior.Scene.Index.Find(owner.Path).Bind(static layout => layout.Lead) != Some(lead) || !prior.Elements.ContainsKey(lead))
         select focus with { Element = lead }).IfNone(focus);

    private Mount Layering(Mount prior) {
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

    private Mount Hovering() =>
        (this with {
            Hover = Routing.Pointer.Map(Scene.Hit).Map(hits => Routing.Pressed.Match(
                Some: pressing => hits.Filter(hit => hit == pressing || (Routing.ActiveTrack.Exists(static track => track.Lifted.IsSome) && hit.Inits.Exists(prefix => Scene.Index.Find(prefix).Exists(static layout => layout.View.Drop.IsSome)))),
                None: () => hits.Filter(hit => Thumb(hit).IsSome || hit.Inits.Exists(prefix =>
                    Scene.PartAt(prefix).IsSome || !Scene.Anchoring(prefix).IsEmpty || Scene.Index.Find(prefix).Exists(static layout => layout.View is View.Plot { Hovered.IsSome: true }))))).IfNone([]),
        }).Unhandled;

    private Mount Nearing(Mount prior) =>
        (this with {
            Nearest = ReferenceEquals(prior.Scene, Scene) && prior.Routing.Pointer == Routing.Pointer
                ? Nearest.Filter(near => Scene.Index.Find(near.Plot).Exists(static layout => layout.View is View.Plot))
                : from at in Routing.Pointer
                  from layout in Scene.Order.Rev().Find(layout => layout.View is View.Plot plot && (plot.Pointed.IsSome || plot.Crosshair.IsSome) && !layout.Path.Inits.Exists(Scene.Inert.Contains) && Scene.Visible(layout.Path).Exists(shown => shown.Contains(at)))
                  from near in Scene.Nearest(layout.Path, at)
                  select near,
        }).Unhandled;

    private Mount Fragmenting(Mount prior) =>
        ReferenceEquals(prior.Scene, Scene) && prior.Nearest == Nearest && prior.Routing.Capture.IsNone && Routing.Capture.IsNone
            ? Unhandled
            : Overlays switch {
                var wanted => toSeq(wanted.Keys).Concat(Scene.Fragments.Keys).Distinct().Fold(Unhandled, (routed, path) =>
                    wanted.Find(path) == routed.Scene.Fragments.Find(path).Map(static layout => layout.View) ? routed : routed.Then(next => next.Applied(next.Scene.Fragment(path, wanted.Find(path))))),
            };

    private Mount Watching(Mount prior) =>
        ReferenceEquals(prior.Scene, Scene) && prior.Hover == Hover && prior.Routing.Pointer == Routing.Pointer && prior.Nearest == Nearest ? Unhandled : WithEffects(Watched(Some(prior)));

    private Mount Tweening(Mount prior, Duration now) {
        (Animations Animations, Seq<SKRect> Damage) faded = Animations.Pruned(Scene).Fading(prior.Scene, Scene, prior.Active(), Active(), now);
        return this with { Animations = faded.Animations, Timers = Timers.Rearmed(Scene, Routing.Pointer, Pending, now), LastTick = now, Output = new Output(faded.Damage, [], None) };
    }

    private Seq<IO<Unit>> Watched(Option<Mount> prior) =>
        Scene.Order.Bind(layout => Watched(
                from held in prior
                from was in held.Scene.Index.Find(layout.Path)
                select new HoverState(held.Scene, was, held.Hover, held.Routing.Pointer, held.Nearest),
                new HoverState(Scene, layout, Hover, Routing.Pointer, Nearest)));

    private static Seq<IO<Unit>> Watched(Option<HoverState> before, HoverState after) =>
        after.Layout.View.Part.Map(part => part.Watched(before.Filter(static held => held.Layout.View.Part.IsSome), after)).IfNone([])
        + Optional(after.Layout.View as View.Plot).ToSeq().Bind(plot =>
            (from hovered in plot.Hovered where before.Bind(static held => held.Plotted) != after.Plotted select hovered(after.Plotted)).ToSeq()
            + (from pointed in plot.Pointed where before.Bind(static held => held.Near) != after.Near select pointed(after.Near)).ToSeq());

    // --- [ROUTES]

    internal Mount Unhandled => this with { Output = Output.Empty };

    internal Mount Handled => this with { Output = Output with { Effects = Some(Output.Effects.IfNone([])) } };

    internal Mount WithEffects(Seq<IO<Unit>> effects) => this with { Output = new Output([], [], Some(effects).Filter(static held => !held.IsEmpty)) };

    internal Mount Applied(SceneChange change) => this with { Scene = change.Scene, Output = new Output(change.Damage, [], None) };

    internal Mount Then(Func<Mount, Mount> next) => next(Unhandled) switch { var after => after with { Output = Output.Combine(after.Output) } };

    internal Mount Otherwise(Func<Mount, Mount> next) => Output.Effects.IsSome ? this : Then(next);

    // --- [LAYERS]

    internal Mount Toggled(PlacedLayer held, Option<SKPoint> at) =>
        at.Filter(_ => held.Layer.Trigger.Exists(static trigger => trigger.AtPointer)) switch {
            var pointed when Opened.ContainsKey(held.Layout.Path) && pointed.IsNone => Closed([held]).Handled,
            var pointed => Refocused(held.Anchor.Bind(anchor => Focusing(Element(anchor, anchor), traversed: false)) | Routing.Focus)
                .Then(next => next.Closed(next.Dismissible.Filter(open => !open.Holds(held.Layout.Path))))
                .Then(next => next.Reopened(next.Opened.AddOrUpdate(held.Layout.Path, pointed)))
                .Handled,
        };

    internal Mount Closed(Seq<PlacedLayer> closing) =>
        Reopened(Opened.RemoveRange(closing.Map(static held => held.Layout.Path))).Then(next => next.WithEffects(closing.Choose(static held => held.Layer.Dismissed)));

    internal Mount Picked(Seq<Key> owner) =>
        Closed(Layers.Filter(held => held.Holds(owner) && owner.Inits.Skip(held.Layout.Path.Count)
            .Exists(prefix => Scene.Index.Find(prefix).Exists(static layout => layout.View.Accessible.Role.Exists(static role => role.Picks)))));

    // --- [FOCUS]

    internal Option<Focus> Focusing(Option<Seq<Key>> element, bool traversed) =>
        element.Map(target => Routing.Focus.Filter(focus => focus.Element == target).IfNone(() => new Focus(target, traversed)));

    internal Mount Refocused(Option<Focus> next) =>
        next.Map(static focus => focus.Element) == Focused ? Unhandled : (this with { Routing = Routing with { Focus = next } }).Unhandled;

    internal Option<Seq<Key>> Element(Seq<Key> owner, Seq<Key> over) => over.Inits.Rev().TakeWhile(prefix => prefix.Count >= owner.Count).Find(Elements.ContainsKey) | Entry(owner);

    private Option<Seq<Key>> Entry(Seq<Key> stop) =>
        Scene.Index.Find(stop).Bind(layout => (layout.Lead.ToSeq() + layout.Preorder.Map(static held => held.Path))
            .Find(entry => Elements.ContainsKey(entry) && Scene.Owner(entry).Exists(owner => owner.Path == stop)));

    // --- [LOOKUPS]

    internal Option<Scroller> Thumb(Seq<Key> path) => Scene.Scroller(path).Filter(scroller => scroller.Owner != path);

    private HashMap<Seq<Key>, Duration> Pending =>
        toHashMap(Scene.Triggered.Filter(held => held.Delay.IsSome && !Opened.ContainsKey(held.Layout.Path) && Over(held)).Choose(static held => held.Delay.Map(delay => (held.Layout.Path, delay))));

    private bool Over(PlacedLayer held) =>
        Routing.Pointer.Exists(at => held.Anchor.Match(Some: anchor => Scene.Visible(anchor).Exists(shown => shown.Contains(at)), None: static () => true)
            || (Opened.ContainsKey(held.Layout.Path) && Scene.Visible(held.Layout.Path).Exists(shown => shown.Contains(at))));

    private Seq<Seq<Key>> Clocks(Seq<Seq<Key>> animated) =>
        Scene.Timed.Map(static layout => layout.Path).Concat(animated).Filter(path =>
            !Host.Theme.Conditions.Contains(Condition.ReduceMotion) || path.Inits.Exists(above => Scene.Index.Find(above).Exists(static layout => layout.Style.Conditions.Contains(Condition.Busy))));

    private HashMap<Seq<Key>, View> Overlays =>
        toHashMap((from near in Nearest
                   from layout in Scene.Index.Find(near.Plot)
                   from plot in Optional(layout.View as View.Plot)
                   from crosshair in plot.Crosshair
                   select (near.Plot, crosshair(near))).ToSeq())
            .AddOrUpdateRange((from part in Routing.Part
                               from overlay in Previewed(part) | part.Track.Preview.Map(figure => Selected(part.Owner, figure))
                               select (part.Owner, overlay)).ToSeq());

    private View Selected(Seq<Key> owner, Figure figure) =>
        new View.Mark(figure) { Role = Roles.Selection } switch {
            var mark when (from layout in Scene.Index.Find(owner) from place in Scene.Places.Find(owner) from plot in place.Plotted(layout) select plot).IsSome => mark,
            var mark => new View.Plot(PlotPlane.Cartesian.Unit, [mark]).Grown,
        };

    private Option<View> Previewed(PointerCapture.Part part) =>
        from lifting in part.Track.Lifted
        from layout in Scene.Index.Find(part.Owner)
        from lift in layout.View.Lift
        from preview in lift.Preview
        from place in Scene.Places.Find(part.Owner)
        from local in place.Local(part.Press.At)
        let frame = Place.Absolute(layout.Frame, place.Origin)
        from at in place.Plotted(layout).Match(Some: plot => plot.At(local), None: () => Some((Vector2)(local - frame.Location)))
        select preview(at);

    private CursorShape Prospective(Seq<Key> path, SKPoint pointer) =>
        Thumb(path).IsSome ? CursorShape.Default
        : path.Inits.Rev().Choose(Scene.PartAt).Head.Exists(static owner => !owner.Enabled) ? CursorShape.NotAllowed
        : Scene.Owners(path).Choose(owner => owner.Part.Began(new Gesture.Press(pointer, None, 1) { Modifiers = Routing.Modifiers }, new HitTarget(Scene, owner, path))
                .Map(static reply => reply.Track.Map(static track => track.Cursor).IfNone(CursorShape.Default)))
            .Head.IfNone(CursorShape.Default);

    private HashSet<InteractionState> Active() =>
        toHashSet((Hover.Head.Filter(path => Scene.Index.Find(path).Exists(static layout => !layout.Style.Conditions.Contains(Condition.Disabled))).ToSeq() + Scene.Highlighted).Map(static path => new InteractionState(path, Interaction.Hover))
            .Concat(Routing.Pressed.Map(static path => new InteractionState(path, Interaction.Pressed)).ToSeq())
            .Concat(Routing.Focus.Filter(focus => Host.Visibility.HasKeyFocus && focus.Traversed).Map(static focus => new InteractionState(focus.Element, Interaction.Focused)).ToSeq()));
}

public sealed record Output(Seq<SKRect> Damage, Seq<AccessibilityNotice> Notices, Option<Seq<IO<Unit>>> Effects) : Monoid<Output> {
    public static Output Empty { get; } = new([], [], None);

    public Option<Lifted> Lifted { get; init; }

    public Output Combine(Output rhs) =>
        new(Damage + rhs.Damage, Notices + rhs.Notices, (Effects | rhs.Effects).Map(_ => Effects.IfNone([]) + rhs.Effects.IfNone([]))) { Lifted = Lifted | rhs.Lifted };
}
