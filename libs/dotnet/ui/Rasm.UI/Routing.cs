using System.Numerics;
using LanguageExt.ClassInstances;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record Gesture {
    internal KeyModifiers Modifiers { get; init; } = KeyModifiers.Empty;

    internal virtual Option<Gesture> Accumulated(Gesture increment) => None;

    internal sealed record Press(SKPoint From, Option<SKPoint> Dragged, int Clicks) : Gesture {
        internal SKPoint At => Dragged.IfNone(From);
        internal SKRect Swept => new SKRect(From.X, From.Y, At.X, At.Y).Standardized;
        internal Seq<PointerSample> Trail { get; init; }
        internal Vector2 Scrolled { get; init; }
        internal bool Ended { get; init; }

        internal Press Updated(PointerSample sample, float slop) =>
            Dragged.IsSome || SKPoint.Distance(sample.At, From) >= slop ? this with { Dragged = Some(sample.At), Trail = Trail.Add(sample) } : this;

        internal Option<Figure.Region> Enclosed(PlotFrame frame) =>
            from start in frame.At(From)
            from end in frame.At(At)
            where Dragged.IsSome
            select new Figure.Region { First = Spanned(frame.Plane, PhysicalAxis.Horizontal, start, end), Second = Spanned(frame.Plane, PhysicalAxis.Vertical, start, end) };

        private static Option<AxisRange> Spanned(PlotPlane plane, PhysicalAxis along, Vector2 start, Vector2 end) =>
            plane.Dimension(along).Bind(_ => AxisRange.Validated(along.Of(start), along.Of(end)).ToOption());
    }

    internal sealed record Scroll(SKPoint At, Vector2 Delta) : Gesture {
        internal override Option<Gesture> Accumulated(Gesture increment) => Optional(increment as Scroll).Map(next => (Gesture)(next with { Delta = Delta + next.Delta }));
    }

    internal sealed record Magnify(SKPoint About, float Magnification) : Gesture {
        internal override Option<Gesture> Accumulated(Gesture increment) => Optional(increment as Magnify).Map(next => (Gesture)(next with { Magnification = ((1f + Magnification) * (1f + next.Magnification)) - 1f }));
    }

    internal sealed record Rotate(float Rotation) : Gesture {
        internal override Option<Gesture> Accumulated(Gesture increment) => Optional(increment as Rotate).Map(next => (Gesture)(next with { Rotation = Rotation + next.Rotation }));
    }

    internal sealed record Perform(Act Act) : Gesture;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PointerCapture {
    private PointerCapture(Seq<Key> owner, Seq<Key> hit) => (Owner, Hit) = (owner, hit);

    internal Seq<Key> Owner { get; }
    internal Seq<Key> Hit { get; }

    internal virtual Seq<IO<Unit>> Cancelled => [];

    internal bool Live(Scene scene) => Switch(scene, part: static (held, part) => held.EnabledPartAt(part.Owner).IsSome, handle: static (held, handle) => held.Scroller(handle.Owner).IsSome);

    internal sealed record Part(Seq<Key> Owner, Seq<Key> Hit, Gesture.Press Press, Track Track) : PointerCapture(Owner, Hit) {
        internal override Seq<IO<Unit>> Cancelled => Track.Cancelled();
    }

    internal sealed record Handle(Seq<Key> Owner, SKPoint From, float Start) : PointerCapture(Owner, Owner);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Latch {
    private Latch(Seq<Key> owner) => Owner = owner;

    internal Seq<Key> Owner { get; }

    internal virtual Seq<IO<Unit>> Cancelled => [];

    internal bool Live(Scene scene) => Switch(scene, part: static (held, part) => held.EnabledPartAt(part.Owner).IsSome, scroll: static (held, scroll) => held.Scroller(scroll.Owner).IsSome);

    internal sealed record Part(Seq<Key> Owner, Option<(Gesture Gesture, Track Track)> Active) : Latch(Owner) {
        internal override Seq<IO<Unit>> Cancelled => Active.Map(static live => live.Track.Cancelled()).IfNone([]);
    }

    internal sealed record Scroll(Seq<Key> Owner, Option<Gesture.Scroll> Active) : Latch(Owner);
}

internal sealed record Focus(Seq<Key> Element, bool Traversed);

internal sealed record Routing {
    // --- [STATE]
    internal static Routing Idle { get; } = new();

    internal Option<SKPoint> Pointer { get; init; }
    internal KeyModifiers Modifiers { get; init; } = KeyModifiers.Empty;
    internal Option<PointerCapture> Capture { get; init; }
    internal Option<Latch> Latch { get; init; }
    internal Option<Focus> Focus { get; init; }

    // --- [DERIVED]
    internal Option<Seq<Key>> Pressed => Capture.Map(static capture => capture.Hit);

    internal Option<PointerCapture.Part> Part => Capture.Bind(static capture => Optional(capture as PointerCapture.Part));

    internal Option<Track> ActiveTrack => Part.Map(static part => part.Track);

    // --- [FOCUS]
    private const float AlignWeight = 5f;

    internal static Seq<Seq<Element>> Stops(Seq<Element> tree) => tree.Bind(Stopped);

    internal static Option<Seq<Key>> Traversed(Seq<Seq<Element>> stops, Option<Seq<Key>> focused, int sign, bool cycles) =>
        from target in focused.Bind(focus => stops.Map(static (stop, index) => (Stop: stop, Index: index)).Find(held => held.Stop.Exists(element => element.Id == focus))).Map(held => held.Index + sign)
                       | (stops.IsEmpty ? None : Some(sign > 0 ? 0 : stops.Count - 1))
        where cycles || (target >= 0 && target < stops.Count)
        from stop in stops.At(((target % stops.Count) + stops.Count) % stops.Count)
        from entry in stop.Find(static element => element.Conditions.Contains(Condition.Selected)) | (sign > 0 ? stop.Head : stop.Last)
        select entry.Id;

    internal static Option<T> Toward<T>(SKRect origin, Seq<(T Item, SKRect Frame)> candidates, Bearing toward) =>
        candidates.Choose(candidate => Distance(origin, candidate.Frame, toward).Map(distance => (candidate.Item, Distance: distance))) switch {
            { IsEmpty: true } => None,
            var ahead => Some(ahead.MinBy(static held => held.Distance).Item),
        };

    private static Seq<Seq<Element>> Stopped(Element element) =>
        element.Role.Composite
            ? Access.Flattened(element.Children).Filter(Focusable) switch {
                { IsEmpty: true } => Focusable(element) ? [[element]] : [],
                var members => [members],
            }
            : (Focusable(element) ? Seq<Seq<Element>>([element]) : []) + element.Children.Bind(Stopped);

    private static bool Focusable(Element element) =>
        !element.Conditions.Contains(Condition.Disabled) && !element.Actions.IsEmpty;

    private static Option<float> Distance(SKRect origin, SKRect candidate, Bearing toward) {
        SKMatrix facing = toward.Along.Map(horizontal: new SKMatrix(toward.Sign, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f), vertical: new SKMatrix(0f, toward.Sign, 0f, 1f, 0f, 0f, 0f, 0f, 1f));
        SKRect exit = facing.MapRect(origin);
        SKRect entry = facing.MapRect(candidate);
        float overlap = float.Min(exit.Right, entry.Right) - float.Max(exit.Left, entry.Left);
        float aligned = float.Min(exit.Bottom, entry.Bottom) - float.Max(exit.Top, entry.Top);
        float gap = float.Max(0f, float.Max(entry.Top - exit.Bottom, exit.Top - entry.Bottom));
        return entry.Left >= exit.Right || (entry.Left >= exit.Left && entry.Right > exit.Right && aligned > 0f)
            ? Some(float.Hypot(float.Max(0f, entry.Left - exit.Right), gap)
                + ((gap + (aligned > 0f ? 0f : exit.Height / 2f)) * toward.Along.Map(horizontal: 30f, vertical: 2f))
                - (aligned > 0f ? AlignWeight * float.Min(aligned / exit.Height, 1f) : 0f)
                - (overlap > 0f && aligned > 0f ? MathF.Sqrt(overlap * aligned) : 0f))
            : None;
    }
}

internal sealed record Reply(Seq<IO<Unit>> Effects, Option<Track> Track);

internal sealed record Track(Func<Gesture, Reply> Moved, Func<Gesture, Seq<IO<Unit>>> Released, Func<Seq<IO<Unit>>> Cancelled) {
    internal CursorShape Cursor { get; init; } = CursorShape.Default;
    internal Option<Vector2> Detent { get; init; }
    internal Option<Figure> Preview { get; init; }
    internal Option<(Transfer.Keys Keys, IO<Transfer> Payload)> Lifted { get; init; }
}

internal sealed record Receiver<TState>(Func<TState, TState, bool> Same, Func<Edit<TState>, Option<IO<Unit>>> Emit) where TState : notnull {
    internal static Receiver<TState> Of<TOut>(Func<TState, TOut> project, Func<Edit<TOut>, Option<IO<Unit>>> edited) where TOut : notnull =>
        new((before, after) => EqDefault<TOut>.Equals(project(before), project(after)), edit => edited(edit.Map(project).As()));
}

internal sealed record Session<TState>(TState Start, Seq<Receiver<TState>> Receivers, CursorShape Cursor) where TState : notnull {
    // --- [STATE]
    internal Func<TState, Gesture, Option<TState>> Step { get; init; } = static (_, _) => None;
    internal Func<TState, TState> Released { get; init; } = static state => state;
    internal Func<TState, Option<Vector2>> Detent { get; init; } = static _ => None;
    internal Func<TState, Option<Figure>> Preview { get; init; } = static _ => None;
    internal Func<TState, Option<(Transfer.Keys Keys, IO<Transfer> Payload)>> Lifted { get; init; } = static _ => None;

    private TState Last { get; init; } = Start;
    private bool Dragging { get; init; }
    private HashSet<int> Opened { get; init; }

    // --- [EMIT]
    internal Option<Reply> Began(Gesture first) => Step(Start, first).Map(state => Continued(state, Drags(first)));

    internal Option<Reply> Stepped(Gesture gesture) => Stepped(start => Step(start, gesture));

    internal Option<Reply> Stepped(Func<TState, Option<TState>> next) =>
        next(Start).Map(state => new Reply(Receivers.Filter(receiver => !receiver.Same(Start, state)).Choose(receiver => receiver.Emit(Edit.Of(EditPhase.Step, state))), None));

    private Seq<(Receiver<TState> Receiver, int Index)> Indexed => Receivers.Map(static (receiver, index) => (Receiver: receiver, Index: index));

    private Track Tracked =>
        new(gesture => Step(Start, gesture).Match(Some: state => Continued(state, Drags(gesture)), None: () => Continued(Last, Dragging)), gesture => Finished(Released(Step(Start, gesture).IfNone(Last))),
            () => Indexed.Filter(held => Opened.Contains(held.Index)).Choose(static held => held.Receiver.Emit(Edit.Cancel<TState>()))) {
            Cursor = Dragging ? Cursor.Dragged : Cursor,
            Detent = Detent(Last),
            Preview = Preview(Last),
            Lifted = Dragging ? Lifted(Last) : None,
        };

    private Reply Continued(TState state, bool dragging) {
        Seq<(int Index, IO<Unit> Effect)> moved = Indexed.Filter(held => !held.Receiver.Same(Last, state))
            .Choose(held => held.Receiver.Emit(Edit.Of(Opened.Contains(held.Index) ? EditPhase.Preview : EditPhase.Begin, state)).Map(effect => (held.Index, effect)));
        return new(moved.Map(static held => held.Effect), Some((this with { Last = state, Dragging = dragging, Opened = Opened.Union(moved.Map(static held => held.Index)) }).Tracked));
    }

    private Seq<IO<Unit>> Finished(TState state) =>
        Indexed.Choose(held =>
            !held.Receiver.Same(Start, state) ? held.Receiver.Emit(Edit.Of(EditPhase.Commit, state))
            : Opened.Contains(held.Index) ? held.Receiver.Emit(Edit.Cancel<TState>())
            : None);

    private static bool Drags(Gesture gesture) => gesture is Gesture.Press { Dragged.IsSome: true };
}

internal sealed record HitTarget(Scene Scene, LayoutNode Layout, SKRect Bounds, Seq<Key> Hit) {
    internal Option<PlotFrame> Frame => Layouts.Plotted(Layout).Map(frame => frame.Translated(Bounds.Location));
}

internal sealed record HoverState(Scene Scene, LayoutNode Layout, Seq<Seq<Key>> Hover, Option<SKPoint> Pointer) {
    internal Option<Vector2> Plotted =>
        from pointer in Pointer
        where Hover.Exists(path => path.Take(Layout.Path.Count) == Layout.Path)
        from bounds in Scene.Frame(Layout.Path)
        from frame in Layouts.Plotted(Layout)
        from at in frame.Translated(bounds.Location).At(pointer)
        select at;
}
