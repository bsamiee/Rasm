using System.Globalization;
using System.Numerics;
using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;
using Wacton.Unicolour;

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ImageRendering {
    private static readonly SKSamplingOptions Smooth = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private static readonly SKSamplingOptions Nearest = new(SKFilterMode.Nearest, SKMipmapMode.None);

    public static readonly ImageRendering Auto = new("auto", Smooth, Smooth);
    public static readonly ImageRendering HighQuality = new("high-quality", new SKSamplingOptions(SKCubicResampler.Mitchell), Smooth);
    public static readonly ImageRendering CrispEdges = new("crisp-edges", Nearest, Nearest);
    public static readonly ImageRendering Pixelated = new("pixelated", Nearest, Smooth);

    private readonly SKSamplingOptions magnified;
    private readonly SKSamplingOptions minified;

    internal SKSamplingOptions Sampling(float density) => density > 1f ? magnified : minified;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GlyphSource {
    internal abstract Seq<(Assembly Assembly, string Resource)> Resources { get; }

    internal abstract (SKPicture Art, bool Tinted) Shown(SKColorF ground, Func<Assembly, string, SKPicture> recorded);

    public sealed record Tinted(Assembly Assembly, string Resource) : GlyphSource {
        internal override Seq<(Assembly Assembly, string Resource)> Resources => [(Assembly, Resource)];

        internal override (SKPicture Art, bool Tinted) Shown(SKColorF ground, Func<Assembly, string, SKPicture> recorded) => (recorded(Assembly, Resource), true);
    }

    public sealed record Paired(Assembly Assembly, string Light, string Dark) : GlyphSource {
        private static readonly Unicolour White = new(ColourSpace.Rgb, 1.0, 1.0, 1.0);
        private static readonly Unicolour Black = new(ColourSpace.Rgb, 0.0, 0.0, 0.0);

        internal override Seq<(Assembly Assembly, string Resource)> Resources => [(Assembly, Light), (Assembly, Dark)];

        internal override (SKPicture Art, bool Tinted) Shown(SKColorF ground, Func<Assembly, string, SKPicture> recorded) =>
            (recorded(Assembly, ground.ToUnicolour() switch { var under => under.Contrast(White) > under.Contrast(Black) ? Dark : Light }), false);
    }

    public sealed record Picture(SKPicture Art) : GlyphSource {
        internal override Seq<(Assembly Assembly, string Resource)> Resources => [];

        internal override (SKPicture Art, bool Tinted) Shown(SKColorF ground, Func<Assembly, string, SKPicture> recorded) => (Art, true);
    }
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Node {
    public Option<Key> Key { get; init; }
    public Option<Role> Role { get; init; }
    public LanguageExt.HashSet<Condition> Conditions { get; init; }
    public Option<Sizing> Width { get; init; }
    public Option<Sizing> Height { get; init; }
    public Option<Factor> Aspect { get; init; }
    public UnitPoint Align { get; init; }
    public Option<Part> Part { get; init; }
    public Accessible Accessible { get; init; } = Accessible.Empty;

    internal Option<Node> Inner => SwitchPartially(@default: static _ => Option<Node>.None, overlay: static overlay => Some(overlay.Base), pin: static pin => Some(pin.Content), styled: static styled => Some(styled.Content));

    internal Option<PlotPlane> Plotted => SwitchPartially(@default: static _ => Option<PlotPlane>.None, plot: static plot => Some(plot.Plane));

    internal Option<Lazy> Windowed => SwitchPartially(@default: static _ => Option<Lazy>.None, lazy: static lazy => Some(lazy));

    internal Option<Pin> Pinned => SwitchPartially(@default: static node => node.Inner.Bind(static inner => inner.Pinned), pin: static pin => Some(pin));

    internal Option<Figure> Traced => SwitchPartially(@default: static _ => Option<Figure>.None, mark: static mark => Some(mark.Figure));

    internal Option<GlyphSource> Glyphed => SwitchPartially(@default: static _ => Option<GlyphSource>.None, glyph: static glyph => Some(glyph.Art));

    internal Option<Styled> Restyled => SwitchPartially(@default: static _ => Option<Styled>.None, styled: static styled => Some(styled));

    internal float Alpha => Restyled.Map(static styled => (float)styled.Opacity).IfNone(1f);

    internal bool Paints => MapPartially(@default: false, mark: true, text: true, glyph: true, image: true, slot: true);

    internal bool Surfaced => MapPartially(@default: true, mark: false, image: false);

    internal bool Inherits => MapPartially(@default: false, text: true, glyph: true);

    internal bool Clips(Node child) => SwitchPartially(child, @default: static (_, _) => false, scroll: static (_, _) => true, plot: static (held, _) => held.Pinned.IsNone);

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
        public ImageRendering Rendering { get; init; } = ImageRendering.Auto;
        public Option<Series> Series { get; init; }
    }

    public sealed record Slot : Node;

    public sealed record Styled(Node Content) : Node {
        public HashMap<Role, Look> Appearance { get; init; }
        public HashMap<Role, HashMap<Metric, float>> Metrics { get; init; }
        public Fraction Opacity { get; init; } = Fraction.MaxValue;
    }
}
