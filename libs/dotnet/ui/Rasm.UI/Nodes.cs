using System.Numerics;
using System.Reflection;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;
using Wacton.Unicolour;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Sizing {
    public Limits Limits { get; init; } = Limits.Open;

    internal virtual float Growth => 0f;
    internal Option<int> Yields => Map(@fixed: Option<int>.None, fit: Some(1), grow: Some(0), percent: Some(1));
    internal bool Stretches => Growth > 0f;
    internal virtual Option<float> Definite => None;

    public static Seq<float> Stretched(Seq<Fixed> items, float space) =>
        Layouts.Distribute(items.Map(static item => new Layouts.Demand(Grow.Unit with { Limits = item.Limits }, item.Limits.Clamped(item.Extent), item.Limits.Min)), space);

    internal float Floor(float content) => SwitchPartially(content, @default: static (_, held) => held.Limits.Min, fit: static (least, held) => float.Max(held.Limits.Min, least));

    public sealed record Fixed(float Extent) : Sizing {
        internal override Option<float> Definite => Some(Limits.Clamped(Extent));
    }

    public sealed record Fit : Sizing {
        public static Fit Content { get; } = new();
    }

    public sealed record Grow(Factor Weight) : Sizing {
        public static Grow Unit { get; } = new(Factor.Unit);

        internal override float Growth => (float)Weight;
    }

    public sealed record Percent(Fraction Fraction) : Sizing;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Tracks {
    public sealed record Explicit(Seq<Sizing> Sizes) : Tracks;

    public sealed record AutoFill(Sizing Size) : Tracks;
}

[SmartEnum]
public sealed partial class ImageFit {
    public static readonly ImageFit Stretch = new(static (content, _) => content);
    public static readonly ImageFit Contain = new(static (content, natural) => content.AspectFit(natural));
    public static readonly ImageFit Cover = new(static (content, natural) => content.AspectFill(natural));

    [UseDelegateFromConstructor]
    public partial SKRect Fitted(SKRect content, SKSize natural);
}

[SmartEnum]
public sealed partial class ImageRendering {
    private static readonly SKSamplingOptions Smooth = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private static readonly SKSamplingOptions Nearest = new(SKFilterMode.Nearest, SKMipmapMode.None);

    public static readonly ImageRendering Auto = new(Smooth, Smooth);
    public static readonly ImageRendering HighQuality = new(new SKSamplingOptions(SKCubicResampler.Mitchell), Smooth);
    public static readonly ImageRendering CrispEdges = new(Nearest, Nearest);
    public static readonly ImageRendering Pixelated = new(Nearest, Smooth);

    private readonly SKSamplingOptions magnified;
    private readonly SKSamplingOptions minified;

    internal SKSamplingOptions Sampling(float density) => density > 1f ? magnified : minified;
}

[SmartEnum]
internal sealed partial class Painting {
    public static readonly Painting Frame = new(paints: false, acceptsBackground: true, inherits: false);
    public static readonly Painting Figure = new(paints: true, acceptsBackground: false, inherits: false);
    public static readonly Painting Ink = new(paints: true, acceptsBackground: true, inherits: true);
    public static readonly Painting Hosted = new(paints: true, acceptsBackground: true, inherits: false);

    internal bool Paints { get; }
    internal bool AcceptsBackground { get; }
    internal bool Inherits { get; }
}

internal sealed record Art(Assembly Assembly, string Resource, bool Tinted) {
    internal Option<SKColor> Current { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GlyphSource {
    internal abstract Art Shown(SKColorF under);

    public sealed record Tinted(Assembly Assembly, string Resource) : GlyphSource {
        internal override Art Shown(SKColorF under) => new(Assembly, Resource, Tinted: true);
    }

    public sealed record Paired(Assembly Assembly, string Light, string Dark) : GlyphSource {
        private static readonly Unicolour White = new(ColourSpace.Rgb, 1.0, 1.0, 1.0);
        private static readonly Unicolour Black = new(ColourSpace.Rgb, 0.0, 0.0, 0.0);

        public string Resource(SKColorF under) {
            Unicolour held = under.ToUnicolour();
            return held.Contrast(White) > held.Contrast(Black) ? Dark : Light;
        }

        internal override Art Shown(SKColorF under) => new(Assembly, Resource(under), Tinted: false);
    }
}

public sealed record Span(string Text) {
    public Option<Role> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }

    public static implicit operator Span(string text) => FromString(text);

    public static Span FromString(string text) => new(text);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SlotKind {
    public sealed record Text(string Value, Option<int> MaxLines, Func<Edit<string>, IO<Unit>> Edited) : SlotKind;

    public sealed record Native : SlotKind;
}

public sealed record Expiry(Duration Lifetime, IO<Unit> Elapsed);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Node {
    private Node(Painting painting) => Painting = painting;

    public Option<Key> Key { get; init; }
    public Option<Role> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public Option<Sizing> Width { get; init; }
    public Option<Sizing> Height { get; init; }
    public Option<Factor> Aspect { get; init; }
    public UnitPoint Align { get; init; } = UnitPoint.Start;
    public Option<Part> Part { get; init; }
    public Option<Expiry> Expires { get; init; }
    public Accessible Accessible { get; init; } = Accessible.Empty;

    internal virtual Option<Node> Inner => None;

    internal virtual Option<PlotPlane> Plotted => None;

    internal virtual Option<Func<Option<Vector2>, IO<Unit>>> Hovering => None;

    internal virtual Option<Lazy> Windowed => None;

    internal Option<Pin> Pinned => SwitchPartially(@default: static node => node.Inner.Bind(static inner => inner.Pinned), pin: static pin => Some(pin));

    internal virtual Option<Figure> Traced => None;

    internal virtual Option<GlyphSource> Glyphed => None;

    internal virtual Option<SKImage> Imaged => None;

    internal virtual Option<Styled> Restyled => None;

    internal float Alpha => Restyled.Map(static styled => (float)styled.Opacity).IfNone(1f);

    internal Painting Painting { get; }

    internal bool Clips(Node child) => SwitchPartially(child, @default: static (_, _) => false, scroll: static (_, _) => true, plot: static (held, _) => held.Pinned.IsNone);

    internal Node Conditioned(Condition condition) =>
        Restyled.Match(Some: styled => styled with { Content = styled.Content.Conditioned(condition) }, None: () => this with { Conditions = Conditions.TryAdd(condition) });

    public sealed record Box() : Node(Painting.Frame);

    public sealed record Flex(PhysicalAxis Axis, Seq<Node> Children) : Node(Painting.Frame) {
        public bool Baseline { get; init; }
    }

    public sealed record Grid(Tracks Columns, Seq<Seq<Node>> Rows) : Node(Painting.Frame);

    public sealed record Lazy(PhysicalAxis Axis, Tracks Across, float Extent, Seq<Key> Items) : Node(Painting.Frame) {
        public HashMap<Key, Node> Realized { get; init; }

        internal override Option<Lazy> Windowed => Some(this);
    }

    public sealed record Overlay(Node Base, Seq<Node> Above) : Node(Painting.Frame) {
        internal override Option<Node> Inner => Some(Base);
    }

    public sealed record Choice(PhysicalAxis Axis, Seq<Node> Alternatives) : Node(Painting.Frame);

    public sealed record Scroll(PhysicalAxis Axis, Node Content) : Node(Painting.Frame);

    public sealed record Plot(PlotPlane Plane, Seq<Node> Children) : Node(Painting.Frame) {
        public Seq<Pin> Labels { get; init; }
        public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }

        internal override Option<PlotPlane> Plotted => Some(Plane);
        internal override Option<Func<Option<Vector2>, IO<Unit>>> Hovering => Hovered;
    }

    public sealed record Pin(Vector2 At, Node Content) : Node(Painting.Frame) {
        public Option<Vector2> Span { get; init; }

        internal override Option<Node> Inner => Some(Content);
    }

    public sealed record Mark(Figure Figure) : Node(Painting.Figure) {
        public Option<Series> Series { get; init; }

        internal override Option<Figure> Traced => Some(Figure);
    }

    public sealed record Text(Seq<Span> Spans) : Node(Painting.Ink) {
        public string Content => string.Concat(Spans.Map(static span => span.Text));
        public Option<int> MaxLines { get; init; }
        public TextAlignment Alignment { get; init; } = TextAlignment.Auto;
    }

    public sealed record Glyph(GlyphSource Art) : Node(Painting.Ink) {
        internal override Option<GlyphSource> Glyphed => Some(Art);
    }

    public sealed record Image(SKImage Source) : Node(Painting.Figure) {
        public ImageFit Fit { get; init; } = ImageFit.Stretch;
        public float Density { get; init; } = 1f;
        public ImageRendering Rendering { get; init; } = ImageRendering.Auto;
        public Option<Series> Series { get; init; }

        internal override Option<SKImage> Imaged => Some(Source);
    }

    public sealed record Slot(SlotKind Kind) : Node(Painting.Hosted);

    public sealed record Styled(Node Content) : Node(Painting.Frame) {
        public StyleSheet Sheet { get; init; } = StyleSheet.Empty;
        public Fraction Opacity { get; init; } = Fraction.MaxValue;

        internal override Option<Node> Inner => Some(Content);
        internal override Option<Styled> Restyled => Some(this);
    }
}
