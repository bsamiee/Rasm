using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class Magnification {
    public static Magnification Free { get; } = Create(float.Epsilon, float.PositiveInfinity);

    public float Least { get; }
    public float Most { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float least, ref float most) =>
        validationError = float.IsFinite(least) && least > 0f && most >= least
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"magnification ({least}, {most}) is not a positive ordered span"));
}

public sealed record Points<TValue>(
    Func<TValue, Seq<(Key Key, Vector2 At)>> Read,
    Func<TValue, Seq<(Key Key, Vector2 To)>, LanguageExt.HashSet<Modifier>, (TValue Value, Seq<Key> Keys)> Moved,
    Func<Vector2, string> Spoken) where TValue : notnull {
    private static readonly Key Only = new Key.Ordinal(0);

    public Option<Func<TValue, Vector2, Option<Key>, Option<(TValue Value, Key Key)>>> Placed { get; init; }
    public Option<Func<TValue, Seq<Key>, Option<TValue>>> Removed { get; init; }
    public Option<Func<TValue, Key, Option<TValue>>> Torn { get; init; }
    public Option<Func<TValue, Key, float, TValue>> Spread { get; init; }

    public static Points<TValue> Fixed(Seq<Lens<TValue, Vector2>> lenses, Func<Vector2, string> spoken) =>
        lenses.Map(static (lens, index) => (Key: (Key)new Key.Ordinal((uint)index), Lens: lens)) switch {
            var rows => toHashMap(rows) switch {
                var keyed => new Points<TValue>(
                    value => rows.Map(row => (row.Key, row.Lens.Get(value))),
                    (value, targets, _) => (targets.Fold(value, (held, target) => keyed.Find(target.Key).Map(lens => lens.Set(target.To, held)).IfNone(held)), targets.Map(static target => target.Key)),
                    spoken) {
                    Placed = Some<Func<TValue, Vector2, Option<Key>, Option<(TValue Value, Key Key)>>>((value, at, nearest) =>
                        from key in nearest
                        from lens in keyed.Find(key)
                        select (lens.Set(at, value), key)),
                },
            },
        };

    public static Points<TValue> Scalar(PlotPlane.Track plane, Lens<TValue, float> value, Func<float, string> spoken) =>
        new(held => [(Only, plane.Along.With(new Vector2(0.5f), value.Get(held)))],
            (held, targets, _) => (targets.Fold(held, (state, target) => value.Set(plane.Along.Of(target.To), state)), targets.Map(static target => target.Key)),
            point => spoken(plane.Along.Of(point))) {
            Placed = Some<Func<TValue, Vector2, Option<Key>, Option<(TValue Value, Key Key)>>>((held, at, _) => Some((value.Set(plane.Along.Of(at), held), Only))),
        };
}

public abstract record Handles {
    private protected static readonly Key Ground = new Key.Named("ground");
    private protected static readonly Key Layer = new Key.Named("handles");
    private protected static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));

    private protected Handles() { }

    public Selection Picked { get; init; } = new Selection.Many([], None, None);
    public Option<Func<Edit<Selection>, IO<Unit>>> Pick { get; init; }
    public Option<Func<Option<Vector2>, IO<Unit>>> Hover { get; init; }
    public Option<Cursor> GroundCursor { get; init; }

    public abstract Node Drawn(PlotPlane plane, Seq<Node> ground, Option<Func<Key, Vector2, Node>> handle = default);

    internal abstract Reply Began(Gesture gesture, Aim aim);

    internal abstract Reply Performed(Gesture gesture, Aim aim);

    internal Option<Seq<Key>> Revealed(Laid laid) => Picked.Lead.Map(lead => laid.Path.Add(Layer).Add(lead));

    internal static bool Offers(Laid laid, Seq<Key> path) => path == laid.Path || path.Init == laid.Path.Add(Layer);

    internal Seq<IO<Unit>> Watched(Option<Sight> before, Sight after) =>
        Hover.Map(hover => Hovered(after) switch {
            var now => before.Bind(Hovered) == now ? Seq<IO<Unit>>() : [hover(now)],
        }).IfNone([]);

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

    private protected static Option<Key> Point(Aim aim) =>
        aim.Laid.Path.Add(Layer) switch {
            var layer => aim.Over.Take(layer.Count) == layer ? aim.Over.At(layer.Count) : None,
        };

    private protected static float Spacing(Laid laid) => Styles.Resolve(laid.Cascade, Role.Grid, laid.Node.Conditions).Metrics[Metric.Spacing];

    private protected static Cursor Gripped(PlotFrame frame) =>
        frame.Plane.Switch(cartesian: static _ => Cursor.Move, track: static track => Cursor.Resizing(track.Along), polar: static _ => Cursor.Move);

    private static Option<Laid> Child(Laid laid, Key key) => laid.Children.Find(child => child.Path.Last == Some(key));

    private static Option<Vector2> Hovered(Sight sight) =>
        from pointer in sight.Pointer
        where sight.Hover.Exists(path => path.Take(sight.Laid.Path.Count) == sight.Laid.Path)
        from bounds in sight.Scene.Frame(sight.Laid.Path)
        from frame in Framed(sight.Laid, bounds)
        from at in frame.At(pointer)
        select at;
}

public sealed record Handles<TValue>(TValue Value, Points<TValue> Points, Func<Edit<TValue>, IO<Unit>> Edited) : Handles where TValue : notnull {
    // --- [STATE]
    private const float Fine = 0.1f;
    private static readonly SKRect Square = new(0f, 0f, 1f, 1f);

    public Option<TValue> Rest { get; init; }

    private Held Start => new(Value, Picked, None);

    private Seq<Sink<Held>> Sinks => Seq(Sink<Held>.Of(static held => held.Value, Edited)) + Pick.Map(static pick => Sink<Held>.Of(static held => held.Picked, pick)).ToSeq();

    // --- [DRAW]
    public override Node Drawn(PlotPlane plane, Seq<Node> ground, Option<Func<Key, Vector2, Node>> handle = default) =>
        (Modified: Rest.Exists(rest => !rest.Equals(Value)), Track: plane.Dimensions is [var only] ? Some(only) : None) switch {
            var held => new Node.Overlay(
                new Node.Plot(plane, ground.Map(node => Modified(node, held.Modified))) { Key = Ground, Width = Grown, Height = Grown },
                [
                    new Node.Plot(plane, Points.Read(Value).Map(point => Stamped(
                        handle.Map(draw => draw(point.Key, point.At)).IfNone(() => new Node.Pin(point.At, new Node.Box()) { Role = Role.Handle, Align = UnitPoint.Center }),
                        point, held.Modified, held.Track))) { Key = Layer, Width = Grown, Height = Grown },
                ]) {
                Part = new Part.Pad(this),
            },
        };

    private Node Stamped(Node node, (Key Key, Vector2 At) point, bool modified, Option<(Orientation Along, Axis Axis)> track) =>
        Modified(node, modified) switch {
            var held => held with {
                Key = point.Key,
                Role = held.Role | Some(Role.Handle),
                Accessible = held.Accessible.Combine(new Accessible {
                    Role = track.IsSome ? AccessRole.Slider : AccessRole.Handle,
                    Value = track.Match(
                        Some: axis => (AccessValue)new AccessValue.Ranged(axis.Along.Of(point.At), axis.Axis.Range.Low, axis.Axis.Range.High) { Description = Points.Spoken(point.At) },
                        None: () => new AccessValue.Text(Points.Spoken(point.At))),
                    Orientation = track.Map(static axis => axis.Along),
                }),
            },
        };

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
                    increment: static (state, _) => state.Handles.Nudged(state.Aim, state.Perform, (point, key) => Bumped(state.Aim, state.Frame, point, key, 1)),
                    decrement: static (state, _) => state.Handles.Nudged(state.Aim, state.Perform, (point, key) => Bumped(state.Aim, state.Frame, point, key, -1)),
                    navigate: static (state, navigate) => state.Handles.Nudged(state.Aim, state.Perform, (point, key) => Navigated(state.Aim, state.Frame, navigate.Toward, point, key)),
                    delete: static (state, delete) => (from removed in state.Handles.Points.Removed
                                                       where delete.Toward.IsNone
                                                       let keys = state.Handles.Doomed(state.Aim)
                                                       select (state.Handles.Session(Cursor.Default) with { Perform = (start, _) => removed(start.Value, keys).Map(value => start with { Value = value }) })
                                                           .Stepped(state.Perform))
                        .IfNone(static () => new Reply.Pass()),
                    selectAll: static (state, _) => state.Handles.Pick.Match(
                        Some: _ => (state.Handles.Session(Cursor.Default) with {
                            Perform = (start, _) => Some(start with { Picked = start.Picked.All(state.Handles.Points.Read(start.Value).Map(static point => point.Key)) }),
                        }).Stepped(state.Perform),
                        None: static () => new Reply.Pass()))),
            None: static () => new Reply.Pass());

    private Session<Held> Session(Cursor cursor) => new(Start, Sinks, cursor) { Detent = static held => held.Detent };

    private Reply Pressed(Gesture.Press press, Aim aim, PlotFrame frame) =>
        (Key: Point(aim), Restored: Rest.Filter(_ => press.Clicks >= 2)) switch {
            var held => held.Key.Match(
                Some: key => (from rest in held.Restored
                              from point in Points.Read(rest).Find(point => point.Key == key)
                              select Session(Gripped(frame)) with { Press = (start, _) => Some(Moved(start, [(key, point.At)], press.Modifiers, None)) })
                    .IfNone(() => Dragging(key, aim, frame))
                    .Began(press),
                None: () => held.Restored.Match(
                    Some: rest => (Session(GroundCursor.IfNone(Gripped(frame))) with { Press = (start, _) => Some(start with { Value = rest }) }).Began(press),
                    None: () => Points.Placed.Match(
                        Some: place => frame.At(press.From).Match(
                            Some: target => (Session(GroundCursor.IfNone(Gripped(frame))) with {
                                Press = (start, moved) => from placed in place(start.Value, target, Nearest(start.Value, target, frame))
                                                          select Dragged(start with { Value = placed.Value }, placed.Key, moved, aim, frame),
                            }).Began(press),
                            None: static () => new Reply.Pass()),
                        None: static () => new Reply.Pass()))),
        };

    private Session<Held> Dragging(Key key, Aim aim, PlotFrame frame) =>
        Session(Gripped(frame)) with { Press = (start, press) => Some(Dragged(Spread(start, key, press, aim), key, press, aim, frame)) };

    private Held Spread(Held start, Key key, Gesture.Press press, Aim aim) =>
        (from spread in Points.Spread
         where press.Scrolled.Y.CompareTo(0f) is not 0
         select start with { Value = spread(start.Value, key, press.Scrolled.Y / Metrics(aim.Laid, Some(key))[Metric.Extent]) }).IfNone(start);

    private Held Dragged(Held start, Key key, Gesture.Press press, Aim aim, PlotFrame frame) =>
        start.Picked.Raised(Points.Read(start.Value).Map(static point => point.Key), key, press.Modifiers) switch {
            var picked when press.Dragged.IsNone => start with { Picked = picked },
            var picked => Tearing(start.Value, key, press, aim, frame).Match(
                Some: torn => new Held(torn, picked, None),
                None: () => Carried(start with { Picked = picked }, key, press, aim, frame)),
        };

    private Option<TValue> Tearing(TValue value, Key key, Gesture.Press press, Aim aim, PlotFrame frame) =>
        Metrics(aim.Laid, Some(key))[Metric.Reach] switch {
            var reach => SKRect.Inflate(frame.Matrix.MapRect(Square), reach, reach).Contains(press.At) ? None : Points.Torn.Bind(torn => torn(value, key)),
        };

    private Held Carried(Held start, Key key, Gesture.Press press, Aim aim, PlotFrame frame) {
        bool fine = press.Modifiers.Contains(Modifier.Common);
        Seq<(Key Key, Vector2 At)> read = Points.Read(start.Value);
        LanguageExt.HashSet<Key> moving = toHashSet(read.Map(static point => point.Key).Filter(held => start.Picked.Holds(key) ? start.Picked.Holds(held) : held == key));
        Vector2 travel = frame.Travel(press.From, press.At).IfNone(Vector2.Zero) * (fine ? Fine : 1f);
        Seq<(Key Key, Vector2 To)> targets = read.Filter(point => moving.Contains(point.Key)).Map(point => (point.Key, To: Snapped(frame, frame.Plane.Held(point.At + travel), press.Modifiers, aim.Laid)));
        Seq<Vector2> detents = Rest.Map(rest => Points.Read(rest).Map(static point => point.At)).IfNone([]) + read.Filter(point => !moving.Contains(point.Key)).Map(static point => point.At);
        Option<Vector2> detent = targets is [var only] && !fine ? frame.Nearest(only.To, detents, Metrics(aim.Laid, Some(key))[Metric.Reach]).Bind(index => detents.At(index)) : None;
        return Moved(start, detent.Match(Some: to => targets.Map(target => (target.Key, To: to)), None: () => targets), press.Modifiers, detent);
    }

    private Held Moved(Held start, Seq<(Key Key, Vector2 To)> targets, LanguageExt.HashSet<Modifier> modifiers, Option<Vector2> detent) =>
        Points.Moved(start.Value, targets, modifiers) switch {
            var (value, keys) => new Held(value, start.Picked.Renamed(toHashMap(targets.Map(static target => target.Key).Zip(keys))), detent),
        };

    private Option<Session<Held>> Wheeled(Aim aim, PlotFrame frame) =>
        from key in Point(aim) | Points.Placed.Bind(_ => Lead)
        from point in Points.Read(Value).Find(point => point.Key == key)
        let notch = Metrics(aim.Laid, Some(key))[Metric.Extent]
        select Session(Gripped(frame)) with { Scroll = (start, scroll) => Some(Notched(start, key, point.At, scroll, frame, notch)) };

    private Held Notched(Held start, Key key, Vector2 point, Gesture.Scroll scroll, PlotFrame frame, float notch) =>
        Notches(scroll.Travel, notch, frame.Plane.Dimensions.Count > 1) switch {
            var steps when steps == Vector2.Zero => start,
            var steps => Moved(start, [(key, Stepped(frame, point, steps, notch))], scroll.Modifiers, None),
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

    private Option<Session<Held>> Turning(Aim aim, PlotFrame frame) =>
        from key in Target(aim)
        from point in Points.Read(Value).Find(point => point.Key == key)
        select Session(Gripped(frame)) with {
            Rotate = (start, rotate) => from to in frame.Plane.Turned(point.At, rotate.Rotation)
                                        select Moved(start, [(key, to)], rotate.Modifiers, None),
        };

    private Reply Nudged(Aim aim, Gesture.Perform perform, Func<Vector2, Key, Vector2> move) =>
        (from key in Target(aim)
         from point in Points.Read(Value).Find(point => point.Key == key)
         select (Session(Cursor.Default) with { Perform = (start, _) => Some(Moved(start, [(key, move(point.At, key))], perform.Modifiers, None)) }).Stepped(perform))
        .IfNone(static () => new Reply.Pass());

    private static Vector2 Bumped(Aim aim, PlotFrame frame, Vector2 point, Key key, int sign) =>
        frame.Plane.Dimensions.Head.Map(first => frame.Stepped(point, first.Along, sign, Metrics(aim.Laid, Some(key))[Metric.Extent])).IfNone(point);

    private static Vector2 Navigated(Aim aim, PlotFrame frame, Navigation toward, Vector2 point, Key key) =>
        toward.Grain.Switch(
            (Frame: frame, Toward: toward, Point: point, Aim: aim, Key: key),
            unit: static held => held.Frame.Stepped(held.Point, held.Toward.Along, held.Toward.Valued(held.Frame), Metrics(held.Aim.Laid, Some(held.Key))[Metric.Extent]),
            page: static held => held.Frame.Stepped(held.Point, held.Toward.Along, held.Toward.Valued(held.Frame), Spacing(held.Aim.Laid)),
            edge: static held => held.Frame.Ended(held.Point, held.Toward.Along, held.Toward.Sign));

    private static Vector2 Snapped(PlotFrame frame, Vector2 value, LanguageExt.HashSet<Modifier> modifiers, Laid laid) =>
        modifiers.Contains(Modifier.Shift) ? frame.Plane.Dimensions.Fold(value, (held, free) => frame.Stepped(held, free.Along, 0, Spacing(laid))) : value;

    // --- [READ]
    private Option<Key> Lead => Picked.Lead | (Points.Read(Value) is [var only] ? Some(only.Key) : None);

    private Option<Key> Target(Aim aim) => Point(aim) | Lead;

    private Seq<Key> Doomed(Aim aim) =>
        Points.Read(Value).Map(static point => point.Key).Filter(Picked.Holds) switch {
            { IsEmpty: true } => Target(aim).ToSeq(),
            var picked => picked,
        };

    private Option<Key> Nearest(TValue value, Vector2 target, PlotFrame frame) =>
        Points.Read(value) switch {
            var read => frame.Nearest(target, read.Map(static point => point.At), float.PositiveInfinity).Bind(index => read.At(index)).Map(static point => point.Key),
        };

    private sealed record Held(TValue Value, Selection Picked, Option<Vector2> Detent);
}
