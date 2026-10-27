using System.Numerics;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Stylus(Fraction Pressure, Vector2 Tilt, bool Eraser);

public sealed record PointerSample(SKPoint At) {
    public Option<Stylus> Stylus { get; init; }
}

public sealed record KeyModifiers(bool Primary, bool Alternate, bool Shift) {
    public static KeyModifiers Empty { get; } = new(Primary: false, Alternate: false, Shift: false);
}

[SmartEnum]
internal sealed partial class Granularity {
    public static readonly Granularity Step = new(static (step, _) => Some(step));
    public static readonly Granularity Page = new(static (_, page) => Some(page));
    public static readonly Granularity End = new(static (_, _) => Option<float>.None);

    [UseDelegateFromConstructor]
    internal partial Option<float> Distance(float step, float page);
}

internal sealed record Bearing(PhysicalAxis Along, int Sign, Granularity Granularity) {
    internal Vector2 Vector => Along.With(Vector2.Zero, Sign);
}

[SmartEnum]
public sealed partial class CursorShape {
    public static readonly CursorShape Default = new();
    public static readonly CursorShape Pointer = new();
    public static readonly CursorShape Text = new();
    public static readonly CursorShape Crosshair = new();
    public static readonly CursorShape Move = new();
    public static readonly CursorShape Grab = new();
    public static readonly CursorShape Grabbing = new();
    public static readonly CursorShape ColumnResize = new();
    public static readonly CursorShape RowResize = new();
    public static readonly CursorShape NotAllowed = new();

    internal CursorShape Dragged => this == Grab ? Grabbing : this;

    public static CursorShape Resizing(PhysicalAxis along) => along.Map(horizontal: ColumnResize, vertical: RowResize);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public sealed partial class Act {
    public static readonly Act Press = new(None);
    public static readonly Act Confirm = new(None);
    public static readonly Act Cancel = new(None);
    public static readonly Act ShowMenu = new(None);
    public static readonly Act ScrollToVisible = new(None);
    public static readonly Act Increment = new(None);
    public static readonly Act Decrement = new(None);
    public static readonly Act Reset = new(None);
    public static readonly Act Delete = new(None);
    public static readonly Act Next = new(None);
    public static readonly Act Previous = new(None);
    public static readonly Act SelectAll = new(None);
    public static readonly Act Left = new(Some(new Bearing(PhysicalAxis.Horizontal, -1, Granularity.Step)));
    public static readonly Act Right = new(Some(new Bearing(PhysicalAxis.Horizontal, 1, Granularity.Step)));
    public static readonly Act Up = new(Some(new Bearing(PhysicalAxis.Vertical, -1, Granularity.Step)));
    public static readonly Act Down = new(Some(new Bearing(PhysicalAxis.Vertical, 1, Granularity.Step)));
    public static readonly Act PageUp = new(Some(new Bearing(PhysicalAxis.Vertical, -1, Granularity.Page)));
    public static readonly Act PageDown = new(Some(new Bearing(PhysicalAxis.Vertical, 1, Granularity.Page)));
    public static readonly Act Home = new(Some(new Bearing(PhysicalAxis.Horizontal, -1, Granularity.End)));
    public static readonly Act End = new(Some(new Bearing(PhysicalAxis.Horizontal, 1, Granularity.End)));
    public static readonly Act Top = new(Some(new Bearing(PhysicalAxis.Vertical, -1, Granularity.End)));
    public static readonly Act Bottom = new(Some(new Bearing(PhysicalAxis.Vertical, 1, Granularity.End)));

    internal Option<Bearing> Toward { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Target {
    public sealed record Focused : Target;

    public sealed record At(SKPoint Point) : Target;

    public sealed record Element(Seq<Key> Id) : Target;

    public sealed record Surface : Target;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Input {
    public KeyModifiers Modifiers { get; init; } = KeyModifiers.Empty;

    public sealed record Pointer(Edit<PointerSample> Stroke, int Clicks) : Input;

    public sealed record Hover(Option<SKPoint> At) : Input;

    public sealed record Scroll(SKPoint At, Edit<Vector2> Delta) : Input;

    public sealed record Magnify(SKPoint At, Edit<float> Amount) : Input;

    public sealed record Rotate(SKPoint At, Edit<float> Radians) : Input;

    public sealed record Drop(SKPoint At, Edit<Transfer> Transfer) : Input;

    public sealed record Perform(Act Act, Target Target) : Input;

    public static Option<Input> Pointed(Edit<PointerSample> stroke, bool primary, bool secondary, int clicks, KeyModifiers modifiers) =>
        (stroke.Phase == EditPhase.Cancel
            ? Some<Input>(new Pointer(stroke, clicks))
            : stroke.Value.Bind(sample =>
                stroke.Phase.Begins && secondary ? Some<Input>(new Perform(Act.ShowMenu, new Target.At(sample.At)))
                : stroke.Phase.Begins || primary ? Some<Input>(new Pointer(stroke, clicks))
                : stroke.Phase == EditPhase.Commit ? None
                : Some<Input>(new Hover(Some(sample.At)))))
        .Map(input => input with { Modifiers = modifiers });

    public static Option<Input> Keyed(Mount mount, string shortcut, KeyModifiers modifiers) =>
        Access.Flattened(mount.Tree).Find(element => element.Accessible.Shortcut == Some(shortcut))
            .Map(element => (Input)new Perform(Act.Press, new Target.Element(element.Id)) { Modifiers = modifiers });
}
