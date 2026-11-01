using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Rasm.UI.Motion;
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

    internal float End(SKRect rect) => Of(new SKPoint(rect.Right, rect.Bottom));
}

[ValueObject<float>(SkipFactoryMethods = true)]
public sealed partial class Fraction : IInterpolated<Fraction> {
    public static Fraction MinValue { get; } = new(0f);
    public static Fraction Half { get; } = new(0.5f);
    public static Fraction MaxValue { get; } = new(1f);

    public Fraction Complement => new(1f - _value);

    public Fraction Midpoint(Fraction other) => new((_value + other._value) / 2f);

    public Option<Fraction> Toward(Fraction to, float progress) => new Fraction(float.Clamp(float.Lerp(_value, to._value, progress), 0f, 1f));

    public static Seq<Fraction> Partition(int count) => toSeq(Range(0, count)).Map(index => new Fraction(index / float.Max(count - 1, 1)));

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

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class TextInfoExtensions {
    extension(TextInfo text) {
        internal string Listed(Seq<string> items) => string.Join($"{text.ListSeparator} ", items);
    }
}

internal static class TransformExtensions {
    extension(SKMatrix matrix) {
        internal Option<SKMatrix> Inverse => matrix.TryInvert(out SKMatrix inverted) ? Some(inverted) : None;
    }
}
