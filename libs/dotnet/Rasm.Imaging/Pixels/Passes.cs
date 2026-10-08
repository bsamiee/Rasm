using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using UnitsNet;
using UnitsNet.Units;
using Wacton.Unicolour;

namespace Rasm.Imaging.Pixels;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IStateParameterVisitor<TRecord, out TResult> {
    public TResult Bounded<TValue, TKey, TError>(Lens<TRecord, TValue> lens, Presentation<TValue, TKey> presentation)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError>;

    public TResult OptionalBounded<TValue, TKey, TError>(Lens<TRecord, Option<TValue>> lens, Presentation<TValue, TKey> presentation)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError>;

    public TResult Choice<TValue, TError>(Lens<TRecord, TValue> lens)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError>;

    public TResult OptionalChoice<TValue, TError>(Lens<TRecord, Option<TValue>> lens)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError>;

    public TResult Variant<TValue, TCase, TError>(Lens<TRecord, TValue> lens)
        where TValue : class
        where TCase : class, IStateCase<TValue>, ISmartEnum<string, TCase, TError>
        where TError : Error, IValidationError<TError>;

    public TResult Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum;

    public TResult Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested>;

    public TResult OptionalRecord<TNested>(Lens<TRecord, Option<TNested>> lens) where TNested : IStateRecord<TNested>;

    public TResult Toggle(Lens<TRecord, bool> lens);

    public TResult Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw>;

    public TResult OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw>;

    public TResult Color(Lens<TRecord, Swatch> lens);

    public TResult OptionalColor(Lens<TRecord, Option<Swatch>> lens);

    public TResult Gradient(Lens<TRecord, Ramp> lens);

    public TResult Swatches<TValue, TError>(Lens<TRecord, TValue> lens)
        where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError>;

    public TResult Keyed<TValue, TRaw, TError>(Lens<TRecord, TValue> lens)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError>;

    public TResult OptionalKeyed<TValue, TRaw, TError>(Lens<TRecord, Option<TValue>> lens)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError>;

    public TResult Loaded<TValue, TKey, TRaw, TError>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
        where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError>;

    public TResult Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull;
}

public interface IStateCaseVisitor<TUnion, out TResult> where TUnion : class {
    public TResult Case<TCase>() where TCase : class, TUnion, IStateRecord<TCase>;
}

public interface IStateCase<TUnion> where TUnion : class {
    public TResult Accept<TResult>(IStateCaseVisitor<TUnion, TResult> visitor);
}

public interface IStateParameter<TRecord> {
    public string Key { get; }
    public StateParameter<TRecord> Kind { get; }
}

public interface IStateRecord<TSelf> where TSelf : IStateRecord<TSelf> {
    public static abstract TSelf Default { get; }
    public static virtual IReadOnlyList<IStateParameter<TSelf>> Parameters => [];
    public static virtual Option<IStateParameter<TSelf>> FindParameter(string key) => None;
}

public interface IStateRecord<TSelf, TParameter, TError> : IStateRecord<TSelf>
    where TSelf : IStateRecord<TSelf, TParameter, TError>
    where TParameter : class, IStateParameter<TSelf>, ISmartEnum<string, TParameter, TError>
    where TError : Error, IValidationError<TError> {
    static IReadOnlyList<IStateParameter<TSelf>> IStateRecord<TSelf>.Parameters => TParameter.Items;

    static Option<IStateParameter<TSelf>> IStateRecord<TSelf>.FindParameter(string key) =>
        TParameter.TryGet(key, out TParameter? parameter) ? Some<IStateParameter<TSelf>>(parameter) : None;
}

public interface IPixelStage<TSelf> : IStateRecord<TSelf> where TSelf : IPixelStage<TSelf> {
    public static abstract Option<PixelPass> Pass(TSelf state, PassContext context);

    public static virtual Seq<GuideChannel> Channels(TSelf state) => [];
}

public interface IFrameJob<TSelf, TState> : IDisposable
    where TSelf : IFrameJob<TSelf, TState>
    where TState : IStateRecord<TState> {
    public static abstract IO<TSelf> Open();

    public static abstract Seq<GuideChannel> Channels(TState state);

    public IO<PixelFrame> Run(TState state, PixelFrame color, HashMap<GuideChannel, PixelFrame> guides, IProgress<int> rows);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GuideChannel {
    public static readonly GuideChannel Depth = new();
    public static readonly GuideChannel Normal = new();
    public static readonly GuideChannel Albedo = new();
    public static readonly GuideChannel ObjectId = new();
    public static readonly GuideChannel MaterialId = new();
}

public sealed record SceneLights(Option<Vector3> Sun, HashMap<Guid, Vector3> Points) {
    public static SceneLights Empty { get; } = new(None, HashMap<Guid, Vector3>());
}

public readonly record struct PassContext(
    PixelExtent Extent, Gamut Working, Display Display, OutputDepth Depth, Transfer Signal, Exposure Exposure,
    Option<Camera> Camera, HashMap<GuideChannel, PixelFrame> Guides, Option<TimeSpan> Time, SceneLights Lights, Derivations Derivations);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PixelPass {
    public abstract Fin<Unit> Run(PixelFrame frame, IProgress<int> progress);

    public sealed record Color : PixelPass {
        public Color(Action<Span<Vector4>> row) => (Row, Table) = (row, None);
        public Color(LutTable table) => (Row, Table) = (table.Apply, table);

        public Action<Span<Vector4>> Row { get; }
        public Option<LutTable> Table { get; }

        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => RowAction.For(frame, (row, _, _) => Row(row), progress);
    }

    public sealed record Pointwise(Action<Span<Vector4>, int, int> Row) : PixelPass {
        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => RowAction.For(frame, Row, progress);
    }

    public sealed record Frame(Func<PixelFrame, IProgress<int>, Fin<Unit>> Kernel) : PixelPass {
        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => Kernel(frame, progress);
    }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TrackScale {
    public static readonly TrackScale Linear = new(
        static (key, low, high) => (key - low) / (high - low),
        static (position, low, high) => low + (position * (high - low)));
    public static readonly TrackScale Log = new(
        static (key, low, high) => double.Max(low, LogFloor) switch { var floor => double.MaxNumber(0, Math.Log(key / floor, high / floor)) },
        static (position, low, high) => double.Max(low, LogFloor) switch { var floor => floor * Math.Pow(high / floor, position) });
    public static readonly TrackScale Cubic = new(
        static (key, low, high) =>
            (Math.Cbrt(((key - low) * ((high * high * high) - (low * low * low)) / (high - low)) + (low * low * low)) - low) / (high - low),
        static (position, low, high) =>
            ((Math.Pow(low + (position * (high - low)), 3) - (low * low * low)) * (high - low) / ((high * high * high) - (low * low * low))) + low);

    private const double LogFloor = 0.5e-8;

    [UseDelegateFromConstructor]
    public partial double Position(double key, double low, double high);

    [UseDelegateFromConstructor]
    public partial double Key(double position, double low, double high);
}

[SmartEnum]
public sealed partial class NumberForm {
    public static readonly NumberForm Track = new();
    public static readonly NumberForm Field = new();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct RampPosition : IMinMaxValue<RampPosition> {
    public static RampPosition MinValue { get; } = new(0d);
    public static RampPosition MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ComplexValueObject]
[ValidationError<InvalidPixelValue>]
public readonly partial struct RampStop {
    public static RampStop Black { get; } = new(RampPosition.MinValue, new Vector4(0f, 0f, 0f, 1f));
    public static RampStop White { get; } = new(RampPosition.MaxValue, Vector4.One);

    public RampPosition Position { get; }
    public Vector4 Color { get; }

    public RampStop At(RampPosition position) => new(position, Color);

    public static Option<RampStop> Parse(string text) =>
        text.Split(' ') is [var at, var red, var green, var blue, var alpha]
            ? from offset in Invariant.Number<double>(at)
              from r in Invariant.Number<float>(red)
              from g in Invariant.Number<float>(green)
              from b in Invariant.Number<float>(blue)
              from a in Invariant.Number<float>(alpha)
              from stop in RampPosition.Validate(offset, provider: null, out RampPosition position) is null && Validate(position, new Vector4(r, g, b, a), out RampStop item) is null ? Some(item) : None
              select stop
            : None;

    public string Format() => string.Create(CultureInfo.InvariantCulture, $"{(double)Position} {Color.X} {Color.Y} {Color.Z} {Color.W}");

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref RampPosition position, ref Vector4 color) =>
        validationError = Vector4.AllWhereAllBitsSet(Vector4.IsFinite(color)) && color.W.CompareTo(0f) >= 0 && color.W.CompareTo(1f) <= 0 ? null : new InvalidPixelValue();
}

[SmartEnum<string>]
[ValidationError<InvalidPixelValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RampInterpolation {
    public static readonly RampInterpolation Linear = new("linear", ColourSpace.Oklab, static t => t);
    public static readonly RampInterpolation Ease = new("ease", ColourSpace.Oklab, static t => t * t * (3d - (2d * t)));
    public static readonly RampInterpolation Constant = new("constant", ColourSpace.Oklab, static _ => 0d);
    public static readonly RampInterpolation LinearRgb = new("linear-rgb", ColourSpace.RgbLinear, Linear.Weight);
    public static readonly RampInterpolation EaseRgb = new("ease-rgb", ColourSpace.RgbLinear, Ease.Weight);
    public static readonly RampInterpolation SmootherRgb = new("smoother-rgb", ColourSpace.RgbLinear, static t => t * t * t * ((t * ((6d * t) - 15d)) + 10d));

    public ColourSpace Space { get; }

    [UseDelegateFromConstructor]
    public partial double Weight(double t);
}

[ComplexValueObject]
[ValidationError<InvalidPixelValue>]
[ObjectFactory<string>]
public sealed partial class Ramp : IConvertible<string> {
    private static readonly Func<(Ramp Ramp, Gamut Gamut), RampTable> Tables = memo(static ((Ramp Ramp, Gamut Gamut) key) => {
        const int texels = 256;
        Seq<RampStop> stops = key.Ramp.Stops;
        RampInterpolation interpolation = key.Ramp.Interpolation;
        if (interpolation.Space == ColourSpace.RgbLinear)
            return new RampTable(stops, (stop, t) => Vector4.Lerp(stops[stop].Color, stops[stop + 1].Color, (float)interpolation.Weight(t)));
        Seq<Unicolour> entered = stops.Map(stop =>
            new Unicolour(key.Gamut.Configuration, ColourSpace.RgbLinear, stop.Color.X, stop.Color.Y, stop.Color.Z, stop.Color.W).ConvertToConfiguration(Configuration.Default));
        Vector4[] samples =
            [.. from pair in entered.Zip(entered.Tail)
             from i in toSeq(Range(0, texels + 1))
             let mixed = pair.First.Mix(pair.Second, interpolation.Space, interpolation.Weight(i / (double)texels)).ConvertToConfiguration(key.Gamut.Configuration)
             select new Vector4((float)mixed.RgbLinear.R, (float)mixed.RgbLinear.G, (float)mixed.RgbLinear.B, (float)mixed.Alpha.A)];
        return new RampTable(stops, (stop, t) => {
            double position = t * texels;
            int at = int.Min((int)position, texels - 1);
            int offset = (stop * (texels + 1)) + at;
            return Vector4.Lerp(samples[offset], samples[offset + 1], (float)(position - at));
        });
    });

    public Seq<RampStop> Stops { get; }
    public RampInterpolation Interpolation { get; }

    public static Ramp Grayscale { get; } = new(Seq(RampStop.Black, RampStop.White), RampInterpolation.Linear);

    public (Ramp Ramp, int Index) Moved(int index, RampPosition position) =>
        (Stops.Take(index).Concat(Stops.Skip(index + 1)),
         Stops.Take(index).Filter(stop => stop.Position <= position).Count + Stops.Skip(index + 1).Filter(stop => stop.Position < position).Count) switch {
             var (rest, at) => (new Ramp(rest.Take(at).Add(Stops[index].At(position)).Concat(rest.Skip(at)), Interpolation), at),
         };

    public RampTable Tabulate(Gamut gamut) => Tables((this, gamut));

    public string ToValue() => string.Join(';', Interpolation.Key.Cons(Stops.Map(static stop => stop.Format())));

    public static InvalidPixelValue? Validate(string? value, IFormatProvider? provider, out Ramp? item) {
        item = null;
        return value is null ? null
            : value.Split(';') is [var key, .. var fields] && RampInterpolation.TryGet(key, out RampInterpolation? interpolation) && toSeq(fields).Traverse(static field => RampStop.Parse(field)).As().Case is Seq<RampStop> stops
                ? Validate(stops, interpolation, out item)
                : new InvalidPixelValue();
    }

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref Seq<RampStop> stops, ref RampInterpolation interpolation) =>
        validationError = stops.Count is >= 1 and <= 32 && stops.Zip(stops.Tail).ForAll(static pair => pair.First.Position <= pair.Second.Position) ? null : new InvalidPixelValue();
}

public sealed class RampTable {
    private readonly Seq<RampStop> stops;
    private readonly Func<int, double, Vector4> segment;

    internal RampTable(Seq<RampStop> stops, Func<int, double, Vector4> segment) => (this.stops, this.segment) = (stops, segment);

    public Vector4 Sample(double position) =>
        (double.IsNaN(position), ~stops.AsSpan().BinarySearch(new Above(position)) - 1) switch {
            (true, _) => new Vector4(float.NaN),
            (_, < 0) => stops[0].Color,
            (_, var stop) when stop == stops.Count - 1 => stops[stop].Color,
            (_, var stop) => segment(stop, (position - stops[stop].Position) / (stops[stop + 1].Position - stops[stop].Position)),
        };

    private readonly record struct Above(double Position) : IComparable<RampStop> {
        public int CompareTo(RampStop other) => other.Position <= Position ? 1 : -1;
    }
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct Swatch : IMinMaxValue<Swatch> {
    public static Swatch MinValue => Black;
    public static Swatch MaxValue => White;
    public static Swatch Black { get; } = new(0x000000);
    public static Swatch White { get; } = new(0xFFFFFF);
    public static Swatch Backdrop { get; } = new(0xA0A0A0);

    public Vector4 Display => new Vector4((_value >> 16) & 0xFF, (_value >> 8) & 0xFF, _value & 0xFF, byte.MaxValue) / byte.MaxValue;

    public Vector3 SceneLight(Gamut working) =>
        new Unicolour(Configuration.Default, ColourSpace.Rgb255, (_value >> 16) & 0xFF, (_value >> 8) & 0xFF, _value & 0xFF).ConvertToConfiguration(working.Configuration).RgbLinear switch {
            var linear => new((float)linear.R, (float)linear.G, (float)linear.B),
        };

    public static Swatch From(Vector4 display) =>
        Vector4.Truncate(Vector4.Clamp((byte.MaxValue * display) + Vector4.Create(0.5f), Vector4.Zero, Vector4.Create(byte.MaxValue))) switch {
            var code => new(((int)code.X << 16) | ((int)code.Y << 8) | (int)code.Z),
        };

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

public sealed record Presentation<TValue, TKey>
    where TValue : IMinMaxValue<TValue>, IConvertible<TKey>
    where TKey : struct, INumber<TKey> {
    public NumberForm Form { get; init; } = NumberForm.Track;
    public Option<UnitInfo> Unit { get; init; }
    public (TKey Low, TKey High) Soft { get; init; } = (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue());
    public Option<TKey> Origin { get; init; }
    public TrackScale Scale { get; init; } = TrackScale.Linear;
    public Option<TKey> Step { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<Ramp> Stops { get; init; }

    public (TKey Low, TKey High) Track(TKey key) =>
        (key < Soft.Low ? key - TKey.Min(Soft.High - key, key - TValue.MinValue.ToValue()) : Soft.Low,
         key > Soft.High ? key + TKey.Min(key - Soft.Low, TValue.MaxValue.ToValue() - key) : Soft.High);

    public double Position(TKey key, (TKey Low, TKey High) track) =>
        Scale.Position(double.CreateChecked(key), double.CreateChecked(track.Low), double.CreateChecked(track.High));

    public double Key(double position, (TKey Low, TKey High) track) =>
        (double.CreateChecked(track.Low), double.CreateChecked(track.High)) switch {
            var (low, high) => double.Clamp(Scale.Key(position, low, high), low, high),
        };

    public Option<Vector4> Fill(double key) =>
        Stops.Map(ramp => ramp.Tabulate(Gamut.StandardRgb).Sample((key - double.CreateChecked(Soft.Low)) / double.CreateChecked(Soft.High - Soft.Low)));
}

public abstract class StateParameter<TRecord> {
    private StateParameter() { }

    public abstract TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor);

    public sealed class Bounded<TValue, TKey, TError>(Lens<TRecord, TValue> lens, Presentation<TValue, TKey> presentation) : StateParameter<TRecord>
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Bounded<TValue, TKey, TError>(lens, presentation);
    }

    public sealed class OptionalBounded<TValue, TKey, TError>(Lens<TRecord, Option<TValue>> lens, Presentation<TValue, TKey> presentation) : StateParameter<TRecord>
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalBounded<TValue, TKey, TError>(lens, presentation);
    }

    public sealed class Choice<TValue, TError>(Lens<TRecord, TValue> lens) : StateParameter<TRecord>
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Choice<TValue, TError>(lens);
    }

    public sealed class OptionalChoice<TValue, TError>(Lens<TRecord, Option<TValue>> lens) : StateParameter<TRecord>
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalChoice<TValue, TError>(lens);
    }

    public sealed class Variant<TValue, TCase, TError>(Lens<TRecord, TValue> lens) : StateParameter<TRecord>
        where TValue : class
        where TCase : class, IStateCase<TValue>, ISmartEnum<string, TCase, TError>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Variant<TValue, TCase, TError>(lens);
    }

    public sealed class Enumerated<TEnum>(Lens<TRecord, TEnum> lens) : StateParameter<TRecord> where TEnum : struct, Enum {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Enumerated(lens);
    }

    public sealed class Record<TNested>(Lens<TRecord, TNested> lens) : StateParameter<TRecord> where TNested : IStateRecord<TNested> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Record(lens);
    }

    public sealed class OptionalRecord<TNested>(Lens<TRecord, Option<TNested>> lens) : StateParameter<TRecord> where TNested : IStateRecord<TNested> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalRecord(lens);
    }

    public sealed class Toggle(Lens<TRecord, bool> lens) : StateParameter<TRecord> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Toggle(lens);
    }

    public sealed class Raw<TRaw>(Lens<TRecord, TRaw> lens) : StateParameter<TRecord> where TRaw : notnull, ISpanParsable<TRaw> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Raw(lens);
    }

    public sealed class OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) : StateParameter<TRecord> where TRaw : notnull, ISpanParsable<TRaw> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalRaw(lens);
    }

    public sealed class Color(Lens<TRecord, Swatch> lens) : StateParameter<TRecord> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Color(lens);
    }

    public sealed class OptionalColor(Lens<TRecord, Option<Swatch>> lens) : StateParameter<TRecord> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalColor(lens);
    }

    public sealed class Gradient(Lens<TRecord, Ramp> lens) : StateParameter<TRecord> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Gradient(lens);
    }

    public sealed class Swatches<TValue, TError>(Lens<TRecord, TValue> lens) : StateParameter<TRecord>
        where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Swatches<TValue, TError>(lens);
    }

    public sealed class Keyed<TValue, TRaw, TError>(Lens<TRecord, TValue> lens) : StateParameter<TRecord>
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Keyed<TValue, TRaw, TError>(lens);
    }

    public sealed class OptionalKeyed<TValue, TRaw, TError>(Lens<TRecord, Option<TValue>> lens) : StateParameter<TRecord>
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.OptionalKeyed<TValue, TRaw, TError>(lens);
    }

    public sealed class Loaded<TValue, TKey, TRaw, TError>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key) : StateParameter<TRecord>
        where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Loaded<TValue, TKey, TRaw, TError>(lens, load, key);
    }

    public sealed class Opaque<TValue>(Lens<TRecord, TValue> lens) : StateParameter<TRecord> where TValue : notnull {
        public override TResult Accept<TResult>(IStateParameterVisitor<TRecord, TResult> visitor) => visitor.Opaque(lens);
    }
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct ShortSideOffset : IMinMaxValue<ShortSideOffset> {
    public static ShortSideOffset MinValue { get; } = new(-8f);
    public static ShortSideOffset MaxValue { get; } = new(8f);
    public static Presentation<ShortSideOffset, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (-1f, 1f), Origin = (float)Neutral };

    public float Pixels(PixelExtent extent) => _value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct ShortSideLength : IMinMaxValue<ShortSideLength> {
    public static ShortSideLength MinValue => Neutral;
    public static ShortSideLength MaxValue { get; } = new(8f);
    public static Presentation<ShortSideLength, float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) };

    public float Pixels(PixelExtent extent) => _value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct ShortSideExtent : IMinMaxValue<ShortSideExtent> {
    public static ShortSideExtent MinValue { get; } = new(0.001f);
    public static ShortSideExtent MaxValue { get; } = new(8f);

    public float Pixels(PixelExtent extent) => _value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct FrameAxis : IMinMaxValue<FrameAxis> {
    public static FrameAxis MinValue { get; } = new(-0.5f);
    public static FrameAxis MaxValue { get; } = new(1.5f);
    public static FrameAxis Start { get; } = new(0f);
    public static FrameAxis Middle { get; } = new(0.5f);
    public static FrameAxis End { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

public sealed record FramePosition(FrameAxis X, FrameAxis Y) {
    public static FramePosition Center { get; } = new(FrameAxis.Middle, FrameAxis.Middle);
    public static FramePosition TopLeft { get; } = new(FrameAxis.Start, FrameAxis.Start);
    public static FramePosition TopRight { get; } = new(FrameAxis.End, FrameAxis.Start);
    public static FramePosition BottomRight { get; } = new(FrameAxis.End, FrameAxis.End);
    public static FramePosition BottomLeft { get; } = new(FrameAxis.Start, FrameAxis.End);

    private static readonly Lens<FramePosition, FrameAxis> XOf =
        Lens<FramePosition, FrameAxis>.New(static position => position.X, static x => position => position with { X = x });
    private static readonly Lens<FramePosition, FrameAxis> YOf =
        Lens<FramePosition, FrameAxis>.New(static position => position.Y, static y => position => position with { Y = y });

    public Vector2 Point(PixelExtent extent) => new(X * extent.Width, Y * extent.Height);

    public static (StateParameter<TRecord> X, StateParameter<TRecord> Y) Kinds<TRecord>(Lens<TRecord, FramePosition> position) =>
        (new StateParameter<TRecord>.Bounded<FrameAxis, float, InvalidPixelValue>(lens(position, XOf), Axis),
         new StateParameter<TRecord>.Bounded<FrameAxis, float, InvalidPixelValue>(lens(position, YOf), Axis));

    private static Presentation<FrameAxis, float> Axis { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) };
}

file readonly struct RowAction(PixelFrame frame, Memory2D<Vector4> rows, Action<Span<Vector4>, int, int> kernel, IProgress<int> progress, StrongBox<int> finished) : IAction {
    public static Unit For(PixelFrame frame, Action<Span<Vector4>, int, int> kernel, IProgress<int> progress) {
        ParallelHelper.For(0, frame.Size.Height, new RowAction(frame, frame.View, kernel, progress, new StrongBox<int>()));
        return unit;
    }

    public void Invoke(int i) {
        kernel(rows.Span.GetRowSpan(i), frame.Origin.X, frame.Line(i));
        progress.Report(Interlocked.Increment(ref finished.Value));
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Derivations {
    private readonly Atom<HashMap<Delegate, (object State, Lazy<object> Value)>> held = Atom(HashMap<Delegate, (object State, Lazy<object> Value)>());

    public TValue Derived<TState, TValue>(TState state, Func<TState, TValue> derive) where TState : notnull where TValue : notnull =>
        (TValue)held.Swap(map => map.Find(derive).Exists(kept => kept.State.Equals(state)) ? map : map.AddOrUpdate(derive, (state, new Lazy<object>(() => derive(state)))))
            .Find(derive).Map(static entry => entry.Value).ValueUnsafe()!.Value;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Valid {
    public static T Value<T, TError>(TError? error, T? item) where T : notnull where TError : Error =>
        error is null ? item! : error.Throw<T>();
}

internal static class Invariant {
    public static Option<T> Number<T>(string text) where T : INumber<T> =>
        T.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out T? number) ? Some(number) : None;
}
