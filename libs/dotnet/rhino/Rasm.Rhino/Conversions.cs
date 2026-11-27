using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.Rhino;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Conversions {

    // --- [SENTINELS]

    public static Guid Unset(Option<Guid> id) => id.IfNone(Guid.Empty);

    public static int Unset(Option<int> index) => index.IfNone(-1);

    public static uint Unset(Option<uint> serial) => serial.IfNone(0u);

    public static string Unset(Option<string> text) => text.IfNone("");

    public static double Unset(Option<double> scalar) => scalar.IfNone(RhinoMath.UnsetValue);

    public static Point3d Unset(Option<Point3d> point) => point.IfNone(Point3d.Unset);

    public static Vector3d Unset(Option<Vector3d> vector) => vector.IfNone(Vector3d.Unset);

    public static ComponentIndex Unset(Option<ComponentIndex> index) => index.IfNone(ComponentIndex.Unset);

    public static Color Unset(Option<Color> color) => color.IfNone(Color.Empty);

    public static Option<Guid> Present(Guid id) => Some(id).Filter(static value => value != Guid.Empty);

    public static Option<int> Present(int index) => Some(index).Filter(static value => value >= 0);

    public static Option<uint> Present(uint serial) => Some(serial).Filter(static value => value is not 0u);

    public static Option<string> Present(string? text) => Optional(text).Filter(static value => !string.IsNullOrEmpty(value));

    public static Option<double> Present(double scalar) => Some(scalar).Filter(RhinoMath.IsValidDouble);

    public static Option<Point3d> Present(Point3d point) => Some(point).Filter(static value => value.IsValid);

    public static Option<Vector3d> Present(Vector3d vector) => Some(vector).Filter(static value => value.IsValid);

    public static Option<Plane> Present(Plane plane) => Some(plane).Filter(static value => value.IsValid);

    public static Option<Line> Present(Line line) => Some(line).Filter(static value => value.IsValid);

    public static Option<BoundingBox> Present(BoundingBox box) => Some(box).Filter(static value => value.IsValid);

    public static Option<ComponentIndex> Present(ComponentIndex index) => Some(index).Filter(static value => !value.IsUnset());

    public static Option<Color> Present(Color color) => Some(color).Filter(static value => !value.IsEmpty);

    public static Option<uint> Serial(RhinoDoc? document) => Optional(document).Map(static value => value.RuntimeSerialNumber);

    // --- [SEQUENCES]

    public static Seq<T> Rows<T>(IEnumerable<T?>? rows) =>
        Optional(rows).ToSeq().Bind(static held => held.AsIterable().Choose(static row => Optional(row)).ToSeq()).Strict();

    public static Fin<Seq<TRow>> Unique<TRow, TKey>(Seq<TRow> rows, Func<TRow, TKey> key, IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull =>
        toSeq(rows.CountBy(key, comparer))
            .Filter(static counted => counted.Value > 1)
            .Traverse(static counted => Fin.Fail<Unit>(Fault.Of(RhinoReason.Repeated, counted.Key)))
            .As()
            .Map(_ => rows);

    // --- [TIME]

    public static Option<LocalDateTime> Present(DateTime value) => Some(value).Filter(static held => held != DateTime.MinValue).Map(LocalDateTime.FromDateTime);

    public static Option<LocalDateTime> Present(DateTime? value) => Optional(value).Bind(Present);

    public static Fin<int> Whole(Duration span, Duration step) =>
        step > Duration.Zero && Int128.DivRem(span.ToInt128Nanoseconds(), step.ToInt128Nanoseconds()) is var (count, remainder) && remainder == 0 && count >= 0 && count <= int.MaxValue
            ? (int)count
            : Fault.Of(RhinoReason.Outside, (span, step));

    // --- [BOUNDS]

    public static Fin<Unit> Within(int index, int itemCount) =>
        index >= 0 && index < itemCount ? unit : Fault.Of(RhinoReason.Outside, itemCount, index);

    public static Fin<double> Within(Interval domain, double parameter) =>
        domain.IncludesParameter(parameter) ? parameter : Fault.Of(RhinoReason.Outside, (domain, parameter));

    // --- [TYPES]

    public static Fin<T> Cast<T>(object? value, object? subject = null) where T : notnull =>
        value is T typed ? typed : Fault.Of(RhinoReason.Mismatched, subject is null ? (value?.GetType(), typeof(T)) : (subject, value?.GetType(), typeof(T)));

    // --- [VALUES]

    public static Option<T> Present<T>(bool present, T value) => present ? Some(value) : None;

    public static Fin<T> Required<T>(T? value, Option<int> index = default, object? subject = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : class =>
        value is { } held ? held : Fault.Of(RhinoReason.Missing, subject ?? (member, line), index);

    public static Fin<T> Required<T>(T? value, Option<int> index = default, object? subject = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : struct =>
        value is { } held ? held : Fault.Of(RhinoReason.Missing, subject ?? (member, line), index);

    public static Fin<T> Required<T>(Option<T> value, Option<int> index = default, object? subject = null, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) =>
        value.ToFin(Fault.Of(RhinoReason.Missing, subject ?? (member, line), index));

    public static Fin<Unit> Unreported(string? report, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) =>
        string.IsNullOrEmpty(report) ? unit : Fault.Of(RhinoReason.Declined, (member, line), inner: Error.New(report));

    public static bool Same(Color held, Color wanted) => held.ToArgb() == wanted.ToArgb();

    // --- [HOOKS]

    public static Fault<RhinoReason>? Rejection<T>(Fin<T> result) =>
        result.Match<Fault<RhinoReason>?>(Succ: static _ => null, Fail: static error =>
            error.Filter<Fault<RhinoReason>>().Head is Fault<RhinoReason> failure && error.Count is 1 ? failure : Fault.Of(RhinoReason.Invalid, typeof(T), inner: error));

    internal static Fault<RhinoReason>? Validate<T>(string? value, Func<string, Fin<T>> validated, out T? item) where T : class {
        Fin<Option<T>> parsed = Optional(value).Traverse(validated).As();
        item = parsed.ToOption().Bind(static held => held).ValueUnsafe();
        return Rejection(parsed);
    }
}
