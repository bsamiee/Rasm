using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Reply {
    internal static Reply Passed<TState, TGesture>(TState _, TGesture __) => new Pass();

    public sealed record Pass : Reply;

    public sealed record Take(Seq<IO<Unit>> Effects, Option<Track> Track) : Reply;
}

internal sealed record Track(Func<Gesture, Reply> Moved, Func<Gesture, Seq<IO<Unit>>> Released, Func<Seq<IO<Unit>>> Cancelled) {
    public CursorShape Cursor { get; init; } = CursorShape.Default;
    public Option<Vector2> Detent { get; init; }
    public Seq<Key> Dragged { get; init; }
}

internal sealed record Receiver<TState>(Func<TState, TState, bool> Same, Func<Edit<TState>, Option<IO<Unit>>> Emit) where TState : notnull {
    public static Receiver<TState> Of<TOut>(Func<TState, TOut> project, Func<Edit<TOut>, Option<IO<Unit>>> edited) where TOut : notnull =>
        new((before, after) => EqualityComparer<TOut>.Default.Equals(project(before), project(after)), edit => edited(edit.Map(project).As()));
}

internal sealed record Session<TState>(TState Start, Seq<Receiver<TState>> Receivers, CursorShape Cursor) where TState : notnull {
    // --- [STATE]
    public Func<TState, Gesture, Option<TState>> Step { get; init; } = static (_, _) => None;
    public Func<TState, Gesture.Press, Option<Seq<IO<Unit>>>> Exit { get; init; } = static (_, _) => None;
    public Option<CursorShape> Dragging { get; init; }
    public Func<TState, Option<Vector2>> Detent { get; init; } = static _ => None;
    public Func<TState, Seq<Key>> Dragged { get; init; } = static _ => [];

    // --- [EMIT]
    internal Reply Began(Gesture first) =>
        Step(Start, first).Match(
            Some: state => Previewed(Start, state) switch {
                var opened => (Reply)new Reply.Take(opened.Map(static held => held.Effect), Some(Tracked(new Run(Start, state, first, toHashSet(opened.Map(static held => held.Index)))))),
            },
            None: static () => new Reply.Pass());

    internal Reply Stepped(Gesture gesture) =>
        Step(Start, gesture).Match(
            Some: state => (Reply)new Reply.Take(Receivers.Filter(receiver => !receiver.Same(Start, state)).Choose(receiver => receiver.Emit(Edit.Step(state))), None),
            None: static () => new Reply.Pass());

    private sealed record Run(TState Origin, TState Last, Gesture Latest, LanguageExt.HashSet<int> Opened);

    private Seq<(Receiver<TState> Receiver, int Index)> Indexed => Receivers.Map(static (receiver, index) => (Receiver: receiver, Index: index));

    private Seq<(int Index, IO<Unit> Effect)> Previewed(TState from, TState to) =>
        Indexed.Filter(held => !held.Receiver.Same(from, to)).Choose(held => held.Receiver.Emit(Edit.Preview(to)).Map(effect => (held.Index, Effect: effect)));

    private Track Tracked(Run run) =>
        new(gesture => Followed(run, gesture), gesture => Finished(run, Step(Start, gesture).IfNone(run.Last)), () => Cancelled(run)) {
            Cursor = run.Latest.SwitchPartially(
                this,
                @default: static (session, _) => session.Cursor,
                press: static (session, press) => press.Dragged.IsSome ? session.Dragging.IfNone(session.Cursor) : session.Cursor),
            Detent = Detent(run.Last),
            Dragged = Dragged(run.Last),
        };

    private Reply Followed(Run run, Gesture gesture) =>
        Step(Start, gesture).Match(Some: state => (State: state, Latest: gesture), None: () => (State: run.Last, run.Latest)) switch {
            var next => gesture.SwitchPartially(
                    (Session: this, next.State),
                    @default: static (_, _) => Option<Seq<IO<Unit>>>.None,
                    press: static (held, press) => held.Session.Exit(held.State, press))
                .Match(
                    Some: extra => (Reply)new Reply.Take(Finished(run, next.State) + extra, None),
                    None: () => Continued(run, next.State, next.Latest)),
        };

    private Reply.Take Continued(Run run, TState state, Gesture latest) =>
        Previewed(run.Last, state) switch {
            var moved => new(moved.Map(static held => held.Effect), Some(Tracked(run with { Last = state, Latest = latest, Opened = run.Opened.Union(moved.Map(static held => held.Index)) }))),
        };

    private Seq<IO<Unit>> Finished(Run run, TState state) =>
        Indexed.Choose(held =>
            !held.Receiver.Same(run.Origin, state) ? held.Receiver.Emit(Edit.Commit(state))
            : run.Opened.Contains(held.Index) ? held.Receiver.Emit(Edit.Cancel<TState>())
            : None);

    private Seq<IO<Unit>> Cancelled(Run run) =>
        Indexed.Filter(held => run.Opened.Contains(held.Index)).Choose(static held => held.Receiver.Emit(Edit.Cancel<TState>()));
}

internal sealed record Aim(Scene Scene, Laid Laid, SKRect Bounds, Seq<Key> Over);

[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Magnification {
    public static Magnification Free { get; } = Create(float.Epsilon, float.PositiveInfinity);

    public float Least { get; }
    public float Most { get; }

    public static Fin<Magnification> Validated(float least, float most) => TryCreate(least, most, out Magnification? magnification, out Invalid? fault) ? magnification : fault;

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref float least, ref float most) =>
        validationError = float.IsFinite(least) && least > 0f && most >= least
            ? null
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"magnification ({least}, {most}) is not a positive ordered span"));
}

internal sealed record Sight(Scene Scene, Laid Laid, Seq<Seq<Key>> Hover, Option<SKPoint> Pointer) {
    public bool InView(Seq<Key> path) => Scene.Bounds(path).Exists(static bounds => !bounds.IsEmpty);

    public Option<Vector2> Plotted =>
        from pointer in Pointer
        where Hover.Exists(path => path.Take(Laid.Path.Count) == Laid.Path)
        from bounds in Scene.Frame(Laid.Path)
        from frame in Layouts.Plotted(Laid)
        from at in frame.Translated(bounds.Location).At(pointer)
        select at;
}

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
        public Func<Edit<Seq<Key>>, Option<IO<Unit>>> Reorder { get; init; } = static _ => None;
        public Func<Seq<Key>, Option<IO<Unit>>> Exported { get; init; } = static _ => None;
        public Func<Seq<Key>, Option<Key>, Option<IO<Unit>>> Dropped { get; init; } = static (_, _) => None;
        public Func<Seq<Key>, SKRect, Option<IO<Unit>>> Menu { get; init; } = static (_, _) => None;
        public Func<Option<Key>, Option<IO<Unit>>> Hover { get; init; } = static _ => None;
        public Func<LanguageExt.HashSet<Key>, Option<IO<Unit>>> Viewed { get; init; } = static _ => None;

        private AccessRole Composite => Value.Map(one: AccessRole.RadioGroup, many: AccessRole.List);

        private bool Reorders => Reorder(Edit.Cancel<Seq<Key>>()).IsSome;

        private bool Exports => Exported([]).IsSome;

        private Seq<Receiver<Held>> Receivers => Seq(Receiver<Held>.Of(static held => held.Value, edit => Some(Edited(edit)))) + (Reorders ? Seq(Receiver<Held>.Of(static held => held.Order, Reorder)) : []);

        // --- [DRAW]
        public Node Drawn(Func<Key, Node> item, Func<Seq<Node>, Node> container) =>
            container(Items.Map(key => Member(item(key), key))) switch {
                var held => held with { Part = this, Accessible = held.Accessible.Combine(new Accessible { Role = Composite }) },
            };

        public Node.Lazy Drawn(Func<Key, Node> item, PhysicalAxis axis, Tracks across, float extent, LanguageExt.HashSet<Key> realized) =>
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
                                                    from tile in state.Choice.Framed(state.Aim).Find(tile => tile.Key == key)
                                                    from effect in state.Choice.Menu(state.Choice.Moving(state.Choice.Value, key), tile.Frame)
                                                    select (Reply)new Reply.Take([effect], None))
                        .IfNone(static () => new Reply.Pass()),
                    navigate: static (state, navigate) => navigate.Toward.Grain.Stride(1f, 0f).Match(
                            Some: stride => stride > 0f ? Adjacent((state.Choice, state.Aim, navigate.Toward)) : None,
                            None: () => navigate.Toward.Sign < 0 ? state.Choice.Items.Head : state.Choice.Items.Last)
                        .Match(
                            Some: key => state.Choice.Stepping(start => start with { Value = start.Value.Led(state.Choice.Items, key, state.Perform.Modifiers.Shift) }).Stepped(state.Perform),
                            None: static () => new Reply.Pass()),
                    selectAll: static (state, _) => state.Choice.Stepping(start => start with { Value = start.Value.All(state.Choice.Items) }).Stepped(state.Perform)));

        internal Seq<IO<Unit>> Watch(Option<Sight> before, Sight after) =>
            (Hovered(after) switch {
                var now => before.Bind(Hovered) == now ? [] : Hover(now).ToSeq(),
            })
            + (Seen(after) switch {
                var now => before.Map(Seen) == Some(now) ? [] : Viewed(now).ToSeq(),
            });

        private Session<Held> Picking(Key key, Aim aim) =>
            new(new Held(Value, Items), Receivers, Reorders ? CursorShape.Grab : CursorShape.Default) {
                Step = (start, gesture) => gesture is Gesture.Press press ? Some(Picked(start, key, press, aim)) : None,
                Dragging = Reorders ? Some(CursorShape.Grabbing) : None,
                Dragged = state => Moving(state.Value, key),
                Exit = (state, press) => press.Dragged.IsSome && !aim.Scene.Covers(press.At) ? Exported(Moving(state.Value, key)).Map(static export => Seq(export)) : None,
            };

        private Held Picked(Held start, Key key, Gesture.Press press, Aim aim) =>
            start.Value.Raised(Items, key, press.Modifiers) switch {
                var picked when press.Dragged.IsNone || (Exports && !aim.Scene.Covers(press.At)) => new Held(picked, Items),
                var picked => new Held(picked, Reorders ? Ordered(Moving(picked, key), At(aim, press.At)) : Items),
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

        private static Option<Key> Adjacent((Choice Choice, Aim Aim, NavigationDirection Toward) held) =>
            held.Choice.Value.Lead.Match(
                Some: lead => held.Choice.Framed(held.Aim) switch {
                    var tiles => tiles.Find(tile => tile.Key == lead).Bind(origin => Routing.Toward(origin.Frame, tiles.Filter(tile => tile.Key != lead).Map(static tile => (Item: tile.Key, tile.Frame)), held.Toward)),
                },
                None: () => held.Choice.Items.Head);

        // --- [READ]
        internal Option<Key> Item(Seq<Key> over, Laid laid) => Item(over, laid, toHashSet(Items));

        internal Seq<(Key Key, Laid Laid)> Tiles(Laid laid) => Tiles(laid, toHashSet(Items));

        private Seq<(Key Key, SKRect Frame)> Framed(Aim aim) => Tiles(aim.Laid).Choose(tile => aim.Scene.Frame(tile.Laid.Path).Map(frame => (tile.Key, frame)));

        private Seq<Key> Moving(Selection picked, Key key) => picked.Holds(key) ? Items.Filter(picked.Holds) : [key];

        private Option<Key> At(Aim aim, SKPoint at) => Framed(aim).Find(tile => tile.Frame.Contains(at)).Map(static tile => tile.Key);

        private LanguageExt.HashSet<Key> Covered(Aim aim, Gesture.Press press) =>
            press.Dragged.IsNone
                ? []
                : new SKRect(press.From.X, press.From.Y, press.At.X, press.At.Y).Standardized switch {
                    var swept => toHashSet(Framed(aim).Filter(tile => tile.Frame.IntersectsWith(swept)).Map(static tile => tile.Key)),
                };

        private Seq<Key> Ordered(Seq<Key> moving, Option<Key> under) =>
            toHashSet(moving) switch {
                var moved => (from target in under.Filter(key => !moved.Contains(key))
                              from place in Selection.Position(Items, target)
                              from first in moving.Head.Bind(key => Selection.Position(Items, key))
                              let rest = Items.Filter(key => !moved.Contains(key))
                              from slot in Selection.Position(rest, target)
                              let index = place > first ? slot + 1 : slot
                              select rest.Take(index) + moving + rest.Skip(index)).IfNone(Items),
            };

        private Option<Key> Hovered(Sight sight) => sight.Hover.Choose(path => Item(path, sight.Laid)).Head;

        private LanguageExt.HashSet<Key> Seen(Sight sight) =>
            toHashSet(sight.Laid.Node.Windowed.Match(
                Some: lazy => (from frame in sight.Scene.Frame(sight.Laid.Path)
                               from shown in sight.Scene.Visible(sight.Laid.Path)
                               select Layouts.Visible(sight.Laid, lazy, lazy.Axis.Of(shown.Location - frame.Location), lazy.Axis.Of(shown.Size))).IfNone([]),
                None: () => Tiles(sight.Laid).Filter(tile => sight.InView(tile.Laid.Path)).Map(static tile => tile.Key)));

        private static Option<Key> Item(Seq<Key> over, Laid laid, LanguageExt.HashSet<Key> members) =>
            laid.Children.Find(child => over.Take(child.Path.Count) == child.Path).Bind(child => Tile(child, members) | Item(over, child, members));

        private static Seq<(Key Key, Laid Laid)> Tiles(Laid laid, LanguageExt.HashSet<Key> members) =>
            laid.Children.Bind(child => Tile(child, members).Match(Some: key => Seq((Key: key, Laid: child)), None: () => Tiles(child, members)));

        private static Option<Key> Tile(Laid laid, LanguageExt.HashSet<Key> members) => laid.Node.Key.Filter(members.Contains);

        private sealed record Held(Selection Value, Seq<Key> Order);
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
                    Some: frame => HandleLayer.Metrics(held.Aim.Laid, None) switch {
                        var metrics => (held.Viewport.Tracking(frame) with {
                            Step = (start, gesture) => gesture is Gesture.Scroll notched
                                ? frame.At(notched.At).Bind(about => held.Viewport.Scaled(start, about, MathF.Pow(metrics[Metric.Notch], -notched.Travel.Y / metrics[Metric.Extent]), frame))
                                : None,
                        }).Stepped(scroll),
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
