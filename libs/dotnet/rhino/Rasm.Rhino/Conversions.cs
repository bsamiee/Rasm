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
    public static Option<Guid> Present(Guid id) =>
        Some(id).Filter(static value => value != Guid.Empty);

    [UserMapping]
    public static Option<int> Present(int index) =>
        Some(index).Filter(static value => value >= 0);

    [UserMapping]
    public static Option<uint> Present(uint serial) =>
        Some(serial).Filter(static value => value != 0u);

    [UserMapping]
    public static Option<string> Present(string? text) =>
        Optional(text).Filter(static value => value.Length > 0);

    [UserMapping]
    public static Option<double> Present(double scalar) =>
        Some(scalar).Filter(static value => value != RhinoMath.UnsetValue);

    [UserMapping]
    public static Option<Point3d> Present(Point3d point) =>
        Some(point).Filter(static value => value != Point3d.Unset);

    [UserMapping]
    public static Option<Vector3d> Present(Vector3d vector) =>
        Some(vector).Filter(static value => value != Vector3d.Unset);

    [UserMapping]
    public static Option<float> Present(float scalar) =>
        Some(scalar).Filter(static value => value != RhinoMath.UnsetSingle);

    [UserMapping]
    public static Option<Plane> Present(Plane plane) =>
        Some(plane).Filter(static value => value != Plane.Unset);

    [UserMapping]
    public static Option<Line> Present(Line line) =>
        Some(line).Filter(static value => value != Line.Unset);

    [UserMapping]
    public static Option<Arc> Present(Arc arc) =>
        Some(arc).Filter(static value => value != Arc.Unset);

    [UserMapping]
    public static Option<BoundingBox> Present(BoundingBox box) =>
        Some(box).Filter(static value => value.IsValid);

    [UserMapping]
    public static Option<Transform> Present(Transform xform) =>
        Some(xform).Filter(static value => value != Transform.Unset);

    [UserMapping]
    public static Option<ComponentIndex> Present(ComponentIndex index) =>
        Some(index).Filter(static value => value != ComponentIndex.Unset);

    [UserMapping]
    public static Option<CurveOrientation> Present(CurveOrientation orientation) =>
        Some(orientation).Filter(static value => value != CurveOrientation.Undefined);

    [UserMapping]
    public static Option<PointContainment> Present(PointContainment containment) =>
        Some(containment).Filter(static value => value != PointContainment.Unset);

    [UserMapping]
    public static Option<Color> Present(Color color) =>
        Some(color).Filter(static value => !Same(value, Color.Empty));

    [UserMapping]
    public static Option<Rectangle> Present(Rectangle rectangle) =>
        Some(rectangle).Filter(static value => value != Rectangle.Empty);

    [UserMapping]
    public static Option<uint> Serial(RhinoDoc? document) =>
        Optional(document).Map(static value => value.RuntimeSerialNumber);

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

    // --- [SEQUENCES]
    [UserMapping(Default = false)]
    public static Seq<T> Rows<T>(IEnumerable<T?>? rows) where T : class =>
        toSeq(rows).Choose(static row => Optional(row)).Strict();

    [UserMapping(Default = false)]
    public static Seq<T> Snapshot<T>(IEnumerable<T>? rows) where T : struct =>
        toSeq(rows).Strict();

    // --- [TIME]
    [UserMapping]
    public static Option<LocalDateTime> Present(DateTime value) =>
        Some(value).Filter(static stamp => stamp != DateTime.MinValue).Map(LocalDateTime.FromDateTime);

    [UserMapping]
    public static Option<LocalDateTime> Present(DateTime? value) =>
        Optional(value).Map(LocalDateTime.FromDateTime);

    public static Fin<int> Whole(Duration span, Duration step) =>
        Int128.DivRem(span.ToInt128Nanoseconds(), step.ToInt128Nanoseconds()) is var (count, remainder)
        && remainder == Int128.Zero && count >= Int128.Zero && count <= int.MaxValue
            ? (int)count
            : new UnrepresentableSpan(span, step);

    // --- [ANSWERS]
    public static Fin<Guid> Required(Guid id, string member) => Present(id).ToFin(new Refused(member));

    public static Fin<int> Required(int index, string member) => Present(index).ToFin(new Refused(member));

    public static Fin<uint> Required(uint serial, string member) => Present(serial).ToFin(new Refused(member));

    public static Fin<Seq<TRow>> NonEmpty<TRow>(Seq<TRow> rows, string member) =>
        rows.IsEmpty ? new Missing(member) : rows;

    public static Fin<Unit> FromResult(Result result, string member) =>
        result switch {
            Result.Success => unit,
            Result.Cancel or Result.CancelModelessDialog => Errors.Cancelled,
            Result.Nothing => new NothingEntered(),
            Result.ExitRhino => new ExitRequested(),
            Result.UnknownCommand => new UnknownCommand(member),
            Result.Failure => new Refused(member),
        };

    public static Fin<Result> ToResult(IO<Unit> effect, EnvIO? env = null) =>
        Try.lift(() => env is null ? effect.Run() : effect.Run(env)).Run().Map(static _ => Result.Success).BindFail(static error =>
            error.IsType<ExitRequested>() ? Result.ExitRhino
            : error.Is(Errors.Cancelled) ? Result.Cancel
            : error.IsType<NothingEntered>() ? Result.Nothing
            : error.IsType<UnknownCommand>() ? Result.UnknownCommand
            : Fin.Fail<Result>(error));

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
