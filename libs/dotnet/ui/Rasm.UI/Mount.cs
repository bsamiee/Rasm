using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Visibility {
    public static readonly Visibility Hidden = new();
    public static readonly Visibility Shown = new();
    public static readonly Visibility Active = new();
}

public sealed record Host(SKSize Size, Theme Theme, Visibility Visibility, bool Cycles, CultureInfo Culture);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccessibilityNotice {
    public sealed record Announced(string Text, Live Live) : AccessibilityNotice;

    public sealed record ValueChanged(Seq<Key> Element) : AccessibilityNotice;

    public sealed record SelectionChanged(Seq<Key> Container) : AccessibilityNotice;

    public sealed record FocusChanged(Seq<Key> Element) : AccessibilityNotice;

    public sealed record LayoutChanged : AccessibilityNotice;

    public sealed record Detent : AccessibilityNotice;
}

public sealed record Output(Seq<SKRect> Damage, Seq<AccessibilityNotice> Notices, Option<IO<Unit>> Answer);

public sealed record Mount {
    // --- [STATE]
    private readonly HashMap<Seq<Key>, Element> elements;

    private Mount(Host host, Scene scene) => (Host, Scene, Routing, Tree) = (host, scene, Routing.Idle, Access.Project(scene, host.Culture));

    public Host Host { get; private init; }
    public Scene Scene { get; private init; }
    public Seq<Seq<Key>> Hover { get; private init; }

    public Seq<Element> Tree {
        get;
        private init {
            field = value;
            elements = Access.Indexed(value);
        }
    }

    internal Routing Routing { get; private init; }
    internal HashMap<(Seq<Key> Path, Interaction Interaction), Animated> Fades { get; private init; }

    // --- [DERIVED]
    public Option<Seq<Key>> Captured => Routing.Capture.Map(static capture => capture.Switch(part: static part => part.Owner, handle: static handle => handle.Adjuster));

    public Option<Seq<Key>> Focused => Routing.Focus.Map(static focus => focus.Element);

    public Option<Editor> Editing => Routing.Focus.Bind(static focus => focus.Editor);

    public Option<SKRect> Caret =>
        from focus in Routing.Focus
        from editor in focus.Editor
        from frame in Scene.Frame(focus.Element)
        let caret = editor.Caret
        select SKRect.Create(frame.Location + caret.Location, caret.Size);

    public Cursor Cursor =>
        Routing.Capture.Match(
            Some: capture => capture.Switch(Scene, part: static (_, part) => part.Track.Cursor, handle: static (scene, handle) => scene.Adjuster(handle.Adjuster).Map(static adjuster => adjuster.Cursor).IfNone(Cursor.Default)),
            None: () => (from path in Hover.Head from pointer in Routing.Pointer select Prospective(path, pointer)).IfNone(Cursor.Default));

    public bool Animating => Host.Visibility != Visibility.Hidden && (!Fades.IsEmpty || (!Host.Theme.Conditions.Contains(Condition.ReduceMotion) && !Scene.Timed.IsEmpty));

    public Option<string> Help =>
        Hover.Head.Bind(path => Scene.Above(path).Choose(elements.Find) switch {
            var above => Seq(
                    above.Choose(static element => element.Accessible.Help).Head
                    | Scene.Index.Find(path).Bind(static laid => Scene.Preorder(laid).Choose(static held => held.Texted.Filter(static text => text.Typeset.Truncated).Map(static text => text.Source.Content)).Head),
                    above.Choose(static element => element.Accessible.Shortcut).Head)
                .Somes() switch {
                    { IsEmpty: true } => Option<string>.None,
                    var shown => Some(string.Join(' ', shown)),
                },
        });

    public HashMap<(Seq<Key> Path, Interaction Interaction), float> Washes(Duration now) =>
        Active() switch {
            var active => toHashMap(active.ToSeq().Map(key => (Key: key, Weight: Fades.Find(key).Map(fade => fade.At(now).Value).IfNone(1f)))
                .Concat(toSeq(Fades.Filter((key, _) => !active.Contains(key)).AsIterable()).Map(entry => (entry.Key, Weight: entry.Value.At(now).Value)))
                .Map(static entry => (entry.Key, Weight: float.Clamp(entry.Weight, 0f, 1f)))
                .Filter(static entry => entry.Weight > 0f)),
        };

    // --- [TRANSITIONS]
    public static Fin<(Mount Mount, Output Output)> Create(Host host, Shipped shipped, Node view, HashMap<Seq<Key>, float> adjusted) =>
        Scene.Of(host.Theme, shipped, view, host.Size, adjusted).Map(static scene => Revealed(None, scene).Scene).Map(scene => new Mount(host, scene) switch {
            var mount => (mount, new Output(Seq(SKRect.Create(host.Size)), [], Sequenced(mount.Effected(mount.Watched(None)).Answer))),
        });

    public Fin<(Mount Mount, Output Output)> Show(Node view, Duration now) =>
        Scene.Reconcile(view).Map(moved => Settled(new Routed(this with { Scene = moved.Scene }, moved.Damage, None), now));

    public (Mount Mount, Output Output) Rehost(Host host, Duration now) =>
        Settled(Restyled(host).Then(mount => Host.Visibility == Visibility.Active && host.Visibility != Visibility.Active ? mount.Cancel() : mount.Unanswered), now);

    public (Mount Mount, Output Output) Receive(Input input, Duration now) => Settled(Dispatched(input, now), now);

    public (Mount Mount, Output Output) Tick(Duration now) =>
        (this with { Fades = Fades.Filter(fade => fade.At(now).Continue) }, new Output(Fades.Keys.ToSeq().Choose(key => Washed(key).Map(static held => held.Damage)).Concat(Scene.Timed.Choose(laid => Scene.Bounds(laid.Path))), [], None));

    private Routed Restyled(Host host) =>
        (Host.Theme == host.Theme ? (Scene, Seq<SKRect>()) : Scene.Restyle(host.Theme)) switch {
            var (styled, restyled) => (styled.Layout.Frame.Size == host.Size ? (styled, Seq<SKRect>()) : styled.Resize(host.Size)) switch {
                var (sized, resized) => new Routed(this with { Host = host, Scene = sized }, restyled.Concat(resized).Concat(Host.Visibility == host.Visibility ? [] : Focused.Bind(sized.Bounds).ToSeq()), None),
            },
        };

    // --- [SETTLE]
    private (Mount Mount, Output Output) Settled(Routed routed, Duration now) =>
        routed.Then(next => next.Revealing(this)).Then(next => next.Pruning(this)).Then(static next => next.Hovering()).Then(next => next.Watching(this)).Then(next => next.Fading(this, now)) switch {
            var settled => (settled.Mount, new Output(
                settled.Damage.Concat(settled.Mount.Routing.Focus != Routing.Focus ? Seq(Focused, settled.Mount.Focused).Somes().Choose(settled.Mount.Scene.Bounds) : []),
                (settled.Mount.Projected(this) ? Access.Changes(elements, settled.Mount.elements) : [])
                    .Concat(settled.Mount.Focused != Focused ? settled.Mount.Focused.Map(static path => (AccessibilityNotice)new AccessibilityNotice.FocusChanged(path)).ToSeq() : [])
                    .Concat(settled.Mount.Routing.Tracked.Bind(static track => track.Detent) switch {
                        var detent => detent.IsSome && detent != Routing.Tracked.Bind(static track => track.Detent) ? Seq<AccessibilityNotice>(new AccessibilityNotice.Detent()) : [],
                    }),
                Sequenced(settled.Answer))),
        };

    private bool Projected(Mount prior) => !ReferenceEquals(prior.Scene, Scene) || !prior.Host.Culture.Equals(Host.Culture);

    private Routed Revealing(Mount prior) =>
        Projected(prior)
            ? Revealed(Some(prior.Scene), Scene) switch {
                var (revealed, damage) => new Routed(this with { Scene = revealed, Tree = Access.Project(revealed, Host.Culture) }, damage, None),
            }
            : Unanswered;

    private Routed Pruning(Mount prior) =>
        (Capture: Routing.Capture.Map(capture => capture.Switch<Mount, (Option<Capture> Kept, Seq<IO<Unit>> Cancelled)>(
                this,
                part: static (mount, part) => mount.Owning(part.Owner).IsSome ? (Some<Capture>(part), []) : (None, part.Track.Cancelled()),
                handle: static (mount, handle) => (mount.Scene.Adjuster(handle.Adjuster).Map(_ => (Capture)handle), []))),
            Latch: Routing.Latch.Map(latch => latch.Switch<Mount, (Option<Latch> Kept, Seq<IO<Unit>> Cancelled)>(
                this,
                part: static (mount, part) => mount.Owning(part.Owner).IsSome ? (Some<Latch>(part), []) : (None, part.Live.Map(static live => live.Track.Cancelled()).IfNone([])),
                scroll: static (mount, scroll) => (mount.Scene.Adjuster(scroll.Adjuster).Map(_ => (Latch)scroll), []))),
            Focus: Routing.Focus.Map(focus => Followed(prior, focus) switch {
                var followed when elements.ContainsKey(followed.Element) && (Owner(followed.Element).IsSome || Scene.Adjuster(followed.Element).IsSome) => (Kept: Some(followed), Left: Seq<IO<Unit>>()),
                _ => (Kept: Option<Focus>.None, Left: prior.Left(focus)),
            })) switch {
                var held => (this with {
                    Routing = Routing with {
                        Capture = held.Capture.Bind(static kept => kept.Kept),
                        Latch = held.Latch.Bind(static kept => kept.Kept),
                        Focus = held.Focus.Bind(static kept => kept.Kept),
                    },
                }).Effected(held.Capture.Map(static kept => kept.Cancelled).IfNone([]) + held.Latch.Map(static kept => kept.Cancelled).IfNone([]) + held.Focus.Map(static kept => kept.Left).IfNone([])),
            };

    private Focus Followed(Mount prior, Focus focus) =>
        (from owner in Owner(focus.Element)
         from lead in Lead(owner.Laid)
         where lead != focus.Element && elements.ContainsKey(lead) && (prior.Scene.Index.Find(owner.Path).Bind(Lead) != Some(lead) || !prior.elements.ContainsKey(lead))
         select focus with { Element = lead }).IfNone(focus);

    private Routed Hovering() =>
        Unanswered with {
            Mount = this with {
                Hover = Routing.Pointer.Map(Hits).Map(hits => Routing.Pressing.Match(
                    Some: pressing => hits.Filter(hit => hit == pressing),
                    None: () => hits.Filter(hit => Handle(hit).IsSome || Scene.Above(hit).Exists(prefix => Parted(prefix).IsSome || Scene.Index.Find(prefix).Exists(static laid => laid.Node.Hovering.IsSome))))).IfNone([]),
            },
        };

    private Routed Watching(Mount prior) =>
        ReferenceEquals(prior.Scene, Scene) && prior.Hover == Hover && prior.Routing.Pointer == Routing.Pointer ? Unanswered : Effected(Watched(Some(prior)));

    private Routed Fading(Mount prior, Duration now) =>
        (Was: prior.Active(), Now: Active()) switch {
            var active => active.Was.SymmetricExcept(active.Now) switch {
                var changed => new Routed(
                    this with { Fades = changed.ToSeq().Fold(Fades.Filter((key, _) => Scene.Index.ContainsKey(key.Path)), (fades, key) => Faded(fades, key, active.Now, now)) },
                    Swept(prior, active.Was.Union(active.Now).Union(toHashSet(Fades.Filter(fade => fade.At(now).Continue).Keys)), changed),
                    None),
            },
        };

    private Seq<SKRect> Swept(Mount prior, LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> washed, LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> changed) =>
        from key in toSeq(washed)
        let now = Washed(key)
        let was = prior.Washed(key).Bind(static held => held.Beyond)
        let beyond = now.Bind(static held => held.Beyond)
        from damage in toSeq(((changed.Contains(key) ? now.Map(static held => held.Damage).ToSeq() : []) + (was == beyond ? [] : Seq(was, beyond).Somes())).Distinct())
        select damage;

    private HashMap<(Seq<Key> Path, Interaction Interaction), Animated> Faded(HashMap<(Seq<Key> Path, Interaction Interaction), Animated> fades, (Seq<Key> Path, Interaction Interaction) key,
                                                                         LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> active, Duration now) =>
        (Curve: Washed(key).Map(static held => held.Motion).IfNone(new TimingCurve.Instant()), Target: active.Contains(key) ? 1f : 0f) switch {
            var (curve, target) => fades.Find(key).Map(fade => fade.Toward(target, now, curve)).IfNone(new Animated(1f - target, target, now, curve)) switch {
                var fade when fade.At(now).Continue => fades.AddOrUpdate(key, fade),
                _ => fades.Remove(key),
            },
        };

    private Seq<IO<Unit>> Watched(Option<Mount> prior) =>
        Scene.Order.Bind(laid => Watched(
            from held in prior
            from was in held.Scene.Index.Find(laid.Path)
            select new Sight(held.Scene, was, held.Hover, held.Routing.Pointer),
            new Sight(Scene, laid, Hover, Routing.Pointer)));

    private static Seq<IO<Unit>> Watched(Option<Sight> before, Sight after) =>
        after.Laid.Node.Part.Map(part => part.Watched(before.Filter(static held => held.Laid.Node.Part.IsSome), after)).IfNone([])
        + after.Laid.Node.Hovering.Map(hover => after.Plotted switch {
            var now => before.Bind(static held => held.Plotted) == now ? Seq<IO<Unit>>() : [hover(now)],
        }).IfNone([]);

    private static (Scene Scene, Seq<SKRect> Damage) Revealed(Option<Scene> prior, Scene scene) =>
        scene.Order.Choose(laid => Lead(laid).Filter(lead => prior.Bind(held => held.Index.Find(laid.Path)).Bind(Lead) != Some(lead)))
            .Fold((Scene: scene, Damage: Seq<SKRect>()), static (held, lead) => held.Scene.Reveal(lead) switch {
                var (shown, damage) => (shown, held.Damage.Concat(damage)),
            });

    private static Option<IO<Unit>> Sequenced(Option<Seq<IO<Unit>>> answer) => answer.Map(static effects => effects.TraverseM(static effect => effect).As().Map(static _ => unit));

    // --- [ROUTE]
    private sealed record Owned(Seq<Key> Path, Laid Laid, Part Part, SKRect Bounds) {
        public bool Enabled => !Laid.Style.Conditions.Contains(Condition.Disabled);
    }

    private sealed record Routed(Mount Mount, Seq<SKRect> Damage, Option<Seq<IO<Unit>>> Answer) {
        public Routed Then(Func<Mount, Routed> next) =>
            next(Mount) switch {
                var after => new(after.Mount, Damage.Concat(after.Damage), Answer.Match(Some: effects => Some(effects + after.Answer.IfNone([])), None: () => after.Answer)),
            };

        public Routed Otherwise(Func<Mount, Routed> next) => Answer.IsSome ? this : Then(next);

        public Routed Routes(Func<Routing, Routing> change) => this with { Mount = Mount with { Routing = change(Mount.Routing) } };

        public Routed Taken() => this with { Answer = Some(Answer.IfNone([])) };
    }

    private Routed Unanswered => new(this, [], None);

    private Routed Effected(Seq<IO<Unit>> effects) => new(this, [], effects.IsEmpty ? None : Some(effects));

    private Routed Answered(Seq<Key> owner, Seq<IO<Unit>> effects) => new(this, Editing.IsSome && Focused == Some(owner) ? Scene.Bounds(owner).ToSeq() : [], Some(effects));

    // --- [DISPATCH]
    private Routed Dispatched(Input input, Duration now) =>
        input.Switch(
            (Mount: this, Now: now),
            pointer: static (held, pointer) => pointer.Stroke.Switch(
                (held.Mount, held.Now, Pointer: pointer),
                began: static (state, began) => state.Mount.Uncaptured().Then(mount => mount.Pointed(Some(began.Value.At), state.Pointer.Modifiers)).Then(mount => mount.Pressed(began.Value, state.Pointer, state.Now)),
                changed: static (state, changed) => state.Mount.Pointed(Some(changed.Value.At), state.Pointer.Modifiers).Then(mount => mount.Moved(changed.Value, state.Pointer.Modifiers, state.Now)),
                ended: static (state, ended) => state.Mount.Pointed(Some(ended.Value.At), state.Pointer.Modifiers).Then(mount => mount.Released(ended.Value, state.Pointer.Modifiers)),
                cancelled: static (state, _) => state.Mount.Cancel()),
            hover: static (held, hover) => held.Mount.Pointed(hover.At, hover.Modifiers),
            wheel: static (held, wheel) => held.Mount.Pointed(Some(wheel.At), wheel.Modifiers).Then(mount => mount.Notched(Gesture.Scroll.Of(wheel.At, wheel.Delta, wheel.Modifiers))),
            scroll: static (held, scroll) => held.Mount.Pointed(Some(scroll.At), scroll.Modifiers).Then(mount => mount.Streamed<Gesture.Scroll, Vector2>(scroll.At, scroll.Delta, scroll.Modifiers)),
            magnify: static (held, magnify) => held.Mount.Pointed(Some(magnify.At), magnify.Modifiers).Then(mount => mount.Streamed<Gesture.Magnify, float>(magnify.At, magnify.Amount, magnify.Modifiers)),
            rotate: static (held, rotate) => held.Mount.Pointed(Some(rotate.At), rotate.Modifiers).Then(mount => mount.Streamed<Gesture.Rotate, float>(rotate.At, rotate.Radians, rotate.Modifiers)),
            perform: static (held, perform) => perform.Act.SwitchPartially(
                (held.Mount, Perform: perform),
                @default: static (state, _) => state.Mount.Targeted(state.Perform),
                cancel: static (state, _) => state.Mount.Routing.Capture.IsSome ? state.Mount.Uncaptured() : state.Mount.Targeted(state.Perform)),
            focused: static (held, _) => held.Mount.Restored(),
            blurred: static (held, _) => held.Mount.Cancel().Then(static mount => mount.Closed()));

    private Routed Pointed(Option<SKPoint> at, LanguageExt.HashSet<Modifier> modifiers) =>
        Unanswered.Routes(routing => routing with {
            Pointer = at,
            Modifiers = modifiers,
            Latch = at == routing.Pointer ? routing.Latch : routing.Latch.Filter(static latch => latch.Switch(part: static part => part.Live.IsSome, scroll: static scroll => scroll.Live.IsSome)),
        });

    private Routed Cancel() => Uncaptured().Then(static mount => mount.Halted());

    // --- [POINTER]
    private Routed Pressed(PointerSample sample, Input.Pointer pointer, Duration now) =>
        Hits(sample.At) switch {
            var hits => (from top in hits.Head from adjuster in Handle(top) select (Handle: top, adjuster.Value)).Match(
                Some: held => Unanswered.Routes(routing => routing with { Capture = new Capture.Handle(held.Handle, sample.At, held.Value) }).Taken(),
                None: () => new Gesture.Press(sample.At, None, pointer.Clicks, sample.Pressure) { Modifiers = pointer.Modifiers } switch {
                    var press => Asking(Chain(hits), (mount, owner, over) => mount.Asked(owner, over, press, now)),
                }),
        };

    private Routed Asked(Owned owner, Seq<Key> over, Gesture.Press press, Duration now) =>
        Landing(Element(owner.Path, over), static element => new Focus.Pressed(element)) switch {
            var landing => owner.Part.Began(press, Aimed(owner, over, landing)).Switch(
                (Mount: this, Owner: owner, Over: over, Press: press, Now: now, Landing: landing),
                pass: static (held, _) => held.Mount.Unanswered,
                take: static (held, take) => held.Mount.Refocused(held.Landing).Then(mount =>
                    (mount with { Routing = mount.Routing with { Capture = take.Track.Map(track => (Capture)new Capture.Part(held.Owner.Path, held.Over, held.Press, track, held.Now)) } })
                    .Answered(held.Owner.Path, take.Effects))),
        };

    private Routed Moved(PointerSample sample, LanguageExt.HashSet<Modifier> modifiers, Duration now) =>
        Routing.Capture.Match(
            Some: capture => capture.Switch(
                (Mount: this, Sample: sample, Modifiers: modifiers, Now: now),
                part: static (held, part) => held.Mount.Dragged(part, held.Sample, held.Modifiers, held.Now),
                handle: static (held, handle) => held.Mount.Slid(handle, held.Sample.At)),
            None: () => Unanswered);

    private Routed Dragged(Capture.Part part, PointerSample sample, LanguageExt.HashSet<Modifier> modifiers, Duration now) =>
        (part.Press.Followed(sample, Slop(part.Owner)) with { Modifiers = modifiers }) switch {
            var press when press == part.Press => Answered(part.Owner, []),
            var press => (float)(now - part.Time).TotalSeconds switch {
                var seconds => press with { Velocity = seconds > 0f ? (Vector2)(sample.At - part.Press.At) / seconds : part.Press.Velocity } switch {
                    var sampled => Tracked(part with { Press = sampled, Time = now }, part.Track.Moved(sampled)),
                },
            },
        };

    private Routed Released(PointerSample sample, LanguageExt.HashSet<Modifier> modifiers) =>
        Routing.Capture.Match(
            Some: capture => capture.Switch(
                (Mount: this with { Routing = Routing with { Capture = None } }, Sample: sample, Modifiers: modifiers),
                part: static (held, part) => (part.Press.Followed(held.Sample, held.Mount.Slop(part.Owner)) with { Modifiers = held.Modifiers }) switch {
                    var press => held.Mount.Answered(part.Owner, part.Track.Released(press)
                        + (part.Track.Carried.IsEmpty
                            ? []
                            : held.Mount.Chain(held.Mount.Hits(held.Sample.At)).Filter(owner => owner.Owner.Path != part.Owner)
                                .Choose(owner => owner.Owner.Part.Landed(part.Track.Carried, held.Mount.Aimed(owner.Owner, owner.Over, held.Mount.Routing.Focus))).Take(1))),
                },
                handle: static (held, handle) => held.Mount.Slid(handle, held.Sample.At)),
            None: () => Unanswered);

    private Routed Tracked(Capture.Part part, Reply reply) =>
        reply.Switch(
            (Mount: this, Part: part),
            pass: static (held, _) => (held.Mount with { Routing = held.Mount.Routing with { Capture = held.Part } }).Answered(held.Part.Owner, []),
            take: static (held, take) => (held.Mount with { Routing = held.Mount.Routing with { Capture = take.Track.Map(track => (Capture)(held.Part with { Track = track })) } })
                .Answered(held.Part.Owner, take.Effects));

    private Routed Slid(Capture.Handle handle, SKPoint at) =>
        Scene.Adjuster(handle.Adjuster).Match(
            Some: adjuster => Changed(Scene.Adjust(adjuster.Owner, _ => handle.Start + (adjuster.Gearing * adjuster.Axis.Of(at - handle.From)))),
            None: () => Unanswered).Taken();

    private Routed Uncaptured() =>
        Routing.Capture.Match(
            Some: capture => new Routed(this with { Routing = Routing with { Capture = None } }, [], Some(capture.Switch(part: static part => part.Track.Cancelled(), handle: static _ => Seq<IO<Unit>>()))),
            None: () => Unanswered);

    // --- [STREAM]
    private Routed Notched(Gesture.Scroll notch) =>
        Sampled(notch, _ => Preceded(Hits(notch.At), notch, (mount, owner, over) => mount.Performed(owner, over, notch), static (mount, scroll, scrollers) =>
            scrollers.Map(adjuster => mount.Changed(mount.Scene.Adjust(adjuster.Owner, value => value - adjuster.Axis.Of(scroll.Travel)))).Find(static moved => moved.Answer.IsSome)
                .IfNone(mount.Unanswered.Taken())));

    private Routed Streamed<TGesture, TIncrement>(SKPoint at, Phase<TIncrement> phase, LanguageExt.HashSet<Modifier> modifiers)
        where TGesture : Gesture, IStreamed<TGesture, TIncrement> where TIncrement : notnull =>
        phase.Switch(
            (Mount: this, At: at, Modifiers: modifiers),
            began: static (held, began) => held.Mount.Sampled(TGesture.Of(held.At, began.Value, held.Modifiers),
                increment => held.Mount.Halted().Then(mount => mount.Latching(mount.Hits(held.At), increment))),
            changed: static (held, changed) => held.Mount.Sampled(TGesture.Of(held.At, changed.Value, held.Modifiers),
                increment => held.Mount.Coupled<TGesture, TIncrement>(held.At, changed.Value, held.Modifiers, increment, static (live, next) => live.Track.Moved(next), static scroll => Some(scroll))),
            ended: static (held, ended) => held.Mount.Sampled(TGesture.Of(held.At, ended.Value, held.Modifiers),
                increment => held.Mount.Coupled<TGesture, TIncrement>(held.At, ended.Value, held.Modifiers, increment, static (live, next) => new Reply.Take(live.Track.Released(next), None), static _ => None)),
            cancelled: static (held, _) => held.Mount.Routing.Capture.IsSome ? held.Mount.Unanswered : held.Mount.Halted());

    private Routed Sampled(Gesture increment, Func<Gesture, Routed> streamed) =>
        Routing.Capture.Match(
            Some: capture => capture.Switch(
                (Mount: this, Increment: increment),
                part: static (held, part) => held.Mount.Wheeled(part, held.Increment),
                handle: static (held, _) => held.Mount.Unanswered),
            None: () => streamed(increment));

    private Routed Wheeled(Capture.Part part, Gesture increment) =>
        Gesture.Scroll.From(increment).Match(
            Some: scroll => (part.Press with { Scrolled = part.Press.Scrolled + scroll.Travel }) switch {
                var press => Tracked(part with { Press = press }, part.Track.Moved(press)),
            },
            None: () => Answered(part.Owner, []));

    private Routed Latching(Seq<Seq<Key>> hits, Gesture increment) =>
        Routing.Latch.Match(
                Some: latch => latch.Switch(
                    (Mount: this, Hits: hits, Increment: increment),
                    part: static (held, part) => held.Mount.Owning(part.Owner).Match(
                        Some: owner => held.Mount.Opened(owner, held.Hits.Find(hit => hit.Take(owner.Path.Count) == owner.Path).IfNone(owner.Path), held.Increment),
                        None: () => held.Mount.Unanswered),
                    scroll: static (held, scroll) => (from slid in Gesture.Scroll.From(held.Increment)
                                                      from adjuster in held.Mount.Scene.Adjuster(scroll.Adjuster)
                                                      where adjuster.Scrolls(slid.Travel)
                                                      select held.Mount.Cored(adjuster, slid)).IfNone(held.Mount.Unanswered)),
                None: () => Unanswered)
            .Otherwise(mount => mount.Preceded(hits, increment, (next, owner, over) => next.Opened(owner, over, increment),
                static (next, scroll, scrollers) => scrollers.Map(adjuster => next.Cored(adjuster, scroll)).Find(static moved => moved.Answer.IsSome).IfNone(next.Unanswered)))
            switch {
                var routed when routed.Answer.IsNone => routed.Routes(static routing => routing with { Latch = None }),
                var routed => routed,
            };

    private Routed Preceded(Seq<Seq<Key>> hits, Gesture gesture, Func<Mount, Owned, Seq<Key>, Routed> part, Func<Mount, Gesture.Scroll, Seq<Adjuster>, Routed> core) =>
        (Chain: Chain(hits), Core: Gesture.Scroll.From(gesture).Map(scroll => (Scroll: scroll, Scrollers: Scrollers(hits, scroll.Travel))), Holding: Focused.Bind(Owner).Map(static owner => owner.Path)) switch {
            var held when gesture.Modifiers.Contains(Modifier.Common) || held.Chain.Head.Exists(owner => held.Holding == Some(owner.Owner.Path)) =>
                Asking(held.Chain, part).Otherwise(mount => mount.Scrolling(held.Core, core)),
            var held => Scrolling(held.Core, core).Otherwise(mount => mount.Asking(held.Chain, part)),
        };

    private Routed Scrolling(Option<(Gesture.Scroll Scroll, Seq<Adjuster> Scrollers)> held, Func<Mount, Gesture.Scroll, Seq<Adjuster>, Routed> core) =>
        held.Filter(static scrolling => !scrolling.Scrollers.IsEmpty).Match(Some: scrolling => core(this, scrolling.Scroll, scrolling.Scrollers), None: () => Unanswered);

    private Routed Asking(Seq<(Owned Owner, Seq<Key> Over)> chain, Func<Mount, Owned, Seq<Key>, Routed> ask) =>
        chain.Fold(Unanswered, (routed, held) => routed.Otherwise(mount => ask(mount, held.Owner, held.Over)));

    private Routed Opened(Owned owner, Seq<Key> over, Gesture increment) =>
        owner.Part.Began(increment, Aimed(owner, over, Routing.Focus)).Switch(
            (Mount: this, Owner: owner, Increment: increment),
            pass: static (held, _) => held.Mount.Unanswered,
            take: static (held, take) => (held.Mount with {
                Routing = held.Mount.Routing with { Latch = new Latch.Part(held.Owner.Path, take.Track.Map(track => new Stream(held.Increment, track))) },
            }).Answered(held.Owner.Path, take.Effects));

    private Routed Cored(Adjuster adjuster, Gesture.Scroll scroll) =>
        Changed(Scene.Adjust(adjuster.Owner, value => value - adjuster.Axis.Of(scroll.Travel))) switch {
            { Answer.IsNone: true } unmoved => unmoved,
            var moved => moved.Routes(routing => routing with { Latch = new Latch.Scroll(adjuster.Owner, Some(scroll)) }),
        };

    private Routed Coupled<TGesture, TIncrement>(SKPoint at, TIncrement value, LanguageExt.HashSet<Modifier> modifiers, Gesture increment, Func<Stream, Gesture, Reply> step,
                                                  Func<Gesture.Scroll, Option<Gesture.Scroll>> kept)
        where TGesture : Gesture, IStreamed<TGesture, TIncrement> where TIncrement : notnull =>
        Routing.Latch.Match(
            Some: latch => latch.Switch(
                (Mount: this, At: at, Value: value, Modifiers: modifiers, Increment: increment, Step: step, Kept: kept),
                part: static (held, part) => (from live in part.Live
                                              from prior in TGesture.From(live.Gesture)
                                              let next = prior.Folded(held.At, held.Value, held.Modifiers)
                                              select held.Mount.Streaming(part.Owner, live, next, held.Step(live, next)))
                    .IfNone(held.Mount.Unanswered),
                scroll: static (held, scroll) => (from live in scroll.Live
                                                  from slid in Gesture.Scroll.From(held.Increment)
                                                  from adjuster in held.Mount.Scene.Adjuster(scroll.Adjuster)
                                                  select held.Mount.Changed(held.Mount.Scene.Adjust(adjuster.Owner, current => current - adjuster.Axis.Of(slid.Travel)))
                                                      .Routes(routing => routing with { Latch = scroll with { Live = held.Kept(live.Folded(slid.At, slid.Travel, slid.Modifiers)) } }))
                    .IfNone(held.Mount.Unanswered)),
            None: () => Unanswered);

    private Routed Streaming(Seq<Key> owner, Stream live, Gesture next, Reply reply) =>
        reply.Switch(
            (Mount: this, Owner: owner, Live: live, Next: next),
            pass: static (held, _) => held.Mount.Unanswered.Routes(routing => routing with { Latch = new Latch.Part(held.Owner, Some(held.Live with { Gesture = held.Next })) }),
            take: static (held, take) => (held.Mount with {
                Routing = held.Mount.Routing with { Latch = new Latch.Part(held.Owner, take.Track.Map(track => new Stream(held.Next, track))) },
            }).Answered(held.Owner, take.Effects));

    private Routed Halted() =>
        Routing.Latch.Match(
            Some: latch => latch.Switch(
                this,
                part: static (mount, part) => part.Live.Match(
                    Some: live => new Routed(mount with { Routing = mount.Routing with { Latch = part with { Live = None } } }, [], Some(live.Track.Cancelled())),
                    None: () => mount.Unanswered),
                scroll: static (mount, scroll) => mount.Unanswered.Routes(routing => routing with { Latch = scroll with { Live = None } })),
            None: () => Unanswered);

    // --- [PERFORM]
    private Routed Targeted(Input.Perform perform) =>
        new Gesture.Perform(perform.Act) { Modifiers = perform.Modifiers } switch {
            var gesture => perform.Target.Switch(
                (Mount: this, Perform: perform, Gesture: gesture),
                focused: static (held, _) => held.Mount.Bubbled(held.Gesture),
                at: static (held, at) => held.Mount.Asking(held.Mount.Chain(held.Mount.Hits(at.Point)), (mount, owner, over) => mount.Performed(owner, over, held.Gesture)),
                element: static (held, element) => held.Mount.Addressed(held.Gesture, element.Id),
                surface: static (held, _) => held.Perform.Act.SwitchPartially(
                    held.Mount,
                    @default: static (mount, _) => mount.Unanswered,
                    next: static (mount, _) => mount.Entered(1),
                    previous: static (mount, _) => mount.Entered(-1))),
        };

    private Routed Addressed(Gesture.Perform gesture, Seq<Key> target) =>
        gesture.Act.SwitchPartially(
                (Mount: this, Target: target),
                @default: static (_, _) => Option<Routed>.None,
                scrollToVisible: static (held, _) => Some(held.Mount.Changed(held.Mount.Scene.Reveal(held.Target))),
                increment: static (held, _) => held.Mount.Adjusting(held.Target, static adjuster => Some(adjuster.Value + adjuster.Step)),
                decrement: static (held, _) => held.Mount.Adjusting(held.Target, static adjuster => Some(adjuster.Value - adjuster.Step)),
                navigate: static (held, navigate) => held.Mount.Adjusting(held.Target, adjuster => adjuster.Navigated(navigate.Toward)))
            .IfNone(() => Replied(gesture, target));

    private Routed Replied(Gesture gesture, Seq<Key> target) => Owner(target).Match(Some: owner => Performed(owner, target, gesture), None: () => Unanswered);

    private Option<Routed> Adjusting(Seq<Key> target, Func<Adjuster, Option<float>> moved) =>
        from adjuster in Scene.Adjuster(target)
        from value in moved(adjuster)
        select Changed(Scene.Adjust(adjuster.Owner, _ => value));

    private Routed Bubbled(Gesture.Perform gesture) =>
        Routing.Focus.Match(
                Some: focus => Asking(Owners(focus.Element).Map(owner => (Owner: owner, Over: focus.Element)), (mount, owner, over) => mount.Performed(owner, over, gesture)),
                None: () => Unanswered)
            .Otherwise(mount => mount.Defaulted(gesture.Act));

    private Routed Defaulted(Act act) =>
        act.SwitchPartially(
            this,
            @default: static (mount, _) => mount.Unanswered,
            navigate: static (mount, navigate) => mount.Focused.Bind(focus => mount.Adjusting(focus, adjuster => adjuster.Navigated(navigate.Toward))).IfNone(() => mount.Roved(navigate.Toward)),
            next: static (mount, _) => mount.Traversed(Routing.Traversed(Routing.Stops(mount.Tree), mount.Focused, 1, mount.Host.Cycles)),
            previous: static (mount, _) => mount.Traversed(Routing.Traversed(Routing.Stops(mount.Tree), mount.Focused, -1, mount.Host.Cycles)));

    private Routed Entered(int sign) => Traversed(Routing.Traversed(Routing.Stops(Tree), None, sign, cycles: true));

    private Routed Roved(Navigation toward) =>
        Traversed(
            from focus in Focused
            from origin in elements.Find(focus)
            from stop in Routing.Stops(Tree).Find(stop => stop.Exists(element => element.Id == focus))
            from target in Routing.Toward(origin.Frame, stop.Filter(element => element.Id != focus).Map(static element => (Item: element.Id, element.Frame)), toward)
            select target);

    private Routed Traversed(Option<Seq<Key>> stop) => stop.Match(Some: element => Refocused(Landing(Some(element), static landed => new Focus.Traversed(landed))).Taken(), None: () => Unanswered);

    private Routed Performed(Owned owner, Seq<Key> over, Gesture gesture) =>
        owner.Part.Performed(gesture, Aimed(owner, over, Routing.Focus)).Switch(
            (Mount: this, Owner: owner),
            pass: static (held, _) => held.Mount.Unanswered,
            take: static (held, take) => held.Mount.Answered(held.Owner.Path, take.Effects));

    // --- [FOCUS]
    private Option<Focus> Landing(Option<Seq<Key>> element, Func<Seq<Key>, Focus> kind) =>
        element.Map(target => Routing.Focus.Filter(focus => focus.Element == target).IfNone(() => kind(target) with { Editor = Owner(target).Bind(static owner => owner.Part.Opened(owner.Laid)) }));

    private Routed Refocused(Option<Focus> next) =>
        next.Map(static focus => focus.Element) == Focused ? Unanswered : (this with { Routing = Routing with { Focus = next } }).Effected(Routing.Focus.Map(Left).IfNone([]));

    private Routed Restored() =>
        (from focus in Routing.Focus
         where focus.Editor.IsNone
         from owner in Owner(focus.Element)
         from editor in owner.Part.Opened(owner.Laid)
         select new Routed(this with { Routing = Routing with { Focus = focus with { Editor = editor } } }, Scene.Bounds(focus.Element).ToSeq(), Some(Seq<IO<Unit>>()))).IfNone(Unanswered);

    private Routed Closed() =>
        Routing.Focus.Filter(static focus => focus.Editor.IsSome).Match(
            Some: focus => new Routed(this with { Routing = Routing with { Focus = focus with { Editor = None } } }, Scene.Bounds(focus.Element).ToSeq(), Some(Left(focus))),
            None: () => Unanswered);

    private Seq<IO<Unit>> Left(Focus focus) =>
        (from editor in focus.Editor
         from owner in Owner(focus.Element)
         select owner.Part.Left(Aimed(owner, focus.Element, Some(focus)))).IfNone([]);

    private Option<Seq<Key>> Element(Seq<Key> owner, Seq<Key> over) => Scene.Above(over).TakeWhile(prefix => prefix.Count >= owner.Count).Find(elements.ContainsKey) | Entry(owner);

    private Option<Seq<Key>> Entry(Seq<Key> stop) =>
        Scene.Index.Find(stop).Bind(laid => (Lead(laid).ToSeq() + Scene.Preorder(laid).Map(static held => held.Path))
            .Find(entry => elements.ContainsKey(entry) && Owner(entry).Exists(owner => owner.Path == stop)));

    // --- [LOOKUP]
    private Seq<Seq<Key>> Hits(SKPoint at) => Scene.Hit(at).Map(static hit => hit.Laid.Path);

    private Seq<(Owned Owner, Seq<Key> Over)> Chain(Seq<Seq<Key>> hits) =>
        toSeq(hits.Choose(hit => Owner(hit).Map(owner => (Owner: owner, Over: hit))).DistinctBy(static held => held.Owner.Path));

    private Option<Owned> Owner(Seq<Key> path) => Owners(path).Head;

    private Seq<Owned> Owners(Seq<Key> path) => Scene.Above(path).Choose(Owning);

    private Option<Owned> Owning(Seq<Key> prefix) => Parted(prefix).Filter(static owner => owner.Enabled);

    private Option<Owned> Parted(Seq<Key> prefix) =>
        from laid in Scene.Index.Find(prefix)
        from part in laid.Node.Part
        from bounds in Scene.Frame(prefix)
        select new Owned(prefix, laid, part, bounds);

    private Aim Aimed(Owned owner, Seq<Key> over, Option<Focus> focus) =>
        new(Scene, owner.Laid, owner.Bounds, over) { Editor = focus.Filter(held => held.Element.Take(owner.Path.Count) == owner.Path).Bind(static held => held.Editor) };

    private float Slop(Seq<Key> owner) => Scene.Index.Find(owner).Map(static laid => laid.Style.Metrics[Metric.Slop]).IfNone(0f);

    private Option<Adjuster> Handle(Seq<Key> path) => Scene.Adjuster(path).Filter(adjuster => adjuster.Owner != path);

    private Seq<Adjuster> Scrollers(Seq<Seq<Key>> hits, Vector2 delta) => hits.Head.Map(Scene.Above).IfNone([]).Choose(Scene.Adjuster).Filter(adjuster => adjuster.Scrolls(delta));

    private Cursor Prospective(Seq<Key> path, SKPoint pointer) =>
        Handle(path).Match(
            Some: static adjuster => adjuster.Cursor,
            None: () => Scene.Above(path).Choose(Parted).Head.Exists(static owner => !owner.Enabled)
                ? Cursor.NotAllowed
                : Owners(path).Choose(owner => owner.Part.Began(new Gesture.Press(pointer, None, 1, 1f) { Modifiers = Routing.Modifiers }, Aimed(owner, path, Routing.Focus))
                        .Switch(pass: static _ => Option<Cursor>.None, take: static take => Some(take.Track.Map(static track => track.Cursor).IfNone(Cursor.Default))))
                    .Head.IfNone(Cursor.Default));

    private Routed Changed((Scene Scene, Seq<SKRect> Damage) next) =>
        ReferenceEquals(next.Scene, Scene) ? Unanswered : new Routed(this with { Scene = next.Scene }, next.Damage, Some(Seq<IO<Unit>>()));

    private LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> Active() =>
        toHashSet(Hover.Head.Filter(path => Scene.Index.Find(path).Exists(static laid => !laid.Style.Conditions.Contains(Condition.Disabled))).Map(static path => (path, Interaction.Hover)).ToSeq()
            .Concat(Routing.Pressing.Map(static path => (path, Interaction.Pressed)).ToSeq())
            .Concat(Routing.Focus.Filter(focus => Host.Visibility == Visibility.Active && focus.Visible(elements)).Map(static focus => (focus.Element, Interaction.Focused)).ToSeq()));

    private Option<(SKRect Damage, Option<SKRect> Beyond, TimingCurve Motion)> Washed((Seq<Key> Path, Interaction Interaction) key) =>
        key.Interaction.Switch(
            (Scene, key.Path),
            hover: Painted,
            pressed: Painted,
            focused: static held => held.Scene.Ring(held.Path).Map(static ring => SKRect.Intersect(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), ring.Clip) switch {
                var reach => (reach, Some(reach), ring.Style.Motion),
            }));

    private static Option<(SKRect Damage, Option<SKRect> Beyond, TimingCurve Motion)> Painted((Scene Scene, Seq<Key> Path) held) =>
        from laid in held.Scene.Index.Find(held.Path)
        from bounds in held.Scene.Bounds(held.Path)
        select (bounds, Option<SKRect>.None, laid.Style.Motion);

    private static Option<Seq<Key>> Lead(Laid laid) =>
        from part in laid.Node.Part
        from lead in part.Revealed(laid)
        select lead;
}
