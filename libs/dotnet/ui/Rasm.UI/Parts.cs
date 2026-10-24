using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Edit<TValue> where TValue : notnull {
    public Option<TValue> Carried =>
        Switch(preview: static held => Some(held.Value), step: static held => Some(held.Value), commit: static held => Some(held.Value), cancel: static _ => Option<TValue>.None);

    public Option<TValue> Settled =>
        Switch(preview: static _ => Option<TValue>.None, step: static held => Some(held.Value), commit: static held => Some(held.Value), cancel: static _ => Option<TValue>.None);

    public Edit<TOut> Map<TOut>(Func<TValue, TOut> map) where TOut : notnull =>
        Switch(
            map,
            preview: static (into, held) => (Edit<TOut>)new Edit<TOut>.Preview(into(held.Value)),
            step: static (into, held) => new Edit<TOut>.Step(into(held.Value)),
            commit: static (into, held) => new Edit<TOut>.Commit(into(held.Value)),
            cancel: static (_, _) => new Edit<TOut>.Cancel());

    public Option<Edit<TOut>> Choose<TOut>(Func<TValue, Option<TOut>> map) where TOut : notnull =>
        Switch(
            map,
            preview: static (into, held) => into(held.Value).Map(static value => (Edit<TOut>)new Edit<TOut>.Preview(value)),
            step: static (into, held) => into(held.Value).Map(static value => (Edit<TOut>)new Edit<TOut>.Step(value)),
            commit: static (into, held) => into(held.Value).Map(static value => (Edit<TOut>)new Edit<TOut>.Commit(value)),
            cancel: static (_, _) => Some<Edit<TOut>>(new Edit<TOut>.Cancel()));

    public sealed record Preview(TValue Value) : Edit<TValue>;

    public sealed record Step(TValue Value) : Edit<TValue>;

    public sealed record Commit(TValue Value) : Edit<TValue>;

    public sealed record Cancel : Edit<TValue>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Selection {
    public abstract Option<Key> Lead { get; }

    public bool Holds(Key key) => Switch(key, one: static (held, one) => one.Picked == Some(held), many: static (held, many) => many.Picked.Contains(held));

    public Selection Raised(Seq<Key> ordered, Key hit, Modifiers modifiers) =>
        Switch(
            (Ordered: ordered, Hit: hit, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(Some(held.Hit)),
            many: static (held, many) => (held.Modifiers.Common, held.Modifiers.Shift) switch {
                (true, _) => new Many(many.Picked.Contains(held.Hit) ? many.Picked.Remove(held.Hit) : many.Picked.Add(held.Hit), Some(held.Hit), Some(held.Hit)),
                (_, true) => Spanned(held.Ordered, many.Anchor, held.Hit),
                _ when many.Picked.Contains(held.Hit) => new Many(many.Picked, Some(held.Hit), Some(held.Hit)),
                _ => Only(held.Hit),
            });

    public Selection Led(Seq<Key> ordered, Key lead, bool extend) =>
        Switch(
            (Ordered: ordered, Lead: lead, Extend: extend),
            one: static (held, _) => (Selection)new One(Some(held.Lead)),
            many: static (held, many) => held.Extend ? Spanned(held.Ordered, many.Anchor | many.Lead, held.Lead) : Only(held.Lead));

    public Selection All(Seq<Key> ordered) => Switch(ordered, one: static (_, one) => (Selection)one, many: static (keys, many) => new Many(toHashSet(keys), many.Anchor, many.Lead));

    internal static Option<int> Position(Seq<Key> ordered, Key key) =>
        ordered.Map(static (held, index) => (Key: held, Index: index)).Find(entry => entry.Key == key).Map(static entry => entry.Index);

    private static Many Only(Key key) => new([key], Some(key), Some(key));

    private static Many Spanned(Seq<Key> ordered, Option<Key> anchor, Key target) =>
        (from end in Position(ordered, target)
         let found = anchor.Bind(held => Position(ordered, held))
         let start = found.IfNone(end)
         select new Many(toHashSet(ordered.Skip(int.Min(start, end)).Take(int.Abs(start - end) + 1)), found.IsSome ? anchor : Some(target), Some(target)))
        .IfNone(() => Only(target));

    public sealed record One(Option<Key> Picked) : Selection {
        public override Option<Key> Lead => Picked;
    }

    public sealed record Many(LanguageExt.HashSet<Key> Picked, Option<Key> Anchor, Option<Key> Lead) : Selection {
        public override Option<Key> Lead { get; } = Lead;
    }
}

public abstract record Grip {
    private protected Grip() { }

    internal virtual Option<int> Snapped => None;

    public sealed record Adjust(SKPoint From, float Start) : Grip;

    public sealed record Held : Grip;

    public sealed record Drag<TValue>(TValue Start, int Index, SKPoint From, PlotPlane Plane, SKMatrix Frame, float Reach, bool Live, bool Placed, Option<int> Detent) : Grip where TValue : notnull {
        internal override Option<int> Snapped => Detent;
    }

    public sealed record Pan(PlotPlane Start, SKPoint From, SKMatrix Frame, bool Live) : Grip;

    public sealed record Pick(Selection Prior, Selection Picked, Seq<Key> Moving, SKPoint From, bool Live) : Grip;

    public sealed record Gesture(Vector2 Remainder, bool Moved) : Grip;
}

public sealed record Handles<TValue>(
    Func<TValue, Seq<Vector2>> Read,
    Func<TValue, int, Vector2, Modifiers, (TValue Value, int Index)> Moved,
    Func<TValue, Vector2, Option<int>, Option<(TValue Value, int Index)>> Placed,
    Func<Vector2, string> Spoken) where TValue : notnull {
    public Option<Func<TValue, Option<int>, Option<TValue>>> Removed { get; init; }
    public Option<Func<TValue, int, Option<TValue>>> Torn { get; init; }
    public Option<Func<TValue, TValue, int, TValue>> Reset { get; init; }
    public Option<Func<TValue, int, float, TValue>> Spread { get; init; }

    public static Handles<TValue> Fixed(Seq<Lens<TValue, Vector2>> points, Func<Vector2, string> spoken) =>
        new(value => points.Map(point => point.Get(value)),
            (value, index, to, _) => (points[index].Set(to, value), index),
            (value, at, nearest) => nearest.Map(index => (points[index].Set(at, value), index)),
            spoken) {
            Reset = Some<Func<TValue, TValue, int, TValue>>((value, rest, index) => points[index].Set(points[index].Get(rest), value)),
        };
}

public sealed record Viewport(PlotPlane Rest, AxisRange Spans, Func<Edit<PlotPlane>, IO<Unit>> Edited);

internal sealed record Delivery(Input Input, Seq<Key> Over, SKRect Bounds, Laid Laid, Option<Grip> Grip, Option<Editor> Editor);

internal sealed record Sight(Laid Laid, Func<Seq<Key>, bool> InView, Option<Seq<Key>> Hover);

public abstract record Part {
    private const float Fine = 0.1f;
    private const float Notch = 1.2f;
    private static readonly SKRect Square = new(0f, 0f, 1f, 1f);

    private protected Part() { }

    internal static IO<Unit> Handled { get; } = IO.pure(unit);

    internal abstract (Option<Grip> Grip, Option<IO<Unit>> Answer) Receive(Delivery delivery);

    internal abstract Cursor Cursor(Laid laid, Seq<Key> over, Option<Grip> grip);

    internal abstract Seq<(Seq<Key> Path, Accessible Accessible)> Access(Laid laid);

    internal virtual bool Edits => false;

    internal virtual Option<Seq<Key>> Revealed(Laid laid) => None;

    internal virtual Option<IO<Unit>> Watched(Option<Sight> before, Sight after) => None;

    internal static Option<IO<Unit>> Joined(params Seq<Option<IO<Unit>>> answers) =>
        answers.Somes().Fold(Option<IO<Unit>>.None, static (held, next) => Some(held.Match(Some: first => first.Bind(_ => next), None: () => next)));

    private protected static (Option<Grip> Grip, Option<IO<Unit>> Answer) Ignored(Delivery delivery) => (delivery.Grip, None);

    private protected static Option<T> Held<T>(Option<Grip> grip) where T : Grip => grip.Bind(static held => Optional(held as T));

    private protected static Option<IO<Unit>> Changed<T>(T current, T next, Func<Edit<T>, IO<Unit>> edited, Func<T, Edit<T>> edit) where T : notnull =>
        next.Equals(current) ? None : Some(edited(edit(next)));

    private protected static string Label(Laid laid) => laid.Node.Accessible.Bind(static held => held.Label).IfNone(() => string.Join(' ', Texts(laid)));

    private static Seq<string> Texts(Laid laid) => (laid.Node is Node.Text text ? Seq(text.Content) : []) + laid.Children.Bind(Texts);

    public sealed record Press(CommandFace Face, Func<SKRect, IO<Unit>> Run) : Part {
        public Option<Func<SKRect, IO<Unit>>> Secondary { get; init; }
        public Option<bool> Marked { get; init; }

        internal override (Option<Grip> Grip, Option<IO<Unit>> Answer) Receive(Delivery delivery) =>
            delivery.Input.SwitchPartially(
                (Press: this, Delivery: delivery),
                @default: static (held, _) => Ignored(held.Delivery),
                pointer: static (held, pointer) => (pointer.Phase, held.Delivery.Grip.IsSome) switch {
                    (PointerPhase.Down, _) => (Some<Grip>(new Grip.Held()), Some(Handled)),
                    (PointerPhase.Up, true) => (None, Some(held.Delivery.Bounds.Contains(pointer.At) ? held.Press.Run(held.Delivery.Bounds) : Handled)),
                    (PointerPhase.Move, true) => (held.Delivery.Grip, Some(Handled)),
                    _ => Ignored(held.Delivery),
                },
                perform: static (held, perform) => perform.Act.SwitchPartially(
                    held,
                    @default: static (state, _) => Ignored(state.Delivery),
                    press: Ran,
                    confirm: Ran,
                    cancel: static (state, _) => (None, state.Delivery.Grip.Map(static _ => Handled)),
                    menu: static (state, _) => (state.Delivery.Grip, state.Press.Secondary.Map(secondary => secondary(state.Delivery.Bounds)))),
                focus: static (held, focus) => focus.Gained ? Ignored(held.Delivery) : (None, None));

        internal override Cursor Cursor(Laid laid, Seq<Key> over, Option<Grip> grip) => UI.Cursor.Default;

        internal override Seq<(Seq<Key> Path, Accessible Accessible)> Access(Laid laid) =>
            [(laid.Path, Described(Face, Marked, laid.Node.Accessible.Map(static held => held.Role), Secondary.IsSome ? [new Act.Press(), new Act.Menu()] : [new Act.Press()]))];

        internal static Accessible Described(CommandFace face, Option<bool> marked, Option<AccessRole> declared, Seq<Act> actions) =>
            declared.IfNone(marked.IsSome ? AccessRole.CheckBox : AccessRole.Button) switch {
                var role => new Accessible(role, face.Caption) {
                    Value = marked.Filter(_ => role is not AccessRole.Disclosure).Map(static on => (AccessValue)new AccessValue.Toggled(on)),
                    Expanded = marked.Filter(_ => role is AccessRole.Disclosure),
                    Actions = actions,
                    Help = face.Help,
                },
            };

        private static (Option<Grip> Grip, Option<IO<Unit>> Answer) Ran((Press Press, Delivery Delivery) held, Act _) =>
            (held.Delivery.Grip, Some(held.Press.Run(held.Delivery.Bounds)));
    }

    public sealed record Pad<TValue>(PlotPlane Plane, TValue Value, Handles<TValue> Handles, Func<Edit<TValue>, IO<Unit>> Edited) : Part where TValue : notnull {
        private static readonly Key Ground = new Key.Named("ground");
        private static readonly Key Points = new Key.Named("handles");

        public Option<TValue> Rest { get; init; }
        public Option<int> Lead { get; init; }
        public Option<Func<int, IO<Unit>>> Led { get; init; }
        public Option<Viewport> Viewport { get; init; }
        public Option<Func<Option<Vector2>, IO<Unit>>> Hover { get; init; }
        public Option<Cursor> GroundCursor { get; init; }

        public Node Plot(Seq<Node> ground, Option<Func<int, Vector2, Node>> handle = default) =>
            new Node.Overlay([
                new Node.Plot(Plane, ground.Map(Modified)) { Key = Ground },
                new Node.Plot(Plane, Handles.Read(Value).Map((point, index) =>
                    Modified(handle.Map(draw => draw(index, point)).IfNone(() => new Node.Pin(point) { Role = Role.Handle }) with { Key = new Key.Ordinal((uint)index) }))) { Key = Points },
            ]) {
                Part = this,
            };

        private Node Modified(Node node) =>
            Rest.Exists(rest => !rest.Equals(Value))
                ? node is Node.Styled styled ? styled with { Content = Modified(styled.Content) } : node with { Conditions = node.Conditions.TryAdd(Condition.Modified) }
                : node;

        // --- [RECEIVE]
        internal override (Option<Grip> Grip, Option<IO<Unit>> Answer) Receive(Delivery delivery) =>
            delivery.Input.SwitchPartially(
                (Pad: this, Delivery: delivery),
                @default: static (held, _) => Ignored(held.Delivery),
                pointer: static (held, pointer) => held.Pad.Pointed(pointer, held.Delivery),
                scroll: static (held, scroll) => held.Pad.Scrolled(scroll, held.Delivery),
                pan: static (held, pan) => held.Pad.Swept(pan, held.Delivery),
                magnify: static (held, magnify) => held.Pad.Magnified(magnify, held.Delivery),
                rotate: static (held, rotate) => held.Pad.Rotated(rotate, held.Delivery),
                perform: static (held, perform) => held.Pad.Performed(perform, held.Delivery),
                focus: static (held, focus) => focus.Gained ? Ignored(held.Delivery) : held.Pad.Cancelled(held.Delivery.Grip));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Pointed(Input.Pointer pointer, Delivery at) =>
            pointer.Phase switch {
                PointerPhase.Down when pointer.Count >= 2 => Restored(at).IfNone(() => Pressed(pointer, at)),
                PointerPhase.Down => Pressed(pointer, at),
                PointerPhase.Move or PointerPhase.Leave => (Held<Grip.Drag<TValue>>(at.Grip).Filter(_ => pointer.Phase is PointerPhase.Move).Map(drag => Dragged(drag, pointer, at))
                    | Held<Grip.Pan>(at.Grip).Filter(_ => pointer.Phase is PointerPhase.Move).Map(pan => Panned(pan, pointer.At, at, settles: false))
                    | Hover.Map(hover => (at.Grip, Answer: Some(hover(pointer.Phase is PointerPhase.Move ? Plane.At(Framed(at), pointer.At) : None))))).IfNone(() => Ignored(at)),
                PointerPhase.Up => (Held<Grip.Drag<TValue>>(at.Grip).Map(drag => Released(drag, pointer, at)) | Held<Grip.Pan>(at.Grip).Map(pan => Panned(pan, pointer.At, at, settles: true)))
                    .IfNone(() => (None, at.Grip.Map(static _ => Handled))),
            };

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Pressed(Input.Pointer pointer, Delivery at) =>
            Framed(at) switch {
                var frame => Point(at).Match(
                    Some: index => (Some<Grip>(new Grip.Drag<TValue>(Value, index, pointer.At, Plane, frame, Handle(at.Laid, Some(index))[Metric.Reach], Live: false, Placed: false, Detent: None)),
                        Leading(index) | Some(Handled)),
                    None: () => ((from target in Plane.At(frame, pointer.At)
                                  let nearest = Plane.Nearest(target, Handles.Read(Value), frame, float.PositiveInfinity)
                                  from placed in Handles.Placed(Value, target, nearest)
                                  select (Grip: Some<Grip>(new Grip.Drag<TValue>(placed.Value, placed.Index, pointer.At, Plane, frame, Handle(at.Laid, nearest)[Metric.Reach], Live: false, Placed: true, Detent: None)),
                                      Answer: Joined(Changed(Value, placed.Value, Edited, static value => new Edit<TValue>.Preview(value)), Leading(placed.Index)) | Some(Handled)))
                        | Viewport.Map(_ => (Grip: Some<Grip>(new Grip.Pan(Plane, pointer.At, frame, Live: false)), Answer: Some(Handled))))
                        .IfNone(() => Ignored(at))),
            };

        private Option<(Option<Grip> Grip, Option<IO<Unit>> Answer)> Restored(Delivery at) =>
            Point(at).Match(
                Some: index => from reset in Handles.Reset
                               from rest in Rest
                               select (Grip: Option<Grip>.None, Answer: Changed(Value, reset(Value, rest, index), Edited, static value => new Edit<TValue>.Commit(value)) | Some(Handled)),
                None: () => Viewport.Map(viewport => (Grip: Option<Grip>.None, Answer: Changed(Plane, viewport.Rest, viewport.Edited, static plane => new Edit<PlotPlane>.Commit(plane)) | Some(Handled)))
                    | Rest.Map(rest => (Grip: Option<Grip>.None, Answer: Changed(Value, rest, Edited, static value => new Edit<TValue>.Commit(value)) | Some(Handled))));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Dragged(Grip.Drag<TValue> drag, Input.Pointer pointer, Delivery at) =>
            !drag.Live && SKPoint.Distance(pointer.At, drag.From) < Handle(at.Laid, Some(drag.Index))[Metric.Slop]
                ? (Some<Grip>(drag), Some(Handled))
                : Moved(drag, pointer.At, pointer.Modifiers, at);

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Moved(Grip.Drag<TValue> drag, SKPoint pointer, Modifiers modifiers, Delivery at) =>
            Through(drag, pointer, modifiers, at) switch {
                var (value, index, detent) => (Some<Grip>(drag with { Live = true, Detent = detent }),
                    Joined(Changed(Value, value, Edited, static held => new Edit<TValue>.Preview(held)), index.Bind(Leading)) | Some(Handled)),
            };

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Released(Grip.Drag<TValue> drag, Input.Pointer pointer, Delivery at) =>
            (None, (drag.Live, drag.Placed) switch {
                (true, _) => Some(Edited(new Edit<TValue>.Commit(Through(drag, pointer.At, pointer.Modifiers, at).Value))),
                (false, true) => Some(Edited(new Edit<TValue>.Commit(drag.Start))),
                (false, false) => Some(Handled),
            });

        private (TValue Value, Option<int> Index, Option<int> Detent) Through(Grip.Drag<TValue> drag, SKPoint pointer, Modifiers modifiers, Delivery at) {
            Seq<Vector2> held = Handles.Read(drag.Start);
            Vector2 origin = held[drag.Index];
            Vector2 raw = drag.Plane.Moved(origin, drag.From, pointer, drag.Frame, modifiers.Common ? Fine : 1f).IfNone(origin);
            Vector2 snapped = modifiers.Shift ? Snapped(drag.Plane, raw, drag.Frame, at.Laid) : raw;
            Seq<Vector2> detents = Rest.Map(Handles.Read).IfNone([]) + held.Map(static (point, index) => (Point: point, Index: index)).Filter(entry => entry.Index != drag.Index).Map(static entry => entry.Point);
            Option<int> detent = modifiers.Common ? None : drag.Plane.Nearest(snapped, detents, drag.Frame, drag.Reach);
            Vector2 to = detent.Map(index => detents[index]).IfNone(snapped);
            return (SKRect.Inflate(drag.Frame.MapRect(Square), drag.Reach, drag.Reach).Contains(pointer) ? None : Handles.Torn.Bind(torn => torn(drag.Start, drag.Index)))
                .Match(
                    Some: static torn => (torn, Option<int>.None, Option<int>.None),
                    None: () => Handles.Moved(drag.Start, drag.Index, to, modifiers) switch {
                        var (value, index) => (value, Some(index), detent),
                    });
        }

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Panned(Grip.Pan pan, SKPoint at, Delivery delivery, bool settles) =>
            !pan.Live && SKPoint.Distance(at, pan.From) < delivery.Laid.Style.Metrics[Metric.Slop]
                ? (settles ? None : Some<Grip>(pan), Some(Handled))
                : (settles ? None : Some<Grip>(pan with { Live = true }), Shifted(pan, at, settles) | Some(Handled));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Swept(Input.Pan pan, Delivery at) =>
            Viewport.Map(_ => Held<Grip.Pan>(at.Grip).IfNone(() => new Grip.Pan(Plane, pan.At - pan.Translation, Framed(at), Live: true)) switch {
                var held => (Grip: pan.Phase is GesturePhase.Ends ? Option<Grip>.None : Some<Grip>(held), Answer: Shifted(held, held.From + pan.Translation, pan.Phase is GesturePhase.Ends) | Some(Handled)),
            }).IfNone(() => Ignored(at));

        private Option<IO<Unit>> Shifted(Grip.Pan pan, SKPoint at, bool settles) =>
            from viewport in Viewport
            from travel in pan.Start.Travel(pan.From, at, pan.Frame)
            from plane in pan.Start.Shifted(-travel).Filter(shifted => shifted.Resolves(pan.Frame))
            from answer in settles ? Some(viewport.Edited(new Edit<PlotPlane>.Commit(plane))) : Changed(Plane, plane, viewport.Edited, static held => new Edit<PlotPlane>.Preview(held))
            select answer;

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Cancelled(Option<Grip> grip) =>
            (None, Held<Grip.Drag<TValue>>(grip).Filter(static drag => drag.Live || drag.Placed).Map(_ => Edited(new Edit<TValue>.Cancel()))
                | Held<Grip.Pan>(grip).Filter(static pan => pan.Live).Bind(_ => Viewport).Map(static viewport => viewport.Edited(new Edit<PlotPlane>.Cancel()))
                | Held<Grip.Gesture>(grip).Filter(static gesture => gesture.Moved).Map(_ => Edited(new Edit<TValue>.Cancel())));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Performed(Input.Perform perform, Delivery at) =>
            perform.Act.SwitchPartially(
                (Pad: this, At: at, perform.Modifiers),
                @default: static (held, _) => Ignored(held.At),
                cancel: static (held, _) => held.Pad.Cancelled(held.At.Grip),
                delete: static (held, _) => (from removed in held.Pad.Handles.Removed
                                             from value in removed(held.Pad.Value, held.Pad.Target(held.At))
                                             select (held.At.Grip, Answer: Changed(held.Pad.Value, value, held.Pad.Edited, static next => new Edit<TValue>.Step(next)) | Some(Handled)))
                    .IfNone(() => Ignored(held.At)),
                step: static (held, step) => held.Pad.Nudged(held.At, held.Modifiers, (point, index) => held.Pad.Plane.Stepped(point, step.Along, step.Count, held.Pad.Framed(held.At), Handle(held.At.Laid, Some(index))[Metric.Extent])),
                page: static (held, page) => held.Pad.Nudged(held.At, held.Modifiers, (point, _) => held.Pad.Plane.Stepped(point, page.Along, page.Count, held.Pad.Framed(held.At), Spacing(held.At.Laid, page.Along))),
                extreme: static (held, extreme) => held.Pad.Nudged(held.At, held.Modifiers, (point, _) => held.Pad.Plane.Ended(point, extreme.Along, extreme.Toward)));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Nudged(Delivery at, Modifiers modifiers, Func<Vector2, int, Vector2> move) =>
            (from index in Target(at)
             from point in Handles.Read(Value).At(index)
             let moved = Handles.Moved(Value, index, move(point, index), modifiers)
             select (at.Grip, Answer: Joined(Changed(Value, moved.Value, Edited, static value => new Edit<TValue>.Step(value)), Leading(moved.Index)) | Some(Handled)))
            .IfNone(() => Ignored(at));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Scrolled(Input.Scroll scroll, Delivery at) =>
            Held<Grip.Drag<TValue>>(at.Grip).Match(
                Some: drag => Handles.Spread.Filter(_ => drag.Live).Map(spread => Spreading(drag, spread, scroll, at)).IfNone(() => (at.Grip, Some(Handled))),
                None: () => Held<Grip.Pan>(at.Grip).IsSome ? (at.Grip, Some(Handled)) : (Zoomed(scroll, at) | Stepped(scroll, at)).IfNone(() => Ignored(at)));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Spreading(Grip.Drag<TValue> drag, Func<TValue, int, float, TValue> spread, Input.Scroll scroll, Delivery at) =>
            Moved(drag with { Start = spread(drag.Start, drag.Index, scroll.Delta.Y / Handle(at.Laid, Some(drag.Index))[Metric.Extent]) }, scroll.At, scroll.Modifiers, at);

        private Option<(Option<Grip> Grip, Option<IO<Unit>> Answer)> Zoomed(Input.Scroll scroll, Delivery at) =>
            from viewport in Viewport
            where Point(at).IsNone
            let frame = Framed(at)
            from about in Plane.At(frame, scroll.At)
            let plane = Scaled(viewport, about, MathF.Pow(Notch, -scroll.Delta.Y / Handle(at.Laid, Target(at))[Metric.Extent]), frame)
            select (Grip: Option<Grip>.None, Answer: Burst(scroll.Precise, scroll.Phase, moved: true, Plane, plane, viewport.Edited) | Some(Handled));

        private Option<(Option<Grip> Grip, Option<IO<Unit>> Answer)> Stepped(Input.Scroll scroll, Delivery at) =>
            from index in Target(at)
            from point in Handles.Read(Value).At(index)
            let notch = Handle(at.Laid, Some(index))[Metric.Extent]
            let gesture = Held<Grip.Gesture>(at.Grip)
            let total = gesture.Map(static held => held.Remainder).IfNone(Vector2.Zero) + new Vector2(scroll.Delta.X, scroll.Delta.Y)
            let notches = new Vector2(MathF.Truncate(total.X / notch), MathF.Truncate(total.Y / notch))
            let steps = Plane.Dimensions.Count > 1 ? notches : MathF.Abs(total.X) > MathF.Abs(total.Y) ? notches with { Y = 0f } : notches with { X = 0f }
            let frame = Framed(at)
            let moved = steps == Vector2.Zero
                ? Option<(TValue Value, int Index)>.None
                : Some(Handles.Moved(Value, index, Plane.Dimensions.Fold(point, (held, free) => (Plane.Dimensions.Count > 1 ? free.Along.Of(steps) : steps.X + steps.Y) switch {
                    var count when count.CompareTo(0f) is 0 => held,
                    var count => Plane.Stepped(held, free.Along, (int)count, frame, notch),
                }), scroll.Modifiers))
            let previewed = gesture.Exists(static held => held.Moved)
            select (Grip: scroll.Precise && scroll.Phase is not GesturePhase.Ends ? Some<Grip>(new Grip.Gesture(total - (steps * notch), previewed || moved.Exists(held => !held.Value.Equals(Value)))) : None,
                Answer: Joined(Burst(scroll.Precise, scroll.Phase, previewed, Value, moved.Map(static held => held.Value), Edited), moved.Bind(held => Leading(held.Index))) | Some(Handled));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Magnified(Input.Magnify magnify, Delivery at) =>
            (from viewport in Viewport
             let frame = Framed(at)
             from about in Plane.At(frame, magnify.At)
             select (at.Grip, Answer: Burst(precise: true, magnify.Phase, moved: true, Plane, Scaled(viewport, about, 1f / (1f + magnify.Amount), frame), viewport.Edited) | Some(Handled)))
            .IfNone(() => Ignored(at));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Rotated(Input.Rotate rotate, Delivery at) =>
            (from polar in Optional(Plane as PlotPlane.Polar)
             from index in Target(at)
             from point in Handles.Read(Value).At(index)
             let moved = Handles.Moved(Value, index, point with { X = polar.Angle.Held(point.X + (rotate.Radians / MathF.Tau * polar.Angle.Range.Span)) }, Modifiers.Plain)
             select (at.Grip, Answer: Burst(precise: true, rotate.Phase, moved: true, Value, Some(moved.Value), Edited) | Some(Handled)))
            .IfNone(() => Ignored(at));

        private static Option<IO<Unit>> Burst<T>(bool precise, GesturePhase phase, bool moved, T current, Option<T> next, Func<Edit<T>, IO<Unit>> edited) where T : notnull =>
            (precise, phase) switch {
                (false, _) => next.Bind(held => Changed(current, held, edited, static value => new Edit<T>.Step(value))),
                (true, GesturePhase.Ends) => moved || next.Exists(held => !held.Equals(current)) ? Some(edited(new Edit<T>.Commit(next.IfNone(current)))) : None,
                (true, _) => next.Bind(held => Changed(current, held, edited, static value => new Edit<T>.Preview(value))),
            };

        private Option<PlotPlane> Scaled(Viewport viewport, Vector2 about, float factor, SKMatrix frame) =>
            from free in Plane.Dimensions.Head
            from rest in viewport.Rest.Dimension(free.Along)
            let ratio = MathF.Abs(free.Axis.Range.Span / rest.Range.Span)
            let held = float.Clamp(ratio * factor, viewport.Spans.Low, viewport.Spans.High) / ratio
            from plane in held.CompareTo(1f) is 0 ? Some(Plane) : Plane.Scaled(about, held).Filter(scaled => scaled.Resolves(frame))
            select plane;

        // --- [READ]
        internal override Cursor Cursor(Laid laid, Seq<Key> over, Option<Grip> grip) =>
            (Held<Grip.Pan>(grip).IsSome, Held<Grip.Drag<TValue>>(grip).IsSome || Point(over, laid).IsSome) switch {
                (true, _) => UI.Cursor.Move,
                (_, true) => Plane.Switch(
                    cartesian: static _ => UI.Cursor.Move,
                    track: static track => track.Along.Resize,
                    polar: static _ => UI.Cursor.Move),
                _ => GroundCursor.IfNone(Viewport.IsSome ? UI.Cursor.Move : UI.Cursor.Default),
            };

        internal override Seq<(Seq<Key> Path, Accessible Accessible)> Access(Laid laid) =>
            Handles.Read(Value).Map((point, index) => (laid.Path.Add(Points).Add(new Key.Ordinal((uint)index)),
                new Accessible(
                    Plane.Dimensions.Count > 1 ? AccessRole.Handle : AccessRole.Slider,
                    Drawn(laid, index).Bind(static child => child.Node.Accessible).Bind(static held => held.Label).IfNone(() => Label(laid))) {
                    Value = Plane.Dimensions.Head.Map(held => (AccessValue)new AccessValue.Range(held.Along.Of(point), held.Axis.Range.Low, held.Axis.Range.High, Handles.Spoken(point))),
                    Actions = Plane.Dimensions.Bind(static held => Seq<Act>(new Act.Step(held.Along, 1), new Act.Step(held.Along, -1))) + (Handles.Removed.IsSome ? Seq<Act>(new Act.Delete()) : []),
                    Help = laid.Node.Accessible.Bind(static held => held.Help),
                }));

        internal override Option<Seq<Key>> Revealed(Laid laid) => Lead.Map(index => laid.Path.Add(Points).Add(new Key.Ordinal((uint)index)));

        private static Option<int> Point(Delivery at) => Point(at.Over, at.Laid);

        private static Option<int> Point(Seq<Key> over, Laid laid) =>
            over.Last.Bind(static last => Optional(last as Key.Ordinal)).Filter(_ => over.Init == laid.Path.Add(Points)).Map(static ordinal => (int)ordinal.Index);

        private Option<int> Target(Delivery at) => Point(at) | Lead | (Handles.Read(Value).Count is 1 ? Some(0) : None);

        private Option<IO<Unit>> Leading(int index) => Lead == Some(index) ? None : Led.Map(led => led(index));

        private static Vector2 Snapped(PlotPlane plane, Vector2 value, SKMatrix frame, Laid laid) =>
            plane.Dimensions.Fold(value, (held, free) => plane.Stepped(held, free.Along, 0, frame, Spacing(laid, free.Along)));

        private SKMatrix Framed(Delivery at) =>
            (from plot in Child(at.Laid, Points)
             from held in Layouts.Plotted(plot)
             select held.Frame.PostConcat(SKMatrix.CreateTranslation(at.Bounds.Left + plot.Frame.Left, at.Bounds.Top + plot.Frame.Top)))
            .IfNone(() => Plane.Frame(at.Bounds));

        private static Option<Laid> Child(Laid laid, Key key) => laid.Children.Find(child => child.Path.Last == Some(key));

        private static Option<Laid> Drawn(Laid laid, int index) => Child(laid, Points).Bind(plot => Child(plot, new Key.Ordinal((uint)index)));

        private static HashMap<Metric, float> Handle(Laid laid, Option<int> index) =>
            index.Bind(held => Drawn(laid, held)).Bind(static drawn => Scene.Preorder(drawn).Find(static held => held.Node.Role.IsSome))
                .Map(static drawn => drawn.Style.Metrics)
                .IfNone(() => Styles.Resolve(laid.Cascade, Role.Handle, laid.Node.Conditions).Metrics);

        private static float Spacing(Laid laid, Orientation along) =>
            Child(laid, Ground).Bind(ground => Scene.Preorder(ground).Find(held => held.Node is Node.Mark { Figure: Figure.Grid grid } && grid.Along == along))
                .Map(static grid => grid.Style.Metrics[Metric.Spacing]).IfNone(laid.Style.Metrics[Metric.Spacing]);
    }

    public sealed record Choice(Seq<Key> Items, Selection Value, Func<Edit<Selection>, IO<Unit>> Edited) : Part {
        private static readonly HashMap<AccessRole, AccessRole> Members = [(AccessRole.TabList, AccessRole.Tab), (AccessRole.Tree, AccessRole.Row), (AccessRole.List, AccessRole.Cell)];

        public Option<Func<Edit<Seq<Key>>, IO<Unit>>> Reorder { get; init; }
        public Option<Func<Seq<Key>, IO<Unit>>> Exported { get; init; }
        public Option<Func<Option<Key>, IO<Unit>>> Hover { get; init; }
        public Option<Func<LanguageExt.HashSet<Key>, IO<Unit>>> Viewed { get; init; }

        public Node Drawn(Func<Key, Node> item, Func<Seq<Node>, Node> container) =>
            container(Items.Map(key => item(key) switch {
                var node => node with {
                    Key = key,
                    Conditions = Value.Holds(key) ? node.Conditions.TryAdd(Condition.Selected) : node.Conditions,
                },
            })) with {
                Part = this,
            };

        // --- [RECEIVE]
        internal override (Option<Grip> Grip, Option<IO<Unit>> Answer) Receive(Delivery delivery) =>
            delivery.Input.SwitchPartially(
                (Choice: this, Delivery: delivery),
                @default: static (held, _) => Ignored(held.Delivery),
                pointer: static (held, pointer) => held.Choice.Pointed(pointer, held.Delivery),
                perform: static (held, perform) => held.Choice.Performed(perform, held.Delivery),
                focus: static (held, focus) => focus.Gained ? Ignored(held.Delivery) : held.Choice.Cancelled(held.Delivery));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Pointed(Input.Pointer pointer, Delivery at) =>
            pointer.Phase switch {
                PointerPhase.Down => Item(at.Over, at.Laid).Match(
                    Some: key => Picked(Value.Raised(Items, key, pointer.Modifiers), key, pointer.At),
                    None: () => Value.Switch(
                        (Choice: this, At: at),
                        one: static (held, _) => Ignored(held.At),
                        many: static (held, many) => (Option<Grip>.None,
                            Changed(held.Choice.Value, new Selection.Many([], None, many.Lead), held.Choice.Edited, static next => new Edit<Selection>.Commit(next)) | Some(Handled)))),
                PointerPhase.Move or PointerPhase.Leave => Held<Grip.Pick>(at.Grip).Filter(_ => pointer.Phase is PointerPhase.Move && (Reorder.IsSome || Exported.IsSome))
                    .Map(pick => Carried(pick, pointer, at)).IfNone(() => Ignored(at)),
                PointerPhase.Up => Held<Grip.Pick>(at.Grip).Map(pick => (Grip: Option<Grip>.None, Answer: Joined(
                        pick.Live ? Reorder.Map(reorder => reorder(new Edit<Seq<Key>>.Commit(Ordered(pick, at)))) : None,
                        Changed(pick.Prior, pick.Picked, Edited, static next => new Edit<Selection>.Commit(next))) | Some(Handled)))
                    .IfNone(static () => (None, None)),
            };

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Picked(Selection picked, Key key, SKPoint at) =>
            (Some<Grip>(new Grip.Pick(Value, picked, picked.Holds(key) ? Items.Filter(picked.Holds) : [key], at, Live: false)),
                Changed(Value, picked, Edited, static next => new Edit<Selection>.Preview(next)) | Some(Handled));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Carried(Grip.Pick pick, Input.Pointer pointer, Delivery at) =>
            (pick.Live || SKPoint.Distance(pointer.At, pick.From) >= at.Laid.Style.Metrics[Metric.Slop], at.Bounds.Contains(pointer.At)) switch {
                (false, _) => (Some<Grip>(pick), Some(Handled)),
                (true, false) when Exported.IsSome => (None, Joined(
                    pick.Live ? Reorder.Map(static reorder => reorder(new Edit<Seq<Key>>.Cancel())) : None,
                    Changed(pick.Prior, pick.Picked, Edited, static next => new Edit<Selection>.Commit(next)),
                    Exported.Map(export => export(pick.Moving)))),
                (true, _) => Reorder.Map(reorder => Reordered(reorder, pick, Ordered(pick, at))).IfNone(() => (Some<Grip>(pick), Some(Handled))),
            };

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Reordered(Func<Edit<Seq<Key>>, IO<Unit>> reorder, Grip.Pick pick, Seq<Key> order) =>
            (Some<Grip>(pick with { Live = true }), (order == Items ? None : Some(reorder(new Edit<Seq<Key>>.Preview(order)))) | Some(Handled));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Cancelled(Delivery at) =>
            Held<Grip.Pick>(at.Grip).Match(
                Some: pick => (None, Joined(
                    pick.Live ? Reorder.Map(static reorder => reorder(new Edit<Seq<Key>>.Cancel())) : None,
                    pick.Picked == pick.Prior ? None : Some(Edited(new Edit<Selection>.Cancel()))) | Some(Handled)),
                None: () => Ignored(at));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Performed(Input.Perform perform, Delivery at) =>
            perform.Act.SwitchPartially(
                (Choice: this, At: at, Extend: perform.Modifiers.Shift),
                @default: static (held, _) => Ignored(held.At),
                press: Committed,
                confirm: Committed,
                cancel: static (held, _) => held.Choice.Cancelled(held.At),
                all: static (held, _) => (held.At.Grip, Changed(held.Choice.Value, held.Choice.Value.All(held.Choice.Items), held.Choice.Edited, static next => new Edit<Selection>.Step(next)) | Some(Handled)),
                step: static (held, step) => held.Choice.Led(held.At, held.Extend, held.Choice.Value.Lead.Match(
                    Some: lead => held.Choice.Tiles(held.At.Laid) switch {
                        var tiles => tiles.Find(tile => tile.Key == lead).Bind(origin =>
                            Routing.Toward(origin.Frame, tiles.Filter(tile => tile.Key != lead).Map(static tile => (Item: tile.Key, tile.Frame)), step.Along, int.Sign(step.Count))),
                    },
                    None: () => held.Choice.Items.Head)),
                extreme: static (held, extreme) => held.Choice.Led(held.At, held.Extend, extreme.Toward is Heading.Back ? held.Choice.Items.Head : held.Choice.Items.Last));

        private static (Option<Grip> Grip, Option<IO<Unit>> Answer) Committed((Choice Choice, Delivery At, bool Extend) held, Act _) =>
            (held.At.Grip, Some(held.Choice.Edited(new Edit<Selection>.Commit(held.Choice.Value))));

        private (Option<Grip> Grip, Option<IO<Unit>> Answer) Led(Delivery at, bool extend, Option<Key> target) =>
            target.Map(key => (at.Grip, Answer: Changed(Value, Value.Led(Items, key, extend), Edited, static next => new Edit<Selection>.Step(next)) | Some(Handled))).IfNone(() => Ignored(at));

        // --- [READ]
        internal override Cursor Cursor(Laid laid, Seq<Key> over, Option<Grip> grip) => Held<Grip.Pick>(grip).Exists(static pick => pick.Live) ? UI.Cursor.Move : UI.Cursor.Default;

        internal override Seq<(Seq<Key> Path, Accessible Accessible)> Access(Laid laid) =>
            laid.Node.Accessible.Map(static held => held.Role).IfNone(Value.Map(one: AccessRole.Group, many: AccessRole.List)) switch {
                var container => (laid.Path, Accessible.Declared(container, laid.Node.Accessible) with { Expanded = laid.Node.Accessible.Bind(static held => held.Expanded) }).Cons(Tiles(laid).Map(tile => (tile.Laid.Path, new Accessible(
                    (tile.Laid.Node.Accessible.Map(static held => held.Role) | Members.Find(container)).IfNone(AccessRole.Radio), Label(tile.Laid)) {
                    Value = new AccessValue.Toggled(Value.Holds(tile.Key)),
                    Expanded = tile.Laid.Node.Accessible.Bind(static held => held.Expanded),
                    Actions = [new Act.Press()],
                    Help = tile.Laid.Node.Accessible.Bind(static held => held.Help),
                }))),
            };

        internal override Option<Seq<Key>> Revealed(Laid laid) => Value.Lead.Bind(lead => Tiles(laid).Find(tile => tile.Key == lead)).Map(static tile => tile.Laid.Path);

        internal override Option<IO<Unit>> Watched(Option<Sight> before, Sight after) =>
            Joined(
                Hover.Bind(hover => Hovered(after) switch {
                    var now => before.Bind(Hovered) == now ? None : Some(hover(now)),
                }),
                Viewed.Bind(viewed => Seen(after) switch {
                    var now => before.Map(Seen) == Some(now) ? None : Some(viewed(now)),
                }));

        private Option<Key> Hovered(Sight sight) => sight.Hover.Bind(path => Item(path, sight.Laid));

        private LanguageExt.HashSet<Key> Seen(Sight sight) => toHashSet(Tiles(sight.Laid).Filter(tile => sight.InView(tile.Laid.Path)).Map(static tile => tile.Key));

        private Option<Key> Item(Seq<Key> over, Laid laid) =>
            laid.Children.Find(child => over.Take(child.Path.Count) == child.Path).Bind(child => Tile(child) | Item(over, child));

        private Seq<(Key Key, Laid Laid, SKRect Frame)> Tiles(Laid laid) => Tiles(laid, SKPoint.Empty);

        private Seq<(Key Key, Laid Laid, SKRect Frame)> Tiles(Laid laid, SKPoint origin) =>
            laid.Children.Bind(child => SKRect.Create(origin + child.Frame.Location, child.Frame.Size) switch {
                var frame => Tile(child).Match(Some: key => Seq((Key: key, Laid: child, Frame: frame)), None: () => Tiles(child, frame.Location)),
            });

        private Option<Key> Tile(Laid laid) => laid.Node.Key.Filter(key => Items.Exists(item => item == key));

        private Seq<Key> Ordered(Grip.Pick pick, Delivery at) =>
            (from target in Item(at.Over, at.Laid).Filter(key => !Moving(pick, key))
             from place in Selection.Position(Items, target)
             from first in pick.Moving.Head.Bind(key => Selection.Position(Items, key))
             let rest = Items.Filter(key => !Moving(pick, key))
             from slot in Selection.Position(rest, target)
             let index = place > first ? slot + 1 : slot
             select rest.Take(index) + pick.Moving + rest.Skip(index)).IfNone(Items);

        private static bool Moving(Grip.Pick pick, Key key) => pick.Moving.Exists(moving => moving == key);
    }

    public sealed record Field : Part {
        private Field(Func<Edit<string>, IO<Unit>> edited) => Edited = edited;

        public Func<Edit<string>, IO<Unit>> Edited { get; }

        internal override bool Edits => true;

        public static Node.Text Drawn(Node.Text text, Func<Edit<string>, IO<Unit>> edited) => text with { Part = new Field(edited) };

        internal override (Option<Grip> Grip, Option<IO<Unit>> Answer) Receive(Delivery delivery) =>
            (
                delivery.Input.SwitchPartially(
                    delivery.Grip,
                    @default: static (grip, _) => grip,
                    pointer: static (grip, pointer) => pointer.Phase switch {
                        PointerPhase.Down => Some<Grip>(new Grip.Held()),
                        PointerPhase.Up => None,
                        PointerPhase.Move or PointerPhase.Leave => grip,
                    },
                    focus: static (grip, focus) => focus.Gained ? grip : None),
                from editor in delivery.Editor
                from text in Optional(delivery.Laid as Laid.Text)
                from next in editor.Next(delivery.Input, text, delivery.Bounds, delivery.Grip.IsSome)
                select next.Bind(edits => edits.TraverseM(Edited).As()).Map(static _ => unit));

        internal override Cursor Cursor(Laid laid, Seq<Key> over, Option<Grip> grip) => UI.Cursor.Text;

        internal override Seq<(Seq<Key> Path, Accessible Accessible)> Access(Laid laid) =>
            [(laid.Path, Accessible.Declared(AccessRole.TextField, laid.Node.Accessible) with {
                Value = new AccessValue.Text(string.Concat(Texts(laid))),
                Actions = [new Act.Confirm(), new Act.Cancel()],
            })];
    }
}
