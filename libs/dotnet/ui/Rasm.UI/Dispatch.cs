using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Routed(Mount Mount, Seq<SKRect> Damage, Option<Seq<IO<Unit>>> Effects) {
    internal Seq<AccessibilityNotice> Notices { get; init; }
    internal Option<Lifted> Lifted { get; init; }

    internal Routed Then(Func<Mount, Routed> next) {
        Routed after = next(Mount);
        return new(after.Mount, Damage.Concat(after.Damage), Effects.Match(Some: effects => Some(effects + after.Effects.IfNone([])), None: () => after.Effects)) {
            Notices = Notices + after.Notices,
            Lifted = Lifted | after.Lifted,
        };
    }

    internal Routed Otherwise(Func<Mount, Routed> next) => Effects.IsSome ? this : Then(next);

    internal Routed WithRouting(Func<Routing, Routing> change) => this with { Mount = Mount with { Routing = change(Mount.Routing) } };

    internal Routed Handled() => this with { Effects = Some(Effects.IfNone([])) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Dispatch {
    extension(Mount mount) {
        // --- [DISPATCH]
        internal Routed Dispatched(Input input) =>
            input.Switch(
                mount,
                pointer: static (held, pointer) => held.Stroked(pointer),
                hover: static (held, hover) => held.Pointed(hover.At, hover.Modifiers),
                scroll: static (held, scroll) => held.Streamed(scroll, scroll.At, scroll.Delta.Map(delta => (Gesture)new Gesture.Scroll(scroll.At, delta)).As()),
                magnify: static (held, magnify) => held.Streamed(magnify, magnify.At, magnify.Amount.Map(amount => (Gesture)new Gesture.Magnify(magnify.At, amount)).As()),
                rotate: static (held, rotate) => held.Streamed(rotate, rotate.At, rotate.Radians.Map(static radians => (Gesture)new Gesture.Rotate(radians)).As()),
                drop: static (held, drop) => held.Pointed(Some(drop.At), drop.Modifiers).Then(next => next.Dropped(drop)),
                perform: static (held, perform) => perform.Act == Act.Cancel && held.Routing.Capture.IsSome ? held.Uncaptured() : held.Targeted(perform));

        internal Routed Cancel() => mount.Uncaptured().Then(static next => next.Unlatched());

        private Routed Pointed(Option<SKPoint> at, KeyModifiers modifiers) =>
            mount.Unhandled.WithRouting(routing => routing with {
                Pointer = at,
                Modifiers = modifiers,
                Latch = at == routing.Pointer ? routing.Latch : routing.Latch.Filter(static latch => latch.Switch(part: static part => part.Active.IsSome, scroll: static scroll => scroll.Active.IsSome)),
            });

        private Routed Changed(SceneChange next) =>
            ReferenceEquals(next.Scene, mount.Scene) ? mount.Unhandled : mount.Applied(next).Handled();

        private Routed Replied(Reply reply, Func<Option<Track>, Routing> routed) => (mount with { Routing = routed(reply.Track) }).Handled(reply.Effects);

        // --- [POINTER]
        private Routed Stroked(Input.Pointer pointer) =>
            pointer.Stroke.Value.Match(
                Some: sample => mount.Pointed(Some(sample.At), pointer.Modifiers).Then(pointed => pointer.Stroke.Phase.Switch(
                    (Mount: pointed, Pointer: pointer, Sample: sample),
                    begin: static held => held.Mount.Uncaptured().Then(next => next.Pressed(held.Sample, held.Pointer)),
                    preview: static held => held.Mount.Moved(held.Sample, held.Pointer.Modifiers),
                    step: static held => held.Mount.Moved(held.Sample, held.Pointer.Modifiers),
                    commit: static held => held.Mount.Released(held.Sample, held.Pointer.Modifiers),
                    cancel: static held => held.Mount.Cancel())),
                None: () => mount.Cancel());

        private Routed Pressed(PointerSample sample, Input.Pointer pointer) =>
            mount.Dismissible.Rev().TakeWhile(held => !mount.Within(held, sample.At)) switch {
                { IsEmpty: true } => mount.Engaged(sample, pointer),
                var outside => mount.Closed(outside).Handled().Then(next => outside.Exists(static held => held.Layer.Modal) ? next.Unhandled : next.Engaged(sample, pointer)),
            };

        private Routed Engaged(PointerSample sample, Input.Pointer pointer) {
            Seq<Seq<Key>> hits = mount.Scene.Hit(sample.At);
            Gesture.Press press = new(sample.At, None, pointer.Clicks) { Modifiers = pointer.Modifiers, Trail = [sample] };
            return (from top in hits.Head from scroller in mount.Thumb(top) select (Thumb: top, scroller.Value)).Match(
                Some: held => mount.Unhandled.WithRouting(routing => routing with { Capture = new PointerCapture.Handle(held.Thumb, sample.At, held.Value) }).Handled(),
                None: () => mount.Chained(mount.Scene.Chain(hits), hits.Head, Act.Press, Some(sample.At), (next, owner, over) => next.PressBegan(owner, over, press)));
        }

        private Routed PressBegan(PartOwner owner, Seq<Key> over, Gesture.Press press) =>
            owner.Part.Began(press, mount.Scene.HitTarget(owner, over)).Map(reply => mount.Refocused(mount.Focusing(mount.Element(owner.Path, over), traversed: false))
                .Then(next => next.Replied(reply, track => next.Routing with { Capture = track.Map(held => (PointerCapture)new PointerCapture.Part(owner.Path, over, press, held)) }))).IfNone(mount.Unhandled);

        private Routed Moved(PointerSample sample, KeyModifiers modifiers) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount, Sample: sample, Modifiers: modifiers),
                    part: static (held, part) => held.Mount.Dragged(part, held.Sample, held.Modifiers),
                    handle: static (held, handle) => held.Mount.ThumbDragged(handle, held.Sample.At)),
                None: () => mount.Unhandled);

        private Routed Dragged(PointerCapture.Part part, PointerSample sample, KeyModifiers modifiers) {
            Gesture.Press press = part.Press.Updated(sample, mount.Scene.Slop(part.Owner)) with { Modifiers = modifiers };
            return press == part.Press ? mount.Handled([]) : mount.Tracked(part with { Press = press }, part.Track.Moved(press)).Then(next => next.Exited(press));
        }

        private Routed Exited(Gesture.Press press) =>
            (from part in mount.Routing.Part
             from lifting in part.Track.Lifted
             where !mount.Scene.Covers(press.At)
             select (mount with { Routing = mount.Routing with { Capture = None } }).Handled(part.Track.Released(press)) with { Lifted = Some(new Lifted(lifting.Payload, press.At)) })
            .IfNone(mount.Unhandled);

        private Routed Released(PointerSample sample, KeyModifiers modifiers) =>
            mount.Routing.Capture.Match(
                Some: capture => capture.Switch(
                    (Mount: mount with { Routing = mount.Routing with { Capture = None } }, Sample: sample, Modifiers: modifiers),
                    part: static (held, part) => held.Mount.Ended(part, held.Sample, held.Modifiers),
                    handle: static (held, handle) => held.Mount.ThumbDragged(handle, held.Sample.At)),
                None: () => mount.Unhandled);

        private Routed Ended(PointerCapture.Part part, PointerSample sample, KeyModifiers modifiers) {
            Gesture.Press press = part.Press.Updated(sample, mount.Scene.Slop(part.Owner)) with { Modifiers = modifiers, Ended = true };
            return mount.Handled(part.Track.Released(press) + part.Track.Lifted.Bind(lifting => mount.Received(sample.At, lifting.Keys, Some(part.Owner))).ToSeq())
                .Then(next => press.Dragged.IsSome ? next.Unhandled : next.Picked(part.Owner));
        }

        private Routed Tracked(PointerCapture.Part part, Reply reply) =>
            mount.Replied(reply, track => mount.Routing with { Capture = track.Map(held => (PointerCapture)(part with { Track = held })) });

        private Routed ThumbDragged(PointerCapture.Handle handle, SKPoint at) =>
            mount.Scene.Scroller(handle.Owner).Map(scroller => mount.Changed(mount.Scene.Adjust(scroller.Owner, _ => handle.Start + (scroller.ThumbRatio * scroller.Axis.Of(at - handle.From))))).IfNone(mount.Unhandled).Handled();

        private Routed Uncaptured() =>
            mount.Routing.Capture.Match(
                Some: capture => new Routed(mount with { Routing = mount.Routing with { Capture = None } }, [], Some(capture.Cancelled)),
                None: () => mount.Unhandled);

        // --- [DROP]
        private Routed Dropped(Input.Drop drop) =>
            (from transfer in drop.Transfer.Value
             where drop.Transfer.Phase != EditPhase.Cancel
             from effect in mount.Received(drop.At, transfer, None)
             select mount.Handled(drop.Transfer.Phase == EditPhase.Commit ? [effect] : [])).IfNone(mount.Unhandled);

        private Option<IO<Unit>> Received(SKPoint at, Transfer transfer, Option<Seq<Key>> source) {
            Option<Seq<Key>> hit = mount.Scene.Hit(at).Head;
            return hit.Map(Scene.AncestorsAndSelf).IfNone([])
                .Filter(path => source != Some(path))
                .Choose(path =>
                    from layout in mount.Scene.Index.Find(path)
                    from drop in layout.Node.Drop
                    from effect in mount.Placing(path, hit, at, layout.Node, drop, transfer)
                    select effect)
                .Head;
        }

        private Option<IO<Unit>> Placing(Seq<Key> target, Option<Seq<Key>> hit, SKPoint at, Node node, Drop drop, Transfer transfer) {
            Option<LayoutNode> child = hit.Filter(over => over.Count > target.Count).Bind(over => mount.Scene.Index.Find(over.Take(target.Count + 1)));
            return from frame in child.Bind(held => mount.Scene.Frame(held.Path)) | mount.Scene.Frame(target)
                   from placement in node.MainAxis.Match(
                       Some: axis => Fraction.Validated((axis.Of(at) - axis.Of(frame.Location)) / axis.Of(frame.Size)).ToOption().Map(along => Placement.At(along, drop.Nests)),
                       None: static () => Some(Placement.Inside))
                   from effect in drop.Accepted(transfer, child.Bind(static held => held.Node.Key), placement)
                   select effect;
        }

        // --- [STREAM]
        private Routed Notched(SKPoint at, Gesture notch) =>
            mount.Sampled(notch, _ => mount.Prioritized(mount.Scene.Hit(at), notch, (next, owner, over) => next.Performed(owner, over, notch), static (next, scroll, scrollers) => next.Slid(scrollers, scroll, latches: false)));

        private Routed Streamed(Input input, SKPoint at, Edit<Gesture> edit) =>
            mount.Pointed(Some(at), input.Modifiers).Then(next => next.Streamed(at, edit.Map(gesture => gesture with { Modifiers = input.Modifiers }).As()));

        private Routed Streamed(SKPoint at, Edit<Gesture> edit) =>
            edit.Value.Match(
                Some: value => edit.Phase == EditPhase.Step ? mount.Notched(at, value) : mount.Sampled(value, increment => edit.Phase switch {
                    { Begins: true } => mount.Unlatched().Then(next => next.Latching(next.Scene.Hit(at), increment)),
                    { Applies: true } => mount.Continued(increment, static (live, next) => new Reply(live.Track.Released(next), None), static _ => None),
                    _ => mount.Continued(increment, static (live, next) => live.Track.Moved(next), static scroll => Some(scroll)),
                }),
                None: () => mount.Routing.Capture.IsSome ? mount.Unhandled : mount.Unlatched());

        private Routed Sampled(Gesture increment, Func<Gesture, Routed> streamed) =>
            mount.Routing.Capture.IsSome ? mount.Routing.Part.Map(part => mount.Wheeled(part, increment)).IfNone(mount.Unhandled) : streamed(increment);

        private Routed Wheeled(PointerCapture.Part part, Gesture increment) =>
            Optional(increment as Gesture.Scroll).Map(scroll => part.Press with { Scrolled = part.Press.Scrolled + scroll.Delta })
                .Map(press => mount.Tracked(part with { Press = press }, part.Track.Moved(press)))
                .IfNone(() => mount.Handled([]));

        private Routed Latching(Seq<Seq<Key>> hits, Gesture increment) =>
            mount.Routing.Latch.Match(
                    Some: latch => latch.Switch(
                        (Mount: mount, Hits: hits, Increment: increment),
                        part: static (held, part) => held.Mount.Scene.EnabledPartAt(part.Owner).Match(
                            Some: owner => held.Mount.Opened(owner, held.Hits.Find(hit => hit.Take(owner.Path.Count) == owner.Path).IfNone(owner.Path), held.Increment),
                            None: () => held.Mount.Unhandled),
                        scroll: static (held, scroll) => (from slid in Optional(held.Increment as Gesture.Scroll)
                                                          from scroller in held.Mount.Scene.Scroller(scroll.Owner)
                                                          where scroller.Scrolls(slid.Delta)
                                                          select held.Mount.Slid([scroller], slid, latches: true)).IfNone(held.Mount.Unhandled)),
                    None: () => mount.Unhandled)
                .Otherwise(next => next.Prioritized(hits, increment, (held, owner, over) => held.Opened(owner, over, increment), static (held, scroll, scrollers) => held.Slid(scrollers, scroll, latches: true)))
                switch {
                    var routed when routed.Effects.IsNone => routed.WithRouting(static routing => routing with { Latch = None }),
                    var routed => routed,
                };

        private Routed Prioritized(Seq<Seq<Key>> hits, Gesture gesture, Func<Mount, PartOwner, Seq<Key>, Routed> part, Func<Mount, Gesture.Scroll, Seq<Scroller>, Routed> core) =>
            (Chain: mount.Scene.Chain(hits), Holding: mount.Focused.Bind(mount.Scene.Owner).Map(static owner => owner.Path)) switch {
                var held when gesture.Modifiers.Primary || held.Chain.Head.Exists(owner => held.Holding == Some(owner.Owner.Path)) =>
                    mount.FirstHandled(held.Chain, part).Otherwise(next => next.Scrolling(hits, gesture, core)),
                var held => mount.Scrolling(hits, gesture, core).Otherwise(next => next.FirstHandled(held.Chain, part)),
            };

        private Routed Scrolling(Seq<Seq<Key>> hits, Gesture gesture, Func<Mount, Gesture.Scroll, Seq<Scroller>, Routed> core) =>
            Optional(gesture as Gesture.Scroll).Map(scroll => (Scroll: scroll, Scrollers: mount.Scene.Scrollers(hits, scroll.Delta))).Filter(static held => !held.Scrollers.IsEmpty)
                .Match(Some: held => core(mount, held.Scroll, held.Scrollers), None: () => mount.Unhandled);

        private Routed FirstHandled(Seq<(PartOwner Owner, Seq<Key> Hit)> chain, Func<Mount, PartOwner, Seq<Key>, Routed> ask) =>
            chain.Fold(mount.Unhandled, (routed, held) => routed.Otherwise(next => ask(next, held.Owner, held.Hit)));

        private Routed Opened(PartOwner owner, Seq<Key> over, Gesture increment) =>
            owner.Part.Began(increment, mount.Scene.HitTarget(owner, over)).Map(reply => mount.Replied(reply, track => mount.Routing with { Latch = new Latch.Part(owner.Path, track.Map(held => (increment, held))) })).IfNone(mount.Unhandled);

        private Routed Slid(Scroller scroller, Vector2 delta) => mount.Changed(mount.Scene.Adjust(scroller.Owner, value => value - scroller.Axis.Of(delta)));

        private Routed Slid(Seq<Scroller> scrollers, Gesture.Scroll scroll, bool latches) =>
            scrollers.Map(scroller => (scroller.Owner, Moved: mount.Slid(scroller, scroll.Delta))).Find(static held => held.Moved.Effects.IsSome).Match(
                Some: held => latches ? held.Moved.WithRouting(routing => routing with { Latch = new Latch.Scroll(held.Owner, Some(scroll)) }) : held.Moved,
                None: () => latches ? mount.Unhandled : mount.Unhandled.Handled());

        private Routed Continued(Gesture increment, Func<(Gesture Gesture, Track Track), Gesture, Reply> step, Func<Gesture.Scroll, Option<Gesture.Scroll>> kept) =>
            mount.Routing.Latch.Match(
                Some: latch => latch.Switch(
                    (Mount: mount, Increment: increment, Step: step, Kept: kept),
                    part: static (held, part) => (from live in part.Active
                                                  from next in live.Gesture.Accumulated(held.Increment)
                                                  select held.Mount.Replied(held.Step(live, next), track => held.Mount.Routing with { Latch = new Latch.Part(part.Owner, track.Map(moved => (next, moved))) }))
                        .IfNone(held.Mount.Unhandled),
                    scroll: static (held, scroll) => (from live in scroll.Active
                                                      from slid in Optional(held.Increment as Gesture.Scroll)
                                                      from scroller in held.Mount.Scene.Scroller(scroll.Owner)
                                                      select held.Mount.Slid(scroller, slid.Delta).WithRouting(routing => routing with { Latch = scroll with { Active = held.Kept(slid with { Delta = live.Delta + slid.Delta }) } }))
                        .IfNone(held.Mount.Unhandled)),
                None: () => mount.Unhandled);

        private Routed Unlatched() =>
            mount.Routing.Latch.Match(
                Some: latch => (mount with {
                    Routing = mount.Routing with { Latch = latch.Switch(part: static part => (Latch)(part with { Active = None }), scroll: static scroll => scroll with { Active = None }) },
                }).WithEffects(latch.Cancelled),
                None: () => mount.Unhandled);

        // --- [PERFORM]
        private Routed Targeted(Input.Perform perform) =>
            perform.Target.Switch(
                (Mount: mount, Perform: perform, Gesture: new Gesture.Perform(perform.Act) { Modifiers = perform.Modifiers }),
                focused: static (held, _) => held.Mount.Bubbled(held.Gesture),
                at: static (held, at) => held.Mount.Scene.Hit(at.Point) switch {
                    var hits => held.Mount.Chained(held.Mount.Scene.Chain(hits), hits.Head, held.Perform.Act, Some(at.Point), (next, owner, over) => next.Performed(owner, over, held.Gesture)),
                },
                element: static (held, element) => held.Mount.Addressed(held.Gesture, element.Id),
                surface: static (held, _) => held.Perform.Act.SwitchPartially(
                    held.Mount,
                    @default: static (next, _) => next.Unhandled,
                    next: static next => next.Traversing(None, 1, cycles: true),
                    previous: static next => next.Traversing(None, -1, cycles: true)));

        private Routed Addressed(Gesture.Perform gesture, Seq<Key> target) =>
            gesture.Act.SwitchPartially(
                    (Mount: mount, Target: target),
                    @default: static (held, act) => act.Toward.Bind(toward => held.Mount.Offset(held.Target, scroller => scroller.Navigated(toward))),
                    scrollToVisible: static held => Some(held.Mount.Changed(held.Mount.Scene.Reveal(held.Target))),
                    increment: static held => held.Mount.Offset(held.Target, static scroller => Some(scroller.Value + scroller.Step)),
                    decrement: static held => held.Mount.Offset(held.Target, static scroller => Some(scroller.Value - scroller.Step)))
                .IfNone(() => mount.Chained(mount.Scene.Owners(target).Take(1).Map(owner => (Owner: owner, Hit: target)), Some(target), gesture.Act, None, (next, owner, over) => next.Performed(owner, over, gesture)));

        private Option<Routed> Offset(Seq<Key> target, Func<Scroller, Option<float>> moved) =>
            from scroller in mount.Scene.Scroller(target)
            from value in moved(scroller)
            select mount.Changed(mount.Scene.Adjust(scroller.Owner, _ => value));

        private Routed Bubbled(Gesture.Perform gesture) =>
            mount.Walked(gesture.Act).IfNone(() => mount.Routing.Focus.Match(
                    Some: focus => mount.Chained(mount.Scene.Owners(focus.Element).Map(owner => (Owner: owner, Hit: focus.Element)), Some(focus.Element), gesture.Act, None, (next, owner, over) => next.Performed(owner, over, gesture)),
                    None: () => mount.Unhandled)
                .Otherwise(next => next.Defaulted(gesture.Act)));

        private Routed Defaulted(Act act) =>
            act.SwitchPartially(
                mount,
                @default: static (held, act) => act.Toward.Map(toward => held.Focused.Bind(focus => held.Offset(focus, scroller => scroller.Navigated(toward))).IfNone(() => held.NavigatedSpatially(toward))).IfNone(held.Unhandled),
                confirm: static held => Scene.Preorder((held.Layers.Rev().Find(layer => held.Focused.Exists(layer.Holds)) | held.Layers.Last).Map(static layer => layer.Layout).IfNone(held.Scene.Layout))
                    .Find(layout => layout.Style.Conditions.Contains(Condition.Default) && held.Scene.EnabledPartAt(layout.Path).IsSome)
                    .Match(Some: target => held.Addressed(new Gesture.Perform(Act.Press), target.Path), None: () => held.Unhandled),
                cancel: static held => held.Dismissible.Last.Match(Some: layer => held.Closed([layer]).Handled(), None: () => held.Unhandled),
                next: static held => held.Traversing(held.Focused, 1, held.Host.Cycles),
                previous: static held => held.Traversing(held.Focused, -1, held.Host.Cycles));

        private Routed NavigatedSpatially(Bearing toward) =>
            mount.Traversed(
                from focus in mount.Focused
                from origin in mount.Elements.Find(focus)
                from stop in mount.Stops.Find(stop => stop.Exists(element => element.Id == focus))
                from target in Routing.Toward(origin.Frame, stop.Filter(element => element.Id != focus).Map(static element => (Item: element.Id, element.Frame)), toward)
                select target);

        private Routed Traversing(Option<Seq<Key>> from, int sign, bool cycles) => mount.Traversed(Routing.Traversed(mount.Stops, from, sign, cycles));

        private Routed Traversed(Option<Seq<Key>> stop) => stop.Match(Some: element => mount.Refocused(mount.Focusing(Some(element), traversed: true)).Handled(), None: () => mount.Unhandled);

        private Routed Performed(PartOwner owner, Seq<Key> over, Gesture gesture) =>
            owner.Part.Performed(gesture, mount.Scene.HitTarget(owner, over))
                .Map(reply => mount.Handled(reply.Effects).Then(next => gesture is Gesture.Perform perform && (perform.Act == Act.Press || perform.Act == Act.Confirm) ? next.Picked(owner.Path) : next.Unhandled))
                .IfNone(mount.Unhandled);

        // --- [LAYERS]
        private Routed Chained(Seq<(PartOwner Owner, Seq<Key> Hit)> chain, Option<Seq<Key>> over, Act act, Option<SKPoint> at, Func<Mount, PartOwner, Seq<Key>, Routed> ask) =>
            over.Bind(target => mount.Anchored(target, act)).Match(
                Some: anchored => mount.FirstHandled(chain.TakeWhile(link => link.Owner.Path.Count > anchored.Depth), ask).Otherwise(next => next.Toggled(anchored.Layer, at)),
                None: () => mount.FirstHandled(chain, ask));

        private Option<(PlacedLayer Layer, int Depth)> Anchored(Seq<Key> over, Act act) =>
            (from prefix in Scene.AncestorsAndSelf(over)
             where mount.Scene.Index.Find(prefix).Exists(static layout => !layout.Style.Conditions.Contains(Condition.Disabled))
             from held in mount.Scene.Anchoring(prefix)
             where held.Acts.Exists(opens => opens == act)
             select (Layer: held, Depth: prefix.Count)).Head;

        private bool Within(PlacedLayer held, SKPoint at) =>
            mount.Scene.Frame(held.Layout.Path).Exists(frame => frame.Contains(at))
            || ((held.Acts.IsEmpty || held.Acts.Exists(static act => act == Act.Press)) && held.Anchor.Bind(mount.Scene.Frame).Exists(frame => frame.Contains(at)));

        // --- [DATUM]
        private Option<Routed> Walked(Act act) =>
            from toward in act.Toward
            where toward.Along == PhysicalAxis.Horizontal
            from focus in mount.Focused
            from layout in mount.Scene.Index.Find(focus)
            from plot in Optional(layout.Node as Node.Plot)
            where plot.Crosshair.IsSome
            let was = mount.Nearest.Filter(near => near.Plot == focus).Map(static near => near.Data).IfNone([])
            let read = Scene.Preorder(layout).Choose(held =>
                from series in held.Node.Key
                from figure in held.Node.Traced
                from datum in Stepped(plot.Plane, figure, was.Find(series), toward)
                select (Series: series, Datum: datum, Spoken: held.Node.Accessible.Label.Map(static label => $"{label} ").IfNone("") + plot.Plane.Spoken(datum.Value, mount.Host.Culture)))
            where !read.IsEmpty
            select (mount with { Nearest = Some(new Nearest(focus, toHashMap(read.Map(static held => (held.Series, held.Datum))))) }).Handled([]) with {
                Notices = [new AccessibilityNotice.Announced(string.Join($"{mount.Host.Culture.TextInfo.ListSeparator} ", read.Map(static held => held.Spoken)), Live.Polite)],
            };
    }

    private static Option<Datum> Stepped(PlotPlane plane, Figure figure, Option<Datum> current, Bearing toward) {
        Seq<Datum> data = figure.Data;
        int last = data.Count - 1;
        int index = toward.Granularity.Distance(1f, int.Max(1, data.Filter(datum => plane.Contains(PhysicalAxis.Horizontal, None, datum.Value.X)).Count)).Match(
            Some: stride => current.Map(datum => datum.Index + (toward.Sign * (int)stride)).IfNone(toward.Sign > 0 ? 0 : last),
            None: () => toward.Sign > 0 ? last : 0);
        return last < 0 ? None : figure.DatumAt(int.Clamp(index, 0, last));
    }
}
