using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IStreamed<TSelf, TIncrement> where TSelf : Gesture, IStreamed<TSelf, TIncrement> where TIncrement : notnull {
    public static abstract TSelf Of(SKPoint at, TIncrement increment, KeyModifiers modifiers);

    public static abstract Option<TSelf> From(Gesture gesture);

    public TSelf Folded(SKPoint at, TIncrement increment, KeyModifiers modifiers);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Phase<T> where T : notnull {
    public sealed record Began(T Value) : Phase<T>;

    public sealed record Changed(T Value) : Phase<T>;

    public sealed record Ended(T Value) : Phase<T>;

    public sealed record Cancelled : Phase<T>;
}

public sealed record PointerSample(SKPoint At, float Pressure);

public sealed record KeyModifiers(bool Common, bool Alternate, bool Shift) {
    public static KeyModifiers None { get; } = new(Common: false, Alternate: false, Shift: false);
}

[SmartEnum]
public sealed partial class Grain {
    public static readonly Grain Unit = new(static (unit, _) => Some(unit));
    public static readonly Grain Page = new(static (_, page) => Some(page));
    public static readonly Grain End = new(static (_, _) => Option<float>.None);

    [UseDelegateFromConstructor]
    public partial Option<float> Stride(float unit, float page);
}

[SmartEnum]
public sealed partial class NavigationDirection {
    public static readonly NavigationDirection Left = new(PhysicalAxis.Horizontal, -1, Grain.Unit);
    public static readonly NavigationDirection Right = new(PhysicalAxis.Horizontal, 1, Grain.Unit);
    public static readonly NavigationDirection Up = new(PhysicalAxis.Vertical, -1, Grain.Unit);
    public static readonly NavigationDirection Down = new(PhysicalAxis.Vertical, 1, Grain.Unit);
    public static readonly NavigationDirection PageUp = new(PhysicalAxis.Vertical, -1, Grain.Page);
    public static readonly NavigationDirection PageDown = new(PhysicalAxis.Vertical, 1, Grain.Page);
    public static readonly NavigationDirection Home = new(PhysicalAxis.Horizontal, -1, Grain.End);
    public static readonly NavigationDirection End = new(PhysicalAxis.Horizontal, 1, Grain.End);
    public static readonly NavigationDirection Top = new(PhysicalAxis.Vertical, -1, Grain.End);
    public static readonly NavigationDirection Bottom = new(PhysicalAxis.Vertical, 1, Grain.End);

    public PhysicalAxis Along { get; }
    public int Sign { get; }
    public Grain Grain { get; }

    public Vector2 Screen => Along.With(Vector2.Zero, Sign);

    public int Valued(PlotFrame frame) => Sign * MathF.Sign(Along.Of(new Vector2(frame.Matrix.ScaleX, frame.Matrix.ScaleY)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CursorShape {
    public static readonly CursorShape Default = new("default");
    public static readonly CursorShape Pointer = new("pointer");
    public static readonly CursorShape Text = new("text");
    public static readonly CursorShape Crosshair = new("crosshair");
    public static readonly CursorShape Move = new("move");
    public static readonly CursorShape Grab = new("grab");
    public static readonly CursorShape Grabbing = new("grabbing");
    public static readonly CursorShape ColumnResize = new("col-resize");
    public static readonly CursorShape RowResize = new("row-resize");
    public static readonly CursorShape NotAllowed = new("not-allowed");

    public static CursorShape Resizing(PhysicalAxis along) => along.Map(horizontal: ColumnResize, vertical: RowResize);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Target {
    public sealed record Focused : Target;

    public sealed record At(SKPoint Point) : Target;

    public sealed record Element(Seq<Key> Id) : Target;

    public sealed record Surface : Target;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Act {
    public sealed record Press : Act;

    public sealed record Confirm : Act;

    public sealed record Cancel : Act;

    public sealed record ShowMenu : Act;

    public sealed record ScrollToVisible : Act;

    public sealed record Increment : Act;

    public sealed record Decrement : Act;

    public sealed record Navigate(NavigationDirection Toward) : Act;

    public sealed record Delete : Act;

    public sealed record Next : Act;

    public sealed record Previous : Act;

    public sealed record SelectAll : Act;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Input {
    public KeyModifiers Modifiers { get; init; } = KeyModifiers.None;

    public sealed record Pointer(Phase<PointerSample> Stroke, int Clicks) : Input;

    public sealed record Hover(Option<SKPoint> At) : Input;

    public sealed record Wheel(SKPoint At, Vector2 Delta) : Input;

    public sealed record Scroll(SKPoint At, Phase<Vector2> Delta) : Input;

    public sealed record Magnify(SKPoint At, Phase<float> Amount) : Input;

    public sealed record Rotate(SKPoint At, Phase<float> Radians) : Input;

    public sealed record Perform(Act Act, Target Target) : Input;

    public sealed record Focused : Input;

    public sealed record Blurred : Input;

    public static Option<Input> Pointed(Phase<PointerSample> stroke, bool primary, bool alternate, int clicks, KeyModifiers modifiers) =>
        stroke.Switch(
                (Stroke: stroke, Primary: primary, Alternate: alternate, Clicks: clicks),
                began: static (held, began) => Some(held.Alternate
                    ? (Input)new Perform(new Act.ShowMenu(), new Target.At(began.Value.At))
                    : new Pointer(held.Stroke, held.Clicks)),
                changed: static (held, changed) => Some(held.Primary ? (Input)new Pointer(held.Stroke, held.Clicks) : new Hover(Some(changed.Value.At))),
                ended: static (held, _) => held.Primary ? Some<Input>(new Pointer(held.Stroke, held.Clicks)) : None,
                cancelled: static (held, _) => Some<Input>(new Pointer(held.Stroke, held.Clicks)))
            .Map(input => input with { Modifiers = modifiers });

    public static Option<Input> Keyed(Mount mount, string shortcut, KeyModifiers modifiers) =>
        Access.Flattened(mount.Tree).Find(element => element.Accessible.Shortcut == Some(shortcut))
            .Map(element => (Input)new Perform(new Act.Press(), new Target.Element(element.Id)) { Modifiers = modifiers });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record Gesture {
    public KeyModifiers Modifiers { get; init; } = KeyModifiers.None;

    public sealed record Press(SKPoint From, Option<SKPoint> Dragged, int Clicks, float Pressure) : Gesture {
        public SKPoint At => Dragged.IfNone(From);
        public Vector2 Velocity { get; init; }
        public Vector2 Scrolled { get; init; }

        public Press Followed(PointerSample sample, float slop) =>
            this with { Pressure = sample.Pressure, Dragged = Dragged.IsSome || SKPoint.Distance(sample.At, From) >= slop ? Some(sample.At) : Dragged };
    }

    public sealed record Scroll(SKPoint At, Vector2 Travel) : Gesture, IStreamed<Scroll, Vector2> {
        public static Scroll Of(SKPoint at, Vector2 increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        public static Option<Scroll> From(Gesture gesture) => gesture.SwitchPartially(@default: static _ => Option<Scroll>.None, scroll: static scroll => Some(scroll));

        public Scroll Folded(SKPoint at, Vector2 increment, KeyModifiers modifiers) => this with { At = at, Travel = Travel + increment, Modifiers = modifiers };
    }

    public sealed record Magnify(SKPoint About, float Magnification) : Gesture, IStreamed<Magnify, float> {
        public static Magnify Of(SKPoint at, float increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        public static Option<Magnify> From(Gesture gesture) => gesture.SwitchPartially(@default: static _ => Option<Magnify>.None, magnify: static magnify => Some(magnify));

        public Magnify Folded(SKPoint at, float increment, KeyModifiers modifiers) =>
            this with { About = at, Magnification = ((1f + Magnification) * (1f + increment)) - 1f, Modifiers = modifiers };
    }

    public sealed record Rotate(SKPoint About, float Rotation) : Gesture, IStreamed<Rotate, float> {
        public static Rotate Of(SKPoint at, float increment, KeyModifiers modifiers) => new(at, increment) { Modifiers = modifiers };

        public static Option<Rotate> From(Gesture gesture) => gesture.SwitchPartially(@default: static _ => Option<Rotate>.None, rotate: static rotate => Some(rotate));

        public Rotate Folded(SKPoint at, float increment, KeyModifiers modifiers) => this with { About = at, Rotation = Rotation + increment, Modifiers = modifiers };
    }

    public sealed record Perform(Act Act) : Gesture;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Capture {
    public sealed record Part(Seq<Key> Owner, Seq<Key> Over, Gesture.Press Press, Track Track, Duration Time) : Capture;

    public sealed record Handle(Seq<Key> Adjuster, SKPoint From, float Start) : Capture;
}

internal sealed record LiveGesture(Gesture Gesture, Track Track);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Latch {
    public sealed record Part(Seq<Key> Owner, Option<LiveGesture> Live) : Latch;

    public sealed record Scroll(Seq<Key> Adjuster, Option<Gesture.Scroll> Live) : Latch;
}

internal sealed record Focus(Seq<Key> Element, bool Traversed) {
    public bool Visible => Traversed;
}

internal sealed record Routing(Option<SKPoint> Pointer, KeyModifiers Modifiers, Option<Capture> Capture, Option<Latch> Latch, Option<Focus> Focus) {
    // --- [STATE]
    public static Routing Idle { get; } = new(None, KeyModifiers.None, None, None, None);

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

    internal static Option<T> Toward<T>(SKRect origin, Seq<(T Item, SKRect Frame)> candidates, NavigationDirection toward) =>
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

    private static (float Low, float High) Spanned(SKRect frame, PhysicalAxis along, int sign) =>
        (Low: along.Of(frame.Location), High: along.Of(new SKPoint(frame.Right, frame.Bottom))) switch {
            var span => sign > 0 ? span : (-span.High, -span.Low),
        };
}
