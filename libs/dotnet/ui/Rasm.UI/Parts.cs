using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Part {
    // --- [GESTURES]
    internal Reply Began(Gesture gesture, HitTarget aim) =>
        Switch(
            (Gesture: gesture, Aim: aim),
            press: static (held, press) => press.Begin(held.Gesture, held.Aim),
            pad: static (held, pad) => pad.Handles.Began(held.Gesture, held.Aim),
            choice: static (held, choice) => choice.Begin(held.Gesture, held.Aim),
            viewport: static (held, viewport) => viewport.Begin(held.Gesture, held.Aim),
            trace: static (held, trace) => trace.Begin(held.Gesture, held.Aim),
            selecting: static (held, selecting) => selecting.Begin(held.Gesture, held.Aim));

    internal Reply Performed(Gesture gesture, HitTarget aim) =>
        Switch(
            (Gesture: gesture, Aim: aim),
            press: static (held, press) => press.Perform(held.Gesture, held.Aim),
            pad: static (held, pad) => pad.Handles.Performed(held.Gesture, held.Aim),
            choice: static (held, choice) => choice.Perform(held.Gesture, held.Aim),
            viewport: static (held, viewport) => viewport.Perform(held.Gesture, held.Aim),
            trace: Reply.Ignore,
            selecting: static (held, selecting) => selecting.Perform(held.Gesture, held.Aim));

    internal bool Offers(LayoutNode owner, Seq<Key> path) =>
        SwitchPartially(
            (Owner: owner, Path: path),
            @default: static (held, _) => held.Path == held.Owner.Path,
            pad: static (held, _) => HandleLayer.Offers(held.Owner, held.Path),
            choice: static (held, choice) => held.Path == held.Owner.Path || choice.Tiles(held.Owner).Exists(tile => tile.Layout.Path == held.Path));

    // --- [WATCH]
    internal Option<Seq<Key>> Revealed(LayoutNode layout) =>
        SwitchPartially(
            layout,
            @default: static (_, _) => Option<Seq<Key>>.None,
            pad: static (held, pad) => pad.Handles.Revealed(held),
            choice: static (held, choice) => choice.Value.Lead.Bind(lead => choice.Paths(held, [lead]).Head));

    internal Seq<IO<Unit>> Watched(Option<HoverState> before, HoverState after) =>
        SwitchPartially(
            (Before: before, After: after),
            @default: static (_, _) => Seq<IO<Unit>>(),
            choice: static (held, choice) => choice.Watch(held.Before, held.After));

    // --- [CASES]
    public sealed record Press(Func<SKRect, IO<Unit>> Run) : Part {
        public Func<SKRect, Option<IO<Unit>>> Secondary { get; init; } = static _ => None;

        public Node Drawn(Node face) => face with { Part = this, Accessible = face.Accessible.Combine(new Accessible { Role = AccessRole.Button }) };

        internal Reply Begin(Gesture gesture, HitTarget aim) =>
            gesture.SwitchPartially((Press: this, Aim: aim), @default: Reply.Ignore, press: static (held, _) => new Reply.Accepted([], Some(held.Press.Holding(held.Aim))));

        internal Reply Perform(Gesture gesture, HitTarget aim) =>
            gesture.SwitchPartially(
                (Press: this, aim.Bounds),
                @default: Reply.Ignore,
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    held,
                    @default: static (_, _) => new Reply.Ignored(),
                    press: static (state, _) => new Reply.Accepted([state.Press.Run(state.Bounds)], None),
                    confirm: static (state, _) => new Reply.Accepted([state.Press.Run(state.Bounds)], None),
                    showMenu: static (state, _) => state.Press.Secondary(state.Bounds).Match(Some: static effect => (Reply)new Reply.Accepted([effect], None), None: static () => new Reply.Ignored())));

        private Track Holding(HitTarget aim) =>
            new(_ => new Reply.Accepted([], Some(Holding(aim))),
                gesture => gesture.SwitchPartially(
                    (Press: this, Aim: aim),
                    @default: static (_, _) => Seq<IO<Unit>>(),
                    press: static (held, press) => held.Aim.Bounds.Contains(press.At) ? Seq(held.Press.Run(held.Aim.Bounds)) : []),
                static () => []);
    }

    public sealed record Pad(HandleLayer Handles) : Part;

    public sealed record Choice(Seq<Key> Items, Selection Value, Func<Edit<Selection>, IO<Unit>> Edited) : Part {
        // --- [STATE]
        public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Reorder { get; init; }
        public Func<Seq<Key>, SKRect, Option<IO<Unit>>> Menu { get; init; } = static (_, _) => None;
        public Func<Option<Key>, Option<IO<Unit>>> Hover { get; init; } = static _ => None;
        public Func<HashSet<Key>, Option<IO<Unit>>> Viewed { get; init; } = static _ => None;

        private AccessRole Composite => Value.Map(one: AccessRole.RadioGroup, many: AccessRole.List);

        private Seq<Receiver<State>> Receivers =>
            Seq(Receiver<State>.Of(static held => held.Value, edit => Some(Edited(edit)))) + Reorder.Map(static reorder => Receiver<State>.Of(static held => held.Order, edit => Some(reorder(edit)))).ToSeq();

        // --- [DRAW]
        public Node Drawn(Func<Key, Node> item, Func<Seq<Node>, Node> container) {
            Node held = container(Items.Map(key => Member(item(key), key)));
            return held with { Part = this, Accessible = held.Accessible.Combine(new Accessible { Role = Composite }) };
        }

        public Node.Lazy Drawn(Func<Key, Node> item, PhysicalAxis axis, Tracks across, float extent, HashSet<Key> realized) =>
            new(axis, across, extent, Items) {
                Realized = toHashMap(Items.Filter(realized.Contains).Map(key => (key, Member(item(key), key)))),
                Part = this,
                Accessible = new Accessible { Role = Composite },
            };

        private Node Member(Node node, Key key) =>
            node with {
                Key = key,
                Role = node.Role | Some(Role.ListRow),
                Conditions = Value.Holds(key) ? node.Conditions.TryAdd(Condition.Selected) : node.Conditions,
                Accessible = node.Accessible.Combine(new Accessible { Role = Composite.Owns.IfNone(AccessRole.Radio) }),
            };

        // --- [GESTURES]
        internal Reply Begin(Gesture gesture, HitTarget aim) =>
            gesture.SwitchPartially(
                (Choice: this, Aim: aim),
                @default: Reply.Ignore,
                press: static (held, press) => held.Choice.Item(held.Aim.Hit, held.Aim.Layout).Match(
                    Some: key => held.Choice.Pressing(key, held.Aim).Began(press),
                    None: () => held.Choice.Value.Switch(
                        (held.Choice, held.Aim, Press: press),
                        one: Reply.Ignore,
                        many: static (state, _) => state.Choice.Marquee(state.Aim).Began(state.Press))));

        internal Reply Perform(Gesture gesture, HitTarget aim) =>
            gesture.SwitchPartially(
                (Choice: this, Aim: aim),
                @default: Reply.Ignore,
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    (held.Choice, held.Aim, Perform: perform),
                    @default: static (_, _) => new Reply.Ignored(),
                    press: static (state, _) => state.Choice.Selected(state.Aim, state.Perform),
                    confirm: static (state, _) => state.Choice.Selected(state.Aim, state.Perform),
                    showMenu: static (state, _) => (from key in state.Choice.Item(state.Aim.Hit, state.Aim.Layout)
                                                    from tile in state.Choice.Framed(state.Aim).Find(tile => tile.Item == key)
                                                    from effect in state.Choice.Menu(state.Choice.Moving(state.Choice.Value, key), tile.Frame)
                                                    select (Reply)new Reply.Accepted([effect], None))
                        .IfNone(static () => new Reply.Ignored()),
                    navigate: static (state, navigate) => navigate.Toward.Granularity.Distance(1f, 0f).Match(
                            Some: stride => stride > 0f ? state.Choice.Adjacent(state.Aim, navigate.Toward) : None,
                            None: () => navigate.Toward.Sign < 0 ? state.Choice.Items.Head : state.Choice.Items.Last)
                        .Match(
                            Some: key => state.Choice.Stepping(start => start with { Value = start.Value.Navigated(state.Choice.Items, key, state.Perform.Modifiers.Shift) }).Stepped(state.Perform),
                            None: static () => new Reply.Ignored()),
                    selectAll: static (state, _) => state.Choice.Stepping(start => start with { Value = start.Value.All(state.Choice.Items) }).Stepped(state.Perform)));

        internal Seq<IO<Unit>> Watch(Option<HoverState> before, HoverState after) {
            Option<Key> hovered = Hovered(after);
            HashSet<Key> seen = Seen(after);
            return (before.Bind(Hovered) == hovered ? [] : Hover(hovered).ToSeq()) + (before.Map(Seen) == Some(seen) ? [] : Viewed(seen).ToSeq());
        }

        private Session<State> Pressing(Key key, HitTarget aim) =>
            new(new State(Value, Items), Receivers, Reorder.IsSome ? CursorShape.Grab : CursorShape.Default) {
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(Pressed(start, key, press, aim)) : None,
                Dragging = Reorder.Map(static _ => CursorShape.Grabbing),
                Lifted = state => aim.Layout.Node.Lift.Map(lift => Moving(state.Value, key) switch {
                    var moving => new Lifting(new Transfer.Keys(Paths(aim.Layout, moving)), lift.Payload(moving)),
                }),
            };

        private State Pressed(State start, Key key, Gesture.Press press, HitTarget aim) =>
            start.Value.Pressed(Items, key, press.Modifiers) switch {
                var selection when press.Dragged.IsNone || (aim.Layout.Node.Lift.IsSome && !aim.Scene.Covers(press.At)) => new State(selection, Items),
                var selection => new State(selection, Reorder.IsSome ? Ordered(Moving(selection, key), At(aim, press.At)) : Items),
            };

        private Session<State> Marquee(HitTarget aim) =>
            new(new State(Value, Items), Receivers, CursorShape.Default) {
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(start with { Value = start.Value.Enclosed(Items, Covered(aim, press), press.Modifiers) }) : None,
            };

        private Session<State> Stepping(Func<State, State> next) => new(new State(Value, Items), Receivers, CursorShape.Default) { Step = (start, gesture) => gesture is Gesture.Perform ? Some(next(start)) : None };

        private Reply Selected(HitTarget aim, Gesture.Perform perform) =>
            Item(aim.Hit, aim.Layout).Match(
                Some: key => Stepping(start => start with { Value = start.Value.Pressed(Items, key, perform.Modifiers) }).Stepped(perform),
                None: static () => new Reply.Ignored());

        private Option<Key> Adjacent(HitTarget aim, NavigationDirection toward) =>
            Value.Lead.Match(
                Some: lead => {
                    Seq<Candidate<Key>> tiles = Framed(aim);
                    return tiles.Find(tile => tile.Item == lead).Bind(origin => Routing.Toward(origin.Frame, tiles.Filter(tile => tile.Item != lead), toward));
                },
                None: () => Items.Head);

        // --- [READ]
        internal Option<Key> Item(Seq<Key> over, LayoutNode layout) => Item(over, layout, toHashSet(Items));

        internal Seq<TileNode> Tiles(LayoutNode layout) => Tiles(layout, toHashSet(Items));

        internal Seq<Seq<Key>> Paths(LayoutNode layout, Seq<Key> keys) {
            HashMap<Key, Seq<Key>> tiles = toHashMap(Tiles(layout).Map(static tile => (tile.Key, tile.Layout.Path)));
            return keys.Choose(key => tiles.Find(key) | layout.Node.Windowed.Filter(lazy => lazy.Items.Exists(item => item == key)).Map(_ => layout.Path.Add(key)));
        }

        private Seq<Candidate<Key>> Framed(HitTarget aim) => Tiles(aim.Layout).Choose(tile => aim.Scene.Frame(tile.Layout.Path).Map(frame => new Candidate<Key>(tile.Key, frame))).Strict();

        private Seq<Key> Moving(Selection selection, Key key) => selection.Holds(key) ? Items.Filter(selection.Holds) : [key];

        private Option<Key> At(HitTarget aim, SKPoint at) => Framed(aim).Find(tile => tile.Frame.Contains(at)).Map(static tile => tile.Item);

        private HashSet<Key> Covered(HitTarget aim, Gesture.Press press) {
            SKRect enclosed = new SKRect(press.From.X, press.From.Y, press.At.X, press.At.Y).Standardized;
            return press.Dragged.IsNone ? [] : toHashSet(Framed(aim).Filter(tile => tile.Frame.IntersectsWith(enclosed)).Map(static tile => tile.Item));
        }

        private Seq<Key> Ordered(Seq<Key> moving, Option<Key> under) {
            HashSet<Key> moved = toHashSet(moving);
            return (from target in under.Filter(key => !moved.Contains(key))
                    from place in Selection.Position(Items, target)
                    from first in moving.Head.Bind(key => Selection.Position(Items, key))
                    let rest = Items.Filter(key => !moved.Contains(key))
                    from slot in Selection.Position(rest, target)
                    let index = place > first ? slot + 1 : slot
                    select rest.Take(index) + moving + rest.Skip(index)).IfNone(Items);
        }

        private Option<Key> Hovered(HoverState sight) => sight.Hover.Choose(path => Item(path, sight.Layout)).Head;

        private HashSet<Key> Seen(HoverState sight) =>
            toHashSet(sight.Layout.Node.Windowed.Match(
                Some: lazy => Layouts.Visible(sight.Scene, sight.Layout, lazy),
                None: () => Tiles(sight.Layout).Filter(tile => sight.InView(tile.Layout.Path)).Map(static tile => tile.Key)));

        private static Option<Key> Item(Seq<Key> over, LayoutNode layout, HashSet<Key> members) =>
            layout.Children.Find(child => over.Take(child.Path.Count) == child.Path).Bind(child => Tile(child, members) | Item(over, child, members));

        private static Seq<TileNode> Tiles(LayoutNode layout, HashSet<Key> members) =>
            layout.Children.Bind(child => Tile(child, members).Match(Some: key => Seq(new TileNode(key, child)), None: () => Tiles(child, members)));

        private static Option<Key> Tile(LayoutNode layout, HashSet<Key> members) => layout.Node.Key.Filter(members.Contains);

        private sealed record State(Selection Value, Seq<Key> Order);

        internal sealed record TileNode(Key Key, LayoutNode Layout);
    }

    public sealed record Viewport(PlotPlane Rest, Func<Edit<PlotPlane>, IO<Unit>> Edited) : Part {
        // --- [STATE]
        public HashSet<PhysicalAxis> Free { get; init; } = toHashSet(PhysicalAxis.Items);
        public Option<PlotPlane> Bounds { get; init; }
        public bool Boxed { get; init; }
        public Magnification Magnification { get; init; } = Magnification.Free;

        // --- [GESTURES]
        internal Reply Begin(Gesture gesture, HitTarget aim) =>
            aim.Frame.Match(
                Some: frame => gesture.SwitchPartially(
                    (Viewport: this, Frame: frame),
                    @default: Reply.Ignore,
                    press: static (held, press) => (press.Clicks >= 2
                        ? held.Viewport.Tracking(held.Frame, (_, gesture) => gesture is Gesture.Press ? Some(held.Viewport.Rest) : None)
                        : held.Viewport.Boxed
                            ? held.Viewport.Boxing(held.Frame)
                            : held.Viewport.Tracking(held.Frame, (start, gesture) => gesture is Gesture.Press moved ? held.Viewport.Panned(held.Frame, start, moved.From, moved.At) : None)).Began(press),
                    scroll: static (held, scroll) => held.Viewport.Tracking(held.Frame, (start, gesture) => gesture is Gesture.Scroll slid ? held.Viewport.Panned(held.Frame, start, slid.At, slid.At + (SKPoint)slid.Delta) : None).Began(scroll),
                    magnify: static (held, magnify) => held.Viewport.Tracking(held.Frame, (start, gesture) => gesture is Gesture.Magnify pinched
                        ? held.Frame.At(pinched.About).Bind(about => held.Viewport.Zoomed(start, about, 1f / (1f + pinched.Magnification)))
                        : None).Began(magnify)),
                None: static () => new Reply.Ignored());

        internal Reply Perform(Gesture gesture, HitTarget aim) =>
            aim.Frame.Match(
                Some: frame => gesture.SwitchPartially(
                    (Viewport: this, Frame: frame, Center: new SKPoint(aim.Bounds.MidX, aim.Bounds.MidY), Metrics: HandleLayer.Metrics(aim.Layout, None)),
                    @default: Reply.Ignore,
                    scroll: static (held, scroll) => held.Viewport.Tracking(held.Frame, (start, gesture) => gesture is Gesture.Scroll notched
                        ? held.Frame.At(notched.At).Bind(about => held.Viewport.Zoomed(start, about, MathF.Pow(held.Metrics[Metric.Notch], -notched.Delta.Y / held.Metrics[Metric.Extent])))
                        : None).Stepped(scroll),
                    perform: static (held, perform) => perform.Act.SwitchPartially(
                            (held.Viewport, held.Frame, held.Center, held.Metrics, Perform: perform),
                            @default: static (_, _) => Option<Func<PlotPlane, Option<PlotPlane>>>.None,
                            increment: static (state, _) => Some(state.Viewport.Notched(state.Frame, state.Center, state.Metrics, state.Perform.Modifiers, -1f)),
                            decrement: static (state, _) => Some(state.Viewport.Notched(state.Frame, state.Center, state.Metrics, state.Perform.Modifiers, 1f)),
                            reset: static (state, _) => Some<Func<PlotPlane, Option<PlotPlane>>>(_ => Some(state.Viewport.Rest)),
                            navigate: static (state, navigate) => Some(state.Viewport.Navigated(state.Frame, state.Center, navigate.Toward, state.Metrics[Metric.Extent])))
                        .Match(
                            Some: move => held.Viewport.Tracking(held.Frame, (start, gesture) => gesture is Gesture.Perform ? move(start) : None).Stepped(perform),
                            None: static () => new Reply.Ignored())),
                None: static () => new Reply.Ignored());

        // --- [SESSIONS]
        private Session<View> Tracking(PlotFrame frame, Func<PlotPlane, Gesture, Option<PlotPlane>> step) =>
            new(new View(frame.Plane), [Receiver<View>.Of(static view => view.Plane, edit => Some(Edited(edit)))], CursorShape.Grab) {
                Step = (start, gesture) => step(start.Plane, gesture).Bind(plane => Settled(frame, plane)).Map(static plane => new View(plane)),
                Dragging = CursorShape.Grabbing,
            };

        private Session<View> Boxing(PlotFrame frame) =>
            Tracking(frame, static (_, _) => None) with {
                Cursor = CursorShape.Crosshair,
                Dragging = None,
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(start with { Box = press.Enclosed(frame).Map(box => box with { First = box.First.Filter(_ => Free.Contains(PhysicalAxis.Horizontal)), Second = box.Second.Filter(_ => Free.Contains(PhysicalAxis.Vertical)) }) }) : None,
                Released = view => view.Box.Bind(box => Fitted(frame, view.Plane, box)).Map(static plane => new View(plane)).IfNone(view),
                Preview = static view => view.Box.Map(static box => (Figure)box),
            };

        // --- [MOVES]
        private Option<PlotPlane> Panned(PlotFrame frame, PlotPlane start, SKPoint from, SKPoint to) =>
            frame.Delta(from, to).Bind(travel => start.Shifted(-Free.Fold(Vector2.Zero, (held, along) => along.With(held, along.Of(travel)))));

        private Option<PlotPlane> Zoomed(PlotPlane start, Vector2 about, float factor) =>
            from rooms in start.Dimensions.Filter(free => Free.Contains(free.Along)).Traverse(free => Room(free)).As()
            from room in Limits.Validated(rooms.Fold(0f, static (floor, next) => float.Max(floor, next.Min)), rooms.Fold(float.PositiveInfinity, static (ceiling, next) => float.Min(ceiling, next.Max))).ToOption()
            let held = room.Clamped(factor)
            from plane in held.CompareTo(1f) is 0 ? Some(start) : start.Scaled(about, about, Free.Fold(Vector2.One, (factors, along) => along.With(factors, held)))
            select plane;

        private Func<PlotPlane, Option<PlotPlane>> Notched(PlotFrame frame, SKPoint center, HashMap<Metric, float> metrics, KeyModifiers modifiers, float sign) =>
            start => frame.At(center).Bind(about => Zoomed(start, about, MathF.Pow(metrics[Metric.Notch], sign * (modifiers.Primary ? metrics[Metric.Fine] : 1f))));

        private Func<PlotPlane, Option<PlotPlane>> Navigated(PlotFrame frame, SKPoint center, NavigationDirection toward, float step) =>
            start => toward.Granularity.Distance(step, frame.Pixels(toward.Along)).Match(
                Some: distance => Panned(frame, start, center + (SKPoint)(toward.Vector * distance), center),
                None: () => from bound in Bounds.Bind(bounds => bounds.Dimension(toward.Along))
                            from held in start.Dimension(toward.Along)
                            where Free.Contains(toward.Along)
                            from plane in start.Shifted(toward.Along.With(Vector2.Zero, toward.ValueSign(frame) > 0 ? bound.Domain.High - held.Domain.High : bound.Domain.Low - held.Domain.Low))
                            select plane);

        private Option<PlotPlane> Fitted(PlotFrame frame, PlotPlane start, Figure.Region box) {
            Seq<(PlaneAxis Free, AxisRange Target)> targets = start.Dimensions.Map(free => (free, free.Along.Map(horizontal: box.First, vertical: box.Second).IfNone(free.Axis.Domain)));
            Vector2 center = targets.Fold(Vector2.Zero, static (held, target) => target.Free.Along.With(held, Midpoint(target.Target)));
            return from centered in start.Shifted(targets.Fold(Vector2.Zero, static (held, target) => target.Free.Along.With(held, Midpoint(target.Target) - Midpoint(target.Free.Axis.Domain))))
                   from zoomed in Zoomed(centered, center, targets.Fold(0f, static (most, target) => float.Max(most, MathF.Abs(target.Target.Span / target.Free.Axis.Domain.Span))))
                   from plane in Settled(frame, zoomed)
                   select plane;
        }

        private Option<PlotPlane> Settled(PlotFrame frame, PlotPlane plane) =>
            Bounds.Match(
                    Some: bounds => plane.Shifted(plane.Dimensions.Fold(Vector2.Zero, (delta, held) => bounds.Dimension(held.Along).Map(bound => held.Along.With(delta, Inside(held.Axis.Domain, bound.Domain))).IfNone(delta))),
                    None: () => Some(plane))
                .Filter(held => (frame with { Plane = held }).Resolves);

        // --- [READ]
        private Option<Limits> Room(PlaneAxis held) =>
            from rest in Rest.Dimension(held.Along)
            let span = MathF.Abs(held.Axis.Domain.Span)
            let ratio = span / MathF.Abs(rest.Domain.Span)
            from room in Limits.Validated(Magnification.Min / ratio, float.Min(Magnification.Max / ratio, Bounds.Bind(bounds => bounds.Dimension(held.Along)).Map(bound => MathF.Abs(bound.Domain.Span) / span).IfNone(float.PositiveInfinity))).ToOption()
            select room;

        private static float Midpoint(AxisRange range) => (range.From + range.To) / 2f;

        private static float Inside(AxisRange domain, AxisRange bound) => float.Max(bound.Low - domain.Low, 0f) + float.Min(bound.High - domain.High, 0f);

        private sealed record View(PlotPlane Plane) {
            internal Option<Figure.Region> Box { get; init; }
        }
    }

    public sealed record Trace(Func<Edit<Seq<TracePoint>>, IO<Unit>> Edited) : Part {
        public bool Closed { get; init; }

        internal Reply Begin(Gesture gesture, HitTarget aim) =>
            aim.Frame.Match(
                Some: frame => gesture.SwitchPartially((Trace: this, Frame: frame), @default: Reply.Ignore, press: static (held, press) => held.Trace.Tracing(held.Frame).Began(press)),
                None: static () => new Reply.Ignored());

        private Session<Seq<TracePoint>> Tracing(PlotFrame frame) =>
            new([], [Receiver<Seq<TracePoint>>.Of(static points => points, edit => Some(Edited(edit)))], CursorShape.Crosshair) {
                Step = (_, gesture) => gesture is Gesture.Press press ? Some(press.Trail.Choose(sample => frame.At(sample.At).Map(at => new TracePoint(at, sample.Stylus)))) : None,
                Released = points => Closed && points is [var first, _, _, ..] ? points.Add(first) : points,
            };
    }

    public sealed record Selecting(Func<Edit<TextRange>, IO<Unit>> Edited) : Part {
        internal Reply Begin(Gesture gesture, HitTarget aim) =>
            aim.Layout.TextNode.Match(
                Some: text => gesture.SwitchPartially(
                    (Selecting: this, Text: text, Origin: aim.Bounds.Location),
                    @default: Reply.Ignore,
                    press: static (held, press) => held.Selecting.Spanning(held.Text, (_, gesture) => gesture is Gesture.Press moved ? Some(Spanned(held.Text, held.Origin, moved)) : None).Began(press)),
                None: static () => new Reply.Ignored());

        internal Reply Perform(Gesture gesture, HitTarget aim) =>
            aim.Layout.TextNode.Match(
                Some: text => gesture.SwitchPartially(
                    (Selecting: this, Text: text),
                    @default: Reply.Ignore,
                    perform: static (held, perform) => perform.Act.SwitchPartially(
                        (held.Selecting, held.Text, Perform: perform),
                        @default: static (_, _) => new Reply.Ignored(),
                        selectAll: static (state, _) => state.Selecting.Spanning(state.Text, (_, gesture) => gesture is Gesture.Perform ? Some(new TextRange(0, state.Text.Typeset.Lines.Length)) : None).Stepped(state.Perform))),
                None: static () => new Reply.Ignored());

        private Session<TextRange> Spanning(LayoutNode.Text text, Func<TextRange, Gesture, Option<TextRange>> step) =>
            new(text.Source.Selected.IfNone(new TextRange(0)), [Receiver<TextRange>.Of(static range => range, edit => Some(Edited(edit)))], CursorShape.Text) { Step = step };

        private static TextRange Spanned(LayoutNode.Text text, SKPoint origin, Gesture.Press press) =>
            (Caret(text, origin, press.From), Caret(text, origin, press.At), press.Clicks) switch {
                (var from, var to, <= 1) => new TextRange(from, to),
                (var from, var to, 2) => Worded(text.Typeset.Lines, int.Min(from, to), int.Max(from, to)),
                _ => new TextRange(0, text.Typeset.Lines.Length),
            };

        private static int Caret(LayoutNode.Text text, SKPoint origin, SKPoint at) =>
            int.Max(0, text.Typeset.Lines.HitTest(at.X - origin.X - text.Origin.X, at.Y - origin.Y - text.Origin.Y).ClosestCodePointIndex);

        private static TextRange Worded(TextBlock block, int low, int high) =>
            toSeq(block.WordBoundaryIndicies) switch {
                var stops => new(stops.Filter(stop => stop <= low).Last.IfNone(0), stops.Find(stop => stop > high).IfNone(block.Length)),
            };
    }
}
