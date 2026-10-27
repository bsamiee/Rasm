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

    public TSelf Folded(SKPoint at, TIncrement increment, KeyModifiers modifiers);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record Gesture {
    internal KeyModifiers Modifiers { get; init; } = KeyModifiers.Plain;

    internal sealed record Press(SKPoint From, Option<SKPoint> Dragged, int Clicks, float Pressure) : Gesture {
        internal SKPoint At => Dragged.IfNone(From);
        internal Vector2 Velocity { get; init; }
        internal Vector2 Scrolled { get; init; }

        internal Press Followed(PointerSample sample, float slop) =>
            this with { Pressure = sample.Pressure, Dragged = Dragged.IsSome || SKPoint.Distance(sample.At, From) >= slop ? Some(sample.At) : Dragged };
    }

    internal sealed record Scroll(SKPoint At, Vector2 Travel) : Gesture, IStreamed<Scroll, Vector2> {
        static Scroll IStreamed<Scroll, Vector2>.Of(SKPoint at, Vector2 increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        Scroll IStreamed<Scroll, Vector2>.Folded(SKPoint at, Vector2 increment, KeyModifiers modifiers) => this with { At = at, Travel = Travel + increment, Modifiers = modifiers };
    }

    internal sealed record Magnify(SKPoint About, float Magnification) : Gesture, IStreamed<Magnify, float> {
        static Magnify IStreamed<Magnify, float>.Of(SKPoint at, float increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        Magnify IStreamed<Magnify, float>.Folded(SKPoint at, float increment, KeyModifiers modifiers) =>
            this with { About = at, Magnification = ((1f + Magnification) * (1f + increment)) - 1f, Modifiers = modifiers };
    }

    internal sealed record Rotate(SKPoint About, float Rotation) : Gesture, IStreamed<Rotate, float> {
        static Rotate IStreamed<Rotate, float>.Of(SKPoint at, float increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        Rotate IStreamed<Rotate, float>.Folded(SKPoint at, float increment, KeyModifiers modifiers) => this with { About = at, Rotation = Rotation + increment, Modifiers = modifiers };
    }

    internal sealed record Perform(Act Act) : Gesture;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PointerCapture {
    internal sealed record Part(Seq<Key> Owner, Seq<Key> Over, Gesture.Press Press, Track Track, Duration Time) : PointerCapture;

    internal sealed record Handle(Seq<Key> Adjuster, SKPoint From, float Start) : PointerCapture;
}

internal sealed record LiveGesture(Gesture Gesture, Track Track);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Latch {
    internal sealed record Part(Seq<Key> Owner, Option<LiveGesture> Live) : Latch;

    internal sealed record Scroll(Seq<Key> Adjuster, Option<Gesture.Scroll> Live) : Latch;
}

internal sealed record Focus(Seq<Key> Element, bool Traversed);

internal sealed record Candidate<T>(T Item, SKRect Frame);

internal sealed record Routing {
    // --- [STATE]
    internal static Routing Idle { get; } = new();

    internal Option<SKPoint> Pointer { get; init; }
    internal KeyModifiers Modifiers { get; init; } = KeyModifiers.Plain;
    internal Option<PointerCapture> Capture { get; init; }
    internal Option<Latch> Latch { get; init; }
    internal Option<Focus> Focus { get; init; }

    // --- [DERIVED]
    internal Option<Seq<Key>> Pressing => Capture.Map(static capture => capture.Switch(part: static part => part.Over, handle: static handle => handle.Adjuster));

    internal Option<Track> Tracked => Capture.Bind(static capture => capture.Switch(part: static part => Some(part.Track), handle: static _ => Option<Track>.None));

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

    private static Stretch Spanned(SKRect frame, PhysicalAxis along, int sign) {
        Stretch span = new(along.Of(frame.Location), along.Of(new SKPoint(frame.Right, frame.Bottom)));
        return sign > 0 ? span : new Stretch(-span.High, -span.Low);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Reply {
    internal static Reply Passed<TState, TGesture>(TState _, TGesture __) => new Pass();

    internal sealed record Pass : Reply;

    internal sealed record Take(Seq<IO<Unit>> Effects, Option<Track> Track) : Reply;
}

internal sealed record Track(Func<Gesture, Reply> Moved, Func<Gesture, Seq<IO<Unit>>> Released, Func<Seq<IO<Unit>>> Cancelled) {
    internal CursorShape Cursor { get; init; } = CursorShape.Default;
    internal Option<Vector2> Detent { get; init; }
    internal Seq<Key> Dragged { get; init; }
}

internal sealed record Receiver<TState>(Func<TState, TState, bool> Same, Func<Edit<TState>, Option<IO<Unit>>> Emit) where TState : notnull {
    internal static Receiver<TState> Of<TOut>(Func<TState, TOut> project, Func<Edit<TOut>, Option<IO<Unit>>> edited) where TOut : notnull =>
        new((before, after) => EqDefault<TOut>.Equals(project(before), project(after)), edit => edited(edit.Map(project).As()));
}

internal sealed record Session<TState>(TState Start, Seq<Receiver<TState>> Receivers, CursorShape Cursor) where TState : notnull {
    // --- [STATE]
    internal Func<TState, Gesture, Option<TState>> Step { get; init; } = static (_, _) => None;
    internal Func<TState, Gesture.Press, Option<Seq<IO<Unit>>>> Exit { get; init; } = static (_, _) => None;
    internal Option<CursorShape> Dragging { get; init; }
    internal Func<TState, Option<Vector2>> Detent { get; init; } = static _ => None;
    internal Func<TState, Seq<Key>> Dragged { get; init; } = static _ => [];

    // --- [EMIT]
    internal Reply Began(Gesture first) =>
        Step(Start, first).Match(
            Some: state => {
                Seq<Preview> opened = Previewed(Start, state);
                return (Reply)new Reply.Take(opened.Map(static held => held.Effect), Some(Tracked(new Run(Start, state, first, toHashSet(opened.Map(static held => held.Index))))));
            },
            None: static () => new Reply.Pass());

    internal Reply Stepped(Gesture gesture) =>
        Step(Start, gesture).Match(
            Some: state => (Reply)new Reply.Take(Receivers.Filter(receiver => !receiver.Same(Start, state)).Choose(receiver => receiver.Emit(Edit.Step(state))), None),
            None: static () => new Reply.Pass());

    private sealed record Run(TState Origin, TState Last, Gesture Latest, HashSet<int> Opened);

    private sealed record Numbered(Receiver<TState> Receiver, int Index);

    private sealed record Preview(int Index, IO<Unit> Effect);

    private Seq<Numbered> Indexed => Receivers.Map(static (receiver, index) => new Numbered(receiver, index));

    private Seq<Preview> Previewed(TState from, TState to) =>
        Indexed.Filter(held => !held.Receiver.Same(from, to)).Choose(held => held.Receiver.Emit(Edit.Preview(to)).Map(effect => new Preview(held.Index, effect)));

    private Track Tracked(Run run) =>
        new(gesture => Followed(run, gesture), gesture => Finished(run, Step(Start, gesture).IfNone(run.Last)), () => Cancelled(run)) {
            Cursor = run.Latest.SwitchPartially(
                this,
                @default: static (session, _) => session.Cursor,
                press: static (session, press) => press.Dragged.IsSome ? session.Dragging.IfNone(session.Cursor) : session.Cursor),
            Detent = Detent(run.Last),
            Dragged = Dragged(run.Last),
        };

    private Reply Followed(Run run, Gesture gesture) {
        (TState State, Gesture Latest) next = Step(Start, gesture).Match(Some: state => (state, gesture), None: () => (run.Last, run.Latest));
        return gesture.SwitchPartially(
                (Session: this, next.State),
                @default: static (_, _) => Option<Seq<IO<Unit>>>.None,
                press: static (held, press) => held.Session.Exit(held.State, press))
            .Match(
                Some: extra => (Reply)new Reply.Take(Finished(run, next.State) + extra, None),
                None: () => Continued(run, next.State, next.Latest));
    }

    private Reply.Take Continued(Run run, TState state, Gesture latest) {
        Seq<Preview> moved = Previewed(run.Last, state);
        return new(moved.Map(static held => held.Effect), Some(Tracked(run with { Last = state, Latest = latest, Opened = run.Opened.Union(moved.Map(static held => held.Index)) })));
    }

    private Seq<IO<Unit>> Finished(Run run, TState state) =>
        Indexed.Choose(held =>
            !held.Receiver.Same(run.Origin, state) ? held.Receiver.Emit(Edit.Commit(state))
            : run.Opened.Contains(held.Index) ? held.Receiver.Emit(Edit.Cancel<TState>())
            : None);

    private Seq<IO<Unit>> Cancelled(Run run) =>
        Indexed.Filter(held => run.Opened.Contains(held.Index)).Choose(static held => held.Receiver.Emit(Edit.Cancel<TState>()));
}

internal sealed record Aim(Scene Scene, Laid Laid, SKRect Bounds, Seq<Key> Over);

internal sealed record Sight(Scene Scene, Laid Laid, Seq<Seq<Key>> Hover, Option<SKPoint> Pointer) {
    internal bool InView(Seq<Key> path) => Scene.Bounds(path).Exists(static bounds => !bounds.IsEmpty);

    internal Option<Vector2> Plotted =>
        from pointer in Pointer
        where Hover.Exists(path => path.Take(Laid.Path.Count) == Laid.Path)
        from bounds in Scene.Frame(Laid.Path)
        from frame in Layouts.Plotted(Laid)
        from at in frame.Translated(bounds.Location).At(pointer)
        select at;
}
