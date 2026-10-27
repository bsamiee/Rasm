using System.Globalization;
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

    internal virtual float GrowWeight => 0f;
    internal Option<int> ShrinkTier => Map(@fixed: Option<int>.None, fit: Some(1), grow: Some(0), percent: Some(1));
    internal bool Stretches => GrowWeight > 0f;
    internal virtual Option<float> Definite => None;

    public static Seq<float> Stretched(Seq<Fixed> items, float space) =>
        Layouts.Distribute(items.Map(static item => new Layouts.FlexItem(Grow.Unit with { Limits = item.Limits }, item.Limits.Clamped(item.Extent), item.Limits.Min)), space);

    internal float Floor(float content) => SwitchPartially(content, @default: static (_, held) => held.Limits.Min, fit: static (least, held) => float.Max(held.Limits.Min, least));

    public sealed record Fixed(float Extent) : Sizing {
        internal override Option<float> Definite => Some(Limits.Clamped(Extent));
    }

    public sealed record Fit : Sizing {
        public static Fit Content { get; } = new();
    }

    public sealed record Grow(Factor Weight) : Sizing {
        public static Grow Unit { get; } = new(Factor.Unit);

        internal override float GrowWeight => (float)Weight;
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
    private static readonly SKSamplingOptions Anisotropy = new(maxAniso: 16);

    public static readonly ImageRendering Auto = new(Smooth, Smooth);
    public static readonly ImageRendering HighQuality = new(new SKSamplingOptions(SKCubicResampler.Mitchell), Smooth);
    public static readonly ImageRendering CrispEdges = new(Nearest, Nearest);
    public static readonly ImageRendering Pixelated = new(Nearest, Smooth);
    public static readonly ImageRendering Sharp = new(Smooth, new SKSamplingOptions(SKCubicResampler.CatmullRom));
    public static readonly ImageRendering Anisotropic = new(Anisotropy, Anisotropy);

    private readonly SKSamplingOptions magnified;
    private readonly SKSamplingOptions minified;

    internal SKSamplingOptions Sampling(float density) => density > 1f ? magnified : minified;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImageSource {
    private ImageSource(SKImageInfo info) => Info = info;

    public SKImageInfo Info { get; }

    public sealed record Still(SKImage Image) : ImageSource(Image.Info);

    public sealed record Encoded : ImageSource {
        private Encoded(SKData data, SKCodec codec) : base(Oriented(codec.Info, codec.EncodedOrigin)) => (Data, Origin, FrameCount) = (data, codec.EncodedOrigin, codec.FrameCount);

        public SKData Data { get; }
        public SKEncodedOrigin Origin { get; }
        public int FrameCount { get; }

        public static Fin<Encoded> Validated(SKData data) {
            using SKCodec? codec = SKCodec.Create(data);
            return codec is { } read
                ? new Encoded(data, read)
                : new UIFault(string.Create(CultureInfo.InvariantCulture, $"{nameof(data)}.{nameof(data.Size)} = {data.Size}"), UIReason.Unreadable);
        }

        private static SKImageInfo Oriented(SKImageInfo info, SKEncodedOrigin origin) =>
            origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom ? info.WithSize(info.Height, info.Width) : info;
    }
}

[SmartEnum]
internal sealed partial class Painting {
    public static readonly Painting Frame = new(paints: false, acceptsBackground: true, inherits: false);
    public static readonly Painting Figure = new(paints: true, acceptsBackground: false, inherits: false);
    public static readonly Painting Foreground = new(paints: true, acceptsBackground: true, inherits: true);
    public static readonly Painting Hosted = new(paints: true, acceptsBackground: true, inherits: false);

    internal bool Paints { get; }
    internal bool AcceptsBackground { get; }
    internal bool Inherits { get; }
}

internal sealed record Art(Assembly Assembly, string Resource, bool Tinted) {
    internal Option<SKColor> Current { get; init; }
    internal HashMap<string, ColorValue> Classes { get; init; }
    internal bool Animated { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GlyphSource {
    public HashMap<string, ColorValue> Classes { get; init; }

    internal Art Resolved(SKColorF under) => Drawn(under) with { Classes = Classes };

    private protected abstract Art Drawn(SKColorF under);

    public sealed record Tinted(Assembly Assembly, string Resource) : GlyphSource {
        private protected override Art Drawn(SKColorF under) => new(Assembly, Resource, Tinted: true);
    }

    public sealed record Paired(Assembly Assembly, string Light, string Dark) : GlyphSource {
        private static readonly Unicolour White = new(ColourSpace.Rgb, 1.0, 1.0, 1.0);
        private static readonly Unicolour Black = new(ColourSpace.Rgb, 0.0, 0.0, 0.0);

        public string Resource(SKColorF under) {
            Unicolour held = under.ToUnicolour();
            return held.Contrast(White) > held.Contrast(Black) ? Dark : Light;
        }

        private protected override Art Drawn(SKColorF under) => new(Assembly, Resource(under), Tinted: false);
    }

    public sealed record Motion(Assembly Assembly, string Resource) : GlyphSource {
        private protected override Art Drawn(SKColorF under) => new(Assembly, Resource, Tinted: false) { Animated = true };
    }
}

public sealed record Span(string Text) {
    public Option<Role> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public Option<CultureInfo> Language { get; init; }
    public Option<Part> Part { get; init; }

    public static implicit operator Span(string text) => FromString(text);

    public static Span FromString(string text) => new(text);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SlotKind {
    public sealed record Text(string Value, Option<int> MaxLines, Func<Edit<string>, IO<Unit>> Edited) : SlotKind;

    public sealed record Native : SlotKind;
}

public sealed record Expiry(Duration Lifetime, IO<Unit> Elapsed);

[SmartEnum]
public sealed partial class Baseline {
    public static readonly Baseline First = new(0, Fraction.MinValue);
    public static readonly Baseline Last = new(^1, Fraction.MinValue);
    public static readonly Baseline Cap = new(0, Fraction.MaxValue);

    internal Index Line { get; }
    internal Fraction Rise { get; }
}

[SmartEnum]
public sealed partial class Truncation {
    public static readonly Truncation End = new(Fraction.MaxValue);
    public static readonly Truncation Middle = new(Fraction.Half);
    public static readonly Truncation Start = new(Fraction.MinValue);

    internal Fraction Head { get; }
}

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
    public Option<Matrix4x4> Transform { get; init; }
    public Option<PhysicalAxis> Sticks { get; init; }
    public Option<Part> Part { get; init; }
    public Option<Lift> Lift { get; init; }
    public Option<Drop> Drop { get; init; }
    public Option<Uri> Link { get; init; }
    public Option<Expiry> Expires { get; init; }
    public Accessible Accessible { get; init; } = Accessible.Empty;

    internal virtual Option<Node> Inner => None;

    internal virtual Option<PlotPlane> Plotted => None;

    internal virtual Option<Func<Option<Vector2>, IO<Unit>>> Hovering => None;

    internal virtual Option<Lazy> Windowed => None;

    internal Option<Pin> Pinned => SwitchPartially(@default: static node => node.Inner.Bind(static inner => inner.Pinned), pin: static pin => Some(pin));

    internal virtual Option<Figure> Traced => None;

    internal virtual Option<GlyphSource> GlyphArt => None;

    internal virtual Option<Styled> StyledNode => None;

    internal virtual Option<Plot> PlotNode => None;

    internal virtual Option<Layer> LayerNode => None;

    internal virtual Option<PhysicalAxis> MainAxis => None;

    internal float Alpha => StyledNode.Map(static styled => (float)styled.Opacity).IfNone(1f);

    internal Painting Painting { get; }

    internal bool Clips(Node child) => SwitchPartially(child, @default: static (_, _) => false, scroll: static (_, _) => true, plot: static (held, _) => held.Pinned.IsNone);

    internal Node Conditioned(Condition condition) =>
        StyledNode.Match(Some: styled => styled with { Content = styled.Content.Conditioned(condition) }, None: () => this with { Conditions = Conditions.TryAdd(condition) });

    public sealed record Box() : Node(Painting.Frame);

    public sealed record Flex(PhysicalAxis Axis, Seq<Node> Children) : Node(Painting.Frame) {
        public Option<Baseline> Baseline { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Grid(Tracks Columns, Seq<Seq<Node>> Rows) : Node(Painting.Frame);

    public sealed record Lazy(PhysicalAxis Axis, Tracks Across, float Extent, Seq<Key> Items) : Node(Painting.Frame) {
        public HashMap<Key, Node> Realized { get; init; }

        internal override Option<Lazy> Windowed => Some(this);
        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Overlay(Node Base, Seq<Node> Above) : Node(Painting.Frame) {
        internal override Option<Node> Inner => Some(Base);
    }

    public sealed record Layer(Option<Seq<Key>> Anchor, Node Content) : Node(Painting.Frame) {
        public UnitPoint Side { get; init; } = UnitPoint.Bottom;
        public float Gap { get; init; }
        public bool Flips { get; init; } = true;
        public Option<IO<Unit>> Dismissed { get; init; }
        public bool Modal { get; init; }
        public Option<Duration> Hover { get; init; }

        internal override Option<Node> Inner => Some(Content);
        internal override Option<Layer> LayerNode => Some(this);
    }

    public sealed record Choice(PhysicalAxis Axis, Seq<Node> Alternatives) : Node(Painting.Frame) {
        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Scroll(PhysicalAxis Axis, Node Content) : Node(Painting.Frame) {
        public Option<UnitPoint> Snap { get; init; }
        public Option<Seq<Key>> Follows { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Plot(PlotPlane Plane, Seq<Node> Children) : Node(Painting.Frame) {
        public Seq<Guide> Guides { get; init; }
        public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }
        public Option<Func<Option<Nearest>, IO<Unit>>> Pointed { get; init; }
        public Option<Func<Nearest, Node>> Crosshair { get; init; }

        internal override Option<PlotPlane> Plotted => Some(Plane);
        internal override Option<Func<Option<Vector2>, IO<Unit>>> Hovering => Hovered;
        internal override Option<Plot> PlotNode => Some(this);
    }

    public sealed record Pin(Vector2 At, Node Content) : Node(Painting.Frame) {
        public Option<Vector2> Span { get; init; }

        internal override Option<Node> Inner => Some(Content);
    }

    public sealed record Mark(Figure Figure) : Node(Painting.Figure) {
        public Option<DataSeries> Series { get; init; }

        internal override Option<Figure> Traced => Some(Figure);
    }

    public sealed record Text(Seq<Span> Spans) : Node(Painting.Foreground) {
        public string Content => string.Concat(Spans.Map(static span => span.Text));
        public Option<int> MaxLines { get; init; }
        public Option<float> MaxHeight { get; init; }
        public Truncation Truncation { get; init; } = Truncation.End;
        public TextAlignment Alignment { get; init; } = TextAlignment.Auto;
        public Option<TextRange> Selected { get; init; }
    }

    public sealed record Glyph(GlyphSource Art) : Node(Painting.Foreground) {
        internal override Option<GlyphSource> GlyphArt => Some(Art);
    }

    public sealed record Image(ImageSource Source) : Node(Painting.Figure) {
        public ImageFit Fit { get; init; } = ImageFit.Stretch;
        public float Density { get; init; } = 1f;
        public ImageRendering Rendering { get; init; } = ImageRendering.Auto;
        public Option<DataSeries> Series { get; init; }
        public Option<ColorRamp> Scale { get; init; }
    }

    public sealed record Slot(SlotKind Kind) : Node(Painting.Hosted);

    public sealed record Styled(Node Content) : Node(Painting.Frame) {
        public StyleSheet Sheet { get; init; } = StyleSheet.Empty;
        public Fraction Opacity { get; init; } = Fraction.MaxValue;

        internal override Option<Node> Inner => Some(Content);
        internal override Option<Styled> StyledNode => Some(this);
    }
}
