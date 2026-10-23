using System.Globalization;
using System.Numerics;
using PeterO.Numbers;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FieldOfView : IPresented<FieldOfView, float> {
    public static FieldOfView MinValue { get; } = new(float.BitIncrement(0f));
    public static FieldOfView MaxValue { get; } = new(float.BitDecrement(float.Pi));
    public static FieldOfView SixthTurn { get; } = new(float.Pi / 3f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SweepAngle : IPresented<SweepAngle, float> {
    public static SweepAngle MinValue { get; } = new(float.BitIncrement(0f));
    public static SweepAngle MaxValue { get; } = new(float.BitDecrement(float.Tau));
    public static SweepAngle HalfTurn { get; } = new(float.Pi);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None,
    SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SignedAngle : IPresented<SignedAngle, float> {
    public static SignedAngle MinValue { get; } = new(-float.Pi);
    public static SignedAngle MaxValue { get; } = new(float.Pi);
    public static SignedAngle Down { get; } = new(-float.Pi / 2f);
    public static SignedAngle Diagonal { get; } = new(float.Pi / 4f);
    public static SignedAngle Up { get; } = new(float.Pi / 2f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None,
    SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Elevation : IPresented<Elevation, float> {
    public static Elevation MinValue { get; } = new(-float.Pi / 2f);
    public static Elevation MaxValue { get; } = new(float.Pi / 2f);
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None,
    SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CylinderHeight : IMinMaxValue<CylinderHeight> {
    public static CylinderHeight MinValue { get; } = new(-float.MaxValue);
    public static CylinderHeight MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FullTurnRadius : IMinMaxValue<FullTurnRadius> {
    public static FullTurnRadius MinValue { get; } = new(float.BitIncrement(0f));
    public static FullTurnRadius MaxValue { get; } = new(float.MaxValue);
    public static FullTurnRadius Standard { get; } = new(2f * 10.5f / 12f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
public readonly partial struct ViewWindow {
    public double CenterX { get; }
    public double CenterY { get; }
    public double Width { get; }
    public double Height { get; }

    public static ViewWindow Centered(FieldOfView field, PixelExtent extent) => Centered(2d * double.Tan(field / 2d), extent);
    public static ViewWindow Centered(SweepAngle sweep, PixelExtent extent) => Centered((double)sweep, extent);

    public ViewWindow Raised(float height) => new(CenterX, CenterY + height, Width, Height);

    public (double X, double Y) WindowPoint(double x, double y, PixelExtent extent) =>
        HomogeneousPoint(x, y, extent) switch {
            var point => (BitConverter.Int64BitsToDouble(point.X.Divide(point.W, EContext.Binary64).ToDoubleBits()), BitConverter.Int64BitsToDouble(point.Y.Divide(point.W, EContext.Binary64).ToDoubleBits())),
        };

    internal (EFloat X, EFloat Y, EFloat W) HomogeneousPoint(double x, double y, PixelExtent extent) {
        (EFloat width, EFloat height, EFloat half) = (EFloat.FromInt32(extent.Width), EFloat.FromInt32(extent.Height), EFloat.FromDouble(0.5d));
        return (((EFloat.FromDouble(CenterX) * width) + ((EFloat.FromDouble(x) - (width * half)) * EFloat.FromDouble(Width))) * height,
            ((EFloat.FromDouble(CenterY) * height) + (((height * half) - EFloat.FromDouble(y)) * EFloat.FromDouble(Height))) * width, width * height);
    }

    public Option<(double X, double Y)> FramePoint((double X, double Y) window, PixelExtent extent) =>
        from input in double.IsFinite(window.X) && double.IsFinite(window.Y) ? Some((Point: window, CenterX, CenterY, Width, Height)) : None
        let width = EFloat.FromDouble(input.Width)
        let height = EFloat.FromDouble(input.Height)
        let x = ((EFloat.FromDouble(input.Point.X) - EFloat.FromDouble(input.CenterX) + (width * EFloat.FromDouble(0.5d))) * EFloat.FromInt32(extent.Width)).Divide(width, EContext.Binary64)
        let y = ((EFloat.FromDouble(input.CenterY) - EFloat.FromDouble(input.Point.Y) + (height * EFloat.FromDouble(0.5d))) * EFloat.FromInt32(extent.Height)).Divide(height, EContext.Binary64)
        from result in x.IsFinite && y.IsFinite ? Some((BitConverter.Int64BitsToDouble(x.ToDoubleBits()), BitConverter.Int64BitsToDouble(y.ToDoubleBits()))) : None
        select result;

    private static ViewWindow Centered(double width, PixelExtent extent) => new(0d, 0d, width, width * extent.Height / extent.Width);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double centerX, ref double centerY, ref double width, ref double height) =>
        validationError = double.IsFinite(centerX) && double.IsFinite(centerY)
            && width is > 0d and < double.PositiveInfinity && height is > 0d and < double.PositiveInfinity ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"View window at ({centerX}, {centerY}) of {width}x{height} must be finite with positive size"));
}

public sealed class Homography {
    private readonly EFloat a;
    private readonly EFloat b;
    private readonly EFloat c;
    private readonly EFloat d;
    private readonly EFloat e;
    private readonly EFloat f;
    private readonly EFloat g;
    private readonly EFloat h;
    private readonly EFloat i;

    internal Homography(EFloat a, EFloat b, EFloat c, EFloat d, EFloat e, EFloat f, EFloat g, EFloat h, EFloat i) =>
        (this.a, this.b, this.c, this.d, this.e, this.f, this.g, this.h, this.i) = (a, b, c, d, e, f, g, h, i);

    public Homography Inverse {
        get {
            (EFloat aa, EFloat ab, EFloat ac) = ((e * i) - (f * h), (c * h) - (b * i), (b * f) - (c * e));
            (EFloat ba, EFloat bb, EFloat bc) = ((f * g) - (d * i), (a * i) - (c * g), (c * d) - (a * f));
            (EFloat ca, EFloat cb, EFloat cc) = ((d * h) - (e * g), (b * g) - (a * h), (a * e) - (b * d));
            EFloat sign = EFloat.FromInt32(((a * aa) + (b * ba) + (c * ca)).Sign);
            return new(aa * sign, ab * sign, ac * sign, ba * sign, bb * sign, bc * sign, ca * sign, cb * sign, cc * sign);
        }
    }

    public Option<(double X, double Y)> Apply((double X, double Y) point) =>
        from input in double.IsFinite(point.X) && double.IsFinite(point.Y)
            ? Some((X: EFloat.FromDoubleBits(BitConverter.DoubleToInt64Bits(point.X)), Y: EFloat.FromDoubleBits(BitConverter.DoubleToInt64Bits(point.Y)))) : None
        let denominator = g.MultiplyAndAdd(input.X, h.MultiplyAndAdd(input.Y, i))
        from w in denominator.Sign > 0 ? Some(denominator) : None
        let x = a.MultiplyAndAdd(input.X, b.MultiplyAndAdd(input.Y, c)).Divide(w, EContext.Binary64)
        let y = d.MultiplyAndAdd(input.X, e.MultiplyAndAdd(input.Y, f)).Divide(w, EContext.Binary64)
        from result in x.IsFinite && y.IsFinite ? Some((BitConverter.Int64BitsToDouble(x.ToDoubleBits()), BitConverter.Int64BitsToDouble(y.ToDoubleBits()))) : None
        select result;
}

[ComplexValueObject]
[ObjectFactory<string>]
public sealed partial class FrameQuad : IConvertible<string> {
    public (double X, double Y) TopLeft { get; }
    public (double X, double Y) TopRight { get; }
    public (double X, double Y) BottomRight { get; }
    public (double X, double Y) BottomLeft { get; }

    public bool Mirrored => Turn(BottomLeft, TopLeft, TopRight) < 0;

    public FrameQuad FlippedHorizontal => new(TopRight, TopLeft, BottomLeft, BottomRight);
    public FrameQuad FlippedVertical => new(BottomLeft, BottomRight, TopRight, TopLeft);
    public FrameQuad Turned => new(BottomLeft, TopLeft, TopRight, BottomRight);

    public Homography Homography {
        get {
            (EFloat x0, EFloat y0, EFloat x1, EFloat y1) = (EFloat.FromDouble(TopLeft.X), EFloat.FromDouble(TopLeft.Y), EFloat.FromDouble(TopRight.X), EFloat.FromDouble(TopRight.Y));
            (EFloat x2, EFloat y2, EFloat x3, EFloat y3) = (EFloat.FromDouble(BottomRight.X), EFloat.FromDouble(BottomRight.Y), EFloat.FromDouble(BottomLeft.X), EFloat.FromDouble(BottomLeft.Y));
            (EFloat sx, EFloat sy, EFloat turn) = (x0 - x1 + x2 - x3, y0 - y1 + y2 - y3, ((x1 - x2) * (y3 - y2)) - ((x3 - x2) * (y1 - y2)));
            EFloat sign = EFloat.FromInt32(turn.Sign);
            (EFloat g, EFloat h, EFloat i) = (((sx * (y3 - y2)) - ((x3 - x2) * sy)) * sign, (((x1 - x2) * sy) - (sx * (y1 - y2))) * sign, turn.Abs());
            return new(((x1 - x0) * i) + (g * x1), ((x3 - x0) * i) + (h * x3), x0 * i,
                ((y1 - y0) * i) + (g * y1), ((y3 - y0) * i) + (h * y3), y0 * i, g, h, i);
        }
    }

    public string ToValue() =>
        string.Create(CultureInfo.InvariantCulture, $"{TopLeft.X},{TopLeft.Y},{TopRight.X},{TopRight.Y},{BottomRight.X},{BottomRight.Y},{BottomLeft.X},{BottomLeft.Y}");

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out FrameQuad? item) {
        item = null;
        return value is null
            ? null
            : toSeq(value.Split(',')).Traverse(static field => Invariant.Number<double>(field)).As().Case switch {
                Seq<double> and [var x0, var y0, var x1, var y1, var x2, var y2, var x3, var y3] => Validate((x0, y0), (x1, y1), (x2, y2), (x3, y3), out item),
                _ => new ValidationError($"Frame quad '{value}' must be 8 comma-separated numbers"),
            };
    }

    private static int Turn((double X, double Y) previous, (double X, double Y) corner, (double X, double Y) next) =>
        (((EFloat.FromDouble(corner.X) - EFloat.FromDouble(previous.X)) * (EFloat.FromDouble(next.Y) - EFloat.FromDouble(corner.Y)))
            - ((EFloat.FromDouble(corner.Y) - EFloat.FromDouble(previous.Y)) * (EFloat.FromDouble(next.X) - EFloat.FromDouble(corner.X)))).Sign;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref (double X, double Y) topLeft, ref (double X, double Y) topRight, ref (double X, double Y) bottomRight, ref (double X, double Y) bottomLeft) =>
        validationError = double.IsFinite(topLeft.X) && double.IsFinite(topLeft.Y) && double.IsFinite(topRight.X) && double.IsFinite(topRight.Y)
            && double.IsFinite(bottomRight.X) && double.IsFinite(bottomRight.Y) && double.IsFinite(bottomLeft.X) && double.IsFinite(bottomLeft.Y)
            && (Turn(bottomLeft, topLeft, topRight), Turn(topLeft, topRight, bottomRight), Turn(topRight, bottomRight, bottomLeft), Turn(bottomRight, bottomLeft, topLeft))
                is ( > 0, > 0, > 0, > 0) or ( < 0, < 0, < 0, < 0) ? null : new ValidationError("Frame quad corners must be finite and convex");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Frustum {
    private Frustum(ViewWindow window) => Window = window;

    public ViewWindow Window { get; }

    internal abstract Vector3 At((double X, double Y) window, float depth);
    internal abstract (double X, double Y) WindowOf(Vector3 view);
    internal abstract (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point, PixelExtent extent);

    internal Option<((double X, double Y) Point, float Depth)> Locate(Vector3 view, PixelExtent extent) =>
        -view.Z > 0f ? Window.FramePoint(WindowOf(view), extent).Map(point => (point, -view.Z)) : None;

    public sealed record Perspective(ViewWindow Window) : Frustum(Window) {
        internal override Vector3 At((double X, double Y) window, float depth) => new((float)(window.X * depth), (float)(window.Y * depth), -depth);
        internal override (double X, double Y) WindowOf(Vector3 view) => ((double)view.X / -view.Z, (double)view.Y / -view.Z);
        internal override (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point, PixelExtent extent) {
            (EFloat x, EFloat y, EFloat w) = Window.HomogeneousPoint(point.X, point.Y, extent);
            EFloat length = ((x * x) + (y * y) + (w * w)).Sqrt(EContext.Binary64.WithUnlimitedExponents());
            return (Vector3.Zero, new Vector3(BitConverter.Int32BitsToSingle(x.Divide(length, EContext.Binary32).ToSingleBits()), BitConverter.Int32BitsToSingle(y.Divide(length, EContext.Binary32).ToSingleBits()), -BitConverter.Int32BitsToSingle(w.Divide(length, EContext.Binary32).ToSingleBits())));
        }
    }

    public sealed record Parallel(ViewWindow Window) : Frustum(Window) {
        internal override Vector3 At((double X, double Y) window, float depth) => new((float)window.X, (float)window.Y, -depth);
        internal override (double X, double Y) WindowOf(Vector3 view) => (view.X, view.Y);
        internal override (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point, PixelExtent extent) => (At(Window.WindowPoint(point.X, point.Y, extent), 0f), -Vector3.UnitZ);
    }
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LensDistance : IMinMaxValue<LensDistance> {
    public static LensDistance MinValue { get; } = new(float.BitIncrement(0f));
    public static LensDistance MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
public readonly partial struct LensFocus {
    public LensDistance FocalLength { get; }
    public LensDistance FocusDistance { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref LensDistance focalLength, ref LensDistance focusDistance) =>
        validationError = (float)focusDistance > (float)focalLength ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Focus distance {focusDistance} must exceed focal length {focalLength}"));
}

public sealed record Camera {
    public Camera(Matrix4x4 viewToWorld, Frustum frustum, PixelExtent extent, Option<LensFocus> lens) =>
        (Location, Orientation, Frustum, Extent, Lens) = (viewToWorld.Translation, Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(viewToWorld)), frustum, extent, lens);

    public Vector3 Location { get; }
    public Quaternion Orientation { get; }
    public Frustum Frustum { get; }
    public PixelExtent Extent { get; }
    public Option<LensFocus> Lens { get; }

    public Matrix4x4 ViewToWorld => Matrix4x4.CreateFromQuaternion(Orientation) with { Translation = Location };
    public Matrix4x4 WorldToView => Matrix4x4.CreateTranslation(-Location) * Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(Orientation));

    public Vector3 WorldPoint((double X, double Y) point, float depth) =>
        Location + Vector3.Transform(Frustum.At(Frustum.Window.WindowPoint(point.X, point.Y, Extent), depth), Orientation);

    public (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point) =>
        Frustum.Ray(point, Extent) switch {
            var (origin, direction) => (Location + Vector3.Transform(origin, Orientation), Vector3.Transform(direction, Orientation)),
        };

    public Option<((double X, double Y) Point, float Depth)> Pixel(Vector3 world) =>
        Frustum.Locate(Vector3.Transform(world - Location, Quaternion.Conjugate(Orientation)), Extent);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WrapMode : ICaptioned {
    public static readonly WrapMode Black = new("black", "Clip", static (point, size, offset) => (point + offset) switch {
        var center => (double.Clamp(center, 0.5d, size - 0.5d), double.Clamp(center + 0.5d, 0d, 1d) * double.Clamp(size + 0.5d - center, 0d, 1d)),
    });
    public static readonly WrapMode Clamp = new("clamp", "Extend", static (point, size, offset) => (double.Clamp(point + offset, 0.5d, size - 0.5d), 1d));
    public static readonly WrapMode Periodic = new("periodic", "Repeat", static (point, size, offset) => (((point % size) + (offset % size)) % size) switch {
        var turn => (turn < 0d ? turn + size : turn, 1d),
    });
    public static readonly WrapMode Mirror = new("mirror", "Mirror", static (point, size, offset) => (2d * size) switch {
        var period => double.Abs(((point % period) + (offset % period)) % period) switch { var turn => (double.Min(turn, period - turn), 1d) },
    });

    public string Caption { get; }

    public bool Tiles => Map(black: false, clamp: false, periodic: true, mirror: true);

    [UseDelegateFromConstructor]
    public partial (double Point, double Weight) Fold(double point, int size, double offset);

    public (ERational Point, ERational Weight) Fold(ERational point, int size) {
        (ERational half, ERational end) = (ERational.Create(1, 2), ERational.FromInt32(size));
        ERational bounded = ERational.Max(half, ERational.Min(point, end - half));
        return Switch(
            black: () => (bounded, ERational.Min(ERational.One, ERational.Max(ERational.Zero, point + half))
                * ERational.Min(ERational.One, ERational.Max(ERational.Zero, end + half - point))),
            clamp: () => (bounded, ERational.One),
            periodic: () => point.Remainder(size) switch { var turn => (turn.Sign < 0 ? turn.Add(size) : turn, ERational.One) },
            mirror: () => end.Multiply(2) switch {
                var period => point.Remainder(period).Abs() switch { var turn => (ERational.Min(turn, period - turn), ERational.One) },
            });
    }

    internal Option<int> Index(double index, int size) =>
        Fold(index, size, 0.5d) switch {
            (var point, > 0d) => Some((int)point),
            _ => None,
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Projection {
    private Projection(PixelExtent extent) => Extent = extent;

    public PixelExtent Extent { get; }
    public virtual bool Upright => false;
    public virtual (WrapMode Across, WrapMode Down) Wrap => (WrapMode.Black, WrapMode.Black);

    public Option<Vector3> Ray((double X, double Y) point) => double.IsFinite(point.X) && double.IsFinite(point.Y) ? Direction(point) : None;
    public Option<(double X, double Y)> Pixel(Vector3 ray) =>
        float.IsFinite(ray.X) && float.IsFinite(ray.Y) && float.IsFinite(ray.Z) && ray != Vector3.Zero ? FramePoint(ray) : None;

    private protected abstract Option<Vector3> Direction((double X, double Y) point);
    private protected abstract Option<(double X, double Y)> FramePoint(Vector3 ray);

    public Option<(double X, double Y)> Reprojected(Projection target, Quaternion turn, (double X, double Y) point) =>
        Ray(point).Bind(ray => target.Pixel(Vector3.Transform(ray, turn)));

    private double Circle => (double)Extent.ShortSide / Extent.Width;

    private (double X, double Y) Centered((double X, double Y) point) => ((point.X / Extent.Width) - 0.5d, ((Extent.Height / 2d) - point.Y) / Extent.Width);
    private (double X, double Y) Uncentered((double X, double Y) centered) => ((centered.X + 0.5d) * Extent.Width, (Extent.Height / 2d) - (centered.Y * Extent.Width));

    private Option<Vector3> Radial((double X, double Y) point, double cut, double scale, Func<double, double, double> angle) =>
        Centered(point) switch {
            var centered => angle(double.Hypot(centered.X, centered.Y), scale) switch {
                var theta when theta <= cut => (double.SinCos(double.Atan2(centered.Y, centered.X)), double.SinCos(theta)) switch {
                    var (azimuth, inclination) => Some(new Vector3((float)(azimuth.Cos * inclination.Sin), (float)(azimuth.Sin * inclination.Sin), (float)-inclination.Cos)),
                },
                _ => None,
            },
        };
    private Option<(double X, double Y)> RadialPoint(Vector3 ray, double cut, double scale, Func<double, double, double> radius) =>
        double.Atan2(double.Hypot(ray.X, ray.Y), -ray.Z) switch {
            var theta when theta <= cut => (double.SinCos(double.Atan2(ray.Y, ray.X)), radius(theta, scale)) switch {
                var (azimuth, reach) => Some(Uncentered((azimuth.Cos * reach, azimuth.Sin * reach))),
            },
            _ => None,
        };

    private static double SolidAngle(double radius, double reach) => 2d * double.Asin(radius / reach);
    private static double SolidRadius(double angle, double reach) => reach * double.Sin(angle / 2d);

    public sealed record Rectilinear(PixelExtent Extent, Frustum.Perspective Frustum) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Some(Frustum.Ray(point, Extent).Direction);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => Frustum.Locate(ray, Extent).Map(static located => located.Point);
    }

    public sealed record Equirectangular(PixelExtent Extent) : Projection(Extent) {
        public override bool Upright => true;
        public override (WrapMode Across, WrapMode Down) Wrap => (WrapMode.Periodic, WrapMode.Clamp);

        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            (double.SinCos(((point.X % Extent.Width / Extent.Width) - 0.5d) * double.Tau), double.SinCos((0.5d - (point.Y % (2d * Extent.Height) / Extent.Height)) * double.Pi)) switch {
                var (longitude, latitude) => Some(new Vector3((float)(longitude.Sin * latitude.Cos), (float)latitude.Sin, (float)(-longitude.Cos * latitude.Cos))),
            };

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            Some((
                ((double.Atan2(ray.X, -ray.Z) / double.Tau) + 0.5d) * Extent.Width,
                (0.5d - double.Atan2Pi(ray.Y, double.Hypot(ray.X, ray.Z))) * Extent.Height));
    }

    public sealed record Cylindrical(PixelExtent Extent, ViewWindow Window) : Projection(Extent) {
        public override bool Upright => true;

        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            from window in Some(Window.WindowPoint(point.X, point.Y, Extent))
            from longitude in double.IsFinite(window.X) ? Some(window.X) : None
            select (double.SinCos(longitude), double.SinCos(double.Atan(window.Y))) switch {
                var (across, latitude) => new Vector3((float)(across.Sin * latitude.Cos), (float)latitude.Sin, (float)(-across.Cos * latitude.Cos)),
            };

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            double.Hypot(ray.X, ray.Z) switch {
                > 0d and var radius => Window.FramePoint((double.Atan2(ray.X, -ray.Z), ray.Y / radius), Extent),
                _ => None,
            };
    }

    public sealed record Fisheye(PixelExtent Extent, SweepAngle Sweep) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Radial(point, Sweep / 2d, Sweep / Circle, static (radius, scale) => radius * scale);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => RadialPoint(ray, Sweep / 2d, Sweep / Circle, static (angle, scale) => angle / scale);
    }

    public sealed record Equisolid(PixelExtent Extent, SweepAngle Sweep, FullTurnRadius FullTurn) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Radial(point, Sweep / 2d, Circle * FullTurn / 2d, SolidAngle);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => RadialPoint(ray, Sweep / 2d, Circle * FullTurn / 2d, SolidRadius);
    }

    public sealed record MirrorBall(PixelExtent Extent) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) => Radial(point, double.Pi, Circle / 2d, SolidAngle);
        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) => RadialPoint(ray, double.Pi, Circle / 2d, SolidRadius);
    }

    public sealed record Stereographic(PixelExtent Extent, SweepAngle Sweep) : Projection(Extent) {
        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            from centered in Some(Centered(point))
            let inverse = 1d / (2d * double.Tan(Sweep / 4d))
            let magnitude = double.Max(inverse, double.Max(double.Abs(centered.X), double.Abs(centered.Y)))
            let local = (X: centered.X / magnitude, Y: centered.Y / magnitude, Z: inverse / magnitude)
            let planar = (local.X * local.X) + (local.Y * local.Y)
            let axial = local.Z * local.Z
            let denominator = planar + axial
            select new Vector3((float)(2d * local.X * local.Z / denominator), (float)(2d * local.Y * local.Z / denominator), (float)((planar - axial) / denominator));

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            from planar in Some(double.Hypot(ray.X, ray.Y))
            where planar > 0d || ray.Z < 0f
            let length = double.Hypot(planar, ray.Z)
            let radius = ray.Z > 0f ? (length + ray.Z) / planar : planar / (length - ray.Z)
            let reach = radius / (2d * double.Tan(Sweep / 4d))
            let azimuth = double.SinCos(double.Atan2(ray.Y, ray.X))
            select Uncentered((azimuth.Cos * reach, azimuth.Sin * reach));
    }

    public sealed record Panini(PixelExtent Extent, FieldOfView Field, Magnitude Distance, AxisFraction Compression) : Projection(Extent) {
        public override bool Upright => true;

        private protected override Option<Vector3> Direction((double X, double Y) point) =>
            from centered in Some(Centered(point))
            let distance = (double)Distance
            let factor = 2d * double.Sin(Field / 2d) / (distance + double.Cos(Field / 2d))
            from cos in distance is 0d
                ? Some(1d / factor / double.Hypot(centered.X, 1d / factor))
                : (from u in Some(double.Abs(centered.X * factor))
                   let delta = double.FusedMultiplyAdd(-distance, u, 1d)
                   where delta > 0d
                   let t = distance * u
                   let remainder = delta * (1d + t)
                   select remainder / (double.Sqrt(remainder + (u * u)) + (u * t)))
            let vertical = centered.Y * (cos / (((1d - Compression) * cos) + Compression))
            let depth = distance is 0d ? -1d / factor : -cos / (factor * (distance + cos))
            let scale = double.Max(double.Abs(centered.X), double.Max(double.Abs(vertical), double.Abs(depth)))
            select Vector3.Normalize(new Vector3((float)(centered.X / scale), (float)(vertical / scale), (float)(depth / scale)));

        private protected override Option<(double X, double Y)> FramePoint(Vector3 ray) =>
            double.Hypot(ray.X, ray.Z) switch {
                var radius when -ray.Z > 0f => (double.SinCos(Field / 2d), -ray.Z / radius) switch {
                    var (edge, cos) => ((Distance + edge.Cos) / (Distance + cos) / (2d * edge.Sin)) switch {
                        var gain => Some(Uncentered((ray.X / radius * gain, ray.Y / radius * (1d - Compression + (Compression / cos)) * gain))),
                    },
                },
                _ => None,
            };
    }
}

public readonly record struct PanoramaView(SignedAngle Heading, Elevation Elevation, SignedAngle Roll) : IStateRecord<PanoramaView, PanoramaViewParameter> {
    public static PanoramaView Default { get; }

    public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(-(float)Heading, Elevation, Roll);

    public static Option<(float Heading, float Elevation, float Roll)> Facing(Vector3 left, Vector3 right) =>
        from input in float.IsFinite(left.X) && float.IsFinite(left.Y) && float.IsFinite(left.Z)
            && float.IsFinite(right.X) && float.IsFinite(right.Y) && float.IsFinite(right.Z) ? Some((Left: left, Right: right)) : None
        let normal = (X: ((double)input.Left.Y * input.Right.Z) - ((double)input.Left.Z * input.Right.Y),
            Y: ((double)input.Left.Z * input.Right.X) - ((double)input.Left.X * input.Right.Z), Z: ((double)input.Left.X * input.Right.Y) - ((double)input.Left.Y * input.Right.X))
        where normal != (0d, 0d, 0d)
        let leftLength = double.Hypot(double.Hypot(input.Left.X, input.Left.Y), input.Left.Z)
        let rightLength = double.Hypot(double.Hypot(input.Right.X, input.Right.Y), input.Right.Z)
        let forward = (X: (input.Left.X / leftLength) + (input.Right.X / rightLength), Y: (input.Left.Y / leftLength) + (input.Right.Y / rightLength), Z: (input.Left.Z / leftLength) + (input.Right.Z / rightLength))
        let across = (X: (normal.Y * forward.Z) - (normal.Z * forward.Y), Y: (normal.Z * forward.X) - (normal.X * forward.Z), Z: (normal.X * forward.Y) - (normal.Y * forward.X))
        let heading = double.Atan2(forward.X, -forward.Z)
        let elevation = double.Atan2(forward.Y, double.Hypot(forward.X, forward.Z))
        let yaw = double.SinCos(heading)
        let pitch = double.SinCos(elevation)
        select ((float)heading, (float)elevation, (float)double.Atan2((-across.X * yaw.Sin * pitch.Sin) + (across.Y * pitch.Cos) + (across.Z * yaw.Cos * pitch.Sin), (across.X * yaw.Cos) + (across.Z * yaw.Sin)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanoramaViewParameter : IStateParameter<PanoramaView> {
    public static readonly PanoramaViewParameter Heading = new("heading", StateParameter.Bounded(Lens<PanoramaView, SignedAngle>.New(static view => view.Heading, static heading => view => view with { Heading = heading })),
        new("Heading", "Rightward turn of view"));
    public static readonly PanoramaViewParameter Elevation = new("elevation", StateParameter.Bounded(Lens<PanoramaView, Elevation>.New(static view => view.Elevation, static elevation => view => view with { Elevation = elevation })),
        new("Elevation", "View upward tilt"));
    public static readonly PanoramaViewParameter Roll = new("roll", StateParameter.Bounded(Lens<PanoramaView, SignedAngle>.New(static view => view.Roll, static roll => view => view with { Roll = roll })),
        new("Roll", "Counterclockwise turn about view axis"));

    public StateParameter<PanoramaView> Kind { get; }
    public ParameterText Text { get; }
}
