using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Edit<TValue> : K<Edit, TValue> {
    public Option<TValue> Carried =>
        Switch(preview: static held => Some(held.Value), step: static held => Some(held.Value), commit: static held => Some(held.Value), cancel: static _ => Option<TValue>.None);

    public Option<TValue> Settled =>
        Switch(preview: static _ => Option<TValue>.None, step: static held => Some(held.Value), commit: static held => Some(held.Value), cancel: static _ => Option<TValue>.None);

    public sealed record Preview(TValue Value) : Edit<TValue>;

    public sealed record Step(TValue Value) : Edit<TValue>;

    public sealed record Commit(TValue Value) : Edit<TValue>;

    public sealed record Cancel : Edit<TValue>;
}

public sealed class Edit : Traversable<Edit> {
    public static K<Edit, B> Map<A, B>(Func<A, B> f, K<Edit, A> ma) =>
        ma.As().Switch(
            f,
            preview: static (map, held) => (Edit<B>)new Edit<B>.Preview(map(held.Value)),
            step: static (map, held) => new Edit<B>.Step(map(held.Value)),
            commit: static (map, held) => new Edit<B>.Commit(map(held.Value)),
            cancel: static (_, _) => new Edit<B>.Cancel());

    public static K<F, K<Edit, B>> Traverse<F, A, B>(Func<A, K<F, B>> f, K<Edit, A> ta) where F : Applicative<F> =>
        ta.As().Switch(
            f,
            preview: static (map, held) => map(held.Value).Map(static value => (K<Edit, B>)new Edit<B>.Preview(value)),
            step: static (map, held) => map(held.Value).Map(static value => (K<Edit, B>)new Edit<B>.Step(value)),
            commit: static (map, held) => map(held.Value).Map(static value => (K<Edit, B>)new Edit<B>.Commit(value)),
            cancel: static (_, _) => F.Pure<K<Edit, B>>(new Edit<B>.Cancel()));

    public static Fold<A, S> FoldStep<A, S>(K<Edit, A> ta, S initialState) =>
        ta.As().Carried.Match(Some: value => Fold.Loop(initialState, value, Fold.Done<A, S>), None: () => Fold.Done<A, S>(initialState));

    public static Fold<A, S> FoldStepBack<A, S>(K<Edit, A> ta, S initialState) =>
        ta.As().Carried.Match(Some: value => Fold.Loop(initialState, value, Fold.Done<A, S>), None: () => Fold.Done<A, S>(initialState));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Selection {
    public Option<Key> Lead => Switch(one: static one => one.Picked, many: static many => many.Focus);

    public bool Holds(Key key) => Switch(key, one: static (held, one) => one.Picked == Some(held), many: static (held, many) => many.Picked.Contains(held));

    public Selection Raised(Seq<Key> ordered, Key hit, LanguageExt.HashSet<Modifier> modifiers) =>
        Switch(
            (Ordered: ordered, Hit: hit, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(Some(held.Hit)),
            many: static (held, many) => (held.Modifiers.Contains(Modifier.Common), held.Modifiers.Contains(Modifier.Shift)) switch {
                (true, _) => new Many(many.Picked.SymmetricExcept([held.Hit]), Some(held.Hit), Some(held.Hit)),
                (_, true) => Spanned(held.Ordered, many.Anchor, held.Hit),
                _ when many.Picked.Contains(held.Hit) => new Many(many.Picked, Some(held.Hit), Some(held.Hit)),
                _ => Only(held.Hit),
            });

    public Selection Led(Seq<Key> ordered, Key lead, bool extend) =>
        Switch(
            (Ordered: ordered, Lead: lead, Extend: extend),
            one: static (held, _) => (Selection)new One(Some(held.Lead)),
            many: static (held, many) => held.Extend ? Spanned(held.Ordered, many.Anchor | many.Focus, held.Lead) : Only(held.Lead));

    public Selection All(Seq<Key> ordered) => Switch(ordered, one: static (_, one) => (Selection)one, many: static (keys, many) => new Many(toHashSet(keys), many.Anchor, many.Focus));

    public Selection Swept(Seq<Key> ordered, LanguageExt.HashSet<Key> covered, LanguageExt.HashSet<Modifier> modifiers) =>
        Switch(
            (Ordered: ordered, Covered: covered, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(held.Ordered.Find(held.Covered.Contains)),
            many: static (held, many) => (held.Modifiers.Contains(Modifier.Common), held.Modifiers.Contains(Modifier.Shift)) switch {
                (true, _) => many.Picked.SymmetricExcept(held.Covered),
                (_, true) => many.Picked.Union(held.Covered),
                _ => held.Covered,
            } switch {
                var picked => held.Ordered.Filter(held.Covered.Contains) switch {
                    var swept => new Many(picked, swept.Head | many.Anchor, swept.Last | many.Focus),
                },
            });

    internal Selection Renamed(HashMap<Key, Key> renames) =>
        Switch(
            renames,
            one: static (held, one) => (Selection)new One(one.Picked.Map(key => Renamed(held, key))),
            many: static (held, many) => new Many(many.Picked.Map(key => Renamed(held, key)), many.Anchor.Map(key => Renamed(held, key)), many.Focus.Map(key => Renamed(held, key))));

    internal static Option<int> Position(Seq<Key> ordered, Key key) =>
        ordered.Map(static (held, index) => (Key: held, Index: index)).Find(entry => entry.Key == key).Map(static entry => entry.Index);

    private static Key Renamed(HashMap<Key, Key> renames, Key key) => renames.Find(key).IfNone(key);

    private static Many Only(Key key) => new([key], Some(key), Some(key));

    private static Many Spanned(Seq<Key> ordered, Option<Key> anchor, Key target) =>
        (from end in Position(ordered, target)
         let found = anchor.Bind(held => Position(ordered, held))
         let start = found.IfNone(end)
         select new Many(toHashSet(ordered.Skip(int.Min(start, end)).Take(int.Abs(start - end) + 1)), found.IsSome ? anchor : Some(target), Some(target)))
        .IfNone(() => Only(target));

    public sealed record One(Option<Key> Picked) : Selection;

    public sealed record Many(LanguageExt.HashSet<Key> Picked, Option<Key> Anchor, Option<Key> Focus) : Selection;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Reply {
    internal static Reply Passed<TState, TGesture>(TState _, TGesture __) => new Pass();

    public sealed record Pass : Reply;

    public sealed record Take(Seq<IO<Unit>> Effects, Option<Track> Track) : Reply;
}

internal sealed record Track(Func<Gesture, Reply> Moved, Func<Gesture, Seq<IO<Unit>>> Released, Func<Seq<IO<Unit>>> Cancelled) {
    public Cursor Cursor { get; init; } = Cursor.Default;
    public Option<Vector2> Detent { get; init; }
    public Seq<Key> Carried { get; init; }
}

internal sealed record Sink<TState>(Func<TState, TState, bool> Same, Func<Edit<TState>, IO<Unit>> Emit) where TState : notnull {
    public static Sink<TState> Of<TOut>(Func<TState, TOut> project, Func<Edit<TOut>, IO<Unit>> edited) where TOut : notnull =>
        new((before, after) => EqualityComparer<TOut>.Default.Equals(project(before), project(after)), edit => edited(edit.Map(project).As()));
}

internal sealed record Session<TState>(TState Start, Seq<Sink<TState>> Sinks, Cursor Cursor) where TState : notnull {
    // --- [STATE]
    public Func<TState, Gesture.Press, Option<TState>> Press { get; init; } = static (_, _) => None;
    public Func<TState, Gesture.Scroll, Option<TState>> Scroll { get; init; } = static (_, _) => None;
    public Func<TState, Gesture.Magnify, Option<TState>> Magnify { get; init; } = static (_, _) => None;
    public Func<TState, Gesture.Rotate, Option<TState>> Rotate { get; init; } = static (_, _) => None;
    public Func<TState, Gesture.Perform, Option<TState>> Perform { get; init; } = static (_, _) => None;
    public Func<TState, Gesture.Press, Option<Seq<IO<Unit>>>> Exit { get; init; } = static (_, _) => None;
    public Option<Cursor> Dragging { get; init; }
    public Func<TState, Option<Vector2>> Detent { get; init; } = static _ => None;
    public Func<TState, Seq<Key>> Carried { get; init; } = static _ => [];

    // --- [EMIT]
    internal Reply Began(Gesture first) =>
        Reduced(first).Match(
            Some: state => Previewed(Start, state) switch {
                var opened => (Reply)new Reply.Take(opened.Map(static held => held.Effect), Some(Tracked(new Run(Start, state, first, toHashSet(opened.Map(static held => held.Index)))))),
            },
            None: static () => new Reply.Pass());

    internal Reply Stepped(Gesture gesture) =>
        Reduced(gesture).Match(
            Some: state => (Reply)new Reply.Take(Sinks.Filter(sink => !sink.Same(Start, state)).Map(sink => sink.Emit(new Edit<TState>.Step(state))), None),
            None: static () => new Reply.Pass());

    private sealed record Run(TState Origin, TState Last, Gesture Latest, LanguageExt.HashSet<int> Opened);

    private Seq<(Sink<TState> Sink, int Index)> Indexed => Sinks.Map(static (sink, index) => (Sink: sink, Index: index));

    private Option<TState> Reduced(Gesture gesture) =>
        gesture.Switch(
            this,
            press: static (session, press) => session.Press(session.Start, press),
            scroll: static (session, scroll) => session.Scroll(session.Start, scroll),
            magnify: static (session, magnify) => session.Magnify(session.Start, magnify),
            rotate: static (session, rotate) => session.Rotate(session.Start, rotate),
            perform: static (session, perform) => session.Perform(session.Start, perform));

    private Seq<(int Index, IO<Unit> Effect)> Previewed(TState from, TState to) =>
        Indexed.Filter(held => !held.Sink.Same(from, to)).Map(held => (held.Index, held.Sink.Emit(new Edit<TState>.Preview(to))));

    private Track Tracked(Run run) =>
        new(gesture => Followed(run, gesture), gesture => Settled(run, Reduced(gesture).IfNone(run.Last)), () => Cancelled(run)) {
            Cursor = run.Latest.SwitchPartially(
                this,
                @default: static (session, _) => session.Cursor,
                press: static (session, press) => press.Dragged.IsSome ? session.Dragging.IfNone(session.Cursor) : session.Cursor),
            Detent = Detent(run.Last),
            Carried = Carried(run.Last),
        };

    private Reply Followed(Run run, Gesture gesture) =>
        Reduced(gesture).Match(Some: state => (State: state, Latest: gesture), None: () => (State: run.Last, run.Latest)) switch {
            var next => gesture.SwitchPartially(
                    (Session: this, next.State),
                    @default: static (_, _) => Option<Seq<IO<Unit>>>.None,
                    press: static (held, press) => held.Session.Exit(held.State, press))
                .Match(
                    Some: extra => (Reply)new Reply.Take(Settled(run, next.State) + extra, None),
                    None: () => Continued(run, next.State, next.Latest)),
        };

    private Reply.Take Continued(Run run, TState state, Gesture latest) =>
        Previewed(run.Last, state) switch {
            var moved => new(moved.Map(static held => held.Effect), Some(Tracked(run with { Last = state, Latest = latest, Opened = run.Opened.Union(moved.Map(static held => held.Index)) }))),
        };

    private Seq<IO<Unit>> Settled(Run run, TState state) =>
        Indexed.Choose(held =>
            !held.Sink.Same(run.Origin, state) ? Some(held.Sink.Emit(new Edit<TState>.Commit(state)))
            : run.Opened.Contains(held.Index) ? Some(held.Sink.Emit(new Edit<TState>.Cancel()))
            : None);

    private Seq<IO<Unit>> Cancelled(Run run) =>
        Indexed.Filter(held => run.Opened.Contains(held.Index)).Map(static held => held.Sink.Emit(new Edit<TState>.Cancel()));
}

internal sealed record Aim(Scene Scene, Laid Laid, SKRect Bounds, Seq<Key> Over) {
    public Option<Editor> Editor { get; init; }
}

internal sealed record Sight(Scene Scene, Laid Laid, Seq<Seq<Key>> Hover, Option<SKPoint> Pointer) {
    public bool InView(Seq<Key> path) => Scene.Bounds(path).Exists(static bounds => !bounds.IsEmpty);
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
            field: static (held, field) => field.Begin(held.Gesture, held.Aim),
            viewport: static (held, viewport) => viewport.Begin(held.Gesture, held.Aim));

    internal Reply Performed(Gesture gesture, Aim aim) =>
        Switch(
            (Gesture: gesture, Aim: aim),
            press: static (held, press) => press.Perform(held.Gesture, held.Aim),
            pad: static (held, pad) => pad.Handles.Performed(held.Gesture, held.Aim),
            choice: static (held, choice) => choice.Perform(held.Gesture, held.Aim),
            field: static (held, field) => field.Perform(held.Gesture, held.Aim),
            viewport: static (held, viewport) => viewport.Perform(held.Gesture, held.Aim));

    internal Option<IO<Unit>> Landed(Seq<Key> carried, Aim aim) =>
        SwitchPartially(
            (Carried: carried, Aim: aim),
            @default: static (_, _) => Option<IO<Unit>>.None,
            choice: static (held, choice) => choice.Dropped.Map(dropped => dropped(held.Carried, choice.Item(held.Aim.Over, held.Aim.Laid))));

    internal Seq<IO<Unit>> Left(Aim aim) =>
        SwitchPartially(aim, @default: static (_, _) => Seq<IO<Unit>>(), field: static (held, field) => held.Editor.Map(editor => field.Committed(editor.Committed())).ToSeq());

    internal Option<Editor> Opened(Laid laid) =>
        SwitchPartially(laid, @default: static (_, _) => Option<Editor>.None, field: static (held, _) => held.Texted.Map(static text => new Editor(text)));

    internal bool Offers(Laid owner, Seq<Key> path) =>
        SwitchPartially(
            (Owner: owner, Path: path),
            @default: static (held, _) => held.Path == held.Owner.Path,
            pad: static (held, _) => Handles.Offers(held.Owner, held.Path),
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
            pad: static (held, pad) => pad.Handles.Watched(held.Before, held.After),
            choice: static (held, choice) => choice.Watch(held.Before, held.After));

    // --- [CASES]
    public sealed record Press(Func<SKRect, IO<Unit>> Run) : Part {
        public Option<Func<SKRect, IO<Unit>>> Secondary { get; init; }

        public Node Drawn(Node face) => face with { Part = this, Accessible = face.Accessible.Combine(new Accessible { Role = AccessRole.Button }) };

        public static Node Disclosure(Node header, bool open, Func<bool, IO<Unit>> toggled, Func<Node> body) =>
            new Node.Flex(Orientation.Vertical,
                new Press(_ => toggled(!open)).Drawn(header with { Accessible = header.Accessible.Combine(new Accessible { Role = AccessRole.Disclosure, Expanded = open }) })
                    .Cons(open ? Seq(body()) : []));

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
                    showMenu: static (state, _) => state.Press.Secondary.Match(Some: secondary => (Reply)new Reply.Take([secondary(state.Bounds)], None), None: static () => new Reply.Pass())));

        private Track Holding(Aim aim) =>
            new(_ => new Reply.Take([], Some(Holding(aim))),
                gesture => gesture.SwitchPartially(
                    (Press: this, Aim: aim),
                    @default: static (_, _) => Seq<IO<Unit>>(),
                    press: static (held, press) => held.Aim.Bounds.Contains(press.At) ? Seq(held.Press.Run(held.Aim.Bounds)) : []),
                static () => []);
    }

    public sealed record Pad(Handles Handles) : Part;

    public sealed record Choice(Seq<Key> Items, Selection Value, Func<Edit<Selection>, IO<Unit>> Edited) : Part {
        // --- [STATE]
        public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Reorder { get; init; }
        public Option<Func<Seq<Key>, IO<Unit>>> Exported { get; init; }
        public Option<Func<Seq<Key>, Option<Key>, IO<Unit>>> Dropped { get; init; }
        public Option<Func<Seq<Key>, SKRect, IO<Unit>>> Menu { get; init; }
        public Option<Func<Option<Key>, IO<Unit>>> Hover { get; init; }
        public Option<Func<LanguageExt.HashSet<Key>, IO<Unit>>> Viewed { get; init; }

        private AccessRole Composite => Value.Map(one: AccessRole.RadioGroup, many: AccessRole.List);

        private Seq<Sink<Held>> Sinks => Seq(Sink<Held>.Of(static held => held.Value, Edited)) + Reorder.Map(static reorder => Sink<Held>.Of(static held => held.Order, reorder)).ToSeq();

        // --- [DRAW]
        public Node Drawn(Func<Key, Node> item, Func<Seq<Node>, Node> container) => Drawn(item, container, Composite);

        public Node.Lazy Drawn(Func<Key, Node> item, Orientation axis, Tracks across, float extent, LanguageExt.HashSet<Key> realized) =>
            new(axis, across, extent, Items) {
                Realized = toHashMap(Items.Filter(realized.Contains).Map(key => (key, Member(item(key), key, Composite)))),
                Part = this,
                Accessible = new Accessible { Role = Composite },
            };

        public static Node Tabs(Seq<(Key Key, Node Head)> heads, Selection.One value, Func<Edit<Selection>, IO<Unit>> edited, Func<Key, Node> body, Orientation axis) =>
            toHashMap(heads) switch {
                var faces => new Node.Flex(axis.Cross,
                    new Choice(heads.Map(static head => head.Key), value, edited).Drawn(key => faces[key], tabs => new Node.Flex(axis, tabs), AccessRole.TabList)
                        .Cons(value.Picked.Map(body).ToSeq())),
            };

        private Node Drawn(Func<Key, Node> item, Func<Seq<Node>, Node> container, AccessRole role) =>
            container(Items.Map(key => Member(item(key), key, role))) switch {
                var held => held with { Part = this, Accessible = held.Accessible.Combine(new Accessible { Role = role }) },
            };

        private Node Member(Node node, Key key, AccessRole role) =>
            node with {
                Key = key,
                Conditions = Value.Holds(key) ? node.Conditions.TryAdd(Condition.Selected) : node.Conditions,
                Accessible = node.Accessible.Combine(new Accessible { Role = role.Owns.IfNone(AccessRole.Radio) }),
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
                    showMenu: static (state, _) => (from menu in state.Choice.Menu
                                                    from key in state.Choice.Item(state.Aim.Over, state.Aim.Laid)
                                                    from tile in state.Choice.Framed(state.Aim).Find(tile => tile.Key == key)
                                                    select (Reply)new Reply.Take([menu(state.Choice.Moving(state.Choice.Value, key), tile.Frame)], None))
                        .IfNone(static () => new Reply.Pass()),
                    navigate: static (state, navigate) => navigate.Toward.Grain.Switch(
                            (state.Choice, state.Aim, navigate.Toward),
                            unit: Adjacent,
                            page: static _ => Option<Key>.None,
                            edge: static held => held.Toward.Sign < 0 ? held.Choice.Items.Head : held.Choice.Items.Last)
                        .Match(
                            Some: key => state.Choice.Stepping(start => start with { Value = start.Value.Led(state.Choice.Items, key, state.Perform.Modifiers.Contains(Modifier.Shift)) }).Stepped(state.Perform),
                            None: static () => new Reply.Pass()),
                    selectAll: static (state, _) => state.Choice.Stepping(start => start with { Value = start.Value.All(state.Choice.Items) }).Stepped(state.Perform)));

        internal Seq<IO<Unit>> Watch(Option<Sight> before, Sight after) =>
            Hover.Map(hover => Hovered(after) switch {
                var now => before.Bind(Hovered) == now ? Seq<IO<Unit>>() : [hover(now)],
            }).IfNone([])
            + Viewed.Map(viewed => Seen(after) switch {
                var now => before.Map(Seen) == Some(now) ? Seq<IO<Unit>>() : [viewed(now)],
            }).IfNone([]);

        private Session<Held> Picking(Key key, Aim aim) =>
            new(new Held(Value, Items), Sinks, Reorder.IsNone ? Cursor.Default : Cursor.Grab) {
                Press = (start, press) => Some(Picked(start, key, press, aim)),
                Dragging = Reorder.Map(static _ => Cursor.Grabbing),
                Carried = state => Moving(state.Value, key),
                Exit = (state, press) => from export in Exported
                                         where press.Dragged.IsSome && !aim.Scene.Covers(press.At)
                                         select Seq(export(Moving(state.Value, key))),
            };

        private Held Picked(Held start, Key key, Gesture.Press press, Aim aim) =>
            start.Value.Raised(Items, key, press.Modifiers) switch {
                var picked when press.Dragged.IsNone || (Exported.IsSome && !aim.Scene.Covers(press.At)) => new Held(picked, Items),
                var picked => new Held(picked, Reorder.Map(_ => Ordered(Moving(picked, key), At(aim, press.At))).IfNone(Items)),
            };

        private Session<Held> Sweeping(Aim aim) =>
            new(new Held(Value, Items), Sinks, Cursor.Default) { Press = (start, press) => Some(start with { Value = start.Value.Swept(Items, Covered(aim, press), press.Modifiers) }) };

        private Session<Held> Stepping(Func<Held, Held> next) => new(new Held(Value, Items), Sinks, Cursor.Default) { Perform = (start, _) => Some(next(start)) };

        private Reply Selected(Aim aim, Gesture.Perform perform) =>
            Item(aim.Over, aim.Laid).Match(
                Some: key => Stepping(start => start with { Value = start.Value.Raised(Items, key, perform.Modifiers) }).Stepped(perform),
                None: static () => new Reply.Pass());

        private static Option<Key> Adjacent((Choice Choice, Aim Aim, Navigation Toward) held) =>
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
                var carried => (from target in under.Filter(key => !carried.Contains(key))
                                from place in Selection.Position(Items, target)
                                from first in moving.Head.Bind(key => Selection.Position(Items, key))
                                let rest = Items.Filter(key => !carried.Contains(key))
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

    public sealed record Field(Func<Edit<string>, IO<Unit>> Edited) : Part {
        public Node.Text Drawn(Node.Text text) =>
            text with { Part = this, Accessible = text.Accessible.Combine(new Accessible { Role = AccessRole.TextBox, Value = new AccessValue.Text(text.Content) }) };

        internal Reply Begin(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially((Field: this, Aim: aim), @default: Reply.Passed, press: static (held, press) => new Reply.Take(held.Field.Edits(press, held.Aim), Some(held.Field.Typing(held.Aim))));

        internal Reply Perform(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially(
                (Field: this, Aim: aim),
                @default: Reply.Passed,
                perform: static (held, perform) => held.Aim.Laid.Switch(
                    (held.Field, held.Aim, Perform: perform),
                    box: Reply.Passed,
                    text: static (state, text) => Editor.Operation(text, state.Perform).Match(
                        Some: operation => (Reply)new Reply.Take(state.Aim.Editor.Map(editor => state.Field.Committed(operation(editor))).ToSeq(), None),
                        None: static () => new Reply.Pass())));

        internal IO<Unit> Committed(IO<Seq<Edit<string>>> edits) => edits.Bind(held => held.TraverseM(Edited).As()).Map(static _ => unit);

        private Track Typing(Aim aim) => new(gesture => new Reply.Take(Edits(gesture, aim), Some(Typing(aim))), static _ => [], static () => []) { Cursor = Cursor.Text };

        private Seq<IO<Unit>> Edits(Gesture gesture, Aim aim) =>
            (from editor in aim.Editor
             from text in aim.Laid.Texted
             from next in editor.Next(gesture, text, aim.Bounds)
             select Committed(next)).ToSeq();
    }

    public sealed record Viewport(PlotPlane Rest, Func<Edit<PlotPlane>, IO<Unit>> Edited) : Part {
        public Magnification Scales { get; init; } = Magnification.Free;

        internal Reply Begin(Gesture gesture, Aim aim) =>
            Framed(aim).Match(
                Some: frame => gesture.SwitchPartially(
                    (Viewport: this, Frame: frame),
                    @default: Reply.Passed,
                    press: static (held, press) => (held.Viewport.Tracking(held.Frame) with {
                        Press = press.Clicks >= 2
                            ? (_, _) => Some(held.Viewport.Rest)
                            : (start, moved) => from travel in held.Frame.Travel(moved.From, moved.At)
                                                from plane in start.Shifted(-travel)
                                                where (held.Frame with { Plane = plane }).Resolves
                                                select plane,
                    }).Began(press),
                    scroll: static (held, scroll) => (held.Viewport.Tracking(held.Frame) with {
                        Scroll = (start, slid) => from travel in held.Frame.Travel(slid.At, new SKPoint(slid.At.X + slid.Travel.X, slid.At.Y + slid.Travel.Y))
                                                  from plane in start.Shifted(-travel)
                                                  where (held.Frame with { Plane = plane }).Resolves
                                                  select plane,
                    }).Began(scroll),
                    magnify: static (held, magnify) => (held.Viewport.Tracking(held.Frame) with {
                        Magnify = (start, pinched) => from about in held.Frame.At(pinched.About)
                                                      from plane in held.Viewport.Scaled(start, about, 1f / (1f + pinched.Magnification), held.Frame)
                                                      select plane,
                    }).Began(magnify)),
                None: static () => new Reply.Pass());

        internal Reply Perform(Gesture gesture, Aim aim) =>
            gesture.SwitchPartially(
                (Viewport: this, Aim: aim),
                @default: Reply.Passed,
                scroll: static (held, scroll) => Framed(held.Aim).Match(
                    Some: frame => Handles.Metrics(held.Aim.Laid, None) switch {
                        var metrics => (held.Viewport.Tracking(frame) with {
                            Scroll = (start, notched) => from about in frame.At(notched.At)
                                                         from plane in held.Viewport.Scaled(start, about, MathF.Pow(metrics[Metric.Notch], -notched.Travel.Y / metrics[Metric.Extent]), frame)
                                                         select plane,
                        }).Stepped(scroll),
                    },
                    None: static () => new Reply.Pass()));

        private Session<PlotPlane> Tracking(PlotFrame frame) =>
            new(frame.Plane, [Sink<PlotPlane>.Of(static plane => plane, Edited)], Cursor.Grab) { Dragging = Cursor.Grabbing };

        private Option<PlotPlane> Scaled(PlotPlane start, Vector2 about, float factor, PlotFrame frame) =>
            from free in start.Dimensions.Head
            from rest in Rest.Dimension(free.Along)
            let ratio = MathF.Abs(free.Axis.Range.Span / rest.Range.Span)
            let held = float.Clamp(ratio * factor, Scales.Least, Scales.Most) / ratio
            from plane in held.CompareTo(1f) is 0 ? Some(start) : start.Scaled(about, held).Filter(scaled => (frame with { Plane = scaled }).Resolves)
            select plane;

        private static Option<PlotFrame> Framed(Aim aim) => Layouts.Plotted(aim.Laid).Map(frame => frame.Translated(aim.Bounds.Location));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EditExtensions {
    extension<TValue>(K<Edit, TValue> edit) {
        public Edit<TValue> As() => (Edit<TValue>)edit;
    }
}
