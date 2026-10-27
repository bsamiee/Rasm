using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Part {
    // --- [ANSWER]
    internal Reply Began(Gesture gesture, Aim aim) =>
        Switch(
            (Gesture: gesture, Aim: aim),
            press: static (held, press) => press.Begin(held.Gesture, held.Aim),
            pad: static (held, pad) => pad.Handles.Began(held.Gesture, held.Aim),
            choice: static (held, choice) => choice.Begin(held.Gesture, held.Aim),
            viewport: static (held, viewport) => viewport.Begin(held.Gesture, held.Aim));

    internal Reply Performed(Gesture gesture, Aim aim) =>
        Switch(
            (Gesture: gesture, Aim: aim),
            press: static (held, press) => press.Perform(held.Gesture, held.Aim),
            pad: static (held, pad) => pad.Handles.Performed(held.Gesture, held.Aim),
            choice: static (held, choice) => choice.Perform(held.Gesture, held.Aim),
            viewport: static (held, viewport) => viewport.Perform(held.Gesture, held.Aim));

    internal Option<IO<Unit>> Drop(Seq<Key> dragged, Aim aim) =>
        SwitchPartially(
            (Dragged: dragged, Aim: aim),
            @default: static (_, _) => Option<IO<Unit>>.None,
            choice: static (held, choice) => choice.Dropped(held.Dragged, choice.Item(held.Aim.Over, held.Aim.Laid)));

    internal bool Offers(Laid owner, Seq<Key> path) =>
        SwitchPartially(
            (Owner: owner, Path: path),
            @default: static (held, _) => held.Path == held.Owner.Path,
            pad: static (held, _) => HandleLayer.Offers(held.Owner, held.Path),
            choice: static (held, choice) => held.Path == held.Owner.Path || choice.Tiles(held.Owner).Exists(tile => tile.Laid.Path == held.Path));

    // --- [WATCH]
    internal Option<Seq<Key>> Revealed(Laid laid) =>
        SwitchPartially(
            laid,
            @default: static (_, _) => Option<Seq<Key>>.None,
            pad: static (held, pad) => pad.Handles.Revealed(held),
            choice: static (held, choice) => choice.Value.Lead.Bind(lead => choice.Tiles(held).Find(tile => tile.Key == lead).Map(static tile => tile.Laid.Path)
                | held.Node.Windowed.Filter(lazy => lazy.Items.Exists(item => item == lead)).Map(_ => held.Path.Add(lead))));

    internal Seq<IO<Unit>> Watched(Option<Sight> before, Sight after) =>
        SwitchPartially(
            (Before: before, After: after),
            @default: static (_, _) => Seq<IO<Unit>>(),
            choice: static (held, choice) => choice.Watch(held.Before, held.After));

    // --- [CASES]
    public sealed record Press(Func<SKRect, IO<Unit>> Run) : Part {
        public Func<SKRect, Option<IO<Unit>>> Secondary { get; init; } = static _ => None;

        public Node Drawn(Node face) => face with { Part = this, Accessible = face.Accessible.Combine(new Accessible { Role = AccessRole.Button }) };

        internal Reply Begin(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially((Press: this, Aim: aim), @default: Reply.Passed, press: static (held, _) => new Reply.Take([], Some(held.Press.Holding(held.Aim))));

        internal Reply Perform(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially(
                (Press: this, aim.Bounds),
                @default: Reply.Passed,
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    held,
                    @default: static (_, _) => new Reply.Pass(),
                    press: static (state, _) => new Reply.Take([state.Press.Run(state.Bounds)], None),
                    confirm: static (state, _) => new Reply.Take([state.Press.Run(state.Bounds)], None),
                    showMenu: static (state, _) => state.Press.Secondary(state.Bounds).Match(Some: static effect => (Reply)new Reply.Take([effect], None), None: static () => new Reply.Pass())));

        private Track Holding(Aim aim) =>
            new(_ => new Reply.Take([], Some(Holding(aim))),
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
        public Option<Func<Seq<Key>, IO<Unit>>> Exported { get; init; }
        public Func<Seq<Key>, Option<Key>, Option<IO<Unit>>> Dropped { get; init; } = static (_, _) => None;
        public Func<Seq<Key>, SKRect, Option<IO<Unit>>> Menu { get; init; } = static (_, _) => None;
        public Func<Option<Key>, Option<IO<Unit>>> Hover { get; init; } = static _ => None;
        public Func<HashSet<Key>, Option<IO<Unit>>> Viewed { get; init; } = static _ => None;

        private AccessRole Composite => Value.Map(one: AccessRole.RadioGroup, many: AccessRole.List);

        private Seq<Receiver<Held>> Receivers =>
            Seq(Receiver<Held>.Of(static held => held.Value, edit => Some(Edited(edit)))) + Reorder.Map(static reorder => Receiver<Held>.Of(static held => held.Order, edit => Some(reorder(edit)))).ToSeq();

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
                Conditions = Value.Holds(key) ? node.Conditions.TryAdd(Condition.Selected) : node.Conditions,
                Accessible = node.Accessible.Combine(new Accessible { Role = Composite.Owns.IfNone(AccessRole.Radio) }),
            };

        // --- [ANSWER]
        internal Reply Begin(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially(
                (Choice: this, Aim: aim),
                @default: Reply.Passed,
                press: static (held, press) => held.Choice.Item(held.Aim.Over, held.Aim.Laid).Match(
                    Some: key => held.Choice.Picking(key, held.Aim).Began(press),
                    None: () => held.Choice.Value.Switch(
                        (held.Choice, held.Aim, Press: press),
                        one: Reply.Passed,
                        many: static (state, _) => state.Choice.Sweeping(state.Aim).Began(state.Press))));

        internal Reply Perform(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially(
                (Choice: this, Aim: aim),
                @default: Reply.Passed,
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    (held.Choice, held.Aim, Perform: perform),
                    @default: static (_, _) => new Reply.Pass(),
                    press: static (state, _) => state.Choice.Selected(state.Aim, state.Perform),
                    confirm: static (state, _) => state.Choice.Selected(state.Aim, state.Perform),
                    showMenu: static (state, _) => (from key in state.Choice.Item(state.Aim.Over, state.Aim.Laid)
                                                    from tile in state.Choice.Framed(state.Aim).Find(tile => tile.Item == key)
                                                    from effect in state.Choice.Menu(state.Choice.Moving(state.Choice.Value, key), tile.Frame)
                                                    select (Reply)new Reply.Take([effect], None))
                        .IfNone(static () => new Reply.Pass()),
                    navigate: static (state, navigate) => navigate.Toward.Grain.Stride(1f, 0f).Match(
                            Some: stride => stride > 0f ? state.Choice.Adjacent(state.Aim, navigate.Toward) : None,
                            None: () => navigate.Toward.Sign < 0 ? state.Choice.Items.Head : state.Choice.Items.Last)
                        .Match(
                            Some: key => state.Choice.Stepping(start => start with { Value = start.Value.Led(state.Choice.Items, key, state.Perform.Modifiers.Shift) }).Stepped(state.Perform),
                            None: static () => new Reply.Pass()),
                    selectAll: static (state, _) => state.Choice.Stepping(start => start with { Value = start.Value.All(state.Choice.Items) }).Stepped(state.Perform)));

        internal Seq<IO<Unit>> Watch(Option<Sight> before, Sight after) {
            Option<Key> hovered = Hovered(after);
            HashSet<Key> seen = Seen(after);
            return (before.Bind(Hovered) == hovered ? [] : Hover(hovered).ToSeq()) + (before.Map(Seen) == Some(seen) ? [] : Viewed(seen).ToSeq());
        }

        private Session<Held> Picking(Key key, Aim aim) =>
            new(new Held(Value, Items), Receivers, Reorder.IsSome ? CursorShape.Grab : CursorShape.Default) {
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(Picked(start, key, press, aim)) : None,
                Dragging = Reorder.Map(static _ => CursorShape.Grabbing),
                Dragged = state => Moving(state.Value, key),
                Exit = (state, press) => press.Dragged.IsSome && !aim.Scene.Covers(press.At) ? Exported.Map(export => Seq(export(Moving(state.Value, key)))) : None,
            };

        private Held Picked(Held start, Key key, Gesture.Press press, Aim aim) =>
            start.Value.Raised(Items, key, press.Modifiers) switch {
                var picked when press.Dragged.IsNone || (Exported.IsSome && !aim.Scene.Covers(press.At)) => new Held(picked, Items),
                var picked => new Held(picked, Reorder.IsSome ? Ordered(Moving(picked, key), At(aim, press.At)) : Items),
            };

        private Session<Held> Sweeping(Aim aim) =>
            new(new Held(Value, Items), Receivers, CursorShape.Default) {
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(start with { Value = start.Value.Swept(Items, Covered(aim, press), press.Modifiers) }) : None,
            };

        private Session<Held> Stepping(Func<Held, Held> next) => new(new Held(Value, Items), Receivers, CursorShape.Default) { Step = (start, gesture) => gesture is Gesture.Perform ? Some(next(start)) : None };

        private Reply Selected(Aim aim, Gesture.Perform perform) =>
            Item(aim.Over, aim.Laid).Match(
                Some: key => Stepping(start => start with { Value = start.Value.Raised(Items, key, perform.Modifiers) }).Stepped(perform),
                None: static () => new Reply.Pass());

        private Option<Key> Adjacent(Aim aim, NavigationDirection toward) =>
            Value.Lead.Match(
                Some: lead => {
                    Seq<Candidate<Key>> tiles = Framed(aim);
                    return tiles.Find(tile => tile.Item == lead).Bind(origin => Routing.Toward(origin.Frame, tiles.Filter(tile => tile.Item != lead), toward));
                },
                None: () => Items.Head);

        // --- [READ]
        internal Option<Key> Item(Seq<Key> over, Laid laid) => Item(over, laid, toHashSet(Items));

        internal Seq<Tiled> Tiles(Laid laid) => Tiles(laid, toHashSet(Items));

        private Seq<Candidate<Key>> Framed(Aim aim) => Tiles(aim.Laid).Choose(tile => aim.Scene.Frame(tile.Laid.Path).Map(frame => new Candidate<Key>(tile.Key, frame))).Strict();

        private Seq<Key> Moving(Selection picked, Key key) => picked.Holds(key) ? Items.Filter(picked.Holds) : [key];

        private Option<Key> At(Aim aim, SKPoint at) => Framed(aim).Find(tile => tile.Frame.Contains(at)).Map(static tile => tile.Item);

        private HashSet<Key> Covered(Aim aim, Gesture.Press press) {
            SKRect swept = new SKRect(press.From.X, press.From.Y, press.At.X, press.At.Y).Standardized;
            return press.Dragged.IsNone ? [] : toHashSet(Framed(aim).Filter(tile => tile.Frame.IntersectsWith(swept)).Map(static tile => tile.Item));
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

        private Option<Key> Hovered(Sight sight) => sight.Hover.Choose(path => Item(path, sight.Laid)).Head;

        private HashSet<Key> Seen(Sight sight) =>
            toHashSet(sight.Laid.Node.Windowed.Match(
                Some: lazy => (from frame in sight.Scene.Frame(sight.Laid.Path)
                               from shown in sight.Scene.Visible(sight.Laid.Path)
                               select Layouts.Visible(sight.Laid, lazy, lazy.Axis.Of(shown.Location - frame.Location), lazy.Axis.Of(shown.Size))).IfNone([]),
                None: () => Tiles(sight.Laid).Filter(tile => sight.InView(tile.Laid.Path)).Map(static tile => tile.Key)));

        private static Option<Key> Item(Seq<Key> over, Laid laid, HashSet<Key> members) =>
            laid.Children.Find(child => over.Take(child.Path.Count) == child.Path).Bind(child => Tile(child, members) | Item(over, child, members));

        private static Seq<Tiled> Tiles(Laid laid, HashSet<Key> members) =>
            laid.Children.Bind(child => Tile(child, members).Match(Some: key => Seq(new Tiled(key, child)), None: () => Tiles(child, members)));

        private static Option<Key> Tile(Laid laid, HashSet<Key> members) => laid.Node.Key.Filter(members.Contains);

        private sealed record Held(Selection Value, Seq<Key> Order);

        internal sealed record Tiled(Key Key, Laid Laid);
    }

    public sealed record Viewport(PlotPlane Rest, Func<Edit<PlotPlane>, IO<Unit>> Edited) : Part {
        public Magnification Scales { get; init; } = Magnification.Free;

        internal Reply Begin(Gesture gesture, Aim aim) =>
            Framed(aim).Match(
                Some: frame => gesture.SwitchPartially(
                    (Viewport: this, Frame: frame),
                    @default: Reply.Passed,
                    press: static (held, press) => (held.Viewport.Tracking(held.Frame) with {
                        Step = press.Clicks >= 2
                            ? (_, gesture) => gesture is Gesture.Press ? Some(held.Viewport.Rest) : None
                            : (start, gesture) => gesture is Gesture.Press moved
                                ? from travel in held.Frame.Travel(moved.From, moved.At)
                                  from plane in start.Shifted(-travel)
                                  where (held.Frame with { Plane = plane }).Resolves
                                  select plane
                                : None,
                    }).Began(press),
                    scroll: static (held, scroll) => (held.Viewport.Tracking(held.Frame) with {
                        Step = (start, gesture) => gesture is Gesture.Scroll slid
                            ? from travel in held.Frame.Travel(slid.At, new SKPoint(slid.At.X + slid.Travel.X, slid.At.Y + slid.Travel.Y))
                              from plane in start.Shifted(-travel)
                              where (held.Frame with { Plane = plane }).Resolves
                              select plane
                            : None,
                    }).Began(scroll),
                    magnify: static (held, magnify) => (held.Viewport.Tracking(held.Frame) with {
                        Step = (start, gesture) => gesture is Gesture.Magnify pinched
                            ? held.Frame.At(pinched.About).Bind(about => held.Viewport.Scaled(start, about, 1f / (1f + pinched.Magnification), held.Frame))
                            : None,
                    }).Began(magnify)),
                None: static () => new Reply.Pass());

        internal Reply Perform(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially(
                (Viewport: this, Aim: aim),
                @default: Reply.Passed,
                scroll: static (held, scroll) => Framed(held.Aim).Match(
                    Some: frame => {
                        HashMap<Metric, float> metrics = HandleLayer.Metrics(held.Aim.Laid, None);
                        return (held.Viewport.Tracking(frame) with {
                            Step = (start, gesture) => gesture is Gesture.Scroll notched
                                ? frame.At(notched.At).Bind(about => held.Viewport.Scaled(start, about, MathF.Pow(metrics[Metric.Notch], -notched.Travel.Y / metrics[Metric.Extent]), frame))
                                : None,
                        }).Stepped(scroll);
                    },
                    None: static () => new Reply.Pass()));

        private Session<PlotPlane> Tracking(PlotFrame frame) =>
            new(frame.Plane, [Receiver<PlotPlane>.Of(static plane => plane, edit => Some(Edited(edit)))], CursorShape.Grab) { Dragging = CursorShape.Grabbing };

        private Option<PlotPlane> Scaled(PlotPlane start, Vector2 about, float factor, PlotFrame frame) =>
            from free in start.Dimensions.Head
            from rest in Rest.Dimension(free.Along)
            let ratio = MathF.Abs(free.Axis.Domain.Span / rest.Domain.Span)
            let held = float.Clamp(ratio * factor, Scales.Least, Scales.Most) / ratio
            from plane in held.CompareTo(1f) is 0 ? Some(start) : start.Scaled(about, held).Filter(scaled => (frame with { Plane = scaled }).Resolves)
            select plane;

        private static Option<PlotFrame> Framed(Aim aim) => Layouts.Plotted(aim.Laid).Map(frame => frame.Translated(aim.Bounds.Location));
    }
}
