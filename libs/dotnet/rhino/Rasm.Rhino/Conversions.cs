using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Rhino;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Rasm.RhinoCommon.Conversions))]

namespace Rasm.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conversions {
    // --- [SENTINELS]
    [UserMapping]
    public static Guid Unset(Option<Guid> id) => id.IfNone(Guid.Empty);

    [UserMapping]
    public static int Unset(Option<int> index) => index.IfNone(-1);

    [UserMapping]
    public static uint Unset(Option<uint> serial) => serial.IfNone(0u);

    [UserMapping]
    public static string Unset(Option<string> text) => text.IfNone("");

    [UserMapping]
    public static double Unset(Option<double> scalar) => scalar.IfNone(RhinoMath.UnsetValue);

    [UserMapping]
    public static Point3d Unset(Option<Point3d> point) => point.IfNone(Point3d.Unset);

    [UserMapping]
    public static Vector3d Unset(Option<Vector3d> vector) => vector.IfNone(Vector3d.Unset);

    [UserMapping]
    public static float Unset(Option<float> scalar) => scalar.IfNone(RhinoMath.UnsetSingle);

    [UserMapping]
    public static Plane Unset(Option<Plane> plane) => plane.IfNone(Plane.Unset);

    [UserMapping]
    public static Line Unset(Option<Line> line) => line.IfNone(Line.Unset);

    [UserMapping]
    public static Arc Unset(Option<Arc> arc) => arc.IfNone(Arc.Unset);

    [UserMapping]
    public static BoundingBox Unset(Option<BoundingBox> box) => box.IfNone(BoundingBox.Empty);

    [UserMapping]
    public static Transform Unset(Option<Transform> xform) => xform.IfNone(Transform.Unset);

    [UserMapping]
    public static ComponentIndex Unset(Option<ComponentIndex> index) => index.IfNone(ComponentIndex.Unset);

    [UserMapping]
    public static CurveOrientation Unset(Option<CurveOrientation> orientation) => orientation.IfNone(CurveOrientation.Undefined);

    [UserMapping]
    public static PointContainment Unset(Option<PointContainment> containment) => containment.IfNone(PointContainment.Unset);

    [UserMapping]
    public static Color Unset(Option<Color> color) => color.IfNone(Color.Empty);

    [UserMapping]
    public static Rectangle Unset(Option<Rectangle> rectangle) => rectangle.IfNone(Rectangle.Empty);

    [UserMapping]
    public static Option<Guid> Present(Guid id) => Some(id).Filter(static value => value != Guid.Empty);

    [UserMapping]
    public static Option<int> Present(int index) => Some(index).Filter(static value => value >= 0);

    [UserMapping]
    public static Option<uint> Present(uint serial) => Some(serial).Filter(static value => value != 0u);

    [UserMapping]
    public static Option<string> Present(string? text) => Optional(text).Filter(static value => !string.IsNullOrEmpty(value));

    [UserMapping]
    public static Option<double> Present(double scalar) => Some(scalar).Filter(RhinoMath.IsValidDouble);

    [UserMapping]
    public static Option<Point3d> Present(Point3d point) => Some(point).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<Vector3d> Present(Vector3d vector) => Some(vector).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<float> Present(float scalar) => Some(scalar).Filter(RhinoMath.IsValidSingle);

    [UserMapping]
    public static Option<Plane> Present(Plane plane) => Some(plane).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<Line> Present(Line line) => Some(line).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<Arc> Present(Arc arc) => Some(arc).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<BoundingBox> Present(BoundingBox box) => Some(box).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<Transform> Present(Transform xform) => Some(xform).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<ComponentIndex> Present(ComponentIndex index) => Some(index).Filter(static value => !value.IsUnset());

    [UserMapping]
    public static Option<CurveOrientation> Present(CurveOrientation orientation) => Some(orientation).Filter(static value => value is not CurveOrientation.Undefined);

    [UserMapping]
    public static Option<PointContainment> Present(PointContainment containment) => Some(containment).Filter(static value => value is not PointContainment.Unset);

    [UserMapping]
    public static Option<Color> Present(Color color) => Some(color).Filter(static value => !Same(value, Color.Empty));

    [UserMapping]
    public static Option<Rectangle> Present(Rectangle rectangle) => Some(rectangle).Filter(static value => !value.IsEmpty);

    [UserMapping]
    public static Option<uint> Serial(RhinoDoc? document) => Optional(document).Map(static value => value.RuntimeSerialNumber);

    // --- [SEQUENCES]
    [UserMapping]
    public static Seq<T> Rows<T>(IEnumerable<T?>? rows) =>
        Optional(rows).ToSeq().Bind(static held => held.AsIterable().Choose(static row => Optional(row)).ToSeq()).Strict();

    // --- [TIME]
    [UserMapping]
    public static Option<LocalDateTime> Present(DateTime value) => Some(value).Filter(static held => held != DateTime.MinValue).Map(LocalDateTime.FromDateTime);

    [UserMapping]
    public static Option<LocalDateTime> Present(DateTime? value) => Optional(value).Bind(Present);

    public static Fin<int> Whole(Duration span, Duration step, [CallerArgumentExpression(nameof(span))] string call = "") =>
        Int128.DivRem(span.ToInt128Nanoseconds(), step.ToInt128Nanoseconds()) is var (count, remainder) && remainder == 0 && count >= 0 && count <= int.MaxValue
            ? (int)count
            : new RhinoFault(call, RhinoReason.Outside);

    // --- [BOUNDS]
    public static Fin<Unit> Within(int index, int itemCount, [CallerArgumentExpression(nameof(index))] string call = "") =>
        index >= 0 && index < itemCount ? unit : new RhinoFault(call, RhinoReason.Outside, index);

    public static Fin<double> Within(Interval domain, double parameter, [CallerArgumentExpression(nameof(parameter))] string call = "") =>
        domain.IncludesParameter(parameter) ? parameter : new RhinoFault(call, RhinoReason.Outside);

    // --- [TYPES]
    public static Fin<T> Cast<T>(object? value, [CallerArgumentExpression(nameof(value))] string call = "") where T : notnull =>
        value is T typed ? typed : new RhinoFault(call, RhinoReason.Mismatched);

    public static Fin<T> Typed<T>(object value, string key) where T : notnull =>
        value is T typed ? typed : new RhinoFault(key, RhinoReason.Mismatched);

    // --- [VALUES]
    public static Option<T> Found<T>(bool found, T value) => found ? Some(value) : None;

    public static bool Same(Color held, Color wanted) => held.ToArgb() == wanted.ToArgb();
}
