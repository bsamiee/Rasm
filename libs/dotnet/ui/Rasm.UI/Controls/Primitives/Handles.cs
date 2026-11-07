using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Controls.Primitives;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record HandlePoint(Key Key, Vector2 At);

public sealed record Points<TValue>(Func<TValue, Seq<HandlePoint>> Read, Func<TValue, Seq<HandlePoint>, KeyModifiers, (TValue Value, HashMap<Key, Option<Key>> Renames)> Moved) where TValue : notnull {
    public Func<TValue, Vector2, Option<Key>, Option<(TValue Value, Key Key)>> Placed { get; init; } = static (_, _, _) => None;
    public Func<TValue, Seq<Key>, Option<(TValue Value, HashMap<Key, Option<Key>> Renames)>> Removed { get; init; } = static (_, _) => None;
    public bool DraggedOut { get; init; }
    public Func<TValue, Key, float, Option<TValue>> Spread { get; init; } = static (_, _, _) => None;
    public Func<TValue, Key, int, Option<TValue>> Incremented { get; init; } = static (_, _, _) => None;

    public static Points<TValue> Fixed(Seq<Lens<TValue, Vector2>> lenses) {
        HashMap<Key, Lens<TValue, Vector2>> keyed = toHashMap(lenses.Map(static (lens, index) => ((Key)Keyed(index), lens)));
        return new Points<TValue>(
            value => lenses.Map((lens, index) => new HandlePoint(Keyed(index), lens.Get(value))),
            (value, targets, _) => (targets.Fold(value, (held, target) => keyed[target.Key].Set(target.At, held)), [])) {
            Placed = (value, at, nearest) => nearest.Map(key => (keyed[key].Set(at, value), key)),
        };
    }

    public static Points<TValue> Listed<TItem>(
        Func<TValue, Seq<TItem>> items, Func<TItem, Vector2> at, Func<TItem, Vector2, Option<TItem>> moved,
        Func<TValue, Seq<(Key Key, TItem Item)>, Option<(TValue Value, Seq<Key> Order)>> rebuilt, Func<TValue, Vector2, Option<TItem>> made,
        Option<Comparison<TItem>> order = default) {
        return new(value => KeyedItems(value).Map(held => new HandlePoint(held.Key, at(held.Item))),
            (value, targets, _) => toHashMap(targets.Map(static target => (target.Key, target.At))) switch {
                var positions => KeyedItems(value).Traverse(held => positions.Find(held.Key).Map(target => moved(held.Item, target)).IfNone(Some(held.Item)).Map(item => (held.Key, Item: item))).As()
                    .Bind(shifted => Rebuilt(value, shifted)).IfNone((value, [])),
            }) {
            Placed = (value, to, _) =>
                from item in made(value, to)
                let held = KeyedItems(value)
                let key = Keyed(held.Count)
                from placed in Rebuilt(value, held.Add((key, item)))
                from leading in placed.Renames[key]
                select (placed.Value, leading),
            Removed = (value, keys) => toHashSet(keys) switch { var removed => Rebuilt(value, KeyedItems(value).Filter(held => !removed.Contains(held.Key))) },
            DraggedOut = true,
        };

        Seq<(Key Key, TItem Item)> KeyedItems(TValue value) => items(value).Map(static (item, index) => ((Key)Keyed(index), item));

        Option<(TValue Value, HashMap<Key, Option<Key>> Renames)> Rebuilt(TValue value, Seq<(Key Key, TItem Item)> changed) {
            Seq<(Key Key, TItem Item)> ordered = order.Map(compare => toSeq(changed.OrderBy(static held => held.Item, System.Collections.Generic.Comparer<TItem>.Create(compare)))).IfNone(changed).Strict();
            return rebuilt(value, ordered).Map(edited => toHashMap(edited.Order.Map(static (key, index) => (key, (Key)Keyed(index)))) switch {
                var renamed => (edited.Value, toHashMap(KeyedItems(value).Map(static held => held.Key).Concat(changed.Map(static held => held.Key)).Distinct().Map(key => (key, renamed.Find(key))))),
            });
        }
    }

    public static Points<TValue> Indexed(Lens<TValue, Seq<Vector2>> points) =>
        Listed(points.Get, static at => at, static (_, to) => Some(to), (value, held) => Some((points.Set(held.Map(static item => item.Item), value), held.Map(static item => item.Key))), static (_, at) => Some(at));

    public static Points<TValue> Scalar(PlotPlane.Track plane, Lens<TValue, float> value) =>
        Fixed([lens(value, Lens<float, Vector2>.New(along => plane.Along.With(new Vector2(0.5f), along), at => _ => plane.Along.Of(at)))]);

    private static Key.Ordinal Keyed(int index) => new((uint)index);
}

[SmartEnum]
public sealed partial class Marquee {
    public static readonly Marquee Box = new(
        static (frame, press) => press.Enclosed(frame).Map(static region => (Figure)region),
        static (press, frames) => Inside(frames, frame => frame.IntersectsWith(press.Swept)));

    public static readonly Marquee Lasso = new(
        static (frame, press) => press.Trail.Choose(sample => frame.At(sample.At)) switch {
            [_, _, ..] and var traced => Some<Figure>(new Figure.Path(traced) { Closed = true }),
            _ => None,
        },
        static (press, frames) => {
            using SKPathBuilder builder = new();
            builder.AddPoly([.. press.Trail.Map(static sample => sample.At)], close: true);
            using SKPath lasso = builder.Detach();
            return Inside(frames, frame => lasso.Contains(frame.MidX, frame.MidY)).Strict();
        });

    [UseDelegateFromConstructor]
    internal partial Option<Figure> Outlined(PlotFrame frame, Gesture.Press press);

    internal Seq<T> Covered<T>(Gesture.Press press, Seq<T> items, Func<T, SKRect> frame) => press.Dragged.IsNone ? [] : Covering(press, items.Map(frame)).Choose(index => items.At(index));

    [UseDelegateFromConstructor]
    private partial Seq<int> Covering(Gesture.Press press, Seq<SKRect> frames);

    private static Seq<int> Inside(Seq<SKRect> frames, Func<SKRect, bool> holds) =>
        frames.Map(static (frame, index) => (Frame: frame, Index: index)).Filter(held => holds(held.Frame)).Map(static held => held.Index);
}

public sealed record Handles<TValue>(TValue Value, Points<TValue> Points, Func<Edit<TValue>, IO<Unit>> Edited) : Part where TValue : notnull {

    // --- [STATE]

    private static readonly Key Background = new Key.Named("background");
    private static readonly Key Layer = new Key.Named("handles");

    public Selection Selection { get; init; } = Selection.Empty;
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }
    public Option<Marquee> Marquee { get; init; }
    public Option<CursorShape> BackgroundCursor { get; init; }
    public Func<Key, Vector2, View> Handle { get; init; } = static (_, at) => new View.Pin(at, new View.Box()) { Role = Roles.Handle, Align = UnitPoint.Center };
    public Option<TValue> Rest { get; init; }
    public Option<float> Gearing { get; init; }
    public bool Snaps { get; init; }

    // --- [DRAWING]

    public View Drawn(PlotPlane plane, Seq<View> background) {
        bool modified = Edit.Restoring(Rest, Some(Value)).IsSome;
        Option<PlaneAxis> track = plane.Dimensions is [var only] ? Some(only) : None;
        return new View.Overlay(
            new View.Plot(plane, background.Map(node => Modified(node, modified))) { Key = Background }.Grown,
            [new View.Plot(plane, Points.Read(Value).Map(point => HandleNode(Modified(Handle(point.Key, point.At), modified), point, plane, track))) { Key = Layer }.Grown]) {
            Part = this,
        };
    }

    private static View HandleNode(View node, HandlePoint point, PlotPlane plane, Option<PlaneAxis> track) =>
        node with {
            Key = point.Key,
            Role = node.Role | Some(Roles.Handle),
            Accessible = node.Accessible.Combine(new Accessible {
                Role = track.IsSome ? AccessRole.Slider : AccessRole.Handle,
                Value = track.Match(
                    Some: axis => (AccessValue)new AccessValue.Ranged(axis.Axis, axis.Along.Of(point.At)),
                    None: () => new AccessValue.Point(plane, point.At)),
                Orientation = track.Map(static axis => axis.Along),
            }),
        };

    private static View Modified(View node, bool modified) => modified ? node.Conditioned(Condition.Modified) : node;

    // --- [GESTURES]

    internal override Option<Reply> Began(Gesture gesture, HitTarget aim) =>
        from frame in Framed(aim.Owner.Layout, aim.Owner.Bounds)
        from session in gesture.SwitchPartially(
            (Handles: this, Aim: aim, Frame: frame),
            @default: static (_, _) => Option<Session<State>>.None,
            press: static (held, press) => held.Handles.Pressing(press, held.Aim, held.Frame),
            scroll: static (held, _) => held.Handles.Moving(held.Aim, held.Frame),
            rotate: static (held, _) => held.Handles.Moving(held.Aim, held.Frame))
        from reply in session.Began(gesture)
        select reply;

    internal override Option<Reply> Performed(Gesture gesture, HitTarget aim) =>
        Framed(aim.Owner.Layout, aim.Owner.Bounds).Bind(frame => gesture.SwitchPartially(
            (Handles: this, Aim: aim, Frame: frame),
            @default: static (_, _) => Option<Reply>.None,
            scroll: static (held, scroll) => held.Handles.Moving(held.Aim, held.Frame).Bind(session => session.Stepped(scroll)),
            perform: static (held, perform) => perform.Act.SwitchPartially(
                (held.Handles, held.Aim, held.Frame, perform.Modifiers),
                @default: static (state, act) => act.Toward.Bind(toward =>
                    toward.Granularity == Granularity.Step && state.Frame.Plane.Dimensions is [var only] && only.Along == toward.Along
                        ? state.Handles.Incrementing(state.Aim, state.Frame, state.Modifiers, toward.Sign * state.Frame.Sign(toward.Along))
                        : state.Handles.Nudged(state.Aim, state.Modifiers, point => Navigated(state.Aim, state.Frame, toward, point))),
                increment: static state => state.Handles.Incrementing(state.Aim, state.Frame, state.Modifiers, 1),
                decrement: static state => state.Handles.Incrementing(state.Aim, state.Frame, state.Modifiers, -1),
                reset: static state => state.Handles.Stepped(start => state.Handles.Rest.Map(rest => start with { Value = rest })),
                delete: static state => state.Handles.Stepped(start => state.Handles.Points.Removed(start.Value, state.Handles.Deleted(state.Aim)).Map(start.Rebuilt)),
                selectAll: static state => state.Handles.Selected.Bind(_ => state.Handles.Stepped(start => Some(start with { Selection = start.Selection.All(state.Handles.Keys(start.Value)) }))))));

    private Session<State> Session(CursorShape cursor) =>
        new(new State(Value, Selection), Seq(Receiver<State>.Of(static held => held.Value, edit => Some(Edited(edit)))) + Selected.Map(static selected => Receiver<State>.Of(static held => held.Selection, edit => Some(selected(edit)))).ToSeq(), cursor) {
            Detent = static held => held.Detent,
            Preview = static held => held.Outline,
        };

    private Option<Reply> Stepped(Func<State, Option<State>> next) => Session(CursorShape.Default).Stepped(next);

    private Option<Session<State>> Pressing(Gesture.Press press, HitTarget aim, PlotFrame frame) {
        Option<TValue> restored = Rest.Filter(_ => press.Clicks >= 2);
        Session<State> background = Session(BackgroundCursor.IfNone(DragCursor(frame)));
        return HitKey(aim).Match(
            Some: key => (from rest in restored
                          from point in Points.Read(rest).Find(point => point.Key == key)
                          select Session(DragCursor(frame)) with { Step = (start, gesture) => gesture is Gesture.Press ? Some(Moved(start, [point], press.Modifiers)) : None })
                || Some(Session(DragCursor(frame)) with { Step = (start, gesture) => gesture is Gesture.Press moved ? Some(Dragged(Spread(start, key, moved, aim), key, moved, aim, frame)) : None }),
            None: () => restored.Map(rest => background with { Step = (start, gesture) => gesture is Gesture.Press ? Some(start with { Value = rest }) : None })
                || Marquee.Filter(_ => Selected.IsSome).Map(marquee => Session(CursorShape.Crosshair) with {
                    Step = (start, gesture) => gesture is Gesture.Press moved
                        ? Some(start with { Selection = start.Selection.Enclosed(Keys(start.Value), Covered(marquee, frame, moved, Points.Read(start.Value)), moved.Modifiers), Outline = marquee.Outlined(frame, moved) })
                        : None,
                })
                || frame.At(press.From).Map(target => background with {
                    Step = (start, gesture) => gesture is Gesture.Press moved
                        ? from placed in Points.Placed(start.Value, target, Nearest(start.Value, target, frame))
                          select Dragged(start with { Value = placed.Value }, placed.Key, moved, aim, frame)
                        : None,
                }));
    }

    private static HashSet<Key> Covered(Marquee marquee, PlotFrame frame, Gesture.Press press, Seq<HandlePoint> read) =>
        toHashSet(marquee.Covered(press, read.Zip(frame.Points(read.Map(static point => point.At))), static held => SKRect.Create(held.Second, SKSize.Empty)).Map(static held => held.First.Key));

    private State Spread(State start, Key key, Gesture.Press press, HitTarget aim) =>
        press.Scrolled.Y.CompareTo(0f) is 0 ? start : Points.Spread(start.Value, key, press.Scrolled.Y / Extent(aim, key)).Map(value => start with { Value = value }).IfNone(start);

    private State Dragged(State start, Key key, Gesture.Press press, HitTarget aim, PlotFrame frame) =>
        start.Selection.Pressed(Keys(start.Value), key, press.Modifiers, press.Clicked) switch {
            var selection when press.Dragged.IsNone => start with { Selection = selection },
            var selection => (Points.DraggedOut && Outside(frame, press, Metrics(aim.Owner.Layout, key)[Metric.Reach]) ? Points.Removed(start.Value, [key]) : None).Match(
                Some: removed => (start with { Selection = selection }).Rebuilt(removed),
                None: () => Translated(start with { Selection = selection }, key, press, aim, frame)),
        };

    private State Translated(State start, Key key, Gesture.Press press, HitTarget aim, PlotFrame frame) {
        bool fine = press.Modifiers.Primary;
        HashMap<Metric, float> metrics = Metrics(aim.Owner.Layout, key);
        float spacing = aim.Owner.Layout.Styled(Roles.Grid).Metrics[Metric.Spacing];
        Seq<HandlePoint> read = Points.Read(start.Value);
        HashSet<Key> moving = toHashSet(read.Map(static point => point.Key).Filter(held => start.Selection.Holds(key) ? start.Selection.Holds(held) : held == key));
        Vector2 travel = Gearing.Match(Some: gearing => Geared(frame, press, gearing), None: () => frame.Delta(press.From, press.At).IfNone(Vector2.Zero)) * (fine ? metrics[Metric.Fine] : 1f);
        Seq<HandlePoint> targets = read.Filter(point => moving.Contains(point.Key)).Map(point => point with { At = Snapped(frame, frame.Plane.Constrained(point.At + travel), press.Modifiers.Shift != Snaps, spacing) });
        Seq<Vector2> detents = Rest.Map(rest => Points.Read(rest).Map(static point => point.At)).IfNone([]) + read.Filter(point => !moving.Contains(point.Key)).Map(static point => point.At);
        Option<Vector2> detent = targets is [var only] && !fine ? frame.Nearest(only.At, detents, metrics[Metric.Reach]).Bind(index => detents.At(index)) : None;
        return Moved(start, detent.Match(Some: to => targets.Map(target => target with { At = to }), None: () => targets), press.Modifiers) with { Detent = detent };
    }

    private static bool Outside(PlotFrame frame, Gesture.Press press, float reach) => !SKRect.Inflate(frame.Box, reach, reach).Contains(press.At);

    private static Vector2 Geared(PlotFrame frame, Gesture.Press press, float gearing) =>
        frame.Plane.Dimensions.Fold(Vector2.Zero, (travel, free) => free.Along.With(travel, free.Along.Of((Vector2)(press.At - press.From)) * gearing * frame.Sign(free.Along)));

    private State Moved(State start, Seq<HandlePoint> targets, KeyModifiers modifiers) => start.Rebuilt(Points.Moved(start.Value, targets, modifiers));

    private Option<Session<State>> Moving(HitTarget aim, PlotFrame frame) =>
        Aimed(aim).Filter(_ => !aim.Scene.Owners(aim.Owner.Layout.Path.Init).Exists(static owner => owner.Part is Viewport)).Map(point => Session(DragCursor(frame)) with {
            Step = (start, gesture) => gesture.SwitchPartially(
                (Handles: this, Start: start, Point: point, Frame: frame, Notch: Extent(aim, point.Key)),
                @default: static (_, _) => Option<State>.None,
                scroll: static (held, scroll) => Some(held.Handles.Notched(held.Start, held.Point, scroll, held.Frame, held.Notch)),
                rotate: static (held, rotate) => held.Frame.Plane.Rotated(held.Point.At, rotate.Rotation).Map(to => held.Handles.Moved(held.Start, [held.Point with { At = to }], rotate.Modifiers))),
        });

    private State Notched(State start, HandlePoint point, Gesture.Scroll scroll, PlotFrame frame, float notch) =>
        Notches(scroll.Delta, notch, frame.Plane.Dimensions.Count > 1) switch {
            var steps when steps == Vector2.Zero => start,
            var steps => Moved(start, [point with { At = Stepped(frame, point.At, steps, notch) }], scroll.Modifiers),
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
        Aimed(aim).Bind(point => Stepped(start => Some(Moved(start, [point with { At = move(point) }], modifiers))));

    private Option<Reply> Incrementing(HitTarget aim, PlotFrame frame, KeyModifiers modifiers, int sign) =>
        Aimed(aim).Bind(point => Stepped(start =>
            Points.Incremented(start.Value, point.Key, sign).Map(value => start with { Value = value })
            || Some(Moved(start, [point with { At = Incremented(aim, frame, point, sign) }], modifiers))));

    private static Vector2 Incremented(HitTarget aim, PlotFrame frame, HandlePoint point, int sign) =>
        frame.Plane.Dimensions.Head.Map(first => frame.Stepped(point.At, first.Along, sign, Extent(aim, point.Key))).IfNone(point.At);

    private static Vector2 Navigated(HitTarget aim, PlotFrame frame, Bearing toward, HandlePoint point) =>
        toward.Granularity.Distance(Extent(aim, point.Key), aim.Owner.Layout.Styled(Roles.Grid).Metrics[Metric.Spacing]).Match(
            Some: stride => frame.Stepped(point.At, toward.Along, toward.Sign * frame.Sign(toward.Along), stride),
            None: () => frame.Ended(point.At, toward.Along, toward.Sign));

    private static Vector2 Snapped(PlotFrame frame, Vector2 value, bool snaps, float spacing) =>
        snaps ? frame.Plane.Dimensions.Fold(value, (held, free) => frame.Stepped(held, free.Along, 0, spacing)) : value;

    // --- [READS]

    internal override Option<Seq<Key>> Revealed(LayoutNode layout) => Selection.Lead.Map(lead => layout.Path.Add(Layer).Add(lead));

    internal override bool Offers(LayoutNode owner, Seq<Key> path) => path == owner.Path || path.Init == owner.Path.Add(Layer);

    private static HashMap<Metric, float> Metrics(LayoutNode layout, Key key) =>
        (from plot in Child(layout, Layer)
         from drawn in Child(plot, key)
         select drawn.Style.Metrics)
        .IfNone(() => layout.Styled(Roles.Handle).Metrics);

    private static Option<PlotFrame> Framed(LayoutNode layout, SKRect bounds) =>
        from plot in Child(layout, Layer)
        from frame in plot.Plotted(bounds.Location + plot.Frame.Location)
        select frame;

    private static Option<Key> HitKey(HitTarget aim) =>
        aim.Owner.Layout.Path.Add(Layer) switch { var layer => aim.Hit.StartsWith(layer) ? aim.Hit.At(layer.Count) : None };

    private static CursorShape DragCursor(PlotFrame frame) => frame.Plane is PlotPlane.Track track ? CursorShape.Resizing(track.Along) : CursorShape.Move;

    private static Option<LayoutNode> Child(LayoutNode layout, Key key) => layout.Children.Find(child => child.Path.Last == Some(key));

    private Seq<Key> Keys(TValue value) => Points.Read(value).Map(static point => point.Key);

    private Option<HandlePoint> Aimed(HitTarget aim) {
        Seq<HandlePoint> points = Points.Read(Value);
        return (HitKey(aim) || Selection.Lead || (points is [var only] ? Some(only.Key) : None)).Bind(key => points.Find(point => point.Key == key));
    }

    private Seq<Key> Deleted(HitTarget aim) =>
        Keys(Value).Filter(Selection.Holds) switch {
            { IsEmpty: true } => Aimed(aim).Map(static point => point.Key).ToSeq(),
            var selection => selection,
        };

    private Option<Key> Nearest(TValue value, Vector2 target, PlotFrame frame) {
        Seq<HandlePoint> read = Points.Read(value);
        return frame.Nearest(target, read.Map(static point => point.At), float.PositiveInfinity).Bind(index => read.At(index)).Map(static point => point.Key);
    }

    private static float Extent(HitTarget aim, Key key) => Metrics(aim.Owner.Layout, key)[Metric.Extent];

    private sealed record State(TValue Value, Selection Selection) {
        internal Option<Vector2> Detent { get; init; }
        internal Option<Figure> Outline { get; init; }

        internal State Rebuilt((TValue Value, HashMap<Key, Option<Key>> Renames) changed) => new(changed.Value, Selection.Renamed(changed.Renames));
    }
}
