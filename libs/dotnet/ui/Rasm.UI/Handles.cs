using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HandlePoint(Key Key, Vector2 At);

public sealed record Dragged<TValue>(TValue Value, Seq<Key> Keys) where TValue : notnull;

public sealed record Points<TValue>(Func<TValue, Seq<HandlePoint>> Read, Func<TValue, Seq<HandlePoint>, KeyModifiers, Dragged<TValue>> Moved) where TValue : notnull {
    public Func<TValue, Vector2, Option<Key>, Option<Dragged<TValue>>> Placed { get; init; } = static (_, _, _) => None;
    public Func<TValue, Seq<Key>, Option<TValue>> Removed { get; init; } = static (_, _) => None;
    public bool DraggedOut { get; init; }
    public Func<TValue, Key, float, Option<TValue>> Spread { get; init; } = static (_, _, _) => None;
    public Func<TValue, Key, int, Option<TValue>> Incremented { get; init; } = static (_, _, _) => None;

    public static Points<TValue> Fixed(Seq<Lens<TValue, Vector2>> lenses) {
        HashMap<Key, Lens<TValue, Vector2>> keyed = toHashMap(lenses.Map(static (lens, index) => ((Key)Keyed(index), lens)));
        return new Points<TValue>(
            value => lenses.Map((lens, index) => new HandlePoint(Keyed(index), lens.Get(value))),
            (value, targets, _) => new Dragged<TValue>(targets.Fold(value, (held, target) => keyed.Find(target.Key).Map(lens => lens.Set(target.At, held)).IfNone(held)), targets.Map(static target => target.Key))) {
            Placed = (value, at, nearest) =>
                from key in nearest
                from lens in keyed.Find(key)
                select new Dragged<TValue>(lens.Set(at, value), [key]),
        };
    }

    public static Points<TValue> Listed<TItem>(
        Func<TValue, Seq<TItem>> items, Func<TItem, Vector2> at, Func<TItem, Vector2, Option<TItem>> moved, Func<TValue, Seq<TItem>, Option<TValue>> rebuilt, Func<TValue, Vector2, Option<TItem>> made) =>
        new(value => items(value).Map((item, index) => new HandlePoint(Keyed(index), at(item))),
            (value, targets, _) =>
                items(value).Map((item, index) => targets.Find(target => Ordinal(target.Key) == Some(index)).Map(target => moved(item, target.At)).IfNone(Some(item))).Traverse(static item => item).As()
                    .Bind(shifted => rebuilt(value, shifted))
                    .Match(
                        Some: edited => new Dragged<TValue>(edited, Rekeyed(items(edited).Map(at), targets)),
                        None: () => new Dragged<TValue>(value, targets.Map(static target => target.Key)))) {
            Placed = (value, to, _) =>
                from item in made(value, to)
                from placed in rebuilt(value, items(value).Add(item))
                select new Dragged<TValue>(placed, Rekeyed(items(placed).Map(at), [new HandlePoint(Keyed(items(value).Count), at(item))])),
            Removed = (value, keys) => rebuilt(value, items(value).Map(static (item, index) => (Item: item, Index: index)).Filter(held => !keys.Exists(key => Ordinal(key) == Some(held.Index))).Map(static held => held.Item)),
            DraggedOut = true,
        };

    public static Points<TValue> Indexed(Lens<TValue, Seq<Vector2>> points) =>
        Listed(points.Get, static at => at, static (_, to) => Some(to), (value, held) => Some(points.Set(held, value)), static (_, at) => Some(at));

    public static Points<TValue> Scalar(PlotPlane.Track plane, Lens<TValue, float> value) =>
        Fixed([lens(value, Lens<float, Vector2>.New(along => plane.Along.With(new Vector2(0.5f), along), at => _ => plane.Along.Of(at)))]);

    private static Key.Ordinal Keyed(int index) => new((uint)index);

    private static Option<int> Ordinal(Key key) => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None;

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
        static (press, points) => Inside(points, press.Swept.Contains),
        static (frame, press) => press.Enclosed(frame).Map(static region => (Figure)region));

    public static readonly Marquee Lasso = new(
        static (press, points) => {
            using SKPathBuilder builder = new();
            builder.AddPoly([.. press.Trail.Map(static sample => sample.At)], close: true);
            using SKPath lasso = builder.Detach();
            return Inside(points, point => lasso.Contains(point.X, point.Y)).Strict();
        },
        static (frame, press) => press.Trail.Choose(sample => frame.At(sample.At)) switch {
            [_, _, ..] and var traced => Some<Figure>(new Figure.Path(traced) { Closed = true }),
            _ => None,
        });

    [UseDelegateFromConstructor]
    internal partial Seq<int> Covered(Gesture.Press press, Seq<SKPoint> points);

    [UseDelegateFromConstructor]
    internal partial Option<Figure> Outlined(PlotFrame frame, Gesture.Press press);

    private static Seq<int> Inside(Seq<SKPoint> points, Func<SKPoint, bool> holds) =>
        points.Map(static (point, index) => (Point: point, Index: index)).Filter(held => holds(held.Point)).Map(static held => held.Index);
}

public sealed record Handles<TValue>(TValue Value, Points<TValue> Points, Func<Edit<TValue>, IO<Unit>> Edited) : Part where TValue : notnull {
    // --- [STATE]
    private static readonly Key Background = new Key.Named("background");
    private static readonly Key Layer = new Key.Named("handles");

    public Selection Selection { get; init; } = new Selection.Many([]);
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }
    public Option<Marquee> Marquee { get; init; }
    public Option<CursorShape> BackgroundCursor { get; init; }
    public Func<Key, Vector2, Node> Handle { get; init; } = static (_, at) => new Node.Pin(at, new Node.Box()) { Role = Role.Handle, Align = UnitPoint.Center };
    public Option<TValue> Rest { get; init; }
    public Option<float> Gearing { get; init; }
    public bool Snaps { get; init; }

    // --- [DRAW]
    public Node Drawn(PlotPlane plane, Seq<Node> background) {
        bool modified = Rest.Exists(rest => !rest.Equals(Value));
        Option<PlaneAxis> track = plane.Dimensions is [var only] ? Some(only) : None;
        return new Node.Overlay(
            new Node.Plot(plane, background.Map(node => Modified(node, modified))) { Key = Background, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit },
            [
                new Node.Plot(plane, Points.Read(Value).Map(point => HandleNode(Modified(Handle(point.Key, point.At), modified), point, plane, track))) {
                    Key = Layer,
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                },
            ]) {
            Part = this,
        };
    }

    private static Node HandleNode(Node node, HandlePoint point, PlotPlane plane, Option<PlaneAxis> track) =>
        node with {
            Key = point.Key,
            Role = node.Role | Some(Role.Handle),
            Accessible = node.Accessible.Combine(new Accessible {
                Role = track.IsSome ? AccessRole.Slider : AccessRole.Handle,
                Value = track.Match(
                    Some: axis => (AccessValue)new AccessValue.Ranged(axis.Along.Of(point.At), axis.Axis.Domain.Low, axis.Axis.Domain.High) { Axis = axis.Axis },
                    None: () => new AccessValue.Point(plane, point.At)),
                Orientation = track.Map(static axis => axis.Along),
            }),
        };

    private static Node Modified(Node node, bool modified) => modified ? node.Conditioned(Condition.Modified) : node;

    // --- [GESTURES]
    internal override Option<Reply> Began(Gesture gesture, HitTarget aim) =>
        from frame in Framed(aim.Layout, aim.Bounds)
        from session in gesture.SwitchPartially(
            (Handles: this, Aim: aim, Frame: frame),
            @default: static (_, _) => Option<Session<State>>.None,
            press: static (held, press) => held.Handles.Pressing(press, held.Aim, held.Frame),
            scroll: static (held, _) => held.Handles.Moving(held.Aim, held.Frame),
            rotate: static (held, _) => held.Handles.Moving(held.Aim, held.Frame))
        from reply in session.Began(gesture)
        select reply;

    internal override Option<Reply> Performed(Gesture gesture, HitTarget aim) =>
        Framed(aim.Layout, aim.Bounds).Bind(frame => gesture.SwitchPartially(
            (Handles: this, Aim: aim, Frame: frame),
            @default: static (_, _) => Option<Reply>.None,
            scroll: static (held, scroll) => held.Handles.Moving(held.Aim, held.Frame).Bind(session => session.Stepped(scroll)),
            perform: static (held, perform) => perform.Act.SwitchPartially(
                (held.Handles, held.Aim, held.Frame, perform.Modifiers),
                @default: static (state, act) => act.Toward.Bind(toward =>
                    toward.Granularity.Distance(1f, 0f).Exists(static stride => stride > 0f) && state.Frame.Plane.Dimensions is [var only] && only.Along == toward.Along
                        ? state.Handles.Incrementing(state.Aim, state.Frame, state.Modifiers, toward.Sign * state.Frame.Sign(toward.Along))
                        : state.Handles.Nudged(state.Aim, state.Modifiers, point => Navigated(state.Aim, state.Frame, toward, point))),
                increment: static state => state.Handles.Incrementing(state.Aim, state.Frame, state.Modifiers, 1),
                decrement: static state => state.Handles.Incrementing(state.Aim, state.Frame, state.Modifiers, -1),
                reset: static state => state.Handles.Stepped(start => state.Handles.Rest.Map(rest => start with { Value = rest })),
                delete: static state => state.Handles.Stepped(start => state.Handles.Points.Removed(start.Value, state.Handles.Deleted(state.Aim)).Map(value => start with { Value = value })),
                selectAll: static state => state.Handles.Selected.Bind(_ => state.Handles.Stepped(start => Some(start with { Selection = start.Selection.All(state.Handles.Keys(start.Value)) }))))));

    private Session<State> Session(CursorShape cursor) =>
        new(new State(Value, Selection, None), Seq(Receiver<State>.Of(static held => held.Value, edit => Some(Edited(edit)))) + Selected.Map(static selected => Receiver<State>.Of(static held => held.Selection, edit => Some(selected(edit)))).ToSeq(), cursor) {
            Detent = static held => held.Detent,
            Preview = static held => held.MarqueeOutline,
        };

    private Option<Reply> Stepped(Func<State, Option<State>> next) => Session(CursorShape.Default).Stepped(next);

    private Option<Session<State>> Pressing(Gesture.Press press, HitTarget aim, PlotFrame frame) {
        Option<TValue> restored = Rest.Filter(_ => press.Clicks >= 2);
        Session<State> background = Session(BackgroundCursor.IfNone(DragCursor(frame)));
        return HitKey(aim).Match(
            Some: key => (from rest in restored
                          from point in Points.Read(rest).Find(point => point.Key == key)
                          select Session(DragCursor(frame)) with { Step = (start, gesture) => gesture is Gesture.Press ? Some(Moved(start, [point], press.Modifiers, None)) : None })
                | Some(Session(DragCursor(frame)) with { Step = (start, gesture) => gesture is Gesture.Press moved ? Some(Dragged(Spread(start, key, moved, aim), key, moved, aim, frame)) : None }),
            None: () => restored.Map(rest => background with { Step = (start, gesture) => gesture is Gesture.Press ? Some(start with { Value = rest }) : None })
                | Marquee.Filter(_ => Selected.IsSome).Map(marquee => Session(CursorShape.Crosshair) with {
                    Step = (start, gesture) => gesture is Gesture.Press moved
                        ? Some(start with { Selection = start.Selection.Enclosed(Keys(start.Value), Covered(marquee, frame, moved, Points.Read(start.Value)), moved.Modifiers), MarqueeOutline = marquee.Outlined(frame, moved) })
                        : None,
                })
                | frame.At(press.From).Map(target => background with {
                    Step = (start, gesture) => gesture is Gesture.Press moved
                        ? from placed in Points.Placed(start.Value, target, Nearest(start.Value, target, frame))
                          from key in placed.Keys.Head
                          select Dragged(start with { Value = placed.Value }, key, moved, aim, frame)
                        : None,
                }));
    }

    private static HashSet<Key> Covered(Marquee marquee, PlotFrame frame, Gesture.Press press, Seq<HandlePoint> read) =>
        press.Dragged.IsNone ? [] : toHashSet(marquee.Covered(press, read.Map(point => frame.Point(point.At))).Choose(index => read.At(index)).Map(static point => point.Key));

    private State Spread(State start, Key key, Gesture.Press press, HitTarget aim) =>
        press.Scrolled.Y.CompareTo(0f) is 0 ? start : Points.Spread(start.Value, key, press.Scrolled.Y / Extent(aim, key)).Map(value => start with { Value = value }).IfNone(start);

    private State Dragged(State start, Key key, Gesture.Press press, HitTarget aim, PlotFrame frame) =>
        start.Selection.Pressed(Keys(start.Value), key, press.Modifiers, press is { Ended: true, Dragged.IsNone: true }) switch {
            var selection when press.Dragged.IsNone => start with { Selection = selection },
            var selection => (Points.DraggedOut && Outside(frame, press, Metrics(aim.Layout, key)[Metric.Reach]) ? Points.Removed(start.Value, [key]) : None).Match(
                Some: remaining => new State(remaining, selection, None),
                None: () => Translated(start with { Selection = selection }, key, press, aim, frame)),
        };

    private State Translated(State start, Key key, Gesture.Press press, HitTarget aim, PlotFrame frame) {
        bool fine = press.Modifiers.Primary;
        HashMap<Metric, float> metrics = Metrics(aim.Layout, key);
        Seq<HandlePoint> read = Points.Read(start.Value);
        HashSet<Key> moving = toHashSet(read.Map(static point => point.Key).Filter(held => start.Selection.Holds(key) ? start.Selection.Holds(held) : held == key));
        Vector2 travel = Gearing.Match(Some: gearing => Geared(frame, press, gearing), None: () => frame.Delta(press.From, press.At).IfNone(Vector2.Zero)) * (fine ? metrics[Metric.Fine] : 1f);
        Seq<HandlePoint> targets = read.Filter(point => moving.Contains(point.Key)).Map(point => point with { At = Snapped(frame, frame.Plane.Constrained(point.At + travel), press.Modifiers.Shift != Snaps, aim.Layout) });
        Seq<Vector2> detents = Rest.Map(rest => Points.Read(rest).Map(static point => point.At)).IfNone([]) + read.Filter(point => !moving.Contains(point.Key)).Map(static point => point.At);
        Option<Vector2> detent = targets is [var only] && !fine ? frame.Nearest(only.At, detents, metrics[Metric.Reach]).Bind(index => detents.At(index)) : None;
        return Moved(start, detent.Match(Some: to => targets.Map(target => target with { At = to }), None: () => targets), press.Modifiers, detent);
    }

    private static bool Outside(PlotFrame frame, Gesture.Press press, float reach) => !SKRect.Inflate(frame.Box, reach, reach).Contains(press.At);

    private static Vector2 Geared(PlotFrame frame, Gesture.Press press, float gearing) =>
        frame.Plane.Dimensions.Fold(Vector2.Zero, (travel, free) => free.Along.With(travel, free.Along.Of((Vector2)(press.At - press.From)) * gearing * frame.Sign(free.Along)));

    private State Moved(State start, Seq<HandlePoint> targets, KeyModifiers modifiers, Option<Vector2> detent) {
        Dragged<TValue> moved = Points.Moved(start.Value, targets, modifiers);
        return new State(moved.Value, start.Selection.Renamed(toHashMap(targets.Map(static target => target.Key).Zip(moved.Keys))), detent);
    }

    private Option<Session<State>> Moving(HitTarget aim, PlotFrame frame) =>
        Aimed(aim).Filter(_ => !aim.Scene.Owners(aim.Layout.Path.Init).Exists(static owner => owner.Part.Navigates)).Map(point => Session(DragCursor(frame)) with {
            Step = (start, gesture) => gesture.SwitchPartially(
                (Handles: this, Start: start, Point: point, Frame: frame, Notch: Extent(aim, point.Key)),
                @default: static (_, _) => Option<State>.None,
                scroll: static (held, scroll) => Some(held.Handles.Notched(held.Start, held.Point, scroll, held.Frame, held.Notch)),
                rotate: static (held, rotate) => held.Frame.Plane.Rotated(held.Point.At, rotate.Rotation).Map(to => held.Handles.Moved(held.Start, [held.Point with { At = to }], rotate.Modifiers, None))),
        });

    private State Notched(State start, HandlePoint point, Gesture.Scroll scroll, PlotFrame frame, float notch) =>
        Notches(scroll.Delta, notch, frame.Plane.Dimensions.Count > 1) switch {
            var steps when steps == Vector2.Zero => start,
            var steps => Moved(start, [point with { At = Stepped(frame, point.At, steps, notch) }], scroll.Modifiers, None),
        };

    private static Vector2 Notches(Vector2 travel, float notch, bool planar) =>
        new Vector2(MathF.Truncate(travel.X / notch), MathF.Truncate(travel.Y / notch)) switch {
            var notches => planar ? notches : MathF.Abs(travel.X) > MathF.Abs(travel.Y) ? notches with { Y = 0f } : notches with { X = 0f },
        };

    private static Vector2 Stepped(PlotFrame frame, Vector2 point, Vector2 steps, float notch) =>
        frame.Plane.Dimensions.Fold(point, (moved, free) => (frame.Plane.Dimensions.Count > 1 ? free.Along.Of(steps) : steps.X + steps.Y) switch {
            var count when count.CompareTo(0f) is 0 => moved,
            var count => frame.Stepped(moved, free.Along, (int)count, notch),
        });

    private Option<Reply> Nudged(HitTarget aim, KeyModifiers modifiers, Func<HandlePoint, Vector2> move) =>
        Aimed(aim).Bind(point => Stepped(start => Some(Moved(start, [point with { At = move(point) }], modifiers, None))));

    private Option<Reply> Incrementing(HitTarget aim, PlotFrame frame, KeyModifiers modifiers, int sign) =>
        Aimed(aim).Bind(point => Stepped(start =>
            Points.Incremented(start.Value, point.Key, sign).Map(value => start with { Value = value })
            | Some(Moved(start, [point with { At = Incremented(aim, frame, point, sign) }], modifiers, None))));

    private static Vector2 Incremented(HitTarget aim, PlotFrame frame, HandlePoint point, int sign) =>
        frame.Plane.Dimensions.Head.Map(first => frame.Stepped(point.At, first.Along, sign, Extent(aim, point.Key))).IfNone(point.At);

    private static Vector2 Navigated(HitTarget aim, PlotFrame frame, Bearing toward, HandlePoint point) =>
        toward.Granularity.Distance(Extent(aim, point.Key), Spacing(aim.Layout)).Match(
            Some: stride => frame.Stepped(point.At, toward.Along, toward.Sign * frame.Sign(toward.Along), stride),
            None: () => frame.Ended(point.At, toward.Along, toward.Sign));

    private static Vector2 Snapped(PlotFrame frame, Vector2 value, bool snaps, LayoutNode layout) =>
        snaps ? frame.Plane.Dimensions.Fold(value, (held, free) => frame.Stepped(held, free.Along, 0, Spacing(layout))) : value;

    // --- [READ]
    internal override Option<Seq<Key>> Revealed(LayoutNode layout) => Selection.Lead.Map(lead => layout.Path.Add(Layer).Add(lead));

    internal override bool Offers(LayoutNode owner, Seq<Key> path) => path == owner.Path || path.Init == owner.Path.Add(Layer);

    private static HashMap<Metric, float> Metrics(LayoutNode layout, Key key) =>
        (from plot in Child(layout, Layer)
         from drawn in Child(plot, key)
         select drawn.Style.Metrics)
        .IfNone(() => Styles.Resolve(layout.Cascade, Role.Handle, layout.Style.Conditions).Metrics);

    private static Option<PlotFrame> Framed(LayoutNode layout, SKRect bounds) =>
        from plot in Child(layout, Layer)
        from frame in Layouts.Plotted(plot)
        select frame.Translated(new SKPoint(bounds.Left + plot.Frame.Left, bounds.Top + plot.Frame.Top));

    private static Option<Key> HitKey(HitTarget aim) =>
        aim.Layout.Path.Add(Layer) switch { var layer => aim.Hit.Take(layer.Count) == layer ? aim.Hit.At(layer.Count) : None };

    private static float Spacing(LayoutNode layout) => Styles.Resolve(layout.Cascade, Role.Grid, layout.Node.Conditions).Metrics[Metric.Spacing];

    private static CursorShape DragCursor(PlotFrame frame) => frame.Plane is PlotPlane.Track track ? CursorShape.Resizing(track.Along) : CursorShape.Move;

    private static Option<LayoutNode> Child(LayoutNode layout, Key key) => layout.Children.Find(child => child.Path.Last == Some(key));

    private Seq<Key> Keys(TValue value) => Points.Read(value).Map(static point => point.Key);

    private Option<HandlePoint> Aimed(HitTarget aim) =>
        (HitKey(aim) | Selection.Lead | (Points.Read(Value) is [var only] ? Some(only.Key) : None)).Bind(key => Points.Read(Value).Find(point => point.Key == key));

    private Seq<Key> Deleted(HitTarget aim) =>
        Keys(Value).Filter(Selection.Holds) switch {
            { IsEmpty: true } => Aimed(aim).Map(static point => point.Key).ToSeq(),
            var selection => selection,
        };

    private Option<Key> Nearest(TValue value, Vector2 target, PlotFrame frame) {
        Seq<HandlePoint> read = Points.Read(value);
        return frame.Nearest(target, read.Map(static point => point.At), float.PositiveInfinity).Bind(index => read.At(index)).Map(static point => point.Key);
    }

    private static float Extent(HitTarget aim, Key key) => Metrics(aim.Layout, key)[Metric.Extent];

    private sealed record State(TValue Value, Selection Selection, Option<Vector2> Detent) {
        internal Option<Figure> MarqueeOutline { get; init; }
    }
}
