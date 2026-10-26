using System.Numerics;
using SkiaSharp;
using Topten.RichTextKit.Editor;

namespace Rasm.UI;

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

[SmartEnum<NavigationKind>(SkipIComparable = true)]
public sealed partial class NavigationDirection {
    public static readonly NavigationDirection CharacterLeft = new(NavigationKind.CharacterLeft, PhysicalAxis.Horizontal, -1, Grain.Unit);
    public static readonly NavigationDirection CharacterRight = new(NavigationKind.CharacterRight, PhysicalAxis.Horizontal, 1, Grain.Unit);
    public static readonly NavigationDirection WordLeft = new(NavigationKind.WordLeft, PhysicalAxis.Horizontal, -1, Grain.Unit);
    public static readonly NavigationDirection WordRight = new(NavigationKind.WordRight, PhysicalAxis.Horizontal, 1, Grain.Unit);
    public static readonly NavigationDirection LineHome = new(NavigationKind.LineHome, PhysicalAxis.Horizontal, -1, Grain.End);
    public static readonly NavigationDirection LineEnd = new(NavigationKind.LineEnd, PhysicalAxis.Horizontal, 1, Grain.End);
    public static readonly NavigationDirection LineUp = new(NavigationKind.LineUp, PhysicalAxis.Vertical, -1, Grain.Unit);
    public static readonly NavigationDirection LineDown = new(NavigationKind.LineDown, PhysicalAxis.Vertical, 1, Grain.Unit);
    public static readonly NavigationDirection PageUp = new(NavigationKind.PageUp, PhysicalAxis.Vertical, -1, Grain.Page);
    public static readonly NavigationDirection PageDown = new(NavigationKind.PageDown, PhysicalAxis.Vertical, 1, Grain.Page);
    public static readonly NavigationDirection DocumentHome = new(NavigationKind.DocumentHome, PhysicalAxis.Vertical, -1, Grain.End);
    public static readonly NavigationDirection DocumentEnd = new(NavigationKind.DocumentEnd, PhysicalAxis.Vertical, 1, Grain.End);

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

    public sealed record Delete(Option<NavigationDirection> Toward) : Act;

    public sealed record Next : Act;

    public sealed record Previous : Act;

    public sealed record SelectAll : Act;

    public sealed record Insert(string Text) : Act;

    public sealed record Compose(string Marked, int Offset) : Act;

    public sealed record Undo : Act;

    public sealed record Redo : Act;
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
        mount.Editing.IsNone || modifiers.Common || modifiers.Alternate
            ? Access.Flattened(mount.Tree).Find(element => element.Accessible.Shortcut == Some(shortcut))
                .Map(element => (Input)new Perform(new Act.Press(), new Target.Element(element.Id)) { Modifiers = modifiers })
            : None;
}
