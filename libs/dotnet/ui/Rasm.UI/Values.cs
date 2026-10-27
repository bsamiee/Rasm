using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>(SkipIComparable = true)]
public sealed partial class PhysicalAxis {
    public static readonly PhysicalAxis Horizontal = new(0);
    public static readonly PhysicalAxis Vertical = new(1);

    public PhysicalAxis Cross => Map(horizontal: Vertical, vertical: Horizontal);

    public float Of(Vector2 value) => value[Key];

    public float Of(SKSize size) => Of(size.ToPoint());

    public Vector2 With(Vector2 value, float along) => value.WithElement(Key, along);

    public SKRect Spanned(SKRect frame, float start, float extent) => SKRect.Create(With(frame.Location, start), new SKSize(With(frame.Size.ToPoint(), extent)));
}

[ValueObject<float>(SkipFactoryMethods = true)]
public sealed partial class Fraction {
    public static Fraction MinValue { get; } = new(0f);
    public static Fraction Half { get; } = new(0.5f);
    public static Fraction MaxValue { get; } = new(1f);

    public Fraction Complement => new(1f - _value);

    public Fraction Midpoint(Fraction other) => new((_value + other._value) / 2f);

    public static Fin<Fraction> Validated(float value) => UIFault.Unless(value is >= 0f and <= 1f, UIReason.Range, value).Map(_ => new Fraction(value));
}

[ValueObject<float>(SkipFactoryMethods = true)]
public sealed partial class Factor {
    public static Factor Unit { get; } = new(1f);

    public static Fin<Factor> Validated(float value) => UIFault.Unless(float.IsFinite(value) && value > 0f, UIReason.Range, value).Map(_ => new Factor(value));
}

public sealed record UnitPoint(Fraction X, Fraction Y) {
    public static UnitPoint Start { get; } = new(Fraction.MinValue, Fraction.MinValue);
    public static UnitPoint Center { get; } = new(Fraction.Half, Fraction.Half);
    public static UnitPoint Bottom { get; } = new(Fraction.Half, Fraction.MaxValue);

    public float Of(PhysicalAxis along) => along.Of(new Vector2((float)X, (float)Y));
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Limits {
    public static Limits Open { get; } = new(0f, float.PositiveInfinity);

    public float Min { get; }
    public float Max { get; }

    public float Clamped(float value) => float.Clamp(value, Min, Max);

    public static Fin<Limits> Validated(float min, float max) => UIFault.Unless(float.IsFinite(min) && min >= 0f && max >= min, UIReason.Order, (min, max)).Map(_ => new Limits(min, max));
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Corners {
    public static Corners Square { get; } = new(0f, 0f, 0f, 0f);

    public float TopLeft { get; }
    public float TopRight { get; }
    public float BottomRight { get; }
    public float BottomLeft { get; }

    public Option<float> Uniform => Seq(TopRight, BottomRight, BottomLeft).ForAll(radius => radius.CompareTo(TopLeft) is 0) ? Some(TopLeft) : None;

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
                < 1f and var scale => new(TopLeft * scale, TopRight * scale, BottomRight * scale, BottomLeft * scale),
                _ => this,
            };

    internal Corners Grown(float by) => new(Grown(TopLeft, by), Grown(TopRight, by), Grown(BottomRight, by), Grown(BottomLeft, by));

    internal bool Holds(SKRect frame, float reach, SKPoint at) {
        SKRect grown = SKRect.Inflate(frame, reach, reach);
        Corners radii = Fitted(frame.Size).Grown(reach);
        Vector2 middle = new(grown.MidX, grown.MidY);
        Vector2 half = new(grown.Width / 2f, grown.Height / 2f);
        return grown.Contains(at)
            && Seq((Radius: radii.TopLeft, Sign: new Vector2(-1f, -1f)), (Radius: radii.TopRight, Sign: new Vector2(1f, -1f)), (Radius: radii.BottomRight, Sign: Vector2.One), (Radius: radii.BottomLeft, Sign: new Vector2(-1f, 1f)))
                .ForAll(corner => ((((Vector2)at - middle) * corner.Sign) - half + new Vector2(corner.Radius)) switch {
                    var beyond => beyond.X <= 0f || beyond.Y <= 0f || beyond.LengthSquared() <= corner.Radius * corner.Radius,
                });
    }

    private static float Concentric(float outer, float across, float down) => float.Max(0f, outer - float.Max(across, down));

    private static float Grown(float radius, float by) => radius > 0f ? float.Max(0f, radius + by) : 0f;
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Magnification {
    public static Magnification Free { get; } = new(float.Epsilon, float.PositiveInfinity);

    public float Min { get; }
    public float Max { get; }

    public static Fin<Magnification> Validated(float least, float most) =>
        UIFault.Unless(float.IsFinite(least) && least > 0f && most >= least, UIReason.Order, (least, most)).Map(_ => new Magnification(least, most));
}

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class UIReason {
    public static readonly UIReason Range = new("value out of range");
    public static readonly UIReason Order = new("values out of order");
    public static readonly UIReason Missing = new("missing entry");
    public static readonly UIReason Derived = new("states a derived fact");
    public static readonly UIReason Repeated = new("repeated entry");
    public static readonly UIReason Unreadable = new("unreadable");
    public static readonly UIReason Refused = new("refused by the renderer");

    public string Message { get; }
}

public sealed record UIFault(string Subject, UIReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject} at index {at}"), () => $"{Reason.Message}: {Subject}"), 0, Inner) {
    public static Fin<Unit> Unless<T>(bool flag, UIReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new UIFault(string.Create(CultureInfo.InvariantCulture, $"{names} = {rejected}"), reason, index);
}
