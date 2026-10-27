using System.Numerics;
using Rasm.UI.Rendering;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Routed(Mount Mount, Seq<SKRect> Damage, Option<Seq<IO<Unit>>> Effects) {
    internal Routed Then(Func<Mount, Routed> next) {
        Routed after = next(Mount);
        return new(after.Mount, Damage.Concat(after.Damage), Effects.Match(Some: effects => Some(effects + after.Effects.IfNone([])), None: () => after.Effects));
    }

    internal Routed Otherwise(Func<Mount, Routed> next) => Effects.IsSome ? this : Then(next);

    internal Routed WithRouting(Func<Routing, Routing> change) => this with { Mount = Mount with { Routing = change(Mount.Routing) } };

    internal Routed Handled() => this with { Effects = Some(Effects.IfNone([])) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Dispatch {
    private sealed record Scrolls(Gesture.Scroll Scroll, Seq<Adjustment> Scrollers);

    extension(Mount mount) {
        // --- [DISPATCH]
        internal Routed Dispatched(Input input, Duration now) =>
            input.Switch(
                (Mount: mount, Now: now),
                pointer: static (held, pointer) => pointer.Stroke.Value.Match(
                    Some: sample => pointer.Stroke.Phase switch {
                        { Begins: true } => held.Mount.Uncaptured().Then(next => next.Pointed(Some(sample.At), pointer.Modifiers)).Then(next => next.Pressed(sample, pointer, held.Now)),
                        { Applies: true } => held.Mount.Pointed(Some(sample.At), pointer.Modifiers).Then(next => next.Released(sample, pointer.Modifiers)),
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
                focused: static (held, _) => held.Mount.Unhandled,
                blurred: static (held, _) => held.Mount.Cancel());

        internal Routed Cancel() => mount.Uncaptured().Then(static next => next.Unlatched());

        private Routed Pointed(Option<SKPoint> at, KeyModifiers modifiers) =>
            mount.Unhandled.WithRouting(routing => routing with {
                Pointer = at,
                Modifiers = modifiers,
                Latch = at == routing.Pointer ? routing.Latch : routing.Latch.Filter(static latch => latch.Switch(part: static part => part.Active.IsSome, scroll: static scroll => scroll.Active.IsSome)),
            });

        private Routed Changed(SceneChange next) =>
            ReferenceEquals(next.Scene, mount.Scene) ? mount.Unhandled : new Routed(mount with { Scene = next.Scene }, next.Damage, Some(Seq<IO<Unit>>()));

        // --- [POINTER]
        private Routed Pressed(PointerSample sample, Input.Pointer pointer, Duration now) {
            Seq<Seq<Key>> hits = mount.Hits(sample.At);
            return (from top in hits.Head from adjuster in mount.Handle(top) select (Handle: top, adjuster.Value)).Match(
                Some: held => mount.Unhandled.WithRouting(routing => routing with { Capture = new PointerCapture.Handle(held.Handle, sample.At, held.Value) }).Handled(),
                None: () => {
                    Gesture.Press press = new(sample.At, None, pointer.Clicks, sample.Pressure) { Modifiers = pointer.Modifiers };
                    return mount.FirstHandled(mount.Scene.Chain(hits), (next, owner, over) => next.PressBegan(owner, over, press, now));
                });
        }

        private Routed PressBegan(PartOwner owner, Seq<Key> over, Gesture.Press press, Duration now) =>
            owner.Part.Began(press, mount.Scene.HitTarget(owner, over)).Switch(
                (Mount: mount, Owner: owner, Over: over, Press: press, Now: now, Focusing: mount.Focusing(mount.Element(owner.Path, over), traversed: false)),
                ignored: static (held, _) => held.Mount.Unhandled,
                accepted: static (held, accepted) => held.Mount.Refocused(held.Focusing).Then(next =>
                    (next with { Routing = next.Routing with { Capture = accepted.Track.Map(track => (PointerCapture)new PointerCapture.Part(held.Owner.Path, held.Over, held.Press, track, held.Now)) } })
                    .Handled(accepted.Effects)));

        private Routed Moved(PointerSample sample, KeyModifiers modifiers, Duration now) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount, Sample: sample, Modifiers: modifiers, Now: now),
                    part: static (held, part) => held.Mount.Dragged(part, held.Sample, held.Modifiers, held.Now),
                    handle: static (held, handle) => held.Mount.ThumbDragged(handle, held.Sample.At)),
                None: () => mount.Unhandled);

        private Routed Dragged(PointerCapture.Part part, PointerSample sample, KeyModifiers modifiers, Duration now) {
            Gesture.Press press = part.Press.Updated(sample, mount.Scene.Slop(part.Owner)) with { Modifiers = modifiers };
            if (press == part.Press) return mount.Handled([]);
            float seconds = (float)(now - part.Time).TotalSeconds;
            Gesture.Press sampled = press with { Velocity = seconds > 0f ? (Vector2)(sample.At - part.Press.At) / seconds : part.Press.Velocity };
            return mount.Tracked(part with { Press = sampled, Time = now }, part.Track.Moved(sampled));
        }

        private Routed Released(PointerSample sample, KeyModifiers modifiers) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount with { Routing = mount.Routing with { Capture = None } }, Sample: sample, Modifiers: modifiers),
                    part: static (held, part) => held.Mount.Handled(part.Track.Released(part.Press.Updated(held.Sample, held.Mount.Scene.Slop(part.Owner)) with { Modifiers = held.Modifiers })
                        + (part.Track.Dragged.IsEmpty
                            ? []
                            : held.Mount.Scene.Chain(held.Mount.Hits(held.Sample.At)).Filter(owner => owner.Owner.Path != part.Owner)
                                .Choose(owner => owner.Owner.Part.Drop(part.Track.Dragged, held.Mount.Scene.HitTarget(owner.Owner, owner.Hit))).Take(1))),
                    handle: static (held, handle) => held.Mount.ThumbDragged(handle, held.Sample.At)),
                None: () => mount.Unhandled);

        private Routed Tracked(PointerCapture.Part part, Reply reply) =>
            reply.Switch(
                (Mount: mount, Part: part),
                ignored: static (held, _) => (held.Mount with { Routing = held.Mount.Routing with { Capture = held.Part } }).Handled([]),
                accepted: static (held, accepted) => (held.Mount with { Routing = held.Mount.Routing with { Capture = accepted.Track.Map(track => (PointerCapture)(held.Part with { Track = track })) } })
                    .Handled(accepted.Effects));

        private Routed ThumbDragged(PointerCapture.Handle handle, SKPoint at) =>
            mount.Scene.Adjustment(handle.Adjuster).Match(
                Some: adjuster => mount.Changed(mount.Scene.Adjust(adjuster.Owner, _ => handle.Start + (adjuster.Gearing * adjuster.Axis.Of(at - handle.From)))),
                None: () => mount.Unhandled).Handled();

        private Routed Uncaptured() =>
            mount.Routing.Capture.Match(
                Some: capture => new Routed(mount with { Routing = mount.Routing with { Capture = None } }, [], Some(capture.Switch(part: static part => part.Track.Cancelled(), handle: static _ => Seq<IO<Unit>>()))),
                None: () => mount.Unhandled);

        // --- [STREAM]
        private Routed Notched(Gesture.Scroll notch) =>
            mount.Sampled(notch, _ => mount.Prioritized(mount.Hits(notch.At), notch, (next, owner, over) => next.Performed(owner, over, notch), static (next, scroll, scrollers) =>
                scrollers.Map(adjuster => next.Changed(next.Scene.Adjust(adjuster.Owner, value => value - adjuster.Axis.Of(scroll.Delta)))).Find(static moved => moved.Effects.IsSome)
                    .IfNone(next.Unhandled.Handled())));

        private Routed Streamed<TGesture, TIncrement>(SKPoint at, Edit<TIncrement> edit, KeyModifiers modifiers)
            where TGesture : Gesture, IStreamed<TGesture, TIncrement> where TIncrement : notnull =>
            edit.Value.Match(
                Some: value => mount.Sampled(TGesture.Of(at, value, modifiers), increment => edit.Phase switch {
                    { Begins: true } => mount.Unlatched().Then(next => next.Latching(next.Hits(at), increment)),
                    { Applies: true } => mount.Continued<TGesture, TIncrement>(at, value, modifiers, increment, static (live, next) => new Reply.Accepted(live.Track.Released(next), None), static _ => None),
                    _ => mount.Continued<TGesture, TIncrement>(at, value, modifiers, increment, static (live, next) => live.Track.Moved(next), static scroll => Some(scroll)),
                }),
                None: () => mount.Routing.Capture.IsSome ? mount.Unhandled : mount.Unlatched());

        private Routed Sampled(Gesture increment, Func<Gesture, Routed> streamed) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount, Increment: increment),
                    part: static (held, part) => held.Mount.Wheeled(part, held.Increment),
                    handle: static (held, _) => held.Mount.Unhandled),
                None: () => streamed(increment));

        private Routed Wheeled(PointerCapture.Part part, Gesture increment) =>
            Optional(increment as Gesture.Scroll).Match(
                Some: scroll => {
                    Gesture.Press press = part.Press with { Scrolled = part.Press.Scrolled + scroll.Delta };
                    return mount.Tracked(part with { Press = press }, part.Track.Moved(press));
                },
                None: () => mount.Handled([]));

        private Routed Latching(Seq<Seq<Key>> hits, Gesture increment) =>
            mount.Routing.Latch.Match(
                    Some: latch => latch.Switch(
                        (Mount: mount, Hits: hits, Increment: increment),
                        part: static (held, part) => held.Mount.Scene.EnabledPartAt(part.Owner).Match(
                            Some: owner => held.Mount.Opened(owner, held.Hits.Find(hit => hit.Take(owner.Path.Count) == owner.Path).IfNone(owner.Path), held.Increment),
                            None: () => held.Mount.Unhandled),
                        scroll: static (held, scroll) => (from slid in Optional(held.Increment as Gesture.Scroll)
                                                          from adjuster in held.Mount.Scene.Adjustment(scroll.Adjuster)
                                                          where adjuster.Scrolls(slid.Delta)
                                                          select held.Mount.Scrolled(adjuster, slid)).IfNone(held.Mount.Unhandled)),
                    None: () => mount.Unhandled)
                .Otherwise(next => next.Prioritized(hits, increment, (held, owner, over) => held.Opened(owner, over, increment),
                    static (held, scroll, scrollers) => scrollers.Map(adjuster => held.Scrolled(adjuster, scroll)).Find(static moved => moved.Effects.IsSome).IfNone(held.Unhandled)))
                switch {
                    var routed when routed.Effects.IsNone => routed.WithRouting(static routing => routing with { Latch = None }),
                    var routed => routed,
                };

        private Routed Prioritized(Seq<Seq<Key>> hits, Gesture gesture, Func<Mount, PartOwner, Seq<Key>, Routed> part, Func<Mount, Gesture.Scroll, Seq<Adjustment>, Routed> core) =>
            (Chain: mount.Scene.Chain(hits), Core: Optional(gesture as Gesture.Scroll).Map(scroll => new Scrolls(scroll, mount.Scene.Scrollers(hits, scroll.Delta))), Holding: mount.Focused.Bind(mount.Scene.Owner).Map(static owner => owner.Path)) switch {
                var held when gesture.Modifiers.Primary || held.Chain.Head.Exists(owner => held.Holding == Some(owner.Owner.Path)) =>
                    mount.FirstHandled(held.Chain, part).Otherwise(next => next.Scrolling(held.Core, core)),
                var held => mount.Scrolling(held.Core, core).Otherwise(next => next.FirstHandled(held.Chain, part)),
            };

        private Routed Scrolling(Option<Scrolls> held, Func<Mount, Gesture.Scroll, Seq<Adjustment>, Routed> core) =>
            held.Filter(static scrolling => !scrolling.Scrollers.IsEmpty).Match(Some: scrolling => core(mount, scrolling.Scroll, scrolling.Scrollers), None: () => mount.Unhandled);

        private Routed FirstHandled(Seq<PartHit> chain, Func<Mount, PartOwner, Seq<Key>, Routed> ask) =>
            chain.Fold(mount.Unhandled, (routed, held) => routed.Otherwise(next => ask(next, held.Owner, held.Hit)));

        private Routed Opened(PartOwner owner, Seq<Key> over, Gesture increment) =>
            owner.Part.Began(increment, mount.Scene.HitTarget(owner, over)).Switch(
                (Mount: mount, Owner: owner, Increment: increment),
                ignored: static (held, _) => held.Mount.Unhandled,
                accepted: static (held, accepted) => (held.Mount with {
                    Routing = held.Mount.Routing with { Latch = new Latch.Part(held.Owner.Path, accepted.Track.Map(track => new ActiveGesture(held.Increment, track))) },
                }).Handled(accepted.Effects));

        private Routed Scrolled(Adjustment adjuster, Gesture.Scroll scroll) =>
            mount.Changed(mount.Scene.Adjust(adjuster.Owner, value => value - adjuster.Axis.Of(scroll.Delta))) switch {
                { Effects.IsNone: true } unmoved => unmoved,
                var moved => moved.WithRouting(routing => routing with { Latch = new Latch.Scroll(adjuster.Owner, Some(scroll)) }),
            };

        private Routed Continued<TGesture, TIncrement>(SKPoint at, TIncrement value, KeyModifiers modifiers, Gesture increment, Func<ActiveGesture, Gesture, Reply> step,
                                                      Func<Gesture.Scroll, Option<Gesture.Scroll>> kept)
            where TGesture : Gesture, IStreamed<TGesture, TIncrement> where TIncrement : notnull =>
            mount.Routing.Latch.Match(
                Some: latch => latch.Switch(
                    (Mount: mount, At: at, Value: value, Modifiers: modifiers, Increment: increment, Step: step, Kept: kept),
                    part: static (held, part) => (from live in part.Active
                                                  from prior in Optional(live.Gesture as TGesture)
                                                  let next = prior.Accumulated(held.At, held.Value, held.Modifiers)
                                                  select held.Mount.Streaming(part.Owner, live, next, held.Step(live, next)))
                        .IfNone(held.Mount.Unhandled),
                    scroll: static (held, scroll) => (from live in scroll.Active
                                                      from slid in Optional(held.Increment as Gesture.Scroll)
                                                      from adjuster in held.Mount.Scene.Adjustment(scroll.Adjuster)
                                                      select held.Mount.Changed(held.Mount.Scene.Adjust(adjuster.Owner, current => current - adjuster.Axis.Of(slid.Delta)))
                                                          .WithRouting(routing => routing with { Latch = scroll with { Active = held.Kept(((IStreamed<Gesture.Scroll, Vector2>)live).Accumulated(slid.At, slid.Delta, slid.Modifiers)) } }))
                        .IfNone(held.Mount.Unhandled)),
                None: () => mount.Unhandled);

        private Routed Streaming(Seq<Key> owner, ActiveGesture live, Gesture next, Reply reply) =>
            reply.Switch(
                (Mount: mount, Owner: owner, Live: live, Next: next),
                ignored: static (held, _) => held.Mount.Unhandled.WithRouting(routing => routing with { Latch = new Latch.Part(held.Owner, Some(held.Live with { Gesture = held.Next })) }),
                accepted: static (held, accepted) => (held.Mount with {
                    Routing = held.Mount.Routing with { Latch = new Latch.Part(held.Owner, accepted.Track.Map(track => new ActiveGesture(held.Next, track))) },
                }).Handled(accepted.Effects));

        private Routed Unlatched() =>
            mount.Routing.Latch.Match(
                Some: latch => latch.Switch(
                    mount,
                    part: static (held, part) => part.Active.Match(
                        Some: live => new Routed(held with { Routing = held.Routing with { Latch = part with { Active = None } } }, [], Some(live.Track.Cancelled())),
                        None: () => held.Unhandled),
                    scroll: static (held, scroll) => held.Unhandled.WithRouting(routing => routing with { Latch = scroll with { Active = None } })),
                None: () => mount.Unhandled);

        // --- [PERFORM]
        private Routed Targeted(Input.Perform perform) =>
            perform.Target.Switch(
                (Mount: mount, Perform: perform, Gesture: new Gesture.Perform(perform.Act) { Modifiers = perform.Modifiers }),
                focused: static (held, _) => held.Mount.Bubbled(held.Gesture),
                at: static (held, at) => held.Mount.FirstHandled(held.Mount.Scene.Chain(held.Mount.Hits(at.Point)), (next, owner, over) => next.Performed(owner, over, held.Gesture)),
                element: static (held, element) => held.Mount.Addressed(held.Gesture, element.Id),
                surface: static (held, _) => held.Perform.Act.SwitchPartially(
                    held.Mount,
                    @default: static (next, _) => next.Unhandled,
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

        private Routed Replied(Gesture gesture, Seq<Key> target) => mount.Scene.Owner(target).Match(Some: owner => mount.Performed(owner, target, gesture), None: () => mount.Unhandled);

        private Option<Routed> Adjusting(Seq<Key> target, Func<Adjustment, Option<float>> moved) =>
            from adjuster in mount.Scene.Adjustment(target)
            from value in moved(adjuster)
            select mount.Changed(mount.Scene.Adjust(adjuster.Owner, _ => value));

        private Routed Bubbled(Gesture.Perform gesture) =>
            mount.Routing.Focus.Match(
                    Some: focus => mount.FirstHandled(mount.Scene.Owners(focus.Element).Map(owner => new PartHit(owner, focus.Element)), (next, owner, over) => next.Performed(owner, over, gesture)),
                    None: () => mount.Unhandled)
                .Otherwise(next => next.Defaulted(gesture.Act));

        private Routed Defaulted(Act act) =>
            act.SwitchPartially(
                mount,
                @default: static (held, _) => held.Unhandled,
                navigate: static (held, navigate) => held.Focused.Bind(focus => held.Adjusting(focus, adjuster => adjuster.Navigated(navigate.Toward))).IfNone(() => held.NavigatedSpatially(navigate.Toward)),
                next: static (held, _) => held.Traversed(Routing.Traversed(Routing.Stops(held.Tree), held.Focused, 1, held.Host.Placement.Cycles)),
                previous: static (held, _) => held.Traversed(Routing.Traversed(Routing.Stops(held.Tree), held.Focused, -1, held.Host.Placement.Cycles)));

        private Routed Entered(int sign) => mount.Traversed(Routing.Traversed(Routing.Stops(mount.Tree), None, sign, cycles: true));

        private Routed NavigatedSpatially(NavigationDirection toward) =>
            mount.Traversed(
                from focus in mount.Focused
                from origin in mount.Elements.Find(focus)
                from stop in Routing.Stops(mount.Tree).Find(stop => stop.Exists(element => element.Id == focus))
                from target in Routing.Toward(origin.Frame, stop.Filter(element => element.Id != focus).Map(static element => new Candidate<Seq<Key>>(element.Id, element.Frame)), toward)
                select target);

        private Routed Traversed(Option<Seq<Key>> stop) => stop.Match(Some: element => mount.Refocused(mount.Focusing(Some(element), traversed: true)).Handled(), None: () => mount.Unhandled);

        private Routed Performed(PartOwner owner, Seq<Key> over, Gesture gesture) =>
            owner.Part.Performed(gesture, mount.Scene.HitTarget(owner, over)).Switch(
                mount,
                ignored: static (held, _) => held.Unhandled,
                accepted: static (held, accepted) => held.Handled(accepted.Effects));
    }
}
