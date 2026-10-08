using System.Diagnostics;
using System.Drawing;
using Rasm.Imaging.Pixels;
using Rhino;
using Rhino.Commands;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Rasm.Rhino.Conversions))]

namespace Rasm.Rhino;

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

    private static Option<T> Present<T>(T? value, Func<Option<T>, T> unset) =>
        Optional(value).Filter(held => !EqualityComparer<T>.Default.Equals(held, unset(None)));

    [UserMapping]
    public static Option<Guid> Present(Guid id) => Present(id, Unset);

    [UserMapping]
    public static Option<int> Present(int index) => Some(index).Filter(static value => value >= 0);

    [UserMapping]
    public static Option<uint> Present(uint serial) => Present(serial, Unset);

    [UserMapping]
    public static Option<string> Present(string? text) => Present(text, Unset);

    [UserMapping]
    public static Option<double> Present(double scalar) => Present(scalar, Unset);

    [UserMapping]
    public static Option<Point3d> Present(Point3d point) => Present(point, Unset);

    [UserMapping]
    public static Option<Vector3d> Present(Vector3d vector) => Present(vector, Unset);

    [UserMapping]
    public static Option<float> Present(float scalar) => Present(scalar, Unset);

    [UserMapping]
    public static Option<Plane> Present(Plane plane) => Present(plane, Unset);

    [UserMapping]
    public static Option<Line> Present(Line line) => Present(line, Unset);

    [UserMapping]
    public static Option<Arc> Present(Arc arc) => Present(arc, Unset);

    [UserMapping]
    public static Option<BoundingBox> Present(BoundingBox box) => Some(box).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<Transform> Present(Transform xform) => Present(xform, Unset);

    [UserMapping]
    public static Option<ComponentIndex> Present(ComponentIndex index) => Present(index, Unset);

    [UserMapping]
    public static Option<CurveOrientation> Present(CurveOrientation orientation) => Present(orientation, Unset);

    [UserMapping]
    public static Option<PointContainment> Present(PointContainment containment) => Present(containment, Unset);

    [UserMapping]
    public static Option<Color> Present(Color color) => Some(color).Filter(static value => !Same(value, Color.Empty));

    [UserMapping]
    public static Option<Rectangle> Present(Rectangle rectangle) => Present(rectangle, Unset);

    [UserMapping]
    public static Option<uint> Serial(RhinoDoc? document) => Optional(document).Map(static value => value.RuntimeSerialNumber);

    // --- [SEQUENCES]
    [UserMapping]
    public static Seq<T> Rows<T>(IEnumerable<T?>? rows) =>
        Optional(rows).ToSeq().Bind(static held => held.AsIterable().Choose(static row => Optional(row)).ToSeq()).Strict();

    // --- [TIME]
    [UserMapping]
    public static Option<LocalDateTime> Present(DateTime value) => Present(value, static _ => DateTime.MinValue).Map(LocalDateTime.FromDateTime);

    [UserMapping]
    public static Option<LocalDateTime> Present(DateTime? value) => Optional(value).Bind(Present);

    public static Fin<int> Whole(Duration span, Duration step) =>
        Int128.DivRem(span.ToInt128Nanoseconds(), step.ToInt128Nanoseconds()) is var (count, remainder) && remainder == 0 && count >= 0 && count <= int.MaxValue
            ? (int)count
            : new UnrepresentableSpan(span, step);

    // --- [ANSWERS]
    public static Fin<Guid> Required(Guid id, string member) => Present(id).ToFin(new Refused(member));

    public static Fin<int> Required(int index, string member) => Present(index).ToFin(new Refused(member));

    public static Fin<uint> Required(uint serial, string member) => Present(serial).ToFin(new Refused(member));

    public static Fin<Seq<TRow>> NonEmpty<TRow>(Seq<TRow> rows, string member) => rows.IsEmpty ? new Missing(member) : rows;

    public static Fin<Unit> FromResult(Result result, string member) =>
        result switch {
            Result.Success => unit,
            Result.Cancel or Result.CancelModelessDialog => Errors.Cancelled,
            Result.Nothing or Result.ExitRhino or Result.UnknownCommand => new Ended(member, result),
            Result.Failure => new Refused(member),
        };

    public static Fin<Result> ToResult(IO<Unit> effect) =>
        Try.lift(effect.Run).Run().Map(static _ => Result.Success).BindFail(static error =>
            Seq(Result.ExitRhino, Result.Cancel, Result.Nothing, Result.UnknownCommand).Find(error.FoldM(static cause =>
                cause is Ended ended ? Seq(ended.Result) : cause.Is(Errors.Cancelled) ? Seq(Result.Cancel) : Seq<Result>()).Contains).ToFin(error));

    public static TResult ByKind<TResult>(RenderContent content, Func<RenderMaterial, TResult> material, Func<RenderEnvironment, TResult> environment, Func<RenderTexture, TResult> texture) =>
        content switch {
            RenderMaterial found => material(found),
            RenderEnvironment found => environment(found),
            RenderTexture found => texture(found),
            _ => throw new UnreachableException(),
        };

    [UserMapping(Default = false)]
    public static RenderContentKind KindOf(RenderContent content) =>
        ByKind(content, static _ => RenderContentKind.Material, static _ => RenderContentKind.Environment, static _ => RenderContentKind.Texture);

    // --- [VALUES]
    public static Fin<T> Validated<T, TRaw, TError>(TRaw raw)
        where T : IObjectFactory<T, TRaw, TError>
        where TRaw : notnull
        where TError : Error, IValidationError<TError> =>
        T.Validate(raw, provider: null, out T? item) is { } error ? error : item!;

    public static bool Same(Color held, Color wanted) => held.ToArgb() == wanted.ToArgb();

    public static Swatch Rgb(Color color) => (Swatch)Color.FromArgb(byte.MinValue, color).ToArgb();

    [UserMapping]
    public static Color Opaque(Swatch swatch) => Color.FromArgb(byte.MaxValue, Color.FromArgb(swatch));
}
