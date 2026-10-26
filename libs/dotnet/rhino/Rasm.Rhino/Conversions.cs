using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using Rasm.Rhino.Document;
using Rasm.UI;
using Rasm.UI.Styling;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using SkiaSharp;
using Wacton.Unicolour;

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
            : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{span} is no whole count of {step}"));

    // --- [BOUNDS]
    public static Fin<Unit> Within(int index, int itemCount, string member) =>
        index >= 0 && index < itemCount ? unit : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{member} index {index} is outside {itemCount} items"));

    public static Fin<double> Within(Interval domain, double parameter, string member) =>
        domain.IncludesParameter(parameter) ? parameter : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{member} parameter {parameter} is outside {domain}"));

    // --- [TYPES]
    public static Fin<T> Cast<T>(object value) where T : notnull =>
        value is T typed ? typed : Error.New($"expected {typeof(T).Name}, got {value.GetType().Name}");

    public static Fin<T> Typed<T>(object value, string key) where T : notnull =>
        value is T typed ? typed : Errors.ParseError($"key {key} holds {value.GetType().Name}, expected {typeof(T).Name}");

    // --- [ANSWERS]
    public static Fin<Guid> Required(Guid id, string member) => Present(id).ToFin(new Refused(member, Refused.Declined));

    public static Fin<int> Required(int index, string member) => Present(index).ToFin(new Refused(member, Refused.Declined));

    public static Fin<uint> Required(uint serial, string member) => Present(serial).ToFin(new Refused(member, Refused.Declined));

    public static Fin<Seq<TRow>> NonEmpty<TRow>(Seq<TRow> rows, string member) => rows.IsEmpty ? new Refused(member, Refused.Absent) : rows;

    public static Fin<Unit> FromResult(Result result, string member) =>
        result switch {
            Result.Success => unit,
            Result.Cancel or Result.CancelModelessDialog => Errors.Cancelled,
            Result.Nothing or Result.ExitRhino or Result.UnknownCommand => new Ended(member, result),
            Result.Failure => new Refused(member, Refused.Declined),
        };

    public static Fin<Result> ToResult(IO<Unit> effect) => ToResult(Callbacks.Settled(effect, CancellationToken.None));

    public static Fin<Result> ToResult(Fin<Unit> settled) =>
        settled.Map(static _ => Result.Success).BindFail(static error =>
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
    public static bool Same(Color held, Color wanted) => held.ToArgb() == wanted.ToArgb();

    [UserMapping]
    public static double Degrees(NorthAngle north) => north;

    // --- [COLORS]
    public static Lens<Color, SKColorF> Rgb { get; } = Lens<Color, SKColorF>.New(ToSKColorF, static color => _ => ToColor(color));

    public static SKColorF ToSKColorF(Color color) =>
        new(color.R / (float)byte.MaxValue, color.G / (float)byte.MaxValue, color.B / (float)byte.MaxValue, color.A / (float)byte.MaxValue);

    public static SKColorF ToSKColorF(Color4f color) => new(color.R, color.G, color.B, color.A);

    public static SKColorF ToSKColorF(Eto.Drawing.Color color) => new(color.R, color.G, color.B, color.A);

    public static Color ToColor(SKColorF color) =>
        new Unicolour(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha).MapToRgbGamut() switch {
            var mapped => mapped.Rgb.Byte255 switch {
                var rgb => Color.FromArgb(mapped.Alpha.A255, rgb.R, rgb.G, rgb.B),
            },
        };

    public static Color4f ToColor4f(SKColorF color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    public static (Color4f Light, double Luminance) Peaked(double red, double green, double blue) =>
        Peaked(new Unicolour(Configuration.Default, ColourSpace.RgbLinear, red, green, blue));

    public static (Color4f Light, double Luminance) Blackbody(double kelvins) =>
        Peaked(new Unicolour(Configuration.Default, kelvins, Locus.Blackbody));

    private static (Color4f Light, double Luminance) Peaked(Unicolour light) =>
        Scaled(Scaled(light).MapToRgbGamut()) switch {
            var peaked => (new Color4f((float)peaked.RgbLinear.R, (float)peaked.RgbLinear.G, (float)peaked.RgbLinear.B, 1f), peaked.Xyz.Y),
        };

    private static Unicolour Scaled(Unicolour light) =>
        light.RgbLinear switch {
            var rgb => double.Max(double.Abs(rgb.R), double.Max(double.Abs(rgb.G), double.Abs(rgb.B))) switch {
                var peak => new Unicolour(light.Configuration, ColourSpace.RgbLinear, rgb.R / peak, rgb.G / peak, rgb.B / peak),
            },
        };

    // --- [IMAGES]
    public static IO<SKImage> ToSKImage(Bitmap bitmap) =>
        DisposalOps.Within(
            () => bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb),
            data => IO.lift(() => Refused.UnlessFound(
                SKImage.FromPixelCopy(new SKImageInfo(data.Width, data.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb()), data.Scan0, data.Stride),
                nameof(SKImage.FromPixelCopy))),
            data => IO.lift(() => bitmap.UnlockBits(data)));

    public static Fin<ColorRamp> ToColorRamp(ColorGradient gradient) =>
        toSeq(gradient.GetColorStops())
            .Traverse(static stop => Fraction.Validated((float)stop.Position).Map(at => new Stop<SKColorF>(ToSKColorF(stop.Color), at)))
            .As()
            .Bind(static stops => stops.IsEmpty
                ? new Refused(nameof(ColorGradient.GetColorStops), Refused.Absent)
                : Fin.Succ<ColorRamp>(new ColorRamp.Smooth(stops) { Space = CssSpace.Srgb }));

    public static ColorGradient ToColorGradient(ColorGradient held, ColorRamp ramp) {
        ColorGradient written = held.Duplicate();
        written.SetColorStops(ramp.Material.Stops.Map(static stop => stop.Color switch {
            Ink.Fixed color => new ColorStop(ToColor(color.Color), (float)stop.At),
            _ => throw new UnreachableException(),
        }));
        return written;
    }
}

internal static class Patches {
    [UserMapping(Default = true)]
    public static int Patched(Option<int> value, [MappingTargetOriginalValue] int original) => value.IfNone(original);

    [UserMapping(Default = true)]
    public static double Patched(Option<double> value, [MappingTargetOriginalValue] double original) => value.IfNone(original);

    [UserMapping(Default = true)]
    public static bool Patched(Option<bool> value, [MappingTargetOriginalValue] bool original) => value.IfNone(original);

    [UserMapping(Default = true)]
    public static Guid Patched(Option<Guid> value, [MappingTargetOriginalValue] Guid original) => value.IfNone(original);

    [UserMapping(Default = true)]
    public static Color Patched(Option<Color> value, [MappingTargetOriginalValue] Color original) => value.IfNone(original);

    [UserMapping(Default = true)]
    public static Point3d Patched(Option<Point3d> value, [MappingTargetOriginalValue] Point3d original) => value.IfNone(original);

    [UserMapping(Default = true)]
    public static Plane Patched(Option<Plane> value, [MappingTargetOriginalValue] Plane original) => value.IfNone(original);
}
