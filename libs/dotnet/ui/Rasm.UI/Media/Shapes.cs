using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Media;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Shape {
    public sealed record Rounded : Shape;

    public sealed record Capsule : Shape;

    public sealed record Outlined : Shape {
        private Outlined(string pathData) => PathData = pathData;

        public string PathData { get; }

        public static Fin<Outlined> Validated(string pathData) {
            using SKPath? parsed = SKPath.ParseSvgPathData(pathData);
            return Optional(parsed).Filter(static path => !path.Bounds.IsEmpty).Map(static path => new Outlined(path.ToSvgPathData())).ToFin(new UIFault(pathData, UIReason.Unreadable));
        }

        internal SKPath Path() => SKPath.ParseSvgPathData(PathData);
    }

    public sealed record Icon(IconSet Set, Outlined Outline) : Shape {
        public View Drawn => new View.Box { Role = Role.Icon, Sheet = new StyleSheet(HashMap((Role.Icon, new Look { Shape = this }))) };
    }

    public sealed record Placed(Shape Inner, SKRect Unit) : Shape;

    public sealed record Combined(Shape First, SKPathOp Op, Shape Second) : Shape;
}

[SmartEnum]
public sealed partial class IconSet {
    public static readonly IconSet Lucide = new(stem: 2f);
    public static readonly IconSet Tabler = new(stem: 2f);
    public static readonly IconSet Fluent = new(stem: 1f);
    public static readonly IconSet MaterialCommunity = new(stem: 2f);

    public float Stem { get; }
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Corners {
    public static Corners Square { get; } = new(0f, 0f, 0f, 0f);

    public float TopLeft { get; }
    public float TopRight { get; }
    public float BottomRight { get; }
    public float BottomLeft { get; }

    public Option<float> Uniform => Seq(TopRight, BottomRight, BottomLeft).ForAll(radius => radius.CompareTo(TopLeft) is 0) ? Some(TopLeft) : None;

    internal Seq<float> Clockwise => [TopLeft, TopRight, BottomRight, BottomLeft];

    public static Fin<Corners> Validated(float topLeft, float topRight, float bottomRight, float bottomLeft) =>
        UIFault.Unless(Seq(topLeft, topRight, bottomRight, bottomLeft).ForAll(static radius => float.IsFinite(radius) && radius >= 0f), UIReason.Range, (topLeft, topRight, bottomRight, bottomLeft)).Map(_ => new Corners(topLeft, topRight, bottomRight, bottomLeft));

    internal static Corners Round(float radius) => new(radius, radius, radius, radius);

    internal Corners Within(SKRect outer, SKRect inner) {
        (float Left, float Top, float Right, float Bottom) gap =
            (float.Max(0f, inner.Left - outer.Left), float.Max(0f, inner.Top - outer.Top), float.Max(0f, outer.Right - inner.Right), float.Max(0f, outer.Bottom - inner.Bottom));
        return new(Concentric(TopLeft, gap.Left, gap.Top), Concentric(TopRight, gap.Right, gap.Top), Concentric(BottomRight, gap.Right, gap.Bottom), Concentric(BottomLeft, gap.Left, gap.Bottom));
    }

    internal Corners Fitted(SKSize size) =>
        Seq((Length: size.Width, Radii: TopLeft + TopRight), (Length: size.Height, Radii: TopRight + BottomRight), (Length: size.Width, Radii: BottomRight + BottomLeft), (Length: size.Height, Radii: BottomLeft + TopLeft))
            .Fold(1f, static (least, edge) => edge.Radii > edge.Length ? float.Min(least, edge.Length / edge.Radii) : least) switch {
                < 1f and var scale => Each(radius => radius * scale),
                _ => this,
            };

    internal Corners Grown(float by) => Each(radius => radius > 0f ? float.Max(0f, radius + by) : 0f);

    private Corners Each(Func<float, float> radius) => new(radius(TopLeft), radius(TopRight), radius(BottomRight), radius(BottomLeft));

    private static float Concentric(float outer, float across, float down) => float.Max(0f, outer - float.Max(across, down));
}
