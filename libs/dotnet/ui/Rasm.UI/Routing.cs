using System.Numerics;
using LanguageExt.ClassInstances;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IStreamed<TSelf, TIncrement> where TSelf : Gesture, IStreamed<TSelf, TIncrement> where TIncrement : notnull {
    public static abstract TSelf Of(SKPoint at, TIncrement increment, KeyModifiers modifiers);

    public TSelf Accumulated(SKPoint at, TIncrement increment, KeyModifiers modifiers);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record Gesture {
    internal KeyModifiers Modifiers { get; init; } = KeyModifiers.Empty;

    internal sealed record Press(SKPoint From, Option<SKPoint> Dragged, int Clicks) : Gesture {
        internal SKPoint At => Dragged.IfNone(From);
        internal Seq<PointerSample> Trail { get; init; }
        internal Option<Stylus> Stylus => Trail.Last.Bind(static sample => sample.Stylus);
        internal Vector2 Velocity { get; init; }
        internal Vector2 Scrolled { get; init; }

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

    internal sealed record Scroll(SKPoint At, Vector2 Delta) : Gesture, IStreamed<Scroll, Vector2> {
        static Scroll IStreamed<Scroll, Vector2>.Of(SKPoint at, Vector2 increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        Scroll IStreamed<Scroll, Vector2>.Accumulated(SKPoint at, Vector2 increment, KeyModifiers modifiers) => this with { At = at, Delta = Delta + increment, Modifiers = modifiers };
    }

    internal sealed record Magnify(SKPoint About, float Magnification) : Gesture, IStreamed<Magnify, float> {
        static Magnify IStreamed<Magnify, float>.Of(SKPoint at, float increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        Magnify IStreamed<Magnify, float>.Accumulated(SKPoint at, float increment, KeyModifiers modifiers) =>
            this with { About = at, Magnification = ((1f + Magnification) * (1f + increment)) - 1f, Modifiers = modifiers };
    }

    internal sealed record Rotate(SKPoint About, float Rotation) : Gesture, IStreamed<Rotate, float> {
        static Rotate IStreamed<Rotate, float>.Of(SKPoint at, float increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        Rotate IStreamed<Rotate, float>.Accumulated(SKPoint at, float increment, KeyModifiers modifiers) => this with { About = at, Rotation = Rotation + increment, Modifiers = modifiers };
    }

    internal sealed record Perform(Act Act) : Gesture;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PointerCapture {
    internal sealed record Part(Seq<Key> Owner, Seq<Key> Hit, Gesture.Press Press, Track Track, Duration Time) : PointerCapture;

    internal sealed record Handle(Seq<Key> Adjuster, SKPoint From, float Start) : PointerCapture;
}

internal sealed record ActiveGesture(Gesture Gesture, Track Track);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Latch {
    internal sealed record Part(Seq<Key> Owner, Option<ActiveGesture> Active) : Latch;

    internal sealed record Scroll(Seq<Key> Adjuster, Option<Gesture.Scroll> Active) : Latch;
}

internal sealed record Focus(Seq<Key> Element, bool Traversed);

internal sealed record Candidate<T>(T Item, SKRect Frame);

internal sealed record Routing {
    // --- [STATE]
    internal static Routing Idle { get; } = new();

    internal Option<SKPoint> Pointer { get; init; }
    internal KeyModifiers Modifiers { get; init; } = KeyModifiers.Empty;
    internal Option<PointerCapture> Capture { get; init; }
    internal Option<Latch> Latch { get; init; }
    internal Option<Focus> Focus { get; init; }

    // --- [DERIVED]
    internal Option<Seq<Key>> Pressed => Capture.Map(static capture => capture.Switch(part: static part => part.Hit, handle: static handle => handle.Adjuster));

    internal Option<Track> ActiveTrack => Capture.Bind(static capture => capture.Switch(part: static part => Some(part.Track), handle: static _ => Option<Track>.None));

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

    internal static Option<T> Toward<T>(SKRect origin, Seq<Candidate<T>> candidates, NavigationDirection toward) =>
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

    private static Option<float> Distance(SKRect origin, SKRect candidate, NavigationDirection toward) {
        (float exitLow, float exitHigh) = Spanned(origin, toward.Along, toward.Sign);
        (float entryLow, float entryHigh) = Spanned(candidate, toward.Along, toward.Sign);
        (float originLow, float originHigh) = Spanned(origin, toward.Along.Cross, 1);
        (float candidateLow, float candidateHigh) = Spanned(candidate, toward.Along.Cross, 1);
        float overlap = float.Min(exitHigh, entryHigh) - float.Max(exitLow, entryLow);
        float aligned = float.Min(originHigh, candidateHigh) - float.Max(originLow, candidateLow);
        float gap = float.Max(0f, float.Max(candidateLow - originHigh, originLow - candidateHigh));
        float extent = originHigh - originLow;
        return entryLow >= exitHigh || (entryLow >= exitLow && entryHigh > exitHigh && aligned > 0f)
            ? Some(float.Hypot(float.Max(0f, entryLow - exitHigh), gap)
                + ((gap + (aligned > 0f ? 0f : extent / 2f)) * toward.Along.Map(horizontal: 30f, vertical: 2f))
                - (aligned > 0f ? AlignWeight * float.Min(aligned / extent, 1f) : 0f)
                - (overlap > 0f && aligned > 0f ? MathF.Sqrt(overlap * aligned) : 0f))
            : None;
    }

    private static FractionRange Spanned(SKRect frame, PhysicalAxis along, int sign) {
        FractionRange span = new(along.Of(frame.Location), along.Of(new SKPoint(frame.Right, frame.Bottom)));
        return sign > 0 ? span : new FractionRange(-span.High, -span.Low);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Reply {
    internal static Reply Ignore<TState, TGesture>(TState _, TGesture __) => new Ignored();

    internal sealed record Ignored : Reply;

    internal sealed record Accepted(Seq<IO<Unit>> Effects, Option<Track> Track) : Reply;
}

internal sealed record Track(Func<Gesture, Reply> Moved, Func<Gesture, Seq<IO<Unit>>> Released, Func<Seq<IO<Unit>>> Cancelled) {
    internal CursorShape Cursor { get; init; } = CursorShape.Default;
    internal Option<Vector2> Detent { get; init; }
    internal Option<Figure> Preview { get; init; }
    internal Option<IO<Transfer>> Lifted { get; init; }
}

internal sealed record Receiver<TState>(Func<TState, TState, bool> Same, Func<Edit<TState>, Option<IO<Unit>>> Emit) where TState : notnull {
    internal static Receiver<TState> Of<TOut>(Func<TState, TOut> project, Func<Edit<TOut>, Option<IO<Unit>>> edited) where TOut : notnull =>
        new((before, after) => EqDefault<TOut>.Equals(project(before), project(after)), edit => edited(edit.Map(project).As()));
}

internal sealed record Session<TState>(TState Start, Seq<Receiver<TState>> Receivers, CursorShape Cursor) where TState : notnull {
    // --- [STATE]
    internal Func<TState, Gesture, Option<TState>> Step { get; init; } = static (_, _) => None;
    internal Func<TState, TState> Released { get; init; } = static state => state;
    internal Option<CursorShape> Dragging { get; init; }
    internal Func<TState, Option<Vector2>> Detent { get; init; } = static _ => None;
    internal Func<TState, Option<Figure>> Preview { get; init; } = static _ => None;
    internal Func<TState, Option<IO<Transfer>>> Lifted { get; init; } = static _ => None;

    // --- [EMIT]
    internal Reply Began(Gesture first) =>
        Step(Start, first).Match(
            Some: state => {
                Seq<Emitted> opened = Previewed(Start, state);
                return (Reply)new Reply.Accepted(opened.Map(static held => held.Effect), Some(Tracked(new Drag(Start, state, first, toHashSet(opened.Map(static held => held.Index))))));
            },
            None: static () => new Reply.Ignored());

    internal Reply Stepped(Gesture gesture) =>
        Step(Start, gesture).Match(
            Some: state => (Reply)new Reply.Accepted(Receivers.Filter(receiver => !receiver.Same(Start, state)).Choose(receiver => receiver.Emit(Edit.Step(state))), None),
            None: static () => new Reply.Ignored());

    private sealed record Drag(TState Origin, TState Last, Gesture Latest, HashSet<int> Opened);

    private sealed record IndexedReceiver(Receiver<TState> Receiver, int Index);

    private sealed record Emitted(int Index, IO<Unit> Effect);

    private Seq<IndexedReceiver> Indexed => Receivers.Map(static (receiver, index) => new IndexedReceiver(receiver, index));

    private Seq<Emitted> Previewed(TState from, TState to) =>
        Indexed.Filter(held => !held.Receiver.Same(from, to)).Choose(held => held.Receiver.Emit(Edit.Preview(to)).Map(effect => new Emitted(held.Index, effect)));

    private Track Tracked(Drag run) =>
        new(gesture => Moved(run, gesture), gesture => Finished(run, Released(Step(Start, gesture).IfNone(run.Last))), () => Cancelled(run)) {
            Cursor = run.Latest.SwitchPartially(
                this,
                @default: static (session, _) => session.Cursor,
                press: static (session, press) => press.Dragged.IsSome ? session.Dragging.IfNone(session.Cursor) : session.Cursor),
            Detent = Detent(run.Last),
            Preview = Preview(run.Last),
            Lifted = run.Latest is Gesture.Press { Dragged.IsSome: true } ? Lifted(run.Last) : None,
        };

    private Reply.Accepted Moved(Drag run, Gesture gesture) =>
        Step(Start, gesture).Match(Some: state => Continued(run, state, gesture), None: () => Continued(run, run.Last, run.Latest));

    private Reply.Accepted Continued(Drag run, TState state, Gesture latest) {
        Seq<Emitted> moved = Previewed(run.Last, state);
        return new(moved.Map(static held => held.Effect), Some(Tracked(run with { Last = state, Latest = latest, Opened = run.Opened.Union(moved.Map(static held => held.Index)) })));
    }

    private Seq<IO<Unit>> Finished(Drag run, TState state) =>
        Indexed.Choose(held =>
            !held.Receiver.Same(run.Origin, state) ? held.Receiver.Emit(Edit.Commit(state))
            : run.Opened.Contains(held.Index) ? held.Receiver.Emit(Edit.Cancel<TState>())
            : None);

    private Seq<IO<Unit>> Cancelled(Drag run) =>
        Indexed.Filter(held => run.Opened.Contains(held.Index)).Choose(static held => held.Receiver.Emit(Edit.Cancel<TState>()));
}

internal sealed record HitTarget(Scene Scene, LayoutNode Layout, SKRect Bounds, Seq<Key> Hit) {
    internal Option<PlotFrame> Frame => Layouts.Plotted(Layout).Map(frame => frame.Translated(Bounds.Location));
}

internal sealed record HoverState(Scene Scene, LayoutNode Layout, Seq<Seq<Key>> Hover, Option<SKPoint> Pointer) {
    internal bool InView(Seq<Key> path) => Scene.Bounds(path).Exists(static bounds => !bounds.IsEmpty);

    internal Option<Vector2> Plotted =>
        from pointer in Pointer
        where Hover.Exists(path => path.Take(Layout.Path.Count) == Layout.Path)
        from bounds in Scene.Frame(Layout.Path)
        from frame in Layouts.Plotted(Layout)
        from at in frame.Translated(bounds.Location).At(pointer)
        select at;
}
