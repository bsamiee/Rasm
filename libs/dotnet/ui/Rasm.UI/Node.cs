using System.Globalization;
using System.Numerics;
using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ImageFit {
    Stretch,
    Contain,
    Cover,
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Opacity : IMinMaxValue<Opacity> {
    public static Opacity MinValue { get; } = new(0f);
    public static Opacity MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
public sealed partial class GlyphRole {
    public static readonly GlyphRole Reset = new("reset");
    public static readonly GlyphRole Load = new("load");
    public static readonly GlyphRole Pick = new("pick");
    public static readonly GlyphRole LockOpen = new("lock-open");
    public static readonly GlyphRole LockClosed = new("lock-closed");
    public static readonly GlyphRole Presets = new("presets");
    public static readonly GlyphRole Options = new("options");
    public static readonly GlyphRole Grid = new("grid");
    public static readonly GlyphRole List = new("list");
    public static readonly GlyphRole FavoriteOn = new("favorite-on");
    public static readonly GlyphRole FavoriteOff = new("favorite-off");
    public static readonly GlyphRole VerticalWipe = new("vertical-wipe");
    public static readonly GlyphRole HorizontalWipe = new("horizontal-wipe");
    public static readonly GlyphRole SideBySide = new("side-by-side");
    public static readonly GlyphRole Swap = new("swap");
    public static readonly GlyphRole Fit = new("fit");
    public static readonly GlyphRole ActualSize = new("actual-size");
    public static readonly GlyphRole Flip = new("flip");
}

public sealed record GlyphSource(Assembly Assembly, string Light, Option<string> Dark) {
    public static Func<GlyphRole, GlyphSource> Embedded(Assembly assembly) => role => new GlyphSource(assembly, $"{role.Key}.svg", None);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public abstract partial record Key {
    private const char Mark = '#';
    private const string DividerValue = "#divider";
    private const string ThumbValue = "#thumb";

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Key? item) {
        item = value switch {
            null => null,
            DividerValue => new Divider(),
            ThumbValue => new Thumb(),
            _ when value.StartsWith($"{Mark}{Mark}", StringComparison.Ordinal) => new Named(value[1..]),
            _ when value.StartsWith(Mark) && uint.TryParse(value.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out uint ordinal) => new Ordinal(ordinal),
            _ => new Named(value),
        };
        return null;
    }

    public string ToValue() =>
        Switch(
            named: static named => named.Name.StartsWith(Mark) ? $"{Mark}{named.Name}" : named.Name,
            ordinal: static ordinal => $"{Mark}{ordinal.Index}",
            divider: static _ => DividerValue,
            thumb: static _ => ThumbValue);

    public sealed record Named(string Name) : Key;

    public sealed record Ordinal(uint Index) : Key;

    public sealed record Divider : Key;

    public sealed record Thumb : Key;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Node {
    public Option<Key> Key { get; init; }
    public Option<Role> Role { get; init; }
    public LanguageExt.HashSet<Condition> Conditions { get; init; }
    public Sizing Width { get; init; } = Sizing.Fit.Content;
    public Height Height { get; init; } = Sizing.Fit.Content;
    public Option<Part> Part { get; init; }
    public Option<Accessible> Accessible { get; init; }

    public sealed record Flex(Orientation Axis, Seq<Node> Children) : Node {
        public Vector2 Align { get; init; }
    }

    public sealed record Grid(Seq<Sizing> Columns, Seq<Seq<Node>> Rows) : Node {
        public Vector2 Align { get; init; }
    }

    public sealed record Wrap(Sizing Column, Seq<Node> Children) : Node {
        public Vector2 Align { get; init; }
    }

    public sealed record Overlay(Seq<Node> Children) : Node;

    public sealed record Choice(Orientation Axis, Seq<Node> Alternatives) : Node;

    public sealed record Scroll(Orientation Axis, Node Content) : Node;

    public sealed record Split(Orientation Axis, Node First, Node Second) : Node;

    public sealed record Plot(PlotPlane Plane, Seq<Node> Children) : Node;

    public sealed record Pin(Vector2 At) : Node {
        public Option<Node> Content { get; init; }
        public Vector2 Align { get; init; } = new(0.5f);
        public Option<Vector2> Span { get; init; }
    }

    public sealed record Mark(Figure Figure) : Node {
        public Option<Series> Series { get; init; }
    }

    public sealed record Text(string Content) : Node {
        public Option<int> MaxLines { get; init; }
        public TextAlignment Align { get; init; } = TextAlignment.Auto;
    }

    public sealed record Glyph(Either<GlyphRole, GlyphSource> Art) : Node;

    public sealed record Image(SKImage Source) : Node {
        public ImageFit Fit { get; init; } = ImageFit.Stretch;
        public SKSamplingOptions Sampling { get; init; } = new(SKFilterMode.Linear, SKMipmapMode.Linear);
        public Option<Series> Series { get; init; }
    }

    public sealed record Slot : Node;

    public sealed record Styled(Node Content) : Node {
        public Option<HashMap<Role, HashMap<Condition, Look>>> Appearance { get; init; }
        public Option<HashMap<Role, HashMap<Metric, float>>> Metrics { get; init; }
        public Opacity Opacity { get; init; } = Opacity.MaxValue;
    }
}
