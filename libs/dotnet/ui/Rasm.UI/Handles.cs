using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HandlePoint(Key Key, Vector2 At);

public sealed record Dragged<TValue>(TValue Value, Seq<Key> Keys) where TValue : notnull;

public sealed record Added<TValue>(TValue Value, Key Key) where TValue : notnull;

public sealed record Dragging<TValue>(Func<TValue, Key, Option<TValue>> DraggedOut, Func<TValue, Key, float, Option<TValue>> Spread) where TValue : notnull;

public sealed record Points<TValue>(Func<TValue, Seq<HandlePoint>> Read, Func<TValue, Seq<HandlePoint>, KeyModifiers, Dragged<TValue>> Moved) where TValue : notnull {
    private static readonly Key Only = new Key.Ordinal(0);

    public Func<TValue, Vector2, Option<Key>, Option<Added<TValue>>> Placed { get; init; } = static (_, _, _) => None;
    public Func<TValue, Seq<Key>, Option<TValue>> Removed { get; init; } = static (_, _) => None;
    public Option<Dragging<TValue>> Dragging { get; init; }

    public static Points<TValue> Fixed(Seq<Lens<TValue, Vector2>> lenses) {
        HashMap<Key, Lens<TValue, Vector2>> keyed = toHashMap(lenses.Map(static (lens, index) => ((Key)Keyed(index), lens)));
        return new Points<TValue>(
            value => lenses.Map((lens, index) => new HandlePoint(Keyed(index), lens.Get(value))),
            (value, targets, _) => new Dragged<TValue>(targets.Fold(value, (held, target) => keyed.Find(target.Key).Map(lens => lens.Set(target.At, held)).IfNone(held)), targets.Map(static target => target.Key))) {
            Placed = (value, at, nearest) =>
                from key in nearest
                from lens in keyed.Find(key)
                select new Added<TValue>(lens.Set(at, value), key),
        };
    }

    public static Points<TValue> Listed<TItem>(
        Func<TValue, Seq<TItem>> items, Func<TItem, Vector2> at, Func<TItem, Vector2, Option<TItem>> moved, Func<TValue, Seq<TItem>, Option<TValue>> rebuilt, Func<TValue, Vector2, Option<TItem>> made) =>
        new(value => items(value).Map((item, index) => new HandlePoint(Keyed(index), at(item))),
            (value, targets, _) =>
                items(value).Map((item, index) => targets.Find(target => Ordinal(target.Key) == Some(index)).Match(Some: target => moved(item, target.At), None: () => Some(item))).Traverse(static item => item).As()
                    .Bind(shifted => rebuilt(value, shifted))
                    .Match(
                        Some: edited => new Dragged<TValue>(edited, Rekeyed(items(edited).Map(at), targets)),
                        None: () => new Dragged<TValue>(value, targets.Map(static target => target.Key)))) {
            Placed = (value, to, _) =>
                from item in made(value, to)
                from placed in rebuilt(value, items(value).Add(item))
                from key in Rekeyed(items(placed).Map(at), [new HandlePoint(Keyed(items(value).Count), at(item))]).Head
                select new Added<TValue>(placed, key),
            Removed = (value, keys) => rebuilt(value, Kept(items(value), keys)),
            Dragging = new Dragging<TValue>((value, key) => rebuilt(value, Kept(items(value), [key])), static (_, _, _) => None),
        };

    public static Points<TValue> Indexed(Lens<TValue, Seq<Vector2>> points) =>
        Listed(points.Get, static at => at, static (_, to) => Some(to), (value, held) => Some(points.Set(held, value)), static (_, at) => Some(at));

    public static Points<TValue> Scalar(PlotPlane.Track plane, Lens<TValue, float> value) =>
        new(held => [new HandlePoint(Only, plane.Along.With(new Vector2(0.5f), value.Get(held)))],
            (held, targets, _) => new Dragged<TValue>(targets.Fold(held, (state, target) => value.Set(plane.Along.Of(target.At), state)), targets.Map(static target => target.Key))) {
            Placed = (held, at, _) => Some(new Added<TValue>(value.Set(plane.Along.Of(at), held), Only)),
        };

    private static Key.Ordinal Keyed(int index) => new((uint)index);

    private static Option<int> Ordinal(Key key) => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None;

    private static Seq<TItem> Kept<TItem>(Seq<TItem> items, Seq<Key> keys) =>
        items.Map(static (item, index) => (Item: item, Index: index)).Filter(held => !keys.Exists(key => Ordinal(key) == Some(held.Index))).Map(static held => held.Item);

    private static Seq<Key> Rekeyed(Seq<Vector2> read, Seq<HandlePoint> targets) =>
        targets.Fold((Claimed: HashSet<int>(), Keys: Seq<Key>()), (held, target) =>
            toSeq(read.Map(static (at, index) => (At: at, Index: index)).Filter(point => !held.Claimed.Contains(point.Index))
                    .OrderBy(point => Vector2.DistanceSquared(point.At, target.At))
                    .ThenBy(point => Math.Abs(point.Index - Ordinal(target.Key).IfNone(point.Index))))
                .Head.Match(
                    Some: point => (held.Claimed.Add(point.Index), held.Keys.Add(Keyed(point.Index))),
                    None: () => (held.Claimed, held.Keys.Add(target.Key))))
        .Keys;
}

[SmartEnum]
public sealed partial class Marquee {
    public static readonly Marquee Box = new(
        static (press, points) => {
            SKRect box = new SKRect(press.From.X, press.From.Y, press.At.X, press.At.Y).Standardized;
            return points.Map(static (point, index) => (Point: point, Index: index)).Filter(held => box.Contains(held.Point)).Map(static held => held.Index);
        },
        static (frame, press) => press.Enclosed(frame).Map(static region => (Figure)region));

    public static readonly Marquee Lasso = new(
        static (press, points) => {
            using SKPathBuilder builder = new();
            builder.AddPoly([.. press.Trail.Map(static sample => sample.At)], close: true);
            using SKPath lasso = builder.Detach();
            return points.Map(static (point, index) => (Point: point, Index: index)).Filter(held => lasso.Contains(held.Point.X, held.Point.Y)).Map(static held => held.Index).Strict();
        },
        static (frame, press) => press.Trail.Choose(sample => frame.At(sample.At)) switch {
            [_, _, ..] and var traced => Some<Figure>(new Figure.Path(traced) { Closed = true }),
            _ => None,
        });

    [UseDelegateFromConstructor]
    internal partial Seq<int> Inside(Gesture.Press press, Seq<SKPoint> points);

    [UseDelegateFromConstructor]
    internal partial Option<Figure> Outlined(PlotFrame frame, Gesture.Press press);
}

public abstract record HandleLayer {
    private protected static readonly Key Background = new Key.Named("background");
    private protected static readonly Key Layer = new Key.Named("handles");

    private protected HandleLayer() { }

    public Selection Selection { get; init; } = new Selection.Many([]);
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }
    public Option<Marquee> Marquee { get; init; }
    public Option<CursorShape> BackgroundCursor { get; init; }
    public Func<Key, Vector2, Node> Handle { get; init; } = static (_, at) => new Node.Pin(at, new Node.Box()) { Role = Role.Handle, Align = UnitPoint.Center };

    public abstract Node Drawn(PlotPlane plane, Seq<Node> background);

    internal abstract Reply Began(Gesture gesture, HitTarget aim);

    internal abstract Reply Performed(Gesture gesture, HitTarget aim);

    internal Option<Seq<Key>> Revealed(LayoutNode layout) => Selection.Lead.Map(lead => layout.Path.Add(Layer).Add(lead));

    internal static bool Offers(LayoutNode layout, Seq<Key> path) => path == layout.Path || path.Init == layout.Path.Add(Layer);

    internal static HashMap<Metric, float> Metrics(LayoutNode layout, Option<Key> key) =>
        (from held in key
         from plot in Child(layout, Layer)
         from drawn in Child(plot, held)
         select drawn.Style.Metrics)
        .IfNone(() => Styles.Resolve(layout.Cascade, Role.Handle, layout.Style.Conditions).Metrics);

    private protected static Option<PlotFrame> Framed(LayoutNode layout, SKRect bounds) =>
        from plot in Child(layout, Layer)
        from frame in Layouts.Plotted(plot)
        select frame.Translated(new SKPoint(bounds.Left + plot.Frame.Left, bounds.Top + plot.Frame.Top));

    private protected static Option<Key> HitKey(HitTarget aim) {
        Seq<Key> layer = aim.Layout.Path.Add(Layer);
        return aim.Hit.Take(layer.Count) == layer ? aim.Hit.At(layer.Count) : None;
    }

    private protected static float Spacing(LayoutNode layout) => Styles.Resolve(layout.Cascade, Role.Grid, layout.Node.Conditions).Metrics[Metric.Spacing];

    private protected static CursorShape DragCursor(PlotFrame frame) =>
        frame.Plane.Switch(cartesian: static _ => CursorShape.Move, track: static track => CursorShape.Resizing(track.Along), polar: static _ => CursorShape.Move, mapped: static _ => CursorShape.Move);

    private static Option<LayoutNode> Child(LayoutNode layout, Key key) => layout.Children.Find(child => child.Path.Last == Some(key));
}

public sealed record Handles<TValue>(TValue Value, Points<TValue> Points, Func<Edit<TValue>, IO<Unit>> Edited) : HandleLayer where TValue : notnull {
    // --- [STATE]
    private static readonly SKRect Square = new(0f, 0f, 1f, 1f);

    public Option<TValue> Rest { get; init; }
    public Option<float> Gearing { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<string> Unit { get; init; }

    private State Start => new(Value, Selection, None);

    private Seq<Receiver<State>> Receivers =>
        Seq(Receiver<State>.Of(static held => held.Value, edit => Some(Edited(edit)))) + Selected.Map(static selected => Receiver<State>.Of(static held => held.Selection, edit => Some(selected(edit)))).ToSeq();

    // --- [DRAW]
    public override Node Drawn(PlotPlane plane, Seq<Node> background) {
        bool modified = Rest.Exists(rest => !rest.Equals(Value));
        Option<PlaneAxis> track = plane.Dimensions is [var only] ? Some(only) : None;
        return new Node.Overlay(
            new Node.Plot(plane, background.Map(node => Modified(node, modified))) { Key = Background, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
            [
                new Node.Plot(plane, Points.Read(Value).Map(point => HandleNode(Handle(point.Key, point.At), point, modified, plane, track))) {
                    Key = Layer,
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                },
            ]) {
            Part = new Part.Pad(this),
        };
    }

    private Node HandleNode(Node node, HandlePoint point, bool modified, PlotPlane plane, Option<PlaneAxis> track) {
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

    // --- [GESTURES]
    internal override Reply Began(Gesture gesture, HitTarget aim) =>
        Framed(aim.Layout, aim.Bounds).Match(
            Some: frame => gesture.SwitchPartially(
                (Handles: this, Aim: aim, Frame: frame),
                @default: Reply.Ignore,
                press: static (held, press) => held.Handles.Pressed(press, held.Aim, held.Frame),
                scroll: static (held, scroll) => held.Handles.Wheeled(held.Aim, held.Frame).Match(Some: session => session.Began(scroll), None: static () => new Reply.Ignored()),
                rotate: static (held, rotate) => held.Handles.Rotating(held.Aim, held.Frame).Match(Some: session => session.Began(rotate), None: static () => new Reply.Ignored())),
            None: static () => new Reply.Ignored());

    internal override Reply Performed(Gesture gesture, HitTarget aim) =>
        Framed(aim.Layout, aim.Bounds).Match(
            Some: frame => gesture.SwitchPartially(
                (Handles: this, Aim: aim, Frame: frame),
                @default: Reply.Ignore,
                scroll: static (held, scroll) => held.Handles.Wheeled(held.Aim, held.Frame).Match(Some: session => session.Stepped(scroll), None: static () => new Reply.Ignored()),
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    (held.Handles, held.Aim, held.Frame, Perform: perform),
                    @default: static (_, _) => new Reply.Ignored(),
                    increment: static (state, _) => state.Handles.Nudged(state.Aim, state.Frame, state.Perform, (point, key) => Incremented(state.Aim, state.Frame, point, key, 1)),
                    decrement: static (state, _) => state.Handles.Nudged(state.Aim, state.Frame, state.Perform, (point, key) => Incremented(state.Aim, state.Frame, point, key, -1)),
                    reset: static (state, _) => state.Handles.Rest.Match(
                        Some: rest => (state.Handles.Session(CursorShape.Default) with { Step = (start, gesture) => gesture is Gesture.Perform ? Some(start with { Value = rest }) : None }).Stepped(state.Perform),
                        None: static () => new Reply.Ignored()),
                    navigate: static (state, navigate) => state.Handles.Nudged(state.Aim, state.Frame, state.Perform, (point, key) => Navigated(state.Aim, state.Frame, navigate.Toward, point, key)),
                    delete: static (state, _) => {
                        Seq<Key> keys = state.Handles.DeletedKeys(state.Aim);
                        return (state.Handles.Session(CursorShape.Default) with {
                            Step = (start, gesture) => gesture is Gesture.Perform ? state.Handles.Points.Removed(start.Value, keys).Map(value => start with { Value = value }) : None,
                        }).Stepped(state.Perform);
                    },
                    selectAll: static (state, _) => state.Handles.Selected.IsSome
                        ? (state.Handles.Session(CursorShape.Default) with {
                            Step = (start, gesture) => gesture is Gesture.Perform ? Some(start with { Selection = start.Selection.All(state.Handles.Points.Read(start.Value).Map(static point => point.Key)) }) : None,
                        }).Stepped(state.Perform)
                        : new Reply.Ignored())),
            None: static () => new Reply.Ignored());

    private Session<State> Session(CursorShape cursor) => new(Start, Receivers, cursor) { Detent = static held => held.Detent, Preview = static held => held.MarqueeOutline };

    private Reply Pressed(Gesture.Press press, HitTarget aim, PlotFrame frame) {
        Option<TValue> restored = Rest.Filter(_ => press.Clicks >= 2);
        return HitKey(aim).Match(
            Some: key => (from rest in restored
                          from point in Points.Read(rest).Find(point => point.Key == key)
                          select Session(DragCursor(frame)) with { Step = (start, gesture) => gesture is Gesture.Press ? Some(Moved(start, [point], press.Modifiers, None)) : None })
                .IfNone(() => Dragging(key, aim, frame))
                .Began(press),
            None: () => restored.Match(
                Some: rest => (Session(BackgroundCursor.IfNone(DragCursor(frame))) with { Step = (start, gesture) => gesture is Gesture.Press ? Some(start with { Value = rest }) : None }).Began(press),
                None: () => Marquee.Filter(_ => Selected.IsSome).Match(
                    Some: marquee => MarqueeSession(marquee, frame).Began(press),
                    None: () => frame.At(press.From).Match(
                    Some: target => (Session(BackgroundCursor.IfNone(DragCursor(frame))) with {
                        Step = (start, gesture) => gesture is Gesture.Press moved
                            ? from placed in Points.Placed(start.Value, target, Nearest(start.Value, target, frame))
                              select Dragged(start with { Value = placed.Value }, placed.Key, moved, aim, frame)
                            : None,
                    }).Began(press),
                    None: static () => new Reply.Ignored()))));
    }

    private Session<State> MarqueeSession(Marquee marquee, PlotFrame frame) =>
        Session(CursorShape.Crosshair) with {
            Step = (start, gesture) => gesture is Gesture.Press press
                ? Some(start with {
                    Selection = start.Selection.Enclosed(Points.Read(start.Value).Map(static point => point.Key), Covered(marquee, frame, press, Points.Read(start.Value)), press.Modifiers),
                    MarqueeOutline = marquee.Outlined(frame, press),
                })
                : None,
        };

    private static HashSet<Key> Covered(Marquee marquee, PlotFrame frame, Gesture.Press press, Seq<HandlePoint> read) =>
        press.Dragged.IsNone ? [] : toHashSet(marquee.Inside(press, read.Map(point => frame.Point(point.At))).Choose(index => read.At(index)).Map(static point => point.Key));

    private Session<State> Dragging(Key key, HitTarget aim, PlotFrame frame) =>
        Session(DragCursor(frame)) with { Step = (start, gesture) => gesture is Gesture.Press press ? Some(Dragged(Spread(start, key, press, aim), key, press, aim, frame)) : None };

    private State Spread(State start, Key key, Gesture.Press press, HitTarget aim) =>
        (from dragging in Points.Dragging
         where press.Scrolled.Y.CompareTo(0f) is not 0
         from value in dragging.Spread(start.Value, key, press.Scrolled.Y / Metrics(aim.Layout, Some(key))[Metric.Extent])
         select start with { Value = value }).IfNone(start);

    private State Dragged(State start, Key key, Gesture.Press press, HitTarget aim, PlotFrame frame) =>
        start.Selection.Pressed(Points.Read(start.Value).Map(static point => point.Key), key, press.Modifiers) switch {
            var selection when press.Dragged.IsNone => start with { Selection = selection },
            var selection => DraggedOut(start.Value, key, press, aim, frame).Match(
                Some: remaining => new State(remaining, selection, None),
                None: () => Translated(start with { Selection = selection }, key, press, aim, frame)),
        };

    private Option<TValue> DraggedOut(TValue value, Key key, Gesture.Press press, HitTarget aim, PlotFrame frame) {
        float reach = Metrics(aim.Layout, Some(key))[Metric.Reach];
        return SKRect.Inflate(frame.Matrix.MapRect(Square), reach, reach).Contains(press.At) ? None : Points.Dragging.Bind(dragging => dragging.DraggedOut(value, key));
    }

    private State Translated(State start, Key key, Gesture.Press press, HitTarget aim, PlotFrame frame) {
        bool fine = press.Modifiers.Primary;
        HashMap<Metric, float> metrics = Metrics(aim.Layout, Some(key));
        Seq<HandlePoint> read = Points.Read(start.Value);
        HashSet<Key> moving = toHashSet(read.Map(static point => point.Key).Filter(held => start.Selection.Holds(key) ? start.Selection.Holds(held) : held == key));
        Vector2 travel = Gearing.Match(Some: gearing => Geared(frame, press, gearing), None: () => frame.Delta(press.From, press.At).IfNone(Vector2.Zero)) * (fine ? metrics[Metric.Fine] : 1f);
        Seq<HandlePoint> targets = read.Filter(point => moving.Contains(point.Key)).Map(point => point with { At = Snapped(frame, frame.Plane.Constrained(point.At + travel), press.Modifiers, aim.Layout) });
        Seq<Vector2> detents = Rest.Map(rest => Points.Read(rest).Map(static point => point.At)).IfNone([]) + read.Filter(point => !moving.Contains(point.Key)).Map(static point => point.At);
        Option<Vector2> detent = targets is [var only] && !fine ? frame.Nearest(only.At, detents, metrics[Metric.Reach]).Bind(index => detents.At(index)) : None;
        return Moved(start, detent.Match(Some: to => targets.Map(target => target with { At = to }), None: () => targets), press.Modifiers, detent);
    }

    private static Vector2 Geared(PlotFrame frame, Gesture.Press press, float gearing) =>
        frame.Plane.Dimensions.Fold(Vector2.Zero, (travel, free) =>
            free.Along.With(travel, free.Along.Of((Vector2)(press.At - press.From)) * gearing * MathF.Sign(free.Along.Of(new Vector2(frame.Matrix.ScaleX, frame.Matrix.ScaleY)))));

    private State Moved(State start, Seq<HandlePoint> targets, KeyModifiers modifiers, Option<Vector2> detent) {
        Dragged<TValue> moved = Points.Moved(start.Value, targets, modifiers);
        return new State(moved.Value, start.Selection.Renamed(toHashMap(targets.Map(static target => target.Key).Zip(moved.Keys))), detent);
    }

    private Option<Session<State>> Moving(HitTarget aim, PlotFrame frame, Func<State, Key, Vector2, Gesture, Option<State>> step) =>
        from key in Target(aim)
        from point in Points.Read(Value).Find(point => point.Key == key)
        select Session(DragCursor(frame)) with { Step = (start, gesture) => step(start, key, point.At, gesture) };

    private Option<Session<State>> Wheeled(HitTarget aim, PlotFrame frame) =>
        Moving(aim, frame, (start, key, point, gesture) => gesture is Gesture.Scroll scroll ? Some(Notched(start, key, point, scroll, frame, Metrics(aim.Layout, Some(key))[Metric.Extent])) : None);

    private State Notched(State start, Key key, Vector2 point, Gesture.Scroll scroll, PlotFrame frame, float notch) =>
        Notches(scroll.Delta, notch, frame.Plane.Dimensions.Count > 1) switch {
            var steps when steps == Vector2.Zero => start,
            var steps => Moved(start, [new HandlePoint(key, Stepped(frame, point, steps, notch))], scroll.Modifiers, None),
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

    private Option<Session<State>> Rotating(HitTarget aim, PlotFrame frame) =>
        Moving(aim, frame, (start, key, point, gesture) => gesture is Gesture.Rotate rotate ? frame.Plane.Rotated(point, rotate.Rotation).Map(to => Moved(start, [new HandlePoint(key, to)], rotate.Modifiers, None)) : None);

    private Reply Nudged(HitTarget aim, PlotFrame frame, Gesture.Perform perform, Func<Vector2, Key, Vector2> move) =>
        Moving(aim, frame, (start, key, point, gesture) => gesture is Gesture.Perform ? Some(Moved(start, [new HandlePoint(key, move(point, key))], perform.Modifiers, None)) : None)
            .Match(Some: session => session.Stepped(perform), None: static () => new Reply.Ignored());

    private static Vector2 Incremented(HitTarget aim, PlotFrame frame, Vector2 point, Key key, int sign) =>
        frame.Plane.Dimensions.Head.Map(first => frame.Stepped(point, first.Along, sign, Metrics(aim.Layout, Some(key))[Metric.Extent])).IfNone(point);

    private static Vector2 Navigated(HitTarget aim, PlotFrame frame, NavigationDirection toward, Vector2 point, Key key) =>
        toward.Granularity.Distance(Metrics(aim.Layout, Some(key))[Metric.Extent], Spacing(aim.Layout)).Match(
            Some: stride => frame.Stepped(point, toward.Along, toward.ValueSign(frame), stride),
            None: () => frame.Ended(point, toward.Along, toward.Sign));

    private static Vector2 Snapped(PlotFrame frame, Vector2 value, KeyModifiers modifiers, LayoutNode layout) =>
        modifiers.Shift ? frame.Plane.Dimensions.Fold(value, (held, free) => frame.Stepped(held, free.Along, 0, Spacing(layout))) : value;

    // --- [READ]
    private Option<Key> Lead => Selection.Lead | (Points.Read(Value) is [var only] ? Some(only.Key) : None);

    private Option<Key> Target(HitTarget aim) => HitKey(aim) | Lead;

    private Seq<Key> DeletedKeys(HitTarget aim) =>
        Points.Read(Value).Map(static point => point.Key).Filter(Selection.Holds) switch {
            { IsEmpty: true } => Target(aim).ToSeq(),
            var selection => selection,
        };

    private Option<Key> Nearest(TValue value, Vector2 target, PlotFrame frame) {
        Seq<HandlePoint> read = Points.Read(value);
        return frame.Nearest(target, read.Map(static point => point.At), float.PositiveInfinity).Bind(index => read.At(index)).Map(static point => point.Key);
    }

    private sealed record State(TValue Value, Selection Selection, Option<Vector2> Detent) {
        internal Option<Figure> MarqueeOutline { get; init; }
    }
}
