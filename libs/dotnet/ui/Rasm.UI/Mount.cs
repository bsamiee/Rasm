using System.Numerics;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Mount {
    // --- [STATE]
    private Mount(Host host, Scene scene) => (Host, Scene, Routing, Tree) = (host, scene, Routing.Idle, Access.Project(scene, host.Culture));

    public Host Host { get; internal init; }
    public Scene Scene { get; internal init; }
    public Seq<Seq<Key>> Hover { get; internal init; }

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
    internal Duration Stepped { get; init; }

    // --- [DERIVED]
    public Option<Seq<Key>> Captured => Routing.Capture.Map(static capture => capture.Switch(part: static part => part.Owner, handle: static handle => handle.Adjuster));

    public Option<Seq<Key>> Focused => Routing.Focus.Map(static focus => focus.Element);

    public CursorShape Cursor =>
        Routing.Capture.Match(
            Some: static capture => capture.Switch(part: static part => part.Track.Cursor, handle: static _ => CursorShape.Default),
            None: () => (from path in Hover.Head from pointer in Routing.Pointer select Prospective(path, pointer)).IfNone(CursorShape.Default));

    public bool Animating => Host.Visibility.Drawn && (Animations.Moving || (!Host.Theme.Conditions.Contains(Condition.ReduceMotion) && !Scene.Timed.IsEmpty));

    public Seq<Hosted> Hosted => Animations.Hosted(Scene, Stepped);

    public HashMap<Washed, float> Washes(Duration now) => Animations.Washes(Active(), now);

    // --- [TRANSITIONS]
    public static Fin<Mounted> Create(Host host, DefaultStyles defaults, Node view, HashMap<Seq<Key>, float> adjusted, Shaping shaping) =>
        Scene.Of(Styles.Root(host.Theme, defaults, host.Placement.Root), view, host.Size, adjusted, shaping).Map(static scene => scene.Revealed(None).Scene).Map(scene => {
            Mount mount = new(host, scene);
            return new Mounted(mount, new Output(Seq(SKRect.Create(host.Size)), [], Sequenced(mount.Effected(mount.Watched(None)).Answer)));
        });

    public Fin<Mounted> Show(Node view, Duration now) =>
        Scene.Reconcile(view).Map(moved => Completed(new Routed(this with { Scene = moved.Scene, Animations = Animations.Opened(Scene, moved.Scene, now) }, moved.Damage, None), now));

    public Mounted Rehost(Host host, Duration now) =>
        Completed(Restyled(host).Then(mount => Host.Visibility.Keyed && !host.Visibility.Keyed ? mount.Cancel() : mount.Unanswered), now);

    public Mounted Receive(Input input, Duration now) => Completed(this.Dispatched(input, now), now);

    public Mounted Tick(Duration now) {
        Animations.Ticked ticked = Animations.Tick(Scene, Stepped, now);
        return new(this with { Animations = ticked.Animations.Timed(Scene, Routing.Pointer, now), Stepped = now },
            new Output(ticked.Damage.Concat(Scene.Timed.Choose(laid => Scene.Bounds(laid.Path))), [], Sequenced(Some(ticked.Elapsed).Filter(static effects => !effects.IsEmpty))));
    }

    private Routed Restyled(Host host) {
        Rearranged styled = Host.Theme == host.Theme && Host.Placement.Root == host.Placement.Root ? Scene.Unchanged : Scene.Restyle(host.Theme, host.Placement.Root);
        Rearranged sized = styled.Scene.Layout.Frame.Size == host.Size ? styled.Scene.Unchanged : styled.Scene.Resize(host.Size);
        return new Routed(this with { Host = host, Scene = sized.Scene }, styled.Damage.Concat(sized.Damage).Concat(Host.Visibility == host.Visibility ? [] : Focused.Bind(sized.Scene.Bounds).ToSeq()), None);
    }

    // --- [SETTLE]
    private Mounted Completed(Routed routed, Duration now) {
        Routed done = routed.Then(next => next.Revealing(this)).Then(next => next.Pruning(this)).Then(static next => next.Hovering()).Then(next => next.Watching(this)).Then(next => next.Tweening(this, now));
        Option<Vector2> detent = done.Mount.Routing.Tracked.Bind(static track => track.Detent);
        return new(done.Mount, new Output(
            done.Damage.Concat(done.Mount.Routing.Focus != Routing.Focus ? Seq(Focused, done.Mount.Focused).Somes().Choose(done.Mount.Scene.Bounds) : []),
            (done.Mount.Projected(this) ? Access.Changes(Elements, done.Mount.Elements) : [])
                .Concat(done.Mount.Focused != Focused ? done.Mount.Focused.Map(static path => (AccessibilityNotice)new AccessibilityNotice.FocusChanged(path)).ToSeq() : [])
                .Concat(detent.IsSome && detent != Routing.Tracked.Bind(static track => track.Detent) ? Seq<AccessibilityNotice>(new AccessibilityNotice.Detent()) : []),
            Sequenced(done.Answer)));
    }

    private bool Projected(Mount prior) => !ReferenceEquals(prior.Scene, Scene) || !prior.Host.Culture.Equals(Host.Culture);

    private Routed Revealing(Mount prior) {
        if (!Projected(prior)) return Unanswered;
        Rearranged revealed = Scene.Revealed(Some(prior.Scene));
        return new Routed(this with { Scene = revealed.Scene, Tree = Access.Project(revealed.Scene, Host.Culture) }, revealed.Damage, None);
    }

    private Routed Pruning(Mount prior) {
        Option<(Option<PointerCapture> Kept, Seq<IO<Unit>> Cancelled)> capture = Routing.Capture.Map(held => held.Switch<Mount, (Option<PointerCapture> Kept, Seq<IO<Unit>> Cancelled)>(
            this,
            part: static (mount, part) => mount.Scene.Owning(part.Owner).IsSome ? (Some<PointerCapture>(part), []) : (None, part.Track.Cancelled()),
            handle: static (mount, handle) => (mount.Scene.Adjuster(handle.Adjuster).Map(_ => (PointerCapture)handle), [])));
        Option<(Option<Latch> Kept, Seq<IO<Unit>> Cancelled)> latch = Routing.Latch.Map(held => held.Switch<Mount, (Option<Latch> Kept, Seq<IO<Unit>> Cancelled)>(
            this,
            part: static (mount, part) => mount.Scene.Owning(part.Owner).IsSome ? (Some<Latch>(part), []) : (None, part.Live.Map(static live => live.Track.Cancelled()).IfNone([])),
            scroll: static (mount, scroll) => (mount.Scene.Adjuster(scroll.Adjuster).Map(_ => (Latch)scroll), [])));
        return (this with {
            Routing = Routing with {
                Capture = capture.Bind(static kept => kept.Kept),
                Latch = latch.Bind(static kept => kept.Kept),
                Focus = Routing.Focus.Map(focus => Followed(prior, focus)).Filter(followed => Elements.ContainsKey(followed.Element) && (Scene.Owner(followed.Element).IsSome || Scene.Adjuster(followed.Element).IsSome)),
            },
        }).Effected(capture.Map(static kept => kept.Cancelled).IfNone([]) + latch.Map(static kept => kept.Cancelled).IfNone([]));
    }

    private Focus Followed(Mount prior, Focus focus) =>
        (from owner in Scene.Owner(focus.Element)
         from lead in Scene.Lead(owner.Laid)
         where lead != focus.Element && Elements.ContainsKey(lead) && (prior.Scene.Index.Find(owner.Path).Bind(Scene.Lead) != Some(lead) || !prior.Elements.ContainsKey(lead))
         select focus with { Element = lead }).IfNone(focus);

    private Routed Hovering() =>
        Unanswered with {
            Mount = this with {
                Hover = Routing.Pointer.Map(Hits).Map(hits => Routing.Pressing.Match(
                    Some: pressing => hits.Filter(hit => hit == pressing),
                    None: () => hits.Filter(hit => Handle(hit).IsSome || Scene.Above(hit).Exists(prefix => Scene.Parted(prefix).IsSome || Scene.Index.Find(prefix).Exists(static laid => laid.Node.Hovering.IsSome))))).IfNone([]),
            },
        };

    private Routed Watching(Mount prior) =>
        ReferenceEquals(prior.Scene, Scene) && prior.Hover == Hover && prior.Routing.Pointer == Routing.Pointer ? Unanswered : Effected(Watched(Some(prior)));

    private Routed Tweening(Mount prior, Duration now) {
        Animations.Washing faded = Animations.Settled(Scene).Fading(prior.Scene, Scene, prior.Active(), Active(), now);
        return new Routed(this with { Animations = faded.Animations.Timed(Scene, Routing.Pointer, now), Stepped = now }, faded.Damage, None);
    }

    private Seq<IO<Unit>> Watched(Option<Mount> prior) =>
        Scene.Order.Bind(laid => Watched(
            from held in prior
            from was in held.Scene.Index.Find(laid.Path)
            select new Sight(held.Scene, was, held.Hover, held.Routing.Pointer),
            new Sight(Scene, laid, Hover, Routing.Pointer)));

    private static Seq<IO<Unit>> Watched(Option<Sight> before, Sight after) =>
        after.Laid.Node.Part.Map(part => part.Watched(before.Filter(static held => held.Laid.Node.Part.IsSome), after)).IfNone([])
        + (from hovered in after.Laid.Node.Hovering
           let now = after.Plotted
           where before.Bind(static held => held.Plotted) != now
           select hovered(now)).ToSeq();

    private static Option<IO<Unit>> Sequenced(Option<Seq<IO<Unit>>> answer) => answer.Map(static effects => effects.TraverseM(static effect => effect).As().Map(static _ => unit));

    // --- [ROUTE]
    internal Routed Unanswered => new(this, [], None);

    internal Routed Effected(Seq<IO<Unit>> effects) => new(this, [], effects.IsEmpty ? None : Some(effects));

    internal Routed Answered(Seq<IO<Unit>> effects) => new(this, [], Some(effects));

    // --- [FOCUS]
    internal Option<Focus> Focusing(Option<Seq<Key>> element, bool traversed) =>
        element.Map(target => Routing.Focus.Filter(focus => focus.Element == target).IfNone(() => new Focus(target, traversed)));

    internal Routed Refocused(Option<Focus> next) =>
        next.Map(static focus => focus.Element) == Focused ? Unanswered : (this with { Routing = Routing with { Focus = next } }).Unanswered;

    internal Option<Seq<Key>> Element(Seq<Key> owner, Seq<Key> over) => Scene.Above(over).TakeWhile(prefix => prefix.Count >= owner.Count).Find(Elements.ContainsKey) | Entry(owner);

    private Option<Seq<Key>> Entry(Seq<Key> stop) =>
        Scene.Index.Find(stop).Bind(laid => (Scene.Lead(laid).ToSeq() + Scene.Preorder(laid).Map(static held => held.Path))
            .Find(entry => Elements.ContainsKey(entry) && Scene.Owner(entry).Exists(owner => owner.Path == stop)));

    // --- [LOOKUP]
    internal Seq<Seq<Key>> Hits(SKPoint at) => Scene.Hit(at).Map(static hit => hit.Laid.Path);

    internal Option<Adjuster> Handle(Seq<Key> path) => Scene.Adjuster(path).Filter(adjuster => adjuster.Owner != path);

    private CursorShape Prospective(Seq<Key> path, SKPoint pointer) =>
        Handle(path).IsSome ? CursorShape.Default
        : Scene.Above(path).Choose(Scene.Parted).Head.Exists(static owner => !owner.Enabled) ? CursorShape.NotAllowed
        : Scene.Owners(path).Choose(owner => owner.Part.Began(new Gesture.Press(pointer, None, 1, 1f) { Modifiers = Routing.Modifiers }, Scene.Aimed(owner, path))
                .Switch(pass: static _ => Option<CursorShape>.None, take: static take => Some(take.Track.Map(static track => track.Cursor).IfNone(CursorShape.Default))))
            .Head.IfNone(CursorShape.Default);

    private HashSet<Washed> Active() =>
        toHashSet(Hover.Head.Filter(path => Scene.Index.Find(path).Exists(static laid => !laid.Style.Conditions.Contains(Condition.Disabled))).Map(static path => new Washed(path, Interaction.Hover)).ToSeq()
            .Concat(Routing.Pressing.Map(static path => new Washed(path, Interaction.Pressed)).ToSeq())
            .Concat(Routing.Focus.Filter(focus => Host.Visibility.Keyed && focus.Traversed).Map(static focus => new Washed(focus.Element, Interaction.Focused)).ToSeq()));
}

public sealed record Mounted(Mount Mount, Output Output);
