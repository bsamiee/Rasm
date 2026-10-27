using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Anchor(Key Key, Vector2 At);

public sealed record Dragged<TValue>(TValue Value, Seq<Key> Keys) where TValue : notnull;

public sealed record Added<TValue>(TValue Value, Key Key) where TValue : notnull;

public sealed record Dragging<TValue>(Func<TValue, Key, Option<TValue>> Torn, Func<TValue, Key, float, Option<TValue>> Spread) where TValue : notnull;

public sealed record Points<TValue>(Func<TValue, Seq<Anchor>> Read, Func<TValue, Seq<Anchor>, KeyModifiers, Dragged<TValue>> Moved) where TValue : notnull {
    private static readonly Key Only = new Key.Ordinal(0);

    public Func<TValue, Vector2, Option<Key>, Option<Added<TValue>>> Placed { get; init; } = static (_, _, _) => None;
    public Func<TValue, Seq<Key>, Option<TValue>> Removed { get; init; } = static (_, _) => None;
    public Option<Dragging<TValue>> Dragging { get; init; }

    public static Points<TValue> Fixed(Seq<Lens<TValue, Vector2>> lenses) {
        HashMap<Key, Lens<TValue, Vector2>> keyed = toHashMap(lenses.Map(static (lens, index) => ((Key)Keyed(index), lens)));
        return new Points<TValue>(
            value => lenses.Map((lens, index) => new Anchor(Keyed(index), lens.Get(value))),
            (value, targets, _) => new Dragged<TValue>(targets.Fold(value, (held, target) => keyed.Find(target.Key).Map(lens => lens.Set(target.At, held)).IfNone(held)), targets.Map(static target => target.Key))) {
            Placed = (value, at, nearest) =>
                from key in nearest
                from lens in keyed.Find(key)
                select new Added<TValue>(lens.Set(at, value), key),
        };
    }

    public static Points<TValue> Listed<TItem>(
        Func<TValue, Seq<TItem>> items, Func<TItem, Vector2> at, Func<TItem, Vector2, Option<TItem>> moved, Func<TValue, Seq<TItem>, Option<TValue>> rebuilt, Func<TValue, Vector2, Option<TItem>> made) =>
        new(value => items(value).Map((item, index) => new Anchor(Keyed(index), at(item))),
            (value, targets, _) =>
                items(value).Map((item, index) => targets.Find(target => Ordinal(target.Key) == Some(index)).Match(Some: target => moved(item, target.At), None: () => Some(item))).Traverse(static item => item).As()
                    .Bind(shifted => rebuilt(value, shifted))
                    .Match(
                        Some: edited => new Dragged<TValue>(edited, Rekeyed(items(edited).Map(at), targets)),
                        None: () => new Dragged<TValue>(value, targets.Map(static target => target.Key)))) {
            Placed = (value, to, _) =>
                from item in made(value, to)
                from placed in rebuilt(value, items(value).Add(item))
                from key in Rekeyed(items(placed).Map(at), [new Anchor(Keyed(items(value).Count), at(item))]).Head
                select new Added<TValue>(placed, key),
            Removed = (value, keys) => rebuilt(value, Kept(items(value), keys)),
            Dragging = new Dragging<TValue>((value, key) => rebuilt(value, Kept(items(value), [key])), static (_, _, _) => None),
        };

    public static Points<TValue> Indexed(Lens<TValue, Seq<Vector2>> points) =>
        Listed(points.Get, static at => at, static (_, to) => Some(to), (value, held) => Some(points.Set(held, value)), static (_, at) => Some(at));

    public static Points<TValue> Scalar(PlotPlane.Track plane, Lens<TValue, float> value) =>
        new(held => [new Anchor(Only, plane.Along.With(new Vector2(0.5f), value.Get(held)))],
            (held, targets, _) => new Dragged<TValue>(targets.Fold(held, (state, target) => value.Set(plane.Along.Of(target.At), state)), targets.Map(static target => target.Key))) {
            Placed = (held, at, _) => Some(new Added<TValue>(value.Set(plane.Along.Of(at), held), Only)),
        };

    private static Key.Ordinal Keyed(int index) => new((uint)index);

    private static Option<int> Ordinal(Key key) => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None;

    private static Seq<TItem> Kept<TItem>(Seq<TItem> items, Seq<Key> keys) =>
        items.Map(static (item, index) => (Item: item, Index: index)).Filter(held => !keys.Exists(key => Ordinal(key) == Some(held.Index))).Map(static held => held.Item);

    private static Seq<Key> Rekeyed(Seq<Vector2> read, Seq<Anchor> targets) =>
        targets.Fold((Claimed: HashSet<int>(), Keys: Seq<Key>()), (held, target) =>
            toSeq(read.Map(static (at, index) => (At: at, Index: index)).Filter(point => !held.Claimed.Contains(point.Index))
                    .OrderBy(point => Vector2.DistanceSquared(point.At, target.At))
                    .ThenBy(point => Math.Abs(point.Index - Ordinal(target.Key).IfNone(point.Index))))
                .Head.Match(
                    Some: point => (held.Claimed.Add(point.Index), held.Keys.Add(Keyed(point.Index))),
                    None: () => (held.Claimed, held.Keys.Add(target.Key))))
        .Keys;
}

public abstract record HandleLayer {
    private protected static readonly Key Ground = new Key.Named("ground");
    private protected static readonly Key Layer = new Key.Named("handles");

    private protected HandleLayer() { }

    public Selection Picked { get; init; } = new Selection.Many([]);
    public Option<Func<Edit<Selection>, IO<Unit>>> Pick { get; init; }
    public Option<CursorShape> GroundCursor { get; init; }
    public Func<Key, Vector2, Node> Handle { get; init; } = static (_, at) => new Node.Pin(at, new Node.Box()) { Role = Role.Handle, Align = UnitPoint.Center };

    public abstract Node Drawn(PlotPlane plane, Seq<Node> ground);

    internal abstract Reply Began(Gesture gesture, Aim aim);

    internal abstract Reply Performed(Gesture gesture, Aim aim);

    internal Option<Seq<Key>> Revealed(Laid laid) => Picked.Lead.Map(lead => laid.Path.Add(Layer).Add(lead));

    internal static bool Offers(Laid laid, Seq<Key> path) => path == laid.Path || path.Init == laid.Path.Add(Layer);

    internal static HashMap<Metric, float> Metrics(Laid laid, Option<Key> key) =>
        (from held in key
         from plot in Child(laid, Layer)
         from drawn in Child(plot, held)
         select drawn.Style.Metrics)
        .IfNone(() => Styles.Resolve(laid.Cascade, Role.Handle, laid.Style.Conditions).Metrics);

    private protected static Option<PlotFrame> Framed(Laid laid, SKRect bounds) =>
        from plot in Child(laid, Layer)
        from frame in Layouts.Plotted(plot)
        select frame.Translated(new SKPoint(bounds.Left + plot.Frame.Left, bounds.Top + plot.Frame.Top));

    private protected static Option<Key> Point(Aim aim) {
        Seq<Key> layer = aim.Laid.Path.Add(Layer);
        return aim.Over.Take(layer.Count) == layer ? aim.Over.At(layer.Count) : None;
    }

    private protected static float Spacing(Laid laid) => Styles.Resolve(laid.Cascade, Role.Grid, laid.Node.Conditions).Metrics[Metric.Spacing];

    private protected static CursorShape Gripped(PlotFrame frame) =>
        frame.Plane.Switch(cartesian: static _ => CursorShape.Move, track: static track => CursorShape.Resizing(track.Along), polar: static _ => CursorShape.Move);

    private static Option<Laid> Child(Laid laid, Key key) => laid.Children.Find(child => child.Path.Last == Some(key));
}

public sealed record Handles<TValue>(TValue Value, Points<TValue> Points, Func<Edit<TValue>, IO<Unit>> Edited) : HandleLayer where TValue : notnull {
    // --- [STATE]
    private static readonly SKRect Square = new(0f, 0f, 1f, 1f);

    public Option<TValue> Rest { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<string> Unit { get; init; }

    private Held Start => new(Value, Picked, None);

    private Seq<Receiver<Held>> Receivers =>
        Seq(Receiver<Held>.Of(static held => held.Value, edit => Some(Edited(edit)))) + Pick.Map(static pick => Receiver<Held>.Of(static held => held.Picked, edit => Some(pick(edit)))).ToSeq();

    // --- [DRAW]
    public override Node Drawn(PlotPlane plane, Seq<Node> ground) {
        bool modified = Rest.Exists(rest => !rest.Equals(Value));
        Option<Dimension> track = plane.Dimensions is [var only] ? Some(only) : None;
        return new Node.Overlay(
            new Node.Plot(plane, ground.Map(node => Modified(node, modified))) { Key = Ground, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
            [
                new Node.Plot(plane, Points.Read(Value).Map(point => Stamped(Handle(point.Key, point.At), point, modified, plane, track))) {
                    Key = Layer,
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                },
            ]) {
            Part = new Part.Pad(this),
        };
    }

    private Node Stamped(Node node, Anchor point, bool modified, PlotPlane plane, Option<Dimension> track) {
        Node held = Modified(node, modified);
        return held with {
            Key = point.Key,
            Role = held.Role | Some(Role.Handle),
            Accessible = held.Accessible.Combine(new Accessible {
                Role = track.IsSome ? AccessRole.Slider : AccessRole.Handle,
                Value = track.Match(
                    Some: axis => (AccessValue)new AccessValue.Ranged(axis.Along.Of(point.At), axis.Axis.Domain.Low, axis.Axis.Domain.High) { Decimals = Decimals | Some(axis.Axis.Decimals), Unit = Unit },
                    None: () => new AccessValue.Point(plane, point.At)),
                Orientation = track.Map(static axis => axis.Along),
            }),
        };
    }

    private static Node Modified(Node node, bool modified) => modified ? node.Conditioned(Condition.Modified) : node;

    // --- [ANSWER]
    internal override Reply Began(Gesture gesture, Aim aim) =>
        Framed(aim.Laid, aim.Bounds).Match(
            Some: frame => gesture.SwitchPartially(
                (Handles: this, Aim: aim, Frame: frame),
                @default: Reply.Passed,
                press: static (held, press) => held.Handles.Pressed(press, held.Aim, held.Frame),
                scroll: static (held, scroll) => held.Handles.Wheeled(held.Aim, held.Frame).Match(Some: session => session.Began(scroll), None: static () => new Reply.Pass()),
                rotate: static (held, rotate) => held.Handles.Turning(held.Aim, held.Frame).Match(Some: session => session.Began(rotate), None: static () => new Reply.Pass())),
            None: static () => new Reply.Pass());

    internal override Reply Performed(Gesture gesture, Aim aim) =>
        Framed(aim.Laid, aim.Bounds).Match(
            Some: frame => gesture.SwitchPartially(
                (Handles: this, Aim: aim, Frame: frame),
                @default: Reply.Passed,
                scroll: static (held, scroll) => held.Handles.Wheeled(held.Aim, held.Frame).Match(Some: session => session.Stepped(scroll), None: static () => new Reply.Pass()),
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    (held.Handles, held.Aim, held.Frame, Perform: perform),
                    @default: static (_, _) => new Reply.Pass(),
                    increment: static (state, _) => state.Handles.Nudged(state.Aim, state.Frame, state.Perform, (point, key) => Bumped(state.Aim, state.Frame, point, key, 1)),
                    decrement: static (state, _) => state.Handles.Nudged(state.Aim, state.Frame, state.Perform, (point, key) => Bumped(state.Aim, state.Frame, point, key, -1)),
                    navigate: static (state, navigate) => state.Handles.Nudged(state.Aim, state.Frame, state.Perform, (point, key) => Navigated(state.Aim, state.Frame, navigate.Toward, point, key)),
                    delete: static (state, _) => {
                        Seq<Key> keys = state.Handles.Doomed(state.Aim);
                        return (state.Handles.Session(CursorShape.Default) with {
                            Step = (start, gesture) => gesture is Gesture.Perform ? state.Handles.Points.Removed(start.Value, keys).Map(value => start with { Value = value }) : None,
                        }).Stepped(state.Perform);
                    },
                    selectAll: static (state, _) => state.Handles.Pick.IsSome
                        ? (state.Handles.Session(CursorShape.Default) with {
                            Step = (start, gesture) => gesture is Gesture.Perform ? Some(start with { Picked = start.Picked.All(state.Handles.Points.Read(start.Value).Map(static point => point.Key)) }) : None,
                        }).Stepped(state.Perform)
                        : new Reply.Pass())),
            None: static () => new Reply.Pass());

    private Session<Held> Session(CursorShape cursor) => new(Start, Receivers, cursor) { Detent = static held => held.Detent };

    private Reply Pressed(Gesture.Press press, Aim aim, PlotFrame frame) {
        Option<TValue> restored = Rest.Filter(_ => press.Clicks >= 2);
        return Point(aim).Match(
            Some: key => (from rest in restored
                          from point in Points.Read(rest).Find(point => point.Key == key)
                          select Session(Gripped(frame)) with { Step = (start, gesture) => gesture is Gesture.Press ? Some(Moved(start, [point], press.Modifiers, None)) : None })
                .IfNone(() => Dragging(key, aim, frame))
                .Began(press),
            None: () => restored.Match(
                Some: rest => (Session(GroundCursor.IfNone(Gripped(frame))) with { Step = (start, gesture) => gesture is Gesture.Press ? Some(start with { Value = rest }) : None }).Began(press),
                None: () => frame.At(press.From).Match(
                    Some: target => (Session(GroundCursor.IfNone(Gripped(frame))) with {
                        Step = (start, gesture) => gesture is Gesture.Press moved
                            ? from placed in Points.Placed(start.Value, target, Nearest(start.Value, target, frame))
                              select Dragged(start with { Value = placed.Value }, placed.Key, moved, aim, frame)
                            : None,
                    }).Began(press),
                    None: static () => new Reply.Pass())));
    }

    private Session<Held> Dragging(Key key, Aim aim, PlotFrame frame) =>
        Session(Gripped(frame)) with { Step = (start, gesture) => gesture is Gesture.Press press ? Some(Dragged(Spread(start, key, press, aim), key, press, aim, frame)) : None };

    private Held Spread(Held start, Key key, Gesture.Press press, Aim aim) =>
        (from dragging in Points.Dragging
         where press.Scrolled.Y.CompareTo(0f) is not 0
         from value in dragging.Spread(start.Value, key, press.Scrolled.Y / Metrics(aim.Laid, Some(key))[Metric.Extent])
         select start with { Value = value }).IfNone(start);

    private Held Dragged(Held start, Key key, Gesture.Press press, Aim aim, PlotFrame frame) =>
        start.Picked.Raised(Points.Read(start.Value).Map(static point => point.Key), key, press.Modifiers) switch {
            var picked when press.Dragged.IsNone => start with { Picked = picked },
            var picked => Tearing(start.Value, key, press, aim, frame).Match(
                Some: torn => new Held(torn, picked, None),
                None: () => Translated(start with { Picked = picked }, key, press, aim, frame)),
        };

    private Option<TValue> Tearing(TValue value, Key key, Gesture.Press press, Aim aim, PlotFrame frame) {
        float reach = Metrics(aim.Laid, Some(key))[Metric.Reach];
        return SKRect.Inflate(frame.Matrix.MapRect(Square), reach, reach).Contains(press.At) ? None : Points.Dragging.Bind(dragging => dragging.Torn(value, key));
    }

    private Held Translated(Held start, Key key, Gesture.Press press, Aim aim, PlotFrame frame) {
        bool fine = press.Modifiers.Common;
        HashMap<Metric, float> metrics = Metrics(aim.Laid, Some(key));
        Seq<Anchor> read = Points.Read(start.Value);
        HashSet<Key> moving = toHashSet(read.Map(static point => point.Key).Filter(held => start.Picked.Holds(key) ? start.Picked.Holds(held) : held == key));
        Vector2 travel = frame.Travel(press.From, press.At).IfNone(Vector2.Zero) * (fine ? metrics[Metric.Fine] : 1f);
        Seq<Anchor> targets = read.Filter(point => moving.Contains(point.Key)).Map(point => point with { At = Snapped(frame, frame.Plane.Held(point.At + travel), press.Modifiers, aim.Laid) });
        Seq<Vector2> detents = Rest.Map(rest => Points.Read(rest).Map(static point => point.At)).IfNone([]) + read.Filter(point => !moving.Contains(point.Key)).Map(static point => point.At);
        Option<Vector2> detent = targets is [var only] && !fine ? frame.Nearest(only.At, detents, metrics[Metric.Reach]).Bind(index => detents.At(index)) : None;
        return Moved(start, detent.Match(Some: to => targets.Map(target => target with { At = to }), None: () => targets), press.Modifiers, detent);
    }

    private Held Moved(Held start, Seq<Anchor> targets, KeyModifiers modifiers, Option<Vector2> detent) {
        Dragged<TValue> moved = Points.Moved(start.Value, targets, modifiers);
        return new Held(moved.Value, start.Picked.Renamed(toHashMap(targets.Map(static target => target.Key).Zip(moved.Keys))), detent);
    }

    private Option<Session<Held>> Moving(Aim aim, PlotFrame frame, Func<Held, Key, Vector2, Gesture, Option<Held>> step) =>
        from key in Target(aim)
        from point in Points.Read(Value).Find(point => point.Key == key)
        select Session(Gripped(frame)) with { Step = (start, gesture) => step(start, key, point.At, gesture) };

    private Option<Session<Held>> Wheeled(Aim aim, PlotFrame frame) =>
        Moving(aim, frame, (start, key, point, gesture) => gesture is Gesture.Scroll scroll ? Some(Notched(start, key, point, scroll, frame, Metrics(aim.Laid, Some(key))[Metric.Extent])) : None);

    private Held Notched(Held start, Key key, Vector2 point, Gesture.Scroll scroll, PlotFrame frame, float notch) =>
        Notches(scroll.Travel, notch, frame.Plane.Dimensions.Count > 1) switch {
            var steps when steps == Vector2.Zero => start,
            var steps => Moved(start, [new Anchor(key, Stepped(frame, point, steps, notch))], scroll.Modifiers, None),
        };

    private static Vector2 Notches(Vector2 travel, float notch, bool planar) {
        Vector2 notches = new(MathF.Truncate(travel.X / notch), MathF.Truncate(travel.Y / notch));
        return planar ? notches : MathF.Abs(travel.X) > MathF.Abs(travel.Y) ? notches with { Y = 0f } : notches with { X = 0f };
    }

    private static Vector2 Stepped(PlotFrame frame, Vector2 point, Vector2 steps, float notch) =>
        frame.Plane.Dimensions.Fold(point, (moved, free) => (frame.Plane.Dimensions.Count > 1 ? free.Along.Of(steps) : steps.X + steps.Y) switch {
            var count when count.CompareTo(0f) is 0 => moved,
            var count => frame.Stepped(moved, free.Along, (int)count, notch),
        });

    private Option<Session<Held>> Turning(Aim aim, PlotFrame frame) =>
        Moving(aim, frame, (start, key, point, gesture) => gesture is Gesture.Rotate rotate ? frame.Plane.Turned(point, rotate.Rotation).Map(to => Moved(start, [new Anchor(key, to)], rotate.Modifiers, None)) : None);

    private Reply Nudged(Aim aim, PlotFrame frame, Gesture.Perform perform, Func<Vector2, Key, Vector2> move) =>
        Moving(aim, frame, (start, key, point, gesture) => gesture is Gesture.Perform ? Some(Moved(start, [new Anchor(key, move(point, key))], perform.Modifiers, None)) : None)
            .Match(Some: session => session.Stepped(perform), None: static () => new Reply.Pass());

    private static Vector2 Bumped(Aim aim, PlotFrame frame, Vector2 point, Key key, int sign) =>
        frame.Plane.Dimensions.Head.Map(first => frame.Stepped(point, first.Along, sign, Metrics(aim.Laid, Some(key))[Metric.Extent])).IfNone(point);

    private static Vector2 Navigated(Aim aim, PlotFrame frame, NavigationDirection toward, Vector2 point, Key key) =>
        toward.Grain.Stride(Metrics(aim.Laid, Some(key))[Metric.Extent], Spacing(aim.Laid)).Match(
            Some: stride => frame.Stepped(point, toward.Along, toward.Valued(frame), stride),
            None: () => frame.Ended(point, toward.Along, toward.Sign));

    private static Vector2 Snapped(PlotFrame frame, Vector2 value, KeyModifiers modifiers, Laid laid) =>
        modifiers.Shift ? frame.Plane.Dimensions.Fold(value, (held, free) => frame.Stepped(held, free.Along, 0, Spacing(laid))) : value;

    // --- [READ]
    private Option<Key> Lead => Picked.Lead | (Points.Read(Value) is [var only] ? Some(only.Key) : None);

    private Option<Key> Target(Aim aim) => Point(aim) | Lead;

    private Seq<Key> Doomed(Aim aim) =>
        Points.Read(Value).Map(static point => point.Key).Filter(Picked.Holds) switch {
            { IsEmpty: true } => Target(aim).ToSeq(),
            var picked => picked,
        };

    private Option<Key> Nearest(TValue value, Vector2 target, PlotFrame frame) {
        Seq<Anchor> read = Points.Read(value);
        return frame.Nearest(target, read.Map(static point => point.At), float.PositiveInfinity).Bind(index => read.At(index)).Map(static point => point.Key);
    }

    private sealed record Held(TValue Value, Selection Picked, Option<Vector2> Detent);
}
