using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IStreamed<TSelf, TIncrement> where TSelf : Gesture, IStreamed<TSelf, TIncrement> where TIncrement : notnull {
    public static abstract TSelf Of(SKPoint at, TIncrement increment, LanguageExt.HashSet<Modifier> modifiers);

    public static abstract Option<TSelf> From(Gesture gesture);

    public TSelf Folded(SKPoint at, TIncrement increment, LanguageExt.HashSet<Modifier> modifiers);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Gesture {
    public LanguageExt.HashSet<Modifier> Modifiers { get; init; }

    public sealed record Press(SKPoint From, Option<SKPoint> Dragged, int Clicks, float Pressure) : Gesture {
        public SKPoint At => Dragged.IfNone(From);
        public Vector2 Velocity { get; init; }
        public Vector2 Scrolled { get; init; }

        public Press Followed(PointerSample sample, float slop) =>
            this with { Pressure = sample.Pressure, Dragged = Dragged.IsSome || SKPoint.Distance(sample.At, From) >= slop ? Some(sample.At) : Dragged };
    }

    public sealed record Scroll(SKPoint At, Vector2 Travel) : Gesture, IStreamed<Scroll, Vector2> {
        public static Scroll Of(SKPoint at, Vector2 increment, LanguageExt.HashSet<Modifier> modifiers) => new(at, increment) { Modifiers = modifiers };

        public static Option<Scroll> From(Gesture gesture) =>
            gesture.Switch(press: static _ => None, scroll: static scroll => Some(scroll), magnify: static _ => None, rotate: static _ => None, perform: static _ => None);

        public Scroll Folded(SKPoint at, Vector2 increment, LanguageExt.HashSet<Modifier> modifiers) => this with { At = at, Travel = Travel + increment, Modifiers = modifiers };
    }

    public sealed record Magnify(SKPoint About, float Magnification) : Gesture, IStreamed<Magnify, float> {
        public static Magnify Of(SKPoint at, float increment, LanguageExt.HashSet<Modifier> modifiers) => new(at, increment) { Modifiers = modifiers };

        public static Option<Magnify> From(Gesture gesture) =>
            gesture.Switch(press: static _ => None, scroll: static _ => None, magnify: static magnify => Some(magnify), rotate: static _ => None, perform: static _ => None);

        public Magnify Folded(SKPoint at, float increment, LanguageExt.HashSet<Modifier> modifiers) =>
            this with { About = at, Magnification = ((1f + Magnification) * (1f + increment)) - 1f, Modifiers = modifiers };
    }

    public sealed record Rotate(SKPoint About, float Rotation) : Gesture, IStreamed<Rotate, float> {
        public static Rotate Of(SKPoint at, float increment, LanguageExt.HashSet<Modifier> modifiers) => new(at, increment) { Modifiers = modifiers };

        public static Option<Rotate> From(Gesture gesture) =>
            gesture.Switch(press: static _ => None, scroll: static _ => None, magnify: static _ => None, rotate: static rotate => Some(rotate), perform: static _ => None);

        public Rotate Folded(SKPoint at, float increment, LanguageExt.HashSet<Modifier> modifiers) => this with { About = at, Rotation = Rotation + increment, Modifiers = modifiers };
    }

    public sealed record Perform(Act Act) : Gesture;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Capture {
    public sealed record Part(Seq<Key> Owner, Seq<Key> Over, Gesture.Press Press, Track Track, Duration Time) : Capture;

    public sealed record Handle(Seq<Key> Adjuster, SKPoint From, float Start) : Capture;
}

internal sealed record Stream(Gesture Gesture, Track Track);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Latch {
    public sealed record Part(Seq<Key> Owner, Option<Stream> Live) : Latch;

    public sealed record Scroll(Seq<Key> Adjuster, Option<Gesture.Scroll> Live) : Latch;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Focus {
    private Focus(Seq<Key> element) => Element = element;

    public Seq<Key> Element { get; init; }
    public Option<Editor> Editor { get; init; }

    public bool Visible(HashMap<Seq<Key>, Element> elements) =>
        Switch(elements, pressed: static (held, pressed) => held.Find(pressed.Element).Exists(static element => element.Role == AccessRole.TextBox), traversed: static (_, _) => true);

    public sealed record Pressed(Seq<Key> Element) : Focus(Element);

    public sealed record Traversed(Seq<Key> Element) : Focus(Element);
}

internal sealed record Routing(Option<SKPoint> Pointer, LanguageExt.HashSet<Modifier> Modifiers, Option<Capture> Capture, Option<Latch> Latch, Option<Focus> Focus) {
    // --- [STATE]
    public static Routing Idle { get; } = new(None, [], None, None, None);

    // --- [DERIVED]
    public Option<Seq<Key>> Pressing => Capture.Map(static capture => capture.Switch(part: static part => part.Over, handle: static handle => handle.Adjuster));

    public Option<Track> Tracked => Capture.Bind(static capture => capture.Switch(part: static part => Some(part.Track), handle: static _ => Option<Track>.None));

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

    internal static Option<T> Toward<T>(SKRect origin, Seq<(T Item, SKRect Frame)> candidates, Navigation toward) =>
        candidates.Choose(candidate => Distance(origin, candidate.Frame, toward).Map(distance => (candidate.Item, Distance: distance))) switch {
            { IsEmpty: true } => None,
            var ahead => Some(ahead.MinBy(static held => held.Distance).Item),
        };

    private static Seq<Seq<Element>> Stopped(Element element) =>
        element.Role.Category == RoleCategory.Composite
            ? element.Children.Bind(Access.Flattened).Filter(Focusable) switch {
                { IsEmpty: true } => Focusable(element) ? [[element]] : [],
                var members => [members],
            }
            : (Focusable(element) ? Seq<Seq<Element>>([element]) : []) + element.Children.Bind(Stopped);

    private static bool Focusable(Element element) =>
        !element.Conditions.Contains(Condition.Disabled) && !element.Actions.IsEmpty;

    private static Option<float> Distance(SKRect origin, SKRect candidate, Navigation toward) {
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

    private static (float Low, float High) Spanned(SKRect frame, Orientation along, int sign) =>
        (Low: along.Of(frame.Location), High: along.Of(new SKPoint(frame.Right, frame.Bottom))) switch {
            var span => sign > 0 ? span : (-span.High, -span.Low),
        };
}
