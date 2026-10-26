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

[SmartEnum]
public sealed partial class Modifier {
    public static readonly Modifier Common = new();
    public static readonly Modifier Alternate = new();
    public static readonly Modifier Shift = new();
}

[SmartEnum]
public sealed partial class Grain {
    public static readonly Grain Unit = new();
    public static readonly Grain Page = new();
    public static readonly Grain Edge = new();
}

[SmartEnum<NavigationKind>(SkipIComparable = true)]
public sealed partial class Navigation {
    public static readonly Navigation CharacterLeft = new(NavigationKind.CharacterLeft, Orientation.Horizontal, -1, Grain.Unit);
    public static readonly Navigation CharacterRight = new(NavigationKind.CharacterRight, Orientation.Horizontal, 1, Grain.Unit);
    public static readonly Navigation WordLeft = new(NavigationKind.WordLeft, Orientation.Horizontal, -1, Grain.Unit);
    public static readonly Navigation WordRight = new(NavigationKind.WordRight, Orientation.Horizontal, 1, Grain.Unit);
    public static readonly Navigation LineHome = new(NavigationKind.LineHome, Orientation.Horizontal, -1, Grain.Edge);
    public static readonly Navigation LineEnd = new(NavigationKind.LineEnd, Orientation.Horizontal, 1, Grain.Edge);
    public static readonly Navigation LineUp = new(NavigationKind.LineUp, Orientation.Vertical, -1, Grain.Unit);
    public static readonly Navigation LineDown = new(NavigationKind.LineDown, Orientation.Vertical, 1, Grain.Unit);
    public static readonly Navigation PageUp = new(NavigationKind.PageUp, Orientation.Vertical, -1, Grain.Page);
    public static readonly Navigation PageDown = new(NavigationKind.PageDown, Orientation.Vertical, 1, Grain.Page);
    public static readonly Navigation DocumentHome = new(NavigationKind.DocumentHome, Orientation.Vertical, -1, Grain.Edge);
    public static readonly Navigation DocumentEnd = new(NavigationKind.DocumentEnd, Orientation.Vertical, 1, Grain.Edge);

    public Orientation Along { get; }
    public int Sign { get; }
    public Grain Grain { get; }

    public Vector2 Screen => Along.With(Vector2.Zero, Sign);

    public int Valued(PlotFrame frame) => Sign * MathF.Sign(Along.Of(new Vector2(frame.Matrix.ScaleX, frame.Matrix.ScaleY)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Cursor {
    public static readonly Cursor Default = new("default");
    public static readonly Cursor Pointer = new("pointer");
    public static readonly Cursor Text = new("text");
    public static readonly Cursor Crosshair = new("crosshair");
    public static readonly Cursor Move = new("move");
    public static readonly Cursor Grab = new("grab");
    public static readonly Cursor Grabbing = new("grabbing");
    public static readonly Cursor ColumnResize = new("col-resize");
    public static readonly Cursor RowResize = new("row-resize");
    public static readonly Cursor NotAllowed = new("not-allowed");

    public static Cursor Resizing(Orientation along) => along.Map(horizontal: ColumnResize, vertical: RowResize);
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

    public sealed record Navigate(Navigation Toward) : Act;

    public sealed record Delete(Option<Navigation> Toward) : Act;

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
    public LanguageExt.HashSet<Modifier> Modifiers { get; init; }

    public sealed record Pointer(Phase<PointerSample> Stroke, int Clicks) : Input;

    public sealed record Hover(Option<SKPoint> At) : Input;

    public sealed record Wheel(SKPoint At, Vector2 Delta) : Input;

    public sealed record Scroll(SKPoint At, Phase<Vector2> Delta) : Input;

    public sealed record Magnify(SKPoint At, Phase<float> Amount) : Input;

    public sealed record Rotate(SKPoint At, Phase<float> Radians) : Input;

    public sealed record Perform(Act Act, Target Target) : Input;

    public sealed record Focused : Input;

    public sealed record Blurred : Input;
}
