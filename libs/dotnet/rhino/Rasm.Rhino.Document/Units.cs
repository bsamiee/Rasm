using System.Numerics;
using Rhino;
using Rhino.DocObjects;
using Rhino.Input;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
public enum DocumentSpace { Model = 0, Page = 1 }

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct UnitsAndTolerances(LengthUnit Unit, double Absolute, double Relative, double AngleRadians, int Precision);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnitsAndTolerancesChange {
    public sealed record Units(LengthUnit Unit, bool Scale) : UnitsAndTolerancesChange;

    public sealed record Tolerances(double Absolute, double Relative, double AngleRadians) : UnitsAndTolerancesChange;

    public sealed record Precision(int Digits) : UnitsAndTolerancesChange;
}

public readonly record struct ModelDistance(RhinoDoc Doc, double Value) : IFormattable {
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => Doc.FormatNumber(Value);
}

public sealed record Limits<T> where T : struct, INumber<T> {
    internal Limits(Option<(T Value, bool Exclusive)> lower, Option<(T Value, bool Exclusive)> upper) {
        Lower = lower;
        Upper = upper;
    }

    public Option<(T Value, bool Exclusive)> Lower { get; }

    public Option<(T Value, bool Exclusive)> Upper { get; }

    public TResult Fold<TResult>(
        Func<(T Value, bool Exclusive), (T Value, bool Exclusive), TResult> both,
        Func<(T Value, bool Exclusive), TResult> lower,
        Func<(T Value, bool Exclusive), TResult> upper,
        Func<TResult> none) =>
        Lower.Match(
            Some: low => Upper.Match(Some: high => both(low, high), None: () => lower(low)),
            None: () => Upper.Match(Some: upper, None: none));

    public Fin<Limits<T>> Inclusive(string member) =>
        Invalid.Unless(!Lower.Exists(static bound => bound.Exclusive) && !Upper.Exists(static bound => bound.Exclusive), this, member);

    public Fin<Limits<T>> AtMost(T upper, string member) =>
        Invalid.Unless(Lower.ForAll(bound => bound.Exclusive ? bound.Value < upper : bound.Value <= upper), new Limits<T>(Lower, Some((upper, false))), member);

    public T Clamp(T value, T inset) =>
        Upper.Fold(
            Lower.Fold(value, (low, bound) => T.Max(low, bound.Exclusive ? bound.Value + inset : bound.Value)),
            (high, bound) => T.Min(high, bound.Exclusive ? bound.Value - inset : bound.Value));

    public Fin<T> Check(T value, string subject) =>
        Check(value, subject, static bound => bound);

    public Fin<T> Check(T value, string subject, Func<T, IFormattable> display) =>
        from low in Lower.Match<Fin<Unit>>(
            Some: bound => bound.Exclusive
                ? (value > bound.Value ? unit : new NotGreaterThan(subject, display(bound.Value)))
                : (value >= bound.Value ? unit : new BelowLowerLimit(subject, display(bound.Value))),
            None: static () => unit)
        from high in Upper.Match<Fin<Unit>>(
            Some: bound => bound.Exclusive
                ? (value < bound.Value ? unit : new NotLessThan(subject, display(bound.Value)))
                : (value <= bound.Value ? unit : new AboveUpperLimit(subject, display(bound.Value))),
            None: static () => unit)
        select value;
}

public static class Limits {
    public static Limits<T> Unbounded<T>() where T : struct, INumber<T> => new(Option<(T, bool)>.None, Option<(T, bool)>.None);

    public static Limits<T> AtLeast<T>(T lower) where T : struct, INumber<T> => new(Some((lower, false)), Option<(T, bool)>.None);

    public static Limits<T> Above<T>(T lower) where T : struct, INumber<T> => new(Some((lower, true)), Option<(T, bool)>.None);

    public static Limits<T> AtMost<T>(T upper) where T : struct, INumber<T> => new(Option<(T, bool)>.None, Some((upper, false)));

    public static Limits<T> Below<T>(T upper) where T : struct, INumber<T> => new(Option<(T, bool)>.None, Some((upper, true)));
}

public sealed record EarthAnchorState(
    Option<(double EarthBasepointLatitude, double EarthBasepointLongitude, double EarthBasepointElevation, EarthCoordinateSystem EarthBasepointElevationCoordinateSystem)> Earth,
    Option<(Point3d ModelBasePoint, Vector3d ModelNorth, Vector3d ModelEast)> Model,
    Option<string> Name,
    Option<string> Description);

public readonly record struct EarthAnchorFrame(global::Rhino.Geometry.Plane EarthAnchorPlane, Vector3d AnchorNorth, global::Rhino.Geometry.Plane ModelCompass, Transform ModelToEarthTransform);

public readonly record struct LengthParse(double Value, LengthUnit Unit, string Exact);

public readonly record struct ScaleParse(double Left, double Right, LengthUnit LeftUnit, LengthUnit RightUnit, double LeftToRight, double RightToLeft);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DocumentUnits {
    // --- [DOCUMENT]
    public static IO<UnitsAndTolerances> Read(RhinoDoc doc, DocumentSpace space) =>
        IO.lift(() => space switch {
            DocumentSpace.Model => new UnitsAndTolerances(doc.ModelUnits, doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians, doc.ModelDistanceDisplayPrecision),
            DocumentSpace.Page => new UnitsAndTolerances(doc.PageUnits, doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians, doc.PageDistanceDisplayPrecision),
        });

    public static IO<UnitsAndTolerances> Adjust(RhinoDoc doc, DocumentSpace space, UnitsAndTolerancesChange change) =>
        from written in change.Switch(
            (Doc: doc, Space: space),
            units: static (state, units) =>
                IO.lift(() => Refused.Unless(state.Doc.AdjustLengthUnits(state.Space == DocumentSpace.Model, units.Unit, units.Scale), nameof(RhinoDoc.AdjustLengthUnits))),
            tolerances: static (state, tolerances) =>
                from valid in IO.lift(() => (
                    PositiveFinite(tolerances.Absolute, Member(state.Space, nameof(RhinoDoc.ModelAbsoluteTolerance), nameof(RhinoDoc.PageAbsoluteTolerance))),
                    PositiveFinite(tolerances.Relative, Member(state.Space, nameof(RhinoDoc.ModelRelativeTolerance), nameof(RhinoDoc.PageRelativeTolerance))),
                    PositiveFinite(tolerances.AngleRadians, Member(state.Space, nameof(RhinoDoc.ModelAngleToleranceRadians), nameof(RhinoDoc.PageAngleToleranceRadians))))
                    .Apply(static (absolute, relative, angle) => (Absolute: absolute, Relative: relative, AngleRadians: angle)).As().ToFin())
                from set in IO.lift(() => state.Space switch {
                    DocumentSpace.Model => (state.Doc.ModelAbsoluteTolerance, state.Doc.ModelRelativeTolerance, state.Doc.ModelAngleToleranceRadians) =
                        (valid.Absolute, valid.Relative, valid.AngleRadians),
                    DocumentSpace.Page => (state.Doc.PageAbsoluteTolerance, state.Doc.PageRelativeTolerance, state.Doc.PageAngleToleranceRadians) =
                        (valid.Absolute, valid.Relative, valid.AngleRadians),
                })
                select unit,
            precision: static (state, precision) =>
                from digits in IO.lift(() => Limits.AtLeast(0).Check(precision.Digits, Member(state.Space, nameof(RhinoDoc.ModelDistanceDisplayPrecision), nameof(RhinoDoc.PageDistanceDisplayPrecision))))
                from set in IO.lift(() => state.Space switch {
                    DocumentSpace.Model => state.Doc.ModelDistanceDisplayPrecision = digits,
                    DocumentSpace.Page => state.Doc.PageDistanceDisplayPrecision = digits,
                })
                select unit)
        from read in Read(doc, space)
        from verified in IO.lift(() => change.Switch<(UnitsAndTolerances Read, DocumentSpace Space), Fin<UnitsAndTolerances>>(
            (read, space),
            units: static (state, units) =>
                Mismatch.Unless(state.Read.Unit == units.Unit, Member(state.Space, nameof(RhinoDoc.ModelUnits), nameof(RhinoDoc.PageUnits)))
                    .Map(_ => state.Read),
            tolerances: static (state, tolerances) =>
                Mismatch.Unless(
                    (state.Read.Absolute == tolerances.Absolute) && (state.Read.Relative == tolerances.Relative) && (state.Read.AngleRadians == tolerances.AngleRadians),
                    Member(state.Space, nameof(RhinoDoc.ModelAbsoluteTolerance), nameof(RhinoDoc.PageAbsoluteTolerance)))
                    .Map(_ => state.Read),
            precision: static (state, precision) =>
                Mismatch.Unless(state.Read.Precision == precision.Digits, Member(state.Space, nameof(RhinoDoc.ModelDistanceDisplayPrecision), nameof(RhinoDoc.PageDistanceDisplayPrecision)))
                    .Map(_ => state.Read)))
        select verified;

    public static Validation<Error, double> PositiveFinite(double value, string member) =>
        double.IsFinite(value) && (value > 0.0) ? value : new Invalid(member);

    private static string Member(DocumentSpace space, string model, string page) =>
        space switch {
            DocumentSpace.Model => model,
            DocumentSpace.Page => page,
        };

    // --- [EARTH_ANCHOR]
    public static IO<EarthAnchorState> ReadEarthAnchor(RhinoDoc doc) =>
        Disposal.Using(() => doc.EarthAnchorPoint, ReadEarthAnchor);

    public static IO<EarthAnchorState> ReadEarthAnchor(EarthAnchorPoint anchor) =>
        IO.lift(() => new EarthAnchorState(
            anchor.EarthLocationIsSet()
                ? Some((anchor.EarthBasepointLatitude, anchor.EarthBasepointLongitude, anchor.EarthBasepointElevation, anchor.EarthBasepointElevationCoordinateSystem))
                : Option<(double, double, double, EarthCoordinateSystem)>.None,
            anchor.ModelLocationIsSet() ? Some((anchor.ModelBasePoint, anchor.ModelNorth, anchor.ModelEast)) : Option<(Point3d, Vector3d, Vector3d)>.None,
            Answers.Present(anchor.Name),
            Answers.Present(anchor.Description)));

    public static IO<Unit> ModifyEarthAnchor(Func<EarthAnchorPoint> read, Action<EarthAnchorPoint> write, Func<EarthAnchorPoint, IO<Unit>> edit) =>
        Disposal.Using(read, anchor =>
            from edited in edit(anchor)
            from written in IO.lift(() => write(anchor))
            select written);

    public static IO<EarthAnchorFrame> ReadEarthAnchorFrame(RhinoDoc doc) =>
        Disposal.Using(() => doc.EarthAnchorPoint, anchor =>
            from located in IO.lift(() => (
                    Missing.Unless(anchor.ModelLocationIsSet(), nameof(EarthAnchorPoint.ModelLocationIsSet)).ToValidation()
                    & Missing.Unless(anchor.EarthLocationIsSet(), nameof(EarthAnchorPoint.EarthLocationIsSet)).ToValidation())
                .ToFin())
            from frame in IO.lift(() => new EarthAnchorFrame(anchor.GetEarthAnchorPlane(out Vector3d north), north, anchor.GetModelCompass(), anchor.GetModelToEarthTransform(doc.ModelUnits)))
            from valid in IO.lift(() => (
                    Invalid.Unless(frame.ModelCompass.IsValid, nameof(EarthAnchorPoint.GetModelCompass)).ToValidation()
                    & Invalid.Unless(frame.ModelToEarthTransform.IsValid, nameof(EarthAnchorPoint.GetModelToEarthTransform)).ToValidation())
                .ToFin())
            select frame);

    // --- [UNITS]
    public static Fin<LengthUnit> CustomUnit(string name, double metersPerUnit) =>
        from named in Invalid.Unless(name.Length > 0, nameof(LengthUnit.FromCustomUnitSystem))
        from sized in PositiveFinite(metersPerUnit, nameof(LengthUnit.FromCustomUnitSystem)).ToFin()
        select LengthUnit.FromCustomUnitSystem(name, sized, UnitSystem.Meters);

    public static Fin<double> UnitScale(LengthUnit from, LengthUnit to) =>
        Invalid.Unless(!LengthUnit.IsUnset(from) && !LengthUnit.IsNone(from) && !LengthUnit.IsUnset(to) && !LengthUnit.IsNone(to), nameof(LengthUnit.Scale))
            .Map(_ => LengthUnit.Scale(from, to));

    // --- [TEXT]
    private const uint DefaultLocaleId = 0u;

    public static IO<LengthParse> ParseLength(string text, StringParserSettings settings, LengthUnit target) =>
        from targeted in IO.lift(() => Invalid.Unless(!LengthUnit.IsUnset(target), nameof(LengthUnit.IsUnset)))
        from length in Disposal.Bracketed(
            IO.lift(() => Missing.Unless(LengthValue.Create(text, settings, out bool all), nameof(LengthValue.Create)).Map(value => (Value: value, ParsedAll: all))),
            static parsed => IO.lift(parsed.Value.Dispose),
            parsed => IO.lift(() =>
                from complete in Invalid.Unless(parsed.ParsedAll, nameof(LengthValue.Create))
                from set in Invalid.Unless(!parsed.Value.IsUnset(), nameof(LengthValue.IsUnset))
                select new LengthParse(parsed.Value.Length(target), parsed.Value.Units, parsed.Value.LengthString)))
        select length;

    public static IO<string> FormatLength(double value, LengthUnit lengthUnit, LengthValue.StringFormat format, Option<uint> localeId) =>
        Disposal.Using(IO.lift(() => Missing.Unless(LengthValue.Create(value, lengthUnit, format, localeId.IfNone(DefaultLocaleId)), nameof(LengthValue.Create))), static length => IO.lift(() => Invalid.Unless(!length.IsUnset(), nameof(LengthValue.IsUnset)).Map(_ => length.LengthString)));

    public static IO<ScaleParse> ParseScale(string text, StringParserSettings settings) =>
        Disposal.Using(IO.lift(() => Missing.Unless(ScaleValue.Create(text, settings), nameof(ScaleValue.Create))), static scale =>
            from set in IO.lift(() => Invalid.Unless(!scale.IsUnset(), nameof(ScaleValue.IsUnset)))
            from parsed in Disposal.Using(IO.lift(() => Missing.Unless(scale.LeftLengthValue(), nameof(ScaleValue.LeftLengthValue))), left =>
                Disposal.Using(IO.lift(() => Missing.Unless(scale.RightLengthValue(), nameof(ScaleValue.RightLengthValue))), right =>
                    from sides in IO.lift(Invalid.Unless(!left.IsUnset() && !right.IsUnset(), nameof(LengthValue.IsUnset)))
                    from ratios in IO.lift(Invalid.Unless((scale.LeftToRightScale > 0.0) && (scale.RightToLeftScale > 0.0), nameof(ScaleValue.LeftToRightScale)))
                    select new ScaleParse(left.Length(), right.Length(), left.Units, right.Units, scale.LeftToRightScale, scale.RightToLeftScale)))
            select parsed);

    public static Fin<double> ParseAngleRadians(string text, bool degrees) =>
        degrees
            ? Invalid.Unless(StringParser.ParseAngleExpressionDegrees(text, out double angleDegrees), RhinoMath.ToRadians(angleDegrees), nameof(StringParser.ParseAngleExpressionDegrees))
            : Invalid.Unless(StringParser.ParseAngleExpressionRadians(text, out double angleRadians), angleRadians, nameof(StringParser.ParseAngleExpressionRadians));

    public static Fin<double> ParseNumber(string text, StringParserSettings settings) {
        StringParserSettings settingsOut = settings;
        return Invalid.Unless(StringParser.ParseNumber(text, text.Length, settings, ref settingsOut, out double answer) >= text.Length, answer, nameof(StringParser.ParseNumber));
    }
}
