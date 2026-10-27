using System.Globalization;
using System.Numerics;
using System.Reflection;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Sizing {
    public Limits Limits { get; init; } = Limits.Open;

    internal virtual float GrowWeight => 0f;
    internal Option<int> ShrinkTier => Map(@fixed: Option<int>.None, fit: Some(1), grow: Some(0), percent: Some(1));
    internal bool Stretches => GrowWeight > 0f;
    internal virtual Option<float> Definite => None;

    public static Seq<float> Shared(Seq<Sizing> sizes, Seq<float> content, float space) =>
        Layouts.Distribute(sizes.Zip(content).Map(held => (held.First, Layouts.Based(held.First, held.Second, space), held.First.Floor(0f))), space);

    internal float Floor(float content) => float.Max(Limits.Min, content);

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

public sealed record Tracks(Seq<Sizing> Sizes) {
    public bool AutoFill { get; init; }
}

[SmartEnum]
public sealed partial class ImageFit {
    public static readonly ImageFit Stretch = new(static (content, _) => content);
    public static readonly ImageFit Contain = new(static (content, natural) => content.AspectFit(natural));
    public static readonly ImageFit Cover = new(static (content, natural) => content.AspectFill(natural));

    [UseDelegateFromConstructor]
    public partial SKRect Fitted(SKRect content, SKSize natural);

    internal static SKMatrix Mapping(SKRect from, SKRect to) =>
        (to.Width / from.Width, to.Height / from.Height) switch {
            var (x, y) => SKMatrix.CreateScaleTranslation(x, y, to.Left - (from.Left * x), to.Top - (from.Top * y)),
        };
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
        private Encoded(SKData data, SKCodec codec)
            : base(Transposed(codec.EncodedOrigin) ? codec.Info.WithSize(codec.Info.Height, codec.Info.Width) : codec.Info) => (Data, FrameCount) = (data, codec.FrameCount);

        public SKData Data { get; }
        public int FrameCount { get; }

        public static Fin<Encoded> Validated(SKData data) {
            using SKCodec? codec = SKCodec.Create(data);
            return codec is { } read
                ? new Encoded(data, read)
                : new UIFault(string.Create(CultureInfo.InvariantCulture, $"{nameof(data)}.{nameof(data.Size)} = {data.Size}"), UIReason.Unreadable);
        }

        internal static bool Transposed(SKEncodedOrigin origin) => origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
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

public sealed record GlyphSource {
    public GlyphSource(Assembly assembly, string resource) =>
        (Assembly, Resource, Dark) = (assembly, resource, $"{Path.ChangeExtension(resource, extension: null)}_darkmode{Path.GetExtension(resource)}" switch {
            var dark => Optional(assembly.GetManifestResourceInfo(dark)).Map(_ => dark),
        });

    public Assembly Assembly { get; }
    public string Resource { get; }
    public bool Tinted { get; init; }
    public HashMap<string, ColorValue> Classes { get; init; }

    internal Option<string> Dark { get; }

    internal bool Animated => Path.GetExtension(Resource).Equals(".json", StringComparison.OrdinalIgnoreCase);

    internal string Shown(SKColorF under) =>
        under.ToUnicolour() switch {
            var held => held.Contrast(Css.White) > held.Contrast(Css.Black) ? Dark.IfNone(Resource) : Resource,
        };
}

public sealed record Span(string Text) {
    public Option<Role> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public Option<CultureInfo> Language { get; init; }
    public Option<Part> Part { get; init; }

    public static implicit operator Span(string text) => FromString(text);

    public static Span FromString(string text) => new(text);
}

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
    private Node(Option<Painting> painting = default) => Painting = painting.IfNone(Painting.Frame);

    public Option<Key> Key { get; init; }
    public Option<Role> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public StyleSheet Sheet { get; init; } = StyleSheet.Empty;
    public Fraction Opacity { get; init; } = Fraction.MaxValue;
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
    public Option<(Duration Lifetime, IO<Unit> Elapsed)> Expires { get; init; }
    public Accessible Accessible { get; init; } = Accessible.Empty;

    public Node Conditioned(Condition condition) => this with { Conditions = Conditions.TryAdd(condition) };

    internal Painting Painting { get; }

    internal virtual Option<Node> Inner => None;

    internal virtual Option<PhysicalAxis> MainAxis => None;

    internal virtual Option<PlotPlane> Plotted => None;

    internal virtual Option<Figure> Traced => None;

    internal Option<Pin> Pinned => SwitchPartially(@default: static node => node.Inner.Bind(static inner => inner.Pinned), pin: static pin => Some(pin));

    internal virtual bool Clips(Node child) => false;

    public sealed record Box : Node;

    public sealed record Flex(PhysicalAxis Axis, Seq<Node> Children) : Node {
        public Option<Baseline> Baseline { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Grid(Tracks Columns, Seq<Seq<Node>> Rows) : Node;

    public sealed record Lazy(PhysicalAxis Axis, Tracks Across, float Extent, Seq<Key> Items) : Node {
        public HashMap<Key, Node> Realized { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Overlay(Node Base, Seq<Node> Above) : Node {
        internal override Option<Node> Inner => Some(Base);
    }

    public sealed record Layer(Option<Seq<Key>> Anchor, Node Content) : Node {
        public UnitPoint Side { get; init; } = UnitPoint.Bottom;
        public float Gap { get; init; }
        public bool Flips { get; init; } = true;
        public Option<IO<Unit>> Dismissed { get; init; }
        public bool Modal { get; init; }
        public Option<Duration> Hover { get; init; }

        internal override Option<Node> Inner => Some(Content);
    }

    public sealed record Choice(PhysicalAxis Axis, Seq<Node> Alternatives) : Node {
        internal override Option<PhysicalAxis> MainAxis => Some(Axis);
    }

    public sealed record Scroll(PhysicalAxis Axis, Node Content) : Node {
        public Option<UnitPoint> Snap { get; init; }
        public Option<Seq<Key>> Follows { get; init; }

        internal override Option<PhysicalAxis> MainAxis => Some(Axis);

        internal override bool Clips(Node child) => true;
    }

    public sealed record Plot(PlotPlane Plane, Seq<Node> Children) : Node {
        public Seq<Guide> Guides { get; init; }
        public Option<Func<Option<Vector2>, IO<Unit>>> Hovered { get; init; }
        public Option<Func<Option<Nearest>, IO<Unit>>> Pointed { get; init; }
        public Option<Func<Nearest, Node>> Crosshair { get; init; }

        internal override Option<PlotPlane> Plotted => Some(Plane);

        internal override bool Clips(Node child) => child.Pinned.IsNone;
    }

    public sealed record Pin(Vector2 At, Node Content) : Node {
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
        public Option<Func<Edit<string>, IO<Unit>>> Edited { get; init; }
    }

    public sealed record Glyph(GlyphSource Art) : Node(Painting.Foreground);

    public sealed record Image(ImageSource Source) : Node(Painting.Figure) {
        public ImageFit Fit { get; init; } = ImageFit.Stretch;
        public Factor Density { get; init; } = Factor.Unit;
        public ImageRendering Rendering { get; init; } = ImageRendering.Auto;
        public Option<DataSeries> Series { get; init; }
        public Option<ColorRamp> Scale { get; init; }
    }

    public sealed record Slot() : Node(Painting.Hosted);
}
