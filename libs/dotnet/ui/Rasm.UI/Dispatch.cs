using System.Numerics;
using Rasm.UI.Rendering;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Routed(Mount Mount, Seq<SKRect> Damage, Option<Seq<IO<Unit>>> Answer) {
    internal Routed Then(Func<Mount, Routed> next) {
        Routed after = next(Mount);
        return new(after.Mount, Damage.Concat(after.Damage), Answer.Match(Some: effects => Some(effects + after.Answer.IfNone([])), None: () => after.Answer));
    }

    internal Routed Otherwise(Func<Mount, Routed> next) => Answer.IsSome ? this : Then(next);

    internal Routed Routes(Func<Routing, Routing> change) => this with { Mount = Mount with { Routing = change(Mount.Routing) } };

    internal Routed Taken() => this with { Answer = Some(Answer.IfNone([])) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Dispatch {
    private sealed record Scrolls(Gesture.Scroll Scroll, Seq<Adjuster> Scrollers);

    extension(Mount mount) {
        // --- [DISPATCH]
        internal Routed Dispatched(Input input, Duration now) =>
            input.Switch(
                (Mount: mount, Now: now),
                pointer: static (held, pointer) => pointer.Stroke.Value.Match(
                    Some: sample => pointer.Stroke.Phase switch {
                        { Opens: true } => held.Mount.Uncaptured().Then(next => next.Pointed(Some(sample.At), pointer.Modifiers)).Then(next => next.Pressed(sample, pointer, held.Now)),
                        { Settles: true } => held.Mount.Pointed(Some(sample.At), pointer.Modifiers).Then(next => next.Released(sample, pointer.Modifiers)),
                        _ => held.Mount.Pointed(Some(sample.At), pointer.Modifiers).Then(next => next.Moved(sample, pointer.Modifiers, held.Now)),
                    },
                    None: () => held.Mount.Cancel()),
                hover: static (held, hover) => held.Mount.Pointed(hover.At, hover.Modifiers),
                wheel: static (held, wheel) => held.Mount.Pointed(Some(wheel.At), wheel.Modifiers).Then(mount => mount.Notched(new Gesture.Scroll(wheel.At, wheel.Delta) { Modifiers = wheel.Modifiers })),
                scroll: static (held, scroll) => held.Mount.Pointed(Some(scroll.At), scroll.Modifiers).Then(mount => mount.Streamed<Gesture.Scroll, Vector2>(scroll.At, scroll.Delta, scroll.Modifiers)),
                magnify: static (held, magnify) => held.Mount.Pointed(Some(magnify.At), magnify.Modifiers).Then(mount => mount.Streamed<Gesture.Magnify, float>(magnify.At, magnify.Amount, magnify.Modifiers)),
                rotate: static (held, rotate) => held.Mount.Pointed(Some(rotate.At), rotate.Modifiers).Then(mount => mount.Streamed<Gesture.Rotate, float>(rotate.At, rotate.Radians, rotate.Modifiers)),
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    (held.Mount, Perform: perform),
                    @default: static (state, _) => state.Mount.Targeted(state.Perform),
                    cancel: static (state, _) => state.Mount.Routing.Capture.IsSome ? state.Mount.Uncaptured() : state.Mount.Targeted(state.Perform)),
                focused: static (held, _) => held.Mount.Unanswered,
                blurred: static (held, _) => held.Mount.Cancel());

        internal Routed Cancel() => mount.Uncaptured().Then(static next => next.Halted());

        private Routed Pointed(Option<SKPoint> at, KeyModifiers modifiers) =>
            mount.Unanswered.Routes(routing => routing with {
                Pointer = at,
                Modifiers = modifiers,
                Latch = at == routing.Pointer ? routing.Latch : routing.Latch.Filter(static latch => latch.Switch(part: static part => part.Live.IsSome, scroll: static scroll => scroll.Live.IsSome)),
            });

        private Routed Changed(Rearranged next) =>
            ReferenceEquals(next.Scene, mount.Scene) ? mount.Unanswered : new Routed(mount with { Scene = next.Scene }, next.Damage, Some(Seq<IO<Unit>>()));

        // --- [POINTER]
        private Routed Pressed(PointerSample sample, Input.Pointer pointer, Duration now) {
            Seq<Seq<Key>> hits = mount.Hits(sample.At);
            return (from top in hits.Head from adjuster in mount.Handle(top) select (Handle: top, adjuster.Value)).Match(
                Some: held => mount.Unanswered.Routes(routing => routing with { Capture = new PointerCapture.Handle(held.Handle, sample.At, held.Value) }).Taken(),
                None: () => {
                    Gesture.Press press = new(sample.At, None, pointer.Clicks, sample.Pressure) { Modifiers = pointer.Modifiers };
                    return mount.Asking(mount.Scene.Chain(hits), (next, owner, over) => next.Asked(owner, over, press, now));
                });
        }

        private Routed Asked(Owned owner, Seq<Key> over, Gesture.Press press, Duration now) =>
            owner.Part.Began(press, mount.Scene.Aimed(owner, over)).Switch(
                (Mount: mount, Owner: owner, Over: over, Press: press, Now: now, Focusing: mount.Focusing(mount.Element(owner.Path, over), traversed: false)),
                pass: static (held, _) => held.Mount.Unanswered,
                take: static (held, take) => held.Mount.Refocused(held.Focusing).Then(next =>
                    (next with { Routing = next.Routing with { Capture = take.Track.Map(track => (PointerCapture)new PointerCapture.Part(held.Owner.Path, held.Over, held.Press, track, held.Now)) } })
                    .Answered(take.Effects)));

        private Routed Moved(PointerSample sample, KeyModifiers modifiers, Duration now) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount, Sample: sample, Modifiers: modifiers, Now: now),
                    part: static (held, part) => held.Mount.Dragged(part, held.Sample, held.Modifiers, held.Now),
                    handle: static (held, handle) => held.Mount.Slid(handle, held.Sample.At)),
                None: () => mount.Unanswered);

        private Routed Dragged(PointerCapture.Part part, PointerSample sample, KeyModifiers modifiers, Duration now) {
            Gesture.Press press = part.Press.Followed(sample, mount.Scene.Slop(part.Owner)) with { Modifiers = modifiers };
            if (press == part.Press) return mount.Answered([]);
            float seconds = (float)(now - part.Time).TotalSeconds;
            Gesture.Press sampled = press with { Velocity = seconds > 0f ? (Vector2)(sample.At - part.Press.At) / seconds : part.Press.Velocity };
            return mount.Tracked(part with { Press = sampled, Time = now }, part.Track.Moved(sampled));
        }

        private Routed Released(PointerSample sample, KeyModifiers modifiers) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount with { Routing = mount.Routing with { Capture = None } }, Sample: sample, Modifiers: modifiers),
                    part: static (held, part) => held.Mount.Answered(part.Track.Released(part.Press.Followed(held.Sample, held.Mount.Scene.Slop(part.Owner)) with { Modifiers = held.Modifiers })
                        + (part.Track.Dragged.IsEmpty
                            ? []
                            : held.Mount.Scene.Chain(held.Mount.Hits(held.Sample.At)).Filter(owner => owner.Owner.Path != part.Owner)
                                .Choose(owner => owner.Owner.Part.Drop(part.Track.Dragged, held.Mount.Scene.Aimed(owner.Owner, owner.Over))).Take(1))),
                    handle: static (held, handle) => held.Mount.Slid(handle, held.Sample.At)),
                None: () => mount.Unanswered);

        private Routed Tracked(PointerCapture.Part part, Reply reply) =>
            reply.Switch(
                (Mount: mount, Part: part),
                pass: static (held, _) => (held.Mount with { Routing = held.Mount.Routing with { Capture = held.Part } }).Answered([]),
                take: static (held, take) => (held.Mount with { Routing = held.Mount.Routing with { Capture = take.Track.Map(track => (PointerCapture)(held.Part with { Track = track })) } })
                    .Answered(take.Effects));

        private Routed Slid(PointerCapture.Handle handle, SKPoint at) =>
            mount.Scene.Adjuster(handle.Adjuster).Match(
                Some: adjuster => mount.Changed(mount.Scene.Adjust(adjuster.Owner, _ => handle.Start + (adjuster.Gearing * adjuster.Axis.Of(at - handle.From)))),
                None: () => mount.Unanswered).Taken();

        private Routed Uncaptured() =>
            mount.Routing.Capture.Match(
                Some: capture => new Routed(mount with { Routing = mount.Routing with { Capture = None } }, [], Some(capture.Switch(part: static part => part.Track.Cancelled(), handle: static _ => Seq<IO<Unit>>()))),
                None: () => mount.Unanswered);

        // --- [STREAM]
        private Routed Notched(Gesture.Scroll notch) =>
            mount.Sampled(notch, _ => mount.Preceded(mount.Hits(notch.At), notch, (next, owner, over) => next.Performed(owner, over, notch), static (next, scroll, scrollers) =>
                scrollers.Map(adjuster => next.Changed(next.Scene.Adjust(adjuster.Owner, value => value - adjuster.Axis.Of(scroll.Travel)))).Find(static moved => moved.Answer.IsSome)
                    .IfNone(next.Unanswered.Taken())));

        private Routed Streamed<TGesture, TIncrement>(SKPoint at, Edit<TIncrement> edit, KeyModifiers modifiers)
            where TGesture : Gesture, IStreamed<TGesture, TIncrement> where TIncrement : notnull =>
            edit.Value.Match(
                Some: value => mount.Sampled(TGesture.Of(at, value, modifiers), increment => edit.Phase switch {
                    { Opens: true } => mount.Halted().Then(next => next.Latching(next.Hits(at), increment)),
                    { Settles: true } => mount.Coupled<TGesture, TIncrement>(at, value, modifiers, increment, static (live, next) => new Reply.Take(live.Track.Released(next), None), static _ => None),
                    _ => mount.Coupled<TGesture, TIncrement>(at, value, modifiers, increment, static (live, next) => live.Track.Moved(next), static scroll => Some(scroll)),
                }),
                None: () => mount.Routing.Capture.IsSome ? mount.Unanswered : mount.Halted());

        private Routed Sampled(Gesture increment, Func<Gesture, Routed> streamed) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount, Increment: increment),
                    part: static (held, part) => held.Mount.Wheeled(part, held.Increment),
                    handle: static (held, _) => held.Mount.Unanswered),
                None: () => streamed(increment));

        private Routed Wheeled(PointerCapture.Part part, Gesture increment) =>
            Optional(increment as Gesture.Scroll).Match(
                Some: scroll => {
                    Gesture.Press press = part.Press with { Scrolled = part.Press.Scrolled + scroll.Travel };
                    return mount.Tracked(part with { Press = press }, part.Track.Moved(press));
                },
                None: () => mount.Answered([]));

        private Routed Latching(Seq<Seq<Key>> hits, Gesture increment) =>
            mount.Routing.Latch.Match(
                    Some: latch => latch.Switch(
                        (Mount: mount, Hits: hits, Increment: increment),
                        part: static (held, part) => held.Mount.Scene.Owning(part.Owner).Match(
                            Some: owner => held.Mount.Opened(owner, held.Hits.Find(hit => hit.Take(owner.Path.Count) == owner.Path).IfNone(owner.Path), held.Increment),
                            None: () => held.Mount.Unanswered),
                        scroll: static (held, scroll) => (from slid in Optional(held.Increment as Gesture.Scroll)
                                                          from adjuster in held.Mount.Scene.Adjuster(scroll.Adjuster)
                                                          where adjuster.Scrolls(slid.Travel)
                                                          select held.Mount.Cored(adjuster, slid)).IfNone(held.Mount.Unanswered)),
                    None: () => mount.Unanswered)
                .Otherwise(next => next.Preceded(hits, increment, (held, owner, over) => held.Opened(owner, over, increment),
                    static (held, scroll, scrollers) => scrollers.Map(adjuster => held.Cored(adjuster, scroll)).Find(static moved => moved.Answer.IsSome).IfNone(held.Unanswered)))
                switch {
                    var routed when routed.Answer.IsNone => routed.Routes(static routing => routing with { Latch = None }),
                    var routed => routed,
                };

        private Routed Preceded(Seq<Seq<Key>> hits, Gesture gesture, Func<Mount, Owned, Seq<Key>, Routed> part, Func<Mount, Gesture.Scroll, Seq<Adjuster>, Routed> core) =>
            (Chain: mount.Scene.Chain(hits), Core: Optional(gesture as Gesture.Scroll).Map(scroll => new Scrolls(scroll, mount.Scene.Scrollers(hits, scroll.Travel))), Holding: mount.Focused.Bind(mount.Scene.Owner).Map(static owner => owner.Path)) switch {
                var held when gesture.Modifiers.Common || held.Chain.Head.Exists(owner => held.Holding == Some(owner.Owner.Path)) =>
                    mount.Asking(held.Chain, part).Otherwise(next => next.Scrolling(held.Core, core)),
                var held => mount.Scrolling(held.Core, core).Otherwise(next => next.Asking(held.Chain, part)),
            };

        private Routed Scrolling(Option<Scrolls> held, Func<Mount, Gesture.Scroll, Seq<Adjuster>, Routed> core) =>
            held.Filter(static scrolling => !scrolling.Scrollers.IsEmpty).Match(Some: scrolling => core(mount, scrolling.Scroll, scrolling.Scrollers), None: () => mount.Unanswered);

        private Routed Asking(Seq<Claim> chain, Func<Mount, Owned, Seq<Key>, Routed> ask) =>
            chain.Fold(mount.Unanswered, (routed, held) => routed.Otherwise(next => ask(next, held.Owner, held.Over)));

        private Routed Opened(Owned owner, Seq<Key> over, Gesture increment) =>
            owner.Part.Began(increment, mount.Scene.Aimed(owner, over)).Switch(
                (Mount: mount, Owner: owner, Increment: increment),
                pass: static (held, _) => held.Mount.Unanswered,
                take: static (held, take) => (held.Mount with {
                    Routing = held.Mount.Routing with { Latch = new Latch.Part(held.Owner.Path, take.Track.Map(track => new LiveGesture(held.Increment, track))) },
                }).Answered(take.Effects));

        private Routed Cored(Adjuster adjuster, Gesture.Scroll scroll) =>
            mount.Changed(mount.Scene.Adjust(adjuster.Owner, value => value - adjuster.Axis.Of(scroll.Travel))) switch {
                { Answer.IsNone: true } unmoved => unmoved,
                var moved => moved.Routes(routing => routing with { Latch = new Latch.Scroll(adjuster.Owner, Some(scroll)) }),
            };

        private Routed Coupled<TGesture, TIncrement>(SKPoint at, TIncrement value, KeyModifiers modifiers, Gesture increment, Func<LiveGesture, Gesture, Reply> step,
                                                      Func<Gesture.Scroll, Option<Gesture.Scroll>> kept)
            where TGesture : Gesture, IStreamed<TGesture, TIncrement> where TIncrement : notnull =>
            mount.Routing.Latch.Match(
                Some: latch => latch.Switch(
                    (Mount: mount, At: at, Value: value, Modifiers: modifiers, Increment: increment, Step: step, Kept: kept),
                    part: static (held, part) => (from live in part.Live
                                                  from prior in Optional(live.Gesture as TGesture)
                                                  let next = prior.Folded(held.At, held.Value, held.Modifiers)
                                                  select held.Mount.Streaming(part.Owner, live, next, held.Step(live, next)))
                        .IfNone(held.Mount.Unanswered),
                    scroll: static (held, scroll) => (from live in scroll.Live
                                                      from slid in Optional(held.Increment as Gesture.Scroll)
                                                      from adjuster in held.Mount.Scene.Adjuster(scroll.Adjuster)
                                                      select held.Mount.Changed(held.Mount.Scene.Adjust(adjuster.Owner, current => current - adjuster.Axis.Of(slid.Travel)))
                                                          .Routes(routing => routing with { Latch = scroll with { Live = held.Kept(((IStreamed<Gesture.Scroll, Vector2>)live).Folded(slid.At, slid.Travel, slid.Modifiers)) } }))
                        .IfNone(held.Mount.Unanswered)),
                None: () => mount.Unanswered);

        private Routed Streaming(Seq<Key> owner, LiveGesture live, Gesture next, Reply reply) =>
            reply.Switch(
                (Mount: mount, Owner: owner, Live: live, Next: next),
                pass: static (held, _) => held.Mount.Unanswered.Routes(routing => routing with { Latch = new Latch.Part(held.Owner, Some(held.Live with { Gesture = held.Next })) }),
                take: static (held, take) => (held.Mount with {
                    Routing = held.Mount.Routing with { Latch = new Latch.Part(held.Owner, take.Track.Map(track => new LiveGesture(held.Next, track))) },
                }).Answered(take.Effects));

        private Routed Halted() =>
            mount.Routing.Latch.Match(
                Some: latch => latch.Switch(
                    mount,
                    part: static (held, part) => part.Live.Match(
                        Some: live => new Routed(held with { Routing = held.Routing with { Latch = part with { Live = None } } }, [], Some(live.Track.Cancelled())),
                        None: () => held.Unanswered),
                    scroll: static (held, scroll) => held.Unanswered.Routes(routing => routing with { Latch = scroll with { Live = None } })),
                None: () => mount.Unanswered);

        // --- [PERFORM]
        private Routed Targeted(Input.Perform perform) =>
            perform.Target.Switch(
                (Mount: mount, Perform: perform, Gesture: new Gesture.Perform(perform.Act) { Modifiers = perform.Modifiers }),
                focused: static (held, _) => held.Mount.Bubbled(held.Gesture),
                at: static (held, at) => held.Mount.Asking(held.Mount.Scene.Chain(held.Mount.Hits(at.Point)), (next, owner, over) => next.Performed(owner, over, held.Gesture)),
                element: static (held, element) => held.Mount.Addressed(held.Gesture, element.Id),
                surface: static (held, _) => held.Perform.Act.SwitchPartially(
                    held.Mount,
                    @default: static (next, _) => next.Unanswered,
                    next: static (next, _) => next.Entered(1),
                    previous: static (next, _) => next.Entered(-1)));

        private Routed Addressed(Gesture.Perform gesture, Seq<Key> target) =>
            gesture.Act.SwitchPartially(
                    (Mount: mount, Target: target),
                    @default: static (_, _) => Option<Routed>.None,
                    scrollToVisible: static (held, _) => Some(held.Mount.Changed(held.Mount.Scene.Reveal(held.Target))),
                    increment: static (held, _) => held.Mount.Adjusting(held.Target, static adjuster => Some(adjuster.Value + adjuster.Step)),
                    decrement: static (held, _) => held.Mount.Adjusting(held.Target, static adjuster => Some(adjuster.Value - adjuster.Step)),
                    navigate: static (held, navigate) => held.Mount.Adjusting(held.Target, adjuster => adjuster.Navigated(navigate.Toward)))
                .IfNone(() => mount.Replied(gesture, target));

        private Routed Replied(Gesture gesture, Seq<Key> target) => mount.Scene.Owner(target).Match(Some: owner => mount.Performed(owner, target, gesture), None: () => mount.Unanswered);

        private Option<Routed> Adjusting(Seq<Key> target, Func<Adjuster, Option<float>> moved) =>
            from adjuster in mount.Scene.Adjuster(target)
            from value in moved(adjuster)
            select mount.Changed(mount.Scene.Adjust(adjuster.Owner, _ => value));

        private Routed Bubbled(Gesture.Perform gesture) =>
            mount.Routing.Focus.Match(
                    Some: focus => mount.Asking(mount.Scene.Owners(focus.Element).Map(owner => new Claim(owner, focus.Element)), (next, owner, over) => next.Performed(owner, over, gesture)),
                    None: () => mount.Unanswered)
                .Otherwise(next => next.Defaulted(gesture.Act));

        private Routed Defaulted(Act act) =>
            act.SwitchPartially(
                mount,
                @default: static (held, _) => held.Unanswered,
                navigate: static (held, navigate) => held.Focused.Bind(focus => held.Adjusting(focus, adjuster => adjuster.Navigated(navigate.Toward))).IfNone(() => held.Roved(navigate.Toward)),
                next: static (held, _) => held.Traversed(Routing.Traversed(Routing.Stops(held.Tree), held.Focused, 1, held.Host.Placement.Cycles)),
                previous: static (held, _) => held.Traversed(Routing.Traversed(Routing.Stops(held.Tree), held.Focused, -1, held.Host.Placement.Cycles)));

        private Routed Entered(int sign) => mount.Traversed(Routing.Traversed(Routing.Stops(mount.Tree), None, sign, cycles: true));

        private Routed Roved(NavigationDirection toward) =>
            mount.Traversed(
                from focus in mount.Focused
                from origin in mount.Elements.Find(focus)
                from stop in Routing.Stops(mount.Tree).Find(stop => stop.Exists(element => element.Id == focus))
                from target in Routing.Toward(origin.Frame, stop.Filter(element => element.Id != focus).Map(static element => new Candidate<Seq<Key>>(element.Id, element.Frame)), toward)
                select target);

        private Routed Traversed(Option<Seq<Key>> stop) => stop.Match(Some: element => mount.Refocused(mount.Focusing(Some(element), traversed: true)).Taken(), None: () => mount.Unanswered);

        private Routed Performed(Owned owner, Seq<Key> over, Gesture gesture) =>
            owner.Part.Performed(gesture, mount.Scene.Aimed(owner, over)).Switch(
                mount,
                pass: static (held, _) => held.Unanswered,
                take: static (held, take) => held.Answered(take.Effects));
    }
}
