using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Topten.RichTextKit;

namespace Rasm.UI.Controls.Primitives;

// --- [MODELS] --------------------------------------------------------------------------

public abstract record Part {

    // --- [GESTURES]

    internal abstract Option<Reply> Began(Gesture gesture, HitTarget aim);

    internal virtual Option<Reply> Performed(Gesture gesture, HitTarget aim) => None;

    internal virtual bool Offers(LayoutNode owner, Seq<Key> path) => path == owner.Path;

    // --- [WATCHING]

    internal virtual Option<Seq<Key>> Revealed(LayoutNode layout) => None;

    internal virtual Seq<IO<Unit>> Watched(Option<HoverState> before, HoverState after) => [];

    // --- [CASES]

    private protected Part() { }

    public sealed record Press(Func<SKRect, IO<Unit>> Run) : Part {
        public Func<SKRect, Option<IO<Unit>>> Secondary { get; init; } = static _ => None;
        public Option<IO<Unit>> Reset { get; init; }
        public Option<IO<Unit>> Delete { get; init; }

        public View Drawn(View face) => face with { Part = this, Accessible = face.Accessible.Combine(new Accessible { Role = AccessRole.Button }) };

        internal static View Dismiss(IO<Unit> run) => new Press(_ => run) { Delete = Some(run) }.Drawn(new View.Box { Role = Roles.Dismiss, Align = UnitPoint.Center });

        internal override Option<Reply> Began(Gesture gesture, HitTarget aim) => gesture is Gesture.Press ? Some(new Reply([], Some(Holding(aim)))) : None;

        internal override Option<Reply> Performed(Gesture gesture, HitTarget aim) =>
            from perform in Optional(gesture as Gesture.Perform)
            from effect in perform.Act.SwitchPartially(
                (Press: this, aim.Owner.Bounds),
                @default: static (_, _) => Option<IO<Unit>>.None,
                press: Activated,
                confirm: Activated,
                showMenu: static held => held.Press.Secondary(held.Bounds),
                reset: static held => held.Press.Reset,
                delete: static held => held.Press.Delete)
            select new Reply([effect], None);

        private Track Holding(HitTarget aim) =>
            new(_ => new Reply([], Some(Holding(aim))),
                gesture => gesture is Gesture.Press press && aim.Owner.Bounds.Contains(press.At) ? Seq(Run(aim.Owner.Bounds)) : [],
                static () => []);

        private static Option<IO<Unit>> Activated((Press Press, SKRect Bounds) held) => Some(held.Press.Run(held.Bounds));
    }

    public sealed record Choice(Seq<Key> Items, Selection Value, Func<Edit<Selection>, IO<Unit>> Edited) : Part {

        // --- [STATE]

        public Choice(Seq<Key> items, Option<Key> value, Func<Edit<Key>, IO<Unit>> edited) : this(items, new Selection.One(value), edit => edited(edit.Choose(static held => held.Lead))) { }

        public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Reorder { get; init; }
        public Option<Func<Seq<Key>, IO<Unit>>> Removed { get; init; }
        public Func<Seq<Key>, SKRect, Option<IO<Unit>>> Menu { get; init; } = static (_, _) => None;
        public Func<Option<Key>, Option<IO<Unit>>> Hover { get; init; } = static _ => None;
        public Func<HashSet<Key>, Option<IO<Unit>>> Viewed { get; init; } = static _ => None;
        public Option<Selection> Rest { get; init; }
        public HashSet<Key> Highlighted { get; init; }
        public Marquee Marquee { get; init; } = Marquee.Box;

        private AccessRole Composite => Value.Map(one: AccessRole.RadioGroup, many: AccessRole.List);

        // --- [DRAWING]

        public View Drawn(Func<Func<View, Key, View>, View> content) =>
            content((node, key) => Member(node, key, Some(Roles.ListRow))) switch {
                var held => held with { Part = this, Accessible = held.Accessible.Combine(new Accessible { Role = Composite }) },
            };

        public View Drawn(Seq<View> faces, Func<Seq<View>, View> container) =>
            Drawn(member => container(toSeq(Items.Zip(faces, (key, face) => member(face, key)))));

        public View Drawn(Func<Key, View> item, Func<Seq<View>, View> container) => Drawn(member => container(Items.Map(key => member(item(key), key))));

        public View Drawn(Seq<Key> shown, Func<Key, View> item, Func<Seq<View>, View> container) {
            HashSet<Key> enabled = toHashSet(Items);
            return Drawn(member => container(shown.Map(key => member(item(key), key) switch {
                var node => enabled.Contains(key) ? node : node.Conditioned(Condition.Disabled),
            })));
        }

        public View.Lazy Drawn(Func<Key, View> item, PhysicalAxis axis, Tracks across, HashSet<Key> realized) =>
            new(axis, across, Items) {
                Realized = toHashMap(Items.Filter(realized.Contains).Map(key => (key, Member(item(key), key, None)))),
                Part = this,
                Accessible = new Accessible { Role = Composite },
            };

        private View Member(View node, Key key, Option<Key> row) =>
            node with {
                Key = key,
                Role = node.Role | row,
                Conditions = node.Conditions.Union((Value.Holds(key) ? Seq(Condition.Selected) : []) + (Highlighted.Contains(key) ? Seq(Condition.Highlighted) : [])),
                Accessible = node.Accessible.Combine(new Accessible { Role = Composite.Owns.IfNone(AccessRole.Radio) }),
            };

        // --- [REMOVAL]

        public static Func<Seq<Key>, IO<Unit>> Without(Seq<Key> items, Func<Edit<Seq<Key>>, IO<Unit>> edited) =>
            removed => toHashSet(removed) switch { var gone => edited(Edit.Of(EditPhase.Commit, items.Filter(item => !gone.Contains(item)))) };

        internal Option<View> Dismiss(Key key) => Removed.Map(removed => Press.Dismiss(removed([key])));

        // --- [GESTURES]

        internal override bool Offers(LayoutNode owner, Seq<Key> path) => path == owner.Path || Tiles(owner).Exists(tile => tile.Layout.Path == path);

        internal override Option<Seq<Key>> Revealed(LayoutNode layout) => Value.Lead.Bind(lead => Paths(layout, [lead]).Head);

        internal override Option<Reply> Began(Gesture gesture, HitTarget aim) =>
            from press in Optional(gesture as Gesture.Press)
            from session in Item(aim.Hit, aim.Owner.Layout).Map(key => Pressing(key, aim)) || (Value is Selection.Many ? Some(Enclosing(aim)) : None)
            from reply in session.Began(press)
            select reply;

        internal override Option<Reply> Performed(Gesture gesture, HitTarget aim) =>
            gesture is Gesture.Perform perform
                ? perform.Act.SwitchPartially(
                    (Choice: this, Aim: aim, perform.Modifiers),
                    @default: static (held, act) => act.Toward.Bind(toward => held.Choice.Reached(held.Aim, toward)).Bind(key => held.Choice.Stepping(start => start.Navigated(held.Choice.Items, key, held.Modifiers.Shift))),
                    press: Activated,
                    confirm: Activated,
                    showMenu: static held =>
                        from key in held.Choice.Item(held.Aim.Hit, held.Aim.Owner.Layout)
                        from tile in held.Choice.Framed(held.Aim).Find(tile => tile.Item == key)
                        from effect in held.Choice.Menu(held.Choice.Moving(held.Choice.Value, key), tile.Frame)
                        select new Reply([effect], None),
                    reset: static held => Edit.Restoring(held.Choice.Rest, Some(held.Choice.Value)).Bind(rest => held.Choice.Stepping(_ => rest)),
                    delete: static held =>
                        from removed in held.Choice.Removed
                        from key in held.Choice.Item(held.Aim.Hit, held.Aim.Owner.Layout) | held.Choice.Value.Lead
                        select new Reply([removed(held.Choice.Moving(held.Choice.Value, key))], None),
                    selectAll: static held => held.Choice.Stepping(start => start.All(held.Choice.Items)))
                : None;

        internal override Seq<IO<Unit>> Watched(Option<HoverState> before, HoverState after) {
            Option<Key> hovered = Hovered(after);
            HashSet<Key> seen = Seen(after);
            return (before.Bind(Hovered) == hovered ? [] : Hover(hovered).ToSeq()) + (before.Map(Seen) == Some(seen) ? [] : Viewed(seen).ToSeq());
        }

        private Session<(Selection Value, Seq<Key> Order, Option<Figure> Outline)> Session(CursorShape cursor) =>
            new((Value, Items, None),
                Seq(Receiver<(Selection Value, Seq<Key> Order, Option<Figure> Outline)>.Of(static held => held.Value, edit => Some(Edited(edit))))
                + Reorder.Map(static reorder => Receiver<(Selection Value, Seq<Key> Order, Option<Figure> Outline)>.Of(static held => held.Order, edit => Some(reorder(edit)))).ToSeq(),
                cursor) {
                Preview = static held => held.Outline,
            };

        private Session<(Selection Value, Seq<Key> Order, Option<Figure> Outline)> Pressing(Key key, HitTarget aim) =>
            Session(Reorder.IsSome ? CursorShape.Grab : CursorShape.Default) with {
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(Pressed(start, key, press, aim)) : None,
                Lifted = state => aim.Owner.Layout.View.Lift.Map(lift => Moving(state.Value, key) switch {
                    var moving => (new Transfer.Keys(Paths(aim.Owner.Layout, moving)), lift.Payload(moving)),
                }),
            };

        private (Selection Value, Seq<Key> Order, Option<Figure> Outline) Pressed((Selection Value, Seq<Key> Order, Option<Figure> Outline) start, Key key, Gesture.Press press, HitTarget aim) =>
            start.Value.Pressed(Items, key, press.Modifiers, press.Clicked) switch {
                var selection when press.Dragged.IsNone || (aim.Owner.Layout.View.Lift.IsSome && !aim.Scene.Covers(press.At)) => start with { Value = selection, Order = Items },
                var selection => start with { Value = selection, Order = Reorder.IsSome ? Ordered(Moving(selection, key), Framed(aim).Find(tile => tile.Frame.Contains(press.At)).Map(static tile => tile.Item)) : Items },
            };

        private Session<(Selection Value, Seq<Key> Order, Option<Figure> Outline)> Enclosing(HitTarget aim) =>
            Session(CursorShape.Default) with {
                Step = (start, gesture) => gesture is Gesture.Press press
                    ? Some(start with {
                        Value = start.Value.Enclosed(Items, toHashSet(Marquee.Covered(press, Framed(aim), static tile => tile.Frame).Map(static tile => tile.Item)), press.Modifiers),
                        Outline = Marquee.Outlined(aim.Frame.IfNone(() => PlotPlane.Cartesian.Unit.In(aim.Owner.Bounds)), press),
                    })
                    : None,
            };

        private Option<Reply> Stepping(Func<Selection, Selection> next) =>
            Session(CursorShape.Default).Stepped(start => Some(start with { Value = next(start.Value) }));

        private static Option<Reply> Activated((Choice Choice, HitTarget Aim, KeyModifiers Modifiers) held) =>
            held.Choice.Item(held.Aim.Hit, held.Aim.Owner.Layout).Bind(key => held.Choice.Stepping(start => start.Pressed(held.Choice.Items, key, held.Modifiers, clicked: true)));

        private Option<Key> Reached(HitTarget aim, Bearing toward) =>
            toward.Granularity.Switch(
                (Choice: this, Aim: aim, Toward: toward),
                step: static held => held.Choice.Value.Lead.Match(
                    Some: lead => held.Choice.Framed(held.Aim) switch {
                        var tiles => tiles.Find(tile => tile.Item == lead).Bind(origin => Routing.Toward(origin.Frame, tiles.Filter(tile => tile.Item != lead), held.Toward)),
                    },
                    None: () => held.Choice.Items.Head),
                page: static _ => Option<Key>.None,
                end: static held => held.Toward.Sign < 0 ? held.Choice.Items.Head : held.Choice.Items.Last);

        // --- [READS]

        private Option<Key> Item(Seq<Key> over, LayoutNode layout) => Item(over, layout, toHashSet(Items));

        private Seq<(Key Key, LayoutNode Layout)> Tiles(LayoutNode layout) => Tiles(layout, toHashSet(Items));

        private Seq<Seq<Key>> Paths(LayoutNode layout, Seq<Key> keys) {
            HashMap<Key, Seq<Key>> tiles = toHashMap(Tiles(layout).Map(static tile => (tile.Key, tile.Layout.Path)));
            return keys.Choose(key => tiles.Find(key) || Optional(layout.View as View.Lazy).Filter(lazy => lazy.Items.Exists(item => item == key)).Map(_ => layout.Path.Add(key)));
        }

        private Seq<(Key Item, SKRect Frame)> Framed(HitTarget aim) => Tiles(aim.Owner.Layout).Choose(tile => aim.Scene.Frame(tile.Layout.Path).Map(frame => (Item: tile.Key, Frame: frame))).Strict();

        private Seq<Key> Moving(Selection selection, Key key) => selection.Holds(key) ? Items.Filter(selection.Holds) : [key];

        private Seq<Key> Ordered(Seq<Key> moving, Option<Key> under) {
            HashSet<Key> moved = toHashSet(moving);
            return (from target in under.Filter(key => !moved.Contains(key))
                    from place in Items.Position(target)
                    from first in moving.Head.Bind(key => Items.Position(key))
                    let rest = Items.Filter(key => !moved.Contains(key))
                    from slot in rest.Position(target)
                    let index = place > first ? slot + 1 : slot
                    select rest.Take(index) + moving + rest.Skip(index)).IfNone(Items);
        }

        private Option<Key> Hovered(HoverState sight) => sight.Hover.Choose(path => Item(path, sight.Layout)).Head;

        private HashSet<Key> Seen(HoverState sight) =>
            toHashSet(Optional(sight.Layout.View as View.Lazy).Match(
                Some: lazy => Layouts.Visible(sight.Scene, sight.Layout, lazy),
                None: () => Tiles(sight.Layout).Filter(tile => sight.Scene.Bounds(tile.Layout.Path).Exists(static bounds => !bounds.IsEmpty)).Map(static tile => tile.Key)));

        private static Option<Key> Item(Seq<Key> over, LayoutNode layout, HashSet<Key> members) =>
            toSeq(SuperEnumerable.TraverseDepthFirst(layout, node => node.Children.Find(child => over.StartsWith(child.Path)).ToSeq())).Skip(1)
                .Choose(node => Tile(node, members)).Head;

        private static Seq<(Key Key, LayoutNode Layout)> Tiles(LayoutNode layout, HashSet<Key> members) =>
            layout.Children.Bind(child => toSeq(SuperEnumerable.TraverseDepthFirst((Layout: child, Key: Tile(child, members)), held => held.Key.IsSome
                ? []
                : held.Layout.Children.Map(node => (Layout: node, Key: Tile(node, members))))))
                .Choose(static held => held.Key.Map(key => (Key: key, held.Layout)));

        private static Option<Key> Tile(LayoutNode layout, HashSet<Key> members) => layout.View.Key.Filter(members.Contains);
    }

    public sealed record Viewport(PlotPlane Rest, Func<Edit<PlotPlane>, IO<Unit>> Edited) : Part {

        // --- [STATE]

        public HashSet<PhysicalAxis> Free { get; init; } = toHashSet(PhysicalAxis.Items);
        public Option<PlotPlane> Bounds { get; init; }
        public bool Boxed { get; init; }
        public Limits Magnification { get; init; } = Limits.Open;
        public Option<Vector2> Natural { get; init; }

        // --- [GESTURES]

        internal override Option<Reply> Began(Gesture gesture, HitTarget aim) =>
            aim.Frame.Bind(frame => gesture.SwitchPartially(
                (Viewport: this, Frame: frame, aim.Owner.Layout.Styled(Roles.Handle).Metrics),
                @default: static (held, _) => held.Viewport.Panning(held.Frame, held.Metrics),
                press: static (held, press) =>
                    press.Clicks >= 2 ? held.Viewport.Tracking(held.Frame, (start, next) => next is Gesture.Press ? held.Viewport.Cycled(held.Frame, start) : None)
                    : held.Viewport.Boxed ? held.Viewport.Boxing(held.Frame)
                    : held.Viewport.Panning(held.Frame, held.Metrics)).Began(gesture));

        internal override Option<Reply> Performed(Gesture gesture, HitTarget aim) =>
            from frame in aim.Frame
            from move in gesture.SwitchPartially(
                (Viewport: this, Frame: frame, Center: new SKPoint(aim.Owner.Bounds.MidX, aim.Owner.Bounds.MidY), aim.Owner.Layout.Styled(Roles.Handle).Metrics),
                @default: static (_, _) => Option<Func<PlotPlane, Option<PlotPlane>>>.None,
                scroll: static (held, wheel) => held.Viewport.Wheeled(held.Frame, wheel, held.Metrics, notch: true),
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    (held.Viewport, held.Frame, held.Center, Notch: held.Metrics[Metric.Notch], Extent: held.Metrics[Metric.Extent], Fine: perform.Modifiers.Primary ? held.Metrics[Metric.Fine] : 1f),
                    @default: static (state, act) => act.Toward.Map(toward => state.Viewport.Navigated(state.Frame, state.Center, toward, state.Extent)),
                    increment: static state => Some(state.Viewport.Notched(state.Frame, state.Center, state.Notch, -state.Fine)),
                    decrement: static state => Some(state.Viewport.Notched(state.Frame, state.Center, state.Notch, state.Fine)),
                    reset: static state => Some<Func<PlotPlane, Option<PlotPlane>>>(start => state.Viewport.Cycled(state.Frame, start))))
            from reply in Tracking(frame, (start, _) => move(start)).Stepped(gesture)
            select reply;

        // --- [SESSIONS]

        private Session<(PlotPlane Plane, Option<Figure.Region> Box)> Tracking(PlotFrame frame, Func<PlotPlane, Gesture, Option<PlotPlane>> step) =>
            new((frame.Plane, None), [Receiver<(PlotPlane Plane, Option<Figure.Region> Box)>.Of(static view => view.Plane, edit => Some(Edited(edit)))], CursorShape.Grab) {
                Step = (start, gesture) => step(start.Plane, gesture).Bind(plane => Bounded(frame, plane)).Map(static plane => (plane, Option<Figure.Region>.None)),
            };

        private Session<(PlotPlane Plane, Option<Figure.Region> Box)> Boxing(PlotFrame frame) =>
            Tracking(frame, static (_, _) => None) with {
                Cursor = CursorShape.Crosshair,
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(start with { Box = press.Enclosed(frame).Map(box => box with { First = box.First.Filter(_ => Free.Contains(PhysicalAxis.Horizontal)), Second = box.Second.Filter(_ => Free.Contains(PhysicalAxis.Vertical)) }) }) : None,
                Released = view => view.Box.Bind(box => Fitted(frame, view.Plane, box)).Map(static plane => (plane, Option<Figure.Region>.None)).IfNone(view),
                Preview = static view => view.Box.Map(static box => (Figure)box),
            };

        private Session<(PlotPlane Plane, Option<Figure.Region> Box)> Panning(PlotFrame frame, HashMap<Metric, float> metrics) =>
            Tracking(frame, (start, gesture) => gesture.SwitchPartially(
                (Viewport: this, Frame: frame, Start: start, Metrics: metrics),
                @default: static (_, _) => Option<PlotPlane>.None,
                press: static (held, press) => held.Viewport.Panned(held.Frame, held.Start, press.From, press.At),
                scroll: static (held, slid) => held.Viewport.Wheeled(held.Frame, slid, held.Metrics, notch: false).Bind(move => move(held.Start)),
                magnify: static (held, pinched) => held.Frame.At(pinched.About).Bind(about => held.Viewport.Zoomed(held.Start, about, 1f / (1f + pinched.Magnification)))));

        // --- [MOVES]

        private Option<Func<PlotPlane, Option<PlotPlane>>> Wheeled(PlotFrame frame, Gesture.Scroll scroll, HashMap<Metric, float> metrics, bool notch) =>
            (MathF.Abs(scroll.Delta.X) > MathF.Abs(scroll.Delta.Y) ? PhysicalAxis.Horizontal : PhysicalAxis.Vertical) switch {
                var along => scroll.Modifiers.Primary || (notch && along.Map(horizontal: false, vertical: Free.Contains(along)))
                    ? Some(Notched(frame, scroll.At, metrics[Metric.Notch], -along.Of(scroll.Delta) / metrics[Metric.Extent]))
                    : Free.Contains(along) ? Some<Func<PlotPlane, Option<PlotPlane>>>(start => Panned(frame, start, scroll.At, scroll.At + (SKPoint)scroll.Delta)) : None,
            };

        private Option<PlotPlane> Panned(PlotFrame frame, PlotPlane start, SKPoint from, SKPoint to) =>
            frame.Delta(from, to).Bind(travel => start.Shifted(-Free.Fold(Vector2.Zero, (held, along) => along.With(held, along.Of(travel)))));

        private Option<PlotPlane> Zoomed(PlotPlane start, Vector2 about, float factor) =>
            from rooms in start.Dimensions.Filter(free => Free.Contains(free.Along)).Traverse(free => Room(free)).As()
            from room in Limits.Validated(rooms.Map(static limit => limit.Min).Max(0f), rooms.Map(static limit => limit.Max).Min(float.PositiveInfinity)).ToOption()
            let held = room.Clamped(factor)
            from plane in held.CompareTo(1f) is 0 ? Some(start) : start.Scaled(about, about, Free.Fold(Vector2.One, (factors, along) => along.With(factors, held)))
            select plane;

        private Func<PlotPlane, Option<PlotPlane>> Notched(PlotFrame frame, SKPoint about, float notch, float notches) =>
            start => frame.At(about).Bind(at => Zoomed(start, at, MathF.Pow(notch, notches)));

        private Func<PlotPlane, Option<PlotPlane>> Navigated(PlotFrame frame, SKPoint center, Bearing toward, float step) =>
            start => toward.Granularity.Distance(step, frame.Pixels(toward.Along)).Match(
                Some: distance => Panned(frame, start, center + (SKPoint)(toward.Along.With(Vector2.Zero, toward.Sign) * distance), center),
                None: () => from bound in Bounds.Bind(bounds => bounds.Dimension(toward.Along))
                            from held in start.Dimension(toward.Along)
                            where Free.Contains(toward.Along)
                            from plane in start.Shifted(toward.Along.With(Vector2.Zero, toward.Sign * frame.Sign(toward.Along) > 0 ? bound.Domain.High - held.Domain.High : bound.Domain.Low - held.Domain.Low))
                            select plane);

        private Option<PlotPlane> Fitted(PlotFrame frame, PlotPlane start, Figure.Region box) {
            Seq<(PlaneAxis Free, AxisRange Target)> targets = start.Dimensions.Map(free => (free, free.Along.Map(horizontal: box.First, vertical: box.Second).IfNone(free.Axis.Domain)));
            return from centered in start.Shifted(targets.Fold(Vector2.Zero, static (held, target) => target.Free.Along.With(held, target.Target.Mid - target.Free.Axis.Domain.Mid)))
                   from zoomed in Zoomed(centered, targets.Fold(Vector2.Zero, static (held, target) => target.Free.Along.With(held, target.Target.Mid)), targets.Fold(0f, static (most, target) => float.Max(most, MathF.Abs(target.Target.Span / target.Free.Axis.Domain.Span))))
                   from plane in Bounded(frame, zoomed)
                   select plane;
        }

        private Option<PlotPlane> Cycled(PlotFrame frame, PlotPlane start) =>
            start != Rest
                ? Some(Rest)
                : from natural in Natural
                  from factors in start.Dimensions.Filter(free => Free.Contains(free.Along))
                      .Traverse(free => Room(free).Map(room => (free.Along, Factor: room.Clamped(frame.Pixels(free.Along) * free.Along.Of(natural) / MathF.Abs(free.Axis.Domain.Span))))).As()
                  from plane in start.Scaled(start.Center, start.Center, factors.Fold(Vector2.One, static (held, factor) => factor.Along.With(held, factor.Factor)))
                  select plane;

        private Option<PlotPlane> Bounded(PlotFrame frame, PlotPlane plane) =>
            Bounds.Match(
                    Some: bounds => plane.Shifted(plane.Dimensions.Fold(Vector2.Zero, (delta, held) => bounds.Dimension(held.Along).Map(bound => held.Along.With(delta, Inside(held.Axis.Domain, bound.Domain))).IfNone(delta))),
                    None: () => Some(plane))
                .Filter(held => (frame with { Plane = held }).Resolves);

        // --- [READS]

        private Option<Limits> Room(PlaneAxis held) =>
            from rest in Rest.Dimension(held.Along)
            let span = MathF.Abs(held.Axis.Domain.Span)
            let ratio = span / MathF.Abs(rest.Domain.Span)
            from room in Limits.Validated(Magnification.Min / ratio, float.Min(Magnification.Max / ratio, Bounds.Bind(bounds => bounds.Dimension(held.Along)).Map(bound => MathF.Abs(bound.Domain.Span) / span).IfNone(float.PositiveInfinity))).ToOption()
            select room;

        private static float Inside(AxisRange domain, AxisRange bound) => float.Max(bound.Low - domain.Low, 0f) + float.Min(bound.High - domain.High, 0f);

        // --- [DRAWING]

        public View Zooms(PlotPlane view) =>
            new View.Flex(PhysicalAxis.Horizontal, [
                Zoom(Roles.Add, Some(Stepped(view, -1))),
                Zoom(Roles.Subtract, Some(Stepped(view, 1))),
                Zoom(Roles.Revert, view == Rest ? None : Some<Part>(new Press(_ => Edited(Edit.Of(EditPhase.Commit, Rest))))),
            ]) {
                Role = Roles.Hud,
            };

        internal View.Plot Overview(PlotPlane view, Seq<View> content) => new(Rest, content.Add(new Brush(view, Edited).Drawn(Rest))) { Role = Roles.Well, Aspect = Rest.Proportion };

        private static View.Flex Zoom(Key glyph, Option<Part> part) =>
            new(PhysicalAxis.Horizontal, [new View.Box { Role = glyph }]) {
                Role = Roles.Button,
                Conditions = part.IsSome ? [Condition.Borderless] : [Condition.Borderless, Condition.Disabled],
                Part = part,
                Accessible = new Accessible { Role = AccessRole.Button },
            };

        // --- [STEPS]

        public Part Stepped(PlotPlane view, int notches) => new Stepping(this, view, notches);

        private sealed record Stepping(Viewport Owner, PlotPlane View, int Notches) : Part {
            internal override Option<Reply> Began(Gesture gesture, HitTarget aim) => Pressed(aim).Began(gesture, aim);

            internal override Option<Reply> Performed(Gesture gesture, HitTarget aim) => Pressed(aim).Performed(gesture, aim);

            private Press Pressed(HitTarget aim) =>
                new(_ => Owner.Zoomed(View, View.Center, MathF.Pow(aim.Owner.Layout.Styled(Roles.Handle).Metrics[Metric.Notch], Notches))
                    .Map(plane => Owner.Edited(Edit.Of(EditPhase.Commit, plane))).IfNone(unitIO));
        }
    }

    public sealed record Trace(Func<Edit<Seq<PointerSample>>, IO<Unit>> Edited) : Part {
        public bool Closed { get; init; }

        internal override Option<Reply> Began(Gesture gesture, HitTarget aim) =>
            from press in Optional(gesture as Gesture.Press)
            from frame in aim.Frame
            from reply in Tracing(frame).Began(press)
            select reply;

        private Session<Seq<PointerSample>> Tracing(PlotFrame frame) =>
            new([], [Receiver<Seq<PointerSample>>.Of(static points => points, edit => Some(Edited(edit)))], CursorShape.Crosshair) {
                Step = (_, gesture) => gesture is Gesture.Press press ? Some(press.Trail.Choose(sample => frame.At(sample.At).Map(at => sample with { At = at }))) : None,
                Released = points => Closed && points is [var first, _, _, ..] ? points.Add(first) : points,
            };
    }

    public sealed record Selecting(Func<Edit<TextRange>, IO<Unit>> Edited) : Part {
        internal override Option<Reply> Began(Gesture gesture, HitTarget aim) =>
            from press in Optional(gesture as Gesture.Press)
            from text in aim.Owner.Layout.TextNode
            from reply in (Spanning(text) with { Step = (_, next) => next is Gesture.Press moved ? Some(Spanned(text, aim.Owner.Bounds.Location, moved)) : None }).Began(press)
            select reply;

        internal override Option<Reply> Performed(Gesture gesture, HitTarget aim) =>
            from perform in Optional(gesture as Gesture.Perform)
            where perform.Act == Act.SelectAll
            from text in aim.Owner.Layout.TextNode
            from reply in Spanning(text).Stepped(_ => Some(Whole(text)))
            select reply;

        private Session<TextRange> Spanning(LayoutNode.Text text) =>
            new(text.Typeset.Text.Selected.IfNone(new TextRange(0)), [Receiver<TextRange>.Of(static range => range, edit => Some(Edited(edit)))], CursorShape.Text);

        private static TextRange Spanned(LayoutNode.Text text, SKPoint origin, Gesture.Press press) =>
            (Caret(text, origin, press.From), Caret(text, origin, press.At), press.Clicks) switch {
                (var from, var to, <= 1) => new TextRange(from, to),
                (var from, var to, 2) => Worded(text.Typeset.Source, int.Min(from, to), int.Max(from, to)),
                _ => Whole(text),
            };

        private static TextRange Whole(LayoutNode.Text text) => new(0, text.Typeset.Source.Length);

        private static int Caret(LayoutNode.Text text, SKPoint origin, SKPoint at) =>
            text.Typeset.HitTest(new SKPoint(at.X - origin.X - text.Origin.X, at.Y - origin.Y - text.Origin.Y)).ClosestCodePointIndex;

        private static TextRange Worded(TextBlock block, int low, int high) =>
            toSeq(block.WordBoundaryIndicies) switch {
                var stops => new(stops.Filter(stop => stop <= low).Last.IfNone(0), stops.Find(stop => stop > high).IfNone(block.Length)),
            };
    }
}
