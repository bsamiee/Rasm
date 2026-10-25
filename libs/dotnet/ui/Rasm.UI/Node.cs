using System.Globalization;
using System.Numerics;
using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ImageFit {
    public static readonly ImageFit Stretch = new(static (content, _) => content);
    public static readonly ImageFit Contain = new(static (content, natural) => content.AspectFit(natural));
    public static readonly ImageFit Cover = new(static (content, natural) => content.AspectFill(natural));

    [UseDelegateFromConstructor]
    public partial SKRect Fitted(SKRect content, SKSize natural);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GlyphSource {
    public sealed record Tinted(Assembly Assembly, string Resource) : GlyphSource;

    public sealed record Paired(Assembly Assembly, string Light, string Dark) : GlyphSource;

    public sealed record Picture(SKPicture Art) : GlyphSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public abstract partial record Key {
    private const char Mark = '#';
    private const string HandleValue = "#handle";

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Key? item) {
        item = value switch {
            null => null,
            HandleValue => new Handle(),
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
            handle: static _ => HandleValue);

    public sealed record Named(string Name) : Key;

    public sealed record Ordinal(uint Index) : Key;

    public sealed record Handle : Key;
}

public sealed record Span(string Text) {
    public Option<Role> Role { get; init; }
    public LanguageExt.HashSet<Condition> Conditions { get; init; }

    public static implicit operator Span(string text) => FromString(text);

    public static Span FromString(string text) => new(text);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Node {
    public Option<Key> Key { get; init; }
    public Option<Role> Role { get; init; }
    public LanguageExt.HashSet<Condition> Conditions { get; init; }
    public Option<Sizing> Width { get; init; }
    public Option<Sizing> Height { get; init; }
    public Option<Aspect> Aspect { get; init; }
    public UnitPoint Align { get; init; }
    public Option<Part> Part { get; init; }
    public Accessible Accessible { get; init; } = Accessible.Empty;

    internal Option<PlotPlane> Plotted =>
        Switch(
            box: static _ => None, flex: static _ => None, grid: static _ => None, lazy: static _ => None, overlay: static _ => None, choice: static _ => None,
            scroll: static _ => None, split: static _ => None, plot: static plot => Some(plot.Plane), pin: static _ => None, mark: static _ => None, text: static _ => None,
            glyph: static _ => None, image: static _ => None, slot: static _ => None, styled: static _ => None);

    internal Option<Lazy> Windowed =>
        Switch(
            box: static _ => None, flex: static _ => None, grid: static _ => None, lazy: static lazy => Some(lazy), overlay: static _ => None, choice: static _ => None,
            scroll: static _ => None, split: static _ => None, plot: static _ => None, pin: static _ => None, mark: static _ => None, text: static _ => None,
            glyph: static _ => None, image: static _ => None, slot: static _ => None, styled: static _ => None);

    internal Option<Pin> Pinned =>
        Switch(
            box: static _ => None, flex: static _ => None, grid: static _ => None, lazy: static _ => None, overlay: static overlay => overlay.Base.Pinned, choice: static _ => None,
            scroll: static _ => None, split: static _ => None, plot: static _ => None, pin: static pin => Some(pin), mark: static _ => None, text: static _ => None,
            glyph: static _ => None, image: static _ => None, slot: static _ => None, styled: static styled => styled.Content.Pinned);

    internal Option<Figure> Traced =>
        Switch(
            box: static _ => None, flex: static _ => None, grid: static _ => None, lazy: static _ => None, overlay: static _ => None, choice: static _ => None,
            scroll: static _ => None, split: static _ => None, plot: static _ => None, pin: static _ => None, mark: static mark => Some(mark.Figure), text: static _ => None,
            glyph: static _ => None, image: static _ => None, slot: static _ => None, styled: static _ => None);

    internal Option<GlyphSource> Glyphed =>
        Switch(
            box: static _ => None, flex: static _ => None, grid: static _ => None, lazy: static _ => None, overlay: static _ => None, choice: static _ => None,
            scroll: static _ => None, split: static _ => None, plot: static _ => None, pin: static _ => None, mark: static _ => None, text: static _ => None,
            glyph: static glyph => Some(glyph.Art), image: static _ => None, slot: static _ => None, styled: static _ => None);

    internal Option<Styled> Restyled =>
        Switch(
            box: static _ => None, flex: static _ => None, grid: static _ => None, lazy: static _ => None, overlay: static _ => None, choice: static _ => None,
            scroll: static _ => None, split: static _ => None, plot: static _ => None, pin: static _ => None, mark: static _ => None, text: static _ => None,
            glyph: static _ => None, image: static _ => None, slot: static _ => None, styled: static styled => Some(styled));

    internal float Alpha => Restyled.Map(static styled => (float)styled.Opacity).IfNone(1f);

    internal bool Paints =>
        Map(box: false, flex: false, grid: false, lazy: false, overlay: false, choice: false, scroll: false, split: false, plot: false, pin: false, mark: true, text: true, glyph: true, image: true, slot: true, styled: false);

    internal bool Surfaced =>
        Map(box: true, flex: true, grid: true, lazy: true, overlay: true, choice: true, scroll: true, split: true, plot: true, pin: true, mark: false, text: true, glyph: true, image: false, slot: true, styled: true);

    internal bool Inherits =>
        Map(box: false, flex: false, grid: false, lazy: false, overlay: false, choice: false, scroll: false, split: false, plot: false, pin: false, mark: false, text: true, glyph: true, image: false, slot: false, styled: false);

    internal bool Clips(Node child) =>
        Switch(
            child,
            box: static (_, _) => false, flex: static (_, _) => false, grid: static (_, _) => false, lazy: static (_, _) => false, overlay: static (_, _) => false, choice: static (_, _) => false,
            scroll: static (_, _) => true, split: static (_, _) => false, plot: static (held, _) => held.Pinned.IsNone, pin: static (_, _) => false, mark: static (_, _) => false,
            text: static (_, _) => false, glyph: static (_, _) => false, image: static (_, _) => false, slot: static (_, _) => false, styled: static (_, _) => false);

    internal Node Conditioned(Condition condition) =>
        Restyled.Match(Some: styled => styled with { Content = styled.Content.Conditioned(condition) }, None: () => this with { Conditions = Conditions.TryAdd(condition) });

    public sealed record Box : Node;

    public sealed record Flex(Orientation Axis, Seq<Node> Children) : Node {
        public bool Baseline { get; init; }
    }

    public sealed record Grid(Tracks Columns, Seq<Seq<Node>> Rows) : Node;

    public sealed record Lazy(Orientation Axis, Tracks Across, float Extent, Seq<Key> Items) : Node {
        public HashMap<Key, Node> Realized { get; init; }
    }

    public sealed record Overlay(Node Base, Seq<Node> Above) : Node;

    public sealed record Choice(Orientation Axis, Seq<Node> Alternatives) : Node;

    public sealed record Scroll(Orientation Axis, Node Content) : Node;

    public sealed record Split(Orientation Axis, Node First, Node Second) : Node;

    public sealed record Plot(PlotPlane Plane, Seq<Node> Children) : Node {
        public Seq<Pin> Labels { get; init; }
    }

    public sealed record Pin(Vector2 At, Node Content) : Node {
        public Option<Vector2> Span { get; init; }
    }

    public sealed record Mark(Figure Figure) : Node {
        public Option<Series> Series { get; init; }
    }

    public sealed record Text(Seq<Span> Spans) : Node {
        public string Content => string.Concat(Spans.Map(static span => span.Text));
        public Option<int> MaxLines { get; init; }
        public TextAlignment Alignment { get; init; } = TextAlignment.Auto;
    }

    public sealed record Glyph(GlyphSource Art) : Node;

    public sealed record Image(SKImage Source) : Node {
        public ImageFit Fit { get; init; } = ImageFit.Stretch;
        public float Density { get; init; } = 1f;
        public SKSamplingOptions Sampling { get; init; } = new(SKFilterMode.Linear, SKMipmapMode.Linear);
        public Option<Series> Series { get; init; }
    }

    public sealed record Slot : Node;

    public sealed record Styled(Node Content) : Node {
        public HashMap<Role, Look> Appearance { get; init; }
        public HashMap<Role, HashMap<Metric, float>> Metrics { get; init; }
        public Opacity Opacity { get; init; } = Opacity.MaxValue;
    }
}
