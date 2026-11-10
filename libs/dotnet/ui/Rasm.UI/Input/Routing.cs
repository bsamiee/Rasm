using Rasm.UI.Automation;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Input;

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PointerCapture {
    private PointerCapture(Seq<Key> owner, Seq<Key> hit) => (Owner, Hit) = (owner, hit);

    internal Seq<Key> Owner { get; }
    internal Seq<Key> Hit { get; }

    internal Seq<IO<Unit>> Cancelled => Switch(part: static part => part.Track.Cancelled(), handle: static _ => []);

    internal sealed record Part(Seq<Key> Owner, Seq<Key> Hit, Gesture.Press Press, Track Track) : PointerCapture(Owner, Hit);

    internal sealed record Handle(Seq<Key> Owner, SKPoint From, float Start) : PointerCapture(Owner, Owner);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Latch {
    private Latch(Seq<Key> owner) => Owner = owner;

    internal Seq<Key> Owner { get; }

    internal Seq<IO<Unit>> Cancelled => Switch(part: static part => part.Active.Map(static live => live.Track.Cancelled()).IfNone([]), scroll: static _ => []);

    internal sealed record Part(Seq<Key> Owner, Option<(Gesture Gesture, Track Track)> Active) : Latch(Owner);

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
    private const float HorizontalOrthogonalWeight = 30f;
    private const float VerticalOrthogonalWeight = 2f;

    internal static Seq<Seq<AutomationElement>> Stops(Seq<AutomationElement> tree) => tree.Bind(Stopped);

    internal static Option<Seq<Key>> Traversed(Seq<Seq<AutomationElement>> stops, Option<Seq<Key>> focused, int sign, bool cycles) =>
        from target in focused.Map(focus => stops.FindIndex(stop => stop.Exists(element => element.Id == focus))).Filter(static index => index >= 0).Map(index => index + sign)
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

    private static Seq<Seq<AutomationElement>> Stopped(AutomationElement element) =>
        element.Role.Composite
            ? AutomationElement.Flattened(element.Children).Filter(Focusable) switch {
                { IsEmpty: true } => Focusable(element) ? [[element]] : [],
                var members => [members],
            }
            : (Focusable(element) ? Seq<Seq<AutomationElement>>([element]) : []) + element.Children.Bind(Stopped);

    private static bool Focusable(AutomationElement element) =>
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
                + ((gap + (aligned > 0f ? 0f : exit.Height / 2f)) * toward.Along.Map(horizontal: HorizontalOrthogonalWeight, vertical: VerticalOrthogonalWeight))
                - (aligned > 0f ? AlignWeight * float.Min(aligned / exit.Height, 1f) : 0f)
                - (overlap > 0f && aligned > 0f ? MathF.Sqrt(overlap * aligned) : 0f))
            : None;
    }
}
