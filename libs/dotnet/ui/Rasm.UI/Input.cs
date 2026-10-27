using System.Numerics;
using Rasm.UI.Plotting;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PointerSample(SKPoint At, float Pressure);

public sealed record KeyModifiers(bool Common, bool Alternate, bool Shift) {
    public static KeyModifiers Plain { get; } = new(Common: false, Alternate: false, Shift: false);
}

[SmartEnum]
public sealed partial class Grain {
    public static readonly Grain Unit = new(static (step, _) => Some(step));
    public static readonly Grain Page = new(static (_, page) => Some(page));
    public static readonly Grain End = new(static (_, _) => Option<float>.None);

    [UseDelegateFromConstructor]
    public partial Option<float> Stride(float step, float page);
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
    public KeyModifiers Modifiers { get; init; } = KeyModifiers.Plain;

    public sealed record Pointer(Edit<PointerSample> Stroke, int Clicks) : Input;

    public sealed record Hover(Option<SKPoint> At) : Input;

    public sealed record Wheel(SKPoint At, Vector2 Delta) : Input;

    public sealed record Scroll(SKPoint At, Edit<Vector2> Delta) : Input;

    public sealed record Magnify(SKPoint At, Edit<float> Amount) : Input;

    public sealed record Rotate(SKPoint At, Edit<float> Radians) : Input;

    public sealed record Perform(Act Act, Target Target) : Input;

    public sealed record Focused : Input;

    public sealed record Blurred : Input;

    public static Option<Input> Pointed(Edit<PointerSample> stroke, bool primary, bool alternate, int clicks, KeyModifiers modifiers) =>
        stroke.Value.Match(
                Some: sample => stroke.Phase switch {
                    { Opens: true } => Some(alternate ? (Input)new Perform(new Act.ShowMenu(), new Target.At(sample.At)) : new Pointer(stroke, clicks)),
                    { Settles: true } => primary ? Some<Input>(new Pointer(stroke, clicks)) : None,
                    _ => Some(primary ? (Input)new Pointer(stroke, clicks) : new Hover(Some(sample.At))),
                },
                None: () => Some<Input>(new Pointer(stroke, clicks)))
            .Map(input => input with { Modifiers = modifiers });

    public static Option<Input> Keyed(Mount mount, string shortcut, KeyModifiers modifiers) =>
        Access.Flattened(mount.Tree).Find(element => element.Accessible.Shortcut == Some(shortcut))
            .Map(element => (Input)new Perform(new Act.Press(), new Target.Element(element.Id)) { Modifiers = modifiers });
}
